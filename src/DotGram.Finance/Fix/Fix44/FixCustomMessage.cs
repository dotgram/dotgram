using System;

namespace DotGram.Finance.Fix.Fix44;

// Written by generate.cs from the FIX 4.4 repository; not edited by hand. From Templates/FixCustomMessage.cs.in.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

/// <summary>A message of a type FIX 4.4 does not define, built by a consumer who knows what it holds.</summary>
/// <remarks>
/// <para>
/// The context's <see cref="Fix44Context.FixMessageFactory"/> is asked, for a MsgType the package has
/// no class for, which of these to build: <c>type =&gt; type == "U1" ? new VenueQuote() : null</c>.
/// The message it answers is then handed its fields one at a time, in the order they were read: the
/// standard header and trailer are taken as every message takes them, and each other field goes to
/// <see cref="Place"/>, which puts it where the message keeps it and answers whether it belongs. A
/// field that does not is out of scope. Where the factory answers null, or there is none, the
/// message is a <see cref="FixMessage.Unknown"/>.
/// </para>
/// <para>
/// <see cref="FixMessage.Validate(Fix44Context)"/> holds the header to the standard, and then asks
/// <see cref="OnValidate"/>, which finds nothing unless the message says what it requires. A finding of
/// the reading is added with <see cref="AddFinding"/> from <see cref="Place"/>; a finding of the
/// validation goes to the collection <see cref="OnValidate"/> is given.
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

	/// <summary>Holds the body to what this message requires, adding what is wrong to <paramref name="findings"/>.</summary>
	/// <param name="context">The context the message is validated in.</param>
	/// <param name="findings">
	/// Where this validation's findings go, made with <see cref="FixFinding.Missing"/> and
	/// <see cref="FixFinding.Of(FixRule, FixField, FixGroupStep[])"/>. It is the call's: it may hold findings of
	/// the message already, and is not to be kept after the call returns.
	/// </param>
	/// <remarks>Called on every <c>Validate</c>, and from as many threads as validate the message: it keeps nothing on the message.</remarks>
	protected virtual void OnValidate(Fix44Context context, ICollection<FixFinding> findings)
	{
	}

	// The thread the reader hands the fields to Place on, while it does, or 0: the one time and the one
	// thread a finding may be added to the message. What reading found is not changed once the message has
	// been read, and a Place that hands the message to another thread does not give that thread the right.
	int _reader;

	/// <summary>Adds one finding of the reading to this message, from <see cref="Place"/> while it is read, on the thread that reads it.</summary>
	/// <param name="finding">What is wrong.</param>
	/// <exception cref="InvalidOperationException">Called at any other time: what reading found is not changed after the message has been read, and a finding of the validation goes to the collection <see cref="OnValidate"/> is given.</exception>
	protected new void AddFinding(FixFinding finding)
	{
		if (finding == null) throw new ArgumentNullException(nameof(finding));

		if (_reader != Environment.CurrentManagedThreadId)
			throw new InvalidOperationException("A finding is added to a message only from Place, while it is read; OnValidate adds its own to the collection it is given.");

		base.AddFinding(finding);
	}

	private protected sealed override void Check(Fix44Context context, FixCheck check)
	{
		OnValidate(context, check.Findings);
	}

	// Called by the reader once the factory has built the message: the type and the fields it was read with.
	internal FixCustomMessage Read(string type, List<FixField> fields)
	{
		MessageType = type;
		Fields      = fields;

		_reader = Environment.CurrentManagedThreadId;

		try
		{
			foreach (var field in fields)
				if (!SetStandardField(field) && !Place(field))
					OutOfScope(field);
		}
		finally
		{
			_reader = 0;
		}

		return this;
	}
}
