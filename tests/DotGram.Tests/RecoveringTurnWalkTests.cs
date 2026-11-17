using System;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the end of a turn of a repetition marked <c>recover</c> costs in a nest of them: it finds the
/// turn's mark and record and puts out the ways back the turn left, and a list nested in a list nested
/// in a list must not make each level look again at everything the levels inside it read. Counted by
/// the records those ends looked at, under <c>DOTGRAM_COUNTS</c>, and not timed.
/// </summary>
/// <remarks>
/// Each end searched down from the top of the arena to the turn's mark, through every turn of every
/// list nested in the element: lists nested 3,200 deep took a second and a half. The value of each
/// grammar spells the tree back, so the tests also say what was read, and that a bad element was
/// stepped over where it stood.
/// </remarks>
public sealed class RecoveringTurnWalkTests
{
	/// <summary>A list whose element may be the list again, with an ordinary repetition after it.</summary>
	const string Nested = """
		trivia = { ' '* }
		Start : @string = t: T & eof => @(t)
		T     : @string = '(' & items: Item* recover ',' => @("!") & ')' => @("(" + string.Join(",", items) + ")")
		              | 'x' => @("x")
		Item  : @string = t: T & b: '!'* & ','? => @(t + b)
		parse Start stream
		""";

	/// <summary>Two lists, each the other's element: <c>[x; {x, [x; …]}]</c>.</summary>
	const string Mixed = """
		trivia = { ' '* }
		Start : @string = t: A & eof => @(t)
		A     : @string = '[' & items: AItem* recover ';' => @("!") & ']' => @("[" + string.Join(";", items) + "]")
		AItem : @string = t: B & ';'? => @(t) | t: X & ';'? => @(t)
		B     : @string = '{' & items: BItem* recover ',' => @("!") & '}' => @("{" + string.Join(",", items) + "}")
		BItem : @string = t: A & ','? => @(t) | t: X & ','? => @(t)
		X     : @string = 'x' => @("x")
		parse Start stream
		""";

	public enum Shape
	{
		/// <summary>Every element read.</summary>
		Nested,

		/// <summary>A bad element at every level, stepped over.</summary>
		Bad,

		/// <summary>An ordinary repetition after every nested list, whose ways back the turn puts out.</summary>
		Bangs,

		/// <summary>Two lists nested in each other.</summary>
		Mixed,

		/// <summary>The same, with a bad element at every level.</summary>
		MixedBad,
	}

	public enum Form
	{
		String,
		Reader,
	}

	/// <summary>Every carrier, every shape, both forms of the publication.</summary>
	public static TheoryData<CarrierKind, Shape, Form> Counted
	{
		get
		{
			var rows = new TheoryData<CarrierKind, Shape, Form>();

			foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Immediate, CarrierKind.Deferred })
				foreach (var shape in Enum.GetValues<Shape>())
					foreach (var form in Enum.GetValues<Form>())
						rows.Add(carrier, shape, form);

			return rows;
		}
	}

	/// <summary>Every shape at a few depths, under the carriers that build differently.</summary>
	public static TheoryData<CarrierKind, Shape, int> Answered
	{
		get
		{
			var rows = new TheoryData<CarrierKind, Shape, int>();

			foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Deferred })
				foreach (var shape in Enum.GetValues<Shape>())
					foreach (var depth in new[] { 0, 1, 2, 5, 40 })
						rows.Add(carrier, shape, depth);

			return rows;
		}
	}

	static string Grammar(Shape shape)
	{
		return shape is Shape.Mixed or Shape.MixedBad ? Mixed : Nested;
	}

	/// <summary>The input at a depth, and the value it reads into.</summary>
	static (string Input, string Value) Nest(Shape shape, int depth)
	{
		var (open, opened, close, closed) = shape switch
		{
			Shape.Nested   => ("(x, ", "(x,", ")", ")"),
			Shape.Bad      => ("(x, ?, ", "(x,!,", ")", ")"),
			Shape.Bangs    => ("(x, ", "(x,", ")!! ", ")!!"),
			Shape.Mixed    => ("[x; {x, ", "[x;{x,", "}]", "}]"),
			Shape.MixedBad => ("[x; ?; {x, ?, ", "[x;!;{x,!,", "}]", "}]"),
			_              => throw new ArgumentOutOfRangeException(nameof(shape)),
		};

		var leaf  = shape is Shape.Mixed or Shape.MixedBad ? "[x]" : "x";
		var input = string.Concat(Enumerable.Repeat(open, depth)) + leaf + string.Concat(Enumerable.Repeat(close, depth));
		var value = string.Concat(Enumerable.Repeat(opened, depth)) + leaf + string.Concat(Enumerable.Repeat(closed, depth));

		// The last close of Bangs has nothing after it to hold the bangs: the outermost list is not an element.
		if (shape == Shape.Bangs && depth > 0)
		{
			input = input.Substring(0, input.Length - 3);
			value = value.Substring(0, value.Length - 2);
		}

		return (input, value);
	}

	/// <summary>
	/// Twice the depth, at most a little over twice the records looked at: linear. Searching down to
	/// each turn's mark through the turns nested in it, each doubling looked at four times as many.
	/// </summary>
	[Theory]
	[MemberData(nameof(Counted))]
	public void A_nest_of_recovering_turns_looks_at_each_record_once(CarrierKind carrier, Shape shape, Form form)
	{
		var assembly = Compiled(Grammar(shape), carrier, counts: true);

		long[] walked = [.. new[] { 100, 200, 400, 800 }.Select(depth => Walked(assembly, shape, depth, form))];

		Assert.True(
			walked[0] > 0 && walked[3] <= 2.2 * walked[2] && walked[2] <= 2.2 * walked[1] && walked[1] <= 2.2 * walked[0],
			$"{string.Join(", ", walked)} records looked at by the ends of turns at 100, 200, 400 and 800 levels.");

		Assert.True(walked[3] <= 40 * 800, $"{walked[3]} records looked at by the ends of turns at 800 levels.");
	}

	/// <summary>
	/// What a nest reads into, bad elements stepped over where they stood, and a nest cut short or
	/// closed once too often refused, from a string and from a reader alike.
	/// </summary>
	[Theory]
	[MemberData(nameof(Answered))]
	public void A_nest_of_recovering_turns_reads_what_it_holds(CarrierKind carrier, Shape shape, int depth)
	{
		var assembly    = Compiled(Grammar(shape), carrier, counts: false);
		var (input, value) = Nest(shape, depth);

		Assert.Equal((true, value), Read(assembly, input, Form.String));
		Assert.Equal((true, value), Read(assembly, input, Form.Reader));

		var close = shape is Shape.Mixed or Shape.MixedBad ? "]" : ")";

		Assert.False(Read(assembly, input + close, Form.String).Read);
		Assert.Equal(Read(assembly, input + close, Form.String), Read(assembly, input + close, Form.Reader));

		if (depth > 0)
		{
			var cut = input.Substring(0, input.LastIndexOf(close[0]));

			// A nest that is not closed is refused, not stepped over: `(x, x` has no list to end.
			Assert.False(Read(assembly, cut, Form.String).Read);
			Assert.Equal(Read(assembly, cut, Form.String), Read(assembly, cut, Form.Reader));
		}
	}

	static long Walked(Assembly assembly, Shape shape, int depth, Form form)
	{
		var counters = assembly.GetTypes()
			.SelectMany(static type => type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
			.Where(static field => field.Name.StartsWith("TurnWalked", StringComparison.Ordinal))
			.ToList();

		Assert.NotEmpty(counters);

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		var (input, value) = Nest(shape, depth);

		Assert.Equal((true, value), Read(assembly, input, form));

		return counters.Sum(static counter => (long)counter.GetValue(null)!);
	}

	static (bool Read, string? Value) Read(Assembly assembly, string input, Form form)
	{
		var type  = assembly.GetType("Grammar")!;
		var match = form == Form.String
			? type.GetMethod("TryParseStart", [typeof(string)])!.Invoke(null, [input])!
			: type.GetMethod("TryParseStart", [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [new StringReader(input), null, null])!;

		var read = (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;

		return (read, read ? (string?)match.GetType().GetProperty("Value")!.GetValue(match) : null);
	}

	static Assembly Compiled(string grammar, CarrierKind carrier, bool counts)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = carrier,
			CountRules    = counts,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, symbols: counts ? ["DOTGRAM_COUNTS"] : null);
	}
}
