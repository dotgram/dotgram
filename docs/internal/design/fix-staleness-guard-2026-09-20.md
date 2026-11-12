# The staleness guard, read before it is designed (2026-09-20)

D71 named a check: a consumer built against one dictionary, a counterparty issuing a newer one, and
the disagreement firing at start-up instead of becoming wrong parses hours later. The instruction
that came with it was to read what the existing reader already does at a disagreement before
designing anything. This is that reading, and it changes what the guard should be.

## What the existing reader does: nothing, and deliberately

`FixValidator.Load(dictionary)` installs a rule for every message type the dictionary describes and
never consults the compiled tables. The two schemas coexist; per message type the last write wins.
There is no check, no warning and no diagnostic, and that is the shape D72 asked for — the table is
the consumer's, and the package does not second-guess a write.

So the guard is not a change to existing behaviour. It is new, and the only question is where it
lives.

## What a consumer can already ask, which is less than it looks

| | our schema | their dictionary |
| --- | --- | --- |
| a tag's type | `FixValues.Type(tag)`, as `FixValueType` | `Name`/`Type(tag)`, as the file spells it |
| a tag's code set | **unreachable** | `Codes(tag)` |
| a tag's name | **unreachable** | `Name(tag)` |
| which fields a message holds | **unreachable** | **unreachable** |
| which fields a component holds | **unreachable** | **unreachable** |

`FixSchema` is internal, so its `Codes`, `Message`, `Component` and `Group` are not a consumer's to
call however public they are inside it; and a dictionary's own composition is held in `SchemaRef[]`,
which is internal too. Even the one row that is reachable on both sides does not line up: ours
answers a `FixValueType` and theirs answers the file's spelling, and `FixVocabulary`, which maps one
to the other, is internal as well.

**So of the three kinds of disagreement this package has recorded — ten tags by type, seventy-seven
by code set, twenty-six by composition — a consumer can reproduce none of them from the public
surface.**

## Except the one that found the most

The strongest check this package has is not over tables at all. It is over **messages**: parse a
corpus, ask both validators, compare the findings. That is what found the twenty-six composition
rows, and it is what found three defects in the generator's own wiring, because the walk had to be
right for either side to say anything.

And it is entirely public. `FixMessages.Parse`, `message.Validate()`, `message.Validate(validator)`
and every field of `FixFinding` are a consumer's to call today. A start-up guard written that way
needs **no new API**: a handful of messages the consumer already has — their own captures, or the
last session's log — read twice and compared.

## What the guard should therefore be

**A recipe, documented, over the consumer's own messages.** It answers the question D71 actually
asked — "does this new file change what my validation says?" — rather than the question the tables
answer, which is "do these two descriptions differ", and which is not the same question: two
descriptions can differ in a tag the consumer never sends.

That reframing is worth more than the saving in API. A table comparison reports seventy-seven code
sets and leaves the reader to work out which of them matter. A message comparison reports only the
differences that showed up in traffic the consumer actually has, and reports them as findings, in
the words the consumer already reads.

**What it costs:** it sees only what the corpus exercises. A dictionary that changes a message type
the consumer has never received says nothing until the day they receive one. That is the real
trade, and it is the opposite of the tables' trade — complete and undirected against partial and
relevant. Both should be said, and the recipe should say which it is.

## What the other road would cost, since it should be priced rather than dismissed

Making the table comparison a consumer's to write means publishing: a tag's code set, a tag's name,
the composition of a message and of a component, and the vocabulary that maps a file's spelling to
a type. That is four or five new public members and, with composition, a public shape for
`SchemaRef` — a type describing a schema reference, which is a considerably larger commitment than
a method.

It is not obviously wrong. A venue integration team comparing two dictionaries before a go-live
wants exactly the complete, undirected answer. But it is a decision about how much of the schema
this package publishes, and it should be taken as one rather than arrived at by building a guard.

## What I would do

The recipe first, because it costs nothing and answers the question as asked. The table comparison
only if somebody wants it, and then as a decision about publishing the schema, not as a guard.

And one thing regardless of which: **our own three lists are a guard already**, pinned in the test
suite, and they fire when our tables and the published dictionary drift apart. That is the
maintainer's half of D71, and it exists. What is missing is the consumer's half.
