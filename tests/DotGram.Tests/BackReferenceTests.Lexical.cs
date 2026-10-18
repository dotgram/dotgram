using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Bindings in a terminal the lexer reads (<c>Lexical = true</c>): the lexer begins such a
/// terminal with what an automaton can read of it, and the terminal's own rule reads the whole
/// token from its start — so the binding has a span and the reference has text to compare with.
/// </summary>
/// <remarks>
/// A grammar cut in two reads from memory and has no reader or stream form of its own (GRAM4026,
/// a limit of the split and not of bindings). So the readings held together here are the grammar
/// over tokens, by the reader and by the engine, and the same grammar over characters as a string
/// and — through the forms B1 opened — a reader, a stream and bytes.
/// </remarks>
public sealed partial class BackReferenceTests
{
	/// <summary>
	/// The rows that fit inside a lexical namespace as one terminal: every row that declares no
	/// trivia of its own, the examples of docs/syntax.md §3.5.1 among them.
	/// </summary>
	public static TheoryData<string> LexicalRowNames()
	{
		return [.. Rows.Where(static row => !row.Grammar.Contains("trivia", StringComparison.Ordinal)).Select(static row => row.Name)];
	}

	/// <summary>
	/// The rows whose terminal begins with nothing an automaton reads, and which the split refuses
	/// by name: a repetition of none begins with nothing. (A left-recursive rule is not one of
	/// them: the climb rewrites it into a loop before the split asks, and the loop begins as its
	/// base does.)
	/// </summary>
	static readonly HashSet<string> Unbegun = ["BindingReadInItsOwnTurn", "RuleThatBindsCalledInALoop"];

	/// <summary>
	/// The rows whose terminal begins with something the automaton reads that can be nothing — a
	/// count by a length, which may be zero turns, right after an optional run — and which the
	/// split declines as it declines any such beginning, with a word and the parser over characters.
	/// </summary>
	static readonly HashSet<string> Fallback = ["CountOfARepeatedBinding"];

	/// <summary>The rows whose scannerless twin is also read through a buffer, to hold all four forms to one answer.</summary>
	static readonly HashSet<string> BufferedTwins =
		["Quoted", "CSharpRawString", "Heredoc", "PostgreSqlDollarQuoted", "XmlElement", "CountBetweenTwoLengths"];

	[Theory]
	[MemberData(nameof(LexicalRowNames))]
	public void A_terminal_that_binds_is_read_over_tokens_as_over_characters(string name)
	{
		BackReferenceEmission.RequireBoth();

		var row  = Named(name);
		var twin = Twin(row.Grammar, row.Rule);

		if (Unbegun.Contains(name) || Fallback.Contains(name))
		{
			var told = GramCompiler.Compile(twin, new GramCompilerOptions { Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance }).Diagnostics;

			if (Unbegun.Contains(name))
				Assert.Contains(told, static one => one.Id == GrammarNormalizer.BindingNotSupported && one.Message.Contains("nothing the lexer can read begins", StringComparison.Ordinal));
			else
				Assert.Contains(told, static one => one.Id == GramCompiler.NotCut && one.Message.Contains("can be nothing", StringComparison.Ordinal));

			return;
		}

		var reader     = Lexical(twin, direct: true).Assembly;
		var engine     = Lexical(twin, direct: false).Assembly;
		var characters = Compiled(twin, CarrierKind.Auto, direct: true);

		foreach (var input in row.Accepted.Concat(row.Refused))
		{
			var over  = EmittedCode.Match(characters, "Grammar", "TryParseRoot", input);
			var read  = EmittedCode.Match(reader, "Grammar", "TryParseRoot", input);
			var run   = EmittedCode.Match(engine, "Grammar", "TryParseRoot", input);

			// The row's own answer, where the trivia around the twin's root cannot change it.
			if (!input.StartsWith(' ') && !input.EndsWith(' '))
				Assert.True(
					over.IsSuccess == row.Accepted.Contains(input),
					$"{name}, over characters: \"{Escaped(input)}\" is {(over.IsSuccess ? "read" : "refused")}");

			foreach (var (how, answer) in new[] { ("reader over tokens", read), ("engine over tokens", run) })
			{
				// The value too, except where the input begins with trivia: a whole parse over
				// characters keeps the leading trivia in a text value and one over tokens does not,
				// binding or none (LeadingTriviaInAValue).
				var valued = !input.StartsWith(' ');

				Assert.True(
					(over.IsSuccess, valued ? Spelled(over.Value) : null) == (answer.IsSuccess, valued ? Spelled(answer.Value) : null),
					$"{name}: \"{Escaped(input)}\"\n  over characters: {over}\n  {how}: {answer}");

				if (over.IsSuccess)
					Assert.Equal(over.Position, answer.Position);
			}
		}

		if (!BufferedTwins.Contains(name))
			return;

		var (buffered, bytes) = Buffered(twin, CarrierKind.Auto, direct: true);

		foreach (var input in row.Accepted.Concat(row.Refused))
			AgreesOverABuffer(name, buffered, "Root", input, "twin over characters", bytes);
	}

	/// <summary>
	/// The automaton reads the beginning of a terminal that binds text together with every other
	/// token, and longest match decides between them as it decides everything the lexer begins:
	/// a raw string's quotes beside a plain string's, a heredoc's opening beside the operator it
	/// starts with, a long bracket beside a bracket.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void The_beginning_of_a_terminal_that_binds_competes_by_longest_match(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			trivia = { (' ' | '\n')* }
			namespace Lexical
			{
				trivia  = none
				Raw     = { $q: '"'{3,} } & { (?!$q & any)* } & $q & ?!'"'
				Str     = '"' & [^ '"']* & '"'
				Heredoc = "<<" & $tag: Name & eol & { (?!($tag & EndLine) & Line)* } & $tag & ?=EndLine
				Long    = '[' & $eq: '='* & '[' & { (?!(']' & $eq & ']') & any)* } & ']' & $eq & ']'
				Name    = ['a'..'z']+
				Line    = [^ '\r' | '\n']+ & EndLine | eol
				EndLine = eol | eof
			}
			Item : @string
				= r: Lexical.Raw                         => @("raw:" + r)
				| s: Lexical.Str                         => @("str:" + s)
				| h: Lexical.Heredoc                     => @("doc:" + h)
				| l: Lexical.Long                        => @("long:" + l)
				| a: Lexical.Name & "<<" & b: Lexical.Name => @("shift:" + a + "," + b)
				| n: Lexical.Name                        => @("name:" + n)
				| '[' & i: Lexical.Name & ']'            => @("index:" + i)
			Items : @string = items: Item* => @(string.Join("|", items))
			parse Items
			""";

		var (assembly, _) = Lexical(grammar, direct);

		string Read(string input)
		{
			var (isSuccess, value, error, position) = EmittedCode.Match(assembly, "Grammar", "TryParseItems", input);

			return isSuccess ? (string)value! : $"refused: {error} at {position}";
		}

		// Three quotes begin a raw string, two a plain one, each read to its own end.
		Assert.Equal("raw:\"\"\"a\"\"\"", Read("\"\"\"a\"\"\""));
		Assert.Equal("str:\"\"|raw:\"\"\"a\"\"b\"\"\"|str:\"c\"", Read("\"\" \"\"\"a\"\"b\"\"\" \"c\""));
		Assert.Equal("raw:\"\"\"\"a\"\"\"b\"\"\"\"", Read("\"\"\"\"a\"\"\"b\"\"\"\""));

		// The heredoc's beginning reads through the end of its line; without one, the operator stands.
		Assert.Equal("doc:<<eof\nbody\neof|name:x", Read("<<eof\nbody\neof\nx"));
		Assert.Equal("shift:a,b|name:c", Read("a <<b c"));
		Assert.Equal("doc:<<end\n\nend", Read("<<end\n\nend"));

		// A long bracket begins with two brackets and a bracket with one.
		Assert.Equal("long:[==[a]]b]=]c]==]|index:x|long:[[]]", Read("[==[a]]b]=]c]==] [x] [[]]"));

		// Where the beginning stands and the rule refuses, no token stands there: the beginning
		// decided, as it does for any terminal the lexer begins. Four quotes are a raw string's
		// opening to the automaton, and two empty strings only over characters.
		Assert.StartsWith("refused", Read("\"\"\"\""), StringComparison.Ordinal);
		Assert.StartsWith("refused", Read("[==[a]=]"), StringComparison.Ordinal);
	}

	/// <summary>
	/// The rest is read from the token's start by the terminal's own rule, so a greedy binding
	/// gives back into the beginning the automaton read: the token is what the rule reads, longer
	/// or shorter than where the automaton stopped.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void The_rule_decides_the_token_where_the_automaton_read_further_or_less(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Quoted = $q: '"'+ & ['a'..'z']* & $q
			}
			Item  : @string   = q: Lexical.Quoted => @(q)
			Items : @string[] = items: Item* => @(items)
			parse Items
			""";

		var (assembly, source) = Lexical(grammar, direct);

		Assert.Contains("Measure_Lexical_Quoted_DotGram(text, p, ref failure", source, StringComparison.Ordinal);

		string[] Read(string input)
		{
			var (isSuccess, value, _, _) = EmittedCode.Match(assembly, "Grammar", "TryParseItems", input);

			return isSuccess ? (string[])value! : ["refused"];
		}

		// The automaton reads `"""a` as the beginning; the rule binds two quotes and stops
		// after `"a""`: shorter than the beginning, and then another token.
		Assert.Equal(["\"\"a\"\"", "\"b\""], Read("\"\"a\"\" \"b\""));
		Assert.Equal(["\"\"\"abc\"\"\""], Read("\"\"\"abc\"\"\""));
		Assert.Equal(["\"a\"", "\"\"b\"\""], Read("\"a\"\"\"b\"\""));
		Assert.Equal(["refused"], Read("\"\"\"a\"\""));
		Assert.Equal(["refused"], Read("\"\"\"a"));
	}

	/// <summary>
	/// A beginning of one fixed spelling that a longer token also starts with: the automaton stops
	/// on the longer token, and the terminal is measured there too, from the token's start.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_longer_token_that_starts_with_the_beginning_is_measured_from_the_start(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Twice = $q: "ab" & $q
			}
			Item : @string = "abab" & 'x' => @("ababx") | t: Lexical.Twice => @("twice:" + t) | "abc" => @("abc")
			Items : @string = items: Item* => @(string.Join("|", items))
			parse Items
			""";

		var (assembly, source) = Lexical(grammar, direct);

		Assert.Contains("Measure_Lexical_Twice_DotGram(text, p, ref failure", source, StringComparison.Ordinal);

		string Read(string input)
		{
			var (isSuccess, value, error, position) = EmittedCode.Match(assembly, "Grammar", "TryParseItems", input);

			return isSuccess ? (string)value! : $"refused: {error} at {position}";
		}

		Assert.Equal("twice:abab|abc", Read("abab abc"));
		Assert.Equal("abc|twice:abab", Read("abc abab"));

		// `abab` is a token of its own and the terminal, both as long: the kind is both, and
		// the syntax reads whichever it asked for.
		Assert.Equal("ababx", Read("ababx"));
		Assert.Equal("twice:abab", Read("abab"));
	}

	/// <summary>
	/// Trivia stands between tokens and not inside one: a heredoc's lines are one token however
	/// many line breaks it holds, and the seam before and after it is skipped as before any token.
	/// A terminal that builds reads its value again over the whole token, binding and all.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Trivia_is_skipped_around_a_token_that_binds_and_a_value_is_read_over_the_whole_of_it(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			trivia = { (' ' | '\n' | '\t' | "--" & [^ '\n']*)* }
			namespace Lexical
			{
				trivia  = none
				Heredoc : @string = "<<" & $tag: Name & eol & body: { (?!($tag & EndLine) & Line)* } & $tag & ?=EndLine => @(body)
				Name    = ['a'..'z']+
				Line    = [^ '\r' | '\n']+ & EndLine | eol
				EndLine = eol | eof
			}
			Call : @string = f: Lexical.Name & '(' & arg: Lexical.Heredoc & ')' => @(f + ":" + arg)
			parse Call
			""";

		var (assembly, _) = Lexical(grammar, direct);

		Assert.Equal(
			"say:one\n\ntwo -- not a comment\n",
			EmittedCode.Match(assembly, "Grammar", "TryParseCall", "say ( -- a comment\n\t<<end\none\n\ntwo -- not a comment\nend\n)").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseCall", "say(<<end\none\nen)").IsSuccess);
	}

	/// <summary>
	/// A repetition over tokens that recovers resynchronizes on a kind, and a terminal that binds
	/// is one of the kinds it reads: a broken element is skipped to the next separator, and the
	/// elements around it are read. The engine alone: over kinds a recovery is not read by
	/// methods (GRAM5005), binding or none.
	/// </summary>
	[Fact]
	public void A_recovering_repetition_over_tokens_reads_a_terminal_that_binds()
	{
		BackReferenceEmission.RequireBoth();

		const bool direct = false;

		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Quoted = $q: '"'+ & ['a'..'z']* & $q
				Name   = ['a'..'z']+
			}
			Item  : @string = n: Lexical.Name & '=' & q: Lexical.Quoted & ';' => @(n + "=" + q)
			Items : @string = items: Item* recover ';' & eof => @(string.Join(",", items))
			parse Items
			""";

		var (assembly, _) = Lexical(grammar, direct);

		Assert.Equal("a=\"x\",b=\"\"y\"\"", EmittedCode.Match(assembly, "Grammar", "TryParseItems", "a = \"x\"; b = \"\"y\"\";").Value);
		Assert.Equal("a=\"x\",c=\"\"\"\"\"\"", EmittedCode.Match(assembly, "Grammar", "TryParseItems", "a = \"x\"; b = = \"y\"; c = \"\"\"\"\"\";").Value);
	}

	/// <summary>
	/// A terminal that calls a rule with a binding, or a rule written as a choice of two
	/// bindings, is begun as far as the automaton reads and read whole by its rule.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_binding_in_what_a_terminal_calls_or_in_an_alternative_of_it_is_read(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Str    = '"' & Body & '"'
				Body   = $q: ['a'..'z']+ & '-' & $q
				Pair   = '<' & $a: Name & '>' & $a | '{' & $b: Name & '}' & $b
				Name   = ['a'..'z']+
			}
			Item  : @string   = s: Lexical.Str => @(s) | p: Lexical.Pair => @(p)
			Items : @string[] = items: Item* => @(items)
			parse Items
			""";

		var (assembly, source) = Lexical(grammar, direct);

		Assert.Contains("Measure_Lexical_Str_DotGram(text, p, ref failure", source, StringComparison.Ordinal);
		Assert.Contains("Measure_Lexical_Pair_DotGram(text, p, ref failure", source, StringComparison.Ordinal);

		Assert.Equal(["\"ab-ab\"", "<x>x", "{yz}yz"], (string[])EmittedCode.Match(assembly, "Grammar", "TryParseItems", "\"ab-ab\" <x>x {yz}yz").Value!);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseItems", "\"ab-ac\"").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseItems", "<x>y").IsSuccess);
	}

	/// <summary>
	/// What is still refused, each with its reason where the binding stands: a terminal that begins
	/// with nothing the automaton reads, and a binding in what trivia is made of, which the scanner
	/// skipping the seam does not read.
	/// </summary>
	[Fact]
	public void A_terminal_that_begins_with_nothing_the_lexer_reads_is_refused_by_name()
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Looked = ?!'x' & $q: ['a'..'z']+ & '-' & $q
				Word   = ['a'..'z']+
			}
			R = Lexical.Looked & Lexical.Word
			""";

		var told = Refusals(grammar);

		Assert.Equal(
			[grammar.IndexOf("$q", StringComparison.Ordinal), grammar.LastIndexOf("$q", StringComparison.Ordinal)],
			told.Select(static one => one.Position));
		Assert.All(told, static one => Assert.Contains("nothing the lexer can read begins 'Looked'", one.Message, StringComparison.Ordinal));
	}

	[Fact]
	public void A_terminal_begun_by_a_rule_that_begins_with_nothing_the_lexer_reads_names_the_terminal()
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Outer  = Inner & '!'
				Inner  = ?='a' & $q: ['a'..'z']+ & '-' & $q
			}
			R = Lexical.Outer
			""";

		var told = Refusals(grammar);

		Assert.Equal(2, told.Count);
		Assert.All(told, static one => Assert.Contains("is in 'Inner', which the lexer reads as part of 'Outer'", one.Message, StringComparison.Ordinal));
		Assert.All(told, static one => Assert.Contains("nothing the lexer can read begins 'Outer'", one.Message, StringComparison.Ordinal));
	}

	[Fact]
	public void A_binding_in_what_trivia_is_made_of_is_refused_for_the_scanner()
	{
		const string grammar =
			"""
			trivia = { (' ' | "--" & Lexical.Long)* }
			namespace Lexical
			{
				trivia = none
				Long = '[' & $eq: '='* & '[' & { (?!(']' & $eq & ']') & any)* } & ']' & $eq & ']'
				Word = ['a'..'z']+
			}
			R = Lexical.Word & Lexical.Word
			""";

		var told = Refusals(grammar);

		Assert.Equal(3, told.Count);
		Assert.All(told, static one => Assert.Contains("skipped between tokens", one.Message, StringComparison.Ordinal));
		Assert.All(told, static one => Assert.Contains("does not read a binding yet", one.Message, StringComparison.Ordinal));
	}

	/// <summary>
	/// The same terminal with no `Lexical` asked for is read over characters as before, with no
	/// diagnostic; and asked for beside a `find`, the grammar is read over characters with the word
	/// the split always says there, not an error about the binding.
	/// </summary>
	[Fact]
	public void A_grammar_with_a_binding_in_a_terminal_is_cut_only_where_asked_and_possible()
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Quoted = $q: '"'+ & ['a'..'z']* & $q
			}
			R = Lexical.Quoted
			parse R
			""";

		Assert.Empty(Errors(grammar));

		var found = GramCompiler.Compile(
			grammar + "\nfind R\n",
			new GramCompilerOptions { Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance }).Diagnostics;

		Assert.DoesNotContain(found, static one => one.Severity == GramSeverity.Error);
		Assert.Contains(found, static one => one.Id == GramCompiler.NotCut && one.Message.Contains("find", StringComparison.Ordinal));
	}

	/// <summary>
	/// A grammar cut in two has no reader or stream form, binding or none: the refusal is the
	/// split's own (GRAM4026) and says nothing about the binding.
	/// </summary>
	[Fact]
	public void A_grammar_cut_in_two_refuses_a_stream_form_as_without_a_binding()
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
			parse R stream
			""";

		var told = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", Lexical = true, BufferedInput = true, BufferedBytes = true, CSharpScanner = RoslynCSharpScanner.Instance }).Diagnostics;

		Assert.DoesNotContain(told, static one => one.Id is GrammarNormalizer.BindingNotSupported or GrammarNormalizer.BindingOverKinds);
		Assert.Contains(told, static one => one.Id == GramCompiler.BufferedUnsupported && one.Message.Contains("token-kind input", StringComparison.Ordinal));
	}

	/// <summary>
	/// Why the rows above compare values only where the input begins with no trivia: a whole parse
	/// over characters keeps the leading trivia in a text value, and one over tokens does not. The
	/// same with no binding anywhere, so it is not the bindings' to answer.
	/// </summary>
	[Fact]
	public void LeadingTriviaInAValue()
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Word = ['a'..'z']+
			}
			Root = Lexical.Word
			parse Root
			""";

		var over = EmittedCode.Match(Compiled(grammar, CarrierKind.Auto, direct: true), "Grammar", "TryParseRoot", "  ab");
		var read = EmittedCode.Match(Lexical(grammar, direct: true).Assembly, "Grammar", "TryParseRoot", "  ab");

		Assert.True(over.IsSuccess && read.IsSuccess);
		Assert.Equal("  ab", over.Value);
		Assert.Equal("ab", read.Value);
	}

	// ── Helpers ─────────────────────────────────────────────────────────────────

	/// <summary>A row's grammar as the one terminal of a lexical namespace, published through a spaced root.</summary>
	static string Twin(string grammar, string rule)
	{
		return
			"trivia = { ' '* }\n" +
			"namespace Lexical\n{\n\ttrivia = none\n" + grammar + "\n}\n" +
			$"Root = Lexical.{rule}\nparse Root\n";
	}

	/// <summary>The grammar cut in two, read by the reader or the engine over the kinds; and its source.</summary>
	static (Assembly Assembly, string Source) Lexical(string grammar, bool direct)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Lexical       = true,
			Direct        = direct,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);
		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GramCompiler.NotCut);

		var source = result.Sources[0].Text;

		Assert.Contains("Tokenize_DotGram(", source, StringComparison.Ordinal);

		return (EmittedCode.Compile(source, sourceParts: result.Sources.Skip(1).Select(static one => one.Text)), source);
	}

	/// <summary>The binding refusals of a grammar asked to be cut in two, in the order told.</summary>
	static List<GramDiagnostic> Refusals(string grammar)
	{
		var told = GramCompiler.Compile(
			grammar + "\nparse R\n",
			new GramCompilerOptions { Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance }).Diagnostics;

		return [.. told.Where(static one => one.Id == GrammarNormalizer.BindingNotSupported)];
	}
}
