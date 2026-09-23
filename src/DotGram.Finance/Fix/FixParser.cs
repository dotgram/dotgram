using System;

namespace DotGram.Finance.Fix;

/// <summary>
/// Reads an ordered, flat list of FIX fields using computed dispatch without message validation.
/// </summary>
public static partial class FixParser
{
	#region ParseFields

	/// <summary>
	/// The <c>maxRetained</c> a call gets when it gives none: 16 Mi characters from a reader, 16 MiB
	/// from a stream.
	/// </summary>
	/// <remarks>
	/// It bounds one field, from its tag through the separator that ends it, or a whole length/data
	/// pair; it does not bound the input, which is read and let go field by field. A reader's
	/// characters take two bytes each, so a field at the limit holds up to 32 MiB of buffer. A call
	/// that needs more passes its own <c>maxRetained</c>.
	/// </remarks>
	public static int DefaultMaxRetained => FixGrammar.DefaultMaxRetained;

	/// <summary>
	/// Reads wire fields separated by SOH.
	/// </summary>
	public static FixField[] ParseFields(string input, FixContext? context = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		var settings = context ?? FixContext.Default;
		var reading  = new FixGrammar.FixReading(settings);

		return settings.Framing == FixFraming.Log
			? FixGrammar.ParseLogFields(input, reading)
			: FixGrammar.ParseFields(input, reading);
	}

	/// <summary>
	/// Reads wire fields separated by SOH from a copy of the input.
	/// </summary>
	/// <remarks>
	/// The parser reads strings, so the input is copied into one first; no returned field refers to the copy.
	/// </remarks>
	public static FixField[] ParseFields(ReadOnlySpan<char> input, FixContext? context = null)
	{
		return ParseFields(input.ToString(), context);
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	/// <param name="input">The reader to consume; it is left open.</param>
	/// <param name="context">Null uses the standard length/data dictionary.</param>
	/// <param name="bufferSize">The initial size of the reusable buffer, in characters or bytes.</param>
	/// <param name="maxRetained">
	/// The most characters one field may take, from its tag through the separator that ends it,
	/// or a whole length/data pair; <see cref="DefaultMaxRetained"/> when not given.
	/// </param>
	/// <exception cref="IOException">A field needs more than <paramref name="maxRetained"/> characters.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null; thrown by the call, not by enumeration.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> or <paramref name="maxRetained"/> is not positive; thrown by the call, not by enumeration.</exception>
	public static IEnumerable<FixField> ReadFields(TextReader input, FixContext? context = null, int bufferSize = 4096, int? maxRetained = null)
	{
		if (input == null)   throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

		var limit = maxRetained ?? DefaultMaxRetained;

		if (limit <= 0)
			throw new ArgumentOutOfRangeException(nameof(maxRetained));

		var settings = context ?? FixContext.Default;
		var reading  = new FixGrammar.FixReading(settings);

		foreach (var field in settings.Framing == FixFraming.Log
			? FixGrammar.ReadLogFields(input, reading, bufferSize, limit)
			: FixGrammar.ReadFields   (input, reading, bufferSize, limit))
			yield return field;
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	/// <param name="input">The stream to consume; it is left open.</param>
	/// <param name="context">Null uses the standard length/data dictionary.</param>
	/// <param name="bufferSize">The initial size of the reusable buffer, in characters or bytes.</param>
	/// <param name="maxRetained">
	/// The most bytes one field may take, from its tag through the separator that ends it,
	/// or a whole length/data pair; <see cref="DefaultMaxRetained"/> when not given.
	/// </param>
	/// <exception cref="IOException">A field needs more than <paramref name="maxRetained"/> bytes.</exception>
	/// <exception cref="ArgumentNullException"><paramref name="input"/> is null; thrown by the call, not by enumeration.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="bufferSize"/> or <paramref name="maxRetained"/> is not positive; thrown by the call, not by enumeration.</exception>
	public static IEnumerable<FixField> ReadFields(Stream input, FixContext? context = null, int bufferSize = 4096, int? maxRetained = null)
	{
		if (input == null)   throw new ArgumentNullException      (nameof(input));
		if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

		var limit = maxRetained ?? DefaultMaxRetained;

		if (limit <= 0)
			throw new ArgumentOutOfRangeException(nameof(maxRetained));

		var settings = context ?? FixContext.Default;
		var reading  = new FixGrammar.FixReading(settings);

		foreach (var field in settings.Framing == FixFraming.Log
			? FixGrammar.ReadLogFields(input, reading, bufferSize, limit)
			: FixGrammar.ReadFields   (input, reading, bufferSize, limit))
			yield return field;
	}

	/// <summary>
	/// Reads wire fields separated by SOH from an array of octets.
	/// </summary>
	/// <remarks>
	/// The array is read where it lies, with no copy; being whole, it is not bounded by <c>maxRetained</c>.
	/// </remarks>
	public static FixField[] ParseFields(byte[] input, FixContext? context = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		var settings = context ?? FixContext.Default;
		var reading  = new FixGrammar.FixReading(settings);

		return settings.Framing == FixFraming.Log
			? FixGrammar.ParseLogFields(input, reading)
			: FixGrammar.ParseFields   (input, reading);
	}

	/// <summary>
	/// Reads wire fields separated by SOH from a copy of the octets.
	/// </summary>
	/// <remarks>
	/// The parser reads an array, so the span is copied into one first; a binary field's value then
	/// refers to that copy rather than to the caller's buffer, which is what makes it safe to keep.
	/// </remarks>
	public static FixField[] ParseFields(ReadOnlySpan<byte> input, FixContext? context = null)
	{
		return ParseFields(input.ToArray(), context);
	}

	#endregion
}
