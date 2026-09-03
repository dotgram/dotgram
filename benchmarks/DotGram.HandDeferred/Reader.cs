using System;
using System.Text;

namespace DotGram.HandDeferred;

/// <summary>
/// <c>Deferred.gram</c>, written by hand, keeping the promise the notation makes.
/// </summary>
/// <remarks>
/// <para>
/// The grammar is beside this file. It is small on purpose and it has exactly the two
/// places where building as you read would break §7.3: <c>Name</c> is read and built
/// before the <c>'='</c> that can fail, and the whole parse can be abandoned at the end
/// by the input not being finished. A hand-written parser that builds where it reads —
/// the SQL yardstick in <c>DotGram.Benchmarks</c> is one — invokes the author's factory
/// for derivations nobody accepted. It gets away with it because its factories are pure
/// allocations. The notation does not get away with it, because it promises the author
/// they need not think about that.
/// </para>
/// <para>
/// So this is the same recursive descent a person would write, with one thing added: it
/// recognizes onto a tape and builds afterwards. There are two phases and they are the
/// two methods below, <see cref="Recognize"/> and <see cref="Construct"/>. Nothing the
/// author wrote runs until the first has answered yes.
/// </para>
/// <para>
/// <b>The invariant that makes it cheap.</b> A reader that fails leaves the tape exactly
/// as it found it. Then every record on the tape belongs to the accepted derivation, and
/// there is nothing to sweep. Only one reader here can fail after writing — <c>Pair</c>,
/// which is the interesting case — so the rewind appears once.
/// </para>
/// <para>
/// <b>The other one.</b> A record is pushed after its children, so a child's index is
/// always smaller than its parent's. The tape is therefore already in an order that can
/// be built in a single forward loop: no walk, no stack, no recursion over the depth of
/// the input. That is why <see cref="Construct"/> is a <c>for</c>.
/// </para>
/// </remarks>
ref struct Reader
{
	/// <summary>What a reader wrote down, and all it wrote down.</summary>
	/// <param name="Shape">Which alternative of which rule this is.</param>
	/// <param name="A">The first child's index, or the start of the text a leaf matched.</param>
	/// <param name="B">The second child's index, or the end of that text.</param>
	readonly record struct Record(Shape Shape, int A, int B);

	/// <summary>One per <c>=&gt;</c> in the grammar, which is one per construction to invoke.</summary>
	enum Shape
	{
		/// <summary><c>Name : @string = t: ['a'..'z']+ =&gt; @(t)</c></summary>
		Name,

		/// <summary><c>Digits : @string = t: ['0'..'9']+ =&gt; @(t)</c></summary>
		Digits,

		/// <summary><c>Pair : @string = name: Name &amp; '=' &amp; value: Digits =&gt; @(name + ":" + value)</c></summary>
		Pair,

		/// <summary><c>Sum = one: Pair =&gt; @(one)</c> — the base of the fold.</summary>
		Only,

		/// <summary><c>Sum = l: Sum &amp; '+' &amp; r: Pair =&gt; @(l + "+" + r)</c> — one step of it.</summary>
		Step,
	}

	readonly ReadOnlySpan<char> _text;

	Record[] _tape;
	int      _count;

	public Reader(ReadOnlySpan<char> text)
	{
		_text  = text;
		_tape  = new Record[16];
		_count = 0;
	}

	/// <summary>Reads the whole input, writing down what it read and building none of it.</summary>
	public bool Recognize()
	{
		var end = Read_Sum(Skip(0));

		return end >= 0 && Skip(end) == _text.Length;
	}

	// ---- reading -----------------------------------------------------------------------

	/// <summary>
	/// <c>Sum</c>, which is left-recursive, read as §4.3 makes it: the base once, then a
	/// run of steps.
	/// </summary>
	int Read_Sum(int at)
	{
		var end = Read_Pair(at);

		if (end < 0)
			return -1;

		// `Sum = one: Pair => @(one)`.
		Push(Shape.Only, _count - 1, 0);

		while (true)
		{
			var plus = Skip(end);

			if (plus >= _text.Length || _text[plus] != '+')
				return end;

			// Taken before the right-hand side is read, so that it is the sum so far and
			// not the pair that is about to be pushed.
			var left = _count - 1;
			var next = Read_Pair(Skip(plus + 1));

			// A step that does not finish is not a step, and `Read_Pair` has already put
			// the tape back. The sum so far stands, and the '+' is given back with it.
			if (next < 0)
				return end;

			// `Sum = l: Sum & '+' & r: Pair => @(l + "+" + r)`.
			Push(Shape.Step, left, _count - 1);

			end = next;
		}
	}

	/// <summary><c>Pair = name: Name &amp; '=' &amp; value: Digits</c>.</summary>
	int Read_Pair(int at)
	{
		var mark = _count;
		var end  = Read_Name(at);

		if (end < 0)
			return -1;

		var name = _count - 1;
		var sign = Skip(end);

		// Here is the whole point. `Name` has been read and its record written, and the
		// derivation it belongs to is about to be abandoned. A parser that had built it
		// would have called the author's `=> @(t)` for a `Name` that is not in the answer.
		// This one drops a record.
		if (sign >= _text.Length || _text[sign] != '=')
		{
			_count = mark;

			return -1;
		}

		end = Read_Digits(Skip(sign + 1));

		if (end < 0)
		{
			_count = mark;

			return -1;
		}

		Push(Shape.Pair, name, _count - 1);

		return end;
	}

	/// <summary><c>Name = t: ['a'..'z']+</c>.</summary>
	int Read_Name(int at)
	{
		var end = at;

		while (end < _text.Length && _text[end] is >= 'a' and <= 'z')
			end++;

		if (end == at)
			return -1;

		Push(Shape.Name, at, end);

		return end;
	}

	/// <summary><c>Digits = t: ['0'..'9']+</c>.</summary>
	int Read_Digits(int at)
	{
		var end = at;

		while (end < _text.Length && _text[end] is >= '0' and <= '9')
			end++;

		if (end == at)
			return -1;

		Push(Shape.Digits, at, end);

		return end;
	}

	/// <summary>
	/// <c>trivia = ' '*</c>, at every seam.
	/// </summary>
	/// <remarks>
	/// It never gives back. Nothing in this grammar follows a seam and wants a space —
	/// there is no look and no class that admits one — so the run reads as far as it can
	/// and is never asked for a shorter reading. That is the same question
	/// <c>Determinism.NeverGivesBack</c> asks, answered here by looking at four rules.
	/// </remarks>
	readonly int Skip(int at)
	{
		var end = at;

		while (end < _text.Length && _text[end] == ' ')
			end++;

		return end;
	}

	void Push(Shape shape, int a, int b)
	{
		if (_count == _tape.Length)
			Array.Resize(ref _tape, _count * 2);

		_tape[_count++] = new Record(shape, a, b);
	}

	// ---- building ----------------------------------------------------------------------

	/// <summary>
	/// Invokes the author's constructions, once each, for the derivation that was
	/// accepted — front to back, because a record's children were pushed before it.
	/// </summary>
	public readonly string Construct()
	{
		var values = new string[_count];

		for (var i = 0; i < _count; i++)
		{
			var (shape, a, b) = _tape[i];

			values[i] = shape switch
			{
				Shape.Name   => Construct_Name  (_text[a..b].ToString()),
				Shape.Digits => Construct_Digits(_text[a..b].ToString()),
				Shape.Pair   => Construct_Pair  (values[a], values[b]),
				Shape.Only   => Construct_Only  (values[a]),
				_            => Construct_Step  (values[a], values[b]),
			};
		}

		// The root is the last record: whatever `Read_Sum` pushed last is what it answered.
		return values[_count - 1];
	}

	// ---- the author's half -------------------------------------------------------------
	//
	// One method per `=>`, which is what the generator emits and what the JIT wants: small
	// enough to inline, and named after the rule so a profile says which construction cost
	// what. The counter is not part of a parser — it is here so that Program can show the
	// promise being kept rather than assert it.

	static int _constructions;

	/// <summary>How many of the author's constructions have run since <see cref="Forget"/>.</summary>
	public static int Constructions => _constructions;

	/// <summary>Sets the count back to nothing, so that one parse can be watched.</summary>
	public static void Forget() => _constructions = 0;

	static string Construct_Name(string t)
	{
		_constructions++;

		return t;
	}

	static string Construct_Digits(string t)
	{
		_constructions++;

		return t;
	}

	static string Construct_Pair(string name, string value)
	{
		_constructions++;

		return name + ":" + value;
	}

	static string Construct_Only(string one)
	{
		_constructions++;

		return one;
	}

	static string Construct_Step(string l, string r)
	{
		_constructions++;

		return l + "+" + r;
	}

	// ---- for reading over ---------------------------------------------------------------

	/// <summary>The tape as it stands, one record to a line.</summary>
	public readonly string Describe()
	{
		var written = new StringBuilder();

		for (var i = 0; i < _count; i++)
		{
			var (shape, a, b) = _tape[i];

			written.Append('#').Append(i).Append('\t').Append(shape.ToString().PadRight(7));

			written.Append(
				shape is Shape.Name or Shape.Digits ? $"\"{_text[a..b]}\"" :
				shape is Shape.Only                 ? $"#{a}" :
				                                      $"#{a} #{b}");

			written.AppendLine();
		}

		return written.ToString();
	}
}
