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
	public static string PathOf(StandaloneIncludedGrammar grammar)
	{
		if (grammar.FilePath is not null)
			return grammar.FilePath;

		var directory = Path.Combine(Path.GetTempPath(), "DotGram", "Included");
		var path      = Path.Combine(directory, $"{FileName(grammar.Name)}.{Hash(grammar.Text)}.gram");

		if (!File.Exists(path))
		{
			Directory.CreateDirectory(directory);
			File.WriteAllText(path, grammar.Text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);
		}

		return path;
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
		var bytes     = sha.ComputeHash(Encoding.UTF8.GetBytes(text));
		var result    = new StringBuilder(16);

		for (var index = 0; index < 8; index++)
			result.Append(bytes[index].ToString("x2", CultureInfo.InvariantCulture));

		return result.ToString();
	}
}
