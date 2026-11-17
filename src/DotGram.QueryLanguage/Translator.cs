using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using DotGram.QueryLanguage.Syntax;

namespace DotGram.QueryLanguage;

/// <summary>
/// Holds one value of a query for linq2db to read as a parameter: a field of a constant is what
/// a provider parameterizes, as it does a lambda's closure.
/// </summary>
sealed class QlHolder<T>(T value)
{
	public readonly T Value = value;
}

static class QlHolder
{
	static readonly ConcurrentDictionary<Type, Func<object?, object>> Factories = new();

	public static Expression Of(Type type, object? value)
	{
		var factory = Factories.GetOrAdd(type, static t =>
		{
			var holder = typeof(QlHolder<>).MakeGenericType(t);
			var arg    = Expression.Parameter(typeof(object), "value");
			var make   = Expression.New(holder.GetConstructors()[0], Expression.Convert(arg, t));

			return Expression.Lambda<Func<object?, object>>(make, arg).Compile();
		});

		var instance = factory(value);

		return Expression.Field(Expression.Constant(instance), "Value");
	}

	public static bool IsHolder(Type type)
	{
		return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(QlHolder<>);
	}
}

/// <summary>
/// Builds the expression tree from the bound tree and a request's values. A fixed set of nodes:
/// Queryable's methods by closed MethodInfo, reads of the members the model maps, holders of
/// values, the operators of the language's types. Nothing the text names is looked up anywhere.
/// </summary>
static class Translator
{
	static readonly MethodInfo Where             = Queryable("Where",             2, m => m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2);
	static readonly MethodInfo OrderBy           = Queryable("OrderBy",           2, null);
	static readonly MethodInfo OrderByDescending = Queryable("OrderByDescending", 2, null);
	static readonly MethodInfo ThenBy            = Queryable("ThenBy",            2, null);
	static readonly MethodInfo ThenByDescending  = Queryable("ThenByDescending",  2, null);
	static readonly MethodInfo Skip              = Queryable("Skip",              2, m => m.GetParameters()[1].ParameterType == typeof(int));
	static readonly MethodInfo Take              = Queryable("Take",              2, m => m.GetParameters()[1].ParameterType == typeof(int));
	static readonly MethodInfo Select            = Queryable("Select",            2, m => m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2);

	static readonly MethodInfo Contains = typeof(Enumerable).GetMethods()
		.Single(m => m.Name == "Contains" && m.GetParameters().Length == 2);

	static readonly MethodInfo StringCompare = typeof(string).GetMethod("Compare", [typeof(string), typeof(string)])!;

	/// <summary>The generic definitions the translator may call, which the audit allows.</summary>
	public static readonly HashSet<MethodInfo> Methods =
		[Where, OrderBy, OrderByDescending, ThenBy, ThenByDescending, Skip, Take, Select, Contains, StringCompare];

	static MethodInfo Queryable(string name, int parameters, Func<MethodInfo, bool>? filter)
	{
		return typeof(System.Linq.Queryable).GetMethods()
			.Single(m => m.Name == name && m.GetParameters().Length == parameters && (filter is null || filter(m)));
	}

	public static Expression Translate(BoundQuery query, Expression root, object?[] values, int take, IQlAdapter adapter)
	{
		var entity = query.Entity.ClrType;
		var row    = Expression.Parameter(entity, "row");
		var source = root;
		var holders = new Expression?[values.Length];

		Expression Holder(Slot slot)
		{
			return holders[slot.Index] ??= QlHolder.Of(slot.ClrType, values[slot.Index]);
		}

		if (query.Where is { } where)
		{
			var body = Build(where, row, Holder, adapter);

			source = Expression.Call(Where.MakeGenericMethod(entity), source, Expression.Quote(Expression.Lambda(body, row)));
		}

		var ordered = false;
		var keyed   = false;

		foreach (var item in query.Order)
		{
			var method = (ordered, item.Descending) switch
			{
				(false, false) => OrderBy,
				(false, true)  => OrderByDescending,
				(true,  false) => ThenBy,
				(true,  true)  => ThenByDescending,
			};

			source  = Order(source, method, row, item.Column.Member, entity);
			ordered = true;
			keyed  |= item.Column.Member == query.Entity.Key;
		}

		// Pages of a result are pages of one order: the key last, so that rows equal on every
		// field sorted by do not move between pages.
		if (!keyed && (query.Limit is not null || query.Offset is not null))
			source = Order(source, ordered ? ThenBy : OrderBy, row, query.Entity.Key, entity);

		if (query.Offset is { } offset)
			source = Expression.Call(Skip.MakeGenericMethod(entity), source, Holder(offset));

		source = Expression.Call(Take.MakeGenericMethod(entity), source, QlHolder.Of(typeof(int), take));

		var fields = new Expression[query.Outputs.Count];

		for (var i = 0; i < fields.Length; i++)
			fields[i] = Expression.Convert(Expression.MakeMemberAccess(row, query.Outputs[i].Column.Member), typeof(object));

		var projection = Expression.Lambda(Expression.NewArrayInit(typeof(object), fields), row);

		return Expression.Call(Select.MakeGenericMethod(entity, typeof(object[])), source, Expression.Quote(projection));
	}

	static Expression Order(Expression source, MethodInfo method, ParameterExpression row, MemberInfo member, Type entity)
	{
		var key = Expression.MakeMemberAccess(row, member);

		return Expression.Call(method.MakeGenericMethod(entity, key.Type), source, Expression.Quote(Expression.Lambda(key, row)));
	}

	static Expression Build(BoundExpr node, ParameterExpression row, Func<Slot, Expression> holder, IQlAdapter adapter)
	{
		switch (node)
		{
			case BoundColumn column:
				return Expression.MakeMemberAccess(row, column.Column.Member);

			case BoundSlot slot:
				return holder(slot.Slot);

			case BoundConvert convert:
			{
				var operand = Build(convert.Operand, row, holder, adapter);

				return Expression.Convert(operand, QlTypes.Clr(convert.Type, IsNullable(operand.Type)));
			}

			case BoundNegate negate:
				return Expression.Negate(Build(negate.Operand, row, holder, adapter));

			case BoundArithmetic arithmetic:
			{
				var (left, right) = Lift(Build(arithmetic.Left, row, holder, adapter), Build(arithmetic.Right, row, holder, adapter));

				return arithmetic.Operator switch
				{
					ArithmeticOperator.Add      => Expression.Add(left, right),
					ArithmeticOperator.Subtract => Expression.Subtract(left, right),
					ArithmeticOperator.Multiply => Expression.Multiply(left, right),
					ArithmeticOperator.Divide   => Expression.Divide(left, right),
					_                           => Expression.Modulo(left, right),
				};
			}

			case BoundCompare compare:
			{
				var (left, right) = Lift(Build(compare.Left, row, holder, adapter), Build(compare.Right, row, holder, adapter));

				// Strings have no ordering operators in C#; string.Compare is what a provider reads as one.
				if (compare.Left.Type == QlType.String && compare.Operator is not (CompareOperator.Equal or CompareOperator.NotEqual))
				{
					left  = Expression.Call(StringCompare, left, right);
					right = Expression.Constant(0);
				}

				return compare.Operator switch
				{
					CompareOperator.Equal          => Expression.Equal(left, right),
					CompareOperator.NotEqual       => Expression.NotEqual(left, right),
					CompareOperator.Less           => Expression.LessThan(left, right),
					CompareOperator.LessOrEqual    => Expression.LessThanOrEqual(left, right),
					CompareOperator.Greater        => Expression.GreaterThan(left, right),
					_                              => Expression.GreaterThanOrEqual(left, right),
				};
			}

			case BoundLogical logical:
			{
				var items = new Expression[logical.Items.Length];

				for (var i = 0; i < items.Length; i++)
					items[i] = Build(logical.Items[i], row, holder, adapter);

				return Balanced(items, 0, items.Length, logical.Operator == LogicalOperator.And);
			}

			case BoundNot not:
				return Expression.Not(Build(not.Operand, row, holder, adapter));

			case BoundIsNull isNull:
			{
				var operand = Build(isNull.Operand, row, holder, adapter);

				// A field that cannot be NULL is never NULL; the database is not asked.
				if (!IsNullable(operand.Type))
					return Expression.Constant(isNull.Negated);

				var test = Expression.Constant(null, operand.Type);

				return isNull.Negated ? Expression.NotEqual(operand, test) : Expression.Equal(operand, test);
			}

			case BoundIn @in:
			{
				var operand = Build(@in.Operand, row, holder, adapter);
				var list    = holder(@in.List);
				var element = list.Type.GetElementType()!;

				if (operand.Type != element)
					operand = Expression.Convert(operand, element);

				var contains = Expression.Call(Contains.MakeGenericMethod(element), list, operand);

				return @in.Negated ? Expression.Not(contains) : contains;
			}

			case BoundLike like:
			{
				var test = adapter.Like(Build(like.Operand, row, holder, adapter), holder(like.Pattern));

				return like.Negated ? Expression.Not(test) : test;
			}

			default:
				throw new InvalidOperationException(node.GetType().Name);
		}
	}

	/// <summary>
	/// AND and OR built as a balanced tree, so that a chain of n costs log n of depth: linq2db's
	/// cost depends on the shape, and the text chooses the length.
	/// </summary>
	static Expression Balanced(Expression[] items, int start, int end, bool and)
	{
		if (end - start == 1)
			return items[start];

		var middle = start + (end - start) / 2;
		var left   = Balanced(items, start, middle, and);
		var right  = Balanced(items, middle, end, and);

		return and ? Expression.AndAlso(left, right) : Expression.OrElse(left, right);
	}

	static (Expression, Expression) Lift(Expression left, Expression right)
	{
		if (left.Type == right.Type)
			return (left, right);

		if (IsNullable(left.Type) && Nullable.GetUnderlyingType(left.Type) == right.Type)
			return (left, Expression.Convert(right, left.Type));

		if (IsNullable(right.Type) && Nullable.GetUnderlyingType(right.Type) == left.Type)
			return (Expression.Convert(left, right.Type), right);

		return (left, right);
	}

	static bool IsNullable(Type type)
	{
		return !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;
	}
}
