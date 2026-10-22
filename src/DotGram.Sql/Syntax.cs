using System;

namespace DotGram.Sql;

/// <summary>
/// What every hierarchy needs and none of them owns: the lists a grammar gathers, and the
/// words it matched read back as the constants they stand for.
/// </summary>
/// <remarks>
/// The words are told apart by length and first letter, which costs nothing and allocates
/// nothing — the alternative is the matched text, and a capture cuts a string.
/// </remarks>
internal static class Syntax
{
	/// <summary>
	/// A node a reading rebuilt with <c>with</c> out of nodes it had already located, told the
	/// range from the first of them to the last: a copy made while reading is not an edit, and
	/// is not left saying it is stale.
	/// </summary>
	/// <remarks>
	/// Where nothing was located — a plain reading — the copy is handed back as it is.
	/// </remarks>
	internal static T Joined<T>(T made, ISqlSpan first, ISqlSpan last) where T : ISqlLocatable
	{
		if (!first.Span.Known && !last.Span.Known)
			return made;

		var from = first.Span.Known ? first.Span : last.Span;
		var to   = last.Span.Known ? last.Span : first.Span;

		made.Locate(from.At, to.End - from.At, from.GapStart);

		return made;
	}

	/// <summary>What a call with no names is handed, once rather than per call.</summary>
	public static readonly SqlList<string> NoNames = [];

	/// <summary>A head and a tail as one array, which is what a separated list comes to.</summary>
	public static T[] Listed<T>(T first, T[]? rest)
	{
		if (rest is null || rest.Length == 0)
			return [first];

		var all = new T[rest.Length + 1];

		all[0] = first;
		rest.CopyTo(all, 1);

		return all;
	}

	/// <summary>A head and a tail of names as one array, the way <see cref="Listed"/> does nodes.</summary>
	public static string[] Named(string first, string[]? rest)
	{
		return Listed(first, rest);
	}

	/// <summary>
	/// What a catalogue statement wrote after its name, as the words were written — or null
	/// where it wrote nothing.
	/// </summary>
	public static string? Tail(string? words)
	{
		return words is null || words.Length == 0 ? null : Spaced(words);
	}

	/// <summary>Words as written, with the whitespace between them made one space.</summary>
	public static string Spaced(string words)
	{
		return Run(words, false);
	}

	/// <summary>Text with every whitespace character taken out: <c>(1 . 2 . 3 . 4)</c> as <c>(1.2.3.4)</c>.</summary>
	/// <summary>
	/// Whether a name may be called as a function: a delimited name of one part may not. The
	/// engine refuses <c>[LEN]('a')</c> and <c>"LEN"('a')</c>, and reads <c>dbo.[LEN]('a')</c>.
	/// </summary>
	public static bool Callable(string name)
	{
		// Not through an empty part: `.f2 (1)` and `db..f2 (1)` are `Msg 102` as a value, where
		// `FROM .f2 (1)` and `FROM dbo..f2 (1)` read a table-valued function — that is another
		// rule's business.
		if (name.Length > 0 && (name[0] == '.' || name.IndexOf("..", StringComparison.Ordinal) >= 0))
			return false;

		if (name.Length == 0 || (name[0] != '[' && name[0] != '"'))
			return true;

		var close = name[0] == '[' ? ']' : '"';

		for (var at = 1; at < name.Length; at++)
		{
			if (name[at] != close)
				continue;

			if (at + 1 < name.Length && name[at + 1] == close)
			{
				at++;
				continue;
			}

			return at + 1 < name.Length;
		}

		return true;
	}

	/// <summary>
	/// The names a date part is written with: the published ones and their abbreviations, and
	/// <c>w</c> beside them, which the engine reads for <c>weekday</c> and the table leaves out.
	/// </summary>
	static readonly HashSet<string> DateParts = new(StringComparer.OrdinalIgnoreCase)
	{
		"year", "yy", "yyyy", "quarter", "qq", "q", "month", "mm", "m", "dayofyear", "dy", "y",
		"day", "dd", "d", "week", "wk", "ww", "weekday", "dw", "w", "hour", "hh", "minute", "mi",
		"n", "second", "ss", "s", "millisecond", "ms", "microsecond", "mcs", "nanosecond", "ns",
		"tzoffset", "tz", "iso_week", "isowk", "isoww",
	};

	/// <summary>The functions whose first argument is a date part.</summary>
	static readonly HashSet<string> DatePartFunctions = new(StringComparer.OrdinalIgnoreCase)
	{
		"DATEPART", "DATENAME", "DATEADD", "DATEDIFF", "DATEDIFF_BIG", "DATETRUNC", "DATE_BUCKET",
	};

	/// <summary>Whether a name, bracketed, quoted or neither, is a date part's.</summary>
	public static bool IsDatePart(string? name)
	{
		return name is not null && DateParts.Contains(Unquoted(name));
	}

	/// <summary>Whether a name of one part is a function whose first argument is a date part.</summary>
	public static bool IsDatePartFunction(string name)
	{
		return DatePartFunctions.Contains(name);
	}

	static string Unquoted(string name)
	{
		return name.Length >= 2 && (name[0] == '[' && name[^1] == ']' || name[0] == '"' && name[^1] == '"')
			? name[1..^1]
			: name;
	}

	public static string Compacted(string text)
	{
		var kept = new char[text.Length];
		var at   = 0;

		foreach (var one in text)
			if (!char.IsWhiteSpace(one))
				kept[at++] = one;

		return new string(kept, 0, at);
	}

	/// <summary>
	/// Whether a bracket holds an IPv4 address: four runs of digits and the three dots between
	/// them, however spaced, and after a colon four more for the mask — which is how an
	/// endpoint's <c>LISTENER_IP</c> is written. The lexer reads <c>1.2</c> as one number, so
	/// the parts are counted here and not by a rule.
	/// </summary>
	public static bool IsAddress(string text)
	{
		var inside = Compacted(text);

		if (inside.Length < 2 || inside[0] != '(' || inside[inside.Length - 1] != ')')
			return false;

		var halves = inside.Substring(1, inside.Length - 2).Split(':');

		return halves.Length <= 2 && Array.TrueForAll(halves, IsFourPart);
	}

	/// <summary>Four runs of digits with a dot between each two, and nothing else.</summary>
	static bool IsFourPart(string text)
	{
		var parts  = 1;
		var digits = 0;

		foreach (var one in text)
		{
			if (one == '.')
			{
				if (digits == 0)
					return false;

				parts++;
				digits = 0;
			}
			else if (one is >= '0' and <= '9')
				digits++;
			else
				return false;
		}

		return parts == 4 && digits > 0;
	}

	/// <summary>
	/// A run of words, one space between them — and what stands inside quotes copied
	/// character for character.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A literal is not a run of words. <c>'a  b'</c> is one value with two spaces in it and
	/// <c>'default()'</c> is not a keyword to be raised, so neither the spacing nor the case of
	/// anything between quotes is this function's to change — which is what <c>MASKED WITH
	/// (FUNCTION = 'default()')</c> came back from as <c>'DEFAULT()'</c>. A closing quote written twice is
	/// one inside the quotes, as T-SQL reads <c>[a]]  b]</c>, <c>"a""  b"</c> and <c>'a''  b'</c>.
	/// </para>
	/// <para>
	/// A line comment ends where its line does, so the line end after one stays a line end: made one
	/// space, <c>--x</c> and the line under it would be one comment, which swallows what the line under
	/// it says. A block comment, the comments inside it counted, is copied as it stands, so that a quote or a
	/// <c>--</c> inside it is neither. A space is what the grammar's spacing is
	/// (<see cref="TransactSql.TransactSqlParser.Spacing"/>).
	/// </para>
	/// </remarks>
	static string Run(string words, bool raised)
	{
		var made  = new System.Text.StringBuilder(words.Length);
		var until = '\0';

		for (var at = 0; at < words.Length; at++)
		{
			var c = words[at];

			if (until != '\0')
			{
				made.Append(c);

				if (c == until)
				{
					// `]]`, `""` and `''`: the quote written twice, and still inside.
					if (at + 1 < words.Length && words[at + 1] == until)
						made.Append(words[++at]);
					else
						until = '\0';
				}

				continue;
			}

			// A block comment, the comments inside it counted, kept as it was written: a quote or a `--` inside it is
			// neither.
			if (c == '/' && at + 1 < words.Length && words[at + 1] == '*')
			{
				var depth = 0;

				for (; at < words.Length; at++)
				{
					made.Append(raised ? char.ToUpperInvariant(words[at]) : words[at]);

					if (words[at] == '/' && at + 1 < words.Length && words[at + 1] == '*')
					{
						made.Append('*');
						depth++;
						at++;
					}
					else if (words[at] == '*' && at + 1 < words.Length && words[at + 1] == '/')
					{
						made.Append('/');
						at++;

						if (--depth == 0)
							break;
					}
				}

				continue;
			}

			// A quote of any of the three kinds, and what it is closed by.
			if (c is '\'' or '"' or '[')
			{
				made.Append(c);
				until = c == '[' ? ']' : c;

				continue;
			}

			// A line comment, to the end of its line, and that end as one line end.
			if (c == '-' && at + 1 < words.Length && words[at + 1] == '-')
			{
				for (; at < words.Length && words[at] is not ('\r' or '\n'); at++)
					made.Append(raised ? char.ToUpperInvariant(words[at]) : words[at]);

				if (at < words.Length)
					made.Append('\n');

				continue;
			}

			if (TransactSql.TransactSqlParser.Spacing(c))
			{
				if (made.Length > 0 && made[made.Length - 1] is not (' ' or '\n'))
					made.Append(' ');
			}
			else
			{
				made.Append(raised ? char.ToUpperInvariant(c) : c);
			}
		}

		return made.ToString().TrimEnd();
	}

	/// <summary>Whether a key is enforced: null where nothing was said, false for <c>NOT ENFORCED</c>.</summary>
	public static bool? Enforced(string? words)
	{
		return words is null ? null : !Squared(words).StartsWith("NOT", StringComparison.Ordinal);
	}

	/// <summary>Whether a foreign key says what it does on a delete, and on an update, once each at most.</summary>
	public static bool ActedOnce(string[]? actions)
	{
		var deletes = 0;
		var updates = 0;

		foreach (var action in actions ?? [])
		{
			var squared = Squared(action);

			if (squared.StartsWith("ON DELETE ", StringComparison.Ordinal))
				deletes++;
			else if (squared.StartsWith("ON UPDATE ", StringComparison.Ordinal))
				updates++;
		}

		return deletes <= 1 && updates <= 1;
	}

	/// <summary>Whether a word that is <c>ON</c> or <c>OFF</c> was the first of the two.</summary>
	public static bool Switched(string word)
	{
		return (word[0] | 0x20) == 'o' && word.Length == 2;
	}

	/// <summary>Which join was written, from the words in front of <c>JOIN</c>.</summary>
	/// <remarks><c>OUTER</c> is noise beside <c>LEFT</c>, <c>RIGHT</c> and <c>FULL</c> (§7.7).</remarks>
	/// <summary>Whether <c>OUTER</c> was written, which is a word and not a meaning.</summary>
	public static bool Outer(string? kind)
	{
		return kind is not null && kind.EndsWith("OUTER", StringComparison.OrdinalIgnoreCase);
	}

	public static SqlJoin Joined(string? kind)
	{
		// The text is the whole of what was written, `LEFT OUTER` and not `LEFT`, so the
		// first word is what is asked about.
		if (kind is null)
			return SqlJoin.Unspecified;

		if (kind.StartsWith("LEFT", StringComparison.OrdinalIgnoreCase))
			return SqlJoin.Left;

		if (kind.StartsWith("RIGHT", StringComparison.OrdinalIgnoreCase))
			return SqlJoin.Right;

		return kind.StartsWith("FULL", StringComparison.OrdinalIgnoreCase)
			? SqlJoin.Full
			: SqlJoin.Inner;
	}

	public static string Quantified(string word)
	{
		return (word[0] | 0x20) switch
		{
			'a' => (word[1] | 0x20) == 'l' ? "ALL" : "ANY",
			's' => "SOME",
			_ => word,
		};
	}

	/// <summary>
	/// A set quantifier, as it was written.
	/// </summary>
	/// <remarks>
	/// It used to come back as one of two constants, and that was a small lie of the kind
	/// this tree is not allowed to tell: <c>SELECT ALL a</c> and <c>SELECT a</c> are two
	/// texts, and a parser that answers <c>null</c> for the first has decided something the
	/// author did not ask it to decide. What a missing <c>ALL</c> <em>means</em> is a question
	/// for whatever reads the tree.
	/// </remarks>
	public static string? Distinctly(string? word)
	{
		return word;
	}

	/// <summary>What a match predicate was qualified by, as one word or two.</summary>
	public static string? Matched(string? unique, string? kind)
	{
		var partial = kind is not null && (kind[0] | 0x20) == 'p';

		return unique is null
			? kind is null ? null : partial ? "PARTIAL" : "FULL"
			: kind is null ? "UNIQUE" : partial ? "UNIQUE PARTIAL" : "UNIQUE FULL";
	}

	/// <summary>Whether a trigger's events are of the kind its time takes: <c>INSTEAD OF</c> takes statements alone.</summary>
	public static bool Fired(string time, string[] events)
	{
		return time != "INSTEAD OF" || events.All(static one => one is "INSERT" or "UPDATE" or "DELETE");
	}

	/// <summary>Whether no word of a list is repeated.</summary>
	public static bool Distinct(string first, string[]? rest)
	{
		return Listed(first, rest).Distinct(StringComparer.Ordinal).Count() == 1 + (rest?.Length ?? 0);
	}

	/// <summary>Whether a name was written bare, rather than in brackets or quotes.</summary>
	public static bool IsBare(string? name)
	{
		return name is { Length: > 0 } && name[0] is not ('[' or '"');
	}

	/// <summary>
	/// Whether a type is one <c>JSON_VALUE</c> may return: any but the large and the special —
	/// <c>XML</c>, <c>TEXT</c>, <c>NTEXT</c>, <c>IMAGE</c>, <c>SQL_VARIANT</c>, <c>JSON</c>, a
	/// vector, the CLR types and a type of the user's — each <c>Msg 102</c>.
	/// </summary>
	public static bool JsonValued(string? type)
	{
		if (type is null || type.IndexOf('.') >= 0)
			return false;

		var end  = type.IndexOfAny([' ', '(', '\t', '\r', '\n']);
		var word = end < 0 ? type : type.Substring(0, end);

		return !Array.Exists(NotJsonValued, one => string.Equals(one, word, StringComparison.OrdinalIgnoreCase));
	}

	static readonly string[] NotJsonValued =
		["XML", "TEXT", "NTEXT", "IMAGE", "SQL_VARIANT", "JSON", "VECTOR", "GEOGRAPHY", "GEOMETRY", "HIERARCHYID"];

	/// <summary>
	/// Whether a statement's words hold no local variable: an <c>@</c> that opens a name, outside
	/// a string, and is not the <c>@@</c> of the server's own.
	/// </summary>
	public static bool WithoutVariable(string? words)
	{
		if (words is null)
			return true;

		var inString = false;

		for (var at = 0; at < words.Length; at++)
		{
			var one = words[at];

			if (one == '\'')
			{
				inString = !inString;
				continue;
			}

			if (inString || one != '@')
				continue;

			if (at + 1 < words.Length && words[at + 1] == '@')
			{
				at++;
				continue;
			}

			if (at > 0 && (char.IsLetterOrDigit(words[at - 1]) || words[at - 1] is '_' or '$' or '#' or '"' or ']'))
				continue;

			return false;
		}

		return true;
	}

	/// <summary>Whether a number is written without an exponent, as a bulk insert's counts are.</summary>
	/// <remarks>
	/// <c>BATCHSIZE = 1e3</c> is <c>Msg 102</c> where <c>100.5</c> is read. The literal holds
	/// its own exponent, so this is a question about what was read rather than about what
	/// follows it.
	/// </remarks>
	public static bool Plain(string? text)
	{
		return text is not null && text.IndexOf('e') < 0 && text.IndexOf('E') < 0;
	}

	/// <summary>Whether a type named through its schema takes the tail it was given.</summary>
	/// <remarks>
	/// <para>
	/// A dotted name is read wherever a type stands — <c>sys.int</c>, <c>sys..int</c>,
	/// <c>master.sys.int</c>, <c>[sys].[int]</c>, <c>dbo.int</c>, <c>sys.nosuchtype</c> — and
	/// what may follow it is the last part's own business. Asked of the engine one tail at a
	/// time, at every level from 100 to 170.
	/// </para>
	/// <para>
	/// <c>VARYING</c> follows <c>char</c>, <c>character</c>, <c>nchar</c> and <c>binary</c>,
	/// with a length or without: <c>sys.char varying (MAX)</c>, <c>[sys].[char] varying
	/// (10)</c>, <c>sys."Char" varying</c> are read. It follows nothing else —
	/// <c>sys.text varying</c>, <c>sys.varchar varying</c>, <c>sys.varbinary varying</c>,
	/// <c>sys.image varying</c>, <c>sys.int varying</c>, <c>sys.nosuchtype varying</c> and
	/// <c>dbo.mytype varying (10)</c> are each <c>Msg 102</c>.
	/// </para>
	/// <para>
	/// <c>NATIONAL</c> stands in front of <c>char</c>, <c>character</c> and <c>text</c>:
	/// <c>national sys.char varying (10)</c>, <c>national sys.character</c>, <c>national
	/// sys.text (10)</c>, <c>national [sys].[char]</c> are read. Before anything else it is
	/// <c>Msg 102</c> — <c>national sys.nchar</c>, <c>national sys.ntext</c>, <c>national
	/// sys.varchar</c>, <c>national sys.int</c>, <c>national dbo.mytype</c> — and
	/// <c>national sys.text varying</c> is refused as well, <c>TEXT</c> taking the word in
	/// front and not the one behind.
	/// </para>
	/// <para>
	/// Numbers in brackets are nobody's business but the length's: <c>sys.nosuchtype (10)</c>,
	/// <c>sys.nosuchtype (10, 2)</c>, <c>dbo.mytype (10, 2)</c> and <c>dbo.mytype (MAX)</c>
	/// are all read. What is in brackets and is not a number belongs to <c>xml</c> alone —
	/// <c>sys.xml (CONTENT dbo.xsd1)</c>, <c>(DOCUMENT dbo.xsd1)</c> and <c>(dbo.xsd1)</c>
	/// are read, and <c>dbo.mytype (CONTENT dbo.xsd1)</c> is <c>Msg 102</c>.
	/// </para>
	/// </remarks>
	public static bool Dotted(string? text)
	{
		if (text is null)
			return false;

		var rest     = text.TrimStart();
		var national = rest.StartsWith("NATIONAL", StringComparison.OrdinalIgnoreCase);

		if (national)
			rest = rest.Substring("NATIONAL".Length).TrimStart();

		// The name ends at the first space or bracket standing outside a quoted part, so that
		// `[sys]."Char" varying` is the name `[sys]."Char"` and the tail `varying`.
		var at    = 0;
		var until = '\0';

		while (at < rest.Length)
		{
			var one = rest[at];

			if (until != '\0')
			{
				if (one == until)
					until = '\0';
			}
			else if (one == '[')
			{
				until = ']';
			}
			else if (one == '"')
			{
				until = '"';
			}
			else if (char.IsWhiteSpace(one) || one == '(')
			{
				break;
			}

			at++;
		}

		var name = rest.Substring(0, at);
		var tail = rest.Substring(at).TrimStart();
		var dot  = name.LastIndexOf('.');
		var last = Unquoted(dot < 0 ? name : name.Substring(dot + 1));

		if (national && !Array.Exists(Nationally, one => string.Equals(one, last, StringComparison.OrdinalIgnoreCase)))
			return false;

		if (tail.Length == 0)
			return true;

		if (tail.StartsWith("VARYING", StringComparison.OrdinalIgnoreCase))
			return national
				? !string.Equals("TEXT", last, StringComparison.OrdinalIgnoreCase)
				: Array.Exists(Varyingly, one => string.Equals(one, last, StringComparison.OrdinalIgnoreCase));

		// A fixed-length string or binary is never `MAX`, however its name is written: `[char]
		// (max)`, `dbo.char (max)` and `"char" (max)` are `Msg 156` as `char (max)` is.
		if (Array.Exists(Varyingly, one => string.Equals(one, last, StringComparison.OrdinalIgnoreCase)) &&
			Array.Exists(tail.Trim('(', ')').Split(','), static piece => string.Equals(piece.Trim(), "MAX", StringComparison.OrdinalIgnoreCase)))
			return false;

		// A bracket holding anything but numbers is the xml content's.
		return Array.TrueForAll(tail.Trim('(', ')').Split(','), static piece => Numbered(piece)) ||
			   string.Equals("XML", last, StringComparison.OrdinalIgnoreCase);

		static bool Numbered(string piece)
		{
			var word = piece.Trim();

			return string.Equals(word, "MAX", StringComparison.OrdinalIgnoreCase) ||
				   word.Length > 0 && Array.TrueForAll(word.ToCharArray(), char.IsDigit);
		}
	}

	/// <summary>The types a dotted name may put <c>VARYING</c> after.</summary>
	static readonly string[] Varyingly = ["CHAR", "CHARACTER", "NCHAR", "NCHARACTER", "BINARY"];

	/// <summary>The types a dotted name may put <c>NATIONAL</c> in front of.</summary>
	static readonly string[] Nationally = ["CHAR", "CHARACTER", "TEXT"];

	/// <summary>Whether a kept sample percentage stands beside the scan it was kept from.</summary>
	/// <remarks>
	/// <c>PERSIST_SAMPLE_PERCENT</c> is <c>Msg 153</c> alone and read beside <c>FULLSCAN</c>,
	/// <c>SAMPLE … PERCENT</c>, <c>SAMPLE … ROWS</c> or <c>RESAMPLE</c>; <c>NORECOMPUTE</c>,
	/// <c>ALL</c> and <c>MAXDOP</c> will not do. Asked of the list as written, and the long name
	/// is struck out of it first: <c>SAMPLE</c> is a part of the name being asked about.
	/// </remarks>
	public static bool Sampled(string? text)
	{
		const string Kept = "PERSIST_SAMPLE_PERCENT";

		if (text is null || text.IndexOf(Kept, StringComparison.OrdinalIgnoreCase) < 0)
			return true;

		var rest = text;

		for (var at = rest.IndexOf(Kept, StringComparison.OrdinalIgnoreCase); at >= 0;
				 at = rest.IndexOf(Kept, StringComparison.OrdinalIgnoreCase))
			rest = rest.Remove(at, Kept.Length);

		return rest.IndexOf("FULLSCAN", StringComparison.OrdinalIgnoreCase) >= 0 ||
			   rest.IndexOf("SAMPLE", StringComparison.OrdinalIgnoreCase) >= 0;
	}

	/// <summary>Whether no more than one of a list's items, as written, begins with a word.</summary>
	public static bool Once(string word, string? first, string[]? rest)
	{
		var said = Begins(first) ? 1 : 0;

		foreach (var one in rest ?? [])
			if (Begins(one))
				said++;

		return said <= 1;

		bool Begins(string? one)
		{
			return one is not null && one.TrimStart().StartsWith(word, StringComparison.OrdinalIgnoreCase);
		}
	}

	/// <summary>
	/// Whether a column's number in a file has the shape of one: a whole number an <c>int</c>
	/// holds, signed or not. Zero and a negative are read by the engine and objected to after
	/// the parse (<c>Msg 16204</c>); a number past an <c>int</c> is <c>Msg 102</c>.
	/// </summary>
	public static bool Ranked(string? ordinal)
	{
		return int.TryParse(ordinal, System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out _);
	}

	/// <summary>A variable and the members named after it, joined as one name.</summary>
	public static string Member(string variable, string[]? members)
	{
		return members is null or { Length: 0 } ? variable : variable + "." + string.Join(".", members);
	}

	/// <summary>Whether a variable is one of the server's, <c>@@ROWCOUNT</c> and its kind.</summary>
	/// <remarks>
	/// They read as values and refuse to be anything else: a cursor, a row's destination or a
	/// row's number (<c>Msg 102</c>), every one of the published names put to the engine. A name
	/// the server has not heard of, <c>@@x</c>, is a variable like any other.
	/// </remarks>
	public static bool IsServerVariable(string? variable)
	{
		return variable is not null && ServerVariables.Contains(variable);
	}

	static readonly HashSet<string> ServerVariables = new(StringComparer.OrdinalIgnoreCase)
	{

		"@@CONNECTIONS", "@@CPU_BUSY", "@@CURSOR_ROWS", "@@DATEFIRST", "@@DBTS", "@@DEFAULT_LANGID",
		"@@DEF_SORTORDER_ID", "@@ERROR", "@@FETCH_STATUS", "@@IDENTITY", "@@IDLE", "@@IO_BUSY",
		"@@LANGID", "@@LANGUAGE", "@@LOCK_TIMEOUT", "@@MAX_CONNECTIONS", "@@MAX_PRECISION",
		"@@MICROSOFTVERSION", "@@NESTLEVEL", "@@OPTIONS", "@@PACKET_ERRORS", "@@PACK_RECEIVED",
		"@@PACK_SENT", "@@PROCID", "@@REMSERVER", "@@ROWCOUNT", "@@SERVERNAME", "@@SERVICENAME",
		"@@SPID", "@@TEXTSIZE", "@@TIMETICKS", "@@TOTAL_ERRORS", "@@TOTAL_READ", "@@TOTAL_WRITE",
		"@@TRANCOUNT", "@@VERSION",
	};

	/// <summary>A run of words as one upper-case word per space.</summary>
	public static string Squared(string words)
	{
		return Run(words, true);
	}

	/// <summary>A word the grammar read and this file has no record for.</summary>
	/// <remarks>
	/// The grammar and the switches in this file are two spellings of one catalogue and have to
	/// agree; where they have drifted apart this says so rather than quietly building the wrong
	/// node. A defect in this file, not in anybody's SQL.
	/// </remarks>
	public static ArgumentOutOfRangeException Unknown(string word)
	{
		return new(nameof(word), word, "The grammar reads this and the tree has no record for it.");
	}
}
