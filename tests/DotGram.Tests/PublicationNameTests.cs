using System;
using System.Linq;
using System.Reflection;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A publication is named by its author, and the overloads the generator writes beside it name their
/// own parameters — <c>input</c>, and <c>context</c> where the grammar has one. A form that hands its
/// arguments to the publication by its simple name would find the parameter there instead of the
/// method (CS0149), so the forms call the method through the class that holds it.
/// </summary>
public sealed class PublicationNameTests
{
	const string Expr = """
		Expr : @int = Term & ('+' & Term)* => @(1)
		Term : @int = '(' & Expr & ')' => @(2) | d: Digits => @(d.Length)
		""";

	const string Word = "Word : @string = w: ['a'..'z']+ => @(w)";

	const string Recs = """
		Row  : @int = d: Digits & eol => @(d.Length)
		Recs : @int[] = (Row)* recover eol & eof
		""";

	/// <summary>
	/// A grammar for each way the generator writes a form that forwards to the publication: the bytes a
	/// caller already holds, a sequence of lines over a reader that streams a <c>find</c>, and one that
	/// streams a <c>parse</c>.
	/// </summary>
	public static TheoryData<string, string, bool> Forms()
	{
		var forms = new TheoryData<string, string, bool>();

		// The names the forms use for a parameter, for a local and for the reading's own state, and
		// two that only look like one. Only the first two are ever in the way (the rest are listed so
		// that a form which starts to call by name is caught at once).
		foreach (var name in new[]
		{
			"input", "context", "at", "length", "position", "from", "text", "value", "source", "match",
			"error", "bufferSize", "maxRetained", "start", "end", "window", "lines",
		})
		{
			forms.Add(name, "buffered", false);
			forms.Add(name, "buffered", true);
			forms.Add(name, "find", false);
			forms.Add(name, "find", true);
			forms.Add(name, "recs", false);
		}

		return forms;
	}

	static string Source(string kind, string name, bool withContext)
	{
		var use   = withContext ? " + context.GetHashCode()" : "";
		var rules = kind switch
		{
			"buffered" => Expr.Replace("=> @(1)", "=> @(1" + use + ")"),
			"find"     => Word.Replace("@(w)", "@(w" + use + ")"),
			_          => Recs.Replace("@(d.Length)", "@(d.Length" + use + ")"),
		};

		var directive = kind switch
		{
			"buffered" => $"parse Expr as {name} stream bytes",
			"find"     => $"find Word as {name}",
			_          => $"parse Recs as {name}",
		};

		return $$""""
			using DotGram;

			namespace NameProbe
			{
				public sealed class Names
				{
				}

				[Gram("""
					using Std;
					{{(withContext ? "context : @Names" : "")}}
					{{rules.Replace("\n", "\n\t\t\t\t\t")}}
					{{directive}}
					""")]
				public static partial class P
				{
				}
			}
			"""";
	}

	[Theory]
	[MemberData(nameof(Forms))]
	public void A_publication_named_like_a_parameter_of_its_forms_compiles(string name, string kind, bool withContext)
	{
		// A grammar read as bytes is told that its character literals cannot be (GRAM4026); the form
		// that forwards bytes is written all the same, and is what these cases are about.
		Assert.NotNull(GeneratorDriverTests.Build(Source(kind, name, withContext), permittedWarning: "GRAM4026"));
	}

	static string[] Signatures(string kind, string name, bool withContext)
	{
		var assembly = GeneratorDriverTests.Build(Source(kind, name, withContext), permittedWarning: "GRAM4026");

		return [.. assembly.GetType("NameProbe.P")!
			.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
			.Select(static method => method.Name + "(" + string.Join(
				", ", method.GetParameters().Select(static parameter => parameter.ParameterType.Name + " " + parameter.Name)) + ")")
			.OrderBy(static one => one, StringComparer.Ordinal)];
	}

	/// <summary>The public parameter names are the API: callers pass them by name.</summary>
	[Fact]
	public void The_parameter_names_of_the_forms_are_unchanged()
	{
		Assert.Equal(
			[
				"Parse(Byte[] input, Names context)",
				"Parse(ReadOnlyMemory`1 input, Names context)",
				"Parse(Stream input, Names context, Nullable`1 bufferSize, Nullable`1 maxRetained)",
				"Parse(String input, Names context)",
				"TryParse(Byte[] input, Names context)",
				"TryParse(ReadOnlyMemory`1 input, Names context)",
				"TryParse(Stream input, Names context, Nullable`1 bufferSize, Nullable`1 maxRetained)",
				"TryParse(String input, Int32 at, Int32 length, Names context)",
				"TryParse(String input, Int32 at, Names context)",
				"TryParse(String input, Int32& at, Int32 length, Names context, Int32& value)",
				"TryParse(String input, Int32& at, Names context, Int32& value)",
				"TryParse(String input, Names context)",
				"TryParse(String input, Names context, Int32& value)",
			],
			Signatures("buffered", "Parse", true));

		Assert.Equal(
			[
				"Find(IEnumerable`1 input)",
				"Find(String input)",
				"Find(TextReader input)",
			],
			Signatures("find", "Find", false));

		Assert.Equal(
			[
				"Read(IEnumerable`1 input)",
				"Read(String input)",
				"Read(TextReader input)",
				"TryRead(String input)",
				"TryRead(String input, Int32 at)",
				"TryRead(String input, Int32 at, Int32 length)",
				"TryRead(String input, Int32& at, Int32 length, Int32[]& value)",
				"TryRead(String input, Int32& at, Int32[]& value)",
				"TryRead(String input, Int32[]& value)",
			],
			Signatures("recs", "Read", false));
	}

	/// <summary>
	/// The class the call goes through is the one the form is written in: a host nested in another,
	/// and a second reading of it that is a class of its own.
	/// </summary>
	[Fact]
	public void A_form_of_a_nested_host_and_of_a_second_reading_calls_through_its_own_class()
	{
		var source = $$""""
			using DotGram;

			namespace NameProbe
			{
				public partial class Outer
				{
					[Gram("""
						using Std;
						Word : @int = ['a'..'z']+ => @(1)
						find Word as input
						""", BufferedInput = true, BufferedBytes = true)]
					[GramOptions(Suffix = "Other")]
					public static partial class P
					{
					}
				}
			}
			"""";

		var assembly = GeneratorDriverTests.Build(source, permittedWarning: "GRAM4026");
		var host     = assembly.GetType("NameProbe.Outer+P")!;

		Assert.NotNull(host.GetMethod("input", [typeof(byte[])]));
		Assert.NotNull(host.GetNestedType("Other")!.GetMethod("input", [typeof(byte[])]));
	}
}
