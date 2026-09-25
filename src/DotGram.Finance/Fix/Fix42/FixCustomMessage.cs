using System;

namespace DotGram.Finance.Fix.Fix42;

// Written by generate.py from the FIX 4.2 repository; not edited by hand. From Templates/FixCustomMessage.cs.in.

/// <summary>A message of a type FIX 4.2 does not define, built by a consumer who knows what it holds.</summary>
/// <remarks>
/// <para>
/// The context's <see cref="Fix42Context.FixMessageFactory"/> is asked, for a MsgType the package has
/// no class for, which of these to build: <c>type =&gt; type == "U1" ? new VenueQuote() : null</c>.
/// The message it answers is then handed its fields one at a time, in the order they were read: the
/// standard header and trailer are taken as every message takes them, and each other field goes to
/// <see cref="Place"/>, which puts it where the message keeps it and answers whether it belongs. A
/// field that does not is out of scope. Where the factory answers null, or there is none, the
/// message is a <see cref="FixMessage.Invalid"/>.
/// </para>
/// <para>
/// <see cref="FixMessage.Validate"/> holds the header to the standard, and then asks
/// <see cref="OnValidate(Fix42Context)"/>, which finds nothing unless the message says what it requires.
/// </para>
/// </remarks>
public abstract class FixCustomMessage : FixMessage
{
	/// <summary>A message before it has been handed its fields.</summary>
	protected FixCustomMessage() : base("", [])
	{
	}

	/// <summary>Puts a field of the body where this message keeps it; answers whether it belongs here.</summary>
	/// <param name="field">A field of the body, the header's and the trailer's taken already.</param>
	/// <returns>False where the field is not one this message holds, which is reported as out of scope.</returns>
	/// <remarks>By default every field belongs, and stays in <see cref="FixMessage.Fields"/> where it was read.</remarks>
	protected virtual bool Place(FixField field)
	{
		return true;
	}

	/// <summary>Holds the body to what this message requires, adding what is wrong with <see cref="FixMessage.AddFinding"/>.</summary>
	/// <param name="context">The context the message is validated in.</param>
	protected virtual void OnValidate(Fix42Context context)
	{
	}

	private protected sealed override void Check(Fix42Context context)
	{
		OnValidate(context);
	}

	// Called by the reader once the factory has built the message: the type and the fields it was read with.
	internal FixCustomMessage Read(string type, List<FixField> fields)
	{
		MessageType = type;
		Fields      = fields;

		foreach (var field in fields)
			if (!SetStandardField(field) && !Place(field))
				AddFinding(new FixFinding(FixRule.FieldNotInScope, field.Tag, field.Position, field, -1));

		return this;
	}
}
