using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>What the references of a rule in an included grammar cost, and how often that grammar is opened for them.</summary>
public sealed class IncludedReferencesTests : IDisposable
{
	readonly string _folder = Path.Combine(Path.GetTempPath(), "DotGram.LanguageServer.Tests", Guid.NewGuid().ToString("N"));

	public IncludedReferencesTests()
	{
		Directory.CreateDirectory(_folder);
	}

	public void Dispose()
	{
		Directory.Delete(_folder, recursive: true);
	}

	/// <summary>
	/// The references of a rule used three hundred times in T-SQL's grammar, which a dialect includes:
	/// mapped onto the client's lines once per grammar, not once per reference.
	/// </summary>
	[Fact]
	public async Task The_references_into_a_large_included_grammar_open_it_once_a_request()
	{
		const string dialect = "Start = T.TSqlIdentifier\nparse Start\n";

		File.Copy(Path.Combine(LspClient.SourceRoot(), "src", "DotGram.Sql", "TransactSql", "TransactSql.gram"), Path.Combine(_folder, "TransactSql.gram"));
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), dialect);
		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), """
			using DotGram;

			namespace Parsers;

			[Gram("TransactSql.gram", DefaultAlias = "T", Lexical = true)]
			public partial class TSql;

			[Gram("Dialect.gram", Lexical = true)]
			[GramInclude(typeof(TSql))]
			public partial class Dialect;
			""");

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		var store = new CountingStore();
		var work  = new ServerWork();

		// Counted for the server started now: its work flows from here.
		ServerWork.Current = work;

		await using var client = await LspClient.StartAsync(new ServerOptions { Store = store }, _folder);

		ServerWork.Current = null;

		await client.OpenAsync(uri, dialect);
		await HostTests.ResolvedAsync(client, client.Server.Document(uri)!.Session);

		var at      = LspClient.Position(dialect, dialect.IndexOf("TSqlIdentifier", StringComparison.Ordinal));
		var request = new { textDocument = new { uri }, position = at, context = new { includeDeclaration = true } };

		// Once to warm what the first request pays for anyway: the core's search of the included grammar.
		var asked      = store.Asked;
		var references = await client.RequestAsync("textDocument/references", request);

		Assert.True(references.GetArrayLength() > 300, $"{references.GetArrayLength()} references.");

		// The one included grammar is asked where it opens once, for all of its references.
		Assert.Equal(1, store.Asked - asked);

		await client.SettleAsync();

		var clock = Stopwatch.StartNew();
		var read  = work.LineReads;

		asked = store.Asked;

		for (var repeat = 0; repeat < 5; repeat++)
			await client.RequestAsync("textDocument/references", request);

		var each = clock.Elapsed.TotalMilliseconds / 5;

		// The work, counted rather than timed: each request asks the store where the included grammar
		// opens once, and reads it into its lines once, for all of its references. A version that did
		// either for every reference did it three hundred times a request, and took 92 ms a request on
		// the T-SQL grammar where once a request takes 2 ms. The time is held only to a bound no machine
		// that runs the test misses.
		Assert.Equal(5, store.Asked - asked);
		Assert.Equal(5, work.LineReads - read);
		Assert.True(each < 5_000, $"A request took {each:0} ms.");
	}

	/// <summary>A cancelled request maps none of a rule's uses in the document itself, however many there are.</summary>
	[Fact]
	public void A_cancelled_request_maps_no_local_reference()
	{
		var text       = string.Concat(Enumerable.Repeat("Word ", 300_000));
		var positions  = Enumerable.Range(0, 300_000).Select(static index => index * 5).ToArray();
		var found      = new GramReferences("Word", 0, positions);
		var lines      = new LineMap(text);

		using var cancelled = new CancellationTokenSource();

		cancelled.Cancel();

		Assert.Throws<OperationCanceledException>(() =>
			NavigationMap.References(found, includeDeclaration: true, "untitled:A.gram", lines, new CountingStore(), cancelled.Token));
	}

	/// <summary>The temporary directory's store, counting how often it is asked where a grammar opens.</summary>
	sealed class CountingStore : IIncludedGrammarStore
	{
		int _asked;

		public int Asked => Volatile.Read(ref _asked);

		public string? PathToOpen(StandaloneIncludedGrammar grammar)
		{
			Interlocked.Increment(ref _asked);
			return TempDirectoryIncludedGrammarStore.Instance.PathToOpen(grammar);
		}

		public bool IsCopy(string filePath)
		{
			return TempDirectoryIncludedGrammarStore.Instance.IsCopy(filePath);
		}
	}
}
