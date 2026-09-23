using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

using DotGram.Finance.Fix44;
using DotGram.Finance.Tests;
using DotGram.Handwritten.Fix;

using Xunit;

namespace DotGram.Finance.Fix44.Tests;

/// <summary>
/// The product held to the oracle: FixParser and the hand parser read every standard message
/// into what Fix44Parser, a literal alternative per tag, reads it into, in every input form.
/// </summary>
public sealed class FixOracleTests
{
	[Theory]
	[MemberData(nameof(FixFixtures.Messages), MemberType = typeof(FixFixtures))]
	public void All_fixtures_match_types_values_and_locations_on_all_inputs(string name, string wire)
	{
		Assert.NotEmpty(name);

		var expected = Fix44Parser.Parse(wire);
		var bytes    = Encoding.Latin1.GetBytes(wire);

		Equal(expected, FixParser.ParseFields(wire));
		Equal(expected, FixParser.ParseFields(bytes));
		Equal(expected, FixParser.ReadFields(new StringReader(wire), new FixContext { BufferSize = 3 }));
		Equal(expected, FixParser.ReadFields(new ShortStream(bytes), new FixContext { BufferSize = 3 }));

		Equal(expected, HandFixParser.Parse(wire));
		Equal(expected, HandFixParser.Parse(bytes));
		Equal(expected, HandFixParser.Parse(new StringReader(wire), new FixContext { BufferSize = 3 }));
		Equal(expected, HandFixParser.Parse(new ShortStream(bytes), new FixContext { BufferSize = 3 }));
	}

	static void Equal(IEnumerable<FixField> expected, IEnumerable<FixField> actual)
	{
		Assert.Equal(expected.Select(Describe), actual.Select(Describe));
	}

	static string Describe(FixField field)
	{
		return field.GetType().Name + JsonSerializer.Serialize(field, field.GetType());
	}

	sealed class ShortStream(byte[] input) : MemoryStream(input)
	{
		public override int Read(byte[] buffer, int offset, int count)
		{
			return base.Read(buffer, offset, Math.Min(count, 1));
		}
	}
}
