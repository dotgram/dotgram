# What a strict message parse allocates (2026-09-20)

Window 88 measured `FixMessages.TryParse` in the strict mode at 4,192 B for a seventeen-field
`NewOrderSingle`, where reading the fields alone is 1,216 B. The architect asked for the account
before any change: what those bytes are, what must be there because of what a message *is*, and
what need not. No code is changed here.

The frame he set, and it is the right one: the comparison is not with the field reader. A message
holds its fields as header, body and trailer, cuts group entries out into their own sets, and
keeps each field's position in the source. Some multiple of the reading is therefore normal, and
"three times the parser" is two different jobs held against each other, not a verdict.

## 1. The measurement

`dotnet-trace --profile gc-verbose` over 200,000 strict parses of the row's own input, read with
`.work/allocs`. By type:

| | share |
| --- | --: |
| `FixNode[]` | **63.3 %** |
| `System.String` | 7.9 % |
| `FixField[]` | 3.3 % |
| `FixSemantics.Reader` | 1.9 % |
| the typed fields themselves (`TransactTime`, `SendingTime`, `Price`, …) | ~13 % together |

And by the first frame of ours on the stack, which is where the same type is allocated in three
different places:

| | share | what it is |
| --- | --: | --- |
| `FixSemantics.TryBuild` | 34.2 % | `Scope` inlined into it — the header, body and trailer arrays |
| `FixFieldFactory.Part0` | 23.3 % | the typed fields and their strings: the reader's own cost, not the layer's |
| `FixMessages.Nodes` | 16.7 % | one flat `FixNode[]` of every field |
| `FixSemantics.Reader.Scope` | 13.1 % | the same allocation not inlined — group entries |

## 2. What the bytes are

Two mechanisms, three sites.

**`FixMessages.Nodes` builds one flat array of every node.** `FixNode` is a readonly struct of two
references and five ints — 40 bytes — so seventeen of them plus the header is 704 B, and the
measured 670 B a call for this site is exactly that one array.

**`FixSemantics.Reader.Scope` builds one array a scope.** It reads fields off a
`List<FixNode> stack` that the `Reader` owns, copies the run belonging to this scope into a new
`FixNode[]`, and removes it from the list. It is called for the header, for the body, for the
trailer, and once more for every entry of every repeating group.

So **each node exists at least twice**: once in the flat array, once in the array of the scope it
belongs to. And the list it is copied out of is a third place, new with every `Reader` and so with
every message, growing from empty by doubling.

## 3. The list's growth is visible, not inferred

The doubling is the kind of claim that is easy to assert from a type name, so it was measured. The
same message with the body's field count varied, lenient (a repeated tag is how the size is
varied, and the strict mode refuses a duplicate):

| body fields | fields in all | bytes | a field |
| --: | --: | --: | --: |
| 4 | 12 | 2,832 | 236 |
| 8 | 16 | 3,440 | **215** |
| 12 | 20 | 4,712 | 236 |
| 16 | 24 | 5,320 | **222** |
| 17 | 25 | 6,776 | 271 |
| 20 | 28 | 7,232 | 258 |
| 24 | 32 | 7,840 | 245 |
| 32 | 40 | 9,056 | **226** |
| 40 | 48 | 12,856 | 268 |
| 48 | 56 | 14,072 | 251 |
| 64 | 72 | 16,504 | **229** |
| 96 | 104 | 26,512 | 255 |

**Two columns, because one of them misled its first reader.** The message is `8`, `9`, `35`, `49`,
`56`, `34`, `52`, then the repeated field, then `10` — seven header fields and a trailer over the
body. What doubles is not the message but the `List<FixNode>` inside the `Reader`, and since
`Scope` copies each area out and calls `RemoveRange`, the count falls back to zero while the
capacity stays: **the high-water mark is the largest single area, which here is the body.** So the
steps fall where the BODY crosses a power of two, not where the message does. Ordering a
measurement of this by total field count would put every size in the middle between two steps and
see nothing.

The cost a field is lowest at 8, 16, 32 and 64 body fields — the sizes where a doubling list has
just filled exactly — and worst immediately after one, at 17. Between 16 and 17 body fields the
message costs 1,456 B more: a thirty-two-slot `FixNode[]` is 1,304 B with its header, the extra
field itself about 72, and its node 40 — 1,416 against the measured 1,456, which is as close as
this arithmetic gets. The steps are real and they are the list.

The floor is ~236 B a field, of which ~72 is the field reader's (measured separately) and ~80 is
the two copies of the node. The rest is strings and the message's own objects.

## 4. What must be there, and what need not

**Must.** The per-scope arrays are what a message *is*: `FixMessages` hands out header, body and
trailer as separate field sets, and a group's entries as nested ones, and a caller walks them. One
array a scope is the shape of the answer, not scaffolding for producing it.

**Need not, in principle.** The flat array from `Nodes` is not part of the message. It exists
because the parser returns `FixField[]` — fields, not nodes — and the message layer needs each
field's position and its length half if it had one. It is an intermediate, and every node in it is
copied into a scope array and then dropped.

**Need not, more clearly.** The `List<FixNode>` is scaffolding by construction: it is a staging
area that `Scope` copies out of and empties. It is new for every message, so every message pays
its growth from zero, which is what §3 measures.

**And the reason it is not simply removable**, which any change here has to answer first: a
scope's nodes are *not* a contiguous range of the flat array once a group is present, because a
group's entries are cut out of the run and hung off their counter. The list exists precisely to
let `Scope` take a run whose length is not known until it stops. A message with no group could be
sliced; a message with one cannot, and FIX messages have groups.

## 5. What this does not say

- It does not say 4,192 B is too much. It says what the 4,192 are, and that about 2,500 of them
  are `FixNode[]` in three places where the message needs one.
- It does not say the strict mode is expensive. Strict and lenient allocate **identically** —
  4,192 B both — so the checks cost nothing in bytes and every byte here is construction. That
  was measured before the profile and it removed the first suspect for free.
- It does not measure time. Everything above is allocation, which is exact; the window's 1,919 ns
  came with a 37 % spread and cannot carry a claim this fine.

## 6. Open, for the architect

1. Is the intermediate flat array removable — could `Nodes` write straight into the list the
   scopes are cut from, so a node exists once before it reaches its scope?
2. Should the `Reader`'s list be retained per thread rather than per message, the way the
   parser's own stores are? That is the same shape as the pool the generator emits, and the same
   bound question applies — which is a good reason to ask it beside `KeptEntries` rather than
   separately.
3. Neither is worth doing on the strength of this document alone: 2,500 B a message is what it
   is, and whether it is worth a change depends on what a consumer parses per second. A row for
   the message layer in the stand would say; there is none today.
