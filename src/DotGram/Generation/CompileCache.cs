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
/// Bounded by what the values hold rather than by how many there are, because one value is a
/// parser's text: tens of megabytes for a large grammar and a few kilobytes for a small one. A
/// string that two values hold — the same parser compiled for two target frameworks — is counted
/// once, which is what it costs. A value larger than the whole budget is not kept at all.
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
	readonly Func<TValue, IEnumerable<TValue>, TValue>? _share;
	readonly object _adding = new();
	long _clock;

	/// <param name="budget">The characters the kept keys and values may hold between them.</param>
	/// <param name="strings">
	/// The strings an entry holds that are worth counting: the large ones, which is what the budget
	/// is about. One held by several entries is counted once.
	/// </param>
	/// <param name="share">
	/// The value about to be kept, given the values kept already, made to hold their very strings
	/// where it holds equal ones; or null to keep values as they come.
	/// </param>
	public CompileCache(long budget, Func<TKey, TValue, IEnumerable<string?>> strings, Func<TValue, IEnumerable<TValue>, TValue>? share = null)
	{
		if (budget <= 0)
			throw new ArgumentOutOfRangeException(nameof(budget));

		Budget   = budget;
		_strings = strings ?? throw new ArgumentNullException(nameof(strings));
		_share   = share;
	}

	/// <summary>The characters the kept entries may hold between them.</summary>
	public long Budget { get; }

	/// <summary>How many entries are kept.</summary>
	public int Count => _entries.Count;

	/// <summary>The characters the kept entries hold between them.</summary>
	public long Size
	{
		get
		{
			lock (_adding)
				return Measure(_entries);
		}
	}

	/// <summary>The value kept under <paramref name="key"/>, which is now the most recently used.</summary>
	public bool TryGet(TKey key, out TValue value)
	{
		if (_entries.TryGetValue(key, out var entry))
		{
			Interlocked.Exchange(ref entry.Used, Interlocked.Increment(ref _clock));

			value = entry.Value;

			return true;
		}

		value = default!;

		return false;
	}

	/// <summary>
	/// Keeps <paramref name="value"/> under <paramref name="key"/>, letting the least recently used
	/// go until what is kept fits the budget.
	/// </summary>
	/// <returns>
	/// Whether it was kept: not where the key is held already, nor where the value alone is larger
	/// than the budget.
	/// </returns>
	public bool Add(TKey key, TValue value)
	{
		lock (_adding)
		{
			if (_entries.ContainsKey(key))
				return false;

			var entry = new Entry(_share is null ? value : _share(value, _entries.Values.Select(static one => one.Value)));

			if (Measure([new KeyValuePair<TKey, Entry>(key, entry)]) > Budget)
				return false;

			entry.Used    = Interlocked.Increment(ref _clock);
			_entries[key] = entry;

			while (Measure(_entries) > Budget)
			{
				var oldest = _entries
					.Where(pair => !pair.Key.Equals(key))
					.OrderBy(static pair => Interlocked.Read(ref pair.Value.Used))
					.First();

				_entries.TryRemove(oldest.Key, out _);
			}
		}

		return true;
	}

	/// <summary>Lets the value under <paramref name="key"/> go, if one is kept.</summary>
	public bool Remove(TKey key)
	{
		lock (_adding)
			return _entries.TryRemove(key, out _);
	}

	/// <summary>Every string the entries hold, each counted once however many of them hold it.</summary>
	long Measure(IEnumerable<KeyValuePair<TKey, Entry>> entries)
	{
		var counted = new HashSet<string>(SameString.Instance);
		var size    = 0L;

		foreach (var pair in entries)
			foreach (var text in _strings(pair.Key, pair.Value.Value))
				if (text is not null && counted.Add(text))
					size += text.Length;

		return size;
	}

	sealed class Entry(TValue value)
	{
		public readonly TValue Value = value;
		public long            Used;
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
