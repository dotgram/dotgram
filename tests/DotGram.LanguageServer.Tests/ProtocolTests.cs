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

	/// <summary>
	/// Edits of every shape a client can send — line breaks inserted next to line breaks, a <c>\r\n</c>
	/// made out of a <c>\r</c> and a <c>\n</c> typed apart — leave the text the client holds.
	/// </summary>
	[Fact]
	public async Task Random_edits_leave_the_text_the_client_holds()
	{
		await using var client = await LspClient.StartAsync();

		const string uri = "untitled:Random.gram";
		var random = new Random(31102026);
		var pieces = new[] { "a", "\r", "\n", "\r\n", "bc", "\n\r", "" };
		var text   = "Start = 'a'\r\nparse Start\r";

		await client.OpenAsync(uri, text);

		for (var version = 2; version < 400; version++)
		{
			var lines    = new LineMap(text);
			var start    = lines.Offset(lines.Position(random.Next(text.Length + 1)));
			var end      = lines.Offset(lines.Position(Math.Min(text.Length, start + random.Next(4))));
			var inserted = string.Concat(Enumerable.Range(0, random.Next(3)).Select(_ => pieces[random.Next(pieces.Length)]));

			await client.NotifyAsync("textDocument/didChange", new
			{
				textDocument   = new { uri, version },
				contentChanges = new[] { new { range = new { start = LspClient.Position(text, start), end = LspClient.Position(text, end) }, text = inserted } },
			});

			text = text.Substring(0, start) + inserted + text.Substring(end);
		}

		await client.DiagnosticsAsync(uri, 399);

		Assert.Equal(text, client.Server.Document(uri)!.Current.String);
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
		var asked   = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var options = new ServerOptions
		{
			CSharp          = false,
			CSharpQuickInfo = async (_, _, cancellationToken) =>
			{
				// Quick Info in C# that takes until it is cancelled.
				asked.TrySetResult();
				await Task.Delay(Timeout.Infinite, cancellationToken);
				return null;
			},
		};

		await using var client = await LspClient.StartAsync(options);

		const string uri  = "untitled:Slow.gram";
		const string text = "Start : @int = 'a' => @(System.Math.Abs(1))\nparse Start\n";

		await client.OpenAsync(uri, text);
		await client.DiagnosticsAsync(uri, 1);

		var hover = client.RequestRawAsync("textDocument/hover", LspClient.At(uri, text, text.IndexOf("Abs", StringComparison.Ordinal)), out var id);

		await asked.Task.WaitAsync(LspClient.Timeout, TestContext.Current.CancellationToken);

		await client.NotifyAsync("textDocument/didChange", new
		{
			textDocument   = new { uri, version = 2 },
			contentChanges = new[] { new { range = new { start = LspClient.Position(text, 0), end = LspClient.Position(text, 0) }, text = "// edited\n" } },
		});

		// Answered while the first request runs: the edit was applied, and requests go on.
		var symbols = await client.RequestAsync("textDocument/documentSymbol", new { textDocument = new { uri } });

		Assert.Equal("Start", symbols.EnumerateArray().First().GetProperty("name").GetString());
		Assert.Equal(1, symbols.EnumerateArray().First().GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());
		Assert.False(hover.IsCompleted);

		await client.NotifyAsync("$/cancelRequest", new { id });

		var cancelled = await hover;

		Assert.Equal(Lsp.RequestCancelled, cancelled.GetProperty("error").GetProperty("code").GetInt32());
	}

	/// <summary>
	/// <c>shutdown</c> cancels the requests in flight and is answered once they have been: a request
	/// waiting for an analysis would hold it up as long as the analysis takes, and a client gives up.
	/// </summary>
	[Fact]
	public async Task Shutdown_cancels_the_requests_in_flight_and_answers_at_once()
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
		var watch    = System.Diagnostics.Stopwatch.StartNew();
		var shutdown = await client.RequestRawAsync("shutdown", null);

		Assert.True(watch.Elapsed < TimeSpan.FromSeconds(2), $"shutdown took {watch.Elapsed}.");
		Assert.Equal(JsonValueKind.Null, shutdown.GetProperty("result").ValueKind);
		Assert.Equal(Lsp.RequestCancelled, (await symbols).GetProperty("error").GetProperty("code").GetInt32());

		release.Set();

		await client.DisposeAsync();
	}

	/// <summary>A request without what it must have is refused as invalid, and the server goes on.</summary>
	[Fact]
	public async Task A_malformed_request_is_refused_and_the_server_goes_on()
	{
		await using var client = await LspClient.StartAsync();

		const string uri  = "untitled:Malformed.gram";
		const string text = "Start = 'a'\nparse Start\n";

		await client.OpenAsync(uri, text);
		await client.DiagnosticsAsync(uri, 1);

		var requests = new (string Method, object Parameters)[]
		{
			("textDocument/hover", new { position = LspClient.Position(text, 0) }),
			("textDocument/hover", new { textDocument = new { uri } }),
			("textDocument/hover", new { textDocument = new { }, position = LspClient.Position(text, 0) }),
			("textDocument/rename", new { textDocument = new { uri }, position = LspClient.Position(text, 0) }),
			("textDocument/completion", new { textDocument = new { uri }, position = new { line = 0 } }),
			("completionItem/resolve", new[] { 1 }),
		};

		foreach (var (method, parameters) in requests)
		{
			var answer = await client.RequestRawAsync(method, parameters);

			Assert.True(answer.TryGetProperty("error", out var error), $"{method} {JsonSerializer.Serialize(parameters)} was answered: {answer}");
			Assert.Equal(Lsp.InvalidParams, error.GetProperty("code").GetInt32());
		}

		var hover = await client.RequestAsync("textDocument/hover", LspClient.At(uri, text, 0));

		Assert.Equal(JsonValueKind.Object, hover.ValueKind);
	}

	/// <summary>Before <c>initialize</c> a request is refused and a notification dropped; a second <c>initialize</c> is refused.</summary>
	[Fact]
	public async Task The_lifecycle_is_the_protocols()
	{
		var client = new LspClient();

		await client.OpenAsync("untitled:Early.gram", "Start = 'a'\n", waitForServer: false);

		var early = await client.RequestRawAsync("textDocument/documentSymbol", new { textDocument = new { uri = "untitled:Early.gram" } });

		Assert.Equal(Lsp.ServerNotInitialized, early.GetProperty("error").GetProperty("code").GetInt32());
		Assert.Null(client.Server.Document("untitled:Early.gram"));

		var shutdown = await client.RequestRawAsync("shutdown", null);

		Assert.Equal(Lsp.ServerNotInitialized, shutdown.GetProperty("error").GetProperty("code").GetInt32());
		Assert.False(client.Connection.ShutdownRequested);

		await client.InitializeAsync();

		var again = await client.RequestRawAsync("initialize", new { processId = 1, capabilities = new { } });

		Assert.Equal(Lsp.InvalidRequest, again.GetProperty("error").GetProperty("code").GetInt32());

		await client.DisposeAsync();
	}

	/// <summary>
	/// What the client sends to resolve an item, it gets back with the description added: the text
	/// edit it made of the list's default range among it, which the insert text lives in.
	/// </summary>
	[Fact]
	public async Task Resolve_keeps_what_the_client_sent()
	{
		await using var client = await LspClient.StartAsync();

		const string uri  = "untitled:Resolve.gram";
		const string text = "Repeat(n) = 'a'{n}\nStart = Re\nparse Start\n";

		await client.OpenAsync(uri, text);
		await client.DiagnosticsAsync(uri, 1);

		var at   = text.IndexOf("Re\n", StringComparison.Ordinal) + 2;
		var list = await client.RequestAsync("textDocument/completion", LspClient.At(uri, text, at));
		var item = list.GetProperty("items").EnumerateArray().Single(static item => item.GetProperty("label").GetString() == "Repeat");

		Assert.Equal("Repeat(", item.GetProperty("textEditText").GetString());

		// As vscode-languageclient sends it: the default range expanded into the item's own edit.
		var sent = new Dictionary<string, object?>
		{
			["label"]            = "Repeat",
			["textEdit"]         = new { range = list.GetProperty("itemDefaults").GetProperty("editRange"), newText = "Repeat(" },
			["insertTextFormat"] = 1,
			["data"]             = item.GetProperty("data"),
		};

		var resolved = await client.RequestAsync("completionItem/resolve", sent);

		Assert.Equal("Repeat(", resolved.GetProperty("textEdit").GetProperty("newText").GetString());
		Assert.Equal(1, resolved.GetProperty("insertTextFormat").GetInt32());
		Assert.Contains("Repeat(n)", resolved.GetProperty("documentation").GetProperty("value").GetString(), StringComparison.Ordinal);
	}

	/// <summary>
	/// Edits, requests and a close at once, many times over: nothing locks up, every document is told
	/// about, its diagnostics come in the order of its versions, and nothing follows the empty list a
	/// close sends.
	/// </summary>
	[Fact]
	public async Task Edits_requests_and_closes_at_once_neither_lock_up_nor_tell_out_of_order()
	{
		await using var client = await LspClient.StartAsync();

		for (var round = 0; round < 20; round++)
		{
			var uri  = $"untitled:Busy{round}.gram";
			var text = "Start = Word\nWord = ['a'..'z']+\nparse Start\n";

			await client.OpenAsync(uri, text);
			await client.DiagnosticsAsync(uri, 1);

			var requests = new List<Task<JsonElement>>();

			for (var version = 2; version < 30; version++)
			{
				await client.NotifyAsync("textDocument/didChange", new
				{
					textDocument   = new { uri, version },
					contentChanges = new[] { new { range = new { start = LspClient.Position(text, 0), end = LspClient.Position(text, 0) }, text = " " } },
				});

				text = " " + text;

				requests.Add(client.RequestRawAsync("textDocument/hover", LspClient.At(uri, text, 1)));
				requests.Add(client.RequestRawAsync("textDocument/documentSymbol", new { textDocument = new { uri } }));
			}

			// Every other round waits for the last version's analysis first, so that some closes come
			// after a publication and some race one.
			if (round % 2 == 0)
				await client.DiagnosticsAsync(uri, 29);

			await client.NotifyAsync("textDocument/didClose", new { textDocument = new { uri } });
			await Task.WhenAll(requests).WaitAsync(LspClient.Timeout, TestContext.Current.CancellationToken);
		}

		// Whatever is still running for a closed document has had its time.
		await Task.Delay(500, TestContext.Current.CancellationToken);

		for (var round = 0; round < 20; round++)
			AssertToldInOrderAndClosed(client, $"untitled:Busy{round}.gram", round % 2 == 0 ? 2 : 1);
	}

	/// <summary>
	/// An analysis that ends after its document was closed tells nothing: the close is made while the
	/// analysis of the version is held, and the analysis let go after it.
	/// </summary>
	[Fact]
	public async Task An_analysis_that_ends_after_the_close_tells_nothing()
	{
		var hold     = new ManualResetEventSlim(true);
		var analyses = 0;
		var options  = new ServerOptions
		{
			CSharp  = false,
			Analyze = (text, context, quiet, clock) =>
			{
				Interlocked.Increment(ref analyses);
				hold.Wait(LspClient.Timeout);
				return GramAnalysisSession.Analyze(text, context, quiet, clock);
			},
		};

		await using var client = await LspClient.StartAsync(options);

		const string uri  = "untitled:Held.gram";
		const string text = "Start = 'a'\nparse Start\n";

		await client.OpenAsync(uri, text);
		await client.DiagnosticsAsync(uri, 1);

		hold.Reset();

		await client.NotifyAsync("textDocument/didChange", new
		{
			textDocument   = new { uri, version = 2 },
			contentChanges = new[] { new { text = text + "// edited\n" } },
		});

		// The analysis of version 2 has started and is held.
		while (Volatile.Read(ref analyses) < 2)
			await Task.Delay(5, TestContext.Current.CancellationToken);

		await client.NotifyAsync("textDocument/didClose", new { textDocument = new { uri } });
		await client.WaitForAsync("textDocument/publishDiagnostics", static item =>
			item.GetProperty("uri").GetString() == uri && !item.TryGetProperty("version", out _));

		hold.Set();

		await Task.Delay(500, TestContext.Current.CancellationToken);

		AssertToldInOrderAndClosed(client, uri, 1);
		Assert.DoesNotContain(client.Notifications("textDocument/publishDiagnostics"), static item =>
			item.TryGetProperty("version", out var version) && version.ValueKind == JsonValueKind.Number && version.GetInt32() == 2);
	}

	/// <summary>
	/// A request waiting for an analysis lets go of the version it holds when it is cancelled or an
	/// edit overtakes it, not when the analysis is published: a large grammar's versions would pile up.
	/// </summary>
	[Fact]
	public async Task A_cancelled_or_overtaken_request_holds_no_version()
	{
		var hold    = new ManualResetEventSlim(false);
		var options = new ServerOptions
		{
			CSharp  = false,
			Analyze = (text, context, quiet, clock) =>
			{
				hold.Wait(LspClient.Timeout);
				return GramAnalysisSession.Analyze(text, context, quiet, clock);
			},
		};

		await using var client = await LspClient.StartAsync(options);

		const string uri  = "untitled:Waiting.gram";
		const string text = "Start = 'a'\nparse Start\n";

		await client.OpenAsync(uri, text);

		var document = client.Server.Document(uri)!;
		var ids      = new List<int>();
		var asked    = new List<Task<JsonElement>>();

		for (var index = 0; index < 20; index++)
		{
			asked.Add(client.RequestRawAsync("textDocument/documentSymbol", new { textDocument = new { uri } }, out var id));
			ids.Add(id);
		}

		await WaitUntilAsync(() => document.Waiting == 20);

		foreach (var id in ids)
			await client.NotifyAsync("$/cancelRequest", new { id });

		foreach (var cancelled in asked)
			Assert.Equal(Lsp.RequestCancelled, (await cancelled).GetProperty("error").GetProperty("code").GetInt32());

		Assert.Equal(0, document.Waiting);

		// One more, which an edit overtakes: it is answered with what is known, and lets go.
		var overtaken = client.RequestRawAsync("textDocument/documentSymbol", new { textDocument = new { uri } });

		await WaitUntilAsync(() => document.Waiting == 1);
		await client.NotifyAsync("textDocument/didChange", new
		{
			textDocument   = new { uri, version = 2 },
			contentChanges = new[] { new { text = text + "// edited\n" } },
		});

		Assert.True((await overtaken).TryGetProperty("result", out _));
		Assert.Equal(0, document.Waiting);

		hold.Set();
	}

	/// <summary>
	/// Several changes in one notification, as several cursors make them, carry the colours outside
	/// them over until the new version is analysed: a hover there still answers.
	/// </summary>
	[Fact]
	public async Task Changes_made_at_once_carry_the_colours_over()
	{
		var analyses = 0;
		var hold     = new ManualResetEventSlim(false);
		var options  = new ServerOptions
		{
			CSharp  = false,
			Analyze = (text, context, quiet, clock) =>
			{
				// The first analysis is the compiler's; the next, of the edited version, is held.
				if (Interlocked.Increment(ref analyses) > 1)
					hold.Wait(LspClient.Timeout);

				return GramAnalysisSession.Analyze(text, context, quiet, clock);
			},
		};

		await using var client = await LspClient.StartAsync(options);

		const string uri  = "untitled:Cursors.gram";
		var text = "Start = Word\nWord = ['a'..'z']+\nOther = Word\nparse Start\n";

		await client.OpenAsync(uri, text);
		await client.DiagnosticsAsync(uri, 1);

		// A space at the start of the first and of the second line, the second given in the text the first left.
		var first  = new { start = LspClient.Position(text, 0), end = LspClient.Position(text, 0) };
		var edited = " " + text;
		var second = new { start = LspClient.Position(edited, edited.IndexOf("Word =", StringComparison.Ordinal)), end = LspClient.Position(edited, edited.IndexOf("Word =", StringComparison.Ordinal)) };

		text = edited.Insert(edited.IndexOf("Word =", StringComparison.Ordinal), " ");

		await client.NotifyAsync("textDocument/didChange", new
		{
			textDocument   = new { uri, version = 2 },
			contentChanges = new[] { new { range = first, text = " " }, new { range = second, text = " " } },
		});

		var hover = await client.RequestAsync("textDocument/hover", LspClient.At(uri, text, text.IndexOf("Other", StringComparison.Ordinal)));

		Assert.Equal(JsonValueKind.Object, hover.ValueKind);
		Assert.Equal(text.IndexOf("Other", StringComparison.Ordinal), new LineMap(text).Offset(new Position(
			hover.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32(),
			hover.GetProperty("range").GetProperty("start").GetProperty("character").GetInt32())));

		hold.Set();
	}

	static async Task WaitUntilAsync(Func<bool> condition)
	{
		var deadline = DateTime.UtcNow + LspClient.Timeout;

		while (!condition())
		{
			Assert.True(DateTime.UtcNow < deadline, "The condition did not come true.");
			await Task.Delay(5, TestContext.Current.CancellationToken);
		}
	}

	/// <summary>A document was told about at least so many times, in the order of its versions, and last that it was closed.</summary>
	static void AssertToldInOrderAndClosed(LspClient client, string uri, int atLeast)
	{
		var published = client.Notifications("textDocument/publishDiagnostics")
			.Where(item => item.GetProperty("uri").GetString() == uri)
			.ToArray();
		var versions  = published
			.Where(static item => item.TryGetProperty("version", out var version) && version.ValueKind == JsonValueKind.Number)
			.Select(static item => item.GetProperty("version").GetInt32())
			.ToArray();

		Assert.True(versions.Length >= atLeast, $"{uri} was told about {versions.Length} times.");
		Assert.Equal(versions.Order(), versions);
		Assert.False(published[^1].TryGetProperty("version", out _), $"{uri}: something was told after its close.");
		Assert.Equal(0, published[^1].GetProperty("diagnostics").GetArrayLength());
	}

	/// <summary>The line map of an edited text, worked out from the one before, is the one read afresh.</summary>
	[Fact]
	public void An_edited_line_map_is_the_texts()
	{
		var random = new Random(20261031);
		var pieces = new[] { "a", "\r", "\n", "\r\n", "bc", "\n\r", "", "\u2028" };
		var text   = SourceText.From("one\r\ntwo\rthree\nfour");
		var lines  = new LineMap(text);

		for (var step = 0; step < 5000; step++)
		{
			var start    = random.Next(text.Length + 1);
			var end      = Math.Min(text.Length, start + random.Next(4));
			var inserted = string.Concat(Enumerable.Range(0, random.Next(3)).Select(_ => pieces[random.Next(pieces.Length)]));

			text  = SourceText.From(string.Concat(text.ToString().AsSpan(0, start), inserted, text.ToString().AsSpan(end)));
			lines = lines.Apply(text, start, end, inserted.Length);

			var fresh = new LineMap(text);

			Assert.Equal(fresh.LineCount, lines.LineCount);

			for (var line = 0; line < fresh.LineCount; line++)
				Assert.Equal(fresh.LineStart(line), lines.LineStart(line));
		}
	}
}
