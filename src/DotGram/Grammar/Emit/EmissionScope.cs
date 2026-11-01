using System;
using System.Collections.Generic;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What the file being emitted gathers while it is written and writes out at its end: the state
/// that belongs to the emission of one file rather than to any one machine.
/// </summary>
/// <remarks>
/// <para>
/// Recorded as it is written rather than found again in the text: a grammar's own C# is in that
/// text too, and a string in a construction that happens to spell a test's name is not a test.
/// Asked from every part of the emitter, static helpers included, so it is ambient: the
/// <see cref="Ambient.Emission"/> of the thread emitting the file, which goes with a walk that
/// moves to another thread (<see cref="DeepStack"/>).
/// </para>
/// </remarks>
sealed class EmissionScope
{
	/// <summary>The masks of categories the file has a test of, as <c>CSharpEmitter.Test</c> writes each.</summary>
	public SortedSet<int> CategoryMasks { get; } = [];

	/// <summary>The methods that call the author's, by their names, as <c>CSharpEmitter.HostCall</c> writes them.</summary>
	public SortedDictionary<string, string> HostCalls { get; } = new(StringComparer.Ordinal);

	/// <summary>The name of the method <c>CSharpEmitter.HostCall</c> wrote, by the call it stands for.</summary>
	public Dictionary<string, string> HostCallKeys { get; } = new(StringComparer.Ordinal);
}
