using System;
using System.Threading.Tasks;

using DotGram.VisualStudio;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class StandaloneGrammarInheritanceTests
{
	[Fact]
	public async Task AppendsTheUnsuffixedBaseGrammarToAStandaloneDialect()
	{
		const string declarations = """
			namespace DotGram
			{
				[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
				sealed class GramAttribute(string source) : System.Attribute
				{
					public string IncludedAs { get; set; } = "";
					public string Suffix { get; set; } = "";
				}
			}

			[DotGram.Gram("Alternative.gram", Suffix = "Immediate", IncludedAs = "Wrong")]
			[DotGram.Gram("SqlStandard92.gram", IncludedAs = "Sql92")]
			abstract class SqlStandard92;

			[DotGram.Gram("TransactSql.gram")]
			abstract class TransactSql : SqlStandard92;
			""";

		using var workspace = new AdhocWorkspace();
		var project = workspace.AddProject(ProjectInfo.Create(
			ProjectId.CreateNewId(),
			VersionStamp.Default,
			"Parsers",
			"Parsers",
			LanguageNames.CSharp,
			parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
			metadataReferences: [MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)]));
		project = project.AddDocument("Parsers.cs", SourceText.From(declarations), filePath: @"P:\Parsers\Parsers.cs").Project;
		project = project.AddAdditionalDocument(
			"SqlStandard92.gram",
			SourceText.From("Word = ['a'..'z']+"),
			filePath: @"P:\Parsers\SqlStandard92.gram").Project;
		project = project.AddAdditionalDocument(
			"Alternative.gram",
			SourceText.From("Wrong = 'x'"),
			filePath: @"P:\Parsers\Alternative.gram").Project;
		project = project.AddAdditionalDocument(
			"TransactSql.gram",
			SourceText.From("using Sql92;\nStart = Sql92.Word"),
			filePath: @"P:\Parsers\TransactSql.gram").Project;

		var inherited = await StandaloneGrammarInheritance.ResolveAsync(
			project.Solution,
			@"P:\Parsers\TransactSql.gram",
			TestContext.Current.CancellationToken);

		Assert.Contains("namespace Sql92\n{\nWord = ['a'..'z']+", inherited, StringComparison.Ordinal);
		Assert.DoesNotContain("namespace Wrong", inherited, StringComparison.Ordinal);
	}
}
