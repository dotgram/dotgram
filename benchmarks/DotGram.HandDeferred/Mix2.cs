using System;

namespace DotGram.HandDeferred;

/// <summary>
/// <c>Deferred.gram</c> with the same rule as <see cref="Mixed"/> — a value where a value
/// will do, a reference where it will not — applied a shape at a time rather than a field
/// at a time.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Mixed"/> keeps <c>Pair</c> a struct and pays for the cycle at one field,
/// which means the run of steps has to live somewhere: an array, grown per parse, which is
/// what walks into the large object heap at a thousand pairs. Here the decision is made
/// about the type instead. <c>Pair</c> sits on the cycle <c>Sum → Pair → Sum</c>, so
/// <c>Pair</c> is a class; and once it is, the run of steps needs no array at all, because
/// the elements can carry the run themselves.
/// </para>
/// <para>
/// That is the shape a generator would emit if it asked, of every rule, whether it can be
/// reached from itself. The ones that cannot stay structs and are held by value —
/// <c>Name</c>, <c>Digits</c>, and <c>Only</c>, which is eight bytes of reference once
/// <c>Pair</c> is one. The ones that can become classes, and there are two of them here.
/// </para>
/// <para>
/// <b>What it trades.</b> More bytes, in smaller pieces: a <c>Pair</c> is an object of
/// forty-eight bytes where <see cref="Mixed"/> writes twenty-four into an array. Nothing
/// it allocates is ever large, so nothing it allocates is ever a gen2 collection, which is
/// the whole of what went wrong for the array readings at ten thousand pairs.
/// </para>
/// <para>
/// The other way out of that is to stop allocating the array per parse — rent it, return
/// it, and never grow it twice. <see cref="Mixed"/> is left alone so the two answers can
/// be compared as they are.
/// </para>
/// </remarks>
sealed class Mix2 : IReading
{
	/// <summary>
	/// <c>Pair</c>, as a class, because it is on the cycle — and a link in the fold's run,
	/// because once it is a class the run is free.
	/// </summary>
	/// <remarks>
	/// <c>Next</c> is not part of the grammar. It is the generator noticing that §4.3 turned
	/// the left recursion into a run of <c>Pair</c>, and that a run of references can be
	/// threaded through its own elements rather than laid beside them.
	/// </remarks>
	sealed class Pair
	{
		readonly Name   _name;
		readonly Digits _value;
		readonly Sum?   _inner;

		public Pair? Next;

		public Pair(Name name, Digits value)
		{
			_name  = name;
			_value = value;
			_inner = null;
		}

		public Pair(Sum inner)
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

	/// <summary>
	/// <c>Sum = one: Pair =&gt; @(one)</c>, still a struct: it holds one reference and is
	/// reached from nothing.
	/// </summary>
	readonly struct Only
	{
		readonly Pair _one;

		public Only(Pair one) => _one = one;

		public string Build(string text) => Author.Only(_one.Build(text));
	}

	/// <summary><c>Sum</c> entire: the base of the fold, and the run threaded over it.</summary>
	sealed class Sum
	{
		readonly Only  _base;
		readonly Pair? _steps;
		readonly int   _count;

		public Sum(Only one, Pair? steps, int count)
		{
			_base  = one;
			_steps = steps;
			_count = count;
		}

		public string Build(string text)
		{
			var value = _base.Build(text);
			var step  = _steps;

			// Counted rather than tested against null: the count is known at the end of
			// reading and costs nothing to keep, and the loop then has one exit.
			for (var i = 0; i < _count; i++, step = step!.Next)
				value = Author.Step(value, step!.Build(text));

			return value;
		}
	}

	readonly string _text;

	Sum? _root;

	public Mix2(string text) => _text = text;

	/// <summary>Reads the whole input, keeping what it read and building none of it.</summary>
	public bool Recognize()
	{
		var end = Read_Sum(Skip(0), out _root);

		return end >= 0 && Skip(end) == _text.Length;
	}

	/// <summary>Runs the fold: the base, then a step per link.</summary>
	public string Construct() => _root!.Build(_text);

	// ---- reading -----------------------------------------------------------------------

	int Read_Sum(int at, out Sum? made)
	{
		made = null;

		var end = Read_Pair(at, out var one);

		if (end < 0)
			return -1;

		Pair? first = null;
		Pair? last  = null;

		var count = 0;

		while (true)
		{
			var plus = Skip(end);

			if (plus >= _text.Length || _text[plus] != '+')
				break;

			var next = Read_Pair(Skip(plus + 1), out var right);

			if (next < 0)
				break;

			if (last is null) first     = right;
			else              last.Next = right;

			last = right;

			count++;

			end = next;
		}

		made = new Sum(new Only(one!), first, count);

		return end;
	}

	/// <summary><c>Pair</c>, the binding or a parenthesised <c>Sum</c>.</summary>
	int Read_Pair(int at, out Pair? made)
	{
		made = null;

		if (at < _text.Length && _text[at] == '(')
		{
			var inside = Read_Sum(Skip(at + 1), out var sum);

			if (inside < 0)
				return -1;

			var close = Skip(inside);

			if (close >= _text.Length || _text[close] != ')')
				return -1;

			made = new Pair(sum!);

			return close + 1;
		}

		var end = Read_Name(at, out var name);

		if (end < 0)
			return -1;

		var sign = Skip(end);

		// The place the whole project is about. `Read_Name` answered with two integers in a
		// local, and the derivation is abandoned by not using it — the same as
		// <see cref="Mixed"/>, because the leaves are structs in both.
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
}
