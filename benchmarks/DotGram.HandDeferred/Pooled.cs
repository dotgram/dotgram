using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;

namespace DotGram.HandDeferred;

/// <summary>
/// <see cref="Mixed"/> again, differing in one thing: where the array of steps comes from.
/// </summary>
/// <remarks>
/// <para>
/// It is here to answer a question rather than to be a sixth idea. <see cref="Mixed"/> is
/// ahead of everything up to a thousand pairs and behind at ten thousand, and the reading
/// offered for that was the large object heap: its one array of steps doubles past
/// eighty-five kilobytes, every parse allocates a large object, and the gen2 column bears
/// it out — two hundred collections per thousand parses where the readings that allocate
/// an object per node have none.
/// </para>
/// <para>
/// That is circumstantial. This is the control: the same shapes, the same layout, the same
/// reading, and the array rented from a free list and given back at the end. If the
/// slowdown was the array being allocated and collected, it goes away here and the gen2
/// column goes to nothing. If it was something else — the cost of copying on each
/// doubling, or the cache behaviour of writing a large array while reading — it stays.
/// </para>
/// <para>
/// <b>It goes away.</b> Nanoseconds per pair, over <see cref="Threshold"/>'s four sizes:
/// </para>
/// <code>
///           1,000     2,000     3,000     4,000
/// Mixed       9.3       9.5      15.7      13.6
/// Pooled      6.2       6.1       6.5       6.4
/// </code>
/// <para>
/// A straight line through the place where <see cref="Mixed"/> steps up by seventy
/// percent, and gen2 goes from thirty-one collections per thousand parses to none.
/// <c>Pair</c> is twenty-four bytes and the array doubles, so its capacities are 2,048 at
/// forty-nine kilobytes and 4,096 at ninety-eight: the step is where the eighty-five
/// kilobyte line is and nowhere else. And the gain is not only there — 0.67 and 0.64 below
/// the line, because allocating and zeroing an array costs something at any size. The
/// large object heap does not create the cost, it triples it.
/// </para>
/// <para>
/// The shapes are copied rather than shared because <c>Pair</c> holds a <c>Sums</c> and
/// <c>Sums</c> holds the array, so the cycle cannot be reused with a different array
/// source. A controlled experiment costs a copy; that is what makes it controlled.
/// </para>
/// </remarks>
sealed class Pooled : IReading
{
	/// <summary>Exactly <see cref="Mixed"/>'s <c>Pair</c>, twenty-four bytes.</summary>
	readonly struct Pair
	{
		readonly Name   _name;
		readonly Digits _value;
		readonly Run?   _inner;

		public Pair(Name name, Digits value)
		{
			_name  = name;
			_value = value;
			_inner = null;
		}

		public Pair(Run inner)
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

	readonly struct Only
	{
		readonly Pair _one;

		public Only(Pair one) => _one = one;

		public string Build(string text) => Author.Only(_one.Build(text));
	}

	/// <summary><c>Sum</c> entire, with the array borrowed rather than made.</summary>
	sealed class Run
	{
		readonly Only    _base;
		readonly Pair[]? _steps;
		readonly int     _count;

		public Run(Only one, Pair[]? steps, int count)
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
	/// The free list, per thread and by size, like the engine's <c>Ways.Rent()</c>.
	/// </summary>
	/// <remarks>
	/// By size is the part that matters, and the first attempt at this got it wrong: one
	/// stack for every length hands a four-element array to a request for four thousand,
	/// which then allocates, and the pool that was supposed to remove the large object
	/// allocated one every parse anyway. The measurement said so — gen2 identical to
	/// <see cref="Mixed"/> to three decimal places — which is what a control is for.
	/// </remarks>
	[ThreadStatic]
	static Stack<Pair[]>?[]? _free;

	readonly string _text;

	readonly List<Pair[]> _borrowed = [];

	/// <summary>
	/// What a run asks for before it knows how long it is.
	/// </summary>
	/// <remarks>
	/// Four is what an array that is going to be grown anyway would start at, and it is
	/// wrong here for a reason the flat inputs hid: a parenthesised group is a run of its
	/// own, so an input with many small groups asks for a first array as many times, and
	/// each one that outgrows four asks again. Sixteen costs nothing — the arrays come
	/// from a pool and are handed back — and skips two rounds of that.
	/// </remarks>
	readonly int _first;

	/// <summary>
	/// Whether a run asks for as much as the longest run any parse has needed so far,
	/// rather than for <see cref="_first"/> and then doubling its way up.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The pool already keeps whatever size an array grew to, so the memory is there;
	/// what it does not do is hand it over on the first ask, because the buckets are by
	/// size and a request for four gets a four. One integer fixes that: remember the
	/// longest run yet seen and start every run there. A run that would have doubled its
	/// way from four to a thousand — nine borrows and eight copies of everything written
	/// so far — takes one borrow and copies nothing.
	/// </para>
	/// <para>
	/// It is a guess about the next input from the last one, so it can be wrong in the
	/// direction of holding more memory than a parse needs: every run alive at once asks
	/// for the high-water size, and a deeply nested input has one run alive per level.
	/// Pooled, so it costs the high-water mark and not an allocation.
	/// </para>
	/// <para>
	/// <b>And it is the one thing in this project that moved every size at once.</b>
	/// Against the same reading without it, over a flat input:
	/// </para>
	/// <code>
	/// | Method       | Pairs | Mean        | Error      | Allocated |
	/// | Pooled       | 5     |    44.54 ns |   0.500 ns |     192 B |
	/// | PooledLearns | 5     |    44.36 ns |   0.432 ns |     192 B |
	/// | Pooled       | 10    |    88.89 ns |   0.599 ns |     192 B |
	/// | PooledLearns | 10    |    68.56 ns |   1.181 ns |     192 B |
	/// | Pooled       | 100   |   717.41 ns |  13.290 ns |     616 B |
	/// | PooledLearns | 100   |   583.91 ns |   3.447 ns |     528 B |
	/// | Pooled       | 1000  | 6,299.12 ns | 119.889 ns |    4072 B |
	/// | PooledLearns | 1000  | 5,856.63 ns | 115.836 ns |    3832 B |
	/// </code>
	/// <para>
	/// Twenty-three percent at ten pairs, nineteen at a hundred, seven at a thousand, and
	/// every gap several times its error. The largest is at the smallest size that has a
	/// run at all, which is the opposite of where it was looked for: what is saved is not
	/// the copying — nine doublings of a thousand elements come to a third of a microsecond
	/// — but the <em>borrows</em>, and a run of nine steps that took three now takes one.
	/// </para>
	/// <para>
	/// The single-threaded benchmark cannot show what sharing the mark is for; what it does
	/// show is that the compare-and-exchange costs nothing measurable, which it should not,
	/// happening once per run rather than once per element.
	/// </para>
	/// </remarks>
	readonly bool _learns;

	/// <summary>The longest run any parse has needed, on any thread.</summary>
	/// <remarks>
	/// <para>
	/// Shared rather than per thread, unlike the arrays it sizes. The arrays have to be
	/// per thread or every borrow would need a lock; this is one integer read once and
	/// written at most once per run, so it can be shared, and sharing is what it is for —
	/// a thread pool where every thread has to meet its own long input before it stops
	/// doubling has not learned anything.
	/// </para>
	/// <para>
	/// A lost update would cost one extra doubling and nothing else, so the barrier is not
	/// load-bearing; it is here because a maximum written by many hands is worth writing
	/// exactly when exactness is this cheap.
	/// </para>
	/// <para>
	/// What it does cost is coupling: one long document teaches every later parse on every
	/// thread to ask for a long array. That is memory held rather than memory allocated —
	/// the arrays come from the pool either way — but it is held for the life of the
	/// process, and a smaller-is-better answer here would be a decaying mark rather than a
	/// maximum.
	/// </para>
	/// </remarks>
	static int _wanted;

	Run? _root;

	public Pooled(string text, int first = 4, bool learns = false)
	{
		_text   = text;
		_first  = first;
		_learns = learns;
	}

	/// <summary>Reads the whole input. What it borrowed goes back before it answers no.</summary>
	public bool Recognize()
	{
		var end = Read_Sum(Skip(0), out _root);
		var all = end >= 0 && Skip(end) == _text.Length;

		if (!all)
			Return();

		return all;
	}

	/// <summary>Runs the fold, then gives the arrays back.</summary>
	public string Construct()
	{
		var value = _root!.Build(_text);

		Return();

		return value;
	}

	// ---- borrowing -----------------------------------------------------------------------

	Pair[] Borrow(int least)
	{
		if (_learns && Volatile.Read(ref _wanted) is var wanted && wanted > least)
			least = wanted;

		var size = 4;

		while (size < least)
			size <<= 1;

		var free = _free ??= new Stack<Pair[]>?[32];
		var bin  = free[BitOperations.Log2((uint)size)];
		var one  = bin is { Count: > 0 } ? bin.Pop() : new Pair[size];

		_borrowed.Add(one);

		return one;
	}

	/// <summary>
	/// Hands back everything borrowed, which a caller that stops after
	/// <see cref="Recognize"/> has to do for itself.
	/// </summary>
	/// <remarks>
	/// <see cref="Construct"/> calls it, and so does a failed <see cref="Recognize"/>. A
	/// successful one cannot: the arrays are the derivation until they have been built. The
	/// benchmark that measures reading alone therefore calls this, and the first version of
	/// it did not — so nothing was ever returned, the pool was empty every parse, and it
	/// allocated exactly what <see cref="Mixed"/> does. Which is what the identical
	/// allocation column was saying, twice, before anyone read it.
	/// </remarks>
	public void Return()
	{
		var free = _free ??= new Stack<Pair[]>?[32];

		foreach (var one in _borrowed)
		{
			var slot = BitOperations.Log2((uint)one.Length);

			(free[slot] ??= new Stack<Pair[]>()).Push(one);
		}

		_borrowed.Clear();
	}

	// ---- reading -------------------------------------------------------------------------

	int Read_Sum(int at, out Run? made)
	{
		made = null;

		var end = Read_Pair(at, out var one);

		if (end < 0)
			return -1;

		var start = new Only(one);

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

			if (steps is null)
			{
				steps = Borrow(_first);
			}
			else if (count == steps.Length)
			{
				var wider = Borrow(count * 2);

				Array.Copy(steps, wider, count);

				steps = wider;
			}

			steps[count++] = right;
			end            = next;
		}

		if (_learns)
			for (var seen = Volatile.Read(ref _wanted); count > seen; )
			{
				var was = Interlocked.CompareExchange(ref _wanted, count, seen);

				if (was == seen)
					break;

				seen = was;
			}

		made = new Run(start, steps, count);

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

			if (close >= _text.Length || _text[close] != ')')
				return -1;

			made = new Pair(sum!);

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
