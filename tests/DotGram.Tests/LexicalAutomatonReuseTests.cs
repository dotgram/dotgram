using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The lexical automata a compile keeps for the other compiles of the process, by a key of what
/// building one reads (<c>LexicalAutomaton.Reused</c>).
/// </summary>
/// <remarks>
/// <para>
/// What is held here is the key: two sets of patterns that make different machines must never be
/// one key, or a compile takes the other grammar's lexer. So every variant below is one detail of a
/// pattern changed — a character, a case, a bound, a delimiter, how rules call one another, whether
/// a pattern is read exactly — and wherever two of them share a key, their machines built afresh
/// must be the same machine. And the same patterns read again from text of their own must be one
/// key, or nothing is ever taken.
/// </para>
/// <para>
/// The shelf is the process's, and the tests of this assembly run side by side, so a test that
/// keeps a machine spells keywords no other test does.
/// </para>
/// </remarks>
public sealed class LexicalAutomatonReuseTests
{
	static RecognitionGraph Graph(string grammar, AutomatonReuse reuse = AutomatonReuse.Off)
	{
		var graph = GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(
					GramLexer.Tokenize(grammar, DotGram.Language.RoslynCSharpScanner.Instance)).File!));

		graph.ReuseAutomata = reuse;

		return graph;
	}

	/// <summary>
	/// A grammar whose lexer is large enough to be kept: a few hundred keywords no other test
	/// spells, an identifier over Unicode letters, a number and a quoted string.
	/// </summary>
	static string Keywords(string prefix)
	{
		var words = string.Join(" | ", Enumerable.Range(0, 300).Select(i => $"\"{prefix}{i:x}\"i"));

		return
			$$"""
			trivia = ' '*
			namespace Lexical
			{
				trivia = none
				Name   = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_']*
				Number = ['0'..'9']+ & ('.' & ['0'..'9']+)?
				Quoted = "'" & (?!"'" & any)* & "'"
			}
			Item  = {{words}} | Lexical.Name | Lexical.Number | Lexical.Quoted | '(' | ')' | "<="
			Start = Item*
			parse Start
			""";
	}

	static string Unique()
	{
		return "k" + Guid.NewGuid().ToString("N").Substring(0, 8);
	}

	[Fact]
	public void The_same_patterns_read_again_take_the_machine_kept()
	{
		var text   = Keywords(Unique());
		var first  = TerminalInventory.Of(Graph(text, AutomatonReuse.On)).Machine!;
		var second = TerminalInventory.Of(Graph(text, AutomatonReuse.On)).Machine!;

		Assert.True(first.Walked >= LexicalAutomaton.Worth, $"walked {first.Walked}");
		Assert.Same(first, second);
	}

	[Fact]
	public void A_machine_taken_under_verification_is_held_to_one_built_afresh()
	{
		var text   = Keywords(Unique());
		var first  = TerminalInventory.Of(Graph(text, AutomatonReuse.On)).Machine!;
		var second = TerminalInventory.Of(Graph(text, AutomatonReuse.Verify)).Machine!;

		Assert.Same(first, second);
	}

	[Fact]
	public void Off_takes_nothing_and_keeps_nothing()
	{
		var text   = Keywords(Unique());
		var first  = TerminalInventory.Of(Graph(text)).Machine!;
		var second = TerminalInventory.Of(Graph(text)).Machine!;
		var kept   = TerminalInventory.Of(Graph(text, AutomatonReuse.On)).Machine!;

		Assert.NotSame(first, second);
		Assert.NotSame(first, kept);
		Assert.NotSame(second, kept);
		Assert.True(LexicalAutomaton.Same(first, second));
		Assert.True(LexicalAutomaton.Same(first, kept));
	}

	/// <summary>
	/// A changed keyword is another machine, and the grammar's own lexer is the one built for it.
	/// </summary>
	[Fact]
	public void A_changed_keyword_is_built_again()
	{
		var prefix  = Unique();
		var folded  = $"\"{prefix}1f\"i";
		var changed = Keywords(prefix).Replace(folded, folded[..^1], StringComparison.Ordinal);
		var first   = TerminalInventory.Of(Graph(Keywords(prefix), AutomatonReuse.On)).Machine!;
		var second  = TerminalInventory.Of(Graph(changed, AutomatonReuse.On)).Machine!;
		var fresh   = TerminalInventory.Of(Graph(changed)).Machine!;

		Assert.NotSame(first, second);
		Assert.True(LexicalAutomaton.Same(second, fresh));
	}

	/// <summary>
	/// The patterns <c>P0</c>, <c>P1</c> and <c>P2</c> of a grammar, as the build is given them:
	/// calls of the rules, whose bodies it reads.
	/// </summary>
	static (RecognitionGraph Graph, List<Node> Patterns) Patterns(string rules)
	{
		var graph = Graph(
			$"""
			{rules}
			Start = (P0 | P1 | P2)*
			parse Start
			""");

		var patterns = new[] { "P0", "P1", "P2" }
			.Select(name => (Node)new Node.Call(graph.Rules.First(rule => rule.Name == name), []))
			.ToList();

		return (graph, patterns);
	}

	const string Base =
		"""
		P0 = "select" | "from"i
		P1 = [\p{L} | '_'] & [\p{L} | \p{Nd} | '_']*
		P2 = "/*" & (?!"*/" & any)* & "*/" & Q
		Q  = ['0'..'9']{1,3}
		""";

	/// <summary>Each the base with one detail changed.</summary>
	static List<string> Variants()
	{
		(string From, string To)[] changes =
		[
			("\"select\"", "\"Select\""),
			("\"select\"", "\"select\"i"),
			("\"select\"", "\"selec\""),
			("\"select\" | \"from\"i", "\"from\"i | \"select\""),
			("\"from\"i", "\"from\""),
			("[\\p{L} | '_']", "[\\p{Lu} | '_']"),
			("[\\p{L} | '_']", "[\\p{L} | '-']"),
			("[\\p{L} | '_']", "[^\\p{L} | '_']"),
			("[\\p{L} | \\p{Nd} | '_']*", "[\\p{L} | \\p{Nd} | '_']+"),
			("[\\p{L} | \\p{Nd} | '_']*", "[\\p{L} | \\p{Nd} | '_']{0,4}"),
			("(?!\"*/\" & any)*", "(?!\"*)\" & any)*"),
			("(?!\"*/\" & any)*", "(?!\"*/\" & ['a'..'z'])*"),
			("\"*/\" & Q", "\"*/\" & ['0'..'9']{1,3}"),
			("\"*/\" & Q", "\"*/\" & Q & Q"),
			("['0'..'9']{1,3}", "['0'..'9']{1,4}"),
			("['0'..'9']{1,3}", "['0'..'9']{2,3}"),
			("['0'..'9']{1,3}", "['0'..'9']{1,}"),
			("['0'..'9']{1,3}", "['0'..'8']{1,3}"),
			("['0'..'9']{1,3}", "(['0'..'9'] | \"x9\"){1,3}"),
			("['0'..'9']{1,3}", "{ ['0'..'9']{1,3} }"),
			("['0'..'9']{1,3}", "['0'..'9']{1,3} & Q"),
		];

		var variants = new List<string> { Base };

		foreach (var (from, to) in changes)
		{
			Assert.Contains(from, Base, StringComparison.Ordinal);

			variants.Add(Base.Replace(from, to, StringComparison.Ordinal));
		}

		return variants;
	}

	/// <summary>
	/// The same patterns read from text of their own, and written differently in nothing a build
	/// reads, are one key.
	/// </summary>
	[Fact]
	public void The_same_patterns_from_another_reading_are_one_key()
	{
		var again = "// again\n" + Base.Replace("P0 = ", "P0 =  ", StringComparison.Ordinal);

		var (first, firstPatterns)   = Patterns(Base);
		var (second, secondPatterns) = Patterns(again);
		var key                      = LexicalAutomaton.KeyOf(first, firstPatterns);

		Assert.NotNull(key);
		Assert.Equal(key, LexicalAutomaton.KeyOf(second, secondPatterns));
	}

	/// <summary>
	/// No two variants share a key unless the machines built afresh for them are the same; and
	/// whether a pattern is read exactly is part of the key.
	/// </summary>
	[Fact]
	public void No_key_is_shared_by_two_different_machines()
	{
		var built = new List<(string Name, string Key, LexicalAutomaton? Machine)>();

		foreach (var exact in new[] { false, true })
			foreach (var rules in Variants())
			{
				var (graph, patterns) = Patterns(rules);
				var key               = LexicalAutomaton.KeyOf(graph, patterns, _ => exact);

				Assert.NotNull(key);

				var machine = LexicalAutomaton.Of(graph, patterns, [], _ => exact);

				built.Add(($"{(exact ? "exact " : "")}{rules}", key!, machine));
			}

		Assert.Contains(built, static one => one.Machine is not null);

		for (var i = 0; i < built.Count; i++)
			for (var j = i + 1; j < built.Count; j++)
			{
				if (built[i].Key != built[j].Key)
					continue;

				var same = (built[i].Machine, built[j].Machine) switch
				{
					(null, null)             => true,
					({ } left, { } right)    => LexicalAutomaton.Same(left, right),
					_                        => false,
				};

				Assert.True(same, $"one key for two machines:\n{built[i].Name}\n{built[j].Name}");
			}

		// And the variants are told apart: a key shared by every one of them would pass the above.
		var shared = built
			.GroupBy(static one => one.Key)
			.Where(static one => one.Count() > 1)
			.Select(static one => string.Join("\n", one.Select(static each => each.Name)));

		Assert.True(!shared.Any(), "variants read as one:\n" + string.Join("\n\n", shared));
	}

	/// <summary>A pattern no machine is built of gives no key, and is not kept.</summary>
	[Fact]
	public void A_pattern_no_machine_reads_has_no_key()
	{
		var (graph, patterns) = Patterns(
			"""
			P0 = "select"
			P1 = ['a'..'z']+
			P2 = 'a' & @Host
			""");

		Assert.Null(LexicalAutomaton.KeyOf(graph, patterns));
	}
}
