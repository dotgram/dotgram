using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.VisualStudio;

/// <summary>Keeps tooling contract targets alive while the runtime graph is pruned.</summary>
internal static class DslGrammarNormalizer
{
	const string ToolingPublicationPrefix = "__DotGramTooling";

	public static RecognitionGraph Normalize(GrammarModel model, IEnumerable<string> contractRules)
	{
		var names = new HashSet<string>(contractRules, StringComparer.Ordinal);
		if (names.Count == 0)
			return GrammarNormalizer.Normalize(model);

		var rules = Rules(model.Root)
			.GroupBy(static rule => rule.Name, StringComparer.Ordinal)
			.Where(group => group.Count() == 1 && names.Contains(group.Key))
			.Select(static group => group.Single())
			.ToArray();
		if (rules.Length == 0)
			return GrammarNormalizer.Normalize(model);

		var publications = model.Publications.ToList();
		for (var index = 0; index < rules.Length; index++)
		{
			var rule = rules[index];
			publications.Add(new Publication(
				PublishKind.Parse,
				rule,
				ToolingPublicationPrefix + index,
				rule.Declaration?.At ?? default,
				rule.Namespace,
				new Dictionary<RuleSymbol, RuleSymbol>(),
				Array.Empty<ResolvedRebinding>()));
		}

		var toolingModel = new GrammarModel(
			model.Root,
			model.Bindings,
			model.WithBindings,
			model.WithOwnRebindings,
			model.Trivia,
			publications,
			model.Diagnostics)
		{
			Context = model.Context,
			State = model.State,
		};
		return GrammarNormalizer.Normalize(toolingModel);
	}

	public static bool IsToolingPublication(Publication publication) =>
		publication.MethodName.StartsWith(ToolingPublicationPrefix, StringComparison.Ordinal);

	static IEnumerable<RuleSymbol> Rules(GrammarNamespace grammarNamespace)
	{
		foreach (var rule in grammarNamespace.Rules.Values)
			yield return rule;

		foreach (var nested in grammarNamespace.Nested)
		foreach (var rule in Rules(nested))
			yield return rule;
	}
}
