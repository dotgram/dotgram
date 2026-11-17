using System;
using System.Collections.Generic;
using System.Globalization;

using DotGram.Grammar.Fragments;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Language;

/// <summary>
/// Reads a host fragment with C#'s parser and lowers it to the closed <see cref="Fragment"/> tree.
/// </summary>
/// <remarks>
/// <para>
/// Beside <see cref="Grammar.ICSharpScanner"/>, not on it. The scanner answers where a fragment
/// ends, and the standard library's fragments are delimited by a scanner of its own that knows no
/// C#; the text between <c>@(</c> and <c>)</c> is the same whoever delimited it, and this reads
/// that text, so every fragment lowers the same way and no scanner has to learn a new question.
/// </para>
/// <para>
/// The parser, not the lexer: the text is already known to be one whole expression, as it is for
/// <see cref="RoslynCSharpScanner.FreeNames"/>, so nothing is left for the parser's greediness to
/// run into.
/// </para>
/// <para>
/// The language version is fixed. A build reads fragments with whatever Roslyn its SDK carries,
/// the command-line tool with the newest; what is portable must not depend on which of them asked,
/// so both parse as the oldest compiler the generator loads into reads by default. A form newer
/// than that is a parse error under either, and is refused as <c>Unparsable</c> by both.
/// </para>
/// <para>
/// Lowering never fails: whatever C# says that the tree has no form for is an
/// <see cref="Fragment.Unportable"/> at its place, and a C# build never reads the tree to print a
/// fragment it can print as written.
/// </para>
/// </remarks>
static class RoslynFragmentLowering
{
	/// <summary>
	/// C# 13, the newest version the oldest Roslyn the generator loads into (4.14) knows.
	/// </summary>
	public static CSharpParseOptions Options { get; } = new(LanguageVersion.CSharp13, DocumentationMode.None);

	public static LoweredFragment Lower(string text)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var parsed   = SyntaxFactory.ParseExpression(text, 0, Options, consumeFullText: true);
		var refusals = new List<Fragment.Unportable>();

		// What follows the expression's last token on a line of its own belongs to the end of the
		// text, not to the expression; it may be trivia and nothing else.
		var end  = parsed.FullSpan.End;
		var tail = end < text.Length ? SyntaxFactory.ParseLeadingTrivia(text.Substring(end)) : default;

		if (parsed.ContainsDiagnostics || end + tail.FullSpan.Length != text.Length)
		{
			var whole = new Fragment.Unportable("Unparsable") { Span = new FragmentSpan(0, text.Length) };

			return new LoweredFragment(text, whole, [whole]);
		}

		var root = Lowering(parsed, refusals);

		// The tree has no place for a comment, and a printer that re-spells a fragment drops it;
		// a comment in a fragment is C#'s.
		foreach (var trivia in parsed.DescendantTrivia(descendIntoTrivia: true))
			Trivia(trivia, 0, refusals);

		foreach (var trivia in tail)
			Trivia(trivia, end, refusals);

		refusals.Sort(static (left, right) => left.Span.Start.CompareTo(right.Span.Start));

		return new LoweredFragment(text, root, refusals);
	}

	static void Trivia(SyntaxTrivia trivia, int offset, List<Fragment.Unportable> refusals)
	{
		var span = new FragmentSpan(offset + trivia.Span.Start, trivia.Span.Length);

		if (trivia.Kind() is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia or
			SyntaxKind.SingleLineDocumentationCommentTrivia or SyntaxKind.MultiLineDocumentationCommentTrivia)
			refusals.Add(new Fragment.Unportable("Comment") { Span = span });
		else if (trivia.IsDirective || trivia.IsKind(SyntaxKind.DisabledTextTrivia))
			refusals.Add(new Fragment.Unportable("Directive") { Span = span });
	}

	static Fragment Lowering(ExpressionSyntax node, List<Fragment.Unportable> refusals)
	{
		var span = Of(node.Span);

		switch (node)
		{
			case LiteralExpressionSyntax literal:
				return Literal(literal, refusals);

			case IdentifierNameSyntax name:
				if (IsVerbatim(name.Identifier))
					return Refuse("VerbatimIdentifier", name, refusals);

				return new Fragment.Name(name.Identifier.ValueText) { Span = span };

			// `string`, `int`: the first part of a host name such as `string.Empty`, which another
			// language spells by a map as it does any other.
			case PredefinedTypeSyntax predefined:
				return new Fragment.Name(predefined.Keyword.ValueText) { Span = span };

			case MemberAccessExpressionSyntax member when member.IsKind(SyntaxKind.SimpleMemberAccessExpression):
				if (member.Name is not IdentifierNameSyntax memberName || IsVerbatim(memberName.Identifier))
				{
					var refused = Refuse(member.Name is IdentifierNameSyntax ? "VerbatimIdentifier" : member.Name.Kind().ToString(), member.Name, refusals);

					Lowering(member.Expression, refusals);

					return refused;
				}

				return new Fragment.Member(Lowering(member.Expression, refusals), memberName.Identifier.ValueText) { Span = span };

			case InvocationExpressionSyntax call:
				return Call(call, refusals);

			case ObjectCreationExpressionSyntax creation:
				return Creation(creation, refusals);

			case BinaryExpressionSyntax binary:
				return Binary(binary, refusals);

			case PrefixUnaryExpressionSyntax not when not.IsKind(SyntaxKind.LogicalNotExpression) && MayBeNegated(not.Operand):
				return new Fragment.Not(Lowering(not.Operand, refusals)) { Span = span };

			case ConditionalExpressionSyntax conditional:
				return new Fragment.Conditional(
					Lowering(conditional.Condition, refusals),
					Lowering(conditional.WhenTrue,  refusals),
					Lowering(conditional.WhenFalse, refusals)) { Span = span };

			case IsPatternExpressionSyntax test:
				return IsNull(test, refusals);

			case ParenthesizedExpressionSyntax parenthesized:
				return new Fragment.Parenthesized(Lowering(parenthesized.Expression, refusals)) { Span = span };

			// `.b` and `[i]` after a `?.` stand only inside the `?.`, which is refused already;
			// what an index holds is lowered, and may be refused on its own.
			case MemberBindingExpressionSyntax:
				return new Fragment.Unportable(node.Kind().ToString()) { Span = span };

			case ElementBindingExpressionSyntax element:
				foreach (var argument in element.ArgumentList.Arguments)
					Lowering(argument.Expression, refusals);

				return new Fragment.Unportable(node.Kind().ToString()) { Span = span };

			default:
				return Refuse(node.Kind().ToString(), node, refusals);
		}
	}

	static Fragment Literal(LiteralExpressionSyntax literal, List<Fragment.Unportable> refusals)
	{
		var span  = Of(literal.Span);
		var token = literal.Token;

		switch (literal.Kind())
		{
			case SyntaxKind.TrueLiteralExpression:
				return new Fragment.Boolean(true) { Span = span };

			case SyntaxKind.FalseLiteralExpression:
				return new Fragment.Boolean(false) { Span = span };

			case SyntaxKind.NullLiteralExpression:
				return new Fragment.Null { Span = span };

			case SyntaxKind.StringLiteralExpression:
				if (!token.IsKind(SyntaxKind.StringLiteralToken))
					return Refuse("RawString", literal, refusals);

				if (token.Text.StartsWith("@", StringComparison.Ordinal))
					return Refuse("VerbatimString", literal, refusals);

				return new Fragment.Text(token.ValueText) { Span = span };

			case SyntaxKind.CharacterLiteralExpression:
				return new Fragment.Character((char)token.Value!) { Span = span };

			case SyntaxKind.NumericLiteralExpression:
				return Number(token, refusals);

			default:
				return Refuse(literal.Kind().ToString(), literal, refusals);
		}
	}

	/// <summary>
	/// An integer in decimal or hexadecimal, or digits, a point and digits; nothing else.
	/// </summary>
	/// <remarks>
	/// What C# makes of a suffix, an exponent, a binary literal or a digit separator is C#'s: the
	/// type of <c>1L</c> or <c>1f</c> is spelled differently in every language, and is refused
	/// where it stands rather than printed as a number of another type.
	/// </remarks>
	static Fragment Number(SyntaxToken token, List<Fragment.Unportable> refusals)
	{
		var text = token.Text;
		var span = Of(token.Span);

		if (text.IndexOf('_') >= 0)
			return Refuse("DigitSeparator", token.Span, refusals);

		var isHex    = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
		var isBinary = text.StartsWith("0b", StringComparison.OrdinalIgnoreCase);

		if (isBinary)
			return Refuse("BinaryInteger", token.Span, refusals);

		if (isHex)
		{
			for (var at = 2; at < text.Length; at++)
				if (!Uri.IsHexDigit(text[at]))
					return Refuse("NumericSuffix", token.Span, refusals);

			return new Fragment.Integer(Convert.ToUInt64(token.Value, CultureInfo.InvariantCulture), true) { Span = span };
		}

		var points = 0;

		foreach (var character in text)
		{
			if (character == '.')
			{
				points++;

				continue;
			}

			if (character is 'e' or 'E')
				return Refuse("Exponent", token.Span, refusals);

			if (character is < '0' or > '9')
				return Refuse("NumericSuffix", token.Span, refusals);
		}

		if (points == 0)
			return new Fragment.Integer(Convert.ToUInt64(token.Value, CultureInfo.InvariantCulture), false) { Span = span };

		// `.5` and `5.` are C#'s; `0.5` is everybody's.
		if (points == 1 && text[0] != '.' && text[text.Length - 1] != '.')
			return new Fragment.Real(text) { Span = span };

		return Refuse("NumericLiteralExpression", token.Span, refusals);
	}

	static Fragment Call(InvocationExpressionSyntax call, List<Fragment.Unportable> refusals)
	{
		// `nameof` is an operator of C#'s compiler, not a call a map could spell.
		if (call.Expression is IdentifierNameSyntax { Identifier.ValueText: "nameof" } name && !IsVerbatim(name.Identifier))
			return Refuse("NameOf", call.Span, refusals);

		var target = Lowering(call.Expression, refusals);

		// A call of a name or of a member; a call of what another call or a parenthesis gave
		// is a delegate's, and another language has no one spelling for it.
		var arguments = Arguments(call.ArgumentList, refusals);

		if (target is not (Fragment.Name or Fragment.Member or Fragment.Unportable))
			return Refuse("DelegateInvocation", call.Span, refusals);

		return new Fragment.Call(target, arguments) { Span = Of(call.Span) };
	}

	static Fragment Creation(ObjectCreationExpressionSyntax creation, List<Fragment.Unportable> refusals)
	{
		var type      = TypeName(creation.Type, refusals);
		var arguments = creation.ArgumentList is { } list ? Arguments(list, refusals) : [];

		if (creation.Initializer is { } initializer)
			return Refuse(initializer.Kind().ToString(), initializer, refusals);

		return new Fragment.New(type, arguments) { Span = Of(creation.Span) };
	}

	/// <summary>
	/// A type to construct, as a name or a dotted name; a generic, an array, a nullable or an
	/// alias-qualified type is C#'s.
	/// </summary>
	static Fragment TypeName(TypeSyntax type, List<Fragment.Unportable> refusals)
	{
		switch (type)
		{
			case IdentifierNameSyntax name when !IsVerbatim(name.Identifier):
				return new Fragment.Name(name.Identifier.ValueText) { Span = Of(name.Span) };

			case PredefinedTypeSyntax predefined:
				return new Fragment.Name(predefined.Keyword.ValueText) { Span = Of(predefined.Span) };

			case QualifiedNameSyntax qualified when qualified.Right is IdentifierNameSyntax right && !IsVerbatim(right.Identifier):
				return new Fragment.Member(TypeName(qualified.Left, refusals), right.Identifier.ValueText) { Span = Of(qualified.Span) };

			case IdentifierNameSyntax name:
				return Refuse("VerbatimIdentifier", name, refusals);

			case QualifiedNameSyntax qualified:
			{
				var refused = Refuse(qualified.Right is IdentifierNameSyntax ? "VerbatimIdentifier" : qualified.Right.Kind().ToString(), qualified.Right, refusals);

				TypeName(qualified.Left, refusals);

				return refused;
			}

			default:
				return Refuse(type.Kind().ToString(), type, refusals);
		}
	}

	static IReadOnlyList<Fragment> Arguments(ArgumentListSyntax list, List<Fragment.Unportable> refusals)
	{
		var arguments = new Fragment[list.Arguments.Count];

		for (var i = 0; i < arguments.Length; i++)
		{
			var argument = list.Arguments[i];

			arguments[i] = argument switch
			{
				{ RefKindKeyword.RawKind: (int)SyntaxKind.OutKeyword } => Refuse("OutArgument",   argument.Span, refusals),
				{ RefKindKeyword.RawKind: (int)SyntaxKind.RefKeyword } => Refuse("RefArgument",   argument.Span, refusals),
				{ RefKindKeyword.RawKind: (int)SyntaxKind.InKeyword }  => Refuse("InArgument",    argument.Span, refusals),
				{ NameColon: not null }                                => Refuse("NamedArgument", argument, refusals),
				_                                                      => Lowering(argument.Expression, refusals),
			};
		}

		return arguments;
	}

	static Fragment Binary(BinaryExpressionSyntax binary, List<Fragment.Unportable> refusals)
	{
		FragmentOperator? operation = binary.Kind() switch
		{
			SyntaxKind.LogicalAndExpression         => FragmentOperator.And,
			SyntaxKind.LogicalOrExpression          => FragmentOperator.Or,
			SyntaxKind.EqualsExpression             => FragmentOperator.Equal,
			SyntaxKind.NotEqualsExpression          => FragmentOperator.NotEqual,
			SyntaxKind.LessThanExpression           => FragmentOperator.Less,
			SyntaxKind.LessThanOrEqualExpression    => FragmentOperator.LessOrEqual,
			SyntaxKind.GreaterThanExpression        => FragmentOperator.Greater,
			SyntaxKind.GreaterThanOrEqualExpression => FragmentOperator.GreaterOrEqual,
			SyntaxKind.AddExpression                => FragmentOperator.Add,
			SyntaxKind.SubtractExpression           => FragmentOperator.Subtract,
			_                                       => null,
		};

		if (binary.IsKind(SyntaxKind.CoalesceExpression))
			return new Fragment.Coalesce(Lowering(binary.Left, refusals), Lowering(binary.Right, refusals)) { Span = Of(binary.Span) };

		if (operation is not { } known)
			return Refuse(binary.Kind().ToString(), binary, refusals);

		return new Fragment.Binary(known, Lowering(binary.Left, refusals), Lowering(binary.Right, refusals)) { Span = Of(binary.Span) };
	}

	/// <summary>
	/// <c>is null</c> and <c>is not null</c>; every other pattern is C#'s, refused at the pattern.
	/// </summary>
	static Fragment IsNull(IsPatternExpressionSyntax test, List<Fragment.Unportable> refusals)
	{
		var negated = false;
		var pattern = test.Pattern;

		if (pattern is UnaryPatternSyntax { RawKind: (int)SyntaxKind.NotPattern } not)
		{
			negated = true;
			pattern = not.Pattern;
		}

		if (pattern is ConstantPatternSyntax { Expression: LiteralExpressionSyntax { RawKind: (int)SyntaxKind.NullLiteralExpression } })
			return new Fragment.IsNull(Lowering(test.Expression, refusals), negated) { Span = Of(test.Span) };

		var refused = Refuse(test.Pattern.Kind().ToString(), test.Pattern, refusals);

		Lowering(test.Expression, refusals);

		return refused;
	}

	/// <summary>
	/// What a <c>!</c> may stand before and mean the same in every language: a parenthesised
	/// comparison, or a call — a boolean, if anything, whatever its type is.
	/// </summary>
	static bool MayBeNegated(ExpressionSyntax operand)
	{
		if (operand is InvocationExpressionSyntax)
			return true;

		return operand is ParenthesizedExpressionSyntax { Expression: var inner } && inner.Kind() is
			SyntaxKind.EqualsExpression or SyntaxKind.NotEqualsExpression or
			SyntaxKind.LessThanExpression or SyntaxKind.LessThanOrEqualExpression or
			SyntaxKind.GreaterThanExpression or SyntaxKind.GreaterThanOrEqualExpression or
			SyntaxKind.IsPatternExpression;
	}

	static bool IsVerbatim(SyntaxToken identifier)
	{
		return identifier.Text.StartsWith("@", StringComparison.Ordinal);
	}

	/// <summary>
	/// Refuses <paramref name="node"/> as <paramref name="form"/>, and lowers what it holds, so
	/// that a form inside a refused one is refused too: a fragment is portable only once every
	/// form in it is, and the census counts what blocks it, not only what blocks it first.
	/// </summary>
	static Fragment.Unportable Refuse(string form, SyntaxNode node, List<Fragment.Unportable> refusals)
	{
		var refused = Refuse(form, node.Span, refusals);

		Within(node, refusals);

		return refused;

		static void Within(SyntaxNode node, List<Fragment.Unportable> refusals)
		{
			foreach (var child in node.ChildNodes())
			{
				// A type is not a value, and whether one ports is not this tree's question: the
				// `T` of `(T)x`, `default(T)` or `new T[] { … }` adds nothing to the refusal of the
				// form it is written in, nor does the empty size of `[]`.
				if (child is TypeSyntax type && (SyntaxFacts.IsInTypeOnlyContext(type) || node is ArrayCreationExpressionSyntax))
					continue;

				if (child is OmittedArraySizeExpressionSyntax)
					continue;

				// An initializer's `{ … }` and the `A =` of each member it sets are the refused
				// form's own; the values are lowered, and may be refused on their own.
				if (child is InitializerExpressionSyntax)
				{
					Within(child, refusals);

					continue;
				}

				if (node is InitializerExpressionSyntax && child is AssignmentExpressionSyntax { Left: IdentifierNameSyntax or ImplicitElementAccessSyntax } member)
				{
					Lowering(member.Right, refusals);

					continue;
				}

				if (child is ExpressionSyntax expression)
					Lowering(expression, refusals);
				else
					Within(child, refusals);
			}
		}
	}

	static Fragment.Unportable Refuse(string form, Microsoft.CodeAnalysis.Text.TextSpan span, List<Fragment.Unportable> refusals)
	{
		var refused = new Fragment.Unportable(form) { Span = Of(span) };

		refusals.Add(refused);

		return refused;
	}

	static FragmentSpan Of(Microsoft.CodeAnalysis.Text.TextSpan span)
	{
		return new FragmentSpan(span.Start, span.Length);
	}
}
