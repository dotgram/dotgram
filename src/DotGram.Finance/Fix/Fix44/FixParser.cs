using System;

namespace DotGram.Finance.Fix.Fix44;

// Written by generate.py from the FIX 4.4 repository; not edited by hand. From Templates/FixParser.cs.in.
// Derived from the FIX Protocol specification (FIX Unified Repository, 2010 edition), Copyright FIX Protocol Limited, https://www.fixtrading.org.

/// <summary>
/// Reads an ordered, flat list of FIX fields using computed dispatch without message validation.
/// </summary>
public static partial class FixParser
{
	#region ParseFields

	/// <summary>
	/// The <see cref="FixContext.MaxRetained"/> a context has unless it is given one: 16 Mi characters
	/// from a reader, 16 MiB from a stream.
	/// </summary>
	/// <remarks>
	/// It bounds one field, from its tag through the separator that ends it, or a whole length/data
	/// pair; it does not bound the input, which is read and let go field by field. A reader's
	/// characters take two bytes each, so a field at the limit holds up to 32 MiB of buffer. A reading
	/// that needs more gives its context a larger one.
	/// </remarks>
	public static int DefaultMaxRetained => FixGrammar.DefaultMaxRetained;

	/// <summary>
	/// Reads wire fields separated by SOH.
	/// </summary>
	public static FixField[] ParseFields(string input, Fix44Context? context = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		var settings = context ?? Fix44Context.Default;
		var reading  = new FixGrammar.FixReading(settings);

		return settings.Framing == FixFraming.Log
			? FixGrammar.ParseLogFields(input, reading)
			: FixGrammar.ParseFields   (input, reading);
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	/// <param name="input">The reader to consume; it is left open.</param>
	/// <param name="context">
	/// Null uses the standard length/data dictionary; its <see cref="FixContext.BufferSize"/> and
	/// <see cref="FixContext.MaxRetained"/> say how the reader is read.
	/// </param>
	/// <exception cref="IOException">A field needs more than <see cref="FixContext.MaxRetained"/> characters.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	public static IEnumerable<FixField> ReadFields(TextReader input, Fix44Context? context = null)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		var settings = context ?? Fix44Context.Default;
		var reading  = new FixGrammar.FixReading(settings);

		foreach (var field in settings.Framing == FixFraming.Log
			? FixGrammar.ReadLogFields(input, reading, settings.BufferSize, settings.MaxRetained)
			: FixGrammar.ReadFields   (input, reading, settings.BufferSize, settings.MaxRetained))
			yield return field;
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	/// <param name="input">The stream to consume; it is left open.</param>
	/// <param name="context">
	/// Null uses the standard length/data dictionary; its <see cref="FixContext.BufferSize"/> and
	/// <see cref="FixContext.MaxRetained"/> say how the stream is read.
	/// </param>
	/// <exception cref="IOException">A field needs more than <see cref="FixContext.MaxRetained"/> bytes.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null.</exception>
	public static IEnumerable<FixField> ReadFields(Stream input, Fix44Context? context = null)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));

		var settings = context ?? Fix44Context.Default;
		var reading  = new FixGrammar.FixReading(settings);

		foreach (var field in settings.Framing == FixFraming.Log
			? FixGrammar.ReadLogFields(input, reading, settings.BufferSize, settings.MaxRetained)
			: FixGrammar.ReadFields   (input, reading, settings.BufferSize, settings.MaxRetained))
			yield return field;
	}

	/// <summary>
	/// Reads wire fields separated by SOH from an array of octets.
	/// </summary>
	/// <remarks>
	/// The array is read where it lies, with no copy; being whole, it is not bounded by
	/// <see cref="FixContext.MaxRetained"/>.
	/// </remarks>
	public static FixField[] ParseFields(byte[] input, Fix44Context? context = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		return ParseFields(new ReadOnlyMemory<byte>(input), context);
	}

	/// <summary>
	/// Reads wire fields separated by SOH from octets the caller already holds.
	/// </summary>
	/// <remarks>
	/// The octets are read where they lie, with no copy; being whole, they are not bounded by
	/// <see cref="FixContext.MaxRetained"/>. No field refers to them once the call returns: a binary
	/// field's value is a copy of its own octets.
	/// </remarks>
	public static FixField[] ParseFields(ReadOnlyMemory<byte> input, Fix44Context? context = null)
	{
		var settings = context ?? Fix44Context.Default;
		var reading  = new FixGrammar.FixReading(settings);

		return settings.Framing == FixFraming.Log
			? FixGrammar.ParseLogFields(input, reading)
			: FixGrammar.ParseFields   (input, reading);
	}

	#endregion
}
