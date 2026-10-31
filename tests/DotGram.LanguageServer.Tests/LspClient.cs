using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>
/// The server in this process, on a pair of pipes, and a client that talks to it as an editor does:
/// framed JSON-RPC, requests answered by id, notifications collected in the order they came.
/// </summary>
sealed class LspClient : IAsyncDisposable
{
	public static readonly JsonSerializerOptions Json = new()
	{
		PropertyNamingPolicy   = JsonNamingPolicy.CamelCase,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
	};

	public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(2);

	readonly AnonymousPipeServerStream _toServer   = new(PipeDirection.Out);
	readonly AnonymousPipeServerStream _fromServer = new(PipeDirection.In);
	readonly AnonymousPipeClientStream _serverIn;
	readonly AnonymousPipeClientStream _serverOut;
	readonly SemaphoreSlim             _writeGate = new(1, 1);
	readonly Task                      _serverRun;
	readonly Task                      _reader;

	readonly ConcurrentDictionary<int, TaskCompletionSource<JsonElement>> _requests = new();
	readonly List<(string Method, JsonElement Params)>                    _notifications = [];
	readonly object                                                         _gate = new();

	int _nextId;

	public LspClient(ServerOptions? options = null)
	{
		_serverIn  = new AnonymousPipeClientStream(PipeDirection.In, _toServer.ClientSafePipeHandle);
		_serverOut = new AnonymousPipeClientStream(PipeDirection.Out, _fromServer.ClientSafePipeHandle);

		Connection = new JsonRpcConnection(_serverIn, _serverOut);
		Server     = new GramLanguageServer(Connection, options ?? new ServerOptions { CSharp = false });
		_serverRun = Task.Run(() => Connection.RunAsync(Server, CancellationToken.None));
		_reader    = Task.Run(ReadAsync);
	}

	public JsonRpcConnection Connection { get; }

	public GramLanguageServer Server { get; }

	/// <summary>Starts a client and a server, and goes through <c>initialize</c> and <c>initialized</c>.</summary>
	public static async Task<LspClient> StartAsync(ServerOptions? options = null, params string[] folders)
	{
		var client = new LspClient(options);

		await client.InitializeAsync(folders);

		return client;
	}

	public async Task<JsonElement> InitializeAsync(params string[] folders)
	{
		var result = await RequestAsync("initialize", new
		{
			processId        = Environment.ProcessId,
			rootUri          = folders.Length == 0 ? null : Uri(folders[0]),
			workspaceFolders = folders.Select(folder => new { uri = Uri(folder), name = Path.GetFileName(folder) }).ToArray(),
			capabilities     = new { workspace = new { semanticTokens = new { refreshSupport = true } } },
		});

		await NotifyAsync("initialized", new { });

		return result;
	}

	public static string Uri(string path)
	{
		return new Uri(Path.GetFullPath(path)).AbsoluteUri;
	}

	/// <summary>A request's result; an error fails the test.</summary>
	public async Task<JsonElement> RequestAsync(string method, object? parameters)
	{
		var answer = await RequestRawAsync(method, parameters);

		if (answer.TryGetProperty("error", out var error))
			Assert.Fail($"{method} answered an error: {error}");

		return answer.GetProperty("result");
	}

	/// <summary>The whole response: <c>result</c> or <c>error</c>.</summary>
	public Task<JsonElement> RequestRawAsync(string method, object? parameters)
	{
		return RequestRawAsync(method, parameters, out _);
	}

	public Task<JsonElement> RequestRawAsync(string method, object? parameters, out int id)
	{
		id = Interlocked.Increment(ref _nextId);

		var done = new TaskCompletionSource<JsonElement>(TaskCreationOptions.RunContinuationsAsynchronously);

		_requests[id] = done;

		var message = new Dictionary<string, object?> { ["jsonrpc"] = "2.0", ["id"] = id, ["method"] = method };

		if (parameters is not null)
			message["params"] = parameters;

		var sent = WriteAsync(message);

		return AnswerAsync(sent, done.Task, method);
	}

	static async Task<JsonElement> AnswerAsync(Task sent, Task<JsonElement> answer, string method)
	{
		await sent;

		var finished = await Task.WhenAny(answer, Task.Delay(Timeout));

		Assert.True(finished == answer, $"{method} was not answered in {Timeout}.");

		return await answer;
	}

	public Task NotifyAsync(string method, object? parameters)
	{
		var message = new Dictionary<string, object?> { ["jsonrpc"] = "2.0", ["method"] = method };

		if (parameters is not null)
			message["params"] = parameters;

		return WriteAsync(message);
	}

	/// <summary>The notifications of a method received so far, in order.</summary>
	public IReadOnlyList<JsonElement> Notifications(string method)
	{
		lock (_gate)
			return _notifications.Where(item => item.Method == method).Select(static item => item.Params).ToArray();
	}

	/// <summary>Waits for a notification that satisfies a condition, counting those already received.</summary>
	public async Task<JsonElement> WaitForAsync(string method, Func<JsonElement, bool> condition)
	{
		var deadline = DateTime.UtcNow + Timeout;

		while (DateTime.UtcNow < deadline)
		{
			lock (_gate)
			{
				foreach (var (name, parameters) in _notifications)
					if (name == method && condition(parameters))
						return parameters;

				Monitor.Wait(_gate, 50);
			}

			await Task.Yield();
		}

		Assert.Fail($"No {method} came in {Timeout}.");
		return default;
	}

	/// <summary>The diagnostics of a document's version, once published.</summary>
	public Task<JsonElement> DiagnosticsAsync(string uri, int version)
	{
		return WaitForAsync("textDocument/publishDiagnostics", parameters =>
			parameters.GetProperty("uri").GetString() == uri &&
			parameters.TryGetProperty("version", out var published) && published.GetInt32() == version);
	}

	public Task OpenAsync(string uri, string text, int version = 1)
	{
		return NotifyAsync("textDocument/didOpen", new { textDocument = new { uri, languageId = "gram", version, text } });
	}

	/// <summary>A position as the protocol has it, of an offset in a text.</summary>
	public static object Position(string text, int offset)
	{
		var position = new LineMap(text).Position(offset);

		return new { line = position.Line, character = position.Character };
	}

	public static object At(string uri, string text, int offset)
	{
		return new { textDocument = new { uri }, position = Position(text, offset) };
	}

	async Task WriteAsync(object message)
	{
		var body   = JsonSerializer.SerializeToUtf8Bytes(message, Json);
		var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length.ToString(CultureInfo.InvariantCulture)}\r\n\r\n");

		await _writeGate.WaitAsync();

		try
		{
			await _toServer.WriteAsync(header);
			await _toServer.WriteAsync(body);
			await _toServer.FlushAsync();
		}
		finally
		{
			_writeGate.Release();
		}
	}

	async Task ReadAsync()
	{
		var buffer = new List<byte>();
		var chunk  = new byte[64 * 1024];
		var start  = 0;

		while (true)
		{
			var read = await _fromServer.ReadAsync(chunk);

			if (read == 0)
				return;

			buffer.AddRange(chunk.AsSpan(0, read));

			while (Frame(buffer, ref start) is { } body)
				Receive(body);

			if (start > 0)
			{
				buffer.RemoveRange(0, start);
				start = 0;
			}
		}
	}

	static byte[]? Frame(List<byte> buffer, ref int start)
	{
		for (var at = start; at + 3 < buffer.Count; at++)
		{
			if (buffer[at] != '\r' || buffer[at + 1] != '\n' || buffer[at + 2] != '\r' || buffer[at + 3] != '\n')
				continue;

			var header = Encoding.ASCII.GetString(buffer.GetRange(start, at - start).ToArray());
			var length = int.Parse(header.Split(':')[1].Trim(), CultureInfo.InvariantCulture);
			var body   = at + 4;

			if (buffer.Count < body + length)
				return null;

			start = body + length;

			return buffer.GetRange(body, length).ToArray();
		}

		return null;
	}

	void Receive(byte[] body)
	{
		var message = JsonDocument.Parse(body).RootElement.Clone();

		if (message.TryGetProperty("method", out var method))
		{
			// A request of the server's, such as a refresh, is answered at once; it is kept as a notification is.
			if (message.TryGetProperty("id", out var id))
				_ = WriteAsync(new Dictionary<string, object?> { ["jsonrpc"] = "2.0", ["id"] = id, ["result"] = null });

			lock (_gate)
			{
				_notifications.Add((method.GetString()!, message.TryGetProperty("params", out var parameters) ? parameters : default));
				Monitor.PulseAll(_gate);
			}

			return;
		}

		if (message.TryGetProperty("id", out var answered) &&
			answered.ValueKind == JsonValueKind.Number &&
			_requests.TryRemove(answered.GetInt32(), out var done))
			done.TrySetResult(message);
	}

	public async ValueTask DisposeAsync()
	{
		try
		{
			await RequestRawAsync("shutdown", null);
			await NotifyAsync("exit", null);
			await _serverRun.WaitAsync(Timeout);
		}
		finally
		{
			_toServer.Dispose();
			_serverIn.Dispose();
			_serverOut.Dispose();
			_fromServer.Dispose();

			await _reader.WaitAsync(Timeout).ContinueWith(static _ => { }, TaskScheduler.Default);
		}
	}

	/// <summary>The repository, for the grammars the tests read.</summary>
	public static string SourceRoot()
	{
		var declared = typeof(LspClient).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
			.FirstOrDefault(static one => one.Key == "DotGramSourceRoot")?.Value;

		if (declared is { Length: > 0 } && File.Exists(Path.Combine(declared, "DotGram.slnx")))
			return Path.GetFullPath(declared);

		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? throw new InvalidOperationException("The repository root is neither declared by the build nor above the test binaries.");
	}
}
