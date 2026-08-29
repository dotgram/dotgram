using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
	const string GramAttribute     = "DotGram.GramAttribute";

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
	public const string SharedStage   = "Shared";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// `[Gram]` itself: always internal, always present, so that the attribute can be
		// written in source at all and nothing has to be found anywhere.
		context.RegisterPostInitializationOutput(static postInit =>
		{
			var source = GramCompiler.EmitMarkerAttributes();

			postInit.AddSource(source.HintName, source.Text);
		});

		// The unit of generation is the host class, not the file (§1). A .gram file no
		// class claims generates nothing — there would be nowhere to put the result.
		var hosts = context.SyntaxProvider.ForAttributeWithMetadataName(
			GramAttribute,
			static (node, _) => node is ClassDeclarationSyntax,
			static (candidate, _) => Host.From(candidate));

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
		//
		// So editing a C# file re-runs the middle stage — a handful of symbol lookups —
		// and stops there, because the answers it produces are the same ones.
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

		context.RegisterSourceOutput(
			answered
				.Select(static (grammar, _) => CompileSafely(grammar))
				.WithTrackingName(CompiledStage),
			static (production, parser) => parser.Deliver(production));

		// One `context` and one `state` for the whole assembly, checked apart from the
		// generation above rather than folded into it: a `Collect` in that pipeline would
		// make every parser's output depend on every grammar, and a keystroke in one would
		// recompile all of them. Here nothing is generated — only said — and the collected
		// value is the few grammars that declare anything, so it holds still while the rest
		// of the project is edited.
		context.RegisterSourceOutput(
			asked
				.Where(static grammar => grammar.Declares.Any)
				.Collect()
				.WithTrackingName(SharedStage),
			static (production, declaring) => DeliverShared(production, declaring));
	}

	/// <summary>
	/// Says so where two grammars in one assembly both declare a <c>context</c> or a
	/// <c>state</c>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Taken now, before anything depends on it, because it is the constraint that keeps a
	/// grammar includable in another later (docs/next.md, "Considered: parser inheritance").
	/// A merged grammar can have only one of each — a context is one object handed to the
	/// whole parse, and a mark is one type for all of them — and an assembly is the widest
	/// extent this can be asked over without a base-class relationship to follow.
	/// </para>
	/// <para>
	/// It is stricter than the rule that will eventually be wanted, which is one per
	/// inheritance chain: two parsers in one assembly that never meet are refused here for a
	/// reason neither of them can see. That is the price of taking it early, and the message
	/// says why rather than only what.
	/// </para>
	/// <para>
	/// Reported at every site rather than at all but one. There is no first among them —
	/// which grammar the generator saw first is not something an author can know or act on —
	/// and whichever file they are looking at is where they need to be told.
	/// </para>
	/// </remarks>
	static void DeliverShared(SourceProductionContext production, ImmutableArray<Grammar> declaring)
	{
		Say(
			DotGram.Grammar.Binding.GrammarBinder.SharedContext,
			"context",
			static grammar => grammar.Declares.ContextAt,
			static grammar => grammar.Declares.ContextLength);

		Say(
			DotGram.Grammar.Binding.GrammarBinder.SharedState,
			"state",
			static grammar => grammar.Declares.StateAt,
			static grammar => grammar.Declares.StateLength);

		void Say(string id, string word, Func<Grammar, int> at, Func<Grammar, int> length)
		{
			var declared = declaring.Where(grammar => at(grammar) >= 0).ToArray();

			if (declared.Length < 2)
				return;

			// Named in a fixed order, so the message does not change with the order the
			// generator happened to see them in.
			var named = declared
				.Select(static grammar => grammar.Host.ClassName)
				.OrderBy(static name => name, StringComparer.Ordinal)
				.ToArray();

			foreach (var grammar in declared)
			{
				var others = string.Join(
					", ", named.Where(name => name != grammar.Host.ClassName));

				var diagnostic = new GramDiagnostic(
					id,
					$"'{grammar.Host.ClassName}' declares a '{word}' and so does {others}. An " +
					$"assembly has at most one, so that one grammar can be included in another: a " +
					$"'{word}' belongs to a whole parse, and a merged grammar could not say which " +
					$"of two it meant.",
					at(grammar),
					length(grammar),
					GramSeverity.Error);

				new Parser(
					null,
					null,
					new EquatableArray<Report>(
					[
						Report.Of(
							diagnostic,
							grammar.Path,
							grammar.Text ?? "",
							grammar.Host.Location,
							grammar.Host.Literal,
							grammar.Host.LiteralAt),
					]))
					.Deliver(production);
			}
		}
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
				new Grammar(host, null, null, default, default, default, Declared.None),
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
					new RoslynSymbolResolver(compilation, grammar.Host.MetadataName))),
			};
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			return Failed(grammar, "answering C# type questions", exception);
		}
	}

	static Parser CompileSafely(Grammar grammar)
	{
		try
		{
			return Compile(grammar);
		}
		catch (Exception exception) when (Recoverable(exception))
		{
			var failed = Failed(grammar, "compiling the recognition graph", exception);

			return new Parser(null, null, failed.Reports);
		}
	}

	static bool Recoverable(Exception exception) =>
		exception is not OperationCanceledException and not OutOfMemoryException;

	static Grammar Failed(Grammar grammar, string stage, Exception exception)
	{
		var reports = ImmutableArray.CreateBuilder<Report>();

		reports.AddRange(grammar.Reports.Items);
		reports.Add(Report.Of(
			Diagnostics.InternalFailure,
			grammar.Host.Location,
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

	/// <summary>
	/// Everything one host produced, as values: the file to add and the diagnostics to
	/// report.
	/// </summary>
	/// <remarks>
	/// A <c>Diagnostic</c> is built at delivery rather than carried, because building it
	/// needs a <c>DiagnosticDescriptor</c> and a <c>Location</c>, and what those compare
	/// as is not this file's business to depend on. The pieces are strings and numbers,
	/// which compare the way arithmetic does.
	/// </remarks>
	readonly record struct Parser(string? HintName, string? Text, EquatableArray<Report> Reports)
	{
		public void Deliver(SourceProductionContext context)
		{
			foreach (var report in Reports.Items)
				try
				{
					context.ReportDiagnostic(report.ToRoslyn());
				}
				catch (Exception exception) when (Recoverable(exception))
				{
					context.ReportDiagnostic(InternalDiagnostic(
						"delivering a grammar diagnostic", exception, report.Fallback));
				}

			if (HintName is not null && Text is not null)
				try
				{
					context.AddSource(HintName, Text);
				}
				catch (Exception exception) when (Recoverable(exception))
				{
					context.ReportDiagnostic(InternalDiagnostic(
						"adding generated C# to the compilation", exception, null));
				}
		}
	}

	static Diagnostic InternalDiagnostic(string stage, Exception exception, Location? location) =>
		Diagnostic.Create(
			Diagnostics.InternalFailure,
			location ?? Location.None,
			stage,
			exception.GetType().FullName ?? exception.GetType().Name,
			exception.Message);

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
		Declared                 Declares = default,
		EquatableArray<Piece>    Pieces   = default);

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
	readonly record struct Piece(
		int       Start,
		int       Length,
		string?   Path,
		string?   Literal,
		int       LiteralAt,
		Location? Location);

	/// <summary>
	/// Where a grammar declares a <c>context</c> and a <c>state</c>, or -1 for neither.
	/// </summary>
	/// <remarks>
	/// Kept as offsets rather than as the declarations themselves because this is a cache
	/// key: two ints and a length say everything the check needs and nothing that changes
	/// when something else in the file does.
	/// </remarks>
	readonly record struct Declared(int ContextAt, int ContextLength, int StateAt, int StateLength)
	{
		public static readonly Declared None = new(-1, 0, -1, 0);

		public bool Any => ContextAt >= 0 || StateAt >= 0;

		public static Declared Of(DotGram.Grammar.Parsing.GrammarFile? file)
		{
			if (file is null)
				return None;

			var declared = None;

			// The root and nowhere else, which is where both are allowed to stand at all
			// (GRAM3015 for `state`; a `context` in a namespace is that grammar's own
			// contract and is not the effective one) — so there is nothing to walk into.
			foreach (var declaration in file.Decls)
				if (declaration is DotGram.Grammar.Parsing.Decl.Context)
					declared = declared with
					{
						ContextAt     = declaration.At.Position,
						ContextLength = declaration.At.Length,
					};
				else if (declaration is DotGram.Grammar.Parsing.Decl.State)
					declared = declared with
					{
						StateAt     = declaration.At.Position,
						StateLength = declaration.At.Length,
					};

			return declared;
		}
	}

	/// <summary>
	/// Stage one: find the grammar and work out what it needs to know about the host's C#.
	/// No compilation, so this is cached on the grammar text and the host alone.
	/// </summary>
	static Grammar Asked(Host host, ImmutableArray<GrammarFile> files)
	{
		var reports = ImmutableArray.CreateBuilder<Report>();

		if (!host.IsPartial)
		{
			reports.Add(Report.Of(Diagnostics.HostNotPartial, host.Location, host.ClassName));

			return new Grammar(host, null, null, default, default, Values(reports), Declared.None);
		}

		// Said before the grammar is read, because it is about the host rather than about
		// the grammar, and because a name that cannot be a namespace would otherwise reach
		// the splice and come back as a parse error in a text nobody wrote.
		if (host.IncludedAs is { } included && !IsIdentifier(included))
			reports.Add(Report.Of(
				Diagnostics.InvalidIncludedName, host.Location, host.ClassName, included));

		if (!TryResolveGrammar(reports, host, files, out var own, out var path))
			return new Grammar(host, null, null, default, default, Values(reports), Declared.None);

		// What the host inherits, joined onto the end of its own — which is where it goes
		// so that the text somebody is editing keeps the offsets it always had
		// (GrammarSplice). A base whose grammar cannot be found is reported against the
		// class that declares it and left out; the rest still compiles, and what it was
		// going to provide comes back as ordinary undefined names.
		var parts   = new List<GrammarSplice.Part>();
		var bases   = new List<Included>();

		foreach (var inherited in host.Includes.Items)
			if (TryResolveGrammar(
				reports,
				inherited.Source,
				SimpleNameOf(inherited.ClassName),
				inherited.ClassName,
				inherited.Location,
				files,
				out var inheritedText,
				out var inheritedPath))
			{
				parts.Add(new GrammarSplice.Part(inheritedText, inherited.Name, null));
				bases.Add(inherited with { Source = inheritedPath });
			}

		var (text, joined) = GrammarSplice.Join(new GrammarSplice.Part(own, null, null), parts);

		var pieces = ImmutableArray.CreateBuilder<Piece>();

		pieces.Add(new Piece(
			0, own.Length, path, host.Literal, host.LiteralAt, host.Location));

		for (var at = 0; at < bases.Count; at++)
			pieces.Add(new Piece(
				joined.Segments[at + 1].Start,
				joined.Segments[at + 1].Length,

				// `Source` now holds the path it resolved to, or null where the grammar was
				// written into the attribute — the same two cases the host's own has.
				bases[at].Source,
				bases[at].Literal,
				bases[at].LiteralAt,
				bases[at].Location));

		// Parsed twice over a grammar's life: once here for the questions, once in the
		// third stage for the answer. Both are cheap next to normalization and emission,
		// and this one only re-runs when the grammar itself changes.
		var parsed = DotGram.Grammar.Parsing.GramParser.Parse(
			DotGram.Grammar.Parsing.GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File;

		return new Grammar(
			host,
			text,
			path,
			new EquatableArray<Question>(Questions.Of(parsed)),
			default,
			Values(reports),
			Declared.Of(parsed),
			new EquatableArray<Piece>(pieces.ToImmutable()));
	}

	/// <summary>
	/// Stage three: the grammar compiled against what the host answered. No compilation
	/// reaches here, so it runs only when the grammar or one of the answers changed.
	/// </summary>
	static Parser Compile(Grammar grammar)
	{
		if (grammar.Text is not { } text)
			return new Parser(null, null, grammar.Reports);

		var host    = grammar.Host;
		var reports = ImmutableArray.CreateBuilder<Report>();

		// What the first stage had to say, carried rather than dropped. Every report it
		// used to make came with an early return — no text, so the branch above hands them
		// on — and the first one that could stand beside a grammar that reads perfectly
		// well went silently missing until it was looked for.
		reports.AddRange(grammar.Reports.Items);

		var result = GramCompiler.Compile(text, new GramCompilerOptions
		{
			FileName       = grammar.Path ?? host.SimpleName + GramFileExtension,
			ClassName      = host.ClassName,
			Namespace      = host.Namespace,
			SymbolResolver = new AnsweredSymbolResolver(grammar.Answers.Items),
			CSharpScanner  = RoslynCSharpScanner.Instance,

			// §7.6. A grammar that is its own file maps onto itself; one written into an
			// attribute maps into the C# file holding it, which has to be searched for
			// rather than computed — see InlineLineMap. Where a host inherits grammars the
			// text is several of them joined, and the map is one per piece with the same
			// two cases inside it.
			LineMap        = MapOf(grammar, text),
		});

		foreach (var diagnostic in result.Diagnostics)
			reports.Add(PlacedIn(grammar, text, diagnostic));

		return new Parser(
			result.Sources.Count > 0 ? host.HintName + ".g.cs" : null,
			result.Sources.Count > 0 ? result.Sources[0].Text  : null,
			Values(reports));
	}

	/// <summary>Where each piece of the joined text belongs (§7.6).</summary>
	static ILineMap? MapOf(Grammar grammar, string text)
	{
		var pieces = grammar.Pieces.Items;

		if (pieces.Length == 0)
			return MapOfPiece(new Piece(0, text.Length, grammar.Path, grammar.Host.Literal,
				grammar.Host.LiteralAt, grammar.Host.Location), text);

		// One piece is the ordinary case and needs no splicing over it: a host inheriting
		// nothing compiles the map it always did.
		if (pieces.Length == 1)
			return MapOfPiece(pieces[0], text);

		return new SplicedLineMap(
		[
			.. pieces.Select(piece => new SplicedLineMap.Segment(
				piece.Start, piece.Length, MapOfPiece(piece, text))),
		]);
	}

	static ILineMap? MapOfPiece(Piece piece, string text)
	{
		var own = text.Substring(piece.Start, piece.Length);

		return piece.Path is { } path
			? new GrammarLineMap(own, path)
			: piece.Literal is { } spelling && piece.Location?.SourceTree is { } tree
				? new InlineLineMap(own, spelling, piece.LiteralAt, tree)
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
		var host   = grammar.Host;
		var pieces = grammar.Pieces.Items;

		foreach (var piece in pieces)
		{
			if (diagnostic.Position < piece.Start || diagnostic.Position > piece.Start + piece.Length)
				continue;

			return Report.Of(
				new GramDiagnostic(
					diagnostic.Id,
					diagnostic.Message,
					diagnostic.Position - piece.Start,
					diagnostic.Length,
					diagnostic.Severity),
				piece.Path,
				text.Substring(piece.Start, piece.Length),
				piece.Location ?? host.Location,
				piece.Literal,
				piece.LiteralAt);
		}

		return Report.Of(
			diagnostic, grammar.Path, text, host.Location, host.Literal, host.LiteralAt);
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

	static EquatableArray<Report> Values(ImmutableArray<Report>.Builder reports) =>
		new(reports.ToImmutable());

	/// <summary>
	/// Works out which grammar a host means: the text written into the attribute, an
	/// explicit path, or — with no argument at all — the file named after the class.
	/// </summary>
	static bool TryResolveGrammar(
		ImmutableArray<Report>.Builder reports,
		Host                           host,
		ImmutableArray<GrammarFile>    files,
		out string                     text,
		out string?                    path) =>
		TryResolveGrammar(
			reports, host.Source, host.SimpleName, host.ClassName, host.Location, files,
			out text, out path);

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
		Location?                      location,
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
					Diagnostics.GrammarFileNotFound, location, wanted, className));

				return false;

			default:
				// Picking one by reference order would make which file won invisible.
				reports.Add(Report.Of(
					Diagnostics.AmbiguousGrammarFile,
					location,
					wanted,
					string.Join(", ", found.Select(file => file.Path))));

				return false;
		}
	}

	static bool IsPath(string source) =>
		source.EndsWith(GramFileExtension, StringComparison.OrdinalIgnoreCase) &&
		source.IndexOf('\n') < 0 &&
		source.IndexOf('\r') < 0;

	/// <summary>
	/// A file matches a wanted path when it ends with it on a separator boundary — the
	/// attribute names a path relative to the project, and what reaches us is absolute.
	/// </summary>
	static bool Matches(string filePath, string wanted)
	{
		var normalized = wanted.Replace('/', '\\');

		if (!filePath.Replace('/', '\\').EndsWith(normalized, StringComparison.OrdinalIgnoreCase))
			return false;

		var boundary = filePath.Length - normalized.Length - 1;

		return boundary < 0 || filePath[boundary] is '\\' or '/';
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
	readonly record struct Included(
		string    Name,
		string    ClassName,
		string?   Source,
		string?   Literal,
		int       LiteralAt,
		Location? Location);

	/// <summary>
	/// A class marked <c>[Gram]</c>, reduced to what generation needs.
	/// </summary>
	/// <remarks>
	/// Deliberately values rather than symbols: an incremental generator compares what
	/// each step produced to decide whether the next one must run again, and a symbol
	/// compares equal to nothing across compilations.
	/// </remarks>
	/// <param name="Literal">
	/// The grammar exactly as the attribute spells it — quotes, escapes, indentation and
	/// all — or null when the grammar came from a file.
	/// </param>
	/// <param name="LiteralAt">
	/// Where that spelling begins in the C# file, so a position in the grammar can be put
	/// back where the author wrote it.
	/// </param>
	readonly record struct Host(
		string    ClassName,
		string?   Namespace,
		string    HintName,
		bool      IsPartial,
		string?   Source,
		Location? Location,
		string?   Literal    = null,
		int       LiteralAt  = 0,
		string?   IncludedAs = null,
		EquatableArray<Included> Includes = default)
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

		static string TypeParametersOf(ClassDeclarationSyntax declaration) =>
			declaration.TypeParameterList is { Parameters.Count: > 0 } parameters
				? "<" + string.Join(", ", parameters.Parameters.Select(static p => p.Identifier.ValueText)) + ">"
				: "";

		public static Host From(GeneratorAttributeSyntaxContext candidate)
		{
			var type        = (INamedTypeSymbol)candidate.TargetSymbol;
			var declaration = (ClassDeclarationSyntax)candidate.TargetNode;
			var attribute   = candidate.Attributes[0];

			var source = attribute.ConstructorArguments.Length == 1
				? attribute.ConstructorArguments[0].Value as string
				: null;

			var includedAs = attribute.NamedArguments
				.FirstOrDefault(static named => named.Key == nameof(Host.IncludedAs))
				.Value.Value as string;

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

			return new Host(
				ClassName: string.Join(".", names),
				Namespace: type.ContainingNamespace.IsGlobalNamespace
					? null
					: type.ContainingNamespace.ToDisplayString(),
				HintName:  type.ToDisplayString().Replace('<', '_').Replace('>', '_'),
				IsPartial: isPartial,
				Source:    source,
				Location:  attribute.ApplicationSyntaxReference is { } reference
					? Microsoft.CodeAnalysis.Location.Create(reference.SyntaxTree, reference.Span)
					: declaration.Identifier.GetLocation(),
				Literal:    written == default ? null : written.Text,
				LiteralAt:  written == default ? 0    : written.SpanStart,
				IncludedAs: includedAs,
				Includes:   new EquatableArray<Included>(Inherited(type)));
		}

		/// <summary>Every grammar up the base chain, nearest first.</summary>
		/// <remarks>
		/// <para>
		/// By display name and not by symbol: the attribute is emitted into every assembly
		/// separately and on purpose, so a base compiled elsewhere carries *its* assembly's
		/// <c>DotGram.GramAttribute</c> and the two types are not the same type. What they
		/// share is what they are called.
		/// </para>
		/// <para>
		/// A base with no grammar is walked past rather than stopping the walk: a class may
		/// sit between two that have one for reasons of its own.
		/// </para>
		/// <para>
		/// Cycles cannot happen — C# forbids a class from inheriting itself, directly or
		/// through anything — which is a property this spelling gets for free and a named
		/// import of grammars would not have.
		/// </para>
		/// </remarks>
		static ImmutableArray<Included> Inherited(INamedTypeSymbol type)
		{
			var included = ImmutableArray.CreateBuilder<Included>();

			for (var above = type.BaseType; above is not null; above = above.BaseType)
			{
				var attribute = above
					.GetAttributes()
					.FirstOrDefault(static candidate =>
						candidate.AttributeClass?.ToDisplayString() == GramAttribute);

				if (attribute is null)
					continue;

				var spelled = attribute.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax syntax &&
					syntax.ArgumentList?.Arguments.FirstOrDefault(
						static argument => argument.NameEquals is null) is
							{ Expression: LiteralExpressionSyntax literal }
						? literal.Token
						: default;

				var named = attribute.NamedArguments
					.FirstOrDefault(static argument => argument.Key == nameof(Host.IncludedAs))
					.Value.Value as string;

				included.Add(new Included(
					Name:      named ?? above.Name,
					ClassName: above.ToDisplayString(),
					Source:    attribute.ConstructorArguments.Length == 1
						? attribute.ConstructorArguments[0].Value as string
						: null,
					Literal:   spelled == default ? null : spelled.Text,
					LiteralAt: spelled == default ? 0    : spelled.SpanStart,

					// Null where the base is in a referenced assembly, which is what makes
					// a diagnostic in its grammar have nowhere to point (docs/next.md).
					Location:  attribute.ApplicationSyntaxReference is { } reference
						? Microsoft.CodeAnalysis.Location.Create(reference.SyntaxTree, reference.Span)
						: null));
			}

			return included.ToImmutable();
		}
	}
}
