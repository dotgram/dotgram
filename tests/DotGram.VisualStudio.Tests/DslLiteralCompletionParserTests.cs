using DotGram.VisualStudio;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class DslLiteralCompletionParserTests
{
	[Theory]
	[InlineData("\"select\"", "select")]
	[InlineData("\"select\"i", "select")]
	[InlineData("'+'", "+")]
	[InlineData("'\\n'", "\n")]
	[InlineData("'\\\\'", "\\")]
	public void DecodesFixedGrammarLiterals(string expected, string insertion)
	{
		var completion = DslLiteralCompletionParser.Parse(expected);

		Assert.NotNull(completion);
		Assert.Equal(expected, completion.Value.Display);
		Assert.Equal(insertion, completion.Value.Insertion);
	}

	[Theory]
	[InlineData("['a'..'z']")]
	[InlineData("end of input")]
	public void RejectsNonLiteralExpectations(string expected) =>
		Assert.Null(DslLiteralCompletionParser.Parse(expected));
}
