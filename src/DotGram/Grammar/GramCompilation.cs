using System;
using System.Collections.Generic;

using DotGram.Grammar.Plan;

namespace DotGram.Grammar;

/// <summary>What a compilation produced: sources to add, and what went wrong.</summary>
sealed class GramCompilation(IReadOnlyList<GeneratedSource> sources, IReadOnlyList<GramDiagnostic> diagnostics)
{
	public IReadOnlyList<GeneratedSource> Sources     { get; } = sources;
	public IReadOnlyList<GramDiagnostic>  Diagnostics { get; } = diagnostics;

	/// <summary>Number of normalized rules, including specialized and library rules.</summary>
	public int NormalizedRuleCount { get; init; }

	/// <summary>Whether recognition uses the lexical split after fallback checks.</summary>
	public bool UsesLexical { get; init; }

	/// <summary>
	/// What the carrier's choice rested on, line by line, where <see cref="GramCompilerOptions.ReportCarriers"/>
	/// asked for it; null otherwise.
	/// </summary>
	public IReadOnlyList<string>? Carriers { get; init; }

	/// <summary>
	/// The author's names the generated class is written under, where a source was written: what a name
	/// the generator makes is made from, and what it is not. Read by the tests that hold the generated
	/// names apart; null where nothing was emitted.
	/// </summary>
	public GramNames? Names { get; init; }

	/// <summary>
	/// Where the host has several readings (<see cref="GramCompilerOptions.SharedTypes"/>), the types
	/// this reading reads from the host and so needs written there; nothing otherwise.
	/// </summary>
	public SharedNeeds SharedNeeds { get; init; }

	/// <summary>
	/// What was decided about the file before any of it was printed (<see cref="ReadingPlan"/>), where
	/// the grammar was sound enough to be planned; null otherwise.
	/// </summary>
	public ReadingPlan? Plan { get; init; }

	/// <summary>Whether the plan was printed (<see cref="EmitContext.Printed"/>): false where it refused what was asked.</summary>
	public bool Printed { get; init; }

	public bool HasErrors
	{
		get
		{
			for (var i = 0; i < Diagnostics.Count; i++)
				if (Diagnostics[i].Severity == GramSeverity.Error)
					return true;

			return false;
		}
	}
}

/// <summary>One file of generated C#.</summary>
readonly record struct GeneratedSource(string HintName, string Text);

/// <summary>
/// The author's names a generated class is written under (<see cref="GramCompilation.Names"/>): the
/// identifiers of the rules and the names of the publications, which the generator's own names are
/// made from; the methods the publications are written as; and the types of the rules.
/// </summary>
sealed record GramNames(IReadOnlyList<string> Identifiers, IReadOnlyList<string> Publications, IReadOnlyList<string> Types);

/// <summary>
/// The types a host with several readings has written once, in the host, for all of them: what one
/// reading needs of them. The host gets the union of its readings' (CSharpEmitter.SharedTypes).
/// </summary>
[Flags]
enum SharedNeeds
{
	None = 0,

	/// <summary><c>SourceSpan</c>, which the grammar's C# or a rule's value names.</summary>
	Span = 1,

	/// <summary><c>SourceSpan</c> as a public signature names it, and so public.</summary>
	SpanShown = 2,

	/// <summary><c>Outcome</c> and <c>Match&lt;T&gt;</c>, which a publication answers with.</summary>
	Match = 4,

	/// <summary>The match of a reading that locates per call, which says where its reading ended.</summary>
	ReadingEnd = 8,

	/// <summary><c>ParserInput&lt;T&gt;</c>, which a buffered reader or a recognizer of the host reads.</summary>
	Input = 16,

	/// <summary>A reading of a host that inherits a grammar, whose types hide the base's (CS0108).</summary>
	Hides = 32,
}

/// <summary>
/// A compiler message, positioned in the grammar text.
/// </summary>
/// <remarks>
/// <para>
/// Deliberately not Roslyn's <c>Diagnostic</c>: the grammar half must stay callable
/// without Roslyn, so the shell converts these on the way out.
/// </para>
/// <para>
/// The severity is <see cref="DiagnosticCatalog"/>'s wherever the identifier is listed there, which
/// a test holds every call site to; the one passed in stands only for an identifier the catalog
/// does not know.
/// </para>
/// </remarks>
sealed record GramDiagnostic(string Id, string Message, int Position, int Length, GramSeverity Severity)
{
	/// <summary>The severity the identifier is reported with (<see cref="DiagnosticCatalog"/>).</summary>
	public GramSeverity Severity { get; init; } = DiagnosticCatalog.SeverityOf(Id, Severity);

	public override string ToString()
	{
		return $"{Id} at {Position}..{Position + Length}: {Message}";
	}
}

public enum GramSeverity
{
	/// <summary>
	/// Something worth knowing about a grammar that is perfectly correct — what it did
	/// not get, and why. Never a reason to change anything.
	/// </summary>
	Info,

	Warning,
	Error,
}

/// <summary>Options for one compilation.</summary>
/// <remarks>
/// Objects of their own — <see cref="Grammar"/>, <see cref="Author"/>, <see cref="Target"/> and, beside them, the shape of the
/// host the target language is generated into, which is that language's part of this class — and a
/// property of this one for each of their properties, so that every caller — the generator, the tool,
/// the editors and the tests — sets an option where it always has.
/// </remarks>
sealed partial class GramCompilerOptions
{
	/// <summary>What is asked about the grammar itself.</summary>
	public GrammarOptions Grammar { get; } = new();

	/// <summary>What the grammar's author asks of the generated parser.</summary>
	public AuthorOptions Author { get; } = new();

	/// <summary>How the generated parser is tuned.</summary>
	public TargetTuning Target { get; } = new();

	/// <inheritdoc cref="GrammarOptions.FileName"/>
	public string FileName { get => Grammar.FileName; set => Grammar.FileName = value; }

	/// <inheritdoc cref="GrammarOptions.LanguageId"/>
	public string? LanguageId { get => Grammar.LanguageId; set => Grammar.LanguageId = value; }

	/// <inheritdoc cref="GrammarOptions.LanguageSource"/>
	public string? LanguageSource { get => Grammar.LanguageSource; set => Grammar.LanguageSource = value; }

	/// <inheritdoc cref="GrammarOptions.LanguageClassifications"/>
	public string? LanguageClassifications { get => Grammar.LanguageClassifications; set => Grammar.LanguageClassifications = value; }

	/// <inheritdoc cref="GrammarOptions.LanguageRecognitionContract"/>
	public string? LanguageRecognitionContract { get => Grammar.LanguageRecognitionContract; set => Grammar.LanguageRecognitionContract = value; }

	/// <summary>
	/// The language the parser is generated in: how the types a grammar names are spelled, and what
	/// reads the host fragments it hands over. Null leaves the choice to the compiler's caller: C#'s,
	/// made of the resolver and the scanner the options hold, which is what every caller does today.
	/// </summary>
	/// <remarks>
	/// A C# language given here generates into the options' host shape, whatever shape it was made
	/// with: the class, its readings and the location type are the compilation's options, and win as
	/// they always have.
	/// </remarks>
	public ITargetLanguage? Language { get; set; }

	/// <inheritdoc cref="TargetTuning.PartSize"/>
	public int? PartSize { get => Target.PartSize; set => Target.PartSize = value; }

	/// <inheritdoc cref="GrammarOptions.Lexical"/>
	public bool Lexical { get => Grammar.Lexical; set => Grammar.Lexical = value; }

	/// <inheritdoc cref="TargetTuning.PrefixTables"/>
	public bool PrefixTables { get => Target.PrefixTables; set => Target.PrefixTables = value; }

	/// <inheritdoc cref="TargetTuning.WordTables"/>
	public bool WordTables { get => Target.WordTables; set => Target.WordTables = value; }

	/// <inheritdoc cref="TargetTuning.SourceFileSize"/>
	public int SourceFileSize { get => Target.SourceFileSize; set => Target.SourceFileSize = value; }

	/// <inheritdoc cref="TargetTuning.Direct"/>
	public bool Direct { get => Target.Direct; set => Target.Direct = value; }

	/// <inheritdoc cref="AuthorOptions.Portable"/>
	public bool Portable { get => Author.Portable; set => Author.Portable = value; }

	/// <inheritdoc cref="AuthorOptions.Carrier"/>
	public CarrierKind Carrier { get => Author.Carrier; set => Author.Carrier = value; }

	/// <inheritdoc cref="AuthorOptions.ConstructionsMayThrow"/>
	public bool ConstructionsMayThrow { get => Author.ConstructionsMayThrow; set => Author.ConstructionsMayThrow = value; }

	/// <inheritdoc cref="AuthorOptions.ReportCarriers"/>
	public bool ReportCarriers { get => Author.ReportCarriers; set => Author.ReportCarriers = value; }

	/// <inheritdoc cref="AuthorOptions.CountRules"/>
	public bool CountRules { get => Author.CountRules; set => Author.CountRules = value; }

	/// <inheritdoc cref="AuthorOptions.Checks"/>
	public bool Checks { get => Author.Checks; set => Author.Checks = value; }

	/// <inheritdoc cref="AuthorOptions.Steps"/>
	public bool Steps { get => Author.Steps; set => Author.Steps = value; }

	/// <inheritdoc cref="AuthorOptions.Trace"/>
	public bool Trace { get => Author.Trace; set => Author.Trace = value; }

	/// <inheritdoc cref="GrammarOptions.CollapseForwarders"/>
	public bool CollapseForwarders { get => Grammar.CollapseForwarders; set => Grammar.CollapseForwarders = value; }

	/// <inheritdoc cref="AuthorOptions.MemoiseFailures"/>
	public bool MemoiseFailures { get => Author.MemoiseFailures; set => Author.MemoiseFailures = value; }

	/// <inheritdoc cref="AuthorOptions.MemoiseSuccesses"/>
	public bool MemoiseSuccesses { get => Author.MemoiseSuccesses; set => Author.MemoiseSuccesses = value; }

	/// <inheritdoc cref="GrammarOptions.AnalyseSharedRecursion"/>
	public bool AnalyseSharedRecursion { get => Grammar.AnalyseSharedRecursion; set => Grammar.AnalyseSharedRecursion = value; }

	/// <inheritdoc cref="AuthorOptions.Utf8Literals"/>
	public bool Utf8Literals { get => Author.Utf8Literals; set => Author.Utf8Literals = value; }

	/// <inheritdoc cref="GrammarOptions.PositionalFollow"/>
	public bool PositionalFollow { get => Grammar.PositionalFollow; set => Grammar.PositionalFollow = value; }

	/// <inheritdoc cref="GrammarOptions.PositionalFollowSplit"/>
	public bool PositionalFollowSplit { get => Grammar.PositionalFollowSplit; set => Grammar.PositionalFollowSplit = value; }

	/// <inheritdoc cref="AuthorOptions.ValueStorage"/>
	public ValueStorageKind ValueStorage { get => Author.ValueStorage; set => Author.ValueStorage = value; }

	/// <inheritdoc cref="AuthorOptions.BufferedInput"/>
	public bool BufferedInput { get => Author.BufferedInput; set => Author.BufferedInput = value; }

	/// <inheritdoc cref="AuthorOptions.BufferedBytes"/>
	public bool BufferedBytes { get => Author.BufferedBytes; set => Author.BufferedBytes = value; }

	/// <inheritdoc cref="AuthorOptions.SpanCaptures"/>
	public bool SpanCaptures { get => Author.SpanCaptures; set => Author.SpanCaptures = value; }

	/// <inheritdoc cref="AuthorOptions.MaxRetained"/>
	public int MaxRetained { get => Author.MaxRetained; set => Author.MaxRetained = value; }

	/// <inheritdoc cref="AuthorOptions.BufferSize"/>
	public int BufferSize { get => Author.BufferSize; set => Author.BufferSize = value; }

	/// <inheritdoc cref="AuthorOptions.Stacks"/>
	public int Stacks { get => Author.Stacks; set => Author.Stacks = value; }

	/// <inheritdoc cref="GrammarOptions.Own"/>
	public int? Own { get => Grammar.Own; set => Grammar.Own = value; }

	/// <inheritdoc cref="GrammarOptions.Cuts"/>
	public IReadOnlyList<int>? Cuts { get => Grammar.Cuts; set => Grammar.Cuts = value; }

	/// <summary>
	/// Handed the bound grammar once names are resolved, for a caller that asks where each
	/// name went — the editor, which navigates by it. Null for the generator, which keeps
	/// nothing of the model past the compile.
	/// </summary>
	internal Action<Binding.GrammarModel>? Bound { get; set; }
}

/// <summary>
/// What one compilation is asked about the grammar itself: which text, how it is read and analysed.
/// </summary>
/// <remarks>
/// Neutral: none of it depends on the language the parser is generated in or on the code it is
/// generated into (<see cref="GramCompilerOptions"/>).
/// </remarks>
sealed class GrammarOptions
{
	/// <summary>Name of the grammar file, used in diagnostics and hint names.</summary>
	public string FileName { get; set; } = "grammar.gram";

	/// <summary>Stable generated-language id; null emits no language descriptor.</summary>
	public string? LanguageId { get; set; }

	/// <summary>The complete grammar source stored in a generated language descriptor.</summary>
	public string? LanguageSource { get; set; }

	/// <summary>Semantic classification mappings stored in a generated language descriptor.</summary>
	public string? LanguageClassifications { get; set; }

	/// <summary>Non-executing guard and external-recognizer mappings stored in the descriptor.</summary>
	public string? LanguageRecognitionContract { get; set; }

	/// <summary>
	/// Whether to read the input as tokens rather than as characters (§4.5's other side).
	/// </summary>
	/// <remarks>
	/// A request rather than a setting: a grammar that cannot be cut in two is compiled over
	/// characters and told why, because the character machine is correct and right there.
	/// `docs/internal/design/lexical-adt-design.md` carries the design and its measurements.
	/// </remarks>
	public bool Lexical { get; set; }

	/// <summary>
	/// Whether a call to a rule that only hands on another rule's value is compiled as the choice
	/// of what it hands on. On by default and not offered in the attribute; off is for a test that
	/// holds a trace build's frames of those rules to a parser that calls them as written.
	/// </summary>
	/// <remarks>
	/// The generator sets it off where the compilation defines <c>DOTGRAM_NO_COLLAPSE</c>, and
	/// nowhere else. Off, the parser is a different one, and not only slower: the order of a
	/// refusal's expected list can differ.
	/// </remarks>
	public bool CollapseForwarders { get; set; } = true;

	/// <summary>
	/// Whether the shared-recursion analysis runs (GRAM4054, and what <see cref="MemoiseSuccesses"/>
	/// remembers). On by default; the editor leaves it off, since it costs a second on a grammar of
	/// a thousand rules on every analysis of the document and the hint is the build's to say.
	/// </summary>
	public bool AnalyseSharedRecursion { get; set; } = true;

	/// <summary>
	/// Whether a <c>parse</c> is compiled knowing that it is also read from a position, where
	/// anything may follow it, and not only to the end of input. Experimental, and off by default.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <c>parse</c> has whole forms, which demand the end of input, and positional and window
	/// forms, which begin where they are told and demand nothing after the rule (§6.3). The rule
	/// is compiled once for both. Off, it is compiled against the end of input, and a decision
	/// taken on the strength of that end — a way back not kept because what it would reach could
	/// not have been followed by the end — can make a positional form miss the rule's first
	/// reading: <c>Start = (R1 | R1)?</c> with <c>R1 = 'a' &amp; 'k'</c>, read from 0 in
	/// <c>ab</c>, answers no reading where the rule reads nothing.
	/// </para>
	/// <para>
	/// On, every positional reading is the rule's first reading, and the whole forms lose the
	/// proofs they rest on: larger readers, and several grammars move from the immediate carrier
	/// to the tape. The generator sets it where the build sets <c>DotGramPositionalFollow</c>.
	/// </para>
	/// </remarks>
	public bool PositionalFollow { get; set; }

	/// <summary>
	/// With <see cref="PositionalFollow"/>, whether the positional end is told apart from anything
	/// where that changes no answer: experimental, off by default, and meaningless without it.
	/// </summary>
	/// <remarks>
	/// <see cref="PositionalFollow"/> says that after a positional reading anything may follow, and
	/// every proof that rested on the end of input gives way. Some of those proofs ask only whether
	/// what follows can <em>fail</em> after the construct succeeded — whether a repetition may be
	/// asked to give a turn back — and from a position, where nothing after the rule is demanded,
	/// it cannot, unless something that reads nothing and can refuse stands between. Set, those
	/// proofs are told that, and hold for the whole forms as they did without the switch; every
	/// other question is answered as <see cref="PositionalFollow"/> answers it. The generator sets
	/// it where the build sets <c>DotGramPositionalFollow</c> to <c>split</c>.
	/// </remarks>
	public bool PositionalFollowSplit { get; set; }

	/// <summary>
	/// How much of the text is the host's own, where the rest of it was included from
	/// a base. Null when all of it is, which is every grammar that inherits nothing.
	/// </summary>
	/// <remarks>
	/// <para>
	/// What it decides is publication: a <c>parse</c> written past this point belongs to
	/// the grammar that was included, and that grammar's own class already published it.
	/// Publishing it again here would put a second method of the same name on a derived
	/// class — and it would be the base's rule, not the dialect's reading of it, which
	/// is the opposite of what a dialect is for. A host takes its base's rules and
	/// publishes what it means to publish.
	/// </para>
	/// <para>
	/// One number rather than a set of ranges, because the joined text puts the host's
	/// own grammar first and everything included after it (<see cref="GrammarSplice"/>).
	/// </para>
	/// </remarks>
	public int? Own { get; set; }

	/// <summary>
	/// Where each included grammar's wrapper and the one after it begin in the joined text
	/// (<see cref="GrammarSplice.Cuts"/>), so that what one leaves open is not closed by the next.
	/// Null when there is nothing to cut besides <see cref="Own"/>.
	/// </summary>
	public IReadOnlyList<int>? Cuts { get; set; }
}

/// <summary>
/// What the grammar's author asks of the generated parser, or declares of the code it is handed:
/// its carrier and memoisation, the input forms it is offered over, its checks and traces, and what
/// the author's own constructions may do.
/// </summary>
/// <remarks>
/// Options of a grammar's host — its attribute, its build — read by the emitter, apart from what a target
/// can do (<see cref="TargetCapabilities"/>) and from the numbers the generator is tuned by
/// (<see cref="TargetTuning"/>). The grammar's own stages ask none of it (<see cref="GramCompilerOptions"/>).
/// </remarks>
sealed class AuthorOptions
{
	/// <summary>
	/// Whether the generated class carries the grammar it was compiled from, for another
	/// project to include (<c>[Gram(ExportGrammar = …)]</c>).
	/// </summary>
	public bool Portable { get; set; }

	/// <summary>
	/// How a reader carries what it has read until the author's constructions run
	/// (<see cref="CarrierKind"/>). <see cref="CarrierKind.Auto"/> by default, which is the
	/// immediate carrier wherever it can carry a machine; a machine the immediate carrier cannot
	/// carry is compiled on the tape instead.
	/// </summary>
	public CarrierKind Carrier { get; set; } = CarrierKind.Auto;

	/// <summary>
	/// Whether a construction of the grammar may throw on a reading the parse gives up
	/// (<c>[Gram(ConstructionsMayThrow = …)]</c>). Then a machine on the immediate carrier holds
	/// what a construction throws and throws it only where the tape would have built the value;
	/// otherwise the exception leaves the parse where it is thrown. The tape ignores it.
	/// </summary>
	public bool ConstructionsMayThrow { get; set; }

	/// <summary>
	/// Whether to say, in <see cref="GramCompilation.Carriers"/>, which carrier each machine has and
	/// what the tape would hold back: the rules the gates name and the place each was found. For a
	/// report; a parse is generated the same either way, and without it nothing of this is kept.
	/// </summary>
	public bool ReportCarriers { get; set; }

	/// <summary>
	/// Whether each rule a reader reads by a method of its own counts its entries, in a field of
	/// that reader written inside <c>#if DOTGRAM_COUNTS</c>. For a test that asks how often a rule
	/// was entered; not a supported setting.
	/// </summary>
	/// <remarks>
	/// The generator sets it where the compilation defines <c>DOTGRAM_COUNTS</c>, and nowhere else:
	/// a field a rule is bytes in every parser, so a parser built without the symbol carries none.
	/// </remarks>
	public bool CountRules { get; set; }

	/// <summary>
	/// Whether the walk that builds values asserts its invariants as it goes, in code written inside
	/// <c>#if DOTGRAM_CHECKS</c>. For the repository's own checked builds; not a supported setting.
	/// </summary>
	/// <remarks>
	/// The generator sets it where the compilation defines <c>DOTGRAM_CHECKS</c>, and nowhere else:
	/// without it no text of the checks is written, where it used to be written everywhere and
	/// compiled nowhere. With it the checks are written inside the same <c>#if</c>, so what runs is
	/// decided by the symbol as before and a parser compiled from this text without the symbol is
	/// the parser compiled without the option.
	/// </remarks>
	public bool Checks { get; set; }

	/// <summary>
	/// Whether the engine's step trace is written into the parser: a line on standard error per step
	/// of the automaton, for whoever works on the generator. Not a supported setting.
	/// </summary>
	/// <remarks>
	/// The generator sets it where the compilation defines <c>DOTGRAM_STEPS</c>, and nowhere else:
	/// without it neither the calls nor the methods they call are written, where they used to be
	/// written everywhere and removed by the C# compiler in every build without the symbol.
	/// </remarks>
	public bool Steps { get; set; }

	/// <summary>
	/// Whether the parser is a trace build: its readers report what they do to a sink set with
	/// <c>Tracing(sink)</c> on the generated class, which carries the sink types and three
	/// ready-made sinks beside the parser (docs/syntax.md, §6.9).
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>[GramParserVariant("Traced", Trace = true)]</c> asks for it on one reading of a class, and the build
	/// property <c>DotGramTrace</c> on every reading of a project. Off, nothing of it is written:
	/// the parser is the same text byte for byte.
	/// </para>
	/// <para>
	/// A trace build is a slightly different program from the one it observes — every return of
	/// a rule's method reports it, so its frames are larger and a deep reading changes stacks a
	/// little sooner — and it answers the same, refusals word for word.
	/// </para>
	/// </remarks>
	public bool Trace { get; set; }

	/// <summary>
	/// Whether a reader over tokens remembers where a rule that can reach itself has failed, and
	/// answers a second entry there at once. On by default; off is for a test that holds the
	/// parser with the memo to the parser without it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// One bit a rule and a token: the rule entered there failed. Nothing else is kept — a
	/// success is read again — and only for the rules that already probe the stack, the entries
	/// of the grammar's recursion, where a failure read again and again is what makes a nest of
	/// brackets cost the cube of its depth. A rule whose reading may depend on more than where it
	/// begins is not remembered: one that reaches a hook naming <c>context</c> or the reading's
	/// state, a machine that recovers, a rule that can give back. A machine over characters, or
	/// one reading a buffered input, remembers nothing.
	/// </para>
	/// <para>
	/// The generator sets it off where the compilation defines <c>DOTGRAM_NO_MEMO</c>, and
	/// nowhere else.
	/// </para>
	/// </remarks>
	public bool MemoiseFailures { get; set; } = true;

	/// <summary>
	/// Whether a reader over tokens remembers where the rules of a recursion some ordered choice
	/// reads twice have succeeded, and what they left, and answers a second entry there at once.
	/// On by default; off is for a test that holds the parser with the memo to the parser without it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A choice whose alternatives share a prefix holding a nested instance of the recursion the
	/// choice stands in, and whose deciding token comes after that instance, reads the instance once
	/// per alternative at every level of a nest: two to the depth (<c>GRAM4054</c>). For the rules of
	/// such a recursion — the first both alternatives enter at one token, among those
	/// <see cref="MemoiseFailures"/> already remembers, under its contract — the reader keeps where
	/// the rule ended and the value it left, two integers a rule and a token and a bit of the memo
	/// word, and every level is read once. Only on the immediate carrier, whose values a register
	/// hands on: the tape unwinds the records of a reading given up; and not where a construction may
	/// throw. Nothing is remembered in a grammar no such choice was found in, so a grammar without
	/// the shape pays nothing.
	/// </para>
	/// <para>
	/// The generator sets it off where the compilation defines <c>DOTGRAM_NO_SUCCESS_MEMO</c>, and
	/// nowhere else.
	/// </para>
	/// </remarks>
	public bool MemoiseSuccesses { get; set; } = true;

	/// <summary>
	/// Whether the consumer's C# is 11 or later, so that the code may hold UTF-8 string
	/// literals. Off by default, which is the C# 8 floor every emitted file is held to.
	/// </summary>
	/// <remarks>
	/// It chooses how a lexer's table is written and nothing a consumer can see: as a UTF-8
	/// literal, which the C# compiler reads as one token, or as an array initializer of every
	/// cell, which is the floor's form and the slowest thing in a large grammar to compile.
	/// The generator sets it from the effective language version of the compilation.
	/// </remarks>
	public bool Utf8Literals { get; set; }

	/// <summary>
	/// Typed value storage for all direct tape readers in this compilation. Other
	/// carriers and the non-direct engine do not use these tables. Auto chooses at
	/// generation time; explicit strategies override both dense and paged heuristics.
	/// </summary>
	public ValueStorageKind ValueStorage { get; set; } = ValueStorageKind.Auto;

	/// <summary>Add a buffered pull-input form beside existing publications.</summary>
	public bool BufferedInput { get; set; }

	/// <summary>Add buffered byte-input publications; no text decoding is performed.</summary>
	public bool BufferedBytes { get; set; }

	/// <summary>Pass character captures to semantic actions as ReadOnlySpan&lt;char&gt;.</summary>
	public bool SpanCaptures { get; set; }

	/// <summary>
	/// What a buffered overload retains at most where the call does not say; see
	/// <c>[Gram(MaxBufferedInputLength = …)]</c>.
	/// </summary>
	public int MaxRetained { get; set; } = int.MaxValue;

	/// <summary>A buffered overload's initial capacity where the call does not say.</summary>
	public int BufferSize { get; set; } = 4096;

	/// <summary>
	/// How many stacks one parse may take beyond the one it began on, or nought for as
	/// many as there is memory for.
	/// </summary>
	/// <remarks>
	/// A reading that runs its stack low carries on over a stack of its own; this is
	/// where that stops and the parse fails with <c>InsufficientExecutionStackException</c>
	/// instead, which is what it did before it could carry on at all.
	/// </remarks>
	public int Stacks { get; set; }
}

/// <summary>
/// How the generated parser is tuned: the numbers the generator divides and tabulates it by, and the
/// switches the tests turn the cheaper strategies off with.
/// </summary>
/// <remarks>
/// Not what the author asks for (<see cref="AuthorOptions"/>), nor what the target can do
/// (<see cref="TargetCapabilities"/>): a different number here writes a different parser that reads the
/// same. Read by the emitter; the grammar's own stages ask none of it (<see cref="GramCompilerOptions"/>).
/// </remarks>
sealed class TargetTuning
{
	/// <summary>
	/// How large the parts of a divided recognizer should be aimed to be, in the
	/// generator's own estimate of basic blocks. Null takes the measured default.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A recognizer too large for one method is written in several (§6.3), and how large
	/// each should be was measured on a synthetic grammar as flat anywhere between sixty
	/// and two hundred and fifty. Flat, but measured on grammars that are not the
	/// consumer's — so the number is theirs to change, from <c>DotGramPartSize</c> in
	/// their build.
	/// </para>
	/// <para>
	/// It is a wish and not a requirement. Every value produces a parser: nought and less
	/// ask for the finest division there is, anything past the size of the recognizer asks
	/// for one part, and no number written here can fail a compilation.
	/// </para>
	/// </remarks>
	public int? PartSize { get; set; }

	/// <summary>
	/// Whether disjoint literal prefixes dispatch through transition tables. On by default and
	/// not offered in the attribute; off is for a test that holds the tables to the chain of
	/// alternatives they replace.
	/// </summary>
	public bool PrefixTables { get; set; } = true;

	/// <summary>
	/// Whether the reader tries a choice of words through a table of them rather than a method a
	/// word. On by default and not offered in the attribute; off is for a test that holds the table
	/// to the alternatives it replaces.
	/// </summary>
	public bool WordTables { get; set; } = true;

	/// <summary>Separate complete engine/reader groups at this character count; zero (the default) keeps one source file.</summary>
	public int SourceFileSize { get; set; }

	/// <summary>
	/// Whether a publication that needs none of the automaton may be compiled as methods
	/// (<c>Machine.Direct.cs</c>). On by default and not offered in the attribute; off keeps
	/// the engine for every publication, which is what a test of the engine, or a comparison
	/// against it, asks for.
	/// </summary>
	public bool Direct { get; set; } = true;
}
