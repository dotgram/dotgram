using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

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

		var own = await ResolvePartAsync(
			project,
			language.SourceKind,
			language.GrammarSource,
			cancellationToken).ConfigureAwait(false);
		if (own.Kind != DslGrammarResolutionKind.Resolved || own.Text is null)
			return own;

		var included = new List<GrammarSplice.Part>(language.IncludedGrammars.Count);
		var candidates = new List<TextDocument>(own.Candidates);
		foreach (var part in language.IncludedGrammars)
		{
			var resolved = await ResolvePartAsync(
				project,
				part.SourceKind,
				part.GrammarSource,
				cancellationToken).ConfigureAwait(false);
			candidates.AddRange(resolved.Candidates);

			if (resolved.Kind != DslGrammarResolutionKind.Resolved || resolved.Text is null)
				return new DslGrammarResolution(
					resolved.Kind,
					null,
					null,
					candidates);

			included.Add(new GrammarSplice.Part(resolved.Text, part.Name, null));
		}

		var joined = GrammarSplice.Join(new GrammarSplice.Part(own.Text, null, null), included);
		return new DslGrammarResolution(
			DslGrammarResolutionKind.Resolved,
			joined.Text,
			own.Document,
			candidates);
	}

	static async Task<DslGrammarResolution> ResolvePartAsync(
		Project project,
		DslGrammarSourceKind sourceKind,
		string grammarSource,
		CancellationToken cancellationToken)
	{
		if (sourceKind == DslGrammarSourceKind.Embedded)
			return new DslGrammarResolution(
				DslGrammarResolutionKind.Resolved,
				grammarSource,
				null,
				Array.Empty<TextDocument>());

		var candidates = project.AdditionalDocuments
			.Where(document => document.FilePath is { } path && Matches(path, grammarSource))
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
