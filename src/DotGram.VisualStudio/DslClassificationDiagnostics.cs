using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.VisualStudio;

public static class DslClassificationDiagnostics
{
	public static async Task<IReadOnlyList<HostDiagnostic>> AnalyzeAsync(
		Document document,
		SyntaxNode root,
		Compilation compilation,
		CancellationToken cancellationToken = default)
	{
		if (document is null)
			throw new ArgumentNullException(nameof(document));
		if (root is null)
			throw new ArgumentNullException(nameof(root));
		if (compilation is null)
			throw new ArgumentNullException(nameof(compilation));

		var catalog = DslLanguageDiscovery.Discover(compilation, cancellationToken);
		var result = new List<HostDiagnostic>();

		foreach (var language in catalog.Languages)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!language.Classifications.Any(definition =>
				definition.Attribute.ApplicationSyntaxReference?.SyntaxTree == root.SyntaxTree))
				continue;

			var resolution = await DslGrammarSourceResolver.ResolveAsync(
				document.Project,
				language,
				cancellationToken).ConfigureAwait(false);
			if (resolution.Kind != DslGrammarResolutionKind.Resolved || resolution.Text is null)
				continue;

			var binding = DslClassificationBinder.Bind(language, resolution.Text);
			foreach (var diagnostic in binding.Diagnostics)
			{
				var syntaxReference = diagnostic.Definition.Attribute.ApplicationSyntaxReference;
				if (syntaxReference?.SyntaxTree != root.SyntaxTree ||
					await syntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) is not AttributeSyntax syntax)
					continue;

				var span = TargetSpan(syntax);
				result.Add(new HostDiagnostic(
					new GramDiagnostic(
						Id(diagnostic.Kind),
						diagnostic.Message,
						0,
						0,
						GramSeverity.Warning),
					span,
					isExact: true));
			}
		}

		return result;
	}

	static TextSpan TargetSpan(AttributeSyntax attribute)
	{
		if (attribute.ArgumentList?.Arguments.FirstOrDefault()?.Expression is not { } expression)
			return attribute.Span;

		if (expression is LiteralExpressionSyntax literal &&
			CSharpStringMap.TryCreate(literal.Token, out var map) &&
			map!.TryMap(0, literal.Token.ValueText.Length, out var mapped))
			return mapped;

		return expression.Span;
	}

	static string Id(DslClassificationBindingDiagnosticKind kind) => kind switch
	{
		DslClassificationBindingDiagnosticKind.MalformedTarget => "GRAM5001",
		DslClassificationBindingDiagnosticKind.UnknownRule     => "GRAM5002",
		DslClassificationBindingDiagnosticKind.AmbiguousRule   => "GRAM5003",
		DslClassificationBindingDiagnosticKind.UnknownCapture  => "GRAM5004",
		DslClassificationBindingDiagnosticKind.DuplicateTarget => "GRAM5005",
		_ => throw new ArgumentOutOfRangeException(nameof(kind)),
	};
}
