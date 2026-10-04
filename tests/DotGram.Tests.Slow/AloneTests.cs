using System;
using System.Linq;
using System.Reflection;

using Xunit;
using Xunit.v3;

namespace DotGram.Tests;

/// <summary>That <see cref="Alone"/> is the collection it says it is: run after every parallel one, with nothing beside it.</summary>
/// <remarks>
/// xunit v3 names a collection whose definition gives no name after the definition's type, and a class that
/// asks for one by the name <c>"Alone"</c> lands in a collection of that name with no definition at all:
/// an ordinary one, run beside every other. Every class here that asked for <see cref="Alone"/> by name
/// was run that way, its static counters and its clock shared with whatever else was running, and the
/// counts and timings that should not move did. Asked for by type, the collection is the definition.
/// </remarks>
[Collection(typeof(Alone))]
public sealed class AloneTests
{
	[Fact]
	public void The_collection_runs_alone()
	{
		var collection = Assert.IsAssignableFrom<IXunitTestCollection>(TestContext.Current.TestCollection);

		Assert.True(collection.DisableParallelization, $"{collection.TestCollectionDisplayName} runs beside the parallel collections.");
	}

	[Fact]
	public void No_class_asks_for_a_collection_by_name()
	{
		var named = typeof(AloneTests).Assembly.GetTypes()
			.Where(static type => type.GetCustomAttributesData().Any(static attribute =>
				attribute.AttributeType == typeof(CollectionAttribute) &&
				attribute.ConstructorArguments.Any(static argument => argument.ArgumentType == typeof(string))))
			.Select(static type => type.Name)
			.ToList();

		Assert.True(named.Count == 0, "Asks for a collection by name, which is not the definition's: " + string.Join(", ", named));
	}
}
