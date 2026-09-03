using System;

namespace DotGram.HandDeferred;

/// <summary>
/// <c>Deferred.gram</c> with no reference in the derivation at all: a shape per
/// alternative, an array per shape, and a child named by its index.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Mixed"/> had to cut the cycle <c>Sum → Pair → Sum</c> somewhere, and cut it
/// with a class. The cut is not the argument for a reference, though — it is an argument
/// for <em>indirection</em>, and a reference is only one kind of that. An index is
/// another: four bytes instead of eight, no object header, no allocation per node, and an
/// array of structs holding no references is not walked by the collector at all.
/// </para>
/// <para>
/// So every shape gets its own array and a child is an <c>int</c> into the array its type
/// says it is in. A tag appears in exactly the two places the grammar has a choice —
/// <c>Sum</c> is <c>Only</c> or <c>Step</c>, <c>Pair</c> is a binding or a parenthesis —
/// and it is a byte beside the index, not a method table pointer reached through a
/// dereference. The dispatch is a <c>switch</c> on it and the call after that is direct.
/// </para>
/// <para>
/// <b>Nothing is ever rewound.</b> The tape has to put itself back when a reading fails,
/// because it builds forwards over everything it holds and a record it never wrote must
/// not be there. This builds from the root by index, so a shape nobody points at is never
/// visited: an abandoned derivation leaves dead slots in the arenas and costs nothing but
/// the high-water mark, which a pool amortises. That is a rewind of no instructions at
/// all, against the tape's one and the closures' allocation.
/// </para>
/// <para>
/// <b>What it gives up.</b> The tape's single forward loop. There is no one order across
/// six arrays, so building walks from the root — and a walk has a depth. The fold is kept
/// flat by hand, with the left spine unwound onto a stack that is reused across the whole
/// parse rather than by recursing; what is left is one frame per parenthesis, which is as
/// deep as the input actually nests and not as long as it is.
/// </para>
/// </remarks>
sealed class Arenas : IReading
{
	enum Sums : byte { Only, Step }

	enum Pairs : byte { Binding, Nested }

	/// <summary>Which <c>Sum</c>, and where it is.</summary>
	readonly struct Sum
	{
		public readonly Sums Kind;
		public readonly int  At;

		public Sum(Sums kind, int at)
		{
			Kind = kind;
			At   = at;
		}
	}

	/// <summary>Which <c>Pair</c>, and where it is.</summary>
	readonly struct Pair
	{
		public readonly Pairs Kind;
		public readonly int   At;

		public Pair(Pairs kind, int at)
		{
			Kind = kind;
			At   = at;
		}
	}

	/// <summary><c>Sum = one: Pair =&gt; @(one)</c>.</summary>
	readonly struct Only
	{
		public readonly Pair One;

		public Only(Pair one) => One = one;
	}

	/// <summary><c>Sum = l: Sum &amp; '+' &amp; r: Pair =&gt; @(l + "+" + r)</c>.</summary>
	readonly struct Step
	{
		public readonly Sum  Left;
		public readonly Pair Right;

		public Step(Sum left, Pair right)
		{
			Left  = left;
			Right = right;
		}
	}

	/// <summary><c>Pair = name: Name &amp; '=' &amp; value: Digits</c>.</summary>
	readonly struct Binding
	{
		public readonly int Name;
		public readonly int Digits;

		public Binding(int name, int digits)
		{
			Name   = name;
			Digits = digits;
		}
	}

	/// <summary><c>Pair = '(' &amp; inner: Sum &amp; ')'</c>.</summary>
	readonly struct Nested
	{
		public readonly Sum Inner;

		public Nested(Sum inner) => Inner = inner;
	}

	readonly string _text;

	Name[]    _names    = new Name[8];
	Digits[]  _digits   = new Digits[8];
	Only[]    _onlys    = new Only[8];
	Step[]    _steps    = new Step[8];
	Binding[] _bindings = new Binding[8];
	Nested[]  _nesteds  = new Nested[4];

	int _nameCount;
	int _digitCount;
	int _onlyCount;
	int _stepCount;
	int _bindingCount;
	int _nestedCount;

	Sum _root;

	public Arenas(string text) => _text = text;

	/// <summary>Reads the whole input into the arenas and builds none of it.</summary>
	public bool Recognize()
	{
		var end = Read_Sum(Skip(0), out _root);

		return end >= 0 && Skip(end) == _text.Length;
	}

	// ---- reading -----------------------------------------------------------------------

	int Read_Sum(int at, out Sum made)
	{
		made = default;

		var end = Read_Pair(at, out var one);

		if (end < 0)
			return -1;

		made = new Sum(Sums.Only, Add(ref _onlys, ref _onlyCount, new Only(one)));

		while (true)
		{
			var plus = Skip(end);

			if (plus >= _text.Length || _text[plus] != '+')
				return end;

			var next = Read_Pair(Skip(plus + 1), out var right);

			if (next < 0)
				return end;

			made = new Sum(Sums.Step, Add(ref _steps, ref _stepCount, new Step(made, right)));
			end  = next;
		}
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

			// Everything that `Sum` wrote stays in the arenas, unreferenced and unvisited.
			// Putting it back would cost six counters; leaving it costs the slots.
			if (close >= _text.Length || _text[close] != ')')
				return -1;

			made = new Pair(Pairs.Nested, Add(ref _nesteds, ref _nestedCount, new Nested(sum)));

			return close + 1;
		}

		var end = Read_Name(at, out var name);

		if (end < 0)
			return -1;

		var sign = Skip(end);

		if (sign >= _text.Length || _text[sign] != '=')
			return -1;

		end = Read_Digits(Skip(sign + 1), out var value);

		if (end < 0)
			return -1;

		made = new Pair(Pairs.Binding, Add(ref _bindings, ref _bindingCount, new Binding(name, value)));

		return end;
	}

	int Read_Name(int at, out int made)
	{
		made = -1;

		var end = at;

		while (end < _text.Length && _text[end] is >= 'a' and <= 'z')
			end++;

		if (end == at)
			return -1;

		made = Add(ref _names, ref _nameCount, new Name(at, end));

		return end;
	}

	int Read_Digits(int at, out int made)
	{
		made = -1;

		var end = at;

		while (end < _text.Length && _text[end] is >= '0' and <= '9')
			end++;

		if (end == at)
			return -1;

		made = Add(ref _digits, ref _digitCount, new Digits(at, end));

		return end;
	}

	/// <summary><c>trivia = ' '*</c>, at every seam, and it never gives back.</summary>
	int Skip(int at)
	{
		var end = at;

		while (end < _text.Length && _text[end] == ' ')
			end++;

		return end;
	}

	static int Add<T>(ref T[] arena, ref int count, T one)
	{
		if (count == arena.Length)
			Array.Resize(ref arena, count * 2);

		arena[count] = one;

		return count++;
	}

	// ---- building ----------------------------------------------------------------------

	int[] _spine = new int[8];
	int   _top;

	/// <summary>Walks from the root, by index.</summary>
	public string Construct() => Build(_root);

	/// <summary>
	/// A <c>Sum</c>, with its left spine unwound onto a stack rather than onto the call
	/// stack, so a fold of any length is one frame.
	/// </summary>
	string Build(Sum sum)
	{
		var from = _top;

		while (sum.Kind == Sums.Step)
		{
			if (_top == _spine.Length)
				Array.Resize(ref _spine, _top * 2);

			_spine[_top++] = sum.At;
			sum            = _steps[sum.At].Left;
		}

		var value = Author.Only(Build(_onlys[sum.At].One));

		while (_top > from)
			value = Author.Step(value, Build(_steps[_spine[--_top]].Right));

		return value;
	}

	/// <summary>A <c>Pair</c>, which is the one place the walk can go deeper.</summary>
	string Build(Pair pair)
	{
		if (pair.Kind == Pairs.Nested)
			return Author.Nested(Build(_nesteds[pair.At].Inner));

		var binding = _bindings[pair.At];

		return Author.Pair(_names[binding.Name].Build(_text), _digits[binding.Digits].Build(_text));
	}
}
