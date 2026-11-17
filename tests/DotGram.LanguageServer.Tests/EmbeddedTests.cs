using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>
/// The grammars written into a C# document — a <c>[Gram]</c>'s string, raw, verbatim or plain, however the
/// attribute is spelled, and a string passed to a <c>StringSyntax("DotGram")</c> parameter — analysed as
/// Visual Studio analyses them: the diagnostics published on the C# document, the colours told to the
/// client to paint over the C# extension's, and hover, definition, references and rename inside them.
/// </summary>
public sealed class EmbeddedTests : IDisposable
{
	readonly string _folder = Path.Combine(Path.GetTempPath(), "DotGram.LanguageServer.Tests", Guid.NewGuid().ToString("N"));

	public EmbeddedTests()
	{
		Directory.CreateDirectory(_folder);
	}

	public void Dispose()
	{
		Directory.Delete(_folder, recursive: true);
	}

	const string Raw = """"
		using DotGram;

		namespace Parsers;

		[Gram("""
			Start  = Word | Number & Missing
			Word   = ['a'..'z']+
			Number = ['0'..'9']+
			parse Start
			""")]
		public partial class Raw;
		"""";

	const string Verbatim = """
		using DotGram;

		namespace Parsers;

		[Gram(@"Start = ""x"" & Word & Missing
		Word = ['a'..'z']+
		parse Start
		")]
		public partial class Verbatim;
		""";

	const string Aliased = """
		using G = DotGram.GramAttribute;

		namespace Parsers;

		[G("Start = 'a' & Missing\nparse Start\n")]
		public partial class Aliased;
		""";

	const string Annotated = """
		using System.Diagnostics.CodeAnalysis;

		namespace Parsers;

		static class Checks
		{
			static void Inspect([StringSyntax("DotGram")] string grammar)
			{
			}

			static void Use()
			{
				Inspect("Start = 'a' & Missing\nparse Start\n");
			}
		}
		""";

	[Theory]
	[InlineData(nameof(Raw))]
	[InlineData(nameof(Verbatim))]
	[InlineData(nameof(Aliased))]
	[InlineData(nameof(Annotated))]
	public async Task A_grammar_in_a_string_has_its_diagnostics_on_the_csharp_document(string form)
	{
		var text = form switch
		{
			nameof(Raw)      => Raw,
			nameof(Verbatim) => Verbatim,
			nameof(Aliased)  => Aliased,
			_                => Annotated,
		};

		var (client, uri) = await OpenAsync(form + ".cs", text);

		await using (client)
		{
			var published = await client.WaitForAsync("textDocument/publishDiagnostics", parameters =>
				parameters.GetProperty("uri").GetString() == uri && parameters.GetProperty("diagnostics").GetArrayLength() > 0);
			var missing = text.IndexOf("Missing", StringComparison.Ordinal);

			// Where the name is in the C# text, escapes and indentation of the string accounted for.
			var diagnostic = published.GetProperty("diagnostics").EnumerateArray()
				.Single(item => item.GetProperty("message").GetString()!.Contains("Missing", StringComparison.Ordinal));

			Assert.StartsWith("GRAM", diagnostic.GetProperty("code").GetString(), StringComparison.Ordinal);
			Assert.Equal("dotgram", diagnostic.GetProperty("source").GetString());
			Assert.Equal(Json(LspClient.Position(text, missing)), Json(diagnostic.GetProperty("range").GetProperty("start")));
			Assert.Equal(Json(LspClient.Position(text, missing + "Missing".Length)), Json(diagnostic.GetProperty("range").GetProperty("end")));

			// And the colours, over the grammar's own words only.
			var told   = await ColorsAsync(client, uri, 1);
			var colors = Spans(told);
			var start  = text.IndexOf("Start", StringComparison.Ordinal);

			Assert.Equal(client.Server.CSharpDocument(uri)!.Session, told.GetProperty("session").GetInt64());

			Assert.Contains((Line(text, start), Column(text, start), "variable"), colors.Select(static span => (span.Line, span.Character, span.Kind)));
			Assert.Contains("keyword", colors.Select(static span => span.Kind));
			Assert.DoesNotContain(colors, span => span.Line < Line(text, text.IndexOf("namespace", StringComparison.Ordinal)));
		}
	}

	[Fact]
	public async Task The_operators_of_a_grammar_are_told_as_special_symbols()
	{
		var (client, uri) = await OpenAsync("Raw.cs", Raw);

		await using (client)
		{
			var colors = Spans(await ColorsAsync(client, uri, 1));

			foreach (var symbol in new[] { "|", "&", "+", ".." })
			{
				var at = Raw.IndexOf(symbol, Raw.IndexOf("Start  =", StringComparison.Ordinal), StringComparison.Ordinal);

				Assert.Contains((Line(Raw, at), Column(Raw, at), "gramSpecialSymbol"), colors.Select(static span => (span.Line, span.Character, span.Kind)));
			}
		}
	}

	[Fact]
	public async Task Hover_definition_references_and_rename_answer_inside_a_grammar_and_nowhere_else()
	{
		var (client, uri) = await OpenAsync("Raw.cs", Raw);

		await using (client)
		{
			await ColorsWithSymbolsAsync(client, uri);

			var definition = Raw.IndexOf("Word   =", StringComparison.Ordinal);
			var use        = Raw.IndexOf("Word |", StringComparison.Ordinal);

			var hover = await client.RequestAsync("textDocument/hover", LspClient.At(uri, Raw, use + 1));

			Assert.Contains("Word", hover.GetProperty("contents").GetProperty("value").GetString(), StringComparison.Ordinal);

			var target = (await client.RequestAsync("textDocument/definition", LspClient.At(uri, Raw, use + 1))).EnumerateArray().Single();

			Assert.Equal(uri, target.GetProperty("uri").GetString());
			Assert.Equal(Json(LspClient.Position(Raw, definition)), Json(target.GetProperty("range").GetProperty("start")));
			Assert.Equal(Json(LspClient.Position(Raw, definition + "Word".Length)), Json(target.GetProperty("range").GetProperty("end")));

			var references = await client.RequestAsync("textDocument/references", new
			{
				textDocument = new { uri },
				position     = LspClient.Position(Raw, use + 1),
				context      = new { includeDeclaration = true },
			});

			Assert.Equal(2, references.GetArrayLength());

			var highlights = await client.RequestAsync("textDocument/documentHighlight", LspClient.At(uri, Raw, use + 1));

			Assert.Equal(2, highlights.GetArrayLength());

			var rename = await client.RequestAsync("textDocument/rename", new
			{
				textDocument = new { uri },
				position     = LspClient.Position(Raw, use + 1),
				newName      = "Letters",
			});

			Assert.Equal(2, rename.GetProperty("changes").GetProperty(uri).GetArrayLength());

			// Outside the grammar every question is the C# extension's.
			var csharp = Raw.IndexOf("partial", StringComparison.Ordinal);

			Assert.Equal(JsonValueKind.Null, (await client.RequestAsync("textDocument/hover", LspClient.At(uri, Raw, csharp))).ValueKind);
			Assert.Equal(JsonValueKind.Null, (await client.RequestAsync("textDocument/definition", LspClient.At(uri, Raw, csharp))).ValueKind);
			Assert.Equal(JsonValueKind.Null, (await client.RequestAsync("textDocument/references", new
			{
				textDocument = new { uri },
				position     = LspClient.Position(Raw, csharp),
				context      = new { includeDeclaration = true },
			})).ValueKind);
		}
	}

	[Fact]
	public async Task An_edit_moves_the_colours_and_the_diagnostics_with_the_text()
	{
		var (client, uri) = await OpenAsync("Raw.cs", Raw);

		await using (client)
		{
			await ColorsAsync(client, uri, 1);

			await client.NotifyAsync("textDocument/didChange", new
			{
				textDocument   = new { uri, version = 2 },
				contentChanges = new[] { new { range = new { start = new { line = 0, character = 0 }, end = new { line = 0, character = 0 } }, text = "// moved\n" } },
			});

			var edited  = "// moved\n" + Raw;
			var bar     = edited.IndexOf('|');
			var missing = edited.IndexOf("Missing", StringComparison.Ordinal);

			// At once, carried over, and again once analysed.
			Assert.Contains((Line(edited, bar), Column(edited, bar), "gramSpecialSymbol"), Spans(await ColorsAsync(client, uri, 2)).Select(static span => (span.Line, span.Character, span.Kind)));

			var diagnostics = await client.DiagnosticsAsync(uri, 2);

			Assert.Equal(
				Json(LspClient.Position(edited, missing)),
				Json(diagnostics.GetProperty("diagnostics").EnumerateArray().Single().GetProperty("range").GetProperty("start")));
		}
	}

	/// <summary>
	/// A grammar that includes one of another project, written there in a file: read as the build reads
	/// it, so that the included grammar's rule is known and only the rule it lacks is reported.
	/// </summary>
	[Fact]
	public async Task A_grammar_includes_one_of_a_referenced_project()
	{
		const string Project = "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n  <ItemGroup>\n    <AdditionalFiles Include=\"**/*.gram\" />\n  </ItemGroup>\n</Project>\n";

		var library = Directory.CreateDirectory(Path.Combine(_folder, "Library")).FullName;
		var app     = Directory.CreateDirectory(Path.Combine(_folder, "App")).FullName;
		var hosts   = """
			using DotGram;

			namespace App;

			[Gram("Start = B.Word & B.Nope\nparse Start\n")]
			[GramInclude(typeof(Library.Base))]
			public partial class Dialect;
			""";

		File.WriteAllText(Path.Combine(library, "Library.csproj"), Project);
		File.WriteAllText(Path.Combine(library, "Base.cs"), "using DotGram;\n\nnamespace Library;\n\n[Gram(\"Base.gram\", DefaultAlias = \"B\")]\npublic partial class Base;\n");
		File.WriteAllText(Path.Combine(library, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(Path.Combine(app, "App.csproj"), Project.Replace("</Project>", "  <ItemGroup>\n    <ProjectReference Include=\"..\\Library\\Library.csproj\" />\n  </ItemGroup>\n</Project>", StringComparison.Ordinal));
		File.WriteAllText(Path.Combine(app, "Hosts.cs"), hosts);

		var uri = LspClient.Uri(Path.Combine(app, "Hosts.cs"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenCSharpAsync(uri, hosts);

		var published = await client.WaitForAsync("textDocument/publishDiagnostics", parameters =>
			parameters.GetProperty("uri").GetString() == uri && parameters.GetProperty("diagnostics").GetArrayLength() > 0);
		var messages = published.GetProperty("diagnostics").EnumerateArray().Select(static item => item.GetProperty("message").GetString()!).ToArray();

		Assert.Contains(messages, static message => message.Contains("Nope", StringComparison.Ordinal));
		Assert.DoesNotContain(messages, static message => message.Contains("Word", StringComparison.Ordinal));
	}

	[Fact]
	public async Task A_csharp_document_gets_no_semantic_tokens_from_this_server()
	{
		var path = Path.Combine(_folder, "Raw.cs");
		var uri  = LspClient.Uri(path);

		File.WriteAllText(path, Raw);

		await using var client = new LspClient();

		var initialized = await client.InitializeAsync(_folder);
		var selector    = initialized.GetProperty("capabilities").GetProperty("semanticTokensProvider").GetProperty("documentSelector");

		Assert.Equal("gram", selector.EnumerateArray().Single().GetProperty("language").GetString());

		await client.OpenCSharpAsync(uri, Raw);
		await ColorsAsync(client, uri, 1);

		var tokens = await client.RequestAsync("textDocument/semanticTokens/full", new { textDocument = new { uri } });

		Assert.Equal(JsonValueKind.Null, tokens.ValueKind);
	}

	[Fact]
	public async Task Closing_a_csharp_document_takes_its_diagnostics_away()
	{
		var (client, uri) = await OpenAsync("Raw.cs", Raw);

		await using (client)
		{
			await client.DiagnosticsAsync(uri, 1);
			await client.NotifyAsync("textDocument/didClose", new { textDocument = new { uri } });
			await client.WaitForAsync("textDocument/publishDiagnostics", parameters =>
				parameters.GetProperty("uri").GetString() == uri && parameters.GetProperty("diagnostics").GetArrayLength() == 0);

			Assert.Null(client.Server.CSharpDocument(uri));
		}
	}

	/// <summary>
	/// The session reads the document while it holds its own lock — carrying spans over an edit, keeping
	/// what a string that stopped being a grammar showed — and the server reads the session while a request
	/// holds the document: edits that break the attribute and mend it again, with readers all the while,
	/// finish.
	/// </summary>
	[Fact]
	public async Task Reading_while_a_grammar_is_broken_and_mended_does_not_deadlock()
	{
		var (client, uri) = await OpenAsync("Raw.cs", Raw);

		await using (client)
		{
			await client.DiagnosticsAsync(uri, 1);

			var document = client.Server.CSharpDocument(uri)!;
			var at       = new LineMap(Raw).Position(Raw.IndexOf("[Gram(", StringComparison.Ordinal) + "[Gram(".Length);
			var stop     = DateTime.UtcNow + TimeSpan.FromSeconds(6);
			var readers  = Enumerable.Range(0, 4).Select(_ => Task.Run(() =>
			{
				while (DateTime.UtcNow < stop)
					document.View(document.Current);
			}, TestContext.Current.CancellationToken));
			var writer = Task.Run(async () =>
			{
				var version = 1;

				while (DateTime.UtcNow < stop)
				{
					// Broken long enough for its analysis to keep what the grammar showed; mended at once.
					document.Edit(++version, [new TextDocumentContentChangeEvent(",", new Range(at, at))]);
					await Task.Delay(1200, TestContext.Current.CancellationToken);
					document.Edit(++version, [new TextDocumentContentChangeEvent("", new Range(at, new Position(at.Line, at.Character + 1)))]);

					for (var index = 0; index < 50; index++)
					{
						document.Edit(++version, [new TextDocumentContentChangeEvent(" ", new Range(at, at))]);
						document.Edit(++version, [new TextDocumentContentChangeEvent("", new Range(at, new Position(at.Line, at.Character + 1)))]);
					}
				}
			}, TestContext.Current.CancellationToken);
			var all = Task.WhenAll([.. readers, writer]);

			Assert.Same(all, await Task.WhenAny(all, Task.Delay(TimeSpan.FromSeconds(60), TestContext.Current.CancellationToken)));
		}
	}

	/// <summary>
	/// Rename, references and prepare-rename give the spans the name is spelled at in the C# text, which
	/// are not the name's length where an escape spells it or an escape before it moves it.
	/// </summary>
	[Theory]
	[InlineData("Escaped", "[Gram(\"Start = \\u0057ord & 'x'\\nWord = ['a'..'z']+\\nparse Start\\n\")]", "\\u0057ord")]
	[InlineData("Verbatim", "[Gram(@\"Start = \"\"x\"\" & Word\nWord = ['a'..'z']+\nparse Start\n\")]", "Word")]
	[InlineData("Raw", "[Gram(\"\"\"\n\tStart = \"x\" & Word\n\tWord = ['a'..'z']+\n\tparse Start\n\t\"\"\")]", "Word")]
	public async Task Rename_and_references_follow_the_name_as_it_is_spelled(string form, string attribute, string use)
	{
		var text = "using DotGram;\n\nnamespace Parsers;\n\n" + attribute + "\npublic partial class " + form + ";\n";

		var (client, uri) = await OpenAsync(form + ".cs", text);

		await using (client)
		{
			await ColorsAsync(client, uri, 1);
			await client.SettleAsync();

			var definition = text.IndexOf("Word = ", StringComparison.Ordinal);
			var spelled    = text.IndexOf(use, text.IndexOf("Start", StringComparison.Ordinal), StringComparison.Ordinal);

			var prepared = await client.RequestAsync("textDocument/prepareRename", LspClient.At(uri, text, spelled + 1));

			Assert.Equal(use, Slice(text, prepared.GetProperty("range")));

			var references = await client.RequestAsync("textDocument/references", new
			{
				textDocument = new { uri },
				position     = LspClient.Position(text, definition),
				context      = new { includeDeclaration = true },
			});

			Assert.Equal(
				new[] { use, "Word" }.Order(StringComparer.Ordinal),
				references.EnumerateArray().Select(item => Slice(text, item.GetProperty("range"))).Order(StringComparer.Ordinal));

			var rename = await client.RequestAsync("textDocument/rename", new
			{
				textDocument = new { uri },
				position     = LspClient.Position(text, definition),
				newName      = "Letters",
			});

			var edits = rename.GetProperty("changes").GetProperty(uri).EnumerateArray()
				.Select(edit => (Start: Offset(text, edit.GetProperty("range").GetProperty("start")), End: Offset(text, edit.GetProperty("range").GetProperty("end")), Text: edit.GetProperty("newText").GetString()!))
				.OrderByDescending(static edit => edit.Start);
			var renamed = text;

			foreach (var edit in edits)
				renamed = renamed[..edit.Start] + edit.Text + renamed[edit.End..];

			Assert.Equal(text.Replace(use, "Letters", StringComparison.Ordinal).Replace("Word = ", "Letters = ", StringComparison.Ordinal), renamed);
		}
	}

	/// <summary>Go to Definition on the name of a grammar file reads the name the editor has, saved or not.</summary>
	[Fact]
	public async Task Go_to_definition_on_a_grammar_file_name_reads_the_unsaved_text()
	{
		const string Hosts = "using DotGram;\n\nnamespace Parsers;\n\n[Gram(\"X.gram\")]\npublic partial class Host;\n";

		File.WriteAllText(Path.Combine(_folder, "X.gram"), "Start = 'x'\nparse Start\n");
		File.WriteAllText(Path.Combine(_folder, "Y.gram"), "Start = 'y'\nparse Start\n");

		var (client, uri) = await OpenAsync("Hosts.cs", Hosts, new ServerOptions());

		await using (client)
		{
			await client.SettleAsync();

			var at = new LineMap(Hosts).Position(Hosts.IndexOf("X.gram", StringComparison.Ordinal));

			await client.NotifyAsync("textDocument/didChange", new
			{
				textDocument   = new { uri, version = 2 },
				contentChanges = new[] { new { range = new { start = new { line = at.Line, character = at.Character }, end = new { line = at.Line, character = at.Character + 1 } }, text = "Y" } },
			});
			await client.SettleAsync();

			var target = (await client.RequestAsync("textDocument/definition", new
			{
				textDocument = new { uri },
				position     = new { line = at.Line, character = at.Character + 2 },
			})).EnumerateArray().Single();

			Assert.Equal(LspClient.Uri(Path.Combine(_folder, "Y.gram")), target.GetProperty("uri").GetString());
		}
	}

	/// <summary>
	/// A C# document that names no grammar, in a project that declares no DSL, has nothing analysed — no
	/// semantic model is asked for it — until an edit writes a grammar into it.
	/// </summary>
	[Fact]
	public async Task A_document_that_can_hold_no_grammar_is_not_analysed_until_it_can()
	{
		const string Plain = "namespace Parsers;\n\npublic static class Plain\n{\n}\n";

		var (client, uri) = await OpenAsync("Plain.cs", Plain);

		await using (client)
		{
			await client.SettleAsync();

			var document = client.Server.CSharpDocument(uri)!;

			Assert.False(document.Analysed);
			Assert.Empty(client.Notifications(GramLanguageServer.EmbeddedColors));

			await client.NotifyAsync("textDocument/didChange", new
			{
				textDocument   = new { uri, version = 2 },
				contentChanges = new[] { new { range = new { start = new { line = 2, character = 0 }, end = new { line = 2, character = 0 } }, text = "[DotGram.Gram(\"Start = 'a' & Missing\\nparse Start\\n\")]\n" } },
			});

			await client.DiagnosticsAsync(uri, 2);

			Assert.True(document.Analysed);
		}
	}

	/// <summary>A document that only calls a DSL's method is analysed where its project declares the DSL.</summary>
	[Fact]
	public async Task A_document_in_a_project_that_declares_a_dsl_is_analysed()
	{
		File.WriteAllText(Path.Combine(_folder, "Language.cs"), "using DotGram;\n\nnamespace Parsers;\n\n[Gram(\"Start = 'a'\\nparse Start\\n\")]\n[GramLanguage(\"letters\")]\npublic static partial class Letters;\n");

		var (client, uri) = await OpenAsync("Use.cs", "namespace Parsers;\n\npublic static class Use\n{\n}\n");

		await using (client)
		{
			await client.SettleAsync();

			Assert.True(client.Server.CSharpDocument(uri)!.Analysed);
		}
	}

	/// <summary>
	/// Where the client runs the C# extension and the project runs the DotGram generator, the build shows a
	/// grammar's diagnostics, and by default the server does not show them a second time; asked to, it does.
	/// </summary>
	[Theory]
	[InlineData(false, "auto", false)]
	[InlineData(true, "auto", false)]
	[InlineData(false, "on", true)]
	public async Task The_builds_diagnostics_are_not_shown_twice(bool evaluate, string mode, bool shown)
	{
		File.WriteAllText(Path.Combine(_folder, "App.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n  <ItemGroup>\n    <PackageReference Include=\"DotGram\" Version=\"0.3.0\" />\n  </ItemGroup>\n</Project>\n");

		var path = Path.Combine(_folder, "Raw.cs");
		var uri  = LspClient.Uri(path);

		File.WriteAllText(path, Raw);

		await using var client = new LspClient(new ServerOptions { CSharp = false, Evaluate = evaluate })
		{
			Options = new { trusted = true, csharpExtension = true, embeddedDiagnostics = mode },
		};

		await client.InitializeAsync(_folder);
		await client.OpenCSharpAsync(uri, Raw);
		await ColorsAsync(client, uri, 1);
		await client.SettleAsync();

		var published = client.Notifications("textDocument/publishDiagnostics")
			.Any(parameters => parameters.GetProperty("uri").GetString() == uri && parameters.GetProperty("diagnostics").GetArrayLength() > 0);

		Assert.Equal(shown, published);
	}

	/// <summary>A span carried over edits as Visual Studio's edge-exclusive tracking carries it.</summary>
	[Theory]
	[InlineData(10, 5, 0, 0, "ab", 12, 5)]   // inserted before: moved
	[InlineData(10, 5, 10, 10, "ab", 12, 5)] // inserted at its start: moved, not grown
	[InlineData(10, 5, 15, 15, "ab", 10, 5)] // inserted at its end: not grown
	[InlineData(10, 5, 12, 12, "ab", 10, 7)] // inserted inside: grown
	[InlineData(10, 5, 8, 12, "", 8, 3)]     // its start deleted: shrunk
	[InlineData(10, 5, 9, 16, "x", 10, 0)]   // replaced whole: gone
	public void A_span_is_carried_over_an_edit(int start, int length, int from, int to, string inserted, int movedStart, int movedLength)
	{
		var text   = new string('.', 40);
		var first  = new GramVersion(1, SourceText.From(text));
		var lines  = new LineMap(text);
		var second = first.Apply(2, [new TextDocumentContentChangeEvent(inserted, lines.Range(from, to - from))]);

		Assert.Equal(new TextSpan(movedStart, movedLength), OpenCSharp.Translate(new TextSpan(start, length), [second], second.Text.Length));
	}

	async Task<(LspClient Client, string Uri)> OpenAsync(string name, string text, ServerOptions? options = null)
	{
		var path = Path.Combine(_folder, name);

		File.WriteAllText(path, text);

		var uri    = LspClient.Uri(path);
		var client = await LspClient.StartAsync(options, _folder);

		await client.OpenCSharpAsync(uri, text);

		return (client, uri);
	}

	static async Task<JsonElement> ColorsAsync(LspClient client, string uri, int version)
	{
		return await client.WaitForAsync(GramLanguageServer.EmbeddedColors, parameters =>
			parameters.GetProperty("uri").GetString() == uri &&
			parameters.GetProperty("version").GetInt32() == version &&
			parameters.GetProperty("data").GetArrayLength() > 0);
	}

	/// <summary>Waits until the analysis that knows the grammars' symbols, not only their colours, is published.</summary>
	static async Task ColorsWithSymbolsAsync(LspClient client, string uri)
	{
		await client.DiagnosticsAsync(uri, 1);
		await client.SettleAsync();
	}

	static IReadOnlyList<(int Line, int Character, int EndLine, int EndCharacter, string Kind)> Spans(JsonElement colors)
	{
		var kinds  = colors.GetProperty("kinds").EnumerateArray().Select(static kind => kind.GetString()!).ToArray();
		var data   = colors.GetProperty("data").EnumerateArray().Select(static value => value.GetInt32()).ToArray();
		var result = new List<(int, int, int, int, string)>();

		for (var at = 0; at < data.Length; at += 5)
			result.Add((data[at], data[at + 1], data[at + 2], data[at + 3], kinds[data[at + 4]]));

		return result;
	}

	static int Offset(string text, JsonElement position)
	{
		return new LineMap(text).Offset(new Position(position.GetProperty("line").GetInt32(), position.GetProperty("character").GetInt32()));
	}

	static string Slice(string text, JsonElement range)
	{
		var start = Offset(text, range.GetProperty("start"));

		return text[start..Offset(text, range.GetProperty("end"))];
	}

	static int Line(string text, int offset)
	{
		return new LineMap(text).Position(offset).Line;
	}

	static int Column(string text, int offset)
	{
		return new LineMap(text).Position(offset).Character;
	}

	static string Json(object value)
	{
		return value is JsonElement element ? element.GetRawText().Replace(" ", "", StringComparison.Ordinal) : JsonSerializer.Serialize(value, LspClient.Json);
	}
}
