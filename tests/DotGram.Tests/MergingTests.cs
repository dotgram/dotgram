using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// No recognizer contains one state twice.
/// </summary>
/// <remarks>
/// <para>
/// Compilation writes a rule's shape wherever the rule is used, so before layout the table
/// holds the same block over and over. <c>Machine.Layout.Merge</c> points every state that
/// does exactly what an earlier one does at that earlier one, and this is what is left: in
/// <c>Url</c>, 427 states became 254.
/// </para>
/// <para>
/// Held against the checked-in snapshots rather than against the generator, so that it is
/// also a guard on the snapshots themselves — one updated without being read would otherwise
/// approve the regression it contains. Per recognizer, because each has its own table and
/// two methods cannot share a state.
/// </para>
/// </remarks>
public sealed class MergingTests
{
	[Theory]
	[MemberData(nameof(Snapshots))]
	public void No_two_states_of_one_recognizer_do_the_same_thing(string name)
	{
		foreach (var (recognizer, states) in Recognizers(File.ReadAllText(Path.Combine(Directory, name))))
		{
			var same = states
				.GroupBy(static one => one.Does, StringComparer.Ordinal)
				.Where(static one => one.Count() > 1)
				.Select(one => $"{recognizer}: {string.Join(" and ", one.Select(static state => state.Name))}")
				.ToArray();

			Assert.True(
				same.Length == 0,
				"States that do the same thing were left apart: " + string.Join("; ", same));
		}
	}

	/// <summary>Each recognizer in a generated file, and the states written in it.</summary>
	static IEnumerable<(string Name, List<(string Name, string Does)> States)> Recognizers(string source)
	{
		var lines      = source.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
		var recognizer = "";
		var blocks     = new List<(string Name, List<string> Body)>();

		for (var i = 0; i < lines.Length; i++)
		{
			if (Method.Match(lines[i]) is { Success: true } method)
			{
				if (Told(blocks) is { Count: > 0 } states)
					yield return (recognizer, states);

				recognizer = method.Groups[1].Value;
				blocks     = [];

				continue;
			}

			if (Label.Match(lines[i]) is not { Success: true } label || Opens(lines, i + 1) is not { } opened)
				continue;

			var depth = 1;
			var body  = new List<string>();

			for (i = opened + 1; i < lines.Length && depth > 0; i++)
			{
				depth += lines[i].Count(static c => c == '{') - lines[i].Count(static c => c == '}');

				if (depth > 0 && lines[i].Trim().Length > 0)
					body.Add(lines[i].Trim());
			}

			blocks.Add((label.Groups[1].Value, body));
			i--;
		}

		if (Told(blocks) is { Count: > 0 } last)
			yield return (recognizer, last);
	}

	/// <summary>
	/// What each block does, for the blocks that say where they go on to.
	/// </summary>
	/// <remarks>
	/// A block that ends by jumping says everything about itself, and two of those that read
	/// the same are one state. A block that does not is the one place layout drops the jump,
	/// where it named whatever is written next — and what is written next may be a state with
	/// no label at all, because nothing names it. Reading the fall-through would mean tracking
	/// those too, and getting it slightly wrong means accusing two states that differ. So they
	/// are left out: this is a guard against merging stopping, and the blocks that end by
	/// jumping are most of them.
	/// </remarks>
	static List<(string Name, string Does)> Told(List<(string Name, List<string> Body)> blocks)
	{
		var told = new List<(string Name, string Does)>(blocks.Count);

		foreach (var (name, body) in blocks)
			if (body.Count > 0 && Goto.Match(body[^1]) is { Success: true } jump)
				told.Add((name, string.Join("\n", body[..^1]) + " -> " + jump.Groups[1].Value));

		return told;
	}

	/// <summary>Where a label's block opens, if the line after it opens one.</summary>
	static int? Opens(string[] lines, int at)
	{
		while (at < lines.Length && lines[at].Trim().Length == 0)
			at++;

		return at < lines.Length && lines[at].Trim() == "{" ? at : null;
	}

	static readonly Regex Method = new(@"^\s*static \w+ (Recognize_\w+)\(", RegexOptions.Compiled);
	static readonly Regex Label  = new(@"^\s*(S\d+):\s*$", RegexOptions.Compiled);
	static readonly Regex Goto   = new(@"^goto (\w+);$", RegexOptions.Compiled);

	public static TheoryData<string> Snapshots =>
		new(System.IO.Directory.GetFiles(Directory, "*.gram.g.cs").Select(Path.GetFileName).OfType<string>());

	static string Directory => Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!, "Snapshots");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
