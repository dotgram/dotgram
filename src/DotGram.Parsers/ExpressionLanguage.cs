using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;

using DotGram;

namespace DotGram.Parsers;

// A small language that compiles to a .NET expression tree — parameters, a block with
// local variables, and `return`:
//
//     (int x, int y) => { int sum = x + y; return sum * sum; }
//
// **What it reads is C#'s expression syntax**, not a shape of its own: the same
// precedence ladder in the same order, the same operators at each level, and the same
// literal forms down to the digit separator and the verbatim string. A reader who knows
// C# — or C, or C++, or Java — should never have to ask how this one writes something,
// and every place where it does differ is written down at the end of this comment.
//
// **Every `=>` below is a call into `System.Linq.Expressions` by name.** There is no
// model of this project's own between the grammar and the API, and no dispatch on an
// operator's text either: one alternative per operator, each naming the factory that
// builds it. That is not tidiness — it is where the seam is tested. A factory that does
// not exist, or one handed the wrong type, is a C# error reported on the line of the
// grammar that asked for it (§7.6); the same choice made by a `switch` over `op` would
// be a run-time exception in a library instead, which is the C# compiler kept out of
// work it can do.
//
// **What is left in this class is what the API has nowhere to keep**, and the list is
// worth reading as exactly that:
//
//   * `Declare`/`Named`/`Scoped` — a `ParameterExpression` is an identity, and the one
//     made for `(int x)` has to be the very object each `x` reads. Nothing in the API
//     holds a mapping from a name to it, and nothing in it knows what a block is for.
//   * `Return`/`Returning` — a `return` is a jump to a label, and the label belongs to
//     the lambda rather than to any statement in it.
//
// Everything else the grammar says itself.
//
// **Two facts about the notation decide the shape**, and the first was measured rather
// than assumed:
//
//   * `=>` runs after the whole match, children before parents and, among siblings, from
//     the end of the text backwards — so a use of `x` is built *before* the parameter
//     that declares it, and no `=>` can resolve a name.
//   * `when` runs *during* the match (§8.1), in reading order.
//
// So declarations and blocks are recorded by guards while reading, and uses are built
// afterwards against a picture that is by then complete. A guard **answers** rather than
// throws, because it also runs on readings the parse abandons — which is also why a
// scope is a pair of positions and not a stack that was pushed and popped. A position is
// the same fact however many times a reading writes it down.
//
// **A block is an expression**, because in this API it is one: `Expression.Block` yields
// the value of its last expression, so `int a = { int b = 2; b * b };` reads and means
// what it looks like. `return` is there too and does what C# does — leave the whole
// lambda, from however deep in.
//
// **And the grammar is shaped to the API in two places**, deliberately:
//
//   * a local says its type — `int sum = …`, not `var sum = …` — because
//     `Expression.Variable` wants one where the declaration is read, and the initializer
//     is not built until long after;
//   * nothing widens on its own — `x + 1.5` over an `int` and a `double` is refused by
//     `Expression.Add` itself, in its own words, because a language that speaks only this
//     API has no place to put a conversion the API did not ask for.
//
// Both are the API's requirements showing through, which is what wiring one up actually
// looks like.
//
// **Where it is not C#, and why.** None of these is a shape the notation could not
// carry; each is the same API requirement showing through again.
//
//   * `null` is an `object` rather than whatever the other side of the operator wants.
//     C# types it by its target, and target typing is a pass over the whole expression —
//     the second layer this file exists to do without. `(string)null` says which instead.
//   * There is no member access, no call and no indexer: `x.Length`, `Math.Max(a, b)`
//     and `a[0]` each need a name looked up in another assembly's metadata, which is a
//     seam this has not been given. It is the obvious next one to give it.
//   * `is`, `as`, `typeof`, `default`, `checked` and a lambda inside an expression are
//     absent for that same reason or for want of a use here.
//   * An assignment is a statement, never an expression, so `a = b = c` and `x += 1` are
//     not written — a block has declarations, expressions and `return`, and no more.
//   * A block may shadow a name an enclosing one declared, which C# refuses outright
//     (CS0136). The nearer name wins here — more permissive than C#, so no valid C# is
//     turned into something else, and the check C# makes is one this has no reason to.

[Gram("""
	@using System;
	@using System.Globalization;
	@using System.Linq.Expressions;

	using Lexical;

	namespace Lexical
	{
		trivia = none

		Word = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_']*

		// ── Numbers, written the way C# writes them ─────────────────────────────────

		Digit    = ['0'..'9']
		HexDigit = ['0'..'9' | 'a'..'f' | 'A'..'F']
		BinDigit = ['0' | '1']

		// A separator stands between digits and is no part of the value, so every rule
		// below hands back the digits with them taken out: `long.Parse` reads a number,
		// not a number and an underscore.
		DecRun = Digit    & ('_'* & Digit)*
		HexRun = HexDigit & ('_'* & HexDigit)*
		BinRun = BinDigit & ('_'* & BinDigit)*

		Exponent = ['e' | 'E'] & ['+' | '-']? & DecRun

		// The three ways C# writes a real: a point, a point with nothing before it, and
		// an exponent standing in for the point.
		RealRun = DecRun & '.' & DecRun & Exponent?
		        | '.' & DecRun & Exponent?
		        | DecRun & Exponent

		// A base is a prefix rather than a suffix, and the digits it admits are its own.
		Dec  : @string = t: DecRun                => @(t.Replace("_", ""))
		Real : @string = t: RealRun               => @(t.Replace("_", ""))
		Hex  : @string = "0x"i & '_'* & t: HexRun => @(t.Replace("_", ""))
		Bin  : @string = "0b"i & '_'* & t: BinRun => @(t.Replace("_", ""))

		Number : @string = r: Real => @(r) | d: Dec => @(d)

		// A suffix says which type the constant is. Lexical, because nothing may come
		// between the digits and the letter — and because §4.6 weaves a boundary round a
		// word literal, and a digit is a word character, so `"L"` after `1` would be
		// refused by the very guard that keeps `int` out of `internal`. A set is not a
		// literal and carries no boundary, which is the other half of why these are
		// written as sets.
		//
		// One rule per suffix, over the digits it suffixes (§4.2), so that `1UL`,
		// `0xFFUL` and `0b1UL` are one rule specialized three times and not three rules.
		Unsigned    (N) : @string = t: N & ['u' | 'U'] => @(t)
		SignedLong  (N) : @string = t: N & ['l' | 'L'] => @(t)
		UnsignedLong(N) : @string
			= t: N & (['u' | 'U'] & ['l' | 'L'] | ['l' | 'L'] & ['u' | 'U']) => @(t)

		Decimals : @string = t: Number & ['m' | 'M'] => @(t)
		Doubles  : @string = t: Number & ['d' | 'D'] => @(t)
		Floats   : @string = t: Number & ['f' | 'F'] => @(t)

		// ── Text, and the characters that stand for themselves ──────────────────────

		// An escape names the character it stands for, one alternative each, so the
		// decoding is the grammar's and every value in it is a C# constant the compiler
		// reads. A table in a helper would say the same thing where nothing checks it.
		Escape : @string
			= "\\a"                    => @("\a")
			| "\\b"                    => @("\b")
			| "\\f"                    => @("\f")
			| "\\n"                    => @("\n")
			| "\\r"                    => @("\r")
			| "\\t"                    => @("\t")
			| "\\v"                    => @("\v")
			| "\\0"                    => @("\0")
			| "\\\\"                   => @("\\")
			| "\\'"                    => @("'")
			| "\\\""                   => @("\"")
			| "\\u" & t: HexDigit{4}   => @(((char)Convert.ToInt32(t, 16)).ToString())
			| "\\U" & t: HexDigit{8}   => @(char.ConvertFromUtf32(Convert.ToInt32(t, 16)))
			| "\\x" & t: HexDigit{1,4} => @(((char)Convert.ToInt32(t, 16)).ToString())

		// The parts of a run: an escape, or the longest stretch that needs none.
		TextPart : @string = e: Escape => @(e) | t: [^ '"' | '\\']+  => @(t)
		CharPart : @string = e: Escape => @(e) | t: [^ '\'' | '\\'] => @(t)

		Text : @string = '"' & parts: TextPart* & '"' => @(string.Concat(parts))
		Char : @string = '\'' & part: CharPart & '\'' => @(part)

		// A verbatim string takes every character as written, and doubling the quote is
		// how it says one — the whole of the difference, and the whole of the rule.
		VerbatimPart : @string = "\"\"" => @("\"") | t: [^ '"']+ => @(t)

		Verbatim : @string = "@\"" & parts: VerbatimPart* & '"' => @(string.Concat(parts))
	}

	// §4.6: a keyword is a whole word, so `returned` is a name and not a jump, and
	// `internal` is a name and not the type `int`.
	wordboundary = [\p{L} | \p{Nd} | '_']

	trivia = { (' ' | '\t' | '\r' | '\n')* }

	// ── A lambda: what it takes, and what it does ───────────────────────────────

	Lambda : @LambdaExpression
		= '(' & (first: Parameter & (',' & rest: Parameter)*)? & ')' & "=>" & body: Expression
		=> @(Expression.Lambda(
			ExpressionLanguage.Returning(body), ExpressionLanguage.Taking(first, rest)))

	// Each type names itself in C#, so `typeof(int)` is checked where it is written and
	// a word that is no type is not a declaration — the grammar refusing that reading
	// rather than a switch over strings refusing it at run time.
	Type : @Type = "sbyte"   => @(typeof(sbyte))
	             | "byte"    => @(typeof(byte))
	             | "short"   => @(typeof(short))
	             | "ushort"  => @(typeof(ushort))
	             | "int"     => @(typeof(int))
	             | "uint"    => @(typeof(uint))
	             | "long"    => @(typeof(long))
	             | "ulong"   => @(typeof(ulong))
	             | "float"   => @(typeof(float))
	             | "double"  => @(typeof(double))
	             | "decimal" => @(typeof(decimal))
	             | "bool"    => @(typeof(bool))
	             | "char"    => @(typeof(char))
	             | "string"  => @(typeof(string))
	             | "object"  => @(typeof(object))

	// The guard is the declaration: it runs while the text is read, which is the only
	// moment this grammar has in the order it is written — and `parserSpan` is where it
	// was read, which is the only thing that can say later which block it belongs to.
	Parameter : @ParameterExpression
		= type: Type & name: Word & when @(ExpressionLanguage.Declare(type, name, parserSpan))
		=> @(ExpressionLanguage.Named(name, parserSpan))

	// ── A block, which is an expression like any other ──────────────────────────

	// `Expression.Block` yields the value of its last expression, so this one does too:
	// statements, and then the expression the block is worth. C# writes that as `return`
	// and both are here — a `return` is a jump out of the whole lambda, as it is in C#,
	// and a trailing expression is the block's own value, as it is in the API.
	//
	// The guard at the end records the extent, which is what a name written inside it
	// resolves against. It runs while the text is read (§8.1); a `=>` would be too late,
	// because a use inside the block is built before the block is.
	Block : @Expression
		= '{' & statements: Statement* & value: Expression? & '}'
		& when @(ExpressionLanguage.Scoped(parserSpan))
		=> @(ExpressionLanguage.Block(statements, value))

	Statement : @Expression = s: Local => @(s) | s: Return => @(s) | s: Expression & ';' => @(s)

	Local : @Expression
		= type: Type & name: Word & when @(ExpressionLanguage.Declare(type, name, parserSpan))
		& '=' & value: Expression & ';'
		=> @(Expression.Assign(ExpressionLanguage.Named(name, parserSpan), value))

	Return : @Expression = "return" & value: Expression & ';'
	                     => @(ExpressionLanguage.Return(value))

	// ── The operators: C#'s ladder, one rule per level of precedence (§4.3) ─────
	//
	// Read this section from the bottom up and it is the table out of the C# spec, in
	// order and with nothing skipped between a name and `?:`. Every level is left
	// recursive, which is where the associativity is — `10 - 3 - 2` is `(10 - 3) - 2` —
	// except the two C# groups to the right, which are written right recursive.

	Expression : @Expression = e: Conditional => @(e)

	// `?:` groups to the right and its condition is one level tighter, so `a ?? b ? c : d`
	// is `(a ?? b) ? c : d` and `a ? b : c ? d : e` is `a ? b : (c ? d : e)`.
	Conditional : @Expression
		= test: Coalesce & '?' & then: Conditional & ':' & otherwise: Conditional
		  => @(Expression.Condition(test, then, otherwise))
		| c: Coalesce => @(c)

	Coalesce : @Expression = left: Or & "??" & right: Coalesce => @(Expression.Coalesce(left, right))
	                       | o: Or                             => @(o)

	Or  : @Expression = left: Or  & "||" & right: And   => @(Expression.OrElse(left, right))
	                  | a: And                          => @(a)

	And : @Expression = left: And & "&&" & right: BitOr => @(Expression.AndAlso(left, right))
	                  | b: BitOr                        => @(b)

	// The bitwise three sit between `&&` and `==`, where C# puts them. `|` and `&` each
	// begin a two-character operator one level out, and the lookahead is what tells them
	// apart — cheaper than letting `a || b` be read as `a | (| b)` and unwound by
	// backtracking, and clearer about why it is not.
	BitOr  : @Expression = left: BitOr  & '|' & ?!'|' & right: BitXor  => @(Expression.Or(left, right))
	                     | x: BitXor                                   => @(x)

	BitXor : @Expression = left: BitXor & '^' & right: BitAnd          => @(Expression.ExclusiveOr(left, right))
	                     | a: BitAnd                                   => @(a)

	BitAnd : @Expression = left: BitAnd & '&' & ?!'&' & right: Equality => @(Expression.And(left, right))
	                     | e: Equality                                  => @(e)

	Equality : @Expression
		= left: Equality & "==" & right: Relational => @(Expression.Equal(left, right))
		| left: Equality & "!=" & right: Relational => @(Expression.NotEqual(left, right))
		| r: Relational                             => @(r)

	// `>` and `>>` are told apart the same way, and here the lookahead earns more: the
	// shift is a level tighter, so without it `a >> b` is read as `a > (> b)` and only
	// the second `>` says otherwise.
	Relational : @Expression
		= left: Relational & "<=" & right: Shift        => @(Expression.LessThanOrEqual(left, right))
		| left: Relational & ">=" & right: Shift        => @(Expression.GreaterThanOrEqual(left, right))
		| left: Relational & '<' & ?!'<' & right: Shift => @(Expression.LessThan(left, right))
		| left: Relational & '>' & ?!'>' & right: Shift => @(Expression.GreaterThan(left, right))
		| s: Shift                                      => @(s)

	Shift : @Expression
		= left: Shift & "<<" & right: Additive => @(Expression.LeftShift(left, right))
		| left: Shift & ">>" & right: Additive => @(Expression.RightShift(left, right))
		| a: Additive                          => @(a)

	Additive : @Expression
		= left: Additive & '+' & right: Multiplicative => @(Expression.Add(left, right))
		| left: Additive & '-' & right: Multiplicative => @(Expression.Subtract(left, right))
		| m: Multiplicative                            => @(m)

	Multiplicative : @Expression
		= left: Multiplicative & '*' & right: Unary => @(Expression.Multiply(left, right))
		| left: Multiplicative & '/' & right: Unary => @(Expression.Divide(left, right))
		| left: Multiplicative & '%' & right: Unary => @(Expression.Modulo(left, right))
		| u: Unary                                  => @(u)

	Unary : @Expression
		= '-' & operand: Unary => @(Expression.Negate(operand))
		| '+' & operand: Unary => @(Expression.UnaryPlus(operand))
		| '!' & operand: Unary => @(Expression.Not(operand))
		| '~' & operand: Unary => @(Expression.OnesComplement(operand))

		// A cast is told from a parenthesized expression by what stands inside it: every
		// type here is a keyword, and a keyword is no name. C# needs a rule of its own to
		// decide this because a type there may be a name; a language whose types are a
		// closed set of keywords does not.
		| '(' & type: Type & ')' & operand: Unary => @(Expression.Convert(operand, type))

		| p: Primary => @(p)

	Primary : @Expression
		= b: Block => @(b)

		| '(' & inner: Expression & ')' => @(inner)

		// The suffixed and prefixed forms first: ordered choice would otherwise read `1L`
		// as the `1` of an `int` and leave the letter to whatever comes next, and only
		// backtracking would find its way here (§11). Reals before integers for the same
		// reason, so that `1.5`, `1e5` and `0x1F` are each read whole rather than as a
		// number and something the parse then has nowhere to put.
		| token: Decimals => @(Expression.Constant(decimal.Parse(token, NumberStyles.Float, CultureInfo.InvariantCulture)))
		| token: Doubles  => @(Expression.Constant(double.Parse(token, NumberStyles.Float, CultureInfo.InvariantCulture)))
		| token: Floats   => @(Expression.Constant(float.Parse(token, NumberStyles.Float, CultureInfo.InvariantCulture)))
		| token: Real     => @(Expression.Constant(double.Parse(token, NumberStyles.Float, CultureInfo.InvariantCulture)))

		| token: UnsignedLong(Hex) => @(Expression.Constant(Convert.ToUInt64(token, 16)))
		| token: SignedLong(Hex)   => @(Expression.Constant(Convert.ToInt64(token, 16)))
		| token: Unsigned(Hex)     => @(Expression.Constant(Convert.ToUInt32(token, 16)))
		| token: Hex               => @(Expression.Constant(Convert.ToInt32(token, 16)))

		| token: UnsignedLong(Bin) => @(Expression.Constant(Convert.ToUInt64(token, 2)))
		| token: SignedLong(Bin)   => @(Expression.Constant(Convert.ToInt64(token, 2)))
		| token: Unsigned(Bin)     => @(Expression.Constant(Convert.ToUInt32(token, 2)))
		| token: Bin               => @(Expression.Constant(Convert.ToInt32(token, 2)))

		| token: UnsignedLong(Dec) => @(Expression.Constant(ulong.Parse(token, CultureInfo.InvariantCulture)))
		| token: SignedLong(Dec)   => @(Expression.Constant(long.Parse(token, CultureInfo.InvariantCulture)))
		| token: Unsigned(Dec)     => @(Expression.Constant(uint.Parse(token, CultureInfo.InvariantCulture)))

		// An integer with no suffix is an `int` where it fits and a `long` where it does
		// not, which is C#'s own rule. `int.TryParse` is what knows, asked while the text
		// is read (§8.1) — so the two are two readings of the same digits, and not a
		// helper this class would otherwise have to hold.
		| token: Dec & when @(int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
		                   => @(Expression.Constant(int.Parse(token, CultureInfo.InvariantCulture)))
		| token: Dec       => @(Expression.Constant(long.Parse(token, CultureInfo.InvariantCulture)))

		| token: Verbatim  => @(Expression.Constant(token))
		| token: Text      => @(Expression.Constant(token))
		| token: Char      => @(Expression.Constant(token[0]))

		| "true"     => @(Expression.Constant(true))
		| "false"    => @(Expression.Constant(false))

		// Typed `object`, because C# types `null` by what it stands against and that is a
		// pass over the whole expression this language does not make. `(string)null` says
		// which, where which one matters.
		| "null"     => @(Expression.Constant(null, typeof(object)))

		| name: Word => @(ExpressionLanguage.Named(name, parserSpan))

	parse Lambda as ParseLambda
	""")]
public static partial class ExpressionLanguage
{
	// ParseLambda and TryParseLambda are generated here.

	/// <summary>Reads the text as a lambda over an expression tree.</summary>
	/// <exception cref="FormatException">The text is not this language.</exception>
	/// <exception cref="ArgumentException">
	/// It is this language and means nothing in it — an operator its operands do not
	/// support, a lambda over a variable nothing declares. Thrown by
	/// <c>System.Linq.Expressions</c> itself, in its own words: this language holds no
	/// opinion the API does not already hold.
	/// </exception>
	public static LambdaExpression Parse(string text)
	{
		_declared?.Clear();
		_scopes?.Clear();
		_returns = null;

		return ParseLambda(text);
	}

	/// <summary>The same, compiled to a delegate of the caller's own type.</summary>
	/// <remarks>
	/// Where the two halves meet a caller: what the text declares has to match what the
	/// delegate takes, and <c>Expression.Lambda</c> is what says so — in a message naming
	/// both, which is better than anything this could invent.
	/// </remarks>
	public static TDelegate Compile<TDelegate>(string text)
		where TDelegate : Delegate
	{
		var lambda = Parse(text);

		return (TDelegate)Expression.Lambda(typeof(TDelegate), lambda.Body, lambda.Parameters).Compile();
	}

	// ── What System.Linq.Expressions has nowhere to keep ────────────────────────

	/// <summary>A block's extent, which is the whole of what a scope is.</summary>
	readonly record struct Scope(int From, int To);

	/// <summary>A variable, the name the text calls it, and where that name was written.</summary>
	readonly record struct Declaration(int At, string Name, ParameterExpression Variable);

	/// <summary>Every block read, by where it stood.</summary>
	/// <remarks>
	/// Recorded by position rather than pushed and popped, and the difference is
	/// backtracking. A guard runs on readings the parse goes on to abandon, so a stack
	/// would be left holding a scope nothing is inside; a position is the same fact
	/// however many times it is written down, and an abandoned reading records an extent
	/// that no surviving name is written inside.
	/// </remarks>
	[ThreadStatic]
	static List<Scope>? _scopes;

	/// <summary>The variables of the lambda being read, in the order they were declared.</summary>
	/// <remarks>
	/// A <c>ParameterExpression</c> is an identity: the one made for <c>(int x)</c> has to
	/// be the very object every <c>x</c> in the body reads, or the compiled lambda closes
	/// over a variable nothing assigns. A list rather than a table by name, because two
	/// blocks beside each other may each declare an <c>x</c> and those are two variables —
	/// which is legal C#, and the whole reason a name is looked up by where it is written.
	/// </remarks>
	[ThreadStatic]
	static List<Declaration>? _declared;

	/// <summary>Where a <c>return</c> goes, made once the first one says what it yields.</summary>
	[ThreadStatic]
	static LabelTarget? _returns;

	/// <summary>A block, recorded while the text is read (§8.1).</summary>
	/// <remarks>
	/// It has to be read rather than built: <c>=&gt;</c> runs children before parents, so
	/// every name inside a block is built before the block itself is, and a scope recorded
	/// there would arrive after the last thing that needed it.
	/// </remarks>
	public static bool Scoped(SourceSpan span)
	{
		(_scopes ??= []).Add(new Scope(span.Start, span.Start + span.Length));

		return true;
	}

	/// <summary>
	/// A declaration, made while the text is read (§8.1) — the only moment this grammar
	/// has in the order it is written.
	/// </summary>
	/// <returns>
	/// Whether this reads as a declaration at all. A guard answers rather than throws,
	/// and it has to: <c>when</c> runs during the match, so it also runs on readings the
	/// parse goes on to abandon.
	/// </returns>
	/// <remarks>
	/// What that costs is that an abandoned reading leaves its name behind. It is a name
	/// at a position now, though, so only a use inside the same block can find it — and
	/// where the reading was abandoned, no such use is left.
	/// </remarks>
	public static bool Declare(Type type, string name, SourceSpan at)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		(_declared ??= []).Add(new Declaration(at.Start, name, Expression.Variable(type, name)));

		return true;
	}

	/// <summary>The variable that name means where it is written.</summary>
	/// <remarks>
	/// <para>
	/// A declaration is visible from where it stands to the end of the block holding it,
	/// which is C#'s rule and is what these two positions decide between them. The
	/// innermost block wins, so an inner one shadows: where C# refuses that outright
	/// (CS0136) this reads the nearer name, which is the more permissive of the two and
	/// turns no valid C# into something else.
	/// </para>
	/// <para>A parameter is written outside every block and is therefore in all of them.</para>
	/// </remarks>
	public static ParameterExpression Named(string name, SourceSpan at)
	{
		var use   = at.Start;
		var found = default(ParameterExpression);
		var inner = int.MinValue;
		var wrote = int.MinValue;

		foreach (var declaration in _declared ?? [])
		{
			if (!string.Equals(declaration.Name, name, StringComparison.Ordinal) ||
				declaration.At > use)
				continue;

			var block = Holding(declaration.At);

			// Declared in a block this use is not inside: the other branch of the same
			// choice, the block before this one. Not a shadow and not an error — simply
			// not a name that is in scope here.
			if (block is { } held && (held.From > use || held.To <= use))
				continue;

			var from = block?.From ?? int.MinValue + 1;

			if (from > inner || from == inner && declaration.At > wrote)
			{
				found = declaration.Variable;
				inner = from;
				wrote = declaration.At;
			}
		}

		return found ?? throw new FormatException($"nothing named '{name}' is declared here.");
	}

	/// <summary>The innermost block a position stands in, or none for the lambda itself.</summary>
	static Scope? Holding(int position)
	{
		var innermost = default(Scope?);

		foreach (var scope in _scopes ?? [])
			if (scope.From <= position && position < scope.To &&
				(innermost is not { } known || scope.From > known.From))
				innermost = scope;

		return innermost;
	}

	/// <summary>The parameters a lambda takes, in the order it wrote them.</summary>
	public static ParameterExpression[] Taking(ParameterExpression? first, ParameterExpression[] rest)
	{
		if (first is null)
			return [];

		var parameters = new ParameterExpression[rest.Length + 1];

		parameters[0] = first;
		rest.CopyTo(parameters, 1);

		return parameters;
	}

	/// <summary>A jump to the lambda's label, which the first <c>return</c> is what makes.</summary>
	public static Expression Return(Expression value)
	{
		if (value is null)
			throw new ArgumentNullException(nameof(value));

		_returns ??= Expression.Label(value.Type, "return");

		return Expression.Return(_returns, value);
	}

	/// <summary>The body with the place its returns go to, where any of them do.</summary>
	/// <remarks>
	/// One label for the lambda rather than one per block, because that is what a
	/// <c>return</c> means in C#: it leaves the whole method, from however deep in. The
	/// lambda is also the only place that can hold it — a block is built before the blocks
	/// around it, so no block knows whether it is the outermost.
	/// </remarks>
	public static Expression Returning(Expression body)
	{
		if (body is null)
			throw new ArgumentNullException(nameof(body));

		return _returns is null
			? body
			: Expression.Block(body, Expression.Label(_returns, Expression.Default(_returns.Type)));
	}

	/// <summary>
	/// A block: the variables it declared, its statements, and the expression it is worth.
	/// </summary>
	/// <remarks>
	/// The variables are read back out of the assignments rather than collected
	/// separately — an assignment to a variable is what a declaration became, so the
	/// statements already say which they are.
	/// </remarks>
	public static Expression Block(Expression[] statements, Expression? value)
	{
		if (statements is null)
			throw new ArgumentNullException(nameof(statements));

		var variables = new List<ParameterExpression>();

		foreach (var statement in statements)
			if (statement is BinaryExpression { NodeType: ExpressionType.Assign, Left: ParameterExpression variable })
				variables.Add(variable);

		var body = new List<Expression>(statements);

		if (value is not null)
			body.Add(value);

		// A block that ends in a `return` never reaches its own end, so it needs no value:
		// the jump is where control goes, and the lambda is what holds the label.
		else if (statements.Length == 0 || statements[statements.Length - 1] is not GotoExpression)
			throw new FormatException(
				"a block is worth its last expression, and this one has none. End it with an " +
				"expression, or with a 'return'.");

		return Expression.Block(variables, body);
	}
}
