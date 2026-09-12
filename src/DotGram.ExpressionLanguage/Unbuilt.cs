using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DotGram.ExpressionLanguage;

// A lambda that cannot be built yet, standing where an argument stands.
//
// `n => n * 2` has no tree until `n` has a type, and `n`'s type is the parameter of whatever
// overload is chosen — which is chosen by looking at the arguments. The way out is for the
// argument to arrive unbuilt: this stands in the list, says how many parameters it has, and
// builds itself once the delegate it is being handed to is known.
//
// It is an `Expression` on purpose. Applicability, the weighing of one candidate against
// another and the inference of type arguments all take expressions, and a second kind of
// argument beside them would mean a second path through every one of those.

public static partial class ExpressionParser
{
	/// <summary>A lambda whose parameters have no types yet, and what it takes to build one.</summary>
	/// <remarks>
	/// <para>
	/// It never reaches a tree. <see cref="Passed"/> builds it against the parameter the
	/// chosen member declares, so what a caller is handed is an ordinary
	/// <c>LambdaExpression</c> — nothing outside this file ever sees the node, which matters
	/// because a visitor written elsewhere (a query provider's, say) would not know it.
	/// </para>
	/// <para>
	/// <see cref="Type"/> has to answer something and the truth is that it has none, so it
	/// says <c>Delegate</c>: the nearest thing to "a delegate, which one not yet settled".
	/// Everything here that would be misled by that asks <see cref="Typed"/> first.
	/// </para>
	/// </remarks>
	public sealed class Unbuilt : Expression
	{
		public Unbuilt(int arity, Func<Type[], LambdaExpression> build)
		{
			if (arity < 0)
				throw new ArgumentOutOfRangeException(nameof(arity), arity, "A lambda takes no fewer than none.");

			Arity  = arity;
			_build = build ?? throw new ArgumentNullException(nameof(build));
		}

		readonly Func<Type[], LambdaExpression> _build;

		readonly Dictionary<Type, LambdaExpression> _built = [];

		readonly List<(Type[] Types, LambdaExpression Made)> _natural = [];

		/// <summary>How many parameters it was written with.</summary>
		public int Arity { get; }

		public override ExpressionType NodeType => ExpressionType.Extension;

		public override Type Type => typeof(Delegate);

		/// <summary>Never: what it reduces to depends on what it is handed to.</summary>
		public override bool CanReduce => false;

		/// <summary>The lambda this is, built for that delegate type.</summary>
		/// <remarks>
		/// Kept once built, so that asking twice — a candidate weighed and then chosen — is
		/// one reading of the body and one set of parameters rather than two that look alike.
		/// </remarks>
		public LambdaExpression Built(Type delegated)
		{
			if (delegated is null)
				throw new ArgumentNullException(nameof(delegated));

			if (_built.TryGetValue(delegated, out var already))
				return already;

			if (Taken(delegated) is not { } types || types.Length != Arity)
				throw new InvalidOperationException(
					$"A lambda of {Arity} parameters cannot be built for '{delegated.Name}'.");

			var made = Built(types);

			// The body decides what it gives back, and the delegate may want that widened —
			// a body worth an `int` handed to a `Func<int, long>` is the delegate's to say.
			if (!delegated.IsAssignableFrom(made.Type))
				made = Expression.Lambda(delegated, made.Body, made.Parameters);

			return _built[delegated] = made;
		}

		/// <summary>The lambda this is, built with those parameter types, worth what its body is.</summary>
		/// <remarks>
		/// What the inference asks for: the result is exactly what the body turned out to be,
		/// which is how a `Select` learns its second type argument from a body nobody could
		/// read until its first was settled.
		/// </remarks>
		public LambdaExpression Built(Type[] types)
		{
			if (types is null)
				throw new ArgumentNullException(nameof(types));

			if (types.Length != Arity)
				throw new InvalidOperationException(
					$"A lambda of {Arity} parameters cannot be built with {types.Length}.");

			foreach (var (had, made) in _natural)
				if (Same(had, types))
					return made;

			var built = _build(types);

			_natural.Add((types, built));

			return built;
		}

		static bool Same(Type[] first, Type[] second)
		{
			for (var at = 0; at < first.Length; at++)
				if (first[at] != second[at])
					return false;

			return true;
		}

		/// <summary>The parameter types a delegate takes, or null where it is no delegate.</summary>
		internal static Type[]? Taken(Type delegated) =>
			typeof(Delegate).IsAssignableFrom(delegated) && delegated.GetMethod("Invoke") is { } invoke
				? Array.ConvertAll(invoke.GetParameters(), one => one.ParameterType)
				: null;
	}

	/// <summary>Whether an expression has a type of its own to be weighed by.</summary>
	/// <remarks>
	/// The literal <c>null</c> and a lambda not yet built are the two that have none: C# types
	/// the first by where it stands and the second by what it is handed to, so neither says
	/// anything about which overload is the better one until that is settled.
	/// </remarks>
	static bool Typed(Expression value) => !ReferenceEquals(value, Null) && value is not Unbuilt;
}
