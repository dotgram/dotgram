using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotGram.Language;

/// <summary>Where an included grammar is opened: its file, or a read-only copy of what an assembly carries.</summary>
/// <remarks>
/// A grammar included from a referenced assembly has no file in reach, only the text its class
/// carries in <c>[GramSource]</c>, and that text is what the including grammar was compiled with. An
/// editor shows it as a document of its own, read-only: editing it would change nothing that is
/// compiled. Where that document lives is the host's to decide — a file under the temporary
/// directory for Visual Studio, which opens files, or nothing on disk at all for a test — and this is
/// the seam it decides through. A copy is read as it stands and not as part of a project, so the
/// store also says which paths are its copies.
/// </remarks>
interface IIncludedGrammarStore
{
	/// <summary>
	/// The path to open the grammar at: its own file where it has one, else its copy, made where it
	/// is not there; null where no copy could be made.
	/// </summary>
	string? PathToOpen(StandaloneIncludedGrammar grammar);

	/// <summary>Whether a path is one of this store's copies.</summary>
	bool IsCopy(string filePath);
}

/// <summary>What a copy of an included grammar is called, whatever stores it.</summary>
/// <remarks>
/// Named for the grammar and for its content, so that a changed reference gets a new copy and two
/// editors that copy the same grammar at once arrive at the same name.
/// </remarks>
static class IncludedGrammarView
{
	static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

	/// <summary>The copy's file name: <c>Name.hash.gram</c>.</summary>
	public static string CopyName(StandaloneIncludedGrammar grammar)
	{
		return $"{FileName(grammar.Name)}.{Hash(grammar.Text)}.gram";
	}

	/// <summary>The grammar's name with what a file name cannot hold replaced.</summary>
	public static string FileName(string name)
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

/// <summary>Copies written under the temporary directory, read-only, for an editor that opens files.</summary>
/// <remarks>
/// A copy is written once under its content's name and never changed, so two instances of an
/// editor that open the same grammar at once write the same file: whichever moves its copy into
/// place first wins, and the other takes that one. Copies a changed reference left behind are
/// removed once nobody has used them for a day.
/// </remarks>
sealed class TempDirectoryIncludedGrammarStore : IIncludedGrammarStore
{
	static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

	public static TempDirectoryIncludedGrammarStore Instance { get; } = new();

	static string CopyDirectory { get; } = Path.Combine(Path.GetTempPath(), "DotGram", "Included");

	/// <summary>The file to open, or null where a copy could not be written.</summary>
	public string? PathOf(StandaloneIncludedGrammar grammar)
	{
		if (grammar.FilePath is not null)
			return grammar.FilePath;

		var name = IncludedGrammarView.FileName(grammar.Name);
		var path = Path.Combine(CopyDirectory, IncludedGrammarView.CopyName(grammar));

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
	public string? PathToOpen(StandaloneIncludedGrammar grammar)
	{
		for (var attempt = 0; attempt < 3; attempt++)
			if (PathOf(grammar) is { } path && File.Exists(path))
				return path;

		return null;
	}

	/// <summary>
	/// Marks a copy as used now, so that no instance's cleanup takes it for one a changed reference
	/// left behind; false where it went meanwhile. Only a copy last marked an hour ago or more:
	/// cleanup waits a day, and a copy asked for again and again is left as it is.
	/// </summary>
	static bool Touch(string path)
	{
		try
		{
			if (File.GetLastWriteTimeUtc(path) > DateTime.UtcNow.AddHours(-1))
				return true;

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
	public bool IsCopy(string filePath)
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
}

/// <summary>Copies held in memory, under paths no file system has, for a host that opens nothing.</summary>
/// <remarks>
/// What a test asks of a store: that a grammar asked for is given a path, the same one every time,
/// and that the path is known for a copy afterwards. The text stays readable by its path.
/// </remarks>
sealed class InMemoryIncludedGrammarStore : IIncludedGrammarStore
{
	/// <summary>The prefix every copy's path begins with.</summary>
	public const string Prefix = "memory:/DotGram/Included/";

	readonly object _gate = new();
	readonly Dictionary<string, string> _copies = new(StringComparer.Ordinal);

	public string? PathToOpen(StandaloneIncludedGrammar grammar)
	{
		if (grammar.FilePath is not null)
			return grammar.FilePath;

		var path = Prefix + IncludedGrammarView.CopyName(grammar);

		lock (_gate)
			_copies[path] = grammar.Text;

		return path;
	}

	public bool IsCopy(string filePath)
	{
		return filePath.StartsWith(Prefix, StringComparison.Ordinal);
	}

	/// <summary>The text of a copy made here, or null.</summary>
	public string? Text(string path)
	{
		lock (_gate)
			return _copies.TryGetValue(path, out var text) ? text : null;
	}
}
