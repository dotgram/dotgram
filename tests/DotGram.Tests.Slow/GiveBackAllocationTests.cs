using System;
using System.Linq;
using System.Reflection;
using System.Threading;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A list refused after its last member allocates, on the immediate carrier, what its members
/// need and not their square: the attempts after a turn is given back read unbuilt, and the one
/// that stands is built once (<c>GiveBackDemandTests</c> holds the counts; this holds the bytes).
/// </summary>
/// <remarks>
/// <para>
/// The turn <c>Ows ',' Ows Item</c> begins with what follows the list begins with, so no turn can
/// be proven final and every one opens a way; a text refused past the list gives them back one at a
/// time. Before the attempts after a give-back read unbuilt, the carrier that builds as it reads
/// rebuilt every standing member on every attempt: 6 MB at 500 members, 24 MB at 1,000, 96 MB at
/// 2,000 — four times as much at every doubling, where the tape allocates nothing at all. The
/// <c>Pair</c> shape is kept beside the list: its tail takes a member back whatever a later analysis
/// proves about the list's own turns, so it keeps this measured once lists like the one here are
/// proven final.
/// </para>
/// <para>
/// <b>The gate is the growth, at four doubling sizes, and the accepted twin.</b> A ratio taken at
/// one pair of sizes lets a small quadratic term through; four sizes do not. And a refusal that
/// allocates more than about what the same text accepted allocates has rebuilt something: the one
/// extra reading the attempt that stands costs is bounded by the accepted twin's own bytes.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class GiveBackAllocationTests
{
	const string Grammar = """
		@using System.Collections.Generic;
		trivia = none
		Ows  = [' ']*
		Word = ['a'..'z']+
		List : @List<string> = first: Item & rest: Rest* & Ows => @(Join(first, rest))
		Rest : @string = Ows & ',' & Ows & item: Item => @(item)
		Item : @string = text: Word => @(text.ToString())
		Pair : @List<string> = l: List & ',' & Ows & 'z' & '!' => @(Mark(l))
		parse List
		parse Pair
		""";

	const string Members = """
		static System.Collections.Generic.List<string> Join(string first, string[] rest)
		{
			var list = new System.Collections.Generic.List<string>(rest.Length + 1) { first };
			list.AddRange(rest);
			return list;
		}
		static System.Collections.Generic.List<string> Mark(System.Collections.Generic.List<string> list)
		{
			list.Add("#");
			return list;
		}
		""";

	static readonly int[] Sizes = [500, 1000, 2000, 4000];

	static string ListOf(int count)
	{
		return string.Join(", ", Enumerable.Repeat("ab", count));
	}

	[Theory]
	[InlineData(CarrierKind.Immediate)]
	[InlineData(CarrierKind.Tape)]
	public void A_refused_list_allocates_about_as_many_bytes_a_member_at_every_size(CarrierKind carrier)
	{
		var host = Compile(carrier);

		AssertLinear(host, "TryParseList", static n => ListOf(n) + ", <", static n => ListOf(n));
	}

	[Theory]
	[InlineData(CarrierKind.Immediate)]
	[InlineData(CarrierKind.Tape)]
	public void A_refused_pair_allocates_about_as_many_bytes_a_member_at_every_size(CarrierKind carrier)
	{
		var host = Compile(carrier);

		AssertLinear(host, "TryParsePair", static n => ListOf(n) + ", z?", static n => ListOf(n) + ", z!");
	}

	/// <summary>
	/// Bytes a refused text allocates grow by about two per doubling, and stay under about twice what
	/// its accepted twin allocates: the first two attempts are built, the attempts after them are not.
	/// </summary>
	static void AssertLinear(Type host, string entry, Func<int, string> refused, Func<int, string> accepted)
	{
		var read = Reader(host, entry);

		// Every size once before anything is measured, so that no pool grows inside a measurement.
		foreach (var size in Sizes)
		{
			Assert.False(read(refused(size)), $"{size} members were read.");
			Assert.True(read(accepted(size)), $"{size} members were refused.");
		}

		var bytes = Sizes.Select(size => (Size: size, Refused: Measure(read, refused(size)), Accepted: Measure(read, accepted(size)))).ToArray();
		var shown = string.Join("; ", bytes.Select(static one => $"{one.Size}: refused {one.Refused:N0}, accepted {one.Accepted:N0}"));

		for (var i = 1; i < bytes.Length; i++)
			Assert.True(
				bytes[i].Refused <= 2.2 * bytes[i - 1].Refused + 1024,
				$"A refusal at {bytes[i].Size} members allocates {(double)bytes[i].Refused / Math.Max(1, bytes[i - 1].Refused):F2} times what it does at {bytes[i - 1].Size}: {shown}.");

		foreach (var (size, refusedBytes, acceptedBytes) in bytes)
			Assert.True(
				refusedBytes <= 2.1 * acceptedBytes + 4096,
				$"A refusal at {size} members allocates {refusedBytes:N0} bytes where the accepted text allocates {acceptedBytes:N0}: {shown}.");
	}

	/// <summary>The fewest bytes of five calls, on a thread of its own with room for the deepest reading.</summary>
	static long Measure(Func<string, bool> read, string text)
	{
		var best = long.MaxValue;
		Exception? failed = null;

		var thread = new Thread(() =>
		{
			try
			{
				read(text);

				for (var i = 0; i < 5; i++)
				{
					var before = GC.GetAllocatedBytesForCurrentThread();
					read(text);
					best = Math.Min(best, GC.GetAllocatedBytesForCurrentThread() - before);
				}
			}
			catch (Exception e)
			{
				failed = e;
			}
		}, 64 * 1024 * 1024);

		thread.Start();
		thread.Join();

		if (failed is not null)
			throw new InvalidOperationException("The measured reading failed.", failed);

		return best;
	}

	static Func<string, bool> Reader(Type host, string entry)
	{
		var method    = host.GetMethod(entry, [typeof(string)])!;
		var isSuccess = method.ReturnType.GetProperty("IsSuccess")!;

		return text => (bool)isSuccess.GetValue(method.Invoke(null, [text]))!;
	}

	static Type Compile(CarrierKind carrier)
	{
		var compiled = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = carrier, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.DoesNotContain(compiled.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(compiled.Sources).Text;

		// The immediate reading is the one measured; the tape beside it is what it is held near.
		Assert.Equal(carrier == CarrierKind.Immediate, source.Contains("unbuilt = 1;", StringComparison.Ordinal));

		return EmittedCode.Compile(source, declarationMembers: Members).GetType("Grammar")!;
	}
}
