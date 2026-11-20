using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotGram.QueryLanguage;

/// <summary>
/// Builds a call's tree from a bound text: the entity's source, then one <see cref="Queryable"/> call per clause as
/// the text wrote it, then a <c>Take</c> of <see cref="QlOptions.MaxRows"/>, then the final projection. Nothing the
/// text names is looked up here: every member comes from the model, every method from the fixed set below.
/// </summary>
static class Translator
{
	static readonly MethodInfo WhereMethod             = Queryable(nameof(System.Linq.Queryable.Where),             IsPredicate);
	static readonly MethodInfo SelectMethod            = Queryable(nameof(System.Linq.Queryable.Select),            IsSelector);
	static readonly MethodInfo OrderByMethod           = Queryable(nameof(System.Linq.Queryable.OrderBy),           IsSelector);
	static readonly MethodInfo OrderByDescendingMethod = Queryable(nameof(System.Linq.Queryable.OrderByDescending), IsSelector);
	static readonly MethodInfo ThenByMethod            = Queryable(nameof(System.Linq.Queryable.ThenBy),            IsSelector);
	static readonly MethodInfo ThenByDescendingMethod  = Queryable(nameof(System.Linq.Queryable.ThenByDescending),  IsSelector);
	static readonly MethodInfo GroupByMethod           = Queryable(nameof(System.Linq.Queryable.GroupBy),           IsSelector);
	static readonly MethodInfo SkipMethod              = Queryable(nameof(System.Linq.Queryable.Skip),              IsCount);
	static readonly MethodInfo TakeMethod              = Queryable(nameof(System.Linq.Queryable.Take),              IsCount);

	static readonly MethodInfo ContainsMethod = typeof(Enumerable).GetMethods().Single(m => m.Name == nameof(Enumerable.Contains) && m.GetParameters().Length == 2);
	static readonly MethodInfo CompareMethod  = typeof(string).GetMethod(nameof(string.Compare), [typeof(string), typeof(string)])!;

	static MethodInfo Queryable(string name, Func<MethodInfo, bool> shape)
	{
		return typeof(Queryable).GetMethods().Single(m => m.Name == name && m.GetParameters().Length == 2 && shape(m));
	}

	/// <summary><c>Expression&lt;Func&lt;T, bool&gt;&gt;</c>, not the overload that also takes the index.</summary>
	static bool IsPredicate(MethodInfo method)
	{
		return method.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2;
	}

	static bool IsSelector(MethodInfo method)
	{
		return method.GetParameters()[1].ParameterType is { IsGenericType: true } type && type.GetGenericArguments()[0].GetGenericArguments().Length == 2;
	}

	static bool IsCount(MethodInfo method)
	{
		return method.GetParameters()[1].ParameterType == typeof(int);
	}

	/// <summary>The rows as arrays of values, over <paramref name="source"/>, with the call's coerced <paramref name="values"/>.</summary>
	public static IQueryable<object?[]> QueryData(BoundQuery query, IQueryable source, object?[] values, QlOptions options)
	{
		var call  = new Call(values, query.Parameters);
		var chain = Chain(query, source.Expression, call, options);
		var row   = Expression.Parameter(RowType(query.Final, query.Entity), "r");

		var columns = new Expression[query.Output.Length];

		for (var i = 0; i < columns.Length; i++)
		{
			var value = call.Value(query.Output[i], row, query.Final);

			columns[i] = value.Type.IsValueType ? Expression.Convert(value, typeof(object)) : value;
		}

		var projection = Expression.Lambda(Expression.NewArrayInit(typeof(object), columns), row);
		var final      = Expression.Call(SelectMethod.MakeGenericMethod(row.Type, typeof(object[])), chain, Expression.Quote(projection));

		return source.Provider.CreateQuery<object?[]>(final);
	}

	/// <summary>
	/// The rows as JSON text, over <paramref name="source"/>, with the call's coerced <paramref name="values"/>: the same
	/// chain as <see cref="QueryData"/>, ending in the calls of <see cref="QlJson"/> instead of an array.
	/// </summary>
	public static IQueryable<string> QueryJsonRow(BoundQuery query, IQueryable source, object?[] values, QlOptions options, QlJsonFormat format, IReadOnlyDictionary<Type, Delegate> formats)
	{
		var call  = new Call(values, query.Parameters);
		var chain = Chain(query, source.Expression, call, options);
		var row   = Expression.Parameter(RowType(query.Final, query.Entity), "r");

		var columns = new Expression[query.Output.Length];

		for (var i = 0; i < columns.Length; i++)
			columns[i] = call.Value(query.Output[i], row, query.Final);

		var projection = Expression.Lambda(JsonProjection.Row(row, query.Output, columns, query.Names, format, formats), row);
		var final      = Expression.Call(SelectMethod.MakeGenericMethod(row.Type, typeof(string)), chain, Expression.Quote(projection));

		return source.Provider.CreateQuery<string>(final);
	}

	/// <summary>The source, each clause as written, and the row bound last.</summary>
	static Expression Chain(BoundQuery query, Expression source, Call call, QlOptions options)
	{
		var chain = source;

		foreach (var clause in query.Clauses)
			chain = Clause(query.Entity, chain, clause, call);

		if (options.MaxRows is { } rows)
			chain = Expression.Call(TakeMethod.MakeGenericMethod(RowType(query.Final, query.Entity)), chain, Expression.Constant(rows));

		return chain;
	}

	/// <summary>The CLR type of a row of <paramref name="shape"/>.</summary>
	public static Type RowType(Shape shape, ModelEntity entity)
	{
		return shape.Kind switch
		{
			ShapeKind.Entity    => entity.Type,
			ShapeKind.EntityRow => typeof(QlRow<>).MakeGenericType(entity.Type),
			_                   => typeof(QlScalars),
		};
	}

	static Expression Clause(ModelEntity entity, Expression chain, BoundClause clause, Call call)
	{
		var before = RowType(clause.Before, entity);
		var row    = Expression.Parameter(before, "r");

		switch (clause)
		{
			case BoundWhere where:
			{
				var condition = call.Value(where.Condition, row, clause.Before);

				// A condition that may be null keeps the rows where it is true, as `where` does in C# with `== true`.
				if (condition.Type != typeof(bool))
					condition = Expression.Equal(condition, Expression.Constant(true, typeof(bool?)));

				return Expression.Call(WhereMethod.MakeGenericMethod(before), chain, Expression.Quote(Expression.Lambda(condition, row)));
			}

			// A `let` nothing reads is not built: what it names is never read, so the provider never sees it.
			case BoundLet { Column.Used: false } when clause.Before.Kind == clause.After.Kind:
				return chain;

			case BoundLet or BoundSelect { Aggregate: false }:
			{
				var made = Row(entity, clause, row, call);

				return Expression.Call(SelectMethod.MakeGenericMethod(before, made.Type), chain, Expression.Quote(Expression.Lambda(made, row)));
			}

			case BoundSelect:
				return Aggregate(entity, chain, clause, call);

			case BoundOrder order:
			{
				var first = true;

				foreach (var key in order.Keys)
				{
					var value = call.Value(key.Key, row, clause.Before);

					// Nulls first or last: a key of 0 and 1 ahead of the value, where the value may be null at all.
					if (key.NullsFirst is { } nullsFirst && (!value.Type.IsValueType || Nullable.GetUnderlyingType(value.Type) is not null))
					{
						var isNull  = Expression.Equal(value, Expression.Constant(null, value.Type));
						var ranking = Expression.Condition(isNull, Expression.Constant(nullsFirst ? 0 : 1), Expression.Constant(nullsFirst ? 1 : 0));

						chain = Order(chain, before, row, ranking, first, false);
						first = false;
					}

					chain = Order(chain, before, row, value, first, key.Descending);
					first = false;
				}

				return chain;
			}

			case BoundPage page:
			{
				var count = call.Value(page.Count, row, clause.Before);

				return Expression.Call((page.Skip ? SkipMethod : TakeMethod).MakeGenericMethod(before), chain, count);
			}

			default:
				throw new InvalidOperationException(clause.GetType().Name);
		}
	}

	static Expression Order(Expression chain, Type row, ParameterExpression parameter, Expression key, bool first, bool descending)
	{
		var method = (first, descending) switch
		{
			(true,  false) => OrderByMethod,
			(true,  true)  => OrderByDescendingMethod,
			(false, false) => ThenByMethod,
			(false, true)  => ThenByDescendingMethod,
		};

		return Expression.Call(method.MakeGenericMethod(row, key.Type), chain, Expression.Quote(Expression.Lambda(key, parameter)));
	}

	/// <summary>
	/// The row after a <c>let</c> or a <c>select</c>: the entity and its values in <see cref="QlRow{TE}"/>, or the
	/// values alone in <see cref="QlScalars"/>. A <c>let</c> carries the values before it over, slot for slot. A
	/// column nothing reads is left out.
	/// </summary>
	static Expression Row(ModelEntity entity, BoundClause clause, ParameterExpression row, Call call)
	{
		var after    = clause.After;
		var bindings = new List<MemberBinding>();

		foreach (var column in after.Columns)
		{
			if (!column.Used)
				continue;

			var value = clause is BoundLet let && let.Column != column
				? call.Read(column, row, clause.Before)
				: call.Value(column.Value, row, clause.Before);

			bindings.Add(Bind(after, column, value));
		}

		var scalars = Expression.MemberInit(Expression.New(typeof(QlScalars)), bindings);

		if (after.Kind != ShapeKind.EntityRow)
			return scalars;

		var type     = typeof(QlRow<>).MakeGenericType(entity.Type);
		var instance = clause.Before.Kind == ShapeKind.Entity ? row : (Expression)Expression.Field(row, nameof(QlRow<object>.E));

		return Expression.MemberInit(
			Expression.New(type),
			Expression.Bind(type.GetField(nameof(QlRow<object>.E))!, instance),
			Expression.Bind(type.GetField(nameof(QlRow<object>.S))!, scalars));
	}

	static MemberBinding Bind(Shape shape, Column column, Expression value)
	{
		var slot = Slot(shape, column);

		return Expression.Bind(slot, value.Type == slot.FieldType ? value : Expression.Convert(value, slot.FieldType));
	}

	/// <summary>A <c>select</c> of aggregates: every row at once, grouped by a constant, into one row of values.</summary>
	static Expression Aggregate(ModelEntity entity, Expression chain, BoundClause clause, Call call)
	{
		var element = RowType(clause.Before, entity);
		var row     = Expression.Parameter(element, "r");
		var grouped = Expression.Call(GroupByMethod.MakeGenericMethod(element, typeof(int)), chain, Expression.Quote(Expression.Lambda(Expression.Constant(0), row)));
		var group   = Expression.Parameter(typeof(IGrouping<,>).MakeGenericType(typeof(int), element), "g");

		var bindings = new List<MemberBinding>();

		foreach (var column in clause.After.Columns)
			if (column.Used)
				bindings.Add(Bind(clause.After, column, call.Aggregate((BoundAggregate)column.Value, group, element, clause.Before)));

		var made = Expression.MemberInit(Expression.New(typeof(QlScalars)), bindings);

		return Expression.Call(SelectMethod.MakeGenericMethod(group.Type, typeof(QlScalars)), grouped, Expression.Quote(Expression.Lambda(made, group)));
	}

	/// <summary>Where each column of a row of <paramref name="shape"/> is: its kind's slots in column order, among the columns read.</summary>
	static FieldInfo Slot(Shape shape, Column column)
	{
		var index = 0;
		var kind  = Slots.KindOf(column.Type);

		foreach (var other in shape.Columns)
		{
			if (other == column)
				return Slots.Field(column.Type, index);

			if (other.Used && Slots.KindOf(other.Type) == kind)
				index++;
		}

		throw new InvalidOperationException($"`{column.Name}` is not a column of the row.");
	}

	/// <summary>One call: its values in their boxes, and the expressions over them.</summary>
	sealed class Call(object?[] values, Parameter[] parameters)
	{
		readonly Expression?[] _boxes = new Expression?[parameters.Length];

		/// <summary>
		/// A value of the call as the tree reads it: a field of a box of its own, one per parameter, so that the tree
		/// has one shape for every value and the provider reads it as a parameter of the query.
		/// </summary>
		Expression Box(Parameter parameter)
		{
			return _boxes[parameter.Index] ??= Holder(parameter.BoxType, Typed(parameter, values[parameter.Index]));
		}

		static object? Typed(Parameter parameter, object? value)
		{
			if (!parameter.IsList || value is null)
				return value;

			var items = (List<object?>)value;
			var array = Array.CreateInstance(parameter.BoxType.GetElementType()!, items.Count);

			for (var i = 0; i < items.Count; i++)
				array.SetValue(items[i], i);

			return array;
		}

		static Expression Holder(Type type, object? value)
		{
			var box = Activator.CreateInstance(typeof(StrongBox<>).MakeGenericType(type), value)!;

			return Expression.Field(Expression.Constant(box), nameof(StrongBox<object>.Value));
		}

		/// <summary>A column of a row of <paramref name="shape"/>, from its slot.</summary>
		public Expression Read(Column column, ParameterExpression row, Shape shape)
		{
			var scalars = shape.Kind == ShapeKind.EntityRow ? Expression.Field(row, nameof(QlRow<object>.S)) : (Expression)row;
			var slot    = Expression.Field(scalars, Slot(shape, column));

			return slot.Type == column.Type.Clr ? slot : Expression.Convert(slot, column.Type.Clr);
		}

		public Expression Value(BoundExpr value, ParameterExpression row, Shape shape)
		{
			switch (value)
			{
				case BoundField field:
				{
					var entity = shape.Kind == ShapeKind.EntityRow ? Expression.Field(row, nameof(QlRow<object>.E)) : (Expression)row;

					return Expression.MakeMemberAccess(entity, field.Field.Member);
				}

				case BoundColumn column:
					return Read(column.Column, row, shape);

				case BoundLiteral literal:
					return Expression.Constant(literal.Value, literal.Type.Clr);

				case BoundParameter parameter:
				{
					var box = Box(parameter.Parameter);

					return box.Type == parameter.Type.Clr ? box : Expression.Convert(box, parameter.Type.Clr);
				}

				case BoundConvert convert:
					return Expression.Convert(Value(convert.Operand, row, shape), convert.Type.Clr);

				case BoundUnary or BoundBinary when !value.ReadsRow && value.Type.IsNumeric:
					// Arithmetic that reads no row reads a parameter (the rest was computed at binding): it is computed
					// now, checked, and goes in a box of its own like a value.
					return Holder(value.Type.Clr, Evaluator.Evaluate(value, values));

				case BoundUnary unary:
				{
					var operand = Value(unary.Operand, row, shape);

					return unary.Kind switch
					{
						ExpressionType.Not           => Expression.Not(operand),
						ExpressionType.NegateChecked => Expression.NegateChecked(operand),
						_                            => Expression.Negate(operand),
					};
				}

				case BoundBinary binary:
				{
					var left  = Value(binary.Left,  row, shape);
					var right = Value(binary.Right, row, shape);

					if (binary.ComparesStrings)
						return Expression.MakeBinary(binary.Kind, Expression.Call(CompareMethod, left, right), Expression.Constant(0));

					return Expression.MakeBinary(binary.Kind, left, right);
				}

				case BoundIn @in:
				{
					var operand = Value(@in.Operand, row, shape);
					var list    = @in.List is { } parameter ? Box(parameter.Parameter) : Expression.NewArrayInit(operand.Type, @in.Items!.Select(i => Value(i, row, shape)));

					return Expression.Call(ContainsMethod.MakeGenericMethod(operand.Type), list, operand);
				}

				case BoundMember member:
					return Member(member, row, shape);

				default:
					throw new InvalidOperationException(value.GetType().Name);
			}
		}

		Expression Member(BoundMember member, ParameterExpression row, Shape shape)
		{
			var target = Value(member.Target, row, shape);

			// A part of a date that may be null is the part of its value, lifted: `(int?)created.Value.Year`.
			if (Nullable.GetUnderlyingType(target.Type) is not null)
				target = Expression.Property(target, nameof(Nullable<int>.Value));

			Expression made = member.Member is PropertyInfo property
				? Expression.Property(target, property)
				: Expression.Call(target, (MethodInfo)member.Member, member.Arguments.Select(a => Value(a, row, shape)));

			return made.Type == member.Type.Clr ? made : Expression.Convert(made, member.Type.Clr);
		}

		/// <summary>An aggregate over the group <paramref name="group"/> of rows of <paramref name="shape"/>.</summary>
		public Expression Aggregate(BoundAggregate aggregate, ParameterExpression group, Type element, Shape shape)
		{
			if (aggregate.Kind == AggregateKind.CountAll)
				return Expression.Call(Enumerables.Count.MakeGenericMethod(element), group);

			var row      = Expression.Parameter(element, "x");
			var argument = Value(aggregate.Argument!, row, shape);

			switch (aggregate.Kind)
			{
				case AggregateKind.Count:
				{
					if (argument.Type.IsValueType && Nullable.GetUnderlyingType(argument.Type) is null)
						return Expression.Call(Enumerables.Count.MakeGenericMethod(element), group);

					var present = Expression.Lambda(Expression.NotEqual(argument, Expression.Constant(null, argument.Type)), row);

					return Expression.Call(Enumerables.CountWhere.MakeGenericMethod(element), group, present);
				}

				case AggregateKind.Sum:
				case AggregateKind.Average:
				{
					// Over the nullable type the result has: an integer's average is a double, so that a database that
					// averages integers in integers averages these as C# does.
					var type     = aggregate.Type.Clr;
					var selector = Expression.Lambda(argument.Type == type ? argument : Expression.Convert(argument, type), row);
					var method   = Enumerables.Numeric(aggregate.Kind == AggregateKind.Sum ? nameof(Enumerable.Sum) : nameof(Enumerable.Average), type);

					return Expression.Call(method.MakeGenericMethod(element), group, selector);
				}

				default:
				{
					var type     = aggregate.Type.Clr;
					var selector = Expression.Lambda(argument.Type == type ? argument : Expression.Convert(argument, type), row);
					var method   = aggregate.Kind == AggregateKind.Min ? Enumerables.Min : Enumerables.Max;

					return Expression.Call(method.MakeGenericMethod(element, type), group, selector);
				}
			}
		}
	}

	/// <summary>The methods of <see cref="Enumerable"/> an aggregate is built of.</summary>
	static class Enumerables
	{
		public static readonly MethodInfo Count      = Find(nameof(Enumerable.Count), 1, null);
		public static readonly MethodInfo CountWhere = Find(nameof(Enumerable.Count), 2, null);
		public static readonly MethodInfo Min        = Find(nameof(Enumerable.Min),   2, m => m.GetGenericArguments().Length == 2);
		public static readonly MethodInfo Max        = Find(nameof(Enumerable.Max),   2, m => m.GetGenericArguments().Length == 2);

		/// <summary><c>Sum</c> or <c>Average</c> with a selector returning <paramref name="result"/>.</summary>
		public static MethodInfo Numeric(string name, Type result)
		{
			return Find(name, 2, m => m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1 && m.GetParameters()[1].ParameterType.GetGenericArguments()[1] == result);
		}

		static MethodInfo Find(string name, int parameters, Func<MethodInfo, bool>? shape)
		{
			return typeof(Enumerable).GetMethods().Single(m => m.Name == name && m.GetParameters().Length == parameters && (shape is null || shape(m)) && (shape is not null || m.GetGenericArguments().Length == 1));
		}
	}
}
