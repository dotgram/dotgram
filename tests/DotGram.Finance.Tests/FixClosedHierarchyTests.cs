using System;
using System.Reflection;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

// The field classes and the contexts are a closed set: a type outside the package cannot derive from
// them, so every case a switch over a field or a context names is the whole of it.
public sealed class FixClosedHierarchyTests
{
	const BindingFlags Declared = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

	[Theory]
	[InlineData(typeof(FixField))]
	[InlineData(typeof(FixField.Typed<>))]
	public void NoConstructorOfAFieldIsVisibleToAnOutsideSubclass(Type type)
	{
		var constructors = type.GetConstructors(Declared);

		Assert.NotEmpty(constructors);

		foreach (var constructor in constructors)
			Assert.False(constructor.IsPublic || constructor.IsFamily || constructor.IsFamilyOrAssembly, constructor.ToString());
	}

	// A record's own copy members are visible to a derived type, so the one that keeps it out is a
	// member the package declares: abstract, and visible only inside it.
	[Fact]
	public void ContextHasAnAbstractMemberNoOutsideTypeCanImplement()
	{
		Assert.Contains(typeof(FixContext).GetMethods(Declared), method => method.IsAbstract && method.IsFamilyAndAssembly);
	}
}
