using System;

namespace DotGram.HandDeferred;

/// <summary>
/// <c>Deferred.gram</c> a fifth time, and the ordinary way: a class per alternative with
/// a virtual method, which is what anyone would write if nobody had asked the question.
/// </summary>
/// <remarks>
/// <para>
/// It is here as the control. <see cref="Boxed"/> pays for a header, a method table
/// pointer and a virtual call because a struct was put behind an interface; a class pays
/// for exactly the same three things because it is a class. If the two land together,
/// then boxing costs nothing beyond being a reference type, and the interesting number
/// was never the box — it was the heap.
/// </para>
/// <para>
/// It is also the fairer of the two, in the one way that matters to a generator: a node
/// keeps its children at their own types wherever the grammar names them, so <c>Pair</c>
/// holds a <c>Name</c> and a <c>Digits</c> and not two <c>Node</c>s, and the JIT sees a
/// sealed type at the call. The places the type has to be forgotten are exactly the two
/// the rest of this project keeps running into: the fold, where <c>Step</c> takes the sum
/// so far, and the choice, where a <c>Pair</c> may have turned out to be a parenthesis.
/// </para>
/// </remarks>
sealed class Classes : IReading
{
	/// <summary>A shape, and what it will build when asked.</summary>
	abstract class Node
	{
		public abstract string Build(string text);
	}

	sealed class Name : Node
	{
		readonly int _at;
		readonly int _end;

		public Name(int at, int end)
		{
			_at  = at;
			_end = end;
		}

		public override string Build(string text) => Author.Name(text.Substring(_at, _end - _at));
	}

	sealed class Digits : Node
	{
		readonly int _at;
		readonly int _end;

		public Digits(int at, int end)
		{
			_at  = at;
			_end = end;
		}

		public override string Build(string text) => Author.Digits(text.Substring(_at, _end - _at));
	}

	sealed class Pair : Node
	{
		readonly Name   _name;
		readonly Digits _value;

		public Pair(Name name, Digits value)
		{
			_name  = name;
			_value = value;
		}

		public override string Build(string text) => Author.Pair(_name.Build(text), _value.Build(text));
	}

	/// <summary><c>Pair = '(' &amp; inner: Sum &amp; ')'</c>.</summary>
	sealed class Nested : Node
	{
		readonly Node _inner;

		public Nested(Node inner) => _inner = inner;

		public override string Build(string text) => Author.Nested(_inner.Build(text));
	}

	sealed class Only : Node
	{
		readonly Node _one;

		public Only(Node one) => _one = one;

		public override string Build(string text) => Author.Only(_one.Build(text));
	}

	/// <summary>The fold, and the one node that has to forget a child's type.</summary>
	sealed class Step : Node
	{
		readonly Node _left;
		readonly Node _right;

		public Step(Node left, Node right)
		{
			_left  = left;
			_right = right;
		}

		public override string Build(string text) => Author.Step(_left.Build(text), _right.Build(text));
	}

	readonly string _text;

	Node? _root;

	public Classes(string text) => _text = text;

	/// <summary>Reads the whole input, building the tree and calling none of it.</summary>
	public bool Recognize()
	{
		var end = Read_Sum(Skip(0), out _root);

		return end >= 0 && Skip(end) == _text.Length;
	}

	/// <summary>Calls the root, which calls its children, as deep as the fold went.</summary>
	public string Construct() => _root!.Build(_text);

	// ---- reading -----------------------------------------------------------------------

	int Read_Sum(int at, out Node? made)
	{
		var end = Read_Pair(at, out var one);

		if (end < 0)
		{
			made = null;

			return -1;
		}

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

			made = new Step(left!, right!);
			end  = next;
		}
	}

	/// <summary><c>Pair</c>, the binding or a parenthesised <c>Sum</c>.</summary>
	int Read_Pair(int at, out Node? made)
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

			made = new Nested(sum!);

			return close + 1;
		}

		var end = Read_Name(at, out var name);

		if (end < 0)
			return -1;

		var sign = Skip(end);

		// And the same place a fifth time: an object allocated, and now unreachable.
		if (sign >= _text.Length || _text[sign] != '=')
			return -1;

		end = Read_Digits(Skip(sign + 1), out var value);

		if (end < 0)
			return -1;

		made = new Pair(name!, value!);

		return end;
	}

	int Read_Name(int at, out Name? made)
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

	int Read_Digits(int at, out Digits? made)
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
