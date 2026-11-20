using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotGram.QueryLanguage;

/// <summary>
/// The final projection of <c>QueryJsonRow</c>: <c>End(Kind(…Kind(Start(anchor, names, formats), c1)…, cn))</c>, a
/// call of <see cref="QlJson"/> a column, in the order of the columns.
/// </summary>
static class JsonProjection
{
	static readonly MethodInfo StartMethod = typeof(QlJson).GetMethod(nameof(QlJson.Start))!;
	static readonly MethodInfo EndMethod   = typeof(QlJson).GetMethod(nameof(QlJson.End))!;

	/// <summary>
	/// The row of <paramref name="columns"/>, the values of <paramref name="output"/> as the tree reads them.
	/// <paramref name="formats"/> are the model's, by type.
	/// </summary>
	public static Expression Row(ParameterExpression row, BoundExpr[] output, Expression[] columns, string[] names, QlJsonFormat format, IReadOnlyDictionary<Type, Delegate> formats)
	{
		// The formats the row uses, one a formatted column, in column order: one constant beside the anchor, so the
		// provider reads one value a row however many columns are formatted.
		var used = new List<Delegate>();

		foreach (var column in columns)
			if (formats.TryGetValue(Core(column.Type), out var known))
				used.Add(known);

		var anchor = Anchor(row, output, columns);

		Expression chain = Expression.Call(
			StartMethod.MakeGenericMethod(anchor.Type),
			anchor,
			Expression.Constant(format == QlJsonFormat.Object ? names.Select(n => QlJson.Quote(n) + ":").ToArray() : null, typeof(string[])),
			Expression.Constant(used.Count > 0 ? used.ToArray() : null, typeof(Delegate[])));

		for (var i = 0; i < columns.Length; i++)
			chain = Write(chain, output[i], columns[i], formats);

		return Expression.Call(EndMethod, chain);
	}

	static Type Core(Type type)
	{
		return Nullable.GetUnderlyingType(type) ?? type;
	}

	/// <summary>
	/// What the row's first call depends on: the first column that reads the row and is of a value type, which a provider
	/// reads a second time without allocating; a string or another reference only where no column is a value; and the
	/// row itself where no column reads it. Such a row is always the values of a <c>select</c> of literals and values of
	/// the call; with a constant there, linq2db reads the whole chain as one value and fails to convert it.
	/// </summary>
	static Expression Anchor(ParameterExpression row, BoundExpr[] output, Expression[] columns)
	{
		Expression? best = null;
		var         rank = int.MaxValue;

		for (var i = 0; i < columns.Length; i++)
		{
			if (!DependsOnRow(output[i]))
				continue;

			var type = columns[i].Type;
			var mine = !type.IsValueType ? 2 : output[i].Type.Type == QlType.Other ? 1 : 0;

			if (mine < rank)
			{
				best = columns[i];
				rank = mine;
			}
		}

		return best ?? row;
	}

	/// <summary>Whether a value reads the row, through the columns it names too: a column computed from a literal does not.</summary>
	static bool DependsOnRow(BoundExpr value)
	{
		return value switch
		{
			BoundColumn column             => DependsOnRow(column.Column.Value),
			BoundField or BoundAggregate   => true,
			BoundLiteral or BoundParameter => false,
			_                              => Reads.Children(value).Any(DependsOnRow),
		};
	}

	static Expression Write(Expression state, BoundExpr bound, Expression value, IReadOnlyDictionary<Type, Delegate> formats)
	{
		var type     = Core(value.Type);
		var nullable = type != value.Type;

		if (formats.ContainsKey(type))
			return Call(nullable ? nameof(QlJson.FormattedNullable) : nameof(QlJson.Formatted), type, state, value);

		if (bound.Type.Type != QlType.Other)
		{
			var method = typeof(QlJson).GetMethod(bound.Type.Type.ToString(), [typeof(QlJsonState), value.Type])
				?? throw new InvalidOperationException($"No writer for {value.Type}.");

			return Expression.Call(method, state, value);
		}

		if (typeof(IFormattable).IsAssignableFrom(type))
			return Call(nullable ? nameof(QlJson.FormattableNullable) : nameof(QlJson.Formattable), type, state, value);

		return Call(nullable ? nameof(QlJson.PrintedNullable) : nameof(QlJson.Printed), type, state, value);
	}

	static Expression Call(string name, Type type, Expression state, Expression value)
	{
		return Expression.Call(typeof(QlJson).GetMethod(name)!.MakeGenericMethod(type), state, value);
	}
}
