using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace DotGram.ExpressionLanguage;

// Which conversion C# makes without being asked, and nothing that applies one.
//
// `System.Linq.Expressions` builds every conversion there is and decides none of them:
// `Expression.Add` over an `int` and a `double` is refused rather than widened, and
// `Expression.Convert` takes a widening, a boxing, a nullable and a user-defined
// `op_Implicit` alike. So the deciding is written here, from the C# specification, and
// every conversion it chooses is built by `Expression.Convert`.
//
// This is the classification alone. The factories that ask it — `Converted`, `Assigned`,
// `Against`, the operators and their promotion — stand beside the grammar that names
// them, because what they build is this API's business and this file's is not.

public static partial class ExpressionParser
{
	/// <summary>The implicit conversion C# makes from that value to that type, or null where it makes none.</summary>
	/// <remarks>
	/// An expression and not only a type, because two of C#'s conversions are about the value:
	/// the literal <c>null</c> converts to anything that can be null, and a constant converts
	/// to a narrower type it fits in — `byte b = 1` is an <c>int</c> that fits, and a
	/// literal 0 is any enum. A constant is converted by making the constant it becomes, so
	/// `1 + x` over a <c>double</c> reads the literal 1.0 rather than a conversion of 1.
	/// </remarks>
	static Expression? Implicitly(Expression value, Type to)
	{
		var from = value.Type;

		if (from == to)
			return value;

		if (ReferenceEquals(value, Null))
			return CanBeNull(to) ? Expression.Constant(null, to) : null;

		if (value is ConstantExpression { Value: { } constant })
		{
			if (Narrowed(constant, to) is { } narrowed)
				return Expression.Constant(narrowed, to);

			if (IsNumeric(from) && IsNumeric(Underlying(to)) && Standard(from, to))
				return Expression.Constant(Changed(constant, Underlying(to)), to);
		}

		if (Standard(from, to))
			return Expression.Convert(value, to);

		return UserDefined(from, to) is { } method ? Through(value, method, to) : null;
	}

	/// <summary>Whether <see cref="Implicitly"/> would find a conversion, asked without building one.</summary>
	/// <remarks>
	/// The question overload resolution and promotion ask of every candidate and every
	/// argument, most of them to be turned down — so it is answered without the nodes that
	/// only the chosen one needs.
	/// </remarks>
	static bool Converts(Expression value, Type to)
	{
		var from = value.Type;

		if (from == to)
			return true;

		if (ReferenceEquals(value, Null))
			return CanBeNull(to);

		// A lambda not yet built goes to any delegate that takes as many parameters as it was
		// written with. Whether its body fits is not a question yet — it has no body until
		// this is the delegate it is being built for.
		if (value is Unbuilt lambda)
			return Unbuilt.Taken(to) is { } types && types.Length == lambda.Arity;

		if (value is ConstantExpression { Value: { } constant } && Narrowed(constant, to) is not null)
			return true;

		return Converts(from, to);
	}

	/// <summary>Whether C# converts one type to another unasked, an operator the author wrote included.</summary>
	/// <remarks>
	/// What the form above falls back to once the value itself has had its say, and what
	/// <see cref="BetterTarget"/> asks of two candidate parameter types. It has to count a
	/// conversion of the author's own: §12.6.4.6 weighs one beside the conversions the
	/// language has, and `Money` beats `decimal` where `Money` is what declares the operator
	/// between them — asked of the C# compiler itself, which is what chooses there.
	/// </remarks>
	static bool Converts(Type from, Type to) =>
		Standard(from, to) || UserDefined(from, to) is not null;

	/// <summary>
	/// A standard implicit conversion between two types: identity, numeric widening,
	/// nullable, reference and boxing — everything C# converts without an operator.
	/// </summary>
	static bool Standard(Type from, Type to)
	{
		if (from == to)
			return true;

		if (Nullable.GetUnderlyingType(to) is { } target)
		{
			var source = Underlying(from);

			return source == target || Widens(source, target);
		}

		if (Nullable.GetUnderlyingType(from) is null && Widens(from, to))
			return true;

		return !to.IsValueType && to.IsAssignableFrom(from);
	}

	/// <summary>C#'s implicit numeric conversions, which only ever widen.</summary>
	static bool Widens(Type from, Type to)
	{
		if (from.IsEnum || to.IsEnum)
			return false;

		return (Type.GetTypeCode(from), Type.GetTypeCode(to)) switch
		{
			(TypeCode.SByte,
			 TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.Byte,
			 TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or
			 TypeCode.Int64 or TypeCode.UInt64 or
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.Int16,
			 TypeCode.Int32 or TypeCode.Int64 or
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.UInt16,
			 TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.Char,
			 TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.Int32,
			 TypeCode.Int64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal)  => true,
			(TypeCode.UInt32,
			 TypeCode.Int64 or TypeCode.UInt64 or
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.Int64 or TypeCode.UInt64,
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.Single, TypeCode.Double)                                           => true,
			_                                                                            => false,
		};
	}

	/// <summary>A constant in a narrower type it fits in, or null where C# converts it to none.</summary>
	/// <remarks>
	/// An <c>int</c> to any integral type that holds it, a <c>long</c> to a <c>ulong</c> where
	/// it is not negative, and a zero of any integral type to any enum.
	/// </remarks>
	static object? Narrowed(object constant, Type to)
	{
		var target = Underlying(to);

		if (target.IsEnum)
			return constant is 0 or 0u or 0L or 0ul ? Enum.ToObject(target, 0) : null;

		return constant switch
		{
			int number => Type.GetTypeCode(target) switch
			{
				TypeCode.SByte  when number is >= sbyte.MinValue and <= sbyte.MaxValue  => (sbyte)number,
				TypeCode.Byte   when number is >= byte.MinValue and <= byte.MaxValue    => (byte)number,
				TypeCode.Int16  when number is >= short.MinValue and <= short.MaxValue  => (short)number,
				TypeCode.UInt16 when number is >= ushort.MinValue and <= ushort.MaxValue => (ushort)number,
				TypeCode.UInt32 when number >= 0                                        => (uint)number,
				TypeCode.UInt64 when number >= 0                                        => (ulong)number,
				_                                                                       => null,
			},
			long number when number >= 0 && target == typeof(ulong) => (ulong)number,
			_                                                         => null,
		};
	}

	/// <summary>A numeric constant as the number of a wider type.</summary>
	/// <remarks>A <c>char</c> goes through its code, which is all it converts as.</remarks>
	static object Changed(object constant, Type to) =>
		Convert.ChangeType(constant is char character ? (int)character : constant, to, CultureInfo.InvariantCulture);

	/// <summary>The user-defined implicit conversion from one type to another, or null.</summary>
	/// <remarks>
	/// Looked for where C# looks — the two types and their bases — among operators whose
	/// parameter the source reaches and whose result reaches the target by a standard
	/// conversion. One such is the answer; of several, the one that is exact at both ends,
	/// and where that is not one either, none: C# calls that ambiguous. Two predefined
	/// numeric types have no user-defined conversion between them even where one is written,
	/// as <c>decimal</c>'s are, and nothing converts to or from an interface this way.
	/// </remarks>
	static MethodInfo? UserDefined(Type from, Type to) =>
		IsNumeric(Underlying(from)) && IsNumeric(Underlying(to))
			? null
			: _operators.GetOrAdd((from, to), static pair => Declared(pair.Item1, pair.Item2));

	/// <summary>The search <see cref="UserDefined"/> makes, once for each pair of types.</summary>
	static MethodInfo? Declared(Type from, Type to)
	{
		var source = Underlying(from);
		var target = Underlying(to);

		if (source.IsInterface || target.IsInterface)
			return null;

		var found = new List<MethodInfo>();

		Consider(source);
		Consider(target);

		if (found.Count == 1)
			return found[0];

		var exact = found.FindAll(method => method.GetParameters()[0].ParameterType == from && method.ReturnType == to);

		return exact.Count == 1 ? exact[0] : null;

		void Consider(Type start)
		{
			for (var type = start; type is not null && type != typeof(object); type = type.BaseType)
				foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
					if (method.Name == "op_Implicit" && !found.Contains(method) &&
						method.GetParameters() is { Length: 1 } parameters &&
						Standard(from, parameters[0].ParameterType) && Standard(method.ReturnType, to))
						found.Add(method);
		}
	}

	/// <summary>The value through a user-defined operator, with a standard conversion either side.</summary>
	static Expression Through(Expression value, MethodInfo method, Type to)
	{
		var input  = Implicitly(value, method.GetParameters()[0].ParameterType)!;
		var result = Expression.Convert(input, method.ReturnType, method);

		return method.ReturnType == to ? result : Expression.Convert(result, to);
	}

	/// <summary>Whether a type is one of C#'s numeric types, <c>char</c> among them.</summary>
	static bool IsNumeric(Type type) =>
		!type.IsEnum && Type.GetTypeCode(type) is >= TypeCode.Char and <= TypeCode.Decimal;

	static Type Underlying(Type type) => Nullable.GetUnderlyingType(type) ?? type;

	static Type Lifted(Type type) => typeof(Nullable<>).MakeGenericType(type);

	static bool CanBeNull(Type type) => !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

	static bool IsLifted(Expression operand) =>
		ReferenceEquals(operand, Null) || Nullable.GetUnderlyingType(operand.Type) is not null;

}
