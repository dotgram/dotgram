using System;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class FixRecoveryTests
{
	[Theory]
	[InlineData(FixParseMode.Strict)]
	[InlineData(FixParseMode.Lenient)]
	public void Message_validation_rejects_recovered_fields_with_the_original_diagnostic(FixParseMode mode)
	{
		var wire    = FixFixtures.Wire("0", "broken|55=END|");
		var fields  = FixParser.Parse(wire);
		var invalid = Assert.Single(fields.OfType<FixField.Invalid>());
		var options = new FixParseOptions(mode);

		Assert.False(FixMessages.TryParse(wire, out var message, out var error, options));
		Assert.Null(message);
		Assert.Equal(invalid.Position, error!.Position);
		Assert.Equal(invalid.Message, error.Reason);

		using var reader = new StringReader(wire);
		using var stream = new MemoryStream(Encoding.Latin1.GetBytes(wire));

		Assert.False(FixMessages.TryParse(reader, out message, out error, mode));
		Assert.Null(message);
		Assert.Equal(invalid.Position, error!.Position);
		Assert.Equal(invalid.Message, error.Reason);
		Assert.False(FixMessages.TryParse(stream, out message, out error, mode));
		Assert.Null(message);
		Assert.Equal(invalid.Position, error!.Position);
		Assert.Equal(invalid.Message, error.Reason);
		Assert.False(FixMessages.TryBuild(wire, fields, out message, out error, options));
		Assert.Null(message);
		Assert.Equal(invalid.Position, error!.Position);
		Assert.Equal(invalid.Message, error.Reason);
	}
}
