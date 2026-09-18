using System;

namespace DotGram.Finance.Fix;

/// <summary>
/// Receives source coordinates from the generated parser.
/// </summary>
public interface IFixLocation
{
	/// <summary>
	/// Receives the extent of the construct just recognized.
	/// </summary>
	/// <param name="position">The zero-based offset where it starts.</param>
	/// <param name="length">How many characters or bytes it covers.</param>
	void Locate(int position, int length);
}
