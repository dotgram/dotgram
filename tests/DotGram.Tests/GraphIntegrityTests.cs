using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// That a normalized grammar still says what it says about itself.
/// </summary>
/// <remarks>
/// <para>
/// Three things a grammar records are keyed by a node's identity: which repetition
/// recovers, which call parses at which strength, which construction may refuse. Every
/// one of them is written down during normalization, and normalization then goes on
/// rewriting nodes — so an entry can end up keyed by a node that is no longer in the
/// tree. Nothing then reads it, nothing complains, and the feature it stood for simply
/// stops happening.
/// </para>
/// <para>
/// That is a bug with no symptom at the point it is made, which is why it is asked about
/// here rather than left to be noticed. It has been made once: sequence results (§4.1
/// case 2) began rewriting repetitions, and every <c>recover</c> under one silently
/// stopped recovering. The test that caught it was a test of recovery; this one catches
/// the next such rewrite whatever it touches.
/// </para>
/// </remarks>
public sealed class GraphIntegrityTests
{
	[Theory]
	[MemberData(nameof(Grammars))]
	public void Nothing_the_graph_knows_is_about_a_node_that_is_gone(string path) =>
		Assert.Empty(Normalized(File.ReadAllText(path)).Orphans());

	/// <summary>
	/// Every grammar checked into the repository: the snapshots and the examples.
	/// </summary>
	/// <remarks>
	/// These are the grammars written to be read rather than to make a point, so they are
	/// the ones that use several features at once — which is where a rewrite reaches a
	/// node that something else had already spoken for.
	/// </remarks>
	public static TheoryData<string> Grammars
	{
		get
		{
			var data = new TheoryData<string>();

			foreach (var directory in new[] { Snapshots, Examples })
				foreach (var file in Directory.GetFiles(directory, "*.gram", SearchOption.AllDirectories))
					data.Add(file);

			return data;
		}
	}

	[Fact]
	public void A_recovering_repetition_survives_a_sequence_result()
	{
		// The rewrite that once orphaned a `recover`: the sequence gets a result type, so
		// its operands are turned into captures and the repetition is rebuilt around them.
		var graph = Normalized(
			"""
			Feed    : @Item[] = Header & Row* recover eol & Trailer & eof
			Header  : @Item   = 'H' & eol               => @(new Head())
			Row     : @Item   = name: ['a'..'z']+ & eol => @(new Line(name))
			Trailer : @Item   = 'T' & eol               => @(new Tail())
			""");

		Assert.Empty(graph.Diagnostics);
		Assert.NotEmpty(graph.Recoveries);
		Assert.Empty(graph.Orphans());
	}

	[Fact]
	public void A_recovering_repetition_inside_a_group_survives_being_collected()
	{
		// The rewrite that reaches inside a group (§4.1 case 2) rebuilds the repetition it
		// finds there, and `recover` is keyed by which node — so this is the same failure
		// as the first one, one level deeper, and would be as silent.
		var graph = Normalized(
			"""
			Feed : @Item[] = Header & (Row* recover eol & Trailer) & eof
			Header  : @Item = 'H' & eol               => @(new Head())
			Row     : @Item = name: ['a'..'z']+ & eol => @(new Line(name))
			Trailer : @Item = 'T' & eol               => @(new Tail())
			""");

		Assert.Empty(graph.Diagnostics);
		Assert.NotEmpty(graph.Recoveries);
		Assert.Empty(graph.Orphans());
	}

	[Fact]
	public void And_so_does_one_in_a_rule_that_hands_its_operand_back()
	{
		// §4.1 case 3 rewrites every alternative of the rule, which is a third place a
		// repetition can be rebuilt under a recovery that was recorded elsewhere.
		var graph = Normalized(
			"""
			Start : Feed = Feed
			Feed : @Item[] = Row* recover eol & eof
			Row  : @Item   = name: ['a'..'z']+ & eol => @(new Line(name))
			""");

		Assert.Empty(graph.Diagnostics);
		Assert.NotEmpty(graph.Recoveries);
		Assert.Empty(graph.Orphans());
	}

	[Fact]
	public void Binding_powers_survive_normalization()
	{
		var graph = Normalized(
			"""
			Start : @int = left: Start & '+' & right: Start << 1 => @(left + right)
			             | left: Start & '*' & right: Start << 2 => @(left * right)
			             | '-' & operand: Start              >> 4 => @(-operand)
			             | '(' & inner: Start & ')'               => @(inner)
			             | digits: ['0'..'9']+                    => @int.Parse(digits)
			""");

		Assert.Empty(graph.Diagnostics);
		Assert.NotEmpty(graph.Powers);
		Assert.Empty(graph.Orphans());
	}

	[Fact]
	public void An_orphan_is_actually_reported()
	{
		// Guarding the guard: a check that can only ever pass is not a check. The graph is
		// told about a node that was never in it, and has to say so.
		var graph = Normalized("Digits = ['0'..'9']+");

		var orphaned = new RecognitionGraph(
			graph.Rules,
			graph.Bodies,
			graph.Nullable,
			graph.Results,
			graph.Types,
			graph.CSharpImports,
			graph.Publications,
			graph.Diagnostics)
		{
			Powers = new Dictionary<Node, int> { [new Node.Literal("elsewhere")] = 1 },
		};

		Assert.Contains("binding power", Assert.Single(orphaned.Orphans()));
	}

	/// <remarks>
	/// With the real scanner: without one an inline <c>@(...)</c> is not read as C# and
	/// the grammar around it comes out as something nobody wrote.
	/// </remarks>
	static RecognitionGraph Normalized(string text) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));

	static string Snapshots => Path.Combine(Here, "..", "Snapshots");
	static string Examples  => Path.Combine(Here, "..", "..", "examples");

	// Declared before what it reads: static initializers run in source order, and one
	// that reads a later field reads the field's default rather than its value.
	static string ThisFile { get; } = FilePath();

	static string Here { get; } = Path.GetDirectoryName(ThisFile)!;

	static string FilePath([CallerFilePath] string path = "") => path;
}
