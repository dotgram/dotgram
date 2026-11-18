using System;
using System.Linq;
using System.Threading;

using DotGram.Grammar.Fragments;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests.Fragments;

/// <summary>
/// A fragment as deep as a grammar can make it, lowered where the stack is smallest: a thread of a
/// megabyte, which is what an analyzer gets on Windows.
/// </summary>
public sealed class FragmentLoweringDepthTests
{
	const int Terms = 20_000;

	[Fact]
	public void A_long_left_leaning_chain_lowers_on_a_small_stack()
	{
		var text   = string.Join(" + ", Enumerable.Repeat("a", Terms));
		var result = OnSmallStack(() => RoslynFragmentLowering.Lower(text));

		Assert.True(result.IsPortable);
		Assert.Equal(text.Length, result.Text.Length);
		Assert.Equal(Terms, Count(result.Root));
	}

	[Fact]
	public void A_long_member_chain_lowers_on_a_small_stack()
	{
		var text   = "a" + string.Concat(Enumerable.Repeat(".b", Terms));
		var result = OnSmallStack(() => RoslynFragmentLowering.Lower(text));

		Assert.True(result.IsPortable, string.Join(", ", result.Refusals.Select(static one => one.Form)));
		Assert.Equal(text.Length, result.Root.Span.Length);
	}

	[Fact]
	public void A_long_chain_refuses_at_the_same_places_on_a_small_stack_as_on_a_large_one()
	{
		var text  = string.Join(" * ", Enumerable.Repeat("a", 3000));
		var small = OnSmallStack(() => RoslynFragmentLowering.Lower(text));
		var large = RoslynFragmentLowering.Lower(text);

		Assert.False(small.IsPortable);
		Assert.Equal(large.Refusals.Select(static one => (one.Form, one.Span)), small.Refusals.Select(static one => (one.Form, one.Span)));
	}

	static int Count(Fragment fragment)
	{
		var count = 0;

		// The tree is as deep as the chain is long, so it is counted along its left edge.
		while (fragment is Fragment.Binary binary)
		{
			count++;

			if (binary.Right is not Fragment.Name)
				throw new InvalidOperationException("Not a chain of names.");

			fragment = binary.Left;
		}

		return count + 1;
	}

	static T OnSmallStack<T>(Func<T> work)
	{
		var result = default(T);
		var thrown = default(Exception);
		var thread = new Thread(
			() =>
			{
				try
				{
					result = work();
				}
				catch (Exception caught)
				{
					thrown = caught;
				}
			},
			1024 * 1024);

		thread.Start();
		thread.Join();

		if (thrown is not null)
			throw new InvalidOperationException("The lowering threw.", thrown);

		return result!;
	}
}
