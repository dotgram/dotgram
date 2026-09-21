using System.Collections.Generic;

namespace DotGram.Finance.Fix;

/// <summary>
/// What composed validation calls: the parts of the walk that do not depend on the schema.
/// </summary>
/// <remarks>
/// The split is the whole idea of composing rules. What a schema says — which tags a scope holds,
/// which are required, what type a value has, what an entry's order is — becomes code, because
/// that is what a table lookup was standing in for. What the schema does not say — that a tag
/// twice in a scope is a duplicate, that a data field follows its length, that a count matches
/// the entries — is the same work for every message, and writing it out ninety-three times would
/// buy nothing and give ninety-three things to disagree with each other.
///
/// <para>
/// Every finding here is worded exactly as <see cref="FixRules"/> words it. Two roads over one
/// dictionary answering in different words is a difference a reader would see and could not
/// explain, so the wording is part of what has to agree.
/// </para>
/// </remarks>
static class FixRuleSupport
{
	/// <summary>A finding about a field that is present, which therefore has a position.</summary>
	public static FixFinding Wrong(FixRule rule, FixScope where, FixFieldView field, int groupTag, int entry, string reason)
	{
		return new(rule, where, field.Tag, groupTag, entry, field.ValuePosition, reason);
	}

	/// <summary>A finding about something absent, which has a tag but nowhere to point.</summary>
	public static FixFinding Missing(FixRule rule, FixScope where, int tag, int groupTag, int entry, string reason)
	{
		return new(rule, where, tag, groupTag, entry, 0, reason);
	}

	/// <summary>
	/// A finding about a component, which has no tag of its own to name.
	/// </summary>
	/// <remarks>
	/// Its own method rather than a null handed to <see cref="Missing"/>: the composed text would
	/// have to spell the null, and a reader of that line would be asked to know that a component
	/// is the one thing here without a tag.
	/// </remarks>
	public static FixFinding Absent(FixScope where, int groupTag, int entry, string reason)
	{
		return new(FixRule.RequiredComponentMissing, where, null, groupTag, entry, 0, reason);
	}

	/// <summary>Whether this scope has already held the tag.</summary>
	public static bool Seen(ulong[] seen, int tag)
	{
		return tag is > 0 and < 957 && (seen[tag >> 6] & 1UL << (tag & 63)) != 0;
	}

	/// <summary>Records the tag in the scope's mask; a tag outside the mask's range is ignored.</summary>
	public static void Mark(ulong[] seen, int tag)
	{
		if (tag is > 0 and < 957)
			seen[tag >> 6] |= 1UL << (tag & 63);
	}

	/// <summary>Whether the scope holds the tag: from the mask, or by looking where it cannot.</summary>
	public static bool Has(FixFieldSet scope, ulong[] seen, int tag)
	{
		return FixRules.Has(scope, seen, tag);
	}

	/// <summary>Whether the scope holds any of these tags, which is what a component's presence is.</summary>
	public static bool Any(FixFieldSet scope, ulong[] seen, int[] tags)
	{
		foreach (var tag in tags)
			if (FixRules.Has(scope, seen, tag))
				return true;

		return false;
	}

	/// <summary>
	/// A tag the schema does not place here — and the two sentences are told apart by asking the
	/// tables, because whether a tag exists at all is the one thing the composed code cannot know
	/// from the scope it was written for.
	/// </summary>
	public static void NotInScope(FixTables tables, List<FixFinding> found, FixScope where, FixFieldView field, int groupTag, int entry)
	{
		found.Add(Wrong(FixRule.FieldNotInScope, where, field, groupTag, entry,
			tables.Defines(field.Tag)
				? "The schema defines this tag, but not in this scope."
				: "The schema defines no such tag."));
	}

	/// <summary>
	/// The field with that tag, which the composed code asks for only where it has just tested
	/// that the scope holds it.
	/// </summary>
	/// <remarks>
	/// A method rather than <c>scope.GetField(tag).Value</c> in the text: the generated code is
	/// read by somebody debugging their dictionary, and a null-valued access there would be one
	/// more thing in it to explain.
	/// </remarks>
	public static FixFieldView Field(FixFieldSet scope, int tag)
	{
		return scope.GetField(tag)!.Value;
	}

	/// <summary>A counter against the entries that followed it.</summary>
	public static void Count(
		List<FixFinding>           found,
		FixScope                   where,
		FixFieldView               counter,
		IReadOnlyList<FixFieldSet> entries,
		int                        groupTag,
		int                        entry,
		bool                       required)
	{
		if (!counter.TryGetInt64(out var count) || count != entries.Count || count < 0 || required && count == 0)
			found.Add(Wrong(FixRule.GroupCountMismatch, where, counter, groupTag, entry,
				$"The count says {counter} and {entries.Count} entries follow."));
	}

	/// <summary>
	/// The length/data pairs of one scope: a data field must follow the length that measures it,
	/// and a length field must precede the data it measures.
	/// </summary>
	/// <remarks>
	/// One call a scope rather than a test a field: which tags are a pair is the standard's, not
	/// the message's, and the scan needs the field before and the field after, which a switch over
	/// one tag does not have.
	/// </remarks>
	public static void Pairs(
		FixTables        tables,
		List<FixFinding> found,
		FixFieldSet      scope,
		FixScope         where,
		int              groupTag,
		int              entry)
	{
		for (var i = 0; i < scope.Nodes.Length; i++)
		{
			var node      = scope.Nodes[i];
			var lengthTag = tables.LengthTag(node.Tag);

			if (lengthTag != 0 &&
				(i == 0 ||
				 scope.Nodes[i - 1].Tag != lengthTag ||
				 !scope.Nodes[i - 1].Field(scope.Source).TryGetInt64(out var measured) ||
				 measured != node.Length))
				found.Add(Wrong(FixRule.DataFieldNotAfterLength, where, node.Field(scope.Source), groupTag, entry,
					$"A data field must immediately follow tag {lengthTag}, which measures it."));

			var dataTag = tables.DataTag(node.Tag);

			if (dataTag != 0 && (i + 1 == scope.Nodes.Length || scope.Nodes[i + 1].Tag != dataTag))
				found.Add(Wrong(FixRule.LengthFieldNotBeforeData, where, node.Field(scope.Source), groupTag, entry,
					$"A length field must immediately precede tag {dataTag}, which it measures."));
		}
	}

	/// <summary>
	/// The two findings that are about the message rather than about any one of its scopes.
	/// </summary>
	public static void Message(FixTables tables, FixMessage message, List<FixFinding> found)
	{
		if (!tables.Describes(message.MessageType))
			found.Add(new FixFinding(
				FixRule.UnknownMessageType, FixScope.Header, 35, 0, -1, 0,
				$"The schema defines no message of type '{message.MessageType}'."));

		if (message.Header.GetField(347) == null && (Encoded(tables, message.Header) || Encoded(tables, message)))
			found.Add(new FixFinding(
				FixRule.MessageEncodingMissing, FixScope.Header, 347, 0, -1, 0,
				"MessageEncoding is required where an Encoded field is present."));
	}

	static bool Encoded(FixTables tables, FixFieldSet scope)
	{
		foreach (var node in scope.Nodes)
		{
			if (tables.RequiresEncoding(node.Tag))
				return true;

			if (node.Entries != null)
				foreach (var entry in node.Entries)
					if (Encoded(tables, entry))
						return true;
		}

		return false;
	}
}
