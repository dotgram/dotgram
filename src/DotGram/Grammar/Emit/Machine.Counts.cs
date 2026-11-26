using System.Collections.Generic;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The characters the runs read, counted where a test asks for it (<see cref="CountsRules"/>, under
/// <c>DOTGRAM_COUNTS</c>).
/// </summary>
/// <remarks>
/// An entry counter says how often a rule was read, and not how far each reading went: a run of a
/// character class costs a step a character, and one scanned again from every place inside itself
/// costs the square of its length while the entries stay linear. These count every character a run
/// steps over, the same characters again on every reading, so that a test can hold that cost to a
/// count rather than to a clock.
/// </remarks>
sealed partial class Machine
{
	/// <summary>The counter of characters a rule's runs stepped over in a direct reader.</summary>
	string ScannedOf(RuleSymbol rule)
	{
		return CSharpEmitter.Inner("CountScanned", CSharpEmitter.IdentifierOf(_graph.Counted(rule)));
	}

	/// <summary>
	/// The rules a direct reader wrote a counted run for, each of which gets its counter: a counter
	/// declared for a rule with no run would be a field nothing assigns.
	/// </summary>
	readonly HashSet<RuleSymbol> _scanning = [];

	/// <summary>The counter of characters the runs of an engine stepped over.</summary>
	string Scanned => Tag.Named("Scanned");

	/// <summary>Whether an engine run was compiled, whose characters <see cref="Scanned"/> counts.</summary>
	bool _scansRuns;

	/// <summary>Counts a character a run of <paramref name="owner"/>'s direct reader stepped over.</summary>
	/// <param name="writer">Where the run's loop is being written, right after its step.</param>
	/// <param name="owner">The rule whose reader the run is in.</param>
	void CountScanned(Writer writer, RuleSymbol owner)
	{
		if (!CountsRules)
			return;

		_scanning.Add(_graph.Counted(owner));

		CountScanned(writer, ScannedOf(owner));
	}

	/// <summary>Counts a character a run stepped over, where the counters are written.</summary>
	/// <param name="writer">Where the run's loop is being written, right after its step.</param>
	/// <param name="counter">The counter the character goes to.</param>
	void CountScanned(Writer writer, string counter)
	{
		if (!CountsRules)
			return;

		writer.Line("#if DOTGRAM_COUNTS");
		writer.Line($"{counter}++;");
		writer.Line("#endif");
	}
}
