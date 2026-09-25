using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix;

/// <summary>
/// What a version of FIX tells a context about its tags: the type of the value of every tag it
/// defines, and its length/data pairs. One a version, built once and shared by every context of it.
/// </summary>
sealed class FixVersion
{
	/// <param name="pairs">The version's length/data pairs, length tag to data tag.</param>
	/// <param name="types">The type of the value of a tag the version defines; <see cref="FixValueType.None"/> for one it does not.</param>
	/// <param name="last">The highest tag the version defines, the length of the table the reader indexes.</param>
	public FixVersion(Dictionary<FixTag, FixTag> pairs, Func<FixTag, FixValueType> types, FixTag last)
	{
		Pairs = pairs;
		Type  = types;
		Codes = FixContext.Codes.Of(pairs, types, last);
	}

	/// <summary>The version's length/data pairs, length tag to data tag.</summary>
	public Dictionary<FixTag, FixTag> Pairs { get; }

	/// <summary>The type of the value of a tag the version defines; <see cref="FixValueType.None"/> for one it does not.</summary>
	public Func<FixTag, FixValueType> Type { get; }

	/// <summary>The table a context of this version starts from.</summary>
	public FixContext.Codes Codes { get; }
}
