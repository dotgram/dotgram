using System;

namespace DotGram.Finance.Fix;

/// <summary>
/// Reads an ordered, flat list of FIX fields using computed dispatch without message validation.
/// </summary>
public static class FixParser
{
	static readonly FixOptions _logOptions = new('|');

	public static FixField[] Parse(string input, FixOptions? options = null)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));
		var context = new FixGrammar.FixContext(options ?? FixOptions.Default);

		return options?.Separator == '|'
			? FixGrammar.ParseLogFields(input, context)
			: FixGrammar.ParseFields(input, context);
	}

	public static FixField[] Parse(ReadOnlySpan<char> input, FixOptions? options = null)
	{
		return Parse(input.ToString(), options);
	}

	/// <summary>Lazily reads fields through a reusable character buffer; leaves the reader open.</summary>
	public static IEnumerable<FixField> Parse(TextReader input, FixOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null)    throw new ArgumentNullException(nameof(input));
		if (bufferSize  <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0) throw new ArgumentOutOfRangeException(nameof(maxRetained));

		var state  = new FixGrammar.FixContext(options ?? FixOptions.Default);
		var fields = options?.Separator == '|'
			? FixGrammar.ReadLogFields(input, state, bufferSize, maxRetained)
			: FixGrammar.ReadFields(input, state, bufferSize, maxRetained);

		foreach (var field in fields)
			yield return field;
	}

	/// <summary>Lazily reads fields through a reusable native byte buffer; leaves the stream open.</summary>
	public static IEnumerable<FixField> Parse(Stream input, FixOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null)    throw new ArgumentNullException(nameof(input));
		if (bufferSize  <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0) throw new ArgumentOutOfRangeException(nameof(maxRetained));
		return Read();

		IEnumerable<FixField> Read()
		{
			var state = new FixGrammar.FixContext(options ?? FixOptions.Default);
			var fields = options?.Separator == '|'
				? FixGrammar.ReadLogFields(input, state, bufferSize, maxRetained)
				: FixGrammar.ReadFields(input, state, bufferSize, maxRetained);
			foreach (var field in fields) yield return field;
		}
	}

	public static FixField[] Parse(byte[] input, FixOptions? options = null)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		using var stream = new MemoryStream(input, writable: false);
		return Parse(stream, options).ToArray();
	}

	public static FixField[] ParseLog(string input)
	{
		return Parse(input, _logOptions);
	}

}
