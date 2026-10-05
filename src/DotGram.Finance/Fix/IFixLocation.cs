using System;

namespace DotGram.Finance.Fix;

/// <summary>
/// Where something was read in the input: its coordinates, and the door the generated parser hands
/// them in through.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="FixField"/> implements this interface. The generated parser calls <see cref="Locate"/> as
/// each rule that built the field finishes, with the final call recording the complete field extent; the
/// last call is the one kept. A consumer building a field directly in code or in a custom DotGram grammar
/// calls <see cref="Locate"/> to give the field its source coordinates. Offsets count UTF-16 code units
/// for character input and bytes for byte input.
/// </para>
/// <para>
/// The <see cref="Length"/> property stores the value length only, excluding the tag, the equals sign
/// and the terminator. The <see cref="Locate"/> method receives the complete extent (tag through
/// terminator) and computes <see cref="Length"/> as given length minus the tag prefix and terminator length.
/// </para>
/// </remarks>
public interface IFixLocation
{
	/// <summary>The zero-based offset where it starts: the tag, or for a binary field the tag of the length before it.</summary>
	int Position { get; }

	/// <summary>The zero-based offset where its value starts, past the tag and the equals sign.</summary>
	int ValuePosition { get; }

	/// <summary>How many characters or bytes its value covers, without the tag and without the separator.</summary>
	int Length { get; }

	/// <summary>
	/// Records the complete extent of the field as recognized by the parser, including tag, equals sign
	/// and terminator. The stored <see cref="Length"/> is computed as the given length minus the tag prefix
	/// and terminator length.
	/// </summary>
	/// <param name="position">The zero-based offset where the field starts in the input.</param>
	/// <param name="length">The complete matched extent, from the first character of the tag through the terminator, inclusive.</param>
	void Locate(int position, int length);
}
