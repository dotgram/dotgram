using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Bindings, references and lengths (<c>$q: X</c>, <c>$q</c>, <c>X{#q}</c>) as the model holds
/// them, and every place one is refused, with where the refusal points (docs/syntax.md §3.5.1).
/// </summary>
/// <remarks>
/// Each refusal is asked through the generator driver, which is where an author meets it, and
/// is held to the character it marks. Each has a twin a step away that must be accepted, asked
/// of the grammar's own stages: what a parser built from it does is the business of the tests
/// that run one.
/// </remarks>
public sealed partial class BackReferenceTests
{
	// ── The model ───────────────────────────────────────────────────────────────

	[Fact]
	public void A_binding_a_reference_and_a_length_are_nodes_of_their_own()
	{
		var body = Body("R = $q: 'a'+ & 'b'{#q} & $q");

		var sequence = Assert.IsType<Node.Sequence>(body);
		var bind     = Assert.IsType<Node.Bind>(sequence.Nodes[0]);
		var counted  = Assert.IsType<Node.Counted>(sequence.Nodes[1]);
		var same     = Assert.IsType<Node.Same>(sequence.Nodes[2]);

		Assert.Equal("q", bind.Binding.Name);
		Assert.IsType<Node.Repeat>(bind.Body);
		Assert.Same(bind.Binding, same.Binding);
		Assert.Same(bind.Binding, Assert.IsType<CountBound.Length>(counted.Min).Binding);
		Assert.Equal(counted.Min, counted.Max);
		Assert.Equal("$q: 'a'+ & 'b'{#q} & $q", body.ToString());

		// The two operands that read the binding are its children; a reference has none.
		Assert.Equal([bind.Body], bind.Children);
		Assert.Equal([counted.Body], counted.Children);
		Assert.Empty(same.Children);
	}

	[Theory]
	[InlineData("'b'{#q,}",   "#q",  null)]
	[InlineData("'b'{2,#q}",  "2",   "#q")]
	[InlineData("'b'{#q,5}",  "#q",  "5")]
	[InlineData("'b'{#q,#r}", "#q",  "#r")]
	public void A_count_keeps_each_side_as_written(string count, string min, string? max)
	{
		var counted = Assert.IsType<Node.Counted>(
			Assert.IsType<Node.Sequence>(Body($"R = $q: 'a' & $r: 'c' & {count}")).Nodes[^1]);

		Assert.Equal(min, counted.Min.ToString());
		Assert.Equal(max, counted.Max?.ToString());
	}

	[Fact]
	public void A_count_named_by_a_value_parameter_is_the_number_the_call_passed()
	{
		var graph   = Normalize("R = P(3)\nP(n) = $q: 'a' & 'b'{n,#q}");
		var counted = graph.Bodies.Values.SelectMany(Walk).OfType<Node.Counted>().Single();

		Assert.Equal(new CountBound.Items(3), counted.Min);
		Assert.IsType<CountBound.Length>(counted.Max);
	}

	[Fact]
	public void Two_top_level_alternatives_bind_two_symbols()
	{
		var binds = Nodes<Node.Bind>(Body("R = $t: 'a' & $t | $t: 'b' & $t"));
		var reads = Nodes<Node.Same>(Body("R = $t: 'a' & $t | $t: 'b' & $t"));

		Assert.Equal(2, binds.Length);
		Assert.NotSame(binds[0].Binding, binds[1].Binding);

		var body = Body("R = $t: 'a' & $t | $t: 'b' & $t");

		foreach (var alternative in Assert.IsType<Node.Choice>(body).Nodes)
			Assert.Same(Nodes<Node.Bind>(alternative).Single().Binding, Nodes<Node.Same>(alternative).Single().Binding);

		Assert.Equal(2, reads.Length);
	}

	[Fact]
	public void Bindings_on_exclusive_routes_of_a_nested_choice_are_one_symbol()
	{
		var body  = Body("R = ('\\'' & $t: W & '\\'' | $t: W) & ' ' & $t\nW = ['a'..'z']+");
		var binds = Nodes<Node.Bind>(body);

		Assert.Equal(2, binds.Length);
		Assert.Same(binds[0].Binding, binds[1].Binding);
		Assert.Same(binds[0].Binding, Nodes<Node.Same>(body).Single().Binding);
	}

	[Theory]
	[InlineData("($q: 'a')*",      0, null)]
	[InlineData("($q: 'a')+",      1, null)]
	[InlineData("($q: 'a'){0,3}",  0, 3)]
	[InlineData("($q: 'a'){2}",    2, 2)]
	[InlineData("($q: V)*",        0, null)]
	public void A_repeated_binding_is_a_binding_of_the_run(string written, int min, int? max)
	{
		// Every repetition but the option: after no turns the binding reads "", which is what
		// makes the reference after it bound on every route.
		var body = Body($"R = {written} & $q\nV : @int = ['0'..'9'] => @(1)");
		var bind = Assert.IsType<Node.Bind>(Assert.IsType<Node.Sequence>(body).Nodes[0]);
		var run  = Assert.IsType<Node.Repeat>(bind.Body);

		Assert.Equal((min, max), (run.Min, run.Max));
		Assert.IsNotType<Node.Bind>(run.Body);
	}

	[Fact]
	public void An_optional_binding_stays_an_option()
	{
		var body = Body("R = ($q: 'a')? & 'b'");
		var run  = Assert.IsType<Node.Repeat>(Assert.IsType<Node.Sequence>(body).Nodes[0]);

		Assert.Equal((0, (int?)1), (run.Min, run.Max));
		Assert.IsType<Node.Bind>(run.Body);
	}

	[Fact]
	public void A_counted_binding_is_a_binding_of_the_count()
	{
		var body = Body("R = $p: 'n'* & ($q: 'a'){#p} & $q");
		var bind = Assert.IsType<Node.Bind>(Assert.IsType<Node.Sequence>(body).Nodes[1]);

		Assert.Equal("q", bind.Binding.Name);
		Assert.IsType<Node.Counted>(bind.Body);
	}

	[Fact]
	public void Each_specialization_binds_symbols_of_its_own()
	{
		var graph = Normalize("R = P('a') & P('b')\nP(x) = $q: x & $q");

		AssertOwnBindings(graph, expected: 2);
	}

	[Fact]
	public void A_rule_copied_by_a_rebinding_binds_symbols_of_its_own()
	{
		var graph = Normalize(
			"""
			R = $q: W & $q
			W = 'a'
			V = 'b'
			parse R
			parse R with (W = V) as RV
			""");

		AssertOwnBindings(graph, expected: 2);
	}

	[Fact]
	public void A_with_inside_a_rule_keeps_the_rule_s_symbols()
	{
		// The operand of the `with` is rebuilt in place, binding and all, and the reference
		// outside it still reads the same binding.
		var graph = Normalize(
			"""
			R = $q: W with (W = V) & $q
			W = 'a'
			V = 'b'
			parse R
			""");

		AssertOwnBindings(graph, expected: 1);
	}

	// ── Accepted ───────────────────────────────────────────────────────────────

	[Theory]
	[InlineData("R = ('\\'' & $t: W & '\\'' | $t: W) & ' ' & $t\nW = ['a'..'z']+")]
	[InlineData("R = $q: ('a'?) & 'b' & $q")]
	[InlineData("R = ($q: 'a')* & 'b' & $q")]
	[InlineData("R = ($q: 'a'){0,3} & 'b' & $q")]
	[InlineData("R = ($q: V)* & 'b' & $q\nV : @int = ['0'..'9'] => @(1)")]
	[InlineData("R = ($q: 'a' & $q & ',')*")]
	[InlineData("R = '[' & $eq: '='* & '[' & (?!(']' & $eq & ']') & any)* & ']' & $eq & ']'")]
	[InlineData("R = { $q: '\"'{3,} } & { (?!$q & any)* } & $q & ?!'\"'")]
	[InlineData("R = '`'{3} & $f: '`'* & (?!('`'{3} & $f) & any)* & '`'{#f,}")]
	[InlineData("R = \"<<\" & $tag: W & '\\n' & $tag | \"<<-\" & $tag: W & '\\t'* & $tag\nW = ['a'..'z']+")]
	[InlineData("R = $q: 'a' & 'b'{#q} & ?=$q")]
	[InlineData("R = q: ($q: 'a') & $q")]
	[InlineData("R = $q: W with (W = V) & $q\nW = 'a'\nV = 'b'")]
	[InlineData("R = P('a') & P('b')\nP(x) = $q: x & $q")]
	public void Accepted(string grammar)
	{
		Assert.Empty(Errors(grammar + "\nparse R"));
	}

	// ── Refused, and where ─────────────────────────────────────────────────────

	[Theory]
	[InlineData("R = 'a' & $q",                   "$q",  "No binding '$q' in this rule")]
	[InlineData("R = 'a'{#q}",                    "#q",  "No binding '$q' in this rule")]
	[InlineData("R = q: 'a' & $q",                "$q",  "q: ($q: …)")]
	[InlineData("R = 'a' & $W\nW = 'b'",          "$W",  "'W' is a rule")]
	public void A_name_no_binding_of_the_rule_has(string grammar, string at, string says)
	{
		Refused(GrammarBinder.UnknownBinding, grammar, at, says);
	}

	[Fact]
	public void A_binding_is_not_read_across_rules()
	{
		Refused(GrammarBinder.UnknownBinding, "R = $q: 'a' & S\nS = 'b' & $q\nparse S", "$q", "No binding '$q' in this rule", 1);
	}

	[Theory]
	[InlineData("R = ($q: 'a')? & $q",              "$q", 1, "'$q: (X?)'")]
	[InlineData("R = ($q: 'a'){0,1} & $q",          "$q", 1, "'$q: (X?)'")]
	[InlineData("R = ($q: 'a' & ',')* & $q",        "$q", 1, "Bind the whole run")]
	[InlineData("R = $q & $q: 'a'",                 "$q", 0, "to the right")]
	[InlineData("R = $q: 'a' | 'b' & $q",           "$q", 1, "another alternative")]
	[InlineData("R = ('x' & $q: 'a' | 'y') & $q",   "$q", 1, "on some route")]
	[InlineData("R = ('x' & $q: 'a' | 'y') & 'b'{#q}", "#q", 0, "on some route")]
	[InlineData("R = ?!($q: 'a') & 'a' & $q",       "$q", 1, "")]
	public void A_reference_some_route_reaches_unbound(string grammar, string at, int occurrence, string says)
	{
		Refused(GrammarNormalizer.UnboundReference, grammar, at, says, occurrence);
	}

	[Theory]
	[InlineData("R = ?=($q: 'a') & 'b'",                          "$q", 0, "inside a lookahead")]
	[InlineData("R = ?=($q: 'a' & $q) & 'b'",                     "$q", 0, "inside a lookahead")]
	[InlineData("R = $q: ('a' & $q)",                             "$q", 1, "inside its own binding")]
	[InlineData("R = $q: 'a'{#q}",                                "#q", 0, "inside its own binding")]
	[InlineData("R = $q: 'a' & $q: 'b' & $q",                     "$q", 1, "bound twice on one route")]
	[InlineData("R = $q: ('a' & $q: 'b')",                        "$q", 1, "bound twice on one route")]
	[InlineData("R = $q: 'a' & F($q)\nF(x) = x",                  "$q", 1, "in an argument of 'F'")]
	[InlineData("R : @string = $q: 'a' => $q",                    "$q", 1, "in the value of a '=>'")]
	[InlineData("R = $q: 'a' & when $q is 'a' & 'b'",            "$q", 1, "'when … is …'")]
	[InlineData("R = $q: 'a' & when $q & 'b'",                   "$q", 1, "in a 'when', which is C#")]
	[InlineData("E = E & '+' & $q: T & $q | T\nT = 'a'",          "$q", 0, "a step of the left-recursive rule 'E'")]
	[InlineData("R = $q: R & 'x' & $q | 'y'",                     "$q", 0, "the leading call of the left-recursive rule 'R'")]
	[InlineData("R = $q: R & 'x' | 'y'",                          "$q", 0, "the leading call of the left-recursive rule 'R'")]
	[InlineData("R = r: ($q: R) & 'x' & $q | 'y'",                "$q", 0, "the leading call of the left-recursive rule 'R'")]
	[InlineData("R = $q: { R } & 'x' | 'y'",                      "$q", 0, "the leading call of the left-recursive rule 'R'")]
	[InlineData("R = $q: (R | 'z') & 'x' & $q | 'y'",             "$q", 0, "the leading call of the left-recursive rule 'R'")]
	[InlineData("R : @string = $q: 'a' & ('b' & $q => @(\"g\")) => @(\"r\")", "$q", 1, "constructing group")]
	public void A_binding_or_a_reference_where_none_can_stand(string grammar, string at, int occurrence, string says)
	{
		Refused(GrammarNormalizer.MisplacedBinding, grammar, at, says, occurrence);
	}

	[Theory]
	[InlineData("R = $q: 'a' & ('b' & $q & ';')* recover ';'", "$q", 1)]
	[InlineData("R = $q: 'a' & ('b'){#q} recover ';'",         "(",  0)]
	[InlineData("R = ($q: 'a')* recover ';' & 'b'",            "(",  0)]
	[InlineData("R = $q: (('a')* recover ';') & $q",           "$q", 0)]
	[InlineData("R = $q: ('x' & ('a' & 'b')* recover ';') & 'c'", "$q", 0)]
	[InlineData("R = $q: S & $q\nS = ('a' & ';')* recover ';'",   "$q", 0)]
	[InlineData("R = $q: S & $q\nS = 'b' & U\nU = ('a' & ';')* recover ';'", "$q", 0)]
	[InlineData("R = $q: S & $q\nS = '(' & S* & ')' | 'b' & U\nU = ('a' & ';')* recover ';'", "$q", 0)]
	public void A_binding_in_a_repetition_that_recovers_is_not_supported_yet(string grammar, string at, int occurrence)
	{
		Refused(GrammarNormalizer.BindingNotSupported, grammar, at, "recover", occurrence);
	}

	[Fact]
	public void A_binding_in_a_terminal_of_a_grammar_cut_in_two_is_not_supported_yet()
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Quoted = $q: '"'+ & ['a'..'z']* & $q
				Word   = ['a'..'z']+
			}
			R = Lexical.Quoted & Lexical.Word
			""";

		Refused(GrammarNormalizer.BindingNotSupported, grammar, "$q", "leave Lexical off", lexical: true);
	}

	[Fact]
	public void A_binding_over_kinds_is_refused_with_a_guard_to_write_instead()
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Letters = ['a'..'z']+
			}
			R = $w: Lexical.Letters & '=' & $w
			""";

		Refused(GrammarNormalizer.BindingOverKinds, grammar, "$w", "when @(close == open)", lexical: true);
	}

	[Fact]
	public void A_grammar_with_a_binding_cut_in_two_only_where_asked()
	{
		// The same grammar over characters, as everywhere a split was not asked for.
		Assert.Empty(Errors(
			"""
			R = $w: Word & '=' & $w
			Word = ['a'..'z']+
			trivia = { ' '* }
			parse R
			"""));
	}

	[Fact]
	public void A_reader_form_asked_for_over_a_binding_is_not_supported_yet()
	{
		const string grammar = "R = $q: 'a'+ & 'b' & $q\nparse R stream";

		var told = GramCompiler.Compile(grammar, new GramCompilerOptions { ClassName = "Grammar" }).Diagnostics;

		var refused = Assert.Single(told, static one => one.Id == GrammarNormalizer.BindingNotSupported);

		Assert.Equal(grammar.IndexOf("parse R stream", StringComparison.Ordinal), refused.Position);
	}

	[Theory]
	[InlineData("trivia = { $q: (' '*) & $q }\nR = 'a'\nparse R")]
	[InlineData("trivia = { $q: (' '*) & $q }\nR = 'a' & 'b'\nparse R")]
	[InlineData("trivia = { $q: (' '*) & $q }\nR = 'a' & 'b' & 'c'\nparse R")]
	[InlineData("trivia = { $q: (' '*) & $q }\nR = S\nS = 'a'\nparse R")]
	[InlineData("trivia = { T }\nT = $q: (' '*) & $q\nR = 'a'\nparse R")]
	public void A_reader_form_over_trivia_that_binds_is_not_supported_yet(string grammar)
	{
		// The binding is in the trivia rather than in a rule the publication calls: woven between
		// operands and read at the two ends of a parse, it is as much a part of the reading.
		var told = GramCompiler.Compile(
			grammar, new GramCompilerOptions { ClassName = "Grammar", BufferedInput = true }).Diagnostics;

		var refused = Assert.Single(told, static one => one.Id == GrammarNormalizer.BindingNotSupported);

		Assert.Equal(grammar.IndexOf("parse R", StringComparison.Ordinal), refused.Position);
		Assert.DoesNotContain(told, static one => one.Severity == GramSeverity.Error && one.Id != GrammarNormalizer.BindingNotSupported);
	}

	[Fact]
	public void A_recursive_rule_a_binding_calls_without_recovering_is_accepted()
	{
		Assert.Empty(Errors("R = $q: S & $q\nS = '(' & S* & ')' | 'b'\nparse R"));
		Assert.Empty(Errors("R = $q: S & $q\nS = 'b'\nT = ('a' & ';')* recover ';'\nparse R\nparse T"));
	}

	[Fact]
	public void A_count_by_a_length_over_what_can_be_empty_is_refused()
	{
		// Bounded or not, as `('b'?){1,3}` is: the same count is no loop at all over a body that
		// cannot be empty.
		Assert.Contains(GrammarNormalizer.NullableRepetition, Errors("R = $q: 'a' & ('b'?){#q,}\nparse R"));
		Assert.Contains(GrammarNormalizer.NullableRepetition, Errors("R = $q: 'a' & ('b'?){1,#q}\nparse R"));
		Assert.Contains(GrammarNormalizer.NullableRepetition, Errors("R = $q: 'a' & ('b'?){#q}\nparse R"));
		Assert.Contains(GrammarNormalizer.NullableRepetition, Errors("R = $q: 'a' & (\"bc\"?){#q,3}\nparse R"));
		Assert.Contains(GrammarNormalizer.NullableRepetition, Errors("R = $q: 'a' & $r: 'c' & ('b'?){#q,#r}\nparse R"));
		Assert.Contains(GrammarNormalizer.NullableRepetition, Errors("R = ':' & ('b'?){1,3}\nparse R"));
		Assert.DoesNotContain(GrammarNormalizer.NullableRepetition, Errors("R = $q: 'a' & 'b'{1,#q}\nparse R"));
	}

	[Fact]
	public void Left_recursion_through_a_binding_is_reported()
	{
		Assert.Contains(GrammarNormalizer.LeftRecursion, Errors("R = $q: S & 'x' | 'y'\nS = R & 'z'\nparse R"));
		Assert.DoesNotContain(GrammarNormalizer.LeftRecursion, Errors("R = $q: R & 'x' & $q | 'y'\nparse R"));
		Assert.DoesNotContain(GrammarNormalizer.LeftRecursion, Errors("R = $q: { R } & 'x' | 'y'\nparse R"));
	}

	// ── Helpers ─────────────────────────────────────────────────────────────────

	static RecognitionGraph Normalize(string source)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(source, RoslynCSharpScanner.Instance)).File));
	}

	/// <summary>The body of the first rule, normalized.</summary>
	static Node Body(string source)
	{
		var graph = Normalize(source);

		Assert.DoesNotContain(graph.Diagnostics, static one => one.Severity == GramSeverity.Error);

		return graph.Bodies[graph.Rules.First(static rule => rule.Name == "R")];
	}

	static T[] Nodes<T>(Node node)
		where T : Node
	{
		return [.. Walk(node).OfType<T>()];
	}

	/// <summary>
	/// Every rule holding a binding reads only its own: as many distinct symbols as expected, and
	/// no reference or length in one rule reads another's.
	/// </summary>
	static void AssertOwnBindings(RecognitionGraph graph, int expected)
	{
		Assert.DoesNotContain(graph.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var symbols = new HashSet<TextBinding>();

		foreach (var body in graph.Bodies.Values)
		{
			var bound = new HashSet<TextBinding>(Nodes<Node.Bind>(body).Select(static one => one.Binding));

			foreach (var node in Walk(body))
				foreach (var binding in Read(node))
					Assert.Contains(binding, bound);

			symbols.UnionWith(bound);
		}

		Assert.Equal(expected, symbols.Count);
	}

	static IEnumerable<Node> Walk(Node node)
	{
		yield return node;

		foreach (var child in node.Children)
			foreach (var inner in Walk(child))
				yield return inner;
	}

	/// <summary>The bindings a reference or a count reads.</summary>
	static IEnumerable<TextBinding> Read(Node node)
	{
		switch (node)
		{
			case Node.Same(var binding, _):
				yield return binding;
				break;

			case Node.Counted(_, var min, var max):
				if (min is CountBound.Length(var low, _))
					yield return low;

				if (max is CountBound.Length(var high, _))
					yield return high;

				break;
		}
	}

	/// <summary>What every stage up to normalization refuses, as identifiers.</summary>
	static string[] Errors(string grammar)
	{
		var parsed = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance);
		var model  = GrammarBinder.Bind(parsed.File, own: grammar.Length);
		var graph  = GrammarNormalizer.Normalize(model, scanner: RoslynCSharpScanner.Instance);

		return
		[
			.. parsed.Diagnostics.Concat(model.Diagnostics).Concat(graph.Diagnostics)
				.Where(static one => one.Severity == GramSeverity.Error)
				.Select(static one => one.Id),
		];
	}

	/// <summary>
	/// Refused through the generator, with <paramref name="id"/> on the <paramref name="occurrence"/>th
	/// <paramref name="at"/> of the grammar, saying <paramref name="says"/>.
	/// </summary>
	static void Refused(string id, string grammar, string at, string says, int occurrence = 0, bool lexical = false)
	{
		var text = grammar + "\nparse R";
		var host = lexical
			? "[DotGram.Gram(Lexical = true)] public partial class Host;"
			: "[DotGram.Gram] public partial class Host;";
		var run  = GeneratorDriverTests.RunGenerator(host, ("/proj/Host.gram", text));

		var position = -1;

		for (var i = 0; i <= occurrence; i++)
			position = text.IndexOf(at, position + 1, StringComparison.Ordinal);

		var found = run.Diagnostics.Where(one => one.Id == id).ToList();

		Assert.True(
			found.Count > 0,
			$"{id} was not reported. Told: {string.Join("; ", run.Diagnostics.Select(static one => one.Id + " " + one.GetMessage()))}");

		var diagnostic = found.FirstOrDefault(one => one.Location.SourceSpan.Start == position);

		Assert.True(
			diagnostic is not null,
			$"{id} not at {position}: {string.Join("; ", found.Select(static one => one.Location.SourceSpan + " " + one.GetMessage()))}");

		Assert.Equal(DiagnosticSeverity.Error, diagnostic!.Severity);
		Assert.Contains(says, diagnostic.GetMessage(), StringComparison.Ordinal);
	}
}
