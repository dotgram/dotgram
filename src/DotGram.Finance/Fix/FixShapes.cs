// SPIKE (not for merging): the types the exposure shapes of the lists are written with. Each column of the
// benchmark is one build of this package with one of FIX_SHAPE_B, _C, _D or _F defined (the project reads
// the property FixShape); the control, A, defines none and this file is empty.

#if FIX_SHAPE_B
using System.Runtime.CompilerServices;

namespace DotGram.Finance.Fix;

/// <summary>The list a group's public view stands over.</summary>
static class FixStore
{
	internal static List<T>? Of<T>(IReadOnlyList<T>? view)
	{
		return Unsafe.As<List<T>?>(view);
	}
}
#endif

#if FIX_SHAPE_C
using System.Collections;

namespace DotGram.Finance.Fix;

/// <summary>A read-only view of the entries of a group; the default view is empty.</summary>
/// <typeparam name="T">The type of an entry.</typeparam>
public readonly struct FixList<T> : IReadOnlyList<T>
{
	internal readonly List<T>? Raw;

	internal FixList(List<T>? list)
	{
		Raw = list;
	}

	/// <summary>The number of entries.</summary>
	public int Count
	{
		get { return Raw is null ? 0 : Raw.Count; }
	}

	/// <summary>The entry at an index.</summary>
	/// <param name="index">The index, from zero.</param>
	public T this[int index]
	{
		get
		{
			if (Raw is null)
				throw new ArgumentOutOfRangeException(nameof(index));

			return Raw[index];
		}
	}

	/// <summary>The entries, in order.</summary>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(Raw);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	/// <summary>Walks the entries of a view.</summary>
	public struct Enumerator : IEnumerator<T>
	{
		readonly List<T>? _list;
		int _index;

		internal Enumerator(List<T>? list)
		{
			_list  = list;
			_index = -1;
		}

		/// <summary>The entry the walk is at.</summary>
		public readonly T Current
		{
			get { return _list![_index]; }
		}

		readonly object? IEnumerator.Current
		{
			get { return Current; }
		}

		/// <summary>Moves to the next entry.</summary>
		public bool MoveNext()
		{
			return _list is not null && ++_index < _list.Count;
		}

		void IEnumerator.Reset()
		{
			_index = -1;
		}

		/// <summary>Nothing to release.</summary>
		public readonly void Dispose()
		{
		}
	}
}
#endif

#if FIX_SHAPE_F
using System.Collections;
using System.Runtime.CompilerServices;

namespace DotGram.Finance.Fix;

/// <summary>Builds the empty list that <c>[]</c> stands for.</summary>
public static class FixList
{
	/// <summary>Answers the one empty list for no elements, so that <c>list ?? []</c> allocates nothing.</summary>
	/// <param name="items">The elements.</param>
	/// <typeparam name="T">The type of an element.</typeparam>
	public static FixList<T> Create<T>(ReadOnlySpan<T> items)
	{
		if (items.IsEmpty)
			return FixList<T>.Empty;

		var list = new FixList<T>(items.Length);

		foreach (var item in items)
			list.Add(item);

		return list;
	}
}

/// <summary>The entries of a group: an array and a count, grown as <see cref="List{T}"/> grows.</summary>
/// <typeparam name="T">The type of an entry.</typeparam>
[CollectionBuilder(typeof(FixList), nameof(FixList.Create))]
public sealed class FixList<T> : IReadOnlyList<T>
{
	/// <summary>The list with no entries.</summary>
	public static readonly FixList<T> Empty = new();

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

	internal void Add(T item)
	{
		if (_count == _items.Length)
			Grow();

		_items[_count++] = item;
	}

	void Grow()
	{
		var items = new T[_items.Length == 0 ? 4 : _items.Length * 2];

		Array.Copy(_items, items, _count);
		_items = items;
	}

	internal T[] ToArray()
	{
		var items = new T[_count];

		Array.Copy(_items, items, _count);

		return items;
	}

	/// <summary>The number of entries.</summary>
	public int Count
	{
		get { return _count; }
	}

	/// <summary>The entry at an index.</summary>
	/// <param name="index">The index, from zero.</param>
	public T this[int index]
	{
		get
		{
			if ((uint)index >= (uint)_count)
				throw new ArgumentOutOfRangeException(nameof(index));

			return _items[index];
		}
	}

	/// <summary>The entries, in order.</summary>
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

	/// <summary>Walks the entries of a list.</summary>
	public struct Enumerator : IEnumerator<T>
	{
		readonly T[] _items;
		readonly int _count;
		int _index;

		internal Enumerator(FixList<T> list)
		{
			_items = list._items;
			_count = list._count;
			_index = -1;
		}

		/// <summary>The entry the walk is at.</summary>
		public readonly T Current
		{
			get { return _items[_index]; }
		}

		readonly object? IEnumerator.Current
		{
			get { return Current; }
		}

		/// <summary>Moves to the next entry.</summary>
		public bool MoveNext()
		{
			return ++_index < _count;
		}

		void IEnumerator.Reset()
		{
			_index = -1;
		}

		/// <summary>Nothing to release.</summary>
		public readonly void Dispose()
		{
		}
	}
}
#endif

#if FIX_SHAPE_D
using System.Collections.Immutable;
using System.Runtime.InteropServices;

namespace DotGram.Finance.Fix;

/// <summary>Sizes the buffer of a group from its counter.</summary>
static class FixBuffer
{
	internal static int Size(FixField.Typed<long>? counter)
	{
		if (counter is { IsValid: true, Value: > 0 and <= 1 << 16 })
			return (int)counter.Value;

		return 4;
	}
}

/// <summary>The entries of a group while they are read: an array sized from the counter, and a count.</summary>
/// <typeparam name="T">The type of an entry.</typeparam>
sealed class FixBuffer<T>
{
	T[] _items;
	int _count;

	internal FixBuffer(int capacity)
	{
		_items = new T[capacity];
	}

	internal void Add(T item)
	{
		if (_count == _items.Length)
			Array.Resize(ref _items, _items.Length * 2);

		_items[_count++] = item;
	}

	internal int Count
	{
		get { return _count; }
	}

	internal T this[int index]
	{
		get { return _items[index]; }
	}

	// The array itself where the counter was right, which is what a group is; a copy, once, where it was not.
	internal static ImmutableArray<T> View(FixBuffer<T>? buffer)
	{
		if (buffer is null)
			return ImmutableArray<T>.Empty;

		if (buffer._items.Length != buffer._count)
			Array.Resize(ref buffer._items, buffer._count);

		return ImmutableCollectionsMarshal.AsImmutableArray(buffer._items);
	}
}
#endif
