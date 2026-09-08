using System;

namespace DotGram.HandDeferred;

/// <summary>
/// The half of <c>Deferred.gram</c> the author wrote: one method per <c>=&gt;</c>, and
/// nothing else.
/// </summary>
/// <remarks>
/// <para>
/// Written once and shared, so that the two parsers beside it differ in exactly one thing
/// — how they arrange to call these late — and in nothing else. Small methods on purpose:
/// that is what the generator emits and what the JIT wants, and it is what makes the
/// difference between a jump table and a delegate visible rather than lost in the noise.
/// </para>
/// <para>
/// The counter is not part of a parser. It is here so that <c>Program</c> can show the
/// promise being kept rather than assert it.
/// </para>
/// </remarks>
static class Author
{
	static int _constructions;

	/// <summary>How many constructions have run since <see cref="Forget"/>.</summary>
	public static int Constructions => _constructions;

	/// <summary>Sets the count back to nothing, so that one parse can be watched.</summary>
	public static void Forget() => _constructions = 0;

	/// <summary><c>Name : @string = t: ['a'..'z']+ =&gt; @(t)</c></summary>
	public static string Name(string t)
	{
		_constructions++;

		return t;
	}

	/// <summary><c>Digits : @string = t: ['0'..'9']+ =&gt; @(t)</c></summary>
	public static string Digits(string t)
	{
		_constructions++;

		return t;
	}

	/// <summary><c>Pair : @string = name: Name &amp; '=' &amp; value: Digits =&gt; @(name + ":" + value)</c></summary>
	public static string Pair(string name, string value)
	{
		_constructions++;

		return name + ":" + value;
	}

	/// <summary><c>Pair = '(' &amp; inner: Sum &amp; ')' =&gt; @("(" + inner + ")")</c></summary>
	public static string Nested(string inner)
	{
		_constructions++;

		return "(" + inner + ")";
	}

	/// <summary><c>Sum = one: Pair =&gt; @(one)</c></summary>
	public static string Only(string one)
	{
		_constructions++;

		return one;
	}

	/// <summary><c>Sum = l: Sum &amp; '+' &amp; r: Pair =&gt; @(l + "+" + r)</c></summary>
	public static string Step(string l, string r)
	{
		_constructions++;

		return l + "+" + r;
	}
}
