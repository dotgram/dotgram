using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DotGram.ExpressionLanguage;

// A tuple literal, which is a `ValueTuple<…>` and nothing else.
//
// `(a, b)` is told from a parenthesised expression by the comma alone, so the two stand as
// neighbouring alternatives and the parenthesis reads first: it is the common case, and the
// alternative that fails is the one re-read.
//
// Element NAMES are left out, deliberately. In C# they are metadata the compiler writes beside
// the type (`TupleElementNamesAttribute`) and erases from the value; a compiled expression tree
// carries the type and nothing beside it, so a name written here could be read by no consumer,
// no delegate and no visitor of the tree. Accepting one and dropping it would be a name that
// silently means nothing; accepting one and honouring it inside a single text would be a naming
// table private to this parser, which is a separate decision. So a name is refused, in a message
// that says what to write instead.

public static partial class ExpressionParser
{
	/// <summary>How many elements one <c>ValueTuple</c> holds before the rest nest in the eighth.</summary>
	const int Flat = 7;

	/// <summary>What stood in the brackets: an expression, a tuple, or a name that cannot be kept.</summary>
	/// <remarks>
	/// <para>
	/// One construction for the three forms because they are one way in the grammar, and they
	/// are one way for a reason that is measured rather than tidy: three ways each beginning
	/// <c>'(' &amp; Expression</c> read the whole of what follows three times over, so a
	/// parenthesis that is never closed cost ×3 for every one before it. Read once, the tail
	/// says which form it was.
	/// </para>
	/// <para>
	/// The order of the three questions is the order C# asks them in. A name is refused first,
	/// because a named tuple is a thing the author meant and got wrong, and saying so is worth
	/// more than whatever the elements would have been refused for. Then a single element is
	/// the expression itself, brackets and all — `(a)` is `a` and was never a tuple of one.
	/// </para>
	/// </remarks>
	internal static Expression Bracketed(Expression first, Expression[]? rest, string? named, string[]? later)
	{
		if (first is null)
			throw new ArgumentNullException(nameof(first));

		if ((named ?? (later is { Length: > 0 } written ? written[0] : null)) is { } name)
			return Unnamed(name);

		return rest is null || rest.Length == 0 ? first : Tupled(first, rest);
	}

	/// <summary>The tuple `(a, b, …)` those elements make.</summary>
	/// <remarks>
	/// <para>
	/// Typed as C# types one: from the elements themselves, each keeping the type it already
	/// has. A tuple is not target-typed here, so an element with no type of its own — the
	/// literal <c>null</c>, which C# types by where it stands — is refused rather than guessed
	/// at, and says so.
	/// </para>
	/// <para>
	/// Past seven elements the eighth is a tuple of the rest, which is what C# builds and what
	/// <c>ValueTuple</c> is shaped for. Nothing here caps the count: the nesting is recursive,
	/// so the limit is the notation's, not this method's.
	/// </para>
	/// </remarks>
	internal static Expression Tupled(Expression first, Expression[] rest)
	{
		if (first is null)
			throw new ArgumentNullException(nameof(first));

		var elements = new Expression[(rest?.Length ?? 0) + 1];

		elements[0] = first;
		rest?.CopyTo(elements, 1);

		for (var at = 0; at < elements.Length; at++)
			if (!Typed(elements[at]))
				throw new InvalidOperationException(
					$"The tuple's element {at + 1} has no type of its own, so the tuple has none. " +
					(ReferenceEquals(elements[at], Null)
						? "Write it with a type, as `(string)null` rather than `null`."
						: "A lambda has no type until it is handed to a delegate; name one it can be built for."));

		return Nested(elements, 0);
	}

	/// <summary>Never a tuple: an element was written with a name.</summary>
	/// <remarks>
	/// The whole of what the grammar's third way builds. It reads a named tuple to the end and
	/// then refuses it, so that what a text is told is about the name it wrote and not about the
	/// colon, or about a name nothing declares, which is what the two ways before it would have
	/// said of the same text.
	/// </remarks>
	internal static Expression Unnamed(string named)
	{
		throw new InvalidOperationException(
			$"A tuple element cannot be named here, so '{named}:' has nowhere to go. " +
			"In C# an element name is metadata beside the type and is erased from the value; " +
			"a compiled expression tree carries the type alone, so the name could be read by " +
			"nothing that runs. Write the elements in order and read them back as Item1, Item2 " +
			"and so on.");
	}

	/// <summary>The tuple of the elements from that one on, nesting past the seventh.</summary>
	static Expression Nested(Expression[] elements, int from)
	{
		var left   = elements.Length - from;
		var held   = left > Flat ? Flat : left;
		var made   = new Expression[held + (left > Flat ? 1 : 0)];

		for (var at = 0; at < held; at++)
			made[at] = elements[from + at];

		if (left > Flat)
			made[held] = Nested(elements, from + Flat);

		var types = new Type[made.Length];

		for (var at = 0; at < made.Length; at++)
			types[at] = made[at].Type;

		return Expression.New(Constructing(types), made);
	}

	/// <summary>The <c>ValueTuple</c> constructor taking exactly those types.</summary>
	/// <remarks>
	/// Asked of the open type by arity and then closed, rather than looked up by name: the
	/// eighth element is itself a tuple, and the constraint on <c>TRest</c> is what makes the
	/// closing fail loudly if this ever hands it something that is not one.
	/// </remarks>
	static ConstructorInfo Constructing(Type[] types)
	{
		var open = Type.GetType("System.ValueTuple`" + types.Length) ??
			throw new InvalidOperationException($"A tuple of {types.Length} elements has no type in this runtime.");

		return open.MakeGenericType(types).GetConstructor(types) ??
			throw new InvalidOperationException($"A tuple of {types.Length} elements has no constructor taking them.");
	}
}
