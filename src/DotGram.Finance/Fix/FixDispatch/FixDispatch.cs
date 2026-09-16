using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotGram.Finance.Fix;

/// <summary>Reads an ordered, flat list of FIX fields using computed dispatch without message validation.</summary>
public static class FixDispatch
{
	static readonly FixDispatchOptions logOptions = new('|');

	public static FixField[] Parse(string input, FixDispatchOptions? options = null)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		var context = new FixDispatchContext(options ?? FixDispatchOptions.Default);
		return options?.Separator == '|'
			? FixDispatchGrammar.ParseLogFields(input, context)
			: FixDispatchGrammar.ParseFields(input, context);
	}

	public static FixField[] Parse(ReadOnlySpan<char> input, FixDispatchOptions? options = null)
		=> Parse(input.ToString(), options);

	/// <summary>Lazily reads fields through a reusable character buffer; leaves the reader open.</summary>
	public static IEnumerable<FixField> Parse(TextReader input, FixDispatchOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0) throw new ArgumentOutOfRangeException(nameof(maxRetained));
		return Read();

		IEnumerable<FixField> Read()
		{
			var state = new FixDispatchContext(options ?? FixDispatchOptions.Default);
			var fields = options?.Separator == '|'
				? FixDispatchGrammar.ReadLogFields(input, state, bufferSize, maxRetained)
				: FixDispatchGrammar.ReadFields(input, state, bufferSize, maxRetained);
			foreach (var field in fields) yield return field;
		}
	}

	/// <summary>Lazily reads fields through a reusable native byte buffer; leaves the stream open.</summary>
	public static IEnumerable<FixField> Parse(Stream input, FixDispatchOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));
		if (maxRetained <= 0) throw new ArgumentOutOfRangeException(nameof(maxRetained));
		return Read();

		IEnumerable<FixField> Read()
		{
			var state = new FixDispatchContext(options ?? FixDispatchOptions.Default);
			var fields = options?.Separator == '|'
				? FixDispatchGrammar.ReadLogFields(input, state, bufferSize, maxRetained)
				: FixDispatchGrammar.ReadFields(input, state, bufferSize, maxRetained);
			foreach (var field in fields) yield return field;
		}
	}

	public static FixField[] Parse(byte[] input, FixDispatchOptions? options = null)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		using var stream = new MemoryStream(input, writable: false);
		return Parse(stream, options).ToArray();
	}

	public static FixField[] ParseLog(string input) => Parse(input, logOptions);

	public static bool TryParse(string? input, out FixField[]? fields, out FixParseError? error, FixDispatchOptions? options = null)
	{
		if (input == null)
		{
			fields = null;
			error = new FixParseError(0, null, null, "Input is null.");
			return false;
		}
		var context = new FixDispatchContext(options ?? FixDispatchOptions.Default);
		return Result(options?.Separator == '|'
			? FixDispatchGrammar.TryParseLogFields(input, context)
			: FixDispatchGrammar.TryParseFields(input, context), out fields, out error);
	}

	public static bool TryParse(ReadOnlySpan<char> input, out FixField[]? fields, out FixParseError? error, FixDispatchOptions? options = null)
		=> TryParse(input.ToString(), out fields, out error, options);

	/// <summary>Reads to EOF and materializes all fields; use Parse for lazy enumeration.</summary>
	public static bool TryParse(TextReader input, out FixField[]? fields, out FixParseError? error, FixDispatchOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		var context = new FixDispatchContext(options ?? FixDispatchOptions.Default);
		return Result(options?.Separator == '|'
			? FixDispatchGrammar.TryParseLogFields(input, context, bufferSize, maxRetained)
			: FixDispatchGrammar.TryParseFields(input, context, bufferSize, maxRetained), out fields, out error);
	}

	/// <summary>Reads to EOF and materializes all fields; use Parse for lazy enumeration.</summary>
	public static bool TryParse(Stream input, out FixField[]? fields, out FixParseError? error, FixDispatchOptions? options = null, int bufferSize = 4096, int maxRetained = int.MaxValue)
	{
		if (input == null) throw new ArgumentNullException(nameof(input));
		var context = new FixDispatchContext(options ?? FixDispatchOptions.Default);
		return Result(options?.Separator == '|'
			? FixDispatchGrammar.TryParseLogFields(input, context, bufferSize, maxRetained)
			: FixDispatchGrammar.TryParseFields(input, context, bufferSize, maxRetained), out fields, out error);
	}

	static bool Result(FixDispatchGrammar.Match<FixField[]> match, out FixField[]? fields, out FixParseError? error)
	{
		fields = match.IsSuccess ? match.Value : null;
		error = match.IsSuccess ? null : new FixParseError((int)match.Position, null, null, match.Error ?? "Invalid FIX field syntax.");
		return match.IsSuccess;
	}
}
