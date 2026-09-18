using System;
using System.Runtime.CompilerServices;

namespace DotGram.Finance.Fix;

/// <summary>Interprets a flat sequence of fields using the FIX message schema.</summary>
static class FixSemantics
{
	public static bool TryBuild(string source, string type, FixNode[] fields, FixParseMode mode, FixParseOptions? options, out FixMessage? message, out FixParseError? error)
	{
		message = null;

		var reader = new Reader(source, type, fields, mode, options);
		var header = reader.Scope(FixSchema.Component(1024));
		var schema = FixSchema.Message(type);

		if (schema.Length == 0 && mode == FixParseMode.Strict)
			reader.Fail(35, 0, "Unknown FIX 4.4 message type.");

		var body    = reader.Scope(schema, body: true, custom: schema.Length == 0);
		var trailer = reader.Scope(FixSchema.Component(1025));

		if (reader.Position != fields.Length && reader.Error == null)
		{
			var field = fields[reader.Position];
			reader.Fail(field.Tag, field.Position, "Field is not permitted in this message scope.");
		}

		error = reader.Error;

		if (error != null)
			return false;

		var result = FixMessageFactory.Message(type, source, header, body, trailer);

		if (!FixValidation.Validate(result, mode, options, out error))
			return false;

		message = result;

		return true;
	}

	static readonly ConditionalWeakTable<SchemaRef[], Dictionary<int, int>> scopes = new();

	static Dictionary<int, int> Members(SchemaRef[] schema)
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
		readonly FixParseMode mode;
		readonly FixParseOptions? options;

		// The nodes of every scope still open, innermost last. A scope reads onto the top and
		// takes its own nodes off as one array, so a group read inside it has come and gone.
		// One per reader, and a reader per message: nothing is kept between messages.
		readonly List<FixNode> stack = new();

		public int Position { get; private set; }
		public FixParseError? Error { get; private set; }

		public Reader(string source, string type, FixNode[] fields, FixParseMode mode, FixParseOptions? options)
		{
			this.source = source;
			this.type = type;
			this.fields = fields;
			this.mode = mode;
			this.options = options;
		}

		public void Fail(int tag, int position, string reason) => Error ??= new FixParseError(position, tag, type, reason);

		public FixNode[] Scope(SchemaRef[] schema, bool body = false, bool custom = false)
		{
			return Scope(Members(schema), body, custom, 0);
		}

		FixNode[] Scope(Dictionary<int, int> members, bool body, bool custom, int delimiter)
		{
			var start = stack.Count;
			while (Position < fields.Length && Error == null)
			{
				var field = fields[Position];
				if (delimiter != 0 && stack.Count > start && field.Tag == delimiter) break;
				if (!members.TryGetValue(field.Tag, out var group))
				{
					// Header extensions start the body; unknown group fields remain in
					// the current entry until a known delimiter or enclosing field.
					var extension = FixSchema.Type(field.Tag) == null &&
						mode == FixParseMode.Lenient;
					if (!(body || delimiter != 0) || field.Tag is 10 or 89 or 93 || !(extension || custom)) break;
				}
				Position++;
				if (group != 0) field = Group(field, group);
				stack.Add(field);
			}

			var nodes = new FixNode[stack.Count - start];

			stack.CopyTo(start, nodes, 0, nodes.Length);
			stack.RemoveRange(start, nodes.Length);

			return nodes;
		}

		FixNode Group(FixNode counter, int id)
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

				scopes[filled++] = new FixFieldSet(source, Scope(members, false, false, delimiter));
			}

			if (filled < scopes.Length)
				Array.Resize(ref scopes, filled);

			return new FixNode(counter.Tag, counter.Position, counter.ValuePosition, counter.Length, Array.AsReadOnly(scopes), counter.TypedValue, id);
		}
	}
}
