using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DotGram.Finance.Fix;

/// <summary>
/// The length/data pairs a parse reads by, the standard's own and a consumer's own together.
/// </summary>
/// <remarks>
/// <para>
/// FIX carries binary values as a pair of fields: a length, then the tag whose value is that many
/// bytes and may hold the separator itself. Which tags are such a pair is the one thing a parse has
/// to know before it reads a value, and the standard settles it for sixteen pairs. A counterparty
/// may define more, in the bilateral range, and those are what this class is for.
/// </para>
/// <para>
/// The answer is worked out once, here, into a table the tag indexes — so the reader's question on
/// every field is an array read, and a consumer's dictionary is never consulted in the middle of a
/// parse. That is also why these options are immutable: to read by different pairs, build different
/// options rather than changing these.
/// </para>
/// </remarks>
public sealed class FixFieldOptions
{
	internal static readonly FixFieldOptions Default = new();

	// What the reader asks of a tag. The values are the grammar's own, so that the guard is a read
	// and not a translation: 1 begins a length/data pair, -1 is the data half standing where a
	// length should have been (which is no field at all), 0 is an ordinary value read to the
	// separator. A tag nobody has declared is ordinary, which is why zero means that.
	const sbyte Ordinary = 0, Length = 1, Data = -1;

	// Past this, a tag goes in the dictionary rather than the table. It covers every tag anyone
	// writes — the bilateral range ends at 39,999 — at 64 KB for the table, built once and shared
	// by every parse these options serve.
	const int Tabled = 65536;

	// The standard alone, which is what most callers want and none of them should pay for twice.
	// A slot filled the first time it is read rather than a field an initializer fills: `Default`
	// is itself a static of this class and would otherwise be built while this one was still null,
	// which is a bug that depends on the order two lines are written in.
	static sbyte[]? settled;

	static sbyte[] Standard =>
		settled ?? Interlocked.CompareExchange(ref settled, Settled(), null) ?? settled;

	readonly sbyte[]                 _kinds;
	readonly Dictionary<int, sbyte>? _far;
	readonly Dictionary<int, int>?   _pairs;

	/// <summary>Reads wire framing by the standard's sixteen length/data pairs alone.</summary>
	public FixFieldOptions() : this(null) { }

	/// <summary>Reads a lossless pipe rendering by the standard's pairs alone.</summary>
	/// <remarks>
	/// Framing is a value here rather than a second name for every method.
	/// Which separator an input uses is a property of the input, not of the caller's wish, and a
	/// property of the input belongs in the value that describes the input. A consumer with their
	/// own pairs AND log framing names the constructor; this is the common case, not the only one.
	/// </remarks>
	public static FixFieldOptions Log { get; } = new(null, null, FixFraming.Log);

	/// <summary>Reads by the standard's pairs and a consumer's own.</summary>
	/// <param name="lengthDataPairs">
	/// Length tag to data tag, copied. The standard's sixteen pairs always hold and are added to,
	/// never replaced, so neither tag of a supplied pair may be one the standard defines.
	/// </param>
	/// <exception cref="ArgumentException">
	/// A tag is not positive, a pair names one tag twice, a data tag is declared twice, a data tag
	/// is also a length tag, or either tag is one the standard already defines.
	/// </exception>
	/// <param name="customFields">
	/// Builds the fields of tags this package does not define; null builds what it builds today.
	/// </param>
	/// <param name="framing">
	/// Wire framing for SOH-separated input, log framing for a lossless pipe rendering.
	/// </param>
	public FixFieldOptions(
		IReadOnlyDictionary<int, int>? lengthDataPairs,
		FixCustomFields?               customFields = null,
		FixFraming                     framing      = FixFraming.Wire)
	{
		if (framing != FixFraming.Wire && framing != FixFraming.Log)
			throw new ArgumentOutOfRangeException(nameof(framing));

		CustomFields = customFields ?? FixSpareFields.Instance;
		Framing      = framing;

		if (lengthDataPairs is null || lengthDataPairs.Count == 0)
		{
			_kinds = Standard;

			return;
		}

		var pairs = new Dictionary<int, int>(lengthDataPairs.Count);
		var data  = new HashSet<int>();
		var top   = Standard.Length;

		foreach (var pair in lengthDataPairs)
		{
			if (pair.Key <= 0 || pair.Value <= 0 || pair.Key == pair.Value)
				throw new ArgumentException(
					"Pairs require positive, distinct length and data tags.", nameof(lengthDataPairs));

			// The standard's meaning for a tag stands. A pair that contradicts it could only be
			// ignored, and a caller who believes something that is not true is worse served by
			// silence than by this.
			if (FixSchema.Defines(pair.Key) || FixSchema.Defines(pair.Value))
				throw new ArgumentException(
					$"Tag {(FixSchema.Defines(pair.Key) ? pair.Key : pair.Value)} is one the standard defines; " +
					"the standard's pairs are added to, not replaced.", nameof(lengthDataPairs));

			if (!data.Add(pair.Value) || !pairs.TryAdd(pair.Key, pair.Value))
				throw new ArgumentException(
					"Pairs require unique length tags and unique data tags.", nameof(lengthDataPairs));

			top = Math.Max(top, Room(pair.Key));
			top = Math.Max(top, Room(pair.Value));
		}

		foreach (var tag in data)
			if (pairs.ContainsKey(tag))
				throw new ArgumentException(
					"A data tag cannot also be a length tag.", nameof(lengthDataPairs));

		var kinds = new sbyte[top];
		var far   = default(Dictionary<int, sbyte>);

		// The standard goes in first and nothing overwrites it: the checks above have already
		// refused everything that could try.
		Array.Copy(Standard, kinds, Standard.Length);

		foreach (var pair in pairs)
		{
			Declare(kinds, ref far, pair.Key,   Length);
			Declare(kinds, ref far, pair.Value, Data);
		}

		_kinds = kinds;
		_far   = far;
		_pairs = pairs;

		static int Room(int tag)
		{
			return tag < Tabled ? tag + 1 : 0;
		}

		static void Declare(sbyte[] kinds, ref Dictionary<int, sbyte>? far, int tag, sbyte kind)
		{
			if ((uint)tag < (uint)kinds.Length)
				kinds[tag] = kind;
			else
				(far ??= [])[tag] = kind;
		}
	}

	/// <summary>Builds the fields of tags this package does not define.</summary>
	/// <remarks>
	/// Never null: where a consumer supplied nothing this is the package's own, so the reader has
	/// one path and never asks whether anybody supplied anything.
	/// </remarks>
	public FixCustomFields CustomFields { get; }

	/// <summary>How the input separates one field from the next.</summary>
	public FixFraming Framing { get; }

	/// <summary>The same dictionary and the same custom fields, reading the other framing.</summary>
	/// <param name="framing">The framing the copy reads.</param>
	/// <remarks>
	/// Because framing is a property of the input and a dictionary is not: a process that reads a
	/// venue's live wire and its logs has one dictionary and two framings, and rebuilding the
	/// options from the pairs would mean keeping the pairs after they had been used. This is the
	/// only thing about a <see cref="FixFieldOptions"/> that is worth changing a copy for; the
	/// pairs and the custom fields are what the object IS.
	/// </remarks>
	public FixFieldOptions With(FixFraming framing)
	{
		return framing == Framing ? this : new FixFieldOptions(this, framing);
	}

	FixFieldOptions(FixFieldOptions other, FixFraming framing)
	{
		if (framing != FixFraming.Wire && framing != FixFraming.Log)
			throw new ArgumentOutOfRangeException(nameof(framing));

		_kinds       = other._kinds;
		_far         = other._far;
		_pairs       = other._pairs;
		CustomFields = other.CustomFields;
		Framing      = framing;
	}

	/// <summary>What the reader does with a tag: 1 a length/data pair, -1 no field, 0 an ordinary value.</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int Kind(int tag)
	{
		var kinds = _kinds;

		if ((uint)tag < (uint)kinds.Length)
			return kinds[tag];

		return _far is not null && _far.TryGetValue(tag, out var kind) ? kind : Ordinary;
	}

	/// <summary>The data tag a length tag is paired with, or zero where it is not a length tag.</summary>
	/// <remarks>
	/// Asked once a field is already known to begin a pair, which is sixteen tags plus a consumer's
	/// and not every field — so this may be a lookup where <see cref="Kind"/> may not.
	/// </remarks>
	internal int DataTag(int lengthTag)
	{
		var standard = FixSchema.DataTag(lengthTag);

		if (standard != 0)
			return standard;

		return _pairs is not null && _pairs.TryGetValue(lengthTag, out var data) ? data : 0;
	}

	/// <summary>Whether a tag carries binary data — the standard's, or one this consumer declared.</summary>
	internal bool IsData(int tag)
	{
		return Kind(tag) == Data;
	}

	static sbyte[] Settled()
	{
		var kinds = new sbyte[FixSchema.TagLimit];

		for (var tag = 1; tag < kinds.Length; tag++)
			kinds[tag] = FixSchema.DataTag(tag) != 0 ? Length : FixSchema.IsData(tag) ? Data : Ordinary;

		return kinds;
	}
}
