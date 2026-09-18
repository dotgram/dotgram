using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

using DotGram.ExpressionLanguage;

namespace DotGram.Handwritten;

using Match      = ExpressionParser.Match<Expression>;
using Segment    = ExpressionParser.Segment;
using SourceSpan = ExpressionParser.SourceSpan;
using State      = ExpressionParser.State;

/// <summary>
/// The expression language of <see cref="ExpressionParser"/>, written by hand: a lexer
/// over the whole input and a recursive descent over the tokens it makes.
/// </summary>
/// <remarks>
/// <para>
/// The mark the generated parser is measured against, and the answer to "how fast would a
/// person have written this". It has to keep <em>looking</em> hand-written: what is here is
/// what someone would write who knew the language and cared about the result, and nothing is
/// shaped by how the generator happens to work.
/// </para>
/// <para>
/// It reads exactly the language the grammar does (docs/design/architecture-decisions.md, D1):
/// the same publications, the same accepted and refused input, the same trees, and a refusal
/// at the same position. <c>ExpressionHandTests</c> holds it to that on every run of the suite.
/// </para>
/// <para>
/// It calls the same factories the grammar's <c>=&gt;</c> calls, and hands the same
/// <see cref="ExpressionParser.State"/> the same spans. That is deliberate: what is being
/// compared is the reading, not the building, and a second implementation of scopes and names
/// would be a second thing to be wrong.
/// </para>
/// <para>
/// Two shapes here are a person's rather than a grammar's, and both are where the two differ
/// most. The operator ladder is one loop over a precedence rather than ten rules — C#'s table,
/// read out of an array — because that is how a person writes ten levels that differ only in a
/// number. And the suffixes of a postfix chain are a loop rather than a left recursion, which
/// is the same saving by the same argument.
/// </para>
/// <para>
/// It builds where it reads, which the grammar's own contract does not (§7.3: a text is read
/// whole and built afterwards). The two part only where building refuses — an operator its
/// operands do not have, a member that is not there — in a text that is refused later anyway:
/// read whole first, it is refused where it stops; built as it is read, it throws first. So a
/// refusal thrown while building sends the reader over the text once more, building nothing,
/// from the state it began in, and a text that does not read is answered as one. That second
/// pass is the price of the contract, not of how the generator is written, and it is paid only
/// by a text that throws.
/// </para>
/// </remarks>
public static class HandExpression
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
	const byte Number   = 46;
	const byte NumberU  = 47;
	const byte NumberL  = 48;
	const byte NumberUL = 49;
	const byte Hex      = 50;
	const byte HexU     = 51;
	const byte HexL     = 52;
	const byte HexUL    = 53;
	const byte Bits     = 54;
	const byte BitsU    = 55;
	const byte BitsL    = 56;
	const byte BitsUL   = 57;
	const byte Real     = 58;
	const byte RealD    = 59;
	const byte RealF    = 60;
	const byte RealM    = 61;

	const byte Text      = 62;
	const byte Verbatim  = 63;
	const byte Character = 64;

	// The strings with holes, and the raw ones. The lexer only finds where each ends; what
	// one is made of is cut out of it again where it is built, which is where it is wanted.
	const byte Interpolated = 65;   // `$"…"`, `$@"…"` and `@$"…"`
	const byte RawText      = 66;   // three to five quotes, no dollar
	const byte RawHoles     = 67;   // three to five quotes after one dollar or two
	const byte RawLong      = 68;   // six quotes or more, or three dollars or more

	const byte Identifier = 69;

	// A word the ASCII reading does not take: it is one token all the same, since the letters
	// that end it are what C# would read as one, and it is no name and no keyword.
	const byte Foreign = 70;

	// The keywords, in the order the language reserves them (the grammar's `Keyword`). A word
	// that is one of these is never a name, which is the whole of what makes it a keyword.
	const byte FirstWord = 71;

	static readonly string[] Words =
	[
		"as",      "bool",      "break",    "byte",      "case",    "catch",
		"char",    "checked",   "continue", "decimal",   "default", "do",
		"double",  "else",      "false",    "finally",   "float",   "for",
		"foreach", "if",        "in",       "int",       "is",      "long",
		"nameof",  "new",       "null",     "object",    "return",  "sbyte",
		"short",   "string",    "switch",   "throw",     "true",    "try",
		"typeof",  "uint",      "ulong",    "unchecked", "ushort",  "using",
		"while",
	];

	// A word that is not in the list would be `FirstWord - 1`, which is `Foreign`, and a
	// parser that reads every keyword as a foreign word refuses everything. Said here, where
	// the word is known, rather than found later as a parser that reads nothing.
	static byte Of(string word)
	{
		var at = Array.IndexOf(Words, word);

		if (at < 0)
			throw new ArgumentOutOfRangeException(nameof(word), word, "Not a word this language reserves.");

		return (byte)(FirstWord + at);
	}

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
	static readonly byte KwForeach   = Of("foreach");
	static readonly byte KwIf        = Of("if");
	static readonly byte KwIn        = Of("in");
	static readonly byte KwInt       = Of("int");
	static readonly byte KwIs        = Of("is");
	static readonly byte KwLong      = Of("long");
	static readonly byte KwNameof    = Of("nameof");
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
	static readonly byte KwTypeof    = Of("typeof");
	static readonly byte KwUint      = Of("uint");
	static readonly byte KwUlong     = Of("ulong");
	static readonly byte KwUnchecked = Of("unchecked");
	static readonly byte KwUshort    = Of("ushort");
	static readonly byte KwUsing     = Of("using");
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
				if (w[0] == 'i' && w[1] == 'n') return KwIn;
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
					case 'b': if (Same(w, "bool")) return KwBool; if (Same(w, "byte")) return KwByte; break;
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
					case 's': if (Same(w, "short")) return KwShort;  if (Same(w, "sbyte")) return KwSbyte; break;
					case 't': if (Same(w, "throw")) return KwThrow;  break;
					case 'u': if (Same(w, "ulong")) return KwUlong;  if (Same(w, "using")) return KwUsing; break;
					case 'w': if (Same(w, "while")) return KwWhile;  break;
				}

				break;

			case 6:
				switch (w[0])
				{
					case 'd': if (Same(w, "double")) return KwDouble; break;
					case 'n': if (Same(w, "nameof")) return KwNameof; break;
					case 'o': if (Same(w, "object")) return KwObject; break;
					case 'r': if (Same(w, "return")) return KwReturn; break;
					case 's': if (Same(w, "string")) return KwString; if (Same(w, "switch")) return KwSwitch; break;
					case 't': if (Same(w, "typeof")) return KwTypeof; break;
					case 'u': if (Same(w, "ushort")) return KwUshort; break;
				}

				break;

			case 7:
				if (Same(w, "checked")) return KwChecked;
				if (Same(w, "decimal")) return KwDecimal;
				if (Same(w, "default")) return KwDefault;
				if (Same(w, "finally")) return KwFinally;
				if (Same(w, "foreach")) return KwForeach;
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

	// ── The publications ────────────────────────────────────────────────────────

	/// <summary>The whole text as a lambda, answering as the grammar's <c>TryParseLambda</c> does.</summary>
	internal static ExpressionParser.Match<LambdaExpression> TryParseLambda(string text, State context)
	{
		return Whole(text, context, ascii: false);
	}

	/// <summary>The same, with a name spelled in ASCII alone, as <c>TryParseAsciiLambda</c> reads one.</summary>
	internal static ExpressionParser.Match<LambdaExpression> TryParseAsciiLambda(string text, State context)
	{
		return Whole(text, context, ascii: true);
	}

	/// <summary>Whether the whole text reads as a lambda, building nothing of it.</summary>
	/// <remarks>
	/// What the reader does where it reads again after building threw, and the first time
	/// through the body of a lambda that says no types: the test that holds it to building
	/// nothing hands it texts every construction of which would throw.
	/// </remarks>
	internal static bool Recognizes(string text, State context)
	{
		var tokens = Tokens.Rent();

		try
		{
			if (Lex(text, 0, text.Length, tokens, ascii: false) >= 0)
				return false;

			var reader = new Reader(tokens, text, context, ascii: false, build: false);

			return reader.Lambda(0, out _) == tokens.Count;
		}
		finally
		{
			Tokens.Return(tokens);
		}
	}

	/// <summary>The tokens of the text, or -1 where the lexer stops short of its end: the lexer alone, timed.</summary>
	public static int LexOnly(string text)
	{
		var tokens = Tokens.Rent();

		try
		{
			return Lex(text, 0, text.Length, tokens, ascii: false) < 0 ? tokens.Count : -1;
		}
		finally
		{
			Tokens.Return(tokens);
		}
	}

	// What a hole of an interpolated string and the body of a lambda that said no types are
	// read with, over a window of the text: the grammar's `ParseHole` and `ParseBody`, and the
	// same two under the ASCII reading.
	static Match Hole(string input, int at, int length, State context)
	{
		return Window(input, at, length, context, ascii: false, body: false);
	}

	static Match AsciiHole(string input, int at, int length, State context)
	{
		return Window(input, at, length, context, ascii: true, body: false);
	}

	static Match Body(string input, int at, int length, State context)
	{
		return Window(input, at, length, context, ascii: false, body: true);
	}

	static Match AsciiBody(string input, int at, int length, State context)
	{
		return Window(input, at, length, context, ascii: true, body: true);
	}

	static ExpressionParser.Match<LambdaExpression> Whole(string text, State context, bool ascii)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var tokens = Tokens.Rent();

		try
		{
			// A character no token begins with is refused before anything is read, wherever it
			// stands, because the generated lexer cuts the whole text into tokens first. That is
			// how the generator reads, not what the language says: a lexer that made tokens as
			// they were asked for would stop at the first mistake the reading met instead.
			var stopped = Lex(text, 0, text.Length, tokens, ascii);

			if (stopped >= 0)
				return ExpressionParser.Match<LambdaExpression>.Failed(
					ExpressionParser.Outcome.NoMatch, Unexpected(text, stopped), stopped, null, null);

			var from = context.Mark();

			LambdaExpression? lambda;
			int               end;
			int               furthest;

			try
			{
				var reader = new Reader(tokens, text, context, ascii, build: true);

				end      = reader.Lambda(0, out lambda);
				furthest = reader.Furthest;
			}
			catch (Exception refused) when (IsRefusal(refused))
			{
				// Built where it was read and refused by what it built: the grammar reads the whole
				// text before building any of it, so what the text owes is what reading alone says.
				context.Rollback(from);

				var reader = new Reader(tokens, text, context, ascii, build: false);

				end      = reader.Lambda(0, out _);
				furthest = reader.Furthest;

				if (end == tokens.Count)
					throw;

				lambda = null;
			}

			if (end == tokens.Count)
				return ExpressionParser.Match<LambdaExpression>.Success(lambda!, 0, Over(tokens, end));

			// A lambda that ends before the text does is refused where it ends.
			if (end > furthest)
				furthest = end;

			return furthest < tokens.Count
				? ExpressionParser.Match<LambdaExpression>.Failed(
					ExpressionParser.Outcome.NoMatch, "Input does not match 'Lambda'.", tokens.Starts[furthest], null, null)
				: ExpressionParser.Match<LambdaExpression>.Failed(
					ExpressionParser.Outcome.Starved, "Expected more input.", text.Length, null, null);
		}
		finally
		{
			Tokens.Return(tokens);
		}
	}

	/// <summary>An expression, or a lambda's body, read over a window of the text and not required to fill it.</summary>
	/// <remarks>
	/// The window's tokens are its own, and a character no token begins with ends them there
	/// rather than refusing the reading. The state is the reading's that asked, so the names the
	/// text around the window declared are names here — which is what a hole and a body are for.
	/// </remarks>
	static Match Window(string input, int at, int length, State context, bool ascii, bool body)
	{
		if (at < 0 || length < 0 || at > input.Length - length)
			return Match.Failed(ExpressionParser.Outcome.NoMatch, "The window is outside the input.", at, null, null);

		var tokens = Tokens.Rent();

		try
		{
			var stopped = Lex(input, at, at + length, tokens, ascii);
			var from    = context.Mark();

			Expression? value;
			int         end;
			int         furthest;

			try
			{
				var reader = new Reader(tokens, input, context, ascii, build: true);

				end      = body ? reader.Value(0, out value) : reader.Assignment(0, out value);
				furthest = reader.Furthest;
			}
			catch (Exception refused) when (IsRefusal(refused))
			{
				context.Rollback(from);

				var reader = new Reader(tokens, input, context, ascii, build: false);

				end      = body ? reader.Value(0, out _) : reader.Assignment(0, out _);
				furthest = reader.Furthest;

				if (end >= 0)
					throw;

				value = null;
			}

			if (end >= 0)
				return Match.Success(value!, at, end == 0 ? -at : Over(tokens, end) - at);

			return furthest < tokens.Count
				? Match.Failed(ExpressionParser.Outcome.NoMatch, "Input does not match.", tokens.Starts[furthest], null, null)
				: Match.Failed(ExpressionParser.Outcome.Starved, "Expected more input.", stopped >= 0 ? stopped : at + length, null, null);
		}
		finally
		{
			Tokens.Return(tokens);
		}
	}

	/// <summary>Where the last token read ends.</summary>
	static int Over(Tokens tokens, int end)
	{
		return end == 0 ? 0 : tokens.Starts[end - 1] + tokens.Lengths[end - 1];
	}

	static string Unexpected(string text, int at)
	{
		return at >= text.Length ? "Expected more input." : $"Unexpected character '{text[at]}'.";
	}

	/// <summary>Whether an exception is the text being refused, as the language's own <c>TryParse</c> tells one.</summary>
	static bool IsRefusal(Exception thrown)
	{
		return thrown is FormatException or InvalidOperationException or OverflowException ||
			thrown is ArgumentException and not ArgumentNullException;
	}

	// ── The lexer ───────────────────────────────────────────────────────────────

	/// <summary>One text's tokens, kept between readings the way the generated lexer keeps its own.</summary>
	/// <remarks>
	/// And what a reading of them keeps as it goes, which lives as long as they do: each word
	/// cut out of the text once, however many times it is asked about; and one stack the lists
	/// a reading builds — a block's statements, a call's arguments — are gathered on and copied
	/// off at their exact length, rather than a list of their own grown and copied again.
	/// </remarks>
	sealed class Tokens
	{
		public byte[]    Kinds   = new byte[64];
		public int[]     Starts  = new int[64];
		public int[]     Lengths = new int[64];
		public string?[] Words   = new string?[64];
		public int       Count;

		public Expression[] Values = new Expression[32];
		public int[]        Taken  = new int[16];

		/// <summary>How far up <see cref="Values"/> a reading went, which is what is cleared when it is done.</summary>
		public int Reached;

		// A few spare sets, one for each reading that can be under way at once: a text, a hole
		// read while it is built, a hole inside that hole. One spare would do for a text with no
		// holes and be held by it while every hole it has makes a set of its own.
		[ThreadStatic]
		static Tokens[]? _spares;

		[ThreadStatic]
		static int _kept;

		public static Tokens Rent()
		{
			return _kept > 0 ? _spares![--_kept] : new Tokens();
		}

		/// <summary>Kept for the next reading, holding on to nothing of this one's.</summary>
		public static void Return(Tokens tokens)
		{
			Array.Clear(tokens.Words, 0, tokens.Count);
			Array.Clear(tokens.Values, 0, tokens.Reached);

			tokens.Reached = 0;

			_spares ??= new Tokens[4];

			if (_kept < _spares.Length)
				_spares[_kept++] = tokens;
		}

		public void Room(int length)
		{
			if (Kinds.Length >= length)
				return;

			Kinds   = new byte[length];
			Starts  = new int[length];
			Lengths = new int[length];
			Words   = new string?[length];
		}

		public void Grow(int count)
		{
			if (Kinds.Length > count)
				return;

			var size = Kinds.Length < 64 ? 64 : Kinds.Length * 2;

			Array.Resize(ref Kinds,   size);
			Array.Resize(ref Starts,  size);
			Array.Resize(ref Lengths, size);
			Array.Resize(ref Words,   size);
		}
	}

	/// <summary>
	/// The text from one offset to another as tokens, whitespace skipped. Where a character
	/// fits no token the tokens end there, and where that is answers; -1 where none does.
	/// </summary>
	/// <remarks>
	/// Nothing past <paramref name="to"/> is looked at, so a token that would run over the end
	/// of a window is no token. Positions are offsets into the whole text all the same.
	/// </remarks>
	static int Lex(string text, int from, int to, Tokens into, bool ascii)
	{
		var s = text.AsSpan(0, to);

		into.Room((to - from) / 3 + 16);
		into.Count = 0;

		var kinds   = into.Kinds;
		var starts  = into.Starts;
		var lengths = into.Lengths;
		var count   = 0;
		var p       = from;
		var stopped = -1;

		while (true)
		{
			while (p < s.Length && (s[p] == ' ' || s[p] == '\t' || s[p] == '\r' || s[p] == '\n'))
				p++;

			if (p >= s.Length)
				break;

			var start = p;
			var c     = s[p];
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
				case ']': kind = RightBracket; p++; break;

				case '^': kind = Next(s, p) == '=' ? Take(ref p, 2, CaretAssign) : Take(ref p, 1, Caret); break;

				// `[]` is a type's own and is one token, the way a lexer takes the longest match.
				// `a[0]` is three, because what follows the bracket is not the other one.
				case '[': kind = Next(s, p) == ']' ? Take(ref p, 2, Brackets) : Take(ref p, 1, LeftBracket); break;
				case '?': kind = Next(s, p) == '?' ? Take(ref p, 2, Coalesce) : Take(ref p, 1, Question);    break;
				case '!': kind = Next(s, p) == '=' ? Take(ref p, 2, NotEqual) : Take(ref p, 1, Not);         break;
				case '*': kind = Next(s, p) == '=' ? Take(ref p, 2, StarAssign)    : Take(ref p, 1, Star);    break;
				case '/': kind = Next(s, p) == '=' ? Take(ref p, 2, SlashAssign)   : Take(ref p, 1, Slash);   break;
				case '%': kind = Next(s, p) == '=' ? Take(ref p, 2, PercentAssign) : Take(ref p, 1, Percent); break;

				case '=':
					kind = Next(s, p) switch
					{
						'=' => Take(ref p, 2, Equal),
						'>' => Take(ref p, 2, Arrow),
						_   => Take(ref p, 1, Assign),
					};
					break;

				// `<<` and `>>` are not tokens, and that is the language rather than an
				// omission: a shift is two of these written with nothing between them, so
				// that `List<List<int>>` can close two argument lists with the same two
				// characters C# closes them with. `<<=` and `>>=` are, being written as
				// one thing and never as two.
				case '<':
					kind = Next(s, p) == '<' && p + 2 < s.Length && s[p + 2] == '=' ? Take(ref p, 3, LeftAssign)
						: Next(s, p) == '=' ? Take(ref p, 2, LessEq)
						: Take(ref p, 1, Less);
					break;

				case '>':
					kind = Next(s, p) == '>' && p + 2 < s.Length && s[p + 2] == '=' ? Take(ref p, 3, RightAssign)
						: Next(s, p) == '=' ? Take(ref p, 2, GreaterEq)
						: Take(ref p, 1, Greater);
					break;

				case '&':
					kind = Next(s, p) switch
					{
						'&' => Take(ref p, 2, AndAlso),
						'=' => Take(ref p, 2, AmpAssign),
						_   => Take(ref p, 1, Amp),
					};
					break;

				case '|':
					kind = Next(s, p) switch
					{
						'|' => Take(ref p, 2, OrElse),
						'=' => Take(ref p, 2, PipeAssign),
						_   => Take(ref p, 1, Pipe),
					};
					break;

				case '+':
					kind = Next(s, p) switch
					{
						'+' => Take(ref p, 2, Increment),
						'=' => Take(ref p, 2, PlusAssign),
						_   => Take(ref p, 1, Plus),
					};
					break;

				case '-':
					kind = Next(s, p) switch
					{
						'-' => Take(ref p, 2, Decrement),
						'=' => Take(ref p, 2, MinusAssign),
						_   => Take(ref p, 1, Minus),
					};
					break;

				// A point begins a real where a digit follows it — `.5` is one — and is a
				// member access where anything else does.
				case '.':
					if (IsDigit(Next(s, p)))
						p = Numeric(s, p, out kind);
					else
						kind = Take(ref p, 1, Dot);
					break;

				case '"':
					p = Quoted(s, p, out kind);
					break;

				case '\'':
					kind = Character;
					p    = CharacterEnd(s, p);
					break;

				case '@':
					kind = Next(s, p) == '"' ? Verbatim : Interpolated;
					p    = Next(s, p) == '"' ? VerbatimEnd(s, p + 2)
						: Next(s, p) == '$' && p + 2 < s.Length && s[p + 2] == '"' ? InterpolatedEnd(s, p + 3, verbatim: true, null)
						: -1;
					break;

				case '$':
					p = Dollars(s, p, out kind);
					break;

				default:
					if (IsDigit(c))
					{
						p = Numeric(s, p, out kind);

						break;
					}

					if (!IsStart(c))
					{
						kind = End;
						p    = -1;

						break;
					}

					var plain = c < 128;

					p++;

					while (p < s.Length && IsPart(s[p]))
					{
						plain &= s[p] < 128;
						p++;
					}

					kind = ascii && !plain ? Foreign : Keyword(s.Slice(start, p - start));
					break;
			}

			if (p < 0)
			{
				stopped = start;

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
			starts [count] = start;
			lengths[count] = p - start;
			count++;
		}

		into.Count = count;

		return stopped;
	}

	static char Next(ReadOnlySpan<char> s, int p)
	{
		return p + 1 < s.Length ? s[p + 1] : '\0';
	}

	static byte Take(ref int p, int width, byte kind)
	{
		p += width;

		return kind;
	}

	/// <summary>
	/// One number, whichever of the forms it is, with its base and its suffix in the kind it
	/// comes back as.
	/// </summary>
	/// <remarks>
	/// A separator stands between two digits and nowhere else: `1_` is the number `1` and
	/// then the word `_`, and `0x` with no digit after it is a `0` and then a word. A base
	/// whose digits are missing was no base.
	/// </remarks>
	static int Numeric(ReadOnlySpan<char> s, int p, out byte kind)
	{
		if (s[p] == '0' && (Next(s, p) | 0x20) is 'x' or 'b')
		{
			var hex    = (Next(s, p) | 0x20) == 'x';
			var digits = p + 2;

			while (digits < s.Length && s[digits] == '_')
				digits++;

			var end = hex ? Run(s, digits, IsHex) : Run(s, digits, IsBit);

			if (end > digits)
			{
				kind = hex
					? Suffixed(s, ref end, Hex,  HexU,  HexL,  HexUL)
					: Suffixed(s, ref end, Bits, BitsU, BitsL, BitsUL);

				return end;
			}
		}

		var at   = Run(s, p, IsDigit);
		var real = false;

		// The three ways C# writes a real: a point with digits on both sides of it, a point
		// with nothing before it, and an exponent standing in for the point.
		if (at == p)
		{
			at   = Run(s, p + 1, IsDigit);
			real = true;
		}
		else if (at < s.Length && s[at] == '.')
		{
			var fraction = Run(s, at + 1, IsDigit);

			if (fraction > at + 1)
			{
				at   = fraction;
				real = true;
			}
		}

		if (at < s.Length && (s[at] | 0x20) == 'e')
		{
			var digits = at + 1;

			if (digits < s.Length && (s[digits] == '+' || s[digits] == '-'))
				digits++;

			var exponent = Run(s, digits, IsDigit);

			if (exponent > digits)
			{
				at   = exponent;
				real = true;
			}
		}

		// The three real suffixes take a decimal as readily as a real — `1m` is a decimal
		// and `1f` a float — so they are asked about before the integer suffixes are.
		if (at < s.Length)
			switch (s[at] | 0x20)
			{
				case 'm': kind = RealM; return at + 1;
				case 'd': kind = RealD; return at + 1;
				case 'f': kind = RealF; return at + 1;
			}

		if (real)
		{
			kind = Real;

			return at;
		}

		kind = Suffixed(s, ref at, Number, NumberU, NumberL, NumberUL);

		return at;
	}

	/// <summary>Past a run of digits with separators between them, or where it began if no digit is there.</summary>
	static int Run(ReadOnlySpan<char> s, int p, Func<char, bool> digit)
	{
		if (p >= s.Length || !digit(s[p]))
			return p;

		p++;

		while (true)
		{
			var next = p;

			while (next < s.Length && s[next] == '_')
				next++;

			if (next >= s.Length || !digit(s[next]))
				return p;

			p = next + 1;
		}
	}

	static byte Suffixed(ReadOnlySpan<char> s, ref int p, byte plain, byte unsigned, byte signedLong, byte both)
	{
		if (p >= s.Length)
			return plain;

		var one = s[p] | 0x20;

		if (one != 'u' && one != 'l')
			return plain;

		p++;

		var two = p < s.Length ? s[p] | 0x20 : 0;

		if (one == 'u' && two == 'l' || one == 'l' && two == 'u')
		{
			p++;

			return both;
		}

		return one == 'u' ? unsigned : signedLong;
	}

	/// <summary>A string at a quote: a plain one, or a raw one where three quotes or more begin it.</summary>
	/// <remarks>
	/// A raw string of three to five quotes that finds no end is not refused on the spot: its
	/// first two quotes are an empty string as well, and the longest token that ends is the one
	/// taken. Six quotes or more are only ever a raw string, and one with no end is none.
	/// </remarks>
	static int Quoted(ReadOnlySpan<char> s, int p, out byte kind)
	{
		var quotes = Count(s, p, '"');

		if (quotes >= 6)
		{
			kind = RawLong;

			return LongRawEnd(s, p + quotes, 0, quotes);
		}

		if (quotes >= 3 && RawTextEnd(s, p, quotes) is var raw && raw >= 0)
		{
			kind = RawText;

			return raw;
		}

		kind = Text;

		return TextEnd(s, p);
	}

	/// <summary>A string that begins with dollars: interpolated, raw, or both.</summary>
	static int Dollars(ReadOnlySpan<char> s, int p, out byte kind)
	{
		var dollars = Count(s, p, '$');
		var after   = p + dollars;
		var quotes  = Count(s, after, '"');

		if (dollars == 1 && quotes == 0 && after + 1 < s.Length && s[after] == '@' && s[after + 1] == '"')
		{
			kind = Interpolated;

			return InterpolatedEnd(s, after + 2, verbatim: true, null);
		}

		if (dollars >= 3 && quotes >= 3 || dollars <= 2 && quotes >= 6)
		{
			kind = RawLong;

			return LongRawEnd(s, after + quotes, dollars, quotes);
		}

		if (quotes >= 3)
		{
			kind = RawHoles;

			return RawHolesEnd(s, after + quotes, dollars, quotes, null);
		}

		kind = Interpolated;

		return dollars == 1 && quotes >= 1 ? InterpolatedEnd(s, after + 1, verbatim: false, null) : -1;
	}

	static int Count(ReadOnlySpan<char> s, int p, char what)
	{
		var at = p;

		while (at < s.Length && s[at] == what)
			at++;

		return at - p;
	}

	/// <summary>Past the closing quote of a plain string, or -1 where the text runs out or an escape is none.</summary>
	static int TextEnd(ReadOnlySpan<char> s, int p)
	{
		for (var at = p + 1; at < s.Length;)
		{
			if (s[at] == '"')
				return at + 1;

			if (s[at] != '\\')
			{
				at++;

				continue;
			}

			at = EscapeEnd(s, at);

			if (at < 0)
				return -1;
		}

		return -1;
	}

	/// <summary>Past a character literal: one character or one escape between two quotes.</summary>
	static int CharacterEnd(ReadOnlySpan<char> s, int p)
	{
		var at = p + 1;

		if (at >= s.Length || s[at] == '\'')
			return -1;

		at = s[at] == '\\' ? EscapeEnd(s, at) : at + 1;

		return at >= 0 && at < s.Length && s[at] == '\'' ? at + 1 : -1;
	}

	/// <summary>Past a verbatim string, where a doubled quote is a quote and nothing else is special.</summary>
	/// <remarks>
	/// Two quotes are a quote only where the string goes on to end: where it does not, the first
	/// of them was its end, and the longest string that ends is the one there is.
	/// </remarks>
	static int VerbatimEnd(ReadOnlySpan<char> s, int at)
	{
		var ended = -1;

		while (at < s.Length)
		{
			if (s[at] != '"')
			{
				at++;

				continue;
			}

			if (Next(s, at) != '"')
				return at + 1;

			ended = at + 1;
			at   += 2;
		}

		return ended;
	}

	/// <summary>Past an escape, or -1 where what follows the backslash is none C# has.</summary>
	/// <remarks>
	/// `\u` takes four hexadecimal digits exactly and `\U` eight; `\x` takes one to four, as
	/// many as there are.
	/// </remarks>
	static int EscapeEnd(ReadOnlySpan<char> s, int at)
	{
		if (at + 1 >= s.Length)
			return -1;

		switch (s[at + 1])
		{
			case 'a' or 'b' or 'f' or 'n' or 'r' or 't' or 'v' or '0' or '\\' or '\'' or '"':
				return at + 2;

			case 'u':
				return HexDigits(s, at + 2, 4) == 4 ? at + 6 : -1;

			case 'U':
				return HexDigits(s, at + 2, 8) == 8 ? at + 10 : -1;

			case 'x':
				var width = HexDigits(s, at + 2, 4);

				return width > 0 ? at + 2 + width : -1;

			default:
				return -1;
		}
	}

	static int HexDigits(ReadOnlySpan<char> s, int at, int most)
	{
		var width = 0;

		while (width < most && at + width < s.Length && IsHex(s[at + width]))
			width++;

		return width;
	}

	/// <summary>What an escape stands for, the one ending at <paramref name="end"/>.</summary>
	static string Escaped(ReadOnlySpan<char> s, int at, int end)
	{
		return s[at + 1] switch
		{
			'a' => "\a",
			'b' => "\b",
			'f' => "\f",
			'n' => "\n",
			'r' => "\r",
			't' => "\t",
			'v' => "\v",
			'0' => "\0",
			'u' => ((char)Convert.ToInt32(s.Slice(at + 2, 4).ToString(), 16)).ToString(),
			'U' => char.ConvertFromUtf32(Convert.ToInt32(s.Slice(at + 2, 8).ToString(), 16)),
			'x' => ((char)Convert.ToInt32(s.Slice(at + 2, end - at - 2).ToString(), 16)).ToString(),
			var c => c.ToString(),
		};
	}

	// ── Interpolated and raw strings ────────────────────────────────────────────
	//
	// Each of these is measured twice: once by the lexer, which only wants the end, and once
	// where the string is built, which wants its pieces — text as it reads and a hole where
	// one stands. The same code does both, handed a list or not.

	/// <summary>
	/// Past the closing quote of an interpolated string's body, from just inside its opening
	/// quote, or -1; its pieces added to <paramref name="parts"/> where there is a list.
	/// </summary>
	/// <remarks>
	/// `{{` and `}}` are a brace each, a hole is everything between one brace and its match, and
	/// a brace standing alone is refused. The plain form reads escapes and the verbatim one a
	/// doubled quote.
	/// </remarks>
	static int InterpolatedEnd(ReadOnlySpan<char> s, int at, bool verbatim, List<Segment>? parts)
	{
		// In the verbatim form two quotes are a quote only where the string goes on to end, as
		// in a verbatim string: where it does not, the first of them was its end.
		var ended = -1;

		while (at < s.Length)
		{
			var c = s[at];

			if (c == '{' || c == '}')
			{
				if (Next(s, at) == c)
				{
					parts?.Add(Segment.Of(c == '{' ? "{" : "}"));
					at += 2;

					continue;
				}

				if (c == '}')
					return ended;

				var close = HoleEnd(s, at + 1, parts);

				if (close < 0)
					return ended;

				at = close + 1;

				continue;
			}

			if (c == '"')
			{
				if (!verbatim || Next(s, at) != '"')
					return at + 1;

				ended = at + 1;

				parts?.Add(Segment.Of("\""));
				at += 2;

				continue;
			}

			if (c == '\\' && !verbatim)
			{
				var end = EscapeEnd(s, at);

				if (end < 0)
					return ended;

				parts?.Add(Segment.Of(Escaped(s, at, end)));
				at = end;

				continue;
			}

			var from = at;

			while (at < s.Length && s[at] is not ('"' or '{' or '}') && (verbatim || s[at] != '\\'))
				at++;

			parts?.Add(Segment.Of(s.Slice(from, at - from).ToString()));
		}

		return ended;
	}

	/// <summary>
	/// A hole, from just inside its brace: the index of the brace that closes it, or -1; the
	/// hole added to <paramref name="parts"/> as the window its expression is read over.
	/// </summary>
	/// <remarks>
	/// What ends a hole is what C#'s lexer says ends one, without reading the expression:
	/// brackets balanced, strings stepped over, and a colon standing at the top the start of
	/// its format. The expression is read afterwards, over exactly that window.
	/// </remarks>
	static int HoleEnd(ReadOnlySpan<char> s, int at, List<Segment>? parts)
	{
		var run   = HoleRunEnd(s, at, inside: false);
		var close = run;

		string? format = null;

		if (close < s.Length && s[close] == ':')
		{
			close++;

			while (close < s.Length && s[close] is not ('{' or '}' or '"' or '\\'))
				close++;

			format = s.Slice(run + 1, close - run - 1).ToString();
		}

		if (close >= s.Length || s[close] != '}')
			return -1;

		parts?.Add(Segment.Hole(at, run - at).Formatted(format));

		return close;
	}

	/// <summary>
	/// As far as a hole's expression may reach: past brackets that close and strings that end,
	/// up to a closing bracket of the hole's own, or a colon at its top.
	/// </summary>
	static int HoleRunEnd(ReadOnlySpan<char> s, int at, bool inside)
	{
		while (at < s.Length)
		{
			var c = s[at];

			switch (c)
			{
				case '(' or '[' or '{':
					var close  = c == '(' ? ')' : c == '[' ? ']' : '}';
					var nested = HoleRunEnd(s, at + 1, inside: true);

					if (nested >= s.Length || s[nested] != close)
						return at;

					at = nested + 1;
					continue;

				case ')' or ']' or '}':
					return at;

				case ':':
					if (!inside)
						return at;

					at++;
					continue;

				case '"' or '\'' or '@' or '$':
					var quoted = QuotedEnd(s, at);

					if (quoted >= 0)
					{
						at = quoted;

						continue;
					}

					// A `$` or an `@` is a character like any other where no quote follows it.
					if ((c == '@' || c == '$') && Next(s, at) != '"')
					{
						at++;

						continue;
					}

					return at;

				default:
					at++;
					continue;
			}
		}

		return at;
	}

	/// <summary>Past a string or a character written inside a hole, or -1 where none stands there.</summary>
	/// <remarks>
	/// Every form a string may take, tried in the order the grammar tries them: the raw ones,
	/// longest first, then the interpolated, the verbatim, the plain string and the character.
	/// </remarks>
	static int QuotedEnd(ReadOnlySpan<char> s, int at)
	{
		var dollars = Count(s, at, '$');
		var quotes  = Count(s, at + dollars, '"');

		if (dollars >= 3 && quotes >= 3 || dollars <= 2 && quotes >= 6)
		{
			var end = LongRawEnd(s, at + dollars + quotes, dollars, quotes);

			if (end >= 0)
				return end;
		}

		for (var width = 5; width >= 3; width--)
		{
			if (quotes < width)
				continue;

			var end = dollars is 1 or 2
				? RawHolesEnd(s, at + dollars + width, dollars, width, null)
				: dollars == 0 ? RawTextEnd(s, at, width) : -1;

			if (end >= 0)
				return end;
		}

		if (dollars == 1 && quotes >= 1 && InterpolatedEnd(s, at + 2, verbatim: false, null) is var interpolated && interpolated >= 0)
			return interpolated;

		if (dollars == 1 && quotes == 0 && at + 2 < s.Length && s[at + 1] == '@' && s[at + 2] == '"' ||
			s[at] == '@' && at + 2 < s.Length && s[at + 1] == '$' && s[at + 2] == '"')
		{
			var end = InterpolatedEnd(s, at + 3, verbatim: true, null);

			if (end >= 0)
				return end;
		}

		return s[at] switch
		{
			'@'  => Next(s, at) == '"' ? VerbatimEnd(s, at + 2) : -1,
			'"'  => TextEnd(s, at),
			'\'' => CharacterEnd(s, at),
			_    => -1,
		};
	}

	/// <summary>Past a raw string of <paramref name="quotes"/> quotes with no holes, from its first quote, or -1.</summary>
	/// <remarks>
	/// Its text never begins with a quote — every quote at the beginning belongs to the
	/// beginning — and holds no run of quotes as long as the ones that close it; the first
	/// such run does close it.
	/// </remarks>
	static int RawTextEnd(ReadOnlySpan<char> s, int p, int quotes)
	{
		var at = p + quotes;

		if (at >= s.Length || s[at] == '"')
			return -1;

		at++;

		while (at < s.Length)
		{
			if (s[at] != '"')
			{
				at++;

				continue;
			}

			var run = Count(s, at, '"');

			if (run >= quotes)
				return at + quotes;

			at += run;
		}

		return -1;
	}

	/// <summary>
	/// Past a raw interpolated string of one or two dollars, from the end of its quotes, or -1;
	/// its pieces added to <paramref name="parts"/> where there is a list.
	/// </summary>
	/// <remarks>
	/// With one dollar a brace always opens a hole, and a closing one outside a hole is
	/// refused. With two, a single brace is text, a run of three is one of text and a hole's
	/// two, and a hole is closed by two.
	/// </remarks>
	static int RawHolesEnd(ReadOnlySpan<char> s, int at, int dollars, int quotes, List<Segment>? parts)
	{
		while (at < s.Length)
		{
			var c = s[at];

			if (c == '"')
			{
				if (Count(s, at, '"') >= quotes)
					return at + quotes;

				parts?.Add(Segment.Of("\""));
				at++;

				continue;
			}

			if (c == '{')
			{
				if (dollars == 2 && (Next(s, at) != '{' || at + 2 < s.Length && s[at + 2] == '{' && Next(s, at + 2) != '{'))
				{
					parts?.Add(Segment.Of("{"));
					at++;

					continue;
				}

				var width = dollars;
				var close = HoleEnd(s, at + width, parts);

				if (close < 0 || Count(s, close, '}') < width)
					return -1;

				at = close + width;

				continue;
			}

			if (c == '}')
			{
				if (dollars == 2 && Next(s, at) != '}')
				{
					parts?.Add(Segment.Of("}"));
					at++;

					continue;
				}

				return -1;
			}

			var from = at;

			while (at < s.Length && s[at] is not ('"' or '{' or '}'))
				at++;

			parts?.Add(Segment.Of(s.Slice(from, at - from).ToString()));
		}

		return -1;
	}

	/// <summary>
	/// Past a raw string of more quotes or dollars than the grammar writes out, from the end of
	/// its quotes, or -1 where C# refuses it: measured the way the language's own host measures
	/// one, since that is what decides where one ends.
	/// </summary>
	/// <remarks>
	/// A run of quotes as long as the opening one closes it, and a longer run is refused. With
	/// dollars, a run of braces shorter than them is text; one as long opens a hole, the braces
	/// past that count being text before it; and one twice as long is refused.
	/// </remarks>
	static int LongRawEnd(ReadOnlySpan<char> s, int at, int dollars, int quotes)
	{
		while (at < s.Length)
		{
			var c = s[at];

			if (c == '"')
			{
				var run = Count(s, at, '"');

				if (run >= quotes)
					return run > quotes ? -1 : at + run;

				at += run;

				continue;
			}

			if (dollars > 0 && c == '{')
			{
				var run = Count(s, at, '{');

				if (run < dollars)
				{
					at += run;

					continue;
				}

				if (run >= 2 * dollars)
					return -1;

				at = LongHoleEnd(s, at + run, dollars);

				if (at < 0)
					return -1;

				continue;
			}

			if (dollars > 0 && c == '}')
			{
				var run = Count(s, at, '}');

				if (run >= dollars)
					return -1;

				at += run;

				continue;
			}

			at++;
		}

		return -1;
	}

	/// <summary>Past the closing braces of a hole in a long raw string, or -1 where it has none.</summary>
	static int LongHoleEnd(ReadOnlySpan<char> s, int at, int dollars)
	{
		var depth = 0;
		var colon = false;

		while (at < s.Length)
		{
			var c = s[at];

			if (colon && c != '}')
			{
				at++;

				continue;
			}

			if (!colon)
			{
				if (c is '(' or '[' or '{')
				{
					depth++;
					at++;

					continue;
				}

				if (c is ')' or ']' || c == '}' && depth > 0)
				{
					depth--;
					at++;

					continue;
				}

				if (c == ':' && depth == 0)
				{
					colon = true;
					at++;

					continue;
				}

				if (c is '"' or '\'' or '@' or '$')
				{
					var skipped = LongSkipped(s, at);

					if (skipped < 0)
						return -1;

					at = skipped > at ? skipped : at + 1;

					continue;
				}

				if (c != '}')
				{
					at++;

					continue;
				}
			}

			return Count(s, at, '}') < dollars ? -1 : at + dollars;
		}

		return -1;
	}

	/// <summary>Past a string or a character inside such a hole, the position itself where none begins, or -1.</summary>
	static int LongSkipped(ReadOnlySpan<char> s, int p)
	{
		var at       = p;
		var dollars  = 0;
		var verbatim = false;

		while (at < s.Length && (s[at] == '$' || s[at] == '@'))
		{
			if (s[at] == '$')
				dollars++;
			else
				verbatim = true;

			at++;
		}

		if (at >= s.Length)
			return p;

		if (at == p && s[at] == '\'')
		{
			at++;

			if (at < s.Length && s[at] == '\\')
				at++;

			at++;

			while (at < s.Length && s[at] != '\'')
				at++;

			return at < s.Length ? at + 1 : -1;
		}

		if (s[at] != '"')
			return p;

		var run = Count(s, at, '"');

		if (run >= 3 && !verbatim)
			return LongRawEnd(s, at + run, dollars, run);

		for (at++; at < s.Length; at++)
		{
			var c = s[at];

			if (c == '"')
			{
				if (verbatim && Next(s, at) == '"')
				{
					at++;

					continue;
				}

				return at + 1;
			}

			if (c == '\\' && !verbatim)
			{
				at++;

				continue;
			}

			if (dollars > 0 && (c == '{' || c == '}') && Next(s, at) == c)
			{
				at++;

				continue;
			}

			if (dollars > 0 && c == '{')
			{
				var next = LongHoleEnd(s, at + 1, 1);

				if (next < 0)
					return -1;

				at = next - 1;
			}
		}

		return -1;
	}

	static bool IsDigit(char c)
	{
		return c >= '0' && c <= '9';
	}

	static bool IsBit(char c)
	{
		return c == '0' || c == '1';
	}

	static bool IsHex(char c)
	{
		return c >= '0' && c <= '9' || c >= 'a' && c <= 'f' || c >= 'A' && c <= 'F';
	}

	static bool IsStart(char c)
	{
		return c == '_' || char.IsLetter(c);
	}

	static bool IsPart(char c)
	{
		return c == '_' || char.IsLetterOrDigit(c);
	}

	// ── The parser ──────────────────────────────────────────────────────────────

	/// <summary>
	/// The reader, over kinds, building the same tree the generated parser builds — or, told
	/// not to build, reading and nothing else.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Every method hands back the token it stopped at, or -1, and writes what it read into an
	/// <c>out</c> parameter — the position is the return value because every caller needs it,
	/// the node an argument because only some do. Where the reader only reads, every node is
	/// null.
	/// </para>
	/// <para>
	/// Reading without building is what the body of a lambda that says no types is read with
	/// the first time, when nothing in it can be built yet, and what a text is read with again
	/// where building it threw. What a guard is handed is built all the same, since the guard
	/// cannot answer without it: a declaration's type, the operand a member is looked for on,
	/// what `var` takes its type from.
	/// </para>
	/// <para>
	/// A refusal is said where it happens, by the furthest token the reading looked at: every
	/// look at a token goes through <see cref="Kind"/>, which remembers how far it has been.
	/// That the lookahead which tells a lambda from a name counts too — `(int x) => y` stops at
	/// the end and not at `y` — is how the generated parser counts, not a rule of the language.
	/// </para>
	/// </remarks>
	ref struct Reader(Tokens tokens, string text, State context, bool ascii, bool build)
	{
		readonly byte[] _kinds   = tokens.Kinds;
		readonly int[]  _starts  = tokens.Starts;
		readonly int[]  _lengths = tokens.Lengths;
		readonly int    _count   = tokens.Count;
		readonly string?[] _words  = tokens.Words;
		readonly Tokens   _tokens  = tokens;
		readonly string   _text    = text;
		readonly State    _context = context;
		readonly bool     _ascii   = ascii;

		bool _build = build;
		int  _furthest;

		// The tops of the two stacks a reading gathers on (Tokens.Values and Tokens.Taken).
		int _values;
		int _taken;

		// What the text being read stands under (the grammar's `state`): nothing, nearly
		// always, so the array is made by the first `checked` and not by every reading.
		ExpressionParser.Reading[]? _marks;
		int _marked;

		/// <summary>The furthest token looked at.</summary>
		public readonly int Furthest => _furthest;

		byte Kind(int i)
		{
			if (i > _furthest)
				_furthest = i;

			return i < _count ? _kinds[i] : End;
		}

		/// <summary>A look at a token that is not a reading of it: what `?!` does in the grammar.</summary>
		/// <remarks>
		/// Where the token is the one looked for, the look refuses what asked and says nothing
		/// about where the text went wrong; where it is not, the reading goes on to that token
		/// anyway. So a negative look counts for nothing, and only there is this used rather
		/// than <see cref="Kind"/>.
		/// </remarks>
		readonly byte Peek(int i)
		{
			return i < _count ? _kinds[i] : End;
		}

		/// <summary>A guard refused what was read up to <paramref name="at"/>.</summary>
		int Refuse(int at)
		{
			if (at > _furthest)
				_furthest = at;

			return -1;
		}

		/// <summary>Where the tokens from one to another stand in the input.</summary>
		readonly SourceSpan Span(int from, int to)
		{
			return new(_starts[from], _starts[to - 1] + _lengths[to - 1] - _starts[from]);
		}

		/// <summary>A token's text, cut out of the input the first time it is asked for.</summary>
		/// <remarks>
		/// A name is asked about where it might be a target, where it might be called, and where
		/// it is an operand at last — the same word each time, and one string.
		/// </remarks>
		readonly string Cut(int i)
		{
			return _words[i] ??= _text.Substring(_starts[i], _lengths[i]);
		}

		readonly ReadOnlySpan<ExpressionParser.Reading> Marks =>
			_marks is null ? default : new(_marks, 0, _marked);

		/// <summary>One more of a list being read, on top of the stack the lists are gathered on.</summary>
		void Push(Expression value)
		{
			var values = _tokens.Values;

			if (_values == values.Length)
				Array.Resize(ref _tokens.Values, values.Length * 2);

			_tokens.Values[_values++] = value;

			if (_values > _tokens.Reached)
				_tokens.Reached = _values;
		}

		/// <summary>The list gathered since <paramref name="from"/>, taken off the stack at its own length.</summary>
		Expression[] Popped(int from)
		{
			var count = _values - from;
			var made  = count == 0 ? [] : new Expression[count];

			Array.Copy(_tokens.Values, from, made, 0, count);

			_values = from;

			return made;
		}

		/// <summary>A list that turned out not to be one: what was gathered for it is let go.</summary>
		int Dropped(int from)
		{
			_values = from;

			return -1;
		}

		/// <summary>A word, which is a name or a keyword: what `var` is read as before it is asked whether it is `var`.</summary>
		static bool IsWord(byte kind)
		{
			return kind == Identifier || kind >= FirstWord;
		}

		/// <summary>Whether that token is the word `var`, without cutting a string to find out.</summary>
		/// <remarks>
		/// `var` is contextual, so there is no kind to switch on, and a `Substring` per
		/// statement would be a string allocated to answer no.
		/// </remarks>
		readonly bool IsVar(int i)
		{
			return i < _count && _kinds[i] == Identifier && _lengths[i] == 3 && Same(_text.AsSpan(_starts[i], 3), "var");
		}

		/// <summary>Reads with building switched off, to find out whether a reading goes through before any of it is built.</summary>
		void Quiet(out bool was)
		{
			was    = _build;
			_build = false;
		}

		// ── The lambda, and what it takes ───────────────────────────────────────

		public int Lambda(int i, out LambdaExpression? node)
		{
			node = null;

			var from = i;

			// The `using`s before it, each recorded as it is read — where the grammar's guard
			// records them, and for the reason it does: every name after asks about them.
			while (Kind(i) == KwUsing)
			{
				i = Import(i);

				if (i < 0)
					return -1;
			}

			if (Kind(i) != LeftParen)
				return -1;

			var kept = _taken;
			var at   = Parameters(i + 1);

			if (Kind(at) != RightParen || Kind(at + 1) != Arrow)
				return Untaken(kept);

			// Where this lambda is, recorded before its body and closed after it, which is
			// what says which lambda a `return` written inside it leaves.
			_context.Entering(Span(from, at + 2));

			var body = Value(at + 2, out var read);

			if (body < 0)
				return Untaken(kept);

			var span = Span(from, body);

			if (!_context.Leaves(span))
			{
				Refuse(body);

				return Untaken(kept);
			}

			var parameters = Taken(kept);

			if (_build)
				node = _context.Finished(Expression.Lambda(_context.Returning(read!, span), parameters!));

			return body;
		}

		int Import(int i)
		{
			if (Kind(i + 1) != Identifier)
				return -1;

			var name = Cut(i + 1);
			var at   = i + 2;

			while (Kind(at) == Dot && Kind(at + 1) == Identifier)
			{
				name  = string.Concat(name, ".", Cut(at + 1));
				at   += 2;
			}

			if (Kind(at) != Semicolon)
				return -1;

			return _context.Imports(name, Span(i, at + 1)) ? at + 1 : Refuse(at + 1);
		}

		/// <summary>The parameters inside the brackets, each declared where it is read; where the list ends.</summary>
		/// <remarks>
		/// What is kept of each is where it begins and which token is its name, on a stack of their
		/// own: a lambda's parameters are read before its body and made after it, and the body may
		/// hold lambdas of its own that are read and made in between.
		/// </remarks>
		int Parameters(int i)
		{
			var at = Parameter(i);

			if (at < 0)
				return i;

			while (Kind(at) == Comma)
			{
				var more = Parameter(at + 1);

				if (more < 0)
					break;

				at = more;
			}

			return at;
		}

		/// <summary>A parameter, declared by the guard while the text is read: the declaration is the one thing here that has to happen then.</summary>
		int Parameter(int i)
		{
			var at = Type(i, out _, build: false);

			if (at < 0 || Kind(at) != Identifier)
				return -1;

			Type(i, out var type, build: true);

			if (!_context.Takes(type!, Cut(at), Span(i, at + 1)))
				return Refuse(at + 1);

			var taken = _tokens.Taken;

			if (_taken + 2 > taken.Length)
				Array.Resize(ref _tokens.Taken, taken.Length * 2);

			_tokens.Taken[_taken++] = i;
			_tokens.Taken[_taken++] = at;

			return at + 1;
		}

		/// <summary>The parameters read since <paramref name="from"/>, made once it is known there is a lambda, and taken off their stack.</summary>
		ParameterExpression[]? Taken(int from)
		{
			var made = _build ? new ParameterExpression[(_taken - from) / 2] : null;

			for (var one = 0; made is not null && one < made.Length; one++)
			{
				var start = _tokens.Taken[from + 2 * one];
				var name  = _tokens.Taken[from + 2 * one + 1];

				made[one] = _context.Named(Cut(name), Span(start, name + 1));
			}

			_taken = from;

			return made;
		}

		/// <summary>Parameters that turned out to be no lambda's: taken off their stack.</summary>
		int Untaken(int from)
		{
			_taken = from;

			return -1;
		}

		/// <summary>A lambda written inside an expression, its parameters' types said.</summary>
		/// <remarks>
		/// The scope is recorded after its body is read, which is where the grammar's guard
		/// stands: a name is looked up by where it is written, and this is what says where the
		/// parameter's inside is.
		/// </remarks>
		int Inner(int i, out Expression? node)
		{
			node = null;

			var kept = _taken;
			var at   = Parameters(i + 1);

			if (Kind(at) != RightParen || Kind(at + 1) != Arrow)
				return Untaken(kept);

			_context.Entering(Span(i, at + 2));

			var body = Value(at + 2, out var read);

			if (body < 0)
				return Untaken(kept);

			var span = Span(i, body);

			if (!_context.Scoped(span) || !_context.Leaves(span))
			{
				Refuse(body);

				return Untaken(kept);
			}

			var parameters = Taken(kept);

			if (_build)
				node = _context.Nested(read!, parameters!, span);

			return body;
		}

		/// <summary>A lambda whose parameters say no types: `n => n * 2`, `(a, b) => a + b`.</summary>
		/// <remarks>
		/// <para>
		/// Asked first whether a `=>` follows at all, because this is tried at every name and
		/// every bracket an operand begins with, and nearly none of them is a lambda.
		/// </para>
		/// <para>
		/// The body is read to find where it ends and nothing of it is built: building it
		/// needs the types, and they are the delegate's the call it is handed to chooses. What
		/// is made is the lambda still to be built, which the call builds by reading the body
		/// again over exactly its own text, the parameters typed.
		/// </para>
		/// </remarks>
		int Untyped(int i, out Expression? node)
		{
			node = null;

			int arrow;

			var bare = Kind(i) == Identifier;

			if (bare)
			{
				arrow = i + 1;
			}
			else
			{
				if (Kind(i + 1) != Identifier)
					return -1;

				arrow = i + 2;

				while (Kind(arrow) == Comma)
				{
					if (Kind(arrow + 1) != Identifier)
						return -1;

					arrow += 2;
				}

				if (Kind(arrow) != RightParen)
					return -1;

				arrow++;
			}

			if (Kind(arrow) != Arrow)
				return -1;

			var parameters = new ExpressionParser.Awaited[bare ? 1 : (arrow - i - 1) / 2];

			for (var one = 0; one < parameters.Length; one++)
			{
				var word = bare ? i : i + 1 + 2 * one;

				parameters[one] = new ExpressionParser.Awaited(Cut(word), _starts[word]);
			}

			var head = Span(i, arrow + 1);

			if (!_context.Awaits(parameters, head) || !_context.Entering(head))
				return Refuse(arrow + 1);

			Quiet(out var was);

			var body = Value(arrow + 1, out _);

			_build = was;

			if (body < 0)
				return -1;

			var span = Span(i, body);

			if (!(_context.Scoped(span) && _context.Leaves(span) && _context.Settles(span)))
				return Refuse(body);

			if (_build)
			{
				var held = Span(arrow + 1, body);

				node = _context.Deferred(
					parameters, new ExpressionParser.Held(held.Start, held.Length), span, _ascii ? AsciiBody : Body);
			}

			return body;
		}

		// ── Types ───────────────────────────────────────────────────────────────
		//
		// A type is built where it is asked for and not where it is read: most places read one
		// before they know whether what they are reading is theirs — a cast before its operand,
		// a declaration before its name — and building one that turns out to be a name's is
		// building on a path that is left.

		int Type(int i, out Type? type, bool build)
		{
			var at = Core(i, out type, build);

			if (at < 0)
				return -1;

			while (Kind(at) == Brackets)
			{
				type = type?.MakeArrayType();
				at++;
			}

			return at;
		}

		int Core(int i, out Type? type, bool build)
		{
			var kind = Kind(i);

			type = null;

			if (kind == Identifier)
				return Named(i, out type, build);

			if (!IsCore(kind))
				return -1;

			if (build)
				type = kind switch
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
					_                         => typeof(object),
				};

			return i + 1;
		}

		/// <summary>Whether a token is a keyword that names a type.</summary>
		static bool IsCore(byte kind)
		{
			return kind == KwSbyte   || kind == KwByte || kind == KwShort || kind == KwUshort || kind == KwInt    ||
				kind == KwUint    || kind == KwLong || kind == KwUlong || kind == KwFloat  || kind == KwDouble ||
				kind == KwDecimal || kind == KwBool || kind == KwChar  || kind == KwString || kind == KwObject;
		}

		/// <summary>
		/// A dotted name, and the type arguments where there are any. What decides that it is a
		/// type at all is whether the name resolves — asked here, while the text is read, so
		/// that the answer can decide how the text reads.
		/// </summary>
		int Named(int i, out Type? type, bool build)
		{
			type = null;

			// Every word of the dotted name, so that the longest one can be asked about first
			// and the tail given back where it names nothing: `Math.PI` is `Math` and a member
			// of it, and only asking says so.
			var last = i;

			while (Kind(last + 1) == Dot && Kind(last + 2) == Identifier)
				last += 2;

			for (; last >= i; last -= 2)
			{
				var after = last + 1;
				var name  = Dotted(i, last);

				// The generic form needs no name that resolves on its own: `List<int>` does and
				// `List` does not, so where there are arguments they are what says the name is
				// a type.
				if (Kind(after) == Less)
				{
					var closed = TypeArguments(after, out var arguments, build);

					if (closed >= 0)
					{
						if (build)
							type = _context.Generic(name, arguments!);

						return closed;
					}
				}

				if (!_context.Resolves(name))
				{
					Refuse(after);

					continue;
				}

				if (build)
					type = _context.TypeNamed(name);

				return after;
			}

			return -1;
		}

		readonly string Dotted(int first, int last)
		{
			var name = Cut(first);

			for (var part = first + 2; part <= last; part += 2)
				name = string.Concat(name, ".", Cut(part));

			return name;
		}

		int TypeArguments(int from, out Type[]? arguments, bool build)
		{
			arguments = null;

			var read = build ? new List<Type>() : null;
			var at   = Type(from + 1, out var first, build);

			if (at < 0)
				return -1;

			read?.Add(first!);

			while (Kind(at) == Comma)
			{
				var next = Type(at + 1, out var more, build);

				if (next < 0)
					break;

				read?.Add(more!);
				at = next;
			}

			if (Kind(at) != Greater)
				return -1;

			arguments = read?.ToArray();

			return at + 1;
		}

		// ── Statements ──────────────────────────────────────────────────────────

		/// <summary>Where a value is wanted and a block or an `if` may stand.</summary>
		public int Value(int i, out Expression? node)
		{
			var kind = Kind(i);

			if (kind == LeftBrace)
				return Block(i, out node);

			// An `if` worth what its branches are, read without building first: where it has no
			// `else` it is the statement form below instead, and that one's branches are read
			// otherwise.
			if (kind == KwIf)
			{
				Quiet(out var was);

				var chosen = IfValue(i, out node);

				_build = was;

				if (chosen >= 0)
					return _build ? IfValue(i, out node) : chosen;
			}

			if (IsControl(kind))
			{
				var control = Control(i, out node);

				if (control >= 0)
					return control;
			}

			return Assignment(i, out node);
		}

		static bool IsControl(byte kind)
		{
			return kind == KwTry || kind == KwIf  || kind == KwWhile  ||
				kind == KwDo  || kind == KwFor || kind == KwSwitch ||
				kind == KwForeach;
		}

		int Statement(int i, out Expression? node)
		{
			var formed = Form(i, out node);

			if (formed >= 0)
				return formed;

			var expression = Assignment(i, out node);

			if (expression >= 0 && Kind(expression) == Semicolon)
				return expression + 1;

			node = null;

			return -1;
		}

		/// <summary>Every statement but an expression with a semicolon after it, which is what a branch reads apart.</summary>
		int Form(int i, out Expression? node)
		{
			var local = Local(i, out node);

			if (local >= 0)
				return local;

			var unsettled = InferredUnsettled(i, out node);

			if (unsettled >= 0)
				return unsettled;

			var inferred = Inferred(i, out node);

			if (inferred >= 0)
				return inferred;

			var kind = Kind(i);

			if (kind == KwReturn)
				return Return(i, out node);

			if (kind == LeftBrace)
				return Block(i, out node);

			if (IsControl(kind))
			{
				var control = Control(i, out node);

				if (control >= 0)
					return control;
			}

			return Jump(i, out node);
		}

		int Local(int i, out Expression? node)
		{
			node = null;

			var at = Type(i, out _, build: false);

			if (at < 0 || Kind(at) != Identifier)
				return -1;

			Type(i, out var type, build: true);

			// Declared before its initializer is read, as the API wants — `Expression.Variable`
			// is handed a type at the declaration — so `int x = x;` reads.
			var name = Cut(at);

			if (!_context.Declare(type!, name, Span(i, at + 1)))
				return Refuse(at + 1);

			if (Kind(at + 1) != Assign)
				return -1;

			var value = Value(at + 2, out var read);

			if (value < 0 || Kind(value) != Semicolon)
				return -1;

			if (_build)
				node = ExpressionParser.Assigned(_context.Named(name, Span(i, value + 1)), read!);

			return value + 1;
		}

		/// <summary>`var` in the body of a lambda that says no types, read before they are known.</summary>
		/// <remarks>
		/// The initializer is worth nothing yet — it is built over parameters with no type — so
		/// the name is declared an <c>object</c>, and nothing made here is kept: the body is
		/// read again once the types are known, and there this refuses and the next one reads.
		/// </remarks>
		int InferredUnsettled(int i, out Expression? node)
		{
			node = null;

			if (!IsWord(Kind(i)))
				return -1;

			if (!IsVar(i) || !_context.Unsettled(Span(i, i + 1)))
				return Refuse(i + 1);

			if (Kind(i + 1) != Identifier || Kind(i + 2) != Assign)
				return -1;

			var name = Cut(i + 1);

			Quiet(out var was);

			var value = Value(i + 3, out _);

			_build = was;

			if (value < 0 || Kind(value) != Semicolon)
				return -1;

			if (!_context.Declare(typeof(object), name, Span(i, value + 1)))
				return Refuse(value + 1);

			if (_build)
				node = Expression.Empty();

			return value + 1;
		}

		/// <summary>A declaration whose type is its initializer's.</summary>
		/// <remarks>
		/// Read where <see cref="Local"/> could not read a type, which is what leaves a real
		/// type named `var` winning and `var` itself usable as a name. The declaration is made
		/// after the initializer, because until that is a tree there is no type to make it
		/// with — so `var x = x;` does not read.
		/// </remarks>
		int Inferred(int i, out Expression? node)
		{
			node = null;

			if (!IsWord(Kind(i)))
				return -1;

			if (!IsVar(i))
				return Refuse(i + 1);

			if (Kind(i + 1) != Identifier || Kind(i + 2) != Assign)
				return -1;

			var name  = Cut(i + 1);
			var value = Value(i + 3, out var read);

			if (value < 0 || Kind(value) != Semicolon)
				return -1;

			// The guard is handed the initializer built, which reading alone did not do.
			if (read is null)
				read = Built(i + 3);

			var span = Span(i, value + 1);

			if (!ExpressionParser.Inferable(read!) || !_context.Declare(read!.Type, name, span))
				return Refuse(value + 1);

			if (_build)
				node = ExpressionParser.Assigned(_context.Named(name, span), read);

			return value + 1;
		}

		/// <summary>A value read again and built, for a guard that has to be handed it where the reading built nothing.</summary>
		Expression? Built(int i)
		{
			var was = _build;

			_build = true;

			Value(i, out var read);

			_build = was;

			return read;
		}

		int Return(int i, out Expression? node)
		{
			node = null;

			var value = Value(i + 1, out var read);

			if (value < 0 || Kind(value) != Semicolon)
				return -1;

			if (_build)
				node = _context.Return(read!, Span(i, value + 1));

			return value + 1;
		}

		int Block(int i, out Expression? node)
		{
			node = null;

			var at   = i + 1;
			var from = _values;

			while (true)
			{
				var one = Statement(at, out var read);

				if (one < 0)
					break;

				if (_build)
					Push(read!);

				at = one;
			}

			var value = Assignment(at, out var last);

			if (value >= 0)
				at = value;
			else
				last = null;

			if (Kind(at) != RightBrace)
				return Dropped(from);

			var span = Span(i, at + 1);

			if (!_context.Scoped(span))
			{
				Refuse(at + 1);

				return Dropped(from);
			}

			if (_build)
				node = _context.Block(Popped(from), span, last);

			return at + 1;
		}

		int Control(int i, out Expression? node)
		{
			var kind = Kind(i);

			if (kind == KwTry)    return Try(i, out node);
			if (kind == KwIf)     return If(i, out node);
			if (kind == KwWhile)  return While(i, out node);
			if (kind == KwDo)     return DoWhile(i, out node);
			if (kind == KwFor)    return For(i, out node);
			if (kind == KwSwitch) return Switch(i, out node);

			// A `foreach` that writes its element type, one that says `var` in a body read
			// before its lambda has types, and one that says `var` where the source is worth
			// something: the order the grammar tries them in.
			var typed = Foreach(i, out node);

			if (typed >= 0)
				return typed;

			var unsettled = ForeachUnsettled(i, out node);

			if (unsettled >= 0)
				return unsettled;

			return ForeachInferred(i, out node);
		}

		/// <summary>An `if` as a statement: with an `else`, worth what its branches are; without one, a statement of its own.</summary>
		int If(int i, out Expression? node)
		{
			node = null;

			if (Kind(i + 1) != LeftParen)
				return -1;

			var test = Assignment(i + 2, out var read);

			if (test < 0 || Kind(test) != RightParen)
				return -1;

			var then = Branch(test + 1, out var whenTrue, out var statement);

			if (then < 0)
				return -1;

			if (Kind(then) == KwElse)
			{
				var otherwise = Branch(then + 1, out var whenFalse, out _);

				if (otherwise >= 0)
				{
					if (_build)
						node = ExpressionParser.Branched(read!, whenTrue!, whenFalse!);

					return otherwise;
				}
			}

			// No `else`, or none that reads: the `if` stands alone, which it can only where
			// what it holds is a statement.
			if (!statement)
				return -1;

			if (_build)
				node = Expression.IfThen(read!, whenTrue!);

			return then;
		}

		/// <summary>The same `if` where a value is wanted, so that its branches are values.</summary>
		int IfValue(int i, out Expression? node)
		{
			node = null;

			if (Kind(i + 1) != LeftParen)
				return -1;

			var test = Assignment(i + 2, out var read);

			if (test < 0 || Kind(test) != RightParen)
				return -1;

			var then = Value(test + 1, out var whenTrue);

			if (then < 0 || Kind(then) != KwElse)
				return -1;

			var otherwise = Value(then + 1, out var whenFalse);

			if (otherwise < 0)
				return -1;

			if (_build)
				node = ExpressionParser.Branched(read!, whenTrue!, whenFalse!);

			return otherwise;
		}

		/// <summary>A branch: a statement where one was written, an expression where one was.</summary>
		/// <remarks>
		/// An expression with a semicolon after it is the statement, and one without is the
		/// expression — read once, and the semicolon decides.
		/// </remarks>
		int Branch(int i, out Expression? node, out bool statement)
		{
			statement = true;

			var formed = Form(i, out node);

			if (formed >= 0)
				return formed;

			var expression = Assignment(i, out node);

			if (expression < 0)
			{
				statement = false;

				return -1;
			}

			if (Kind(expression) == Semicolon)
				return expression + 1;

			statement = false;

			return expression;
		}

		int While(int i, out Expression? node)
		{
			node = null;

			if (Kind(i + 1) != LeftParen)
				return -1;

			var test = Assignment(i + 2, out var read);

			if (test < 0 || Kind(test) != RightParen)
				return -1;

			_context.Opening(Span(i, test + 1));

			var body = Statement(test + 1, out var inside);

			if (body < 0)
				return -1;

			var span = Span(i, body);

			if (!_context.Loops(span))
				return Refuse(body);

			if (_build)
				node = Expression.Loop(
					Expression.Condition(read!, inside!, Expression.Break(_context.Exit(span)), typeof(void)),
					_context.Exit(span),
					_context.Again(span));

			return body;
		}

		int DoWhile(int i, out Expression? node)
		{
			node = null;

			_context.Opening(Span(i, i + 1));

			var body = Statement(i + 1, out var inside);

			if (body < 0 || Kind(body) != KwWhile || Kind(body + 1) != LeftParen)
				return -1;

			var test = Assignment(body + 2, out var read);

			if (test < 0 || Kind(test) != RightParen || Kind(test + 1) != Semicolon)
				return -1;

			var span = Span(i, test + 2);

			if (!_context.Loops(span))
				return Refuse(test + 2);

			// `Expression.Loop`'s own continue label stands at the top of the body, which is
			// where C# puts it for a `while` and not for a `do`: there it goes to the test.
			if (_build)
				node = Expression.Loop(
					Expression.Block(
						inside!,
						Expression.Label(_context.Again(span)),
						Expression.Condition(read!, Expression.Empty(), Expression.Break(_context.Exit(span)), typeof(void))),
					_context.Exit(span));

			return test + 2;
		}

		int For(int i, out Expression? node)
		{
			node = null;

			if (Kind(i + 1) != LeftParen)
				return -1;

			var init = Statement(i + 2, out var start);

			if (init < 0)
				return -1;

			var test = Assignment(init, out var read);

			if (test < 0 || Kind(test) != Semicolon)
				return -1;

			var step = Assignment(test + 1, out var next);

			if (step < 0 || Kind(step) != RightParen)
				return -1;

			_context.Opening(Span(i, step + 1));

			var body = Statement(step + 1, out var inside);

			if (body < 0)
				return -1;

			var span = Span(i, body);

			if (!_context.Loops(span) || !_context.Scoped(span))
				return Refuse(body);

			if (_build)
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

		/// <summary>A `foreach` that writes the element type, which is known where it is written.</summary>
		int Foreach(int i, out Expression? node)
		{
			node = null;

			if (Kind(i + 1) != LeftParen)
				return -1;

			var at = Type(i + 2, out _, build: false);

			if (at < 0 || Kind(at) != Identifier)
				return -1;

			Type(i + 2, out var type, build: true);

			var name = Cut(at);

			if (!_context.Declare(type!, name, Span(i, at + 1)))
				return Refuse(at + 1);

			if (Kind(at + 1) != KwIn)
				return -1;

			var over = Assignment(at + 2, out var source);

			if (over < 0 || Kind(over) != RightParen)
				return -1;

			return Iteration(i, over, name, source, out node);
		}

		/// <summary>`foreach (var …)` in a body read before its lambda has types, where the source is worth nothing yet.</summary>
		int ForeachUnsettled(int i, out Expression? node)
		{
			node = null;

			if (Kind(i + 1) != LeftParen || !IsWord(Kind(i + 2)))
				return -1;

			if (!IsVar(i + 2) || !_context.Unsettled(Span(i, i + 3)))
				return Refuse(i + 3);

			if (Kind(i + 3) != Identifier || Kind(i + 4) != KwIn)
				return -1;

			var name = Cut(i + 3);

			Quiet(out var was);

			var over = Assignment(i + 5, out _);

			_build = was;

			if (over < 0 || Kind(over) != RightParen)
				return -1;

			var head = Span(i, over + 1);

			if (!_context.Declare(typeof(object), name, head) || !_context.Opening(head))
				return Refuse(over + 1);

			Quiet(out was);

			var body = Statement(over + 1, out _);

			_build = was;

			if (body < 0)
				return -1;

			var span = Span(i, body);

			if (!_context.Loops(span) || !_context.Scoped(span))
				return Refuse(body);

			if (_build)
				node = Expression.Empty();

			return body;
		}

		/// <summary>`foreach (var …)`, declared once the source is a tree with an element type to ask for.</summary>
		int ForeachInferred(int i, out Expression? node)
		{
			node = null;

			if (Kind(i + 1) != LeftParen || !IsWord(Kind(i + 2)))
				return -1;

			if (!IsVar(i + 2))
				return Refuse(i + 3);

			if (Kind(i + 3) != Identifier || Kind(i + 4) != KwIn)
				return -1;

			var name = Cut(i + 3);
			var over = Assignment(i + 5, out var source);

			if (over < 0 || Kind(over) != RightParen)
				return -1;

			if (source is null)
			{
				var was = _build;

				_build = true;

				Assignment(i + 5, out source);

				_build = was;
			}

			if (!(ExpressionParser.Yielded(source!) is { } item && _context.Declare(item, name, Span(i, over + 1))))
				return Refuse(over + 1);

			return Iteration(i, over, name, source, out node);
		}

		/// <summary>What the two `foreach`s that build share: the loop and the scope, and what C# lowers one to.</summary>
		int Iteration(int i, int over, string name, Expression? source, out Expression? node)
		{
			node = null;

			_context.Opening(Span(i, over + 1));

			var body = Statement(over + 1, out var inside);

			if (body < 0)
				return -1;

			var span = Span(i, body);

			if (!_context.Loops(span) || !_context.Scoped(span))
				return Refuse(body);

			if (_build)
				node = _context.Block(
					[], span,
					ExpressionParser.Iterated(
						_context.Named(name, span), source!, inside!, _context.Exit(span), _context.Again(span)));

			return body;
		}

		int Switch(int i, out Expression? node)
		{
			node = null;

			if (Kind(i + 1) != LeftParen)
				return -1;

			var value = Assignment(i + 2, out var read);

			if (value < 0 || Kind(value) != RightParen || Kind(value + 1) != LeftBrace)
				return -1;

			_context.Breaking(Span(i, value + 2));

			var at    = value + 2;
			var cases = _build ? new List<SwitchCase>() : null;

			while (Kind(at) == KwCase)
			{
				var test = Assignment(at + 1, out var label);

				if (test < 0 || Kind(test) != Colon)
					return -1;

				var body = Statements(test + 1, out var statements);

				if (body < 0)
					return -1;

				cases?.Add(Expression.SwitchCase(Expression.Block(statements!), label!));
				at = body;
			}

			var fallback = default(Expression);

			if (Kind(at) == KwDefault)
			{
				if (Kind(at + 1) != Colon)
					return -1;

				var body = Statements(at + 2, out var statements);

				if (body < 0)
					return -1;

				if (_build)
					fallback = Expression.Block(statements!);

				at = body;
			}

			if (Kind(at) != RightBrace)
				return -1;

			var span = Span(i, at + 1);

			if (!_context.Breaks(span))
				return Refuse(at + 1);

			if (_build)
				node = Expression.Block(
					Expression.Switch(typeof(void), read!, fallback, null, ExpressionParser.Against(cases!.ToArray(), read!.Type)),
					Expression.Label(_context.Exit(span)));

			return at + 1;
		}

		/// <summary>One statement at least, and as many after it as there are.</summary>
		/// <remarks>
		/// A statement that is not there where the next one would begin is where the list ends,
		/// not where the text went wrong — and so is the first one missing, which is how the
		/// generated parser counts a repetition: a turn that fails where it began is no refusal.
		/// That is its accounting and not the language's; it shows only here, where nothing after
		/// the list looks at the same token again.
		/// </remarks>
		int Statements(int i, out Expression[]? statements)
		{
			statements = null;

			var at   = i;
			var from = _values;
			var some = false;

			while (true)
			{
				var before = _furthest;
				var one    = Statement(at, out var statement);

				if (one < 0)
				{
					if (_furthest == at && before < at)
						_furthest = before;

					break;
				}

				if (_build)
					Push(statement!);

				at   = one;
				some = true;
			}

			if (!some)
				return -1;

			if (_build)
				statements = Popped(from);

			return at;
		}

		int Try(int i, out Expression? node)
		{
			node = null;

			if (Kind(i + 1) != LeftBrace)
				return -1;

			var body = Block(i + 1, out var inside);

			if (body < 0)
				return -1;

			var at       = body;
			var handlers = _build ? new List<CatchBlock>() : null;
			var caught   = false;

			while (Kind(at) == KwCatch)
			{
				var one = Catch(at, out var handler);

				if (one < 0)
					return -1;

				handlers?.Add(handler!);
				at     = one;
				caught = true;
			}

			if (Kind(at) == KwFinally)
			{
				if (Kind(at + 1) != LeftBrace)
					return -1;

				var final = Block(at + 1, out var last);

				if (final < 0)
					return -1;

				if (_build)
					node = caught
						? Expression.TryCatchFinally(inside!, last!, handlers!.ToArray())
						: Expression.TryFinally(inside!, last!);

				return final;
			}

			if (!caught)
				return -1;

			if (_build)
				node = Expression.TryCatch(inside!, handlers!.ToArray());

			return at;
		}

		int Catch(int i, out CatchBlock? node)
		{
			node = null;

			if (Kind(i + 1) != LeftParen)
				return -1;

			var at = Type(i + 2, out _, build: false);

			if (at < 0 || Kind(at) != Identifier)
				return -1;

			Type(i + 2, out var type, build: true);

			var name = Cut(at);

			if (!_context.Declare(type!, name, Span(i, at + 1)))
				return Refuse(at + 1);

			if (Kind(at + 1) != RightParen || Kind(at + 2) != LeftBrace)
				return -1;

			var body = Block(at + 2, out var inside);

			if (body < 0)
				return -1;

			var span = Span(i, body);

			if (!_context.Scoped(span))
				return Refuse(body);

			if (_build)
				node = Expression.Catch(_context.Named(name, span), inside!);

			return body;
		}

		/// <summary>A `break`, a `continue` or a `throw`, with the semicolon that ends it.</summary>
		int Jump(int i, out Expression? node)
		{
			node = null;

			var kind = Kind(i);

			if (kind == KwBreak || kind == KwContinue)
			{
				if (Kind(i + 1) != Semicolon)
					return -1;

				if (_build)
					node = kind == KwBreak
						? Expression.Break(_context.Exit(Span(i, i + 1)))
						: Expression.Continue(_context.Again(Span(i, i + 1)));

				return i + 2;
			}

			if (kind != KwThrow)
				return -1;

			var value = Assignment(i + 1, out var thrown);

			if (value >= 0)
			{
				if (Kind(value) != Semicolon)
					return -1;

				if (_build)
					node = Expression.Throw(thrown!);

				return value + 1;
			}

			if (Kind(i + 1) != Semicolon)
				return -1;

			if (_build)
				node = Expression.Rethrow();

			return i + 2;
		}

		// ── The operators ───────────────────────────────────────────────────────

		public int Assignment(int i, out Expression? node)
		{
			if (Kind(i) == Identifier)
			{
				var element = Written(i, out node);

				if (element >= 0)
					return element;
			}

			var target = Target(i, out var name, out var member);

			if (target >= 0)
			{
				var operation = Kind(target);

				if (IsAssignment(operation) && (operation != Assign || Peek(target + 1) != Assign))
				{
					var value = Assignment(target + 1, out var read);

					if (value < 0)
					{
						node = null;

						return -1;
					}

					node = _build
						? Assigned(operation, member is null ? name! : ExpressionParser.Member(name!, member, _context.Caller), read!)
						: null;

					return value;
				}
			}

			return Conditional(i, out node);
		}

		/// <summary>An element written to: `a[i] = v`, and only the plain `=`.</summary>
		/// <remarks>
		/// The indices are read before it is known whether they are a target or the operand of
		/// something else, so they are read first without being built: an index read as the
		/// wrong one of the two would have been built on a path the reading leaves.
		/// </remarks>
		int Written(int i, out Expression? node)
		{
			node = null;

			Quiet(out var was);

			var name = Name(i, out _);
			var at   = name >= 0 ? Indices(name, out _) : -1;

			_build = was;

			if (at < 0 || Kind(at) != Assign || Peek(at + 1) == Assign)
				return -1;

			Expression?   written = null;
			Expression[]? indices = null;

			if (_build)
			{
				Name(i, out written);
				Indices(name, out indices);
			}

			var value = Assignment(at + 1, out var read);

			if (value < 0)
				return -1;

			if (_build)
				node = ExpressionParser.Assigned(ExpressionParser.Place(written!, indices!, _context.Caller), read!);

			return value;
		}

		static bool IsAssignment(byte kind)
		{
			return kind is Assign or PlusAssign or MinusAssign or StarAssign or SlashAssign or
					PercentAssign or AmpAssign or PipeAssign or CaretAssign or LeftAssign or RightAssign;
		}

		readonly Expression Assigned(byte operation, Expression target, Expression value)
		{
			return operation switch
			{
				PlusAssign    => ExpressionParser.AddAssign(target, value, Marks),
				MinusAssign   => ExpressionParser.SubtractAssign(target, value, Marks),
				StarAssign    => ExpressionParser.MultiplyAssign(target, value, Marks),
				SlashAssign   => ExpressionParser.ArithmeticAssign(Expression.DivideAssign, Expression.Divide, target, value, Marks),
				PercentAssign => ExpressionParser.ArithmeticAssign(Expression.ModuloAssign, Expression.Modulo, target, value, Marks),
				AmpAssign     => ExpressionParser.IntegralAssign(Expression.AndAssign, Expression.And, target, value, Marks),
				PipeAssign    => ExpressionParser.IntegralAssign(Expression.OrAssign, Expression.Or, target, value, Marks),
				CaretAssign   => ExpressionParser.IntegralAssign(Expression.ExclusiveOrAssign, Expression.ExclusiveOr, target, value, Marks),
				LeftAssign    => ExpressionParser.ShiftAssign(Expression.LeftShiftAssign, Expression.LeftShift, target, value, Marks),
				RightAssign   => ExpressionParser.ShiftAssign(Expression.RightShiftAssign, Expression.RightShift, target, value, Marks),
				_             => ExpressionParser.Assigned(target, value),
			};
		}

		/// <summary>What may be written to: a name, or a member of one.</summary>
		/// <remarks>
		/// The guard asks whether the member is one a value can be written to, so it is handed
		/// the name built even where nothing else is: `s.Trim()` is read here on the way to
		/// finding out it is a call, and is refused here for it.
		/// </remarks>
		int Target(int i, out Expression? name, out string? member)
		{
			member = null;

			var at = Name(i, out name, always: true);

			if (at < 0)
				return -1;

			if (Kind(at) == Dot && Kind(at + 1) == Identifier)
			{
				member = Cut(at + 1);
				at    += 2;
			}

			return ExpressionParser.Has(name!, member, _context.Caller) ? at : Refuse(at);
		}

		int Conditional(int i, out Expression? node)
		{
			var at = Binary(i, 1, out node);

			if (at < 0 || Kind(at) != Question)
				return at;

			var then = Conditional(at + 1, out var whenTrue);

			if (then < 0 || Kind(then) != Colon)
				return at;

			var otherwise = Conditional(then + 1, out var whenFalse);

			if (otherwise < 0)
				return at;

			if (_build)
				node = ExpressionParser.Chosen(node!, whenTrue, whenFalse);

			return otherwise;
		}

		/// <summary>
		/// C#'s ladder, from `??` at the loosest to `*` at the tightest, as a loop over a
		/// precedence rather than ten rules that differ only in a number.
		/// </summary>
		int Binary(int i, int least, out Expression? node)
		{
			// `??` groups to the right and sits between `?:` and `||`, so it is written here
			// rather than in the table: a level that folds leftwards cannot say it.
			if (least <= 1)
			{
				var at = Binary(i, 2, out node);

				if (at < 0 || Kind(at) != Coalesce)
					return at;

				var right = Binary(at + 1, 1, out var other);

				if (right < 0)
					return at;

				if (_build)
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

				var operation = Kind(read);

				// `is` and `as` are the two whose right side is a type and not an operand, and
				// they sit at the relational level because C# puts them there.
				if (operation == KwIs || operation == KwAs)
				{
					var named = Type(read + 1, out var type, _build);

					if (named < 0)
						return read;

					if (_build)
						node = operation == KwIs ? Expression.TypeIs(node!, type!) : Expression.TypeAs(node!, type!);

					read = named;

					continue;
				}

				var right = Binary(read + width, level + 1, out var other);

				if (right < 0)
					return read;

				if (_build)
					node = width == 2 ? Shifted(operation, node!, other!) : Applied(operation, node!, other!);

				read = right;
			}
		}

		/// <summary>Which level the operator at this token belongs to, and how many tokens it is.</summary>
		/// <remarks>
		/// Two of them are two tokens: a shift is `&lt;` or `&gt;` written twice with nothing
		/// between them, which is what lets `List&lt;List&lt;int&gt;&gt;` close two argument
		/// lists. The comparison one level out is the same character once, and what tells them
		/// apart is whether the second stands right against the first.
		/// </remarks>
		int Level(int i, out int width)
		{
			width = 1;

			var kind = Kind(i);

			if (kind == OrElse)  return 2;
			if (kind == AndAlso) return 3;
			if (kind == Pipe)    return Peek(i + 1) == Pipe ? 0 : 4;
			if (kind == Caret)   return 5;
			if (kind == Amp)     return Peek(i + 1) == Amp ? 0 : 6;

			if (kind == Equal || kind == NotEqual)
				return 7;

			if (kind == Less || kind == Greater)
			{
				if (Kind(i + 1) != kind)
					return 8;

				if (_starts[i] + _lengths[i] != _starts[i + 1])
					return 0;

				width = 2;

				return 9;
			}

			if (kind == LessEq || kind == GreaterEq) return 8;
			if (kind == KwIs   || kind == KwAs)      return 8;
			if (kind == Plus   || kind == Minus)     return 10;

			if (kind == Star || kind == Slash || kind == Percent)
				return 11;

			return 0;
		}

		readonly Expression Applied(byte operation, Expression left, Expression right)
		{
			return operation switch
			{
				OrElse    => Expression.OrElse(left, right),
				AndAlso   => Expression.AndAlso(left, right),
				Pipe      => ExpressionParser.Integral(Expression.Or, left, right),
				Caret     => ExpressionParser.Integral(Expression.ExclusiveOr, left, right),
				Amp       => ExpressionParser.Integral(Expression.And, left, right),
				Equal     => ExpressionParser.Equality(Expression.Equal, left, right),
				NotEqual  => ExpressionParser.Equality(Expression.NotEqual, left, right),
				LessEq    => ExpressionParser.Relational(Expression.LessThanOrEqual, left, right),
				GreaterEq => ExpressionParser.Relational(Expression.GreaterThanOrEqual, left, right),
				Less      => ExpressionParser.Relational(Expression.LessThan, left, right),
				Greater   => ExpressionParser.Relational(Expression.GreaterThan, left, right),
				Plus      => ExpressionParser.Add(left, right, Marks),
				Minus     => ExpressionParser.Subtract(left, right, Marks),
				Star      => ExpressionParser.Multiply(left, right, Marks),
				Slash     => ExpressionParser.Arithmetic(Expression.Divide, left, right),
				_         => ExpressionParser.Arithmetic(Expression.Modulo, left, right),
			};
		}

		/// <summary>The shift, which the table above reaches with a width of two.</summary>
		static Expression Shifted(byte operation, Expression left, Expression right)
		{
			return operation == Less
					? ExpressionParser.Shift(Expression.LeftShift, left, right)
					: ExpressionParser.Shift(Expression.RightShift, left, right);
		}

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

				if (_build)
					node = kind == Increment ? Expression.PreIncrementAssign(target!) : Expression.PreDecrementAssign(target!);

				return at;
			}

			if (kind == Minus || kind == Plus || kind == Not || kind == Tilde)
			{
				var at = Unary(i + 1, out var operand);

				if (at < 0)
					return -1;

				if (_build)
					node = kind switch
					{
						Minus => ExpressionParser.Negate(operand!, Marks),
						Plus  => ExpressionParser.Arithmetic(Expression.UnaryPlus, operand!),
						Not   => Expression.Not(operand!),
						_     => ExpressionParser.Integral(Expression.OnesComplement, operand!),
					};

				return at;
			}

			// A cast is told from a parenthesized expression by what stands inside it, and
			// where that is no type this reading is simply not a cast. The type is built once
			// the operand is there, which is when it is known to be one.
			if (kind == LeftParen)
			{
				var named = Type(i + 1, out _, build: false);

				if (named >= 0 && Kind(named) == RightParen)
				{
					var operand = Unary(named + 1, out var read);

					if (operand >= 0)
					{
						if (_build)
						{
							Type(i + 1, out var type, build: true);

							node = ExpressionParser.Cast(read!, type!, Marks);
						}

						return operand;
					}
				}
			}

			return Postfix(i, out node);
		}

		/// <summary>Everything written after an operand: a member, a call, an index, in a chain read once from left to right.</summary>
		int Postfix(int i, out Expression? node)
		{
			node = null;

			var at = -1;

			// The three heads that are a name and something: a call of one, and the two that
			// write to one.
			if (Kind(i) == Identifier)
			{
				var name = Name(i, out _, build: false);

				if (name >= 0)
				{
					if (Kind(name) == LeftParen)
					{
						var arguments = Arguments(name, out var args);

						if (arguments >= 0)
						{
							if (_build)
							{
								Name(i, out var target);

								node = ExpressionParser.Invoked(target!, args!);
							}

							at = arguments;
						}
					}

					if (at < 0 && (Kind(name) == Increment || Kind(name) == Decrement))
					{
						if (_build)
						{
							Name(i, out var target);

							node = Kind(name) == Increment
								? Expression.PostIncrementAssign(target!)
								: Expression.PostDecrementAssign(target!);
						}

						at = name + 1;
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
					var member = Cut(at + 1);

					if (Kind(at + 2) == LeftParen)
					{
						var arguments = Arguments(at + 2, out var args);

						if (arguments >= 0)
						{
							if (_build)
								node = _context.Calling(node!, member, args!);

							at = arguments;

							continue;
						}
					}

					if (_build)
						node = ExpressionParser.Member(node!, member, _context.Caller);

					at += 2;

					continue;
				}

				if (Kind(at) == LeftBracket)
				{
					var indices = Indices(at, out var read);

					if (indices < 0)
						break;

					if (_build)
						node = ExpressionParser.Indexed(node!, read!, _context.Caller);

					at = indices;

					continue;
				}

				// A guard takes the rest of the chain with it: what is written after it is
				// protected by it, so all of it is read as steps and built inside the test.
				// A `?` that begins no step is the ternary's, and is left where it is.
				if (Kind(at) == Question && (Kind(at + 1) == LeftBracket || Kind(at + 1) == Dot && Kind(at + 2) == Identifier))
				{
					var chain = Chain(at, out var steps);

					if (chain < 0)
						break;

					if (_build)
						node = ExpressionParser.Chained(node!, steps!, _context);

					at = chain;

					continue;
				}

				break;
			}

			return at;
		}

		/// <summary>A guarded step and every step written after it, as the one chain they are.</summary>
		/// <remarks>
		/// Read rather than built, which is what `?.` costs: the steps after the guard belong
		/// inside its test, and a reader that builds as it goes would have built them outside.
		/// </remarks>
		int Chain(int i, out ExpressionParser.Step[]? steps)
		{
			steps = null;

			var read = _build ? new List<ExpressionParser.Step>() : null;
			var at   = i;

			while (true)
			{
				var guarded = Kind(at) == Question;
				var from    = guarded ? at + 1 : at;

				if (Kind(from) == Dot && Kind(from + 1) == Identifier)
				{
					var member    = Cut(from + 1);
					var arguments = Arguments(from + 2, out var args);

					read?.Add(new ExpressionParser.Step(member, arguments >= 0 ? args : null, null, guarded));
					at = arguments >= 0 ? arguments : from + 2;

					continue;
				}

				if (Kind(from) == LeftBracket)
				{
					var indices = Indices(from, out var index);

					if (indices < 0)
						break;

					read?.Add(new ExpressionParser.Step(null, null, index, guarded));
					at = indices;

					continue;
				}

				break;
			}

			if (at == i)
				return -1;

			steps = read?.ToArray();

			return at;
		}

		// ── What an operand is ──────────────────────────────────────────────────

		int Primary(int i, out Expression? node)
		{
			node = null;

			var kind = Kind(i);

			if (kind == KwNew)
				return New(i, out node);

			// A type and something of it, told from `a.b` by whether the name resolves — or a
			// keyword that names a type, which `Core` reads before it asks for a name.
			if (kind == Identifier || IsCore(kind))
			{
				var named = Core(i, out _, build: false);

				if (named >= 0 && Kind(named) == Dot && Kind(named + 1) == Identifier)
				{
					var member = Cut(named + 1);
					var called = Arguments(named + 2, out var args);

					if (_build)
					{
						Core(i, out var type, build: true);

						node = called >= 0
							? ExpressionParser.Called(type!, member, args!, _context.Caller)
							: ExpressionParser.StaticMember(type!, member, _context.Caller);
					}

					return called >= 0 ? called : named + 2;
				}
			}

			if (kind == KwChecked || kind == KwUnchecked)
			{
				if (Kind(i + 1) != LeftParen)
					return -1;

				Mark(kind == KwChecked ? ExpressionParser.Reading.Checked : ExpressionParser.Reading.Unchecked);

				var inner = Assignment(i + 2, out node);

				_marked--;

				if (inner < 0 || Kind(inner) != RightParen)
					return -1;

				return inner + 1;
			}

			// A type where a value is wanted, and what a type defaults to: the same reading
			// twice over, told apart by which word opened it.
			if (kind == KwTypeof || kind == KwDefault)
			{
				if (Kind(i + 1) != LeftParen)
					return -1;

				var read = Type(i + 2, out var type, _build);

				if (read < 0 || Kind(read) != RightParen)
					return -1;

				if (_build)
					node = kind == KwTypeof ? Expression.Constant(type, typeof(Type)) : Expression.Default(type!);

				return read + 1;
			}

			// A name answered with as it was written, which is its last part and nothing
			// looked up.
			if (kind == KwNameof)
			{
				if (Kind(i + 1) != LeftParen || Kind(i + 2) != Identifier)
					return -1;

				var last = i + 2;

				while (Kind(last + 1) == Dot && Kind(last + 2) == Identifier)
					last += 2;

				if (Kind(last + 1) != RightParen)
					return -1;

				if (_build)
					node = Expression.Constant(Cut(last));

				return last + 2;
			}

			// A lambda written where a value is wanted, tried before the parenthesis it begins
			// like: `(int y) => y * 2` and `(y)` are told apart by what stands after the `)`.
			if (kind == LeftParen)
			{
				var lambda = Inner(i, out node);

				if (lambda >= 0)
					return lambda;
			}

			if (kind == LeftParen || kind == Identifier)
			{
				var untyped = Untyped(i, out node);

				if (untyped >= 0)
					return untyped;
			}

			if (kind == LeftParen)
			{
				var inner = Assignment(i + 1, out node);

				if (inner < 0 || Kind(inner) != RightParen)
					return -1;

				return inner + 1;
			}

			if (kind >= Number && kind <= RawLong)
			{
				if (_build)
					node = Literal(i, kind);

				return i + 1;
			}

			if (kind == KwTrue || kind == KwFalse)
			{
				if (_build)
					node = Expression.Constant(kind == KwTrue);

				return i + 1;
			}

			if (kind == KwNull)
			{
				if (_build)
					node = ExpressionParser.Null;

				return i + 1;
			}

			return Name(i, out node);
		}

		void Mark(ExpressionParser.Reading reading)
		{
			_marks ??= new ExpressionParser.Reading[4];

			if (_marked == _marks.Length)
				Array.Resize(ref _marks, _marked * 2);

			_marks[_marked++] = reading;
		}

		int New(int i, out Expression? node)
		{
			node = null;

			var at = Type(i + 1, out var type, _build);

			if (at < 0)
				return -1;

			// `new int[n]`: a size, and the array is the element type's.
			if (Kind(at) == LeftBracket)
			{
				var size = Assignment(at + 1, out var read);

				if (size < 0 || Kind(size) != RightBracket)
					return -1;

				if (_build)
					node = Expression.NewArrayBounds(type!, read!);

				return size + 1;
			}

			// `new int[] { … }`: the brackets belong to the type, and the braces are what tell
			// this from a constructor. Whether the type is an array is a guard's question, so
			// it is built to be asked.
			if (type is null)
				Type(i + 1, out type, build: true);

			if (type!.IsArray && Kind(at) == LeftBrace)
			{
				var from = _values;
				var read = Expressions(at + 1);

				if (Kind(read) != RightBrace)
					return Dropped(from);

				if (_build)
					node = Expression.NewArrayInit(
						type.GetElementType()!, ExpressionParser.Converted(Popped(from), type.GetElementType()!));

				return read + 1;
			}

			if (!type.IsArray)
				Refuse(at);

			var arguments = Arguments(at, out var args);

			if (arguments < 0)
				return -1;

			var fields   = default(ExpressionParser.Setting[]);
			var elements = default(ExpressionParser.Element[]);
			var after    = arguments;

			// One tail rather than three alternatives: what stands inside the braces is what
			// says which of the two it is, and `Name =` is the narrower — asked first, and
			// without building, because it is the other one where it fails further in.
			if (Kind(after) == LeftBrace)
			{
				var bound = Bound(after, out fields);

				if (bound >= 0)
				{
					after = bound;
				}
				else
				{
					var listed = Elements(after + 1, out elements);

					if (listed >= 0 && Kind(listed) == RightBrace)
						after = listed + 1;
					else
						elements = null;
				}
			}

			if (_build)
				node = ExpressionParser.Made(type, args!, fields, elements, _context.Caller);

			return after;
		}

		/// <summary>Member initializers, read first without building, since braces that fail as these are elements.</summary>
		int Bound(int i, out ExpressionParser.Setting[]? settings)
		{
			settings = null;

			Quiet(out var was);

			var bound = Bindings(i, out _);

			_build = was;

			if (bound >= 0 && _build)
				Bindings(i, out settings);

			return bound;
		}

		int Bindings(int i, out ExpressionParser.Setting[]? settings)
		{
			settings = null;

			if (Kind(i) != LeftBrace)
				return -1;

			var read = _build ? new List<ExpressionParser.Setting>() : null;
			var at   = Binding(i + 1, out var first);

			if (at < 0)
				return -1;

			read?.Add(first);

			while (Kind(at) == Comma)
			{
				var more = Binding(at + 1, out var next);

				if (more < 0)
					break;

				read?.Add(next);
				at = more;
			}

			if (Kind(at) != RightBrace)
				return -1;

			settings = read?.ToArray();

			return at + 1;
		}

		/// <summary>What stands after one member's `=`: a value, a nested initializer of members, or one of elements.</summary>
		int Binding(int i, out ExpressionParser.Setting setting)
		{
			setting = default;

			if (Kind(i) != Identifier || Kind(i + 1) != Assign)
				return -1;

			var name = Cut(i);

			if (Kind(i + 2) == LeftBrace)
			{
				var nested = Bound(i + 2, out var inside);

				if (nested >= 0)
				{
					if (_build)
						setting = new ExpressionParser.Setting(name, null, inside, null);

					return nested;
				}

				var listed = Elements(i + 3, out var items);

				if (listed >= 0 && Kind(listed) == RightBrace)
				{
					if (_build)
						setting = new ExpressionParser.Setting(name, null, null, items);

					return listed + 1;
				}
			}

			var value = Assignment(i + 2, out var read);

			if (value < 0)
				return -1;

			if (_build)
				setting = new ExpressionParser.Setting(name, read, null, null);

			return value;
		}

		int Elements(int i, out ExpressionParser.Element[]? elements)
		{
			elements = null;

			var read = _build ? new List<ExpressionParser.Element>() : null;
			var at   = Element(i, out ExpressionParser.Element first);

			if (at < 0)
				return -1;

			read?.Add(first);

			while (Kind(at) == Comma)
			{
				var more = Element(at + 1, out ExpressionParser.Element next);

				if (more < 0)
					break;

				read?.Add(next);
				at = more;
			}

			elements = read?.ToArray();

			return at;
		}

		/// <summary>What one call to `Add` takes: one expression, or for a dictionary two, in braces of their own.</summary>
		int Element(int i, out ExpressionParser.Element element)
		{
			element = default;

			if (Kind(i) == LeftBrace)
			{
				var from = _values;
				var at   = Expressions(i + 1);

				if (at == i + 1 || Kind(at) != RightBrace)
					return Dropped(from);

				if (_build)
					element = new ExpressionParser.Element(Popped(from));

				return at + 1;
			}

			var only = Assignment(i, out var value);

			if (only < 0)
				return -1;

			if (_build)
				element = ExpressionParser.Only(value!);

			return only;
		}

		int Arguments(int i, out Expression[]? arguments)
		{
			arguments = null;

			if (Kind(i) != LeftParen)
				return -1;

			var from = _values;
			var at   = Expressions(i + 1);

			if (Kind(at) != RightParen)
				return Dropped(from);

			arguments = _build ? Popped(from) : [];

			return at + 1;
		}

		int Indices(int i, out Expression[]? indices)
		{
			indices = null;

			if (Kind(i) != LeftBracket)
				return -1;

			var from = _values;
			var at   = Expressions(i + 1);

			if (at == i + 1 || Kind(at) != RightBracket)
				return Dropped(from);

			if (_build)
				indices = Popped(from);

			return at + 1;
		}

		/// <summary>Expressions with commas between them, gathered on the stack: where they end, or where they would have begun if there is none.</summary>
		int Expressions(int i)
		{
			var at = Assignment(i, out var first);

			if (at < 0)
				return i;

			if (_build)
				Push(first!);

			while (Kind(at) == Comma)
			{
				var more = Assignment(at + 1, out var next);

				if (more < 0)
					break;

				if (_build)
					Push(next!);

				at = more;
			}

			return at;
		}

		/// <summary>A word that names a variable, which is what makes it a name at all.</summary>
		int Name(int i, out Expression? node, bool build = true, bool always = false)
		{
			node = null;

			if (Kind(i) != Identifier)
				return -1;

			var name = Cut(i);
			var span = Span(i, i + 1);

			if (!_context.Knows(name, span))
				return Refuse(i + 1);

			if (always || build && _build)
				node = _context.Named(name, span);

			return i + 1;
		}

		// ── Constants ───────────────────────────────────────────────────────────

		readonly Expression Literal(int i, byte kind)
		{
			if (kind == Text)      return Expression.Constant(Unescaped(i));
			if (kind == Character) return Expression.Constant(Unescaped(i)[0]);
			if (kind == Verbatim)  return Expression.Constant(Unverbatim(i));

			if (kind == RawText)
				return Expression.Constant(ExpressionParser.Raw(Cut(i), Count(_text.AsSpan(0, _starts[i] + _lengths[i]), _starts[i], '"')));

			if (kind >= Interpolated)
				return ExpressionParser.Interpolation(Pieces(i, kind), _context, _ascii ? AsciiHole : Hole);

			var digits = Digits(i, kind);

			return kind switch
			{
				Real     => Expression.Constant(double.Parse(digits, NumberStyles.Float, CultureInfo.InvariantCulture)),
				RealD    => Expression.Constant(double.Parse(digits, NumberStyles.Float, CultureInfo.InvariantCulture)),
				RealF    => Expression.Constant(float.Parse(digits, NumberStyles.Float, CultureInfo.InvariantCulture)),
				RealM    => Expression.Constant(decimal.Parse(digits, NumberStyles.Float, CultureInfo.InvariantCulture)),
				// Which type an integer is depends on its value, and the language's own answer
				// to that is the one to give.
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

		/// <summary>An interpolated or raw string cut into what it is made of, which its holes are read over.</summary>
		readonly ExpressionParser.InterpolatedText Pieces(int i, byte kind)
		{
			var from = _starts[i];
			var s    = _text.AsSpan(0, from + _lengths[i]);

			if (kind == RawLong)
				return ExpressionParser.RawByHand(Cut(i), _text, Span(i, i + 1));

			var parts = new List<Segment>();

			if (kind == RawHoles)
			{
				var dollars = Count(s, from, '$');
				var quotes  = Count(s, from + dollars, '"');

				RawHolesEnd(s, from + dollars + quotes, dollars, quotes, parts);

				return ExpressionParser.Raw(parts.ToArray(), _text);
			}

			var verbatim = s[from] == '@' || s[from + 1] == '@';

			InterpolatedEnd(s, from + (verbatim ? 3 : 2), verbatim, parts);

			return new ExpressionParser.InterpolatedText(parts.ToArray(), _text);
		}

		/// <summary>
		/// The digits of a number, without the base it was written in, the suffix that says
		/// its type, or the separators that are no part of its value.
		/// </summary>
		readonly string Digits(int i, byte kind)
		{
			var from = _starts[i];
			var to   = from + _lengths[i];

			// The base is a prefix, and the suffix is the letter or two at the end. Which of
			// them the number has, the kind already says.
			if (kind is >= Hex and <= BitsUL)
				from += 2;

			to -= kind switch
			{
				NumberU or NumberL or HexU or HexL or BitsU or BitsL => 1,
				RealD or RealF or RealM                              => 1,
				NumberUL or HexUL or BitsUL                          => 2,
				_                                                    => 0,
			};

			var span = _text.AsSpan(from, to - from);

			return span.IndexOf('_') < 0 ? span.ToString() : span.ToString().Replace("_", "");
		}

		/// <summary>The text a quoted run stands for, with the escapes it wrote read back.</summary>
		readonly string Unescaped(int i)
		{
			var s    = _text.AsSpan(0, _starts[i] + _lengths[i] - 1);
			var made = new StringBuilder(_lengths[i]);

			for (var at = _starts[i] + 1; at < s.Length;)
			{
				if (s[at] != '\\')
				{
					made.Append(s[at]);
					at++;

					continue;
				}

				var end = EscapeEnd(s, at);

				made.Append(Escaped(s, at, end));
				at = end;
			}

			return made.ToString();
		}

		/// <summary>The text of a verbatim string, every character as written and a doubled quote one.</summary>
		readonly string Unverbatim(int i)
		{
			return _text.Substring(_starts[i] + 2, _lengths[i] - 3).Replace("\"\"", "\"");
		}
	}
}
