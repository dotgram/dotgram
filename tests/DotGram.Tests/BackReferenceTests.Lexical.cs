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
/// The split is taken only where it cannot change the answer: the body is atomic, and no other
/// token, nor trivia, can match a longer text that begins like the terminal; where the rule
/// refuses what the beginning began, the position is lexed again without the terminal.
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
	/// The rows the split declines with a word, read over characters: a beginning that can be
	/// nothing (a count by a length right after an optional run), and one the trivia between tokens
	/// can begin — a title, a fence or an item that may start with a space.
	/// </summary>
	static readonly Dictionary<string, string> Fallback = new(StringComparer.Ordinal)
	{
		["CountOfARepeatedBinding"]        = "can be nothing",
		["MarkdownFence"]                  = "trivia can begin",
		["MarkdownFenceOfEitherCharacter"] = "trivia can begin",
		["RestructuredTextHeading"]        = "trivia can begin",
		["SurrogatePairs"]                 = "trivia can begin",
		["AnyItemIsACodeUnit"]             = "trivia can begin",
	};

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

		if (Unbegun.Contains(name) || Fallback.ContainsKey(name))
		{
			var told = GramCompiler.Compile(twin, new GramCompilerOptions { Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance }).Diagnostics;

			if (Unbegun.Contains(name))
				Assert.Contains(told, static one => one.Id == GrammarNormalizer.BindingNotSupported && one.Message.Contains("nothing the lexer can read begins", StringComparison.Ordinal));
			else
				Assert.Contains(told, one => one.Id == GramCompiler.NotCut && one.Message.Contains(Fallback[name], StringComparison.Ordinal));

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
				Same(name, input, over, answer, how);
		}

		if (!BufferedTwins.Contains(name))
			return;

		var (buffered, bytes) = Buffered(twin, CarrierKind.Auto, direct: true);

		foreach (var input in row.Accepted.Concat(row.Refused))
			AgreesOverABuffer(name, buffered, "Root", input, "twin over characters", bytes);
	}

	/// <summary>
	/// The automaton reads the beginning of a terminal that binds text together with every other
	/// token, and longest match decides between the beginnings as it decides every token: a raw
	/// string's three quotes beside a plain string's, a heredoc's opening beside the operator it
	/// starts with, a long bracket beside a bracket. Where a beginning won and the rule refused,
	/// the position is lexed again without the terminal, and the plain string or the bracket stands.
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
				Raw     = { $q: '"'{3,} & (?!$q & any)* & $q & ?!'"' }
				Str     = '"' & [^ '"']* & '"'
				Heredoc = { "<<" & $tag: Name & eol & (?!($tag & EndLine) & Line)* & $tag & ?=EndLine }
				Long    = { '[' & $eq: '='* & '[' & (?!(']' & $eq & ']') & any)* & ']' & $eq & ']' }
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

		var (assembly, source) = Lexical(grammar, direct);
		var twin = Compiled(grammar, CarrierKind.Auto, direct: true);

		Assert.Contains("Scan_Unbound(", source, StringComparison.Ordinal);

		string Read(string input)
		{
			var answer = EmittedCode.Match(assembly, "Grammar", "TryParseItems", input);

			Same("competitors", input, EmittedCode.Match(twin, "Grammar", "TryParseItems", input), answer, direct ? "reader" : "engine");

			return answer.IsSuccess ? (string)answer.Value! : $"refused: {answer.Error} at {answer.Position}";
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

		// Four quotes begin a raw string to the automaton and the rule refuses them; read again
		// without it, they are two empty strings, as over characters. An unclosed heredoc is the
		// operator and the names, and an unclosed long bracket a bracket and then nothing.
		Assert.Equal("str:\"\"|str:\"\"", Read("\"\"\"\""));
		Assert.Equal("shift:a,end|name:body", Read("a <<end\nbody"));
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
				Quoted = { $q: '"'+ & ['a'..'z']* & $q }
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
	/// A token that can match a longer text than the beginning begins is a competitor the rule's
	/// reading could be cut below, and the split declines, naming it and the fix.
	/// </summary>
	[Fact]
	public void A_longer_token_that_starts_with_the_beginning_declines_the_split()
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Twice = { $q: "ab" & $q }
			}
			Item : @string = t: Lexical.Twice => @("twice:" + t) | "abc" => @("abc") | "abab" & 'x' => @("ababx")
			Items : @string = items: Item* => @(string.Join("|", items))
			parse Items
			""";

		var why = Declined(grammar);

		Assert.Contains("'Twice' binds text and '\"abc\"' can match a longer text that begins like 'Twice'", why, StringComparison.Ordinal);
		Assert.Contains("put both in one terminal as alternatives; this part is read over characters", why, StringComparison.Ordinal);
	}

	/// <summary>
	/// A competitor that matches the beginning's string, as long: the kind is both, and the rule
	/// decides. Longer than the string, the token is the terminal; refused, the other token stands,
	/// found by reading the position again; as long, the syntax asks for the one it wants, here the
	/// mark declared first.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_token_as_long_as_the_beginning_shares_its_kind_and_the_rule_decides(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Mark = "go"
				T    = { $q: ['g'..'o']+ & ('-' & $q | ?!'-') }
			}
			Item : @string = m: Lexical.Mark => @("kw") | t: Lexical.T => @("t:" + t) | '-' => @("-") | 'x' => @("x")
			Items : @string = items: Item* => @(string.Join("|", items))
			parse Items
			""";

		var (assembly, _) = Lexical(grammar, direct);
		var twin = Compiled(grammar, CarrierKind.Auto, direct: true);

		string Read(string input, bool held)
		{
			var answer = EmittedCode.Match(assembly, "Grammar", "TryParseItems", input);

			if (held)
				Same("tie", input, EmittedCode.Match(twin, "Grammar", "TryParseItems", input), answer, direct ? "reader" : "engine");

			return answer.IsSuccess ? (string)answer.Value! : "refused";
		}

		// Longer than the shared string: the terminal.
		Assert.Equal("t:go-go", Read("go-go", held: false));
		Assert.Equal("t:gog", Read("gog", held: false));

		// Refused past it: the mark, and what follows.
		Assert.Equal("kw|-|x", Read("go-x", held: true));

		// As long: both, and the mark is asked for first.
		Assert.Equal("kw", Read("go", held: true));
		Assert.Equal("kw|x|kw", Read("go x go", held: true));

		// Over characters the choice is ordered and `go-go` is the mark, a dash and the mark, and
		// `gog` the mark and a terminal of one letter: the one difference a regular token in the
		// terminal's place would make as well, longest match against the first alternative.
		Assert.Equal("kw|-|kw", (string)EmittedCode.Match(twin, "Grammar", "TryParseItems", "go-go").Value!);
		Assert.Equal("kw|t:g", (string)EmittedCode.Match(twin, "Grammar", "TryParseItems", "gog").Value!);
	}

	/// <summary>
	/// Two terminals that bind text and begin each other's beginnings: whichever won, the other
	/// could read longer, and the split declines. Beginnings neither of which begins the other's
	/// are split together.
	/// </summary>
	[Fact]
	public void Two_terminals_that_bind_text_must_begin_apart()
	{
		const string comparable =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				A = { $q: 'a' & $q & "aabbb" }
				B = { $r: "aa" & $r }
			}
			Item : @string = a: Lexical.A => @("A:" + a) | b: Lexical.B => @("B:" + b)
			Items : @string = items: Item* => @(string.Join("|", items))
			parse Items
			""";

		Assert.Contains("'A' and 'B' both bind text and one's beginning begins the other's; this part is read over characters", Declined(comparable), StringComparison.Ordinal);

		// Read over characters, the grammar is the one it would have been: `aaaabbb` is `A` alone.
		Assert.Equal("A:aaaabbb", (string)EmittedCode.Match(Compiled(comparable, CarrierKind.Auto, direct: true), "Grammar", "TryParseItems", "aaaabbb").Value!);

		const string apart =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Raw  = { $q: '"'{3,} & (?!$q & any)* & $q }
				Long = { '[' & $eq: '='* & '[' & (?!(']' & $eq & ']') & any)* & ']' & $eq & ']' }
			}
			Item : @string = r: Lexical.Raw => @("raw:" + r) | l: Lexical.Long => @("long:" + l)
			Items : @string = items: Item* => @(string.Join("|", items))
			parse Items
			""";

		var (assembly, _) = Lexical(apart, direct: true);

		Assert.Equal("raw:\"\"\"a\"\"\"|long:[=[b]=]", EmittedCode.Match(assembly, "Grammar", "TryParseItems", "\"\"\"a\"\"\" [=[b]=]").Value);
	}

	/// <summary>
	/// Two terminals that bind text and begin with the same string are two to measure at one
	/// place, and only one would be: they must begin apart, as long as well as longer, and the
	/// split declines; read over characters, both are read.
	/// </summary>
	[Fact]
	public void Two_terminals_that_bind_text_with_one_beginning_decline_the_split()
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				A = { $q: 'a' & 'b' & $q }
				B = { 'a' & $q: 'b' & $q }
			}
			Root : @string = a: Lexical.A => @("A:" + a) | b: Lexical.B => @("B:" + b)
			parse Root
			""";

		Assert.Contains("'A' and 'B' both bind text and one's beginning begins the other's", Declined(grammar), StringComparison.Ordinal);

		var twin = Compiled(grammar, CarrierKind.Auto, direct: true);

		Assert.Equal("A:aba", EmittedCode.Match(twin, "Grammar", "TryParseRoot", "aba").Value);
		Assert.Equal("B:abb", EmittedCode.Match(twin, "Grammar", "TryParseRoot", "abb").Value);
	}

	/// <summary>
	/// A keyword the position reads again without the terminal ends where the first automaton
	/// would have gone on into the terminal's beginning, so it is asked about the word boundary
	/// as any keyword is: `a` cannot stand right before `b`, and the input is refused as over
	/// characters.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_keyword_read_again_without_the_terminal_keeps_its_word_boundary(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			trivia = { ' '* }
			wordboundary = ['a'..'z']
			namespace Lexical
			{
				trivia = none
				T = { $q: 'a' & ['a'..'z'] & $q }
				B = 'b'
			}
			Root : @string = "a" & b: Lexical.B => @("a " + b) | t: Lexical.T => @("t:" + t)
			parse Root
			""";

		var (assembly, _) = Lexical(grammar, direct);
		var twin = Compiled(grammar, CarrierKind.Auto, direct: true);

		foreach (var input in new[] { "ab", "a b", "aab", "aba", "abab", "a" })
			Same("boundary", input, EmittedCode.Match(twin, "Grammar", "TryParseRoot", input), EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input), direct ? "reader" : "engine");

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseRoot", "ab").IsSuccess);
		Assert.Equal("a b", EmittedCode.Match(assembly, "Grammar", "TryParseRoot", "a b").Value);
		Assert.Equal("t:aba", EmittedCode.Match(assembly, "Grammar", "TryParseRoot", "aba").Value);
	}

	/// <summary>
	/// A terminal that binds text is read as one token only where its body is atomic: over
	/// characters the same rule could otherwise give a shorter reading back and read on another
	/// way, where the token's length is settled. Not atomic, the split declines and says so;
	/// atomic, both forms agree, the second alternative never tried after the first read.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_terminal_that_binds_text_is_one_token_only_where_it_is_atomic(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		const string open =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T = $q: ('a' & 'b')+ & ($q & ':')* & $q | 'a'+ & ['a'..'b']*
			}
			Item : @string = t: Lexical.T => @("t:" + t)
			Items : @string = items: Item* => @(string.Join("|", items))
			parse Items
			""";

		Assert.Contains("'T' binds text, so write `{ }` around its body to read it as one token; this part is read over characters", Declined(open), StringComparison.Ordinal);

		const string atomic =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T = { $q: ('a' & 'b')+ & ($q & ':')* & $q | 'a'+ & ['a'..'b']* }
			}
			Item : @string = t: Lexical.T => @("t:" + t)
			Items : @string = items: Item* => @(string.Join("|", items))
			parse Items
			""";

		var (assembly, _) = Lexical(atomic, direct);
		var twin = Compiled(atomic, CarrierKind.Auto, direct: true);

		foreach (var input in new[] { "ababb", "abab", "abab:abab", "aab", "ab ab" })
			Same("atomic", input, EmittedCode.Match(twin, "Grammar", "TryParseItems", input), EmittedCode.Match(assembly, "Grammar", "TryParseItems", input), direct ? "reader" : "engine");
	}

	/// <summary>
	/// A terminal whose beginning spells other tokens — an identifier and a mark — is begun where
	/// they would stand, and where its rule refuses the position is read again without it: the
	/// identifier and the mark, as over characters. Whole, from a position, and inside a window.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void Where_the_rule_refuses_the_position_is_read_again_without_the_terminal(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T  = { $q: ['a'..'z']+ & ':' & $q }
				Id = ['a'..'z']+
			}
			Item : @string = t: Lexical.T => @("t:" + t) | i: Lexical.Id => @("id:" + i) | ':' => @(":")
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";

		var (assembly, _) = Lexical(grammar, direct);
		var twin = Compiled(grammar, CarrierKind.Auto, direct: true);
		var how  = direct ? "reader" : "engine";

		foreach (var input in new[] { "ab:ab", "ab:ac", "ab:", "a:b", ":ab", "ab:ab ab:", "ab: ab:ab" })
		{
			Same("relex", input, EmittedCode.Match(twin, "Grammar", "TryParseRoot", input), EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input), how);

			// Inside a window, at the start of a longer text and after a copy of itself: no end is
			// demanded, so a reading stops where the items stop, and both forms stop alike. (A
			// reading from a position alone is cut into tokens from the text's start over kinds,
			// binding or none: <see cref="AReadingFromAPositionOverKinds"/>.)
			var doubled = input + input;

			Assert.Equal(EmittedCode.Answered(twin, "Grammar", "TryParseRoot", doubled, input.Length, input.Length), EmittedCode.Answered(assembly, "Grammar", "TryParseRoot", doubled, input.Length, input.Length));
			Assert.Equal(EmittedCode.Answered(twin, "Grammar", "TryParseRoot", doubled, 0, input.Length), EmittedCode.Answered(assembly, "Grammar", "TryParseRoot", doubled, 0, input.Length));
		}

		Assert.Equal("id:ab|:|id:ac", EmittedCode.Match(assembly, "Grammar", "TryParseRoot", "ab:ac").Value);
	}

	/// <summary>
	/// The forms the section is written for, each beside the token its beginning starts like — a
	/// long bracket beside an index, a heredoc beside a shift, a Rust and a C++ raw string beside
	/// names — are all split, and an unclosed one is read again as those tokens, as over characters.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void The_forms_of_the_section_are_split_beside_the_tokens_they_start_like(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			trivia = { (' ' | '\n')* }
			namespace Lexical
			{
				trivia  = none
				Long    = { '[' & $eq: '='* & '[' & (?!(']' & $eq & ']') & any)* & ']' & $eq & ']' }
				Heredoc = { "<<" & $tag: Name & eol & (?!($tag & EndLine) & Line)* & $tag & ?=EndLine }
				Rust    = { 'r' & $h: '#'* & '"' & (?!('"' & $h) & any)* & '"' & $h }
				Cpp     = { 'R' & '"' & $d: [^ '(' | ')' | ' ' | '\\' | '"' | '\n']{0,16} & '(' & (?!(')' & $d & '"') & any)* & ')' & $d & '"' }
				Name    = ['a'..'z' | 'A'..'Z' | '_']+
				Line    = [^ '\r' | '\n']+ & EndLine | eol
				EndLine = eol | eof
			}
			Item : @string
				= l: Lexical.Long                          => @("long:" + l)
				| h: Lexical.Heredoc                       => @("doc:" + h)
				| r: Lexical.Rust                          => @("rust:" + r)
				| c: Lexical.Cpp                           => @("cpp:" + c)
				| '[' & i: Lexical.Name & ']'              => @("index:" + i)
				| a: Lexical.Name & "<<" & b: Lexical.Name => @("shift:" + a + "," + b)
				| n: Lexical.Name                          => @("name:" + n)
				| '#'                                      => @("#")
				| '"'                                      => @("quote")
			Items : @string = items: Item* => @(string.Join("|", items))
			parse Items
			""";

		var (assembly, source) = Lexical(grammar, direct);
		var twin = Compiled(grammar, CarrierKind.Auto, direct: true);

		Assert.Contains("Scan_Unbound(", source, StringComparison.Ordinal);

		foreach (var input in new[]
		{
			"[==[a]]b]=]c]==] [x] [[]]", "[==[abc", "[=[a]==]",
			"<<eof\nbody\neof\nx", "a <<b c", "a <<end\nbody", "<<-end\nx",
			"r\"a\" r#\"a\"b\"# r", "r#\"abc", "r# r",
			"R\"d(a)b)d\" R\"(x)\"", "R\"d(abc", "R\"",
			"[[x]] <<t\nx\nt\nr\"\" R\"()\"",
		})
			Same("forms", input, EmittedCode.Match(twin, "Grammar", "TryParseItems", input), EmittedCode.Match(assembly, "Grammar", "TryParseItems", input), direct ? "reader" : "engine");

		// An unclosed Rust raw string is a name, a mark, a quote and names: the position was read
		// again without the terminal, for both.
		Assert.Equal("index:x|name:r|#|quote|name:abc|quote|name:x", EmittedCode.Match(assembly, "Grammar", "TryParseItems", "[x] r#\"abc\" x").Value);
	}

	/// <summary>
	/// Trivia is skipped before every token, so it may neither begin where a terminal that binds
	/// text begins nor read on past its beginning: a comment to the end of the line beside a form
	/// that opens with the same character declines the split.
	/// </summary>
	[Fact]
	public void Trivia_that_can_begin_where_the_terminal_begins_declines_the_split()
	{
		const string grammar =
			"""
			trivia = { (' ' | '#' & [^ '\n']*)* }
			namespace Lexical
			{
				trivia = none
				Hashed = { '#' & $q: '"'+ & (?!$q & any)* & $q & '#' }
				Name   = ['a'..'z']+
			}
			Item : @string = h: Lexical.Hashed => @("h:" + h) | n: Lexical.Name => @("n:" + n)
			Items : @string = items: Item* => @(string.Join("|", items))
			parse Items
			""";

		Assert.Contains("'Hashed' binds text and trivia can begin where 'Hashed' begins", Declined(grammar), StringComparison.Ordinal);
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
				Heredoc : @string = { "<<" & $tag: Name & eol & body: (?!($tag & EndLine) & Line)* & $tag & ?=EndLine } => @(body)
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
	/// elements around it are read — also where the terminal's beginning won and its rule refused,
	/// the position then read again as the identifier and the mark. The engine alone: over kinds
	/// a recovery is not read by methods (GRAM5005), binding or none.
	/// </summary>
	[Fact]
	public void A_recovering_repetition_over_tokens_reads_a_terminal_that_binds()
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T  = { $q: ['a'..'z']+ & ':' & $q }
				Id = ['a'..'z']+
			}
			Item  : @string = t: Lexical.T & ';' => @(t) | i: Lexical.Id & ';' => @("id:" + i) | ':' & ';' => @(":")
			Items : @string = items: Item* recover ';' & eof => @(string.Join(",", items))
			parse Items
			""";

		var (assembly, _) = Lexical(grammar, direct: false);
		var twin = Compiled(grammar, CarrierKind.Auto, direct: false);

		foreach (var input in new[] { "ab:ab; cd:cd;", "ab:ab; cd:; ef:ef;", "ab:; x;", "ab:ab; cd : cd; ef:ef;" })
			Same("recover", input, EmittedCode.Match(twin, "Grammar", "TryParseItems", input), EmittedCode.Match(assembly, "Grammar", "TryParseItems", input), "engine");

		Assert.Equal("ab:ab,ef:ef", EmittedCode.Match(assembly, "Grammar", "TryParseItems", "ab:ab; cd:; ef:ef;").Value);
		Assert.Equal("ab:ab,id:cd,ef:ef", EmittedCode.Match(assembly, "Grammar", "TryParseItems", "ab:ab; cd; ef:ef;").Value);
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
				Str    = { '"' & Body & '"' }
				Body   = $q: ['a'..'z']+ & '-' & $q
				Pair   = { '<' & $a: Name & '>' & $a | '{' & $b: Name & '}' & $b }
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
				Looked = { ?!'x' & $q: ['a'..'z']+ & '-' & $q }
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
				Outer  = { Inner & '!' }
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
				Quoted = { $q: '"'+ & ['a'..'z']* & $q }
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
				Quoted = { $q: '"'+ & ['a'..'z']* & $q }
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

	/// <summary>
	/// Why the readings from a position above are taken inside a window: over kinds a reading from
	/// a position alone cuts the whole text into tokens first, and a position inside a token is no
	/// token's start — a name that spans it is not read from its middle, as it is over characters,
	/// and the reading is refused. The same with no binding anywhere, so it is not the bindings' to
	/// answer.
	/// </summary>
	[Fact]
	public void AReadingFromAPositionOverKinds()
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Word = ['a'..'z']+
			}
			Item : @string = w: Lexical.Word => @(w)
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";

		var over = EmittedCode.Answered(Compiled(grammar, CarrierKind.Auto, direct: true), "Grammar", "TryParseRoot", "abab", 2);
		var read = EmittedCode.Answered(Lexical(grammar, direct: true).Assembly, "Grammar", "TryParseRoot", "abab", 2);

		Assert.Equal((true, "ab"), (over.Read, (string?)over.Value));
		Assert.Equal((false, null), (read.Read, (string?)read.Value));
	}

	/// <summary>
	/// A diamond of rules that bind — each a choice of two that call the next — is cut in time
	/// linear in its depth: the beginning of a rule is remembered, and a choice of two alternatives
	/// that begin as one rule does begins as that rule does once. Set the environment variable to
	/// have the times written beside the test's temporary files.
	/// </summary>
	[Fact]
	public void A_diamond_of_rules_that_bind_is_cut_in_bounded_time()
	{
		static string Diamond(int depth)
		{
			var rules = new System.Text.StringBuilder();

			for (var k = 0; k < depth; k++)
			{
				rules.Append(k == 0 ? $"\tR{k} = {{ $q{k}: (A{k + 1} | B{k + 1}) & $q{k} }}\n" : $"\tR{k} = $q{k}: (A{k + 1} | B{k + 1}) & $q{k}\n");
				rules.Append($"\tA{k + 1} = R{k + 1} & 'a'\n");
				rules.Append($"\tB{k + 1} = R{k + 1} & 'b'\n");
			}

			rules.Append($"\tR{depth} = 'x' | 'y'\n");

			return "trivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n" + rules +
				"}\nItem : @string = t: Lexical.R0 => @(t)\nRoot : @string = items: Item* => @(string.Join(\"|\", items))\nparse Root\n";
		}

		var told = new System.Text.StringBuilder();

		foreach (var depth in new[] { 18, 24 })
		{
			var clock = System.Diagnostics.Stopwatch.StartNew();
			var cut   = GramCompiler.Compile(Diamond(depth), new GramCompilerOptions { Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance });
			var took  = clock.ElapsedMilliseconds;

			Assert.Contains("Tokenize_DotGram(", cut.Sources[0].Text, StringComparison.Ordinal);
			Assert.True(depth > 18 || took < 1000, $"a diamond of {depth} took {took} ms to cut");

			told.AppendLine($"diamond {depth}: {took} ms");
		}

		if (Environment.GetEnvironmentVariable("DOTGRAM_LEXTIMING") is { } where)
			System.IO.File.WriteAllText(where, told.ToString());
	}

	// ── Helpers ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// A row's grammar as the one terminal of a lexical namespace, atomic as a token has to be,
	/// published through a spaced root.
	/// </summary>
	static string Twin(string grammar, string rule)
	{
		return
			"trivia = { ' '* }\n" +
			"namespace Lexical\n{\n\ttrivia = none\n" + grammar + $"\n\tToken = {{ {rule} }}\n}}\n" +
			"Root = Lexical.Token\nparse Root\n";
	}

	/// <summary>
	/// Two readings of one input said the same: the answer and the value, and where it was read,
	/// the position. The words of a refusal name kinds over tokens and characters over characters,
	/// so where both refused, a difference in them or in the place is written out, not asserted;
	/// the value is compared only where the input begins with no trivia (<see cref="LeadingTriviaInAValue"/>).
	/// </summary>
	static void Same(
		string name,
		string input,
		(bool IsSuccess, object? Value, string? Error, long Position) over,
		(bool IsSuccess, object? Value, string? Error, long Position) answer,
		string how)
	{
		var valued = !input.StartsWith(' ');

		Assert.True(
			(over.IsSuccess, valued ? Spelled(over.Value) : null) == (answer.IsSuccess, valued ? Spelled(answer.Value) : null),
			$"{name}: \"{Escaped(input)}\"\n  over characters: {over}\n  {how}: {answer}");

		if (over.IsSuccess)
			Assert.Equal(over.Position, answer.Position);
		else if (over.Position != answer.Position || over.Error != answer.Error)
			TestContext.Current.TestOutputHelper?.WriteLine($"{name}: \"{Escaped(input)}\" refused apart\n  over characters: {over}\n  {how}: {answer}");
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

	/// <summary>Why a grammar asked to be cut in two was read over characters instead, as GRAM5004 says it.</summary>
	static string Declined(string grammar)
	{
		var told = GramCompiler.Compile(grammar, new GramCompilerOptions { Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance }).Diagnostics;

		Assert.DoesNotContain(told, static one => one.Severity == GramSeverity.Error);

		var declined = told.FirstOrDefault(static one => one.Id == GramCompiler.NotCut);

		Assert.True(declined is not null, "The grammar was cut in two.");

		return declined!.Message;
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
