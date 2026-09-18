using System;
using System.Collections.Generic;
using System.IO;

using DotGram.Finance.Fix;

namespace DotGram.Finance.Tests;

/// <summary>
/// One way of reading FIX fields, as the shared tests call it: the generated parser, the
/// hand parser that reads the same grammar, or the Fix44 oracle.
/// </summary>
/// <remarks>
/// Each overload takes whether the input is in log framing. A null limit leaves the parser
/// its own default.
/// </remarks>
public sealed class FieldParser(
	string                                                   name,
	Func<string, bool, FixField[]>                           text,
	Func<byte[], bool, FixField[]>                           bytes,
	Func<TextReader, bool, int, int?, IEnumerable<FixField>> reader,
	Func<Stream, bool, int, int?, IEnumerable<FixField>>     stream)
{
	public string Name => name;

	public FixField[] Parse(string input)
	{
		return text(input, false);
	}

	public FixField[] ParseLog(string input)
	{
		return text(input, true);
	}

	public FixField[] Parse(byte[] input)
	{
		return bytes(input, false);
	}

	public FixField[] ParseLog(byte[] input)
	{
		return bytes(input, true);
	}

	public IEnumerable<FixField> Parse(TextReader input, int bufferSize, int? maxRetained = null)
	{
		return reader(input, false, bufferSize, maxRetained);
	}

	public IEnumerable<FixField> ParseLog(TextReader input, int bufferSize, int? maxRetained = null)
	{
		return reader(input, true, bufferSize, maxRetained);
	}

	public IEnumerable<FixField> Parse(Stream input, int bufferSize, int? maxRetained = null)
	{
		return stream(input, false, bufferSize, maxRetained);
	}

	public IEnumerable<FixField> ParseLog(Stream input, int bufferSize, int? maxRetained = null)
	{
		return stream(input, true, bufferSize, maxRetained);
	}

	public override string ToString()
	{
		return name;
	}
}
