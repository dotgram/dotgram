# Building a message without copying its nodes three times (2026-09-20)

The numbers are in — this session's ladder, reproduced on the stand at the four step heights
computed before they were measured (`fix-message-bytes-2026-09-20.md`). This is the design the
architect asked for after them. No code is changed.

## The constraint, first, because it is what the shape is for

**A scope's nodes are not a contiguous range of the flat array as soon as a group is present.** A
group's counter keeps its entries hanging off it, and the entry nodes are cut out of the run, so
the body's own nodes are the input's positions *minus* the ones the groups consumed. Everything
below is bounded by that.

The `List<FixNode>` is not an untidiness somebody left behind. It is the answer to a real
question — a scope has to accumulate nodes whose count is not known until it stops, and whose
members are not adjacent in the input — and anything proposed here has to answer that question
too, or it is not a simplification but a defect. Whoever reads this next: the list is a solution,
not a mess, and the third path is the interesting one.

## What the three copies cost

A strict parse of a seventeen-field message is 4,192 B, of which `FixNode[]` is 63 %. Those bytes
are three things, separated by arithmetic over the measured ladder and agreeing with the profile's
per-frame split to within a few per cent:

| | on a 17-field message | what it is |
| --- | --: | --- |
| the staging `List<FixNode>` | ~1,190 B (~47 % of the nodes) | capacities 4, 8 and 16 allocated in turn, from empty, every message |
| the flat array from `Nodes` | ~670 B (~26 %) | one `FixNode[]` of every field, the input `Scope` walks |
| the per-scope arrays | ~750 B (~29 %) | header, body, trailer — **this is the message** |

The list's share grows with the message, because a doubling list allocates about twice the slots
it ends up using: at 64 body fields it is 4 + 8 + 16 + 32 + 64 = 124 slots, ~5,080 B, against a
whole message of 16,848 — about 30 % of everything, to hold 64 nodes.

So of the nodes' bytes, roughly three quarters are scaffolding and one quarter is the answer.

## Two changes, and they are not equal

### 1. Stop the list from growing from empty on every message

The `Reader` is new for every message and its list with it, so every message pays the whole
doubling ladder again. Retaining it across messages removes that entirely: the first message on a
thread pays the growth, and no later one does.

**This is not a new mechanism and must not become one.** It is the third consumer of the retention
that performance-ff is building for the emitted pools, on the same rule and quenched on the same
transition to idle, and it declares no retention policy of its own. The list holds references to
fields, so it is the same retention question, and answering it twice would be the defect this
repository spent a day removing from two other places.

Expected: ~30 % of a large message's bytes, ~28 % of a small one's, and nothing at all on the
first message a thread parses. Cheap, contained, and measurable by the ladder that already exists.

### 2. Remove the flat intermediate — harder, and the constraint is why

`Nodes` builds one array of every node, and `Scope` walks it by index while pushing accepted nodes
onto the list. So the flat array is the *input* and the list is the *output*: they are not two
copies of one thing, and "write straight into the list" does not typecheck as a plan.

What would remove it is counting before filling: walk the input to learn how many nodes each scope
takes, then fill the scope arrays directly. That is the classical trade — a second pass of time
against an array of bytes — and here it looks favourable, since the ladder is ~60 ns a field and
the nodes are a third of the bytes.

**But the counting pass has to make the same decisions as the filling one**, and those decisions
include parsing the groups: which entries a counter takes, and therefore where the body resumes.
Doing that twice is not a cheap pass; doing it once and remembering it is a staging list under
another name. So this is genuinely open, and I would not start it before (1) has landed and been
measured — (1) may take enough off that (2) stops being worth its complexity.

## What I would measure before and after

The ladder, unchanged: `fixmsg/slope-*` at body 8/9, 16/17, 32/33, 64/65. It is the right
instrument for exactly the reason it was built — **if (1) works, the four steps disappear** and
the line becomes flat at ~168 B a field. A change that removes a doubling ladder should be visible
as the disappearance of a doubling ladder, and if the steps are still there afterwards, the change
did not do what it says.

Time will not show it: the steps are +85 ns where a field is ~60, inside a base spread of 2-13 %.
Bytes answer this and time does not.

## Open

1. Does the retained list need a bound of its own, or does the shared mechanism's bound cover it?
   A list that once held a 10,000-field message would otherwise hold 400 KB for ever.
2. Is there a third path — one that neither stages nor counts twice? The obvious candidate is
   building each scope's array as a linked run and flattening once, which trades the list for
   per-node links and is probably worse. I have not found a better one, and I am not confident
   none exists.
3. `FixNode` is 40 bytes: two references and five ints, one of which (`GroupId`) is zero on every
   node that is not a group counter. Packing is a different change from this one and should not be
   folded into it, but it is the other lever on the same quantity.
