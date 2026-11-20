using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DotGram.QueryLanguage.Probes.Core;

/// <summary>
/// What the translator does: one <see cref="Queryable"/> call per clause, as written, onto the source the host gave.
/// A body is built from the row parameter by the caller, so the probes can name the internal carriers only through
/// these methods.
/// </summary>
public static class Chain
{
	static IQueryable Call(string method, IQueryable source, Type[] arguments, params Expression[] rest)
	{
		var call = Expression.Call(typeof(Queryable), method, arguments, [source.Expression, .. rest]);

		return source.Provider.CreateQuery(call);
	}

	static LambdaExpression Lambda(IQueryable source, Func<ParameterExpression, Expression> body, out Expression built)
	{
		var row = Expression.Parameter(source.ElementType, "r");

		built = body(row);

		return Expression.Lambda(built, row);
	}

	public static IQueryable Where(IQueryable source, Func<ParameterExpression, Expression> predicate)
	{
		var lambda = Lambda(source, predicate, out _);

		return Call("Where", source, [source.ElementType], Expression.Quote(lambda));
	}

	public static IQueryable Select(IQueryable source, Func<ParameterExpression, Expression> projection)
	{
		var lambda = Lambda(source, projection, out var body);

		return Call("Select", source, [source.ElementType, body.Type], Expression.Quote(lambda));
	}

	public static IQueryable OrderBy(IQueryable source, Func<ParameterExpression, Expression> key, bool descending = false)
	{
		var lambda = Lambda(source, key, out var body);

		return Call(descending ? "OrderByDescending" : "OrderBy", source, [source.ElementType, body.Type], Expression.Quote(lambda));
	}

	public static IQueryable ThenBy(IQueryable source, Func<ParameterExpression, Expression> key, bool descending = false)
	{
		var lambda = Lambda(source, key, out var body);

		return Call(descending ? "ThenByDescending" : "ThenBy", source, [source.ElementType, body.Type], Expression.Quote(lambda));
	}

	/// <summary><c>take</c> with a literal count: an int constant, as the binder folds it.</summary>
	public static IQueryable Take(IQueryable source, int count)
	{
		return Call("Take", source, [source.ElementType], Expression.Constant(count));
	}

	public static IQueryable Skip(IQueryable source, int count)
	{
		return Call("Skip", source, [source.ElementType], Expression.Constant(count));
	}

	/// <summary><c>take @n</c>: the count read from the value's box.</summary>
	public static IQueryable Take(IQueryable source, StrongBox<int> count)
	{
		return Call("Take", source, [source.ElementType], Value(count));
	}

	public static IQueryable Skip(IQueryable source, StrongBox<int> count)
	{
		return Call("Skip", source, [source.ElementType], Value(count));
	}

	/// <summary>A value of the call: one box per parameter, so the shape of the tree never depends on the value.</summary>
	public static Expression Value<T>(StrongBox<T> box)
	{
		return Expression.Field(Expression.Constant(box), nameof(StrongBox<T>.Value));
	}

	/// <summary><c>x in @list</c>: <c>Enumerable.Contains(box.Value, x)</c>.</summary>
	public static Expression In<T>(StrongBox<T[]> list, Expression item)
	{
		return Expression.Call(typeof(Enumerable), nameof(Enumerable.Contains), [typeof(T)], Value(list), item);
	}

	/// <summary>The whole-set aggregate: <c>GroupBy(r =&gt; 0)</c>, then a projection of the group.</summary>
	public static IQueryable GroupAll(IQueryable source)
	{
		var row    = Expression.Parameter(source.ElementType, "r");
		var lambda = Expression.Lambda(Expression.Constant(0), row);

		return Call("GroupBy", source, [source.ElementType, typeof(int)], Expression.Quote(lambda));
	}

	/// <summary>An aggregate over the group parameter <paramref name="group"/> (<c>IGrouping&lt;int, TRow&gt;</c>).</summary>
	public static Expression Aggregate(ParameterExpression group, string method, Func<ParameterExpression, Expression>? selector = null)
	{
		var element = group.Type.GetGenericArguments()[1];

		if (selector == null)
		{
			return Expression.Call(typeof(Enumerable), method, [element], group);
		}

		var row    = Expression.Parameter(element, "x");
		var body   = selector(row);
		var lambda = Expression.Lambda(body, row);

		if (method is "Min" or "Max")
		{
			return Expression.Call(typeof(Enumerable), method, [element, body.Type], group, lambda);
		}

		return Expression.Call(typeof(Enumerable), method, [element], group, lambda);
	}

	/// <summary>The final projection of <c>QueryData</c>: <c>new object?[] { (object?)c1, … }</c>.</summary>
	public static IQueryable<object?[]> Data(IQueryable source, Func<ParameterExpression, IEnumerable<Expression>> columns)
	{
		var projected = Select(source, r => Expression.NewArrayInit(typeof(object), columns(r).Select(c => c.Type == typeof(object) ? c : Expression.Convert(c, typeof(object)))));

		return (IQueryable<object?[]>)projected;
	}
}

/// <summary>Builds the internal row carriers into a tree.</summary>
public static class Carriers
{
	public static Type Scalars => typeof(QlScalars);

	public static Type Row(Type entity)
	{
		return typeof(QlRow<>).MakeGenericType(entity);
	}

	/// <summary>The slot name for a value of <paramref name="type"/> at <paramref name="index"/>: <c>Int3</c>, <c>DecimalN0</c>.</summary>
	public static string Slot(Type type, int index)
	{
		var nullable = Nullable.GetUnderlyingType(type);
		var core     = nullable ?? type;

		string kind;

		if      (core == typeof(bool))           kind = "Bool";
		else if (core == typeof(int))            kind = "Int";
		else if (core == typeof(long))           kind = "Long";
		else if (core == typeof(decimal))        kind = "Decimal";
		else if (core == typeof(double))         kind = "Double";
		else if (core == typeof(string))         return "String" + index;
		else if (core == typeof(DateTime))       kind = "Timestamp";
		else if (core == typeof(DateTimeOffset)) kind = "TimestampTz";
		else if (core == typeof(Guid))           kind = "Guid";
		else return "Other" + index;

		return kind + (nullable != null ? "N" : "") + index;
	}

	/// <summary><c>new QlScalars { Slot = value, … }</c>; a value outside the kinds goes to an object slot.</summary>
	public static Expression NewScalars(IEnumerable<(string Slot, Expression Value)> slots)
	{
		var bindings = slots.Select(s =>
		{
			var field = typeof(QlScalars).GetField(s.Slot)!;
			var value = s.Value.Type == field.FieldType ? s.Value : Expression.Convert(s.Value, field.FieldType);

			return (MemberBinding)Expression.Bind(field, value);
		});

		return Expression.MemberInit(Expression.New(typeof(QlScalars)), bindings);
	}

	/// <summary><c>new QlRow&lt;TE&gt; { E = entity, S = scalars }</c>.</summary>
	public static Expression NewRow(Expression entity, Expression scalars)
	{
		var type = Row(entity.Type);

		return Expression.MemberInit(
			Expression.New(type),
			Expression.Bind(type.GetField("E")!, entity),
			Expression.Bind(type.GetField("S")!, scalars));
	}

	/// <summary>Reads a slot of a scalar row (<c>r.Int3</c>) or of the scalars of an entity row (<c>r.S.Int3</c>).</summary>
	public static Expression Read(Expression row, string slot)
	{
		if (row.Type != typeof(QlScalars))
		{
			row = Expression.Field(row, "S");
		}

		return Expression.Field(row, slot);
	}

	/// <summary>The entity of an entity row: <c>r.E</c>.</summary>
	public static Expression Entity(Expression row)
	{
		return Expression.Field(row, "E");
	}
}

/// <summary>Builds the JSON chain of the final projection: <c>End(Kind(…Kind(Start(anchor), c1)…, cn))</c>.</summary>
public static class Json
{
	static MethodInfo Method(string name, params Type[] parameters)
	{
		return typeof(QlJson).GetMethod(name, BindingFlags.Public | BindingFlags.Static, parameters)
			?? throw new InvalidOperationException($"no QlJson.{name}({string.Join(", ", parameters.Select(p => p.Name))})");
	}

	static string Kind(Type type)
	{
		var core = Nullable.GetUnderlyingType(type) ?? type;

		if (core == typeof(bool))           return "Bool";
		if (core == typeof(int))            return "Int32";
		if (core == typeof(long))           return "Int64";
		if (core == typeof(decimal))        return "Decimal";
		if (core == typeof(double))         return "Double";
		if (core == typeof(string))         return "String";
		if (core == typeof(DateTime))       return "Timestamp";
		if (core == typeof(DateTimeOffset)) return "TimestampTz";
		if (core == typeof(Guid))           return "Guid";

		throw new ArgumentException($"{type} is not one of the nine kinds.");
	}

	static Expression Name(string? name)
	{
		return Expression.Constant(name == null ? null : "\"" + name + "\":", typeof(string));
	}

	public static Expression Start(Expression anchor, bool asObject)
	{
		var method = typeof(QlJson).GetMethod(asObject ? "StartObject" : "StartArray")!.MakeGenericMethod(anchor.Type);

		return Expression.Call(method, anchor);
	}

	public static Expression End(Expression state, bool asObject)
	{
		return Expression.Call(Method(asObject ? "EndObject" : "EndArray", typeof(QlJsonState)), state);
	}

	/// <summary>A value of one of the nine kinds.</summary>
	public static Expression Write(Expression state, string? name, Expression value)
	{
		var method = Method(Kind(value.Type), typeof(QlJsonState), typeof(string), value.Type);

		return Expression.Call(method, state, Name(name), value);
	}

	/// <summary>A value printed by a host format; the delegate is a constant of the tree.</summary>
	public static Expression Formatted(Expression state, string? name, Delegate format, Expression value)
	{
		var core = Nullable.GetUnderlyingType(value.Type);

		var method = core != null
			? typeof(QlJson).GetMethod(nameof(QlJson.FormattedNullable))!.MakeGenericMethod(core)
			: typeof(QlJson).GetMethod(nameof(QlJson.Formatted))!.MakeGenericMethod(value.Type);

		return Expression.Call(method, state, Name(name), Expression.Constant(format, format.GetType()), value);
	}

	/// <summary>The fallback: the tree carries the format's id, the delegate stays in a static table.</summary>
	public static Expression FormattedById(Expression state, string? name, int id, Delegate format, Expression value)
	{
		QlJson.Formats[id] = format;

		var method = typeof(QlJson).GetMethod(nameof(QlJson.FormattedById))!.MakeGenericMethod(value.Type);

		return Expression.Call(method, state, Name(name), Expression.Constant(id), value);
	}

	/// <summary>A value of a type outside the nine without a format: <c>ToString(null, invariant)</c> or <c>ToString()</c>.</summary>
	public static Expression Default(Expression state, string? name, Expression value)
	{
		var formattable = typeof(IFormattable).IsAssignableFrom(Nullable.GetUnderlyingType(value.Type) ?? value.Type);

		var method = typeof(QlJson).GetMethod(formattable ? nameof(QlJson.DefaultFormattable) : nameof(QlJson.DefaultToString))!.MakeGenericMethod(value.Type);

		return Expression.Call(method, state, Name(name), value);
	}

	/// <summary>
	/// The whole chain over the columns, in order. <paramref name="anchor"/> is the column the first link depends on;
	/// <paramref name="write"/> overrides how a column is written (by its index), for formats.
	/// </summary>
	public static Expression Row(Expression anchor, IReadOnlyList<(string Name, Expression Value)> columns, bool asObject, Func<int, Expression, string?, Expression, Expression?>? write = null)
	{
		var state = Start(anchor, asObject);

		for (var i = 0; i < columns.Count; i++)
		{
			var name = asObject ? columns[i].Name : null;

			state = write?.Invoke(i, state, name, columns[i].Value) ?? Write(state, name, columns[i].Value);
		}

		return End(state, asObject);
	}

	/// <summary>The object format with each name passed as an int id into a static table instead of a string constant.</summary>
	public static Expression ObjectRowWithNameIds(Expression anchor, IReadOnlyList<(string Name, Expression Value)> columns)
	{
		var state = Start(anchor, asObject: true);

		for (var i = 0; i < columns.Count; i++)
		{
			QlJson.Names[i] = "\"" + columns[i].Name + "\":";

			state = Expression.Call(Method(nameof(QlJson.NameById), typeof(QlJsonState), typeof(int)), state, Expression.Constant(i));
			state = Write(state, null, columns[i].Value);
		}

		return End(state, asObject: true);
	}

	/// <summary>The object format with all names in one array constant next to the root, read once per row.</summary>
	public static Expression ObjectRowWithNamesOnce(Expression anchor, IReadOnlyList<(string Name, Expression Value)> columns)
	{
		var names = columns.Select(c => "\"" + c.Name + "\":").ToArray();
		var state = Start(anchor, asObject: true);

		for (var i = 0; i < columns.Count; i++)
		{
			state = Write(state, null, columns[i].Value);
		}

		var set = Expression.Call(Method(nameof(QlJson.SetNames), typeof(string[])), Expression.Constant(names));

		return Expression.Call(Method(nameof(QlJson.EndObjectNamed), typeof(QlJsonState), typeof(QlJsonState)), set, state);
	}

	/// <summary>Rows started so far, in this process.</summary>
	public static long Starts => System.Threading.Interlocked.Read(ref QlJson.Starts);
}
