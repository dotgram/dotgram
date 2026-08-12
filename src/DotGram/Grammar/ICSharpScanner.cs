using System;

namespace DotGram.Grammar;

/// <summary>
/// Finds where an inline <c>@(...)</c> expression ends.
/// </summary>
/// <remarks>
/// <para>
/// The second seam. <c>@(</c> is the only place in a <c>.gram</c> file that holds raw
/// C# text, and finding its closing parenthesis is a lexer's job, not a counter's: a
/// <c>)</c> can sit inside a string, a verbatim or raw or interpolated literal, a
/// character literal, or a comment.
/// </para>
/// <para>
/// A fake implementation that gets those cases wrong will make tests pass while the
/// product is broken, so tests that exercise <c>@(...)</c> itself must use the real
/// Roslyn-backed scanner. A fake is only for tests where inline expressions do not
/// appear at all.
/// </para>
/// </remarks>
public interface ICSharpScanner
{
	/// <summary>
	/// Given the index of the <c>(</c> that opens an inline expression, finds the index
	/// of the matching <c>)</c>.
	/// </summary>
	/// <returns><c>false</c> when the expression is unterminated.</returns>
	bool TryFindClosingParenthesis(string text, int openParenthesisIndex, out int closeParenthesisIndex);
}
