using System;

using DotGram;

namespace DotGram.Parsers;

/// <summary>
/// The expression layer of standard SQL: <c>&lt;value expression&gt;</c> and
/// <c>&lt;search condition&gt;</c>, as ISO/IEC 9075:1992 defines them.
/// </summary>
/// <remarks>
/// <para>
/// The bottom of the language, and deliberately only the bottom. SQL divides into
/// expressions, clauses and statements, and the two above cannot be written honestly
/// without the one below: a <c>SELECT</c> is mostly places where an expression stands.
/// So this is the expression layer, finished and testable on its own, and the query
/// level goes above it later.
/// </para>
/// <para>
/// <b>The rule names are the standard's, production for production.</b>
/// <c>SearchCondition</c>, <c>BooleanTerm</c>, <c>BooleanFactor</c>, <c>BooleanTest</c>,
/// <c>BooleanPrimary</c>, <c>Predicate</c>, <c>RowValueConstructor</c>,
/// <c>ValueExpression</c>, <c>Term</c>, <c>Factor</c>, <c>ValueExpressionPrimary</c> —
/// so that a reader with the standard open can follow one against the other, and so that
/// a disagreement about what something should do is settled by a section number rather
/// than by taste. Where a name would have come from an implementation's object model
/// instead, it did not.
/// </para>
/// <para>
/// The standard's edition is 1992, which is the compact core the later ones extend, and
/// what is taken from later editions is marked where it is taken.
/// </para>
/// <para>
/// <b>One divergence, and it is written where it happens.</b> §6.11 gives four value
/// towers — numeric, string, datetime, interval — that share a bottom, so
/// <c>a + b</c> belongs to two of them at once and only the types of <c>a</c> and
/// <c>b</c> say which. That is not a defect in the standard: §6.11 describes syntax
/// modulo type resolution, and a parser has no types. The four are one untyped ladder
/// here, which is what every implementation does.
/// </para>
/// <para>
/// Nothing is built yet. The grammar recognizes and the tree comes later, deliberately:
/// the shape of the node classes is a decision of its own, and getting the language
/// right first is what makes that decision about the tree rather than about the parse.
/// </para>
/// </remarks>
[Gram("""
	@using System;

	using Lexical;

	// ── §5 Lexical elements ─────────────────────────────────────────────────────
	//
	// A namespace with no trivia, so a token is its characters and nothing may stand
	// between them (§4.5). Everything outside it may be spaced and commented freely.
	namespace Lexical
	{
		trivia = none

		// §5.2. A regular identifier is a letter and then letters, digits or underscores;
		// a delimited one is anything between double quotes, with "" for a quote inside.
		// The standard's <identifier body> admits <identifier part> beyond ASCII, which is
		// what \p{L} and \p{Nd} say here.
		IdentifierStart = [\p{L} | '_']
		IdentifierPart  = [\p{L} | \p{Nd} | '_']
		RegularIdentifier = IdentifierStart & IdentifierPart*

		DelimitedIdentifier = '"' & ("\"\"" | [^ '"'])* & '"'

		// §5.3 <unsigned numeric literal>. An exact numeric has an optional point; an
		// approximate one is a mantissa and an exponent. Written in that order so that
		// `1E5` is read whole rather than as `1` and something the parse cannot place.
		Digits = ['0'..'9']+
		ExactNumericLiteral = Digits & ('.' & Digits?)? | '.' & Digits
		ApproximateNumericLiteral = ExactNumericLiteral & 'E'i & ['+' | '-']? & Digits
		UnsignedNumericLiteral = ApproximateNumericLiteral | ExactNumericLiteral

		// §5.3 <character string literal>, with '' for a quote inside. The optional
		// introducer names a character set.
		CharacterStringLiteral = ('_' & RegularIdentifier)? & QuotedString
		QuotedString = '\'' & ("''" | [^ '\''])* & '\''

		NationalCharacterStringLiteral = 'N'i & QuotedString
		BitStringLiteral = 'B'i & '\'' & ['0' | '1']* & '\''
		HexStringLiteral = 'X'i & '\'' & ['0'..'9' | 'a'..'f' | 'A'..'F']* & '\''

		// §5.3 <datetime literal> and <interval literal> carry their type in front of the
		// string, which is what tells them from a character string.
		DateLiteral      = "DATE"i      & Space & QuotedString
		TimeLiteral      = "TIME"i      & Space & QuotedString
		TimestampLiteral = "TIMESTAMP"i & Space & QuotedString
		Space = (' ' | '\t' | '\r' | '\n')+
	}

	// §5.2, and the reason the keywords below are literals rather than a rule: a keyword
	// is a whole word, so `ANDY` is an identifier and not `AND` followed by `Y`. §4.6
	// weaves this beside every word literal without it being written again.
	wordboundary = IdentifierPart

	// §5.4 <comment> is `--` to the end of the line; §5.2 allows the bracketed form in
	// later editions and it costs nothing to read.
	trivia = { (Whitespace | LineComment | BlockComment)* }
	Whitespace   = [' ' | '\t' | '\r' | '\n']+
	LineComment  = "--" & [^ '\n' | '\r']*
	BlockComment = "/*" & (?!"*/" & any)* & "*/"

	// ── §8 Predicates, and the tower the standard calls a search condition ──────
	//
	// The names are the standard's, production for production: <search condition> over
	// <boolean term> over <boolean factor> over <boolean test> over <boolean primary>.
	// Left recursive where the standard is left recursive, which §4.3 folds into a loop,
	// so the operand at the head is read once however many operators follow it.

	SearchCondition = SearchCondition & "OR"i & BooleanTerm
	                | BooleanTerm

	BooleanTerm = BooleanTerm & "AND"i & BooleanFactor
	            | BooleanFactor

	BooleanFactor = "NOT"i? & BooleanTest

	// §8.13 <boolean test>. `IS TRUE` and its fellows, and the standard allows them to
	// stack — `x IS TRUE IS NOT FALSE` is legal — so this repeats rather than being one
	// optional tail.
	BooleanTest = BooleanPrimary & ("IS"i & "NOT"i? & TruthValue)*
	TruthValue  = "TRUE"i | "FALSE"i | "UNKNOWN"i

	BooleanPrimary = Predicate
	               | '(' & SearchCondition & ')'

	// §8.1. Written in the order the standard lists them, except that the ones sharing a
	// left operand are gathered: <row value constructor> opens six of the nine, and
	// reading it once for all of them is what §4.3's folding is for. The three that do
	// not — EXISTS, UNIQUE and the LIKE whose operand is narrower — stand on their own.
	Predicate = "EXISTS"i & TableSubquery
	          | "UNIQUE"i & TableSubquery
	          | RowValueConstructor & PredicateTail

	// What may follow a row on the left of a predicate. One rule so that the row is read
	// once: written as six alternatives of <predicate> each beginning with <row value
	// constructor>, a row would be read six times before the parse found out which
	// predicate it was in — and a row holds whole value expressions.
	PredicateTail
	          // §8.2 <comparison predicate> and §8.8 <quantified comparison predicate>
	          // share their operator, and differ only in what stands on the right.
	          // The two comparison forms share their operator, so it is read once and
	          // what may follow it is the choice — which is the same grammar written the
	          // way GRAM4016 asks for, and a row on the right is not cheap to read twice.
	          = CompOp & (Quantifier & TableSubquery | RowValueConstructor)

	          // §8.3 <between predicate>
	          | "NOT"i? & "BETWEEN"i & RowValueConstructor
	            & "AND"i & RowValueConstructor

	          // §8.4 <in predicate>
	          | "NOT"i? & "IN"i & InPredicateValue

	          // §8.5 <like predicate>. The standard narrows both sides to character
	          // strings; that is a type rule and not a syntax one, so it is not written
	          // here — see the note on the collapsed towers above.
	          | "NOT"i? & "LIKE"i & ValueExpression
	            & ("ESCAPE"i & ValueExpression)?

	          // §8.6 <null predicate>
	          | "IS"i & "NOT"i? & "NULL"i

	          // §8.10 <match predicate>
	          | "MATCH"i & "UNIQUE"i? & ("PARTIAL"i | "FULL"i)?
	            & TableSubquery

	          // §8.12 <overlaps predicate>
	          | "OVERLAPS"i & RowValueConstructor

	CompOp = "<>" | "<=" | ">=" | '=' | '<' | '>'
	Quantifier = "ALL"i | "SOME"i | "ANY"i

	InPredicateValue = TableSubquery
	                 | '(' & ValueExpression & (',' & ValueExpression)* & ')'

	// ── §7.1 Row value constructor ─────────────────────────────────────────────
	//
	// A predicate compares rows, and a single value is a row of one — which is why
	// `a = 1` and `(a, b) = (1, 2)` are one production in the standard rather than two.

	RowValueConstructor = '(' & RowValueConstructorElement
	                          & (',' & RowValueConstructorElement)+ & ')'
	                    | TableSubquery
	                    | RowValueConstructorElement

	RowValueConstructorElement = ValueExpression | "NULL"i | "DEFAULT"i

	// ── §6.11 Value expression ─────────────────────────────────────────────────
	//
	// **One tower where the standard has four, and this is a deliberate divergence.**
	// §6.11 defines <value expression> as a choice of <numeric value expression>,
	// <string value expression>, <datetime value expression> and <interval value
	// expression>, each with a ladder of its own, and all four bottoming out in the same
	// <value expression primary>. So `a + b` matches the numeric ladder and the datetime
	// one at once, and which it is depends on the types of `a` and `b` — which a parser
	// does not know and the standard does not expect it to: §6.11 is a description of
	// syntax modulo type resolution, not a grammar anybody can run.
	//
	// Every implementation collapses the four into one untyped ladder, and so does this.
	// What is lost is nothing syntactic: the four ladders have the same shape and the same
	// precedence, and `||` is simply admitted at the additive level where the standard
	// gives it a ladder of its own.

	ValueExpression = ValueExpression & ('+' | '-' | "||") & Term
	                | Term

	Term = Term & ('*' | '/') & Factor
	     | Factor

	Factor = ['+' | '-']? & ValueExpressionPrimary

	// §6.11 <value expression primary>, in the standard's order, plus the value functions
	// §6.16 through §6.18 fold into <numeric primary>, <string value expression> and
	// <datetime value expression>. Gathered here because they are all primaries once the
	// towers are one.
	ValueExpressionPrimary
	    = '(' & ValueExpression & ')'
	    | CaseExpression
	    | CastSpecification
	    | ValueFunction
	    | SetFunctionSpecification
	    | ScalarSubquery
	    | UnsignedValueSpecification
	    | ColumnReference

	// ── §6.9 Set function, §6.16-6.18 value functions ──────────────────────────

	// COUNT(*) is its own shape; the rest take a set quantifier and a value expression.
	SetFunctionSpecification
	    = "COUNT"i & '(' & '*' & ')'
	    | SetFunctionType & '(' & SetQuantifier? & ValueExpression & ')'

	SetFunctionType = "AVG"i | "MAX"i | "MIN"i | "SUM"i | "COUNT"i
	SetQuantifier   = "DISTINCT"i | "ALL"i

	// The functions the standard spells out rather than leaving to <routine invocation>.
	// Each is a keyword and a fixed shape, which is why they are here and not a call.
	ValueFunction
	    = "POSITION"i    & '(' & ValueExpression & "IN"i & ValueExpression & ')'
	    | "EXTRACT"i     & '(' & ExtractField & "FROM"i & ValueExpression & ')'
	    | "CHAR_LENGTH"i & '(' & ValueExpression & ')'
	    | "CHARACTER_LENGTH"i & '(' & ValueExpression & ')'
	    | "OCTET_LENGTH"i & '(' & ValueExpression & ')'
	    | "BIT_LENGTH"i  & '(' & ValueExpression & ')'
	    | "SUBSTRING"i   & '(' & ValueExpression & "FROM"i & ValueExpression
	                     & ("FOR"i & ValueExpression)? & ')'
	    | "UPPER"i       & '(' & ValueExpression & ')'
	    | "LOWER"i       & '(' & ValueExpression & ')'
	    | "CONVERT"i     & '(' & ValueExpression & "USING"i & QualifiedName & ')'
	    | "TRANSLATE"i   & '(' & ValueExpression & "USING"i & QualifiedName & ')'
	    | "TRIM"i        & '(' & TrimSpecification? & ValueExpression?
	                     & "FROM"i & ValueExpression & ')'
	    | "TRIM"i        & '(' & ValueExpression & ')'
	    | "CURRENT_DATE"i
	    | "CURRENT_TIME"i      & ('(' & Digits & ')')?
	    | "CURRENT_TIMESTAMP"i & ('(' & Digits & ')')?

	ExtractField = "YEAR"i | "MONTH"i | "DAY"i | "HOUR"i | "MINUTE"i | "SECOND"i
	             | "TIMEZONE_HOUR"i | "TIMEZONE_MINUTE"i
	TrimSpecification = "LEADING"i | "TRAILING"i | "BOTH"i

	// ── §6.20 Case expression, §6.10 Cast ──────────────────────────────────────
	//
	// The standard's <case abbreviation> — NULLIF and COALESCE — is part of <case
	// expression>, not something beside it, and is written here where the standard puts
	// it.
	CaseExpression
	    = "NULLIF"i   & '(' & ValueExpression & ',' & ValueExpression & ')'
	    | "COALESCE"i & '(' & ValueExpression & (',' & ValueExpression)* & ')'
	    | "CASE"i & ValueExpression & SimpleWhen+
	              & ("ELSE"i & Result)? & "END"i
	    | "CASE"i & SearchedWhen+ & ("ELSE"i & Result)? & "END"i

	SimpleWhen   = "WHEN"i & ValueExpression & "THEN"i & Result
	SearchedWhen = "WHEN"i & SearchCondition  & "THEN"i & Result
	Result       = ValueExpression | "NULL"i

	CastSpecification = "CAST"i & '(' & CastOperand & "AS"i & DataType & ')'
	CastOperand = ValueExpression | "NULL"i

	// ── §6.1 Data type, cut to what a CAST target can be ───────────────────────

	DataType
	    = ("CHARACTER"i & "VARYING"i? | "CHAR"i & "VARYING"i? | "VARCHAR"i)
	      & Length? & ("CHARACTER"i & "SET"i & QualifiedName)?
	    | ("NATIONAL"i & ("CHARACTER"i | "CHAR"i) & "VARYING"i? | "NCHAR"i & "VARYING"i?) & Length?
	    | ("BIT"i & "VARYING"i?) & Length?
	    | ("NUMERIC"i | "DECIMAL"i | "DEC"i) & ('(' & Digits & (',' & Digits)? & ')')?
	    | "INTEGER"i | "INT"i | "SMALLINT"i
	    | ("FLOAT"i & ('(' & Digits & ')')?) | "REAL"i | ("DOUBLE"i & "PRECISION"i)
	    | "DATE"i
	    | ("TIME"i | "TIMESTAMP"i) & ('(' & Digits & ')')?
	      & ("WITH"i & "TIME"i & "ZONE"i)?
	    | "INTERVAL"i & IntervalQualifier

	Length = '(' & Digits & ')'

	// §10.1 <interval qualifier>, which is what an INTERVAL type and an interval literal
	// both end with.
	IntervalQualifier = SingleDatetimeField & ("TO"i & SingleDatetimeField)?
	SingleDatetimeField = ExtractField & ('(' & Digits & (',' & Digits)? & ')')?

	// ── §6.3 Value specification, §6.4 Column reference ────────────────────────

	UnsignedValueSpecification = UnsignedLiteral | GeneralValueSpecification

	UnsignedLiteral = UnsignedNumericLiteral
	                | CharacterStringLiteral
	                | NationalCharacterStringLiteral
	                | BitStringLiteral
	                | HexStringLiteral
	                | DateLiteral
	                | TimeLiteral
	                | TimestampLiteral
	                | IntervalLiteral

	// §5.3. The sign belongs to the literal here and not to <factor>, which is what lets
	// `INTERVAL -'1' DAY` read.
	IntervalLiteral = "INTERVAL"i & ['+' | '-']? & QuotedString
	                & IntervalQualifier

	// §6.3 <general value specification>: the parameters and the niladic functions that
	// stand where a value does.
	GeneralValueSpecification
	    = ':' & Identifier & ("INDICATOR"i? & ':' & Identifier)?
	    | '?'
	    | "USER"i | "CURRENT_USER"i | "SESSION_USER"i | "SYSTEM_USER"i | "VALUE"i

	// §6.4. A column reference is a qualified name, and how many parts it has is a
	// question about a catalogue rather than about syntax — so the syntax admits the
	// depth the standard allows and says nothing about what the parts mean.
	ColumnReference = QualifiedName

	QualifiedName = Identifier & (('.' & Identifier))*

	// The lookahead stands in front: an identifier is a word that is *not* reserved,
	// which is what 5.2 says. Behind it, it would be asking whether a reserved word
	// follows one — a different question, and one that refuses `x IN (1, 2)`.
	Identifier = ?!Reserved & RegularIdentifier | DelimitedIdentifier

	// ── The seam where the query level will go ─────────────────────────────────
	//
	// §7.9 <query specification> and everything above it are the next layer up, and this
	// grammar is the layer below. A subquery is left as a rule that reads a parenthesized
	// run rather than a query — so the language is closed and testable now, and the seam
	// is visible in the text rather than being a hole nobody remembers.
	//
	// What it accepts is wider than SQL, deliberately: anything balanced. That is the one
	// place this grammar is knowingly wrong, and it is wrong in the direction that cannot
	// mask a bug in the rest of it.
	TableSubquery  = Subquery
	ScalarSubquery = Subquery

	Subquery = '(' & (Balanced | [^ '(' | ')'])* & ')'
	Balanced = '(' & (Balanced | [^ '(' | ')'])* & ')'

	// ── §5.2 Reserved words ────────────────────────────────────────────────────
	//
	// What an identifier may not be. The standard's list is long and most of it belongs
	// to the statement level; what is here is the part this layer can be confused by —
	// a word that begins or continues an expression or a predicate.
	Reserved = ("AND"i | "ALL"i | "ANY"i | "AS"i | "BETWEEN"i | "BOTH"i | "BY"i
	         | "CASE"i | "CAST"i | "COALESCE"i | "CROSS"i | "CURRENT_DATE"i
	         | "CURRENT_TIME"i | "CURRENT_TIMESTAMP"i | "CURRENT_USER"i
	         | "DEFAULT"i | "DISTINCT"i | "ELSE"i | "END"i | "ESCAPE"i | "EXISTS"i
	         | "FALSE"i | "FOR"i | "FROM"i | "FULL"i | "GROUP"i | "HAVING"i | "IN"i
	         | "INTERVAL"i | "IS"i | "JOIN"i | "LEADING"i | "LIKE"i | "MATCH"i
	         | "NOT"i | "NULL"i | "NULLIF"i | "ON"i | "OR"i | "ORDER"i | "OVERLAPS"i
	         | "PARTIAL"i | "SELECT"i | "SESSION_USER"i | "SOME"i | "SYSTEM_USER"i
	         | "THEN"i | "TRAILING"i | "TRUE"i | "UNIQUE"i | "UNKNOWN"i | "USER"i
	         | "USING"i | "VALUE"i | "WHEN"i | "WHERE"i)
	         & ?!IdentifierPart

	parse SearchCondition as ParseSearchCondition
	parse ValueExpression as ParseValueExpression
	""")]
public static partial class SqlExpressions
{
	[ThreadStatic]
	static Parser? _parser;

	static partial void RentParser(ref Parser parser)
	{
		parser  = _parser!;
		_parser = null;
	}

	static partial void ReturnParser(Parser parser) => _parser = parser;
}
