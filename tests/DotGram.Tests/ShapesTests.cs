using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What each rule's value would be carried as, were values typed shapes rather than tape.
/// </summary>
/// <remarks>
/// The first stage of the redesign in <c>docs/next.md</c> is a report and not an emitter,
/// and these are its checks: that the decision rule matches what the hand-written readings
/// in <c>benchmarks/DotGram.HandDeferred</c> did by eye, and that it can be asked of every
/// grammar in the repository at once.
/// </remarks>
public sealed class ShapesTests
{
	const string Deferred =
		"trivia = ' '*\n" +
		"Sum : @string = l: Sum & '+' & r: Pair => @(l + \"+\" + r)\n" +
		"              | one: Pair              => @(one)\n" +
		"Pair : @string = name: Name & '=' & value: Digits => @(name + \":\" + value)\n" +
		"Name   : @string = t: ['a'..'z']+ => @(t)\n" +
		"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
		"parse Sum\n";

	const string Parenthesised =
		"trivia = ' '*\n" +
		"Sum : @string = l: Sum & '+' & r: Pair => @(l + \"+\" + r)\n" +
		"              | one: Pair              => @(one)\n" +
		"Pair : @string = name: Name & '=' & value: Digits => @(name + \":\" + value)\n" +
		"                | '(' & inner: Sum & ')'          => @(\"(\" + inner + \")\")\n" +
		"Name   : @string = t: ['a'..'z']+ => @(t)\n" +
		"Digits : @string = t: ['0'..'9']+ => @(t)\n" +
		"parse Sum\n";

	/// <summary>A fold is a loop, not a cycle: nothing in the flat grammar needs a reference.</summary>
	[Fact]
	public void A_fold_alone_makes_no_class()
	{
		var report = Shapes.Of(Graph(Deferred));

		Assert.Equal(0, report.Classes);
		Assert.Empty(report.Cycles);
		Assert.True(Rule(report, "Sum").Folds);
		Assert.Equal(Shapes.Carrier.Struct, Rule(report, "Sum").Carrier);
		Assert.Equal(Shapes.Carrier.Struct, Rule(report, "Pair").Carrier);
		Assert.Equal(Shapes.Carrier.None,   Rule(report, "trivia").Carrier);
	}

	/// <summary>The parenthesis closes the cycle, and both rules on it become classes.</summary>
	[Fact]
	public void A_parenthesis_puts_Sum_and_Pair_on_a_cycle()
	{
		var report = Shapes.Of(Graph(Parenthesised));

		Assert.Equal(Shapes.Carrier.Class,  Rule(report, "Sum").Carrier);
		Assert.Equal(Shapes.Carrier.Class,  Rule(report, "Pair").Carrier);
		Assert.Equal(Shapes.Carrier.Struct, Rule(report, "Name").Carrier);
		Assert.Equal(Shapes.Carrier.Struct, Rule(report, "Digits").Carrier);

		var cycle = Assert.Single(report.Cycles);

		Assert.Equal(["Pair", "Sum"], cycle.Rules.Select(one => one.Name).OrderBy(one => one, StringComparer.Ordinal));
	}

	/// <summary>The sizes the hand-written readings measured, arrived at by the rule.</summary>
	/// <remarks>
	/// <c>Name</c> and <c>Digits</c> are two integers; <c>Pair</c> is both of them and a
	/// reference to the <c>Sum</c> it may hold — twenty-four bytes, which is what
	/// <c>Mixed.Sizes()</c> prints.
	/// </remarks>
	[Fact]
	public void Sizes_match_the_hand_written_shapes()
	{
		var report = Shapes.Of(Graph(Parenthesised));

		Assert.Equal(8,  Rule(report, "Name").Bytes);
		Assert.Equal(8,  Rule(report, "Digits").Bytes);
		Assert.Equal(24, Rule(report, "Pair").Bytes);
	}

	/// <summary>A struct nested by value counts for what it holds, not for a reference.</summary>
	[Fact]
	public void A_struct_member_is_counted_whole()
	{
		var report = Shapes.Of(Graph(Deferred));

		// Pair holds Name and Digits by value: sixteen. Sum holds l (Sum, a fold — still a
		// struct, but its own size is not known while it is being sized, so a reference),
		// r and one, both Pair by value.
		Assert.Equal(16, Rule(report, "Pair").Bytes);
	}

	/// <summary>A <c>when</c> is flagged, so the redesign knows where values are wanted early.</summary>
	[Fact]
	public void A_guard_is_flagged()
	{
		var report = Shapes.Of(Graph(
			"Start : @int = d: ['0'..'9']+ & when @(d.Length < 3) => @(int.Parse(d))\n"));

		Assert.True(Rule(report, "Start").Guarded);
	}

	/// <summary>
	/// The report over every grammar in the repository, written where it can be read and
	/// summarized in <c>docs/next.md</c>.
	/// </summary>
	[Fact]
	public void Every_grammar_in_the_repository_is_reported()
	{
		var root  = ReaderCoverageTests.Root(AppContext.BaseDirectory);
		var files = Directory.GetFiles(Path.Combine(root, "examples"), "*.cs", SearchOption.AllDirectories)
			.Concat(Directory.GetFiles(Path.Combine(root, "src", "DotGram.Parsers"), "*.cs"))
			.Concat(Directory.GetFiles(Path.Combine(root, "tests", "Snapshots"), "*.gram"))
			.Where(one => !one.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
			.OrderBy(one => one, StringComparer.Ordinal)
			.ToList();

		var written = new StringBuilder();
		var totals  = new Totals();
		var tables  = new StringBuilder();
		var seen    = 0;

		foreach (var file in files)
		{
			var grammars = file.EndsWith(".gram", StringComparison.Ordinal)
				? [(File.ReadAllText(file), false)]
				: ReaderCoverageTests.Grammars(file).ToList();

			foreach (var (text, lexical) in grammars)
			{
				var compiled = GramCompiler.Compile(text, new GramCompilerOptions
				{
					ClassName = "Probe", CSharpScanner = RoslynCSharpScanner.Instance, Lexical = lexical,
				});

				if (compiled.Diagnostics.Any(one => one.Severity == GramSeverity.Error))
					continue;

				var whole  = Graph(text, whole: true);
				var split  = lexical ? LexicalSplit.Of(whole) : null;
				var graph  = split?.Syntax ?? whole;
				var report = Shapes.Of(graph, overKinds: split is not null);
				var ways   = WaysIn(compiled.Sources[0].Text, graph);

				seen++;
				totals.Add(report, ways, split is not null);

				var name = Path.GetFileNameWithoutExtension(file) +
					(grammars.Count > 1 ? $" #{grammars.IndexOf((text, lexical)) + 1}" : "");

				written.Append($"{name,-32} {(split is not null ? "kinds" : "chars"),-5}  {report.Summary()}; {ways.Count} on ways\n");

				// The yardstick, in full: the grammar whose generated parser is measured against
				// the hand-written one.
				if (file.EndsWith("SqlStandard92.cs", StringComparison.Ordinal))
				{
					tables.Append($"\n=== {name}\n{report.Table()}");
					tables.Append($"on ways: {string.Join(", ", ways.Select(one => one.Name).OrderBy(one => one, StringComparer.Ordinal))}\n");
				}
			}
		}

		written.Append('\n').Append(totals.Summary());
		written.Append(tables);

		File.WriteAllText(Path.Combine(root, ".work", "shapes.txt"), written.ToString());

		Assert.True(seen >= 20, $"Only {seen} grammars were reported.");
	}

	/// <summary>
	/// The rules the emitter wrote a way back into: the ones whose reader got a
	/// <c>_Body</c> wrapper, which is emitted only where something under the rule can be on
	/// the tape.
	/// </summary>
	static HashSet<RuleSymbol> WaysIn(string generated, RecognitionGraph graph)
	{
		var named = new HashSet<string>(
			Regex.Matches(generated, @"static int Read_([A-Za-z][A-Za-z0-9]*)(?:_[A-Za-z0-9]+)*_Body\(")
				.Select(one => one.Groups[1].Value));

		return new HashSet<RuleSymbol>(graph.Rules.Where(one => named.Contains(one.Name)));
	}

	sealed class Totals
	{
		int _grammars, _rules, _valued, _structs, _classes, _cycles, _folds, _guarded, _gathering, _climbing;
		int _entries, _streaming, _recovering, _onWays, _overKinds;

		public void Add(Shapes.Report report, HashSet<RuleSymbol> ways, bool overKinds)
		{
			_grammars++;
			_rules      += report.Rules.Count;
			_valued     += report.Valued;
			_structs    += report.Structs;
			_classes    += report.Classes;
			_cycles     += report.Cycles.Count;
			_folds      += report.Rules.Count(one => one.Folds);
			_guarded    += report.Rules.Count(one => one.Guarded);
			_gathering  += report.Rules.Count(one => one.Sequences > 0);
			_climbing   += report.Rules.Count(one => one.Climbs);
			_entries    += report.Entries.Count;
			_streaming  += report.Entries.Count(one => one.Streams);
			_recovering += report.Recovers ? 1 : 0;
			_onWays     += ways.Count;
			_overKinds  += overKinds ? 1 : 0;
		}

		public string Summary() =>
			$"{_grammars} grammars ({_overKinds} over kinds): {_rules} rules, {_valued} valued, " +
			$"{_structs} structs, {_classes} classes in {_cycles} cycles; {_folds} folds, {_guarded} guarded, " +
			$"{_gathering} gathering, {_climbing} climbing; {_onWays} on ways; " +
			$"{_entries} entries, {_streaming} streaming, {_recovering} grammars recover\n";
	}

	static Shapes.Rule Rule(Shapes.Report report, string name) =>
		report.Rules.Single(one => one.Symbol.Name == name);

	static RecognitionGraph Graph(string text, bool whole = false) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(
					GramLexer.Tokenize(whole ? text : text + "\nparse Start", RoslynCSharpScanner.Instance)).File));
}
