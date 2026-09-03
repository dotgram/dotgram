using System;
using System.Threading;
using System.Threading.Tasks;

using DotGram.VisualStudio;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class RoslynGramCompletionTests
{
	[Fact]
	public async Task QualifiedMemberPrefersEnumFieldOverSameNamedMethod()
	{
		using var workspace = new AdhocWorkspace();
		var project = workspace.AddProject("Navigation", LanguageNames.CSharp)
			.AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
			.AddDocument("Symbols.cs", """
				namespace Example;
				public enum SqlPredicateKind { Quantified }
				public static class SqlParser { public static string Quantified(string value) => value; }
				""").Project;
		const string expression = "SqlPredicateKind.Quantified";

		var symbol = await RoslynGramCompletion.QualifiedMemberAsync(
			project,
			expression,
			expression.IndexOf("Quantified", StringComparison.Ordinal) + 1,
			CancellationToken.None);

		var field = Assert.IsAssignableFrom<IFieldSymbol>(symbol);
		Assert.Equal("SqlPredicateKind", field.ContainingType.Name);
	}

	[Fact]
	public async Task UnqualifiedMemberBelongsToStandaloneGrammarHost()
	{
		using var workspace = new AdhocWorkspace();
		var project = workspace.AddProject("Navigation", LanguageNames.CSharp)
			.AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
			.AddDocument("Host.cs", """
				namespace Example;
				public static class GrammarHost { private static decimal Raise(decimal value) => value; }
				public static class OtherHost { private static decimal Raise(decimal value) => value; }
				""").Project;
		var compilation = await project.GetCompilationAsync(TestContext.Current.CancellationToken);
		var host = compilation!.GetTypeByMetadataName("Example.GrammarHost")!;
		var member = host.GetMembers("Raise")[0];
		var other = compilation.GetTypeByMetadataName("Example.OtherHost")!.GetMembers("Raise")[0];
		const string expression = "Raise(value, 1)";
		var position = expression.IndexOf("Raise", StringComparison.Ordinal) + 1;

		Assert.True(RoslynGramCompletion.IsUnqualifiedHostMember(expression, position, member, host));
		Assert.False(RoslynGramCompletion.IsUnqualifiedHostMember(expression, position, other, host));
		Assert.False(RoslynGramCompletion.IsUnqualifiedHostMember("Other.Raise(value)", 7, member, host));
	}

	[Fact]
	public async Task FindsHostMemberInsideEmbeddedGrammar()
	{
		using var workspace = new AdhocWorkspace();
		const string source = """"
			namespace DotGram
			{
				public sealed class GramAttribute : System.Attribute
				{
					public GramAttribute(string source) { }
				}
			}
			namespace Example
			{
				[DotGram.Gram("""
					Value : @decimal = number: '1' => @(Raise(number))
					""")]
				public static class GrammarHost
				{
					private static decimal Raise(decimal value) => value;
				}
			}
			"""";
		var project = workspace.AddProject("Navigation", LanguageNames.CSharp)
			.AddMetadataReference(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
			.AddDocument("Host.cs", source, filePath: "P:\\Host.cs").Project;
		var compilation = await project.GetCompilationAsync(TestContext.Current.CancellationToken);
		var member = compilation!.GetTypeByMetadataName("Example.GrammarHost")!.GetMembers("Raise")[0];

		var references = await RoslynGramCompletion.EmbeddedGrammarReferencesAsync(
			member, project, TestContext.Current.CancellationToken);

		var reference = Assert.Single(references);
		Assert.Equal(source.IndexOf("Raise(number)", StringComparison.Ordinal), reference.Position);
	}
}
