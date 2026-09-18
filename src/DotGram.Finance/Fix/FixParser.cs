using System;

namespace DotGram.Finance.Fix;

/// <summary>
/// Reads an ordered, flat list of FIX fields using computed dispatch without message validation.
/// </summary>
public static class FixParser
{
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
	public static FixField[] Parse(string input, FixFieldOptions? options = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		var context = new FixGrammar.FixContext(options ?? FixFieldOptions.Default);

		return FixGrammar.ParseFields(input, context);
	}

	/// <summary>
	/// Reads wire fields separated by SOH from a copy of the input.
	/// </summary>
	/// <remarks>
	/// The parser reads strings, so the input is copied into one first; no returned field refers to the copy.
	/// </remarks>
	public static FixField[] Parse(ReadOnlySpan<char> input, FixFieldOptions? options = null)
	{
		return Parse(input.ToString(), options);
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	/// <param name="input">The reader to consume; it is left open.</param>
	/// <param name="options">Null uses the standard length/data dictionary.</param>
	/// <param name="bufferSize">The initial size of the reusable buffer, in characters or bytes.</param>
	/// <param name="maxRetained">
	/// The most characters one field may take, from its tag through the separator that ends it,
	/// or a whole length/data pair; <see cref="DefaultMaxRetained"/> when not given.
	/// </param>
	/// <exception cref="IOException">A field needs more than <paramref name="maxRetained"/> characters.</exception>
	public static IEnumerable<FixField> Parse(TextReader input, FixFieldOptions? options = null, int bufferSize = 4096, int? maxRetained = null)
	{
		if (input == null)     throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0)   throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0)  throw new ArgumentOutOfRangeException(nameof(maxRetained));

		var context = new FixGrammar.FixContext(options ?? FixFieldOptions.Default);

		foreach (var field in FixGrammar.ReadFields(input, context, bufferSize, maxRetained))
			yield return field;
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	/// <param name="input">The stream to consume; it is left open.</param>
	/// <param name="options">Null uses the standard length/data dictionary.</param>
	/// <param name="bufferSize">The initial size of the reusable buffer, in characters or bytes.</param>
	/// <param name="maxRetained">
	/// The most bytes one field may take, from its tag through the separator that ends it,
	/// or a whole length/data pair; <see cref="DefaultMaxRetained"/> when not given.
	/// </param>
	/// <exception cref="IOException">A field needs more than <paramref name="maxRetained"/> bytes.</exception>
	public static IEnumerable<FixField> Parse(Stream input, FixFieldOptions? options = null, int bufferSize = 4096, int? maxRetained = null)
	{
		if (input == null)     throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0)   throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0)  throw new ArgumentOutOfRangeException(nameof(maxRetained));

		return Read();

		IEnumerable<FixField> Read()
		{
			var context = new FixGrammar.FixContext(options ?? FixFieldOptions.Default);

			foreach (var field in FixGrammar.ReadFields(input, context, bufferSize, maxRetained))
				yield return field;
		}
	}

	/// <summary>
	/// Reads wire fields separated by SOH from an array of octets.
	/// </summary>
	/// <remarks>
	/// The parser reads bytes only as a stream, so the array is wrapped in one.
	/// </remarks>
	public static FixField[] Parse(byte[] input, FixFieldOptions? options = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		using var stream = new MemoryStream(input, writable: false);

		return Parse(stream, options).ToArray();
	}

	/// <summary>
	/// Reads log fields separated by a pipe with optional surrounding spaces.
	/// </summary>
	public static FixField[] ParseLog(string input, FixFieldOptions? options = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		var context = new FixGrammar.FixContext(options ?? FixFieldOptions.Default);

		return FixGrammar.ParseLogFields(input, context);
	}

	/// <summary>
	/// Reads log fields separated by a pipe with optional surrounding spaces from a copy of the input.
	/// </summary>
	/// <remarks>
	/// The parser reads strings, so the input is copied into one first; no returned field refers to the copy.
	/// </remarks>
	public static FixField[] ParseLog(ReadOnlySpan<char> input, FixFieldOptions? options = null)
	{
		return ParseLog(input.ToString(), options);
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	/// <param name="input">The reader to consume; it is left open.</param>
	/// <param name="options">Null uses the standard length/data dictionary.</param>
	/// <param name="bufferSize">The initial size of the reusable buffer, in characters or bytes.</param>
	/// <param name="maxRetained">
	/// The most characters one field may take, from its tag through the separator that ends it,
	/// or a whole length/data pair; <see cref="DefaultMaxRetained"/> when not given.
	/// </param>
	/// <exception cref="IOException">A field needs more than <paramref name="maxRetained"/> characters.</exception>
	public static IEnumerable<FixField> ParseLog(TextReader input, FixFieldOptions? options = null, int bufferSize = 4096, int? maxRetained = null)
	{
		if (input == null)     throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0)   throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0)  throw new ArgumentOutOfRangeException(nameof(maxRetained));

		var context = new FixGrammar.FixContext(options ?? FixFieldOptions.Default);

		foreach (var field in FixGrammar.ReadLogFields(input, context, bufferSize, maxRetained))
			yield return field;
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	/// <param name="input">The stream to consume; it is left open.</param>
	/// <param name="options">Null uses the standard length/data dictionary.</param>
	/// <param name="bufferSize">The initial size of the reusable buffer, in characters or bytes.</param>
	/// <param name="maxRetained">
	/// The most bytes one field may take, from its tag through the separator that ends it,
	/// or a whole length/data pair; <see cref="DefaultMaxRetained"/> when not given.
	/// </param>
	/// <exception cref="IOException">A field needs more than <paramref name="maxRetained"/> bytes.</exception>
	public static IEnumerable<FixField> ParseLog(Stream input, FixFieldOptions? options = null, int bufferSize = 4096, int? maxRetained = null)
	{
		if (input == null)     throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0)   throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0)  throw new ArgumentOutOfRangeException(nameof(maxRetained));

		return Read();

		IEnumerable<FixField> Read()
		{
			var context = new FixGrammar.FixContext(options ?? FixFieldOptions.Default);

			foreach (var field in FixGrammar.ReadLogFields(input, context, bufferSize, maxRetained))
				yield return field;
		}
	}

	/// <summary>
	/// Reads log fields separated by a pipe with optional surrounding spaces from an array of octets.
	/// </summary>
	/// <remarks>
	/// The parser reads bytes only as a stream, so the array is wrapped in one.
	/// </remarks>
	public static FixField[] ParseLog(byte[] input, FixFieldOptions? options = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		using var stream = new MemoryStream(input, writable: false);

		return ParseLog(stream, options).ToArray();
	}
}
