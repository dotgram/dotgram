using System;
using System.Collections.Generic;

namespace DotGram.Benchmarks;

/// <summary>
/// <see cref="HandSql"/>'s language again, read the way the generated parser reads it: a
/// lexer into kinds, and a recursive descent over those.
/// </summary>
/// <remarks>
/// <para>
/// The yardstick <see cref="HandSql"/> could not be. That one is scannerless, and the
/// generated parser is not, so a ratio between them measures the lexical split and says
/// nothing about how either parser is shaped — which was the question. This one tokenizes
/// first, so what separates it from the generated parser is the reader and only the reader.
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
/// Kinds are looked up through <see cref="Dictionary{TKey,TValue}"/>'s span lookup rather
/// than a trie, because that is what one writes: it allocates nothing, hashes the word once
/// and is over. Whether a trie would beat it is a question about this file, not about the
/// generator, and the answer would move both sides of nothing.
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
		ValuesWord = Of("VALUES"), When = Of("WHEN");

	static readonly Dictionary<string, byte> Reserved = Build();

	static Dictionary<string, byte> Build()
	{
		var table = new Dictionary<string, byte>(Words.Length, StringComparer.OrdinalIgnoreCase);

		for (var i = 0; i < Words.Length; i++)
			table[Words[i]] = (byte)(FirstWord + i);

		return table;
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

					kind = Reserved.GetAlternateLookup<ReadOnlySpan<char>>()
						.TryGetValue(s.Slice(from, p - from), out var word)
							? word
							: Identifier;

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

	static bool IsStart(char c) => char.IsLetter(c) || c == '_';
	static bool IsPart (char c) => char.IsLetter(c) || char.IsDigit(c) || c == '_';

	// ── The parser, over kinds ──────────────────────────────────────────────────

	/// <summary>Whether the whole input is a search condition.</summary>
	public static bool Parse(string text)
	{
		var tokens = Rented();

		if (!Lex(text, tokens))
			return false;

		var reader = new Reader(tokens, text);
		var end    = reader.SearchCondition(0);

		return end >= 0 && end == tokens.Count;
	}

	/// <summary>What the lexer alone costs, so the reader's share can be had by subtraction.</summary>
	public static int LexOnly(string text)
	{
		var tokens = Rented();

		return Lex(text, tokens) ? tokens.Count : -1;
	}

	readonly ref struct Reader(Tokens tokens, string text)
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

		// ── §8.12 Search condition ──────────────────────────────────────────────

		public int SearchCondition(int i) => Condition(i, 1);

		/// <remarks>
		/// <c>OR</c> and <c>AND</c> in one loop over a precedence rather than a rule each.
		/// The grammar has to write a rule per level because that is how a grammar says
		/// which binds tighter; a person writes the number down and loops, and the two
		/// accept the same language. What it saves is a call and a frame per operand per
		/// level, which is the shape of every expression ladder there is.
		/// </remarks>
		int Condition(int i, int least)
		{
			var at = BooleanFactor(i);

			if (at < 0)
				return -1;

			while (true)
			{
				var kind = Kind(at);
				var binds = kind == Or ? 1 : kind == And ? 2 : 0;

				if (binds < least)
					return at;

				var right = Condition(at + 1, binds + 1);

				if (right < 0)
					return at;

				at = right;
			}
		}

		int BooleanFactor(int i) => BooleanTest(Kind(i) == Not ? i + 1 : i);

		int BooleanTest(int i)
		{
			var at = BooleanPrimary(i);

			if (at < 0)
				return -1;

			while (Kind(at) == Is)
			{
				var next  = Kind(at + 1) == Not ? at + 2 : at + 1;
				var value = Kind(next);

				if (value != True && value != False && value != Unknown)
					return at;

				at = next + 1;
			}

			return at;
		}

		int BooleanPrimary(int i)
		{
			var at = Predicate(i);

			if (at >= 0)
				return at;

			if (Kind(i) != Open)
				return -1;

			at = SearchCondition(i + 1);

			return at < 0 ? -1 : At(at, Close);
		}

		// ── §8.1 Predicate ──────────────────────────────────────────────────────

		int Predicate(int i)
		{
			var kind = Kind(i);

			if (kind == Exists || kind == Unique)
			{
				var sub = Subquery(i + 1);

				if (sub >= 0)
					return sub;
			}

			var at = RowValueConstructor(i);

			return at < 0 ? -1 : PredicateTail(at);
		}

		int PredicateTail(int i)
		{
			var kind = Kind(i);

			if (kind is var c && (c == Eq || c == Lt || c == Gt || c == Le || c == Ge || c == Ne))
			{
				var after = i + 1;
				var many  = Kind(after);

				if (many == All || many == Some || many == Any)
				{
					var sub = Subquery(after + 1);

					if (sub >= 0)
						return sub;
				}

				return RowValueConstructor(after);
			}

			var not   = kind == Not;
			var after2 = not ? i + 1 : i;
			var second = Kind(after2);

			if (second == Between)
			{
				var low = RowValueConstructor(after2 + 1);

				if (low < 0 || Kind(low) != And)
					return -1;

				return RowValueConstructor(low + 1);
			}

			if (second == In)
				return InPredicateValue(after2 + 1);

			if (second == Like)
			{
				var pattern = ValueExpression(after2 + 1);

				if (pattern < 0)
					return -1;

				if (Kind(pattern) != Escape)
					return pattern;

				var how = ValueExpression(pattern + 1);

				return how < 0 ? pattern : how;
			}

			if (not)
				return -1;

			if (kind == Is)
			{
				var next = Kind(i + 1) == Not ? i + 2 : i + 1;

				return At(next, Null);
			}

			if (kind == Match)
			{
				var at = i + 1;

				if (Kind(at) == Unique)
					at++;

				if (Kind(at) == Partial || Kind(at) == Full)
					at++;

				return Subquery(at);
			}

			return kind == Overlaps ? RowValueConstructor(i + 1) : -1;
		}

		int InPredicateValue(int i)
		{
			var sub = Subquery(i);

			if (sub >= 0)
				return sub;

			if (Kind(i) != Open)
				return -1;

			var at = ValueExpression(i + 1);

			if (at < 0)
				return -1;

			while (Kind(at) == Comma)
			{
				var next = ValueExpression(at + 1);

				if (next < 0)
					break;

				at = next;
			}

			return At(at, Close);
		}

		// ── §7.1 Row value constructor ──────────────────────────────────────────

		int RowValueConstructor(int i)
		{
			var at = RowValueConstructorElement(i);

			if (at >= 0)
				return at;

			// Only a `(` can open a row of several or a subquery, so nothing else is tried.
			if (Kind(i) == Open)
			{
				var first = RowValueConstructorElement(i + 1);

				if (first >= 0)
				{
					var rest  = first;
					var count = 0;

					while (Kind(rest) == Comma)
					{
						var next = RowValueConstructorElement(rest + 1);

						if (next < 0)
							break;

						rest = next;
						count++;
					}

					if (count > 0 && At(rest, Close) is var close && close >= 0)
						return close;
				}
			}

			return Subquery(i);
		}

		int RowValueConstructorElement(int i)
		{
			var at = ValueExpression(i);

			if (at >= 0)
				return at;

			var kind = Kind(i);

			return kind == Null || kind == Default ? i + 1 : -1;
		}

		// ── §6.11 Value expression ──────────────────────────────────────────────

		public int ValueExpression(int i) => Value(i, 1);

		/// <remarks><c>+ - ||</c> and <c>* /</c> the same way, for the same saving.</remarks>
		int Value(int i, int least)
		{
			var at = Factor(i);

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

				var right = Value(at + 1, binds + 1);

				if (right < 0)
					return at;

				at = right;
			}
		}

		int Factor(int i)
		{
			var kind = Kind(i);

			return Primary(kind == Plus || kind == Minus ? i + 1 : i);
		}

		/// <remarks>
		/// One switch on the token standing here, which is the whole of what a person does
		/// that the grammar's eight-way ordered choice does not.
		/// </remarks>
		int Primary(int i)
		{
			var kind = Kind(i);

			if (kind == Open)
			{
				var sub = Subquery(i);

				if (sub >= 0)
					return sub;

				var inner = ValueExpression(i + 1);

				return inner < 0 ? -1 : At(inner, Close);
			}

			if (kind == Number || kind == Text)
				return i + 1;

			if (kind == Identifier)
				return QualifiedName(i);

			if (kind == Colon)
			{
				var name = At(i + 1, Identifier);

				if (name < 0)
					return -1;

				var indicator = Word(name, "INDICATOR");
				var colon     = At(indicator < 0 ? name : indicator, Colon);

				if (colon < 0)
					return name;

				var second = At(colon, Identifier);

				return second < 0 ? name : second;
			}

			if (kind == Query)
				return i + 1;

			if (kind == Case || kind == Coalesce || kind == NullIf)
				return CaseExpression(i);

			if (kind == Cast)
				return CastSpecification(i);

			if (kind == Avg || kind == Max || kind == Min || kind == Sum || kind == Count)
				return SetFunction(i);

			if (kind == User || kind == CurrentUser || kind == SessionUser ||
				kind == SystemUser || kind == ValueWord || kind == CurrentDate)
			{
				return i + 1;
			}

			if (kind == CurrentTime || kind == CurrentTimestamp)
			{
				var precision = Length(i + 1);

				return precision < 0 ? i + 1 : precision;
			}

			if (kind == Interval)
			{
				var at = Kind(i + 1) == Plus || Kind(i + 1) == Minus ? i + 2 : i + 1;

				at = At(at, Text);

				return at < 0 ? -1 : IntervalQualifier(at);
			}

			return ValueFunction(i);
		}

		// ── §6.9 Set function, §6.16-6.18 value functions ───────────────────────

		int SetFunction(int i)
		{
			var kind = Kind(i);

			if (kind == Count && Kind(i + 1) == Open && Kind(i + 2) == Star)
				return At(i + 3, Close);

			if (Kind(i + 1) != Open)
				return -1;

			var at = i + 2;

			if (Kind(at) == Distinct || Kind(at) == All)
				at++;

			var value = ValueExpression(at);

			return value < 0 ? -1 : At(value, Close);
		}

		int ValueFunction(int i)
		{
			var kind = Kind(i);

			if (kind == Position)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var what = ValueExpression(i + 2);

				if (what < 0 || Kind(what) != In)
					return -1;

				var where = ValueExpression(what + 1);

				return where < 0 ? -1 : At(where, Close);
			}

			if (kind == Extract)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var field = ExtractField(i + 2);

				if (field < 0 || Kind(field) != From)
					return -1;

				var source = ValueExpression(field + 1);

				return source < 0 ? -1 : At(source, Close);
			}

			if (kind == CharLength || kind == CharacterLength || kind == OctetLength ||
				kind == BitLength || kind == Upper || kind == Lower)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var only = ValueExpression(i + 2);

				return only < 0 ? -1 : At(only, Close);
			}

			if (kind == Substring)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var source = ValueExpression(i + 2);

				if (source < 0 || Kind(source) != From)
					return -1;

				var start = ValueExpression(source + 1);

				if (start < 0)
					return -1;

				if (Kind(start) == For)
				{
					var length = ValueExpression(start + 1);

					if (length >= 0)
						start = length;
				}

				return At(start, Close);
			}

			if (kind == Convert || kind == Translate)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var value = ValueExpression(i + 2);

				if (value < 0 || Kind(value) != Using_)
					return -1;

				var name = QualifiedName(value + 1);

				return name < 0 ? -1 : At(name, Close);
			}

			if (kind != Trim)
				return -1;

			if (Kind(i + 1) != Open)
				return -1;

			var how = Kind(i + 2);
			var at  = how == Leading || how == Trailing || how == Both ? i + 3 : i + 2;
			var one = ValueExpression(at);
			var from = Kind(one < 0 ? at : one);

			if (from == From)
			{
				var target = ValueExpression((one < 0 ? at : one) + 1);

				return target < 0 ? -1 : At(target, Close);
			}

			return one < 0 ? -1 : At(one, Close);
		}

		int ExtractField(int i)
		{
			foreach (var field in Fields)
				if (Word(i, field) is var at && at >= 0)
					return at;

			return -1;
		}

		// ── §6.9 Case, §6.10 Cast ───────────────────────────────────────────────

		int CaseExpression(int i)
		{
			var kind = Kind(i);

			if (kind == NullIf)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var left = ValueExpression(i + 2);

				if (left < 0 || Kind(left) != Comma)
					return -1;

				var right = ValueExpression(left + 1);

				return right < 0 ? -1 : At(right, Close);
			}

			if (kind == Coalesce)
			{
				if (Kind(i + 1) != Open)
					return -1;

				var one = ValueExpression(i + 2);

				if (one < 0)
					return -1;

				while (Kind(one) == Comma)
				{
					var more = ValueExpression(one + 1);

					if (more < 0)
						break;

					one = more;
				}

				return At(one, Close);
			}

			// `CASE x WHEN ...` where an operand stands before the first `WHEN`, and the
			// searched form where none does.
			var at = Kind(i + 1) == When ? i + 1 : ValueExpression(i + 1);

			if (at < 0)
				return -1;

			var searched = at == i + 1;
			var count    = 0;

			while (Kind(at) == When)
			{
				var test = searched ? SearchCondition(at + 1) : ValueExpression(at + 1);

				if (test < 0 || Kind(test) != Then)
					break;

				var result = Result(test + 1);

				if (result < 0)
					break;

				at = result;
				count++;
			}

			if (count == 0)
				return -1;

			if (Kind(at) == Else)
			{
				var otherwise = Result(at + 1);

				if (otherwise >= 0)
					at = otherwise;
			}

			return At(at, EndWord);
		}

		int Result(int i)
		{
			var at = ValueExpression(i);

			return at >= 0 ? at : At(i, Null);
		}

		int CastSpecification(int i)
		{
			if (Kind(i + 1) != Open)
				return -1;

			var operand = ValueExpression(i + 2);

			if (operand < 0)
				operand = At(i + 2, Null);

			if (operand < 0 || Kind(operand) != As)
				return -1;

			var type = DataType(operand + 1);

			return type < 0 ? -1 : At(type, Close);
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

		int Subquery(int i)
		{
			if (Kind(i) != Open)
				return -1;

			var opening = Kind(i + 1);

			if (opening != Select && opening != ValuesWord && opening != Table)
				return -1;

			var depth = 1;

			for (var at = i + 1; at < _count; at++)
			{
				if (_kinds[at] == Open)
					depth++;
				else if (_kinds[at] == Close && --depth == 0)
					return at + 1;
			}

			return -1;
		}
	}

	static readonly string[] Fields =
	[
		"YEAR", "MONTH", "DAY", "HOUR", "MINUTE", "SECOND", "TIMEZONE_HOUR", "TIMEZONE_MINUTE",
	];
}
