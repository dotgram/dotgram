using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A file compiled without its settled copies, because a machine of it is not a reader over a string,
/// says of each rule whose run has no upper bound that a refusal inside a long run of it takes time
/// quadratic in its length (GRAM5022, and GRAM5023 for a grammar that reads segments): once for each
/// such rule, with the cause the plan gives and the one remedy that can be done, naming the first
/// publication of that cause; and nowhere a copy is kept, a bounded run gives back, or the author's own
/// options leave no copies.
/// </summary>
public sealed class SettledDroppedWarningTests
{
	/// <summary>
	/// A name ending in a run, called where an attribute may follow and before an attribute's <c>'='</c>:
	/// the name gives characters back at the first and nothing could take them at the second.
	/// </summary>
	static string Names(string run, string publications, string start = "Start : @string = p: Pair & (';' & Pair)* & eof => @(p)")
	{
		return $$"""
			trivia = none

			Word  = ['a'..'z'] & ['a'..'z' | '0'..'9']{{run}}
			Name  : @string = text: Word => @(text)
			Path  : @string = n: Name => @(n)
			                | '/' & p: Path => @("/" + p)
			Pair  : @string = k: Path & '=' & a: Path & b: Name? & '!' => @(k + "=" + a + "|" + b)
			Items : @string[] = Pair*
			{{start}}

			{{publications}}
			""";
	}

	/// <summary>A start rule that captures what a lookahead saw, which methods do not read: the engine does.</summary>
	const string Peeking = "Start : @string = p: Pair & (';' & Pair)* & peek: ?=Word & eof => @(p)";

	const string Move = "Move it into its own grammar.";

	static string Because(string rule, string reason, string remedy)
	{
		return $"Rule '{rule}' is read without its settled copy because {reason}, " +
			$"so a refusal inside a long run of '{rule}' takes time quadratic in its length." +
			(remedy.Length == 0 ? "" : " " + remedy);
	}

	[Fact]
	public void A_find_is_always_read_by_the_engine_and_is_only_moved()
	{
		var grammar = Names("*", "parse Start\nfind Pair");
		var told    = Assert.Single(Told(grammar));

		Assert.Equal(Because("Word", "publication 'FindPair' runs on the engine", Move), told.Message);
		Assert.Equal(GramSeverity.Warning, told.Severity);
		Assert.Equal(grammar.IndexOf("Word  =", StringComparison.Ordinal), told.Position);
	}

	[Fact]
	public void A_yield_is_told_as_a_find_is()
	{
		var told = Assert.Single(Told(Names("*", "parse Items as Lazy yield : @string")));

		Assert.Equal(Because("Word", "publication 'Lazy' runs on the engine", Move), told.Message);
	}

	[Fact]
	public void A_parse_that_methods_cannot_read_keeps_the_approved_words()
	{
		var told = Assert.Single(Told(Names("*", "parse Start", Peeking)));

		Assert.Equal(
			"Rule 'Word' is read without its settled copy because publication 'ParseStart' runs on the engine, " +
			"so a refusal inside a long run of 'Word' takes time quadratic in its length. " +
			"Read 'ParseStart' by methods, or move it into its own grammar.",
			told.Message);
	}

	[Fact]
	public void A_parse_with_no_result_type_is_also_read_a_window_at_a_time_and_is_told_to_give_it_one()
	{
		var told = Assert.Single(Told(Names("*", "parse Items")));

		Assert.Equal(
			Because("Word", "publication 'ParseItems' is also read a window at a time, on the engine",
				"Give 'Items' a result type, or move it into its own grammar."),
			told.Message);

		Assert.Empty(Told(Names("*", "parse Items : @string[]")));
	}

	[Fact]
	public void A_positional_parse_is_read_from_a_position()
	{
		var told = Assert.Single(Told(Names("*", "parse Start\npositional parse Pair")));

		Assert.Equal(Because("Word", "publication 'ParsePair' is read from a position", Move), told.Message);
	}

	[Fact]
	public void A_stream_is_read_from_a_buffer()
	{
		var told = Assert.Single(Told(Names("*", "parse Start\nparse Pair as Streamed : @string stream")));

		Assert.Equal(Because("Word", "publication 'Streamed' reads a stream", Move), told.Message);
	}

	[Fact]
	public void A_plain_parse_the_plan_compiles_flat_is_not_told_to_be_read_by_methods()
	{
		var told = Assert.Single(Told(Names("*", "parse Start\nparse Name")));

		Assert.Equal(Because("Word", "publication 'ParseName' is compiled flat", Move), told.Message);
	}

	[Fact]
	public void A_grammar_that_reads_segments_is_informed_and_no_publication_is_blamed()
	{
		var result = Compile(Names("*", "parse Start", "Start : @string = p: Pair & (eos & Pair)* & eos => @(p)"), settle: true);
		var told   = Assert.Single(result.Diagnostics, static one => one.Id == GramCompiler.SettledDroppedSegments);

		Assert.Equal(Because("Word", "the grammar reads segments ('eos')", ""), told.Message);
		Assert.Equal(GramSeverity.Info, told.Severity);
		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GramCompiler.SettledDropped);
	}

	[Fact]
	public void A_publication_that_asks_for_it_is_named_before_a_plain_parse_of_the_plan()
	{
		var told = Assert.Single(Told(Names("*", "parse Name\nfind Pair", Peeking)));

		Assert.Contains("publication 'FindPair' runs", told.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void The_first_publication_in_source_order_of_the_cause_is_named()
	{
		Assert.Contains("publication 'FindPath' runs", Assert.Single(Told(Names("*", "find Path\nfind Pair"))).Message, StringComparison.Ordinal);
		Assert.Contains("publication 'ParseName' is compiled flat", Assert.Single(Told(Names("*", "parse Name\nparse Start", Peeking))).Message, StringComparison.Ordinal);
		Assert.Contains("publication 'ParseStart' runs on the engine", Assert.Single(Told(Names("*", "parse Start\nparse Name", Peeking))).Message, StringComparison.Ordinal);
	}

	[Fact]
	public void A_run_at_the_end_of_a_rule_the_tail_calls_is_told_at_the_rule_it_ends_and_once()
	{
		var grammar = Names("*", "find Pair");
		var told    = Assert.Single(Told(grammar));

		// `Name` and `Path` end in a call that ends in the run: they are copied, and are not what is told.
		Assert.StartsWith("Rule 'Word' ", told.Message, StringComparison.Ordinal);
		Assert.Equal(grammar.IndexOf("Word  =", StringComparison.Ordinal), told.Position);
	}

	[Theory]
	[InlineData("+")]
	[InlineData("{2,}")]
	public void Any_run_with_no_upper_bound_is_told(string run)
	{
		Assert.Equal(["GRAM5022"], Told(Names(run, "parse Start\nfind Pair")).Select(static one => one.Id));
	}

	[Theory]
	[InlineData("{0,8}")]
	[InlineData("{1,3}")]
	public void A_bounded_run_is_never_told(string run)
	{
		Assert.Empty(Told(Names(run, "parse Start\nfind Pair")));
	}

	[Fact]
	public void Nothing_is_told_where_every_machine_is_a_reader_and_the_copies_are_kept()
	{
		var result = Compile(Names("*", "parse Start"), settle: true);

		Assert.DoesNotContain(result.Diagnostics, static one => one.Id is GramCompiler.SettledDropped or GramCompiler.SettledDroppedSegments);
		Assert.Contains("_Settled(", Assert.Single(result.Sources).Text, StringComparison.Ordinal);
	}

	[Fact]
	public void Nothing_is_told_where_the_copies_are_not_asked_for()
	{
		Assert.DoesNotContain(Compile(Names("*", "parse Start\nfind Pair"), settle: false).Diagnostics, static one => one.Id == "GRAM5022");
	}

	/// <summary>
	/// Where the author's own options leave no copies there is nothing to drop and nothing is said. Over
	/// kinds it is the one place a machine that only reads a token again is made, and it settles nothing: that
	/// machine is never the cause of a dropped copy.
	/// </summary>
	[Theory]
	[InlineData("lexical")]
	[InlineData("methods")]
	[InlineData("buffered")]
	public void Nothing_is_told_where_the_options_leave_no_copies(string option)
	{
		var result = GramCompiler.Compile(Names("*", "parse Start\nfind Pair"), new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			Lexical        = option == "lexical",
			Direct         = option != "methods",
			BufferedInput  = option == "buffered",
		});

		Assert.DoesNotContain(result.Diagnostics, static one => one.Id is GramCompiler.SettledDropped or GramCompiler.SettledDroppedSegments);
		Assert.True(result.Graph is null || result.Graph.Settled.Count == 0);
	}

	[Fact]
	public void Telling_it_changes_nothing_in_the_file()
	{
		var grammar = Names("*", "parse Start\nfind Pair");
		var with    = Compile(grammar, settle: true);
		var without = Compile(grammar, settle: false);

		Assert.Equal(without.Sources.Select(static one => one.Text), with.Sources.Select(static one => one.Text));
		Assert.Equal(
			without.Diagnostics.Select(static one => one.Id).Append("GRAM5022").Order(StringComparer.Ordinal),
			with.Diagnostics.Select(static one => one.Id).Order(StringComparer.Ordinal));
	}

	/// <summary>
	/// Every grammar that is shipped — the packages' and the examples' — read over characters says nothing
	/// of this: those that end a rule in an unbounded run that gives characters back are compiled whole,
	/// with what they include, and none of them is put on the engine by a publication. The grammars the
	/// tests keep as fixtures are not shipped, and some of them have a <c>find</c> beside such a rule on
	/// purpose.
	/// </summary>
	[Fact]
	public void No_grammar_of_the_repository_is_told_it()
	{
		var told       = new System.Collections.Concurrent.ConcurrentBag<string>();
		var faults     = new System.Collections.Concurrent.ConcurrentBag<string>();
		var candidates = new System.Collections.Concurrent.ConcurrentBag<string>();
		var read       = 0;

		System.Threading.Tasks.Parallel.ForEach(
			Shipped().Where(static grammar => grammar.Modes.Contains(false)),
			grammar =>
			{
				System.Threading.Interlocked.Increment(ref read);

				try
				{
					if (ExponentialRepetitionTests.Built(grammar, lexical: false, out _, settle: true).SettledRuns.Count == 0)
						return;

					candidates.Add(grammar.Name);

					var options = new GramCompilerOptions
					{
						ClassName      = "Grammar",
						CSharpScanner  = RoslynCSharpScanner.Instance,
						SymbolResolver = PermissiveSymbolResolver.Instance,
					};

					var text = grammar.Text;

					if (grammar.Included.Count > 0)
					{
						var (joined, map) = GrammarSplice.Join(
							new GrammarSplice.Part(text, null, null),
							[.. grammar.Included.Select(static one => new GrammarSplice.Part(one.Text, one.Name, null))]);

						options.Own  = text.Length;
						options.Cuts = GrammarSplice.Cuts(joined, map.Segments.Skip(1).Select(static one => (one.Start, one.Length)));
						text         = joined;
					}

					var result = GramCompiler.Compile(text, options);

					foreach (var diagnostic in result.Diagnostics)
						if (diagnostic.Id is GramCompiler.SettledDropped or GramCompiler.SettledDroppedSegments)
							told.Add($"{grammar.Name}: {diagnostic.Message}");
						else if (diagnostic.Severity == GramSeverity.Error)
							faults.Add($"{grammar.Name}: {diagnostic.Id} {diagnostic.Message}");
				}
				catch (Exception error)
				{
					faults.Add($"{grammar.Name}: threw {error.GetType().Name}: {error.Message}");
				}
			});

		TestContext.Current.SendDiagnosticMessage(
			$"{read} grammars read over characters; {candidates.Count} have a rule that a copy settles an unbounded run of: " +
			string.Join(", ", candidates.Order(StringComparer.Ordinal)));

		Assert.True(read > 0);
		Assert.Contains(candidates, static name => name.StartsWith("XmlExample", StringComparison.Ordinal));
		Assert.True(faults.IsEmpty, string.Join("\n", faults.Order(StringComparer.Ordinal)));
		Assert.True(told.IsEmpty, string.Join("\n", told.Order(StringComparer.Ordinal)));
	}

	static IEnumerable<ExponentialRepetitionTests.CorpusGrammar> Shipped()
	{
		return new[] { "src", "examples" }
			.Select(static folder => Path.Combine(SourceRoot.Directory, folder))
			.Where(Directory.Exists)
			.SelectMany(static root => ExponentialRepetitionTests.Grammars(root));
	}

	static List<GramDiagnostic> Told(string grammar)
	{
		return [.. Compile(grammar, settle: true).Diagnostics.Where(static one => one.Id == "GRAM5022")];
	}

	static GramCompilation Compile(string grammar, bool settle)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			SettleRunTails = settle,
		});
	}
}
