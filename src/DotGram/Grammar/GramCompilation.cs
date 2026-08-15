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
}
