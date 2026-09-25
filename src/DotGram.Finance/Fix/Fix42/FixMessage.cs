using System;

namespace DotGram.Finance.Fix.Fix42;

// Written by generate.py from the FIX 4.2 repository; not edited by hand. From Templates/FixMessage.cs.in.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

/// <summary>
/// The forty-six messages FIX 4.2 describes, each a case of <see cref="FixMessage"/>.
/// </summary>
/// <remarks>
/// Nested, because this is a closed set and not forty-six free names: the set is fixed, every
/// case is a <see cref="FixMessage"/>, and a switch over them is meant to be exhaustive. The
/// package already writes the field hierarchy this way one layer down — <c>FixField.Invalid</c>,
/// <c>FixField.Date</c> — and two spellings of one idea in one package is what this removes.
/// At the point of use it reads as what it is: <c>FixMessage.NewOrderSingle</c> says what the
/// thing IS where it is written, and puts the base type in front of every arm of every switch,
/// which is what makes the hierarchy visibly closed where it is taken apart.
/// </remarks>
public abstract partial class FixMessage : IFixFindings
{
	internal FixMessage(string messageType, List<FixField> fields)
	{
		MessageType = messageType;
		Fields      = fields;
	}

	/// <summary>
	/// The MsgType, tag 35.
	/// </summary>
	public string         MessageType { get; private protected set; }
	/// <summary>
	/// All fields in wire order, including header and trailer; group entries are flattened into the list.
	/// </summary>
	public List<FixField> Fields      { get; private protected set; }


	/// <summary>Everything found wrong with this message, or null while nothing has been.</summary>
	public List<FixFinding>? InvalidFindings { get; private set; }

	/// <summary>Whether nothing has been found wrong with this message.</summary>
	/// <remarks>
	/// True before <see cref="Validate"/> is called means only that the reading found nothing: the
	/// message has not been held to the schema yet.
	/// </remarks>
	public bool IsValid => InvalidFindings is null;

	// What the reading measured of the octets this message was read from, which it does not keep: the
	// octets of the body and their sum before CheckSum, for Validate to hold BodyLength and CheckSum to;
	// -1 where it was not read from octets, or has no BodyLength.
	internal int MeasuredBodyLength = -1;
	internal int MeasuredCheckSum   = -1;

	// Only so that a second Validate does not fill the findings twice. One pass over an input is
	// one context, so a message is validated once and a second call answers what the first did.
	bool _validated;

	/// <summary>Holds this message to the schema, adding what is wrong to <see cref="InvalidFindings"/>.</summary>
	/// <param name="context">The schema to hold it to.</param>
	/// <returns>Whether nothing is wrong with it.</returns>
	public bool Validate(Fix42Context context)
	{
		if (context == null) throw new ArgumentNullException(nameof(context));

		if (_validated)
			return IsValid;

		_validated = true;

		// The standard header and trailer first, which every type carries, then the type itself.
		context.Validators.StandardHeader(context, this);
		Check(context);

		return IsValid;
	}

	/// <summary>Asks the context for the check of this message type and runs it.</summary>
	private protected abstract void Check(Fix42Context context);

	/// <summary>Adds one finding to this message.</summary>
	/// <param name="finding">What is wrong.</param>
	protected internal void AddFinding(FixFinding finding)
	{
		(InvalidFindings ??= []).Add(finding);
	}

	void IFixFindings.AddFinding(FixFinding finding)
	{
		AddFinding(finding);
	}

	/// <summary>A message of a MsgType FIX 4.2 does not define and no message factory builds; its fields remain in wire order.</summary>
	/// <remarks>
	/// Not valid from the moment it is read: its type is what is wrong with it, and that is said once,
	/// as <see cref="FixRule.UnknownMessageType"/>. The standard header and trailer are the standard's
	/// and are read as themselves; <see cref="Validate"/> holds them to it and asks nothing more.
	/// </remarks>
	public sealed class Invalid : FixMessage
	{
		internal Invalid(string type, List<FixField> fields) : base(type, fields)
		{
			foreach (var field in fields)
				SetStandardField(field);

			AddFinding(new FixFinding(FixRule.UnknownMessageType, FixTag.MsgType, MsgType?.Position ?? 0, MsgType, -1));
		}

		/// <summary>Nothing: the type is not one the schema describes, and that has been said.</summary>
		private protected override void Check(Fix42Context context)
		{
		}
	}
}
