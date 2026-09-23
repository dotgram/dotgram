using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace DotGram.Finance.Fix44;

/// <summary>Reads an ordered, flat list of FIX 4.4 fields without message validation.</summary>
public static class Fix44Parser
{
	/// <summary>
	/// Reads wire fields separated by SOH.
	/// </summary>
	public static FixField[] Parse(string input)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		var context = new Fix44Context();

		return Fix44Grammar.ParseFields(input, context);
	}

	public static FixField[] Parse(ReadOnlySpan<char> input)
	{
		return Parse(input.ToString());
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	public static IEnumerable<FixField> Parse(TextReader input, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null)     throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0)   throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0)  throw new ArgumentOutOfRangeException(nameof(maxRetained));

		return Read();

		IEnumerable<FixField> Read()
		{
			var context = new Fix44Context();

			foreach (var field in Fix44Grammar.ReadFields(input, context, bufferSize, maxRetained))
				yield return field;
		}
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	public static IEnumerable<FixField> Parse(Stream input, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null)     throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0)   throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0)  throw new ArgumentOutOfRangeException(nameof(maxRetained));

		return Read();

		IEnumerable<FixField> Read()
		{
			var context = new Fix44Context();

			foreach (var field in Fix44Grammar.ReadFields(input, context, bufferSize, maxRetained))
				yield return field;
		}
	}

	public static FixField[] Parse(byte[] input)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		using var stream = new MemoryStream(input, writable: false);

		return Parse(stream).ToArray();
	}

	/// <summary>
	/// Reads log fields separated by a pipe with optional surrounding spaces.
	/// </summary>
	public static FixField[] ParseLog(string input)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		var context = new Fix44Context();

		return Fix44Grammar.ParseLogFields(input, context);
	}

	public static FixField[] ParseLog(ReadOnlySpan<char> input)
	{
		return ParseLog(input.ToString());
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	public static IEnumerable<FixField> ParseLog(TextReader input, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null)     throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0)   throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0)  throw new ArgumentOutOfRangeException(nameof(maxRetained));

		return Read();

		IEnumerable<FixField> Read()
		{
			var context = new Fix44Context();

			foreach (var field in Fix44Grammar.ReadLogFields(input, context, bufferSize, maxRetained))
				yield return field;
		}
	}

	/// <summary>
	/// Lazily reads fields through a reusable buffer; leaves the input open.
	/// </summary>
	public static IEnumerable<FixField> ParseLog(Stream input, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null)     throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0)   throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0)  throw new ArgumentOutOfRangeException(nameof(maxRetained));

		return Read();

		IEnumerable<FixField> Read()
		{
			var context = new Fix44Context();

			foreach (var field in Fix44Grammar.ReadLogFields(input, context, bufferSize, maxRetained))
				yield return field;
		}
	}

	public static FixField[] ParseLog(byte[] input)
	{
		if (input == null)
			throw new ArgumentNullException(nameof(input));

		using var stream = new MemoryStream(input, writable: false);

		return ParseLog(stream).ToArray();
	}

	public static bool TryParse(string? input, out FixField[]? fields, out FixParseError? error)
	{
		if (input == null)
		{
			fields = null;
			error = new FixParseError(0, null, null, "Input is null.");
			return false;
		}
		var context = new Fix44Context();
		return Result(Fix44Grammar.TryParseFields(input, context), out fields, out error);
	}

	public static bool TryParse(ReadOnlySpan<char> input, out FixField[]? fields, out FixParseError? error)
	{
		return TryParse(input.ToString(), out fields, out error);
	}

	/// <summary>Reads to EOF and materializes all fields; use Parse for lazy enumeration.</summary>
	public static bool TryParse(TextReader input, out FixField[]? fields, out FixParseError? error, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		var context = new Fix44Context();
		return Result(Fix44Grammar.TryParseFields(input, context, bufferSize, maxRetained), out fields, out error);
	}

	/// <summary>Reads to EOF and materializes all fields; use Parse for lazy enumeration.</summary>
	public static bool TryParse(Stream input, out FixField[]? fields, out FixParseError? error, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		var context = new Fix44Context();
		return Result(Fix44Grammar.TryParseFields(input, context, bufferSize, maxRetained), out fields, out error);
	}

	static bool Result(Fix44Grammar.Match<FixField[]> match, out FixField[]? fields, out FixParseError? error)
	{
		fields = match.IsSuccess ? match.Value : null;
		error = match.IsSuccess ? null : new FixParseError((int)match.Position, null, null, match.Error ?? "Invalid FIX field syntax.");
		if (fields != null)
			foreach (var field in fields)
				if (field is FixField.Invalid invalid)
				{
					error = new FixParseError(invalid.Position, null, null, invalid.Message);
					return false;
				}
		return match.IsSuccess;
	}
}
