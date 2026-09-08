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
/// <b>And a group costs nothing to keep.</b> Every other reading makes an object per
/// <c>Sum</c> — per parenthesis in the input — and cannot pool it, because until the parse
/// is built that object is the answer and giving it back would be throwing the derivation
/// away. So their allocation grows with the number of groups: <see cref="Pooled"/>, which
/// pools its arrays perfectly, still goes from 184 bytes to four kilobytes as the
/// parentheses go from none to sixty-six. Here a <c>Sum</c> is an index into an arena that
/// was already rented, so it is not an allocation at all, and the figure stays at a
/// hundred and twenty bytes whatever the input does.
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
/// <b>The arenas are rented, and it makes them slower.</b> That was not the expectation.
/// Six arrays are six chances to allocate a large object, and unpooled this was the
/// slowest reading of all at ten thousand pairs — 1.23 of the tape where <see cref="Mixed"/>
/// with its one array was 1.10 — so a slot per arena, taken on the way in and given back
/// on the way out, ought to have been the same win it is for <see cref="Pooled"/>. The
/// allocation went where it was supposed to: a hundred and twenty bytes a parse at every
/// size, which is the <c>Arenas</c> object and nothing else. The time went the wrong way.
/// </para>
/// <code>
///                    5 pairs    10 pairs   100 pairs  1,000 pairs
/// rented              68.3 ns    146.3 ns   2,225.9 ns  23,105.9 ns
/// afresh              82.4 ns    164.6 ns   1,347.6 ns  12,879.6 ns
/// </code>
/// <para>
/// <c>ArenasFresh</c> in the benchmarks is that second row: the same code, the same slots,
/// the pool simply never given anything back, so every parse allocates. On that input it
/// is twice as fast from a hundred pairs up, which says the cost is the reuse and not the
/// plumbing around it.
/// </para>
/// <para>
/// <b>And on another input it is the other way round.</b> <see cref="Nesting"/> runs the
/// same thousand pairs folded one, five and ten levels deep instead of laid out flat, and
/// there the rented arenas are the faster of the two — 7.8 against 9.5 microseconds at
/// five levels, 8.9 against 11.2 at ten. Same code, same arena sizes, near enough the same
/// number of records. Two inputs of the same length disagree about whether reusing the
/// arrays helps, and the flat result reproduced across two separate runs, so it is not
/// noise in the ordinary sense.
/// </para>
/// <para>
/// So the honest state of it: on the flat input renting measured slower, on the nested one
/// faster, and nothing here explains which. The reading first offered — that six pooled
/// arrays are scattered where six freshly bump-allocated ones lie together and are already
/// in cache — survives the flat half and says nothing about the other. It is left written
/// down as the thing to test, with cache-miss counters, rather than as the answer. The one
/// number that does not move is the allocation: a hundred and twenty bytes a parse at every
/// size and every depth, which is the <c>Arenas</c> object and nothing else.
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

	// One slot per arena, per thread. An arena is taken on the way in and given back on
	// the way out, keeping whatever size it grew to, so a second parse of the same shape
	// allocates nothing at all. There is no need for the size buckets `Pooled` has to
	// keep: only one `Arenas` reads at a time on a thread, so one slot is the whole pool.
	[ThreadStatic] static Name[]?    _freeNames;
	[ThreadStatic] static Digits[]?  _freeDigits;
	[ThreadStatic] static Only[]?    _freeOnlys;
	[ThreadStatic] static Step[]?    _freeSteps;
	[ThreadStatic] static Binding[]? _freeBindings;
	[ThreadStatic] static Nested[]?  _freeNesteds;
	[ThreadStatic] static int[]?     _freeSpine;

	readonly string _text;

	Name[]    _names;
	Digits[]  _digits;
	Only[]    _onlys;
	Step[]    _steps;
	Binding[] _bindings;
	Nested[]  _nesteds;

	int _nameCount;
	int _digitCount;
	int _onlyCount;
	int _stepCount;
	int _bindingCount;
	int _nestedCount;

	Sum _root;

	public Arenas(string text)
	{
		_text = text;

		_names    = Take(ref _freeNames,    8);
		_digits   = Take(ref _freeDigits,   8);
		_onlys    = Take(ref _freeOnlys,    8);
		_steps    = Take(ref _freeSteps,    8);
		_bindings = Take(ref _freeBindings, 8);
		_nesteds  = Take(ref _freeNesteds,  4);
		_spine    = Take(ref _freeSpine,    8);

		static T[] Take<T>(ref T[]? slot, int least)
		{
			var one = slot ?? new T[least];

			slot = null;

			return one;
		}
	}

	/// <summary>Reads the whole input into the arenas and builds none of it.</summary>
	public bool Recognize()
	{
		var end = Read_Sum(Skip(0), out _root);
		var all = end >= 0 && Skip(end) == _text.Length;

		if (!all)
			Return();

		return all;
	}

	/// <summary>
	/// Hands the arenas back, at whatever size they grew to.
	/// </summary>
	/// <remarks>
	/// <see cref="Construct"/> calls it, and so does a failed <see cref="Recognize"/>. A
	/// successful one cannot: until the shapes have been built, the arenas are the
	/// derivation. So a caller that stops after reading has to call this, and one that
	/// does not is measuring a pool that is always empty — which is exactly how
	/// <see cref="Pooled"/> came out identical to <see cref="Mixed"/> twice.
	/// </remarks>
	public void Return()
	{
		_freeNames    = _names;
		_freeDigits   = _digits;
		_freeOnlys    = _onlys;
		_freeSteps    = _steps;
		_freeBindings = _bindings;
		_freeNesteds  = _nesteds;
		_freeSpine    = _spine;
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

	int[] _spine;
	int   _top;

	/// <summary>Walks from the root, by index, and gives the arenas back.</summary>
	public string Construct()
	{
		var value = Build(_root);

		Return();

		return value;
	}

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
