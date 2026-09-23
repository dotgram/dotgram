using System;

using DotGram.Finance.Fix44;
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
			(input, log)                         => log ? FixParser.ParseFields(input, FixContext.WithLogFraming) : FixParser.ParseFields(input),
			(input, log)                         => log ? FixParser.ParseFields(input, FixContext.WithLogFraming) : FixParser.ParseFields(input),
			(input, log, bufferSize, maxRetained) => FixParser.ReadFields(input, FixFixtures.Reading(log, bufferSize, maxRetained)),
			(input, log, bufferSize, maxRetained) => FixParser.ReadFields(input, FixFixtures.Reading(log, bufferSize, maxRetained))),
		new FieldParser("hand",
			(input, log)                         => log ? HandFixParser.ParseLog(input) : HandFixParser.Parse(input),
			(input, log)                         => log ? HandFixParser.ParseLog(input) : HandFixParser.Parse(input),
			(input, log, bufferSize, maxRetained) => log ? HandFixParser.ParseLog(input, FixFixtures.Reading(log, bufferSize, maxRetained)) : HandFixParser.Parse(input, FixFixtures.Reading(log, bufferSize, maxRetained)),
			(input, log, bufferSize, maxRetained) => log ? HandFixParser.ParseLog(input, FixFixtures.Reading(log, bufferSize, maxRetained)) : HandFixParser.Parse(input, FixFixtures.Reading(log, bufferSize, maxRetained))),
	];
}
