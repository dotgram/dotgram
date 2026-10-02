using System;
using System.Linq.Expressions;

namespace DotGram.ExpressionLanguage;

// `new(…)` with its type left out, standing where a target type will say what it constructs.
//
// C# reads `List<int> list = new();`, `Next = new() { … }` and `M(new(1))` by the place the
// `new` stands in — the variable's declared type, the member's, the parameter's — and there is a
// conversion from such a `new` to every type, whether or not that type can be made that way
// (§10.2.18). So it arrives unbuilt, as a switch with no natural type does, and builds itself
// once the target is known; where nothing ever converts it, it is refused, as C# refuses it.

public static partial class ExpressionParser
{
	/// <summary>A `new(…)` that leaves its type out, with its arguments and initializer.</summary>
	/// <remarks>
	/// It converts to every type, as C#'s does, so overload resolution weighs it by the targets
	/// alone, and `M(new())` over two unrelated parameter types is ambiguous there as here. What
	/// cannot be made that way — no constructor fits, an interface — is refused once built,
	/// which is where C# refuses it too. A nullable value type constructs what it holds.
	/// </remarks>
	internal sealed class Unmade : Targetless
	{
		public Unmade(Expression[] arguments, Setting[]? fields, Element[]? items, ResolutionScope scope)
		{
			_arguments = arguments ?? throw new ArgumentNullException(nameof(arguments));
			_fields    = fields;
			_items     = items;
			_scope     = scope ?? throw new ArgumentNullException(nameof(scope));
		}

		readonly Expression[]   _arguments;
		readonly Setting[]?     _fields;
		readonly Element[]?     _items;
		readonly ResolutionScope _scope;

		/// <summary>Every type: whether it can be made that way is asked once it is built.</summary>
		/// <remarks>
		/// Asked of Roslyn: `M(Box)` beside `M(params Box[])` is ambiguous for `M(new())` (CS0121),
		/// so even an array, which no `new()` can make, is a type it converts to there.
		/// </remarks>
		public override bool Builds(Type target)
		{
			return target is not null && target != typeof(void);
		}

		public override Expression Built(Type target)
		{
			if (target is null)
				throw new ArgumentNullException(nameof(target));

			var made = Nullable.GetUnderlyingType(target) ?? target;

			if (made.IsArray || made.IsInterface || made.IsAbstract || made.IsPointer || made.IsByRef)
				throw new InvalidOperationException($"The type '{target.Name}' may not be used as the target type of new().");

			var built = Made(made, _arguments, _fields, _items, _scope);

			return made == target ? built : Expression.Convert(built, target);
		}

		public override InvalidOperationException Refusal()
		{
			return new InvalidOperationException("There is no target type for 'new()'.");
		}
	}

	/// <summary>A `new(…)` with its type left out, waiting for the place it stands in.</summary>
	internal static Expression Targeted(Expression[] arguments, Setting[]? fields, Element[]? items, State context)
	{
		if (context is null)
			throw new ArgumentNullException(nameof(context));

		context.Waiting();

		return new Unmade(arguments, fields, items, context.Reach);
	}
}
