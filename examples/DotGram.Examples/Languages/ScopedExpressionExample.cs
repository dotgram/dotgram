using System;
using System.Collections.Generic;

using DotGram;

namespace DotGram.Examples.Languages;

// An expression language where a name has to be declared before it is used:
//
//     let x = 2; let y = x + 3; y * x        →  10
//     y + 1                                  →  no match: nothing declared y
//
// Both of those are the same shape of text. What tells them apart is not the grammar —
// it is what the reading has worked out by the time it reaches the name, and there is
// nowhere in a parse to keep that. A `=>` runs after the whole match, so it is too late;
// a `static` field would be shared by every thread and every parse, which is a bug
// waiting for the second one.
//
// So the grammar declares somewhere to put it (docs/syntax.md §7.7):
//
//     context : @Names
//
// The type is a C# name this notation never resolves — it is written into the generated
// signature and checked where it is written. The caller makes one and hands it over, and
// every publication takes it:
//
//     Scoped.ParseProgram(text, new Names())
//
// `context` is then a name a `when` or a `=>` may use, and a hook that does not name it
// is not passed it: a grammar that declares a context and never uses one is compiled
// exactly like a grammar that declares none.
//
// The two hooks are on either side of the match, which is the whole of why there are two.
// A `when` runs while the text is being read, in the order it is written — so `Let`
// records the name as it passes it, and every use after that can be checked. A `=>` runs
// once the rule has accepted, which is where a value is built.
//
// A guard that answers false is an alternative that did not match, not an error: ordered
// choice goes on to the next one. Here the name alternative is the last, so a name nobody
// declared ends the parse — and `TryParseProgram` says where.

/// <summary>What the reading works out: the names, and what each was bound to.</summary>
/// <remarks>
/// Ordinary C#, with no idea a parser exists. The grammar names the type and calls the
/// methods; what the type does about it is this file's business.
/// </remarks>
public sealed class Names
{
	readonly Dictionary<string, int> _values = new(StringComparer.Ordinal);

	/// <summary>Records a binding, and answers true so a `when` may call it.</summary>
	public bool Declare(string name, int value)
	{
		_values[name] = value;

		return true;
	}

	/// <summary>Whether anything has been bound to this name yet.</summary>
	public bool Known(string name) => _values.ContainsKey(name);

	/// <summary>What it was bound to.</summary>
	public int Of(string name) => _values[name];
}

[Gram("""
	@using DotGram.Examples.Languages;

	using Std;

	context : @Names

	trivia = Blank?

	Program : @int = Let* & value: Expr & eof => @(value)

	// Read left to right, so the `when` runs after the value it records is known and
	// before anything that could use the name.
	Let = "let" & name: Identifier & '=' & value: Expr & ';'
	    & when @(context.Declare(name, value))

	Expr : @int = left: Expr & '+' & right: Expr  << 1 => @(left + right)
	            | left: Expr & '*' & right: Expr  << 2 => @(left * right)
	            | '(' & inner: Expr & ')'              => @(inner)
	            | n: Integer                           => @(n)
	            // The `!` is not decoration. A guard is handed every capture of the rule
	            // it stands in — here `n` as well — and the ones another alternative
	            // would have set arrive as null, so the compiler cannot know this is the
	            // alternative that set `name`. A `=>` runs after one alternative has been
	            // chosen and needs none.
	            | name: Identifier & when @(context.Known(name!)) => @(context.Of(name))

	parse Program
	""")]
public static partial class Scoped
{
	/// <summary>What a program works out to, or the reason it does not.</summary>
	public static string Explain(string program)
	{
		var answer = TryParseProgram(program, new Names());

		return answer.IsSuccess
			? program + " = " + answer.Value
			: answer.Error + " at " + answer.Position;
	}
}
