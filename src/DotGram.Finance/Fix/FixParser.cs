using System;

namespace DotGram.Finance.Fix;

/// <summary>
/// Reads an ordered, flat list of FIX fields using computed dispatch without message validation.
/// </summary>
public static class FixParser
{
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
	public static IEnumerable<FixField> Parse(TextReader input, FixFieldOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
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
	public static IEnumerable<FixField> Parse(Stream input, FixFieldOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
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
	public static IEnumerable<FixField> ParseLog(TextReader input, FixFieldOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
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
	public static IEnumerable<FixField> ParseLog(Stream input, FixFieldOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
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

	public static FixField[] ParseLog(byte[] input, FixFieldOptions? options = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		using var stream = new MemoryStream(input, writable: false);

		return ParseLog(stream, options).ToArray();
	}
}
