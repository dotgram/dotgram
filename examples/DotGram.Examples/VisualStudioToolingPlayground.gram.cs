using System;

using DotGram;

namespace DotGram.Examples;

// Host for the standalone VisualStudioToolingPlayground.gram AdditionalFile. Keeping it
// separate from the embedded playground lets navigation and generated API behavior be
// tested through the same project context without the two grammars sharing a host.
[Gram("VisualStudioToolingPlayground.gram")]
public static partial class StandaloneVisualStudioToolingPlayground
{
	// F12 on Raise in VisualStudioToolingPlayground.gram should land here. F12 on
	// ToolingEvaluate below should eventually navigate in the opposite direction once
	// generated-API navigation is implemented.
	static decimal Raise(decimal value, decimal exponent) =>
		(decimal)Math.Pow((double)value, (double)exponent);

	public static decimal EvaluateForTooling(string expression) =>
		ToolingEvaluate(expression);
}
