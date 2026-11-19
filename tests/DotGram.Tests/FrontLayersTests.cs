using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The stages that read a grammar say nothing in the words of one target language: what they tell
/// an author of the language the parser is generated in is asked of that language's wording.
/// </summary>
/// <remarks>
/// The guard project beside the generator (DotGram.Grammar.Front.Guard) holds the references: a stage
/// that calls an emitter does not build. It cannot hold the text of a message, which builds whatever
/// it says; this reads the same sources for that.
/// </remarks>
public class FrontLayersTests
{
	[Fact]
	public void No_message_of_a_stage_names_the_target_language_itself()
	{
		var found = new List<string>();

		foreach (var file in Stages())
		{
			var lines = File.ReadAllLines(file);

			for (var line = 0; line < lines.Length; line++)
			{
				if (lines[line].TrimStart().StartsWith("//", StringComparison.Ordinal))
					continue;

				foreach (Match literal in Literal.Matches(lines[line]))
					if (OfOneLanguage.IsMatch(literal.Value))
						found.Add($"{Path.GetRelativePath(Grammar, file).Replace('\\', '/')}({line + 1}): {literal.Value}");
			}
		}

		Assert.True(
			found.Count == 0,
			"A stage that reads a grammar names the target language in its own words; ask ITargetLanguage.Wording:\n" +
			string.Join("\n", found));
	}

	[Fact]
	public void The_stages_are_what_the_guard_compiles()
	{
		var guard = File.ReadAllText(Path.Combine(SourceRoot.Directory, "src", "DotGram.Grammar.Front.Guard", "DotGram.Grammar.Front.Guard.csproj"));

		foreach (var folder in OfTheTarget)
			Assert.Contains($@"<Compile Remove=""..\DotGram\Grammar\{folder}\**"" />", guard, StringComparison.Ordinal);

		Assert.NotEmpty(Stages());
	}

	/// <summary>The sources of the stages: Grammar/ without the folders a target language lives in.</summary>
	static IEnumerable<string> Stages()
	{
		return Directory.GetFiles(Grammar, "*.cs", SearchOption.AllDirectories)
			.Where(static file =>
			{
				var relative = Path.GetRelativePath(Grammar, file).Replace('\\', '/');

				return !OfTheTarget.Any(folder => relative.StartsWith(folder + "/", StringComparison.Ordinal)) &&
					!relative.EndsWith(".generate.cs", StringComparison.Ordinal) &&
					!InOtherWords.Contains(relative);
			})
			.Order(StringComparer.Ordinal);
	}

	/// <summary>The folders of Grammar/ that are a target language's: the guard compiles without them.</summary>
	static readonly string[] OfTheTarget = ["Emit", "CSharp"];

	/// <summary>
	/// What is left out, and why: the tree's dump, which a test reads and no author does, marks a host
	/// name the way it always has; and the catalog's titles are the reference of the one product
	/// there is (docs/diagnostics.md), held to it word for word by DiagnosticsReferenceTests.
	/// </summary>
	static readonly HashSet<string> InOtherWords = new(StringComparer.Ordinal)
	{
		"Parsing/Tree.cs",
		"DiagnosticCatalog.cs",
	};

	/// <summary>A string literal on one line, interpolated or not.</summary>
	static readonly Regex Literal = new(@"""(?:[^""\\]|\\.)*""", RegexOptions.Compiled);

	/// <summary>What only one target language, or its library, is called.</summary>
	static readonly Regex OfOneLanguage = new(
		@"C#|\.NET\b|ReadOnlySpan|ParserInput<|Nullable|IEnumerable<|TextReader|partial class|global::|System\.",
		RegexOptions.Compiled);

	static string Grammar => Path.Combine(SourceRoot.Directory, "src", "DotGram", "Grammar");
}
