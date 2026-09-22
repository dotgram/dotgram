using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

using DotGram.ExpressionLanguage;

namespace DotGram.Finance.Fix;

/// <summary>
/// The slots a loaded dictionary fills: the same straight-line checks the package compiles in,
/// written as text from what the file says and compiled by the expression language.
/// </summary>
/// <remarks>
/// <para>
/// A dictionary is read into a copy of the slots it is loaded over, so that loading composes:
/// the standard, then a venue, then a rule a test adds, each a context built from the one before.
/// What a file does not mention it has no opinion about, and the slot keeps what it had.
/// </para>
/// <para>
/// What a message or a block requires <em>adds</em> to the check already in the slot, since a
/// venue asks for more than the standard and never less. What a field may hold <em>replaces</em>
/// it, since a venue that lists the values of a field lists all of them, and the standard's list
/// would refuse the ones it added.
/// </para>
/// </remarks>
partial class FixValidators
{
	static readonly Assembly Here = typeof(FixValidators).Assembly;

	/// <summary>The same slots in a new object, to be written to without touching this one.</summary>
	internal FixValidators Clone()
	{
		return (FixValidators)MemberwiseClone();
	}

	/// <summary>A copy of these slots with every check the dictionary describes added or replaced.</summary>
	/// <exception cref="FormatException">The file describes a type, a field or a block this package has no class for.</exception>
	internal FixValidators Load(FixDictionary dictionary)
	{
		var loaded = Clone();

		foreach (var (type, spelled, members) in dictionary.Messages)
		{
			// The MsgType is the key; a name is a dictionary's own spelling of it.
			var name = FixNames.MessageName(type)
				?? throw new FormatException($"The message '{spelled}' ({type}) is not a type this package has a class for.");
			var slot = typeof(FixValidators).GetProperty(name, BindingFlags.Public | BindingFlags.Instance)!;
			var text = MessageText(name, members, dictionary);

			if (text is not null)
				Add(loaded, slot, text);
		}

		foreach (var (name, members) in dictionary.Components)
		{
			var block = Here.GetType("DotGram.Finance.Fix.I" + name);

			// A repeating component is a group, and a group is checked where its entries are built;
			// only a block has a slot of its own. A component the standard does not know is refused,
			// since there is no carrier to ask it of.
			if (block is null)
			{
				if (FixNames.Tag("No" + name) == 0 && !IsGroupComponent(members))
					throw new FormatException($"The component '{name}' is not a block this package has a class for.");

				continue;
			}

			var slot = typeof(FixValidators).GetProperty(name, BindingFlags.Public | BindingFlags.Instance)!;
			var text = BlockText(name, block, members, dictionary);

			if (text is not null)
				Add(loaded, slot, text);
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

			var slot = typeof(FixValidators).GetProperty(name, BindingFlags.Public | BindingFlags.Instance)!;
			var text = FieldText(name, tag, field.Codes);

			if (text is not null)
				Replace(loaded, slot, text);
		}

		return loaded;
	}

	// A group in a component list is one whose members are indented under a counter; a file that
	// declares a repeating component gives it a group of the same counter.
	static bool IsGroupComponent(List<FixDictionary.Member> members)
	{
		return members.Count == 1 && members[0].Kind == FixDictionary.Member.Group;
	}

	static void Add(FixValidators into, PropertyInfo slot, string text)
	{
		var compiled = Compile(slot.PropertyType, text);
		var previous = (Delegate)slot.GetValue(into)!;

		// A multicast delegate calls both in order and answers with the last, which is the
		// message's own IsValid and so already counts what the first one found.
		slot.SetValue(into, Delegate.Combine(previous, compiled));
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

	static string? MessageText(string name, List<FixDictionary.Member> members, FixDictionary dictionary)
	{
		var body = new StringBuilder();
		var type = typeof(FixMessage).GetNestedType(name)!;

		Requirements(body, "message", type, members, dictionary, name);

		if (body.Length == 0)
			return null;

		return Using + "(FixContext context, FixMessage." + name + " message) => {\n" + body + "return message.IsValid; }";
	}

	static string? BlockText(string name, Type block, List<FixDictionary.Member> members, FixDictionary dictionary)
	{
		var body = new StringBuilder();

		Requirements(body, "block", block, members, dictionary, name);

		if (body.Length == 0)
			return null;

		return Using + "(FixContext context, FixMessage message, I" + name + " block) => {\n" + body + "return message.IsValid; }";
	}

	// One line a required member: a field or a group counter that is absent, or a block of which
	// no field is present. An optional member says nothing a slot needs to hear.
	static void Requirements(StringBuilder body, string subject, Type carrier, List<FixDictionary.Member> members, FixDictionary dictionary, string where)
	{
		foreach (var member in members)
		{
			if (!member.Required)
				continue;

			switch (member.Kind)
			{
				case FixDictionary.Member.Field:
				case FixDictionary.Member.Group:
				{
					var tag  = Tag(member.Name, dictionary);
					var ours = FixNames.Name(tag)!;

					if (carrier.GetProperty(ours) is null)
						throw new FormatException($"'{where}' has no field '{member.Name}' ({tag}) for the dictionary to require.");

					body.Append("if (").Append(subject).Append('.').Append(ours).Append(" == null) message.AddFinding(new FixFinding(FixRule.RequiredFieldMissing, ")
						.Append(tag.ToString(CultureInfo.InvariantCulture)).Append(", 0, null, -1));\n");
					break;
				}

				case FixDictionary.Member.Component:
				{
					var block = Here.GetType("DotGram.Finance.Fix.I" + member.Name);

					if (block is null)
					{
						// A required repeating component is a required counter.
						if (dictionary.Components.TryGetValue(member.Name, out var inner) && IsGroupComponent(inner))
						{
							var tag     = Tag(inner[0].Name, dictionary);
							var counter = FixNames.Name(tag)!;

							if (carrier.GetProperty(counter) is null)
								throw new FormatException($"'{where}' has no group '{inner[0].Name}' ({tag}) for the dictionary to require.");

							body.Append("if (").Append(subject).Append('.').Append(counter).Append(" == null) message.AddFinding(new FixFinding(FixRule.RequiredFieldMissing, ")
								.Append(tag.ToString(CultureInfo.InvariantCulture)).Append(", 0, null, -1));\n");
							break;
						}

						throw new FormatException($"The component '{member.Name}' is not a block this package has a class for.");
					}

					if (!block.IsAssignableFrom(carrier))
						throw new FormatException($"'{where}' does not carry the block '{member.Name}' for the dictionary to require.");

					var fields = block.GetProperties();

					body.Append("if (");

					for (var i = 0; i < fields.Length; i++)
					{
						if (i > 0) body.Append(" && ");
						body.Append(subject).Append('.').Append(fields[i].Name).Append(" == null");
					}

					body.Append(") message.AddFinding(new FixFinding(FixRule.RequiredComponentMissing, ")
						.Append(FixNames.Tag(fields[0].Name).ToString(CultureInfo.InvariantCulture)).Append(", 0, null, -1));\n");
					break;
				}
			}
		}
	}

	static string? FieldText(string name, int tag, string[] codes)
	{
		var field = typeof(FixField).GetNestedType(name)!;
		var value = field.BaseType!.GetGenericArguments()[0];
		var body  = new StringBuilder();
		var bad   = "message.AddFinding(new FixFinding(FixRule.InvalidValue, " + tag.ToString(CultureInfo.InvariantCulture) + ", field.Position, field, -1));";

		body.Append("if (!field.IsValid) ").Append(bad).Append('\n');

		if (value == typeof(bool))
			return null;

		if (value == typeof(string[]))
		{
			body.Append("else foreach (var code in field.Value) if (");
			Codes(body, "code", codes, value);
			body.Append(") { ").Append(bad).Append(" break; }\n");
		}
		else
		{
			body.Append("else if (");
			Codes(body, "field.Value", codes, value);
			body.Append(") ").Append(bad).Append('\n');
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
		if (dictionary.Tags.TryGetValue(name, out var tag))
			return tag;

		if (dictionary.Tags.TryGetValue(name, out tag) || (tag = FixNames.Tag(name)) != 0)
			return FixNames.Name(tag) is not null ? tag : throw new FormatException($"The field '{name}' ({tag}) is not a field this package has a class for.");

		throw new FormatException($"The name '{name}' is not a field this package has a class for, and the dictionary does not number it.");
	}
}
