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

	// ---- §7, the query level -----------------------------------------------------------------

	/// <summary>§7.9 <c>SELECT</c>, and the clauses under it.</summary>
	/// <remarks>
	/// One record for the whole of a query specification and its table expression, because
	/// the clauses are optional rather than alternative: what tells one query from another
	/// is which of them are empty. <see cref="From"/> holds the table references the
	/// standard writes as a comma list, which is a cross join said the older way.
	/// </remarks>
	public sealed record Query(
		bool Distinct,
		SqlNode[] Columns,
		SqlNode[] From,
		SqlNode? Where,
		SqlNode[] GroupBy,
		SqlNode? Having) : SqlNode;

	/// <summary>One entry of a select list: what it is, and what it is called.</summary>
	public sealed record Selected(SqlNode Value, string? Name) : SqlNode;

	/// <summary><c>*</c>, or <c>t.*</c> — which is no column, so it is not one.</summary>
	public sealed record Star(string? Qualifier) : SqlNode;

	/// <summary>
	/// §7.4 one entry of a <c>FROM</c> clause: a table by name or a query standing where
	/// one does, the name it is known by there, and the names its columns are given.
	/// </summary>
	public sealed record Source(
		string? Table, SqlNode? Derived, string? Name, string[]? Columns) : SqlNode;

	/// <summary>§7.5 two sources and the join between them.</summary>
	/// <remarks>
	/// <see cref="On"/> where the join was qualified by a condition, <see cref="Using"/>
	/// where it named columns, and neither for a cross or a natural join.
	/// </remarks>
	public sealed record Join(
		SqlJoin Kind, bool Natural, SqlNode Left, SqlNode Right,
		SqlNode? On = null, string[]? Using = null) : SqlNode;

	/// <summary>§7.2 <c>VALUES (…), (…)</c> — a table written out.</summary>
	public sealed record TableValue(SqlNode[] Rows) : SqlNode;

	/// <summary>§13.1 a query and the order its rows are asked for in.</summary>
	public sealed record Ordered(SqlNode Of, SqlNode[] By) : SqlNode;

	/// <summary>One <c>ORDER BY</c> entry: what to sort by, and which way.</summary>
	public sealed record Sorted(SqlNode Value, bool Down) : SqlNode;

	// ---- the statements ----------------------------------------------------------------------
	//
	// One record per kind, beside the others rather than under a base of their own, which is
	// what this ADT is: one root and one level of descendants. A statement is a node like any
	// other because a statement holds queries and a query holds statements — `INSERT … SELECT`
	// one way and `SELECT … FROM (MERGE … OUTPUT …)` the other.
	//
	// What is read and dropped keeps growing, and it is worth naming here rather than only in
	// the grammar: `TOP`, `OVER`, the hints, the windows, a named query's `WITH`, and now
	// `OUTPUT`. Each is a decoration on how a statement runs or what it returns rather than on
	// what it is, and each would be a field on four records here. When one of them is wanted,
	// it is one field — but none is wanted yet, and a field nobody reads is a field that drifts.

	/// <summary>Rows written into a table, from a list, a query or nothing at all.</summary>
	/// <remarks>
	/// <see cref="Rows"/> is a <see cref="TableValue"/> where they were written out, a query
	/// where they come from one, and a <see cref="Row"/> of nothing for `DEFAULT VALUES`.
	/// </remarks>
	public sealed record Insert(SqlNode? Target, string[]? Columns, SqlNode Rows) : SqlNode;

	/// <summary>Rows changed in place: what to change, to what, and which rows.</summary>
	/// <remarks>
	/// <see cref="From"/> is T-SQL's extension and not the standard's: a second `FROM` naming
	/// the tables the rows to change are found by joining.
	/// </remarks>
	public sealed record Update(
		SqlNode? Target, SqlNode[] Set, SqlNode[] From, SqlNode? Where) : SqlNode;

	/// <summary>Rows removed, and the same two ways of saying which.</summary>
	public sealed record Delete(SqlNode? Target, SqlNode[] From, SqlNode? Where) : SqlNode;

	/// <summary>
	/// One statement that inserts, updates and deletes, according to what a join found.
	/// </summary>
	public sealed record Merge(
		SqlNode? Target, SqlNode Using, SqlNode On, SqlNode[] Whens) : SqlNode;

	/// <summary>
	/// One arm of a merge: whether it fired on a match, which side the match was missing
	/// from, what else had to be true, and what to do.
	/// </summary>
	public sealed record MergeWhen(
		bool OnMatch, string? By, SqlNode? Condition, SqlNode Action) : SqlNode;

	/// <summary>
	/// One entry of a `SET`: what is assigned, the operator it was assigned with where that
	/// was not a plain `=`, and the value.
	/// </summary>
	public sealed record Assign(string Target, string? Operator, SqlNode Value) : SqlNode;

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

	/// <summary>Which set operator was written, and whether it keeps duplicates.</summary>
	public static SqlOperator Combined(string operatorText, string? all) =>
		operatorText.ToUpperInvariant() switch
		{
			"UNION"     => all is null ? SqlOperator.Union     : SqlOperator.UnionAll,
			"EXCEPT"    => all is null ? SqlOperator.Except    : SqlOperator.ExceptAll,
			_           => all is null ? SqlOperator.Intersect : SqlOperator.IntersectAll,
		};

	/// <summary>Whether a sort specification asked for descending order (§13.1).</summary>
	public static bool Descending(string? order) =>
		string.Equals(order, "DESC", StringComparison.OrdinalIgnoreCase);

	/// <summary>A join from the words around it: the kind, and which of the two tails it had.</summary>
	public static Join Joining(
		string? kind, string? natural, SqlNode left, SqlNode right, SqlNode? on, string[]? columns) =>
		new(Joined(kind), natural is not null, left, right, on, columns);

	/// <summary>Which join was written, from the words in front of <c>JOIN</c>.</summary>
	/// <remarks><c>OUTER</c> is noise beside <c>LEFT</c>, <c>RIGHT</c> and <c>FULL</c> (§7.5).</remarks>
	public static SqlJoin Joined(string? kind)
	{
		// The text is the whole of what was written, `LEFT OUTER` and not `LEFT`, so the
		// first word is what is asked about.
		if (kind is null)
			return SqlJoin.Inner;

		if (kind.StartsWith("LEFT", StringComparison.OrdinalIgnoreCase))
			return SqlJoin.Left;

		if (kind.StartsWith("RIGHT", StringComparison.OrdinalIgnoreCase))
			return SqlJoin.Right;

		return kind.StartsWith("FULL", StringComparison.OrdinalIgnoreCase)
			? SqlJoin.Full
			: SqlJoin.Inner;
	}

	/// <summary>A head and a tail of names as one array, the way <see cref="Listed"/> does nodes.</summary>
	public static string[] Named(string first, string[]? rest)
	{
		if (rest is null || rest.Length == 0)
			return [first];

		var all = new string[rest.Length + 1];

		all[0] = first;
		rest.CopyTo(all, 1);

		return all;
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

	/// <summary>§7.10, where the operands are tables rather than values.</summary>
	Union, UnionAll, Except, ExceptAll, Intersect, IntersectAll,
}

/// <summary>Which join (§7.5).</summary>
public enum SqlJoin
{
	Cross, Inner, Left, Right, Full, Union,
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
