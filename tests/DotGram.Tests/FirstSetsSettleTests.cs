using System;
using System.Linq;
using System.Text;
using System.Threading;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a rule can begin with is worked out to the end on every grammar, however its rules lead
/// with one another: where it settles, as it always has; where it would go round, cut off.
/// </summary>
/// <remarks>
/// <para>
/// The answer is a fixed point, reached by putting each rule's new answer in place of the one
/// before, and the step that answers does not only add: a part of a sequence that must consume
/// but begins with nothing is passed over, and before a rule is reached its answer is nothing.
/// Round a ring of rules that lead with one another, <c>R0 = R1</c>, <c>R1 = R3 | 'b'</c>,
/// <c>R3 = R0 &amp; 'a'</c>, the <c>a</c> of <c>R3</c> came while <c>R0</c> was nothing and went
/// once it was something, and came back once that was taken out of <c>R0</c>, for ever. It runs
/// inside the generator, so the build never finished.
/// </para>
/// <para>
/// Such a grammar is refused — it is left recursion no rewrite takes — but only once the
/// normalizer has finished, and the normalizer asks what rules begin with on the way. So the loop
/// is bounded by how often one rule's answer may change, from the height of the answers the
/// grammar can have, and past it the grammar is refused (GRAM4057, or what refuses it already).
/// Every grammar that settled before settles the same.
/// </para>
/// </remarks>
public sealed class FirstSetsSettleTests
{
	/// <summary>Long enough for any of these on a loaded machine; each takes milliseconds.</summary>
	static readonly TimeSpan Limit = TimeSpan.FromSeconds(30);

	/// <summary>
	/// The ring alone, then grammars a generator of random recursive grammars found the compiler
	/// never finishing on: each left recursion no rewrite takes, with more around it.
	/// </summary>
	public static TheoryData<string, string> Rings { get; } = new()
	{
		{
			"the ring alone",
			"""
			R0 = R1
			R1 = R3 | 'b'
			R3 = R0 & 'a'
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 01",
			"""
			R0 = R1
			R1 = (R2 & R0 & (when @(true) & (("" | 'c'))*))
			R2 = (R3)+
			R3 = ((((R0){0,2}){0,2})* & '(')+
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 02",
			"""
			R0 = ($q0: "a"i & ("" | 'b' | (($q1: 'c' & 'c' & $q1))*) & $q0)
			R1 = ((R2)* & ')')+
			R2 = ('a'? & R3 & ("" | (('(')* & ')')+ | "a"i) | 'a')
			R3 = ('b'? & R1 & ((('b')* & ['a'..'b'])+){2} | 'a')
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 03",
			"""
			R0 = (('(' & R0 & ')' & 'b' | '(' & R2 & ')' & ')'))*
			R1 = ("ab"? & R3 & R2 | 'a')
			R2 = switch @(0) { case 0: R4 case 1: ?= ($q0: "a"i & ('b'? & R0 & 'a' | ')') & $q0) }
			R3 = R4
			R4 = R1
			P(item) = ('(' & R1 & ')' & "ab" | '(' & R0 & ')' & item)
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 04",
			"""
			R0 = (when @(true) & (?! ([^ 'a' | ')'] & 'c') & ((R1)* & 'b')+ & (when @(true) & ((R1)* & [^ 'a' | ')'])+)))
			R1 = ((('('? & R2 & ("a"i & Ns.E) | ['a'..'b']))* & ')')+
			R2 = (('b'? & R0 & ?= { 'b' } | ')')){2}
			namespace Ns with (R1 = R2)
			{
				E = Ns.E
			}
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 05",
			"""
			trivia = { ' '* }
			namespace Token { trivia = none
			 Id = ['a'..'z']+ }
			R0 = ?= ((switch @(0) { case 0: '(' case 1: R1 } | "")){1,3}
			R1 = R3
			R2 = R0
			R3 = ?! ("as" & "") & R4
			R4 = ("as"? & P(Token.Id) & (when @(true) & (('(')* & ')')+) | Token.Id)
			P(item) = R1
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 06",
			"""
			trivia = { ' '* }
			namespace Token { trivia = none
			 Id = ['a'..'z']+ }
			R0 = R1
			R1 = (R3 | ('(' & R2 & ')' & ')' | '(' & R0 & ')' & ',') | R2)
			R2 = "as"
			R3 = (R0 & ?= P("cast") & (')'? & R2 & switch @(0) { case 0: ('(' | R3 | ')') case 1: (when @(true) & R3) } | Token.Id))
			P(item) = R0
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 07",
			"""
			R0 = (when @(true) & R3)
			R1 = ('b'? & R4 & (($q0: "ab" & 'c' & $q0) | "" | R1 | ((['a'..'b'])* & 'b')+) | "ab")
			R2 = ?! ('(' & R4 & ')' & 'b' | '(' & R3 & ')' & 'b') & R1
			R3 = (when @(true) & { ('(' | { R4 }) })
			R4 = (when @(true) & (((R0 & 'b' & R4) & '(') & ?= switch @(0) { case 0: R2 case 1: 'a' }))
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 08",
			"""
			R0 = ((R0 | (['a'..'b']? & R0 & ?= 'c' | 'c')))?
			R1 = (((("ab" | R2) | R2 | ['a'..'b'] | R2))* & "a"i)+
			R2 = ?! ('b' & 'c') & (('(' & R3 & ')' & 'a' | '(' & R1 & ')' & 'a') | ((R0 | "a"i | '(' | 'b') & P(R3) & ($q0: ')' & "" & $q0)) | (when @(true) & { '(' }))
			R3 = (R3 & ?! ('b' & "a"i) & 'b')
			P(item) = (['a'..'b']? & R1 & { (item | item | ')') } | 'c')
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 09",
			"""
			trivia = { ' '* }
			namespace Token { trivia = none
			 Id = ['a'..'z']+ }
			R0 = R1
			R1 = ((R2 & ('(' & R1 & ')' & "cast" | '(' & R0 & ')' & "as"))){1,3}
			R2 = ("as"? & R0 & ((switch @(0) { case 0: ')' case 1: "as" } | (')' & '(' & ',')) | (?= R3)? | ((',' | "") & R2) | (($q0: "cast" & ',' & $q0) & R1 & ("" | Token.Id | ')' | R3))) | "cast")
			R3 = ($q1: Token.Id & ?= R3 & $q1)
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 10",
			"""
			trivia = { ' '* }
			namespace Token { trivia = none
			 Id = ['a'..'z']+ }
			R0 = ("cast"? & R1 & ?! ("as" & '(') & (?= R2 & ($q0: '(' & ')' & $q0)) | ',')
			R1 = (R3 & "as" & ('(' & R3 & ')' & Token.Id | '(' & R0 & ')' & ','))
			R2 = { ('(' & R3 & ')' & "as" | '(' & R2 & ')' & ',') }
			R3 = ((when @(true) & R0) & ?! ("as" & ',') & (Token.Id)* & ((("cast" & ')') | "" | ?= ')'))+)
			P(item) = item
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 11",
			"""
			R0 = (R1)*
			R1 = ?! (P(R0) | ($q0: 'a' & ('b'? & R0 & [^ 'a' | ')'] | "ab") & $q0) | ([^ 'a' | ')']? & R0 & (P('b') | 'a' | "" | P('b')) | [^ 'a' | ')']) | ((R2 | 'b' | ['a'..'b']) & ['a'..'b'] & R1)) & ("ab"? & R3 & switch @(0) { case 0: { ['a'..'b'] } case 1: R1 } | 'a')
			R2 = (when @(true) & ((('c')* & ')')+)?)
			R3 = ((?! ($q1: ')' & 'b' & $q1) & R0)* & ['a'..'b'])+
			P(item) = (((item? & R0 & "ab" | item))+ & '(' & R1)
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 12",
			"""
			trivia = { ' '* }
			namespace Token { trivia = none
			 Id = ['a'..'z']+ }
			R0 = { (?= (P(R1) | Token.Id | "cast" | '(') & R1 & ($q0: ')' & ("as")+ & $q0)) }
			R1 = ("cast"? & R3 & ('(' & R3 & ')' & Token.Id | '(' & R4 & ')' & ',') | "as")
			R2 = ?! { R2 } & (switch @(0) { case 0: ('(' & "cast") case 1: ?! "cast" & R1 } | (R3 | { "cast" } | ')' | P(R4)))
			R3 = (','? & R0 & ((when @(true) & (R2)*) & (((')')* & ')')+)?) | '(')
			R4 = (R3 & Token.Id)
			P(item) = R3
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 13",
			"""
			R0 = R4
			R1 = ($q0: 'c' & ([^ 'a' | ')']? & R2 & ?= '(' | '(') & $q0)
			R2 = (R3){1,3}
			R3 = (('b'? & R4 & ('c' | ('b'? & R0 & "a"i | [^ 'a' | ')']) | ?= R1 | ?! '(' & P("a"i)) | ['a'..'b']) | { ("ab"? & P('c') & '(' | "ab") } | ((('(' & R0 & ')' & 'c' | '(' & P("") & ')' & 'b') | (when @(true) & 'b') | (when @(true) & "ab")) | ("" | { ['a'..'b'] } | "ab")) | ?= ({ 'a' } & (R2 & R4 & P(R0)) & ((R2)* & '(')+))
			R4 = ('(' & P('b') & ')' & 'a' | '(' & R1 & ')' & ')')
			P(item) = R2
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 14",
			"""
			R0 = ((('(' & P(['a'..'b']) & ')' & "" | '(' & R0 & ')' & "ab") | ('(' & R3 & ')' & '(' | '(' & P(R3) & ')' & [^ 'a' | ')']) | ?! (')' & '(') & ($q0: [^ 'a' | ')'] & 'a' & $q0)) & ($q1: "a"i & (when @(true) & { 'b' }) & $q1) & (($q2: "a"i & switch @(0) { case 0: P('a') case 1: [^ 'a' | ')'] } & $q2) & (?! ('b' & 'c') & P(R2) & ($q3: ['a'..'b'] & R2 & $q3))))
			R1 = R3
			R2 = P('a')
			R3 = { ('a'? & R2 & (R0){1,3} | 'a') }
			P(item) = ?! (item & item) & ('c'? & R1 & (item){0,2} | ')')
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 15",
			"""
			trivia = { ' '* }
			namespace Token { trivia = none
			 Id = ['a'..'z']+ }
			R0 = (("cast"? & R1 & ((('(')* & "cast")+ & ("as"){2} & ("as" | ')')) | "as") | "cast" | (($q0: Token.Id & ('(' & R1 & ')' & "as" | '(' & R1 & ')' & '(') & $q0)){1,3} | ("cast" & ?= ('('? & P(',') & "" | Token.Id)))
			R1 = ('('? & R2 & switch @(0) { case 0: (')' | R1) case 1: switch @(0) { case 0: ('(' & R1 & ')' & ',' | '(' & R0 & ')' & Token.Id) case 1: "as" } } | '(')
			R2 = R0
			P(item) = { ?= (R0 | "cast" | item | R0) }
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 16",
			"""
			R0 = (((')'? & R1 & R2 | [^ 'a' | ')']))* & ')')+
			R1 = (R3 & (switch @(0) { case 0: ("ab" | "a"i | [^ 'a' | ')'] | 'b') case 1: ('a' | R3 | "ab") }){0,2} & (when @(true) & switch @(0) { case 0: ?= "ab" case 1: ('c'? & R2 & 'c' | 'c') }))
			R2 = ((({ "ab" } | (when @(true) & '(')))*)*
			R3 = ((({ R0 } | ?! '(' & ['a'..'b'] | { "ab" }))* & '(')+
			R4 = ?! switch @(0) { case 0: (when @(true) & ?! ['a'..'b'] & ')') case 1: R4 } & ((("ab"? & R4 & 'a' | ['a'..'b']))? & [^ 'a' | ')'])
			P(item) = (((R1)* & "ab")+)*
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 17",
			"""
			R0 = (("a"i? & R2 & R0 | ')') & (when @(true) & (($q0: '(' & R4 & $q0) | ([^ 'a' | ')'] & "a"i & 'b') | R2)))
			R1 = ((((R3 & Ns.E & 'b') & (when @(true) & 'a')) | "") & ([^ 'a' | ')'] | ('(' & R1 & ')' & "ab" | '(' & Ns.E & ')' & "a"i) | R2 | ""))
			R2 = R2
			R3 = (Ns.E | "" | ((Ns.E & "ab") & (("ab")* & "a"i)+))
			R4 = { (("" | "") & ?! (['a'..'b'] & ['a'..'b']) & ?! ([^ 'a' | ')'] & 'b') & "a"i) }
			P(item) = (when @(true) & ((R3)* & 'c')+)
			namespace Ns with (R1 = R0)
			{
				E = (('c'? & P(R2) & ('(')* | ""))*
			}
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 18",
			"""
			R0 = R3
			R1 = ('b'? & R0 & (($q0: 'b' & (R0 & "ab") & $q0) | "" | ("a"i | ("" | ['a'..'b'])) | R4) | ['a'..'b'])
			R2 = ($q1: ['a'..'b'] & R1 & $q1)
			R3 = ((?! ('(' & ['a'..'b']) & R4 | ?! (R3 | R1 | R1 | "") & ?! ("ab" & ['a'..'b']) & ')' | "") & ?= (("" | "" | R0 | 'b') & (P(')')){0,2} & "ab") & ['a'..'b'])
			R4 = (((R0 | "") | ('b'? & R1 & ('(' & R4 & ')' & '(' | '(' & R1 & ')' & '(') | ['a'..'b']) | ('(' & R4 & ')' & "a"i | '(' & P(R4) & ')' & ['a'..'b'])) | { { '(' } } | R1 | (((when @(true) & ['a'..'b']) & ('(' | ""))){0,2})
			P(item) = R0
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 19",
			"""
			trivia = { ' '* }
			namespace Token { trivia = none
			 Id = ['a'..'z']+ }
			R0 = (R3 & switch @(0) { case 0: switch @(0) { case 0: ((R4)* & Token.Id)+ case 1: (when @(true) & ',') } case 1: ((',')* & Token.Id)+ })
			R1 = switch @(0) { case 0: (')'? & Ns.E & { R0 } | Token.Id) case 1: ',' }
			R2 = ?! ('(' & Token.Id) & ((?= R0 | ("cast"? & R2 & "as" | ',') | (Token.Id & ')' & Token.Id) | ($q0: "cast" & '(' & $q0)) & ',')
			R3 = ((Ns.E & ($q1: '(' & ($q2: ',' & "as" & $q2) & $q1)) & "as" & R4)
			R4 = ((when @(true) & (switch @(0) { case 0: R4 case 1: "cast" })*))?
			P(item) = (?= R0 & R0)
			namespace Ns with (R2 = R0)
			{
				E = R0
			}
			Start = R0 & eof
			parse Start
			"""
		},
		{
			"found 20",
			"""
			R0 = ?! ([^ 'a' | ')'] & "a"i) & ')'
			R1 = (R2 & ((?= R3 & (R3)+) & { ('(' & R4 & ')' & ['a'..'b'] | '(' & R2 & ')' & 'a') } & (R1 | "")))
			R2 = { { (("" | R4 | ')' | "ab") & R4 & 'c') } }
			R3 = ((switch @(0) { case 0: ((P(R1))* & "")+ case 1: (when @(true) & 'b') } & (("a"i? & R4 & R2 | ['a'..'b']) & ?= 'b' & ('(' & R1 & ')' & 'a' | '(' & R1 & ')' & 'c'))) | "a"i | { ('(' & R1 & ')' & ['a'..'b'] | '(' & R1 & ')' & [^ 'a' | ')']) })
			R4 = (R1 & (((('c')* & "a"i)+ | "" | ($q0: "" & 'b' & $q0) | ('c' & P(R4))) & R4 & (Ns.E & "a"i & ('b' | "" | "" | ""))) & ?= (P(R0) | (P(R0) | R4 | Ns.E) | ($q1: "" & 'a' & $q1) | R1))
			P(item) = (("" | (item? & R0 & R2 | ')') | switch @(0) { case 0: 'c' case 1: '(' } | "") | ((item | 'b' | 'c' | R0) & ')' & ('b' & R1 & item)))
			namespace Ns with (R1 = R2)
			{
				E = ?! ('(' & 'b') & switch @(0) { case 0: switch @(0) { case 0: P(R0) case 1: 'b' } case 1: (['a'..'b']){2} }
			}
			Start = R0 & eof
			parse Start
			"""
		},
	};

	[Theory]
	[MemberData(nameof(Rings))]
	public void A_ring_of_rules_that_lead_with_one_another_settles(string name, string grammar)
	{
		var result = Compile(grammar, lexical: grammar.StartsWith("trivia", StringComparison.Ordinal));

		Assert.True(result is not null, $"{name}: the compiler did not finish in {Limit.TotalSeconds} s");
		Assert.Contains(result.Diagnostics, static one => one.Id == "GRAM4002");
		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == "GRAM4057");
	}

	/// <summary>
	/// The ring alone goes round, and is cut off at the bound: the loop is told so, and the
	/// grammar is refused for its left recursion, quickly.
	/// </summary>
	[Fact]
	public void The_ring_is_cut_off_at_the_bound()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();
		var graph = Within(static () => Normalize("R0 = R1\nR1 = R3 | 'b'\nR3 = R0 & 'a'\nStart = R0 & eof\nparse Start\n"));

		Assert.True(graph is not null, $"the normalizer did not finish in {Limit.TotalSeconds} s");
		Assert.NotNull(FirstSets.Unsettled(graph));
		Assert.Contains(graph.Diagnostics, static one => one.Id == "GRAM4002");
		Assert.True(watch.Elapsed.TotalSeconds < 5, $"{watch.Elapsed.TotalSeconds:F1} s");
	}

	/// <summary>The same ring led into through an optional, and through a choice of two calls.</summary>
	[Theory]
	[InlineData("R0 = R1\nR1 = R3 | 'b'\nR3 = 'x'? & R0 & 'a'\nStart = R0 & eof\nparse Start\n")]
	[InlineData("R0 = R1\nR1 = R3 | 'b'\nR3 = (R0 | R0 & 'c') & 'a'\nStart = R0 & eof\nparse Start\n")]
	public void The_ring_settles_however_it_is_led_into(string grammar)
	{
		var result = Compile(grammar, lexical: false);

		Assert.True(result is not null, $"the compiler did not finish in {Limit.TotalSeconds} s");
		Assert.Contains(result.Diagnostics, static one => one.Id == "GRAM4002");
	}

	/// <summary>
	/// A ring through a look over a rule that never matches, written in either order: <c>R1</c>
	/// begins where <c>R0</c> does, with <c>a</c>, and its <c>(</c> never comes first. The answer
	/// may not depend on which rule was worked out first.
	/// </summary>
	[Theory]
	[InlineData("D = ?! 'a' & 'a'\nR0 = ?! D & 'a' & R1?\nR1 = R0 & '('\nStart = R0 & eof\nparse Start\n")]
	[InlineData("D = ?! 'a' & 'a'\nR1 = R0 & '('\nR0 = ?! D & 'a' & R1?\nStart = R0 & eof\nparse Start\n")]
	public void A_rule_begins_as_what_it_leads_with_whichever_comes_first(string grammar)
	{
		var graph = Within(() => Normalize(grammar));

		Assert.True(graph is not null, $"the normalizer did not finish in {Limit.TotalSeconds} s");

		foreach (var name in new[] { "R0", "R1" })
			Assert.Equal([new CharRange('a', 'a')], FirstSets.Of(new Node.Call(Rule(graph, name), []), graph).Ranges);
	}

	/// <summary>
	/// Settled, every rule begins with what its body begins with, read the way everything after
	/// settling reads it: no more, kept from a reading that was taken back, and no less. Over the
	/// random rings and over grammars dense in looks and in rules that never match, may match
	/// nothing or read one character, with fixed seeds; those cut off are left out.
	/// </summary>
	[Fact]
	public void Every_rule_begins_as_its_body_does()
	{
		var random  = new Random(20261111);
		var checks  = 0;

		for (var i = 0; i < 600; i++)
		{
			var grammar = i % 2 == 0 ? Ring(random) : Dense(random);
			var graph   = Within(() => Normalize(grammar));

			Assert.True(graph is not null, $"the normalizer did not finish in {Limit.TotalSeconds} s on\n{grammar}");

			// Cut off, the answers are joined rather than worked out.
			if (FirstSets.Unsettled(graph) is not null)
				continue;

			foreach (var rule in graph.Rules)
			{
				if (!graph.Bodies.TryGetValue(rule, out var body))
					continue;

				var settled = FirstSets.Of(new Node.Call(rule, []), graph);
				var read    = FirstSets.Of(body, graph);

				Assert.True(FirstSets.Same(settled, read), $"{rule.Name} begins with more or less than its body on\n{grammar}");

				checks++;
			}
		}

		Assert.True(checks > 500, $"only {checks} rules were checked");
	}

	/// <summary>
	/// A repetition that recovers over a rule that never matches steps over what is there and
	/// succeeds, so it is not a part that never matches: <c>Rows</c> begins with the dot it
	/// synchronizes on, and the optional dot before it gives its dot back. Taken as a part that
	/// never matches, it stopped the sequence, and <c>..</c> was refused.
	/// </summary>
	[Theory]
	[InlineData("'.'?", false)]
	[InlineData("'.'?", true)]
	[InlineData("'.'*", false)]
	[InlineData("'.'*", true)]
	public void A_repetition_that_recovers_is_not_a_part_that_never_matches(string lead, bool direct)
	{
		var grammar  = $"D = ?! 'a' & 'a'\nRows = D+ recover '.' & '.'\nStart = {lead} & Rows & eof\nparse Start\n";
		var compiled = Within(() => GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Direct        = direct,
		}));

		Assert.True(compiled is not null, $"the compiler did not finish in {Limit.TotalSeconds} s");

		EmittedCode.Quiet(compiled.Diagnostics);

		var assembly = EmittedCode.Compile(Assert.Single(compiled.Sources).Text);

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "..").IsSuccess);
		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseStart", "...").IsSuccess);
	}

	/// <summary>
	/// A repetition that recovers after a part that consumes: what the sequence begins with is
	/// what that part does, and the reader forms are not offered for the grammar (GRAM5001,
	/// GRAM5002), as before.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_repetition_that_recovers_after_a_part_keeps_its_forms(bool direct)
	{
		const string Grammar = "D = ?! 'a' & 'a'\nItem : @int = '\\n' => @(1)\nStart : @int[] = 'x' & Item* & D{1} recover eol & '\\n' & eof\nparse Start\n";

		var compiled = Within(() => GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Direct        = direct,
		}));

		Assert.True(compiled is not null, $"the compiler did not finish in {Limit.TotalSeconds} s");
		Assert.Contains(compiled.Diagnostics, static one => one.Id == "GRAM5001");
		Assert.Contains(compiled.Diagnostics, static one => one.Id == "GRAM5002");
		Assert.DoesNotContain(compiled.Sources, static source => source.Text.Contains("ParseStart(global::System.IO.TextReader", StringComparison.Ordinal));
	}

	/// <summary>
	/// Over random grammars with repetitions that recover, with a fixed seed: the loop finishes, and
	/// every rule that settled begins with what its body begins with.
	/// </summary>
	[Fact]
	public void Random_grammars_that_recover_settle()
	{
		var random = new Random(20261113);
		var checks = 0;

		for (var i = 0; i < 300; i++)
		{
			var lines = Dense(random, recovering: true).Split('\n', StringSplitOptions.RemoveEmptyEntries);
			var text  = string.Join("\n", lines);
			var graph = Within(() => Normalize(text));

			Assert.True(graph is not null, $"the normalizer did not finish in {Limit.TotalSeconds} s on\n{text}");

			if (FirstSets.Unsettled(graph) is not null)
				continue;

			foreach (var rule in graph.Rules)
			{
				if (!graph.Bodies.TryGetValue(rule, out var body))
					continue;

				Assert.True(
					FirstSets.Same(FirstSets.Of(new Node.Call(rule, []), graph), FirstSets.Of(body, graph)),
					$"{rule.Name} does not begin as its body does on\n{text}");

				checks++;
			}
		}

		Assert.True(checks > 500, $"only {checks} rules were checked");
	}

	/// <summary>
	/// Small grammars dense in rules that lead with one another — through an optional, a
	/// choice, a look — with a fixed seed: the compiler finishes on every one.
	/// </summary>
	[Fact]
	public void Random_rings_settle()
	{
		var random = new Random(20261110);
		var seen   = 0;

		for (var i = 0; i < 300; i++)
		{
			var grammar = Ring(random);
			var result  = Compile(grammar, lexical: false);

			Assert.True(result is not null, $"the compiler did not finish in {Limit.TotalSeconds} s on\n{grammar}");

			if (result.Diagnostics.Any(static one => one.Id == "GRAM4002"))
				seen++;
		}

		// Most of them are refused as left recursion, which is the shape this is about.
		Assert.True(seen > 100, $"only {seen} of 300 were left recursion");
	}

	/// <summary>A few rules, each a choice whose alternatives mostly lead with a call.</summary>
	static string Ring(Random random)
	{
		var rules = random.Next(2, 5);
		var text  = new StringBuilder();

		for (var rule = 0; rule < rules; rule++)
		{
			var alternatives = new string[random.Next(1, 4)];

			for (var i = 0; i < alternatives.Length; i++)
			{
				var parts = new string[random.Next(1, 4)];

				for (var j = 0; j < parts.Length; j++)
					parts[j] = Part(random, rules, j == 0);

				alternatives[i] = string.Join(" & ", parts);
			}

			text.Append($"R{rule} = ").AppendLine(string.Join(" | ", alternatives));
		}

		text.AppendLine("Start = R0 & eof");
		text.AppendLine("parse Start");

		return text.ToString();
	}

	static string Part(Random random, int rules, bool leading)
	{
		var call = $"R{random.Next(rules)}";

		return random.Next(leading ? 10 : 6) switch
		{
			0     => "'a'",
			1     => "'b'",
			2     => "\"ab\"",
			3     => $"'x'? & {call}",
			4     => $"({call} | 'c')",
			5     => $"?! 'a' & {call}",
			6     => $"?= {call} & {call}",
			_     => call,
		};
	}

	/// <summary>
	/// A few rules over a rule that never matches (<c>D</c>), one that reads one character
	/// (<c>K</c>) and one that may match nothing (<c>N</c>), led into through looks over them.
	/// </summary>
	static string Dense(Random random, bool recovering = false)
	{
		var rules = random.Next(2, 5);
		var text  = new StringBuilder();

		for (var rule = 0; rule < rules; rule++)
		{
			var alternatives = new string[random.Next(1, 4)];

			for (var i = 0; i < alternatives.Length; i++)
			{
				var parts = new string[random.Next(1, 4)];

				for (var j = 0; j < parts.Length; j++)
					parts[j] = Leading(random, rules, recovering);

				alternatives[i] = string.Join(" & ", parts);
			}

			text.Append($"R{rule} = ").AppendLine(string.Join(" | ", alternatives));
		}

		text.AppendLine("D = ?! 'a' & 'a'");
		text.AppendLine("K = 'a' | 'b'");
		text.AppendLine("N = 'c'?");
		text.AppendLine("Start = R0 & eof");
		text.AppendLine("parse Start");

		return text.ToString();
	}

	static string Leading(Random random, int rules, bool recovering)
	{
		var call = random.Next(6) switch
		{
			0 => "D",
			1 => "K",
			2 => "N",
			_ => $"R{random.Next(rules)}",
		};

		return random.Next(recovering ? 13 : 11) switch
		{
			11    => $"({call})+ recover '.'",
			12    => $"({call})* recover ')'",
			0     => "'a'",
			1     => "'('",
			2     => "\"\"",
			3     => $"?! {call}",
			4     => $"?= {call}",
			5     => $"{call}?",
			6     => $"({call} | 'b')",
			7     => $"({call})+",
			_     => call,
		};
	}

	static RecognitionGraph Normalize(string grammar)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File!),
			scanner: RoslynCSharpScanner.Instance);
	}

	static RuleSymbol Rule(RecognitionGraph graph, string name)
	{
		return graph.Rules.Single(rule => rule.Name == name);
	}

	/// <summary>The compilation, or null where it did not finish in time.</summary>
	static GramCompilation? Compile(string grammar, bool lexical)
	{
		return Within(() => GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Lexical       = lexical,
			CSharpScanner = RoslynCSharpScanner.Instance,
		}));
	}

	/// <summary>
	/// What the work makes, or null where it did not finish in time. On a thread of its own, so
	/// that a compiler going round for ever fails the test rather than holding the run.
	/// </summary>
	static T? Within<T>(Func<T> work)
		where T : class
	{
		T? result = null;

		var worker = new Thread(() => result = work(), 16 * 1024 * 1024)
		{
			IsBackground = true,
		};

		worker.Start();

		return worker.Join(Limit) ? result : null;
	}
}
