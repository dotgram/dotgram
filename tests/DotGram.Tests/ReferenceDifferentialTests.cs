using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The generated automaton against the executable semantics, on grammars nobody wrote.
/// </summary>
/// <remarks>
/// <para>
/// Every optimization the emitter makes — predicted dispatch, literal runs, silent
/// loops, standing exits, the seam pair — claims to preserve §11. This is the test of
/// the claim as a whole rather than of any one of them: a random grammar, compiled and
/// run against <see cref="ReferenceInterpreter"/> on random inputs, must agree on every
/// verdict. A disagreement is a defect by construction, and the failure message carries
/// the grammar and the input, ready for <see cref="Shrink"/>.
/// </para>
/// <para>
/// The generator stays inside the interpreter's fence — no guards, no externals, no
/// recursion — and inside the engine's own gates: grammars that draw an error diagnostic
/// are skipped, since both sides need something to run. Half the grammars get spacing
/// trivia, because the seam machinery is where the newest proofs live.
/// </para>
/// <para>
/// Seeded, so a failure is a failure again tomorrow, and the seed is in the message.
/// </para>
/// </remarks>
public sealed class ReferenceDifferentialTests
{
	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	public void The_automaton_agrees_with_the_semantics(int seed)
	{
		var random   = new Random(seed * 7919);
		var compiled = 0;

		while (compiled < 40)
		{
			var grammar = Generate(random);
			var result  = GramCompiler.Compile(
				grammar,
				new GramCompilerOptions
				{
					ClassName = "Grammar",
					CSharpScanner = RoslynCSharpScanner.Instance,
				});

			if (result.Diagnostics.Any(diagnostic => diagnostic.Severity == GramSeverity.Error))
				continue;

			compiled++;

			var assembly = EmittedCode.Compile(result.Sources[0].Text);
			var graph    = Normalized(grammar);
			var start    = graph.Rules.First(rule => rule.Name == "Start");

			for (var round = 0; round < 30; round++)
			{
				var input = Input(random);

				bool engine;

				try
				{
					engine = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).IsSuccess;
				}
				catch (Exception thrown)
				{
					// A parser that throws is as wrong as one that answers wrongly, and the
					// grammar is what the reader needs to see.
					Assert.Fail(
						$"seed {seed}: engine threw {thrown.GetType().Name}: {thrown.Message}\n" +
						$"input: \"{input}\"\ngrammar:\n{grammar}");

					return;
				}

				var oracle = ReferenceInterpreter.Parses(graph, start, input);

				Assert.True(
					engine == oracle,
					$"seed {seed}: engine says {engine}, semantics say {oracle}\n" +
					$"input: \"{input}\"\ngrammar:\n{grammar}");
			}
		}
	}

	/// <summary>
	/// The same, over grammars whose start rule binds a text and reads it again: by reference,
	/// inside a lookahead, as the bound of a count, and as the "until" scan, with a greedy or
	/// choosing body that has to give items back for the reference to match.
	/// </summary>
	[Theory]
	[InlineData(1, true)]
	[InlineData(2, true)]
	[InlineData(3, true)]
	[InlineData(1, false)]
	[InlineData(2, false)]
	[InlineData(3, false)]
	public void The_automaton_agrees_with_the_semantics_over_bindings(int seed, bool direct)
	{
		if (direct)
			BackReferenceEmission.RequireReader();
		else
		{
			BackReferenceEmission.RequireReader();
			BackReferenceEmission.RequireEngine();
		}

		var random   = new Random(seed * 104729);
		var compiled = 0;

		while (compiled < 30)
		{
			var grammar = GenerateBound(random);

			if (Refused(grammar))
				continue;

			var result = GramCompiler.Compile(
				grammar,
				new GramCompilerOptions
				{
					ClassName     = "Grammar",
					CSharpScanner = RoslynCSharpScanner.Instance,
					Direct        = direct,
				});

			Assert.DoesNotContain(result.Diagnostics, diagnostic => diagnostic.Severity == GramSeverity.Error);

			compiled++;

			var assembly = EmittedCode.Compile(result.Sources[0].Text);
			var graph    = Normalized(grammar);
			var start    = graph.Rules.First(rule => rule.Name == "Start");

			for (var round = 0; round < 30; round++)
			{
				var input  = Input(random);
				var oracle = ReferenceInterpreter.Parses(graph, start, input);

				bool engine;

				try
				{
					engine = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).IsSuccess;
				}
				catch (Exception thrown)
				{
					Assert.Fail(
						$"seed {seed}: the parser threw {thrown.GetType().Name}: {thrown.Message}\n" +
						$"input: \"{input}\"\ngrammar:\n{grammar}");

					return;
				}

				Assert.True(
					engine == oracle,
					$"seed {seed}, direct {direct}: the parser says {engine}, the semantics say {oracle}\n" +
					$"input: \"{input}\"\ngrammar:\n{grammar}");
			}
		}
	}

	/// <summary>
	/// The grammars the fuzzer above runs, asked of the grammar's own stages and the semantics
	/// alone: enough of them pass every check to be worth running, and on none of them do the
	/// semantics reach a reference some path left unbound — which the checks refuse, so a path
	/// that did would be a route the checks missed.
	/// </summary>
	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	public void The_semantics_answer_every_bound_grammar_the_checks_pass(int seed)
	{
		var random   = new Random(seed * 104729);
		var passed   = 0;
		var tried    = 0;

		while (passed < 30)
		{
			tried++;

			Assert.True(tried < 600, $"seed {seed}: only {passed} of {tried} generated grammars pass the checks.");

			var grammar = GenerateBound(random);

			if (Refused(grammar))
				continue;

			passed++;

			var graph = Normalized(grammar);
			var start = graph.Rules.First(rule => rule.Name == "Start");

			for (var round = 0; round < 30; round++)
			{
				var input = Input(random);

				try
				{
					ReferenceInterpreter.Parses(graph, start, input);
				}
				catch (InvalidOperationException unbound)
				{
					Assert.Fail($"seed {seed}: {unbound.Message}\ninput: \"{input}\"\ngrammar:\n{grammar}");
				}
			}
		}
	}

	// ── The generator ────────────────────────────────────────────────────────────

	/// <summary>
	/// A small grammar inside the interpreter's fence: rules call only later rules, so
	/// nothing recurses; nothing guards, nothing is external, nothing constructs.
	/// </summary>
	static string Generate(Random random)
	{
		var rules = random.Next(1, 4);
		var text  = new StringBuilder();

		// Most grammars are spaced, because the seam machinery is where the newest
		// proofs live — the standing exits, the peel, the pair — and a third of those
		// wear atomic braces with a comment form, which is what routes their seams
		// through the scanner compilation.
		switch (random.Next(4))
		{
			case 0:
				text.AppendLine("trivia = [' ']*");

				break;

			case 1:
				text.AppendLine("trivia = { (' ' | \"//\" & [^ 'c' | ' ']*)* }");

				break;
		}

		for (var i = 0; i < rules; i++)
		{
			text.Append(i == 0 ? "Start = " : $"R{i} = ");
			text.AppendLine(Body(random, depth: 0, callableFrom: i + 1, ruleCount: rules));
		}

		text.AppendLine("parse Start");

		return text.ToString();
	}

	static string Body(Random random, int depth, int callableFrom, int ruleCount)
	{
		// Leaves only, once deep enough.
		var pick = depth >= 3 ? random.Next(3) : random.Next(10);

		switch (pick)
		{
			case 0:
				return random.Next(3) switch
				{
					0 => $"'{Letter(random)}'",
					1 => $"\"{Letter(random)}{Letter(random)}\"",
					_ => $"\"{Letter(random)}\"i",
				};

			case 1:
				return random.Next(3) switch
				{
					0 => "['a'..'b']",
					1 => "[^ 'a' | 'c']",
					_ => "['b'..'c' | 'x']",
				};

			case 2:
				// A call, where there is a later rule to call; a leaf otherwise.
				return callableFrom < ruleCount
					? $"R{random.Next(callableFrom, ruleCount)}"
					: $"'{Letter(random)}'";

			case 3:
			case 4:
			{
				var parts = new string[random.Next(2, 4)];

				for (var i = 0; i < parts.Length; i++)
					parts[i] = Body(random, depth + 1, callableFrom, ruleCount);

				return "(" + string.Join(" & ", parts) + ")";
			}

			case 5:
			case 6:
			{
				var alternatives = new string[random.Next(2, 4)];

				for (var i = 0; i < alternatives.Length; i++)
					alternatives[i] = Body(random, depth + 1, callableFrom, ruleCount);

				return "(" + string.Join(" | ", alternatives) + ")";
			}

			case 7:
			{
				var quantifier = random.Next(5) switch
				{
					0 => "?",
					1 => "*",
					2 => "+",
					3 => "{2}",
					_ => "{1,3}",
				};

				return Body(random, depth + 1, callableFrom, ruleCount) + quantifier;
			}

			case 8:
				return "{ " + Body(random, depth + 1, callableFrom, ruleCount) + " }";

			default:
				return (random.Next(2) == 0 ? "?= " : "?! ") +
					Body(random, depth + 1, callableFrom, ruleCount);
		}
	}

	/// <summary>
	/// A grammar whose start rule binds <c>$q</c> to a generated body and then reads it again,
	/// with generated operands around both.
	/// </summary>
	/// <remarks>
	/// The binding takes one of the forms the section names — plain, as a repeated binding (a
	/// binding of the run), or with the body made optional inside it — and the reading one of the
	/// operands that see a binding. Some of what this writes the checks refuse (a reference to a
	/// body that may be empty under <c>*</c>, a count whose body may be empty); those are skipped,
	/// as above.
	/// </remarks>
	static string GenerateBound(Random random)
	{
		var rules = random.Next(1, 3);
		var text  = new StringBuilder();

		if (random.Next(4) == 0)
			text.AppendLine("trivia = [' ']*");

		var body = Body(random, depth: 1, callableFrom: 1, ruleCount: rules);

		var binding = random.Next(5) switch
		{
			0 => $"($q: {Leaf(random)})*",
			1 => $"($q: {Leaf(random)}){{0,2}}",
			2 => $"$q: ({body}?)",
			3 => $"{{ $q: {body} }}",
			_ => $"$q: {body}",
		};

		var reading = random.Next(9) switch
		{
			0 => "$q",
			1 => "?!$q & any",
			2 => "?=$q & any",
			3 => $"{Leaf(random)}{{#q}}",
			4 => $"{Leaf(random)}{{#q,}}",
			5 => $"{Leaf(random)}{{1,#q}}",
			6 => "(?!$q & any)* & $q",
			7 => "($q | 'c')",
			_ => "$q & $q",
		};

		var before = random.Next(3) == 0 ? Body(random, depth: 2, callableFrom: 1, ruleCount: rules) + " & " : "";
		var middle = random.Next(2) == 0 ? " & " + Body(random, depth: 2, callableFrom: 1, ruleCount: rules) : "";

		text.AppendLine($"Start = {before}{binding}{middle} & {reading}");

		for (var i = 1; i < rules; i++)
			text.AppendLine($"R{i} = " + Body(random, depth: 1, callableFrom: i + 1, ruleCount: rules));

		text.AppendLine("parse Start");

		return text.ToString();
	}

	/// <summary>One item, as a literal or a set: what a count repeats and a repeated binding binds.</summary>
	static string Leaf(Random random)
	{
		return random.Next(3) switch
		{
			0 => $"'{Letter(random)}'",
			1 => "['a'..'b']",
			_ => "[^ 'c']",
		};
	}

	/// <summary>Whether the grammar's own stages refuse it, without generating anything.</summary>
	static bool Refused(string grammar)
	{
		var parsed = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance);
		var model  = GrammarBinder.Bind(parsed.File, own: grammar.Length);
		var graph  = GrammarNormalizer.Normalize(model, scanner: RoslynCSharpScanner.Instance);

		return parsed.Diagnostics.Concat(model.Diagnostics).Concat(graph.Diagnostics)
			.Any(static one => one.Severity == GramSeverity.Error);
	}

	static char Letter(Random random)
	{
		return (char)('a' + random.Next(3));
	}

	static string Input(Random random)
	{
		var length = random.Next(0, 9);
		var text   = new StringBuilder(length);

		for (var i = 0; i < length; i++)
			text.Append(random.Next(6) == 0 ? ' ' : (char)('a' + random.Next(3)));

		return text.ToString();
	}

	static RecognitionGraph Normalized(string text)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));
	}
}
