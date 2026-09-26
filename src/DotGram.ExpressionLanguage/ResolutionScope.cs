using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace DotGram.ExpressionLanguage;

// Where a text's names are looked for.
//
// Before this, they were looked for in whatever the process had loaded. That is not a rule a
// caller can reason about: the same text handed to the same call twice could mean two things,
// if something loaded in between. It is also not what C# does — a compilation sees its
// references, not the contents of the machine — and this language's whole contract is to be C#.
//
// So a scope is a compilation's references, written down: the assembly the text is read for, and
// what that assembly references, transitively. Nothing is found merely because it is loaded.
//
// What it is NOT is a resolver. The host does not decide what a name means; it decides where the
// name is looked for, and the meaning stays C#'s — which member of an overload set, which of two
// types a bare name is, when an internal one is nameable. Those rules took two defects in one
// week to get right, and an implementation of them handed to every host would fail quietly
// rather than loudly.

/// <summary>Where a text's names are looked for: an assembly and what it references.</summary>
/// <remarks>
/// <para>
/// Immutable, and compared by identity. <see cref="Around(Assembly)"/> returns the same instance
/// for the same assembly, so the common path makes no new key: a scope is what every cache inside
/// the parser is keyed by, and a fresh instance per call would empty all of them on every call.
/// The shaping forms — <see cref="Of"/>, <see cref="With"/>, <see cref="WithoutInternals"/> —
/// each make one scope, which a host is expected to keep rather than build per reading.
/// </para>
/// <para>
/// The assemblies are walked once, when the scope is made. A scope's answers therefore cannot
/// change under it, which is the whole of what it is for.
/// </para>
/// </remarks>
public sealed class ResolutionScope
{
	ResolutionScope(Assembly caller, Assembly[] assemblies, bool internals)
	{
		Caller        = caller;
		Assemblies    = assemblies;
		SeesInternals = internals;
	}

	static readonly ConcurrentDictionary<Assembly, ResolutionScope> _around = new();

	/// <summary>The assembly the text is read for, and whose internal members it may name.</summary>
	public Assembly Caller { get; }

	/// <summary>Every assembly a name may be found in, the caller first.</summary>
	public IReadOnlyList<Assembly> Assemblies { get; }

	/// <summary>Whether the caller's own internal types and members answer.</summary>
	public bool SeesInternals { get; }

	/// <summary>The scope a compilation of that assembly would have: it and what it references.</summary>
	/// <remarks>
	/// <para>
	/// The references are walked transitively and loaded, which is the one side effect this has:
	/// an assembly named in the graph but not yet in the process is brought in. One instance per
	/// assembly, kept, so that asking twice is one walk and one cache key.
	/// </para>
	/// <para>
	/// <b>What that side effect costs, measured 2026-09-26.</b> Over the benchmark assembly it is
	/// 23 ms and 145 assemblies brought in — the process held 12 before the call and 157 after —
	/// and the second call is 0.02 ms, being the same instance. On the stand's first-call rows that
	/// is the whole of a first reading's rise: `el/floor` 21.0 ms to 40.7, `el/ladder` 24.8 to 43.7,
	/// `el/block` 27.5 to 47.4, on both carriers, while the methods the runtime compiled went DOWN
	/// (192 to 165 for `el/floor`) — so it is loading and not compiling.
	/// </para>
	/// <para>
	/// It is paid ONCE for an assembly, in the process that reads texts for it, and a host that
	/// reads more than one text never pays it again. A host that reads one short text in a short
	/// process pays it for that one text, and there it is most of what the reading costs. Whether
	/// the default should load the closure eagerly or only name it and load where a name is looked
	/// for is open (docs/design/expression-resolution-scope-2026-09-25.md); the SET a scope answers
	/// from is the closure either way, so deferring the loading would not make an answer depend on
	/// what the process has loaded.
	/// </para>
	/// <para>
	/// <b>That instance is kept for the life of the process, and so is what it holds.</b> A scope
	/// names its assemblies, and the parser's caches hold the types and members it has resolved,
	/// keyed by it — so an assembly a scope has been made around, or has ever answered from,
	/// stays reachable. For a collectible <c>AssemblyLoadContext</c> that means it will not
	/// unload. A host that loads and unloads plugins should read their texts through a scope
	/// built over assemblies that outlive them, or accept that what it has read keeps them.
	/// Making the interning weak alone would not change this: the caches hold them either way.
	/// </para>
	/// </remarks>
	public static ResolutionScope Around(Assembly caller)
	{
		if (caller is null)
			throw new ArgumentNullException(nameof(caller));

		return _around.GetOrAdd(caller, static one => new ResolutionScope(one, Closure(one, null), true));
	}

	/// <summary>The same, and those assemblies too, as further references of it.</summary>
	public static ResolutionScope Of(Assembly caller, params Assembly[] assemblies)
	{
		if (caller is null)
			throw new ArgumentNullException(nameof(caller));

		return assemblies is null || assemblies.Length == 0
			? Around(caller)
			: new ResolutionScope(caller, Closure(caller, assemblies), true);
	}

	/// <summary>This scope and those assemblies, as a new one; this one is unchanged.</summary>
	public ResolutionScope With(params Assembly[] more)
	{
		return more is null || more.Length == 0
			? this
			: new ResolutionScope(Caller, Closure(Caller, Joined(Assemblies, more)), SeesInternals);
	}

	/// <summary>The same scope with the caller's internals hidden, as another assembly would see it.</summary>
	public ResolutionScope WithoutInternals()
	{
		return SeesInternals ? new ResolutionScope(Caller, [.. Assemblies], false) : this;
	}

	/// <summary>Whether that assembly's internal types and members answer in this scope.</summary>
	/// <remarks>
	/// The caller's, and no other's. C# gives a compilation the internals of the assembly being
	/// compiled, and `InternalsVisibleTo` is a grant one assembly makes to a NAMED other at build
	/// time — not something a reading can claim for itself. <see cref="WithoutInternals"/> turns
	/// even the caller's off, which is how a host reads a text the way another assembly would.
	/// </remarks>
	internal bool Declares(Assembly assembly)
	{
		return SeesInternals && assembly == Caller;
	}

	/// <summary>The caller, what it references, and anything else named, each once and in order.</summary>
	/// <remarks>
	/// An assembly that cannot be loaded is passed over rather than thrown for: a reference the
	/// graph names and the process cannot find is a fact about the deployment, and a text that
	/// does not name anything in it should still read.
	/// </remarks>
	static Assembly[] Closure(Assembly caller, Assembly[]? also)
	{
		var seen  = new HashSet<string>(StringComparer.Ordinal);
		var found = new List<Assembly>();
		var queue = new Queue<Assembly>();

		Consider(caller);

		foreach (var one in also ?? [])
			Consider(one);

		while (queue.Count > 0)
		{
			var one = queue.Dequeue();

			found.Add(one);

			foreach (var name in one.GetReferencedAssemblies())
			{
				if (seen.Contains(name.FullName))
					continue;

				try
				{
					Consider(Assembly.Load(name));
				}
				catch (Exception thrown) when (thrown is BadImageFormatException or System.IO.FileNotFoundException
					or System.IO.FileLoadException)
				{
					seen.Add(name.FullName);
				}
			}
		}

		return [.. found];

		void Consider(Assembly one)
		{
			if (one is not null && !one.IsDynamic && seen.Add(one.FullName!))
				queue.Enqueue(one);
		}
	}

	static Assembly[] Joined(IReadOnlyList<Assembly> first, Assembly[] second)
	{
		var all = new Assembly[first.Count + second.Length];

		for (var at = 0; at < first.Count; at++)
			all[at] = first[at];

		second.CopyTo(all, first.Count);

		return all;
	}
}
