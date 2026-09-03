using System;

namespace DotGram.HandDeferred;

/// <summary>
/// <c>Deferred.gram</c> again, and safe again, but with the answer carried the other way:
/// a rule returns the value it read, deferred — a <c>Func&lt;string&gt;</c> that has not
/// been called.
/// </summary>
/// <remarks>
/// <para>
/// This is the honest version of "why not just return the object". Returning the built
/// object would break §7.3; returning a closure that would build it does not, and a rule
/// gets to answer with a value again instead of with a side effect on a tape. The
/// recursion reads naturally, there is no index arithmetic, and nothing is invoked until
/// <see cref="Construct"/> asks for it.
/// </para>
/// <para>
/// Line for line it is the same parser as <see cref="Reader"/>. Every difference between
/// the two files is the representation of the deferred call, which is what makes them
/// worth reading side by side — and what makes the numbers <c>Program --time</c> prints
/// mean one thing rather than several.
/// </para>
/// <para>
/// <b>What it costs, measured rather than argued.</b> <c>Program --time</c> times the two
/// phases apart, because together they say nothing: this grammar's own construction is
/// <c>l + "+" + r</c> over a left-leaning fold, quadratic in the pairs, and it is nine
/// tenths of everything. Apart, at four hundred pairs:
/// </para>
/// <code>
/// recognize, tape           15.8 us      48,936 B    1.00x
/// recognize, closures       23.7 us     153,624 B    1.50x
///
/// construct, table          61.5 us     859,192 B    1.00x
/// construct, stack          60.6 us     846,456 B    0.98x
/// construct, closures       57.1 us     846,368 B    0.93x
/// </code>
/// <para>
/// So closures are half again dearer to <em>write</em> and three times the allocation — a
/// deferred node here is two objects, a display class holding the capture and the delegate
/// pointing at it, scattered rather than contiguous and every one of them traced. A
/// derivation that is abandoned leaves that as garbage, where the tape's rewind is one
/// integer.
/// </para>
/// <para>
/// And closures are cheaper to <em>read</em>, which is the part worth sitting with. The
/// tape carries the shape of each node to the end and dispatches on it while building; a
/// closure resolved that dispatch when it was made. The work is the same work, moved
/// earlier and paid for with an allocation. Summed, the two readings are within a few
/// percent of each other for this grammar — the tape ahead by four at four hundred pairs
/// and behind at forty.
/// </para>
/// <para>
/// What is not within a few percent is the shape of the risk. The closure chain is walked
/// by calling it, one frame per node, as deep as the derivation — and <c>Sum</c> leans
/// left, so that is one frame per pair, and a long enough input takes the process down
/// where the tape's loop would not have noticed. Nothing can pool a closure, either, so
/// the allocation is not something a busier program can arrange away, and it is three
/// times as much of it.
/// </para>
/// </remarks>
sealed class Closures
{
	readonly string _text;

	Func<string>? _made;

	public Closures(string text) => _text = text;

	/// <summary>Reads the whole input, arranging the constructions and calling none.</summary>
	public bool Recognize()
	{
		var end = Read_Sum(Skip(0), out _made);

		return end >= 0 && Skip(end) == _text.Length;
	}

	/// <summary>Calls what was arranged, root first, each node calling its children.</summary>
	public string Construct() => _made!();

	// ---- reading -----------------------------------------------------------------------

	/// <summary><c>Sum</c>, folded the same way, with the sum so far carried as a delegate.</summary>
	int Read_Sum(int at, out Func<string>? made)
	{
		var end = Read_Pair(at, out var one);

		if (end < 0)
		{
			made = null;

			return -1;
		}

		// `Sum = one: Pair => @(one)`. One display class and one delegate, and `one` is
		// held alive by it.
		var sum = made = () => Author.Only(one!());

		while (true)
		{
			var plus = Skip(end);

			if (plus >= _text.Length || _text[plus] != '+')
				return end;

			var left = sum;
			var next = Read_Pair(Skip(plus + 1), out var right);

			// The step is abandoned. Whatever `Read_Pair` allocated on the way to failing
			// is unreachable now, which is the whole of the rewind and also the cost of it.
			if (next < 0)
				return end;

			// `Sum = l: Sum & '+' & r: Pair => @(l + "+" + r)`. Captures two delegates, so
			// the chain is threaded here rather than laid out in an array.
			sum = made = () => Author.Step(left(), right!());
			end = next;
		}
	}

	/// <summary><c>Pair = name: Name &amp; '=' &amp; value: Digits</c>.</summary>
	int Read_Pair(int at, out Func<string>? made)
	{
		made = null;

		var end = Read_Name(at, out var name);

		if (end < 0)
			return -1;

		var sign = Skip(end);

		// The same place as in `Reader`, and the same promise kept: `Author.Name` has not
		// run. What has happened is that a closure and its display class were allocated
		// for it, and are now garbage.
		if (sign >= _text.Length || _text[sign] != '=')
			return -1;

		end = Read_Digits(Skip(sign + 1), out var value);

		if (end < 0)
			return -1;

		made = () => Author.Pair(name!(), value!());

		return end;
	}

	/// <summary><c>Name = t: ['a'..'z']+</c>.</summary>
	int Read_Name(int at, out Func<string>? made)
	{
		var end = at;

		while (end < _text.Length && _text[end] is >= 'a' and <= 'z')
			end++;

		if (end == at)
		{
			made = null;

			return -1;
		}

		// The capture is deferred too, so that the comparison is of the deferral and not
		// of who cuts the string. It captures `this`, `at` and `end`.
		var start = at;
		var stop  = end;

		made = () => Author.Name(_text.Substring(start, stop - start));

		return end;
	}

	/// <summary><c>Digits = t: ['0'..'9']+</c>.</summary>
	int Read_Digits(int at, out Func<string>? made)
	{
		var end = at;

		while (end < _text.Length && _text[end] is >= '0' and <= '9')
			end++;

		if (end == at)
		{
			made = null;

			return -1;
		}

		var start = at;
		var stop  = end;

		made = () => Author.Digits(_text.Substring(start, stop - start));

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
}
