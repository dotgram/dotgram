# DotGram 0.3.0

The draft of the GitHub release text, kept here until the tag so that it is reviewed like any
other change. Everything below the rule is pasted as the release's body; lines marked
`TODO(freeze)` are filled in, or dropped, at the candidate freeze.

---

# DotGram 0.3.0

One tree for every SQL parser, a parser generator that reads more and refuses in linear time
where it did not, bindings and back-references in the notation, a trace build that says why an
input was refused, and a command-line tool for grammars too large to generate on every build.
Every package's full notes, with what to write instead for each breaking change, are on its
NuGet page.

## Highlights

- **Generator.** The default carrier now builds as it reads wherever it can, which makes accepted
  parses typically 30-60% faster; `Carrier = GramCarrier.Tape` keeps the old behaviour, and
  `GRAM5016` says where the tape is the safer choice. New in the notation: bindings and
  back-references (`$q: X`, `$q`, `X{#q}`) for constructs closed by the text that opened them,
  `eos` for input read as a sequence of segments, and `Std.Nested(open, close)`. A trace build
  (`Trace = true`) reports what a parser reads to a sink, and `GramWhy` says why the last call was
  refused. A command-line build keeps compiled parsers between builds. Ignore-case literals and
  `\p{…}` read Unicode 16.0 on every runtime.
- **Readers agree with strings.** A parse read from a `TextReader`, lines or segments reads what
  the same parse reads over a string, and a rule a reader cannot read that way gets no reader
  overload, with `GRAM5001` saying why. TODO(freeze): one sentence on the reader/engine agreement
  fix (outcome, position and value the same in every form).
- **DotGram.Cli**, a new .NET tool: `dotnet dotgram generate` writes the parsers a build would
  generate, so that a project with a very large grammar compiles them without referencing the
  generator.
- **DotGram.Sql** builds one tree, `DotGram.Sql.Ast`, from all three parsers: T-SQL, SQL:2023 and
  SQL-92. `SqlStandardParser` reads by level (six entries), lists are `SqlList<T>` and compare by
  content, and `SqlScript.Read` cuts a script into batches byte for byte as sqlcmd 18.6 does.
  TODO(freeze): the T-SQL speed and allocation against 0.2.0, from the release benchmark.
- **DotGram.Finance**: validation is a question that keeps nothing, every finding says its path
  through the repeating groups, and the message limit and log framing are the context's.
- **DotGram.Web** refuses a long adversarial Link, Forwarded, Content-Disposition or Accept header,
  an email address or a Content-Type in linear time; the Link reader holds parameter values to
  RFC 8288.
- **DotGram.ExpressionLanguage** reads more of C# — initializers as C# reads them, `with`,
  target-typed `new()`, `T?` — and parses accepted text 30-55% faster.
- **Visual Studio:** standalone grammars follow their host as the solution changes, and navigation
  reaches into included grammars.

## Breaking changes

Each package's notes list them with what to write instead. The ones most projects meet:

- **DotGram:** the default carrier (above); new warnings `GRAM4043`, `GRAM4051` and `GRAM4042`,
  which fail a build with warnings as errors until the grammar is changed; `GRAM0010` below C# 8;
  ignore-case literals no longer take a case partner across ASCII (`'s'i` and `ſ`).
- **DotGram.Sql:** the 0.2.0 T-SQL tree in `DotGram.Sql` is removed — read, walk and write
  `DotGram.Sql.Ast`; the 0.2.0 `DotGram.Sql.Ast` tree changes shape (a SELECT holds a `Query`,
  `SelectColumn`, one `Limit`, a DROP record per kind, reshaped SET options; docs/ast.md);
  `Sql2023Writer` is `SqlWriter`; `SqlStandardParser` publishes six entries
  where it published forty-two productions; the position and window forms of `TryParseScript`
  are removed in favour of `SqlScript.Read`.
- **DotGram.Finance:** `Validate` returns the findings and keeps none (`InvalidFindings` and
  `IsValid` are gone); `FixFinding` is a class with a `Path`; `FixMessage.Invalid` is
  `FixMessage.Unknown`; `maxMessageLength` is the context's `MaxMessageLength`.

## Packages

| Package | |
| --- | --- |
| [DotGram](https://www.nuget.org/packages/DotGram/0.3.0) | the generator (analyzer package, no runtime) |
| [DotGram.Cli](https://www.nuget.org/packages/DotGram.Cli/0.3.0) | the generator as a .NET tool, `dotnet dotgram` |
| [DotGram.Sql](https://www.nuget.org/packages/DotGram.Sql/0.3.0) | T-SQL, SQL:2023 and SQL-92, one tree |
| [DotGram.Web](https://www.nuget.org/packages/DotGram.Web/0.3.0) | JSON, HTTP headers, URIs, dates, … |
| [DotGram.ExpressionLanguage](https://www.nuget.org/packages/DotGram.ExpressionLanguage/0.3.0) | C#-style expressions to `System.Linq.Expressions` |
| [DotGram.Finance](https://www.nuget.org/packages/DotGram.Finance/0.3.0) | FIX 4.2, 4.4, 5.0 SP2 |
| [Visual Studio extension](https://marketplace.visualstudio.com/items?itemName=dotgram.dotgram) | VS 2022 17.14+ and VS 18 |

## Known issues

- Over tokens (`Lexical = true`), a reading from a position counts trivia before trailing empty
  operands differently: `'a' & 'b'*` on `"a "` has length 2 over characters and 1 over tokens
  ([#5](https://github.com/dotgram/dotgram/issues/5)).
- A published lexical rule that can be empty is refused the empty input over tokens, with no
  diagnostic: with `namespace Lex { trivia = { ' '* } Name = ['a'..'y']+ Opt = 'z'* }`,
  `Start = Lex.Name & Lex.Opt` and `parse Lex.Opt`, `TryParseOpt("")` accepts over characters and
  refuses over tokens ([#5](https://github.com/dotgram/dotgram/issues/5)).
- `Start = (Lex.Kw | Lex.Digits)*` refuses `"if 12"` and `"1 2"` over characters and accepts them
  over tokens ([#5](https://github.com/dotgram/dotgram/issues/5)).
- Below C# 11, each parser class adds its expected-set table to the assembly's user-string heap
  (about 600 KB for T-SQL); an assembly near the heap's 16 MB fails with CS8103
  ([#35](https://github.com/dotgram/dotgram/issues/35); README, Compatibility).
- A refusal's message can differ between the forms of one publication in three shapes (a root
  after leading trivia on the engine, a root a flat method compiles, a rule read whole inside
  braces by the reader): one names what was expected where another says the rule's `on fail`
  sentence. Outcome and position agree.
- A part in front of a rule that recovers gives back no shorter reading for the rule's bad first
  element: with `Rows = Row+ recover ';' & '.'`, `';'* & Rows` refuses `;.`, which `Rows` reads.
- Two shapes are refused a reader overload they could have: space trivia reached through the
  seam after an optional prefix, and alternatives that call fixed-width rules.
- Over characters, a repetition that gives a turn back reads the turns before it again from the
  start, so a refused text can cost the square of its length. Bound the size of untrusted input.
- A `PerCall` located class has no forms over a buffered input (`TextReader`, `Stream`, bytes); a
  grammar that needs them located names a second reading without `PerCall`.
- Over tokens, a window that holds no token (empty, or trivia alone) is refused, where over
  characters an empty repetition reads it.
- By default a parse read from a position or a window can refuse, or cut short, the rule's first
  reading in a few shapes (an optional or a repetition before a negative lookahead, a quoted string
  with doubled quotes; docs/syntax.md §6.8). The whole forms are right. The experimental build
  property `DotGramPositionalFollow` fixes them at a cost.
- On the tape (`Carrier = GramCarrier.Tape`), a `switch` selector or a guard that reads what a
  construction writes into the context sees it before that construction runs, since the tape runs
  constructions once the parse is accepted. DotGram.Finance's FIX grammar is pinned to the
  immediate carrier for this.

The attached files are the exact artifacts published to NuGet and the Marketplace;
`SHA256SUMS.txt` lists their hashes.
