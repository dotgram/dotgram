using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DotGram.LanguageServer;

/// <summary>
/// <c>dotnet DotGram.LanguageServer.dll</c>: the language server on standard input and output.
/// <c>--version</c> prints the version; <c>--log &lt;path&gt;</c> writes every message to a file.
/// </summary>
static class Program
{
	static async Task<int> Main(string[] args)
	{
		string? log = null;

		for (var index = 0; index < args.Length; index++)
		{
			switch (args[index])
			{
				case "--version":
					Console.WriteLine(GramLanguageServer.Version);
					return 0;

				case "--log" when index + 1 < args.Length:
					log = args[++index];
					break;

				// What a client passes to say which transport it speaks; this one speaks only stdio.
				case "--stdio":
					break;

				default:
					await Console.Error.WriteLineAsync($"Unknown argument '{args[index]}'. Usage: DotGram.LanguageServer [--version] [--log <path>]").ConfigureAwait(false);
					return 2;
			}
		}

		var input  = Console.OpenStandardInput();
		var output = Console.OpenStandardOutput();

		// Standard output is the protocol's: anything else written to it would break the stream.
		Console.SetOut(Console.Error);

		using var trace = log is null ? null : new StreamWriter(log, append: true) { AutoFlush = true };
		var gate        = new object();

		Action<string>? write = trace is null
			? null
			: line =>
			{
				lock (gate)
					trace.WriteLine($"{DateTime.Now:HH:mm:ss.fff} {line}");
			};

		var connection = new JsonRpcConnection(input, output, write);
		var server     = new GramLanguageServer(connection, new ServerOptions());

		await connection.RunAsync(server, CancellationToken.None).ConfigureAwait(false);

		return connection.ShutdownRequested ? 0 : 1;
	}
}
