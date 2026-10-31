using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace DotGram.LanguageServer;

/// <summary>What a request answers: a result written by the serializer, or an error.</summary>
readonly struct RpcReply
{
	RpcReply(Action<Utf8JsonWriter>? result, int errorCode, string? errorMessage)
	{
		Result       = result;
		ErrorCode    = errorCode;
		ErrorMessage = errorMessage;
	}

	public Action<Utf8JsonWriter>? Result { get; }

	public int ErrorCode { get; }

	public string? ErrorMessage { get; }

	public bool IsError => ErrorMessage is not null;

	public static RpcReply Null { get; } = new(static writer => writer.WriteNullValue(), 0, null);

	public static RpcReply Ok<T>(T? value, JsonTypeInfo<T> type)
	{
		if (value is null)
			return Null;

		return new RpcReply(writer => JsonSerializer.Serialize(writer, value, type), 0, null);
	}

	public static RpcReply Error(int code, string message)
	{
		return new RpcReply(null, code, message);
	}
}

/// <summary>What the connection asks of the server it carries.</summary>
interface IRpcTarget
{
	/// <summary>
	/// A notification, handled on the reading thread before the next message is read: the
	/// notifications about one document are applied in the order they were sent, and a request
	/// read after them sees what they did.
	/// </summary>
	void Notify(string method, JsonElement? parameters);

	/// <summary>
	/// A request, bound on the reading thread — to the documents as they are when it is read — and
	/// answered off it by the work this returns; null for a method the server does not have.
	/// </summary>
	Func<CancellationToken, Task<RpcReply>>? Bind(string method, JsonElement? parameters);
}

/// <summary>
/// JSON-RPC 2.0 over a pair of streams with the Language Server Protocol's base framing: a
/// <c>Content-Length</c> header, a blank line, and that many bytes of UTF-8 JSON.
/// </summary>
/// <remarks>
/// One loop reads the messages and never waits on a request: a request runs as a task of its own
/// with a token that <c>$/cancelRequest</c> cancels, so an edit or a cancellation sent while a
/// slow request is running is read and applied at once. <c>shutdown</c> is answered once every
/// request in flight has been; <c>exit</c> ends the loop.
/// </remarks>
sealed class JsonRpcConnection
{
	static readonly byte[] HeaderEnd = "\r\n\r\n"u8.ToArray();

	readonly Stream        _input;
	readonly Stream        _output;
	readonly SemaphoreSlim _writeGate = new(1, 1);
	readonly Action<string>? _trace;

	readonly ConcurrentDictionary<string, CancellationTokenSource> _pending = new(StringComparer.Ordinal);
	readonly ConcurrentDictionary<Task, bool>                      _running = new();

	int  _nextId;
	bool _shutdown;

	public JsonRpcConnection(Stream input, Stream output, Action<string>? trace = null)
	{
		_input  = input;
		_output = output;
		_trace  = trace;
	}

	/// <summary>Whether <c>shutdown</c> was asked for before <c>exit</c>: the process's exit code says so.</summary>
	public bool ShutdownRequested => Volatile.Read(ref _shutdown);

	/// <summary>Reads and dispatches messages until <c>exit</c> or the end of the input.</summary>
	public async Task RunAsync(IRpcTarget target, CancellationToken cancellationToken)
	{
		var reader = new FrameReader(_input);

		while (!cancellationToken.IsCancellationRequested)
		{
			var body = await reader.ReadAsync(cancellationToken).ConfigureAwait(false);

			if (body is null)
				break;

			_trace?.Invoke("<-- " + Encoding.UTF8.GetString(body));

			JsonDocument message;

			try
			{
				message = JsonDocument.Parse(body);
			}
			catch (JsonException exception)
			{
				await WriteErrorAsync(null, Lsp.ParseError, exception.Message).ConfigureAwait(false);
				continue;
			}

			using (message)
			{
				if (!Dispatch(target, message.RootElement))
					break;
			}
		}
	}

	/// <summary>Handles one message; false where it was <c>exit</c>.</summary>
	bool Dispatch(IRpcTarget target, JsonElement message)
	{
		if (message.ValueKind != JsonValueKind.Object)
		{
			_ = WriteErrorAsync(null, Lsp.InvalidRequest, "A message is a JSON object.");
			return true;
		}

		var hasId     = message.TryGetProperty("id", out var idElement);
		var id        = hasId ? idElement.GetRawText() : null;
		var hasMethod = message.TryGetProperty("method", out var methodElement) && methodElement.ValueKind == JsonValueKind.String;

		// A response to a request this server sent: nothing waits for one.
		if (!hasMethod)
			return true;

		var method     = methodElement.GetString()!;
		JsonElement? parameters = message.TryGetProperty("params", out var paramsElement) ? paramsElement.Clone() : null;

		if (!hasId)
		{
			switch (method)
			{
				case "exit":
					return false;

				case "$/cancelRequest":
					Cancel(parameters);
					return true;
			}

			try
			{
				target.Notify(method, parameters);
			}
			catch (Exception exception) when (exception is not OutOfMemoryException)
			{
				_ = LogAsync(Lsp.MessageError, $"{method}: {exception}");
			}

			return true;
		}

		if (method == "shutdown")
		{
			Volatile.Write(ref _shutdown, true);
			_ = ShutdownAsync(id!);
			return true;
		}

		if (ShutdownRequested)
		{
			_ = WriteErrorAsync(id, Lsp.InvalidRequest, "The server is shutting down.");
			return true;
		}

		Func<CancellationToken, Task<RpcReply>>? work;

		try
		{
			work = target.Bind(method, parameters);
		}
		catch (Exception exception) when (exception is JsonException or ArgumentException or InvalidOperationException)
		{
			_ = WriteErrorAsync(id, Lsp.InvalidParams, exception.Message);
			return true;
		}

		if (work is null)
		{
			_ = WriteErrorAsync(id, Lsp.MethodNotFound, $"The server has no method '{method}'.");
			return true;
		}

		var cancellation = new CancellationTokenSource();

		_pending[id!] = cancellation;
		Track(Task.Run(() => AnswerAsync(id!, method, work, cancellation)));

		return true;
	}

	void Track(Task task)
	{
		_running[task] = true;
		_ = task.ContinueWith(done => _running.TryRemove(done, out _), TaskScheduler.Default);
	}

	async Task AnswerAsync(string id, string method, Func<CancellationToken, Task<RpcReply>> work, CancellationTokenSource cancellation)
	{
		RpcReply reply;

		try
		{
			reply = await work(cancellation.Token).ConfigureAwait(false);
		}
		catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
		{
			reply = RpcReply.Error(Lsp.RequestCancelled, "The request was cancelled.");
		}
		catch (Exception exception) when (exception is not OutOfMemoryException)
		{
			_ = LogAsync(Lsp.MessageError, $"{method}: {exception}");
			reply = RpcReply.Error(Lsp.InternalError, exception.Message);
		}
		finally
		{
			_pending.TryRemove(id, out _);
		}

		// A cancelled request is answered as cancelled even where its work finished regardless.
		if (cancellation.IsCancellationRequested && !reply.IsError)
			reply = RpcReply.Error(Lsp.RequestCancelled, "The request was cancelled.");

		cancellation.Dispose();

		await WriteReplyAsync(id, reply).ConfigureAwait(false);
	}

	async Task ShutdownAsync(string id)
	{
		while (true)
		{
			var running = _running.Keys.Where(static task => !task.IsCompleted).ToArray();

			if (running.Length == 0)
				break;

			try
			{
				await Task.WhenAll(running).ConfigureAwait(false);
			}
			catch (Exception exception) when (exception is not OutOfMemoryException)
			{
				// Each request has answered its own failure.
			}
		}

		await WriteReplyAsync(id, RpcReply.Null).ConfigureAwait(false);
	}

	void Cancel(JsonElement? parameters)
	{
		if (parameters is { ValueKind: JsonValueKind.Object } value &&
			value.TryGetProperty("id", out var id) &&
			_pending.TryGetValue(id.GetRawText(), out var cancellation))
		{
			try
			{
				cancellation.Cancel();
			}
			catch (ObjectDisposedException)
			{
				// Answered meanwhile.
			}
		}
	}

	/// <summary>Sends a notification to the client.</summary>
	public Task NotifyAsync<T>(string method, T value, JsonTypeInfo<T> type)
	{
		return WriteAsync(writer =>
		{
			writer.WriteString("method", method);
			writer.WritePropertyName("params");
			JsonSerializer.Serialize(writer, value, type);
		});
	}

	/// <summary>Sends a request to the client whose answer nothing waits for, such as a refresh.</summary>
	public Task RequestAsync(string method)
	{
		var id = "dotgram-" + Interlocked.Increment(ref _nextId).ToString(CultureInfo.InvariantCulture);

		return WriteAsync(writer =>
		{
			writer.WriteString("id", id);
			writer.WriteString("method", method);
		});
	}

	public Task LogAsync(int type, string message)
	{
		return NotifyAsync("window/logMessage", new LogMessageParams(type, message), LspJson.Default.LogMessageParams);
	}

	Task WriteReplyAsync(string id, RpcReply reply)
	{
		return reply.IsError
			? WriteErrorAsync(id, reply.ErrorCode, reply.ErrorMessage!)
			: WriteAsync(writer =>
			{
				writer.WritePropertyName("id");
				writer.WriteRawValue(id);
				writer.WritePropertyName("result");
				reply.Result!(writer);
			});
	}

	Task WriteErrorAsync(string? id, int code, string message)
	{
		return WriteAsync(writer =>
		{
			writer.WritePropertyName("id");

			if (id is null)
				writer.WriteNullValue();
			else
				writer.WriteRawValue(id);

			writer.WriteStartObject("error");
			writer.WriteNumber("code", code);
			writer.WriteString("message", message);
			writer.WriteEndObject();
		});
	}

	async Task WriteAsync(Action<Utf8JsonWriter> body)
	{
		var buffer = new MemoryStream();

		using (var writer = new Utf8JsonWriter(buffer))
		{
			writer.WriteStartObject();
			writer.WriteString("jsonrpc", "2.0");
			body(writer);
			writer.WriteEndObject();
		}

		var header = Encoding.ASCII.GetBytes($"Content-Length: {buffer.Length.ToString(CultureInfo.InvariantCulture)}\r\n\r\n");

		await _writeGate.WaitAsync().ConfigureAwait(false);

		try
		{
			if (_trace is not null)
				_trace("--> " + Encoding.UTF8.GetString(buffer.GetBuffer(), 0, (int)buffer.Length));

			await _output.WriteAsync(header).ConfigureAwait(false);
			await _output.WriteAsync(buffer.GetBuffer().AsMemory(0, (int)buffer.Length)).ConfigureAwait(false);
			await _output.FlushAsync().ConfigureAwait(false);
		}
		catch (IOException)
		{
			// The client has gone; the reading loop ends on its own.
		}
		catch (ObjectDisposedException)
		{
		}
		finally
		{
			_writeGate.Release();
		}
	}

	/// <summary>Reads one framed message at a time.</summary>
	sealed class FrameReader(Stream input)
	{
		readonly byte[] _buffer = new byte[64 * 1024];
		int             _start;
		int             _end;

		/// <summary>The next message's body, or null at the end of the input.</summary>
		public async Task<byte[]?> ReadAsync(CancellationToken cancellationToken)
		{
			var header = await ReadHeaderAsync(cancellationToken).ConfigureAwait(false);

			if (header is null)
				return null;

			var length = -1;

			foreach (var line in header.Split("\r\n", StringSplitOptions.RemoveEmptyEntries))
			{
				var colon = line.IndexOf(':', StringComparison.Ordinal);

				if (colon > 0 &&
					line.AsSpan(0, colon).Trim().Equals("Content-Length", StringComparison.OrdinalIgnoreCase) &&
					int.TryParse(line.AsSpan(colon + 1).Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var parsed))
					length = parsed;
			}

			if (length < 0)
				throw new InvalidDataException("A message header without a Content-Length: " + header);

			var body = new byte[length];
			var read = 0;

			while (read < length)
			{
				if (_start == _end && !await FillAsync(cancellationToken).ConfigureAwait(false))
					return null;

				var count = Math.Min(length - read, _end - _start);

				Buffer.BlockCopy(_buffer, _start, body, read, count);
				_start += count;
				read   += count;
			}

			return body;
		}

		async Task<string?> ReadHeaderAsync(CancellationToken cancellationToken)
		{
			var header = new List<byte>();

			while (true)
			{
				if (_start == _end && !await FillAsync(cancellationToken).ConfigureAwait(false))
					return null;

				header.Add(_buffer[_start++]);

				if (header.Count >= HeaderEnd.Length && EndsWithHeaderEnd(header))
					return Encoding.ASCII.GetString([.. header]);
			}
		}

		static bool EndsWithHeaderEnd(List<byte> header)
		{
			for (var index = 0; index < HeaderEnd.Length; index++)
				if (header[header.Count - HeaderEnd.Length + index] != HeaderEnd[index])
					return false;

			return true;
		}

		async Task<bool> FillAsync(CancellationToken cancellationToken)
		{
			_start = 0;
			_end   = await input.ReadAsync(_buffer, cancellationToken).ConfigureAwait(false);

			return _end > 0;
		}
	}
}
