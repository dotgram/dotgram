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
public sealed class FixList<T> : IReadOnlyList<T>, IList<T>
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

	// The writing half of IList<T> is explicit and refuses, as ReadOnlyCollection<T> does: the interface is here so that
	// the operators of System.Linq that look for ICollection<T> and IList<T> take their fast paths.
	bool ICollection<T>.IsReadOnly
	{
		get
		{
			return true;
		}
	}

	T IList<T>.this[int index]
	{
		get
		{
			return this[index];
		}
		set
		{
			throw new NotSupportedException("The list is read-only.");
		}
	}

	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException("The list is read-only.");
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException("The list is read-only.");
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException("The list is read-only.");
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException("The list is read-only.");
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException("The list is read-only.");
	}

	bool ICollection<T>.Contains(T item)
	{
		return Array.IndexOf(_items, item, 0, _count) >= 0;
	}

	int IList<T>.IndexOf(T item)
	{
		return Array.IndexOf(_items, item, 0, _count);
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		Array.Copy(_items, 0, array, arrayIndex, _count);
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

