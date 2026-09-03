using System;
using System.Runtime.CompilerServices;

namespace DotGram.HandDeferred;

/// <summary>What a shape can do once the reading has been accepted.</summary>
/// <remarks>
/// Only the two leaves implement it, and only so that <see cref="Boxed"/> can hold them
/// where it holds everything — behind a reference. Nothing in <see cref="Unboxed"/> calls
/// through it: there the types are known, the fields have them, and the calls are direct.
/// </remarks>
interface IBuilds
{
	string Build(string text);
}

/// <summary><c>Name : @string = t: ['a'..'z']+ =&gt; @(t)</c>, as eight bytes.</summary>
readonly struct Name : IBuilds
{
	readonly int _at;
	readonly int _end;

	public Name(int at, int end)
	{
		_at  = at;
		_end = end;
	}

	public string Build(string text) => Author.Name(text.Substring(_at, _end - _at));
}

/// <summary><c>Digits : @string = t: ['0'..'9']+ =&gt; @(t)</c>, likewise.</summary>
readonly struct Digits : IBuilds
{
	readonly int _at;
	readonly int _end;

	public Digits(int at, int end)
	{
		_at  = at;
		_end = end;
	}

	public string Build(string text) => Author.Digits(text.Substring(_at, _end - _at));
}

/// <summary>
/// <c>Pair : @string = name: Name &amp; '=' &amp; value: Digits =&gt; @(name + ":" + value)</c>,
/// holding both children inside itself. Sixteen bytes.
/// </summary>
/// <remarks>
/// The children are named rather than parameters, because the grammar names them: a
/// <c>Pair</c> is a <c>Name</c> and a <c>Digits</c> and can be nothing else. Written
/// <c>Pair&lt;TName, TValue&gt;</c> it would compile to the same layout and the same
/// instructions — the JIT gives a struct type argument its own copy — and say less.
/// </remarks>
readonly struct Pair
{
	readonly Name   _name;
	readonly Digits _value;

	public Pair(Name name, Digits value)
	{
		_name  = name;
		_value = value;
	}

	public string Build(string text) => Author.Pair(_name.Build(text), _value.Build(text));
}

/// <summary><c>Sum = one: Pair =&gt; @(one)</c>, the base of the fold.</summary>
readonly struct Only
{
	readonly Pair _one;

	public Only(Pair one) => _one = one;

	public string Build(string text) => Author.Only(_one.Build(text));
}

/// <summary>
/// <c>Deferred.gram</c> a third time, with the deferred call as a value rather than an
/// object: a struct per alternative, holding what it captured, nested inside the struct
/// that captured it.
/// </summary>
/// <remarks>
/// <para>
/// The idea is to keep <see cref="Closures"/>'s shape — a rule answers with the value it
/// read, not yet built — and pay nothing for it: no display class, no delegate, no
/// pointer to chase, and no rewind either, because a shape that is abandoned was a local
/// and is simply not used. <c>Pair</c> is sixteen bytes with both children inside it,
/// against the five records of twelve bytes the tape writes for the same pair.
/// </para>
/// <para>
/// <b>And here is the wall.</b> Every shape above names its children, because the grammar
/// does. <c>Step</c> cannot. <c>Sum</c>'s step is <c>l: Sum &amp; '+' &amp; r: Pair</c>, so
/// its left child is the sum so far — <c>Only</c> for one pair, a <c>Step</c> of that for
/// two, a <c>Step</c> of that for three. The type grows with the input, and no program can
/// name a type whose depth it learns at run time. Making it <c>Step&lt;TLeft, TRight&gt;</c>
/// does not help, it only moves the naming to the call site, which has the same problem:
/// this is the one place a type parameter would have been reached for, and the one place it
/// does not reach. The way out is to forget the left child's type and box it, which is
/// <see cref="Boxed"/>.
/// </para>
/// <para>
/// <b>The way round it.</b> A fold is a loop, and a loop is over one element type. The
/// sum is not a leaning tower of <c>Step</c>s but a base and a run of <c>Pair</c> — all of
/// one type, so an array of them, by value, with no boxing anywhere. That is the whole representation: one struct and one array of
/// structs. It works because §4.3 already turned the left recursion into a loop, and the
/// generator knows it did.
/// </para>
/// <para>
/// <b>What it comes to.</b> At four hundred pairs, against the tape:
/// </para>
/// <code>
/// recognize, tape            8.5 us      48,936 B    1.00x
/// recognize, closures       17.3 us     153,624 B    2.04x
/// recognize, structs         5.9 us      16,272 B    0.70x
/// recognize, boxed          11.0 us      44,792 B    1.29x
/// recognize, classes        10.0 us      44,792 B    1.18x
///
/// construct, table          60.5 us     859,192 B    1.00x
/// construct, closures       58.7 us     846,368 B    0.97x
/// construct, structs        54.3 us     846,368 B    0.90x
/// construct, boxed          57.6 us     846,368 B    0.95x
/// construct, classes        57.0 us     846,368 B    0.94x
/// </code>
/// <para>
/// Ahead on both phases and a third of the tape's allocation, which is the one number
/// that is not a few percent. The construct column's floor is the author's own strings —
/// all four readings allocate the same 846 KB there, which is what says the rest of the
/// column is being measured and not the concatenation.
/// </para>
/// <para>
/// So the technique reaches exactly as far as the shape is statically bounded. A grammar
/// that recurses into itself somewhere other than a fold — <c>Expr = '(' &amp; Expr &amp;
/// ')'</c> — has a derivation whose type is as deep as its input, and there the boxing is
/// unavoidable. That is the answer to whether this generalizes: it does not. But falling
/// back is not falling far — <see cref="Boxed"/> is 1.29x and <see cref="Classes"/> 1.18x
/// where this is 0.70x, and both are well ahead of a closure. The cliff is a step.
/// </para>
/// </remarks>
sealed class Unboxed
{
	readonly string _text;

	Only? _base;

	Pair[] _steps = new Pair[8];
	int    _count;

	public Unboxed(string text) => _text = text;

	/// <summary>Reads the whole input, keeping what it read as values and building none of it.</summary>
	public bool Recognize()
	{
		var end = Read_Sum(Skip(0));

		return end >= 0 && Skip(end) == _text.Length;
	}

	/// <summary>The fold, run for real this time: the base, then a step per element.</summary>
	public string Construct()
	{
		var value = _base!.Value.Build(_text);

		for (var i = 0; i < _count; i++)
			value = Author.Step(value, _steps[i].Build(_text));

		return value;
	}

	// ---- reading -----------------------------------------------------------------------

	int Read_Sum(int at)
	{
		var end = Read_Pair(at, out var one);

		if (end < 0)
			return -1;

		_base  = new Only(one);
		_count = 0;

		while (true)
		{
			var plus = Skip(end);

			if (plus >= _text.Length || _text[plus] != '+')
				return end;

			var next = Read_Pair(Skip(plus + 1), out var right);

			if (next < 0)
				return end;

			if (_count == _steps.Length)
				Array.Resize(ref _steps, _count * 2);

			_steps[_count++] = right;
			end              = next;
		}
	}

	/// <summary><c>Pair = name: Name &amp; '=' &amp; value: Digits</c>.</summary>
	int Read_Pair(int at, out Pair made)
	{
		made = default;

		var end = Read_Name(at, out var name);

		if (end < 0)
			return -1;

		var sign = Skip(end);

		// The same place again, and this time there is nothing at all to undo. What
		// `Read_Name` answered is a local holding two integers; the derivation is
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
