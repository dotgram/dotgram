using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace DotGram.Finance.Fix44;

/// <summary>
/// A FIX dictionary as a file says it: the composition of each message, the components, and the
/// fields with their code sets, by name.
/// </summary>
/// <remarks>
/// <para>
/// The format is QuickFIX's: <c>&lt;fix&gt;</c> over <c>&lt;header&gt;</c>, <c>&lt;trailer&gt;</c>,
/// <c>&lt;messages&gt;</c>, <c>&lt;components&gt;</c> and <c>&lt;fields&gt;</c>. Any of the five may
/// be absent, because a file need not be a whole dictionary: a fragment that says what one venue
/// adds is read the same way, and what it does not mention it has no opinion about.
/// </para>
/// <para>
/// Nothing is resolved here. A message refers to its fields by name and this keeps the names; what
/// a name means is asked when a check is written from it, of the file first and of the standard
/// after. Where the file cannot be read the refusal names the element and the line.
/// </para>
/// </remarks>
sealed class FixDictionary
{
	/// <summary>One thing a scope holds: a field, a component, or a group with members of its own.</summary>
	public sealed class Member(int kind, string name, bool required)
	{
		public const int Field     = 0;
		public const int Component = 1;
		public const int Group     = 2;

		public readonly int          Kind     = kind;
		public readonly string       Name     = name;
		public readonly bool         Required = required;
		public readonly List<Member> Members  = [];
	}

	/// <summary>A field of the file: its name, the type it names, and the values it lists.</summary>
	public sealed class Field(string name, string? type, string[]? codes)
	{
		public readonly string    Name  = name;
		public readonly string?   Type  = type;
		public readonly string[]? Codes = codes;
	}

	public readonly List<Member>                                    Header     = [];
	public readonly List<Member>                                    Trailer    = [];
	public readonly List<(string Type, string Name, List<Member> Members)> Messages = [];
	public readonly Dictionary<string, List<Member>>                Components = new(StringComparer.Ordinal);
	public readonly Dictionary<int, Field>                          Fields     = [];
	public readonly Dictionary<string, int>                         Tags       = new(StringComparer.Ordinal);

	public static FixDictionary Parse(string text)
	{
		if (text == null) throw new ArgumentNullException(nameof(text));

		using var reader = XmlReader.Create(new StringReader(text), Settings());

		return Read(reader);
	}

	public static FixDictionary Read(TextReader input)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		using var reader = XmlReader.Create(input, Settings());

		return Read(reader);
	}

	public static FixDictionary Read(Stream input)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		using var reader = XmlReader.Create(input, Settings());

		return Read(reader);
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
				case "header":     Members(reader, read.Header);  break;
				case "trailer":    Members(reader, read.Trailer); break;
				case "messages":   ReadMessages(reader, read);   break;
				case "components": ReadComponents(reader, read); break;
				case "fields":     ReadFields(reader, read);     break;
				default:           reader.Skip();                 break;
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

			var name    = Required(reader, "name");
			var type    = Required(reader, "msgtype");
			var members = new List<Member>();

			Members(reader, members);
			read.Messages.Add((type, name, members));
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

			var name    = Required(reader, "name");
			var members = new List<Member>();

			Members(reader, members);

			if (read.Components.ContainsKey(name))
				throw Bad(reader, $"The component '{name}' is declared twice.");

			read.Components.Add(name, members);
		}
	}

	static void ReadFields(XmlReader reader, FixDictionary read)
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

			// A name is how every member refers to a field, so a name that means two tags would
			// make a message's composition ambiguous. The file is refused rather than read to
			// whichever of the two came last.
			if (read.Tags.TryGetValue(name, out var already))
				throw Bad(reader, $"The name '{name}' is given to tags {already} and {tag}.");

			read.Fields.Add(tag, new Field(name, type, codes));
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
					into.Add(new Member(Member.Field, Required(reader, "name"), Is(reader)));
					if (!reader.IsEmptyElement) reader.Skip();
					break;

				case "component":
					into.Add(new Member(Member.Component, Required(reader, "name"), Is(reader)));
					if (!reader.IsEmptyElement) reader.Skip();
					break;

				case "group":
				{
					var group = new Member(Member.Group, Required(reader, "name"), Is(reader));

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
}
