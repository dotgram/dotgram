# Writes the FIX 4.4 message model and its checks from the FIX repository:
#
#     FixMessage.Types.cs   a class a message type, its fields read in one switch
#     FixComponents.cs      an interface a component, its repeating groups nested in it
#     FixValidator44.cs      the check of every message type, component and group entry
#     ../FixTag.cs          the number of every field, as a constant named for it
#     FixStandard.cs        the type of the value of every field
#
# The repository is tests/Corpus/FixRepository/FIX.4.4/Base, and a field is named as the
# repository names it, or as the common data dictionaries do where the two differ. A field is a class of the type of
# its value, FixField.Decimal, and its tag says which field it is. Groups are named for their counter: a group is the
# class <Counter>Group, nested in whatever carries it — a message, a component's interface, or the
# entry of another group — and its entries are the list <Counter>Groups beside the counter. So the
# names a data dictionary uses are the names of the code, and a dictionary loaded at run time
# is written into checks by putting its names in place.
#
#     python src/DotGram.Finance/Fix44/generate.py
#
# The fields' own checks (FixValidator44.Fields.cs), the standard header (FixValidator44.Header.cs),
# the base class (FixMessage.cs) and the helpers are written by hand and not touched here.

import os
import collections
import xml.etree.ElementTree as ET

here = os.path.dirname(os.path.abspath(__file__))
root = os.path.normpath(os.path.join(here, "..", "..", "..", ".."))
base = os.path.join(root, "tests", "Corpus", "FixRepository", "FIX.4.4", "Base")


def rows(name):
    for e in ET.parse(os.path.join(base, name)).getroot():
        yield {c.tag: (c.text or "").strip() for c in e}


# ── what the repository says ───────────────────────────────────────────────────────────────────

fields     = list(rows("Fields.xml"))
field_type = {int(f["Tag"]): f["Type"] for f in fields}

# The names of the fields: the repository's, and the data dictionaries' for the two they name otherwise.
field_name = {int(f["Tag"]): f["Name"] for f in fields}
field_name.update({23: "IOIid", 33: "LinesOfText"})

# The class a field's value is read into, by the type the repository gives the field.
value_class = {
    "String": "Text", "Currency": "Text", "Exchange": "Text", "Country": "Text",
    "char": "Character",
    "Boolean": "Boolean",
    "int": "Integer", "Length": "Integer", "SeqNum": "Integer", "NumInGroup": "Integer",
    "float": "Decimal", "Qty": "Decimal", "Price": "Decimal", "PriceOffset": "Decimal", "Amt": "Decimal", "Percentage": "Decimal",
    "UTCTimestamp": "Timestamp",
    "UTCTimeOnly": "Time",
    "UTCDateOnly": "Date", "LocalMktDate": "Date",
    "MonthYear": "MonthYear",
    "MultipleValueString": "Multiple",
    "data": "Data",
}

# FIX 4.4 declares these two char and, in the same table, publishes values a single character
# cannot hold: 10, 11 and 12 for MiscFeeType, 99 for MassCancelRejectReason. Where the declared type
# cannot hold what the specification publishes, the field is read as text: it keeps every value the
# document prints, and which of them are allowed is the code set's answer rather than the type's shape.
field_class = {tag: value_class[type_] for tag, type_ in field_type.items()}
field_class.update({139: "Text", 532: "Text"})

components = {c["Name"]: c for c in rows("Components.xml")}
contents   = collections.defaultdict(list)

for r in rows("MsgContents.xml"):
    contents[r["ComponentID"]].append(r)

for cid in contents:
    contents[cid].sort(key=lambda r: float(r["Position"]))

# The data dictionaries' names for the four types the repository names otherwise.
renamed = {
    "IOI":                                     "IndicationOfInterest",
    "MultilegOrderCancelReplace":              "MultilegOrderCancelReplaceRequest",
    "NetworkCounterpartySystemStatusRequest":  "NetworkStatusRequest",
    "NetworkCounterpartySystemStatusResponse": "NetworkStatusResponse",
}

messages = sorted(
    ({"Name": renamed.get(m["Name"], m["Name"]), "MsgType": m["MsgType"], "Id": m["ComponentID"]} for m in rows("Messages.xml")),
    key=lambda m: m["Name"])


# ── the model ──────────────────────────────────────────────────────────────────────────────────

class Field:
    def __init__(self, tag, required):
        self.tag, self.required = tag, required
        self.name = field_name[tag]

    @property
    def type_name(self):
        return "FixField." + field_class[self.tag]

class Block:
    def __init__(self, name, required):
        self.name, self.required = name, required

class Group:
    def __init__(self, counter, required, entry):
        self.counter, self.required, self.entry = counter, required, entry

    @property
    def list(self):
        return self.counter.name + "Groups"

class Entry:
    """The entry of a group: a class nested in its carrier, named for the counter."""
    def __init__(self, name, type_name, slot, members):
        self.name, self.type_name, self.slot, self.members = name, type_name, slot, members

    @property
    def opener(self):
        return first_field(self.members)


def first_field(members):
    head = members[0]

    if isinstance(head, Field):
        return head

    if isinstance(head, Block):
        return first_field(interfaces[head.name].members)

    return head.counter


def members_of(component_rows, owner_type, owner_slot):
    """The members a list of rows gives a carrier whose C# type is owner_type and whose slots begin owner_slot.

    A group is written two ways in the repository: as a repeating component referred to by name, and
    inline, as a counter followed by its members one indent deeper. Both are the same group here."""
    found, at = [], 0

    while at < len(component_rows):
        r        = component_rows[at]
        text     = r["TagText"]
        required = r["Reqd"] == "1"
        indent   = int(r["Indent"])

        if text in ("StandardHeader", "StandardTrailer"):
            at += 1
            continue

        if text.isdigit():
            end = at + 1

            while end < len(component_rows) and int(component_rows[end]["Indent"]) > indent:
                end += 1

            if end > at + 1:
                found.append(group_of(int(text), required, component_rows[at + 1:end], owner_type, owner_slot))
            else:
                found.append(Field(int(text), required))

            at = end
            continue

        component = components[text]
        kind      = component["ComponentType"]

        if kind in ("Block", "BlockRepeating"):
            found.append(Block(text, required))
        elif kind == "ImplicitBlockRepeating":
            inner = contents[component["ComponentID"]]
            found.append(group_of(int(inner[0]["TagText"]), required, inner[1:], owner_type, owner_slot))
        # Hop, the one ImplicitBlock, belongs to the standard header, which is written by hand.

        at += 1

    return found


def group_of(counter_tag, required, entry_rows, owner_type, owner_slot):
    counter = Field(counter_tag, required)
    name    = counter.name + "Group"
    type_   = owner_type + "." + name
    slot    = owner_slot + "_" + counter.name
    entry   = Entry(name, type_, slot, [])

    entry.members = members_of(entry_rows, type_, slot)

    return Group(counter, required, entry)


class Interface:
    def __init__(self, name, members):
        self.name, self.members = name, members

    @property
    def type_name(self):
        return "I" + self.name

    @property
    def bases(self):
        return [m.name for m in self.members if isinstance(m, Block)]


interfaces = {}

for name, component in sorted(components.items()):
    kind = component["ComponentType"]

    if name in ("StandardHeader", "StandardTrailer") or kind not in ("Block", "BlockRepeating"):
        continue

    if kind == "Block":
        interfaces[name] = Interface(name, [])
    else:
        interfaces[name] = Interface(name, [])

for name, interface in interfaces.items():
    component = components[name]

    interface.members = members_of(contents[component["ComponentID"]], "I" + name, name)

message_members = {m["Name"]: members_of(contents[m["Id"]], "FixMessage." + m["Name"], m["Name"]) for m in messages}


def properties(members):
    """Every property a carrier of these members declares, its blocks' included: (kind, member)."""
    for m in members:
        if isinstance(m, Field):
            yield "field", m
        elif isinstance(m, Group):
            yield "field", m.counter
            yield "list", m
        else:
            yield from properties(interfaces[m.name].members)


def blocks_of(members):
    """The interfaces a carrier implements: every block it carries, and every block those carry."""
    out = []

    for m in members:
        if isinstance(m, Block):
            out.append(m.name)
            out.extend(blocks_of(interfaces[m.name].members))

    return sorted(set(out))


def entries_in(members):
    """The groups a carrier nests: its own, and not those of the blocks it carries."""
    return [m for m in members if isinstance(m, Group)]


def all_entries(members):
    for g in entries_in(members):
        yield g.entry
        yield from all_entries(g.entry.members)


# ── writing ────────────────────────────────────────────────────────────────────────────────────

def wire(tag):
    return field_type.get(tag, "String")


def field_doc(f):
    return f"The FIX {f.name}, tag {f.tag}, wire type <c>{wire(f.tag)}</c>; null when the field is absent."


def list_doc(g):
    return f"The entries counted by {g.counter.name}, tag {g.counter.tag}; null when the group is absent."


def class_body(members, indent, setter, opener=None):
    """The properties of a carrier class, and the classes of the groups it nests."""
    pad, out = "\t" * indent, []

    for kind, m in properties(members):
        if kind == "field":
            out.append(f"{pad}/// <summary>{field_doc(m)}</summary>")

            if opener is not None and m.tag == opener.tag:
                out.append(f"{pad}public required {m.type_name} {m.name} {{ get; init; }}")
            else:
                out.append(f"{pad}public {m.type_name}? {m.name} {{ get; {setter}; }}")
        else:
            out.append(f"{pad}/// <summary>{list_doc(m)}</summary>")
            out.append(f"{pad}public List<{m.entry.type_name}>? {m.list} {{ get; {setter}; }}")

        out.append("")

    for g in entries_in(members):
        out.extend(entry_class(g, indent))

    return out


def entry_class(g, indent):
    pad, e = "\t" * indent, g.entry
    implements = blocks_of(e.members)
    head = f"{pad}public sealed class {e.name}" + (" : " + ", ".join("I" + b for b in implements) if implements else "")
    out = [f"{pad}/// <summary>One entry of the group counted by {g.counter.name}, tag {g.counter.tag}.</summary>", head, pad + "{"]
    out.extend(class_body(e.members, indent + 1, "internal set", e.opener))

    while out[-1] == "":
        out.pop()

    out += [pad + "}", ""]

    return out


# The reading: one switch over the tags of a type, each arm placing its field where it belongs —
# on the message, or on the last entry of the group it is in.

def arms(members, holder, lists, index):
    out = []

    def failing():
        return " || ".join(f"{l} is null || {l}.Count == 0" if i == 0 else f"{l} is null || {l}!.Count == 0" for i, l in enumerate(lists))

    def place(f):
        if not lists:
            out.append(f"\t\t\t\t\tcase FixTag.{f.name}: if ({f.name} is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); {f.name} = ({f.type_name})field; break;")
            return

        out.append(f"\t\t\t\t\tcase FixTag.{f.name}:")
        out.append(f"\t\t\t\t\t\tif ({failing()})")
        out.append( "\t\t\t\t\t\t\tAddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));")
        out.append(f"\t\t\t\t\t\telse if ({holder}{f.name} is not null)")
        out.append(f"\t\t\t\t\t\t\tAddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, {index}));")
        out.append( "\t\t\t\t\t\telse")
        out.append(f"\t\t\t\t\t\t\t{holder}{f.name} = ({f.type_name})field;")
        out.append( "\t\t\t\t\t\tbreak;")

    def walk(members, skip):
        for m in members:
            if isinstance(m, Field):
                if m.tag != skip:
                    place(m)
            elif isinstance(m, Block):
                walk(interfaces[m.name].members, skip)
            else:
                place(m.counter)

                opener = m.entry.opener
                target = f"{holder}{m.list}"

                # The first entry of a group whose counter has not been read is entries nothing counts:
                # said once, at the entry, by the counter's tag.
                uncounted = f"if ({target} is null && {holder}{m.counter.name} is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, FixTag.{m.counter.name}, field.Position, field, -1));"
                add       = f"({target} ??= []).Add(new () {{ {opener.name} = ({opener.type_name})field }});"

                if not lists:
                    out.append(f"\t\t\t\t\tcase FixTag.{opener.name}:")
                    out.append(f"\t\t\t\t\t\t{uncounted}")
                    out.append(f"\t\t\t\t\t\t{add}")
                    out.append( "\t\t\t\t\t\tbreak;")
                else:
                    out.append(f"\t\t\t\t\tcase FixTag.{opener.name}:")
                    out.append(f"\t\t\t\t\t\tif ({failing()})")
                    out.append( "\t\t\t\t\t\t\tAddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));")
                    out.append( "\t\t\t\t\t\telse")
                    out.append( "\t\t\t\t\t\t{")
                    out.append(f"\t\t\t\t\t\t\t{uncounted}")
                    out.append(f"\t\t\t\t\t\t\t{add}")
                    out.append( "\t\t\t\t\t\t}")
                    out.append( "\t\t\t\t\t\tbreak;")

                out.extend(arms(m.entry.members, f"{target}![^1].", lists + [target], f"{target}!.Count - 1")[1])

    walk(members, None if not lists else first_field(members).tag)

    return None, out


def message_class(m):
    members = message_members[m["Name"]]
    implements = blocks_of(members)
    head = f"\tpublic sealed partial class {m['Name']} : FixMessage" + "".join(", I" + b for b in implements)
    out = [f"\t/// <summary>FIX 4.4 {m['Name']}, MsgType {m['MsgType']}.</summary>", head, "\t{",
           f"\t\tinternal {m['Name']}(List<FixField> fields) : base(\"{m['MsgType']}\", fields)", "\t\t{",
           "\t\t\tforeach (var field in fields)", "\t\t\t{", "\t\t\t\tswitch (field.Tag)", "\t\t\t\t{"]
    out.extend(arms(members, "", [], "-1")[1])
    out += ["", "\t\t\t\t\tdefault:", "\t\t\t\t\t\tif (!SetStandardField(field))",
            "\t\t\t\t\t\t\tAddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));", "",
            "\t\t\t\t\t\tbreak;", "\t\t\t\t}", "\t\t\t}", "\t\t}", ""]
    out.extend(class_body(members, 2, "internal set"))
    out += ["\t\t/// <summary>Asks the context for the check of this type and runs it.</summary>",
            "\t\tprivate protected override void Check(Fix44Context context)", "\t\t{",
            f"\t\t\tcontext.Validators.{m['Name']}(context, this);", "\t\t}", "\t}", ""]

    return out


def interface_text(i):
    bases = i.bases
    head = f"public interface {i.type_name}" + (" : " + ", ".join("I" + b for b in bases) if bases else "")
    kind = "repeating component" if components[i.name]["ComponentType"] == "BlockRepeating" else "block"
    out = [f"/// <summary>The FIX 4.4 {i.name} {kind}, wherever it is carried.</summary>", head, "{"]

    for m in i.members:
        if isinstance(m, Field):
            out += [f"\t/// <summary>The FIX {m.name}, tag {m.tag}.</summary>", f"\t{m.type_name}? {m.name} {{ get; }}", ""]
        elif isinstance(m, Group):
            out += [f"\t/// <summary>The FIX {m.counter.name}, tag {m.counter.tag}.</summary>", f"\t{m.counter.type_name}? {m.counter.name} {{ get; }}", ""]
            out += [f"\t/// <summary>{list_doc(m)}</summary>", f"\tList<{m.entry.type_name}>? {m.list} {{ get; }}", ""]

    for g in entries_in(i.members):
        out.extend(entry_class(g, 1))

    while out[-1] == "":
        out.pop()

    return out + ["}", ""]


# The checks: what the repository requires of a carrier, each block it holds, each field handed to
# its slot, each group's count held to its entries and each entry to its own check — in the order
# the repository lists them, which is the order a dictionary loaded at run time is written in.

def check_body(members, subject, entry_opener=None):
    out = []

    for m in members:
        if isinstance(m, Field):
            field(m, out, subject, entry_opener)
        elif isinstance(m, Block):
            first = first_field(interfaces[m.name].members)

            if m.required:
                out.append(f"\t\tif (Empty((I{m.name}){subject})) Absent(message, FixTag.{first.name});")
                out.append(f"\t\telse context.Validators.{m.name}(context, message, {subject});")
            else:
                out.append(f"\t\tif (!Empty((I{m.name}){subject})) context.Validators.{m.name}(context, message, {subject});")
        else:
            field(m.counter, out, subject, entry_opener)
            out.append(f"\t\tCounted(message, {subject}.{m.counter.name}, {subject}.{m.list});")
            out.append(f"\t\tif ({subject}.{m.list} is not null)")
            out.append(f"\t\t\tfor (var i = 0; i < {subject}.{m.list}.Count; i++)")
            out.append(f"\t\t\t\tcontext.Validators.{m.entry.slot}(context, message, {subject}.{m.list}[i], i);")

    return out


def field(f, out, subject, entry_opener):
    call = f"context.Validators.{f.name}(context, message, {subject}.{f.name});"

    if entry_opener is not None and f.tag == entry_opener.tag:
        # The field an entry opens with is there, or there would be no entry.
        out.append(f"		{call}")
    elif f.required:
        where = f", {subject}.{entry_opener.name}.Position, index" if entry_opener is not None else ""
        out.append(f"\t\tif ({subject}.{f.name} is null) Missing(message, FixTag.{f.name}{where});")
        out.append(f"\t\telse {call}")
    else:
        out.append(f"\t\tif ({subject}.{f.name} is not null) {call}")


def validators_text():
    slots, checks, empties = [], [], []

    for m in messages:
        slots += [f"\t/// <summary>Holds a FIX 4.4 {m['Name']} to the schema.</summary>",
                  f"\tpublic Func<Fix44Context, FixMessage.{m['Name']}, bool> {m['Name']} {{ get; set; }} = Validate{m['Name']};", ""]
        checks += [f"\tstatic bool Validate{m['Name']}(Fix44Context context, FixMessage.{m['Name']} message)", "\t{"]
        checks += check_body(message_members[m["Name"]], "message")
        checks += ["", "\t\treturn message.IsValid;", "\t}", ""]

    for i in interfaces.values():
        slots += [f"\t/// <summary>Holds a FIX 4.4 {i.name} to the schema, wherever it is carried.</summary>",
                  f"\tpublic Func<Fix44Context, FixMessage, {i.type_name}, bool> {i.name} {{ get; set; }} = Validate{i.name};", ""]
        checks += [f"\tstatic bool Validate{i.name}(Fix44Context context, FixMessage message, {i.type_name} block)", "\t{"]
        checks += check_body(i.members, "block")
        checks += ["", "\t\treturn message.IsValid;", "\t}", ""]

        props = [m for kind, m in properties(i.members)]
        empties += [f"\t/// <summary>Whether a carrier of the FIX 4.4 {i.name} has none of its fields.</summary>",
                    f"\tinternal static bool Empty({i.type_name} block)", "\t{",
                    "\t\treturn " + " &&\n\t\t\t".join(f"block.{p.name if isinstance(p, Field) else p.list} is null" for p in props) + ";", "\t}", ""]

    entries = [e for i in interfaces.values() for e in all_entries(i.members)] + \
              [e for m in messages for e in all_entries(message_members[m["Name"]])]

    for e in entries:
        slots += [f"\t/// <summary>Holds one entry of {e.type_name} to the schema.</summary>",
                  f"\tpublic Func<Fix44Context, FixMessage, {e.type_name}, int, bool> {e.slot} {{ get; set; }} = Validate{e.slot};", ""]
        checks += [f"\tstatic bool Validate{e.slot}(Fix44Context context, FixMessage message, {e.type_name} entry, int index)", "\t{"]
        checks += check_body(e.members, "entry", e.opener)
        checks += ["", "\t\treturn message.IsValid;", "\t}", ""]

    head = [
        "using System;",
        "using System.Collections.Generic;",
        "",
        "namespace DotGram.Finance.Fix.Fix44;",
        "",
        "// Written by generate.py from the FIX 4.4 repository; not edited by hand.",
        "",
        "/// <summary>",
        "/// The check of every FIX 4.4 message type, of every component it reuses, and of every entry of",
        "/// every repeating group; the fields' own are in FixValidator44.Fields.cs.",
        "/// </summary>",
        "/// <remarks>",
        "/// Straight-line checks written against the fields of the class they are about, and nothing else:",
        "/// a field the repository marks required, a component it marks required and the carrier left empty,",
        "/// every field handed to its own slot, and every count held to the entries that follow it. Every",
        "/// slot is typed on what it checks and named for it — a message type, a component, and a group",
        "/// entry by the path to it, <c>NewOrderSingle_NoAllocs</c> — so that a dictionary loaded at run",
        "/// time replaces the slots it describes by name and leaves the rest.",
        "/// </remarks>",
        "partial class FixValidator44",
        "{",
        "\t/// <summary>What this package compiles in, which is what a context takes unless it is given another.</summary>",
        "\tpublic static readonly FixValidator44 Default = new();",
        "",
    ]

    return head + slots + checks + empties[:-1] + ["}", ""]


def write(name, lines):
    text = "\n".join(lines).replace("\r\n", "\n").replace("\n", "\r\n")
    open(os.path.join(here, name), "wb").write(b"\xef\xbb\xbf" + text.encode("utf-8"))
    print("written", name, len(lines), "lines")


types = [
    "using System.Collections.Generic;",
    "",
    "namespace DotGram.Finance.Fix.Fix44;",
    "",
    "// Written by generate.py from the FIX 4.4 repository; not edited by hand.",
    "",
    "public abstract partial class FixMessage",
    "{",
]

for m in messages:
    types.extend(message_class(m))

types.pop()
types += ["}", ""]

component_lines = [
    "using System;",
    "",
    "// ReSharper disable InconsistentNaming",
    "",
    "namespace DotGram.Finance.Fix.Fix44;",
    "",
    "// Written by generate.py from the FIX 4.4 repository; not edited by hand.",
    "//",
    "// The components FIX 4.4 reuses across message types, as the shape a message or a group entry has",
    "// when it carries one. A component is written into its carrier field by field, because that is",
    "// what it is on the wire; the interface is those same fields read as the one thing they are, so",
    "// that what is asked of a component is written once and asked of every carrier. A group inside a",
    "// component is the same group wherever the component is carried, so its entry is a class nested",
    "// in the interface: IInstrument.NoSecurityAltIDGroup.",
    "",
]

for i in interfaces.values():
    component_lines.extend(interface_text(i))

tag_lines = [
    "namespace DotGram.Finance.Fix;",
    "",
    "// Written by generate.py from the FIX 4.4 repository; not edited by hand.",
    "",
    "/// <summary>The number of every field of FIX 4.4, as a constant named for the field: <c>FixField.Decimal { Tag: FixTag.OrderQty }</c>.</summary>",
    "/// <remarks>A tag is its number, and a tag FIX 4.4 does not define is only that: <c>25005</c>.</remarks>",
    "public static class FixTag",
    "{",
]

for tag, name in sorted(field_name.items()):
    tag_lines += [f"\t/// <summary>{name}, a <see cref=\"FixField.{field_class[tag]}\"/>.</summary>", f"\tpublic const int {name} = {tag};", ""]

tag_lines[-1:] = ["}", ""]

width = max(len(name) for name in field_name.values())

standard_lines = [
    "namespace DotGram.Finance.Fix.Fix44;",
    "",
    "// Written by generate.py from the FIX 4.4 repository; not edited by hand.",
    "",
    "static class FixStandard",
    "{",
    "\t/// <summary>The type of the value of a field FIX 4.4 defines; <see cref=\"FixValueType.None\"/> for a tag it does not.</summary>",
    "\tinternal static FixValueType Type(int tag)",
    "\t{",
    "\t\treturn tag switch",
    "\t\t{",
]

for tag, name in sorted(field_name.items()):
    standard_lines.append(f"\t\t\tFixTag.{name.ljust(width)} => FixValueType.{field_class[tag]},")

standard_lines += [f"\t\t\t{'_'.ljust(width + 7)} => FixValueType.None,", "\t\t};", "\t}", "}", ""]

write(os.path.join("..", "FixTag.cs"), tag_lines)
write("FixStandard.cs", standard_lines)
write("FixMessage.Types.cs", types)
write("FixComponents.cs", component_lines[:-1] + [""])
write("FixValidator44.cs", validators_text())
