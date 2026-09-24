using System;

namespace DotGram.Finance.Fix44;

/// <summary>Builds the field of a tag FIX 4.4 does not define, from the tag and its value; null where it does not know the tag.</summary>
/// <param name="tag">The tag the field is read under.</param>
/// <param name="value">The value as the wire had it, one character an octet; it may not be kept.</param>
/// <returns>The field, whose tag is <paramref name="tag"/>, or null.</returns>
public delegate FixCustomField? FixFieldFactory(int tag, ReadOnlySpan<char> value);

/// <summary>A field of a tag FIX 4.4 does not define, built by the context's <see cref="FixContext.FixFieldFactory"/>.</summary>
/// <remarks>
/// Every such field is a <see cref="FixCustomField{T}"/>, built the way the standard's fields are: the
/// value is read first, by the <see cref="FixConvert"/> conversion of its type, and handed to the field.
/// Use it as it is, or derive a class of your own from it, to name the field or to hold its value to
/// more.
/// </remarks>
public abstract class FixCustomField : FixField
{
	private protected FixCustomField(int tag, bool isValid) : base(tag, isValid)
	{
	}

	// ── what the reader does with a tag it has no class for ───────────────────────────────────────

	internal static FixField Build(int tag, ReadOnlySpan<char> value, FixFieldFactory? factory)
	{
		return Made(tag, value, factory) ?? (FixField)new Invalid(tag, value);
	}

	internal static FixField Build(int tag, ReadOnlySpan<byte> value, FixFieldFactory? factory)
	{
		if (factory is null)
			return new Invalid(tag, value);

		// The factory reads characters: each octet is the character of the same code.
		Span<char> chars = value.Length <= 256 ? stackalloc char[value.Length] : new char[value.Length];

		for (var at = 0; at < value.Length; at++)
			chars[at] = (char)value[at];

		return Made(tag, chars, factory) ?? (FixField)new Invalid(tag, value);
	}

	internal static FixField Build(int tag, ReadOnlyMemory<byte> data, FixFieldFactory? factory)
	{
		return Build(tag, data.Span, factory);
	}

	static FixCustomField? Made(int tag, ReadOnlySpan<char> value, FixFieldFactory? factory)
	{
		if (factory?.Invoke(tag, value) is not { } field)
			return null;

		return field.Tag == tag
			? field
			: throw new InvalidOperationException($"The field factory was asked for tag {tag} and built a field of tag {field.Tag}.");
	}
}

/// <summary>A field of a tag FIX 4.4 does not define, whose value is a <typeparamref name="T"/>.</summary>
/// <typeparam name="T">What the value converts to.</typeparam>
/// <param name="tag">The tag the field is read under.</param>
/// <param name="value">
/// Whether the value is valid, and the value: what a <see cref="FixConvert"/> conversion answers, or a
/// derived class's own check of it, <c>(converted.Valid &amp;&amp; check, converted.Value)</c>.
/// </param>
public class FixCustomField<T>(int tag, (bool Valid, T Value) value) : FixCustomField(tag, value.Valid)
{
	readonly T _value = value.Value;

	/// <summary>The value, where it is valid.</summary>
	/// <exception cref="InvalidOperationException"><see cref="FixField.IsValid"/> is false.</exception>
	public T Value => IsValid ? _value : throw new InvalidOperationException("The field has no valid typed value; inspect its original wire value.");

	/// <summary>Returns the value and reports whether it is valid.</summary>
	/// <param name="result">The value, assigned even where it is not valid.</param>
	/// <returns>The value of <see cref="FixField.IsValid"/>.</returns>
	public bool TryGetValue(out T result)
	{
		result = _value;
		return IsValid;
	}
}
