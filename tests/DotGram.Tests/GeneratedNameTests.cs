using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
	[InlineData("DefaultBufferSize")]
	[InlineData("DefaultMaxRetained")]
	[InlineData("Failure")]
	[InlineData("ParserEntry")]
	[InlineData("Probe")]
	public void A_publication_named_like_a_member_of_the_class_is_refused(string name)
	{
		var result = Compile($"W = 'w'\nparse W as {name}");

		Assert.Contains(result.Diagnostics, static one => one.Id == CSharpEmitter.GeneratedNameClash);
		Assert.Empty(result.Sources);
	}

	/// <summary>
	/// The methods a publication is written as are its name and, for a <c>parse</c>, <c>Try</c> before it:
	/// a rule whose type would be called one of them is given another name for it, and two publications
	/// cannot write one.
	/// </summary>
	[Theory]
	[InlineData("T = x: 'a' & TryParseT\nTryParseT = y: 'b'\nparse T", "TryParseTValue")]
	[InlineData("TryRead = x: 'x'\nS = TryRead & eof\nparse TryRead as Read\nparse S", "TryReadValue")]
	[InlineData("Header = h: 'h'\nparse Header as Header", "HeaderValue")]
	public void A_rule_does_not_take_the_name_of_a_method_a_publication_is_written_as(string grammar, string type)
	{
		foreach (var direct in new[] { true, false })
			Assert.NotNull(Built(grammar, direct).GetType("Probe+" + type));
	}

	[Theory]
	[InlineData("T = 'a'\nQ = 'b'\nparse T\nparse Q as TryParseT")]
	[InlineData("T = 'a'\nQ = 'b'\nparse T as Read\nfind Q as TryRead")]
	[InlineData("T = 'a'\nparse T as Recognize_T")]
	[InlineData("T = 'a'\nparse T as ReadBuffered_X")]
	[InlineData("T = 'a'\nparse T as OnHandingOff")]
	public void Two_publications_cannot_write_one_method_nor_one_a_reserved_name(string grammar)
	{
		Assert.Contains(Compile(grammar).Diagnostics, static one => one.Id == CSharpEmitter.GeneratedNameClash);
	}

	/// <summary>The factory of a rule is a method of the class, and a type is not called what a factory is.</summary>
	[Fact]
	public void A_rule_does_not_take_the_name_of_the_factory_of_another()
	{
		const string grammar = "R : @int = 'r' => @(1)\nConstruct_R = c: 'c'\nS = r: R & c: Construct_R\nparse S";

		foreach (var direct in new[] { true, false })
			Assert.NotNull(Built(grammar, direct).GetType("Probe+Construct_RValue"));
	}

	/// <summary>
	/// Two rules are two sets of methods, named after them: a rule called <c>A_Whole</c> beside a published
	/// <c>A</c> is the method that reads all of <c>A</c>.
	/// </summary>
	[Fact]
	public void Rules_whose_methods_are_named_alike_are_refused()
	{
		Assert.Contains(
			Compile("A = x: 'a'\nA_Whole = y: 'b'\nS = A & A_Whole\nparse A\nparse S").Diagnostics,
			static one => one.Id == CSharpEmitter.GeneratedNameClash);
	}

	/// <summary>
	/// What the engine keeps for its own use is not named so that a rule or a publication can be called
	/// it, in the engines that keep it: the tape's reader and the engine, over a string and a reader.
	/// </summary>
	[Theory]
	[InlineData("Recycle")]
	[InlineData("Recycled")]
	[InlineData("RentParser")]
	[InlineData("ReturnParser")]
	[InlineData("KeptEntries")]
	[InlineData("LargeParserIdle")]
	[InlineData("SpareParserIdle")]
	[InlineData("_spareParser")]
	[InlineData("_largeTokens")]
	public void What_the_engine_keeps_is_no_name_of_the_authors(string name)
	{
		var grammar = $"{name} = x: 'a'\nS = {name} & eof\nparse S";

		foreach (var direct in new[] { true, false })
			foreach (var carrier in new[] { CarrierKind.Tape, CarrierKind.Immediate })
				foreach (var buffered in new[] { false, true })
				{
					_ = Built(grammar, direct, buffered, carrier);

					// The fields are reserved, not renamed: a publication may not take one.
					if (name.StartsWith('_'))
					{
						Assert.Contains(Compile($"W = 'w'\nparse W as {name}", direct, buffered, carrier).Diagnostics, static one => one.Id == CSharpEmitter.GeneratedNameClash);

						continue;
					}

					_ = Built($"W = 'w'\nparse W as {name}", direct, buffered, carrier);
					_ = Built($"W = 'w'\nfind W as {name} stream", direct, buffered, carrier);
				}
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
	/// Every member the generator writes into the class — method, field, property, constant or type —
	/// is named with <c>_DotGram</c> in it, which no name of the author's can be; or is one of the names
	/// it reserves, which no publication or rule's type may take; or is made from a name of the author's
	/// along a path that is checked: a method named after a rule or a publication under a prefix it
	/// reserves (<c>Recognize_</c>, <c>Try</c>…), or the type of a rule. So that adding a member to the
	/// generator cannot leave a collision unsaid. Read as the compiler reads it, over every snapshot.
	/// </summary>
	[Fact]
	public void Every_member_the_snapshots_write_is_a_name_the_generator_keeps_apart()
	{
		var directory = Path.Combine(SourceRoot.Directory, "tests", "Snapshots");
		var reserved  = new HashSet<string>(CSharpEmitter.PublicNames.Concat(CSharpEmitter.SupportNames).Concat(CSharpEmitter.TraceNames).Concat(CSharpEmitter.LexerNames), StringComparer.Ordinal);
		var unkept    = new SortedSet<string>(StringComparer.Ordinal);
		var seen      = 0;

		foreach (var file in Directory.GetFiles(directory, "*.g.cs"))
		{
			var grammar = Path.Combine(directory, Path.GetFileName(file).Split('.')[0] + ".gram");
			var written = File.Exists(grammar) ? File.ReadAllText(grammar) : "";
			var methods = new HashSet<string>(StringComparer.Ordinal);

			// What the directives publish: `parse R`, `find R as Name`.
			foreach (Match directive in Regex.Matches(written, @"^\s*(parse|find|yield)\b([^\r\n]*)\r?$", RegexOptions.Multiline))
			{
				var rest  = directive.Groups[2].Value;
				var alias = Regex.Match(rest, @"\bas\s+(\w+)");
				var rule  = Regex.Match(rest, @"^\s*(?:\w+\.)*(\w+)");
				var name  = alias.Success
					? alias.Groups[1].Value
					: (directive.Groups[1].Value == "find" ? "Find" : "Parse") + rule.Groups[1].Value;

				methods.Add(name);
				methods.Add("Try" + name);
			}

			var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(file), cancellationToken: TestContext.Current.CancellationToken);
			var root = tree.GetRoot(TestContext.Current.CancellationToken);

			foreach (var host in root.DescendantNodes().OfType<ClassDeclarationSyntax>().Where(static one => one.Modifiers.Any(static modifier => modifier.Text == "partial")))
				foreach (var member in host.Members)
				{
					var names = member switch
					{
						MethodDeclarationSyntax method     => [method.Identifier.Text],
						FieldDeclarationSyntax field       => [.. field.Declaration.Variables.Select(static one => one.Identifier.Text)],
						PropertyDeclarationSyntax property => [property.Identifier.Text],
						BaseTypeDeclarationSyntax type     => [type.Identifier.Text],
						_                                  => Array.Empty<string>(),
					};

					foreach (var name in names)
					{
						seen++;

						var rulesType = member is BaseTypeDeclarationSyntax && (Regex.IsMatch(written, @"\b" + Regex.Escape(name) + @"\b") ||
							name.EndsWith("Value", StringComparison.Ordinal) && Regex.IsMatch(written, @"\b" + Regex.Escape(name.Substring(0, name.Length - 5)) + @"\b"));

						// A suffix class is the author's to name (Suffix = "…"), and a nested one is a member of the host.
						var suffix = member is ClassDeclarationSyntax nested && nested.Modifiers.Any(static modifier => modifier.Text == "partial");

						if (name.Contains("_DotGram", StringComparison.Ordinal) || reserved.Contains(name) || methods.Contains(name) || rulesType || suffix ||
							Array.Exists(CSharpEmitter.FamilyPrefixes, prefix => name.StartsWith(prefix, StringComparison.Ordinal)))
							continue;

						unkept.Add(name + " (" + Path.GetFileName(file) + ")");
					}
				}
		}

		Assert.True(seen > 0, "no member was found in any snapshot");
		Assert.True(unkept.Count == 0, "written under a name nothing keeps apart from the author's: " + string.Join(", ", unkept));
	}

	/// <summary>The lexer of a grammar read over tokens has a public <c>Scan</c>, and no other has.</summary>
	[Fact]
	public void Scan_is_reserved_only_where_a_lexer_is_written()
	{
		const string grammar = "trivia = { ' '* }\nScan = x: 'a'\nS = Scan & eof\nparse S";

		var plain = GramCompiler.Compile(grammar + " as Scan", new GramCompilerOptions { ClassName = "Probe", CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.DoesNotContain(plain.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var lexed = GramCompiler.Compile(grammar + " as Scan", new GramCompilerOptions { ClassName = "Probe", Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.Contains(lexed.Diagnostics, static one => one.Id == CSharpEmitter.GeneratedNameClash);

		var typed = GramCompiler.Compile(grammar, new GramCompilerOptions { ClassName = "Probe", Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.DoesNotContain(typed.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(typed.Sources[0].Text, className: "Probe", sourceParts: typed.Sources.Skip(1).Select(static one => one.Text));

		Assert.NotNull(assembly.GetType("Probe+ScanValue"));
	}

	static GramCompilation Compile(string grammar, bool direct = true, bool buffered = false, CarrierKind carrier = CarrierKind.Auto)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Probe",
			Direct        = direct,
			Carrier       = carrier,
			BufferedInput = buffered,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
	}

	static Assembly Built(string grammar, bool direct = true, bool buffered = false, CarrierKind carrier = CarrierKind.Auto)
	{
		var result = Compile(grammar, direct, buffered, carrier);

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		return EmittedCode.Compile(result.Sources[0].Text, className: "Probe", sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}
}
