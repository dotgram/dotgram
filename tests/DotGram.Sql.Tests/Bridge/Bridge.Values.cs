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

				Expression.GraphMatch      => throw BridgeGap.Missing(old, "Expression.GraphMatch, a graph pattern family (proposal 15)"),
				Expression.RowsetOrder     => throw BridgeGap.Missing(old, "a typed T-SQL rowset table source (proposal 7)"),
				Expression.Prefixed        => throw BridgeGap.Missing(old, "a typed T-SQL rowset table source (proposal 7)"),
				Expression.Pieced          => throw BridgeGap.Missing(old, "a typed T-SQL rowset table source (proposal 7)"),
				Expression.Measured        => throw BridgeGap.Unwritten(old, "Expression.Measured"),
				Expression.OdbcEscape      => throw BridgeGap.Unwritten(old, "Expression.OdbcEscape"),
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
					throw BridgeGap.Missing(old, "Expression.Variable for `@x` and `@@x` (decided 2026-09-15, not declared)");

				case SqlLiteralKind.Parameter when text == "?":
					return new Ast.Expression.Parameter(Ast.ParameterKind.Dynamic);

				case SqlLiteralKind.Hex:
					throw BridgeGap.Missing(old, "LiteralValue.Binary has no flag for T-SQL's `0x` spelling (proposal 11)");

				case SqlLiteralKind.Bit:
					throw BridgeGap.Missing(old, "a bit string literal, which SQL:2023 removed");

				default:
					throw BridgeGap.Open(old, $"inventory SqlLiteralKind ({old.Kind})");
			}
		}

		static Ast.Expression Value(Ast.LiteralValue value)
		{
			return new Ast.Expression.Literal(value);
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
		/// The built-ins whose arguments are not all values, which proposal 12 gives typed nodes not yet declared.
		/// </summary>
		static readonly HashSet<string> Typed = new(StringComparer.OrdinalIgnoreCase)
		{
			"CONVERT", "TRY_CONVERT", "TRY_CAST", "PARSE", "TRY_PARSE",
		};

		/// <summary>
		/// The JSON functions the standard has nodes of its own for, which T-SQL's old tree reads as calls.
		/// </summary>
		static readonly HashSet<string> Json = new(StringComparer.OrdinalIgnoreCase)
		{
			"JSON_OBJECT", "JSON_ARRAY", "JSON_VALUE", "JSON_QUERY", "JSON_EXISTS", "JSON_OBJECTAGG", "JSON_ARRAYAGG",
			"JSON_SCALAR", "JSON_SERIALIZE", "JSON_TABLE",
		};

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
			var name = old.Name;

			if (name.Equals("CAST", StringComparison.OrdinalIgnoreCase))
			{
				if (old.Arguments.Length != 1 || old.Word is null)
					throw BridgeGap.Open(old, "inventory Expression.RoutineInvocation (a CAST that is not one value and a type)");

				return new Ast.Expression.Cast(Of(old.Arguments[0]), Type(old, old.Word));
			}

			if (name.Equals("AT TIME ZONE", StringComparison.OrdinalIgnoreCase))
			{
				if (old.Arguments.Length != 2)
					throw BridgeGap.Open(old, "inventory Expression.RoutineInvocation (AT TIME ZONE without its two operands)");

				return new Ast.Expression.AtTimeZone(Of(old.Arguments[0]), Of(old.Arguments[1]));
			}

			if (name.Equals("TRIM", StringComparison.OrdinalIgnoreCase))
				return Trim(old);

			if (Json.Contains(name))
				throw BridgeGap.Open(old, "inventory Expression.JsonPair (T-SQL's JSON functions as the standard's JSON nodes)");

			if (Typed.Contains(name))
				throw BridgeGap.Missing(old, $"a typed node for {name.ToUpperInvariant()} (proposal 12)");

			if (name.StartsWith("NEXT VALUE FOR", StringComparison.OrdinalIgnoreCase))
				throw BridgeGap.Missing(old, "NextValue with a window (proposal 12)");

			if (Syntax.IsDatePartFunction(name.ToUpperInvariant()))
				throw BridgeGap.Missing(old, "a date part argument (proposal 12)");

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
			// The words between the call and its window are text in the old tree: a null treatment is two words,
			// and WITHIN GROUP holds an order the text would have to be read again for.
			Ast.NullTreatment? nulls = old.Within is null ? null : Words(old.Within) switch
			{
				["IGNORE", "NULLS"]  => Ast.NullTreatment.IgnoreNulls,
				["RESPECT", "NULLS"] => Ast.NullTreatment.RespectNulls,
				_                    => throw BridgeGap.Open(old, "inventory Expression.WindowFunction (WITHIN GROUP kept as text)"),
			};

			if (old.Function is not Expression.RoutineInvocation call || Function(call) is not Ast.Expression.Invocation invocation)
				throw BridgeGap.Open(old, "inventory Expression.WindowFunction (a window over something that is not a call)");

			invocation = invocation with { Nulls = nulls };

			return old.Over is null ? invocation : invocation with { Over = Window(old.Over) };
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
			if (window.Frame is not null)
				throw BridgeGap.Open(window, "inventory Clause.Window (the frame kept as text)");

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
				null);
		}

		// ── Names ─────────────────────────────────────────────────────────────────

		/// <summary>
		/// A name the old tree keeps as joined text, split into its parts: <c>[dbo].[t]</c>, <c>a..b</c> with an
		/// empty part, <c>"x"</c>, <c>#t</c>. A variable is not a name and is refused.
		/// </summary>
		public static Ast.QualifiedName Name(object owner, string text)
		{
			if (text.StartsWith('@'))
				throw BridgeGap.Missing(owner, "Expression.Variable for `@x` and `@@x` (decided 2026-09-15, not declared)");

			var parts = new List<Ast.Identifier>();
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

				parts.Add(Part(owner, text.Substring(start, Math.Min(at, text.Length) - start)));

				if (at >= text.Length)
					break;

				at++;
			}

			return new Ast.QualifiedName([.. parts]);
		}

		/// <summary>
		/// One part of a name, with the style its first character says.
		/// </summary>
		public static Ast.Identifier Part(object owner, string text)
		{
			if (text.StartsWith('@'))
				throw BridgeGap.Missing(owner, "Expression.Variable for `@x` and `@@x` (decided 2026-09-15, not declared)");

			if (text == "*")
				throw BridgeGap.Open(owner, "inventory Expression.ColumnReference (`t.*` standing where a value does)");

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
		/// A data type the old tree keeps as text, where it is a spelling the new <see cref="Ast.DataType"/>
		/// already has. The rest — <c>tinyint</c>, <c>datetime2</c>, <c>nvarchar</c>, <c>[int]</c>,
		/// T-SQL's <c>timestamp</c> — is <c>DataType.Named</c> (proposal 14), which is not declared.
		/// </summary>
		public static Ast.DataType Type(object owner, string text)
		{
			var open  = text.IndexOf('(');
			var words = string.Join(' ', (open < 0 ? text : text[..open]).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
			var tail  = open < 0 ? "" : text[open..].Trim();

			string[] arguments = [];

			if (tail.Length > 0)
			{
				if (!tail.EndsWith(')') || tail.IndexOf(')') != tail.Length - 1)
					throw BridgeGap.Missing(owner, $"DataType.Named for `{text}` (proposal 14)");

				arguments = [.. tail[1..^1].Split(',').Select(static one => one.Trim().ToUpperInvariant())];
			}

			int? Number(int at)
			{
				if (arguments.Length <= at)
					return null;

				return int.TryParse(arguments[at], out var value) ? value : throw BridgeGap.Missing(owner, $"DataType.Named for `{text}` (proposal 14)");
			}

			void Arity(int most)
			{
				if (arguments.Length > most)
					throw BridgeGap.Missing(owner, $"DataType.Named for `{text}` (proposal 14)");
			}

			Ast.DataType Numeric(Ast.NumericTypeKind kind, int most)
			{
				Arity(most);

				return new Ast.DataType.Numeric(kind, Number(0), Number(1));
			}

			Ast.DataType Character(Ast.CharacterTypeKind kind)
			{
				Arity(1);

				return arguments is ["MAX"] ? new Ast.DataType.Character(kind, Max: true) : new Ast.DataType.Character(kind, Number(0));
			}

			Ast.DataType Binary(Ast.BinaryTypeKind kind)
			{
				Arity(1);

				return arguments is ["MAX"] ? new Ast.DataType.Binary(kind, Max: true) : new Ast.DataType.Binary(kind, Number(0));
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
				"DATE" when arguments.Length == 0 => new Ast.DataType.DateTime(Ast.DateTimeTypeKind.Date),
				"TIME" when arguments.Length <= 1 => new Ast.DataType.DateTime(Ast.DateTimeTypeKind.Time, Number(0)),
				_                            => throw BridgeGap.Missing(owner, $"DataType.Named for `{text}` (proposal 14)"),
			};
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
