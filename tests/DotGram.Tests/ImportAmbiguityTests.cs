using System;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Which rule a name means where namespaces meet: two imports that supply one name (§5), and a name a
/// namespace's header binds that its block declares again (§5.1).
/// </summary>
public sealed class ImportAmbiguityTests
{
	const string Imports = "using A;\nusing B;\nnamespace A { W = 'a' }\nnamespace B { W = 'b' }\n";

	static GramDiagnostic[] Errors(string grammar)
	{
		return [.. GramCompiler.Compile(grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance })
			.Diagnostics.Where(static one => one.Severity == GramSeverity.Error)];
	}

	[Theory]
	[InlineData(Imports + "P = W\nparse P", "P = W")]
	[InlineData("using B;\nusing A;\nnamespace A { W = 'a' }\nnamespace B { W = 'b' }\nP = W\nparse P", "P = W")]
	[InlineData(Imports + "P = 'x' & W & 'y'\nparse P", "& W &")]
	[InlineData(Imports + "parse W", "parse W")]
	public void A_name_two_imports_supply_is_refused_where_it_is_read(string grammar, string near)
	{
		var told = Assert.Single(Errors(grammar));

		Assert.Equal(GrammarBinder.AmbiguousImport, told.Id);
		// At the name where it is read; for a directive, at the directive.
		Assert.StartsWith(near.StartsWith("parse", StringComparison.Ordinal) ? "parse W" : "W", grammar.Substring(told.Position), StringComparison.Ordinal);
		Assert.True(told.Position >= grammar.IndexOf(near, StringComparison.Ordinal), told.ToString());
		Assert.Contains("'A.W'", told.Message, StringComparison.Ordinal);
		Assert.Contains("'B.W'", told.Message, StringComparison.Ordinal);
	}

	[Theory]
	[InlineData(Imports + "P = A.W\nparse P", "a")]
	[InlineData(Imports + "P = B.W\nparse P", "b")]
	[InlineData(Imports + "W = 'c'\nP = W\nparse P", "c")]
	[InlineData("using A;\nnamespace A { W = 'a' }\nnamespace B { W = 'b' }\nP = W\nparse P", "a")]
	public void While_qualification_an_own_rule_or_one_import_settles_it(string grammar, string input)
	{
		Assert.Empty(Errors(grammar));

		var result   = GramCompiler.Compile(grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });
		var assembly = EmittedCode.Compile(string.Join("\n", result.Sources.Select(static one => one.Text)));

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseP", input).IsSuccess);
	}

	/// <summary>
	/// A name the header binds and the block declares again is GRAM3010 wherever it came from: from an
	/// enclosing namespace it was GRAM3012, which advised the rebinding already written.
	/// </summary>
	[Theory]
	[InlineData("X = 'x'\nY = 'y'\nnamespace N with (X = Y) { X = 'z'\nP = X }\nparse N.P")]
	[InlineData("using M;\nnamespace M { X = 'x' }\nY = 'y'\nnamespace N with (X = Y) { X = 'z'\nP = X }\nparse N.P")]
	public void A_name_the_header_binds_declared_again_is_one_number(string grammar)
	{
		var told = Assert.Single(Errors(grammar));

		Assert.Equal(GrammarBinder.NamespaceBoundNameRedeclared, told.Id);
	}

	[Fact]
	public void While_a_rule_shadowing_an_enclosing_one_with_no_header_is_still_GRAM3012()
	{
		var told = Assert.Single(Errors("X = 'x'\nnamespace N { X = 'z'\nP = X }\nparse N.P"));

		Assert.Equal(GrammarBinder.ShadowsEnclosingRule, told.Id);
	}

	static bool Reads(string grammar, string method, string input)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(string.Join("\n", result.Sources.Select(static one => one.Text)));

		return EmittedCode.Match(assembly, "Grammar", method, input).IsSuccess;
	}

	/// <summary>
	/// A rebinding's left side may be qualified: it means the rule the name resolves to, without a
	/// <c>using</c>, the same as the unqualified name where that rule is in view.
	/// </summary>
	[Theory]
	[InlineData("Num = Std.Digit+\nLower = ['a'..'f']\nP = Num with (Std.Digit = Lower)\nparse P")]
	[InlineData("using Std;\nNum = Digit+\nLower = ['a'..'f']\nP = Num with (Std.Digit = Lower)\nparse P")]
	[InlineData("using Std;\nNum = Digit+\nLower = ['a'..'f']\nP = Num with (Digit = Lower)\nparse P")]
	public void A_qualified_left_side_rebinds_the_rule_it_names(string grammar)
	{
		Assert.True(Reads(grammar, "TryParseP", "abf"));
		Assert.False(Reads(grammar, "TryParseP", "12"));
	}

	[Fact]
	public void And_a_namespace_header_may_bind_one()
	{
		const string Grammar = "namespace A { B = 'b' }\nC = 'c'\nnamespace N with (A.B = C) { P = A.B & 'x' }\nparse N.P";

		Assert.True(Reads(Grammar, "TryParseP", "cx"));
		Assert.False(Reads(Grammar, "TryParseP", "bx"));
	}

	[Theory]
	[InlineData("Num = Std.Digit+\nLower = ['a'..'f']\nP = Num with (Std.Nope = Lower)\nparse P", GrammarBinder.UnknownRebindingTarget)]
	[InlineData("Num = Std.Digit+\nLower = ['a'..'f']\nP = Num with (Nowhere.Digit = Lower)\nparse P", GrammarBinder.UnknownRebindingTarget)]
	[InlineData("Num = Std.Digit+\nWrap(x) = x\nP = Num with (Std.Digit = Wrap)\nparse P", GrammarBinder.ParameterizedRebinding)]
	public void While_a_qualified_left_side_is_checked_as_an_unqualified_one_is(string grammar, string id)
	{
		Assert.Contains(Errors(grammar), one => one.Id == id);
		Assert.DoesNotContain(Errors(grammar), static one => one.Id == "GRAM2001");
	}
}
