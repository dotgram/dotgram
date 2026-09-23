using System;

namespace DotGram.Finance.Fix44;

/// <summary>A field of a tag FIX 4.4 does not define, built by a consumer who knows what it means.</summary>
/// <remarks>
/// <para>
/// The context's <see cref="FixContext.FixFieldFactory"/> is asked, for a tag the package has no
/// class for, which of these to build: <c>tag =&gt; tag == 25005 ? new Status() : null</c>. The
/// field it answers is then handed the value as the wire had it, through <see cref="Read(ReadOnlySpan{char})"/>,
/// and what that returns is the field's <see cref="FixField.IsValid"/>. Where the factory answers
/// null, or there is none, the field is a <see cref="FixField.Invalid"/> of that tag.
/// </para>
/// <para>
/// A value arrives as a span and may not be kept: whatever the field holds has to be a copy of what
/// it needs. The parse reuses the memory underneath.
/// </para>
/// </remarks>
public abstract class FixCustomField : FixField
{
	/// <summary>A field of this tag, before it has been handed its value.</summary>
	/// <param name="tag">The tag the field is read under.</param>
	protected FixCustomField(int tag) : base(tag, true)
	{
	}

	/// <summary>Takes the value, as characters; answers whether it fits the field.</summary>
	/// <param name="value">The value as it stood on the wire, which may not be kept.</param>
	protected internal abstract bool Read(ReadOnlySpan<char> value);

	/// <summary>Takes the value, as bytes; answers whether it fits the field.</summary>
	/// <param name="value">The value as it stood on the wire, which may not be kept.</param>
	/// <remarks>Each byte is the character of the same code, which is what FIX text is, and the characters go to <see cref="Read(ReadOnlySpan{char})"/>.</remarks>
	protected internal virtual bool Read(ReadOnlySpan<byte> value)
	{
		Span<char> chars = value.Length <= 256 ? stackalloc char[value.Length] : new char[value.Length];

		for (var at = 0; at < value.Length; at++)
			chars[at] = (char)value[at];

		return Read(chars);
	}

	/// <summary>Takes the payload of a length/data pair whose data tag this is; answers whether it fits the field.</summary>
	/// <param name="data">The payload, already the length the pair declared.</param>
	/// <remarks>Handed memory rather than a span, because the payload is already bytes and already its own; by default read as bytes.</remarks>
	protected internal virtual bool Read(ReadOnlyMemory<byte> data)
	{
		return Read(data.Span);
	}

	// ── what the reader does with a tag it has no class for ───────────────────────────────────────

	internal static FixField Build(int tag, ReadOnlySpan<char> value, Func<int, FixCustomField?>? factory)
	{
		if (Made(tag, factory) is not { } field)
			return new Invalid(tag, value);

		field.IsValid = field.Read(value);

		return field;
	}

	internal static FixField Build(int tag, ReadOnlySpan<byte> value, Func<int, FixCustomField?>? factory)
	{
		if (Made(tag, factory) is not { } field)
			return new Invalid(tag, value);

		field.IsValid = field.Read(value);

		return field;
	}

	internal static FixField Build(int tag, ReadOnlyMemory<byte> data, Func<int, FixCustomField?>? factory)
	{
		if (Made(tag, factory) is not { } field)
			return new Invalid(tag, data);

		field.IsValid = field.Read(data);

		return field;
	}

	static FixCustomField? Made(int tag, Func<int, FixCustomField?>? factory)
	{
		if (factory?.Invoke(tag) is not { } field)
			return null;

		return field.Tag == tag
			? field
			: throw new InvalidOperationException($"The field factory was asked for tag {tag} and built a field of tag {field.Tag}.");
	}
}
