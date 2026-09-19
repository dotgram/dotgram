using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The seam §4.5 reads at every gap, and what the generator may assume about it: that its run
/// need not give back to a seam after it.
/// </summary>
public sealed class SeamTests
{
	const string Spaced = "trivia = [' ' | '\\t']*\nItem : @string = 'b' => @(\"b\")\n";

	/// <summary>
	/// A run of the seam before the end, or before another seam, has nothing to give: every
	/// split of it ends where it would. So a grammar spaced by §4.5 is read without a way into
	/// its trivia, and the immediate carrier takes it.
	/// </summary>
	[Fact]
	public void A_seam_run_gives_nothing_to_the_seam_after_it() =>
		Assert.Contains("carrier: immediate; gate: none",
			Carriers(Spaced + "File : @string[] = '[' & (Item & ';')* & eof\nparse File\n"));

	/// <summary>
	/// Unless something between the two seams answers by where it stands: a guard there passes
	/// or fails by which split was taken, so the run keeps its way.
	/// </summary>
	[Fact]
	public void A_guard_between_two_seams_keeps_the_way() =>
		Assert.Contains("again trivia: opens a way",
			Carriers(Spaced + "Checked = when @(Ok())\nFile : @string[] = '[' & (Item & Checked & ';')* & eof\nparse File\n"));

	/// <summary>And where what follows the seam can itself begin with what the seam reads.</summary>
	[Fact]
	public void What_can_begin_inside_the_seam_keeps_the_way() =>
		Assert.Contains("again trivia: opens a way",
			Carriers(Spaced + "File : @string[] = '[' & (Item & v: [^ ';']+ & ';')* & eof\nparse File\n"));

	/// <summary>
	/// A turn that is a choice of alternatives each led by the seam — what an operator loop and
	/// left recursion come to — is compared past the seam, as a turn led by it is.
	/// </summary>
	[Fact]
	public void A_turn_of_seam_led_alternatives_is_compared_past_the_seam()
	{
		var graph = GrammarNormalizer.Normalize(GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(
			"trivia = [' ' | '\\t']*\nNum : @int = ['0'..'9']+ => @(1)\n" +
			"Sum : @int = l: Sum & '+' & r: Num => @(l + r) | l: Sum & '-' & r: Num => @(l - r) | n: Num => @(n)\n" +
			"parse Sum\n",
			RoslynCSharpScanner.Instance)).File));

		var sum  = graph.Bodies.Keys.Single(rule => rule.Name == "Sum");
		var loop = Within(graph.Bodies[sum]).OfType<Node.Repeat>().Single();

		Assert.IsType<Node.Choice>(loop.Body);
		Assert.True(Determinism.NeverGivesBack(loop, FollowSets.Of(graph)[sum], graph, FollowSets.SeamOf(sum, graph)));
	}

	/// <summary>
	/// A turn that begins with the seam that follows the repetition begins past it instead, and
	/// reads it last: the same reading, with the seam no longer read at a turn's head and again
	/// after the turn is given back. A seam written as an optional of a star is the same seam.
	/// </summary>
	[Theory]
	[InlineData("", "File : @string[] = (Item & ';')* & eof", "trivia & (item0: Item & trivia & ';' & trivia)* & eof")]
	[InlineData("", "File : @string[] = '[' & (Item & ';')* & eof", "'[' & trivia & (item0: Item & trivia & ';' & trivia)* & eof")]
	[InlineData("Spacing = [' ' | '\\t']+\n", "File : @string[] = (Item & ';')* & eof", "trivia & (item0: Item & trivia & ';' & trivia)* & eof")]
	public void The_seam_leaves_the_head_of_the_turn(string before, string rule, string body) =>
		Assert.Equal(body, Body(
			(before.Length > 0 ? before + "trivia = Spacing?\nItem : @string = 'b' => @(\"b\")\n" : Spaced) + rule + "\nparse File\n",
			"File"));

	/// <summary>
	/// Not where what the turn reads could begin inside the seam: trivia with a comment in it
	/// stops anywhere inside the comment.
	/// </summary>
	[Fact]
	public void A_seam_something_can_begin_inside_stays_at_the_head_of_the_turn() =>
		Assert.Equal(
			"(trivia & item0: Item & trivia & ';')* & trivia & eof",
			Body(
				"trivia = (' ' | '/' & '*' & (?!'*' & any)* & '*')*\nItem : @string = 'b' => @(\"b\")\n" +
				"File : @string[] = (Item & ';')* & eof\nparse File\n",
				"File"));

	/// <summary>
	/// What the rewritten grammar answers, on every carrier and rendering: the same values, and
	/// a refusal in the same place.
	/// </summary>
	[Theory]
	[InlineData("[ b ; b;  ")]
	[InlineData("[b;b;")]
	[InlineData("[")]
	[InlineData("[ b ; x")]
	[InlineData("[b;;")]
	[InlineData("[ b b ;")]
	public void The_answers_are_the_same_on_every_carrier(string input)
	{
		const string Grammar = Spaced + "File : @string[] = '[' & (Item & ';')* & eof\nparse File\n";

		var answers = new[] { (CarrierKind.Tape, true), (CarrierKind.Immediate, true), (CarrierKind.Tape, false) }
			.Select(one =>
			{
				var compilation = GramCompiler.Compile(Grammar, new GramCompilerOptions
				{
					ClassName = "Grammar", Carrier = one.Item1, Direct = one.Item2, CSharpScanner = RoslynCSharpScanner.Instance,
				});
				EmittedCode.Quiet(compilation.Diagnostics);
				var match = EmittedCode.Match(EmittedCode.Compile(Assert.Single(compilation.Sources).Text), "Grammar", "TryParseFile", input);
				return (match.IsSuccess, Value: match.IsSuccess ? string.Join(",", (string[])match.Value!) : "", match.Position);
			})
			.ToList();

		Assert.All(answers, answer => Assert.Equal(answers[0], answer));
		Assert.Equal(input.Count(c => c == ';') == input.Count(c => c == 'b') && !input.Contains('x'), answers[0].IsSuccess);
	}

	static string Body(string grammar, string rule)
	{
		var graph = GrammarNormalizer.Normalize(
			GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File));

		return graph.Bodies.Single(one => one.Key.Name == rule).Value.ToString()!.Replace(" => <sequence>", "");
	}

	static System.Collections.Generic.IEnumerable<Node> Within(Node node) =>
		node.Children.SelectMany(Within).Prepend(node);

	static string[] Carriers(string grammar)
	{
		var compilation = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, ReportCarriers = true,
		});

		return [.. compilation.Carriers!];
	}
}
