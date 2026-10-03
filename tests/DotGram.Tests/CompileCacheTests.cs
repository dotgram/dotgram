using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The parsers the generator keeps from one compilation to the next in the same process: what a
/// command-line build in a long-lived compiler server takes instead of compiling a grammar again.
/// </summary>
/// <remarks>
/// <para>
/// Each driver below is a fresh one, so nothing the incremental pipeline keeps can answer for the
/// cache: a parser that is not compiled again was found in it. What a test reads is the generation
/// report's line, whose last item is the time the compile took, or <c>cached</c> where it took none.
/// </para>
/// <para>
/// The cache is the process's, and the tests of this assembly run side by side. So each test here
/// compiles a grammar of its own — a name no other test spells — and none can find another's parser,
/// or lose its own to another's.
/// </para>
/// </remarks>
public sealed class CompileCacheTests
{
	[Fact]
	public void The_same_input_compiled_again_is_taken_from_the_cache()
	{
		var source = Host(Unique());
		var first  = Run(source);
		var second = Run(source);

		Assert.Matches(@"[0-9]+\.[0-9]{2} ms generation$", Report(first));
		Assert.EndsWith(", cached", Report(second), StringComparison.Ordinal);
		Assert.Equal(Parser(first), Parser(second));

		// The rest of the line is the same: it is the same parser.
		Assert.Equal(
			Report(first).Substring(0, Report(first).LastIndexOf(", ", StringComparison.Ordinal)),
			Report(second).Substring(0, Report(second).LastIndexOf(", ", StringComparison.Ordinal)));
	}

	[Fact]
	public void A_changed_grammar_is_compiled_again()
	{
		var name = Unique();

		Run(Host(name));

		Assert.Matches(@"ms generation$", Report(Run(Host(name).Replace("['a'..'z']+", "['a'..'y']+", StringComparison.Ordinal))));
	}

	/// <summary>
	/// The answers the compilation gave about the host's C# are what the parser is built against,
	/// and an edit to C# alone can change them: here the order of a constructor's parameters, which
	/// decides which capture goes where.
	/// </summary>
	[Fact]
	public void A_changed_answer_about_the_hosts_csharp_is_compiled_again()
	{
		var name    = Unique();
		var grammar = $$"""
			public sealed class {{name}}Pair(string left, string right)
			{
				public string Left  { get; } = left;
				public string Right { get; } = right;
			}

			[DotGram.Gram("Pair : @{{name}}Pair = Left: ['a'..'z']+ & '-' & Right: ['a'..'z']+\nparse Pair")]
			public partial class {{name}};
			""";

		var first   = Run(grammar);
		var same    = Run(grammar + "\nclass Unrelated" + name + " { }");
		var swapped = Run(grammar.Replace("(string left, string right)", "(string right, string left)", StringComparison.Ordinal));

		Assert.EndsWith(", cached", Report(same), StringComparison.Ordinal);
		Assert.Matches(@"ms generation$", Report(swapped));
		Assert.NotEqual(Parser(first), Parser(swapped));
	}

	[Theory]
	[InlineData("build_property.DotGramReportGeneration", "full")]
	[InlineData("build_property.DotGramPositionalFollow", "true")]
	public void A_changed_build_option_is_compiled_again(string property, string value)
	{
		var source = Host(Unique());

		Run(source);

		Assert.Matches(@"ms generation$", Report(Run(source, (property, value))));
	}

	[Fact]
	public void A_changed_preprocessor_symbol_the_compile_reads_is_compiled_again()
	{
		var source = Host(Unique());

		Run(source);

		Assert.Matches(@"ms generation$", Report(Run(source, ["DOTGRAM_COUNTS"], [])));
	}

	[Fact]
	public void No_cache_compiles_every_time()
	{
		var source = Host(Unique());

		Run(source);

		Assert.Matches(@"ms generation$", Report(Run(source, ("build_property.DotGramNoCache", "true"))));
		Assert.Matches(@"ms generation$", Report(Run(source, ("build_property.DotGramNoCache", "TRUE"))));
	}

	/// <summary>
	/// The repository's own check: a parser taken from the cache is compiled again and held to the
	/// fresh one. Equal, the line says so and nothing else is reported.
	/// </summary>
	[Fact]
	public void Verification_compiles_again_and_finds_the_kept_parser_the_same()
	{
		var source = Host(Unique());
		var first  = Run(source);
		var second = Run(source, ("build_property.DotGramVerifyCache", "true"));

		Assert.EndsWith(", cached and verified", Report(second), StringComparison.Ordinal);
		Assert.Equal(Parser(first), Parser(second));
		Assert.DoesNotContain(second.Diagnostics, diagnostic => diagnostic.Id == "GRAM0001");
	}

	/// <summary>
	/// And where they differ, the build fails: the kept parser is altered behind the cache's back,
	/// which is what a key that missed one of the compile's inputs would look like from outside.
	/// </summary>
	[Fact]
	public void Verification_fails_the_build_where_the_kept_parser_differs()
	{
		var name   = Unique();
		var source = Host(name);
		var first  = Run(source);

		Tamper(name);

		var second = Run(source, ("build_property.DotGramVerifyCache", "true"));

		Assert.Contains(
			second.Diagnostics,
			diagnostic => diagnostic.Id == "GRAM0001" &&
				diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("differs from a fresh compile of the same input: the text", StringComparison.Ordinal));

		// What the build is handed is the fresh parser, not the altered one.
		Assert.Equal(Parser(first), Parser(second));
	}

	/// <summary>
	/// A parser taken from the cache reports what its compile reported: diagnostics are part of what
	/// is kept, and delivered again on every hit.
	/// </summary>
	[Fact]
	public void A_parser_taken_from_the_cache_reports_its_diagnostics_again()
	{
		var source = Host(Unique()).Replace(")]", ", Lexical = true)]", StringComparison.Ordinal);
		var first  = Run(source);
		var second = Run(source);

		Assert.EndsWith(", cached", Report(second), StringComparison.Ordinal);
		Assert.Contains(first.Diagnostics, static diagnostic => diagnostic.Id == "GRAM5004");
		Assert.Equal(
			first.Diagnostics.Select(static diagnostic => diagnostic.ToString()),
			second.Diagnostics.Select(static diagnostic => diagnostic.ToString()));
	}

	/// <summary>
	/// A grammar with errors of its own reports them the same the second time. Whether that second
	/// compile was a hit cannot be read here: such a grammar writes no report line.
	/// </summary>
	[Fact]
	public void A_grammar_with_errors_reports_the_same_errors_the_second_time()
	{
		var source = Host(Unique()).Replace("parse Start", "Other = Missing\\nparse Start", StringComparison.Ordinal);
		var first  = Run(source);
		var second = Run(source);

		Assert.Contains(first.Diagnostics,  diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
		Assert.Equal(
			first.Diagnostics.Select(static diagnostic => diagnostic.ToString()),
			second.Diagnostics.Select(static diagnostic => diagnostic.ToString()));
	}

	/// <summary>
	/// The editor's builds keep nothing: a grammar compiled only by them is compiled afresh by the
	/// next build that is not one. Read through that build, since a design-time one writes no report.
	/// </summary>
	[Fact]
	public void A_design_time_build_keeps_nothing()
	{
		var source = Host(Unique());

		Run(source, ("build_property.DesignTimeBuild", "true"));
		Run(source, ("build_property.DesignTimeBuild", "true"));

		Assert.Matches(@"ms generation$", Report(Run(source)));
	}

	// ── The cache itself ─────────────────────────────────────────────────────────

	[Fact]
	public void The_budget_lets_the_least_recently_used_go()
	{
		var cache = new CompileCache<string, string>(10, 100, static (key, value) => [value]);

		Assert.True(cache.Add("a", new string('a', 4)));
		Assert.True(cache.Add("b", new string('b', 4)));
		Assert.True(cache.TryGet("a", out _));

		// Twelve characters would be held: the one read least recently goes, which is "b".
		Assert.True(cache.Add("c", new string('c', 4)));

		Assert.True(cache.TryGet("a", out _));
		Assert.False(cache.TryGet("b", out _));
		Assert.True(cache.TryGet("c", out _));
		Assert.Equal(8, cache.Size);
	}

	[Fact]
	public void A_value_larger_than_the_budget_is_not_kept_and_lets_nothing_go()
	{
		var cache = new CompileCache<string, string>(10, 100, static (key, value) => [value]);

		cache.Add("a", new string('a', 4));

		Assert.False(cache.Add("huge", new string('h', 11)));
		Assert.True(cache.TryGet("a", out _));
		Assert.False(cache.TryGet("huge", out _));
	}

	[Fact]
	public void A_string_two_entries_hold_is_counted_once()
	{
		var shared = new string('s', 6);
		var cache  = new CompileCache<string, string>(10, 100, static (key, value) => [value]);

		cache.Add("net10.0",        shared);
		cache.Add("netstandard2.0", shared);

		Assert.Equal(2, cache.Count);
		Assert.Equal(6, cache.Size);
	}

	/// <summary>
	/// The sharing is the cache's to do, under its lock: two equal texts finished at the same moment
	/// — both target frameworks of a project compiled side by side — end up one string.
	/// </summary>
	[Fact]
	public void An_equal_string_is_shared_with_the_one_kept_and_counted_once()
	{
		var cache = new CompileCache<string, string>(
			10,
			100,
			static (key, value) => [value],
			share: static (value, kept) => kept.FirstOrDefault(one => string.Equals(one, value, StringComparison.Ordinal)) ?? value);

		cache.Add("net10.0",        new string('s', 6));
		cache.Add("netstandard2.0", new string('s', 6));

		Assert.True(cache.TryGet("net10.0", out var first));
		Assert.True(cache.TryGet("netstandard2.0", out var second));
		Assert.Same(first, second);
		Assert.Equal(6, cache.Size);
	}

	/// <summary>
	/// What an entry holds besides its large strings is counted too: entries that each hold almost no
	/// text but thousands of answers cannot pile up under a budget of characters.
	/// </summary>
	[Fact]
	public void The_estimate_of_what_else_an_entry_holds_counts_against_the_budget()
	{
		var cache = new CompileCache<string, string>(10, 100, static (key, value) => [value], static (key, value) => 4);

		cache.Add("a", "a");
		cache.Add("b", "b");

		// Five each: the third would make fifteen, and the oldest goes.
		cache.Add("c", "c");

		Assert.False(cache.TryGet("a", out _));
		Assert.Equal(2, cache.Count);
		Assert.Equal(10, cache.Size);
	}

	[Fact]
	public void No_more_entries_than_the_capacity_are_kept_however_little_they_hold()
	{
		var cache = new CompileCache<string, string>(1_000, 2, static (key, value) => [value]);

		cache.Add("a", "a");
		cache.Add("b", "b");
		cache.TryGet("a", out _);
		cache.Add("c", "c");

		Assert.Equal(2, cache.Count);
		Assert.True(cache.TryGet("a", out _));
		Assert.False(cache.TryGet("b", out _));
		Assert.True(cache.TryGet("c", out _));
	}

	/// <summary>
	/// Letting an entry go frees only the strings no other entry still holds: one shared with an
	/// entry that stays is still counted, and the eviction goes on until the rest fits.
	/// </summary>
	[Fact]
	public void Eviction_counts_a_shared_string_until_its_last_holder_goes()
	{
		var shared = new string('s', 6);
		var cache  = new CompileCache<string, string>(12, 100, static (key, value) => [value]);

		cache.Add("a", shared);
		cache.Add("b", shared);
		cache.Add("c", new string('c', 4));

		// Six and four: ten. Seven more makes seventeen; "a" going frees nothing, "b" frees six.
		cache.Add("d", new string('d', 7));

		Assert.False(cache.TryGet("a", out _));
		Assert.False(cache.TryGet("b", out _));
		Assert.True(cache.TryGet("c", out _));
		Assert.Equal(11, cache.Size);
	}

	[Fact]
	public void The_first_of_two_concurrent_adds_is_the_one_kept()
	{
		var cache = new CompileCache<string, string>(10, 100, static (key, value) => [value]);
		var first = new string('x', 2);

		Assert.True(cache.Add("a", first));
		Assert.False(cache.Add("a", new string('x', 2)));
		Assert.True(cache.TryGet("a", out var kept));
		Assert.Same(first, kept);
	}

	// ── Helpers ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// Alters the text of the parser the generator keeps for the host named <paramref name="name"/>,
	/// through reflection: the cache gives nothing outside the generator a way to do it.
	/// </summary>
	static void Tamper(string name)
	{
		var cache   = typeof(GramGenerator).GetField("Compiled", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
		var entries = (IEnumerable)cache.GetType().GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(cache)!;

		foreach (var pair in entries)
		{
			var entry  = pair.GetType().GetProperty("Value")!.GetValue(pair)!;
			var field  = entry.GetType().GetField("Value")!;
			var parser = field.GetValue(entry)!;
			var text   = (string?)parser.GetType().GetProperty("Text")!.GetValue(parser);

			if (text is null || !text.Contains(name, StringComparison.Ordinal))
				continue;

			parser.GetType()
				.GetField("<Text>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)!
				.SetValue(parser, text + "// altered\n");
			field.SetValue(entry, parser);

			return;
		}

		Assert.Fail($"No kept parser for {name}.");
	}

	/// <summary>A name for a host no other test in the process compiles.</summary>
	static string Unique()
	{
		return "Cached" + Guid.NewGuid().ToString("N");
	}

	static string Host(string name)
	{
		return $"[DotGram.Gram(\"Start = ['a'..'z']+\\nparse Start\")] public partial class {name};";
	}

	static GeneratorDriverRunResult Run(string source, params (string Key, string Value)[] options)
	{
		return Run(source, [], options);
	}

	static GeneratorDriverRunResult Run(string source, string[] symbols, params (string Key, string Value)[] options)
	{
		var parseOptions = CSharpParseOptions.Default
			.WithLanguageVersion(LanguageVersion.Preview)
			.WithPreprocessorSymbols(symbols);
		var references = AppDomain.CurrentDomain.GetAssemblies()
			.Where(static assembly => !assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
			.Select(static assembly => MetadataReference.CreateFromFile(assembly.Location));
		var compilation = CSharpCompilation.Create(
			"CompileCache",
			[CSharpSyntaxTree.ParseText(source, parseOptions, "Host.cs")],
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		var properties = new Dictionary<string, string>(StringComparer.Ordinal)
		{
			["build_property.DotGramReportGeneration"] = "summary",
		};

		foreach (var (key, value) in options)
			properties[key] = value;

		return CSharpGeneratorDriver.Create(
				[new GramGenerator().AsSourceGenerator()],
				parseOptions: parseOptions,
				optionsProvider: new OptionsProvider(properties))
			.RunGenerators(compilation, TestContext.Current.CancellationToken)
			.GetRunResult();
	}

	/// <summary>The report's one line, without the comment it is written in.</summary>
	static string Report(GeneratorDriverRunResult run)
	{
		return run.GeneratedTrees
			.Single(static tree => tree.FilePath.EndsWith(".DotGramReport.g.cs", StringComparison.Ordinal))
			.ToString()
			.TrimStart('/', ' ')
			.TrimEnd();
	}

	/// <summary>The parser's own file: the one named after its host, which every name here begins with.</summary>
	static string Parser(GeneratorDriverRunResult run)
	{
		return run.Results
			.SelectMany(static result => result.GeneratedSources)
			.Single(static source => source.HintName.StartsWith("Cached", StringComparison.Ordinal) &&
				source.HintName.EndsWith(".g.cs", StringComparison.Ordinal) &&
				!source.HintName.Contains(".DotGramReport", StringComparison.Ordinal))
			.SourceText
			.ToString();
	}

	sealed class OptionsProvider(Dictionary<string, string> properties) : AnalyzerConfigOptionsProvider
	{
		public override AnalyzerConfigOptions GlobalOptions { get; } = new Options(properties);

		public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
		{
			return GlobalOptions;
		}

		public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
		{
			return GlobalOptions;
		}
	}

	sealed class Options(Dictionary<string, string> properties) : AnalyzerConfigOptions
	{
		public override bool TryGetValue(string key, out string value)
		{
			return properties.TryGetValue(key, out value!);
		}
	}
}
