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
	internal static Expression Called(Expression target, string name, Expression[] arguments, Assembly caller)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		var chosen = Chose(
			Methods(target.Type, name, instance: true, arguments, caller), arguments,
			$"'{target.Type.Name}' has no method '{name}'");

		return Expression.Call(target, (MethodInfo)chosen.Member, chosen.Arguments);
	}

	/// <summary>A call on a type: the static method C# would choose for these arguments.</summary>
	internal static Expression Called(Type type, string name, Expression[] arguments, Assembly caller)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		var chosen = Chose(
			Methods(type, name, instance: false, arguments, caller), arguments,
			$"'{type.Name}' has no method '{name}'");

		return Expression.Call((MethodInfo)chosen.Member, chosen.Arguments);
	}

	/// <summary>A delegate called, its arguments converted to what it takes.</summary>
	internal static Expression Invoked(Expression target, Expression[] arguments)
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

		var chosen = Applicable(Overload.Of(invoke, invoke.GetParameters()), arguments) ??
			throw new InvalidOperationException(
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

		foreach (var one in _constructors.GetOrAdd((type, caller), static key => Constructors(key)))
			if (Applicable(one, arguments) is { } candidate)
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
	/// <param name="Member">The member the call would reach.</param>
	/// <param name="Parameters">What it takes, as metadata says.</param>
	/// <param name="Expanded">Whether a <c>params</c> array's elements were written one by one.</param>
	/// <param name="Defaults">How many optional parameters were left to their defaults.</param>
	readonly record struct Candidate(MemberInfo Member, ParameterInfo[] Parameters, bool Expanded, int Defaults)
	{
		/// <summary>The type the argument at that position is converted to.</summary>
		public Type At(int position)
		{
			return Expanded && position >= Parameters.Length - 1
				? Parameters[Parameters.Length - 1].ParameterType.GetElementType()!
				: Parameters[position].ParameterType;
		}
	}

	/// <summary>An overload before any argument is asked: its parameters, and what they already say.</summary>
	/// <param name="Member">The member this overload is.</param>
	/// <param name="Parameters">What it takes, as metadata says.</param>
	/// <param name="Params">Whether the last parameter is a <c>params</c> array.</param>
	/// <param name="Usable">Whether every parameter it takes can be written as an argument.</param>
	/// <remarks>
	/// Those two ride along because neither is an answer about a call. A member taking a
	/// parameter by reference is no candidate whatever it is handed — nothing in the language
	/// spells <c>ref</c> at a call — and a <c>params</c> array is one whatever it is handed, so
	/// both are worked out where the parameters are, once and for as long as they are kept, and
	/// the choosing below reads them instead of asking metadata again for every overload of
	/// every call. A ref struct is not among them: a compiled expression tree holds
	/// <c>ReadOnlySpan&lt;T&gt;</c> as a parameter, a local and a return, on .NET and on .NET
	/// Framework alike.
	/// </remarks>
	readonly record struct Overload(MemberInfo Member, ParameterInfo[] Parameters, bool Params, bool Usable)
	{
		/// <summary>An overload with what its parameters say already worked out.</summary>
		public static Overload Of(MemberInfo member, ParameterInfo[] parameters)
		{
			var usable = true;

			foreach (var parameter in parameters)
				if (parameter.ParameterType.IsByRef)
				{
					usable = false;
					break;
				}

			return new Overload(
				member, parameters,
				parameters.Length > 0 &&
					parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), false),
				usable);
		}
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

		foreach (var one in Cached(_methods, (type, name, instance, caller), static key => Named(key)))
			if (Fitting(one, arguments) is { } candidate)
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
				foreach (var one in Cached(_extensions, (holder, name, caller), static key => Extending(key)))
					if (seen.Add((MethodInfo)one.Member) && Fitting(one, extended) is { } candidate)
						found.Add(candidate);
		}
	}

	/// <summary>
	/// The extension methods by that name a static class holds that the calling assembly could
	/// reach, with their parameters, before any argument is asked.
	/// </summary>
	static Overload[] Extending((Type Holder, string Name, Assembly Caller) key)
	{
		var extending = new List<Overload>();

		foreach (var method in key.Holder.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
			if (string.Equals(method.Name, key.Name, StringComparison.Ordinal) &&
				(!method.ContainsGenericParameters || method.IsGenericMethodDefinition) &&
				Reachable(method, key.Caller) &&
				method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false) &&
				method.GetParameters() is { Length: > 0 } parameters)
				extending.Add(Overload.Of(method, parameters));

		return [.. extending];
	}

	/// <summary>
	/// The methods a type has by that name that the calling assembly could reach, with their
	/// parameters, before any argument is asked.
	/// </summary>
	static Overload[] Named((Type Type, string Name, bool Instance, Assembly Caller) key)
	{
		const BindingFlags Any = BindingFlags.Public | BindingFlags.NonPublic;

		var named = new List<Overload>();

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
					named.Add(Overload.Of(method, method.GetParameters()));
		}
	}

	/// <summary>A type's constructors the calling assembly could reach, with their parameters.</summary>
	static Overload[] Constructors((Type Type, Assembly Caller) key)
	{
		var constructors = new List<Overload>();

		foreach (var one in key.Type.GetConstructors(
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
			if (Reachable(one, key.Caller))
				constructors.Add(Overload.Of(one, one.GetParameters()));

		return [.. constructors];
	}

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

		foreach (var one in _indexers.GetOrAdd((type, caller), static key => Indexing(key)))
			if (Applicable(one, arguments) is { } candidate)
				found.Add(candidate);

		return found;
	}

	/// <summary>A type's indexers the caller could reach, with their parameters, before any argument is asked.</summary>
	static Overload[] Indexing((Type Type, Assembly Caller) key)
	{
		var indexers = new List<Overload>();

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
					indexers.Add(Overload.Of(indexer, parameters));
		}
	}

	/// <summary>Whether the arguments fit, and in which form (C#'s applicable function member).</summary>
	/// <remarks>
	/// The normal form first — an argument for each parameter, and a default for each one
	/// after — and then, where the last parameter is a <c>params</c> array, the expanded one,
	/// with that array's elements written one by one. A parameter taken by reference makes the
	/// member no candidate at all: chosen, it would build a tree that does not compile, where
	/// the overload beside it would have.
	/// </remarks>
	static Candidate? Applicable(Overload one, Expression[] arguments)
	{
		if (!one.Usable)
			return null;

		var parameters = one.Parameters;
		var count      = parameters.Length;

		if (arguments.Length <= count)
		{
			var fits = true;

			for (var at = 0; at < arguments.Length && fits; at++)
				fits = Converts(arguments[at], parameters[at].ParameterType);

			for (var at = arguments.Length; at < count && fits; at++)
				fits = parameters[at].IsOptional;

			if (fits)
				return new Candidate(one.Member, parameters, false, count - arguments.Length);
		}

		if (one.Params && arguments.Length >= count - 1)
		{
			var element = parameters[count - 1].ParameterType.GetElementType()!;
			var fits    = true;

			for (var at = 0; at < arguments.Length && fits; at++)
				fits = Converts(arguments[at], at < count - 1 ? parameters[at].ParameterType : element);

			if (fits)
				return new Candidate(one.Member, parameters, true, 0);
		}

		return null;
	}

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

		// And the one whose parameters were written more specifically, before any type argument
		// was put into them (§12.6.4.3): `Max(Func<TSource, double>)` over
		// `Max<TSource, TResult>(Func<TSource, TResult>)` where both became `Func<int, double>`.
		return MoreSpecific(first.Member, second.Member, arguments.Length);
	}

	/// <summary>Which of two members' written parameters are the more specific, over the arguments given.</summary>
	/// <returns>Above zero where the first's are, below where the second's are, and zero where neither.</returns>
	static int MoreSpecific(MemberInfo first, MemberInfo second, int count)
	{
		if (Written(first) is not { } mine || Written(second) is not { } theirs)
			return 0;

		var more = false;
		var less = false;

		for (var at = 0; at < count && at < mine.Length && at < theirs.Length; at++)
		{
			var specific = Specific(mine[at].ParameterType, theirs[at].ParameterType);

			more |= specific > 0;
			less |= specific < 0;
		}

		return more == less ? 0 : more ? 1 : -1;

		static ParameterInfo[]? Written(MemberInfo member)
		{
			return member switch
			{
				MethodInfo { IsGenericMethod: true } method => method.GetGenericMethodDefinition().GetParameters(),
				MethodBase method => method.GetParameters(),
				_ => null,
			};
		}
	}

	/// <summary>Whether one written type is more specific than another (§12.6.4.3).</summary>
	/// <remarks>
	/// A type parameter is less specific than anything that is not one; a constructed type is
	/// more specific where one of its type arguments is and none is less, and an array where its
	/// element type is.
	/// </remarks>
	static int Specific(Type first, Type second)
	{
		if (first.IsGenericParameter != second.IsGenericParameter)
			return first.IsGenericParameter ? -1 : 1;

		if (first.IsArray && second.IsArray && first.GetArrayRank() == second.GetArrayRank())
			return Specific(first.GetElementType()!, second.GetElementType()!);

		if (!first.IsGenericType || !second.IsGenericType ||
			first.GetGenericTypeDefinition() != second.GetGenericTypeDefinition())
		{
			return 0;
		}

		var mine   = first.GetGenericArguments();
		var theirs = second.GetGenericArguments();
		var more   = false;
		var less   = false;

		for (var at = 0; at < mine.Length; at++)
		{
			var specific = Specific(mine[at], theirs[at]);

			more |= specific > 0;
			less |= specific < 0;
		}

		return more == less ? 0 : more ? 1 : -1;
	}

	/// <summary>Which of two conversions of one argument is better (C#'s better conversion).</summary>
	static int Better(Expression argument, Type first, Type second)
	{
		if (first == second)
			return 0;

		// An argument with no type of its own matches neither exactly, so the targets are
		// weighed against each other alone — except a lambda, which has a body to be weighed by.
		if (Typed(argument))
		{
			if (argument.Type == first)
				return 1;

			if (argument.Type == second)
				return -1;
		}
		else if (argument is Unbuilt lambda)
		{
			return BetterReturn(lambda, first, second);
		}

		return BetterTarget(first, second);
	}

	/// <summary>Which of two delegates a lambda with no types is the better one to hand to (§12.6.4.5).</summary>
	/// <remarks>
	/// Only two that take the same parameters are compared this way, which is every pair that
	/// `Sum`, `Max` and their kin offer: the one whose return type the body is worth exactly,
	/// then one that gives something back over one that gives nothing, then the better of the
	/// two return types as targets. The body is built for those parameters to be asked — once,
	/// since a built lambda is kept.
	/// </remarks>
	static int BetterReturn(Unbuilt lambda, Type first, Type second)
	{
		if (Unbuilt.Taken(first) is not { } taken ||
			Unbuilt.Taken(second) is not { } other ||
			taken.Length != lambda.Arity ||
			!System.Linq.Enumerable.SequenceEqual(taken, other) ||
			Array.Exists(taken, static one => one.ContainsGenericParameters) ||
			Unbuilt.Returned(first) is not { } one ||
			Unbuilt.Returned(second) is not { } two ||
			one == two)
		{
			return 0;
		}

		var worth = lambda.Built(taken).ReturnType;

		if (worth == one)
			return 1;

		if (worth == two)
			return -1;

		if (two == typeof(void))
			return 1;

		if (one == typeof(void))
			return -1;

		return BetterTarget(one, two);
	}

	/// <summary>
	/// Which of two types is the better one to convert to: the one that converts to the
	/// other and not back, and a signed type over an unsigned one where neither does.
	/// </summary>
	/// <remarks>
	/// The converting asked about here is every implicit conversion C# has, an operator the
	/// author wrote among them — which is what tells `Given(Money)` from `Given(decimal)`
	/// when both are handed an `int`, and what the standard conversions alone called a tie.
	/// </remarks>
	static int BetterTarget(Type first, Type second)
	{
		var down = Converts(first, second);
		var up   = Converts(second, first);

		if (down != up)
			return down ? 1 : -1;

		if (IsSigned(first) && IsUnsigned(second))
			return 1;

		if (IsSigned(second) && IsUnsigned(first))
			return -1;

		return 0;
	}

	static bool IsSigned(Type type)
	{
		return type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long);
	}

	static bool IsUnsigned(Type type)
	{
		return type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);
	}

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
				? Given(arguments[at], parameters[at].ParameterType)
				: Defaulted(parameters[at]);

		if (chosen.Expanded)
		{
			var element = parameters[fixedCount].ParameterType.GetElementType()!;
			var rest    = new Expression[arguments.Length - fixedCount];

			for (var at = 0; at < rest.Length; at++)
				rest[at] = Given(arguments[fixedCount + at], element);

			passed[fixedCount] = Expression.NewArrayInit(element, rest);
		}

		return passed;
	}

	/// <summary>One argument as the parameter it goes to takes it.</summary>
	/// <remarks>
	/// This is where a lambda that had no types is built: the parameter says which delegate
	/// it is, and the delegate says what its parameters are. Nothing after this point has a
	/// node of this file's own in it.
	/// </remarks>
	static Expression Given(Expression argument, Type to)
	{
		return argument is Unbuilt lambda ? lambda.Built(to) : Implicitly(argument, to)!;
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
	static string Listing(Expression[] arguments)
	{
		return string.Join(", ", arguments.Select(Shown));
	}

	/// <summary>An expression's type for a message, and the literal <c>null</c> as C# names it.</summary>
	static string Shown(Expression value)
	{
		return ReferenceEquals(value, Null) ? "<null>" : value.Type.Name;
	}
}
