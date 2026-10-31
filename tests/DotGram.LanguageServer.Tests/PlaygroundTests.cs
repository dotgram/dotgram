using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>
/// What the providers answer on the Visual Studio playground grammars, asked through the protocol,
/// is what the extension answered there: the recordings <c>PlaygroundSnapshotTests</c> holds the
/// providers to, written again from the server's answers and compared line for line.
/// </summary>
/// <remarks>
/// <para>
/// The same provider, the same data, two adapters: a difference here that the core's own test does
/// not show is the server's mapping losing or changing something.
/// </para>
/// <para>
/// The grammars are opened unsaved, so they are read as they stand, with no host looked for and
/// nothing kept quiet — as the recording read them. So nothing here covers a host's options, an
/// included grammar, Roslyn's answers in C#, the kinds of completion items or the edit range an item
/// carries; <c>HostTests</c> and <c>ProtocolTests</c> do.
/// </para>
/// <para>
/// What the protocol does not carry is left out of the comparison, on both sides and nowhere else:
/// the span Visual Studio tracks a signature help session by, which a client works out itself; the
/// picture beside a Quick Info, beyond a rule's definition being a block of grammar; Ctrl+click,
/// which a client answers with Go To Definition; and the definition a reference list starts from.
/// Roslyn is not asked, as it was not in the recording: a C# name goes nowhere, and Quick Info in
/// C# is recorded as the question it would have been.
/// </para>
/// </remarks>
public sealed class PlaygroundTests
{
	public static TheoryData<string> Grammars()
	{
		return new TheoryData<string>("VisualStudioToolingPlayground.gram", "ToolingQuery.gram", "RecoveryPlayground.gram");
	}

	[Theory]
	[MemberData(nameof(Grammars))]
	public async Task The_server_answers_as_the_extension_did(string grammar)
	{
		var root     = LspClient.SourceRoot();
		var text     = File.ReadAllText(Path.Combine(root, "tests", "DotGram.VisualStudio.Tests", "Playground", grammar));
		var markers  = File.ReadAllLines(Path.Combine(root, "tests", "DotGram.Language.Tests", "Playground", "CompletionMarkers.txt"))
			.Where(line => line.StartsWith(grammar + "|", StringComparison.Ordinal))
			.Select(line => line.Substring(grammar.Length + 1))
			.ToArray();
		var expected = File.ReadAllText(Path.Combine(
			root, "tests", "DotGram.Language.Tests", "Playground", Path.GetFileNameWithoutExtension(grammar) + ".txt"))
			.Replace("\r\n", "\n");

		var askedRoslyn = false;
		var options     = new ServerOptions
		{
			CSharp          = false,
			CSharpQuickInfo = (_, _, _) =>
			{
				askedRoslyn = true;
				return Task.FromResult<DotGram.Language.RoslynGramQuickInfo?>(null);
			},
		};

		await using var client = await LspClient.StartAsync(options);

		// Unsaved, so that it is read as it stands, as the recording read it.
		var uri = "untitled:" + grammar;

		await client.OpenAsync(uri, text);
		await client.DiagnosticsAsync(uri, 1);

		var document = client.Server.Document(uri)!.Session.Result!.Document;
		var tokens   = document.Classifications.Select(static item => item.Position).Distinct().OrderBy(static at => at).ToArray();
		var result   = new StringBuilder();

		foreach (var marker in markers)
			await CompletionAsync(client, uri, text, Marker(text, marker), result);

		foreach (var position in tokens)
		{
			askedRoslyn = false;

			var hover = await client.RequestAsync("textDocument/hover", LspClient.At(uri, text, position));

			if (askedRoslyn)
			{
				result.Append("quickinfo ").Append(Place(text, position)).Append(" csharp\n");
				continue;
			}

			if (hover.ValueKind == JsonValueKind.Null)
				continue;

			var (start, length) = Offsets(text, hover.GetProperty("range"));

			result.Append("quickinfo ").Append(Place(text, position)).Append(" span=").Append(Span(text, start, length)).Append('\n');
			QuickInfo(hover.GetProperty("contents").GetProperty("value").GetString()!, result);
		}

		for (var position = 0; position < text.Length; position++)
		{
			if (text[position] is not ('(' or ','))
				continue;

			var at   = position + 1;
			var help = await client.RequestAsync("textDocument/signatureHelp", LspClient.At(uri, text, at));

			if (help.ValueKind == JsonValueKind.Null)
				continue;

			var active = help.TryGetProperty("activeParameter", out var parameter) ? parameter.GetInt32() : -1;

			foreach (var signature in help.GetProperty("signatures").EnumerateArray())
			{
				var label      = signature.GetProperty("label").GetString()!;
				var parameters = signature.GetProperty("parameters").EnumerateArray()
					.Select(item => item.GetProperty("label").EnumerateArray().Select(static bound => bound.GetInt32()).ToArray())
					.Select(bounds => (Name: label.Substring(bounds[0], bounds[1] - bounds[0]), Start: bounds[0], Length: bounds[1] - bounds[0]))
					.ToArray();

				result.Append("signature ").Append(Place(text, at))
					.Append(" | ").Append(Escape(label))
					.Append(" | ").Append(Escape(signature.TryGetProperty("documentation", out var documentation) ? documentation.GetProperty("value").GetString() : ""))
					.Append(" | current=").Append(Escape(active >= 0 ? parameters[active].Name : "-"))
					.Append(" | ").Append(string.Join(", ", parameters.Select(static item => $"{Escape(item.Name)}@{item.Start}+{item.Length}")))
					.Append('\n');
			}
		}

		foreach (var position in tokens)
		{
			var definition = await client.RequestAsync("textDocument/definition", LspClient.At(uri, text, position));

			if (definition.ValueKind == JsonValueKind.Array)
				foreach (var location in definition.EnumerateArray())
					result.Append("definition ").Append(Place(text, position)).Append(" -> ")
						.Append(location.GetProperty("uri").GetString() == uri
							? "here " + Place(text, Offsets(text, location.GetProperty("range")).Start)
							: "file " + location.GetProperty("uri").GetString())
						.Append('\n');

			var references = await client.RequestAsync("textDocument/references", new
			{
				textDocument = new { uri },
				position     = LspClient.Position(text, position),
				context      = new { includeDeclaration = true },
			});

			if (references.ValueKind == JsonValueKind.Array)
			{
				var locations = references.EnumerateArray().ToArray();
				var here      = locations.Where(location => location.GetProperty("uri").GetString() == uri)
					.Select(location => Offsets(text, location.GetProperty("range")))
					.ToArray();
				var name      = text.Substring(here[0].Start, here[0].Length);

				result.Append("references ").Append(Place(text, position))
					.Append(" name=").Append(name)
					.Append(" at=[").Append(string.Join(", ", here.Select(at => Place(text, at.Start)))).Append(']')
					.Append(" elsewhere=").Append(locations.Length - here.Length)
					.Append('\n');

				var rename = await client.RequestRawAsync("textDocument/rename", new
				{
					textDocument = new { uri },
					position     = LspClient.Position(text, position),
					newName      = name + "_",
				});

				result.Append("rename ").Append(Place(text, position)).Append(' ')
					.Append(rename.TryGetProperty("error", out _)
						? "refused"
						: "edits=[" + string.Join(", ", rename.GetProperty("result").GetProperty("changes").GetProperty(uri).EnumerateArray()
							.Select(edit => Offsets(text, edit.GetProperty("range")))
							.Select(edit => Span(text, edit.Start, edit.Length))) + "]")
					.Append('\n');
			}

			var highlights = await client.RequestAsync("textDocument/documentHighlight", LspClient.At(uri, text, position));

			if (highlights.ValueKind == JsonValueKind.Array)
				result.Append("highlight ").Append(Place(text, position)).Append(" -> [")
					.Append(string.Join(", ", highlights.EnumerateArray().Select(highlight =>
					{
						var (start, length) = Offsets(text, highlight.GetProperty("range"));

						return Span(text, start, length) + " " +
							(highlight.GetProperty("kind").GetInt32() == Lsp.HighlightWrite ? "DefinitionHighlightTag" : "ReferenceHighlightTag");
					})))
					.Append("]\n");
		}

		var actual   = result.ToString();
		var recorded = Comparable(expected);

		// A server that answered nothing at all would match a recording that has nothing of it.
		foreach (var kind in new[] { "completion", "quickinfo", "definition", "references", "rename", "highlight" })
			Assert.Contains(kind + " ", actual, StringComparison.Ordinal);

		Assert.True(
			recorded == Comparable(actual),
			$"{grammar}: the server no longer answers as the extension did. First difference:\n{FirstDifference(recorded, Comparable(actual))}");
	}

	static async Task CompletionAsync(LspClient client, string uri, string text, int position, StringBuilder result)
	{
		result.Append("completion ").Append(Place(text, position));

		var list = await client.RequestAsync("textDocument/completion", new
		{
			textDocument = new { uri },
			position     = LspClient.Position(text, position),
		});

		if (list.ValueKind == JsonValueKind.Null)
		{
			result.Append(" none\n");
			return;
		}

		var (start, length) = Offsets(text, list.GetProperty("itemDefaults").GetProperty("editRange"));

		result.Append(" applicable=").Append(Span(text, start, length)).Append('\n');

		foreach (var item in list.GetProperty("items").EnumerateArray())
		{
			var resolved = await client.RequestAsync("completionItem/resolve", item);
			var label    = item.GetProperty("label").GetString()!;

			result.Append("  ").Append(Escape(label))
				.Append(" | insert=").Append(Escape(Text(item, "textEditText") ?? label))
				.Append(" | suffix=").Append(Escape(item.TryGetProperty("labelDetails", out var details) ? Text(details, "detail") : ""))
				.Append(" | sort=").Append(Escape(Text(item, "sortText") ?? label))
				.Append(" | filter=").Append(Escape(Text(item, "filterText") ?? label))
				.Append(" | ").Append(Escape(resolved.GetProperty("documentation").GetProperty("value").GetString())).Append('\n');
		}
	}

	/// <summary>The panel's elements read back out of the Markdown the server made of them.</summary>
	static void QuickInfo(string markdown, StringBuilder result)
	{
		var blocks  = Blocks(markdown);
		var divider = blocks.FindIndex(static block => block is { Fenced: false, Text: "---" });

		for (var index = 0; index < divider; index++)
			result.Append("  text ").Append(Escape(Unescape(blocks[index].Text))).Append('\n');

		var item = blocks.Skip(divider + 1).ToList();

		if (item.Count == 0)
			return;

		result.Append("  icon ").Append(item[0].Fenced ? "Method" : "Other").Append('\n');
		result.Append("  text ").Append(Escape(item[0].Fenced ? item[0].Text : Unescape(item[0].Text))).Append('\n');

		if (item.Count > 2 && item[1].Text == "**" + HoverMap.ReferencedRules + "**")
		{
			result.Append("  text Show referenced rules\n");
			result.Append("  hidden ").Append(Escape(item[2].Text)).Append('\n');
		}
	}

	/// <summary>Paragraphs and fenced blocks, as this server writes them.</summary>
	static List<(bool Fenced, string Text)> Blocks(string markdown)
	{
		var result = new List<(bool Fenced, string Text)>();
		var at     = 0;

		while (at < markdown.Length)
		{
			if (markdown[at] == '`')
			{
				var fence = 0;

				while (markdown[at + fence] == '`')
					fence++;

				var start = markdown.IndexOf('\n', at) + 1;
				var end   = markdown.IndexOf("\n" + new string('`', fence), start - 1, StringComparison.Ordinal);

				result.Add((true, markdown.Substring(start, Math.Max(0, end - start))));
				at = end + 1 + fence;
			}
			else
			{
				var end = markdown.IndexOf("\n\n", at, StringComparison.Ordinal);

				if (end < 0)
					end = markdown.Length;

				result.Add((false, markdown.Substring(at, end - at)));
				at = end;
			}

			while (at < markdown.Length && markdown[at] == '\n')
				at++;
		}

		return result;
	}

	static string Unescape(string text)
	{
		var result = new StringBuilder(text.Length);

		for (var index = 0; index < text.Length; index++)
		{
			if (text[index] == '\\' && index + 1 < text.Length && HoverMap.Markup.IndexOf(text[index + 1]) >= 0)
				index++;
			else if (text[index] == ' ' && text.AsSpan(index).StartsWith("  \n"))
			{
				index++;
				continue;
			}

			result.Append(text[index]);
		}

		return result.ToString();
	}

	/// <summary>
	/// A recording with what the protocol does not carry taken out: the tracking span of signature
	/// help, the kind of picture beside a Quick Info other than a rule's, Ctrl+click, a definition
	/// that is C# or a generated method (Roslyn's to find, and not asked here), and the definition a
	/// reference list was found from.
	/// </summary>
	static string Comparable(string recording)
	{
		var result = new StringBuilder();

		foreach (var line in recording.Split('\n'))
		{
			if (line.StartsWith("navigable ", StringComparison.Ordinal))
				continue;

			if (line.StartsWith("definition ", StringComparison.Ordinal) &&
				(line.Contains(" -> csharp ", StringComparison.Ordinal) || line.Contains(" -> published ", StringComparison.Ordinal)))
				continue;

			var kept = line;

			if (kept.StartsWith("signature ", StringComparison.Ordinal) && kept.IndexOf(" applicable=", StringComparison.Ordinal) is var applicable and >= 0)
				kept = kept.Substring(0, applicable) + kept.Substring(kept.IndexOf(" | ", applicable, StringComparison.Ordinal));

			if (kept.StartsWith("references ", StringComparison.Ordinal) && kept.IndexOf(" definition=", StringComparison.Ordinal) is var definition and >= 0)
				kept = kept.Substring(0, definition) + kept.Substring(kept.IndexOf(" at=[", definition, StringComparison.Ordinal));

			if (kept is "  icon LocalVariable" or "  icon IntellisenseKeyword")
				kept = "  icon Other";

			result.Append(kept).Append('\n');
		}

		return result.ToString();
	}

	static string? Text(JsonElement element, string name)
	{
		return element.TryGetProperty(name, out var value) ? value.GetString() : null;
	}

	static (int Start, int Length) Offsets(string text, JsonElement range)
	{
		var lines = new LineMap(text);
		var start = lines.Offset(new Position(range.GetProperty("start").GetProperty("line").GetInt32(), range.GetProperty("start").GetProperty("character").GetInt32()));
		var end   = lines.Offset(new Position(range.GetProperty("end").GetProperty("line").GetInt32(), range.GetProperty("end").GetProperty("character").GetInt32()));

		return (start, end - start);
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
}
