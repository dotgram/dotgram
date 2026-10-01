// Writes every version of FIX this package reads from the FIX repository, and FixTag from all of them:
//
//     dotnet run src/DotGram.Finance/Fix/generate.cs
//
// Each version is a directory of its own, Fix/Fix44 for FIX 4.4, and everything in it is written here:
//
//     FixMessage.Types.cs          a class a message type, its fields read in one switch
//     FixMessage.Header.cs         the standard header and trailer, read and held by every message
//     FixComponents.cs             an interface a component, its repeating groups nested in it
//     FixStandard.cs               the type of the value of every field
//     FixValidator<NN>.cs          the check of every message type, component and group entry
//     FixValidator<NN>.Fields.cs   the check of every field against its type and its code set
//     and from Templates/, with the version's names and the tables the repository gives put in:
//     FixMessage.cs, FixCustomMessage.cs, FixParser.cs, FixParser.Messages.cs, FixParser.Streaming.cs,
//     Fix<NN>Context.cs, FixValidator<NN>.Header.cs
//
// and one file that every version shares, FixTag.cs: the number of every tag of every version, named
// as the newest version that has the tag names it.
//
// The repository is tests/Corpus/FixRepository. A field is named as the repository names it, or as
// the common data dictionaries do where the two differ; a field is a class of the type of its value,
// FixField.Decimal, and its tag says which field it is. Groups are named for their counter: a group is
// the class <Counter>Group, nested in whatever carries it — a message, a component's interface, or
// the entry of another group — and its entries are the list <Counter>Groups beside the counter. So the
// names a data dictionary uses are the names of the code, and a dictionary loaded at run time is
// written into checks by putting its names in place.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Row = System.Collections.Generic.Dictionary<string, string>;

var scriptPath = Fx.ScriptPath();
var here       = Path.GetDirectoryName(scriptPath)!;
var root       = Path.GetFullPath(Path.Combine(here, "..", "..", ".."));
var corpus     = Path.Combine(root, "tests", "Corpus", "FixRepository");
var templates  = Path.Combine(here, "Templates");

try
{
	var tagName = Fx.TagNames(corpus);
	var models  = new List<Model>();

	foreach (var v in Fx.Versions)
	{
		var model  = new Model(v, corpus, tagName);
		var writer = new Writer(model, templates, here);

		writer.Run();
		models.Add(model);
	}

	Fx.WriteFixTag(models, tagName, here);
}
catch (GeneratorException ex)
{
	Console.Error.WriteLine(ex.Message);
	Environment.Exit(1);
}

// ── the versions ───────────────────────────────────────────────────────────────────────────────

sealed class Version
{
	public Version(string key, string directory, string ns, string title, string begin, string? session = null,
		Dictionary<int, string>? fieldNames = null, Dictionary<string, string>? messageNames = null, Dictionary<string, string>? classNames = null)
	{
		Key         = key;
		Directory   = directory;
		Ns          = ns;
		Title       = title;
		Begin       = begin;
		Session     = session;
		FieldNames  = fieldNames ?? new Dictionary<int, string>();
		MessageNames = messageNames ?? new Dictionary<string, string>();
		ClassNames  = classNames ?? new Dictionary<string, string>();
	}

	public string Key          { get; }
	public string Directory    { get; }
	public string Ns           { get; }
	public string Title        { get; }
	public string Begin        { get; }
	public string? Session     { get; }
	public Dictionary<int, string>    FieldNames   { get; }
	public Dictionary<string, string> MessageNames { get; }
	public Dictionary<string, string> ClassNames   { get; }

	public string Number    => Ns.Substring(3);
	public string Context   => "Fix" + Number + "Context";
	public string Validator => "FixValidator" + Number;

	public List<string> Directories()
	{
		return Session is null ? new List<string> { Directory } : new List<string> { Directory, Session };
	}
}

// ── the members a carrier holds ───────────────────────────────────────────────────────────────

abstract class Member
{
}

sealed class Field : Member
{
	public Field(Model model, int tag, bool required)
	{
		Model    = model;
		Tag      = tag;
		Required = required;
		Name     = model.FieldName[tag];
	}

	public Model Model    { get; }
	public int   Tag      { get; }
	public bool  Required { get; }
	public string Name    { get; }

	public string Cls      => Model.FieldClass[Tag];
	public string TypeName => "FixField." + Cls;
	public string Const    => Model.TagConst(Tag);
	public string Slot     => Model.FieldSlot[Tag];
}

sealed class Block : Member
{
	public Block(string name, bool required)
	{
		Name     = name;
		Required = required;
	}

	public string Name     { get; }
	public bool   Required { get; }
}

sealed class Group : Member
{
	public Group(Field counter, bool required, Entry entry)
	{
		Counter  = counter;
		Required = required;
		Entry    = entry;
	}

	public Field Counter  { get; }
	public bool  Required { get; }
	public Entry Entry    { get; }

	public string List => Counter.Name + "Groups";
}

sealed class Entry
{
	public Entry(Model model, string name, string typeName, string slot, List<Member> members)
	{
		Model    = model;
		Name     = name;
		TypeName = typeName;
		Slot     = slot;
		Members  = members;
	}

	public Model        Model    { get; }
	public string       Name     { get; }
	public string       TypeName { get; }
	public string       Slot     { get; }
	public List<Member> Members  { get; set; }

	public Field Opener => Fx.FirstField(Model, Members);
}

sealed class Interface
{
	public Interface(Model model, string name, List<Member> members)
	{
		Model   = model;
		Name    = name;
		Members = members;
	}

	public Model        Model   { get; }
	public string       Name    { get; }
	public List<Member> Members { get; set; }

	public string TypeName => "I" + Name;

	public List<string> Bases
	{
		get
		{
			var bases = new List<string>();

			foreach (var member in Members)
				if (member is Block block)
					bases.Add(block.Name);

			return bases;
		}
	}
}

sealed class MessageInfo
{
	public MessageInfo(string name, string msgType, string directory, string componentId)
	{
		Name        = name;
		MsgType     = msgType;
		Directory   = directory;
		ComponentId = componentId;
	}

	public string Name        { get; }
	public string MsgType     { get; }
	public string Directory   { get; }
	public string ComponentId { get; }
}

sealed class GeneratorException : Exception
{
	public GeneratorException(string message) : base(message)
	{
	}
}

// ── one version ────────────────────────────────────────────────────────────────────────────────

sealed class Model
{
	public Version V { get; }

	public Dictionary<int, Row>         Fields     { get; } = new();
	public Dictionary<int, string>      FieldType  { get; } = new();
	public Dictionary<int, string>      FieldName  { get; } = new();
	public Dictionary<int, List<string>> Enums     { get; } = new();
	public Dictionary<int, string>      ValueType  { get; } = new();
	public Dictionary<int, string>      FieldClass { get; } = new();
	public Dictionary<int, int>         Pairs      { get; } = new();

	public Dictionary<string, Row>                         Components  { get; } = new();
	public Dictionary<(string Directory, string ComponentId), List<Row>> Contents { get; } = new();
	public Dictionary<string, string>                      ComponentDir { get; } = new();

	public List<MessageInfo> Messages { get; } = new();

	public Dictionary<string, Interface> InterfacesByName { get; } = new();
	public List<Interface>               InterfacesOrdered { get; } = new();

	public Dictionary<string, List<Member>> MessageMembers { get; } = new();

	public Dictionary<int, string> FieldSlot { get; } = new();

	public List<Member> Header  { get; }
	public List<Member> Trailer { get; }

	readonly Dictionary<int, string> _tagName;
	readonly string _corpus;

	public Model(Version v, string corpus, Dictionary<int, string> tagName)
	{
		V        = v;
		_corpus  = corpus;
		_tagName = tagName;

		var directories = v.Directories();

		foreach (var d in directories)
			foreach (var f in Fx.ReadRows(corpus, d, "Fields.xml"))
				Fields[int.Parse(f["Tag"], CultureInfo.InvariantCulture)] = f;

		foreach (var kv in Fields)
		{
			FieldType[kv.Key] = kv.Value["Type"];
			FieldName[kv.Key] = v.FieldNames.TryGetValue(kv.Key, out var named) ? named : kv.Value["Name"];
		}

		foreach (var d in directories)
		{
			foreach (var e in Fx.ReadRows(corpus, d, "Enums.xml"))
			{
				var tag = int.Parse(e["Tag"], CultureInfo.InvariantCulture);

				if (!Fields.ContainsKey(tag))
					continue;

				if (!Enums.TryGetValue(tag, out var list))
				{
					list = new List<string>();
					Enums[tag] = list;
				}

				if (!list.Contains(e["Value"]))
					list.Add(e["Value"]);
			}
		}

		var unknown = FieldType.Values.Where(t => !Fx.ValueClass.ContainsKey(t)).Distinct().OrderBy(t => t, StringComparer.Ordinal).ToList();

		if (unknown.Count > 0)
			throw new GeneratorException($"{v.Title}: no value class for the types [{string.Join(", ", unknown.Select(u => "'" + u + "'"))}]");

		// A field declared char whose code set publishes values a single character cannot hold is read as
		// text: it keeps every value the document prints, and which of them are allowed is the code set's
		// answer rather than the type's shape.
		foreach (var kv in FieldType)
		{
			var cls = Fx.ValueClass[kv.Value];

			if (cls == "Character" && Enums.TryGetValue(kv.Key, out var codes) && codes.Any(c => c.Length != 1))
				cls = "Text";

			ValueType[kv.Key] = cls;
		}

		foreach (var kv in ValueType)
			FieldClass[kv.Key] = Fx.ClassAlias.TryGetValue(kv.Value, out var alias) ? alias : kv.Value;

		// The length/data pairs: a Length field that names the data it measures, or, where the
		// repository leaves that out (three fields of FIX 5.0 SP2), the Length field named for the data.
		var byName = new Dictionary<string, int>();

		foreach (var kv in Fields)
			byName[kv.Value["Name"]] = kv.Key;

		foreach (var kv in Fields)
		{
			var t = kv.Key;
			var f = kv.Value;

			if (f.TryGetValue("AssociatedDataTag", out var data) && !string.IsNullOrEmpty(data) && f["Type"] == "Length" &&
				FieldType.TryGetValue(int.Parse(data, CultureInfo.InvariantCulture), out var dataType) && (dataType == "data" || dataType == "XMLData"))
			{
				Pairs[t] = int.Parse(data, CultureInfo.InvariantCulture);
			}
		}

		foreach (var kv in Fields)
		{
			var t = kv.Key;
			var f = kv.Value;

			if ((f["Type"] == "data" || f["Type"] == "XMLData") && !Pairs.Values.Contains(t))
			{
				int? length = null;

				if (byName.TryGetValue(f["Name"] + "Len", out var l1))
					length = l1;
				else if (byName.TryGetValue(f["Name"] + "Length", out var l2))
					length = l2;

				if (length is int lengthTag && Fields[lengthTag]["Type"] == "Length")
					Pairs[lengthTag] = t;
			}
		}

		foreach (var d in directories)
			foreach (var r in Fx.ReadRows(corpus, d, "MsgContents.xml"))
			{
				var key = (d, r["ComponentID"]);

				if (!Contents.TryGetValue(key, out var list))
				{
					list = new List<Row>();
					Contents[key] = list;
				}

				list.Add(r);
			}

		// A later directory's component wins where it has rows: FIXT 1.1 declares MsgTypeGrp and lists
		// none of its members, which the application's repository does.
		foreach (var d in directories)
		{
			foreach (var c in Fx.ReadRows(corpus, d, "Components.xml"))
			{
				var key = (d, c["ComponentID"]);
				var hasContents = Contents.TryGetValue(key, out var list) && list.Count > 0;

				if (hasContents || !ComponentDir.ContainsKey(c["Name"]))
				{
					ComponentDir[c["Name"]] = d;
					Components[c["Name"]]   = c;
				}
			}
		}

		foreach (var key in Contents.Keys.ToList())
			Contents[key] = Contents[key].OrderBy(r => double.Parse(r["Position"], CultureInfo.InvariantCulture)).ToList();

		var messages = new Dictionary<string, MessageInfo>();

		foreach (var d in directories)
		{
			foreach (var m in Fx.ReadRows(corpus, d, "Messages.xml"))
			{
				var named = v.MessageNames.TryGetValue(m["Name"], out var mn) ? mn : m["Name"];
				var name  = v.ClassNames.TryGetValue(named, out var cn) ? cn : named;

				messages[m["MsgType"]] = new MessageInfo(name, m["MsgType"], d, m["ComponentID"]);
			}
		}

		Messages = messages.Values.OrderBy(m => m.Name, StringComparer.Ordinal).ToList();

		foreach (var name in Components.Keys.OrderBy(n => n, StringComparer.Ordinal))
		{
			var component = Components[name];

			if (name == "StandardHeader" || name == "StandardTrailer")
				continue;

			var componentType = component["ComponentType"];

			if (componentType != "Block" && componentType != "BlockRepeating")
				continue;

			var iface = new Interface(this, name, new List<Member>());

			InterfacesByName[name] = iface;
			InterfacesOrdered.Add(iface);
		}

		foreach (var iface in InterfacesOrdered)
			iface.Members = MembersOf(ComponentRows(iface.Name), "I" + iface.Name, iface.Name);

		foreach (var msg in Messages)
			MessageMembers[msg.Name] = MembersOf(Contents[(msg.Directory, msg.ComponentId)], "FixMessage." + msg.Name, msg.Name);

		var taken = new HashSet<string>(Messages.Select(m => m.Name));

		foreach (var name in InterfacesByName.Keys)
			taken.Add(name);

		foreach (var kv in FieldName)
			FieldSlot[kv.Key] = taken.Contains(kv.Value) ? kv.Value + "Field" : kv.Value;

		Header  = MembersOf(ComponentRows("StandardHeader"), "FixMessage", "StandardHeader", header: true);
		Trailer = MembersOf(ComponentRows("StandardTrailer"), "FixMessage", "StandardTrailer", header: true);
	}

	public List<Row> ComponentRows(string name)
	{
		var component = Components[name];

		return Contents[(ComponentDir[name], component["ComponentID"])];
	}

	public string TagConst(int t)
	{
		return "FixTag." + _tagName[t];
	}

	public string TagNameOf(int t)
	{
		return _tagName[t];
	}

	public List<Member> MembersOf(List<Row> componentRows, string ownerType, string ownerSlot, bool header = false)
	{
		var found = new List<Member>();
		var at    = 0;

		while (at < componentRows.Count)
		{
			var r        = componentRows[at];
			var text     = r["TagText"];
			var required = r["Reqd"] == "1";
			var indent   = int.Parse(r["Indent"], CultureInfo.InvariantCulture);

			if (text == "StandardHeader" || text == "StandardTrailer")
			{
				at += 1;
				continue;
			}

			if (text.Length > 0 && text.All(char.IsDigit))
			{
				var end = at + 1;

				while (end < componentRows.Count && int.Parse(componentRows[end]["Indent"], CultureInfo.InvariantCulture) > indent)
					end += 1;

				if (end > at + 1)
					found.Add(GroupOf(int.Parse(text, CultureInfo.InvariantCulture), required, componentRows.GetRange(at + 1, end - (at + 1)), ownerType, ownerSlot));
				else
					found.Add(new Field(this, int.Parse(text, CultureInfo.InvariantCulture), required));

				at = end;
				continue;
			}

			var component = Components[text];
			var kind      = component["ComponentType"];

			if ((kind == "Block" || kind == "BlockRepeating") && !header)
			{
				found.Add(new Block(text, required));
			}
			else if (kind == "ImplicitBlockRepeating" || kind == "OptimisedImplicitBlockRepeating")
			{
				var inner = ComponentRows(text);

				found.Add(GroupOf(int.Parse(inner[0]["TagText"], CultureInfo.InvariantCulture), required, inner.GetRange(1, inner.Count - 1), ownerType, ownerSlot));
			}
			else
			{
				found.AddRange(MembersOf(ComponentRows(text), ownerType, ownerSlot, header));
			}

			at += 1;
		}

		return found;
	}

	public Group GroupOf(int counterTag, bool required, List<Row> entryRows, string ownerType, string ownerSlot)
	{
		var counter = new Field(this, counterTag, required);
		var name    = counter.Name + "Group";
		var typeName = ownerType + "." + name;
		var slot    = ownerSlot + "_" + counter.Name;
		var entry   = new Entry(this, name, typeName, slot, new List<Member>());

		entry.Members = MembersOf(entryRows, typeName, slot);

		return new Group(counter, required, entry);
	}
}

// ── module-level helpers ──────────────────────────────────────────────────────────────────────

static class Fx
{
	// The repository's notice lets portions of the specification be extracted into other work provided the
	// origin is referenced and the specification is said to be Copyright FIX Protocol Limited; every table
	// written here is such a portion. It says where the data came from and changes nothing about the
	// licence of this code, which is the repository's own.
	public const string Attribution = "// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.";

	public static readonly List<Version> Versions = new()
	{
		new Version("4.2", "FIX.4.2", "Fix42", "FIX 4.2", "FIX.4.2",
			messageNames: new Dictionary<string, string>
			{
				["IOI"]                       = "IndicationofInterest",
				["OrderSingle"]                = "NewOrderSingle",
				["OrderList"]                  = "NewOrderList",
				["AllocationInstructionAck"]   = "AllocationACK",
			}),
		new Version("4.4", "FIX.4.4", "Fix44", "FIX 4.4", "FIX.4.4",
			fieldNames: new Dictionary<int, string> { [23] = "IOIid", [33] = "LinesOfText" },
			messageNames: new Dictionary<string, string>
			{
				["IOI"]                                     = "IndicationOfInterest",
				["MultilegOrderCancelReplace"]              = "MultilegOrderCancelReplaceRequest",
				["NetworkCounterpartySystemStatusRequest"]  = "NetworkStatusRequest",
				["NetworkCounterpartySystemStatusResponse"] = "NetworkStatusResponse",
			}),
		new Version("5.0", "FIX.5.0SP2", "Fix50", "FIX 5.0 SP2", "FIXT.1.1", session: "FIXT.1.1",
			fieldNames: new Dictionary<int, string> { [327] = "HaltReasonInt" },
			classNames: new Dictionary<string, string> { ["SecurityStatus"] = "SecurityStatusMessage" }),
	};

	// How a field's value is read, by the type the repository gives the field: the reader, which is also
	// the class the value is held in except where CLASS names another.
	public static readonly Dictionary<string, string> ValueClass = new()
	{
		["String"] = "Text", ["Currency"] = "Text", ["Exchange"] = "Text", ["Country"] = "Text", ["Language"] = "Text",
		["char"] = "Character",
		["Boolean"] = "Boolean",
		["int"] = "Integer", ["Length"] = "Integer", ["SeqNum"] = "Integer", ["NumInGroup"] = "Integer", ["DayOfMonth"] = "Integer",
		["float"] = "Decimal", ["Qty"] = "Decimal", ["Price"] = "Decimal", ["PriceOffset"] = "Decimal", ["Amt"] = "Decimal", ["Percentage"] = "Decimal",
		["UTCTimestamp"] = "Timestamp", ["TZTimestamp"] = "ZonedTimestamp",
		["UTCTimeOnly"] = "Time", ["TZTimeOnly"] = "ZonedTime",
		["UTCDateOnly"] = "Date", ["UTCDate"] = "Date", ["LocalMktDate"] = "Date",
		["MonthYear"] = "MonthYear",
		["MultipleValueString"] = "Multiple", ["MultipleCharValue"] = "Multiple", ["MultipleStringValue"] = "MultipleString",
		["data"] = "Data", ["XMLData"] = "Data",
	};

	// A zoned timestamp is read by a reader of its own and held as any other instant is.
	public static readonly Dictionary<string, string> ClassAlias = new()
	{
		["ZonedTimestamp"] = "Timestamp",
		["MultipleString"] = "Multiple",
	};

	public static string ScriptPath([CallerFilePath] string path = "")
	{
		return path;
	}

	public static IEnumerable<Row> ReadRows(string corpus, string directory, string name)
	{
		var document = XDocument.Load(Path.Combine(corpus, directory, "Base", name));

		foreach (var element in document.Root!.Elements())
		{
			var row = new Row();

			foreach (var child in element.Elements())
				row[child.Name.LocalName] = child.Value.Trim();

			yield return row;
		}
	}

	public static string Words(int n)
	{
		var ones = "zero one two three four five six seven eight nine ten eleven twelve thirteen fourteen fifteen sixteen seventeen eighteen nineteen".Split(' ');
		var tens = "_ _ twenty thirty forty fifty sixty seventy eighty ninety".Split(' ');

		if (n < 20)
			return ones[n];

		if (n < 100)
			return tens[n / 10] + (n % 10 == 0 ? "" : "-" + ones[n % 10]);

		return n.ToString(CultureInfo.InvariantCulture);
	}

	/// <summary>Every tag of every version, named as the newest version that has it names it.</summary>
	public static Dictionary<int, string> TagNames(string corpus)
	{
		var names = new Dictionary<int, string>();

		foreach (var v in Versions)
			foreach (var d in v.Directories())
				foreach (var f in ReadRows(corpus, d, "Fields.xml"))
					names[int.Parse(f["Tag"], CultureInfo.InvariantCulture)] = f["Name"];

		return names;
	}

	public static Field FirstField(Model model, List<Member> members)
	{
		var head = members[0];

		if (head is Field field)
			return field;

		if (head is Block block)
			return FirstField(model, model.InterfacesByName[block.Name].Members);

		return ((Group)head).Counter;
	}

	/// <summary>Every property a carrier of these members declares, its blocks' included: (kind, member).</summary>
	public static IEnumerable<(string Kind, object Member)> PropertiesOf(Model model, List<Member> members)
	{
		foreach (var m in members)
		{
			if (m is Field field)
			{
				yield return ("field", field);
			}
			else if (m is Group group)
			{
				yield return ("field", group.Counter);
				yield return ("list", group);
			}
			else
			{
				var block = (Block)m;

				foreach (var item in PropertiesOf(model, model.InterfacesByName[block.Name].Members))
					yield return item;
			}
		}
	}

	/// <summary>The interfaces a carrier implements: every block it carries, and every block those carry.</summary>
	public static List<string> BlocksOf(Model model, List<Member> members)
	{
		var found = new List<string>();

		foreach (var m in members)
		{
			if (m is Block block)
			{
				found.Add(block.Name);
				found.AddRange(BlocksOf(model, model.InterfacesByName[block.Name].Members));
			}
		}

		return found.Distinct().OrderBy(n => n, StringComparer.Ordinal).ToList();
	}

	/// <summary>The groups a carrier nests: its own, and not those of the blocks it carries.</summary>
	public static List<Group> EntriesIn(List<Member> members)
	{
		return members.OfType<Group>().ToList();
	}

	public static IEnumerable<Entry> AllEntries(List<Member> members)
	{
		foreach (var g in EntriesIn(members))
		{
			yield return g.Entry;

			foreach (var e in AllEntries(g.Entry.Members))
				yield return e;
		}
	}

	public static string ExpandTabs(string s, int tabSize)
	{
		var sb     = new StringBuilder();
		var column = 0;

		foreach (var ch in s)
		{
			if (ch == '\t')
			{
				var spaces = tabSize - (column % tabSize);

				sb.Append(' ', spaces);
				column += spaces;
			}
			else
			{
				sb.Append(ch);
				column += 1;

				if (ch == '\n')
					column = 0;
			}
		}

		return sb.ToString();
	}

	public static void WriteFixTag(List<Model> models, Dictionary<int, string> tagName, string here)
	{
		var classes = new Dictionary<int, HashSet<string>>();

		foreach (var m in models)
		{
			foreach (var kv in m.FieldClass)
			{
				if (!classes.TryGetValue(kv.Key, out var set))
				{
					set = new HashSet<string>();
					classes[kv.Key] = set;
				}

				set.Add(kv.Value);
			}
		}

		var titles = string.Join(", ", models.Select(m => m.V.Title));
		var outLines = new List<string>
		{
			"namespace DotGram.Finance.Fix;",
			"",
			$"// Written by generate.py from the {titles} repository; not edited by hand.",
			Attribution,
			"",
			"/// <summary>The number of every field of every FIX version this package reads, as a constant named for the field: <c>FixField.Decimal { Tag: FixTag.OrderQty }</c>.</summary>",
			"/// <remarks>A tag is its number, and a tag no version defines is only that: <c>25005</c>.</remarks>",
			"public static class FixTag",
			"{",
		};

		foreach (var t in tagName.Keys.OrderBy(t => t))
		{
			var read = classes.TryGetValue(t, out var set)
				? string.Join(" or ", set.OrderBy(c => c, StringComparer.Ordinal).Select(c => $"<see cref=\"FixField.{c}\"/>"))
				: "";

			outLines.Add(read.Length > 0 ? $"\t/// <summary>{tagName[t]}, a {read}.</summary>" : $"\t/// <summary>{tagName[t]}.</summary>");
			outLines.Add($"\tpublic const int {tagName[t]} = {t};");
			outLines.Add("");
		}

		outLines.RemoveAt(outLines.Count - 1);
		outLines.Add("}");
		outLines.Add("");

		var text = string.Join("\n", outLines).Replace("\n", "\r\n");
		var bytes = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.UTF8.GetBytes(text)).ToArray();

		File.WriteAllBytes(Path.Combine(here, "FixTag.cs"), bytes);
		Console.WriteLine($"written FixTag.cs {outLines.Count} lines");
	}
}

// ── writing ────────────────────────────────────────────────────────────────────────────────────

sealed class Writer
{
	readonly Model  _m;
	readonly Version _v;
	readonly string _templates;
	readonly string _here;

	public Writer(Model model, string templates, string here)
	{
		_m         = model;
		_v         = model.V;
		_templates = templates;
		_here      = here;
	}

	/// <summary>The heading every file written here carries: what wrote it, and where its tables come from,
	/// as the repository's notice asks of anything extracted from the specification.</summary>
	public string Written(string? source = null)
	{
		var made = $"// Written by generate.py from the {_v.Title} repository; not edited by hand.";

		return made + (source != null ? $" From {source}." : "") + "\n" + Fx.Attribution;
	}

	public string Wire(int tag)
	{
		return _m.FieldType.TryGetValue(tag, out var type) ? type : "String";
	}

	public string FieldDoc(Field f)
	{
		return $"The FIX {f.Name}, tag {f.Tag}, wire type <c>{Wire(f.Tag)}</c>; null when the field is absent.";
	}

	public static string ListDoc(Group g)
	{
		return $"The entries counted by {g.Counter.Name}, tag {g.Counter.Tag}; null when the group is absent.";
	}

	/// <summary>The properties of a carrier class, and the classes of the groups it nests.</summary>
	public List<string> ClassBody(List<Member> members, int indent, string setter, Field? opener = null, Func<Field, string>? doc = null)
	{
		var pad = new string('\t', indent);
		var outLines = new List<string>();
		var docFunc = doc ?? FieldDoc;

		foreach (var (kind, m) in Fx.PropertiesOf(_m, members))
		{
			if (kind == "field")
			{
				var field = (Field)m;

				outLines.Add($"{pad}/// <summary>{docFunc(field)}</summary>");

				if (opener != null && field.Tag == opener.Tag)
					outLines.Add($"{pad}public required {field.TypeName} {field.Name} {{ get; init; }}");
				else
					outLines.Add($"{pad}public {field.TypeName}? {field.Name} {{ get; {setter}; }}");
			}
			else
			{
				var group = (Group)m;

				outLines.Add($"{pad}/// <summary>{ListDoc(group)}</summary>");
				outLines.Add($"{pad}public List<{group.Entry.TypeName}>? {group.List} {{ get; {setter}; }}");
			}

			outLines.Add("");
		}

		foreach (var g in Fx.EntriesIn(members))
			outLines.AddRange(EntryClass(g, indent));

		return outLines;
	}

	public List<string> EntryClass(Group g, int indent)
	{
		var pad = new string('\t', indent);
		var e   = g.Entry;
		var implements = Fx.BlocksOf(_m, e.Members);
		var head = $"{pad}public sealed class {e.Name}" + (implements.Count > 0 ? " : " + string.Join(", ", implements.Select(b => "I" + b)) : "");
		var outLines = new List<string>
		{
			$"{pad}/// <summary>One entry of the group counted by {g.Counter.Name}, tag {g.Counter.Tag}.</summary>",
			head,
			pad + "{",
		};

		outLines.AddRange(ClassBody(e.Members, indent + 1, "internal set", e.Opener));

		while (outLines[^1] == "")
			outLines.RemoveAt(outLines.Count - 1);

		outLines.Add(pad + "}");
		outLines.Add("");

		return outLines;
	}

	// The reading: one switch over the tags of a type, each arm placing its field where it belongs —
	// on the message, or on the last entry of the group it is in.

	public List<string> Arms(List<Member> members, string holder, List<string> lists, string index, string pad = "\t\t\t\t\t")
	{
		var outLines = new List<string>();

		string Failing()
		{
			var parts = new List<string>();

			for (var i = 0; i < lists.Count; i++)
			{
				var l = lists[i];

				parts.Add(i == 0 ? $"{l} is null || {l}.Count == 0" : $"{l} is null || {l}!.Count == 0");
			}

			return string.Join(" || ", parts);
		}

		void Place(Field f)
		{
			if (lists.Count == 0)
			{
				outLines.Add($"{pad}case {f.Const}: if ({f.Name} is not null) AddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, -1)); {f.Name} = ({f.TypeName})field; break;");
				return;
			}

			outLines.Add($"{pad}case {f.Const}:");
			outLines.Add($"{pad}\tif ({Failing()})");
			outLines.Add($"{pad}\t\tAddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));");
			outLines.Add($"{pad}\telse if ({holder}{f.Name} is not null)");
			outLines.Add($"{pad}\t\tAddFinding(new FixFinding(FixRule.DuplicateField, field.Tag, field.Position, field, {index}));");
			outLines.Add($"{pad}\telse");
			outLines.Add($"{pad}\t\t{holder}{f.Name} = ({f.TypeName})field;");
			outLines.Add($"{pad}\tbreak;");
		}

		void Walk(List<Member> walkMembers, int? skip)
		{
			foreach (var m in walkMembers)
			{
				if (m is Field field)
				{
					if (field.Tag != skip)
						Place(field);
				}
				else if (m is Block block)
				{
					Walk(_m.InterfacesByName[block.Name].Members, skip);
				}
				else
				{
					var group = (Group)m;

					if (group.Counter.Tag != skip)
						Place(group.Counter);

					var opener = group.Entry.Opener;
					var target = $"{holder}{group.List}";

					// The first entry of a group whose counter has not been read is entries nothing counts:
					// said once, at the entry, by the counter's tag.
					var uncounted = $"if ({target} is null && {holder}{group.Counter.Name} is null) AddFinding(new FixFinding(FixRule.GroupCountMismatch, {group.Counter.Const}, field.Position, field, -1));";
					var add       = $"({target} ??= []).Add(new () {{ {opener.Name} = ({opener.TypeName})field }});";

					if (lists.Count == 0)
					{
						outLines.Add($"{pad}case {opener.Const}:");
						outLines.Add($"{pad}\t{uncounted}");
						outLines.Add($"{pad}\t{add}");
						outLines.Add($"{pad}\tbreak;");
					}
					else
					{
						outLines.Add($"{pad}case {opener.Const}:");
						outLines.Add($"{pad}\tif ({Failing()})");
						outLines.Add($"{pad}\t\tAddFinding(new FixFinding(FixRule.GroupCountMismatch, field.Tag, field.Position, field, -1));");
						outLines.Add($"{pad}\telse");
						outLines.Add($"{pad}\t{{");
						outLines.Add($"{pad}\t\t{uncounted}");
						outLines.Add($"{pad}\t\t{add}");
						outLines.Add($"{pad}\t}}");
						outLines.Add($"{pad}\tbreak;");
					}

					var nextLists = new List<string>(lists) { target };

					outLines.AddRange(Arms(group.Entry.Members, $"{target}![^1].", nextLists, $"{target}!.Count - 1", pad));
				}
			}
		}

		Walk(members, lists.Count == 0 ? (int?)null : Fx.FirstField(_m, members).Tag);

		return outLines;
	}

	public List<string> MessageClass(MessageInfo msg)
	{
		var members = _m.MessageMembers[msg.Name];
		var implements = Fx.BlocksOf(_m, members);
		var head = $"\tpublic sealed partial class {msg.Name} : FixMessage" + string.Concat(implements.Select(b => ", I" + b));
		var outLines = new List<string>
		{
			$"\t/// <summary>{_v.Title} {msg.Name}, MsgType {msg.MsgType}.</summary>",
			head,
			"\t{",
			$"\t\tinternal {msg.Name}(List<FixField> fields) : base(\"{msg.MsgType}\", fields)",
			"\t\t{",
			"\t\t\tforeach (var field in fields)",
			"\t\t\t{",
			"\t\t\t\tswitch (field.Tag)",
			"\t\t\t\t{",
		};

		outLines.AddRange(Arms(members, "", new List<string>(), "-1"));
		outLines.AddRange(new[]
		{
			"",
			"\t\t\t\t\tdefault:",
			"\t\t\t\t\t\tif (!SetStandardField(field))",
			"\t\t\t\t\t\t\tAddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));",
			"",
			"\t\t\t\t\t\tbreak;",
			"\t\t\t\t}",
			"\t\t\t}",
			"\t\t}",
			"",
		});

		outLines.AddRange(ClassBody(members, 2, "internal set"));
		outLines.AddRange(new[]
		{
			"\t\t/// <summary>Asks the context for the check of this type and runs it.</summary>",
			$"\t\tprivate protected override void Check({_v.Context} context)",
			"\t\t{",
			$"\t\t\tcontext.Validators.{msg.Name}(context, this);",
			"\t\t}",
			"\t}",
			"",
		});

		return outLines;
	}

	public List<string> InterfaceText(Interface i)
	{
		var bases = i.Bases;
		var head  = $"public interface {i.TypeName}" + (bases.Count > 0 ? " : " + string.Join(", ", bases.Select(b => "I" + b)) : "");
		var kind  = _m.Components[i.Name]["ComponentType"] == "BlockRepeating" ? "repeating component" : "block";
		var outLines = new List<string>
		{
			$"/// <summary>The {_v.Title} {i.Name} {kind}, wherever it is carried.</summary>",
			head,
			"{",
		};

		foreach (var mm in i.Members)
		{
			if (mm is Field field)
			{
				outLines.Add($"\t/// <summary>The FIX {field.Name}, tag {field.Tag}.</summary>");
				outLines.Add($"\t{field.TypeName}? {field.Name} {{ get; }}");
				outLines.Add("");
			}
			else if (mm is Group group)
			{
				outLines.Add($"\t/// <summary>The FIX {group.Counter.Name}, tag {group.Counter.Tag}.</summary>");
				outLines.Add($"\t{group.Counter.TypeName}? {group.Counter.Name} {{ get; }}");
				outLines.Add("");
				outLines.Add($"\t/// <summary>{ListDoc(group)}</summary>");
				outLines.Add($"\tList<{group.Entry.TypeName}>? {group.List} {{ get; }}");
				outLines.Add("");
			}
		}

		foreach (var g in Fx.EntriesIn(i.Members))
			outLines.AddRange(EntryClass(g, 1));

		while (outLines[^1] == "")
			outLines.RemoveAt(outLines.Count - 1);

		outLines.Add("}");
		outLines.Add("");

		return outLines;
	}

	// The checks: what the repository requires of a carrier, each block it holds, each field handed to
	// its slot, each group's count held to its entries and each entry to its own check — in the order
	// the repository lists them, which is the order a dictionary loaded at run time is written in.

	public List<string> CheckBody(List<Member> members, string subject, Field? entryOpener = null)
	{
		var outLines = new List<string>();

		foreach (var mm in members)
		{
			if (mm is Field field)
			{
				FieldCheck(field, outLines, subject, entryOpener);
			}
			else if (mm is Block block)
			{
				var first = Fx.FirstField(_m, _m.InterfacesByName[block.Name].Members);

				if (block.Required)
				{
					outLines.Add($"\t\tif (Empty((I{block.Name}){subject})) Absent(message, {first.Const});");
					outLines.Add($"\t\telse context.Validators.{block.Name}(context, message, {subject});");
				}
				else
				{
					outLines.Add($"\t\tif (!Empty((I{block.Name}){subject})) context.Validators.{block.Name}(context, message, {subject});");
				}
			}
			else
			{
				var group = (Group)mm;

				FieldCheck(group.Counter, outLines, subject, entryOpener);
				outLines.Add($"\t\tCounted(message, {subject}.{group.Counter.Name}, {subject}.{group.List});");
				outLines.Add($"\t\tif ({subject}.{group.List} is not null)");
				outLines.Add($"\t\t\tfor (var i = 0; i < {subject}.{group.List}.Count; i++)");
				outLines.Add($"\t\t\t\tcontext.Validators.{group.Entry.Slot}(context, message, {subject}.{group.List}[i], i);");
			}
		}

		return outLines;
	}

	public static void FieldCheck(Field f, List<string> outLines, string subject, Field? entryOpener)
	{
		var call = $"context.Validators.{f.Slot}(context, message, {subject}.{f.Name});";

		if (entryOpener != null && f.Tag == entryOpener.Tag)
		{
			outLines.Add($"\t\t{call}");
		}
		else if (f.Required)
		{
			var where = entryOpener != null ? $", {subject}.{entryOpener.Name}.Position, index" : "";

			outLines.Add($"\t\tif ({subject}.{f.Name} is null) Missing(message, {f.Const}{where});");
			outLines.Add($"\t\telse {call}");
		}
		else
		{
			outLines.Add($"\t\tif ({subject}.{f.Name} is not null) {call}");
		}
	}

	public List<string> ValidatorsText()
	{
		var v = _v;
		var ctx = _v.Context;
		var slots = new List<string>();
		var checks = new List<string>();
		var empties = new List<string>();

		foreach (var msg in _m.Messages)
		{
			slots.Add($"\t/// <summary>Holds a {v.Title} {msg.Name} to the schema.</summary>");
			slots.Add($"\tpublic Func<{ctx}, FixMessage.{msg.Name}, bool> {msg.Name} {{ get; set; }} = Validate{msg.Name};");
			slots.Add("");

			checks.Add($"\tstatic bool Validate{msg.Name}({ctx} context, FixMessage.{msg.Name} message)");
			checks.Add("\t{");
			checks.AddRange(CheckBody(_m.MessageMembers[msg.Name], "message"));
			checks.Add("");
			checks.Add("\t\treturn message.IsValid;");
			checks.Add("\t}");
			checks.Add("");
		}

		foreach (var i in _m.InterfacesOrdered)
		{
			slots.Add($"\t/// <summary>Holds a {v.Title} {i.Name} to the schema, wherever it is carried.</summary>");
			slots.Add($"\tpublic Func<{ctx}, FixMessage, {i.TypeName}, bool> {i.Name} {{ get; set; }} = Validate{i.Name};");
			slots.Add("");

			checks.Add($"\tstatic bool Validate{i.Name}({ctx} context, FixMessage message, {i.TypeName} block)");
			checks.Add("\t{");
			checks.AddRange(CheckBody(i.Members, "block"));
			checks.Add("");
			checks.Add("\t\treturn message.IsValid;");
			checks.Add("\t}");
			checks.Add("");

			var props = Fx.PropertiesOf(_m, i.Members).Select(p => p.Member).ToList();

			empties.Add($"\t/// <summary>Whether a carrier of the {v.Title} {i.Name} has none of its fields.</summary>");
			empties.Add($"\tinternal static bool Empty({i.TypeName} block)");
			empties.Add("\t{");
			empties.Add("\t\treturn " + string.Join(" &&\n\t\t\t", props.Select(p => p is Field pf ? $"block.{pf.Name} is null" : $"block.{((Group)p).List} is null")) + ";");
			empties.Add("\t}");
			empties.Add("");
		}

		var entries = _m.InterfacesOrdered.SelectMany(i => Fx.AllEntries(i.Members))
			.Concat(_m.Messages.SelectMany(msg => Fx.AllEntries(_m.MessageMembers[msg.Name])))
			.ToList();

		foreach (var e in entries)
		{
			slots.Add($"\t/// <summary>Holds one entry of {e.TypeName} to the schema.</summary>");
			slots.Add($"\tpublic Func<{ctx}, FixMessage, {e.TypeName}, int, bool> {e.Slot} {{ get; set; }} = Validate{e.Slot};");
			slots.Add("");

			checks.Add($"\tstatic bool Validate{e.Slot}({ctx} context, FixMessage message, {e.TypeName} entry, int index)");
			checks.Add("\t{");
			checks.AddRange(CheckBody(e.Members, "entry", e.Opener));
			checks.Add("");
			checks.Add("\t\treturn message.IsValid;");
			checks.Add("\t}");
			checks.Add("");
		}

		var head = new List<string>
		{
			"using System;",
			"using System.Collections.Generic;",
			"",
			$"namespace DotGram.Finance.Fix.{v.Ns};",
			"",
			Written(),
			"",
			"/// <summary>",
			$"/// The check of every {v.Title} message type, of every component it reuses, and of every entry of",
			$"/// every repeating group; the fields' own are in {v.Validator}.Fields.cs.",
			"/// </summary>",
			"/// <remarks>",
			"/// Straight-line checks written against the fields of the class they are about, and nothing else:",
			"/// a field the repository marks required, a component it marks required and the carrier left empty,",
			"/// every field handed to its own slot, and every count held to the entries that follow it. Every",
			"/// slot is typed on what it checks and named for it — a message type, a component, and a group",
			"/// entry by the path to it, <c>NewOrderSingle_NoAllocs</c> — so that a dictionary loaded at run",
			"/// time replaces the slots it describes by name and leaves the rest.",
			"/// </remarks>",
			$"partial class {v.Validator}",
			"{",
			"\t/// <summary>What this package compiles in, which is what a context takes unless it is given another.</summary>",
			$"\tpublic static readonly {v.Validator} Default = new();",
			"",
		};

		var result = new List<string>();

		result.AddRange(head);
		result.AddRange(Translations());
		result.AddRange(slots);
		result.AddRange(checks);

		if (empties.Count > 0)
			result.AddRange(empties.GetRange(0, empties.Count - 1));

		result.Add("}");
		result.Add("");

		return result;
	}

	/// <summary>The names a dictionary loaded at run time is translated by, where the version cannot keep its own.</summary>
	public List<string> Translations()
	{
		var outLines = new List<string>();
		var classes = _m.V.ClassNames;
		var fields = new Dictionary<string, string>();

		foreach (var kv in _m.FieldName)
			if (_m.FieldSlot[kv.Key] != kv.Value)
				fields[kv.Value] = _m.FieldSlot[kv.Key];

		var tables = new (string Method, Dictionary<string, string> Table, string Why)[]
		{
			("MessageClass", classes, "a message whose class cannot have the name the dictionaries give it"),
			("FieldSlotName", fields, "a field whose name a message or a component has first"),
		};

		foreach (var (method, table, why) in tables)
		{
			if (table.Count == 0)
				continue;

			var width = table.Keys.Max(k => k.Length) + 2;

			outLines.Add($"\t/// <summary>The translation of {why}.</summary>");
			outLines.Add($"\tprivate protected override string {method}(string name)");
			outLines.Add("\t{");
			outLines.Add("\t\treturn name switch");
			outLines.Add("\t\t{");

			foreach (var k in table.Keys.OrderBy(k => k, StringComparer.Ordinal))
				outLines.Add($"\t\t\t{("\"" + k + "\"").PadRight(width)} => \"{table[k]}\",");

			outLines.Add($"\t\t\t{"_".PadRight(width)} => name,");
			outLines.Add("\t\t};");
			outLines.Add("\t}");
			outLines.Add("");
		}

		// The fields the version names otherwise than FixTag, which takes the newest version's names:
		// a fragment of a dictionary may name one it does not describe.
		var tags = new Dictionary<string, int>();

		foreach (var kv in _m.FieldName)
			if (_m.TagNameOf(kv.Key) != kv.Value)
				tags[kv.Value] = kv.Key;

		if (tags.Count > 0)
		{
			var width = tags.Keys.Max(k => k.Length) + 2;

			outLines.Add("\t/// <summary>The tag of a field this version names otherwise than FixTag does.</summary>");
			outLines.Add("\tprivate protected override int FieldTag(string name)");
			outLines.Add("\t{");
			outLines.Add("\t\treturn name switch");
			outLines.Add("\t\t{");

			foreach (var k in tags.Keys.OrderBy(k => k, StringComparer.Ordinal))
				outLines.Add($"\t\t\t{("\"" + k + "\"").PadRight(width)} => {tags[k]},");

			outLines.Add($"\t\t\t{"_".PadRight(width)} => 0,");
			outLines.Add("\t\t};");
			outLines.Add("\t}");
			outLines.Add("");
		}

		return outLines;
	}

	// The fields' own checks: the value's type, and the code set the repository publishes for it.

	public static string Literal(string cls, string code)
	{
		if (cls == "Character")
			return "'" + (code == "\\" || code == "'" ? "\\" + code : code) + "'";

		if (cls == "Integer" || cls == "Decimal")
			return code;

		return "\"" + code.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
	}

	/// <summary>The alternatives of a pattern, `a or b or c`, broken at `or` where a line passes the width.</summary>
	public static List<string> Wrapped(List<string> items, string first, string pad, int width = 116)
	{
		var lines = new List<string>();
		var line  = first;

		for (var k = 0; k < items.Count; k++)
		{
			var piece = items[k] + (k < items.Count - 1 ? " or" : "");

			if (Fx.ExpandTabs(line, 4).Length + piece.Length + 1 > width && !line.EndsWith("("))
			{
				lines.Add(line);
				line = pad + piece;
			}
			else
			{
				line += (line.EndsWith("(") ? "" : " ") + piece;
			}
		}

		lines.Add(line);

		return lines;
	}

	public List<string> FieldsText()
	{
		var v = _v;
		var ctx = _v.Context;
		var tags = _m.FieldName.Keys.OrderBy(t => t).ToList();
		var rows = new List<(string Doc, string Func, string Name)>();

		foreach (var t in tags)
		{
			var name = _m.FieldName[t];
			var cls  = _m.FieldClass[t];
			var codes = cls != "Boolean" && _m.Enums.TryGetValue(t, out var c) ? c : null;
			var listed = codes != null && codes.Count > 0 ? "the values the specification lists" : "its type";

			rows.Add(($"\t/// <summary>Holds a {v.Title} {name}, tag {t}, to {listed}.</summary>", $"Func<{ctx}, FixMessage, FixField.{cls}, bool>", _m.FieldSlot[t]));
		}

		var tw = rows.Max(r => r.Func.Length);
		var nw = rows.Max(r => r.Name.Length);

		var outLines = new List<string>
		{
			"using System;",
			"",
			$"namespace DotGram.Finance.Fix.{v.Ns};",
			"",
			Written(),
			"",
			$"/// <summary>The check of every field of {v.Title} against its type, and against the values the repository lists for it.</summary>",
			$"partial class {v.Validator}",
			"{",
			"\t// The fields, every one, so that a dictionary has a slot for whatever it limits.",
			"",
		};

		foreach (var (doc, func, name) in rows)
		{
			outLines.Add(doc);
			outLines.Add($"\tpublic {func.PadRight(tw)} {name.PadRight(nw)} {{ get; set; }} = Validate{name};");
			outLines.Add("");
		}

		foreach (var t in tags)
		{
			var cls = _m.FieldClass[t];
			var codes = cls != "Boolean" && _m.Enums.TryGetValue(t, out var c) ? c : null;

			outLines.Add($"\tstatic bool Validate{_m.FieldSlot[t]}({ctx} context, FixMessage message, FixField.{cls} field)");
			outLines.Add("\t{");

			if (codes == null || codes.Count == 0)
			{
				outLines.Add("\t\tif (!field.IsValid)");
				outLines.Add("\t\t\tInvalid(message, field);");
			}
			else
			{
				var items = codes.OrderBy(x => x, StringComparer.Ordinal)
					.Select(code => Literal(cls == "Text" || cls == "Multiple" || cls == "MonthYear" ? "Text" : cls, code))
					.ToList();

				if (cls == "Multiple")
				{
					outLines.Add("\t\tforeach (var code in field.Value)");

					var wrapped = Wrapped(items, "\t\t\tif (code is not (", "\t\t\t\t");

					wrapped[^1] += "))";
					outLines.AddRange(wrapped);
					outLines.Add("\t\t\t{");
					outLines.Add("\t\t\t\tInvalid(message, field);");
					outLines.Add("");
					outLines.Add("\t\t\t\tbreak;");
					outLines.Add("\t\t\t}");
				}
				else if (cls == "Text" || cls == "Character")
				{
					var wrapped = Wrapped(items, "\t\tif (field.Value is not (", "\t\t\t");

					wrapped[^1] += "))";
					outLines.AddRange(wrapped);
					outLines.Add("\t\t\tInvalid(message, field);");
				}
				else
				{
					outLines.Add("\t\tif (!field.IsValid)");
					outLines.Add("\t\t\tInvalid(message, field);");

					var wrapped = Wrapped(items, "\t\telse if (field.Value is not (", "\t\t\t");

					wrapped[^1] += "))";
					outLines.AddRange(wrapped);
					outLines.Add("\t\t\tInvalid(message, field);");
				}
			}

			outLines.Add("");
			outLines.Add("\t\treturn message.IsValid;");
			outLines.Add("\t}");
			outLines.Add("");
		}

		outLines.RemoveAt(outLines.Count - 1);
		outLines.Add("}");
		outLines.Add("");

		return outLines;
	}

	// The standard header and trailer, which every message carries and reads before its own fields.

	public List<string> HeaderText()
	{
		var v = _v;
		var members = _m.Header.Concat(_m.Trailer).ToList();
		var outLines = new List<string>
		{
			"using System.Collections.Generic;",
			"",
			$"namespace DotGram.Finance.Fix.{v.Ns};",
			"",
			Written(),
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
		};

		outLines.AddRange(Arms(members, "", new List<string>(), "-1", pad: "\t\t\t"));
		outLines.AddRange(new[]
		{
			"",
			"\t\t\tdefault: return false;",
			"\t\t}",
			"",
			"\t\treturn true;",
			"\t}",
			"",
		});

		var headerTags = new HashSet<int>(Flat(_m.Header).Select(h => h.Tag));

		string Doc(Field f)
		{
			var part = headerTags.Contains(f.Tag) ? "StandardHeader" : "StandardTrailer";

			return $"The FIX {f.Name}, tag {f.Tag}, of this message's {part}; null when the field is absent.";
		}

		outLines.AddRange(ClassBody(members, 1, "protected set", doc: Doc));

		while (outLines[^1] == "")
			outLines.RemoveAt(outLines.Count - 1);

		outLines.Add("}");
		outLines.Add("");

		return outLines;
	}

	public IEnumerable<Field> Flat(List<Member> members)
	{
		foreach (var mm in members)
		{
			if (mm is Field field)
			{
				yield return field;
			}
			else if (mm is Group group)
			{
				yield return group.Counter;

				foreach (var f in Flat(group.Entry.Members))
					yield return f;
			}
		}
	}

	// The tables the templates are given.

	public List<string> MessageSwitch()
	{
		var width = _m.Messages.Max(msg => msg.Name.Length);
		var longest = _m.Messages.Max(msg => msg.MsgType.Length);

		if (longest > 2)
			throw new GeneratorException($"{_v.Title}: a MsgType of {longest} characters, which the switch does not read");

		var outLines = new List<string>();

		foreach (var msg in _m.Messages.OrderBy(m => m.MsgType.Length).ThenBy(m => m.MsgType, StringComparer.Ordinal))
		{
			var t = msg.MsgType;
			var second = t.Length == 1 ? "_  " : "'" + t[1] + "'";
			var key = $"({t.Length}, '{t[0]}', {second})";

			outLines.Add($"\t\t\t\t{key} => new FixMessage.{msg.Name.PadRight(width)} (fields),");
		}

		return outLines;
	}

	public List<string> VersionText()
	{
		var pairs = _m.Pairs.OrderBy(kv => kv.Value).ToList();
		var lw = pairs.Max(p => _m.TagNameOf(p.Key).Length) + "FixTag.".Length + 1;
		var dw = pairs.Max(p => _m.TagNameOf(p.Value).Length) + "FixTag.".Length;
		var outLines = new List<string> { "\t\tpublic static readonly FixVersion Version = new(new()", "\t\t{" };

		foreach (var (l, d) in pairs.Select(kv => (kv.Key, kv.Value)))
			outLines.Add($"\t\t\t{{ {("FixTag." + _m.TagNameOf(l) + ",").PadRight(lw)} {("FixTag." + _m.TagNameOf(d)).PadRight(dw)} }},");

		var last = _m.FieldName.Keys.Max();

		outLines.Add($"\t\t}}, FixStandard.Type, {_m.TagConst(last)});");

		return outLines;
	}

	public List<string> HeaderRequired()
	{
		var required = Flat(_m.Header.Concat(_m.Trailer).ToList()).Where(f => f.Required).ToList();
		var width = required.Max(f => f.Name.Length);

		return required.Select(f => $"\t\tif (message.{f.Name.PadRight(width)} is null) Missing(message, {f.Const});").ToList();
	}

	/// <summary>The count of every group of the header and trailer held to its entries, between blank lines.</summary>
	public List<string> HeaderGroups()
	{
		var groups = _m.Header.Concat(_m.Trailer).OfType<Group>().ToList();

		if (groups.Count == 0)
			return new List<string> { "" };

		var outLines = new List<string> { "" };

		outLines.AddRange(groups.Select(g => $"\t\tCounted(message, message.{g.Counter.Name}, message.{g.List});"));
		outLines.Add("");

		return outLines;
	}

	public List<string> HeaderSets()
	{
		var header  = Flat(_m.Header).Select(f => f.Const).ToList();
		var trailer = Flat(_m.Trailer).Select(f => f.Const).ToList();
		var encoded = _m.Pairs.Values.OrderBy(t => t).Where(t => _m.FieldName[t].StartsWith("Encoded")).Select(t => _m.TagConst(t)).ToList();

		List<string> Body(List<string> tags)
		{
			return Wrapped(tags, "\t\treturn tag is", "\t\t\t", width: 132);
		}

		var outLines = new List<string> { "\tstatic bool IsHeader(int tag)", "\t{" };

		outLines.AddRange(Body(header));
		outLines[^1] += ";";
		outLines.AddRange(new[] { "\t}", "", "\tstatic bool IsEncoded(int tag)", "\t{" });
		outLines.AddRange(Body(encoded));
		outLines[^1] += ";";
		outLines.AddRange(new[] { "\t}", "", "\tstatic bool IsTrailer(int tag)", "\t{" });
		outLines.AddRange(Body(trailer));
		outLines[^1] += ";";
		outLines.Add("\t}");

		return outLines;
	}

	public List<string> Template(string name, Dictionary<string, List<string>> tables)
	{
		var text = File.ReadAllText(Path.Combine(_templates, name)).Replace("\r\n", "\n");

		text = text.Replace("@@Ns@@", _v.Ns).Replace("@@Context@@", _v.Context).Replace("@@Validator@@", _v.Validator)
			.Replace("@@BeginString@@", _v.Begin).Replace("@@Title@@", _v.Title)
			.Replace("@@MessageCountWords@@", Fx.Words(_m.Messages.Count)).Replace("@@PairCountWords@@", Fx.Words(_m.Pairs.Count));

		foreach (var (token, tableLines) in tables)
			text = text.Replace($"@@{token}@@", string.Join("\n", tableLines));

		var left = Regex.Matches(text, @"@@\w+@@").Select(m => m.Value).ToList();

		if (left.Count > 0)
			throw new GeneratorException($"{name}: tokens left unwritten: [{string.Join(", ", left.Select(l => "'" + l + "'"))}]");

		var lines = text.Split('\n').ToList();
		var at = lines.FindIndex(l => l.StartsWith("namespace "));

		lines.Insert(at + 1, Written("Templates/" + name));
		lines.Insert(at + 1, "");

		return lines;
	}

	public void Write(string name, List<string> lines)
	{
		var directory = Path.Combine(_here, _v.Ns);

		Directory.CreateDirectory(directory);

		var text = string.Join("\n", lines).Replace("\r\n", "\n").Replace("\n", "\r\n");
		var bytes = new byte[] { 0xEF, 0xBB, 0xBF }.Concat(Encoding.UTF8.GetBytes(text)).ToArray();

		File.WriteAllBytes(Path.Combine(directory, name), bytes);
		Console.WriteLine($"written {_v.Ns}/{name} {lines.Count} lines");
	}

	public Model Run()
	{
		var v = _v;
		var m = _m;

		var types = new List<string>
		{
			"using System.Collections.Generic;", "", $"namespace DotGram.Finance.Fix.{v.Ns};", "", Written(), "",
			"public abstract partial class FixMessage", "{",
		};

		foreach (var msg in m.Messages)
			types.AddRange(MessageClass(msg));

		types.RemoveAt(types.Count - 1);
		types.Add("}");
		types.Add("");

		var components = new List<string>
		{
			"using System;", "", "// ReSharper disable InconsistentNaming", "", $"namespace DotGram.Finance.Fix.{v.Ns};", "",
			Written(), "//",
			$"// The components {v.Title} reuses across message types, as the shape a message or a group entry has",
			"// when it carries one. A component is written into its carrier field by field, because that is",
			"// what it is on the wire; the interface is those same fields read as the one thing they are, so",
			"// that what is asked of a component is written once and asked of every carrier. A group inside a",
			"// component is the same group wherever the component is carried, so its entry is a class nested",
			"// in the interface: IInstrument.NoSecurityAltIDGroup.", "",
		};

		foreach (var i in m.InterfacesOrdered)
			components.AddRange(InterfaceText(i));

		var width = m.FieldName.Keys.Max(t => m.TagNameOf(t).Length);
		var standard = new List<string>
		{
			$"namespace DotGram.Finance.Fix.{v.Ns};", "", Written(), "", "static class FixStandard", "{",
			$"\t/// <summary>The type of the value of a field {v.Title} defines; <see cref=\"FixValueType.None\"/> for a tag it does not.</summary>",
			"\tinternal static FixValueType Type(int tag)", "\t{", "\t\treturn tag switch", "\t\t{",
		};

		foreach (var t in m.FieldName.Keys.OrderBy(t => t))
			standard.Add($"\t\t\tFixTag.{m.TagNameOf(t).PadRight(width)} => FixValueType.{m.ValueType[t]},");

		standard.Add($"\t\t\t{"_".PadRight(width + 7)} => FixValueType.None,");
		standard.Add("\t\t};");
		standard.Add("\t}");
		standard.Add("}");
		standard.Add("");

		Write("FixMessage.Types.cs", types);
		Write("FixMessage.Header.cs", HeaderText());

		// A version whose repository names no component outside the header, FIX 4.2, has none to write.
		if (m.InterfacesOrdered.Count > 0)
		{
			Write("FixComponents.cs", components);
		}
		else
		{
			var path = Path.Combine(_here, v.Ns, "FixComponents.cs");

			if (File.Exists(path))
				File.Delete(path);
		}

		Write("FixStandard.cs", standard);
		Write($"{v.Validator}.cs", ValidatorsText());
		Write($"{v.Validator}.Fields.cs", FieldsText());

		Write("FixMessage.cs", Template("FixMessage.cs.in", new Dictionary<string, List<string>>()));
		Write("FixCustomMessage.cs", Template("FixCustomMessage.cs.in", new Dictionary<string, List<string>>()));
		Write("FixParser.cs", Template("FixParser.cs.in", new Dictionary<string, List<string>>()));
		Write("FixParser.Messages.cs", Template("FixParser.Messages.cs.in", new Dictionary<string, List<string>> { ["MessageSwitch"] = MessageSwitch() }));
		Write("FixParser.Streaming.cs", Template("FixParser.Streaming.cs.in", new Dictionary<string, List<string>>()));
		Write($"{v.Context}.cs", Template("Context.cs.in", new Dictionary<string, List<string>> { ["Version"] = VersionText() }));
		Write($"{v.Validator}.Header.cs", Template("Validator.Header.cs.in", new Dictionary<string, List<string>>
		{
			["HeaderRequired"] = HeaderRequired(),
			["HeaderSets"]     = HeaderSets(),
			["HeaderGroups"]   = HeaderGroups(),
		}));

		return m;
	}
}
