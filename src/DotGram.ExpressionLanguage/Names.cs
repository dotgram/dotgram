using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotGram.ExpressionLanguage;

// What a name written as a type means in the caller's runtime reference graph.
//
// A layer of its own because it answers before anything is built: whether a word is a
// type at all is what tells `(Foo)x` from `(foo)`, and the grammar asks it while the
// text is read.

public static partial class ExpressionParser
{
	// ── What a name written as a type means ─────────────────────────────────────
	//
	// The keywords are the grammar's, written as `typeof(int)` where the C# compiler reads
	// them. A name is not: `Exception` means something only against a set of namespaces to
	// look in, and the text says which with a `using`, as a C# file does. Nothing is
	// imported unasked, `System` included — what a name means is written where it is used.
	// The `using`s belong to the reading (`State`); name caches belong to its caller.

	/// <summary>A dotted name from the words the grammar read, and nothing between them.</summary>
	/// <remarks>
	/// The parts and not the run: the words are captured one at a time, so whatever spacing
	/// stood between them in the text is not in the name. `System . Text` is `System.Text`,
	/// which is what it means and what the lookup below can answer about.
	/// </remarks>
	/// <summary>The last word of a dotted name, which is what <c>nameof</c> answers with.</summary>
	/// <remarks>
	/// `nameof(s.Length)` is "Length" and `nameof(x)` is "x": C# answers with the name and
	/// not with the path to it, and the path is what the parts before the last one are.
	/// </remarks>
	internal static string Last(string head, string[]? tail) =>
		tail is { Length: > 0 } ? tail[tail.Length - 1] : head;

	internal static string Dotted(string head, string[]? tail) =>
		tail is null || tail.Length == 0 ? head : head + "." + string.Join(".", tail);

	/// <summary>A dotted name within a namespace as a type, or null where it is none.</summary>
	/// <remarks>
	/// The longest part of the name that is a type by its full name, and the rest as types
	/// nested in it one at a time — so `Environment.SpecialFolder` is found through
	/// <c>Environment</c>, which metadata calls <c>System.Environment+SpecialFolder</c> and no
	/// full name written with dots would reach.
	///
	/// The calling assembly is asked first, and its internal types answer as well as its
	/// public ones: a type the calling code declares stands in front of one of the same full
	/// name elsewhere, as a type in C#'s own compilation does.
	/// </remarks>
	static Type? Qualified(string? space, string dotted, Assembly caller)
	{
		var end = dotted.Length;

		while (true)
		{
			var head = dotted.Substring(0, end);
			var full = space is null ? head : space + "." + head;

			if ((Loaded.Inside(caller, full) ?? Loaded.Find(caller, full)) is { } type)
			{
				for (var at = end; type is not null && at < dotted.Length;)
				{
					var next = dotted.IndexOf('.', at + 1);

					if (next < 0)
						next = dotted.Length;

					type = Nested(type, dotted.Substring(at + 1, next - at - 1), caller);
					at   = next;
				}

				return type;
			}

			end = dotted.LastIndexOf('.', end - 1);

			if (end < 0)
				return null;
		}
	}

	/// <summary>A type nested in another by that name, where C# in the calling assembly could name it.</summary>
	/// <remarks>
	/// Public, or — inside a type the calling assembly declares — internal or protected
	/// internal. Never private or protected alone: nothing here is written inside the type
	/// that holds it, or one derived from it.
	/// </remarks>
	static Type? Nested(Type outer, string name, Assembly caller) =>
		outer.GetNestedType(name, BindingFlags.Public | BindingFlags.NonPublic) is { } nested &&
		(nested.IsNestedPublic || outer.Assembly == caller && (nested.IsNestedAssembly || nested.IsNestedFamORAssem))
			? nested
			: null;

	/// <summary>Names in the caller's assembly and its transitive runtime references.</summary>
	/// <remarks>
	/// <para>
	/// The reference graph is fixed per caller. Loading an unrelated assembly cannot change
	/// a name, namespace or extension-method lookup. Other assemblies contribute public types.
	/// </para>
	/// <para>
	/// This is the runtime reference graph, not the compiler's original reference list.
	/// References are loaded through their owner's load context on .NET and through the
	/// assembly loader on .NET Framework. Duplicate public names are ambiguous.
	/// </para>
	/// </remarks>
	static class Loaded
	{
		static readonly ConcurrentDictionary<(Assembly, string), Type?> _types = new();
		static readonly ConcurrentDictionary<Assembly, HashSet<string>> _namespaces = new();
		static readonly ConcurrentDictionary<(Assembly, string), Type[]> _holders = new();
		static readonly ConditionalWeakTable<Assembly, Assembly[]> _references = new();

		static Assembly[] References(Assembly caller) => _references.GetValue(caller, static root =>
		{
			var seen = new HashSet<Assembly> { root };
			var pending = new Queue<Assembly>();
			var references = new List<Assembly>();
			pending.Enqueue(root);
			while (pending.Count > 0)
			{
				var owner = pending.Dequeue();
				foreach (var name in owner.GetReferencedAssemblies())
				{
					var assembly = LoadReference(owner, name);
					if (!seen.Add(assembly))
						continue;
					references.Add(assembly);
					pending.Enqueue(assembly);
				}
			}
			return [.. references];
		});

		// Reflection keeps the netstandard2.0 asset usable on .NET Framework without a
		// System.Runtime.Loader package dependency, while honoring .NET load contexts.
		static readonly Type? ContextType = typeof(object).Assembly.GetType("System.Runtime.Loader.AssemblyLoadContext");
		static readonly MethodInfo? GetContext = ContextType?.GetMethod("GetLoadContext", [typeof(Assembly)]);
		static readonly MethodInfo? LoadInContext = ContextType?.GetMethod("LoadFromAssemblyName", [typeof(AssemblyName)]);

		static Assembly LoadReference(Assembly owner, AssemblyName name) =>
			GetContext?.Invoke(null, [owner]) is { } context
				? (Assembly)LoadInContext!.Invoke(context, [name])!
				: Assembly.Load(name);

		static readonly ConcurrentDictionary<(Assembly, string), Type[]> _holdersInside = new();

		/// <summary>The public static classes in that namespace in the caller's references.</summary>
		/// <remarks>
		/// What an extension method is written in, and the only thing worth walking a namespace
		/// for: a class that is not static holds none, and C# looks for one nowhere else.
		/// </remarks>
		public static Type[] Holders(Assembly caller, string @namespace) =>
			Cached(_holders, (caller, @namespace), static key => Held(key.Item1, key.Item2));

		/// <summary>The same in the calling assembly, where an internal class is nameable too.</summary>
		public static Type[] HoldersInside(Assembly caller, string @namespace) =>
			_holdersInside.GetOrAdd(
				(caller, @namespace),
				static key =>
				{
					var holders = new List<Type>();

					foreach (var type in Declared(key.Item1))
						if (Nameable(type) && Holds(type, key.Item2))
							holders.Add(type);

					return [.. holders];
				});

		static Type[] Held(Assembly caller, string @namespace)
		{
			var holders = new List<Type>();

			foreach (var assembly in References(caller))
			{
				if (assembly.IsDynamic)
					continue;

				foreach (var type in Declared(assembly))
					if (type.IsVisible && Holds(type, @namespace))
						holders.Add(type);
			}

			return [.. holders];
		}

		/// <summary>Whether a type is a static class standing in that namespace, extensions and all.</summary>
		/// <remarks>
		/// A static class is abstract and sealed at once, which is how C# writes one into
		/// metadata, and one holding extension methods carries the attribute the compiler puts
		/// on it — asked here so that a namespace of ordinary classes costs one test each.
		/// </remarks>
		static bool Holds(Type type, string @namespace) =>
			type is { IsAbstract: true, IsSealed: true, IsNested: false, IsGenericTypeDefinition: false } &&
			string.Equals(type.Namespace, @namespace, StringComparison.Ordinal) &&
			type.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);

		/// <summary>An assembly's types, or as many of them as it can load.</summary>
		static IEnumerable<Type> Declared(Assembly assembly)
		{
			try
			{
				return assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException partial)
			{
				return partial.Types.OfType<Type>();
			}
		}

		/// <summary>The unique public type by that full name in the caller's references, or null.</summary>
		public static Type? Find(Assembly caller, string fullName) =>
			Cached(_types, (caller, fullName), static key => Search(key.Item1, key.Item2));

		/// <summary>Whether a reference has a public type in that namespace, or in one inside it.</summary>
		/// <remarks>
		/// Inside it too, because C# takes `using System.Collections;` whether or not that
		/// namespace declares a type of its own: it is there because something is in it.
		/// </remarks>
		public static bool Has(Assembly caller, string @namespace) =>
			_namespaces.GetOrAdd(caller, static assembly => Gather(assembly)).Contains(@namespace);

		static readonly ConcurrentDictionary<(Assembly, string), Type?> _inside = new();

		static readonly ConcurrentDictionary<Assembly, HashSet<string>> _insideNamespaces = new();

		/// <summary>
		/// The type that full name means in the calling assembly — an internal one as well —
		/// where that assembly's own code could name it; or null.
		/// </summary>
		/// <remarks>
		/// Kept apart from the rest and never forgotten: what one assembly declares does not
		/// change when another loads.
		/// </remarks>
		public static Type? Inside(Assembly caller, string fullName) =>
			Cached(
				_inside,
				(caller, fullName),
				static key => key.Item1.GetType(key.Item2, false, false) is { } type && Nameable(type) ? type : null);

		/// <summary>Whether the calling assembly declares a type in that namespace, or in one inside it.</summary>
		public static bool HasInside(Assembly caller, string @namespace) =>
			_insideNamespaces.GetOrAdd(caller, static assembly => Spaces(assembly)).Contains(@namespace);

		/// <summary>Every namespace an assembly's nameable types stand in, and each one around those.</summary>
		static HashSet<string> Spaces(Assembly assembly)
		{
			IEnumerable<Type> types;

			try
			{
				types = assembly.GetTypes();
			}
			catch (ReflectionTypeLoadException partial)
			{
				types = partial.Types.OfType<Type>();
			}

			var namespaces = new HashSet<string>(StringComparer.Ordinal);

			foreach (var type in types)
			{
				if (!Nameable(type))
					continue;

				var space = type.Namespace;

				while (space is not null && namespaces.Add(space))
					space = space.LastIndexOf('.') is var dot and >= 0 ? space.Substring(0, dot) : null;
			}

			return namespaces;
		}

		/// <summary>
		/// Whether code in a type's own assembly could name it: nested, if at all, only in types
		/// it could name, and never private or protected alone.
		/// </summary>
		static bool Nameable(Type type)
		{
			for (var each = type; each.IsNested; each = each.DeclaringType!)
				if (!(each.IsNestedPublic || each.IsNestedAssembly || each.IsNestedFamORAssem))
					return false;

			return true;
		}

		static Type? Search(Assembly caller, string name)
		{
			Type? found = null;
			foreach (var assembly in References(caller))
				if (!assembly.IsDynamic && assembly.GetType(name, false, false) is { IsVisible: true } type)
				{
					if (found is not null && found != type)
						throw new InvalidOperationException($"'{name}' is ambiguous between '{found.Assembly.FullName}' and '{type.Assembly.FullName}'.");
					found = type;
				}

			return found;
		}

		static HashSet<string> Gather(Assembly caller)
		{
			var namespaces = new HashSet<string>(StringComparer.Ordinal);

			foreach (var assembly in References(caller))
			{
				if (assembly.IsDynamic)
					continue;

				Type[] types;

				try
				{
					types = assembly.GetExportedTypes();
				}
				catch (Exception exception) when (
					exception is NotSupportedException or TypeLoadException or ReflectionTypeLoadException or
						System.IO.FileNotFoundException or System.IO.FileLoadException)
				{
					// An assembly whose types cannot all be loaded contributes none, as a
					// reference C# cannot read contributes none.
					continue;
				}

				foreach (var type in types)
				{
					var space = type.Namespace;

					while (space is not null && namespaces.Add(space))
						space = space.LastIndexOf('.') is var dot and >= 0 ? space.Substring(0, dot) : null;
				}
			}

			return namespaces;
		}
	}
}
