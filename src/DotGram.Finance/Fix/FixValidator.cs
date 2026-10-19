using System;

namespace DotGram.Finance.Fix;

/// <summary>
/// The checks of a version of FIX, held as one object of slots, and what every check says a finding
/// with. A version derives its own, whose slots are its message types, blocks and fields.
/// </summary>
abstract partial class FixValidator
{
	/// <summary>A field the schema requires of this type, and the message does not have.</summary>
	internal static void Missing(FixCheck check, int tag)
	{
		check.Add(FixRule.RequiredFieldMissing, tag, -1, null);
	}

	/// <summary>A field the schema requires of an entry, and the entry does not have: said at the field the entry opened with.</summary>
	internal static void Missing(FixCheck check, int tag, int position)
	{
		check.Add(FixRule.RequiredFieldMissing, tag, position, null);
	}

	/// <summary>A block the schema requires of this type, and the carrier has no field of.</summary>
	/// <remarks>Named by the first tag it would have held, since a block has no tag of its own.</remarks>
	internal static void Absent(FixCheck check, int tag)
	{
		check.Add(FixRule.RequiredComponentMissing, tag, -1, null);
	}

	/// <summary>A block the schema requires of an entry, and the entry has no field of: said at the field the entry opened with.</summary>
	internal static void Absent(FixCheck check, int tag, int position)
	{
		check.Add(FixRule.RequiredComponentMissing, tag, position, null);
	}

	/// <summary>A value that does not fit its field: not its type, or not one the specification lists.</summary>
	internal static void Invalid(FixCheck check, FixField field)
	{
		check.Add(FixRule.InvalidValue, field.Tag, field.Position, field);
	}

	/// <summary>Nothing: what a loaded check of a message, block or entry the file gives no members is written with.</summary>
	internal static void Nothing()
	{
	}

	/// <summary>A counter and the entries that follow it, which have to be the same number.</summary>
	/// <remarks>
	/// A counter absent while entries are present is a finding of the reading, made where the first
	/// entry was built, so there is nothing left to say about it here.
	/// </remarks>
	internal static void Counted<T>(FixCheck check, FixField.Typed<long>? counter, List<T>? entries)
	{
		if (counter is null)
			return;

		if (!counter.IsValid || counter.Value != (entries?.Count ?? 0))
			check.Add(FixRule.GroupCountMismatch, counter.Tag, counter.Position, counter);
	}
}
