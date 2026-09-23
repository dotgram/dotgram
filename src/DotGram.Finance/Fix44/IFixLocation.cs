using System;

namespace DotGram.Finance.Fix44;

/// <summary>
/// Where something was read in the input: its coordinates, and the door the generated parser hands
/// them in through.
/// </summary>
/// <remarks>
/// What a consumer asks of a field a finding names: <c>finding.Field</c> is one, and its
/// coordinates say where in the input to look. Offsets count UTF-16 code units for character
/// input and bytes for byte input.
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
	/// Receives the extent of the construct just recognized; called by the generated parser, as
	/// each rule that built the value finishes, and the last call is the one kept.
	/// </summary>
	/// <param name="position">The zero-based offset where it starts.</param>
	/// <param name="length">How many characters or bytes it covers.</param>
	void Locate(int position, int length);
}
