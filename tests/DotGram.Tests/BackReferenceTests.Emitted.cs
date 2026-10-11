using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The rows read by generated parsers: on every carrier, by the reader and by the engine, as a
/// whole input, from a position, and searched for — every reading agreeing with the semantics and
/// with every other on the answer, the value, and where and why a refusal stopped.
/// </summary>
/// <remarks>
/// Each test skips until the back end it needs generates code for bindings
/// (<see cref="BackReferenceEmission"/>).
/// </remarks>
public sealed partial class BackReferenceTests
{
	static readonly CarrierKind[] Carriers = [CarrierKind.Tape, CarrierKind.Immediate, CarrierKind.Auto];

	[Theory]
	[MemberData(nameof(RowNames))]
	public void The_reader_decides_every_row_on_every_carrier(string name)
	{
		BackReferenceEmission.RequireReader();

		var row      = Named(name);
		var readings = Carriers.ToDictionary(static carrier => carrier, carrier => Readings(row, carrier, direct: true));

		foreach (var (carrier, read) in readings)
			Decided(row, read, $"reader, {carrier}");

		Agree(row, readings.Select(static one => ($"reader, {one.Key}", one.Value)));
	}

	[Theory]
	[MemberData(nameof(RowNames))]
	public void The_engine_reads_every_row_as_the_reader_does(string name)
	{
		BackReferenceEmission.RequireBoth();

		var row      = Named(name);
		var readings = new List<(string, Reading[])> { ("reader, Tape", Readings(row, CarrierKind.Tape, direct: true)) };

		foreach (var carrier in Carriers)
		{
			var read = Readings(row, carrier, direct: false);

			Decided(row, read, $"engine, {carrier}");
			readings.Add(($"engine, {carrier}", read));
		}

		Agree(row, readings);
	}

	[Theory]
	[MemberData(nameof(FindRowNames))]
	public void Find_finds_every_occurrence_on_every_carrier(string name)
	{
		BackReferenceEmission.RequireEngine();

		var row = Named(name);

		foreach (var carrier in Carriers)
			foreach (var direct in new[] { true, false })
			{
				var assembly = Compiled(row.Published(), carrier, direct);

				Assert.Equal(
					row.Found!.Select(static at => (long)at),
					EmittedCode.FoundAt(assembly, "Grammar", "Find" + row.Rule, row.Text!));
			}
	}

	/// <summary>
	/// A reading begun inside a longer text, and inside a window of one, stops where the same
	/// reading of the text alone does: a binding's span and a reference's comparison are both
	/// positions, and an offset that leaked into either would read the wrong items.
	/// </summary>
	[Theory]
	[InlineData("CSharpRawString", true)]
	[InlineData("CSharpRawString", false)]
	[InlineData("XmlElement", true)]
	[InlineData("XmlElement", false)]
	[InlineData("CountBetweenTwoLengths", true)]
	[InlineData("CountBetweenTwoLengths", false)]
	public void A_reading_begun_inside_a_longer_text_reads_the_same(string name, bool direct)
	{
		if (direct)
			BackReferenceEmission.RequireReader();
		else
			BackReferenceEmission.RequireBoth();

		var row      = Named(name);
		var assembly = Compiled(row.Grammar + $"\nparse {row.Rule}\n", CarrierKind.Auto, direct);
		var method   = "TryParse" + row.Rule;

		foreach (var input in row.Accepted.Concat(row.Refused))
		{
			var alone  = EmittedCode.Answered(assembly, "Grammar", method, input, 0);
			var inside = EmittedCode.Answered(assembly, "Grammar", method, "##" + input + "##", 2);
			var window = EmittedCode.Answered(assembly, "Grammar", method, "##" + input + "##", 2, input.Length);

			Assert.Equal((alone.Read, alone.At), (window.Read, window.At - 2));

			// Past the input there is more text, which none of these rows reads on into.
			if (alone.Read && alone.At == input.Length)
				Assert.Equal((true, input.Length), (inside.Read, inside.At - 2));
		}
	}

	/// <summary>
	/// A rule with members and a binding: the binding is no member and takes no member's place,
	/// and the capture of a reference holds the text the reference matched.
	/// </summary>
	[Theory]
	[InlineData(CarrierKind.Tape, true)]
	[InlineData(CarrierKind.Immediate, true)]
	[InlineData(CarrierKind.Auto, true)]
	[InlineData(CarrierKind.Tape, false)]
	[InlineData(CarrierKind.Immediate, false)]
	[InlineData(CarrierKind.Auto, false)]
	public void A_binding_beside_members_leaves_them_as_they_are(CarrierKind carrier, bool direct)
	{
		if (direct)
			BackReferenceEmission.RequireReader();
		else
			BackReferenceEmission.RequireBoth();

		var assembly = Compiled(
			"""
			R : @string = c: ($q: W) & ' ' & n: N & ' ' & r: $q => @(c + "|" + n + "|" + r)
			N : @int    = ['0'..'9'] => @(parserText[0] - '0')
			W           = ['a'..'z']+
			parse R
			""",
			carrier,
			direct);

		Assert.Equal("ab|7|ab", EmittedCode.Match(assembly, "Grammar", "TryParseR", "ab 7 ab").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseR", "ab 7 ac").IsSuccess);
	}

	// ── Helpers ─────────────────────────────────────────────────────────────────

	/// <summary>What one parser said about one input.</summary>
	sealed record Reading(string Input, bool IsSuccess, string? Value, string? Error, long Position);

	static Reading[] Readings(Row row, CarrierKind carrier, bool direct)
	{
		var assembly = Compiled(row.Grammar + $"\nparse {row.Rule}\n", carrier, direct);

		return
		[
			.. row.Accepted.Concat(row.Refused).Select(input =>
			{
				var (isSuccess, value, error, position) = EmittedCode.Match(assembly, "Grammar", "TryParse" + row.Rule, input);

				return new Reading(input, isSuccess, Spelled(value), error, position);
			}),
		];
	}

	/// <summary>Every input decided as the row says.</summary>
	static void Decided(Row row, Reading[] readings, string how)
	{
		foreach (var reading in readings)
		{
			var expected = row.Accepted.Contains(reading.Input);

			Assert.True(
				reading.IsSuccess == expected,
				$"{row.Name}, {how}: \"{Escaped(reading.Input)}\" is {(expected ? "refused" : "read")}" +
				(reading.Error is null ? "" : $" ({reading.Error} at {reading.Position})"));
		}
	}

	/// <summary>Every parser said the same about every input: answer, value, and the refusal's place and words.</summary>
	static void Agree(Row row, IEnumerable<(string How, Reading[] Readings)> all)
	{
		var parsers = all.ToArray();
		var (firstHow, first) = parsers[0];

		foreach (var (how, readings) in parsers.Skip(1))
			for (var i = 0; i < first.Length; i++)
				Assert.True(
					first[i] == readings[i],
					$"{row.Name}: \"{Escaped(first[i].Input)}\"\n  {firstHow}: {first[i]}\n  {how}: {readings[i]}");
	}

	static string? Spelled(object? value)
	{
		return value switch
		{
			null                           => null,
			string text                    => text,
			System.Collections.IEnumerable many => "[" + string.Join(",", many.Cast<object?>().Select(Spelled)) + "]",
			_                              => value.ToString(),
		};
	}

	static Assembly Compiled(string grammar, CarrierKind carrier, bool direct)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier       = carrier,
			Direct        = direct,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static source => source.Text));
	}
}
