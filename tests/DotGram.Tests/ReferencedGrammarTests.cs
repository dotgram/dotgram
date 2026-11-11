using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A grammar in another assembly, included and inherited through the <c>[GramSource]</c> its
/// generator wrote onto it, and the parsers that come out run (§6.7).
/// </summary>
/// <remarks>
/// Two compilations, each driven by the generator in memory. The library's grammar is a
/// <c>.gram</c> file only the library's compilation is handed, and the library is referenced by
/// the consumer as an image, so the grammar's text reaches the consumer through the attribute or
/// not at all. The consumer is then loaded into a context of its own, its reference to the
/// library resolved there, and called.
/// </remarks>
public sealed class ReferencedGrammarTests
{
	const string Library = """
		namespace GrammarLibrary
		{
			[DotGram.Gram("Lexemes.gram")]
			public partial class Lexemes { }
		}
		""";

	const string Lexemes = """
		Number : @int = n: ['0'..'9']+ => @(int.Parse(n))
		parse Number
		""";

	const string Consumer = """
		using GrammarLibrary;

		[DotGram.GramInclude(typeof(Lexemes), Alias = "L")]
		[DotGram.Gram("using L;\nSum : @int = a: L.Number & '+' & b: L.Number => @(a + b)\nparse Sum")]
		public partial class ImportedGrammar { }

		[DotGram.Gram("using Lexemes;\nStart : @int = n: Number & '!' => @(n)\nparse Start")]
		public partial class DerivedGrammar : Lexemes { }
		""";

	[Fact]
	public void An_include_and_a_base_read_GramSource_from_a_referenced_assembly()
	{
		var library  = Emit("GrammarLibrary", Library, ("Lexemes.gram", Lexemes));
		var context  = new Context("GrammarLibrary", library);
		var consumer = context.LoadFromStream(new MemoryStream(Emit("GrammarConsumer", Consumer, reference: library)));

		Assert.Equal(42, Call(consumer, "ImportedGrammar", "ParseSum", "19+23"));
		Assert.False(IsSuccess(consumer, "ImportedGrammar", "TryParseSum", "19+x"));
		Assert.Equal(42, Call(consumer, "DerivedGrammar", "ParseStart", "42!"));
		Assert.False(IsSuccess(consumer, "DerivedGrammar", "TryParseStart", "42?"));
	}

	/// <summary>
	/// A warning inside a grammar the assembly carries lands on the attribute of the class that
	/// includes it: the carried text is written in no file, so there is no place in one to find.
	/// </summary>
	/// <remarks>
	/// It used to be looked for as if it were that class's literal — found in the carried text
	/// and its offset applied from the start of the class's file, which put the squiggle on
	/// whatever stood there.
	/// </remarks>
	[Fact]
	public void A_warning_in_a_carried_grammar_lands_on_the_including_attribute()
	{
		// `Other` is published by the library and reached by nothing in the consumer.
		var library = Emit("GrammarLibrary", Library, ("Lexemes.gram", Lexemes + "\nOther = 'x' & Number\nparse Other"));

		const string consumer = """
			// Lines above, so that an offset into the carried text
			// would land somewhere other than the attribute.
			using GrammarLibrary;

			[DotGram.GramInclude(typeof(Lexemes), Alias = "L")]
			[DotGram.Gram("using L;\nSum : @int = a: L.Number & '+' & b: L.Number => @(a + b)\nparse Sum")]
			public partial class ImportedGrammar { }
			""";

		var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var compilation  = CSharpCompilation.Create(
			"GrammarConsumer",
			[CSharpSyntaxTree.ParseText(consumer, parseOptions, "GrammarConsumer.cs", cancellationToken: TestContext.Current.CancellationToken)],
			((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
				.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
				.Append(MetadataReference.CreateFromImage(library)),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parseOptions)
			.RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics, TestContext.Current.CancellationToken);

		var unreached = Assert.Single(diagnostics, static one => one.Id == "GRAM4018");

		Assert.Contains("Other", unreached.GetMessage(), StringComparison.Ordinal);
		Assert.Equal("GrammarConsumer.cs", unreached.Location.SourceTree?.FilePath);
		Assert.Equal(consumer.IndexOf("DotGram.Gram(", StringComparison.Ordinal), unreached.Location.SourceSpan.Start);
	}

	/// <summary>
	/// An included class in an assembly referenced under an extern alias, reached through a global
	/// alias of its namespace: the generated file imports it through the extern alias, which is the
	/// one root its namespace is under, and builds with warnings as errors.
	/// </summary>
	[Fact]
	public void An_include_from_an_assembly_under_an_extern_alias_is_imported_through_the_alias()
	{
		const string library = """
			namespace Lib
			{
				[DotGram.Gram("Rest = @M\nparse Rest")]
				public partial class G
				{
					public static bool M(System.ReadOnlySpan<char> input, ref int pos)
					{
						pos = input.Length;

						return true;
					}
				}
			}
			""";

		const string consumer = """
			extern alias X;
			global using Lib = X::Lib;

			namespace App
			{
				[DotGram.GramInclude(typeof(Lib.G))]
				[DotGram.Gram("using G;\nStart = Rest\nparse Start")]
				public partial class Host { }
			}
			""";

		var image    = Emit("AliasLibrary", library);
		var context  = new Context("AliasLibrary", image);
		var built    = context.LoadFromStream(new MemoryStream(Emit("AliasConsumer", consumer, reference: image, aliases: ["X"])));

		Assert.True(IsSuccess(built, "App.Host", "TryParseStart", "abc"));
	}

	/// <summary>
	/// An assembly built by DotGram 0.2 carries its attributes under the names they had then —
	/// <c>IncludedAs</c> on <c>[Gram]</c>, <c>As</c> on <c>[GramInclude]</c>, <c>[GramOptions(Suffix = …)]</c>
	/// — and a grammar is still included from it under the name it says.
	/// </summary>
	/// <remarks>
	/// The library is the one 0.2.0 built, checked in as it came out (tests/DotGram020Library), so
	/// what is read is what 0.2 wrote and not a guess at it.
	/// </remarks>
	[Fact]
	public void A_grammar_is_included_from_an_assembly_built_by_DotGram_0_2()
	{
		const string consumer = """
			using OldLib;

			[DotGram.GramInclude(typeof(Lexemes))]
			[DotGram.Gram("using Lex;\nSum : @int = a: Lex.Number & '+' & b: Lex.Number => @(a + b)\nparse Sum")]
			public partial class ByDefaultAlias { }

			[DotGram.GramInclude(typeof(Pairs))]
			[DotGram.Gram("using Pairs;\nStart : @int = p: Pair & '!' => @(p)\nparse Start")]
			public partial class ByAlias { }
			""";

		var image   = File.ReadAllBytes(Path.Combine(SourceRoot.Directory, "tests", "DotGram020Library", "OldLib.dll"));
		var context = new Context("OldLib", image);
		// GRAM4018 is said, wrongly, of 'Number', reached only through the include Pairs makes under
		// an alias of its own. 0.2.0 says it too; it is not about the names 0.2 wrote.
		var built   = context.LoadFromStream(new MemoryStream(Reporting("OldConsumer", consumer, null, image, null, out _, permitted: "GRAM4018")));

		Assert.Equal(42, Call(built, "ByDefaultAlias", "ParseSum", "19+23"));
		Assert.Equal(42, Call(built, "ByAlias", "ParseStart", "6,7!"));
	}

	/// <summary>
	/// A grammar a referenced assembly carries is the one included, whatever a local file of the
	/// same name says: a fixture called <c>Lexemes.gram</c> here is not the library's grammar. Where it
	/// says something else the build is told, as information.
	/// </summary>
	[Theory]
	[InlineData("Number : @int = n: ['0'..'9']+ => @(int.Parse(n) * 100)\nparse Number", true)]
	[InlineData(Lexemes, false)]
	public void A_local_file_does_not_take_the_place_of_a_referenced_grammar(string local, bool told)
	{
		var library  = Emit("GrammarLibrary", Library, ("Lexemes.gram", Lexemes));
		var context  = new Context("GrammarLibrary", library);
		var consumer = context.LoadFromStream(new MemoryStream(
			Reporting("GrammarConsumer", Consumer, ("fixtures/Lexemes.gram", local), library, null, out var diagnostics)));

		Assert.Equal(42, Call(consumer, "ImportedGrammar", "ParseSum", "19+23"));
		Assert.Equal(42, Call(consumer, "DerivedGrammar", "ParseStart", "42!"));
		Assert.Equal(told ? 2 : 0, diagnostics.Count(static one => one.Id == "GRAM0017"));
		Assert.All(diagnostics.Where(static one => one.Id == "GRAM0017"), static one =>
		{
			Assert.Equal(DiagnosticSeverity.Info, one.Severity);
			Assert.Contains("fixtures/Lexemes.gram", one.GetMessage(), StringComparison.Ordinal);
		});
	}

	/// <summary>A class compiled beside the host is read from its file, which is the one being edited.</summary>
	[Fact]
	public void A_grammar_of_this_compilation_is_read_from_its_file()
	{
		const string source = """
			[DotGram.Gram]
			[DotGram.GramSource("Number : @int = n: ['0'..'9']+ => @(int.Parse(n) * 100)\nparse Number")]
			internal partial class Lexemes { }

			[DotGram.GramInclude(typeof(Lexemes))]
			[DotGram.Gram("using Lexemes;\nSum : @int = a: Number & '+' & b: Number => @(a + b)\nparse Sum")]
			public partial class Summing { }
			""";

		var built = new Context("None", []).LoadFromStream(new MemoryStream(Reporting("Beside", source, ("Lexemes.gram", Lexemes), null, null, out var diagnostics)));

		Assert.Equal(42, Call(built, "Summing", "ParseSum", "19+23"));
		Assert.DoesNotContain(diagnostics, static one => one.Id == "GRAM0017");
	}

	static object? Call(Assembly assembly, string type, string method, string input)
	{
		return assembly.GetType(type, true)!.GetMethod(method, [typeof(string)])!.Invoke(null, [input]);
	}

	static bool IsSuccess(Assembly assembly, string type, string method, string input)
	{
		var match = Call(assembly, type, method, input)!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
	}

	/// <summary>The generator run over one source, and what it and the source make together.</summary>
	/// <param name="aliases">The extern aliases the reference is given; with them, the build holds warnings to be errors.</param>
	static byte[] Emit(string name, string source, (string Path, string Text)? file = null, byte[]? reference = null, string[]? aliases = null)
	{
		return Reporting(name, source, file, reference, aliases, out _);
	}

	/// <summary>The same, and what the generator reported.</summary>
	static byte[] Reporting(
		string                      name,
		string                      source,
		(string Path, string Text)? file,
		byte[]?                     reference,
		string[]?                   aliases,
		out ImmutableArray<Diagnostic> reported,
		string?                     permitted = null)
	{
		var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var compilation  = CSharpCompilation.Create(
			name,
			[CSharpSyntaxTree.ParseText(source, parseOptions, name + ".cs")],
			((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
				.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
				.Concat(reference is null ? [] : [MetadataReference.CreateFromImage(reference).WithAliases(aliases ?? [])]),
			new CSharpCompilationOptions(
				OutputKind.DynamicallyLinkedLibrary,
				generalDiagnosticOption: aliases is null ? ReportDiagnostic.Default : ReportDiagnostic.Error));

		CSharpGeneratorDriver
			.Create(
				[new GramGenerator().AsSourceGenerator()],
				additionalTexts: file is { } one ? [new InMemoryFile(one.Path, one.Text)] : [],
				parseOptions: parseOptions)
			.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics, TestContext.Current.CancellationToken);

		Assert.Empty(diagnostics.Where(one => one.Severity != DiagnosticSeverity.Info && one.Id != permitted));

		reported = diagnostics;

		using var stream = new MemoryStream();

		var emitted = output.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(emitted.Success, string.Join("\n", emitted.Diagnostics.Where(static one => one.Severity == DiagnosticSeverity.Error)));

		return stream.ToArray();
	}

	/// <summary>A .gram file that never touched a disk.</summary>
	sealed class InMemoryFile(string path, string text) : AdditionalText
	{
		public override string Path { get; } = path;

		public override SourceText GetText(CancellationToken cancellationToken = default)
		{
			return SourceText.From(text);
		}
	}

	/// <summary>A context that finds the library in memory, where nothing else can.</summary>
	sealed class Context(string name, byte[] image) : AssemblyLoadContext
	{
		protected override Assembly? Load(AssemblyName assembly)
		{
			return assembly.Name == name ? LoadFromStream(new MemoryStream(image)) : null;
		}
	}
}
