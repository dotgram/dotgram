using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Text.Json;
using System.Threading.Tasks;

using DotGram.Language;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>
/// A grammar's host found without a build: in the workspace folder's C# that declares grammars, read
/// as the generator reads it — its <c>Lexical</c>, what it includes, and a C# name in the grammar
/// going to its declaration.
/// </summary>
public sealed class HostTests : IDisposable
{
	readonly string _folder = Path.Combine(Path.GetTempPath(), "DotGram.LanguageServer.Tests", Guid.NewGuid().ToString("N"));

	public HostTests()
	{
		Directory.CreateDirectory(_folder);
	}

	public void Dispose()
	{
		Directory.Delete(_folder, recursive: true);
	}

	const string Hosts = """
		using DotGram;

		namespace Parsers;

		[Gram("Base.gram", IncludedAs = "B")]
		public partial class Base;

		[Gram("Missing.gram", IncludedAs = "C")]
		[GramSource("Letter = ['a'..'z']\n")]
		public partial class Carried;

		[Gram("Dialect.gram", Lexical = true)]
		[GramInclude(typeof(Base))]
		[GramInclude(typeof(Carried))]
		public partial class Dialect;
		""";

	const string Base    = "Word = ['a'..'z']+\n";
	const string Dialect = "trivia = [' ']*\nStart = B.Word & C.Letter\nparse Start\n";

	[Fact]
	public async Task The_playground_grammars_find_their_hosts_and_their_csharp()
	{
		var project = Path.Combine(LspClient.SourceRoot(), "tests", "DotGram.VisualStudio.Tests");
		var path    = Path.Combine(project, "Playground", "VisualStudioToolingPlayground.gram");
		var text    = File.ReadAllText(path);
		var uri     = LspClient.Uri(path);

		await using var client = await LspClient.StartAsync(new ServerOptions(), project);

		await client.OpenAsync(uri, text);

		var session = client.Server.Document(uri)!.Session;

		await ResolvedAsync(session);

		// F12 on a C# name in the grammar: the host's method, in the host's file.
		var raise      = text.IndexOf("Raise(", StringComparison.Ordinal);
		var definition = await client.RequestAsync("textDocument/definition", LspClient.At(uri, text, raise + 1));
		var target     = definition.EnumerateArray().Single();
		var host       = Path.Combine(project, "Playground", "VisualStudioToolingPlayground.gram.cs");

		Assert.Equal(LspClient.Uri(host), target.GetProperty("uri").GetString());
		Assert.Equal(
			File.ReadAllLines(host).Select((line, index) => (line, index)).First(static pair => pair.line.Contains("static decimal Raise(", StringComparison.Ordinal)).index,
			target.GetProperty("range").GetProperty("start").GetProperty("line").GetInt32());

		// Completion in the grammar's C#, from Roslyn: the members of a framework type.
		var math       = text.IndexOf("decimal.Parse", StringComparison.Ordinal);
		var completion = await client.RequestAsync("textDocument/completion", LspClient.At(uri, text, math + "decimal.".Length));

		Assert.Contains("Parse", completion.GetProperty("items").EnumerateArray().Select(static item => item.GetProperty("label").GetString()));

		// Quick Info there, from Roslyn too.
		var hover = await client.RequestAsync("textDocument/hover", LspClient.At(uri, text, math + "decimal.".Length + 1));

		Assert.Contains("```csharp", hover.GetProperty("contents").GetProperty("value").GetString(), StringComparison.Ordinal);
	}

	[Fact]
	public async Task A_host_in_the_folder_gives_its_options_and_what_it_includes()
	{
		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts);
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), Base);
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);

		var result  = await ResolvedAsync(client.Server.Document(uri)!.Session);
		var context = result.Resolution.Context!.Value;

		Assert.True(context.Lexical);
		Assert.Equal(["B", "C"], context.Included.Select(static item => item.Name));

		// Into the included grammar's file, and into the read-only copy of the one only a class carries.
		var word   = await client.RequestAsync("textDocument/definition", LspClient.At(uri, Dialect, Dialect.IndexOf("Word", StringComparison.Ordinal)));
		var letter = await client.RequestAsync("textDocument/definition", LspClient.At(uri, Dialect, Dialect.IndexOf("Letter", StringComparison.Ordinal)));

		Assert.Equal(LspClient.Uri(Path.Combine(_folder, "Base.gram")), word.EnumerateArray().Single().GetProperty("uri").GetString());

		var copy = FileUris.ToPath(letter.EnumerateArray().Single().GetProperty("uri").GetString())!;

		Assert.Equal("Letter = ['a'..'z']\n", File.ReadAllText(copy));
		Assert.True(TempDirectoryIncludedGrammarStore.Instance.IsCopy(copy));

		// References reach into the included grammar.
		var references = await client.RequestAsync("textDocument/references", new
		{
			textDocument = new { uri },
			position     = LspClient.Position(Dialect, Dialect.IndexOf("Word", StringComparison.Ordinal)),
			context      = new { includeDeclaration = true },
		});

		Assert.Contains(LspClient.Uri(Path.Combine(_folder, "Base.gram")), references.EnumerateArray().Select(static item => item.GetProperty("uri").GetString()));

		// A rule an included grammar declares is not renamed from here.
		var rename = await client.RequestRawAsync("textDocument/prepareRename", LspClient.At(uri, Dialect, Dialect.IndexOf("Word", StringComparison.Ordinal)));

		Assert.Equal(Lsp.RequestFailed, rename.GetProperty("error").GetProperty("code").GetInt32());
	}

	[Fact]
	public async Task A_host_written_after_the_grammar_is_opened_is_found()
	{
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), Base);
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);

		var session = client.Server.Document(uri)!.Session;

		await StateAsync(session, StandaloneGrammarState.NoHost);

		var hosts = Path.Combine(_folder, "Hosts.cs");

		File.WriteAllText(hosts, Hosts);

		await client.NotifyAsync("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = LspClient.Uri(hosts), type = Lsp.FileCreated } } });

		await ResolvedAsync(session);
	}

	[Fact]
	public async Task A_folder_past_the_cap_reads_its_grammars_as_they_stand()
	{
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);
		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts);

		for (var index = 0; index < FolderWorkspace.MaxSourceFiles; index++)
			File.WriteAllText(Path.Combine(_folder, $"File{index}.cs"), "class C {}");

		var path = Path.Combine(_folder, "Dialect.gram");
		var uri  = LspClient.Uri(path);

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);
		await client.DiagnosticsAsync(uri, 1);

		var log = await client.WaitForAsync("window/logMessage", static item =>
			item.GetProperty("message").GetString()!.Contains($"more than {FolderWorkspace.MaxSourceFiles}", StringComparison.Ordinal));

		Assert.Equal(1, log.GetProperty("message").GetString()!.Count(static character => character == '\n') + 1);
		Assert.True(client.Server.Document(uri)!.Session.Store.IsCopy(path));
	}

	/// <summary>
	/// An included grammar open in the editor is its editor's text for the grammars that include it,
	/// unsaved edits and all, and a change to its file on disk does not take that back.
	/// </summary>
	[Fact]
	public async Task An_open_included_grammar_is_its_editors_text()
	{
		const string extended = Base + "Extra = 'x'\n";
		const string uses     = "trivia = [' ']*\nStart = B.Word & B.Extra & C.Letter\nparse Start\n";

		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts);
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), Base);
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), uses);

		var basePath = Path.Combine(_folder, "Base.gram");
		var baseUri  = LspClient.Uri(basePath);
		var uri      = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		// Opened already edited: B.Extra is in the editor and not on disk.
		await client.OpenAsync(baseUri, extended);
		await client.OpenAsync(uri, uses);

		await ResolvedAsync(client.Server.Document(uri)!.Session);
		await NoErrorsAsync(client, uri);

		// The file changes on disk, back to what it was; the editor's text stays what B is.
		File.WriteAllText(basePath, Base);
		await client.NotifyAsync("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = baseUri, type = Lsp.FileChanged } } });
		await client.NotifyAsync("textDocument/didChange", new
		{
			textDocument   = new { uri, version = 2 },
			contentChanges = new[] { new { text = uses + "// edited\n" } },
		});

		await client.DiagnosticsAsync(uri, 2);
		await NoErrorsAsync(client, uri);
	}

	static async Task NoErrorsAsync(LspClient client, string uri)
	{
		var session = client.Server.Document(uri)!.Session;
		var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);

		// The host's resolution and the analyses it starts settle; then the last published is the truth.
		while (DateTime.UtcNow < deadline &&
			session.Result?.Document.Diagnostics.Any(static diagnostic => diagnostic.Severity == DotGram.Grammar.GramSeverity.Error) != false)
			await Task.Delay(50, TestContext.Current.CancellationToken);

		Assert.DoesNotContain(
			session.Result!.Document.Diagnostics,
			static diagnostic => diagnostic.Severity == DotGram.Grammar.GramSeverity.Error);
	}

	/// <summary>Links back up the folder are not followed: the walk ends, and the grammar is told about.</summary>
	[Fact]
	public async Task A_link_to_a_parent_folder_is_not_followed()
	{
		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts);
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), Base);
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		try
		{
			// Two, so that a walk that follows them meets every path through both: 2^n folders n deep.
			Directory.CreateSymbolicLink(Path.Combine(_folder, "loop"), _folder);
			Directory.CreateSymbolicLink(Path.Combine(_folder, "again"), _folder);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			Assert.Skip("This machine does not let a test make a link: " + exception.Message);
		}

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);
		await ResolvedAsync(client.Server.Document(uri)!.Session);
	}

	/// <summary>
	/// A workspace whose reading stalls — an enumeration of a network share that does not come back — is
	/// given up after its time, said so, and read as it stands; what the reading finds later is dropped.
	/// </summary>
	[Fact]
	public void A_reading_that_stalls_is_given_up()
	{
		string? said = null;

		var stalled = new ManualResetEventSlim(false);
		var read    = FolderWorkspace.Load(
			cancellationToken =>
			{
				stalled.Wait(LspClient.Timeout);
				return FolderWorkspace.Create([_folder], static _ => { }, cancellationToken);
			},
			message => said = message,
			TimeSpan.FromMilliseconds(300));

		Assert.False(read.Hosts);
		Assert.False(stalled.IsSet);
		Assert.Contains("took longer than", said, StringComparison.Ordinal);

		stalled.Set();
	}

	/// <summary>
	/// Two analyses of one version — before its host was found and after — are told in the order they
	/// were made: the last diagnostics the client has for the version are the host's.
	/// </summary>
	[Fact]
	public async Task Two_analyses_of_one_version_are_told_in_order()
	{
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), Base);
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);

		var session = client.Server.Document(uri)!.Session;

		await StateAsync(session, StandaloneGrammarState.NoHost);

		var hosts = Path.Combine(_folder, "Hosts.cs");

		File.WriteAllText(hosts, Hosts);
		await client.NotifyAsync("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = LspClient.Uri(hosts), type = Lsp.FileCreated } } });

		var resolved = await ResolvedAsync(session);

		await client.SettleAsync();

		var told = client.Notifications("textDocument/publishDiagnostics")
			.Where(item => item.GetProperty("uri").GetString() == uri)
			.ToArray();

		Assert.True(told.Length >= 2, $"The version was told about {told.Length} times.");
		Assert.True(told.All(static item => item.GetProperty("version").GetInt32() == 1));
		Assert.Equal(
			resolved.Document.Diagnostics.Select(static diagnostic => diagnostic.Id),
			told[^1].GetProperty("diagnostics").EnumerateArray().Select(static diagnostic => diagnostic.GetProperty("code").GetString()));
		Assert.NotEqual(
			told[0].GetProperty("diagnostics").GetArrayLength(),
			told[^1].GetProperty("diagnostics").GetArrayLength());
	}

	/// <summary>A reading given up reads no more files: it stops at the next one.</summary>
	[Fact]
	public void A_reading_given_up_reads_no_more()
	{
		for (var index = 0; index < 3; index++)
			File.WriteAllText(Path.Combine(_folder, $"G{index}.gram"), Base);

		using var cancellation = new CancellationTokenSource();
		var read = 0;

		Assert.Throws<OperationCanceledException>(() => FolderWorkspace.Create(
			[_folder],
			static _ => { },
			cancellation.Token,
			read: path =>
			{
				read++;
				cancellation.Cancel();
				return File.ReadAllText(path);
			}));

		Assert.Equal(1, read);
	}

	/// <summary>
	/// A watched file whose read never ends — a pipe here, a share that does not answer elsewhere —
	/// holds up neither the other updates nor the texts behind them: a hundred edits of an open grammar
	/// and its close leave none of its texts alive.
	/// </summary>
	[Fact]
	public async Task A_read_that_stalls_holds_no_edited_text()
	{
		Assert.SkipWhen(OperatingSystem.IsWindows(), "A file whose read blocks is a named pipe, made with mkfifo.");

		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts);
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), Base);

		var basePath = Path.Combine(_folder, "Base.gram");
		var uri      = LspClient.Uri(basePath);
		var pipe     = Path.Combine(_folder, "Stalled.cs");

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.Server.Folder.WaitAsync(LspClient.Timeout, TestContext.Current.CancellationToken);

		using (var mkfifo = System.Diagnostics.Process.Start("mkfifo", pipe))
			await mkfifo.WaitForExitAsync(TestContext.Current.CancellationToken);

		try
		{
			await client.NotifyAsync("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = LspClient.Uri(pipe), type = Lsp.FileCreated } } });
			await client.OpenAsync(uri, Base);

			var texts = await EditAsync(client, uri, 100);

			await client.NotifyAsync("textDocument/didClose", new { textDocument = new { uri } });
			await client.SettleAsync();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			var alive = texts.Count(static text => text.TryGetTarget(out _));

			Assert.True(alive <= 1, $"{alive} of 100 edited texts are still alive.");
		}
		finally
		{
			// The read is let go: a writer that opens the pipe and closes it ends it.
			await File.WriteAllTextAsync(pipe, "", TestContext.Current.CancellationToken);
		}
	}

	/// <summary>Edits an open document so many times and returns its texts, held weakly.</summary>
	static async Task<List<WeakReference<Microsoft.CodeAnalysis.Text.SourceText>>> EditAsync(LspClient client, string uri, int count)
	{
		var texts = new List<WeakReference<Microsoft.CodeAnalysis.Text.SourceText>>();

		for (var version = 2; version < 2 + count; version++)
		{
			await client.NotifyAsync("textDocument/didChange", new
			{
				textDocument   = new { uri, version },
				contentChanges = new[] { new { range = new { start = new { line = 0, character = 0 }, end = new { line = 0, character = 0 } }, text = " " } },
			});

			while (client.Server.Document(uri)?.Current.Number != version)
				await Task.Delay(1, TestContext.Current.CancellationToken);

			texts.Add(new WeakReference<Microsoft.CodeAnalysis.Text.SourceText>(client.Server.Document(uri)!.Current.Text));
		}

		return texts;
	}

	/// <summary>A folder that holds more entries than the walk looks at is given up, and said so.</summary>
	[Fact]
	public void A_walk_past_its_bound_gives_the_folders_up()
	{
		for (var index = 0; index < 10; index++)
			File.WriteAllText(Path.Combine(_folder, $"File{index}.txt"), "");

		string? said = null;

		var workspace = FolderWorkspace.Create([_folder], message => said = message, TestContext.Current.CancellationToken, maxEntries: 5);

		Assert.False(workspace.Hosts);
		Assert.Contains("more than 5 files and folders", said, StringComparison.Ordinal);
	}

	/// <summary>Two C# files whose names differ in case are two files where the file system says so.</summary>
	[Fact]
	public async Task Files_whose_names_differ_in_case_are_two_on_a_case_sensitive_system()
	{
		Assert.SkipWhen(OperatingSystem.IsWindows() || OperatingSystem.IsMacOS(), "The file system ignores case.");

		var upper = Path.Combine(_folder, "Hosts.cs");
		var lower = Path.Combine(_folder, "hosts.cs");

		File.WriteAllText(upper, Hosts);

		var workspace = FolderWorkspace.Create([_folder], static _ => { }, TestContext.Current.CancellationToken);

		File.WriteAllText(lower, "[DotGram.Gram(\"Other.gram\")] partial class Other;");
		workspace.FileChanged(lower, deleted: false);

		var documents = workspace.Workspace.CurrentSolution.GetProject(workspace.Project)!.Documents
			.Where(static document => document.FilePath is not null)
			.ToDictionary(static document => document.FilePath!, StringComparer.Ordinal);

		Assert.Equal(Hosts, (await documents[upper].GetTextAsync(TestContext.Current.CancellationToken)).ToString());
		Assert.True(documents.ContainsKey(lower));
	}

	/// <summary>
	/// Go To Definition into an included grammar lands where the protocol counts the line: a U+2028
	/// in a comment there breaks a line for Roslyn and not for the client.
	/// </summary>
	[Fact]
	public async Task A_definition_in_an_included_grammar_is_on_the_clients_line()
	{
		const string withSeparator = "// one\u2028two\nWord = ['a'..'z']+\n";

		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts);
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), withSeparator);
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);
		await ResolvedAsync(client.Server.Document(uri)!.Session);

		var definition = await client.RequestAsync("textDocument/definition", LspClient.At(uri, Dialect, Dialect.IndexOf("Word", StringComparison.Ordinal)));
		var start      = definition.EnumerateArray().Single().GetProperty("range").GetProperty("start");

		Assert.Equal(1, start.GetProperty("line").GetInt32());
		Assert.Equal(0, start.GetProperty("character").GetInt32());
	}

	/// <summary>
	/// A grammar closed before the server started it — the folders were still being read — is never
	/// started: nothing is analysed, followed or told of it.
	/// </summary>
	[Fact]
	public async Task A_grammar_closed_before_it_starts_is_never_started()
	{
		await using var client = await LspClient.StartAsync();

		var workspace = new Microsoft.CodeAnalysis.AdhocWorkspace();
		var document  = new OpenGrammar(
			"untitled:Closed.gram", null, new GramVersion(1, Microsoft.CodeAnalysis.Text.SourceText.From(Dialect)), client.Server);
		var told      = 0;

		document.Close(() => told++);

		Assert.False(document.Start(workspace));
		Assert.Equal(1, told);

		await client.SettleAsync();

		Assert.Null(document.Session.Result);
	}

	[Fact]
	public void A_file_is_in_the_project_for_what_its_syntax_declares()
	{
		Assert.True(FolderWorkspace.Declares("[DotGram.Gram(\"A.gram\")] partial class A;"));
		Assert.True(FolderWorkspace.Declares("using DotGram; [GramAttribute] partial class A;"));
		Assert.True(FolderWorkspace.Declares("[GramInclude(typeof(B))] partial class A;"));
		Assert.True(FolderWorkspace.Declares("[global::DotGram.GramSource(\"x\")] class A;"));

		// A mention of the name is not a declaration.
		Assert.False(FolderWorkspace.Declares("// [Gram(\"A.gram\")]\nclass A { string s = \"[Gram(\\\"A.gram\\\")]\"; }"));
		Assert.False(FolderWorkspace.Declares("[Grammar] class A;"));
	}

	static async Task<GramAnalysisResult<GramVersion>> ResolvedAsync(GramAnalysisSession<GramVersion> session)
	{
		return await StateAsync(session, StandaloneGrammarState.Resolved);
	}

	static async Task<GramAnalysisResult<GramVersion>> StateAsync(GramAnalysisSession<GramVersion> session, StandaloneGrammarState state)
	{
		var deadline = DateTime.UtcNow + LspClient.Timeout;

		while (DateTime.UtcNow < deadline)
		{
			if (session.Result is { } result && result.Resolution.State == state)
				return result;

			await Task.Delay(50, TestContext.Current.CancellationToken);
		}

		Assert.Fail($"The host is {session.Resolution.State}, not {state}.");
		return null;
	}
}
