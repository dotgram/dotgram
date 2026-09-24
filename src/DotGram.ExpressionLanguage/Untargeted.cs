using System;
using System.Linq.Expressions;

namespace DotGram.ExpressionLanguage;

// A switch expression whose arms meet in no type of their own, standing where a target type
// will say what it is.
//
// `tag switch { 1 => new Field<long>(…), 2 => new Field<decimal>(…), _ => null }` has no
// natural type: no arm converts to another. C# reads it anyway wherever the place it stands in
// says what is wanted — the return of the lambda, an argument, the variable assigned to — and
// converts every arm to that. So the switch arrives unbuilt, and builds itself once the target
// is known.
//
// It is an `Expression` for the reason [[Unbuilt]] is one: applicability, the weighing of one
// candidate against another and every conversion take expressions, and a second kind of operand
// beside them would mean a second path through each.

public static partial class ExpressionParser
{
	/// <summary>A switch expression waiting for the type its arms are to be converted to.</summary>
	/// <remarks>
	/// <para>
	/// It never reaches a tree. <see cref="Implicitly"/> builds it against whatever it is being
	/// converted to, so what anything outside these two files sees is an ordinary
	/// <c>SwitchExpression</c> — which matters, because a visitor written elsewhere would not
	/// know this node.
	/// </para>
	/// <para>
	/// <see cref="Type"/> has to answer something and the truth is that it has none, so it says
	/// <c>object</c>: the nearest thing to "some type, not settled". Everything that would be
	/// misled by that asks <see cref="Typed"/> first, exactly as it does for a lambda.
	/// </para>
	/// </remarks>
	internal sealed class Untargeted : Expression
	{
		public Untargeted(Expression value, Arm[] arms, Expression left, Expression right)
		{
			_value = value ?? throw new ArgumentNullException(nameof(value));
			_arms  = arms  ?? throw new ArgumentNullException(nameof(arms));
			_left  = left;
			_right = right;
		}

		readonly Expression _value;
		readonly Arm[]      _arms;
		readonly Expression _left;
		readonly Expression _right;

		public override ExpressionType NodeType => ExpressionType.Extension;

		public override Type Type => typeof(object);

		/// <summary>Never: what it reduces to depends on what it is being converted to.</summary>
		public override bool CanReduce => false;

		/// <summary>Whether every arm converts to that type, which is what makes it buildable there.</summary>
		public bool Builds(Type target)
		{
			if (target is null || target == typeof(void))
				return false;

			foreach (var arm in _arms)
				if (!Converts(arm.Body, target))
					return false;

			return true;
		}

		/// <summary>The switch this is, every arm converted to that type.</summary>
		public Expression Built(Type target)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			foreach (var arm in _arms)
				if (!Converts(arm.Body, target))
					throw new InvalidOperationException(
						$"A switch arm worth '{Shown(arm.Body)}' does not convert to '{target.Name}', " +
						"which is what the place this switch stands in asks for.");

			return Switched(_value, _arms, target);
		}

		/// <summary>What this would have been refused with, had nothing ever asked for a type.</summary>
		/// <remarks>
		/// Kept from where the arms were weighed, so that a text with no target is refused in
		/// the same words it was refused in before there was any target typing at all — naming
		/// the two arms that meet in nothing rather than the whole switch.
		/// </remarks>
		public InvalidOperationException Refusal()
		{
			return new InvalidOperationException(
				"Type of switch expression cannot be determined because there is no implicit " +
				$"conversion between '{Shown(_left)}' and '{Shown(_right)}'.");
		}

		/// <summary>Whether a tree still holds one, anywhere in it.</summary>
		internal static Untargeted? Remains(Expression tree)
		{
			var finder = new Finder();

			finder.Visit(tree);

			return finder.Found;
		}

		/// <summary>A walk that stops looking once it has seen one.</summary>
		/// <remarks>
		/// Its own <c>VisitExtension</c>, since the base one reduces the node to look inside it,
		/// and this one cannot be reduced.
		/// </remarks>
		sealed class Finder : ExpressionVisitor
		{
			public Untargeted? Found { get; private set; }

			public override Expression? Visit(Expression? node)
			{
				return Found is not null ? node : base.Visit(node);
			}

			protected override Expression VisitExtension(Expression node)
			{
				if (node is Untargeted untargeted)
				{
					Found = untargeted;

					return node;
				}

				return base.VisitExtension(node);
			}
		}
	}
}
