using System;
using System.Collections.Generic;

using DotGram.Parsers;

namespace DotGram.Benchmarks;

/// <summary>
/// <c>SqlStandard92</c>'s search condition written by hand, the way the generated parser
/// reads it: a lexer into kinds, and precedence climbing over those. The yardstick every
/// claim about what the generated parser costs is divided by.
/// </summary>
/// <remarks>
/// <para>
/// It is here because it once was not: every "so many times the hand-written parser" in
/// <c>docs/next.md</c> came from a file in a scratch directory outside the repository, and
/// the directory was cleared. This one is built by the solution, and
/// <see cref="SqlAgainst.Agree"/> holds it to the generated parser's language before
/// anything is timed.
/// </para>
/// <para>
/// It tokenizes first because the generated parser does, so what separates the two is the
/// reader and only the reader. A scannerless version was tried and retired: a ratio
/// against it measured the lexical split, not either parser's shape.
/// </para>
/// <para>
/// <b>The lexer does the same work.</b> One pass, skipping trivia, and every reserved word
/// (§5.2) becomes a kind of its own, so the parser's test for <c>AND</c> is one comparison
/// against one byte, exactly as the generated parser's is. A word that is not reserved is
/// an identifier, which is what keeps <c>INTEGER</c> usable as a column name: the grammar
/// reserves seventy-five words and no more, and the places that want an unreserved keyword
/// — <c>VARYING</c>, <c>SET</c>, <c>ZONE</c>, the datetime fields — compare the text there,
/// where nothing measured ever goes.
/// </para>
/// <para>
/// A word is classified by its length and first letter and then compared against the few
/// reserved words of that shape. It was a hashed span lookup first, and that lost to the
/// first day's parser on inputs made of names: a name is refused by its length or its
/// first letter before anything is compared, and a hash is computed regardless.
/// </para>
/// </remarks>
static class HandSqlTokens
{
	// ── The alphabet ────────────────────────────────────────────────────────────

	public const byte End = 0;

	// Punctuation and operators.
	internal const byte Open = 1, Close = 2, Comma = 3, Dot = 4, Plus = 5, Minus = 6, Star = 7,
	           Slash = 8, Eq = 9, Lt = 10, Gt = 11, Le = 12, Ge = 13, Ne = 14, Concat = 15,
	           Colon = 16, Query = 17;

	// What a run of characters became.
	internal const byte Identifier = 18, Number = 19, Text = 20;

	// The reserved words, one kind each (§5.2). Their order is the table below.
	internal const byte FirstWord = 21;

	static readonly string[] Words =
	[
		"ALL", "AND", "ANY", "AS", "AVG",
		"BETWEEN", "BIT_LENGTH", "BOTH", "BY",
		"CASE", "CAST", "CHARACTER_LENGTH", "CHAR_LENGTH", "COALESCE", "CONVERT", "COUNT",
		"CROSS", "CURRENT_DATE", "CURRENT_TIME", "CURRENT_TIMESTAMP", "CURRENT_USER",
		"DEFAULT", "DISTINCT",
		"ELSE", "END", "ESCAPE", "EXISTS", "EXTRACT",
		"FALSE", "FOR", "FROM", "FULL",
		"GROUP", "HAVING",
		"IN", "INTERVAL", "IS", "JOIN",
		"LEADING", "LIKE", "LOWER",
		"MATCH", "MAX", "MIN",
		"NOT", "NULL", "NULLIF",
		"OCTET_LENGTH", "ON", "OR", "ORDER", "OVERLAPS",
		"PARTIAL", "POSITION",
		"SELECT", "SESSION_USER", "SOME", "SUBSTRING", "SUM", "SYSTEM_USER",
		"TABLE", "THEN", "TRAILING", "TRANSLATE", "TRIM", "TRUE",
		"UNIQUE", "UNKNOWN", "UPPER", "USER", "USING",
		"VALUE", "VALUES",
		"WHEN", "WHERE",
	];

	static byte Of(string word) => (byte)(FirstWord + Array.IndexOf(Words, word));

	internal static readonly byte All = Of("ALL"), And = Of("AND"), Any = Of("ANY"), As = Of("AS"),
		Avg = Of("AVG"), Between = Of("BETWEEN"), BitLength = Of("BIT_LENGTH"),
		Both = Of("BOTH"), Case = Of("CASE"), Cast = Of("CAST"),
		CharacterLength = Of("CHARACTER_LENGTH"), CharLength = Of("CHAR_LENGTH"),
		Coalesce = Of("COALESCE"), Convert = Of("CONVERT"), Count = Of("COUNT"),
		CurrentDate = Of("CURRENT_DATE"), CurrentTime = Of("CURRENT_TIME"),
		CurrentTimestamp = Of("CURRENT_TIMESTAMP"), CurrentUser = Of("CURRENT_USER"),
		Default = Of("DEFAULT"), Distinct = Of("DISTINCT"), Else = Of("ELSE"),
		EndWord = Of("END"), Escape = Of("ESCAPE"), Exists = Of("EXISTS"),
		Extract = Of("EXTRACT"), False = Of("FALSE"), For = Of("FOR"), From = Of("FROM"),
		Full = Of("FULL"), In = Of("IN"), Interval = Of("INTERVAL"), Is = Of("IS"),
		Leading = Of("LEADING"), Like = Of("LIKE"), Lower = Of("LOWER"), Match = Of("MATCH"),
		Max = Of("MAX"), Min = Of("MIN"), Not = Of("NOT"), Null = Of("NULL"),
		NullIf = Of("NULLIF"), OctetLength = Of("OCTET_LENGTH"), Or = Of("OR"),
		Overlaps = Of("OVERLAPS"), Partial = Of("PARTIAL"), Position = Of("POSITION"),
		Select = Of("SELECT"), SessionUser = Of("SESSION_USER"), Some = Of("SOME"),
		Substring = Of("SUBSTRING"), Sum = Of("SUM"), SystemUser = Of("SYSTEM_USER"),
		Table = Of("TABLE"), Then = Of("THEN"), Trailing = Of("TRAILING"),
		Translate = Of("TRANSLATE"), Trim = Of("TRIM"), True = Of("TRUE"),
		Unique = Of("UNIQUE"), Unknown = Of("UNKNOWN"), Upper = Of("UPPER"),
		User = Of("USER"), Using_ = Of("USING"), ValueWord = Of("VALUE"),
		ValuesWord = Of("VALUES"), When = Of("WHEN"),
		// Reserved and never read here: a name may not be one, and that is all the parser asks.
		By = Of("BY"), Cross = Of("CROSS"), Group = Of("GROUP"), Having = Of("HAVING"),
		Join = Of("JOIN"), On = Of("ON"), Order = Of("ORDER"), Where = Of("WHERE");

	/// <summary>
	/// Which reserved word a run of characters is, or <see cref="Identifier"/>: by its
	/// length, then its first letter, then the few words of that shape. A name that is not
	/// a keyword is refused by its length or its first letter and compared against
	/// nothing, which is where a hashed lookup lost to the first day's parser on inputs
	/// made of names — a hash is computed whether or not anything could match.
	/// </summary>
	static byte Keyword(ReadOnlySpan<char> w)
	{
		switch (w.Length)
		{
			case 2:
				switch (w[0] | 0x20)
				{
					case 'a': return Same(w, "AS") ? As : Identifier;
					case 'b': return Same(w, "BY") ? By : Identifier;
					case 'i':
						if (Same(w, "IN")) return In;
						if (Same(w, "IS")) return Is;
						break;
					case 'o':
						if (Same(w, "ON")) return On;
						if (Same(w, "OR")) return Or;
						break;
				}

				break;
			case 3:
				switch (w[0] | 0x20)
				{
					case 'a':
						if (Same(w, "ALL")) return All;
						if (Same(w, "AND")) return And;
						if (Same(w, "ANY")) return Any;
						if (Same(w, "AVG")) return Avg;
						break;
					case 'e': return Same(w, "END") ? EndWord : Identifier;
					case 'f': return Same(w, "FOR") ? For : Identifier;
					case 'm':
						if (Same(w, "MAX")) return Max;
						if (Same(w, "MIN")) return Min;
						break;
					case 'n': return Same(w, "NOT") ? Not : Identifier;
					case 's': return Same(w, "SUM") ? Sum : Identifier;
				}

				break;
			case 4:
				switch (w[0] | 0x20)
				{
					case 'b': return Same(w, "BOTH") ? Both : Identifier;
					case 'c':
						if (Same(w, "CASE")) return Case;
						if (Same(w, "CAST")) return Cast;
						break;
					case 'e': return Same(w, "ELSE") ? Else : Identifier;
					case 'f':
						if (Same(w, "FROM")) return From;
						if (Same(w, "FULL")) return Full;
						break;
					case 'j': return Same(w, "JOIN") ? Join : Identifier;
					case 'l': return Same(w, "LIKE") ? Like : Identifier;
					case 'n': return Same(w, "NULL") ? Null : Identifier;
					case 's': return Same(w, "SOME") ? Some : Identifier;
					case 't':
						if (Same(w, "THEN")) return Then;
						if (Same(w, "TRIM")) return Trim;
						if (Same(w, "TRUE")) return True;
						break;
					case 'u': return Same(w, "USER") ? User : Identifier;
					case 'w': return Same(w, "WHEN") ? When : Identifier;
				}

				break;
			case 5:
				switch (w[0] | 0x20)
				{
					case 'c':
						if (Same(w, "COUNT")) return Count;
						if (Same(w, "CROSS")) return Cross;
						break;
					case 'f': return Same(w, "FALSE") ? False : Identifier;
					case 'g': return Same(w, "GROUP") ? Group : Identifier;
					case 'l': return Same(w, "LOWER") ? Lower : Identifier;
					case 'm': return Same(w, "MATCH") ? Match : Identifier;
					case 'o': return Same(w, "ORDER") ? Order : Identifier;
					case 't': return Same(w, "TABLE") ? Table : Identifier;
					case 'u':
						if (Same(w, "UPPER")) return Upper;
						if (Same(w, "USING")) return Using_;
						break;
					case 'v': return Same(w, "VALUE") ? ValueWord : Identifier;
					case 'w': return Same(w, "WHERE") ? Where : Identifier;
				}

				break;
			case 6:
				switch (w[0] | 0x20)
				{
					case 'e':
						if (Same(w, "ESCAPE")) return Escape;
						if (Same(w, "EXISTS")) return Exists;
						break;
					case 'h': return Same(w, "HAVING") ? Having : Identifier;
					case 'n': return Same(w, "NULLIF") ? NullIf : Identifier;
					case 's': return Same(w, "SELECT") ? Select : Identifier;
					case 'u': return Same(w, "UNIQUE") ? Unique : Identifier;
					case 'v': return Same(w, "VALUES") ? ValuesWord : Identifier;
				}

				break;
			case 7:
				switch (w[0] | 0x20)
				{
					case 'b': return Same(w, "BETWEEN") ? Between : Identifier;
					case 'c': return Same(w, "CONVERT") ? Convert : Identifier;
					case 'd': return Same(w, "DEFAULT") ? Default : Identifier;
					case 'e': return Same(w, "EXTRACT") ? Extract : Identifier;
					case 'l': return Same(w, "LEADING") ? Leading : Identifier;
					case 'p': return Same(w, "PARTIAL") ? Partial : Identifier;
					case 'u': return Same(w, "UNKNOWN") ? Unknown : Identifier;
				}

				break;
			case 8:
				switch (w[0] | 0x20)
				{
					case 'c': return Same(w, "COALESCE") ? Coalesce : Identifier;
					case 'd': return Same(w, "DISTINCT") ? Distinct : Identifier;
					case 'i': return Same(w, "INTERVAL") ? Interval : Identifier;
					case 'o': return Same(w, "OVERLAPS") ? Overlaps : Identifier;
					case 'p': return Same(w, "POSITION") ? Position : Identifier;
					case 't': return Same(w, "TRAILING") ? Trailing : Identifier;
				}

				break;
			case 9:
				switch (w[0] | 0x20)
				{
					case 's': return Same(w, "SUBSTRING") ? Substring : Identifier;
					case 't': return Same(w, "TRANSLATE") ? Translate : Identifier;
				}

				break;
			case 10:
				switch (w[0] | 0x20)
				{
					case 'b': return Same(w, "BIT_LENGTH") ? BitLength : Identifier;
				}

				break;
			case 11:
				switch (w[0] | 0x20)
				{
					case 'c': return Same(w, "CHAR_LENGTH") ? CharLength : Identifier;
					case 's': return Same(w, "SYSTEM_USER") ? SystemUser : Identifier;
				}

				break;
			case 12:
				switch (w[0] | 0x20)
				{
					case 'c':
						if (Same(w, "CURRENT_DATE")) return CurrentDate;
						if (Same(w, "CURRENT_TIME")) return CurrentTime;
						if (Same(w, "CURRENT_USER")) return CurrentUser;
						break;
					case 'o': return Same(w, "OCTET_LENGTH") ? OctetLength : Identifier;
					case 's': return Same(w, "SESSION_USER") ? SessionUser : Identifier;
				}

				break;
			case 16:
				switch (w[0] | 0x20)
				{
					case 'c': return Same(w, "CHARACTER_LENGTH") ? CharacterLength : Identifier;
				}

				break;
			case 17:
				switch (w[0] | 0x20)
				{
					case 'c': return Same(w, "CURRENT_TIMESTAMP") ? CurrentTimestamp : Identifier;
				}

				break;
		}

		return Identifier;
	}

	/// <summary>
	/// The run against one keyword of the same length, folding case by setting bit five —
	/// exact for the letters and the underscore a keyword is made of, and the first
	/// character that differs ends it. The library's case-insensitive comparison was the
	/// cost on inputs made of names: a call and a table walk per candidate, to find out
	/// what the first character already said.
	/// </summary>
	static bool Same(ReadOnlySpan<char> w, string word)
	{
		for (var i = 0; i < word.Length; i++)
			if ((w[i] | 0x20) != (word[i] | 0x20))
				return false;

		return true;
	}

	// ── The lexer ───────────────────────────────────────────────────────────────

	/// <summary>One input's tokens, kept between parses the way the generated lexer keeps its own.</summary>
	public sealed class Tokens
	{
		public byte[] Kinds   = new byte[64];
		public int[]  Starts  = new int[64];
		public int[]  Lengths = new int[64];
		public int    Count;
		public bool   Stopped;

		public void Room(int length)
		{
			if (Kinds.Length >= length)
				return;

			Kinds   = new byte[length];
			Starts  = new int[length];
			Lengths = new int[length];
		}
	}

	[ThreadStatic]
	static Tokens? _tokens;

	public static Tokens Rented() => _tokens ??= new Tokens();

	/// <summary>The whole input as tokens, trivia skipped. False where a character fits nothing.</summary>
	public static bool Lex(string text, Tokens into)
	{
		var s = text.AsSpan();

		into.Room(s.Length + 1);
		into.Count   = 0;
		into.Stopped = false;

		var kinds   = into.Kinds;
		var starts  = into.Starts;
		var lengths = into.Lengths;
		var count   = 0;
		var p       = 0;

		while (true)
		{
			// Trivia: whitespace, a line comment, a block comment.
			while (p < s.Length)
			{
				var t = s[p];

				if (t is ' ' or '\t' or '\r' or '\n')
				{
					p++;
				}
				else if (t == '-' && p + 1 < s.Length && s[p + 1] == '-')
				{
					p += 2;

					while (p < s.Length && s[p] != '\n' && s[p] != '\r')
						p++;
				}
				else if (t == '/' && p + 1 < s.Length && s[p + 1] == '*')
				{
					p += 2;

					while (p + 1 < s.Length && !(s[p] == '*' && s[p + 1] == '/'))
						p++;

					p = p + 1 < s.Length ? p + 2 : s.Length;
				}
				else
				{
					break;
				}
			}

			if (p >= s.Length)
				break;

			var from = p;
			var c    = s[p];
			byte kind;

			switch (c)
			{
				case '(': kind = Open;  p++; break;
				case ')': kind = Close; p++; break;
				case ',': kind = Comma; p++; break;
				case '+': kind = Plus;  p++; break;
				case '-': kind = Minus; p++; break;
				case '*': kind = Star;  p++; break;
				case '/': kind = Slash; p++; break;
				case '=': kind = Eq;    p++; break;
				case ':': kind = Colon; p++; break;
				case '?': kind = Query; p++; break;

				case '<':
					p++;

					if (p < s.Length && s[p] == '=')      { kind = Le; p++; }
					else if (p < s.Length && s[p] == '>') { kind = Ne; p++; }
					else                                    kind = Lt;

					break;

				case '>':
					p++;

					if (p < s.Length && s[p] == '=') { kind = Ge; p++; }
					else                               kind = Gt;

					break;

				case '|':
					if (p + 1 >= s.Length || s[p + 1] != '|')
					{
						into.Stopped = true;

						return false;
					}

					kind = Concat;
					p   += 2;

					break;

				case '.':
					p++;

					if (p < s.Length && s[p] is >= '0' and <= '9')
					{
						while (p < s.Length && s[p] is >= '0' and <= '9')
							p++;

						kind = Number;
						p    = Exponent(s, p);
					}
					else
					{
						kind = Dot;
					}

					break;

				case '\'':
					p = Quoted(s, p);

					if (p < 0)
					{
						into.Stopped = true;

						return false;
					}

					kind = Text;

					break;

				case '"':
					p++;

					while (true)
					{
						if (p >= s.Length)
						{
							into.Stopped = true;

							return false;
						}

						if (s[p] == '"')
						{
							if (p + 1 < s.Length && s[p + 1] == '"') { p += 2; continue; }

							p++;

							break;
						}

						p++;
					}

					kind = Identifier;

					break;

				default:
					if (c is >= '0' and <= '9')
					{
						while (p < s.Length && s[p] is >= '0' and <= '9')
							p++;

						if (p < s.Length && s[p] == '.')
						{
							p++;

							while (p < s.Length && s[p] is >= '0' and <= '9')
								p++;
						}

						kind = Number;
						p    = Exponent(s, p);

						break;
					}

					if (!IsStart(c))
					{
						into.Stopped = true;

						return false;
					}

					p++;

					while (p < s.Length && IsPart(s[p]))
						p++;

					// `N'x'`, `B'01'`, `X'ff'` — a one-letter prefix on a quoted string.
					if (p - from == 1 && p < s.Length && s[p] == '\'' &&
						char.ToUpperInvariant(c) is 'N' or 'B' or 'X')
					{
						p = Quoted(s, p);

						if (p < 0)
						{
							into.Stopped = true;

							return false;
						}

						kind = Text;

						break;
					}

					// `_charset'x'` is one literal too.
					if (c == '_' && p < s.Length && s[p] == '\'')
					{
						p = Quoted(s, p);

						if (p < 0)
						{
							into.Stopped = true;

							return false;
						}

						kind = Text;

						break;
					}

					kind = Keyword(s.Slice(from, p - from));

					break;
			}

			kinds  [count] = kind;
			starts [count] = from;
			lengths[count] = p - from;
			count++;
		}

		kinds[count]   = End;
		starts[count]  = s.Length;
		lengths[count] = 0;
		into.Count     = count;

		return true;
	}

	static int Exponent(ReadOnlySpan<char> s, int p)
	{
		if (p >= s.Length || (s[p] != 'e' && s[p] != 'E'))
			return p;

		var at = p + 1;

		if (at < s.Length && (s[at] == '+' || s[at] == '-'))
			at++;

		if (at >= s.Length || s[at] is < '0' or > '9')
			return p;

		while (at < s.Length && s[at] is >= '0' and <= '9')
			at++;

		return at;
	}

	static int Quoted(ReadOnlySpan<char> s, int p)
	{
		p++;

		while (p < s.Length)
		{
			if (s[p] != '\'')
			{
				p++;

				continue;
			}

			if (p + 1 < s.Length && s[p + 1] == '\'')
			{
				p += 2;

				continue;
			}

			return p + 1;
		}

		return -1;
	}

	// ASCII first, which is nearly every character there is, and the Unicode question
	// only where the fast tests said no.
	static bool IsStart(char c) =>
		(uint)((c | 0x20) - 'a') < 26 || c == '_' || (c > 127 && char.IsLetter(c));

	static bool IsPart(char c) =>
		(uint)((c | 0x20) - 'a') < 26 || (uint)(c - '0') < 10 || c == '_' ||
		(c > 127 && char.IsLetterOrDigit(c));

	// ── The parser, over kinds ──────────────────────────────────────────────────

	/// <summary>The whole input as a search condition, or null where it is not one.</summary>
	public static SqlNode? Build(string text)
	{
		var tokens = Rented();

		if (!Lex(text, tokens))
			return null;

		var reader = new Reader(tokens, text);
		var end    = reader.SearchCondition(0, out var node);

		return end >= 0 && end == tokens.Count ? node : null;
	}

	/// <summary>Whether the whole input is a search condition, tree and all.</summary>
	public static bool Parse(string text) => Build(text) is not null;

	/// <summary>What a call with no arguments is handed, once rather than per call.</summary>
	static readonly SqlNode[] None = [];

	/// <summary>The nodes that are always the same node, built once.</summary>
	static readonly SqlNode NullValue    = new SqlLiteral(SqlLiteralKind.Null,      "NULL");
	static readonly SqlNode DefaultValue = new SqlLiteral(SqlLiteralKind.Default,   "DEFAULT");
	static readonly SqlNode Parameter    = new SqlLiteral(SqlLiteralKind.Parameter, "?");

	/// <summary>What the lexer alone costs, so the reader's share can be had by subtraction.</summary>
	public static int LexOnly(string text)
	{
		var tokens = Rented();

		return Lex(text, tokens) ? tokens.Count : -1;
	}

	/// <summary>
	/// The reader, over kinds, building the same tree the generated parser builds.
	/// </summary>
	/// <remarks>
	/// Every method hands back the token it stopped at, or -1, and writes what it read
	/// into an <c>out</c> parameter. That is how a person writes a recursive descent in
	/// C#: the position is the return value because every caller needs it, and the node
	/// is an argument because only some do.
	/// </remarks>
	ref struct Reader(Tokens tokens, string text)
	{
		readonly byte[] _kinds   = tokens.Kinds;
		readonly int[]  _starts  = tokens.Starts;
		readonly int[]  _lengths = tokens.Lengths;
		readonly int    _count   = tokens.Count;
		readonly string _text    = text;

		byte Kind(int i) => i < _count ? _kinds[i] : End;

		int At(int i, byte kind) => Kind(i) == kind ? i + 1 : -1;

		/// <summary>An identifier whose text is a given unreserved keyword (§5.2 reserves seventy-five).</summary>
		int Word(int i, string word) =>
			Kind(i) == Identifier &&
			_text.AsSpan(_starts[i], _lengths[i]).Equals(word.AsSpan(), StringComparison.OrdinalIgnoreCase)
				? i + 1
				: -1;

		/// <summary>The text the tokens from one to another stand on, as it was written.</summary>
		readonly string Cut(int from, int to) =>
			_text.Substring(_starts[from], _starts[to - 1] + _lengths[to - 1] - _starts[from]);

		// ── §8.12 Search condition ──────────────────────────────────────────────

		public int SearchCondition(int i, out SqlNode? node) => Condition(i, 1, out node);

		/// <remarks>
		/// <c>OR</c> and <c>AND</c> in one loop over a precedence rather than a rule each.
		/// The grammar has to write a rule per level because that is how a grammar says
		/// which binds tighter; a person writes the number down and loops, and the two
		/// accept the same language. What it saves is a call and a frame per operand per
		/// level, which is the shape of every expression ladder there is.
		/// </remarks>
		int Condition(int i, int least, out SqlNode? node)
		{
			var at = BooleanFactor(i, out node);

			if (at < 0)
				return -1;

			while (true)
			{
				var kind  = Kind(at);
				var binds = kind == Or ? 1 : kind == And ? 2 : 0;

				if (binds < least)
					return at;

				var right = Condition(at + 1, binds + 1, out var operand);

				if (right < 0)
					return at;

				node = new SqlBinary(binds == 1 ? SqlOperator.Or : SqlOperator.And, node!, operand!);
				at   = right;
			}
		}

		int BooleanFactor(int i, out SqlNode? node)
		{
			if (Kind(i) != Not)
				return BooleanTest(i, out node);

			var at = BooleanTest(i + 1, out node);

			if (at >= 0)
				node = new SqlUnary(SqlOperator.Not, node!);

			return at;
		}

		int BooleanTest(int i, out SqlNode? node)
		{
			var at = BooleanPrimary(i, out node);

			if (at < 0)
				return -1;

			while (Kind(at) == Is)
			{
				var negated = Kind(at + 1) == Not;
				var next    = negated ? at + 2 : at + 1;
				var value   = Kind(next);

				if (value != True && value != False && value != Unknown)
					return at;

				node = new SqlTruthTest(
					node!, negated,
					value == True ? SqlTruth.True : value == False ? SqlTruth.False : SqlTruth.Unknown);
				at   = next + 1;
			}

			return at;
		}

		int BooleanPrimary(int i, out SqlNode? node)
		{
			var at = Predicate(i, out node);

			if (at >= 0)
				return at;

			if (Kind(i) != Open)
				return -1;

			at = SearchCondition(i + 1, out node);

			return at < 0 ? -1 : At(at, Close);
		}

		// ── §8.1 Predicate ──────────────────────────────────────────────────────

		int Predicate(int i, out SqlNode? node)
		{
			var kind = Kind(i);

			if (kind == Exists || kind == Unique)
			{
				var sub = Subquery(i + 1, out var query);

				if (sub >= 0)
				{
					node = new SqlPredicate(
						kind == Exists ? SqlPredicateKind.Exists : SqlPredicateKind.Unique,
						false, new[] { query! });

					return sub;
				}
			}

			var at = RowValueConstructor(i, out var left);

			if (at < 0)
			{
				node = null;

				return -1;
			}

			return PredicateTail(at, left!, out node);
		}

		/// <remarks>
		/// The row on the left is handed in rather than filled in afterwards: a person
		/// reading a predicate has it in a local already, and there is no rule boundary
		/// here to stop them from passing it.
		/// </remarks>
		int PredicateTail(int i, SqlNode left, out SqlNode? node)
		{
			node = null;

			var kind = Kind(i);

			if (kind == Eq || kind == Lt || kind == Gt || kind == Le || kind == Ge || kind == Ne)
			{
				var compared = kind switch
				{
					Eq => SqlOperator.Equal,
					Ne => SqlOperator.NotEqual,
					Lt => SqlOperator.Less,
					Le => SqlOperator.LessOrEqual,
					Gt => SqlOperator.Greater,
					_  => SqlOperator.GreaterOrEqual,
				};
				var after = i + 1;
				var many  = Kind(after);

				if (many == All || many == Some || many == Any)
				{
					var sub = Subquery(after + 1, out var query);

					if (sub >= 0)
					{
						node = new SqlPredicate(
							SqlPredicateKind.Quantified, false, new[] { left, query! },
							compared, many == All ? "ALL" : many == Some ? "SOME" : "ANY");

						return sub;
					}
				}

				var end = RowValueConstructor(after, out var right);

				if (end < 0)
					return -1;

				node = new SqlPredicate(
					SqlPredicateKind.Comparison, false, new[] { left, right! }, compared);

				return end;
			}

			var negated = kind == Not;
			var word    = negated ? i + 1 : i;
			var second  = Kind(word);

			if (second == Between)
			{
				var low = RowValueConstructor(word + 1, out var lowest);

				if (low < 0 || Kind(low) != And)
					return -1;

				var high = RowValueConstructor(low + 1, out var highest);

				if (high < 0)
					return -1;

				node = new SqlPredicate(
					SqlPredicateKind.Between, negated, new[] { left, lowest!, highest! });

				return high;
			}

			if (second == In)
			{
				var end = InPredicateValue(word + 1, out var values);

				if (end < 0)
					return -1;

				node = new SqlPredicate(SqlPredicateKind.In, negated, new[] { left, values! });

				return end;
			}

			if (second == Like)
			{
				var end = ValueExpression(word + 1, out var pattern);

				if (end < 0)
					return -1;

				if (Kind(end) == Escape)
				{
					var how = ValueExpression(end + 1, out var escape);

					if (how >= 0)
					{
						node = new SqlPredicate(
							SqlPredicateKind.Like, negated, new[] { left, pattern!, escape! });

						return how;
					}
				}

				node = new SqlPredicate(SqlPredicateKind.Like, negated, new[] { left, pattern! });

				return end;
			}

			if (negated)
				return -1;

			if (kind == Is)
			{
				var no   = Kind(i + 1) == Not;
				var next = no ? i + 2 : i + 1;
				var end  = At(next, Null);

				if (end < 0)
					return -1;

				node = new SqlPredicate(SqlPredicateKind.IsNull, no, new[] { left });

				return end;
			}

			if (kind == Match)
			{
				var at     = i + 1;
				var unique = Kind(at) == Unique;

				if (unique)
					at++;

				var partial = Kind(at) == Partial;
				var full    = Kind(at) == Full;

				if (partial || full)
					at++;

				var end = Subquery(at, out var query);

				if (end < 0)
					return -1;

				node = new SqlPredicate(
					SqlPredicateKind.Match, false, new[] { left, query! }, null,
					unique
						? partial ? "UNIQUE PARTIAL" : full ? "UNIQUE FULL" : "UNIQUE"
						: partial ? "PARTIAL" : full ? "FULL" : null);

				return end;
			}

			if (kind != Overlaps)
				return -1;

			var over = RowValueConstructor(i + 1, out var other);

			if (over < 0)
				return -1;

			node = new SqlPredicate(SqlPredicateKind.Overlaps, false, new[] { left, other! });

			return over;
		}

		int InPredicateValue(int i, out SqlNode? node)
		{
			var sub = Subquery(i, out node);

			if (sub >= 0)
				return sub;

			if (Kind(i) != Open)
				return -1;

			var at = ValueExpression(i + 1, out var first);

			if (at < 0)
				return -1;

			var values = new List<SqlNode> { first! };

			while (Kind(at) == Comma)
			{
				var next = ValueExpression(at + 1, out var more);

				if (next < 0)
					break;

				values.Add(more!);
				at = next;
			}

			var close = At(at, Close);

			if (close < 0)
				return -1;

			node = new SqlRow(values.ToArray());

			return close;
		}

		// ── §7.1 Row value constructor ──────────────────────────────────────────

		int RowValueConstructor(int i, out SqlNode? node)
		{
			var at = RowValueConstructorElement(i, out node);

			if (at >= 0)
				return at;

			// Only a `(` can open a row of several or a subquery, so nothing else is tried.
			if (Kind(i) == Open)
			{
				var first = RowValueConstructorElement(i + 1, out var head);

				if (first >= 0 && Kind(first) == Comma)
				{
					var values = new List<SqlNode> { head! };
					var rest   = first;

					while (Kind(rest) == Comma)
					{
						var next = RowValueConstructorElement(rest + 1, out var more);

						if (next < 0)
							break;

						values.Add(more!);
						rest = next;
					}

					var close = At(rest, Close);

					if (values.Count > 1 && close >= 0)
					{
						node = new SqlRow(values.ToArray());

						return close;
					}
				}
			}

			return Subquery(i, out node);
		}

		int RowValueConstructorElement(int i, out SqlNode? node)
		{
			var at = ValueExpression(i, out node);

			if (at >= 0)
				return at;

			var kind = Kind(i);

			if (kind == Null)
			{
				node = NullValue;

				return i + 1;
			}

			if (kind != Default)
				return -1;

			node = DefaultValue;

			return i + 1;
		}

		// ── §6.11 Value expression ──────────────────────────────────────────────

		public int ValueExpression(int i, out SqlNode? node) => Value(i, 1, out node);

		/// <remarks><c>+ - ||</c> and <c>* /</c> the same way, for the same saving.</remarks>
		int Value(int i, int least, out SqlNode? node)
		{
			var at = Factor(i, out node);

			if (at < 0)
				return -1;

			while (true)
			{
				var kind  = Kind(at);
				var binds = kind == Plus || kind == Minus || kind == Concat ? 1
					: kind == Star || kind == Slash ? 2
					: 0;

				if (binds < least)
					return at;

				var right = Value(at + 1, binds + 1, out var operand);

				if (right < 0)
					return at;

				node = new SqlBinary(
					kind switch
					{
						Plus   => SqlOperator.Add,
						Minus  => SqlOperator.Subtract,
						Concat => SqlOperator.Concatenate,
						Star   => SqlOperator.Multiply,
						_      => SqlOperator.Divide,
					},
					node!, operand!);
				at = right;
			}
		}

		int Factor(int i, out SqlNode? node)
		{
			var kind = Kind(i);

			if (kind != Plus && kind != Minus)
				return Primary(i, out node);

			var at = Primary(i + 1, out node);

			if (at >= 0)
				node = new SqlUnary(kind == Minus ? SqlOperator.Negate : SqlOperator.Identity, node!);

			return at;
		}

		/// <remarks>
		/// One switch on the token standing here, which is the whole of what a person does
		/// that the grammar's eight-way ordered choice does not.
		/// </remarks>
		int Primary(int i, out SqlNode? node)
		{
			node = null;

			var kind = Kind(i);

			if (kind == Open)
			{
				var sub = Subquery(i, out node);

				if (sub >= 0)
					return sub;

				var inner = ValueExpression(i + 1, out node);

				return inner < 0 ? -1 : At(inner, Close);
			}

			if (kind == Number || kind == Text)
			{
				node = new SqlLiteral(
					kind == Number ? SqlLiteralKind.Number : SqlLiteralKind.Text, Cut(i, i + 1));

				return i + 1;
			}

			if (kind == Identifier)
			{
				var at = QualifiedName(i);

				if (at < 0)
					return -1;

				node = new SqlName(Cut(i, at));

				return at;
			}

			if (kind == Colon)
			{
				var name = At(i + 1, Identifier);

				if (name < 0)
					return -1;

				var indicator = Word(name, "INDICATOR");
				var colon     = At(indicator < 0 ? name : indicator, Colon);
				var at        = name;

				if (colon >= 0 && At(colon, Identifier) is var second && second >= 0)
					at = second;

				node = new SqlLiteral(SqlLiteralKind.Parameter, Cut(i, at));

				return at;
			}

			if (kind == Query)
			{
				node = Parameter;

				return i + 1;
			}

			if (kind == Case || kind == Coalesce || kind == NullIf)
				return CaseExpression(i, out node);

			if (kind == Cast)
				return CastSpecification(i, out node);

			if (kind == Avg || kind == Max || kind == Min || kind == Sum || kind == Count)
				return SetFunction(i, out node);

			if (kind == User || kind == CurrentUser || kind == SessionUser ||
				kind == SystemUser || kind == ValueWord)
			{
				node = new SqlLiteral(SqlLiteralKind.Special, Cut(i, i + 1));

				return i + 1;
			}

			if (kind == CurrentDate)
			{
				node = new SqlCall("CURRENT_DATE", None);

				return i + 1;
			}

			if (kind == CurrentTime || kind == CurrentTimestamp)
			{
				var name      = kind == CurrentTime ? "CURRENT_TIME" : "CURRENT_TIMESTAMP";
				var precision = Length(i + 1);

				node = new SqlCall(name, None, precision < 0 ? null : Cut(i + 2, i + 3));

				return precision < 0 ? i + 1 : precision;
			}

			if (kind == Interval)
			{
				var at = Kind(i + 1) == Plus || Kind(i + 1) == Minus ? i + 2 : i + 1;

				at = At(at, Text);

				if (at < 0)
					return -1;

				var end = IntervalQualifier(at);

				if (end < 0)
					return -1;

				node = new SqlLiteral(SqlLiteralKind.Interval, Cut(i, end));

				return end;
			}

			return ValueFunction(i, out node);
		}

		// ── §6.9 Set function, §6.16-6.18 value functions ───────────────────────

		int SetFunction(int i, out SqlNode? node)
		{
			node = null;

			var kind = Kind(i);

			if (kind == Count && Kind(i + 1) == Open && Kind(i + 2) == Star)
			{
				var star = At(i + 3, Close);

				if (star < 0)
					return -1;

				node = new SqlCall("COUNT", None, "*");

				return star;
			}

			if (Kind(i + 1) != Open)
				return -1;

			var at         = i + 2;
			var quantifier = Kind(at);
			var distinctly = quantifier == Distinct ? "DISTINCT" : quantifier == All ? "ALL" : null;

			if (distinctly is not null)
				at++;

			var value = ValueExpression(at, out var argument);

			if (value < 0)
				return -1;

			var close = At(value, Close);

			if (close < 0)
				return -1;

			node = new SqlCall(
				kind == Avg ? "AVG" : kind == Max ? "MAX" : kind == Min ? "MIN" : kind == Sum ? "SUM" : "COUNT",
				new[] { argument! },
				distinctly);

			return close;
		}

		int ValueFunction(int i, out SqlNode? node)
		{
			node = null;

			var kind = Kind(i);

			if (kind == Position)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var what = ValueExpression(i + 2, out var needle);

				if (what < 0 || Kind(what) != In)
					return -1;

				var where = ValueExpression(what + 1, out var haystack);

				if (where < 0)
					return -1;

				var close = At(where, Close);

				if (close < 0)
					return -1;

				node = new SqlCall("POSITION", new[] { needle!, haystack! });

				return close;
			}

			if (kind == Extract)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var field = ExtractField(i + 2);

				if (field < 0 || Kind(field) != From)
					return -1;

				var source = ValueExpression(field + 1, out var from);

				if (source < 0)
					return -1;

				var close = At(source, Close);

				if (close < 0)
					return -1;

				node = new SqlCall("EXTRACT", new[] { from! }, Cut(i + 2, field));

				return close;
			}

			if (kind == CharLength || kind == CharacterLength || kind == OctetLength ||
				kind == BitLength || kind == Upper || kind == Lower)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var only = ValueExpression(i + 2, out var argument);

				if (only < 0)
					return -1;

				var close = At(only, Close);

				if (close < 0)
					return -1;

				node = new SqlCall(
					kind == CharLength ? "CHAR_LENGTH"
						: kind == CharacterLength ? "CHARACTER_LENGTH"
						: kind == OctetLength ? "OCTET_LENGTH"
						: kind == BitLength ? "BIT_LENGTH"
						: kind == Upper ? "UPPER" : "LOWER",
					new[] { argument! });

				return close;
			}

			if (kind == Substring)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var source = ValueExpression(i + 2, out var subject);

				if (source < 0 || Kind(source) != From)
					return -1;

				var start = ValueExpression(source + 1, out var from);

				if (start < 0)
					return -1;

				SqlNode? length = null;

				if (Kind(start) == For)
				{
					var counted = ValueExpression(start + 1, out length);

					if (counted >= 0)
						start = counted;
				}

				var close = At(start, Close);

				if (close < 0)
					return -1;

				node = new SqlCall(
					"SUBSTRING",
					length is null ? new[] { subject!, from! } : new[] { subject!, from!, length });

				return close;
			}

			if (kind == Convert || kind == Translate)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var value = ValueExpression(i + 2, out var argument);

				if (value < 0 || Kind(value) != Using_)
					return -1;

				var name = QualifiedName(value + 1);

				if (name < 0)
					return -1;

				var close = At(name, Close);

				if (close < 0)
					return -1;

				node = new SqlCall(
					kind == Convert ? "CONVERT" : "TRANSLATE", new[] { argument! },
					Cut(value + 1, name));

				return close;
			}

			if (kind != Trim || Kind(i + 1) != Open)
				return -1;

			var how     = Kind(i + 2);
			var trimmed = how == Leading || how == Trailing || how == Both;
			var at      = trimmed ? i + 3 : i + 2;
			var one     = ValueExpression(at, out var first);
			var next    = Kind(one < 0 ? at : one);

			if (next == From)
			{
				var target = ValueExpression((one < 0 ? at : one) + 1, out var subject);

				if (target < 0)
					return -1;

				var closed = At(target, Close);

				if (closed < 0)
					return -1;

				node = new SqlCall(
					"TRIM",
					one < 0 ? new[] { subject! } : new[] { first!, subject! },
					trimmed ? Cut(i + 2, i + 3) : null);

				return closed;
			}

			if (one < 0)
				return -1;

			var end = At(one, Close);

			if (end < 0)
				return -1;

			node = new SqlCall("TRIM", new[] { first! });

			return end;
		}

		int ExtractField(int i)
		{
			foreach (var field in Fields)
				if (Word(i, field) is var at && at >= 0)
					return at;

			return -1;
		}

		// ── §6.9 Case, §6.10 Cast ───────────────────────────────────────────────

		int CaseExpression(int i, out SqlNode? node)
		{
			node = null;

			var kind = Kind(i);

			if (kind == NullIf)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var left = ValueExpression(i + 2, out var first);

				if (left < 0 || Kind(left) != Comma)
					return -1;

				var right = ValueExpression(left + 1, out var second);

				if (right < 0)
					return -1;

				var close = At(right, Close);

				if (close < 0)
					return -1;

				node = new SqlCall("NULLIF", new[] { first!, second! });

				return close;
			}

			if (kind == Coalesce)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var one = ValueExpression(i + 2, out var first);

				if (one < 0)
					return -1;

				var values = new List<SqlNode> { first! };

				while (Kind(one) == Comma)
				{
					var more = ValueExpression(one + 1, out var next);

					if (more < 0)
						break;

					values.Add(next!);
					one = more;
				}

				var close = At(one, Close);

				if (close < 0)
					return -1;

				node = new SqlCall("COALESCE", values.ToArray());

				return close;
			}

			// `CASE x WHEN ...` where an operand stands before the first `WHEN`, and the
			// searched form where none does.
			SqlNode? operand = null;

			var at       = Kind(i + 1) == When ? i + 1 : ValueExpression(i + 1, out operand);
			var searched = at == i + 1;

			if (at < 0)
				return -1;

			var whens = new List<SqlWhen>();

			while (Kind(at) == When)
			{
				var test = searched
					? SearchCondition(at + 1, out var asked)
					: ValueExpression(at + 1, out asked);

				if (test < 0 || Kind(test) != Then)
					break;

				var result = Result(test + 1, out var answer);

				if (result < 0)
					break;

				whens.Add(new SqlWhen(asked!, answer!));
				at = result;
			}

			if (whens.Count == 0)
				return -1;

			SqlNode? otherwise = null;

			if (Kind(at) == Else)
			{
				var last = Result(at + 1, out otherwise);

				if (last >= 0)
					at = last;
			}

			var end = At(at, EndWord);

			if (end < 0)
				return -1;

			node = new SqlCase(operand, whens.ToArray(), otherwise);

			return end;
		}

		int Result(int i, out SqlNode? node)
		{
			var at = ValueExpression(i, out node);

			if (at >= 0)
				return at;

			var end = At(i, Null);

			node = end < 0 ? null : NullValue;

			return end;
		}

		int CastSpecification(int i, out SqlNode? node)
		{
			node = null;

			if (Kind(i + 1) != Open)
				return -1;

			var operand = ValueExpression(i + 2, out var value);

			if (operand < 0)
			{
				operand = At(i + 2, Null);
				value   = NullValue;
			}

			if (operand < 0 || Kind(operand) != As)
				return -1;

			var type = DataType(operand + 1);

			if (type < 0)
				return -1;

			var close = At(type, Close);

			if (close < 0)
				return -1;

			node = new SqlCall("CAST", new[] { value! }, Cut(operand + 1, type));

			return close;
		}

		// ── §6.1 Data type ──────────────────────────────────────────────────────

		int DataType(int i)
		{
			// Every type name is an unreserved word, so this is where the text is read.
			if (Kind(i) != Identifier)
				return Kind(i) == Interval ? IntervalQualifier(i + 1) : -1;

			var at = i + 1;

			if (Word(i, "VARCHAR") >= 0)
				return Charset(Length(at) is var v && v >= 0 ? v : at);

			if (Word(i, "CHARACTER") >= 0 || Word(i, "CHAR") >= 0)
			{
				if (Word(at, "VARYING") is var varying && varying >= 0)
					at = varying;

				return Charset(Length(at) is var l && l >= 0 ? l : at);
			}

			if (Word(i, "NCHAR") >= 0)
			{
				if (Word(at, "VARYING") is var varying && varying >= 0)
					at = varying;

				return Length(at) is var l && l >= 0 ? l : at;
			}

			if (Word(i, "NATIONAL") >= 0)
			{
				if (Word(at, "CHARACTER") < 0 && Word(at, "CHAR") < 0)
					return -1;

				at++;

				if (Word(at, "VARYING") is var varying && varying >= 0)
					at = varying;

				return Length(at) is var l && l >= 0 ? l : at;
			}

			if (Word(i, "BIT") >= 0)
			{
				if (Word(at, "VARYING") is var varying && varying >= 0)
					at = varying;

				return Length(at) is var l && l >= 0 ? l : at;
			}

			if (Word(i, "NUMERIC") >= 0 || Word(i, "DECIMAL") >= 0 || Word(i, "DEC") >= 0)
				return Scale(at) is var scale && scale >= 0 ? scale : at;

			if (Word(i, "INTEGER") >= 0 || Word(i, "INT") >= 0 || Word(i, "SMALLINT") >= 0 ||
				Word(i, "REAL") >= 0 || Word(i, "DATE") >= 0)
			{
				return at;
			}

			if (Word(i, "FLOAT") >= 0)
				return Length(at) is var l && l >= 0 ? l : at;

			if (Word(i, "DOUBLE") >= 0)
				return Word(at, "PRECISION");

			if (Word(i, "TIME") >= 0 || Word(i, "TIMESTAMP") >= 0)
			{
				if (Length(at) is var l && l >= 0)
					at = l;

				if (Word(at, "WITH") is var with && with >= 0 &&
					Word(with, "TIME") is var time && time >= 0 &&
					Word(time, "ZONE") is var zone && zone >= 0)
				{
					at = zone;
				}

				return at;
			}

			return -1;
		}

		int Charset(int i)
		{
			if (Word(i, "CHARACTER") is var set && set >= 0 &&
				Word(set, "SET") is var setAt && setAt >= 0 &&
				QualifiedName(setAt) is var name && name >= 0)
			{
				return name;
			}

			return i;
		}

		int Length(int i)
		{
			if (Kind(i) != Open || Kind(i + 1) != Number)
				return -1;

			return At(i + 2, Close);
		}

		int Scale(int i)
		{
			if (Kind(i) != Open || Kind(i + 1) != Number)
				return -1;

			var at = i + 2;

			if (Kind(at) == Comma && Kind(at + 1) == Number)
				at += 2;

			return At(at, Close);
		}

		int IntervalQualifier(int i)
		{
			var at = SingleDatetimeField(i);

			if (at < 0)
				return -1;

			if (Word(at, "TO") is var to && to >= 0 && SingleDatetimeField(to) is var second && second >= 0)
				at = second;

			return at;
		}

		int SingleDatetimeField(int i)
		{
			var at = ExtractField(i);

			if (at < 0)
				return -1;

			return Scale(at) is var scale && scale >= 0 ? scale : at;
		}

		// ── §6.4 Column reference, and the query seam ───────────────────────────

		int QualifiedName(int i)
		{
			var at = At(i, Identifier);

			if (at < 0)
				return -1;

			while (Kind(at) == Dot && Kind(at + 1) == Identifier)
				at += 2;

			return at;
		}

		int Subquery(int i, out SqlNode? node)
		{
			node = null;

			if (Kind(i) != Open)
				return -1;

			var opening = Kind(i + 1);

			if (opening != Select && opening != ValuesWord && opening != Table)
				return -1;

			var depth = 1;

			for (var at = i + 1; at < _count; at++)
			{
				if (_kinds[at] == Open)
				{
					depth++;
				}
				else if (_kinds[at] == Close && --depth == 0)
				{
					node = new SqlSubquery(Cut(i, at + 1));

					return at + 1;
				}
			}

			return -1;
		}
	}

	static readonly string[] Fields =
	[
		"YEAR", "MONTH", "DAY", "HOUR", "MINUTE", "SECOND", "TIMEZONE_HOUR", "TIMEZONE_MINUTE",
	];
}
