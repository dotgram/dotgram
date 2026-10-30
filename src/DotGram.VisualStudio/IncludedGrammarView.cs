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
	/// <remarks>
	/// A copy is named for its content and never changed once it is there, so two instances of
	/// Visual Studio that open the same grammar at once write the same file: whichever moves its
	/// copy into place first wins, and the other takes that one.
	/// </remarks>
	public static string? PathOf(StandaloneIncludedGrammar grammar)
	{
		if (grammar.FilePath is not null)
			return grammar.FilePath;

		var name = FileName(grammar.Name);
		var path = Path.Combine(CopyDirectory, $"{name}.{Hash(grammar.Text)}.gram");

		if (File.Exists(path) && Touch(path))
			return path;

		string? written = null;

		try
		{
			Directory.CreateDirectory(CopyDirectory);

			// Written whole under a name of its own and then moved into place, so that a copy
			// is never seen half written.
			written = Path.Combine(CopyDirectory, $"{name}.{Guid.NewGuid():N}.tmp");

			File.WriteAllText(written, grammar.Text, Utf8);
			File.SetAttributes(written, File.GetAttributes(written) | FileAttributes.ReadOnly);

			try
			{
				File.Move(written, path);
				written = null;
			}
			catch (IOException) when (File.Exists(path))
			{
				// Another instance put the same content there first.
			}

			RemoveOlder(name, path);

			return path;
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			return File.Exists(path) ? path : null;
		}
		finally
		{
			if (written is not null)
				Delete(written);
		}
	}

	/// <summary>
	/// The file to open, made again where it went in the moment between being found and being
	/// opened: another instance's cleanup may take a copy it found a day old.
	/// </summary>
	public static string? PathToOpen(StandaloneIncludedGrammar grammar)
	{
		for (var attempt = 0; attempt < 3; attempt++)
			if (PathOf(grammar) is { } path && File.Exists(path))
				return path;

		return null;
	}

	/// <summary>
	/// Marks a copy as used now, so that no instance's cleanup takes it for one a changed reference
	/// left behind; false where it went meanwhile.
	/// </summary>
	static bool Touch(string path)
	{
		try
		{
			File.SetAttributes(path, FileAttributes.Normal);
			File.SetLastWriteTimeUtc(path, DateTime.UtcNow);
			File.SetAttributes(path, FileAttributes.ReadOnly);

			return true;
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// Gone, or held by someone who is about to use it: what is there is current either way.
			return File.Exists(path);
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
	/// <remarks>
	/// Only those nobody has written for a day: a newer one may be the copy another instance of
	/// Visual Studio has just made for a reference of its own, and is about to open.
	/// </remarks>
	static void RemoveOlder(string name, string current)
	{
		foreach (var older in Directory.EnumerateFiles(CopyDirectory, name + ".*.gram"))
		{
			if (string.Equals(older, current, StringComparison.OrdinalIgnoreCase) ||
				File.GetLastWriteTimeUtc(older) > DateTime.UtcNow.AddDays(-1))
				continue;

			Delete(older);
		}
	}

	static void Delete(string path)
	{
		try
		{
			File.SetAttributes(path, FileAttributes.Normal);
			File.Delete(path);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// Open in an editor, most likely; it goes the next time.
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
