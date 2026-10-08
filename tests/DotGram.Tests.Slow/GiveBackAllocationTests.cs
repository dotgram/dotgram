using System;
using System.IO;
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

	/// <summary>Which form reads the text: over a string, over a <c>TextReader</c>, or over a <c>Stream</c> of bytes.</summary>
	public enum Form { Text, Reader, Stream }

	[Theory]
	[InlineData(CarrierKind.Immediate, Form.Text)]
	[InlineData(CarrierKind.Immediate, Form.Reader)]
	[InlineData(CarrierKind.Immediate, Form.Stream)]
	[InlineData(CarrierKind.Tape, Form.Text)]
	public void A_refused_list_allocates_about_as_many_bytes_a_member_at_every_size(CarrierKind carrier, Form form)
	{
		var host = Compile(carrier, form);

		AssertLinear(host, "TryParseList", form, static n => ListOf(n) + ", <", static n => ListOf(n));
	}

	[Theory]
	[InlineData(CarrierKind.Immediate, Form.Text)]
	[InlineData(CarrierKind.Immediate, Form.Reader)]
	[InlineData(CarrierKind.Immediate, Form.Stream)]
	[InlineData(CarrierKind.Tape, Form.Text)]
	public void A_refused_pair_allocates_about_as_many_bytes_a_member_at_every_size(CarrierKind carrier, Form form)
	{
		var host = Compile(carrier, form);

		AssertLinear(host, "TryParsePair", form, static n => ListOf(n) + ", z?", static n => ListOf(n) + ", z!");
	}

	/// <summary>
	/// Bytes a refused text allocates grow by about two per doubling, and stay under about twice what
	/// its accepted twin allocates: the first two attempts are built, the attempts after them are not.
	/// Over a reader or a stream the buffer the call rents and the reader over it are the constant.
	/// </summary>
	static void AssertLinear(Type host, string entry, Form form, Func<int, string> refused, Func<int, string> accepted)
	{
		var read = Reader(host, entry, form);

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
				refusedBytes <= 2.1 * acceptedBytes + 16384,
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

	static Func<string, bool> Reader(Type host, string entry, Form form)
	{
		var method = form switch
		{
			Form.Reader => host.GetMethod(entry, [typeof(TextReader), typeof(int?), typeof(int?)])!,
			Form.Stream => host.GetMethod(entry, [typeof(Stream), typeof(int?), typeof(int?)])!,
			_ => host.GetMethod(entry, [typeof(string)])!,
		};
		var isSuccess = method.ReturnType.GetProperty("IsSuccess")!;

		return form switch
		{
			Form.Reader => text => (bool)isSuccess.GetValue(method.Invoke(null, [new StringReader(text), null, null]))!,
			Form.Stream => text => (bool)isSuccess.GetValue(method.Invoke(null, [new MemoryStream(System.Text.Encoding.ASCII.GetBytes(text)), null, null]))!,
			_ => text => (bool)isSuccess.GetValue(method.Invoke(null, [text]))!,
		};
	}

	static Type Compile(CarrierKind carrier, Form form)
	{
		// The stream form is published beside the string one (`stream bytes`), and reads bytes.
		var grammar = form == Form.Stream
			? Grammar.Replace("parse List", "parse List stream bytes").Replace("parse Pair", "parse Pair stream bytes")
			: Grammar;

		var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = carrier, CSharpScanner = RoslynCSharpScanner.Instance, BufferedInput = form != Form.Text,
		});

		Assert.DoesNotContain(compiled.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(compiled.Sources).Text;

		// The immediate reading is the one measured; the tape beside it is what it is held near.
		Assert.Equal(carrier == CarrierKind.Immediate, source.Contains("Retried_DotGram(", StringComparison.Ordinal));

		return EmittedCode.Compile(source, declarationMembers: Members).GetType("Grammar")!;
	}
}
