using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The generator guide, <c>docs/guide/generator</c>, compiled through the generator and run as it is written.
/// </summary>
/// <remarks>
/// <para>
/// The fences of a page are grouped into examples by the word <c>example=name</c> in their info string. A
/// <c>```csharp</c> fence is a C# file of the example, a <c>```dotgram file=path</c> fence a <c>.gram</c>
/// file it lists as an additional file. The C# of an example is compiled with the generator attached, as a
/// project that references the package is. A diagnostic identifier among the words of its first C# fence
/// (<c>GRAM0002</c>) says the generator reports it, and nothing else at the level of a warning or above; no
/// identifier says it reports nothing at that level. <c>LangVersion=7.3</c> compiles the example at that
/// version, and a word <c>DotGramName=value</c> sets that build property, as a project that sets it
/// does (<c>DotGramTrace=true</c>). An example the generator reports no error for is then emitted, and has to compile with no error
/// anywhere and no warning in what the generator wrote.
/// </para>
/// <para>
/// A <c>```text run example=name</c> fence holds runs of the example, one a line:
/// <c>expression → value</c>. The expression is C#, evaluated in a class beside the example's own, which
/// sees every namespace the example declares. A value is shown as C# writes a literal of it, a sequence as
/// <c>[a, b]</c>, anything else by its <c>ToString</c>.
/// </para>
/// <para>
/// <c>pending</c> on the first C# fence of an example skips it, for an example that shows the behaviour of a
/// change not landed yet. Every C# or grammar fence that belongs to no example says <c>fragment</c>, so that
/// nothing is left unchecked by accident. The Russian blocks between <c>&lt;!-- ru:begin --&gt;</c> and
/// <c>&lt;!-- ru:end --&gt;</c> hold no fences and are skipped.
/// </para>
/// </remarks>
public sealed class GeneratorGuideTests
{
	[Theory]
	[MemberData(nameof(Examples))]
	public void An_example_of_the_generator_guide_does_what_the_page_says(string page, string name)
	{
		var example = All(page)[name];

		if (example.Words.Contains("pending"))
			Assert.Skip($"{page}, example {name} (line {example.Line}) shows behaviour a change has not landed yet.");

		var version = example.Words
			.Where(static word => word.StartsWith("LangVersion=", StringComparison.Ordinal))
			.Select(static word => LanguageVersionFacts.TryParse(word["LangVersion=".Length..], out var parsed) ? parsed : LanguageVersion.Latest)
			.DefaultIfEmpty(LanguageVersion.Latest)
			.First();

		var parse = CSharpParseOptions.Default.WithLanguageVersion(version);
		var trees = example.Sources
			.Select((source, at) => CSharpSyntaxTree.ParseText(source, parse, $"{name}.{at}.cs", cancellationToken: TestContext.Current.CancellationToken))
			.ToList();

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.GeneratorGuide",
			trees,
			References(),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		var driver = CSharpGeneratorDriver
			.Create(
				[new GramGenerator().AsSourceGenerator()],
				additionalTexts: example.Files.Select(static file => (AdditionalText)new GuideFile(file.Path, file.Text)),
				parseOptions: parse,
				optionsProvider: new BuildProperties(example.Words
					.Where(static word => word.StartsWith("DotGram", StringComparison.Ordinal) && word.Contains('=', StringComparison.Ordinal))
					.ToDictionary(
						static word => "build_property." + word[..word.IndexOf('=', StringComparison.Ordinal)],
						static word => word[(word.IndexOf('=', StringComparison.Ordinal) + 1)..],
						StringComparer.Ordinal)))
			.RunGeneratorsAndUpdateCompilation(compilation, out var output, out _, TestContext.Current.CancellationToken);

		var expected = example.Words.Where(static word => word.StartsWith("GRAM", StringComparison.Ordinal)).ToHashSet();
		var reported = driver.GetRunResult().Diagnostics
			.Where(one => one.Severity >= DiagnosticSeverity.Warning || expected.Contains(one.Id))
			.ToList();

		var said = $"{page}, example {name} (line {example.Line}):\n" + string.Join("\n", reported.Select(static one => one.ToString()));

		Assert.True(expected.SetEquals(reported.Select(static one => one.Id)), said);

		if (reported.Any(static one => one.Severity == DiagnosticSeverity.Error))
		{
			Assert.Empty(example.Runs);

			return;
		}

		var runner = Runner(example, trees, parse);
		var built  = output.AddSyntaxTrees(runner);

		using var stream = new MemoryStream();

		var emitted    = built.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
		var complaints = emitted.Diagnostics
			.Where(one => one.Severity == DiagnosticSeverity.Error ||
				one.Severity == DiagnosticSeverity.Warning && one.Location.SourceTree is { } tree && !trees.Contains(tree) && tree != runner)
			.ToList();

		Assert.True(complaints.Count == 0, said + "\n" + string.Join("\n", complaints.Select(static one => one.ToString())));

		if (example.Runs.Count == 0)
			return;

		var type = Assembly.Load(stream.ToArray()).GetType("DotGram.Tests.GuideRun.Runs_DotGram")!;

		for (var at = 0; at < example.Runs.Count; at++)
		{
			var (expression, answer) = example.Runs[at];

			Assert.Equal($"{expression} → {answer}", $"{expression} → {Show(type.GetMethod("R" + at.ToString(CultureInfo.InvariantCulture))!.Invoke(null, null))}");
		}
	}

	/// <summary>Every example of every page, by its page and its name.</summary>
	public static TheoryData<string, string> Examples
	{
		get
		{
			var data = new TheoryData<string, string>();

			foreach (var page in Pages())
				foreach (var name in All(page).Keys)
					data.Add(page, name);

			return data;
		}
	}

	/// <summary>A page that lost its examples checks nothing, and a fence that belongs to no example is checked by nothing.</summary>
	[Fact]
	public void Every_fence_of_the_guide_is_an_example_or_says_it_is_a_fragment()
	{
		var pages = Pages().ToList();

		Assert.NotEmpty(pages);

		foreach (var page in pages)
		{
			foreach (var fence in Fences(page))
				Assert.True(
					fence.Words is not (["csharp", ..] or ["dotgram", ..]) ||
					fence.Words.Any(static word => word.StartsWith("example=", StringComparison.Ordinal) || word == "fragment"),
					$"{page}, line {fence.Line}: a ```{string.Join(' ', fence.Words)} fence names no example and is not marked fragment.");

			if (!page.EndsWith("README.md", StringComparison.Ordinal))
				Assert.True(All(page).Count > 0, $"{page} holds no example.");
		}
	}

	static IEnumerable<string> Pages()
	{
		var directory = Path.Combine(SourceRoot.Directory, "docs", "guide", "generator");

		return Directory.Exists(directory)
			? Directory
				.EnumerateFiles(directory, "*.md")
				.Select(static path => Path.GetRelativePath(SourceRoot.Directory, path).Replace('\\', '/'))
				.OrderBy(static path => path, StringComparer.Ordinal)
			: [];
	}

	sealed record Fence(int Line, string[] Words, string Text);

	sealed record Example(
		int Line,
		string[] Words,
		List<string> Sources,
		List<(string Path, string Text)> Files,
		List<(string Expression, string Answer)> Runs);

	static List<Fence> Fences(string page)
	{
		var text = File.ReadAllText(Path.Combine(SourceRoot.Directory, page)).Replace("\r\n", "\n", StringComparison.Ordinal);

		return Regex
			.Matches(text, "^```([^\n]*)\n(.*?)^```[ \t]*$", RegexOptions.Singleline | RegexOptions.Multiline)
			.Where(one => !InRussian(text, one.Index))
			.Select(one => new Fence(
				text.Take(one.Index).Count(static c => c == '\n') + 1,
				one.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries),
				one.Groups[2].Value))
			.ToList();
	}

	static Dictionary<string, Example> All(string page)
	{
		var examples = new Dictionary<string, Example>(StringComparer.Ordinal);

		foreach (var fence in Fences(page))
		{
			var name = fence.Words.FirstOrDefault(static word => word.StartsWith("example=", StringComparison.Ordinal))?["example=".Length..];

			if (name is null)
				continue;

			if (!examples.TryGetValue(name, out var example))
				examples[name] = example = new Example(fence.Line, [], [], [], []);

			switch (fence.Words)
			{
				case ["csharp", ..]:
					if (example.Sources.Count == 0)
						examples[name] = example = example with { Words = fence.Words[1..] };

					example.Sources.Add(fence.Text);
					break;

				case ["dotgram", ..]:
					var path = fence.Words.FirstOrDefault(static word => word.StartsWith("file=", StringComparison.Ordinal))?["file=".Length..];

					Assert.True(path is not null, $"{page}, line {fence.Line}: a grammar of an example says file=path.");

					example.Files.Add(("/project/" + path, fence.Text));
					break;

				case ["text", "run", ..]:
					foreach (var line in fence.Text.Split('\n').Where(static line => line.Trim().Length > 0))
					{
						var arrow = line.LastIndexOf('→');

						Assert.True(arrow > 0, $"{page}, line {fence.Line}: not a run: '{line}'. Write expression → value.");

						example.Runs.Add((line[..arrow].Trim(), line[(arrow + 1)..].Trim()));
					}

					break;
			}
		}

		return examples;
	}

	/// <summary>Whether an offset of a page stands inside a Russian block.</summary>
	static bool InRussian(string text, int at)
	{
		var opens  = text.LastIndexOf("<!-- ru:begin -->", at, StringComparison.Ordinal);
		var closes = text.LastIndexOf("<!-- ru:end -->", at, StringComparison.Ordinal);

		return opens >= 0 && opens > closes;
	}

	/// <summary>The runs of an example as methods of one class, which sees every namespace the example declares.</summary>
	static SyntaxTree Runner(Example example, List<SyntaxTree> trees, CSharpParseOptions parse)
	{
		var text = new StringBuilder();

		foreach (var used in trees
			.SelectMany(static tree => tree.GetRoot().DescendantNodes().OfType<BaseNamespaceDeclarationSyntax>())
			.Select(static one => one.Name.ToString())
			.Append("System")
			.Append("System.Linq")
			.Distinct(StringComparer.Ordinal))
			text.Append("using ").Append(used).Append(";\n");

		text.Append("namespace DotGram.Tests.GuideRun\n{\n\tpublic static class Runs_DotGram\n\t{\n");

		for (var at = 0; at < example.Runs.Count; at++)
			text.Append("\t\tpublic static object? R").Append(at.ToString(CultureInfo.InvariantCulture))
				.Append("()\n\t\t{\n\t\t\treturn ").Append(example.Runs[at].Expression).Append(";\n\t\t}\n");

		text.Append("\t}\n}\n");

		return CSharpSyntaxTree.ParseText(text.ToString(), parse.WithLanguageVersion(LanguageVersion.Latest), "Runs.cs");
	}

	static ImmutableArray<MetadataReference> References()
	{
		return [
			.. AppDomain.CurrentDomain
				.GetAssemblies()
				.Where(static assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
				.Select(static assembly => (MetadataReference)MetadataReference.CreateFromFile(assembly.Location)),
		];
	}

	static string Show(object? value)
	{
		return value switch
		{
			null         => "null",
			string text  => Literal(text),
			char one     => "'" + Literal(one.ToString())[1..^1] + "'",
			bool yes     => yes ? "true" : "false",
			IEnumerable sequence
			             => "[" + string.Join(", ", sequence.Cast<object?>().Select(Show)) + "]",
			IFormattable formattable
			             => formattable.ToString(null, CultureInfo.InvariantCulture),
			_            => value.ToString() ?? "",
		};
	}

	static string Literal(string text)
	{
		return "\"" + text
			.Replace("\\", "\\\\", StringComparison.Ordinal)
			.Replace("\"", "\\\"", StringComparison.Ordinal)
			.Replace("\n", "\\n", StringComparison.Ordinal)
			.Replace("\r", "\\r", StringComparison.Ordinal)
			.Replace("\t", "\\t", StringComparison.Ordinal) + "\"";
	}

	/// <summary>The build properties an example sets, as the compiler hands them to a generator.</summary>
	sealed class BuildProperties(Dictionary<string, string> properties) : AnalyzerConfigOptionsProvider
	{
		public override AnalyzerConfigOptions GlobalOptions { get; } = new Options(properties);

		public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
		{
			return Options.Empty;
		}

		public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
		{
			return Options.Empty;
		}

		sealed class Options(Dictionary<string, string> values) : AnalyzerConfigOptions
		{
			public static Options Empty { get; } = new([]);

			public override bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
			{
				return values.TryGetValue(key, out value);
			}
		}
	}

	/// <summary>A <c>.gram</c> file of an example, which never touched a disk.</summary>
	sealed class GuideFile(string path, string text) : AdditionalText
	{
		public override string Path { get; } = path;

		public override SourceText GetText(CancellationToken cancellationToken = default)
		{
			return SourceText.From(text);
		}
	}
}
