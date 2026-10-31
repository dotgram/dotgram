using DotGram.Language;

using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Text;

namespace DotGram.VisualStudio;

/// <summary>The symbol at a position of a buffer, by the shared provider, whichever kind of buffer it is.</summary>
static class GramFindReferencesTarget
{
	public static GramReferences? At(
		ITextBuffer buffer,
		ITextSnapshot snapshot,
		int position,
		VisualStudioWorkspace workspace,
		ITextDocumentFactoryService documents)
	{
		return buffer.ContentType.IsOfType(GramContentType.Name)
			? GramNavigationProvider.References(GramBufferAnalysis.For(buffer).View(snapshot), position)
			: GramNavigationProvider.References(EmbeddedGrammarBufferAnalysis.For(buffer, workspace, documents).View(snapshot), position);
	}
}
