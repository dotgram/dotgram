using System;

using DotGram;

namespace DotGram.Examples;

// Host for the standalone VisualStudioToolingPlayground.gram AdditionalFile. Keeping it
// separate from the embedded playground lets navigation and generated API behavior be
// tested through the same project context without the two grammars sharing a host.
[Gram("VisualStudioToolingPlayground.gram")]
public static partial class StandaloneVisualStudioToolingPlayground
{
	// F12 on Raise in VisualStudioToolingPlayground.gram should land here.
	static decimal Raise(decimal value, decimal exponent) =>
		(decimal)Math.Pow((double)value, (double)exponent);

	// F12 on ToolingEvaluate should return to its publication in the standalone .gram file.
	public static decimal EvaluateForTooling(string expression) =>
		ToolingEvaluate(expression);
}
