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
	/// Whether a publication the reader can write is written by it
	/// (<c>Machine.Reader.cs</c>) rather than by the rendering it is replacing. Off by
	/// default while the reader is being taught the rest of the language.
	/// </summary>
	/// <summary>
	/// Whether the methods are written the way a person would have written them, where
	/// they can be. Null takes the reader and says nothing where it declines; true asks
	/// for it and is told where it declines (GRAM5006); false keeps the older rendering.
	/// </summary>
	public bool? Reader { get; set; }
}
