using System;
using System.IO;

namespace DotGram.Finance.Fix;

/// <summary>The schema a message is held to.</summary>
/// <remarks>
/// <para>
/// Passed to <see cref="FixMessage.Validate"/> rather than read from anywhere, so that a process
/// reading two counterparties with two schemas is expressible: each pass over an input has its
/// own, and nothing about one reaches the other.
/// </para>
/// <para>
/// What it carries is the check of each message type, held as one object of ninety-four slots so
/// that a dictionary loaded at run time replaces the slots it describes and leaves the rest. The
/// object is not part of what a consumer sees: what replaces a check is a schema, read from a
/// file, and not a delegate written by hand.
/// </para>
/// </remarks>
public sealed class FixContext
{
	/// <summary>FIX 4.4 as this package compiles it in.</summary>
	public static FixContext Default { get; } = new();

	/// <summary>The check of each message type, of each block, and of each field.</summary>
	internal FixValidators Validators { get; init; } = FixValidators.Default;

	/// <summary>This schema with a dictionary loaded over it.</summary>
	/// <param name="dictionary">A QuickFIX dictionary, or a fragment of one: the text of the file.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot, named for the slot with the extension <c>.el</c>; null writes nothing.</param>
	/// <returns>A new context; this one is unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
	/// <exception cref="FormatException">The text is not a dictionary this package can read, or names a type, a field or a block it has no class for.</exception>
	/// <remarks>
	/// <para>
	/// Loading composes. The standard, then a venue's file, then a rule a test adds, each a context
	/// built from the one before: <c>FixContext.Default.Load(venue).Load(fragment)</c>. What a file
	/// does not mention it has no opinion about, so a fragment need say only what it adds.
	/// </para>
	/// <para>
	/// What a message or a block requires is added to what the schema already asked, since a venue
	/// asks for more than the standard and never less. What a field may hold replaces the list,
	/// since a venue that lists the values of a field lists all of them.
	/// </para>
	/// </remarks>
	public FixContext Load(string dictionary, string? emitTo = null)
	{
		if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

		return new FixContext { Validators = Validators.Load(FixDictionary.Parse(dictionary), Emitter(emitTo)) };
	}

	/// <inheritdoc cref="Load(string, string)"/>
	/// <param name="dictionary">The dictionary's text; the reader is read and left open.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot; null writes nothing.</param>
	public FixContext Load(TextReader dictionary, string? emitTo = null)
	{
		if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

		return new FixContext { Validators = Validators.Load(FixDictionary.Read(dictionary), Emitter(emitTo)) };
	}

	/// <inheritdoc cref="Load(string, string)"/>
	/// <param name="dictionary">The dictionary's octets; the stream is read and left open.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot; null writes nothing.</param>
	public FixContext Load(Stream dictionary, string? emitTo = null)
	{
		if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

		return new FixContext { Validators = Validators.Load(FixDictionary.Read(dictionary), Emitter(emitTo)) };
	}

	// The texts a load writes are the expression language's, one file a slot, so that what a
	// dictionary was turned into can be read and kept; a check that would not compile is written
	// before the refusal names it.
	static Action<string, string>? Emitter(string? directory)
	{
		if (directory is null)
			return null;

		Directory.CreateDirectory(directory);

		return (slot, text) => File.WriteAllText(Path.Combine(directory, slot + ".el"), text);
	}
}
