using System;
using System.IO;

using Xunit;

namespace DotGram.LanguageServer.Tests;

/// <summary>A document's URI as VS Code writes it is the file it names, and the server's URIs are written the same way.</summary>
public sealed class UriTests
{
	[Fact]
	public void A_windows_uri_is_the_windows_path()
	{
		Assert.SkipUnless(OperatingSystem.IsWindows(), "Windows paths.");

		Assert.Equal(@"C:\Users\Igor\a b.gram", FileUris.ToPath("file:///c%3A/Users/Igor/a%20b.gram"));
		Assert.Equal(@"C:\Users\a.gram", FileUris.ToPath("file:///C:/Users/a.gram"), ignoreCase: true);
		Assert.Equal(@"\\server\share\a.gram", FileUris.ToPath("file://server/share/a.gram"), ignoreCase: true);
		Assert.Equal("file:///c%3A/Users/a%20b.gram", FileUris.ToUri(@"C:\Users\a b.gram"));
		Assert.Equal("file://server/share/a.gram", FileUris.ToUri(@"\\server\share\a.gram"));
	}

	[Fact]
	public void A_unix_uri_is_the_unix_path()
	{
		Assert.SkipWhen(OperatingSystem.IsWindows(), "Unix paths.");

		Assert.Equal("/home/igor/a b#1.gram", FileUris.ToPath("file:///home/igor/a%20b%231.gram"));
		Assert.Equal("file:///home/igor/a%20b%231.gram", FileUris.ToUri("/home/igor/a b#1.gram"));
	}

	[Fact]
	public void Another_scheme_is_no_file()
	{
		Assert.Null(FileUris.ToPath("untitled:Untitled-1"));
		Assert.Null(FileUris.ToPath(null));
	}

	[Fact]
	public void The_client_and_the_server_write_a_path_alike()
	{
		var path = Path.Combine(Path.GetTempPath(), "a b", "Ünïcode.gram");

		Assert.Equal(LspClient.Uri(path), FileUris.ToUri(path));
		Assert.Equal(Path.GetFullPath(path), FileUris.ToPath(LspClient.Uri(path)));
	}
}
