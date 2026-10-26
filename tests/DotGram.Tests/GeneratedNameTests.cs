using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
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
	[InlineData("T = 'a'\nparse T as OnHandingOff")]
	public void Two_publications_cannot_write_one_method_nor_one_a_reserved_name(string grammar)
	{
		Assert.Contains(Compile(grammar).Diagnostics, static one => one.Id == CSharpEmitter.GeneratedNameClash);
	}

	/// <summary>
	/// A rule called after what the generator once named a factory, a walk or a variant of one keeps its
	/// name, and so does its type: everything the generator makes from a name carries the mark, which no
	/// name of the author's can.
	/// </summary>
	[Theory]
	[InlineData("R : @int = 'r' => @(1)\nConstruct_R = c: 'c'\nS = r: R & c: Construct_R\nparse S", "Construct_R")]
	[InlineData("A = x: 'a'\nA_Whole = y: 'b'\nS = A & A_Whole\nparse A\nparse S", "A_Whole")]
	[InlineData("A = x: 'a'\nA_Body = y: 'b'\nS = A & A_Body\nparse A\nparse S", "A_Body")]
	[InlineData("A = x: 'a'\nRecognize_A = y: 'b'\nS = A & Recognize_A\nparse S\nparse A as Recognize_A_Whole", "Recognize_A")]
	public void A_rule_named_like_a_name_the_generator_makes_keeps_its_own(string grammar, string type)
	{
		foreach (var direct in new[] { true, false })
			foreach (var buffered in new[] { false, true })
				Assert.NotNull(Built(grammar, direct, buffered).GetType("Probe+" + type));
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
	/// The kinds are one closed table, and no two of its entries write one kind: a number written into a
	/// kind is read as <c>#</c>, and every entry with every variant is a different word. No kind holds an
	/// underscore or the mark, and no name the generator keeps plain begins with one.
	/// </summary>
	[Fact]
	public void The_kinds_are_a_closed_table_of_words_apart()
	{
		foreach (var table in new[] { CSharpEmitter.HostKinds, CSharpEmitter.NestedKinds })
		{
			var expanded = CSharpEmitter.Expanded(table).ToArray();

			Assert.All(expanded, static one => Assert.DoesNotContain("_", one.Kind, StringComparison.Ordinal));
			Assert.All(expanded, static one => Assert.DoesNotContain("DotGram", one.Kind, StringComparison.Ordinal));
			Assert.All(expanded, static one => Assert.Equal(one.Kind, CSharpEmitter.Shape(one.Kind)));
			Assert.Equal(expanded.Length, expanded.Select(static one => one.Kind).Distinct(StringComparer.Ordinal).Count());
		}

		var plain = CSharpEmitter.PublicNames.Concat(CSharpEmitter.SupportNames).Concat(CSharpEmitter.TraceNames).Concat(CSharpEmitter.LexerNames);

		foreach (var name in plain)
			Assert.Null(GeneratedNameRule.Fault(name, host: false, null, new HashSet<string>()));
	}

	/// <summary>
	/// Every name the generator declares in a snapshot is the author's, one it keeps whatever the grammar
	/// says, or made from the grammar's own names by the one rule (<see cref="GeneratedNameRule"/>) — so that
	/// a family written outside it fails here rather than in a review — and no scope declares one name
	/// twice but for a method's overloads. Read as the compiler reads it, over every rendering.
	/// </summary>
	[Theory]
	[MemberData(nameof(Snapshots))]
	public void Every_name_a_snapshot_declares_is_kept_apart(string name)
	{
		var text   = File.ReadAllText(Path.Combine(SnapshotDirectory, name + ".gram"));
		var faults = new SortedSet<string>(StringComparer.Ordinal);
		var seen   = 0;

		foreach (var (options, _) in SnapshotTests.Renderings(name, text))
		{
			var result = GramCompiler.Compile(text, options);
			var names  = result.Names!;
			var known  = new HashSet<string>(names.Identifiers, StringComparer.Ordinal);
			var author = GeneratedNameRule.Author(names);
			var trees  = result.Sources.Select(static one => CSharpSyntaxTree.ParseText(one.Text, cancellationToken: TestContext.Current.CancellationToken)).ToArray();

			foreach (var (scope, host, member, signature) in Declarations(trees))
			{
				seen++;

				if (GeneratedNameRule.Fault(member, host, known, author, options.Trace, options.Lexical) is { } fault)
					faults.Add($"{scope}.{member} {fault}");
			}

			foreach (var twice in Twice(Declarations(trees)))
				faults.Add(twice);
		}

		Assert.True(seen > 0, "no member was found");
		Assert.True(faults.Count == 0, $"{faults.Count} names not kept apart:\n" + string.Join("\n", faults.Take(200)));
	}

	/// <summary>
	/// Every member of a generated file: the scope it is declared in, whether that is the class the
	/// generator writes into, its name, and what tells two of one name apart — a method's parameters, or
	/// nothing for anything else.
	/// </summary>
	internal static IEnumerable<(string Scope, bool Host, string Name, string? Signature)> Declarations(IEnumerable<SyntaxTree> trees)
	{
		foreach (var tree in trees)
			foreach (var type in tree.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<TypeDeclarationSyntax>())
			{
				// The class the generator writes into is partial, and so is a suffix of it; a rule's type is the
				// author's, and what it declares is its captures (GRAM5018's last check).
				var host = type.Modifiers.Any(static modifier => modifier.Text == "partial");

				if (!host && type.Parent is TypeDeclarationSyntax && !IsGenerators(type))
					continue;

				var scope = string.Join(".", type.AncestorsAndSelf().OfType<BaseTypeDeclarationSyntax>().Reverse().Select(static one => one.Identifier.Text));

				foreach (var member in type.Members)
				{
					var declared = member switch
					{
						MethodDeclarationSyntax method       => [(method.Identifier.Text, (string?)string.Join(",", method.ParameterList.Parameters.Select(static one => string.Join(" ", one.Modifiers.Select(static modifier => modifier.Text).Append(one.Type?.ToString())))))],
						FieldDeclarationSyntax field         => field.Declaration.Variables.Select(static one => (one.Identifier.Text, (string?)null)),
						EventFieldDeclarationSyntax field    => field.Declaration.Variables.Select(static one => (one.Identifier.Text, (string?)null)),
						PropertyDeclarationSyntax property   => [(property.Identifier.Text, null)],
						EventDeclarationSyntax @event        => [(@event.Identifier.Text, null)],
						DelegateDeclarationSyntax @delegate  => [(@delegate.Identifier.Text, null)],
						ClassDeclarationSyntax nested when nested.Modifiers.Any(static modifier => modifier.Text == "partial") => [],
						BaseTypeDeclarationSyntax nested     => [(nested.Identifier.Text, null)],
						_                                    => Array.Empty<(string, string?)>(),
					};

					foreach (var (name, signature) in declared)
						yield return (scope, host, name, signature);
				}
			}
	}

	/// <summary>Whether a nested type is one the generator declares for its own use rather than a rule's type.</summary>
	static bool IsGenerators(TypeDeclarationSyntax type)
	{
		var name = type.Identifier.Text;

		return name.Contains(CSharpEmitter.NameMark, StringComparison.Ordinal) || CSharpEmitter.PublicNames.Contains(name) ||
			CSharpEmitter.SupportNames.Contains(name) || CSharpEmitter.TraceNames.Contains(name) ||
			type.Ancestors().OfType<TypeDeclarationSyntax>().Any(static one => !one.Modifiers.Any(static modifier => modifier.Text == "partial"));
	}

	/// <summary>
	/// What one scope declares twice: a name that is not a method's, or one method of one signature.
	/// Overloads are let be: two names the generator makes are one only where kind and identifiers are,
	/// so two methods of one name are one family over different inputs, never two families met by chance.
	/// </summary>
	internal static IEnumerable<string> Twice(IEnumerable<(string Scope, bool Host, string Name, string? Signature)> declarations)
	{
		foreach (var group in declarations.GroupBy(static one => (one.Scope, one.Name)))
		{
			var all = group.ToArray();

			if (all.Length > 1 && (all.Any(static one => one.Signature is null) ||
				all.Select(static one => one.Signature).Distinct(StringComparer.Ordinal).Count() < all.Length))
				yield return $"{group.Key.Scope}.{group.Key.Name} is declared {all.Length} times";
		}
	}

	static string SnapshotDirectory => Path.Combine(SourceRoot.Directory, "tests", "Snapshots");

	public static TheoryData<string> Snapshots => SnapshotTests.Snapshots;

	/// <summary>
	/// The families of names the generator once wrote into one place twice, each found by a review after
	/// the reserved list before it had missed it: a publication or a rule named like a reader, a factory
	/// over bytes, a lexer's table, a scanner of another machine, a reader's body or a scalar scanner.
	/// Each compiles now, and so does a rule named <c>DotGram</c>, which the names the generator keeps for
	/// itself begin with.
	/// </summary>
	[Theory]
	[InlineData("a", "R = r: 'r'\nparse R as Read stream\nparse R as ReadBuffered_Read", "buffered")]
	[InlineData("b", "R : @int = 'r' => @(1)\nR_Bytes : @int = 'b' => @(2)\nS : @int = a: R & b: R_Bytes => @(a + b)\nparse S as Both stream bytes", "bytes")]
	[InlineData("c", "trivia = { ' '* }\nnamespace Token\n{\n\ttrivia = none\n\tName = ['a'..'z']+\n\tNumber = ['0'..'9']+\n}\nS = Token.Name & '=' & Token.Number & eof\nparse S\nparse S as Scan_Accepts", "lexical")]
	[InlineData("d", "trivia = { ' '* }\nL : @int = d: ['0'..'9']+ => @(1)\nL_R : @int = d: ['a'..'z']+ => @(2)\nS : @int = a: L & b: L_R & eof => @(a + b)\nparse S\nparse S as Construct_L_R", "lexical")]
	[InlineData("e", "T = ['a'..'z']+\nT_A = ['0'..'9']+\nA_B = T & eof\nB = T_A & eof\nparse A_B\nparse B", "tape")]
	[InlineData("f", "Digits : @int = text: ['0'..'9']+ => @(int.Parse(text))\nA : @int = d: Digits & '^' & e: Digits & when @(d >= e) => @(d - e) | d: Digits => @(d)\nA_Body : @int = d: Digits & '^' & e: Digits & when @(d >= e) => @(d - e) | d: Digits => @(d)\nS : @int = a: A & ',' & b: A_Body & eof => @(a + b)\nparse S", "Tape")]
	[InlineData("g", "R : @string = t: { ['a'..'z']+ } => @(t)\nRead_R = x: '#'\nS : @string = a: R & when @(a.Length > 0) & ' ' & b: R & k: Read_R & ';' => @(a + b)\nfind S", "")]
	[InlineData("trace", "R = ['a' | 'c' | 'e']\nfind R as RecognizeClass0", "trace")]
	[InlineData("trace", "R = x: 'a'\nparse R as Read stream\nfind R as Find stream", "trace")]
	[InlineData("unwalked", "R = 'a'\nparse R as RecognizeFurther", "")]
	[InlineData("unwalked", "R = 'a' | \"ab\"\nparse R as RecognizeRanOut\nfind R as RecognizeAgreeing", "")]
	[InlineData("DotGram", "DotGram = x: 'a'\nS = DotGram & eof\nparse S\nparse DotGram", "")]
	[InlineData("DotGram", "DotGram = x: 'a'\nS = DotGram & eof\nparse S\nparse DotGram", "tape,buffered")]
	[InlineData("DotGram", "DotGram = x: 'a'\nS = DotGram & eof\nparse S\nparse DotGram", "Tape")]
	[InlineData("DotGram", "trivia = { ' '* }\nnamespace Token\n{\n\ttrivia = none\n\tName = ['a'..'z']+\n}\nDotGram = n: Token.Name\nS = DotGram & '=' & DotGram & eof\nparse S", "lexical")]
	public void A_family_that_once_met_another_compiles(string family, string grammar, string flags)
	{
		var said   = flags.Split(',');
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Probe",
			Direct        = !said.Contains("tape"),
			BufferedInput = said.Contains("buffered"),
			BufferedBytes = said.Contains("bytes"),
			Lexical       = said.Contains("lexical"),
			Trace         = said.Contains("trace"),
			Carrier       = said.Contains("Tape") ? CarrierKind.Tape : CarrierKind.Auto,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.False(result.Diagnostics.Any(static one => one.Severity == GramSeverity.Error), family + ": " + string.Join("; ", result.Diagnostics.Select(static one => one.Id + " " + one.Message)));

		_ = EmittedCode.Compile(result.Sources[0].Text, className: "Probe", sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	/// <summary>
	/// A value-returning external recognizer is a rule of its own, named after the external (`@Count`), which
	/// is no identifier: the names made from it are written with a digit first, which no rule of the author's
	/// can be, and the class compiles, beside a rule of the author's called after the external.
	/// </summary>
	[Theory]
	[InlineData("Rest : @int = @Count")]
	[InlineData("Rest : @int = v: @Count => @(v)\nS = Rest & eof")]
	[InlineData("Rest : @int = @Count\nCount = 'c'\nS : @int = r: Rest & Count & eof => @(r)\nparse S")]
	public void A_value_returning_external_names_its_rule_with_an_identifier(string grammar)
	{
		const string declarations = "public static bool Count(global::System.ReadOnlySpan<char> input, ref int pos, out int value) { value = 0; return false; }";

		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Probe",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(
				CSharpCompilation.Create(
					"Probe", [CSharpSyntaxTree.ParseText($"public partial class Probe {{ {declarations} }}", cancellationToken: TestContext.Current.CancellationToken)],
					EmittedCode.References, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)),
				"Probe"),
		});

		Assert.False(result.Diagnostics.Any(static one => one.Severity == GramSeverity.Error), string.Join("; ", result.Diagnostics.Select(static one => one.Id + " " + one.Message)));
		Assert.DoesNotContain("_@", result.Sources[0].Text, StringComparison.Ordinal);

		_ = EmittedCode.Compile(result.Sources[0].Text, className: "Probe", declarationMembers: declarations, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	/// <summary>A namespace joined to a rule whose name begins with the mark would put the mark inside an identifier.</summary>
	[Fact]
	public void A_rule_that_joins_into_the_mark_is_refused()
	{
		Assert.Contains(
			Compile("namespace A { DotGram_B = x: 'b' }\nS = A.DotGram_B & eof\nparse S").Diagnostics,
			static one => one.Id == CSharpEmitter.GeneratedNameClash);
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
