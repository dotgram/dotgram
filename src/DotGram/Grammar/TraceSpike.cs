using System;

namespace DotGram.Grammar;

/// <summary>Spike only: whether the compilation on this thread is a trace build.</summary>
static class TraceSpike
{
	/// <summary>0 off, 1 traced, 2 traced and forwarding rules kept as written.</summary>
	[ThreadStatic]
	internal static int Mode;
}
