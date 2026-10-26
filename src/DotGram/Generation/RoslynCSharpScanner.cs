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

		return new HashSet<string>(Free(parsed).Select(name => name.Identifier.ValueText), StringComparer.Ordinal);
	}

	/// <summary>
	/// The expression with the free names <paramref name="names"/> maps spelled as it says.
	/// </summary>
	/// <remarks>
	/// Replaced in the text rather than in the tree and printed back: a tree printed back is
	/// the tree's spacing, and the text is what a line directive points the author at.
	/// </remarks>
	public string? Renamed(string expression, IReadOnlyDictionary<string, string> names)
	{
		if (expression is null || names is null)
			return null;

		var parsed = SyntaxFactory.ParseExpression(expression);

		if (parsed.ContainsDiagnostics)
			return null;

		var text = expression;

		// From the end, so a replacement longer or shorter than its name moves nothing still
		// to be replaced.
		foreach (var name in Free(parsed).OrderByDescending(one => one.Identifier.SpanStart))
			if (names.TryGetValue(name.Identifier.ValueText, out var spelled))
				text = text.Substring(0, name.Identifier.SpanStart) + spelled + text.Substring(name.Identifier.Span.End);

		return text;
	}

	/// <summary>Every use of a name the expression does not itself introduce where the use is.</summary>
	/// <remarks>
	/// <para>
	/// A name is introduced over a scope, not over the whole expression: in
	/// <c>items.Select(x =&gt; x + 1).Sum() + x</c> the lambda's <c>x</c> is its own, and the
	/// last <c>x</c> is the outer one, which C# 8 lets a lambda parameter shadow. So each name
	/// introduced is kept with the node it is in scope over, and a use is free unless one of
	/// those nodes holds it.
	/// </para>
	/// <para>
	/// The scopes are C#'s: a lambda's parameters over the lambda, a local over its block, a
	/// pattern variable over the statement, arm or clause that ends it — the block around an
	/// <c>if</c>, which it leaks into, but a loop's own condition and an embedded statement
	/// each their own — and a range variable over its query but not over its own source.
	/// </para>
	/// </remarks>
	static IEnumerable<IdentifierNameSyntax> Free(ExpressionSyntax parsed)
	{
		var scopes = new Dictionary<string, List<(SyntaxNode Over, SyntaxNode? Except)>>(StringComparer.Ordinal);

		foreach (var declared in parsed.DescendantNodes())
			if (Introduced(declared, parsed) is var (identifier, over, except) && identifier.ValueText.Length > 0)
			{
				if (!scopes.TryGetValue(identifier.ValueText, out var list))
					scopes[identifier.ValueText] = list = [];

				list.Add((over, except));
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

			// `var` in `out var n` or `var y = …` is the keyword, not a name.
			if (name.IsVar)
				continue;

			if (!scopes.TryGetValue(name.Identifier.ValueText, out var over) || !over.Any(scope => Holds(scope, name)))
				yield return name;
		}

		static bool Holds((SyntaxNode Over, SyntaxNode? Except) scope, SyntaxNode name)
		{
			return scope.Over.Span.Contains(name.Span) && scope.Except?.Span.Contains(name.Span) != true;
		}
	}

	/// <summary>
	/// The name a node introduces, the node it is in scope over, and the part of that node it
	/// is not in scope over — or null when the node introduces nothing.
	/// </summary>
	static (SyntaxToken Identifier, SyntaxNode Over, SyntaxNode? Except)? Introduced(SyntaxNode node, ExpressionSyntax root)
	{
		switch (node)
		{
			// A lambda's, an anonymous method's or a local function's parameter: over that function.
			case ParameterSyntax parameter:
				return (parameter.Identifier, (parameter.Parent is ParameterListSyntax list ? list.Parent : parameter.Parent) ?? root, null);

			// A local declared in a statement, over the block that holds it; one declared by
			// a `for`, `using` or `fixed`, over that statement.
			case VariableDeclaratorSyntax variable:
				return (variable.Identifier, variable.Parent?.Parent switch
				{
					LocalDeclarationStatementSyntax statement => statement.Parent ?? root,
					{ } other                                 => other,
					null                                      => root,
				}, null);

			// A pattern variable or an `out var`: over the nearest thing that ends its scope,
			// or the whole expression outside any.
			case SingleVariableDesignationSyntax designation:
				return (designation.Identifier, Enclosing(designation) ?? root, null);

			case ForEachStatementSyntax loop:
				return (loop.Identifier, loop, loop.Expression);

			case CatchDeclarationSyntax caught:
				return (caught.Identifier, caught.Parent ?? root, null);

			case LocalFunctionStatementSyntax function:
				return (function.Identifier, function.Parent ?? root, null);

			// A range variable, over its query, but not over the source it ranges over.
			case FromClauseSyntax from:
				return (from.Identifier, Query(from) ?? root, from.Expression);

			case JoinClauseSyntax join:
				return (join.Identifier, Query(join) ?? root, join.InExpression);

			case LetClauseSyntax let:
				return (let.Identifier, Query(let) ?? root, null);

			case JoinIntoClauseSyntax into:
				return (into.Identifier, Query(into) ?? root, null);

			case QueryContinuationSyntax continuation:
				return (continuation.Identifier, continuation, null);

			default:
				return null;
		}

		static SyntaxNode? Enclosing(SyntaxNode node)
		{
			return node.Ancestors().FirstOrDefault(static one => one is
				AnonymousFunctionExpressionSyntax or BlockSyntax or SwitchSectionSyntax or SwitchExpressionArmSyntax or
				CatchClauseSyntax or WhileStatementSyntax or DoStatementSyntax or ForStatementSyntax or
				CommonForEachStatementSyntax or UsingStatementSyntax or LockStatementSyntax or FixedStatementSyntax or
				QueryClauseSyntax or SelectOrGroupClauseSyntax ||
				// An embedded statement — the body of an `if` or a loop written without braces — is
				// a scope of its own.
				one is StatementSyntax && one.Parent is not (BlockSyntax or SwitchSectionSyntax or LabeledStatementSyntax));
		}

		static SyntaxNode? Query(SyntaxNode node)
		{
			return node.Ancestors().FirstOrDefault(static one => one is QueryExpressionSyntax);
		}
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
