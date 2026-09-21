using System;
using System.Collections.Generic;
using System.IO;

namespace DotGram.Finance.Fix;

/// <summary>
/// Loading a counterparty's schema: the one call that changes what validation says.
/// </summary>
/// <remarks>
/// <para>
/// It reads the file and writes the rules of every type the file describes, and it answers with
/// nothing. A file that is not a dictionary this reader accepts is a <see cref="FormatException"/>,
/// not a half-loaded schema: a consumer checks the file they are about to deploy, and validation
/// that quietly holds half of one schema and half of another is worse than a refusal at the door.
/// </para>
/// <para>
/// Nothing is given back because there is nothing to hold: a rule lives in the field of the class
/// it belongs to, and the way back is to assign the package's own rule again —
/// <c>FixMessage.NewOrderSingle.Rule = FixValidator.ValidateNewOrderSingle;</c>.
/// </para>
/// </remarks>
public static partial class FixParser
{
	/// <summary>Holds every message type a dictionary describes to that dictionary's schema.</summary>
	/// <param name="input">The dictionary's octets; the stream is read and left open.</param>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	/// <exception cref="FormatException">The file is not a dictionary this reader accepts.</exception>
	public static void LoadDictionary(Stream input)
	{
		if (input is null)
			throw new ArgumentNullException(nameof(input));

		Install(FixDictionary.Load(input));
	}

	/// <summary>Holds every message type a dictionary describes to that dictionary's schema.</summary>
	/// <param name="input">The dictionary's text; the reader is read and left open.</param>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	/// <exception cref="FormatException">The file is not a dictionary this reader accepts.</exception>
	public static void LoadDictionary(TextReader input)
	{
		if (input is null)
			throw new ArgumentNullException(nameof(input));

		Install(FixDictionary.Load(input));
	}

	/// <summary>
	/// Everything the file describes, written to the fields it belongs in — after the whole file
	/// has been read, so a refusal leaves every rule as it was.
	/// </summary>
	static void Install(FixDictionary dictionary)
	{
		var tables = new DictionaryTables(dictionary);

		FixMessageRule rule = (message, findings) => FixRules.Check(tables, message, findings);

		foreach (var type in dictionary.MessageTypes)
			if (FixRuleFields.Fields.TryGetValue(type, out var field))
				field(rule);

		// A type this package has no class for is not touched here, and that is the decision rather
		// than an omission: a message type without a class has no field to hold a rule, and what a
		// consumer does about a counterparty's own types is write the class -- FixCustomMessages
		// builds it, and it validates itself. A loaded dictionary changes the ninety-three and no more.
	}
}
