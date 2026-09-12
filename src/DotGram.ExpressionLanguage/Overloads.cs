using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DotGram.ExpressionLanguage;

// Which overload C# would choose, and the arguments as it takes them.
//
// `System.Linq.Expressions` chooses among overloads by a rule that is not C#'s, so the
// choosing is done here for all four — a method, a constructor, an indexer, a delegate
// — by C#'s rules over the conversions beside them.

public static partial class ExpressionParser
{
	// ── Calls: the overload C# would choose ─────────────────────────────────────
	//
	// `Expression.Call` takes a method by name and chooses among the overloads itself, and
	// chooses by a rule that is not C#'s: an argument has to be of its parameter's type or
	// assignable to it by reference, no overload is better than another, and a name matches
	// in any case. `Expression.New` and `Expression.Property` take the member and choose
	// nothing. So the choosing is done here, once, for all four — a method, a constructor,
	// an indexer, a delegate — by C#'s rules over the conversions below: which forms are
	// applicable, a `params` array written out one by one, a default for what is left out,
	// and then the one better than every other.

	/// <summary>A call on a value: the method C# would choose for these arguments.</summary>
	public static Expression Called(Expression target, string name, Expression[] arguments, Assembly caller)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		var chosen = Chose(
			Methods(target.Type, name, instance: true, arguments, caller), arguments,
			$"'{target.Type.Name}' has no method '{name}'");

		return Expression.Call(target, (MethodInfo)chosen.Member, chosen.Arguments);
	}

	/// <summary>A call on a type: the static method C# would choose for these arguments.</summary>
	public static Expression Called(Type type, string name, Expression[] arguments, Assembly caller)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		var chosen = Chose(
			Methods(type, name, instance: false, arguments, caller), arguments,
			$"'{type.Name}' has no method '{name}'");

		return Expression.Call((MethodInfo)chosen.Member, chosen.Arguments);
	}

	/// <summary>A delegate called, its arguments converted to what it takes.</summary>
	public static Expression Invoked(Expression target, Expression[] arguments)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		// What is not a delegate is the API's to refuse, in its own words.
		if (!typeof(Delegate).IsAssignableFrom(target.Type) || target.Type.GetMethod("Invoke") is null)
			return Expression.Invoke(target, arguments);

		return Expression.Invoke(target, Invoking(target, arguments).Arguments);
	}

	/// <summary>A delegate's <c>Invoke</c>, and the arguments it takes.</summary>
	/// <remarks>
	/// One candidate and nothing to choose between, so this asks whether it applies rather
	/// than which of several is better. No calling assembly comes into it: a delegate's
	/// <c>Invoke</c> is reachable wherever the delegate type itself is.
	/// </remarks>
	static Resolution Invoking(Expression target, Expression[] arguments)
	{
		var invoke = target.Type.GetMethod("Invoke")!;

		var chosen = Applicable(invoke, invoke.GetParameters(), arguments) ?? throw new InvalidOperationException(
			$"'{target.Type.Name}' cannot be invoked with ({Listing(arguments)}).");

		return new Resolution(chosen.Member, Passed(chosen, arguments));
	}

	/// <summary>The constructor C# would choose for these arguments, called with them.</summary>
	/// <remarks>
	/// A value type's constructor of no arguments is not in its metadata at all, and
	/// <c>Expression.New</c> has a form for it that takes the type alone.
	/// </remarks>
	static NewExpression Constructed(Type type, Expression[] arguments, Assembly caller)
	{
		if (arguments.Length == 0 && type.IsValueType)
			return Expression.New(type);

		var chosen = Chose(Constructing(type, arguments, caller), arguments, $"'{type.Name}' has no constructor");

		return Expression.New((ConstructorInfo)chosen.Member, chosen.Arguments);
	}

	/// <summary>The constructors these arguments fit.</summary>
	static List<Candidate> Constructing(Type type, Expression[] arguments, Assembly caller)
	{
		var found = new List<Candidate>();

		foreach (var (constructor, parameters) in _constructors.GetOrAdd((type, caller), static key => Constructors(key)))
			if (Applicable(constructor, parameters, arguments) is { } candidate)
				found.Add(candidate);

		return found;
	}

	/// <summary>The candidate C# would call, with the arguments as it takes them.</summary>
	/// <remarks>
	/// The pair every caller wants: <see cref="Resolved"/> says which member, and
	/// <see cref="Passed"/> says what it is handed, and working the second out apart from the
	/// first is how two callers come to disagree about the same call.
	/// </remarks>
	static Resolution Chose(List<Candidate> found, Expression[] arguments, string missing)
	{
		var chosen = Resolved(found, arguments, missing);

		return new Resolution(chosen.Member, Passed(chosen, arguments));
	}

	/// <summary>One way a call could be read: the member, and the form its arguments take.</summary>
	/// <param name="Expanded">Whether a <c>params</c> array's elements were written one by one.</param>
	/// <param name="Defaults">How many optional parameters were left to their defaults.</param>
	readonly record struct Candidate(MemberInfo Member, ParameterInfo[] Parameters, bool Expanded, int Defaults)
	{
		/// <summary>The type the argument at that position is converted to.</summary>
		public Type At(int position) =>
			Expanded && position >= Parameters.Length - 1
				? Parameters[Parameters.Length - 1].ParameterType.GetElementType()!
				: Parameters[position].ParameterType;
	}

	/// <summary>The methods by that name the arguments fit, by the name exactly as written.</summary>
	/// <remarks>
	/// An interface's own methods do not include what it inherits, nor <c>object</c>'s, and a
	/// value of an interface type has both — `list.Contains(1)` on an <c>IList&lt;int&gt;</c>
	/// is <c>ICollection&lt;T&gt;</c>'s. A generic method is no candidate: nothing here can
	/// name its type arguments, and nothing infers them.
	/// </remarks>
	static List<Candidate> Methods(Type type, string name, bool instance, Expression[] arguments, Assembly caller)
	{
		var found = new List<Candidate>();

		foreach (var (method, parameters) in Cached(_methods, (type, name, instance, caller), static key => Named(key)))
			if (Fitting(method, parameters, arguments) is { } candidate)
				found.Add(candidate);

		return found;
	}

	/// <summary>The extension methods by that name the arguments fit, through the text's `using`s.</summary>
	/// <remarks>
	/// An extension method is a static method whose first parameter is the receiver, so the
	/// candidates are gathered over the arguments with the receiver written in front of them
	/// and nothing else here has to know the difference. A generic one is no candidate, for
	/// the reason <see cref="Methods"/> gives: nothing infers its type arguments yet, which is
	/// what keeps `Where` and `Select` out until they can be inferred.
	/// </remarks>
	static List<Candidate> Extensions(
		string name, Expression[] extended, Assembly caller, IReadOnlyList<string>? imports)
	{
		var found = new List<Candidate>();
		var seen  = new HashSet<MethodInfo>();

		foreach (var space in imports ?? [])
		{
			Consider(Loaded.Holders(space));
			Consider(Loaded.HoldersInside(caller, space));
		}

		return found;

		// A class the calling assembly declares publicly stands in both lists, and the same
		// method twice is two candidates neither of which is better than the other.
		void Consider(Type[] holders)
		{
			foreach (var holder in holders)
				foreach (var method in holder.GetMethods(
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
					if ((!method.ContainsGenericParameters || method.IsGenericMethodDefinition) &&
						string.Equals(method.Name, name, StringComparison.Ordinal) &&
						Reachable(method, caller) &&
						method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false) &&
						method.GetParameters() is { Length: > 0 } parameters &&
						seen.Add(method) &&
						Fitting(method, parameters, extended) is { } candidate)
						found.Add(candidate);
		}
	}

	/// <summary>
	/// The methods a type has by that name that the calling assembly could reach, with their
	/// parameters, before any argument is asked.
	/// </summary>
	static (MemberInfo, ParameterInfo[])[] Named((Type Type, string Name, bool Instance, Assembly Caller) key)
	{
		const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic;

		var named = new List<(MemberInfo, ParameterInfo[])>();

		Consider(key.Type.GetMethods(
			key.Instance
				? Any | BindingFlags.Instance
				: Any | BindingFlags.Static | BindingFlags.FlattenHierarchy));

		if (key.Instance && key.Type.IsInterface)
		{
			foreach (var inherited in key.Type.GetInterfaces())
				Consider(inherited.GetMethods(Any | BindingFlags.Instance));

			Consider(typeof(object).GetMethods(BindingFlags.Public | BindingFlags.Instance));
		}

		return [.. named];

		void Consider(MethodInfo[] methods)
		{
			// A generic method is kept as the definition it is: what its type arguments are is
			// a question about the arguments, which are not here yet. Anything else still
			// carrying type parameters is nobody's candidate.
			foreach (var method in methods)
				if (string.Equals(method.Name, key.Name, StringComparison.Ordinal) &&
					(!method.ContainsGenericParameters || method.IsGenericMethodDefinition) &&
					Reachable(method, key.Caller))
					named.Add((method, method.GetParameters()));
		}
	}

	/// <summary>A type's constructors the calling assembly could reach, with their parameters.</summary>
	static (MemberInfo, ParameterInfo[])[] Constructors((Type Type, Assembly Caller) key) =>
	[
		.. key.Type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(one => Reachable(one, key.Caller))
			.Select(static one => ((MemberInfo)one, one.GetParameters())),
	];
	/// <summary>
	/// The indexers the arguments fit and the calling assembly could reach, an interface's
	/// inherited ones among them.
	/// </summary>
	/// <remarks>
	/// An indexer is the property a type's <c>DefaultMemberAttribute</c> names — `Item`
	/// usually, `Chars` for a string — and it is looked for by that name among every property
	/// rather than through <c>GetDefaultMembers</c>, which answers with public ones only.
	/// </remarks>
	static List<Candidate> Indexers(Type type, Expression[] arguments, Assembly caller)
	{
		var found = new List<Candidate>();

		foreach (var (indexer, parameters) in _indexers.GetOrAdd((type, caller), static key => Indexing(key)))
			if (Applicable(indexer, parameters, arguments) is { } candidate)
				found.Add(candidate);

		return found;
	}

	/// <summary>A type's indexers the caller could reach, with their parameters, before any argument is asked.</summary>
	static (MemberInfo, ParameterInfo[])[] Indexing((Type Type, Assembly Caller) key)
	{
		var indexers = new List<(MemberInfo, ParameterInfo[])>();

		Consider(key.Type);

		if (key.Type.IsInterface)
			foreach (var inherited in key.Type.GetInterfaces())
				Consider(inherited);

		return [.. indexers];

		void Consider(Type declaring)
		{
			if (declaring.GetCustomAttribute<DefaultMemberAttribute>(true)?.MemberName is not { } name)
				return;

			foreach (var indexer in declaring.GetProperties(
				BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				if (string.Equals(indexer.Name, name, StringComparison.Ordinal) && Reachable(indexer, key.Caller) &&
					indexer.GetIndexParameters() is { Length: > 0 } parameters)
					indexers.Add((indexer, parameters));
		}
	}
	/// <summary>Whether the arguments fit, and in which form (C#'s applicable function member).</summary>
	/// <remarks>
	/// The normal form first — an argument for each parameter, and a default for each one
	/// after — and then, where the last parameter is a <c>params</c> array, the expanded one,
	/// with that array's elements written one by one. A parameter taken by reference, or of a
	/// type an expression tree cannot hold, makes the member no candidate at all: chosen, it
	/// would build a tree that does not compile, where the overload beside it would have.
	/// </remarks>
	static Candidate? Applicable(MemberInfo member, ParameterInfo[] parameters, Expression[] arguments)
	{
		foreach (var parameter in parameters)
			if (parameter.ParameterType.IsByRef || Unrepresentable(parameter.ParameterType))
				return null;

		var count = parameters.Length;

		if (arguments.Length <= count)
		{
			var fits = true;

			for (var at = 0; at < arguments.Length && fits; at++)
				fits = Converts(arguments[at], parameters[at].ParameterType);

			for (var at = arguments.Length; at < count && fits; at++)
				fits = parameters[at].IsOptional;

			if (fits)
				return new Candidate(member, parameters, false, count - arguments.Length);
		}

		if (count > 0 && arguments.Length >= count - 1 &&
			parameters[count - 1].IsDefined(typeof(ParamArrayAttribute), false))
		{
			var element = parameters[count - 1].ParameterType.GetElementType()!;
			var fits    = true;

			for (var at = 0; at < arguments.Length && fits; at++)
				fits = Converts(arguments[at], at < count - 1 ? parameters[at].ParameterType : element);

			if (fits)
				return new Candidate(member, parameters, true, 0);
		}

		return null;
	}

	/// <summary>A ref struct, which an expression tree cannot hold — <c>Span&lt;T&gt;</c> and its kind.</summary>
	static bool Unrepresentable(Type type) =>
#if NETSTANDARD2_0
		type.IsValueType && type.GetCustomAttributesData().Any(
			static attribute => attribute.AttributeType.FullName == "System.Runtime.CompilerServices.IsByRefLikeAttribute");
#else
		type.IsByRefLike;
#endif

	/// <summary>The one candidate better than every other, which is the one C# calls.</summary>
	/// <remarks>
	/// Methods a more derived type declared stand in front of its base types' first, as C#
	/// has them (§12.8.10.2): an override is found once, and a method hidden by `new` is not
	/// found beside the one hiding it.
	/// </remarks>
	/// <exception cref="InvalidOperationException">No candidate, or two that neither is better than.</exception>
	static Candidate Resolved(List<Candidate> found, Expression[] arguments, string missing)
	{
		var standing = found.FindAll(one => !found.Exists(other =>
			other.Member.DeclaringType != one.Member.DeclaringType &&
			one.Member.DeclaringType!.IsAssignableFrom(other.Member.DeclaringType)));

		foreach (var one in standing)
			if (standing.TrueForAll(other => other.Equals(one) || Compared(one, other, arguments) > 0))
				return one;

		if (standing.Count == 0)
			throw new InvalidOperationException($"{missing} taking ({Listing(arguments)}).");

		throw new InvalidOperationException(
			$"The call is ambiguous between '{standing[0].Member}' and '{standing[1].Member}'.");
	}

	/// <summary>Which of two candidates is the better function member, as C# decides it.</summary>
	/// <returns>Above zero where the first is, below where the second is, and zero where neither.</returns>
	static int Compared(Candidate first, Candidate second, Expression[] arguments)
	{
		var firstBetter  = false;
		var secondBetter = false;

		for (var at = 0; at < arguments.Length; at++)
		{
			var better = Better(arguments[at], first.At(at), second.At(at));

			firstBetter  |= better > 0;
			secondBetter |= better < 0;
		}

		if (firstBetter != secondBetter)
			return firstBetter ? 1 : -1;

		if (firstBetter)
			return 0;

		// Every argument converted equally well: the tie-breakers. A method that needed no
		// inference over one that did (§12.6.4.3), the normal form over the expanded one, and
		// no defaults over some.
		if (Guessed(first.Member) != Guessed(second.Member))
			return Guessed(first.Member) ? -1 : 1;

		if (first.Expanded != second.Expanded)
			return first.Expanded ? -1 : 1;

		if (first.Defaults == 0 != (second.Defaults == 0))
			return first.Defaults == 0 ? 1 : -1;

		return 0;
	}

	/// <summary>Which of two conversions of one argument is better (C#'s better conversion).</summary>
	static int Better(Expression argument, Type first, Type second)
	{
		if (first == second)
			return 0;

		if (!ReferenceEquals(argument, Null))
		{
			if (argument.Type == first)
				return 1;

			if (argument.Type == second)
				return -1;
		}

		return BetterTarget(first, second);
	}

	/// <summary>
	/// Which of two types is the better one to convert to: the one that converts to the
	/// other and not back, and a signed type over an unsigned one where neither does.
	/// </summary>
	static int BetterTarget(Type first, Type second)
	{
		var down = Standard(first, second);
		var up   = Standard(second, first);

		if (down != up)
			return down ? 1 : -1;

		if (IsSigned(first) && IsUnsigned(second))
			return 1;

		if (IsSigned(second) && IsUnsigned(first))
			return -1;

		return 0;
	}

	static bool IsSigned(Type type) =>
		type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long);

	static bool IsUnsigned(Type type) =>
		type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);

	/// <summary>The arguments as the chosen candidate takes them.</summary>
	/// <remarks>
	/// Each converted to its parameter, a default for each optional parameter left out, and
	/// the tail of an expanded call gathered into the array the <c>params</c> parameter is.
	/// </remarks>
	static Expression[] Passed(Candidate chosen, Expression[] arguments)
	{
		var parameters = chosen.Parameters;
		var passed     = new Expression[parameters.Length];
		var fixedCount = chosen.Expanded ? parameters.Length - 1 : parameters.Length;

		for (var at = 0; at < fixedCount; at++)
			passed[at] = at < arguments.Length
				? Implicitly(arguments[at], parameters[at].ParameterType)!
				: Defaulted(parameters[at]);

		if (chosen.Expanded)
		{
			var element = parameters[fixedCount].ParameterType.GetElementType()!;
			var rest    = new Expression[arguments.Length - fixedCount];

			for (var at = 0; at < rest.Length; at++)
				rest[at] = Implicitly(arguments[fixedCount + at], element)!;

			passed[fixedCount] = Expression.NewArrayInit(element, rest);
		}

		return passed;
	}

	/// <summary>What an optional parameter left out is worth.</summary>
	/// <remarks>
	/// Metadata keeps an enum's default as its underlying number, so it is made the enum
	/// again; a default of <c>null</c> or <c>default</c> is the type's default.
	/// </remarks>
	static Expression Defaulted(ParameterInfo parameter)
	{
		var type = parameter.ParameterType;

		if (!parameter.HasDefaultValue || parameter.DefaultValue is not { } value)
			return Expression.Default(type);

		var underlying = Underlying(type);

		if (underlying.IsEnum && value.GetType() != underlying)
			value = Enum.ToObject(underlying, value);

		return Expression.Constant(value, type);
	}

	/// <summary>The types of some arguments, for a message.</summary>
	static string Listing(Expression[] arguments) => string.Join(", ", arguments.Select(Shown));

	/// <summary>An expression's type for a message, and the literal <c>null</c> as C# names it.</summary>
	static string Shown(Expression value) => ReferenceEquals(value, Null) ? "<null>" : value.Type.Name;
}
