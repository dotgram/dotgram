using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A name the generator writes into the class twice, or takes from the author, is a compile error in
/// the consumer's build that points at code nobody wrote. What can be made unique is; what only the
/// author can rename is refused where it is written.
/// </summary>
public sealed class GeneratedNameTests
{
	/// <summary>A publication cannot be called what the class declares beside it.</summary>
	[Theory]
	[InlineData("Match")]
	[InlineData("Outcome")]
	[InlineData("SourceSpan")]
	[InlineData("ParserInput")]
	[InlineData("OnRecovered")]
	[InlineData("Failure")]
	[InlineData("ParserEntry")]
	[InlineData("Probe")]
	public void A_publication_named_like_a_member_of_the_class_is_refused(string name)
	{
		var result = Compile($"W = 'w'\nparse W as {name}");

		Assert.Contains(result.Diagnostics, static one => one.Id == CSharpEmitter.GeneratedNameClash);
		Assert.Empty(result.Sources);
	}

	/// <summary>Nor like the type that is made for a rule of the grammar.</summary>
	[Fact]
	public void A_publication_named_like_the_type_of_a_rule_is_refused()
	{
		var result = Compile("Header = h: 'h'\nparse Header as Header");

		Assert.Contains(result.Diagnostics, static one => one.Id == CSharpEmitter.GeneratedNameClash);
	}

	/// <summary>
	/// <c>Grammar</c> in the class <c>Grammar</c> is <c>GrammarValue</c>, and so is the rule of that
	/// name; two rules whose types meet in one name are told apart, the one that kept its own name
	/// keeping it.
	/// </summary>
	[Fact]
	public void Rules_whose_types_meet_in_one_name_are_told_apart()
	{
		var assembly = Built("Probe = x: ['a'..'z']\nProbeValue = y: ['a'..'z']\nparse Probe\nparse ProbeValue");
		var host     = assembly.GetType("Probe")!;

		Assert.NotNull(host.GetNestedType("ProbeValue"));
		Assert.NotNull(host.GetNestedType("ProbeValueValue"));
		Assert.Equal("x", host.GetNestedType("ProbeValueValue")!.GetProperty("X") is null ? "y" : "x");
		Assert.NotNull(host.GetNestedType("ProbeValue")!.GetProperty("Y"));
	}

	/// <summary>
	/// Every type the class nests for its own use has a name no name of the author's can be (it holds
	/// <c>_DotGram</c>), but two, which are reserved instead, and the public ones: a rule called after
	/// one of those gets a type of another name, and a rule called after any other keeps its own.
	/// </summary>
	[Theory]
	[InlineData("Failure", true)]
	[InlineData("ParserEntry", true)]
	[InlineData("Match", true)]
	[InlineData("Outcome", true)]
	[InlineData("SourceSpan", true)]
	[InlineData("ParserInput", true)]
	[InlineData("OnRecovered", true)]
	[InlineData("Window", false)]
	[InlineData("Lines", false)]
	[InlineData("Held", false)]
	[InlineData("Parser", false)]
	[InlineData("ParserArena", false)]
	[InlineData("Ways", false)]
	[InlineData("DirectValues", false)]
	[InlineData("ImmediateValues", false)]
	[InlineData("BufferedText", false)]
	[InlineData("BufferedBytes", false)]
	[InlineData("IParserInputSource", false)]
	public void A_rule_named_like_a_type_of_the_class_gets_a_type_that_compiles(string name, bool renamed)
	{
		var grammar = $"{name} = x: 'a'\nS = {name} & eof\nparse S";

		foreach (var direct in new[] { true, false })
			foreach (var buffered in new[] { false, true })
			{
				var assembly = Built(grammar, direct, buffered);

				Assert.NotNull(assembly.GetType("Probe+" + name + (renamed ? "Value" : "")));
			}
	}

	/// <summary>
	/// A publication called after a type the class nests for its own use is no collision once that type
	/// is named in the reserved space: <c>find Word as Lines</c> was CS0102.
	/// </summary>
	[Theory]
	[InlineData("Lines")]
	[InlineData("Window")]
	[InlineData("Parser")]
	[InlineData("ParserArena")]
	[InlineData("Ways")]
	[InlineData("Held")]
	[InlineData("DirectValues")]
	[InlineData("ImmediateValues")]
	[InlineData("BufferedText")]
	[InlineData("BufferedBytes")]
	[InlineData("IParserInputSource")]
	public void A_publication_named_like_a_type_in_the_reserved_space_compiles(string name)
	{
		foreach (var direct in new[] { true, false })
			foreach (var buffered in new[] { false, true })
			{
				_ = Built($"W = 'w'\nfind W as {name}", direct, buffered);
				_ = Built($"W = 'w'\nparse W as {name}", direct, buffered);
				_ = Built($"W = w: 'w'\nparse W as {name} stream", direct, buffered);
				_ = Built($"W = w: 'w'\nfind W as {name} stream", direct, buffered);
			}
	}

	/// <summary>
	/// Two rules that are one name once their namespaces are joined are two methods of one name and
	/// two types of one name.
	/// </summary>
	[Fact]
	public void Two_rules_written_alike_in_the_generated_code_are_refused()
	{
		var result = Compile("namespace A { B = x: 'b' }\nA_B = y: 'c'\nStart = A.B & A_B\nparse Start");

		Assert.Contains(result.Diagnostics, static one => one.Id == CSharpEmitter.GeneratedNameClash);
	}

	[Fact]
	public void Rules_in_namespaces_that_do_not_meet_are_not()
	{
		var result = Compile("namespace A { B = x: 'b' }\nA_C = y: 'c'\nStart = A.B & A_C\nparse Start");

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);
	}

	/// <summary>Two captures that are one property of the value.</summary>
	[Theory]
	[InlineData("R = a: 'a' & A: 'b'\nparse R")]
	[InlineData("R = r: 'a' & rValue: 'b'\nparse R")]
	public void Captures_that_become_one_property_are_refused(string grammar)
	{
		var result = Compile(grammar);

		Assert.Contains(result.Diagnostics, static one => one.Id == CSharpEmitter.GeneratedNameClash);
	}

	/// <summary>
	/// Every type the generator nests in the class is one of the names it reserves — a publication is
	/// refused them, a rule's type does not take them — or has <c>_DotGram</c> in its name, which no
	/// name of the author's can, so that adding one cannot leave a collision unsaid. What the author's rules
	/// make is told by the grammar's own text.
	/// </summary>
	[Fact]
	public void Every_type_the_snapshots_nest_is_a_name_the_generator_keeps_apart()
	{
		var directory = Path.Combine(SourceRoot.Directory, "tests", "Snapshots");
		var declaration = new Regex(@"^(\t+)(?:public |internal |private )?(?:sealed |readonly |ref |abstract )*(?:class|struct|enum|interface) ([A-Za-z_]\w*)", RegexOptions.Multiline);
		var found = new SortedSet<string>(StringComparer.Ordinal);
		var seen  = 0;

		foreach (var file in Directory.GetFiles(directory, "*.g.cs"))
		{
			var text = File.ReadAllText(file);
			var host = Regex.Match(text, @"^(\t*)(?:public |internal )?partial class \w+", RegexOptions.Multiline);

			if (!host.Success)
				continue;

			var depth    = host.Groups[1].Length + 1;
			var grammar  = Path.Combine(directory, Path.GetFileName(file).Split(".gram")[0] + ".gram");
			var written  = File.Exists(grammar) ? File.ReadAllText(grammar) : "";

			foreach (Match nested in declaration.Matches(text))
			{
				if (nested.Groups[1].Length != depth)
					continue;

				seen++;

				var name = nested.Groups[2].Value;

				// A rule's own type is named after the rule, which the grammar says.
				if (Regex.IsMatch(written, @"\b" + Regex.Escape(name) + @"\b") || name.EndsWith("Value", StringComparison.Ordinal) && Regex.IsMatch(written, @"\b" + Regex.Escape(name.Substring(0, name.Length - 5)) + @"\b"))
					continue;

				found.Add(name);
			}
		}

		Assert.True(seen > 0, "no nested type was found in any snapshot");
		var unkept = found
			.Except(CSharpEmitter.PublicNames.Concat(CSharpEmitter.SupportNames))
			.Where(static name => !name.Contains("_DotGram", StringComparison.Ordinal))
			.ToArray();

		Assert.True(unkept.Length == 0, "nested but kept apart from nothing: " + string.Join(", ", unkept));
	}

	static GramCompilation Compile(string grammar, bool direct = true, bool buffered = false)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Probe",
			Direct        = direct,
			BufferedInput = buffered,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
	}

	static Assembly Built(string grammar, bool direct = true, bool buffered = false)
	{
		var result = Compile(grammar, direct, buffered);

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		return EmittedCode.Compile(result.Sources[0].Text, className: "Probe", sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}
}
