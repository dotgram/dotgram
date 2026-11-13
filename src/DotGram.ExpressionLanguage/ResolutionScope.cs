using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

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
/// The assemblies are walked once, and not before they are wanted: the first question that
/// needs them walks the graph, and a text that asks none never does. A scope's answers cannot
/// change under it either way, which is the whole of what it is for — the closure is derived
/// from names, and when it is derived changes nothing about what it holds.
/// </para>
/// </remarks>
public sealed class ResolutionScope
{
	ResolutionScope(Assembly caller, Lazy<Assembly[]> assemblies, bool internals)
		: this(caller, assemblies, internals, true, null)
	{
	}

	ResolutionScope(
		Assembly caller, Lazy<Assembly[]> assemblies, bool internals, bool defaults, ResolutionScope? root)
	{
		Caller         = caller;
		SeesInternals  = internals;
		ImportsDefault = defaults;
		_assemblies    = assemblies;
		_root          = root ?? this;
	}

	/// <summary>The namespaces every text may name without writing a `using` for them.</summary>
	/// <remarks>
	/// <para>
	/// The five namespaces C#'s classic class-file template writes at the top of a new file, so
	/// that a text reads with the names a C# file starts with. They stand as if the text had
	/// written them before its own `using`s, and are PEERS of them, which is what C# does with a
	/// global using: a name two of them both give is ambiguous where it is used,
	/// whichever of the two the text wrote (CS0104, checked against Roslyn).
	/// </para>
	/// <para>
	/// They are recorded without being looked for. A namespace here that the scope's assemblies
	/// do not hold is simply absent, not a refusal — a text's own `using` of one that is not
	/// there is still CS0246 — and that is also what keeps them from walking the closure: a text
	/// naming no type must load nothing, and checking five namespaces would have loaded
	/// everything.
	/// </para>
	/// <para>
	/// What they cost: a text that calls an extension method searches these namespaces for one
	/// besides its own, which is a constant handful of cached lookups per search — the answers
	/// are kept per namespace and per holder, so nothing is worked out twice and no more methods
	/// are tested for applicability. On an extension-heavy text it is single digit percent; on a
	/// text that calls no extension method it is nothing at all, and it is never bytes: the
	/// same methods are tested and the same memory is allocated.
	/// <see cref="WithoutDefaultImports"/> is the way out.
	/// </para>
	/// </remarks>
	public static IReadOnlyList<string> DefaultImports { get; } =
	[
		"System",
		"System.Collections.Generic",
		"System.Linq",
		"System.Text",
		"System.Threading.Tasks",
	];

	/// <summary>Whether <see cref="DefaultImports"/> stand in a text read through this scope.</summary>
	public bool ImportsDefault { get; }

	/// <summary>The scope this one was shaped from, which holds the shapes of it.</summary>
	/// <remarks>
	/// A scope is what every cache inside the parser is keyed by, so a shaping form that made a
	/// new instance on every call would empty those caches on every call. The four shapes of one
	/// scope — internals on or off, defaults on or off — are made once each and kept here, so
	/// that <c>WithoutInternals().WithoutDefaultImports()</c> and the same two the other way
	/// round are not merely equal but the SAME instance.
	/// </remarks>
	readonly ResolutionScope _root;

	readonly ResolutionScope?[] _shapes = new ResolutionScope?[4];

	/// <summary>This scope shaped that way, made once and kept.</summary>
	ResolutionScope Shaped(bool internals, bool defaults)
	{
		if (internals == SeesInternals && defaults == ImportsDefault)
			return this;

		var root = _root;
		var at   = (internals ? 1 : 0) + (defaults ? 2 : 0);

		lock (root._shapes)
		{
			return root._shapes[at] ??= internals == root.SeesInternals && defaults == root.ImportsDefault
				? root
				: new ResolutionScope(root.Caller, root._assemblies, internals, defaults, root);
		}
	}

	/// <summary>The closure, walked at the first question that needs it and once however many ask.</summary>
	/// <remarks>
	/// <c>ExecutionAndPublication</c> and not a cheaper mode: two readings on two threads asking
	/// their first question at the same moment must walk the graph once between them, since the
	/// walk LOADS assemblies and doing it twice would do that twice.
	/// </remarks>
	readonly Lazy<Assembly[]> _assemblies;

	/// <summary>A scope whose closure is worked out when something first asks for it.</summary>
	static ResolutionScope Deferred(Assembly caller, Func<Assembly[]> closure, bool internals)
	{
		return new ResolutionScope(
			caller, new Lazy<Assembly[]>(closure, LazyThreadSafetyMode.ExecutionAndPublication), internals);
	}

	static readonly ConcurrentDictionary<Assembly, ResolutionScope> _around = new();

	/// <summary>The assembly the text is read for, and whose internal members it may name.</summary>
	public Assembly Caller { get; }

	/// <summary>Every assembly a name may be found in, the caller first.</summary>
	/// <remarks>
	/// Reading this is what walks the graph, where nothing has yet. A host that would rather have
	/// the walk over with, at a moment of its own choosing than inside its first reading, reads it.
	/// </remarks>
	public IReadOnlyList<Assembly> Assemblies => _assemblies.Value;

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
	/// <b>None of it happens here.</b> The walk is done at the first question that needs the
	/// assemblies — a name looked for as a type, a method looked for among the extensions of an
	/// imported namespace, or a host reading <see cref="Assemblies"/> — and a text that asks none
	/// of those never causes it. Counted: of `(int x) => x`, `(int x) => (x + 1) * 2`,
	/// `using System; (string s) => s.Length > 0 ? s.Trim() : s` and
	/// `(int x) => System.Math.Abs(x) + 1`, the first three ask the type tables nothing at all,
	/// because a keyword type and a member access never reach them.
	/// </para>
	/// <para>
	/// <b>It changes when, and nothing else.</b> The closure is derived from the names an assembly
	/// references, and deriving it later does not change what it holds: in the default load
	/// context a name binds to the first assembly loaded under it, eager or late alike. The two
	/// can differ only where the process ITSELF changes what a name resolves to in between — a
	/// different version loaded under the same name, or a reference that could not be found at
	/// scope creation and can be found later. A host that does that has a moving deployment, and
	/// a scope that wants the walk over with at a moment of its own choosing reads
	/// <see cref="Assemblies"/>.
	/// </para>
	/// <para>
	/// <b>What that side effect costs.</b> Over a large application's assemblies, some 20 ms,
	/// nearly all of it the runtime loading assemblies and not the walk itself; it is paid on the
	/// first text that NAMES a type, not at scope creation, and once for the life of the scope.
	/// </para>
	/// <para>
	/// <b>A long name costs once more.</b> The first dotted name of more than nine parts that may
	/// be a type asks how deep the types in reach go, which lists the public and forwarded types
	/// of every assembly in the closure, and every type of the caller's where its internals are
	/// seen: about 12 ms and 260 KB over 29 assemblies. Each assembly is
	/// counted once in the process, whatever scopes it is in, so a scope over assemblies already
	/// counted pays a lookup each. A name whose head the text declared — a member chain on a
	/// parameter — never asks, and walks nothing.
	/// </para>
	/// <para>
	/// It is paid ONCE for an assembly, in the process that reads texts for it, and a host that
	/// reads more than one text never pays it again. A host whose texts name no types does not pay
	/// it at all. Reading each assembly's names off its file instead, with metadata and loading
	/// nothing, was tried and dropped: the runtime's loader is faster at this than reading every
	/// type name is.
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

		return _around.GetOrAdd(caller, static one => Deferred(one, () => Closure(one, null), true));
	}

	/// <summary>
	/// The scope of an assembly and what it references, with those further assemblies as more
	/// references of it.
	/// </summary>
	public static ResolutionScope Of(Assembly caller, params Assembly[] assemblies)
	{
		if (caller is null)
			throw new ArgumentNullException(nameof(caller));

		return assemblies is null || assemblies.Length == 0
			? Around(caller)
			: Deferred(caller, () => Closure(caller, assemblies), true);
	}

	/// <summary>This scope and those assemblies, as a new one; this one is unchanged.</summary>
	public ResolutionScope With(params Assembly[] more)
	{
		return more is null || more.Length == 0
			? this
			: Deferred(Caller, () => Closure(Caller, Joined(Assemblies, more)), SeesInternals);
	}

	/// <summary>
	/// This scope with <see cref="DefaultImports"/> left out, so that a text names only the
	/// namespaces its own `using`s give; this one is unchanged.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Where it looks is unchanged, so the closure is shared and never walked twice; what changes
	/// is only whether <see cref="DefaultImports"/> stand. Composes with
	/// <see cref="WithoutInternals"/> in either order, and gives the same instance either way.
	/// </para>
	/// <para>
	/// Leaving them out is worth a little speed on a text that calls extension methods: the five
	/// namespaces are searched for one besides the text's own, a constant handful of cached
	/// lookups per search, single digit percent on an extension-heavy text and nothing on a text
	/// without such a call. It costs no allocation either way. The reason to leave them out is
	/// ordinarily that a host wants a text to name what it uses, not the speed.
	/// </para>
	/// </remarks>
	public ResolutionScope WithoutDefaultImports()
	{
		return Shaped(SeesInternals, false);
	}

	/// <summary>
	/// This scope with the caller's internal types and members hidden, as another assembly would
	/// see them; this one is unchanged.
	/// </summary>
	public ResolutionScope WithoutInternals()
	{
		// The same closure, so the same Lazy: two scopes that differ in what they may SEE of the
		// caller do not differ in where they look, and sharing it walks the graph once for both.
		return Shaped(false, ImportsDefault);
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
					Consider(Load(one, name));
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

	/// <summary>
	/// A reference loaded where its owner was: through the owner's load context on .NET, through
	/// Assembly.Load on .NET Framework, which has no AssemblyLoadContext.
	/// </summary>
	/// <remarks>
	/// Not <c>Assembly.Load</c> on .NET: that binds in the context of the assembly calling it, which
	/// is this one, so a caller loaded into a context of its own — a plugin — had its references
	/// looked for in the default context, found nothing there, and could name none of their types.
	/// </remarks>
	static Assembly Load(Assembly owner, AssemblyName name)
	{
#if NETSTANDARD2_0
		// The netstandard2.0 asset runs on .NET too, where the context exists but cannot be named.
		if (LoadFromAssemblyName is null || GetLoadContext?.Invoke(null, [owner]) is not { } context)
			return Assembly.Load(name);

		try
		{
			return (Assembly)LoadFromAssemblyName.Invoke(context, [name])!;
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is { } inner)
		{
			System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(inner).Throw();
			throw;
		}
#else
		return (System.Runtime.Loader.AssemblyLoadContext.GetLoadContext(owner) ??
			System.Runtime.Loader.AssemblyLoadContext.Default).LoadFromAssemblyName(name);
#endif
	}

#if NETSTANDARD2_0
	static readonly Type?       LoadContextType      = typeof(object).Assembly.GetType("System.Runtime.Loader.AssemblyLoadContext");
	static readonly MethodInfo? GetLoadContext       = LoadContextType?.GetMethod("GetLoadContext", [typeof(Assembly)]);
	static readonly MethodInfo? LoadFromAssemblyName = LoadContextType?.GetMethod("LoadFromAssemblyName", [typeof(AssemblyName)]);
#endif

	static Assembly[] Joined(IReadOnlyList<Assembly> first, Assembly[] second)
	{
		var all = new Assembly[first.Count + second.Length];

		for (var at = 0; at < first.Count; at++)
			all[at] = first[at];

		second.CopyTo(all, first.Count);

		return all;
	}
}
