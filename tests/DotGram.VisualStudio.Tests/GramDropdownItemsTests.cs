using System;

using DotGram.VisualStudio;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class GramDropdownItemsTests
{
	// The list is sorted by name, so the first entry is any declaration at all: a caret on
	// `@using`, before the first rule, must select none of them rather than the first.
	[Theory]
	[InlineData(0,  -1)]
	[InlineData(9,  -1)]
	[InlineData(10,  1)]
	[InlineData(15,  0)]
	[InlineData(19,  1)]
	[InlineData(29,  1)]
	[InlineData(30, -1)]
	[InlineData(40,  2)]
	[InlineData(50, -1)]
	public void SelectsTheInnermostItemAtThePositionOrNone(int position, int expected)
	{
		var items = new GramDropdownItem[]
		{
			new("Block16",   14,  4, 14),
			new("Fields",    10, 20, 10),
			new("Separator", 40,  5, 40),
		};

		Assert.Equal(expected, GramDropdownItems.SelectionAt(items, position));
	}
}
