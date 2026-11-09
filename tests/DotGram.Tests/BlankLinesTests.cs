using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using static System.IO.SearchOption;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The blank lines between statements of the handwritten C#, held to
/// <c>docs/coding-conventions.md</c> ("Blank lines between statements") and to a baseline of what the
/// sources already did otherwise, <c>BlankLinesBaseline.txt</c>.
/// </summary>
/// <remarks>
/// <para>
/// Four things are counted, file by file, over every handwritten <c>.cs</c> under <c>src/</c> and
/// <c>tests/</c>, from the syntax tree and its trivia alone:
/// </para>
/// <list type="bullet">
/// <item><c>declarations</c>: a run of local declarations that touches a neighbouring statement which is not
/// a declaration, with no blank line between them — except one declaration of one variable directly before
/// the <c>return</c> after it;</item>
/// <item><c>control</c>: an <c>if</c>, <c>for</c>, <c>foreach</c>, <c>while</c>, <c>do</c>, <c>switch</c>,
/// <c>using</c>, <c>lock</c>, <c>try</c>, <c>checked</c>, <c>unsafe</c>, <c>fixed</c> or a bare block with a
/// neighbouring statement and no blank line between them;</item>
/// <item><c>short-if-group</c>: the same for a group of short <c>if</c>s — an <c>if</c> with no <c>else</c>
/// and no braces whose single statement is on its line or the next — which may stand together with no
/// blank line inside, and the group as a whole apart; a block that holds nothing but the group and a
/// <c>break</c> or <c>return</c> after it, a case's body say, needs no blank line before that;</item>
/// <item><c>directives</c>: an <c>#if</c> with no blank line above it, or an <c>#endif</c> with none below,
/// in front of a statement — except directly after a <c>{</c> or a case label, or directly before a
/// <c>}</c>.</item>
/// </list>
/// <para>
/// Only neighbours in one statement list are compared, so the first statement after a <c>{</c> and the
/// last before a <c>}</c> need nothing, and <c>else</c>, <c>catch</c> and <c>finally</c>, which are no
/// statements of their own, are never counted. A comment directly above a statement belongs to it: the
/// blank line goes before the comment, and a comment with no blank line above it is no separation, nor is an
/// <c>#endif</c>.
/// </para>
/// <para>
/// The baseline holds a count per file and rule, not lines, which move with every edit. A file that does
/// worse than its count fails with every place it breaks the rule; a file that does better fails too,
/// until the baseline is written again, so that the count can only come down:
/// <c>DOTGRAM_RECORD_BLANK_LINES_BASELINE=1 dotnet test tests/DotGram.Tests -c Release --filter BlankLinesTests</c>.
/// </para>
/// </remarks>
public sealed class BlankLinesTests(ITestOutputHelper output)
{
	const string BaselineName = "BlankLinesBaseline.txt";
	const string RecordName   = "DOTGRAM_RECORD_BLANK_LINES_BASELINE";

	static readonly string[] Roots = ["src", "tests"];

	[Fact]
	public void No_file_breaks_the_blank_line_rule_more_than_its_baseline_says()
	{
		var found = Scan(SourceRoot.Directory);

		if (Environment.GetEnvironmentVariable(RecordName) is { Length: > 0 })
		{
			Record(found);

			return;
		}

		var baseline = Read();
		var faults   = new List<string>();
		var counted  = found
			.GroupBy(static one => (one.File, one.Rule))
			.ToDictionary(static group => group.Key, static group => group.ToList());

		foreach (var (key, places) in counted.OrderBy(static one => Order(one.Key), StringComparer.Ordinal))
		{
			var then = baseline.GetValueOrDefault(key);

			if (places.Count > then)
				faults.Add(
					$"{key.File}: {places.Count} breaks of `{key.Rule}` where the baseline has {then}:" +
					string.Concat(places.Select(static one => $"\n  {one.File}:{one.Line}: {one.What}")));
		}

		foreach (var (key, then) in baseline.OrderBy(static one => Order(one.Key), StringComparer.Ordinal))
		{
			var now = counted.TryGetValue(key, out var places) ? places.Count : 0;

			if (now < then)
				faults.Add(
					$"{key.File}: {now} breaks of `{key.Rule}` where the baseline has {then}. " +
					$"Lower the baseline: {RecordName}=1 and run this test again.");
		}

		foreach (var fault in faults)
			output.WriteLine(fault);

		Assert.True(faults.Count == 0, string.Join(Environment.NewLine, faults));
	}

	/// <summary>The rule's own example, and the shapes around it, counted as the rule says.</summary>
	[Fact]
	public void The_rule_counts_what_the_conventions_say()
	{
		const string source =
			"""
			class C
			{
				void Example(string? a, string? b, int n)
				{
					Prepare();
					Load();

					if (a is null)  return;
					if (b is null)  Fail();
					if (n < 0)      n = 0;

					Write(a);
					Write(b);
				}

				void Guards(string? a, string? b)
				{
					if (a is null)
						return;
					if (b is null)
						return;

					var c = a + b;
					var d = c.Length;

					Write(c);

					try
					{
						Write(d.ToString());
					}
					catch
					{
						Fail();
					}
					finally
					{
						Load();
					}

					// The comment belongs to the loop: the blank line goes above it.
					foreach (var e in c)
						Write(e.ToString());
				}

				int One(string a)
				{
					var b = a.Length;
					return b;
				}

				void Case(int k)
				{
					switch (k)
					{
						case 1:
							if (k > 0) Fail();
							break;

						case 2:
							if (k > 1)
								return;
							if (k > 2)
								Fail();
							return;
					}

					Load();

			#if DEBUG
					Fail();
			#endif

					Load();
				}

				void Prepare() { }
				void Load() { }
				void Fail() { }
				void Write(string s) { }
			}
			""";

		Assert.Equal([], Breaks(source));

		const string broken =
			"""
			class C
			{
				void Broken(string? a, int n)
				{
					Prepare();
					var b = a;
					if (b is null)
						return;
					Write(b);
					// A comment is no separation.
					foreach (var c in b)
						Write(c.ToString());

					if (a is null) return;
					if (n < 0)     n = 0;
					Write(a);
					while (n > 0)
					{
						n--;
					}
					Load();
				}

				int Two(string a)
				{
					var b = a.Length;
					var c = b + 1;
					return c;
				}

				void Case(int k)
				{
					switch (k)
					{
						case 1:
							Load();
							if (k > 0) Load();
							break;
					}

					{
						Load();
					}
					Load();
					foreach (var c in "ab") Load();
					Load();
			#if DEBUG
					Load();
			#endif
					Load();
				}

				void Prepare() { }
				void Load() { }
				void Write(string s) { }
			}
			""";

		Assert.Equal(
			[
				"6 declarations",
				"7 short-if-group",
				"9 short-if-group",
				"11 control",
				"16 short-if-group",
				"17 control",
				"21 control",
				"28 declarations",
				"37 short-if-group",
				"38 short-if-group",
				"44 control",
				"45 control",
				"46 control",
				"47 directives",
				"49 directives",
			],
			Breaks(broken));

		static string[] Breaks(string text)
		{
			var breaks = Check("C.cs", CSharpSyntaxTree.ParseText(text));

			return [.. breaks.OrderBy(static one => one.Line).Select(static one => $"{one.Line} {one.Rule}")];
		}
	}

	readonly record struct Break(string File, string Rule, int Line, string What);

	enum Kind { Declaration, Control, ShortIf, Other, Neutral }

	/// <summary>Every handwritten file under the roots, each checked.</summary>
	static List<Break> Scan(string root)
	{
		var found   = new List<Break>();
		var options = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var paths   = Roots.SelectMany(one => Directory.EnumerateFiles(Path.Combine(root, one), "*.cs", AllDirectories));

		foreach (var path in paths)
		{
			var file = Path.GetRelativePath(root, path).Replace('\\', '/');

			if (IsEmitted(file, path))
				continue;

			found.AddRange(Check(file, CSharpSyntaxTree.ParseText(File.ReadAllText(path), options, path)));
		}

		return found;
	}

	/// <summary>
	/// Whether a file is written by a tool rather than by hand: build output, a generator's output beside its
	/// grammar or template, or a file that says so in its first lines.
	/// </summary>
	static bool IsEmitted(string file, string path)
	{
		if (file.Contains("/obj/", StringComparison.Ordinal) || file.Contains("/bin/", StringComparison.Ordinal))
			return true;

		if (file.EndsWith(".g.cs",        StringComparison.Ordinal) ||
			file.EndsWith(".generate.cs", StringComparison.Ordinal) ||
			file.EndsWith(".Table.cs",    StringComparison.Ordinal))
			return true;

		// Every version of FIX is written by generate.cs from the FIX repository.
		if (file.StartsWith("src/DotGram.Finance/Fix/Fix", StringComparison.Ordinal) && file.Count(static c => c == '/') > 3)
			return true;

		using var reader = new StreamReader(path);

		for (var line = 0; line < 8 && reader.ReadLine() is { } text; line++)
		{
			if (text.Contains("<auto-generated",    StringComparison.Ordinal) ||
				text.Contains("not edited by hand", StringComparison.Ordinal))
				return true;
		}

		return false;
	}

	/// <summary>Every statement list of a file, its neighbours compared.</summary>
	static IEnumerable<Break> Check(string file, SyntaxTree tree)
	{
		var text = tree.GetText();

		foreach (var node in tree.GetRoot().DescendantNodes())
		{
			var statements = node switch
			{
				BlockSyntax         block   => block.Statements,
				SwitchSectionSyntax section => section.Statements,
				_                           => default,
			};

			for (var at = 0; at < statements.Count; at++)
			{
				foreach (var directive in Directives(statements[at], text))
					yield return directive with { File = file };
			}

			for (var at = 1; at < statements.Count; at++)
			{
				var before = statements[at - 1];
				var after  = statements[at];

				if (BlankBefore(after) || Excused(statements, at))
					continue;

				if (Rule(KindOf(before), KindOf(after)) is not { } rule)
					continue;

				var line = after.GetFirstToken().GetLocation().GetLineSpan().StartLinePosition.Line + 1;

				var what = $"{Describe(before)} then {Describe(after)} with no blank line between";

				yield return new Break(file, rule, line, what);
			}
		}
	}

	/// <summary>
	/// Whether the statement at <paramref name="at"/> may touch the one before it though their kinds say otherwise:
	/// one declaration of one variable and the <c>return</c> after it, or the <c>break</c> or <c>return</c> that
	/// ends a block holding nothing but short <c>if</c>s before it.
	/// </summary>
	static bool Excused(SyntaxList<StatementSyntax> statements, int at)
	{
		var after = statements[at];

		var before = statements[at - 1];

		if (after is ReturnStatementSyntax && before is LocalDeclarationStatementSyntax { Declaration.Variables.Count: 1 })
			return at == 1 || statements[at - 2] is not LocalDeclarationStatementSyntax || BlankBefore(before);

		if (after is BreakStatementSyntax or ReturnStatementSyntax && at == statements.Count - 1)
			return statements.Take(at).All(static one => KindOf(one) == Kind.ShortIf);

		return false;
	}

	/// <summary>
	/// The <c>#if</c>s with no blank line above and the <c>#endif</c>s with none below, of those in front of a
	/// statement. The line above may also open the block or be a case label; the line below may close the block
	/// or be another directive.
	/// </summary>
	static IEnumerable<Break> Directives(StatementSyntax statement, SourceText text)
	{
		foreach (var trivia in statement.GetFirstToken().LeadingTrivia)
		{
			var kind = trivia.Kind();

			if (kind is not (SyntaxKind.IfDirectiveTrivia or SyntaxKind.EndIfDirectiveTrivia))
				continue;

			var line = text.Lines.GetLineFromPosition(trivia.SpanStart).LineNumber;

			if (kind == SyntaxKind.IfDirectiveTrivia)
			{
				var above = line > 0 ? text.Lines[line - 1].ToString().Trim() : "";

				if (above.Length > 0 && !above.EndsWith('{') && !above.EndsWith(':') && !above.StartsWith('#'))
					yield return new Break("", "directives", line + 1, "`#if` with no blank line above it");
			}
			else
			{
				var below = line + 1 < text.Lines.Count ? text.Lines[line + 1].ToString().Trim() : "";

				if (below.Length > 0 && !below.StartsWith('}') && !below.StartsWith('#'))
					yield return new Break("", "directives", line + 1, "`#endif` with no blank line below it");
			}
		}
	}

	/// <summary>What two neighbours with no blank line between them break, or null where they may touch.</summary>
	static string? Rule(Kind before, Kind after)
	{
		if (before == Kind.Neutral || after == Kind.Neutral)
			return null;

		if (before == Kind.ShortIf && after == Kind.ShortIf)
			return null;

		if (before == Kind.ShortIf || after == Kind.ShortIf)
			return before is Kind.Control || after is Kind.Control ? "control" : "short-if-group";

		if (before == Kind.Control || after == Kind.Control)
			return "control";

		if ((before == Kind.Declaration) != (after == Kind.Declaration))
			return "declarations";

		return null;
	}

	static Kind KindOf(StatementSyntax statement)
	{
		return statement switch
		{
			LocalDeclarationStatementSyntax                      => Kind.Declaration,
			IfStatementSyntax shortIf when IsShort(shortIf)      => Kind.ShortIf,
			LocalFunctionStatementSyntax or EmptyStatementSyntax => Kind.Neutral,

			IfStatementSyntax  or ForStatementSyntax     or CommonForEachStatementSyntax or WhileStatementSyntax or
			DoStatementSyntax  or SwitchStatementSyntax  or UsingStatementSyntax         or LockStatementSyntax or
			TryStatementSyntax or CheckedStatementSyntax or UnsafeStatementSyntax        or FixedStatementSyntax or
			BlockSyntax
				=> Kind.Control,

			_ => Kind.Other,
		};
	}

	/// <summary>
	/// An <c>if</c> with no <c>else</c> and no braces, whose condition is one line and whose single statement,
	/// itself no control statement, is one line on the condition's line or the next.
	/// </summary>
	static bool IsShort(IfStatementSyntax statement)
	{
		if (statement.Else is not null || statement.Statement is BlockSyntax)
			return false;

		if (KindOf(statement.Statement) is Kind.Control or Kind.ShortIf)
			return false;

		var condition = statement.GetLocation().GetLineSpan().StartLinePosition.Line;
		var closing   = statement.CloseParenToken.GetLocation().GetLineSpan().StartLinePosition.Line;
		var body      = statement.Statement.GetLocation().GetLineSpan();

		return closing == condition &&
			body.StartLinePosition.Line == body.EndLinePosition.Line &&
			body.StartLinePosition.Line - condition <= 1;
	}

	/// <summary>
	/// Whether the statement's leading trivia holds a blank line: an end of line with nothing but whitespace
	/// before it on its line. The end of the line the statement before ends on is that statement's trailing
	/// trivia, so whatever leads this one starts a line of its own.
	/// </summary>
	static bool BlankBefore(StatementSyntax statement)
	{
		var empty = true;

		foreach (var trivia in statement.GetFirstToken().LeadingTrivia)
		{
			switch (trivia.Kind())
			{
				case SyntaxKind.WhitespaceTrivia:
					break;

				case SyntaxKind.EndOfLineTrivia:
					if (empty)
						return true;

					empty = true;
					break;

				default:
					// A directive carries the end of its line inside it; a comment does not.
					empty = trivia.IsDirective;
					break;
			}
		}

		return false;
	}

	static string Describe(StatementSyntax statement)
	{
		return statement switch
		{
			LocalDeclarationStatementSyntax                 => "a declaration",
			IfStatementSyntax shortIf when IsShort(shortIf) => "a short `if`",
			ExpressionStatementSyntax                       => "a statement",
			_                                               => $"`{statement.GetFirstToken().ValueText}`",
		};
	}

	/// <summary>A file's counts in the order of its path, then of the rule's name.</summary>
	static string Order((string File, string Rule) key)
	{
		return key.File + " " + key.Rule;
	}

	static string Baseline()
	{
		return Path.Combine(SourceRoot.Directory, "tests", "DotGram.Tests", BaselineName);
	}

	static Dictionary<(string File, string Rule), int> Read()
	{
		var counts = new Dictionary<(string File, string Rule), int>();

		foreach (var line in File.ReadLines(Baseline()))
		{
			if (line.Length == 0 || line[0] == '#')
				continue;

			var parts = line.Split(' ');

			counts[(parts[0], parts[1])] = int.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture);
		}

		return counts;
	}

	static void Record(List<Break> found)
	{
		var text = new StringBuilder();

		text.Append("# Breaks of the blank-line rule (docs/coding-conventions.md) the sources had\r\n");
		text.Append("# when it was written: `path rule count`.\r\n");
		text.Append("# BlankLinesTests holds every file to its counts and fails when one goes up or comes down.\r\n");
		text.Append("# Written by the test, not by hand: DOTGRAM_RECORD_BLANK_LINES_BASELINE=1.\r\n");

		var groups = found
			.GroupBy(static one => (one.File, one.Rule))
			.OrderBy(static one => Order(one.Key), StringComparer.Ordinal);

		foreach (var group in groups)
			text.Append($"{group.Key.File} {group.Key.Rule} {group.Count()}\r\n");

		File.WriteAllText(Baseline(), text.ToString(), new UTF8Encoding(false));
	}
}
