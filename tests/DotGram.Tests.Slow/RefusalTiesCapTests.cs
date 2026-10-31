using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The cap on the ties where the grammar is built to be large: a choice of two to the thirteen copies,
/// each refused over the ties of the others. Compiling and reading it takes minutes, so it is here and
/// not with the fast tests (<c>RefusalTiesTests</c>).
/// </summary>
public sealed class RefusalTiesCapTests
{
	/// <summary>
	/// Past the cap the same choice whose widest group is a call refused over the ties ends "among others",
	/// in every form, the same on two readings, and the list stays within the cap and its mark. Below it
	/// (12 levels) the text is what it was.
	/// </summary>
	[Theory]
	[InlineData(12)]
	[InlineData(13)]
	public void A_call_refused_over_a_long_list_of_ties_is_cut_at_the_cap_and_not_before(int levels)
	{
		var want = levels < 13
			? new Dictionary<string, string>
			{
				["engine"]    = "Expected 'b', 'c', 'd', ['0'..'9' | 'e'..'z'], 'q', 'r' or ['0'..'9' | 'e'..'z' | '!'].",
				["tape"]      = "Expected 'b', 'c', 'd', 'q', ['0'..'9' | 'e'..'z'], 'r' or ['0'..'9' | 'e'..'z' | '!'].",
				["immediate"] = "Expected 'b', 'c', 'd', 'q', ['0'..'9' | 'e'..'z'], 'r' or ['0'..'9' | 'e'..'z' | '!'].",
			}
			: new Dictionary<string, string>
			{
				["engine"]    = "Expected 'b', 'c' or 'd', among others.",
				["tape"]      = "Expected 'b', 'c' or 'd', among others.",
				["immediate"] = "Expected 'b', 'c' or 'd', among others.",
			};

		foreach (var (name, direct, carrier) in TieShapes.Renderings)
		{
			var parser = TieShapes.Grammar(TieShapes.Dispatch(levels), direct, carrier);
			var first  = TieShapes.Ask(parser, "TryParseStart", "aaa?");

			Assert.True(first.Tied <= first.Cap + 1, $"{name}: {first.Tied} sets tied");
			Assert.Equal(want[name], first.Said);
			Assert.Equal(first, TieShapes.Ask(parser, "TryParseStart", "aaa?"));
		}
	}
}
