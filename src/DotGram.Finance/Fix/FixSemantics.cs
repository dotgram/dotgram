using System;
using System.Runtime.CompilerServices;

namespace DotGram.Finance.Fix;

/// <summary>Interprets a flat sequence of fields using the FIX message schema.</summary>
static class FixSemantics
{
	public static bool TryBuild(string source, string type, FixNode[] fields, FixParseOptions? options, out FixMessage? message, out FixParseError? error)
	{
		message = null;

		var reader  = new Reader(source, type, fields, options);
		var header  = reader.Scope(FixSchema.Component(1024));
		var body    = reader.Scope(FixSchema.Message(type), body: true);
		var trailer = reader.Scope(FixSchema.Component(1025));

		if (reader.Position != fields.Length && reader.Error == null)
		{
			var field = fields[reader.Position];
			reader.Fail(field.Tag, field.Position, "Field is not permitted in this message scope.");
		}

		error = reader.Error;

		if (error != null)
			return false;

		message = FixMessageFactory.Message(type, source, header, body, trailer);

		return true;
	}

	static readonly ConditionalWeakTable<SchemaRef[], Dictionary<int, int>> scopes = new();

	// Also what the validator asks whether a tag belongs in a scope. One map, so construction and
	// checking cannot disagree about what a scope contains.
	internal static Dictionary<int, int> Members(SchemaRef[] schema)
	{
		return scopes.GetValue(schema, CreateMembers);
	}

	static Dictionary<int, int> CreateMembers(SchemaRef[] schema)
	{
		var members = new Dictionary<int, int>();
		Add(schema, members);
		return members;
	}

	static void Add(SchemaRef[] schema, Dictionary<int, int> members)
	{
		foreach (var reference in schema)
		{
			if (reference.Kind == 1)
			{
				if (reference.Id is not (1024 or 1025)) Add(FixSchema.Component(reference.Id), members);
			}
			else members[reference.Kind == 2 ? FixSchema.Counter(reference.Id) : reference.Id] = reference.Kind == 2 ? reference.Id : 0;
		}
	}

	static int First(SchemaRef[] schema)
	{
		foreach (var reference in schema)
		{
			if (reference.Kind != 1) return reference.Kind == 2 ? FixSchema.Counter(reference.Id) : reference.Id;
			var first = First(FixSchema.Component(reference.Id));
			if (first != 0) return first;
		}
		return 0;
	}

	sealed class Reader
	{
		readonly string source;
		readonly string type;
		readonly FixNode[] fields;
		readonly FixParseOptions? options;

		// The nodes of every scope still open, innermost last. A scope reads onto the top and
		// takes its own nodes off as one array, so a group read inside it has come and gone.
		// One per reader, and a reader per message: nothing is kept between messages.
		readonly List<FixNode> stack = new();

		// The membership map of every scope still open, innermost last, alongside the nodes. A
		// group entry ends when it meets a tag that an ENCLOSING scope claims; a tag nobody claims
		// stays where it was written, because ending an entry early loses the entries after it.
		readonly List<Dictionary<int, int>> open = new();

		public int Position { get; private set; }
		public FixParseError? Error { get; private set; }

		public Reader(string source, string type, FixNode[] fields, FixParseOptions? options)
		{
			this.source = source;
			this.type = type;
			this.fields = fields;
			this.options = options;
		}

		public void Fail(int tag, int position, string reason) => Error ??= new FixParseError(position, tag, type, reason);

		public FixNode[] Scope(SchemaRef[] schema, bool body = false)
		{
			return Scope(Members(schema), body, 0);
		}

		// `body` is true for the body and for every group entry inside it: what a scope does with
		// a field it does not list depends on whether the body is still to come.
		FixNode[] Scope(Dictionary<int, int> members, bool body, int delimiter)
		{
			var start = stack.Count;

			open.Add(members);

			while (Position < fields.Length && Error == null)
			{
				var field = fields[Position];

				if (delimiter != 0 && stack.Count > start && field.Tag == delimiter) break;

				if (!members.TryGetValue(field.Tag, out var group) && Ends(field.Tag, body, delimiter))
					break;

				Position++;
				if (group != 0) field = Group(field, group, body);
				stack.Add(field);
			}

			open.RemoveAt(open.Count - 1);

			var nodes = new FixNode[stack.Count - start];

			stack.CopyTo(start, nodes, 0, nodes.Length);
			stack.RemoveRange(start, nodes.Length);

			return nodes;
		}

		// Whether a field the open scope does not list ends it. Construction asks where a field
		// goes; whether it should be there is a finding (D53), so the body takes everything and
		// only a boundary that nothing else can draw is kept.
		bool Ends(int tag, bool body, int delimiter)
		{
			// The trailer ends whatever is open. Its fields belong to no other scope, and a group
			// running to the end of the message would otherwise swallow them.
			if (tag is 10 or 89 or 93)
				return true;

			// The header ends at its first non-member, and so does a group entry inside it: what
			// follows the header is the body, whose membership is not open yet and so can claim
			// nothing. Reading NoHops was what found this — without it a hop entry ran on and
			// swallowed the body.
			if (!body)
				return true;

			// The body runs from the header to the trailer and takes every field between, listed
			// or not: a message the reader will not build is a message the layer never sees.
			if (delimiter == 0)
				return false;

			// A group entry under the body ends only at a tag an enclosing scope claims; a tag
			// nobody claims stays where it was written, because ending an entry early would lose
			// the entries after it. The entry itself is the last map open.
			for (var i = 0; i < open.Count - 1; i++)
				if (open[i].ContainsKey(tag))
					return true;

			return false;
		}

		FixNode Group(FixNode counter, int id, bool body)
		{
			if (!counter.Field(source).TryGetInt64(out var count) || count < 0 || count > fields.Length - Position)
			{
				Fail(counter.Tag, counter.ValuePosition, "Invalid or impossible NumInGroup.");
				return counter;
			}
			var schema    = FixSchema.Group(id);
			var delimiter = First(schema);
			var members   = Members(schema);

			// The count came from the input, but it was held to the fields left above, so the
			// array is never larger than what remains to fill it.
			var scopes = new FixFieldSet[count];
			var filled = 0;

			while (filled < scopes.Length && Error == null)
			{
				if (Position == fields.Length || fields[Position].Tag != delimiter)
				{
					Fail(counter.Tag, counter.ValuePosition, "Group entry must begin with its schema delimiter; NumInGroup is not satisfied.");
					break;
				}

				scopes[filled++] = new FixFieldSet(source, Scope(members, body, delimiter));
			}

			if (filled < scopes.Length)
				Array.Resize(ref scopes, filled);

			return new FixNode(counter.Tag, counter.Position, counter.ValuePosition, counter.Length, Array.AsReadOnly(scopes), counter.TypedValue, id);
		}
	}
}
