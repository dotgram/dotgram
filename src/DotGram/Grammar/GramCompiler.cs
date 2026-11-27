using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar;

/// <summary>
/// Compiles grammar text into the source of a parser. This is the whole grammar half: everything
/// a caller needs, in one call, with no Roslyn involved.
/// </summary>
/// <remarks>
/// The source generator is a shell over this — it reads <c>.gram</c> files out of the
/// compilation, calls <see cref="Compile"/>, and hands the results back to Roslyn.
/// Anything else that wants to compile a grammar (a test, a CLI, a playground) calls
/// the same method and gets the same answer, which is the point: generated code can be
/// obtained as a string and inspected as code.
/// </remarks>
static partial class GramCompiler
{
	/// <summary>
	/// Reads one grammar: the first two stages, with the standard library spliced on.
	/// </summary>
	/// <remarks>
	/// Every grammar has the library, the way every C# compilation has the framework: it is
	/// spliced onto the end, so that nothing the author wrote moves, and what is not called
	/// is pruned before anything is emitted. What is written onto the class as its source
	/// stays the text as written — the includer's own compilation splices its own copy. One
	/// method so that whoever reads a grammar ahead of compiling it — the generator, for its
	/// questions — reads the same text.
	/// </remarks>
	/// <param name="language">The language that reads the grammar's host fragments.</param>
	/// <param name="grammarText">Contents of a <c>.gram</c> file.</param>
	/// <param name="own">Where the author's own text ends in <paramref name="grammarText"/>, where grammars were joined after it.</param>
	/// <param name="cuts">Where each further text of its own begins.</param>
	internal static ParseResult Read(ITargetLanguage language, string grammarText, int? own, IReadOnlyList<int>? cuts)
	{
		// The author's own text, the grammars the host included after it and the library are
		// three texts: what one leaves open is not closed by the next.
		return GramParser.Parse(GramLexer.TokenizeIn(
			language, StandardLibrary.SplicedOnto(grammarText), CutsOf(own, cuts), grammarText.Length,
			language.ForLibrary));
	}

	/// <summary>Where each text of its own begins in a joined one: the host's own cut, and the given ones.</summary>
	internal static IReadOnlyList<int>? CutsOf(int? own, IReadOnlyList<int>? cuts)
	{
		if (own is null)
			return cuts;

		List<int> all = [own.Value];

		if (cuts is not null)
			all.AddRange(cuts);

		return all;
	}

	/// <summary>Compiles one grammar into a parser written in a given language.</summary>
	/// <param name="language">
	/// The one language of the whole compilation: it reads the grammar's host fragments, is asked
	/// about its types by every stage below, and writes the parser.
	/// </param>
	/// <param name="grammarText">Contents of a <c>.gram</c> file.</param>
	/// <param name="options">Compilation options.</param>
	internal static GramCompilation Compile(ITargetLanguage language, string grammarText, GramCompilerOptions options)
	{
		if (language is null)
			throw new ArgumentNullException(nameof(language));

		if (grammarText is null)
			throw new ArgumentNullException(nameof(grammarText));

		if (options is null)
			throw new ArgumentNullException(nameof(options));

		// Settled copies only where every machine of the file is a reader over a string: an engine, a
		// machine over a buffer, from a position or over tokens records what a refusal expected from
		// how each place was printed, and a copy can be printed another way. What cannot be read so is
		// known before anything is compiled where the options say it; the rest is known from the plan,
		// and where it is not all readers the grammar is compiled again as though there were no copies,
		// so that the file is the one it would have been.
		if (options.SettleRunTails && !options.Lexical && options.Direct && !options.BufferedInput && !options.BufferedBytes)
		{
			var settled = Compile(language, grammarText, options, settle: true, told: null, out var dropped);

			if (dropped is null)
				return settled;

			return Compile(language, grammarText, options, settle: false, dropped, out _);
		}

		return Compile(language, grammarText, options, settle: false, told: null, out _);
	}

	/// <summary>
	/// The same, with settled copies or without; with them, nothing where a machine of the file is not a
	/// reader over a string, and <paramref name="dropped"/> what the author is told of the copies that
	/// were then dropped (<see cref="Dropped"/>), which the grammar compiled again without them says
	/// as <paramref name="told"/>.
	/// </summary>
	static GramCompilation Compile(
		ITargetLanguage language, string grammarText, GramCompilerOptions options, bool settle,
		IReadOnlyList<GramDiagnostic>? told, out IReadOnlyList<GramDiagnostic>? dropped)
	{
		dropped = null;

		var sources     = new List<GeneratedSource>();
		var diagnostics = new List<GramDiagnostic>();

		// The pipeline, each stage its own type with its own contract so it can be
		// exercised and diffed on its own:
		//
		//   GramLexer        .TokenizeIn  (text)        -> TokenList
		//   GramParser       .Parse       (tokens)      -> ParseResult
		//   GrammarBinder    .BindIn      (file)        -> GrammarModel
		//   GrammarNormalizer.NormalizeIn (model)       -> RecognitionGraph
		//   ITargetLanguage  .Emit      (graph)         -> the parser, by the language's emitter

		// Every stage reads a character class and a literal as ranges and runs of UTF-16 code units
		// (CharRange); a language whose input is made of anything else needs those stages first.
		if (language.TextUnit != TextUnit.Utf16CodeUnit)
			throw new NotSupportedException($"A parser over {language.TextUnit} is not generated: every stage reads UTF-16 code units.");

		var parsed = Read(language, grammarText, options.Own, options.Cuts);

		diagnostics.AddRange(parsed.Diagnostics);

		// Past the author's text is what was spliced on: what the host included, and after
		// that the standard library, which Read put there. Each is a root of its own.
		var model = GrammarBinder.BindIn(
			parsed.File, language, Math.Min(options.Own ?? int.MaxValue, grammarText.Length));

		options.Bound?.Invoke(model);

		var graph = GrammarNormalizer.NormalizeIn(
			model, language, options.PositionalFollow, options.PositionalFollowSplit, options.CollapseForwarders, settle);

		// What the publications ask for alone says that some machine will not be a reader over a string.
		var readersOnly = graph.Settled.Count > 0;

		if (readersOnly && Blocked(graph) is { } blocked)
		{
			dropped = Dropped(graph, blocked);

			return new GramCompilation([], []);
		}

		// What the later stages made of a declaration whose syntax did not come out is
		// about a tree that was guessed at, not about the grammar. Dropped rather than
		// reported, and only for the declarations that actually broke — a rule that is
		// wrong should still be told about when the one above it is unreadable
		// (docs/internal/implementation.md §0).
		var broken = Broken(parsed);

		foreach (var diagnostic in model.Diagnostics)
			if (!Inside(broken, diagnostic))
				diagnostics.Add(diagnostic);

		if (told is not null)
			foreach (var diagnostic in told)
				if (!Inside(broken, diagnostic))
					diagnostics.Add(diagnostic);

		foreach (var diagnostic in graph.Diagnostics)
			if (!Inside(broken, diagnostic))
				diagnostics.Add(diagnostic);

		// What a rule begins with goes round for ever on some grammars rather than settling — rings
		// of rules that lead with one another, which are refused for that already. Where nothing
		// else refused the grammar, this does: its answers were cut off, not worked out.
		if (!HasErrors(diagnostics) && FirstSets.Unsettled(graph) is { } unsettled)
			diagnostics.Add(new GramDiagnostic(
				GrammarNormalizer.FirstSetUnsettled,
				$"What '{unsettled.Name}' can begin with does not settle: working it out goes round without end. " +
				"This is a defect of the compiler's; the grammar is refused rather than compiled from an unfinished answer.",
				unsettled.Declaration?.At.Position ?? 0,
				unsettled.Declaration?.At.Length ?? 0,
				GramSeverity.Error));

		// What the grammar asked for and cannot have, said where the asking is (§6.3).
		// After normalization because it is a question about the graph, and only when the
		// grammar is otherwise sound: telling an author what a broken rule will not get is
		// answering a question they are not asking yet.
		if (!HasErrors(diagnostics))
		{
			foreach (var diagnostic in graph.WhenSound)
				if (!Inside(broken, diagnostic))
					diagnostics.Add(diagnostic);

			// An extent-only byte publication returns byte[], while its string form
			// returns string. The explicit contract must accept both input domains.
			foreach (var publication in graph.Publications)
				if (publication.Kind != PublishKind.Yield && publication.ResultType is { } contract &&
					(options.BufferedBytes || publication.BufferedBytes) &&
					!graph.Types.ContainsKey(publication.Rule) && graph.Results[publication.Rule].Count == 0 &&
					!GrammarNormalizer.PublicationFits(
						language, language.SequenceOf(language.Spelling(KnownType.Byte)), GrammarNormalizer.TypeName(contract, language),
						publication.DeclaredIn))
					diagnostics.Add(new GramDiagnostic(GrammarNormalizer.PublicationTypeMismatch,
						"The byte input form returns byte[], which is not assignable to the publication type.",
						contract.At.Position, contract.At.Length, GramSeverity.Error));

			diagnostics.AddRange(Retention.Check(graph));
			diagnostics.AddRange(FirstSets.Check(graph));

			// What the emitter would write twice into the class, which only the author can rename.
			diagnostics.AddRange(language.NameClashes(graph, options));
		}

		// Every stage runs even after an earlier one failed — a grammar with one bad rule
		// should still report what is wrong with the other twelve (docs/internal/implementation.md §0).
		// Only emission is skipped, because code built from a broken grammar would bury
		// the real message under compiler errors in the consumer's build.
		// The grammar cut in two, where the host asked and the grammar allows it. Asked
		// after the diagnostics because it is a question about a sound grammar, and answered
		// with the character machine where it cannot be: that machine is correct and right
		// there, so a refusal is worth a word rather than a failed build.
		if (options.Lexical && graph.Segmented && GrammarNormalizer.EndOfSegmentAt(model) is { } named)
			diagnostics.Add(new GramDiagnostic(
				GrammarNormalizer.LexicalSegments,
				"A grammar that names 'eos' is read over characters: a token cannot reach over the end of a " +
				"segment, and the end of a segment is not a kind of token. Leave Lexical off for it " +
				"(docs/syntax.md §3.1.1).",
				named.Position, named.Length, GramSeverity.Error));

		if ((options.BufferedInput || options.BufferedBytes ||
			 graph.Publications.Any(static publication => publication.BufferedInput || publication.BufferedBytes)) &&
			graph.Segmented && GrammarNormalizer.EndOfSegmentAt(model) is { } segmented)
			diagnostics.Add(new GramDiagnostic(
				GrammarNormalizer.BufferedSegments,
				"A grammar that names 'eos' reads a sequence of segments: its publications take a list, a " +
				"sequence or a reader of lines, and the buffered and byte forms read neither. Leave them off " +
				"for it (docs/syntax.md §6.3).",
				segmented.Position, segmented.Length, GramSeverity.Error));

		var lexical = options.Lexical && !HasErrors(diagnostics) ? Cut(graph, diagnostics, language) : null;

		// A rule that can come back to itself through a recognizer that does not move, said of the graph
		// over characters: in a grammar cut in two its terminals are read over them by the tokenizer, and
		// its syntactic half calls no recognizer (GRAM5004 keeps a cut from one).
		if (!HasErrors(diagnostics))
			diagnostics.AddRange(Reentry.Check(graph));

		// Trivia that gives back characters a class beside it also reads: exponential on a refused
		// input. Over characters only — a grammar cut in two reads its trivia with a scanner, which
		// never gives back what it took.
		if (lexical is null && !HasErrors(diagnostics))
			diagnostics.AddRange(TriviaOverlap.Check(graph));

		// A repetition that can read one run of text in two ways: a refused input is read again
		// for every way the run can be cut. Over characters, and in a grammar cut in two over the
		// kinds of the rules that give back what they read.
		if (!HasErrors(diagnostics))
			diagnostics.AddRange(ExponentialRepetition.Report(graph, lexical));

		// An ordered choice that reads a nested instance of its own recursion before it can be refused:
		// a nest of them is read once per alternative at every level. What the analysis finds decides
		// which rules' successes the readers remember (Machine.Memo.cs), and the emitter says of each
		// choice whether they do (GRAM4054).
		var shared = options.AnalyseSharedRecursion && !HasErrors(diagnostics) ? SharedRecursion.Check(graph, lexical).Found : null;

		// And the one question that has a different answer on the other side of the cut:
		// over characters an overlap between an optional and what follows it is settled by
		// backtracking, and over kinds a reading that fits is the one that stands.
		if (lexical is not null && !HasErrors(diagnostics))
			diagnostics.AddRange(FirstSets.Committed(lexical.Syntax, lexical.Inventory));

		// A terminal that is nothing but `@M` has no beginning the automaton could stop on, so
		// the lexer asks the host at the start of every token. Correct and a call a token; the
		// beginning written in front of it is what makes it one call a terminal.
		if (lexical is not null)
			foreach (var (pattern, _) in lexical.Inventory.Externals)
				diagnostics.Add(new GramDiagnostic(
					Unanchored,
					$"'{pattern.Rule.Name}' is nothing but '@{pattern.Method}', so the lexer asks " +
					$"'{pattern.Method}' at the start of every token. Write what it begins with in front " +
					$"of it — '{pattern.Rule.Name} = \"…\" & @{pattern.Method}' — and it is asked only " +
					"where that stands.",
					pattern.Rule.Declaration?.At.Position ?? 0,
					pattern.Rule.Declaration?.At.Length ?? 0,
					GramSeverity.Warning));

		var sourceParts = new List<string>();
		var carriers    = options.ReportCarriers ? new List<string>() : null;
		var context     = new EmitContext
		{
			Lexical       = lexical,
			Diagnostics   = diagnostics,
			GrammarSource = options.Portable ? grammarText : null,
			SourceParts   = sourceParts,
			Carriers      = carriers,
			SharedChoices = shared,
			ReadersOnly   = readersOnly,
		};

		// Nothing where the plan refused what the grammar asks of the target: that is said, and no
		// parser is written.
		if (!HasErrors(diagnostics) && language.Emit(lexical?.Syntax ?? graph, options, context) is { } text)
			sources.Add(new GeneratedSource(language.SourceName(options, 0), text));

		if (context.LeftTheReaders)
		{
			dropped = Dropped(graph, context.Blocked);

			return new GramCompilation([], []);
		}

		for (var part = 0; part < sourceParts.Count; part++)
			sources.Add(new GeneratedSource(language.SourceName(options, part + 1), sourceParts[part]));

		return new GramCompilation(sources, OnePerPosition(diagnostics))
		{
			NormalizedRuleCount = graph.Rules.Count,
			UsesLexical = lexical is not null,
			SharedNeeds = context.SharedNeeds,
			Carriers = carriers,
			Plan = context.Plan,
			Printed = context.Printed,
			Graph = graph,
			Names = sources.Count == 0 ? null : language.NamesOf(
				lexical?.Syntax ?? graph, lexical is null ? [] : [graph, lexical.Source], options),
		};
	}

	/// <summary>
	/// What puts a file beyond the readers before anything is planned: a grammar that reads segments, or
	/// the first publication, in the order written, that asks for what no reader over a string can give.
	/// Null where the plan has to say.
	/// </summary>
	static SettleBlock? Blocked(RecognitionGraph graph)
	{
		if (graph.Segmented)
			return new SettleBlock(SettleCause.Segments, null);

		foreach (var publication in graph.Publications)
			if (SettleBlock.Asked(publication) is { } asked)
				return new SettleBlock(asked, publication);

		return null;
	}

	/// <summary>
	/// What the author is told of the settled copies a file is compiled without, because a machine of it
	/// is not a reader over a string: one message for each rule whose copy would have settled a run with
	/// no upper bound, at the rule, with the cause (<see cref="SettleBlock"/>) and what can be done about it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Without the copy, a refusal inside a long run of the rule is read again for every place the run
	/// can be cut, as many as the run has characters, and each of them reads the rest of the run:
	/// time quadratic in its length. A run with an upper bound costs no more than the bound, so it is
	/// not told. The publication named is the first, in the order written, of the cause; the remedy is
	/// only one that can be done: a <c>find</c> is always read by the engine, so it is moved; a grammar
	/// that reads segments has no publication to blame and nothing to move, so it is only informed
	/// (<see cref="SettledDroppedSegments"/>). Nothing is said where no publication is of a cause.
	/// </para>
	/// </remarks>
	static IReadOnlyList<GramDiagnostic> Dropped(RecognitionGraph graph, SettleBlock? block)
	{
		if (block is null)
			return [];

		var by   = block.By?.MethodName;
		var move = "Move it into its own grammar.";
		var told = new List<GramDiagnostic>();

		var (reason, remedy) = block.Cause switch
		{
			SettleCause.Engine   => ($"publication '{by}' runs on the engine", $"Read '{by}' by methods, or move it into its own grammar."),
			SettleCause.Find     => ($"publication '{by}' runs on the engine", move),
			SettleCause.Window   => ($"publication '{by}' is also read a window at a time, on the engine",
				$"Give '{block.By!.Rule.Name}' a result type, or move it into its own grammar."),
			SettleCause.Position => ($"publication '{by}' is read from a position", move),
			SettleCause.Stream   => ($"publication '{by}' reads a stream", move),
			SettleCause.Flat     => ($"publication '{by}' is compiled flat", move),
			_                    => ("the grammar reads segments ('eos')", ""),
		};

		foreach (var rule in graph.SettledRuns)
		{
			var text = $"Rule '{rule.Name}' is read without its settled copy because {reason}, " +
				$"so a refusal inside a long run of '{rule.Name}' takes time quadratic in its length." +
				(remedy.Length == 0 ? "" : " " + remedy);
			var at   = rule.Declaration?.At ?? default;

			told.Add(block.Cause == SettleCause.Segments
				? new GramDiagnostic(SettledDroppedSegments, text, at.Position, at.Length, GramSeverity.Info)
				: new GramDiagnostic(SettledDropped, text, at.Position, at.Length, GramSeverity.Warning));
		}

		return told;
	}

	/// <summary>A grammar cut in two, or null with a word about why it was not.</summary>
	/// <remarks>
	/// <para>
	/// Four things stop it and each is said in its own words. A grammar with no trivia is
	/// already where it belongs — a URL is characters — and asking for a split there is a
	/// misunderstanding rather than a fault. A terminal that is not a regular language cannot
	/// be read together with the others. A <c>find</c> hunts through characters for a place
	/// to begin, which is not a thing to do to a stream of tokens. And <c>trivia</c> not
	/// written in braces is not a scanner, and the seam between tokens is skipped by one.
	/// </para>
	/// <para>
	/// Information and not a warning: the parser that comes out is the one that would have
	/// come out anyway, and nothing the author did is wrong.
	/// </para>
	/// </remarks>
	static LexicalSplit? Cut(RecognitionGraph graph, ICollection<GramDiagnostic> diagnostics, ITargetLanguage language)
	{
		// A warning and not a remark: `Lexical = true` was asked for, and what comes out
		// instead is a parser the author did not ask for — one that reads the same grammar
		// over characters, where a lookahead after a seam sees the next token's first
		// character rather than nothing at all. SQL92 was read that way for a while, and
		// took `t INNER JOIN u` for an alias, because the fallback was an Info that no
		// build shows.
		void Say(string why)
		{
			diagnostics.Add(new GramDiagnostic(
				NotCut,
				"This grammar is compiled over characters rather than tokens: " + why + ". " +
				"The parser is the one it would have been without the request.",
				0,
				0,
				GramSeverity.Warning));
		}

		if (graph.Publications.Any(one => one.Kind == PublishKind.Yield))
		{
			Say("Yield publications currently use character recognition rather than token-kind input.");
			return null;
		}

		if (graph.Publications.Any(one => one.Kind == PublishKind.Find))
		{
			Say("`find` hunts through characters for a place to begin, and a stream of tokens " +
				"has no such places");

			return null;
		}

		// Errors and not a reason: a grammar asked to be read over tokens is not quietly read over
		// characters because a binding stands in it (TerminalInventory.BoundTextRefusals).
		if (TerminalInventory.BoundTextRefusals(graph) is { Count: > 0 } refusals)
		{
			foreach (var refusal in refusals)
				diagnostics.Add(refusal);

			return null;
		}

		var split = LexicalSplit.Of(graph);

		if (split is null)
		{
			var inventory = TerminalInventory.Of(graph);

			Say(inventory.Applies
				? "its terminals cannot all be read together — " +
					string.Join("; ", inventory.Blocked.Take(3))
				: "no rule carries trivia, so there is nothing to tell a token from a character");

			return null;
		}

		if (split.Blocked.Count > 0)
		{
			Say(string.Join("; ", split.Blocked.Distinct().Take(3)));

			return null;
		}

		if (!split.Trivia.Any(rule => graph.Bodies.TryGetValue(rule, out var body) && body is Node.Atomic))
		{
			Say("`trivia` is not written in braces, and the seam between tokens is skipped by " +
				"the scanner braces ask for (§4.5)");

			return null;
		}

		// Braces ask for a scanner and do not make one. The tokenizer skips the seam in one
		// call, and trivia no scanner can read — a choice whose ways cannot be told apart by
		// where they begin — was skipped by nothing at all: every space reached the syntax as
		// input no token reads, and the parser refused what it was written to read.
		if (!language.ScansSeam(split))
		{
			Say("`trivia` cannot be read by committing to its first reading, and the seam " +
				"between tokens is skipped by a scanner that does (§4.5). A rule in it that reaches " +
				"itself is one a scanner cannot read; a comment that nests is written " +
				"`Std.Nested(open, close)`, which it can");

			return null;
		}

		// Errors and not a reason, as for a binding above: what the lexer reads is a token, the
		// longest match of a regular language, so a repetition in it marked `recover` would be read
		// as a plain one and the grammar would mean something else than it says.
		if (LexedRecoveries(graph, split) is { Count: > 0 } lexed)
		{
			foreach (var refusal in lexed)
				diagnostics.Add(refusal);

			return null;
		}

		return split;
	}

	/// <summary>
	/// Every repetition marked <c>recover</c> in a rule the lexer of this split reads — a terminal,
	/// what a terminal is made of, or what trivia is — each where it was written.
	/// </summary>
	static List<GramDiagnostic> LexedRecoveries(RecognitionGraph graph, LexicalSplit split)
	{
		var refused = new List<GramDiagnostic>();

		if (graph.Recoveries.Count == 0)
			return refused;

		var lexed = new HashSet<RuleSymbol>();

		foreach (var pattern in split.Inventory.Patterns)
			if (pattern is TerminalInventory.Pattern.Class(_, var terminal))
				lexed.UnionWith(graph.Reaches(terminal));

		foreach (var trivia in split.Trivia)
			lexed.UnionWith(graph.Reaches(trivia));

		var said = new HashSet<(int, int)>();

		foreach (var rule in graph.Rules)
			if (lexed.Contains(rule) && graph.Bodies.TryGetValue(rule, out var body))
				Look(body, rule);

		return refused;

		void Look(Node node, RuleSymbol rule)
		{
			foreach (var one in NodeWalk.Descendants(node))
			{
				if (!graph.Recoveries.TryGetValue(one, out var recovery))
					continue;

				if (said.Add((recovery.At.Position, recovery.At.Length)))
					refused.Add(new GramDiagnostic(
						GrammarNormalizer.RecoveryInToken,
						$"'recover' in '{graph.Origin(rule).Name}' would be read as a plain repetition: the grammar is cut in two " +
						"(Lexical = true) and the lexer reads this rule as part of a token, the longest match of a " +
						"regular language, which has no element in it to step over. Recover in the syntax that " +
						"reads the tokens, or read this grammar over characters: leave Lexical off (docs/syntax.md §8.2).",
						recovery.At.Position, recovery.At.Length, GramSeverity.Error));

				Look(recovery.Sync, rule);
			}
		}
	}

	/// <summary>The grammar was sound and still could not be cut in two.</summary>
	public const string NotCut = "GRAM5004";

	/// <summary>A rule ending in a run with no upper bound is read without its settled copy, because a publication is not read by methods over a string.</summary>
	public const string SettledDropped = "GRAM5022";

	/// <summary>The same, said as information, of a grammar that reads segments: nothing is to be moved or changed.</summary>
	public const string SettledDroppedSegments = "GRAM5023";

	/// <summary>A repetition can read one run of text in two ways, so a refused input is read again for every way of cutting it.</summary>
	public const string ExponentialRepetitions = "GRAM4052";

	/// <summary>Two alternatives of a rule read a nested instance of its recursion before either can be refused; the parser remembers the recursion's successes, and the rewrite that needs none is shown.</summary>
	public const string SharedRecursions = "GRAM4054";

	/// <summary>The immediate carrier, asked for by name, could not carry a machine of the grammar.</summary>
	public const string CarrierRefused = "GRAM5007";

	/// <summary>An explicitly requested buffered input form is not supported.</summary>
	public const string BufferedUnsupported = "GRAM4026";

	/// <summary>
	/// A grammar left to choose is carried as Immediate, and the tape would hold back
	/// constructions that run for a reading the parse then gives up: information.
	/// </summary>
	public const string CarrierChosen = "GRAM5012";

	/// <summary>A buffered form read by the engine where its string form is read by methods, and why.</summary>
	public const string BufferedOnEngine = "GRAM5014";

	/// <summary>
	/// A grammar left to choose is carried as Immediate, and the tape would be safer: constructions
	/// share <c>context</c> with hooks that run during recognition, or a refused input rebuilds lists.
	/// </summary>
	public const string CarrierCaution = "GRAM5016";

	/// <summary>
	/// A construction of a publication that is read from a reader or a sequence of lines changes the
	/// grammar's context, which a streamed reading may do a different number of times than a string.
	/// </summary>
	public const string StreamedContextChange = "GRAM5017";

	/// <summary>A terminal the host measures has nothing in front of it to be found by.</summary>
	public const string Unanchored = "GRAM5011";

	/// <summary>
	/// A terminal of a grammar read over tokens whose value is built by reading its token again holds a guard that
	/// names the context, which then runs again against the context as the parse has written it by then.
	/// </summary>
	public const string RereadContextGuard = "GRAM5019";

	/// <summary>
	/// One error per position, in the order they were raised.
	/// </summary>
	/// <remarks>
	/// The second thing said about a place is the first stage's failure told again by the
	/// next one — <c>Unexpected character '~'</c> and then <c>Expected a rule, a namespace
	/// or a publication directive</c>, both pointing at the same character. The earlier one
	/// is the one that says what actually happened; the later one describes the wreckage.
	/// Warnings and information are left alone: they are about the grammar rather than
	/// about what went wrong reading it, and two of them in one place can both be true.
	/// </remarks>
	static List<GramDiagnostic> OnePerPosition(List<GramDiagnostic> diagnostics)
	{
		var kept = new List<GramDiagnostic>(diagnostics.Count);
		var seen = new HashSet<int>();

		foreach (var diagnostic in diagnostics)
			if (diagnostic.Severity != GramSeverity.Error || seen.Add(diagnostic.Position))
				kept.Add(diagnostic);

		return kept;
	}

	/// <summary>
	/// The extent of every declaration the parser could not read whole.
	/// </summary>
	/// <remarks>
	/// A syntax error leaves the tree holding a guess, and everything a later stage says
	/// about that guess is about the guess. The declaration is the unit because it is the
	/// one the parser recovers at: what is around it was read normally and still deserves
	/// to be checked.
	/// </remarks>
	static List<(int From, int To)> Broken(ParseResult parsed)
	{
		var starts = new List<int>();

		foreach (var declaration in parsed.File.Decls)
			starts.Add(declaration.At.Position);

		starts.Sort();

		var extents = new List<(int, int)>();

		foreach (var diagnostic in parsed.Diagnostics)
		{
			if (diagnostic.Severity != GramSeverity.Error)
				continue;

			// From the start of the declaration the error is in, because a later stage
			// reports against the declaration rather than against the character; and up to
			// the next one, because that is where the parser found its feet again. A
			// declaration whose own start was never read leaves the extent open at the
			// front, which is the safe direction.
			var from = 0;
			var to   = int.MaxValue;

			foreach (var start in starts)
				if (start <= diagnostic.Position)
					from = start;
				else
				{
					to = start;

					break;
				}

			extents.Add((from, to));
		}

		return extents;
	}

	static bool Inside(List<(int From, int To)> extents, GramDiagnostic diagnostic)
	{
		foreach (var (from, to) in extents)
			if (from <= diagnostic.Position && diagnostic.Position < to)
				return true;

		return false;
	}

	static bool HasErrors(List<GramDiagnostic> diagnostics)
	{
		foreach (var diagnostic in diagnostics)
			if (diagnostic.Severity == GramSeverity.Error)
				return true;

		return false;
	}
}
