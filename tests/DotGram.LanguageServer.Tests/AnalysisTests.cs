using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>What the server publishes and colours equals what the shared core computed, for the largest grammar and for every one.</summary>
public sealed class AnalysisTests
{
	static string TransactSqlPath => Path.Combine(LspClient.SourceRoot(), "src", "DotGram.Sql", "TransactSql", "TransactSql.gram");

	/// <summary>
	/// The T-SQL grammar opened in its project's folder: its host is found, <c>Lexical</c> with it, and
	/// the diagnostics published are the core's under the options the host gives.
	/// </summary>
	[Fact]
	public async Task The_TransactSql_diagnostics_are_the_cores_under_its_hosts_options()
	{
		var folder = Path.Combine(LspClient.SourceRoot(), "src", "DotGram.Sql");
		var text   = File.ReadAllText(TransactSqlPath);
		var uri    = LspClient.Uri(TransactSqlPath);

		await using var client = await LspClient.StartAsync(null, folder);

		await client.OpenAsync(uri, text);

		var session = client.Server.Document(uri)!.Session;
		var result  = await ResolvedAsync(session);

		Assert.True(result.Resolution.Context!.Value.Lexical);

		var expected = Rendered(text, GramAnalysisSession.Analyze(text, result.Resolution.Context, suppressContextDiagnostics: false).Document.Diagnostics);

		// The analysis made before the host was found was published too; the last one is the host's.
		var published = await client.WaitForAsync("textDocument/publishDiagnostics", item =>
			item.GetProperty("uri").GetString() == uri && Rendered(item).SequenceEqual(expected));

		Assert.Equal(expected, Rendered(published));
	}

	/// <summary>Outside any folder the grammar is read as it stands, quiet about what depends on a host, as a copy is.</summary>
	[Fact]
	public async Task Without_a_folder_the_TransactSql_diagnostics_are_the_cores_read_as_a_copy()
	{
		var text = File.ReadAllText(TransactSqlPath);
		var uri  = LspClient.Uri(TransactSqlPath);

		await using var client = await LspClient.StartAsync();

		await client.OpenAsync(uri, text);

		var published = await client.DiagnosticsAsync(uri, 1);
		var expected  = GramAnalysisSession.Analyze(text, null, suppressContextDiagnostics: true).Document.Diagnostics;

		Assert.True(client.Server.Document(uri)!.Session.Store.IsCopy(Path.GetFullPath(TransactSqlPath)));
		Assert.Equal(Rendered(text, expected), Rendered(published));
	}

	/// <summary>
	/// An edit to the T-SQL grammar is told about within the session's own analysis time, its pause
	/// after typing and 200 ms: the server adds no wait of its own. Same process, same machine as the
	/// analysis, so a busy machine slows both.
	/// </summary>
	[Fact]
	public async Task An_edit_is_told_about_within_the_analysis_time()
	{
		var text = File.ReadAllText(TransactSqlPath);
		const string uri = "untitled:TransactSql.gram";

		await using var client = await LspClient.StartAsync();

		await client.OpenAsync(uri, text);
		await client.DiagnosticsAsync(uri, 1);

		var watch = Stopwatch.StartNew();

		await client.NotifyAsync("textDocument/didChange", new
		{
			textDocument   = new { uri, version = 2 },
			contentChanges = new[] { new { range = new { start = LspClient.Position(text, 0), end = LspClient.Position(text, 0) }, text = "// edited\n" } },
		});
		await client.DiagnosticsAsync(uri, 2);

		var elapsed  = watch.Elapsed;
		var analysis = client.Server.Document(uri)!.Session.Result!.Timing.Total;
		var bound    = analysis + TimeSpan.FromMilliseconds(GramAnalysisSession.AnalysisDelayMilliseconds + 200);

		TestContext.Current.TestOutputHelper?.WriteLine(
			$"keystroke to diagnostics {elapsed.TotalMilliseconds:F0} ms; analysis {analysis.TotalMilliseconds:F0} ms; " +
			$"pause {GramAnalysisSession.AnalysisDelayMilliseconds} ms; the server's own {(elapsed - analysis).TotalMilliseconds - GramAnalysisSession.AnalysisDelayMilliseconds:F0} ms");

		Assert.True(elapsed <= bound, $"An edit took {elapsed.TotalMilliseconds:F0} ms to be told about; the analysis took {analysis.TotalMilliseconds:F0} ms.");
	}

	/// <summary>
	/// A thousand keystrokes into the T-SQL grammar, as fast as the client sends them, hold a request
	/// behind them for little: an edit reads only what it changed, not the whole grammar.
	/// </summary>
	[Fact]
	public async Task A_burst_of_keystrokes_does_not_hold_a_request_up()
	{
		var text = File.ReadAllText(TransactSqlPath);
		const string uri = "untitled:Burst.gram";

		await using var client = await LspClient.StartAsync();

		await client.OpenAsync(uri, text);
		await client.DiagnosticsAsync(uri, 1);

		// At the end of the first line, typed one space at a time.
		var at    = text.IndexOfAny(['\r', '\n']);
		var watch = Stopwatch.StartNew();

		for (var version = 2; version <= 1001; version++)
		{
			var position = new { line = 0, character = at };

			await client.NotifyAsync("textDocument/didChange", new
			{
				textDocument   = new { uri, version },
				contentChanges = new[] { new { range = new { start = position, end = position }, text = " " } },
			});

			at++;
		}

		text = text.Insert(text.IndexOfAny(['\r', '\n']), new string(' ', 1000));

		var sent = watch.Elapsed;

		await client.RequestAsync("textDocument/hover", LspClient.At(uri, text, 0));

		var answered = watch.Elapsed;

		TestContext.Current.TestOutputHelper?.WriteLine(
			$"1,000 edits sent in {sent.TotalMilliseconds:F0} ms; a request after them answered at {answered.TotalMilliseconds:F0} ms");

		Assert.Equal(text, client.Server.Document(uri)!.Current.String);
		Assert.True(answered - sent < TimeSpan.FromSeconds(6), $"A request waited {(answered - sent).TotalMilliseconds:F0} ms behind 1,000 edits.");
	}

	/// <summary>Every grammar of the repository coloured through the protocol exactly as its classifications say, through the legend.</summary>
	[Fact]
	public async Task The_semantic_tokens_are_the_classifications_of_every_grammar()
	{
		await using var client = await LspClient.StartAsync();

		var failures = new List<string>();
		var index    = 0;

		foreach (var (name, text) in Corpus.Grammars())
		{
			var uri = $"untitled:corpus{++index}.gram";

			await client.OpenAsync(uri, text);

			var tokens   = await client.RequestAsync("textDocument/semanticTokens/full", new { textDocument = new { uri } });
			var document = client.Server.Document(uri)!.Session.Result!.Document;
			var actual   = Decode(text, tokens.GetProperty("data").EnumerateArray().Select(static item => item.GetInt32()).ToArray());
			var expected = Expected(text, document.Classifications);

			if (!actual.SequenceEqual(expected))
			{
				var at = Enumerable.Range(0, Math.Min(actual.Count, expected.Count)).FirstOrDefault(i => actual[i] != expected[i]);

				failures.Add($"{name}: {actual.Count} tokens, {expected.Count} expected; first difference at {at}: " +
					$"{(at < actual.Count ? actual[at] : default)} vs {(at < expected.Count ? expected[at] : default)}");
			}

			await client.NotifyAsync("textDocument/didClose", new { textDocument = new { uri } });
		}

		Assert.True(index > 50, $"The corpus has {index} grammars.");
		Assert.True(failures.Count == 0, string.Join("\n", failures.Take(20)));
	}

	/// <summary>Folding and the outline are the document's own, line for line.</summary>
	[Fact]
	public async Task Folding_and_the_outline_are_the_documents()
	{
		var text = File.ReadAllText(TransactSqlPath);
		const string uri = "untitled:Structure.gram";

		await using var client = await LspClient.StartAsync();

		await client.OpenAsync(uri, text);

		var folding  = await client.RequestAsync("textDocument/foldingRange", new { textDocument = new { uri } });
		var symbols  = await client.RequestAsync("textDocument/documentSymbol", new { textDocument = new { uri } });
		var document = client.Server.Document(uri)!.Session.Result!.Document;
		var lines    = new LineMap(text);

		var expectedFolding = document.FoldingRanges
			.Select(range => (Start: lines.Position(range.Position).Line, End: lines.Position(range.Position + range.Length).Line, range.CollapsedText))
			.Where(static range => range.End > range.Start)
			.ToArray();
		var actualFolding = folding.EnumerateArray()
			.Select(static range => (Start: range.GetProperty("startLine").GetInt32(), End: range.GetProperty("endLine").GetInt32(), range.GetProperty("collapsedText").GetString()!))
			.ToArray();

		Assert.NotEmpty(expectedFolding);
		Assert.Equal(expectedFolding, actualFolding);

		Assert.NotEmpty(document.DocumentSymbols);
		Assert.Equal(Outline(document.DocumentSymbols, lines), Outline(symbols));
	}

	static IEnumerable<string> Outline(IReadOnlyList<GramDocumentSymbol> symbols, LineMap lines, string indent = "")
	{
		foreach (var symbol in symbols)
		{
			yield return $"{indent}{symbol.Name} {symbol.Kind} {lines.Position(symbol.Position)} {lines.Position(symbol.SelectionPosition)}";

			foreach (var child in Outline(symbol.Children, lines, indent + "  "))
				yield return child;
		}
	}

	static IEnumerable<string> Outline(JsonElement symbols, string indent = "")
	{
		foreach (var symbol in symbols.EnumerateArray())
		{
			var kind = symbol.GetProperty("kind").GetInt32() switch
			{
				Lsp.SymbolNamespace => GramDocumentSymbolKind.Namespace,
				Lsp.SymbolMethod    => GramDocumentSymbolKind.Publication,
				_                   => GramDocumentSymbolKind.Rule,
			};

			yield return $"{indent}{symbol.GetProperty("name").GetString()} {kind} {Start(symbol.GetProperty("range"))} {Start(symbol.GetProperty("selectionRange"))}";

			foreach (var child in Outline(symbol.GetProperty("children"), indent + "  "))
				yield return child;
		}
	}

	static Position Start(JsonElement range)
	{
		var start = range.GetProperty("start");

		return new Position(start.GetProperty("line").GetInt32(), start.GetProperty("character").GetInt32());
	}

	static async Task<GramAnalysisResult<GramVersion>> ResolvedAsync(GramAnalysisSession<GramVersion> session)
	{
		var deadline = DateTime.UtcNow + LspClient.Timeout;

		while (DateTime.UtcNow < deadline)
		{
			if (session.Result is { Resolution.State: StandaloneGrammarState.Resolved } result)
				return result;

			await Task.Delay(50, TestContext.Current.CancellationToken);
		}

		Assert.Fail($"The host was not found: {session.Resolution.State}.");
		return null;
	}

	static IReadOnlyList<string> Rendered(string text, IReadOnlyList<GramDiagnostic> diagnostics)
	{
		var lines = new LineMap(text);

		return diagnostics
			.Select(diagnostic => $"{diagnostic.Id} {lines.Range(diagnostic.Position, diagnostic.Length)} {DiagnosticsMap.Severity(diagnostic.Severity)} dotgram {diagnostic.Message}")
			.ToArray();
	}

	static IReadOnlyList<string> Rendered(JsonElement published)
	{
		return published.GetProperty("diagnostics").EnumerateArray()
			.Select(static diagnostic =>
			{
				var range = diagnostic.GetProperty("range");
				var end   = range.GetProperty("end");

				return $"{diagnostic.GetProperty("code").GetString()} " +
					$"{new Range(Start(range), new Position(end.GetProperty("line").GetInt32(), end.GetProperty("character").GetInt32()))} " +
					$"{diagnostic.GetProperty("severity").GetInt32()} {diagnostic.GetProperty("source").GetString()} {diagnostic.GetProperty("message").GetString()}";
			})
			.ToArray();
	}

	/// <summary>The protocol's integers back to offsets in the text.</summary>
	static List<(int Start, int Length, string Type)> Decode(string text, int[] data)
	{
		var lines     = new LineMap(text);
		var result    = new List<(int Start, int Length, string Type)>();
		var line      = 0;
		var character = 0;

		for (var at = 0; at < data.Length; at += 5)
		{
			character = data[at] == 0 ? character + data[at + 1] : data[at + 1];
			line     += data[at];

			result.Add((lines.LineStart(line) + character, data[at + 2], SemanticTokensMap.TokenTypes[data[at + 3]]));
		}

		return result;
	}

	/// <summary>
	/// The classifications cut where the text's lines end, which is where a client's tokens end, and
	/// nothing else: none of them overlap, so none is lost.
	/// </summary>
	static List<(int Start, int Length, string Type)> Expected(string text, IReadOnlyList<GramClassifiedSpan> classifications)
	{
		var result = new List<(int Start, int Length, string Type)>();
		var end    = 0;

		foreach (var item in classifications.Where(static item => item.Length > 0).OrderBy(static item => item.Position))
		{
			Assert.True(item.Position >= end, $"Two classifications overlap at {item.Position}.");

			var type  = SemanticTokensMap.TypeOf(item.Kind);
			var start = item.Position;

			for (var at = item.Position; at < item.Position + item.Length; at++)
			{
				if (text[at] is not ('\r' or '\n'))
					continue;

				if (at > start)
					result.Add((start, at - start, type));

				start = at + 1;
			}

			if (item.Position + item.Length > start)
				result.Add((start, item.Position + item.Length - start, type));

			end = item.Position + item.Length;
		}

		return result;
	}
}

/// <summary>Every grammar in the repository: each <c>.gram</c> file and each grammar written into a <c>[Gram("…")]</c>, as the editor tests read them.</summary>
static class Corpus
{
	public static IReadOnlyList<(string Name, string Text)> Grammars()
	{
		var root   = LspClient.SourceRoot();
		var result = new List<(string Name, string Text)>();

		foreach (var file in Files(root, "*.gram"))
			result.Add((Path.GetRelativePath(root, file), File.ReadAllText(file)));

		foreach (var file in Files(root, "*.cs"))
		{
			var source = File.ReadAllText(file);

			if (!source.Contains("Gram", StringComparison.Ordinal))
				continue;

			var index = 0;

			foreach (var attribute in CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<AttributeSyntax>())
			{
				var name = attribute.Name switch
				{
					QualifiedNameSyntax qualified   => qualified.Right.Identifier.ValueText,
					AliasQualifiedNameSyntax global => global.Name.Identifier.ValueText,
					SimpleNameSyntax simple         => simple.Identifier.ValueText,
					_                               => "",
				};

				if (name is not ("Gram" or "GramAttribute") ||
					attribute.ArgumentList?.Arguments.FirstOrDefault() is not { NameEquals: null } argument ||
					Constant(argument.Expression) is not { } grammar ||
					grammar.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) && grammar.IndexOf('\n') < 0)
					continue;

				result.Add(($"{Path.GetRelativePath(root, file)}#{++index}", grammar));
			}
		}

		return result;
	}

	static string? Constant(ExpressionSyntax expression)
	{
		return expression switch
		{
			LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression) => literal.Token.ValueText,
			ParenthesizedExpressionSyntax parenthesized => Constant(parenthesized.Expression),
			BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AddExpression) &&
				Constant(binary.Left) is { } left && Constant(binary.Right) is { } right => left + right,
			_ => null,
		};
	}

	static IEnumerable<string> Files(string root, string pattern)
	{
		foreach (var top in new[] { "src", "examples", "tests", "benchmarks" })
			foreach (var file in Directory.EnumerateFiles(Path.Combine(root, top), pattern, SearchOption.AllDirectories))
				if (!Path.GetRelativePath(root, file).Split('/', '\\').Any(static part => part is "bin" or "obj" or ".build"))
					yield return file;
	}
}
