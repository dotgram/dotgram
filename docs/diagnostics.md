# Diagnostics

Every message .Gram reports carries an identifier, so it can be looked up, suppressed, or
argued with. This is the list.

**Numbers go by the stage that raises them.** `GRAM0xxx` is the Roslyn shell — the part that
finds a grammar and hands it to the compiler; `GRAM1xxx` the lexer; `GRAM2xxx` the parser;
`GRAM3xxx` the binder, which resolves names; `GRAM4xxx` the normalizer, which lowers the
grammar and checks what it means; `GRAM5xxx` the analyses that decide what a grammar *gets*
rather than whether it is one. `GRAM6xxx` is not the generator's: the Visual Studio extension
reports it in the editor, and it has a section of its own before the retired numbers.

**This file is the list, and the analyzer's release files are not.** `AnalyzerReleases.Shipped.md`
holds only what the Roslyn shell declares as rules of its own, which is `GRAM0xxx`. Everything a
grammar is told is reported through a descriptor made from its identifier as the message arrives,
so Roslyn never sees those rules to track them — and a suppression or a severity is written
against the identifier either way.

**A retired number is not reused.** A suppression written against the old meaning would
silently acquire a new one. The retired numbers are listed at the end.

**Where a message points.** A grammar in a `.gram` file is underlined in that file. A grammar
written into the `[Gram("…")]` attribute is underlined inside the string literal, at the
character the grammar's own position maps to. A grammar inherited from a base class is
underlined where it was written, in the base's own file — see §5.1.

---

## GRAM0xxx — finding the grammar

| Id | What it says | What to do |
| --- | --- | --- |
| `GRAM0001` | The generator itself failed. | A defect in .Gram, not in the grammar. The message names the stage and carries the exception; please report it. |
| `GRAM0002` | A class hosting a grammar must be `partial`. | Add `partial` to the class the `[Gram]` attribute is on. |
| `GRAM0003` | No grammar file for a `[Gram]` class. | Add the `.gram` file, or add it to the project as `<AdditionalFiles Include="…" />`. |
| `GRAM0004` | More than one grammar file matches. | Two additional files answer to the same name. Name one of them in the attribute: `[Gram("Feed.gram")]`. |
| `GRAM0005` | The name a grammar is included under is not an identifier. | `[Gram(IncludedAs = "…")]` names the namespace an inheriting grammar reaches this one through, so it has to be one identifier. |
| `GRAM0006` | Two readings of one grammar want the same scope. | The `[Gram]` and every `[GramOptions]` on a class is a compilation of its own and needs a scope of its own: the `[Gram]` goes in the class itself, and every `[GramOptions]` names a nested class with `Suffix`. |
| `GRAM0007` | The name a grammar is compiled under is not an identifier. | `[GramOptions(Suffix = "…")]` names a nested class, and a class is named by an identifier. |
| `GRAM0008` | Two grammars are included under one name. | An included grammar is wrapped in a namespace named after it, which is what keeps two grammars' rules apart: `Sql92.Identifier` and `Lex.Identifier` are different names. Two under one name are one namespace, and their rules do collide — give all but one an `As` of its own. |
| `GRAM0009` | A buffer option is not positive. | `[Gram(MaxRetained = …)]` is the most a buffered parse may hold and `BufferSize` the capacity it starts with, so either is at least 1. Leave it out for no limit, or give a call its own `maxRetained`. |

## GRAM1xxx — reading the characters

| Id | What it says | What to do |
| --- | --- | --- |
| `GRAM1001` | Unterminated character literal. | Close the `'`. |
| `GRAM1002` | Unterminated string literal. | Close the `"`. |
| `GRAM1003` | Unterminated comment. | Close the `*/`. |
| `GRAM1004` | Unrecognized escape sequence. | The escapes are the C# ones: `\n`, `\r`, `\t`, `\\`, `\'`, `\"`, `\0`, `\uXXXX`. |
| `GRAM1005` | Unexpected character. | Nothing in the notation begins with it. |
| `GRAM1006` | Unterminated inline `@(…)` expression. | Close the bracket. The scanner reads C# to find where the expression ends, so an unbalanced one runs to the end of the file. |
| `GRAM1007` | An inline `@(…)` expression needs a C# scanner, and none was supplied. | Only reachable when the grammar half is driven directly, without a host compilation. Inside a build there is always one. |
| `GRAM1008` | Expected a Unicode category in the form `\p{Lu}`. | Braces and a category name. |
| `GRAM1009` | Not a Unicode category. | One of `Lu`, `Ll`, `Nd`, `Zs`, … or a group such as `L` or `N`. |
| `GRAM1010` | A character literal holds one character. | `''` holds none and `'ab'` holds two. Write `"ab"` for a string. |
| `GRAM1011` | `$` or `#` needs a name right after it. | The sigil and the name are one token: `$name: X` binds the text `X` matched, `$name` matches it again, and `X{#name}` counts by its length. Write them with no space, and with a name rather than a number. |

## GRAM2xxx — reading the shape

| Id | What it says | What to do |
| --- | --- | --- |
| `GRAM2001` | Expected a particular token. | The message names it. |
| `GRAM2002` | Expected a rule, a namespace or a publication directive. | Only those four things stand at the top of a grammar: a rule, `namespace`, `parse`/`find`, and the `context`/`state`/`trivia` declarations. |
| `GRAM2003` | Expected a literal, a reference, an element set, a group or an atomic group. | An operand is missing where one has to stand — often after a `&` or a `\|`. Said in its own words for `when:`, a capture called `when`: `when` begins a guard, so rename the capture. |
| `GRAM2004` | Expected a name. | |
| `GRAM2005` | A binding power is a whole number. | `<< 2`, `>> 3` (§4.3.1). |
| `GRAM2006` | A namespace header's rebindings need `with`. | `namespace Name with (A = B) { … }` (§5.1). |
| `GRAM2007` | A publication of anything but a rule's name needs `as`. | `parse` and `find` name the method after the rule; an expression has no name to make one from, so give it one: `parse X & Y as Both`. |
| `GRAM2008` | A named yield publication places a result type before `yield`. | Write the element contract after `yield : @T` (§6). |
| `GRAM2009` | A condition after `when` whose operand is not followed by `is`. | A `when` is either a C# guard, written `when @(…)` and asked while the parser runs, or a condition about the grammar, written `when A is B` and answered while it is built. Both sides of that question are written out, so an operand with no `is` after it is neither one thing nor the other. |
| `GRAM2010` | A length `#q` stands only inside count braces. | `#q` is the length of the text bound to `$q`, and a length is a count: `X{#q}`, `X{#q,}`, `X{2,#q}`. To match the bound text itself, write `$q`. |

## GRAM3xxx — resolving the names

| Id | What it says | What to do |
| --- | --- | --- |
| `GRAM3001` | A rule of this name is already defined in this namespace. | Rename one, or put one in a nested namespace to shadow the other. |
| `GRAM3002` | No rule of that name. | |
| `GRAM3003` | No namespace of that name is in view. | |
| `GRAM3004` | No C# type of that name is in view. | The type is looked up the way C# would look it up beside the `using` directives the grammar declares. |
| `GRAM3005` | Two publications would generate the same method. | Give one another name with `as`. |
| `GRAM3006` | A rebinding's left side names no visible rule. | (§5.1) |
| `GRAM3007` | A rebinding's right side names no visible rule. | (§5.1) |
| `GRAM3008` | A rule is bound more than once in one `with`. | |
| `GRAM3009` | A rebinding's two sides take different numbers of parameters. | A rebinding substitutes one recognizer for another, so they have to take the same arguments (§4.2). |
| `GRAM3010` | A rule bound by the active namespace's own header cannot be redeclared. | Use a nested namespace header to replace it. |
| `GRAM3011` | A rebinding is circular. | In one header (`with (A = B, B = A)`), or once substitutions stack: `(X with (D = Y)) with (Y = D)` loops the same way (§5.1). |
| `GRAM3012` | A rule shadows one from an enclosing namespace or an import. | If it is meant to *replace* it, say so with a rebinding: `namespace Name with (X = …) { … }` (§5.1). |
| `GRAM3014` | A grammar declares two `context` types. | One contract for the rules written here. A grammar including this one may strengthen it for its own (§7.7). |
| `GRAM3016` | A grammar declares two `state` types. | Two concerns are told apart by their values, read by the hook that cares — not by a second type (§7.8). |
| `GRAM3019` | The context handed to a parse does not satisfy a contract it inherited. | A grammar including another may strengthen the contract; it cannot replace it (§7.7). |
| `GRAM3020` | Two grammars in one composition declare a different `state`. | Every mark a parse places is written in one type (§7.8). |
| `GRAM3021` | `$q` or `#q` where the rule binds nothing called `q`. | A binding is read in the rule that bound it (§3.5.1). Binding names are their own: a capture `q:` is not a binding — write `q: ($q: X)` to have both — and `$q` never names a rule. A rule that needs another rule's text receives it as a capture and compares with a guard. |

## GRAM4xxx — what the grammar means

| Id | What it says | What to do |
| --- | --- | --- |
| `GRAM4001` | The body of a repetition can match without consuming input. | The repetition would not terminate. |
| `GRAM4002` | Left recursion the rewrite cannot take. | Direct left recursion is built, and so is an indirect one through rules that only forward. What is refused is an intermediary that does something of its own, a rule with no alternative to start from, and an alternative recursive on both sides (§4.3). |
| `GRAM4003` | `trivia` must accept empty input. | It is inserted between every pair of operands, and a required match would demand whitespace everywhere (§4.5). |
| `GRAM4005` | A name in an element set is not a rule. | |
| `GRAM4006` | A capture inside a lookahead. | What a lookahead records is dropped whether it matched or not. Capture the lookahead itself: `?=X` produces X's value (§3.4). |
| `GRAM4007` | One name is captured twice with different types. | A member has one type; give the two captures the same one, or different names (§7.3). A fold step (§4.3) that collects a name another alternative holds once is the same case: one side is a sequence, the other a scalar. |
| `GRAM4008` | A `=>` is not on an alternative, or a rule that builds does not say what type. | A `=>` builds the rule's value, so it belongs at the end of an alternative, and the rule needs `: @T` to say what it builds. |
| `GRAM4009` | An alternative is recursive and states no strength while its siblings do. | A rule uses one convention or the other — levels as rules, or `<<` and `>>` on every recursive alternative (§4.3.1). |
| `GRAM4010` | A recovery's `=>` has no sequence to put the rejected element in. | The repetition collects text rather than values. Give the repeated rule a capture of its own, or drop the `=>` and report out of band (§8.2). A rule whose every alternative forwards another rule's value — `Line : @T = v: Item => @(v) \| v: Remark => @(v)` — builds a value, and is read as one here. |
| `GRAM4011` | A rule's declared type is neither a C# type nor a rule in view. | §4.1 case 3 takes the value of a rule named here; a C# type is written with `@`. |
| `GRAM4012` | A capture takes one of the supplied names. | Every name the parser supplies to a `=>` or a `when` begins with `parser`, which is what that prefix is for (§7.3). |
| `GRAM4013` | A value stands where a piece of grammar goes. | A value is allowed where a value is expected — a count, an argument of `@Method`, inside `@(…)`. Drop its type to make it a recognizer (§4.2). |
| `GRAM4014` | A rebinding's replacement produces an incompatible result. | A rule that reads the replaced one into a type of its own — a declared C# type, a sequence, a constructor — expects the replaced rule's type; so does one that reads a copy of a rule following it (`R : B`) under a `with` or a publication's `with`, whatever its action then does with the value. The message names that reader, the rule it reads and both types. Declare the reader as `: Rule` of the rule it reads, so that its type follows the replacement (§4.1 case 3), or replace with a rule whose result converts to the expected type (§5.1). |
| `GRAM4015` | An external recognizer has more than one value-returning overload with different `T`. | Bare `@Name` cannot say which is meant; give it one such overload, or none (§7.1). |
| `GRAM4016` | Two alternatives begin with the same operand, and that operand leads back to the rule. | **A warning, and a choice rather than a mistake.** Ordered choice reads the operand once for each alternative, so the reading doubles at every level of nesting. Written as one alternative with the rest of the longer one optional it is read once — but that is a different grammar wherever the operand can give back. See §4.5 on saying a lexeme is read once. |
| `GRAM4017` | Two substitutions each need the other applied first: two rules each contain a `with` that reaches the other, a namespace's header reaches a `with` that calls back into the namespace from outside it, or two namespaces' headers each reach, from outside, a rule the other rebinds. | Neither can be specialized against the other already specialized. Give one of them the rebinding the other was going to apply, or write the substitution as a `namespace Name with (…)` block around both; for a header, write the rebinding inside the namespace or on a publication (§5.1). |
| `GRAM4018` | A rule nothing reaches. | A warning: nothing published names it, no rule the grammar reads calls it, no element set drew its items from it, and no rebinding puts it in place of one — so nothing it says is ever read. Publish it, call it, or delete it. Said only about a grammar that is otherwise sound, because a declaration whose syntax did not come out can leave any rule below it unreached; and never about `trivia`, `wordboundary` or the rest of the built-in rules, which are configuration rather than rules a grammar calls; nor about the standard library (`Std`), which every grammar has whole and few call much of. A grammar that publishes nothing is not asked at all. |
| `GRAM4019` | `word` in a grammar that never said what continues a word. | `word` is a run of whatever `wordboundary` matches (§4.6), so without one it is a run of nothing: the rule matches everywhere and consumes nothing. Declare the boundary, or say the shape that was actually meant. |
| `GRAM4020` | A `when … is …` somewhere other than beside the operands of an alternative. | The condition is answered while the parser is built, and what it decides is whether an alternative is in this parser at all — so an alternative is what it has to be able to delete. Inside a group, a repetition or a capture there is nothing for it to remove. Move it out to the operands of the alternative it belongs to. |
| `GRAM4021` | A `when … is …` whose sides are not two listable sets of strings. | `is` asks whether two recognizers have a string in common. That is decidable, but this compiler answers it only where each side is a literal, a choice of them, or a rule that is one — which is what a domain and a range are. An error, and the build fails with it: the alternative is kept as though the condition held rather than guessed at, since deciding it either way silently changes what the parser reads. Write each side as one of those. |
| `GRAM4022` | Every alternative of a rule is ruled out by a condition in this parser. | Information, not a warning: a construct removed from a dialect has to be removable, and this is what removing one looks like — a grammar written per version does it in nearly every parser it publishes. The rule becomes one that cannot match, so a caller of it fails where the construct is written and nowhere else. Worth a second look only where no removal was meant. |
| `GRAM4023` | A grammar published in more than 64 readings. | A `with` that substitutes a rule only conditions ask about makes a reading of the grammar, and the parsers of one rule share one machine told apart by the reading's number — one bit each in a word. Sixty-four is room for every version and product of a real language several times over; publish fewer, or split the grammar. |
| `GRAM4024` | A `when` that leaves different C# behind in different readings. | The parsers of one rule share a machine, and a condition decides per reading whether its alternative is there. Where the C# beside it survives in two readings as two different pieces of C#, the C# would have to be told which reading it runs in, and it is not. Write the condition so that the C# that survives is the same wherever it does. |
| `GRAM4025` | A C# method a grammar names cannot be called the way its position calls it. | A bare `@Name` is an external recognizer, `static bool M(ReadOnlySpan<char> input, ref int pos)` or the same with `out T value` after the position; an `[@Name]` in an element set is a predicate, `static bool M(char c)` — any method a character can be passed to (§7.1). It is looked for where the generated call will look: the class the grammar is attached to, the classes around it, what they derive from, and the classes of the grammars it includes. The message says which it is — no method of that name, or only methods of another shape or out of reach. Said only where that is certain: a method of the right shape anywhere in the compilation's source is taken to be the one meant, and a grammar compiled without a host is not asked. |
| `GRAM4026` | An explicitly requested buffered input form is unsupported. | The diagnostic names the unsupported construct or input domain. Existing text and legacy reader forms are not replacements for this explicit request. See syntax §6.3 for the current supported subset. |
| `GRAM4027` | A `yield` publication cannot safely produce the requested element sequence. | Publish a complete non-nullable `Rule*` or `Rule+` sequence, optionally through transparent wrappers. Collection factories, outer choices, suffixes, recovery and implicit collection trivia are not supported yet. Use a compatible C# element type after `yield : @T`. |
| `GRAM4028` | A publication result does not fit its explicit C# contract. | Choose a compatible base type, interface or array type. The contract changes the public API, not construction. Byte extent publications must also accept `byte[]`. |
| `GRAM4029` | A mark is placed, or `parserState`/`parserMarks` asked for, in a grammar that declares no `state`. | A mark is a value of the type `state : @T` declares, so without one there is nothing to write it in. Declare the type the marks are written in (§7.8). |
| `GRAM4030` | A mark's value names a capture of its own alternative. | A mark is what the grammar wrote at the site, the same every time the site is reached; a capture is a value of one reading. Place one mark per value, in an alternative each, and let a construction that must tell readings apart name `parserMarks` (§7.8). |
| `GRAM4031` | A guard in a fold step names another step's name for the value so far, which this step has none of. | Several steps of one left-recursive rule (§4.3) may capture the leading, self-recursive call under different names; a guard is handed the value so far only under the name its own step gave it. Naming a sibling step's name instead is not bound in this step — give every step that shares a head the same name, or capture the leading call of this step under the name the guard reads. |
| `GRAM4032` | A reference or a length may be read where nothing bound its text. | Every route to `$q` or `#q` binds `q` before it, in the same alternative (§3.5.1). An option `($q: X)?` binds nothing on the route that skips it — `$q: (X?)` binds `""` there; a binding in a repetition whose turns hold more than the binding, `($q: X & ',')*`, is read only within its turn — bind the whole run; and a binding in a sibling alternative, or to the right of the reference, does not count. |
| `GRAM4033` | A binding or a reference where none can stand. | The message says which: a binding inside a lookahead, which gives back what it read (a reference inside one, `?!$q`, reads an outer binding and is fine); a reference inside its own binding; one name bound twice on one route; a binding and its reference on two sides of a constructing group `( … => … )`, which is built as a rule of its own; a binding in a step of a left-recursive rule, or around its leading call to itself; or `$q` where a value goes — in C# after `=>` or `when`, in `when … is …`, or as an argument, which is read inside another rule (§3.5.1). |
| `GRAM4034` | A binding, reference or length where it is not supported yet. | Three places not yet supported: a repetition that recovers (`recover`, §8.2), what such a repetition skips to, or a binding around one, written inside it or in a rule it calls; a rule the lexer reads once the grammar is cut in two (`Lexical = true`); and a reader or stream form (`TextReader`, `Stream`) of a publication that reaches a binding, in a rule it reads or in the trivia read around it. The string forms read the same grammar; the automatic reader form beside a `find` is left out rather than refused. |
| `GRAM4035` | A binding, reference or length in a rule read over token kinds. | Once a grammar is cut into a lexer and a parser (`Lexical = true`), a rule with trivia reads tokens, and a binding there would bind kinds, not text. Compare two captures with a guard instead, `close: Name & when @(close == open)`, or read the grammar over characters (§3.5.1). |
| `GRAM4036` | A `with` expression inside an instance of a parameterized rule made only while another rebinding was applied. | Not supported yet: the instance is made after every `with` is put in order, and its `with` was dropped. Write the rebinding where the instance is called, or on the rule's own declaration (§5.1). |
| `GRAM4038` | A `recover` inside a lookahead. | A lookahead is a reading of its own that answers yes or no, and the rule it is written in plans no recovery for it, so the repetition would be read as a plain one. Name the recovering repetition as a rule and call that rule from the lookahead — a rule plans its own recoveries — or take the `recover` off (§8.2). Also said where a parameter used inside a lookahead is passed a recovering repetition. |

## GRAM5xxx — what a grammar gets

These do not say a grammar is wrong. They say what it will not be given, and why.

| Id | What it says | What to do |
| --- | --- | --- |
| `GRAM5001` | A publication gets no overload taking a reader. | The message says which of the reasons applies: something the rule matches can cross a line, or the rule keeps what it cannot keep from a window (§6.3). |
| `GRAM5002` | A repetition can begin with the same input as what follows it. | Information: the reading is ambiguous where the two overlap, and the parser resolves it by order rather than by meaning. |
| `GRAM5003` | A generated method is left past the size at which the JIT stops optimizing it. | A warning: the parser is correct and several times slower than it needs to be. The message names the estimate, the budget the generator divides methods under, and what to split to restore optimization. |
| `GRAM5004` | `Lexical = true` was asked for and the grammar cannot be cut in two. | A warning: the parser that comes out is the one the grammar would have been without the request, which reads the same grammar over characters — and the two readings are not always the same parser, since a lookahead after a seam sees the next token's first character over characters and nothing over tokens. Asked for and not given is worth a warning. The message says which of the six it is — no trivia at all, so there is nothing to tell a token from a character; a terminal that is not a regular language; a `find`, which hunts through characters for a place to begin; a `trivia` not written in braces, the seam between tokens being skipped by the scanner braces ask for; a published rule that is no token of its own over tokens — what the seam is made of, or a helper a terminal calls; or a published terminal that builds a value, which over tokens only the rules that call it read. |
| `GRAM5005` | A split grammar's syntactic half cannot be read by methods, so it runs on the shared engine. | A warning: the grammar was cut in two to be read by methods, and part of it is not. The engine reads it as the methods would — over kinds it compiles each call, and each choice and repetition of a rule not marked `?`, as if braced, so a rule's answer stands (docs/syntax.md §4) and a choice or a repetition that has matched is revisited only inside a rule marked `?` — but it is the slower rendering, and a grammar written to be read by methods is told which rule took it off them. The message names the rule and what about it the methods refused: a recovery, a stream, a `find`, a captured lookahead, a guard handed what a reader cannot hand it, or a rule called with arguments. |
| `GRAM5007` | The immediate carrier was asked for by name and is not what carries some of the grammar. | Only where `Carrier = GramCarrier.Immediate` is written: a grammar left to the generator falls back to the tape in silence, and the carriers report says which of its machines did. The machines named are compiled on the tape, and each is the one it would have been without the request; the rest of the grammar keeps the carrier. The message names the publications and the input forms that moved, and every reason and every rule that stands in the way: the carrier refuses a machine that recovers where a construction is not settled at its rule's end, one with a rule that is an extent collected across the turns of a repetition, one where two members gathered across turns onto the stack of one type are taken by one construction (members of alternatives with a construction each never meet, so those are carried), and one where a sequence is gathered from values of several types. The other case is a grammar nothing of which is read by methods at all, since a carrier is what a reader holds: then the whole grammar is on the tape, and the message says why it is not read by methods. A `find` beside a `parse` is not one of these: it is a machine on the engine next to one the carrier is carrying, which is `GRAM5005`. A warning: the parser is correct, but a carrier asked for by name is a carrier somebody means to measure or to rely on, and one that quietly is not there makes every comparison against it compare the tape with itself. |
| `GRAM5009` | An optional or a repetition can take what follows it, where nothing can give it back. | Information. Over characters this is settled by backtracking and the parse succeeds by the second reading, which is `GRAM5002`; over kinds a rule's answer stands (§4), so the optional takes what follows, what follows is not there any more, and the rule fails one token past what it ate. Asked only of a grammar cut in two, and only of what the optional can match in **one token** — an optional that begins like the next clause and cannot match the whole of it fails and gives nothing back. The fix is a lookahead in front of it naming what it may not take, as `?!SourceClause & Identifier` does. Where the sequence has nothing left to read, what follows is what follows the *rule*. |
| `GRAM5011` | A terminal of a split grammar is nothing but `@M`. | A warning: the lexer has no beginning to stop on, so it asks the host at the start of every token, before its own automaton. Write what the terminal begins with in front of it — `Blob = '<' & @ReadBlob` — and the automaton reads that beginning with every other token, and the host is asked only where it stands, from where it ends. The rest may be a rule instead of a method, where the grammar can say it: `Comment = '/*' & Nested`. |
| `GRAM5012` | A grammar left to the generator is carried as `Immediate`, and the tape would hold some of its constructions back. | Information, said only where `Carrier` is not written. The generator carries every grammar it can with the immediate carrier: every construction runs where its alternative is read, and the walk the tape runs at the end is not there. Where a rule it builds is read for a derivation that may be given up, or can be read again after it has answered, its constructions can run for text the parser then reads another way, or reads a second time — in a grammar with no context and no recovery a constant number of times, at most two attempts for each enclosing loop that gives back: the reading made again for the message, and every attempt after a turn has been given back a second time until one stands, build nothing but what a guard names. With a context, the reading for the message builds again; where a way back is taken in a loop that cannot read unbuilt — a context, a recovery, a rule whose own guard asks for its value so far — the warning is `GRAM5016` instead. Where a construction writes `context` and nothing that runs during recognition reads it, the message says so first. A construction that only builds does not mind; one that can throw, or has side effects, behaves differently there than on the tape: an exception it throws escapes the publication (§7.5). The message names the rules. `Carrier = GramCarrier.Tape` holds every construction back until a parse has accepted (§3.7); `Carrier = GramCarrier.Immediate` says the author has checked, and silences this and every reason found later. Nothing is said where nothing is held back, where the immediate carrier refuses the grammar, or where `GRAM5016` is said. |
| `GRAM5014` | A buffered form is read by the engine where its string form is read by methods. | Information. A publication read by methods over a string is read by methods over a `TextReader` or a `Stream` too, except where a rule it reaches can reach itself: a reading that deep goes on in another thread with the whole input, and a buffer cannot hand that over. The parse is correct either way; this says why the two forms are two renderings. |
| `GRAM5013` | Whether a refused input is read again over a context put back as it was. | Information. A parse reads first without recording what it would say if refused, and reads a refused input again to say it (§7.7). Over a context the reading writes into, the second reading has to begin where the first began: where the context's type has an instance `Mark()` and a `Rollback` taking what `Mark()` returns, the parser calls them and says so; where it has not, every input is read once, recording, and the message says the pair is what would change that. |
| `GRAM5016` | A grammar left to the generator is carried as `Immediate`, and the tape would be safer. | A warning, said only where `Carrier` is not written, for the two reasons that can change more than whether a construction runs twice. One: constructions that name `context` share it with a hook that runs during recognition — a `when`, a `switch` selector, or an external recognizer handed the context — and a rule they are in is read for a derivation that may not stand or can be read again. Carried immediately a construction runs as its alternative is read, interleaved with those hooks, rather than after the parse has accepted, so a guard or a recognizer can see what a construction wrote, and a construction that reads `context` sees it as it stands at that point: what the grammar accepts and the values it builds can both differ from the tape's (§7.7). Two: a refused input can allocate memory quadratic in its length, because what a rule that builds has read is built again each time a way back is taken — a list once a turn given back: the reader reads unbuilt after a loop's second give-back, and does not where the grammar has a context or a recovery, or where a rule's own guard asks for its value so far. Said of every loop that asks for another reading of a rule that builds, in such a grammar: whether the input can take a way back once a turn is not something the generator can see. For untrusted input keep the tape. The message names the rules. `Carrier = GramCarrier.Tape` holds every construction back until a parse has accepted; `Carrier = GramCarrier.Immediate` says the author has checked, and silences this and every reason found later. |

## GRAM6xxx — the Visual Studio extension

The generator does not report these, and a build never shows them. The Visual Studio extension
reports them in the editor: `GRAM600x` about the classifications a `[GramClassify]` attribute
declares for a grammar's language, `GRAM61xx` about a string written in that language at an
embedded DSL site.

| Id | What it says | What to do |
| --- | --- | --- |
| `GRAM6001` | A classification target is neither `Rule` nor `Rule.capture`. | A warning, underlined inside the attribute's target. Name a rule, or a rule and one of its captures, separated by one dot. |
| `GRAM6002` | The grammar rule a classification target names was not found. | A warning. Name a rule the grammar declares. |
| `GRAM6003` | The grammar rule a classification target names is ambiguous. | A warning: more than one rule answers to the name. Name the one meant. |
| `GRAM6004` | The capture a classification target names was not found in its rule. | A warning. Name a capture the rule declares. |
| `GRAM6005` | A classification target is specified more than once. | A warning. Keep one `[GramClassify]` per target. |
| `GRAM6101` | The text of an embedded DSL site does not match its language. | An error, underlined at the position where recognition failed. The message names what was expected there. |

## Retired numbers

Not reused, and listed so that a suppression written against one is recognizable as dead.

| Id | What it used to say |
| --- | --- |
| `GRAM3013` | Refused a `context` inside a namespace. See `GRAM3019`. |
| `GRAM3015` | Refused a `state` inside a namespace. See `GRAM3020`. |
| `GRAM3017` | One `context` per assembly. See `GRAM3019`. |
| `GRAM3018` | One `state` per assembly. See `GRAM3020`. |
| `GRAM4004` | Retired before release. |
| `GRAM5006` | Said that the reader was asked for and declined a grammar. See `GRAM5005`. |
| `GRAM5008` | Said, as information, that the tape was holding constructions back for a promise the grammar might not need, and offered `Carrier = GramCarrier.Immediate`. See `GRAM5012`. |
| `GRAM5010` | Said, as information, that a publication compiled as a plain method gets no overload taking a position or a window. |
| `GRAM5015` | Warned that the immediate carrier was asked for by name over a grammar the generator would have kept on the tape. The generator keeps no grammar on the tape by choice now, and an explicit carrier is the author's decision and is told nothing; a grammar left to choose is told the reasons as `GRAM5016` and `GRAM5012`. |
