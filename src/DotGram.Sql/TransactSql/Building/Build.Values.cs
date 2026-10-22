using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Sql.TransactSql;

static partial class Build
{
	/// <summary>
	/// Names, literals, variables, operators, data types and functions: the values the rules build on the common tree in
	/// their actions, and the strings names and types are read as until a builder takes them apart.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A rule that reads a value builds its node at once, with the node its parts already are. Where the common tree has
	/// no place for what was read, the node is <see cref="Unbuilt"/>, which says why; a value built over one is that
	/// one, and a builder that takes one (<see cref="Of"/>) throws its gap, which the builder's statement holds, as a
	/// statement built from a part with no place always did.
	/// </para>
	/// <para>
	/// A few shapes a builder takes apart and nothing else holds — a word standing for a name, a call whose arguments say
	/// more than values, a key and its value — stand as a <see cref="Held"/> until the builder that knows what they mean
	/// meets them; anywhere else <see cref="Of"/> makes of one what it is as a value.
	/// </para>
	/// </remarks>
	internal static class Values
	{
		/// <summary>What a call with no values is handed, once rather than per call.</summary>
		public static readonly SqlList<Ast.Expression> None = [];

		/// <summary>
		/// A value as a node of the common tree: itself, or what a held shape is as a value, told where the shape was
		/// written; a value with no place throws its gap.
		/// </summary>
		public static Ast.Expression Of(Ast.Expression value)
		{
			if (value is not Held held)
				return value;

			// A held shape may hold others, a call's arguments its own calls: deep, the rest goes on with a stack of its own.
			if (!SqlStack.Enough())
				return Deeper(value, static node => Of((Ast.Expression)node));

			var made = held.Resolved();

			if (locating && held.Read is { Known: true } span)
				made.Locate(span.At, span.Length);

			return made;
		}

		/// <summary>
		/// What an entry rule hands back: a held value resolved, so that a reading of an internal entry hands back the
		/// common tree's node, and one with no place the extension the public entry makes of it (<see cref="Carried(Ast.Expression, bool)"/>).
		/// </summary>
		public static Ast.Expression Entered(Ast.Expression value)
		{
			return Carried(value, locating);
		}

		/// <summary>A value built at once, or the gap that says why it could not be: the value of every builder below.</summary>
		static Ast.Expression Gapped(BuildGap gap)
		{
			return new Unbuilt(gap);
		}

		/// <summary>
		/// The same, where the rules ask of the value what it was read as (<see cref="Unbuilt.Shape"/>), whether it held a query,
		/// and what it was read over.
		/// </summary>
		static Ast.Expression Gapped(BuildGap gap, string? shape, bool query, SqlList<Ast.Expression> parts)
		{
			return new Unbuilt(gap) { Shape = shape, HoldsQuery = query, Parts = parts };
		}

		/// <summary>The values a CASE was read over.</summary>
		static SqlList<Ast.Expression> Parts(Ast.Expression? operand, SqlList<Clause> whens, Ast.Expression? otherwise)
		{
			var parts = new List<Ast.Expression>();

			if (operand is not null)
				parts.Add(operand);

			foreach (var one in whens)
				if (one is Clause.When(var test, var result))
				{
					parts.Add(test);
					parts.Add(result);
				}

			if (otherwise is not null)
				parts.Add(otherwise);

			return [.. parts];
		}

		/// <summary>The values a primary and what was reached through it were read over.</summary>
		static SqlList<Ast.Expression> Parts(Ast.Expression primary, Reach[]? members, Zoning[]? zones)
		{
			var parts = new List<Ast.Expression> { primary };

			foreach (var member in members ?? [])
				parts.AddRange(member.Arguments ?? []);

			foreach (var zone in zones ?? [])
				parts.Add(zone.Zone);

			return [.. parts];
		}

		// ── What the rules ask of a value ─────────────────────────────────────────

		/// <summary>What a value in brackets holds, or null where it is not one.</summary>
		public static Ast.Expression? Bracketed(Ast.Expression? value)
		{
			return value switch
			{
				Ast.Expression.Parenthesized(var inner)               => inner,
				Enclosed(var inner)                                   => inner,
				Unbuilt { Shape: "Expression.Parenthesized", Parts: [var inner] } => inner,
				_                                                     => null,
			};
		}

		/// <summary>
		/// The text of a value written as a column or a word, as written: <c>t.*</c>, <c>*</c>, a name in its parts; null
		/// where it is no such thing. What a builder that reads a word an option is set to reads.
		/// </summary>
		public static string? Wording(Ast.Expression? value)
		{
			return value switch
			{
				Word(var text)                                                  => text,
				Ast.Expression.Asterisk                                         => "*",
				Ast.Expression.Reference(var name)                              => Joined(name),
				Ast.Expression.Wildcard(Ast.Expression.Reference(var name), Ast.WildcardKind.Member) => Joined(name) + ".*",
				_                                                               => null,
			};

			static string Joined(Ast.QualifiedName name)
			{
				if (name.Count == 1)
					return name[0].Text;

				var parts = new string[name.Count];

				for (var at = 0; at < parts.Length; at++)
					parts[at] = name[at].Text;

				return string.Join(".", parts);
			}
		}

		/// <summary>
		/// Whether a value is what the reference calls a literal where a value stands: a number, a string, a binary string,
		/// <c>NULL</c>, <c>DEFAULT</c>, a variable or a parameter, or one of the words that stand for a value.
		/// </summary>
		public static bool IsLiteral(Ast.Expression? value)
		{
			return value is Ast.LiteralValue or Ast.Expression.Default or Ast.Expression.Variable or Ast.Expression.Parameter
				or Ast.Expression.Current or Literal;
		}

		/// <summary>Whether a value is a variable or a parameter, <c>@x</c>, <c>?</c>, <c>:x</c>.</summary>
		public static bool IsParameter(Ast.Expression? value)
		{
			return value is Ast.Expression.Variable or Ast.Expression.Parameter or Literal { Kind: "Parameter" };
		}

		/// <summary>A literal's text as written, quotes and all, or null where the value is no literal with a text.</summary>
		public static string? LiteralText(Ast.Expression? value)
		{
			return value switch
			{
				Ast.LiteralValue.String { Kind: Ast.StringLiteralKind.National } national => "N" + national.Text,
				Ast.LiteralValue.String text      => text.Text,
				Ast.LiteralValue.Numeric number   => number.Text,
				Ast.LiteralValue.Binary binary    => "0x" + binary.Text,
				Ast.LiteralValue.Null             => "NULL",
				Ast.Expression.Default            => "DEFAULT",
				Ast.Expression.Variable(var name) => name.Text,
				Ast.Expression.Parameter          => "?",
				Literal(_, var text)              => text,
				_                                 => null,
			};
		}

		// ── The operators ─────────────────────────────────────────────────────────

		/// <summary>
		/// A pair of brackets around a value. Around a subquery, which has brackets of its own, the value stays a bracketed
		/// subquery for the rules that ask whether a value was written in brackets (<see cref="Bracketed"/>), and is the
		/// subquery of a bracketed query on the common tree (<see cref="Enclosed"/>).
		/// </summary>
		public static Ast.Expression Parenthesized(Ast.Expression value)
		{
			try
			{
				var inside = Of(value);

				return inside is Ast.Expression.Subquery subquery
					? new Enclosed(subquery)
					: new Ast.Expression.Parenthesized(inside);
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, "Expression.Parenthesized", false, [value]);
			}
		}

		public static Ast.Expression Binary(Ast.Expression left, Ast.BinaryOperator op, Ast.Expression right)
		{
			try
			{
				return new Ast.Expression.Binary(Of(left), op, Of(right));
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, null, false, [left, right]);
			}
		}

		/// <summary>An additive operator and its two operands: T-SQL's bitwise three and its two shifts among them, which bind as weakly.</summary>
		public static Ast.Expression Additive(string operatorText, Ast.Expression left, Ast.Expression right)
		{
			return Binary(left, operatorText switch
			{
				"+"  => Ast.BinaryOperator.Add,
				"-"  => Ast.BinaryOperator.Subtract,
				"&"  => Ast.BinaryOperator.BitwiseAnd,
				"|"  => Ast.BinaryOperator.BitwiseOr,
				"^"  => Ast.BinaryOperator.BitwiseXor,
				"<<" => Ast.BinaryOperator.ShiftLeft,
				">>" => Ast.BinaryOperator.ShiftRight,
				_    => Ast.BinaryOperator.Concatenate,
			}, right);
		}

		/// <summary>Likewise for the multiplicative operators, T-SQL's <c>%</c> among them.</summary>
		public static Ast.Expression Multiplicative(string operatorText, Ast.Expression left, Ast.Expression right)
		{
			return Binary(left, operatorText switch
			{
				"*" => Ast.BinaryOperator.Multiply,
				"/" => Ast.BinaryOperator.Divide,
				_   => Ast.BinaryOperator.Modulo,
			}, right);
		}

		/// <summary>Likewise for the sign in front of one operand, T-SQL's <c>~</c> among them, and <c>NOT</c>.</summary>
		public static Ast.Expression Signed(string sign, Ast.Expression operand)
		{
			return Unary(sign switch
			{
				"-" => Ast.UnaryOperator.Minus,
				"~" => Ast.UnaryOperator.BitwiseNot,
				_   => Ast.UnaryOperator.Plus,
			}, operand);
		}

		public static Ast.Expression Unary(Ast.UnaryOperator op, Ast.Expression operand)
		{
			try
			{
				return new Ast.Expression.Unary(op, Of(operand));
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, null, false, [operand]);
			}
		}

		public static Ast.Expression IsTruth(Ast.Expression operand, bool not, string truth)
		{
			try
			{
				return new Ast.Expression.IsTruth(Of(operand), not, (truth[0] | 0x20) switch
				{
					't' => Ast.BooleanLiteral.True,
					'f' => Ast.BooleanLiteral.False,
					_   => Ast.BooleanLiteral.Unknown,
				});
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, null, false, [operand]);
			}
		}

		/// <summary>A value and the collation it is compared under.</summary>
		public static Ast.Expression Collated(Ast.Expression value, string collation)
		{
			try
			{
				return new Ast.Expression.Collate(Of(value), new Ast.CollationName(Name("Expression.Collated", collation)));
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, null, false, [value]);
			}
		}

		// ── The predicates ────────────────────────────────────────────────────────

		/// <summary>
		/// What may follow a row on the left of a predicate, built without the row and closed over it once the row is
		/// read (<see cref="Predicated"/>): the row is read once for every predicate that begins with one.
		/// </summary>
		internal abstract class Tail
		{
			/// <summary>The predicate over its left side, the row it was read after.</summary>
			public abstract Ast.Expression Over(Ast.Expression left);

			/// <summary>The values the tail was read over.</summary>
			public abstract SqlList<Ast.Expression> Parts { get; }

			/// <summary>Whether the tail holds a query of its own.</summary>
			public virtual bool Query => false;

			/// <summary>The same tail with <c>NOT</c> read in front of it, where it is one a <c>NOT</c> may stand in front of.</summary>
			public virtual Tail Negated()
			{
				return this;
			}
		}

		/// <summary><c>= right</c>, and the other comparisons, <c>!=</c> spelled as written.</summary>
		sealed class Compared(Ast.ComparisonOperator op, bool bang, Ast.Expression right) : Tail
		{
			public override SqlList<Ast.Expression> Parts => [right];

			public override Ast.Expression Over(Ast.Expression left)
			{
				return new Ast.Expression.Comparison(Of(left), op, Of(right), bang);
			}
		}

		/// <summary><c>= ALL (…)</c>: a comparison with every row of a subquery.</summary>
		sealed class QuantifiedTail(Ast.ComparisonOperator op, bool bang, string quantifier, Query query) : Tail
		{
			public override SqlList<Ast.Expression> Parts => [];

			public override bool Query => true;

			public override Ast.Expression Over(Ast.Expression left)
			{
				if (bang)
					throw BuildGap.Missing("Expression.Quantified", "QuantifiedComparison has no spelling flag for `!=`");

				var how = quantifier.ToUpperInvariant() switch
				{
					"ALL"  => Ast.Quantifier.All,
					"SOME" => Ast.Quantifier.Some,
					"ANY"  => Ast.Quantifier.Any,
					_      => throw BuildGap.Open("Expression.Quantified", $"inventory Expression.Quantified (quantifier `{quantifier}`)"),
				};

				return new Ast.Expression.QuantifiedComparison(Of(left), op, how, QueryOf(query));
			}
		}

		sealed class BetweenTail(bool not, Ast.Expression low, Ast.Expression high) : Tail
		{
			public override SqlList<Ast.Expression> Parts => [low, high];

			public override Ast.Expression Over(Ast.Expression left)
			{
				return new Ast.Expression.Between(Of(left), not, null, Of(low), Of(high));
			}

			public override Tail Negated()
			{
				return new BetweenTail(true, low, high);
			}
		}

		sealed class InTail(bool not, Ast.Expression source) : Tail
		{
			public override SqlList<Ast.Expression> Parts => [source];

			public override Ast.Expression Over(Ast.Expression left)
			{
				return new Ast.Expression.In(Of(left), not, Of(source) switch
				{
					Ast.Expression.Row(var items, _)    => new Ast.InSource.Values(items),
					Ast.Expression.Subquery(var query) => new Ast.InSource.Subquery(query),
					var other                           => throw BuildGap.Open("Expression.In", $"inventory Expression.In (a source that is {other.GetType().Name})"),
				});
			}

			public override Tail Negated()
			{
				return new InTail(true, source);
			}
		}

		sealed class LikeTail(bool not, Ast.Expression pattern, Escape? escape) : Tail
		{
			public override SqlList<Ast.Expression> Parts => escape is null ? [pattern] : [pattern, escape.Value];

			public override Ast.Expression Over(Ast.Expression left)
			{
				// T-SQL's `{ESCAPE '%'}`: the escape value itself, written in ODBC's braces.
				if (escape is { Braced: true } braced)
					return braced.Kind.Equals("ESCAPE", StringComparison.OrdinalIgnoreCase)
						? new Ast.Expression.Like(Of(left), not, Ast.LikeKind.Like, Of(pattern), Of(braced.Value)) { BracedEscape = true }
						: throw BuildGap.Open("Expression.Like", $"inventory Expression.Like (an escape in braces of kind `{braced.Kind}`)");

				return new Ast.Expression.Like(Of(left), not, Ast.LikeKind.Like, Of(pattern), escape is { } plain ? Of(plain.Value) : null);
			}

			public override Tail Negated()
			{
				return new LikeTail(true, pattern, escape);
			}
		}

		sealed class IsNullTail(bool not) : Tail
		{
			public override SqlList<Ast.Expression> Parts => [];

			public override Ast.Expression Over(Ast.Expression left)
			{
				return new Ast.Expression.IsNull(Of(left), not);
			}
		}

		sealed class MatchTail(string? qualifier, Query query) : Tail
		{
			public override SqlList<Ast.Expression> Parts => [];

			public override bool Query => true;

			public override Ast.Expression Over(Ast.Expression left)
			{
				var words  = qualifier is null ? [] : Words(qualifier);
				var unique = words.Contains("UNIQUE");
				var rest   = words.Where(static one => one != "UNIQUE").ToArray();

				Ast.MatchType? type = rest switch
				{
					[]          => null,
					["FULL"]    => Ast.MatchType.Full,
					["PARTIAL"] => Ast.MatchType.Partial,
					["SIMPLE"]  => Ast.MatchType.Simple,
					_           => throw BuildGap.Open("Expression.Match", $"inventory Expression.Match (qualifier `{qualifier}`)"),
				};

				return new Ast.Expression.Match(Of(left), unique, type, QueryOf(query));
			}
		}

		sealed class OverlapsTail(Ast.Expression right) : Tail
		{
			public override SqlList<Ast.Expression> Parts => [right];

			public override Ast.Expression Over(Ast.Expression left)
			{
				return new Ast.Expression.Overlaps(Of(left), Of(right));
			}
		}

		/// <summary>An escape: its value, and whether it was written in ODBC's braces, with the word inside them.</summary>
		internal sealed record Escape(Ast.Expression Value, bool Braced, string Kind);

		/// <summary>A comparison's operator, and whether it was <c>!=</c>, which the tree keeps beside <c>&lt;&gt;</c>.</summary>
		static (Ast.ComparisonOperator Operator, bool Bang) Compare(string operatorText)
		{
			return Syntax.Compacted(operatorText) switch
			{
				"="  => (Ast.ComparisonOperator.Equal, false),
				"<>" => (Ast.ComparisonOperator.NotEqual, false),
				"!=" => (Ast.ComparisonOperator.NotEqual, true),
				"<"  => (Ast.ComparisonOperator.Less, false),
				"<=" => (Ast.ComparisonOperator.LessOrEqual, false),
				"!<" => (Ast.ComparisonOperator.NotLess, false),
				">"  => (Ast.ComparisonOperator.Greater, false),
				"!>" => (Ast.ComparisonOperator.NotGreater, false),
				_    => (Ast.ComparisonOperator.GreaterOrEqual, false),
			};
		}

		public static Tail Comparison(string operatorText, Ast.Expression right)
		{
			var (op, bang) = Compare(operatorText);

			return new Compared(op, bang, right);
		}

		public static Tail Quantified(string operatorText, string quantifier, Query query)
		{
			var (op, bang) = Compare(operatorText);

			return new QuantifiedTail(op, bang, Syntax.Quantified(quantifier), query);
		}

		public static Tail Between(Ast.Expression low, Ast.Expression high)
		{
			return new BetweenTail(false, low, high);
		}

		public static Tail In(Ast.Expression source)
		{
			return new InTail(false, source);
		}

		public static Tail Like(Ast.Expression pattern, Escape? escape)
		{
			return new LikeTail(false, pattern, escape);
		}

		public static Tail IsNull(bool not)
		{
			return new IsNullTail(not);
		}

		public static Tail Match(string? unique, string? kind, Query query)
		{
			return new MatchTail(Syntax.Matched(unique, kind), query);
		}

		public static Tail Overlaps(Ast.Expression right)
		{
			return new OverlapsTail(right);
		}

		/// <summary>A tail with the <c>NOT</c> read in front of it, where one was.</summary>
		public static Tail Negated(Tail tail, bool not)
		{
			return not ? tail.Negated() : tail;
		}

		/// <summary>The predicate a row and the tail read after it are.</summary>
		public static Ast.Expression Predicated(Ast.Expression left, Tail tail)
		{
			try
			{
				return tail.Over(left);
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, null, tail.Query, [left, .. tail.Parts]);
			}
		}

		public static Ast.Expression Exists(Query query)
		{
			try
			{
				return new Ast.Expression.Exists(QueryOf(query));
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, "Expression.Exists", true, []);
			}
		}

		public static Ast.Expression Unique(Query query)
		{
			try
			{
				return new Ast.Expression.Unique(null, QueryOf(query));
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, "Expression.Unique", true, []);
			}
		}

		public static Ast.Expression Subquery(Query query)
		{
			try
			{
				return new Ast.Expression.Subquery(QueryOf(query));
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, "Expression.Subquery", true, []);
			}
		}

		/// <summary><c>IS [NOT] DISTINCT FROM</c>: a row, or a quantifier and a subquery (<c>DistinctTail</c>).</summary>
		public static Ast.Expression IsDistinct(Ast.Expression left, bool not, Ast.Expression right)
		{
			try
			{
				// DistinctTail's quantifier before a subquery, which the old tree keeps as a word in front of it.
				if (right is Prefixed(var word, Ast.Expression.Subquery or Unbuilt) prefixed && word.ToUpperInvariant() is "ALL" or "ANY" or "SOME")
					return new Ast.Expression.IsDistinct(Of(left), not, Of(prefixed.Value))
					{
						Quantifier = word.ToUpperInvariant() switch { "ALL" => Ast.Quantifier.All, "ANY" => Ast.Quantifier.Any, _ => Ast.Quantifier.Some },
					};

				return new Ast.Expression.IsDistinct(Of(left), not, Of(right));
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, null, false, [left, right]);
			}
		}

		/// <summary>T-SQL's graph <c>MATCH (…)</c>, its drawing read token by token.</summary>
		public static Ast.Expression GraphMatch(string pattern)
		{
			try
			{
				return Queries.GraphMatch("Expression.GraphMatch", pattern);
			}
			catch (BuildGap gap)
			{
				return Gapped(gap);
			}
		}

		public static Ast.Expression And(Ast.Expression left, Ast.Expression right)
		{
			return Binary(left, Ast.BinaryOperator.And, right);
		}

		public static Ast.Expression Or(Ast.Expression left, Ast.Expression right)
		{
			return Binary(left, Ast.BinaryOperator.Or, right);
		}

		public static Ast.Expression Not(Ast.Expression operand)
		{
			return Unary(Ast.UnaryOperator.Not, operand);
		}

		// ── Rows, cases, columns ──────────────────────────────────────────────────

		public static Ast.Expression Row(SqlList<Ast.Expression> items)
		{
			try
			{
				return new Ast.Expression.Row(Each(items, Of));
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, null, false, items);
			}
		}

		public static Ast.Expression Case(Ast.Expression? operand, SqlList<Clause> whens, Ast.Expression? otherwise)
		{
			try
			{
				return new Ast.Expression.Case(operand is null ? null : Of(operand), Each(whens, When), otherwise is null ? null : Of(otherwise));
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, null, false, Parts(operand, whens, otherwise));
			}
		}

		static Ast.CaseWhen When(Clause clause)
		{
			return clause is Clause.When(var test, var result)
				? new Ast.CaseWhen([Of(test)], Of(result))
				: throw BuildGap.Open(clause, "inventory Clause.When (a CASE arm that is not a WHEN)");
		}

		/// <summary>
		/// A column, as written, dots and all: <c>t.*</c>, all columns of <c>t</c>; a bare <c>*</c>, every column; any other a
		/// reference to the name. Where the name has no place the word is held, and taking it throws why.
		/// </summary>
		public static Ast.Expression Reference(string text)
		{
			try
			{
				return Word.Of(text);
			}
			catch (BuildGap)
			{
				return new Word(text);
			}
		}

		/// <summary>A word standing for a name, held for a builder that asks what it says (<see cref="Word"/>).</summary>
		public static Ast.Expression Worded(string text)
		{
			return new Word(text);
		}

		public static Ast.Expression Null()
		{
			return new Ast.LiteralValue.Null();
		}

		public static Ast.Expression Default()
		{
			return new Ast.Expression.Default();
		}

		public static Ast.Argument Argument(Ast.Expression value)
		{
			return new Ast.Argument(Of(value));
		}

		// ── Literals ──────────────────────────────────────────────────────────────

		static Ast.Expression Value(Ast.LiteralValue value)
		{
			return value;
		}

		public static Ast.Expression Number(string text)
		{
			return new Ast.LiteralValue.Numeric(text, Numeric(text));
		}

		public static Ast.Expression Text(string text)
		{
			return new Ast.LiteralValue.String(text, Ast.StringLiteralKind.Character);
		}

		public static Ast.Expression National(string text)
		{
			return new Ast.LiteralValue.String(text.Substring(1), Ast.StringLiteralKind.National);
		}

		/// <summary>A binary string, the digits after the introducer, as a string's text leaves its introducer out.</summary>
		public static Ast.Expression Hex(string text)
		{
			return new Ast.LiteralValue.Binary(text[2..]) { ZeroX = true };
		}

		public static Ast.Expression Bit()
		{
			return new Literal("Bit", "");
		}

		/// <summary>A literal of a kind the common tree has no record for yet: a date, a time, a timestamp, an interval.</summary>
		public static Ast.Expression Unread(string kind)
		{
			return new Literal(kind, "");
		}

		/// <summary>A variable, <c>@x</c>, a dynamic parameter, <c>?</c>, or a host parameter, which has no record yet.</summary>
		public static Ast.Expression Parameter(string text)
		{
			return text.StartsWith('@') ? new Ast.Expression.Variable(new Ast.Identifier(text))
				: text == "?" ? new Ast.Expression.Parameter(Ast.ParameterKind.Dynamic)
				: new Literal("Parameter", text);
		}

		/// <summary>A string literal as one token of text, <c>'a'</c> or <c>N'a'</c>, where a builder reads one from words.</summary>
		public static Ast.Expression String(string token)
		{
			return token[0] is 'N' or 'n'
				? new Ast.LiteralValue.String(token[1..], Ast.StringLiteralKind.National)
				: new Ast.LiteralValue.String(token, Ast.StringLiteralKind.Character);
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

		/// <summary>A word that stands where a value does: <c>USER</c>, <c>CURRENT_USER</c>, <c>VALUE</c> and the rest.</summary>
		public static Ast.Expression Special(string text)
		{
			Ast.CurrentValue kind;

			switch (text.ToUpperInvariant())
			{
				case "USER":              kind = Ast.CurrentValue.User; break;
				case "CURRENT_USER":      kind = Ast.CurrentValue.CurrentUser; break;
				case "SESSION_USER":      kind = Ast.CurrentValue.SessionUser; break;
				case "SYSTEM_USER":       kind = Ast.CurrentValue.SystemUser; break;
				case "VALUE":             kind = Ast.CurrentValue.Value; break;
				case "CURRENT_DATE":      kind = Ast.CurrentValue.Date; break;
				case "CURRENT_TIME":      kind = Ast.CurrentValue.Time; break;
				case "CURRENT_TIMESTAMP": kind = Ast.CurrentValue.Timestamp; break;
				default:                  return Gapped(BuildGap.Open("Expression.Literal", $"inventory SqlLiteralKind (Special `{text}`)"));
			}

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
		static Ast.Expression? JsonCall(Call old, string upper)
		{
			// The words after the last argument, and that argument without them.
			var arguments = old.Arguments.ToList();
			var words     = "";

			if (arguments is [.., Hinted(var last, var tail)])
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
					: throw BuildGap.Open(old, $"inventory Expression.RoutineInvocation ({upper} RETURNING {returning})");
			}

			Ast.JsonMember Member(Ast.Expression one)
			{
				return one is JsonPair(var key, var value)
					? new Ast.JsonMember(Of(key), Of(value), Ast.JsonMemberSyntax.Colon)
					: throw BuildGap.Open(old, $"inventory Expression.RoutineInvocation ({upper} with a member that is not a key and a value)");
			}

			BuildGap Rest()
			{
				return BuildGap.Open(old, $"inventory Expression.RoutineInvocation ({upper} with the words `{words}`)");
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
						: throw BuildGap.Open(old, $"inventory Expression.RoutineInvocation (JSON_OBJECTAGG of {(arguments.Count == 0 ? "no pair" : "several pairs")}, where the record holds one)");

				case "JSON_ARRAYAGG":
					if (arguments is not [var item])
						throw BuildGap.Open(old, "inventory Expression.RoutineInvocation (JSON_ARRAYAGG of other than one value, where the record holds one)");

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
					throw BuildGap.Open(old, $"inventory Expression.RoutineInvocation ({upper} as a call, which the standard reads as its own node)");
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

		public static Ast.Expression Function(Call old)
		{
			var name  = old.Name;
			var upper = name.ToUpperInvariant();

			// IdentityFunction: the type the old tree keeps as the call's word, and the seed and the step.
			if (upper == "IDENTITY" && old.Word is { } typed && old.Arguments.Length is 0 or 2)
				return new Ast.Expression.Identity(Type(old, typed))
				{
					Seed      = old.Arguments.Length == 2 ? (Ast.LiteralValue.Numeric)Of(old.Arguments[0]) : null,
					Increment = old.Arguments.Length == 2 ? (Ast.LiteralValue.Numeric)Of(old.Arguments[1]) : null,
				};

			// AI_GENERATE_EMBEDDINGS: what is embedded, and the model after USE MODEL, which the old tree keeps as words.
			if (upper == "AI_GENERATE_EMBEDDINGS" && old is { Word: null, Arguments: [Hinted(var source, var words)] })
				return Embeddings(old, source, words);

			// FullTextPredicate: the columns, what is searched for, and LANGUAGE and its value.
			if (upper is "CONTAINS" or "FREETEXT" && old.Word is null)
				return FullText(old, upper == "CONTAINS" ? Ast.FullTextPredicateKind.Contains : Ast.FullTextPredicateKind.FreeText);

			// `CAST (DISTINCT 1)` reads, as any call does, with a quantifier where the type would stand.
			var type = old.Word is { } word && word.ToUpperInvariant() is not ("DISTINCT" or "ALL" or "*") ? word : null;

			if (upper is "CAST" or "TRY_CAST" && type is not null)
			{
				if (old.Arguments.Length != 1)
					throw BuildGap.Open(old, "inventory Expression.RoutineInvocation (a CAST that is not one value and a type)");

				return new Ast.Expression.Cast(Of(old.Arguments[0]), Type(old, type)) { Try = upper == "TRY_CAST" };
			}

			// `[TRY_]CONVERT (type, value [, style])`: the type is the old call's word, and the arguments are the rest.
			if (upper is "CONVERT" or "TRY_CONVERT" && type is not null)
			{
				if (old.Arguments.Length is < 1 or > 2)
					throw BuildGap.Open(old, "inventory Expression.RoutineInvocation (a CONVERT that is not a type, a value and a style)");

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
					throw BuildGap.Open(old, "inventory Expression.RoutineInvocation (a PARSE that is not a value, a type and a culture)");

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
					throw BuildGap.Open(old, "inventory Expression.RoutineInvocation (a NEXT VALUE FOR without its sequence)");

				return new Ast.Expression.NextValue(Name(old, old.Word));
			}

			if (name.Equals("AT TIME ZONE", StringComparison.OrdinalIgnoreCase))
			{
				if (old.Arguments.Length != 2)
					throw BuildGap.Open(old, "inventory Expression.RoutineInvocation (AT TIME ZONE without its two operands)");

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

			// A call with no arguments written without its brackets, where the text says so: the common tree's invocation
			// has them, so that is a gap; a call not located is taken to have had them.
			if (old.Arguments.Length == 0 && old.Word is null && Recover(Spelled(old), static text => !text.TrimEnd().EndsWith(')'), false))
				throw BuildGap.Lost(old, "whether a call with no arguments was written with brackets");

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
					throw BuildGap.Open(old, $"inventory Expression.RoutineInvocation (the word `{old.Word}`)");
			}

			return new Ast.Expression.Invocation(Name(old, name), arguments) { Quantifier = quantifier };
		}

		/// <summary>
		/// A call whose first argument is a date part, <c>DATEADD (day, 1, d)</c>: the old tree reads the part as
		/// a column, and the new one as the <see cref="Ast.Expression.DatePart"/> it is.
		/// </summary>
		/// <summary>
		/// CONTAINS or FREETEXT, which the old tree keeps as a call: the columns, a list in brackets or one alone, what is
		/// searched for, and the language, the word in front of its value.
		/// </summary>
		static Ast.Expression FullText(Call old, Ast.FullTextPredicateKind kind)
		{
			if (old.Arguments is not [var searched, var search, .. var rest] || rest.Length > 1
				|| rest is [var said] && said is not Prefixed || rest is [Prefixed(var other, _)] && !other.Equals("LANGUAGE", StringComparison.OrdinalIgnoreCase))
				throw BuildGap.Open(old, $"inventory Expression.RoutineInvocation ({old.Name} with these arguments)");

			var bracketed = searched is Ast.Expression.Row;
			SqlList<Ast.Expression> items = searched is Ast.Expression.Row(var list, _) ? list : [searched];

			return new Ast.Expression.FullTextPredicate(kind, Each(items, Of), Of(search))
			{
				Language         = rest is [Prefixed(_, var language)] ? Of(language) : null,
				ColumnsBracketed = bracketed,
			};
		}

		/// <summary>
		/// <c>USE MODEL m [PARAMETERS p]</c> after AI_GENERATE_EMBEDDINGS's source: the model's name to PARAMETERS, and the
		/// value after it read again as a value.
		/// </summary>
		static Ast.Expression Embeddings(Call old, Ast.Expression source, string words)
		{
			var located = SqlTokens.Located(words);
			var tokens  = located.ConvertAll(static one => one.Token);

			if (tokens is not [{ Kind: SqlTokens.Kind.Word, Text: var use }, { Kind: SqlTokens.Kind.Word, Text: var model }, _, ..]
				|| !use.Equals("USE", StringComparison.OrdinalIgnoreCase) || !model.Equals("MODEL", StringComparison.OrdinalIgnoreCase))
				throw BuildGap.Open(old, $"inventory Expression.Hinted (`{words}` after AI_GENERATE_EMBEDDINGS)");

			// UseModel: the model's name first, a TSqlQualifiedName whose parts and dots alternate (a part may be left out
			// between two dots), so `USE MODEL PARAMETERS` names a model PARAMETERS; then PARAMETERS and its value, if any.
			var end = 2;

			if (tokens[end].Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited)
				end++;

			while (end < tokens.Count && tokens[end].Text == ".")
			{
				end++;

				if (end < tokens.Count && tokens[end].Kind is SqlTokens.Kind.Word or SqlTokens.Kind.Delimited)
					end++;
			}

			if (end < tokens.Count && !(tokens[end].Kind == SqlTokens.Kind.Word && tokens[end].Text.Equals("PARAMETERS", StringComparison.OrdinalIgnoreCase)))
				throw BuildGap.Open(old, $"inventory Expression.Hinted (`{words}` after AI_GENERATE_EMBEDDINGS)");

			var parameters = end < tokens.Count ? end : -1;
			var named      = string.Concat(tokens.Skip(2).Take(end - 2).Select(static one => one.Text));
			Ast.Expression? given = null;

			if (parameters >= 0)
			{
				// The keyword's own place in the text, not the first token spelled as it is (`PARAMETERS.PARAMETERS`).
				var at   = located[parameters].At;
				var read = Queries.Specified("SELECT " + words[(at + "PARAMETERS".Length)..]);

				given = read is { Columns: [Ast.SelectColumn.ExpressionItem { Expression: var value, Alias: null }] }
					? Of(value)
					: throw BuildGap.Open(old, $"inventory Expression.Hinted (`{words}` after AI_GENERATE_EMBEDDINGS)");
			}

			return new Ast.Expression.AiGenerateEmbeddings(Of(source), Name(old, named))
			{
				Parameters = given,
			};
		}

		static Ast.Expression DatePart(Call old)
		{
			// A word of the reference's table, unquoted and unbracketed, is the date part; anything else first — `[yy]`,
			// `(year)`, a string, a variable — is the call as written, its first argument a value as the others are.
			if (old.Arguments is not [Word(var part), ..] || !DateParts.TryGetValue(part, out var made))
				return new Ast.Expression.Invocation(Name(old, old.Name), Each(old.Arguments, Argument));

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
		/// An ODBC escape, <c>{fn LEFT (a, 2)}</c>, <c>{d '2026-09-26'}</c>: its kind, what is inside it, and whether the
		/// function's brackets were written. A function inside one is ODBC's and not T-SQL's — <c>{fn CONVERT (a,
		/// SQL_INTEGER)}</c> names its type as ODBC does — so it is a plain call.
		/// </summary>
		public static Ast.Expression Odbc(string kind, Ast.Expression inside, bool called)
		{
			try
			{
				if (kind.Length == 0)
					throw BuildGap.Open("Expression.OdbcEscape", "inventory Expression.OdbcEscape (an escape without its kind)");

				var value = inside is Call { Word: null } call
					? new Ast.Expression.Invocation(Name(call, call.Name), Each(call.Arguments, Argument))
					: Of(inside);

				return new Ast.Expression.OdbcEscape(Part("Expression.OdbcEscape", kind), value, called);
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, null, false, [inside]);
			}
		}

		/// <summary>What <c>{ fn … }</c> holds, and whether a function's brackets were written in it.</summary>
		internal sealed record OdbcInside(Ast.Expression Value, bool Called);

		/// <summary>
		/// T-SQL's <c>TRIM ([LEADING | TRAILING | BOTH] [characters FROM] string)</c>, which the old tree keeps as
		/// a call with the specification as its word: two arguments are the characters and the string, and
		/// <c>FROM</c> stood between them; one is the string, and <c>FROM</c> stood before it where the text says.
		/// </summary>
		static Ast.Expression Trim(Call old)
		{
			Ast.TrimSpecification? specification = old.Word?.ToUpperInvariant() switch
			{
				null       => null,
				"LEADING"  => Ast.TrimSpecification.Leading,
				"TRAILING" => Ast.TrimSpecification.Trailing,
				"BOTH"     => Ast.TrimSpecification.Both,
				_          => throw BuildGap.Open(old, $"inventory Expression.RoutineInvocation (TRIM's word `{old.Word}`)"),
			};

			return old.Arguments switch
			{
				[var characters, var value] => new Ast.Expression.Trim(specification, Of(characters), true, Of(value)),
				[var value]                 => new Ast.Expression.Trim(specification, null,
					specification is not null && Recovered(old.From, false), Of(value)),
				_                           => throw BuildGap.Open(old, "inventory Expression.RoutineInvocation (TRIM with no string)"),
			};
		}

		/// <summary>What may follow a call, as <c>CallTail</c> reads it: a null treatment or a WITHIN GROUP as written, and the window.</summary>
		internal sealed record Windowing(string? Within, Clause? Over);

		/// <summary>A call and what was written after it, where anything was.</summary>
		public static Ast.Expression Called(Ast.Expression call, Windowing? tail)
		{
			if (tail is null)
				return call;

			try
			{
				return Windowed(call, tail);
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, null, false, [call]);
			}
		}

		/// <summary>The record a window's gap is said under.</summary>
		const string WindowFunction = "Expression.WindowFunction";

		static Ast.Expression Windowed(Ast.Expression called, Windowing old)
		{
			if (called is Call sequence && Function(sequence) is Ast.Expression.NextValue next)
			{
				// `NEXT VALUE FOR s OVER (ORDER BY …)`: a specification, never a window's name.
				if (old.Within is not null || old.Over is not Clause.Window window)
					throw BuildGap.Open(WindowFunction, "inventory Expression.WindowFunction (NEXT VALUE FOR over something that is not a window)");

				return next with { Over = Specification(window) };
			}

			// A JSON aggregate takes its window as the standard's record does; WITHIN GROUP after it has no place there.
			var function = called is Call aggregate ? Function(aggregate) : null;

			if (function is Ast.Expression.JsonObjectAggregate or Ast.Expression.JsonArrayAggregate)
			{
				if (old.Within is not null || old.Over is null)
					throw BuildGap.Open(WindowFunction, "inventory Expression.WindowFunction (a JSON aggregate with WITHIN GROUP or a null treatment, which its record has no place for)");

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
						within = new Ast.WithinGroupClause(Ordering(WindowFunction, text[(SqlTokens.Find(text, "(") + 1)..SqlTokens.FindLast(text, ")")]));
						break;

					default:
						throw BuildGap.Open(WindowFunction, $"inventory Expression.WindowFunction (the words `{text}` before its window)");
				}
			}

			if (function is not Ast.Expression.Invocation invocation)
				throw BuildGap.Open(WindowFunction, "inventory Expression.WindowFunction (a window over something that is not a call)");

			invocation = invocation with { Nulls = nulls, WithinGroup = within, GraphPath = path };

			return old.Over is null ? invocation : invocation with { Over = Window(old.Over) };
		}

		/// <summary>
		/// The tokens of a stretch of text with where each begins in it, as the scan found them, so that a token is never
		/// found again inside a comment or another token before it.
		/// </summary>
		static List<(SqlTokens.Token Token, int At)> Placed(string text)
		{
			return SqlTokens.Located(text);
		}

		/// <summary>
		/// An <c>ORDER BY …</c> the old tree keeps as text, read by the old grammar as a query's own order, and
		/// carried as that order is.
		/// </summary>
		static Ast.OrderByClause Ordering(object owner, string text)
		{
			if (Queries.Selected("SELECT 1 " + text) is { By: Clause.OrderBy { Offset: null, Fetch: null } by })
				return Queries.Order(by);

			throw BuildGap.Open(owner, $"inventory Expression.WindowFunction (an order `{text}` that does not read)");
		}

		/// <summary>
		/// An <c>OVER</c>: a window's name alone, or a specification.
		/// </summary>
		static Ast.WindowReference Window(Clause over)
		{
			if (over is not Clause.Window window)
				throw BuildGap.Open(over, "inventory Clause.Window (an OVER that is not a window)");

			// `OVER w` and `OVER (w)` are one old node; the text says which, and without it the name alone is taken.
			// Located where the window was read: the OVER with the name or the brackets after it.
			if (window is { Name: { } named, PartitionBy.Length: 0, By: null, Frame: null }
				&& !Recover(Spelled(window), static text => SqlTokens.Of(text) is [_, { Text: "(" }, ..], false))
				return Build.Placed(new Ast.WindowReference.NameRef(Part(over, named)), window, locating);

			return Build.Placed(new Ast.WindowReference.Specification(Specification(window)), window, locating);
		}

		public static Ast.WindowSpecification Specification(Clause.Window window)
		{
			Ast.OrderByClause? order = null;

			if (window.By is Clause.OrderBy by)
			{
				if (by.Offset is not null || by.Fetch is not null)
					throw BuildGap.Open(window, "inventory Clause.OrderBy (OFFSET in a window)");

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
				return at < words.Count ? words[at++] : null;
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
						throw BuildGap.Open(window, $"inventory Clause.Window (a frame `{text}`)");
				}
			}

			var unit = Next() switch
			{
				"ROWS"  => Ast.WindowFrameUnit.Rows,
				"RANGE" => Ast.WindowFrameUnit.Range,
				_       => throw BuildGap.Open(window, $"inventory Clause.Window (a frame `{text}`)"),
			};

			Ast.WindowFrameExtent extent;

			if (at < words.Count && words[at] == "BETWEEN")
			{
				at++;

				var from = Bound();

				if (Next() != "AND")
					throw BuildGap.Open(window, $"inventory Clause.Window (a frame `{text}`)");

				extent = new Ast.WindowFrameExtent.Between(from, Bound());
			}
			else
				extent = new Ast.WindowFrameExtent.Single(Bound());

			if (at != words.Count)
				throw BuildGap.Open(window, $"inventory Clause.Window (a frame `{text}`)");

			return new Ast.WindowFrame(unit, extent, null, null);
		}

		// ── Members and zones ─────────────────────────────────────────────────────

		/// <summary>
		/// A member reached through a value, <c>.name</c> or <c>::name</c>, and a method's arguments: read after the value,
		/// and built over it where the value is closed (<see cref="Reached"/>).
		/// </summary>
		internal sealed record Reach(string By, string Name, SqlList<Ast.Expression>? Arguments) : Read;

		/// <summary>
		/// <c>AT TIME ZONE zone</c>, with the collation written in front of it, read after the value and built over it where the
		/// value is closed.
		/// </summary>
		internal sealed record Zoning(string? Collation, Ast.Expression Zone) : Read;

		/// <summary>
		/// A primary and what was reached through it: the members written after it, and the zones it is read in, each with
		/// the collation written in front of it. Each is built over what stood to its left, told the range from the primary's
		/// first character to its own last.
		/// </summary>
		public static Ast.Expression Reached(Ast.Expression primary, Reach[]? members, Zoning[]? zones)
		{
			if (members is null && zones is null)
				return primary;

			try
			{
				if (members is not null)
					foreach (var member in members)
						primary = Joined(new Ast.Expression.Member(
							Of(primary),
							member.By == "::" ? Ast.MemberAccessKind.StaticMethod : Ast.MemberAccessKind.Dot,
							Part("Expression.Member", member.Name),
							member.Arguments is { } passed ? Each(passed, Argument) : null), primary, member);

				if (zones is not null)
					foreach (var zone in zones)
					{
						// The collation in front of a zone is told the value's range: it was written with no range of its own.
						var of = zone.Collation is { } how
							? Joined(new Ast.Expression.Collate(Of(primary), new Ast.CollationName(Name("Expression.Collated", how))), primary, primary)
							: primary;

						primary = Joined(new Ast.Expression.AtTimeZone(Of(of), Of(zone.Zone)), primary, zone);
					}

				return primary;
			}
			catch (BuildGap gap)
			{
				return Gapped(gap, null, false, Parts(primary, members, zones));
			}
		}

		/// <summary>
		/// A node built over others, told the range from the first of them to the last, as the reading tells a node: a node
		/// made while reading is no edit.
		/// </summary>
		static Ast.Expression Joined(Ast.Expression made, ISqlSpan first, ISqlSpan last)
		{
			var head = SpanOf(first);
			var tail = SpanOf(last);

			if (!head.Known && !tail.Known)
				return made;

			var from = head.Known ? head : tail;
			var to   = tail.Known ? tail : head;

			((ISqlLocatable)made).Locate(from.At, to.End - from.At, from.At);

			return made;
		}

		// ── What a builder takes apart ────────────────────────────────────────────

		/// <summary>
		/// A value of a shape only a builder takes apart, or that has no place on the common tree yet: kept as read, with
		/// the record it was read as, under which its gap is said.
		/// </summary>
		internal abstract record Held(string Record) : Ast.Expression
		{
			/// <summary>What it is as a value of the common tree; a shape with no place there throws its gap.</summary>
			public abstract Ast.Expression Resolved();
		}

		/// <summary>A value with no place on the common tree, and why: what a builder that takes it throws.</summary>
		internal sealed record Unbuilt(BuildGap Gap) : Held(Gap.OldType)
		{
			/// <summary>What the value was read as, where a rule asks: <c>Expression.Parenthesized</c>, <c>Expression.Subquery</c>.</summary>
			public string? Shape { get; init; }

			/// <summary>Whether the value held a query of its own, which a rule's condition asks (<see cref="Rules.Binds"/>).</summary>
			public bool HoldsQuery { get; init; }

			/// <summary>The values it was read over, which a rule's condition walks as it walks any value.</summary>
			public SqlList<Ast.Expression> Parts { get; init; } = [];

			public override Ast.Expression Resolved()
			{
				throw Gap;
			}
		}

		/// <summary>
		/// A subquery in a pair of brackets of its own, <c>((SELECT 1))</c>: kept as written until a builder takes it, so that
		/// a rule that asks whether a value was written in brackets (<c>TOP</c>, <c>RECEIVE TOP</c>) or whether it is a
		/// subquery (<c>PREDICT</c>'s model and data) is answered as for any other bracketed value. As a value it is the
		/// common tree's subquery of a bracketed query: the second pair is the query's (<see cref="Ast.Query.Parenthesized"/>).
		/// </summary>
		internal sealed record Enclosed(Ast.Expression.Subquery Value) : Held("Expression.Parenthesized")
		{
			public override Ast.Expression Resolved()
			{
				return new Ast.Expression.Subquery(new Ast.Query.Parenthesized(Value.Query));
			}
		}

		/// <summary>A literal of a kind the common tree has no record for yet, and its text: a host parameter's.</summary>
		internal sealed record Literal(string Kind, string Text) : Held("Expression.Literal")
		{
			public override Ast.Expression Resolved()
			{
				throw Kind == "Bit"
					? BuildGap.Missing(Record, "a bit string literal, which SQL:2023 removed")
					: BuildGap.Open(Record, $"inventory SqlLiteralKind ({Kind})");
			}
		}

		/// <summary>
		/// A word written where a name or a value stands, as written: a column (<c>t.*</c> all of <c>t</c>'s, a bare
		/// <c>*</c> every one), or a word an option is set to, which its builder reads as a word.
		/// </summary>
		internal sealed record Word(string Text) : Held("Expression.ColumnReference")
		{
			public override Ast.Expression Resolved()
			{
				return Of(Text);
			}

			/// <summary>The column a word names.</summary>
			public static Ast.Expression Of(string text)
			{
				// `t.*` as an argument: all columns of `t`, its target the table's reference, as the standard's parser builds it.
				if (text.Length > 2 && text.EndsWith(".*", StringComparison.Ordinal))
					return new Ast.Expression.Wildcard(new Ast.Expression.Reference(Name(Column, text[..^2])), Ast.WildcardKind.Member);

				// A bare `*` where a value stands: CONTAINS, FREETEXT and the semantic-search functions' every column.
				return text == "*" ? new Ast.Expression.Asterisk() : new Ast.Expression.Reference(Name(Column, text));
			}

			const string Column = "Expression.ColumnReference";
		}

		/// <summary>
		/// A call: the name as written, its arguments, and the one word a few of them carry — <c>DISTINCT</c>, a date part,
		/// a trim specification, the type a cast names. What it is depends on all three (<see cref="Function"/>).
		/// </summary>
		internal sealed record Call(string Name, SqlList<Ast.Expression> Arguments, string? Word = null) : Held("Expression.RoutineInvocation")
		{
			/// <summary>Whether TRIM's <c>FROM</c> was written, which a trim of one value keeps.</summary>
			public bool From { get; init; }

			public override Ast.Expression Resolved()
			{
				return Function(this);
			}
		}

		/// <summary>An argument with a word in front of it: <c>BULK 'f'</c>, <c>CHANGES t</c>, <c>LANGUAGE 1033</c>.</summary>
		internal sealed record Prefixed(string Word, Ast.Expression Value) : Held("Expression.Prefixed")
		{
			public override Ast.Expression Resolved()
			{
				throw BuildGap.Missing(Record, "a typed T-SQL rowset table source (proposal 7)");
			}
		}

		/// <summary>A value and the unit written after it: <c>10 MINUTES</c>, <c>50 PERCENT</c>, <c>4 GB</c>.</summary>
		internal sealed record Measured(Ast.Expression Value, string Unit) : Held("Expression.Measured")
		{
			public override Ast.Expression Resolved()
			{
				throw BuildGap.Unwritten(Record, "Expression.Measured");
			}
		}

		/// <summary>A value with the words written after it that say how it is to be taken, as written.</summary>
		internal sealed record Hinted(Ast.Expression Value, string Words) : Held("Expression.Hinted")
		{
			public override Ast.Expression Resolved()
			{
				throw BuildGap.Open(Record, "inventory Expression.Hinted");
			}
		}

		/// <summary><c>key : value</c> in a JSON constructor.</summary>
		internal sealed record JsonPair(Ast.Expression Key, Ast.Expression Value) : Held("Expression.JsonPair")
		{
			public override Ast.Expression Resolved()
			{
				throw BuildGap.Open(Record, "inventory Expression.JsonPair");
			}
		}

		/// <summary>An argument written with its name, <c>MODEL = m</c>.</summary>
		internal sealed record NamedArgument(string Name, Ast.Expression Value) : Held("Expression.NamedArgument")
		{
			public override Ast.Expression Resolved()
			{
				throw BuildGap.Open(Record, "inventory Expression.NamedArgument");
			}
		}

		/// <summary>A value with the name it is known by, <c>DATA = t AS d</c>, and whether AS was written.</summary>
		internal sealed record Aliased(Ast.Expression Value, string Alias, bool As) : Held("Expression.Aliased")
		{
			public override Ast.Expression Resolved()
			{
				throw BuildGap.Open(Record, "inventory Expression.Aliased");
			}
		}

		/// <summary>Values written as one argument with semicolons between them, OPENROWSET's login.</summary>
		internal sealed record Pieced(SqlList<Ast.Expression> Parts) : Held("Expression.Pieced")
		{
			public override Ast.Expression Resolved()
			{
				throw BuildGap.Missing(Record, "a typed T-SQL rowset table source (proposal 7)");
			}
		}

		/// <summary>The <c>ORDER (c1 ASC, c2 DESC) UNIQUE</c> a bulk rowset is declared to arrive in.</summary>
		internal sealed record RowsetOrder(SqlList<Clause> By, bool IsUnique) : Held("Expression.RowsetOrder")
		{
			public override Ast.Expression Resolved()
			{
				throw BuildGap.Missing(Record, "a typed T-SQL rowset table source (proposal 7)");
			}
		}

		// ── Names ─────────────────────────────────────────────────────────────────

		/// <summary>
		/// A name the old tree keeps as joined text, split into its parts: <c>[dbo].[t]</c>, <c>a..b</c> with an
		/// empty part, <c>"x"</c>, <c>#t</c>. A variable is not a name and is refused.
		/// </summary>
		public static Ast.QualifiedName Name(object owner, string text)
		{
			if (text.StartsWith('@'))
				throw BuildGap.Missing(owner, "a variable where a name stands (an assignment, a table source, EXEC); the family's `Variable` member comes with those statements");

			// A name of one part, which nearly every name is: no list of its parts to cut it into.
			if (text.IndexOf('.') < 0)
				return new Ast.QualifiedName(Part(owner, text, split: false));

			if (Dotted(text) is { } dotted)
				return dotted;

			var parts = Split(text);
			var first = Part(owner, parts[0], split: false);
			var rest  = new Ast.Identifier[parts.Count - 1];

			for (var at = 0; at < rest.Length; at++)
				rest[at] = Part(owner, parts[at + 1], split: false);

			return new Ast.QualifiedName(first, rest);
		}

		/// <summary>
		/// A name of several parts that are all bare words (<c>dbo.Orders</c>, <c>o.Total</c>, <c>db.dbo.t</c>), cut at its dots
		/// without the tokens a delimited part would need; null for any other text, which <see cref="Name"/> cuts as it always did.
		/// A part is ASCII letters, digits and underscores, never empty and never starting with a digit, so that none of the
		/// checks <see cref="Part(object, string)"/> makes can refuse it.
		/// </summary>
		static Ast.QualifiedName? Dotted(string text)
		{
			var dots = 0;

			for (var at = 0; at < text.Length; at++)
			{
				var c = text[at];

				if (c == '.')
				{
					if (at == 0 || at == text.Length - 1 || text[at - 1] == '.')
						return null;

					dots++;
				}
				else if (c is >= '0' and <= '9')
				{
					if (at == 0 || text[at - 1] == '.')
						return null;
				}
				else if (!(IsAsciiLetter(c) || c == '_'))
					return null;
			}

			if (dots == 0)
				return null;

			var rest  = new Ast.Identifier[dots];
			var start = text.IndexOf('.');
			var first = new Ast.Identifier(text.Substring(0, start));

			for (var one = 0; one < dots; one++)
			{
				var end = text.IndexOf('.', start + 1);

				if (end < 0)
					end = text.Length;

				rest[one] = new Ast.Identifier(text.Substring(start + 1, end - start - 1));
				start     = end;
			}

			return new Ast.QualifiedName(first, rest);
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
			if (split && text.Length > 0 && text[0] != '@' && text.IndexOf('.') >= 0 && Split(text).Count > 1)
				throw BuildGap.Open(owner, $"inventory cross-cutting gaps, identifier styles (a qualified name `{text}` where one name stands)");

			if (text.StartsWith('@'))
				throw BuildGap.Missing(owner, "a variable where a name stands (an assignment, a table source, EXEC); the family's `Variable` member comes with those statements");

			if (Blank(text) && text.Length > 0 && text[0] is not '[' and not '"')
				throw BuildGap.Open(owner, $"inventory cross-cutting gaps, identifier styles (a name part `{text}`)");

			// A part that is none: a string or a number where a name stands (`OPENDATASOURCE (…).'t'`, `xml(10)`), or a
			// delimited name with more text after its closing delimiter (`"Char" varying`, a type word the old tree keeps).
			// A delimited name with blanks around it (`[a] .b`) is the blank's gap above.
			if (text.Length > 0 && (text[0] == '\'' || char.IsDigit(text[0]) || (text[0] is '[' or '"' && !Delimited(text))))
				throw BuildGap.Open(owner, text[0] is '[' or '"' && Delimited(text.TrimEnd())
					? $"inventory cross-cutting gaps, identifier styles (a name part `{text}`)"
					: $"inventory cross-cutting gaps, identifier styles (a name part `{text}` that is no name)");

			return text.Length > 0 && text[0] == '[' ? new Ast.Identifier(text, Ast.IdentifierStyle.Bracketed)
				: text.Length > 0 && text[0] == '"' ? new Ast.Identifier(text, Ast.IdentifierStyle.Delimited)
				: new Ast.Identifier(text);
		}

		/// <summary>Whether a text holds a blank anywhere.</summary>
		static bool Blank(string text)
		{
			foreach (var c in text)
				if (TransactSqlParser.Spacing(c))
					return true;

			return false;
		}

		/// <summary>Whether a text is one delimited name, from its opening delimiter to its closing one, which is its last character.</summary>
		static bool Delimited(string text)
		{
			var close = text[0] == '[' ? ']' : '"';

			for (var at = 1; at < text.Length; at++)
			{
				if (text[at] != close)
					continue;

				if (at + 1 < text.Length && text[at + 1] == close)
				{
					at++;

					continue;
				}

				return at == text.Length - 1;
			}

			return false;
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
			if (Plain(text) is { } plain)
				return plain;

			var open  = SqlTokens.Find(text, "(");
			var head  = (open < 0 ? text : text[..open]).Trim();
			var words = string.Join(' ', Spaced(head).Split([' '], StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
			var tail  = open < 0 ? "" : text[open..].Trim();

			if (tail.Length > 0 && (!tail.EndsWith(')') || tail.IndexOf(')') != tail.Length - 1))
				throw BuildGap.Open(owner, $"inventory DataType (a type `{text}` whose brackets do not close it)");

			var inside    = tail.Length > 0 ? tail[1..^1].Trim() : "";
			var arguments = tail.Length > 0 ? inside.Split(',').Select(static one => one.Trim()).ToArray() : [];

			// A name in the brackets is xml's schema collection alone.
			if (arguments.Any(static one => one.StartsWith('[') || one.StartsWith('"')) && !Unquoted(head.Split('.')[^1].Trim()).Equals("xml", StringComparison.OrdinalIgnoreCase))
				throw BuildGap.Open(owner, $"inventory DataType (`{text}`: a name in the brackets of a type)");

			// DottedType: NATIONAL before a name and VARYING after it, the name quoted or qualified by sys: the standard's
			// spelling of several words, written with that name.
			if (SpelledType(owner, head, arguments) is { } spelled)
				return spelled;

			// Several words unbracketed: only the standard's spellings of several words.
			if (head.Length == 0 || Words(head).Count != 1 && !head.Contains('.'))
				return Standard(words, arguments) ?? throw BuildGap.Open(owner, $"inventory DataType (a type `{text}` of several words)");

			var name   = Name(owner, head);
			var last   = Unquoted(name.Parts[^1].Text).TrimEnd();
			var system = name.Parts.Count == 1 || name.Parts.Count == 2 && Unquoted(name.Parts[0].Text).Equals("sys", StringComparison.OrdinalIgnoreCase);
			var bare   = name.Parts is [{ Style: Ast.IdentifierStyle.Regular }];

			if (system && StandardWords.Contains(last) && Standard(last.ToUpperInvariant(), arguments) is { } standard)
				return bare ? standard : standard with { WrittenName = name };

			// A vector's dimensions or MAX, and its base type, one of the page's two words; any other word is a gap.
			if (system && last.Equals("vector", StringComparison.OrdinalIgnoreCase))
			{
				var vector = arguments switch
				{
					[] => new Ast.DataType.Vector(null),
					[var max] when max.Equals("MAX", StringComparison.OrdinalIgnoreCase) => new Ast.DataType.Vector(null) { Max = true },
					[var size] when int.TryParse(size, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var dimensions) => new Ast.DataType.Vector(dimensions),
					[var size, var word] when int.TryParse(size, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var dimensions)
						&& word.ToUpperInvariant() is "FLOAT32" or "FLOAT16" => new Ast.DataType.Vector(dimensions)
						{
							BaseType = word.Equals("FLOAT32", StringComparison.OrdinalIgnoreCase) ? Ast.VectorBaseType.Float32 : Ast.VectorBaseType.Float16,
						},
					_ => throw BuildGap.Open(owner, $"inventory DataType (`{text}`: a vector of other than dimensions, MAX and a base type of the page's)"),
				};

				return bare ? vector : vector with { WrittenName = name };
			}

			// MAX after a type with no MAX of its own (AnyLength, ExactMax): the type's name and the word.
			if (system && arguments is [var only] && only.Equals("MAX", StringComparison.OrdinalIgnoreCase)
				&& SystemTypes.Contains(last) && !last.Equals("xml", StringComparison.OrdinalIgnoreCase) && !last.Equals("vector", StringComparison.OrdinalIgnoreCase))
				return new Ast.DataType.Named(name, []) { Max = true };

			// xml, however written, before the system types, so that `[xml]` is never Named.
			if (system && last.Equals("xml", StringComparison.OrdinalIgnoreCase))
				return bare ? Xml(owner, text, inside) : Xml(owner, text, inside) with { WrittenName = name };

			if (system && SystemTypes.Contains(last))
				return arguments.All(static one => one.Length > 0 && one.All(char.IsDigit))
					? new Ast.DataType.Named(name, [.. arguments.Select(static one => Value(new Ast.LiteralValue.Numeric(one, Ast.NumericLiteralKind.DecimalInteger)))])
					: throw BuildGap.Open(owner, $"inventory DataType (`{text}`: a word in the brackets of a type)");

			// A type of the user's with the numbers DottedTail reads after it, one or two, or MAX alone.
			return arguments switch
			{
				[] => new Ast.DataType.UserDefined(name),
				[var max] when max.Equals("MAX", StringComparison.OrdinalIgnoreCase) => new Ast.DataType.UserDefined(name) { Max = true },
				{ Length: 1 or 2 } when arguments.All(static one => one.Length > 0 && one.All(char.IsDigit))
					=> new Ast.DataType.UserDefined(name) { Arguments = [.. arguments.Select(static one => Value(new Ast.LiteralValue.Numeric(one, Ast.NumericLiteralKind.DecimalInteger)))] },
				_ => throw BuildGap.Open(owner, $"inventory DataType (`{text}`: a user-defined type with arguments)"),
			};
		}

		/// <summary>
		/// The type a bare word spells, with numbers or <c>MAX</c> in brackets after it (<c>int</c>, <c>DECIMAL(18, 2)</c>,
		/// <c>nvarchar(50)</c>, <c>varchar(MAX)</c>), which nearly every type is, answered without cutting the text into tokens;
		/// null for any other text, which <see cref="Type"/> reads as it always did. The answers are the ones its steps give
		/// such a text, in their order: a standard spelling, <c>xml</c> and <c>vector</c> left to them, a system type as
		/// <see cref="Ast.DataType.Named"/>, any other word as <see cref="Ast.DataType.UserDefined"/>. Where those steps would
		/// refuse the text (arguments they have no place for) this answers null as well, so that they refuse it.
		/// </summary>
		static Ast.DataType? Plain(string text)
		{
			if (text.Length == 0 || !(IsAsciiLetter(text[0]) || text[0] == '_'))
				return null;

			var end = 1;

			while (end < text.Length && (IsAsciiLetter(text[end]) || text[end] is >= '0' and <= '9' || text[end] == '_'))
				end++;

			var arguments = Array.Empty<string>();
			var at        = end;

			while (at < text.Length && IsBlank(text[at]))
				at++;

			if (at < text.Length)
			{
				var last = text.Length;

				while (IsBlank(text[last - 1]))
					last--;

				if (text[at] != '(' || last - at < 3 || text[last - 1] != ')')
					return null;

				// The arguments between the brackets, each without the blanks around it: whole numbers, or MAX.
				var count = 1;

				for (var i = at + 1; i < last - 1; i++)
				{
					var c = text[i];

					if (c == ',')
						count++;
					else if (!(IsAsciiLetter(c) || c is >= '0' and <= '9' || IsBlank(c)))
						return null;
				}

				arguments = new string[count];

				var from = at + 1;

				for (var one = 0; one < count; one++)
				{
					var to = from;

					while (to < last - 1 && text[to] != ',')
						to++;

					var start = from;
					var stop  = to;

					while (start < stop && IsBlank(text[start]))
						start++;

					while (stop > start && IsBlank(text[stop - 1]))
						stop--;

					var argument = text.Substring(start, stop - start);

					if (argument.Length == 0 || !(IsNumber(argument) || argument.Equals("MAX", StringComparison.OrdinalIgnoreCase)))
						return null;

					arguments[one] = argument;
					from           = to + 1;
				}
			}

			var word = text.Substring(0, end);

			if (StandardWords.Contains(word) && Standard(word.ToUpperInvariant(), arguments) is { } standard)
				return standard;

			if (word.Equals("xml", StringComparison.OrdinalIgnoreCase) || word.Equals("vector", StringComparison.OrdinalIgnoreCase))
				return null;

			var name = new Ast.QualifiedName(new Ast.Identifier(word));
			var max  = arguments is [var only] && only.Equals("MAX", StringComparison.OrdinalIgnoreCase);

			if (SystemTypes.Contains(word))
			{
				if (max)
					return new Ast.DataType.Named(name, []) { Max = true };

				return Numbers(arguments)
					? new Ast.DataType.Named(name, [.. arguments.Select(static one => Value(new Ast.LiteralValue.Numeric(one, Ast.NumericLiteralKind.DecimalInteger)))])
					: null;
			}

			if (arguments.Length == 0)
				return new Ast.DataType.UserDefined(name);

			if (max)
				return new Ast.DataType.UserDefined(name) { Max = true };

			return arguments.Length is 1 or 2 && Numbers(arguments)
				? new Ast.DataType.UserDefined(name) { Arguments = [.. arguments.Select(static one => Value(new Ast.LiteralValue.Numeric(one, Ast.NumericLiteralKind.DecimalInteger)))] }
				: null;
		}

		static bool IsAsciiLetter(char c)
		{
			return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z';
		}

		static bool IsBlank(char c)
		{
			return c is ' ' or '\t' or '\r' or '\n';
		}

		static bool IsNumber(string text)
		{
			foreach (var c in text)
				if (c is < '0' or > '9')
					return false;

			return text.Length > 0;
		}

		static bool Numbers(string[] arguments)
		{
			foreach (var one in arguments)
				if (!IsNumber(one))
					return false;

			return true;
		}

		/// <summary>
		/// <c>national sys.char varying</c>, <c>[sys].[char] varying</c>, <c>national [sys].[char]</c>: NATIONAL, a name of
		/// one part or <c>sys</c> and one, quoted or qualified, and VARYING, NATIONAL or VARYING written, which make with the
		/// name's word one of the standard's spellings of several words. The standard's record with the name as written;
		/// null where the head is no such thing.
		/// </summary>
		static Ast.DataType? SpelledType(object owner, string head, string[] arguments)
		{
			var located  = SqlTokens.Located(head);
			var tokens   = located.ConvertAll(static one => one.Token);
			var national = tokens is [{ Kind: SqlTokens.Kind.Word, Text: var first }, ..] && first.Equals("NATIONAL", StringComparison.OrdinalIgnoreCase);
			var varying  = tokens.Count > 1 && tokens[^1] is { Kind: SqlTokens.Kind.Word, Text: var last } && last.Equals("VARYING", StringComparison.OrdinalIgnoreCase);
			var from     = national ? 1 : 0;
			var count    = tokens.Count - from - (varying ? 1 : 0);

			if (!national && !varying || count is not (1 or 3))
				return null;

			var part = tokens[from];

			if (count == 3 && (tokens[from + 1].Text != "." || !Unquoted(part.Text).Equals("sys", StringComparison.OrdinalIgnoreCase)))
				return null;

			var named = tokens[from + count - 1];

			if (named.Kind is not (SqlTokens.Kind.Word or SqlTokens.Kind.Delimited) || part.Kind is not (SqlTokens.Kind.Word or SqlTokens.Kind.Delimited))
				return null;

			// Bare and unqualified, the words are the standard's spelling already, which the caller reads.
			if (count == 1 && named.Kind == SqlTokens.Kind.Word)
				return null;

			var spelling = (national ? "NATIONAL " : "") + Unquoted(named.Text).ToUpperInvariant() + (varying ? " VARYING" : "");

			return Standard(spelling, arguments) is { } standard
				? standard with { WrittenName = Name(owner, head[located[from].At..(located[from + count - 1].At + named.Text.Length)]) }
				: null;
		}

		/// <summary>A text with each character of the grammar's spacing made a space.</summary>
		static string Spaced(string text)
		{
			var made = text.ToCharArray();

			for (var at = 0; at < made.Length; at++)
				if (TransactSqlParser.Spacing(made[at]))
					made[at] = ' ';

			return new string(made);
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
				throw BuildGap.Open(owner, $"inventory DataType (an xml type `{text}`)");

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
