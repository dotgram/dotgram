using System.Collections.Generic;

namespace DotGram.Finance.Fix44;

/// <summary>What every check says a finding with.</summary>
partial class FixValidators
{
	/// <summary>A field the schema requires of this type, and the message does not have.</summary>
	internal static void Missing(FixMessage message, FixTag tag)
	{
		message.AddFinding(new FixFinding(FixRule.RequiredFieldMissing, tag, 0, null, -1));
	}

	/// <summary>A field the schema requires of an entry, and the entry does not have: said at the field the entry opened with.</summary>
	internal static void Missing(FixMessage message, FixTag tag, int position, int index)
	{
		message.AddFinding(new FixFinding(FixRule.RequiredFieldMissing, tag, position, null, index));
	}

	/// <summary>A block the schema requires of this type, and the carrier has no field of.</summary>
	/// <remarks>Named by the first tag it would have held, since a block has no tag of its own.</remarks>
	internal static void Absent(FixMessage message, FixTag tag)
	{
		message.AddFinding(new FixFinding(FixRule.RequiredComponentMissing, tag, 0, null, -1));
	}

	/// <summary>A value that does not fit its field: not its type, or not one the specification lists.</summary>
	internal static void Invalid(FixMessage message, FixField field)
	{
		message.AddFinding(new FixFinding(FixRule.InvalidValue, field.Tag, field.Position, field, -1));
	}

	/// <summary>A counter and the entries that follow it, which have to be the same number.</summary>
	/// <remarks>
	/// A counter absent while entries are present is a finding of the reading, made where the first
	/// entry was built, so there is nothing left to say about it here.
	/// </remarks>
	internal static void Counted<T>(FixMessage message, FixField.Typed<long>? counter, List<T>? entries)
	{
		if (counter is null)
			return;

		if (!counter.IsValid || counter.Value != (entries?.Count ?? 0))
			message.AddFinding(new FixFinding(FixRule.GroupCountMismatch, counter.Tag, counter.Position, counter, -1));
	}
}
