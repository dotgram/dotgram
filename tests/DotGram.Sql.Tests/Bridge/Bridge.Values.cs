using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.Tests;

static partial class Bridge
{
	/// <summary>
	/// Names, literals, variables, operators, data types and functions: every <see cref="DotGram.Sql.Expression"/>,
	/// and the strings the old tree keeps names and types in.
	/// </summary>
	internal static class Values
	{
		public static readonly string[] Owns =
		[
			.. typeof(Expression).GetNestedTypes().Where(static one => one.IsSubclassOf(typeof(Expression))).Select(NameOf),
			"Clause.When",
			"Clause.Window",
			"Clause.WindowDefinition",
		];

		public static bool TryConvert(object old, [NotNullWhen(true)] out Ast.ISqlNode? made)
		{
			made = old is Expression expression ? Of(expression) : null;

			return made is not null;
		}

		public static Ast.Expression Of(Expression old)
		{
			return old switch
			{
				Expression.Or(var left, var right)          => Binary(left, Ast.BinaryOperator.Or, right),
				Expression.And(var left, var right)         => Binary(left, Ast.BinaryOperator.And, right),
				Expression.Not(var operand)                 => new Ast.Expression.Unary(Ast.UnaryOperator.Not, Of(operand)),
				Expression.IsTruth(var operand, var not, var truth) => new Ast.Expression.IsTruth(Of(operand), not, Truth(truth)),

				Expression.Add(var left, var right)         => Binary(left, Ast.BinaryOperator.Add, right),
				Expression.Subtract(var left, var right)    => Binary(left, Ast.BinaryOperator.Subtract, right),
				Expression.Concatenate(var left, var right) => Binary(left, Ast.BinaryOperator.Concatenate, right),
				Expression.Multiply(var left, var right)    => Binary(left, Ast.BinaryOperator.Multiply, right),
				Expression.Divide(var left, var right)      => Binary(left, Ast.BinaryOperator.Divide, right),
				Expression.Modulo(var left, var right)      => Binary(left, Ast.BinaryOperator.Modulo, right),
				Expression.BitwiseAnd(var left, var right)  => Binary(left, Ast.BinaryOperator.BitwiseAnd, right),
				Expression.BitwiseOr(var left, var right)   => Binary(left, Ast.BinaryOperator.BitwiseOr, right),
				Expression.BitwiseXor(var left, var right)  => Binary(left, Ast.BinaryOperator.BitwiseXor, right),
				Expression.ShiftLeft(var left, var right)   => Binary(left, Ast.BinaryOperator.ShiftLeft, right),
				Expression.ShiftRight(var left, var right)  => Binary(left, Ast.BinaryOperator.ShiftRight, right),

				Expression.Negate(var operand)              => new Ast.Expression.Unary(Ast.UnaryOperator.Minus, Of(operand)),
				Expression.Plus(var operand)                => new Ast.Expression.Unary(Ast.UnaryOperator.Plus, Of(operand)),
				Expression.BitwiseNot(var operand)          => new Ast.Expression.Unary(Ast.UnaryOperator.BitwiseNot, Of(operand)),

				Expression.Comparison(var left, var op, var right) =>
					new Ast.Expression.Comparison(Of(left), Comparison(op), Of(right), op == SqlComparison.NotEqualBang),

				Expression.Quantified quantified            => Quantified(quantified),

				Expression.Between(var value, var not, var low, var high) =>
					new Ast.Expression.Between(Of(value), not, null, Of(low), Of(high)),

				Expression.In(var value, var not, var from) => new Ast.Expression.In(Of(value), not, InSource(old, from)),

				// T-SQL's `{ESCAPE '%'}`: the escape value itself, written in ODBC's braces.
				Expression.Like(var value, var not, var pattern, Expression.OdbcEscape(var kind, var braced, _)) =>
					kind.Equals("ESCAPE", StringComparison.OrdinalIgnoreCase)
						? new Ast.Expression.Like(Of(value), not, Ast.LikeKind.Like, Of(pattern), Of(braced)) { BracedEscape = true }
						: throw BridgeGap.Open(old, $"inventory Expression.Like (an escape in braces of kind `{kind}`)"),

				Expression.Like(var value, var not, var pattern, var escape) =>
					new Ast.Expression.Like(Of(value), not, Ast.LikeKind.Like, Of(pattern), escape is null ? null : Of(escape)),

				Expression.IsNull(var value, var not)       => new Ast.Expression.IsNull(Of(value), not),
				Expression.Exists(var query)                => new Ast.Expression.Exists(QueryOf(query)),
				Expression.Unique(var query)                => new Ast.Expression.Unique(null, QueryOf(query)),
				Expression.Match match                      => Match(match),
				Expression.Overlaps(var left, var right)    => new Ast.Expression.Overlaps(Of(left), Of(right)),

				Expression.IsDistinctFrom(var left, var not, var right) =>
					new Ast.Expression.IsDistinct(Of(left), not, Of(right)),

				Expression.RoutineInvocation call           => Function(call),
				Expression.WindowFunction windowed          => Windowed(windowed),

				Expression.Case(var operand, var whens, var otherwise) => new Ast.Expression.Case(
					operand is null ? null : Of(operand),
					Each(whens, When),
					otherwise is null ? null : Of(otherwise)),

				// `t.*` as an argument: all columns of `t`, its target the table's reference, as the standard's parser builds it.
				Expression.ColumnReference(var text) when text.Length > 2 && text.EndsWith(".*", StringComparison.Ordinal) =>
					new Ast.Expression.Wildcard(new Ast.Expression.Reference(Name(old, text[..^2])), Ast.WildcardKind.Member),

				// A bare `*` where a value stands: CONTAINS, FREETEXT and the semantic-search functions' every column.
				Expression.ColumnReference("*")             => new Ast.Expression.Asterisk(),
				Expression.ColumnReference(var text)        => new Ast.Expression.Reference(Name(old, text)),
				Expression.Literal literal                  => Literal(literal),
				Expression.RowValueConstructor(var items)   => new Ast.Expression.Row(Each(items, Of)),

				Expression.Member(var of, var by, var name, var arguments) => new Ast.Expression.Member(
					Of(of),
					by == "::" ? Ast.MemberAccessKind.StaticMethod : Ast.MemberAccessKind.Dot,
					Part(old, name),
					arguments is { } passed ? Each(passed, Argument) : null),

				Expression.Collated(var value, var collation) =>
					new Ast.Expression.Collate(Of(value), new Ast.CollationName(Name(old, collation))),

				Expression.Parenthesized(var value)         => Parenthesized(Of(value)),
				Expression.Subquery(var query)              => new Ast.Expression.Subquery(QueryOf(query)),

				Expression.GraphMatch(var pattern) => Queries.GraphMatch(old, pattern),
				Expression.RowsetOrder     => throw BridgeGap.Missing(old, "a typed T-SQL rowset table source (proposal 7)"),
				Expression.Prefixed        => throw BridgeGap.Missing(old, "a typed T-SQL rowset table source (proposal 7)"),
				Expression.Pieced          => throw BridgeGap.Missing(old, "a typed T-SQL rowset table source (proposal 7)"),
				Expression.Measured        => throw BridgeGap.Unwritten(old, "Expression.Measured"),
				Expression.OdbcEscape escape => Odbc(escape),
				Expression.Hinted          => throw BridgeGap.Open(old, "inventory Expression.Hinted"),
				Expression.JsonPair        => throw BridgeGap.Open(old, "inventory Expression.JsonPair"),
				Expression.NamedArgument   => throw BridgeGap.Open(old, "inventory Expression.NamedArgument"),
				Expression.Aliased         => throw BridgeGap.Open(old, "inventory Expression.Aliased"),

				_ => throw BridgeGap.Missing(old, "no arm"),
			};
		}

		/// <summary>
		/// A pair of brackets around a value. Around a subquery, which has brackets of its own, the new tree
		/// takes the second pair as the query's (<see cref="Ast.Query.Parenthesized"/>): <c>((SELECT 1))</c> is a
		/// subquery of a bracketed query, where the old tree has a bracketed subquery.
		/// </summary>
		static Ast.Expression Parenthesized(Ast.Expression value)
		{
			return value is Ast.Expression.Subquery(var query)
				? new Ast.Expression.Subquery(new Ast.Query.Parenthesized(query))
				: new Ast.Expression.Parenthesized(value);
		}

		static Ast.Expression Binary(Expression left, Ast.BinaryOperator op, Expression right)
		{
			return new Ast.Expression.Binary(Of(left), op, Of(right));
		}

		static Ast.BooleanLiteral Truth(SqlTruth truth)
		{
			return truth switch
			{
				SqlTruth.True  => Ast.BooleanLiteral.True,
				SqlTruth.False => Ast.BooleanLiteral.False,
				_              => Ast.BooleanLiteral.Unknown,
			};
		}

		public static Ast.ComparisonOperator Comparison(SqlComparison op)
		{
			return op switch
			{
				SqlComparison.Equal          => Ast.ComparisonOperator.Equal,
				SqlComparison.NotEqual       => Ast.ComparisonOperator.NotEqual,
				SqlComparison.NotEqualBang   => Ast.ComparisonOperator.NotEqual,
				SqlComparison.Less           => Ast.ComparisonOperator.Less,
				SqlComparison.LessOrEqual    => Ast.ComparisonOperator.LessOrEqual,
				SqlComparison.Greater        => Ast.ComparisonOperator.Greater,
				SqlComparison.GreaterOrEqual => Ast.ComparisonOperator.GreaterOrEqual,
				SqlComparison.NotLess        => Ast.ComparisonOperator.NotLess,
				SqlComparison.NotGreater     => Ast.ComparisonOperator.NotGreater,
				_                            => throw new ArgumentOutOfRangeException(nameof(op), op, null),
			};
		}

		static Ast.Expression Quantified(Expression.Quantified old)
		{
			if (old.Operator == SqlComparison.NotEqualBang)
				throw BridgeGap.Missing(old, "QuantifiedComparison has no spelling flag for `!=`");

			var quantifier = old.Quantifier.ToUpperInvariant() switch
			{
				"ALL"  => Ast.Quantifier.All,
				"SOME" => Ast.Quantifier.Some,
				"ANY"  => Ast.Quantifier.Any,
				_      => throw BridgeGap.Open(old, $"inventory Expression.Quantified (quantifier `{old.Quantifier}`)"),
			};

			return new Ast.Expression.QuantifiedComparison(Of(old.Left), Comparison(old.Operator), quantifier, QueryOf(old.Query));
		}

		static Ast.InSource InSource(Expression owner, Expression from)
		{
			return from switch
			{
				Expression.RowValueConstructor(var items) => new Ast.InSource.Values(Each(items, Of)),
				Expression.Subquery(var query)            => new Ast.InSource.Subquery(QueryOf(query)),
				_                                         => throw BridgeGap.Open(owner, $"inventory Expression.In (a source that is {NameOf(from)})"),
			};
		}

		static Ast.Expression Match(Expression.Match old)
		{
			var words  = old.Qualifier is null ? [] : Words(old.Qualifier);
			var unique = words.Contains("UNIQUE");
			var rest   = words.Where(static one => one != "UNIQUE").ToArray();

			Ast.MatchType? type = rest switch
			{
				[]            => null,
				["FULL"]      => Ast.MatchType.Full,
				["PARTIAL"]   => Ast.MatchType.Partial,
				["SIMPLE"]    => Ast.MatchType.Simple,
				_             => throw BridgeGap.Open(old, $"inventory Expression.Match (qualifier `{old.Qualifier}`)"),
			};

			return new Ast.Expression.Match(Of(old.Value), unique, type, QueryOf(old.Query));
		}

		static Ast.CaseWhen When(Clause clause)
		{
			return clause is Clause.When(var test, var result)
				? new Ast.CaseWhen([Of(test)], Of(result))
				: throw BridgeGap.Open(clause, "inventory Clause.When (a CASE arm that is not a WHEN)");
		}

		public static Ast.Argument Argument(Expression value)
		{
			return new Ast.Argument(Of(value));
		}

		// ── Literals ──────────────────────────────────────────────────────────────

		static Ast.Expression Literal(Expression.Literal old)
		{
			var text = old.Text;

			switch (old.Kind)
			{
				case SqlLiteralKind.Number:
					return Value(new Ast.LiteralValue.Numeric(text, Numeric(text)));

				case SqlLiteralKind.Text:
					return Value(new Ast.LiteralValue.String(text, Ast.StringLiteralKind.Character));

				case SqlLiteralKind.National:
					return Value(new Ast.LiteralValue.String(text.Substring(1), Ast.StringLiteralKind.National));

				case SqlLiteralKind.Null:
					return Value(new Ast.LiteralValue.Null());

				case SqlLiteralKind.Default:
					return new Ast.Expression.Default();

				case SqlLiteralKind.Special:
					return Special(old);

				case SqlLiteralKind.Parameter when text.StartsWith('@'):
					return new Ast.Expression.Variable(new Ast.Identifier(text));

				case SqlLiteralKind.Parameter when text == "?":
					return new Ast.Expression.Parameter(Ast.ParameterKind.Dynamic);

				// The digits after the introducer, as a string's text leaves its introducer out.
				case SqlLiteralKind.Hex:
					return Value(new Ast.LiteralValue.Binary(text[2..]) { ZeroX = true });

				case SqlLiteralKind.Bit:
					throw BridgeGap.Missing(old, "a bit string literal, which SQL:2023 removed");

				default:
					throw BridgeGap.Open(old, $"inventory SqlLiteralKind ({old.Kind})");
			}
		}

		static Ast.Expression Value(Ast.LiteralValue value)
		{
			return value;
		}

		/// <summary>
		/// A string literal as one token of text, <c>'a'</c> or <c>N'a'</c>, where the old tree keeps it in a text.
		/// </summary>
		public static Ast.Expression String(string token)
		{
			return token[0] is 'N' or 'n'
				? Value(new Ast.LiteralValue.String(token[1..], Ast.StringLiteralKind.National))
				: Value(new Ast.LiteralValue.String(token, Ast.StringLiteralKind.Character));
		}

		/// <summary>
		/// What kind of number a numeric literal's text is: a currency sign in front is money, an exponent
		/// is an approximate number, a point is a decimal, and anything else an integer.
		/// </summary>
		static Ast.NumericLiteralKind Numeric(string text)
		{
			// A signed number, where the grammar reads one as a literal: the sign says nothing of the kind.
			text = text.TrimStart('+', '-', ' ', '\t');

			if (text.Length > 0 && !char.IsDigit(text[0]) && text[0] != '.')
				return Ast.NumericLiteralKind.Money;

			if (text.IndexOfAny(['e', 'E']) >= 0)
				return Ast.NumericLiteralKind.Approximate;

			return text.Contains('.') ? Ast.NumericLiteralKind.Decimal : Ast.NumericLiteralKind.DecimalInteger;
		}

		static Ast.Expression Special(Expression.Literal old)
		{
			Ast.CurrentValue kind = old.Text.ToUpperInvariant() switch
			{
				"USER"              => Ast.CurrentValue.User,
				"CURRENT_USER"      => Ast.CurrentValue.CurrentUser,
				"SESSION_USER"      => Ast.CurrentValue.SessionUser,
				"SYSTEM_USER"       => Ast.CurrentValue.SystemUser,
				"VALUE"             => Ast.CurrentValue.Value,
				"CURRENT_DATE"      => Ast.CurrentValue.Date,
				"CURRENT_TIME"      => Ast.CurrentValue.Time,
				"CURRENT_TIMESTAMP" => Ast.CurrentValue.Timestamp,
				_                   => throw BridgeGap.Open(old, $"inventory SqlLiteralKind (Special `{old.Text}`)"),
			};

			return new Ast.Expression.Current(kind);
		}

		// ── Functions ─────────────────────────────────────────────────────────────

		/// <summary>
		/// The JSON functions the standard has nodes of its own for, which T-SQL's old tree reads as calls.
		/// </summary>
		static readonly HashSet<string> Json = new(StringComparer.OrdinalIgnoreCase)
		{
			"JSON_OBJECT", "JSON_ARRAY", "JSON_VALUE", "JSON_QUERY", "JSON_EXISTS", "JSON_OBJECTAGG", "JSON_ARRAYAGG",
			"JSON_SCALAR", "JSON_SERIALIZE", "JSON_TABLE",
		};

		/// <summary>
		/// T-SQL's JSON constructors and functions as the standard's JSON nodes: the old tree keeps them as a call, a
		/// pair as a JsonPair, and the clauses after the last argument as its words. JSON_VALUE of another argument
		/// count stays a call, as the grammar reads it; the standard's functions T-SQL has no syntax for are a gap.
		/// </summary>
		static Ast.Expression? JsonCall(Expression.RoutineInvocation old, string upper)
		{
			// The words after the last argument, and that argument without them.
			var arguments = old.Arguments.ToList();
			var words     = "";

			if (arguments is [.., Expression.Hinted(var last, var tail)])
			{
				arguments[^1] = last;
				words         = tail;
			}

			// The clauses by their tokens' places, never by searching the text: JSON_VALUE's `RETURNING type` from
			// the first token, a constructor's `RETURNING JSON` the last two; then the null clause; then what is left.
			var tokens = Placed(words);
			var end    = tokens.Count;

			static bool Is(SqlTokens.Token token, string word)
			{
				return token.Kind == SqlTokens.Kind.Word && token.Text.Equals(word, StringComparison.OrdinalIgnoreCase);
			}

			string? returning = null;

			if (upper == "JSON_VALUE" && tokens is [var first, var type, ..] && Is(first.Token, "RETURNING"))
			{
				returning = words[type.At..].Trim();
				end       = 0;
			}
			else if (upper != "JSON_VALUE" && tokens is [.., var word, var json] && Is(word.Token, "RETURNING") && Is(json.Token, "JSON"))
			{
				returning = json.Token.Text;
				end       = tokens.Count - 2;
			}

			var said = tokens.Take(end).Select(static one => one.Token.Kind == SqlTokens.Kind.Word ? one.Token.Text.ToUpperInvariant() : one.Token.Text).ToList();

			Ast.JsonNullHandling? nulls = said switch
			{
				[.., "NULL", "ON", "NULL"]   => Ast.JsonNullHandling.NullOnNull,
				[.., "ABSENT", "ON", "NULL"] => Ast.JsonNullHandling.AbsentOnNull,
				_                            => null,
			};

			if (nulls is not null)
			{
				said = said[..^3];
				end -= 3;
			}

			// What is left as written, from its first token to the clauses after it.
			var rest = end == 0 ? "" : words[tokens[0].At..(end < tokens.Count ? tokens[end].At : words.Length)];

			Ast.JsonOutput? Output()
			{
				return returning is null ? null
					: returning.Equals("JSON", StringComparison.OrdinalIgnoreCase) ? new Ast.JsonOutput(new Ast.DataType.Json())
					: throw BridgeGap.Open(old, $"inventory Expression.RoutineInvocation ({upper} RETURNING {returning})");
			}

			Ast.JsonMember Member(Expression one)
			{
				return one is Expression.JsonPair(var key, var value)
					? new Ast.JsonMember(Of(key), Of(value), Ast.JsonMemberSyntax.Colon)
					: throw BridgeGap.Open(old, $"inventory Expression.RoutineInvocation ({upper} with a member that is not a key and a value)");
			}

			BridgeGap Rest()
			{
				return BridgeGap.Open(old, $"inventory Expression.RoutineInvocation ({upper} with the words `{words}`)");
			}

			switch (upper)
			{
				case "JSON_OBJECT":
					if (said.Count > 0)
						throw Rest();

					return new Ast.Expression.JsonObject(Each([.. arguments], Member), nulls, null, Output());

				case "JSON_ARRAY":
					if (said.Count > 0)
						throw Rest();

					return new Ast.Expression.JsonArray(Each([.. arguments], one => new Ast.JsonElement(Of(one))), nulls, Output());

				case "JSON_OBJECTAGG":
					if (said.Count > 0)
						throw Rest();

					return arguments is [var pair]
						? new Ast.Expression.JsonObjectAggregate(Member(pair), nulls, null, Output())
						: throw BridgeGap.Open(old, $"inventory Expression.RoutineInvocation (JSON_OBJECTAGG of {(arguments.Count == 0 ? "no pair" : "several pairs")}, where the record holds one)");

				case "JSON_ARRAYAGG":
					if (arguments is not [var item])
						throw BridgeGap.Open(old, "inventory Expression.RoutineInvocation (JSON_ARRAYAGG of other than one value, where the record holds one)");

					// What is left is the aggregate's order, read again as an order.
					var order = said.Count == 0 ? null
						: said is ["ORDER", "BY", ..] ? Ordering(old, rest)
						: throw Rest();

					return new Ast.Expression.JsonArrayAggregate(new Ast.JsonElement(Of(item)), order, nulls, Output());

				// JSON_VALUE (x, path [RETURNING type]); a type is its alone, and JSON is not one.
				case "JSON_VALUE" when arguments.Count == 2 && said.Count == 0 && nulls is null:
					return new Ast.Expression.JsonValue(
						new Ast.JsonApiCommon(Of(arguments[0]), Of(arguments[1]), null, []),
						returning is null ? null : Type(old, returning),
						null,
						null);

				case "JSON_VALUE":
					return null;

				// JSON_QUERY (x [, path] [WITH ARRAY WRAPPER]).
				case "JSON_QUERY" when arguments.Count is 1 or 2 && returning is null && nulls is null:
					Ast.JsonWrapperBehavior? wrapper = said switch
					{
						[]                            => null,
						["WITH", "ARRAY", "WRAPPER"] => Ast.JsonWrapperBehavior.WithArray,
						_                             => throw Rest(),
					};

					return new Ast.Expression.JsonQuery(
						new Ast.JsonApiCommon(Of(arguments[0]), arguments.Count > 1 ? Of(arguments[1]) : null, null, []),
						null,
						wrapper,
						null,
						null,
						null);

				default:
					throw BridgeGap.Open(old, $"inventory Expression.RoutineInvocation ({upper} as a call, which the standard reads as its own node)");
			}
		}

		/// <summary>
		/// The functions written without brackets, which the old tree keeps as a call with no arguments.
		/// </summary>
		static readonly Dictionary<string, Ast.CurrentValue> Niladic = new(StringComparer.OrdinalIgnoreCase)
		{
			["CURRENT_TIMESTAMP"] = Ast.CurrentValue.Timestamp,
			["CURRENT_DATE"]      = Ast.CurrentValue.Date,
			["CURRENT_TIME"]      = Ast.CurrentValue.Time,
		};

		public static Ast.Expression Function(Expression.RoutineInvocation old)
		{
			var name  = old.Name;
			var upper = name.ToUpperInvariant();

			// `CAST (DISTINCT 1)` reads, as any call does, with a quantifier where the type would stand.
			var type = old.Word is { } word && word.ToUpperInvariant() is not ("DISTINCT" or "ALL" or "*") ? word : null;

			if (upper is "CAST" or "TRY_CAST" && type is not null)
			{
				if (old.Arguments.Length != 1)
					throw BridgeGap.Open(old, "inventory Expression.RoutineInvocation (a CAST that is not one value and a type)");

				return new Ast.Expression.Cast(Of(old.Arguments[0]), Type(old, type)) { Try = upper == "TRY_CAST" };
			}

			// `[TRY_]CONVERT (type, value [, style])`: the type is the old call's word, and the arguments are the rest.
			if (upper is "CONVERT" or "TRY_CONVERT" && type is not null)
			{
				if (old.Arguments.Length is < 1 or > 2)
					throw BridgeGap.Open(old, "inventory Expression.RoutineInvocation (a CONVERT that is not a type, a value and a style)");

				return new Ast.Expression.Convert(
					Type(old, type),
					Of(old.Arguments[0]),
					old.Arguments.Length > 1 ? Of(old.Arguments[1]) : null,
					upper == "TRY_CONVERT");
			}

			// `[TRY_]PARSE (value AS type [USING culture])`: the culture is the second argument.
			if (upper is "PARSE" or "TRY_PARSE" && type is not null)
			{
				if (old.Arguments.Length is < 1 or > 2)
					throw BridgeGap.Open(old, "inventory Expression.RoutineInvocation (a PARSE that is not a value, a type and a culture)");

				return new Ast.Expression.Parse(
					Of(old.Arguments[0]),
					Type(old, type),
					old.Arguments.Length > 1 ? Of(old.Arguments[1]) : null,
					upper == "TRY_PARSE");
			}

			// `NEXT VALUE FOR s`: the sequence is the old call's word.
			if (Words(name) is ["NEXT", "VALUE", "FOR"])
			{
				if (old.Arguments.Length != 0 || old.Word is null)
					throw BridgeGap.Open(old, "inventory Expression.RoutineInvocation (a NEXT VALUE FOR without its sequence)");

				return new Ast.Expression.NextValue(Name(old, old.Word));
			}

			if (name.Equals("AT TIME ZONE", StringComparison.OrdinalIgnoreCase))
			{
				if (old.Arguments.Length != 2)
					throw BridgeGap.Open(old, "inventory Expression.RoutineInvocation (AT TIME ZONE without its two operands)");

				return new Ast.Expression.AtTimeZone(Of(old.Arguments[0]), Of(old.Arguments[1]));
			}

			if (name.Equals("TRIM", StringComparison.OrdinalIgnoreCase))
				return Trim(old);

			if (Json.Contains(name) && JsonCall(old, upper) is { } json)
				return json;

			if (Syntax.IsDatePartFunction(upper) && old.Word is null)
				return DatePart(old);

			if (old.Arguments.Length == 0 && old.Word is null && Niladic.TryGetValue(name, out var current))
				return new Ast.Expression.Current(current);

			if (old.Arguments.Length == 0 && old.Word is null && Bracketless(old))
				throw BridgeGap.Lost(old, "whether a call with no arguments was written with brackets");

			Ast.SetQuantifier? quantifier = null;
			SqlList<Ast.Argument> arguments;

			switch (old.Word?.ToUpperInvariant())
			{
				case null:
					arguments = Each(old.Arguments, Argument);
					break;

				case "*":
					arguments = [new Ast.Argument(new Ast.Expression.Asterisk())];
					break;

				case "DISTINCT":
					quantifier = Ast.SetQuantifier.Distinct;
					arguments  = Each(old.Arguments, Argument);
					break;

				case "ALL":
					quantifier = Ast.SetQuantifier.All;
					arguments  = Each(old.Arguments, Argument);
					break;

				default:
					throw BridgeGap.Open(old, $"inventory Expression.RoutineInvocation (the word `{old.Word}`)");
			}

			return new Ast.Expression.Invocation(Name(old, name), arguments) { Quantifier = quantifier };
		}

		/// <summary>
		/// A call whose first argument is a date part, <c>DATEADD (day, 1, d)</c>: the old tree reads the part as
		/// a column, and the new one as the <see cref="Ast.Expression.DatePart"/> it is.
		/// </summary>
		static Ast.Expression DatePart(Expression.RoutineInvocation old)
		{
			if (old.Arguments is not [Expression.ColumnReference(var part), ..] || part.Contains('.'))
				throw BridgeGap.Open(old, $"inventory Expression.RoutineInvocation ({old.Name} without a date part)");

			if (!DateParts.TryGetValue(part, out var made))
				throw BridgeGap.Open(old, $"inventory Expression.RoutineInvocation (a date part `{part}` the reference's table does not have)");

			return new Ast.Expression.Invocation(
				Name(old, old.Name),
				[new Ast.Argument(new Ast.Expression.DatePart(made.Kind, made.Spelling)), .. old.Arguments.Skip(1).Select(Argument)]);
		}

		/// <summary>
		/// The words of DATEADD's and DATEPART's table of date parts, each with its part and which of its words it
		/// is, compared without case.
		/// </summary>
		public static readonly Dictionary<string, (Ast.DatePartKind Kind, Ast.DatePartSpelling Spelling)> DateParts = Table();

		static Dictionary<string, (Ast.DatePartKind, Ast.DatePartSpelling)> Table()
		{
			var table = new Dictionary<string, (Ast.DatePartKind, Ast.DatePartSpelling)>(StringComparer.OrdinalIgnoreCase);

			void Add(Ast.DatePartKind kind, params string[] words)
			{
				for (var at = 0; at < words.Length; at++)
					table.Add(words[at], (kind, (Ast.DatePartSpelling)at));
			}

			Add(Ast.DatePartKind.Year,        "year", "yy", "yyyy");
			Add(Ast.DatePartKind.Quarter,     "quarter", "qq", "q");
			Add(Ast.DatePartKind.Month,       "month", "mm", "m");
			Add(Ast.DatePartKind.DayOfYear,   "dayofyear", "dy", "y");
			Add(Ast.DatePartKind.Day,         "day", "dd", "d");
			Add(Ast.DatePartKind.Week,        "week", "wk", "ww");
			Add(Ast.DatePartKind.Weekday,     "weekday", "dw", "w");
			Add(Ast.DatePartKind.Hour,        "hour", "hh");
			Add(Ast.DatePartKind.Minute,      "minute", "mi", "n");
			Add(Ast.DatePartKind.Second,      "second", "ss", "s");
			Add(Ast.DatePartKind.Millisecond, "millisecond", "ms");
			Add(Ast.DatePartKind.Microsecond, "microsecond", "mcs");
			Add(Ast.DatePartKind.Nanosecond,  "nanosecond", "ns");
			Add(Ast.DatePartKind.TzOffset,    "tzoffset", "tz");
			Add(Ast.DatePartKind.IsoWeek,     "iso_week", "isowk", "isoww");

			return table;
		}

		/// <summary>
		/// An ODBC escape, <c>{fn LEFT (a, 2)}</c>, <c>{d '2026-09-26'}</c>. A function inside one is ODBC's and
		/// not T-SQL's — <c>{fn CONVERT (a, SQL_INTEGER)}</c> names its type as ODBC does — so it is a plain call.
		/// </summary>
		static Ast.Expression Odbc(Expression.OdbcEscape old)
		{
			if (old.Kind.Length == 0)
				throw BridgeGap.Open(old, "inventory Expression.OdbcEscape (an escape without its kind)");

			var value = old.Value is Expression.RoutineInvocation { Word: null } call
				? new Ast.Expression.Invocation(Name(call, call.Name), Each(call.Arguments, Argument))
				: Of(old.Value);

			return new Ast.Expression.OdbcEscape(Part(old, old.Kind), value, old.Called);
		}

		/// <summary>
		/// T-SQL's <c>TRIM ([LEADING | TRAILING | BOTH] [characters FROM] string)</c>, which the old tree keeps as
		/// a call with the specification as its word: two arguments are the characters and the string, and
		/// <c>FROM</c> stood between them; one is the string, and <c>FROM</c> stood before it where the text says.
		/// </summary>
		static Ast.Expression Trim(Expression.RoutineInvocation old)
		{
			Ast.TrimSpecification? specification = old.Word?.ToUpperInvariant() switch
			{
				null       => null,
				"LEADING"  => Ast.TrimSpecification.Leading,
				"TRAILING" => Ast.TrimSpecification.Trailing,
				"BOTH"     => Ast.TrimSpecification.Both,
				_          => throw BridgeGap.Open(old, $"inventory Expression.RoutineInvocation (TRIM's word `{old.Word}`)"),
			};

			return old.Arguments switch
			{
				[var characters, var value] => new Ast.Expression.Trim(specification, Of(characters), true, Of(value)),
				[var value]                 => new Ast.Expression.Trim(specification, null,
					specification is not null && Recover("Expression.Trim.FromKeyword", Spelled(old), static text => Words(text).Contains("FROM"), false), Of(value)),
				_                           => throw BridgeGap.Open(old, "inventory Expression.RoutineInvocation (TRIM with no string)"),
			};
		}

		/// <summary>
		/// Whether a call with no arguments was written without its brackets, where the text says; a call
		/// not located is taken to have had them.
		/// </summary>
		static bool Bracketless(Expression.RoutineInvocation old)
		{
			return Recover("Expression.Invocation without brackets", Spelled(old), static text => !text.TrimEnd().EndsWith(')'), false);
		}

		static Ast.Expression Windowed(Expression.WindowFunction old)
		{
			if (old.Function is Expression.RoutineInvocation sequence && Function(sequence) is Ast.Expression.NextValue next)
			{
				// `NEXT VALUE FOR s OVER (ORDER BY …)`: a specification, never a window's name.
				if (old.Within is not null || old.Over is not Clause.Window window)
					throw BridgeGap.Open(old, "inventory Expression.WindowFunction (NEXT VALUE FOR over something that is not a window)");

				return next with { Over = Specification(window) };
			}

			// A JSON aggregate takes its window as the standard's record does; WITHIN GROUP after it has no place there.
			var function = old.Function is Expression.RoutineInvocation aggregate ? Function(aggregate) : null;

			if (function is Ast.Expression.JsonObjectAggregate or Ast.Expression.JsonArrayAggregate)
			{
				if (old.Within is not null || old.Over is null)
					throw BridgeGap.Open(old, "inventory Expression.WindowFunction (a JSON aggregate with WITHIN GROUP or a null treatment, which its record has no place for)");

				return function switch
				{
					Ast.Expression.JsonObjectAggregate objects => objects with { Over = Window(old.Over) },
					var arrays                                 => ((Ast.Expression.JsonArrayAggregate)arrays) with { Over = Window(old.Over) },
				};
			}

			// The words between the call and its window are text in the old tree: a null treatment is two words,
			// and WITHIN GROUP an order, which is read again as the old tree reads one.
			Ast.NullTreatment? nulls = null;
			Ast.WithinGroupClause? within = null;
			var path = false;

			if (old.Within is { } text)
			{
				switch (Words(text))
				{
					case ["IGNORE", "NULLS"]:
						nulls = Ast.NullTreatment.IgnoreNulls;
						break;

					case ["RESPECT", "NULLS"]:
						nulls = Ast.NullTreatment.RespectNulls;
						break;

					// SHORTEST_PATH's aggregates, over a graph path's nodes.
					case ["WITHIN", "GROUP", "(", "GRAPH", "PATH", ")"]:
						path = true;
						break;

					case ["WITHIN", "GROUP", "(", "ORDER", "BY", .., ")"]:
						within = new Ast.WithinGroupClause(Ordering(old, text[(text.IndexOf('(') + 1)..text.LastIndexOf(')')]));
						break;

					default:
						throw BridgeGap.Open(old, $"inventory Expression.WindowFunction (the words `{text}` before its window)");
				}
			}

			if (function is not Ast.Expression.Invocation invocation)
				throw BridgeGap.Open(old, "inventory Expression.WindowFunction (a window over something that is not a call)");

			invocation = invocation with { Nulls = nulls, WithinGroup = within, GraphPath = path };

			return old.Over is null ? invocation : invocation with { Over = Window(old.Over) };
		}

		/// <summary>
		/// The tokens of a stretch of text with where each begins in it, found in order so that a token is never
		/// found again inside one before it.
		/// </summary>
		static List<(SqlTokens.Token Token, int At)> Placed(string text)
		{
			var placed = new List<(SqlTokens.Token, int)>();
			var from   = 0;

			foreach (var token in SqlTokens.Of(text))
			{
				var at = text.IndexOf(token.Text, from, StringComparison.Ordinal);

				placed.Add((token, at));
				from = at + token.Text.Length;
			}

			return placed;
		}

		/// <summary>
		/// An <c>ORDER BY …</c> the old tree keeps as text, read by the old grammar as a query's own order, and
		/// carried as that order is.
		/// </summary>
		static Ast.OrderByClause Ordering(object owner, string text)
		{
			var read = DotGram.Sql.TransactSql.TransactSqlParser.TryParseSql("SELECT 1 " + text);

			if (read.IsSuccess && read.Value is [Statement.Select { OrderBy: Clause.OrderBy { Offset: null, Fetch: null } by }])
				return Queries.Order(by);

			throw BridgeGap.Open(owner, $"inventory Expression.WindowFunction (an order `{text}` that does not read)");
		}

		/// <summary>
		/// An <c>OVER</c>: a window's name alone, or a specification.
		/// </summary>
		static Ast.WindowReference Window(Clause over)
		{
			if (over is not Clause.Window window)
				throw BridgeGap.Open(over, "inventory Clause.Window (an OVER that is not a window)");

			// `OVER w` and `OVER (w)` are one old node; the text says which, and without it the name alone is taken.
			if (window is { Name: { } named, PartitionBy.Length: 0, By: null, Frame: null }
				&& !Recover("WindowReference.NameRef against (name)", Spelled(window), static text => text.TrimStart().StartsWith('('), false))
				return new Ast.WindowReference.NameRef(Part(over, named));

			return new Ast.WindowReference.Specification(Specification(window));
		}

		public static Ast.WindowSpecification Specification(Clause.Window window)
		{
			Ast.OrderByClause? order = null;

			if (window.By is Clause.OrderBy by)
			{
				if (by.Offset is not null || by.Fetch is not null)
					throw BridgeGap.Open(window, "inventory Clause.OrderBy (OFFSET in a window)");

				order = Queries.Order(by);
			}

			return new Ast.WindowSpecification(
				window.Name is { } name ? Part(window, name) : null,
				Each(window.PartitionBy, Of),
				order,
				window.Frame is { } frame ? Frame(window, frame) : null);
		}

		/// <summary>
		/// A window's frame, which the old tree keeps as text: <c>ROWS BETWEEN UNBOUNDED PRECEDING AND 2 FOLLOWING</c>.
		/// T-SQL's frame is <c>ROWS</c> or <c>RANGE</c>, one bound or two, and an offset that is a number.
		/// </summary>
		static Ast.WindowFrame Frame(Clause.Window window, string text)
		{
			var words = Words(text);
			var at    = 0;

			string? Next()
			{
				return at < words.Length ? words[at++] : null;
			}

			Ast.WindowFrameBound Bound()
			{
				var first = Next();

				switch (first, Next())
				{
					case ("UNBOUNDED", "PRECEDING"):
						return new Ast.WindowFrameBound(Ast.WindowBoundKind.UnboundedPreceding);

					case ("UNBOUNDED", "FOLLOWING"):
						return new Ast.WindowFrameBound(Ast.WindowBoundKind.UnboundedFollowing);

					case ("CURRENT", "ROW"):
						return new Ast.WindowFrameBound(Ast.WindowBoundKind.CurrentRow);

					case ({ } number, { } side) when side is "PRECEDING" or "FOLLOWING" && number.All(char.IsDigit):
						return new Ast.WindowFrameBound(
							side == "PRECEDING" ? Ast.WindowBoundKind.Preceding : Ast.WindowBoundKind.Following,
							Value(new Ast.LiteralValue.Numeric(number, Ast.NumericLiteralKind.DecimalInteger)));

					default:
						throw BridgeGap.Open(window, $"inventory Clause.Window (a frame `{text}`)");
				}
			}

			var unit = Next() switch
			{
				"ROWS"  => Ast.WindowFrameUnit.Rows,
				"RANGE" => Ast.WindowFrameUnit.Range,
				_       => throw BridgeGap.Open(window, $"inventory Clause.Window (a frame `{text}`)"),
			};

			Ast.WindowFrameExtent extent;

			if (at < words.Length && words[at] == "BETWEEN")
			{
				at++;

				var from = Bound();

				if (Next() != "AND")
					throw BridgeGap.Open(window, $"inventory Clause.Window (a frame `{text}`)");

				extent = new Ast.WindowFrameExtent.Between(from, Bound());
			}
			else
				extent = new Ast.WindowFrameExtent.Single(Bound());

			if (at != words.Length)
				throw BridgeGap.Open(window, $"inventory Clause.Window (a frame `{text}`)");

			return new Ast.WindowFrame(unit, extent, null, null);
		}

		// ── Names ─────────────────────────────────────────────────────────────────

		/// <summary>
		/// A name the old tree keeps as joined text, split into its parts: <c>[dbo].[t]</c>, <c>a..b</c> with an
		/// empty part, <c>"x"</c>, <c>#t</c>. A variable is not a name and is refused.
		/// </summary>
		public static Ast.QualifiedName Name(object owner, string text)
		{
			if (text.StartsWith('@'))
				throw BridgeGap.Missing(owner, "a variable where a name stands (an assignment, a table source, EXEC); the family's `Variable` member comes with those statements");

			return new Ast.QualifiedName([.. Split(text).Select(part => Part(owner, part, split: false))]);
		}

		/// <summary>A name's text cut at its dots, a dot inside a delimited part not among them.</summary>
		static List<string> Split(string text)
		{
			var parts = new List<string>();
			var at    = 0;

			while (true)
			{
				var start = at;

				while (at < text.Length && text[at] != '.')
				{
					var close = text[at] switch { '[' => ']', '"' => '"', _ => '\0' };

					if (close == '\0')
					{
						at++;

						continue;
					}

					// A delimiter doubled inside the name is the delimiter: `[a]]b]`, `"a""b"`.
					for (at++; at < text.Length; at++)
					{
						if (text[at] != close)
							continue;

						if (at + 1 < text.Length && text[at + 1] == close)
						{
							at++;

							continue;
						}

						break;
					}

					at++;
				}

				parts.Add(text.Substring(start, Math.Min(at, text.Length) - start));

				if (at >= text.Length)
					break;

				at++;
			}

			return parts;
		}

		/// <summary>
		/// One part of a name, with the style its first character says. The old tree keeps some names as text where
		/// one identifier stands; one of several parts, `t.b` or `[t].[b]`, is not one identifier, and so a gap.
		/// </summary>
		public static Ast.Identifier Part(object owner, string text)
		{
			return Part(owner, text, split: true);
		}

		static Ast.Identifier Part(object owner, string text, bool split)
		{
			if (split && text.Length > 0 && text[0] != '@' && Split(text).Count > 1)
				throw BridgeGap.Open(owner, $"inventory cross-cutting gaps, identifier styles (a qualified name `{text}` where one name stands)");

			if (text.StartsWith('@'))
				throw BridgeGap.Missing(owner, "a variable where a name stands (an assignment, a table source, EXEC); the family's `Variable` member comes with those statements");

			if (text.Any(char.IsWhiteSpace) && text.Length > 0 && text[0] is not '[' and not '"')
				throw BridgeGap.Open(owner, $"inventory cross-cutting gaps, identifier styles (a name part `{text}`)");

			return text.Length > 0 && text[0] == '[' ? new Ast.Identifier(text, Ast.IdentifierStyle.Bracketed)
				: text.Length > 0 && text[0] == '"' ? new Ast.Identifier(text, Ast.IdentifierStyle.Delimited)
				: new Ast.Identifier(text);
		}

		public static SqlList<Ast.Identifier> Parts(object owner, SqlList<string>? names)
		{
			return names is { } list ? Each(list, one => Part(owner, one)) : [];
		}

		// ── Data types ────────────────────────────────────────────────────────────

		/// <summary>
		/// T-SQL's system types, as of SQL Server 2025, and the one-word synonyms: a closed list, compared without
		/// case and without a name's delimiters.
		/// </summary>
		public static readonly HashSet<string> SystemTypes = new(StringComparer.OrdinalIgnoreCase)
		{
			"bigint", "binary", "bit", "char", "cursor", "date", "datetime", "datetime2", "datetimeoffset", "decimal",
			"float", "geography", "geometry", "hierarchyid", "image", "int", "json", "money", "nchar", "ntext",
			"numeric", "nvarchar", "real", "rowversion", "smalldatetime", "smallint", "smallmoney", "sql_variant",
			"sysname", "table", "text", "time", "timestamp", "tinyint", "uniqueidentifier", "varbinary", "varchar",
			"vector", "xml", "dec", "integer", "character",
		};

		/// <summary>
		/// The one-word spellings whose meaning is the standard type's, which T-SQL also reads as a name, bracketed,
		/// quoted or qualified by <c>sys</c>, for the same type.
		/// </summary>
		public static readonly HashSet<string> StandardWords = new(StringComparer.OrdinalIgnoreCase)
		{
			"int", "integer", "smallint", "bigint", "dec", "decimal", "numeric", "float", "real", "char", "character",
			"varchar", "nchar", "nvarchar", "binary", "varbinary", "date", "time", "json", "xml",
		};

		/// <summary>
		/// A data type the old tree keeps as text. On SQL Server a system type's name bracketed, quoted or qualified by
		/// <c>sys</c> is the same type as the bare word. In order: a one-word standard spelling, bare or so written, is
		/// the standard's record, with <c>WrittenName</c> unless it was bare (never <c>timestamp</c>, T-SQL's
		/// rowversion); a standard spelling of several words, unbracketed, is the standard's record; <c>xml</c>, bare
		/// or so written, is <see cref="Ast.DataType.Xml"/>, with <c>WrittenName</c> unless it was bare; a name, or <c>sys</c> and a name, that is one of T-SQL's system
		/// types is <see cref="Ast.DataType.Named"/>, its arguments numbers; a word or a bracketed <c>[max]</c> in the
		/// brackets is a gap; and every other name is the standard's <see cref="Ast.DataType.UserDefined"/>.
		/// </summary>
		public static Ast.DataType Type(object owner, string text)
		{
			var open  = text.IndexOf('(');
			var head  = (open < 0 ? text : text[..open]).Trim();
			var words = string.Join(' ', head.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
			var tail  = open < 0 ? "" : text[open..].Trim();

			if (tail.Length > 0 && (!tail.EndsWith(')') || tail.IndexOf(')') != tail.Length - 1))
				throw BridgeGap.Open(owner, $"inventory DataType (a type `{text}` whose brackets do not close it)");

			var inside    = tail.Length > 0 ? tail[1..^1].Trim() : "";
			var arguments = tail.Length > 0 ? inside.Split(',').Select(static one => one.Trim()).ToArray() : [];

			if (arguments.Any(static one => one.StartsWith('[') || one.StartsWith('"')))
				throw BridgeGap.Open(owner, $"inventory DataType (`{text}`: a name in the brackets of a type)");

			// Several words unbracketed: only the standard's spellings of several words.
			if (head.Length == 0 || Words(head).Length != 1 && !head.Contains('.'))
				return Standard(words, arguments) ?? throw BridgeGap.Open(owner, $"inventory DataType (a type `{text}` of several words)");

			var name   = Name(owner, head);
			var last   = Unquoted(name.Parts[^1].Text).TrimEnd();
			var system = name.Parts.Count == 1 || name.Parts.Count == 2 && Unquoted(name.Parts[0].Text).Equals("sys", StringComparison.OrdinalIgnoreCase);
			var bare   = name.Parts is [{ Style: Ast.IdentifierStyle.Regular }];

			if (system && StandardWords.Contains(last) && Standard(last.ToUpperInvariant(), arguments) is { } standard)
				return bare ? standard : standard with { WrittenName = name };

			// xml, however written, before the system types, so that `[xml]` is never Named.
			if (system && last.Equals("xml", StringComparison.OrdinalIgnoreCase))
				return bare ? Xml(owner, text, inside) : Xml(owner, text, inside) with { WrittenName = name };

			if (system && SystemTypes.Contains(last))
				return arguments.All(static one => one.Length > 0 && one.All(char.IsDigit))
					? new Ast.DataType.Named(name, [.. arguments.Select(static one => Value(new Ast.LiteralValue.Numeric(one, Ast.NumericLiteralKind.DecimalInteger)))])
					: throw BridgeGap.Open(owner, $"inventory DataType (`{text}`: a word in the brackets of a type)");

			return arguments.Length == 0
				? new Ast.DataType.UserDefined(name)
				: throw BridgeGap.Open(owner, $"inventory DataType (`{text}`: a user-defined type with arguments)");
		}

		/// <summary>A name part's text without its delimiters, a doubled closing one undoubled.</summary>
		public static string Unquoted(string text)
		{
			return text.Length >= 2 && text[0] == '[' && text[^1] == ']' ? text[1..^1].Replace("]]", "]", StringComparison.Ordinal)
				: text.Length >= 2 && text[0] == '"' && text[^1] == '"' ? text[1..^1].Replace("\"\"", "\"", StringComparison.Ordinal)
				: text;
		}

		/// <summary>
		/// The standard's type a T-SQL spelling is, or null where the standard has no such spelling or not with
		/// those arguments.
		/// </summary>
		static Ast.DataType? Standard(string words, string[] arguments)
		{
			var upper = arguments.Select(static one => one.ToUpperInvariant()).ToArray();
			var valid = true;

			int? Number(int at)
			{
				if (upper.Length <= at)
					return null;

				if (int.TryParse(upper[at], out var value))
					return value;

				valid = false;

				return null;
			}

			Ast.DataType? Checked(Ast.DataType type, int most)
			{
				return valid && upper.Length <= most ? type : null;
			}

			Ast.DataType? Numeric(Ast.NumericTypeKind kind, int most)
			{
				return Checked(new Ast.DataType.Numeric(kind, Number(0), Number(1)), most);
			}

			Ast.DataType? Character(Ast.CharacterTypeKind kind)
			{
				return upper is ["MAX"] ? new Ast.DataType.Character(kind, Max: true) : Checked(new Ast.DataType.Character(kind, Number(0)), 1);
			}

			Ast.DataType? Binary(Ast.BinaryTypeKind kind)
			{
				return upper is ["MAX"] ? new Ast.DataType.Binary(kind, Max: true) : Checked(new Ast.DataType.Binary(kind, Number(0)), 1);
			}

			return words switch
			{
				"INT"                        => Numeric(Ast.NumericTypeKind.Int, 0),
				"INTEGER"                    => Numeric(Ast.NumericTypeKind.Integer, 0),
				"SMALLINT"                   => Numeric(Ast.NumericTypeKind.SmallInt, 0),
				"BIGINT"                     => Numeric(Ast.NumericTypeKind.BigInt, 0),
				"NUMERIC"                    => Numeric(Ast.NumericTypeKind.Numeric, 2),
				"DECIMAL"                    => Numeric(Ast.NumericTypeKind.Decimal, 2),
				"DEC"                        => Numeric(Ast.NumericTypeKind.Dec, 2),
				"FLOAT"                      => Numeric(Ast.NumericTypeKind.Float, 1),
				"REAL"                       => Numeric(Ast.NumericTypeKind.Real, 0),
				"DOUBLE PRECISION"           => Numeric(Ast.NumericTypeKind.DoublePrecision, 0),
				"CHARACTER"                  => Character(Ast.CharacterTypeKind.Character),
				"CHAR"                       => Character(Ast.CharacterTypeKind.Char),
				"CHARACTER VARYING"          => Character(Ast.CharacterTypeKind.CharacterVarying),
				"CHAR VARYING"               => Character(Ast.CharacterTypeKind.CharVarying),
				"NVARCHAR"                   => Character(Ast.CharacterTypeKind.Nvarchar),
				"VARCHAR"                    => Character(Ast.CharacterTypeKind.Varchar),
				"NATIONAL CHARACTER"         => Character(Ast.CharacterTypeKind.NationalCharacter),
				"NATIONAL CHAR"              => Character(Ast.CharacterTypeKind.NationalChar),
				"NCHAR"                      => Character(Ast.CharacterTypeKind.Nchar),
				"NATIONAL CHARACTER VARYING" => Character(Ast.CharacterTypeKind.NationalCharacterVarying),
				"NATIONAL CHAR VARYING"      => Character(Ast.CharacterTypeKind.NationalCharVarying),
				"NCHAR VARYING"              => Character(Ast.CharacterTypeKind.NcharVarying),
				"BINARY"                     => Binary(Ast.BinaryTypeKind.Binary),
				"BINARY VARYING"             => Binary(Ast.BinaryTypeKind.BinaryVarying),
				"VARBINARY"                  => Binary(Ast.BinaryTypeKind.Varbinary),
				"DATE"                       => Checked(new Ast.DataType.DateTime(Ast.DateTimeTypeKind.Date), 0),
				"TIME"                       => Checked(new Ast.DataType.DateTime(Ast.DateTimeTypeKind.Time, Number(0)), 1),
				"JSON"                       => Checked(new Ast.DataType.Json(), 0),
				_                            => null,
			};
		}

		/// <summary>
		/// <c>xml</c>, and what its brackets say: <c>CONTENT</c> or <c>DOCUMENT</c>, and the schema collection.
		/// </summary>
		static Ast.DataType Xml(object owner, string text, string inside)
		{
			if (inside.Length == 0)
				return new Ast.DataType.Xml();

			var first = inside.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);

			Ast.XmlTypeModifier? content = first[0].ToUpperInvariant() switch
			{
				"CONTENT"  => Ast.XmlTypeModifier.Content,
				"DOCUMENT" => Ast.XmlTypeModifier.Document,
				_          => null,
			};

			var schema = content is null ? inside : first.Length > 1 ? first[1].Trim() : "";

			if (schema.Length == 0 || schema.Any(char.IsWhiteSpace))
				throw BridgeGap.Open(owner, $"inventory DataType (an xml type `{text}`)");

			return new Ast.DataType.Xml(content, Name(owner, schema));
		}

		/// <summary>
		/// A data type's text as the comparison of old trees reads it: no spaces, upper case.
		/// </summary>
		public static string Squeezed(string text)
		{
			var squeezed = new StringBuilder(text.Length);

			foreach (var c in text)
				if (!char.IsWhiteSpace(c))
					squeezed.Append(char.ToUpperInvariant(c));

			return squeezed.ToString();
		}
	}
}
