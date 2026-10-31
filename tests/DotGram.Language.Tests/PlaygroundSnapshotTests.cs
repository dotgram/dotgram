using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// What the feature providers answer on the Visual Studio playground grammars — completion at marked
/// places, Quick Info, definition, Ctrl+click, references, rename and highlights at every token,
/// signature help after every parenthesis and comma — equals what the extension answered there before
/// those answers were computed outside it.
/// </summary>
/// <remarks>
/// <para>
/// The baselines in <c>Playground/</c> were written on Windows by a test that drove the extension's own
/// completion source, Quick Info builder, command handlers and taggers over the same grammars, and
/// read the text of the panels it made. They are not regenerated here: a difference is a change in
/// what the editor shows, and is either a regression or a change to make on purpose, together with a
/// new recording.
/// </para>
/// <para>
/// Nothing reaches Roslyn, as nothing did there: completion in C# offers DotGram's own names only,
/// and Quick Info in C# is recorded as a question for Roslyn rather than answered.
/// </para>
/// </remarks>
public sealed class PlaygroundSnapshotTests
{
	public static TheoryData<string> Grammars()
	{
		return new TheoryData<string>("VisualStudioToolingPlayground.gram", "ToolingQuery.gram", "RecoveryPlayground.gram");
	}

	[Theory]
	[MemberData(nameof(Grammars))]
	public async Task The_providers_answer_as_the_extension_did(string grammar)
	{
		var root     = SourceRoot();
		var text     = File.ReadAllText(Path.Combine(root, "tests", "DotGram.VisualStudio.Tests", "Playground", grammar));
		var markers  = File.ReadAllLines(Path.Combine(root, "tests", "DotGram.Language.Tests", "Playground", "CompletionMarkers.txt"))
			.Where(line => line.StartsWith(grammar + "|", StringComparison.Ordinal))
			.Select(line => line.Substring(grammar.Length + 1))
			.ToArray();

		// The recording's own line breaks are LF; a checkout may have made them CRLF. A carriage
		// return inside a recorded value is written as \r and survives either way.
		var expected = File.ReadAllText(Path.Combine(
			root, "tests", "DotGram.Language.Tests", "Playground", Path.GetFileNameWithoutExtension(grammar) + ".txt"))
			.Replace("\r\n", "\n");

		var actual = await RecordAsync(text, markers);

		Assert.True(
			expected == actual,
			$"{grammar}: the providers no longer answer as the extension did. First difference:\n{FirstDifference(expected, actual)}");
	}

	static async Task<string> RecordAsync(string text, IReadOnlyList<string> completionMarkers)
	{
		var document = GramAnalysisSession.Analyze(text, null, suppressContextDiagnostics: false).Document;
		var view     = new GramStandaloneView(text, document);
		var result   = new StringBuilder();
		var tokens   = document.Classifications.Select(static item => item.Position).Distinct().OrderBy(static at => at).ToArray();

		// Completion, at the marked places.
		var completion = new StandaloneGramCompletionProvider(document);

		foreach (var marker in completionMarkers)
		{
			var position = Marker(text, marker);

			result.Append("completion ").Append(Place(text, position));

			if (completion.Start(text, position) is not { } applicable)
			{
				result.Append(" none\n");
				continue;
			}

			result.Append(" applicable=").Append(Span(text, applicable.Start, applicable.Length)).Append('\n');

			foreach (var item in await completion.ItemsAsync(text, position, applicable, null, CancellationToken.None))
				result.Append("  ").Append(Escape(item.Label))
					.Append(" | insert=").Append(Escape(item.InsertText ?? item.Label))
					.Append(" | suffix=").Append(Escape(item.Suffix))
					.Append(" | sort=").Append(Escape(item.SortText ?? item.Label))
					.Append(" | filter=").Append(Escape(item.FilterText ?? item.Label))
					.Append(" | ").Append(Escape(item.Description)).Append('\n');
		}

		// Quick Info, at the start of every classified token.
		foreach (var position in tokens)
		{
			var askedRoslyn = false;
			var answer = await GramQuickInfoProvider.StandaloneAsync(
				view,
				position,
				(_, _, _) =>
				{
					askedRoslyn = true;
					return Task.FromResult<RoslynGramQuickInfo?>(null);
				},
				CancellationToken.None);

			if (askedRoslyn)
			{
				result.Append("quickinfo ").Append(Place(text, position)).Append(" csharp\n");
				continue;
			}

			if (answer.Item is not { } item)
				continue;

			result.Append("quickinfo ").Append(Place(text, position)).Append(" span=").Append(Span(text, item.Span.Start, item.Span.Length)).Append('\n');
			QuickInfo(item, answer.Diagnostics, result);
		}

		// Signature help, after every opening parenthesis and comma.
		var signatures = new StandaloneGramSignatureHelpProvider(document);

		for (var position = 0; position < text.Length; position++)
		{
			if (text[position] is not ('(' or ','))
				continue;

			var at = position + 1;

			if (signatures.At(text, at) is not { } help)
				continue;

			foreach (var signature in help.Signatures)
				result.Append("signature ").Append(Place(text, at))
					.Append(" applicable=").Append(Span(text, help.ApplicableSpan.Start, help.ApplicableSpan.Length))
					.Append(" | ").Append(Escape(signature.Label))
					.Append(" | ").Append(Escape(signature.Documentation))
					.Append(" | current=").Append(Escape(help.ActiveParameter >= 0 ? signature.Parameters[help.ActiveParameter].Name : "-"))
					.Append(" | ").Append(string.Join(", ", signature.Parameters.Select(static parameter =>
						$"{Escape(parameter.Name)}@{parameter.Locus.Start}+{parameter.Locus.Length}")))
					.Append('\n');
		}

		// Definition, Ctrl+click, references, rename and highlights, at the start of every classified token.
		foreach (var position in tokens)
		{
			if (await GramNavigationProvider.DefinitionAsync(view, position, null, CancellationToken.None) is { } definition)
				result.Append("definition ").Append(Place(text, position)).Append(" -> ").Append(Definition(text, definition)).Append('\n');

			if (GramNavigationProvider.Navigable(view, position) is { } navigable)
				result.Append("navigable ").Append(Place(text, position)).Append(" span=")
					.Append(Span(text, navigable.Span.Start, navigable.Span.Length)).Append(" -> ")
					.Append(navigable.Target switch
					{
						GramLocation.InDocument target       => Place(text, target.Position),
						GramLocation.InIncludedGrammar       => "IncludedGrammarNavigableSymbol",
						GramLocation.InFile                  => "FileNavigableSymbol",
						_                                    => "NonNavigableSymbol",
					})
					.Append('\n');

			if (GramNavigationProvider.References(view, position) is { } found)
			{
				result.Append("references ").Append(Place(text, position))
					.Append(" name=").Append(found.Name)
					.Append(" definition=").Append(found.DefinitionPosition >= text.Length ? "elsewhere" : Place(text, found.DefinitionPosition))
					.Append(" at=[").Append(string.Join(", ", found.Positions.Select(at => Place(text, at)))).Append(']')
					.Append(" elsewhere=").Append(found.Elsewhere.Count)
					.Append('\n');

				var rename = GramRenameProvider.Rename(found, found.Name + "_");

				result.Append("rename ").Append(Place(text, position)).Append(' ')
					.Append(rename.Conflicts.Count > 0
						? "refused"
						: "edits=[" + string.Join(", ", rename.Edits.Select(edit => Span(text, edit.Span.Start, edit.Span.Length))) + "]")
					.Append('\n');
			}

			var highlights = GramNavigationProvider.Highlights(view, position);

			if (highlights.Count > 0)
				result.Append("highlight ").Append(Place(text, position)).Append(" -> [")
					.Append(string.Join(", ", highlights.Select(highlight =>
						Span(text, highlight.Span.Start, highlight.Span.Length) + " " +
						(highlight.IsDefinition ? "DefinitionHighlightTag" : "ReferenceHighlightTag"))))
					.Append("]\n");
		}

		return result.ToString();
	}

	static string Definition(string text, GramLocation definition)
	{
		return definition switch
		{
			GramLocation.PublishedMethod target   => "published " + target.MethodName,
			GramLocation.InStandardLibrary target => $"std {target.Rule} {target.Line}:{target.Column}",
			GramLocation.HostSymbol target        => $"csharp {Escape(target.Expression)} @{target.Position}",
			GramLocation.InIncludedGrammar target => $"included {target.Grammar.Name} {target.Line}:{target.Column}",
			GramLocation.InDocument target        => "here " + Place(text, target.Position),
			_                                     => definition.ToString(),
		};
	}

	/// <summary>
	/// The text of the panel the extension makes of an item, element by element, as the recording read
	/// it: the squiggles' messages in front, then the picture, the summary, and the referenced rules
	/// behind a link, hidden.
	/// </summary>
	static void QuickInfo(GramQuickInfo item, IReadOnlyList<string> diagnostics, StringBuilder result)
	{
		foreach (var diagnostic in diagnostics)
			result.Append("  text ").Append(Escape(diagnostic)).Append('\n');

		switch (item.Kind)
		{
			case GramQuickInfoKind.Grammar:
				result.Append("  icon ").Append(item.Icon switch
				{
					GramQuickInfoIcon.Method        => "Method",
					GramQuickInfoIcon.LocalVariable => "LocalVariable",
					_                               => "IntellisenseKeyword",
				}).Append('\n');
				result.Append("  text ").Append(Escape(Text(item.Sections[0]))).Append('\n');

				if (item.Details.Count > 0)
				{
					result.Append("  text Show referenced rules\n");
					result.Append("  hidden ").Append(Escape(string.Join("\n", item.Details.Select(Text)))).Append('\n');
				}

				break;

			case GramQuickInfoKind.Suppressed when diagnostics.Count == 0:
				result.Append("  suppressed\n");
				break;

			default:
				foreach (var line in item.Lines)
					result.Append("  text ").Append(Escape(line)).Append('\n');
				break;
		}
	}

	static string Text(IReadOnlyList<GramQuickInfoRun> runs)
	{
		return string.Concat(runs.Select(static run => run.Text));
	}

	static string FirstDifference(string expected, string actual)
	{
		var left  = expected.Split('\n');
		var right = actual.Split('\n');

		for (var index = 0; index < Math.Max(left.Length, right.Length); index++)
		{
			var one   = index < left.Length ? left[index] : "<end>";
			var other = index < right.Length ? right[index] : "<end>";

			if (one != other)
				return $"line {index + 1}\n  recorded: {one}\n  now:      {other}";
		}

		return "none";
	}

	static int Marker(string text, string marker)
	{
		var caret  = marker.IndexOf('^');
		var needle = marker.Remove(caret, 1);
		var at     = text.IndexOf(needle, StringComparison.Ordinal);

		Assert.True(at >= 0, "Marker not found: " + marker);

		return at + caret;
	}

	static string Place(string text, int position)
	{
		var line   = 1;
		var column = 1;

		for (var at = 0; at < position && at < text.Length; at++)
		{
			if (text[at] == '\n')
			{
				line++;
				column = 1;
			}
			else
				column++;
		}

		return line + ":" + column;
	}

	static string Span(string text, int position, int length)
	{
		return Place(text, position) + "+" + length;
	}

	static string Escape(string? value)
	{
		return value is null
			? "<null>"
			: value.Replace("\\", "\\\\").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t");
	}

	static string SourceRoot()
	{
		var declared = typeof(PlaygroundSnapshotTests).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
			.FirstOrDefault(static one => one.Key == "DotGramSourceRoot")?.Value;

		if (declared is { Length: > 0 } && File.Exists(Path.Combine(declared, "DotGram.slnx")))
			return Path.GetFullPath(declared);

		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? throw new InvalidOperationException("The repository root is neither declared by the build nor above the test binaries.");
	}
}
