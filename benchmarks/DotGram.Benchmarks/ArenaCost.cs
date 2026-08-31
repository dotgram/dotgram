using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;

namespace DotGram.Benchmarks;

/// <summary>
/// What the arena costs, at the rate one parse actually uses it.
/// </summary>
/// <remarks>
/// <para>
/// Counted rather than guessed, and at two scales, because the answer is not the same at
/// both. A parse of <c>http://example.com</c> makes 54 appends, 172 reads through the
/// indexer, 24 writes and 36 removals, over an arena small enough to sit in L1. A parse of
/// <c>Url.gram</c> by the self-hosted grammar of the notation makes 3,152 appends, 9,972
/// reads and 299 removals, over 2,857 entries — a hundred kilobytes, past L1 and most of
/// the way through L2. One iteration below is one of those profiles once.
/// </para>
/// <para>
/// The interesting one is the read. An entry is nine integers, and the indexer hands it
/// back by value, so every look at a resume point copies thirty-six bytes out of the array
/// to read one or two of them. The engine that came before this one kept that state in
/// locals, which is to say in registers. Both readings are here.
/// </para>
/// <para>
/// <b>Both scales answer the same, and that is the result.</b> Reading in place is within
/// two per cent of reading by value at either profile. The second scale was added because
/// the first was suspected of being too small to show the copy — 9,972 indexer reads
/// against 172, over an array of a hundred kilobytes rather than nine — and it shows the
/// same answer. Thirty-six bytes copied out of an array that a parse has just written is
/// a copy inside the cache, and the engine's own comment about the copy being interesting
/// was a guess this benchmark now refuses.
/// </para>
/// <para>
/// The last two rows ask the other question the same way. The materializer does not sweep
/// the arena; it follows <c>capturedAt = linkNexts[capturedAt]</c> from a head, so every
/// hop is a load that depends on the one before it and nothing can be prefetched. Read
/// against each other — they do the same number of reads and neither writes or removes,
/// so they are comparable to one another rather than to the two rows above — the chain is
/// <em>not</em> the slower of the two at either scale. At a hundred kilobytes the whole
/// arena is inside L2 and a dependent load costs nothing to speak of.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ArenaCost
{
	/// <summary>One parse's worth of arena traffic, counted from a real parse.</summary>
	/// <param name="Name">What the profile is of, for the results table.</param>
	public sealed record Profile(string Name, int Adds, int Reads, int Writes, int Removes)
	{
		public override string ToString() => Name;
	}

	/// <remarks>
	/// The URL's numbers are counted from the parse the other benchmarks time. The
	/// self-hosted numbers are `ParserArena.Add` and `get_Item` call counts out of a
	/// line-by-line profile of 600 parses of <c>Url.gram</c> through <c>GramGrammar</c>,
	/// divided by 600.
	/// </remarks>
	public static IEnumerable<Profile> Profiles =>
	[
		new("a URL",       54,   172,  24,  36),
		new("a grammar", 3152, 9972, 610, 299),
	];

	[ParamsSource(nameof(Profiles))]
	public Profile Traffic { get; set; } = new("a URL", 54, 172, 24, 36);

	/// <summary>The emitted <c>ParserEntry</c>, field for field.</summary>
	readonly struct Entry
	{
		public Entry(
			int kind, int state, int position, int callIndex, int atomicIndex,
			int repeatIndex, int lookaheadIndex, int value, int ruleIndex)
		{
			Kind = kind;
			State = state;
			Position = position;
			CallIndex = callIndex;
			AtomicIndex = atomicIndex;
			RepeatIndex = repeatIndex;
			LookaheadIndex = lookaheadIndex;
			Value = value;
			RuleIndex = ruleIndex;
		}

		public int Kind { get; }
		public int State { get; }
		public int Position { get; }
		public int CallIndex { get; }
		public int AtomicIndex { get; }
		public int RepeatIndex { get; }
		public int LookaheadIndex { get; }
		public int Value { get; }
		public int RuleIndex { get; }
	}

	/// <summary>The emitted arena: a class, with an indexer that hands entries back by value.</summary>
	sealed class ByValue
	{
		Entry[] _items = new Entry[4096];

		public int Count { get; private set; }

		public Entry this[int index]
		{
			get => _items[index];
			set => _items[index] = value;
		}

		public void Add(Entry entry)
		{
			if (Count == _items.Length)
				Array.Resize(ref _items, Count * 2);

			_items[Count++] = entry;
		}

		public void RemoveAt(int index)
		{
			var after = Count - index - 1;

			if (after > 0)
				Array.Copy(_items, index + 1, _items, index, after);

			Count--;
		}

		public void Clear() => Count = 0;
	}

	/// <summary>The same, reading in place instead of copying out.</summary>
	sealed class ByReference
	{
		Entry[] _items = new Entry[4096];

		public int Count { get; private set; }

		public ref readonly Entry this[int index] => ref _items[index];

		public void Write(int index, in Entry entry) => _items[index] = entry;

		public void Add(in Entry entry)
		{
			if (Count == _items.Length)
				Array.Resize(ref _items, Count * 2);

			_items[Count++] = entry;
		}

		public void RemoveAt(int index)
		{
			var after = Count - index - 1;

			if (after > 0)
				Array.Copy(_items, index + 1, _items, index, after);

			Count--;
		}

		public void Clear() => Count = 0;
	}

	readonly ByValue     _byValue     = new();
	readonly ByReference _byReference = new();

	/// <summary>
	/// The order the materializer reads entries in: a chain, not a sweep.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The build pass finds a completed call's captures by following
	/// <c>capturedAt = linkNexts[capturedAt]</c> from a head — a list threaded through an
	/// array in the order entries were written, which for a nest of calls is not the order
	/// they lie in. Every hop is a load that depends on the one before it, so nothing can
	/// be prefetched and each miss is paid in full.
	/// </para>
	/// <para>
	/// The two rows below read the same entries the same number of times and differ only in
	/// the order: one sweeps the array, the other follows the chain the link pass built.
	/// The difference is what the shape of the traversal costs, with everything else —
	/// the struct copy, the field reads, the loop — held equal.
	/// </para>
	/// </remarks>
	int[] _chain = [];

	[GlobalSetup]
	public void ThreadTheChain()
	{
		var (_, adds, _, _, _) = Traffic;

		_chain = new int[adds];

		// The link pass threads each entry onto the head of its owner's list, so a chain
		// runs backwards through the array in strides — the shape a nest of calls leaves.
		// Reproduced here as a stride, which is the same dependent-load pattern without
		// needing a parse to generate it.
		for (var i = 0; i < adds; i++)
			_chain[i] = (int)(((long)i * 1103515245 + 12345) % adds);
	}

	[Benchmark]
	public int Entries_swept_in_order()
	{
		var arena = _byValue;
		var (_, adds, reads, _, _) = Traffic;

		arena.Clear();

		for (var i = 0; i < adds; i++)
			arena.Add(Made(i));

		var total = 0;

		for (var i = 0; i < reads; i++)
		{
			var entry = arena[i % adds];

			total += entry.State + entry.Position;
		}

		return total;
	}

	[Benchmark]
	public int Entries_read_along_a_chain()
	{
		var arena = _byValue;
		var (_, adds, reads, _, _) = Traffic;
		var chain = _chain;

		arena.Clear();

		for (var i = 0; i < adds; i++)
			arena.Add(Made(i));

		var total = 0;
		var at    = 0;

		for (var i = 0; i < reads; i++)
		{
			// The dependent hop: where to look next comes out of what was just read.
			at = chain[at];

			var entry = arena[at];

			total += entry.State + entry.Position;
		}

		return total;
	}

	static Entry Made(int i) => new(1, i, i, i, -1, -1, -1, i, -1);

	[Benchmark(Baseline = true)]
	public int Entries_handed_back_by_value()
	{
		var arena = _byValue;
		var (_, adds, reads, writes, removes) = Traffic;

		arena.Clear();

		for (var i = 0; i < adds; i++)
			arena.Add(Made(i));

		var total = 0;

		for (var i = 0; i < reads; i++)
		{
			// What the failure dispatcher does: take the entry, look at two of its nine
			// fields, and never touch the rest.
			var entry = arena[i % adds];

			total += entry.State + entry.Position;
		}

		for (var i = 0; i < writes; i++)
			arena[i] = Made(i);

		for (var i = 0; i < removes; i++)
			arena.RemoveAt(arena.Count - 1);

		return total;
	}

	[Benchmark]
	public int Entries_read_where_they_lie()
	{
		var arena = _byReference;
		var (_, adds, reads, writes, removes) = Traffic;

		arena.Clear();

		for (var i = 0; i < adds; i++)
			arena.Add(Made(i));

		var total = 0;

		for (var i = 0; i < reads; i++)
		{
			ref readonly var entry = ref arena[i % adds];

			total += entry.State + entry.Position;
		}

		for (var i = 0; i < writes; i++)
			arena.Write(i, Made(i));

		for (var i = 0; i < removes; i++)
			arena.RemoveAt(arena.Count - 1);

		return total;
	}
}
