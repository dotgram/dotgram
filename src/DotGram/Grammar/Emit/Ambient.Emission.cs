using System;

namespace DotGram.Grammar;

/// <summary>The part of the generator's per-thread state that belongs to the file being emitted.</summary>
sealed partial class Ambient
{
	/// <summary>What the file being emitted gathers as it is written; null outside <c>CSharpEmitter.Emit</c>.</summary>
	public Emit.EmissionScope? Emission { get; set; }
}
