using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

using DotGram.Generation;
using DotGram.Sql.Standard;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Brackets nested in a SQL condition, counted in rule entries: what each nest costs today, held
/// so that it cannot get worse unseen, and the nests that cost their depth held to it.
/// </summary>
/// <remarks>
/// <para>
/// <b>What it holds.</b> <c>BooleanPrimary</c> (SqlStandard92.gram) asks for a predicate before a
/// bracketed condition, so each level of <c>WHERE ((( … a = 1 … )))</c> first reads everything below
/// it as the value its predicate opens with, and each value bracket reads everything below it once
/// more as a subquery: the accepted nest cost the cube of its depth, and one never closed tried
/// every alternative and cost its cube too — until the reader remembered where a recursive rule had
/// failed (<c>Machine.Memo.cs</c>). Nearly all of those readings were the same rule failing at the
/// same token again, and now both nests cost their depth. Values nested in a predicate, <c>((( a ))) = 1</c>, cost
/// their depth. Asking for the bracketed condition first was tried: the nest of conditions became
/// linear, values in a predicate became a square, and a bracketed value holding a condition of its
/// own, compared, became exponential — which is why those shapes are held here as well.
/// </para>
/// <para>
/// <b>Counted, not timed</b> (D144). Every rule the reader reads by a method of its own counts its
/// entries where the compilation defines <c>DOTGRAM_COUNTS</c>, and the count here is their sum. The
/// grammars are <c>TransactSql.gram</c> and <c>SqlStandard92.gram</c> as they ship, run through the
/// generator here with the symbol defined, under a name the package shows its internals to, since
/// the code beside the grammars uses them. A count is a function of the grammar and the generator,
/// so it is what the shipped parser does, without a clock or a quiet machine.
/// </para>
/// <para>
/// <b>Depths a doubling apart</b>, and what each doubling adds may grow by at most the factor of the
/// nest's class, with a tenth to spare for the ends: two for a depth. Counted at 25, 50, 100 and 200
/// levels, an entry the memo answers counted as one (without it the two nests were 5,660,639 and
/// 11,321,278 at 200):
/// </para>
/// <code>
///   a nest of conditions, accepted          561    1,061    2,061    4,061   n
///   values nested in a predicate            114      189      339      639   n
///   a nest of conditions, never closed    1,122    2,122    4,122    8,122   n
/// </code>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class BracketNestingCountTests
{
	/// <summary>Four depths, a doubling apart.</summary>
	static readonly int[] Depths = [25, 50, 100, 200];

	/// <summary>How much more than its class a doubling may add, for what the ends of the text cost.</summary>
	const double Slack = 1.1;

	[Theory]
	[InlineData("a nest of conditions, accepted",         2)]
	[InlineData("values nested in a predicate, accepted", 2)]
	[InlineData("a nest of conditions, never closed",     2)]
	public void A_nest_of_brackets_costs_no_more_than_its_class(string shape, int growth)
	{
		var counts = new List<long>();

		foreach (var depth in Depths)
		{
			var text = "SELECT 1 WHERE " + Nest(shape, depth);

			counts.Add(Count(text, accepted: !shape.EndsWith("never closed", StringComparison.Ordinal)));
			TestContext.Current.TestOutputHelper?.WriteLine($"{shape}: {counts[^1]:N0} rule entries at {depth} levels");

			// Or the counters are not being written and everything below passes on nothing.
			Assert.True(counts[0] > 0, $"{shape}: nothing was counted at {Depths[0]} levels.");

			if (counts.Count < 3)
				continue;

			var earlier = counts[^2] - counts[^3];
			var later   = counts[^1] - counts[^2];

			Assert.True(
				earlier > 0 && later <= earlier * growth * Slack,
				$"{shape}: {string.Join(", ", counts)} rule entries at " +
				$"{string.Join(", ", Depths.Take(counts.Count))} levels, so a doubling added {later} where " +
				$"the one before added {earlier}, {(double)later / earlier:F2} times as much against " +
				$"{growth} for the class this nest is held to.");
		}
	}

	/// <summary>
	/// A bracketed value that holds a condition of its own, compared, with the next level inside
	/// that condition: <c>(CASE WHEN (CASE WHEN … THEN 1 END) = 1 THEN 1 END) = 1</c>. Read as the
	/// predicate's operand first, each level is read once, and the count grows by a constant a level
	/// (147, 255, 471 at 4, 8 and 16). Asked as a condition first, each bracket read everything inside
	/// it before the comparison after it failed the attempt, and again as the operand: twice a level,
	/// doubling with each, over ten seconds at 25 levels. Held linear here, at depths small enough
	/// that the doubling fails in milliseconds.
	/// </summary>
	[Theory]
	[InlineData("a bracketed CASE compared, nested in its WHEN")]
	[InlineData("a bracketed IIF compared, nested in its condition")]
	[InlineData("a twice bracketed subquery compared, nested in its WHERE")]
	public void A_bracketed_value_that_holds_a_condition_costs_its_depth(string shape)
	{
		Holds_its_depth(shape, depth => Count("SELECT 1 WHERE " + Holding(shape, depth), accepted: true));
	}

	/// <summary>The same in the standard's own parser, which has no <c>IIF</c>.</summary>
	[Theory]
	[InlineData("a bracketed CASE compared, nested in its WHEN")]
	[InlineData("a twice bracketed subquery compared, nested in its WHERE")]
	public void A_bracketed_value_that_holds_a_condition_costs_its_depth_in_SQL_92(string shape)
	{
		Holds_its_depth(shape, depth => Count(Sql92, "TryParseSelect", "SELECT a FROM t WHERE " + Holding(shape, depth), accepted: true));
	}

	/// <summary>
	/// <b>A known super-linear shape</b>, already so before any reordering: a twice bracketed
	/// <c>CASE</c>, compared, whose <c>WHEN</c> holds the next level in a bracket of its own,
	/// <c>((CASE WHEN (…) THEN 1 END)) = 1</c>. Each level cost three times the one inside it — 3,143,
	/// 9,434, 28,307, 84,926 and 254,783 rule entries at 4 to 8 levels, 2.3 million at 10. Remembering
	/// where a rule failed took nine tenths of that away: 1,505, 3,171, 6,533, 13,287 and 26,825, twice
	/// a level, every one of what is left a reading that succeeded and is read again. Held to that
	/// factor so that it cannot get worse; remembering successes too should make it linear, and then
	/// this is to be tightened to <see cref="Holds_its_depth"/>.
	/// </summary>
	[Fact]
	public void A_bracketed_CASE_whose_WHEN_is_bracketed_is_known_to_cost_twice_a_level()
	{
		const string shape = "a twice bracketed CASE compared, its WHEN bracketed";
		const double known = 2;

		var counts = new List<long>();

		for (var depth = 4; depth <= 8; depth++)
		{
			counts.Add(Count("SELECT 1 WHERE " + Holding(shape, depth), accepted: true));
			TestContext.Current.TestOutputHelper?.WriteLine($"{shape}: {counts[^1]:N0} rule entries at {depth} levels");

			if (counts.Count < 2)
				continue;

			Assert.True(
				counts[^2] > 0 && counts[^1] <= counts[^2] * known * Slack,
				$"{shape}: {string.Join(", ", counts)} rule entries from 4 levels, a level apart, so the last level " +
				$"cost {(double)counts[^1] / counts[^2]:F2} times the one before against {known} known.");
		}
	}

	static void Holds_its_depth(string shape, Func<int, long> count)
	{
		int[] depths = [4, 8, 16];
		var counts   = new List<long>();

		foreach (var depth in depths)
		{
			counts.Add(count(depth));
			TestContext.Current.TestOutputHelper?.WriteLine($"{shape}: {counts[^1]:N0} rule entries at {depth} levels");
		}

		var earlier = counts[1] - counts[0];
		var later   = counts[2] - counts[1];

		Assert.True(
			earlier > 0 && later <= earlier * 2 * Slack,
			$"{shape}: {string.Join(", ", counts)} rule entries at {string.Join(", ", depths)} levels, so a " +
			$"doubling added {later} where the one before added {earlier}, {(double)later / earlier:F2} times as " +
			"much against 2 for a nest that costs its depth.");
	}

	static string Holding(string shape, int depth)
	{
		var text = "a = 1";

		for (var level = 0; level < depth; level++)
		{
			text = shape switch
			{
				"a bracketed CASE compared, nested in its WHEN"            => "(CASE WHEN " + text + " THEN 1 END) = 1",
				"a bracketed IIF compared, nested in its condition"        => "(IIF(" + text + ", 1, 0)) = 1",
				"a twice bracketed subquery compared, nested in its WHERE" => "((SELECT 1 FROM t WHERE " + text + ")) = 1",
				"a twice bracketed CASE compared, its WHEN bracketed"      => "((CASE WHEN (" + text + ") THEN 1 END)) = 1",
				_                                                          => throw new ArgumentOutOfRangeException(nameof(shape)),
			};
		}

		return text;
	}

	static string Nest(string shape, int depth)
	{
		var open  = new string('(', depth);
		var close = new string(')', depth);

		return shape switch
		{
			"a nest of conditions, accepted"         => open + "a = 1" + close,
			"values nested in a predicate, accepted" => open + "a" + close + " = 1",
			"a nest of conditions, never closed"     => open + "a = 1",
			_                                        => throw new ArgumentOutOfRangeException(nameof(shape)),
		};
	}

	/// <summary>Rule entries of one reading of a statement at level 170, from a fresh zero.</summary>
	static long Count(string text, bool accepted)
	{
		return Count(Parser, "TryParseStatement170", text, accepted);
	}

	/// <summary>Rule entries of one reading by a counted parser's publication, from a fresh zero.</summary>
	static long Count(Type parser, string publication, string text, bool accepted)
	{
		var counters = Counters(parser);

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		var read = parser.GetMethod(
			publication,
			BindingFlags.Public | BindingFlags.Static,
			binder: null,
			[typeof(string)],
			modifiers: null)!;

		object?    match = null;
		Exception? error = null;

		// On a stack with room for the nest, so that no reading is carried onto another thread.
		var thread = new Thread(() =>
		{
			try { match = read.Invoke(null, [text]); }
			catch (Exception e) { error = e; }
		}, 256 * 1024 * 1024);

		thread.Start();
		thread.Join();

		if (error is not null)
			throw new InvalidOperationException("the counted reading threw", error);

		Assert.Equal(accepted, (bool)match!.GetType().GetProperty("IsSuccess")!.GetValue(match)!);

		return counters.Sum(counter => (long)counter.GetValue(null)!);
	}

	/// <summary>Every rule's counter, in every reader of the counted parser.</summary>
	static FieldInfo[] Counters(Type parser)
	{
		return
		[
			.. parser.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public)
				.SelectMany(type => type.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
				.Where(field => field.Name.StartsWith("CountEntered_", StringComparison.Ordinal)),
		];
	}

	/// <summary>The counted copy of <c>TransactSqlParser</c>.</summary>
	static Type Parser => _parser.Value;

	static readonly Lazy<Type> _parser = new(() => Compile(Host, "TransactSql", "TransactSql.gram", "DotGram.Sql.TransactSql.Counted.TransactSqlParser"));

	/// <summary>The counted copy of <c>Sql92Parser</c>, whose <c>BooleanPrimary</c> T-SQL reads.</summary>
	static Type Sql92 => _sql92.Value;

	static readonly Lazy<Type> _sql92 = new(() => Compile(Host92, "Standard", "SqlStandard92.gram", "DotGram.Sql.Standard.Counted.Sql92Parser"));

	/// <summary>The shipped host's attributes.</summary>
	const string Host92 =
		"using DotGram;\n" +
		"namespace DotGram.Sql.Standard.Counted\n" +
		"{\n" +
		"\t[Gram(\"SqlStandard92.gram\", Lexical = true)]\n" +
		"\tpublic abstract partial class Sql92Parser\n" +
		"\t{\n" +
		"\t}\n" +
		"}\n";

	/// <summary>The shipped host's attributes, less the located reading, which is not counted here.</summary>
	const string Host =
		"using DotGram;\n" +
		"namespace DotGram.Sql.TransactSql.Counted\n" +
		"{\n" +
		"\t[GramInclude(typeof(DotGram.Sql.Standard.Sql92Parser), As = \"Sql92\")]\n" +
		"\t[Gram(\"TransactSql.gram\", Lexical = true)]\n" +
		"\tpublic abstract partial class TransactSqlParser\n" +
		"\t{\n" +
		"\t}\n" +
		"}\n";

	static Type Compile(string hostText, string directory, string file, string name)
	{
		var parse = CSharpParseOptions.Default
			.WithLanguageVersion(LanguageVersion.Latest)
			.WithPreprocessorSymbols("DOTGRAM_COUNTS");
		var host  = CSharpSyntaxTree.ParseText(hostText, parse, "Host.cs");
		var gram  = Grammar(directory, file);

		// The package by name: what this process had loaded when the list was first taken need not
		// have included it yet.
		var package    = typeof(Sql92Parser).Assembly.Location;
		var references = EmittedCode.References
			.Where(reference => !string.Equals(reference.Display, package, StringComparison.OrdinalIgnoreCase))
			.Append(MetadataReference.CreateFromFile(package));

		// The one name DotGram.Sql shows its internals to, which the code beside its grammars reads.
		var compilation = CSharpCompilation.Create(
			"DotGram.Benchmarks",
			[host],
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		CSharpGeneratorDriver
			.Create(
				[new GramGenerator().AsSourceGenerator()],
				additionalTexts: [new InMemoryFile(gram, File.ReadAllText(gram))],
				parseOptions: parse)
			.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var reported);

		Assert.DoesNotContain(reported, one => one.Severity == DiagnosticSeverity.Error);

		// Or the counters are not in what was generated and every count is zero, which is flat.
		Assert.Contains(output.SyntaxTrees, tree => tree.ToString().Contains("CountEntered_", StringComparison.Ordinal));

		using var stream = new MemoryStream();

		var result = output.Emit(stream);

		Assert.True(
			result.Success,
			$"The counted {name} did not compile:\n" +
			string.Join("\n", result.Diagnostics.Where(one => one.Severity == DiagnosticSeverity.Error).Take(20)));

		return Assembly.Load(stream.ToArray()).GetType(name)!;
	}

	static string Grammar(string directory, string file, [CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "src", "DotGram.Sql", directory, file));
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
}
