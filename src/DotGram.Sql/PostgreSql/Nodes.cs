using System;
using System.Globalization;

using DotGram.Sql.Ast;

namespace DotGram.Sql.PostgreSql;

// Inside the namespace, where they are asked before DotGram.Sql's own Expression, Query and Statement.
using Expression = DotGram.Sql.Ast.Expression;
using Query = DotGram.Sql.Ast.Query;

/// <summary>
/// How <c>PostgreSql.gram</c> makes the common tree out of what it read: the words and lists a rule
/// captured, turned into nodes. Pure factories: they read no host state, so a construction run for a
/// derivation the reading then gives up leaves nothing behind.
/// </summary>
static class Nodes
{
	// ── Names ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// A Unicode name, <c>U&amp;"a"</c>, and the escape character its <c>UESCAPE 'c'</c> named, where one did:
	/// the name as written up to its closing quote, and the character out of the string's quotes.
	/// </summary>
	public static Identifier UnicodeIdentifier(string text, string? escape)
	{
		return new Identifier(text, IdentifierStyle.UnicodeDelimited, EscapeCharacter(escape));
	}

	/// <summary>A Unicode string, <c>U&amp;'a'</c>, and the escape character its <c>UESCAPE 'c'</c> named: the text from its first quote.</summary>
	public static LiteralValue UnicodeString(string text, string? escape)
	{
		return new LiteralValue.String(Quoted(text), StringLiteralKind.Unicode, null, EscapeCharacter(escape));
	}

	/// <summary>A string of a kind its first letter says, <c>E'a'</c> or <c>N'a'</c>: the text from its first quote, as the standard's reading keeps it.</summary>
	public static LiteralValue StringLiteral(string text, StringLiteralKind kind)
	{
		return new LiteralValue.String(Quoted(text), kind);
	}

	/// <summary>A string of bits, <c>B'0101'</c> or <c>X'1FF'</c>: the text from its first quote, and which letter began it.</summary>
	public static LiteralValue BitStringLiteral(string text)
	{
		return new LiteralValue.BitString(Quoted(text)) { Hexadecimal = (text[0] | 0x20) == 'x' };
	}

	/// <summary>The text from its first quote on.</summary>
	public static string Quoted(string text)
	{
		return text.Substring(text.IndexOf('\''));
	}

	/// <summary>The character inside a one-character string, <c>'!'</c> or <c>E'!'</c>, or null where no string was written.</summary>
	static char? EscapeCharacter(string? quoted)
	{
		return quoted is null ? null : quoted[quoted.IndexOf('\'') + 1];
	}

	/// <summary>
	/// Whether a string names an escape character as the server allows one: exactly one character, and none of a hexit,
	/// <c>+</c>, a quote of either kind or a space.
	/// </summary>
	public static bool IsEscape(string quoted)
	{
		var body = quoted.Substring(quoted.IndexOf('\'') + 1, quoted.Length - quoted.IndexOf('\'') - 2);

		if (body.Length != 1)
			return false;

		var c = body[0];

		return !Uri.IsHexDigit(c) && c != '+' && c != '\'' && c != '"' && !char.IsWhiteSpace(c);
	}

	/// <summary>A name and the parts after it, as many as were read.</summary>
	public static QualifiedName Chain(Identifier first, Identifier[]? rest)
	{
		return new QualifiedName(first, rest);
	}

	/// <summary>A list of what was read, the first and the rest, or the empty list where nothing was.</summary>
	public static SqlList<T> List<T>(T? first, T[]? rest)
		where T : class
	{
		return first is null ? [] : SqlList.Own(Syntax.Listed(first, rest));
	}

	/// <summary>A list of what was read, or null where nothing was.</summary>
	public static SqlList<T>? ListOrNull<T>(T? first, T[]? rest)
		where T : class
	{
		return first is null ? null : List(first, rest);
	}

	/// <summary>A list of what a repetition read, or the empty list.</summary>
	public static SqlList<T> List<T>(T[]? items)
	{
		return items is null ? [] : SqlList.Own(items);
	}

	// ── Literals ───────────────────────────────────────────────────────────────

	/// <summary>
	/// A number as written, its kind read from its text: an integer in one of four radixes, a decimal with a
	/// point, or an approximate one with an exponent.
	/// </summary>
	public static LiteralValue Number(string text)
	{
		var kind = text.Length > 1 && text[0] == '0'
			? text[1] switch
			{
				'x' or 'X' => NumericLiteralKind.HexInteger,
				'o' or 'O' => NumericLiteralKind.OctalInteger,
				'b' or 'B' => NumericLiteralKind.BinaryInteger,
				_          => Decimal(text),
			}
			: Decimal(text);

		return new LiteralValue.Numeric(text, kind);
	}

	/// <summary>A decimal number's kind: approximate with an exponent, decimal with a point, an integer otherwise.</summary>
	static NumericLiteralKind Decimal(string text)
	{
		if (text.IndexOfAny(['e', 'E']) >= 0)
			return NumericLiteralKind.Approximate;

		return text.IndexOf('.') >= 0 ? NumericLiteralKind.Decimal : NumericLiteralKind.DecimalInteger;
	}

	/// <summary>
	/// A positional parameter, <c>$1</c>: its number, read from the text after the sign. Leading zeros are the number's,
	/// not the tree's — <c>$01</c> is parameter 1 and is written back as <c>$1</c>, the one normalisation this reading makes.
	/// </summary>
	public static Expression Parameter(string text)
	{
		return new Expression.Parameter(ParameterKind.Positional) { Ordinal = int.Parse(text.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture) };
	}

	/// <summary>Whether a parameter's number is one the server can hold ("parameter number too large" otherwise).</summary>
	public static bool FitsParameter(string text)
	{
		return int.TryParse(text.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out _);
	}

	/// <summary>An integer written in the text, in any of the four radixes, as a number; null where none was written.</summary>
	public static int? Integer(string? text)
	{
		return text is null ? null : TryInteger(text, out var value) ? value : throw new FormatException(text);
	}

	/// <summary>
	/// Whether an integer written in the text — a precision, a length, a bound — is one the server reads as an integer:
	/// a number too large for one is a different token to the server (an FCONST), and a syntax error where an integer
	/// is asked for. Null, where none was written, fits.
	/// </summary>
	public static bool FitsInteger(string? text)
	{
		return text is null || TryInteger(text, out _);
	}

	/// <summary>The value of an integer token, <c>0x1F</c>, <c>0o17</c>, <c>0b11</c>, <c>1_000</c> or plain, where it fits in 32 bits.</summary>
	static bool TryInteger(string text, out int value)
	{
		var radix = text.Length > 1 && text[0] == '0'
			? (text[1] | 0x20) switch { 'x' => 16, 'o' => 8, 'b' => 2, _ => 10 }
			: 10;
		var  total = 0L;

		for (var i = radix == 10 ? 0 : 2; i < text.Length; i++)
		{
			var c = text[i];

			if (c == '_')
				continue;

			var digit = c >= 'a' ? c - 'a' + 10 : c >= 'A' ? c - 'A' + 10 : c - '0';

			total = total * radix + digit;

			if (total > int.MaxValue)
			{
				value = 0;
				return false;
			}
		}

		value = (int)total;
		return true;
	}

	/// <summary>
	/// A date, time or timestamp literal: the standard's record where the string is a plain one, which is all the record
	/// holds; the type and the string, a cast spelled as a typed literal, for a string of any other form.
	/// </summary>
	public static Expression DateTimeLiteral(string kind, LiteralValue literal)
	{
		var which = (kind[0] | 0x20) switch
		{
			'd' => DateTimeLiteralKind.Date,
			't' when kind.Length == 4 => DateTimeLiteralKind.Time,
			_   => DateTimeLiteralKind.Timestamp,
		};

		if (literal is LiteralValue.String { Kind: StringLiteralKind.Character } plain)
			return new LiteralValue.DateTime(which, plain.Text);

		var type = which switch
		{
			DateTimeLiteralKind.Date => DateTimeTypeKind.Date,
			DateTimeLiteralKind.Time => DateTimeTypeKind.Time,
			_                        => DateTimeTypeKind.Timestamp,
		};

		return TypedLiteral(new DataType.DateTime(type), literal);
	}

	/// <summary>
	/// An interval literal: the standard's record where the string is a plain one, which is all the record holds; the
	/// type and the string, a cast spelled as a typed literal, for a string of any other form.
	/// </summary>
	public static Expression IntervalLiteral(LiteralValue literal, IntervalQualifier? qualifier)
	{
		if (literal is LiteralValue.String { Kind: StringLiteralKind.Character } plain)
			return new LiteralValue.Interval(plain.Text, qualifier);

		return TypedLiteral(new DataType.Interval(qualifier), literal);
	}

	/// <summary>A typed literal, <c>t 'text'</c>: a cast of the string to the type, spelled as a typed literal.</summary>
	public static Expression TypedLiteral(DataType type, LiteralValue literal)
	{
		return new Expression.Cast(literal, type) { Spelling = CastSpelling.TypedLiteral };
	}

	/// <summary>
	/// A type named by a plain name and its modifiers, as gram.y's GenericType: the standard's record where the name
	/// is one of the server's own spellings of a standard type — `int4`, `float8`, `bool`, `varchar`, `timestamptz`;
	/// `bpchar` stays a type by name, the tree having no spelling for it —
	/// written plain and alone, so that a text two parsers read is one tree; a type by name for every other.
	/// </summary>
	public static DataType NamedType(QualifiedName name, SqlList<Expression> modifiers)
	{
		if (name.Count == 1 && name[0] is { Style: IdentifierStyle.Regular, UnicodeEscape: null } word)
		{
			// A length that fits 32 bits; any other modifier leaves the type one by name with its modifiers as they are.
			var length = modifiers.Count == 1 && modifiers[0] is LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger } n && FitsInteger(n.Text) ? n.Text : null;
			var core   = word.Text.ToUpperInvariant() switch
			{
				"INT2" or "INT4" or "INT8" or "FLOAT4" or "FLOAT8" when modifiers.Count == 0 => Numeric(word.Text, null, null),
				"BOOL" when modifiers.Count == 0 => new DataType.Boolean { Abbreviated = true },
				"VARCHAR" when modifiers.Count == 0 || length is not null => Characters("VARCHAR", length),
				"TIMESTAMPTZ" or "TIMETZ" when modifiers.Count == 0 || length is not null => Temporal(word.Text, length, "WITH"),
				_ => (DataType?)null,
			};

			if (core is not null)
				return core;
		}

		return new DataType.Named(name, modifiers);
	}

	// ── Operators ──────────────────────────────────────────────────────────────

	/// <summary>
	/// Whether a run of operator characters is read as an operator by its symbol: not one of the symbols the grammar reads
	/// by their own literals at their own levels — the arithmetic, the comparisons, `=>` — and not a run a comment begins
	/// inside (the server would cut the run there, which this lexer cannot).
	/// </summary>
	public static bool IsOperator(string text)
	{
		switch (text)
		{
			case "+" or "-" or "*" or "/" or "%" or "^" or "<" or ">" or "=" or "<=" or ">=" or "<>" or "!=" or "=>":
				return false;
		}

		return text.IndexOf("--", StringComparison.Ordinal) < 0 && text.IndexOf("/*", StringComparison.Ordinal) < 0;
	}

	/// <summary>
	/// Whether a text is a run the lexer reads as one operator token (scan.l): operator characters only, and a run
	/// ending in <c>+</c> or <c>-</c> holding one of <c>~ ! @ # % ^ &amp; | ` ?</c>, so that its end is not a sign.
	/// </summary>
	public static bool IsOperatorRun(string text)
	{
		const string characters = "+-*/<>=~!@#%^&|`?";
		const string qualifying = "~!@#%^&|`?";

		if (text.Length == 0)
			return false;

		foreach (var c in text)
		{
			if (characters.IndexOf(c) < 0)
				return false;
		}

		return text.Length == 1 || text[text.Length - 1] is not ('+' or '-') || text.IndexOfAny(qualifying.ToCharArray()) >= 0;
	}

	/// <summary>An operator's symbol as a name of one part.</summary>
	public static QualifiedName OperatorName(string symbol)
	{
		return new QualifiedName(new Identifier(symbol));
	}

	/// <summary>An operator's symbol with the schemas written before it, <c>pg_catalog.+</c>.</summary>
	public static QualifiedName OperatorName(Identifier[]? schemas, string symbol)
	{
		if (schemas is null)
			return OperatorName(symbol);

		var rest = new Identifier[schemas.Length];

		System.Array.Copy(schemas, 1, rest, 0, schemas.Length - 1);
		rest[rest.Length - 1] = new Identifier(symbol);

		return new QualifiedName(schemas[0], rest);
	}

	/// <summary>
	/// An operator called by its symbol: the standard's node where the tree names the symbol's meaning —
	/// <c>||</c> — and a call by name for every other.
	/// </summary>
	public static Expression Called(string symbol, Expression? left, Expression right)
	{
		if (left is not null && symbol == "||")
			return Binary(left, BinaryOperator.Concatenate, right);

		return new Expression.OperatorCall(OperatorName(symbol), left, right);
	}

	/// <summary>An operator called by name, <c>OPERATOR(schema.op)</c>.</summary>
	public static Expression Called(QualifiedName name, Expression? left, Expression right)
	{
		return new Expression.OperatorCall(name, left, right) { OperatorKeyword = true };
	}

	public static Expression Binary(Expression left, BinaryOperator op, Expression right)
	{
		return new Expression.Binary(left, op, right);
	}

	/// <summary>The operator a symbol of the standard's names.</summary>
	public static BinaryOperator BinaryOf(string symbol)
	{
		return symbol switch
		{
			"+" => BinaryOperator.Add,
			"-" => BinaryOperator.Subtract,
			"*" => BinaryOperator.Multiply,
			"/" => BinaryOperator.Divide,
			"%" => BinaryOperator.Modulo,
			_   => throw new ArgumentOutOfRangeException(nameof(symbol), symbol, "no operator of the standard's"),
		};
	}

	/// <summary>A sign before a value: the standard's unary node, over a number too — the sign is the server's operator, not the number's.</summary>
	public static Expression Signed(string sign, Expression operand)
	{
		return new Expression.Unary(sign == "-" ? UnaryOperator.Minus : UnaryOperator.Plus, operand);
	}

	public static ComparisonOperator ComparisonOf(string symbol)
	{
		return symbol switch
		{
			"=" => ComparisonOperator.Equal,
			"<>" or "!=" => ComparisonOperator.NotEqual,
			"<" => ComparisonOperator.Less,
			">" => ComparisonOperator.Greater,
			"<=" => ComparisonOperator.LessOrEqual,
			_ => ComparisonOperator.GreaterOrEqual,
		};
	}

	public static Expression Compared(Expression left, string symbol, Expression right)
	{
		return new Expression.Comparison(left, ComparisonOf(symbol), right, Exclamation: symbol == "!=");
	}

	public static LikeKind LikeOf(string word)
	{
		return (word[0] | 0x20) switch
		{
			'i' => LikeKind.ILike,
			's' => LikeKind.Similar,
			_   => LikeKind.Like,
		};
	}

	// ── The tails after one operand ────────────────────────────────────────────

	/// <summary>A test after a value, at the IS level: which, negated or not, and what it compares with.</summary>
	public abstract record Test
	{
		public sealed record Null(bool Not, bool Postfix) : Test;
		public sealed record Truth(bool Not, string Word) : Test;
		public sealed record Distinct(bool Not, Expression Right) : Test;
		public sealed record Document(bool Not) : Test;
		public sealed record Normalized(bool Not, string? Form) : Test;
	}

	/// <summary>A comparison after a tested value, the tests after it, and the comparison after those, as far as it goes.</summary>
	public sealed record More(string Op, Expression Right, Test[]? Tails, More? Next);

	/// <summary>
	/// Whether tests and what follows them are read as the server reads them: <c>IS DISTINCT FROM</c> takes its right
	/// operand up to the comparison level, so it is the last test and no comparison follows it.
	/// </summary>
	public static bool TestsFit(Test[]? tails, More? more)
	{
		if (tails is null)
			return true;

		for (var i = 0; i < tails.Length; i++)
		{
			if (tails[i] is Test.Distinct && (i < tails.Length - 1 || more is not null))
				return false;
		}

		return true;
	}

	/// <summary>The value with its tests applied, the comparison after them, and so on, folded from the left.</summary>
	public static Expression Tested(Expression value, Test[]? tails, More? more)
	{
		value = Tested(value, tails);

		for (; more is not null; more = more.Next)
			value = Tested(Compared(value, more.Op, more.Right), more.Tails);

		return value;
	}

	/// <summary>The value with its tests applied, each over the one before.</summary>
	public static Expression Tested(Expression value, Test[]? tails)
	{
		if (tails is null)
			return value;

		foreach (var tail in tails)
		{
			value = tail switch
			{
				Test.Null(var not, var postfix)   => new Expression.IsNull(value, not) { Postfix = postfix },
				Test.Truth(var not, var word)     => new Expression.IsTruth(value, not, (word[0] | 0x20) switch { 't' => BooleanLiteral.True, 'f' => BooleanLiteral.False, _ => BooleanLiteral.Unknown }),
				Test.Distinct(var not, var right) => new Expression.IsDistinct(value, not, right),
				Test.Document(var not)            => new Expression.IsDocument(value, not),
				Test.Normalized(var not, var form) => new Expression.IsNormalized(value, not, form is null ? null : (NormalForm)Enum.Parse(typeof(NormalForm), form.ToUpperInvariant(), true)),
				_ => throw new ArgumentOutOfRangeException(nameof(tails)),
			};
		}

		return value;
	}

	/// <summary>A predicate's tail after its first operand: which, and its parts.</summary>
	public abstract record Tail
	{
		public sealed record Between(bool Not, string? Symmetry, Expression Low, Expression High) : Tail;
		public sealed record In(bool Not, InSource Source) : Tail;
		public sealed record Like(bool Not, string Kind, Expression Pattern, Expression? Escape) : Tail;
		public sealed record Quantified(QuantifiedOperator Operator, string Quantifier, Expression Right) : Tail;
		public sealed record Overlaps(Expression Right) : Tail;
	}

	/// <summary>Whether a tail fits its operand: OVERLAPS takes a row on its left, as gram.y's `row OVERLAPS row` has it.</summary>
	public static bool TailFits(Expression value, Tail? tail)
	{
		return tail is not Tail.Overlaps || value is Expression.Row;
	}

	/// <summary>The predicate the value and its tail make.</summary>
	public static Expression Predicated(Expression value, Tail tail)
	{
		switch (tail)
		{
			case Tail.Between(var not, var symmetry, var low, var high):
				return new Expression.Between(value, not, symmetry is null ? null : (symmetry[0] | 0x20) == 's' ? BetweenSymmetry.Symmetric : BetweenSymmetry.Asymmetric, low, high);

			case Tail.In(var not, var source):
				return new Expression.In(value, not, source);

			case Tail.Like(var not, var kind, var pattern, var escape):
				return new Expression.Like(value, not, LikeOf(kind), pattern, escape);

			case Tail.Quantified(var op, var word, var right):
				var quantifier = (word[1] | 0x20) switch { 'l' => Quantifier.All, 'o' => Quantifier.Some, _ => Quantifier.Any };

				// The standard's shape — a comparison over a subquery — is the standard's record, in every dialect.
				if (op is QuantifiedOperator.Comparison comparison && right is Expression.Subquery subquery)
					return new Expression.QuantifiedComparison(value, comparison.Operator, quantifier, subquery.Query);

				return new Expression.QuantifiedOperation(value, op, quantifier, right);

			case Tail.Overlaps(var other):
				// gram.y: `row OVERLAPS row`, both rows in brackets; the left one was read as the operand.
				return new Expression.Overlaps(value, other);

			default:
				throw new ArgumentOutOfRangeException(nameof(tail));
		}
	}

	// ── The steps after a primary ──────────────────────────────────────────────

	/// <summary>One step of an indirection: a member, a star, a subscript or a slice.</summary>
	public abstract record Step
	{
		public sealed record Member(Identifier Name) : Step;
		public sealed record Star : Step;
		public sealed record Subscript(Expression Index) : Step;
		public sealed record Slice(Expression? Lower, Expression? Upper) : Step;
	}

	/// <summary>The primary with its steps applied, each over the one before.</summary>
	public static Expression Reached(Expression primary, Step[]? steps)
	{
		if (steps is null)
			return primary;

		foreach (var step in steps)
		{
			primary = step switch
			{
				Step.Member(var name)        => new Expression.Member(primary, MemberAccessKind.Dot, name),
				Step.Star                    => new Expression.Wildcard(primary, WildcardKind.Member),
				Step.Subscript(var index)    => new Expression.Element(primary, index),
				Step.Slice(var low, var up)  => new Expression.Element(primary, low, up) { Colon = true },
				_ => throw new ArgumentOutOfRangeException(nameof(steps)),
			};
		}

		return primary;
	}

	// ── Primaries ──────────────────────────────────────────────────────────────

	/// <summary>A row of values, <c>ROW(a, b)</c> or <c>(a, b)</c>.</summary>
	public static Expression Row(Expression? first, Expression[]? rest, bool rowKeyword)
	{
		return new Expression.Row(List(first, rest), rowKeyword);
	}

	/// <summary>What followed the first value inside a bracket: more values, or set operations, or nothing.</summary>
	public abstract record BracketTail
	{
		public sealed record Row(Expression[] Rest) : BracketTail;
		public sealed record Operations(SetOperation[]? List) : BracketTail;
	}

	/// <summary>Whether what followed the first value fits it: set operations follow a subquery only.</summary>
	public static bool BracketFits(Expression first, BracketTail tail)
	{
		return tail is not BracketTail.Operations { List: not null } || first is Expression.Subquery;
	}

	/// <summary>
	/// What a pair of brackets held: a row, where a comma followed the first value; a query, where a set operation did
	/// — the subquery the first value was being its first operand, in brackets of its own; the value in brackets
	/// otherwise, a subquery's brackets being the query's own.
	/// </summary>
	public static Expression Bracket(Expression first, BracketTail tail)
	{
		switch (tail)
		{
			case BracketTail.Row(var rest):
				return Row(first, rest, false);

			case BracketTail.Operations(var operations):
				if (first is not Expression.Subquery subquery)
					return new Expression.Parenthesized(first);

				var query = new Query.Parenthesized(subquery.Query);

				return new Expression.Subquery(Combined(query, operations));

			default:
				throw new ArgumentOutOfRangeException(nameof(tail));
		}
	}

	/// <summary>IN's source, from what its brackets held: the subquery, or the values of the row, or the one value.</summary>
	public static InSource InSourceOf(Expression held)
	{
		return held switch
		{
			Expression.Subquery subquery            => new InSource.Subquery(subquery.Query),
			Expression.Row { RowKeyword: false } row => new InSource.Values(row.Items),
			Expression.Parenthesized(var value)     => new InSource.Values([value]),
			_                                        => new InSource.Values([held]),
		};
	}

	/// <summary>The right side of a quantified form, from what its brackets held: the subquery, or the one value.</summary>
	public static Expression QuantifiedValue(Expression held)
	{
		return held is Expression.Parenthesized(var value) ? value : held;
	}

	/// <summary>An array, <c>ARRAY[a, b]</c>, nested or empty.</summary>
	public static Expression Array(Expression? first, Expression[]? rest)
	{
		return new Expression.Array(List(first, rest));
	}

	/// <summary>A call of a function by a key word the server spells it with, its arguments plain values.</summary>
	public static Expression Invoked(string word, Expression? first, Expression[]? rest)
	{
		var values = List(first, rest);
		var arguments = new Argument[values.Count];

		for (var i = 0; i < arguments.Length; i++)
			arguments[i] = new Argument(values[i]);

		return new Expression.Invocation(OperatorName(word), SqlList.Own(arguments));
	}

	/// <summary>A call of a function by a key word, its arguments as they were read.</summary>
	public static Expression Invoked(string word, Argument? first, Argument[]? rest)
	{
		return new Expression.Invocation(OperatorName(word), List(first, rest));
	}

	/// <summary>What stood after a function's name: a bracketed call and what followed its close, or a string alone.</summary>
	public abstract record FuncTail
	{
		public sealed record Call(CallBody Body, CallRest Rest) : FuncTail;
		public sealed record Typed(LiteralValue Literal) : FuncTail;
	}

	/// <summary>What stood inside a call's brackets: a star, or the arguments with their quantifier and order.</summary>
	public abstract record CallBody
	{
		public sealed record Star : CallBody;
		public sealed record Arguments(string? Quantifier, SqlList<Argument> List, OrderByClause? Order) : CallBody;
	}

	/// <summary>What stood after a call's close: a string, which makes the call a typed literal, or the clauses.</summary>
	public abstract record CallRest
	{
		public sealed record Typed(LiteralValue Literal) : CallRest;
		public sealed record Clauses(WithinGroupClause? Within, FilterClause? Filter, WindowReference? Over) : CallRest;
	}

	/// <summary>
	/// Whether a call's body may be a type's modifiers, where a string after the call makes it a type: plain values,
	/// at least one — no quantifier, no order, no star, no name, no VARIADIC — as the server reads them.
	/// </summary>
	public static bool ModifiersFit(CallBody body, CallRest rest)
	{
		if (rest is not CallRest.Typed)
			return true;

		if (body is not CallBody.Arguments { Quantifier: null, Order: null, List.Count: > 0 } arguments)
			return false;

		foreach (var argument in arguments.List)
		{
			if (argument.Variadic || argument.Name is not null)
				return false;
		}

		return true;
	}

	/// <summary>Whether a key-word function was given as many values as it takes: NULLIF two, the others one or more.</summary>
	public static bool ArityFits(string word, Expression[]? rest)
	{
		return (word[0] | 0x20) != 'n' || rest is { Length: 1 };
	}

	/// <summary>A name and what followed it: a call, a call turned into a typed literal, or a typed literal on the bare name.</summary>
	public static Expression Function(QualifiedName name, FuncTail tail)
	{
		switch (tail)
		{
			case FuncTail.Typed(var literal):
				// `date '…'` is the standard's date literal (`date` is no key word of PostgreSQL's, and reaches here as a name).
				if (name.Count == 1 && name[0] is { Style: IdentifierStyle.Regular, UnicodeEscape: null } word && word.Text.Equals("date", StringComparison.OrdinalIgnoreCase))
					return DateTimeLiteral("DATE", literal);

				return TypedLiteral(NamedType(name, []), literal);

			case FuncTail.Call(var body, var rest):
				var arguments = body is CallBody.Arguments a ? a : null;
				var call = new Expression.Invocation(name, body is CallBody.Star ? [new Argument(new Expression.Asterisk())] : arguments!.List)
				{
					Quantifier = arguments?.Quantifier is { } quantifier ? (quantifier[0] | 0x20) == 'd' ? SetQuantifier.Distinct : SetQuantifier.All : null,
					OrderBy    = arguments?.Order,
				};

				switch (rest)
				{
					case CallRest.Typed(var literal):
						// `numeric(10, 2) '1.5'`: the call's arguments are the type's modifiers.
						var modifiers = new Expression[call.Arguments.Count];

						for (var i = 0; i < modifiers.Length; i++)
							modifiers[i] = call.Arguments[i].Value;

						return TypedLiteral(NamedType(name, SqlList.Own(modifiers)), literal);

					case CallRest.Clauses(var within, var filter, var over):
						return within is null && filter is null && over is null ? call : call with { WithinGroup = within, Filter = filter, Over = over };

					default:
						throw new ArgumentOutOfRangeException(nameof(tail));
				}

			default:
				throw new ArgumentOutOfRangeException(nameof(tail));
		}
	}

	/// <summary>A select item's label: the name, and whether AS stood before it.</summary>
	public sealed record Label(Identifier Name, bool AsKeyword);

	public static SortItem Sorted(Expression key, string? direction, string? nulls)
	{
		return new SortItem(
			key,
			direction is null ? null : (direction[0] | 0x20) == 'a' ? SortDirection.Asc : SortDirection.Desc,
			nulls is null ? null : (nulls[0] | 0x20) == 'f' ? NullOrdering.First : NullOrdering.Last);
	}

	/// <summary>The key-word values, <c>CURRENT_DATE</c> and the rest, and a precision where one was written.</summary>
	public static Expression Current(string word, string? precision)
	{
		var kind = word.ToUpperInvariant() switch
		{
			"CURRENT_DATE"      => CurrentValue.Date,
			"CURRENT_TIME"      => CurrentValue.Time,
			"CURRENT_TIMESTAMP" => CurrentValue.Timestamp,
			"LOCALTIME"         => CurrentValue.LocalTime,
			"LOCALTIMESTAMP"    => CurrentValue.LocalTimestamp,
			"CURRENT_ROLE"      => CurrentValue.Role,
			"CURRENT_USER"      => CurrentValue.CurrentUser,
			"SESSION_USER"      => CurrentValue.SessionUser,
			"SYSTEM_USER"       => CurrentValue.SystemUser,
			"USER"              => CurrentValue.User,
			"CURRENT_CATALOG"   => CurrentValue.Catalog,
			_                   => CurrentValue.Schema,
		};

		return new Expression.Current(kind, null, precision is null ? null : Number(precision));
	}

	/// <summary>A cast, in the spelling it was written.</summary>
	public static Expression Cast(Expression value, DataType type, CastSpelling? spelling)
	{
		return new Expression.Cast(value, type) { Spelling = spelling };
	}

	/// <summary>EXTRACT's field: one the standard names, or any other.</summary>
	public abstract record Field
	{
		public sealed record Named(string Word) : Field;
		public sealed record Other(Expression Value) : Field;
	}

	public static Expression Extracted(Field field, Expression source)
	{
		return field switch
		{
			Field.Named(var word) => new Expression.Extract((ExtractField)Enum.Parse(typeof(ExtractField), word, true), source),
			Field.Other(var value) => new Expression.Extract(null, source) { NamedField = value },
			_ => throw new ArgumentOutOfRangeException(nameof(field)),
		};
	}

	public static Expression Normalized(Expression value, string? form)
	{
		return new Expression.Normalize(value, form is null ? null : (NormalForm)Enum.Parse(typeof(NormalForm), form.ToUpperInvariant(), true), null);
	}

	/// <summary>
	/// SUBSTRING's arguments after the value: the start, the count, which came first, or a pattern and its escape; or
	/// the plain values after it, which make the call one by name.
	/// </summary>
	public sealed record Substr(Expression? From, Expression? For, bool ForFirst, Expression? Escape, Expression[]? More);

	public static Expression Substring(Expression value, Substr s)
	{
		if (s.More is not null || (s.From is null && s.For is null && s.Escape is null))
			return Invoked("SUBSTRING", value, s.More);

		if (s.Escape is { } escape)
			return new Expression.SubstringSimilar(value, s.From!, escape);

		return new Expression.Substring(value, s.From, s.For, null) { ForFirst = s.ForFirst };
	}

	/// <summary>OVERLAY's arguments after the value: the standard's PLACING form, or the plain values after it.</summary>
	public sealed record Overlay(Expression? Placing, Expression? From, Expression? For, Expression[]? More);

	public static Expression Overlaid(Expression value, Overlay o)
	{
		if (o.Placing is null)
			return Invoked("OVERLAY", value, o.More);

		return new Expression.Overlay(value, o.Placing, o.From!, o.For, null);
	}

	/// <summary>
	/// TRIM's arguments: the characters before FROM, whether FROM was written, and the values after it (the source, or
	/// a list the standard's record cannot hold).
	/// </summary>
	public sealed record TrimArguments(Expression? Characters, bool FromKeyword, SqlList<Expression> Values)
	{
		/// <summary>The first value read, and what followed it: the rest of a list, or FROM and the source.</summary>
		public static TrimArguments Of(Expression first, TrimArguments tail)
		{
			return tail.FromKeyword ? tail with { Characters = first } : tail with { Values = [first, .. tail.Values] };
		}
	}

	public static Expression Trimmed(string? specification, TrimArguments t)
	{
		var spec = specification is null ? null : (TrimSpecification?)((specification[0] | 0x20) switch { 'b' => TrimSpecification.Both, 'l' => TrimSpecification.Leading, _ => TrimSpecification.Trailing });

		if (t.Values.Count == 1)
			return new Expression.Trim(spec, t.Characters, t.FromKeyword, t.Values[0]);

		// A list of values is a call of `trim` by name, as the server reads it (btrim and its kin).
		var arguments = new Argument[t.Values.Count + (t.Characters is null ? 0 : 1)];
		var at = 0;

		if (t.Characters is { } characters)
			arguments[at++] = new Argument(characters);

		foreach (var value in t.Values)
			arguments[at++] = new Argument(value);

		return new Expression.Invocation(OperatorName("TRIM"), SqlList.Own(arguments));
	}

	// ── Types ──────────────────────────────────────────────────────────────────

	/// <summary>The array bounds after a type: the standard's <c>ARRAY[n]</c>, or PostgreSQL's brackets, one pair a dimension.</summary>
	public abstract record Bounds
	{
		public sealed record Standard(string? Cardinality) : Bounds;
		public sealed record Brackets(Dimension[]? Dimensions) : Bounds;
	}

	/// <summary>One pair of brackets after a type, and the bound inside it where one was written.</summary>
	public sealed record Dimension(int? Bound);

	/// <summary>A type with its array bounds applied; <c>SETOF</c> is not a type of the tree's and is refused as a gap.</summary>
	public static DataType Typed(DataType element, Bounds bounds, bool setOf)
	{
		if (setOf)
			return new DataType.Extension("PostgreSQL", "SETOF", [element]);

		switch (bounds)
		{
			case Bounds.Standard(var cardinality):
				return new DataType.Array(element, Integer(cardinality));

			case Bounds.Brackets(var dimensions):
				if (dimensions is null)
					return element;

				var type = element;

				foreach (var dimension in dimensions)
					type = new DataType.Array(type, dimension.Bound) { Brackets = true };

				return type;

			default:
				throw new ArgumentOutOfRangeException(nameof(bounds));
		}
	}

	public static DataType Numeric(string kind, string? precision, string? scale)
	{
		var which = kind.ToUpperInvariant() switch
		{
			"INT"              => NumericTypeKind.Int,
			"INTEGER"          => NumericTypeKind.Integer,
			"SMALLINT"         => NumericTypeKind.SmallInt,
			"BIGINT"           => NumericTypeKind.BigInt,
			"REAL"             => NumericTypeKind.Real,
			"INT2"             => NumericTypeKind.Int2,
			"INT4"             => NumericTypeKind.Int4,
			"INT8"             => NumericTypeKind.Int8,
			"FLOAT4"           => NumericTypeKind.Float4,
			"FLOAT8"           => NumericTypeKind.Float8,
			"FLOAT"            => NumericTypeKind.Float,
			"DOUBLE PRECISION" => NumericTypeKind.DoublePrecision,
			"DECIMAL"          => NumericTypeKind.Decimal,
			"DEC"              => NumericTypeKind.Dec,
			_                  => NumericTypeKind.Numeric,
		};

		return new DataType.Numeric(which, Integer(precision), Integer(scale));
	}

	/// <summary>
	/// An exact number's type, whose modifiers gram.y reads as any expressions (opt_type_modifiers): the standard's record
	/// where they are a precision and a scale, integers that fit; a type by the name written, with the modifiers as they
	/// are, otherwise — which the server reads and refuses later, as it reads `numeric(1.5)`.
	/// </summary>
	public static DataType Exact(string kind, Expression? first, Expression[]? rest)
	{
		if (first is null)
			return Numeric(kind, null, null);

		var scale = rest is { Length: 1 } ? rest[0] : null;

		if (rest is not { Length: > 1 } && IsIntegerThatFits(first) && (scale is null || IsIntegerThatFits(scale)))
			return Numeric(kind, ((LiteralValue.Numeric)first).Text, scale is null ? null : ((LiteralValue.Numeric)scale).Text);

		return NamedType(new QualifiedName(new Identifier(kind)), List(first, rest));
	}

	/// <summary>Whether a value is an integer literal, in any radix, that fits 32 bits.</summary>
	static bool IsIntegerThatFits(Expression value)
	{
		return value is LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger or NumericLiteralKind.HexInteger or NumericLiteralKind.OctalInteger or NumericLiteralKind.BinaryInteger } number
			&& FitsInteger(number.Text);
	}

	public static DataType Bits(bool varying, string? length)
	{
		return new DataType.BitString(varying ? BitStringTypeKind.BitVarying : BitStringTypeKind.Bit, Integer(length));
	}

	/// <summary>A character type by the words written, which are the standard's spellings, and its length.</summary>
	public static DataType Characters(string words, string? length)
	{
		var normalized = string.Join(" ", words.ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
		var kind = normalized switch
		{
			"CHARACTER VARYING"          => CharacterTypeKind.CharacterVarying,
			"CHAR VARYING"               => CharacterTypeKind.CharVarying,
			"VARCHAR"                    => CharacterTypeKind.Varchar,
			"NATIONAL CHARACTER VARYING" => CharacterTypeKind.NationalCharacterVarying,
			"NATIONAL CHAR VARYING"      => CharacterTypeKind.NationalCharVarying,
			"NCHAR VARYING"              => CharacterTypeKind.NcharVarying,
			"NATIONAL CHARACTER"         => CharacterTypeKind.NationalCharacter,
			"NATIONAL CHAR"              => CharacterTypeKind.NationalChar,
			"CHARACTER"                  => CharacterTypeKind.Character,
			"CHAR"                       => CharacterTypeKind.Char,
			_                            => CharacterTypeKind.Nchar,
		};

		return new DataType.Character(kind, Integer(length));
	}

	/// <summary>A time or a timestamp, its precision, and its time zone; the one-word spellings say WITH.</summary>
	public static DataType Temporal(string word, string? precision, string? zone)
	{
		var upper = word.ToUpperInvariant();
		var kind  = upper.StartsWith("TIMESTAMP", StringComparison.Ordinal) ? DateTimeTypeKind.Timestamp : DateTimeTypeKind.Time;
		var mode  = zone is null ? (TimeZoneMode?)null : zone.StartsWith("WITHOUT", StringComparison.OrdinalIgnoreCase) ? TimeZoneMode.Without : TimeZoneMode.With;

		return new DataType.DateTime(kind, Integer(precision), mode) { Abbreviated = upper.EndsWith("TZ", StringComparison.Ordinal) };
	}

	/// <summary>A type by name, with the modifiers written after it.</summary>
	public static DataType NamedType(Identifier first, Identifier[]? rest, Expression? a, Expression[]? b)
	{
		return NamedType(Chain(first, rest), List(a, b));
	}

	public static IntervalQualifier Qualifier(string start, string? leadingPrecision, string? end, string? fractionalPrecision)
	{
		return new IntervalQualifier(
			(DateTimeField)Enum.Parse(typeof(DateTimeField), start, true),
			Integer(leadingPrecision),
			end is null ? null : (DateTimeField)Enum.Parse(typeof(DateTimeField), end, true),
			Integer(fractionalPrecision));
	}

	// ── Queries ────────────────────────────────────────────────────────────────

	/// <summary>What stood after SELECT: a quantifier, or the values of DISTINCT ON.</summary>
	public abstract record Distinct
	{
		public sealed record Of(SetQuantifier Quantifier) : Distinct;
		public sealed record On(Expression First, Expression[]? Rest) : Distinct;
	}

	/// <summary>Whether the target list may be empty: after SELECT and SELECT ALL, not after DISTINCT.</summary>
	public static bool TargetsFit(Distinct? distinct, SelectColumn? first)
	{
		return first is not null || distinct is null or Distinct.Of { Quantifier: SetQuantifier.All };
	}

	public static Query Selected(Distinct? distinct, SelectColumn? first, SelectColumn[]? rest, FromClause? from, Expression? where, GroupByClause? group, Expression? having, WindowClause? window)
	{
		var query = new Query.Select { Columns = List(first, rest), From = from, Where = where, GroupBy = group, Having = having, Window = window };

		return distinct switch
		{
			null => query,
			Distinct.Of(var quantifier) => query with { Quantifier = quantifier },
			Distinct.On(var head, var tail) => query with { Quantifier = SetQuantifier.Distinct, DistinctOn = List(head, tail) },
			_ => throw new ArgumentOutOfRangeException(nameof(distinct)),
		};
	}

	/// <summary>`TABLE t`, which is `SELECT * FROM t` and says so; the relation may carry ONLY or the star.</summary>
	public static Query Tabled(TableSource relation)
	{
		return new Query.Select { Columns = [new SelectColumn.All()], From = new FromClause([relation]), Table = true };
	}

	/// <summary>ALL or DISTINCT by its word; null where none was written.</summary>
	public static SetQuantifier? QuantifierOf(string? word)
	{
		return word is null ? null : (word[0] | 0x20) == 'a' ? SetQuantifier.All : SetQuantifier.Distinct;
	}

	// ── The clauses of the whole result ────────────────────────────────────────

	/// <summary>What followed ORDER BY: the limit and the locking clauses, in either order.</summary>
	public sealed record SelectTail(Limit? Limit, LockingClause[]? Locking);

	/// <summary>
	/// What gram.y's select_no_parens refuses in its actions: where the operand is one query in brackets with no set
	/// operation after it, a second WITH, ORDER BY, LIMIT or OFFSET on it ("multiple ORDER BY clauses not allowed");
	/// WITH TIES with no ORDER BY to break them by; and FOR READ ONLY beside another locking clause.
	/// </summary>
	public static bool ClausesFit(WithClause? with, Query body, OrderByClause? order, SelectTail? tail)
	{
		var limit   = tail?.Limit;
		var locking = tail?.Locking;

		if (!TiesAndSkipFit(limit, locking))
			return false;

		if (locking is { Length: > 1 })
		{
			foreach (var clause in locking)
			{
				if (clause.Mode == LockingMode.ReadOnly)
					return false;
			}
		}

		var inner = body;

		while (inner is Query.Parenthesized { SetOperations.Count: 0 } bracketed)
			inner = bracketed.Query;

		if (ReferenceEquals(inner, body))
			return limit is not { WithTies: true } || order is not null;

		if (with is not null && inner.With is not null)
			return false;

		if (order is not null && inner.OrderBy is not null)
			return false;

		if (limit is not null && inner.Limit is { } had)
		{
			if (limit.Offset is not null && had.Offset is not null)
				return false;

			if ((limit.Count is not null || limit.LimitAll || limit.FetchRowWord is not null) && (had.Count is not null || had.LimitAll || had.FetchRowWord is not null))
				return false;
		}

		return limit is not { WithTies: true } || order is not null || inner.OrderBy is not null;
	}

	/// <summary>gram.y: "SKIP LOCKED and WITH TIES options cannot be used together" — on one level; a bracketed query's WITH TIES is its own.</summary>
	static bool TiesAndSkipFit(Limit? limit, LockingClause[]? locking)
	{
		if (limit is not { WithTies: true } || locking is null)
			return true;

		foreach (var clause in locking)
		{
			if (clause.Wait == LockWait.SkipLocked)
				return false;
		}

		return true;
	}

	/// <summary>The query with the clauses of the whole result on it, as the standard's reading places them.</summary>
	public static Query Finished(WithClause? with, Query body, OrderByClause? order, SelectTail? tail)
	{
		if (with is null && order is null && tail is null)
			return body;

		return body with
		{
			With    = with ?? body.With,
			OrderBy = order ?? body.OrderBy,
			Limit   = tail?.Limit ?? body.Limit,
			Locking = tail?.Locking is { } locking ? List(locking) : body.Locking,
		};
	}

	/// <summary>LIMIT (or FETCH) and OFFSET as one limit, and which was written first.</summary>
	public static Limit Limited(Limit? count, Limit? offset, bool offsetFirst)
	{
		if (offset is null)
			return count!;

		if (count is null)
			return offset;

		return count with { Offset = offset.Offset, OffsetRowWord = offset.OffsetRowWord, OffsetAfter = !offsetFirst };
	}

	/// <summary>FETCH FIRST | NEXT [n] ROW | ROWS ONLY | WITH TIES, as the standard's reading builds it.</summary>
	public static Limit Fetched(string position, Expression? count, string word, string ties)
	{
		return new Limit
		{
			Count        = count,
			FetchRowWord = RowWordOf(word),
			Position     = (position[0] | 0x20) == 'f' ? FetchPosition.First : FetchPosition.Next,
			WithTies     = (ties[0] | 0x20) == 'w',
		};
	}

	public static RowWord? RowWordOf(string? word)
	{
		return word is null ? null : word.Length == 3 ? RowWord.Row : RowWord.Rows;
	}

	/// <summary>
	/// Whether a value is gram.y's select_fetch_first_value: a primary (c_expr), or a signed number; a larger
	/// expression is refused there, so that `OFFSET 1 ROWS` is told from `OFFSET 1 - 1` with ROWS after it.
	/// </summary>
	public static bool IsFetchValue(Expression? value)
	{
		return value switch
		{
			null => true,
			Expression.Unary { Operator: UnaryOperator.Minus or UnaryOperator.Plus, Operand: LiteralValue.Numeric } => true,
			Expression.Unary or Expression.Binary or Expression.OperatorCall or Expression.Comparison or Expression.Cast { Spelling: CastSpelling.DoubleColon }
				or Expression.AtTimeZone or Expression.Collate or Expression.IsNull or Expression.IsTruth or Expression.IsDistinct or Expression.IsNormalized
				or Expression.IsDocument or Expression.Between or Expression.In or Expression.Like or Expression.QuantifiedComparison or Expression.QuantifiedOperation or Expression.Overlaps => false,
			_ => true,
		};
	}

	/// <summary>A locking clause: its strength by its words, the tables after OF, and NOWAIT or SKIP LOCKED.</summary>
	public static LockingClause Locked(string mode, QualifiedName? first, QualifiedName[]? rest, string? wait)
	{
		var strength = (mode[0] | 0x20) switch
		{
			'n' => LockingMode.NoKeyUpdate,
			'u' => LockingMode.Update,
			'k' => LockingMode.KeyShare,
			_   => LockingMode.Share,
		};

		SqlList<QualifiedName> of = [];

		if (first is { } head)
		{
			var names = new QualifiedName[1 + (rest?.Length ?? 0)];

			names[0] = head;
			rest?.CopyTo(names, 1);
			of = SqlList.Own(names);
		}

		return new LockingClause(strength, of)
		{
			Wait = wait is null ? null : (wait[0] | 0x20) == 'n' ? LockWait.NoWait : LockWait.SkipLocked,
		};
	}

	/// <summary>A common table expression, with AS [NOT] MATERIALIZED where it was written.</summary>
	public static CommonTableExpression Cte(Identifier name, Identifier? first, Identifier[]? rest, string? materialized, Query query, SearchClause? search, CycleClause? cycle)
	{
		return new CommonTableExpression(name, List(first, rest), query, search, cycle)
		{
			Materialized = materialized is null ? null : (materialized[0] | 0x20) != 'n',
		};
	}

	public static SearchClause Searched(string order, Identifier first, Identifier[]? rest, Identifier set)
	{
		return new SearchClause((order[0] | 0x20) == 'd' ? SearchOrder.DepthFirst : SearchOrder.BreadthFirst, List(first, rest), set);
	}

	// ── Grouping and windows ───────────────────────────────────────────────────

	/// <summary>A grouping item: a row in brackets is the standard's bracketed grouping set, any other value a set of one.</summary>
	public static GroupingElement GroupItem(Expression value)
	{
		return value is Expression.Row { RowKeyword: false } row
			? new GroupingElement.Ordinary(row.Items, true)
			: new GroupingElement.Ordinary([value]);
	}

	/// <summary>ROLLUP's or CUBE's items, each a grouping item.</summary>
	public static SqlList<GroupingElement> GroupItems(Expression first, Expression[]? rest)
	{
		var values = List(first, rest);
		var items  = new GroupingElement[values.Count];

		for (var at = 0; at < items.Length; at++)
			items[at] = GroupItem(values[at]);

		return SqlList.Own(items);
	}

	public static WindowFrame Framed(string unit, WindowFrameExtent extent, WindowFrameExclusion? exclusion)
	{
		var which = (unit[0] | 0x20) switch
		{
			'r' when unit.Length == 5 => WindowFrameUnit.Range,
			'r' => WindowFrameUnit.Rows,
			_   => WindowFrameUnit.Groups,
		};

		return new WindowFrame(which, extent, exclusion, null);
	}

	/// <summary>A frame bound by its direction, UNBOUNDED where no value was written.</summary>
	public static WindowFrameBound Bound(string direction, Expression? value)
	{
		var preceding = (direction[0] | 0x20) == 'p';

		if (value is null)
			return new WindowFrameBound(preceding ? WindowBoundKind.UnboundedPreceding : WindowBoundKind.UnboundedFollowing);

		return new WindowFrameBound(preceding ? WindowBoundKind.Preceding : WindowBoundKind.Following, value);
	}

	/// <summary>
	/// What gram.y refuses of a frame in its actions: a single bound that follows, a start at UNBOUNDED FOLLOWING, an
	/// end at UNBOUNDED PRECEDING, and a start after its end (a following start with a current-row or preceding end,
	/// a current-row start with a preceding end).
	/// </summary>
	public static bool FrameFits(WindowFrameExtent extent)
	{
		switch (extent)
		{
			case WindowFrameExtent.Single(var only):
				return only.Kind is not (WindowBoundKind.Following or WindowBoundKind.UnboundedFollowing);

			case WindowFrameExtent.Between(var from, var to):
				if (from.Kind == WindowBoundKind.UnboundedFollowing || to.Kind == WindowBoundKind.UnboundedPreceding)
					return false;

				if (from.Kind == WindowBoundKind.Following && to.Kind is WindowBoundKind.CurrentRow or WindowBoundKind.Preceding)
					return false;

				return from.Kind != WindowBoundKind.CurrentRow || to.Kind != WindowBoundKind.Preceding;

			default:
				throw new ArgumentOutOfRangeException(nameof(extent));
		}
	}

	public static WindowFrameExclusion Exclusion(string words)
	{
		return (words[0] | 0x20) switch
		{
			'c' => WindowFrameExclusion.CurrentRow,
			'g' => WindowFrameExclusion.Group,
			't' => WindowFrameExclusion.Ties,
			_   => WindowFrameExclusion.NoOthers,
		};
	}

	// ── The sources of a FROM ──────────────────────────────────────────────────

	/// <summary>A relation with its alias and its TABLESAMPLE.</summary>
	public static TableSource Aliased(TableSource relation, Alias? alias, SampleClause? sample)
	{
		var named = (TableSource.Named)relation;

		return named with { Alias = alias, Sample = sample };
	}

	public static SampleClause Sample(string method, Expression percentage, Expression? repeat)
	{
		return new SampleClause((method[0] | 0x20) == 'b' ? SampleMethod.Bernoulli : SampleMethod.System, percentage, repeat);
	}

	/// <summary>Whether a call may stand as a table: a call by name with no FILTER, WITHIN GROUP or OVER (func_expr_windowless).</summary>
	public static bool IsWindowless(Expression call)
	{
		return call is Expression.Invocation { Filter: null, WithinGroup: null, Over: null };
	}

	/// <summary>A column's name and, where a type follows it, a column definition.</summary>
	public sealed record ColumnDef(Identifier Name, DataType? Type);

	/// <summary>A function's alias: the name, and the column names or the column definitions after it.</summary>
	public sealed record FuncAlias(Alias? Alias, SqlList<FieldDefinition>? Definitions);

	/// <summary>
	/// Whether what followed a function is one of gram.y's func_alias_clause shapes: a name, with column names or
	/// column definitions in brackets; or AS and column definitions alone. A list is of names or of definitions, never
	/// both.
	/// </summary>
	public static bool FuncAliasFits(bool asWord, Identifier? name, ColumnDef? first, ColumnDef[]? rest)
	{
		if (first is null)
			return name is not null;

		var typed = first.Type is not null;

		if (rest is not null)
		{
			foreach (var column in rest)
			{
				if (column.Type is not null != typed)
					return false;
			}
		}

		return name is not null || asWord && typed;
	}

	public static FuncAlias FuncAliasOf(bool asWord, Identifier? name, ColumnDef? first, ColumnDef[]? rest)
	{
		if (first is null)
			return new FuncAlias(new Alias(name!, null, asWord), null);

		var columns = List(first, rest);

		if (first.Type is null)
		{
			var names = new Identifier[columns.Count];

			for (var at = 0; at < names.Length; at++)
				names[at] = columns[at].Name;

			return new FuncAlias(new Alias(name!, SqlList.Own(names), asWord), null);
		}

		var definitions = new FieldDefinition[columns.Count];

		for (var at = 0; at < definitions.Length; at++)
			definitions[at] = new FieldDefinition(columns[at].Name, columns[at].Type!);

		return new FuncAlias(name is null ? null : new Alias(name, null, asWord), SqlList.Own(definitions));
	}

	/// <summary>
	/// A function as a table: the standard's <c>Unnest</c> for a plain, unqualified <c>unnest</c> with value arguments and
	/// no column definitions, so that a text both readings read is one tree; a function source for every other.
	/// </summary>
	public static TableSource FunctionSource(bool lateral, Expression call, bool ordinality, FuncAlias? alias)
	{
		var invocation = (Expression.Invocation)call;

		if (alias?.Definitions is null && invocation.Name is [{ Style: IdentifierStyle.Regular, UnicodeEscape: null } word]
			&& word.Text.Equals("unnest", StringComparison.OrdinalIgnoreCase) && invocation.Quantifier is null && invocation.OrderBy is null && PlainArguments(invocation))
		{
			var values = new Expression[invocation.Arguments.Count];

			for (var at = 0; at < values.Length; at++)
				values[at] = invocation.Arguments[at].Value;

			return new TableSource.Unnest(SqlList.Own(values), ordinality, alias?.Alias) { Lateral = lateral };
		}

		return new TableSource.Function(invocation, alias?.Alias) { Lateral = lateral, WithOrdinality = ordinality, ColumnDefinitions = alias?.Definitions };
	}

	static bool PlainArguments(Expression.Invocation invocation)
	{
		foreach (var argument in invocation.Arguments)
		{
			if (argument.Name is not null || argument.Variadic)
				return false;
		}

		return true;
	}

	public static TableSource RowsFrom(bool lateral, RowsFromItem first, RowsFromItem[]? rest, bool ordinality, FuncAlias? alias)
	{
		return new TableSource.RowsFrom(List(first, rest), ordinality, alias?.Alias) { Lateral = lateral, ColumnDefinitions = alias?.Definitions };
	}

	// ── One parenthesis in a FROM ──────────────────────────────────────────────

	/// <summary>What a parenthesis in a FROM held: a query, or a table reference (a join, a parenthesis, a subquery).</summary>
	public sealed record Held(Query? Query, TableSource? Source);

	/// <summary>
	/// Whether what followed a table reference inside a parenthesis fits it: set operations, ORDER BY, a limit or a lock follow
	/// a bare subquery only, as the operands of gram.y's select_with_parens, and never a join; and the subquery's own WITH TIES
	/// and SKIP LOCKED do not meet.
	/// </summary>
	public static bool HeldClausesFit(TableSource source, SetOperation[]? operations, OrderByClause? order, SelectTail? tail)
	{
		if (operations is null or { Length: 0 } && order is null && tail is null)
			return true;

		if (source is not TableSource.Subquery { Alias: null, Lateral: false } subquery)
			return false;

		return ClausesFit(null, new Query.Parenthesized(subquery.Query) with { SetOperations = List(operations) }, order, tail);
	}

	/// <summary>
	/// What the parenthesis held, by what followed its first element: a bare subquery alone is a query in its own
	/// parentheses, as the standard's reading has it; with set operations or clauses after it, the query they make; a join
	/// or a parenthesis is the source it is.
	/// </summary>
	public static Held HeldOf(TableSource source, SetOperation[]? operations, OrderByClause? order, SelectTail? tail)
	{
		if (source is TableSource.Subquery { Alias: null, Lateral: false } subquery)
		{
			var query = (Query)new Query.Parenthesized(subquery.Query);

			if (operations is { Length: > 0 })
				query = Combined(query, operations);

			return new Held(Finished(null, query, order, tail), null);
		}

		return new Held(null, source);
	}

	/// <summary>
	/// Whether a parenthesis in a FROM may hold what it held: a query, with LATERAL or without; a join, or a parenthesis
	/// with no alias of its own, with no LATERAL before it — never a bare name, `FROM (a)`, nor an aliased subquery or
	/// parenthesis, `FROM ((SELECT 1) x)`, `FROM ((a JOIN b ON true) x) y`, as gram.y's table_ref has it.
	/// </summary>
	public static bool HeldFits(bool lateral, Held held, Alias? alias)
	{
		if (held.Query is not null)
			return true;

		return !lateral && held.Source is TableSource.Join or TableSource.Parenthesized { Alias: null };
	}

	public static TableSource HeldSource(bool lateral, Held held, Alias? alias)
	{
		if (held.Query is { } query)
			return new TableSource.Subquery(query) { Lateral = lateral, Alias = alias };

		return Bracketed(held.Source!, alias);
	}

	/// <summary>Whether every column of a definition list has its type, as ROWS FROM's items ask (TableFuncElementList); none at all fits.</summary>
	public static bool AllTyped(ColumnDef? first, ColumnDef[]? rest)
	{
		if (first is null)
			return true;

		if (first.Type is null)
			return false;

		if (rest is not null)
		{
			foreach (var column in rest)
			{
				if (column.Type is null)
					return false;
			}
		}

		return true;
	}

	/// <summary>The column definitions read, each with its type; null where none were written.</summary>
	public static SqlList<FieldDefinition>? Definitions(ColumnDef? first, ColumnDef[]? rest)
	{
		if (first is null)
			return null;

		var columns     = List(first, rest);
		var definitions = new FieldDefinition[columns.Count];

		for (var at = 0; at < definitions.Length; at++)
			definitions[at] = new FieldDefinition(columns[at].Name, columns[at].Type!);

		return SqlList.Own(definitions);
	}

	/// <summary>One UNION, EXCEPT or INTERSECT, its quantifier, and the operand after it.</summary>
	public static SetOperation Operation(string word, string? quantifier, Query operand)
	{
		var all  = quantifier is not null && (quantifier[0] | 0x20) == 'a';
		var kind = (word[0] | 0x20) switch
		{
			'u' => all ? SetOperator.UnionAll : SetOperator.Union,
			'e' => all ? SetOperator.ExceptAll : SetOperator.Except,
			_   => all ? SetOperator.IntersectAll : SetOperator.Intersect,
		};

		return new SetOperation(kind, operand) { Distinct = quantifier is not null && !all };
	}

	/// <summary>The first operand with the operations after it, as the standard's reading folds them.</summary>
	public static Query Combined(Query first, SetOperation[]? rest)
	{
		if (rest is not { Length: > 0 })
			return first;

		if (first.SetOperations.Count == 0)
			return first with { SetOperations = SqlList.Own(rest) };

		return first with { SetOperations = [.. first.SetOperations, .. rest] };
	}

	/// <summary>A table by name, with ONLY, the inheritance star and its alias.</summary>
	public static TableSource Table(QualifiedName name, bool only, bool star, Alias? alias)
	{
		return new TableSource.Named(name) { Only = only, Star = star, Alias = alias };
	}

	/// <summary>Whether OUTER stands where the server reads it: after LEFT, RIGHT or FULL.</summary>
	public static bool JoinWordsFit(string? kind, string? outer)
	{
		return outer is null || kind is not null && (kind[0] | 0x20) != 'i';
	}

	/// <summary>A join, as the standard's grammar makes one: the kind by its word, the specification after it.</summary>
	public static TableSource Joined(TableSource left, string? kind, bool natural, bool outer, TableSource right, JoinSpecification? specification)
	{
		return new TableSource.Join
		{
			Left          = left,
			Right         = right,
			Kind          = kind is null ? null : (kind[0] | 0x20) switch { 'c' => JoinKind.Cross, 'i' => JoinKind.Inner, 'l' => JoinKind.Left, 'r' => JoinKind.Right, _ => JoinKind.Full },
			Natural       = natural,
			OuterKeyword  = outer,
			Specification = specification,
		};
	}

	/// <summary>A bracketed join, and the alias after the bracket.</summary>
	public static TableSource Bracketed(TableSource inner, Alias? alias)
	{
		return new TableSource.Parenthesized(inner) { Alias = alias };
	}
}
