using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace DotGram.ExpressionLanguage;

// What a name written as a type means, and what the loaded assemblies say about names.
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
	// The `using`s belong to the reading (`State`); what is shared is only what the loaded
	// assemblies say, which is the same for every reading.

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
	internal static string Last(string head, string[]? tail)
	{
		return tail is { Length: > 0 } ? tail[tail.Length - 1] : head;
	}

	internal static string Dotted(string head, string[]? tail)
	{
		return tail is null || tail.Length == 0 ? head : head + "." + string.Join(".", tail);
	}

	/// <summary>A dotted name within a namespace as a type: none, one, or the two it is ambiguous between.</summary>
	/// <remarks>
	/// <para>
	/// The longest part of the name that is a type by its full name, and the rest as types
	/// nested in it one at a time — so `Environment.SpecialFolder` is found through
	/// <c>Environment</c>, which metadata calls <c>System.Environment+SpecialFolder</c> and no
	/// full name written with dots would reach.
	/// </para>
	/// <para>
	/// The calling assembly is asked first, and its internal types answer as well as its
	/// public ones: a type the calling code declares stands in front of one of the same full
	/// name elsewhere, as a type in C#'s own compilation does (CS0436, which C# only warns
	/// about and this, having no warnings, says nothing about).
	/// </para>
	/// <para>
	/// Two answers are two referenced assemblies declaring that full name as two different
	/// types, which C# refuses (CS0433). Where the ambiguous type is the head of a longer name,
	/// the two answers are that head and not what is nested in it, since that is the type C#
	/// names — and they stand only where either holds the rest, so `N.T.Value` over a property
	/// is still no type and `N.T` is where the refusal is said. The array is kept and shared;
	/// nothing may write to it.
	/// </para>
	/// </remarks>
	static Type[] Qualified(string? space, string dotted, ResolutionScope scope)
	{
		// Kept, because every import is asked about every name a text writes and the answer cannot
		// change under a scope. The key is the three parts and not the name they make, so a question
		// that has been asked costs a lookup and builds nothing; the walk below is what builds.
		return Cached(
			_qualified, _qualifiedAbsent, (space, dotted, scope),
			static key => Walked(key.Item1, key.Item2, key.Item3), Nothing);
	}

	/// <summary>The same question, worked out: the longest prefix that is a type, then what is nested.</summary>
	static Type[] Walked(string? space, string dotted, ResolutionScope scope)
	{
		var end = dotted.Length;

		while (true)
		{
			var head  = dotted.Substring(0, end);
			var full  = space is null ? head : space + "." + head;
			var found = Loaded.Inside(scope, full) is { } own ? [own] : Loaded.Find(scope, full);

			if (found.Length > 0)
			{
				if (end == dotted.Length)
					return found;

				var inner = Inward(found[0], dotted, end, scope);

				if (found.Length == 1)
					return inner is null ? [] : [inner];

				return inner is not null || Inward(found[1], dotted, end, scope) is not null ? found : [];
			}

			end = dotted.LastIndexOf('.', end - 1);

			if (end < 0)
				return [];
		}
	}

	/// <summary>The rest of a dotted name after <paramref name="end"/>, as types nested in that one.</summary>
	static Type? Inward(Type outer, string dotted, int end, ResolutionScope scope)
	{
		var type = (Type?)outer;

		for (var at = end; type is not null && at < dotted.Length;)
		{
			var next = dotted.IndexOf('.', at + 1);

			if (next < 0)
				next = dotted.Length;

			type = Nested(type, dotted.Substring(at + 1, next - at - 1), scope);
			at   = next;
		}

		return type;
	}

	/// <summary>A type as C# writes it in a message: <c>N.Outer.G&lt;X&gt;</c>, not <c>N.Outer+G`1[X]</c>.</summary>
	internal static string Spelled(Type type)
	{
		return type.IsGenericParameter ? type.Name : Spelled(type, type.GetGenericArguments());
	}

	/// <summary>
	/// The same, where <paramref name="arguments"/> are the innermost type's: metadata puts every
	/// argument of every enclosing generic type on the nested one, and each level shows its own.
	/// </summary>
	static string Spelled(Type type, Type[] arguments)
	{
		var outer = type.DeclaringType;
		var from  = outer is null ? 0 : outer.GetGenericArguments().Length;
		var to    = type.GetGenericArguments().Length;
		var tick  = type.Name.IndexOf('`');
		var text  = outer is not null ? Spelled(outer, arguments) + "." :
			type.Namespace is { } space ? space + "." : "";

		text += tick < 0 ? type.Name : type.Name.Substring(0, tick);

		if (to > from)
			text += "<" + string.Join(", ", arguments.Skip(from).Take(to - from).Select(Spelled)) + ">";

		return text;
	}

	/// <summary>A type nested in another by that name, where C# in the calling assembly could name it.</summary>
	/// <remarks>
	/// Public, or — inside a type the calling assembly declares — internal or protected
	/// internal. Never private or protected alone: nothing here is written inside the type
	/// that holds it, or one derived from it.
	/// </remarks>
	static Type? Nested(Type outer, string name, ResolutionScope scope)
	{
		return outer.GetNestedType(name, BindingFlags.Public | BindingFlags.NonPublic) is { } nested &&
		(nested.IsNestedPublic || scope.Declares(outer.Assembly) && (nested.IsNestedAssembly || nested.IsNestedFamORAssem))
			? nested
			: null;
	}

	/// <summary>What the assemblies loaded into this process say about names.</summary>
	/// <remarks>
	/// <para>
	/// Shared, because it is the same for every reading — unlike the `using`s, which belong
	/// to one. Public types only: a type another assembly keeps internal is not one C# written
	/// outside it can name, and neither is it here.
	/// </para>
	/// <para>
	/// Both answers are kept once found, a type's absence included: a name is asked about far
	/// more often than it names anything, since every `s.Length` asks whether `s` is a type.
	/// An assembly loaded later may make an absent name present, so a load forgets what was
	/// kept — which is also why this is a class of its own, whose static constructor is the
	/// one place the subscription is made exactly once.
	/// </para>
	/// </remarks>
	static class Loaded
	{
		static readonly ConcurrentDictionary<(ResolutionScope, string), Type[]> _types = new();

		/// <summary>The same, for the names of that shape that are not there.</summary>
		static readonly ConcurrentDictionary<(ResolutionScope, string), Type[]> _typesAbsent = new();

		static readonly ConcurrentDictionary<ResolutionScope, HashSet<string>> _namespaces = new();

		// There is no static constructor here any more, and its absence is the point. It used to
		// subscribe to `AssemblyLoad` and clear these caches on every load, because what they
		// held depended on what the process had loaded and a new assembly could change every
		// answer. A scope's assemblies are fixed when the scope is made, so nothing loading
		// later can change what it answers — and a cache keyed by the scope therefore never has
		// to be thrown away.

		static readonly ConcurrentDictionary<(ResolutionScope, string), Type[]> _holders = new();

		/// <summary>The same, for the names of that shape that are not there.</summary>
		static readonly ConcurrentDictionary<(ResolutionScope, string), Type[]> _holdersAbsent = new();

		static readonly ConcurrentDictionary<(ResolutionScope, string), Type[]> _holdersInside = new();

		/// <summary>The public static classes standing in that namespace, in any loaded assembly.</summary>
		/// <remarks>
		/// What an extension method is written in, and the only thing worth walking a namespace
		/// for: a class that is not static holds none, and C# looks for one nowhere else.
		/// </remarks>
		public static Type[] Holders(ResolutionScope scope, string @namespace)
		{
			return Cached(
				_holders, _holdersAbsent, (scope, @namespace), static key => Held(key.Item2, key.Item1), Nothing);
		}

		/// <summary>The same in the calling assembly, where an internal class is nameable too.</summary>
		public static Type[] HoldersInside(ResolutionScope scope, string @namespace)
		{
			return _holdersInside.GetOrAdd(
				(scope, @namespace),
				static key =>
				{
					var holders = new List<Type>();

					foreach (var type in key.Item1.SeesInternals ? Declared(key.Item1.Caller) : [])
						if (Nameable(type) && Holds(type, key.Item2))
							holders.Add(type);

					return [.. holders];
				});
		}

		static Type[] Held(string @namespace, ResolutionScope scope)
		{
			var holders = new List<Type>();

			foreach (var assembly in scope.Assemblies)
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
		static bool Holds(Type type, string @namespace)
		{
			return type is { IsAbstract: true, IsSealed: true, IsNested: false, IsGenericTypeDefinition: false } &&
			string.Equals(type.Namespace, @namespace, StringComparison.Ordinal) &&
			type.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false);
		}

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

		/// <summary>
		/// The public type that full name means in the scope's assemblies; none; or, where two of
		/// them declare it as two different types, both.
		/// </summary>
		public static Type[] Find(ResolutionScope scope, string fullName)
		{
			return Cached(
				_types, _typesAbsent, (scope, fullName), static key => Search(key.Item2, key.Item1), Nothing);
		}

		/// <summary>Whether a loaded assembly has a public type in that namespace, or in one inside it.</summary>
		/// <remarks>
		/// Inside it too, because C# takes `using System.Collections;` whether or not that
		/// namespace declares a type of its own: it is there because something is in it.
		/// </remarks>
		public static bool Has(ResolutionScope scope, string @namespace)
		{
			return _namespaces.GetOrAdd(scope, static one => Gather(one)).Contains(@namespace);
		}

		static readonly ConcurrentDictionary<(ResolutionScope, string), Type?> _inside = new();

		/// <summary>The same, for the names of that shape that are not there.</summary>
		static readonly ConcurrentDictionary<(ResolutionScope, string), Type?> _insideAbsent = new();

		static readonly ConcurrentDictionary<ResolutionScope, HashSet<string>> _insideNamespaces = new();

		/// <summary>
		/// The type that full name means in the calling assembly — an internal one as well —
		/// where that assembly's own code could name it; or null.
		/// </summary>
		/// <remarks>
		/// Kept apart from the rest and never forgotten: what one assembly declares does not
		/// change when another loads.
		/// </remarks>
		public static Type? Inside(ResolutionScope scope, string fullName)
		{
			return Cached(
				_inside,
				_insideAbsent,
				(scope, fullName),
				static key => key.Item1.SeesInternals && key.Item1.Caller.GetType(key.Item2, false, false) is { } type &&
					Nameable(type)
					? type
					: null,
				Nothing);
		}

		/// <summary>Whether the calling assembly declares a type in that namespace, or in one inside it.</summary>
		public static bool HasInside(ResolutionScope scope, string @namespace)
		{
			return scope.SeesInternals &&
				_insideNamespaces.GetOrAdd(scope, static one => Spaces(one.Caller)).Contains(@namespace);
		}

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

		/// <summary>What <see cref="Find"/> answers, worked out.</summary>
		/// <remarks>
		/// <para>
		/// Every assembly is asked, and not only until one answers: two references declaring one
		/// full name is C#'s CS0433, and the first found winning made the meaning of a text depend
		/// on the order the references were walked in. Asking the rest costs only the first lookup
		/// of a name that IS found — a name that is not has always asked every assembly — and the
		/// answer is kept like any other.
		/// </para>
		/// <para>
		/// Compared by identity, since one type may be answered by many assemblies: a facade
		/// (<c>System.Runtime</c>, <c>netstandard</c>) forwards <c>System.Object</c> to where it is
		/// declared, and each returns that one type. Two versions of one assembly side by side are
		/// two assemblies and two types, and ambiguous — which is C#'s answer where both are
		/// referenced and strongly named (CS0433; weakly named, it refuses the second reference
		/// itself, CS1704).
		/// </para>
		/// <para>
		/// The calling assembly, which the scope holds first, answers alone where it answers at
		/// all: its own type wins over a reference's, as C# prefers the compilation's (CS0436).
		/// </para>
		/// </remarks>
		static Type[] Search(string name, ResolutionScope scope)
		{
			var found = default(Type?);

			foreach (var assembly in scope.Assemblies)
			{
				if (assembly.IsDynamic || assembly.GetType(name, false, false) is not { IsVisible: true } type)
					continue;

				if (assembly == scope.Caller)
					return [type];

				if (found is null)
					found = type;
				else if (type != found)
					return [found, type];
			}

			return found is null ? [] : [found];
		}

		static HashSet<string> Gather(ResolutionScope scope)
		{
			var namespaces = new HashSet<string>(StringComparer.Ordinal);

			foreach (var assembly in scope.Assemblies)
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
