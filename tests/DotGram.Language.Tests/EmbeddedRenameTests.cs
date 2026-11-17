using System;
using System.Linq;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// References and rename in a grammar written into a C# string take each occurrence where it is spelled in
/// the C# text, which is not the name's length after its start where an escape spells the name
/// (<c>\u0057ord</c>) or an escape before it moves it (<c>""</c> in a verbatim string): Visual Studio's
/// rename and VS Code's edit what is spelled, not a span the length of the name the grammar reads.
/// </summary>
public sealed class EmbeddedRenameTests
{
	const string Attribute = """
		namespace DotGram
		{
			sealed class GramAttribute(string text) : System.Attribute;
		}

		""";

	[Theory]
	[InlineData("[DotGram.Gram(\"Start = \\u0057ord & 'x'\\nWord = ['a'..'z']+\\nparse Start\\n\")]", "\\u0057ord")]
	[InlineData("[DotGram.Gram(\"Start = W\\x6Frd & 'x'\\nWord = ['a'..'z']+\\nparse Start\\n\")]", "W\\x6Frd")]
	[InlineData("[DotGram.Gram(@\"Start = \"\"x\"\" & Word\nWord = ['a'..'z']+\nparse Start\n\")]", "Word")]
	[InlineData("[DotGram.Gram(\"\"\"\n\tStart = \"x\" & Word\n\tWord = ['a'..'z']+\n\tparse Start\n\t\"\"\")]", "Word")]
	public void Rename_edits_each_name_as_it_is_spelled(string attribute, string spelled)
	{
		var source     = Attribute + attribute + "\nclass Parser;\n";
		var view       = EmbeddedNavigationTests.View(source);
		var definition = source.IndexOf("Word = ", StringComparison.Ordinal);
		var use        = source.IndexOf(spelled, source.IndexOf("Start = ", StringComparison.Ordinal), StringComparison.Ordinal);

		var found = GramNavigationProvider.References(view, use + 1);

		Assert.NotNull(found);
		Assert.Equal(
			new[] { spelled, "Word" }.Order(StringComparer.Ordinal),
			found.Spans.Select(span => source.Substring(span.Start, span.Length)).Order(StringComparer.Ordinal));

		var rename  = GramRenameProvider.Rename(found, "Letters");
		var renamed = source;

		foreach (var edit in rename.Edits.OrderByDescending(static edit => edit.Span.Start))
			renamed = renamed[..edit.Span.Start] + edit.NewText + renamed[edit.Span.End..];

		Assert.Equal(
			source.Remove(use, spelled.Length).Insert(use, "Letters").Replace("Word = ", "Letters = ", StringComparison.Ordinal),
			renamed);
		Assert.Equal(definition, found.DefinitionPosition);
	}
}
