using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace DotGram.VisualStudio;

public enum DslGrammarResolutionKind
{
	Resolved,
	Missing,
	Ambiguous,
}

public sealed class DslGrammarResolution(
	DslGrammarResolutionKind kind,
	string? text,
	TextDocument? document,
	IReadOnlyList<TextDocument> candidates)
{
	public DslGrammarResolutionKind Kind { get; } = kind;
	public string? Text { get; } = text;
	public TextDocument? Document { get; } = document;
	public IReadOnlyList<TextDocument> Candidates { get; } = candidates;
}

/// <summary>
/// Resolves the grammar source seen by tooling with the same path matching rules as
/// the source generator. No candidate is selected when a project contains an
/// ambiguous suffix match.
/// </summary>
public static class DslGrammarSourceResolver
{
	public static async Task<DslGrammarResolution> ResolveAsync(
		Project project,
		DslLanguageDefinition language,
		CancellationToken cancellationToken = default)
	{
		if (project is null)
			throw new ArgumentNullException(nameof(project));
		if (language is null)
			throw new ArgumentNullException(nameof(language));

		if (language.SourceKind == DslGrammarSourceKind.Embedded)
			return new DslGrammarResolution(
				DslGrammarResolutionKind.Resolved,
				language.GrammarSource,
				null,
				Array.Empty<TextDocument>());

		var candidates = project.AdditionalDocuments
			.Where(document => document.FilePath is { } path && Matches(path, language.GrammarSource))
			.ToArray();

		if (candidates.Length == 0)
			return new DslGrammarResolution(
				DslGrammarResolutionKind.Missing,
				null,
				null,
				Array.Empty<TextDocument>());

		if (candidates.Length > 1)
			return new DslGrammarResolution(
				DslGrammarResolutionKind.Ambiguous,
				null,
				null,
				candidates);

		var text = await candidates[0].GetTextAsync(cancellationToken).ConfigureAwait(false);
		return new DslGrammarResolution(
			DslGrammarResolutionKind.Resolved,
			text.ToString(),
			candidates[0],
			candidates);
	}

	static bool Matches(string filePath, string wanted)
	{
		var normalizedPath   = filePath.Replace('/', '\\');
		var normalizedWanted = wanted.Replace('/', '\\');

		if (!normalizedPath.EndsWith(normalizedWanted, StringComparison.OrdinalIgnoreCase))
			return false;

		var boundary = normalizedPath.Length - normalizedWanted.Length - 1;
		return boundary < 0 || normalizedPath[boundary] == '\\';
	}
}
