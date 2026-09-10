using System;

namespace DotGram.PackageSmoke;

// What the package promises, asked of the package rather than of the repository: a
// grammar in an attribute becomes a parser during the build, in a project whose only
// connection to this repository is a version on a PackageReference, compiled by the
// oldest Roslyn the generator says it supports.
//
// The grammar is deliberately the smallest thing with a construction in it. What is
// under test is the package — that the analyzer loads under the floor, that what it
// emits compiles without the repository's own settings, and that the analyzer folder
// is the shape a compiler looks in. The language is tested everywhere else.
//
// Not in the solution, and run by CI after the pack step: it cannot be restored until
// the package it names exists.

[Gram("""
	trivia = [' ' | '	']*

	Number : @int = ['0'..'9']+ => @(int.Parse(parserText))
	Sum    : @int = left: Number & '+' & right: Number => @(left + right)

	parse Sum
	""")]
public partial class Adder
{
}

static class Program
{
	static int Main()
	{
		var match = Adder.TryParseSum("1 + 41");

		if (!match.IsSuccess)
		{
			Console.Error.WriteLine($"the generated parser refused input it should have read: {match.Error}");

			return 1;
		}

		Console.WriteLine($"1 + 41 = {match.Value}");

		return match.Value == 42 ? 0 : 1;
	}
}
