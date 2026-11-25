using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

using DotGram.QueryLanguage.Syntax;

namespace DotGram.QueryLanguage;

/// <summary>
/// Resolves a text against the model. A name is a field of the entity, a <c>let</c> or a column of the last
/// <c>select</c>, and nothing else; a field is used only where its rights allow, and where they do not it is
/// reported exactly as a name that is not there.
/// </summary>
sealed partial class Binder
{
	Binder(ModelEntity entity, LambdaExpression? like, QlOptions options)
	{
		_entity  = entity;
		_like    = like;
		_options = options;
		_shape   = new Shape(ShapeKind.Entity, []);
	}

	readonly ModelEntity                    _entity;
	readonly LambdaExpression?              _like;
	readonly QlOptions                      _options;
	readonly Dictionary<string, Parameter>  _parameters = new(Names.Comparer);
	readonly List<Parameter>                _order      = [];
	readonly List<Column>                   _declared   = [];

	/// <summary>The name the entity's fields may be written after: the alias, or the entity's own name.</summary>
	string _qualifier = "";

	/// <summary>The row the next clause reads.</summary>
	Shape _shape;

	/// <summary>Where the last <c>select</c> stands, and so whether the entity's fields are still in scope.</summary>
	int? _selectAt;

	/// <summary>Binds <paramref name="query"/> to its entity in <paramref name="entities"/>, held to <paramref name="options"/>.</summary>
	/// <param name="query">The text as read.</param>
	/// <param name="entities">The model's entities by name.</param>
	/// <param name="like">The host's LIKE, <c>(s, p) =&gt; …</c>; null where the model declares none and <c>like</c> is refused.</param>
	/// <param name="options">The bounds of the query.</param>
	public static BoundQuery Bind(QuerySyntax query, IReadOnlyDictionary<string, ModelEntity> entities, LambdaExpression? like, QlOptions options)
	{
		if (!entities.TryGetValue(query.Root.Text, out var entity))
			throw new QlException(QlErrorCode.UnknownName, $"Unknown entity `{query.Root.Text}`.", query.Root.Position);

		return new Binder(entity, like, options).BindQuery(query);
	}

	static QlException Error(QlErrorCode code, string message, int position)
	{
		return new QlException(code, message, position);
	}

	BoundQuery BindQuery(QuerySyntax query)
	{
		_qualifier = query.Alias?.Text ?? _entity.Name;
		_needs     = RightsPlan.Of(query, _qualifier, _entity.Fields.Where(f => (f.Use & QlUse.Select) != 0).Select(f => f.Name), out _star);

		if (query.Alias is { } alias && _entity.Find(alias.Text) is { } field)
			throw Error(QlErrorCode.Syntax, $"`{field.Name}` is a field of `{_entity.Name}`: choose another name for the entity.", alias.Position);

		var clauses = new List<BoundClause>();
		var nodes   = 1L;

		foreach (var clause in query.Clauses)
		{
			clauses.Add(BindClause(clause));

			// The size of the text with each name counted as what it names, at every use: a chain of `let`s that
			// each name the one before twice is small to write and doubles at every step once the names are replaced.
			nodes += clause.Size;

			if (Saturated(nodes + _expansion) > _options.MaxNodes)
				throw Error(QlErrorCode.Limit, $"The query has more than {_options.MaxNodes} parts, counting each name at every use as what it names.", clause.Position);
		}

		CheckSkips(query.Clauses);

		var (output, names) = Output(query);

		foreach (var value in output)
			Use(value, QlUse.Select);

		var warnings = _declared
			.Where(c => !c.Used)
			.OrderBy(c => c.Position)
			.Select(c => new QlWarning(QlWarningCode.UnusedName, $"`{c.Name}` is never used.", c.Position))
			.ToArray();

		foreach (var parameter in _order)
			if (parameter.Type is null)
				throw Error(QlErrorCode.Type, $"The type of `@{parameter.Name}` is not known: compare it with a field.", parameter.Position);

		return new BoundQuery(_entity, [.. clauses], _shape, output, names, [.. _order], warnings);
	}

	/// <summary>The columns a row returns: the last <c>select</c>'s, or the entity's fields with <see cref="QlUse.Select"/>.</summary>
	(BoundExpr[] Output, string[] Names) Output(QuerySyntax query)
	{
		if (_shape.Kind == ShapeKind.Scalars)
		{
			var columns = _finalColumns!;

			return ([.. columns.Select(c => (BoundExpr)new BoundColumn(c, c.Position))], [.. columns.Select(c => c.Name)]);
		}

		var fields = _entity.Fields.Where(f => (f.Use & QlUse.Select) != 0).ToArray();

		if (fields.Length == 0)
			throw Error(QlErrorCode.Type, $"`{_entity.Name}` has no fields to return.", query.Root.Position);

		if (fields.Length > _options.MaxColumns)
			throw Error(QlErrorCode.Limit, $"`{_entity.Name}` has more than {_options.MaxColumns} fields to return: list them in `select`.", query.Root.Position);

		return ([.. fields.Select(f => (BoundExpr)new BoundField(f, f.Name, query.Root.Position, null))], [.. fields.Select(f => f.Name)]);
	}

	/// <summary>What naming columns has added to the size of the text so far: each use of a name, its column's size less one.</summary>
	long _expansion;

	static long Saturated(long value)
	{
		return Math.Min(value, int.MaxValue);
	}

	/// <summary>The columns of the last <c>select</c>, which the result returns where nothing replaces them.</summary>
	Column[]? _finalColumns;

	BoundClause BindClause(ClauseSyntax clause)
	{
		switch (clause)
		{
			case WhereClause where:
			{
				var condition = BindValue(where.Condition, null);

				if (condition.Type.Type != QlType.Bool)
					throw Error(QlErrorCode.Type, $"`where` takes a condition, and this is a {condition.Type.Name}.", where.Condition.Position);

				Use(condition, QlUse.Filter);

				return new BoundWhere(_shape, condition, where.Position);
			}

			case LetClause let:
				return BindLet(let);

			case OrderClause order:
			{
				var keys = new BoundOrderKey[order.Items.Length];

				for (var i = 0; i < keys.Length; i++)
				{
					var item = order.Items[i];
					var key  = BindValue(item.Key, null);

					Use(key, QlUse.Sort);

					keys[i] = new BoundOrderKey(key, item.Descending, item.NullsFirst);
				}

				return new BoundOrder(_shape, keys, order.Position);
			}

			case SelectClause select:
				return BindSelect(select);

			case PageClause page:
				return new BoundPage(_shape, page.Skip, BindCount(page), page.Position);

			default:
				throw new InvalidOperationException(clause.GetType().Name);
		}
	}

	BoundClause BindLet(LetClause let)
	{
		var name = let.Name.Text;

		if (Names.Comparer.Equals(name, _qualifier) && _selectAt is null)
			throw Error(QlErrorCode.Syntax, $"`{name}` is the entity's name here: call the `let` something else.", let.Name.Position);

		if (Find(name) is { } existing)
			throw Error(QlErrorCode.Syntax, $"`{existing.Name}` is already a name here: call the `let` something else.", let.Name.Position);

		var mark   = _expansion;
		var value  = BindValue(let.Value, null);
		var column = new Column(name, let.Name.Position, value) { Expanded = Saturated(let.Value.Size + _expansion - mark) };
		var before = _shape;

		CheckSlots([.. before.Columns, column], let.Name.Position);

		_declared.Add(column);
		_shape = new Shape(before.Kind == ShapeKind.Scalars ? ShapeKind.Scalars : ShapeKind.EntityRow, [.. before.Columns, column]);

		return new BoundLet(before, _shape, column, let.Position);
	}

	BoundClause BindSelect(SelectClause select)
	{
		var before  = _shape;
		var columns = new List<Column>();
		var names   = new HashSet<string>(Names.Comparer);

		if (select.Items is null)
		{
			// `*`: the columns of the row as it stands — the fields with Select before any `select`, the columns
			// of the last one after it.
			if (_selectAt is null)
			{
				// A field the rest of the text uses without its right is left out, so that naming it later fails as a
				// name the `*` never gave: as a field the model does not have would.
				foreach (var field in _entity.Fields)
					if ((field.Use & QlUse.Select) != 0 && (!_star.TryGetValue((select, field.Name), out var needs) || (field.Use & needs) == needs))
						columns.Add(new Column(field.Name, select.Position, new BoundField(field, field.Name, select.Position, null)));
			}
			else
			{
				foreach (var column in _finalColumns!)
				{
					columns.Add(new Column(column.Name, select.Position, new BoundColumn(column, select.Position)) { Expanded = column.Expanded });

					_expansion = Saturated(_expansion + column.Expanded);
				}
			}

			if (columns.Count == 0)
				throw Error(QlErrorCode.Type, $"`{_entity.Name}` has no fields to return.", select.Position);

			if (columns.Count > _options.MaxColumns)
				throw Error(QlErrorCode.Limit, $"`*` names more than {_options.MaxColumns} columns: list them.", select.Position);

			foreach (var column in columns)
				names.Add(column.Name);
		}
		else
		{
			var aggregates = select.Items.Count(i => i.Value is CallExpr call && Aggregates.Is(call.Name.Text));

			if (aggregates > 0 && aggregates < select.Items.Length)
			{
				var plain = select.Items.First(i => !(i.Value is CallExpr call && Aggregates.Is(call.Name.Text)));

				throw Error(QlErrorCode.Type, $"`{Describe(plain.Value)}` is not aggregated: with an aggregate, every column of a `select` is one (group by comes later).", plain.Value.Position);
			}

			foreach (var item in select.Items)
			{
				var mark  = _expansion;
				var value = aggregates > 0 ? BindAggregate((CallExpr)item.Value) : BindValue(item.Value, null);
				var name  = item.Alias?.Text ?? DefaultName(item.Value);

				if (name is null)
					throw Error(QlErrorCode.Syntax, $"Name this column: `{Describe(item.Value)} as x`.", Start(item.Value));

				if (!names.Add(name))
					throw Error(QlErrorCode.Syntax, $"`{name}` names two columns of this `select`: rename one with `as`.", item.Alias?.Position ?? Start(item.Value));

				columns.Add(new Column(name, item.Alias?.Position ?? Start(item.Value), value) { Expanded = Saturated(item.Value.Size + _expansion - mark) });
			}
		}

		CheckSlots(columns, select.Position);

		foreach (var column in columns)
			_declared.Add(column);

		_selectAt     = select.Position;
		_finalColumns = [.. columns];
		_shape        = new Shape(ShapeKind.Scalars, [.. columns]);

		return new BoundSelect(before, _shape, select.Items is not null && select.Items.Length > 0 && select.Items[0].Value is CallExpr first && Aggregates.Is(first.Name.Text), select.Position);
	}

	/// <summary>A column named by what it reads: a name, or a field after the alias. Anything else is given its name.</summary>
	string? DefaultName(ExprSyntax value)
	{
		return value switch
		{
			NameExpr name                                                                  => name.Name.Text,
			MemberExpr { Target: NameExpr, Arguments: null } member when IsQualifier(member) => member.Name.Text,
			CallExpr call when Aggregates.Is(call.Name.Text)                               => Aggregates.Name(call.Name.Text),
			_                                                                              => null,
		};
	}

	/// <summary>Where an expression starts in the text: its first operand's start, where its own position is its operator's.</summary>
	static int Start(ExprSyntax value)
	{
		return value switch
		{
			BinaryExpr binary     => Start(binary.Left),
			MemberExpr member     => Start(member.Target),
			LogicalExpr logical   => Start(logical.Items[0]),
			IsNullExpr isNull     => Start(isNull.Operand),
			InExpr @in            => Start(@in.Operand),
			BetweenExpr between   => Start(between.Operand),
			LikeExpr like         => Start(like.Operand),
			_                     => value.Position,
		};
	}

	/// <summary>The text of a short expression, for a message; a long one is shortened.</summary>
	static string Describe(ExprSyntax value)
	{
		return value switch
		{
			NameExpr name                                  => name.Name.Text,
			MemberExpr { Target: NameExpr target } member  => target.Name.Text + "." + member.Name.Text + (member.Arguments is null ? "" : "(…)"),
			CallExpr call                                  => call.Name.Text + "(…)",
			_                                              => "…",
		};
	}

	/// <summary>A row holds at most so many values of one kind: the carriers have that many slots of each.</summary>
	void CheckSlots(IEnumerable<Column> columns, int position)
	{
		var full = columns.GroupBy(c => Slots.KindOf(c.Type)).FirstOrDefault(g => g.Count() > Slots.PerKind);

		if (full is not null)
			throw Error(QlErrorCode.Limit, $"More than {Slots.PerKind} values of one type are in a row here.", position);
	}

	BoundExpr BindCount(PageClause page)
	{
		var word = page.Skip ? "skip" : "take";

		switch (page.Count)
		{
			case LiteralExpr literal:
			{
				if (!int.TryParse(literal.Text, NumberStyles.None, CultureInfo.InvariantCulture, out var count))
					throw Error(QlErrorCode.Limit, $"`{word}` takes a count up to {int.MaxValue}.", literal.Position);

				return new BoundLiteral(count, ScalarType.Of(QlType.Int, false), literal.Position);
			}

			case ParameterExpr parameter:
			{
				var bound = Parameter(parameter, ScalarType.Of(QlType.Int, false), nullable: false);

				bound.Parameter.IsCount = true;

				return bound;
			}

			default:
				throw new InvalidOperationException();
		}
	}

	/// <summary>The skips the text writes, counted now against <see cref="QlOptions.MaxSkip"/>; with a parameter's the call counts them again.</summary>
	void CheckSkips(ClauseSyntax[] clauses)
	{
		var skipped = 0L;

		foreach (var clause in clauses)
		{
			if (clause is PageClause { Skip: true, Count: LiteralExpr literal } page)
			{
				skipped += int.Parse(literal.Text, CultureInfo.InvariantCulture);

				if (skipped > _options.MaxSkip)
					throw Error(QlErrorCode.Limit, $"The query skips more than {_options.MaxSkip} rows.", page.Position);
			}
		}
	}

	// ── Names ───────────────────────────────────────────────────────────────

	/// <summary>A <c>let</c> or a column in scope by that name.</summary>
	Column? Find(string name)
	{
		var columns = _shape.Columns;

		for (var i = columns.Length - 1; i >= 0; i--)
			if (Names.Comparer.Equals(columns[i].Name, name))
				return columns[i];

		return null;
	}

	bool IsQualifier(MemberExpr member)
	{
		return member.Target is NameExpr target && _selectAt is null && Names.Comparer.Equals(target.Name.Text, _qualifier) && Find(target.Name.Text) is null;
	}

	BoundExpr ResolveName(NameSyntax name)
	{
		if (Find(name.Text) is { } column)
		{
			_expansion = Saturated(_expansion + column.Expanded - 1);

			return new BoundColumn(column, name.Position);
		}

		if (_selectAt is { } at)
			throw Error(QlErrorCode.UnknownName, $"`{name.Text}` is not in the `select` at {at}.", name.Position);

		if (Names.Comparer.Equals(name.Text, _qualifier))
			throw Error(QlErrorCode.Type, $"`{name.Text}` is the entity: use one of its fields, `{name.Text}.name`.", name.Position);

		if (_entity.Find(name.Text) is { } field && Allowed(field, name))
			return new BoundField(field, name.Text, name.Position, null);

		throw Unknown(name.Text, name.Position, null);
	}

	/// <summary>The rights each field the text names needs where it stands, known before anything is bound.</summary>
	Dictionary<NameSyntax, QlUse> _needs = [];

	/// <summary>The rights each field a <c>*</c> before any <c>select</c> takes will need, by that <c>select</c> and the field.</summary>
	Dictionary<(SelectClause, string), QlUse> _star = [];

	/// <summary>
	/// Whether <paramref name="field"/> has every right the text needs of it at <paramref name="name"/>. Asked where
	/// the field is resolved, so that a field without its right fails there, as a field the model lacks would.
	/// </summary>
	bool Allowed(ModelField field, NameSyntax name)
	{
		return !_needs.TryGetValue(name, out var needs) || (field.Use & needs) == needs;
	}

	/// <summary>
	/// The error for a name that is not there — and, word for word and at the same place, for a field used where
	/// its rights do not reach, so that the error never tells which.
	/// </summary>
	QlException Unknown(string name, int position, string? qualifier)
	{
		return qualifier is null
			? Error(QlErrorCode.UnknownName, $"Unknown name `{name}`.", position)
			: Error(QlErrorCode.UnknownName, $"`{qualifier}` has no field `{name}`.", position);
	}

	/// <summary>Checks that every field <paramref name="value"/> reads may be used for <paramref name="right"/>, and marks the columns it reads used.</summary>
	void Use(BoundExpr value, QlUse right)
	{
		switch (value)
		{
			case BoundField field:
				if ((field.Field.Use & right) == 0)
					throw Unknown(field.Written, field.Position, field.Qualifier);

				return;

			case BoundColumn column:
				column.Column.Used = true;

				if ((column.Column.Checked & right) != right)
				{
					column.Column.Checked |= right;

					Use(column.Column.Value, right);
				}

				return;

			case BoundParameter { Parameter.IsList: true }:
				return;
		}

		foreach (var child in Reads.Children(value))
			Use(child, right);
	}

	// ── Parameters ──────────────────────────────────────────────────────────

	/// <summary>A parameter where a value of <paramref name="type"/> stands; one name stands where one type does, every time.</summary>
	BoundParameter Parameter(ParameterExpr syntax, ScalarType type, bool nullable, bool list = false)
	{
		if (!_parameters.TryGetValue(syntax.Name, out var parameter))
		{
			parameter = new Parameter(syntax.Name, syntax.Position, _order.Count);

			_parameters.Add(syntax.Name, parameter);
			_order.Add(parameter);
		}

		if (parameter.Type is { } known)
		{
			if (known.Type != type.Type || known.Core != type.Core || parameter.IsList != list)
				throw Error(QlErrorCode.Type, $"`@{syntax.Name}` stands for {Kind(known, parameter.IsList)} and for {Kind(type, list)}.", syntax.Position);
		}
		else
		{
			parameter.Type   = type.WithNullable(false);
			parameter.IsList = list;
		}

		parameter.Nullable &= nullable;

		return new BoundParameter(parameter, type.WithNullable(nullable && type.Nullable), syntax.Position);

		static string Kind(ScalarType type, bool list)
		{
			return list ? $"a list of {type.Name}" : $"a {type.Name}";
		}
	}
}
