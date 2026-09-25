using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Xml;

namespace DotGram.Finance.Fix;

/// <summary>
/// A FIX data dictionary as a file says it: the composition of each message, the components, and the
/// fields with their code sets, by name. Read from a file, merged and edited in code, and then applied
/// to a context.
/// </summary>
/// <remarks>
/// <para>
/// The format is the FIX data dictionary's: <c>&lt;fix&gt;</c> over <c>&lt;header&gt;</c>, <c>&lt;trailer&gt;</c>,
/// <c>&lt;messages&gt;</c>, <c>&lt;components&gt;</c> and <c>&lt;fields&gt;</c>. Any of the five may
/// be absent, because a file need not be a whole dictionary: a fragment that says what one venue
/// adds is read the same way, and what it does not mention it has no opinion about.
/// </para>
/// <para>
/// Nothing is resolved here, and nothing is checked against a version: a message refers to its fields
/// by name and this keeps the names. What a name means is asked when the dictionary is applied to a
/// context, which refuses what the version's model has no place for. Reading refuses only what is not
/// a dictionary; where the file cannot be read the refusal names the element and the line.
/// </para>
/// <para>
/// The object is a plain, editable value: add a message, change what one requires, drop a code from a
/// field, and apply the result. Applying takes what the dictionary says at that moment; editing it
/// afterwards changes nothing in a context already made from it.
/// </para>
/// </remarks>
public sealed class FixDictionary
{
	/// <summary>The fields, components and groups of the standard header, where the dictionary describes it.</summary>
	public List<FixDictionaryMember> Header { get; } = [];

	/// <summary>The fields, components and groups of the standard trailer, where the dictionary describes it.</summary>
	public List<FixDictionaryMember> Trailer { get; } = [];

	/// <summary>The message types, by MsgType, in the order the dictionary gives them.</summary>
	public KeyedCollection<string, FixDictionaryMessage> Messages { get; } = new Keyed<string, FixDictionaryMessage>(static one => one.MsgType);

	/// <summary>The components, by name, in the order the dictionary gives them.</summary>
	public KeyedCollection<string, FixDictionaryComponent> Components { get; } = new Keyed<string, FixDictionaryComponent>(static one => one.Name);

	/// <summary>The fields, by tag.</summary>
	public Dictionary<int, FixDictionaryField> Fields { get; } = [];

	/// <summary>
	/// A new dictionary: this one with <paramref name="over"/> read over it. A message type, component or
	/// field <paramref name="over"/> describes replaces this one's whole; a header or trailer it
	/// describes replaces this one's.
	/// </summary>
	/// <param name="over">What is read over this dictionary: a venue's file, a correction, a fragment.</param>
	/// <returns>A new dictionary sharing nothing with either, so that editing it edits neither.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="over"/> is null.</exception>
	/// <remarks>A message type is known by its MsgType, a component by its name and a field by its tag.</remarks>
	public FixDictionary Merge(FixDictionary over)
	{
		if (over == null) throw new ArgumentNullException(nameof(over));

		var merged = Copy();

		foreach (var message in over.Messages)
		{
			var copy = message.Copy();

			if (merged.Messages.Contains(message.MsgType))
				merged.Messages[merged.Messages.IndexOf(merged.Messages[message.MsgType])] = copy;
			else
				merged.Messages.Add(copy);
		}

		foreach (var component in over.Components)
		{
			var copy = component.Copy();

			if (merged.Components.Contains(component.Name))
				merged.Components[merged.Components.IndexOf(merged.Components[component.Name])] = copy;
			else
				merged.Components.Add(copy);
		}

		foreach (var field in over.Fields)
			merged.Fields[field.Key] = field.Value.Copy();

		if (over.Header.Count > 0)
		{
			merged.Header.Clear();
			merged.Header.AddRange(FixDictionaryMember.Copy(over.Header));
		}

		if (over.Trailer.Count > 0)
		{
			merged.Trailer.Clear();
			merged.Trailer.AddRange(FixDictionaryMember.Copy(over.Trailer));
		}

		return merged;
	}

	FixDictionary Copy()
	{
		var copy = new FixDictionary();

		copy.Header.AddRange(FixDictionaryMember.Copy(Header));
		copy.Trailer.AddRange(FixDictionaryMember.Copy(Trailer));

		foreach (var message in Messages)
			copy.Messages.Add(message.Copy());

		foreach (var component in Components)
			copy.Components.Add(component.Copy());

		foreach (var field in Fields)
			copy.Fields.Add(field.Key, field.Value.Copy());

		return copy;
	}

	/// <summary>Reads a dictionary from its text.</summary>
	/// <param name="text">A FIX data dictionary, or a fragment of one.</param>
	/// <returns>What the text says, unresolved.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
	/// <exception cref="FormatException">The text is not a dictionary this package can read: the refusal names the element and the line.</exception>
	public static FixDictionary Parse(string text)
	{
		if (text == null) throw new ArgumentNullException(nameof(text));

		using var reader = XmlReader.Create(new StringReader(text), Settings());

		return Read(reader);
	}

	/// <inheritdoc cref="Parse(string)"/>
	/// <param name="input">The dictionary's text; the reader is read and left open.</param>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	public static FixDictionary Read(TextReader input)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		using var reader = XmlReader.Create(input, Settings());

		return Read(reader);
	}

	/// <inheritdoc cref="Parse(string)"/>
	/// <param name="input">The dictionary's octets; the stream is read and left open.</param>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	public static FixDictionary Read(Stream input)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		using var reader = XmlReader.Create(input, Settings());

		return Read(reader);
	}

	/// <inheritdoc cref="Parse(string)"/>
	/// <param name="fileName">The path of the dictionary's file.</param>
	/// <exception cref="ArgumentNullException"><paramref name="fileName"/> is null.</exception>
	public static FixDictionary LoadFile(string fileName)
	{
		if (fileName == null) throw new ArgumentNullException(nameof(fileName));

		using var stream = File.OpenRead(fileName);

		return Read(stream);
	}

	static XmlReaderSettings Settings()
	{
		return new XmlReaderSettings
		{
			DtdProcessing    = DtdProcessing.Prohibit,
			XmlResolver      = null,
			IgnoreComments   = true,
			IgnoreWhitespace = true,
			CloseInput       = false,
		};
	}

	static FixDictionary Read(XmlReader reader)
	{
		var read = new FixDictionary();

		if (!reader.ReadToFollowing("fix"))
			throw new FormatException("The text has no <fix> element.");

		if (reader.IsEmptyElement)
			return read;

		while (reader.Read())
		{
			if (reader.NodeType == XmlNodeType.EndElement)
				break;

			if (reader.NodeType != XmlNodeType.Element)
				continue;

			switch (reader.Name)
			{
				case "header":     Members       (reader, read.Header);  break;
				case "trailer":    Members       (reader, read.Trailer); break;
				case "messages":   ReadMessages  (reader, read);         break;
				case "components": ReadComponents(reader, read);         break;
				case "fields":     ReadFields    (reader, read);         break;
				default:           reader.Skip();                        break;
			}
		}

		return read;
	}

	static void ReadMessages(XmlReader reader, FixDictionary read)
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

			var message = new FixDictionaryMessage(Required(reader, "msgtype"), Required(reader, "name"));

			// Two descriptions of one type in one file: the later is the file's, as it would be read
			// over another file.
			if (read.Messages.Contains(message.MsgType))
				read.Messages.Remove(message.MsgType);

			Members(reader, message.Members);
			read.Messages.Add(message);
		}
	}

	static void ReadComponents(XmlReader reader, FixDictionary read)
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

			var component = new FixDictionaryComponent(Required(reader, "name"));

			if (read.Components.Contains(component.Name))
				throw Bad(reader, $"The component '{component.Name}' is declared twice.");

			Members(reader, component.Members);
			read.Components.Add(component);
		}
	}

	static void ReadFields(XmlReader reader, FixDictionary read)
	{
		if (reader.IsEmptyElement)
			return;

		var names = new Dictionary<string, int>(StringComparer.Ordinal);

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
			var field  = new FixDictionaryField(name, reader.GetAttribute("type"));

			if (!int.TryParse(number, NumberStyles.None, CultureInfo.InvariantCulture, out var tag) || tag <= 0)
				throw Bad(reader, $"The field '{name}' has number '{number}', which is not a tag.");

			Values(reader, field.Codes);

			if (read.Fields.ContainsKey(tag))
				throw Bad(reader, $"Tag {tag} is declared twice.");

			// A name is how every member refers to a field, so a name that means two tags would
			// make a message's composition ambiguous. The file is refused rather than read to
			// whichever of the two came last; a dictionary edited into that state is refused where
			// it is applied.
			if (names.TryGetValue(name, out var already))
				throw Bad(reader, $"The name '{name}' is given to tags {already} and {tag}.");

			names.Add(name, tag);
			read.Fields.Add(tag, field);
		}
	}

	static void Values(XmlReader reader, List<string> into)
	{
		if (reader.IsEmptyElement)
			return;

		while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
		{
			if (reader.NodeType != XmlNodeType.Element)
				continue;

			if (reader.Name != "value")
			{
				reader.Skip();
				continue;
			}

			into.Add(Required(reader, "enum"));

			if (!reader.IsEmptyElement)
				reader.Skip();
		}
	}

	// The members of a scope: fields, component references, and groups, which carry members of
	// their own and nest.
	static void Members(XmlReader reader, List<FixDictionaryMember> into)
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
					into.Add(FixDictionaryMember.Field(Required(reader, "name"), Is(reader)));
					if (!reader.IsEmptyElement) reader.Skip();
					break;

				case "component":
					into.Add(FixDictionaryMember.Component(Required(reader, "name"), Is(reader)));
					if (!reader.IsEmptyElement) reader.Skip();
					break;

				case "group":
				{
					var group = FixDictionaryMember.Group(Required(reader, "name"), Is(reader));

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

	static bool Is(XmlReader reader)
	{
		return reader.GetAttribute("required") is "Y" or "y";
	}

	static string Required(XmlReader reader, string attribute)
	{
		return reader.GetAttribute(attribute) ?? throw Bad(reader, $"<{reader.Name}> has no {attribute}.");
	}

	static FormatException Bad(XmlReader reader, string reason)
	{
		var where = reader is IXmlLineInfo info && info.HasLineInfo() ? $" (line {info.LineNumber})" : "";

		return new FormatException(reason + where);
	}

	/// <summary>A collection kept in its order and looked up by a key its items carry.</summary>
	sealed class Keyed<TKey, T>(Func<T, TKey> key) : KeyedCollection<TKey, T>
		where TKey : notnull
	{
		protected override TKey GetKeyForItem(T item)
		{
			return key(item);
		}
	}
}

/// <summary>A message type of a dictionary: its MsgType, its name, and what it carries.</summary>
/// <param name="msgType">The MsgType, <c>D</c>.</param>
/// <param name="name">The name, which is the name of the message's class: <c>NewOrderSingle</c>.</param>
public sealed class FixDictionaryMessage(string msgType, string name)
{
	/// <summary>The MsgType, by which a dictionary knows the message.</summary>
	public string MsgType { get; } = msgType ?? throw new ArgumentNullException(nameof(msgType));

	/// <summary>The name, which is the name of the message's class.</summary>
	public string Name { get; set; } = name ?? throw new ArgumentNullException(nameof(name));

	/// <summary>What the message carries, in order.</summary>
	public List<FixDictionaryMember> Members { get; } = [];

	internal FixDictionaryMessage Copy()
	{
		var copy = new FixDictionaryMessage(MsgType, Name);

		copy.Members.AddRange(FixDictionaryMember.Copy(Members));

		return copy;
	}
}

/// <summary>A component of a dictionary: its name, which is the interface <c>I</c> and its name, and what it carries.</summary>
/// <param name="name">The name: <c>Instrument</c>.</param>
public sealed class FixDictionaryComponent(string name)
{
	/// <summary>The name, by which a dictionary knows the component.</summary>
	public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

	/// <summary>What the component carries, in order.</summary>
	public List<FixDictionaryMember> Members { get; } = [];

	internal FixDictionaryComponent Copy()
	{
		var copy = new FixDictionaryComponent(Name);

		copy.Members.AddRange(FixDictionaryMember.Copy(Members));

		return copy;
	}
}

/// <summary>A field of a dictionary: its name, the type it names, and the values it lists.</summary>
/// <param name="name">The name, by which messages and components refer to it: <c>OrdType</c>.</param>
/// <param name="type">The type as a dictionary names it, <c>CHAR</c>; null where it names none.</param>
public sealed class FixDictionaryField(string name, string? type = null)
{
	/// <summary>The name, by which messages and components refer to the field.</summary>
	public string Name { get; set; } = name ?? throw new ArgumentNullException(nameof(name));

	/// <summary>The type as a dictionary names it; null where it names none.</summary>
	/// <remarks>It types only a tag the version does not define: a standard field keeps the version's type.</remarks>
	public string? Type { get; set; } = type;

	/// <summary>The values the field may take; empty where the dictionary lists none, and then any value of its type is one.</summary>
	public List<string> Codes { get; } = [];

	internal FixDictionaryField Copy()
	{
		var copy = new FixDictionaryField(Name, Type);

		copy.Codes.AddRange(Codes);

		return copy;
	}
}

/// <summary>What a member of a message, component or group is.</summary>
public enum FixDictionaryMemberKind
{
	/// <summary>A field, by its name.</summary>
	Field,

	/// <summary>A component, by its name.</summary>
	Component,

	/// <summary>A repeating group, named for its counter, with members of its own.</summary>
	Group,
}

/// <summary>One thing a message, component or group carries: a field, a component, or a group.</summary>
public sealed class FixDictionaryMember
{
	FixDictionaryMember(FixDictionaryMemberKind kind, string name, bool required)
	{
		Kind     = kind;
		Name     = name ?? throw new ArgumentNullException(nameof(name));
		Required = required;
	}

	/// <summary>A field, by its name.</summary>
	/// <param name="name">The field's name.</param>
	/// <param name="required">Whether the carrier must have it.</param>
	/// <returns>The member.</returns>
	public static FixDictionaryMember Field(string name, bool required = false)
	{
		return new(FixDictionaryMemberKind.Field, name, required);
	}

	/// <summary>A component, by its name.</summary>
	/// <param name="name">The component's name.</param>
	/// <param name="required">Whether the carrier must have any of it.</param>
	/// <returns>The member.</returns>
	public static FixDictionaryMember Component(string name, bool required = false)
	{
		return new(FixDictionaryMemberKind.Component, name, required);
	}

	/// <summary>A repeating group, named for its counter; its members are added to <see cref="Members"/>.</summary>
	/// <param name="counter">The name of the counter field: <c>NoPartyIDs</c>.</param>
	/// <param name="required">Whether the carrier must have the counter.</param>
	/// <returns>The member.</returns>
	public static FixDictionaryMember Group(string counter, bool required = false)
	{
		return new(FixDictionaryMemberKind.Group, counter, required);
	}

	/// <summary>What the member is.</summary>
	public FixDictionaryMemberKind Kind { get; }

	/// <summary>The name of the field or component, or of a group's counter.</summary>
	public string Name { get; }

	/// <summary>Whether the carrier must have it.</summary>
	public bool Required { get; set; }

	/// <summary>A group's members, the first of which opens each entry; empty for a field or a component.</summary>
	public List<FixDictionaryMember> Members { get; } = [];

	internal static IEnumerable<FixDictionaryMember> Copy(List<FixDictionaryMember> members)
	{
		foreach (var member in members)
		{
			var copy = new FixDictionaryMember(member.Kind, member.Name, member.Required);

			copy.Members.AddRange(Copy(member.Members));

			yield return copy;
		}
	}
}
