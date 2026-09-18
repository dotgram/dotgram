using System;

using DotGram.Finance.Tests;

namespace DotGram.Finance.Fix44.Tests;

/// <summary>
/// The shared field-parser tests over the Fix44 oracle: it must behave as the product does,
/// or comparing the product with it proves nothing.
/// </summary>
public sealed class OracleFieldReaderTests : FixFieldReaderTests
{
	protected override FieldParser[] Parsers { get; } =
	[
		new FieldParser("fix44",
			(input, log)                         => log ? Fix44Parser.ParseLog(input) : Fix44Parser.Parse(input),
			(input, log)                         => log ? Fix44Parser.ParseLog(input) : Fix44Parser.Parse(input),
			(input, log, bufferSize, maxRetained) => log ? Fix44Parser.ParseLog(input, bufferSize, maxRetained ?? int.MaxValue) : Fix44Parser.Parse(input, bufferSize, maxRetained ?? int.MaxValue),
			(input, log, bufferSize, maxRetained) => log ? Fix44Parser.ParseLog(input, bufferSize, maxRetained ?? int.MaxValue) : Fix44Parser.Parse(input, bufferSize, maxRetained ?? int.MaxValue)),
	];
}
