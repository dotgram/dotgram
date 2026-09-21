using System;

using DotGram.Finance.Fix;
using DotGram.Handwritten.Fix;

namespace DotGram.Finance.Tests;

/// <summary>
/// The shared field-parser tests over what ships and what is held to it: the generated
/// FixParser and the hand parser that reads the same grammar.
/// </summary>
public sealed class ProductFieldReaderTests : FixFieldReaderTests
{
	protected override FieldParser[] Parsers { get; } =
	[
		new FieldParser("generated",
			(input, log)                         => log ? FixParser.ParseFields(input, FixFieldOptions.Log) : FixParser.ParseFields(input),
			(input, log)                         => log ? FixParser.ParseFields(input, FixFieldOptions.Log) : FixParser.ParseFields(input),
			(input, log, bufferSize, maxRetained) => log ? FixParser.ReadFields(input, FixFieldOptions.Log, bufferSize, maxRetained) : FixParser.ReadFields(input, null, bufferSize, maxRetained),
			(input, log, bufferSize, maxRetained) => log ? FixParser.ReadFields(input, FixFieldOptions.Log, bufferSize, maxRetained) : FixParser.ReadFields(input, null, bufferSize, maxRetained)),
		new FieldParser("hand",
			(input, log)                         => log ? HandFixParser.ParseLog(input) : HandFixParser.Parse(input),
			(input, log)                         => log ? HandFixParser.ParseLog(input) : HandFixParser.Parse(input),
			(input, log, bufferSize, maxRetained) => log ? HandFixParser.ParseLog(input, null, bufferSize, maxRetained) : HandFixParser.Parse(input, null, bufferSize, maxRetained),
			(input, log, bufferSize, maxRetained) => log ? HandFixParser.ParseLog(input, null, bufferSize, maxRetained) : HandFixParser.Parse(input, null, bufferSize, maxRetained)),
	];
}
