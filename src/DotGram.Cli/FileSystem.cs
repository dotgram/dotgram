using System;
using System.Collections.Concurrent;

namespace DotGram.Cli;

/// <summary>How a directory's file system compares names.</summary>
static class FileSystem
{
	/// <summary>One probe per directory: a lazy value, so that two callers asking at once wait for the one probe.</summary>
	static readonly ConcurrentDictionary<string, Lazy<StringComparer>> Probed = new(StringComparer.Ordinal);

	/// <summary>
	/// Names as the file system holding the directory tells them apart, asked of it: a file is made
	/// with one case and looked for with the other. A volume of either kind can be met on any system
	/// (APFS and NTFS directories can be either), so nothing is assumed from the system. A directory
	/// that does not exist holds no file to tell apart, and gets the ordinal comparer. Asked once per
	/// directory for the run, however many threads ask: the probe file has a name of its own (a GUID),
	/// is created only where none exists, and is deleted only by the call that created it.
	/// </summary>
	public static StringComparer NamesIn(string directory)
	{
		var full = Path.GetFullPath(directory);

		return Directory.Exists(full) ? Probed.GetOrAdd(full, static directory => new Lazy<StringComparer>(() => Probe(directory))).Value : StringComparer.Ordinal;
	}

	static StringComparer Probe(string directory)
	{
		var made   = Path.Combine(directory, ".dotgram-case-Probe-" + Guid.NewGuid().ToString("N"));
		var looked = Path.Combine(directory, Path.GetFileName(made).ToLowerInvariant());
		var created = false;

		try
		{
			// CreateNew: a file of that name that is not ours is never opened, let alone deleted.
			using (new FileStream(made, FileMode.CreateNew, FileAccess.Write, FileShare.None))
				created = true;

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
			if (created)
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
}
