using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every name a snapshot's class declares, handed back to the author: as a publication, as a rule (and so
/// a rule's type), as a member written by hand in the host class and as an external <c>@M</c> — and every
/// part of every such name, between its underscores, as a rule, since a rule's name is what the
/// generator's own names are made from. Each must compile, or be refused by GRAM5018 for a name the
/// generated class keeps whatever the grammar says.
/// </summary>
/// <remarks>
/// <para>
/// Not one compilation per name: the names of one kind go into one grammar together, and a failure is
/// read off the compiler's errors, each of which names what it is about. A name that holds the mark is
/// left out (GRAM1012 refuses it in a grammar, and the documented reservation covers a member written by
/// hand), and so is a name that is the author's own already: a publication's method, a rule's type. A
/// member written by hand under a name the class keeps (<c>Match</c>, <c>Failure</c>) is the documented
/// reservation too, and is left out of the hand-written forms.
/// </para>
/// <para>
/// An added rule or publication the grammar refuses for some other reason than its name (a rule called
/// <c>trivia</c> is the grammar's trivia, and may not take a capture) is dropped and counted, and the
/// test fails if it dropped most of what it added: it is the names that are under test.
/// </para>
/// </remarks>
public sealed class GeneratedNameMirrorTests
{
	[Theory]
	[MemberData(nameof(Snapshots))]
	public void Every_name_a_snapshot_declares_can_be_the_authors(string name)
	{
		var text = File.ReadAllText(Path.Combine(SourceRoot.Directory, "tests", "Snapshots", name + ".gram"));
		var (options, declarations) = SnapshotTests.Renderings(name, text).First();
		var result = GramCompiler.Compile(text, options);
		var names  = result.Names!;
		var author = GeneratedNameRule.Author(names);
		var trees  = result.Sources.Select(static one => CSharpSyntaxTree.ParseText(one.Text, cancellationToken: TestContext.Current.CancellationToken)).ToArray();
		var all    = GeneratedNameTests.Declarations(trees).Select(static one => one.Name).Distinct(StringComparer.Ordinal)
			.Where(static one => Regex.IsMatch(one, "^[A-Za-z_][A-Za-z0-9_]*$") && !one.Contains(CSharpEmitter.NameMark, StringComparison.Ordinal) && SyntaxFacts.GetKeywordKind(one) == SyntaxKind.None)
			.OrderBy(static one => one, StringComparer.Ordinal)
			.ToArray();
		var kept   = new HashSet<string>(
			CSharpEmitter.PublicNames.Concat(CSharpEmitter.SupportNames).Concat(CSharpEmitter.TraceNames).Concat(options.Lexical ? CSharpEmitter.LexerNames : []).Append(name),
			StringComparer.Ordinal);
		// The first rule the grammar publishes without arguments, or its first rule where it publishes none.
		var start  = Regex.Match(text, @"^\s*parse\s+((?:\w+\.)*\w+)(?!\w|\s*\()", RegexOptions.Multiline) is { Success: true } parsed
			? parsed.Groups[1].Value
			: Regex.Matches(text, @"^([A-Z]\w*)\s*[:=]", RegexOptions.Multiline).First().Groups[1].Value;
		var faults = new List<string>();

		Assert.NotEmpty(all);
		Assert.NotEmpty(start);

		// As a publication of a rule the grammar already publishes, which shares its machine.
		var published = all.Where(one => !author.Contains(one) && !author.Contains("Try" + one)).ToArray();

		foreach (var batch in new[] { published.Where(static one => !one.StartsWith("Try", StringComparison.Ordinal)), published.Where(static one => one.StartsWith("Try", StringComparison.Ordinal)) })
			faults.AddRange(Added(text, options, declarations, kept, batch.ToArray(), one => $"parse {start} as {one}", null, "publication"));

		// As a rule, and every part of every name as one: each is a type of its own, and each is reached.
		var parts = all
			.SelectMany(static one => Parts(one))
			.Concat(all)
			.Where(one => Regex.IsMatch(one, "^[A-Za-z_][A-Za-z0-9_]*$") && !one.Contains(CSharpEmitter.NameMark, StringComparison.Ordinal) && SyntaxFacts.GetKeywordKind(one) == SyntaxKind.None && one != "MirrorAll" && one != "MirrorProbe")

			// A rule the grammar has already is not added again: that is a second definition, not a name.
			.Where(one => !names.Identifiers.Contains(one) && !Regex.IsMatch(text, @"^[ \t]*" + one + @"[ \t]*[(:=]", RegexOptions.Multiline))
			.Distinct(StringComparer.Ordinal)
			.OrderBy(static one => one, StringComparer.Ordinal)
			.ToArray();

		faults.AddRange(Added(text, options, declarations, kept, parts, static one => $"{one} = mirror: \"<{one}>\"", "MirrorAll = ({0})\nparse MirrorAll as MirrorProbe", "rule"));

		// Written by hand in the host class, and called as an external recognizer, which buffered input
		// cannot call (GRAM4026).
		if (options.Suffix is null)
		{
			var hand = all.Where(one => !author.Contains(one) && !kept.Contains(one)).ToArray();

			faults.AddRange(Added(text, options, declarations, kept, hand, null, null, "member", static one => $"public static int {one} = 0;"));
			if (!options.BufferedInput && !options.BufferedBytes)
				faults.AddRange(Added(
					text, options, declarations, kept, hand, null, "MirrorAll = ({0})\nparse MirrorAll as MirrorProbe", "external",
					static one => $"public static bool {one}(global::System.ReadOnlySpan<char> input, ref int pos) {{ return false; }}",
					static one => "@" + one));
		}

		Assert.True(faults.Count == 0, $"{faults.Count} names an author may write do not compile:\n" + string.Join("\n", faults.Take(100)));
	}

	/// <summary>
	/// What goes wrong when a batch of names is given to the author one way: a GRAM5018 for a name the class
	/// does not keep, and every error the C# compiler finds. Names the grammar refuses for any other reason
	/// are dropped and the rest tried again.
	/// </summary>
	/// <param name="declare">The grammar line a name becomes, or null for none.</param>
	/// <param name="reach">A line reaching every added rule, <c>{0}</c> for the choice of them, or null.</param>
	/// <param name="member">The member a name becomes in the host class, or null for none.</param>
	/// <param name="called">What a name is in the reaching choice, where that is not the name itself.</param>
	static IEnumerable<string> Added(
		string text, GramCompilerOptions options, string? declarations, HashSet<string> kept, IReadOnlyList<string> names,
		Func<string, string>? declare, string? reach, string way, Func<string, string>? member = null, Func<string, string>? called = null)
	{
		var live    = names.ToList();
		var dropped = 0;
		var faults  = new List<string>();

		for (var round = 0; round < 20 && live.Count > 0; round++)
		{
			var grammar = new StringBuilder(text).Append("\n\n");
			var lines   = new Dictionary<int, string>();
			var line    = text.Count(static c => c == '\n') + 3;

			foreach (var one in live)
			{
				if (declare is null)
					break;

				grammar.Append(declare(one)).Append('\n');
				lines[line++] = one;
			}

			if (reach is not null)
				grammar.Append(string.Format(System.Globalization.CultureInfo.InvariantCulture, reach, string.Join(" | ", live.Select(one => called?.Invoke(one) ?? one)))).Append('\n');

			var source  = grammar.ToString();
			var result  = GramCompiler.Compile(source, options);
			var refused = new HashSet<string>(StringComparer.Ordinal);

			foreach (var diagnostic in result.Diagnostics.Where(static one => one.Severity == GramSeverity.Error))
			{
				var at    = source.Take(diagnostic.Position).Count(static c => c == '\n') + 1;
				var named = lines.TryGetValue(at, out var found)
					? found
					: live.FirstOrDefault(one => diagnostic.Message.Contains("'" + one + "'", StringComparison.Ordinal) || diagnostic.Message.Contains("'@" + one + "'", StringComparison.Ordinal));

				// Said of the name it is about, which is the surer of the two.
				if (diagnostic.Id == CSharpEmitter.GeneratedNameClash && live.FirstOrDefault(one => diagnostic.Message.StartsWith("'" + one + "'", StringComparison.Ordinal)) is { } meant)
					named = meant;

				if (named is null)
				{
					faults.Add($"{way}s: {diagnostic.Id} {diagnostic.Message}");

					continue;
				}

				if (diagnostic.Id == CSharpEmitter.GeneratedNameClash && !kept.Contains(named))
					faults.Add($"{way} {named}: {diagnostic.Message}");

				refused.Add(named);

				if (diagnostic.Id != CSharpEmitter.GeneratedNameClash)
					dropped++;
			}

			if (faults.Count > 0)
				return faults;

			if (refused.Count > 0)
			{
				live.RemoveAll(refused.Contains);

				continue;
			}

			Assert.True(dropped <= names.Count / 2, $"{way}s: {dropped} of {names.Count} refused for something other than their names");

			var written = string.Join("\n", (declarations is null ? [] : new[] { declarations }).Concat(member is null ? [] : live.Select(member)));
			var errors  = EmittedCode.Errors(result.Sources[0].Text, options.ClassName, options.Namespace, written, result.Sources.Skip(1).Select(static one => one.Text));

			return errors.Select(error => $"{way}s: {error}").Take(30);
		}

		return faults;
	}

	/// <summary>Every run of a name's underscore-separated parts, the whole excepted.</summary>
	static IEnumerable<string> Parts(string name)
	{
		var parts = name.Split('_');

		for (var from = 0; from < parts.Length; from++)
			for (var to = from + 1; to <= parts.Length; to++)
				if (to - from < parts.Length && parts.Skip(from).Take(to - from).All(static one => one.Length > 0))
					yield return string.Join("_", parts.Skip(from).Take(to - from));
	}

	public static TheoryData<string> Snapshots => SnapshotTests.Snapshots;
}
