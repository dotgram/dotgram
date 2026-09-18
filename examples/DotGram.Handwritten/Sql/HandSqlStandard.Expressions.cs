using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Handwritten;

using After = SqlTowers.After;
using Bracket = SqlTowers.Bracket;
using Op = SqlTowers.Op;
using Operated = SqlTowers.Operated;
using Piece = SqlTowers.Piece;
using Step = SqlTowers.Step;
using Stepping = SqlTowers.Stepping;
using Tail = SqlTowers.Tail;
using Typed = SqlTowers.Typed;

// ISO/IEC 9075-2:2023 §6.28's value expressions, §6.3's primaries and §8's predicates, which are
// one reading: the BNF types its expressions as towers that meet only in a primary, so the shape
// they share is read once and SqlTowers says what the whole can still be.
partial class HandSqlStandard
{
	// ── The publications ───────────────────────────────────────────────────────

	/// <summary>Reads the whole input as a <c>&lt;value expression&gt;</c>.</summary>
	public static Expression ParseValueExpression(string input) =>
		TryParseValueExpression(input, out var value) ? value : throw Refused(input, "value expression");

	/// <summary>Reads the whole input as a <c>&lt;value expression&gt;</c>, answering rather than throwing.</summary>
	public static bool TryParseValueExpression(string input, out Expression value)
	{
		var cursor = new SqlCursor(input);

		return Whole(ValueExpression(ref cursor, out var read), ref cursor, read, out value);
	}

	public static Expression ParseCommonValueExpression(string input) =>
		TryParseCommonValueExpression(input, out var value) ? value : throw Refused(input, "common value expression");

	public static bool TryParseCommonValueExpression(string input, out Expression value) => Tower(input, SqlTowers.Value, out value);

	public static Expression ParseNumericValueExpression(string input) =>
		TryParseNumericValueExpression(input, out var value) ? value : throw Refused(input, "numeric value expression");

	public static bool TryParseNumericValueExpression(string input, out Expression value) => Tower(input, SqlTowers.Numeric, out value);

	public static Expression ParseStringValueExpression(string input) =>
		TryParseStringValueExpression(input, out var value) ? value : throw Refused(input, "string value expression");

	public static bool TryParseStringValueExpression(string input, out Expression value) => Tower(input, SqlTowers.String, out value);

	public static Expression ParseCharacterValueExpression(string input) =>
		TryParseCharacterValueExpression(input, out var value) ? value : throw Refused(input, "character value expression");

	public static bool TryParseCharacterValueExpression(string input, out Expression value) => Tower(input, SqlTowers.Character, out value);

	public static Expression ParseBinaryValueExpression(string input) =>
		TryParseBinaryValueExpression(input, out var value) ? value : throw Refused(input, "binary value expression");

	public static bool TryParseBinaryValueExpression(string input, out Expression value) => Tower(input, SqlTowers.Binary, out value);

	public static Expression ParseDatetimeValueExpression(string input) =>
		TryParseDatetimeValueExpression(input, out var value) ? value : throw Refused(input, "datetime value expression");

	public static bool TryParseDatetimeValueExpression(string input, out Expression value) => Tower(input, SqlTowers.Datetime, out value);

	public static Expression ParseIntervalValueExpression(string input) =>
		TryParseIntervalValueExpression(input, out var value) ? value : throw Refused(input, "interval value expression");

	public static bool TryParseIntervalValueExpression(string input, out Expression value) => Tower(input, SqlTowers.Interval, out value);

	public static Expression ParseBooleanValueExpression(string input) =>
		TryParseBooleanValueExpression(input, out var value) ? value : throw Refused(input, "boolean value expression");

	public static bool TryParseBooleanValueExpression(string input, out Expression value)
	{
		var cursor = new SqlCursor(input);

		return Whole(BooleanValueExpression(ref cursor, out var read), ref cursor, read, out value);
	}

	public static Expression ParseSearchCondition(string input) =>
		TryParseSearchCondition(input, out var value) ? value : throw Refused(input, "search condition");

	public static bool TryParseSearchCondition(string input, out Expression value) => TryParseBooleanValueExpression(input, out value);

	public static Expression ParsePredicate(string input) =>
		TryParsePredicate(input, out var value) ? value : throw Refused(input, "predicate");

	public static bool TryParsePredicate(string input, out Expression value)
	{
		var cursor = new SqlCursor(input);

		return Whole(BooleanPrimary(ref cursor, out var read) && (read.Roles & SqlTowers.Logical) != 0, ref cursor, read, out value);
	}

	public static Expression ParseRowValuePredicand(string input) =>
		TryParseRowValuePredicand(input, out var value) ? value : throw Refused(input, "row value predicand");

	public static bool TryParseRowValuePredicand(string input, out Expression value)
	{
		var cursor = new SqlCursor(input);

		return Whole(RowValuePredicand(ref cursor, out var read), ref cursor, read, out value);
	}

	public static Expression ParseValueExpressionPrimary(string input) =>
		TryParseValueExpressionPrimary(input, out var value) ? value : throw Refused(input, "value expression primary");

	public static bool TryParseValueExpressionPrimary(string input, out Expression value) =>
		Tower(input, SqlTowers.Bare | SqlTowers.Parenthesized, out value);

	/// <summary>A tower of the common value expression, which is the shape they all share.</summary>
	static bool Tower(string input, int roles, out Expression value)
	{
		var cursor = new SqlCursor(input);

		return Whole(CommonValueExpressionOrRow(ref cursor, out var read) && (read.Roles & roles) != 0, ref cursor, read, out value);
	}

	static bool Whole(bool read, ref SqlCursor cursor, Typed value, out Expression node)
	{
		if (read && cursor.AtEnd)
		{
			node = value.Node;

			return true;
		}

		node = null!;

		return false;
	}

	// ── §8.1 Boolean value expressions ─────────────────────────────────────────

	/// <summary>
	/// <c>&lt;value expression&gt;</c>. A value that is no boolean is a boolean primary with nothing
	/// around it, so the boolean reading is the one reading.
	/// </summary>
	static bool ValueExpression(ref SqlCursor cursor, out Typed value) => Disjunction(ref cursor, out value);

	/// <summary><c>&lt;boolean value expression&gt;</c>, <c>&lt;search condition&gt;</c>.</summary>
	static bool BooleanValueExpression(ref SqlCursor cursor, out Typed value)
	{
		var save = cursor;

		if (Disjunction(ref cursor, out value) && (value.Roles & SqlTowers.Truth) != 0)
			return true;

		cursor = save;
		value  = default;

		return false;
	}

	/// <summary>The node of a search condition, where a rule keeps the node and asks nothing else.</summary>
	static bool SearchCondition(ref SqlCursor cursor, out Expression value)
	{
		if (BooleanValueExpression(ref cursor, out var read))
		{
			value = read.Node;

			return true;
		}

		value = null!;

		return false;
	}

	/// <summary><c>OR</c> joins booleans; a term with none of them is whatever it is.</summary>
	static bool Disjunction(ref SqlCursor cursor, out Typed value)
	{
		if (!Conjunction(ref cursor, out value))
			return false;

		if (cursor.Word != SqlWord.Or)
			return true;

		var rest = new List<Typed>();

		while (cursor.Take(SqlWord.Or))
		{
			if (!Conjunction(ref cursor, out var next))
				return Refuse(out value);

			rest.Add(next);
		}

		value = SqlTowers.Connected(value, rest, BinaryOperator.Or);

		return value.Roles != 0 || Refuse(out value);
	}

	static bool Conjunction(ref SqlCursor cursor, out Typed value)
	{
		if (!BooleanFactor(ref cursor, out value))
			return false;

		if (cursor.Word != SqlWord.And)
			return true;

		var rest = new List<Typed>();

		while (cursor.Take(SqlWord.And))
		{
			if (!BooleanFactor(ref cursor, out var next))
				return Refuse(out value);

			rest.Add(next);
		}

		value = SqlTowers.Connected(value, rest, BinaryOperator.And);

		return value.Roles != 0 || Refuse(out value);
	}

	static bool BooleanFactor(ref SqlCursor cursor, out Typed value)
	{
		if (cursor.Take(SqlWord.Not))
		{
			if (!BooleanTest(ref cursor, out value) || (value.Roles & SqlTowers.Truth) == 0)
				return Refuse(out value);

			value = new Typed(new Expression.Unary(UnaryOperator.Not, value.Node), SqlTowers.Logical | SqlTowers.Truth);

			return true;
		}

		return BooleanTest(ref cursor, out value);
	}

	/// <summary><c>&lt;boolean test&gt;</c>: <c>IS TRUE</c>, <c>IS NOT UNKNOWN</c>.</summary>
	static bool BooleanTest(ref SqlCursor cursor, out Typed value)
	{
		if (!BooleanPrimary(ref cursor, out value))
			return false;

		while (true)
		{
			var save = cursor;

			if (!cursor.Take(SqlWord.Is))
				return true;

			var not   = cursor.Take(SqlWord.Not);
			var truth =
				cursor.Take(SqlWord.True)    ? BooleanLiteral.True :
				cursor.Take(SqlWord.False)   ? BooleanLiteral.False :
				cursor.Take(SqlWord.Unknown) ? BooleanLiteral.Unknown :
				(BooleanLiteral?)null;

			if (truth is null)
			{
				cursor = save;

				return true;
			}

			if ((value.Roles & SqlTowers.Truth) == 0)
				return Refuse(out value);

			value = new Typed(new Expression.IsTruth(value.Node, not, truth.Value), SqlTowers.Logical | SqlTowers.Truth);
		}
	}

	/// <summary>
	/// <c>&lt;boolean primary&gt;</c>: a predicate, or a boolean predicand. Almost every predicate
	/// begins with a row value predicand, so the predicand is read once and what follows it says
	/// which predicate it is — or that it is none.
	/// </summary>
	static bool BooleanPrimary(ref SqlCursor cursor, out Typed value)
	{
		switch (cursor.Word)
		{
			case SqlWord.Exists:
			{
				var save = cursor;

				cursor.Take();

				if (TableSubquery(ref cursor, out var query))
				{
					value = new Typed(new Expression.Exists(query), SqlTowers.Logical | SqlTowers.Truth);

					return true;
				}

				cursor = save;

				break;
			}

			case SqlWord.JsonExists:
			{
				if (JSONExistsPredicate(ref cursor, out var json))
				{
					value = new Typed(json, SqlTowers.Logical | SqlTowers.Truth);

					return true;
				}

				break;
			}

			case SqlWord.Unique:
			{
				var save = cursor;

				cursor.Take();

				NullDistinctness? nulls = null;

				if (cursor.TakeWord("NULLS"))
				{
					var not = cursor.Take(SqlWord.Not);

					if (!cursor.Take(SqlWord.Distinct))
					{
						cursor = save;

						break;
					}

					nulls = not ? NullDistinctness.NotDistinct : NullDistinctness.Distinct;
				}

				if (TableSubquery(ref cursor, out var query))
				{
					value = new Typed(new Expression.Unique(nulls, query), SqlTowers.Logical | SqlTowers.Truth);

					return true;
				}

				cursor = save;

				break;
			}

			case SqlWord.Period:
			{
				var save = cursor;

				if (PeriodConstructor(ref cursor, out var period) && PeriodPredicatePart2(ref cursor, out var predicate))
				{
					value = new Typed(predicate with { Left = period }, SqlTowers.Logical | SqlTowers.Truth);

					return true;
				}

				cursor = save;

				break;
			}
		}

		if (!RowValuePredicand(ref cursor, out value))
			return false;

		var taken = cursor;

		if (!PredicatePart2(ref cursor, out var part))
			return true;

		if ((value.Roles & part.Roles) == 0)
		{
			// The predicand is not what this predicate takes, and the reading is refused rather
			// than left as a predicand: the BNF's guard is on the pair.
			cursor = taken;

			return Refuse(out value);
		}

		value = new Typed(Predicated(value.Node, part.Node!), SqlTowers.Logical | SqlTowers.Truth);

		return true;
	}

	static bool Refuse(out Typed value)
	{
		value = default;

		return false;
	}

	// ── §8 Predicates ──────────────────────────────────────────────────────────

	/// <summary>
	/// A predicate's second part, which is what the BNF calls it: each says what it takes before it,
	/// and builds the predicate with the left side left empty.
	/// </summary>
	static bool PredicatePart2(ref SqlCursor cursor, out Tail part)
	{
		var save = cursor;

		switch (cursor.Kind)
		{
			case SqlTokenKind.Equal:
			case SqlTokenKind.Less:
			case SqlTokenKind.Greater:
			case SqlTokenKind.LessOrEqual:
			case SqlTokenKind.GreaterOrEqual:
			case SqlTokenKind.NotEqual:
			{
				var op = cursor.Kind switch
				{
					SqlTokenKind.Equal          => ComparisonOperator.Equal,
					SqlTokenKind.Less           => ComparisonOperator.Less,
					SqlTokenKind.Greater        => ComparisonOperator.Greater,
					SqlTokenKind.LessOrEqual    => ComparisonOperator.LessOrEqual,
					SqlTokenKind.GreaterOrEqual => ComparisonOperator.GreaterOrEqual,
					_                           => ComparisonOperator.NotEqual,
				};

				cursor.Take();

				// A quantified comparison is asked after a plain one: `ANY ((SELECT …))` is an
				// aggregate of a scalar subquery too, and read as one it may go on.
				if (RowValuePredicand(ref cursor, out var right))
				{
					part = new Tail(SqlTowers.Any, new Expression.Comparison(null!, op, right.Node));

					return true;
				}

				var quantifier =
					cursor.Take(SqlWord.All)  ? Quantifier.All :
					cursor.Take(SqlWord.Some) ? Quantifier.Some :
					cursor.Take(SqlWord.Any)  ? Quantifier.Any :
					(Quantifier?)null;

				if (quantifier is not null && TableSubquery(ref cursor, out var query))
				{
					part = new Tail(SqlTowers.Any, new Expression.QuantifiedComparison(null!, op, quantifier.Value, query));

					return true;
				}

				break;
			}

			case SqlTokenKind.Word:
				switch (cursor.Word)
				{
					case SqlWord.Match:
					{
						cursor.Take();

						var unique = cursor.Take(SqlWord.Unique);
						var type   =
							cursor.TakeWord("SIMPLE")  ? Ast.MatchType.Simple :
							cursor.TakeWord("PARTIAL") ? Ast.MatchType.Partial :
							cursor.TakeWord("FULL")    ? Ast.MatchType.Full :
							(Ast.MatchType?)null;

						if (TableSubquery(ref cursor, out var query))
						{
							part = new Tail(SqlTowers.Any, new Expression.Match(null!, unique, type, query));

							return true;
						}

						break;
					}

					case SqlWord.Not:
					{
						cursor.Take();

						if (NegatablePredicatePart2(ref cursor, out var negatable))
						{
							part = new Tail(SqlTowers.Any, Negated(negatable, true));

							return true;
						}

						break;
					}

					case SqlWord.Is:
					{
						cursor.Take();

						var not = cursor.Take(SqlWord.Not);

						if (IsPredicatePart2(ref cursor, out var tested))
						{
							part = tested with { Node = Negated(tested.Node!, not) };

							return true;
						}

						break;
					}

					case SqlWord.Overlaps:
					{
						cursor.Take();

						if (RowValuePredicand(ref cursor, out var right))
						{
							part = new Tail(SqlTowers.Any, new Expression.Overlaps(null!, right.Node));

							return true;
						}

						break;
					}

				}

				// `FORMAT JSON IS [NOT] JSON`, the JSON predicate with its input clause: §5.2
				// reserves neither word.
				if (cursor.IsWord("FORMAT") && JSONInputClause(ref cursor, out var input) && cursor.Take(SqlWord.Is))
				{
					var not = cursor.Take(SqlWord.Not);

					if (cursor.Take(SqlWord.Json))
					{
						part = new Tail(
							SqlTowers.String | SqlTowers.Json,
							new Expression.JsonPredicate(null!, input, not, JSONPredicateTypeConstraint(ref cursor), JSONKeyUniquenessConstraint(ref cursor)));

						return true;
					}

					cursor = save;
				}

				// <period predicate part 2>, and the negatable parts a NOT may also stand before.
				if (PeriodPredicatePart2(ref cursor, out var period))
				{
					part = new Tail(SqlTowers.Chain, period);

					return true;
				}

				if (NegatablePredicatePart2(ref cursor, out var written))
				{
					part = new Tail(SqlTowers.Any, written);

					return true;
				}

				break;
		}

		cursor = save;
		part   = default;

		return false;
	}

	/// <summary>What may stand after <c>NOT</c>, and without one.</summary>
	static bool NegatablePredicatePart2(ref SqlCursor cursor, out Expression predicate)
	{
		var save = cursor;

		switch (cursor.Word)
		{
			case SqlWord.Between:
			{
				cursor.Take();

				var symmetry =
					cursor.Take(SqlWord.Asymmetric) ? BetweenSymmetry.Asymmetric :
					cursor.Take(SqlWord.Symmetric)  ? BetweenSymmetry.Symmetric :
					(BetweenSymmetry?)null;

				if (RowValuePredicand(ref cursor, out var low) && cursor.Take(SqlWord.And) && RowValuePredicand(ref cursor, out var high))
				{
					predicate = new Expression.Between(null!, false, symmetry, low.Node, high.Node);

					return true;
				}

				break;
			}

			case SqlWord.In:
			{
				cursor.Take();

				if (TableSubquery(ref cursor, out var query))
				{
					predicate = new Expression.In(null!, false, new InSource.Query(query));

					return true;
				}

				if (cursor.Take(SqlTokenKind.LeftParen))
				{
					var values = new List<Expression>();

					while (true)
					{
						if (!RowValueExpression(ref cursor, out var one))
							break;

						values.Add(one.Node);

						if (!cursor.Take(SqlTokenKind.Comma))
							break;
					}

					if (values.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
					{
						predicate = new Expression.In(null!, false, new InSource.Values(values));

						return true;
					}
				}

				break;
			}

			case SqlWord.Like:
			{
				cursor.Take();

				if (CommonValueExpression(ref cursor, out var pattern) && (pattern.Roles & SqlTowers.String) != 0)
				{
					var escape = new Typed(null!, SqlTowers.String);
					var marked = cursor;

					if (cursor.Take(SqlWord.Escape))
					{
						if (!CommonValueExpression(ref cursor, out escape) || (escape.Roles & SqlTowers.String) == 0)
						{
							cursor = marked;
							escape = new Typed(null!, SqlTowers.String);
						}
					}

					// §8.5: the pattern and the escape are of one kind, character or binary.
					if ((pattern.Roles & escape.Roles & SqlTowers.String) != 0)
					{
						predicate = new Expression.Like(null!, false, LikeKind.Like, pattern.Node, escape.Node);

						return true;
					}
				}

				break;
			}

			case SqlWord.Similar:
			{
				cursor.Take();

				if (cursor.Take(SqlWord.To) && Character(ref cursor, out var pattern))
				{
					Expression? escape = null;
					var marked = cursor;

					if (cursor.Take(SqlWord.Escape) && !Character(ref cursor, out escape))
					{
						cursor = marked;
						escape = null;
					}

					predicate = new Expression.Like(null!, false, LikeKind.Similar, pattern, escape);

					return true;
				}

				break;
			}

			case SqlWord.LikeRegex:
			{
				cursor.Take();

				if (Character(ref cursor, out var pattern))
				{
					Expression? flag = null;
					var marked = cursor;

					if (cursor.TakeWord("FLAG") && !Character(ref cursor, out flag))
					{
						cursor = marked;
						flag   = null;
					}

					predicate = new Expression.Like(null!, false, LikeKind.Regex, pattern, null, flag);

					return true;
				}

				break;
			}

			case SqlWord.Member:
			case SqlWord.Submultiset:
			{
				var member = cursor.Word == SqlWord.Member;

				cursor.Take();

				var of = cursor.Take(SqlWord.Of);

				if (CommonValueExpression(ref cursor, out var collection) && (collection.Roles & SqlTowers.Multiset) != 0)
				{
					predicate = member
						? new Expression.MemberOf(null!, false, of, collection.Node)
						: new Expression.SubmultisetOf(null!, false, of, collection.Node);

					return true;
				}

				break;
			}
		}

		cursor    = save;
		predicate = null!;

		return false;
	}

	/// <summary>What may stand after <c>IS</c> and its <c>NOT</c>.</summary>
	static bool IsPredicatePart2(ref SqlCursor cursor, out Tail part)
	{
		var save = cursor;

		switch (cursor.Word)
		{
			case SqlWord.Null:
				cursor.Take();

				part = new Tail(SqlTowers.Any, new Expression.IsNull(null!, false));

				return true;

			case SqlWord.Distinct:
				cursor.Take();

				if (cursor.Take(SqlWord.From) && RowValuePredicand(ref cursor, out var right))
				{
					part = new Tail(SqlTowers.Any, new Expression.IsDistinct(null!, false, right.Node));

					return true;
				}

				break;

			case SqlWord.Of:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen))
				{
					var types = new List<TypeTest>();

					while (true)
					{
						var only = cursor.Take(SqlWord.Only);

						if (!Names(ref cursor, 3, out var name))
							break;

						types.Add(new TypeTest(name, only));

						if (!cursor.Take(SqlTokenKind.Comma))
							break;
					}

					if (types.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
					{
						part = new Tail(SqlTowers.Any, new Expression.IsOf(null!, false, types));

						return true;
					}
				}

				break;
			}

			case SqlWord.Json:
			{
				cursor.Take();

				part = new Tail(
					SqlTowers.String | SqlTowers.Json,
					new Expression.JsonPredicate(null!, null, false, JSONPredicateTypeConstraint(ref cursor), JSONKeyUniquenessConstraint(ref cursor)));

				return true;
			}

			case SqlWord.Name when cursor.IsWord("A"):
				cursor.Take();

				if (cursor.Take(SqlWord.Set))
				{
					part = new Tail(SqlTowers.Any, new Expression.IsSet(null!, false));

					return true;
				}

				break;

			default:
			{
				// `IS [<normal form>] NORMALIZED`.
				var form = NormalForm(ref cursor);

				if (cursor.TakeWord("NORMALIZED"))
				{
					part = new Tail(SqlTowers.Any, new Expression.IsNormalized(null!, false, form));

					return true;
				}

				break;
			}
		}

		cursor = save;
		part   = default;

		return false;
	}

	static NormalForm? NormalForm(ref SqlCursor cursor) =>
		cursor.TakeWord("NFC")  ? Ast.NormalForm.NFC :
		cursor.TakeWord("NFD")  ? Ast.NormalForm.NFD :
		cursor.TakeWord("NFKC") ? Ast.NormalForm.NFKC :
		cursor.TakeWord("NFKD") ? Ast.NormalForm.NFKD :
		null;

	/// <summary>A predicate's second part with the predicand it follows put in.</summary>
	static Expression Predicated(Expression left, Expression tail) =>
		tail switch
		{
			Expression.Comparison c           => c with { Left = left },
			Expression.QuantifiedComparison q => q with { Left = left },
			Expression.Between b              => b with { Value = left },
			Expression.In i                   => i with { Value = left },
			Expression.Like l                 => l with { Value = left },
			Expression.IsNull n               => n with { Value = left },
			Expression.IsDistinct d           => d with { Left = left },
			Expression.IsNormalized n         => n with { Value = left },
			Expression.MemberOf m             => m with { Value = left },
			Expression.SubmultisetOf s        => s with { Value = left },
			Expression.IsSet s                => s with { Value = left },
			Expression.IsOf o                 => o with { Value = left },
			Expression.Match m                => m with { Value = left },
			Expression.Overlaps o             => o with { Left = left },
			Expression.JsonPredicate j        => j with { Value = left },
			Expression.PeriodPredicate p      => p with { Left = left is Expression.Reference r ? new PeriodValue.Reference(r.Name) : throw new ArgumentOutOfRangeException(nameof(left), left, "A period predicate names its period by a chain.") },
			_                                 => throw new ArgumentOutOfRangeException(nameof(tail), tail, "A predicate this method cannot complete."),
		};

	static Expression Negated(Expression predicate, bool not) =>
		!not ? predicate : predicate switch
		{
			Expression.Between b       => b with { Not = true },
			Expression.In i            => i with { Not = true },
			Expression.Like l          => l with { Not = true },
			Expression.IsNull n        => n with { Not = true },
			Expression.IsDistinct d    => d with { Not = true },
			Expression.IsNormalized n  => n with { Not = true },
			Expression.MemberOf m      => m with { Not = true },
			Expression.SubmultisetOf s => s with { Not = true },
			Expression.IsSet s         => s with { Not = true },
			Expression.IsOf o          => o with { Not = true },
			Expression.JsonPredicate j => j with { Not = true },
			_                          => throw new ArgumentOutOfRangeException(nameof(predicate), predicate, "A predicate that takes no NOT."),
		};

	// ── §8.20 Period predicate ─────────────────────────────────────────────────

	static bool PeriodPredicatePart2(ref SqlCursor cursor, out Expression.PeriodPredicate predicate)
	{
		var save      = cursor;
		var immediate = cursor.TakeWord("IMMEDIATELY");

		var op =
			!immediate && cursor.Take(SqlWord.Overlaps) ? PeriodOperator.Overlaps :
			!immediate && cursor.Take(SqlWord.Equals)   ? PeriodOperator.Equals :
			cursor.Take(SqlWord.Precedes)               ? (immediate ? PeriodOperator.ImmediatelyPrecedes : PeriodOperator.Precedes) :
			cursor.Take(SqlWord.Succeeds)               ? (immediate ? PeriodOperator.ImmediatelySucceeds : PeriodOperator.Succeeds) :
			(PeriodOperator?)null;

		if (op is not null)
		{
			if (PeriodPredicand(ref cursor, out var right))
			{
				predicate = new Expression.PeriodPredicate(null!, op.Value, new PeriodRight.Period(right));

				return true;
			}

			cursor    = save;
			predicate = null!;

			return false;
		}

		if (!immediate && cursor.Take(SqlWord.Contains))
		{
			// `CONTAINS` takes a period or a point in time.
			if (CommonValueExpression(ref cursor, out var point) && (point.Roles & SqlTowers.Datetime) != 0)
			{
				predicate = new Expression.PeriodPredicate(null!, PeriodOperator.Contains, new PeriodRight.Point(point.Node));

				return true;
			}

			cursor = save;

			cursor.Take();

			if (PeriodPredicand(ref cursor, out var period))
			{
				predicate = new Expression.PeriodPredicate(null!, PeriodOperator.Contains, new PeriodRight.Period(period));

				return true;
			}
		}

		cursor    = save;
		predicate = null!;

		return false;
	}

	static bool PeriodPredicand(ref SqlCursor cursor, out PeriodValue period)
	{
		if (PeriodConstructor(ref cursor, out period))
			return true;

		if (IdentifierChain(ref cursor, out var name))
		{
			period = new PeriodValue.Reference(name);

			return true;
		}

		period = null!;

		return false;
	}

	static bool PeriodConstructor(ref SqlCursor cursor, out PeriodValue period)
	{
		var save = cursor;

		period = null!;

		if (!cursor.Take(SqlWord.Period) || !cursor.Take(SqlTokenKind.LeftParen))
		{
			cursor = save;

			return false;
		}

		if (Datetime(ref cursor, out var start) && cursor.Take(SqlTokenKind.Comma) && Datetime(ref cursor, out var end) && cursor.Take(SqlTokenKind.RightParen))
		{
			period = new PeriodValue.Range(start, end);

			return true;
		}

		cursor = save;

		return false;
	}

	// ── §7.1 Row value constructors, as far as a predicate needs them ──────────

	/// <summary>
	/// <c>&lt;row value predicand&gt;</c>: a common value expression — which a boolean predicand in
	/// brackets and a value expression primary already are — or an explicit row.
	/// </summary>
	static bool RowValuePredicand(ref SqlCursor cursor, out Typed value) => CommonValueExpressionOrRow(ref cursor, out value);

	/// <summary>
	/// <c>&lt;row value expression&gt;</c>: a value expression primary not in brackets, or an
	/// explicit row, so <c>x IN (a + 1)</c> is refused as the BNF refuses it.
	/// </summary>
	static bool RowValueExpression(ref SqlCursor cursor, out Typed value)
	{
		var save = cursor;

		if (CommonValueExpressionOrRow(ref cursor, out value) && (value.Roles & (SqlTowers.Row | SqlTowers.Bare)) != 0)
			return true;

		cursor = save;

		return Refuse(out value);
	}

	// ── §6.28 Common value expressions ─────────────────────────────────────────

	/// <summary>
	/// Operands and the operators between them, which <see cref="SqlTowers"/> holds to the towers and
	/// builds the node of. An explicit row is an operand in shape, and something only alone.
	/// </summary>
	static bool CommonValueExpressionOrRow(ref SqlCursor cursor, out Typed value)
	{
		var save = cursor;

		if (!Operand(ref cursor, out var first))
			return Refuse(out value);

		List<Operated>? rest = null;

		while (Operator(ref cursor, out var op))
		{
			if (!Operand(ref cursor, out var next))
			{
				cursor = save;

				return Refuse(out value);
			}

			(rest ??= []).Add(new Operated(op, next));
		}

		if (SqlTowers.Common(first, rest) == 0)
		{
			cursor = save;

			return Refuse(out value);
		}

		value = SqlTowers.Expression(first, rest);

		return true;
	}

	/// <summary>A common value expression that is a value of some tower, and no bare row.</summary>
	static bool CommonValueExpression(ref SqlCursor cursor, out Typed value)
	{
		var save = cursor;

		if (CommonValueExpressionOrRow(ref cursor, out value) && (value.Roles & SqlTowers.Value) != 0)
			return true;

		cursor = save;

		return Refuse(out value);
	}

	/// <summary>The node of a tower, where a rule keeps the node and asks nothing of its towers.</summary>
	static bool Value(ref SqlCursor cursor, out Expression value) => Node(ref cursor, SqlTowers.Value | SqlTowers.Row | SqlTowers.Truth, out value);

	static bool Numeric(ref SqlCursor cursor, out Expression value) => Node(ref cursor, SqlTowers.Numeric, out value);

	static bool Character(ref SqlCursor cursor, out Expression value) => Node(ref cursor, SqlTowers.Character, out value);

	static bool Datetime(ref SqlCursor cursor, out Expression value) => Node(ref cursor, SqlTowers.Datetime, out value);

	/// <summary>
	/// A value expression of one tower, as a node. <c>Value</c> is the whole <c>&lt;value
	/// expression&gt;</c>, which is the boolean reading, since a value that is no boolean is a
	/// boolean primary with nothing around it.
	/// </summary>
	static bool Node(ref SqlCursor cursor, int roles, out Expression value)
	{
		var save = cursor;

		if (roles == (SqlTowers.Value | SqlTowers.Row | SqlTowers.Truth))
		{
			if (ValueExpression(ref cursor, out var whole))
			{
				value = whole.Node;

				return true;
			}

			cursor = save;
			value  = null!;

			return false;
		}

		if (CommonValueExpression(ref cursor, out var read) && (read.Roles & roles) != 0)
		{
			value = read.Node;

			return true;
		}

		cursor = save;
		value  = null!;

		return false;
	}

	/// <summary>The operators the towers share, read once for all of them.</summary>
	static bool Operator(ref SqlCursor cursor, out Op op)
	{
		switch (cursor.Kind)
		{
			case SqlTokenKind.Concat  : cursor.Take(); op = new Op(SqlTowers.Concatenate); return true;
			case SqlTokenKind.Asterisk: cursor.Take(); op = new Op(SqlTowers.Times);       return true;
			case SqlTokenKind.Solidus : cursor.Take(); op = new Op(SqlTowers.Divided);     return true;
			case SqlTokenKind.Plus    : cursor.Take(); op = new Op(SqlTowers.Plus);        return true;
			case SqlTokenKind.Minus   : cursor.Take(); op = new Op(SqlTowers.Minus);       return true;

			case SqlTokenKind.Word when cursor.Word == SqlWord.Multiset:
			{
				var save = cursor;

				cursor.Take();

				var multiset =
					cursor.Take(SqlWord.Union)     ? MultisetOperator.Union :
					cursor.Take(SqlWord.Except)    ? MultisetOperator.Except :
					cursor.Take(SqlWord.Intersect) ? MultisetOperator.Intersect :
					(MultisetOperator?)null;

				if (multiset is not null)
				{
					var quantifier =
						cursor.Take(SqlWord.All)      ? Ast.SetQuantifier.All :
						cursor.Take(SqlWord.Distinct) ? Ast.SetQuantifier.Distinct :
						(Ast.SetQuantifier?)null;

					op = new Op(SqlTowers.MultisetOperator, multiset.Value, quantifier);

					return true;
				}

				cursor = save;

				break;
			}
		}

		op = default;

		return false;
	}

	/// <summary>
	/// What every tower's factor is made of: a sign, a primary, and what may follow one — an interval
	/// qualifier, a time zone, a collate clause, or a type's name.
	/// </summary>
	static bool Operand(ref SqlCursor cursor, out Piece piece)
	{
		var save = cursor;

		UnaryOperator? sign =
			cursor.Take(SqlTokenKind.Plus)  ? UnaryOperator.Plus :
			cursor.Take(SqlTokenKind.Minus) ? UnaryOperator.Minus :
			null;

		if (!Primary(ref cursor, out var primary))
		{
			cursor = save;
			piece  = default;

			return false;
		}

		piece = new Piece(sign, primary, Postfix(ref cursor, primary));

		if (SqlTowers.Roles(piece) != 0)
			return true;

		cursor = save;
		piece  = default;

		return false;
	}

	/// <summary>What may follow a primary, and what it makes of it.</summary>
	static After Postfix(ref SqlCursor cursor, Typed primary)
	{
		var save = cursor;

		// `.SPECIFICTYPE`, which is a character value function and may be collated in turn.
		if (cursor.Take(SqlTokenKind.Dot))
		{
			if (cursor.TakeWord("SPECIFICTYPE"))
			{
				var brackets = false;
				var marked   = cursor;

				if (cursor.Take(SqlTokenKind.LeftParen))
				{
					if (cursor.Take(SqlTokenKind.RightParen))
						brackets = true;
					else
						cursor = marked;
				}

				var specific = new Expression.Member(null!, MemberAccessKind.Dot, new Identifier("SPECIFICTYPE"), brackets ? [] : null);

				if (CollateClause(ref cursor, out var collation))
					return new After(SqlTowers.TypedCollated, new Expression.Collate(specific, collation));

				return new After(SqlTowers.Typed_, specific);
			}

			cursor = save;
		}

		if (IntervalQualifier(ref cursor, out var qualifier))
			return new After(SqlTowers.Qualified, new Expression.IntervalQualified(null!, qualifier));

		if (cursor.Take(SqlWord.At))
		{
			if (cursor.Take(SqlWord.Local))
				return new After(SqlTowers.Zoned, new Expression.AtTimeZone(null!, null));

			if (cursor.Take(SqlWord.Time) && cursor.TakeWord("ZONE") && IntervalPrimary(ref cursor, out var zone))
				return new After(SqlTowers.Zoned, new Expression.AtTimeZone(null!, zone));

			cursor = save;
		}

		if (CollateClause(ref cursor, out var collate))
			return new After(SqlTowers.Collated, new Expression.Collate(null!, collate));

		return default;
	}

	static bool CollateClause(ref SqlCursor cursor, out CollationName collation)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Collate) && Names(ref cursor, 3, out var name))
		{
			collation = new CollationName(name);

			return true;
		}

		cursor    = save;
		collation = null!;

		return false;
	}

	/// <summary>
	/// A time zone's zone: an interval primary, which is a value expression primary with or without a
	/// qualifier.
	/// </summary>
	static bool IntervalPrimary(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		if (!Primary(ref cursor, out var primary))
			return false;

		if (IntervalQualifier(ref cursor, out var qualifier))
		{
			if ((primary.Roles & SqlTowers.Value) != SqlTowers.Value)
			{
				cursor = save;

				return false;
			}

			value = new Expression.IntervalQualified(primary.Node, qualifier);

			return true;
		}

		if ((primary.Roles & SqlTowers.Interval) == 0)
		{
			cursor = save;

			return false;
		}

		value = primary.Node;

		return true;
	}

	// ── §6.3 Value expression primary ──────────────────────────────────────────

	/// <summary>
	/// A primary of any tower: a value function is of the tower it names, and an array's may be
	/// subscripted, which makes it a value expression primary.
	/// </summary>
	static bool Primary(ref SqlCursor cursor, out Typed value)
	{
		var save = cursor;

		if (ValueFunction(ref cursor, out value))
		{
			var subscript = cursor;

			if (ArrayElementStep(ref cursor, out var element))
			{
				if ((value.Roles & SqlTowers.Array) == 0)
				{
					cursor = save;

					return Refuse(out value);
				}

				value = SqlTowers.Stepped(value, SqlTowers.Subscripted(element, PrimarySteps(ref cursor).Steps));

				return true;
			}

			cursor = subscript;

			return true;
		}

		return PrimaryReading(ref cursor, out value);
	}

	/// <summary>
	/// A primary read as one, with the steps after it: a field reference, a method invocation and an
	/// array element reference each begin with a value expression primary, which is left recursion
	/// through other productions, and they are steps here.
	/// </summary>
	static bool PrimaryReading(ref SqlCursor cursor, out Typed value)
	{
		var save = cursor;

		// A scalar subquery is asked before a bracket's value expression: `((SELECT a FROM t))` is
		// both, and only the subquery is a non-parenthesized primary.
		if (ScalarSubquery(ref cursor, out var query))
		{
			value = SqlTowers.Stepped(new Typed(new Expression.Subquery(query), SqlTowers.Value | SqlTowers.Truth | SqlTowers.Bare), PrimarySteps(ref cursor));

			return true;
		}

		if (cursor.Kind == SqlTokenKind.LeftParen)
		{
			if (!Bracketed(ref cursor, out value))
				return Refuse(out value);
		}
		else if (!PrimaryBase(ref cursor, out value))
		{
			return Refuse(out value);
		}

		var steps = PrimarySteps(ref cursor);

		// A row takes no step.
		if (steps.Roles != 0 && (value.Roles & SqlTowers.Row) != 0)
		{
			cursor = save;

			return Refuse(out value);
		}

		value = SqlTowers.Stepped(value, steps);

		return true;
	}

	/// <summary>
	/// A bracket, read once: a parenthesized value expression, an explicit row and a generalized
	/// invocation each begin with a bracket and a value expression, and what follows it says which.
	/// </summary>
	static bool Bracketed(ref SqlCursor cursor, out Typed value)
	{
		var save = cursor;

		value = default;

		if (!cursor.Take(SqlTokenKind.LeftParen) || !ValueExpression(ref cursor, out var inner))
		{
			cursor = save;

			return false;
		}

		if (!BracketTail(ref cursor, out var tail))
		{
			cursor = save;

			return false;
		}

		if (tail.Kind == SqlTowers.Bare && (inner.Roles & (SqlTowers.Bare | SqlTowers.Parenthesized)) == 0)
		{
			cursor = save;

			return false;
		}

		value = tail.Kind switch
		{
			SqlTowers.Parenthesized => new Typed(new Expression.Parenthesized(inner.Node), SqlTowers.Value | SqlTowers.Parenthesized | (inner.Roles & SqlTowers.Truth)),
			SqlTowers.Row           => new Typed(new Expression.Row(Items(inner.Node, tail.Rest)), SqlTowers.Row),
			_                       => new Typed(new Expression.Member(new Expression.Generalized(inner.Node, tail.Type!), MemberAccessKind.Dot, tail.Method!, tail.Arguments), SqlTowers.Value | SqlTowers.Truth | SqlTowers.Bare),
		};

		return true;
	}

	static bool BracketTail(ref SqlCursor cursor, out Bracket tail)
	{
		var save = cursor;

		if (cursor.Take(SqlTokenKind.RightParen))
		{
			tail = new Bracket(SqlTowers.Parenthesized);

			return true;
		}

		if (cursor.Kind == SqlTokenKind.Comma)
		{
			var rest = new List<Expression>();

			while (cursor.Take(SqlTokenKind.Comma))
			{
				if (!Value(ref cursor, out var one))
				{
					cursor = save;
					tail   = default;

					return false;
				}

				rest.Add(one);
			}

			if (cursor.Take(SqlTokenKind.RightParen))
			{
				tail = new Bracket(SqlTowers.Row, rest);

				return true;
			}

			cursor = save;
			tail   = default;

			return false;
		}

		// `(a AS t).m ()`, a generalized invocation.
		if (cursor.Take(SqlWord.As) && DataType(ref cursor, out var type) && cursor.Take(SqlTokenKind.RightParen) &&
			cursor.Take(SqlTokenKind.Dot) && Identifier(ref cursor, out var method))
		{
			var arguments = SQLArgumentList(ref cursor);

			tail = new Bracket(SqlTowers.Bare, null, type, method, arguments);

			return true;
		}

		cursor = save;
		tail   = default;

		return false;
	}

	static IReadOnlyList<Expression> Items(Expression first, List<Expression>? rest)
	{
		var items = new List<Expression>(1 + (rest?.Count ?? 0)) { first };

		if (rest is not null)
			items.AddRange(rest);

		return items;
	}

	/// <summary>The steps after a primary, as many as there are.</summary>
	static Stepping PrimarySteps(ref SqlCursor cursor)
	{
		List<Step>? steps = null;

		while (PrimaryStep(ref cursor, out var step))
			(steps ??= []).Add(step);

		return new Stepping(SqlTowers.Steps(steps), steps);
	}

	/// <summary>
	/// A step after a primary — <c>.name</c>, <c>-&gt;name</c>, <c>[i]</c>, <c>.*</c> — with the
	/// operations of a JSON simplified accessor among them.
	/// </summary>
	static bool PrimaryStep(ref SqlCursor cursor, out Step step)
	{
		var save = cursor;

		switch (cursor.Kind)
		{
			case SqlTokenKind.Dot:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.Asterisk))
				{
					step = new Step(SqlTowers.Bare | SqlTowers.Starred, new Expression.Wildcard(null!, WildcardKind.Member));

					return true;
				}

				// A JSON item method is asked before a method: its names are mostly reserved words
				// and its arguments are fixed.
				if (JSONMethod(ref cursor, out var method))
				{
					step = new Step(SqlTowers.Bare, method);

					return true;
				}

				if (Identifier(ref cursor, out var name))
				{
					step = new Step(SqlTowers.Bare, new Expression.Member(null!, MemberAccessKind.Dot, name, SQLArgumentList(ref cursor)));

					return true;
				}

				break;
			}

			case SqlTokenKind.Arrow:
			{
				cursor.Take();

				if (Identifier(ref cursor, out var name))
				{
					step = new Step(SqlTowers.Bare, new Expression.Member(null!, MemberAccessKind.Dereference, name, SQLArgumentList(ref cursor)));

					return true;
				}

				break;
			}

			case SqlTokenKind.LeftBracket:
			case SqlTokenKind.LeftTrigraph:
			{
				if (ArrayElementStep(ref cursor, out step))
					return true;

				// `[*]` and the subscripts of a JSON simplified accessor.
				if (cursor.Take(SqlTokenKind.LeftBracket))
				{
					if (cursor.Take(SqlTokenKind.Asterisk) && cursor.Take(SqlTokenKind.RightBracket))
					{
						step = new Step(SqlTowers.Bare, new Expression.Wildcard(null!, WildcardKind.Array));

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
						step = new Step(SqlTowers.Bare, new Expression.JsonAccessor(null!, new JsonPathAccessor.Array(subscripts)));

						return true;
					}
				}

				break;
			}
		}

		cursor = save;
		step   = default;

		return false;
	}

	/// <summary><c>&lt;array element reference&gt;</c>: a subscript in brackets or trigraph brackets.</summary>
	static bool ArrayElementStep(ref SqlCursor cursor, out Step step)
	{
		var save = cursor;

		step = default;

		if (cursor.Kind is not (SqlTokenKind.LeftBracket or SqlTokenKind.LeftTrigraph))
			return false;

		var trigraphs = cursor.Kind == SqlTokenKind.LeftTrigraph;

		cursor.Take();

		if (Numeric(ref cursor, out var index) && cursor.Kind is SqlTokenKind.RightBracket or SqlTokenKind.RightTrigraph)
		{
			cursor.Take();

			step = new Step(SqlTowers.Subscript, new Expression.Element(null!, index, null, trigraphs));

			return true;
		}

		cursor = save;

		return false;
	}

	// ── §6.3 What a non-parenthesized primary begins with ──────────────────────

	/// <summary>
	/// A primary that is no bracket. The BNF writes twenty alternatives and tries them in order; a
	/// key word says which of them it is, and only where several begin alike is one tried after
	/// another.
	/// </summary>
	static bool PrimaryBase(ref SqlCursor cursor, out Typed value)
	{
		var save = cursor;

		switch (cursor.Kind)
		{
			case SqlTokenKind.Colon:
			case SqlTokenKind.Question:
				if (GeneralValueSpecification(ref cursor, out var parameter))
					return Bare(parameter, out value);

				break;

			case SqlTokenKind.Number:
			case SqlTokenKind.String:
			case SqlTokenKind.National:
			case SqlTokenKind.UnicodeString:
			case SqlTokenKind.Binary:
				if (UnsignedLiteral(ref cursor, out var literal))
					return Bare(new Expression.Literal(literal), out value);

				break;

			// A name is a word, a delimited identifier or a Unicode delimited one, and everything
			// below reads a name where it reads no key word.
			case SqlTokenKind.Delimited:
			case SqlTokenKind.UnicodeName:
			case SqlTokenKind.Word:
				// A literal is asked first, as the BNF asks it: `DATE '2020-01-01'`, `INTERVAL '1'
				// DAY` and the truth values are words, and no name is any of them.
				if (UnsignedLiteral(ref cursor, out var written))
					return Bare(new Expression.Literal(written), out value);

				switch (cursor.Word)
				{
					case SqlWord.Row:
					{
						// `ROW (a, b)`, an explicit row value constructor.
						cursor.Take();

						if (cursor.Take(SqlTokenKind.LeftParen))
						{
							var items = new List<Expression>();

							while (true)
							{
								if (!Value(ref cursor, out var one))
									break;

								items.Add(one);

								if (!cursor.Take(SqlTokenKind.Comma))
									break;
							}

							if (items.Count > 0 && cursor.Take(SqlTokenKind.RightParen))
							{
								value = new Typed(new Expression.Row(items, true), SqlTowers.Row);

								return true;
							}
						}

						cursor = save;

						break;
					}

					case SqlWord.Case:
					case SqlWord.Nullif:
					case SqlWord.Coalesce:
						if (CaseExpression(ref cursor, out var caseValue))
							return Bare(caseValue, out value);

						break;

					case SqlWord.Cast:
						if (CastSpecification(ref cursor, out var cast))
							return Bare(cast, out value);

						break;

					case SqlWord.Treat:
						if (SubtypeTreatment(ref cursor, out var treat))
							return Bare(treat, out value);

						break;

					case SqlWord.New:
						if (NewSpecification(ref cursor, out var made))
							return Bare(made, out value);

						break;

					case SqlWord.Deref:
						if (ReferenceResolution(ref cursor, out var dereference))
							return Bare(dereference, out value);

						break;

					case SqlWord.Element:
						if (MultisetElementReference(ref cursor, out var element))
							return Bare(element, out value);

						break;

					case SqlWord.Greatest:
					case SqlWord.Least:
						if (GreatestOrLeastFunction(ref cursor, out var extreme))
							return Bare(extreme, out value);

						break;

					case SqlWord.Array:
					case SqlWord.Multiset:
					case SqlWord.Table:
						if (CollectionValueConstructor(ref cursor, out var collection))
							return Bare(collection, out value);

						break;

					case SqlWord.Module:
					{
						// `MODULE.q.c`, a column of a module's table.
						var word = cursor.TextOf(cursor.Token);

						cursor.Take();

						if (cursor.Take(SqlTokenKind.Dot) && Identifier(ref cursor, out var qualified) &&
							cursor.Take(SqlTokenKind.Dot) && Identifier(ref cursor, out var column))
							return Bare(new Expression.Reference(new QualifiedName([new Identifier(word), qualified, column])), out value);

						cursor = save;

						break;
					}
				}

				// `NEXT VALUE FOR s`, whose first word §5.2 does not reserve.
				if (cursor.IsWord("NEXT") && NextValueExpression(ref cursor, out var next))
					return Bare(next, out value);

				// The words a value specification, a window function, a JSON function or a row
				// pattern navigation begins with, and the names everything else begins with.
				if (WindowedFunction(ref cursor, out var windowed))
					return Bare(windowed, out value);

				if (JSONValueConstructorOrQuery(ref cursor, out var json))
					return Bare(json, out value);

				if (RowPatternNavigationOperation(ref cursor, out var navigation))
					return Bare(navigation, out value);

				if (GeneralValueSpecification(ref cursor, out var specification))
					return Bare(specification, out value);

				if (StaticMethodInvocation(ref cursor, out var method))
					return Bare(method, out value);

				if (RoutineInvocation(ref cursor, out var invocation))
				{
					value = new Typed(invocation, SqlTowers.Value | SqlTowers.Truth | SqlTowers.Bare | SqlTowers.Invoked);

					return true;
				}

				// A name, and what follows it: the rest of an identifier chain, or `OVER` and a
				// window, which makes the name a row pattern measure's.
				if (Identifier(ref cursor, out var name))
				{
					var over = cursor;

					if (cursor.Take(SqlWord.Over) && WindowNameOrSpecification(ref cursor, out var window))
					{
						value = new Typed(
							new Expression.Invocation(new QualifiedName([name]), []) { WithoutParentheses = true, Over = window },
							SqlTowers.Value | SqlTowers.Truth | SqlTowers.Bare);

						return true;
					}

					cursor = over;

					// A name followed by a method call stops before the method: `a.b.c.d(1)` is
					// `a.b.c` and a method `d`.
					List<Identifier>? rest = null;

					while (true)
					{
						var dotted = cursor;

						if (!cursor.Take(SqlTokenKind.Dot) || !Identifier(ref cursor, out var part) || cursor.Kind == SqlTokenKind.LeftParen)
						{
							cursor = dotted;

							break;
						}

						(rest ??= []).Add(part);
					}

					value = new Typed(new Expression.Reference(Chain(name, rest)), SqlTowers.Value | SqlTowers.Truth | SqlTowers.Bare | SqlTowers.Chain);

					return true;
				}

				break;
		}

		cursor = save;

		return Refuse(out value);
	}

	/// <summary>A non-parenthesized value expression primary, which every tower has.</summary>
	static bool Bare(Expression node, out Typed value)
	{
		value = new Typed(node, SqlTowers.Value | SqlTowers.Truth | SqlTowers.Bare);

		return true;
	}

	// ── §6.4 General value specification ───────────────────────────────────────

	static bool GeneralValueSpecification(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		switch (cursor.Kind)
		{
			case SqlTokenKind.Colon:
			{
				cursor.Take();

				if (Identifier(ref cursor, out var name))
				{
					Identifier? indicator = null;
					var keyword = false;
					var marked  = cursor;

					if (cursor.Take(SqlWord.Indicator))
					{
						keyword = true;

						if (!cursor.Take(SqlTokenKind.Colon) || !Identifier(ref cursor, out indicator))
						{
							cursor    = marked;
							keyword   = false;
							indicator = null;
						}
					}
					else if (cursor.Take(SqlTokenKind.Colon))
					{
						if (!Identifier(ref cursor, out indicator))
						{
							cursor    = marked;
							indicator = null;
						}
					}

					value = new Expression.Parameter(ParameterKind.Host, name) { Indicator = indicator, IndicatorKeyword = indicator is not null && keyword };

					return true;
				}

				break;
			}

			case SqlTokenKind.Question:
				cursor.Take();

				value = new Expression.Parameter(ParameterKind.Dynamic);

				return true;

			case SqlTokenKind.Word:
				switch (cursor.Word)
				{
					case SqlWord.CurrentTransformGroupForType:
						cursor.Take();

						if (Names(ref cursor, 3, out var type))
						{
							value = new Expression.Current(CurrentValue.TransformGroupForType, type);

							return true;
						}

						break;

					case SqlWord.CurrentCatalog:              return Current(ref cursor, CurrentValue.Catalog, out value);
					case SqlWord.CurrentDefaultTransformGroup: return Current(ref cursor, CurrentValue.DefaultTransformGroup, out value);
					case SqlWord.CurrentPath:                 return Current(ref cursor, CurrentValue.Path, out value);
					case SqlWord.CurrentRole:                 return Current(ref cursor, CurrentValue.Role, out value);
					case SqlWord.CurrentSchema:               return Current(ref cursor, CurrentValue.Schema, out value);
					case SqlWord.CurrentUser:                 return Current(ref cursor, CurrentValue.CurrentUser, out value);
					case SqlWord.SessionUser:                 return Current(ref cursor, CurrentValue.SessionUser, out value);
					case SqlWord.SystemUser:                  return Current(ref cursor, CurrentValue.SystemUser, out value);
					case SqlWord.User:                        return Current(ref cursor, CurrentValue.User, out value);
					case SqlWord.Value:                       return Current(ref cursor, CurrentValue.Value, out value);

				}

				// `COLLATION FOR (v)`, whose words §5.2 does not reserve.
				if (cursor.IsWord("COLLATION"))
				{
					cursor.Take();

					if (cursor.TakeWord("FOR") && cursor.Take(SqlTokenKind.LeftParen) &&
						Node(ref cursor, SqlTowers.String, out var text) && cursor.Take(SqlTokenKind.RightParen))
					{
						value = new Expression.CollationFor(text);

						return true;
					}
				}

				break;
		}

		cursor = save;
		value  = null!;

		return false;
	}

	static bool Current(ref SqlCursor cursor, CurrentValue kind, out Expression value)
	{
		cursor.Take();

		value = new Expression.Current(kind);

		return true;
	}

	// ── §6.12 Case expression, §6.13 Cast specification ────────────────────────

	static bool CaseExpression(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		switch (cursor.Word)
		{
			case SqlWord.Nullif:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var first) && cursor.Take(SqlTokenKind.Comma) &&
					Value(ref cursor, out var second) && cursor.Take(SqlTokenKind.RightParen))
				{
					value = Called("NULLIF", first, second);

					return true;
				}

				break;
			}

			case SqlWord.Coalesce:
			{
				cursor.Take();

				if (cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var first))
				{
					var arguments = new List<Argument> { new(first) };

					while (cursor.Take(SqlTokenKind.Comma))
					{
						if (!Value(ref cursor, out var next))
						{
							cursor = save;
							value  = null!;

							return false;
						}

						arguments.Add(new Argument(next));
					}

					if (arguments.Count > 1 && cursor.Take(SqlTokenKind.RightParen))
					{
						value = new Expression.Invocation(new QualifiedName([new Identifier("COALESCE")]), arguments);

						return true;
					}
				}

				break;
			}

			case SqlWord.Case:
			{
				cursor.Take();

				// A simple case has an operand; a searched one goes straight to `WHEN`.
				Expression? operand = null;

				if (cursor.Word != SqlWord.When && RowValuePredicand(ref cursor, out var read))
					operand = read.Node;

				var whens = new List<CaseWhen>();

				while (cursor.Take(SqlWord.When))
				{
					if (operand is null)
					{
						if (!SearchCondition(ref cursor, out var condition) || !cursor.Take(SqlWord.Then) || !Result(ref cursor, out var result))
						{
							cursor = save;

							return false;
						}

						whens.Add(new CaseWhen([condition], result));
					}
					else
					{
						var operands = new List<Expression>();

						while (true)
						{
							if (!WhenOperand(ref cursor, out var one))
							{
								cursor = save;

								return false;
							}

							operands.Add(one);

							if (!cursor.Take(SqlTokenKind.Comma))
								break;
						}

						if (!cursor.Take(SqlWord.Then) || !Result(ref cursor, out var result))
						{
							cursor = save;

							return false;
						}

						whens.Add(new CaseWhen(operands, result));
					}
				}

				if (whens.Count == 0)
					break;

				Expression? otherwise = null;

				if (cursor.Take(SqlWord.Else) && !Result(ref cursor, out otherwise))
				{
					cursor = save;

					return false;
				}

				if (cursor.Take(SqlWord.End))
				{
					value = new Expression.Case(operand, whens, otherwise);

					return true;
				}

				break;
			}
		}

		cursor = save;
		value  = null!;

		return false;
	}

	/// <summary>
	/// <c>&lt;when operand&gt;</c>: a row value predicand, or a predicate's second part with nothing
	/// before it — <c>WHEN &lt; 5</c> — whose missing left side is the case operand.
	/// </summary>
	static bool WhenOperand(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		if (PredicatePart2(ref cursor, out var part) && part.Roles == SqlTowers.Any)
		{
			value = Predicated(new Expression.CaseOperand(), part.Node!);

			return true;
		}

		cursor = save;

		if (RowValuePredicand(ref cursor, out var read))
		{
			value = read.Node;

			return true;
		}

		value = null!;

		return false;
	}

	static bool Result(ref SqlCursor cursor, out Expression value)
	{
		if (Value(ref cursor, out value))
			return true;

		if (cursor.Take(SqlWord.Null))
		{
			value = new Expression.Literal(new LiteralValue.Null());

			return true;
		}

		return false;
	}

	static bool CastSpecification(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		value = null!;

		cursor.Take();

		if (!cursor.Take(SqlTokenKind.LeftParen) || !CastOperand(ref cursor, out var operand) || !cursor.Take(SqlWord.As) || !DataType(ref cursor, out var type))
		{
			cursor = save;

			return false;
		}

		string? format = null;
		var marked = cursor;

		if (cursor.TakeWord("FORMAT"))
		{
			if (cursor.Kind == SqlTokenKind.String)
			{
				format = cursor.TextOf(cursor.Token);

				cursor.Take();
			}
			else
			{
				cursor = marked;
			}
		}

		if (cursor.Take(SqlTokenKind.RightParen))
		{
			value = new Expression.Cast(operand, type, format);

			return true;
		}

		cursor = save;

		return false;
	}

	static bool CastOperand(ref SqlCursor cursor, out Expression value) =>
		Value(ref cursor, out value) || ImplicitlyTypedValueSpecification(ref cursor, out value);

	static bool ImplicitlyTypedValueSpecification(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		if (cursor.Take(SqlWord.Null))
		{
			value = new Expression.Literal(new LiteralValue.Null());

			return true;
		}

		if (cursor.Word is SqlWord.Array or SqlWord.Multiset)
		{
			var array = cursor.Word == SqlWord.Array;

			cursor.Take();

			if (cursor.Kind is SqlTokenKind.LeftBracket or SqlTokenKind.LeftTrigraph)
			{
				var trigraphs = cursor.Kind == SqlTokenKind.LeftTrigraph;

				cursor.Take();

				if (cursor.Kind is SqlTokenKind.RightBracket or SqlTokenKind.RightTrigraph)
				{
					cursor.Take();

					value = array ? new Expression.Array([], trigraphs) : new Expression.Multiset([], trigraphs);

					return true;
				}
			}
		}

		cursor = save;
		value  = null!;

		return false;
	}

	static bool ContextuallyTypedValueSpecification(ref SqlCursor cursor, out Expression value)
	{
		if (ImplicitlyTypedValueSpecification(ref cursor, out value))
			return true;

		if (cursor.Take(SqlWord.Default))
		{
			value = new Expression.Default();

			return true;
		}

		return false;
	}

	// ── §6.14–6.23 The primaries the BNF spells out ────────────────────────────

	static bool NextValueExpression(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		cursor.Take();

		if (cursor.Take(SqlWord.Value) && cursor.Take(SqlWord.For) && Names(ref cursor, 3, out var sequence))
		{
			value = new Expression.NextValue(sequence);

			return true;
		}

		cursor = save;
		value  = null!;

		return false;
	}

	static bool SubtypeTreatment(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		cursor.Take();

		if (cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var operand) && cursor.Take(SqlWord.As) && TargetSubtype(ref cursor, out var type) &&
			cursor.Take(SqlTokenKind.RightParen))
		{
			value = new Expression.Treat(operand, type);

			return true;
		}

		cursor = save;
		value  = null!;

		return false;
	}

	static bool TargetSubtype(ref SqlCursor cursor, out DataType type)
	{
		if (cursor.Word == SqlWord.Ref)
			return ReferenceType(ref cursor, out type);

		if (Names(ref cursor, 3, out var name))
		{
			type = new DataType.UserDefined(name);

			return true;
		}

		type = null!;

		return false;
	}

	static bool NewSpecification(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		cursor.Take();

		if (Names(ref cursor, 3, out var name) && SQLArgumentList(ref cursor) is { } arguments)
		{
			value = new Expression.New(name, arguments);

			return true;
		}

		cursor = save;
		value  = null!;

		return false;
	}

	static bool ReferenceResolution(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		cursor.Take();

		if (cursor.Take(SqlTokenKind.LeftParen) && CommonValueExpressionOrRow(ref cursor, out var primary) &&
			(primary.Roles & (SqlTowers.Bare | SqlTowers.Parenthesized)) != 0 && cursor.Take(SqlTokenKind.RightParen))
		{
			value = new Expression.Dereference(primary.Node);

			return true;
		}

		cursor = save;
		value  = null!;

		return false;
	}

	static bool MultisetElementReference(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		cursor.Take();

		if (cursor.Take(SqlTokenKind.LeftParen) && Node(ref cursor, SqlTowers.Multiset, out var multiset) && cursor.Take(SqlTokenKind.RightParen))
		{
			value = Called("ELEMENT", multiset);

			return true;
		}

		cursor = save;
		value  = null!;

		return false;
	}

	static bool GreatestOrLeastFunction(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;
		var word = cursor.Word == SqlWord.Greatest ? "GREATEST" : "LEAST";

		cursor.Take();

		if (cursor.Take(SqlTokenKind.LeftParen) && Value(ref cursor, out var first))
		{
			var arguments = new List<Argument> { new(first) };

			while (cursor.Take(SqlTokenKind.Comma))
			{
				if (!Value(ref cursor, out var next))
					break;

				arguments.Add(new Argument(next));
			}

			if (arguments.Count > 1 && cursor.Take(SqlTokenKind.RightParen))
			{
				value = new Expression.Invocation(new QualifiedName([new Identifier(word)]), arguments);

				return true;
			}
		}

		cursor = save;
		value  = null!;

		return false;
	}

	static bool CollectionValueConstructor(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;
		var word = cursor.Word;

		cursor.Take();

		// By enumeration: `ARRAY[1, 2]`, `MULTISET[1]`.
		if (word != SqlWord.Table && cursor.Kind is SqlTokenKind.LeftBracket or SqlTokenKind.LeftTrigraph)
		{
			var trigraphs = cursor.Kind == SqlTokenKind.LeftTrigraph;

			cursor.Take();

			var items = new List<Expression>();

			while (true)
			{
				if (!Value(ref cursor, out var one))
					break;

				items.Add(one);

				if (!cursor.Take(SqlTokenKind.Comma))
					break;
			}

			if (items.Count > 0 && cursor.Kind is SqlTokenKind.RightBracket or SqlTokenKind.RightTrigraph)
			{
				cursor.Take();

				value = word == SqlWord.Array ? new Expression.Array(items, trigraphs) : new Expression.Multiset(items, trigraphs);

				return true;
			}

			cursor = save;
			cursor.Take();
		}

		// By query: `ARRAY (SELECT …)`, `TABLE (SELECT …)`.
		if (TableSubquery(ref cursor, out var query))
		{
			value = new Expression.CollectionQuery(
				word switch
				{
					SqlWord.Array    => CollectionKind.Array,
					SqlWord.Multiset => CollectionKind.Multiset,
					_                => CollectionKind.Table,
				},
				query);

			return true;
		}

		cursor = save;
		value  = null!;

		return false;
	}

	static bool StaticMethodInvocation(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		if (Names(ref cursor, 3, out var type) && cursor.Take(SqlTokenKind.DoubleColon) && Identifier(ref cursor, out var method))
		{
			value = new Expression.Member(new Expression.Reference(type), MemberAccessKind.StaticMethod, method, SQLArgumentList(ref cursor));

			return true;
		}

		cursor = save;
		value  = null!;

		return false;
	}

	static bool RoutineInvocation(ref SqlCursor cursor, out Expression value)
	{
		var save = cursor;

		if (Names(ref cursor, 3, out var name) && cursor.Kind == SqlTokenKind.LeftParen && SQLArgumentList(ref cursor) is { } arguments)
		{
			value = new Expression.Invocation(name, arguments);

			return true;
		}

		cursor = save;
		value  = null!;

		return false;
	}

	// ── §6.5 SQL argument list ─────────────────────────────────────────────────

	/// <summary>An argument list where one is written, and null where none is.</summary>
	static IReadOnlyList<Argument>? SQLArgumentList(ref SqlCursor cursor)
	{
		var save = cursor;

		if (!cursor.Take(SqlTokenKind.LeftParen))
			return null;

		var arguments = new List<Argument>();

		if (cursor.Take(SqlTokenKind.RightParen))
			return arguments;

		while (true)
		{
			if (!SQLArgument(ref cursor, out var argument))
			{
				cursor = save;

				return null;
			}

			arguments.Add(argument);

			if (!cursor.Take(SqlTokenKind.Comma))
				break;
		}

		if (cursor.Take(SqlTokenKind.RightParen))
			return arguments;

		cursor = save;

		return null;
	}

	static bool SQLArgument(ref SqlCursor cursor, out Argument argument)
	{
		var save = cursor;

		// A named argument is asked first, since its name would otherwise be read as a value.
		if (Identifier(ref cursor, out var name) && cursor.Take(SqlTokenKind.DoubleArrow))
		{
			if (ArgumentValue(ref cursor, out var named))
			{
				argument = new Argument(named, name, true);

				return true;
			}

			cursor = save;
			argument = default!;

			return false;
		}

		cursor = save;

		if (Value(ref cursor, out var value))
		{
			var typed = cursor;

			if (cursor.Take(SqlWord.As))
			{
				if (Names(ref cursor, 3, out var type))
				{
					argument = new Argument(new Expression.Generalized(value, new DataType.UserDefined(type)));

					return true;
				}

				cursor = typed;
			}

			argument = new Argument(value);

			return true;
		}

		if (ContextuallyTypedValueSpecification(ref cursor, out var contextual))
		{
			argument = new Argument(contextual);

			return true;
		}

		cursor   = save;
		argument = default!;

		return false;
	}

	static bool ArgumentValue(ref SqlCursor cursor, out Expression value) =>
		Value(ref cursor, out value) || ContextuallyTypedValueSpecification(ref cursor, out value);

	/// <summary>A function the BNF spells out whose arguments are a comma list.</summary>
	static Expression.Invocation Called(string word, params Expression?[] arguments)
	{
		var written = new List<Argument>();

		foreach (var argument in arguments)
			if (argument is not null)
				written.Add(new Argument(argument));

		return new Expression.Invocation(new QualifiedName([new Identifier(word)]), written);
	}
}
