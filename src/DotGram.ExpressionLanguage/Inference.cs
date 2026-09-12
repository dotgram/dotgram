using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DotGram.ExpressionLanguage;

// What a generic method's type arguments are, read off the arguments it was handed.
// A layer of its own because it answers before applicability does: a definition with no
// answer is no candidate, and one with an answer is an ordinary method from there on.

public static partial class ExpressionParser
{
	// ── A generic method's type arguments, taken from what it is handed ─────────
	//
	// C# infers them in rounds, and the round this does not make is the one that types a
	// lambda from the method it is being passed to: that would need the argument to arrive
	// unbuilt, and construction runs children before parents. So a lambda says the types of
	// its parameters — `l.Where((int n) => n > 1)` — and arrives as a `Func<int, bool>`,
	// which is a type like any other and says everything the inference needs.

	/// <summary>The candidate these arguments make of a member, a generic method's inferred.</summary>
	static Candidate? Fitting(MemberInfo member, ParameterInfo[] parameters, Expression[] arguments)
	{
		if (member is not MethodInfo { IsGenericMethodDefinition: true } definition)
			return Applicable(member, parameters, arguments);

		return Inferred(definition, parameters, arguments) is { } made
			? Applicable(made, made.GetParameters(), arguments)
			: null;
	}

	/// <summary>A generic method with its type arguments read off the arguments it was given.</summary>
	/// <remarks>
	/// Inference proposes and <see cref="Applicable"/> disposes: a call the answer does not
	/// fit is refused where every other unfitting call is. A constraint the answer breaks is
	/// not a refusal either but an absence — <c>MakeGenericMethod</c> is what knows the
	/// constraints, and what it will not make is a method C# would not have found.
	/// </remarks>
	static MethodInfo? Inferred(MethodInfo definition, ParameterInfo[] parameters, Expression[] arguments)
	{
		var wanted = definition.GetGenericArguments();
		var bounds = new List<Type>?[wanted.Length];

		for (var at = 0; at < parameters.Length && at < arguments.Length; at++)
			// The literal `null` has no type of its own and so says nothing about anything.
			// C# infers nothing from it either, and refuses the call outright where it was
			// the only thing that could have said what a type parameter is (CS0411).
			if (!ReferenceEquals(arguments[at], Null))
				Bind(parameters[at].ParameterType, arguments[at].Type, bounds);

		var made = new Type[bounds.Length];

		for (var at = 0; at < bounds.Length; at++)
			if (Fixed(bounds[at]) is { } one)
				made[at] = one;
			else
				return null;

		try
		{
			return definition.MakeGenericMethod(made);
		}
		catch (ArgumentException)
		{
			return null;
		}
	}

	/// <summary>The one bound that every other bound reaches, which is what fixing is (§12.6.3).</summary>
	/// <remarks>
	/// A type parameter said twice is not the first answer but the one the others converge
	/// on: `Both(1, 2L)` is a <c>long</c>, because an <c>int</c> reaches one and a
	/// <c>long</c> does not reach an <c>int</c>. Where no bound is reached by all of them,
	/// or where none was gathered at all, there is nothing to fix it to and the method is no
	/// candidate — which is the refusal C# spells CS0411.
	/// </remarks>
	static Type? Fixed(List<Type>? bounds)
	{
		if (bounds is not { Count: > 0 })
			return null;

		if (bounds.Count == 1)
			return bounds[0];

		var found = default(Type);

		foreach (var candidate in bounds)
		{
			if (!bounds.TrueForAll(other => Converts(other, candidate)))
				continue;

			// Two that each reach the other is no answer either, and C# has none to give.
			if (found is not null)
				return null;

			found = candidate;
		}

		return found;
	}

	/// <summary>What a parameter's type says about the method's type parameters, given an argument's.</summary>
	/// <remarks>
	/// Structural, which is the half of C#'s inference that a built argument can answer:
	/// `IEnumerable&lt;TSource&gt;` against a `List&lt;int&gt;` finds the interface it
	/// implements and says <c>TSource</c> is <c>int</c>, and `Func&lt;TSource, TResult&gt;`
	/// against a `Func&lt;int, string&gt;` says both at once — which is how a `Select` learns
	/// what its lambda gives back.
	/// </remarks>
	static void Bind(Type parameter, Type argument, List<Type>?[] bounds)
	{
		if (parameter.IsGenericParameter)
		{
			// Every argument that speaks about it is gathered, and which of them the answer
			// is belongs to `Fixed`: taking the first would make the answer depend on the
			// order the arguments were written in.
			if (parameter.DeclaringMethod is not null)
			{
				var gathered = bounds[parameter.GenericParameterPosition] ??= [];

				if (!gathered.Contains(argument))
					gathered.Add(argument);
			}

			return;
		}

		if (parameter.IsArray)
		{
			if (argument.IsArray)
				Bind(parameter.GetElementType()!, argument.GetElementType()!, bounds);

			return;
		}

		if (!parameter.IsGenericType)
			return;

		var definition = parameter.GetGenericTypeDefinition();
		var wanted     = parameter.GetGenericArguments();

		foreach (var each in Kinds(argument))
		{
			if (!each.IsGenericType || each.GetGenericTypeDefinition() != definition)
				continue;

			var had = each.GetGenericArguments();

			for (var at = 0; at < wanted.Length && at < had.Length; at++)
				Bind(wanted[at], had[at], bounds);

			return;
		}
	}

	/// <summary>A type, what it derives from and what it implements: where a match may be found.</summary>
	static IEnumerable<Type> Kinds(Type type)
	{
		for (var each = type; each is not null; each = each.BaseType)
			yield return each;

		foreach (var implemented in type.GetInterfaces())
			yield return implemented;
	}

	/// <summary>Whether a method's type arguments were inferred rather than written.</summary>
	static bool Guessed(MemberInfo member) => member is MethodInfo { IsGenericMethod: true };

}
