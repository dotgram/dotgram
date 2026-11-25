using System;
using System.Collections;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Generation;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A rule typed by another rule that gives text, an extent or an array, with more than one alternative.
/// </summary>
/// <remarks>
/// <para>
/// <c>Name : Quoted = Quoted | Bare</c> says that <c>Name</c> has the value of whichever operand matched.
/// Each alternative has to hand that operand back, and an operand is one that has a value: a rule that
/// builds one, and also a rule that builds nothing and captures nothing, whose value is the extent it
/// matched. The extent was not counted, so one alternative with such a rule handed back nothing, the
/// rewrite was dropped for the whole rule, and the generated file built <c>new string()</c> for it
/// (CS1729) or called a method with an argument too many (CS1501).
/// </para>
/// </remarks>
public sealed class RuleTypedByTextTests
{
	const string Quoted  = "\nQuoted : @string = '[' & b: Bare & ']' => @(b)";
	const string Words   = "\nBare : @string = ['a'..'z']+";
	const string Plain   = "\nBare = ['a'..'z']+";
	const string Letters = "\nPlain = ['a'..'z']+";

	[Theory]
	[InlineData("Start : Quoted = Quoted | Bare" + Quoted + Words, "[abc]", "abc")]
	[InlineData("Start : Quoted = Quoted | Bare" + Quoted + Words, "abc", "abc")]
	[InlineData("Start : Quoted = Bare | Quoted" + Quoted + Words, "[abc]", "abc")]
	[InlineData("Start : Quoted = Quoted | Word" + Quoted + Words + "\nWord : @string = ['a'..'z']+", "abc", "abc")]
	[InlineData("Start : Quoted = Quoted | Bare | '-' & Bare" + Quoted + Words, "-abc", "abc")]
	[InlineData("Start : Quoted = Quoted" + Quoted + Words, "[abc]", "abc")]

	// Neither operand builds a value, and each declares text: what an alternative says is the operand's, not its own extent.
	[InlineData("Start : Bare = '<' & Bare & '>' | Bare" + Words, "<abc>", "abc")]
	[InlineData("Start : Bare = '<' & Bare & '>' | Bare" + Words, "abc", "abc")]
	[InlineData("Start : Bare = '<' & Bare & '>'" + Words, "<abc>", "abc")]
	[InlineData("Start : Bare = Bare | Other" + Words + "\nOther : @string = ['0'..'9']+", "123", "123")]
	[InlineData("Start : Bare = '(' & Bare & ')' | Bare" + Words, "(abc)", "abc")]

	// Written in two branches of a group, of which one runs: one operand on every route.
	[InlineData("Start : Bare = '-'? & ('<' & Bare & '>' | Bare)" + Words, "-<abc>", "abc")]
	[InlineData("Start : Bare = '-'? & ('<' & Bare & '>' | Bare)" + Words, "abc", "abc")]

	// A repetition that never runs is nobody's operand.
	[InlineData("Start : Bare = '<' & Bare{0} & Bare & '>'" + Words, "<abc>", "abc")]

	// Through a rule that is itself typed by a rule.
	[InlineData("Start : Mid = Mid | Bare\nMid : Bare = Bare | '!' & Bare" + Words, "!abc", "abc")]
	[InlineData("Start : Mid = Mid | Bare\nMid : Bare = Bare | '!' & Bare" + Words, "abc", "abc")]
	public void Gives_the_text_of_the_operand_that_matched(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Parsed(grammar, input));
	}

	/// <summary>
	/// Operands that declare nothing say nothing of their value, so the rule's is case 4's: the extent it
	/// matched, however many alternatives it has.
	/// </summary>
	[Theory]
	[InlineData("Start : Bare = Bare | Other" + Plain + "\nOther = ['0'..'9']+", "123", "123")]
	[InlineData("Start : Bare = '<' & Bare & '>' | Bare" + Plain, "<abc>", "<abc>")]
	[InlineData("Start : Bare = '<' & Bare & '>' | Bare" + Plain, "abc", "abc")]
	[InlineData("Start : Mid = Mid\nMid : Bare = '!' & Bare | Bare" + Plain, "!abc", "!abc")]
	public void Gives_the_extent_where_no_operand_declares_a_value(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Parsed(grammar, input));
	}

	/// <summary>
	/// Some alternatives have an operand that produces the value and some have none, so there is no one value
	/// to give: told on the rule, not left to the C# compiler.
	/// </summary>
	[Theory]
	[InlineData("Start : Quoted = Quoted | Bare" + Quoted + Plain)]
	[InlineData("Start : Quoted = Quoted | Word" + Quoted + Words + "\nWord = ['a'..'z']+")]
	[InlineData("Start : Quoted = Quoted | Bare & Bare" + Quoted + Words)]
	[InlineData("Start : Mid = Mid | Bare\nMid : Bare = Bare | '!' & Bare" + Plain)]
	[InlineData("Start : Span = Span | Digits\nSpan : @SourceSpan = ['a'..'z']+\nDigits = ['0'..'9']+")]

	// An operand that is not one on every route: several, repeated, optional, or written in two branches of a group.
	[InlineData("Start : Bare = Bare & ' ' & Bare" + Words)]
	[InlineData("Start : Bare = (Bare & ',')+" + Words)]
	[InlineData("Start : Bare = '<' & (Bare & ';')* recover ';' & '>'" + Words)]
	[InlineData("Start : Bare = Bare{2}" + Words)]
	[InlineData("Start : Bare = '<' & Bare? & '>'" + Words)]
	[InlineData("Start : Span = Span & ' ' & Span\nSpan : @SourceSpan = ['a'..'z']+")]

	// A left-recursive rule says how it is built with a '=>' of its own.
	[InlineData("Start : Bare = Start & '+' & Bare | Bare" + Words)]
	[InlineData("Start : Bare = Start & '+' & Bare | Bare" + Plain)]
	[InlineData("Start : Span = Start & '+' & Span | Span\nSpan : @SourceSpan = ['a'..'z']+")]
	public void Is_told_where_the_alternatives_do_not_each_give_the_value(string grammar)
	{
		var result = Compile(grammar + "\nparse Start");
		var found  = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.UnbuiltConstruction, found.Id);
		Assert.Contains("'Start' takes the type of the rule it names", found.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A name speaks for the operand it names, so the declared type is not given up for the extent where an operand
	/// is named: it is refused, and the rule does not become a generated type.
	/// </summary>
	[Theory]
	[InlineData("Start : Bare = '<' & x: Bare & '>'" + Words)]
	[InlineData("Start : Bare = '<' & x: Bare & '>' | '[' & x: Bare & ']'" + Words)]
	public void A_capture_without_a_construction_does_not_change_the_type(string grammar)
	{
		var result = Compile(grammar + "\nparse Start");
		var found  = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.UnbuiltConstruction, found.Id);
	}

	/// <summary>
	/// A left-recursive rule that declares the type of its extent gives its extent, as one that is not left-recursive
	/// does: the text for <c>: @string</c>, and the bounds for <c>: @SourceSpan</c>.
	/// </summary>
	[Theory]
	[InlineData("Start : @SourceSpan = Start & '+' & Plain | Plain" + Letters, "a+bc", 4)]
	[InlineData("Start : @SourceSpan = Start & '+' & Span | Span\nSpan : @SourceSpan = ['a'..'z']+", "a+bc", 4)]
	[InlineData("Start : @SourceSpan = '(' & Start & ')' | Start & '+' & Plain | Plain" + Letters, "(a+b)+c", 7)]
	[InlineData("Start : Expr = '(' & Expr & ')'\nExpr : @SourceSpan = Expr & '+' & Plain | Plain" + Letters, "(a+b)", 3)]
	public void A_left_recursive_SourceSpan_rule_gives_its_extent(string grammar, string input, int length)
	{
		var value = Parsed(grammar, input, kind: true)!;

		Assert.Equal(length, (int)value.GetType().GetProperty("Length")!.GetValue(value)!);
	}

	[Fact]
	public void A_left_recursive_text_rule_gives_its_extent()
	{
		Assert.Equal("a+b", Parsed("Start : @string = Start & '+' & Plain | Plain" + Letters, "a+b"));
	}

	/// <summary>
	/// A rule that takes another's type and is left-recursive says which operand it hands on with a <c>=&gt;</c>
	/// of its own, and says so for every type.
	/// </summary>
	[Theory]
	[InlineData("Start : Num = Start & '+' & Num | Num\nNum : @int = ['0'..'9']+ => @(int.Parse(parserText))")]
	[InlineData("Start : Bare = Start & '+' & Plain | Plain" + Words + Letters)]
	public void A_left_recursive_rule_typed_by_a_rule_is_told_why(string grammar)
	{
		var result = Compile(grammar + "\nparse Start");
		var found  = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Contains("'=>' of its own to say", found.Message, StringComparison.Ordinal);
	}

	/// <summary>
	/// The route that has none or several is told for every type, and the advice is not to declare text where the
	/// type is a number or a class.
	/// </summary>
	[Theory]
	[InlineData("Start : Num = '<' & Num? & '>'\nNum : @int = ['0'..'9']+ => @(int.Parse(parserText))", "may have none")]
	[InlineData("Start : Num = Num+\nNum : @int = ['0'..'9'] => @(int.Parse(parserText))", "may have more than one")]
	[InlineData("Start : Item = '<' & Item? & '>'\nItem = x: ['a'..'z']+", "may have none")]
	public void A_non_text_operand_that_is_not_one_is_told_why(string grammar, string why)
	{
		var result = Compile(grammar + "\nparse Start");
		var found  = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Contains(why, found.Message, StringComparison.Ordinal);
		Assert.DoesNotContain("as ': @string'", found.Message, StringComparison.Ordinal);
	}

	[Fact]
	public void A_repetition_of_nought_is_not_more_than_one()
	{
		var result = Compile("Start : Bare = '<' & Bare{0} & Plain & '>'" + Words + Letters + "\nparse Start");

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);
	}

	/// <summary>
	/// The operand is held by a capture of the rule's own making, which no name the author wrote can be: a capture
	/// named like it, <c>item0</c> or <c>items</c>, is the author's and is not what the rule hands back.
	/// </summary>
	[Theory]
	[InlineData("Start : Bare = item0: '<' & Bare & '>'" + Words, "<abc>")]
	[InlineData("Start : Bare = '<' & Bare & items: '>'" + Words, "<abc>")]
	[InlineData("Start : Bare = '<' & Bare & item: '>'" + Words, "<abc>")]
	[InlineData("Start : Bare = '<' & Bare & item1: '>' | Bare" + Words, "<abc>")]
	[InlineData("Start : Quoted = Quoted | '!' & Bare & items: '!'" + Quoted + Words, "!abc!")]
	[InlineData("Start : Num = '<' & Num & item0: '>'\nNum : @int = ['0'..'9']+ => @(int.Parse(parserText))", "<1>")]
	public void A_capture_the_author_names_like_the_operand_is_not_the_operand(string grammar, string input)
	{
		Assert.Contains(Convert.ToString(Parsed(grammar, input, kind: true)), new[] { "abc", "1" });
	}

	/// <summary>A name on what is no operand, a rule that declared nothing, adds nothing: the rule is the extent it matched.</summary>
	[Fact]
	public void A_name_on_what_declared_no_type_is_ignored()
	{
		Assert.Equal("<abc>", Parsed("Start : Bare = '<' & x: Bare & '>'" + Plain, "<abc>"));
	}

	/// <summary>
	/// A group that has a rule that builds the value in one branch and a rule that only matched it in another is
	/// one operand on each route: what the route that ran read is what is handed back.
	/// </summary>
	[Theory]
	[InlineData("-<abc>")]
	[InlineData("abc")]
	public void A_group_of_a_built_operand_and_a_matched_one_gives_the_operand(string input)
	{
		Assert.Equal("abc", Parsed("Start : Bare = '-'? & ('<' & Quoted & '>' | Bare)\nQuoted : @string = x: ['a'..'z']+ => @(x)" + Words, input));
	}

	/// <summary>
	/// What a rule hands back from a text operand is the same in every mode that reads characters: a capture is a
	/// span under <c>UseSpanCaptures</c>, and the string it is handed back as is made of it.
	/// </summary>
	[Theory]
	[InlineData("Start : Bare = '<' & Bare & '>'", "<abc>")]
	[InlineData("Start : Bare = x: '<' & Bare & '>'", "<abc>")]
	[InlineData("Start : Bare = '<' & Bare & items: '>'", "<abc>")]
	[InlineData("Start : Bare = '-'? & ('<' & Bare & '>' | Bare)", "-<abc>")]
	[InlineData("Start : Bare = '-'? & ('<' & Bare & '>' | Bare)", "abc")]
	[InlineData("Start : Word = '-'? & ('<' & Word & '>' | Bare)\nWord : @string = c: ['a'..'z']+ => @(c.ToString())", "-<abc>")]
	[InlineData("Start : Word = '-'? & ('<' & Word & '>' | Bare)\nWord : @string = c: ['a'..'z']+ => @(c.ToString())", "abc")]
	[InlineData("Start : Quoted = Quoted | Bare\nQuoted : @string = '[' & b: Bare & ']' => @(b.ToString())", "[abc]")]
	[InlineData("Start : Quoted = Quoted | Bare\nQuoted : @string = '[' & b: Bare & ']' => @(b.ToString())", "abc")]
	public void A_text_operand_gives_its_text_under_span_captures(string grammar, string input)
	{
		foreach (var option in new[] { "", ", UseSpanCaptures = true" })
		{
			var type = GeneratorDriverTests.Build(
				"[DotGram.Gram(\"" + grammar.Replace("\n", "\\n") + "\\nBare : @string = ['a'..'z']+\\nparse Start\"" + option + ")]\npublic partial class Spanned { }")
				.GetType("Spanned")!;

			Assert.Equal("abc", (string)type.GetMethod("ParseStart", [typeof(string)])!.Invoke(null, [input])!);
		}
	}

	/// <summary>
	/// Over bytes a raw capture is a span that nothing decodes (§6.3), so a rule that hands back a text operand is
	/// refused there, in the grammar's terms, as a type made of a raw capture is.
	/// </summary>
	[Theory]
	[InlineData("Start : Bare = '<' & Bare & '>'")]
	[InlineData("Start : Bare = x: '<' & Bare & '>'")]
	[InlineData("Start : Bare = '-'? & ('<' & Bare & '>' | Bare)")]

	// Each route of a rule has a construction of its own, and the refusal is of every one: a built operand first and
	// a raw text one after it.
	[InlineData("Start : Quoted = Quoted | Bare\nQuoted : @string = '[' & b: Bare & ']' => @(b.ToString())")]
	[InlineData("Start : Word = Word | Bare\nWord : @string = c: ['a'..'z'] => @(c.ToString())")]
	[InlineData("Start : Word = Word | '!' & Bare\nWord : @string = c: ['a'..'z'] => @(c.ToString())")]
	public void A_text_operand_over_bytes_is_refused(string grammar)
	{
		var run   = GeneratorDriverTests.RunGenerator(
			"[DotGram.Gram(\"" + grammar.Replace("\n", "\\n") + "\\nBare : @string = ['a'..'z']+\\nparse Start\\nparse Start as Bytes stream bytes\")]\npublic partial class Spanned { }");
		var found = Assert.Single(run.Diagnostics, static one => one.Severity == DiagnosticSeverity.Error);

		var message = found.GetMessage();

		Assert.Equal("GRAM4026", found.Id);

		// The operand's rule is the name the grammar has for it; the capture the compiler made of it is nobody's.
		Assert.Contains("hands back the text of 'Bare'", message, StringComparison.Ordinal);
		Assert.DoesNotContain("item", message, StringComparison.Ordinal);
	}

	/// <summary>
	/// A local the generated factory declares is not one an author's capture may take: <c>count</c>, <c>items</c>
	/// and <c>at</c> are names a capture is called.
	/// </summary>
	[Theory]
	[InlineData("items")]
	[InlineData("count")]
	[InlineData("at")]
	[InlineData("item0")]
	public void A_capture_beside_a_collected_sequence_may_have_any_name(string name)
	{
		var result = Compile("Start : @string[] = Word+ & " + name + ": ';'\nWord : @string = c: ['a'..'z'] => @(c)\nparse Start");

		EmittedCode.Quiet(result.Diagnostics);

		var (isSuccess, value, error, _) = EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text)),
			"Grammar",
			"TryParseStart",
			"ab;");

		Assert.True(isSuccess, error);
		Assert.Equal(["a", "b"], (string[])value!);
	}

	/// <summary>
	/// A name is never part of an implicit value (§4.1): what a sequence collects is its operands, so a capture the author
	/// wrote of anything else, a literal's text or the bounds of an extent, adds nothing to the array, whatever the
	/// element type is and whatever the name is.
	/// </summary>
	[Theory]
	[InlineData("Start : @int[] = Num & x: ',' & Num\nNum : @int = ['0'..'9'] => @(int.Parse(parserText))", "1,2", "1;2")]
	[InlineData("Start : @int[] = Num & item0: ',' & Num\nNum : @int = ['0'..'9'] => @(int.Parse(parserText))", "1,2", "1;2")]
	[InlineData("Start : @string[] = x: '[' & Word+ & ']'\nWord : @string = c: ['a'..'z']+ => @(c.ToString())", "[ab]", "ab")]
	[InlineData("Start : @string[] = Word & (x: ',' & Word)*\nWord : @string = c: ['a'..'z']+ => @(c.ToString())", "ab,cd,ef", "ab;cd;ef")]
	[InlineData("Start : @string[] = Word & x: ';'?\nWord : @string = c: ['a'..'z']+ => @(c.ToString())", "ab;", "ab")]
	public void A_name_beside_a_sequence_adds_nothing_to_it(string grammar, string input, string expected)
	{
		var value = (System.Collections.IEnumerable)Parsed(grammar, input, kind: true)!;

		Assert.Equal(expected, string.Join(";", value.Cast<object>()));
	}

	[Fact]
	public void A_name_beside_a_sequence_of_bounds_adds_nothing_to_it()
	{
		var value = (System.Collections.IEnumerable)Parsed("Start : @SourceSpan[] = Span & x: ';'\nSpan : @SourceSpan = ['a'..'z']+", "ab;", kind: true)!;

		Assert.Single(value.Cast<object>());
	}

	/// <summary>
	/// A name speaks for what it names (§4.1): an operand that is named is no longer one the implicit value is made of, and
	/// where that leaves nothing the rule is told, with the name.
	/// </summary>
	[Theory]
	[InlineData("Start : @string[] = rows: Word+\nWord : @string = c: ['a'..'z']+ => @(c.ToString())")]
	[InlineData("Start : Bare = '<' & b: Bare & '>'\nBare : @string = ['a'..'z']+")]
	[InlineData("Start : Num = '<' & n: Num & '>'\nNum : @int = ['0'..'9']+ => @(int.Parse(parserText))")]
	public void A_named_operand_leaves_the_implicit_value_and_the_rule_is_told(string grammar)
	{
		var result = Compile(grammar + "\nparse Start");
		var found  = result.Diagnostics.First(static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.UnbuiltConstruction, found.Id);
		Assert.Contains("A name speaks for what it names", found.Message, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("Start : @string[] = w: Word & Word\nWord : @string = c: ['a'..'z'] => @(c.ToString())", "ab", "b")]
	[InlineData("Start : @int[] = n: Num & Num\nNum : @int = ['0'..'9'] => @(int.Parse(parserText))", "12", "2")]
	public void A_named_operand_beside_another_is_not_collected(string grammar, string input, string expected)
	{
		var value = (System.Collections.IEnumerable)Parsed(grammar, input, kind: true)!;

		Assert.Equal(expected, string.Join(";", value.Cast<object>()));
	}

	/// <summary>A name on something that is no operand, in a rule that declared the type of its extent, adds nothing to the extent.</summary>
	[Fact]
	public void A_name_in_a_rule_of_an_extent_adds_nothing_to_it()
	{
		Assert.Equal("abc", Parsed("Start : @string = x: Letters\nLetters = ['a'..'z']+", "abc"));
	}

	[Fact]
	public void A_capture_the_sequence_does_not_hold_is_left_out_by_dropping_the_name()
	{
		var value = (int[])Parsed("Start : @int[] = Num & ',' & Num\nNum : @int = ['0'..'9'] => @(int.Parse(parserText))", "1,2", kind: true)!;

		Assert.Equal([1, 2], value);
	}

	/// <summary>The local a located factory declares is not one an author's capture may take.</summary>
	[Fact]
	public void A_capture_named_made_in_a_located_rule_builds()
	{
		const string members = """
			public interface ILoc { void Locate(int at, int length); }
			public sealed class Loc : ILoc { public string S = ""; public int At = -1; public void Locate(int at, int length) { At = at; } }
			""";

		var host = CSharpCompilation.Create(
			"Host",
			[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {members} }}", cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References);

		var result = GramCompiler.Compile(
			"Root : @Loc = made: ['a'..'z']+ => @(new Loc { S = made })\nparse Root",
			new GramCompilerOptions
			{
				ClassName      = "Grammar",
				CSharpScanner  = RoslynCSharpScanner.Instance,
				SymbolResolver = new RoslynSymbolResolver(host, "Grammar"),
				LocationType   = "Grammar.ILoc",
			});

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		EmittedCode.Compile(result.Sources[0].Text, declarationMembers: members, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	/// <summary>
	/// One grammar is one value whether captures are strings or spans: an optional capture beside what a sequence
	/// collects is left out where it was not read, and the machine hands an absent one over as the span that stands
	/// for none, which an empty one read from the input is not.
	/// </summary>
	[Theory]
	[InlineData("ab", "ab")]
	[InlineData("ab;", "ab")]
	public void An_absent_optional_capture_beside_a_sequence_is_the_same_in_every_mode(string input, string expected)
	{
		foreach (var option in new[] { "", ", UseSpanCaptures = true" })
		{
			var type = GeneratorDriverTests.Build(
				"[DotGram.Gram(\"Start : @string[] = Word & x: ';'?\\nWord : @string = c: ['a'..'z']+ => @(c.ToString())\\nparse Start\"" + option + ")]\npublic partial class Spanned { }")
				.GetType("Spanned")!;

			Assert.Equal(expected, string.Join(",", (string[])type.GetMethod("ParseStart", [typeof(string)])!.Invoke(null, [input])!));
		}
	}

	/// <summary>
	/// A host's constructor is handed the capture as it is read, a span where captures are spans: which overload it has
	/// is the host's, and a conversion the generator wrote would be a copy it did not ask for (§6.3).
	/// </summary>
	[Fact]
	public void A_hosts_constructor_is_handed_the_span()
	{
		var run = GeneratorDriverTests.RunGenerator(
			"[DotGram.Gram(\"Start : @Box = name: ['a'..'z']+\\nparse Start\", UseSpanCaptures = true)]\n" +
			"public partial class Boxes { public sealed class Box { public Box(System.ReadOnlySpan<char> name) { } } }");
		var text = string.Concat(run.Results.SelectMany(static one => one.GeneratedSources).Select(static one => one.SourceText.ToString()));

		Assert.DoesNotContain(run.Diagnostics, static one => one.Severity == DiagnosticSeverity.Error);
		Assert.DoesNotContain("name.ToString()", text, StringComparison.Ordinal);
	}

	/// <summary>The name of the grammar's own context is a parameter of every method that builds, and a capture of it is a second.</summary>
	[Fact]
	public void A_capture_may_not_be_named_like_the_context()
	{
		var result = Compile("context : @Names\nRoot : @string = context: ['a'..'z']+ => @(context.ToString())\nparse Root");
		var found  = Assert.Single(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal(GrammarNormalizer.ReservedCaptureName, found.Id);
	}

	/// <summary>The local a located factory declares has a name the author cannot take, so a member of the host named like it is read as the host's.</summary>
	[Fact]
	public void A_host_member_named_made_is_read_in_a_located_rule()
	{
		const string members = """
			public interface ILoc { void Locate(int at, int length); }
			public sealed class Loc : ILoc { public string S = ""; public void Locate(int at, int length) { } }
			public static string made = "m";
			""";

		var host = CSharpCompilation.Create(
			"Host",
			[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {members} }}", cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References);

		var result = GramCompiler.Compile(
			"Root : @Loc = ['a'..'z']+ => @(new Loc { S = made })\nparse Root",
			new GramCompilerOptions
			{
				ClassName      = "Grammar",
				CSharpScanner  = RoslynCSharpScanner.Instance,
				SymbolResolver = new RoslynSymbolResolver(host, "Grammar"),
				LocationType   = "Grammar.ILoc",
			});

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		EmittedCode.Compile(result.Sources[0].Text, declarationMembers: members, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	/// <summary>
	/// A located factory declares nothing of its own beside the author's C#: a capture or a member of the host named as
	/// anything the author may write is read as it is written.
	/// </summary>
	[Theory]
	[InlineData("Root : @Loc = parserMade: ['a'..'z']+ => @(new Loc { S = parserMade.ToString() })", "")]
	[InlineData("Root : @Loc = parserMade: ['a'..'z']+ & s: '!' => @(new Loc { S = s.ToString() })", "")]
	[InlineData("Root : @Loc = s: ['a'..'z']+ => @(new Loc { S = parserMade + s.ToString() })", "public static string parserMade = \"host\";")]
	[InlineData("Root : @Loc = s: ['a'..'z']+ => @(new Loc { S = made + s.ToString() })", "public static string made = \"host\";")]
	public void A_located_rule_reads_the_authors_names_as_written(string grammar, string extra)
	{
		var members = """
			public interface ILoc { void Locate(int at, int length); }
			public sealed class Loc : ILoc { public string S = ""; public void Locate(int at, int length) { } }
			""" + extra;

		var host = CSharpCompilation.Create(
			"Host",
			[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {members} }}", cancellationToken: TestContext.Current.CancellationToken)],
			EmittedCode.References);

		var result = GramCompiler.Compile(
			grammar + "\nparse Root",
			new GramCompilerOptions
			{
				ClassName      = "Grammar",
				CSharpScanner  = RoslynCSharpScanner.Instance,
				SymbolResolver = new RoslynSymbolResolver(host, "Grammar"),
				LocationType   = "Grammar.ILoc",
			});

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		EmittedCode.Compile(result.Sources[0].Text, declarationMembers: members, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	/// <summary>
	/// A rule parameter is no name of the generated code that could be a second <c>context</c>: it builds beside a declared
	/// context, as a recognizer and as a value, as it did.
	/// </summary>
	[Theory]
	[InlineData("P(context) : @string = v: context => @(v.ToString())\nRoot : @string = P(W)")]
	[InlineData("P(context: @int) : @string = v: W => @(v.ToString() + context)\nRoot : @string = P(1)")]
	public void A_parameter_named_context_builds_beside_a_declared_context(string rules)
	{
		var result = Compile("context : @Names\n" + rules + "\nW : @string = ['a'..'z']+\nparse Root");

		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GrammarNormalizer.ReservedCaptureName);
	}

	/// <summary>
	/// A name on a group is a name on something that is no operand, and adds nothing: what is in the group counts as it
	/// would unnamed, in a rule that takes an operand's value as in a sequence.
	/// </summary>
	[Theory]
	[InlineData("Start : Word = x: ('<' & Word & '>')\nWord : @string = c: ['a'..'z']+ => @(c.ToString())", "<abc>", "abc")]
	[InlineData("Start : Bare = x: ('<' & Bare & '>')\nBare : @string = ['a'..'z']+", "<abc>", "abc")]
	public void A_name_on_a_group_adds_nothing_to_the_value_of_an_operand(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Parsed(grammar, input));
	}

	[Theory]
	[InlineData("Start : @string[] = Word & x: (',' & Word)*", "ab,cd,ef", "ab;cd;ef")]
	[InlineData("Start : @string[] = x: (Word & ',' & Word)", "ab,cd", "ab;cd")]
	[InlineData("Start : @string[] = Word & (',' & x: Word)*", "ab,cd,ef", "ab")]
	public void A_name_on_a_group_adds_nothing_to_a_sequence(string rules, string input, string expected)
	{
		var value = (System.Collections.IEnumerable)Parsed(rules + "\nWord : @string = c: ['a'..'z']+ => @(c.ToString())", input, kind: true)!;

		Assert.Equal(expected, string.Join(";", value.Cast<object>()));
	}

	/// <summary>A name that a guard reads is read, and the rule is the one it would be unnamed: an extent of text as an extent of bounds.</summary>
	[Theory]
	[InlineData("Start : @string = x: ['a'..'z']+ & when @(x.Length > 1)", "abc", "abc")]
	[InlineData("Start : Bare = x: '<' & when @(x.Length == 1) & Bare & '>'\nBare : @string = ['a'..'z']+", "<abc>", "abc")]
	public void A_name_read_by_a_guard_leaves_the_value_as_it_is(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Parsed(grammar, input));
	}

	/// <summary>A name on an operand of another type is no operand's name: it adds nothing, whatever the rule declared.</summary>
	[Theory]
	[InlineData("Start : @string = n: Num & ['a'..'z']+\nNum : @int = ['0'..'9']+ => @(int.Parse(parserText))", "12abc", "12abc")]
	[InlineData("Start : @string = i: Item & '!'\nItem = x: ['a'..'z']+", "abc!", "abc!")]
	public void A_name_on_an_operand_of_another_type_adds_nothing(string grammar, string input, string expected)
	{
		// Through the real generator: whether `int` fits a `string` is a question the host answers.
		var type = GeneratorDriverTests.Build(
			"[DotGram.Gram(\"" + grammar.Replace("\n", "\\n") + "\\nparse Start\")]\npublic partial class Typed { }")
			.GetType("Typed")!;

		Assert.Equal(expected, (string)type.GetMethod("ParseStart", [typeof(string)])!.Invoke(null, [input])!);
	}

	/// <summary>A left-recursive rule that collects has no saying which operand a step hands on: it is told, and the generator does not throw.</summary>
	[Fact]
	public void A_left_recursive_sequence_is_told()
	{
		var result = Compile("Start : @string[] = Start & ',' & Word | Word\nWord : @string = c: ['a'..'z']+ => @(c.ToString())\nparse Start");

		Assert.Contains(result.Diagnostics, static one => one.Id == GrammarNormalizer.UnbuiltConstruction);
	}

	/// <summary>
	/// A capture named <c>context</c> beside a declared context is a second parameter only where the C# of the rule writes
	/// it: elsewhere it is a name like any other.
	/// </summary>
	[Theory]
	[InlineData("Start : Word = '<' & Word & context: '>'\nWord : @string = c: ['a'..'z']+ => @(c.ToString())", false)]
	[InlineData("Start : @string[] = Word & context: ';'\nWord : @string = c: ['a'..'z']+ => @(c.ToString())", false)]
	[InlineData("Start : @string = context: ['a'..'z']+", false)]
	[InlineData("Start : @string = context: ['a'..'z']+ => @(context.ToString())", true)]
	public void A_capture_named_context_is_refused_only_where_the_rule_writes_the_context(string grammar, bool refused)
	{
		var result = Compile("context : @Names\n" + grammar + "\nparse Start");

		Assert.Equal(refused, result.Diagnostics.Any(static one => one.Id == GrammarNormalizer.ReservedCaptureName));
	}

	/// <summary>
	/// A name that a §7.3 constructor of the declared type reads is read, as one a <c>=&gt;</c> reads is: the value is the
	/// one the constructor builds, <c>new string('c', 3)</c>, and not the extent.
	/// </summary>
	[Fact]
	public void A_string_built_by_its_own_constructor_from_the_names_keeps_that_value()
	{
		var type = GeneratorDriverTests.Build(
			"[DotGram.Gram(\"Start : @string = c: Ch & count: Num\\nCh : @char = x: ['a'..'z'] => @('c')\\n" +
			"Num : @int = d: ['0'..'9']+ => @(d.Length)\\nparse Start\")]\npublic partial class Ctor { }")
			.GetType("Ctor")!;

		Assert.Equal("ccc", (string)type.GetMethod("ParseStart", [typeof(string)])!.Invoke(null, ["a123"])!);
	}

	/// <summary>A rule that takes the type of a text rule asks the same of its names as one that declares it.</summary>
	[Theory]
	[InlineData("Start : @string[] = Inner & ',' & Bare\\nInner : Bare = c: Ch & count: Num\\nBare : @string = ['a'..'z']+\\n", "a123,de", "ccc")]
	[InlineData("Start : @string[] = Inner & ',' & Bare\\nInner : Bare = c: Ch & count: Num & when @(count > 0)\\nBare : @string = ['a'..'z']+\\n", "a123,de", "ccc")]
	[InlineData("Start : @string[] = Inner & ',' & Bare\\nInner : Bare = c: Ch & count: Num & Bare\\nBare : @string = ['a'..'z']+\\n", "a123xy,de", "ccc")]
	public void A_rule_typed_by_a_text_rule_asks_the_constructor(string grammar, string input, string expected)
	{
		var type = GeneratorDriverTests.Build(
			"[DotGram.Gram(\"" + grammar + "Ch : @char = x: ['a'..'z'] => @('c')\\nNum : @int = d: ['0'..'9']+ => @(d.Length)\\nparse Start\")]\npublic partial class Typed2 { }")
			.GetType("Typed2")!;
		var value = type.GetMethod("ParseStart", [typeof(string)])!.Invoke(null, [input])!;

		Assert.Equal(expected, value is string[] many ? string.Join(",", many) : (string)value);
	}

	/// <summary>A name that several constructors of the type tie on (<c>string(char[] value)</c>, <c>string(ReadOnlySpan&lt;char&gt; value)</c>) builds nothing, so it is read by nothing and the rule is the extent, as with any other name.</summary>
	[Theory]
	[InlineData("value")]
	[InlineData("x")]
	public void A_name_that_constructors_tie_on_is_the_extent_as_any_other_name_is(string name)
	{
		var type = GeneratorDriverTests.Build(
			"[DotGram.Gram(\"Start : @string = " + name + ": ['a'..'z']+\\nparse Start\")]\npublic partial class Tied { }")
			.GetType("Tied")!;

		Assert.Equal("abc", (string)type.GetMethod("ParseStart", [typeof(string)])!.Invoke(null, ["abc"])!);
	}

	/// <summary>The names of the generated span's own constructor are told, as they were, and are not silently the extent.</summary>
	[Fact]
	public void A_SourceSpan_rule_named_like_the_span_constructor_is_told()
	{
		var run = GeneratorDriverTests.RunGenerator(
			"[DotGram.Gram(\"Start : @SourceSpan = start: N1 & length: N1\\nN1 : @int = d: ['0'..'9'] => @(d.Length)\\nparse Start\")]\npublic partial class Spans { }");

		Assert.Contains(run.Diagnostics, static one => one.Id == "GRAM4008");
	}

	/// <summary>The word <c>context</c> in a literal, or after a dot, is no name the code of the rule writes.</summary>
	[Theory]
	[InlineData("Start : @string = context: W & y: W => @(y.ToString() + \"context\")\nW : @string = c: ['a'..'z']+ => @(c.ToString())")]
	[InlineData("Start : @string = context: W & y: W => @(y.ToString().context)\nW : @string = c: ['a'..'z']+ => @(c.ToString())")]
	public void The_word_context_that_the_code_does_not_name_is_no_name_of_the_context(string grammar)
	{
		var result = Compile("context : @Names\n" + grammar + "\nparse Start");

		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GrammarNormalizer.ReservedCaptureName);
	}

	/// <summary>A left-recursive rule that builds its value with a '=>' of its own is as it was.</summary>
	[Fact]
	public void A_left_recursive_rule_builds_with_its_own_construction()
	{
		Assert.Equal("b", Parsed("Start : Bare = Start & '+' & b: Bare => @(b) | b: Bare => @(b)" + Words, "a+b"));
	}

	[Theory]
	[InlineData("Start : Span = Span | Digits\nSpan : @SourceSpan = ['a'..'z']+\nDigits : @SourceSpan = ['0'..'9']+", "123", "3")]
	[InlineData("Start : Span = '<' & Span & '>' | Span\nSpan : @SourceSpan = ['a'..'z']+", "<abc>", "3")]
	[InlineData("Start : Span = '-'? & ('<' & Span & '>' | Span)\nSpan : @SourceSpan = ['a'..'z']+", "-<ab>", "2")]
	public void Gives_the_extent_of_the_operand_that_matched(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Length(grammar, input));
	}

	[Theory]
	[InlineData("Start : Words = Words | Digits\nWords : @string[] = Word+\nDigits : @string[] = Digit+\nWord : @string = c: ['a'..'z'] => @(c)\nDigit : @string = c: ['0'..'9'] => @(c)", "ab", "a,b")]
	[InlineData("Start : Words = Words | Digits\nWords : @string[] = Word+\nDigits : @string[] = Digit+\nWord : @string = c: ['a'..'z'] => @(c)\nDigit : @string = c: ['0'..'9'] => @(c)", "12", "1,2")]
	[InlineData("Start : Words = '<' & Words & '>' | Words\nWords : @string[] = Word+\nWord : @string = c: ['a'..'z'] => @(c)", "<ab>", "a,b")]
	[InlineData("Start : Words = Words | Digits\nWords : @int = ['a'..'z']+ => @(1)\nDigits : @int = ['0'..'9']+ => @(2)", "12", "2")]
	public void Gives_the_value_of_the_operand_that_matched(string grammar, string input, string expected)
	{
		Assert.Equal(expected, Show(Parsed(grammar, input, kind: true)));
	}

	static string Show(object? value)
	{
		return value is IEnumerable sequence and not string ? string.Join(",", sequence.Cast<object>()) : "" + value;
	}

	static string? Parsed(string grammar, string input)
	{
		return (string?)Parsed(grammar, input, kind: true);
	}

	static object? Parsed(string grammar, string input, bool kind)
	{
		var result = Compile(grammar + "\nparse Start");

		EmittedCode.Quiet(result.Diagnostics);

		var (isSuccess, value, error, _) = EmittedCode.Match(
			EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text)),
			"Grammar",
			"TryParseStart",
			input);

		Assert.True(isSuccess, error);

		return value;
	}

	static string Length(string grammar, string input)
	{
		var value = Parsed(grammar, input, kind: true)!;

		return "" + value.GetType().GetProperty("Length")!.GetValue(value);
	}

	static GramCompilation Compile(string grammar)
	{
		return GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });
	}
}
