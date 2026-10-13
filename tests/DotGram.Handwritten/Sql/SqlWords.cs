using System;

namespace DotGram.Handwritten;

/// <summary>The spellings of §5.2's reserved words, and the search that turns a word into one.</summary>
/// <remarks>
/// <para>
/// A word is refused by its length before anything is compared, and by its first letter after
/// that, so a name — which is most words in most SQL — costs two tests and no comparison. What
/// survives both is compared against the few reserved words of that shape.
/// </para>
/// <para>
/// A hashed lookup was the other candidate and is the worse one here: it computes a hash of every
/// word, names included, where this computes nothing for a name of a length or an initial that no
/// reserved word has.
/// </para>
/// </remarks>
static class SqlWords
{
	/// <summary>Every reserved word, in <see cref="SqlWord"/>'s order: <c>Spellings[(int)word - 1]</c>.</summary>
	public static readonly string[] Spellings =
	[
		"ABS", "ABSENT", "ACOS", "ALL", "ALLOCATE",
		"ALTER", "AND", "ANY", "ANY_VALUE", "ARE",
		"ARRAY", "ARRAY_AGG", "ARRAY_MAX_CARDINALITY", "AS", "ASENSITIVE",
		"ASIN", "ASYMMETRIC", "AT", "ATAN", "ATOMIC",
		"AUTHORIZATION", "AVG", "BEGIN", "BEGIN_FRAME", "BEGIN_PARTITION",
		"BETWEEN", "BIGINT", "BINARY", "BLOB", "BOOLEAN",
		"BOTH", "BTRIM", "BY", "CALL", "CALLED",
		"CARDINALITY", "CASCADED", "CASE", "CAST", "CEIL",
		"CEILING", "CHAR", "CHAR_LENGTH", "CHARACTER", "CHARACTER_LENGTH",
		"CHECK", "CLASSIFIER", "CLOB", "CLOSE", "COALESCE",
		"COLLATE", "COLLECT", "COLUMN", "COMMIT", "CONDITION",
		"CONNECT", "CONSTRAINT", "CONTAINS", "CONVERT", "COPY",
		"CORR", "CORRESPONDING", "COS", "COSH", "COUNT",
		"COVAR_POP", "COVAR_SAMP", "CREATE", "CROSS", "CUBE",
		"CUME_DIST", "CURRENT", "CURRENT_CATALOG", "CURRENT_DATE", "CURRENT_DEFAULT_TRANSFORM_GROUP",
		"CURRENT_PATH", "CURRENT_ROLE", "CURRENT_ROW", "CURRENT_SCHEMA", "CURRENT_TIME",
		"CURRENT_TIMESTAMP", "CURRENT_TRANSFORM_GROUP_FOR_TYPE", "CURRENT_USER", "CURSOR", "CYCLE",
		"DATE", "DAY", "DEALLOCATE", "DEC", "DECFLOAT",
		"DECIMAL", "DECLARE", "DEFAULT", "DEFINE", "DELETE",
		"DENSE_RANK", "DEREF", "DESCRIBE", "DETERMINISTIC", "DISCONNECT",
		"DISTINCT", "DOUBLE", "DROP", "DYNAMIC", "EACH",
		"ELEMENT", "ELSE", "EMPTY", "END", "END_FRAME",
		"END_PARTITION", "END-EXEC", "EQUALS", "ESCAPE", "EVERY",
		"EXCEPT", "EXEC", "EXECUTE", "EXISTS", "EXP",
		"EXTERNAL", "EXTRACT", "FALSE", "FETCH", "FILTER",
		"FIRST_VALUE", "FLOAT", "FLOOR", "FOR", "FOREIGN",
		"FRAME_ROW", "FREE", "FROM", "FULL", "FUNCTION",
		"FUSION", "GET", "GLOBAL", "GRANT", "GREATEST",
		"GROUP", "GROUPING", "GROUPS", "HAVING", "HOLD",
		"HOUR", "IDENTITY", "IN", "INDICATOR", "INITIAL",
		"INNER", "INOUT", "INSENSITIVE", "INSERT", "INT",
		"INTEGER", "INTERSECT", "INTERSECTION", "INTERVAL", "INTO",
		"IS", "JOIN", "JSON", "JSON_ARRAY", "JSON_ARRAYAGG",
		"JSON_EXISTS", "JSON_OBJECT", "JSON_OBJECTAGG", "JSON_QUERY", "JSON_SCALAR",
		"JSON_SERIALIZE", "JSON_TABLE", "JSON_TABLE_PRIMITIVE", "JSON_VALUE", "LAG",
		"LANGUAGE", "LARGE", "LAST_VALUE", "LATERAL", "LEAD",
		"LEADING", "LEAST", "LEFT", "LIKE", "LIKE_REGEX",
		"LISTAGG", "LN", "LOCAL", "LOCALTIME", "LOCALTIMESTAMP",
		"LOG", "LOG10", "LOWER", "LPAD", "LTRIM",
		"MATCH", "MATCH_NUMBER", "MATCH_RECOGNIZE", "MATCHES", "MAX",
		"MEMBER", "MERGE", "METHOD", "MIN", "MINUTE",
		"MOD", "MODIFIES", "MODULE", "MONTH", "MULTISET",
		"NATIONAL", "NATURAL", "NCHAR", "NCLOB", "NEW",
		"NO", "NONE", "NORMALIZE", "NOT", "NTH_VALUE",
		"NTILE", "NULL", "NULLIF", "NUMERIC", "OCCURRENCES_REGEX",
		"OCTET_LENGTH", "OF", "OFFSET", "OLD", "OMIT",
		"ON", "ONE", "ONLY", "OPEN", "OR",
		"ORDER", "OUT", "OUTER", "OVER", "OVERLAPS",
		"OVERLAY", "PARAMETER", "PARTITION", "PATTERN", "PER",
		"PERCENT", "PERCENT_RANK", "PERCENTILE_CONT", "PERCENTILE_DISC", "PERIOD",
		"PORTION", "POSITION", "POSITION_REGEX", "POWER", "PRECEDES",
		"PRECISION", "PREPARE", "PRIMARY", "PROCEDURE", "PTF",
		"RANGE", "RANK", "READS", "REAL", "RECURSIVE",
		"REF", "REFERENCES", "REFERENCING", "REGR_AVGX", "REGR_AVGY",
		"REGR_COUNT", "REGR_INTERCEPT", "REGR_R2", "REGR_SLOPE", "REGR_SXX",
		"REGR_SXY", "REGR_SYY", "RELEASE", "RESULT", "RETURN",
		"RETURNS", "REVOKE", "RIGHT", "ROLLBACK", "ROLLUP",
		"ROW", "ROW_NUMBER", "ROWS", "RPAD", "RTRIM",
		"RUNNING", "SAVEPOINT", "SCOPE", "SCROLL", "SEARCH",
		"SECOND", "SEEK", "SELECT", "SENSITIVE", "SESSION_USER",
		"SET", "SHOW", "SIMILAR", "SIN", "SINH",
		"SKIP", "SMALLINT", "SOME", "SPECIFIC", "SPECIFICTYPE",
		"SQL", "SQLEXCEPTION", "SQLSTATE", "SQLWARNING", "SQRT",
		"START", "STATIC", "STDDEV_POP", "STDDEV_SAMP", "SUBMULTISET",
		"SUBSET", "SUBSTRING", "SUBSTRING_REGEX", "SUCCEEDS", "SUM",
		"SYMMETRIC", "SYSTEM", "SYSTEM_TIME", "SYSTEM_USER", "TABLE",
		"TABLESAMPLE", "TAN", "TANH", "THEN", "TIME",
		"TIMESTAMP", "TIMEZONE_HOUR", "TIMEZONE_MINUTE", "TO", "TRAILING",
		"TRANSLATE", "TRANSLATE_REGEX", "TRANSLATION", "TREAT", "TRIGGER",
		"TRIM", "TRIM_ARRAY", "TRUE", "TRUNCATE", "UESCAPE",
		"UNION", "UNIQUE", "UNKNOWN", "UNNEST", "UPDATE",
		"UPPER", "USER", "USING", "VALUE", "VALUES",
		"VALUE_OF", "VAR_POP", "VAR_SAMP", "VARBINARY", "VARCHAR",
		"VARYING", "VERSIONING", "WHEN", "WHENEVER", "WHERE",
		"WIDTH_BUCKET", "WINDOW", "WITH", "WITHIN", "WITHOUT",
		"YEAR",
	];

	/// <summary>The shortest and the longest reserved word.</summary>
	const int Shortest = 2, Longest = 32;

	/// <summary>
	/// The reserved words of each length and initial letter, as indices into <see cref="Spellings"/>:
	/// one bucket per <c>(length - Shortest) * 26 + letter</c>.
	/// </summary>
	static readonly ushort[][] Shapes = Shape();

	static ushort[][] Shape()
	{
		var counts = new int[(Longest - Shortest + 1) * 26];

		for (var at = 0; at < Spellings.Length; at++)
			counts[Bucket(Spellings[at])]++;

		var shapes = new ushort[counts.Length][];

		for (var at = 0; at < counts.Length; at++)
			shapes[at] = counts[at] == 0 ? [] : new ushort[counts[at]];

		var filled = new int[counts.Length];

		for (var at = 0; at < Spellings.Length; at++)
		{
			var bucket = Bucket(Spellings[at]);

			shapes[bucket][filled[bucket]++] = (ushort)(at + 1);
		}

		return shapes;
	}

	static int Bucket(string word)
	{
		return (word.Length - Shortest) * 26 + (word[0] - 'A');
	}

	/// <summary>Which reserved word this is, or <see cref="SqlWord.Name"/> where it is a name.</summary>
	public static SqlWord Of(ReadOnlySpan<char> word)
	{
		if ((uint)(word.Length - Shortest) > Longest - Shortest)
			return SqlWord.Name;

		var letter = (uint)((word[0] | 0x20) - 'a');

		if (letter > 'z' - 'a')
			return SqlWord.Name;

		foreach (var candidate in Shapes[(word.Length - Shortest) * 26 + (int)letter])
			if (word.Equals(Spellings[candidate - 1], StringComparison.OrdinalIgnoreCase))
				return (SqlWord)candidate;

		return SqlWord.Name;
	}
}
