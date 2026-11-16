using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// How many times each <c>=&gt;</c> runs, counted by the constructions themselves, under each carrier:
/// on a feed a <c>recover</c> steps through, read from a string and from a reader, and on a choice
/// whose first alternative is given up.
/// </summary>
/// <remarks>
/// <para>
/// docs/syntax.md §3.7 says when a construction runs and leaves the count on a reading given up
/// unpromised; §6.3 says a streamed reading may run a construction that changes state a different
/// number of times than a string does. These tests write down what the counts are today, so that
/// a change to any of them is seen and decided rather than found by a user whose construction
/// counts. They do not assert the order the constructions run in, which nothing promises (over a
/// string the recovery's factory runs before the records around it, and the inner rule's
/// constructions before the outer ones).
/// </para>
/// <para>
/// <c>Auto</c>, <c>Immediate</c> and <c>Deferred</c> — the tape, which was called <c>Tape</c> before
/// 0.3.0 — are each asked. A repetition marked <c>recover</c> is carried by the tape whatever is
/// named: <c>Immediate</c> is told so (<c>GRAM5007</c>) and its counts are the tape's.
/// </para>
/// </remarks>
public sealed class ConstructionCountTests
{
	const string Members = """
		public static global::System.Collections.Generic.List<string> Calls = new global::System.Collections.Generic.List<string>();
		static int Record(int d) { Calls.Add("Row" + d); return d; }
		static int Digit(string d) { Calls.Add("Num" + d); return d[0] - '0'; }
		static int Broken() { Calls.Add("Broken"); return -1; }
		static int Shared() { Calls.Add("A"); return 7; }
		static int First(int a) { Calls.Add("X"); return a; }
		static int Second(int a) { Calls.Add("Y"); return a; }
		""";

	/// <summary>A record with a rule inside it, so a broken record has a construction it reached before it broke.</summary>
	const string Recovering = """
		Num  : @int   = d: ['0'..'9']                => @(Digit(d))
		Row  : @int   = 'R' & d: Num & '\n'          => @(Record(d))
		Rows : @int[] = Row* recover '\n'            => @(Broken())
		parse Rows
		""";

	/// <summary>
	/// The second record is broken after its digit, so its <c>Num</c> was read and its <c>Row</c> was not.
	/// </summary>
	const string Feed = "R1\nR5x\nR2\n";

	/// <summary>
	/// Two alternatives that both begin with <c>A</c>, captured under two names so that the generator
	/// does not read it once for both (§3.7 says it may, where the names agree).
	/// </summary>
	const string Choosing = """
		Start : @int = a: A & 'x' => @(First(a))
		             | b: A & 'y' => @(Second(b))
		A     : @int = 'a'        => @(Shared())
		parse Start
		""";

	/// <summary>
	/// Over a string every construction of an accepted record runs once, the recovery's once, and
	/// the inner rule of the broken record not at all — under every carrier, because the tape carries
	/// a <c>recover</c> under every one.
	/// </summary>
	[Theory]
	[InlineData(CarrierKind.Auto)]
	[InlineData(CarrierKind.Immediate)]
	[InlineData(CarrierKind.Deferred)]
	public void Under_recover_from_a_string_each_accepted_construction_runs_once(CarrierKind carrier)
	{
		var (host, told) = Compile(Recovering, carrier);
		var values       = Run(host, "ParseRows", Feed);

		Assert.Equal([1, -1, 2], (int[])values.Value!);
		Assert.Equal("Broken Num1 Num2 Row1 Row2", values.Calls);
		Assert.Equal(carrier == CarrierKind.Immediate, told.Contains(GramCompiler.CarrierRefused));
	}

	/// <summary>
	/// Over a reader the same, except the last record: read once to the end of what the window held,
	/// and read again once the reader has said there is no more, so its constructions run twice.
	/// </summary>
	/// <remarks>
	/// §6.3 allows this — "a result that reaches the end of what is held is not an answer while the
	/// reader has more, and is read again" — and it holds under the tape as well, which runs a
	/// construction once per derivation accepted over a string (§3.7). It is the reader that reads
	/// the last record twice; the carriers agree with each other.
	/// </remarks>
	[Theory]
	[InlineData(CarrierKind.Auto)]
	[InlineData(CarrierKind.Immediate)]
	[InlineData(CarrierKind.Deferred)]
	public void Under_recover_from_a_reader_the_last_records_constructions_run_twice(CarrierKind carrier)
	{
		var (host, _) = Compile(Recovering, carrier);
		var values    = Run(host, "ParseRows", new StringReader(Feed));

		Assert.Equal([1, -1, 2], (int[])values.Value!);
		Assert.Equal("Broken Num1 Num2 Num2 Row1 Row2 Row2", values.Calls);
	}

	/// <summary>
	/// The first alternative reads <c>A</c> and is given up at <c>y</c>: the immediate carrier has run
	/// <c>A</c>'s construction for it and runs it again for the second; the tape runs it once, for the
	/// alternative that stood.
	/// </summary>
	[Theory]
	[InlineData(CarrierKind.Auto,      "A A Y")]
	[InlineData(CarrierKind.Immediate, "A A Y")]
	[InlineData(CarrierKind.Deferred,  "A Y")]
	public void A_construction_of_an_alternative_given_up_runs_where_the_carrier_says(CarrierKind carrier, string expected)
	{
		var (host, _) = Compile(Choosing, carrier);
		var values    = Run(host, "ParseStart", "ay");

		Assert.Equal(7, values.Value);
		Assert.Equal(expected, values.Calls);
	}

	/// <summary>
	/// Refused input: under the immediate carrier the construction of every alternative read has run,
	/// once each, though nothing is accepted; under the tape none has.
	/// </summary>
	[Theory]
	[InlineData(CarrierKind.Auto,      "A A")]
	[InlineData(CarrierKind.Immediate, "A A")]
	[InlineData(CarrierKind.Deferred,  "")]
	public void A_refused_input_runs_what_the_carrier_ran_before_it_was_refused(CarrierKind carrier, string expected)
	{
		var (host, _) = Compile(Choosing, carrier);

		Assert.Throws<FormatException>(() => Run(host, "ParseStart", "az"));
		Assert.Equal(expected, Calls(host));
	}

	static (object? Value, string Calls) Run(Type host, string method, object input)
	{
		Reset(host);

		object? value;

		try
		{
			value = host.GetMethod(method, [input is TextReader ? typeof(TextReader) : typeof(string)])!.Invoke(null, [input]);
		}
		catch (TargetInvocationException e)
		{
			throw e.InnerException!;
		}

		if (value is IEnumerable values)
			value = values.Cast<int>().ToArray();

		return (value, Calls(host));
	}

	static void Reset(Type host)
	{
		((List<string>)host.GetField("Calls")!.GetValue(null)!).Clear();
	}

	/// <summary>What the constructions said, sorted: how many times each ran, and not in what order.</summary>
	static string Calls(Type host)
	{
		return string.Join(" ", ((List<string>)host.GetField("Calls")!.GetValue(null)!).OrderBy(static one => one, StringComparer.Ordinal));
	}

	static (Type Host, string[] Told) Compile(string grammar, CarrierKind carrier)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = carrier,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var host = EmittedCode.Compile(Assert.Single(result.Sources).Text, declarationMembers: Members).GetType("Grammar")!;

		return (host, result.Diagnostics.Select(static one => one.Id).ToArray());
	}
}
