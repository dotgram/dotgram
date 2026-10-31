using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.QuickInfo;

namespace DotGram.LanguageServer;

/// <summary>
/// Completion and Quick Info in the C# of a grammar, asked of Roslyn's Features: what the shared core
/// takes as delegates, since the core reads C# with Roslyn and does not complete it.
/// </summary>
/// <remarks>The same questions the Visual Studio extension asks of its own Roslyn, over the server's workspace.</remarks>
sealed class RoslynFeatures(RoslynGramCompletion core)
{
	public RoslynGramCompletion Core { get; } = core;

	public async Task<IReadOnlyList<GramCompletionItem>> CompletionAsync(string prefix, CancellationToken cancellationToken)
	{
		var project = Core.Project();

		if (project is null)
			return [];

		var document = RoslynGramCompletion.SyntheticDocument(project, prefix);
		var service  = CompletionService.GetService(document);

		if (service is null)
			return [];

		var completions = await service.GetCompletionsAsync(
			document,
			RoslynGramCompletion.ExpressionOffset + prefix.Length,
			cancellationToken: cancellationToken).ConfigureAwait(false);

		return completions.ItemsList
			.Select(static item => new GramCompletionItem(
				item.DisplayText,
				RoslynGramCompletion.CSharpDescription,
				item.DisplayText,
				item.InlineDescription ?? "",
				item.SortText,
				item.FilterText,
				FromCSharp: true))
			.ToArray();
	}

	public async Task<RoslynGramQuickInfo?> QuickInfoAsync(string expression, int position, CancellationToken cancellationToken)
	{
		var project = Core.Project();

		if (project is null)
			return null;

		var document = RoslynGramCompletion.SyntheticDocument(project, expression);
		var service  = QuickInfoService.GetService(document);

		if (service is null)
			return null;

		var item = await service.GetQuickInfoAsync(
			document,
			RoslynGramCompletion.ExpressionOffset + position,
			cancellationToken).ConfigureAwait(false);

		if (item is null)
			return null;

		return new RoslynGramQuickInfo(
			item.Sections
				.Select(static section => (IReadOnlyList<RoslynTaggedRun>)section.TaggedParts
					.Select(static part => new RoslynTaggedRun(part.Tag, part.Text))
					.ToArray())
				.ToArray());
	}
}
