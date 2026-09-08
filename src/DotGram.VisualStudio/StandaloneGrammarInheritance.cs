using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.VisualStudio;

/// <summary>Builds the inherited tail used to analyze a standalone grammar in its C# host context.</summary>
static class StandaloneGrammarInheritance
{
	const string GramAttribute = "DotGram.GramAttribute";

	public static async Task<string?> ResolveAsync(
		Solution solution,
		string filePath,
		CancellationToken cancellationToken)
	{
		var grammar = solution.Projects
			.SelectMany(static project => project.AdditionalDocuments)
			.FirstOrDefault(candidate => string.Equals(
				candidate.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
		if (grammar is null)
			return null;

		var project = grammar.Project;
		var host = await HostAsync(project, filePath, cancellationToken).ConfigureAwait(false);
		if (host is null)
			return null;

		var included = new List<GrammarSplice.Part>();
		for (var current = host.BaseType; current is not null; current = current.BaseType)
		{
			var attribute = PrimaryGram(current.GetAttributes());
			if (attribute is null)
				continue;

			var source = attribute.ConstructorArguments.Length == 0
				? current.Name + ".gram"
				: attribute.ConstructorArguments[0].Value as string;
			if (source is null)
				continue;

			var text = IsFile(source)
				? await FileTextAsync(project, source, cancellationToken).ConfigureAwait(false)
				: source;
			if (text is null)
				continue;

			var name = attribute.NamedArguments
				.FirstOrDefault(static argument => argument.Key == "IncludedAs")
				.Value.Value as string ?? current.Name;
			included.Add(new GrammarSplice.Part(text, name, null));
		}

		return included.Count == 0
			? null
			: GrammarSplice.Join(new GrammarSplice.Part("", null, null), included).Text;
	}

	static async Task<INamedTypeSymbol?> HostAsync(
		Project project,
		string grammarPath,
		CancellationToken cancellationToken)
	{
		foreach (var document in project.Documents)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (root is null || model is null)
				continue;

			foreach (var declaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
			{
				var type = model.GetDeclaredSymbol(declaration, cancellationToken) as INamedTypeSymbol;
				var attribute = type is null ? null : PrimaryGram(type.GetAttributes());
				if (type is null || attribute is null)
					continue;

				var source = attribute.ConstructorArguments.Length == 0
					? type.Name + ".gram"
					: attribute.ConstructorArguments[0].Value as string;
				if (source is not null && IsFile(source) && Matches(grammarPath, source))
					return type;
			}
		}

		return null;
	}

	static AttributeData? PrimaryGram(IEnumerable<AttributeData> attributes) =>
		attributes.FirstOrDefault(static attribute =>
			attribute.AttributeClass?.ToDisplayString() == GramAttribute &&
			attribute.NamedArguments.All(static argument => argument.Key != "Suffix"));

	static async Task<string?> FileTextAsync(
		Project project,
		string source,
		CancellationToken cancellationToken)
	{
		var document = project.AdditionalDocuments.FirstOrDefault(candidate =>
			candidate.FilePath is not null && Matches(candidate.FilePath, source));
		return document is null
			? null
			: (await document.GetTextAsync(cancellationToken).ConfigureAwait(false)).ToString();
	}

	static bool IsFile(string source) =>
		source.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) &&
		source.IndexOf('\r') < 0 && source.IndexOf('\n') < 0;

	static bool Matches(string filePath, string wanted)
	{
		var path = filePath.Replace('/', '\\');
		var suffix = wanted.Replace('/', '\\');
		if (!path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
			return false;

		var boundary = path.Length - suffix.Length - 1;
		return boundary < 0 || path[boundary] == '\\';
	}
}
