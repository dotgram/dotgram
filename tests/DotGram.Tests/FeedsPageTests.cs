using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The pages about feeds — the repository README's section on feeds written by people, and
/// <c>docs/feeds.md</c> — run as they are written.
/// </summary>
/// <remarks>
/// <para>
/// Each example there is four blocks: the file a person wrote, the grammar and the class it is
/// attached to, the loop that reads the file, and what the loop prints. The page's claim is the
/// last block — that this input, read by this code, says exactly this, a bad record in its place
/// among the good ones. So the grammar is run through the generator, the loop is compiled beside
/// what it made, and the loop is run over the input and its output compared with the page's.
/// </para>
/// <para>
/// The blocks are compiled as written but for two names: <c>File.OpenText</c> hands back the
/// page's input rather than opening a file, and <c>Console</c> writes into a buffer of the run's
/// own. Both are replaced in the text rather than by touching the process, so runs in parallel
/// cannot see each other's output. <c>Console.Error</c> goes to the same buffer as
/// <c>Console.Out</c>, as it does in a terminal — the order the two arrive in is part of what the
/// page shows.
/// </para>
/// <para>
/// The same grammars are in <c>examples/DotGram.Examples/Feeds</c>, for whoever copies an example
/// as a file; the last test holds the copies to one text. An example may be on both pages — the
/// README shows the first trading feed of <c>docs/feeds.md</c> — so an example is named by its
/// page and its class, and each page's copy is run and held to the file on its own.
/// </para>
/// </remarks>
public sealed class FeedsPageTests
{
	[Theory]
	[MemberData(nameof(Examples))]
	public void An_example_prints_what_the_page_says_it_prints(string page, string host)
	{
		var example  = Find(page, host);
		var blocks   = Blocks(page).Where(static one => one.Language == "csharp").Select(static one => one.Text).ToList();
		var declared = Substituted(ShippedPages.Inherited(blocks, example.At) + example.Declaration);
		var uses     = Substituted(ShippedPages.Inherited(blocks, example.At + 1) + example.Use);
		var grammar  = ShippedPages.GrammarIn(example.Declaration)!;

		var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = host,
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(
				CSharpCompilation.Create(
					host,
					[
						Parse(declared),
						Parse(GramCompiler.EmitMarkerAttributes().Text),
						Parse(Harness),
					],
					EmittedCode.References,
					new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)),
				host),
		});

		EmittedCode.Quiet(compiled.Diagnostics);

		var built = CSharpCompilation.Create(
			host + ".Run",
			[
				Parse(uses),
				Parse(declared),
				Parse(Harness),
				Parse(GramCompiler.EmitMarkerAttributes().Text),
				Parse(DotGram.Grammar.Emit.SupportEmitter.EmbeddedAttribute),
				.. compiled.Sources.Select(static one => Parse(one.Text)),
			],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.ConsoleApplication));

		using var stream = new MemoryStream();

		var emitted = built.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(
			emitted.Success,
			$"{host}:\n" + string.Join("\n", emitted.Diagnostics
				.Where(static one => one.Severity == DiagnosticSeverity.Error)
				.Select(static one => one.ToString())));

		var assembly = Assembly.Load(stream.ToArray());
		var captured = assembly.GetType("Captured")!;

		captured.GetField("Input")!.SetValue(null, example.Input);

		// The page's numbers are written the invariant way, and a run on a machine set to another
		// culture would print a decimal comma. The culture is the thread's, so this run alone sees it.
		var culture = CultureInfo.CurrentCulture;

		CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

		try
		{
			var main = assembly.EntryPoint!;

			main.Invoke(null, main.GetParameters().Length == 0 ? null : [Array.Empty<string>()]);
		}
		finally
		{
			CultureInfo.CurrentCulture = culture;
		}

		var printed = captured.GetField("Out")!.GetValue(null)!.ToString()!;

		Assert.Equal(Lines(example.Output), Lines(printed));
	}

	[Theory]
	[MemberData(nameof(Examples))]
	public void And_the_examples_project_holds_the_same_grammar(string page, string host)
	{
		var example = Find(page, host);
		var folder  = Path.Combine(Root, "examples", "DotGram.Examples", "Feeds");

		var file = Directory
			.EnumerateFiles(folder, "*.cs")
			.Select(static path => File.ReadAllText(path))
			.SingleOrDefault(text => ShippedPages.HostIn(text) == host);

		Assert.True(file is not null, $"No example under {folder} declares 'partial class {host}'.");

		Assert.Equal(
			Lines(ShippedPages.GrammarIn(example.Declaration)!),
			Lines(ShippedPages.GrammarIn(file)!));
	}

	/// <summary>Every example of the pages, by its page and the class its grammar is attached to.</summary>
	public static TheoryData<string, string> Examples
	{
		get
		{
			var data = new TheoryData<string, string>();

			foreach (var page in Pages)
				foreach (var one in All(page))
					data.Add(page, one.Host);

			return data;
		}
	}

	/// <summary>The pages, by their path from the repository's root.</summary>
	static readonly string[] Pages = ["README.md", "docs/feeds.md"];

	/// <summary>One example: what was read, what reads it, and what that prints.</summary>
	sealed record Example(string Host, int At, string Input, string Declaration, string Use, string Output);

	static Example Find(string page, string host)
	{
		return All(page).Single(one => one.Host == host);
	}

	/// <summary>
	/// The examples in the order the page gives them: the text block before a grammar is its input,
	/// the C# block after it the loop, and the text block after that the output.
	/// </summary>
	static IEnumerable<Example> All(string page)
	{
		var blocks = Blocks(page);
		var csharp = -1;

		for (var at = 0; at < blocks.Count; at++)
		{
			if (blocks[at].Language != "csharp")
				continue;

			csharp++;

			var host = ShippedPages.HostIn(blocks[at].Text);

			if (host is null || ShippedPages.GrammarIn(blocks[at].Text) is null)
				continue;

			Assert.True(at >= 1 && blocks[at - 1].Language == "text", $"{host}: no input block above the grammar.");
			Assert.True(at + 2 < blocks.Count, $"{host}: no loop and output below the grammar.");
			Assert.True(blocks[at + 1].Language == "csharp", $"{host}: the block after the grammar is not the loop.");
			Assert.True(blocks[at + 2].Language == "text", $"{host}: the block after the loop is not its output.");

			yield return new Example(host, csharp, blocks[at - 1].Text, blocks[at].Text, blocks[at + 1].Text, blocks[at + 2].Text);
		}
	}

	/// <summary>
	/// The fenced blocks of a page, with the language each is marked as: of the README, the section
	/// on feeds alone; of a page of its own, all of it.
	/// </summary>
	static List<(string Language, string Text)> Blocks(string path)
	{
		var page    = File.ReadAllText(Path.Combine(Root, path)).Replace("\r\n", "\n", StringComparison.Ordinal);
		var section = page;

		if (path == "README.md")
		{
			var opens = page.IndexOf("\n" + Heading + "\n", StringComparison.Ordinal);

			Assert.True(opens >= 0, $"README.md has no '{Heading}'.");

			var closes = page.IndexOf("\n## ", opens + Heading.Length, StringComparison.Ordinal);

			section = closes < 0 ? page[opens..] : page[opens..closes];
		}

		return Regex
			.Matches(section, "```([a-z]*)\n(.*?)```", RegexOptions.Singleline)
			.Select(static one => (one.Groups[1].Value, one.Groups[2].Value))
			.ToList();
	}

	const string Heading = "## Feeds written by people";

	/// <summary>The block with the two names the run stands in for replaced.</summary>
	static string Substituted(string code)
	{
		return code
			.Replace("File.OpenText(", "Sample.OpenText(", StringComparison.Ordinal)
			.Replace("Console.", "Captured.", StringComparison.Ordinal);
	}

	/// <summary>What the two names stand for in a run: the page's input, and a buffer of its own.</summary>
	const string Harness = """
		#nullable enable

		static class Captured
		{
			public static string Input = "";

			public static readonly System.IO.StringWriter Out = new();

			public static System.IO.TextWriter Error
			{
				get { return Out; }
			}

			public static void WriteLine(string? value)
			{
				Out.WriteLine(value);
			}

			public static void WriteLine(object? value)
			{
				Out.WriteLine(value);
			}
		}

		static class Sample
		{
			public static System.IO.TextReader OpenText(string path)
			{
				return new System.IO.StringReader(Captured.Input);
			}
		}
		""";

	static SyntaxTree Parse(string code)
	{
		return CSharpSyntaxTree.ParseText(
			code,
			new CSharpParseOptions(LanguageVersion.Preview),
			cancellationToken: TestContext.Current.CancellationToken);
	}

	static string[] Lines(string text)
	{
		return text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n').Split('\n');
	}

	static string Root { get; } = Repository();

	static string Repository()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? throw new InvalidOperationException("The repository root is not above the test binaries.");
	}
}
