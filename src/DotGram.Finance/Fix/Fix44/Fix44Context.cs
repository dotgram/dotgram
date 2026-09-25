using System;
using System.Collections.Generic;
using System.IO;

namespace DotGram.Finance.Fix.Fix44;

/// <summary>What a reading of FIX 4.4 is done by: the framing, the type of every field and the length/data pairs, the checks a message is held to, and the messages a consumer builds.</summary>
/// <remarks>
/// <para>
/// A <see cref="FixContext"/> of FIX 4.4, passed to every call of <see cref="FixParser"/> and to
/// <see cref="FixMessage.Validate"/>. Immutable, and changed with <c>with</c>:
/// <c>Fix44Context.Default with { Framing = FixFraming.Log }</c>.
/// </para>
/// <para>
/// What it adds to the context every version shares is FIX 4.4's own: the types and pairs of the
/// tags the standard defines, the check of each of its message types, blocks and fields, the load
/// of a dictionary over them, and the factory of the messages it does not define.
/// </para>
/// </remarks>
public sealed record Fix44Context : FixContext
{
	// The standard's sixteen pairs, length tag to data tag, and the type of every field, built once.
	// A class of its own so that it exists before Default, which is built from it.
	static class Standard
	{
		public static readonly FixVersion Version = new(new()
		{
			{ FixTag.SignatureLength,                  FixTag.Signature                     },
			{ FixTag.SecureDataLen,                    FixTag.SecureData                    },
			{ FixTag.RawDataLength,                    FixTag.RawData                       },
			{ FixTag.XmlDataLen,                       FixTag.XmlData                       },
			{ FixTag.EncodedIssuerLen,                 FixTag.EncodedIssuer                 },
			{ FixTag.EncodedSecurityDescLen,           FixTag.EncodedSecurityDesc           },
			{ FixTag.EncodedListExecInstLen,           FixTag.EncodedListExecInst           },
			{ FixTag.EncodedTextLen,                   FixTag.EncodedText                   },
			{ FixTag.EncodedSubjectLen,                FixTag.EncodedSubject                },
			{ FixTag.EncodedHeadlineLen,               FixTag.EncodedHeadline               },
			{ FixTag.EncodedAllocTextLen,              FixTag.EncodedAllocText              },
			{ FixTag.EncodedUnderlyingIssuerLen,       FixTag.EncodedUnderlyingIssuer       },
			{ FixTag.EncodedUnderlyingSecurityDescLen, FixTag.EncodedUnderlyingSecurityDesc },
			{ FixTag.EncodedListStatusTextLen,         FixTag.EncodedListStatusText         },
			{ FixTag.EncodedLegIssuerLen,              FixTag.EncodedLegIssuer              },
			{ FixTag.EncodedLegSecurityDescLen,        FixTag.EncodedLegSecurityDesc        },
		}, FixStandard.Type, FixTag.LegInterestAccrualDate);
	}

	/// <summary>A context of FIX 4.4 as this package compiles it in, reading the wire by the standard's own pairs.</summary>
	public Fix44Context() : base(Standard.Version, FixValidators.Default)
	{
	}

	/// <summary>
	/// FIX 4.4 as this package compiles it in, read from the wire by the standard's own pairs.
	/// </summary>
	public static Fix44Context Default { get; } = new();

	/// <summary>The same, reading a lossless pipe rendering: bare <c>|</c>, spaced <c> | </c>, or a mixture.</summary>
	/// <remarks>
	/// Framing is a value here rather than a second name for every method. Which separator an input
	/// uses is a property of the input, not of the caller's wish, and a property of the input belongs
	/// in the value that describes the input.
	/// </remarks>
	public static Fix44Context WithLogFraming => Default with { Framing = FixFraming.Log };

	/// <summary>Builds the message of a MsgType FIX 4.4 does not define: <c>type =&gt; type == "U1" ? new VenueQuote() : null</c>.</summary>
	/// <remarks>
	/// Asked only of a type the package has no class for. What it builds is handed the fields; where it
	/// answers null, or there is none, the message is a <see cref="FixMessage.Invalid"/>.
	/// </remarks>
	public Func<string,FixCustomMessage?>? FixMessageFactory { get; init; }

	/// <summary>The check of each message type, of each block, and of each field.</summary>
	internal FixValidators Validators
	{
		get => (FixValidators)Checks;
		init => Checks = value;
	}

	/// <summary>This context with a dictionary loaded over its schema.</summary>
	/// <param name="dictionary">A QuickFIX dictionary, or a fragment of one: the text of the file.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot, named for the slot with the extension <c>.el</c>; null writes nothing.</param>
	/// <returns>A new context, reading as this one does; this one is unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
	/// <exception cref="FormatException">The text is not a dictionary this package can read, or a check written from it names what the model does not have.</exception>
	/// <remarks>
	/// <para>
	/// Loading composes. The standard, then a venue's file, then a rule a test adds, each a context
	/// built from the one before: <c>Fix44Context.Default.Load(venue).Load(fragment)</c>. What a file
	/// does not mention it has no opinion about, so a fragment need say only what it adds.
	/// </para>
	/// <para>
	/// A message type, a component, a group's entries or a field the file describes has the file's
	/// whole check from then on, and what the file does not say of it is no longer asked.
	/// </para>
	/// <para>
	/// The file's names are the names of the checks: a message is the class of its name, a component
	/// the interface <c>I</c> and its name, a group the entries <c>&lt;Counter&gt;Groups</c> nested
	/// in what carries it. A file that places a field where the model has no property for it does
	/// not load; a correction read over it, in the same load, can put the place right before
	/// anything is written: <see cref="Load(IEnumerable{string}, string)"/>.
	/// </para>
	/// </remarks>
	public Fix44Context Load(string dictionary, string? emitTo = null)
	{
		if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

		return (Fix44Context)Loaded(FixDictionary.Parse(dictionary), emitTo);
	}

	/// <inheritdoc cref="Load(string, string)"/>
	/// <param name="dictionary">The dictionary's text; the reader is read and left open.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot; null writes nothing.</param>
	public Fix44Context Load(TextReader dictionary, string? emitTo = null)
	{
		if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

		return (Fix44Context)Loaded(FixDictionary.Read(dictionary), emitTo);
	}

	/// <inheritdoc cref="Load(string, string)"/>
	/// <param name="dictionary">The dictionary's octets; the stream is read and left open.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot; null writes nothing.</param>
	public Fix44Context Load(Stream dictionary, string? emitTo = null)
	{
		if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

		return (Fix44Context)Loaded(FixDictionary.Read(dictionary), emitTo);
	}

	/// <summary>This context with several dictionaries loaded over its schema as one: each read over the one before.</summary>
	/// <param name="dictionaries">The texts of the files, in order: a later file's message type, component or field replaces an earlier file's.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot; null writes nothing.</param>
	/// <returns>A new context, reading as this one does; this one is unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="dictionaries"/> is null, or one of them is.</exception>
	/// <exception cref="FormatException">A text is not a dictionary this package can read, or a check written from them names what the model does not have.</exception>
	/// <remarks>
	/// What a correction to somebody else's file is for. A file and its correction loaded one after
	/// the other would have the file's checks written first, and a place the file has wrong refuses
	/// to compile before the correction is read; read as one, the correction's description of a type
	/// is the one written. <c>Fix44Context.Default.Load([theirs, corrections])</c>.
	/// </remarks>
	public Fix44Context Load(IEnumerable<string> dictionaries, string? emitTo = null)
	{
		if (dictionaries == null) throw new ArgumentNullException(nameof(dictionaries));

		var read = new List<FixDictionary>();

		foreach (var dictionary in dictionaries)
			read.Add(FixDictionary.Parse(dictionary ?? throw new ArgumentNullException(nameof(dictionaries))));

		return (Fix44Context)Loaded(FixDictionary.Over(read), emitTo);
	}

	/// <inheritdoc cref="Load(string, string)"/>
	/// <param name="fileName">The path of the dictionary's file.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot; null writes nothing.</param>
	/// <exception cref="ArgumentNullException"><paramref name="fileName"/> is null.</exception>
	public Fix44Context LoadFile(string fileName, string? emitTo = null)
	{
		if (fileName == null) throw new ArgumentNullException(nameof(fileName));

		using var stream = File.OpenRead(fileName);

		return Load(stream, emitTo);
	}
}
