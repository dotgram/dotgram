using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

using DotGram.Language;

using Microsoft.CodeAnalysis.Text;

namespace DotGram.LanguageServer;

/// <summary>The completion provider's items as <c>textDocument/completion</c> carries them.</summary>
/// <remarks>
/// The span completion replaces is the list's default edit range, so that one item's insert
/// text is all it carries of an edit; a description, which can be a rule's whole definition, is
/// sent on <c>completionItem/resolve</c> for the item the user selects.
/// </remarks>
static class CompletionMap
{
	public static CompletionList Map(TextSpan applicable, IReadOnlyList<GramCompletionItem> items, int list, LineMap lines)
	{
		var result = new CompletionItem[items.Count];

		for (var index = 0; index < items.Count; index++)
			result[index] = Item(items[index], new CompletionItemData(list, index));

		return new CompletionList(false, new CompletionItemDefaults(lines.Range(applicable.Start, applicable.Length)), result);
	}

	static CompletionItem Item(GramCompletionItem item, CompletionItemData data)
	{
		return new CompletionItem(
			item.Label,
			item.Suffix.Length == 0 ? null : new CompletionItemLabelDetails(item.Suffix),
			null,
			item.SortText,
			item.FilterText,
			item.InsertText is { } insert && insert != item.Label ? insert : null,
			data);
	}

	/// <summary>The item with its description, as the client shows it beside the list; the rest of it as the client sent it.</summary>
	public static void Resolve(JsonObject item, GramCompletionItem source)
	{
		item["documentation"] = new JsonObject
		{
			["kind"]  = "plaintext",
			["value"] = source.Description,
		};
	}
}
