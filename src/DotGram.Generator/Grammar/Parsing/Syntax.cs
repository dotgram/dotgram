using System;
using System.Collections.Generic;
using System.Text;

namespace DotGram.Grammar.Parsing;

/// <summary>Where something is in the grammar text.</summary>
public readonly record struct Location(int Position, int Length)
{
	public int End => Position + Length;

	public override string ToString() => $"[{Position}..{End})";
}

/// <summary>
/// A node: where it is, and what is under it.
/// </summary>
/// <remarks>
/// <para>
/// Position and shape are two axes, kept apart on purpose. This tree carries the
/// first — a location and a uniform way to walk children — while what a node <i>is</i>
/// lives in a parallel flat sum, <see cref="Decl"/> or <see cref="Expr"/>, aggregated
/// rather than inherited.
/// </para>
/// <para>
/// Fusing them looks tidier and is not. A pattern over the shape alone reads
/// <c>Choice([Literal(_, var a), Literal(_, var b)])</c>; the same pattern with a
/// location woven through every level is unreadable past the second one — and the
/// checks that matter here, shadowed prefixes and nullable repetition bodies, are all
/// several levels deep.
/// </para>
/// <para>
/// Where a location is needed for a shape several levels down, both are at hand:
/// <c>Children[i]</c> corresponds to the i-th sub-shape, and that correspondence is
/// established once, where the node is built.
/// </para>
/// <para>
/// Only sums are split this way. A product — a parameter, a type name, an import — has
/// no alternatives to match on, so it carries its location inline and needs no
/// counterpart here.
/// </para>
/// </remarks>
public abstract record Syntax(Location At, IReadOnlyList<Syntax> Children)
{
	public sealed record Declaration(Decl What, Location At, IReadOnlyList<Syntax> Children)
		: Syntax(At, Children);

	public sealed record Expression(Expr What, Location At, IReadOnlyList<Syntax> Children)
		: Syntax(At, Children);

	/// <summary>One node per line, indented.</summary>
	public sealed override string ToString()
	{
		var text = new StringBuilder();

		Render(this, text, 0);

		return text.ToString().TrimEnd();
	}

	internal static void Render(Syntax node, StringBuilder text, int depth)
	{
		switch (node)
		{
			case Declaration(Decl.Rule(var name, var parameters, var type, _), _, _):

				text.Append('\t', depth).Append("Rule ").AppendLine(Dump.Quote(name));

				foreach (var parameter in parameters)
					Dump.Write(parameter, text, depth + 1);

				if (type is not null)
					Dump.Write(type, text, depth + 1);

				break;

			case Declaration(Decl.Scope(var name, var usings, _), _, _):

				text.Append('\t', depth).Append("Scope ").AppendLine(Dump.Quote(name));

				foreach (var import in usings)
					text.Append('\t', depth + 1).AppendLine(Dump.Of(import));

				break;

			case Declaration(var what, _, _):
				text.Append('\t', depth).AppendLine(Dump.Of(what));
				break;

			case Expression(Expr.ElementSet(var negated, var items), _, _):

				text.Append('\t', depth).AppendLine(negated ? "ElementSet (negated)" : "ElementSet");

				foreach (var item in items)
					Dump.Write(item, text, depth + 1);

				break;

			case Expression(var what, _, _):
				text.Append('\t', depth).AppendLine(Dump.Of(what));
				break;
		}

		foreach (var child in node.Children)
			Render(child, text, depth + 1);
	}
}

// ── Products: no alternatives, so no split and no counterpart node ───────────────

public sealed record GrammarFile(
	IReadOnlyList<Using> Usings, IReadOnlyList<Syntax.Declaration> Decls, Location At)
{
	public override string ToString()
	{
		var text = new StringBuilder().AppendLine("File");

		foreach (var import in Usings)
			text.Append('\t').AppendLine(Dump.Of(import));

		foreach (var declaration in Decls)
			Syntax.Render(declaration, text, 1);

		return text.ToString().TrimEnd();
	}
}

public sealed record Using  (bool IsCSharp, string Name, Location At);
public sealed record Param  (string Name, TypeRef? Type, Location At);
public sealed record TypeRef(bool IsCSharp, string Name, bool IsSequence, Location At);

// ── Sums: flat, one level of alternatives each ───────────────────────────────────

public enum PublishKind { Parse, Match, Find, FindAll }

public abstract record Decl
{
	public sealed record Rule(
		string Name, IReadOnlyList<Param> Params, TypeRef? Type, Syntax.Expression Body) : Decl;

	public sealed record Scope(
		string Name, IReadOnlyList<Using> Usings, IReadOnlyList<Syntax.Declaration> Decls) : Decl;

	public sealed record Publish(PublishKind Kind, string RuleName, string? Alias) : Decl;
}

public enum QuantifierKind { Optional, ZeroOrMore, OneOrMore, Count }

/// <summary>
/// The shape of a recognition expression, with no locations in it — which is what
/// makes a deep pattern over it readable.
/// </summary>
public abstract record Expr
{
	public sealed record Choice    (IReadOnlyList<Expr> Alternatives)          : Expr;
	public sealed record Sequence  (IReadOnlyList<Expr> Operands)              : Expr;
	public sealed record Construct (Expr Pattern, Expr Value)                  : Expr;
	public sealed record Guard     (Expr Value)                                : Expr;
	public sealed record Capture   (string Name, Expr Operand)                 : Expr;
	public sealed record Group     (Expr Body)                                 : Expr;
	public sealed record Lookahead (bool IsPositive, Expr Operand)             : Expr;
	public sealed record Literal   (bool IsChar, string Value)                 : Expr;
	public sealed record ElementSet(bool IsNegated, IReadOnlyList<Elem> Items) : Expr;
	public sealed record CSharp    (string Text)                               : Expr;

	public sealed record Reference(bool IsCSharp, string Name, IReadOnlyList<TypeRef> TypeArguments) : Expr;
	public sealed record Call     (Reference Target, IReadOnlyList<Expr> Arguments)                  : Expr;

	public sealed record Quantified(
		Expr Operand, QuantifierKind Kind, int? Min, string? MinName, int? Max, string? MaxName) : Expr;
}

public abstract record Elem
{
	public sealed record Chars   (string From, string? To)  : Elem;
	public sealed record Category(string Name)              : Elem;
	public sealed record Ref     (Expr.Reference Reference) : Elem;
}

// ── Rendering, in one place rather than spread over every node ───────────────────

static class Dump
{
	public static void Write(Param     value, StringBuilder text, int depth) => text.Append('\t', depth).AppendLine(Of(value));
	public static void Write(TypeRef   value, StringBuilder text, int depth) => text.Append('\t', depth).AppendLine(Of(value));
	public static void Write(Elem      value, StringBuilder text, int depth) => text.Append('\t', depth).AppendLine(Of(value));

	public static string Of(Using import) =>
		$"Using{(import.IsCSharp ? " (C#)" : "")} {Quote(import.Name)}";

	public static string Of(Param parameter) => $"Parameter {Quote(parameter.Name)}";

	public static string Of(TypeRef type) =>
		$"Type{(type.IsCSharp ? " (C#)" : "")} {Quote(type.Name)}{(type.IsSequence ? "[]" : "")}";

	public static string Of(Decl declaration) => declaration switch
	{
		Decl.Scope  (var name, _, _)                 => $"Scope {Quote(name)}",
		Decl.Rule   (var name, _, _, _)              => $"Rule {Quote(name)}",
		Decl.Publish(var kind, var rule, null)       => $"Publication {kind} {Quote(rule)}",
		Decl.Publish(var kind, var rule, var alias)  => $"Publication {kind} {Quote(rule)} as {Quote(alias)}",
		_                                            => declaration.GetType().Name,
	};

	public static string Of(Expr expression) => expression switch
	{
		Expr.Choice                               => "Choice",
		Expr.Construct                            => "Alternative",
		Expr.Sequence                             => "Sequence",
		Expr.Guard                                => "Guard",
		Expr.Capture(var name, _)                 => $"Capture {Quote(name)}",
		Expr.Group                                => "Group",
		Expr.Lookahead(true,  _)                  => "PositiveLookahead",
		Expr.Lookahead(false, _)                  => "NegativeLookahead",
		Expr.Literal(true,  var value)            => $"Char {Quote(value)}",
		Expr.Literal(false, var value)            => $"String {Quote(value)}",
		Expr.ElementSet(true,  _)                 => "ElementSet (negated)",
		Expr.ElementSet(false, _)                 => "ElementSet",
		Expr.CSharp(var text)                     => $"CSharp {Quote(text)}",
		Expr.Reference(var isCSharp, var name, _) => $"Reference{(isCSharp ? " (C#)" : "")} {Quote(name)}",
		Expr.Call                                 => "Call",

		Expr.Quantified(_, var kind, var min, var minName, var max, var maxName) => kind switch
		{
			QuantifierKind.Optional   => "Optional",
			QuantifierKind.ZeroOrMore => "ZeroOrMore",
			QuantifierKind.OneOrMore  => "OneOrMore",
			_                         => $"Count {Bound(min, minName)}..{Bound(max, maxName) ?? "*"}",
		},

		_ => expression.GetType().Name,
	};

	public static string Of(Elem item) => item switch
	{
		Elem.Chars(var from, null)   => $"Char {Quote(from)}",
		Elem.Chars(var from, var to) => $"Range {Quote(from)}..{Quote(to)}",
		Elem.Category(var name)      => $"Category {Quote(name)}",
		Elem.Ref                     => "Item",
		_                            => item.GetType().Name,
	};

	static string? Bound(int? value, string? name) => name ?? value?.ToString();

	public static string Quote(string value) =>
		$"\"{value.Replace("\\", @"\\").Replace("\"", "\\\"")}\"";
}
