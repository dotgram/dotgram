using System;

namespace DotGram.Parsers;

/// <summary>
/// What a SQL parser builds — <see cref="SqlStandard92"/>, or anything else that reads the
/// same grammar: a search condition or a value expression as a tree.
/// </summary>
/// <remarks>
/// <para>
/// <b>One class, with its descendants inside it.</b> Eleven records cover the whole of §6
/// through §8, because what distinguishes an <c>OR</c> from a <c>*</c>, or a <c>BETWEEN</c>
/// from a <c>LIKE</c>, is written as a field rather than as a type. The alternative — a
/// record per production, three levels deep — is the shape a grammar suggests and a
/// consumer regrets: a visitor over it has forty methods, and adding the fortieth
/// production breaks every visitor already written. Nested rather than beside, so that the
/// tree is one name to import and one place to read.
/// </para>
/// <para>
/// Aggregation over inheritance for the same reason. A predicate holds its operands in an
/// array rather than in named fields per kind, so the shape of <c>x BETWEEN a AND b</c> is
/// "the kind is Between and there are three operands" rather than a type of its own. What
/// each operand means is in <see cref="SqlPredicateKind"/>'s documentation, where a reader
/// looks once.
/// </para>
/// <para>
/// <b>The tree knows how it is made.</b> The words a grammar matches and the lists it
/// gathers become nodes through the statics here — <see cref="Compared"/>,
/// <see cref="TruthOf"/>, <see cref="Listed"/> and the rest — so that any parser of this
/// language, generated under any carrier or written by hand, builds the same tree by the
/// same code. They used to be the generated parser's own, and a second parser of the same
/// grammar could not reach them.
/// </para>
/// <para>
/// Nothing here is a position: the tree says what was written, not where. A consumer that
/// needs the text back cuts it from the input itself, which is what §7.6 is for.
/// </para>
/// </remarks>
public abstract record SqlNode
{
	/// <summary>Two operands and the operator between them.</summary>
	public sealed record Binary(SqlOperator Operator, SqlNode Left, SqlNode Right) : SqlNode;

	/// <summary>One operand and the operator in front of it — <c>NOT</c>, and the signs.</summary>
	public sealed record Unary(SqlOperator Operator, SqlNode Operand) : SqlNode;

	/// <summary>
	/// <c>x IS NOT TRUE</c> and its fellows: what is tested, and what it is tested against.
	/// </summary>
	public sealed record TruthTest(SqlNode Operand, bool Negated, SqlTruth Truth) : SqlNode;

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
	public sealed record Predicate(
		SqlPredicateKind Kind, bool Negated, SqlNode[] Operands,
		SqlOperator? Operator = null, string? Word = null) : SqlNode;

	/// <summary>
	/// A function, a set function or a cast: the name as written, its arguments, and the one
	/// word a few of them carry — <c>DISTINCT</c>, a datetime field, a trim specification, or
	/// the type a cast names.
	/// </summary>
	public sealed record Call(string Name, SqlNode[] Arguments, string? Word = null) : SqlNode;

	/// <summary>A <c>CASE</c>, simple where it has an operand and searched where it does not.</summary>
	public sealed record Case(SqlNode? Operand, When[] Whens, SqlNode? Else) : SqlNode;

	/// <summary>One <c>WHEN … THEN …</c> of a <c>CASE</c>.</summary>
	public sealed record When(SqlNode Test, SqlNode Result) : SqlNode;

	/// <summary>A column reference, as written, dots and all.</summary>
	public sealed record Column(string Text) : SqlNode;

	/// <summary>
	/// A literal, a parameter, or one of the words that stand where a value does —
	/// <c>NULL</c>, <c>DEFAULT</c>, <c>CURRENT_USER</c>.
	/// </summary>
	public sealed record Literal(SqlLiteralKind Kind, string Text) : SqlNode;

	/// <summary>A row of several values, <c>(a, b)</c>.</summary>
	public sealed record Row(SqlNode[] Values) : SqlNode;

	/// <summary>
	/// A subquery, kept as the text between its parentheses.
	/// </summary>
	/// <remarks>
	/// The grammar does not read a <c>SELECT</c> — §6 and §8 are what it covers, and a query
	/// specification is a chapter of its own. It reads far enough to find the parenthesis
	/// that closes, and hands over what stood inside.
	/// </remarks>
	public sealed record Subquery(string Text) : SqlNode;

	// ---- how a parser makes these ------------------------------------------------------------

	/// <summary>What a call with no arguments is handed, once rather than per call.</summary>
	public static readonly SqlNode[] None = [];

	/// <summary>The two words that stand where a value does and are always the same node.</summary>
	public static readonly SqlNode NullValue    = new Literal(SqlLiteralKind.Null,    "NULL");
	public static readonly SqlNode DefaultValue = new Literal(SqlLiteralKind.Default, "DEFAULT");

	/// <summary>A head and a tail as one array, which is what a separated list comes to.</summary>
	public static SqlNode[] Listed(SqlNode first, SqlNode[]? rest)
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
	/// The row is read once for all nine predicates, so the tail is built without it and
	/// the slot it left is filled here. Written into the array rather than copied onto a
	/// new record: the array is this predicate's own and nothing has seen it yet.
	/// </remarks>
	public static Predicate Predicated(SqlNode left, Predicate tail)
	{
		tail.Operands[0] = left;

		return tail;
	}

	public static SqlOperator Additive(string operatorText) => operatorText switch
	{
		"+" => SqlOperator.Add,
		"-" => SqlOperator.Subtract,
		_   => SqlOperator.Concatenate,
	};

	public static SqlOperator Multiplicative(string operatorText) =>
		operatorText == "*" ? SqlOperator.Multiply : SqlOperator.Divide;

	public static SqlOperator Signed(string sign) =>
		sign == "-" ? SqlOperator.Negate : SqlOperator.Identity;

	public static SqlOperator Compared(string operatorText) => operatorText switch
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
	/// The text is what the author wrote, in whatever case they wrote it; the tree says the
	/// word. Told apart by length and first letter, which costs nothing and allocates
	/// nothing — the alternative is the matched text, and a capture cuts a string.
	/// </remarks>
	public static SqlTruth TruthOf(string word) =>
		(word[0] | 0x20) switch
		{
			't' => SqlTruth.True,
			'f' => SqlTruth.False,
			_   => SqlTruth.Unknown,
		};

	public static string Aggregate(string word) =>
		(word[0] | 0x20) switch
		{
			'a' => "AVG",
			's' => "SUM",
			'c' => "COUNT",
			'm' => (word[1] | 0x20) == 'a' ? "MAX" : "MIN",
			_   => word,
		};

	public static string? Quantified(string? word) =>
		word is null
			? null
			: (word[0] | 0x20) switch
			{
				'a' => (word[1] | 0x20) == 'l' ? "ALL" : "ANY",
				's' => "SOME",
				_   => word,
			};

	public static string? Distinctly(string? word) =>
		word is null ? null : (word[0] | 0x20) == 'd' ? "DISTINCT" : "ALL";

	/// <summary>What a match predicate was qualified by, as one word or two.</summary>
	public static string? Matched(string? unique, string? kind)
	{
		var partial = kind is not null && (kind[0] | 0x20) == 'p';

		return unique is null
			? kind is null ? null : partial ? "PARTIAL" : "FULL"
			: kind is null ? "UNIQUE" : partial ? "UNIQUE PARTIAL" : "UNIQUE FULL";
	}
}

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
