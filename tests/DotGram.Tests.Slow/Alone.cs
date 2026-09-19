using System;

using Xunit;

namespace DotGram.Tests;

/// <summary>The collection xunit runs after every parallel one, with nothing beside it: what measures the process's heap.</summary>
[CollectionDefinition(DisableParallelization = true)]
public sealed class Alone;
