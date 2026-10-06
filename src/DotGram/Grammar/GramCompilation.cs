using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>What a compilation produced: sources to add, and what went wrong.</summary>
public sealed class GramCompilation(IReadOnlyList<GeneratedSource> sources, IReadOnlyList<GramDiagnostic> diagnostics)
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
public readonly record struct GeneratedSource(string HintName, string Text);

/// <summary>
/// A compiler message, positioned in the grammar text.
/// </summary>
/// <remarks>
/// Deliberately not Roslyn's <c>Diagnostic</c>: the grammar half must stay callable
/// without Roslyn, so the shell converts these on the way out.
/// </remarks>
public sealed record GramDiagnostic(string Id, string Message, int Position, int Length, GramSeverity Severity)
{
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
public sealed class GramCompilerOptions
{
	/// <summary>Name of the grammar file, used in diagnostics and hint names.</summary>
	public string FileName { get; set; } = "grammar.gram";

	/// <summary>The partial class the generated members go into (§1).</summary>
	public string ClassName { get; set; } = "Grammar";

	/// <summary>Its namespace; null for the global one.</summary>
	public string? Namespace { get; set; }

	/// <summary>Stable generated-language id; null emits no language descriptor.</summary>
	public string? LanguageId { get; set; }

	/// <summary>The complete grammar source stored in a generated language descriptor.</summary>
	public string? LanguageSource { get; set; }

	/// <summary>Semantic classification mappings stored in a generated language descriptor.</summary>
	public string? LanguageClassifications { get; set; }

	/// <summary>Non-executing guard and external-recognizer mappings stored in the descriptor.</summary>
	public string? LanguageRecognitionContract { get; set; }

	/// <summary>
	/// Resolves the C# names a grammar refers to with <c>@</c>. Defaults to a resolver
	/// that accepts everything, which is right for tests of the grammar side and wrong
	/// for real generation.
	/// </summary>
	public ISymbolResolver SymbolResolver { get; set; } = PermissiveSymbolResolver.Instance;

	/// <summary>
	/// Finds where an inline <c>@(...)</c> expression ends. Null means the grammar may
	/// not use one — a diagnostic, not a crash.
	/// </summary>
	public ICSharpScanner? CSharpScanner { get; set; }

	/// <summary>
	/// Where the C# a grammar hands over is, for the <c>#line</c> directives of §7.6.
	/// </summary>
	/// <remarks>
	/// Null emits none, and that is the right answer for a caller with nothing to point
	/// at — a grammar compiled from a string in a test has no file an editor could open,
	/// and a directive naming one that does not exist is worse than none.
	/// </remarks>
	public ILineMap? LineMap { get; set; }

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
	/// Whether to read the input as tokens rather than as characters (§4.5's other side).
	/// </summary>
	/// <remarks>
	/// A request rather than a setting: a grammar that cannot be cut in two is compiled over
	/// characters and told why, because the character machine is correct and right there.
	/// `docs/design/lexical-adt-design.md` carries the design and its measurements.
	/// </remarks>
	public bool Lexical { get; set; }

	/// <summary>
	/// Whether disjoint literal prefixes dispatch through transition tables. On by default and
	/// not offered in the attribute; off is for a test that holds the tables to the chain of
	/// alternatives they replace.
	/// </summary>
	public bool PrefixTables { get; set; } = true;

	/// <summary>Separate complete engine/reader groups at this character count; zero (the default) keeps one source file.</summary>
	public int SourceFileSize { get; set; }

	/// <summary>
	/// Whether a publication that needs none of the automaton may be compiled as methods
	/// (<c>Machine.Direct.cs</c>). On by default and not offered in the attribute; off keeps
	/// the engine for every publication, which is what a test of the engine, or a comparison
	/// against it, asks for.
	/// </summary>
	public bool Direct { get; set; } = true;

	/// <summary>
	/// An interface whose implementors are told where they were written, by qualified name.
	/// </summary>
	/// <remarks>
	/// What <c>[Gram(…, LocationType = typeof(ISqlSpan))]</c> names. A rule whose value is
	/// assignable to it has its construction handed the span the rule stands on; a rule whose
	/// value is not, and a grammar that names nothing here, are compiled exactly as before.
	/// </remarks>
	public string? LocationType { get; set; }

	/// <summary>
	/// The nested class whose published methods call this parser with locations on, where
	/// <see cref="LocationType"/> is decided per call rather than by a second compilation
	/// (<c>[GramOptions(PerCall = true)]</c>). Null compiles locations in, as before.
	/// </summary>
	public string? LocatedFacade { get; set; }

	/// <summary>Whether the host declares <see cref="LocatedFacade"/> itself.</summary>
	public bool LocatedFacadeDeclared { get; set; }

	/// <summary>
	/// The classes whose static members the generated code may name, by qualified name.
	/// </summary>
	/// <remarks>
	/// One per grammar this one is built on. A rule that comes from another grammar goes on
	/// calling the helpers its author wrote, and those live beside that grammar's own host —
	/// so the generated file names them with <c>using static</c> rather than requiring the
	/// including class to derive from something for the sake of what its methods can see.
	/// </remarks>
	public IReadOnlyList<string> StaticImports { get; set; } = [];

	/// <summary>
	/// Whether the generated class carries the grammar it was compiled from, for another
	/// project to include (<c>[Gram(Portable = …)]</c>).
	/// </summary>
	public bool Portable { get; set; }

	/// <summary>
	/// How a reader carries what it has read until the author's constructions run
	/// (<see cref="CarrierKind"/>). The generator's choice by default; the others are the
	/// author's, and a grammar a chosen carrier cannot carry is compiled on the tape instead.
	/// </summary>
	public CarrierKind Carrier { get; set; } = CarrierKind.Auto;

	/// <summary>
	/// Whether to say, in <see cref="GramCompilation.Carriers"/>, why the carrier is the one it is:
	/// the rules the tape keeps and the place each was found. For a report; a parse is generated the
	/// same either way, and without it nothing of this is kept.
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
	/// Whether the parser is a trace build: its readers report what they do to a sink set with
	/// <c>Tracing(sink)</c> on the generated class, which carries the sink types and three
	/// ready-made sinks beside the parser (docs/syntax.md, §6.9).
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>[GramOptions(Trace = true)]</c> asks for it on one reading of a class, and the build
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
	/// <c>[Gram(MaxRetained = …)]</c>.
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

	/// <summary>
	/// The nested class this compilation goes into, where the host holds more than one
	/// (<c>[Gram(Suffix = "...")]</c>). Null puts it in the host class itself.
	/// </summary>
	/// <remarks>
	/// A host with two grammars — the same one compiled two ways, usually — cannot put
	/// both in one scope: a file's support is written once and named for what it is, so a
	/// second copy of <c>Match&lt;T&gt;</c>, of the failure, of the lexer and of the value
	/// tables would collide with the first, name for name. A class of its own is a scope
	/// of its own. What the host declares stays in reach either way: a nested class reads
	/// the static members of the class around it by their simple names, which is what a
	/// grammar's <c>=&gt;</c> calls are.
	/// </remarks>
	public string? Suffix { get; set; }

	/// <summary>
	/// Whether the host declares the <see cref="Suffix"/> class itself, and so decides who may
	/// see it.
	/// </summary>
	/// <remarks>
	/// A nested class written by the author as <c>internal static partial class Immediate { }</c>
	/// is the author's to make internal, and the generated part then says nothing of its own
	/// about accessibility. Where the author declares nothing the part is <c>public</c>, which
	/// is what it has always been — a nested class would otherwise default to private.
	/// </remarks>
	public bool SuffixDeclared { get; set; }

	/// <summary>
	/// Which compilation of a host writes the types the host's own C# names — the span
	/// (§7.5) and the match (§6.1). Null where the host has one grammar and they are its
	/// own; true for the one that writes them for every other; false for the others.
	/// </summary>
	public bool? SharedTypes { get; set; }

	/// <summary>
	/// Whether the host inherits a grammar from a base class, so that what this
	/// compilation writes stands beside a parser the base already wrote.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A dialect is a class that inherits a grammar and publishes its own reading of
	/// it — T-SQL over standard SQL. Both classes are whole parsers, and the derived
	/// one names its members what the base named its own: <c>ParseSelect</c> is
	/// <c>ParseSelect</c> in either. In C# that is hiding, and hiding is exactly what
	/// a dialect means — <c>TransactSql.ParseSelect</c> reads T-SQL — so the warning
	/// asking whether it was intended is answered here rather than left to the
	/// consumer, who did not write the code it is about.
	/// </para>
	/// <para>
	/// The support types are hidden too, and are not shared instead: a base declares
	/// them only where it publishes something (<see cref="SharedTypes"/>), so a host
	/// that went without its own would have none at all wherever it inherits a grammar
	/// that publishes nothing — which is the ordinary shape of a grammar written to be
	/// inherited. Each class carries its own, and the values they hold — the trees the
	/// author's own <c>=&gt;</c> builds — are the same types either way.
	/// </para>
	/// </remarks>
	public bool Inherits { get; set; }

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
	/// Handed the bound grammar once names are resolved, for a caller that asks where each
	/// name went — the editor, which navigates by it. Null for the generator, which keeps
	/// nothing of the model past the compile.
	/// </summary>
	internal Action<Binding.GrammarModel>? Bound { get; set; }
}
