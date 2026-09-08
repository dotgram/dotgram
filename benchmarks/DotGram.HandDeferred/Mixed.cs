using System;
using System.Runtime.CompilerServices;

namespace DotGram.HandDeferred;

/// <summary>
/// <c>Pair : @string = name: Name &amp; '=' &amp; value: Digits =&gt; @(name + ":" + value)</c>
/// or <c>'(' &amp; inner: Sum &amp; ')'</c>, whichever it read. Twenty-four bytes.
/// </summary>
/// <remarks>
/// The children are named rather than parameters, because the grammar names them. Written
/// <c>Pair&lt;TName, TValue&gt;</c> it would compile to the same layout and the same
/// instructions — the JIT gives a struct type argument its own copy — and say less.
/// <para>
/// The choice needs no tag of its own: a <c>Pair</c> that read a parenthesis has an
/// <c>_inner</c> and one that read a binding has not, and a null check is the whole of the
/// discrimination.
/// </para>
/// </remarks>
readonly struct Pair
{
	readonly Name   _name;
	readonly Digits _value;
	readonly Sums?  _inner;

	public Pair(Name name, Digits value)
	{
		_name  = name;
		_value = value;
		_inner = null;
	}

	public Pair(Sums inner)
	{
		_name  = default;
		_value = default;
		_inner = inner;
	}

	public string Build(string text) =>
		_inner is null
			? Author.Pair  (_name.Build(text), _value.Build(text))
			: Author.Nested(_inner.Build(text));
}

/// <summary><c>Sum = one: Pair =&gt; @(one)</c>, the base of the fold.</summary>
readonly struct Only
{
	readonly Pair _one;

	public Only(Pair one) => _one = one;

	public string Build(string text) => Author.Only(_one.Build(text));
}

/// <summary>
/// <c>Sum</c> entire: the base of the fold and the run of steps over it — and the one
/// reference type in this reading.
/// </summary>
/// <remarks>
/// <para>
/// It is a class because it has to be. <c>Sum</c> reaches back into itself through
/// <c>Pair</c>'s parenthesised alternative, so a <c>Sum</c> held inside a <c>Pair</c> held
/// inside a <c>Sum</c> is a struct that contains itself, which has no size. Something on
/// this cycle must be a reference, and this is the smallest thing that can be: one object
/// per <c>Sum</c> actually read, not one per node.
/// </para>
/// <para>
/// The fold itself stays flat. <c>Step</c> was never written down at all — a step is an
/// element of <c>_steps</c>, and building is the loop in <see cref="Build"/>, so the left
/// recursion costs neither a type nor a stack frame.
/// </para>
/// </remarks>
sealed class Sums
{
	readonly Only    _base;
	readonly Pair[]? _steps;
	readonly int     _count;

	public Sums(Only one, Pair[]? steps, int count)
	{
		_base  = one;
		_steps = steps;
		_count = count;
	}

	public string Build(string text)
	{
		var value = _base.Build(text);

		for (var i = 0; i < _count; i++)
			value = Author.Step(value, _steps![i].Build(text));

		return value;
	}
}

/// <summary>
/// <c>Deferred.gram</c> with the deferred call carried as a value wherever a value will
/// do, and as a reference exactly where it will not.
/// </summary>
/// <remarks>
/// <para>
/// The idea is to keep <see cref="Closures"/>'s shape — a rule answers with the value it
/// read, not yet built — and pay nothing for it: no display class, no delegate, no pointer
/// to chase, and no rewind either, because a shape that is abandoned was a local and is
/// simply not used. <c>Pair</c> is twenty-four bytes with its children inside it, against
/// the five records of twelve bytes the tape writes for the same pair.
/// </para>
/// <para>
/// <b>Where the wall is.</b> Every shape here names its children, because the grammar
/// does. <c>Step</c> cannot, and neither can the parenthesis. A step's left child is the
/// sum so far — <c>Only</c> for one pair, a step of that for two — and a parenthesis holds
/// a whole <c>Sum</c>, which holds pairs, which may hold parentheses. Both types grow with
/// the input, and no program names a type whose depth it learns at run time. Making them
/// generic does not help; it moves the naming to the call site, which has the same
/// problem.
/// </para>
/// <para>
/// <b>What each of the two costs.</b> They are not the same problem and they do not have
/// the same answer. The fold is a loop, and a loop is over one element type: the sum is a
/// base and a run of <c>Pair</c>, all of one type, an array of them by value, and no
/// indirection at all. §4.3 had already turned the left recursion into that loop and the
/// generator knows it did. The parenthesis is not a loop and there is nothing to turn it
/// into: the cycle has to be cut, and cutting it costs one reference. So the boxing that
/// looked like the price of the whole technique is the price of one field in one
/// alternative — one object per <c>Sum</c> that was actually parenthesised, where
/// <see cref="Boxed"/> and <see cref="Classes"/> pay one per node.
/// </para>
/// <para>
/// <see cref="Arenas"/> is the same reading with even that reference taken out, to see
/// what the cut is worth when it is made with an index instead.
/// </para>
/// </remarks>
sealed class Mixed : IReading
{
	readonly string _text;

	Sums? _root;

	public Mixed(string text) => _text = text;

	/// <summary>Reads the whole input, keeping what it read as values and building none of it.</summary>
	public bool Recognize()
	{
		var end = Read_Sum(Skip(0), out _root);

		return end >= 0 && Skip(end) == _text.Length;
	}

	/// <summary>Runs the fold for real: the base, then a step per element.</summary>
	public string Construct() => _root!.Build(_text);

	// ---- reading -----------------------------------------------------------------------

	int Read_Sum(int at, out Sums? made)
	{
		made = null;

		var end = Read_Pair(at, out var one);

		if (end < 0)
			return -1;

		var start = new Only(one);

		// Left null until there is a step to put in it, so a sum of one pair is one object
		// and not two.
		Pair[]? steps = null;

		var count = 0;

		while (true)
		{
			var plus = Skip(end);

			if (plus >= _text.Length || _text[plus] != '+')
				break;

			var next = Read_Pair(Skip(plus + 1), out var right);

			if (next < 0)
				break;

			if (steps is null)                steps = new Pair[4];
			else if (count == steps.Length)   Array.Resize(ref steps, count * 2);

			steps[count++] = right;
			end            = next;
		}

		made = new Sums(start, steps, count);

		return end;
	}

	/// <summary><c>Pair</c>, the binding or a parenthesised <c>Sum</c>.</summary>
	int Read_Pair(int at, out Pair made)
	{
		made = default;

		if (at < _text.Length && _text[at] == '(')
		{
			var inside = Read_Sum(Skip(at + 1), out var sum);

			if (inside < 0)
				return -1;

			var close = Skip(inside);

			// The `Sum` inside is dropped by not being used, object and all. It is the one
			// place this reading leaves anything for the collector.
			if (close >= _text.Length || _text[close] != ')')
				return -1;

			made = new Pair(sum!);

			return close + 1;
		}

		var end = Read_Name(at, out var name);

		if (end < 0)
			return -1;

		var sign = Skip(end);

		// The place the whole project is about, and here there is nothing at all to undo.
		// What `Read_Name` answered is a local holding two integers; the derivation is
		// abandoned by not using it. The tape rewinds a counter and the closures leave
		// garbage; this leaves the stack pointer where it was.
		if (sign >= _text.Length || _text[sign] != '=')
			return -1;

		end = Read_Digits(Skip(sign + 1), out var value);

		if (end < 0)
			return -1;

		made = new Pair(name, value);

		return end;
	}

	int Read_Name(int at, out Name made)
	{
		var end = at;

		while (end < _text.Length && _text[end] is >= 'a' and <= 'z')
			end++;

		made = new Name(at, end);

		return end == at ? -1 : end;
	}

	int Read_Digits(int at, out Digits made)
	{
		var end = at;

		while (end < _text.Length && _text[end] is >= '0' and <= '9')
			end++;

		made = new Digits(at, end);

		return end == at ? -1 : end;
	}

	/// <summary><c>trivia = ' '*</c>, at every seam, and it never gives back.</summary>
	int Skip(int at)
	{
		var end = at;

		while (end < _text.Length && _text[end] == ' ')
			end++;

		return end;
	}

	// ---- for reading over ---------------------------------------------------------------

	/// <summary>What the shapes weigh, which is the argument for them in one line.</summary>
	public static string Sizes() =>
		$"Name {Unsafe.SizeOf<Name>()} B, " +
		$"Digits {Unsafe.SizeOf<Digits>()} B, " +
		$"Pair {Unsafe.SizeOf<Pair>()} B, " +
		$"Only {Unsafe.SizeOf<Only>()} B";
}
