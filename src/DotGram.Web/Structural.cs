using System;
using System.Collections.Generic;

namespace DotGram.Web;

/// <summary>Equality by content for the lists the records of this package hold.</summary>
/// <remarks>
/// A record compares a list it holds by reference, which makes two readings of one text unequal.
/// The records whose value is a list say what equal means through these, element by element and in order.
/// </remarks>
static class Structural
{
	public static bool Same<T>(IReadOnlyList<T>? left, IReadOnlyList<T>? right, IEqualityComparer<T>? comparer = null)
	{
		if (ReferenceEquals(left, right))
			return true;

		if (left is null || right is null || left.Count != right.Count)
			return false;

		comparer ??= EqualityComparer<T>.Default;

		for (var index = 0; index < left.Count; index++)
			if (!comparer.Equals(left[index], right[index]))
				return false;

		return true;
	}

	public static int Hash<T>(IReadOnlyList<T>? items, IEqualityComparer<T>? comparer = null)
	{
		if (items is null)
			return 0;

		comparer ??= EqualityComparer<T>.Default;

		var hash = items.Count;

		foreach (var item in items)
			hash = Combine(hash, item is null ? 0 : comparer.GetHashCode(item));

		return hash;
	}

	public static int Combine(int hash, int next) => unchecked(hash * 31 + next);
}
