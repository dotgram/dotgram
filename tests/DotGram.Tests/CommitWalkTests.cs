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
/// What a commit costs in a nest: an atomic group that keeps its records, and a lookahead that hands
/// on a value, put out the ways back above them when they succeed, and a nest of them must look at
/// each record once over the whole parse rather than once for every level around it. Counted by the
/// records the commits looked at, under <c>DOTGRAM_COUNTS</c>, and not timed.
/// </summary>
/// <remarks>
/// The shape came from a grammar of type patterns: the tail of every list atomic, as in
/// <c>'(' &amp; first: T &amp; { (',' &amp; more: T)* } &amp; ')'</c>, read by the automaton. A value collected in
/// the tail is a call whose records stay on the arena, so every level's commit looked again at
/// everything the levels inside it had recorded and committed: ten thousand nested tuples took two
/// seconds, and a type nested as deep five. The direct reader has no arena and never had the cost;
/// these grammars are compiled without it.
/// </remarks>
public sealed class CommitWalkTests
{
	/// <summary>A list whose atomic tail holds the nest: <c>(x, (x, (x, x)))</c>.</summary>
	const string Tail = """
		trivia = { ' '* }
		Start : @int = t: T & eof => @(t)
		T : @int = '(' & first: T & { (',' & more: T)* } & ')' => @(first + more.Length + 1)
		         | 'x' => @(0)
		parse Start stream
		""";

	/// <summary>Alternatives and conjunctions, each list's tail atomic: <c>x | (x | (x))</c>.</summary>
	const string AndOr = """
		trivia = { ' '* }
		Start : @int = o: Or & eof => @(o)
		Or    : @int = first: And & { ('|' & more: And)* } => @(first + more.Length)
		And   : @int = first: Un & { ('&' & more: Un)* } => @(first + more.Length)
		Un    : @int = '(' & o: Or & ')' => @(o + 1)
		             | 'x' => @(0)
		parse Start stream
		""";

	/// <summary>The tail's nest read first by a lookahead that hands its value on, then again.</summary>
	const string Looked = """
		trivia = { ' '* }
		Start : @int = v: ?=T & t: T & eof => @(v + t)
		T : @int = '(' & first: T & { (',' & more: T)* } & ')' => @(first + more.Length + 1)
		         | 'x' => @(0)
		parse Start stream
		""";

	public enum Shape
	{
		Tail,
		Or,
		And,
		Looked,
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

	/// <summary>The carriers that build differently, every shape, a few depths.</summary>
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
		return shape switch
		{
			Shape.Tail   => Tail,
			Shape.Or     => AndOr,
			Shape.And    => AndOr,
			Shape.Looked => Looked,
			_            => throw new ArgumentOutOfRangeException(nameof(shape)),
		};
	}

	static string Nest(Shape shape, int depth)
	{
		var open = shape switch
		{
			Shape.Or  => "x | (",
			Shape.And => "x & (",
			_         => "(x, ",
		};

		return string.Concat(Enumerable.Repeat(open, depth)) + "x" + new string(')', depth);
	}

	/// <summary>
	/// Twice the depth, at most a little over twice the records looked at: linear. Looking at every
	/// record above the boundary, each doubling looked at four times as many: 2,880,400 records for
	/// tuples nested 800 deep, which are now 7,196.
	/// </summary>
	[Theory]
	[MemberData(nameof(Counted))]
	public void A_nest_of_commits_looks_at_each_record_once(CarrierKind carrier, Shape shape, Form form)
	{
		var assembly = Compiled(Grammar(shape), carrier, direct: false, counts: true);

		long[] looked = [.. new[] { 100, 200, 400, 800 }.Select(depth => Passed(assembly, Nest(shape, depth), form))];

		Assert.True(
			looked[0] > 0 && looked[3] <= 2.2 * looked[2] && looked[2] <= 2.2 * looked[1] && looked[1] <= 2.2 * looked[0],
			$"{string.Join(", ", looked)} records looked at by commits at 100, 200, 400 and 800 levels.");

		Assert.True(looked[3] <= 40 * 800, $"{looked[3]} records looked at by commits at 800 levels.");
	}

	/// <summary>
	/// What the automaton answers is what the direct reader answers, on nests that hold and on nests
	/// cut short, where the ways the commits put out are what a refusal would have gone back to.
	/// </summary>
	[Theory]
	[MemberData(nameof(Answered))]
	public void A_nest_of_commits_answers_what_the_reader_answers(CarrierKind carrier, Shape shape, int depth)
	{
		var engine = Compiled(Grammar(shape), carrier, direct: false, counts: false);
		var reader = Compiled(Grammar(shape), carrier, direct: true, counts: false);
		var nest   = Nest(shape, depth);

		foreach (var input in new[] { nest, nest + ")", nest.Substring(0, nest.Length / 2), nest.Replace("x)", "x,)"), nest.Replace(", ", " ") })
			Assert.Equal(
				EmittedCode.Match(reader, "Grammar", "TryParseStart", input),
				EmittedCode.Match(engine, "Grammar", "TryParseStart", input));
	}

	/// <summary>
	/// A group still commits its own choice when a group inside it committed first: the outer group
	/// took the <c>x</c>, and giving it back would let <c>'x' &amp; 'q'</c> read <c>xq</c>; it keeps what it
	/// took and the input is refused, by the automaton as by the reader.
	/// </summary>
	[Theory]
	[InlineData("ixq", false)]
	[InlineData("ixxq", true)]
	[InlineData("(i)xq", false)]
	[InlineData("(i)xxq", true)]
	[InlineData("(((i)))xq", false)]
	[InlineData("(((i)))xxq", true)]
	public void A_group_around_a_committed_one_still_commits_its_own_choice(string input, bool read)
	{
		const string grammar = """
			Start : @int = g: G & 'x' & 'q' & eof => @(g)
			G     : @int = { a: A & 'x'? } => @(a)
			A     : @int = { '(' & i: A & ')' } => @(i + 1)
			             | 'i' => @(0)
			parse Start
			""";

		foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Deferred })
		{
			var engine = Compiled(grammar, carrier, direct: false, counts: false);
			var reader = Compiled(grammar, carrier, direct: true, counts: false);

			Assert.Equal(read, EmittedCode.Match(engine, "Grammar", "TryParseStart", input).IsSuccess);
			Assert.Equal(EmittedCode.Match(reader, "Grammar", "TryParseStart", input), EmittedCode.Match(engine, "Grammar", "TryParseStart", input));
		}
	}

	static long Passed(Assembly assembly, string input, Form form)
	{
		var counters = assembly.GetTypes()
			.SelectMany(static type => type.GetFields(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
			.Where(static field => field.Name.StartsWith("PassedOver", StringComparison.Ordinal))
			.ToList();

		Assert.NotEmpty(counters);

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		var type  = assembly.GetType("Grammar")!;
		var match = form == Form.String
			? type.GetMethod("TryParseStart", [typeof(string)])!.Invoke(null, [input])!
			: type.GetMethod("TryParseStart", [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [new StringReader(input), null, null])!;

		Assert.True((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!, $"{form} refused {input.Length} characters.");

		return counters.Sum(static counter => (long)counter.GetValue(null)!);
	}

	static Assembly Compiled(string grammar, CarrierKind carrier, bool direct, bool counts)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = carrier,
			Direct        = direct,
			CountRules    = counts,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, symbols: counts ? ["DOTGRAM_COUNTS"] : null);
	}
}
