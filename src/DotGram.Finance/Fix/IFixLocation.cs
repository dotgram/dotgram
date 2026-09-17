using System;

namespace DotGram.Finance.Fix;

/// <summary>
/// Receives source coordinates from the generated parser.
/// </summary>
public interface IFixLocation
{
	void Locate(int position, int length);
}
