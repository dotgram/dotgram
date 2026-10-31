using System;
using System.IO;
using System.Linq;
using System.Text;

namespace DotGram.LanguageServer;

/// <summary>Local paths to <c>file:</c> URIs and back, in the form VS Code writes them.</summary>
/// <remarks>
/// VS Code writes a Windows path <c>C:\Users\a.gram</c> as <c>file:///c%3A/Users/a.gram</c>: the drive
/// letter lower-cased and its colon escaped. <see cref="Uri.LocalPath"/> reads that as <c>/c:/Users/a.gram</c>,
/// which <see cref="Path.GetFullPath(string)"/> then makes a path under the current drive, so the URI is
/// read here by hand: unescaped, the slash before a drive letter dropped, a host read as a UNC share.
/// </remarks>
static class FileUris
{
	/// <summary>The local path of a <c>file:</c> URI; null for any other scheme, such as an unsaved document's.</summary>
	public static string? ToPath(string? uri)
	{
		if (uri is null || !uri.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
			return null;

		var rest      = uri.Substring("file:".Length);
		var authority = "";

		if (rest.StartsWith("//", StringComparison.Ordinal))
		{
			rest = rest.Substring(2);

			var slash = rest.IndexOf('/', StringComparison.Ordinal);

			authority = slash < 0 ? rest : rest.Substring(0, slash);
			rest      = slash < 0 ? "/" : rest.Substring(slash);
		}

		var end = rest.IndexOfAny(['?', '#']);

		if (end >= 0)
			rest = rest.Substring(0, end);

		var path = Uri.UnescapeDataString(rest);

		// On Windows a host, localhost too, is a UNC share's, \\localhost\share; elsewhere localhost is this machine.
		if (!OperatingSystem.IsWindows() && authority.Equals("localhost", StringComparison.OrdinalIgnoreCase))
			authority = "";

		try
		{
			if (OperatingSystem.IsWindows())
			{
				if (authority.Length > 0)
					return Path.GetFullPath(@"\\" + authority + path.Replace('/', '\\'));

				// The drive as Windows writes it, upper-case, which VS Code lower-cases.
				if (path.Length >= 3 && path[0] == '/' && char.IsAsciiLetter(path[1]) && path[2] == ':')
					path = char.ToUpperInvariant(path[1]) + path.Substring(2);

				return Path.GetFullPath(path.Replace('/', '\\'));
			}

			return Path.GetFullPath(authority.Length > 0 ? "//" + authority + path : path);
		}
		catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
		{
			return null;
		}
	}

	/// <summary>A local path as VS Code writes it as a URI.</summary>
	public static string ToUri(string path)
	{
		var full = Path.GetFullPath(path);

		if (OperatingSystem.IsWindows())
		{
			full = full.Replace('\\', '/');

			if (full.StartsWith("//", StringComparison.Ordinal))
			{
				var slash = full.IndexOf('/', 2);
				var host  = slash < 0 ? full.Substring(2) : full.Substring(2, slash - 2);

				return "file://" + host + Escape(slash < 0 ? "/" : full.Substring(slash));
			}

			if (full.Length >= 2 && char.IsAsciiLetter(full[0]) && full[1] == ':')
				return "file:///" + char.ToLowerInvariant(full[0]) + "%3A" + Escape(full.Substring(2));
		}

		return "file://" + Escape(full);
	}

	static string Escape(string path)
	{
		return string.Join("/", path.Split('/').Select(Uri.EscapeDataString));
	}
}
