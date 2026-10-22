using System;
using System.Collections.Concurrent;

namespace DotGram.Cli;

/// <summary>How a directory's file system compares names.</summary>
static class FileSystem
{
	static readonly ConcurrentDictionary<string, StringComparer> Probed = new(StringComparer.Ordinal);

	/// <summary>
	/// Names as the file system holding the directory tells them apart, asked of it: a file is made
	/// with one case and looked for with the other. A volume of either kind can be met on any system
	/// (APFS and NTFS directories can be either), so nothing is assumed from the system. A directory
	/// that does not exist holds no file to tell apart, and gets the ordinal comparer. Asked once per
	/// directory for the run.
	/// </summary>
	public static StringComparer NamesIn(string directory)
	{
		var full = Path.GetFullPath(directory);

		return Directory.Exists(full) ? Probed.GetOrAdd(full, Probe) : StringComparer.Ordinal;
	}

	static StringComparer Probe(string directory)
	{
		var made   = Path.Combine(directory, ".dotgram-case-Probe-" + Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture));
		var looked = Path.Combine(directory, Path.GetFileName(made).ToLowerInvariant());

		try
		{
			File.WriteAllBytes(made, []);

			return File.Exists(looked) ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			// A folder the tool cannot write into it cannot write its files into either; that is said
			// where they are written.
			return StringComparer.Ordinal;
		}
		finally
		{
			try
			{
				File.Delete(made);
			}
			catch (IOException)
			{
			}
		}
	}
}
