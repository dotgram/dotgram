using System;
using System.Text;
using System.Text.Json;

namespace DotGram.Cli;

/// <summary>
/// The output folder: the generated files, and <c>.dotgram.json</c>, which lists them so that a file
/// a later run no longer writes is deleted and nothing else in the folder is ever touched.
/// </summary>
static class OutputFolder
{
	public const string ManifestName = ".dotgram.json";

	/// <summary>
	/// What the manifest says and how it is written: one format, versioned, the tool and generator that
	/// wrote the folder, and the files, sorted — what the next run deletes when it no longer writes one.
	/// </summary>
	public static string Manifest(IReadOnlyDictionary<string, string> files, string toolVersion, string generatorVersion)
	{
		var text = new StringBuilder();

		text.Append("{\n");
		text.Append("  \"format\": 1,\n");
		text.Append("  \"tool\": ").Append(JsonSerializer.Serialize(toolVersion)).Append(",\n");
		text.Append("  \"generator\": ").Append(JsonSerializer.Serialize(generatorVersion)).Append(",\n");
		text.Append("  \"files\": [");

		var first = true;

		foreach (var name in files.Keys.Order(StringComparer.Ordinal))
		{
			text.Append(first ? "\n" : ",\n");
			text.Append("    { \"name\": ").Append(JsonSerializer.Serialize(name)).Append(" }");

			first = false;
		}

		text.Append(first ? "]\n" : "\n  ]\n");
		text.Append("}\n");

		return text.ToString();
	}

	/// <summary>The file names a manifest in the folder lists, or none.</summary>
	public static IReadOnlyList<string> Listed(string directory)
	{
		var path = Path.Combine(directory, ManifestName);

		if (!File.Exists(path))
			return [];

		try
		{
			using var json = JsonDocument.Parse(File.ReadAllText(path));

			return
			[
				.. json.RootElement.GetProperty("files").EnumerateArray()
					.Select(static file => file.GetProperty("name").GetString()!)
					.Where(static name => name.Length > 0 && Path.GetFileName(name) == name),
			];
		}
		catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException or IOException or UnauthorizedAccessException)
		{
			throw ToolException.Failure($"{path} is not a manifest this tool wrote ({exception.Message}); delete it to start the folder afresh.");
		}
	}

	/// <summary>
	/// Writes every file that changed, UTF-8 with a byte order mark as csc writes them, each through a
	/// temporary file and a rename, deletes those the last manifest listed that this run did not write,
	/// and writes the manifest last.
	/// </summary>
	/// <param name="names">How the file system compares names; asked of it (<see cref="FileSystem.NamesIn"/>) but in a test.</param>
	/// <returns>How many files were written, how many were already as they are, and how many deleted.</returns>
	public static (int Written, int Unchanged, int Deleted) Write(
		string directory, IReadOnlyDictionary<string, string> files, string manifest, StringComparer? names = null)
	{
		var previous = Listed(directory);
		try
		{
			Directory.CreateDirectory(directory);

			names ??= FileSystem.NamesIn(directory);

			var written   = 0;
			var unchanged = 0;
			var deleted   = 0;

			var current = new HashSet<string>(files.Keys, names);

			foreach (var (name, text) in files)
			{
				// A file the last run wrote under another case of the same name, where the file system
				// takes the two for one: removed first, so that the file now has the name it is given.
				foreach (var old in previous)
					if (names.Equals(old, name) && !string.Equals(old, name, StringComparison.Ordinal) && File.Exists(Path.Combine(directory, old)))
						File.Delete(Path.Combine(directory, old));

				if (WriteIfChanged(Path.Combine(directory, name), Bytes(text, bom: true)))
					written++;
				else
					unchanged++;
			}

			// What the last run wrote and this one does not, by the file system's own sense of a name:
			// a spelling that differs only in case from one just written is that file, and stays.
			foreach (var name in previous)
			{
				if (current.Contains(name))
					continue;

				var path = Path.Combine(directory, name);

				if (File.Exists(path))
				{
					File.Delete(path);
					deleted++;
				}
			}

			WriteIfChanged(Path.Combine(directory, ManifestName), Bytes(manifest, bom: false));

			return (written, unchanged, deleted);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			throw ToolException.Failure($"cannot write into {directory}: {exception.Message}");
		}
	}

	static byte[] Bytes(string text, bool bom)
	{
		var body = Encoding.UTF8.GetBytes(text);

		if (!bom)
			return body;

		var bytes = new byte[body.Length + 3];

		bytes[0] = 0xEF;
		bytes[1] = 0xBB;
		bytes[2] = 0xBF;

		body.CopyTo(bytes, 3);

		return bytes;
	}

	/// <summary>Writes the file unless it already holds exactly these bytes, so that an unchanged file keeps its time.</summary>
	static bool WriteIfChanged(string path, byte[] bytes)
	{
		if (File.Exists(path) && new FileInfo(path).Length == bytes.Length && File.ReadAllBytes(path).AsSpan().SequenceEqual(bytes))
			return false;

		var temporary = path + ".tmp-" + Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture);

		try
		{
			File.WriteAllBytes(temporary, bytes);
			File.Move(temporary, path, overwrite: true);
		}
		finally
		{
			if (File.Exists(temporary))
				File.Delete(temporary);
		}

		return true;
	}
}
