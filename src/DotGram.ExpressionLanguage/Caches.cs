using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace DotGram.ExpressionLanguage;

// What reflection has already answered, kept.
//
// Every answer here is the same every time it is asked and costs an allocation every
// time. The ones keyed by what a text said are bounded, because a text can grow them.

/// <summary>The expression language: its parser, and what it keeps between readings.</summary>
public static partial class ExpressionParser
{
	// What reflection answers about a type is the same every time it is asked, and costs an
	// allocation every time: a method's parameters are copied out on each `GetParameters`. A
	// call over `Math.Max` asks about a dozen overloads and, for each argument, whether
	// `IntPtr` declares a conversion to it — so each answer is kept once it has been worked
	// out, as the names of types already are.

	/// <summary>How many answers that say NOTHING are kept before they are forgotten.</summary>
	/// <remarks>
	/// <para>
	/// A cache whose key comes out of the text is a cache a text can grow: every `s.Nothing`
	/// anybody types is a name that is not there and an answer that says so, and a service
	/// reading what people send it would keep every one of them for ever.
	/// </para>
	/// <para>
	/// **Only such an answer can grow without limit.** An answer that FOUND something is bounded
	/// by the metadata of the assemblies a scope holds: there are only so many members on the
	/// types that exist. So the bound is on the answers that say nothing, and what a real
	/// vocabulary asks for is kept however large it is.
	/// </para>
	/// <para>
	/// This was measured rather than reasoned. A FIX 4.4 dictionary compiles 571 check texts of
	/// 598,232 characters and needs about 4,500 distinct member lookups; against the old bound,
	/// which counted every answer, the cache filled and cleared once per pass and the rest of
	/// each pass was reflection again. Of the answers it held, NONE said nothing.
	/// </para>
	/// </remarks>
	const int Remembered = 4096;

	/// <summary>An answer kept: found ones without limit, and ones that say nothing up to the bound.</summary>
	/// <remarks>
	/// The two are kept apart rather than counted apart, so that the bound means exactly what it
	/// says and clearing throws away only what it was written to protect against. Cleared whole
	/// rather than evicted one at a time: what pushes past the bound is a text naming something
	/// new each time, which nothing will ask about again, and keeping the order to evict the
	/// oldest would cost every lookup something to spare that case a rebuild it does not need.
	/// </remarks>
	static TValue Cached<TKey, TValue>(
		ConcurrentDictionary<TKey, TValue> kept,
		ConcurrentDictionary<TKey, TValue> absent,
		TKey key,
		Func<TKey, TValue> answer,
		Func<TValue, bool> nothing)
		where TKey : notnull
	{
		// Asked only where the answer is not here yet. `ConcurrentDictionary.Count` takes every
		// bucket lock and adds the buckets up, so asking it on the way in makes each lookup
		// cost what the cache has grown to — and a lookup is what this is for. A process that
		// has read one rich text paid that on every parse after it.
		if (kept.TryGetValue(key, out var found) || absent.TryGetValue(key, out found))
			return found;

		var made = answer(key);

		if (!nothing(made))
			return kept.GetOrAdd(key, made);

		if (absent.Count >= Remembered)
			absent.Clear();

		return absent.GetOrAdd(key, made);
	}

	/// <summary>Whether an answer found nothing, for each of the shapes an answer has.</summary>
	static bool Nothing<T>(T[] found)
	{
		return found is null || found.Length == 0;
	}

	/// <summary>The same for an answer that is one thing or none.</summary>
	static bool Nothing(object? found)
	{
		return found is null;
	}

	// Kept by caller as well, since what is reachable depends on who asks.
	//
	// What is kept is the overload and not its parameters alone. Whether a parameter is taken
	// by reference, and whether the last one is a `params` array, are answers about the member
	// rather than about any call, and asking them again for every call is most of what choosing
	// among overloads was spending itself on: over `Math.Max`, thirteen of them, ~57 ns a
	// candidate for the `params` attribute and ~20 ns for each parameter looked over.

	static readonly ConcurrentDictionary<(Type, string, bool, ResolutionScope), Overload[]> _methods = new();

	/// <summary>The same, for the names of that shape that are not there.</summary>
	static readonly ConcurrentDictionary<(Type, string, bool, ResolutionScope), Overload[]> _methodsAbsent = new();

	// An extension method is looked for in every static class an imported namespace holds, on
	// every call that finds nothing of its own: `Enumerable` alone is two hundred methods, and
	// each was asked its name, whether it is generic, whether it extends and whether the caller
	// may reach it, every time. Kept by the class that holds it, which no assembly loaded later
	// changes — what a load changes is which classes a namespace holds, and that is kept apart
	// and forgotten on a load. Like every cache here it holds the types it is keyed by, as
	// `_methods` does, and a collectible assembly's are kept alive by it no more and no less.
	static readonly ConcurrentDictionary<(Type, string, ResolutionScope), Overload[]> _extensions = new();

	/// <summary>The same, for the names of that shape that are not there.</summary>
	static readonly ConcurrentDictionary<(Type, string, ResolutionScope), Overload[]> _extensionsAbsent = new();

	static readonly ConcurrentDictionary<(Type, ResolutionScope), Overload[]> _constructors = new();

	static readonly ConcurrentDictionary<(Type, ResolutionScope), Overload[]> _indexers = new();

	// A member read is asked about as often as a call, and more: every `s.Length` asks it,
	// and every compound assignment asks it once in a guard and again where it is built.

	static readonly ConcurrentDictionary<(Type, string, ResolutionScope), MemberInfo?> _instanceMembers = new();

	/// <summary>The same, for the names of that shape that are not there.</summary>
	static readonly ConcurrentDictionary<(Type, string, ResolutionScope), MemberInfo?> _instanceMembersAbsent = new();

	static readonly ConcurrentDictionary<(Type, string, ResolutionScope), MemberInfo?> _staticMembers = new();

	/// <summary>The same, for the names of that shape that are not there.</summary>
	static readonly ConcurrentDictionary<(Type, string, ResolutionScope), MemberInfo?> _staticMembersAbsent = new();

	static readonly ConcurrentDictionary<(Type, Type), MethodInfo?> _operators = new();
}
