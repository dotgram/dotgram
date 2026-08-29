using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Generation;

/// <summary>
/// Finds the end of an inline <c>@(...)</c> expression using Roslyn's own C# lexer.
/// </summary>
/// <remarks>
/// <para>
/// The lexer, not the parser. <c>SyntaxFactory.ParseExpression</c> looks like the right
/// tool and is not: it is greedy and knows nothing about where a grammar expression
/// ends, while <c>&amp;</c>, <c>|</c>, <c>*</c>, <c>+</c>, <c>?</c>, <c>[</c> and
/// <c>..</c> are all valid C# operators. On <c>when @(qty &gt; 0) &amp; b: Y</c> it
/// would happily consume <c>&amp; b</c> and stop only at the colon.
/// </para>
/// <para>
/// Tokens are what make this correct: a string, verbatim, interpolated or raw literal,
/// a character literal and a comment each arrive as a single token, so a <c>)</c>
/// inside one cannot change the nesting depth.
/// </para>
/// </remarks>
public sealed class RoslynCSharpScanner : ICSharpScanner
{
	public static readonly RoslynCSharpScanner Instance = new();

	/// <summary>
	/// The names the expression uses and does not introduce, from its syntax alone.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The parser this time rather than the lexer, and it is the right tool here for the
	/// same reason it was the wrong one above: the text handed over is already known to be
	/// one whole expression, so nothing is left for greediness to run into.
	/// </para>
	/// <para>
	/// Four things are not free names, and all four are visible in the tree. The name after
	/// a dot is a member — <c>other.context</c> names `other`. The name before a dot may be
	/// a namespace or a type, and is kept: telling `System.Math` from `total.Length` needs a
	/// compilation, and answering "free" for both costs a parameter nobody passes rather
	/// than a wrong one. A name a lambda in the expression introduced belongs to that
	/// lambda. And text inside a literal is not a name at all, which is the whole of what
	/// searching the spelling got wrong.
	/// </para>
	/// </remarks>
	public IReadOnlyCollection<string>? FreeNames(string expression)
	{
		if (expression is null)
			return null;

		var parsed = SyntaxFactory.ParseExpression(expression);

		// An expression this cannot read is one this cannot answer for. Null rather than
		// nothing, so a caller falls back rather than concluding it asks for nothing.
		if (parsed.ContainsDiagnostics)
			return null;

		var free  = new HashSet<string>(StringComparer.Ordinal);
		var bound = new HashSet<string>(StringComparer.Ordinal);

		foreach (var declared in parsed.DescendantNodes())
			switch (declared)
			{
				case ParameterSyntax parameter:
					bound.Add(parameter.Identifier.ValueText);
					break;

				case VariableDeclaratorSyntax variable:
					bound.Add(variable.Identifier.ValueText);
					break;

				case SingleVariableDesignationSyntax designation:
					bound.Add(designation.Identifier.ValueText);
					break;
			}

		foreach (var name in parsed.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
		{
			// The right of a dot, of a `?.`, or of a `->` is a member of whatever stands to
			// its left, and names nothing the parser could supply.
			if (name.Parent is MemberAccessExpressionSyntax member && member.Name == name)
				continue;

			if (name.Parent is MemberBindingExpressionSyntax binding && binding.Name == name)
				continue;

			// `X = value` inside an object initializer names a member of the type being
			// made, not anything in scope.
			if (name.Parent is AssignmentExpressionSyntax assignment &&
				assignment.Left == name &&
				assignment.Parent is InitializerExpressionSyntax)
				continue;

			// `f(name: value)` names a parameter of `f`.
			if (name.Parent is NameColonSyntax or NameEqualsSyntax)
				continue;

			if (!bound.Contains(name.Identifier.ValueText))
				free.Add(name.Identifier.ValueText);
		}

		return free;
	}

	public bool TryFindClosingParenthesis(string text, int openParenthesisIndex, out int closeParenthesisIndex)
	{
		closeParenthesisIndex = -1;

		if (text is null || openParenthesisIndex < 0 || openParenthesisIndex >= text.Length || text[openParenthesisIndex] != '(')
			return false;

		var depth = 0;

		// initialTokenPosition matters as much as offset: without it the returned spans
		// are numbered from zero rather than from where lexing started, and every
		// position handed back is short by openParenthesisIndex.
		foreach (var token in SyntaxFactory.ParseTokens(
			text,
			offset:               openParenthesisIndex,
			initialTokenPosition: openParenthesisIndex))
		{
			switch (token.Kind())
			{
				case SyntaxKind.OpenParenToken:
					depth++;
					break;

				case SyntaxKind.CloseParenToken:
					if (--depth == 0)
					{
						closeParenthesisIndex = token.SpanStart;
						return true;
					}

					break;

				case SyntaxKind.EndOfFileToken:
					return false;
			}
		}

		return false;
	}
}
