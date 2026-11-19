using System;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Xunit;
using Xunit.Sdk;

namespace DotGram.LanguageServer.Tests;

/// <summary>
/// The test client's waits: what fails one is the server having had nothing to do for a while, not the
/// time taken, which a busy machine — every test suite at once on a few processors — stretches.
/// </summary>
/// <remarks>
/// What is lost while the server has nothing to do fails a wait after the patience. What hangs in work
/// the server still holds on its scheduler fails it only after <see cref="LspClient.WorkLimit"/>.
/// </remarks>
public sealed class WaitTests
{
	static readonly TimeSpan Patience = TimeSpan.FromMilliseconds(200);

	const string Uri  = "untitled:Wait.gram";
	const string Text = "Start = 'a'\nparse Start\n";

	/// <summary>
	/// An analysis that takes many times the patience is waited for: the server works all along. The
	/// patience is the test's only for that wait: what comes before and after it — the messages on their
	/// way between the two sides, which a busy machine holds up for seconds — is waited for as every
	/// test waits.
	/// </summary>
	[Fact]
	public async Task A_wait_goes_on_while_the_server_works()
	{
		var options = new ServerOptions
		{
			CSharp  = false,
			Analyze = static (text, context, quiet, clock) =>
			{
				Thread.Sleep(Patience * 5);
				return GramAnalysisSession.Analyze(text, context, quiet, clock);
			},
		};

		await using var client = new LspClient(options);

		await client.InitializeAsync();
		await client.OpenAsync(Uri, Text);

		var session = client.Server.Document(Uri)!.Session;

		client.Patience = Patience;
		await client.UntilAsync(() => session.Result is not null, static () => "The analysis was not published.");
		client.Patience = LspClient.Timeout;
	}

	/// <summary>
	/// What never comes fails the wait once the server has had nothing to do for the patience, and the
	/// failure says how the server stands.
	/// </summary>
	[Fact]
	public async Task A_wait_for_what_never_comes_fails_once_the_server_has_nothing_to_do()
	{
		await using var client = new LspClient();

		await client.InitializeAsync();
		await client.OpenAsync(Uri, Text);
		await client.DiagnosticsAsync(Uri, 1);
		await client.SettleAsync();

		client.Patience = Patience;

		var failure = await Assert.ThrowsAsync<FailException>(() => client.DiagnosticsAsync(Uri, 2));

		client.Patience = LspClient.Timeout;

		Assert.Contains("No textDocument/publishDiagnostics came.", failure.Message, StringComparison.Ordinal);
		Assert.Contains("Its scheduler is idle", failure.Message, StringComparison.Ordinal);
	}
}
