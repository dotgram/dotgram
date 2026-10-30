using System;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// The tooling attributes' names, written by the emitter and read by the generator and the
/// editor: each is a name the emitted attributes declare, and the descriptor a parser carries is
/// written under it.
/// </summary>
public sealed class GramMetadataNamesTests
{
	public static TheoryData<string, string> Names()
	{
		return new TheoryData<string, string>
		{
			{ GramMetadataNames.Language,           GramMetadataNames.QualifiedLanguage },
			{ GramMetadataNames.Classify,           GramMetadataNames.QualifiedClassify },
			{ GramMetadataNames.LanguageDescriptor, GramMetadataNames.QualifiedLanguageDescriptor },
			{ GramMetadataNames.ToolingGuard,       GramMetadataNames.QualifiedToolingGuard },
			{ GramMetadataNames.ToolingExternal,    GramMetadataNames.QualifiedToolingExternal },
		};
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void Each_name_is_an_attribute_the_emitter_declares(string name, string qualified)
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var compilation       = CSharpCompilation.Create(
			"Support",
			[
				CSharpSyntaxTree.ParseText(SupportEmitter.Attributes, cancellationToken: cancellationToken),
				CSharpSyntaxTree.ParseText(SupportEmitter.EmbeddedAttribute, cancellationToken: cancellationToken),
			],
			[MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		var type = compilation.GetTypeByMetadataName(qualified);

		Assert.DoesNotContain(
			compilation.GetDiagnostics(cancellationToken), static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
		Assert.NotNull(type);
		Assert.Equal(name, type.Name);
		Assert.Equal(GramMetadataNames.Namespace, type.ContainingNamespace.ToDisplayString());
		Assert.Equal(qualified, type.ToDisplayString());
		Assert.Equal("System.Attribute", type.BaseType?.ToDisplayString());
	}

	[Fact]
	public void A_parser_of_a_language_carries_its_descriptor_under_the_name()
	{
		var compilation = GramCompiler.Compile("A = 'a'\nparse A", new GramCompilerOptions
		{
			CSharpScanner  = RoslynCSharpScanner.Instance,
			LanguageId     = "test.language",
			LanguageSource = "A = 'a'\nparse A",
		});

		Assert.DoesNotContain(compilation.Diagnostics, static diagnostic => diagnostic.Severity == GramSeverity.Error);
		Assert.Contains(
			compilation.Sources,
			static source => source.Text.Contains("[global::" + GramMetadataNames.QualifiedLanguageDescriptor + "(", StringComparison.Ordinal));
	}
}
