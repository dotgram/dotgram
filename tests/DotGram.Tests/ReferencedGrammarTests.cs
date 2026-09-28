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
