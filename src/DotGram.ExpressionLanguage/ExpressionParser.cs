using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace DotGram.ExpressionLanguage;

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
//   * `Branched` — `Expression.Condition` is one factory with two answers, the branches'
//     type or `void`, and which one an `if` meant is a question about this API alone.
//   * `Implicitly`, and what is written over it — which conversion C# makes unasked, the
//     one type an operator's operands are promoted to, the overload a call means.
//   * `TypeNamed`, `Member`, `Indexed`, `Called` and the rest of what a name in metadata
//     turns into — which type a name means in the namespaces the text's `using`s bring
//     in, which member a name is on a given type, which overload takes these arguments.
//     The grammar says where a name stands; the type it stands against is the host's,
//     and is not known until the operand is built.
//   * `Integer` — which of four types an integer is depends on its value as well as on
//     how it was written, and that is a question about the digits rather than the text.
//   * `Add`, `Negate`, `Cast` and the rest that have a checked form — the API has two
//     nodes for each, and which one a `checked` meant is again a question about the API.
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
// **And the grammar is shaped to the API in one place**, deliberately: a local says its
// type — `int sum = …`, not `var sum = …` — because `Expression.Variable` wants one where
// the declaration is read, and the initializer is not built until long after. That is the
// API's requirement showing through, which is what wiring one up actually looks like.
//
// **The conversions C# makes unasked are made here too**, and by the host rather than the
// grammar. `Expression.Add` over an `int` and a `double` refuses rather than widening, and
// `Expression.Call` wants an argument of its parameter's own type; `Expression.Convert`
// builds any conversion it is told to and decides none. Which one C# would have made is a
// question about types, so it is answered where the types are — `Implicitly`, and the
// operators and the overload resolution written over it — and the grammar goes on naming
// the factory each operator is.
//
// **Where it is not C#, and why.** None of these is a shape the notation could not
// carry; each is an API requirement showing through again, or something not written yet.
//
//   * `null` takes the type of what it is converted to — an argument, an assignment, the
//     other operand, the other branch — as C#'s does, but where nothing converts it, as
//     the whole body of a lambda `Parse` reads, it is an `object`.
//   * A call is resolved by C#'s rules, less what those rules need and this language
//     cannot write: no generic method is named or inferred, no extension method is found,
//     and no argument is passed by `ref`, `out` or name.
//   * A `using` names a namespace and nothing else — no alias, no `using static` — and
//     reaches the public types of the assemblies already loaded: there are no references
//     to say which others, and a type in none of them is not there to be named.
//   * A constant is a literal, or a literal with a minus before it: `byte b = 1 + 1` is
//     refused where C# folds the sum first and then converts it.
//   * An increment or a compound assignment writes to a name or to one member of a name —
//     not to an element, and not to a longer chain.
//   * `typeof`, `default`, `nameof`, `?.`, `var`, `foreach`, an interpolated string and a
//     lambda inside an expression are not written yet.
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

		// The same word with the alphabet named rather than asked for by category, for
		// the publication below that reads only it. Declared here, inside `trivia =
		// none`, and not written into the `with` itself: a substitution written on a
		// `parse` reads the trivia surrounding the directive (§5.1), which out there
		// spaces its operands — and a word whose letters may be spaced apart is not one.
		AsciiWord = ['a'..'z' | 'A'..'Z' | '_'] & ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']*

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
			|  "ushort"  | "using"   | "while")
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
		UnsignedLong(N) : @string = t: N & (['u' | 'U'] & ['l' | 'L'] | ['l' | 'L'] & ['u' | 'U']) => @(t)

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
		= Import* & '(' & (first: Parameter & (',' & rest: Parameter)*)? & ')' & "=>" & body: Value
		=> @(Expression.Lambda(context.Returning(body), ExpressionParser.Taking(first, rest)))

	// A `using` stands before the lambda, as it stands at the top of a C# file, and names a
	// namespace whose types every name after it may mean. The guard records it while the
	// text is read, which is the moment every later guard asks about a name — and refuses
	// a namespace that is not there, as C# does.
	Import : @string
		= "using" & head: Word & ('.' & part: NamePart)* & ';'
		  & when @(context.Imports(ExpressionParser.Dotted(head, part), parserSpan))
		  => @(ExpressionParser.Dotted(head, part))

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
	// keyword and cannot be: what `Exception` means is a question about the namespaces the
	// text's `using`s name, and it is asked while the text is read so that the answer can
	// decide how the text reads.
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

	NamedType? : @Type
		= head: Word & ('.' & part: NamePart)*
		  & args: ('<' & first: Type & (',' & rest: Type)* & '>')?
		  & when @(args != null || context.Resolves(ExpressionParser.Dotted(head, part)))
		  => @(args is null
		       ? context.TypeNamed(ExpressionParser.Dotted(head, part))
		       : context.Generic(
		           ExpressionParser.Dotted(head, part), ExpressionParser.Types(first!, rest)))

	// One rule for every argument list there is, so that a call, a constructor and an
	// indexer all say it the same way and each hands the API one array.
	Arguments : @Expression[] = '(' & (first: Expression & (',' & rest: Expression)*)? & ')'
		     => @(ExpressionParser.Listed(first, rest))

	// What a member initializer sets, as the text said it: the member's name and the value,
	// with which member that is left until the type is known — which is at construction,
	// where the type is.
	Bindings : @Setting[] = '{' & first: Binding & (',' & rest: Binding)* & '}'
		    => @(ExpressionParser.Set(first, rest))

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
		=> @(ExpressionParser.Listed(first, rest))

	// An element is what one call to `Add` takes, which is usually one expression and for
	// a dictionary is two. C# writes the second in braces of its own, and the API has a
	// node for it — `ElementInit` — because a collection whose `Add` takes two arguments
	// cannot be described by a list of values.
	Element : @Element
		= '{' & first: Expression & (',' & rest: Expression)* & '}'
		  => @(new Element(ExpressionParser.Listed(first, rest)))
		| only: Expression => @(ExpressionParser.Only(only))

	Indices : @Expression[]
		= '[' & first: Expression & (',' & rest: Expression)* & ']'
		=> @(ExpressionParser.Listed(first, rest))

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
		=> @(ExpressionParser.Assigned(context.Named(name, parserSpan), value))

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
	Value : @Expression
		= b: Block => @(b) | c: IfValue => @(c) | c: Control => @(c) | e: Expression => @(e)

	Control : @Expression
		= c: Try     => @(c)
		| c: If      => @(c)
		| c: While   => @(c)
		| c: DoWhile => @(c)
		| c: For     => @(c)
		| c: Switch  => @(c)

	If : @Expression
		= "if" & '(' & test: Expression & ')' & then: Branch & "else" & otherwise: Branch => @(ExpressionParser.Branched(test, then, otherwise))
		| "if" & '(' & test: Expression & ')' & then: Statement => @(Expression.IfThen(test, then))

	// The same `if` where a value is wanted: its branches are values, so the `;` after
	// `else 0` is the declaration's and not the branch's. Over kinds a rule's answer stands
	// (§4), and a branch read as the statement `0;` would not hand the `;` back; statement
	// position keeps the rule above, whose branches are statements first.
	IfValue : @Expression
		= "if" & '(' & test: Expression & ')' & then: Value & "else" & otherwise: Value
		  => @(ExpressionParser.Branched(test, then, otherwise))

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
		= "while" & '(' & test: Expression & ')'
		& when @(context.Opening(parserSpan)) & body: Statement
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
		= "do" & when @(context.Opening(parserSpan)) & body: Statement
		& "while" & '(' & test: Expression & ')' & ';'
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
		& when @(context.Opening(parserSpan)) & body: Statement
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
		= "switch" & '(' & value: Expression & ')' & '{'
		& when @(context.Breaking(parserSpan))
		& cases: Case* & fallback: Fallback? & '}'
		& when @(context.Breaks(parserSpan))
		=> @(Expression.Block(
			Expression.Switch(typeof(void), value, fallback, null, ExpressionParser.Against(cases, value.Type)),
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
		  => @(ExpressionParser.Assigned(ExpressionParser.Place(target, at), value))

		// Each compound form names the assignment the API has for it and the operator it
		// stands for: C#'s `x op= y` is `x = (T)(x op y)`, which is the node the API has only
		// where the operator's own type is the target's.
		| target: Target & "+="  & value: Assignment
		  => @(ExpressionParser.AddAssign(target, value, parserState))
		| target: Target & "-="  & value: Assignment
		  => @(ExpressionParser.SubtractAssign(target, value, parserState))
		| target: Target & "*="  & value: Assignment
		  => @(ExpressionParser.MultiplyAssign(target, value, parserState))
		| target: Target & "/="  & value: Assignment
		  => @(ExpressionParser.ArithmeticAssign(Expression.DivideAssign, Expression.Divide, target, value, parserState))
		| target: Target & "%="  & value: Assignment
		  => @(ExpressionParser.ArithmeticAssign(Expression.ModuloAssign, Expression.Modulo, target, value, parserState))
		| target: Target & "&="  & value: Assignment
		  => @(ExpressionParser.IntegralAssign(Expression.AndAssign, Expression.And, target, value, parserState))
		| target: Target & "|="  & value: Assignment
		  => @(ExpressionParser.IntegralAssign(Expression.OrAssign, Expression.Or, target, value, parserState))
		| target: Target & "^="  & value: Assignment
		  => @(ExpressionParser.IntegralAssign(Expression.ExclusiveOrAssign, Expression.ExclusiveOr, target, value, parserState))
		| target: Target & "<<=" & value: Assignment
		  => @(ExpressionParser.ShiftAssign(Expression.LeftShiftAssign, Expression.LeftShift, target, value, parserState))
		| target: Target & ">>=" & value: Assignment
		  => @(ExpressionParser.ShiftAssign(Expression.RightShiftAssign, Expression.RightShift, target, value, parserState))
		| target: Target & '=' & ?!'=' & value: Assignment => @(ExpressionParser.Assigned(target, value))
		| c: Conditional                           => @(c)

	// What may be written to. An element is read one way and written another — `ArrayIndex`
	// answers with a value and `ArrayAccess` with a place — and which is wanted is decided
	// by where it stands, which is a thing the grammar knows and the API does not.
	// The member is an optional tail rather than a second alternative: written as two, the
	// name is read once for each and so is every alternative of `Assignment` that begins
	// with this. One reading is the same language here because a member begins with '.',
	// which a name cannot contain.
	// The guard is what lets an assignment read its target before it knows which operator
	// follows: `s.Trim()` is read here as `s` and a member `Trim` on the way to finding out
	// it is a call, and a construction that threw for a name that is not a property would
	// only work because it runs after the parse. What the guard asks is what the
	// construction is about to do, so the two cannot drift.
	Target : @Expression
		= n: Name & ('.' & member: Word)? & when @(ExpressionParser.Has(n, member))
		=> @(member is null ? n : ExpressionParser.Member(n, member))

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
		  => @(ExpressionParser.Chosen(test, then, otherwise))

	Coalesce : @Expression
		= left: Binary & ("??" & right: Coalesce)?
		  => @(ExpressionParser.Coalesced(left, right))

	// C#'s ladder, from `||` down to `*`, as one rule with the strengths written down
	// (§4.3.1) rather than as ten rules stacked on one another. The language and the tree
	// are the same either way; what is not the same is what it costs to reach an operand.
	// Written as levels there are ten calls and ten first-set tests between an assignment
	// and a name, every one of them for every operand a text has — measured at 94
	// nanoseconds a parenthesis against a hand-written parser's 30, which is the whole of
	// what the two differed by (docs/next.md).
	//
	// Three of the alternatives need a word about the character they begin with. `|` and
	// `&` each begin a two-character operator one level out, and the lookahead is what
	// tells them apart — cheaper than letting `a || b` be read as `a | (| b)` and unwound
	// by backtracking, and clearer about why it is not. `>` is the same and earns more:
	// the shift is a level tighter, so without the lookahead `a >> b` reads as `a > (> b)`.
	//
	// And the shift is written as two `>` glued rather than as one `">>"`, which is what
	// lets `List<List<int>>` close two argument lists with the same two characters C#
	// closes them with. A literal `">>"` is a token, and a token cannot be half spent: the
	// type argument list wants one `>` and would be handed a shift. Written this way there
	// is no `>>` for the lexer to make, `~` says the two stand with nothing between them,
	// and `a > > b` is refused exactly as C# refuses it.
	Binary : @Expression
		= left: Binary & "||" & right: Binary        << 1  => @(Expression.OrElse(left, right))
		| left: Binary & "&&" & right: Binary        << 2  => @(Expression.AndAlso(left, right))
		| left: Binary & '|' & ?!'|' & right: Binary << 3  => @(ExpressionParser.Integral(Expression.Or, left, right))
		| left: Binary & '^' & right: Binary         << 4  => @(ExpressionParser.Integral(Expression.ExclusiveOr, left, right))
		| left: Binary & '&' & ?!'&' & right: Binary << 5  => @(ExpressionParser.Integral(Expression.And, left, right))
		| left: Binary & "==" & right: Binary        << 6  => @(ExpressionParser.Equality(Expression.Equal, left, right))
		| left: Binary & "!=" & right: Binary        << 6  => @(ExpressionParser.Equality(Expression.NotEqual, left, right))
		| left: Binary & "is" & type: Type           << 7  => @(Expression.TypeIs(left, type))
		| left: Binary & "as" & type: Type           << 7  => @(Expression.TypeAs(left, type))
		| left: Binary & "<=" & right: Binary        << 7  => @(ExpressionParser.Relational(Expression.LessThanOrEqual, left, right))
		| left: Binary & ">=" & right: Binary        << 7  => @(ExpressionParser.Relational(Expression.GreaterThanOrEqual, left, right))
		| left: Binary & '<' & ?!'<' & right: Binary << 7  => @(ExpressionParser.Relational(Expression.LessThan, left, right))
		| left: Binary & '>' & ?!'>' & right: Binary << 7  => @(ExpressionParser.Relational(Expression.GreaterThan, left, right))
		| left: Binary & '<' ~ '<' & right: Binary   << 8  => @(ExpressionParser.Shift(Expression.LeftShift, left, right))
		| left: Binary & '>' ~ '>' & right: Binary   << 8  => @(ExpressionParser.Shift(Expression.RightShift, left, right))
		| left: Binary & '+' & right: Binary         << 9
		  => @(ExpressionParser.Add(left, right, parserState))
		| left: Binary & '-' & right: Binary         << 9
		  => @(ExpressionParser.Subtract(left, right, parserState))
		| left: Binary & '*' & right: Binary         << 10
		  => @(ExpressionParser.Multiply(left, right, parserState))
		| left: Binary & '/' & right: Binary         << 10 => @(ExpressionParser.Arithmetic(Expression.Divide, left, right))
		| left: Binary & '%' & right: Binary         << 10 => @(ExpressionParser.Arithmetic(Expression.Modulo, left, right))
		| u: Unary                                          => @(u)

	// `++` and `--` before `+` and `-`, so that `--x` is one operator and not two, and over
	// a name for the same reason assignment is: they write to what they read.
	Unary : @Expression
		= "++" & target: Name => @(Expression.PreIncrementAssign(target))
		| "--" & target: Name => @(Expression.PreDecrementAssign(target))
		| '-' & operand: Unary => @(ExpressionParser.Negate(operand, parserState))
		| '+' & operand: Unary => @(ExpressionParser.Arithmetic(Expression.UnaryPlus, operand))
		| '!' & operand: Unary => @(Expression.Not(operand))
		| '~' & operand: Unary => @(ExpressionParser.Integral(Expression.OnesComplement, operand))

		// A cast is told from a parenthesized expression by what stands inside it. A keyword
		// type is no name, and a name is a type only where `NamedType`'s guard finds one —
		// which is the question C# needs a rule of its own for, asked of the host while the
		// text is read: `(Exception)e` is a cast, and `(e)` is a parenthesis.
		| '(' & type: Type & ')' & operand: Unary
		  => @(ExpressionParser.Cast(operand, type, parserState))

		| p: Postfix => @(p)

	// Everything written after an operand rather than before it. Left recursive, which is
	// what makes `a.b.c(d)[0]` one chain read once — §4.3 reads the operand at the head of
	// it and then folds the suffixes on, rather than starting over for each.
	//
	// Which method a name means is C#'s overload resolution, and `Called` is that. The API's
	// own, inside `Expression.Call`, takes an argument only of its parameter's exact type or
	// assignable to it by reference: `Math.Sqrt(x)` over an `int` finds nothing there, and
	// `Console.WriteLine(s)` finds two and refuses both.
	Postfix : @Expression
		= target: Postfix & '.' & member: Word & args: Arguments
		  => @(ExpressionParser.Called(target, member, args))

		| target: Postfix & '.' & member: Word => @(ExpressionParser.Member(target, member))

		// An index is a list, so a two-dimensional array and an indexer of two arguments are
		// both written without another rule.
		| target: Postfix & at: Indices => @(ExpressionParser.Indexed(target, at))

		| target: Name & args: Arguments => @(ExpressionParser.Invoked(target, args))

		| target: Name & "++" => @(Expression.PostIncrementAssign(target))
		| target: Name & "--" => @(Expression.PostDecrementAssign(target))
		| p: Primary          => @(p)

	Primary : @Expression
		= "new" & type: Type & '[' & size: Expression & ']'
		  => @(Expression.NewArrayBounds(type, size))
		// `new int[] { … }`: the `[]` is the array type's own — `Type` reads it as one token,
		// the lexer taking the longest match — and the braces are what tell this from a
		// constructor. Over kinds a rule's answer stands (§4), so `Type` is not asked to give
		// the `[]` back for a `"[]"` written here; the guard asks for an array type instead.
		| "new" & type: Type & when @(type is { IsArray: true })
		  & '{' & (first: Expression & (',' & rest: Expression)*)? & '}'
		  => @(Expression.NewArrayInit(
		       type.GetElementType()!,
		       ExpressionParser.Converted(ExpressionParser.Listed(first, rest), type.GetElementType()!)))
		// An initializer is written after the constructor's own arguments, and which of the
		// two it is is what stands inside the braces: `Name = value` sets a member, and an
		// expression is an element to add. Both are one optional tail rather than three
		// alternatives, for the reason `Conditional` gives above — three alternatives read
		// the arguments three times before finding out which they are, and the arguments
		// hold whole expressions. Nine nested `new`s took a second that way.
		| "new" & type: Type & args: Arguments
		  & (fields: Bindings | '{' & items: Elements & '}')?
		  => @(ExpressionParser.Made(type, args, fields, items))

		// A type, then something of it. Told from `a.b` by the guard inside `NamedType`,
		// which is the same question C# answers with a section of its own — a dotted name
		// is a type where it names one, and an expression where it does not.
		// The arguments are an optional tail rather than a second alternative: written as
		// two, the type and the member are read once for each, and a dotted type name is
		// not cheap to read. One reading is the same language because arguments begin with
		// '(', which nothing at the end of a member name can be.
		| type: NamedType & '.' & member: Word & args: Arguments?
		  => @(args is null
		       ? ExpressionParser.StaticMember(type, member)
		       : ExpressionParser.Called(type, member, args))

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

		// An integer is the first of `int`, `uint`, `long` and `ulong` that holds it, and a
		// suffix strikes some of them off the list. Which one that is depends on the value,
		// which is a question about the digits rather than the text, so `Integer` answers
		// it — one alternative per way of writing a number, and not one per type it may be.
		| token: UnsignedLong(Hex) => @(ExpressionParser.Integer(token, 16, unsigned: true,  wide: true))
		| token: SignedLong(Hex)   => @(ExpressionParser.Integer(token, 16, unsigned: false, wide: true))
		| token: Unsigned(Hex)     => @(ExpressionParser.Integer(token, 16, unsigned: true,  wide: false))
		| token: Hex               => @(ExpressionParser.Integer(token, 16, unsigned: false, wide: false))

		| token: UnsignedLong(Bin) => @(ExpressionParser.Integer(token, 2,  unsigned: true,  wide: true))
		| token: SignedLong(Bin)   => @(ExpressionParser.Integer(token, 2,  unsigned: false, wide: true))
		| token: Unsigned(Bin)     => @(ExpressionParser.Integer(token, 2,  unsigned: true,  wide: false))
		| token: Bin               => @(ExpressionParser.Integer(token, 2,  unsigned: false, wide: false))

		| token: UnsignedLong(Dec) => @(ExpressionParser.Integer(token, 10, unsigned: true,  wide: true))
		| token: SignedLong(Dec)   => @(ExpressionParser.Integer(token, 10, unsigned: false, wide: true))
		| token: Unsigned(Dec)     => @(ExpressionParser.Integer(token, 10, unsigned: true,  wide: false))
		| token: Dec               => @(ExpressionParser.Integer(token, 10, unsigned: false, wide: false))

		| token: Verbatim  => @(Expression.Constant(token))
		| token: Text      => @(Expression.Constant(token))
		| token: Char      => @(Expression.Constant(token[0]))

		| "true"     => @(Expression.Constant(true))
		| "false"    => @(Expression.Constant(false))

		// One node, and not a fresh constant each time: C# types `null` by where it stands,
		// and the conversions tell the literal from an `object` that happens to be null by
		// which node it is.
		| "null"     => @(ExpressionParser.Null)

		| n: Name    => @(n)

	// The guard is what makes this rule readable speculatively, which it has to be: an
	// assignment reads its target as a name before it knows which operator follows, and
	// `Math.Max(x, 1)` reads `Math` as one before `NamedType` gets a look. A `=>` that
	// threw for a name the next alternative reads perfectly well would only work because
	// it runs after the parse; a `when` refuses while it is being read, which is what the
	// rule means anyway — a name is a name where something declares it.
	Name : @Expression = ?!Keyword & name: Word & when @(context.Knows(name, parserSpan))
	                   => @(context.Named(name, parserSpan))

	parse Lambda as ParseLambda

	// The same language with its identifiers spelled in ASCII, and one line to say so
	// (§5.1). A binding on a publication clones what the directive reaches and rewrites
	// every call inside the clones, so every rule that reads a word — a parameter, a
	// member, a type, a label, a name — reads this one, while `ParseLambda` beside it
	// goes on reading what Unicode calls a letter.
	parse Lambda with (Word = AsciiWord) as ParseAsciiLambda
	""", Lexical = true)]

// The same grammar with the constructions run where they are read rather than after
// the parse is accepted (§6.5). It is here to be measured and to be held against the
// reading above: the two must answer alike on every input, which is what
// ExpressionCarrierTests asks. Nothing in this language needs a construction deferred —
// the one place that did, a name resolved by a factory that threw, is a `when` now.
[GramOptions(Carrier = GramCarrier.Immediate, Suffix = "Immediate")]
public static partial class ExpressionParser
{
	// ParseLambda and TryParseLambda are generated here.

	/// <summary>Reads the text as a lambda over an expression tree.</summary>
	/// <exception cref="FormatException">
	/// The text is not this language, or names what is not there: a variable nothing
	/// declares, a namespace a `using` names, a type or a member.
	/// </exception>
	/// <exception cref="InvalidOperationException">
	/// It is this language and means nothing in it — an operator its operands do not
	/// support, a call no overload fits or two fit equally, a name two `using`s both give,
	/// a `?:` whose branches meet in no type. In C#'s words where the rule is C#'s, and in
	/// <c>System.Linq.Expressions</c>' own where it is the API that refuses.
	/// </exception>
	/// <exception cref="ArgumentException">
	/// The same, where the API calls it an argument: a value assigned to what cannot hold
	/// it, a condition that is not a <c>bool</c>.
	/// </exception>
	/// <exception cref="OverflowException">
	/// An integer too large for any integral type, which C# refuses as well (CS1021).
	/// </exception>
	public static LambdaExpression Parse(string text)
	{
		var state = new State();
		var match = TryParseLambda(text, state);

		if (match.IsSuccess)
			return match.Value!;

		// A name nothing declares is refused by `Name`'s guard like anything else, and what
		// the parser then says is that it wanted an expression here. It is the state that
		// knows why, so where the parse ended exactly where a name was refused, it is the
		// state that says it.
		throw new FormatException(state.Refused() ?? match.Error!);
	}

	/// <summary>The same, answering rather than throwing.</summary>
	/// <remarks>
	/// <para>
	/// For everything <see cref="Parse"/> would throw for, and not only for text that is not
	/// this language: a name nothing declares, a member the type does not have, an operator
	/// its operands do not support. A caller holding text somebody typed cannot tell those
	/// from a mistake in the syntax before asking, and should not need a second way of
	/// being told.
	/// </para>
	/// <para>
	/// Where the text was read and the tree it asked for could not be built,
	/// <c>Position</c> is zero: the whole text was read, and the factory that refused it
	/// does not say where.
	/// </para>
	/// </remarks>
	public static Match<LambdaExpression> TryParse(string text)
	{
		var state = new State();
		Match<LambdaExpression> match;

		try
		{
			match = TryParseLambda(text, state);
		}
		catch (Exception thrown) when (IsRefusal(thrown))
		{
			return Match<LambdaExpression>.Failed(Outcome.NoMatch, thrown.Message, 0, null, null);
		}

		return match.IsSuccess || state.Refused() is not { } refused
			? match
			: Match<LambdaExpression>.Failed(Outcome.NoMatch, refused, state.RefusedAt, null, null);
	}

	/// <summary>Whether an exception is the text being refused, as against a defect here.</summary>
	/// <remarks>
	/// The four <see cref="Parse"/> documents, and not the null checks every helper in this
	/// class makes: an <c>ArgumentNullException</c> is this class handing itself nothing,
	/// which no text can cause and an answer would hide.
	/// </remarks>
	static bool IsRefusal(Exception thrown) =>
		thrown is FormatException or InvalidOperationException or OverflowException ||
		thrown is ArgumentException and not ArgumentNullException;


	/// <summary>The same, compiled to a delegate of the caller's own type.</summary>
	/// <remarks>
	/// Where the two halves meet a caller: what the text declares has to match what the
	/// delegate takes, and <c>Expression.Lambda</c> is what says so — in a message naming
	/// both, which is better than anything this could invent. What the body is worth is
	/// converted to what the delegate returns, as C# converts a lambda's body, so
	/// `(int x) => x` is a <c>Func&lt;int, long&gt;</c> as well.
	/// </remarks>
	public static TDelegate Compile<TDelegate>(string text)
		where TDelegate : Delegate
	{
		var lambda  = Parse(text);
		var returns = typeof(TDelegate).GetMethod("Invoke")!.ReturnType;
		var body    = returns == typeof(void) ? lambda.Body : Converted(lambda.Body, returns);

		return (TDelegate)Expression.Lambda(typeof(TDelegate), body, lambda.Parameters).Compile();
	}

	// ── What a name written as a type means ─────────────────────────────────────
	//
	// The keywords are the grammar's, written as `typeof(int)` where the C# compiler reads
	// them. A name is not: `Exception` means something only against a set of namespaces to
	// look in, and the text says which with a `using`, as a C# file does. Nothing is
	// imported unasked, `System` included — what a name means is written where it is used.
	// The `using`s belong to the reading (`State`); what is shared is only what the loaded
	// assemblies say, which is the same for every reading.

	/// <summary>A dotted name from the words the grammar read, and nothing between them.</summary>
	/// <remarks>
	/// The parts and not the run: the words are captured one at a time, so whatever spacing
	/// stood between them in the text is not in the name. `System . Text` is `System.Text`,
	/// which is what it means and what the lookup below can answer about.
	/// </remarks>
	public static string Dotted(string head, string[]? tail) =>
		tail is null || tail.Length == 0 ? head : head + "." + string.Join(".", tail);

	/// <summary>A dotted name within a namespace as a type, or null where it is none.</summary>
	/// <remarks>
	/// The longest part of the name that is a type by its full name, and the rest as types
	/// nested in it one at a time — so `Environment.SpecialFolder` is found through
	/// <c>Environment</c>, which metadata calls <c>System.Environment+SpecialFolder</c> and no
	/// full name written with dots would reach.
	/// </remarks>
	static Type? Qualified(string? space, string dotted)
	{
		var end = dotted.Length;

		while (true)
		{
			var head = dotted.Substring(0, end);

			if (Loaded.Find(space is null ? head : space + "." + head) is { } type)
			{
				for (var at = end; type is not null && at < dotted.Length;)
				{
					var next = dotted.IndexOf('.', at + 1);

					if (next < 0)
						next = dotted.Length;

					type = type.GetNestedType(dotted.Substring(at + 1, next - at - 1), BindingFlags.Public);
					at   = next;
				}

				return type;
			}

			end = dotted.LastIndexOf('.', end - 1);

			if (end < 0)
				return null;
		}
	}

	/// <summary>What the assemblies loaded into this process say about names.</summary>
	/// <remarks>
	/// <para>
	/// Shared, because it is the same for every reading — unlike the `using`s, which belong
	/// to one. Public types only: a type another assembly keeps internal is not one C# written
	/// outside it can name, and neither is it here.
	/// </para>
	/// <para>
	/// Both answers are kept once found, a type's absence included: a name is asked about far
	/// more often than it names anything, since every `s.Length` asks whether `s` is a type.
	/// An assembly loaded later may make an absent name present, so a load forgets what was
	/// kept — which is also why this is a class of its own, whose static constructor is the
	/// one place the subscription is made exactly once.
	/// </para>
	/// </remarks>
	static class Loaded
	{
		static readonly ConcurrentDictionary<string, Type?> _types = new(StringComparer.Ordinal);

		static HashSet<string>? _namespaces;

		static Loaded() =>
			AppDomain.CurrentDomain.AssemblyLoad += static (_, _) =>
			{
				_types.Clear();
				_namespaces = null;
			};

		/// <summary>The public type that full name means in any loaded assembly, or null.</summary>
		public static Type? Find(string fullName) => _types.GetOrAdd(fullName, static name => Search(name));

		/// <summary>Whether a loaded assembly has a public type in that namespace, or in one inside it.</summary>
		/// <remarks>
		/// Inside it too, because C# takes `using System.Collections;` whether or not that
		/// namespace declares a type of its own: it is there because something is in it.
		/// </remarks>
		public static bool Has(string @namespace) => (_namespaces ?? Gather()).Contains(@namespace);

		static Type? Search(string name)
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
				if (!assembly.IsDynamic && assembly.GetType(name, false, false) is { IsVisible: true } type)
					return type;

			return null;
		}

		static HashSet<string> Gather()
		{
			var namespaces = new HashSet<string>(StringComparer.Ordinal);

			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				if (assembly.IsDynamic)
					continue;

				Type[] types;

				try
				{
					types = assembly.GetExportedTypes();
				}
				catch (Exception exception) when (
					exception is NotSupportedException or TypeLoadException or ReflectionTypeLoadException or
						System.IO.FileNotFoundException or System.IO.FileLoadException)
				{
					// An assembly whose types cannot all be loaded contributes none, as a
					// reference C# cannot read contributes none.
					continue;
				}

				foreach (var type in types)
				{
					var space = type.Namespace;

					while (space is not null && namespaces.Add(space))
						space = space.LastIndexOf('.') is var dot and >= 0 ? space.Substring(0, dot) : null;
				}
			}

			return _namespaces = namespaces;
		}
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

	/// <remarks>
	/// And the one of them that means something else over text: a `+` with a string on
	/// either side is concatenation, which C# compiles to <c>string.Concat</c> and the API has
	/// no operator for.
	/// </remarks>
	public static Expression Add(Expression left, Expression right, ReadOnlySpan<Reading> reading) =>
		Joined(left, right) ?? Arithmetic(Checked(reading) ? Expression.AddChecked : Expression.Add, left, right);

	public static Expression Subtract(Expression left, Expression right, ReadOnlySpan<Reading> reading) =>
		Arithmetic(Checked(reading) ? Expression.SubtractChecked : Expression.Subtract, left, right);

	public static Expression Multiply(Expression left, Expression right, ReadOnlySpan<Reading> reading) =>
		Arithmetic(Checked(reading) ? Expression.MultiplyChecked : Expression.Multiply, left, right);

	/// <remarks>
	/// `-2147483648` is the <c>int</c> it looks like, though `2147483648` alone is a
	/// <c>uint</c>, and `-9223372036854775808` is a <c>long</c> in the same way: C# reads
	/// each as one literal where it stands after a minus, and they are the only two
	/// constants that have to be read that way to be written at all.
	///
	/// Any other literal with a minus before it is folded into a constant as well, because
	/// that is what C# calls it: `sbyte s = -1` converts only because `-1` is a constant
	/// that fits. And a <c>uint</c> is negated as the <c>long</c> C# makes of it.
	/// </remarks>
	public static Expression Negate(Expression operand, ReadOnlySpan<Reading> reading)
	{
		if (operand is ConstantExpression { Value: var value })
		{
			if (value is 2147483648u)
				return Expression.Constant(int.MinValue);

			if (value is 9223372036854775808ul)
				return Expression.Constant(long.MinValue);

			if (Negative(value) is { } negative)
				return Expression.Constant(negative);
		}

		Func<Expression, UnaryExpression> make = Checked(reading) ? Expression.NegateChecked : Expression.Negate;

		return Operand(_negatables, operand, null) is { } type ? make(Implicitly(operand, type)!) : make(operand);
	}

	/// <summary>A constant's negation, or null where it has none of its own type.</summary>
	static object? Negative(object? value) => value switch
	{
		int number when number != int.MinValue   => -number,
		long number when number != long.MinValue => -number,
		float number                             => -number,
		double number                            => -number,
		decimal number                           => -number,
		_                                        => null,
	};

	/// <remarks>
	/// A cast is where the difference is most visible and least like the others: `(byte)300`
	/// is 44 unchecked and throws checked, and neither is an error the C# compiler would
	/// have caught here — the value is not a constant until the tree is compiled.
	/// </remarks>
	public static Expression Cast(Expression operand, Type type, ReadOnlySpan<Reading> reading) =>
		ReferenceEquals(operand, Null) && CanBeNull(type) ? Expression.Constant(null, type)
		: Checked(reading) ? Expression.ConvertChecked(operand, type)
		: Expression.Convert(operand, type);

	public static Expression AddAssign(Expression target, Expression value, ReadOnlySpan<Reading> reading) =>
		Compound(
			Checked(reading) ? Expression.AddAssignChecked : Expression.AddAssign,
			Add(target, value, reading), target, reading);

	public static Expression SubtractAssign(Expression target, Expression value, ReadOnlySpan<Reading> reading) =>
		Compound(
			Checked(reading) ? Expression.SubtractAssignChecked : Expression.SubtractAssign,
			Subtract(target, value, reading), target, reading);

	public static Expression MultiplyAssign(Expression target, Expression value, ReadOnlySpan<Reading> reading) =>
		Compound(
			Checked(reading) ? Expression.MultiplyAssignChecked : Expression.MultiplyAssign,
			Multiply(target, value, reading), target, reading);

	/// <summary>A compound assignment whose operator is arithmetic and has no checked form.</summary>
	public static Expression ArithmeticAssign(
		Func<Expression, Expression, BinaryExpression> assign, Func<Expression, Expression, BinaryExpression> make,
		Expression target, Expression value, ReadOnlySpan<Reading> reading) =>
		Compound(assign, Arithmetic(make, target, value), target, reading);

	/// <summary>A compound assignment whose operator is bitwise.</summary>
	public static Expression IntegralAssign(
		Func<Expression, Expression, BinaryExpression> assign, Func<Expression, Expression, BinaryExpression> make,
		Expression target, Expression value, ReadOnlySpan<Reading> reading) =>
		Compound(assign, Integral(make, target, value), target, reading);

	/// <summary>A compound assignment whose operator is a shift.</summary>
	public static Expression ShiftAssign(
		Func<Expression, Expression, BinaryExpression> assign, Func<Expression, Expression, BinaryExpression> make,
		Expression target, Expression value, ReadOnlySpan<Reading> reading) =>
		Compound(assign, Shift(make, target, value), target, reading);

	/// <summary>C#'s <c>x op= y</c>, which is <c>x = (T)(x op y)</c>.</summary>
	/// <remarks>
	/// The API's own node where the operator came out in the target's type over the target
	/// itself — `a += 5` over an `int` is <c>AddAssign</c>, as it always was. Where it did not
	/// — a <c>byte</c>, which C#'s arithmetic reads as an <c>int</c>, or a string, which it
	/// concatenates — the cast back is written out, checked where a `checked` stands over it.
	/// Reading the target twice is safe, because a target here is a name or a member of one.
	/// </remarks>
	static Expression Compound(
		Func<Expression, Expression, BinaryExpression> assign, Expression computed, Expression target,
		ReadOnlySpan<Reading> reading) =>
		computed is BinaryExpression binary && binary.Left == target && binary.Type == target.Type &&
		binary.Method?.DeclaringType != typeof(string)
			? assign(target, binary.Right)
			: Expression.Assign(target, computed.Type == target.Type ? computed : Cast(computed, target.Type, reading));

	/// <summary>Whether <see cref="Member"/> would have something to build, asked before it runs.</summary>
	/// <remarks>
	/// The same search <see cref="Member"/> makes, because what this answers has to be what
	/// that one does. No member is not an error here: it means this reading is not a member
	/// access, and something else will read the text.
	/// </remarks>
	public static bool Has(Expression target, string? name)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		return
			name is null ||
			target.Type.IsArray && string.Equals(name, "Length", StringComparison.Ordinal) ||
			InstanceMember(target.Type, name) is not null;
	}

	/// <summary>What <c>a.b</c> reads, which the type of <c>a</c> decides.</summary>
	/// <remarks>
	/// An array's length is a node of this tree — <c>ArrayLength</c> — where every other
	/// type's is a property, and nothing in the syntax says which. It could not be a guard
	/// either: a guard runs while the text is read and the operand of a fold is not built
	/// until after, so the only place that can ask the operand what it is, is here.
	/// </remarks>
	/// <exception cref="FormatException">The type has no such property or field.</exception>
	public static Expression Member(Expression target, string name)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		if (target.Type.IsArray && string.Equals(name, "Length", StringComparison.Ordinal))
			return Expression.ArrayLength(target);

		return InstanceMember(target.Type, name) switch
		{
			PropertyInfo property => Expression.Property(target, property),
			FieldInfo    field    => Expression.Field(target, field),
			_ => throw new FormatException($"'{target.Type.Name}' has no property or field named '{name}'."),
		};
	}

	/// <summary>The public instance property or field a name means on a type, or null.</summary>
	/// <remarks>
	/// <para>
	/// Not <c>Expression.PropertyOrField</c>, which answers a different question in two
	/// ways. It looks through no interface: an interface's base interfaces are not its base
	/// type, so `Count` on an `IList&lt;int&gt;` — which is `ICollection&lt;T&gt;`'s — was no
	/// member at all. And where nothing public matches it goes on to what is not public,
	/// which no C# written outside the type can read.
	/// </para>
	/// <para>
	/// The name exactly as written, as C# reads one. <c>PropertyOrField</c> goes on to the
	/// same name in another case, and <c>Expression.Call</c> does the same for a method, so
	/// both of them read `s.length` as `s.Length`.
	/// </para>
	/// </remarks>
	static MemberInfo? InstanceMember(Type type, string name)
	{
		const BindingFlags Declared = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		// The type and what it derives from, and — for an interface — what it inherits.
		for (var each = type; each is not null; each = each.BaseType)
			if (DeclaredOn(each, name, Declared) is { } found)
				return found;

		if (type.IsInterface)
			foreach (var inherited in type.GetInterfaces())
				if (DeclaredOn(inherited, name, Declared) is { } found)
					return found;

		return null;
	}

	/// <summary>
	/// A field or a property that takes no index, declared by that type itself. An array
	/// rather than <c>GetProperty</c>, which throws where it finds more than one.
	/// </summary>
	static MemberInfo? DeclaredOn(Type type, string name, BindingFlags flags)
	{
		foreach (var member in type.GetMember(name, MemberTypes.Property | MemberTypes.Field, flags))
			if (member is FieldInfo || member is PropertyInfo property && property.GetIndexParameters().Length == 0)
				return member;

		return null;
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

		return target.Type.IsArray ? Expression.ArrayAccess(target, Converted(at, typeof(int))) : Indexed(target, at);
	}

	/// <summary>What <c>a[i]</c> reads, likewise.</summary>
	/// <remarks>
	/// An array's element is a node of this tree and anything else's is an indexer — whose
	/// name is not always <c>Item</c>, `string` calling its own <c>Chars</c>. The type says
	/// which through its default member, which is what an indexer is, and which of several
	/// is meant is the same overload resolution a call makes.
	/// </remarks>
	public static Expression Indexed(Expression target, Expression[] at)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		if (at is null)
			throw new ArgumentNullException(nameof(at));

		if (target.Type.IsArray)
			return Expression.ArrayIndex(target, Converted(at, typeof(int)));

		var chosen = Resolved(Indexers(target.Type, at), at, $"'{target.Type.Name}' has no indexer");

		return Expression.Property(target, (PropertyInfo)chosen.Member, Passed(chosen, at));
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

	// ── Calls: the overload C# would choose ─────────────────────────────────────
	//
	// `Expression.Call` takes a method by name and chooses among the overloads itself, and
	// chooses by a rule that is not C#'s: an argument has to be of its parameter's type or
	// assignable to it by reference, no overload is better than another, and a name matches
	// in any case. `Expression.New` and `Expression.Property` take the member and choose
	// nothing. So the choosing is done here, once, for all four — a method, a constructor,
	// an indexer, a delegate — by C#'s rules over the conversions below: which forms are
	// applicable, a `params` array written out one by one, a default for what is left out,
	// and then the one better than every other.

	/// <summary>A call on a value: the method C# would choose for these arguments.</summary>
	public static Expression Called(Expression target, string name, Expression[] arguments)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		var chosen = Resolved(
			Methods(target.Type, name, instance: true, arguments), arguments,
			$"'{target.Type.Name}' has no method '{name}'");

		return Expression.Call(target, (MethodInfo)chosen.Member, Passed(chosen, arguments));
	}

	/// <summary>A call on a type: the static method C# would choose for these arguments.</summary>
	public static Expression Called(Type type, string name, Expression[] arguments)
	{
		if (type is null)
			throw new ArgumentNullException(nameof(type));

		var chosen = Resolved(
			Methods(type, name, instance: false, arguments), arguments, $"'{type.Name}' has no method '{name}'");

		return Expression.Call((MethodInfo)chosen.Member, Passed(chosen, arguments));
	}

	/// <summary>A delegate called, its arguments converted to what it takes.</summary>
	public static Expression Invoked(Expression target, Expression[] arguments)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		// What is not a delegate is the API's to refuse, in its own words.
		if (!typeof(Delegate).IsAssignableFrom(target.Type) || target.Type.GetMethod("Invoke") is not { } invoke)
			return Expression.Invoke(target, arguments);

		var chosen = Applicable(invoke, invoke.GetParameters(), arguments) ?? throw new InvalidOperationException(
			$"'{target.Type.Name}' cannot be invoked with ({Listing(arguments)}).");

		return Expression.Invoke(target, Passed(chosen, arguments));
	}

	/// <summary>The constructor C# would choose for these arguments, called with them.</summary>
	/// <remarks>
	/// A value type's constructor of no arguments is not in its metadata at all, and
	/// <c>Expression.New</c> has a form for it that takes the type alone.
	/// </remarks>
	static NewExpression Constructed(Type type, Expression[] arguments)
	{
		if (arguments.Length == 0 && type.IsValueType)
			return Expression.New(type);

		var found = new List<Candidate>();

		foreach (var (constructor, parameters) in _constructors.GetOrAdd(
			type, static type => [.. type.GetConstructors().Select(static one => ((MemberInfo)one, one.GetParameters()))]))
			if (Applicable(constructor, parameters, arguments) is { } candidate)
				found.Add(candidate);

		var chosen = Resolved(found, arguments, $"'{type.Name}' has no constructor");

		return Expression.New((ConstructorInfo)chosen.Member, Passed(chosen, arguments));
	}

	/// <summary>One way a call could be read: the member, and the form its arguments take.</summary>
	/// <param name="Expanded">Whether a <c>params</c> array's elements were written one by one.</param>
	/// <param name="Defaults">How many optional parameters were left to their defaults.</param>
	readonly record struct Candidate(MemberInfo Member, ParameterInfo[] Parameters, bool Expanded, int Defaults)
	{
		/// <summary>The type the argument at that position is converted to.</summary>
		public Type At(int position) =>
			Expanded && position >= Parameters.Length - 1
				? Parameters[Parameters.Length - 1].ParameterType.GetElementType()!
				: Parameters[position].ParameterType;
	}

	/// <summary>The methods by that name the arguments fit, by the name exactly as written.</summary>
	/// <remarks>
	/// An interface's own methods do not include what it inherits, nor <c>object</c>'s, and a
	/// value of an interface type has both — `list.Contains(1)` on an <c>IList&lt;int&gt;</c>
	/// is <c>ICollection&lt;T&gt;</c>'s. A generic method is no candidate: nothing here can
	/// name its type arguments, and nothing infers them.
	/// </remarks>
	static List<Candidate> Methods(Type type, string name, bool instance, Expression[] arguments)
	{
		var found = new List<Candidate>();

		foreach (var (method, parameters) in _methods.GetOrAdd((type, name, instance), static key => Named(key)))
			if (Applicable(method, parameters, arguments) is { } candidate)
				found.Add(candidate);

		return found;
	}

	/// <summary>The methods a type has by that name, with their parameters, before any argument is asked.</summary>
	static (MemberInfo, ParameterInfo[])[] Named((Type Type, string Name, bool Instance) key)
	{
		var named = new List<(MemberInfo, ParameterInfo[])>();

		Consider(key.Type.GetMethods(
			key.Instance
				? BindingFlags.Public | BindingFlags.Instance
				: BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy));

		if (key.Instance && key.Type.IsInterface)
		{
			foreach (var inherited in key.Type.GetInterfaces())
				Consider(inherited.GetMethods(BindingFlags.Public | BindingFlags.Instance));

			Consider(typeof(object).GetMethods(BindingFlags.Public | BindingFlags.Instance));
		}

		return [.. named];

		void Consider(MethodInfo[] methods)
		{
			foreach (var method in methods)
				if (string.Equals(method.Name, key.Name, StringComparison.Ordinal) && !method.ContainsGenericParameters)
					named.Add((method, method.GetParameters()));
		}
	}

	// What reflection answers about a type is the same every time it is asked, and costs an
	// allocation every time: a method's parameters are copied out on each `GetParameters`. A
	// call over `Math.Max` asks about a dozen overloads and, for each argument, whether
	// `IntPtr` declares a conversion to it — so each answer is kept once it has been worked
	// out, as the names of types already are.

	static readonly ConcurrentDictionary<(Type, string, bool), (MemberInfo, ParameterInfo[])[]> _methods = new();

	static readonly ConcurrentDictionary<Type, (MemberInfo, ParameterInfo[])[]> _constructors = new();

	static readonly ConcurrentDictionary<(Type, Type), MethodInfo?> _operators = new();

	/// <summary>The indexers the arguments fit, an interface's inherited ones among them.</summary>
	static List<Candidate> Indexers(Type type, Expression[] arguments)
	{
		var found = new List<Candidate>();

		Consider(type);

		if (type.IsInterface)
			foreach (var inherited in type.GetInterfaces())
				Consider(inherited);

		return found;

		void Consider(Type declaring)
		{
			foreach (var member in declaring.GetDefaultMembers())
				if (member is PropertyInfo indexer && indexer.GetIndexParameters() is { Length: > 0 } parameters &&
					Applicable(indexer, parameters, arguments) is { } candidate)
					found.Add(candidate);
		}
	}

	/// <summary>Whether the arguments fit, and in which form (C#'s applicable function member).</summary>
	/// <remarks>
	/// The normal form first — an argument for each parameter, and a default for each one
	/// after — and then, where the last parameter is a <c>params</c> array, the expanded one,
	/// with that array's elements written one by one. A parameter taken by reference, or of a
	/// type an expression tree cannot hold, makes the member no candidate at all: chosen, it
	/// would build a tree that does not compile, where the overload beside it would have.
	/// </remarks>
	static Candidate? Applicable(MemberInfo member, ParameterInfo[] parameters, Expression[] arguments)
	{
		foreach (var parameter in parameters)
			if (parameter.ParameterType.IsByRef || Unrepresentable(parameter.ParameterType))
				return null;

		var count = parameters.Length;

		if (arguments.Length <= count)
		{
			var fits = true;

			for (var at = 0; at < arguments.Length && fits; at++)
				fits = Converts(arguments[at], parameters[at].ParameterType);

			for (var at = arguments.Length; at < count && fits; at++)
				fits = parameters[at].IsOptional;

			if (fits)
				return new Candidate(member, parameters, false, count - arguments.Length);
		}

		if (count > 0 && arguments.Length >= count - 1 &&
			parameters[count - 1].IsDefined(typeof(ParamArrayAttribute), false))
		{
			var element = parameters[count - 1].ParameterType.GetElementType()!;
			var fits    = true;

			for (var at = 0; at < arguments.Length && fits; at++)
				fits = Converts(arguments[at], at < count - 1 ? parameters[at].ParameterType : element);

			if (fits)
				return new Candidate(member, parameters, true, 0);
		}

		return null;
	}

	/// <summary>A ref struct, which an expression tree cannot hold — <c>Span&lt;T&gt;</c> and its kind.</summary>
	static bool Unrepresentable(Type type) =>
#if NETSTANDARD2_0
		type.IsValueType && type.GetCustomAttributesData().Any(
			static attribute => attribute.AttributeType.FullName == "System.Runtime.CompilerServices.IsByRefLikeAttribute");
#else
		type.IsByRefLike;
#endif

	/// <summary>The one candidate better than every other, which is the one C# calls.</summary>
	/// <remarks>
	/// Methods a more derived type declared stand in front of its base types' first, as C#
	/// has them (§12.8.10.2): an override is found once, and a method hidden by `new` is not
	/// found beside the one hiding it.
	/// </remarks>
	/// <exception cref="InvalidOperationException">No candidate, or two that neither is better than.</exception>
	static Candidate Resolved(List<Candidate> found, Expression[] arguments, string missing)
	{
		var standing = found.FindAll(one => !found.Exists(other =>
			other.Member.DeclaringType != one.Member.DeclaringType &&
			one.Member.DeclaringType!.IsAssignableFrom(other.Member.DeclaringType)));

		foreach (var one in standing)
			if (standing.TrueForAll(other => other.Equals(one) || Compared(one, other, arguments) > 0))
				return one;

		if (standing.Count == 0)
			throw new InvalidOperationException($"{missing} taking ({Listing(arguments)}).");

		throw new InvalidOperationException(
			$"The call is ambiguous between '{standing[0].Member}' and '{standing[1].Member}'.");
	}

	/// <summary>Which of two candidates is the better function member, as C# decides it.</summary>
	/// <returns>Above zero where the first is, below where the second is, and zero where neither.</returns>
	static int Compared(Candidate first, Candidate second, Expression[] arguments)
	{
		var firstBetter  = false;
		var secondBetter = false;

		for (var at = 0; at < arguments.Length; at++)
		{
			var better = Better(arguments[at], first.At(at), second.At(at));

			firstBetter  |= better > 0;
			secondBetter |= better < 0;
		}

		if (firstBetter != secondBetter)
			return firstBetter ? 1 : -1;

		if (firstBetter)
			return 0;

		// Every argument converted equally well: the tie-breakers. The normal form over the
		// expanded one, and no defaults over some.
		if (first.Expanded != second.Expanded)
			return first.Expanded ? -1 : 1;

		if (first.Defaults == 0 != (second.Defaults == 0))
			return first.Defaults == 0 ? 1 : -1;

		return 0;
	}

	/// <summary>Which of two conversions of one argument is better (C#'s better conversion).</summary>
	static int Better(Expression argument, Type first, Type second)
	{
		if (first == second)
			return 0;

		if (!ReferenceEquals(argument, Null))
		{
			if (argument.Type == first)
				return 1;

			if (argument.Type == second)
				return -1;
		}

		return BetterTarget(first, second);
	}

	/// <summary>
	/// Which of two types is the better one to convert to: the one that converts to the
	/// other and not back, and a signed type over an unsigned one where neither does.
	/// </summary>
	static int BetterTarget(Type first, Type second)
	{
		var down = Standard(first, second);
		var up   = Standard(second, first);

		if (down != up)
			return down ? 1 : -1;

		if (IsSigned(first) && IsUnsigned(second))
			return 1;

		if (IsSigned(second) && IsUnsigned(first))
			return -1;

		return 0;
	}

	static bool IsSigned(Type type) =>
		type == typeof(sbyte) || type == typeof(short) || type == typeof(int) || type == typeof(long);

	static bool IsUnsigned(Type type) =>
		type == typeof(byte) || type == typeof(ushort) || type == typeof(uint) || type == typeof(ulong);

	/// <summary>The arguments as the chosen candidate takes them.</summary>
	/// <remarks>
	/// Each converted to its parameter, a default for each optional parameter left out, and
	/// the tail of an expanded call gathered into the array the <c>params</c> parameter is.
	/// </remarks>
	static Expression[] Passed(Candidate chosen, Expression[] arguments)
	{
		var parameters = chosen.Parameters;
		var passed     = new Expression[parameters.Length];
		var fixedCount = chosen.Expanded ? parameters.Length - 1 : parameters.Length;

		for (var at = 0; at < fixedCount; at++)
			passed[at] = at < arguments.Length
				? Implicitly(arguments[at], parameters[at].ParameterType)!
				: Defaulted(parameters[at]);

		if (chosen.Expanded)
		{
			var element = parameters[fixedCount].ParameterType.GetElementType()!;
			var rest    = new Expression[arguments.Length - fixedCount];

			for (var at = 0; at < rest.Length; at++)
				rest[at] = Implicitly(arguments[fixedCount + at], element)!;

			passed[fixedCount] = Expression.NewArrayInit(element, rest);
		}

		return passed;
	}

	/// <summary>What an optional parameter left out is worth.</summary>
	/// <remarks>
	/// Metadata keeps an enum's default as its underlying number, so it is made the enum
	/// again; a default of <c>null</c> or <c>default</c> is the type's default.
	/// </remarks>
	static Expression Defaulted(ParameterInfo parameter)
	{
		var type = parameter.ParameterType;

		if (!parameter.HasDefaultValue || parameter.DefaultValue is not { } value)
			return Expression.Default(type);

		var underlying = Underlying(type);

		if (underlying.IsEnum && value.GetType() != underlying)
			value = Enum.ToObject(underlying, value);

		return Expression.Constant(value, type);
	}

	/// <summary>The types of some arguments, for a message.</summary>
	static string Listing(Expression[] arguments) => string.Join(", ", arguments.Select(Shown));

	/// <summary>An expression's type for a message, and the literal <c>null</c> as C# names it.</summary>
	static string Shown(Expression value) => ReferenceEquals(value, Null) ? "<null>" : value.Type.Name;

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
				Expression.Bind(member, Converted(setting.Value!, MemberType(member)));
		}

		return bound;
	}

	/// <summary>What a field or property holds, which a nested initializer is written in.</summary>
	static Type MemberType(MemberInfo member) =>
		member is PropertyInfo property ? property.PropertyType : ((FieldInfo)member).FieldType;

	/// <summary>Those elements against the collection type that has the `Add`.</summary>
	/// <remarks>
	/// The overload is chosen by the arguments, the same way a call's is — and for the same
	/// reason it is done here and not in the grammar: what `Add` a collection has is a
	/// question about the type, and the type is a sibling of the braces rather than
	/// something inside them.
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
			var chosen    = Resolved(
				Methods(type, "Add", instance: true, arguments), arguments, $"'{type.Name}' has no method 'Add'");

			added[at] = Expression.ElementInit((MethodInfo)chosen.Member, Passed(chosen, arguments));
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
		var made = Constructed(type, args);

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
	///
	/// Agreeing is what <see cref="Chosen"/> asks of a `?:` — one branch converts to the
	/// other's type and not back — and where neither does, the answer is <c>void</c> rather
	/// than a refusal: an `if` is a statement first.
	/// </remarks>
	public static Expression Branched(Expression test, Expression then, Expression otherwise) =>
		Common(then, otherwise) is { } type
			? Expression.Condition(test, Implicitly(then, type)!, Implicitly(otherwise, type)!, type)
			: Expression.Condition(test, then, otherwise, typeof(void));

	/// <summary>A <c>?:</c> where one was written, and the test alone where none was.</summary>
	/// <remarks>
	/// Typed as C# types one: where one branch converts to the other's type and not back,
	/// that type, and a literal <c>null</c> takes the type of the branch beside it. Where
	/// neither — `c ? 1 : "a"` — it is refused as C# refuses it (CS0173), and not made
	/// <c>void</c> as an `if` would be: a `?:` is always worth something.
	/// </remarks>
	public static Expression Chosen(Expression test, Expression? then, Expression? otherwise)
	{
		if (then is null || otherwise is null)
			return test;

		var type = Common(then, otherwise) ?? throw new InvalidOperationException(
			"Type of conditional expression cannot be determined because there is no implicit " +
			$"conversion between '{Shown(then)}' and '{Shown(otherwise)}'.");

		return Expression.Condition(test, Implicitly(then, type)!, Implicitly(otherwise, type)!);
	}

	/// <summary>The one type two branches meet in, or null where they meet in none.</summary>
	/// <remarks>
	/// Asked of the expressions first, so that a constant converts as a constant: `c ? u : 1`
	/// over a <c>ulong</c> is a <c>ulong</c>. Where both convert that way the types decide,
	/// as `c ? (byte)1 : 1` is an <c>int</c> — the byte widens and the int does not narrow.
	/// </remarks>
	static Type? Common(Expression first, Expression second)
	{
		if (first.Type == second.Type)
			return first.Type;

		if (ReferenceEquals(first, Null))
			return CanBeNull(second.Type) ? second.Type : null;

		if (ReferenceEquals(second, Null))
			return CanBeNull(first.Type) ? first.Type : null;

		var toSecond = Converts(first, second.Type);
		var toFirst  = Converts(second, first.Type);

		if (toSecond && toFirst)
		{
			toSecond = Standard(first.Type, second.Type);
			toFirst  = Standard(second.Type, first.Type);
		}

		return toSecond == toFirst ? null : toSecond ? second.Type : first.Type;
	}

	/// <summary>A <c>??</c> where one was written, and the left side where none was.</summary>
	/// <remarks>
	/// C#'s three cases in C#'s order: the right side converted to what the left one holds,
	/// to the left side's own type, and then the left side converted to the right's. Where
	/// none applies, the API is asked as written and refuses in its own words.
	/// </remarks>
	public static Expression Coalesced(Expression left, Expression? right)
	{
		if (right is null)
			return left;

		var held = Nullable.GetUnderlyingType(left.Type);

		if (held is not null && Implicitly(right, held) is { } plain)
			return Expression.Coalesce(left, plain);

		if (Implicitly(right, left.Type) is { } same)
			return Expression.Coalesce(left, same);

		if (Standard(held ?? left.Type, right.Type))
			return Expression.Coalesce(
				Expression.Convert(left, CanBeNull(right.Type) ? right.Type : Lifted(right.Type)), right);

		return Expression.Coalesce(left, right);
	}

	/// <summary>An integer constant, typed the way C# types one.</summary>
	/// <remarks>
	/// <para>
	/// The first of a list of types that holds the value, and the suffix is what says which
	/// list: none is <c>int</c>, <c>uint</c>, <c>long</c>, <c>ulong</c>; <c>u</c> is
	/// <c>uint</c>, <c>ulong</c>; <c>l</c> is <c>long</c>, <c>ulong</c>; <c>ul</c> is
	/// <c>ulong</c> alone.
	/// </para>
	/// <para>
	/// The base changes nothing about it. `0xFFFFFFFF` is the <c>uint</c> 4294967295, as it
	/// is in C#, where reading the digits as an <c>int</c>'s bits — which is what
	/// <c>Convert.ToInt32(digits, 16)</c> does — makes it −1 without a word.
	/// </para>
	/// </remarks>
	/// <param name="digits">The digits alone: no prefix, no suffix, no separator.</param>
	/// <param name="radix">The base they are written in: 2, 10 or 16.</param>
	/// <param name="unsigned">Whether a <c>u</c> struck the signed types off the list.</param>
	/// <param name="wide">Whether an <c>l</c> struck the 32-bit types off the list.</param>
	/// <exception cref="OverflowException">No type in the list holds it, which C# refuses as well (CS1021).</exception>
	public static ConstantExpression Integer(string digits, int radix, bool unsigned, bool wide)
	{
		var value = radix == 10
			? ulong.Parse(digits, NumberStyles.None, CultureInfo.InvariantCulture)
			: Convert.ToUInt64(digits, radix);

		return !unsigned && !wide && value <= int.MaxValue ? Expression.Constant((int)value)
			: !wide && value <= uint.MaxValue              ? Expression.Constant((uint)value)
			: !unsigned && value <= long.MaxValue          ? Expression.Constant((long)value)
			: Expression.Constant(value);
	}

	// ── Conversions: the ones C# makes without being asked ──────────────────────
	//
	// `System.Linq.Expressions` builds every conversion there is — `Expression.Convert` takes
	// a numeric widening, a boxing, a nullable, a user-defined `op_Implicit` alike — and
	// decides none of them: `Expression.Add` over an `int` and a `double` is refused rather
	// than widened. Which conversion C# would make unasked is a question about types, and the
	// API keeps its own answer to it internal. So the answer is here, written from the C#
	// specification, and every conversion it chooses is built by `Expression.Convert`.

	/// <summary>The literal <c>null</c>, one node wherever it is written.</summary>
	/// <remarks>
	/// C# types <c>null</c> by where it stands, and the conversions below are where that is
	/// decided — so the literal has to be told apart from an <c>object</c> that happens to be
	/// null, and being this node is how. Typed <c>object</c> until something converts it.
	/// </remarks>
	public static readonly ConstantExpression Null = Expression.Constant(null, typeof(object));

	/// <summary>C#'s predefined arithmetic operators take one of these, in this order.</summary>
	static readonly Type[] _arithmetics =
		[typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal)];

	/// <summary>And its bitwise ones and its shifts one of these.</summary>
	static readonly Type[] _integrals = [typeof(int), typeof(uint), typeof(long), typeof(ulong)];

	/// <summary>And its unary minus one of these: there is none over an unsigned type.</summary>
	static readonly Type[] _negatables = [typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)];

	static readonly MethodInfo _concatStrings =
		typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)])!;

	static readonly MethodInfo _concatObjects =
		typeof(string).GetMethod(nameof(string.Concat), [typeof(object), typeof(object)])!;

	/// <summary>The value converted to that type as C# converts implicitly, or as it is where C# would not.</summary>
	/// <remarks>
	/// Unconverted rather than refused where there is no conversion: the factory it is handed
	/// to is what refuses it, in its own words — `Expression.Assign` says which type cannot be
	/// assigned to which, better than a message this could invent.
	/// </remarks>
	public static Expression Converted(Expression value, Type to)
	{
		if (value is null)
			throw new ArgumentNullException(nameof(value));

		return Implicitly(value, to) ?? value;
	}

	/// <summary>Each of those values converted to that type.</summary>
	public static Expression[] Converted(Expression[] values, Type to)
	{
		if (values is null)
			throw new ArgumentNullException(nameof(values));

		var converted = new Expression[values.Length];

		for (var at = 0; at < values.Length; at++)
			converted[at] = Converted(values[at], to);

		return converted;
	}

	/// <summary>An assignment, the value converted to what it is assigned to.</summary>
	public static Expression Assigned(Expression target, Expression value)
	{
		if (target is null)
			throw new ArgumentNullException(nameof(target));

		return Expression.Assign(target, Converted(value, target.Type));
	}

	/// <summary>A switch's cases, each test converted to the type of what is switched on.</summary>
	/// <remarks>
	/// A case is built before the switch it belongs to, so its test is typed by itself:
	/// `case 1:` is an <c>int</c> until it meets the <c>byte</c> it is compared with.
	/// </remarks>
	public static SwitchCase[] Against(SwitchCase[] cases, Type type)
	{
		if (cases is null)
			throw new ArgumentNullException(nameof(cases));

		var against = new SwitchCase[cases.Length];

		for (var at = 0; at < cases.Length; at++)
			against[at] = Expression.SwitchCase(cases[at].Body, Converted([.. cases[at].TestValues], type));

		return against;
	}

	/// <summary>The implicit conversion C# makes from that value to that type, or null where it makes none.</summary>
	/// <remarks>
	/// An expression and not only a type, because two of C#'s conversions are about the value:
	/// the literal <c>null</c> converts to anything that can be null, and a constant converts
	/// to a narrower type it fits in — `byte b = 1` is an <c>int</c> that fits, and a
	/// literal 0 is any enum. A constant is converted by making the constant it becomes, so
	/// `1 + x` over a <c>double</c> reads the literal 1.0 rather than a conversion of 1.
	/// </remarks>
	static Expression? Implicitly(Expression value, Type to)
	{
		var from = value.Type;

		if (from == to)
			return value;

		if (ReferenceEquals(value, Null))
			return CanBeNull(to) ? Expression.Constant(null, to) : null;

		if (value is ConstantExpression { Value: { } constant })
		{
			if (Narrowed(constant, to) is { } narrowed)
				return Expression.Constant(narrowed, to);

			if (IsNumeric(from) && IsNumeric(Underlying(to)) && Standard(from, to))
				return Expression.Constant(Changed(constant, Underlying(to)), to);
		}

		if (Standard(from, to))
			return Expression.Convert(value, to);

		return UserDefined(from, to) is { } method ? Through(value, method, to) : null;
	}

	/// <summary>Whether <see cref="Implicitly"/> would find a conversion, asked without building one.</summary>
	/// <remarks>
	/// The question overload resolution and promotion ask of every candidate and every
	/// argument, most of them to be turned down — so it is answered without the nodes that
	/// only the chosen one needs.
	/// </remarks>
	static bool Converts(Expression value, Type to)
	{
		var from = value.Type;

		if (from == to)
			return true;

		if (ReferenceEquals(value, Null))
			return CanBeNull(to);

		if (value is ConstantExpression { Value: { } constant } && Narrowed(constant, to) is not null)
			return true;

		return Standard(from, to) || UserDefined(from, to) is not null;
	}

	/// <summary>
	/// A standard implicit conversion between two types: identity, numeric widening,
	/// nullable, reference and boxing — everything C# converts without an operator.
	/// </summary>
	static bool Standard(Type from, Type to)
	{
		if (from == to)
			return true;

		if (Nullable.GetUnderlyingType(to) is { } target)
		{
			var source = Underlying(from);

			return source == target || Widens(source, target);
		}

		if (Nullable.GetUnderlyingType(from) is null && Widens(from, to))
			return true;

		return !to.IsValueType && to.IsAssignableFrom(from);
	}

	/// <summary>C#'s implicit numeric conversions, which only ever widen.</summary>
	static bool Widens(Type from, Type to)
	{
		if (from.IsEnum || to.IsEnum)
			return false;

		return (Type.GetTypeCode(from), Type.GetTypeCode(to)) switch
		{
			(TypeCode.SByte,
			 TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.Byte,
			 TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or
			 TypeCode.Int64 or TypeCode.UInt64 or
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.Int16,
			 TypeCode.Int32 or TypeCode.Int64 or
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.UInt16,
			 TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.Char,
			 TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 or
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.Int32,
			 TypeCode.Int64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal)  => true,
			(TypeCode.UInt32,
			 TypeCode.Int64 or TypeCode.UInt64 or
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.Int64 or TypeCode.UInt64,
			 TypeCode.Single or TypeCode.Double or TypeCode.Decimal)                     => true,
			(TypeCode.Single, TypeCode.Double)                                           => true,
			_                                                                            => false,
		};
	}

	/// <summary>A constant in a narrower type it fits in, or null where C# converts it to none.</summary>
	/// <remarks>
	/// An <c>int</c> to any integral type that holds it, a <c>long</c> to a <c>ulong</c> where
	/// it is not negative, and a zero of any integral type to any enum.
	/// </remarks>
	static object? Narrowed(object constant, Type to)
	{
		var target = Underlying(to);

		if (target.IsEnum)
			return constant is 0 or 0u or 0L or 0ul ? Enum.ToObject(target, 0) : null;

		return constant switch
		{
			int number => Type.GetTypeCode(target) switch
			{
				TypeCode.SByte  when number is >= sbyte.MinValue and <= sbyte.MaxValue  => (sbyte)number,
				TypeCode.Byte   when number is >= byte.MinValue and <= byte.MaxValue    => (byte)number,
				TypeCode.Int16  when number is >= short.MinValue and <= short.MaxValue  => (short)number,
				TypeCode.UInt16 when number is >= ushort.MinValue and <= ushort.MaxValue => (ushort)number,
				TypeCode.UInt32 when number >= 0                                        => (uint)number,
				TypeCode.UInt64 when number >= 0                                        => (ulong)number,
				_                                                                       => null,
			},
			long number when number >= 0 && target == typeof(ulong) => (ulong)number,
			_                                                         => null,
		};
	}

	/// <summary>A numeric constant as the number of a wider type.</summary>
	/// <remarks>A <c>char</c> goes through its code, which is all it converts as.</remarks>
	static object Changed(object constant, Type to) =>
		Convert.ChangeType(constant is char character ? (int)character : constant, to, CultureInfo.InvariantCulture);

	/// <summary>The user-defined implicit conversion from one type to another, or null.</summary>
	/// <remarks>
	/// Looked for where C# looks — the two types and their bases — among operators whose
	/// parameter the source reaches and whose result reaches the target by a standard
	/// conversion. One such is the answer; of several, the one that is exact at both ends,
	/// and where that is not one either, none: C# calls that ambiguous. Two predefined
	/// numeric types have no user-defined conversion between them even where one is written,
	/// as <c>decimal</c>'s are, and nothing converts to or from an interface this way.
	/// </remarks>
	static MethodInfo? UserDefined(Type from, Type to) =>
		IsNumeric(Underlying(from)) && IsNumeric(Underlying(to))
			? null
			: _operators.GetOrAdd((from, to), static pair => Declared(pair.Item1, pair.Item2));

	/// <summary>The search <see cref="UserDefined"/> makes, once for each pair of types.</summary>
	static MethodInfo? Declared(Type from, Type to)
	{
		var source = Underlying(from);
		var target = Underlying(to);

		if (source.IsInterface || target.IsInterface)
			return null;

		var found = new List<MethodInfo>();

		Consider(source);
		Consider(target);

		if (found.Count == 1)
			return found[0];

		var exact = found.FindAll(method => method.GetParameters()[0].ParameterType == from && method.ReturnType == to);

		return exact.Count == 1 ? exact[0] : null;

		void Consider(Type start)
		{
			for (var type = start; type is not null && type != typeof(object); type = type.BaseType)
				foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
					if (method.Name == "op_Implicit" && !found.Contains(method) &&
						method.GetParameters() is { Length: 1 } parameters &&
						Standard(from, parameters[0].ParameterType) && Standard(method.ReturnType, to))
						found.Add(method);
		}
	}

	/// <summary>The value through a user-defined operator, with a standard conversion either side.</summary>
	static Expression Through(Expression value, MethodInfo method, Type to)
	{
		var input  = Implicitly(value, method.GetParameters()[0].ParameterType)!;
		var result = Expression.Convert(input, method.ReturnType, method);

		return method.ReturnType == to ? result : Expression.Convert(result, to);
	}

	/// <summary>Whether a type is one of C#'s numeric types, <c>char</c> among them.</summary>
	static bool IsNumeric(Type type) =>
		!type.IsEnum && Type.GetTypeCode(type) is >= TypeCode.Char and <= TypeCode.Decimal;

	static Type Underlying(Type type) => Nullable.GetUnderlyingType(type) ?? type;

	static Type Lifted(Type type) => typeof(Nullable<>).MakeGenericType(type);

	static bool CanBeNull(Type type) => !type.IsValueType || Nullable.GetUnderlyingType(type) is not null;

	static bool IsLifted(Expression operand) =>
		ReferenceEquals(operand, Null) || Nullable.GetUnderlyingType(operand.Type) is not null;

	// ── Operators: C#'s predefined ones, and the promotion they imply ───────────
	//
	// C#'s binary numeric promotion is its overload resolution over the predefined operators,
	// and it is written that way here: every type of the operator's list both operands
	// convert to is a candidate, and the answer is the one better than all the others. That
	// is what makes `byte + byte` an `int`, `int + uint` a `long`, `uint + 1` a `uint` — the
	// constant fits — and `long + ulong` nothing at all, as in C#. Lifted, where either side
	// is nullable, to the nullable of the same answer. Where either operand is not a
	// predefined numeric type the API is asked as written, which is where a user-defined
	// operator is found — `DateTime - TimeSpan` — and where a refusal is said in its words.

	/// <summary>The type C# promotes the operands to, or null where no predefined operator applies.</summary>
	static Type? Operand(Type[] among, Expression left, Expression? right)
	{
		// The common case: one type already, and one the operator takes.
		if (!ReferenceEquals(left, Null) &&
			(right is null || right.Type == left.Type && !ReferenceEquals(right, Null)) &&
			Array.IndexOf(among, Underlying(left.Type)) >= 0)
			return left.Type;

		if (!Predefined(left) || right is not null && !Predefined(right))
			return null;

		var lifted = IsLifted(left) || right is not null && IsLifted(right);
		var fits   = new List<Type>(among.Length);

		foreach (var each in among)
		{
			var operand = lifted ? Lifted(each) : each;

			if (Converts(left, operand) && (right is null || Converts(right, operand)))
				fits.Add(each);
		}

		foreach (var each in fits)
			if (fits.TrueForAll(other => other == each || BetterTarget(each, other) > 0))
				return lifted ? Lifted(each) : each;

		return null;
	}

	/// <summary>Whether an operand is one C#'s predefined numeric operators could take.</summary>
	static bool Predefined(Expression operand) =>
		ReferenceEquals(operand, Null) || IsNumeric(Underlying(operand.Type));

	/// <summary>An arithmetic operator — <c>+ - * / %</c> — over operands promoted as C# promotes them.</summary>
	public static Expression Arithmetic(
		Func<Expression, Expression, BinaryExpression> make, Expression left, Expression right) =>
		Operand(_arithmetics, left, right) is { } type
			? make(Implicitly(left, type)!, Implicitly(right, type)!)
			: make(left, right);

	/// <summary>The unary <c>+</c>, likewise.</summary>
	public static Expression Arithmetic(Func<Expression, UnaryExpression> make, Expression operand) =>
		Operand(_arithmetics, operand, null) is { } type ? make(Implicitly(operand, type)!) : make(operand);

	/// <summary>A bitwise operator — <c>&amp; | ^</c> — over integers, <c>bool</c>s or one enum.</summary>
	/// <remarks>
	/// An enum's flags are combined as its underlying integers and made the enum again, which
	/// is what C# does and the API has no operator for.
	/// </remarks>
	public static Expression Integral(
		Func<Expression, Expression, BinaryExpression> make, Expression left, Expression right)
	{
		if (Operand(_integrals, left, right) is { } type)
			return make(Implicitly(left, type)!, Implicitly(right, type)!);

		var (first, second) = Unified(left, right);

		return first.Type == second.Type && Underlying(first.Type).IsEnum
			? Expression.Convert(make(AsUnderlying(first), AsUnderlying(second)), first.Type)
			: make(first, second);
	}

	/// <summary>The unary <c>~</c>, likewise.</summary>
	public static Expression Integral(Func<Expression, UnaryExpression> make, Expression operand) =>
		Operand(_integrals, operand, null) is { } type ? make(Implicitly(operand, type)!)
		: Underlying(operand.Type).IsEnum ? Expression.Convert(make(AsUnderlying(operand)), operand.Type)
		: make(operand);

	/// <summary>A shift: the left side promoted on its own, and the count an <c>int</c>.</summary>
	public static Expression Shift(
		Func<Expression, Expression, BinaryExpression> make, Expression left, Expression right)
	{
		if (Operand(_integrals, left, null) is not { } promoted)
			return make(left, right);

		var lifted = IsLifted(left) || IsLifted(right);
		var type   = lifted ? Lifted(Underlying(promoted)) : promoted;
		var count  = lifted ? typeof(int?) : typeof(int);

		return Implicitly(left, type) is { } shifted && Implicitly(right, count) is { } by
			? make(shifted, by)
			: make(left, right);
	}

	/// <summary><c>==</c> and <c>!=</c>: numbers promoted, and otherwise one side converted to the other's type.</summary>
	/// <remarks>
	/// Which is how `s == null` compares a string with a string, `n == 3` a nullable with a
	/// nullable, and `e == 0` an enum with the enum a literal zero converts to.
	/// </remarks>
	public static Expression Equality(
		Func<Expression, Expression, BinaryExpression> make, Expression left, Expression right)
	{
		if (Operand(_arithmetics, left, right) is { } type)
			return make(Implicitly(left, type)!, Implicitly(right, type)!);

		var (first, second) = Unified(left, right);

		return make(first, second);
	}

	/// <summary><c>&lt; &gt; &lt;= &gt;=</c>: likewise, and an enum ordered by its underlying number.</summary>
	public static Expression Relational(
		Func<Expression, Expression, BinaryExpression> make, Expression left, Expression right)
	{
		if (Operand(_arithmetics, left, right) is { } type)
			return make(Implicitly(left, type)!, Implicitly(right, type)!);

		var (first, second) = Unified(left, right);

		return first.Type == second.Type && Underlying(first.Type).IsEnum
			? make(AsUnderlying(first), AsUnderlying(second))
			: make(first, second);
	}

	/// <summary>A `+` over text, which is <c>string.Concat</c>; null where neither side is a string.</summary>
	/// <remarks>
	/// Written as the API's <c>Add</c> node with the method it runs, so the tree still reads as
	/// the `+` the text wrote. Two strings are joined as strings, and anything else beside a
	/// string as an <c>object</c> — what C#'s own <c>string + object</c> operator takes.
	/// </remarks>
	static Expression? Joined(Expression left, Expression right)
	{
		var leftText  = left.Type == typeof(string);
		var rightText = right.Type == typeof(string);

		if (!leftText && !rightText)
			return null;

		if ((leftText || ReferenceEquals(left, Null)) && (rightText || ReferenceEquals(right, Null)))
			return Expression.Add(
				Implicitly(left, typeof(string))!, Implicitly(right, typeof(string))!, _concatStrings);

		return Expression.Add(Converted(left, typeof(object)), Converted(right, typeof(object)), _concatObjects);
	}

	/// <summary>Two operands of one type where one converts to the other's, and as they are otherwise.</summary>
	static (Expression, Expression) Unified(Expression left, Expression right) =>
		left.Type == right.Type                        ? (left, right)
		: Implicitly(right, left.Type) is { } asLeft  ? (left, asLeft)
		: Implicitly(left, right.Type) is { } asRight ? (asRight, right)
		: (left, right);

	/// <summary>An enum as its underlying number, nullable where it was.</summary>
	static Expression AsUnderlying(Expression value)
	{
		var number = Enum.GetUnderlyingType(Underlying(value.Type));

		return Expression.Convert(value, Nullable.GetUnderlyingType(value.Type) is null ? number : Lifted(number));
	}

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
	/// What is here and not shared includes the `using`s: a text's are its own, and the
	/// next text reads with none. What is not here is what the loaded assemblies say — the
	/// type a full name means, the namespaces there are — which is the same for every
	/// reading and is shared on purpose.
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
		public ParameterExpression Named(string name, SourceSpan at) =>
			Find(name, at) ?? throw new FormatException(NothingNamed(name));

		/// <summary>
		/// Whether that name means a variable where it is written — the same question
		/// <see cref="Named"/> answers, asked before anything is built of it.
		/// </summary>
		/// <remarks>
		/// A `Name` is read speculatively: `Math.Max(x, 1)` reads `Math` as one before
		/// finding out it is a type, and every compound assignment reads its target as one
		/// before finding out which operator follows. A construction that throws for a name
		/// the next alternative reads perfectly well is a construction that only works
		/// because it runs late — true of the tape and not of a carrier that builds where it
		/// reads (`CarrierKind.Immediate`), and not something a grammar should rest on
		/// either way. So the rule refuses instead, in a `when`, and what is refused is
		/// remembered here: a name nothing declares is worth saying so about, and a parse
		/// that ends there has no other way to say it.
		/// </remarks>
		public bool Knows(string name, SourceSpan at)
		{
			if (Find(name, at) is not null)
				return true;

			Refuse(at.Start, NothingNamed(name));

			return false;
		}

		/// <summary>Why a guard refused, kept where it is the furthest refusal yet.</summary>
		void Refuse(int at, string why)
		{
			if (at >= _refusedAt)
			{
				_refusedAt = at;
				_refusal   = why;
			}
		}

		int     _refusedAt = -1;
		string? _refusal;

		/// <summary>
		/// What to say about a parse that refused a name, or null where it refused none and
		/// the parser's own message is the one to give.
		/// </summary>
		/// <remarks>
		/// The furthest name refused, and not the one standing where the parse gave up: a
		/// refused name leaves the reading with nowhere to go, and where it stops after that
		/// is a fact about the rest of the text rather than about the mistake. `x + y` with
		/// no `y` gives up at the end of the input, four characters past the word that is
		/// the reason.
		/// </remarks>
		public string? Refused() => _refusal;

		/// <summary>Where what <see cref="Refused"/> speaks of was written.</summary>
		internal int RefusedAt => _refusedAt;

		// ── What a name written as a type means ─────────────────────────────────────

		/// <summary>The namespaces the text's `using`s named, in the order it named them.</summary>
		List<string>? _imports;

		/// <summary>A `using`, recorded while the text is read (§8.1).</summary>
		/// <returns>
		/// Whether that namespace is there. One that is not is refused, as C# refuses it
		/// (CS0246), and said so about. One named twice is recorded once, which is also what
		/// a reading that reads the directive again writes.
		/// </returns>
		public bool Imports(string @namespace, SourceSpan at)
		{
			if (@namespace is null)
				throw new ArgumentNullException(nameof(@namespace));

			if (!Loaded.Has(@namespace))
			{
				Refuse(at.Start, $"The type or namespace name '{@namespace}' could not be found.");

				return false;
			}

			if (!(_imports ??= []).Contains(@namespace, StringComparer.Ordinal))
				_imports.Add(@namespace);

			return true;
		}

		/// <summary>Whether this name is a type here, asked while the text is read (§8.1).</summary>
		/// <remarks>
		/// This is what tells `(Foo)x` from `(foo)`, which C# needs a rule of its own for: a
		/// parenthesized name is a cast where the name is a type and an expression where it is
		/// not, and the guard answering no is what sends the parse to the other reading. A
		/// name two `using`s both give is a type as well — an ambiguous one, which
		/// <see cref="TypeNamed"/> refuses where it is built.
		/// </remarks>
		public bool Resolves(string name) => Meanings(name, out _, out _) > 0;

		/// <summary>The type that name means.</summary>
		/// <exception cref="FormatException">It means none.</exception>
		/// <exception cref="InvalidOperationException">It means two, which C# refuses (CS0104).</exception>
		public Type TypeNamed(string name) => Only(name, name);

		/// <summary>The type that name and those arguments mean.</summary>
		/// <remarks>
		/// A generic type is named in metadata by its arity — <c>Func`2</c> — which is one more
		/// thing about the runtime rather than about the language, and so is here rather than
		/// in the grammar. The name looked up is the one the author wrote with the count of
		/// what they wrote it over.
		/// </remarks>
		public Type Generic(string name, Type[] arguments)
		{
			if (arguments is null)
				throw new ArgumentNullException(nameof(arguments));

			var open = Only(
				name + "`" + arguments.Length.ToString(CultureInfo.InvariantCulture),
				name + "<" + new string(',', arguments.Length - 1) + ">");

			return open.MakeGenericType(arguments);
		}

		/// <summary>The one type a metadata name means, or why there is not one.</summary>
		Type Only(string name, string shown) =>
			Meanings(name, out var first, out var second) switch
			{
				0 => throw new FormatException($"The type or namespace name '{shown}' could not be found."),
				1 => first!,
				_ => throw new InvalidOperationException(
					$"'{shown}' is an ambiguous reference between '{first}' and '{second}'."),
			};

		/// <summary>
		/// How many types a name means here — written whole, or inside a namespace a `using`
		/// imported — and the first two of them.
		/// </summary>
		int Meanings(string name, out Type? first, out Type? second)
		{
			Type? one   = null;
			Type? two   = null;
			var   count = 0;

			for (var at = -1; at < (_imports?.Count ?? 0); at++)
			{
				var found = Qualified(at < 0 ? null : _imports![at], name);

				if (found is null || found == one || found == two)
					continue;

				if (count++ == 0)
					one = found;
				else
					two ??= found;
			}

			first  = one;
			second = two;

			return count;
		}

		static string NothingNamed(string name) => $"nothing named '{name}' is declared here.";

		ParameterExpression? Find(string name, SourceSpan at)
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

			return found;
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
			Close(_loops ??= [], span);

			return Breaks(span);
		}

		/// <summary>A switch, which only a <c>break</c> may name.</summary>
		public bool Breaks(SourceSpan span)
		{
			Close(_breakables ??= [], span);

			return true;
		}

		/// <summary>
		/// A loop begun: everything from here on is inside it until it says where it ends.
		/// </summary>
		/// <remarks>
		/// A jump names the loop it is written in, and a loop knows how far it reaches only
		/// once its body has been read — so a reading that builds the jump where it stands
		/// would ask about a loop nothing has recorded yet. Written down twice instead:
		/// once where the loop begins, reaching to the end of the text, and once where it
		/// ends, with the extent it turned out to have. The label is keyed by where the
		/// loop begins, which is the same before and after, so a jump built under the open
		/// extent and one built under the closed one name the same label.
		///
		/// Nothing is lost where a reading defers instead. The open extent is replaced by
		/// the closed one, and until it is, the only positions inside it are the ones being
		/// read — which are the loop's own body.
		/// </remarks>
		public bool Opening(SourceSpan span)
		{
			(_loops ??= []).Add(new Scope(span.Start, int.MaxValue));

			return Breaking(span);
		}

		/// <summary>The same for a switch, which only a <c>break</c> may name.</summary>
		public bool Breaking(SourceSpan span)
		{
			(_breakables ??= []).Add(new Scope(span.Start, int.MaxValue));

			return true;
		}

		/// <summary>The extent an <see cref="Opening"/> left open, given the one it has.</summary>
		static void Close(List<Scope> among, SourceSpan span)
		{
			for (var i = among.Count - 1; i >= 0; i--)
				if (among[i].From == span.Start && among[i].To == int.MaxValue)
				{
					among[i] = new Scope(span.Start, span.Start + span.Length);

					return;
				}

			among.Add(new Scope(span.Start, span.Start + span.Length));
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

			return Expression.Return(_returns, Converted(value, _returns.Type));
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
