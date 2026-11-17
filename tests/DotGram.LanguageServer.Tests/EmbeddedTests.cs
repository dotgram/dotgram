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
			var colors = Spans(await ColorsAsync(client, uri, 1));
			var start  = text.IndexOf("Start", StringComparison.Ordinal);

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

	async Task<(LspClient Client, string Uri)> OpenAsync(string name, string text)
	{
		var path = Path.Combine(_folder, name);

		File.WriteAllText(path, text);

		var uri    = LspClient.Uri(path);
		var client = await LspClient.StartAsync(null, _folder);

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
