using System;

namespace DotGram.Finance.Fix.Fix44;

// Written by generate.cs from the FIX 4.4 repository; not edited by hand. From Templates/FixMessage.cs.in.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

/// <summary>
/// The ninety-three messages FIX 4.4 describes, each a case of <see cref="FixMessage"/>.
/// </summary>
/// <remarks>
/// Nested, because this is a closed set and not ninety-three free names: the set is fixed, every
/// case is a <see cref="FixMessage"/>, and a switch over them is meant to be exhaustive. The
/// package already writes the field hierarchy this way one layer down — <c>FixField.Invalid</c>,
/// <c>FixField.Date</c> — and two spellings of one idea in one package is what this removes.
/// At the point of use it reads as what it is: <c>FixMessage.NewOrderSingle</c> says what the
/// thing IS where it is written, and puts the base type in front of every arm of every switch,
/// which is what makes the hierarchy visibly closed where it is taken apart.
/// </remarks>
public abstract partial class FixMessage
{
	internal FixMessage(string messageType, List<FixField> fields)
	{
		MessageType = messageType;
		Fields      = fields;
	}

	/// <summary>
	/// The MsgType, tag 35.
	/// </summary>
	/// <remarks>
	/// The type this message was read as, never null; <see cref="MsgType"/> is the field it was read from,
	/// with its position in the input.
	/// </remarks>
	public string         MessageType { get; private protected set; }
	/// <summary>
	/// All fields in wire order, including header and trailer; group entries are flattened into the list.
	/// </summary>
	/// <remarks>
	/// A plain list, as every group's is: once read, the message is the caller's. Nothing keeps it in step
	/// with the properties, so a field removed here is still where its property holds it.
	/// </remarks>
	public List<FixField> Fields      { get; private protected set; }


	/// <summary>Everything found wrong with this message, or null while nothing has been.</summary>
	// What reading found, or null while it has found nothing.
	FixReadFindings? _findings;

	/// <summary>What reading this message found wrong with it, in the order it was found; empty when nothing.</summary>
	/// <remarks>
	/// A message is read once and these do not change after: <see cref="Validate(Fix44Context)"/> answers them
	/// first, with what holding the message to a schema finds.
	/// </remarks>
	public IReadOnlyList<FixFinding> Findings => (IReadOnlyList<FixFinding>?)_findings ?? Array.Empty<FixFinding>();

	// What the reading measured of the octets this message was read from, which it does not keep: the
	// octets of the body and their sum before CheckSum, for Validate to hold BodyLength and CheckSum to;
	// -1 where it was not read from octets, or has no BodyLength.
	internal int MeasuredBodyLength = -1;
	internal int MeasuredCheckSum   = -1;

	/// <summary>Holds this message to a schema, and answers everything wrong with it.</summary>
	/// <param name="context">The schema to hold it to.</param>
	/// <returns>
	/// What reading found, <see cref="Findings"/>, and then what the schema finds; empty when nothing is
	/// wrong. A new list each call, the caller's to keep.
	/// </returns>
	/// <remarks>
	/// Nothing is kept on the message: every call asks again, of the context it is given, and calls from
	/// several threads on one message do not meet. A valid message is validated without allocating.
	/// </remarks>
	public IReadOnlyList<FixFinding> Validate(Fix44Context context)
	{
		if (context == null) throw new ArgumentNullException(nameof(context));

		var check = FixCheck.Rent(null);

		try
		{
			Run(context, check);

			return check.Found();
		}
		finally
		{
			check.Return();
		}
	}

	/// <summary>Holds this message to a schema, and adds everything wrong with it to a collection of the caller's.</summary>
	/// <param name="context">The schema to hold it to.</param>
	/// <param name="findings">What to add the findings to: what reading found, <see cref="Findings"/>, and then what the schema finds.</param>
	/// <returns>How many it added; zero where nothing is wrong.</returns>
	/// <remarks>
	/// For a caller who validates many messages into one collection, cleared between them, or adds its own
	/// rules' findings beside the schema's: no list is made per message, whatever is wrong with it.
	/// </remarks>
	public int Validate(Fix44Context context, ICollection<FixFinding> findings)
	{
		if (context  == null) throw new ArgumentNullException(nameof(context));
		if (findings == null) throw new ArgumentNullException(nameof(findings));

		var before = findings.Count;
		var check  = FixCheck.Rent(findings);

		try
		{
			Run(context, check);
		}
		finally
		{
			check.Return();
		}

		return findings.Count - before;
	}

	// What reading found first, then the standard header and trailer, which every type carries, then the
	// type itself.
	void Run(Fix44Context context, FixCheck check)
	{
		if (_findings is not null)
			foreach (var finding in _findings)
				check.Findings.Add(finding);

		context.Validators.StandardHeader(context, check, this);
		Check(context, check);
	}

	/// <summary>Asks the context for the check of this message type and runs it.</summary>
	private protected abstract void Check(Fix44Context context, FixCheck check);

	// Adds one finding of the reading. A custom message adds its own through FixCustomMessage.AddFinding,
	// which says when it may.
	internal void AddFinding(FixFinding finding)
	{
		(_findings ??= new FixReadFindings()).Add(finding);
	}

	// The reader's findings, one helper a kind, so that each of the many places a message type reads a
	// field is a call and not the finding written out.

	// A tag read twice where it is held once: on the message, or in the entry the path ends at, each step
	// a counter's tag and an index.
	private protected void Duplicate(FixField field)
	{
		AddFinding(FixFinding.At(FixRule.DuplicateField, field.Tag, field.Position, field, null));
	}

	private protected void Duplicate(FixField field, int counter, int index)
	{
		AddFinding(FixFinding.At(FixRule.DuplicateField, field.Tag, field.Position, field, [new(counter, index)]));
	}

	private protected void Duplicate(FixField field, int counter1, int index1, int counter2, int index2)
	{
		AddFinding(FixFinding.At(FixRule.DuplicateField, field.Tag, field.Position, field, [new(counter1, index1), new(counter2, index2)]));
	}

	private protected void Duplicate(FixField field, int counter1, int index1, int counter2, int index2, int counter3, int index3)
	{
		AddFinding(FixFinding.At(FixRule.DuplicateField, field.Tag, field.Position, field, [new(counter1, index1), new(counter2, index2), new(counter3, index3)]));
	}

	private protected void Duplicate(FixField field, int counter1, int index1, int counter2, int index2, int counter3, int index3, int counter4, int index4)
	{
		AddFinding(FixFinding.At(FixRule.DuplicateField, field.Tag, field.Position, field, [new(counter1, index1), new(counter2, index2), new(counter3, index3), new(counter4, index4)]));
	}

	private protected void Duplicate(FixField field, int counter1, int index1, int counter2, int index2, int counter3, int index3, int counter4, int index4, int counter5, int index5)
	{
		AddFinding(FixFinding.At(FixRule.DuplicateField, field.Tag, field.Position, field, [new(counter1, index1), new(counter2, index2), new(counter3, index3), new(counter4, index4), new(counter5, index5)]));
	}

	// The first entry of a group whose counter has not been read: entries nothing counts, said at the
	// entry by the counter's tag, in the entry the path ends at.
	private protected void Uncounted(int counter, FixField field)
	{
		AddFinding(FixFinding.At(FixRule.GroupCountMismatch, counter, field.Position, field, null));
	}

	private protected void Uncounted(int counter, FixField field, int outer, int index)
	{
		AddFinding(FixFinding.At(FixRule.GroupCountMismatch, counter, field.Position, field, [new(outer, index)]));
	}

	private protected void Uncounted(int counter, FixField field, int outer1, int index1, int outer2, int index2)
	{
		AddFinding(FixFinding.At(FixRule.GroupCountMismatch, counter, field.Position, field, [new(outer1, index1), new(outer2, index2)]));
	}

	private protected void Uncounted(int counter, FixField field, int outer1, int index1, int outer2, int index2, int outer3, int index3)
	{
		AddFinding(FixFinding.At(FixRule.GroupCountMismatch, counter, field.Position, field, [new(outer1, index1), new(outer2, index2), new(outer3, index3)]));
	}

	private protected void Uncounted(int counter, FixField field, int outer1, int index1, int outer2, int index2, int outer3, int index3, int outer4, int index4)
	{
		AddFinding(FixFinding.At(FixRule.GroupCountMismatch, counter, field.Position, field, [new(outer1, index1), new(outer2, index2), new(outer3, index3), new(outer4, index4)]));
	}

	// A field of an entry with no entry open to hold it: the group's count does not match what follows.
	private protected void Unentered(FixField field)
	{
		AddFinding(FixFinding.At(FixRule.GroupCountMismatch, field.Tag, field.Position, field, null));
	}

	// A field the type has no place for.
	private protected void OutOfScope(FixField field)
	{
		AddFinding(FixFinding.At(FixRule.FieldNotInScope, field.Tag, field.Position, field, null));
	}

	/// <summary>A message of a MsgType FIX 4.4 does not define and no message factory builds; its fields remain in wire order.</summary>
	/// <remarks>
	/// Not valid from the moment it is read: its type is what is wrong with it, and that is said once,
	/// as <see cref="FixRule.UnknownMessageType"/>. The standard header and trailer are the standard's
	/// and are read as themselves; <see cref="Validate(Fix44Context)"/> holds them to it and asks nothing more.
	/// </remarks>
	public sealed class Unknown : FixMessage
	{
		internal Unknown(string type, List<FixField> fields) : base(type, fields)
		{
			foreach (var field in fields)
				SetStandardField(field);

			AddFinding(FixFinding.At(FixRule.UnknownMessageType, FixTag.MsgType, MsgType?.Position ?? -1, MsgType, null));
		}

		/// <summary>Nothing: the type is not one the schema describes, and that has been said.</summary>
		private protected override void Check(Fix44Context context, FixCheck check)
		{
		}
	}
}
