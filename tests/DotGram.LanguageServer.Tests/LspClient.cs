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

	/// <summary>
	/// How long a wait goes on while the server has nothing to do: what is waited for did not come, and
	/// nothing that could bring it is running. Also the bound of a test's own gates.
	/// </summary>
	public static readonly TimeSpan Timeout = TimeSpan.FromMinutes(2);

	/// <summary>
	/// How long a wait goes on, at most, while the server works. A busy machine — every test suite at once
	/// on a few processors — stretches an analysis of the T-SQL grammar past <see cref="Timeout"/>, and
	/// such a wait says nothing about the server; this bounds only work that never ends.
	/// </summary>
	public static readonly TimeSpan WorkLimit = TimeSpan.FromMinutes(20);

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

	// Completed and replaced whenever a message comes in: what a wait wakes on, instead of its next look.
	TaskCompletionSource _arrived = new(TaskCreationOptions.RunContinuationsAsynchronously);

	int _nextId;

	public LspClient(ServerOptions? options = null)
	{
		_serverIn  = new AnonymousPipeClientStream(PipeDirection.In, _toServer.ClientSafePipeHandle);
		_serverOut = new AnonymousPipeClientStream(PipeDirection.Out, _fromServer.ClientSafePipeHandle);

		Connection = new JsonRpcConnection(_serverIn, _serverOut);
		Server     = new GramLanguageServer(Connection, (options ?? new ServerOptions { CSharp = false }) with { Scheduler = Scheduler });
		_serverRun = Task.Run(() => Connection.RunAsync(Server, CancellationToken.None));
		_reader    = Task.Run(ReadAsync);
	}

	public JsonRpcConnection Connection { get; }

	/// <summary>The clock and threads the server's analyses run on, which say when nothing is running.</summary>
	public TrackingScheduler Scheduler { get; } = new();

	public GramLanguageServer Server { get; }

	/// <summary>
	/// Whether the server is working now: an analysis or a search for a host running or due on its
	/// scheduler, the folders being read, or the workspace's updates being applied. A wait of a second
	/// or more on its clock — for a host to settle, before a retry — is not work.
	/// </summary>
	public bool Working => !Scheduler.Idle || !Server.Folder.IsCompleted || !Server.Updates.Drained.IsCompleted;

	/// <summary>Whether the client says the workspace is trusted, which lets the server evaluate its projects with MSBuild.</summary>
	public bool Trusted { get; init; } = true;

	/// <summary>How long a wait goes on while the server has nothing to do: <see cref="Timeout"/>, or shorter where a test asks.</summary>
	public TimeSpan Patience { get; init; } = Timeout;

	/// <summary>The whole of what <c>initialize</c> says of the extension's settings, where not only whether the workspace is trusted.</summary>
	public object? Options { get; init; }

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
			initializationOptions = Options ?? new { trusted = Trusted },
		});

		await NotifyAsync("initialized", new { });

		return result;
	}

	/// <summary>
	/// A path as VS Code writes it as a URI, written here independently of the server: on Windows the
	/// drive letter lower-cased and its colon escaped, <c>file:///c%3A/Users/a.gram</c>, and every
	/// character but the unreserved ones and the slashes escaped.
	/// </summary>
	public static string Uri(string path)
	{
		var full   = Path.GetFullPath(path).Replace('\\', '/');
		var result = new StringBuilder("file://");
		var at     = 0;

		if (full.StartsWith("//", StringComparison.Ordinal))
		{
			var slash = full.IndexOf('/', 2);

			result.Append(full, 2, slash - 2);
			at = slash;
		}
		else if (full.Length >= 2 && char.IsAsciiLetter(full[0]) && full[1] == ':')
		{
			result.Append('/').Append(char.ToLowerInvariant(full[0])).Append("%3A");
			at = 2;
		}

		foreach (var character in Encoding.UTF8.GetBytes(full.Substring(at)))
		{
			if (char.IsAsciiLetterOrDigit((char)character) || character is (byte)'-' or (byte)'.' or (byte)'_' or (byte)'~' or (byte)'/')
				result.Append((char)character);
			else
				result.Append('%').Append(character.ToString("X2", CultureInfo.InvariantCulture));
		}

		return result.ToString();
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

	async Task<JsonElement> AnswerAsync(Task sent, Task<JsonElement> answer, string method)
	{
		await sent;
		await UntilAsync(() => answer.IsCompleted, () => $"{method} was not answered.");

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
		JsonElement found = default;

		await UntilAsync(
			() =>
			{
				lock (_gate)
				{
					foreach (var (name, parameters) in _notifications)
					{
						if (name == method && condition(parameters))
						{
							found = parameters;
							return true;
						}
					}
				}

				return false;
			},
			() => $"No {method} came.");

		return found;
	}

	/// <summary>
	/// Waits until a condition holds, looking again whenever a message comes in and at least every few
	/// milliseconds. What fails the wait is the time the server had nothing to do while it waited —
	/// <see cref="Patience"/> of it — not the time taken: a busy machine slows the server's work down,
	/// and does not lose what it sends. Work that goes on for <see cref="WorkLimit"/> fails it as well.
	/// </summary>
	/// <param name="failure">What did not come, said in the failure with how the server stands.</param>
	public async Task UntilAsync(Func<bool> condition, Func<string> failure)
	{
		var waited = System.Diagnostics.Stopwatch.StartNew();
		var idle   = TimeSpan.Zero;
		var last   = TimeSpan.Zero;

		while (true)
		{
			// Taken before the look, so that a message that comes after it wakes the wait.
			Task arrived;

			lock (_gate)
				arrived = _arrived.Task;

			if (condition())
				return;

			var now = waited.Elapsed;

			if (!Working)
				idle += now - last;

			last = now;

			if (idle >= Patience || now >= WorkLimit)
				Assert.Fail($"{failure()} The server had nothing to do for {idle.TotalSeconds:F1} s of the {now.TotalSeconds:F1} s waited. {Standing()}");

			await Task.WhenAny(arrived, Task.Delay(20));
		}
	}

	/// <summary>How the server stands, and the last of what it logged: what a failed wait says beside what did not come.</summary>
	string Standing()
	{
		var folder = Server.Folder;
		var logged = Notifications("window/logMessage")
			.Select(static item => item.GetProperty("message").GetString())
			.TakeLast(10)
			.ToArray();

		return
			$"Its scheduler is {(Scheduler.Idle ? "idle" : "busy")}, the folders are {(folder.IsCompleted ? "read" : "being read")}, " +
			$"its updates are {(Server.Updates.Drained.IsCompleted ? "applied" : "being applied")}, {Server.Updates.Reading} watched files are being read. " +
			(logged.Length == 0 ? "It logged nothing." : "It logged:" + string.Concat(logged.Select(static line => Environment.NewLine + "  " + line)));
	}

	/// <summary>
	/// Waits until the server has nothing left to do — every message sent before handled, the folders
	/// read, the workspace's updates applied, no analysis or search for a host running or due, no
	/// document waiting to start — and until everything it sent meanwhile has arrived: a barrier, where
	/// a test would otherwise wait for a while and hope.
	/// </summary>
	/// <remarks>
	/// A read of a watched file still running is not waited for: it may never end, which is what one
	/// test is about. Everything else that does work runs on the server's scheduler or inside an update.
	/// </remarks>
	public async Task SettleAsync()
	{
		// The incoming side first: a request is bound after every notification sent before it has been
		// handled, so what those set going has started by the time it is answered.
		await FenceAsync();

		while (true)
		{
			await UntilAsync(() => !Working, static () => "The server did not settle.");

			// The outgoing side: answered after everything the server told before it. Then once more
			// nothing may have started meanwhile, or the round goes again.
			await FenceAsync();

			if (!Working)
				return;
		}
	}

	Task FenceAsync()
	{
		return RequestRawAsync("textDocument/documentSymbol", new { textDocument = new { uri = "untitled:barrier" } });
	}

	/// <summary>A message as the protocol frames it.</summary>
	public static byte[] Frame(object message)
	{
		var body   = JsonSerializer.SerializeToUtf8Bytes(message, Json);
		var header = Encoding.ASCII.GetBytes($"Content-Length: {body.Length.ToString(CultureInfo.InvariantCulture)}\r\n\r\n");

		return [.. header, .. body];
	}

	/// <summary>The diagnostics of a document's version, once published.</summary>
	public Task<JsonElement> DiagnosticsAsync(string uri, int version)
	{
		return WaitForAsync("textDocument/publishDiagnostics", parameters =>
			parameters.GetProperty("uri").GetString() == uri &&
			parameters.TryGetProperty("version", out var published) && published.GetInt32() == version);
	}

	/// <summary>Opens a document, and returns once the server holds it.</summary>
	public async Task OpenAsync(string uri, string text, int version = 1, bool waitForServer = true)
	{
		await NotifyAsync("textDocument/didOpen", new { textDocument = new { uri, languageId = "gram", version, text } });

		if (!waitForServer)
			return;

		await UntilAsync(() => Server.Document(uri) is not null, () => $"{uri} was not opened.");
	}

	/// <summary>Opens a C# document, and returns once the server holds it.</summary>
	public async Task OpenCSharpAsync(string uri, string text, int version = 1)
	{
		await NotifyAsync("textDocument/didOpen", new { textDocument = new { uri, languageId = "csharp", version, text } });
		await UntilAsync(() => Server.CSharpDocument(uri) is not null, () => $"{uri} was not opened.");
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
				_notifications.Add((method.GetString()!, message.TryGetProperty("params", out var parameters) ? parameters : default));
		}
		else if (message.TryGetProperty("id", out var answered) &&
			answered.ValueKind == JsonValueKind.Number &&
			_requests.TryRemove(answered.GetInt32(), out var done))
			done.TrySetResult(message);

		TaskCompletionSource arrived;

		lock (_gate)
		{
			arrived  = _arrived;
			_arrived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		}

		arrived.TrySetResult();
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

/// <summary>
/// The machine's clock and thread pool, counting what runs on them: work is running from its start to
/// its end, except while it waits a second or more — for a host to settle, or before a retry — which
/// is waiting, not work, and would hold a barrier for its whole length.
/// </summary>
sealed class TrackingScheduler : DotGram.Language.IGramScheduler
{
	readonly System.Diagnostics.Stopwatch _clock = System.Diagnostics.Stopwatch.StartNew();
	int                                   _busy;

	public bool Idle => Volatile.Read(ref _busy) == 0;

	public long Now => _clock.ElapsedMilliseconds;

	/// <summary>Every wait asked for, in order: what a test counts instead of timing.</summary>
	public System.Collections.Concurrent.ConcurrentQueue<int> Delays { get; } = new();

	public async Task DelayAsync(int milliseconds, CancellationToken cancellationToken)
	{
		Delays.Enqueue(milliseconds);

		if (milliseconds < 1000)
		{
			await Task.Delay(milliseconds, cancellationToken);
			return;
		}

		Interlocked.Decrement(ref _busy);

		try
		{
			await Task.Delay(milliseconds, cancellationToken);
		}
		finally
		{
			Interlocked.Increment(ref _busy);
		}
	}

	public void Run(Func<Task> work)
	{
		Interlocked.Increment(ref _busy);

		_ = Task.Run(async () =>
		{
			try
			{
				await work();
			}
			finally
			{
				Interlocked.Decrement(ref _busy);
			}
		});
	}
}
