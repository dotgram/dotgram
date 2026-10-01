using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What an ignore-case literal matches, asked of every place that decides it.
/// </summary>
/// <remarks>
/// <para>
/// The rule (docs/syntax.md): each character matches itself and its simple upper and lower
/// case, and a partner on the other side of ASCII is not taken. Every row below is put to
/// <see cref="CaseFold"/> itself, to the first set, to the spellings the overlap analysis
/// enumerates, and to generated parsers: the engine, the direct reader, a scanned rule and
/// a grammar cut into tokens by the lexical automaton — single characters, ASCII keywords
/// compared in one call, and literals beyond ASCII compared a character at a time.
/// </para>
/// <para>
/// They used to answer by three different folds: the matcher and the first sets by
/// <c>ToUpperInvariant</c> (<c>'s'i</c> read U+017F), a keyword by ordinal folding, and the
/// lexical automaton by the upper and lower pair, so one literal read differently with and
/// without <c>Lexical = true</c>.
/// </para>
/// </remarks>
public sealed class CaseFoldTests
{
	/// <summary>A literal, the inputs it reads, and the inputs it refuses.</summary>
	static readonly (string Literal, string[] Reads, string[] Refuses)[] Table =
	[
		("s",            ["s", "S"],                         ["ſ"]),
		("S",            ["s", "S"],                         ["ſ"]),
		("ſ",       ["ſ"],                         ["s", "S"]),
		("k",            ["k", "K"],                         ["K"]),
		("K",            ["k", "K"],                         ["K"]),
		("K",       ["K"],                         ["k", "K"]),
		("i",            ["i", "I"],                         ["ı", "İ"]),
		("I",            ["i", "I"],                         ["ı", "İ"]),
		("ı",       ["ı"],                         ["i", "I"]),
		("İ",       ["İ"],                         ["i", "I"]),
		("μ",       ["μ", "Μ"],               ["µ"]),
		("Μ",       ["μ", "Μ"],               ["µ"]),
		("µ",       ["µ", "Μ"],               ["μ"]),
		("σ",       ["σ", "Σ"],               ["ς"]),
		("Σ",       ["σ", "Σ"],               ["ς"]),
		("ς",       ["ς", "Σ"],               ["σ"]),
		("ß",       ["ß"],                         ["ẞ"]),
		("ẞ",       ["ẞ", "ß"],               []),
		("1",            ["1"],                              ["!"]),
		("привет",       ["привет", "ПРИВЕТ", "пРиВеТ"],     ["пpивет", "привеt"]),
		("select",       ["select", "SELECT", "SeLeCt"],     ["ſelect", "ſELECT", "seleсt"]),
		("kelvin",       ["KELVIN", "kElViN"],               ["Kelvin", "kelviո"]),
		("is",           ["IS", "iS"],                       ["ıs", "İS", "iſ"]),
		("μs",      ["ΜS", "μs"],             ["µs", "μſ"]),
		("σς", ["ΣΣ", "σς"],   ["ςσ", "σσ"]),
		("a1",           ["A1", "a1"],                       ["A!", "à1"]),
	];

	public static TheoryData<int> Rows()
	{
		var rows = new TheoryData<int>();

		for (var i = 0; i < Table.Length; i++)
			rows.Add(i);

		return rows;
	}

	/// <summary>The rule itself, for every character there is, over the generator's own table.</summary>
	[Fact]
	public void Every_character_folds_to_itself_and_its_cases_on_its_own_side_of_ascii()
	{
		for (var c = 0; c <= char.MaxValue; c++)
		{
			var value = (char)c;
			var cases = CaseFold.Of(value);

			Assert.Contains(value, cases);
			Assert.Equal(cases.Distinct().OrderBy(static one => one), cases);

			foreach (var one in cases)
			{
				if (one != value && one != CaseFold.Upper(value) && one != CaseFold.Lower(value) ||
					one <= 0x7F != value <= 0x7F)
				{
					Assert.Fail($"U+{c:X4} folds to U+{(int)one:X4}");
				}
			}

			var partners = new[] { CaseFold.Upper(value), CaseFold.Lower(value) };

			foreach (var one in partners)
				if (one <= 0x7F == value <= 0x7F)
					Assert.Contains(one, cases);
		}
	}

	/// <summary>
	/// The mappings are Unicode 16's simple ones as UnicodeData.txt gives them, before the rule
	/// drops a partner across ASCII: U+0131 does upper-case to <c>I</c> there, and the Kelvin sign
	/// lower-cases to <c>k</c>.
	/// </summary>
	[Fact]
	public void The_case_tables_are_unicode_16s_simple_mappings()
	{
		Assert.Equal("16.0.0", CaseFold.UnicodeVersion);

		Assert.Equal('A',      CaseFold.Upper('a'));
		Assert.Equal('a',      CaseFold.Lower('A'));
		Assert.Equal('I',      CaseFold.Upper('\u0131'));
		Assert.Equal('i',      CaseFold.Lower('\u0130'));
		Assert.Equal('S',      CaseFold.Upper('\u017F'));
		Assert.Equal('k',      CaseFold.Lower('\u212A'));
		Assert.Equal('\u039C', CaseFold.Upper('\u00B5'));
		Assert.Equal('\u01C4', CaseFold.Upper('\u01C5'));
		Assert.Equal('\u01C6', CaseFold.Lower('\u01C5'));
		Assert.Equal('\u00DF', CaseFold.Lower('\u1E9E'));
		Assert.Equal('\u00DF', CaseFold.Upper('\u00DF'));
		Assert.Equal('\uA7CB', CaseFold.Upper('\u0264'));
		Assert.Equal('\uA7CE', CaseFold.Upper('\uA7CE'));
		Assert.Equal('1',      CaseFold.Upper('1'));
	}

	/// <summary>
	/// Ordinal folding, which compares an ASCII keyword in one call, is the rule there: it pairs
	/// an ASCII character with nothing beyond ASCII, on the runtime the tests run on.
	/// </summary>
	[Fact]
	public void Ordinal_folding_agrees_with_the_rule_on_every_ascii_literal()
	{
		for (var a = 0; a <= 0x7F; a++)
		{
			var literal = ((char)a).ToString();

			for (var x = 0; x <= char.MaxValue; x++)
			{
				var input = (char)x;

				if (CaseFold.Matches((char)a, input) !=
					MemoryExtensions.Equals([input], literal.AsSpan(), StringComparison.OrdinalIgnoreCase))
				{
					Assert.Fail($"U+{a:X4} against U+{x:X4}");
				}
			}
		}
	}

	/// <summary>The table, against the rule and against what the analyses build from it.</summary>
	[Theory]
	[MemberData(nameof(Rows))]
	public void The_analyses_read_what_the_rule_reads(int row)
	{
		var (literal, reads, refuses) = Table[row];
		var spellings = CaseFold.Spellings(literal, 1 << 12)!;
		var first     = FirstSets.Of(new Node.Literal(literal) { IgnoreCase = true }, Graph("A = 'a'"));

		foreach (var (input, expected) in Inputs(reads, refuses))
		{
			Assert.Equal(expected, CaseFold.Matches(literal, input.AsSpan()));
			Assert.Equal(expected, spellings.Contains(input));

			// The first set is the first character's cases and nothing more.
			Assert.Equal(
				CaseFold.Matches(literal[0], input[0]),
				first.Overlaps(FirstSets.First.Chars([new CharRange(input[0], input[0])])));
		}

		Assert.All(spellings, one => Assert.True(CaseFold.Matches(literal, one.AsSpan()), one));
	}

	/// <summary>
	/// The table, against generated parsers: the engine, the flat rendering and the direct
	/// reader, which must also refuse at the same place; a scanned rule; and a grammar the
	/// lexical automaton cuts into tokens.
	/// </summary>
	/// <remarks>
	/// <c>L</c> is the literal alone, which the flat rendering takes where it may. <c>R</c> is
	/// the literal under a value and a recursion, which keeps it out of the flat rendering and
	/// in the reader. <c>W</c> is atomic and recordless, so a call to it is a scanner.
	/// </remarks>
	[Fact]
	public void The_generated_parsers_read_what_the_rule_reads()
	{
		var grammar = new StringBuilder();
		// Tokens are told from characters by trivia, so the grammar cut into tokens has some.
		var tokens  = new StringBuilder("trivia = { ' '* }\n");

		for (var i = 0; i < Table.Length; i++)
		{
			var literal = Quoted(Table[i].Literal);

			grammar.Append($"L{i} = {literal}i\n");
			grammar.Append($"R{i} : @string = t: {literal}i => @(t.ToString()) | '(' & inner: R{i} & ')' => @(inner)\n");
			grammar.Append($"W{i} = {{ ({literal}i)+ }}\n");
			grammar.Append($"S{i} = W{i} & 'z' | '(' & W{i} & ')'\n");
			grammar.Append($"parse L{i}\nparse R{i}\nparse S{i}\n");

			tokens.Append($"L{i} = {literal}i\nparse L{i}\n");
		}

		var engine  = Compiled(grammar.ToString(), direct: false, lexical: false);
		var direct  = Compiled(grammar.ToString(), direct: true,  lexical: false);
		var lexical = Compiled(tokens.ToString(),  direct: false, lexical: true);

		Assert.Contains("Scan_W0_", engine.Source, StringComparison.Ordinal);
		Assert.Contains("Read_R0_", direct.Source, StringComparison.Ordinal);
		Assert.DoesNotContain("Read_R0_", engine.Source, StringComparison.Ordinal);
		Assert.Contains("Tokenize_DotGram(", lexical.Source, StringComparison.Ordinal);

		var failures = new List<string>();

		for (var i = 0; i < Table.Length; i++)
		{
			var (literal, reads, refuses) = Table[i];

			foreach (var (input, expected) in Inputs(reads, refuses))
			{
				var where = Mismatch(literal, input);
				var asked = new (string Site, Assembly Assembly, string Rule, string Input, bool Placed)[]
				{
					("engine",         engine.Assembly,  "L", input,       true),
					("flat",           direct.Assembly,  "L", input,       true),
					("engine, valued", engine.Assembly,  "R", input,       true),
					("reader",         direct.Assembly,  "R", input,       true),
					("scan, engine",   engine.Assembly,  "S", input + "z", false),
					("scan, direct",   direct.Assembly,  "S", input + "z", false),
					("lexical",        lexical.Assembly, "L", input,       false),
				};

				foreach (var (site, assembly, rule, text, placed) in asked)
				{
					var answer = EmittedCode.Match(assembly, "Grammar", $"TryParse{rule}{i}", text);

					if (answer.IsSuccess != expected)
						failures.Add($"{site}: \"{literal}\"i against \"{input}\" read {answer.IsSuccess}, wanted {expected}");

					// Where the literal is refused is where the character that does not fit is.
					else if (!expected && placed && answer.Position != where)
						failures.Add($"{site}: \"{literal}\"i against \"{input}\" refused at {answer.Position}, wanted {where}");
				}
			}
		}

		Assert.True(failures.Count == 0, string.Join("\n", failures));
	}

	static IEnumerable<(string Input, bool Expected)> Inputs(string[] reads, string[] refuses)
	{
		return reads.Select(static one => (one, true)).Concat(refuses.Select(static one => (one, false)));
	}

	/// <summary>Where the first character the literal does not read is.</summary>
	static int Mismatch(string literal, string input)
	{
		var at = 0;

		while (at < literal.Length && CaseFold.Matches(literal[at], input[at]))
			at++;

		return at;
	}

	static string Quoted(string literal)
	{
		return literal.Length == 1 ? $"'{literal}'" : $"\"{literal}\"";
	}

	static (string Source, Assembly Assembly) Compiled(string grammar, bool direct, bool lexical)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Direct = direct, Lexical = lexical, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var source = Assert.Single(result.Sources).Text;

		return (source, EmittedCode.Compile(source));
	}

	static RecognitionGraph Graph(string text)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));
	}
}
