using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The SQL grammar, asked of the reader whole.
/// </summary>
/// <remarks>
/// <para>
/// Every other case in <c>ReaderTests</c> is a grammar of a few lines aimed at one
/// construct. This is four hundred lines of somebody else's language — a ladder of
/// precedence levels, lists gathered across the turns of a repetition, folds, and
/// forty-two shapes of predicate — and what it asks is only whether the reader can write
/// it at all.
/// </para>
/// <para>
/// Not whether the two renderings build the same tree out of it, which is the question
/// worth asking and needs a harness this is not: the generator resolves the types a
/// construction names against a real Roslyn compilation, and a grammar compiled on its own
/// here has no symbol resolver to do it with. That comparison wants the reader turned on
/// for a real build, and is what comes next.
/// </para>
/// <para>
/// The grammar is read out of the file it lives in rather than copied, so this cannot go
/// stale against the parser it is about.
/// </para>
/// </remarks>
public sealed class SqlReaderTests
{
	[Fact]
	public void The_reader_can_write_the_whole_of_it()
	{
		var result = GramCompiler.Compile(
			Grammar,
			new GramCompilerOptions
			{
				ClassName     = "SqlStandard92",
				CSharpScanner = RoslynCSharpScanner.Instance,
				Lexical       = true,
				Reader        = true,
			});

		Assert.Empty(result.Diagnostics.Where(one => one.Severity == GramSeverity.Error));

		var declined = result.Diagnostics.FirstOrDefault(one => one.Id == "GRAM5006");

		Assert.True(declined is null, declined?.Message);
	}

	/// <summary>The grammar as the parser carries it, read out of the file it lives in.</summary>
	static string Grammar
	{
		get
		{
			var kept = new List<string>();
			var open = false;

			foreach (var line in Lines(Read("SqlStandard92.cs")))
			{
				if (!open)
				{
					if (line.StartsWith("[Gram(", StringComparison.Ordinal))
						open = true;

					continue;
				}

				if (line.TrimStart().StartsWith("\"\"\"", StringComparison.Ordinal))
					break;

				kept.Add(line.StartsWith("\t", StringComparison.Ordinal) ? line.Substring(1) : line);
			}

			Assert.NotEmpty(kept);

			return string.Join(Break, kept);
		}
	}

	static string[] Lines(string source) => source.Replace("\r\n", Break).Split('\n');

	const string Break = "\n";

	static string Read(string file) =>
		System.IO.File.ReadAllText(
			System.IO.Path.Combine(Root(AppContext.BaseDirectory), "src", "DotGram.Parsers", file));

	static string Root(string from)
	{
		var at = new System.IO.DirectoryInfo(from);

		while (at is not null && !System.IO.File.Exists(System.IO.Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		Assert.NotNull(at);

		return at!.FullName;
	}
}
