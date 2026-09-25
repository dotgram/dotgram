# Writes every version of FIX this package reads from the FIX repository, and FixTag from all of them:
#
#     python src/DotGram.Finance/Fix/generate.py
#
# Each version is a directory of its own, Fix/Fix44 for FIX 4.4, and everything in it is written here:
#
#     FixMessage.Types.cs          a class a message type, its fields read in one switch
#     FixMessage.Header.cs         the standard header and trailer, read and held by every message
#     FixComponents.cs             an interface a component, its repeating groups nested in it
#     FixStandard.cs               the type of the value of every field
#     FixValidator<NN>.cs          the check of every message type, component and group entry
#     FixValidator<NN>.Fields.cs   the check of every field against its type and its code set
#     and from Templates/, with the version's names and the tables the repository gives put in:
#     FixMessage.cs, FixCustomMessage.cs, FixParser.cs, FixParser.Messages.cs, FixParser.Streaming.cs,
#     Fix<NN>Context.cs, FixValidator<NN>.Header.cs
#
# and one file that every version shares, FixTag.cs: the number of every tag of every version, named
# as the newest version that has the tag names it.
#
# The repository is tests/Corpus/FixRepository. A field is named as the repository names it, or as
# the common data dictionaries do where the two differ; a field is a class of the type of its value,
# FixField.Decimal, and its tag says which field it is. Groups are named for their counter: a group is
# the class <Counter>Group, nested in whatever carries it — a message, a component's interface, or
# the entry of another group — and its entries are the list <Counter>Groups beside the counter. So the
# names a data dictionary uses are the names of the code, and a dictionary loaded at run time is
# written into checks by putting its names in place.

import os
import re
import sys
import collections
import xml.etree.ElementTree as ET

here      = os.path.dirname(os.path.abspath(__file__))
root      = os.path.normpath(os.path.join(here, "..", "..", ".."))
corpus    = os.path.join(root, "tests", "Corpus", "FixRepository")
templates = os.path.join(here, "Templates")

# The repository's notice lets portions of the specification be extracted into other work provided the
# origin is referenced and the specification is said to be Copyright FIX Protocol Limited; every table
# written here is such a portion. It says where the data came from and changes nothing about the
# licence of this code, which is the repository's own.
ATTRIBUTION = "// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org."


# ── the versions ───────────────────────────────────────────────────────────────────────────────

class Version:
    def __init__(self, key, directory, ns, title, begin, session=None, field_names=None, message_names=None, class_names=None):
        self.key, self.directory, self.ns, self.title, self.begin = key, directory, ns, title, begin
        # The repository that describes the session layer, where a version travels over another.
        self.session = session
        # The data dictionaries' names, where they name a field or a message otherwise than the repository.
        self.field_names   = field_names or {}
        self.message_names = message_names or {}
        # The classes named otherwise than the dictionaries name their messages, because C# would not
        # let the class keep the name; a dictionary loaded at run time is translated by the same table.
        self.class_names = class_names or {}

    @property
    def number(self):
        return self.ns[3:]

    @property
    def context(self):
        return f"Fix{self.number}Context"

    @property
    def validator(self):
        return f"FixValidator{self.number}"


VERSIONS = [
    Version("4.2", "FIX.4.2", "Fix42", "FIX 4.2", "FIX.4.2",
        message_names={
            "IOI":                      "IndicationofInterest",
            "OrderSingle":              "NewOrderSingle",
            "OrderList":                "NewOrderList",
            "AllocationInstructionAck": "AllocationACK",
        }),
    Version("4.4", "FIX.4.4", "Fix44", "FIX 4.4", "FIX.4.4",
        field_names={23: "IOIid", 33: "LinesOfText"},
        message_names={
            "IOI":                                     "IndicationOfInterest",
            "MultilegOrderCancelReplace":              "MultilegOrderCancelReplaceRequest",
            "NetworkCounterpartySystemStatusRequest":  "NetworkStatusRequest",
            "NetworkCounterpartySystemStatusResponse": "NetworkStatusResponse",
        }),
    Version("5.0", "FIX.5.0SP2", "Fix50", "FIX 5.0 SP2", "FIXT.1.1", session="FIXT.1.1",
        field_names={327: "HaltReasonInt"},
        class_names={"SecurityStatus": "SecurityStatusMessage"}),
]

# How a field's value is read, by the type the repository gives the field: the reader, which is also
# the class the value is held in except where CLASS names another.
VALUE_CLASS = {
    "String": "Text", "Currency": "Text", "Exchange": "Text", "Country": "Text", "Language": "Text",
    "char": "Character",
    "Boolean": "Boolean",
    "int": "Integer", "Length": "Integer", "SeqNum": "Integer", "NumInGroup": "Integer", "DayOfMonth": "Integer",
    "float": "Decimal", "Qty": "Decimal", "Price": "Decimal", "PriceOffset": "Decimal", "Amt": "Decimal", "Percentage": "Decimal",
    "UTCTimestamp": "Timestamp", "TZTimestamp": "ZonedTimestamp",
    "UTCTimeOnly": "Time", "TZTimeOnly": "ZonedTime",
    "UTCDateOnly": "Date", "UTCDate": "Date", "LocalMktDate": "Date",
    "MonthYear": "MonthYear",
    "MultipleValueString": "Multiple", "MultipleCharValue": "Multiple", "MultipleStringValue": "MultipleString",
    "data": "Data", "XMLData": "Data",
}

# A zoned timestamp is read by a reader of its own and held as any other instant is.
CLASS = {"ZonedTimestamp": "Timestamp", "MultipleString": "Multiple"}


def rows(directory, name):
    for e in ET.parse(os.path.join(corpus, directory, "Base", name)).getroot():
        yield {c.tag: (c.text or "").strip() for c in e}


def words(n):
    ones = "zero one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen".split()
    tens = "_ _ twenty thirty forty fifty sixty seventy eighty ninety".split()

    if n < 20:
        return ones[n]

    if n < 100:
        return tens[n // 10] + ("" if n % 10 == 0 else "-" + ones[n % 10])

    return str(n)


# ── the names every version shares ─────────────────────────────────────────────────────────────

def tag_names():
    """Every tag of every version, named as the newest version that has it names it."""
    names = {}

    for v in VERSIONS:
        for d in (v.directory,) + ((v.session,) if v.session else ()):
            for f in rows(d, "Fields.xml"):
                names[int(f["Tag"])] = f["Name"]

    return names


TAG_NAME = tag_names()


# ── one version ────────────────────────────────────────────────────────────────────────────────

class Model:
    """What the repository says of one version, and the C# it is written into."""

    def __init__(self, v):
        self.v = v
        # Where a version travels over a session layer, the application's repository repeats the
        # session's messages, fields and components; the session layer's own are the ones read.
        directories = (v.directory,) + ((v.session,) if v.session else ())

        fields = {}
        for d in directories:
            for f in rows(d, "Fields.xml"):
                fields[int(f["Tag"])] = f

        self.fields     = fields
        self.field_type = {t: f["Type"] for t, f in fields.items()}
        self.field_name = {t: v.field_names.get(t, f["Name"]) for t, f in fields.items()}

        enums = collections.defaultdict(list)
        for d in directories:
            for e in rows(d, "Enums.xml"):
                if int(e["Tag"]) in fields and e["Value"] not in enums[int(e["Tag"])]:
                    enums[int(e["Tag"])].append(e["Value"])

        self.enums = enums

        unknown = sorted({t for t in self.field_type.values() if t not in VALUE_CLASS})
        if unknown:
            raise SystemExit(f"{v.title}: no value class for the types {unknown}")

        # A field declared char whose code set publishes values a single character cannot hold is read as
        # text: it keeps every value the document prints, and which of them are allowed is the code set's
        # answer rather than the type's shape.
        self.value_type = {}
        for t, type_ in self.field_type.items():
            cls = VALUE_CLASS[type_]
            if cls == "Character" and any(len(code) != 1 for code in enums.get(t, ())):
                cls = "Text"
            self.value_type[t] = cls

        self.field_class = {t: CLASS.get(cls, cls) for t, cls in self.value_type.items()}

        # The length/data pairs: a Length field that names the data it measures, or, where the
        # repository leaves that out (three fields of FIX 5.0 SP2), the Length field named for the data.
        self.pairs = {}
        by_name = {f["Name"]: t for t, f in fields.items()}
        for t, f in fields.items():
            data = f.get("AssociatedDataTag")
            if data and f["Type"] == "Length" and self.field_type.get(int(data)) in ("data", "XMLData"):
                self.pairs[t] = int(data)
        for t, f in fields.items():
            if f["Type"] in ("data", "XMLData") and t not in self.pairs.values():
                length = by_name.get(f["Name"] + "Len", by_name.get(f["Name"] + "Length"))
                if length is not None and fields[length]["Type"] == "Length":
                    self.pairs[length] = t

        self.components = {}
        self.contents   = collections.defaultdict(list)
        for d in directories:
            for r in rows(d, "MsgContents.xml"):
                self.contents[(d, r["ComponentID"])].append(r)

        # A later directory's component wins where it has rows: FIXT 1.1 declares MsgTypeGrp and lists
        # none of its members, which the application's repository does.
        self.component_dir = {}
        for d in directories:
            for c in rows(d, "Components.xml"):
                if self.contents[(d, c["ComponentID"])] or c["Name"] not in self.component_dir:
                    self.component_dir[c["Name"]] = d
                    self.components[c["Name"]] = c

        for key in self.contents:
            self.contents[key].sort(key=lambda r: float(r["Position"]))

        messages = {}
        for d in directories:
            for m in rows(d, "Messages.xml"):
                named = v.message_names.get(m["Name"], m["Name"])
                messages[m["MsgType"]] = {"Name": v.class_names.get(named, named), "MsgType": m["MsgType"], "Id": (d, m["ComponentID"])}

        self.messages = sorted(messages.values(), key=lambda m: m["Name"])

        self.interfaces = {}
        for name, component in sorted(self.components.items()):
            if name in ("StandardHeader", "StandardTrailer") or component["ComponentType"] not in ("Block", "BlockRepeating"):
                continue
            self.interfaces[name] = Interface(self, name, [])

        for name, interface in self.interfaces.items():
            interface.members = self.members_of(self.component_rows(name), "I" + name, name)

        self.message_members = {m["Name"]: self.members_of(self.contents[m["Id"]], "FixMessage." + m["Name"], m["Name"]) for m in self.messages}

        taken = {m["Name"] for m in self.messages} | set(self.interfaces)
        self.field_slot = {t: name + "Field" if name in taken else name for t, name in self.field_name.items()}

        header  = self.members_of(self.component_rows("StandardHeader"), "FixMessage", "StandardHeader", header=True)
        trailer = self.members_of(self.component_rows("StandardTrailer"), "FixMessage", "StandardTrailer", header=True)
        self.header, self.trailer = header, trailer

    def component_rows(self, name):
        component = self.components[name]
        return self.contents[(self.component_dir[name], component["ComponentID"])]

    def tag(self, t):
        return f"FixTag.{TAG_NAME[t]}"

    def members_of(self, component_rows, owner_type, owner_slot, header=False):
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
                    found.append(self.group_of(int(text), required, component_rows[at + 1:end], owner_type, owner_slot))
                else:
                    found.append(Field(self, int(text), required))

                at = end
                continue

            component = self.components[text]
            kind      = component["ComponentType"]

            if kind in ("Block", "BlockRepeating") and not header:
                found.append(Block(text, required))
            elif kind in ("ImplicitBlockRepeating", "OptimisedImplicitBlockRepeating"):
                inner = self.component_rows(text)
                found.append(self.group_of(int(inner[0]["TagText"]), required, inner[1:], owner_type, owner_slot))
            else:
                # An implicit block, or a block of the header: its members are the carrier's own.
                found.extend(self.members_of(self.component_rows(text), owner_type, owner_slot, header))

            at += 1

        return found

    def group_of(self, counter_tag, required, entry_rows, owner_type, owner_slot):
        counter = Field(self, counter_tag, required)
        name    = counter.name + "Group"
        type_   = owner_type + "." + name
        slot    = owner_slot + "_" + counter.name
        entry   = Entry(self, name, type_, slot, [])

        entry.members = self.members_of(entry_rows, type_, slot)

        return Group(counter, required, entry)


class Field:
    def __init__(self, model, tag, required):
        self.model, self.tag, self.required = model, tag, required
        self.name = model.field_name[tag]

    @property
    def cls(self):
        return self.model.field_class[self.tag]

    @property
    def type_name(self):
        return "FixField." + self.cls

    @property
    def const(self):
        return self.model.tag(self.tag)

    @property
    def slot(self):
        return self.model.field_slot[self.tag]


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
    def __init__(self, model, name, type_name, slot, members):
        self.model, self.name, self.type_name, self.slot, self.members = model, name, type_name, slot, members

    @property
    def opener(self):
        return first_field(self.model, self.members)


class Interface:
    def __init__(self, model, name, members):
        self.model, self.name, self.members = model, name, members

    @property
    def type_name(self):
        return "I" + self.name

    @property
    def bases(self):
        return [m.name for m in self.members if isinstance(m, Block)]


def first_field(model, members):
    head = members[0]

    if isinstance(head, Field):
        return head

    if isinstance(head, Block):
        return first_field(model, model.interfaces[head.name].members)

    return head.counter


def properties(model, members):
    """Every property a carrier of these members declares, its blocks' included: (kind, member)."""
    for m in members:
        if isinstance(m, Field):
            yield "field", m
        elif isinstance(m, Group):
            yield "field", m.counter
            yield "list", m
        else:
            yield from properties(model, model.interfaces[m.name].members)


def blocks_of(model, members):
    """The interfaces a carrier implements: every block it carries, and every block those carry."""
    out = []

    for m in members:
        if isinstance(m, Block):
            out.append(m.name)
            out.extend(blocks_of(model, model.interfaces[m.name].members))

    return sorted(set(out))


def entries_in(members):
    """The groups a carrier nests: its own, and not those of the blocks it carries."""
    return [m for m in members if isinstance(m, Group)]


def all_entries(members):
    for g in entries_in(members):
        yield g.entry
        yield from all_entries(g.entry.members)


# ── writing ────────────────────────────────────────────────────────────────────────────────────

class Writer:
    def __init__(self, model):
        self.m, self.v = model, model.v

    # The heading every file written here carries.
    def written(self, source=None):
        """The heading every file written here carries: what wrote it, and where its tables come from,
        as the repository's notice asks of anything extracted from the specification."""
        made = f"// Written by generate.py from the {self.v.title} repository; not edited by hand."
        return made + (f" From {source}." if source else "") + "\n" + ATTRIBUTION

    def wire(self, tag):
        return self.m.field_type.get(tag, "String")

    def field_doc(self, f):
        return f"The FIX {f.name}, tag {f.tag}, wire type <c>{self.wire(f.tag)}</c>; null when the field is absent."

    @staticmethod
    def list_doc(g):
        return f"The entries counted by {g.counter.name}, tag {g.counter.tag}; null when the group is absent."

    def class_body(self, members, indent, setter, opener=None, doc=None):
        """The properties of a carrier class, and the classes of the groups it nests."""
        pad, out = "\t" * indent, []

        for kind, m in properties(self.m, members):
            if kind == "field":
                out.append(f"{pad}/// <summary>{(doc or self.field_doc)(m)}</summary>")

                if opener is not None and m.tag == opener.tag:
                    out.append(f"{pad}public required {m.type_name} {m.name} {{ get; init; }}")
                else:
                    out.append(f"{pad}public {m.type_name}? {m.name} {{ get; {setter}; }}")
            else:
                out.append(f"{pad}/// <summary>{self.list_doc(m)}</summary>")
                out.append(f"{pad}public List<{m.entry.type_name}>? {m.list} {{ get; {setter}; }}")

            out.append("")

        for g in entries_in(members):
            out.extend(self.entry_class(g, indent))

        return out

    def entry_class(self, g, indent):
        pad, e = "\t" * indent, g.entry
        implements = blocks_of(self.m, e.members)
        head = f"{pad}public sealed class {e.name}" + (" : " + ", ".join("I" + b for b in implements) if implements else "")
        out = [f"{pad}/// <summary>One entry of the group counted by {g.counter.name}, tag {g.counter.tag}.</summary>", head, pad + "{"]
        out.extend(self.class_body(e.members, indent + 1, "internal set", e.opener))

        while out[-1] == "":
            out.pop()

        out += [pad + "}", ""]

        return out

    # The reading: one switch over the tags of a type, each arm placing its field where it belongs —
    # on the message, or on the last entry of the group it is in.

    def arms(self, members, holder, lists, index, pad="\t\t\t\t\t"):
        out = []

        def failing():
            return " || ".join(f"{l} is null || {l}.Count == 0" if i == 0 else f"{l} is null || {l}!.Count == 0" for i, l in enumerate(lists))

        def place(f):
            if not lists:
                out.append(f"{pad}case {f.const}: if ({f.name} is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); {f.name} = ({f.type_name})field; break;")
                return

            out.append(f"{pad}case {f.const}:")
            out.append(f"{pad}\tif ({failing()})")
            out.append(f"{pad}\t\tAddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));")
            out.append(f"{pad}\telse if ({holder}{f.name} is not null)")
            out.append(f"{pad}\t\tAddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, {index}));")
            out.append(f"{pad}\telse")
            out.append(f"{pad}\t\t{holder}{f.name} = ({f.type_name})field;")
            out.append(f"{pad}\tbreak;")

        def walk(members, skip):
            for m in members:
                if isinstance(m, Field):
                    if m.tag != skip:
                        place(m)
                elif isinstance(m, Block):
                    walk(self.m.interfaces[m.name].members, skip)
                else:
                    # An entry may open with a group of its own, whose counter is then the entry's opener
                    # and taken where the entry begins.
                    if m.counter.tag != skip:
                        place(m.counter)

                    opener = m.entry.opener
                    target = f"{holder}{m.list}"

                    # The first entry of a group whose counter has not been read is entries nothing counts:
                    # said once, at the entry, by the counter's tag.
                    uncounted = f"if ({target} is null && {holder}{m.counter.name} is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, {m.counter.const}, field.Position, field, -1));"
                    add       = f"({target} ??= []).Add(new () {{ {opener.name} = ({opener.type_name})field }});"

                    if not lists:
                        out.append(f"{pad}case {opener.const}:")
                        out.append(f"{pad}\t{uncounted}")
                        out.append(f"{pad}\t{add}")
                        out.append(f"{pad}\tbreak;")
                    else:
                        out.append(f"{pad}case {opener.const}:")
                        out.append(f"{pad}\tif ({failing()})")
                        out.append(f"{pad}\t\tAddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));")
                        out.append(f"{pad}\telse")
                        out.append(f"{pad}\t{{")
                        out.append(f"{pad}\t\t{uncounted}")
                        out.append(f"{pad}\t\t{add}")
                        out.append(f"{pad}\t}}")
                        out.append(f"{pad}\tbreak;")

                    out.extend(self.arms(m.entry.members, f"{target}![^1].", lists + [target], f"{target}!.Count - 1", pad))

        walk(members, None if not lists else first_field(self.m, members).tag)

        return out

    def message_class(self, msg):
        members = self.m.message_members[msg["Name"]]
        implements = blocks_of(self.m, members)
        head = f"\tpublic sealed partial class {msg['Name']} : FixMessage" + "".join(", I" + b for b in implements)
        out = [f"\t/// <summary>{self.v.title} {msg['Name']}, MsgType {msg['MsgType']}.</summary>", head, "\t{",
               f"\t\tinternal {msg['Name']}(List<FixField> fields) : base(\"{msg['MsgType']}\", fields)", "\t\t{",
               "\t\t\tforeach (var field in fields)", "\t\t\t{", "\t\t\t\tswitch (field.Tag)", "\t\t\t\t{"]
        out.extend(self.arms(members, "", [], "-1"))
        out += ["", "\t\t\t\t\tdefault:", "\t\t\t\t\t\tif (!SetStandardField(field))",
                "\t\t\t\t\t\t\tAddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));", "",
                "\t\t\t\t\t\tbreak;", "\t\t\t\t}", "\t\t\t}", "\t\t}", ""]
        out.extend(self.class_body(members, 2, "internal set"))
        out += ["\t\t/// <summary>Asks the context for the check of this type and runs it.</summary>",
                f"\t\tprivate protected override void Check({self.v.context} context)", "\t\t{",
                f"\t\t\tcontext.Validators.{msg['Name']}(context, this);", "\t\t}", "\t}", ""]

        return out

    def interface_text(self, i):
        bases = i.bases
        head = f"public interface {i.type_name}" + (" : " + ", ".join("I" + b for b in bases) if bases else "")
        kind = "repeating component" if self.m.components[i.name]["ComponentType"] == "BlockRepeating" else "block"
        out = [f"/// <summary>The {self.v.title} {i.name} {kind}, wherever it is carried.</summary>", head, "{"]

        for mm in i.members:
            if isinstance(mm, Field):
                out += [f"\t/// <summary>The FIX {mm.name}, tag {mm.tag}.</summary>", f"\t{mm.type_name}? {mm.name} {{ get; }}", ""]
            elif isinstance(mm, Group):
                out += [f"\t/// <summary>The FIX {mm.counter.name}, tag {mm.counter.tag}.</summary>", f"\t{mm.counter.type_name}? {mm.counter.name} {{ get; }}", ""]
                out += [f"\t/// <summary>{self.list_doc(mm)}</summary>", f"\tList<{mm.entry.type_name}>? {mm.list} {{ get; }}", ""]

        for g in entries_in(i.members):
            out.extend(self.entry_class(g, 1))

        while out[-1] == "":
            out.pop()

        return out + ["}", ""]

    # The checks: what the repository requires of a carrier, each block it holds, each field handed to
    # its slot, each group's count held to its entries and each entry to its own check — in the order
    # the repository lists them, which is the order a dictionary loaded at run time is written in.

    def check_body(self, members, subject, entry_opener=None):
        out = []

        for mm in members:
            if isinstance(mm, Field):
                self.field_check(mm, out, subject, entry_opener)
            elif isinstance(mm, Block):
                first = first_field(self.m, self.m.interfaces[mm.name].members)

                if mm.required:
                    out.append(f"\t\tif (Empty((I{mm.name}){subject})) Absent(message, {first.const});")
                    out.append(f"\t\telse context.Validators.{mm.name}(context, message, {subject});")
                else:
                    out.append(f"\t\tif (!Empty((I{mm.name}){subject})) context.Validators.{mm.name}(context, message, {subject});")
            else:
                self.field_check(mm.counter, out, subject, entry_opener)
                out.append(f"\t\tCounted(message, {subject}.{mm.counter.name}, {subject}.{mm.list});")
                out.append(f"\t\tif ({subject}.{mm.list} is not null)")
                out.append(f"\t\t\tfor (var i = 0; i < {subject}.{mm.list}.Count; i++)")
                out.append(f"\t\t\t\tcontext.Validators.{mm.entry.slot}(context, message, {subject}.{mm.list}[i], i);")

        return out

    @staticmethod
    def field_check(f, out, subject, entry_opener):
        call = f"context.Validators.{f.slot}(context, message, {subject}.{f.name});"

        if entry_opener is not None and f.tag == entry_opener.tag:
            # The field an entry opens with is there, or there would be no entry.
            out.append(f"\t\t{call}")
        elif f.required:
            where = f", {subject}.{entry_opener.name}.Position, index" if entry_opener is not None else ""
            out.append(f"\t\tif ({subject}.{f.name} is null) Missing(message, {f.const}{where});")
            out.append(f"\t\telse {call}")
        else:
            out.append(f"\t\tif ({subject}.{f.name} is not null) {call}")

    def validators_text(self):
        v, ctx = self.v, self.v.context
        slots, checks, empties = [], [], []

        for msg in self.m.messages:
            slots += [f"\t/// <summary>Holds a {v.title} {msg['Name']} to the schema.</summary>",
                      f"\tpublic Func<{ctx}, FixMessage.{msg['Name']}, bool> {msg['Name']} {{ get; set; }} = Validate{msg['Name']};", ""]
            checks += [f"\tstatic bool Validate{msg['Name']}({ctx} context, FixMessage.{msg['Name']} message)", "\t{"]
            checks += self.check_body(self.m.message_members[msg["Name"]], "message")
            checks += ["", "\t\treturn message.IsValid;", "\t}", ""]

        for i in self.m.interfaces.values():
            slots += [f"\t/// <summary>Holds a {v.title} {i.name} to the schema, wherever it is carried.</summary>",
                      f"\tpublic Func<{ctx}, FixMessage, {i.type_name}, bool> {i.name} {{ get; set; }} = Validate{i.name};", ""]
            checks += [f"\tstatic bool Validate{i.name}({ctx} context, FixMessage message, {i.type_name} block)", "\t{"]
            checks += self.check_body(i.members, "block")
            checks += ["", "\t\treturn message.IsValid;", "\t}", ""]

            props = [mm for kind, mm in properties(self.m, i.members)]
            empties += [f"\t/// <summary>Whether a carrier of the {v.title} {i.name} has none of its fields.</summary>",
                        f"\tinternal static bool Empty({i.type_name} block)", "\t{",
                        "\t\treturn " + " &&\n\t\t\t".join(f"block.{p.name if isinstance(p, Field) else p.list} is null" for p in props) + ";", "\t}", ""]

        entries = [e for i in self.m.interfaces.values() for e in all_entries(i.members)] + \
                  [e for msg in self.m.messages for e in all_entries(self.m.message_members[msg["Name"]])]

        for e in entries:
            slots += [f"\t/// <summary>Holds one entry of {e.type_name} to the schema.</summary>",
                      f"\tpublic Func<{ctx}, FixMessage, {e.type_name}, int, bool> {e.slot} {{ get; set; }} = Validate{e.slot};", ""]
            checks += [f"\tstatic bool Validate{e.slot}({ctx} context, FixMessage message, {e.type_name} entry, int index)", "\t{"]
            checks += self.check_body(e.members, "entry", e.opener)
            checks += ["", "\t\treturn message.IsValid;", "\t}", ""]

        head = [
            "using System;",
            "using System.Collections.Generic;",
            "",
            f"namespace DotGram.Finance.Fix.{v.ns};",
            "",
            self.written(),
            "",
            "/// <summary>",
            f"/// The check of every {v.title} message type, of every component it reuses, and of every entry of",
            f"/// every repeating group; the fields' own are in {v.validator}.Fields.cs.",
            "/// </summary>",
            "/// <remarks>",
            "/// Straight-line checks written against the fields of the class they are about, and nothing else:",
            "/// a field the repository marks required, a component it marks required and the carrier left empty,",
            "/// every field handed to its own slot, and every count held to the entries that follow it. Every",
            "/// slot is typed on what it checks and named for it — a message type, a component, and a group",
            "/// entry by the path to it, <c>NewOrderSingle_NoAllocs</c> — so that a dictionary loaded at run",
            "/// time replaces the slots it describes by name and leaves the rest.",
            "/// </remarks>",
            f"partial class {v.validator}",
            "{",
            "\t/// <summary>What this package compiles in, which is what a context takes unless it is given another.</summary>",
            f"\tpublic static readonly {v.validator} Default = new();",
            "",
        ]

        return head + self.translations() + slots + checks + empties[:-1] + ["}", ""]

    def translations(self):
        """The names a dictionary loaded at run time is translated by, where the version cannot keep its own."""
        out = []
        classes = self.v.class_names
        fields  = {name: self.m.field_slot[t] for t, name in self.m.field_name.items() if self.m.field_slot[t] != name}

        for method, table, why in [
                ("MessageClass", classes, "a message whose class cannot have the name the dictionaries give it"),
                ("FieldSlotName", fields, "a field whose name a message or a component has first")]:
            if not table:
                continue

            width = max(len(k) for k in table) + 2
            out += [f"\t/// <summary>The translation of {why}.</summary>",
                    f"\tprivate protected override string {method}(string name)", "\t{", "\t\treturn name switch", "\t\t{"]
            out += [f"\t\t\t{(chr(34) + k + chr(34)).ljust(width)} => \"{table[k]}\"," for k in sorted(table)]
            out += [f"\t\t\t{'_'.ljust(width)} => name,", "\t\t};", "\t}", ""]

        # The fields the version names otherwise than FixTag, which takes the newest version's names:
        # a fragment of a dictionary may name one it does not describe.
        tags = {name: t for t, name in self.m.field_name.items() if TAG_NAME[t] != name}

        if tags:
            width = max(len(k) for k in tags) + 2
            out += ["\t/// <summary>The tag of a field this version names otherwise than FixTag does.</summary>",
                    "\tprivate protected override int FieldTag(string name)", "\t{", "\t\treturn name switch", "\t\t{"]
            out += [f"\t\t\t{(chr(34) + k + chr(34)).ljust(width)} => {tags[k]}," for k in sorted(tags)]
            out += [f"\t\t\t{'_'.ljust(width)} => 0,", "\t\t};", "\t}", ""]

        return out

    # The fields' own checks: the value's type, and the code set the repository publishes for it.

    @staticmethod
    def literal(cls, code):
        if cls == "Character":
            return "'" + ("\\" + code if code in ("\\", "'") else code) + "'"
        if cls in ("Integer", "Decimal"):
            return code
        return '"' + code.replace("\\", "\\\\").replace('"', '\\"') + '"'

    @staticmethod
    def wrapped(items, first, pad, width=116):
        """The alternatives of a pattern, `a or b or c`, broken at `or` where a line passes the width."""
        lines, line = [], first
        for k, item in enumerate(items):
            piece = item + (" or" if k < len(items) - 1 else "")
            if len(line.expandtabs(4)) + len(piece) + 1 > width and not line.endswith("("):
                lines.append(line)
                line = pad + piece
            else:
                line += ("" if line.endswith("(") else " ") + piece
        lines.append(line)
        return lines

    def fields_text(self):
        v, ctx = self.v, self.v.context
        tags = sorted(self.m.field_name)
        rows_ = []

        for t in tags:
            name, cls = self.m.field_name[t], self.m.field_class[t]
            codes = self.m.enums.get(t) if cls != "Boolean" else None
            listed = "the values the specification lists" if codes else "its type"
            rows_.append((f"\t/// <summary>Holds a {v.title} {name}, tag {t}, to {listed}.</summary>", f"Func<{ctx}, FixMessage, FixField.{cls}, bool>", self.m.field_slot[t]))

        tw = max(len(r[1]) for r in rows_)
        nw = max(len(r[2]) for r in rows_)

        out = [
            "using System;",
            "",
            f"namespace DotGram.Finance.Fix.{v.ns};",
            "",
            self.written(),
            "",
            f"/// <summary>The check of every field of {v.title} against its type, and against the values the repository lists for it.</summary>",
            f"partial class {v.validator}",
            "{",
            "\t// The fields, every one, so that a dictionary has a slot for whatever it limits.",
            "",
        ]

        for doc, func, name in rows_:
            out += [doc, f"\tpublic {func.ljust(tw)} {name.ljust(nw)} {{ get; set; }} = Validate{name};", ""]

        for t in tags:
            name, cls = self.m.field_name[t], self.m.field_class[t]
            codes = self.m.enums.get(t) if cls != "Boolean" else None
            out.append(f"\tstatic bool Validate{self.m.field_slot[t]}({ctx} context, FixMessage message, FixField.{cls} field)")
            out.append("\t{")

            if not codes:
                out += ["\t\tif (!field.IsValid)", "\t\t\tInvalid(message, field);"]
            else:
                items = [self.literal("Text" if cls in ("Text", "Multiple", "MonthYear") else cls, c) for c in sorted(codes)]

                if cls == "Multiple":
                    out.append("\t\tforeach (var code in field.Value)")
                    out += self.wrapped(items, "\t\t\tif (code is not (", "\t\t\t\t")
                    out[-1] += "))"
                    out += ["\t\t\t{", "\t\t\t\tInvalid(message, field);", "", "\t\t\t\tbreak;", "\t\t\t}"]
                elif cls in ("Text", "Character"):
                    out += self.wrapped(items, "\t\tif (field.Value is not (", "\t\t\t")
                    out[-1] += "))"
                    out.append("\t\t\tInvalid(message, field);")
                else:
                    out += ["\t\tif (!field.IsValid)", "\t\t\tInvalid(message, field);"]
                    out += self.wrapped(items, "\t\telse if (field.Value is not (", "\t\t\t")
                    out[-1] += "))"
                    out.append("\t\t\tInvalid(message, field);")

            out += ["", "\t\treturn message.IsValid;", "\t}", ""]

        out.pop()
        return out + ["}", ""]

    # The standard header and trailer, which every message carries and reads before its own fields.

    def header_text(self):
        v = self.v
        members = self.m.header + self.m.trailer
        out = [
            "using System.Collections.Generic;",
            "",
            f"namespace DotGram.Finance.Fix.{v.ns};",
            "",
            self.written(),
            "",
            "public abstract partial class FixMessage",
            "{",
            "\t/// <summary>",
            "\t/// Takes a field of the standard header or trailer, and answers whether the tag was one of",
            "\t/// theirs, so that a message type's own constructor can report what neither it nor this took.",
            "\t/// </summary>",
            "\t/// <param name=\"field\">The field to take.</param>",
            "\tprotected bool SetStandardField(FixField field)",
            "\t{",
            "\t\tswitch (field.Tag)",
            "\t\t{",
        ]
        out += self.arms(members, "", [], "-1", pad="\t\t\t")
        out += ["", "\t\t\tdefault: return false;", "\t\t}", "", "\t\treturn true;", "\t}", ""]

        def doc(f):
            part = "StandardHeader" if any(f.tag == h.tag for h in self.flat(self.m.header)) else "StandardTrailer"
            return f"The FIX {f.name}, tag {f.tag}, of this message's {part}; null when the field is absent."

        out += self.class_body(members, 1, "protected set", doc=doc)

        while out[-1] == "":
            out.pop()

        return out + ["}", ""]

    def flat(self, members):
        for mm in members:
            if isinstance(mm, Field):
                yield mm
            elif isinstance(mm, Group):
                yield mm.counter
                yield from self.flat(mm.entry.members)

    # The tables the templates are given.

    def message_switch(self):
        width = max(len(msg["Name"]) for msg in self.m.messages)
        longest = max(len(msg["MsgType"]) for msg in self.m.messages)

        if longest > 2:
            raise SystemExit(f"{self.v.title}: a MsgType of {longest} characters, which the switch does not read")

        out = []
        for msg in sorted(self.m.messages, key=lambda msg: (len(msg["MsgType"]), msg["MsgType"])):
            t = msg["MsgType"]
            key = f"({len(t)}, '{t[0]}', {('_  ' if len(t) == 1 else repr(t[1]))})"
            out.append(f"\t\t\t\t{key} => new FixMessage.{msg['Name'].ljust(width)} (fields),")

        return out

    def version_text(self):
        pairs = sorted(self.m.pairs.items(), key=lambda p: p[1])
        lw = max(len(TAG_NAME[l]) for l, _ in pairs) + len("FixTag.") + 1
        dw = max(len(TAG_NAME[d]) for _, d in pairs) + len("FixTag.")
        out = ["\t\tpublic static readonly FixVersion Version = new(new()", "\t\t{"]
        for l, d in pairs:
            out.append(f"\t\t\t{{ {('FixTag.' + TAG_NAME[l] + ',').ljust(lw)} {('FixTag.' + TAG_NAME[d]).ljust(dw)} }},")
        last = max(self.m.field_name)
        out.append(f"\t\t}}, FixStandard.Type, {self.m.tag(last)});")
        return out

    def header_required(self):
        required = [f for f in self.flat(self.m.header + self.m.trailer) if f.required]
        width = max(len(f.name) for f in required)
        return [f"\t\tif (message.{f.name.ljust(width)} is null) Missing(message, {f.const});" for f in required]

    def header_groups(self):
        """The count of every group of the header and trailer held to its entries, between blank lines."""
        groups = [mm for mm in self.m.header + self.m.trailer if isinstance(mm, Group)]

        if not groups:
            return [""]

        return [""] + [f"\t\tCounted(message, message.{g.counter.name}, message.{g.list});" for g in groups] + [""]

    def header_sets(self):
        header  = [f.const for f in self.flat(self.m.header)]
        trailer = [f.const for f in self.flat(self.m.trailer)]
        encoded = [self.m.tag(t) for t in sorted(self.m.pairs.values()) if self.m.field_name[t].startswith("Encoded")]

        def body(tags):
            return self.wrapped(tags, "\t\treturn tag is", "\t\t\t", width=132)

        out = ["\tstatic bool IsHeader(int tag)", "\t{"] + body(header)
        out[-1] += ";"
        out += ["\t}", "", "\tstatic bool IsEncoded(int tag)", "\t{"] + body(encoded)
        out[-1] += ";"
        out += ["\t}", "", "\tstatic bool IsTrailer(int tag)", "\t{"] + body(trailer)
        out[-1] += ";"
        out += ["\t}"]
        return out

    def template(self, name, tables):
        text = open(os.path.join(templates, name), encoding="utf-8-sig").read().replace("\r\n", "\n")
        text = (text.replace("@@Ns@@", self.v.ns).replace("@@Context@@", self.v.context).replace("@@Validator@@", self.v.validator)
                    .replace("@@BeginString@@", self.v.begin).replace("@@Title@@", self.v.title)
                    .replace("@@MessageCountWords@@", words(len(self.m.messages))).replace("@@PairCountWords@@", words(len(self.m.pairs))))

        for token, lines in tables.items():
            text = text.replace(f"@@{token}@@", "\n".join(lines))

        left = re.findall(r"@@\w+@@", text)
        if left:
            raise SystemExit(f"{name}: tokens left unwritten: {left}")

        # A template's own heading, and the reason it is not edited in the version's directory.
        lines = text.split("\n")
        at = next(k for k, l in enumerate(lines) if l.startswith("namespace "))
        lines[at + 1:at + 1] = ["", self.written("Templates/" + name)]
        return lines

    def write(self, name, lines):
        directory = os.path.join(here, self.v.ns)
        os.makedirs(directory, exist_ok=True)
        text = "\n".join(lines).replace("\r\n", "\n").replace("\n", "\r\n")
        open(os.path.join(directory, name), "wb").write(b"\xef\xbb\xbf" + text.encode("utf-8"))
        print(f"written {self.v.ns}/{name}", len(lines), "lines")

    def run(self):
        v, m = self.v, self.m

        types = ["using System.Collections.Generic;", "", f"namespace DotGram.Finance.Fix.{v.ns};", "", self.written(), "",
                 "public abstract partial class FixMessage", "{"]
        for msg in m.messages:
            types.extend(self.message_class(msg))
        types.pop()
        types += ["}", ""]

        components = ["using System;", "", "// ReSharper disable InconsistentNaming", "", f"namespace DotGram.Finance.Fix.{v.ns};", "",
                      self.written(), "//",
                      f"// The components {v.title} reuses across message types, as the shape a message or a group entry has",
                      "// when it carries one. A component is written into its carrier field by field, because that is",
                      "// what it is on the wire; the interface is those same fields read as the one thing they are, so",
                      "// that what is asked of a component is written once and asked of every carrier. A group inside a",
                      "// component is the same group wherever the component is carried, so its entry is a class nested",
                      "// in the interface: IInstrument.NoSecurityAltIDGroup.", ""]
        for i in m.interfaces.values():
            components.extend(self.interface_text(i))

        width = max(len(TAG_NAME[t]) for t in m.field_name)
        standard = [f"namespace DotGram.Finance.Fix.{v.ns};", "", self.written(), "", "static class FixStandard", "{",
                    f"\t/// <summary>The type of the value of a field {v.title} defines; <see cref=\"FixValueType.None\"/> for a tag it does not.</summary>",
                    "\tinternal static FixValueType Type(int tag)", "\t{", "\t\treturn tag switch", "\t\t{"]
        for t in sorted(m.field_name):
            standard.append(f"\t\t\tFixTag.{TAG_NAME[t].ljust(width)} => FixValueType.{m.value_type[t]},")
        standard += [f"\t\t\t{'_'.ljust(width + 7)} => FixValueType.None,", "\t\t};", "\t}", "}", ""]

        self.write("FixMessage.Types.cs", types)
        self.write("FixMessage.Header.cs", self.header_text())
        # A version whose repository names no component outside the header, FIX 4.2, has none to write.
        if m.interfaces:
            self.write("FixComponents.cs", components[:-1] + [""] if components[-1] == "" else components)
        elif os.path.exists(os.path.join(here, v.ns, "FixComponents.cs")):
            os.remove(os.path.join(here, v.ns, "FixComponents.cs"))
        self.write("FixStandard.cs", standard)
        self.write(f"{v.validator}.cs", self.validators_text())
        self.write(f"{v.validator}.Fields.cs", self.fields_text())

        self.write("FixMessage.cs", self.template("FixMessage.cs.in", {}))
        self.write("FixCustomMessage.cs", self.template("FixCustomMessage.cs.in", {}))
        self.write("FixParser.cs", self.template("FixParser.cs.in", {}))
        self.write("FixParser.Messages.cs", self.template("FixParser.Messages.cs.in", {"MessageSwitch": self.message_switch()}))
        self.write("FixParser.Streaming.cs", self.template("FixParser.Streaming.cs.in", {}))
        self.write(f"{v.context}.cs", self.template("Context.cs.in", {"Version": self.version_text()}))
        self.write(f"{v.validator}.Header.cs", self.template("Validator.Header.cs.in", {"HeaderRequired": self.header_required(), "HeaderSets": self.header_sets(), "HeaderGroups": self.header_groups()}))

        return m


def fix_tag(models):
    """FixTag: every tag of every version, named as the newest version that has it names it."""
    classes = {}
    for m in models:
        for t, cls in m.field_class.items():
            classes.setdefault(t, set()).add(cls)

    titles = ", ".join(m.v.title for m in models)
    out = ["namespace DotGram.Finance.Fix;", "",
           f"// Written by generate.py from the {titles} repository; not edited by hand.", ATTRIBUTION, "",
           "/// <summary>The number of every field of every FIX version this package reads, as a constant named for the field: <c>FixField.Decimal { Tag: FixTag.OrderQty }</c>.</summary>",
           "/// <remarks>A tag is its number, and a tag no version defines is only that: <c>25005</c>.</remarks>",
           "public static class FixTag", "{"]

    for t in sorted(TAG_NAME):
        read = " or ".join(f"<see cref=\"FixField.{c}\"/>" for c in sorted(classes.get(t, ())))
        out += [f"\t/// <summary>{TAG_NAME[t]}, a {read}.</summary>" if read else f"\t/// <summary>{TAG_NAME[t]}.</summary>",
                f"\tpublic const int {TAG_NAME[t]} = {t};", ""]

    out[-1:] = ["}", ""]
    text = "\n".join(out).replace("\n", "\r\n")
    open(os.path.join(here, "FixTag.cs"), "wb").write(b"\xef\xbb\xbf" + text.encode("utf-8"))
    print("written FixTag.cs", len(out), "lines")


if __name__ == "__main__":
    models = [Writer(Model(v)).run() for v in VERSIONS]
    fix_tag(models)
