using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace DotGram.Handwritten;

/// <summary>What a token of ISO/IEC 9075-2:2023 §5.2 is.</summary>
/// <remarks>
/// A literal keeps the kind its introducing letter gave it, because that is what the tree asks
/// for; everything else that is not a word is punctuation, one kind per spelling, so that the
/// parser's test for a comma is one comparison.
/// </remarks>
enum SqlTokenKind : byte
{
	/// <summary>Nothing has been read yet. Never handed to the parser.</summary>
	Pending,

	/// <summary>The input ended.</summary>
	End,

	/// <summary>A word: a reserved word where <see cref="SqlToken.Word"/> says which, a name otherwise.</summary>
	Word,

	/// <summary>A delimited identifier, <c>"a"</c>.</summary>
	Delimited,

	/// <summary>A Unicode delimited identifier, <c>U&amp;"a"</c>, with its escape where one was named.</summary>
	UnicodeName,

	/// <summary>An unsigned numeric literal, in any radix.</summary>
	Number,

	/// <summary>A character string literal, with its introducer where one was written.</summary>
	String,

	/// <summary>A national character string literal, <c>N'a'</c>.</summary>
	National,

	/// <summary>A Unicode character string literal, <c>U&amp;'a'</c>.</summary>
	UnicodeString,

	/// <summary>A binary string literal, <c>X'0A'</c>.</summary>
	Binary,

	LeftParen, RightParen, Comma, Dot, Semicolon, Colon, DoubleColon,
	Equal, Less, Greater, LessOrEqual, GreaterOrEqual, NotEqual,
	Plus, Minus, Asterisk, Solidus, Percent, Concat, Arrow, Ampersand,
	LeftBracket, RightBracket, LeftBrace, RightBrace, LeftBraceMinus, MinusRightBrace,
	Question,

	/// <summary>The trigraph brackets <c>??(</c> and <c>??)</c>, which §5.2 makes one token each.</summary>
	LeftTrigraph, RightTrigraph,

	/// <summary>Something that begins no token, or a token the lexical rules do not finish.</summary>
	Unknown,
}

/// <summary>One token: where it is, what it is, and what a literal keeps beyond its bounds.</summary>
struct SqlToken
{
	public SqlTokenKind Kind;

	/// <summary>Which reserved word a <see cref="SqlTokenKind.Word"/> is, or <see cref="SqlWord.Name"/>.</summary>
	public SqlWord Word;

	/// <summary>The first character of the token.</summary>
	public int Start;

	/// <summary>One past its last character.</summary>
	public int End;

	/// <summary>A literal's first quote, or -1 where it has none.</summary>
	public int Quote;

	/// <summary>
	/// One past a literal's last quote: the same as <see cref="End"/> unless a <c>UESCAPE</c>
	/// followed it, which the token holds and the literal's text does not.
	/// </summary>
	public int Body;

	/// <summary>
	/// How many quoted runs a string literal was written in: <c>'a' 'b'</c> is two. A date, a
	/// time and an interval string are one run, and the parser asks this to refuse the rest.
	/// </summary>
	public int Parts;

	/// <summary>
	/// Whether an identifier part stands immediately before the token, which only happens where
	/// no separator divides it from a number: the <c>OCTETS</c> of <c>198OCTETS</c>.
	/// </summary>
	/// <remarks>
	/// §5.2 makes a key word a whole word, and guards it on both sides: a word that runs into what
	/// stands before it is no key word, though it is still a name. So a glued word is refused by
	/// <see cref="SqlCursor.Take(SqlWord)"/> and read by <c>Identifier</c> all the same, which is
	/// what lets <c>2K</c> be a length and its multiplier.
	/// </remarks>
	public bool Glued;
}

/// <summary>
/// The text, where the reading is, and the token in front of it: §5.2's tokens, made one at a
/// time as the parser asks for them.
/// </summary>
/// <remarks>
/// <para>
/// There is no array of tokens. The generated parser reads the characters where it stands and
/// keeps nothing of what it passed, so a token array here would be a cost on one side of the
/// comparison and not the other — and a parser written by hand for a language this size would
/// not pay it either, when one token of lookahead is all the reading needs.
/// </para>
/// <para>
/// Everything a copy of this struct holds is a value, so the parser saves a position by copying
/// it and goes back by copying it back. That is what an ordered choice does between alternatives,
/// and it costs no allocation and no re-reading of the token already in hand.
/// </para>
/// </remarks>
struct SqlCursor
{
	public readonly string Text;

	/// <summary>Where the token in front of the reading begins to be looked for.</summary>
	int _next;

	SqlToken _token;

	public SqlCursor(string text)
	{
		Text   = text;
		_next  = 0;
		_token = new SqlToken { Kind = SqlTokenKind.Pending };
	}

	// ── What is in front ───────────────────────────────────────────────────────

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	void Ensure()
	{
		if (_token.Kind == SqlTokenKind.Pending)
			Lex();
	}

	public SqlTokenKind Kind
	{
		get { Ensure(); return _token.Kind; }
	}

	public SqlWord Word
	{
		get { Ensure(); return _token.Word; }
	}

	public SqlToken Token
	{
		get { Ensure(); return _token; }
	}

	public bool AtEnd
	{
		get { Ensure(); return _token.Kind == SqlTokenKind.End; }
	}

	/// <summary>The token's characters.</summary>
	public ReadOnlySpan<char> Span
	{
		get { Ensure(); return Text.AsSpan(_token.Start, _token.End - _token.Start); }
	}

	// ── Taking it ──────────────────────────────────────────────────────────────

	/// <summary>Consumes the token in front, whatever it is.</summary>
	public void Take()
	{
		Ensure();

		_next       = _token.End;
		_token.Kind = SqlTokenKind.Pending;
	}

	/// <summary>Takes the reserved word, or leaves the reading where it was.</summary>
	public bool Take(SqlWord word)
	{
		Ensure();

		if (_token.Kind != SqlTokenKind.Word || _token.Word != word || _token.Glued)
			return false;

		Take();

		return true;
	}

	/// <summary>Takes a token of that kind, or leaves the reading where it was.</summary>
	public bool Take(SqlTokenKind kind)
	{
		Ensure();

		if (_token.Kind != kind)
			return false;

		Take();

		return true;
	}

	/// <summary>
	/// Takes a word that §5.2 does not reserve — <c>VARYING</c>, <c>ZONE</c>, <c>OCTETS</c> — by
	/// its spelling. A reserved one is taken by <see cref="SqlWord"/> instead, which compares an
	/// integer where this compares characters.
	/// </summary>
	public bool TakeWord(string spelling)
	{
		Ensure();

		if (_token.Kind != SqlTokenKind.Word || _token.Glued || !Span.Equals(spelling, StringComparison.OrdinalIgnoreCase))
			return false;

		Take();

		return true;
	}

	/// <summary>Whether the word in front is that unreserved word, without taking it.</summary>
	public bool IsWord(string spelling)
	{
		Ensure();

		return _token.Kind == SqlTokenKind.Word && !_token.Glued && Span.Equals(spelling, StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>The text of a token, as the tree keeps it.</summary>
	public readonly string TextOf(in SqlToken token) => Text.Substring(token.Start, token.End - token.Start);

	// ── §5.2 Separators ────────────────────────────────────────────────────────

	/// <summary>Past the comments and whitespace that begin at <paramref name="at"/>.</summary>
	int Trivia(int at) => Trivia(at, false);

	/// <summary>
	/// The same, with every bracketed comment closed at the first <c>*/</c> instead of at the one a
	/// nested reading finds.
	/// </summary>
	/// <remarks>
	/// Where a comment closes is a choice, and the BNF makes it inside a rule the parser may go back
	/// into: a separator that leaves no literal to read is read again the other way. Only a comment
	/// left open makes the two differ — `/* a /* b */ c */ '...'` reads the same either way — so the
	/// second reading is asked for only where the first one fails, and real SQL never asks.
	/// </remarks>
	int Trivia(int at, bool earliest)
	{
		var text = Text;

		while (at < text.Length)
		{
			var ch = text[at];

			if (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r' || char.IsWhiteSpace(ch))
			{
				at++;
			}
			else if (ch == '-' && at + 1 < text.Length && text[at + 1] == '-')
			{
				// A simple comment runs to the end of its line, and the end of the text ends
				// the last line as well as a line break would.
				at += 2;

				while (at < text.Length && text[at] != '\n' && text[at] != '\r' && text[at] != '\u0085' && text[at] != '\u2028' && text[at] != '\u2029')
					at++;
			}
			else if (ch == '/' && at + 1 < text.Length && text[at + 1] == '*')
			{
				var end = earliest ? Plain(at) : Bracketed(at);

				// A comment that closes nowhere is no separator, and the reading stops in front of
				// it rather than passing over it.
				if (end < 0)
					return at;

				at = end;
			}
			else
			{
				break;
			}
		}

		return at;
	}

	/// <summary>Past a bracketed comment, or -1 where it closes nowhere.</summary>
	/// <remarks>
	/// §5.2 writes the contents of a bracketed comment as <c>(&lt;bracketed comment&gt; | any
	/// character that does not begin <c>*/</c>)*</c>, so <c>/* a /* b */ c */</c> is one comment —
	/// and so is <c>/* a /* b * c */</c>, where reading the inner one as a comment leaves the outer
	/// unclosed and the choice takes its other branch instead. The two branches are tried in that
	/// order here, which is the order the BNF writes them in.
	/// </remarks>
	readonly int Bracketed(int at) => Contents(at + 2);

	/// <summary>Past a bracketed comment closed at the first <c>*/</c> in it, or -1.</summary>
	readonly int Plain(int at)
	{
		var text = Text;

		for (at += 2; at + 1 < text.Length; at++)
			if (text[at] == '*' && text[at + 1] == '/')
				return at + 2;

		return -1;
	}

	/// <summary>Past the contents of a comment and its closing <c>*/</c>, or -1.</summary>
	readonly int Contents(int at)
	{
		var text = Text;

		while (at < text.Length)
		{
			if (text[at] == '*' && at + 1 < text.Length && text[at + 1] == '/')
				return at + 2;

			// A comment inside the comment, where reading it as one lets this comment close.
			if (text[at] == '/' && at + 1 < text.Length && text[at + 1] == '*')
			{
				var inner = Contents(at + 2);

				if (inner >= 0)
				{
					var rest = Contents(inner);

					if (rest >= 0)
						return rest;
				}
			}

			at++;
		}

		return -1;
	}

	// ── §5.2 Tokens ────────────────────────────────────────────────────────────

	void Lex()
	{
		var text = Text;
		var at   = Trivia(_next);

		_token.Start = at;
		_token.End   = at;
		_token.Word  = SqlWord.Name;
		_token.Quote = -1;
		_token.Body  = at;
		_token.Parts = 0;
		_token.Glued = false;

		if (at >= text.Length)
		{
			_token.Kind = SqlTokenKind.End;

			return;
		}

		var ch = text[at];

		switch (ch)
		{
			case '(': Punctuation(SqlTokenKind.LeftParen,  at + 1); return;
			case ')': Punctuation(SqlTokenKind.RightParen, at + 1); return;
			case ',': Punctuation(SqlTokenKind.Comma,      at + 1); return;
			case ';': Punctuation(SqlTokenKind.Semicolon,  at + 1); return;
			case '*': Punctuation(SqlTokenKind.Asterisk,   at + 1); return;
			case '/': Punctuation(SqlTokenKind.Solidus,    at + 1); return;
			case '%': Punctuation(SqlTokenKind.Percent,    at + 1); return;
			case '+': Punctuation(SqlTokenKind.Plus,       at + 1); return;
			case '&': Punctuation(SqlTokenKind.Ampersand,  at + 1); return;
			case '=': Punctuation(SqlTokenKind.Equal,      at + 1); return;
			case '[': Punctuation(SqlTokenKind.LeftBracket,  at + 1); return;
			case ']': Punctuation(SqlTokenKind.RightBracket, at + 1); return;
			case '}': Punctuation(SqlTokenKind.RightBrace,   at + 1); return;

			case '-':
				// `-}` closes a row pattern's exclusion; `--` was trivia and never reaches here.
				Punctuation(At(at + 1) == '}' ? SqlTokenKind.MinusRightBrace : SqlTokenKind.Minus, At(at + 1) == '}' ? at + 2 : at + 1);
				return;

			case '{':
				Punctuation(At(at + 1) == '-' ? SqlTokenKind.LeftBraceMinus : SqlTokenKind.LeftBrace, At(at + 1) == '-' ? at + 2 : at + 1);
				return;

			case '<':
				switch (At(at + 1))
				{
					case '>': Punctuation(SqlTokenKind.NotEqual,      at + 2); return;
					case '=': Punctuation(SqlTokenKind.LessOrEqual,   at + 2); return;
					default : Punctuation(SqlTokenKind.Less,          at + 1); return;
				}

			case '>':
				Punctuation(At(at + 1) == '=' ? SqlTokenKind.GreaterOrEqual : SqlTokenKind.Greater, At(at + 1) == '=' ? at + 2 : at + 1);
				return;

			case '|':
				Punctuation(At(at + 1) == '|' ? SqlTokenKind.Concat : SqlTokenKind.Unknown, At(at + 1) == '|' ? at + 2 : at + 1);
				return;

			case ':':
				Punctuation(At(at + 1) == ':' ? SqlTokenKind.DoubleColon : SqlTokenKind.Colon, At(at + 1) == ':' ? at + 2 : at + 1);
				return;

			case '?':
				// `??(` and `??)` are one token each (§5.2), so a question mark that begins one
				// is not a question mark of its own: `(A??)` is refused where `(A? ?)` is not.
				if (At(at + 1) == '?' && (At(at + 2) == '(' || At(at + 2) == ')'))
				{
					Punctuation(At(at + 2) == '(' ? SqlTokenKind.LeftTrigraph : SqlTokenKind.RightTrigraph, at + 3);
					return;
				}

				Punctuation(SqlTokenKind.Question, at + 1);
				return;

			case '.':
				// A period that begins a number is the number's: `.5`.
				if (IsDigit(At(at + 1)))
				{
					Number(at);
					return;
				}

				Punctuation(SqlTokenKind.Dot, at + 1);
				return;

			case '\'':
				StringLiteral(at, at, SqlTokenKind.String);
				return;

			case '"':
				DelimitedIdentifier(at);
				return;

			case '_':
				Introduced(at);
				return;
		}

		if (IsDigit(ch))
		{
			Number(at);

			return;
		}

		// A letter may begin a literal rather than a word: `N'a'`, `X'0A'`, `U&'a'` and `U&"a"`.
		if ((ch | 0x20) == 'n' && At(at + 1) == '\'')
		{
			StringLiteral(at, at + 1, SqlTokenKind.National);

			return;
		}

		if ((ch | 0x20) == 'x' && At(at + 1) == '\'')
		{
			BinaryLiteral(at);

			return;
		}

		if ((ch | 0x20) == 'u' && At(at + 1) == '&')
		{
			if (At(at + 2) == '\'')
			{
				StringLiteral(at, at + 2, SqlTokenKind.UnicodeString);

				return;
			}

			if (At(at + 2) == '"')
			{
				UnicodeDelimitedIdentifier(at, at + 2);

				return;
			}
		}

		if (IsIdentifierStart(ch))
		{
			WordToken(at);

			return;
		}

		Punctuation(SqlTokenKind.Unknown, at + 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	readonly char At(int at) => (uint)at < (uint)Text.Length ? Text[at] : '\0';

	void Punctuation(SqlTokenKind kind, int end)
	{
		_token.Kind = kind;
		_token.End  = end;
		_token.Body = end;
	}

	// ── §5.2 <regular identifier>, §5.2 <reserved word> ────────────────────────

	void WordToken(int at)
	{
		var text = Text;
		var end  = at + 1;

		while (end < text.Length && IsIdentifierPart(text[end]))
			end++;

		_token.Kind  = SqlTokenKind.Word;
		_token.End   = end;
		_token.Body  = end;
		_token.Word  = SqlWords.Of(text.AsSpan(at, end - at));
		_token.Glued = at > 0 && IsIdentifierPart(text[at - 1]);
	}

	// ── §5.2 <delimited identifier>, <Unicode delimited identifier> ────────────

	void DelimitedIdentifier(int at)
	{
		var end = ClosingQuote(at, '"');

		if (end < 0 || end == at + 2)
		{
			// Unterminated, or `""`, whose body §5.2 does not allow to be empty.
			Punctuation(SqlTokenKind.Unknown, at + 1);

			return;
		}

		_token.Kind  = SqlTokenKind.Delimited;
		_token.End   = end;
		_token.Body  = end;
		_token.Quote = at;
	}

	void UnicodeDelimitedIdentifier(int at, int quote)
	{
		var end = ClosingQuote(quote, '"');

		if (end < 0 || end == quote + 2)
		{
			Punctuation(SqlTokenKind.Unknown, at + 1);

			return;
		}

		_token.Kind  = SqlTokenKind.UnicodeName;
		_token.Quote = quote;
		_token.Body  = end;
		_token.End   = Escape(end);
	}

	/// <summary>
	/// Past a run that ends in <paramref name="quote"/>, where a doubled quote stands for one
	/// inside it, or -1 where the text ends first.
	/// </summary>
	readonly int ClosingQuote(int at, char quote)
	{
		var text = Text;

		at++;

		while (at < text.Length)
		{
			if (text[at] == quote)
			{
				if (at + 1 < text.Length && text[at + 1] == quote)
				{
					at += 2;

					continue;
				}

				return at + 1;
			}

			at++;
		}

		return -1;
	}

	/// <summary>
	/// Past a <c>UESCAPE</c> that names the escape character, where one is written against the
	/// closing quote. §5.2 lets no separator stand before it, and the escape is one character.
	/// </summary>
	readonly int Escape(int at)
	{
		const string Word = "UESCAPE";

		if (at + Word.Length + 3 > Text.Length)
			return at;

		if (!Text.AsSpan(at, Word.Length).Equals(Word, StringComparison.OrdinalIgnoreCase))
			return at;

		var quote = at + Word.Length;

		// Only the reverse solidus, the escape the Syntax Rules choose where none is named:
		// what the grammar reads, and what the tree keeps.
		if (Text[quote] != '\'' || Text[quote + 1] != '\\' || quote + 2 >= Text.Length || Text[quote + 2] != '\'')
			return at;

		return quote + 3;
	}

	// ── §5.3 Character, national, Unicode and binary string literals ───────────

	/// <summary>
	/// A literal from <paramref name="at"/>, whose first quote is at <paramref name="quote"/>:
	/// its runs, the separators between them, and a Unicode literal's escape.
	/// </summary>
	void StringLiteral(int at, int quote, SqlTokenKind kind)
	{
		var end = QuotedParts(quote, false, out var parts);

		if (end < 0)
		{
			Punctuation(SqlTokenKind.Unknown, at + 1);

			return;
		}

		_token.Kind  = kind;
		_token.Quote = quote;
		_token.Body  = end;
		_token.Parts = parts;
		_token.End   = kind == SqlTokenKind.UnicodeString ? Escape(end) : end;
	}

	/// <summary>
	/// A binary string literal. Its hexits come in pairs that spaces may stand between, and a run
	/// that holds anything else is no binary literal — so the letter is read as a word instead,
	/// which is what the grammar's ordered choice comes down to.
	/// </summary>
	void BinaryLiteral(int at)
	{
		var end = QuotedParts(at + 1, true, out var parts);

		if (end < 0)
		{
			WordToken(at);

			return;
		}

		_token.Kind  = SqlTokenKind.Binary;
		_token.Quote = at + 1;
		_token.Body  = end;
		_token.End   = end;
		_token.Parts = parts;
	}

	/// <summary>
	/// Whether one run, from its opening quote at <paramref name="at"/> to <paramref name="end"/>,
	/// is <c>&lt;binary part&gt;</c>s: hexits in pairs, which a space may stand between.
	/// </summary>
	readonly bool BinaryPart(int at, int end)
	{
		var text   = Text;
		var hexits = 0;

		for (at++; at < end - 1; at++)
		{
			if (text[at] == ' ')
				continue;

			if (!IsHexit(text[at]))
				return false;

			hexits++;
		}

		return hexits % 2 == 0;
	}

	/// <summary>
	/// Past the quoted runs that begin at <paramref name="at"/>: one run, and every further run a
	/// separator divides from it, since §5.3 makes <c>'a' 'b'</c> one literal.
	/// </summary>
	int QuotedParts(int at, bool hexits, out int parts)
	{
		parts = 0;

		var end = Run(at, hexits);

		if (end < 0)
			return -1;

		var tail = Tail(end, hexits);

		parts = 1 + tail.Parts;

		return tail.End;
	}

	/// <summary>
	/// How far the runs after <paramref name="end"/> reach, and how many there are: a run the
	/// separator does not reach, or that is no run, ends the literal, and what stands there is
	/// refused where it stands rather than here.
	/// </summary>
	/// <remarks>
	/// Where a separator holds a comment that closes nowhere, its two readings end in different
	/// places and each may leave a different literal behind. The BNF writes the runs as a repetition
	/// the parser may go back into, so it takes whichever reading leaves the longest literal — which
	/// is what this searches for. The two readings differ only after an unclosed comment, so the
	/// search branches nowhere else.
	/// </remarks>
	(int End, int Parts) Tail(int end, bool hexits)
	{
		var nested   = Continuation(end, hexits, false);
		var earliest = Continuation(end, hexits, true);
		var best     = (End: end, Parts: 0);

		if (nested >= 0)
		{
			var rest = Tail(nested, hexits);

			best = (rest.End, rest.Parts + 1);
		}

		if (earliest >= 0 && earliest != nested)
		{
			var rest = Tail(earliest, hexits);

			if (rest.End > best.End)
				best = (rest.End, rest.Parts + 1);
		}

		return best;
	}

	/// <summary>
	/// Past the separator that stands after <paramref name="end"/> and the run after it, or -1
	/// where no further run is written there.
	/// </summary>
	int Continuation(int end, bool hexits, bool earliest)
	{
		var next = Trivia(end, earliest);

		return next >= Text.Length || Text[next] != '\'' ? -1 : Run(next, hexits);
	}

	/// <summary>Past one quoted run, or -1 where it does not close or holds what it may not.</summary>
	readonly int Run(int at, bool hexits)
	{
		var end = ClosingQuote(at, '\'');

		return end >= 0 && (!hexits || BinaryPart(at, end)) ? end : -1;
	}

	/// <summary>
	/// An introduced literal, <c>_latin1'a'</c>: the character set is part of the token, so
	/// §5.4's rule against a reserved word is asked inside it and <c>_select.latin1'a'</c> is
	/// refused.
	/// </summary>
	void Introduced(int at)
	{
		var text = Text;
		var name = at + 1;

		// `(<introduced name> <period>){0,2}`, and the SQL language identifier after them.
		for (var taken = 0; taken < 2; taken++)
		{
			var after = IntroducedName(name);

			if (after < 0 || after >= text.Length || text[after] != '.')
				break;

			name = after + 1;
		}

		var end = LanguageIdentifier(name);

		if (end < 0)
		{
			Punctuation(SqlTokenKind.Unknown, at + 1);

			return;
		}

		if (end < text.Length && text[end] == '\'')
		{
			StringLiteral(at, end, SqlTokenKind.String);

			return;
		}

		if (end + 2 < text.Length && (text[end] | 0x20) == 'u' && text[end + 1] == '&' && text[end + 2] == '\'')
		{
			StringLiteral(at, end + 2, SqlTokenKind.UnicodeString);

			return;
		}

		Punctuation(SqlTokenKind.Unknown, at + 1);
	}

	/// <summary>Past a name inside an introducer, or -1 where there is none.</summary>
	readonly int IntroducedName(int at)
	{
		var text = Text;

		if (at >= text.Length)
			return -1;

		if (text[at] == '"')
		{
			var end = ClosingQuote(at, '"');

			return end == at + 2 ? -1 : end;
		}

		if ((text[at] | 0x20) == 'u' && at + 1 < text.Length && text[at + 1] == '&' && at + 2 < text.Length && text[at + 2] == '"')
		{
			var end = ClosingQuote(at + 2, '"');

			// §5.2 does not let the body of a delimited identifier be empty, here either.
			return end < 0 || end == at + 4 ? -1 : Escape(end);
		}

		if (!IsIdentifierStart(text[at]))
			return -1;

		var word = at + 1;

		while (word < text.Length && IsIdentifierPart(text[word]))
			word++;

		// §5.4: a regular identifier is no reserved word, inside a token as well as outside one.
		return SqlWords.Of(text.AsSpan(at, word - at)) == SqlWord.Name ? word : -1;
	}

	/// <summary>
	/// Past an <c>&lt;SQL language identifier&gt;</c> — a Latin letter, then Latin letters,
	/// digits and underscores — or -1 where there is none.
	/// </summary>
	readonly int LanguageIdentifier(int at)
	{
		var text = Text;

		if (at >= text.Length || !IsLatinLetter(text[at]))
			return -1;

		var end = at + 1;

		while (end < text.Length && (IsLatinLetter(text[end]) || IsDigit(text[end]) || text[end] == '_'))
			end++;

		// The name is a whole word: a letter of another script after it is part of no name here.
		return end < text.Length && IsIdentifierPart(text[end]) ? -1 : end;
	}

	// ── §5.3 <unsigned numeric literal> ────────────────────────────────────────

	void Number(int at)
	{
		var text = Text;
		var end  = at;

		if (text[at] == '0' && at + 1 < text.Length)
		{
			// A radix other than ten, which begins with a zero and a letter: `0x1F`, `0o17`, `0b101`.
			var radix = text[at + 1] | 0x20;

			if (radix == 'x' || radix == 'o' || radix == 'b')
			{
				var digits = Digits(at + 2, radix);

				if (digits > 0)
				{
					// `0b10E1`: an integer of any radix is an <exact numeric literal>, and an
					// exponent after one makes it approximate.
					_token.Kind = SqlTokenKind.Number;
					_token.End  = Exponent(digits);
					_token.Body = _token.End;

					return;
				}
			}
		}

		if (text[end] != '.')
		{
			end = Digits(end, 'd');

			if (end < text.Length && text[end] == '.')
				end = Fraction(end);
		}
		else
		{
			end = Fraction(end);
		}

		_token.Kind = SqlTokenKind.Number;
		_token.End  = Exponent(end);
		_token.Body = _token.End;
	}

	/// <summary>Past an exponent, which makes the number approximate: `1E5`, `1.5E-3`, `1.e5`.</summary>
	readonly int Exponent(int at)
	{
		var text = Text;

		if (at >= text.Length || (text[at] | 0x20) != 'e')
			return at;

		var exponent = at + 1;

		if (exponent < text.Length && (text[exponent] == '+' || text[exponent] == '-'))
			exponent++;

		var digits = Digits(exponent, 'd');

		return digits > 0 ? digits : at;
	}

	/// <summary>Past a period and the digits after it, where any were written.</summary>
	readonly int Fraction(int at)
	{
		var digits = Digits(at + 1, 'd');

		return digits > 0 ? digits : at + 1;
	}

	/// <summary>
	/// Past the digits of one radix, an underscore allowed between two of them, or 0 where the
	/// first character is no digit of it.
	/// </summary>
	readonly int Digits(int at, int radix)
	{
		var text = Text;

		// A radix other than ten lets an underscore stand before its first digit as well as
		// between two of them: `0X_1` is a hexadecimal one.
		if (radix != 'd' && at < text.Length && text[at] == '_')
			at++;

		if (at >= text.Length || !IsDigit(text[at], radix))
			return 0;

		at++;

		while (at < text.Length)
		{
			if (IsDigit(text[at], radix))
			{
				at++;
			}
			else if (text[at] == '_' && at + 1 < text.Length && IsDigit(text[at + 1], radix))
			{
				at += 2;
			}
			else
			{
				break;
			}
		}

		return at;
	}

	// ── The character classes §5.1 and §5.2 are written in ─────────────────────

	public static bool IsDigit(char ch) => (uint)(ch - '0') <= 9;

	static bool IsDigit(char ch, int radix) =>
		radix switch
		{
			'x' => IsHexit(ch),
			'o' => (uint)(ch - '0') <= 7,
			'b' => ch == '0' || ch == '1',
			_   => (uint)(ch - '0') <= 9,
		};

	static bool IsHexit(char ch) => (uint)(ch - '0') <= 9 || (uint)((ch | 0x20) - 'a') <= 5;

	static bool IsLatinLetter(char ch) => (uint)((ch | 0x20) - 'a') <= 25;

	/// <summary>An <c>&lt;identifier start&gt;</c>: a letter of any script, or a letter-like number.</summary>
	public static bool IsIdentifierStart(char ch) =>
		(uint)((ch | 0x20) - 'a') <= 25 ||
		ch > 127 && (char.IsLetter(ch) || char.GetUnicodeCategory(ch) == UnicodeCategory.LetterNumber);

	/// <summary>
	/// An <c>&lt;identifier part&gt;</c>: a start, a digit, the middle dot, a combining mark, a
	/// connector or a format character. It is also the word boundary §5.2 guards a key word with.
	/// </summary>
	public static bool IsIdentifierPart(char ch)
	{
		if ((uint)((ch | 0x20) - 'a') <= 25 || (uint)(ch - '0') <= 9 || ch == '_')
			return true;

		if (ch < 128)
			return false;

		return char.GetUnicodeCategory(ch) switch
		{
			UnicodeCategory.UppercaseLetter      or
			UnicodeCategory.LowercaseLetter      or
			UnicodeCategory.TitlecaseLetter      or
			UnicodeCategory.ModifierLetter       or
			UnicodeCategory.OtherLetter          or
			UnicodeCategory.LetterNumber         or
			UnicodeCategory.NonSpacingMark       or
			UnicodeCategory.SpacingCombiningMark or
			UnicodeCategory.DecimalDigitNumber   or
			UnicodeCategory.ConnectorPunctuation or
			UnicodeCategory.Format               => true,
			_                                    => ch == '\u00B7',
		};
	}
}
