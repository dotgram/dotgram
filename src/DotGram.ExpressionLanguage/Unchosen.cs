using System;
using System.Linq.Expressions;

namespace DotGram.ExpressionLanguage;

// A `?:` whose branches meet in no type, standing where a target type will say what it is.
//
// `c ? 1 : "a"` and `c ? new() : null` have no type of their own, and C# reads them anyway
// wherever the place they stand in says what is wanted (§12.18, the target-typed conditional):
// each branch is converted to that. So the conditional arrives unbuilt, as a switch with no
// natural type does, and is refused in the words it always was where nothing types it.

public static partial class ExpressionParser
{
	/// <summary>A conditional waiting for the type its branches are to be converted to.</summary>
	internal sealed class Unchosen : Targetless
	{
		public Unchosen(Expression test, Expression then, Expression otherwise)
		{
			_test      = test      ?? throw new ArgumentNullException(nameof(test));
			_then      = then      ?? throw new ArgumentNullException(nameof(then));
			_otherwise = otherwise ?? throw new ArgumentNullException(nameof(otherwise));
		}

		readonly Expression _test;
		readonly Expression _then;
		readonly Expression _otherwise;

		/// <summary>Whether both branches convert to that type.</summary>
		public override bool Builds(Type target)
		{
			return target is not null && target != typeof(void) && Converts(_then, target) && Converts(_otherwise, target);
		}

		/// <summary>The conditional this is, both branches converted to that type.</summary>
		public override Expression Built(Type target)
		{
			if (!Builds(target))
				throw new InvalidOperationException(
					$"A branch of a conditional expression does not convert to '{target.Name}', which is what the place " +
					"it stands in asks for.");

			return Expression.Condition(_test, Implicitly(_then, target)!, Implicitly(_otherwise, target)!);
		}

		/// <summary>What this was refused with before a conditional could be typed by its place.</summary>
		public override InvalidOperationException Refusal()
		{
			return new InvalidOperationException(
				"Type of conditional expression cannot be determined because there is no implicit " +
				$"conversion between '{Shown(_then)}' and '{Shown(_otherwise)}'.");
		}
	}
}
