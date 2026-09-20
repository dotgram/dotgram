using System;
using System.Linq;

using DotGram.Finance.Fix;

namespace DotGram.Finance.Generator.PackageSmoke;

// What the package promises, asked of the package rather than of the repository: a dictionary
// named in an attribute and listed as an additional file becomes validation during the build, in
// a project whose only connection to this repository is a version on two PackageReferences,
// compiled by the oldest Roslyn the generator says it supports.
//
// The dictionary is deliberately the smallest thing with every part in it. What is under test is
// the package — that the analyzer loads under the floor, that it finds the attribute and the file,
// that what it emits compiles without the repository's own settings, and that the analyzer folder
// is the shape a compiler looks in. What the rules say about FIX 4.4 is held elsewhere.
//
// Not in the solution, and run by CI after the pack step: it cannot be restored until the packages
// it names exist.

[FixDictionaryFile("Venue.xml")]
public static partial class VenueRules;

static class Program
{
	static int Main()
	{
		if (VenueRules.Entries.Count != 1 || !VenueRules.Entries.ContainsKey("U1"))
		{
			Console.Error.WriteLine(
				"the generated table does not describe the dictionary's one message type: " +
				string.Join(", ", VenueRules.Entries.Keys));

			return 1;
		}

		var right = Framed("35=U1|49=S|56=T|34=1|52=20260920-12:00:00|11=A|55=AAPL|54=1|");

		if (!FixMessages.TryParse(right, out var message, out var error))
		{
			Console.Error.WriteLine($"the smoke's own message does not parse: {error}");

			return 1;
		}

		var found = message!.Validate(VenueRules.Validator);

		if (found.Length != 0)
		{
			Console.Error.WriteLine(
				"the generated rules refused a message the dictionary allows: " +
				string.Join("; ", found.Select(f => f.ToString())));

			return 1;
		}

		// And they are rules, not an empty table: a required field gone, and a value outside its
		// code set, each named.
		if (!Reports("35=U1|49=S|56=T|34=1|52=20260920-12:00:00|55=AAPL|54=1|", FixRule.RequiredFieldMissing, 11) ||
			!Reports("35=U1|49=S|56=T|34=1|52=20260920-12:00:00|11=A|55=AAPL|54=Z|", FixRule.InvalidValue, 54))
			return 1;

		Console.WriteLine("DotGram.Finance.Generator: a dictionary compiled, and the rules are rules.");

		return 0;
	}

	static bool Reports(string body, FixRule rule, int tag)
	{
		if (!FixMessages.TryParse(Framed(body), out var message, out var error))
		{
			Console.Error.WriteLine($"the smoke's own message does not parse: {error}");

			return false;
		}

		var found = message!.Validate(VenueRules.Validator);

		if (found.Any(f => f.Rule == rule && f.Tag == tag))
			return true;

		Console.Error.WriteLine(
			$"the generated rules did not report {rule} for tag {tag}: " +
			string.Join("; ", found.Select(f => f.ToString())));

		return false;
	}

	static string Framed(string body)
	{
		var fields = body.Replace('|', '\u0001');
		var head   = "8=FIX.4.4\u00019=" + fields.Length + "\u0001";
		var sum    = 0;

		foreach (var c in head + fields)
			sum += c;

		return head + fields + "10=" + (sum % 256).ToString("D3") + "\u0001";
	}
}
