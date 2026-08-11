using System;
using System.Collections.Generic;
using System.Text;

namespace DotGram.Grammar.Syntax;

/// <summary>Base of everything the parser produces.</summary>
/// <remarks>
/// One node type per production in docs/syntax.md §9. The tree is deliberately close to
/// the notation rather than to what recognition needs: normalizing into a recognition
/// graph is a later stage with its own contract, and keeping the two apart is what
/// lets a parsing regression be told apart from a normalization one.
/// </remarks>
public abstract record SyntaxNode(int Position, int Length)
{
	/// <summary>Children, in source order. Empty for leaves.</summary>
	public virtual IReadOnlyList<SyntaxNode> Children => [];

	/// <summary>The node's own label in a dump — kind, plus what distinguishes it.</summary>
	protected abstract string Label { get; }

	public sealed override string ToString()
	{
		var text = new StringBuilder();

		Write(text, 0);

		return text.ToString().TrimEnd();
	}

	void Write(StringBuilder text, int depth)
	{
		text.Append('\t', depth).AppendLine(Label);

		foreach (var child in Children)
			child.Write(text, depth + 1);
	}

	protected static string Quote(string value) =>
		$"\"{value.Replace("\\", @"\\").Replace("\"", "\\\"")}\"";
}

// ── File and declarations ────────────────────────────────────────────────────────

public sealed record GrammarFile(
	IReadOnlyList<UsingDirective> Usings,
	IReadOnlyList<Declaration>    Declarations,
	int Position, int Length) : SyntaxNode(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [.. Usings, .. Declarations];
	protected override string Label => "File";
}

/// <param name="IsCSharp">True for <c>@using</c>, false for a grammar-scope import.</param>
public sealed record UsingDirective(bool IsCSharp, string Name, int Position, int Length)
	: SyntaxNode(Position, Length)
{
	protected override string Label => $"Using{(IsCSharp ? " (C#)" : "")} {Quote(Name)}";
}

public abstract record Declaration(int Position, int Length) : SyntaxNode(Position, Length);

public sealed record ScopeDeclaration(
	string                        Name,
	IReadOnlyList<UsingDirective> Usings,
	IReadOnlyList<Declaration>    Declarations,
	int Position, int Length) : Declaration(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [.. Usings, .. Declarations];
	protected override string Label => $"Scope {Quote(Name)}";
}

public enum PublicationKind { Parse, Match, Find, FindAll }

public sealed record PublicationDirective(
	PublicationKind Kind, string RuleName, string? Alias,
	int Position, int Length) : Declaration(Position, Length)
{
	protected override string Label =>
		$"Publication {Kind} {Quote(RuleName)}" + (Alias is null ? "" : $" as {Quote(Alias)}");
}

public sealed record RuleDeclaration(
	string                             Name,
	IReadOnlyList<ParameterDeclaration> Parameters,
	TypeReference?                     Type,
	Expression                         Body,
	int Position, int Length) : Declaration(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children =>
		[.. Parameters, .. Type is null ? Array.Empty<SyntaxNode>() : [Type], Body];

	protected override string Label => $"Rule {Quote(Name)}";
}

/// <param name="Type">Null when the parameter takes a recognizer of unconstrained type.</param>
public sealed record ParameterDeclaration(string Name, TypeReference? Type, int Position, int Length)
	: SyntaxNode(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => Type is null ? [] : [Type];
	protected override string Label => $"Parameter {Quote(Name)}";
}

/// <param name="IsSequence">True for <c>T[]</c>.</param>
public sealed record TypeReference(bool IsCSharp, string Name, bool IsSequence, int Position, int Length)
	: SyntaxNode(Position, Length)
{
	protected override string Label =>
		$"Type{(IsCSharp ? " (C#)" : "")} {Quote(Name)}{(IsSequence ? "[]" : "")}";
}

// ── Expressions ──────────────────────────────────────────────────────────────────

public abstract record Expression(int Position, int Length) : SyntaxNode(Position, Length);

public sealed record ChoiceExpression(IReadOnlyList<Expression> Alternatives, int Position, int Length)
	: Expression(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [.. Alternatives];
	protected override string Label => "Choice";
}

/// <param name="Construction">The <c>=&gt;</c> part, when written.</param>
public sealed record AlternativeExpression(Expression Pattern, Expression? Construction, int Position, int Length)
	: Expression(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children =>
		Construction is null ? [Pattern] : [Pattern, Construction];

	protected override string Label => "Alternative";
}

public sealed record SequenceExpression(IReadOnlyList<Expression> Operands, int Position, int Length)
	: Expression(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [.. Operands];
	protected override string Label => "Sequence";
}

public sealed record GuardExpression(Expression Value, int Position, int Length)
	: Expression(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [Value];
	protected override string Label => "Guard";
}

public enum QuantifierKind { Optional, ZeroOrMore, OneOrMore, Count }

/// <param name="Min">Literal lower bound, or null when it is a parameter name.</param>
/// <param name="MinName">Parameter naming the lower bound, as in <c>{n}</c>.</param>
public sealed record QuantifiedExpression(
	Expression Operand, QuantifierKind Kind,
	int? Min, string? MinName, int? Max, string? MaxName,
	int Position, int Length) : Expression(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [Operand];

	protected override string Label => Kind switch
	{
		QuantifierKind.Optional   => "Optional",
		QuantifierKind.ZeroOrMore => "ZeroOrMore",
		QuantifierKind.OneOrMore  => "OneOrMore",
		_                         => $"Count {Bound(Min, MinName)}..{Bound(Max, MaxName) ?? "*"}",
	};

	static string? Bound(int? value, string? name) => name ?? value?.ToString();
}

public sealed record LookaheadExpression(bool IsPositive, Expression Operand, int Position, int Length)
	: Expression(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [Operand];
	protected override string Label => IsPositive ? "PositiveLookahead" : "NegativeLookahead";
}

public sealed record CaptureExpression(string Name, Expression Operand, int Position, int Length)
	: Expression(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [Operand];
	protected override string Label => $"Capture {Quote(Name)}";
}

public sealed record GroupExpression(Expression Body, int Position, int Length)
	: Expression(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [Body];
	protected override string Label => "Group";
}

public sealed record LiteralExpression(bool IsCharacter, string Value, int Position, int Length)
	: Expression(Position, Length)
{
	protected override string Label => $"{(IsCharacter ? "Char" : "String")} {Quote(Value)}";
}

public sealed record ElementSetExpression(
	bool IsNegated, IReadOnlyList<ElementSetItem> Items, int Position, int Length)
	: Expression(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [.. Items];
	protected override string Label => IsNegated ? "ElementSet (negated)" : "ElementSet";
}

public sealed record ReferenceExpression(
	bool IsCSharp, string Name, IReadOnlyList<TypeReference> TypeArguments,
	int Position, int Length) : Expression(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [.. TypeArguments];
	protected override string Label => $"Reference{(IsCSharp ? " (C#)" : "")} {Quote(Name)}";
}

public sealed record CallExpression(
	ReferenceExpression Target, IReadOnlyList<Expression> Arguments, int Position, int Length)
	: Expression(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [Target, .. Arguments];
	protected override string Label => "Call";
}

public sealed record CSharpExpression(string Text, int Position, int Length)
	: Expression(Position, Length)
{
	protected override string Label => $"CSharp {Quote(Text)}";
}

// ── Element-set items ────────────────────────────────────────────────────────────

public abstract record ElementSetItem(int Position, int Length) : SyntaxNode(Position, Length);

/// <param name="To">Null for a single character rather than a range.</param>
public sealed record CharacterRangeItem(string From, string? To, int Position, int Length)
	: ElementSetItem(Position, Length)
{
	protected override string Label =>
		To is null ? $"Char {Quote(From)}" : $"Range {Quote(From)}..{Quote(To)}";
}

public sealed record UnicodeCategoryItem(string Category, int Position, int Length)
	: ElementSetItem(Position, Length)
{
	protected override string Label => $"Category {Quote(Category)}";
}

public sealed record ReferenceItem(ReferenceExpression Reference, int Position, int Length)
	: ElementSetItem(Position, Length)
{
	public override IReadOnlyList<SyntaxNode> Children => [Reference];
	protected override string Label => "Item";
}
