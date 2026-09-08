using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>What a compilation produced: sources to add, and what went wrong.</summary>
public sealed class GramCompilation(IReadOnlyList<GeneratedSource> sources, IReadOnlyList<GramDiagnostic> diagnostics)
{
	public IReadOnlyList<GeneratedSource> Sources     { get; } = sources;
	public IReadOnlyList<GramDiagnostic>  Diagnostics { get; } = diagnostics;

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
	public override string ToString() => $"{Id} at {Position}..{Position + Length}: {Message}";
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
	/// `docs/lexical-adt-design.md` carries the design and its measurements.
	/// </remarks>
	public bool Lexical { get; set; }

	/// <summary>
	/// Whether a publication that needs none of the automaton may be compiled as methods
	/// (<c>Machine.Direct.cs</c>). On by default; off keeps the engine for every publication,
	/// which is what a test of the engine, or a comparison against it, asks for.
	/// </summary>
	public bool Direct { get; set; } = true;

	/// <summary>
	/// How a reader carries what it has read until the author's constructions run
	/// (<see cref="CarrierKind"/>). The tape by default, which keeps §7.3; the others are
	/// the author's choice, and a grammar a chosen carrier cannot carry is compiled on the
	/// tape instead.
	/// </summary>
	public CarrierKind Carrier { get; set; } = CarrierKind.Tape;

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
	/// Whether a publication the reader can write is written by it
	/// (<c>Machine.Reader.cs</c>) rather than by the rendering it is replacing. Off by
	/// default while the reader is being taught the rest of the language.
	/// </summary>
}
