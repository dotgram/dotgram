using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace DotGram.Generation;

/// <summary>
/// Roslyn shell over <see cref="GramCompiler"/>.
/// </summary>
/// <remarks>
/// Everything here is Roslyn-specific: find the classes that host a grammar, decide
/// accessibility from what the compilation references, convert diagnostics on the way
/// back. Compilation itself lives in <see cref="DotGram.Grammar"/> and is callable —
/// and testable — without any of this.
/// </remarks>
[Generator(LanguageNames.CSharp)]
public sealed class GramGenerator : IIncrementalGenerator
{
	const string GramFileExtension = ".gram";
	const string GramAttribute        = "DotGram.GramAttribute";
	const string GramOptionsAttribute = "DotGram.GramOptionsAttribute";

	/// <summary>
	/// The stages, named so that what re-ran can be read back.
	/// </summary>
	/// <remarks>
	/// Public because the names are how the incremental behaviour is checked at all —
	/// Roslyn reports a step under the name it was given and under no other — and a
	/// property nothing can observe is a property nothing keeps.
	/// </remarks>
	public const string AskedStage    = "Asked";
	public const string AnsweredStage = "Answered";
	public const string CompiledStage = "Compiled";
	public const string PlacedStage   = "Placed";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// `[Gram]` itself: always internal, always present, so that the attribute can be
		// written in source at all and nothing has to be found anywhere.
		context.RegisterPostInitializationOutput(static postInit =>
		{
			var source = GramCompiler.EmitMarkerAttributes();

			// What `[Embedded]` on each of them names, shared with every other generator that
			// asks: the attributes are the assembly's own and are not seen from outside it.
			postInit.AddEmbeddedAttributeDefinition();
			postInit.AddSource(source.HintName, source.Text);
		});

		// The unit of generation is the host class, not the file (§1). A .gram file no
		// class claims generates nothing — there would be nowhere to put the result.
		var found = context.SyntaxProvider.ForAttributeWithMetadataName(
			GramAttribute,
			static (node, _) => node is ClassDeclarationSyntax,
			static (candidate, _) => Host.All(candidate))
			.SelectMany(static (all, _) => all);

		// What a host says, and where it says it, go separate ways (D145). The compile reads the
		// first alone, so an edit that moves an attribute and leaves what it says as it was stops
		// here: the host compares equal and nothing below it runs. Where it is — the attribute,
		// the literal's line and column — is gathered for the two steps at the end that need it.
		var hosts = found.Select(static (one, _) => one.Host);
		var sites = found
			.Select(static (one, _) => one.Site)
			.Collect()
			.Select(static (all, _) => Sites(all));

		var files = context.AdditionalTextsProvider
			.Where(static file => file.Path.EndsWith(GramFileExtension, StringComparison.OrdinalIgnoreCase))
			.Select(static (file, cancellationToken) => new GrammarFile(
				Path: file.Path,
				Text: file.GetText(cancellationToken)?.ToString() ?? ""))
			.Collect();

		// Three stages, and the shape of them is what makes this incremental.
		//
		// A `Compilation` is a different object after every keystroke, so anything
		// downstream of one is recomputed for every character typed. Binding genuinely
		// needs it for declared C# types, their constructors and their properties, so the
		// dependency cannot be removed, only narrowed to what it is for. C# methods are not
		// among those questions: syntax fixes their call shape and the C# compiler binds it.
		//
		//   grammar  ──► the questions its C# names raise      cached on the grammar
		//   + host                     │
		//                              ▼
		//   Compilation ──► the answers, as a list of values   re-runs, and is cheap
		//                              │
		//                              ▼
		//   grammar + host + answers ──► the parser            cached on all three
		//                              │
		//                              ▼
		//   parser + site ──► the parser's #line numbers        a pass over the text
		//
		// So editing a C# file re-runs the middle stage — a handful of symbol lookups —
		// and stops there, because the answers it produces are the same ones. Editing the
		// host's own file above its attribute re-runs the last, and only it.
		// The names are what a test reads to say which stage re-ran, and are the only way
		// to tell "the answers were the same" from "nothing was asked".
		var asked = hosts
			.Combine(files)
			.Select(static (input, _) => AskedSafely(input.Left, input.Right))
			.WithTrackingName(AskedStage);

		var answered = asked
			.Combine(context.CompilationProvider)
			.Select(static (input, _) => AnswerSafely(input.Left, input.Right))
			.WithTrackingName(AnsweredStage);

		var reporting = context.AnalyzerConfigOptionsProvider.Select(static (options, _) =>
			options.GlobalOptions.TryGetValue("build_property.DesignTimeBuild", out var designTime) &&
			string.Equals(designTime, "true", StringComparison.OrdinalIgnoreCase)
				? Reporting.None
				: options.GlobalOptions.TryGetValue("build_property.DotGramReportGeneration", out var asked)
					? Asked(asked)
					: Reporting.None);

		// A compilation that defines DOTGRAM_COUNTS — one of the repository's own count tests, never a
		// shipped package — gets a counter of entries in every rule method as well (D144). Asked of the
		// parse options, which change when the symbols do and not on an edit.
		// And one that defines DOTGRAM_NO_MEMO is compiled without the memo of failures, so that a
		// test can hold the parser with it to the parser without it (GramCompilerOptions.MemoiseFailures).
		// And `DotGramTrace`: every grammar of the project compiled as a trace build, as if each of
		// its readings said `Trace = true` (GramCompilerOptions.Trace). Off unless the property says
		// `true`.
		var tracing = context.AnalyzerConfigOptionsProvider.Select(static (options, _) =>
			IsTrue(options, "build_property.DotGramTrace"));

		// And one whose effective C# is 11 or later may hold UTF-8 string literals, which a lexer's
		// table is written as there (GramCompilerOptions.Utf8Literals): the version the consumer's
		// <LangVersion> resolves to, never one inferred from the target framework.
		//
		// And one that defines DOTGRAM_CHECKS gets the walk's invariants written into it, behind the
		// symbol as they always were; a parser built without it carries no text of them at all
		// (GramCompilerOptions.Checks).
		//
		// And one that defines DOTGRAM_NO_COLLAPSE calls every rule that only forwards another's value
		// as written, so that a test can hold a trace build's frames of those rules to the rules
		// themselves (GramCompilerOptions.CollapseForwarders).
		var counting = context.ParseOptionsProvider.Combine(tracing).Select(static (input, _) =>
			(Counts: input.Left.PreprocessorSymbolNames.Contains(CountsSymbol),
			Memoises: !input.Left.PreprocessorSymbolNames.Contains(NoMemoSymbol),
			Traces: input.Right,
			Collapses: !input.Left.PreprocessorSymbolNames.Contains(NoCollapseSymbol),
			Utf8: input.Left is CSharpParseOptions { LanguageVersion: >= LanguageVersion.CSharp11 },
			Checks: input.Left.PreprocessorSymbolNames.Contains(ChecksSymbol)));

		// `DotGramPositionalFollow`: an experimental build-wide switch that compiles every `parse`
		// knowing it is also read from a position (GramCompilerOptions.PositionalFollow). Off
		// unless the property says `true`.
		// `split` is the same switch with the positional end told apart where that changes no answer
		// (GramCompilerOptions.PositionalFollowSplit).
		var positional = context.AnalyzerConfigOptionsProvider.Select(static (options, _) =>
			options.GlobalOptions.TryGetValue("build_property.DotGramPositionalFollow", out var value)
				? string.Equals(value.Trim(), "true", StringComparison.OrdinalIgnoreCase) ? Positional.Follow :
				string.Equals(value.Trim(), "split", StringComparison.OrdinalIgnoreCase) ? Positional.Split :
				Positional.Off
				: Positional.Off);

		// `DotGramNoCache`: compile every grammar afresh rather than take a parser kept from an
		// earlier compilation in this process (Compiled). `DotGramVerifyCache` is the repository's
		// own: take it, compile afresh as well, and fail the build where the two differ.
		// A design-time build — the editor's — keeps nothing either: its driver keeps the last parser
		// already, its process does not end, and a grammar being typed would fill the cache with a
		// parser for every state of it.
		var caching = context.AnalyzerConfigOptionsProvider.Select(static (options, _) =>
			IsTrue(options, "build_property.DotGramNoCache") ? Caching.Off :
			IsTrue(options, "build_property.DesignTimeBuild") ? Caching.Off :
			IsTrue(options, "build_property.DotGramVerifyCache") ? Caching.Verify :
			Caching.On);

		var compiled = answered.Combine(reporting).Combine(counting).Combine(positional).Combine(caching)
			.Select(static (input, _) => CompileCached(
				input.Left.Left.Left.Left, input.Left.Left.Left.Right, input.Left.Left.Right, input.Left.Right, input.Right))
			.WithTrackingName(CompiledStage);

		// Each parser beside where its host is written. The lookup runs for every parser whenever
		// any host moves, and is one dictionary read; what it hands on compares equal for every
		// host that did not move, so only those that did are placed again.
		var located = compiled
			.Combine(sites)
			.Select(static (input, _) => new Located(
				input.Left,
				input.Right.TryGetValue(input.Left.Key, out var site) ? site : default));

		// The generated file: the compiled text with the literal's place written into each
		// `#line` — a pass over the text, where a host that moved has any (Parser.PlacedAt). A
		// grammar from a `.gram` file has none, and its text is handed on as it is.
		var placed = located
			.Select(static (input, _) => input.Parser.PlacedAt(input.Site))
			.WithTrackingName(PlacedStage);

		context.RegisterSourceOutput(placed, static (production, parser) => parser.Deliver(production));

		// The diagnostics need a tree to point into, and the trees are a projection of the
		// compilation, which changes on every keystroke. So they are combined HERE and nowhere
		// earlier: the compile keeps its cached value and only the reporting runs again. The map is
		// built once per delivery and only where a diagnostic actually asks for a tree.
		context.RegisterSourceOutput(
			located.Combine(context.CompilationProvider.Select(static (compilation, _) => compilation.SyntaxTrees.ToImmutableArray())),
			static (production, input) =>
			{
				Dictionary<string, SyntaxTree>? found = null;

				input.Left.Parser.Report(
					production,
					input.Left.Site,
					path =>
					{
						found ??= input.Right
							.GroupBy(static tree => tree.FilePath, StringComparer.Ordinal)
							.ToDictionary(static one => one.Key, static one => one.First(), StringComparer.Ordinal);

						return found.TryGetValue(path, out var tree) ? tree : null;
					});
			});

	}

	// ── Parsers ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// Grammar text is untrusted input. An unexpected non-fatal exception is a compiler
	/// defect to report, never a reason for Roslyn to disable the generator with CS8785.
	/// </summary>
	static Grammar AskedSafely(Host host, ImmutableArray<GrammarFile> files)
	{
		try
		{
			return Asked(host, files);
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			return Failed(
				new Grammar(host, null, null, default, default, default),
				"reading and analyzing the grammar",
				exception);
		}
	}

	static Grammar AnswerSafely(Grammar grammar, Compilation compilation)
	{
		try
		{
			return grammar with
			{
				Answers = new EquatableArray<Answer>(Questions.Ask(
					grammar.Questions.Items,
					new RoslynSymbolResolver(
						compilation,
						grammar.Host.MetadataName,
						[.. grammar.Host.Includes.Items.Select(static one => one.ClassName)]))),
			};
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			return Failed(grammar, "answering C# type questions", exception);
		}
	}

	/// <summary>
	/// What `DotGramReportGeneration` asked for, with the two older spellings it had.
	/// </summary>
	/// <remarks>
	/// Anything else is <see cref="Reporting.None"/>, which is also what an unset property means.
	/// A generator has no good way to complain about a build property — it would have to raise a
	/// diagnostic in every compilation that set it, which is the noise this level exists to
	/// remove — so an unrecognized word is silence rather than a message.
	/// </remarks>
	static Reporting Asked(string asked)
	{
		return asked.Trim().ToLowerInvariant() switch
		{
			"full" or "true" => Reporting.Full,
			"summary" => Reporting.Summary,
			_ => Reporting.None,
		};
	}

	/// <summary>The symbol under which emitted code carries the counters of the repository's tests.</summary>
	const string CountsSymbol = "DOTGRAM_COUNTS";

	/// <summary>The symbol under which the walk asserts its invariants as it goes.</summary>
	const string ChecksSymbol = "DOTGRAM_CHECKS";

	/// <summary>The symbol under which a test compiles a parser without the memo of failures.</summary>
	const string NoMemoSymbol = "DOTGRAM_NO_MEMO";

	/// <summary>The symbol under which a test compiles a parser that calls its forwarding rules as written.</summary>
	const string NoCollapseSymbol = "DOTGRAM_NO_COLLAPSE";

	/// <summary>What <c>DotGramPositionalFollow</c> asks for: nothing, <c>true</c> or <c>split</c>.</summary>
	enum Positional
	{
		Off,
		Follow,
		Split,
	}

	static Parser CompileSafely(Grammar grammar, Reporting reporting, (bool Counts, bool Memoises, bool Traces, bool Collapses, bool Utf8, bool Checks) counting, Positional positional)
	{
		try
		{
			return Compile(grammar, reporting, counting, positional);
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			var failed = Failed(grammar, "compiling the recognition graph", exception);

			return new Parser(grammar.Host.Key, null, null, failed.Reports);
		}
	}

	/// <summary>Whether a build asked for the parsers kept from earlier compilations, and how.</summary>
	enum Caching
	{
		On,
		Off,
		Verify,
	}

	/// <summary>Everything the compile reads, as values: what a kept parser is found by.</summary>
	/// <remarks>
	/// Exactly the arguments of <see cref="Compile"/>: the grammar — the host, its joined text and
	/// paths, the questions and the answers the compilation gave to them — and the three things the
	/// build says about how to compile it. The same equality the incremental pipeline already
	/// trusts to skip the compile within one driver, so keeping a parser across drivers adds no
	/// second notion of "the same input". <paramref name="Generator"/> is which build of the
	/// generator compiled it: implied already, since every loaded assembly of the generator has a
	/// cache of its own, and written down so that nothing has to know that.
	/// </remarks>
	readonly record struct CompileKey(
		Grammar                                                                          Grammar,
		Reporting                                                                        Reporting,
		(bool Counts, bool Memoises, bool Traces, bool Collapses, bool Utf8, bool Checks) Counting,
		Positional                                                                       Positional,
		Guid                                                                             Generator);

	/// <summary>
	/// The parsers compiled in this process, kept for the next compilation that asks for one of them.
	/// </summary>
	/// <remarks>
	/// A command-line build after an edit to C# alone compiles every grammar of the project again
	/// — the answers come out the same, but no driver is kept between two builds to know it — and
	/// that compile is most of the generator's time: seconds for a grammar of a thousand rules. The
	/// compiler server keeps this assembly loaded, so the parser kept here is found again.
	/// <para>
	/// The budget is forty million characters, 80 MB as .NET holds them: what the three SQL parsers
	/// of DotGram.Sql hold for both of its target frameworks, the largest this repository has, with
	/// room for them to grow. Counted, those are 29 million, most of it the parsers' text — T-SQL's
	/// alone is sixteen million — which the two frameworks share; their questions and answers are
	/// ten thousand between the three grammars. A smaller budget keeps them apart, and each build
	/// compiles a grammar again for one framework or the other. No larger, though: a long-lived server is
	/// shared by every project and every checkout on the machine, and each build of the generator it
	/// loads has a cache of its own. Two target frameworks of one project mostly compile the same
	/// text, and hold it once (Shared). The large strings are counted exactly and the rest — the
	/// questions and answers, the diagnostics — by an estimate (WeightOf), so that a grammar whose
	/// C# types have thousands of members weighs what it holds; and no more than 64 entries are
	/// kept, whatever they weigh. A generator built again in the same place does not add a second
	/// cache beside the first: when that was measured, each build after such a change ran in a new
	/// compiler server process.
	/// </para>
	/// </remarks>
	static readonly CompileCache<CompileKey, Parser> Compiled = new(
		budget:   40_000_000,
		capacity: 64,
		strings:  static (key, parser) => StringsOf(key, parser),
		weight:   static (key, parser) => WeightOf(key, parser),
		share:    static (parser, kept) => Shared(parser, kept));

	static readonly Guid GeneratorBuild = typeof(GramGenerator).Assembly.ManifestModule.ModuleVersionId;

	/// <summary>
	/// <see cref="CompileSafely"/>, or the parser an earlier compilation in this process made of the
	/// same input.
	/// </summary>
	static Parser CompileCached(Grammar grammar, Reporting reporting, (bool Counts, bool Memoises, bool Traces, bool Collapses, bool Utf8, bool Checks) counting, Positional positional, Caching caching)
	{
		// No text is a grammar that never got as far as the compile, which only hands its reports on.
		if (caching == Caching.Off || grammar.Text is null)
			return CompileSafely(grammar, reporting, counting, positional);

		var key = new CompileKey(grammar, reporting, counting, positional, GeneratorBuild);

		if (Compiled.TryGet(key, out var kept))
			return caching == Caching.Verify ? Verified(key, kept) : kept;

		var parser = CompileSafely(grammar, reporting, counting, positional);

		// A compile that failed inside is a defect, and is answered again next time rather than
		// remembered: it may have been the moment and not the input.
		if (!parser.Reports.Items.Any(static report => report.Id == Diagnostics.InternalFailure.Id))
			Compiled.Add(key, Recalled(parser));

		return parser;
	}

	/// <summary>
	/// A kept parser held to a fresh compile of its own input: where the two differ, something the
	/// compile reads is missing from <see cref="CompileKey"/>, and the build fails saying so.
	/// </summary>
	static Parser Verified(CompileKey key, Parser kept)
	{
		var fresh = CompileSafely(key.Grammar, key.Reporting, key.Counting, key.Positional);

		if (Recalled(fresh) == kept)
			return kept with { Summary = Recalled(kept.Summary, "cached and verified") };

		Compiled.Remove(key);

		var reports = ImmutableArray.CreateBuilder<Report>();

		reports.AddRange(fresh.Reports.Items);
		reports.Add(Report.Of(
			Diagnostics.InternalFailure,
			Site.Attribute,
			"verifying the parser kept from an earlier compilation",
			"DotGram.Generation.CompileCache",
			"it differs from a fresh compile of the same input: " + Difference(kept, Recalled(fresh))));

		return fresh with { Reports = Values(reports) };
	}

	/// <summary>Which part of two parsers differs, for the message that says they do.</summary>
	static string Difference(Parser kept, Parser fresh)
	{
		return
			kept.Text != fresh.Text       ? "the text" :
			kept.Parts != fresh.Parts     ? "the parts" :
			kept.Reports != fresh.Reports ? "the diagnostics" :
			kept.Detail != fresh.Detail   ? "the detail of the report" :
			"the summary or the names";
	}

	/// <summary>The parser as it is kept: the report's line says it was not compiled this time.</summary>
	static Parser Recalled(Parser parser)
	{
		return parser with { Summary = Recalled(parser.Summary, "cached") };
	}

	/// <summary>
	/// The report's line with the time the compile took replaced by <paramref name="instead"/>: that
	/// time is the line's last item, and a parser taken from the cache took none of it.
	/// </summary>
	static string? Recalled(string? summary, string instead)
	{
		var timing = summary?.LastIndexOf(", ", StringComparison.Ordinal) ?? -1;

		return timing < 0 ? summary : summary!.Substring(0, timing + 2) + instead;
	}

	/// <summary>
	/// The parser with its text the very string a kept parser already holds, where one holds the same:
	/// two target frameworks whose answers differ in nothing the parser spells compile the same text,
	/// and the second keeps no copy of it.
	/// </summary>
	static Parser Shared(Parser parser, IEnumerable<Parser> kept)
	{
		if (parser.Text is not { } text)
			return parser;

		foreach (var other in kept)
			if (other.Text is { } same && same.Length == text.Length && !ReferenceEquals(same, text) && string.Equals(same, text, StringComparison.Ordinal))
				return parser with { Text = same };

		return parser;
	}

	/// <summary>
	/// The large strings of an entry, which the budget counts exactly: the grammar, what it compiled
	/// into, and what the diagnostics quote of it.
	/// </summary>
	static IEnumerable<string?> StringsOf(CompileKey key, Parser parser)
	{
		yield return key.Grammar.Text;
		yield return key.Grammar.Host.Literal;
		yield return parser.Text;
		yield return parser.Summary;
		yield return parser.Detail;

		foreach (var part in parser.Parts.Items)
			yield return part.Text;

		foreach (var report in key.Grammar.Reports.Items.Concat(parser.Reports.Items))
		{
			yield return report.Grammar;
			yield return report.Written;
		}
	}

	/// <summary>
	/// Everything else an entry holds, in characters, as an estimate: the questions, the answers with
	/// the members and parameters they list, the diagnostics and the pieces.
	/// </summary>
	/// <remarks>
	/// The questions and answers are counted by the room their elements take in their arrays and not
	/// by the names in them, which a grammar's thousands of questions share among a few hundred
	/// types: the SQL:2023 grammar asks 8,172. The members and parameters an answer lists, and the
	/// diagnostics, are small objects with strings of their own, counted as such. Not exact, and not
	/// meant to be: it is what makes a key weigh what it holds.
	/// </remarks>
	static long WeightOf(CompileKey key, Parser parser)
	{
		var grammar = key.Grammar;
		var weight  = (long)ObjectWeight * (4 + grammar.Pieces.Items.Length + parser.Parts.Items.Length)
			+ (long)QuestionWeight * grammar.Questions.Items.Length
			+ (long)AnswerWeight   * grammar.Answers.Items.Length;

		foreach (var answer in grammar.Answers.Items)
		{
			foreach (var constructor in answer.Constructors.Items)
			{
				weight += ObjectWeight;

				foreach (var parameter in constructor.Items)
					weight += ObjectWeight + parameter.Name.Length + parameter.Type.Length;
			}

			if (answer.Properties.Items.Length > 0)
				weight += ObjectWeight;

			// The names a Fitting answer lists are the questions' own strings: only the array is its own.
			if (answer.Fits.Items.Length > 0)
				weight += ObjectWeight + (long)ReferenceWeight * answer.Fits.Items.Length;

			foreach (var member in answer.Properties.Items)
				weight += ObjectWeight + member.Name.Length + member.Type.Length;
		}

		foreach (var report in grammar.Reports.Items.Concat(parser.Reports.Items))
		{
			weight += 4 * ObjectWeight + report.Id.Length + report.Title.Length + report.MessageFormat.Length + (report.FilePath?.Length ?? 0);

			foreach (var argument in report.Arguments.Items)
				weight += ObjectWeight + argument.Length;
		}

		return weight;
	}

	/// <summary>A small object's header and fields, in characters: about 24 bytes.</summary>
	const int ObjectWeight = 12;

	/// <summary>A <see cref="Question"/> in its array, in characters: 24 bytes.</summary>
	const int QuestionWeight = 12;

	/// <summary>An <see cref="Answer"/> in its array, in characters: 64 bytes.</summary>
	const int AnswerWeight = 32;

	/// <summary>A reference in an array, in characters: 8 bytes.</summary>
	const int ReferenceWeight = 4;

	static bool IsTrue(AnalyzerConfigOptionsProvider options, string property)
	{
		return options.GlobalOptions.TryGetValue(property, out var value) &&
			string.Equals(value.Trim(), "true", StringComparison.OrdinalIgnoreCase);
	}

	static bool Recoverable(Exception exception)
	{
		return exception is not OperationCanceledException and not OutOfMemoryException;
	}

	static Grammar Failed(Grammar grammar, string stage, Exception exception)
	{
		var reports = ImmutableArray.CreateBuilder<Report>();

		reports.AddRange(grammar.Reports.Items);
		reports.Add(Report.Of(
			Diagnostics.InternalFailure,
			Site.Attribute,
			stage,
			exception.GetType().FullName ?? exception.GetType().Name,
			exception.Message));

		// Stop this host here. Continuing with partial questions or answers would only turn
		// one defect into a cascade from the next stage.
		return grammar with
		{
			Text      = null,
			Questions = default,
			Answers   = default,
			Reports   = Values(reports),
		};
	}

	/// <summary>How much a build asked the generator to say about what it did.</summary>
	/// <remarks>
	/// Three levels and not a switch, because the two answers people want are different: a build
	/// wants to know the generator ran, and a measurement wants everything it decided. The middle
	/// one is what a repository leaves on, and the last costs analysis — the carriers are worked
	/// out for the report and for nothing else.
	/// </remarks>
	enum Reporting
	{
		None,
		Summary,
		Full,
	}

	/// <summary>
	/// Everything one host produced, as values: the file to add and the diagnostics to
	/// report.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <c>Diagnostic</c> is built at delivery rather than carried, because building it
	/// needs a <c>DiagnosticDescriptor</c> and a <c>Location</c>, and what those compare
	/// as is not this file's business to depend on. The pieces are strings and numbers,
	/// which compare the way arithmetic does.
	/// </para>
	/// <para>
	/// As the compile leaves it, the text of a grammar written into an attribute has a mark
	/// where each <c>#line</c>'s number goes, and <see cref="Summary"/> a hole for the size of
	/// the text; <see cref="PlacedAt"/> fills both in.
	/// </para>
	/// </remarks>
	/// <param name="Key">The host it came from, to find where that host is written.</param>
	/// <param name="Marked">
	/// Whether any mark in the text is the compile's. False where the grammar holds the mark's
	/// character itself, and then nothing in the text is rewritten.
	/// </param>
	readonly record struct Parser(string Key, string? HintName, string? Text, EquatableArray<Report> Reports, EquatableArray<GeneratedSource> Parts = default, string? Summary = null, string? Detail = null, bool Marked = false)
	{
		/// <summary>The diagnostics, which need a tree to point into and so cannot be cached.</summary>
		public void Report(SourceProductionContext context, Site site, Func<string, SyntaxTree?> treeOf)
		{
			foreach (var report in Reports.Items)
				try
				{
					context.ReportDiagnostic(report.ToRoslyn(treeOf, site));
				}
				catch (Exception exception) when (Recoverable(exception))
				{
					context.ReportDiagnostic(InternalDiagnostic(
						"delivering a grammar diagnostic", exception, site.PlaceOf(report.Anchor), treeOf));
				}

		}

		/// <summary>The parser with where its host is written written into it.</summary>
		/// <remarks>
		/// The step an edit that moves the attribute re-runs instead of the compile: a pass over
		/// the text. Handed back as it is where there is nothing to write, which is every grammar
		/// from a <c>.gram</c> file and every one whose C# has no <c>#line</c>.
		/// </remarks>
		public Parser PlacedAt(Site site)
		{
			if (Text is null)
				return this;

			var path  = site.Path ?? "";
			var text  = Marked ? CSharpEmitter.Placed(Text, path, site.Line, site.Column) : Text;
			var parts = !Marked ? Parts : new EquatableArray<GeneratedSource>(
			[
				.. Parts.Items.Select(part => part with { Text = CSharpEmitter.Placed(part.Text, path, site.Line, site.Column) }),
			]);

			return ReferenceEquals(text, Text) && Summary is null && parts == Parts
				? this
				: this with
				{
					Text    = text,
					Parts   = parts,
					Summary = Summary is null
						? null
						: string.Format(CultureInfo.InvariantCulture, Summary, Encoding.UTF8.GetByteCount(text)),
				};
		}

		/// <summary>
		/// The generated file, which depends on nothing outside the compiled value and so is added
		/// again only when that value changes.
		/// </summary>
		public void Deliver(SourceProductionContext context)
		{
			if (HintName is not null && Text is not null)
				try
				{
					context.AddSource(HintName, Text);
					for (var part = 0; part < Parts.Items.Length; part++)
						context.AddSource(HintName.Substring(0, HintName.Length - 5) + $".part-{part + 1:D4}.g.cs", Parts.Items[part].Text);

					if (Summary is not null)
						context.AddSource(HintName + ".DotGramReport.g.cs", "// " + Summary + "\n");

					// A file of its own rather than more lines in that one, so that the build can
					// print the two at two importances without reading what it prints.
					if (Detail is not null)
						context.AddSource(HintName + ".DotGramReportDetail.g.cs", "// " + Detail + "\n");
				}
				catch (Exception exception) when (Recoverable(exception))
				{
					context.ReportDiagnostic(InternalDiagnostic(
						"adding generated C# to the compilation", exception, null, static _ => null));
				}
		}
	}

	/// <summary>A compiled parser and where its host is written, met at the end of the pipeline.</summary>
	readonly record struct Located(Parser Parser, Site Site);

	/// <summary>Every host's <see cref="Site"/>, by its key.</summary>
	/// <remarks>
	/// Not a value that compares: it is rebuilt only when some site changed, and then everything
	/// that reads it has to look again anyway.
	/// </remarks>
	static Dictionary<string, Site> Sites(ImmutableArray<Site> all)
	{
		var sites = new Dictionary<string, Site>(all.Length, StringComparer.Ordinal);

		foreach (var site in all)
			sites[site.Key] = site;

		return sites;
	}

	static Diagnostic InternalDiagnostic(string stage, Exception exception, Place? location, Func<string, SyntaxTree?> treeOf)
	{
		return Diagnostic.Create(
			Diagnostics.InternalFailure,
			location?.ToLocation(treeOf) ?? Location.None,
			stage,
			exception.GetType().FullName ?? exception.GetType().Name,
			exception.Message);
	}

	/// <summary>
	/// One host's grammar, found and read, with the questions its C# names raise and —
	/// once the middle stage has run — what the host said about them.
	/// </summary>
	/// <remarks>
	/// Every field is a value, which is the point: this is what the expensive stage is
	/// cached on.
	/// </remarks>
	readonly record struct Grammar(
		Host                     Host,
		string?                  Text,
		string?                  Path,
		EquatableArray<Question> Questions,
		EquatableArray<Answer>   Answers,
		EquatableArray<Report>   Reports,
		EquatableArray<Piece>    Pieces = default);

	/// <summary>
	/// One grammar inside the joined text, and everything needed to put a position in it
	/// back where it was written.
	/// </summary>
	/// <remarks>
	/// The joining happens in the cheap stage, where the additional files are; the placing
	/// happens in the third, where the diagnostics are. This is what travels between them,
	/// and both a <c>#line</c> and a squiggle are built from the same one — working out
	/// which grammar a position came from twice, from two sets of numbers, is how the two
	/// come to disagree.
	/// </remarks>
	/// <param name="Anchor">
	/// The attribute a report from this piece falls back to, and whose literal
	/// <paramref name="Literal"/> is, as a <see cref="Site"/> anchor: where it is written is not
	/// the compile's to know (D145).
	/// </param>
	readonly record struct Piece(
		int       Start,
		int       Length,
		string?   Path,
		string?   Literal,
		int       Anchor);

	/// <summary>
	/// Stage one: find the grammar and work out what it needs to know about the host's C#.
	/// No compilation, so this is cached on the grammar text and the host alone.
	/// </summary>
	static Grammar Asked(Host host, ImmutableArray<GrammarFile> files)
	{
		var reports = ImmutableArray.CreateBuilder<Report>();

		if (!host.IsPartial)
		{
			reports.Add(Report.Of(Diagnostics.HostNotPartial, Site.Attribute, host.ClassName));

			return new Grammar(host, null, null, default, default, Values(reports));
		}

		// Said before the grammar is read, because it is about the host rather than about
		// the grammar, and because a name that cannot be a namespace would otherwise reach
		// the splice and come back as a parse error in a text nobody wrote.
		if (host.IncludedAs is { } included && !IsIdentifier(included))
			reports.Add(Report.Of(
				Diagnostics.InvalidIncludedName, Site.Attribute, host.ClassName, included));

		// Said about the host for the same reason: a scope named twice, or named with
		// something that is not a class's name, is settled before a grammar is read.
		if (host.Repeated)
		{
			reports.Add(Report.Of(Diagnostics.RepeatedGrammarScope, Site.Attribute, host.ClassName));

			return new Grammar(host, null, null, default, default, Values(reports));
		}

		if (host.Suffix is { Length: > 0 } scope && !IsIdentifier(scope))
		{
			reports.Add(Report.Of(Diagnostics.InvalidGrammarScope, Site.Attribute, host.ClassName, scope));

			return new Grammar(host, null, null, default, default, Values(reports));
		}

		// A limit or a capacity of nothing is no reading of any input: say so where it is
		// written, rather than let every call throw ArgumentOutOfRangeException.
		foreach (var (option, value) in new[] { (nameof(Host.MaxRetained), host.MaxRetained), (nameof(Host.BufferSize), host.BufferSize) })
		{
			if (value > 0)
				continue;

			reports.Add(Report.Of(Diagnostics.BufferOptionNotPositive, Site.Attribute, option, value.ToString(System.Globalization.CultureInfo.InvariantCulture)));

			return new Grammar(host, null, null, default, default, Values(reports));
		}

		if (!TryResolveGrammar(reports, host, files, out var own, out var path))
			return new Grammar(host, null, null, default, default, Values(reports));

		// What the host inherits, joined onto the end of its own — which is where it goes
		// so that the text somebody is editing keeps the offsets it always had
		// (GrammarSplice). A base whose grammar cannot be found is reported against the
		// class that declares it and left out; the rest still compiles, and what it was
		// going to provide comes back as ordinary undefined names.
		var parts   = new List<GrammarSplice.Part>();
		var bases   = new List<(Included Included, int Anchor)>();

		for (var index = 0; index < host.Includes.Items.Length; index++)
		{
			var inherited = host.Includes.Items[index];
			var anchor    = inherited.Placed ? Site.Include(index) : Site.None;

			// The file first, so that a grammar somebody can still edit is the one whose
			// offsets a diagnostic points into. What the assembly carries is the fallback and
			// is the only thing there is across a reference — reported against nothing rather
			// than reported twice, which is why the first attempt keeps its own list.
			var aside = ImmutableArray.CreateBuilder<Report>();

			if (TryResolveGrammar(
				aside,
				inherited.Source,
				SimpleNameOf(inherited.ClassName),
				inherited.ClassName,
				anchor,
				files,
				out var inheritedText,
				out var inheritedPath))
			{
				parts.Add(new GrammarSplice.Part(inheritedText, inherited.Name, null));
				bases.Add((inherited with { Source = inheritedPath }, anchor));
			}
			else if (inherited.Portable is { Length: > 0 } carried)
			{
				// Carried by the assembly and written nowhere, so no literal of any file is it: a
				// report in it lands on the attribute rather than being looked for in a spelling
				// it was never in.
				parts.Add(new GrammarSplice.Part(carried, inherited.Name, null));
				bases.Add((inherited with { Source = null, Literal = null }, anchor));
			}
			else
			{
				reports.AddRange(aside);
			}
		}

		// Each include is spliced into a namespace named after it, and that is the whole of
		// why one grammar's rules cannot collide with another's. Two under one name are one
		// namespace, and then they can — reported here, where the names are still names,
		// rather than later as a duplicate rule in a namespace nobody wrote.
		foreach (var group in bases.GroupBy(static one => one.Included.Name, StringComparer.Ordinal))
		{
			if (group.Count() < 2)
				continue;

			var first = group.First().Anchor;

			reports.Add(Report.Of(
				Diagnostics.RepeatedIncludedName,
				first == Site.None ? Site.Attribute : first,
				host.ClassName,
				string.Join(" and ", group.Select(static one => SimpleNameOf(one.Included.ClassName))),
				group.Key));
		}

		var (text, joined) = GrammarSplice.Join(new GrammarSplice.Part(own, null, null), parts);

		var pieces = ImmutableArray.CreateBuilder<Piece>();

		pieces.Add(new Piece(0, own.Length, path, host.Literal, Site.Attribute));

		for (var at = 0; at < bases.Count; at++)
			pieces.Add(new Piece(
				joined.Segments[at + 1].Start,
				joined.Segments[at + 1].Length,

				// `Source` now holds the path it resolved to, or null where the grammar was
				// written into the attribute — the same two cases the host's own has.
				bases[at].Included.Source,
				bases[at].Included.Literal,

				// A base in a referenced assembly has no attribute to point at; the host's
				// is the nearest place that is somebody's.
				bases[at].Anchor == Site.None ? Site.Attribute : bases[at].Anchor));

		// Parsed twice over a grammar's life: once here for the questions, once in the
		// third stage for the answer. Both are cheap next to normalization and emission,
		// and this one only re-runs when the grammar itself changes. Through the compiler's
		// own reading, so that what the standard library brings in is asked about too.
		var parsed = GramCompiler.Read(text, RoslynCSharpScanner.Instance).File;

		return new Grammar(
			host,
			text,
			path,
			new EquatableArray<Question>(Questions.Of(parsed, host.LocationType)),
			default,
			Values(reports),
			new EquatableArray<Piece>(pieces.ToImmutable()));
	}

	/// <summary>
	/// Stage three: the grammar compiled against what the host answered. No compilation
	/// reaches here, so it runs only when the grammar or one of the answers changed.
	/// </summary>
	static Parser Compile(Grammar grammar, Reporting reporting, (bool Counts, bool Memoises, bool Traces, bool Collapses, bool Utf8, bool Checks) counting, Positional positional)
	{
		if (grammar.Text is not { } text)
			return new Parser(grammar.Host.Key, null, null, grammar.Reports);

		var host    = grammar.Host;
		var reports = ImmutableArray.CreateBuilder<Report>();

		// More than one piece means a base's grammar was joined onto this one, which is
		// what makes this class a dialect: its parser stands beside the base's and names
		// its members the same. A base whose grammar could not be found leaves one piece
		// and nothing to stand beside, which is the right answer either way.
		var inherits = grammar.Pieces.Items.Length > 1;

		// What the first stage had to say, carried rather than dropped. Every report it
		// used to make came with an early return — no text, so the branch above hands them
		// on — and the first one that could stand beside a grammar that reads perfectly
		// well went silently missing until it was looked for.
		reports.AddRange(grammar.Reports.Items);

		// Whether a `#line` into the host's literal may be left to the output step as a mark. Not
		// where the grammar itself holds the mark's character: its C# is copied into the output as
		// written, and a mark the author wrote would be rewritten as one the compile did. Such a
		// grammar keeps its literal's C# without a `#line` rather than risk a broken file.
		var marked = CSharpEmitter.CanMark(text);

		var timer = reporting != Reporting.None ? Stopwatch.StartNew() : null;
		var result = GramCompiler.Compile(text, new GramCompilerOptions
		{
			FileName       = grammar.Path ?? host.SimpleName + GramFileExtension,
			ClassName      = host.ClassName,
			Namespace      = host.Namespace,
			SymbolResolver = new AnsweredSymbolResolver(grammar.Answers.Items),
			CSharpScanner  = RoslynCSharpScanner.Instance,
			LanguageId     = host.LanguageId,
			LanguageSource = host.LanguageId is null ? null : text,
			LanguageClassifications = host.LanguageId is null ? null : host.LanguageClassifications,
			LanguageRecognitionContract = host.LanguageId is null ? null : host.LanguageRecognitionContract,

			// §7.6. A grammar that is its own file maps onto itself; one written into an
			// attribute maps into the C# file holding it, which has to be searched for
			// rather than computed — see InlineLineMap. Where a host inherits grammars the
			// text is several of them joined, and the map is one per piece with the same
			// two cases inside it.
			LineMap        = MapOf(grammar, text, marked),

			// Nought is what the attribute holds when nobody set it, and what somebody
			// setting it to nought means: take the measured default either way.
			PartSize       = host.PartSize == 0 ? null : host.PartSize,
			Lexical        = host.Lexical,
			LocationType   = host.LocationType,
			LocatedFacade  = host.LocatedFacade,
			LocatedFacadeDeclared = host.LocatedFacadeDeclared,

			// The host of every grammar this one is built on: what a rule from there calls
			// lives beside it, and `using static` is what puts it in reach without the
			// including class having to derive from anything.
			StaticImports  = [.. host.Includes.Items.Select(static one => one.ClassName)],
			Portable       = host.Portable,
			Carrier        = (CarrierKind)host.Carrier,
			BufferedInput  = host.BufferedInput,
			BufferedBytes  = host.BufferedBytes,
			SpanCaptures   = host.SpanCaptures,
			MaxRetained    = host.MaxRetained,
			BufferSize     = host.BufferSize,
			Stacks         = host.Stacks,
			Suffix         = host.Suffix,
			SuffixDeclared = host.SuffixDeclared,
			SharedTypes    = host.Shared,
			Inherits       = inherits,
			Own            = inherits ? grammar.Pieces.Items[0].Length : null,

			// The report asked for is the carriers' too: the carrier of each machine and what the
			// tape would hold back, rule by rule.
			// Only the full one — this is analysis nobody pays for who is not reading it.
			ReportCarriers = reporting == Reporting.Full,
			CountRules     = counting.Counts,
			MemoiseFailures = counting.Memoises,
			Trace           = host.Trace || counting.Traces,
			CollapseForwarders = counting.Collapses,
			Utf8Literals    = counting.Utf8,
			Checks          = counting.Checks,

			// Experimental and off unless the build asks (DotGramPositionalFollow).
			PositionalFollow = positional != Positional.Off,
			PositionalFollowSplit = positional == Positional.Split,
		});

		timer?.Stop();

		foreach (var diagnostic in result.Diagnostics)
			reports.Add(PlacedIn(grammar, text, diagnostic));

		return new Parser(
			host.Key,
			result.Sources.Count > 0 ? host.HintName + ".g.cs" : null,
			result.Sources.Count > 0 ? result.Sources[0].Text  : null,
			Values(reports),
			new EquatableArray<GeneratedSource>([.. result.Sources.Skip(1)]),
			// The one line a quiet build shows: the generator ran, on this host, and this is what
			// came out of it. What it decided on the way — the mode, the options it was given,
			// and the carrier of every machine — is the detail beside it, which only a build that
			// asked for the full report has at all. The size is the placed text's, so it is left a
			// hole here and filled in where the text is placed (Parser.PlacedAt).
			timer is not null && result.Sources.Count > 0
				? string.Format(CultureInfo.InvariantCulture,
					"DotGram: {0}, {1} normalized rules, {{0}} bytes UTF-8 C#, {2:F2} ms generation",
					host.HintName, result.NormalizedRuleCount, timer.Elapsed.TotalMilliseconds)
				: null,
			reporting == Reporting.Full && result.Sources.Count > 0
				? string.Format(CultureInfo.InvariantCulture,
					"DotGram: {0}, mode={1}, options: Lexical={2}, " +
					"Carrier={3}, BufferedInput={4}, BufferedBytes={5}, SpanCaptures={6}",
					host.HintName,
					result.UsesLexical ? "lexical" : "characters", host.Lexical,
					(CarrierKind)host.Carrier,
					host.BufferedInput, host.BufferedBytes, host.SpanCaptures) +
					string.Concat((result.Carriers ?? []).Select(static line => "\n// " + line))
				: null,
			marked);
	}

	/// <summary>Where each piece of the joined text belongs (§7.6).</summary>
	/// <param name="marked">Whether the host's own literal may be mapped with marks at all.</param>
	static ILineMap? MapOf(Grammar grammar, string text, bool marked)
	{
		var pieces = grammar.Pieces.Items;

		if (pieces.Length == 0)
			return MapOfPiece(new Piece(0, text.Length, grammar.Path, grammar.Host.Literal, Site.Attribute), text, marked);

		// One piece is the ordinary case and needs no splicing over it: a host inheriting
		// nothing compiles the map it always did.
		if (pieces.Length == 1)
			return MapOfPiece(pieces[0], text, marked);

		return new SplicedLineMap(
		[
			.. pieces.Select((piece, at) => new SplicedLineMap.Segment(
				piece.Start, piece.Length, MapOfPiece(piece, text, marked && at == 0))),
		]);
	}

	/// <param name="inline">
	/// Whether the piece, if it is written into an attribute, is mapped into its C# file. Only the
	/// host's own is: a base's literal is left without a <c>#line</c>, as it always was.
	/// </param>
	static ILineMap? MapOfPiece(Piece piece, string text, bool inline)
	{
		var written = text.Substring(piece.Start, piece.Length);

		return piece.Path is { } path
			? new GrammarLineMap(written, path)
			: inline && piece.Literal is { } spelling
				? new InlineLineMap(written, spelling)
				: null;
	}

	/// <summary>
	/// A diagnostic placed in the grammar it came from rather than in the joined text.
	/// </summary>
	/// <remarks>
	/// Its position arrives in the joined text's offsets and has to leave in one grammar's,
	/// because that is what a squiggle is put on. A position in the wrapper a joined grammar
	/// is written into belongs to no grammar; it keeps the host's own fallback, which puts
	/// the message on the class rather than nowhere.
	/// </remarks>
	static Report PlacedIn(Grammar grammar, string text, GramDiagnostic diagnostic)
	{
		foreach (var piece in grammar.Pieces.Items)
		{
			if (diagnostic.Position < piece.Start || diagnostic.Position > piece.Start + piece.Length)
				continue;

			// Cut at the piece's end: an unterminated `@(` runs to the end of the joined
			// text, and what lies past the piece is not this grammar's to underline.
			return Report.Of(
				new GramDiagnostic(
					diagnostic.Id,
					diagnostic.Message,
					diagnostic.Position - piece.Start,
					Math.Min(diagnostic.Length, piece.Start + piece.Length - diagnostic.Position),
					diagnostic.Severity),
				piece.Path,
				text.Substring(piece.Start, piece.Length),
				piece.Anchor,
				piece.Literal);
		}

		// Past every piece — the standard library, which is spliced on after the pieces are
		// cut. Nowhere to point, so the class it is, with no offset into a literal that does
		// not hold the position.
		return Report.Of(
			new GramDiagnostic(diagnostic.Id, diagnostic.Message, 0, 0, diagnostic.Severity),
			null, text, Site.Attribute);
	}

	/// <summary>The innermost name of a dotted one.</summary>
	static string SimpleNameOf(string className)
	{
		var dot = className.LastIndexOf('.');

		return dot < 0 ? className : className.Substring(dot + 1);
	}

	/// <summary>One identifier, which is all a namespace can be named by.</summary>
	static bool IsIdentifier(string name)
	{
		if (name.Length == 0 || !(char.IsLetter(name[0]) || name[0] == '_'))
			return false;

		for (var at = 1; at < name.Length; at++)
			if (!(char.IsLetterOrDigit(name[at]) || name[at] == '_'))
				return false;

		return true;
	}

	static EquatableArray<Report> Values(ImmutableArray<Report>.Builder reports)
	{
		return new(reports.ToImmutable());
	}

	/// <summary>
	/// Works out which grammar a host means: the text written into the attribute, an
	/// explicit path, or — with no argument at all — the file named after the class.
	/// </summary>
	static bool TryResolveGrammar(
		ImmutableArray<Report>.Builder reports,
		Host host,
		ImmutableArray<GrammarFile> files,
		out string text,
		out string? path)
	{
		return TryResolveGrammar(
			reports, host.Source, host.SimpleName, host.ClassName, Site.Attribute, files,
			out text, out path);
	}

	/// <summary>
	/// The same for a grammar that is not this host's — one it inherits.
	/// </summary>
	/// <remarks>
	/// Told apart from the host's own by nothing at all, which is the point: a base's
	/// grammar is found the way any grammar is, and its diagnostics are placed against the
	/// class that declares it rather than against the one that inherited it.
	/// </remarks>
	static bool TryResolveGrammar(
		ImmutableArray<Report>.Builder reports,
		string?                        source,
		string                         simpleName,
		string                         className,
		int                            anchor,
		ImmutableArray<GrammarFile>    files,
		out string                     text,
		out string?                    path)
	{
		text = "";
		path = null;

		// A single line ending in .gram is a path; anything else is the grammar itself.
		// The two are told apart exactly the way the attribute documents it, and a grammar
		// short enough to be mistaken for a path would not be a grammar.
		if (source is { } written && !IsPath(written))
		{
			text = written;

			return true;
		}

		var wanted = source ?? simpleName + GramFileExtension;
		var found  = files.Where(file => Matches(file.Path, wanted)).ToImmutableArray();

		switch (found.Length)
		{
			case 1:
				text = found[0].Text;
				path = found[0].Path;

				return true;

			case 0:
				reports.Add(Report.Of(
					Diagnostics.GrammarFileNotFound, anchor, wanted, className));

				return false;

			default:
				// Picking one by reference order would make which file won invisible.
				reports.Add(Report.Of(
					Diagnostics.AmbiguousGrammarFile,
					anchor,
					wanted,
					string.Join(", ", found.Select(file => file.Path))));

				return false;
		}
	}

	static bool IsPath(string source)
	{
		return GramIncludes.IsPath(source);
	}

	/// <summary>
	/// A file matches a wanted path when it ends with it on a separator boundary — the
	/// attribute names a path relative to the project, and what reaches us is absolute.
	/// </summary>
	static bool Matches(string filePath, string wanted)
	{
		return GramIncludes.Matches(filePath, wanted);
	}

	// ── What the shell carries between stages ────────────────────────────────────

	readonly record struct GrammarFile(string Path, string Text);

	/// <summary>
	/// A grammar a host inherits, as the attribute on that base class spells it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Values and not symbols, for the reason <see cref="Host"/> gives about itself. Read in
	/// the cheap stage all the same: <c>ForAttributeWithMetadataName</c> hands over the
	/// target's symbol because that provider is semantic anyway, so walking to a base and
	/// reading a constant off it costs no dependency on the compilation and loses no
	/// caching. A string is equatable, and editing a base's grammar invalidates its
	/// derivatives exactly as it should.
	/// </para>
	/// <para>
	/// <see cref="Source"/> is the attribute's argument unresolved — a path or the text
	/// itself, told apart the same way the host's own is, and by the same code, one stage
	/// later where the additional files are in hand.
	/// </para>
	/// </remarks>
	/// <param name="Name">What a grammar including this one writes after `using`.</param>
	/// <param name="ClassName">Whose grammar it is, for anything that has to say so.</param>
	/// <param name="Placed">
	/// Whether its attribute is in this compilation's source, so a report can point at it. Where
	/// it is is the host's <see cref="Site"/>, which the compile does not read.
	/// </param>
	/// <param name="Portable">
	/// The grammar as the included class carries it — <c>[GramSource]</c>, written there by
	/// the generator that compiled it. Null where the class was not compiled by this
	/// generator. What makes an include work across an assembly boundary, where the
	/// <c>.gram</c> file is nowhere in reach.
	/// </param>
	readonly record struct Included(
		string    Name,
		string    ClassName,
		string?   Source,
		string?   Literal,
		bool      Placed,
		string?   Portable = null);

	/// <summary>
	/// A class marked <c>[Gram]</c>, reduced to what generation needs.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Deliberately values rather than symbols: an incremental generator compares what
	/// each step produced to decide whether the next one must run again, and a symbol
	/// compares equal to nothing across compilations.
	/// </para>
	/// <para>
	/// And what the class says rather than where it says it. Where — the attribute, where the
	/// literal begins, its line and column — is the host's <see cref="Site"/>, which travels
	/// beside this and reaches only the output and the reporting, so that an edit moving the
	/// class and leaving the grammar as it was leaves the compile to be reused (D145).
	/// </para>
	/// </remarks>
	/// <param name="Key">
	/// Which of the class's readings this is, to find its <see cref="Site"/> by: two readings
	/// may name one scope, and that is reported against each of them.
	/// </param>
	/// <param name="Literal">
	/// The grammar exactly as the attribute spells it — quotes, escapes, indentation and
	/// all — or null when the grammar came from a file.
	/// </param>
	readonly record struct Host(
		string    Key,
		string    ClassName,
		string?   Namespace,
		string    HintName,
		bool      IsPartial,
		string?   Source,
		string?   LanguageId,
		string    LanguageClassifications,
		string    LanguageRecognitionContract,
		string?   Literal    = null,
		string?   IncludedAs = null,
		EquatableArray<Included> Includes = default,
		int       PartSize   = 0,
		bool      Lexical    = false,
		int       Carrier    = 0,
		bool      BufferedInput = false,
		bool      BufferedBytes = false,
		bool      SpanCaptures = false,
		int       MaxRetained = int.MaxValue,
		int       BufferSize = 4096,
		int       Stacks     = 0,
		string?   Suffix     = null,
		bool      SuffixDeclared = false,
		bool      Repeated   = false,
		bool?     Shared     = null,
		string?   LocationType = null,
		bool      Portable   = false,
		bool      PerCall    = false,
		string?   LocatedFacade = null,
		bool      LocatedFacadeDeclared = false,
		bool      Trace      = false)
	{
		/// <summary>
		/// The name a grammar including this one writes after <c>using</c>.
		/// </summary>
		/// <remarks>
		/// The host's own name unless the attribute said otherwise, so that following the
		/// <c>:</c> from an including class lands on the answer. Not the C# namespace of
		/// the generated code, which is decided by where the host is declared — the two
		/// senses of the word were separated on purpose (docs/next.md, the `context` to
		/// `namespace` rename) and are kept apart here by not using it.
		/// </remarks>
		public string IncludedName => IncludedAs ?? SimpleName;

		/// <summary>
		/// The host as metadata names it, for looking its own members up.
		/// </summary>
		/// <remarks>
		/// Nested classes are joined by <c>+</c> and not by <c>.</c>, which is the whole
		/// difference between what a grammar writes and what a compilation is asked. Type
		/// parameters are not part of it — <c>Parser&lt;T&gt;</c> is metadata's
		/// <c>Parser`1</c> — so a generic host is left alone rather than looked up wrongly.
		/// </remarks>
		public string? MetadataName
		{
			get
			{
				if (ClassName.IndexOf('<') >= 0)
					return null;

				var nested = ClassName.Replace('.', '+');

				return Namespace is null ? nested : Namespace + "." + nested;
			}
		}

		/// <summary>
		/// The class the grammar is looked up by: the innermost one, without its type
		/// parameters — <c>Parser&lt;T&gt;</c> looks for <c>Parser.gram</c>.
		/// </summary>
		public string SimpleName
		{
			get
			{
				// The innermost class first, then its type parameters: a type parameter
				// list never contains a dot, but a nested name does.
				var name = ClassName;
				var dot  = name.LastIndexOf('.');

				if (dot >= 0)
					name = name.Substring(dot + 1);

				var angle = name.IndexOf('<');

				return angle < 0 ? name : name.Substring(0, angle);
			}
		}

		static string TypeParametersOf(ClassDeclarationSyntax declaration)
		{
			return declaration.TypeParameterList is { Parameters.Count: > 0 } parameters
				? "<" + string.Join(", ", parameters.Parameters.Select(static p => p.Identifier.ValueText)) + ">"
				: "";
		}

		/// <summary>
		/// One host per reading the class asks for: the <c>[Gram]</c>, and then every
		/// <c>[GramOptions]</c> in the order written.
		/// </summary>
		/// <remarks>
		/// Several are several compilations of the class, each in a nested class of its own
		/// and a file of its own. A suffix written twice, or left off twice, would name one
		/// scope twice; the second one to do it is marked here and told so where the rest of
		/// the host's own diagnostics are said.
		/// </remarks>
		public static ImmutableArray<(Host Host, Site Site)> All(GeneratorAttributeSyntaxContext candidate)
		{
			var type     = (INamedTypeSymbol)candidate.TargetSymbol;
			var readings = new List<AttributeData>(candidate.Attributes);

			// Only the class's own: `[GramOptions]` is what a class writes about its own
			// grammar, and a base class's readings are that class's compilations.
			foreach (var attribute in type.GetAttributes())
				if (attribute.AttributeClass?.ToDisplayString() == GramOptionsAttribute)
					readings.Add(attribute);

			var hosts = ImmutableArray.CreateBuilder<(Host Host, Site Site)>(readings.Count);
			var taken = new HashSet<string>(StringComparer.Ordinal);

			// A reading that differs from the first only in being told where its values were
			// written, decided per call: not a compilation of its own but a nested class of
			// published methods over the first's parser, which is compiled with the location
			// type and told the class's name.
			string? facade         = null;
			string? facadeLocation = null;
			var     facadeDeclared = false;

			for (var index = 1; index < readings.Count; index++)
			{
				var (perCall, _) = From(candidate, readings[index], index);

				if (!perCall.PerCall || perCall.LocationType is null || perCall.Suffix is not { Length: > 0 } || facade is not null)
					continue;

				facade         = perCall.Suffix;
				facadeLocation = perCall.LocationType;
				facadeDeclared = perCall.SuffixDeclared;
				readings.RemoveAt(index);
				index--;
			}

			foreach (var attribute in readings)
			{
				// Everything a reading does not say, it takes from the grammar's own: which
				// grammar, whether it is read as tokens, how it is divided. That is what a
				// `[GramOptions]` means — the same parser, compiled differently — and what it
				// does say is the difference. The first has nothing to take from.
				var (host, site) = From(candidate, attribute, hosts.Count, hosts.Count > 0 ? hosts[0] : null);

				hosts.Add((
					host with
					{
						Repeated = !taken.Add(host.Suffix ?? ""),

						// The first writes what the host's own C# names; the rest read it from the
						// class around them. Null where there is nothing to share it with.
						Shared   = readings.Count > 1 ? hosts.Count == 0 : null,
					},
					site));
			}

			// Given to the first only once every reading has taken what it inherits from it: the
			// location type is the per-call facade's, and another reading compiled into a class of its
			// own would otherwise inherit it and locate on every call.
			if (facade is not null)
				hosts[0] = (
					hosts[0].Host with
					{
						LocationType          = facadeLocation,
						LocatedFacade         = facade,
						LocatedFacadeDeclared = facadeDeclared,
					},
					hosts[0].Site);

			return hosts.ToImmutable();
		}

		/// <param name="reading">Which of the class's readings this is, the first being 0.</param>
		static (Host Host, Site Site) From(
			GeneratorAttributeSyntaxContext candidate, AttributeData attribute, int reading, (Host Host, Site Site)? firstRead = null)
		{
			var first = firstRead?.Host;

			var type        = (INamedTypeSymbol)candidate.TargetSymbol;
			var declaration = (ClassDeclarationSyntax)candidate.TargetNode;

			var source = attribute.ConstructorArguments.Length == 1
				? attribute.ConstructorArguments[0].Value as string
				: null;

			var named = source is null && first is not null;

			var includedAs = attribute.NamedArguments
				.FirstOrDefault(static named => named.Key == nameof(Host.IncludedAs))
				.Value.Value as string;
			var languageId = type.GetAttributes()
				.FirstOrDefault(static candidate =>
					candidate.AttributeClass?.ToDisplayString() == "DotGram.GramLanguageAttribute")
				?.ConstructorArguments.FirstOrDefault().Value as string;
			var classifications = string.Join("\n", type.GetAttributes()
				.Where(static candidate =>
					candidate.AttributeClass?.ToDisplayString() == "DotGram.GramClassifyAttribute")
				.Select(Classification)
				.Where(static value => value is not null));
			var recognitionContract = string.Join("\n", type.GetAttributes()
				.Select(RecognitionContract)
				.Where(static value => value is not null));

			// Zero for "nothing was said", which is also what a consumer writing
			// `PartSize = 0` means: the emitter reads it as no wish and takes its default.
			// Anything else goes through as written and is answered with a parser, however
			// unreasonable — see `Machine.PartSize`.
			var partSize = attribute.NamedArguments
				.FirstOrDefault(static named => named.Key == nameof(Host.PartSize))
				.Value.Value as int? ?? first?.PartSize ?? 0;

			// A request and not a setting: a grammar that cannot be cut in two is compiled
			// over characters and told why (GRAM5004), so nothing written here fails a build.
			var lexical = attribute.NamedArguments
				.FirstOrDefault(static named => named.Key == nameof(Host.Lexical))
				.Value.Value as bool? ?? first?.Lexical ?? false;

			// A `typeof(…)` argument arrives as the symbol it named, and what the compiler
			// needs of it is a name it can ask the resolver about — the same currency every
			// other type in a grammar is written in.
			// What the class already says, unless the attribute says otherwise: a host nobody
			// outside can name is a host nobody outside can include, and carrying its grammar
			// would be paying for a door into a wall.
			var portable = attribute.NamedArguments
				.FirstOrDefault(static named => named.Key == nameof(Host.Portable))
				.Value.Value as bool? ?? first?.Portable ?? Visible(type);

			var locationType = (attribute.NamedArguments
				.FirstOrDefault(static named => named.Key == nameof(Host.LocationType))
				.Value.Value as INamedTypeSymbol)?.ToDisplayString() ?? first?.LocationType;

			// Which carrier the author chose (docs/next.md, the redesign). An enum constant
			// reaches an analyzer as its underlying integer, and nought is the generator's own
			// choice.
			var carrier = attribute.NamedArguments
				.FirstOrDefault(static named => named.Key == nameof(Host.Carrier))
				.Value.Value as int? ?? first?.Carrier ?? 0;

			// How many stacks a parse may take past the one it began on. Nought is as many
			// as there is memory for, which is the default.
			var stacks = attribute.NamedArguments
				.FirstOrDefault(static named => named.Key == nameof(Host.Stacks))
				.Value.Value as int? ?? first?.Stacks ?? 0;

			// Which nested class this compilation goes into, where the host has more than
			// one grammar. Null is the host class itself, which one of them may be.
			var suffix = attribute.NamedArguments
				.FirstOrDefault(static named => named.Key == nameof(Host.Suffix))
				.Value.Value as string;

			// Whether the host declares that nested class itself, and so says how visible it
			// is. What a generator sees is the author's code alone — its own output is not in
			// the compilation yet — so a member of that name is the author's.
			var suffixDeclared = suffix is { Length: > 0 } && type.GetTypeMembers(suffix).Length > 0;

			// A request, like `Lexical`: a grammar the reader cannot write is written the
			// way it was before the reader existed and told so (GRAM5006).
			// The literal as written, kept beside the value it decodes to. A diagnostic
			// carries an offset into the value; putting it where the author can see it
			// means finding that place in the spelling, and the spelling is the only thing
			// that knows where the escapes and the indentation went.
			// The first positional argument and not the only one: a named argument beside it
			// is legal — `[Gram("…", IncludedAs = "Json")]` — and requiring exactly one
			// would quietly stop finding the spelling the moment somebody wrote one, taking
			// every diagnostic's placement with it.
			var written = attribute.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax syntax &&
				syntax.ArgumentList?.Arguments.FirstOrDefault(
					static argument => argument.NameEquals is null) is
						{ Expression: LiteralExpressionSyntax spelled }
					? spelled.Token
					: default;

			// A nested host is written back out nested, so every enclosing class has to be
			// partial too. Checking here says so at the class; leaving it says so at
			// generated code the author never wrote.
			var names     = new List<string>();
			var isPartial = true;

			for (var node = declaration; node is not null; node = node.Parent as ClassDeclarationSyntax)
			{
				// With the type parameters: a partial declaration has to name them the
				// same way, or it declares a different type. Their constraints do not
				// have to be repeated, and are not.
				names.Insert(0, node.Identifier.ValueText + TypeParametersOf(node));
				isPartial &= node.Modifiers.Any(modifier => modifier.ValueText == "partial");
			}

			var hintName = type.ToDisplayString().Replace('<', '_').Replace('>', '_') +
				(suffix is { Length: > 0 } ? "." + suffix : "");
			var key      = hintName + "#" + reading.ToString(CultureInfo.InvariantCulture);
			var included = Inherited(type);

			// Where the literal is: the first's where the grammar is, as the spelling is.
			var literal = written == default
				? default
				: written.GetLocation().GetLineSpan().StartLinePosition;

			var site = named
				? firstRead!.Value.Site with { Key = key }
				: new Site(
					key,
					At:        null,
					Path:      written == default ? null : candidate.SemanticModel.SyntaxTree.FilePath,
					LiteralAt: written == default ? 0 : written.SpanStart,
					Line:      literal.Line + 1,
					Column:    literal.Character + 1,
					Bases:     default);

			site = site with
			{
				At    = Place.Of(attribute.ApplicationSyntaxReference is { } reference
					? Microsoft.CodeAnalysis.Location.Create(reference.SyntaxTree, reference.Span)
					: declaration.Identifier.GetLocation()),
				Bases = new EquatableArray<Site.Base>([.. included.Select(static one => one.Site)]),
			};

			var host = new Host(
				Key:       key,
				ClassName: string.Join(".", names),
				Namespace: type.ContainingNamespace.IsGlobalNamespace
					? null
					: type.ContainingNamespace.ToDisplayString(),
				HintName:  hintName,
				IsPartial: isPartial,
				Source:    named ? first!.Value.Source : source,
				LanguageId: languageId,
				LanguageClassifications: classifications,
				LanguageRecognitionContract: recognitionContract,
				// The spelling stays the first's where the grammar is: a diagnostic carries an
				// offset into the grammar, and putting it where the author can see it means
				// finding it in the text they actually wrote.
				Literal:    named ? first!.Value.Literal   : written == default ? null : written.Text,
				IncludedAs: includedAs,
				Includes:   new EquatableArray<Included>([.. included.Select(static one => one.Included)]),
				PartSize:   partSize,
				Lexical:    lexical,
				Carrier:    carrier,
				SpanCaptures: attribute.NamedArguments.FirstOrDefault(static named => named.Key == nameof(Host.SpanCaptures)).Value.Value as bool? ?? first?.SpanCaptures ?? false,
				BufferedBytes: attribute.NamedArguments.FirstOrDefault(static named => named.Key == nameof(Host.BufferedBytes)).Value.Value as bool? ?? first?.BufferedBytes ?? false,
				BufferedInput: attribute.NamedArguments.FirstOrDefault(static named => named.Key == nameof(Host.BufferedInput)).Value.Value as bool? ?? first?.BufferedInput ?? false,
				MaxRetained: attribute.NamedArguments.FirstOrDefault(static named => named.Key == nameof(Host.MaxRetained)).Value.Value as int? ?? first?.MaxRetained ?? int.MaxValue,
				BufferSize: attribute.NamedArguments.FirstOrDefault(static named => named.Key == nameof(Host.BufferSize)).Value.Value as int? ?? first?.BufferSize ?? 4096,
				Stacks:     stacks,
				Suffix:     suffix,
				SuffixDeclared: suffixDeclared,
				LocationType: locationType,
				Portable:   portable,
				PerCall:    attribute.NamedArguments.FirstOrDefault(static named => named.Key == nameof(Host.PerCall)).Value.Value as bool? ?? false,
				// A trace build of this reading (GramCompilerOptions.Trace), which a later reading
				// takes from the first like everything else it does not say.
				Trace:      attribute.NamedArguments.FirstOrDefault(static named => named.Key == nameof(Host.Trace)).Value.Value as bool? ?? first?.Trace ?? false);

			return (host, site);
		}

		/// <summary>Whether anything outside this assembly could name the class.</summary>
		static bool Visible(INamedTypeSymbol type)
		{
			for (var at = (ITypeSymbol?)type; at is not null; at = at.ContainingType)
				if (at.DeclaredAccessibility != Accessibility.Public)
					return false;

			return true;
		}

		static string? Classification(AttributeData attribute)
		{
			if (attribute.ConstructorArguments is not
				[
					{ Value: string target },
					{ Kind: TypedConstantKind.Enum, Value: not null } role,
				])
				return null;

			var field = role.Type?.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(candidate =>
				candidate.HasConstantValue && Equals(candidate.ConstantValue, role.Value));
			return field is null ? null : target + "\t" + field.Name;
		}

		static string? RecognitionContract(AttributeData attribute)
		{
			var name = attribute.AttributeClass?.ToDisplayString();
			return (name, attribute.ConstructorArguments) switch
			{
				("DotGram.GramToolingGuardAttribute", [{ Value: string expression }, { Value: bool accepted }]) =>
					"G\t" + expression + "\t" + (accepted ? "1" : "0"),
				("DotGram.GramToolingExternalAttribute", [{ Value: string method }, { Value: string rule }]) =>
					"E\t" + method + "\t" + rule,
				_ => null,
			};
		}

		/// <summary>Every grammar this one is built on, nearest first.</summary>
		/// <remarks>
		/// The walk is <see cref="GramIncludes.Walk"/>, which the editor reads too, so that what
		/// it analyses a grammar with is what this compiles it with. What is added here is
		/// where each one's attribute is written, for a report to point at.
		/// </remarks>
		static ImmutableArray<(Included Included, Site.Base Site)> Inherited(INamedTypeSymbol type)
		{
			var included = ImmutableArray.CreateBuilder<(Included, Site.Base)>();

			foreach (var include in GramIncludes.Walk(type))
			{
				var attribute = include.Attribute;

				var spelled = attribute.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax syntax &&
					syntax.ArgumentList?.Arguments.FirstOrDefault(
						static argument => argument.NameEquals is null) is
							{ Expression: LiteralExpressionSyntax literal }
						? literal.Token
						: default;

				// Null where the base is in a referenced assembly, which is what makes
				// a diagnostic in its grammar have nowhere to point (docs/next.md).
				var at = Place.Of(attribute.ApplicationSyntaxReference is { } reference
					? Microsoft.CodeAnalysis.Location.Create(reference.SyntaxTree, reference.Span)
					: null);

				included.Add((
					new Included(
						Name:      include.Name,
						ClassName: include.Type.ToDisplayString(),
						Source:    include.Source,
						Literal:   spelled == default ? null : spelled.Text,
						Placed:    at is not null,
						Portable:  include.Portable),
					new Site.Base(at, spelled == default ? 0 : spelled.SpanStart)));
			}

			return included.ToImmutable();
		}
	}
}
