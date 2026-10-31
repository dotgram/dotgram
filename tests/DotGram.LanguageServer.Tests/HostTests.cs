using System;
using System.IO;
using System.Linq;
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

		var copy = GramLanguageServer.LocalPath(letter.EnumerateArray().Single().GetProperty("uri").GetString())!;

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
