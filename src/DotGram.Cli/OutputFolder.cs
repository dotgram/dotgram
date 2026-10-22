using System;
using System.Security.Cryptography;
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
	/// What the manifest says and how it is written: one format, versioned, the files sorted, each
	/// with the SHA-256 of its text with line ends made LF — so that a checkout that stores LF and one
	/// that stores CRLF hold the same manifest.
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

		foreach (var (name, content) in files.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
		{
			text.Append(first ? "\n" : ",\n");
			text.Append("    { \"name\": ").Append(JsonSerializer.Serialize(name))
				.Append(", \"sha256\": \"").Append(Hash(content)).Append("\" }");

			first = false;
		}

		text.Append(first ? "]\n" : "\n  ]\n");
		text.Append("}\n");

		return text.ToString();
	}

	static string Hash(string text)
	{
		return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Normalized(text)))).ToLowerInvariant();
	}

	/// <summary>Line ends as LF, and no byte order mark: the text as two checkouts agree on it.</summary>
	public static string Normalized(string text)
	{
		if (text.Length > 0 && text[0] == '﻿')
			text = text.Substring(1);

		return text.Replace("\r\n", "\n").Replace('\r', '\n');
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
		catch (Exception exception) when (exception is JsonException or KeyNotFoundException or InvalidOperationException or IOException)
		{
			throw ToolException.Failure($"{path} is not a manifest this tool wrote ({exception.Message}); delete it to start the folder afresh.");
		}
	}

	/// <summary>
	/// Writes every file that changed, UTF-8 with a byte order mark as csc writes them, each through a
	/// temporary file and a rename, deletes those the last manifest listed that this run did not write,
	/// and writes the manifest last.
	/// </summary>
	/// <returns>How many files were written, how many were already as they are, and how many deleted.</returns>
	public static (int Written, int Unchanged, int Deleted) Write(string directory, IReadOnlyDictionary<string, string> files, string manifest)
	{
		var previous = Listed(directory);

		try
		{
			Directory.CreateDirectory(directory);

			var written   = 0;
			var unchanged = 0;
			var deleted   = 0;

			foreach (var (name, text) in files)
			{
				if (WriteIfChanged(Path.Combine(directory, name), Bytes(text, bom: true)))
					written++;
				else
					unchanged++;
			}

			foreach (var name in previous)
			{
				if (files.ContainsKey(name))
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

	/// <summary>How the folder compares with what a run would write.</summary>
	/// <param name="Equal">Whether every file is there, equal once line ends are made alike, and no listed file is left over.</param>
	/// <param name="LineEndsOnly">Equal, but only once line ends (or the byte order mark) are made alike.</param>
	/// <param name="Difference">The first difference, in words, where there is one.</param>
	public readonly record struct Comparison(bool Equal, bool LineEndsOnly, string? Difference);

	/// <summary>Compares the folder with what a run would write, writing nothing.</summary>
	public static Comparison Verify(string directory, IReadOnlyDictionary<string, string> files, string manifest)
	{
		var lineEnds = false;

		foreach (var (name, text) in files.Append(new KeyValuePair<string, string>(ManifestName, manifest)))
		{
			var path = Path.Combine(directory, name);

			if (!File.Exists(path))
				return new Comparison(false, false, $"{path}: missing; the run would write it.");

			var bytes  = File.ReadAllBytes(path);
			var wanted = Bytes(text, bom: name != ManifestName);

			if (bytes.AsSpan().SequenceEqual(wanted))
				continue;

			var actual   = Normalized(Encoding.UTF8.GetString(bytes));
			var expected = Normalized(text);

			if (actual == expected)
			{
				lineEnds = true;
				continue;
			}

			return new Comparison(false, false, $"{path}: {FirstDifference(expected, actual)}");
		}

		var listed = Listed(directory);

		foreach (var name in listed)
			if (!files.ContainsKey(name) && File.Exists(Path.Combine(directory, name)))
				return new Comparison(false, false, $"{Path.Combine(directory, name)}: the manifest lists it and the run would not write it.");

		return new Comparison(true, lineEnds, null);
	}

	/// <summary>The first line the two texts differ on, both of them, cut to a readable length.</summary>
	static string FirstDifference(string expected, string actual)
	{
		var wanted = expected.Split('\n');
		var found  = actual.Split('\n');

		for (var line = 0; ; line++)
		{
			var a = line < wanted.Length ? wanted[line] : null;
			var b = line < found.Length ? found[line] : null;

			if (a == b)
			{
				if (a is null)
					return "differs";

				continue;
			}

			var number = (line + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

			return $"differs from line {number}:" + Environment.NewLine +
				"  generated: " + Shown(a) + Environment.NewLine +
				"  in folder: " + Shown(b);
		}
	}

	static string Shown(string? line)
	{
		if (line is null)
			return "(end of file)";

		return line.Length <= 160 ? line : line.Substring(0, 160) + "…";
	}
}
