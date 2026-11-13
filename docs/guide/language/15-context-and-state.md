# Context and state

Two kinds of state a grammar may need beside its captures: what a parse **works out** as it goes — a table of
names, the labels a jump may go to — and what **holds while** something is being read — inside `checked(…)`,
inside a loop. The first is `context`, an object the caller hands over; the second is `state`, marks the
grammar places on what it reads. The specification is
[`syntax.md` §7.7–§7.8](../../syntax.md#77-what-a-parse-works-out-and-the-api-has-nowhere-to-keep).

## `context : @T`

A grammar may declare one context type. Every publication then takes an object of it after the input, and
`context` is a name a `when`, a `=>`, a `switch` selector and a recognizer may use.

```dotgram Deferred
context : @Names

Name    = { ['a'..'z']+ }
Decl    = "let " & n: Name & when @(context.Declare(n))
Use     = n: Name & when @(context.Knows(n))
Program = (Decl | Use) & (';' & (Decl | Use))*

parse Program
```

```csharp
using System.Collections.Generic;

partial class Grammar
{
	public sealed class Names
	{
		readonly HashSet<string> _declared = [];

		public bool Declare(string name)
		{
			_declared.Add(name);

			return true;
		}

		public bool Knows(string name)
		{
			return _declared.Contains(name);
		}
	}
}
```

```text examples
ParseProgram("let a;a", new) → "let a;a"
ParseProgram("let a;b", new) → refused
ParseProgram("a;let a", new) → refused
```

From C# the caller makes the object and hands it over: `Grammar.ParseProgram(text, new Grammar.Names())`.
The host attribute here says `Carrier = GramCarrier.Deferred`: with a context, the default carrier may rebuild
what was read each time a refused input is given back, and the compiler says so (`GRAM5016`) — the same
grammar on the default carrier:

```dotgram GRAM5016
context : @Names

Name    = { ['a'..'z']+ }
Decl    = "let " & n: Name & when @(context.Declare(n))
Use     = n: Name & when @(context.Knows(n))
Program = (Decl | Use) & (';' & (Decl | Use))*

parse Program
```

```csharp
using System.Collections.Generic;

partial class Grammar
{
	public sealed class Names
	{
		readonly HashSet<string> _declared = [];

		public bool Declare(string name)
		{
			_declared.Add(name);

			return true;
		}

		public bool Knows(string name)
		{
			return _declared.Contains(name);
		}
	}
}
```

Notes:

- A hook that does not name `context` is not passed it: a grammar that declares a context and never uses it
  compiles as one that declares none, and its publications take no extra argument.
- A `when` runs while the text is read, in the order written; a `=>` runs as its alternative is read (or after
  the parse, on the deferred carrier). The two may interleave, and the compiler warns where constructions share `context`
  with a guard (`GRAM5016`).
- **Nothing here is undone.** A `when` runs on readings the parse goes on to abandon, and what it wrote stays
  written: what belongs in a context is what can be written more than once without harm — a declaration keyed
  by where it was read. Where the type offers `Mark()` and `Rollback(mark)`, the parser uses them between the
  two readings of a refused input, and says so (`GRAM5013`, information).
- One context per grammar (`GRAM3014`). A grammar included in another may declare its own as a contract the
  caller's object must satisfy (`GRAM3019`).

```dotgram GRAM3014
context : @System.Text.StringBuilder
context : @System.Collections.ArrayList

Word = { ['a'..'z']+ } & when @(context != null)

parse Word
```

## `state : @T` and `with state @(…)`

A mark changes nothing about what is read: the same characters, the same route. It says what stands over a
construction, which reads the marks as `parserState`, outermost first.

```dotgram
state : @Mode

Name   : @string = n: { ['a'..'z']+ } => @(Describe(n, parserState))
Quoted : @string = '"' & v: Item with state @(Mode.Quoted) & '"' => v
Item   : @string = q: Quoted => q
                 | n: Name   => n

parse Item
```

```csharp
using System;

partial class Grammar
{
	public enum Mode
	{
		Plain,
		Quoted,
	}

	static string Describe(string name, ReadOnlySpan<Mode> state)
	{
		return state.Length == 0 ? name : name + " (" + state.Length + " deep)";
	}
}
```

```text examples
ParseItem("a") → "a"
ParseItem("\"a\"") → "a (1 deep)"
ParseItem("\"\"a\"\"") → "a (2 deep)"
```

Notes:

- `parserState` is a `ReadOnlySpan<T>` of the marks standing over the construction, outermost first, so the last
  is the nearest. `parserMarks` is a `ReadOnlySpan<int>` beside it (below).
- Both are handed to a `=>` that names them, and not to a `when`: what stands over a reading is not settled
  until it is accepted (`GRAM4053`). A grammar that places no mark hands an empty span.
- One `state` type for all marks (`GRAM3016`); two concerns are two values of it, not two declarations. A
  grammar meant to be included declares it as a reference type, so others can add their concerns.
- A mark's value is what the grammar wrote at the site, the same every time: it may not name a capture of its
  own alternative (`GRAM4030`).
- An abandoned reading's marks are abandoned with it; nothing unwinds them.
- `with state` and `with (…)` share a word and no mechanism: a rebinding replaces rules before anything runs,
  a mark leaves the grammar alone and is visible only to constructions.

**Where each mark was placed.** A mark's value says *what* stands over a construction, never *which*: two
groups side by side are both under `Group`. `parserMarks` tells them apart: for each mark standing over the
construction, outermost first, where in the input the reading under it began.

```dotgram
state : @int

Name  : @string = n: { ['a'..'z']+ } => @(n + "@" + string.Join(",", parserMarks.ToArray()))
Group : @string = '(' & v: Item with state @(0) & ')' => v
Item  : @string = g: Group => g
                | n: Name  => n
List  : @string[] = Item & (',' & Item)*

parse List
```

```text examples
ParseList("a") → ["a@"]
ParseList("(a),(b)") → ["a@1", "b@5"]
ParseList("((a))") → ["a@1,2"]
```

Each group is marked with the same value, `0`; the positions say that `a` and `b` stand under two different
groups, and that `a` in `((a))` stands under two, begun at 1 and 2. Only the beginning is given: a
construction may run before the reading under its mark ends.

Refused:

```dotgram GRAM4029
Name   = { ['a'..'z']+ }
Quoted = '"' & Name with state @(1) & '"'

parse Quoted
```

```dotgram GRAM4053
state : @int

Name = { ['a'..'z']+ } & when @(parserState.Length == 0)

parse Name
```

```dotgram GRAM3016
state : @int
state : @string

Name : @string = n: { ['a'..'z']+ } => @(n + parserState.Length)

parse Name
```

A mark in a grammar that declares no `state` has no type to be (`GRAM4029`); a guard is handed no marks
(`GRAM4053`); and there is one state type (`GRAM3016`).
