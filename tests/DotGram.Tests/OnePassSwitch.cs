#if DOTGRAM_ONE_PASS
using System.Runtime.CompilerServices;

using DotGram.Grammar;

namespace DotGram.Tests;

/// <summary>
/// An experiment's switch: a build that defines <c>DOTGRAM_ONE_PASS</c> compiles every grammar of
/// the repository to read a refused input once, recording, and this makes the grammars these tests
/// compile for themselves read the same way (GramCompilerOptions.OnePassRefusal).
/// </summary>
static class OnePassSwitch
{
	[ModuleInitializer]
	internal static void Set()
	{
		GramCompilerOptions.OnePassDefault = true;
	}
}
#endif
