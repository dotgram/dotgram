using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DotGram.Generation;

/// <summary>
/// What the generator keeps of its own work from one compilation to the next: a value by its key,
/// the least recently used out first, held to a budget of characters.
/// </summary>
/// <remarks>
/// <para>
/// A command-line build has no generator driver to keep anything in between two builds: every one
/// runs every stage again, and the compile of a large grammar is seconds of it. The compiler server
/// does keep the generator's assembly loaded, so a static of it lives as long as the server, and a
/// value kept here is there for the next build of the same project — or of the other target
/// framework of this one. That is the whole of what this is for. Nothing on disk: a server that
/// exits takes it with it, and an assembly of the generator built again is a different assembly,
/// with statics of its own.
/// </para>
/// <para>
/// Bounded by what the entries hold, because one value is a parser's text: tens of megabytes for a
/// large grammar and a few kilobytes for a small one. What an entry holds is counted in characters:
/// its large strings exactly — a string that two entries hold, the same parser compiled for two
/// target frameworks, is counted once, which is what it costs — and everything else it holds as
/// an estimate the caller gives. A cap on how many entries there are stands beside that, so that
/// entries which each weigh little cannot pile up without end. An entry heavier than the whole
/// budget is not kept at all.
/// </para>
/// <para>
/// Reading takes no lock. Adding takes one, briefly, to keep the count of what is held and the
/// order things leave in, and to let a new value share a string with one already kept — under the
/// lock, so that two values finished at the same moment cannot both miss the other's copy. The
/// work that produced the value is done before it, outside, so two compilations that miss at once
/// both do the work and the second to finish keeps the first one's entry.
/// </para>
/// <para>
/// Public so that the bound can be held to a test; nothing outside the generator uses it.
/// </para>
/// </remarks>
/// <typeparam name="TKey">Everything the value was computed from, compared as values.</typeparam>
/// <typeparam name="TValue">What was computed.</typeparam>
public sealed class CompileCache<TKey, TValue>
	where TKey : IEquatable<TKey>
{
	readonly ConcurrentDictionary<TKey, Entry> _entries = new();
	readonly Func<TKey, TValue, IEnumerable<string?>> _strings;
	readonly Func<TKey, TValue, long> _weight;
	readonly Func<TValue, IEnumerable<TValue>, TValue>? _share;
	readonly object _adding = new();
	long _clock;

	/// <param name="budget">The characters the kept entries may hold between them.</param>
	/// <param name="capacity">How many entries may be kept, however little they hold.</param>
	/// <param name="strings">
	/// The large strings an entry holds, counted exactly. One held by several entries is counted
	/// once.
	/// </param>
	/// <param name="weight">
	/// Everything else an entry holds, as an estimate in characters: arrays, small strings, the
	/// objects around them. Counted for each entry on its own.
	/// </param>
	/// <param name="share">
	/// The value about to be kept, given the values kept already, made to hold their very strings
	/// where it holds equal ones; or null to keep values as they come.
	/// </param>
	public CompileCache(
		long                                         budget,
		int                                          capacity,
		Func<TKey, TValue, IEnumerable<string?>>     strings,
		Func<TKey, TValue, long>?                    weight = null,
		Func<TValue, IEnumerable<TValue>, TValue>?   share  = null)
	{
		if (budget <= 0)
			throw new ArgumentOutOfRangeException(nameof(budget));

		if (capacity <= 0)
			throw new ArgumentOutOfRangeException(nameof(capacity));

		Budget   = budget;
		Capacity = capacity;
		_strings = strings ?? throw new ArgumentNullException(nameof(strings));
		_weight  = weight ?? NoWeight;
		_share   = share;
	}

	/// <summary>The characters the kept entries may hold between them.</summary>
	public long Budget { get; }

	/// <summary>How many entries may be kept.</summary>
	public int Capacity { get; }

	/// <summary>How many entries are kept.</summary>
	public int Count => _entries.Count;

	/// <summary>The characters the kept entries hold between them, estimate included.</summary>
	public long Size
	{
		get
		{
			lock (_adding)
				return Holdings(_entries, out _);
		}
	}

	/// <summary>The value kept under <paramref name="key"/>, which is now the most recently used.</summary>
	public bool TryGet(TKey key, out TValue value)
	{
		if (_entries.TryGetValue(key, out var entry))
		{
			entry.Touch(Interlocked.Increment(ref _clock));

			value = entry.Value;

			return true;
		}

		value = default!;

		return false;
	}

	/// <summary>
	/// Keeps <paramref name="value"/> under <paramref name="key"/>, letting the least recently used
	/// go until what is kept fits the budget and the capacity.
	/// </summary>
	/// <returns>
	/// Whether it was kept: not where the key is held already, nor where the entry alone is heavier
	/// than the budget.
	/// </returns>
	public bool Add(TKey key, TValue value)
	{
		lock (_adding)
		{
			if (_entries.ContainsKey(key))
				return false;

			var entry = new Entry(_share is null ? value : _share(value, _entries.Values.Select(static one => one.Value)));

			if (Holdings([new KeyValuePair<TKey, Entry>(key, entry)], out _) > Budget)
				return false;

			entry.Touch(Interlocked.Increment(ref _clock));
			_entries[key] = entry;

			// Measured once and sorted once: each entry let go takes away its estimate and the
			// strings no other entry still holds.
			var size  = Holdings(_entries, out var holders);
			var count = _entries.Count;

			if (size <= Budget && count <= Capacity)
				return true;

			var oldest = _entries
				.Where(pair => !pair.Key.Equals(key))
				.OrderBy(static pair => pair.Value.Used)
				.ToList();

			foreach (var pair in oldest)
			{
				if (size <= Budget && count <= Capacity)
					break;

				_entries.TryRemove(pair.Key, out _);
				count--;
				size -= _weight(pair.Key, pair.Value.Value);

				foreach (var text in _strings(pair.Key, pair.Value.Value))
					if (text is not null && --holders[text] == 0)
						size -= text.Length;
			}
		}

		return true;
	}

	static long NoWeight(TKey key, TValue value)
	{
		return 0;
	}

	/// <summary>Lets the value under <paramref name="key"/> go, if one is kept.</summary>
	public bool Remove(TKey key)
	{
		lock (_adding)
			return _entries.TryRemove(key, out _);
	}

	/// <summary>
	/// What the entries hold: each string once however many of them hold it, and each entry's
	/// estimate of the rest.
	/// </summary>
	/// <param name="holders">How many times the entries name each string.</param>
	long Holdings(IEnumerable<KeyValuePair<TKey, Entry>> entries, out Dictionary<string, int> holders)
	{
		var size = 0L;

		holders = new Dictionary<string, int>(SameString.Instance);

		foreach (var pair in entries)
		{
			size += _weight(pair.Key, pair.Value.Value);

			foreach (var text in _strings(pair.Key, pair.Value.Value))
			{
				if (text is null)
					continue;

				holders.TryGetValue(text, out var held);

				if (held == 0)
					size += text.Length;

				holders[text] = held + 1;
			}
		}

		return size;
	}

	sealed class Entry(TValue value)
	{
		public readonly TValue Value = value;
		long _used;

		public long Used => Interlocked.Read(ref _used);

		/// <summary>
		/// Marks the entry used at <paramref name="stamp"/>, unless a later use already has: two
		/// readers that took their stamps in one order may get here in the other.
		/// </summary>
		public void Touch(long stamp)
		{
			var seen = Interlocked.Read(ref _used);

			while (seen < stamp)
			{
				var was = Interlocked.CompareExchange(ref _used, stamp, seen);

				if (was == seen)
					return;

				seen = was;
			}
		}
	}

	/// <summary>Strings told apart by which object they are, which is what memory is counted in.</summary>
	sealed class SameString : IEqualityComparer<string>
	{
		public static readonly SameString Instance = new();

		public bool Equals(string? x, string? y)
		{
			return ReferenceEquals(x, y);
		}

		public int GetHashCode(string obj)
		{
			return RuntimeHelpers.GetHashCode(obj);
		}
	}
}
