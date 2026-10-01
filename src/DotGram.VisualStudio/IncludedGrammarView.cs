using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotGram.VisualStudio;

/// <summary>Where an included grammar is opened: its file, or a read-only copy of what an assembly carries.</summary>
/// <remarks>
/// A grammar included from a referenced assembly has no file in reach, only the text its
/// class carries in <c>[GramSource]</c>, and that text is what the including grammar was
/// compiled with. It is written to a file of its own under the temporary directory, named
/// for the grammar and its content so that a changed reference gets a new copy, and marked
/// read-only: editing it would change nothing that is compiled.
/// </remarks>
static class IncludedGrammarView
{
	static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

	static string CopyDirectory { get; } = Path.Combine(Path.GetTempPath(), "DotGram", "Included");

	/// <summary>The file to open, or null where a copy could not be written.</summary>
	public static string? PathOf(StandaloneIncludedGrammar grammar)
	{
		if (grammar.FilePath is not null)
			return grammar.FilePath;

		var name = FileName(grammar.Name);
		var path = Path.Combine(CopyDirectory, $"{name}.{Hash(grammar.Text)}.gram");

		try
		{
			if (File.Exists(path) && File.ReadAllText(path, Utf8) == grammar.Text)
				return path;

			Directory.CreateDirectory(CopyDirectory);

			// Written whole under a name of its own and then moved into place, so that a copy
			// is never seen half written; one that is there and differs is replaced.
			var written = Path.Combine(CopyDirectory, $"{name}.{Guid.NewGuid():N}.tmp");

			File.WriteAllText(written, grammar.Text, Utf8);

			if (File.Exists(path))
			{
				File.SetAttributes(path, FileAttributes.Normal);
				File.Delete(path);
			}

			File.Move(written, path);
			File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);

			RemoveOlder(name, path);

			return path;
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			return null;
		}
	}

	/// <summary>Whether a file is one of these copies, which is read as it stands and not as part of a project.</summary>
	public static bool IsCopy(string filePath)
	{
		return string.Equals(
			Path.GetDirectoryName(Path.GetFullPath(filePath)),
			Path.GetFullPath(CopyDirectory),
			StringComparison.OrdinalIgnoreCase);
	}

	/// <summary>The copies of earlier texts of the same grammar, which a changed reference left behind.</summary>
	static void RemoveOlder(string name, string current)
	{
		foreach (var older in Directory.EnumerateFiles(CopyDirectory, name + ".*.gram"))
		{
			if (string.Equals(older, current, StringComparison.OrdinalIgnoreCase))
				continue;

			try
			{
				File.SetAttributes(older, FileAttributes.Normal);
				File.Delete(older);
			}
			catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
			{
				// Open in an editor, most likely; it goes the next time.
			}
		}
	}

	static string FileName(string name)
	{
		var invalid = Path.GetInvalidFileNameChars();
		var result  = new StringBuilder(name.Length);

		foreach (var character in name)
			result.Append(Array.IndexOf(invalid, character) >= 0 ? '_' : character);

		return result.ToString();
	}

	static string Hash(string text)
	{
		using var sha = SHA256.Create();
		var bytes     = sha.ComputeHash(Utf8.GetBytes(text));
		var result    = new StringBuilder(16);

		for (var index = 0; index < 8; index++)
			result.Append(bytes[index].ToString("x2", CultureInfo.InvariantCulture));

		return result.ToString();
	}
}
