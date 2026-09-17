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
	public static FixField[] Parse(string input, FixOptions? options = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		var context = new FixGrammar.FixContext(options ?? FixOptions.Default);

		return FixGrammar.ParseFields(input, context);
	}

	public static FixField[] Parse(ReadOnlySpan<char> input, FixOptions? options = null)
	{
		return Parse(input.ToString(), options);
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	public static IEnumerable<FixField> Parse(TextReader input, FixOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null)     throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0)   throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0)  throw new ArgumentOutOfRangeException(nameof(maxRetained));

		var context = new FixGrammar.FixContext(options ?? FixOptions.Default);

		foreach (var field in FixGrammar.ReadFields(input, context, bufferSize, maxRetained))
			yield return field;
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	public static IEnumerable<FixField> Parse(Stream input, FixOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null)     throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0)   throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0)  throw new ArgumentOutOfRangeException(nameof(maxRetained));

		return Read();

		IEnumerable<FixField> Read()
		{
			var context = new FixGrammar.FixContext(options ?? FixOptions.Default);

			foreach (var field in FixGrammar.ReadFields(input, context, bufferSize, maxRetained))
				yield return field;
		}
	}

	public static FixField[] Parse(byte[] input, FixOptions? options = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		using var stream = new MemoryStream(input, writable: false);

		return Parse(stream, options).ToArray();
	}

	/// <summary>
	/// Reads log fields separated by a pipe with optional surrounding spaces.
	/// </summary>
	public static FixField[] ParseLog(string input, FixOptions? options = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		var context = new FixGrammar.FixContext(options ?? FixOptions.Default);

		return FixGrammar.ParseLogFields(input, context);
	}

	public static FixField[] ParseLog(ReadOnlySpan<char> input, FixOptions? options = null)
	{
		return ParseLog(input.ToString(), options);
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	public static IEnumerable<FixField> ParseLog(TextReader input, FixOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null)     throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0)   throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0)  throw new ArgumentOutOfRangeException(nameof(maxRetained));

		var context = new FixGrammar.FixContext(options ?? FixOptions.Default);

		foreach (var field in FixGrammar.ReadLogFields(input, context, bufferSize, maxRetained))
			yield return field;
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	public static IEnumerable<FixField> ParseLog(Stream input, FixOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null)     throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0)   throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0)  throw new ArgumentOutOfRangeException(nameof(maxRetained));

		return Read();

		IEnumerable<FixField> Read()
		{
			var context = new FixGrammar.FixContext(options ?? FixOptions.Default);

			foreach (var field in FixGrammar.ReadLogFields(input, context, bufferSize, maxRetained))
				yield return field;
		}
	}

	public static FixField[] ParseLog(byte[] input, FixOptions? options = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		using var stream = new MemoryStream(input, writable: false);

		return ParseLog(stream, options).ToArray();
	}
}
