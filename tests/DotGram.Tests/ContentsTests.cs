using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The table of contents of <c>docs/syntax.md</c> is the headings of that document.
/// </summary>
/// <remarks>
/// Two thousand lines and forty sections need a way in, and a listing that has fallen behind
/// the document is worse than none: it sends a reader to a section that has moved or is gone.
/// So it is not maintained by hand. This builds what the listing should be from the headings
/// themselves and compares — a heading added, renamed or removed fails here until the listing
/// says so too.
/// </remarks>
public sealed class ContentsTests
{
	[Fact]
	public void The_contents_of_the_specification_are_its_headings()
	{
		var document = File.ReadAllText(Specification).Replace("\r\n", "\n", StringComparison.Ordinal);

		Assert.Equal(Expected(document), Listed(document));
	}

	/// <summary>And no two sections answer to the same link.</summary>
	[Fact]
	public void And_no_two_sections_share_an_anchor()
	{
		var anchors = Headings(File.ReadAllText(Specification).Replace("\r\n", "\n", StringComparison.Ordinal))
			.Select(one => Anchor(one.Title))
			.ToArray();

		var twice = anchors
			.GroupBy(one => one, StringComparer.Ordinal)
			.Where(one => one.Count() > 1)
			.Select(one => one.Key)
			.ToArray();

		Assert.True(twice.Length == 0, "Two sections would link to the same place: " + string.Join(", ", twice));
	}

	/// <summary>What the listing must say, built from the headings.</summary>
	static string Expected(string document)
	{
		var text = new StringBuilder();

		foreach (var (depth, title) in Headings(document))
			text
				.Append(depth == 0 ? "" : "  ")
				.Append("- [").Append(title).Append("](#").Append(Anchor(title)).Append(")\n");

		return text.ToString();
	}

	/// <summary>And what it says.</summary>
	static string Listed(string document)
	{
		var at = document.IndexOf("\n## Contents\n", StringComparison.Ordinal);

		Assert.True(at >= 0, "docs/syntax.md has no '## Contents' section.");

		var text = new StringBuilder();

		foreach (var line in document[at..].Split('\n').Skip(2))
		{
			if (line.Length == 0)
				continue;

			if (!line.TrimStart().StartsWith("- [", StringComparison.Ordinal))
				break;

			text.Append(line).Append('\n');
		}

		return text.ToString();
	}

	/// <summary>
	/// The sections, in order, skipping fenced code and the listing's own heading.
	/// </summary>
	static IEnumerable<(int Depth, string Title)> Headings(string document)
	{
		var fenced = false;

		foreach (var line in document.Split('\n'))
		{
			if (line.StartsWith("```", StringComparison.Ordinal))
			{
				fenced = !fenced;

				continue;
			}

			if (fenced)
				continue;

			if (line.StartsWith("## ", StringComparison.Ordinal) && line[3..].Trim() != "Contents")
				yield return (0, line[3..].Trim());

			else if (line.StartsWith("### ", StringComparison.Ordinal))
				yield return (1, line[4..].Trim());
		}
	}

	/// <summary>
	/// The fragment a heading answers to: lowercased, everything but letters, digits, spaces,
	/// hyphens and underscores dropped, and the spaces that remain turned into hyphens.
	/// </summary>
	static string Anchor(string title)
	{
		var text = new StringBuilder(title.Length);

		foreach (var c in title.ToLowerInvariant())
			if (char.IsLetterOrDigit(c) || c is '-' or '_')
				text.Append(c);
			else if (c == ' ')
				text.Append('-');

		return text.ToString();
	}

	static string Specification =>
		Path.Combine(
			Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!)!,
			"docs",
			"syntax.md");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
