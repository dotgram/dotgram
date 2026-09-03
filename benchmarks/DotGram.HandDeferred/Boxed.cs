using System;

namespace DotGram.HandDeferred;

/// <summary>
/// <c>Deferred.gram</c> a fourth time: the same value shapes, but held behind
/// <see cref="IBuilds"/> instead of inside one another.
/// </summary>
/// <remarks>
/// <para>
/// This is the way out <see cref="Unboxed"/> has to refuse. There a step could not name
/// the sum it stands on, because the type would grow with the input; here a child is an
/// <c>IBuilds</c>, its type is forgotten at the field, and <c>Step</c> can be written the
/// way the grammar reads — the fold as a left-leaning tower, one node wrapping the last.
/// The grammar's own shape is expressed for the first time.
/// </para>
/// <para>
/// It is paid for at the field. Assigning a struct to an interface boxes it: a header, a
/// method table pointer, and the eight or sixteen bytes that were the whole shape a
/// moment ago, allocated on the heap and traced by the collector. And the call is an
/// interface call on a reference of unknown type, five shapes deep into a polymorphic
/// tree, so nothing devirtualizes and nothing inlines.
/// </para>
/// <para>
/// <b>And it is cheaper than it sounds.</b> At four hundred pairs:
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
/// Boxing costs almost nothing beyond being a reference type: this and
/// <see cref="Classes"/> allocate the same 44,792 bytes to the byte, because a boxed
/// struct is an object and there is nothing else to it. What separates them is the call —
/// a class node keeps its children at their own sealed types and this one has forgotten
/// them — and that is worth eleven percent, not a multiple.
/// </para>
/// <para>
/// The one that stands out is neither: a closure is <em>two</em> objects where a box is
/// one, and 154 KB against 45. Being on the heap is not what costs; being on it twice is.
/// </para>
/// <para>
/// The leaves are shared with <see cref="Unboxed"/> deliberately: <c>Name</c> and
/// <c>Digits</c> are the same two structs, and the only difference between the two
/// readings is whether a parent keeps its children by value or by reference. That is the
/// comparison, with everything else held still.
/// </para>
/// </remarks>
sealed class Boxed
{
	/// <summary><c>Pair</c>, with its children behind the interface.</summary>
	readonly struct Pair : IBuilds
	{
		readonly IBuilds _name;
		readonly IBuilds _value;

		public Pair(IBuilds name, IBuilds value)
		{
			_name  = name;
			_value = value;
		}

		public string Build(string text) => Author.Pair(_name.Build(text), _value.Build(text));
	}

	/// <summary><c>Sum = one: Pair =&gt; @(one)</c>.</summary>
	readonly struct Only : IBuilds
	{
		readonly IBuilds _one;

		public Only(IBuilds one) => _one = one;

		public string Build(string text) => Author.Only(_one.Build(text));
	}

	/// <summary>
	/// <c>Sum = l: Sum &amp; '+' &amp; r: Pair</c> — the shape <see cref="Unboxed"/> could
	/// not write down.
	/// </summary>
	readonly struct Step : IBuilds
	{
		readonly IBuilds _left;
		readonly IBuilds _right;

		public Step(IBuilds left, IBuilds right)
		{
			_left  = left;
			_right = right;
		}

		public string Build(string text) => Author.Step(_left.Build(text), _right.Build(text));
	}

	readonly string _text;

	IBuilds? _root;

	public Boxed(string text) => _text = text;

	/// <summary>Reads the whole input, boxing what it read and building none of it.</summary>
	public bool Recognize()
	{
		var end = Read_Sum(Skip(0), out _root);

		return end >= 0 && Skip(end) == _text.Length;
	}

	/// <summary>Calls the root, which calls its children, as deep as the fold went.</summary>
	public string Construct() => _root!.Build(_text);

	// ---- reading -----------------------------------------------------------------------

	int Read_Sum(int at, out IBuilds? made)
	{
		var end = Read_Pair(at, out var one);

		if (end < 0)
		{
			made = null;

			return -1;
		}

		// Boxed here, and `one` was boxed where it was made.
		made = new Only(one!);

		while (true)
		{
			var plus = Skip(end);

			if (plus >= _text.Length || _text[plus] != '+')
				return end;

			var left = made;
			var next = Read_Pair(Skip(plus + 1), out var right);

			if (next < 0)
				return end;

			// One more box per step, wrapping the tower so far.
			made = new Step(left!, right!);
			end  = next;
		}
	}

	/// <summary><c>Pair = name: Name &amp; '=' &amp; value: Digits</c>.</summary>
	int Read_Pair(int at, out IBuilds? made)
	{
		made = null;

		var end = Read_Name(at, out var name);

		if (end < 0)
			return -1;

		var sign = Skip(end);

		// The same place a fourth time. `Unboxed` left a local on the stack; this one
		// leaves a box on the heap, exactly as the closures do.
		if (sign >= _text.Length || _text[sign] != '=')
			return -1;

		end = Read_Digits(Skip(sign + 1), out var value);

		if (end < 0)
			return -1;

		made = new Pair(name!, value!);

		return end;
	}

	/// <summary><c>Name = t: ['a'..'z']+</c>, the same struct <see cref="Unboxed"/> uses.</summary>
	int Read_Name(int at, out IBuilds? made)
	{
		var end = at;

		while (end < _text.Length && _text[end] is >= 'a' and <= 'z')
			end++;

		if (end == at)
		{
			made = null;

			return -1;
		}

		made = new Name(at, end);

		return end;
	}

	/// <summary><c>Digits = t: ['0'..'9']+</c>.</summary>
	int Read_Digits(int at, out IBuilds? made)
	{
		var end = at;

		while (end < _text.Length && _text[end] is >= '0' and <= '9')
			end++;

		if (end == at)
		{
			made = null;

			return -1;
		}

		made = new Digits(at, end);

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
