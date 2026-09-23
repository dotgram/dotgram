# Writes quickfixn-fix44-errata.xml beside this script, from the FIX 4.4 repository beside it:
# the thirteen message types QuickFIX/n's FIX44.xml places differently from the repository,
# each written whole the way the repository has it, in that file's format. Run it again when
# the list changes (the Unplaced lines of FixLoadTests are the list); the file is not edited
# by hand.
#
#     python tests/Corpus/Fix/quickfixn-fix44-errata.py

import os
import xml.etree.ElementTree as ET
from xml.sax.saxutils import quoteattr

here = os.path.dirname(os.path.abspath(__file__))
base = os.path.join(here, "..", "FixRepository", "FIX.4.4", "Base")

def rows(name):
    for e in ET.parse(os.path.join(base, name)).getroot():
        yield {c.tag: (c.text or "").strip() for c in e}

messages   = {m["Name"]: m for m in rows("Messages.xml")}
components = {c["Name"]: c for c in rows("Components.xml")}
fields     = {f["Tag"]: f["Name"] for f in rows("Fields.xml")}

# The names QuickFIX gives the fields, where its file declares them: the errata is read after that
# file and spells a field the way it does.
for f in ET.parse(os.path.join(here, "FIX44.xml")).getroot().find("fields"):
    if f.get("number") in fields:
        fields[f.get("number")] = f.get("name")
contents   = {}

for r in rows("MsgContents.xml"):
    contents.setdefault(r["ComponentID"], []).append(r)

for cid in contents:
    contents[cid].sort(key=lambda r: float(r["Position"]))

# The message types QuickFIX/n's FIX44.xml places differently from the repository, as the load
# of that file reports them (FixLoadTests: Unplaced).
ERRATA = [
    "QuoteRequestReject", "CrossOrderCancelReplaceRequest", "AllocationInstruction", "AllocationReport",
    "SettlementInstructions", "TradeCaptureReport", "AssignmentReport", "CollateralRequest",
    "CollateralAssignment", "CollateralResponse", "CollateralReport", "CollateralInquiry", "CollateralInquiryAck",
]

def yn(reqd):
    return "Y" if reqd == "1" else "N"

# A component's members in QuickFIX's form: a field by name; a block, or a repeating component the
# repository names (Parties), as a component reference; and an implicit repeating component as the
# group QuickFIX writes, named for its counter, the entry's members inside it. `skip` is the counter
# row of a repeating component, which the group's name already says. Every component referred to is
# noted, so that its description can be written beside the messages and the file reads alone.
used = []

def member_lines(cid, indent, skip=0):
    lines = []

    for r in contents[cid][skip:]:
        text = r["TagText"]

        if text in ("StandardHeader", "StandardTrailer"):
            continue

        pad = "\t" * indent

        if text.isdigit():
            lines.append(f'{pad}<field name={quoteattr(fields[text])} required="{yn(r["Reqd"])}"/>')
            continue

        comp = components[text]

        if comp["ComponentType"] in ("Block", "BlockRepeating"):
            lines.append(f'{pad}<component name={quoteattr(text)} required="{yn(r["Reqd"])}"/>')

            if text not in used:
                used.append(text)

            continue

        lines.extend(group_lines(comp, r["Reqd"], indent))

    return lines

def group_lines(comp, reqd, indent):
    pad     = "\t" * indent
    counter = contents[comp["ComponentID"]][0]
    assert counter["TagText"].isdigit(), (comp["Name"], counter)

    return ([f'{pad}<group name={quoteattr(fields[counter["TagText"]])} required="{yn(reqd)}">'] +
            member_lines(comp["ComponentID"], indent + 1, skip=1) +
            [f'{pad}</group>'])

def component_lines(name, indent):
    comp = components[name]
    pad  = "\t" * indent

    if comp["ComponentType"] == "Block":
        inner = member_lines(comp["ComponentID"], indent + 1)
    else:
        inner = group_lines(comp, contents[comp["ComponentID"]][0]["Reqd"], indent + 1)

    return [f'{pad}<component name={quoteattr(name)}>'] + inner + [f'{pad}</component>']

out = [
    '<?xml version="1.0" encoding="utf-8"?>',
    '<!--',
    '\tWhat QuickFIX/n\'s FIX44.xml places differently from the FIX 4.4 repository, said the way the',
    '\trepository says it, in the format that file is in. Loaded after that file, it puts these thirteen',
    '\tmessage types back to the standard, read as one with it: FixContext.Default.Load([FIX44.xml, this]).',
    '',
    '\tWritten from the FIX Unified Repository (tests/Corpus/FixRepository) by the script beside this',
    '\tfile, message for message: every member of each type, the repository\'s implicit repeating',
	'\tcomponents written as the groups QuickFIX writes, its blocks and named repeating components as',
	'\tcomponent references, described after the messages. A load replaces a message type\'s whole check,',
	'\tso each type is given whole and not as a difference. Not edited by hand.',
    '-->',
    '<fix major="4" minor="4">',
    '\t<messages>',
]

for name in ERRATA:
    m = messages[name]
    out.append(f'\t\t<message name={quoteattr(name)} msgtype={quoteattr(m["MsgType"])} msgcat="app">')
    out.extend(member_lines(m["ComponentID"], 3))
    out.append('\t\t</message>')

out.append('\t</messages>')
out.append('\t<components>')

at = 0

while at < len(used):
    out.extend(component_lines(used[at], 2))
    at += 1

out += ['\t</components>', '</fix>', '']

path = os.path.join(here, "quickfixn-fix44-errata.xml")
open(path, "wb").write("\r\n".join(out).encode("utf-8"))
print("written", path, len(out), "lines")
