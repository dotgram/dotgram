using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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
// **What one reading works out lives in `State`, which the grammar declares as its
// `context` (§7.7) and the caller hands over.** It used to be seven `[ThreadStatic]` fields
// and a `Begin` that cleared them — a discipline rather than a guarantee, and one that
// failed: the generated `TryParseLambda` never called it, so one parse's blocks were still
// standing when the next one asked. Nothing there can be forgotten now, because nothing
// survives the call.
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

	// What this reading works out, handed over by the caller and living exactly as long as
	// the call. Everything the host used to keep in a thread-static field is a field of
	// this, which is why nothing has to be cleared between one parse and the next.
	context : @State

	// What a piece of this text is being read under, as against what the reading works
	// out (§7.8). Overflow is the only thing here that needs it today, and the type is
	// the grammar's rather than that concern's: a second one would be more values, not a
	// second declaration.
	state : @Reading

	namespace Lexical
	{
		trivia = none

		Word = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_']*

		// What this language reserves. `Name` refuses these, which is the whole of what
		// makes a keyword one: C# says an identifier is a word that is not a keyword, and
		// leaving it to the order of alternatives only works where the keyword's own
		// reading is tried first. `checked(x + 1)` is where that broke — a keyword followed
		// by a parenthesized expression is indistinguishable from a call until something
		// says the word is not a name, and `Postfix` reads a call before `Primary` is
		// reached at all.
		//
		// The boundary is written here rather than left to §4.6, which weaves one beside a
		// word literal standing where a match is *taken* — this one stands inside a
		// lookahead that takes nothing, and `checkedTotal` was refused for beginning with
		// `checked` until the boundary was said out loud. Whether the weaving should reach
		// inside a lookahead is a question for the notation; saying it here is right either
		// way, because what this rule means is "one of these words, whole".
		Keyword
			= ("as"      | "bool"    | "break"   | "byte"      | "case"   | "catch"
			|  "char"    | "checked" | "continue"| "decimal"   | "default"| "do"
			|  "double"  | "else"    | "false"   | "finally"   | "float"  | "for"
			|  "if"      | "int"     | "is"      | "long"      | "new"    | "null"
			|  "object"  | "return"  | "sbyte"   | "short"     | "string" | "switch"
			|  "throw"   | "true"    | "try"     | "uint"      | "ulong"  | "unchecked"
			|  "ushort"  | "while")
			& ?![\p{L} | \p{Nd} | '_']

		// ── Numbers, written the way C# writes them ─────────────────────────────────

		Digit    = ['0'..'9']
		HexDigit = ['0'..'9' | 'a'..'f' | 'A'..'F']
		BinDigit = ['0' | '1']

		// A separator stands between digits and is no part of the value, so every rule
		// below hands back the digits with them taken out: `long.Parse` reads a number,
		// not a number and an underscore.
		DecRun = { Digit  & ('_'* & Digit)* }
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
			context.Returning(body), ExpressionLanguage.Taking(first, rest)))

	// Each type names itself in C#, so `typeof(int)` is checked where it is written and
	// a word that is no type is not a declaration — the grammar refusing that reading
	// rather than a switch over strings refusing it at run time.
	// A type is a name for one, and then as many `[]` as the author wrote. Left recursive,
	// so `int[][]` is read once and folded rather than started over.
	Type : @Type = t: Type & "[]" => @(t.MakeArrayType())
	             | c: Core        => @(c)

	Core : @Type = "sbyte"   => @(typeof(sbyte))
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
	             | t: NamedType => @(t)

	// The keywords above are written where the C# compiler reads them. A name is not a
	// keyword and cannot be: what `Exception` means is a question about the namespaces this
	// host was told to look in, and it is asked while the text is read so that the answer
	// can decide how the text reads.
	//
	// The generic form asks nothing while reading, and cannot: what a guard may look at is
	// what the text said, and the arguments here are types the `=>` has not built yet. It
	// needs no guard either — nothing else in this language is a name followed by `<`, a
	// type, and `>`, so a reading that gets that far is a generic type or is nothing.
	// The type arguments are an optional tail rather than a second alternative: written as
	// two, the dotted name is read once for each, and it is the most expensive operand
	// here. One reading is the same language because arguments begin with '<', which a
	// dotted name cannot contain.
	//
	// The guard keeps the place it had, which is load-bearing: a generic form needs no
	// name that resolves on its own — `List<int>` resolves and `List` does not — so it
	// asks only where there are no arguments to say what the name is.
	// The dotted name is read here rather than lexed, and that is a correction. As a lexeme
	// it was one unit that had to hand its own tail back when the guard below said the whole
	// of it named no type — `Math.PI` read as `Math.PI`, refused, then re-read as `Math` with
	// `.PI` left for member access. That works only where a lexeme may be taken apart again,
	// which is to say only over characters: a tokenizer decides where a token ends once, and
	// `Math.PI` arriving whole is a member access that can never be read.
	//
	// Written as words with dots between them it is the same language and gives the same
	// answer by the same means — the repetition hands a turn back where the lexeme handed a
	// suffix back — and the parts are captured rather than the run, so `System . Text` names
	// `System.Text` and the spaces the author put in are nowhere in the string.
	// One word of a dotted name, given a type so that the parts arrive one at a time. A bare
	// `part: Word` under a repetition captures the run between the first and the last, spaces
	// and dots and all; a typed part is an array of words, and a name assembled from those
	// has nothing in it the author did not name.
	NamePart : @string = w: Word => @(w)

	NamedType : @Type
		= head: Word & ('.' & part: NamePart)*
		  & args: ('<' & first: Type & (',' & rest: Type)* & '>')?
		  & when @(args != null || ExpressionLanguage.Resolves(ExpressionLanguage.Dotted(head, part)))
		  => @(args is null
		       ? ExpressionLanguage.TypeNamed(ExpressionLanguage.Dotted(head, part))
		       : ExpressionLanguage.Generic(
		           ExpressionLanguage.Dotted(head, part), ExpressionLanguage.Types(first!, rest)))

	// One rule for every argument list there is, so that a call, a constructor and an
	// indexer all say it the same way and each hands the API one array.
	Arguments : @Expression[]
		= '(' & (first: Expression & (',' & rest: Expression)*)? & ')'
		=> @(ExpressionLanguage.Listed(first, rest))

	// What a member initializer sets, as the text said it: the member's name and the value,
	// with which member that is left until the type is known — which is at construction,
	// where the type is.
	Bindings : @Setting[]
		= '{' & first: Binding & (',' & rest: Binding)* & '}'
		=> @(ExpressionLanguage.Set(first, rest))

	// Three things one syntax says, told apart by what stands after the `=` — a value, a
	// nested initializer of members, or a nested one of elements. One route rather than
	// three alternatives, for the reason `Primary`'s `new` gives: three would read the
	// name and the `=` three times over, and the third reading holds a whole expression.
	//
	// `Bindings` is tried before the braced elements because it is the narrower of the
	// two: it wants `Word =` inside, and where that is absent the same brace opens a list.
	// One token of lookahead settles it.
	Binding : @Setting
		= name: Word & '='
		& (nested: Bindings | '{' & items: Elements & '}' | value: Expression)
		=> @(new Setting(name, value, nested, items))

	Elements : @Element[]
		= first: Element & (',' & rest: Element)*
		=> @(ExpressionLanguage.Listed(first, rest))

	// An element is what one call to `Add` takes, which is usually one expression and for
	// a dictionary is two. C# writes the second in braces of its own, and the API has a
	// node for it — `ElementInit` — because a collection whose `Add` takes two arguments
	// cannot be described by a list of values.
	Element : @Element
		= '{' & first: Expression & (',' & rest: Expression)* & '}'
		  => @(new Element(ExpressionLanguage.Listed(first, rest)))
		| only: Expression => @(ExpressionLanguage.Only(only))

	Indices : @Expression[]
		= '[' & first: Expression & (',' & rest: Expression)* & ']'
		=> @(ExpressionLanguage.Listed(first, rest))

	// The guard is the declaration: it runs while the text is read, which is the only
	// moment this grammar has in the order it is written — and `parserSpan` is where it
	// was read, which is the only thing that can say later which block it belongs to.
	Parameter : @ParameterExpression
		= type: Type & name: Word & when @(context.Takes(type, name, parserSpan))
		=> @(context.Named(name, parserSpan))

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
		& when @(context.Scoped(parserSpan))
		=> @(context.Block(statements, parserSpan, value))

	Statement : @Expression
		= s: Local            => @(s)
		| s: Return           => @(s)
		| s: Block            => @(s)
		| s: Control          => @(s)
		| s: Jump & ';'       => @(s)
		| s: Expression & ';' => @(s)

	Local : @Expression
		= type: Type & name: Word & when @(context.Declare(type, name, parserSpan))
		& '=' & value: Value & ';'
		=> @(Expression.Assign(context.Named(name, parserSpan), value))

	Return : @Expression = "return" & value: Value & ';'
	                     => @(context.Return(value))

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
		= c: Try     => @(c)
		| c: If      => @(c)
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
		& when @(context.Loops(parserSpan))
		=> @(Expression.Loop(
			Expression.Condition(
				test, body, Expression.Break(context.Exit(parserSpan)), typeof(void)),
			context.Exit(parserSpan),
			context.Again(parserSpan)))

	// `Expression.Loop`'s own continue label stands at the top of the body, which is where
	// C# puts it for a `while` and not where it puts it for a `do`: there it goes to the
	// test. So this one places the label itself, with `Expression.Label`, and leaves the
	// loop's own continue unused.
	DoWhile : @Expression
		= "do" & body: Statement & "while" & '(' & test: Expression & ')' & ';'
		& when @(context.Loops(parserSpan))
		=> @(Expression.Loop(
			Expression.Block(
				body,
				Expression.Label(context.Again(parserSpan)),
				Expression.Condition(
					test,
					Expression.Empty(),
					Expression.Break(context.Exit(parserSpan)),
					typeof(void))),
			context.Exit(parserSpan)))

	// A `for` is a scope as well as a loop — `int i = 0` belongs to it and not to what is
	// around it — so it records both, and the block that holds the initializer is what
	// declares the variable the initializer assigns.
	For : @Expression
		= "for" & '(' & init: Statement & test: Expression & ';' & step: Expression & ')'
		& body: Statement
		& when @(context.Loops(parserSpan) && context.Scoped(parserSpan))
		=> @(context.Block(
			new[] { init }, parserSpan,
			Expression.Loop(
				Expression.Condition(
					test,
					Expression.Block(body, Expression.Label(context.Again(parserSpan)), step),
					Expression.Break(context.Exit(parserSpan)),
					typeof(void)),
				context.Exit(parserSpan))))

	// A `switch` is what a `break` may name besides a loop, and C# says so — a `break` in a
	// case leaves the switch and not the loop around it. So it records an extent of its own
	// and puts the label the jumps go to after itself.
	Switch : @Expression
		= "switch" & '(' & value: Expression & ')' & '{' & cases: Case* & fallback: Fallback? & '}'
		& when @(context.Breaks(parserSpan))
		=> @(Expression.Block(
			Expression.Switch(typeof(void), value, fallback, null, cases),
			Expression.Label(context.Exit(parserSpan))))

	Case : @SwitchCase
		= "case" & test: Expression & ':' & body: Statement+
		=> @(Expression.SwitchCase(Expression.Block(body), test))

	Fallback : @Expression = "default" & ':' & body: Statement+ => @(Expression.Block(body))

	Jump : @Expression
		= "break"                     => @(Expression.Break(context.Exit(parserSpan)))
		| "continue"                  => @(Expression.Continue(context.Again(parserSpan)))
		| "throw" & value: Expression => @(Expression.Throw(value))
		| "throw"                     => @(Expression.Rethrow())

	// Three factories and three shapes, so the grammar says which by what is written and
	// nothing here has to ask. The bodies are blocks and so are worth something, which
	// `TryCatch` requires them to agree on — the API's rule, in the API's words.
	Try : @Expression =
		  "try" & body: Block & handlers: Catch+ & "finally" & final: Block => @(Expression.TryCatchFinally(body, final, handlers))
		| "try" & body: Block & handlers: Catch+                            => @(Expression.TryCatch(body, handlers))
		| "try" & body: Block &                    "finally" & final: Block => @(Expression.TryFinally(body, final))

	// The caught variable belongs to the handler and not to what is around it, so the
	// `catch` records a scope of its own — the `(` it is declared in stands outside the
	// handler's block, and without this the block around the `try` would claim it.
	Catch : @CatchBlock
		= "catch" & '(' & type: Type & name: Word
		& when @(context.Declare(type, name, parserSpan)) & ')' & body: Block
		& when @(context.Scoped(parserSpan))
		=> @(Expression.Catch(context.Named(name, parserSpan), body))

	// ── The operators: C#'s ladder, one rule per level of precedence (§4.3) ─────
	//
	// Read this section from the bottom up and it is the table out of the C# spec, in
	// order and with nothing skipped between a name and `?:`. Every level is left
	// recursive, which is where the associativity is — `10 - 3 - 2` is `(10 - 3) - 2` —
	// except the two C# groups to the right, which are written right recursive.

	Expression : @Expression = e: Assignment => @(e)

	// C# puts assignment lowest of all and groups it to the right, and its left side is a
	// unary expression rather than any expression at all — `a + b = c` is not one. Here it
	// is narrower still, and deliberately: a name, a member of a name, or an element of
	// one. Written as `Unary`, each of these eleven alternatives would read a whole operand
	// before finding out it is not the one, and an operand may be a block — which made a
	// lambda with two braces in it take longer to read than there is time.
	Assignment : @Expression
		// An element is written to by one alternative and not by eleven, and that is a
		// measurement: an index is an expression, eleven alternatives read it eleven times
		// before finding out which operator they are, and `a[a[a[a[0]]]] = 1` took most of
		// a second. So a compound assignment writes to a name or a member of one, and only
		// the plain `=` writes to an element.
		= target: Name & at: Indices & '=' & ?!'=' & value: Assignment
		  => @(Expression.Assign(ExpressionLanguage.Place(target, at), value))

		| target: Target & "+="  & value: Assignment
		  => @(ExpressionLanguage.AddAssign(target, value, parserState))
		| target: Target & "-="  & value: Assignment
		  => @(ExpressionLanguage.SubtractAssign(target, value, parserState))
		| target: Target & "*="  & value: Assignment
		  => @(ExpressionLanguage.MultiplyAssign(target, value, parserState))
		| target: Target & "/="  & value: Assignment => @(Expression.DivideAssign(target, value))
		| target: Target & "%="  & value: Assignment => @(Expression.ModuloAssign(target, value))
		| target: Target & "&="  & value: Assignment => @(Expression.AndAssign(target, value))
		| target: Target & "|="  & value: Assignment => @(Expression.OrAssign(target, value))
		| target: Target & "^="  & value: Assignment => @(Expression.ExclusiveOrAssign(target, value))
		| target: Target & "<<=" & value: Assignment => @(Expression.LeftShiftAssign(target, value))
		| target: Target & ">>=" & value: Assignment => @(Expression.RightShiftAssign(target, value))
		| target: Target & '=' & ?!'=' & value: Assignment => @(Expression.Assign(target, value))
		| c: Conditional                           => @(c)

	// What may be written to. An element is read one way and written another — `ArrayIndex`
	// answers with a value and `ArrayAccess` with a place — and which is wanted is decided
	// by where it stands, which is a thing the grammar knows and the API does not.
	// The member is an optional tail rather than a second alternative: written as two, the
	// name is read once for each and so is every alternative of `Assignment` that begins
	// with this. One reading is the same language here because a member begins with '.',
	// which a name cannot contain.
	Target : @Expression
		= n: Name & ('.' & member: Word)?
		=> @(member is null ? n : ExpressionLanguage.Member(n, member))

	// `?:` groups to the right and its condition is one level tighter, so `a ?? b ? c : d`
	// is `(a ?? b) ? c : d` and `a ? b : c ? d : e` is `a ? b : (c ? d : e)`.
	//
	// **The tail is optional rather than the whole thing being two alternatives**, and that
	// is the difference between this parser and one that cannot be used. Written as
	// `test: Coalesce & '?' & … | c: Coalesce`, each of these two rules reads its operand
	// once to look for an operator that is usually not there and once more to hand it on —
	// so each doubles per level of nesting, and the two of them together multiplied the
	// cost of a parenthesis by four. `(((0 + 1) + 1) + 1)` nine deep, sixty-eight
	// characters, took **thirty seconds**; with the tail optional it takes 0.39 ms.
	//
	// The left-recursive levels below never had this: §4.3 folds them, and a fold reads its
	// operand once by construction. These two are the only right-associative ones, which is
	// why they are the only two that were written this way.
	Conditional : @Expression
		= test: Coalesce & ('?' & then: Conditional & ':' & otherwise: Conditional)?
		  => @(ExpressionLanguage.Chosen(test, then, otherwise))

	Coalesce : @Expression
		= left: Or & ("??" & right: Coalesce)?
		  => @(ExpressionLanguage.Coalesced(left, right))

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
	//
	// The shift below is written as two `>` glued rather than as one `">>"`, and that is
	// what lets `List<List<int>>` close two argument lists with the same two characters
	// C# closes them with. A literal `">>"` is a token, and a token cannot be half spent:
	// the type argument list wants one `>` and would be handed a shift. Written this way
	// there is no `>>` for the lexer to make, `~` says the two stand with nothing between
	// them, and `a > > b` is refused exactly as C# refuses it.
	Relational : @Expression
		= left: Relational & "is" &        type : Type  => @(Expression.TypeIs(left, type))
		| left: Relational & "as" &        type : Type  => @(Expression.TypeAs(left, type))
		| left: Relational & "<=" &        right: Shift => @(Expression.LessThanOrEqual(left, right))
		| left: Relational & ">=" &        right: Shift => @(Expression.GreaterThanOrEqual(left, right))
		| left: Relational & '<' & ?!'<' & right: Shift => @(Expression.LessThan(left, right))
		| left: Relational & '>' & ?!'>' & right: Shift => @(Expression.GreaterThan(left, right))
		| s: Shift                                      => @(s)

	Shift : @Expression
		= left: Shift & '<' ~ '<' & right: Additive => @(Expression.LeftShift(left, right))
		| left: Shift & '>' ~ '>' & right: Additive => @(Expression.RightShift(left, right))
		| a: Additive                               => @(a)

	Additive : @Expression
		= left: Additive & '+' & right: Multiplicative
		  => @(ExpressionLanguage.Add(left, right, parserState))
		| left: Additive & '-' & right: Multiplicative
		  => @(ExpressionLanguage.Subtract(left, right, parserState))
		| m: Multiplicative                            => @(m)

	Multiplicative : @Expression
		= left: Multiplicative & '*' & right: Unary
		  => @(ExpressionLanguage.Multiply(left, right, parserState))
		| left: Multiplicative & '/' & right: Unary => @(Expression.Divide(left, right))
		| left: Multiplicative & '%' & right: Unary => @(Expression.Modulo(left, right))
		| u: Unary                                  => @(u)

	// `++` and `--` before `+` and `-`, so that `--x` is one operator and not two, and over
	// a name for the same reason assignment is: they write to what they read.
	Unary : @Expression
		= "++" & target: Name => @(Expression.PreIncrementAssign(target))
		| "--" & target: Name => @(Expression.PreDecrementAssign(target))
		| '-' & operand: Unary => @(ExpressionLanguage.Negate(operand, parserState))
		| '+' & operand: Unary => @(Expression.UnaryPlus(operand))
		| '!' & operand: Unary => @(Expression.Not(operand))
		| '~' & operand: Unary => @(Expression.OnesComplement(operand))

		// A cast is told from a parenthesized expression by what stands inside it: every
		// type here is a keyword, and a keyword is no name. C# needs a rule of its own to
		// decide this because a type there may be a name; a language whose types are a
		// closed set of keywords does not.
		| '(' & type: Type & ')' & operand: Unary
		  => @(ExpressionLanguage.Cast(operand, type, parserState))

		| p: Postfix => @(p)

	// Everything written after an operand rather than before it. Left recursive, which is
	// what makes `a.b.c(d)[0]` one chain read once — §4.3 reads the operand at the head of
	// it and then folds the suffixes on, rather than starting over for each.
	//
	// `Expression.Call` takes a method by its name and chooses the overload itself, and
	// `Expression.PropertyOrField` answers the same question for the other two. That is
	// most of why so little of this is written here: the API's own resolution is better
	// than one this could invent, and it reports what it could not find in its own words.
	Postfix : @Expression
		= target: Postfix & '.' & member: Word & args: Arguments
		  => @(Expression.Call(target, member, null, args))

		| target: Postfix & '.' & member: Word => @(ExpressionLanguage.Member(target, member))

		// Written through a rule of its own rather than inline, for a reason that is a
		// generator defect and not a taste: a capture whose rule leads back into this fold
		// — `index: Expression` here — comes out typed as a sequence with the fold's own
		// operand dropped, and the emitted C# does not compile. `Arguments` above has the
		// same shape and is fine, which is what said to write this one the same way. The
		// two-line reduction is in docs/next.md.
		//
		// It reads better for it: an index is a list, so a two-dimensional array and an
		// indexer of two arguments are both written without another rule.
		| target: Postfix & at: Indices => @(ExpressionLanguage.Indexed(target, at))

		| target: Name & args: Arguments => @(Expression.Invoke(target, args))

		| target: Name & "++" => @(Expression.PostIncrementAssign(target))
		| target: Name & "--" => @(Expression.PostDecrementAssign(target))
		| p: Primary          => @(p)

	Primary : @Expression
		= "new" & type: Type & '[' & size: Expression & ']'
		  => @(Expression.NewArrayBounds(type, size))
		// `"[]"` and not `'[' & ']'`, which is the same thing said the other way and the way
		// that cannot be read as tokens: a lexer takes the longest match, `Type` above names
		// `"[]"` as one, and two marks written apart here are one token by the time this rule
		// sees them. One spelling for one thing.
		| "new" & type: Type & "[]"
		  & '{' & (first: Expression & (',' & rest: Expression)*)? & '}'
		  => @(Expression.NewArrayInit(type, ExpressionLanguage.Listed(first, rest)))
		// An initializer is written after the constructor's own arguments, and which of the
		// two it is is what stands inside the braces: `Name = value` sets a member, and an
		// expression is an element to add. Both are one optional tail rather than three
		// alternatives, for the reason `Conditional` gives above — three alternatives read
		// the arguments three times before finding out which they are, and the arguments
		// hold whole expressions. Nine nested `new`s took a second that way.
		| "new" & type: Type & args: Arguments
		  & (fields: Bindings | '{' & items: Elements & '}')?
		  => @(ExpressionLanguage.Made(type, args, fields, items))

		// A type, then something of it. Told from `a.b` by the guard inside `NamedType`,
		// which is the same question C# answers with a section of its own — a dotted name
		// is a type where it names one, and an expression where it does not.
		// The arguments are an optional tail rather than a second alternative: written as
		// two, the type and the member are read once for each, and a dotted type name is
		// not cheap to read. One reading is the same language because arguments begin with
		// '(', which nothing at the end of a member name can be.
		| type: NamedType & '.' & member: Word & args: Arguments?
		  => @(args is null
		       ? ExpressionLanguage.StaticMember(type, member)
		       : Expression.Call(type, member, null, args))

		// §7.8, and the one thing in this language that changes what a construction builds
		// without changing anything about what is read. The operand is an ordinary
		// expression, read by the ordinary rules; the mark stands over it and the
		// arithmetic below asks what it stands under.
		| "checked"   & '(' & inner: Expression with state @(Reading.Checked)   & ')' => @(inner)
		| "unchecked" & '(' & inner: Expression with state @(Reading.Unchecked) & ')' => @(inner)

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
		//
		// A pair like this is what the fold's committed residue exists for. Both
		// alternatives read the same digits, so past them the choice is about which
		// factory runs and not about the text — and left uncommitted it once left a live
		// way back at every literal, which made refusing exponential: 2^(literals)
		// rereadings of everything after, 74/327/1299 us at two, four and six
		// parentheses. `ExpressionBenchmarks` holds the refusals that would say if that
		// ever comes back.
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

	Name : @Expression = ?!Keyword & name: Word => @(context.Named(name, parserSpan))

	parse Lambda as ParseLambda
	""", Lexical = true)]
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
	public static LambdaExpression Parse(string text) => ParseLambda(text, new State());

	/// <summary>The same, answering rather than throwing where the text is not this language.</summary>
	/// <remarks>
	/// The generated <c>TryParseLambda</c> beside it is the parser and nothing else, and
	/// what this adds is the one thing a parser cannot know: that a new parse is beginning,
	/// and that everything the last one wrote down about names, blocks and loops is now
	/// about a text nobody is reading. Call it rather than the generated one.
	/// </remarks>
	public static Match<LambdaExpression> TryParse(string text) => TryParseLambda(text, new State());


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

	// ── What a name written as a type means ─────────────────────────────────────
	//
	// The keywords are the grammar's, written as `typeof(int)` where the C# compiler reads
	// them. A name is not: `Exception` means something only against a set of namespaces to
	// look in, and that set is the host's to hold — it is what a `using` is, and no grammar
	// can carry one for an API it has not been pointed at yet.

	static readonly List<string> _namespaces = ["System"];

	static readonly ConcurrentDictionary<string, Type?> _resolved = new(StringComparer.Ordinal);

	/// <summary>Look for type names in this namespace as well, the way a <c>using</c> does.</summary>
	public static void Using(string @namespace)
	{
		if (string.IsNullOrWhiteSpace(@namespace))
			throw new ArgumentException("A namespace to search cannot be empty.", nameof(@namespace));

		lock (_namespaces)
		{
			if (_namespaces.Contains(@namespace, StringComparer.Ordinal))
				return;

			_namespaces.Add(@namespace);
		}

		// What was not found before may be found now, and what was found still is.
		_resolved.Clear();
	}

	/// <summary>Whether this name is a type here, asked while the text is read (§8.1).</summary>
	/// <remarks>
	/// This is what tells `(Foo)x` from `(foo)`, which C# needs a rule of its own for: a
	/// parenthesized name is a cast where the name is a type and an expression where it is
	/// not, and the guard answering no is what sends the parse to the other reading.
	/// </remarks>
	public static bool Resolves(string name) => Lookup(name) is not null;

	/// <summary>A dotted name from the words the grammar read, and nothing between them.</summary>
	/// <remarks>
	/// The parts and not the run: the words are captured one at a time, so whatever spacing
	/// stood between them in the text is not in the name. `System . Text` is `System.Text`,
	/// which is what it means and what the lookup below can answer about.
	/// </remarks>
	public static string Dotted(string head, string[]? tail) =>
		tail is null || tail.Length == 0 ? head : head + "." + string.Join(".", tail);

	/// <summary>The type that name means.</summary>
	/// <exception cref="FormatException">It means none.</exception>
	public static Type TypeNamed(string name) =>
		Lookup(name) ?? throw new FormatException($"there is no type named '{name}' here.");

	/// <summary>The name against the namespaces, then against every assembly loaded.</summary>
	static Type? Lookup(string name)
	{
		if (_resolved.TryGetValue(name, out var known))
			return known;

		string[] tries;

		lock (_namespaces)
			tries = [name, .. _namespaces.Select(space => space + "." + name)];

		var found = tries.Select(one => Type.GetType(one, false, false)).FirstOrDefault(one => one is not null);

		if (found is null)
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				found = tries.Select(one => assembly.GetType(one, false, false))
					.FirstOrDefault(one => one is not null);

				if (found is not null)
					break;
			}

		return _resolved[name] = found;
	}

	/// <summary>What a piece of the text is being read under — the grammar's `state` (§7.8).</summary>
	/// <remarks>
	/// One type for the whole grammar, which is what §7.8 asks: a second concern would be
	/// more values here rather than a second declaration, and what tells two concerns apart
	/// is that a reader of one walks past the values of the other. <see cref="Checked"/>
	/// asks for the arithmetic that throws on overflow; <see cref="Unchecked"/> asks for
	/// the arithmetic that wraps, and exists so that it can be asked for again inside a
	/// `checked` that already stands over it.
	/// </remarks>
	public enum Reading
	{
		Checked,
		Unchecked,
	}

	/// <summary>Whether the nearest mark that speaks about overflow says to check it.</summary>
	/// <remarks>
	/// Read from the end, which is the nearest, and stopping at the first value of this
	/// concern rather than at the first value: that is what lets a mark about something
	/// else stand between a `checked` and the arithmetic under it without hiding it. C#'s
	/// default is unchecked, and so is the answer where nothing says otherwise.
	/// </remarks>
	static bool Checked(ReadOnlySpan<Reading> reading)
	{
		for (var i = reading.Length - 1; i >= 0; i--)
			switch (reading[i])
			{
				case Reading.Checked   : return true;
				case Reading.Unchecked : return false;
			}

		return false;
	}

	// The eight nodes `System.Linq.Expressions` has two of. Written here rather than in the
	// grammar for the reason everything else in this class is: the grammar says what a `+`
	// is, in the word every language uses for it, and the host says what a `+` turns into
	// here. A conditional written eight times into the notation would have said the same
	// thing worse, and would have put a C# question — which overload — where a reader is
	// looking for the shape of an expression.

	public static Expression Add(Expression left, Expression right, ReadOnlySpan<Reading> reading) =>
		Checked(reading) ? Expression.AddChecked(left, right) : Expression.Add(left, right);

	public static Expression Subtract(Expression left, Expression right, ReadOnlySpan<Reading> reading) =>
		Checked(reading) ? Expression.SubtractChecked(left, right) : Expression.Subtract(left, right);

	public static Expression Multiply(Expression left, Expression right, ReadOnlySpan<Reading> reading) =>
		Checked(reading) ? Expression.MultiplyChecked(left, right) : Expression.Multiply(left, right);

	public static Expression Negate(Expression operand, ReadOnlySpan<Reading> reading) =>
		Checked(reading) ? Expression.NegateChecked(operand) : Expression.Negate(operand);

	/// <remarks>
	/// A cast is where the difference is most visible and least like the others: `(byte)300`
	/// is 44 unchecked and throws checked, and neither is an error the C# compiler would
	/// have caught here — the value is not a constant until the tree is compiled.
	/// </remarks>
	public static Expression Cast(Expression operand, Type type, ReadOnlySpan<Reading> reading) =>
		Checked(reading) ? Expression.ConvertChecked(operand, type) : Expression.Convert(operand, type);

	public static Expression AddAssign(Expression target, Expression value, ReadOnlySpan<Reading> reading) =>
		Checked(reading)
			? Expression.AddAssignChecked(target, value)
			: Expression.AddAssign(target, value);

	public static Expression SubtractAssign(
		Expression target, Expression value, ReadOnlySpan<Reading> reading) =>
		Checked(reading)
			? Expression.SubtractAssignChecked(target, value)
			: Expression.SubtractAssign(target, value);

	public static Expression MultiplyAssign(
		Expression target, Expression value, ReadOnlySpan<Reading> reading) =>
		Checked(reading)
			? Expression.MultiplyAssignChecked(target, value)
			: Expression.MultiplyAssign(target, value);

	/// <summary>What <c>a.b</c> reads, which the type of <c>a</c> decides.</summary>
	/// <remarks>
	/// An array's length is a node of this tree — <c>ArrayLength</c> — where every other
	/// type's is a property, and nothing in the syntax says which. It could not be a guard
	/// either: a guard runs while the text is read and the operand of a fold is not built
	/// until after, so the only place that can ask the operand what it is, is here.
	/// </remarks>
	public static Expression Member(Expression target, string name)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		return target.Type.IsArray && string.Equals(name, "Length", StringComparison.Ordinal)
			? Expression.ArrayLength(target)
			: Expression.PropertyOrField(target, name);
	}

	/// <summary>The same element as a place to write rather than a value to read.</summary>
	/// <remarks>
	/// The API keeps the two apart where C# does not: <c>ArrayIndex</c> answers with a
	/// value and cannot be assigned to, <c>ArrayAccess</c> answers with the element itself.
	/// Which one `a[0]` means is decided by which side of the `=` it stands on, which the
	/// grammar knows and the API cannot.
	/// </remarks>
	public static Expression Place(Expression target, Expression[] at)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		if (at is null)
			throw new ArgumentNullException(nameof(at));

		return target.Type.IsArray ? Expression.ArrayAccess(target, at) : Indexed(target, at);
	}

	/// <summary>What <c>a[i]</c> reads, likewise.</summary>
	/// <remarks>
	/// An array's element is a node of this tree and anything else's is an indexer — whose
	/// name is not always <c>Item</c>, `string` calling its own <c>Chars</c>. The type says
	/// which through its default member, which is what an indexer is.
	/// </remarks>
	public static Expression Indexed(Expression target, Expression[] at)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		if (at is null)
			throw new ArgumentNullException(nameof(at));

		if (target.Type.IsArray)
			return Expression.ArrayIndex(target, at);

		foreach (var member in target.Type.GetDefaultMembers())
			if (member is PropertyInfo indexer && indexer.GetIndexParameters().Length == at.Length)
				return Expression.Property(target, indexer, at);

		throw new FormatException($"'{target.Type.Name}' has no indexer taking {at.Length} of them.");
	}

	/// <summary>A static property or a static field, whichever that name is.</summary>
	/// <remarks>
	/// Two factories and one syntax: `T.Name` says nothing about which, and the type does.
	/// The instance form needs no such method — <c>Expression.PropertyOrField</c> is the
	/// API's own answer to the same question, and there is no static overload of it.
	/// </remarks>
	public static Expression StaticMember(Type type, string name)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		const BindingFlags Statics = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;

		if (type.GetProperty(name, Statics) is { } property)
			return Expression.Property(null, property);

		if (type.GetField(name, Statics) is { } field)
			return Expression.Field(null, field);

		throw new FormatException($"'{type.Name}' has no static '{name}'.");
	}

	/// <summary>The constructor those arguments fit.</summary>
	/// <remarks>
	/// <c>Expression.Call</c> takes a method by name and picks the overload itself;
	/// <c>Expression.New</c> takes a <c>ConstructorInfo</c> and has no such overload, so
	/// this is the one place the choosing is done here. Assignable rather than equal, so
	/// that `new Exception(text)` finds the one taking a string.
	/// </remarks>
	public static ConstructorInfo Constructor(Type type, Expression[] arguments)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		if (arguments is null)
			throw new ArgumentNullException(nameof(arguments));

		foreach (var candidate in type.GetConstructors())
		{
			var parameters = candidate.GetParameters();

			if (parameters.Length != arguments.Length)
				continue;

			var fits = true;

			for (var at = 0; at < parameters.Length; at++)
				fits &= parameters[at].ParameterType.IsAssignableFrom(arguments[at].Type);

			if (fits)
				return candidate;
		}

		throw new FormatException(
			$"'{type.Name}' has no constructor taking ({string.Join(", ", arguments.Select(one => one.Type.Name))}).");
	}

	/// <summary>The type that name and those arguments mean.</summary>
	/// <remarks>
	/// A generic type is named in metadata by its arity — <c>Func`2</c> — which is one more
	/// thing about the runtime rather than about the language, and so is here rather than
	/// in the grammar. The name looked up is the one the author wrote with the count of
	/// what they wrote it over.
	/// </remarks>
	public static Type Generic(string name, Type[] arguments)
	{
		if (arguments is null)
			throw new ArgumentNullException(nameof(arguments));

		var open = Lookup(name + "`" + arguments.Length.ToString(CultureInfo.InvariantCulture))
			?? throw new FormatException(
				$"there is no type named '{name}' taking {arguments.Length} of them here.");

		return open.MakeGenericType(arguments);
	}

	/// <summary>A generic type's arguments, in the order they were written.</summary>
	public static Type[] Types(Type first, Type[] rest)
	{
		if (rest is null)
			throw new ArgumentNullException(nameof(rest));

		var arguments = new Type[rest.Length + 1];

		arguments[0] = first;
		rest.CopyTo(arguments, 1);

		return arguments;
	}

	/// <summary>What one member initializer said, before the type is known.</summary>
	/// <remarks>
	/// The one type of this file's own, and it carries syntax rather than meaning: which
	/// member a name is cannot be worked out where the name is read, because the type is a
	/// sibling of the braces rather than something above them. A pair of a name and a value
	/// is what the text said and nothing more. A tuple would have said the same, and the
	/// notation has no place to write one — a rule's type is a name.
	/// </remarks>
	/// <param name="Value">What the member is assigned, or null where it is initialized.</param>
	/// <param name="Fields">A nested member initializer's own settings, or null.</param>
	/// <param name="Items">A nested collection initializer's own elements, or null.</param>
	public readonly record struct Setting(
		string Name, Expression? Value, Setting[]? Fields, Element[]? Items);

	/// <summary>What one call to a collection's `Add` takes.</summary>
	/// <remarks>
	/// A list rather than an expression, because `Add` is not obliged to take one thing:
	/// `new Dictionary&lt;int, string&gt; { { 1, "a" } }` calls it with two, which is what
	/// `Expression.ElementInit` exists to say and what a list of values could not.
	/// </remarks>
	public readonly record struct Element(Expression[] Arguments);

	/// <summary>One element, where the text wrote it without braces of its own.</summary>
	public static Element Only(Expression value) => new([value]);


	/// <summary>What an initializer sets, in the order it was written.</summary>
	public static Setting[] Set(Setting first, Setting[] rest)
	{
		if (rest is null)
			throw new ArgumentNullException(nameof(rest));

		var set = new Setting[rest.Length + 1];

		set[0] = first;
		rest.CopyTo(set, 1);

		return set;
	}

	/// <summary>Those settings against the type that has the members.</summary>
	/// <remarks>
	/// The grammar reads `Name = value` and stops there, because which member a name is
	/// cannot be known where it is read: the type is a sibling of the braces rather than
	/// something above them. So the pairs travel as text and a value, and the member is
	/// found here, where the type is in hand.
	/// </remarks>
	public static MemberBinding[] Bound(Type type, Setting[] settings)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		if (settings is null)
			throw new ArgumentNullException(nameof(settings));

		var bound = new MemberBinding[settings.Length];

		for (var at = 0; at < settings.Length; at++)
		{
			var setting = settings[at];
			var members = type.GetMember(
				setting.Name,
				MemberTypes.Property | MemberTypes.Field,
				BindingFlags.Public | BindingFlags.Instance);

			if (members.Length != 1)
				throw new FormatException($"'{type.Name}' has no one member named '{setting.Name}'.");

			var member = members[0];

			// Which of the three the text wrote, answered here rather than where it was
			// read: a nested initializer needs the *member's* type to go on, and that is
			// known one step further in than the name was.
			bound[at] =
				setting.Fields is { } fields ? Expression.MemberBind(member, Bound(MemberType(member), fields)) :
				setting.Items  is { } items  ? Expression.ListBind(member, Added(MemberType(member), items)) :
				Expression.Bind(member, setting.Value!);
		}

		return bound;
	}

	/// <summary>What a field or property holds, which a nested initializer is written in.</summary>
	static Type MemberType(MemberInfo member) =>
		member is PropertyInfo property ? property.PropertyType : ((FieldInfo)member).FieldType;

	/// <summary>Those elements against the collection type that has the `Add`.</summary>
	/// <remarks>
	/// The overload is chosen by the arguments, the same way `Expression.Call` chooses one
	/// — and for the same reason it is done here and not in the grammar: what `Add` a
	/// collection has is a question about the type, and the type is a sibling of the braces
	/// rather than something inside them.
	/// </remarks>
	public static ElementInit[] Added(Type type, Element[] elements)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		if (elements is null)
			throw new ArgumentNullException(nameof(elements));

		var added = new ElementInit[elements.Length];

		for (var at = 0; at < elements.Length; at++)
		{
			var arguments = elements[at].Arguments;
			var add = type.GetMethod(
				"Add",
				BindingFlags.Public | BindingFlags.Instance,
				binder: null,
				[.. arguments.Select(static argument => argument.Type)],
				modifiers: null);

			added[at] = add is not null
				? Expression.ElementInit(add, arguments)
				: throw new FormatException(
					$"'{type.Name}' has no 'Add' taking " +
					$"({string.Join(", ", arguments.Select(static argument => argument.Type.Name))}).");
		}

		return added;
	}

	/// <summary>A construction, with whatever initializer was written after it.</summary>
	/// <remarks>
	/// Three factories and one syntax, and which is meant is whether an initializer was
	/// written and what stood inside its braces. That could have been three alternatives,
	/// each naming its own factory, and reading the arguments three times is what that cost
	/// — so the reading is one and the choosing is here. It is the shape a generator that
	/// factored the common head of its alternatives would let the grammar keep.
	/// </remarks>
	public static Expression Made(Type type, Expression[] args, Setting[]? fields, Element[]? items)
	{
		var made = Expression.New(Constructor(type, args), args);

		return fields is not null ? Expression.MemberInit(made, Bound(type, fields))
			: items is not null   ? Expression.ListInit(made, Added(type, items))
			: made;
	}

	/// <summary>The arguments of a call, in the order they were written.</summary>
	public static Expression[] Listed(Expression? first, Expression[] rest)
	{
		if (first is null)
			return [];

		var arguments = new Expression[rest.Length + 1];

		arguments[0] = first;
		rest.CopyTo(arguments, 1);

		return arguments;
	}

	/// <summary>The elements of a collection initializer, in the order they were written.</summary>
	/// <remarks>
	/// The same shape as the one above and a second method rather than a generic one: an
	/// <see cref="Element"/> is a struct and an <see cref="Expression"/> is not, so the
	/// absent case they would have to share is spelled two different ways. Here there is
	/// no absent case — the grammar asks for one element before the run — which is the
	/// other half of why they do not merge.
	/// </remarks>
	public static Element[] Listed(Element first, Element[] rest)
	{
		if (rest is null)
			throw new ArgumentNullException(nameof(rest));

		var elements = new Element[rest.Length + 1];

		elements[0] = first;
		rest.CopyTo(elements, 1);

		return elements;
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

	/// <summary>An <c>if</c> with an <c>else</c>, worth what its branches agree on.</summary>
	/// <remarks>
	/// <c>Expression.Condition</c> is one factory with two answers — the branches' own type,
	/// or <c>typeof(void)</c> where they have none in common — and which of them a given
	/// <c>if</c> could have meant is a question about this API and not about the language.
	/// The grammar says what an `if` is, in the words every language uses for it; this says
	/// what that turns into here. Written the other way round, the grammar would have to
	/// carry a distinction that only <c>System.Linq.Expressions</c> makes.
	/// </remarks>
	public static Expression Chosen(Expression test, Expression? then, Expression? otherwise) =>
		then is null || otherwise is null
			? test
			: Expression.Condition(
				test, then, otherwise, then.Type == otherwise.Type ? then.Type : typeof(void));

	/// <summary>A <c>??</c> where one was written, and the left side where none was.</summary>
	public static Expression Coalesced(Expression left, Expression? right) =>
		right is null ? left : Expression.Coalesce(left, right);

	/// <summary>
	/// What one reading of this language works out, and <c>System.Linq.Expressions</c> has
	/// nowhere to keep.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The grammar declares this as its `context` (§7.7) and the caller hands one over, so
	/// it lives exactly as long as a parse. It was seven <c>[ThreadStatic]</c> fields and a
	/// <c>Begin</c> that cleared them, which is a discipline rather than a guarantee — and
	/// the discipline failed: the generated <c>TryParseLambda</c> never called it, so one
	/// parse's blocks were still standing when the next one asked and a perfectly good text
	/// was told there was no such name. Nothing here can be forgotten to be cleared, because
	/// nothing survives the call.
	/// </para>
	/// <para>
	/// What is not here is the namespace list and the type cache. Those belong to the
	/// parser rather than to a parse, are shared on purpose, and stayed where they were.
	/// </para>
	/// </remarks>
	public sealed class State
	{
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
		List<Scope>? _scopes;

		/// <summary>The variables of the lambda being read, in the order they were declared.</summary>
		/// <remarks>
		/// A <c>ParameterExpression</c> is an identity: the one made for <c>(int x)</c> has to
		/// be the very object every <c>x</c> in the body reads, or the compiled lambda closes
		/// over a variable nothing assigns. A list rather than a table by name, because two
		/// blocks beside each other may each declare an <c>x</c> and those are two variables —
		/// which is legal C#, and the whole reason a name is looked up by where it is written.
		/// </remarks>
		List<Declaration>? _declared;

		/// <summary>Where a <c>return</c> goes, made once the first one says what it yields.</summary>
		LabelTarget? _returns;

		/// <summary>A block, recorded while the text is read (§8.1).</summary>
		/// <remarks>
		/// It has to be read rather than built: <c>=&gt;</c> runs children before parents, so
		/// every name inside a block is built before the block itself is, and a scope recorded
		/// there would arrive after the last thing that needed it.
		/// </remarks>
		public bool Scoped(SourceSpan span)
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
		public bool Declare(Type type, string name, SourceSpan at) =>
			Holds(Expression.Variable(type ?? throw new ArgumentNullException(nameof(type)), name), name, at);

		/// <summary>The same for a lambda's parameter, which the API names apart.</summary>
		/// <remarks>
		/// <c>Expression.Parameter</c> and <c>Expression.Variable</c> make the same kind of
		/// node, and the API keeps two names for it because a language does: one is what a
		/// lambda is handed and the other is what a block declares. This one reads them apart
		/// because it can — they are two rules — and says so by naming both.
		/// </remarks>
		public bool Takes(Type type, string name, SourceSpan at) =>
			Holds(Expression.Parameter(type ?? throw new ArgumentNullException(nameof(type)), name), name, at);

		bool Holds(ParameterExpression variable, string name, SourceSpan at)
		{
			(_declared ??= []).Add(new Declaration(at.Start, name, variable));

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
		public ParameterExpression Named(string name, SourceSpan at)
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
		Scope? Holding(int position) => Innermost(_scopes, position);

		/// <summary>The innermost of these extents holding a position, or none.</summary>
		Scope? Innermost(List<Scope>? among, int position)
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

		List<Scope>? _loops;

		List<Scope>? _breakables;

		Dictionary<int, LabelTarget>? _exits;

		Dictionary<int, LabelTarget>? _agains;

		/// <summary>A loop, which a <c>break</c> and a <c>continue</c> may both name.</summary>
		public bool Loops(SourceSpan span)
		{
			(_loops ??= []).Add(new Scope(span.Start, span.Start + span.Length));

			return Breaks(span);
		}

		/// <summary>A switch, which only a <c>break</c> may name.</summary>
		public bool Breaks(SourceSpan span)
		{
			(_breakables ??= []).Add(new Scope(span.Start, span.Start + span.Length));

			return true;
		}

		/// <summary>Where a <c>break</c> written here goes.</summary>
		public LabelTarget Exit(SourceSpan at) =>
			Labelled(
				_exits ??= [],
				Innermost(_breakables, at.Start) ??
					throw new FormatException("a 'break' here is inside no loop and no switch."),
				"break");

		/// <summary>Where a <c>continue</c> written here goes.</summary>
		public LabelTarget Again(SourceSpan at) =>
			Labelled(
				_agains ??= [],
				Innermost(_loops, at.Start) ??
					throw new FormatException("a 'continue' here is inside no loop."),
				"continue");

		/// <summary>One label per extent, made the first time anything asks for it.</summary>
		LabelTarget Labelled(Dictionary<int, LabelTarget> labels, Scope? of, string name)
		{
			var at = of!.Value.From;

			if (!labels.TryGetValue(at, out var target))
				labels[at] = target = Expression.Label(name);

			return target;
		}

		/// <summary>A jump to the lambda's label, which the first <c>return</c> is what makes.</summary>
		public Expression Return(Expression value)
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
		public Expression Returning(Expression body)
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
		public Expression Block(Expression[] statements, SourceSpan at, Expression? value)
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
}
