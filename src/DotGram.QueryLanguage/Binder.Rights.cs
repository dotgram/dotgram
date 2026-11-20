using System.Collections.Generic;

using DotGram.QueryLanguage.Syntax;

namespace DotGram.QueryLanguage;

/// <summary>
/// The rights each field a text names will need where it stands, worked out from the text alone before it is bound:
/// a field in a <c>where</c> needs <see cref="QlUse.Filter"/>, in an <c>order by</c> <see cref="QlUse.Sort"/>, in the
/// result <see cref="QlUse.Select"/>, and in a <c>let</c> or a <c>select</c> column whatever the places that read that
/// column need. The binder checks them where it resolves the field, so a field without its right fails exactly where
/// and as a field the model does not have would — before any error of type the rest of the text might hold.
/// </summary>
/// <remarks>
/// It reads names as the binder does — a <c>let</c> or a column in scope before a field, the alias before a
/// <c>select</c> only — and needs nothing else: a name it cannot place is the binder's to refuse.
/// </remarks>
static class RightsPlan
{
	/// <summary>A <c>let</c> or a <c>select</c> column: the rights the places reading it need, and the columns it reads.</summary>
	sealed class Declared
	{
		public QlUse                Needs;
		public readonly List<Declared> Reads = [];
	}

	/// <summary>Where a field stands: a place that needs a right of its own, or a column, which needs what its readers need.</summary>
	readonly struct Place(QlUse right, Declared? column)
	{
		public QlUse     Right  { get; } = right;
		public Declared? Column { get; } = column;
	}

	/// <summary>The rights each field occurrence of <paramref name="query"/> needs, by its name in the text.</summary>
	public static Dictionary<NameSyntax, QlUse> Of(QuerySyntax query, string qualifier)
	{
		var fields   = new List<(NameSyntax Name, Place Place)>();
		var declared = new List<Declared>();
		var scope    = new Dictionary<string, Declared>(Names.Comparer);
		var selected = false;

		List<Declared>? last = null;

		void Walk(ExprSyntax node, Place place, Declared? into)
		{
			switch (node)
			{
				case NameExpr name:
					// A column named in a column passes on what that one needs; named at a place, it needs the place's right.
					if (scope.TryGetValue(name.Name.Text, out var column))
					{
						if (into is not null)
							into.Reads.Add(column);
						else
							column.Needs |= place.Right;
					}
					else if (!selected && !Names.Comparer.Equals(name.Name.Text, qualifier))
						fields.Add((name.Name, place));

					return;

				case MemberExpr { Target: NameExpr target, Arguments: null } member when !selected && Names.Comparer.Equals(target.Name.Text, qualifier) && !scope.ContainsKey(target.Name.Text):
					fields.Add((member.Name, place));
					return;

				case MemberExpr member:
					Walk(member.Target, place, into);

					foreach (var argument in member.Arguments ?? [])
						Walk(argument, place, into);

					return;

				case CallExpr call:
					foreach (var argument in call.Arguments)
						Walk(argument, place, into);

					return;

				case ParenthesizedExpr parenthesized:
					Walk(parenthesized.Inner, place, into);
					return;

				case UnaryExpr unary:
					Walk(unary.Operand, place, into);
					return;

				case BinaryExpr binary:
					Walk(binary.Left,  place, into);
					Walk(binary.Right, place, into);
					return;

				case LogicalExpr logical:
					foreach (var item in logical.Items)
						Walk(item, place, into);

					return;

				case IsNullExpr isNull:
					Walk(isNull.Operand, place, into);
					return;

				case BetweenExpr between:
					Walk(between.Operand, place, into);
					Walk(between.Low,     place, into);
					Walk(between.High,    place, into);
					return;

				case InExpr @in:
					Walk(@in.Operand, place, into);

					foreach (var item in @in.Items ?? [])
						Walk(item, place, into);

					return;
			}
		}

		Declared Declare(ExprSyntax value)
		{
			var column = new Declared();

			declared.Add(column);
			Walk(value, new Place(0, column), column);

			return column;
		}

		foreach (var clause in query.Clauses)
		{
			switch (clause)
			{
				case WhereClause where:
					Walk(where.Condition, new Place(QlUse.Filter, null), null);
					break;

				case OrderClause order:
					foreach (var item in order.Items)
						Walk(item.Key, new Place(QlUse.Sort, null), null);

					break;

				case LetClause let:
				{
					var column = Declare(let.Value);

					if (!scope.ContainsKey(let.Name.Text))
						scope[let.Name.Text] = column;

					break;
				}

				case SelectClause select:
				{
					var columns = new Dictionary<string, Declared>(Names.Comparer);

					last = [];

					if (select.Items is null)
					{
						// `*` after a `select`: the same columns again, each reading the one it copies. Before one, the
						// fields with Select, which the binder takes by their right.
						if (selected)
						{
							foreach (var previous in scope)
							{
								var copy = new Declared();

								copy.Reads.Add(previous.Value);
								declared.Add(copy);
								columns[previous.Key] = copy;
								last.Add(copy);
							}
						}
					}
					else
					{
						foreach (var item in select.Items)
						{
							var column = Declare(item.Value);

							last.Add(column);

							if ((item.Alias?.Text ?? DefaultName(item.Value, qualifier, selected, scope)) is { } name && !columns.ContainsKey(name))
								columns[name] = column;
						}
					}

					scope    = columns;
					selected = true;
					break;
				}
			}
		}

		foreach (var column in last ?? [])
			column.Needs |= QlUse.Select;

		// A column reads only columns declared before it: from the last back, each passes on what it needs.
		for (var i = declared.Count - 1; i >= 0; i--)
			foreach (var read in declared[i].Reads)
				read.Needs |= declared[i].Needs;

		var needs = new Dictionary<NameSyntax, QlUse>();

		foreach (var (name, place) in fields)
			needs[name] = place.Column?.Needs ?? place.Right;

		return needs;
	}

	/// <summary>The name a <c>select</c> gives a column the text does not name, as the binder gives it.</summary>
	static string? DefaultName(ExprSyntax value, string qualifier, bool selected, Dictionary<string, Declared> scope)
	{
		return value switch
		{
			NameExpr name => name.Name.Text,
			MemberExpr { Target: NameExpr target, Arguments: null } member when !selected && Names.Comparer.Equals(target.Name.Text, qualifier) && !scope.ContainsKey(target.Name.Text) => member.Name.Text,
			CallExpr call when Aggregates.Is(call.Name.Text) => Aggregates.Name(call.Name.Text),
			_ => null,
		};
	}
}
