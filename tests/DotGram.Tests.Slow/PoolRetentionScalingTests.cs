using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A parse reuses what the parse before it grew, however large the document was.
/// </summary>
/// <remarks>
/// <para>
/// Everything a parse needs beyond the tree — the arena, the value tables, the ways and the
/// links — is grown once and kept on the thread for the next parse. Kept, that is, up to a
/// bound, past which it was let go: and a document past the bound therefore paid for the whole
/// machinery again on every parse, growing it from nothing by doubling. That is not a bend in
/// the curve but a cliff: one entry over and everything goes, so a document a third larger than
/// the last good one cost twenty times the memory (finance-24, 2026-09-19).
/// </para>
/// <para>
/// What a second parse of the same text allocates, against what the first allocated, is exactly
/// the question: reuse is the whole of the difference between them. Below the bound the ratio
/// is a fifth or less — the second parse still builds the tree, which is the floor and not
/// zero — and above it, before this was fixed, it was 1.00 to three decimals, the second parse
/// doing everything the first did.
/// </para>
/// <para>
/// The sizes are finance-24's, measured rather than reasoned: the arena's bound falls at about
/// 3,150 accept ranges and the reader pools' at 11,000 addresses for the first of them and
/// 15,000 for the second — two pools under one constant, which is why a size between those two
/// catches half the defect and reads 0.6 rather than 1.0. Each case is taken well clear of its
/// boundary, not because the boundary drifts (it does not: the arena doubles from sixteen, so
/// its capacity crosses exactly when its use does) but because a grammar change that adds one
/// arena entry per element would otherwise turn the control side silently into a second failing
/// side.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class PoolRetentionScalingTests
{
	/// <summary>
	/// The shapes, frozen. What the text says decides where the boundary falls — a bare media
	/// type reaches it at about 5,100 elements and this one at 3,150 — so the format is part of
	/// the test rather than a detail of it, and changing it means measuring the ladder again.
	/// </summary>
	const string AcceptRange = "text/x{0};q=0.5";
	const string AddressItem = "a{0}@example.com";

	/// <summary>
	/// The floor is not zero: a second parse still builds the tree. finance-24 read 0.07 for the
	/// accept lists and 0.20 to 0.21 for the address lists, from 500 elements to 10,000, in
	/// fresh processes and again warm; a cliff reads 1.00. Anything between wants looking at
	/// rather than passing, which is why this sits where it does and not at 0.9.
	/// </summary>
	const double Reused = 0.35;

	[Fact]
	public void An_accept_list_below_the_arena_bound_reuses_the_machinery()
	{
		Held(2_000, AcceptRange, static text => MediaRange.TryParseAccept(text, out _));
	}

	[Fact]
	public void And_one_past_it_does_the_same()
	{
		Held(4_000, AcceptRange, static text => MediaRange.TryParseAccept(text, out _));
	}

	[Fact]
	public void An_address_list_below_the_reader_bounds_reuses_the_pools()
	{
		Held(5_000, AddressItem, static text => EmailAddress.TryParseList(text, out _));
	}

	/// <summary>
	/// Past both of them: one pool stops being kept at about 11,000 addresses and the other at
	/// about 15,000, so a size short of the second would hold only half of this.
	/// </summary>
	[Fact]
	public void And_one_past_both_of_them_does_the_same()
	{
		Held(20_000, AddressItem, static text => EmailAddress.TryParseList(text, out _));
	}

	/// <summary>
	/// A store kept past the bound keeps the room it grew and not what was built in it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The two are separable and were not separated. The branch that parks an oversized store
	/// ends in a <c>return</c>, and the lines that empty the tables sit below it — so the parked
	/// store went to its slot with every value of the last parse still in it, and held them for
	/// eight rentals and a weak reference after that. A reader that parsed one large document
	/// and then went quiet kept that document's whole tree alive, having handed its caller the
	/// only copy anyone asked for and dropped it.
	/// </para>
	/// <para>
	/// Measured rather than reasoned: at 20,000 addresses a value of the finished parse survived
	/// three forced blocking gen-2 collections; at 5,000, below the bound, it did not. That is
	/// the whole of the difference between the two, and it is why the size here is the size the
	/// ratio tests use — the same document, asked a different question about the same store.
	/// </para>
	/// <para>
	/// Allocation is not what this measures, so it is indifferent to what else runs on the
	/// machine: what it asks is whether an object is reachable, and that is an answer and not a
	/// measurement.
	/// </para>
	/// </remarks>
	[Fact]
	public void A_parse_past_the_bound_does_not_keep_what_it_built()
	{
		Assert.False(
			Retains(20_000),
			"a value of the finished parse was still reachable, so the store kept past the bound " +
			"was parked with its tables full and is holding the whole of the last parse.");
	}

	/// <summary>And below it, where the store goes to the ordinary spare slot instead.</summary>
	[Fact]
	public void Nor_does_one_below_the_bound()
	{
		Assert.False(Retains(5_000), "a value of the finished parse was still reachable.");
	}

	/// <summary>
	/// Whether a value of the parse outlives it. The pools are the thread's, so the thread has
	/// to outlive the check — a store on a dead thread is unreachable for a reason that has
	/// nothing to do with the question.
	/// </summary>
	static bool Retains(int elements)
	{
		var text  = string.Join(", ", Enumerable.Range(0, elements).Select(one => string.Format(AddressItem, one)));
		var alive = false;

		OnItsOwnThread(() =>
		{
			// The parse under test is the second: the first publishes the tables and grows the
			// pools, and a store that was never outsized cannot be parked full.
			Assert.True(EmailAddress.TryParseList(text, out _));

			var weak = Parsed(text);

			for (var round = 0; round < 3; round++)
			{
				GC.Collect(2, GCCollectionMode.Forced, blocking: true);
				GC.WaitForPendingFinalizers();
			}

			alive = weak.IsAlive;
		});

		return alive;
	}

	/// <summary>
	/// A frame of its own, left before anything is collected: a local holding the array would
	/// keep it alive on its own and the answer would be about this method rather than the store.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	static WeakReference Parsed(string text)
	{
		Assert.True(EmailAddress.TryParseList(text, out var addresses));

		return new WeakReference(addresses[0]);
	}

	static void Held(int elements, string shape, Func<string, bool> parse)
	{
		var text = string.Join(", ", Enumerable.Range(0, elements).Select(one => string.Format(shape, one)));

		// Warm the process before measuring anything: the tables of expected strings and
		// everything else published once, and the code itself. Those land in whoever parses
		// first, and that must not be the parse being measured.
		OnItsOwnThread(() => Assert.True(parse(text)));

		long first = 0, second = 0;
		int gen2First = 0, gen2Second = 0;

		// The pools are the thread's, so the measurement needs a thread that has never parsed:
		// on a warm one the first parse is cheap too and the ratio says nothing.
		OnItsOwnThread(() =>
		{
			var before = GC.GetAllocatedBytesForCurrentThread();
			var collections = GC.CollectionCount(2);

			Assert.True(parse(text));

			first     = GC.GetAllocatedBytesForCurrentThread() - before;
			gen2First = GC.CollectionCount(2) - collections;

			before      = GC.GetAllocatedBytesForCurrentThread();
			collections = GC.CollectionCount(2);

			Assert.True(parse(text));

			second     = GC.GetAllocatedBytesForCurrentThread() - before;
			gen2Second = GC.CollectionCount(2) - collections;
		});

		var reused = (double)second / first;

		Assert.True(
			reused < Reused,
			$"{elements:N0} elements: the second parse allocated {reused:P0} of the first " +
			$"({second / 1024.0:N0} KB against {first / 1024.0:N0} KB, gen2 {gen2First} then " +
			$"{gen2Second}), so the machinery the first parse grew was not kept for it.");
	}

	/// <summary>A thread of its own, with room for a deep reading of a long document.</summary>
	static void OnItsOwnThread(Action what)
	{
		Exception? failed = null;
		var thread = new Thread(
			() =>
			{
				try
				{
					what();
				}
				catch (Exception error)
				{
					failed = error;
				}
			},
			16 * 1024 * 1024);

		thread.Start();
		thread.Join();

		if (failed is not null)
			throw failed;
	}
}
