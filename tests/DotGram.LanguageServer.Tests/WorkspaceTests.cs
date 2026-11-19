using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>
/// The workspace a grammar's host is looked for in, as the build would see it: one project per
/// <c>.csproj</c>, every C# file of a project with its global usings and its base classes, and the
/// folders the client has now.
/// </summary>
public sealed class WorkspaceTests : IDisposable
{
	readonly string _folder = Path.Combine(Path.GetTempPath(), "DotGram.LanguageServer.Tests", Guid.NewGuid().ToString("N"));

	public WorkspaceTests()
	{
		Directory.CreateDirectory(_folder);
	}

	public void Dispose()
	{
		Directory.Delete(_folder, recursive: true);
	}

	// The grammars given to the build as the guide has them; a project that does not list a grammar does
	// not compile it, and the server, which evaluates the project, does not either.
	const string Project = "<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>\n    <TargetFramework>net10.0</TargetFramework>\n  </PropertyGroup>\n  <ItemGroup>\n    <AdditionalFiles Include=\"**/*.gram\" />\n  </ItemGroup>\n</Project>\n";
	const string Dialect = "trivia = [' ']*\nStart = B.Word\nparse Start\n";

	static string Hosts(bool lexical)
	{
		return $$"""
			using DotGram;

			namespace Parsers;

			[Gram("Base.gram", DefaultAlias = "B")]
			public partial class Base;

			[Gram("Dialect.gram", Lexical = {{(lexical ? "true" : "false")}})]
			[GramInclude(typeof(Base))]
			public partial class Dialect;
			""";
	}

	/// <summary>
	/// Two projects in one folder, each with a host of the same name for a grammar of the same name:
	/// each grammar is read with its own project's host and includes its own project's file.
	/// </summary>
	[Fact]
	public async Task Each_project_reads_its_grammar_with_its_own_host()
	{
		foreach (var (name, lexical) in new[] { ("A", true), ("B", false) })
		{
			var directory = Directory.CreateDirectory(Path.Combine(_folder, name)).FullName;

			File.WriteAllText(Path.Combine(directory, name + ".csproj"), Project);
			File.WriteAllText(Path.Combine(directory, "Hosts.cs"), Hosts(lexical));
			File.WriteAllText(Path.Combine(directory, "Base.gram"), "Word = ['a'..'z']+\n");
			File.WriteAllText(Path.Combine(directory, "Dialect.gram"), Dialect);
		}

		await using var client = await LspClient.StartAsync(null, _folder);

		foreach (var (name, lexical) in new[] { ("A", true), ("B", false) })
		{
			var uri = LspClient.Uri(Path.Combine(_folder, name, "Dialect.gram"));

			await client.OpenAsync(uri, Dialect);

			var context = (await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session)).Resolution.Context!.Value;

			Assert.Equal(lexical, context.Lexical);
			Assert.Equal(Path.Combine(_folder, name, "Base.gram"), context.Included.Single().FilePath);
		}
	}

	/// <summary>A host that names <c>[Gram]</c> through a project's <c>global using DotGram;</c> is found.</summary>
	[Fact]
	public async Task A_host_is_read_with_its_projects_global_usings()
	{
		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project);
		File.WriteAllText(Path.Combine(_folder, "GlobalUsings.cs"), "global using DotGram;\n");
		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts(lexical: true).Replace("using DotGram;", "", StringComparison.Ordinal));
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);

		var context = (await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session)).Resolution.Context!.Value;

		Assert.True(context.Lexical);
		Assert.Equal(["B"], context.Included.Select(static item => item.Name));
	}

	/// <summary>A host that names <c>[Gram]</c> through its project file's <c>&lt;Using Include="DotGram" /&gt;</c> is found.</summary>
	[Fact]
	public async Task A_host_is_read_with_its_project_files_usings()
	{
		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project.Replace("</Project>", "  <ItemGroup>\n    <Using Include=\"DotGram\" />\n  </ItemGroup>\n</Project>", StringComparison.Ordinal));
		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts(lexical: true).Replace("using DotGram;", "", StringComparison.Ordinal));
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);

		var context = (await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session)).Resolution.Context!.Value;

		Assert.True(context.Lexical);
		Assert.Equal(["B"], context.Included.Select(static item => item.Name));
	}

	/// <summary>
	/// A host includes a grammar class of a project it references: the grammar is that project's file, as
	/// the build reads it from the referenced assembly, not one of the same name in the host's project.
	/// </summary>
	[Fact]
	public async Task A_grammar_of_a_referenced_project_is_included_from_that_project()
	{
		var library = Directory.CreateDirectory(Path.Combine(_folder, "Library")).FullName;
		var app     = Directory.CreateDirectory(Path.Combine(_folder, "App")).FullName;

		File.WriteAllText(Path.Combine(library, "Library.csproj"), Project);
		File.WriteAllText(Path.Combine(library, "Base.cs"), "using DotGram;\n\nnamespace Library;\n\n[Gram(\"Base.gram\", DefaultAlias = \"B\")]\npublic partial class Base;\n");
		File.WriteAllText(Path.Combine(library, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(Path.Combine(app, "App.csproj"), Project.Replace("</Project>", "  <ItemGroup>\n    <ProjectReference Include=\"..\\Library\\Library.csproj\" />\n  </ItemGroup>\n</Project>", StringComparison.Ordinal));
		File.WriteAllText(Path.Combine(app, "Hosts.cs"), "using DotGram;\n\nnamespace App;\n\n[Gram(\"Dialect.gram\")]\n[GramInclude(typeof(Library.Base))]\npublic partial class Dialect;\n");
		File.WriteAllText(Path.Combine(app, "Base.gram"), "Other = 'x'\n");
		File.WriteAllText(Path.Combine(app, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(app, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);

		var context = (await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session)).Resolution.Context!.Value;

		Assert.Equal(Path.Combine(library, "Base.gram"), context.Included.Single().FilePath);
	}

	/// <summary>
	/// A project below another: the outer project compiles the inner one's C# too, as an SDK project's
	/// glob takes every file below it, so a base class declared there is the outer host's base.
	/// </summary>
	[Fact]
	public async Task A_project_compiles_the_files_of_a_project_below_it_as_the_build_does()
	{
		var nested = Directory.CreateDirectory(Path.Combine(_folder, "Nested")).FullName;

		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project);
		File.WriteAllText(Path.Combine(nested, "Nested.csproj"), Project);
		File.WriteAllText(Path.Combine(_folder, "Base.cs"), "using DotGram;\n\nnamespace Parsers;\n\n[Gram(\"Base.gram\", DefaultAlias = \"B\")]\npublic abstract partial class Base;\n");
		File.WriteAllText(Path.Combine(nested, "Middle.cs"), "namespace Parsers;\n\npublic abstract partial class Middle : Base;\n");
		File.WriteAllText(Path.Combine(_folder, "Dialect.cs"), "using DotGram;\n\nnamespace Parsers;\n\n[Gram(\"Dialect.gram\")]\npublic partial class Dialect : Middle;\n");
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);

		var context = (await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session)).Resolution.Context!.Value;

		Assert.Equal(["B"], context.Included.Select(static item => item.Name));
	}

	/// <summary>
	/// A project's usings as MSBuild has them: an implicit one the project removes is not there, and one
	/// whose condition is false is not either.
	/// </summary>
	[Fact]
	public async Task A_projects_usings_are_what_MSBuild_makes_of_them()
	{
		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project.Replace("</Project>", """
			  <PropertyGroup>
			    <ImplicitUsings>enable</ImplicitUsings>
			  </PropertyGroup>
			  <ItemGroup>
			    <Using Remove="System.Threading.Tasks" />
			    <Using Include="DotGram" Condition="false" />
			  </ItemGroup>
			</Project>
			""", StringComparison.Ordinal));
		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts(lexical: true).Replace("using DotGram;", "", StringComparison.Ordinal));
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		await using var client = await LspClient.StartAsync(null, _folder);

		var folder  = await client.Server.Folder;
		var usings  = folder.Workspace.CurrentSolution.Projects
			.SelectMany(static project => project.Documents)
			.Single(static document => document.Name.EndsWith(".GlobalUsings.g.cs", StringComparison.Ordinal));
		var text    = (await usings.GetTextAsync(TestContext.Current.CancellationToken)).ToString();

		Assert.Contains("global::System.Linq;", text, StringComparison.Ordinal);
		Assert.DoesNotContain("System.Threading.Tasks", text, StringComparison.Ordinal);
		Assert.DoesNotContain("DotGram", text, StringComparison.Ordinal);
	}

	/// <summary>
	/// A workspace the client does not trust is not evaluated — MSBuild would run the project's own code —
	/// and its projects are read from their folders as they stand.
	/// </summary>
	[Fact]
	public async Task An_untrusted_workspace_is_read_without_MSBuild()
	{
		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project.Replace("</Project>", "  <ItemGroup>\n    <Using Include=\"DotGram\" Condition=\"false\" />\n  </ItemGroup>\n</Project>", StringComparison.Ordinal));
		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts(lexical: true));

		await using var client = new LspClient { Trusted = false };

		await client.InitializeAsync(_folder);

		var folder = await client.Server.Folder;

		Assert.False(folder.Evaluated);
		Assert.Single(folder.Workspace.CurrentSolution.Projects, static project => project.Name == "Parsers");
	}

	/// <summary>
	/// A file a project's global usings are in, saved: the grammar's host is looked for again, and is gone
	/// with the using; it comes back when the using does.
	/// </summary>
	[Fact]
	public async Task A_saved_change_to_a_projects_global_usings_finds_the_host_again()
	{
		var usings = Path.Combine(_folder, "GlobalUsings.cs");

		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project);
		File.WriteAllText(usings, "global using DotGram;\n");
		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts(lexical: true).Replace("using DotGram;", "", StringComparison.Ordinal));
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);
		await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session);

		File.WriteAllText(usings, "// nothing\n");
		await client.NotifyAsync("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = LspClient.Uri(usings), type = Lsp.FileChanged } } });

		var deadline = DateTime.UtcNow + LspClient.Timeout;

		while (client.Server.Document(uri)!.Session.Resolution.State == StandaloneGrammarState.Resolved)
		{
			Assert.True(DateTime.UtcNow < deadline, "The host is still found without the using.");
			await Task.Delay(50, TestContext.Current.CancellationToken);
		}

		File.WriteAllText(usings, "global using DotGram;\n");
		await client.NotifyAsync("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri = LspClient.Uri(usings), type = Lsp.FileChanged } } });

		await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session);
	}

	/// <summary>
	/// A project file saved while a folder is being added: its reading of the folders waits for the one
	/// running and then reads the folders the workspace has, the added one among them.
	/// </summary>
	[Fact]
	public async Task A_project_file_saved_while_a_folder_is_added_keeps_the_folder()
	{
		var first   = Directory.CreateDirectory(Path.Combine(_folder, "first")).FullName;
		var added   = Directory.CreateDirectory(Path.Combine(_folder, "added")).FullName;
		var project = Path.Combine(first, "First.csproj");
		var held    = Path.Combine(added, "Held.gram");
		var reading = new ManualResetEventSlim(false);
		var release = new ManualResetEventSlim(false);

		File.WriteAllText(project, Project);
		File.WriteAllText(held, "Word = 'a'\n");

		var workspace = FolderWorkspace.Create(
			[first],
			static _ => { },
			TestContext.Current.CancellationToken,
			read: file =>
			{
				if (file == held)
				{
					reading.Set();
					release.Wait(LspClient.Timeout);
				}

				return File.ReadAllText(file);
			});

		var adding = Task.Run(() => workspace.Reload([first, added]), TestContext.Current.CancellationToken);

		Assert.True(reading.Wait(LspClient.Timeout, TestContext.Current.CancellationToken));

		// The save's reading starts while the folder's is still running, and waits for it.
		var saving = new Thread(() => workspace.FileRead(project, Project));

		saving.Start();

		while ((saving.ThreadState & ThreadState.WaitSleepJoin) == 0)
			await Task.Delay(5, TestContext.Current.CancellationToken);

		release.Set();
		await adding;
		saving.Join(LspClient.Timeout);

		Assert.Equal([first, added], workspace.Folders);
	}

	/// <summary>
	/// A grammar opened before it is on disk, below a project MSBuild evaluates, and then saved: the
	/// project is evaluated again, its glob takes the grammar in, and the grammar finds its host there.
	/// </summary>
	[Fact]
	public async Task A_new_grammar_saved_below_an_evaluated_project_finds_its_host()
	{
		var path = Path.Combine(_folder, "Dialect.gram");
		var uri  = LspClient.Uri(path);

		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project);
		File.WriteAllText(Path.Combine(_folder, "Hosts.cs"), Hosts(lexical: true));
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), "Word = ['a'..'z']+\n");

		await using var client = await LspClient.StartAsync(null, _folder);

		Assert.True((await client.Server.Folder).Evaluated);

		await client.OpenAsync(uri, Dialect);
		await client.DiagnosticsAsync(uri, 1);

		File.WriteAllText(path, Dialect);
		await client.NotifyAsync("workspace/didChangeWatchedFiles", new { changes = new[] { new { uri, type = Lsp.FileCreated } } });

		var context = (await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session)).Resolution.Context!.Value;

		Assert.True(context.Lexical);
		Assert.Equal(["B"], context.Included.Select(static item => item.Name));
	}

	/// <summary>
	/// A project that compiles more C# from elsewhere than the cap allows, by links: the folders are given
	/// up as one with that many files of its own is, and said so.
	/// </summary>
	[Fact]
	public void Linked_files_count_toward_the_cap()
	{
		var project  = Path.Combine(_folder, "Parsers.csproj");
		var linked   = Enumerable.Range(0, FolderWorkspace.MaxSourceFiles + 1).Select(index => Path.Combine(Path.GetTempPath(), "elsewhere", $"File{index}.cs")).ToArray();
		string? said = null;

		File.WriteAllText(project, Project);

		var workspace = FolderWorkspace.Create(
			[_folder],
			message => said ??= message,
			TestContext.Current.CancellationToken,
			read: static file => file.EndsWith(".csproj", StringComparison.Ordinal) ? File.ReadAllText(file) : "class C {}",
			evaluate: (projects, _, _) => projects.ToDictionary(
				static item => item,
				item => new ProjectEvaluation(item, linked, [], [], []),
				FolderWorkspace.PathComparer));

		Assert.False(workspace.Hosts);
		Assert.Contains($"more than {FolderWorkspace.MaxSourceFiles}", said, StringComparison.Ordinal);
	}

	/// <summary>
	/// The folders read again while MSBuild takes longer than the reading's whole deadline — as a busy
	/// machine makes it, every test suite at once on a few processors: the deadline is for a reading
	/// that stalls, and the evaluation, which has a bound of its own, is not counted in it. The
	/// projects stay what MSBuild says, and nothing is given up.
	/// </summary>
	[Fact]
	public void A_slow_evaluation_keeps_the_folders()
	{
		var grammar = Path.Combine(_folder, "Base.gram");
		var said    = new ConcurrentQueue<string>();
		var limit   = TimeSpan.FromMilliseconds(200);
		var slow    = false;

		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project);
		File.WriteAllText(grammar, "Word = ['a'..'z']+\n");

		var workspace = FolderWorkspace.Create(
			[_folder],
			said.Enqueue,
			TestContext.Current.CancellationToken,
			evaluate: (projects, _, _) =>
			{
				if (Volatile.Read(ref slow))
					Thread.Sleep(limit * 5);

				return projects.ToDictionary(static item => item, item => new ProjectEvaluation(item, [], [grammar], [], []), FolderWorkspace.PathComparer);
			});

		Volatile.Write(ref slow, true);
		workspace.Reload(null, limit: limit);

		Assert.Empty(said);
		Assert.True(workspace.Hosts);
		Assert.True(workspace.Evaluated);
		Assert.Contains(workspace.Workspace.CurrentSolution.Projects.SelectMany(static project => project.AdditionalDocuments), document => document.FilePath == grammar);
	}

	/// <summary>
	/// An evaluation that never answers is stopped at its own bound, said so, and its projects are read
	/// by their folders, as where MSBuild fails: the grammars still have their hosts looked for.
	/// </summary>
	[Fact]
	public void An_evaluation_that_never_answers_is_stopped_and_its_projects_read_by_their_folders()
	{
		var grammar = Path.Combine(_folder, "Base.gram");
		var said    = new ConcurrentQueue<string>();

		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project);
		File.WriteAllText(grammar, "Word = ['a'..'z']+\n");

		var workspace = FolderWorkspace.Create(
			[_folder],
			said.Enqueue,
			TestContext.Current.CancellationToken,
			evaluate: static (_, _, cancellationToken) =>
			{
				cancellationToken.WaitHandle.WaitOne(LspClient.Timeout);
				cancellationToken.ThrowIfCancellationRequested();

				throw new InvalidOperationException("The evaluation was not stopped.");
			},
			maxEvaluation: TimeSpan.FromMilliseconds(200));

		Assert.Contains(said, static message => message.Contains("MSBuild took longer than", StringComparison.Ordinal));
		Assert.True(workspace.Hosts);
		Assert.False(workspace.Evaluated);
		Assert.Contains(workspace.Workspace.CurrentSolution.Projects.SelectMany(static project => project.AdditionalDocuments), document => document.FilePath == grammar);
	}

	/// <summary>
	/// A file saved while the folders are read again: the reading began with the older text, and the newer
	/// one, applied meanwhile, is what the workspace has once the reading is done.
	/// </summary>
	[Fact]
	public async Task A_file_saved_while_the_folders_are_read_again_keeps_its_new_text()
	{
		var path    = Path.Combine(_folder, "Base.gram");
		var reading = new ManualResetEventSlim(false);
		var release = new ManualResetEventSlim(false);
		var hold    = false;

		File.WriteAllText(path, "Word = 'old'\n");

		var workspace = FolderWorkspace.Create(
			[_folder],
			static _ => { },
			TestContext.Current.CancellationToken,
			read: file =>
			{
				var text = File.ReadAllText(file);

				if (Volatile.Read(ref hold) && file == path)
				{
					reading.Set();
					release.Wait(LspClient.Timeout);
				}

				return text;
			});

		Volatile.Write(ref hold, true);

		var reload = Task.Run(() => workspace.Reload([_folder]), TestContext.Current.CancellationToken);

		Assert.True(reading.Wait(LspClient.Timeout, TestContext.Current.CancellationToken));

		File.WriteAllText(path, "Word = 'new'\n");
		workspace.FileRead(path, "Word = 'new'\n");
		release.Set();

		await reload;

		var document = workspace.Workspace.CurrentSolution.Projects
			.SelectMany(static project => project.AdditionalDocuments)
			.Single(document => document.FilePath == path);

		Assert.Equal("Word = 'new'\n", (await document.GetTextAsync(TestContext.Current.CancellationToken)).ToString());
	}

	/// <summary>
	/// A grammar a host takes from a base class of its base class is included, where the class between
	/// the two declares no grammar of its own and names no grammar attribute.
	/// </summary>
	[Fact]
	public async Task A_grammar_is_included_through_a_base_class_without_a_grammar()
	{
		File.WriteAllText(Path.Combine(_folder, "Parsers.csproj"), Project);
		File.WriteAllText(Path.Combine(_folder, "Base.cs"), "using DotGram;\n\nnamespace Parsers;\n\n[Gram(\"Base.gram\", DefaultAlias = \"B\")]\npublic abstract partial class Base;\n");
		File.WriteAllText(Path.Combine(_folder, "Middle.cs"), "namespace Parsers;\n\npublic abstract partial class Middle : Base;\n");
		File.WriteAllText(Path.Combine(_folder, "Dialect.cs"), "using DotGram;\n\nnamespace Parsers;\n\n[Gram(\"Dialect.gram\")]\npublic partial class Dialect : Middle;\n");
		File.WriteAllText(Path.Combine(_folder, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(Path.Combine(_folder, "Dialect.gram"), Dialect);

		var uri = LspClient.Uri(Path.Combine(_folder, "Dialect.gram"));

		await using var client = await LspClient.StartAsync(null, _folder);

		await client.OpenAsync(uri, Dialect);

		var context = (await HostTests.ResolvedAsync(client.Server.Document(uri)!.Session)).Resolution.Context!.Value;

		Assert.Equal(["B"], context.Included.Select(static item => item.Name));
	}

	[Fact]
	public async Task The_server_says_it_follows_the_workspace_folders()
	{
		await using var client = new LspClient();

		var result  = await client.InitializeAsync(_folder);
		var folders = result.GetProperty("capabilities").GetProperty("workspace").GetProperty("workspaceFolders");

		Assert.True(folders.GetProperty("supported").GetBoolean());
		Assert.True(folders.GetProperty("changeNotifications").GetBoolean());
	}

	/// <summary>A folder added to the workspace while a grammar in it is open: the grammar's host is looked for there.</summary>
	[Fact]
	public async Task A_folder_added_to_the_workspace_has_its_hosts_found()
	{
		var other  = Directory.CreateDirectory(Path.Combine(_folder, "other")).FullName;
		var added  = Directory.CreateDirectory(Path.Combine(_folder, "added")).FullName;
		var path   = Path.Combine(added, "Dialect.gram");
		var uri    = LspClient.Uri(path);

		File.WriteAllText(Path.Combine(added, "Hosts.cs"), Hosts(lexical: true));
		File.WriteAllText(Path.Combine(added, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(path, Dialect);

		await using var client = await LspClient.StartAsync(null, other);

		await client.OpenAsync(uri, Dialect);
		await client.DiagnosticsAsync(uri, 1);

		Assert.True(client.Server.Document(uri)!.Session.Store.IsCopy(path));

		await client.NotifyAsync("workspace/didChangeWorkspaceFolders", new
		{
			@event = new
			{
				added   = new[] { new { uri = LspClient.Uri(added), name = "added" } },
				removed = Array.Empty<object>(),
			},
		});

		var context = (await ResolvedAsync(client, uri)).Resolution.Context!.Value;

		Assert.True(context.Lexical);
		Assert.Equal(Path.Combine(added, "Base.gram"), context.Included.Single().FilePath);
	}

	/// <summary>A folder taken out of the workspace: its grammars are read as they stand, and its C# is no longer asked.</summary>
	[Fact]
	public async Task A_folder_removed_from_the_workspace_no_longer_hosts()
	{
		var other   = Directory.CreateDirectory(Path.Combine(_folder, "other")).FullName;
		var removed = Directory.CreateDirectory(Path.Combine(_folder, "removed")).FullName;
		var path    = Path.Combine(removed, "Dialect.gram");
		var uri     = LspClient.Uri(path);

		File.WriteAllText(Path.Combine(removed, "Hosts.cs"), Hosts(lexical: true));
		File.WriteAllText(Path.Combine(removed, "Base.gram"), "Word = ['a'..'z']+\n");
		File.WriteAllText(path, Dialect);

		await using var client = await LspClient.StartAsync(null, other, removed);

		await client.OpenAsync(uri, Dialect);
		await ResolvedAsync(client, uri);

		await client.NotifyAsync("workspace/didChangeWorkspaceFolders", new
		{
			@event = new
			{
				added   = Array.Empty<object>(),
				removed = new[] { new { uri = LspClient.Uri(removed), name = "removed" } },
			},
		});

		var deadline = DateTime.UtcNow + LspClient.Timeout;

		while (!(client.Server.Document(uri) is { } document && document.Session.Store.IsCopy(path) && document.Session.Result is not null))
		{
			Assert.True(DateTime.UtcNow < deadline, "The grammar is still read with the removed folder's host.");
			await Task.Delay(50, TestContext.Current.CancellationToken);
		}

		var folder = await client.Server.Folder;

		Assert.False(folder.Contains(path));
		Assert.DoesNotContain(
			folder.Workspace.CurrentSolution.Projects.SelectMany(static project => project.Documents),
			document => document.FilePath is { } file && file.StartsWith(removed, StringComparison.Ordinal));
	}

	/// <summary>The grammar's session settled with its host found; the document may have been opened again by the server meanwhile.</summary>
	static async Task<GramAnalysisResult<GramVersion>> ResolvedAsync(LspClient client, string uri)
	{
		var deadline = DateTime.UtcNow + LspClient.Timeout;

		while (DateTime.UtcNow < deadline)
		{
			if (client.Server.Document(uri)?.Session.Result is { Resolution.State: StandaloneGrammarState.Resolved } result)
				return result;

			await Task.Delay(50, TestContext.Current.CancellationToken);
		}

		Assert.Fail($"The host is {client.Server.Document(uri)?.Session.Resolution.State}, not resolved.");
		return null;
	}
}
