using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The analysis of repetitions that can read one run of text in two ways: what it tells, with
/// what witness, and what it is silent about and why.
/// </summary>
/// <remarks>
/// Asked of the analysis directly, over the graph the compiler would build, and not through a
/// diagnostic: which grammars it tells is held here first, its words later.
/// </remarks>
public sealed class ExponentialRepetitionTests
{
	[Theory]
	[InlineData("R = (('a' | 'a' & 'a')+)* & 'b'", "'a' 'a'")]
	[InlineData("R = ('x'+)* & 'b'",                "'x' 'x'")]
	[InlineData("R = ('x'+)+ & 'b'",                "'x' 'x'")]
	[InlineData("R = (\"xy\" | \"xy\")* & 'b'",     "'x' 'y'")]
	[InlineData("R = ('a' | 'a' & 'a')+ & 'b'",     "'a' 'a'")]

	// Through a call: what a rule reads is read where it is called.
	[InlineData("X = 'x'+\nR = X* & 'b'", "'x' 'x'")]

	// A bounded repetition inside is written out turn by turn, and still cuts two ways.
	[InlineData("R = ('x'{1,2})* & 'b'", "'x' 'x'")]

	// A choice of two that can both read nothing is two ways to read nothing, between two letters.
	[InlineData("R = ('a' & ('b'? | 'c'?) & 'd')* & ';'", "'a' 'd'")]
	public void A_repetition_that_cuts_two_ways_is_told(string rules, string witness)
	{
		var told = Assert.Single(Analyse(rules + "\nparse R\n").Found);

		Assert.Equal("R", told.Rule.Name);
		Assert.Equal(witness, told.Witness);
	}

	/// <summary>
	/// Folding white space optional on both sides of a repeated word: the space between two
	/// words is the end of one turn or the start of the next.
	/// </summary>
	[Fact]
	public void Optional_space_on_both_sides_of_a_repeated_word_is_told()
	{
		var told = Assert.Single(Analyse(
			"Cfws = ' '+\n" +
			"W = { ['a'..'z']+ }\n" +
			"R = (Cfws? & W & Cfws?)+ & ';'\n" +
			"parse R\n").Found);

		Assert.Equal("R", told.Rule.Name);
		Assert.Equal("W ' ' W", told.Witness);
	}

	/// <summary>A literal run of characters repeated inside a repetition: the URI template's shape before it was braced.</summary>
	[Fact]
	public void The_unbraced_template_literal_run_is_told()
	{
		var told = Assert.Single(Analyse(
			"LiteralChar = ['a'..'z' | '0'..'9']\n" +
			"Pct = '%' & ['0'..'9'] & ['0'..'9']\n" +
			"Literals = (LiteralChar | Pct)+\n" +
			"Part = Literals | '{' & ['a'..'z']+ & '}'\n" +
			"R = Part* & eof\n" +
			"parse R\n").Found);

		Assert.Equal("R", told.Rule.Name);
	}

	/// <summary>Only the innermost of a nest is told: the one around it is ambiguous because of it.</summary>
	[Fact]
	public void The_innermost_of_a_nest_is_told()
	{
		var told = Assert.Single(Analyse("R = (('a' | 'a' & 'a')+)* & 'b'\nparse R\n").Found);

		Assert.Equal("('a' | \"aa\")+", told.Loop.ToString());
	}

	[Theory]

	// Braces commit what a turn, or the whole run, read.
	[InlineData("R = { ('x'+)* } & 'b'")]
	[InlineData("R = ({ 'x'+ })* & 'b'")]
	[InlineData("R = { ('a' | 'a' & 'a')+ } & 'b'")]

	// Alternatives that begin differently.
	[InlineData("R = (',' & ['a'..'z'] | ';' & ['0'..'9'])* & eof")]
	[InlineData("R = ['a'..'z'] & (',' & ['a'..'z']+)* & eof")]

	// Two repetitions one after the other cut a run in as many ways as it is long, which is
	// polynomial: another diagnostic's business.
	[InlineData("R = 'a'* & 'a'* & 'b'")]

	// A run pinned at the end of each turn.
	[InlineData("R = ('_'* & ['0'..'9'])* & eof")]

	// Alternatives of one character each are one class by the time the graph is built.
	[InlineData("R = ('x' | 'x')* & 'b'")]
	[InlineData("R = (['a'..'z'] | ['a'..'f'])* & ';'")]
	public void A_repetition_that_cuts_one_way_is_not_told(string rules)
	{
		Assert.Empty(Analyse(rules + "\nparse R\n").Found);
	}

	/// <summary>
	/// A branch that can never be finished reads its beginning a second way, but never gets to
	/// the next turn by it, so a run is still cut one way.
	/// </summary>
	[Fact]
	public void A_branch_that_cannot_be_finished_is_not_a_second_reading()
	{
		Assert.Empty(Analyse("R = ('x' & 'y' | 'x' & 'y' & ['b'..'a'])* & 'b'\nparse R\n").Found);
	}

	/// <summary>
	/// A nest whose outer turns can never be finished: the outer is trimmed to nothing, and the
	/// inner one, whose own turns can, is told by itself. A refusal of it is exponential all the
	/// same, since the backtracking tries every reading before it learns the turn cannot end.
	/// </summary>
	[Fact]
	public void The_inner_repetition_of_a_dead_turn_is_told_by_itself()
	{
		var told = Assert.Single(Analyse("R = ((\"xy\" | \"xy\")+ & ['b'..'a'] | 'z')* & eof\nparse R\n").Found);

		Assert.Equal("(\"xy\" | \"xy\")+", told.Loop.ToString());
	}

	/// <summary>The trivia woven between operands is GRAM4051's: it reads nothing here.</summary>
	[Fact]
	public void A_trivia_seam_is_not_told()
	{
		var result = Analyse(
			"trivia = ' '*\n" +
			"W = ['a'..'z']+\n" +
			"R = (W & ',')+ & eof\n" +
			"parse R\n");

		Assert.Empty(result.Found);
	}

	const string Split =
		"trivia = { ' '* }\n" +
		"namespace Token { trivia = none\n Id = ['a'..'z']+ }\n";

	/// <summary>Over kinds a rule marked <c>?</c> gives back what it read, and is asked.</summary>
	[Fact]
	public void A_rule_that_gives_back_is_told_over_kinds()
	{
		var told = Assert.Single(Analyse(
			Split +
			"R? = (Token.Id | Token.Id & Token.Id)+ & ';'\n" +
			"Start = R & eof\n" +
			"parse Start\n",
			lexical: true).Found);

		Assert.Equal("R", told.Rule.Name);
	}

	/// <summary>Over kinds a rule not marked <c>?</c> answers once: its repetitions are as if braced.</summary>
	[Fact]
	public void A_rule_that_answers_once_is_not_told_over_kinds()
	{
		Assert.Empty(Analyse(
			Split +
			"R = (Token.Id | Token.Id & Token.Id)+ & ';'\n" +
			"Start = R & eof\n" +
			"parse Start\n",
			lexical: true).Found);
	}

	/// <summary>
	/// The same shape in a recursive rule over kinds, which a reader remembers the failure of at
	/// a token: it answers once anyway, and nothing is told.
	/// </summary>
	[Fact]
	public void A_memoised_recursive_rule_over_kinds_is_not_told()
	{
		Assert.Empty(Analyse(
			Split +
			"R = (Token.Id | Token.Id & Token.Id)+ & (';' | '(' & R & ')')\n" +
			"Start = R & eof\n" +
			"parse Start\n",
			lexical: true).Found);
	}

	/// <summary>The lexer's terminals are read by its automaton and give nothing back.</summary>
	[Fact]
	public void A_lexer_terminal_is_not_told()
	{
		Assert.Empty(Analyse(
			"trivia = { ' '* }\n" +
			"namespace Token { trivia = none\n Id = (['a'..'z']+)+ }\n" +
			"Start = Token.Id+ & eof\n" +
			"parse Start\n",
			lexical: true).Found);
	}

	/// <summary>
	/// A miss, pinned: a call to a recursive rule is one opaque letter, which overlaps only
	/// itself, so a turn that reads the same text through it and through a character is not
	/// seen. Not told is not proved safe.
	/// </summary>
	[Fact]
	public void A_recursive_call_reading_the_same_text_is_a_miss()
	{
		var result = Analyse(
			"X = 'a' | '(' & X & ')'\n" +
			"R = (X | 'a')+ & 'b'\n" +
			"parse R\n");

		Assert.Empty(result.Found);
		Assert.True(result.Opaque > 0);
	}

	/// <summary>A call past the inlining budget is opaque and counted, and the repetition is still asked.</summary>
	[Fact]
	public void A_call_past_the_inlining_budget_is_opaque()
	{
		var big = "Big = " + string.Join(" | ", Enumerable.Range(0, 300).Select(static i => $"\"k{i}\"")) + "\n";

		var result = Analyse(big + "R = (Big | ',')* & eof\nparse R\n");

		Assert.Empty(result.Found);
		Assert.True(result.Opaque > 0);
	}

	/// <summary>The work done grows with the grammar, and is counted for a test to hold it to.</summary>
	[Fact]
	public void The_work_is_counted()
	{
		var graph = Graph("R = ('a' | 'a' & 'a')+ & 'b'\nparse R\n", lexical: false, out var lexical);

		ExponentialRepetition.Check(graph, lexical, out var steps);

		Assert.True(steps > 0);
	}

	/// <summary>
	/// Every grammar in the repository, asked: what is told, where, with what witness, and how
	/// much of each grammar the analysis could not see.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A report and not yet a guard: it writes <c>ExponentialRepetitions.txt</c> beside the
	/// assembly and to the output, and asserts nothing. A hit is either a grammar that a refused
	/// input can drive exponentially, which is a defect of that grammar, or a shape the analysis
	/// takes for one, which is a defect of the analysis; each is judged by driving it.
	/// </para>
	/// <para>
	/// The grammars are the files, every raw-string <c>[Gram]</c> attribute, and each read the
	/// way it is compiled: over kinds where some attribute or snapshot rendering asks for
	/// <c>Lexical</c>, over characters where some use does not.
	/// </para>
	/// </remarks>
	[Fact]
	public void Corpus()
	{
		var root   = SourceRoot.Directory;
		var report = new StringBuilder();
		var hits   = 0;
		var read   = 0;

		foreach (var (name, text, modes) in Grammars(root))
		{
			foreach (var lexical in modes)
			{
				RecognitionGraph graph;
				LexicalSplit?    split;
				long             built;

				var watch = Stopwatch.StartNew();

				try
				{
					graph = Built(name, text, lexical, out split);
					built = watch.ElapsedTicks;
				}
				catch (Exception error)
				{
					report.AppendLine($"{name} [{(lexical ? "kinds" : "chars")}]: not read ({error.GetType().Name}: {error.Message.Split('\n')[0]})");

					continue;
				}

				watch.Restart();

				var result  = ExponentialRepetition.Check(graph, split, out var steps);
				var checking = watch.ElapsedTicks;

				read++;
				hits += result.Found.Count;

				var mode = lexical ? split is null ? "kinds asked, read over chars" : "kinds" : "chars";

				report.AppendLine(string.Create(
					System.Globalization.CultureInfo.InvariantCulture,
					$"{name} [{mode}]: {result.Loops} loops, {result.Skipped} past the budget, {result.Opaque} opaque letters, " +
					$"{result.Found.Count} told; {steps} steps, {Milliseconds(checking):F1} ms against {Milliseconds(built):F1} ms to build the graph"));

				foreach (var told in result.Found)
				{
					var line = told.Rule.Declaration is { } declaration
						? 1 + text.Take(Math.Min(declaration.At.Position, text.Length)).Count(static c => c == '\n')
						: 0;

					report.AppendLine($"  HIT {told.Rule.Name} (line {line}): {Shortened(told.Loop.ToString())} reads {told.Witness} in two ways");
				}
			}
		}

		report.Insert(0, $"{read} grammars read, {hits} hits.\n");

		var written = report.ToString();

		File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "ExponentialRepetitions.txt"), written);
		TestContext.Current.SendDiagnosticMessage(written);
		Console.WriteLine(written);
	}

	static double Milliseconds(long ticks)
	{
		return ticks * 1000.0 / Stopwatch.Frequency;
	}

	static string Shortened(string text)
	{
		const int most = 100;

		return text.Length <= most ? text : text.Substring(0, most) + " …";
	}

	/// <summary>The graph a grammar compiles to; the library itself is read as it is, with nothing spliced onto it.</summary>
	static RecognitionGraph Built(string name, string text, bool lexical, out LexicalSplit? split)
	{
		var parsed = name == "Std.gram"
			? DotGram.Grammar.Parsing.GramParser.Parse(DotGram.Grammar.Parsing.GramLexer.Tokenize(text, RoslynCSharpScanner.Instance))
			: GramCompiler.Read(text, RoslynCSharpScanner.Instance);

		var model = GrammarBinder.Bind(parsed.File, null, text.Length);
		var graph = GrammarNormalizer.Normalize(model, scanner: RoslynCSharpScanner.Instance);

		split = lexical ? LexicalSplit.Of(graph) : null;

		return graph;
	}

	/// <summary>
	/// Every grammar in the repository, with the ways it is read: over characters, over kinds, or both.
	/// </summary>
	static IEnumerable<(string Name, string Text, bool[] Modes)> Grammars(string root)
	{
		var attribute = new Regex(
			"\\[(?:DotGram\\.)?Gram\\((?:\"\"\"\\r?\\n(?<text>.*?)\\r?\\n\\s*\"\"\"|\"(?<file>[^\"]+\\.gram)\")(?<rest>[^\\]]*)\\]",
			RegexOptions.Singleline);

		// A file's modes by the attributes that name it: those beside it, where there are any, since
		// that is its host; a test that writes the name in a string of its own is not.
		var files  = new Dictionary<string, HashSet<bool>>(StringComparer.Ordinal);
		var beside = new Dictionary<string, HashSet<bool>>(StringComparer.Ordinal);
		var inline = new List<(string, string, bool[])>();

		foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
		{
			if (Scratch(root, path))
				continue;

			var source = File.ReadAllText(path);

			foreach (Match match in attribute.Matches(source))
			{
				var lexical = Regex.IsMatch(match.Groups["rest"].Value, "Lexical\\s*=\\s*true");

				if (match.Groups["file"].Success)
				{
					var file = Path.GetFileName(match.Groups["file"].Value);
					var near = File.Exists(Path.Combine(Path.GetDirectoryName(path)!, file));
					var table = near ? beside : files;

					if (!table.TryGetValue(file, out var modes))
						table[file] = modes = [];

					modes.Add(lexical);

					continue;
				}

				inline.Add((
					Path.GetFileName(path) + ":" + (1 + source.Take(match.Index).Count(static c => c == '\n')),
					string.Join("\n", match.Groups["text"].Value.Split('\n').Select(static one => one.TrimStart('\t'))),
					[lexical]));
			}
		}

		foreach (var path in Directory.EnumerateFiles(root, "*.gram", SearchOption.AllDirectories))
		{
			if (Scratch(root, path))
				continue;

			var name = Path.GetFileName(path);
			var text = File.ReadAllText(path);

			var modes = beside.TryGetValue(name, out var hosted) ? [.. hosted] :
				files.TryGetValue(name, out var used) ? [.. used] : new HashSet<bool>();

			if (Path.GetFileName(Path.GetDirectoryName(path)) == "Snapshots")
				foreach (var (options, _) in SnapshotRenderings.Of(Path.GetFileNameWithoutExtension(path), text, EmittedCode.References))
					modes.Add(options.Lexical);

			if (modes.Count == 0)
				modes.Add(false);

			yield return (name, text, [.. modes.OrderBy(static one => one)]);
		}

		foreach (var one in inline)
			yield return one;
	}

	/// <summary>Build output and scratch, judged below the root as <c>ExponentialShapeProbe</c> judges it.</summary>
	static bool Scratch(string root, string path)
	{
		var below = path.Substring(root.Length);

		return
			below.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
			below.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
			below.Contains($"{Path.DirectorySeparatorChar}.work{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
			below.Contains($"{Path.DirectorySeparatorChar}.build{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
	}

	internal static ExponentialRepetition.Result Analyse(string grammar, bool lexical = false)
	{
		var graph = Graph(grammar, lexical, out var split);

		return ExponentialRepetition.Check(graph, split);
	}

	/// <summary>The graph the compiler builds, and the grammar cut in two where that was asked for and done.</summary>
	internal static RecognitionGraph Graph(string grammar, bool lexical, out LexicalSplit? split)
	{
		var parsed = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance);

		Assert.DoesNotContain(parsed.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var model = GrammarBinder.Bind(parsed.File, null, grammar.Length);
		var graph = GrammarNormalizer.Normalize(model, scanner: RoslynCSharpScanner.Instance);

		Assert.DoesNotContain(graph.Diagnostics, static one => one.Severity == GramSeverity.Error);

		split = lexical ? LexicalSplit.Of(graph) : null;

		if (lexical)
			Assert.NotNull(split);

		return graph;
	}
}
