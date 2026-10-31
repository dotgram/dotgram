using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Language;

namespace DotGram.LanguageServer;

/// <summary>A Quick Info as <c>textDocument/hover</c> carries it: Markdown.</summary>
/// <remarks>
/// The squiggles' messages first, as Visual Studio puts them; a rule's definition as a block of
/// grammar, which the client colours with the extension's TextMate grammar; the referenced rules,
/// which Visual Studio hides behind a link, after it under a heading; what Roslyn says about C# as a
/// block of C# and paragraphs. The runs' own colours are the panel's, and Markdown has none.
/// </remarks>
static class HoverMap
{
	public const string ReferencedRules = "Referenced rules";

	public static Hover? Map(GramQuickInfoAnswer answer, int position, LineMap lines)
	{
		var markdown = new StringBuilder();

		foreach (var diagnostic in answer.Diagnostics)
			Paragraph(markdown, Escape(diagnostic));

		if (answer.Item is not { } item)
			return markdown.Length == 0 ? null : new Hover(MarkupContent.Markdown(markdown.ToString()), lines.Range(position, 0));

		if (answer.Diagnostics.Count > 0 && item.Kind != GramQuickInfoKind.Suppressed)
			Paragraph(markdown, "---");

		switch (item.Kind)
		{
			case GramQuickInfoKind.Grammar:
				var summary = Text(item.Sections[0]);

				if (item.Icon == GramQuickInfoIcon.Method)
					Fenced(markdown, "gram", summary);
				else
					Paragraph(markdown, Escape(summary));

				if (item.Details.Count > 0)
				{
					Paragraph(markdown, "**" + ReferencedRules + "**");
					Fenced(markdown, "gram", string.Join("\n", item.Details.Select(Text)));
				}

				break;

			case GramQuickInfoKind.CSharp:
				for (var index = 0; index < item.Sections.Count; index++)
				{
					var section = Text(item.Sections[index]);

					if (section.Length == 0)
						continue;

					if (index == 0)
						Fenced(markdown, "csharp", section);
					else
						Paragraph(markdown, Escape(section));
				}

				break;

			case GramQuickInfoKind.Suppressed:
				break;

			default:
				foreach (var line in item.Lines)
					Paragraph(markdown, Escape(line));

				break;
		}

		return markdown.Length == 0
			? null
			: new Hover(MarkupContent.Markdown(markdown.ToString()), lines.Range(item.Span.Start, item.Span.Length));
	}

	static string Text(IReadOnlyList<GramQuickInfoRun> runs)
	{
		return string.Concat(runs.Select(static run => run.Text));
	}

	static void Paragraph(StringBuilder markdown, string text)
	{
		if (markdown.Length > 0)
			markdown.Append("\n\n");

		markdown.Append(text);
	}

	/// <summary>A fenced block, its fence longer than any run of backticks in the text.</summary>
	static void Fenced(StringBuilder markdown, string language, string text)
	{
		var longest = 0;
		var run     = 0;

		foreach (var character in text)
		{
			run     = character == '`' ? run + 1 : 0;
			longest = Math.Max(longest, run);
		}

		var fence = new string('`', Math.Max(3, longest + 1));

		Paragraph(markdown, fence + language + "\n" + text + "\n" + fence);
	}

	/// <summary>Text that Markdown shows as it is: every character it could read as markup behind a backslash, and line breaks kept.</summary>
	public static string Escape(string text)
	{
		var result = new StringBuilder(text.Length + 16);

		foreach (var character in text.Replace("\r\n", "\n"))
		{
			if (character == '\n')
			{
				result.Append("  \n");
				continue;
			}

			if (Markup.IndexOf(character) >= 0)
				result.Append('\\');

			result.Append(character);
		}

		return result.ToString();
	}

	public const string Markup = "\\`*_{}[]<>()#+-.!|~";
}
