using System;

namespace DotGram.Parsers;

/// <summary>
/// What <see cref="SqlStandard92"/> builds: a search condition or a value expression as
/// a tree.
/// </summary>
/// <remarks>
/// <para>
/// <b>One base and one level of descendants.</b> Nine records cover the whole of §6
/// through §8, because what distinguishes an <c>OR</c> from a <c>*</c>, or a
/// <c>BETWEEN</c> from a <c>LIKE</c>, is written as a field rather than as a type. The
/// alternative — a record per production, three levels deep — is the shape a grammar
/// suggests and a consumer regrets: a visitor over it has forty methods, and adding the
/// fortieth production breaks every visitor already written.
/// </para>
/// <para>
/// Aggregation over inheritance for the same reason. A predicate holds its operands in
/// an array rather than in named fields per kind, so the shape of <c>x BETWEEN a AND b</c>
/// is "the kind is Between and there are three operands" rather than a type of its own.
/// What each operand means is in <see cref="SqlPredicateKind"/>'s documentation, where a
/// reader looks once.
/// </para>
/// <para>
/// Nothing here is a position: the tree says what was written, not where. A consumer that
/// needs the text back cuts it from the input itself, which is what §7.6 is for.
/// </para>
/// </remarks>
public abstract record SqlNode;

/// <summary>Two operands and the operator between them.</summary>
public sealed record SqlBinary(SqlOperator Operator, SqlNode Left, SqlNode Right) : SqlNode;

/// <summary>One operand and the operator in front of it — <c>NOT</c>, and the signs.</summary>
public sealed record SqlUnary(SqlOperator Operator, SqlNode Operand) : SqlNode;

/// <summary>
/// <c>x IS NOT TRUE</c> and its fellows: what is tested, and what it is tested against.
/// </summary>
public sealed record SqlTruthTest(SqlNode Operand, bool Negated, SqlTruth Truth) : SqlNode;

/// <summary>
/// A predicate (§8): its kind, whether <c>NOT</c> was written in the middle of it, and
/// its operands in the order the standard writes them.
/// </summary>
/// <remarks>
/// <see cref="Operator"/> is set where a comparison operator was written, and
/// <see cref="Word"/> carries the one word some predicates need and no two need alike —
/// the quantifier of a quantified comparison, and <c>UNIQUE</c>, <c>PARTIAL</c> or
/// <c>FULL</c> on a match.
/// </remarks>
public sealed record SqlPredicate(
	SqlPredicateKind Kind, bool Negated, SqlNode[] Operands,
	SqlOperator? Operator = null, string? Word = null) : SqlNode;

/// <summary>
/// A function, a set function or a cast: the name as written, its arguments, and the one
/// word a few of them carry — <c>DISTINCT</c>, a datetime field, a trim specification, or
/// the type a cast names.
/// </summary>
public sealed record SqlCall(string Name, SqlNode[] Arguments, string? Word = null) : SqlNode;

/// <summary>A <c>CASE</c>, simple where it has an operand and searched where it does not.</summary>
public sealed record SqlCase(SqlNode? Operand, SqlWhen[] Whens, SqlNode? Else) : SqlNode;

/// <summary>One <c>WHEN … THEN …</c> of a <c>CASE</c>.</summary>
public sealed record SqlWhen(SqlNode Test, SqlNode Result) : SqlNode;

/// <summary>A column reference, as written, dots and all.</summary>
public sealed record SqlName(string Text) : SqlNode;

/// <summary>
/// A literal, a parameter, or one of the words that stand where a value does —
/// <c>NULL</c>, <c>DEFAULT</c>, <c>CURRENT_USER</c>.
/// </summary>
public sealed record SqlLiteral(SqlLiteralKind Kind, string Text) : SqlNode;

/// <summary>A row of several values, <c>(a, b)</c>.</summary>
public sealed record SqlRow(SqlNode[] Values) : SqlNode;

/// <summary>
/// A subquery, kept as the text between its parentheses.
/// </summary>
/// <remarks>
/// The grammar does not read a <c>SELECT</c> — §6 and §8 are what it covers, and a query
/// specification is a chapter of its own. It reads far enough to find the parenthesis that
/// closes, and hands over what stood inside.
/// </remarks>
public sealed record SqlSubquery(string Text) : SqlNode;

/// <summary>What stands between two operands, or in front of one.</summary>
public enum SqlOperator
{
	Or, And, Not,
	Add, Subtract, Concatenate, Multiply, Divide,
	Negate, Identity,
	Equal, NotEqual, Less, LessOrEqual, Greater, GreaterOrEqual,
}

/// <summary>
/// Which predicate (§8), and what its operands are.
/// </summary>
/// <remarks>
/// The first operand is the row on the left in every kind that has one, so a consumer
/// that only wants what is being tested reads <c>Operands[0]</c> without asking which
/// predicate it is.
/// <list type="bullet">
/// <item><see cref="Comparison"/> — the two sides, and <c>Operator</c>.</item>
/// <item><see cref="Quantified"/> — the left side and the subquery, with <c>Operator</c> and the quantifier as the word.</item>
/// <item><see cref="Between"/> — the value, the low bound, the high bound.</item>
/// <item><see cref="In"/> — the value, and one operand that is a row of the listed values or a subquery.</item>
/// <item><see cref="Like"/> — the value, the pattern, and the escape where one was written.</item>
/// <item><see cref="IsNull"/> — the value alone.</item>
/// <item><see cref="Exists"/> and <see cref="Unique"/> — the subquery alone.</item>
/// <item><see cref="Match"/> — the value and the subquery, with the modifiers as the word.</item>
/// <item><see cref="Overlaps"/> — the two rows.</item>
/// </list>
/// </remarks>
public enum SqlPredicateKind
{
	Comparison, Quantified, Between, In, Like, IsNull, Exists, Unique, Match, Overlaps,
}

/// <summary>What a literal is, where the text alone does not say.</summary>
public enum SqlLiteralKind
{
	Number, Text, National, Bit, Hex, Date, Time, Timestamp, Interval,
	Null, Default, Parameter, Special,
}

/// <summary>The three truth values a test compares against (§8.13).</summary>
public enum SqlTruth
{
	True, False, Unknown,
}
