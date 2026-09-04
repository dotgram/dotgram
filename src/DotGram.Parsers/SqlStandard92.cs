using System;

using DotGram;

namespace DotGram.Parsers;

/// <summary>
/// The expression layer of standard SQL — <c>&lt;value expression&gt;</c> and
/// <c>&lt;search condition&gt;</c> — as ISO/IEC 9075:1992 defines them.
/// </summary>
/// <remarks>
/// <para>
/// The bottom of the language, and deliberately only the bottom. SQL divides into
/// expressions, clauses and statements, and the two above cannot be written honestly
/// without the one below: a <c>SELECT</c> is mostly places where an expression stands.
/// So this is the expression layer, finished and testable on its own, and the query
/// level goes above it later.
/// </para>
/// <para>
/// <b>The rule names are the standard's, production for production.</b>
/// <c>SearchCondition</c>, <c>BooleanTerm</c>, <c>BooleanFactor</c>, <c>BooleanTest</c>,
/// <c>BooleanPrimary</c>, <c>Predicate</c>, <c>RowValueConstructor</c>,
/// <c>ValueExpression</c>, <c>Term</c>, <c>Factor</c>, <c>ValueExpressionPrimary</c> —
/// so that a reader with the standard open can follow one against the other, and so that
/// a disagreement about what something should do is settled by a section number rather
/// than by taste. Where a name would have come from an implementation's object model
/// instead, it did not.
/// </para>
/// <para>
/// The standard's edition is 1992, which is the compact core the later ones extend, and
/// what is taken from later editions is marked where it is taken.
/// </para>
/// <para>
/// <b>One divergence, and it is written where it happens.</b> §6.11 gives four value
/// towers — numeric, string, datetime, interval — that share a bottom, so
/// <c>a + b</c> belongs to two of them at once and only the types of <c>a</c> and
/// <c>b</c> say which. That is not a defect in the standard: §6.11 describes syntax
/// modulo type resolution, and a parser has no types. The four are one untyped ladder
/// here, which is what every implementation does.
/// </para>
/// <para>
/// Nothing is built yet. The grammar recognizes and the tree comes later, deliberately:
/// the shape of the node classes is a decision of its own, and getting the language
/// right first is what makes that decision about the tree rather than about the parse.
/// </para>
/// </remarks>
[Gram("SqlStandard92.gram", Lexical = true)]
public static partial class SqlStandard92
{
	/// <summary>What a call with no arguments is handed, once rather than per call.</summary>
	static readonly SqlNode[] None = [];

	/// <summary>The two words that stand where a value does and are always the same node.</summary>
	static readonly SqlNode NullValue    = new SqlLiteral(SqlLiteralKind.Null,    "NULL");
	static readonly SqlNode DefaultValue = new SqlLiteral(SqlLiteralKind.Default, "DEFAULT");

	/// <summary>A head and a tail as one array, which is what a separated list comes to.</summary>
	static SqlNode[] Listed(SqlNode first, SqlNode[]? rest)
	{
		if (rest is null || rest.Length == 0)
			return [first];

		var all = new SqlNode[rest.Length + 1];

		all[0] = first;
		rest.CopyTo(all, 1);

		return all;
	}

	/// <summary>
	/// The left operand written into the tail the predicate was read as.
	/// </summary>
	/// <remarks>
	/// The row is read once for all nine predicates (§8.1's note above), so the tail is
	/// built without it and the slot it left is filled here. Written into the array rather
	/// than copied onto a new record: the array is this predicate's own and nothing has
	/// seen it yet.
	/// </remarks>
	static SqlPredicate Predicated(SqlNode left, SqlPredicate tail)
	{
		tail.Operands[0] = left;

		return tail;
	}

	static SqlOperator Additive(string operatorText) => operatorText switch
	{
		"+" => SqlOperator.Add,
		"-" => SqlOperator.Subtract,
		_   => SqlOperator.Concatenate,
	};

	static SqlOperator Multiplicative(string operatorText) =>
		operatorText == "*" ? SqlOperator.Multiply : SqlOperator.Divide;

	static SqlOperator Signed(string sign) =>
		sign == "-" ? SqlOperator.Negate : SqlOperator.Identity;

	static SqlOperator Compared(string operatorText) => operatorText switch
	{
		"="  => SqlOperator.Equal,
		"<>" => SqlOperator.NotEqual,
		"<"  => SqlOperator.Less,
		"<=" => SqlOperator.LessOrEqual,
		">"  => SqlOperator.Greater,
		_    => SqlOperator.GreaterOrEqual,
	};

	/// <summary>
	/// A word the grammar matched, as the one constant that stands for it.
	/// </summary>
	/// <remarks>
	/// The text is what the author wrote, in whatever case they wrote it; the tree says
	/// the word. Told apart by length and first letter, which costs nothing and allocates
	/// nothing — the alternative is the matched text, and a capture cuts a string.
	/// </remarks>
	static SqlTruth Truth(string word) =>
		(word[0] | 0x20) switch
		{
			't' => SqlTruth.True,
			'f' => SqlTruth.False,
			_   => SqlTruth.Unknown,
		};

	static string Aggregate(string word) =>
		(word[0] | 0x20) switch
		{
			'a' => "AVG",
			's' => "SUM",
			'c' => "COUNT",
			'm' => (word[1] | 0x20) == 'a' ? "MAX" : "MIN",
			_   => word,
		};

	static string? Quantified(string? word) =>
		word is null
			? null
			: (word[0] | 0x20) switch
			{
				'a' => (word[1] | 0x20) == 'l' ? "ALL" : "ANY",
				's' => "SOME",
				_   => word,
			};

	static string? Distinctly(string? word) =>
		word is null ? null : (word[0] | 0x20) == 'd' ? "DISTINCT" : "ALL";

	/// <summary>What a match predicate was qualified by, as one word or two.</summary>
	static string? Matched(string? unique, string? kind)
	{
		var partial = kind is not null && (kind[0] | 0x20) == 'p';

		return unique is null
			? kind is null ? null : partial ? "PARTIAL" : "FULL"
			: kind is null ? "UNIQUE" : partial ? "UNIQUE PARTIAL" : "UNIQUE FULL";
	}
}
