using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DotGram.Finance.Fix;

/// <summary>
/// Writes a dictionary's rules for one message type as the text of a rule, to be compiled.
/// </summary>
/// <remarks>
/// <para>
/// The text is two lambdas, one inside the other. The outer one takes the tables and builds what
/// is constant for the whole rule — the code sets, and the tag lists a component's presence is
/// asked of — then returns the inner one, which is the rule itself and captures them. Constants
/// built once at load and captured are what keeps them out of a message's way: the same array
/// written inline would be built again for every message validated.
/// </para>
/// <para>
/// <strong>Every arm says where it came from.</strong> The text is compiled in the consumer's own
/// process, so a refusal is reported about code they did not write; a comment naming the
/// dictionary record — the message, the component, the tag, in the file's own spelling — is what
/// makes a line number mean something. <see cref="FixRuleText.Blank"/> removes the comments
/// before compiling by turning them into spaces, so the line and column a compiler names are the
/// line and column a reader is looking at.
/// </para>
/// <para>
/// What the schema says becomes code. What it does not say — a duplicate tag, a length/data pair,
/// a count against its entries — stays in <see cref="FixRuleSupport"/> and is called: it is the
/// same work for every message, and ninety-three copies of it would be ninety-three things to
/// disagree with each other.
/// </para>
/// </remarks>
sealed class FixRuleComposer(FixDictionary dictionary, int lean = 0)
{
	/// <summary>
	/// How much of the rule to leave out: 0 all of it, 1 without the value checks and the
	/// length/data scan, 2 without the membership switch as well.
	/// </summary>
	/// <remarks>
	/// Not an option anybody is offered: it exists so that the SAME rules can be built small and
	/// large and the difference measured. A compiled rule's price turned out to follow how many of
	/// them are hot rather than what they do, which is a claim about code — and a claim about code
	/// is tested by taking code away, a layer at a time, and nothing else with it.
	/// </remarks>
	readonly int lean = lean;

	readonly List<string>    constants = [];
	readonly HashSet<string> declared  = [];

	StringBuilder text = new();

	/// <summary>The text of the rule for one message type, ready to be compiled.</summary>
	/// <param name="messageType">The <c>MsgType</c> as the dictionary spells it.</param>
	public string Rule(string messageType)
	{
		if (messageType is null) throw new ArgumentNullException(nameof(messageType));

		constants.Clear();
		declared.Clear();

		var name = dictionary.MessageName(messageType) ?? messageType;
		var body = new StringBuilder();

		// The body is written first, because writing it is what discovers which constants it needs.
		text = body;

		Line(2, "FixRuleSupport.Message(tables, message, found);");
		Blank();

		Scope(2, "message.Header", dictionary.Header, FixScope.Header, "the standard header", 0, "-1", 0);
		Scope(2, "message", dictionary.Message(messageType), FixScope.Body, $"message '{messageType}' - {name}", 0, "-1", 0);
		Scope(2, "message.Trailer", dictionary.Trailer, FixScope.Trailer, "the standard trailer", 0, "-1", 0);

		var whole = new StringBuilder();

		text = whole;

		Line(0, "using System;");
		Line(0, "using System.Collections.Generic;");
		Blank();
		Line(0, "using DotGram.Finance.Fix;");
		Blank();
		Comment(0, $"{dictionary.Version}: the rule for message '{messageType}' - {name}");
		Line(0, "(FixTables tables) =>");
		Line(0, "{");

		foreach (var constant in constants)
			whole.Append(constant);

		if (constants.Count != 0)
			Blank();

		Line(1, "return (FixMessage message, List<FixFinding> found) =>");
		Line(1, "{");

		whole.Append(body);

		Line(1, "};");
		Line(0, "}");

		return whole.ToString();
	}

	/// <summary>
	/// How many arms that message type's body switch has, which is what its text costs most.
	/// </summary>
	/// <remarks>
	/// For measuring, not for composing: the question behind it is whether a compiled rule's price
	/// follows the number of tags it can name, which is a property of the schema and not of any
	/// message.
	/// </remarks>
	public int Arms(string messageType) => Members(dictionary.Message(messageType)).Count;

	/// <summary>
	/// One scope: the header, the body, the trailer, or one entry of a repeating group.
	/// </summary>
	/// <remarks>
	/// Every local the scope declares carries its depth in groups, because a group inside a group
	/// is written inside the loop that walks the outer one: a name declared twice there is the
	/// inner one hiding the outer, which a compiler would refuse and a reader would misread.
	/// </remarks>
	void Scope(int at, string source, SchemaRef[] schema, FixScope where, string what, int groupTag, string entry, int depth)
	{
		var scope = "scope" + depth;
		var seen  = "seen"  + depth;
		var rank  = "rank"  + depth;

		Comment(at, what);
		Line(at, "{");
		Line(at + 1, $"FixFieldSet {scope} = {source};");
		Line(at + 1, $"ulong[] {seen} = new ulong[15];");

		if (groupTag != 0 && lean < 2)
			Line(at + 1, $"int {rank} = -1;");

		Blank();
		Line(at + 1, $"foreach (FixNode node in {scope}.Nodes)");
		Line(at + 1, "{");
		Line(at + 2, $"FixFieldView field = node.Field({scope}.Source);");
		Blank();
		// Level 2 writes no switch at all. It is not a validator -- it cannot say that a tag does not
		// belong -- and exists only to ask whether the switch is what the wide runs are paying for.
		if (lean < 2)
		{
			Line(at + 2, "switch (node.Tag)");
			Line(at + 2, "{");

			var ranks = groupTag == 0 ? null : Ranks(schema);

			foreach (var tag in Members(schema))
				Case(at + 3, tag, where, ranks, rank, groupTag, entry);

			Line(at + 3, "default:");
			Line(at + 4, $"FixRuleSupport.NotInScope(tables, found, {Where(where)}, field, {groupTag}, {entry});");
			Line(at + 4, "break;");
			Line(at + 2, "}");
			Blank();
		}
		Line(at + 2, $"if (FixRuleSupport.Seen({seen}, node.Tag))");
		Line(at + 3, $"found.Add(FixRuleSupport.Wrong(FixRule.DuplicateField, {Where(where)}, field, {groupTag}, {entry},");
		Line(at + 4, "\"The tag appears more than once in this scope.\"));");
		Blank();
		Line(at + 2, $"FixRuleSupport.Mark({seen}, node.Tag);");
		Line(at + 1, "}");
		Blank();
		if (lean == 0)
		{
			Line(at + 1, $"FixRuleSupport.Pairs(tables, found, {scope}, {Where(where)}, {groupTag}, {entry});");
			Blank();
		}

		References(at + 1, schema, where, groupTag, entry, depth);

		Line(at, "}");
		Blank();
	}

	/// <summary>One arm of a scope's switch: what the schema says about that tag, and nothing more.</summary>
	void Case(int at, int tag, FixScope where, Dictionary<int, int>? ranks, string rank, int groupTag, string entry)
	{
		var name = dictionary.Name(tag) ?? "a tag the dictionary does not name";

		Line(at, $"case {tag}:{Trailing($"{name}, tag {tag}")}");

		if (ranks != null && ranks.TryGetValue(tag, out var ordinal))
		{
			Line(at + 1, $"if ({ordinal} < {rank})");
			Line(at + 2, $"found.Add(FixRuleSupport.Wrong(FixRule.FieldOutOfOrder, {Where(where)}, field, {groupTag}, {entry},");
			Line(at + 3, "\"The fields of a group entry are not in the order the schema gives them.\"));");
			Line(at + 1, "else");
			Line(at + 2, $"{rank} = {ordinal};");
			Blank();
		}

		var type = lean > 0 ? FixValueType.None : FixVocabulary.Of(dictionary.CodeType(tag));

		if (type != FixValueType.None)
		{
			var codes = dictionary.CodeArray(tag);
			var set   = codes is null ? "null" : Codes(tag, codes, name);

			Line(at + 1, $"if (!FixValues.Valid(field, FixValueType.{type}, {set}))");
			Line(at + 2, $"found.Add(FixRuleSupport.Wrong(FixRule.InvalidValue, {Where(where)}, field, {groupTag}, {entry},");
			Line(at + 3, "\"The value does not fit the field's type, or is not one of its code set.\"));");
			Blank();
		}

		Line(at + 1, "break;");
		Blank();
	}

	/// <summary>
	/// What a scope requires: its fields, its components, and the groups whose entries are walked.
	/// </summary>
	void References(int at, SchemaRef[] schema, FixScope where, int groupTag, string entry, int depth)
	{
		var scope = "scope" + depth;
		var seen  = "seen"  + depth;

		foreach (var reference in schema)
		{
			if (reference.Kind == 1)
			{
				var child = dictionary.Component(reference.Id);
				var name  = dictionary.ComponentName(reference.Id);
				var tags  = Members(child);

				// A component nobody requires, holding nothing that is required and no group, has
				// nothing to say: the presence test would open a block with nothing in it, which is
				// not a program and would not be read as one.
				var says = Says(child);

				if (tags.Count == 0 || !says && !reference.Required)
					continue;

				Comment(at, $"component '{name}'{(reference.Required ? ", which this scope requires" : "")}");

				var any = $"FixRuleSupport.Any({scope}, {seen}, {Component(reference.Id, name, tags)})";

				if (!says)
				{
					Line(at, $"if (!{any})");
					Line(at + 1, $"found.Add(FixRuleSupport.Absent({Where(where)}, {groupTag}, {entry},");
					Line(at + 2, $"\"A component the schema requires has none of its fields; its first is tag {tags[0]}.\"));");
					Blank();

					continue;
				}

				Line(at, $"if ({any})");
				Line(at, "{");

				References(at + 1, child, where, groupTag, entry, depth);

				Line(at, "}");

				if (reference.Required)
				{
					Line(at, "else");
					Line(at + 1, $"found.Add(FixRuleSupport.Absent({Where(where)}, {groupTag}, {entry},");
					Line(at + 2, $"\"A component the schema requires has none of its fields; its first is tag {tags[0]}.\"));");
				}

				Blank();

				continue;
			}

			var tag = reference.Kind == 2 ? dictionary.Counter(reference.Id) : reference.Id;

			if (reference.Kind == 2)
			{
				Group(at, reference, tag, where, groupTag, entry, depth);

				continue;
			}

			if (!reference.Required)
				continue;

			Comment(at, $"{dictionary.Name(tag) ?? "a tag the dictionary does not name"}, tag {tag}, which this scope requires");
			Line(at, $"if (!FixRuleSupport.Has({scope}, {seen}, {tag}))");
			Line(at + 1, $"found.Add(FixRuleSupport.Missing(FixRule.RequiredFieldMissing, {Where(where)}, {tag}, {groupTag}, {entry},");
			Line(at + 2, "\"The schema requires this tag in this scope.\"));");
			Blank();
		}
	}

	/// <summary>A repeating group: its count, and every entry walked as a scope of its own.</summary>
	void Group(int at, SchemaRef reference, int tag, FixScope where, int groupTag, string entry, int depth)
	{
		var scope   = "scope"   + depth;
		var seen    = "seen"    + depth;
		var counter = "counter" + depth;
		var entries = "entries" + depth;
		var index   = "index"   + depth;
		var name    = dictionary.Name(tag) ?? tag.ToString(CultureInfo.InvariantCulture);

		Comment(at, $"group '{name}', counted by tag {tag}{(reference.Required ? ", which this scope requires" : "")}");
		Line(at, $"if (FixRuleSupport.Has({scope}, {seen}, {tag}))");
		Line(at, "{");
		Line(at + 1, $"FixFieldView {counter} = FixRuleSupport.Field({scope}, {tag});");
		Line(at + 1, $"IReadOnlyList<FixFieldSet> {entries} = {scope}.GetGroup({tag});");
		Blank();
		Line(at + 1, $"FixRuleSupport.Count(found, {Where(where)}, {counter}, {entries}, {groupTag}, {entry}, {(reference.Required ? "true" : "false")});");
		Blank();
		Line(at + 1, $"int {index} = 0;");
		Blank();
		Line(at + 1, $"foreach (FixFieldSet one in {entries})");
		Line(at + 1, "{");

		// An entry is a scope like any other, with two differences the schema decides: its fields
		// are held to the schema's order, and its findings carry the group and the entry they are in.
		Scope(at + 2, "one", dictionary.Group(reference.Id), where, $"one entry of group '{name}'", tag, index, depth + 1);

		Line(at + 2, $"{index} = {index} + 1;");
		Line(at + 1, "}");
		Line(at, "}");

		if (reference.Required)
		{
			Line(at, "else");
			Line(at + 1, $"found.Add(FixRuleSupport.Missing(FixRule.RequiredFieldMissing, {Where(where)}, {tag}, {groupTag}, {entry},");
			Line(at + 2, "\"The schema requires this tag in this scope.\"));");
		}

		Blank();
	}

	// ── the constants the outer lambda holds ────────────────────────────────────────────────

	string Codes(int tag, string[] codes, string name)
	{
		var identifier = "codes" + tag.ToString(CultureInfo.InvariantCulture);

		if (declared.Add(identifier))
		{
			var one = new StringBuilder();

			one.Append('\t').Append("string[] ").Append(identifier).Append(" = new string[] { ");

			for (var i = 0; i < codes.Length; i++)
			{
				if (i != 0)
					one.Append(", ");

				FixRuleText.Literal(one, codes[i]);
			}

			one.Append(" };   // ").Append(FixRuleText.Note($"the code set of {name}, tag {tag}")).Append('\n');

			constants.Add(one.ToString());
		}

		return identifier;
	}

	string Component(int id, string name, List<int> tags)
	{
		var identifier = "part" + id.ToString(CultureInfo.InvariantCulture);

		if (declared.Add(identifier))
		{
			var one = new StringBuilder();

			one.Append('\t').Append("int[] ").Append(identifier).Append(" = new int[] { ");

			for (var i = 0; i < tags.Count; i++)
			{
				if (i != 0)
					one.Append(", ");

				one.Append(tags[i].ToString(CultureInfo.InvariantCulture));
			}

			one.Append(" };   // ").Append(FixRuleText.Note($"the tags of component '{name}'")).Append('\n');

			constants.Add(one.ToString());
		}

		return identifier;
	}

	// ── what the schema says, resolved here so that the text never has to ask ───────────────

	/// <summary>
	/// Whether a scope has anything to say beyond what its switch already said: a required field,
	/// a group to walk, or a component that has one of those.
	/// </summary>
	bool Says(SchemaRef[] schema)
	{
		foreach (var reference in schema)
			if (reference.Kind == 2 ||
				reference.Kind == 0 && reference.Required ||
				reference.Kind == 1 && (reference.Required || Says(dictionary.Component(reference.Id))))
				return true;

		return false;
	}

	/// <summary>Every tag a scope holds, components expanded, in the schema's order.</summary>
	List<int> Members(SchemaRef[] schema)
	{
		var members = new List<int>();
		var seen     = new HashSet<int>();

		void Add(SchemaRef[] refs)
		{
			foreach (var reference in refs)
			{
				if (reference.Kind == 1)
				{
					Add(dictionary.Component(reference.Id));

					continue;
				}

				var tag = reference.Kind == 2 ? dictionary.Counter(reference.Id) : reference.Id;

				if (seen.Add(tag))
					members.Add(tag);
			}
		}

		Add(schema);

		return members;
	}

	/// <summary>Each tag's place in the schema's order, which a group entry is held to.</summary>
	Dictionary<int, int> Ranks(SchemaRef[] schema)
	{
		var ranks   = new Dictionary<int, int>();
		var ordinal = 0;

		void Add(SchemaRef[] refs)
		{
			foreach (var reference in refs)
			{
				if (reference.Kind == 1)
				{
					Add(dictionary.Component(reference.Id));

					continue;
				}

				var tag = reference.Kind == 2 ? dictionary.Counter(reference.Id) : reference.Id;

				if (!ranks.ContainsKey(tag))
					ranks.Add(tag, ordinal);

				ordinal++;
			}
		}

		Add(schema);

		return ranks;
	}

	// ── writing ─────────────────────────────────────────────────────────────────────────────

	static string Where(FixScope where) => "FixScope." + where;

	void Line(int at, string line) => text.Append('\t', at).Append(line).Append('\n');

	void Blank() => text.Append('\n');

	void Comment(int at, string what) => text.Append('\t', at).Append("// ").Append(FixRuleText.Note(what)).Append('\n');

	static string Trailing(string what) => "   // " + FixRuleText.Note(what);
}
