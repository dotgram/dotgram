using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The language guide, <c>docs/guide/language</c>, compiled and run as it is written.
/// </summary>
/// <remarks>
/// <para>
/// Every <c>```dotgram</c> fence of a page is a whole grammar, and is compiled. The words after the
/// language in its info string say what the page claims about it: a diagnostic identifier
/// (<c>```dotgram GRAM4006</c>) says the grammar reports it, and nothing else at the level of a
/// warning or above; no identifier says it reports nothing at that level. <c>Lexical</c> compiles it
/// with <c>Lexical = true</c>, as the host attribute would. A grammar that reports no error is then
/// compiled as C#, and what the generator wrote has to compile without a warning. <c>pending</c>
/// marks a grammar that shows the behaviour of a fix not yet made, and is skipped until the word is
/// taken away; <c>recheck</c> marks one whose diagnostics a fix in flight may change, and is held to
/// what it says now.
/// </para>
/// <para>
/// A <c>```csharp</c> fence right after a grammar that declares a <c>partial class</c> is the host it is
/// attached to — the methods its <c>@</c> calls — and is compiled beside it; without one the host is
/// an empty <c>partial class Grammar</c>. A <c>```text examples</c> fence after that holds runs, one a
/// line: <c>Method("input") → value</c>, <c>→ refused</c>, or <c>→ refused: message</c>, which holds the
/// text of the refusal too. The method is called with the one string;
/// a <c>Match</c> it answers is read, a <c>FormatException</c> it throws is a refusal, and a sequence of
/// matches is their values. A value is shown as C# writes a literal of it (<c>"text"</c>, <c>'c'</c>,
/// <c>3</c>, <c>true</c>, <c>null</c>), a sequence as <c>[a, b]</c>, anything else by its <c>ToString</c>.
/// </para>
/// <para>
/// The Russian blocks between <c>&lt;!-- ru:begin --&gt;</c> and <c>&lt;!-- ru:end --&gt;</c> hold no
/// fences and are skipped regardless, so that removing them changes nothing here.
/// </para>
/// </remarks>
public sealed class LanguageGuideTests
{
	[Theory]
	[MemberData(nameof(Grammars))]
	public void A_grammar_of_the_guide_says_what_the_page_says(string page, int index)
	{
		var example  = All(page)[index];

		if (example.Words.Contains("pending"))
			Assert.Skip($"{page}, grammar {index + 1} (line {example.Line}) shows behaviour a fix has not landed yet.");

		var host     = example.Host is { } declared ? ShippedPages.HostIn(declared) ?? "Grammar" : "Grammar";
		var hostCode = example.Host ?? "partial class Grammar { }";

		var compiled = GramCompiler.Compile(example.Grammar, new GramCompilerOptions
		{
			ClassName      = host,
			Lexical        = example.Words.Contains("Lexical"),
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(
				CSharpCompilation.Create(
					host,
					[Parse(hostCode), Parse(GramCompiler.EmitMarkerAttributes().Text)],
					EmittedCode.References,
					new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)),
				host),
		});

		var expected = example.Words.Where(static word => word.StartsWith("GRAM", StringComparison.Ordinal)).ToHashSet();
		var reported = compiled.Diagnostics
			.Where(one => one.Severity != GramSeverity.Info || expected.Contains(one.Id))
			.ToList();

		var said = $"{page}, grammar {index + 1} (line {example.Line}):\n{example.Grammar}\n" +
			string.Join("\n", reported.Select(static one => $"{one.Id} {one.Severity}: {one.Message}"));

		Assert.True(expected.SetEquals(reported.Select(static one => one.Id)), said);

		if (reported.Any(static one => one.Severity == GramSeverity.Error))
		{
			Assert.Null(example.Runs);

			return;
		}

		var hostTree = Parse(hostCode);
		var built    = CSharpCompilation.Create(
			"DotGram.Tests.Guide",
			[
				hostTree,
				Parse(GramCompiler.EmitMarkerAttributes().Text),
				Parse(SupportEmitter.EmbeddedAttribute),
				.. compiled.Sources.Select(static one => Parse(one.Text)),
			],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		using var stream = new MemoryStream();

		var emitted = built.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);

		// The generated files are held to no warning, as a consumer who builds with warnings as errors
		// holds them; the host's own code is the page's, and only its errors count.
		var complaints = emitted.Diagnostics
			.Where(one => one.Severity == DiagnosticSeverity.Error ||
				one.Severity == DiagnosticSeverity.Warning && one.Location.SourceTree != hostTree)
			.ToList();

		Assert.True(complaints.Count == 0, said + "\n" + string.Join("\n", complaints.Select(static one => one.ToString())));

		if (example.Runs is null)
			return;

		var type = Assembly.Load(stream.ToArray()).GetType(host)!;

		foreach (var (call, input, answer) in example.Runs)
		{
			var got = Run(type, call, input);

			// A bare `refused` says only that the input is refused; `refused: …` says what the
			// refusal says, too.
			if (answer == "refused" && got.StartsWith("refused", StringComparison.Ordinal))
				got = "refused";

			Assert.Equal($"{call}({Literal(input)}) → {answer}", $"{call}({Literal(input)}) → {got}");
		}
	}

	/// <summary>Every grammar of every page of the guide, by its page and its place on it.</summary>
	public static TheoryData<string, int> Grammars
	{
		get
		{
			var data = new TheoryData<string, int>();

			foreach (var page in Pages())
				for (var at = 0; at < All(page).Count; at++)
					data.Add(page, at);

			return data;
		}
	}

	/// <summary>The guide has pages, and each of them has grammars: a page that lost its fences is a page that checks nothing.</summary>
	[Fact]
	public void The_guide_is_there()
	{
		var pages = Pages().ToList();

		Assert.NotEmpty(pages);

		foreach (var page in pages.Where(static one => !one.EndsWith("README.md", StringComparison.Ordinal)))
			Assert.True(All(page).Count > 0, $"{page} holds no grammar.");
	}

	static IEnumerable<string> Pages()
	{
		return Directory
			.EnumerateFiles(Path.Combine(SourceRoot.Directory, "docs", "guide", "language"), "*.md")
			.Select(static path => Path.GetRelativePath(SourceRoot.Directory, path).Replace('\\', '/'))
			.OrderBy(static path => path, StringComparer.Ordinal);
	}

	/// <summary>One grammar of a page, the host it is attached to, and the runs the page shows of it.</summary>
	sealed record Example(int Line, string[] Words, string Grammar, string? Host, List<(string Call, string Input, string Answer)>? Runs);

	static List<Example> All(string page)
	{
		var text   = File.ReadAllText(Path.Combine(SourceRoot.Directory, page)).Replace("\r\n", "\n", StringComparison.Ordinal);
		var blocks = Regex
			.Matches(text, "^```([^\n]*)\n(.*?)^```[ \t]*$", RegexOptions.Singleline | RegexOptions.Multiline)
			.Cast<Match>()
			.Where(one => !InRussian(text, one.Index))
			.Select(one => (
				Line: text.Take(one.Index).Count(static c => c == '\n') + 1,
				Words: one.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries),
				Text: one.Groups[2].Value))
			.ToList();

		var examples = new List<Example>();

		for (var at = 0; at < blocks.Count; at++)
		{
			if (blocks[at].Words is not ["dotgram", ..])
				continue;

			var    next = at + 1;
			string? host = null;

			if (next < blocks.Count && blocks[next].Words is ["csharp", ..] && ShippedPages.HostIn(blocks[next].Text) is not null)
				host = blocks[next++].Text;

			List<(string, string, string)>? runs = null;

			if (next < blocks.Count && blocks[next].Words is ["text", "examples", ..])
				runs = [.. blocks[next].Text.Split('\n').Where(static line => line.Trim().Length > 0).Select(Runs)];

			examples.Add(new Example(blocks[at].Line, blocks[at].Words[1..], blocks[at].Text, host, runs));
		}

		return examples;
	}

	/// <summary>Whether an offset of a page stands inside a Russian block.</summary>
	static bool InRussian(string text, int at)
	{
		var opens  = text.LastIndexOf("<!-- ru:begin -->", at, StringComparison.Ordinal);
		var closes = text.LastIndexOf("<!-- ru:end -->", at, StringComparison.Ordinal);

		return opens >= 0 && opens > closes;
	}

	/// <summary><c>Method("input") → answer</c>, the input a C# string literal.</summary>
	static (string, string, string) Runs(string line)
	{
		var shape = Regex.Match(line.Trim(), "^([A-Za-z_][A-Za-z0-9_]*)\\(\"((?:[^\"\\\\]|\\\\.)*)\"\\)\\s*→\\s*(.*)$");

		Assert.True(shape.Success, $"Not a run: '{line}'. Write Method(\"input\") → value.");

		return (shape.Groups[1].Value, Unescaped(shape.Groups[2].Value), shape.Groups[3].Value.Trim());
	}

	static string Unescaped(string text)
	{
		var built = new StringBuilder();

		for (var at = 0; at < text.Length; at++)
		{
			if (text[at] != '\\' || at + 1 == text.Length)
			{
				built.Append(text[at]);

				continue;
			}

			at++;

			if (text[at] == 'u' && at + 4 < text.Length)
			{
				built.Append((char)int.Parse(text.Substring(at + 1, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture));

				at += 4;

				continue;
			}

			built.Append(text[at] switch
			{
				'n' => '\n',
				'r' => '\r',
				't' => '\t',
				'0' => '\0',
				var other => other,
			});
		}

		return built.ToString();
	}

	static string Run(Type type, string call, string input)
	{
		var method = type.GetMethod(call, [typeof(string)]);

		Assert.True(method is not null, $"{type.Name} has no {call}(string).");

		object? answer;

		try
		{
			answer = method!.Invoke(null, [input]);
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is FormatException refused)
		{
			return "refused: " + refused.Message;
		}

		if (answer is not null && IsMatch(answer.GetType()))
			return Matched(answer);

		if (answer is IEnumerable sequence and not string)
		{
			var shown = new List<string>();

			foreach (var one in sequence)
				shown.Add(one is not null && IsMatch(one.GetType()) ? Matched(one) : Show(one));

			return "[" + string.Join(", ", shown) + "]";
		}

		return Show(answer);
	}

	static bool IsMatch(Type type)
	{
		return type.IsGenericType && type.Name.StartsWith("Match", StringComparison.Ordinal) && type.GetProperty("IsSuccess") is not null;
	}

	static string Matched(object match)
	{
		var type = match.GetType();

		return (bool)type.GetProperty("IsSuccess")!.GetValue(match)!
			? Show(type.GetProperty("Value")!.GetValue(match))
			: "refused: " + type.GetProperty("Error")!.GetValue(match);
	}

	static string Show(object? value)
	{
		return value switch
		{
			null         => "null",
			string text  => Literal(text),
			char one     => "'" + Literal(one.ToString())[1..^1] + "'",
			bool yes     => yes ? "true" : "false",
			IEnumerable sequence
			             => "[" + string.Join(", ", sequence.Cast<object?>().Select(Show)) + "]",
			IFormattable formattable
			             => formattable.ToString(null, CultureInfo.InvariantCulture),
			_ when Generated(value.GetType())
			             => Members(value),
			_            => value.ToString() ?? "",
		};
	}

	/// <summary>
	/// Whether a value is of a type the grammar made, which says nothing of itself: one of the run's
	/// own assembly whose <c>ToString</c> is <see cref="object"/>'s.
	/// </summary>
	static bool Generated(Type type)
	{
		return type.Assembly.GetName().Name == "DotGram.Tests.Guide" &&
			type.GetMethod(nameof(ToString), Type.EmptyTypes)!.DeclaringType == typeof(object);
	}

	/// <summary><c>Name { Member = value, … }</c>, the members in the order they are declared.</summary>
	static string Members(object value)
	{
		var members = value
			.GetType()
			.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.OrderBy(static one => one.MetadataToken)
			.Select(one => one.Name + " = " + Show(one.GetValue(value)));

		return value.GetType().Name + " { " + string.Join(", ", members) + " }";
	}

	static string Literal(string text)
	{
		return "\"" + text
			.Replace("\\", "\\\\", StringComparison.Ordinal)
			.Replace("\"", "\\\"", StringComparison.Ordinal)
			.Replace("\n", "\\n", StringComparison.Ordinal)
			.Replace("\r", "\\r", StringComparison.Ordinal)
			.Replace("\t", "\\t", StringComparison.Ordinal)
			.Replace("\0", "\\0", StringComparison.Ordinal) + "\"";
	}

	static SyntaxTree Parse(string code)
	{
		return CSharpSyntaxTree.ParseText(code, cancellationToken: TestContext.Current.CancellationToken);
	}
}
