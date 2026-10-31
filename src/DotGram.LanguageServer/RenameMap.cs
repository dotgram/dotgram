using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Language;

namespace DotGram.LanguageServer;

/// <summary>The rename provider's answers as <c>textDocument/prepareRename</c> and <c>textDocument/rename</c> carry them.</summary>
/// <remarks>
/// The edits are grouped by document; the provider's are all in the document renamed in, as in
/// Visual Studio, and a rename an included grammar would have to share is refused with its reason.
/// </remarks>
static class RenameMap
{
	public static PrepareRenameResult Prepare(GramReferences found, int position, LineMap lines)
	{
		var at = found.Positions.FirstOrDefault(start => start <= position && position <= start + found.Name.Length);

		return new PrepareRenameResult(lines.Range(at, found.Name.Length), found.Name);
	}

	public static WorkspaceEdit Edit(GramRename rename, string uri, LineMap lines)
	{
		return new WorkspaceEdit(new Dictionary<string, IReadOnlyList<TextEdit>>(StringComparer.Ordinal)
		{
			[uri] = rename.Edits.Select(edit => new TextEdit(lines.Range(edit.Span.Start, edit.Span.Length), edit.NewText)).ToArray(),
		});
	}
}
