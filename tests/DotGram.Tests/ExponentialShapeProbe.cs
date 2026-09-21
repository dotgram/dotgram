using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Where in this repository's grammars a repetition sits inside a repetition with nothing
/// between them that commits — the shape whose refusal costs two to the n.
/// </summary>
/// <remarks>
/// <para>
/// <b>A list of places, not of defects.</b> It says this shape is here; whether a given one
/// can be driven exponentially is decided by an input, not by reading. Two could (D57): a URI
/// template's <c>Part*</c> over <c>(LiteralChar | PctEncoded)+</c> refused a thirty-three
/// character input in three and a half seconds, and an address list took a hundred and seven
/// on twenty-eight. Both are atomic now, and the way to judge any other place on the list is
/// the same — drive that shape's own worst input through that parser and see how long the
/// refusal takes.
/// </para>
/// <para>
/// Read it beside the measuring stand's list of entry points that have no refusal ladder: a
/// place named here whose entry point is also unmeasured there is a shape nobody has tried to
/// break and nobody would notice breaking.
/// </para>
/// <para>
/// <b>It is calibrated, and that is why its output may be believed.</b> The first version of
/// this walk looked only at the outer repetition's own body, found neither of the two known
/// cases — the template's inner repetition is two calls away and behind a choice — and named
/// two harmless ones instead. A clean list from a detector nobody has calibrated is the most
/// expensive answer there is, because it stops the search. If this is ever changed, check
/// first that it still finds a known positive.
/// </para>
/// </remarks>
public sealed class ExponentialShapeProbe
{
	[Fact]
	public void List_every_repetition_inside_a_repetition()
	{
		var root  = Root();
		var told  = new StringBuilder();
		var seen  = 0;
		var found = 0;

		foreach (var (name, text) in Grammars(root))
		{
			RecognitionGraph graph;

			try
			{
				graph = GrammarNormalizer.Normalize(
					GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));
			}
			catch (Exception error)
			{
				told.AppendLine($"  (not read) {name}: {error.GetType().Name}");

				continue;
			}

			seen++;

			foreach (var rule in graph.Rules)
			{
				if (!graph.Bodies.TryGetValue(rule, out var body))
					continue;

				foreach (var place in Nested(body, graph, rule))
				{
					found++;
					told.AppendLine($"  {name}  {rule.Name}: {place}");
				}
			}
		}

		Assert.True(seen > 50, $"Only {seen} grammars were read; the walk over the repository is broken.");

		var actual   = told.ToString().Replace("\r\n", "\n");
		var expected = Path.Combine(root, "tests", "DotGram.Tests", "ExponentialShapes.txt");

		if (!File.Exists(expected))
		{
			File.WriteAllText(expected, actual);

			Assert.Fail($"No list yet; wrote {found} places to {expected}. Read it, and commit it if it is right.");
		}

		// The recorded file carries a verdict and its reason beside each place; only the places
		// themselves are compared, so a judgement can be written without the walk parsing it.
		var recorded = string.Concat(
			File.ReadAllLines(expected)
				.Where(line => !line.TrimStart().StartsWith("#", StringComparison.Ordinal) && line.Length > 0)
				.Select(line => line + "\n"));

		if (recorded == actual)
			return;

		var rejected = expected + ".actual";

		File.WriteAllText(rejected, actual);

		Assert.Fail(
			$"The places have changed: {found} now. Diff {expected} against {rejected}. " +
			"A place gone is a shape somebody sealed - take it out of the list. A place added is a " +
			"grammar that has grown one, and wants the same question asked of it: how long does its " +
			"worst refusal take?");
	}

	/// <summary>
	/// A repetition whose turn can begin with another unbounded repetition, with nothing after
	/// it in that turn that must be read: the outer can then cut one run of text in as many
	/// ways as the inner has lengths, and a refusal tries every one of them.
	/// </summary>
	/// <remarks>
	/// The inner repetition is almost never written inside the outer's own body. In the case
	/// this was built for it is two calls away and behind a choice — <c>Part*</c> over
	/// <c>Part</c> over <c>Literals</c> over <c>(LiteralChar | PctEncoded)+</c> — so the ways a
	/// turn can begin are followed through calls and alternatives. A first version of this probe
	/// looked only at the syntactic body, found neither of the two known cases, and named two
	/// harmless ones instead.
	/// </remarks>
	static IEnumerable<string> Nested(Node node, RecognitionGraph graph, RuleSymbol owner)
	{
		foreach (var outer in Walk(node, inAtomic: false))
		{
			if (outer is not Node.Repeat { Max: null } repeat)
				continue;

			foreach (var inner in Leading(repeat.Body, graph, []))
				yield return Describe(repeat) + " over " + Describe(inner) + " (" + Spelling(inner) + ")";
		}
	}

	/// <summary>
	/// Every unbounded repetition a turn can begin with and be finished by, following calls and
	/// alternatives: what follows it in the turn must be able to read nothing, or the turn is
	/// pinned by that and cuts one way.
	/// </summary>
	static IEnumerable<Node.Repeat> Leading(Node node, RecognitionGraph graph, HashSet<RuleSymbol> seen)
	{
		switch (Unwrapped(node))
		{
			case Node.Repeat { Max: null } inner when !FirstSets.Nullable(inner.Body, graph):
				yield return inner;

				break;

			case Node.Call(var called, _) when seen.Add(called) && graph.Bodies.TryGetValue(called, out var body):
				foreach (var one in Leading(body, graph, seen))
					yield return one;

				break;

			case Node.Choice(var alternatives):
				foreach (var alternative in alternatives)
					foreach (var one in Leading(alternative, graph, [.. seen]))
						yield return one;

				break;

			case Node.Sequence(var parts) when parts.Count > 0:
			{
				// Only where the rest of the turn can read nothing: `('_'* & Digit)*` is pinned by
				// the digit and cuts one way, while `((A | B)+)*` is not pinned at all.
				var rest = parts.Count == 1 ? null : new Node.Sequence([.. parts.Skip(1)]);

				if (rest is null || FirstSets.Nullable(rest, graph))
					foreach (var one in Leading(parts[0], graph, seen))
						yield return one;

				break;
			}
		}
	}

	/// <summary>What the inner repetition is written as, as far as one line can say it.</summary>
	static string Spelling(Node.Repeat inner) => Unwrapped(inner.Body) switch
	{
		Node.Choice(var alternatives) => alternatives.Count + " alternatives",
		Node.Call(var called, _)      => called.Name,
		var other                     => other.GetType().Name,
	};

	/// <summary>Every node, not entering an atomic group: what it seals cannot be re-cut.</summary>
	static IEnumerable<Node> Walk(Node node, bool inAtomic)
	{
		if (node is Node.Atomic)
			yield break;

		if (!inAtomic)
			yield return node;

		foreach (var child in Children(node))
			foreach (var one in Walk(child, inAtomic))
				yield return one;
	}

	static IEnumerable<Node> Children(Node node) => node switch
	{
		Node.Sequence(var parts)     => parts,
		Node.Choice(var choices)     => choices,
		Node.Repeat(var body, _, _)  => [body],
		Node.Capture(_, var held)    => [held],
		Node.Construct(var built, _) => [built],
		Node.Marked(var kept, _)     => [kept],
		Node.Lookahead(_, var seen)  => [seen],
		Node.Atomic(var kept)        => [kept],
		_                            => [],
	};

	static Node Unwrapped(Node node) => node switch
	{
		Node.Capture(_, var held)    => Unwrapped(held),
		Node.Construct(var built, _) => Unwrapped(built),
		Node.Marked(var kept, _)     => Unwrapped(kept),
		_                            => node,
	};

	/// <summary>Build output and the scratch directory, neither of which is the repository's.</summary>
	/// <remarks>
	/// Asked of the path <b>below the root</b> and not of the whole of it. A worktree may itself
	/// sit in a directory called <c>.work</c> -- this repository keeps them there -- and a test
	/// over the absolute path then calls every grammar in the repository scratch and reports the
	/// walk as broken. What is scratch is scratch relative to the repository, like everything
	/// else this walk decides.
	/// </remarks>
	static bool Scratch(string root, string path)
	{
		var below = path.Substring(root.Length);

		return
			below.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
			below.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal) ||
			below.Contains($"{Path.DirectorySeparatorChar}.work{Path.DirectorySeparatorChar}", StringComparison.Ordinal);
	}

	static string Describe(Node.Repeat repeat) =>
		(repeat.Min == 0 ? "(…)*" : "(…)+") + " of " + Unwrapped(repeat.Body).GetType().Name.Replace("Node+", "");

	/// <summary>Every grammar in the repository: the files, and the text of every attribute.</summary>
	/// <remarks>
	/// The repository's grammars, which is not the same as every grammar file on the disk. Build
	/// output is skipped for the obvious reason, and `.work` because it is scratch: a grammar left
	/// there by an experiment is nobody's to give a verdict on, and one of them makes this guard
	/// fail for the person who ran the experiment and pass for everybody else — which reads as a
	/// grammar that grew a shape and is a directory nobody meant to be read.
	/// </remarks>
	static IEnumerable<(string Name, string Text)> Grammars(string root)
	{
		foreach (var path in Directory.EnumerateFiles(root, "*.gram", SearchOption.AllDirectories))
			if (!Scratch(root, path))
				yield return (Path.GetFileName(path), File.ReadAllText(path));

		foreach (var path in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
		{
			if (Scratch(root, path))
				continue;

			var source = File.ReadAllText(path);

			foreach (Match match in Regex.Matches(source, "\\[Gram\\(\"\"\"\\r?\\n(.*?)\\r?\\n\\s*\"\"\"", RegexOptions.Singleline))
				yield return (
					Path.GetFileName(path),
					string.Join("\n", match.Groups[1].Value.Split('\n').Select(one => one.TrimStart('\t'))));
		}
	}

	static string Root()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !Directory.Exists(Path.Combine(at.FullName, "src", "DotGram")))
			at = at.Parent;

		Assert.NotNull(at);

		return at!.FullName;
	}
}
