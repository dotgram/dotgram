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
// **What is left in this class is what belongs to this API rather than to the language.**
// The grammar says what an `if` is, what a block is, what a name is — in the words every
// language uses for them. The host says what those turn into *here*. That division is the
// point: a grammar carrying `System.Linq.Expressions`' own distinctions would be a grammar
// for one API, and the whole question this file exists to answer is whether the notation
// can be pointed at somebody else's. So the list below is worth reading as exactly that —
// what is specific to the target, kept where the target's name is already written:
//
//   * `Declare`/`Named`/`Scoped` — a `ParameterExpression` is an identity, and the one
//     made for `(int x)` has to be the very object each `x` reads. Nothing in the API
//     holds a mapping from a name to it, and nothing in it knows what a block is for.
//   * `Loops`/`Breaks`/`Exit`/`Again` — the same thing for a label: which loop a `break`
//     leaves is where it is written, and the label is an identity too.
//   * `Return`/`Returning` — a `return` is a jump to a label, and the label belongs to
//     the lambda rather than to any statement in it.
//   * `Chosen` — `Expression.Condition` is one factory with two answers, the branches'
//     type or `void`, and which one an `if` meant is a question about this API alone.
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
// what it looks like — and nothing here decides that beyond passing the statements on.
// `return` is there too and does what C# does: leave the whole lambda, from however deep.
//
// An `if` with an `else` is worth what its branches are worth, for the same reason, so
// `int n = if (c) 1 else 2;` reads as well. `?:` is that same factory and does not stand
// in for it: a branch of `?:` is an expression, and a branch of `if` is a statement, which
// is where a block with declarations in it can go.
//
// Both stand where a value is *expected* — an initializer, a `return`, a branch, the last
// thing in a block — and not as an operand in the middle of one: `1 + { … }` is not
// written here. That is a measurement rather than a taste. A construct reachable both as a
// statement and as a `Primary` is read once as each, at every level of a nest of them, and
// a chain of three `else if`s took 1.6 seconds before each of them had one route.
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
//   * A loop is void. The API would type one — a `Loop` whose break label carries a value
//     is worth it — but only a loop with no ordinary way out can, since the ordinary way
//     out would have to carry a value too, and C#'s `break` carries nothing.
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
		= '(' & (first: Parameter & (',' & rest: Parameter)*)? & ')' & "=>" & body: Value
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
		=> @(ExpressionLanguage.Block(statements, parserSpan, value))

	Statement : @Expression
		= s: Local            => @(s)
		| s: Return           => @(s)
		| s: Block            => @(s)
		| s: Control          => @(s)
		| s: Jump & ';'       => @(s)
		| s: Expression & ';' => @(s)

	Local : @Expression
		= type: Type & name: Word & when @(ExpressionLanguage.Declare(type, name, parserSpan))
		& '=' & value: Value & ';'
		=> @(Expression.Assign(ExpressionLanguage.Named(name, parserSpan), value))

	Return : @Expression = "return" & value: Value & ';'
	                     => @(ExpressionLanguage.Return(value))

	// ── The statements that carry a body, and so end without a semicolon ────────
	//
	// An `if` with an `else` is worth what its branches are worth, so it is a `Primary` as
	// well as a statement and `int n = if (c) 1 else 2;` reads. `?:` is the same factory
	// and does not replace it: a branch there is an expression, and this one takes a
	// statement, which is where a block with declarations in it can go.
	//
	// The rest are void. The API would type a loop too — a `Loop` whose break label
	// carries a value is worth one — but only a loop with no ordinary way out can have it,
	// since the ordinary way out would have to carry one as well, and C# has no `break`
	// that does. A `switch` here is C#'s statement, likewise.

	// Where a value is expected and a block or an `if` may stand: an initializer, a
	// `return`, the branch of an `if`.
	//
	// **Two routes to one construct is what makes a parse exponential**, and this rule is
	// the answer to it. A `Block` that is both a statement and a `Primary` is read once as
	// each — at every level of a nest of them — and so is an `if` that is both a statement
	// and a `Primary`. Written that way, a chain of three `else if`s took 1.6 seconds and a
	// nest of five braces took 428 ms, both doubling and worse per level. Reachable one way
	// only, with the value positions naming them here, both are too fast to measure.
	Value : @Expression = b: Block => @(b) | c: Control => @(c) | e: Expression => @(e)

	Control : @Expression
		= c: If      => @(c)
		| c: While   => @(c)
		| c: DoWhile => @(c)
		| c: For     => @(c)
		| c: Switch  => @(c)

	If : @Expression
		= "if" & '(' & test: Expression & ')' & then: Branch & "else" & otherwise: Branch
		  => @(ExpressionLanguage.Chosen(test, then, otherwise))
		| "if" & '(' & test: Expression & ')' & then: Statement
		  => @(Expression.IfThen(test, then))

	// A branch is a statement where one was written and an expression where one was: C#
	// only has the first, and the second is what `int n = if (c) 1 else 2;` needs. The
	// statement is tried first, so `if (c) x = 1; else x = 2;` reads its semicolons the
	// way it looks like it should, and the bare form is what the parse falls back to.
	Branch : @Expression = s: Statement => @(s) | s: Expression => @(s)

	// Which loop a `break` belongs to is the same question a name asks — which block is it
	// written in — and it is answered the same way: the guard records the extent while the
	// text is read, and the jump looks it up when it is built. It has to be that way round
	// here too, because a `break` is built before the loop that holds it.
	While : @Expression
		= "while" & '(' & test: Expression & ')' & body: Statement
		& when @(ExpressionLanguage.Loops(parserSpan))
		=> @(Expression.Loop(
			Expression.Condition(
				test, body, Expression.Break(ExpressionLanguage.Exit(parserSpan)), typeof(void)),
			ExpressionLanguage.Exit(parserSpan),
			ExpressionLanguage.Again(parserSpan)))

	// `Expression.Loop`'s own continue label stands at the top of the body, which is where
	// C# puts it for a `while` and not where it puts it for a `do`: there it goes to the
	// test. So this one places the label itself, with `Expression.Label`, and leaves the
	// loop's own continue unused.
	DoWhile : @Expression
		= "do" & body: Statement & "while" & '(' & test: Expression & ')' & ';'
		& when @(ExpressionLanguage.Loops(parserSpan))
		=> @(Expression.Loop(
			Expression.Block(
				body,
				Expression.Label(ExpressionLanguage.Again(parserSpan)),
				Expression.Condition(
					test,
					Expression.Empty(),
					Expression.Break(ExpressionLanguage.Exit(parserSpan)),
					typeof(void))),
			ExpressionLanguage.Exit(parserSpan)))

	// A `for` is a scope as well as a loop — `int i = 0` belongs to it and not to what is
	// around it — so it records both, and the block that holds the initializer is what
	// declares the variable the initializer assigns.
	For : @Expression
		= "for" & '(' & init: Statement & test: Expression & ';' & step: Expression & ')'
		& body: Statement
		& when @(ExpressionLanguage.Loops(parserSpan) && ExpressionLanguage.Scoped(parserSpan))
		=> @(ExpressionLanguage.Block(
			new[] { init }, parserSpan,
			Expression.Loop(
				Expression.Condition(
					test,
					Expression.Block(body, Expression.Label(ExpressionLanguage.Again(parserSpan)), step),
					Expression.Break(ExpressionLanguage.Exit(parserSpan)),
					typeof(void)),
				ExpressionLanguage.Exit(parserSpan))))

	// A `switch` is what a `break` may name besides a loop, and C# says so — a `break` in a
	// case leaves the switch and not the loop around it. So it records an extent of its own
	// and puts the label the jumps go to after itself.
	Switch : @Expression
		= "switch" & '(' & value: Expression & ')' & '{' & cases: Case* & fallback: Fallback? & '}'
		& when @(ExpressionLanguage.Breaks(parserSpan))
		=> @(Expression.Block(
			Expression.Switch(typeof(void), value, fallback, null, cases),
			Expression.Label(ExpressionLanguage.Exit(parserSpan))))

	Case : @SwitchCase
		= "case" & test: Expression & ':' & body: Statement+
		=> @(Expression.SwitchCase(Expression.Block(body), test))

	Fallback : @Expression = "default" & ':' & body: Statement+ => @(Expression.Block(body))

	Jump : @Expression
		= "break"    => @(Expression.Break(ExpressionLanguage.Exit(parserSpan)))
		| "continue" => @(Expression.Continue(ExpressionLanguage.Again(parserSpan)))

	// ── The operators: C#'s ladder, one rule per level of precedence (§4.3) ─────
	//
	// Read this section from the bottom up and it is the table out of the C# spec, in
	// order and with nothing skipped between a name and `?:`. Every level is left
	// recursive, which is where the associativity is — `10 - 3 - 2` is `(10 - 3) - 2` —
	// except the two C# groups to the right, which are written right recursive.

	Expression : @Expression = e: Assignment => @(e)

	// C# puts assignment lowest of all and groups it to the right, and its left side is a
	// unary expression rather than any expression at all — `a + b = c` is not one. Here it
	// is narrower still: a name, because a name is the only thing this language has that
	// can be written to. That is not only about what is legal. Written as `Unary`, each of
	// these eleven alternatives reads a whole operand before finding out it is not the one,
	// and an operand may be a block — which made a lambda with two braces in it take longer
	// to read than there is time. A name is one word, and eleven words is nothing.
	Assignment : @Expression
		= target: Name & "+="  & value: Assignment => @(Expression.AddAssign(target, value))
		| target: Name & "-="  & value: Assignment => @(Expression.SubtractAssign(target, value))
		| target: Name & "*="  & value: Assignment => @(Expression.MultiplyAssign(target, value))
		| target: Name & "/="  & value: Assignment => @(Expression.DivideAssign(target, value))
		| target: Name & "%="  & value: Assignment => @(Expression.ModuloAssign(target, value))
		| target: Name & "&="  & value: Assignment => @(Expression.AndAssign(target, value))
		| target: Name & "|="  & value: Assignment => @(Expression.OrAssign(target, value))
		| target: Name & "^="  & value: Assignment => @(Expression.ExclusiveOrAssign(target, value))
		| target: Name & "<<=" & value: Assignment => @(Expression.LeftShiftAssign(target, value))
		| target: Name & ">>=" & value: Assignment => @(Expression.RightShiftAssign(target, value))
		| target: Name & '=' & ?!'=' & value: Assignment => @(Expression.Assign(target, value))
		| c: Conditional                           => @(c)

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

	// `++` and `--` before `+` and `-`, so that `--x` is one operator and not two, and over
	// a name for the same reason assignment is: they write to what they read.
	Unary : @Expression
		= "++" & target: Name => @(Expression.PreIncrementAssign(target))
		| "--" & target: Name => @(Expression.PreDecrementAssign(target))
		| '-' & operand: Unary => @(Expression.Negate(operand))
		| '+' & operand: Unary => @(Expression.UnaryPlus(operand))
		| '!' & operand: Unary => @(Expression.Not(operand))
		| '~' & operand: Unary => @(Expression.OnesComplement(operand))

		// A cast is told from a parenthesized expression by what stands inside it: every
		// type here is a keyword, and a keyword is no name. C# needs a rule of its own to
		// decide this because a type there may be a name; a language whose types are a
		// closed set of keywords does not.
		| '(' & type: Type & ')' & operand: Unary => @(Expression.Convert(operand, type))

		| p: Postfix => @(p)

	// The one level C# has that this one had no need of until there was something to assign
	// to, and over a name for the same reason.
	Postfix : @Expression
		= target: Name & "++" => @(Expression.PostIncrementAssign(target))
		| target: Name & "--" => @(Expression.PostDecrementAssign(target))
		| p: Primary          => @(p)

	Primary : @Expression
		= '(' & inner: Expression & ')' => @(inner)

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

		| n: Name    => @(n)

	Name : @Expression = name: Word => @(ExpressionLanguage.Named(name, parserSpan))

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
		Begin();

		return ParseLambda(text);
	}

	/// <summary>The same, answering rather than throwing where the text is not this language.</summary>
	/// <remarks>
	/// The generated <c>TryParseLambda</c> beside it is the parser and nothing else, and
	/// what this adds is the one thing a parser cannot know: that a new parse is beginning,
	/// and that everything the last one wrote down about names, blocks and loops is now
	/// about a text nobody is reading. Call it rather than the generated one.
	/// </remarks>
	public static Match<LambdaExpression> TryParse(string text)
	{
		Begin();

		return TryParseLambda(text);
	}

	/// <summary>Forget the parse before this one. Every list here is keyed by position.</summary>
	static void Begin()
	{
		_declared?.Clear();
		_scopes?.Clear();
		_loops?.Clear();
		_breakables?.Clear();
		_exits?.Clear();
		_agains?.Clear();
		_returns = null;
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
	static Scope? Holding(int position) => Innermost(_scopes, position);

	/// <summary>The innermost of these extents holding a position, or none.</summary>
	static Scope? Innermost(List<Scope>? among, int position)
	{
		var innermost = default(Scope?);

		foreach (var scope in among ?? [])
			if (scope.From <= position && position < scope.To &&
				(innermost is not { } known || scope.From > known.From))
				innermost = scope;

		return innermost;
	}

	// ── Where a break and a continue go ─────────────────────────────────────────
	//
	// The same question as a name's, and the same answer: which one a jump belongs to is
	// where it is written. A `break` may name a loop or a switch and a `continue` only a
	// loop, which is C#'s rule and the reason these are two lists. Both are read while the
	// text is; the labels themselves are made where they are first asked for, because a
	// jump is built before the thing it jumps out of.

	[ThreadStatic]
	static List<Scope>? _loops;

	[ThreadStatic]
	static List<Scope>? _breakables;

	[ThreadStatic]
	static Dictionary<int, LabelTarget>? _exits;

	[ThreadStatic]
	static Dictionary<int, LabelTarget>? _agains;

	/// <summary>A loop, which a <c>break</c> and a <c>continue</c> may both name.</summary>
	public static bool Loops(SourceSpan span)
	{
		(_loops ??= []).Add(new Scope(span.Start, span.Start + span.Length));

		return Breaks(span);
	}

	/// <summary>A switch, which only a <c>break</c> may name.</summary>
	public static bool Breaks(SourceSpan span)
	{
		(_breakables ??= []).Add(new Scope(span.Start, span.Start + span.Length));

		return true;
	}

	/// <summary>Where a <c>break</c> written here goes.</summary>
	public static LabelTarget Exit(SourceSpan at) =>
		Labelled(
			_exits ??= [],
			Innermost(_breakables, at.Start) ??
				throw new FormatException("a 'break' here is inside no loop and no switch."),
			"break");

	/// <summary>Where a <c>continue</c> written here goes.</summary>
	public static LabelTarget Again(SourceSpan at) =>
		Labelled(
			_agains ??= [],
			Innermost(_loops, at.Start) ??
				throw new FormatException("a 'continue' here is inside no loop."),
			"continue");

	/// <summary>One label per extent, made the first time anything asks for it.</summary>
	static LabelTarget Labelled(Dictionary<int, LabelTarget> labels, Scope? of, string name)
	{
		var at = of!.Value.From;

		if (!labels.TryGetValue(at, out var target))
			labels[at] = target = Expression.Label(name);

		return target;
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

	/// <summary>An <c>if</c> with an <c>else</c>, worth what its branches agree on.</summary>
	/// <remarks>
	/// <c>Expression.Condition</c> is one factory with two answers — the branches' own type,
	/// or <c>typeof(void)</c> where they have none in common — and which of them a given
	/// <c>if</c> could have meant is a question about this API and not about the language.
	/// The grammar says what an `if` is, in the words every language uses for it; this says
	/// what that turns into here. Written the other way round, the grammar would have to
	/// carry a distinction that only <c>System.Linq.Expressions</c> makes.
	/// </remarks>
	public static Expression Chosen(Expression test, Expression then, Expression otherwise)
	{
		if (then is null)
			throw new ArgumentNullException(nameof(then));

		if (otherwise is null)
			throw new ArgumentNullException(nameof(otherwise));

		return Expression.Condition(
			test, then, otherwise, then.Type == otherwise.Type ? then.Type : typeof(void));
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

		// Where the body is worth what a `return` is worth, the label takes it: control that
		// reaches the end of the body arrives at the label, and what the label is worth
		// there is what fell into it. Written the other way — the label after the body,
		// with a default — the body's own value is computed and thrown away, and a lambda
		// that ends in an expression answers `default` instead of it.
		//
		// Where the body is worth nothing, because every path out of it is a `return`, there
		// is nothing to fall through with and the default is the only thing to put there.
		return _returns is null
			? body
			: body.Type == _returns.Type
				? Expression.Label(_returns, body)
				: Expression.Block(body, Expression.Label(_returns, Expression.Default(_returns.Type)));
	}

	/// <summary>
	/// A block: the variables it declared, its statements, and the expression it is worth.
	/// </summary>
	/// <remarks>
	/// The variables are the declarations this block holds — the ones whose innermost
	/// block is this one — and not, as they once were, whatever the statements assign to.
	/// That reading was right while a declaration was the only thing that could assign,
	/// and stopped being right the moment `a = 1;` was a statement: it collected the same
	/// variable twice and the tree said so.
	/// </remarks>
	public static Expression Block(Expression[] statements, SourceSpan at, Expression? value)
	{
		if (statements is null)
			throw new ArgumentNullException(nameof(statements));

		var variables = new List<ParameterExpression>();

		foreach (var declaration in _declared ?? [])
			if (Holding(declaration.At) is { } held && held.From == at.Start)
				variables.Add(declaration.Variable);

		var body = new List<Expression>(statements);

		if (value is not null)
			body.Add(value);

		// Nothing decides what the block is worth beyond this: `Expression.Block` is worth
		// its last expression, whatever that turns out to be. A trailing expression is one,
		// an `if` whose branches agree is one, a statement whose value nobody wanted is one
		// that nobody reads, and a block ending in a `return` never reaches its end at all.
		if (body.Count == 0)
			throw new FormatException("a block has to hold something.");

		return Expression.Block(variables, body);
	}
}
