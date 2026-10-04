using System;

using Xunit;

namespace DotGram.Tests;

/// <summary>The collection xunit runs after every parallel one, with nothing beside it: what measures the process's heap.</summary>
/// <remarks>Asked for by type, <c>[Collection(typeof(Alone))]</c>: by the name <c>"Alone"</c> it is another collection, run in parallel (<see cref="AloneTests"/>).</remarks>
[CollectionDefinition(DisableParallelization = true)]
public sealed class Alone;
