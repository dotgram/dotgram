using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

using DotGram.QueryLanguage.Syntax;

namespace DotGram.QueryLanguage;

// The values of a text: names, literals, parameters, operators, members and aggregates, each typed as C# would
// type it over the language's types, and refused where C# would not take it.
sealed partial class Binder
{
	/// <summary>
	/// Binds a value. <paramref name="expected"/> is the type the place gives a value that has none of its own — a
	/// parameter, <c>null</c>, a number — and <paramref name="nullable"/> whether a parameter there may be null.
	/// </summary>
	BoundExpr BindValue(ExprSyntax syntax, ScalarType? expected, bool nullable = true)
	{
		switch (syntax)
		{
			case ParenthesizedExpr parenthesized:
				return BindValue(parenthesized.Inner, expected, nullable);

			case NameExpr name:
				return ResolveName(name.Name);

			case LiteralExpr literal:
				return Literal(literal, expected);

			case TypedLiteralExpr typed:
				return TypedLiteral(typed);

			case ParameterExpr parameter:
				if (expected is { } type)
					return Parameter(parameter, type, nullable);

				if (_parameters.TryGetValue(parameter.Name, out var known) && known.Type is { } typed2 && !known.IsList)
					return Parameter(parameter, typed2, nullable);

				throw Error(QlErrorCode.Type, $"The type of `@{parameter.Name}` is not known here: compare it with a field.", parameter.Position);

			case MemberExpr member:
				return BindMember(member);

			case CallExpr call:
				if (Aggregates.Is(call.Name.Text))
					throw Error(QlErrorCode.Type, $"`{call.Name.Text}` is an aggregate: it stands on its own as a column of `select`.", call.Position);

				throw Error(QlErrorCode.UnknownName, $"`{call.Name.Text}` is not a function of the language.", call.Position);

			case UnaryExpr unary:
				return BindUnary(unary, expected);

			case BinaryExpr binary:
				return binary.IsArithmetic ? BindArithmetic(binary, expected) : BindComparison(binary.Left, binary.Operator, binary.Right, binary.Position);

			case LogicalExpr logical:
				return BindLogical(logical);

			case IsNullExpr isNull:
				return BindIsNull(isNull);

			case InExpr @in:
				return BindIn(@in);

			case BetweenExpr between:
				return BindBetween(between);

			case LikeExpr like:
				return BindLike(like);

			default:
				throw new InvalidOperationException(syntax.GetType().Name);
		}
	}

	/// <summary>A value that takes its type from the other side: a parameter, <c>null</c> or a number.</summary>
	static bool IsUntyped(ExprSyntax syntax)
	{
		return syntax switch
		{
			ParenthesizedExpr parenthesized                                             => IsUntyped(parenthesized.Inner),
			ParameterExpr                                                               => true,
			LiteralExpr { Kind: LiteralKind.Null or LiteralKind.Number }                => true,
			UnaryExpr { Operator: UnaryOperator.Negate, Operand: var operand }          => IsUntyped(operand),
			_                                                                           => false,
		};
	}

	/// <summary>Two operands: the one with a type of its own first, so that it types the other.</summary>
	(BoundExpr Left, BoundExpr Right) BindPair(ExprSyntax left, ExprSyntax right, bool nullable)
	{
		if (IsUntyped(left) && !IsUntyped(right))
		{
			var typed = BindValue(right, null, nullable);

			return (BindValue(left, typed.Type, nullable), typed);
		}

		var first = BindValue(left, null, nullable);

		return (first, BindValue(right, first.Type, nullable));
	}

	// ── Literals ────────────────────────────────────────────────────────────

	BoundExpr Literal(LiteralExpr literal, ScalarType? expected)
	{
		switch (literal.Kind)
		{
			case LiteralKind.True:
				return new BoundLiteral(true, ScalarType.Of(QlType.Bool, false), literal.Position);

			case LiteralKind.False:
				return new BoundLiteral(false, ScalarType.Of(QlType.Bool, false), literal.Position);

			case LiteralKind.Null:
				if (expected is not { } type)
					throw Error(QlErrorCode.Type, "`null` has no type here: compare a value with it, `x == null`.", literal.Position);

				return new BoundLiteral(null, type.WithNullable(true), literal.Position);

			case LiteralKind.String:
			{
				var value = Unescape(literal);

				if (value.Length > _options.MaxStringLength)
					throw Error(QlErrorCode.Limit, $"The string is longer than {_options.MaxStringLength} characters.", literal.Position);

				return new BoundLiteral(value, ScalarType.Of(QlType.String, false), literal.Position);
			}

			default:
				return Number(literal.Text, literal.Position, expected);
		}
	}

	/// <summary>A number: an <c>int</c>, <c>long</c> or <c>decimal</c> as written, a <c>double</c> with an exponent — or the type the other side has, where the number is exactly one of those.</summary>
	BoundExpr Number(string text, int position, ScalarType? expected)
	{
		if (expected is { IsNumeric: true } target && Numbers.Convert(text, target.Type) is { } adapted)
			return new BoundLiteral(adapted, target.WithNullable(false), position);

		var natural = Numbers.Natural(text) ?? throw Error(QlErrorCode.Overflow, $"`{text}` is too large for a number of the language.", position);

		return new BoundLiteral(natural, ScalarType.Of(Numbers.TypeOf(natural), false), position);
	}

	BoundExpr TypedLiteral(TypedLiteralExpr typed)
	{
		var text = Unescape(typed.Value);

		if (Names.Comparer.Equals(typed.Type.Text, "timestamp"))
		{
			if (!Coercion.TryTimestamp(text, out var value))
				throw Error(QlErrorCode.Type, $"`{text}` is not a timestamp: write `timestamp 'yyyy-MM-dd HH:mm:ss'`, without an offset.", typed.Value.Position);

			return new BoundLiteral(value, ScalarType.Of(QlType.Timestamp, false), typed.Position);
		}

		if (Names.Comparer.Equals(typed.Type.Text, "timestamptz"))
		{
			if (!Coercion.TryTimestampTz(text, out var value))
				throw Error(QlErrorCode.Type, $"`{text}` is not a timestamp with an offset: write `timestamptz 'yyyy-MM-dd HH:mm:ss+02:00'`.", typed.Value.Position);

			return new BoundLiteral(value, ScalarType.Of(QlType.TimestampTz, false), typed.Position);
		}

		throw Error(QlErrorCode.Type, $"`{typed.Type.Text}` does not type a literal: write `timestamp '…'` or `timestamptz '…'`.", typed.Position);
	}

	/// <summary>
	/// The text of a string literal. Four escapes only — <c>\\</c>, <c>\'</c>, <c>\"</c> and <c>\uXXXX</c> — so that a
	/// text written with SQL's habits, where a backslash is a character, is refused rather than read as something else.
	/// </summary>
	static string Unescape(LiteralExpr literal)
	{
		var text = literal.Text;

		if (text.IndexOf('\\') < 0)
			return text.Substring(1, text.Length - 2);

		var builder = new StringBuilder(text.Length);

		for (var i = 1; i < text.Length - 1; i++)
		{
			var c = text[i];

			if (c != '\\')
			{
				builder.Append(c);
				continue;
			}

			var next = text[i + 1];

			if (next is '\\' or '\'' or '"')
			{
				builder.Append(next);
				i++;
				continue;
			}

			if (next == 'u' && i + 5 < text.Length && ushort.TryParse(text.Substring(i + 2, 4), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var code))
			{
				builder.Append((char)code);
				i += 5;
				continue;
			}

			throw new QlException(QlErrorCode.Syntax, $"`\\{next}` is not an escape of the language: write `\\\\` for a backslash, `\\'` or `\\\"` for a quote, `\\uXXXX` for any character.", literal.Position + i);
		}

		return builder.ToString();
	}

	// ── Operators ───────────────────────────────────────────────────────────

	BoundExpr BindUnary(UnaryExpr unary, ScalarType? expected)
	{
		if (unary.Operator == UnaryOperator.Not)
		{
			var operand = BindValue(unary.Operand, ScalarType.Of(QlType.Bool, true));

			if (operand.Type.Type != QlType.Bool)
				throw Error(QlErrorCode.Type, $"`!` takes a condition, and this is a {operand.Type.Name}.", unary.Position);

			return new BoundUnary(ExpressionType.Not, operand, operand.Type, unary.Position);
		}

		// `-5` is a number, and takes the other side's type as a number does.
		if (unary.Operand is LiteralExpr { Kind: LiteralKind.Number } number)
			return Number("-" + number.Text, unary.Position, expected);

		var value = BindValue(unary.Operand, expected, nullable: false);

		if (!value.Type.IsNumeric)
			throw Error(QlErrorCode.Type, $"`-` takes a number, and this is a {value.Type.Name}.", unary.Position);

		var kind = value.Type.Type is QlType.Int or QlType.Long ? ExpressionType.NegateChecked : ExpressionType.Negate;

		return Fold(new BoundUnary(kind, value, value.Type, unary.Position));
	}

	BoundExpr BindArithmetic(BinaryExpr binary, ScalarType? expected)
	{
		var (left, right) = IsUntyped(binary.Left) && IsUntyped(binary.Right) && expected is { IsNumeric: true }
			? (BindValue(binary.Left, expected, nullable: false), BindValue(binary.Right, expected, nullable: false))
			: BindPair(binary.Left, binary.Right, nullable: false);

		var op = Symbol(binary.Operator);

		if (left.Type.Type == QlType.String || right.Type.Type == QlType.String)
			throw Error(QlErrorCode.Type, $"`{op}` is not defined for strings.", binary.Position);

		if (!left.Type.IsNumeric || !right.Type.IsNumeric)
			throw Error(QlErrorCode.Type, $"`{op}` takes numbers, and these are {left.Type.Name} and {right.Type.Name}.", binary.Position);

		if (left is BoundLiteral { Value: null } || right is BoundLiteral { Value: null })
			throw Error(QlErrorCode.Type, $"`null` is not a number: `{op}` takes numbers.", binary.Position);

		var common = Numbers.Common(left.Type.Type, right.Type.Type)
			?? throw Error(QlErrorCode.Type, $"`{op}` does not mix {left.Type.Name} and {right.Type.Name}: they do not convert to each other without loss.", binary.Position);

		var type     = ScalarType.Of(common, left.Type.Nullable || right.Type.Nullable);
		var integral = common is QlType.Int or QlType.Long;

		var kind = binary.Operator switch
		{
			BinaryOperator.Add      => integral ? ExpressionType.AddChecked      : ExpressionType.Add,
			BinaryOperator.Subtract => integral ? ExpressionType.SubtractChecked : ExpressionType.Subtract,
			BinaryOperator.Multiply => integral ? ExpressionType.MultiplyChecked : ExpressionType.Multiply,
			BinaryOperator.Divide   => ExpressionType.Divide,
			_                       => ExpressionType.Modulo,
		};

		return Fold(new BoundBinary(kind, Convert(left, type), Convert(right, type), type, binary.Position));
	}

	/// <summary>An arithmetic that reads no row and no parameter is computed now, checked, and an error is the text's.</summary>
	static BoundExpr Fold(BoundExpr value)
	{
		if (value.ReadsRow || Evaluator.HasParameters(value))
			return value;

		return new BoundLiteral(Evaluator.Evaluate(value, null), value.Type, value.Position);
	}

	BoundExpr BindComparison(ExprSyntax leftSyntax, BinaryOperator op, ExprSyntax rightSyntax, int position)
	{
		var (left, right) = BindPair(leftSyntax, rightSyntax, nullable: true);

		return Compare(left, op, right, position);
	}

	BoundExpr Compare(BoundExpr left, BinaryOperator op, BoundExpr right, int position)
	{
		var symbol   = Symbol(op);
		var equality = op is BinaryOperator.Equal or BinaryOperator.NotEqual;

		if (!equality && (left is BoundLiteral { Value: null } || right is BoundLiteral { Value: null }))
			throw Error(QlErrorCode.Type, $"`null` compares only with `==` and `!=`.", position);

		ScalarType type;

		if (left.Type.IsNumeric && right.Type.IsNumeric)
		{
			var common = Numbers.Common(left.Type.Type, right.Type.Type)
				?? throw Error(QlErrorCode.Type, $"`{symbol}` does not compare {left.Type.Name} with {right.Type.Name}: they do not convert to each other without loss.", position);

			type = ScalarType.Of(common, left.Type.Nullable || right.Type.Nullable);
		}
		else if (left.Type.Type == right.Type.Type && left.Type.Core == right.Type.Core)
		{
			type = left.Type.WithNullable(left.Type.Nullable || right.Type.Nullable);
		}
		else
		{
			throw Error(QlErrorCode.Type, $"`{symbol}` does not compare {left.Type.Name} with {right.Type.Name}.", position);
		}

		if (!equality && type.Type is QlType.Bool or QlType.Guid)
			throw Error(QlErrorCode.Type, $"`{symbol}` is not defined for {type.Name}: it compares only with `==` and `!=`.", position);

		var kind = op switch
		{
			BinaryOperator.Equal       => ExpressionType.Equal,
			BinaryOperator.NotEqual    => ExpressionType.NotEqual,
			BinaryOperator.Less        => ExpressionType.LessThan,
			BinaryOperator.LessOrEqual => ExpressionType.LessThanOrEqual,
			BinaryOperator.Greater     => ExpressionType.GreaterThan,
			_                          => ExpressionType.GreaterThanOrEqual,
		};

		// A type outside the language compares as its own operators say, and one that has none for this is refused
		// here: what the provider makes of an operator that exists is the provider's.
		if (type.Type == QlType.Other && !Operators.Exist(kind, type.Clr))
			throw Error(QlErrorCode.Type, $"Operator `{symbol}` is not defined for `{ScalarType.ShortName(type.Core)}`.", position);

		return new BoundBinary(kind, Convert(left, type), Convert(right, type), ScalarType.Of(QlType.Bool, false), position);
	}

	BoundExpr BindLogical(LogicalExpr logical)
	{
		var items = new BoundExpr[logical.Items.Length];

		for (var i = 0; i < items.Length; i++)
		{
			items[i] = BindValue(logical.Items[i], ScalarType.Of(QlType.Bool, true));

			if (items[i].Type.Type != QlType.Bool)
				throw Error(QlErrorCode.Type, $"`{(logical.Operator == LogicalOperator.And ? "&&" : "||")}` takes conditions, and this is a {items[i].Type.Name}.", logical.Items[i].Position);
		}

		return Balanced(logical.Operator == LogicalOperator.And ? ExpressionType.AndAlso : ExpressionType.OrElse, items, 0, items.Length);
	}

	/// <summary>A list of conditions as a balanced tree: its depth is the logarithm of its length, not the length.</summary>
	static BoundExpr Balanced(ExpressionType kind, BoundExpr[] items, int start, int end)
	{
		if (end - start == 1)
			return items[start];

		var middle = (start + end) / 2;
		var left   = Balanced(kind, items, start, middle);
		var right  = Balanced(kind, items, middle, end);
		var type   = ScalarType.Of(QlType.Bool, left.Type.Nullable || right.Type.Nullable);

		return new BoundBinary(kind, Convert(left, type), Convert(right, type), type, left.Position);
	}

	BoundExpr BindIsNull(IsNullExpr isNull)
	{
		var operand = BindValue(isNull.Operand, null);
		var type    = operand.Type.WithNullable(true);
		var test    = new BoundBinary(isNull.Negated ? ExpressionType.NotEqual : ExpressionType.Equal, Convert(operand, type), new BoundLiteral(null, type, isNull.Position), ScalarType.Of(QlType.Bool, false), isNull.Position);

		return test;
	}

	BoundExpr BindIn(InExpr @in)
	{
		var operand = BindValue(@in.Operand, null);

		BoundExpr result;

		if (@in.List is { } list)
		{
			result = new BoundIn(operand, null, Parameter(list, operand.Type, nullable: operand.Type.Nullable, list: true), ScalarType.Of(QlType.Bool, false), @in.Position);
		}
		else
		{
			var items    = new BoundExpr[@in.Items!.Length];
			var nullable = operand.Type.Nullable;

			for (var i = 0; i < items.Length; i++)
			{
				items[i] = BindValue(@in.Items[i], operand.Type);

				if (items[i].ReadsRow)
					throw Error(QlErrorCode.Type, "The values of `in` are literals and parameters: they read no row.", @in.Items[i].Position);

				if (items[i].Type.Type != operand.Type.Type || items[i].Type.Core != operand.Type.Core)
					throw Error(QlErrorCode.Type, $"`in` looks for a {operand.Type.Name}, and this is a {items[i].Type.Name}.", @in.Items[i].Position);

				nullable |= items[i].Type.Nullable;
			}

			var type = operand.Type.WithNullable(nullable);

			for (var i = 0; i < items.Length; i++)
				items[i] = Convert(items[i], type);

			result = new BoundIn(Convert(operand, type), items, null, ScalarType.Of(QlType.Bool, false), @in.Position);
		}

		return @in.Negated ? new BoundUnary(ExpressionType.Not, result, result.Type, @in.Position) : result;
	}

	BoundExpr BindBetween(BetweenExpr between)
	{
		var operand = BindValue(between.Operand, null);
		var low     = Compare(operand, BinaryOperator.GreaterOrEqual, BindValue(between.Low,  operand.Type), between.Position);
		var high    = Compare(operand, BinaryOperator.LessOrEqual,    BindValue(between.High, operand.Type), between.Position);
		var both    = new BoundBinary(ExpressionType.AndAlso, low, high, ScalarType.Of(QlType.Bool, false), between.Position);

		return between.Negated ? new BoundUnary(ExpressionType.Not, both, both.Type, between.Position) : both;
	}

	/// <summary>
	/// <c>x like p</c>, as the host's LIKE: the language reads nothing in the pattern. The left is a string of the rows,
	/// so that the database computes it; the right is any string. A model without a LIKE refuses it after its operands,
	/// so that a field named there without its right is reported as it is anywhere else.
	/// </summary>
	BoundExpr BindLike(LikeExpr like)
	{
		var text = ScalarType.Of(QlType.String, true);

		if (IsUntyped(like.Operand) || like.Operand is LiteralExpr or TypedLiteralExpr)
			throw Error(QlErrorCode.Type, "`like` matches a value of the rows: write the field on its left, `name like @pattern`.", like.Position);

		var operand = BindValue(like.Operand, text);

		if (operand.Type.Type != QlType.String)
			throw Error(QlErrorCode.Type, $"`like` matches strings, and this is a {operand.Type.Name}.", like.Position);

		if (!operand.ReadsRow)
			throw Error(QlErrorCode.Type, "`like` matches a value of the rows: write the field on its left, `name like @pattern`.", like.Position);

		var pattern = BindValue(like.Pattern, text);

		if (pattern.Type.Type != QlType.String)
			throw Error(QlErrorCode.Type, $"The pattern of `like` is a string, and this is a {pattern.Type.Name}.", like.Pattern.Position);

		var hook = _like ?? throw Error(QlErrorCode.Type, "`like` is not available in this model: the host has not declared what it means.", like.Position);

		return new BoundLike(Convert(operand, text), Convert(pattern, text), like.Negated, hook, like.Position);
	}

	/// <summary>A value as another type: <c>int</c> to <c>long</c>, <c>decimal</c> or <c>double</c>, <c>T</c> to <c>T?</c>.</summary>
	static BoundExpr Convert(BoundExpr value, ScalarType type)
	{
		if (value.Type.Type == type.Type && value.Type.Core == type.Core && value.Type.Clr == type.Clr)
			return value;

		if (value is BoundLiteral literal)
			return new BoundLiteral(literal.Value is null ? null : Numbers.Widen(literal.Value, type.Type), type, literal.Position);

		return new BoundConvert(value, type);
	}

	static string Symbol(BinaryOperator op)
	{
		return op switch
		{
			BinaryOperator.Add            => "+",
			BinaryOperator.Subtract       => "-",
			BinaryOperator.Multiply       => "*",
			BinaryOperator.Divide         => "/",
			BinaryOperator.Modulo         => "%",
			BinaryOperator.Equal          => "==",
			BinaryOperator.NotEqual       => "!=",
			BinaryOperator.Less           => "<",
			BinaryOperator.LessOrEqual    => "<=",
			BinaryOperator.Greater        => ">",
			_                             => ">=",
		};
	}

	// ── Members ─────────────────────────────────────────────────────────────

	BoundExpr BindMember(MemberExpr member)
	{
		if (member.Target is NameExpr target && Find(target.Name.Text) is null && Names.Comparer.Equals(target.Name.Text, _qualifier))
		{
			if (_selectAt is { } at)
				throw Error(QlErrorCode.UnknownName, $"`{target.Name.Text}` is not available after the `select` at {at}.", target.Position);

			if (member.Arguments is not null)
				throw Error(QlErrorCode.Type, $"`{target.Name.Text}.{member.Name.Text}` is a field, not a call.", member.Name.Position);

			var field = _entity.Find(member.Name.Text) is { } found && Allowed(found, member.Name)
				? found
				: throw Unknown(member.Name.Text, member.Name.Position, target.Name.Text);

			return new BoundField(field, member.Name.Text, member.Name.Position, target.Name.Text);
		}

		if (IsUntyped(member.Target) || member.Target is LiteralExpr or TypedLiteralExpr)
			throw Error(QlErrorCode.Type, $"`{member.Name.Text}` of a value that reads no row is the caller's to compute: lower or upper the value on the caller's side.", member.Name.Position);

		var receiver = BindValue(member.Target, null);

		if (!receiver.ReadsRow)
			throw Error(QlErrorCode.Type, $"`{member.Name.Text}` of a value that reads no row is the caller's to compute: lower or upper the value on the caller's side.", member.Name.Position);

		var entry = Members.Find(receiver.Type.Type, member.Name.Text)
			?? throw Error(QlErrorCode.UnknownName, $"`{receiver.Type.Name}` has no member `{member.Name.Text}`.", member.Name.Position);

		var arguments = member.Arguments;

		if (entry.Parameters is null && arguments is not null)
			throw Error(QlErrorCode.Type, $"`{entry.Name}` is a property: write it without brackets.", member.Name.Position);

		if (entry.Parameters is { } parameters && (arguments is null || arguments.Length != parameters.Length))
			throw Error(QlErrorCode.Type, $"`{entry.Name}` takes {parameters.Length} argument{(parameters.Length == 1 ? "" : "s")}: `{entry.Name}({(parameters.Length == 1 ? "x" : "")})`.", member.Name.Position);

		var bound = new BoundExpr[arguments?.Length ?? 0];

		for (var i = 0; i < bound.Length; i++)
		{
			bound[i] = BindValue(arguments![i], ScalarType.Of(entry.Parameters![i], false), nullable: false);

			if (bound[i].Type.Type != entry.Parameters[i])
				throw Error(QlErrorCode.Type, $"`{entry.Name}` takes a {ScalarType.Of(entry.Parameters[i], false).Name}, and this is a {bound[i].Type.Name}.", arguments[i].Position);
		}

		var type = ScalarType.Of(entry.Result, receiver.Type.Nullable && entry.Lifted);

		return new BoundMember(receiver, entry.Member(receiver.Type.Core), bound, type, member.Name.Position);
	}

	// ── Aggregates ──────────────────────────────────────────────────────────

	BoundExpr BindAggregate(CallExpr call)
	{
		var kind = Aggregates.KindOf(call.Name.Text, call.Star);
		var name = Aggregates.Name(call.Name.Text);

		if (call.Star != (kind == AggregateKind.CountAll) || (!call.Star && call.Arguments.Length != 1))
			throw Error(QlErrorCode.Type, kind is AggregateKind.CountAll or AggregateKind.Count ? "`Count` takes `*` or one value." : $"`{name}` takes one value.", call.Position);

		if (kind == AggregateKind.CountAll)
			return new BoundAggregate(kind, null, ScalarType.Of(QlType.Int, false), call.Position);

		var argument = BindValue(call.Arguments[0], null);

		if (!argument.ReadsRow)
			throw Error(QlErrorCode.Type, $"`{name}` aggregates a value of the rows: this one reads none.", call.Arguments[0].Position);

		ScalarType type;

		switch (kind)
		{
			case AggregateKind.Count:
				type = ScalarType.Of(QlType.Int, false);
				break;

			case AggregateKind.Sum:
			case AggregateKind.Average:
				if (!argument.Type.IsNumeric)
					throw Error(QlErrorCode.Type, $"`{name}` takes a number, and this is a {argument.Type.Name}.", call.Arguments[0].Position);

				type = ScalarType.Of(kind == AggregateKind.Average && argument.Type.Type is QlType.Int or QlType.Long ? QlType.Double : argument.Type.Type, true);
				break;

			default:
				type = argument.Type.WithNullable(true);
				break;
		}

		return new BoundAggregate(kind, argument, type, call.Position);
	}
}
