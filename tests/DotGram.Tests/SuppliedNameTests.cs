using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A name the parser supplies is declared where the grammar may write it, and refused by a diagnostic
/// of the generator's where it may not: it never reaches the consumer's build as an undeclared identifier.
/// </summary>
/// <remarks>
/// Each position is handed a fixed set (docs/syntax.md §8.2). The ones handed are built and run in every
/// rendering the generator has — the engine, the tape, and the immediate carrier — and the ones refused
/// are refused before any of them is written.
/// </remarks>
public sealed class SuppliedNameTests
{
	const string Rows =
		"Row : @string = t: ['a'..'z']+ & ';' => @(t)\n" +
		"Start : @string[] = Row* recover ';' => @(FACTORY)\nparse Start\n";

	/// <summary>
	/// A <c>recover</c> factory that names <c>parserInput</c> is handed the whole input, as a <c>=&gt;</c>
	/// is. The engine took the name for one nobody asked for, and the generated file said CS0103.
	/// </summary>
	[Theory]
	[InlineData("parserInput", "[ab|ab;1;cd;|cd]")]
	[InlineData("parserInput.Length.ToString()", "[ab|8|cd]")]
	[InlineData("parserInput.Substring(parserSpan.Start, parserSpan.Length)", "[ab|1|cd]")]
	[InlineData("parserText + \"@\" + parserInput.Length", "[ab|1@8|cd]")]
	public void A_recovery_factory_is_handed_the_whole_input(string factory, string expected)
	{
		foreach (var (name, direct, carrier) in TieShapes.Renderings)
		{
			var parser = TieShapes.Grammar(Rows.Replace("FACTORY", factory, StringComparison.Ordinal), direct, carrier);

			Assert.Equal(name + expected, name + TieShapes.Ask(parser, "TryParseStart", "ab;1;cd;").Said);
		}
	}

	/// <summary>
	/// Where there is no whole input to hand over, the factory is refused as a <c>=&gt;</c> is: the
	/// buffered form (GRAM4026) and the reader (GRAM5001) say so, and the string form is still written.
	/// </summary>
	[Fact]
	public void A_recovery_factory_that_asks_for_the_input_is_refused_a_buffer_and_a_reader()
	{
		var grammar = Rows.Replace("FACTORY", "parserInput", StringComparison.Ordinal);

		foreach (var (name, direct, carrier) in TieShapes.Renderings)
		{
			var result = Compile(grammar, direct, carrier);

			var buffered = Assert.Single(result.Diagnostics, static one => one.Id == GramCompiler.BufferedUnsupported);

			Assert.Contains("parserInput", buffered.Message, StringComparison.Ordinal);
			Assert.Contains(result.Diagnostics, static one => one.Id == "GRAM5001" && one.Message.Contains("'parserInput'", StringComparison.Ordinal));
			Assert.DoesNotContain("TryParseStart(global::System.IO.TextReader", result.Sources[0].Text, StringComparison.Ordinal);
			Assert.Contains("TryParseStart(string", result.Sources[0].Text, StringComparison.Ordinal);
		}
	}

	/// <summary>
	/// A name written where the parser does not hand it is a diagnostic of the generator's, in every
	/// rendering, and not an undeclared identifier in the file it writes (it used to be CS0103 for the
	/// input and the marks in a <c>when</c>, and for a rejected element's name in a <c>=&gt;</c>).
	/// </summary>
	[Theory]
	[InlineData("Start = ['a'..'z']+ & when @(parserInput.Length > 0)", "parserInput", "a 'when'")]
	[InlineData("state : @int\nStart = ['a'..'z']+ & when @(parserState.Length == 0)", "parserState", "a 'when'")]
	[InlineData("state : @int\nStart = ['a'..'z']+ & when @(parserMarks.Length == 0)", "parserMarks", "a 'when'")]
	[InlineData("state : @int\nStart = (['a'..'z']+ with state @(1)) & when @(parserState.Length == 1 && parserMarks.Length == 1)", "parserState", "a 'when'")]
	[InlineData("Start : @string = t: ['a'..'z']+ & when @(parserInput.Length > 0) => @(t)", "parserInput", "a 'when'")]
	[InlineData("Start : @string = t: ['a'..'z']+ => @(parserMessage)", "parserMessage", "a '=>'")]
	[InlineData("Start : @int = t: ['a'..'z']+ => @(parserOrdinal + parserLine)", "parserOrdinal", "a '=>'")]
	[InlineData("Start : @string = t: ['a'..'z']+ => @(parserExpected)", "parserExpected", "a '=>'")]
	[InlineData("Start : @long = t: ['a'..'z']+ => @(parserPosition)", "parserPosition", "a '=>'")]
	[InlineData("Start : @int = t: ['a'..'z']+ => @(parserFailureLine)", "parserFailureLine", "a '=>'")]
	public void A_name_that_is_not_handed_over_is_refused(string grammar, string name, string place)
	{
		foreach (var (_, direct, carrier) in TieShapes.Renderings)
		{
			var result = Compile(grammar + "\nparse Start\n", direct, carrier);
			var said   = Assert.Single(result.Diagnostics, static one => one.Id == GrammarNormalizer.SuppliedNameNotHere);

			Assert.Equal(GramSeverity.Error, said.Severity);
			Assert.Contains($"'{name}'", said.Message, StringComparison.Ordinal);
			Assert.Contains(place, said.Message, StringComparison.Ordinal);
		}
	}

	/// <summary>
	/// A rejected element was never read, so no mark stands over it: a factory that names the marks is
	/// refused, with or without a <c>state</c> (it was handed an <c>int</c> by that name, or nothing).
	/// </summary>
	[Theory]
	[InlineData(true, "parserState")]
	[InlineData(true, "parserMarks")]
	[InlineData(false, "parserState")]
	[InlineData(false, "parserMarks")]
	public void A_recovery_factory_is_not_handed_marks(bool declared, string name)
	{
		var grammar = (declared ? "state : @int\n" : "") + Rows.Replace("FACTORY", name + ".ToString()", StringComparison.Ordinal);

		foreach (var (_, direct, carrier) in TieShapes.Renderings)
		{
			var result = Compile(grammar, direct, carrier);
			var said   = Assert.Single(result.Diagnostics, static one => one.Id == GrammarNormalizer.SuppliedNameNotHere);

			Assert.Contains($"'{name}'", said.Message, StringComparison.Ordinal);
		}
	}

	/// <summary>
	/// The names a <c>when</c> and a <c>=&gt;</c> are handed are still accepted, built, and read in every
	/// rendering: the check refuses what is not handed and nothing else. A string that spells a refused
	/// name asks for nothing.
	/// </summary>
	[Fact]
	public void The_names_that_are_handed_over_still_compile_and_run()
	{
		const string grammar =
			"state : @int\n" +
			"Word : @string = t: ['a'..'z']+ & when @(parserText.Length > 0 && parserSpan.Length == parserText.Length && \"parserInput\" != \"parserMarks\") => @(t + parserInput.Length + parserState.Length + parserMarks.Length)\n" +
			"Start : @string = w: Word with state @(1) & ';' => @(w)\nparse Start\n";

		foreach (var (name, direct, carrier) in TieShapes.Renderings)
		{
			var parser = TieShapes.Grammar(grammar, direct, carrier);

			Assert.True("accepted" == TieShapes.Ask(parser, "TryParseStart", "abc;").Said, name);
		}
	}

	/// <summary>
	/// A name of the host that happens to be spelled like a supplied one is the host's where it is called or
	/// made: <c>parserMessage()</c> is its method and <c>new parserText()</c> its type, and neither is
	/// refused (GRAM4053) nor handed a parameter of that name, which would make the call fail to compile.
	/// </summary>
	[Theory]
	[InlineData("Start : @string = 'a' => @parserMessage()")]
	[InlineData("Start : @string = 'a' => @(parserMessage())")]
	[InlineData("Start : @string = 'a' => @(parserMessage() + new parserText().ToString())")]
	[InlineData("Start : @string = 'a' & when @(parserMessage() != null) => @(\"ok\")")]
	[InlineData("Start : @string = 'a' => @(((parserText)new parserText()).ToString() + (typeof(parserText) is null ? \"\" : \"\") + parserMessage())")]
	[InlineData("Start : @string[] = Row* recover ';' => @(parserMessage())\nRow : @string = t: ['a'..'z']+ & ';' => @(t)")]
	public void A_host_name_spelled_like_a_supplied_one_is_the_hosts_where_it_is_called_or_made(string grammar)
	{
		const string members =
			"public static string parserMessage() { return \"ok\"; }\n" +
			"public sealed class parserText { public override string ToString() { return \"\"; } }\n";

		foreach (var (name, direct, carrier) in TieShapes.Renderings)
		{
			var result = Compile(grammar + "\nparse Start\n", direct, carrier);

			Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GrammarNormalizer.SuppliedNameNotHere);

			// Built beside the host, as a consumer's build builds it.
			var parser = TieShapes.Grammar(grammar + "\nparse Start\n", direct, carrier, members: members);

			Assert.True(parser.GetMethod("TryParseStart", [typeof(string)]) is not null, name);
		}
	}

	/// <summary>
	/// A name written with an <c>@</c> or a Unicode escape is the name it spells, for the factory's parameters
	/// as for the check that it is handed: the generated file declares it.
	/// </summary>
	[Theory]
	[InlineData("@parserInput")]
	[InlineData("parserInpu\\u0074")]
	[InlineData("@parserInpu\\u0074")]
	[InlineData("par\\u0073erInput")]
	public void A_recovery_factory_may_spell_a_supplied_name_otherwise(string spelled)
	{
		foreach (var (name, direct, carrier) in TieShapes.Renderings)
		{
			var parser = TieShapes.Grammar(Rows.Replace("FACTORY", spelled, StringComparison.Ordinal), direct, carrier);

			Assert.Equal(name + "[ab|ab;1;cd;|cd]", name + TieShapes.Ask(parser, "TryParseStart", "ab;1;cd;").Said);
		}
	}

	/// <summary>
	/// A factory that does not ask for the input is not charged for it: a string or a comment that spells the
	/// name leaves the parameter unwritten and the reader form unrefused.
	/// </summary>
	[Theory]
	[InlineData("\"parserInputTag\"")]
	[InlineData("parserText /* parserInput */")]
	[InlineData("parserText + \"parserInput\"")]
	[InlineData("parserText + nameof(Tag.parserInput)")]
	public void A_recovery_factory_that_does_not_ask_for_the_input_is_not_charged_for_it(string factory)
	{
		var grammar = Rows.Replace("FACTORY", factory, StringComparison.Ordinal);

		foreach (var (name, direct, carrier) in TieShapes.Renderings)
		{
			var result = Compile(grammar, direct, carrier);

			Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GramCompiler.BufferedUnsupported || one.Id == "GRAM5001");
			Assert.DoesNotContain("string parserInput", result.Sources[0].Text, StringComparison.Ordinal);
		}
	}

	static GramCompilation Compile(string grammar, bool direct, CarrierKind carrier)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			Direct         = direct,
			Carrier        = carrier,
			BufferedInput  = true,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			CSharpScanner  = RoslynCSharpScanner.Instance,
		});
	}
}
