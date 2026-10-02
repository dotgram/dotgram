using System;
using System.Linq.Expressions;

namespace DotGram.ExpressionLanguage;

// A lambda written inside a text, whose body has no type of its own, standing where the delegate
// it is converted to will say what that body gives back.
//
// `Func<Box> f = () => c ? new() : new();` reads in C#: the lambda converts to the delegate, and
// its body to what the delegate returns, which types the body. A lambda here is built where it is
// read, its type from its body's; so where that body is waiting for a type, the lambda waits too.

public static partial class ExpressionParser
{
	/// <summary>A lambda whose body waits for the return type of the delegate it is converted to.</summary>
	internal sealed class Unreturned : Targetless
	{
		public Unreturned(Targetless body, ParameterExpression[] parameters)
		{
			_body       = body       ?? throw new ArgumentNullException(nameof(body));
			_parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
		}

		readonly Targetless            _body;
		readonly ParameterExpression[] _parameters;

		/// <summary>Whether that is a delegate taking these parameters whose return the body converts to.</summary>
		public override bool Builds(Type target)
		{
			return Returned(target) is { } returned && _body.Builds(returned);
		}

		public override Expression Built(Type target)
		{
			var returned = Returned(target) ?? throw new InvalidOperationException(
				$"The lambda converts to no '{target.Name}': it takes {_parameters.Length} parameter(s) of its own types.");

			return Expression.Lambda(target, _body.Built(returned), _parameters);
		}

		public override InvalidOperationException Refusal()
		{
			return _body.Refusal();
		}

		/// <summary>What that delegate returns, where it takes exactly these parameters and returns something.</summary>
		Type? Returned(Type target)
		{
			if (target is null || !typeof(Delegate).IsAssignableFrom(target) || target.GetMethod("Invoke") is not { } invoke)
				return null;

			var taken = invoke.GetParameters();

			if (taken.Length != _parameters.Length || invoke.ReturnType == typeof(void))
				return null;

			for (var at = 0; at < taken.Length; at++)
				if (taken[at].ParameterType != _parameters[at].Type)
					return null;

			return invoke.ReturnType;
		}
	}
}
