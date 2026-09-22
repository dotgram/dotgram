using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using DotGram.ExpressionLanguage;

namespace DotGram.Finance.Fix;

/// <summary>
/// The slots a loaded dictionary fills: the same straight-line checks the package compiles in,
/// written as text from what the file says and compiled by the expression language.
/// </summary>
/// <remarks>
/// <para>
/// A dictionary is read into a copy of the slots it is loaded over, so that the context loaded
/// over is unchanged and a second load starts from the first. A slot the file describes is
/// <em>replaced</em>: the check of a message type, of a block, or of a field the file describes
/// is the file's whole check, and what it does not say the slot no longer asks. A file that
/// mentions no message, block or field leaves that slot as it was.
/// </para>
/// <para>
/// A message's check written from the file has the shape of the compiled-in one: what the file
/// marks required is asked for, every field it lists is handed to that field's slot, every
/// block it lists to that block's slot, and every group's count is held to its entries. A block's
/// check hands its own fields to their slots the same way, so that a field is asked once,
/// whichever of the two lists it.
/// </para>
/// </remarks>
partial class FixValidators
{
	static readonly Assembly Here = typeof(FixValidators).Assembly;

	/// <summary>
	/// What the dictionary said that this package has no place for: a member of a message or a block
	/// that the class here does not carry, one line each, in the order the file said them.
	/// </summary>
	/// <remarks>
	/// A file and the repository this package was written from can place a field differently —
	/// QuickFIX/n has NoQuoteQualifiers on a QuoteRequestReject where the repository has it inside
	/// the group — and a rule about a field a class cannot hold is a rule with nothing to hold. It is
	/// not a refusal of the whole file, and it is not passed over in silence: it is here to be read.
	/// </remarks>
	internal List<string> Unplaced { get; private set; } = [];

	/// <summary>The same slots in a new object, to be written to without touching this one.</summary>
	internal FixValidators Clone()
	{
		return (FixValidators)MemberwiseClone();
	}

	/// <summary>A copy of these slots with every check the dictionary describes replaced by the file's.</summary>
	/// <exception cref="FormatException">The file describes a type, a field or a block this package has no class for.</exception>
	internal FixValidators Load(FixDictionary dictionary)
	{
		var loaded = Clone();

		loaded.Unplaced = [.. Unplaced];

		foreach (var (type, spelled, members) in dictionary.Messages)
		{
			// The MsgType is the key; a name is a dictionary's own spelling of it.
			var name = FixNames.MessageName(type)
				?? throw new FormatException($"The message '{spelled}' ({type}) is not a type this package has a class for.");

			Replace(loaded, Slot(name), MessageText(name, members, dictionary, loaded));
		}

		foreach (var (name, members) in dictionary.Components)
		{
			var block = Here.GetType("DotGram.Finance.Fix.I" + name);

			// A repeating component is a group, and a group is checked where its entries are built;
			// only a block has a slot of its own.
			if (block is null)
			{
				if (!IsGroupComponent(members))
					loaded.Unplaced.Add($"component {name}: not a block this package has a class for");

				continue;
			}

			Replace(loaded, Slot(name), BlockText(name, block, members, dictionary, loaded));
		}

		foreach (var (tag, field) in dictionary.Fields)
		{
			if (field.Codes is null)
				continue;

			// The number is the key; a name is a dictionary's own spelling of it. A tag this package has
			// no class for has no slot and nothing to limit: no message of this package builds it, so a
			// file that lists its values is describing a field that arrives as Invalid whatever it says.
			// It is a refusal only where a message is made to require it, which is a rule with nothing
			// to hold.
			var name = FixNames.Name(tag);

			if (name is null)
				continue;

			var text = FieldText(name, tag, field.Codes);

			if (text is not null)
				Replace(loaded, Slot(name), text);
		}

		return loaded;
	}

	static PropertyInfo Slot(string name)
	{
		return typeof(FixValidators).GetProperty(name, BindingFlags.Public | BindingFlags.Instance)
			?? throw new InvalidOperationException($"No slot is called '{name}'.");
	}

	// A group in a component list is one whose members are indented under a counter; a file that
	// declares a repeating component gives it a group of the same counter.
	static bool IsGroupComponent(List<FixDictionary.Member> members)
	{
		return members.Count == 1 && members[0].Kind == FixDictionary.Member.Group;
	}

	static void Replace(FixValidators into, PropertyInfo slot, string text)
	{
		slot.SetValue(into, Compile(slot.PropertyType, text));
	}

	static Delegate Compile(Type delegateType, string text)
	{
		var method = typeof(ExpressionParser)
			.GetMethod(nameof(ExpressionParser.Compile), [typeof(string), typeof(Assembly)])!
			.MakeGenericMethod(delegateType);

		try
		{
			return (Delegate)method.Invoke(null, [text, Here])!;
		}
		catch (TargetInvocationException e) when (e.InnerException is not null)
		{
			throw new FormatException("A check written from the dictionary could not be compiled: " + e.InnerException.Message + Environment.NewLine + text, e.InnerException);
		}
	}

	// ── the texts ────────────────────────────────────────────────────────────────────────────

	const string Using = "using DotGram.Finance.Fix;\n";

	static string MessageText(string name, List<FixDictionary.Member> members, FixDictionary dictionary, FixValidators loaded)
	{
		var body = new StringBuilder();

		Members(body, "message", typeof(FixMessage).GetNestedType(name)!, members, dictionary, name, loaded);

		return Using + "(FixContext context, FixMessage." + name + " message) => {\n" + body + "return message.IsValid; }";
	}

	static string BlockText(string name, Type block, List<FixDictionary.Member> members, FixDictionary dictionary, FixValidators loaded)
	{
		var body = new StringBuilder();

		Members(body, "block", block, members, dictionary, name, loaded);

		return Using + "(FixContext context, FixMessage message, I" + name + " block) => {\n" + body + "return message.IsValid; }";
	}

	// The shape of a compiled-in check, one member at a time: a field is asked for when required
	// and handed to its slot when present; a group's counter likewise, and its count held to its
	// entries; a block is asked for when required and handed to its slot when present.
	static void Members(StringBuilder body, string subject, Type carrier, List<FixDictionary.Member> members, FixDictionary dictionary, string where, FixValidators loaded)
	{
		var unplaced = loaded.Unplaced;

		foreach (var member in members)
		{
			switch (member.Kind)
			{
				case FixDictionary.Member.Field:
				{
					var tag  = Tag(member.Name, dictionary);
					var ours = FixNames.Name(tag)!;

					if (carrier.GetProperty(ours) is null)
					{
						unplaced.Add($"{where}: field {member.Name} ({tag})" + (member.Required ? ", required" : ""));
						break;
					}

					if (member.Required && subject == "entry")
						body.Append("if (entry.").Append(ours).Append(" == null) FixValidators.Missing(message, ").Append(tag.ToString(CultureInfo.InvariantCulture)).Append(", ").Append(Opened(carrier)).Append(", index);\n");
					else if (member.Required)
						body.Append("if (").Append(subject).Append('.').Append(ours).Append(" == null) FixValidators.Missing(message, ").Append(tag.ToString(CultureInfo.InvariantCulture)).Append(");\n");

					body.Append("if (").Append(subject).Append('.').Append(ours).Append(" != null) context.Validators.").Append(ours)
						.Append(".Invoke(context, message, ").Append(subject).Append('.').Append(ours).Append(");\n");
					break;
				}

				case FixDictionary.Member.Group:
					Group(body, subject, carrier, member.Name, member.Required, member.Members, dictionary, where, loaded);
					break;

				case FixDictionary.Member.Component:
				{
					var block = Here.GetType("DotGram.Finance.Fix.I" + member.Name);

					if (block is null)
					{
						// A repeating component is its group, declared once under its own name.
						if (dictionary.Components.TryGetValue(member.Name, out var inner) && IsGroupComponent(inner))
						{
							Group(body, subject, carrier, inner[0].Name, member.Required, inner[0].Members, dictionary, where, loaded);
							break;
						}

						unplaced.Add($"{where}: component {member.Name}" + (member.Required ? ", required" : ""));
						break;
					}

					if (!block.IsAssignableFrom(carrier))
					{
						unplaced.Add($"{where}: block {member.Name}" + (member.Required ? ", required" : ""));
						break;
					}

					var fields = block.GetProperties();
					var empty  = new StringBuilder();

					for (var i = 0; i < fields.Length; i++)
					{
						if (i > 0) empty.Append(" && ");
						empty.Append(subject).Append('.').Append(fields[i].Name).Append(" == null");
					}

					if (member.Required)
						body.Append("if (").Append(empty).Append(") FixValidators.Absent(message, ").Append(FixNames.Tag(fields[0].Name).ToString(CultureInfo.InvariantCulture)).Append(");\n")
							.Append("else context.Validators.").Append(member.Name).Append(".Invoke(context, message, ").Append(subject).Append(");\n");
					else
						body.Append("if (!(").Append(empty).Append(")) context.Validators.").Append(member.Name).Append(".Invoke(context, message, ").Append(subject).Append(");\n");
					break;
				}
			}
		}
	}

	// A group: its counter is a field of the carrier, required or not, and its entries are the list
	// whose entry type is opened by the group's first member, which is how the wire cuts them.
	static void Group(StringBuilder body, string subject, Type carrier, string counterName, bool required, List<FixDictionary.Member> members, FixDictionary dictionary, string where, FixValidators loaded)
	{
		var unplaced = loaded.Unplaced;

		var counterTag = Tag(counterName, dictionary);
		var counter    = FixNames.Name(counterTag)!;

		if (carrier.GetProperty(counter) is null)
		{
			unplaced.Add($"{where}: group {counterName} ({counterTag})" + (required ? ", required" : ""));

			return;
		}

		if (required)
			body.Append("if (").Append(subject).Append('.').Append(counter).Append(" == null) FixValidators.Missing(message, ").Append(counterTag.ToString(CultureInfo.InvariantCulture)).Append(");\n");

		var opener = Opener(members, dictionary);
		var list   = opener is null ? null : EntriesOpenedBy(carrier, opener);

		if (list is null)
			return;

		var entryType = carrier.GetProperty(list)!.PropertyType.GetGenericArguments()[0];

		body.Append("FixValidators.Counted(message, ").Append(subject).Append('.').Append(counter).Append(", ").Append(subject).Append('.').Append(list).Append(");\n")
			.Append("if (").Append(subject).Append('.').Append(list).Append(" != null) for (var i = 0; i < ").Append(subject).Append('.').Append(list).Append(".Count; i++) context.Validators.")
			.Append(entryType.Name).Append(".Invoke(context, message, ").Append(subject).Append('.').Append(list).Append("[i], i);\n");

		// What the file says of the entry is the entry's check, replaced like any other slot. A group
		// named with no members says only that it is there.
		if (members.Count > 0)
		{
			var entry = new StringBuilder();

			Members(entry, "entry", entryType, members, dictionary, where + "/" + counterName, loaded);
			Replace(loaded, Slot(entryType.Name), Using + "(FixContext context, FixMessage message, FixGroup." + entryType.Name + " entry, int index) => {\n" + entry + "return message.IsValid; }");
		}
	}

	// The field an entry opens with: the first member of the group, through a block to its first
	// field and through a nested group to its counter, since the wire cuts entries by the first tag.
	static string? Opener(List<FixDictionary.Member> members, FixDictionary dictionary)
	{
		if (members.Count == 0)
			return null;

		var first = members[0];

		switch (first.Kind)
		{
			case FixDictionary.Member.Field:
			case FixDictionary.Member.Group:
				return FixNames.Name(Tag(first.Name, dictionary));

			default:
				if (Here.GetType("DotGram.Finance.Fix.I" + first.Name) is { } block)
					return block.GetProperties()[0].Name;

				return dictionary.Components.TryGetValue(first.Name, out var inner) ? Opener(inner, dictionary) : null;
		}
	}

	// Where an entry began: the position of the field it opened with, or zero where the entry type
	// names no such field.
	static string Opened(Type entryType)
	{
		foreach (var property in entryType.GetProperties())
			if (property.GetCustomAttribute<RequiredMemberAttribute>() is not null)
				return "entry." + property.Name + ".Position";

		return "0";
	}

	// The list property of a carrier whose entry type is opened by a field of that name: the
	// delimiter is required on the entry, since the wire marks no other boundary.
	static string? EntriesOpenedBy(Type carrier, string delimiter)
	{
		foreach (var property in carrier.GetProperties())
		{
			var type = property.PropertyType;

			if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(List<>))
				continue;

			var opener = type.GetGenericArguments()[0].GetProperty(delimiter);

			if (opener is not null && opener.GetCustomAttribute<RequiredMemberAttribute>() is not null)
				return property.Name;
		}

		return null;
	}

	static string? FieldText(string name, int tag, string[] codes)
	{
		var field = typeof(FixField).GetNestedType(name)!;
		var value = field.BaseType!.GetGenericArguments()[0];
		var body  = new StringBuilder();

		if (value == typeof(bool))
			return null;

		body.Append("if (!field.IsValid) FixValidators.Invalid(message, field);\n");

		if (value == typeof(string[]))
		{
			body.Append("else foreach (var code in field.Value) if (");
			Codes(body, "code", codes, value);
			body.Append(") { FixValidators.Invalid(message, field); break; }\n");
		}
		else
		{
			body.Append("else if (");
			Codes(body, "field.Value", codes, value);
			body.Append(") FixValidators.Invalid(message, field);\n");
		}

		return Using + "(FixContext context, FixMessage message, FixField." + name + " field) => {\n" + body + "return message.IsValid; }";
	}

	// The value is none of the codes: one inequality a code, in the literal form its type reads.
	static void Codes(StringBuilder body, string subject, string[] codes, Type value)
	{
		for (var i = 0; i < codes.Length; i++)
		{
			if (i > 0) body.Append(" && ");

			body.Append(subject).Append(" != ");

			if (value == typeof(char))
				body.Append('\'').Append(codes[i] == "\\" || codes[i] == "'" ? "\\" + codes[i] : codes[i]).Append('\'');
			else if (value == typeof(string) || value == typeof(string[]))
				body.Append('"').Append(codes[i].Replace("\\", "\\\\").Replace("\"", "\\\"")).Append('"');
			else
				body.Append(codes[i]);
		}
	}

	static int Tag(string name, FixDictionary dictionary)
	{
		if (dictionary.Tags.TryGetValue(name, out var tag) || (tag = FixNames.Tag(name)) != 0)
			return FixNames.Name(tag) is not null ? tag : throw new FormatException($"The field '{name}' ({tag}) is not a field this package has a class for.");

		throw new FormatException($"The name '{name}' is not a field this package has a class for, and the dictionary does not number it.");
	}
}
