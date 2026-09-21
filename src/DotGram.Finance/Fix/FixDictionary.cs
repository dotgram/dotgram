using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace DotGram.Finance.Fix;

/// <summary>
/// A FIX dictionary read from a file: the tags, their types and code sets, the components, the
/// repeating groups, and the composition of each message.
/// </summary>
/// <remarks>
/// <para>
/// This package compiles FIX 4.4's schema in and ships no dictionary of anyone's. A counterparty's
/// dictionary is the consumer's file, in the consumer's repository, read by the consumer's code —
/// which is where that file's licence obligations sit, including if a diagnostic quotes its text.
/// </para>
/// <para>
/// The format is QuickFIX's: <c>&lt;fix&gt;</c> over <c>&lt;header&gt;</c>, <c>&lt;trailer&gt;</c>,
/// <c>&lt;messages&gt;</c>, <c>&lt;components&gt;</c> and <c>&lt;fields&gt;</c>. There is no DTD and
/// no schema anywhere in the QuickFIX tree, so the format is whatever its readers accept and they
/// differ from one another. What this one accepts is written down member by member, and where it
/// refuses it names the element and the line.
/// </para>
/// <para>
/// A dictionary is read once and asked many times, so an instance is immutable and may be used from
/// several threads.
/// </para>
/// </remarks>
sealed class FixDictionary
{
	readonly Dictionary<int, Field>         fields;
	readonly Dictionary<string, SchemaRef[]> messages;
	readonly Dictionary<string, string>     messageNames;
	readonly SchemaRef[][]                  components;
	readonly string[]                       componentNames;
	readonly SchemaRef[][]                  groups;
	readonly int[]                          counters;

	FixDictionary(
		string                           version,
		Dictionary<int, Field>           fields,
		Dictionary<string, SchemaRef[]>  messages,
		Dictionary<string, string>       messageNames,
		SchemaRef[][]                    components,
		string[]                         componentNames,
		SchemaRef[][]                    groups,
		int[]                            counters,
		SchemaRef[]                      header,
		SchemaRef[]                      trailer)
	{
		Version           = version;
		this.fields       = fields;
		this.messages     = messages;
		this.messageNames = messageNames;
		this.components   = components;
		this.componentNames = componentNames;
		this.groups       = groups;
		this.counters     = counters;
		this.header       = header;
		this.trailer      = trailer;
	}

	readonly SchemaRef[] header;
	readonly SchemaRef[] trailer;

	/// <summary>The version the file declares, as <c>FIX.4.4</c>.</summary>
	/// <remarks>
	/// Built from <c>type</c>, <c>major</c>, <c>minor</c> and <c>servicepack</c>. The published
	/// FIX 4.4 dictionary carries only <c>major</c> and <c>minor</c>, so <c>type</c> defaults to
	/// <c>FIX</c> and a service pack of zero is left off — which is how QuickFIX spells it too.
	/// </remarks>
	public string Version { get; }

	/// <summary>The MsgType of every message the dictionary describes.</summary>
	public IReadOnlyCollection<string> MessageTypes => messages.Keys;

	/// <summary>Every tag the dictionary declares, in no particular order.</summary>
	public IReadOnlyCollection<int> Tags => fields.Keys;

	/// <summary>The name of a tag, or null where the dictionary declares no such tag.</summary>
	/// <param name="tag">The numeric FIX tag.</param>
	public string? Name(int tag) => fields.TryGetValue(tag, out var field) ? field.Name : null;

	/// <summary>The declared type of a tag, as the file spells it, or null where there is none.</summary>
	/// <param name="tag">The numeric FIX tag.</param>
	/// <remarks>
	/// The vocabulary is the file's and not this package's: a dictionary may name a type this
	/// package has no code for, and reading one does not fail because of it.
	/// </remarks>
	public string? Type(int tag) => fields.TryGetValue(tag, out var field) ? field.Type : null;

	/// <summary>The code set of a tag, or null where the dictionary gives it none.</summary>
	/// <param name="tag">The numeric FIX tag.</param>
	public IReadOnlyList<string>? Codes(int tag) =>
		fields.TryGetValue(tag, out var field) ? field.Codes : null;

	/// <summary>The name a message type is given, or null where the dictionary has no such type.</summary>
	/// <param name="messageType">The MsgType, tag 35.</param>
	public string? MessageName(string messageType) =>
		messageType != null && messageNames.TryGetValue(messageType, out var name) ? name : null;

	// The same shapes FixSchema answers in, so one walk reads either. The ids are this
	// dictionary's own: a component is one id per declaration, and a group is one id per SITE,
	// because QuickFIX declares a group inline and the same name carries different members in
	// different messages — 59 names over 226 sites in the published FIX 4.4 file.
	internal SchemaRef[] Message(string type) => messages.TryGetValue(type, out var refs) ? refs : [];
	internal SchemaRef[] Component(int id) => components[id];

	// The name the file gave the component, which is what a comment about it has to say: an id
	// is ours and means nothing in the file a reader would go back to.
	internal string ComponentName(int id) => componentNames[id];
	internal SchemaRef[] Group(int id) => groups[id];
	internal int Counter(int id) => counters[id];
	internal SchemaRef[] Header => header;
	internal SchemaRef[] Trailer => trailer;
	internal bool Defines(int tag) => fields.ContainsKey(tag);
	internal bool Describes(string type) => messages.ContainsKey(type);

	// What the public Type and Codes answer, without the interface types: the walk asks these once
	// a field, and an array it can index beats a list it has to go through an interface to read.
	internal string?   CodeType(int tag) => fields.TryGetValue(tag, out var field) ? field.Type : null;
	internal string[]? CodeArray(int tag) => fields.TryGetValue(tag, out var field) ? field.Codes : null;

	/// <summary>Reads a dictionary from a stream, which is left open.</summary>
	/// <param name="input">The file's octets.</param>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	/// <exception cref="FormatException">The file is not a dictionary this reader accepts.</exception>
	public static FixDictionary Load(Stream input)
	{
		if (input is null)
			throw new ArgumentNullException(nameof(input));

		using var reader = XmlReader.Create(input, Settings());

		return Read(reader);
	}

	/// <summary>Reads a dictionary from a reader, which is left open.</summary>
	/// <param name="input">The file's text.</param>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	/// <exception cref="FormatException">The file is not a dictionary this reader accepts.</exception>
	public static FixDictionary Load(TextReader input)
	{
		if (input is null)
			throw new ArgumentNullException(nameof(input));

		using var reader = XmlReader.Create(input, Settings());

		return Read(reader);
	}

	/// <summary>Reads a dictionary from the whole text of one.</summary>
	/// <param name="text">The file.</param>
	/// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
	/// <exception cref="FormatException">The file is not a dictionary this reader accepts.</exception>
	public static FixDictionary Parse(string text)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		using var reader = new StringReader(text);

		return Load(reader);
	}

	// A dictionary is somebody else's file, and the two things an XML reader fetches from the
	// network or the disk on its own are the two turned off here.
	static XmlReaderSettings Settings() => new()
	{
		DtdProcessing                = DtdProcessing.Prohibit,
		XmlResolver                  = null,
		IgnoreComments               = true,
		IgnoreWhitespace             = true,
		IgnoreProcessingInstructions = true,
	};

	readonly struct Field(string name, string? type, string[]? codes)
	{
		public readonly string    Name  = name;
		public readonly string?   Type  = type;
		public readonly string[]? Codes = codes;
	}

	// ── reading ──────────────────────────────────────────────────────────────────────────────
	//
	// A member names a field, a component or a group by NAME, and `<fields>` is the last section
	// of the published file, so nothing can be resolved to a tag while it is being read. The parse
	// therefore builds a tree of names and resolves it once the whole file is in hand.

	sealed class Member(int kind, string name, bool required)
	{
		public readonly int          Kind     = kind;   // 0 field, 1 component, 2 group
		public readonly string       Name     = name;
		public readonly bool         Required = required;
		public readonly List<Member> Members  = [];
	}

	sealed class Reading
	{
		public string?                       Type;
		public string?                       Major;
		public string?                       Minor;
		public string?                       ServicePack;
		public readonly List<Member>         Header       = [];
		public readonly List<Member>         Trailer      = [];
		public readonly List<(string Type, string Name, List<Member> Members)> Messages = [];
		public readonly Dictionary<string, List<Member>> Components = new(StringComparer.Ordinal);
		public readonly Dictionary<string, int>          Tags       = new(StringComparer.Ordinal);
		public readonly Dictionary<int, Field>           Fields     = [];
	}

	static FormatException Bad(XmlReader reader, string reason)
	{
		var at = reader as IXmlLineInfo;

		return new FormatException(at is not null && at.HasLineInfo()
			? $"{reason} (line {at.LineNumber}, position {at.LinePosition})"
			: reason);
	}

	static string Required(XmlReader reader, string attribute)
		=> reader.GetAttribute(attribute) ?? throw Bad(reader, $"<{reader.Name}> has no {attribute}.");

	static FixDictionary Read(XmlReader reader)
	{
		var read = new Reading();

		if (!reader.ReadToFollowing("fix"))
			throw new FormatException("The file has no <fix> element.");

		read.Type        = reader.GetAttribute("type");
		read.Major       = Required(reader, "major");
		read.Minor       = Required(reader, "minor");
		read.ServicePack = reader.GetAttribute("servicepack");

		if (!reader.IsEmptyElement)
			while (reader.Read())
			{
				if (reader.NodeType == XmlNodeType.EndElement)
					break;

				if (reader.NodeType != XmlNodeType.Element)
					continue;

				switch (reader.Name)
				{
					case "header":     Members(reader, read.Header);  break;
					case "trailer":    Members(reader, read.Trailer); break;
					case "messages":   Messages(reader, read);        break;
					case "components": Components(reader, read);      break;
					case "fields":     Fields(reader, read);          break;

					// A section this reader has no use for is skipped rather than refused: the
					// format has no schema, so an unexpected element is somebody's extension and
					// not necessarily a mistake.
					default: reader.Skip(); break;
				}
			}

		return Resolve(read);
	}

	static void Messages(XmlReader reader, Reading read)
	{
		if (reader.IsEmptyElement)
			return;

		while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType != XmlNodeType.Element)
				continue;

			if (reader.Name != "message")
			{
				reader.Skip();
				continue;
			}

			var name    = Required(reader, "name");
			var type    = Required(reader, "msgtype");
			var members = new List<Member>();

			Members(reader, members);
			read.Messages.Add((type, name, members));
		}
	}

	static void Components(XmlReader reader, Reading read)
	{
		if (reader.IsEmptyElement)
			return;

		while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType != XmlNodeType.Element)
				continue;

			if (reader.Name != "component")
			{
				reader.Skip();
				continue;
			}

			var name    = Required(reader, "name");
			var members = new List<Member>();

			Members(reader, members);

			if (read.Components.ContainsKey(name))
				throw Bad(reader, $"The component '{name}' is declared twice.");

			read.Components.Add(name, members);
		}
	}

	static void Fields(XmlReader reader, Reading read)
	{
		if (reader.IsEmptyElement)
			return;

		while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType != XmlNodeType.Element)
				continue;

			if (reader.Name != "field")
			{
				reader.Skip();
				continue;
			}

			var number = Required(reader, "number");
			var name   = Required(reader, "name");
			var type   = reader.GetAttribute("type");

			if (!int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out var tag) || tag <= 0)
				throw Bad(reader, $"The field '{name}' has number '{number}', which is not a tag.");

			var codes = Values(reader);

			if (read.Fields.ContainsKey(tag))
				throw Bad(reader, $"Tag {tag} is declared twice.");

			read.Fields.Add(tag, new Field(name, type, codes));

			// A name is how every member refers to a field, so a name that means two tags would
			// make a message's composition ambiguous. The file is refused rather than read to
			// whichever of the two came last.
			if (read.Tags.TryGetValue(name, out var already))
				throw Bad(reader, $"The name '{name}' is given to tags {already} and {tag}.");

			read.Tags.Add(name, tag);
		}
	}

	static string[]? Values(XmlReader reader)
	{
		if (reader.IsEmptyElement)
			return null;

		var codes = new List<string>();

		while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType != XmlNodeType.Element)
				continue;

			if (reader.Name != "value")
			{
				reader.Skip();
				continue;
			}

			codes.Add(Required(reader, "enum"));

			if (!reader.IsEmptyElement)
				reader.Skip();
		}

		return codes.Count == 0 ? null : codes.ToArray();
	}

	// The members of a scope: fields, component references, and groups, which carry members of
	// their own and nest.
	static void Members(XmlReader reader, List<Member> into)
	{
		if (reader.IsEmptyElement)
			return;

		var depth = reader.Depth;

		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == depth)
				break;

			if (reader.NodeType != XmlNodeType.Element)
				continue;

			switch (reader.Name)
			{
				case "field":
					into.Add(new Member(0, Required(reader, "name"), Is(reader)));
					if (!reader.IsEmptyElement) reader.Skip();
					break;

				case "component":
					into.Add(new Member(1, Required(reader, "name"), Is(reader)));
					if (!reader.IsEmptyElement) reader.Skip();
					break;

				case "group":
				{
					var group = new Member(2, Required(reader, "name"), Is(reader));

					into.Add(group);
					Members(reader, group.Members);
					break;
				}

				default:
					reader.Skip();
					break;
			}
		}
	}

	// `required` is `Y` or `N`; anything else would be a five-valued presence this format does not
	// have, so it is refused rather than read as one of the two.
	static bool Is(XmlReader reader)
	{
		var required = reader.GetAttribute("required");

		return required switch
		{
			"Y"  => true,
			"N"  => false,
			null => false,
			_    => throw Bad(reader, $"required='{required}' on <{reader.Name} name='{reader.GetAttribute("name")}'>; the format has only Y and N."),
		};
	}

	static FixDictionary Resolve(Reading read)
	{
		if (read.Fields.Count == 0)
			throw new FormatException("The dictionary declares no fields.");

		var components = new List<SchemaRef[]>();
		var componentNames = new List<string>();
		var groups     = new List<SchemaRef[]>();
		var counters   = new List<int>();
		var ids        = new Dictionary<string, int>(StringComparer.Ordinal);

		SchemaRef[] Refs(List<Member> members, string where)
		{
			var refs = new SchemaRef[members.Count];

			for (var i = 0; i < members.Count; i++)
			{
				var member = members[i];

				switch (member.Kind)
				{
					case 0:
						refs[i] = new SchemaRef(Tag(member.Name, where), member.Required, 0);
						break;

					case 1:
						refs[i] = new SchemaRef(ComponentId(member.Name, where), member.Required, 1);
						break;

					default:
					{
						// A group's counter is the field its own name names, and the group is one
						// id per site: the same name carries different members in different
						// messages, so an id per name would merge two shapes into one.
						var id = groups.Count;

						groups.Add([]);
						counters.Add(Tag(member.Name, where));
						groups[id] = Refs(member.Members, where + "/" + member.Name);

						if (groups[id].Length == 0)
							throw new FormatException($"The group '{member.Name}' in {where} has no members, so no entry could begin.");

						refs[i] = new SchemaRef(id, member.Required, 2);
						break;
					}
				}
			}

			return refs;
		}

		int Tag(string name, string where) =>
			read.Tags.TryGetValue(name, out var tag)
				? tag
				: throw new FormatException($"{where} names the field '{name}', which the dictionary does not declare.");

		int ComponentId(string name, string where)
		{
			if (ids.TryGetValue(name, out var id))
				return id;

			if (!read.Components.TryGetValue(name, out var members))
				throw new FormatException($"{where} names the component '{name}', which the dictionary does not declare.");

			id = components.Count;
			components.Add([]);
			componentNames.Add(name);
			ids.Add(name, id);
			components[id] = Refs(members, "the component '" + name + "'");

			return id;
		}

		var messages     = new Dictionary<string, SchemaRef[]>(StringComparer.Ordinal);
		var messageNames = new Dictionary<string, string>(StringComparer.Ordinal);

		foreach (var (type, name, members) in read.Messages)
		{
			if (messages.ContainsKey(type))
				throw new FormatException($"Two messages are given MsgType '{type}': '{messageNames[type]}' and '{name}'.");

			messages.Add(type, Refs(members, "the message '" + name + "'"));
			messageNames.Add(type, name);
		}

		var header  = Refs(read.Header,  "the header");
		var trailer = Refs(read.Trailer, "the trailer");

		var version = (read.Type ?? "FIX") + "." + read.Major + "." + read.Minor +
			(read.ServicePack is null or "0" ? "" : "." + read.ServicePack);

		return new FixDictionary(
			version, read.Fields, messages, messageNames,
			components.ToArray(), componentNames.ToArray(), groups.ToArray(), counters.ToArray(), header, trailer);
	}
}
