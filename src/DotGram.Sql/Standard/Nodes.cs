using System;
using System.Collections.Generic;

using DotGram.Sql.Ast;

namespace DotGram.Sql.Standard;

// Inside the namespace, where they are asked before DotGram.Sql's own Expression and Statement — the
// tree T-SQL builds, which a using directive above the namespace would lose to.
using Expression = DotGram.Sql.Ast.Expression;
using Statement = DotGram.Sql.Ast.Statement;

/// <summary>
/// How <c>SqlStandard.gram</c> makes the SQL:2023 tree out of what it read: the words and lists a
/// rule captured, turned into nodes (docs/design/sql-ast.md).
/// </summary>
static class Nodes
{
	// ── Names ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// A Unicode delimited identifier, <c>U&amp;"a"</c> or <c>U&amp;"a!0041" UESCAPE '!'</c>: the name as written
	/// up to its closing quote, and the escape character where one was named.
	/// </summary>
	public static Identifier UnicodeIdentifier(string text)
	{
		var close = text.LastIndexOf('"');
		var name  = text.Substring(0, close + 1);
		var rest  = text.Substring(close + 1);
		var quote = rest.IndexOf('\'');

		return new Identifier(name, IdentifierStyle.UnicodeDelimited, quote >= 0 && quote + 1 < rest.Length ? rest[quote + 1] : null);
	}

	/// <summary>A name made of a word read as a key word, <c>MODULE</c>, and the names after it.</summary>
	public static QualifiedName Named(string word, params Identifier[] rest)
	{
		var parts = new Identifier[rest.Length + 1];

		parts[0] = new Identifier(word);
		Array.Copy(rest, 0, parts, 1, rest.Length);

		return new QualifiedName(parts);
	}

	/// <summary>A name and the parts after it, as many as were read.</summary>
	public static QualifiedName Chain(Identifier first, Identifier[]? rest)
	{
		if (rest is not { Length: > 0 })
			return new QualifiedName([first]);

		var parts = new Identifier[rest.Length + 1];

		parts[0] = first;
		Array.Copy(rest, 0, parts, 1, rest.Length);

		return new QualifiedName(parts);
	}

	/// <summary>A name and one optional part after it.</summary>
	public static QualifiedName Chain(Identifier first, Identifier? second) =>
		second is null ? new QualifiedName([first]) : new QualifiedName([first, second]);

	/// <summary>A character set's name: its qualifiers, and the SQL language identifier after them.</summary>
	public static CharacterSetName CharacterSet(Identifier[]? qualifiers, string name)
	{
		var parts = new Identifier[(qualifiers?.Length ?? 0) + 1];

		qualifiers?.CopyTo(parts, 0);
		parts[parts.Length - 1] = new Identifier(name);

		return new CharacterSetName(new QualifiedName(parts));
	}

	// ── What is not built yet ──────────────────────────────────────────────────

	/// <summary>
	/// What a chapter not yet building its tree stands in for: the window and JSON functions, a JSON
	/// subscript, a value function. Each goes as its chapter builds, and none is left when all have.
	/// </summary>
	public static Expression Unbuilt() => new Expression.Extension("SQL:2023", "Unbuilt", []);

	/// <summary>The query a subquery holds, until §7 builds its tree.</summary>
	public static Statement.Select UnbuiltQuery() => new();

	// ── §6.3 Value expression primary ──────────────────────────────────────────

	public static Expression Null() => new Expression.Literal(new LiteralValue.Null());

	/// <summary>A bracket's value expression and what followed it: brackets, a row, or a generalized invocation.</summary>
	public static Towers.Typed Bracketed(Towers.Typed value, Towers.Bracket tail) =>
		tail.Kind switch
		{
			Towers.Parenthesized => new(new Expression.Parenthesized(value.Node), Towers.Value | Towers.Parenthesized | (value.Roles & Towers.Truth)),
			Towers.Row           => new(new Expression.Row(List(value.Node, tail.Rest)), Towers.Row),
			_                    => new(new Expression.Member(new Expression.Generalized(value.Node, tail.Type!), MemberAccessKind.Dot, tail.Method!, tail.Arguments), Towers.Value | Towers.Truth | Towers.Bare),
		};

	/// <summary>A name and what followed it: a row pattern measure called over a window, or a chain.</summary>
	public static Towers.Typed ChainOrMeasure(Identifier name, Towers.Chained chained) =>
		chained.Measure
			? new(new Expression.Invocation(new QualifiedName([name]), []) { WithoutParentheses = true, Over = chained.Over }, Towers.Value | Towers.Truth | Towers.Bare)
			: new(new Expression.Reference(Chain(name, chained.Rest)), Towers.Value | Towers.Truth | Towers.Bare | Towers.Chain);

	/// <summary>`.SPECIFICTYPE`, with its brackets where they were written.</summary>
	public static Expression.Member SpecificType(string word, string? brackets) =>
		new(null!, MemberAccessKind.Dot, new Identifier(word), brackets is null ? null : []);

	/// <summary>A JSON item method: its name, and the numbers in its brackets.</summary>
	public static Expression Method(string word, string? first, string? second)
	{
		var arguments = new List<Argument>();

		if (first is not null)
			arguments.Add(new Argument(new Expression.Literal(NumericLiteral(first))));

		if (second is not null)
			arguments.Add(new Argument(new Expression.Literal(NumericLiteral(second))));

		return new Expression.Member(null!, MemberAccessKind.Dot, new Identifier(word), arguments);
	}

	/// <summary>A host parameter, `:a INDICATOR :i`, and whether the key word stood before its indicator.</summary>
	public static Expression HostParameter(Identifier name, Identifier? indicator, bool keyword) =>
		new Expression.Parameter(ParameterKind.Host, name) { Indicator = indicator, IndicatorKeyword = indicator is not null && keyword };

	public static Expression Current(string word) =>
		new Expression.Current(word.ToUpperInvariant() switch
		{
			"CURRENT_CATALOG"                 => CurrentValue.Catalog,
			"CURRENT_DEFAULT_TRANSFORM_GROUP" => CurrentValue.DefaultTransformGroup,
			"CURRENT_PATH"                    => CurrentValue.Path,
			"CURRENT_ROLE"                    => CurrentValue.Role,
			"CURRENT_SCHEMA"                  => CurrentValue.Schema,
			"CURRENT_USER"                    => CurrentValue.CurrentUser,
			"SESSION_USER"                    => CurrentValue.SessionUser,
			"SYSTEM_USER"                     => CurrentValue.SystemUser,
			"USER"                            => CurrentValue.User,
			_                                 => CurrentValue.Value,
		});

	/// <summary>A function the BNF spells out whose arguments are a comma list: its name as written, and the values.</summary>
	public static Expression.Invocation Invoked(string word, Expression first, Expression[]? rest)
	{
		var arguments = new Argument[(rest?.Length ?? 0) + 1];

		arguments[0] = new Argument(first);

		for (var at = 1; at < arguments.Length; at++)
			arguments[at] = new Argument(rest![at - 1]);

		return new Expression.Invocation(new QualifiedName([new Identifier(word)]), arguments);
	}

	/// <summary>An array or a multiset by enumeration, empty where nothing was written, and whether its brackets were trigraphs.</summary>
	public static Expression Collection(string word, string bracket, Expression? first, Expression[]? rest)
	{
		IReadOnlyList<Expression> items = first is null ? [] : List(first, rest);
		var trigraphs = bracket == "??(";

		return (word[0] | 0x20) == 'a' ? new Expression.Array(items, trigraphs) : new Expression.Multiset(items, trigraphs);
	}

	public static CollectionKind CollectionKindOf(string word) =>
		(word[0] | 0x20) switch
		{
			'a' => CollectionKind.Array,
			'm' => CollectionKind.Multiset,
			_   => CollectionKind.Table,
		};

	/// <summary>An SQL argument list: nothing, or the first argument and the rest.</summary>
	public static IReadOnlyList<Argument> Arguments(Argument? first, Argument[]? rest) =>
		first is null ? [] : List(first, rest);

	/// <summary>Values read with their towers, as the nodes alone.</summary>
	public static IReadOnlyList<Expression> Values(Towers.Typed first, Towers.Typed[]? rest)
	{
		var all = new Expression[(rest?.Length ?? 0) + 1];

		all[0] = first.Node;

		for (var at = 1; at < all.Length; at++)
			all[at] = rest![at - 1].Node;

		return all;
	}

	// ── §6.28 Operators ────────────────────────────────────────────────────────

	public static Ast.MultisetOperator Multiset(string word) =>
		(word[0] | 0x20) switch
		{
			'u' => Ast.MultisetOperator.Union,
			'e' => Ast.MultisetOperator.Except,
			_   => Ast.MultisetOperator.Intersect,
		};

	public static SetQuantifier? Quantifier(string? word) =>
		word is null ? null : (word[0] | 0x20) == 'a' ? SetQuantifier.All : SetQuantifier.Distinct;

	// ── §8 Predicates ──────────────────────────────────────────────────────────

	/// <summary>What `IS [NOT] TRUE` said: whether it was written, and what it tests.</summary>
	public readonly record struct Truth(bool Tested, bool Not, Ast.BooleanLiteral Value);

	/// <summary>Whether a truth test was written, asked by a guard, which may see a nullable struct (Towers.RolesOf).</summary>
	public static bool TestedOf(Truth? truth) => truth?.Tested ?? false;

	public static Ast.BooleanLiteral Truthful(string word) =>
		(word[0] | 0x20) switch
		{
			't' => Ast.BooleanLiteral.True,
			'f' => Ast.BooleanLiteral.False,
			_   => Ast.BooleanLiteral.Unknown,
		};

	/// <summary>A predicate's second part with the predicand it follows put in: the one reading of the row, and the predicate it turned out to be.</summary>
	public static Expression Predicated(Expression left, Expression tail) =>
		tail switch
		{
			Expression.Comparison c           => c with { Left = left },
			Expression.QuantifiedComparison q => q with { Left = left },
			Expression.Between b              => b with { Value = left },
			Expression.In i                   => i with { Value = left },
			Expression.Like l                 => l with { Value = left },
			Expression.IsNull n               => n with { Value = left },
			Expression.IsDistinct d           => d with { Left = left },
			Expression.IsNormalized n         => n with { Value = left },
			Expression.MemberOf m             => m with { Value = left },
			Expression.SubmultisetOf s        => s with { Value = left },
			Expression.IsSet s                => s with { Value = left },
			Expression.IsOf o                 => o with { Value = left },
			Expression.Match m                => m with { Value = left },
			Expression.Overlaps o             => o with { Left = left },
			Expression.JsonPredicate j        => j with { Value = left },
			Expression.PeriodPredicate p      => p with { Left = left is Expression.Reference r ? new PeriodValue.Reference(r.Name) : throw new ArgumentOutOfRangeException(nameof(left), left, "A period predicate names its period by a chain.") },
			_                                 => throw new ArgumentOutOfRangeException(nameof(tail), tail, "A predicate this method cannot complete."),
		};

	/// <summary>A predicate with `NOT` in it, where it was written.</summary>
	public static Expression Negated(Expression predicate, bool not) =>
		!not ? predicate : predicate switch
		{
			Expression.Between b       => b with { Not = true },
			Expression.In i            => i with { Not = true },
			Expression.Like l          => l with { Not = true },
			Expression.IsNull n        => n with { Not = true },
			Expression.IsDistinct d    => d with { Not = true },
			Expression.IsNormalized n  => n with { Not = true },
			Expression.MemberOf m      => m with { Not = true },
			Expression.SubmultisetOf s => s with { Not = true },
			Expression.IsSet s         => s with { Not = true },
			Expression.IsOf o          => o with { Not = true },
			Expression.JsonPredicate j => j with { Not = true },
			_                          => throw new ArgumentOutOfRangeException(nameof(predicate), predicate, "A predicate that takes no NOT."),
		};

	/// <summary>A comparison, or a quantified one, with the operator written.</summary>
	public static Expression Compared(Expression tail, string op)
	{
		var compared = op switch
		{
			"<>" => ComparisonOperator.NotEqual,
			"<=" => ComparisonOperator.LessOrEqual,
			">=" => ComparisonOperator.GreaterOrEqual,
			"="  => ComparisonOperator.Equal,
			"<"  => ComparisonOperator.Less,
			_    => ComparisonOperator.Greater,
		};

		return tail is Expression.QuantifiedComparison q ? q with { Operator = compared } : ((Expression.Comparison)tail) with { Operator = compared };
	}

	public static Ast.Quantifier Quantified(string word) =>
		(word[0] | 0x20) switch
		{
			's'                              => Ast.Quantifier.Some,
			'a' when (word[1] | 0x20) == 'l' => Ast.Quantifier.All,
			_                                => Ast.Quantifier.Any,
		};

	public static Ast.MatchType? Matching(string? word) =>
		word is null ? null : (word[0] | 0x20) switch
		{
			's' => Ast.MatchType.Simple,
			'p' => Ast.MatchType.Partial,
			_   => Ast.MatchType.Full,
		};

	/// <summary>`UNIQUE NULLS [NOT] DISTINCT`: null where nothing was said.</summary>
	public static NullDistinctness? Nulls(string? distinct, string? not) =>
		distinct is null ? null : not is null ? NullDistinctness.Distinct : NullDistinctness.NotDistinct;

	public static BetweenSymmetry? Symmetry(string? word) =>
		word is null ? null : (word[0] | 0x20) == 'a' ? BetweenSymmetry.Asymmetric : BetweenSymmetry.Symmetric;

	public static Expression Membership(string word, bool of, Expression collection) =>
		(word[0] | 0x20) == 'm'
			? new Expression.MemberOf(null!, false, of, collection)
			: new Expression.SubmultisetOf(null!, false, of, collection);

	public static NormalForm? Form(string? word) =>
		word?.ToUpperInvariant() switch
		{
			null   => null,
			"NFC"  => NormalForm.NFC,
			"NFD"  => NormalForm.NFD,
			"NFKC" => NormalForm.NFKC,
			_      => NormalForm.NFKD,
		};

	public static JsonPredicateType? JsonType(string? word) =>
		word?.ToUpperInvariant() switch
		{
			null     => null,
			"VALUE"  => JsonPredicateType.Value,
			"ARRAY"  => JsonPredicateType.Array,
			"OBJECT" => JsonPredicateType.Object,
			_        => JsonPredicateType.Scalar,
		};

	/// <summary>`WITH UNIQUE KEYS` and its three fellows, from the words written: null where none were.</summary>
	public static JsonKeyUniqueness? Uniqueness(string? words)
	{
		if (words is null)
			return null;

		var without = words.TrimStart().StartsWith("WITHOUT", StringComparison.OrdinalIgnoreCase);
		var keys    = words.TrimEnd().EndsWith("KEYS", StringComparison.OrdinalIgnoreCase);

		return without
			? keys ? JsonKeyUniqueness.WithoutUniqueKeys : JsonKeyUniqueness.WithoutUnique
			: keys ? JsonKeyUniqueness.WithUniqueKeys : JsonKeyUniqueness.WithUnique;
	}

	public static JsonEncoding? Encoding(string? word) =>
		word?.ToUpperInvariant() switch
		{
			null    => null,
			"UTF8"  => JsonEncoding.Utf8,
			"UTF16" => JsonEncoding.Utf16,
			_       => JsonEncoding.Utf32,
		};

	public static PeriodOperator Period(string word, bool immediately) =>
		word.ToUpperInvariant() switch
		{
			"OVERLAPS"                  => PeriodOperator.Overlaps,
			"EQUALS"                    => PeriodOperator.Equals,
			"PRECEDES" when immediately => PeriodOperator.ImmediatelyPrecedes,
			"PRECEDES"                  => PeriodOperator.Precedes,
			_ when immediately          => PeriodOperator.ImmediatelySucceeds,
			_                           => PeriodOperator.Succeeds,
		};

	// ── §6.30–6.36 Value functions ─────────────────────────────────────────────

	/// <summary>A name read as a key word, as the one part of a routine's name.</summary>
	public static QualifiedName Name(string word) => new([new Identifier(word)]);

	/// <summary>A function the BNF spells out whose arguments are a comma list: its name as written, and the arguments written, in order.</summary>
	public static Expression.Invocation Call(string word, params Expression?[] arguments)
	{
		var written = new List<Argument>(arguments.Length);

		foreach (var argument in arguments)
			if (argument is not null)
				written.Add(new Argument(argument));

		return new Expression.Invocation(Name(word), written);
	}

	/// <summary>What a regular expression function searches: a pattern, its flags, the string, where to start, and the units.</summary>
	public sealed record Search(Expression Pattern, Expression? Flag, Expression Value, Expression? From, CharacterLengthUnits? Using);

	public static Expression.Regex Regex(RegexFunction function, Search search) =>
		new(function, search.Pattern, search.Value) { Flag = search.Flag, From = search.From, Using = search.Using };

	public static RegexPositionStartOrAfter? StartOrAfter(string? word) =>
		word is null ? null : (word[0] | 0x20) == 's' ? RegexPositionStartOrAfter.Start : RegexPositionStartOrAfter.After;

	public static LengthFunction LengthFunctionOf(string word) =>
		word.Length > "CHAR_LENGTH".Length ? LengthFunction.CharacterLength : LengthFunction.CharLength;

	public static int UnitCode(string units) => (units[0] | 0x20) == 'c' ? 1 : 2;

	public static CharacterLengthUnits? UnitsOf(int code) =>
		code switch
		{
			1 => CharacterLengthUnits.Characters,
			2 => CharacterLengthUnits.Octets,
			_ => null,
		};

	public static CharacterLengthUnits? UnitsOf(string? units) => units is null ? null : UnitsOf(UnitCode(units));

	public static ExtractField ExtractFieldOf(string word) =>
		word.ToUpperInvariant() switch
		{
			"YEAR"            => Ast.ExtractField.Year,
			"MONTH"           => Ast.ExtractField.Month,
			"DAY"             => Ast.ExtractField.Day,
			"HOUR"            => Ast.ExtractField.Hour,
			"MINUTE"          => Ast.ExtractField.Minute,
			"SECOND"          => Ast.ExtractField.Second,
			"TIMEZONE_HOUR"   => Ast.ExtractField.TimezoneHour,
			_                 => Ast.ExtractField.TimezoneMinute,
		};

	public static TranslateFunction TranslateOf(string word) =>
		(word[0] | 0x20) == 'c' ? TranslateFunction.Convert : TranslateFunction.Translate;

	/// <summary>`NORMALIZE`: the value, the form, and the result's length with its multiplier and units.</summary>
	public static Expression Normalized(Expression value, string? form, Length? length)
	{
		var size = length?.LargeObject;

		return new Expression.Normalize(value, Form(form), size is null ? null : new Expression.Literal(NumericLiteral(size.Value.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))))
		{
			MaxLengthMultiplier = size?.Multiplier,
			MaxLengthUnit       = length?.Unit,
		};
	}

	/// <summary>What follows a substring's string: `FROM`, `FOR` and units (kind 0, or 1 with units), or `SIMILAR` and `ESCAPE` (kind 2).</summary>
	public sealed record SubstringParts(int Kind, Expression? From, Expression? For, CharacterLengthUnits? Using, Expression? Pattern, Expression? Escape);

	/// <summary>A character substring, or a binary one where no units are named; `SIMILAR` a character one's alone.</summary>
	public static bool SubstringFits(Towers.Typed? value, SubstringParts? tail)
	{
		var roles = Towers.RolesOf(value);
		var kind  = tail?.Kind ?? 0;

		return kind == 2 ? (roles & Towers.Character) != 0 : Towers.Characters(roles, kind == 1);
	}

	public static Expression Substring(Expression value, SubstringParts tail) =>
		tail.Kind == 2
			? new Expression.SubstringSimilar(value, tail.Pattern!, tail.Escape!)
			: new Expression.Substring(value, tail.From!, tail.For, tail.Using);

	/// <summary>A trim's operands as read, and what they can be: the towers of the character and the source together.</summary>
	public sealed record TrimParts(int Roles, TrimSpecification? Specification, Expression? Character, bool From, Expression Source);

	public static int RolesOf(TrimParts? parts) => parts?.Roles ?? 0;

	public static TrimSpecification TrimSpecificationOf(string word) =>
		(word[0] | 0x20) switch
		{
			'l' => TrimSpecification.Leading,
			't' => TrimSpecification.Trailing,
			_   => TrimSpecification.Both,
		};

	public static Expression Trimmed(TrimParts parts) =>
		new Expression.Trim(parts.Specification, parts.Character, parts.From, parts.Source);

	public static Expression CurrentTime(string word, string? precision) =>
		new Expression.Current(
			word.ToUpperInvariant() switch
			{
				"CURRENT_TIMESTAMP" => CurrentValue.Timestamp,
				"CURRENT_TIME"      => CurrentValue.Time,
				"LOCALTIMESTAMP"    => CurrentValue.LocalTimestamp,
				_                   => CurrentValue.LocalTime,
			},
			null,
			precision is null ? null : new Expression.Literal(NumericLiteral(precision)));

	// ── §6.10 Window functions, §10.9 Aggregates ───────────────────────────────

	/// <summary>An aggregate with `RUNNING` or `FINAL` before it.</summary>
	public static Expression WithSemantics(Expression aggregate, string word) =>
		aggregate switch
		{
			Expression.Invocation i         => i with { Semantics = SemanticsOf(word) },
			Expression.JsonArrayAggregate a  => a with { Semantics = SemanticsOf(word) },
			Expression.JsonObjectAggregate o => o with { Semantics = SemanticsOf(word) },
			_                                => throw new ArgumentOutOfRangeException(nameof(aggregate), aggregate, "An aggregate this method does not know."),
		};

	/// <summary>An aggregate with the window it is computed over, where one was named.</summary>
	public static Expression WithOver(Expression aggregate, WindowReference? over) =>
		over is null ? aggregate : aggregate switch
		{
			Expression.Invocation i         => i with { Over = over },
			Expression.JsonArrayAggregate a  => a with { Over = over },
			Expression.JsonObjectAggregate o => o with { Over = over },
			_                                => throw new ArgumentOutOfRangeException(nameof(aggregate), aggregate, "An aggregate this method does not know."),
		};

	/// <summary>An aggregate with its `FILTER`.</summary>
	public static Expression Filtered(Expression aggregate, FilterClause filter) =>
		aggregate switch
		{
			Expression.Invocation i         => i with { Filter = filter },
			Expression.JsonArrayAggregate a  => a with { Filter = filter },
			Expression.JsonObjectAggregate o => o with { Filter = filter },
			_                                => throw new ArgumentOutOfRangeException(nameof(aggregate), aggregate, "An aggregate this method does not know."),
		};

	public static RowPatternSemantics SemanticsOf(string word) =>
		(word[0] | 0x20) == 'r' ? RowPatternSemantics.Running : RowPatternSemantics.Final;

	public static NullTreatment? NullTreatmentOf(string? words) =>
		words is null ? null : (words.TrimStart()[0] | 0x20) == 'r' ? NullTreatment.RespectNulls : NullTreatment.IgnoreNulls;

	public static FromFirstOrLast? FirstOrLast(string? word) =>
		word is null ? null : (word[0] | 0x20) == 'f' ? FromFirstOrLast.First : FromFirstOrLast.Last;

	/// <summary>A row marker, and the sign and delta after it where they were written.</summary>
	public static Expression Marked(string word, string? sign, Expression? delta) =>
		new Expression.RowMarker(
			word.ToUpperInvariant() switch
			{
				"BEGIN_PARTITION" => RowMarkerKind.BeginPartition,
				"BEGIN_FRAME"     => RowMarkerKind.BeginFrame,
				"CURRENT_ROW"     => RowMarkerKind.CurrentRow,
				"FRAME_ROW"       => RowMarkerKind.FrameRow,
				"END_FRAME"       => RowMarkerKind.EndFrame,
				_                 => RowMarkerKind.EndPartition,
			},
			SignOf(sign), delta);

	/// <summary>`ON OVERFLOW TRUNCATE`: the filler where one was written, and `WITH COUNT` or `WITHOUT COUNT`.</summary>
	public static ListaggOverflow Truncated(string? filler, string count) =>
		new(true, filler is null ? null : new Expression.Literal(StringLiteral(filler, StringLiteralKind.Character)), count.Length == 4);

	// ── JSON ───────────────────────────────────────────────────────────────────

	/// <summary>A list whose first element may not have been written, and then none was.</summary>
	public static IReadOnlyList<T> ListOrEmpty<T>(T? first, T[]? rest) where T : class =>
		first is null ? [] : List(first, rest);

	public static JsonNullHandling? NullHandling(string? words) =>
		words is null ? null : (words.TrimStart()[0] | 0x20) == 'n' ? JsonNullHandling.NullOnNull : JsonNullHandling.AbsentOnNull;

	/// <summary>A wrapper behavior from its words: `WITHOUT [ARRAY]`, `WITH [CONDITIONAL | UNCONDITIONAL] [ARRAY]`.</summary>
	public static JsonWrapperBehavior? Wrapper(string? words)
	{
		if (words is null)
			return null;

		var upper = words.ToUpperInvariant();
		var array = upper.TrimEnd().EndsWith("ARRAY", StringComparison.Ordinal);

		if (upper.TrimStart().StartsWith("WITHOUT", StringComparison.Ordinal))
			return array ? JsonWrapperBehavior.WithoutArray : JsonWrapperBehavior.Without;

		if (upper.Contains("UNCONDITIONAL"))
			return array ? JsonWrapperBehavior.WithUnconditionalArray : JsonWrapperBehavior.WithUnconditional;

		if (upper.Contains("CONDITIONAL"))
			return array ? JsonWrapperBehavior.WithConditionalArray : JsonWrapperBehavior.WithConditional;

		return array ? JsonWrapperBehavior.WithArray : JsonWrapperBehavior.With;
	}

	public static JsonQueryBehavior? QueryBehavior(string? words)
	{
		if (words is null)
			return null;

		var upper = words.ToUpperInvariant().Trim();

		return upper.StartsWith("ERROR", StringComparison.Ordinal) ? JsonQueryBehavior.Error
			: upper.StartsWith("NULL", StringComparison.Ordinal) ? JsonQueryBehavior.Null
			: upper.EndsWith("ARRAY", StringComparison.Ordinal) ? JsonQueryBehavior.EmptyArray
			: JsonQueryBehavior.EmptyObject;
	}

	public static JsonExistsErrorBehavior? ExistsBehavior(string? word) =>
		word?.ToUpperInvariant() switch
		{
			null      => null,
			"TRUE"    => JsonExistsErrorBehavior.True,
			"FALSE"   => JsonExistsErrorBehavior.False,
			"UNKNOWN" => JsonExistsErrorBehavior.Unknown,
			_         => JsonExistsErrorBehavior.Error,
		};

	// ── §6.39 The path language ────────────────────────────────────────────────

	/// <summary>A path operator and the operand after it.</summary>
	public sealed record PathOperated(string Operator, JsonPathExpression Operand);

	public static JsonPathExpression PathFold(JsonPathExpression first, PathOperated[]? rest)
	{
		foreach (var (op, operand) in rest ?? [])
			first = new JsonPathExpression.Binary(first, op switch
			{
				"+" => JsonPathBinaryOperator.Add,
				"-" => JsonPathBinaryOperator.Subtract,
				"*" => JsonPathBinaryOperator.Multiply,
				"/" => JsonPathBinaryOperator.Divide,
				_   => JsonPathBinaryOperator.Modulo,
			}, operand);

		return first;
	}

	/// <summary>Signs before a path accessor, the first written outermost.</summary>
	public static JsonPathExpression PathSigned(string? signs, JsonPathExpression operand)
	{
		for (var at = (signs?.Length ?? 0) - 1; at >= 0; at--)
			operand = new JsonPathExpression.Unary(signs![at] == '-' ? JsonPathUnaryOperator.Minus : JsonPathUnaryOperator.Plus, operand);

		return operand;
	}

	public static JsonPathExpression PathAccessed(JsonPathExpression primary, JsonPathAccessor[]? accessors)
	{
		foreach (var accessor in accessors ?? [])
			primary = new JsonPathExpression.Access(primary, accessor);

		return primary;
	}

	/// <summary>An item method in the path language, from the method the SQL accessor reads it as.</summary>
	public static JsonMethod PathMethod(Expression method)
	{
		var member    = (Expression.Member)method;
		var arguments = member.Arguments ?? [];
		int? First() => arguments.Count > 0 ? Integer(((LiteralValue.Numeric)((Expression.Literal)arguments[0].Value).Value).Text) : null;
		int? Second() => arguments.Count > 1 ? Integer(((LiteralValue.Numeric)((Expression.Literal)arguments[1].Value).Value).Text) : null;

		return member.Name.Text.ToUpperInvariant() switch
		{
			"TYPE"         => new JsonMethod(JsonMethodKind.Type),
			"SIZE"         => new JsonMethod(JsonMethodKind.Size),
			"DOUBLE"       => new JsonMethod(JsonMethodKind.Double),
			"CEILING"      => new JsonMethod(JsonMethodKind.Ceiling),
			"FLOOR"        => new JsonMethod(JsonMethodKind.Floor),
			"ABS"          => new JsonMethod(JsonMethodKind.Abs),
			"KEYVALUE"     => new JsonMethod(JsonMethodKind.KeyValue),
			"BIGINT"       => new JsonMethod(JsonMethodKind.BigInt),
			"BOOLEAN"      => new JsonMethod(JsonMethodKind.Boolean),
			"DATETIME"     => new JsonMethod(JsonMethodKind.DateTime),
			"DATE"         => new JsonMethod(JsonMethodKind.Date),
			"INTEGER"      => new JsonMethod(JsonMethodKind.Integer),
			"NUMBER"       => new JsonMethod(JsonMethodKind.Number),
			"STRING"       => new JsonMethod(JsonMethodKind.String),
			"DECIMAL"      => new JsonMethod(JsonMethodKind.Decimal, First(), Second()),
			"TIMESTAMP_TZ" => new JsonMethod(JsonMethodKind.TimestampTz, First()),
			"TIMESTAMP"    => new JsonMethod(JsonMethodKind.Timestamp, First()),
			"TIME_TZ"      => new JsonMethod(JsonMethodKind.TimeTz, First()),
			_              => new JsonMethod(JsonMethodKind.Time, First()),
		};
	}

	public static JsonPathPredicate PathOr(JsonPathPredicate first, JsonPathPredicate[]? rest) =>
		rest is not { Length: > 0 } ? first : new JsonPathPredicate.Or(List(first, rest));

	public static JsonPathComparisonOperator PathComparison(string op) =>
		op switch
		{
			"<>" => JsonPathComparisonOperator.NotEqual,
			"<=" => JsonPathComparisonOperator.LessOrEqual,
			">=" => JsonPathComparisonOperator.GreaterOrEqual,
			"<"  => JsonPathComparisonOperator.Less,
			_    => JsonPathComparisonOperator.Greater,
		};

	// ── §7.6 Row pattern recognition, §7.11 Windows, §10.10 Sort specifications ─

	/// <summary>`MATCH_RECOGNIZE`: what its common syntax said, with its partitioning, order, measures and rows per match.</summary>
	public static RowPatternClause Recognized(IReadOnlyList<Expression>? partition, OrderByClause? order, IReadOnlyList<RowPatternMeasure>? measures, RowsPerMatch? rows, RowPatternClause common) =>
		common with { PartitionBy = partition ?? [], OrderBy = order, Measures = measures ?? [], RowsPerMatch = rows };

	public static RowsPerMatch AllRows(string? handling) =>
		handling is null ? RowsPerMatch.All : (handling.TrimStart()[0] | 0x20) switch
		{
			's' => RowsPerMatch.AllShowEmpty,
			'o' => RowsPerMatch.AllOmitEmpty,
			_   => RowsPerMatch.AllWithUnmatched,
		};

	/// <summary>The common syntax of a row pattern: the skip, `INITIAL` or `SEEK`, the pattern, its subsets and its definitions.</summary>
	public static RowPatternClause Common(RowPatternSkip? skip, string? initial, RowPattern pattern, IReadOnlyList<RowPatternSubset>? subsets, RowPatternDefinition first, RowPatternDefinition[]? rest) =>
		new([], null, [], null, skip, initial is null ? null : (initial[0] | 0x20) == 'i' ? RowPatternInitial.Initial : RowPatternInitial.Seek, pattern, subsets ?? [], List(first, rest));

	public static RowPattern Alternation(RowPattern first, RowPattern[]? rest) =>
		rest is not { Length: > 0 } ? first : new RowPattern.Alternation(List(first, rest));

	public static RowPattern Sequence(RowPattern first, RowPattern[]? rest) =>
		rest is not { Length: > 0 } ? first : new RowPattern.Sequence(List(first, rest));

	/// <summary>A window frame: the measures before it, its units, extent and exclusion, and the row pattern after it.</summary>
	public static WindowFrame Framed(IReadOnlyList<RowPatternMeasure>? measures, string units, WindowFrameExtent extent, WindowFrameExclusion? exclusion, RowPatternClause? common)
	{
		var unit = (units[0] | 0x20) switch
		{
			'r' when (units[1] | 0x20) == 'o' => WindowFrameUnit.Rows,
			'r'                               => WindowFrameUnit.Range,
			_                                 => WindowFrameUnit.Groups,
		};

		var pattern = common is null && measures is null ? null : (common ?? new RowPatternClause([], null, [], null, null, null, null, [], [])) with { Measures = measures ?? [] };

		return new WindowFrame(unit, extent, exclusion, pattern);
	}

	public static WindowFrameExclusion Exclusion(string words) =>
		(words.TrimStart()[0] | 0x20) switch
		{
			'c' => WindowFrameExclusion.CurrentRow,
			'g' => WindowFrameExclusion.Group,
			't' => WindowFrameExclusion.Ties,
			_   => WindowFrameExclusion.NoOthers,
		};

	public static SortItem Sorted(Expression key, string? direction, string? nulls) =>
		new(key,
			direction is null ? null : (direction[0] | 0x20) == 'a' ? SortDirection.Asc : SortDirection.Desc,
			nulls is null ? null : (nulls[0] | 0x20) == 'f' ? NullOrdering.First : NullOrdering.Last);

	// ── Lists ──────────────────────────────────────────────────────────────────

	/// <summary>The first of a list and the rest of it, as one list in the order written.</summary>
	public static IReadOnlyList<T> List<T>(T first, T[]? rest)
	{
		if (rest is not { Length: > 0 })
			return [first];

		var all = new T[rest.Length + 1];

		all[0] = first;
		Array.Copy(rest, 0, all, 1, rest.Length);

		return all;
	}

	// ── §5.3 Literals ──────────────────────────────────────────────────────────

	/// <summary>A number as written, its sign in front where one was, and which of the numeric literals it is.</summary>
	public static LiteralValue NumericLiteral(string text)
	{
		var body = text.Length > 0 && (text[0] == '+' || text[0] == '-') ? text.Substring(1) : text;
		var kind =
			body.Length > 1 && body[0] == '0' && (body[1] | 0x20) == 'x' ? NumericLiteralKind.HexInteger :
			body.Length > 1 && body[0] == '0' && (body[1] | 0x20) == 'o' ? NumericLiteralKind.OctalInteger :
			body.Length > 1 && body[0] == '0' && (body[1] | 0x20) == 'b' ? NumericLiteralKind.BinaryInteger :
			body.IndexOf('e') >= 0 || body.IndexOf('E') >= 0 ? NumericLiteralKind.Approximate :
			body.IndexOf('.') >= 0 ? NumericLiteralKind.Decimal :
			NumericLiteralKind.DecimalInteger;

		return new LiteralValue.Numeric(text, kind);
	}

	/// <summary>
	/// A character string literal of any kind: its introducer's character set, what was written from
	/// its first quote to its last, and a Unicode literal's escape character.
	/// </summary>
	public static LiteralValue StringLiteral(string text, StringLiteralKind kind)
	{
		var quote = text.IndexOf('\'');
		var start = text[0] == '_' ? 1 : -1;

		CharacterSetName? characterSet = null;

		if (start == 1)
		{
			var introduced = text.Substring(1, quote - 1);

			if (kind == StringLiteralKind.Unicode)
				introduced = introduced.Substring(0, introduced.Length - 2);

			characterSet = CharacterSet(introduced);
		}

		var escaped = kind == StringLiteralKind.Unicode && text.EndsWith(DefaultEscape, StringComparison.OrdinalIgnoreCase);
		var end     = escaped ? text.Length - DefaultEscape.Length : text.Length;
		var literal = text.Substring(quote, end - quote);

		return new LiteralValue.String(literal, kind, characterSet, escaped ? '\\' : null);
	}

	/// <summary>
	/// The one escape specifier the BNF reads, a part of the token with nothing inside it: the Syntax
	/// Rules choose another character, and a separator ends the token.
	/// </summary>
	const string DefaultEscape = "UESCAPE'\\'";

	/// <summary>A binary string literal: what was written from its first quote to its last.</summary>
	public static LiteralValue BinaryLiteral(string text) =>
		new LiteralValue.Binary(text.Substring(text.IndexOf('\'')));

	public static LiteralValue BooleanLiteral(string word) =>
		new LiteralValue.Boolean((word[0] | 0x20) switch
		{
			't' => Ast.BooleanLiteral.True,
			'f' => Ast.BooleanLiteral.False,
			_   => Ast.BooleanLiteral.Unknown,
		});

	public static UnaryOperator? SignOf(string? sign) =>
		sign switch
		{
			"+" => UnaryOperator.Plus,
			"-" => UnaryOperator.Minus,
			_   => null,
		};

	/// <summary>A character set's name as an introducer writes it, the dots between its parts.</summary>
	static CharacterSetName CharacterSet(string dotted)
	{
		var parts = dotted.Split('.');
		var names = new Identifier[parts.Length];

		for (var at = 0; at < parts.Length; at++)
			names[at] = new Identifier(parts[at], parts[at].Length > 0 && parts[at][0] == '"' ? IdentifierStyle.Delimited : IdentifierStyle.Regular);

		return new CharacterSetName(new QualifiedName(names));
	}

	// ── Numbers inside a production ────────────────────────────────────────────

	/// <summary>An unsigned integer as the BNF writes one — in any radix, with underscores — or null.</summary>
	public static int? Integer(string? text) =>
		text is null ? null : (int)Long(text);

	static long Long(string text)
	{
		text = text.Replace("_", "");

		if (text.Length > 2 && text[0] == '0')
		{
			switch (text[1] | 0x20)
			{
				case 'x': return Convert.ToInt64(text.Substring(2), 16);
				case 'o': return Convert.ToInt64(text.Substring(2), 8);
				case 'b': return Convert.ToInt64(text.Substring(2), 2);
			}
		}

		return long.Parse(text, System.Globalization.CultureInfo.InvariantCulture);
	}

	// ── §10.1 Interval qualifier ───────────────────────────────────────────────

	public static DateTimeField Field(string word) =>
		word.ToUpperInvariant() switch
		{
			"YEAR"   => DateTimeField.Year,
			"MONTH"  => DateTimeField.Month,
			"DAY"    => DateTimeField.Day,
			"HOUR"   => DateTimeField.Hour,
			"MINUTE" => DateTimeField.Minute,
			_        => DateTimeField.Second,
		};

	/// <summary>A start field and an end field: the start's precision, and the end's fractional one.</summary>
	public static IntervalQualifier Qualifier(IntervalQualifier start, IntervalQualifier end) =>
		new(start.Start, start.LeadingPrecision, end.Start, end.FractionalPrecision);

	// ── §6.1 Data types ────────────────────────────────────────────────────────

	/// <summary>A collection type's suffix: `ARRAY` with its cardinality and its brackets, or `MULTISET`.</summary>
	public readonly record struct Suffix(bool Multiset, int? Cardinality, bool Trigraphs);

	public static Suffix ArraySuffix(string? bracket, string? cardinality) =>
		new(false, Integer(cardinality), bracket == "??(");

	public static Suffix MultisetSuffix() => new(true, null, false);

	/// <summary>A type and the collection suffixes after it, each wrapping what stands before it.</summary>
	public static DataType Collected(DataType type, Suffix[]? suffixes)
	{
		foreach (var suffix in suffixes ?? [])
			type = suffix.Multiset ? new DataType.Multiset(type) : new DataType.Array(type, suffix.Cardinality, suffix.Trigraphs);

		return type;
	}

	/// <summary>A length as a character string type writes it: a number or a large object's size, and its units.</summary>
	public readonly record struct Length(int? Characters, LargeObjectSize? LargeObject, LengthUnit? Unit);

	public static Length LengthOf(string number, string? units) => new(Integer(number), null, Units(units));

	public static Length LengthOf(LargeObjectSize size, string? units) => new(null, size, Units(units));

	public static LargeObjectSize Size(string number, string? multiplier) =>
		new(Long(number), multiplier is null ? null : multiplier[0]);

	static LengthUnit? Units(string? units) =>
		units is null ? null : (units[0] | 0x20) == 'c' ? LengthUnit.Characters : LengthUnit.Octets;

	/// <summary>A character string type by its spelling — the words written, one space between them — and its length.</summary>
	public static DataType Character(string spelling, Length? length) =>
		new DataType.Character(
			string.Join(" ", spelling.ToUpperInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)) switch
			{
				"CHARACTER"                        => CharacterTypeKind.Character,
				"CHAR"                             => CharacterTypeKind.Char,
				"CHARACTER VARYING"                => CharacterTypeKind.CharacterVarying,
				"CHAR VARYING"                     => CharacterTypeKind.CharVarying,
				"VARCHAR"                          => CharacterTypeKind.Varchar,
				"CHARACTER LARGE OBJECT"           => CharacterTypeKind.CharacterLargeObject,
				"CHAR LARGE OBJECT"                => CharacterTypeKind.CharLargeObject,
				"CLOB"                             => CharacterTypeKind.Clob,
				"NATIONAL CHARACTER"               => CharacterTypeKind.NationalCharacter,
				"NATIONAL CHAR"                    => CharacterTypeKind.NationalChar,
				"NCHAR"                            => CharacterTypeKind.Nchar,
				"NATIONAL CHARACTER VARYING"       => CharacterTypeKind.NationalCharacterVarying,
				"NATIONAL CHAR VARYING"            => CharacterTypeKind.NationalCharVarying,
				"NCHAR VARYING"                    => CharacterTypeKind.NcharVarying,
				"NATIONAL CHARACTER LARGE OBJECT"  => CharacterTypeKind.NationalCharacterLargeObject,
				"NCHAR LARGE OBJECT"               => CharacterTypeKind.NcharLargeObject,
				_                                  => CharacterTypeKind.Nclob,
			},
			length?.Characters, length?.Unit, LargeObject: length?.LargeObject);

	/// <summary>A character string type with the character set and the collation written after it.</summary>
	public static DataType Characters(DataType type, CharacterSetName? characterSet, CollationName? collation) =>
		characterSet is null && collation is null ? type : ((DataType.Character)type) with { CharacterSet = characterSet, Collation = collation };

	public static DataType Binary(string spelling, string? length, LargeObjectSize? size) =>
		new DataType.Binary(
			spelling switch
			{
				"BINARY"              => BinaryTypeKind.Binary,
				"BINARY VARYING"      => BinaryTypeKind.BinaryVarying,
				"VARBINARY"           => BinaryTypeKind.Varbinary,
				"BINARY LARGE OBJECT" => BinaryTypeKind.BinaryLargeObject,
				_                     => BinaryTypeKind.Blob,
			},
			Integer(length), size);

	public static DataType NumericType(string spelling, string? precision, string? scale) =>
		new DataType.Numeric(
			spelling.ToUpperInvariant() switch
			{
				"NUMERIC"          => NumericTypeKind.Numeric,
				"DECIMAL"          => NumericTypeKind.Decimal,
				"DEC"              => NumericTypeKind.Dec,
				"SMALLINT"         => NumericTypeKind.SmallInt,
				"INTEGER"          => NumericTypeKind.Integer,
				"INT"              => NumericTypeKind.Int,
				"BIGINT"           => NumericTypeKind.BigInt,
				"FLOAT"            => NumericTypeKind.Float,
				"REAL"             => NumericTypeKind.Real,
				"DOUBLE PRECISION" => NumericTypeKind.DoublePrecision,
				_                  => NumericTypeKind.DecFloat,
			},
			Integer(precision), Integer(scale));

	public static DataType DateTime(string word, string? precision, string? zone) =>
		new DataType.DateTime(
			(word.Length > 4 ? DateTimeTypeKind.Timestamp : DateTimeTypeKind.Time),
			Integer(precision),
			zone is null ? null : (zone.Length > 4 ? TimeZoneMode.Without : TimeZoneMode.With));
}
