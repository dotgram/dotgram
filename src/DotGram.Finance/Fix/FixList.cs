using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DotGram.Finance.Fix;

// A spike, not landed: the two read-only list shapes measured against List<T>. Only one is compiled,
// chosen by FixShape (DotGram.Finance.csproj).

#if FIXSHAPE_C

/// <summary>
/// The entries a message read, in order: a view over the list the reader built, empty where it built none.
/// </summary>
/// <typeparam name="T">The type of an entry.</typeparam>
public readonly struct FixList<T> : IReadOnlyList<T>
{
	readonly List<T>? _items;

	internal FixList(List<T>? items)
	{
		_items = items;
	}

	internal List<T>? Items => _items;

	/// <summary>
	/// The number of entries.
	/// </summary>
	public int Count => _items is null ? 0 : _items.Count;

	/// <summary>
	/// The entry at an index.
	/// </summary>
	/// <param name="index">From zero.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not below <see cref="Count"/>.</exception>
	public T this[int index]
	{
		get
		{
			if (_items is null)
				throw new ArgumentOutOfRangeException(nameof(index));

			return _items[index];
		}
	}

	/// <summary>
	/// Walks the entries without allocating.
	/// </summary>
	/// <returns>The enumerator.</returns>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(_items);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>
	/// Walks the entries of a <see cref="FixList{T}"/>.
	/// </summary>
	public struct Enumerator : IEnumerator<T>
	{
		readonly List<T>? _items;
		int               _index;
		T                 _current;

		internal Enumerator(List<T>? items)
		{
			_items   = items;
			_index   = 0;
			_current = default!;
		}

		/// <summary>
		/// The entry the enumerator is at.
		/// </summary>
		public readonly T Current => _current;

		readonly object? IEnumerator.Current => _current;

		/// <summary>
		/// Moves to the next entry.
		/// </summary>
		/// <returns>Whether there is one.</returns>
		public bool MoveNext()
		{
			var items = _items;

			if (items is not null && (uint)_index < (uint)items.Count)
			{
				_current = items[_index];
				_index++;

				return true;
			}

			return false;
		}

		void IEnumerator.Reset()
		{
			_index   = 0;
			_current = default!;
		}

		/// <summary>
		/// Nothing to release.
		/// </summary>
		public readonly void Dispose()
		{
		}
	}
}

#elif FIXSHAPE_F

/// <summary>
/// The entries a message read, in order, held by this list itself; read-only outside the package.
/// </summary>
/// <typeparam name="T">The type of an entry.</typeparam>
[CollectionBuilder(typeof(FixList), nameof(FixList.Create))]
public sealed class FixList<T> : IReadOnlyList<T>
{
	internal static readonly FixList<T> Empty = new();

	T[] _items;
	int _count;

	internal FixList()
	{
		_items = [];
	}

	internal FixList(int capacity)
	{
		_items = capacity == 0 ? [] : new T[capacity];
	}

	internal FixList(T[] items)
	{
		_items = items;
		_count = items.Length;
	}

	internal T[] Buffer => _items;

	/// <summary>
	/// The number of entries.
	/// </summary>
	public int Count => _count;

	/// <summary>
	/// The entry at an index.
	/// </summary>
	/// <param name="index">From zero.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is not below <see cref="Count"/>.</exception>
	public T this[int index]
	{
		get
		{
			if ((uint)index >= (uint)_count)
				throw new ArgumentOutOfRangeException(nameof(index));

			return _items[index];
		}
	}

	// Grows as List<T> does: four, then twice the length.
	internal void Add(T item)
	{
		var items = _items;
		var count = _count;

		if ((uint)count < (uint)items.Length)
		{
			items[count] = item;
			_count       = count + 1;

			return;
		}

		AddWithResize(item);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	void AddWithResize(T item)
	{
		var count = _count;
		var grown = new T[_items.Length == 0 ? 4 : _items.Length * 2];

		Array.Copy(_items, grown, count);

		_items        = grown;
		_items[count] = item;
		_count        = count + 1;
	}

	/// <summary>
	/// Walks the entries without allocating.
	/// </summary>
	/// <returns>The enumerator.</returns>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>
	/// Walks the entries of a <see cref="FixList{T}"/>.
	/// </summary>
	public struct Enumerator : IEnumerator<T>
	{
		readonly FixList<T> _list;
		int                 _index;
		T                   _current;

		internal Enumerator(FixList<T> list)
		{
			_list    = list;
			_index   = 0;
			_current = default!;
		}

		/// <summary>
		/// The entry the enumerator is at.
		/// </summary>
		public readonly T Current => _current;

		readonly object? IEnumerator.Current => _current;

		/// <summary>
		/// Moves to the next entry.
		/// </summary>
		/// <returns>Whether there is one.</returns>
		public bool MoveNext()
		{
			var list = _list;

			if ((uint)_index < (uint)list._count)
			{
				_current = list._items[_index];
				_index++;

				return true;
			}

			return false;
		}

		void IEnumerator.Reset()
		{
			_index   = 0;
			_current = default!;
		}

		/// <summary>
		/// Nothing to release.
		/// </summary>
		public readonly void Dispose()
		{
		}
	}
}

/// <summary>
/// What a collection expression of a <see cref="FixList{T}"/> is built by: <c>?? []</c> where a group is absent.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class FixList
{
	/// <summary>
	/// A list of the items; the one empty list where there are none.
	/// </summary>
	/// <typeparam name="T">The type of an entry.</typeparam>
	/// <param name="items">The items.</param>
	/// <returns>The list.</returns>
	public static FixList<T> Create<T>(ReadOnlySpan<T> items)
	{
		return items.Length == 0 ? FixList<T>.Empty : new FixList<T>(items.ToArray());
	}
}

#endif

#if FIXSHAPE_Q

/// <summary>
/// The usual query operators over a <see cref="FixList{T}"/> as plain loops: they take the list itself, so a call
/// is bound here and not to the <see cref="System.Linq.Enumerable"/> overload that boxes the enumerator.
/// </summary>
public static class FixListExtensions
{
	/// <summary>
	/// The number of entries.
	/// </summary>
	/// <typeparam name="T">The type of an entry.</typeparam>
	/// <param name="list">The list.</param>
	/// <returns>The number of entries.</returns>
	public static int Count<T>(this FixList<T> list)
	{
		return list.Count;
	}

	/// <summary>
	/// The number of entries that satisfy a condition.
	/// </summary>
	/// <typeparam name="T">The type of an entry.</typeparam>
	/// <param name="list">The list.</param>
	/// <param name="predicate">The condition.</param>
	/// <returns>The number of entries.</returns>
	public static int Count<T>(this FixList<T> list, Func<T, bool> predicate)
	{
		var items = list.Buffer;
		var count = list.Count;
		var found = 0;

		for (var i = 0; i < count; i++)
			if (predicate(items[i]))
				found++;

		return found;
	}

	/// <summary>
	/// Whether the list has an entry.
	/// </summary>
	/// <typeparam name="T">The type of an entry.</typeparam>
	/// <param name="list">The list.</param>
	/// <returns>Whether it is not empty.</returns>
	public static bool Any<T>(this FixList<T> list)
	{
		return list.Count != 0;
	}

	/// <summary>
	/// Whether an entry satisfies a condition.
	/// </summary>
	/// <typeparam name="T">The type of an entry.</typeparam>
	/// <param name="list">The list.</param>
	/// <param name="predicate">The condition.</param>
	/// <returns>Whether there is one.</returns>
	public static bool Any<T>(this FixList<T> list, Func<T, bool> predicate)
	{
		var items = list.Buffer;
		var count = list.Count;

		for (var i = 0; i < count; i++)
			if (predicate(items[i]))
				return true;

		return false;
	}

	/// <summary>
	/// The first entry.
	/// </summary>
	/// <typeparam name="T">The type of an entry.</typeparam>
	/// <param name="list">The list.</param>
	/// <returns>The entry.</returns>
	/// <exception cref="InvalidOperationException">The list is empty.</exception>
	public static T First<T>(this FixList<T> list)
	{
		if (list.Count == 0)
			throw new InvalidOperationException("The list has no entries.");

		return list.Buffer[0];
	}

	/// <summary>
	/// The first entry that satisfies a condition.
	/// </summary>
	/// <typeparam name="T">The type of an entry.</typeparam>
	/// <param name="list">The list.</param>
	/// <param name="predicate">The condition.</param>
	/// <returns>The entry.</returns>
	/// <exception cref="InvalidOperationException">No entry satisfies the condition.</exception>
	public static T First<T>(this FixList<T> list, Func<T, bool> predicate)
	{
		var items = list.Buffer;
		var count = list.Count;

		for (var i = 0; i < count; i++)
			if (predicate(items[i]))
				return items[i];

		throw new InvalidOperationException("No entry satisfies the condition.");
	}

	/// <summary>
	/// The first entry, or the default of the type where there is none.
	/// </summary>
	/// <typeparam name="T">The type of an entry.</typeparam>
	/// <param name="list">The list.</param>
	/// <returns>The entry.</returns>
	public static T? FirstOrDefault<T>(this FixList<T> list)
	{
		return list.Count == 0 ? default : list.Buffer[0];
	}

	/// <summary>
	/// The first entry that satisfies a condition, or the default of the type where there is none.
	/// </summary>
	/// <typeparam name="T">The type of an entry.</typeparam>
	/// <param name="list">The list.</param>
	/// <param name="predicate">The condition.</param>
	/// <returns>The entry.</returns>
	public static T? FirstOrDefault<T>(this FixList<T> list, Func<T, bool> predicate)
	{
		var items = list.Buffer;
		var count = list.Count;

		for (var i = 0; i < count; i++)
			if (predicate(items[i]))
				return items[i];

		return default;
	}

	/// <summary>
	/// The entries that satisfy a condition, found now (not when the result is walked).
	/// </summary>
	/// <typeparam name="T">The type of an entry.</typeparam>
	/// <param name="list">The list.</param>
	/// <param name="predicate">The condition.</param>
	/// <returns>A new list; the one empty list where none does.</returns>
	public static FixList<T> Where<T>(this FixList<T> list, Func<T, bool> predicate)
	{
		var items = list.Buffer;
		var count = list.Count;
		var found = new FixList<T>();

		for (var i = 0; i < count; i++)
			if (predicate(items[i]))
				found.Add(items[i]);

		return found.Count == 0 ? FixList<T>.Empty : found;
	}

	/// <summary>
	/// The entries in a new array.
	/// </summary>
	/// <typeparam name="T">The type of an entry.</typeparam>
	/// <param name="list">The list.</param>
	/// <returns>The array.</returns>
	public static T[] ToArray<T>(this FixList<T> list)
	{
		var count = list.Count;

		if (count == 0)
			return [];

		var array = new T[count];

		Array.Copy(list.Buffer, array, count);

		return array;
	}
}

#endif
