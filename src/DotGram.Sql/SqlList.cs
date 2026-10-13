using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace DotGram.Sql;

/// <summary>
/// The list a node holds — its columns, its arguments, its clauses — compared by what is in it.
/// </summary>
/// <remarks>
/// <para>
/// A record compares its fields, and an array compares as a reference: two readings of the same
/// text built two arrays, and the trees were unequal for no reason a reader could see. This is one
/// array, read only, that compares element by element, so a record holding it compares by content
/// with nothing written per record, and a record added later does too.
/// </para>
/// <para>
/// It is the size of the array reference it replaces. <c>default</c> is the empty list: it equals
/// <c>[]</c> and hashes the same. Where a member may be left out altogether it is a
/// <c>SqlList&lt;T&gt;?</c>, and there null and empty stay different answers.
/// </para>
/// <para>
/// Made from a collection expression — <c>[a, b]</c>, <c>[.. items]</c> — or from
/// <see cref="SqlList.From{T}(IEnumerable{T})"/>; both copy, so whoever made a list keeps no
/// handle into the tree. There is no conversion from an array: an array passed where a list is
/// wanted is a compile error, not a silent copy.
/// </para>
/// </remarks>
[CollectionBuilder(typeof(SqlList), nameof(SqlList.Create))]
public readonly struct SqlList<T> : IReadOnlyList<T>, IEquatable<SqlList<T>>
{
	// Null is the empty list; a T[] is the elements; anything else is the one element of a list of
	// one, held without an array (only where T is a reference type that no T[] can be).
	readonly object? _items;

	static readonly bool Inline = !typeof(T).IsValueType && !typeof(T).IsAssignableFrom(typeof(T[]));

	/// <summary>Takes the array as it is; the caller hands it over and keeps no other reference.</summary>
	internal SqlList(T[]? items)
	{
		_items = Inline && items is { Length: 1 } ? items[0] : items;
	}

	SqlList(object? one, bool _)
	{
		_items = one;
	}

	internal static SqlList<T> One(T item)
	{
		return Inline && item is not null ? new SqlList<T>(item, false) : new SqlList<T>([item]);
	}

	T[] Items => _items is null ? Array.Empty<T>() : _items as T[] ?? [(T)_items];

	/// <summary>How many elements the list holds.</summary>
	public int Count => _items is null ? 0 : _items is T[] items ? items.Length : 1;

	/// <summary>How many elements the list holds, under the name an array gives it.</summary>
	public int Length => Count;

	/// <summary>Whether the list holds nothing.</summary>
	public bool IsEmpty => _items is null || _items is T[] { Length: 0 };

	/// <summary>The element at <paramref name="index"/>.</summary>
	public T this[int index]
	{
		get
		{
			if (_items is T[] items)
				return items[index];

			if (_items is null || index != 0)
				throw new IndexOutOfRangeException();

			return (T)_items;
		}
	}

	/// <summary>The elements as a span, without a copy.</summary>
#if NET
	[System.Diagnostics.CodeAnalysis.UnscopedRef]
#endif
	public ReadOnlySpan<T> AsSpan()
	{
		if (_items is null)
			return default;

		if (_items is T[] items)
			return items;

#if NET
		return System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<object, T>(ref Unsafe.AsRef(in _items)), 1);
#else
		return new[] { (T)_items };
#endif
	}

	/// <summary>A part of the list, as a list of its own; what a list pattern's <c>..</c> reads.</summary>
	public SqlList<T> Slice(int start, int length)
	{
		if ((uint)start > (uint)Length || (uint)length > (uint)(Length - start))
			throw new ArgumentOutOfRangeException(start < 0 || start > Length ? nameof(start) : nameof(length));

		return length == 0 ? default : new SqlList<T>(AsSpan().Slice(start, length).ToArray());
	}

	/// <summary>Whether any element matches <paramref name="match"/>, as <see cref="Array.Exists{T}(T[], Predicate{T})"/> asks of an array.</summary>
	public bool Exists(Predicate<T> match)
	{
		return Array.Exists(Items, match);
	}

	/// <summary>Whether every element matches <paramref name="match"/>, as <see cref="Array.TrueForAll{T}(T[], Predicate{T})"/> asks of an array.</summary>
	public bool TrueForAll(Predicate<T> match)
	{
		return Array.TrueForAll(Items, match);
	}

	/// <summary>The elements copied into a new array.</summary>
	public T[] ToArray()
	{
		return AsSpan().ToArray();
	}

	/// <summary>Whether two lists hold equal elements in the same order.</summary>
	public static bool operator ==(SqlList<T> left, SqlList<T> right)
	{
		return left.Equals(right);
	}

	/// <summary>Whether two lists differ in length or in an element.</summary>
	public static bool operator !=(SqlList<T> left, SqlList<T> right)
	{
		return !left.Equals(right);
	}

	/// <summary>Whether <paramref name="other"/> holds equal elements in the same order.</summary>
	/// <remarks>
	/// The lengths first, then each pair through <see cref="EqualityComparer{T}.Default"/>, which for
	/// a node is the record's own equality.
	/// </remarks>
	public bool Equals(SqlList<T> other)
	{
		if (ReferenceEquals(_items, other._items))
			return true;

		var mine   = AsSpan();
		var theirs = other.AsSpan();

		if (mine.Length != theirs.Length)
			return false;

		var comparer = EqualityComparer<T>.Default;

		for (var at = 0; at < mine.Length; at++)
		{
			if (!comparer.Equals(mine[at], theirs[at]))
				return false;
		}

		return true;
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		return obj is SqlList<T> other && Equals(other);
	}

	/// <summary>A hash of the length and of every element in order.</summary>
	/// <remarks>
	/// Every element counts, and its place does: <c>[a, b]</c> and <c>[b, a]</c> hash apart, and so do
	/// lists differing only in an element in the middle, which is where machine-written SQL varies.
	/// The same arithmetic on every framework; the values are this process's, as every hash is.
	/// </remarks>
	public override int GetHashCode()
	{
		var items    = AsSpan();
		var comparer = EqualityComparer<T>.Default;
		var hash     = SqlHash.Start(items.Length);

		foreach (var item in items)
			hash = SqlHash.Add(hash, item is null ? 0 : comparer.GetHashCode(item));

		return SqlHash.Finish(hash);
	}

	/// <summary>The elements between brackets, the way a collection expression writes them.</summary>
	public override string ToString()
	{
		var text = new StringBuilder("[");

		foreach (var item in Items)
		{
			if (text.Length > 1)
				text.Append(", ");

			text.Append(item);
		}

		return text.Append(']').ToString();
	}

	/// <summary>Walks the elements in order, without allocating.</summary>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return ((IEnumerable<T>)Items).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Items.GetEnumerator();
	}

	/// <summary>What <c>foreach</c> over a <see cref="SqlList{T}"/> walks with.</summary>
	public struct Enumerator
	{
		readonly SqlList<T> _list;
		readonly int        _count;
		int                 _at;

		internal Enumerator(SqlList<T> list)
		{
			_list  = list;
			_count = list.Count;
			_at    = -1;
		}

		/// <summary>The element the walk stands on.</summary>
		public readonly T Current => _list[_at];

		/// <summary>Steps to the next element; false past the last.</summary>
		public bool MoveNext()
		{
			return ++_at < _count;
		}
	}
}

/// <summary>
/// Makes <see cref="SqlList{T}"/>s.
/// </summary>
public static class SqlList
{
	/// <summary>
	/// A list of a copy of <paramref name="items"/>: what a collection expression such as
	/// <c>[a, b]</c> calls.
	/// </summary>
	public static SqlList<T> Create<T>(ReadOnlySpan<T> items)
	{
		return items.IsEmpty ? default : items.Length == 1 ? SqlList<T>.One(items[0]) : new SqlList<T>(items.ToArray());
	}

	/// <summary>A list of a copy of <paramref name="items"/>, in their order.</summary>
	public static SqlList<T> From<T>(IEnumerable<T> items)
	{
		if (items is null)
			throw new ArgumentNullException(nameof(items));

		return items switch
		{
			SqlList<T> list => list,
			T[] array       => Create<T>(array),
			_               => Own(items.ToArray()),
		};
	}

	/// <summary>A list of a copy of <paramref name="items"/>, in their order.</summary>
	public static SqlList<T> ToSqlList<T>(this IEnumerable<T> items)
	{
		return From(items);
	}

	/// <summary>
	/// A list over <paramref name="items"/> itself, not a copy: for an array nobody else holds,
	/// which is every array a reading gathers. Null is the empty list.
	/// </summary>
	internal static SqlList<T> Own<T>(T[]? items)
	{
		return new SqlList<T>(items);
	}

	/// <summary>
	/// <see cref="Own{T}(T[])"/> for a member that may be left out: null stays null.
	/// </summary>
	internal static SqlList<T>? OwnOrNull<T>(T[]? items)
	{
		return items is null ? null : new SqlList<T>(items);
	}
}

/// <summary>
/// The hash a list combines its elements with: ordered, the length in it, the same on every
/// framework.
/// </summary>
/// <remarks>
/// The rounds and the final mix are xxHash32's, which is what <c>System.HashCode</c> does on the
/// frameworks that have it, without its per-process seed. Not an exclusive or: that would hash
/// <c>[a, b]</c> as <c>[b, a]</c> and let two equal elements cancel.
/// </remarks>
static class SqlHash
{
	const uint Prime2 = 2246822519U;
	const uint Prime3 = 3266489917U;
	const uint Prime4 = 668265263U;
	const uint Prime5 = 374761393U;

	public static uint Start(int length)
	{
		return Prime5 + (uint)length;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint Add(uint hash, int value)
	{
		hash += (uint)value * Prime3;

		return ((hash << 17) | (hash >> 15)) * Prime4;
	}

	public static int Finish(uint hash)
	{
		hash ^= hash >> 15;
		hash *= Prime2;
		hash ^= hash >> 13;
		hash *= Prime3;
		hash ^= hash >> 16;

		return (int)hash;
	}
}
