using System;
using System.Collections.Generic;

namespace DotGram.Finance.Fix;

/// <summary>Checks one message and adds to <paramref name="findings"/> whatever is wrong with it.</summary>
/// <param name="message">The message to check.</param>
/// <param name="findings">Where to put what is wrong; it may already hold findings of another rule.</param>
/// <remarks>
/// <para>
/// One shape throughout, taking the base message, because one table holds them all. A rule written
/// for a message type the package models casts to that type itself, knowing which it is.
/// </para>
/// <para>
/// A rule is called once per message and may be called from several threads at once, so it must not
/// keep state between calls. It adds and does not clear: what is already in the list is another
/// rule's answer about the same message.
/// </para>
/// </remarks>
public delegate void FixMessageRule(FixMessage message, List<FixFinding> findings);

/// <summary>
/// The schema a message is held to, and the rules that hold it.
/// </summary>
/// <remarks>
/// <para>
/// A message is asked whether it is right — <see cref="FixMessage.Validate(FixValidator)"/> — and
/// this is what it is asked against. Checking is a layer over a built message and not a mode of
/// building one (D53): what a parse does is read the wire, what this does is hold the result to a
/// schema. Keeping them apart is what lets a counterparty's dictionary be data — reading is fixed
/// at build time and checking is not, so a schema richer than FIX 4.4's can arrive at run time
/// without touching the parser.
/// </para>
/// <para>
/// <strong>The rules are a table, and it is the consumer's to write.</strong> A rule can be
/// replaced at any moment, loading a dictionary is simply a call that writes many entries, and the
/// last write wins. Nothing is restored for you and nothing is copied behind your back; putting an
/// entry back is reading <see cref="Compiled"/> and writing it. Reading an entry while another
/// thread replaces it is safe — a reader sees one rule or the other — and the one hazard is order:
/// a dictionary loaded after a rule was replaced overwrites it.
/// </para>
/// <para>
/// <strong><see cref="Standard"/> is shared and cannot be written to.</strong> It is what
/// <see cref="FixMessage.Validate()"/> asks, so a write to it would change the answer for every
/// caller in the process, including code that never asked for a dictionary to be loaded. Make your
/// own with <c>new FixValidator()</c>, which starts from the same rules.
/// </para>
/// <para>
/// It answers with every finding. A reader chasing a disagreement with a counterparty is ill
/// served by a check that stops at the first: they would run it again for each of the rest.
/// </para>
/// </remarks>
public sealed class FixValidator
{
	static readonly FixFinding[] Nothing = [];

	readonly Dictionary<string, FixMessageRule> rules;
	readonly bool                               shared;

	FixValidator(bool shared)
	{
		rules       = new Dictionary<string, FixMessageRule>(StringComparer.Ordinal);
		this.shared = shared;
	}

	/// <summary>A validator whose rules are the ones this package compiles in, and yours to replace.</summary>
	public FixValidator() : this(shared: false) { }

	/// <summary>The rules this package compiles in, shared by every caller and not written to.</summary>
	/// <remarks>
	/// This is what <c>FixParseMode.Strict</c> checked while validation lived inside building, in
	/// its new home. A consumer moving across writes <c>message.Validate()</c> where they wrote
	/// <c>FixParseMode.Strict</c>.
	/// </remarks>
	public static FixValidator Standard { get; } = new(shared: true);

	/// <summary>
	/// The rule this package compiles in, which holds a message to FIX 4.4 whatever its type.
	/// </summary>
	/// <remarks>
	/// The entry every message type starts at, and what to write back to undo a replacement. It is
	/// one rule rather than one per type because it reads the type off the message it is given.
	/// </remarks>
	public static FixMessageRule Compiled { get; } =
		static (message, findings) => FixRules.Check(CompiledTables.Instance, message, findings);

	/// <summary>The rule a message type is held to.</summary>
	/// <param name="messageType">The MsgType, tag 35.</param>
	/// <value>
	/// The rule written for that type, or <see cref="Compiled"/> where none has been: an entry is
	/// never null, and a type nobody has written an entry for is held to the compiled rule, which
	/// reports the type as unknown if the schema does not describe it.
	/// </value>
	/// <exception cref="ArgumentNullException"><paramref name="messageType"/> is null, or the value written is.</exception>
	/// <exception cref="InvalidOperationException">
	/// The validator is <see cref="Standard"/>, which is shared and cannot be written to.
	/// </exception>
	public FixMessageRule this[string messageType]
	{
		get
		{
			if (messageType is null)
				throw new ArgumentNullException(nameof(messageType));

			return rules.TryGetValue(messageType, out var rule) ? rule : Compiled;
		}

		set
		{
			if (messageType is null)
				throw new ArgumentNullException(nameof(messageType));

			if (value is null)
				throw new ArgumentNullException(nameof(value));

			Mine();

			rules[messageType] = value;
		}
	}

	/// <summary>
	/// Writes a rule for every message type a dictionary describes, replacing what was there.
	/// </summary>
	/// <param name="dictionary">A dictionary some consumer read.</param>
	/// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
	/// <exception cref="InvalidOperationException">
	/// The validator is <see cref="Standard"/>, which is shared and cannot be written to.
	/// </exception>
	/// <remarks>
	/// Many entries, one call, and the last write wins: a type the dictionary does not describe is
	/// left at whatever it was, and a rule replaced before this call is overwritten by it.
	/// </remarks>
	public void Load(FixDictionary dictionary)
	{
		if (dictionary is null)
			throw new ArgumentNullException(nameof(dictionary));

		Mine();

		var tables = new DictionaryTables(dictionary);

		foreach (var type in dictionary.MessageTypes)
			rules[type] = (message, findings) => FixRules.Check(tables, message, findings);
	}

	void Mine()
	{
		if (shared)
			throw new InvalidOperationException(
				"FixValidator.Standard is shared by every caller in the process and cannot be written to: " +
				"a rule written here would change what FixMessage.Validate() answers for code that never " +
				"asked. Use new FixValidator(), which starts from the same rules.");
	}

	// A message is asked whether it is valid, so the verb is FixMessage.Validate and this is what
	// stands behind it (D69). One public way in, and it reads the way a consumer says the thing.
	internal FixFinding[] Check(FixMessage message)
	{
		var found = new List<FixFinding>();

		this[message.MessageType](message, found);

		return found.Count == 0 ? Nothing : found.ToArray();
	}
}
