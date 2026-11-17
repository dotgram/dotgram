using System;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the grammar's stages ask the target language, held to the answers they gave when each
/// stage still read the spellings itself: moving a question into the language does not change it.
/// </summary>
public class TargetLanguageTests
{
	/// <summary>
	/// A forwarding rule whose <c>=&gt;</c> wraps its capture in two brackets is not collapsed: the
	/// forwarding test takes one bracket off, and <c>(x)</c> is not <c>x</c>. So the forwarding rule
	/// keeps its own <c>on fail</c> out of <c>Other</c>, which never calls it.
	/// </summary>
	[Fact]
	public void A_forward_in_two_brackets_is_not_collapsed()
	{
		const string grammar = """
			Root : @int = x: Forward => @(x)
			Forward : @int on fail "Expected a forward." = x: Leaf => @((x))
			Leaf : @int = d: ['0'..'9']+ => @(int.Parse(d))
			Other : @int = x: Leaf & eof => @(x)
			parse Root
			parse Leaf
			parse Other
			""";

		var result = GramCompiler.Compile(grammar, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });

		EmittedCode.Quiet(result.Diagnostics);

		var assembly = EmittedCode.Compile(
			result.Sources[0].Text, sourceParts: [.. result.Sources.Skip(1).Select(static one => one.Text)]);

		var (isSuccess, _, error, _) = EmittedCode.Match(assembly, "Grammar", "TryParseOther", "x");

		Assert.False(isSuccess);
		Assert.Equal("Expected ['0'..'9'].", error);
		Assert.Equal(7, EmittedCode.Match(assembly, "Grammar", "TryParseRoot", "7").Value);
	}

	/// <summary>
	/// Where the host cannot be seen, which declared types a rule that captures but builds nothing is
	/// refused for (GRAM4008): one the normalizer reads loosely as C#'s own — <c>System.int</c> and
	/// <c>System.nint</c> among them, which C# itself would not read — is never built from captures on
	/// the host's behalf. Any other is left for the build that sees the host. The nullable and the
	/// <c>global::</c> spellings are held to what they were answered before the question moved, too.
	/// </summary>
	[Theory]
	[InlineData("int",                  true)]
	[InlineData("int?",                 false)]
	[InlineData("System.int",           true)]
	[InlineData("global::System.int",   false)]
	[InlineData("global::int",          false)]
	[InlineData("Int32",                true)]
	[InlineData("Int32?",               false)]
	[InlineData("System.Int32",         true)]
	[InlineData("System.Int32?",        false)]
	[InlineData("global::Int32",        false)]
	[InlineData("global::System.Int32", false)]
	[InlineData("String",               true)]
	[InlineData("global::String",       false)]
	[InlineData("System.nint",          true)]
	[InlineData("nuint?",               false)]
	[InlineData("IntPtr",               true)]
	[InlineData("System.UIntPtr",       true)]
	[InlineData("System.Decimal?",      false)]
	[InlineData("Row",                  false)]
	[InlineData("Row?",                 false)]
	[InlineData("System.Row",           false)]
	[InlineData("System.System.Int32",  false)]
	[InlineData("Void",                 false)]
	[InlineData("System.Void",          false)]
	public void An_unseen_host_reads_a_type_of_its_own_loosely(string type, bool builtIn)
	{
		var result = GramCompiler.Compile(
			$"Root : @{type} = value: 'a'\nparse Root\n",
			new GramCompilerOptions
			{
				SymbolResolver = UnseenHostSymbolResolver.Instance,
				CSharpScanner  = RoslynCSharpScanner.Instance,
			});

		Assert.Equal(builtIn, result.Diagnostics.Any(static one => one.Id == GrammarNormalizer.UnbuiltConstruction));
	}

	/// <summary>Each rule of <see cref="ITargetLanguage.SoleName"/> as the stage that asks by it always answered.</summary>
	[Theory]
	[InlineData("x",            "OneBracket",             "x")]
	[InlineData(" ( x ) ",      "OneBracket",             "x")]
	[InlineData("((x))",        "OneBracket",             "(x)")]
	[InlineData("(a) + (b)",    "OneBracket",             "a) + (b")]
	[InlineData("((x))",        "EveryBracket",           "x")]
	[InlineData("(a) + (b)",    "EveryBracket",           "a) + (b")]
	[InlineData("(1)",          "EveryBracket",           "1")]
	[InlineData("((x))",        "EveryBracketIdentifier", "x")]
	[InlineData("(a) + (b)",    "EveryBracketIdentifier", null)]
	[InlineData("(1)",          "EveryBracketIdentifier", null)]
	[InlineData("()",           "EveryBracketIdentifier", null)]
	[InlineData("((x))",        "WrappingBrackets",       "x")]
	[InlineData("(a) + (b)",    "WrappingBrackets",       null)]
	[InlineData("(1)",          "WrappingBrackets",       "1")]
	[InlineData("()",           "WrappingBrackets",       null)]
	[InlineData("x.Length",     "WrappingBrackets",       null)]
	public void Each_stage_takes_a_fragment_for_one_name_by_its_own_rule(string fragment, string rule, string? expected)
	{
		Assert.Equal(expected, CSharpLanguage.Instance.SoleName(fragment, Enum.Parse<SoleNameRule>(rule)));
	}

	/// <summary>
	/// A C# language handed in generates into the compilation's own host shape: what values are told
	/// their locations through, and the class that asks per call, are the options', as with the
	/// language the compiler makes itself — and so is everything written.
	/// </summary>
	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void A_language_handed_in_takes_the_options_host_shape(bool perCall)
	{
		const string grammar = """
			Pair : @Point = x: Num & ',' & y: Num => @(new Point(x, y))
			Num : @int = d: ['0'..'9']+ => @(int.Parse(d))
			parse Pair
			""";

		GramCompilerOptions Options(ITargetLanguage? language)
		{
			return new GramCompilerOptions
			{
				CSharpScanner = RoslynCSharpScanner.Instance,
				Language      = language,
				LocationType  = "ILocated",
				LocatedFacade = perCall ? "Located" : null,
			};
		}

		var made  = GramCompiler.Compile(grammar, Options(null));
		var given = GramCompiler.Compile(grammar, Options(new CSharpLanguage(PermissiveSymbolResolver.Instance, RoslynCSharpScanner.Instance)));

		Assert.Equal(
			made.Diagnostics.Select(static one => one.ToString()),
			given.Diagnostics.Select(static one => one.ToString()));
		Assert.Equal(made.Sources.Select(static one => one.Text), given.Sources.Select(static one => one.Text));
		Assert.Contains("ILocated", made.Sources[0].Text, StringComparison.Ordinal);
		Assert.Equal(perCall, made.Sources[0].Text.Contains("partial class Located", StringComparison.Ordinal));
	}
}
