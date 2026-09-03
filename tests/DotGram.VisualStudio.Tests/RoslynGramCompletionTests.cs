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
}
