using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>The protocol itself: the lifecycle, what the server says it can do, edits, and requests that wait.</summary>
public sealed class ProtocolTests
{
	[Fact]
	public async Task Initialize_shutdown_and_exit_go_round()
	{
		var client = new LspClient();
		var result = await client.InitializeAsync();
		var server = result.GetProperty("capabilities");

		Assert.Equal("DotGram", result.GetProperty("serverInfo").GetProperty("name").GetString());
		Assert.Equal("utf-16", server.GetProperty("positionEncoding").GetString());
		Assert.Equal(Lsp.SyncIncremental, server.GetProperty("textDocumentSync").GetProperty("change").GetInt32());
		Assert.True(server.GetProperty("textDocumentSync").GetProperty("openClose").GetBoolean());

		foreach (var provider in new[]
		{
			"hoverProvider", "definitionProvider", "referencesProvider", "documentHighlightProvider",
			"foldingRangeProvider", "documentSymbolProvider",
		})
			Assert.True(server.GetProperty(provider).GetBoolean(), provider);

		Assert.True(server.GetProperty("completionProvider").GetProperty("resolveProvider").GetBoolean());
		Assert.True(server.GetProperty("renameProvider").GetProperty("prepareProvider").GetBoolean());
		Assert.True(server.GetProperty("semanticTokensProvider").GetProperty("full").GetBoolean());
		Assert.Equal(
			SemanticTokensMap.TokenTypes,
			server.GetProperty("semanticTokensProvider").GetProperty("legend").GetProperty("tokenTypes")
				.EnumerateArray().Select(static type => type.GetString()!));
		Assert.Contains("(", server.GetProperty("signatureHelpProvider").GetProperty("triggerCharacters").EnumerateArray().Select(static item => item.GetString()));

		var shutdown = await client.RequestRawAsync("shutdown", null);

		Assert.Equal(JsonValueKind.Null, shutdown.GetProperty("result").ValueKind);
		Assert.True(client.Connection.ShutdownRequested);

		await client.DisposeAsync();
	}

	[Fact]
	public async Task A_method_the_server_does_not_have_is_answered_as_such()
	{
		await using var client = await LspClient.StartAsync();

		var answer = await client.RequestRawAsync("textDocument/codeAction", new { });

		Assert.Equal(Lsp.MethodNotFound, answer.GetProperty("error").GetProperty("code").GetInt32());
	}

	/// <summary>A sequence of edits, as a client sends them, leaves the text the client holds.</summary>
	[Fact]
	public async Task Incremental_edits_leave_the_text_the_client_holds()
	{
		await using var client = await LspClient.StartAsync();

		const string uri = "untitled:Edits.gram";
		var text = "Start = Word\r\nWord = ['a'..'z']+\nparse Start\r";

		await client.OpenAsync(uri, text);

		var edits = new (int Start, int End, string Text)[]
		{
			(0, 5, "Begin"),                   // replace a word
			(12, 12, "\n// note"),             // insert at the end of the first line
			(text.Length, text.Length, "\n"), // insert at the very end
			(6, 7, ""),                        // delete
			(0, 0, "\uD83D\uDE00 "),                     // a surrogate pair: two UTF-16 units
			(3, 8, "Start"),                   // after it
		};

		var version = 1;

		foreach (var (start, end, inserted) in edits)
		{
			var range = new { start = LspClient.Position(text, start), end = LspClient.Position(text, end) };

			text = text.Substring(0, start) + inserted + text.Substring(end);
			version++;

			await client.NotifyAsync("textDocument/didChange", new
			{
				textDocument   = new { uri, version },
				contentChanges = new[] { new { range, text = inserted } },
			});
		}

		await client.DiagnosticsAsync(uri, version);

		var current = client.Server.Document(uri)!.Current;

		Assert.Equal(version, current.Number);
		Assert.Equal(text, current.String);
	}

	/// <summary>The protocol's lines end at <c>\n</c>, <c>\r\n</c> and <c>\r</c>, and at nothing else Roslyn would break a line at.</summary>
	[Fact]
	public void Lines_are_the_protocols()
	{
		var text  = "a\u2028b\r\nc\rd\ne\u0085f";
		var lines = new LineMap(SourceText.From(text));

		Assert.Equal(4, lines.LineCount);
		Assert.Equal(new Position(0, 2), lines.Position(text.IndexOf('b', StringComparison.Ordinal)));
		Assert.Equal(new Position(3, 0), lines.Position(text.IndexOf('e', StringComparison.Ordinal)));
		Assert.Equal(text.IndexOf('c', StringComparison.Ordinal), lines.Offset(new Position(1, 0)));
		Assert.Equal(text.IndexOf('\r', StringComparison.Ordinal), lines.Offset(new Position(0, 99)));
		Assert.Equal(text.Length, lines.Offset(new Position(9, 0)));
	}

	/// <summary>
	/// A request still running on a document holds up neither the edits to it nor the requests after
	/// them, and a <c>$/cancelRequest</c> for it is answered as the protocol says while it runs.
	/// </summary>
	[Fact]
	public async Task A_running_request_holds_up_neither_an_edit_nor_its_cancellation()
	{
		var release  = new ManualResetEventSlim(false);
		var analyses = 0;
		var options  = new ServerOptions
		{
			CSharp  = false,
			Analyze = (text, context, quiet, clock) =>
			{
				// The first analysis takes as long as the test says; the rest are the compiler's.
				if (Interlocked.Increment(ref analyses) == 1)
					release.Wait(LspClient.Timeout);

				return GramAnalysisSession.Analyze(text, context, quiet, clock);
			},
		};

		await using var client = await LspClient.StartAsync(options);

		const string uri = "untitled:Slow.gram";
		const string text = "Start = Word\nWord = ['a'..'z']+\nparse Start\n";

		await client.OpenAsync(uri, text);

		// Waits for the version's analysis, which the test holds back.
		var references = client.RequestRawAsync("textDocument/references", new
		{
			textDocument = new { uri },
			position     = LspClient.Position(text, 0),
			context      = new { includeDeclaration = true },
		}, out var id);

		await client.NotifyAsync("textDocument/didChange", new
		{
			textDocument   = new { uri, version = 2 },
			contentChanges = new[] { new { text = text + "// edited\n" } },
		});

		// Answered while the first request runs: the edit was applied, and requests go on.
		var hover = await client.RequestRawAsync("textDocument/hover", LspClient.At(uri, text, 0));

		Assert.True(hover.TryGetProperty("result", out _));
		Assert.False(references.IsCompleted);
		Assert.Equal(2, client.Server.Document(uri)!.Current.Number);

		await client.NotifyAsync("$/cancelRequest", new { id });

		var cancelled = await references;

		Assert.Equal(Lsp.RequestCancelled, cancelled.GetProperty("error").GetProperty("code").GetInt32());

		release.Set();

		await client.DiagnosticsAsync(uri, 2);
	}

	/// <summary><c>shutdown</c> is answered after every request in flight has been.</summary>
	[Fact]
	public async Task Shutdown_waits_for_the_requests_in_flight()
	{
		var release = new ManualResetEventSlim(false);
		var options = new ServerOptions
		{
			CSharp  = false,
			Analyze = (text, context, quiet, clock) =>
			{
				release.Wait(LspClient.Timeout);
				return GramAnalysisSession.Analyze(text, context, quiet, clock);
			},
		};

		var client = await LspClient.StartAsync(options);

		const string uri = "untitled:Shutdown.gram";
		const string text = "Start = 'a'\nparse Start\n";

		await client.OpenAsync(uri, text);

		var symbols  = client.RequestRawAsync("textDocument/documentSymbol", new { textDocument = new { uri } });
		var shutdown = client.RequestRawAsync("shutdown", null);

		await Task.Delay(200, TestContext.Current.CancellationToken);

		Assert.False(shutdown.IsCompleted);

		release.Set();

		Assert.True((await symbols).TryGetProperty("result", out _));
		Assert.Equal(JsonValueKind.Null, (await shutdown).GetProperty("result").ValueKind);

		await client.DisposeAsync();
	}
}
