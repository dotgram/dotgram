using Xunit;

namespace DotGram.Tests;

/// <summary>
/// An external recognizer or predicate is a method of the author's, called by the name the grammar
/// writes, from generated methods whose parameters and locals have names of their own. A method
/// called <c>text</c>, <c>p</c> or <c>failure</c> would be found there as the local and not as the
/// method (CS0149), so the call names the class that declares it.
/// </summary>
public sealed class ExternalNameTests
{
	public static TheoryData<string, string> Cases()
	{
		var cases = new TheoryData<string, string>();

		// The names the generated machines declare, the ones that only look like one, and the
		// ordinary kind.
		foreach (var name in new[]
		{
			"text", "p", "c", "pos", "value", "failure", "expected", "entries", "externalPos", "externalValue",
			"parser", "recognized", "state", "context", "input", "start", "end", "Plain",
		})
		{
			cases.Add("recognizer", name);
			cases.Add("valued", name);
			cases.Add("predicate", name);
			cases.Add("view", name);
		}

		return cases;
	}

	static string Source(string kind, string name)
	{
		var (method, operand) = kind switch
		{
			"recognizer" => (
				$"static bool {name}(System.ReadOnlySpan<char> s, ref int q) {{ if (q < s.Length && s[q] == 'a') {{ q++; return true; }} return false; }}",
				$"@{name}"),
			"valued" => (
				$"static bool {name}(System.ReadOnlySpan<char> s, ref int q, out int v) {{ v = 1; if (q < s.Length && s[q] == 'a') {{ q++; return true; }} return false; }}",
				$"@{name}"),
			"predicate" => (
				$"static bool {name}(char z) {{ return z == 'a'; }}",
				$"[@{name}]+"),
			_ => (
				$"static bool {name}(ParserInput<char> s, ref int q, Names z) {{ return s.TryAdvance(ref q, 1); }}",
				$"@{name}"),
		};

		var context = kind == "view" ? "context : @Names" : "";

		return $$""""
			using DotGram;

			namespace ExternalProbe
			{
				public sealed class Names
				{
				}

				public static partial class Outer
				{
					[Gram("""
						using Std;
						{{context}}
						Start : @int = {{operand}} & eof => @(1)
						parse Start
						""", {{(kind == "view" ? "BufferedInput = true" : "Portable = true")}})]
					public static partial class P
					{
						{{method}}
					}
				}
			}
			"""";
	}

	[Theory]
	[MemberData(nameof(Cases))]
	public void An_external_method_named_like_a_local_of_the_generated_code_is_called(string kind, string name)
	{
		Assert.NotNull(GeneratorDriverTests.Build(Source(kind, name)));
	}
}
