using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A file compiled without its settled copies, because a publication is read by the engine, says of
/// each rule whose run has no upper bound that a refusal inside a long run of it takes time quadratic
/// in its length (GRAM5022): once for each such rule, naming the first publication that put the file
/// on the engine, and nowhere a copy is kept or a bounded run gives back.
/// </summary>
public sealed class SettledDroppedWarningTests
{
	/// <summary>
	/// A name ending in a run, called where an attribute may follow and before an attribute's <c>'='</c>:
	/// the name gives characters back at the first and nothing could take them at the second.
	/// </summary>
	static string Names(string run, string publications)
	{
		return $$"""
			trivia = none

			Word  = ['a'..'z'] & ['a'..'z' | '0'..'9']{{run}}
			Name  : @string = text: Word => @(text)
			Path  : @string = n: Name => @(n)
			                | '/' & p: Path => @("/" + p)
			Pair  : @string = k: Path & '=' & a: Path & b: Name? & '!' => @(k + "=" + a + "|" + b)
			Start : @string = p: Pair & (';' & Pair)* & eof => @(p)

			parse Start
			{{publications}}
			""";
	}

	[Fact]
	public void A_find_puts_the_file_on_the_engine_and_the_unbounded_run_is_told()
	{
		var grammar = Names("*", "find Pair");
		var told    = Assert.Single(Told(grammar));

		Assert.Equal(
			"Rule 'Word' is read without its settled copy because publication 'FindPair' runs on the engine, " +
			"so a refusal inside a long run of 'Word' takes time quadratic in its length. " +
			"Read 'FindPair' by methods, or move it into its own grammar.",
			told.Message);

		Assert.Equal(GramSeverity.Warning, told.Severity);
		Assert.Equal(grammar.IndexOf("Word  =", StringComparison.Ordinal), told.Position);
	}

	[Fact]
	public void The_first_publication_in_source_order_that_is_not_read_by_methods_is_named()
	{
		var told = Assert.Single(Told(Names("*", "find Path\nfind Pair")));

		Assert.Contains("publication 'FindPath' runs", told.Message, StringComparison.Ordinal);
		Assert.Contains("Read 'FindPath' by methods", told.Message, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("+")]
	[InlineData("{2,}")]
	public void Any_run_with_no_upper_bound_is_told(string run)
	{
		Assert.Equal(["GRAM5022"], Told(Names(run, "find Pair")).Select(static one => one.Id));
	}

	[Theory]
	[InlineData("{0,8}")]
	[InlineData("{1,3}")]
	public void A_bounded_run_is_never_told(string run)
	{
		Assert.Empty(Told(Names(run, "find Pair")));
	}

	[Fact]
	public void Nothing_is_told_where_every_machine_is_a_reader_and_the_copies_are_kept()
	{
		var result = Compile(Names("*", ""), settle: true);

		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == "GRAM5022");
		Assert.Contains("_Settled(", Assert.Single(result.Sources).Text, StringComparison.Ordinal);
	}

	[Fact]
	public void Nothing_is_told_where_the_copies_are_not_asked_for()
	{
		Assert.DoesNotContain(Compile(Names("*", "find Pair"), settle: false).Diagnostics, static one => one.Id == "GRAM5022");
	}

	[Fact]
	public void Telling_it_changes_nothing_in_the_file()
	{
		var grammar = Names("*", "find Pair");
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
						if (diagnostic.Id == GramCompiler.SettledDropped)
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
