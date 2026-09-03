# Diagnostics

Every message .Gram reports carries an identifier, so it can be looked up, suppressed, or
argued with. This is the list.

**Numbers go by the stage that raises them.** `GRAM0xxx` is the Roslyn shell — the part that
finds a grammar and hands it to the compiler; `GRAM1xxx` the lexer; `GRAM2xxx` the parser;
`GRAM3xxx` the binder, which resolves names; `GRAM4xxx` the normalizer, which lowers the
grammar and checks what it means; `GRAM5xxx` the analyses that decide what a grammar *gets*
rather than whether it is one.

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

## GRAM2xxx — reading the shape

| Id | What it says | What to do |
| --- | --- | --- |
| `GRAM2001` | Expected a particular token. | The message names it. |
| `GRAM2002` | Expected a rule, a namespace or a publication directive. | Only those four things stand at the top of a grammar: a rule, `namespace`, `parse`/`find`, and the `context`/`state`/`trivia` declarations. |
| `GRAM2003` | Expected a literal, a reference, an element set, a group or an atomic group. | An operand is missing where one has to stand — often after a `&` or a `\|`. |
| `GRAM2004` | Expected a name. | |
| `GRAM2005` | A binding power is a whole number. | `<< 2`, `>> 3` (§4.3.1). |
| `GRAM2006` | A namespace header's rebindings need `with`. | `namespace Name with (A = B) { … }` (§5.1). |
| `GRAM2007` | A publication of anything but a rule's name needs `as`. | `parse` and `find` name the method after the rule; an expression has no name to make one from, so give it one: `parse X & Y as Both`. |
| `GRAM2008` | A type here belongs to an expression, not to a rule that already declares one. | The rule says its own type where it is written (§6). |

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
| `GRAM3011` | A rebinding is circular. | |
| `GRAM3012` | A rule shadows one from an enclosing namespace or an import. | If it is meant to *replace* it, say so with a rebinding: `namespace Name with (X = …) { … }` (§5.1). |
| `GRAM3014` | A grammar declares two `context` types. | One contract for the rules written here. A grammar including this one may strengthen it for its own (§7.7). |
| `GRAM3016` | A grammar declares two `state` types. | Two concerns are told apart by their values, read by the hook that cares — not by a second type (§7.8). |
| `GRAM3019` | The context handed to a parse does not satisfy a contract it inherited. | A grammar including another may strengthen the contract; it cannot replace it (§7.7). |
| `GRAM3020` | Two grammars in one composition declare a different `state`. | Every mark a parse places is written in one type (§7.8). |

## GRAM4xxx — what the grammar means

| Id | What it says | What to do |
| --- | --- | --- |
| `GRAM4001` | The body of a repetition can match without consuming input. | The repetition would not terminate. |
| `GRAM4002` | Left recursion is not built yet. | Write the loop with a quantifier instead (§4.3). |
| `GRAM4003` | `trivia` must accept empty input. | It is inserted between every pair of operands, and a required match would demand whitespace everywhere (§4.5). |
| `GRAM4005` | A name in an element set is not a rule. | |
| `GRAM4006` | A capture inside a lookahead. | A lookahead consumes nothing and answers only whether it matched, so there is nothing to capture. |
| `GRAM4007` | One name is captured twice with different types. | A member has one type; give the two captures the same one, or different names (§7.3). |
| `GRAM4008` | A `=>` is not on an alternative, or a rule that builds does not say what type. | A `=>` builds the rule's value, so it belongs at the end of an alternative, and the rule needs `: @T` to say what it builds. |
| `GRAM4009` | An alternative is recursive and states no strength while its siblings do. | A rule uses one convention or the other — levels as rules, or `<<` and `>>` on every recursive alternative (§4.3.1). |
| `GRAM4010` | A recovery's `=>` has no sequence to put the rejected element in. | The repetition collects text rather than values. Give the repeated rule a capture of its own, or drop the `=>` and report out of band (§8.2). |
| `GRAM4011` | A rule's declared type is neither a C# type nor a rule in view. | §4.1 case 3 takes the value of a rule named here; a C# type is written with `@`. |
| `GRAM4012` | A capture takes one of the supplied names. | Every name the parser supplies to a `=>` or a `when` begins with `parser`, which is what that prefix is for (§7.3). |
| `GRAM4013` | A value stands where a piece of grammar goes. | A value is allowed where a value is expected — a count, an argument of `@Method`, inside `@(…)`. Drop its type to make it a recognizer (§4.2). |
| `GRAM4014` | A rebinding's replacement produces an incompatible result. | (§5.1) |
| `GRAM4015` | An external recognizer has more than one value-returning overload with different `T`. | Bare `@Name` cannot say which is meant; give it one such overload, or none (§7.1). |
| `GRAM4016` | Two alternatives begin with the same operand, and that operand leads back to the rule. | **A warning, and a choice rather than a mistake.** Ordered choice reads the operand once for each alternative, so the reading doubles at every level of nesting. Written as one alternative with the rest of the longer one optional it is read once — but that is a different grammar wherever the operand can give back. See §4.5 on saying a lexeme is read once. |
| `GRAM4017` | Two rules each contain a `with` that reaches the other. | Neither can be specialized against the other already specialized. Give one of them the rebinding the other was going to apply, or write the substitution as a `namespace Name with (…)` block around both (§5.1). |

## GRAM5xxx — what a grammar gets

These do not say a grammar is wrong. They say what it will not be given, and why.

| Id | What it says | What to do |
| --- | --- | --- |
| `GRAM5001` | A publication gets no overload taking a reader. | The message says which of the reasons applies: something the rule matches can cross a line, or the rule keeps what it cannot keep from a window (§6.3). |
| `GRAM5002` | A repetition can begin with the same input as what follows it. | A warning: the reading is ambiguous where the two overlap, and the parser resolves it by order rather than by meaning. |
| `GRAM5003` | A generated method is left past the size at which the JIT stops optimizing it. | A warning: the parser is correct and several times slower than it needs to be. The message names the estimate, the budget the generator divides methods under, and what to split to restore optimization. |
| `GRAM5004` | `Lexical = true` was asked for and the grammar cannot be cut in two. | Information, not a warning: the parser is the one it would have been without the request, and nothing the author wrote is wrong. The message says which of the four it is — no trivia at all, so there is nothing to tell a token from a character; a terminal that is not a regular language; a `find`, which hunts through characters for a place to begin; or a `trivia` not written in braces, the seam between tokens being skipped by the scanner braces ask for. |
| `GRAM5005` | A split grammar's syntactic half cannot be read by methods, so it runs on the shared engine. | A warning: over kinds a rule's answer stands (docs/syntax.md §4), and it is the methods that say so — on the engine a choice that has matched can be revisited when something later fails. The parse is correct as ordered choice over characters; it is the committed reading the notation promises over kinds that is not what runs. The message names the rule and what about it the methods refused: a recovery, a stream, a `find`, a captured lookahead, a guard handed what a reader cannot hand it, or a rule called with arguments. |

## Retired numbers

Not reused, and listed so that a suppression written against one is recognizable as dead.

| Id | What it used to say |
| --- | --- |
| `GRAM3013` | Refused a `context` inside a namespace. A grammar included in another declares its own contract, which is `GRAM3019`'s subject now. |
| `GRAM3015` | Refused a `state` inside a namespace. Included grammars land in one, so the place was the wrong thing to refuse; the claim is checked by `GRAM3020`. |
| `GRAM3017` | One `context` per assembly. Replaced by `GRAM3019`, which asks about a composition rather than an assembly. |
| `GRAM3018` | One `state` per assembly. Replaced by `GRAM3020`, likewise. |
| `GRAM4004` | Retired before release. |
| `GRAM5006` | Said that the reader — the rendering by methods written the way a person writes them — was asked for and declined a grammar, which the older rendering by methods then wrote. The reader is the only rendering by methods now, and what it does not read the engine does, which `GRAM5005` reports. |

`GRAM0001` is the one number that has been used twice. It reported a check that no longer
exists, and now reports the generator itself failing.
