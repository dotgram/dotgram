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
		return new Expression.Parameter(ParameterKind.Positional) { Ordinal = int.Parse(text.AsSpan(1), CultureInfo.InvariantCulture) };
	}

	/// <summary>Whether a parameter's number is one the server can hold ("parameter number too large" otherwise).</summary>
	public static bool FitsParameter(string text)
	{
		return int.TryParse(text.AsSpan(1), NumberStyles.None, CultureInfo.InvariantCulture, out _);
	}

	/// <summary>An integer written in the text, as a number; null where none was written.</summary>
	public static int? Integer(string? text)
	{
		return text is null ? null : int.Parse(text.Replace("_", ""), CultureInfo.InvariantCulture);
	}

	/// <summary>A date, time or timestamp literal: the standard's record, the string from its first quote.</summary>
	public static LiteralValue DateTimeLiteral(string kind, string text)
	{
		var which = (kind[0] | 0x20) switch
		{
			'd' => DateTimeLiteralKind.Date,
			't' when kind.Length == 4 => DateTimeLiteralKind.Time,
			_   => DateTimeLiteralKind.Timestamp,
		};

		return new LiteralValue.DateTime(which, Quoted(text));
	}

	/// <summary>A typed literal, <c>t 'text'</c>: a cast of the string to the type, spelled as a typed literal.</summary>
	public static Expression TypedLiteral(DataType type, string text)
	{
		return new Expression.Cast(StringLiteral(text, StringLiteralKind.Character), type) { Spelling = CastSpelling.TypedLiteral };
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
			var length = modifiers.Count == 1 && modifiers[0] is LiteralValue.Numeric { Kind: NumericLiteralKind.DecimalInteger } n ? n.Text : null;
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

	/// <summary>An operator's symbol as a name of one part.</summary>
	public static QualifiedName OperatorName(string symbol)
	{
		return new QualifiedName(new Identifier(symbol));
	}

	/// <summary>An operator's symbol with the schemas written before it, <c>pg_catalog.+</c>.</summary>
	public static QualifiedName OperatorName(Identifier[]? schemas, string symbol)
	{
		return schemas is null ? OperatorName(symbol) : new QualifiedName(schemas[0], [.. schemas[1..], new Identifier(symbol)]);
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

	/// <summary>A sign before a value: the standard's unary node, and a signed number where the value is a bare number.</summary>
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
				Test.Normalized(var not, var form) => new Expression.IsNormalized(value, not, form is null ? null : Enum.Parse<NormalForm>(form.ToUpperInvariant())),
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

	/// <summary>A value in brackets, kept as the bracket it was.</summary>
	public static Expression Parenthesized(Expression inner)
	{
		return new Expression.Parenthesized(inner);
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
		public sealed record Typed(string Text) : FuncTail;
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
		public sealed record Typed(string Text) : CallRest;
		public sealed record Clauses(WithinGroupClause? Within, FilterClause? Filter, WindowReference? Over) : CallRest;
	}

	/// <summary>A name and what followed it: a call, a call turned into a typed literal, or a typed literal on the bare name.</summary>
	public static Expression Function(QualifiedName name, FuncTail tail)
	{
		switch (tail)
		{
			case FuncTail.Typed(var text):
				// `date '…'` is the standard's date literal (`date` is no key word of PostgreSQL's, and reaches here as a name).
				if (name.Count == 1 && name[0] is { Style: IdentifierStyle.Regular, UnicodeEscape: null } word && word.Text.Equals("date", StringComparison.OrdinalIgnoreCase))
					return DateTimeLiteral("DATE", text);

				return TypedLiteral(NamedType(name, []), text);

			case FuncTail.Call(var body, var rest):
				var arguments = body is CallBody.Arguments a ? a : null;
				var call = new Expression.Invocation(name, body is CallBody.Star ? [new Argument(new Expression.Asterisk())] : arguments!.List)
				{
					Quantifier = arguments?.Quantifier is { } quantifier ? (quantifier[0] | 0x20) == 'd' ? SetQuantifier.Distinct : SetQuantifier.All : null,
					OrderBy    = arguments?.Order,
				};

				switch (rest)
				{
					case CallRest.Typed(var text):
						// `numeric(10, 2) '1.5'`: the call's arguments are the type's modifiers.
						var modifiers = new Expression[call.Arguments.Count];

						for (var i = 0; i < modifiers.Length; i++)
							modifiers[i] = call.Arguments[i].Value;

						return TypedLiteral(NamedType(name, SqlList.Own(modifiers)), text);

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
			Field.Named(var word) => new Expression.Extract(Enum.Parse<ExtractField>(word, ignoreCase: true), source),
			Field.Other(var value) => new Expression.Extract(null, source) { NamedField = value },
			_ => throw new ArgumentOutOfRangeException(nameof(field)),
		};
	}

	public static Expression Normalized(Expression value, string? form)
	{
		return new Expression.Normalize(value, form is null ? null : Enum.Parse<NormalForm>(form.ToUpperInvariant()), null);
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
			Enum.Parse<DateTimeField>(start, ignoreCase: true),
			Integer(leadingPrecision),
			end is null ? null : Enum.Parse<DateTimeField>(end, ignoreCase: true),
			Integer(fractionalPrecision));
	}

	// ── Queries ────────────────────────────────────────────────────────────────

	/// <summary>What stood after SELECT: a quantifier, or the values of DISTINCT ON.</summary>
	public abstract record Distinct
	{
		public sealed record Of(SetQuantifier Quantifier) : Distinct;
		public sealed record On(Expression First, Expression[]? Rest) : Distinct;
	}

	public static Query Selected(Distinct? distinct, SelectColumn? first, SelectColumn[]? rest, FromClause? from, Expression? where)
	{
		var query = new Query.Select { Columns = List(first, rest), From = from, Where = where };

		return distinct switch
		{
			null => query,
			Distinct.Of(var quantifier) => query with { Quantifier = quantifier },
			Distinct.On(var head, var tail) => query with { Quantifier = SetQuantifier.Distinct, DistinctOn = List(head, tail) },
			_ => throw new ArgumentOutOfRangeException(nameof(distinct)),
		};
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
