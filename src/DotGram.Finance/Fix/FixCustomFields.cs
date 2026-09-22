using System;

namespace DotGram.Finance.Fix;

/// <summary>
/// Builds the fields of tags this package knows nothing about.
/// </summary>
/// <remarks>
/// <para>
/// The package holds a case for each of the 912 tags FIX 4.4 defines. A tag outside that set —
/// a counterparty's, a venue's, anything in the bilateral range — has no case, and what is built
/// for it is a <see cref="FixField.Custom"/> carrying the octets. A consumer who knows what those
/// tags mean supplies one of these instead and builds their own fields, typed as they like.
/// </para>
/// <para>
/// Three methods because a field is built three ways: from the characters of a text field, from
/// its bytes, and from the payload of a length/data pair. They are abstract rather than virtual so
/// that a reading answered in one form and not another is a compiler error here and not a
/// difference between two parses of the same message in production.
/// </para>
/// <para>
/// A value arrives as a span and may not be kept: whatever is built has to take a copy of what it
/// needs before returning. The type says so, and the parse reuses the memory underneath.
/// </para>
/// <para>
/// <strong>This builds field objects; it does not make a tag known to the message layer.</strong>
/// The schema is FIX 4.4's, so whoever built the field, a tag outside it has no property of a
/// message to sit in: <see cref="FixMessage.Fields"/> carries it and nothing else places it.
/// Reading a counterparty's dictionary is a separate question.
/// </para>
/// </remarks>
public abstract class FixCustomFields
{
	/// <summary>Builds the field of a tag whose value was read as characters.</summary>
	/// <param name="tag">The numeric FIX tag, which is outside the set this package defines.</param>
	/// <param name="value">The value as it stood on the wire, which may not be kept.</param>
	public abstract FixField Text(int tag, ReadOnlySpan<char> value);

	/// <summary>Builds the field of a tag whose value was read as bytes.</summary>
	/// <param name="tag">The numeric FIX tag, which is outside the set this package defines.</param>
	/// <param name="value">The value as it stood on the wire, which may not be kept.</param>
	public abstract FixField Text(int tag, ReadOnlySpan<byte> value);

	/// <summary>Builds the field of a length/data pair whose data tag this package does not define.</summary>
	/// <param name="tag">The data tag of the pair.</param>
	/// <param name="value">The payload, already the length the pair declared.</param>
	/// <remarks>
	/// Unlike the two text forms this one is handed memory rather than a span, because the payload
	/// of a pair is already bytes and already its own. It is not called where the octets could not
	/// be read at all — a character above 255 in a log rendering, say — since there would be
	/// nothing to build from; the package's own case reports that.
	/// </remarks>
	public abstract FixField Binary(int tag, ReadOnlyMemory<byte> value);

	/// <summary>What this package builds for such a tag: the octets, under their own tag.</summary>
	/// <param name="tag">The numeric FIX tag.</param>
	/// <param name="value">The value as it stood on the wire.</param>
	/// <remarks>
	/// For a tag a consumer's own switch does not recognise either. Falling through is then one
	/// line and reads as falling through.
	/// </remarks>
	protected static FixField Spare(int tag, ReadOnlySpan<char> value)
	{
		return new FixField.Custom(tag, FixConvert.Data(value));
	}

	/// <inheritdoc cref="Spare(int, ReadOnlySpan{char})"/>
	protected static FixField Spare(int tag, ReadOnlySpan<byte> value)
	{
		return new FixField.Custom(tag, FixConvert.Data(value));
	}

	/// <inheritdoc cref="Spare(int, ReadOnlySpan{char})"/>
	protected static FixField Spare(int tag, ReadOnlyMemory<byte> value)
	{
		return new FixField.Custom(tag, (true, value));
	}
}

/// <summary>
/// What stands where a consumer supplied nothing, so that the reader has one path and not two.
/// </summary>
/// <remarks>
/// The point of the seam is that a known tag pays nothing for it. That holds because the reader
/// never asks whether a consumer supplied anything — this instance is always there, and the 912
/// arms that answer a known tag never reach it.
/// </remarks>
sealed class FixSpareFields : FixCustomFields
{
	internal static readonly FixSpareFields Instance = new();

	FixSpareFields() { }

	public override FixField Text(int tag, ReadOnlySpan<char> value)
	{
		return Spare(tag, value);
	}

	public override FixField Text(int tag, ReadOnlySpan<byte> value)
	{
		return Spare(tag, value);
	}

	public override FixField Binary(int tag, ReadOnlyMemory<byte> value)
	{
		return Spare(tag, value);
	}
}
