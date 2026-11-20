using System;
using System.Globalization;
using System.Linq.Expressions;

namespace DotGram.QueryLanguage;

/// <summary>The numbers of the language: how a literal is read, which types meet without loss, and widening.</summary>
static class Numbers
{
	/// <summary>
	/// A number as written: an <c>int</c> where it fits, then a <c>long</c>, then a <c>decimal</c>; with a point a
	/// <c>decimal</c>; with an exponent a <c>double</c>. Null where it is too large for any of them.
	/// </summary>
	public static object? Natural(string text)
	{
		if (text.IndexOf('e') >= 0 || text.IndexOf('E') >= 0)
			return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var real) && !double.IsInfinity(real) ? real : null;

		if (text.IndexOf('.') < 0)
		{
			if (int.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var small))
				return small;

			if (long.TryParse(text, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var large))
				return large;
		}

		return decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var exact) ? exact : null;
	}

	/// <summary>The number as a value of <paramref name="type"/>, where it is exactly one; null where it is not.</summary>
	public static object? Convert(string text, QlType type)
	{
		var natural = Natural(text);

		switch (type)
		{
			case QlType.Int:
				return natural is int ? natural : null;

			case QlType.Long:
				return natural switch { int small => (long)small, long large => large, _ => null };

			case QlType.Decimal:
				if (natural is double)
					return decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var exact) ? exact : null;

				return natural is null ? null : Widen(natural, QlType.Decimal);

			case QlType.Double:
				return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var real) && !double.IsInfinity(real) ? real : null;

			default:
				return null;
		}
	}

	public static QlType TypeOf(object number)
	{
		return number switch
		{
			int     => QlType.Int,
			long    => QlType.Long,
			decimal => QlType.Decimal,
			_       => QlType.Double,
		};
	}

	/// <summary>
	/// The type two numbers meet in without loss: <c>int</c> to <c>long</c> to <c>decimal</c>, and <c>int</c> to
	/// <c>double</c>. <c>long</c> and <c>double</c> do not meet (a <c>double</c> holds integers exactly only to
	/// 2^53), nor do <c>decimal</c> and <c>double</c>.
	/// </summary>
	public static QlType? Common(QlType left, QlType right)
	{
		if (left == right)
			return left;

		return (Rank(left), Rank(right)) switch
		{
			(0, 1) or (1, 0) => QlType.Long,
			(0, 2) or (2, 0) => QlType.Decimal,
			(1, 2) or (2, 1) => QlType.Decimal,
			(0, 3) or (3, 0) => QlType.Double,
			_                => null,
		};

		static int Rank(QlType type)
		{
			return type switch
			{
				QlType.Int     => 0,
				QlType.Long    => 1,
				QlType.Decimal => 2,
				_              => 3,
			};
		}
	}

	/// <summary>A number of one type as a number of a wider one.</summary>
	public static object Widen(object value, QlType type)
	{
		return type switch
		{
			QlType.Int     => value,
			QlType.Long    => value switch { int i => (long)i, _ => value },
			QlType.Decimal => value switch { int i => (decimal)i, long l => (decimal)l, _ => value },
			QlType.Double  => value switch { int i => (double)i, long l => (double)l, decimal d => (double)d, _ => value },
			_              => value,
		};
	}
}

/// <summary>
/// Computes a value that reads no row, as C# would and checked: an integer that overflows, and a division of
/// integers or decimals by zero, are errors of the text at the operator — not a wrapped number, and not an error
/// of the database later.
/// </summary>
static class Evaluator
{
	public static bool HasParameters(BoundExpr value)
	{
		if (value is BoundParameter)
			return true;

		foreach (var child in Reads.Children(value))
			if (HasParameters(child))
				return true;

		return false;
	}

	/// <summary>The value; <paramref name="values"/> holds the call's values by parameter index, or is null at binding.</summary>
	public static object? Evaluate(BoundExpr value, object?[]? values)
	{
		switch (value)
		{
			case BoundLiteral literal:
				return literal.Value;

			case BoundParameter parameter:
				return values![parameter.Parameter.Index];

			case BoundConvert convert:
				return Evaluate(convert.Operand, values) is { } operand ? Numbers.Widen(operand, convert.Type.Type) : null;

			case BoundUnary unary:
				return Negate(Evaluate(unary.Operand, values), unary.Position);

			case BoundBinary binary:
				return Arithmetic(binary.Kind, Evaluate(binary.Left, values), Evaluate(binary.Right, values), binary.Position);

			default:
				throw new InvalidOperationException(value.GetType().Name);
		}
	}

	static object? Negate(object? operand, int position)
	{
		try
		{
			return operand switch
			{
				null      => null,
				int i     => checked(-i),
				long l    => checked(-l),
				decimal d => -d,
				_         => -(double)operand,
			};
		}
		catch (OverflowException)
		{
			throw new QlException(QlErrorCode.Overflow, "The value overflows.", position);
		}
	}

	static object? Arithmetic(ExpressionType kind, object? left, object? right, int position)
	{
		if (left is null || right is null)
			return null;

		try
		{
			return (left, right) switch
			{
				(int a, int b)         => Integers(kind, a, b),
				(long a, long b)       => Longs(kind, a, b),
				(decimal a, decimal b) => Decimals(kind, a, b),
				_                      => Doubles(kind, (double)left, (double)right),
			};
		}
		catch (OverflowException)
		{
			throw new QlException(QlErrorCode.Overflow, "The value overflows.", position);
		}
		catch (DivideByZeroException)
		{
			throw new QlException(QlErrorCode.DivisionByZero, "Division by zero.", position);
		}
	}

	static object Integers(ExpressionType kind, int a, int b)
	{
		return kind switch
		{
			ExpressionType.AddChecked      => checked(a + b),
			ExpressionType.SubtractChecked => checked(a - b),
			ExpressionType.MultiplyChecked => checked(a * b),
			ExpressionType.Divide          => checked(a / b),
			_                              => a % b,
		};
	}

	static object Longs(ExpressionType kind, long a, long b)
	{
		return kind switch
		{
			ExpressionType.AddChecked      => checked(a + b),
			ExpressionType.SubtractChecked => checked(a - b),
			ExpressionType.MultiplyChecked => checked(a * b),
			ExpressionType.Divide          => checked(a / b),
			_                              => a % b,
		};
	}

	static object Decimals(ExpressionType kind, decimal a, decimal b)
	{
		return kind switch
		{
			ExpressionType.Add      => a + b,
			ExpressionType.Subtract => a - b,
			ExpressionType.Multiply => a * b,
			ExpressionType.Divide   => a / b,
			_                       => a % b,
		};
	}

	/// <summary>
	/// Doubles as the other kinds are computed: a division by zero and a result past the largest double are errors of
	/// the text, not an infinity or a NaN that the query would carry to the provider, which has no literal for them.
	/// </summary>
	static object Doubles(ExpressionType kind, double a, double b)
	{
		if (kind is ExpressionType.Divide or ExpressionType.Modulo && b == 0)
			throw new DivideByZeroException();

		var result = kind switch
		{
			ExpressionType.Add      => a + b,
			ExpressionType.Subtract => a - b,
			ExpressionType.Multiply => a * b,
			ExpressionType.Divide   => a / b,
			_                       => a % b,
		};

		if (double.IsInfinity(result) || double.IsNaN(result))
			throw new OverflowException();

		return result;
	}
}
