using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

using DotGram.ExpressionLanguage;

namespace DotGram.Benchmarks;

/// <summary>
/// The expression language of <see cref="ExpressionParser"/>, written by hand: a lexer
/// over the whole input and a recursive descent over the tokens it makes.
/// </summary>
/// <remarks>
/// <para>
/// What <see cref="HandSqlTokens"/> is to <c>SqlStandard92</c>, this is to
/// <c>ExpressionParser</c> — the mark a generated parser is measured against, and the
/// answer to "how fast would a person have written this". It has to keep <em>looking</em>
/// hand-written: what is here is what someone would write who knew the language and cared
/// about the result, and nothing is shaped by how the generator happens to work.
/// </para>
/// <para>
/// It calls the same factories the grammar's <c>=&gt;</c> calls, and hands the same
/// <see cref="ExpressionParser.State"/> the same spans. That is deliberate: what is
/// being compared is the reading, not the building, and a second implementation of scopes
/// and names would be a second thing to be wrong. The same reason
/// <see cref="HandSqlTokens"/> builds the shipped tree rather than one of its own.
/// </para>
/// <para>
/// Two shapes here are a person's rather than a grammar's, and both are where the two
/// differ most. The operator ladder is one loop over a precedence rather than ten rules —
/// C#'s table, read out of an array — because that is how a person writes ten levels that
/// differ only in a number. And the suffixes of a postfix chain are a loop rather than a
/// left recursion, which is the same saving by the same argument. Everything else follows
/// the grammar rule for rule, so that a disagreement is a disagreement about the language
/// and not about which of the two was written more cleverly.
/// </para>
/// </remarks>
static class HandExpression
{
	// ── Token kinds ─────────────────────────────────────────────────────────────
	//
	// One byte each, punctuation first so that the lexer's switch over a character can
	// answer most of them without looking further, and the keywords after — those are a
	// word first and a keyword second, which is the order they are recognized in.

	const byte End = 0;

	const byte LeftParen    =  1;
	const byte RightParen   =  2;
	const byte LeftBracket  =  3;
	const byte RightBracket =  4;
	const byte Brackets     =  5;   // `[]`, an array type's own and never an empty index
	const byte LeftBrace    =  6;
	const byte RightBrace   =  7;
	const byte Comma        =  8;
	const byte Semicolon    =  9;
	const byte Colon        = 10;
	const byte Dot          = 11;
	const byte Question     = 12;
	const byte Coalesce     = 13;   // `??`
	const byte Arrow        = 14;   // `=>`

	const byte Assign     = 15;
	const byte Equal      = 16;
	const byte NotEqual   = 17;
	const byte Less       = 18;
	const byte LessEq     = 19;
	const byte Greater    = 20;
	const byte GreaterEq  = 21;
	const byte AndAlso    = 22;
	const byte OrElse     = 23;
	const byte Amp        = 24;
	const byte Pipe       = 25;
	const byte Caret      = 26;
	const byte Not        = 27;
	const byte Tilde      = 28;
	const byte Plus       = 29;
	const byte Minus      = 30;
	const byte Star       = 31;
	const byte Slash      = 32;
	const byte Percent    = 33;
	const byte Increment  = 34;
	const byte Decrement  = 35;

	const byte PlusAssign    = 36;
	const byte MinusAssign   = 37;
	const byte StarAssign    = 38;
	const byte SlashAssign   = 39;
	const byte PercentAssign = 40;
	const byte AmpAssign     = 41;
	const byte PipeAssign    = 42;
	const byte CaretAssign   = 43;
	const byte LeftAssign    = 44;   // `<<=`
	const byte RightAssign   = 45;   // `>>=`

	// A number carries what it is in its kind: the lexer walked the digits and the suffix
	// already, and a second pass over them to find out what a person wrote would be a
	// pass nobody needs. `Number` is the unsuffixed decimal, which is an `int` where it
	// fits and a `long` where it does not — C#'s rule, and the one thing here the parser
	// decides rather than the lexer.
	const byte Number     = 46;
	const byte NumberU    = 47;
	const byte NumberL    = 48;
	const byte NumberUL   = 49;
	const byte Hex        = 50;
	const byte HexU       = 51;
	const byte HexL       = 52;
	const byte HexUL      = 53;
	const byte Bits     = 54;
	const byte BitsU    = 55;
	const byte BitsL    = 56;
	const byte BitsUL   = 57;
	const byte Real       = 58;
	const byte RealD      = 59;
	const byte RealF      = 60;
	const byte RealM      = 61;

	const byte Text     = 62;
	const byte Verbatim = 63;
	const byte Character = 64;

	const byte Identifier = 65;

	// The keywords, in the order the language reserves them (§5 of the grammar). A word
	// that is one of these is never a name, which is the whole of what makes it a keyword.
	const byte FirstWord = 66;

	static readonly string[] Words =
	[
		"as",      "bool",    "break",   "byte",    "case",     "catch",
		"char",    "checked", "continue","decimal", "default",  "do",
		"double",  "else",    "false",   "finally", "float",    "for",
		"if",      "int",     "is",      "long",    "new",      "null",
		"object",  "return",  "sbyte",   "short",   "string",   "switch",
		"throw",   "true",    "try",     "uint",    "ulong",    "unchecked",
		"ushort",  "while",
	];

	static byte Of(string word) => (byte)(FirstWord + Array.IndexOf(Words, word));

	static readonly byte KwAs        = Of("as");
	static readonly byte KwBool      = Of("bool");
	static readonly byte KwBreak     = Of("break");
	static readonly byte KwByte      = Of("byte");
	static readonly byte KwCase      = Of("case");
	static readonly byte KwCatch     = Of("catch");
	static readonly byte KwChar      = Of("char");
	static readonly byte KwChecked   = Of("checked");
	static readonly byte KwContinue  = Of("continue");
	static readonly byte KwDecimal   = Of("decimal");
	static readonly byte KwDefault   = Of("default");
	static readonly byte KwDo        = Of("do");
	static readonly byte KwDouble    = Of("double");
	static readonly byte KwElse      = Of("else");
	static readonly byte KwFalse     = Of("false");
	static readonly byte KwFinally   = Of("finally");
	static readonly byte KwFloat     = Of("float");
	static readonly byte KwFor       = Of("for");
	static readonly byte KwIf        = Of("if");
	static readonly byte KwInt       = Of("int");
	static readonly byte KwIs        = Of("is");
	static readonly byte KwLong      = Of("long");
	static readonly byte KwNew       = Of("new");
	static readonly byte KwNull      = Of("null");
	static readonly byte KwObject    = Of("object");
	static readonly byte KwReturn    = Of("return");
	static readonly byte KwSbyte     = Of("sbyte");
	static readonly byte KwShort     = Of("short");
	static readonly byte KwString    = Of("string");
	static readonly byte KwSwitch    = Of("switch");
	static readonly byte KwThrow     = Of("throw");
	static readonly byte KwTrue      = Of("true");
	static readonly byte KwTry       = Of("try");
	static readonly byte KwUint      = Of("uint");
	static readonly byte KwUlong     = Of("ulong");
	static readonly byte KwUnchecked = Of("unchecked");
	static readonly byte KwUshort    = Of("ushort");
	static readonly byte KwWhile     = Of("while");

	/// <summary>Which keyword a word is, or <see cref="Identifier"/> where it is none.</summary>
	/// <remarks>
	/// Switched on the length first and the first letter second, which between them leave
	/// at most three words to compare and usually one. A dictionary would say the same
	/// thing and hash the word to find out.
	/// </remarks>
	static byte Keyword(ReadOnlySpan<char> w)
	{
		switch (w.Length)
		{
			case 2:
				if (w[0] == 'a' && w[1] == 's') return KwAs;
				if (w[0] == 'd' && w[1] == 'o') return KwDo;
				if (w[0] == 'i' && w[1] == 'f') return KwIf;
				if (w[0] == 'i' && w[1] == 's') return KwIs;
				break;

			case 3:
				if (Same(w, "for")) return KwFor;
				if (Same(w, "int")) return KwInt;
				if (Same(w, "new")) return KwNew;
				if (Same(w, "try")) return KwTry;
				break;

			case 4:
				switch (w[0])
				{
					case 'b': if (Same(w, "bool")) return KwBool; break;
					case 'c': if (Same(w, "case")) return KwCase; if (Same(w, "char")) return KwChar; break;
					case 'e': if (Same(w, "else")) return KwElse; break;
					case 'l': if (Same(w, "long")) return KwLong; break;
					case 'n': if (Same(w, "null")) return KwNull; break;
					case 't': if (Same(w, "true")) return KwTrue; break;
					case 'u': if (Same(w, "uint")) return KwUint; break;
				}

				break;

			case 5:
				switch (w[0])
				{
					case 'b': if (Same(w, "break")) return KwBreak;  break;
					case 'c': if (Same(w, "catch")) return KwCatch;  break;
					case 'f': if (Same(w, "false")) return KwFalse;  if (Same(w, "float")) return KwFloat; break;
					case 's': if (Same(w, "short")) return KwShort;  break;
					case 't': if (Same(w, "throw")) return KwThrow;  break;
					case 'u': if (Same(w, "ulong")) return KwUlong;  break;
					case 'w': if (Same(w, "while")) return KwWhile;  break;
				}

				break;

			case 6:
				switch (w[0])
				{
					case 'd': if (Same(w, "double")) return KwDouble; break;
					case 'o': if (Same(w, "object")) return KwObject; break;
					case 'r': if (Same(w, "return")) return KwReturn; break;
					case 's': if (Same(w, "string")) return KwString; if (Same(w, "switch")) return KwSwitch; break;
					case 'u': if (Same(w, "ushort")) return KwUshort; break;
				}

				break;

			case 7:
				if (Same(w, "checked")) return KwChecked;
				if (Same(w, "decimal")) return KwDecimal;
				if (Same(w, "default")) return KwDefault;
				if (Same(w, "finally")) return KwFinally;
				break;

			case 8:
				if (Same(w, "continue")) return KwContinue;
				break;

			case 9:
				if (Same(w, "unchecked")) return KwUnchecked;
				break;
		}

		return Identifier;
	}

	static bool Same(ReadOnlySpan<char> w, string word)
	{
		for (var i = 0; i < word.Length; i++)
			if (w[i] != word[i])
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

		public void Room(int length)
		{
			if (Kinds.Length >= length)
				return;

			Kinds   = new byte[length];
			Starts  = new int[length];
			Lengths = new int[length];
		}

		public void Grow(int count)
		{
			if (Kinds.Length > count)
				return;

			var size = Kinds.Length < 64 ? 64 : Kinds.Length * 2;

			Array.Resize(ref Kinds,   size);
			Array.Resize(ref Starts,  size);
			Array.Resize(ref Lengths, size);
		}
	}

	[ThreadStatic]
	static Tokens? _tokens;

	public static Tokens Rented() => _tokens ??= new Tokens();

	/// <summary>The whole input as tokens, whitespace skipped. False where a character fits nothing.</summary>
	public static bool Lex(string text, Tokens into)
	{
		var s = text.AsSpan();

		into.Room(s.Length / 3 + 16);
		into.Count = 0;

		var kinds   = into.Kinds;
		var starts  = into.Starts;
		var lengths = into.Lengths;
		var count   = 0;
		var p       = 0;

		while (true)
		{
			while (p < s.Length && (s[p] == ' ' || s[p] == '\t' || s[p] == '\r' || s[p] == '\n'))
				p++;

			if (p >= s.Length)
				break;

			var from = p;
			var c    = s[p];
			byte kind;

			switch (c)
			{
				case '(': kind = LeftParen;    p++; break;
				case ')': kind = RightParen;   p++; break;
				case '{': kind = LeftBrace;    p++; break;
				case '}': kind = RightBrace;   p++; break;
				case ',': kind = Comma;        p++; break;
				case ';': kind = Semicolon;    p++; break;
				case ':': kind = Colon;        p++; break;
				case '~': kind = Tilde;        p++; break;
				case '^': kind = p + 1 < s.Length && s[p + 1] == '=' ? Take(ref p, 2, CaretAssign) : Take(ref p, 1, Caret); break;

				// `[]` is a type's own and is one token, the way the generated lexer takes
				// the longest match. `a[0]` is three, because what follows the bracket is
				// not the other one.
				case '[':
					kind = p + 1 < s.Length && s[p + 1] == ']' ? Take(ref p, 2, Brackets) : Take(ref p, 1, LeftBracket);
					break;

				case ']': kind = RightBracket; p++; break;

				case '?':
					kind = p + 1 < s.Length && s[p + 1] == '?' ? Take(ref p, 2, Coalesce) : Take(ref p, 1, Question);
					break;

				case '=':
					kind = p + 1 >= s.Length ? Take(ref p, 1, Assign)
						: s[p + 1] == '=' ? Take(ref p, 2, Equal)
						: s[p + 1] == '>' ? Take(ref p, 2, Arrow)
						: Take(ref p, 1, Assign);
					break;

				case '!':
					kind = p + 1 < s.Length && s[p + 1] == '=' ? Take(ref p, 2, NotEqual) : Take(ref p, 1, Not);
					break;

				// `<<` and `>>` are not tokens, and that is the language rather than an
				// omission: a shift is two of these written with nothing between them, so
				// that `List<List<int>>` can close two argument lists with the same two
				// characters C# closes them with. `<<=` and `>>=` are, being written as
				// one thing and never as two.
				case '<':
					kind = p + 2 < s.Length && s[p + 1] == '<' && s[p + 2] == '=' ? Take(ref p, 3, LeftAssign)
						: p + 1 < s.Length && s[p + 1] == '=' ? Take(ref p, 2, LessEq)
						: Take(ref p, 1, Less);
					break;

				case '>':
					kind = p + 2 < s.Length && s[p + 1] == '>' && s[p + 2] == '=' ? Take(ref p, 3, RightAssign)
						: p + 1 < s.Length && s[p + 1] == '=' ? Take(ref p, 2, GreaterEq)
						: Take(ref p, 1, Greater);
					break;

				case '&':
					kind = p + 1 >= s.Length ? Take(ref p, 1, Amp)
						: s[p + 1] == '&' ? Take(ref p, 2, AndAlso)
						: s[p + 1] == '=' ? Take(ref p, 2, AmpAssign)
						: Take(ref p, 1, Amp);
					break;

				case '|':
					kind = p + 1 >= s.Length ? Take(ref p, 1, Pipe)
						: s[p + 1] == '|' ? Take(ref p, 2, OrElse)
						: s[p + 1] == '=' ? Take(ref p, 2, PipeAssign)
						: Take(ref p, 1, Pipe);
					break;

				case '+':
					kind = p + 1 >= s.Length ? Take(ref p, 1, Plus)
						: s[p + 1] == '+' ? Take(ref p, 2, Increment)
						: s[p + 1] == '=' ? Take(ref p, 2, PlusAssign)
						: Take(ref p, 1, Plus);
					break;

				case '-':
					kind = p + 1 >= s.Length ? Take(ref p, 1, Minus)
						: s[p + 1] == '-' ? Take(ref p, 2, Decrement)
						: s[p + 1] == '=' ? Take(ref p, 2, MinusAssign)
						: Take(ref p, 1, Minus);
					break;

				case '*':
					kind = p + 1 < s.Length && s[p + 1] == '=' ? Take(ref p, 2, StarAssign) : Take(ref p, 1, Star);
					break;

				case '/':
					kind = p + 1 < s.Length && s[p + 1] == '=' ? Take(ref p, 2, SlashAssign) : Take(ref p, 1, Slash);
					break;

				case '%':
					kind = p + 1 < s.Length && s[p + 1] == '=' ? Take(ref p, 2, PercentAssign) : Take(ref p, 1, Percent);
					break;

				// A point begins a real where a digit follows it — `.5` is one — and is a
				// member access where anything else does.
				case '.':
					if (p + 1 < s.Length && s[p + 1] >= '0' && s[p + 1] <= '9')
					{
						p    = Numeric(s, p, out kind);
						break;
					}

					kind = Take(ref p, 1, Dot);
					break;

				case '"':
					p = Quoted(s, p, '"');

					if (p < 0)
						return false;

					kind = Text;
					break;

				case '\'':
					p = Quoted(s, p, '\'');

					if (p < 0)
						return false;

					kind = Character;
					break;

				case '@':
					if (p + 1 >= s.Length || s[p + 1] != '"')
						return false;

					p = p + 2;

					while (true)
					{
						if (p >= s.Length)
							return false;

						if (s[p] != '"')
						{
							p++;

							continue;
						}

						if (p + 1 < s.Length && s[p + 1] == '"')
						{
							p += 2;

							continue;
						}

						p++;

						break;
					}

					kind = Verbatim;
					break;

				default:
					if (c >= '0' && c <= '9')
					{
						p = Numeric(s, p, out kind);

						break;
					}

					if (!IsStart(c))
						return false;

					p++;

					while (p < s.Length && IsPart(s[p]))
						p++;

					kind = Keyword(s.Slice(from, p - from));
					break;
			}

			if (count == kinds.Length)
			{
				into.Grow(count);

				kinds   = into.Kinds;
				starts  = into.Starts;
				lengths = into.Lengths;
			}

			kinds  [count] = kind;
			starts [count] = from;
			lengths[count] = p - from;
			count++;
		}

		into.Grow(count);

		kinds   = into.Kinds;
		starts  = into.Starts;
		lengths = into.Lengths;

		kinds  [count] = End;
		starts [count] = s.Length;
		lengths[count] = 0;
		into.Count     = count;

		return true;
	}

	static byte Take(ref int p, int width, byte kind)
	{
		p += width;

		return kind;
	}

	/// <summary>
	/// One number, whichever of the eight forms it is, with its base and its suffix in the
	/// kind it comes back as.
	/// </summary>
	static int Numeric(ReadOnlySpan<char> s, int p, out byte kind)
	{
		if (s[p] == '0' && p + 1 < s.Length && (s[p + 1] == 'x' || s[p + 1] == 'X'))
		{
			p += 2;

			while (p < s.Length && (s[p] == '_' || IsHex(s[p])))
				p++;

			kind = Suffixed(s, ref p, Hex, HexU, HexL, HexUL);

			return p;
		}

		if (s[p] == '0' && p + 1 < s.Length && (s[p + 1] == 'b' || s[p + 1] == 'B'))
		{
			p += 2;

			while (p < s.Length && (s[p] == '_' || s[p] == '0' || s[p] == '1'))
				p++;

			kind = Suffixed(s, ref p, Bits, BitsU, BitsL, BitsUL);

			return p;
		}

		var real = false;

		while (p < s.Length && (s[p] == '_' || IsDigit(s[p])))
			p++;

		if (p < s.Length && s[p] == '.' && p + 1 < s.Length && IsDigit(s[p + 1]))
		{
			real = true;
			p++;

			while (p < s.Length && (s[p] == '_' || IsDigit(s[p])))
				p++;
		}

		if (p < s.Length && (s[p] == 'e' || s[p] == 'E'))
		{
			var after = p + 1;

			if (after < s.Length && (s[after] == '+' || s[after] == '-'))
				after++;

			if (after < s.Length && IsDigit(s[after]))
			{
				real = true;
				p    = after;

				while (p < s.Length && (s[p] == '_' || IsDigit(s[p])))
					p++;
			}
		}

		// The three real suffixes take a decimal as readily as a real — `1m` is a decimal
		// and `1f` a float — so they are asked about before the integer suffixes are.
		if (p < s.Length)
			switch (s[p])
			{
				case 'm': case 'M': p++; kind = RealM; return p;
				case 'd': case 'D': p++; kind = RealD; return p;
				case 'f': case 'F': p++; kind = RealF; return p;
			}

		if (real)
		{
			kind = Real;

			return p;
		}

		kind = Suffixed(s, ref p, Number, NumberU, NumberL, NumberUL);

		return p;
	}

	static byte Suffixed(ReadOnlySpan<char> s, ref int p, byte plain, byte unsigned, byte signedLong, byte both)
	{
		if (p >= s.Length)
			return plain;

		var one = s[p] | 0x20;

		if (one == 'u')
		{
			p++;

			if (p < s.Length && (s[p] | 0x20) == 'l')
			{
				p++;

				return both;
			}

			return unsigned;
		}

		if (one == 'l')
		{
			p++;

			if (p < s.Length && (s[p] | 0x20) == 'u')
			{
				p++;

				return both;
			}

			return signedLong;
		}

		return plain;
	}

	/// <summary>Past the closing quote, or -1 where the text runs out first.</summary>
	static int Quoted(ReadOnlySpan<char> s, int p, char quote)
	{
		p++;

		while (true)
		{
			if (p >= s.Length)
				return -1;

			if (s[p] == '\\')
			{
				p += 2;

				continue;
			}

			if (s[p] == quote)
				return p + 1;

			p++;
		}
	}

	static bool IsDigit(char c) => c >= '0' && c <= '9';

	static bool IsHex(char c) =>
		c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F';

	static bool IsStart(char c) => c == '_' || char.IsLetter(c);

	static bool IsPart(char c) => c == '_' || char.IsLetterOrDigit(c);

	// ── The parser ──────────────────────────────────────────────────────────────

	/// <summary>The lambda the text is, or null where it is not one.</summary>
	public static LambdaExpression? Build(string text)
	{
		var tokens = Rented();

		if (!Lex(text, tokens))
			return null;

		var reader = new Reader(tokens, text, new ExpressionParser.State());
		var end    = reader.Lambda(0, out var node);

		return end == tokens.Count ? node : null;
	}

	/// <summary>Whether the whole input is a lambda, tree and all.</summary>
	public static bool Parse(string text)
	{
		try
		{
			return Build(text) is not null;
		}
		catch (Exception exception) when (exception is FormatException or ArgumentException or InvalidOperationException)
		{
			// The API refuses what this language cannot type — `1 + "a"`, a member no type
			// has — in its own words and by throwing, and so does the generated parser.
			// A verdict is what is being compared, so a refusal by exception is a refusal.
			return false;
		}
	}

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
	/// into an <c>out</c> parameter — the position is the return value because every
	/// caller needs it, the node an argument because only some do.
	/// </remarks>
	ref struct Reader(Tokens tokens, string text, ExpressionParser.State context)
	{
		readonly byte[] _kinds   = tokens.Kinds;
		readonly int[]  _starts  = tokens.Starts;
		readonly int[]  _lengths = tokens.Lengths;
		readonly int    _count   = tokens.Count;
		readonly string _text    = text;

		readonly ExpressionParser.State _context = context;

		// What the text being read stands under (§7.8 of the grammar). Two of these is
		// already more than any expression written by a person, and it grows if it has to.
		ExpressionParser.Reading[] _marks = new ExpressionParser.Reading[8];
		int _marked;

		readonly byte Kind(int i) => i < _count ? _kinds[i] : End;

		/// <summary>Where the tokens from one to another stand in the input.</summary>
		readonly ExpressionParser.SourceSpan Span(int from, int to) =>
			new(_starts[from], _starts[to - 1] + _lengths[to - 1] - _starts[from]);

		readonly string Cut(int i) => _text.Substring(_starts[i], _lengths[i]);

		readonly ReadOnlySpan<ExpressionParser.Reading> Marks => new(_marks, 0, _marked);

		void Mark(ExpressionParser.Reading reading)
		{
			if (_marked == _marks.Length)
				Array.Resize(ref _marks, _marked * 2);

			_marks[_marked++] = reading;
		}

		// ── The lambda, and what it takes ───────────────────────────────────────

		public int Lambda(int i, out LambdaExpression? node)
		{
			node = null;

			if (Kind(i) != LeftParen)
				return -1;

			var at    = i + 1;
			var taken = new List<ParameterExpression>();

			if (Kind(at) != RightParen)
			{
				var first = Parameter(at, out var one);

				if (first < 0)
					return -1;

				taken.Add(one!);
				at = first;

				while (Kind(at) == Comma)
				{
					var more = Parameter(at + 1, out var next);

					if (more < 0)
						return -1;

					taken.Add(next!);
					at = more;
				}
			}

			if (Kind(at) != RightParen || Kind(at + 1) != Arrow)
				return -1;

			var body = Value(at + 2, out var read);

			if (body < 0)
				return -1;

			node = Expression.Lambda(_context.Returning(read!), taken.ToArray());

			return body;
		}

		int Parameter(int i, out ParameterExpression? node)
		{
			node = null;

			var at = Type(i, out var type);

			if (at < 0 || Kind(at) != Identifier)
				return -1;

			var name = Cut(at);
			var span = Span(i, at + 1);

			if (!_context.Takes(type!, name, span))
				return -1;

			node = _context.Named(name, span);

			return at + 1;
		}

		// ── Types ───────────────────────────────────────────────────────────────

		int Type(int i, out Type? type)
		{
			var at = Core(i, out type);

			if (at < 0)
				return -1;

			while (Kind(at) == Brackets)
			{
				type = type!.MakeArrayType();
				at++;
			}

			return at;
		}

		int Core(int i, out Type? type)
		{
			type = Kind(i) switch
			{
				var k when k == KwSbyte   => typeof(sbyte),
				var k when k == KwByte    => typeof(byte),
				var k when k == KwShort   => typeof(short),
				var k when k == KwUshort  => typeof(ushort),
				var k when k == KwInt     => typeof(int),
				var k when k == KwUint    => typeof(uint),
				var k when k == KwLong    => typeof(long),
				var k when k == KwUlong   => typeof(ulong),
				var k when k == KwFloat   => typeof(float),
				var k when k == KwDouble  => typeof(double),
				var k when k == KwDecimal => typeof(decimal),
				var k when k == KwBool    => typeof(bool),
				var k when k == KwChar    => typeof(char),
				var k when k == KwString  => typeof(string),
				var k when k == KwObject  => typeof(object),
				_                         => null,
			};

			return type is not null ? i + 1 : Named(i, out type);
		}

		/// <summary>
		/// A dotted name, and the type arguments where there are any. What decides that it
		/// is a type at all is whether the name resolves — asked here, while the text is
		/// read, so that the answer can decide how the text reads.
		/// </summary>
		int Named(int i, out Type? type)
		{
			type = null;

			if (Kind(i) != Identifier)
				return -1;

			// Every word of the dotted name, so that the longest one can be asked about
			// first and the tail given back where it names nothing: `Math.PI` is `Math`
			// and a member of it, and only asking says so.
			var words = new List<int> { i };
			var at    = i + 1;

			while (Kind(at) == Dot && Kind(at + 1) == Identifier)
			{
				words.Add(at + 1);
				at += 2;
			}

			for (var last = words.Count - 1; last >= 0; last--)
			{
				var after = words[last] + 1;
				var name  = Dotted(words, last);

				// The generic form needs no name that resolves on its own: `List<int>`
				// does and `List` does not, so where there are arguments they are what
				// says the name is a type.
				if (Kind(after) == Less)
				{
					var closed = TypeArguments(after, out var arguments);

					if (closed >= 0)
					{
						type = ExpressionParser.Generic(name, arguments!);

						return closed;
					}
				}

				if (!ExpressionParser.Resolves(name))
					continue;

				type = ExpressionParser.TypeNamed(name);

				return after;
			}

			return -1;
		}

		readonly string Dotted(List<int> parts, int last)
		{
			var name = Cut(parts[0]);

			for (var part = 1; part <= last; part++)
				name += "." + Cut(parts[part]);

			return name;
		}

		int TypeArguments(int from, out Type[]? arguments)
		{
			arguments = null;

			var read = new List<Type>();
			var one  = Type(from + 1, out var first);

			if (one < 0)
				return -1;

			read.Add(first!);

			while (Kind(one) == Comma)
			{
				var next = Type(one + 1, out var more);

				if (next < 0)
					return -1;

				read.Add(more!);
				one = next;
			}

			if (Kind(one) != Greater)
				return -1;

			arguments = read.ToArray();

			return one + 1;
		}

		// ── Statements ──────────────────────────────────────────────────────────

		/// <summary>Where a value is wanted and a block or an `if` may stand.</summary>
		int Value(int i, out Expression? node)
		{
			if (Kind(i) == LeftBrace)
				return Block(i, out node);

			if (Kind(i) == KwIf)
			{
				var chosen = IfValue(i, out node);

				if (chosen >= 0)
					return chosen;
			}

			if (IsControl(Kind(i)))
			{
				var control = Control(i, out node);

				if (control >= 0)
					return control;
			}

			return Expr(i, out node);
		}

		readonly bool IsControl(byte kind) =>
			kind == KwTry || kind == KwIf || kind == KwWhile ||
			kind == KwDo  || kind == KwFor || kind == KwSwitch;

		int Statement(int i, out Expression? node)
		{
			node = null;

			var local = Local(i, out node);

			if (local >= 0)
				return local;

			if (Kind(i) == KwReturn)
			{
				var value = Value(i + 1, out var read);

				if (value >= 0 && Kind(value) == Semicolon)
				{
					node = _context.Return(read!);

					return value + 1;
				}

				return -1;
			}

			if (Kind(i) == LeftBrace)
				return Block(i, out node);

			if (IsControl(Kind(i)))
			{
				var control = Control(i, out node);

				if (control >= 0)
					return control;
			}

			var jump = Jump(i, out node);

			if (jump >= 0 && Kind(jump) == Semicolon)
				return jump + 1;

			var expression = Expr(i, out node);

			if (expression >= 0 && Kind(expression) == Semicolon)
				return expression + 1;

			node = null;

			return -1;
		}

		int Local(int i, out Expression? node)
		{
			node = null;

			var at = Type(i, out var type);

			if (at < 0 || Kind(at) != Identifier)
				return -1;

			var name = Cut(at);

			if (Kind(at + 1) != Assign)
				return -1;

			if (!_context.Declare(type!, name, Span(i, at + 1)))
				return -1;

			var value = Value(at + 2, out var read);

			if (value < 0 || Kind(value) != Semicolon)
				return -1;

			node = Expression.Assign(_context.Named(name, Span(i, value + 1)), read!);

			return value + 1;
		}

		int Block(int i, out Expression? node)
		{
			node = null;

			if (Kind(i) != LeftBrace)
				return -1;

			var at         = i + 1;
			var statements = new List<Expression>();

			while (true)
			{
				var one = Statement(at, out var read);

				if (one < 0)
					break;

				statements.Add(read!);
				at = one;
			}

			var value = Expr(at, out var last);

			if (value >= 0)
				at = value;
			else
				last = null;

			if (Kind(at) != RightBrace)
				return -1;

			var span = Span(i, at + 1);

			if (!_context.Scoped(span))
				return -1;

			node = _context.Block(statements.ToArray(), span, last);

			return at + 1;
		}

		int Control(int i, out Expression? node)
		{
			node = null;

			var kind = Kind(i);

			if (kind == KwTry)    return Try(i, out node);
			if (kind == KwIf)     return If(i, out node);
			if (kind == KwWhile)  return While(i, out node);
			if (kind == KwDo)     return DoWhile(i, out node);
			if (kind == KwFor)    return For(i, out node);
			if (kind == KwSwitch) return Switch(i, out node);

			return -1;
		}

		int If(int i, out Expression? node)
		{
			node = null;

			if (Kind(i) != KwIf || Kind(i + 1) != LeftParen)
				return -1;

			var test = Expr(i + 2, out var read);

			if (test < 0 || Kind(test) != RightParen)
				return -1;

			var then = Branch(test + 1, out var whenTrue);

			if (then >= 0 && Kind(then) == KwElse)
			{
				var otherwise = Branch(then + 1, out var whenFalse);

				if (otherwise >= 0)
				{
					node = ExpressionParser.Chosen(read!, whenTrue, whenFalse);

					return otherwise;
				}
			}

			var alone = Statement(test + 1, out var only);

			if (alone < 0)
				return -1;

			node = Expression.IfThen(read!, only!);

			return alone;
		}

		/// <summary>The same `if` where a value is wanted, so that its branches are values.</summary>
		int IfValue(int i, out Expression? node)
		{
			node = null;

			if (Kind(i) != KwIf || Kind(i + 1) != LeftParen)
				return -1;

			var test = Expr(i + 2, out var read);

			if (test < 0 || Kind(test) != RightParen)
				return -1;

			var then = Value(test + 1, out var whenTrue);

			if (then < 0 || Kind(then) != KwElse)
				return -1;

			var otherwise = Value(then + 1, out var whenFalse);

			if (otherwise < 0)
				return -1;

			node = ExpressionParser.Chosen(read!, whenTrue, whenFalse);

			return otherwise;
		}

		int Branch(int i, out Expression? node)
		{
			var statement = Statement(i, out node);

			return statement >= 0 ? statement : Expr(i, out node);
		}

		int While(int i, out Expression? node)
		{
			node = null;

			if (Kind(i) != KwWhile || Kind(i + 1) != LeftParen)
				return -1;

			var test = Expr(i + 2, out var read);

			if (test < 0 || Kind(test) != RightParen)
				return -1;

			_context.Opening(Span(i, test + 1));

			var body = Statement(test + 1, out var inside);

			if (body < 0)
				return -1;

			var span = Span(i, body);

			if (!_context.Loops(span))
				return -1;

			node = Expression.Loop(
				Expression.Condition(
					read!, inside!, Expression.Break(_context.Exit(span)), typeof(void)),
				_context.Exit(span),
				_context.Again(span));

			return body;
		}

		int DoWhile(int i, out Expression? node)
		{
			node = null;

			if (Kind(i) != KwDo)
				return -1;

			_context.Opening(Span(i, i + 1));

			var body = Statement(i + 1, out var inside);

			if (body < 0 || Kind(body) != KwWhile || Kind(body + 1) != LeftParen)
				return -1;

			var test = Expr(body + 2, out var read);

			if (test < 0 || Kind(test) != RightParen || Kind(test + 1) != Semicolon)
				return -1;

			var span = Span(i, test + 2);

			if (!_context.Loops(span))
				return -1;

			node = Expression.Loop(
				Expression.Block(
					inside!,
					Expression.Label(_context.Again(span)),
					Expression.Condition(
						read!,
						Expression.Empty(),
						Expression.Break(_context.Exit(span)),
						typeof(void))),
				_context.Exit(span));

			return test + 2;
		}

		int For(int i, out Expression? node)
		{
			node = null;

			if (Kind(i) != KwFor || Kind(i + 1) != LeftParen)
				return -1;

			var init = Statement(i + 2, out var start);

			if (init < 0)
				return -1;

			var test = Expr(init, out var read);

			if (test < 0 || Kind(test) != Semicolon)
				return -1;

			var step = Expr(test + 1, out var next);

			if (step < 0 || Kind(step) != RightParen)
				return -1;

			_context.Opening(Span(i, step + 1));

			var body = Statement(step + 1, out var inside);

			if (body < 0)
				return -1;

			var span = Span(i, body);

			if (!_context.Loops(span) || !_context.Scoped(span))
				return -1;

			node = _context.Block(
				[start!], span,
				Expression.Loop(
					Expression.Condition(
						read!,
						Expression.Block(inside!, Expression.Label(_context.Again(span)), next!),
						Expression.Break(_context.Exit(span)),
						typeof(void)),
					_context.Exit(span)));

			return body;
		}

		int Switch(int i, out Expression? node)
		{
			node = null;

			if (Kind(i) != KwSwitch || Kind(i + 1) != LeftParen)
				return -1;

			var value = Expr(i + 2, out var read);

			if (value < 0 || Kind(value) != RightParen || Kind(value + 1) != LeftBrace)
				return -1;

			_context.Breaking(Span(i, value + 2));

			var at    = value + 2;
			var cases = new List<SwitchCase>();

			while (Kind(at) == KwCase)
			{
				var one = Case(at, out var read2);

				if (one < 0)
					return -1;

				cases.Add(read2!);
				at = one;
			}

			var fallback = default(Expression);

			if (Kind(at) == KwDefault)
			{
				if (Kind(at + 1) != Colon)
					return -1;

				var body = Bodies(at + 2, out var statements);

				if (body < 0)
					return -1;

				fallback = Expression.Block(statements!);
				at       = body;
			}

			if (Kind(at) != RightBrace)
				return -1;

			var span = Span(i, at + 1);

			if (!_context.Breaks(span))
				return -1;

			node = Expression.Block(
				Expression.Switch(typeof(void), read!, fallback, null, cases.ToArray()),
				Expression.Label(_context.Exit(span)));

			return at + 1;
		}

		int Case(int i, out SwitchCase? node)
		{
			node = null;

			var test = Expr(i + 1, out var read);

			if (test < 0 || Kind(test) != Colon)
				return -1;

			var body = Bodies(test + 1, out var statements);

			if (body < 0)
				return -1;

			node = Expression.SwitchCase(Expression.Block(statements!), read!);

			return body;
		}

		/// <summary>One statement at least, and as many after it as there are.</summary>
		int Bodies(int i, out Expression[]? statements)
		{
			statements = null;

			var at   = i;
			var read = new List<Expression>();

			while (true)
			{
				var one = Statement(at, out var statement);

				if (one < 0)
					break;

				read.Add(statement!);
				at = one;
			}

			if (read.Count == 0)
				return -1;

			statements = read.ToArray();

			return at;
		}

		int Try(int i, out Expression? node)
		{
			node = null;

			if (Kind(i) != KwTry)
				return -1;

			var body = Block(i + 1, out var inside);

			if (body < 0)
				return -1;

			var at       = body;
			var handlers = new List<CatchBlock>();

			while (Kind(at) == KwCatch)
			{
				var one = Catch(at, out var handler);

				if (one < 0)
					return -1;

				handlers.Add(handler!);
				at = one;
			}

			if (Kind(at) == KwFinally)
			{
				var final = Block(at + 1, out var last);

				if (final < 0)
					return -1;

				node = handlers.Count > 0
					? Expression.TryCatchFinally(inside!, last!, handlers.ToArray())
					: Expression.TryFinally(inside!, last!);

				return final;
			}

			if (handlers.Count == 0)
				return -1;

			node = Expression.TryCatch(inside!, handlers.ToArray());

			return at;
		}

		int Catch(int i, out CatchBlock? node)
		{
			node = null;

			if (Kind(i) != KwCatch || Kind(i + 1) != LeftParen)
				return -1;

			var at = Type(i + 2, out var type);

			if (at < 0 || Kind(at) != Identifier)
				return -1;

			var name = Cut(at);

			if (!_context.Declare(type!, name, Span(i, at + 1)))
				return -1;

			if (Kind(at + 1) != RightParen)
				return -1;

			var body = Block(at + 2, out var inside);

			if (body < 0)
				return -1;

			var span = Span(i, body);

			if (!_context.Scoped(span))
				return -1;

			node = Expression.Catch(_context.Named(name, span), inside!);

			return body;
		}

		int Jump(int i, out Expression? node)
		{
			node = null;

			var kind = Kind(i);

			if (kind == KwBreak)
			{
				node = Expression.Break(_context.Exit(Span(i, i + 1)));

				return i + 1;
			}

			if (kind == KwContinue)
			{
				node = Expression.Continue(_context.Again(Span(i, i + 1)));

				return i + 1;
			}

			if (kind != KwThrow)
				return -1;

			var value = Expr(i + 1, out var read);

			if (value >= 0)
			{
				node = Expression.Throw(read!);

				return value;
			}

			node = Expression.Rethrow();

			return i + 1;
		}

		// ── The operators ───────────────────────────────────────────────────────

		public int Expr(int i, out Expression? node) => Assignment(i, out node);

		int Assignment(int i, out Expression? node)
		{
			node = null;

			// An element is written to by one alternative and not by eleven: an index is
			// an expression, and reading it once for each operator is what makes a nest of
			// them cost what it should not.
			if (Kind(i) == Identifier)
			{
				var name = Name(i, out var written);

				if (name >= 0 && Kind(name) == LeftBracket)
				{
					var at = Indices(name, out var indices);

					if (at >= 0 && Kind(at) == Assign && Kind(at + 1) != Assign)
					{
						var value = Assignment(at + 1, out var read);

						if (value < 0)
							return -1;

						node = Expression.Assign(ExpressionParser.Place(written!, indices!), read!);

						return value;
					}
				}
			}

			var target = Target(i, out var place);

			if (target >= 0)
			{
				var operation = Kind(target);
				var compound  = true;

				if (operation == Assign && Kind(target + 1) == Assign)
					compound = false;

				if (compound && IsAssignment(operation))
				{
					var value = Assignment(target + 1, out var read);

					if (value < 0)
						return -1;

					node = Assigned(operation, place!, read!);

					return value;
				}
			}

			return Conditional(i, out node);
		}

		static bool IsAssignment(byte kind) =>
			kind is Assign or PlusAssign or MinusAssign or StarAssign or SlashAssign or
				PercentAssign or AmpAssign or PipeAssign or CaretAssign or LeftAssign or RightAssign;

		readonly Expression Assigned(byte operation, Expression target, Expression value) =>
			operation switch
			{
				PlusAssign    => ExpressionParser.AddAssign(target, value, Marks),
				MinusAssign   => ExpressionParser.SubtractAssign(target, value, Marks),
				StarAssign    => ExpressionParser.MultiplyAssign(target, value, Marks),
				SlashAssign   => Expression.DivideAssign(target, value),
				PercentAssign => Expression.ModuloAssign(target, value),
				AmpAssign     => Expression.AndAssign(target, value),
				PipeAssign    => Expression.OrAssign(target, value),
				CaretAssign   => Expression.ExclusiveOrAssign(target, value),
				LeftAssign    => Expression.LeftShiftAssign(target, value),
				RightAssign   => Expression.RightShiftAssign(target, value),
				_             => Expression.Assign(target, value),
			};

		/// <summary>What may be written to: a name, or a member of one.</summary>
		int Target(int i, out Expression? node)
		{
			var at = Name(i, out node);

			if (at < 0)
				return -1;

			if (Kind(at) != Dot || Kind(at + 1) != Identifier)
				return at;

			var member = Cut(at + 1);

			if (!ExpressionParser.Has(node!, member))
				return at;

			node = ExpressionParser.Member(node!, member);

			return at + 2;
		}

		int Conditional(int i, out Expression? node)
		{
			var at = Binary(i, 1, out node);

			if (at < 0 || Kind(at) != Question)
				return at;

			var then = Conditional(at + 1, out var whenTrue);

			if (then < 0 || Kind(then) != Colon)
				return -1;

			var otherwise = Conditional(then + 1, out var whenFalse);

			if (otherwise < 0)
				return -1;

			node = ExpressionParser.Chosen(node!, whenTrue, whenFalse);

			return otherwise;
		}

		/// <summary>
		/// C#'s ladder, from `||` at the loosest to `*` at the tightest, as a loop over a
		/// precedence rather than ten rules that differ only in a number.
		/// </summary>
		/// <remarks>
		/// The grammar has to write a rule per level, because that is how a grammar says
		/// which binds tighter, and §4.3 folds each of them into a loop of its own. A
		/// person writes the number down and loops once — the same language, and one frame
		/// per expression rather than ten.
		/// </remarks>
		int Binary(int i, int least, out Expression? node)
		{
			// `??` groups to the right and sits between `?:` and `||`, so it is written
			// here rather than in the table: a level that folds leftwards cannot say it.
			if (least <= 1)
			{
				var at = Binary(i, 2, out node);

				if (at < 0 || Kind(at) != Coalesce)
					return at;

				var right = Binary(at + 1, 1, out var other);

				if (right < 0)
					return -1;

				node = ExpressionParser.Coalesced(node!, other);

				return right;
			}

			var read = Unary(i, out node);

			if (read < 0)
				return -1;

			while (true)
			{
				var level = Level(read, out var width);

				if (level < least)
					return read;

				// `is` and `as` are the two whose right side is a type and not an operand,
				// and they sit at the relational level because C# puts them there.
				if (Kind(read) == KwIs || Kind(read) == KwAs)
				{
					var named = Type(read + 1, out var type);

					if (named < 0)
						return -1;

					node = Kind(read) == KwIs
						? Expression.TypeIs(node!, type!)
						: Expression.TypeAs(node!, type!);
					read = named;

					continue;
				}

				var operation = Kind(read);
				var right     = Binary(read + width, level + 1, out var other);

				if (right < 0)
					return -1;

				node = width == 2 ? Shifted(operation, node!, other!) : Applied(operation, node!, other!);
				read = right;
			}
		}

		/// <summary>
		/// Which level the operator at this token belongs to, and how many tokens it is.
		/// </summary>
		/// <remarks>
		/// Two of them are two tokens: a shift is `&lt;` or `&gt;` written twice with
		/// nothing between them, which is what lets `List&lt;List&lt;int&gt;&gt;` close two
		/// argument lists. The comparison one level out is the same character once, and
		/// what tells them apart is whether the second stands right against the first.
		/// </remarks>
		readonly int Level(int i, out int width)
		{
			width = 1;

			var kind = Kind(i);

			if (kind == OrElse)   return 2;
			if (kind == AndAlso)  return 3;
			if (kind == Pipe)     return Kind(i + 1) == Pipe ? 0 : 4;
			if (kind == Caret)    return 5;
			if (kind == Amp)      return Kind(i + 1) == Amp ? 0 : 6;
			if (kind == Equal || kind == NotEqual) return 7;

			if (kind == Less || kind == Greater)
			{
				if (Kind(i + 1) == kind)
				{
					if (_starts[i] + _lengths[i] != _starts[i + 1])
						return 0;

					width = 2;

					return 9;
				}

				return 8;
			}

			if (kind == LessEq || kind == GreaterEq) return 8;
			if (kind == KwIs   || kind == KwAs)      return 8;
			if (kind == Plus   || kind == Minus)     return 10;
			if (kind == Star || kind == Slash || kind == Percent) return 11;

			return 0;
		}

		readonly Expression Applied(byte operation, Expression left, Expression right) =>
			operation switch
			{
				OrElse    => Expression.OrElse(left, right),
				AndAlso   => Expression.AndAlso(left, right),
				Pipe      => Expression.Or(left, right),
				Caret     => Expression.ExclusiveOr(left, right),
				Amp       => Expression.And(left, right),
				Equal     => Expression.Equal(left, right),
				NotEqual  => Expression.NotEqual(left, right),
				LessEq    => Expression.LessThanOrEqual(left, right),
				GreaterEq => Expression.GreaterThanOrEqual(left, right),
				Less      => Expression.LessThan(left, right),
				Greater   => Expression.GreaterThan(left, right),
				Plus      => ExpressionParser.Add(left, right, Marks),
				Minus     => ExpressionParser.Subtract(left, right, Marks),
				Star      => ExpressionParser.Multiply(left, right, Marks),
				Slash     => Expression.Divide(left, right),
				_         => Expression.Modulo(left, right),
			};

		/// <summary>The shift, which the table above reaches with a width of two.</summary>
		readonly Expression Shifted(byte operation, Expression left, Expression right) =>
			operation == Less
				? Expression.LeftShift(left, right)
				: Expression.RightShift(left, right);

		int Unary(int i, out Expression? node)
		{
			node = null;

			var kind = Kind(i);

			// `++` and `--` before `+` and `-`, and over a name for the reason assignment
			// is: they write to what they read.
			if (kind == Increment || kind == Decrement)
			{
				var at = Name(i + 1, out var target);

				if (at < 0)
					return -1;

				node = kind == Increment
					? Expression.PreIncrementAssign(target!)
					: Expression.PreDecrementAssign(target!);

				return at;
			}

			if (kind == Minus || kind == Plus || kind == Not || kind == Tilde)
			{
				var at = Unary(i + 1, out var operand);

				if (at < 0)
					return -1;

				node = kind switch
				{
					Minus => ExpressionParser.Negate(operand!, Marks),
					Plus  => Expression.UnaryPlus(operand!),
					Not   => Expression.Not(operand!),
					_     => Expression.OnesComplement(operand!),
				};

				return at;
			}

			// A cast is told from a parenthesized expression by what stands inside it, and
			// where that is no type this reading is simply not a cast.
			if (kind == LeftParen)
			{
				var named = Type(i + 1, out var type);

				if (named >= 0 && Kind(named) == RightParen)
				{
					var operand = Unary(named + 1, out var read);

					if (operand >= 0)
					{
						node = ExpressionParser.Cast(read!, type!, Marks);

						return operand;
					}
				}
			}

			return Postfix(i, out node);
		}

		/// <summary>
		/// Everything written after an operand: a member, a call, an index, in a chain read
		/// once from left to right.
		/// </summary>
		int Postfix(int i, out Expression? node)
		{
			node = null;

			var at = -1;

			// The three heads that are a name and something: a call of one, and the two
			// that write to one.
			if (Kind(i) == Identifier)
			{
				var name = Name(i, out var target);

				if (name >= 0)
				{
					var arguments = Arguments(name, out var args);

					if (arguments >= 0)
					{
						node = Expression.Invoke(target!, args!);
						at   = arguments;
					}
					else if (Kind(name) == Increment)
					{
						node = Expression.PostIncrementAssign(target!);
						at   = name + 1;
					}
					else if (Kind(name) == Decrement)
					{
						node = Expression.PostDecrementAssign(target!);
						at   = name + 1;
					}
				}
			}

			if (at < 0)
			{
				at = Primary(i, out node);

				if (at < 0)
					return -1;
			}

			while (true)
			{
				if (Kind(at) == Dot && Kind(at + 1) == Identifier)
				{
					var member    = Cut(at + 1);
					var arguments = Arguments(at + 2, out var args);

					if (arguments >= 0)
					{
						node = Expression.Call(node!, member, null, args!);
						at   = arguments;

						continue;
					}

					node = ExpressionParser.Member(node!, member);
					at  += 2;

					continue;
				}

				if (Kind(at) == LeftBracket)
				{
					var indices = Indices(at, out var read);

					if (indices < 0)
						break;

					node = ExpressionParser.Indexed(node!, read!);
					at   = indices;

					continue;
				}

				break;
			}

			return at;
		}

		// ── What an operand is ──────────────────────────────────────────────────

		int Primary(int i, out Expression? node)
		{
			node = null;

			var kind = Kind(i);

			if (kind == KwNew)
				return New(i, out node);

			// A type and something of it, told from `a.b` by whether the name resolves.
			if (kind == Identifier)
			{
				var named = Named(i, out var type);

				if (named >= 0 && Kind(named) == Dot && Kind(named + 1) == Identifier)
				{
					var member    = Cut(named + 1);
					var arguments = Arguments(named + 2, out var args);

					if (arguments >= 0)
					{
						node = Expression.Call(type!, member, null, args!);

						return arguments;
					}

					node = ExpressionParser.StaticMember(type!, member);

					return named + 2;
				}
			}

			if (kind == KwChecked || kind == KwUnchecked)
			{
				if (Kind(i + 1) != LeftParen)
					return -1;

				Mark(kind == KwChecked
					? ExpressionParser.Reading.Checked
					: ExpressionParser.Reading.Unchecked);

				var inner = Expr(i + 2, out node);

				_marked--;

				if (inner < 0 || Kind(inner) != RightParen)
					return -1;

				return inner + 1;
			}

			if (kind == LeftParen)
			{
				var inner = Expr(i + 1, out node);

				if (inner < 0 || Kind(inner) != RightParen)
					return -1;

				return inner + 1;
			}

			if (kind == KwTrue)
			{
				node = Expression.Constant(true);

				return i + 1;
			}

			if (kind == KwFalse)
			{
				node = Expression.Constant(false);

				return i + 1;
			}

			if (kind == KwNull)
			{
				node = Expression.Constant(null, typeof(object));

				return i + 1;
			}

			if (kind >= Number && kind <= Character)
			{
				node = Literal(i, kind);

				return i + 1;
			}

			return Name(i, out node);
		}

		int New(int i, out Expression? node)
		{
			node = null;

			var at = Type(i + 1, out var type);

			if (at < 0)
				return -1;

			// `new int[n]`: a size, and the array is the element type's.
			if (Kind(at) == LeftBracket)
			{
				var size = Expr(at + 1, out var read);

				if (size < 0 || Kind(size) != RightBracket)
					return -1;

				node = Expression.NewArrayBounds(type!, read!);

				return size + 1;
			}

			// `new int[] { … }`: the brackets belong to the type, and the braces are what
			// tell this from a constructor.
			if (type!.IsArray && Kind(at) == LeftBrace)
			{
				var items = new List<Expression>();
				var read  = at + 1;

				if (Kind(read) != RightBrace)
				{
					var first = Expr(read, out var one);

					if (first < 0)
						return -1;

					items.Add(one!);
					read = first;

					while (Kind(read) == Comma)
					{
						var more = Expr(read + 1, out var next);

						if (more < 0)
							return -1;

						items.Add(next!);
						read = more;
					}
				}

				if (Kind(read) != RightBrace)
					return -1;

				node = Expression.NewArrayInit(type.GetElementType()!, items.ToArray());

				return read + 1;
			}

			var arguments = Arguments(at, out var args);

			if (arguments < 0)
				return -1;

			var fields   = default(ExpressionParser.Setting[]);
			var elements = default(ExpressionParser.Element[]);
			var after    = arguments;

			// One tail rather than three alternatives: what stands inside the braces is
			// what says which of the two it is, and `Name =` is the narrower.
			if (Kind(after) == LeftBrace)
			{
				var bound = Bindings(after, out fields);

				if (bound >= 0)
				{
					after = bound;
				}
				else
				{
					var listed = Elements(after + 1, out elements);

					if (listed >= 0 && Kind(listed) == RightBrace)
						after = listed + 1;
				}
			}

			node = ExpressionParser.Made(type, args!, fields, elements);

			return after;
		}

		int Bindings(int i, out ExpressionParser.Setting[]? settings)
		{
			settings = null;

			if (Kind(i) != LeftBrace)
				return -1;

			var read = new List<ExpressionParser.Setting>();
			var at   = Binding(i + 1, out var first);

			if (at < 0)
				return -1;

			read.Add(first);

			while (Kind(at) == Comma)
			{
				var more = Binding(at + 1, out var next);

				if (more < 0)
					return -1;

				read.Add(next);
				at = more;
			}

			if (Kind(at) != RightBrace)
				return -1;

			settings = read.ToArray();

			return at + 1;
		}

		int Binding(int i, out ExpressionParser.Setting setting)
		{
			setting = default;

			if (Kind(i) != Identifier || Kind(i + 1) != Assign)
				return -1;

			var name = Cut(i);

			if (Kind(i + 2) == LeftBrace)
			{
				var nested = Bindings(i + 2, out var inside);

				if (nested >= 0)
				{
					setting = new ExpressionParser.Setting(name, null, inside, null);

					return nested;
				}

				var listed = Elements(i + 3, out var items);

				if (listed >= 0 && Kind(listed) == RightBrace)
				{
					setting = new ExpressionParser.Setting(name, null, null, items);

					return listed + 1;
				}
			}

			var value = Expr(i + 2, out var read);

			if (value < 0)
				return -1;

			setting = new ExpressionParser.Setting(name, read, null, null);

			return value;
		}

		int Elements(int i, out ExpressionParser.Element[]? elements)
		{
			elements = null;

			var read = new List<ExpressionParser.Element>();
			var at   = Element(i, out var first);

			if (at < 0)
				return -1;

			read.Add(first);

			while (Kind(at) == Comma)
			{
				var more = Element(at + 1, out var next);

				if (more < 0)
					return -1;

				read.Add(next);
				at = more;
			}

			elements = read.ToArray();

			return at;
		}

		int Element(int i, out ExpressionParser.Element element)
		{
			element = default;

			// A braced element is what one `Add` of two arguments takes, which is how C#
			// writes an entry of a dictionary.
			if (Kind(i) == LeftBrace)
			{
				var arguments = new List<Expression>();
				var at        = Expr(i + 1, out var first);

				if (at < 0)
					return -1;

				arguments.Add(first!);

				while (Kind(at) == Comma)
				{
					var more = Expr(at + 1, out var next);

					if (more < 0)
						return -1;

					arguments.Add(next!);
					at = more;
				}

				if (Kind(at) != RightBrace)
					return -1;

				element = new ExpressionParser.Element(arguments.ToArray());

				return at + 1;
			}

			var only = Expr(i, out var value);

			if (only < 0)
				return -1;

			element = ExpressionParser.Only(value!);

			return only;
		}

		int Arguments(int i, out Expression[]? arguments)
		{
			arguments = null;

			if (Kind(i) != LeftParen)
				return -1;

			var read = new List<Expression>();
			var at   = i + 1;

			if (Kind(at) != RightParen)
			{
				var first = Expr(at, out var one);

				if (first < 0)
					return -1;

				read.Add(one!);
				at = first;

				while (Kind(at) == Comma)
				{
					var more = Expr(at + 1, out var next);

					if (more < 0)
						return -1;

					read.Add(next!);
					at = more;
				}
			}

			if (Kind(at) != RightParen)
				return -1;

			arguments = read.ToArray();

			return at + 1;
		}

		int Indices(int i, out Expression[]? indices)
		{
			indices = null;

			if (Kind(i) != LeftBracket)
				return -1;

			var read  = new List<Expression>();
			var at    = Expr(i + 1, out var first);

			if (at < 0)
				return -1;

			read.Add(first!);

			while (Kind(at) == Comma)
			{
				var more = Expr(at + 1, out var next);

				if (more < 0)
					return -1;

				read.Add(next!);
				at = more;
			}

			if (Kind(at) != RightBracket)
				return -1;

			indices = read.ToArray();

			return at + 1;
		}

		/// <summary>A word that names a variable, which is what makes it a name at all.</summary>
		int Name(int i, out Expression? node)
		{
			node = null;

			if (Kind(i) != Identifier)
				return -1;

			var name = Cut(i);
			var span = Span(i, i + 1);

			if (!_context.Knows(name, span))
				return -1;

			node = _context.Named(name, span);

			return i + 1;
		}

		// ── Constants ───────────────────────────────────────────────────────────

		readonly Expression Literal(int i, byte kind)
		{
			if (kind == Text)      return Expression.Constant(Unescaped(i, false));
			if (kind == Character) return Expression.Constant(Unescaped(i, false)[0]);
			if (kind == Verbatim)  return Expression.Constant(Unescaped(i, true));

			var digits = Digits(i, kind);

			return kind switch
			{
				Real     => Expression.Constant(double.Parse(digits, NumberStyles.Float, CultureInfo.InvariantCulture)),
				RealD    => Expression.Constant(double.Parse(digits, NumberStyles.Float, CultureInfo.InvariantCulture)),
				RealF    => Expression.Constant(float.Parse(digits, NumberStyles.Float, CultureInfo.InvariantCulture)),
				RealM    => Expression.Constant(decimal.Parse(digits, NumberStyles.Float, CultureInfo.InvariantCulture)),
				// Which type an integer is depends on its value, and the parser's own answer
				// to that is the one to give: the two are held against each other.
				Hex      => ExpressionParser.Integer(digits, 16, unsigned: false, wide: false),
				HexU     => ExpressionParser.Integer(digits, 16, unsigned: true,  wide: false),
				HexL     => ExpressionParser.Integer(digits, 16, unsigned: false, wide: true),
				HexUL    => ExpressionParser.Integer(digits, 16, unsigned: true,  wide: true),
				Bits     => ExpressionParser.Integer(digits, 2,  unsigned: false, wide: false),
				BitsU    => ExpressionParser.Integer(digits, 2,  unsigned: true,  wide: false),
				BitsL    => ExpressionParser.Integer(digits, 2,  unsigned: false, wide: true),
				BitsUL   => ExpressionParser.Integer(digits, 2,  unsigned: true,  wide: true),
				NumberU  => ExpressionParser.Integer(digits, 10, unsigned: true,  wide: false),
				NumberL  => ExpressionParser.Integer(digits, 10, unsigned: false, wide: true),
				NumberUL => ExpressionParser.Integer(digits, 10, unsigned: true,  wide: true),
				_        => ExpressionParser.Integer(digits, 10, unsigned: false, wide: false),
			};
		}

		/// <summary>
		/// The digits of a number, without the base it was written in, the suffix that says
		/// its type, or the separators that are no part of its value.
		/// </summary>
		readonly string Digits(int i, byte kind)
		{
			var from = _starts[i];
			var to   = from + _lengths[i];

			// The base is a prefix, and the suffix is the letter or two at the end. Which
			// of them the number has, the kind already says: the lexer walked the
			// characters once and there is nothing here to work out again.
			if (kind is >= Hex and <= BitsUL)
				from += 2;

			to -= kind switch
			{
				NumberU or NumberL or HexU or HexL or BitsU or BitsL => 1,
				RealD or RealF or RealM                                  => 1,
				NumberUL or HexUL or BitsUL                            => 2,
				_                                                        => 0,
			};

			var span = _text.AsSpan(from, to - from);

			return span.IndexOf('_') < 0 ? span.ToString() : span.ToString().Replace("_", "");
		}

		/// <summary>The text a quoted run stands for, with the escapes it wrote read back.</summary>
		readonly string Unescaped(int i, bool verbatim)
		{
			var from = _starts[i] + (verbatim ? 2 : 1);
			var to   = _starts[i] + _lengths[i] - 1;
			var made = new System.Text.StringBuilder(to - from);

			for (var at = from; at < to; at++)
			{
				var c = _text[at];

				if (verbatim)
				{
					made.Append(c);

					if (c == '"')
						at++;

					continue;
				}

				if (c != '\\')
				{
					made.Append(c);

					continue;
				}

				at++;

				switch (_text[at])
				{
					case 'a':  made.Append('\a'); break;
					case 'b':  made.Append('\b'); break;
					case 'f':  made.Append('\f'); break;
					case 'n':  made.Append('\n'); break;
					case 'r':  made.Append('\r'); break;
					case 't':  made.Append('\t'); break;
					case 'v':  made.Append('\v'); break;
					case '0':  made.Append('\0'); break;
					case '\\': made.Append('\\'); break;
					case '\'': made.Append('\''); break;
					case '"':  made.Append('"');  break;

					case 'u':
						made.Append((char)Convert.ToInt32(_text.Substring(at + 1, 4), 16));
						at += 4;
						break;

					case 'U':
						made.Append(char.ConvertFromUtf32(Convert.ToInt32(_text.Substring(at + 1, 8), 16)));
						at += 8;
						break;

					case 'x':
					{
						var width = 0;

						while (width < 4 && at + 1 + width < to && IsHex(_text[at + 1 + width]))
							width++;

						made.Append((char)Convert.ToInt32(_text.Substring(at + 1, width), 16));
						at += width;
						break;
					}

					default:
						made.Append(_text[at]);
						break;
				}
			}

			return made.ToString();
		}
	}
}
