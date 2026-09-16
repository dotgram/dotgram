using System;
using System.Collections.Generic;
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
		var body = reader.Scope(schema, body: true, custom: schema.Length == 0);
		var trailer = reader.Scope(FixSchema.Component(1025));
		if (reader.Position != fields.Length && reader.Error == null)
		{
			var field = fields[reader.Position];
			reader.Fail(field.Tag, field.Position, "Field is not permitted in this message scope.");
		}
		error = reader.Error;
		if (error != null) return false;
		var result = FixFactories.Message(type, source, header, body, trailer);
		if (!FixValidation.Validate(result, mode, options, out error)) return false;
		message = result;
		return true;
	}

	static readonly ConditionalWeakTable<SchemaRef[], Dictionary<int, int>> scopes = new();
	static Dictionary<int, int> Members(SchemaRef[] schema) => scopes.GetValue(schema, CreateMembers);
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

		public FixNode[] Scope(SchemaRef[] schema, bool body = false, bool custom = false, int delimiter = 0)
		{
			var members = Members(schema);
			var nodes = new List<FixNode>();
			while (Position < fields.Length && Error == null)
			{
				var field = fields[Position];
				if (delimiter != 0 && nodes.Count > 0 && field.Tag == delimiter) break;
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
				nodes.Add(field);
			}
			return nodes.ToArray();
		}

		FixNode Group(FixNode counter, int id)
		{
			if (!counter.Field(source).TryGetInt64(out var count) || count < 0 || count > fields.Length - Position)
			{
				Fail(counter.Tag, counter.ValuePosition, "Invalid or impossible NumInGroup.");
				return counter;
			}
			var schema = FixSchema.Group(id);
			var delimiter = First(schema);
			var entries = new List<FixNode[]>();
			for (long n = 0; n < count && Error == null; n++)
			{
				if (Position == fields.Length || fields[Position].Tag != delimiter)
				{
					Fail(counter.Tag, counter.ValuePosition, "Group entry must begin with its schema delimiter; NumInGroup is not satisfied.");
					break;
				}
				entries.Add(Scope(schema, delimiter: delimiter));
			}
			return new FixNode(counter.Tag, counter.Position, counter.ValuePosition, counter.Length, FixFactories.Group(id, source, entries), counter.TypedValue);
		}
	}
}
