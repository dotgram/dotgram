#if FIXSHAPE_Q

using System;

using DotGram.Finance.Fix;

// In System.Linq, as System.Linq.Async puts its operators: a consumer that imports only System.Linq reaches them.
namespace System.Linq;

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
	public static int Count<T>(this DotGram.Finance.Fix.FixList<T> list)
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
	public static int Count<T>(this DotGram.Finance.Fix.FixList<T> list, Func<T, bool> predicate)
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
	public static bool Any<T>(this DotGram.Finance.Fix.FixList<T> list)
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
	public static bool Any<T>(this DotGram.Finance.Fix.FixList<T> list, Func<T, bool> predicate)
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
	public static T First<T>(this DotGram.Finance.Fix.FixList<T> list)
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
	public static T First<T>(this DotGram.Finance.Fix.FixList<T> list, Func<T, bool> predicate)
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
	public static T? FirstOrDefault<T>(this DotGram.Finance.Fix.FixList<T> list)
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
	public static T? FirstOrDefault<T>(this DotGram.Finance.Fix.FixList<T> list, Func<T, bool> predicate)
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
	public static DotGram.Finance.Fix.FixList<T> Where<T>(this DotGram.Finance.Fix.FixList<T> list, Func<T, bool> predicate)
	{
		var items = list.Buffer;
		var count = list.Count;
		var found = new DotGram.Finance.Fix.FixList<T>();

		for (var i = 0; i < count; i++)
			if (predicate(items[i]))
				found.Add(items[i]);

		return found.Count == 0 ? DotGram.Finance.Fix.FixList<T>.Empty : found;
	}

	/// <summary>
	/// The entries in a new array.
	/// </summary>
	/// <typeparam name="T">The type of an entry.</typeparam>
	/// <param name="list">The list.</param>
	/// <returns>The array.</returns>
	public static T[] ToArray<T>(this DotGram.Finance.Fix.FixList<T> list)
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
