using System;
using System.Collections.Generic;

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

	/// <summary>
	/// The names an expression uses that it does not itself introduce.
	/// </summary>
	/// <remarks>
	/// <para>
	/// What a <c>=&gt;</c> or a <c>when</c> asks the parser for is exactly the free names in
	/// it that the parser has something to give — a capture, a supplied name, the context.
	/// Working that out by searching the text is wrong in both directions and was:
	/// <c>@(Log("parserInput"))</c> claimed the whole input, which then refused the grammar
	/// its flat rendering, and <c>@(other.context)</c> claimed the context because a dot is
	/// not an identifier character.
	/// </para>
	/// <para>
	/// Syntax only. Which type a name has needs a compilation and this does not ask: a name
	/// standing on its own is free, a name after a dot is a member, a name inside a literal
	/// is text, and a name a lambda in the expression introduced is that lambda's. That is
	/// the whole of the question, and all of it is in the syntax tree.
	/// </para>
	/// <para>
	/// Null where the scanner cannot parse the expression, which is not the same as an empty
	/// set: a caller that gets null falls back to what it did before rather than concluding
	/// the expression asks for nothing.
	/// </para>
	/// </remarks>
	IReadOnlyCollection<string>? FreeNames(string expression);
}
