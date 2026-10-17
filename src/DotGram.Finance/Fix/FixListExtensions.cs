#if FIXSHAPE_Q

using System;

using DotGram.Finance.Fix;

// In System.Linq, as System.Linq.Async puts its operators: a consumer that imports only System.Linq reaches them.
namespace System.Linq;

/// <summary>
/// The query operators with a condition over a <see cref="FixList{T}"/> as plain loops: they take the list itself, so a call
/// is bound here and not to the <see cref="System.Linq.Enumerable"/> overload that boxes the enumerator. The others find their fast paths through
/// <see cref="System.Collections.Generic.ICollection{T}"/> and <see cref="System.Collections.Generic.IList{T}"/>.
/// </summary>
public static class FixListExtensions
{
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
}

#endif
