using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using DotGram.Generation;
using DotGram.Sql.Standard;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The shipped SQL grammars run through the generator here, under the build's switches a test wants
/// to compare: with and without the memo of failures, under each value of the positional follow,
/// and with the entry counters or without.
/// </summary>
/// <remarks>
/// Each variant is the grammar file as it ships, behind the shipped host's attributes, in a
/// namespace of its own and an assembly named as the one the package shows its internals to, since
/// the code beside the grammars uses them. Compiled once a process and kept.
/// </remarks>
static class SqlVariants
{
	/// <summary>The T-SQL parser, less the located reading.</summary>
	public const string TransactSql = "TransactSql";

	/// <summary>The SQL:2023 parser.</summary>
	public const string Standard = "Standard";

	/// <summary>The SQL-92 parser T-SQL includes.</summary>
	public const string Sql92 = "Sql92";

	/// <summary>The SQL-92 grammar as it was before a bracket was read once (a fixture under <c>Fixtures/ReadOnce</c>), for the differential.</summary>
	public const string Sql92Before = "Sql92Before";

	/// <summary>
	/// The name a variant publishes an entry under. T-SQL's grammar publishes its entries to its own assembly as
	/// <c>TryParseSyntax…</c>, building the T-SQL tree, and the shipped class carries them onto the common tree under
	/// the public names; a variant is the grammar alone, so it has the first.
	/// </summary>
	public static string Entry(string grammar, string publication)
	{
		if (grammar != TransactSql || !publication.StartsWith("TryParse", StringComparison.Ordinal) || publication.StartsWith("TryParseSyntax", StringComparison.Ordinal))
			return publication;

		var level = publication.Substring("TryParse".Length);

		return "TryParseSyntax" + (level == "SearchCondition" ? "Condition" : level);
	}

	/// <summary>A parser of one grammar compiled one way.</summary>
	/// <param name="follow"><c>off</c>, <c>true</c> or <c>split</c>, as <c>DotGramPositionalFollow</c> says it.</param>
	/// <param name="memo">Whether the memo of failures is written (<c>DOTGRAM_NO_MEMO</c> where not).</param>
	/// <param name="counts">Whether the entry counters are written (<c>DOTGRAM_COUNTS</c>).</param>
	public static Type Parser(string grammar, string follow = "off", bool memo = true, bool counts = false)
	{
		return Parser(grammar, follow, memo, counts, headGuard: false, located: false);
	}

	/// <summary>A parser of one grammar compiled one way, with the head-guard experiment on or off and with or without a located reading beside it.</summary>
	/// <remarks>The head-guard experiment is a static switch of the emitter: compilations are serialised around it, so a caller asking for it should run alone.</remarks>
	public static Type Parser(string grammar, string follow, bool memo, bool counts, bool headGuard, bool located)
	{
		return Compiled.GetOrAdd((grammar, follow, memo, counts, headGuard, located), static key => Compile(key.Grammar, key.Follow, key.Memo, key.Counts, key.HeadGuard, key.Located));
	}

	/// <summary>The size of what a variant compiled to: the generated C# in characters and the assembly in bytes.</summary>
	public static readonly ConcurrentDictionary<Type, (long SourceChars, long AssemblyBytes)> Sizes = new();

	/// <summary>The SQL-92 grammar with the value tower instantiated twice (a fixture under <c>Fixtures/ReadOnce/Split</c>).</summary>
	public const string Sql92Split = "Sql92Split";

	static readonly ConcurrentDictionary<(string Grammar, string Follow, bool Memo, bool Counts, bool HeadGuard, bool Located), Type> Compiled = new();

	static readonly object Compiling = new();

	static Type Compile(string grammar, string follow, bool memo, bool counts, bool headGuard, bool located)
	{
		var (host, directory, file, name) = grammar switch
		{
			TransactSql => (TransactSqlHost, "TransactSql", "TransactSql.gram", "DotGram.Sql.TransactSql.Variant.TransactSqlParser"),
			Standard    => (StandardHost, "Standard", "SqlStandard.gram", "DotGram.Sql.Standard.Variant.SqlStandardParser"),
			Sql92       => (Sql92Host, "Standard", "SqlStandard92.gram", "DotGram.Sql.Standard.Variant.Sql92Parser"),
			Sql92Before => (Sql92Host.Replace("Standard.Variant", "Standard.Before"), Fixture("ReadOnce"), "SqlStandard92.gram", "DotGram.Sql.Standard.Before.Sql92Parser"),
			Sql92Split  => (Sql92Host.Replace("Standard.Variant", "Standard.Split"), Fixture(Path.Combine("ReadOnce", "Split")), "SqlStandard92.gram", "DotGram.Sql.Standard.Split.Sql92Parser"),
			_           => throw new ArgumentOutOfRangeException(nameof(grammar)),
		};

		if (headGuard)
		{
			host = host.Replace("Standard.Variant", "Standard.Guarded").Replace("Standard.Before", "Standard.BeforeGuarded").Replace("Standard.Split", "Standard.SplitGuarded");
			name = name.Replace("Standard.Variant", "Standard.Guarded").Replace("Standard.Before", "Standard.BeforeGuarded").Replace("Standard.Split", "Standard.SplitGuarded");
		}

		if (located)
			host = host.Replace("\tpublic abstract partial class", "\t[GramOptions(LocationType = typeof(global::DotGram.Sql.ISqlLocatable), Suffix = \"Located\")]\n\tpublic abstract partial class");

		lock (Compiling)
		{
			DotGram.Grammar.Emit.CSharpEmitter.HeadGuardFirstExperiment = headGuard;

			try
			{
				return Compile(host, directory, file, name, follow, memo, counts);
			}
			finally
			{
				DotGram.Grammar.Emit.CSharpEmitter.HeadGuardFirstExperiment = false;
			}
		}
	}

	static Type Compile(string host, string directory, string file, string name, string follow, bool memo, bool counts)
	{

		string[] symbols = [.. counts ? ["DOTGRAM_COUNTS"] : Array.Empty<string>(), .. memo ? Array.Empty<string>() : ["DOTGRAM_NO_MEMO"]];

		var parse = CSharpParseOptions.Default
			.WithLanguageVersion(LanguageVersion.Latest)
			.WithPreprocessorSymbols(symbols);
		var tree  = CSharpSyntaxTree.ParseText(host, parse, "Host.cs");
		var path  = Grammar(directory, file);

		// The package by name: what this process had loaded when the list was first taken need not
		// have included it yet.
		var package    = typeof(Sql92Parser).Assembly.Location;
		var references = EmittedCode.References
			.Where(reference => !string.Equals(reference.Display, package, StringComparison.OrdinalIgnoreCase))
			.Append(MetadataReference.CreateFromFile(package));

		var compilation = CSharpCompilation.Create(
			"DotGram.Benchmarks",
			[tree],
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		CSharpGeneratorDriver
			.Create(
				[new GramGenerator().AsSourceGenerator()],
				additionalTexts: [new InMemoryFile(path, File.ReadAllText(path))],
				parseOptions: parse,
				optionsProvider: new Options(follow))
			.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var reported);

		Assert.DoesNotContain(reported, one => one.Severity == DiagnosticSeverity.Error);

		// Or the switch did not reach the generator, and the two variants compared are one.
		Assert.Equal(memo, output.SyntaxTrees.Any(one => one.ToString().Contains("MemoFailed(pos", StringComparison.Ordinal)));

		if (counts)
			Assert.Contains(output.SyntaxTrees, one => one.ToString().Contains("CountEntered_", StringComparison.Ordinal));

		using var stream = new MemoryStream();

		var result = output.Emit(stream);

		Assert.True(
			result.Success,
			$"{name} ({follow}, memo {memo}) did not compile:\n" +
			string.Join("\n", result.Diagnostics.Where(one => one.Severity == DiagnosticSeverity.Error).Take(20)));

		var loaded = Assembly.Load(stream.ToArray()).GetType(name)!;

		Sizes[loaded] = (output.SyntaxTrees.Skip(1).Sum(static one => (long)one.Length), stream.Length);

		return loaded;
	}

	/// <summary>The shipped host's attributes, less the located reading, on the tape.</summary>
	/// <remarks>
	/// The tape, named: these variants have always been the tape's — what the walks read is what
	/// <c>DeepNestingCountTests</c> counts — and the shipped hosts name the immediate carrier, which
	/// a variant left to choose would now be given too.
	/// </remarks>
	const string TransactSqlHost =
		"using DotGram;\n" +
		"namespace DotGram.Sql.TransactSql.Variant\n" +
		"{\n" +
		"\t[Gram(\"TransactSql.gram\", Lexical = true, Carrier = GramCarrier.Tape)]\n" +
		"\tpublic abstract partial class TransactSqlParser\n" +
		"\t{\n" +
		"\t}\n" +
		"}\n";

	const string StandardHost =
		"using DotGram;\n" +
		"namespace DotGram.Sql.Standard.Variant\n" +
		"{\n" +
		"\t[Gram(\"SqlStandard.gram\", Lexical = true, Carrier = GramCarrier.Tape)]\n" +
		"\tpublic abstract partial class SqlStandardParser\n" +
		"\t{\n" +
		"\t}\n" +
		"}\n";

	const string Sql92Host =
		"using DotGram;\n" +
		"namespace DotGram.Sql.Standard.Variant\n" +
		"{\n" +
		"\t[Gram(\"SqlStandard92.gram\", Lexical = true, Carrier = GramCarrier.Tape)]\n" +
		"\tpublic abstract partial class Sql92Parser\n" +
		"\t{\n" +
		"\t}\n" +
		"}\n";

	static string Grammar(string directory, string file, [CallerFilePath] string here = "")
	{
		return Path.IsPathRooted(directory)
			? Path.Combine(directory, file)
			: Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "src", "DotGram.Sql", directory, file));
	}

	/// <summary>A directory of grammar fixtures beside this file: a grammar as it was, kept for a differential.</summary>
	static string Fixture(string name, [CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "Fixtures", name));
	}

	/// <summary>A .gram file handed to the generator as the build hands it one.</summary>
	sealed class InMemoryFile(string path, string text) : AdditionalText
	{
		public override string Path { get; } = path;

		public override SourceText GetText(CancellationToken cancellationToken = default)
		{
			return SourceText.From(text);
		}
	}

	/// <summary>What the build would say of <c>DotGramPositionalFollow</c>.</summary>
	sealed class Options(string follow) : AnalyzerConfigOptionsProvider
	{
		public override AnalyzerConfigOptions GlobalOptions { get; } = new Global(follow);

		public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
		{
			return GlobalOptions;
		}

		public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
		{
			return GlobalOptions;
		}
	}

	sealed class Global(string follow) : AnalyzerConfigOptions
	{
		public override bool TryGetValue(string key, out string value)
		{
			value = key == "build_property.DotGramPositionalFollow" && follow != "off" ? follow : "";

			return value.Length > 0;
		}
	}
}
