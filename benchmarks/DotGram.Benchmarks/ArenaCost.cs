using System;

using BenchmarkDotNet.Attributes;

namespace DotGram.Benchmarks;

/// <summary>
/// What the arena costs, at the rate one parse actually uses it.
/// </summary>
/// <remarks>
/// <para>
/// Counted rather than guessed: parsing <c>http://example.com</c> makes 54 appends, 172
/// reads through the indexer, 24 writes and 36 removals. One iteration below is that
/// profile once, so what it reports is the arena's share of a parse of that URL — against
/// the 391 ns the whole parse takes.
/// </para>
/// <para>
/// The interesting one is the read. An entry is nine integers, and the indexer hands it
/// back by value, so every look at a resume point copies thirty-six bytes out of the array
/// to read one or two of them. The engine that came before this one kept that state in
/// locals, which is to say in registers. Both readings are here.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ArenaCost
{
	const int Adds = 54, Reads = 172, Writes = 24, Removes = 36;

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
		Entry[] _items = new Entry[256];

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
		Entry[] _items = new Entry[256];

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

	static Entry Made(int i) => new(1, i, i, i, -1, -1, -1, i, -1);

	[Benchmark(Baseline = true)]
	public int Entries_handed_back_by_value()
	{
		var arena = _byValue;

		arena.Clear();

		for (var i = 0; i < Adds; i++)
			arena.Add(Made(i));

		var total = 0;

		for (var i = 0; i < Reads; i++)
		{
			// What the failure dispatcher does: take the entry, look at two of its nine
			// fields, and never touch the rest.
			var entry = arena[i % Adds];

			total += entry.State + entry.Position;
		}

		for (var i = 0; i < Writes; i++)
			arena[i] = Made(i);

		for (var i = 0; i < Removes; i++)
			arena.RemoveAt(arena.Count - 1);

		return total;
	}

	[Benchmark]
	public int Entries_read_where_they_lie()
	{
		var arena = _byReference;

		arena.Clear();

		for (var i = 0; i < Adds; i++)
			arena.Add(Made(i));

		var total = 0;

		for (var i = 0; i < Reads; i++)
		{
			ref readonly var entry = ref arena[i % Adds];

			total += entry.State + entry.Position;
		}

		for (var i = 0; i < Writes; i++)
			arena.Write(i, Made(i));

		for (var i = 0; i < Removes; i++)
			arena.RemoveAt(arena.Count - 1);

		return total;
	}
}
