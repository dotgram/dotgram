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
/// Anything that knows where it came from.
/// </summary>
/// <remarks>
/// The one thing the otherwise unrelated trees have in common, and the only reason
/// they would ever need a common view. Uniform traversal across kinds, should it ever
/// be wanted, is written against this rather than bought with a base class.
/// </remarks>
public interface ILocated
{
	Location At { get; }
}

// ── Products: no alternatives, so nothing to match on ────────────────────────────

public sealed record GrammarFile(IReadOnlyList<Using> Usings, IReadOnlyList<Decl> Decls, Location At) : ILocated
{
	public override string ToString() => Dump.Of(this);
}

public sealed record Using  (bool IsCSharp, string Name, Location At)                     : ILocated;
public sealed record Param  (string Name, TypeRef? Type, Location At)                     : ILocated;
public sealed record TypeRef(bool IsCSharp, string Name, bool IsSequence, Location At)    : ILocated;

// ── Sums: flat, one level of alternatives each ───────────────────────────────────

/// <summary>
/// The two directives of §6, and the whole of the difference between them: whether
/// input that does not match may sit between the matches.
/// </summary>
public enum PublishKind { Parse, Find }

/// <summary>What a grammar file declares.</summary>
public abstract record Decl : ILocated
{
	/// <remarks>
	/// An init property rather than a positional parameter, deliberately: as a
	/// parameter it would appear in every pattern over every case, and the patterns are
	/// what this shape exists for.
	/// </remarks>
	public required Location At { get; init; }

	public sealed record Rule(
		string Name, IReadOnlyList<Param> Params, TypeRef? Type, Expr Body) : Decl;

	public sealed record Scope(
		string Name, IReadOnlyList<Using> Usings, IReadOnlyList<Decl> Decls) : Decl;

	public sealed record Publish(PublishKind Kind, string RuleName, string? Alias) : Decl;

	public sealed override string ToString() => Dump.Of(this);
}

public enum QuantifierKind { Optional, ZeroOrMore, OneOrMore, Count }

/// <summary>The shape of a recognition expression.</summary>
public abstract record Expr : ILocated
{
	public required Location At { get; init; }

	public sealed record Choice    (IReadOnlyList<Expr> Alternatives)          : Expr;
	public sealed record Sequence  (IReadOnlyList<Expr> Operands)              : Expr;
	public sealed record Construct (Expr Pattern, Expr Value)                  : Expr;

	/// <summary>An alternative that states its own binding power (§4.3.1).</summary>
	public sealed record Bound     (Expr Body, bool IsLeft, int Level)         : Expr;

	/// <summary>A repetition that survives a bad element (§8.2).</summary>
	public sealed record Recovering(Expr Body, Expr Sync, Expr? Factory)       : Expr;

	public sealed record Guard     (Expr Value)                                : Expr;
	public sealed record Capture   (string Name, Expr Operand)                 : Expr;
	public sealed record Group     (Expr Body)                                 : Expr;
	public sealed record Atomic    (Expr Body)                                 : Expr;
	public sealed record Lookahead (bool IsPositive, Expr Operand)             : Expr;
	public sealed record Literal   (bool IsChar, string Value)                 : Expr;
	public sealed record ElementSet(bool IsNegated, IReadOnlyList<Elem> Items) : Expr;
	public sealed record CSharp    (string Text)                               : Expr;

	/// <summary>
	/// A number, which is a value rather than a recognizer.
	/// </summary>
	/// <remarks>
	/// Only where a value goes: an argument to a parameterized rule (§4.2). There is no
	/// position in a recognition expression where a bare number means anything, which is
	/// why it is not one of the operands.
	/// </remarks>
	public sealed record Number(int Value) : Expr;

	public sealed record Reference(bool IsCSharp, string Name, IReadOnlyList<TypeRef> TypeArguments) : Expr;
	public sealed record Call     (Reference Target, IReadOnlyList<Expr> Arguments)                  : Expr;

	public sealed record Quantified(
		Expr Operand, QuantifierKind Kind, int? Min, string? MinName, int? Max, string? MaxName) : Expr;

	public sealed override string ToString() => Dump.Of(this);
}

/// <summary>One member of an element set.</summary>
public abstract record Elem : ILocated
{
	public required Location At { get; init; }

	public sealed record Chars   (string From, string? To)  : Elem;
	public sealed record Category(string Name)              : Elem;
	public sealed record Ref     (Expr.Reference Reference) : Elem;
}

// ── Rendering, in one place rather than spread over every case ───────────────────

static class Dump
{
	public static string Of(GrammarFile file)
	{
		var text = new StringBuilder().AppendEndingWith("File");

		foreach (var import in file.Usings)
			Write(text, 1, Label(import));

		foreach (var declaration in file.Decls)
			Write(text, 1, declaration);

		return text.ToString().TrimEnd();
	}

	public static string Of(Decl declaration)
	{
		var text = new StringBuilder();

		Write(text, 0, declaration);

		return text.ToString().TrimEnd();
	}

	public static string Of(Expr expression)
	{
		var text = new StringBuilder();

		Write(text, 0, expression);

		return text.ToString().TrimEnd();
	}

	static void Write(StringBuilder text, int depth, string label) =>
		text.Append('\t', depth).AppendEndingWith(label);

	static void Write(StringBuilder text, int depth, Decl declaration)
	{
		switch (declaration)
		{
			case Decl.Rule(var name, var parameters, var type, var body):

				Write(text, depth, $"Rule {Quote(name)}");

				foreach (var parameter in parameters)
					Write(text, depth + 1, Label(parameter));

				if (type is not null)
					Write(text, depth + 1, Label(type));

				Write(text, depth + 1, body);
				break;

			case Decl.Scope(var name, var usings, var declarations):

				Write(text, depth, $"Scope {Quote(name)}");

				foreach (var import in usings)
					Write(text, depth + 1, Label(import));

				foreach (var nested in declarations)
					Write(text, depth + 1, nested);

				break;

			case Decl.Publish(var kind, var rule, var alias):

				Write(text, depth, alias is null
					? $"Publication {kind} {Quote(rule)}"
					: $"Publication {kind} {Quote(rule)} as {Quote(alias)}");

				break;
		}
	}

	static void Write(StringBuilder text, int depth, Expr expression)
	{
		Write(text, depth, Label(expression));

		foreach (var child in Children(expression))
			Write(text, depth + 1, child);

		if (expression is Expr.ElementSet(_, var items))
			foreach (var item in items)
				Write(text, depth + 1, Label(item));

		if (expression is Expr.Reference(_, _, var typeArguments))
			foreach (var typeArgument in typeArguments)
				Write(text, depth + 1, Label(typeArgument));
	}

	/// <summary>Sub-expressions, in source order. The one place traversal is defined.</summary>
	public static IReadOnlyList<Expr> Children(Expr expression) => expression switch
	{
		Expr.Choice(var alternatives)       => alternatives,
		Expr.Sequence(var operands)         => operands,
		Expr.Construct(var pattern, var value) => [pattern, value],
		Expr.Guard(var value)               => [value],
		Expr.Capture(_, var operand)        => [operand],
		Expr.Bound(var body, _, _)          => [body],
		Expr.Recovering(var body, var sync, null)         => [body, sync],
		Expr.Recovering(var body, var sync, var factory)  => [body, sync, factory],
		Expr.Group(var body)                => [body],
		Expr.Atomic(var body)               => [body],
		Expr.Lookahead(_, var operand)      => [operand],
		Expr.Quantified(var operand, _, _, _, _, _) => [operand],
		Expr.Call(var target, var arguments) => [target, .. arguments],
		_                                   => [],
	};

	public static string Label(Using import) =>
		$"Using{(import.IsCSharp ? " (C#)" : "")} {Quote(import.Name)}";

	public static string Label(Param parameter) => $"Parameter {Quote(parameter.Name)}";

	public static string Label(TypeRef type) =>
		$"Type{(type.IsCSharp ? " (C#)" : "")} {Quote(type.Name)}{(type.IsSequence ? "[]" : "")}";

	public static string Label(Expr expression) => expression switch
	{
		Expr.Choice                               => "Choice",
		Expr.Construct                            => "Alternative",
		Expr.Bound(_, var isLeft, var level)      => (isLeft ? "Left " : "Right ") + level,
		Expr.Recovering                           => "Recovering",
		Expr.Sequence                             => "Sequence",
		Expr.Guard                                => "Guard",
		Expr.Capture(var name, _)                 => $"Capture {Quote(name)}",
		Expr.Group                                => "Group",
		Expr.Atomic                               => "Atomic",
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

	public static string Label(Elem item) => item switch
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
