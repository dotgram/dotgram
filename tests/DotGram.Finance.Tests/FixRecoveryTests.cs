using System;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

public sealed class FixRecoveryTests
{
	/// <summary>
	/// A field the reader could not read is a refusal, not a finding.
	/// </summary>
	/// <remarks>
	/// The field parser recovers so that one bad field does not cost the rest of the wire, and the
	/// message layer then refuses to build over the wreckage with the diagnostic the parser
	/// recorded. That line does not move with D53: a finding is about a field that was read, and
	/// this one was not.
	/// </remarks>
	[Fact]
	public void Message_building_rejects_recovered_fields_with_the_original_diagnostic()
	{
		var wire    = FixFixtures.Wire("0", "broken|55=END|");
		var fields  = FixParser.ParseFields(wire);
		var invalid = Assert.Single(fields.OfType<FixField.Invalid>());
		FixFieldOptions? options = null;

		Assert.False(FixParser.TryParseMessage(wire, out var message, out var error, options));
		Assert.Null(message);
		Assert.Equal(invalid.Position, error!.Position);
		Assert.Equal(invalid.Message, error.Reason);

		using var reader = new StringReader(wire);
		using var stream = new MemoryStream(Encoding.Latin1.GetBytes(wire));

		Assert.False(FixParser.TryReadMessage(reader, out message, out error));
		Assert.Null(message);
		Assert.Equal(invalid.Position, error!.Position);
		Assert.Equal(invalid.Message, error.Reason);
		Assert.False(FixParser.TryReadMessage(stream, out message, out error));
		Assert.Null(message);
		Assert.Equal(invalid.Position, error!.Position);
		Assert.Equal(invalid.Message, error.Reason);
		Assert.False(FixParser.TryBuildMessage(wire, fields, out message, out error, options));
		Assert.Null(message);
		Assert.Equal(invalid.Position, error!.Position);
		Assert.Equal(invalid.Message, error.Reason);
	}
}
