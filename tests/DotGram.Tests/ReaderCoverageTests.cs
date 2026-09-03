using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every grammar in the repository is read by the reader.
/// </summary>
/// <remarks>
/// <para>
/// The reader is the rendering, and the one it replaced stays only for what the reader
/// declines. This says that nothing shipped or shown declines: every parser and every
/// example, read out of the files they live in with <c>Reader = true</c>, which is the
/// setting that is told where it declined (<c>GRAM5006</c>).
/// </para>
/// <para>
/// A decline is not a failure of the build — the older rendering writes what the reader
/// will not — so nothing else would say. This does, and the day it goes red is the day
/// somebody wrote a grammar the reader cannot yet write.
/// </para>
/// <para>
/// What it cannot ask about is a grammar that needs the symbol resolver to compile at
/// all: a rule whose value is built by the constructor of the type it declares, with no
/// <c>=&gt;</c>, is resolved against the Roslyn compilation the generator runs in, and
/// here there is none. Such a grammar reports an error before the reader is asked, and
/// is counted rather than judged. Today that is <c>TypedCsvExample</c>.
/// </para>
/// </remarks>
public sealed class ReaderCoverageTests
{
	[Fact]
	public void Every_parser_and_example_is_read_by_the_reader()
	{
		var root  = Root(AppContext.BaseDirectory);
		var files = Directory.GetFiles(Path.Combine(root, "examples"), "*.cs", SearchOption.AllDirectories)
			.Concat(Directory.GetFiles(Path.Combine(root, "src", "DotGram.Parsers"), "*.cs"))
			.Where(one => !one.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
			.OrderBy(one => one, StringComparer.Ordinal)
			.ToList();

		var declined   = new List<string>();
		var seen       = 0;
		var unresolved = 0;

		foreach (var file in files)
			foreach (var (grammar, lexical) in Grammars(File.ReadAllText(file)))
			{
				seen++;

				var result = GramCompiler.Compile(
					grammar,
					new GramCompilerOptions
					{
						ClassName     = "Probe",
						CSharpScanner = RoslynCSharpScanner.Instance,
						Lexical       = lexical,
						Reader        = true,
					});

				if (result.Diagnostics.Any(one => one.Severity == GramSeverity.Error))
				{
					unresolved++;

					continue;
				}

				foreach (var one in result.Diagnostics)
					if (one.Id == "GRAM5006")
						declined.Add($"{Path.GetFileName(file)}: {one.Message}");
			}

		Assert.True(seen >= 20, $"Only {seen} grammars were found under examples/ and src/DotGram.Parsers/.");
		Assert.True(unresolved <= 1, $"{unresolved} grammars could not be compiled without a symbol resolver.");
		Assert.Empty(declined);
	}

	/// <summary>The grammars a file carries, as the raw string literals they are written in.</summary>
	/// <remarks>
	/// A raw string literal takes the indentation of its closing quotes off every line, and
	/// so does this — which is what makes a grammar written across lines under <c>[Gram(</c>
	/// the same grammar the generator reads.
	/// </remarks>
	static IEnumerable<(string Grammar, bool Lexical)> Grammars(string source)
	{
		var text = source.Replace("\r\n", "\n").Split('\n');

		for (var i = 0; i < text.Length; i++)
		{
			if (!text[i].TrimStart().StartsWith("[Gram(\"\"\"", StringComparison.Ordinal))
				continue;

			var j = i + 1;

			while (j < text.Length && !text[j].TrimStart().StartsWith("\"\"\"", StringComparison.Ordinal))
				j++;

			if (j >= text.Length)
				break;

			var closing = text[j];
			var indent  = closing.Length - closing.TrimStart().Length;
			var kept    = new List<string>();

			for (var k = i + 1; k < j; k++)
				kept.Add(text[k].Length >= indent ? text[k].Substring(indent) : text[k].TrimStart());

			yield return (string.Join("\n", kept), closing.Contains("Lexical = true", StringComparison.Ordinal));
		}
	}

	static string Root(string from)
	{
		var at = new DirectoryInfo(from);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		Assert.NotNull(at);

		return at!.FullName;
	}
}
