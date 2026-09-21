using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Handwritten;

using Typed = SqlTowers.Typed;

// ISO/IEC 9075-2:2023 §6.30–6.36, the value functions; §6.10 and §10.9, the window functions and
// the aggregates; §6.26, the row pattern navigation operations.
partial class HandSqlStandard
{
	/// <summary>
	/// A function of some tower, and the tower it is of: <c>ABS</c> is a number's and an interval's
	/// both, and a value function is a primary of whatever tower it names.
	/// </summary>
	static bool ValueFunction(ref SqlCursor cursor, out Typed value)
	{
		var save = cursor;

		switch (cursor.Word)
		{
			// <absolute value expression>, <interval absolute value function>.
			case SqlWord.Abs:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && CommonValueExpression(ref cursor, out var operand) &&
					(operand.Roles & (SqlTowers.Numeric | SqlTowers.Interval)) != 0 && cursor.Take(SqlTokenKind.RightParen))
				{
					value = new Typed(Called("ABS", operand.Node), operand.Roles & (SqlTowers.Numeric | SqlTowers.Interval));

					return true;
				}

				break;
			}

			case SqlWord.TrimArray:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Node(ref cursor, SqlTowers.Array, out var array) && cursor.Take(SqlTokenKind.Comma) &&
					Numeric(ref cursor, out var count) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = new Typed(Called("TRIM_ARRAY", array, count), SqlTowers.Array);

					return true;
				}

				break;
			}

			case SqlWord.Set:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Node(ref cursor, SqlTowers.Multiset, out var multiset) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = new Typed(Called("SET", multiset), SqlTowers.Multiset);

					return true;
				}

				break;
			}

			case SqlWord.Json:
			case SqlWord.JsonScalar:
			{
				if (JSONTypedValueFunction(ref cursor, out var json))
				{
					value = new Typed(json, SqlTowers.Json);

					return true;
				}

				break;
			}

			default:
			{
				if (NumericValueFunction(ref cursor, out var numeric))
				{
					value = new Typed(numeric, SqlTowers.Numeric);

					return true;
				}

				if (StringValueFunction(ref cursor, out var text))
				{
					value = new Typed(text, SqlTowers.String);

					return true;
				}

				if (DatetimeValueFunction(ref cursor, out var datetime))
				{
					value = new Typed(datetime, SqlTowers.Datetime);

					return true;
				}

				break;
			}
		}

		cursor = save;

		return Refuse(out value);
	}

	// ── §6.30 Numeric value function ───────────────────────────────────────────

	static bool NumericValueFunction(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;
		var word = cursor.Word;

		value = null!;

		switch (word)
		{
			case SqlWord.Position:
			{
				// <character position expression>, <binary position expression>.
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && CommonValueExpression(ref cursor, out var sought) && cursor.Take(SqlWord.In) &&
					CommonValueExpression(ref cursor, out var within))
				{
					var units = UsingUnits(ref cursor);

					if (SqlTowers.Characters(sought.Roles & within.Roles, units > 0) && cursor.Take(SqlTokenKind.RightParen))
					{
						value = new Expression.Position(sought.Node, within.Node, Units(units));

						return true;
					}
				}

				break;
			}

			case SqlWord.OccurrencesRegex:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && RegexSearch(ref cursor, out var search) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = Regex(RegexFunction.OccurrencesRegex, search);

					return true;
				}

				break;
			}

			case SqlWord.PositionRegex:
			{
				cursor.Take();

				if (!cursor.Take(SqlTokenKind.LeftParen))
					break;

				var where =
					cursor.TakeWord("START") ? RegexPositionStartOrAfter.Start :
					cursor.TakeWord("AFTER") ? RegexPositionStartOrAfter.After :
					(RegexPositionStartOrAfter?)null;

				if (RegexSearch(ref cursor, out var search))
				{
					var occurrence = Occurrence(ref cursor);
					var grouping   = Group(ref cursor);

					if (cursor.Take(SqlTokenKind.RightParen))
					{
						value = Regex(RegexFunction.PositionRegex, search) with { StartOrAfter = where, Occurrence = occurrence, Group = grouping };

						return true;
					}
				}

				break;
			}

			case SqlWord.Extract:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && ExtractField(ref cursor, out var field) && cursor.Take(SqlWord.From) &&
					CommonValueExpression(ref cursor, out var source) && (source.Roles & (SqlTowers.Datetime | SqlTowers.Interval)) != 0 &&
					cursor.Take(SqlTokenKind.RightParen))
				{
					value = new Expression.Extract(field, source.Node);

					return true;
				}

				break;
			}

			case SqlWord.CharLength:
			case SqlWord.CharacterLength:
			{
				var spelled = word == SqlWord.CharacterLength;

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Character(ref cursor, out var text))
				{
					CharacterLengthUnits? units = null;

					if (cursor.Take(SqlWord.Using))
					{
						units = Units(CharLengthUnits(ref cursor));

						if (units is null)
							break;
					}

					if (cursor.Take(SqlTokenKind.RightParen))
					{
						value = new Expression.Length(spelled ? LengthFunction.CharacterLength : LengthFunction.CharLength, text, units);

						return true;
					}
				}

				break;
			}

			case SqlWord.OctetLength:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Node(ref cursor, SqlTowers.String, out var text) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = new Expression.Length(LengthFunction.OctetLength, text, null);

					return true;
				}

				break;
			}

			case SqlWord.Cardinality:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Node(ref cursor, SqlTowers.Array | SqlTowers.Multiset, out var collection) &&
					cursor.Take(SqlTokenKind.RightParen))
				{
					value = Called("CARDINALITY", collection);

					return true;
				}

				break;
			}

			case SqlWord.ArrayMaxCardinality:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Node(ref cursor, SqlTowers.Array, out var array) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = Called("ARRAY_MAX_CARDINALITY", array);

					return true;
				}

				break;
			}

			case SqlWord.Mod:
			case SqlWord.Log:
			case SqlWord.Power:
			{
				var spelling = word == SqlWord.Mod ? "MOD" : word == SqlWord.Log ? "LOG" : "POWER";

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Numeric(ref cursor, out var first) && cursor.Take(SqlTokenKind.Comma) &&
					Numeric(ref cursor, out var second) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = Called(spelling, first, second);

					return true;
				}

				break;
			}

			case SqlWord.Sin:
			case SqlWord.Cos:
			case SqlWord.Tan:
			case SqlWord.Sinh:
			case SqlWord.Cosh:
			case SqlWord.Tanh:
			case SqlWord.Asin:
			case SqlWord.Acos:
			case SqlWord.Atan:
			case SqlWord.Log10:
			case SqlWord.Ln:
			case SqlWord.Exp:
			case SqlWord.Sqrt:
			case SqlWord.Floor:
			case SqlWord.Ceil:
			case SqlWord.Ceiling:
			{
				var spelling = cursor.TextOf(cursor.Token);

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Numeric(ref cursor, out var operand) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = Called(spelling.ToUpperInvariant(), operand);

					return true;
				}

				break;
			}

			case SqlWord.WidthBucket:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Numeric(ref cursor, out var operand) && cursor.Take(SqlTokenKind.Comma) &&
					Numeric(ref cursor, out var low) && cursor.Take(SqlTokenKind.Comma) && Numeric(ref cursor, out var high) &&
					cursor.Take(SqlTokenKind.Comma) && Numeric(ref cursor, out var count) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = Called("WIDTH_BUCKET", operand, low, high, count);

					return true;
				}

				break;
			}

			case SqlWord.MatchNumber:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = Called("MATCH_NUMBER");

					return true;
				}

				break;
			}
		}

		cursor = save;
		value  = null!;

		return false;
	}

	static bool ExtractField(ref SqlCursor cursor, out ExtractField field)
	{
		var read =
			cursor.Take(SqlWord.Year)            ? Ast.ExtractField.Year :
			cursor.Take(SqlWord.Month)           ? Ast.ExtractField.Month :
			cursor.Take(SqlWord.Day)             ? Ast.ExtractField.Day :
			cursor.Take(SqlWord.Hour)            ? Ast.ExtractField.Hour :
			cursor.Take(SqlWord.Minute)          ? Ast.ExtractField.Minute :
			cursor.Take(SqlWord.Second)          ? Ast.ExtractField.Second :
			cursor.Take(SqlWord.TimezoneHour)    ? Ast.ExtractField.TimezoneHour :
			cursor.Take(SqlWord.TimezoneMinute)  ? Ast.ExtractField.TimezoneMinute :
			(ExtractField?)null;

		field = read ?? default;

		return read is not null;
	}

	/// <summary>What the regular expression functions share: a pattern, its flags, the string searched, and where.</summary>
	static bool RegexSearch(ref SqlCursor cursor, out Search search)
	{
		var save = cursor;

		search = default;

		if (!Character(ref cursor, out var pattern))
			return false;

		Expression? flag = null;

		if (cursor.TakeWord("FLAG") && !Character(ref cursor, out flag))
		{
			cursor = save;

			return false;
		}

		if (!cursor.Take(SqlWord.In) || !Character(ref cursor, out var value))
		{
			cursor = save;

			return false;
		}

		Expression? from = null;

		if (cursor.Take(SqlWord.From) && !Numeric(ref cursor, out from))
		{
			cursor = save;

			return false;
		}

		CharacterLengthUnits? units = null;

		if (cursor.Take(SqlWord.Using))
		{
			units = Units(CharLengthUnits(ref cursor));

			if (units is null)
			{
				cursor = save;

				return false;
			}
		}

		search = new Search(pattern, flag, value, from, units);

		return true;
	}

	/// <summary>What a regular expression function searches: a pattern, its flags, the string, where to start, and the units.</summary>
	readonly record struct Search(Expression Pattern, Expression? Flag, Expression Value, Expression? From, CharacterLengthUnits? Using);

	static Expression.Regex Regex(RegexFunction function, Search search)
	{
		return new(function, search.Pattern, search.Value) { Flag = search.Flag, From = search.From, Using = search.Using };
	}

	static Expression? Occurrence(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.TakeWord("OCCURRENCE") && Numeric(ref cursor, out var value))
			return value;

		cursor = save;

		return null;
	}

	static Expression? Group(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Group) && Numeric(ref cursor, out var value))
			return value;

		cursor = save;

		return null;
	}

	/// <summary>Which units were named: none (0), characters (1), octets (2).</summary>
	static int UsingUnits(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Using))
		{
			var units = CharLengthUnits(ref cursor);

			if (units > 0)
				return units;

			cursor = save;
		}

		return 0;
	}

	static int CharLengthUnits(ref SqlCursor cursor)
	{
		return cursor.TakeWord("CHARACTERS") ? 1 :
		cursor.TakeWord("OCTETS") ? 2 :
		0;
	}

	/// <summary>The same units as a character string type writes them.</summary>
	static LengthUnit? LengthUnitOf(int code)
	{
		return code switch
		{
			1 => LengthUnit.Characters,
			2 => LengthUnit.Octets,
			_ => null,
		};
	}

	static CharacterLengthUnits? Units(int code)
	{
		return code switch
		{
			1 => CharacterLengthUnits.Characters,
			2 => CharacterLengthUnits.Octets,
			_ => null,
		};
	}

	// ── §6.31 String value function ────────────────────────────────────────────

	static bool StringValueFunction(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;
		var word = cursor.Word;

		value = null!;

		switch (word)
		{
			case SqlWord.Substring:
				return SubstringFunction(ref cursor, out value);

			case SqlWord.JsonSerialize:
				return JSONSerialize(ref cursor, out value);

			case SqlWord.SubstringRegex:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && RegexSearch(ref cursor, out var search))
				{
					var occurrence = Occurrence(ref cursor);
					var grouping   = Group(ref cursor);

					if (cursor.Take(SqlTokenKind.RightParen))
					{
						value = Regex(RegexFunction.SubstringRegex, search) with { Occurrence = occurrence, Group = grouping };

						return true;
					}
				}

				break;
			}

			case SqlWord.Upper:
			case SqlWord.Lower:
			{
				var spelling = word == SqlWord.Upper ? "UPPER" : "LOWER";

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Character(ref cursor, out var text) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = Called(spelling, text);

					return true;
				}

				break;
			}

			case SqlWord.Convert:
			case SqlWord.Translate:
			{
				var convert = word == SqlWord.Convert;

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Character(ref cursor, out var text) && cursor.Take(SqlWord.Using) &&
					Names(ref cursor, 3, out var name) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = new Expression.TranslateUsing(convert ? TranslateFunction.Convert : TranslateFunction.Translate, text, name);

					return true;
				}

				break;
			}

			case SqlWord.TranslateRegex:
			{
				cursor.Take();

				if (!cursor.Take(SqlTokenKind.LeftParen) || !Character(ref cursor, out var pattern))
					break;

				Expression? flag = null;

				if (cursor.TakeWord("FLAG") && !Character(ref cursor, out flag))
					break;

				if (!cursor.Take(SqlWord.In) || !Character(ref cursor, out var text))
					break;

				Expression? replacement = null;

				if (cursor.Take(SqlWord.With) && !Character(ref cursor, out replacement))
					break;

				Expression? from = null;

				if (cursor.Take(SqlWord.From) && !Numeric(ref cursor, out from))
					break;

				CharacterLengthUnits? units = null;

				if (cursor.Take(SqlWord.Using))
				{
					units = Units(CharLengthUnits(ref cursor));

					if (units is null)
						break;
				}

				Expression? occurrence = null;
				var all = false;

				var marked = cursor;

				if (cursor.TakeWord("OCCURRENCE"))
				{
					if (cursor.Take(SqlWord.All))
						all = true;
					else if (!Numeric(ref cursor, out occurrence))
						cursor = marked;
				}

				if (cursor.Take(SqlTokenKind.RightParen))
				{
					value = new Expression.Regex(RegexFunction.TranslateRegex, pattern, text)
					{
						Flag           = flag,
						Replacement    = replacement,
						From           = from,
						Using          = units,
						Occurrence     = occurrence,
						AllOccurrences = all,
					};

					return true;
				}

				break;
			}

			case SqlWord.Lpad:
			case SqlWord.Rpad:
			{
				var spelling = word == SqlWord.Lpad ? "LPAD" : "RPAD";

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Character(ref cursor, out var text) && cursor.Take(SqlTokenKind.Comma) &&
					Numeric(ref cursor, out var length))
				{
					Expression? pad = null;

					if (cursor.Take(SqlTokenKind.Comma) && !Character(ref cursor, out pad))
						break;

					if (cursor.Take(SqlTokenKind.RightParen))
					{
						value = Called(spelling, text, length, pad);

						return true;
					}
				}

				break;
			}

			case SqlWord.Trim:
				return TrimFunction(ref cursor, out value);

			case SqlWord.Btrim:
			case SqlWord.Ltrim:
			case SqlWord.Rtrim:
			{
				var spelling = word == SqlWord.Btrim ? "BTRIM" : word == SqlWord.Ltrim ? "LTRIM" : "RTRIM";

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Character(ref cursor, out var text))
				{
					Expression? characters = null;

					if (cursor.Take(SqlTokenKind.Comma) && !Character(ref cursor, out characters))
						break;

					if (cursor.Take(SqlTokenKind.RightParen))
					{
						value = Called(spelling, text, characters);

						return true;
					}
				}

				break;
			}

			case SqlWord.Overlay:
				return OverlayFunction(ref cursor, out value);

			case SqlWord.Normalize:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Character(ref cursor, out var text))
				{
					NormalForm?  form   = null;
					StringLength length = default;

					if (cursor.Take(SqlTokenKind.Comma))
					{
						form = NormalForm(ref cursor);

						if (form is null)
							break;

						// `NORMALIZE(a, NFC, 10 K CHARACTERS)`: a large object's length and its units.
						if (cursor.Take(SqlTokenKind.Comma))
						{
							if (!LargeObjectLength(ref cursor, out var size))
								break;

							length = new StringLength(null, size, LengthUnitOf(CharLengthUnits(ref cursor)));
						}
					}

					if (cursor.Take(SqlTokenKind.RightParen))
					{
						value = new Expression.Normalize(text, form, length.LargeObject is null ? null : new Expression.Literal(NumericLiteral(length.LargeObject.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))))
						{
							MaxLengthMultiplier = length.LargeObject?.Multiplier,
							MaxLengthUnit       = length.Unit,
						};

						return true;
					}
				}

				break;
			}

			case SqlWord.Classifier:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen))
				{
					Identifier? variable = null;

					Identifier(ref cursor, out variable!);

					if (cursor.Take(SqlTokenKind.RightParen))
					{
						value = Called("CLASSIFIER", variable is null ? null : new Expression.Reference(new QualifiedName([variable])));

						return true;
					}
				}

				break;
			}
		}

		cursor = save;
		value  = null!;

		return false;
	}

	/// <summary>
	/// <c>&lt;character substring function&gt;</c>, <c>&lt;binary substring function&gt;</c> and the
	/// regular expression one: all three begin with the string, which is read once.
	/// </summary>
	static bool SubstringFunction(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		cursor.Take();

		if (!cursor.Take(SqlTokenKind.LeftParen) || !CommonValueExpression(ref cursor, out var text))
		{
			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.Similar))
		{
			if (Character(ref cursor, out var pattern) && cursor.Take(SqlWord.Escape) && Character(ref cursor, out var escape) &&
				(text.Roles & SqlTowers.Character) != 0 && cursor.Take(SqlTokenKind.RightParen))
			{
				value = new Expression.SubstringSimilar(text.Node, pattern, escape);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlWord.From) && Numeric(ref cursor, out var from))
		{
			Expression? length = null;

			if (cursor.Take(SqlWord.For) && !Numeric(ref cursor, out length))
			{
				cursor = save;

				return false;
			}

			var units = UsingUnits(ref cursor);

			if (SqlTowers.Characters(text.Roles, units > 0) && cursor.Take(SqlTokenKind.RightParen))
			{
				value = new Expression.Substring(text.Node, from, length, Units(units));

				return true;
			}
		}

		cursor = save;

		return false;
	}

	/// <summary>
	/// <c>&lt;trim function&gt;</c>: <c>[[spec] [character] FROM] source</c>, whose operands begin
	/// alike and are read once.
	/// </summary>
	static bool TrimFunction(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		cursor.Take();

		if (!cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return false;
		}

		TrimSpecification? specification =
			cursor.Take(SqlWord.Leading)  ? TrimSpecification.Leading :
			cursor.Take(SqlWord.Trailing) ? TrimSpecification.Trailing :
			cursor.Take(SqlWord.Both)     ? TrimSpecification.Both :
			null;

		Expression? character = null;
		Expression  source;
		var         from  = false;
		var         roles = SqlTowers.String;

		if (cursor.Take(SqlWord.From))
		{
			from = true;

			if (!CommonValueExpression(ref cursor, out var only))
			{
				cursor = save;

				return false;
			}

			roles &= only.Roles;
			source = only.Node;
		}
		else
		{
			if (!CommonValueExpression(ref cursor, out var first))
			{
				cursor = save;

				return false;
			}

			if (cursor.Take(SqlWord.From))
			{
				from = true;

				if (!CommonValueExpression(ref cursor, out var second))
				{
					cursor = save;

					return false;
				}

				character = first.Node;
				source    = second.Node;
				roles    &= first.Roles & second.Roles;
			}
			else
			{
				if (specification is not null)
				{
					// A specification was written, so what follows must be `[character] FROM source`.
					cursor = save;

					return false;
				}

				source = first.Node;
				roles &= first.Roles;
			}
		}

		if ((roles & SqlTowers.String) != 0 && cursor.Take(SqlTokenKind.RightParen))
		{
			value = new Expression.Trim(specification, character, from, source);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool OverlayFunction(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		cursor.Take();

		if (cursor.Take(SqlTokenKind.LeftParen) && CommonValueExpression(ref cursor, out var text) && cursor.TakeWord("PLACING") &&
			CommonValueExpression(ref cursor, out var placing) && cursor.Take(SqlWord.From) && Numeric(ref cursor, out var from))
		{
			Expression? length = null;

			if (!cursor.Take(SqlWord.For) || Numeric(ref cursor, out length))
			{
				var units = UsingUnits(ref cursor);

				if (SqlTowers.Characters(text.Roles & placing.Roles, units > 0) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = new Expression.Overlay(text.Node, placing.Node, from, length, Units(units));

					return true;
				}
			}
		}

		cursor = save;

		return false;
	}

	// ── §6.32 Datetime value function ──────────────────────────────────────────

	static bool DatetimeValueFunction(ref SqlCursor cursor, out Expression value)
	{
		switch (cursor.Word)
		{
			case SqlWord.CurrentDate:
				cursor.Take();

				value = new Expression.Current(CurrentValue.Date);

				return true;

			case SqlWord.CurrentTimestamp:
			case SqlWord.CurrentTime:
			case SqlWord.Localtimestamp:
			case SqlWord.Localtime:
			{
				var kind = cursor.Word switch
				{
					SqlWord.CurrentTimestamp => CurrentValue.Timestamp,
					SqlWord.CurrentTime      => CurrentValue.Time,
					SqlWord.Localtimestamp   => CurrentValue.LocalTimestamp,
					_                        => CurrentValue.LocalTime,
				};

				cursor.Take();

				var precision = Length(ref cursor);

				value = new Expression.Current(kind, null, precision is null ? null : new Expression.Literal(new LiteralValue.Numeric(precision.Value.ToString(System.Globalization.CultureInfo.InvariantCulture), NumericLiteralKind.DecimalInteger)));

				return true;
			}
		}

		value = null!;

		return false;
	}

	// ── §6.10 Window function, §10.9 Aggregate function ────────────────────────

	/// <summary>
	/// <c>&lt;set function specification&gt;</c>, <c>&lt;window function&gt;</c> and
	/// <c>&lt;nested window function&gt;</c>. A window function's type may be an aggregate, so an
	/// aggregate is read once, with <c>OVER</c> after it or not.
	/// </summary>
	static bool WindowedFunction(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		switch (cursor.Word)
		{
			case SqlWord.Running:
			case SqlWord.Name when cursor.IsWord("FINAL"):
			{
				var semantics = cursor.IsWord("RUNNING") ? RowPatternSemantics.Running : RowPatternSemantics.Final;

				cursor.Take();

				if (AggregateFunction(ref cursor, out var aggregate))
				{
					value = WithSemantics(aggregate, semantics);

					return true;
				}

				cursor = save;

				return false;
			}

			case SqlWord.Grouping:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen))
				{
					var columns = new List<Argument>();

					while (true)
					{
						if (!ColumnReference(ref cursor, out var column))
							break;

						columns.Add(new Argument(column));

						if (!cursor.Take(SqlTokenKind.Comma))
							break;
					}

					if (columns.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
					{
						value = new Expression.Invocation(new QualifiedName([new Identifier("GROUPING")]), columns);

						return true;
					}
				}

				cursor = save;

				return false;
			}

			case SqlWord.Ntile:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && SimpleOrDynamicValue(ref cursor, out var tiles) && cursor.Take(SqlTokenKind.RightParen) &&
					WindowOver(ref cursor, out var over))
				{
					value = Called("NTILE", tiles) with { Over = over };

					return true;
				}

				cursor = save;

				return false;
			}

			case SqlWord.Lead:
			case SqlWord.Lag:
			{
				var spelling = cursor.Word == SqlWord.Lead ? "LEAD" : "LAG";

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var operand))
				{
					Expression? offset = null, otherwise = null;

					if (cursor.Take(SqlTokenKind.Comma))
					{
						if (cursor.Kind != SqlTokenKind.Number)
						{
							cursor = save;

							return false;
						}

						offset = new Expression.Literal(NumericLiteral(cursor.TextOf(cursor.Token)));

						cursor.Take();

						if (cursor.Take(SqlTokenKind.Comma) && !Value(ref cursor, out otherwise))
						{
							cursor = save;

							return false;
						}
					}

					if (cursor.Take(SqlTokenKind.RightParen))
					{
						var nulls = NullTreatment(ref cursor);

						if (WindowOver(ref cursor, out var over))
						{
							value = Called(spelling, operand, offset, otherwise) with { Nulls = nulls, Over = over };

							return true;
						}
					}
				}

				cursor = save;

				return false;
			}

			case SqlWord.FirstValue:
			case SqlWord.LastValue:
			{
				var spelling = cursor.Word == SqlWord.FirstValue ? "FIRST_VALUE" : "LAST_VALUE";

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var operand) && cursor.Take(SqlTokenKind.RightParen))
				{
					var nulls = NullTreatment(ref cursor);

					if (WindowOver(ref cursor, out var over))
					{
						value = Called(spelling, operand) with { Nulls = nulls, Over = over };

						return true;
					}
				}

				cursor = save;

				return false;
			}

			case SqlWord.NthValue:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var operand) && cursor.Take(SqlTokenKind.Comma) &&
					SimpleOrDynamicValue(ref cursor, out var nth) && cursor.Take(SqlTokenKind.RightParen))
				{
					FromFirstOrLast? end = null;

					if (cursor.Take(SqlWord.From))
					{
						end =
							cursor.TakeWord("FIRST") ? FromFirstOrLast.First :
							cursor.TakeWord("LAST")  ? FromFirstOrLast.Last :
							(FromFirstOrLast?)null;

						if (end is null)
						{
							cursor = save;

							return false;
						}
					}

					var nulls = NullTreatment(ref cursor);

					if (WindowOver(ref cursor, out var over))
					{
						value = Called("NTH_VALUE", operand, nth) with { From = end, Nulls = nulls, Over = over };

						return true;
					}
				}

				cursor = save;

				return false;
			}

			case SqlWord.ValueOf:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var operand) && cursor.Take(SqlWord.At) &&
					RowMarker(ref cursor, out var marker))
				{
					Expression? delta = null;
					var minus = false;

					if (cursor.Kind is SqlTokenKind.Plus or SqlTokenKind.Minus)
					{
						minus = cursor.Kind == SqlTokenKind.Minus;

						cursor.Take();

						if (!SimpleOrDynamicValue(ref cursor, out delta))
						{
							cursor = save;

							return false;
						}
					}

					Expression? otherwise = null;

					if (cursor.Take(SqlTokenKind.Comma) && !Value(ref cursor, out otherwise))
					{
						cursor = save;

						return false;
					}

					if (cursor.Take(SqlTokenKind.RightParen))
					{
						value = new Expression.ValueOf(operand, new Expression.RowMarker(marker, delta is null ? null : minus ? UnaryOperator.Minus : UnaryOperator.Plus, delta), otherwise);

						return true;
					}
				}

				cursor = save;

				return false;
			}

			case SqlWord.RowNumber:
			{
				// `ROW_NUMBER(<row marker>)` is a nested window function; `ROW_NUMBER() OVER w` is not.
				var marked = cursor;

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && RowMarker(ref cursor, out var marker) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = Called("ROW_NUMBER", new Expression.RowMarker(marker));

					return true;
				}

				cursor = marked;

				break;
			}
		}

		// `RANK() OVER w` and its fellows, and every aggregate with or without a window.
		switch (cursor.Word)
		{
			case SqlWord.Rank:
			case SqlWord.DenseRank:
			case SqlWord.PercentRank:
			case SqlWord.CumeDist:
			case SqlWord.RowNumber:
			{
				var spelling = cursor.TextOf(cursor.Token).ToUpperInvariant();
				var marked   = cursor;

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && cursor.Take(SqlTokenKind.RightParen) && WindowOver(ref cursor, out var over))
				{
					value = Called(spelling) with { Over = over };

					return true;
				}

				cursor = marked;

				break;
			}
		}

		if (AggregateFunction(ref cursor, out var written))
		{
			var over = cursor;

			if (cursor.Take(SqlWord.Over) && WindowNameOrSpecification(ref cursor, out var window))
			{
				value = WithOver(written, window);

				return true;
			}

			cursor = over;
			value  = written;

			return true;
		}

		cursor = save;

		return false;
	}

	static bool WindowOver(ref SqlCursor cursor, out WindowReference over)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Over) && WindowNameOrSpecification(ref cursor, out over))
			return true;

		cursor = save;
		over   = null!;

		return false;
	}

	static bool WindowNameOrSpecification(ref SqlCursor cursor, out WindowReference over)
	{
		var save = cursor;

		if (cursor.Kind == SqlTokenKind.LeftParen)
		{
			if (WindowSpecification(ref cursor, out var specification))
			{
				over = new WindowReference.Specification(specification);

				return true;
			}
		}
		else if (Identifier(ref cursor, out var name))
		{
			over = new WindowReference.NameRef(name);

			return true;
		}

		cursor = save;
		over   = null!;

		return false;
	}

	static NullTreatment? NullTreatment(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.TakeWord("RESPECT") && cursor.TakeWord("NULLS"))
			return Ast.NullTreatment.RespectNulls;

		cursor = save;

		if (cursor.TakeWord("IGNORE") && cursor.TakeWord("NULLS"))
			return Ast.NullTreatment.IgnoreNulls;

		cursor = save;

		return null;
	}

	static bool RowMarker(ref SqlCursor cursor, out RowMarkerKind marker)
	{
		var read =
			cursor.Take(SqlWord.BeginPartition) ? RowMarkerKind.BeginPartition :
			cursor.Take(SqlWord.BeginFrame)     ? RowMarkerKind.BeginFrame :
			cursor.Take(SqlWord.CurrentRow)     ? RowMarkerKind.CurrentRow :
			cursor.Take(SqlWord.FrameRow)       ? RowMarkerKind.FrameRow :
			cursor.Take(SqlWord.EndFrame)       ? RowMarkerKind.EndFrame :
			cursor.Take(SqlWord.EndPartition)   ? RowMarkerKind.EndPartition :
			(RowMarkerKind?)null;

		marker = read ?? default;

		return read is not null;
	}

	/// <summary>A simple value specification, or a dynamic parameter.</summary>
	static bool SimpleOrDynamicValue(ref SqlCursor cursor, out Expression value)
	{
		if (cursor.Take(SqlTokenKind.Question))
		{
			value = new Expression.Parameter(ParameterKind.Dynamic);

			return true;
		}

		return SimpleValueSpecification(ref cursor, out value);
	}

	static Expression WithSemantics(Expression aggregate, RowPatternSemantics semantics)
	{
		return aggregate switch
		{
			Expression.Invocation i => i with { Semantics = semantics },
			Expression.JsonArrayAggregate a => a with { Semantics = semantics },
			Expression.JsonObjectAggregate o => o with { Semantics = semantics },
			_ => throw new ArgumentOutOfRangeException(nameof(aggregate), aggregate, "An aggregate this method does not know."),
		};
	}

	static Expression WithOver(Expression aggregate, WindowReference over)
	{
		return aggregate switch
		{
			Expression.Invocation i => i with { Over = over },
			Expression.JsonArrayAggregate a => a with { Over = over },
			Expression.JsonObjectAggregate o => o with { Over = over },
			_ => throw new ArgumentOutOfRangeException(nameof(aggregate), aggregate, "An aggregate this method does not know."),
		};
	}

	static Expression Filtered(Expression aggregate, FilterClause filter)
	{
		return aggregate switch
		{
			Expression.Invocation i => i with { Filter = filter },
			Expression.JsonArrayAggregate a => a with { Filter = filter },
			Expression.JsonObjectAggregate o => o with { Filter = filter },
			_ => throw new ArgumentOutOfRangeException(nameof(aggregate), aggregate, "An aggregate this method does not know."),
		};
	}

	// ── §10.9 Aggregate function ───────────────────────────────────────────────

	static bool AggregateFunction(ref SqlCursor cursor, out Expression value)
	{
		if (!AggregateCall(ref cursor, out value))
			return false;

		var save = cursor;

		if (cursor.TakeWord("FILTER") && cursor.Take(SqlTokenKind.LeftParen) && cursor.Take(SqlWord.Where) &&
			SearchCondition(ref cursor, out var condition) && cursor.Take(SqlTokenKind.RightParen))
		{
			value = Filtered(value, new FilterClause(condition));

			return true;
		}

		cursor = save;

		return true;
	}

	static bool AggregateCall(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;
		var word = cursor.Word;

		value = null!;

		switch (word)
		{
			case SqlWord.JsonArrayagg:
			case SqlWord.JsonObjectagg:
				return JSONAggregateFunction(ref cursor, out value);

			case SqlWord.Count:
			{
				// `COUNT(*)`, and `COUNT([DISTINCT] v)` with the computational operations.
				var marked = cursor;

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && cursor.Take(SqlTokenKind.Asterisk) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = Called("COUNT", new Expression.Asterisk());

					return true;
				}

				cursor = marked;

				break;
			}

			case SqlWord.Rank:
			case SqlWord.DenseRank:
			case SqlWord.PercentRank:
			case SqlWord.CumeDist:
			{
				var spelling = cursor.TextOf(cursor.Token).ToUpperInvariant();

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var first))
				{
					var arguments = new List<Argument> { new(first) };

					while (cursor.Take(SqlTokenKind.Comma))
					{
						if (!Value(ref cursor, out var next))
						{
							cursor = save;

							return false;
						}

						arguments.Add(new Argument(next));
					}

					if (cursor.Take(SqlTokenKind.RightParen) && WithinGroupSpecification(ref cursor, out var within))
					{
						value = new Expression.Invocation(new QualifiedName([new Identifier(spelling)]), arguments) { WithinGroup = within };

						return true;
					}
				}

				cursor = save;

				return false;
			}

			case SqlWord.PercentileCont:
			case SqlWord.PercentileDisc:
			{
				var spelling = cursor.Word == SqlWord.PercentileCont ? "PERCENTILE_CONT" : "PERCENTILE_DISC";

				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Numeric(ref cursor, out var fraction) && cursor.Take(SqlTokenKind.RightParen) &&
					WithinGroupSpecification(ref cursor, out var within))
				{
					value = Called(spelling, fraction) with { WithinGroup = within };

					return true;
				}

				cursor = save;

				return false;
			}

			case SqlWord.ArrayAgg:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var operand))
				{
					OrderByClause? order = null;

					if (cursor.Take(SqlWord.Order) && (!cursor.Take(SqlWord.By) || !SortSpecificationList(ref cursor, out order)))
					{
						cursor = save;

						return false;
					}

					if (cursor.Take(SqlTokenKind.RightParen))
					{
						value = Called("ARRAY_AGG", operand) with { OrderBy = order };

						return true;
					}
				}

				cursor = save;

				return false;
			}

			case SqlWord.Listagg:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen))
				{
					var quantifier = SetQuantifier(ref cursor);

					if (Character(ref cursor, out var listed) && cursor.Take(SqlTokenKind.Comma) && cursor.Kind == SqlTokenKind.String)
					{
						var separator = StringLiteral(ref cursor, StringLiteralKind.Character);
						var overflow  = ListaggOverflowClause(ref cursor);

						if (cursor.Take(SqlTokenKind.RightParen) && WithinGroupSpecification(ref cursor, out var within))
						{
							value = Called("LISTAGG", listed, new Expression.Literal(separator)) with
							{
								Quantifier  = quantifier,
								Overflow    = overflow,
								WithinGroup = within,
							};

							return true;
						}
					}
				}

				cursor = save;

				return false;
			}
		}

		// <general set function>: a computational operation and one value.
		if (ComputationalOperation(cursor.Word))
		{
			var spelling = cursor.TextOf(cursor.Token).ToUpperInvariant();

			cursor.Take();

			if (cursor.Take(SqlTokenKind.LeftParen))
			{
				var quantifier = SetQuantifier(ref cursor);

				if (Value(ref cursor, out var operand) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = Called(spelling, operand) with { Quantifier = quantifier };

					return true;
				}
			}

			cursor = save;

			return false;
		}

		if (BinarySetFunction(cursor.Word))
		{
			var spelling = cursor.TextOf(cursor.Token).ToUpperInvariant();

			cursor.Take();

			if (cursor.Take(SqlTokenKind.LeftParen) && Numeric(ref cursor, out var first) && cursor.Take(SqlTokenKind.Comma) &&
				Numeric(ref cursor, out var second) && cursor.Take(SqlTokenKind.RightParen))
			{
				value = Called(spelling, first, second);

				return true;
			}

			cursor = save;

			return false;
		}

		cursor = save;

		return false;
	}

	static bool ComputationalOperation(SqlWord word)
	{
		return word is SqlWord.Avg or SqlWord.Max or SqlWord.Min or SqlWord.Sum or SqlWord.Every or SqlWord.AnyValue or SqlWord.Any or SqlWord.Some
			or SqlWord.Count or SqlWord.StddevPop or SqlWord.StddevSamp or SqlWord.VarSamp or SqlWord.VarPop or SqlWord.Collect
			or SqlWord.Fusion or SqlWord.Intersection;
	}

	static bool BinarySetFunction(SqlWord word)
	{
		return word is SqlWord.CovarPop or SqlWord.CovarSamp or SqlWord.Corr or SqlWord.RegrSlope or SqlWord.RegrIntercept or SqlWord.RegrCount
			or SqlWord.RegrR2 or SqlWord.RegrAvgx or SqlWord.RegrAvgy or SqlWord.RegrSxx or SqlWord.RegrSyy or SqlWord.RegrSxy;
	}

	static SetQuantifier? SetQuantifier(ref SqlCursor cursor)
	{
		return cursor.Take(SqlWord.Distinct) ? Ast.SetQuantifier.Distinct :
		cursor.Take(SqlWord.All) ? Ast.SetQuantifier.All :
		null;
	}

	static bool WithinGroupSpecification(ref SqlCursor cursor, out WithinGroupClause within)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Within) && cursor.Take(SqlWord.Group) && cursor.Take(SqlTokenKind.LeftParen) &&
			cursor.Take(SqlWord.Order) && cursor.Take(SqlWord.By) && SortSpecificationList(ref cursor, out var order) &&
			cursor.Take(SqlTokenKind.RightParen))
		{
			within = new WithinGroupClause(order!);

			return true;
		}

		cursor = save;
		within = null!;

		return false;
	}

	static ListaggOverflow? ListaggOverflowClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.On) && cursor.TakeWord("OVERFLOW"))
		{
			if (cursor.TakeWord("ERROR"))
				return new ListaggOverflow(false);

			if (cursor.Take(SqlWord.Truncate))
			{
				LiteralValue? filler = null;

				if (cursor.Kind == SqlTokenKind.String)
					filler = StringLiteral(ref cursor, StringLiteralKind.Character);

				var with = cursor.Take(SqlWord.With);

				if ((with || cursor.Take(SqlWord.Without)) && cursor.Take(SqlWord.Count))
					return new ListaggOverflow(true, filler is null ? null : new Expression.Literal(filler), with);
			}
		}

		cursor = save;

		return null;
	}

	// ── §6.26 Row pattern navigation operation ─────────────────────────────────

	/// <summary>
	/// What only a navigation is: <c>RUNNING</c> or <c>FINAL</c> and then <c>FIRST</c> or
	/// <c>LAST</c>. One without them is a routine's invocation in shape, and read as one.
	/// </summary>
	static bool RowPatternNavigationOperation(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		if (!cursor.IsWord("RUNNING") && !cursor.IsWord("FINAL"))
			return false;

		var semantics = cursor.IsWord("RUNNING") ? RowPatternSemantics.Running : RowPatternSemantics.Final;

		cursor.Take();

		var spelling =
			cursor.TakeWord("FIRST") ? "FIRST" :
			cursor.TakeWord("LAST")  ? "LAST" :
			null;

		if (spelling is not null && cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var operand))
		{
			Expression? offset = null;

			if (!cursor.Take(SqlTokenKind.Comma) || SimpleOrDynamicValue(ref cursor, out offset))
			{
				if (cursor.Take(SqlTokenKind.RightParen))
				{
					value = Called(spelling, operand, offset) with { Semantics = semantics };

					return true;
				}
			}
		}

		cursor = save;

		return false;
	}
}
