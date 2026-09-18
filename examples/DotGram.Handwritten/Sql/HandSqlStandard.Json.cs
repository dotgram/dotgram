using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Handwritten;

// ISO/IEC 9075-2:2023 §6.33's JSON value function, §6.37's constructors, §6.38's query, §6.39's
// simplified accessor and §7.11's JSON table. What the BNF spells, and only that: a path is a
// character string literal, which the functions take whole, and the path language is read where the
// SQL tokens reach — its literals and key names are left to the Syntax Rules.
partial class HandSqlStandard
{
	// ── The clauses the JSON functions share ───────────────────────────────────

	/// <summary><c>&lt;JSON input clause&gt;</c>: <c>FORMAT JSON [ENCODING UTF8]</c>.</summary>
	static bool JSONInputClause(ref SqlCursor cursor, out JsonInputClause? input)
	{
		var save = cursor;

		input = null;

		if (!cursor.TakeWord("FORMAT"))
			return false;

		if (!cursor.Take(SqlWord.Json))
		{
			cursor = save;

			return false;
		}

		input = new JsonInputClause(JSONRepresentation(ref cursor));

		return true;
	}

	static JsonRepresentation JSONRepresentation(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.TakeWord("ENCODING"))
		{
			var encoding =
				cursor.TakeWord("UTF8")  ? JsonEncoding.Utf8 :
				cursor.TakeWord("UTF16") ? JsonEncoding.Utf16 :
				cursor.TakeWord("UTF32") ? JsonEncoding.Utf32 :
				(JsonEncoding?)null;

			if (encoding is not null)
				return new JsonRepresentation(encoding);

			cursor = save;
		}

		return new JsonRepresentation();
	}

	/// <summary>The <c>FORMAT JSON</c> after a value, where one was written.</summary>
	static JsonInputClause? Format(ref SqlCursor cursor) => JSONInputClause(ref cursor, out var input) ? input : null;

	static JsonPredicateType? JSONPredicateTypeConstraint(ref SqlCursor cursor) =>
		cursor.Take(SqlWord.Value)      ? JsonPredicateType.Value :
		cursor.Take(SqlWord.Array)      ? JsonPredicateType.Array :
		cursor.TakeWord("OBJECT")       ? JsonPredicateType.Object :
		cursor.TakeWord("SCALAR")       ? JsonPredicateType.Scalar :
		null;

	/// <summary><c>WITH UNIQUE [KEYS]</c> and its three fellows.</summary>
	static JsonKeyUniqueness? JSONKeyUniquenessConstraint(ref SqlCursor cursor)
	{
		var save = cursor;
		var with = cursor.Take(SqlWord.With);

		if (!with && !cursor.Take(SqlWord.Without))
			return null;

		if (!cursor.Take(SqlWord.Unique))
		{
			cursor = save;

			return null;
		}

		var keys = cursor.TakeWord("KEYS");

		return with
			? keys ? JsonKeyUniqueness.WithUniqueKeys : JsonKeyUniqueness.WithUnique
			: keys ? JsonKeyUniqueness.WithoutUniqueKeys : JsonKeyUniqueness.WithoutUnique;
	}

	static JsonNullHandling? JSONConstructorNullClause(ref SqlCursor cursor)
	{
		var save = cursor;

		var nulls =
			cursor.Take(SqlWord.Null)   ? JsonNullHandling.NullOnNull :
			cursor.Take(SqlWord.Absent) ? JsonNullHandling.AbsentOnNull :
			(JsonNullHandling?)null;

		if (nulls is not null && cursor.Take(SqlWord.On) && cursor.Take(SqlWord.Null))
			return nulls;

		cursor = save;

		return null;
	}

	/// <summary><c>&lt;JSON output clause&gt;</c>: <c>RETURNING t [FORMAT JSON]</c>.</summary>
	static JsonOutput? JSONOutputClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.TakeWord("RETURNING") && DataType(ref cursor, out var type))
			return new JsonOutput(type, Format(ref cursor)?.Representation);

		cursor = save;

		return null;
	}

	/// <summary><c>&lt;JSON API common syntax&gt;</c>: a context item, a path, its name, and what it is passed.</summary>
	static bool JSONAPICommonSyntax(ref SqlCursor cursor, out JsonApiCommon common)
	{
		var save = cursor;

		common = null!;

		if (!JSONInputExpression(ref cursor, out var context) || !cursor.Take(SqlTokenKind.Comma) || !PathSpecification(ref cursor, out var path))
		{
			cursor = save;

			return false;
		}

		Identifier? name = null;

		if (cursor.Take(SqlWord.As) && !Identifier(ref cursor, out name))
		{
			cursor = save;

			return false;
		}

		var passing = new List<JsonArgument>();

		if (cursor.TakeWord("PASSING"))
		{
			while (true)
			{
				if (!JSONInputExpression(ref cursor, out var argument) || !cursor.Take(SqlWord.As) || !Identifier(ref cursor, out var named))
				{
					cursor = save;

					return false;
				}

				passing.Add(new JsonArgument(argument.Value, named, argument.Format));

				if (!cursor.Take(SqlTokenKind.Comma))
					break;
			}
		}

		common = new JsonApiCommon(context.Value, path, name, passing, context.Format);

		return true;
	}

	/// <summary>A value and the <c>FORMAT JSON</c> after it.</summary>
	static bool JSONInputExpression(ref SqlCursor cursor, out JsonElement element)
	{
		if (!Value(ref cursor, out var value))
		{
			element = null!;

			return false;
		}

		element = new JsonElement(value, Format(ref cursor));

		return true;
	}

	/// <summary>A path, which the BNF writes as a character string literal and the tree keeps whole.</summary>
	static bool PathSpecification(ref SqlCursor cursor, out string path)
	{
		if (cursor.Kind != SqlTokenKind.String)
		{
			path = null!;

			return false;
		}

		var token = cursor.Token;

		path = cursor.Text.Substring(token.Quote, token.Body - token.Quote);

		cursor.Take();

		return true;
	}

	// ── §6.33 JSON value function, §6.38 JSON query, §6.37 the constructors ────

	/// <summary>The JSON functions a value expression primary may be.</summary>
	static bool JSONValueConstructorOrQuery(ref SqlCursor cursor, out Expression value)
	{
		switch (cursor.Word)
		{
			case SqlWord.JsonValue : return JSONValueFunction(ref cursor, out value);
			case SqlWord.JsonQuery : return JSONQuery(ref cursor, out value);
			case SqlWord.JsonObject: return JSONObjectConstructor(ref cursor, out value);
			case SqlWord.JsonArray : return JSONArrayConstructor(ref cursor, out value);
		}

		value = null!;

		return false;
	}

	static bool JSONValueFunction(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		cursor.Take();

		if (!cursor.Take(SqlTokenKind.LeftParen) || !JSONAPICommonSyntax(ref cursor, out var common))
		{
			cursor = save;

			return false;
		}

		DataType? returning = null;

		if (cursor.TakeWord("RETURNING") && !DataType(ref cursor, out returning))
		{
			cursor = save;

			return false;
		}

		var empty = JSONValueBehavior(ref cursor, true);
		var error = JSONValueBehavior(ref cursor, false);

		if (cursor.Take(SqlTokenKind.RightParen))
		{
			value = new Expression.JsonValue(common, returning, empty, error);

			return true;
		}

		cursor = save;

		return false;
	}

	/// <summary>What happens on an empty result or on an error, where it was written.</summary>
	static JsonValueBehavior? JSONValueBehavior(ref SqlCursor cursor, bool empty)
	{
		var save = cursor;

		JsonValueBehavior? behaviour = null;

		if (cursor.TakeWord("ERROR"))
			behaviour = new JsonValueBehavior.Error();
		else if (cursor.Take(SqlWord.Null))
			behaviour = new JsonValueBehavior.Null();
		else if (cursor.Take(SqlWord.Default) && Value(ref cursor, out var otherwise))
			behaviour = new JsonValueBehavior.Default(otherwise);

		if (behaviour is not null && cursor.Take(SqlWord.On) && (empty ? cursor.Take(SqlWord.Empty) : cursor.TakeWord("ERROR")))
			return behaviour;

		cursor = save;

		return null;
	}

	static bool JSONQuery(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		cursor.Take();

		if (!cursor.Take(SqlTokenKind.LeftParen) || !JSONAPICommonSyntax(ref cursor, out var common))
		{
			cursor = save;

			return false;
		}

		var output  = JSONOutputClause(ref cursor);
		var wrapper = JSONQueryWrapperBehavior(ref cursor);
		var quotes  = JSONQueryQuotes(ref cursor);
		var empty   = JSONQueryBehavior(ref cursor, true);
		var error   = JSONQueryBehavior(ref cursor, false);

		if (cursor.Take(SqlTokenKind.RightParen))
		{
			value = new Expression.JsonQuery(common, output, wrapper, quotes, empty, error);

			return true;
		}

		cursor = save;

		return false;
	}

	/// <summary><c>WITHOUT [ARRAY] WRAPPER</c>, <c>WITH [CONDITIONAL | UNCONDITIONAL] [ARRAY] WRAPPER</c>.</summary>
	static JsonWrapperBehavior? JSONQueryWrapperBehavior(ref SqlCursor cursor)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Without))
		{
			var array = cursor.Take(SqlWord.Array);

			if (cursor.TakeWord("WRAPPER"))
				return array ? JsonWrapperBehavior.WithoutArray : JsonWrapperBehavior.Without;

			cursor = save;

			return null;
		}

		if (cursor.Take(SqlWord.With))
		{
			var conditional   = cursor.TakeWord("CONDITIONAL");
			var unconditional = !conditional && cursor.TakeWord("UNCONDITIONAL");
			var array         = cursor.Take(SqlWord.Array);

			if (cursor.TakeWord("WRAPPER"))
				return unconditional
					? array ? JsonWrapperBehavior.WithUnconditionalArray : JsonWrapperBehavior.WithUnconditional
					: conditional
						? array ? JsonWrapperBehavior.WithConditionalArray : JsonWrapperBehavior.WithConditional
						: array ? JsonWrapperBehavior.WithArray : JsonWrapperBehavior.With;

			cursor = save;
		}

		return null;
	}

	static JsonQuotes? JSONQueryQuotes(ref SqlCursor cursor)
	{
		var save = cursor;

		var behaviour =
			cursor.TakeWord("KEEP") ? JsonQuotesBehavior.Keep :
			cursor.TakeWord("OMIT") ? JsonQuotesBehavior.Omit :
			(JsonQuotesBehavior?)null;

		if (behaviour is not null && cursor.TakeWord("QUOTES"))
		{
			var scalar = cursor;
			var on     = cursor.Take(SqlWord.On) && cursor.TakeWord("SCALAR") && cursor.TakeWord("STRING");

			if (!on)
				cursor = scalar;

			return new JsonQuotes(behaviour.Value, on);
		}

		cursor = save;

		return null;
	}

	static JsonQueryBehavior? JSONQueryBehavior(ref SqlCursor cursor, bool empty)
	{
		var save = cursor;

		JsonQueryBehavior? behaviour = null;

		if (cursor.TakeWord("ERROR"))
			behaviour = Ast.JsonQueryBehavior.Error;
		else if (cursor.Take(SqlWord.Null))
			behaviour = Ast.JsonQueryBehavior.Null;
		else if (cursor.Take(SqlWord.Empty))
			behaviour =
				cursor.Take(SqlWord.Array)  ? Ast.JsonQueryBehavior.EmptyArray :
				cursor.TakeWord("OBJECT")   ? Ast.JsonQueryBehavior.EmptyObject :
				null;

		if (behaviour is not null && cursor.Take(SqlWord.On) && (empty ? cursor.Take(SqlWord.Empty) : cursor.TakeWord("ERROR")))
			return behaviour;

		cursor = save;

		return null;
	}

	static bool JSONObjectConstructor(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		cursor.Take();

		if (!cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return false;
		}

		var members = new List<JsonMember>();

		JsonNullHandling? nulls = null;
		JsonKeyUniqueness? unique = null;

		if (JSONNameAndValue(ref cursor, out var first))
		{
			members.Add(first);

			while (cursor.Take(SqlTokenKind.Comma))
			{
				if (!JSONNameAndValue(ref cursor, out var next))
				{
					cursor = save;

					return false;
				}

				members.Add(next);
			}

			nulls  = JSONConstructorNullClause(ref cursor);
			unique = JSONKeyUniquenessConstraint(ref cursor);
		}

		var output = JSONOutputClause(ref cursor);

		if (cursor.Take(SqlTokenKind.RightParen))
		{
			value = new Expression.JsonObject(members, nulls, unique, output);

			return true;
		}

		cursor = save;

		return false;
	}

	/// <summary>A name and a value: <c>KEY k VALUE v</c>, <c>k VALUE v</c> or <c>k : v</c>.</summary>
	static bool JSONNameAndValue(ref SqlCursor cursor, out JsonMember member)
	{
		var save = cursor;

		member = null!;

		if (cursor.TakeWord("KEY"))
		{
			if (Character(ref cursor, out var key) && cursor.Take(SqlWord.Value) && JSONInputExpression(ref cursor, out var written))
			{
				member = new JsonMember(key, written.Value, JsonMemberSyntax.KeyValue, written.Format);

				return true;
			}

			// §5.2 does not reserve `KEY`, so a member may be named by a column called that:
			// `JSON_OBJECTAGG(KEY VALUE v)` names the column `KEY`.
			cursor = save;
		}

		if (Character(ref cursor, out var name))
		{
			var colon = cursor.Take(SqlTokenKind.Colon);

			if ((colon || cursor.Take(SqlWord.Value)) && JSONInputExpression(ref cursor, out var written))
			{
				member = new JsonMember(name, written.Value, colon ? JsonMemberSyntax.Colon : JsonMemberSyntax.Value, written.Format);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static bool JSONArrayConstructor(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		cursor.Take();

		if (!cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return false;
		}

		// By enumeration, which is asked first, and then by query.
		var marked   = cursor;
		var elements = new List<JsonElement>();

		if (JSONInputExpression(ref cursor, out var first))
		{
			elements.Add(first);

			var complete = true;

			while (cursor.Take(SqlTokenKind.Comma))
			{
				if (!JSONInputExpression(ref cursor, out var next))
				{
					complete = false;

					break;
				}

				elements.Add(next);
			}

			if (complete)
			{
				var nulls  = JSONConstructorNullClause(ref cursor);
				var output = JSONOutputClause(ref cursor);

				if (cursor.Take(SqlTokenKind.RightParen))
				{
					value = new Expression.JsonArray(elements, nulls, output);

					return true;
				}
			}
		}

		// The elements are one optional group: where what follows them is not the closing bracket,
		// they were no elements, and what was read as one is the output clause or a query.
		cursor = marked;

		var alone = JSONOutputClause(ref cursor);

		if (cursor.Take(SqlTokenKind.RightParen))
		{
			value = new Expression.JsonArray([], null, alone);

			return true;
		}

		cursor = marked;

		if (QueryExpression(ref cursor, out var query))
		{
			var format = Format(ref cursor);
			var handling = JSONConstructorNullClause(ref cursor);
			var output = JSONOutputClause(ref cursor);

			if (cursor.Take(SqlTokenKind.RightParen))
			{
				value = new Expression.JsonArrayQuery(query, format, output, handling);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static bool JSONExistsPredicate(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		cursor.Take();

		if (cursor.Take(SqlTokenKind.LeftParen) && JSONAPICommonSyntax(ref cursor, out var common))
		{
			JsonExistsErrorBehavior? behaviour = null;

			var marked = cursor;

			if (cursor.Take(SqlWord.True))
				behaviour = JsonExistsErrorBehavior.True;
			else if (cursor.Take(SqlWord.False))
				behaviour = JsonExistsErrorBehavior.False;
			else if (cursor.Take(SqlWord.Unknown))
				behaviour = JsonExistsErrorBehavior.Unknown;
			else if (cursor.TakeWord("ERROR"))
				behaviour = JsonExistsErrorBehavior.Error;

			if (behaviour is not null && !(cursor.Take(SqlWord.On) && cursor.TakeWord("ERROR")))
			{
				cursor    = marked;
				behaviour = null;
			}

			if (cursor.Take(SqlTokenKind.RightParen))
			{
				value = new Expression.JsonExists(common, behaviour);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	/// <summary><c>&lt;JSON typed value function&gt;</c>: <c>JSON (…)</c> and <c>JSON_SCALAR (…)</c>.</summary>
	static bool JSONTypedValueFunction(ref SqlCursor cursor, out Expression value)
	{
		var save   = cursor;
		var parsed = cursor.Word == SqlWord.Json;

		value = null!;

		cursor.Take();

		if (parsed)
		{
			if (cursor.Take(SqlTokenKind.LeftParen) && Node(ref cursor, SqlTowers.String, out var text))
			{
				var format = Format(ref cursor);
				var unique = JSONKeyUniquenessConstraint(ref cursor);

				if (cursor.Take(SqlTokenKind.RightParen))
				{
					value = new Expression.JsonParse(text, format, unique);

					return true;
				}
			}
		}
		else if (cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var scalar) && cursor.Take(SqlTokenKind.RightParen))
		{
			value = new Expression.JsonScalar(scalar);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool JSONSerialize(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		cursor.Take();

		if (cursor.Take(SqlTokenKind.LeftParen) && Node(ref cursor, SqlTowers.Json, out var json))
		{
			var output = JSONOutputClause(ref cursor);

			if (cursor.Take(SqlTokenKind.RightParen))
			{
				value = new Expression.JsonSerialize(json, output);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static bool JSONAggregateFunction(ref SqlCursor cursor, out Expression value)
	{
		var save  = cursor;
		var array = cursor.Word == SqlWord.JsonArrayagg;

		value = null!;

		cursor.Take();

		if (!cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return false;
		}

		if (array)
		{
			if (JSONInputExpression(ref cursor, out var element))
			{
				OrderByClause? order = null;

				if (cursor.Take(SqlWord.Order) && (!cursor.Take(SqlWord.By) || !SortSpecificationList(ref cursor, out order)))
				{
					cursor = save;

					return false;
				}

				var nulls  = JSONConstructorNullClause(ref cursor);
				var output = JSONOutputClause(ref cursor);

				if (cursor.Take(SqlTokenKind.RightParen))
				{
					value = new Expression.JsonArrayAggregate(element, order, nulls, output);

					return true;
				}
			}
		}
		else if (JSONNameAndValue(ref cursor, out var member))
		{
			var nulls  = JSONConstructorNullClause(ref cursor);
			var unique = JSONKeyUniquenessConstraint(ref cursor);
			var output = JSONOutputClause(ref cursor);

			if (cursor.Take(SqlTokenKind.RightParen))
			{
				value = new Expression.JsonObjectAggregate(member, nulls, unique, output);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	// ── §6.39 JSON simplified accessor ─────────────────────────────────────────

	/// <summary>
	/// <c>&lt;JSON method&gt;</c>, as a method called on what stands before it: its names are mostly
	/// reserved words of SQL and so no method's names, and its arguments are fixed.
	/// </summary>
	static bool JSONMethod(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		if (cursor.Kind != SqlTokenKind.Word)
			return false;

		var written  = cursor.TextOf(cursor.Token);
		var spelling = written.ToUpperInvariant();

		switch (spelling)
		{
			case "TYPE":
			case "SIZE":
			case "DOUBLE":
			case "CEILING":
			case "FLOOR":
			case "ABS":
			case "KEYVALUE":
			case "BIGINT":
			case "BOOLEAN":
			case "DATETIME":
			case "DATE":
			case "INTEGER":
			case "NUMBER":
			case "STRING":
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = Method(written, null, null);

					return true;
				}

				break;

			case "DECIMAL":
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen))
				{
					string? precision = null, scale = null;

					if (UnsignedText(ref cursor, out precision) && cursor.Take(SqlTokenKind.Comma) && !UnsignedText(ref cursor, out scale))
						break;

					if (cursor.Take(SqlTokenKind.RightParen))
					{
						value = Method(written, precision, scale);

						return true;
					}
				}

				break;

			case "TIMESTAMP_TZ":
			case "TIMESTAMP":
			case "TIME_TZ":
			case "TIME":
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen))
				{
					UnsignedText(ref cursor, out var precision);

					if (cursor.Take(SqlTokenKind.RightParen))
					{
						value = Method(written, precision, null);

						return true;
					}
				}

				break;
		}

		cursor = save;

		return false;
	}

	/// <summary>A JSON item method: its name as written, and the numbers in its brackets.</summary>
	static Expression Method(string word, string? first, string? second)
	{
		var arguments = new List<Argument>();

		if (first is not null)
			arguments.Add(new Argument(new Expression.Literal(NumericLiteral(first))));

		if (second is not null)
			arguments.Add(new Argument(new Expression.Literal(NumericLiteral(second))));

		return new Expression.Member(null!, MemberAccessKind.Dot, new Identifier(word), arguments);
	}

	/// <summary>An unsigned integer as it was written, which the tree keeps.</summary>
	static bool UnsignedText(ref SqlCursor cursor, out string? text)
	{
		if (cursor.Kind != SqlTokenKind.Number || !IsUnsignedInteger(cursor.Span))
		{
			text = null;

			return false;
		}

		text = cursor.TextOf(cursor.Token);

		cursor.Take();

		return true;
	}

	/// <summary>An item method as the path language writes it.</summary>
	static JsonMethod PathMethod(Expression method)
	{
		var member    = (Expression.Member)method;
		var arguments = member.Arguments ?? [];

		int? Number(int at) =>
			arguments.Count > at ? (int)Long(((LiteralValue.Numeric)((Expression.Literal)arguments[at].Value).Value).Text.AsSpan()) : null;

		return member.Name.Text.ToUpperInvariant() switch
		{
			"TYPE"         => new JsonMethod(JsonMethodKind.Type),
			"SIZE"         => new JsonMethod(JsonMethodKind.Size),
			"DOUBLE"       => new JsonMethod(JsonMethodKind.Double),
			"CEILING"      => new JsonMethod(JsonMethodKind.Ceiling),
			"FLOOR"        => new JsonMethod(JsonMethodKind.Floor),
			"ABS"          => new JsonMethod(JsonMethodKind.Abs),
			"KEYVALUE"     => new JsonMethod(JsonMethodKind.KeyValue),
			"BIGINT"       => new JsonMethod(JsonMethodKind.BigInt),
			"BOOLEAN"      => new JsonMethod(JsonMethodKind.Boolean),
			"DATETIME"     => new JsonMethod(JsonMethodKind.DateTime),
			"DATE"         => new JsonMethod(JsonMethodKind.Date),
			"INTEGER"      => new JsonMethod(JsonMethodKind.Integer),
			"NUMBER"       => new JsonMethod(JsonMethodKind.Number),
			"STRING"       => new JsonMethod(JsonMethodKind.String),
			"DECIMAL"      => new JsonMethod(JsonMethodKind.Decimal, Number(0), Number(1)),
			"TIMESTAMP_TZ" => new JsonMethod(JsonMethodKind.TimestampTz, Number(0)),
			"TIMESTAMP"    => new JsonMethod(JsonMethodKind.Timestamp, Number(0)),
			"TIME_TZ"      => new JsonMethod(JsonMethodKind.TimeTz, Number(0)),
			_              => new JsonMethod(JsonMethodKind.Time, Number(0)),
		};
	}

	/// <summary><c>&lt;JSON subscript&gt;</c>: one path expression, or a range of two.</summary>
	static bool JSONSubscript(ref SqlCursor cursor, out JsonSubscript subscript)
	{
		var save = cursor;

		subscript = null!;

		if (!JSONPathWff(ref cursor, out var from))
			return false;

		JsonPathExpression? to = null;

		if (cursor.Take(SqlWord.To) && !JSONPathWff(ref cursor, out to))
		{
			cursor = save;

			return false;
		}

		subscript = new JsonSubscript(from, to);

		return true;
	}

	/// <summary>The path language's arithmetic over its primaries.</summary>
	static bool JSONPathWff(ref SqlCursor cursor, out JsonPathExpression value)
	{
		if (!JSONPathMultiplicative(ref cursor, out value))
			return false;

		while (cursor.Kind is SqlTokenKind.Plus or SqlTokenKind.Minus)
		{
			var op = cursor.Kind == SqlTokenKind.Plus ? JsonPathBinaryOperator.Add : JsonPathBinaryOperator.Subtract;
			var save = cursor;

			cursor.Take();

			if (!JSONPathMultiplicative(ref cursor, out var right))
			{
				cursor = save;

				break;
			}

			value = new JsonPathExpression.Binary(value, op, right);
		}

		return true;
	}

	static bool JSONPathMultiplicative(ref SqlCursor cursor, out JsonPathExpression value)
	{
		if (!JSONPathUnary(ref cursor, out value))
			return false;

		while (cursor.Kind is SqlTokenKind.Asterisk or SqlTokenKind.Solidus or SqlTokenKind.Percent)
		{
			var op = cursor.Kind switch
			{
				SqlTokenKind.Asterisk => JsonPathBinaryOperator.Multiply,
				SqlTokenKind.Solidus  => JsonPathBinaryOperator.Divide,
				_                     => JsonPathBinaryOperator.Modulo,
			};

			var save = cursor;

			cursor.Take();

			if (!JSONPathUnary(ref cursor, out var right))
			{
				cursor = save;

				break;
			}

			value = new JsonPathExpression.Binary(value, op, right);
		}

		return true;
	}

	static bool JSONPathUnary(ref SqlCursor cursor, out JsonPathExpression value)
	{
		var save  = cursor;
		var signs = new List<bool>();

		while (cursor.Kind is SqlTokenKind.Plus or SqlTokenKind.Minus)
		{
			signs.Add(cursor.Kind == SqlTokenKind.Minus);

			cursor.Take();
		}

		if (!JSONPathAccessor(ref cursor, out value))
		{
			cursor = save;

			return false;
		}

		for (var at = signs.Count - 1; at >= 0; at--)
			value = new JsonPathExpression.Unary(signs[at] ? JsonPathUnaryOperator.Minus : JsonPathUnaryOperator.Plus, value);

		return true;
	}

	static bool JSONPathAccessor(ref SqlCursor cursor, out JsonPathExpression value)
	{
		if (!JSONPathPrimary(ref cursor, out value))
			return false;

		while (JSONPathAccessorOp(ref cursor, out var accessor))
			value = new JsonPathExpression.Access(value, accessor);

		return true;
	}

	static bool JSONPathPrimary(ref SqlCursor cursor, out JsonPathExpression value)
	{
		var save = cursor;

		value = null!;

		if (cursor.Kind == SqlTokenKind.Unknown && cursor.Span.Length == 1 && cursor.Span[0] == '$')
		{
			cursor.Take();

			value = new JsonPathExpression.Variable(JsonPathVariableKind.Context);

			return true;
		}

		if (cursor.TakeWord("LAST"))
		{
			value = new JsonPathExpression.Variable(JsonPathVariableKind.Last);

			return true;
		}

		if (cursor.Take(SqlTokenKind.LeftParen) && JSONPathWff(ref cursor, out var inner) && cursor.Take(SqlTokenKind.RightParen))
		{
			value = new JsonPathExpression.Parenthesized(inner);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool JSONPathAccessorOp(ref SqlCursor cursor, out JsonPathAccessor accessor)
	{
		var save = cursor;

		accessor = null!;

		if (cursor.Take(SqlTokenKind.Dot))
		{
			if (cursor.Take(SqlTokenKind.Asterisk))
			{
				accessor = new JsonPathAccessor.WildcardMember();

				return true;
			}

			if (JSONMethod(ref cursor, out var method))
			{
				accessor = new JsonPathAccessor.Method(PathMethod(method));

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlTokenKind.LeftBracket))
		{
			if (cursor.Take(SqlTokenKind.Asterisk) && cursor.Take(SqlTokenKind.RightBracket))
			{
				accessor = new JsonPathAccessor.WildcardArray();

				return true;
			}

			cursor = save;
			cursor.Take();

			var subscripts = new List<JsonSubscript>();

			while (true)
			{
				if (!JSONSubscript(ref cursor, out var subscript))
					break;

				subscripts.Add(subscript);

				if (!cursor.Take(SqlTokenKind.Comma))
					break;
			}

			if (subscripts.Count > 0 && cursor.Take(SqlTokenKind.RightBracket))
			{
				accessor = new JsonPathAccessor.Array(subscripts);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Take(SqlTokenKind.Question) && cursor.Take(SqlTokenKind.LeftParen) && JSONPathPredicate(ref cursor, out var predicate) &&
			cursor.Take(SqlTokenKind.RightParen))
		{
			accessor = new JsonPathAccessor.Filter(predicate);

			return true;
		}

		cursor = save;

		return false;
	}

	/// <summary>
	/// The path language's predicates, as far as the SQL tokens reach: <c>&amp;&amp;</c>, <c>!</c>,
	/// <c>==</c> and <c>!=</c> are no SQL tokens, so a conjunction, a negation and those comparisons
	/// are never read; <c>||</c> is the concatenation operator's token, and a disjunction is.
	/// </summary>
	static bool JSONPathPredicate(ref SqlCursor cursor, out JsonPathPredicate value)
	{
		if (!JSONPredicatePrimary(ref cursor, out value))
			return false;

		List<JsonPathPredicate>? rest = null;

		while (cursor.Kind == SqlTokenKind.Concat)
		{
			var save = cursor;

			cursor.Take();

			if (!JSONPredicatePrimary(ref cursor, out var next))
			{
				cursor = save;

				break;
			}

			(rest ??= [value]).Add(next);
		}

		if (rest is not null)
			value = new JsonPathPredicate.Or(rest);

		return true;
	}

	static bool JSONPredicatePrimary(ref SqlCursor cursor, out JsonPathPredicate value)
	{
		var save = cursor;

		value = null!;

		if (cursor.Take(SqlWord.Exists))
		{
			if (cursor.Take(SqlTokenKind.LeftParen) && JSONPathWff(ref cursor, out var inner) && cursor.Take(SqlTokenKind.RightParen))
			{
				value = new JsonPathPredicate.Exists(inner);

				return true;
			}

			cursor = save;

			return false;
		}

		if (cursor.Kind == SqlTokenKind.LeftParen)
		{
			var bracket = cursor;

			cursor.Take();

			if (JSONPathPredicate(ref cursor, out var inner) && cursor.Take(SqlTokenKind.RightParen))
			{
				var unknown = cursor;

				if (cursor.Take(SqlWord.Is) && cursor.Take(SqlWord.Unknown))
				{
					value = new JsonPathPredicate.IsUnknown(inner);

					return true;
				}

				cursor = unknown;
				value  = new JsonPathPredicate.Parenthesized(inner);

				return true;
			}

			cursor = bracket;
		}

		if (JSONPathWff(ref cursor, out var left))
		{
			var op = cursor.Kind switch
			{
				SqlTokenKind.NotEqual       => JsonPathComparisonOperator.NotEqual,
				SqlTokenKind.LessOrEqual    => JsonPathComparisonOperator.LessOrEqual,
				SqlTokenKind.GreaterOrEqual => JsonPathComparisonOperator.GreaterOrEqual,
				SqlTokenKind.Less           => JsonPathComparisonOperator.Less,
				SqlTokenKind.Greater        => JsonPathComparisonOperator.Greater,
				_                           => (JsonPathComparisonOperator?)null,
			};

			if (op is not null)
			{
				cursor.Take();

				if (JSONPathWff(ref cursor, out var right))
				{
					value = new JsonPathPredicate.Comparison(left, op.Value, right);

					return true;
				}
			}
		}

		cursor = save;

		return false;
	}

	// ── §7.11 JSON table ───────────────────────────────────────────────────────

	static bool JSONTable(ref SqlCursor cursor, out JsonTableDefinition table)
	{
		var save = cursor;

		table = null!;

		cursor.Take();

		if (cursor.Take(SqlTokenKind.LeftParen) && JSONAPICommonSyntax(ref cursor, out var common) &&
			JSONTableColumnsClause(ref cursor, out var columns))
		{
			var plan = JSONTablePlanClause(ref cursor);

			JsonTableErrorBehavior? error = null;

			var marked = cursor;

			if (cursor.TakeWord("ERROR"))
				error = JsonTableErrorBehavior.Error;
			else if (cursor.Take(SqlWord.Empty))
				error = JsonTableErrorBehavior.Empty;

			if (error is not null && !(cursor.Take(SqlWord.On) && cursor.TakeWord("ERROR")))
			{
				cursor = marked;
				error  = null;
			}

			if (cursor.Take(SqlTokenKind.RightParen))
			{
				table = new JsonTableDefinition(common, columns, plan, error);

				return true;
			}
		}

		cursor = save;

		return false;
	}

	static bool JSONTablePrimitive(ref SqlCursor cursor, out JsonTableDefinition table)
	{
		var save = cursor;

		table = null!;

		cursor.Take();

		if (cursor.Take(SqlTokenKind.LeftParen) && JSONAPICommonSyntax(ref cursor, out var common) && cursor.TakeWord("COLUMNS") &&
			cursor.Take(SqlTokenKind.LeftParen))
		{
			var columns = new List<JsonTableColumn>();

			while (true)
			{
				if (!JSONTablePrimitiveColumn(ref cursor, out var column))
					break;

				columns.Add(column);

				if (!cursor.Take(SqlTokenKind.Comma))
					break;
			}

			if (columns.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
			{
				JsonTableErrorBehavior? error =
					cursor.TakeWord("ERROR")   ? JsonTableErrorBehavior.Error :
					cursor.Take(SqlWord.Empty) ? JsonTableErrorBehavior.Empty :
					null;

				if (error is not null && cursor.Take(SqlWord.On) && cursor.TakeWord("ERROR") && cursor.Take(SqlTokenKind.RightParen))
				{
					table = new JsonTableDefinition(common, columns, null, error, true);

					return true;
				}
			}
		}

		cursor = save;

		return false;
	}

	static bool JSONTableColumnsClause(ref SqlCursor cursor, out IReadOnlyList<JsonTableColumn> columns)
	{
		var save = cursor;
		var read = new List<JsonTableColumn>();

		columns = read;

		if (!cursor.TakeWord("COLUMNS") || !cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return false;
		}

		while (true)
		{
			if (!JSONTableColumnDefinition(ref cursor, out var column))
				break;

			read.Add(column);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (read.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
			return true;

		cursor = save;

		return false;
	}

	static bool JSONTableColumnDefinition(ref SqlCursor cursor, out JsonTableColumn column)
	{
		var save = cursor;

		column = null!;

		if (cursor.TakeWord("NESTED"))
		{
			var keyword = cursor.TakeWord("PATH");

			if (PathSpecification(ref cursor, out var path))
			{
				Identifier? name = null;

				if (cursor.Take(SqlWord.As) && !Identifier(ref cursor, out name))
				{
					cursor = save;

					return false;
				}

				if (JSONTableColumnsClause(ref cursor, out var nested))
				{
					column = new JsonTableColumn.Nested(path, name, nested, keyword);

					return true;
				}
			}

			cursor = save;

			return false;
		}

		var ordinal = cursor;

		if (Identifier(ref cursor, out var ordinality) && cursor.Take(SqlWord.For) && cursor.TakeWord("ORDINALITY"))
		{
			column = new JsonTableColumn.Ordinality(ordinality);

			return true;
		}

		cursor = ordinal;

		return JSONTableTypedColumn(ref cursor, out column);
	}

	static bool JSONTablePrimitiveColumn(ref SqlCursor cursor, out JsonTableColumn column)
	{
		var save = cursor;

		column = null!;

		if (Identifier(ref cursor, out var name) && cursor.Take(SqlWord.For))
		{
			if (cursor.TakeWord("ORDINALITY"))
			{
				column = new JsonTableColumn.Ordinality(name);

				return true;
			}

			if (cursor.TakeWord("CHAINING"))
			{
				column = new JsonTableColumn.Chaining(name);

				return true;
			}
		}

		cursor = save;

		return JSONTableTypedColumn(ref cursor, out column);
	}

	/// <summary>
	/// A regular column and a formatted one are read once: each is a name, a type, a path and what
	/// happens on empty and on error, and what only one of them may say tells them apart.
	/// </summary>
	static bool JSONTableTypedColumn(ref SqlCursor cursor, out JsonTableColumn column)
	{
		var save = cursor;

		column = null!;

		if (!Identifier(ref cursor, out var name) || !DataType(ref cursor, out var type))
		{
			cursor = save;

			return false;
		}

		var kinds  = 0;
		var format = Format(ref cursor);

		if (format is not null)
			kinds |= 2;

		string? path = null;

		if (cursor.TakeWord("PATH") && !PathSpecification(ref cursor, out path))
		{
			cursor = save;

			return false;
		}

		var wrapper = JSONQueryWrapperBehavior(ref cursor);

		if (wrapper is not null)
			kinds |= 2;

		var quotes = JSONQueryQuotes(ref cursor);

		if (quotes is not null)
			kinds |= 2;

		var empty = JSONColumnBehavior(ref cursor, true, ref kinds);
		var error = JSONColumnBehavior(ref cursor, false, ref kinds);

		if (kinds == 3)
		{
			cursor = save;

			return false;
		}

		column = kinds == 2
			? new JsonTableColumn.Formatted(name, type, format?.Representation, path, wrapper, quotes, empty.Query, error.Query)
			: new JsonTableColumn.Regular(name, type, path, empty.Value, error.Value);

		return true;
	}

	/// <summary>What a JSON table column does on empty or on error, as both kinds of column say it.</summary>
	static (JsonValueBehavior? Value, JsonQueryBehavior? Query) JSONColumnBehavior(ref SqlCursor cursor, bool empty, ref int kinds)
	{
		var save = cursor;

		JsonValueBehavior? value = null;
		JsonQueryBehavior? query = null;
		var kind = 0;

		if (cursor.TakeWord("ERROR"))
		{
			value = new JsonValueBehavior.Error();
			query = Ast.JsonQueryBehavior.Error;
		}
		else if (cursor.Take(SqlWord.Null))
		{
			value = new JsonValueBehavior.Null();
			query = Ast.JsonQueryBehavior.Null;
		}
		else if (cursor.Take(SqlWord.Default))
		{
			if (Value(ref cursor, out var otherwise))
			{
				value = new JsonValueBehavior.Default(otherwise);
				kind  = 1;
			}
		}
		else if (cursor.Take(SqlWord.Empty))
		{
			if (cursor.Take(SqlWord.Array))
			{
				query = Ast.JsonQueryBehavior.EmptyArray;
				kind  = 2;
			}
			else if (cursor.TakeWord("OBJECT"))
			{
				query = Ast.JsonQueryBehavior.EmptyObject;
				kind  = 2;
			}
		}

		if ((value is not null || query is not null) && cursor.Take(SqlWord.On) && (empty ? cursor.Take(SqlWord.Empty) : cursor.TakeWord("ERROR")))
		{
			kinds |= kind;

			return (value, query);
		}

		cursor = save;

		return (null, null);
	}

	static JsonTablePlan? JSONTablePlanClause(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.TakeWord("PLAN"))
			return null;

		if (cursor.Take(SqlWord.Default))
		{
			if (cursor.Take(SqlTokenKind.LeftParen))
			{
				var inner = JSONTableDefaultPlanChoices(ref cursor);

				if (inner is not null && cursor.Take(SqlTokenKind.RightParen))
					return inner;
			}

			cursor = save;

			return null;
		}

		if (cursor.Take(SqlTokenKind.LeftParen) && JSONTablePlan(ref cursor, out var plan) && cursor.Take(SqlTokenKind.RightParen))
			return plan;

		cursor = save;

		return null;
	}

	static JsonTablePlan? JSONTableDefaultPlanChoices(ref SqlCursor cursor)
	{
		JsonTableDefaultInnerOuter? InnerOuter(ref SqlCursor reading) =>
			reading.Take(SqlWord.Inner) ? JsonTableDefaultInnerOuter.Inner :
			reading.Take(SqlWord.Outer) ? JsonTableDefaultInnerOuter.Outer :
			null;

		JsonTableDefaultUnionCross? UnionCross(ref SqlCursor reading) =>
			reading.Take(SqlWord.Union) ? JsonTableDefaultUnionCross.Union :
			reading.Take(SqlWord.Cross) ? JsonTableDefaultUnionCross.Cross :
			null;

		var save   = cursor;
		var inner  = InnerOuter(ref cursor);

		if (inner is not null)
		{
			JsonTableDefaultUnionCross? union = null;

			if (cursor.Take(SqlTokenKind.Comma))
			{
				union = UnionCross(ref cursor);

				if (union is null)
				{
					cursor = save;

					return null;
				}
			}

			return new JsonTablePlan.Default(inner, union);
		}

		var cross = UnionCross(ref cursor);

		if (cross is not null)
		{
			JsonTableDefaultInnerOuter? second = null;

			if (cursor.Take(SqlTokenKind.Comma))
			{
				second = InnerOuter(ref cursor);

				if (second is null)
				{
					cursor = save;

					return null;
				}
			}

			return new JsonTablePlan.Default(second, cross, true);
		}

		cursor = save;

		return null;
	}

	static bool JSONTablePlan(ref SqlCursor cursor, out JsonTablePlan plan)
	{
		var save = cursor;

		if (!JSONTablePlanPrimary(ref cursor, out plan))
			return false;

		// A parent is a name, and a plan in brackets alone is no plan.
		if (cursor.Word is SqlWord.Outer or SqlWord.Inner)
		{
			var outer = cursor.Word == SqlWord.Outer;

			cursor.Take();

			if (plan is JsonTablePlan.Name parent && JSONTablePlanPrimary(ref cursor, out var child))
			{
				plan = outer ? new JsonTablePlan.Outer(parent.Value, child) : new JsonTablePlan.Inner(parent.Value, child);

				return true;
			}

			cursor = save;
			plan   = null!;

			return false;
		}

		if (cursor.Word is SqlWord.Union or SqlWord.Cross)
		{
			var union = cursor.Word == SqlWord.Union;
			var items = new List<JsonTablePlan> { plan };

			while (cursor.Take(union ? SqlWord.Union : SqlWord.Cross))
			{
				if (!JSONTablePlanPrimary(ref cursor, out var next))
				{
					cursor = save;
					plan   = null!;

					return false;
				}

				items.Add(next);
			}

			plan = union ? new JsonTablePlan.Union(items) : new JsonTablePlan.Cross(items);

			return true;
		}

		return true;
	}

	static bool JSONTablePlanPrimary(ref SqlCursor cursor, out JsonTablePlan plan)
	{
		var save = cursor;

		if (Identifier(ref cursor, out var name))
		{
			plan = new JsonTablePlan.Name(name);

			return true;
		}

		if (cursor.Take(SqlTokenKind.LeftParen) && JSONTablePlan(ref cursor, out var inner) && cursor.Take(SqlTokenKind.RightParen))
		{
			plan = new JsonTablePlan.Parenthesized(inner);

			return true;
		}

		cursor = save;
		plan   = null!;

		return false;
	}
}
