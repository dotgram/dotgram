using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotGram.Finance.Fix;

/// <summary>Reads an ordered, flat list of FIX 4.4 fields without message validation.</summary>
public static class Fix44
{
	static readonly FixFieldOptions logOptions = new('|');

	public static FixField[] Parse(string input, FixFieldOptions? options = null)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		var context = new FixContext(options);
		return options?.Separator == '|'
			? FixGrammar.ParseLogFields(input, context)
			: FixGrammar.ParseFields(input, context);
	}

	public static FixField[] Parse(ReadOnlySpan<char> input, FixFieldOptions? options = null)
		=> Parse(input.ToString(), options);

	/// <summary>Lazily reads fields through a reusable character buffer; leaves the reader open.</summary>
	public static IEnumerable<FixField> Parse(TextReader input, FixFieldOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0) throw new ArgumentOutOfRangeException(nameof(maxRetained));
		return Read();

		IEnumerable<FixField> Read()
		{
			var state = new FixContext(options);
			var fields = options?.Separator == '|'
				? FixGrammar.ReadLogFields(input, state, bufferSize, maxRetained)
				: FixGrammar.ReadFields(input, state, bufferSize, maxRetained);
			foreach (var field in fields) yield return field;
		}
	}

	/// <summary>Lazily reads fields through a reusable native byte buffer; leaves the stream open.</summary>
	public static IEnumerable<FixField> Parse(Stream input, FixFieldOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0) throw new ArgumentOutOfRangeException(nameof(maxRetained));
		return Read();

		IEnumerable<FixField> Read()
		{
			var state = new FixContext(options);
			var fields = options?.Separator == '|'
				? FixGrammar.ReadLogFields(input, state, bufferSize, maxRetained)
				: FixGrammar.ReadFields(input, state, bufferSize, maxRetained);
			foreach (var field in fields) yield return field;
		}
	}

	public static FixField[] Parse(byte[] input, FixFieldOptions? options = null)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		using var stream = new MemoryStream(input, writable: false);
		return Parse(stream, options).ToArray();
	}

	public static FixField[] ParseLog(string input) => Parse(input, logOptions);

	public static bool TryParse(string? input, out FixField[]? fields, out FixParseError? error, FixFieldOptions? options = null)
	{
		if (input == null)
		{
			fields = null;
			error = new FixParseError(0, null, null, "Input is null.");
			return false;
		}
		var context = new FixContext(options);
		return Result(options?.Separator == '|'
			? FixGrammar.TryParseLogFields(input, context)
			: FixGrammar.TryParseFields(input, context), out fields, out error);
	}

	public static bool TryParse(ReadOnlySpan<char> input, out FixField[]? fields, out FixParseError? error, FixFieldOptions? options = null)
		=> TryParse(input.ToString(), out fields, out error, options);

	/// <summary>Reads to EOF and materializes all fields; use Parse for lazy enumeration.</summary>
	public static bool TryParse(TextReader input, out FixField[]? fields, out FixParseError? error, FixFieldOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		var context = new FixContext(options);
		return Result(options?.Separator == '|'
			? FixGrammar.TryParseLogFields(input, context, bufferSize, maxRetained)
			: FixGrammar.TryParseFields(input, context, bufferSize, maxRetained), out fields, out error);
	}

	/// <summary>Reads to EOF and materializes all fields; use Parse for lazy enumeration.</summary>
	public static bool TryParse(Stream input, out FixField[]? fields, out FixParseError? error, FixFieldOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		var context = new FixContext(options);
		return Result(options?.Separator == '|'
			? FixGrammar.TryParseLogFields(input, context, bufferSize, maxRetained)
			: FixGrammar.TryParseFields(input, context, bufferSize, maxRetained), out fields, out error);
	}

	static bool Result(FixGrammar.Match<FixField[]> match, out FixField[]? fields, out FixParseError? error)
	{
		fields = match.IsSuccess ? match.Value : null;
		error = match.IsSuccess ? null : new FixParseError((int)match.Position, null, null, match.Error ?? "Invalid FIX field syntax.");
		return match.IsSuccess;
	}
}
