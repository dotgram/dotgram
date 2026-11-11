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
/// to compare: with and without the memo of failures, with and without the memo of successes, on
/// the tape or on the immediate carrier, under each value of the positional follow, and with the
/// entry counters or without; and two rewrites of the SQL-92 grammar kept as fixtures beside it,
/// for a differential against the grammar as it ships.
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

	/// <summary>
	/// The SQL-92 grammar with every bracket read once and classified after (a fixture under
	/// <c>Fixtures/ReadOnce/Cover</c>): the rewrite that needs no memo, kept to be measured against
	/// the grammar as it ships with the memo of successes.
	/// </summary>
	public const string Sql92Cover = "Sql92Cover";

	/// <summary>The same with the value tower instantiated twice, once a context (a fixture under <c>Fixtures/ReadOnce/Split</c>).</summary>
	public const string Sql92Split = "Sql92Split";

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

	/// <summary>A parser of one grammar compiled one way, on the tape these variants have always been on.</summary>
	/// <param name="follow"><c>off</c>, <c>true</c> or <c>split</c>, as <c>DotGramPositionalFollow</c> says it.</param>
	/// <param name="memo">Whether the memo of failures is written (<c>DOTGRAM_NO_MEMO</c> where not).</param>
	/// <param name="counts">Whether the entry counters are written (<c>DOTGRAM_COUNTS</c>).</param>
	public static Type Parser(string grammar, string follow = "off", bool memo = true, bool counts = false)
	{
		return Parser(grammar, follow, memo, counts, immediate: false);
	}

	/// <summary>A parser of one grammar compiled one way, on the immediate carrier (the shipped parsers') or on the tape.</summary>
	/// <param name="immediate">Whether the host names the immediate carrier rather than the tape.</param>
	/// <param name="keeps">Whether the memo of successes is written (<c>DOTGRAM_NO_SUCCESS_MEMO</c> where not).</param>
	/// <param name="located">Whether a located reading is compiled beside the plain one, as a nested type <c>Located</c>.</param>
	public static Type Parser(string grammar, string follow, bool memo, bool counts, bool immediate, bool keeps = true, bool located = false)
	{
		return Compiled.GetOrAdd((grammar, follow, memo, counts, immediate, keeps, located), static key => Compile(key.Grammar, key.Follow, key.Memo, key.Counts, key.Immediate, key.Keeps, key.Located));
	}

	/// <summary>The size of what a variant compiled to: the generated C# in characters and the assembly in bytes.</summary>
	public static readonly ConcurrentDictionary<Type, (long SourceChars, long AssemblyBytes)> Sizes = new();

	static readonly ConcurrentDictionary<(string Grammar, string Follow, bool Memo, bool Counts, bool Immediate, bool Keeps, bool Located), Type> Compiled = new();

	static Type Compile(string grammar, string follow, bool memo, bool counts, bool immediate, bool keeps, bool located)
	{
		var (directory, file, className, helpers) = grammar switch
		{
			TransactSql => ("TransactSql", "TransactSql.gram", "TransactSqlParser", ""),
			Standard    => ("Standard", "SqlStandard.gram", "SqlStandardParser", ""),
			Sql92       => ("Standard", "SqlStandard92.gram", "Sql92Parser", ""),
			Sql92Cover  => (Fixture(Path.Combine("ReadOnce", "Cover")), "SqlStandard92.gram", "Sql92Parser", CoverNodes),
			Sql92Split  => (Fixture(Path.Combine("ReadOnce", "Split")), "SqlStandard92.gram", "Sql92Parser", CoverNodes),
			_           => throw new ArgumentOutOfRangeException(nameof(grammar)),
		};

		// A namespace of its own for every way of compiling, so that two variants loaded into one
		// process never share a name.
		var ns   = "DotGram.Sql." + (immediate ? "Immediate" : "Tape") + (keeps ? "" : "Unkept") + (located ? "Located" : "") + "." + grammar;
		var name = ns + "." + className;
		var host = Host
			.Replace("NAMESPACE", ns)
			.Replace("GRAMMAR", file)
			.Replace("CARRIER", immediate ? "Immediate" : "Tape")
			.Replace("CLASS", className)
			.Replace("LOCATED", located ? "\t[GramVariant(LocationType = typeof(global::DotGram.Sql.ISqlLocatable), Name = \"Located\")]\n" : "")
			.Replace("HELPERS", helpers);

		return Compile(host, directory, file, name, follow, memo, counts, keeps);
	}

	static Type Compile(string host, string directory, string file, string name, string follow, bool memo, bool counts, bool keeps)
	{
		string[] symbols = [.. counts ? ["DOTGRAM_COUNTS"] : Array.Empty<string>(), .. memo ? Array.Empty<string>() : ["DOTGRAM_NO_MEMO"], .. keeps ? Array.Empty<string>() : ["DOTGRAM_NO_SUCCESS_MEMO"]];

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

		if (!keeps)
			Assert.DoesNotContain(output.SyntaxTrees, one => one.ToString().Contains("Kept(pos", StringComparison.Ordinal));

		if (counts)
			Assert.Contains(output.SyntaxTrees, one => one.ToString().Contains("CountEntered_", StringComparison.Ordinal));

		using var stream = new MemoryStream();

		var result = output.Emit(stream);

		Assert.True(
			result.Success,
			$"{name} ({follow}, memo {memo}) did not compile:\n" +
			string.Join("\n", result.Diagnostics.Where(one => one.Severity == DiagnosticSeverity.Error).Take(20)));

		var loaded = Assembly.Load(stream.ToArray()).GetType(name)!;

		if (Environment.GetEnvironmentVariable("SQLVARIANTS_DUMP") is { Length: > 0 } dump)
		{
			Directory.CreateDirectory(dump);

			foreach (var one in output.SyntaxTrees.Skip(1))
				File.WriteAllText(Path.Combine(dump, name + (memo ? "" : ".nomemo") + (counts ? ".counts" : "") + "." + Path.GetFileName(one.FilePath)), one.ToString());
		}

		Sizes[loaded] = (output.SyntaxTrees.Skip(1).Sum(static one => (long)one.Length), stream.Length);

		return loaded;
	}

	/// <summary>The shipped host's attributes, less the located reading, with the carrier, the namespace and the class named per variant.</summary>
	/// <remarks>
	/// The tape, named, for the variants that have always been the tape's — what the walks read is
	/// what <c>DeepNestingCountTests</c> counts — and the immediate carrier, named, for the shipped
	/// parsers' own, which a variant left to choose would be given too.
	/// </remarks>
	const string Host =
		"using DotGram;\n" +
		"using DotGram.Sql.Ast;\n" +
		"namespace NAMESPACE\n" +
		"{\n" +
		"\t[Gram(\"GRAMMAR\", Lexical = true, Carrier = GramCarrier.CARRIER)]\n" +
		"LOCATED" +
		"\tpublic abstract partial class CLASS\n" +
		"\t{\n" +
		"\t}\n" +
		"HELPERS" +
		"}\n";

	/// <summary>
	/// What the rewritten SQL-92 grammars ask of a value the one tower read: whether it is a condition,
	/// a value an arithmetic operator may take, or a bare <c>NULL</c> or <c>DEFAULT</c> only a row may begin
	/// with. The shipped grammar reads conditions and values by rules of their own and asks none of this.
	/// </summary>
	const string CoverNodes =
		"\tstatic class CoverNodes\n" +
		"\t{\n" +
		"\t\tpublic static bool IsCondition(Expression value)\n" +
		"\t\t{\n" +
		"\t\t\tswitch (value)\n" +
		"\t\t\t{\n" +
		"\t\t\t\tcase Expression.Comparison:\n" +
		"\t\t\t\tcase Expression.QuantifiedComparison:\n" +
		"\t\t\t\tcase Expression.Between:\n" +
		"\t\t\t\tcase Expression.In:\n" +
		"\t\t\t\tcase Expression.Like:\n" +
		"\t\t\t\tcase Expression.IsNull:\n" +
		"\t\t\t\tcase Expression.IsTruth:\n" +
		"\t\t\t\tcase Expression.Match:\n" +
		"\t\t\t\tcase Expression.Overlaps:\n" +
		"\t\t\t\tcase Expression.Exists:\n" +
		"\t\t\t\tcase Expression.Unique:\n" +
		"\t\t\t\t\treturn true;\n" +
		"\t\t\t\tcase Expression.Binary { Operator: BinaryOperator.And or BinaryOperator.Or }:\n" +
		"\t\t\t\t\treturn true;\n" +
		"\t\t\t\tcase Expression.Unary { Operator: UnaryOperator.Not } not:\n" +
		"\t\t\t\t\treturn IsCondition(not.Operand);\n" +
		"\t\t\t\tcase Expression.Parenthesized parenthesized:\n" +
		"\t\t\t\t\treturn IsCondition(parenthesized.Value);\n" +
		"\t\t\t\tdefault:\n" +
		"\t\t\t\t\treturn false;\n" +
		"\t\t\t}\n" +
		"\t\t}\n" +
		"\t\tpublic static bool IsValue(Expression value)\n" +
		"\t\t{\n" +
		"\t\t\treturn value is not Expression.Row && !IsCondition(value);\n" +
		"\t\t}\n" +
		"\t\tpublic static bool IsRowHeadOnly(Expression value)\n" +
		"\t\t{\n" +
		"\t\t\treturn value is LiteralValue.Null or Expression.Default;\n" +
		"\t\t}\n" +
		"\t}\n";

	static string Grammar(string directory, string file, [CallerFilePath] string here = "")
	{
		return Path.IsPathRooted(directory)
			? Path.Combine(directory, file)
			: Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "src", "DotGram.Sql", directory, file));
	}

	/// <summary>A directory of grammar fixtures beside this file: a grammar rewritten, kept for a differential.</summary>
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
