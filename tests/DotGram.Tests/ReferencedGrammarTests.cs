using System;
using System.Collections.Generic;
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

		[DotGram.GramInclude(typeof(Lexemes), As = "L")]
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

			[DotGram.GramInclude(typeof(Lexemes), As = "L")]
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
	static byte[] Emit(string name, string source, (string Path, string Text)? file = null, byte[]? reference = null)
	{
		var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var compilation  = CSharpCompilation.Create(
			name,
			[CSharpSyntaxTree.ParseText(source, parseOptions, name + ".cs")],
			((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
				.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
				.Concat(reference is null ? [] : [MetadataReference.CreateFromImage(reference)]),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		CSharpGeneratorDriver
			.Create(
				[new GramGenerator().AsSourceGenerator()],
				additionalTexts: file is { } one ? [new InMemoryFile(one.Path, one.Text)] : [],
				parseOptions: parseOptions)
			.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics, TestContext.Current.CancellationToken);

		Assert.Empty(diagnostics.Where(static one => one.Severity != DiagnosticSeverity.Info));

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
