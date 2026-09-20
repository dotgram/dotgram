using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix;

/// <summary>
/// Checks a message that has already been built, and answers with everything wrong with it.
/// </summary>
/// <remarks>
/// <para>
/// A message is asked whether it is right — <see cref="FixMessage.Validate(FixValidator)"/> — and
/// this is what it is asked against. Validation is a layer over a built message, not a mode of
/// building one (D53). What a parse
/// does is read the wire; what this does is hold the result to a schema. Keeping them apart is
/// what lets a counterparty's dictionary be data: reading is fixed at build time and checking is
/// not, so a schema richer than FIX 4.4's can arrive at run time without touching the parser.
/// </para>
/// <para>
/// An instance is immutable and may be used from several threads. Hold one per process, not one
/// per message: a dictionary is read once and asked many times, which is the whole reason this is
/// an object rather than a method on the message.
/// </para>
/// <para>
/// It answers with every finding. A reader chasing a disagreement with a counterparty is ill
/// served by a check that stops at the first: they would run it again for each of the rest.
/// </para>
/// </remarks>
public sealed class FixValidator
{
	static readonly FixFinding[] Nothing = [];

	FixValidator() { }

	/// <summary>Checks against the schema this package compiles in, which is FIX 4.4's.</summary>
	/// <remarks>
	/// This is what <c>FixParseMode.Strict</c> checked while validation lived inside building, in
	/// its new home and unchanged. A consumer moving across writes <c>message.Validate()</c> where
	/// they wrote <c>FixParseMode.Strict</c>.
	/// </remarks>
	public static FixValidator Standard { get; } = new();

	// A message is asked whether it is valid, so the verb is FixMessage.Validate and this is what
	// stands behind it (D69). One public way in, and it reads the way a consumer says the thing.
	internal FixFinding[] Check(FixMessage message)
	{
		List<FixFinding>? found = null;

		var schema = FixSchema.Message(message.MessageType);

		if (schema.Length == 0)
			Add(ref found, new FixFinding(
				FixRule.UnknownMessageType, FixScope.Header, 35, 0, -1, 0,
				$"The schema defines no message of type '{message.MessageType}'."));

		if (message.Header.GetField(347) == null && (Encoded(message.Header) || Encoded(message)))
			Add(ref found, new FixFinding(
				FixRule.MessageEncodingMissing, FixScope.Header, 347, 0, -1, 0,
				"MessageEncoding is required where an Encoded field is present."));

		Scope(ref found, message.Header,  FixSchema.Component(1024), FixScope.Header,  0, -1, ordered: false);
		Scope(ref found, message,         schema,                    FixScope.Body,    0, -1, ordered: false);
		Scope(ref found, message.Trailer, FixSchema.Component(1025), FixScope.Trailer, 0, -1, ordered: false);

		return found is null ? Nothing : found.ToArray();
	}

	static void Add(ref List<FixFinding>? found, FixFinding finding) => (found ??= []).Add(finding);

	static bool Encoded(FixFieldSet scope)
	{
		foreach (var node in scope.Nodes)
		{
			if (FixSchema.RequiresEncoding(node.Tag))
				return true;

			if (node.Entries != null)
				foreach (var entry in node.Entries)
					if (Encoded(entry))
						return true;
		}

		return false;
	}

	static void Scope(
		ref List<FixFinding>? found,
		FixFieldSet           scope,
		SchemaRef[]           schema,
		FixScope              where,
		int                   groupTag,
		int                   entryIndex,
		bool                  ordered)
	{
		Span<ulong> seen = stackalloc ulong[15];

		var           previousRank = -1;
		HashSet<int>? extended     = null;

		// Where a message names a type the schema does not know there is nothing to be out of
		// place against, and saying so of every field would bury the one finding that matters.
		var members = schema.Length == 0 ? null : FixSemantics.Members(schema);

		seen.Clear();

		for (var i = 0; i < scope.Nodes.Length; i++)
		{
			var node  = scope.Nodes[i];
			var field = node.Field(scope.Source);

			if (ordered)
			{
				var ordinal = 0;
				var rank    = Rank(schema, node.Tag, ref ordinal);

				if (rank >= 0 && rank < previousRank)
					Add(ref found, Wrong(FixRule.FieldOutOfOrder, where, field, groupTag, entryIndex,
						"The fields of a group entry are not in the order the schema gives them."));
				else if (rank >= 0)
					previousRank = rank;
			}

			if (node.Tag is < 957 and > 0)
			{
				var bit = 1UL << (node.Tag & 63);

				if ((seen[node.Tag >> 6] & bit) != 0)
					Add(ref found, Wrong(FixRule.DuplicateField, where, field, groupTag, entryIndex,
						"The tag appears more than once in this scope."));

				seen[node.Tag >> 6] |= bit;
			}
			else if (!(extended ??= []).Add(node.Tag))
			{
				Add(ref found, Wrong(FixRule.DuplicateField, where, field, groupTag, entryIndex,
					"The tag appears more than once in this scope."));
			}

			var lengthTag = FixSchema.LengthTag(node.Tag);

			if (lengthTag != 0 &&
				(i == 0 ||
				 scope.Nodes[i - 1].Tag != lengthTag ||
				 !scope.Nodes[i - 1].Field(scope.Source).TryGetInt64(out var measured) ||
				 measured != node.Length))
				Add(ref found, Wrong(FixRule.DataFieldNotAfterLength, where, field, groupTag, entryIndex,
					$"A data field must immediately follow tag {lengthTag}, which measures it."));

			var dataTag = FixSchema.DataTag(node.Tag);

			if (dataTag != 0 && (i + 1 == scope.Nodes.Length || scope.Nodes[i + 1].Tag != dataTag))
				Add(ref found, Wrong(FixRule.LengthFieldNotBeforeData, where, field, groupTag, entryIndex,
					$"A length field must immediately precede tag {dataTag}, which it measures."));

			// Construction asks where a field goes and puts it in the scope it was written in;
			// whether it belongs there is this question, and it is the one the strict mode used to
			// answer by refusing to build the message at all.
			if (members != null && !members.ContainsKey(node.Tag))
				Add(ref found, Wrong(FixRule.FieldNotInScope, where, field, groupTag, entryIndex,
					FixSchema.Type(node.Tag) == null
						? "The schema defines no such tag."
						: "The schema defines this tag, but not in this scope."));

			if (!FixPrimitives.Valid(field, FixSchema.Type(node.Tag), FixSchema.Codes(node.Tag)))
				Add(ref found, Wrong(FixRule.InvalidValue, where, field, groupTag, entryIndex,
					"The value does not fit the field's type, or is not one of its code set."));
		}

		References(ref found, scope, seen, schema, where, groupTag, entryIndex);
	}

	static void References(
		ref List<FixFinding>? found,
		FixFieldSet           scope,
		ReadOnlySpan<ulong>   seen,
		SchemaRef[]           schema,
		FixScope              where,
		int                   groupTag,
		int                   entryIndex)
	{
		foreach (var reference in schema)
		{
			if (reference.Kind == 1)
			{
				// The header and the trailer are components of every message and are walked as
				// scopes of their own, so a body's references to them are not followed here.
				if (reference.Id is 1024 or 1025)
					continue;

				var child   = FixSchema.Component(reference.Id);
				var present = Present(scope, seen, child);

				if (reference.Required && !present)
					Add(ref found, new FixFinding(
						FixRule.RequiredComponentMissing, where, null, groupTag, entryIndex, 0,
						$"A component the schema requires has none of its fields; its first is tag {First(child)}."));

				if (present)
					References(ref found, scope, seen, child, where, groupTag, entryIndex);

				continue;
			}

			var tag = reference.Kind == 2 ? FixSchema.Counter(reference.Id) : reference.Id;

			if (!Has(scope, seen, tag))
			{
				if (reference.Required)
					Add(ref found, new FixFinding(
						FixRule.RequiredFieldMissing, where, tag, groupTag, entryIndex, 0,
						"The schema requires this tag in this scope."));

				continue;
			}

			if (reference.Kind != 2)
				continue;

			var counter = scope.GetField(tag)!.Value;
			var entries = scope.GetGroup(tag);

			if (!counter.TryGetInt64(out var count) || count != entries.Count || count < 0 ||
				reference.Required && count == 0)
				Add(ref found, Wrong(FixRule.GroupCountMismatch, where, counter, groupTag, entryIndex,
					$"The count says {counter} and {entries.Count} entries follow."));

			for (var entry = 0; entry < entries.Count; entry++)
				Scope(ref found, entries[entry], FixSchema.Group(reference.Id), where, tag, entry, ordered: true);
		}
	}

	// A component is an id in our schema and a consumer cannot resolve one to a name, so a finding
	// about a missing component names the first tag it would have held. That is the thing they can
	// look up, and usually the thing they forgot.
	static int First(SchemaRef[] schema)
	{
		foreach (var reference in schema)
		{
			var tag = reference.Kind == 1
				? First(FixSchema.Component(reference.Id))
				: reference.Kind == 2 ? FixSchema.Counter(reference.Id) : reference.Id;

			if (tag != 0)
				return tag;
		}

		return 0;
	}

	// A scope's standard tags are in the mask its fields marked; any other tag is looked for
	// among the fields themselves.
	internal static bool Has(FixFieldSet scope, ReadOnlySpan<ulong> seen, int tag) =>
		tag is > 0 and < 957 ? (seen[tag >> 6] & 1UL << (tag & 63)) != 0 : scope.GetField(tag) != null;

	static bool Present(FixFieldSet scope, ReadOnlySpan<ulong> seen, SchemaRef[] schema)
	{
		foreach (var reference in schema)
			if (reference.Kind == 1
				? Present(scope, seen, FixSchema.Component(reference.Id))
				: Has(scope, seen, reference.Kind == 2 ? FixSchema.Counter(reference.Id) : reference.Id))
				return true;

		return false;
	}

	static int Rank(SchemaRef[] schema, int tag, ref int ordinal)
	{
		foreach (var reference in schema)
		{
			if (reference.Kind == 1)
			{
				var rank = Rank(FixSchema.Component(reference.Id), tag, ref ordinal);

				if (rank >= 0)
					return rank;
			}
			else
			{
				if ((reference.Kind == 2 ? FixSchema.Counter(reference.Id) : reference.Id) == tag)
					return ordinal;

				ordinal++;
			}
		}

		return -1;
	}

	static FixFinding Wrong(FixRule rule, FixScope where, FixFieldView field, int groupTag, int entryIndex, string reason) =>
		new(rule, where, field.Tag, groupTag, entryIndex, field.ValuePosition, reason);
}
