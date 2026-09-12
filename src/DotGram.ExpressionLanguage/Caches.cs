using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace DotGram.ExpressionLanguage;

// What reflection has already answered, kept.
//
// Every answer here is the same every time it is asked and costs an allocation every
// time. The ones keyed by what a text said are bounded, because a text can grow them.

public static partial class ExpressionParser
{
	// What reflection answers about a type is the same every time it is asked, and costs an
	// allocation every time: a method's parameters are copied out on each `GetParameters`. A
	// call over `Math.Max` asks about a dozen overloads and, for each argument, whether
	// `IntPtr` declares a conversion to it — so each answer is kept once it has been worked
	// out, as the names of types already are.

	/// <summary>How many answers keyed by what a text said are kept before they are forgotten.</summary>
	/// <remarks>
	/// A cache whose key comes out of the text is a cache a text can grow: every `s.Nothing`
	/// anybody types is a name that is not there and an answer that says so, and a service
	/// reading what people send it would keep every one of them for ever.
	/// </remarks>
	const int Remembered = 4096;

	/// <summary>An answer kept, where what is kept cannot grow past <see cref="Remembered"/>.</summary>
	/// <remarks>
	/// Cleared whole rather than evicted one at a time. What a grammar really asks about
	/// settles far below the bound — the types, members and methods one language names — and
	/// what pushes past it is a text naming something new each time, which nothing will ask
	/// about again. Keeping the order to evict the oldest would cost every lookup something
	/// to spare that case a rebuild it does not need.
	/// </remarks>
	static TValue Cached<TKey, TValue>(
		ConcurrentDictionary<TKey, TValue> cache, TKey key, Func<TKey, TValue> answer)
		where TKey : notnull
	{
		// Asked only where the answer is not here yet. `ConcurrentDictionary.Count` takes every
		// bucket lock and adds the buckets up, so asking it on the way in makes each lookup
		// cost what the cache has grown to — and a lookup is what this is for. A process that
		// has read one rich text paid that on every parse after it.
		if (cache.TryGetValue(key, out var found))
			return found;

		if (cache.Count >= Remembered)
			cache.Clear();

		return cache.GetOrAdd(key, answer);
	}

	// Kept by caller as well, since what is reachable depends on who asks.

	static readonly ConcurrentDictionary<(Type, string, bool, Assembly), (MemberInfo, ParameterInfo[])[]> _methods = new();

	static readonly ConcurrentDictionary<(Type, Assembly), (MemberInfo, ParameterInfo[])[]> _constructors = new();

	static readonly ConcurrentDictionary<(Type, Assembly), (MemberInfo, ParameterInfo[])[]> _indexers = new();

	// A member read is asked about as often as a call, and more: every `s.Length` asks it,
	// and every compound assignment asks it once in a guard and again where it is built.

	static readonly ConcurrentDictionary<(Type, string, Assembly), MemberInfo?> _instanceMembers = new();

	static readonly ConcurrentDictionary<(Type, string, Assembly), MemberInfo?> _staticMembers = new();

	static readonly ConcurrentDictionary<(Type, Type), MethodInfo?> _operators = new();
}
