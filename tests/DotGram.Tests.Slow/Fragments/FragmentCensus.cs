using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using DotGram.Grammar;
using DotGram.Grammar.Fragments;
using DotGram.Grammar.Parsing;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Tests.Fragments;

/// <summary>
/// Every host fragment of the grammars the repository ships, and of the standard library, read
/// into the fragment tree: how many are portable as written, and which forms keep the others C#'s.
/// </summary>
/// <remarks>
/// <para>
/// The grammars are every <c>.gram</c> file and every grammar written into a <c>[Gram("…")]</c>
/// attribute under <c>src</c> and <c>examples</c>, grouped by what ships them; a new grammar joins
/// without anyone adding it here. The standard library is read as a build reads it: spliced onto an
/// author's text and delimited by its own scanner.
/// </para>
/// <para>
/// Compiled into DotGram.Tests, under the oldest Roslyn the generator loads into, and into
/// DotGram.Cli.Tests, under the newest, which the tool carries: the two runs must give the same
/// table, since the lowering fixes the language version it parses with. That table is checked in
/// (<see cref="CommittedPath"/>) and both runs are held to it, so a change to what is portable, and
/// a difference between the two Roslyns, each show up as a failure and as a diff.
/// </para>
/// </remarks>
static class FragmentCensus
{
	public sealed record Found(string Group, string Place, int Position, string Text);

	public static IReadOnlyList<Found> Fragments()
	{
		return _fragments ??= Read(Root());
	}

	static IReadOnlyList<Found>? _fragments;

	/// <summary>The environment variable that makes a run write the census instead of comparing with it.</summary>
	public const string WriteVariable = "DOTGRAM_WRITE_FRAGMENT_CENSUS";

	/// <summary>The census as checked in: what both Roslyns must read the shipped fragments as.</summary>
	public static string CommittedPath()
	{
		return Path.Combine(Root(), "tests", "DotGram.Tests.Slow", "Fragments", "FragmentCensus.md");
	}

	/// <summary>
	/// How the census read here differs from the one checked in, or <c>null</c> where it does not:
	/// the first line that differs, and how to write the file anew.
	/// </summary>
	public static string? Disagreement(string table)
	{
		var path = CommittedPath();

		if (Environment.GetEnvironmentVariable(WriteVariable) == "1")
		{
			File.WriteAllText(path, table.Replace("\n", "\r\n"), new UTF8Encoding(false));

			return null;
		}

		var committed = File.Exists(path) ? File.ReadAllText(path).Replace("\r\n", "\n") : "";
		var expected  = committed.Split('\n');
		var actual    = table.Split('\n');

		for (var i = 0; i < Math.Max(expected.Length, actual.Length); i++)
		{
			var was = i < expected.Length ? expected[i] : "<end>";
			var now = i < actual.Length   ? actual[i]   : "<end>";

			if (was != now)
				return $"The fragment census read here differs from {path} at line {i + 1}:\n  checked in: {was}\n  read here:  {now}\n" +
					$"If the change is meant, write the file anew with {WriteVariable}=1 in DotGram.Tests.Slow and commit it.";
		}

		return null;
	}

	/// <summary>
	/// The census as text: one table of fragments per group, one of the forms that block them.
	/// </summary>
	/// <remarks>
	/// A fragment is counted once under every form it holds ("holding it"), and once more under a
	/// form when that is the only form it holds ("alone"): what would become portable if that one
	/// form were.
	/// </remarks>
	public static string Table(IReadOnlyList<Found> fragments)
	{
		var lowered = fragments.Select(static found => (found.Group, Lowered: RoslynFragmentLowering.Lower(found.Text))).ToList();
		var groups  = lowered.Select(static one => one.Group).Distinct().OrderBy(Order).ThenBy(static name => name, StringComparer.Ordinal).ToList();
		var text    = new StringBuilder();

		text.Append("| Grammars | Fragments | Portable as written | Of them with no call, member or `new` | Not portable |\n");
		text.Append("| --- | ---: | ---: | ---: | ---: |\n");

		foreach (var group in groups.Append("All"))
		{
			var these    = lowered.Where(one => group == "All" || one.Group == group).ToList();
			var portable = these.Count(static one => one.Lowered.IsPortable);
			var own      = these.Count(static one => one.Lowered.IsPortable && !Reaches(one.Lowered.Root));

			text.Append($"| {group} | {these.Count} | {portable} ({Percent(portable, these.Count)}) | {own} | {these.Count - portable} |\n");
		}

		var forms = lowered
			.Where(static one => !one.Lowered.IsPortable)
			.SelectMany(static one => one.Lowered.Refusals.Select(static refusal => refusal.Form).Distinct().Select(form => (Form: form, one.Group, Alone: one.Lowered.Refusals.All(other => other.Form == form))))
			.GroupBy(static one => one.Form)
			.Select(static form => (
				Form:    form.Key,
				Holding: form.Count(),
				Alone:   form.Count(static one => one.Alone),
				Where:   string.Join(", ", form.GroupBy(static one => one.Group).OrderBy(static group => Order(group.Key)).ThenBy(static group => group.Key, StringComparer.Ordinal).Select(static group => $"{group.Key} {group.Count()}"))))
			.OrderByDescending(static form => form.Holding)
			.ThenBy(static form => form.Form, StringComparer.Ordinal);

		text.Append('\n');
		text.Append("| Form | Fragments holding it | Alone | Where |\n");
		text.Append("| --- | ---: | ---: | --- |\n");

		foreach (var form in forms)
			text.Append($"| {form.Form} | {form.Holding} | {form.Alone} | {form.Where} |\n");

		text.Append('\n');
		text.Append($"Verdicts: {Digest(fragments, lowered.Select(static one => one.Lowered).ToList())}\n");

		return text.ToString();

		static string Percent(int part, int whole)
		{
			return whole == 0 ? "-" : $"{100.0 * part / whole:0.0} %";
		}
	}

	/// <summary>
	/// Whether the fragment reaches host code: a call, a member read or a construction, each of
	/// which another language spells only through a map or the printer's convention.
	/// </summary>
	static bool Reaches(Fragment fragment)
	{
		return fragment switch
		{
			Fragment.Call or Fragment.Member or Fragment.New => true,
			Fragment.Binary binary         => Reaches(binary.Left) || Reaches(binary.Right),
			Fragment.Not not               => Reaches(not.Operand),
			Fragment.Coalesce coalesce     => Reaches(coalesce.Left) || Reaches(coalesce.Right),
			Fragment.Conditional choice    => Reaches(choice.Condition) || Reaches(choice.WhenTrue) || Reaches(choice.WhenFalse),
			Fragment.IsNull test           => Reaches(test.Operand),
			Fragment.Parenthesized inner   => Reaches(inner.Inner),
			_                              => false,
		};
	}

	/// <summary>
	/// One digest of every fragment's place and of the forms refused in it, so that two readings
	/// that agree on the counts but not fragment by fragment are told apart.
	/// </summary>
	static string Digest(IReadOnlyList<Found> fragments, IReadOnlyList<LoweredFragment> lowered)
	{
		var text = new StringBuilder();

		for (var i = 0; i < fragments.Count; i++)
			text.Append(fragments[i].Place).Append('@').Append(fragments[i].Position).Append(':')
				.Append(string.Join(",", lowered[i].Refusals.Select(static one => $"{one.Form}+{one.Span.Start}+{one.Span.Length}"))).Append('\n');

		using var sha = SHA256.Create();

		return string.Concat(sha.ComputeHash(Encoding.UTF8.GetBytes(text.ToString())).Take(8).Select(static b => b.ToString("x2")));
	}

	/// <summary>
	/// The order the groups are listed in: the packages, then the examples and the library.
	/// </summary>
	static int Order(string group)
	{
		return group switch
		{
			"T-SQL"               => 0,
			"SQL standard"        => 1,
			"SQL-92"              => 2,
			"PostgreSQL"          => 3,
			"Web"                 => 4,
			"FIX"                 => 5,
			"Expression language" => 6,
			"CLI"                 => 7,
			"Examples"            => 8,
			"Std"                 => 9,
			_                     => 10,
		};
	}

	static IReadOnlyList<Found> Read(string root)
	{
		var result = new List<Found>();

		foreach (var (place, text) in Grammars(root))
		{
			var group = Group(place);

			foreach (var token in GramLexer.Tokenize(text, RoslynCSharpScanner.Instance).Tokens)
				if (token.Kind == TokenKind.HostExpression)
					result.Add(new Found(group, place, token.Position, token.Value!));
		}

		// The library is read where a build reads it, spliced after the author's text and
		// delimited by its own scanner, which knows no C#: what the tree is lowered from is the text
		// between `@(` and `)`, whoever found the `)`.
		const string own = "Own = 'a'\n";

		var spliced = StandardLibrary.SplicedOnto(own);

		foreach (var token in GramLexer.Tokenize(spliced, RoslynCSharpScanner.Instance, own.Length, StandardLibrary.Scanner).Tokens)
			if (token.Kind == TokenKind.HostExpression && token.Position >= own.Length)
				result.Add(new Found("Std", "src/DotGram/Grammar/Std.gram", token.Position - own.Length, token.Value!));

		// In an order no file system chooses.
		return [.. result.OrderBy(static found => found.Place, StringComparer.Ordinal).ThenBy(static found => found.Position)];
	}

	static string Group(string place)
	{
		if (place.StartsWith("examples/", StringComparison.Ordinal))
			return "Examples";

		if (place.StartsWith("src/DotGram.Sql/TransactSql/", StringComparison.Ordinal))
			return "T-SQL";

		if (place.StartsWith("src/DotGram.Sql/Standard/SqlStandard92", StringComparison.Ordinal))
			return "SQL-92";

		if (place.StartsWith("src/DotGram.Sql/Standard/SqlStandard", StringComparison.Ordinal))
			return "SQL standard";

		if (place.StartsWith("src/DotGram.Sql/PostgreSql/", StringComparison.Ordinal))
			return "PostgreSQL";

		if (place.StartsWith("src/DotGram.Web/", StringComparison.Ordinal))
			return "Web";

		if (place.StartsWith("src/DotGram.Finance/", StringComparison.Ordinal))
			return "FIX";

		if (place.StartsWith("src/DotGram.ExpressionLanguage/", StringComparison.Ordinal))
			return "Expression language";

		if (place.StartsWith("src/DotGram.Cli/", StringComparison.Ordinal))
			return "CLI";

		return place;
	}

	/// <summary>
	/// Every grammar under <c>src</c> and <c>examples</c> but the standard library, which
	/// <see cref="Read"/> reads the way a build does.
	/// </summary>
	static IEnumerable<(string Place, string Text)> Grammars(string root)
	{
		foreach (var file in Files(root, "*.gram"))
			if (Relative(root, file) != "src/DotGram/Grammar/Std.gram")
				yield return (Relative(root, file), File.ReadAllText(file));

		foreach (var file in Files(root, "*.cs"))
		{
			var source = File.ReadAllText(file);

			if (source.IndexOf("Gram", StringComparison.Ordinal) < 0)
				continue;

			var index = 0;

			foreach (var attribute in CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<AttributeSyntax>())
			{
				var name = attribute.Name switch
				{
					QualifiedNameSyntax qualified   => qualified.Right.Identifier.ValueText,
					AliasQualifiedNameSyntax global => global.Name.Identifier.ValueText,
					SimpleNameSyntax simple         => simple.Identifier.ValueText,
					_                               => "",
				};

				if (name is not ("Gram" or "GramAttribute") ||
					attribute.ArgumentList?.Arguments.FirstOrDefault() is not { NameEquals: null } argument)
					continue;

				var place = $"{Relative(root, file)}#{++index}";

				if (Constant(argument.Expression) is not { } grammar)
					throw new InvalidOperationException($"{place}: a [Gram] whose grammar is no constant the census can read.");

				// A file's name, which the grammar of that file stands for.
				if (grammar.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) && grammar.IndexOf('\n') < 0)
					continue;

				yield return (place, grammar);
			}
		}
	}

	/// <summary>
	/// The text of a constant string: a literal, or literals joined with <c>+</c>.
	/// </summary>
	static string? Constant(ExpressionSyntax expression)
	{
		return expression switch
		{
			LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression) => literal.Token.ValueText,
			ParenthesizedExpressionSyntax parenthesized => Constant(parenthesized.Expression),
			BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AddExpression) &&
				Constant(binary.Left) is { } left && Constant(binary.Right) is { } right => left + right,
			_ => null,
		};
	}

	static IEnumerable<string> Files(string root, string pattern)
	{
		foreach (var top in new[] { "src", "examples" })
			foreach (var file in Directory.EnumerateFiles(Path.Combine(root, top), pattern, SearchOption.AllDirectories))
			{
				if (Relative(root, file).Split('/').Any(static part => part is "bin" or "obj" or ".build"))
					continue;

				yield return file;
			}
	}

	static string Relative(string root, string file)
	{
		return Path.GetRelativePath(root, file).Replace('\\', '/');
	}

	static string Root()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? throw new InvalidOperationException("No DotGram.slnx above " + AppContext.BaseDirectory + ".");
	}
}
