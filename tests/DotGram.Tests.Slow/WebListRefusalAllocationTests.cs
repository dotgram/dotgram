using System;
using System.Linq;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A refused list header allocates what its elements need, not their square.
/// </summary>
/// <remarks>
/// <para>
/// <c>ForwardedField</c>'s <c>ElementList</c> and <c>Link</c>'s <c>Field</c> both read a comma
/// list with no closing delimiter: what ends it is running out of list to read. A field of n good
/// elements followed by one bad one (an unclosed value, an unclosed target) leaves that bad tail
/// unconsumed, which only the outermost <c>eof</c> check — outside the list — notices. Before the
/// list was sealed, that failure replayed <c>ElementList</c> / <c>Field</c> from their own start,
/// giving one element back a retry and re-reading, and re-materialising, every element still
/// standing: n retries of O(n), values rebuilt every time. Measured before sealing: 1.17 GB at
/// 1,024 Forwarded elements, 414 MB at 1,024 links, both refused. Sealing the list (<c>{ }</c>)
/// commits it once it has read as many elements as it can, so a failure past it cannot ask for
/// fewer — the greedy reading is the only one a comma list ever meant, and nothing downstream
/// loses by that commit standing.
/// </para>
/// <para>
/// <b>The gate is bytes, not a bound at one size.</b> A quadratic allocator still passes a single
/// large-input ceiling if the ceiling is loose enough to admit ordinary noise, so this asks the
/// same question <see cref="BlockScalingTests"/> does, at four doubling sizes: bytes an element
/// stays flat, rather than reading a total against one number picked to fit a first measurement.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class WebListRefusalAllocationTests
{
	/// <summary>Doubling sizes; the note's own worst case (1,024) is the top rung.</summary>
	static readonly int[] Sizes = [128, 256, 512, 1024];

	[Fact]
	public void A_refused_Forwarded_field_still_refuses_at_every_size()
	{
		foreach (var size in Sizes)
			Assert.False(ForwardedElement.TryParseField(ForwardedRefusal(size), out _), $"{size} elements were read.");
	}

	[Fact]
	public void A_refused_Link_field_still_refuses_at_every_size()
	{
		foreach (var size in Sizes)
			Assert.False(WebLink.TryParseField(LinkRefusal(size), out _), $"{size} links were read.");
	}

	/// <summary>Four times the elements, and what a refusal allocates grows by about four — not sixteen.</summary>
	[Fact]
	public void A_refused_Forwarded_field_allocates_about_as_many_bytes_an_element_at_every_size()
	{
		AssertLinear(size => Allocated(() => ForwardedElement.TryParseField(ForwardedRefusal(size), out _)));
	}

	/// <summary>Four times the links, and what a refusal allocates grows by about four — not sixteen.</summary>
	[Fact]
	public void A_refused_Link_field_allocates_about_as_many_bytes_a_link_at_every_size()
	{
		AssertLinear(size => Allocated(() => WebLink.TryParseField(LinkRefusal(size), out _)));
	}

	/// <summary>
	/// Asserts bytes an element stay within 15% of one another at every doubling in <see cref="Sizes"/>. A
	/// quadratic allocator doubles bytes an element at every doubling in the count; this catches that at
	/// each of three steps rather than only between the first and the last.
	/// </summary>
	static void AssertLinear(Func<int, long> allocatedFor)
	{
		var perElement = Sizes.Select(size => allocatedFor(size) / (double)size).ToArray();

		for (var i = 1; i < Sizes.Length; i++)
			Assert.True(
				perElement[i] <= perElement[i - 1] * 1.15,
				$"At {Sizes[i - 1]} elements a refusal allocated {perElement[i - 1]:F1} bytes each, and at " +
				$"{Sizes[i]} it allocated {perElement[i]:F1} bytes each — {perElement[i] / perElement[i - 1]:F2} " +
				"times as many, where a list that does not re-read itself would stay flat.");
	}

	/// <summary>Bytes this thread allocates running <paramref name="action"/>, warmed up first so pool growth is not counted.</summary>
	static long Allocated(Action action)
	{
		action();
		action();

		var before = GC.GetAllocatedBytesForCurrentThread();

		action();

		return GC.GetAllocatedBytesForCurrentThread() - before;
	}

	/// <summary><paramref name="elements"/> valid Forwarded elements, then one with no value after <c>for=</c>.</summary>
	static string ForwardedRefusal(int elements)
	{
		return string.Join(", ", Enumerable.Range(0, elements).Select(GoodElement)) + ", for=";
	}

	static string GoodElement(int index)
	{
		return "for=192.0." + (index / 250 % 250) + "." + (index % 250 + 1);
	}

	/// <summary><paramref name="links"/> valid link-values, then one whose target is never closed.</summary>
	static string LinkRefusal(int links)
	{
		return string.Join(", ", Enumerable.Range(0, links).Select(GoodLink)) + ", <unclosed";
	}

	static string GoodLink(int index)
	{
		return "<https://example.com/" + index + ">; rel=next";
	}
}
