using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;

using DotGram.Finance.Fix.Fix44;
using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// FIX's grammar, read with the positional follow told apart, answers as with the positional
/// follow alone: the counterpart of <see cref="PositionalSplitWebTests"/> for the one shipped
/// grammar with a <c>context</c>.
/// </summary>
/// <remarks>
/// The grammar is read from its file and compiled here with the code beside it, against the
/// package for everything else; the assembly takes the name the package lets see inside it. Its
/// texts are the Finance tests' messages, with the log's bar and the wire's SOH, each once more
/// with one character changed.
/// </remarks>
public sealed class PositionalSplitFixTests
{
	[Fact]
	public void Split_answers_as_the_positional_follow_does()
	{
		var texts    = Texts();
		var compared = 0;
		var wrong    = new List<string>();

		foreach (var direct in new[] { false, true })
		{
			var follow  = Variant(direct, split: false).GetType("DotGram.Finance.Fix.FixGrammar")!;
			var split   = Variant(direct, split: true).GetType("DotGram.Finance.Fix.FixGrammar")!;
			var random  = new Random(direct ? 7 : 3);
			var reading = follow.GetNestedType("FixReading")!;
			var other   = split.GetNestedType("FixReading")!;

			foreach (var name in PositionalSplitWebTests.Publications(follow, extra: 1))
				foreach (var text in texts)
				{
					var places = PositionalSplitWebTests.Places(text, random, out var windows);

					foreach (var (form, at, length) in PositionalSplitWebTests.Forms(places, windows))
					{
						compared++;

						var answer = PositionalSplitWebTests.Answer(follow, name, form, text, at, length, () => Activator.CreateInstance(reading, Fix44Context.Default)!);
						var given  = PositionalSplitWebTests.Answer(split, name, form, text, at, length, () => Activator.CreateInstance(other, Fix44Context.Default)!);

						if (!string.Equals(answer, given, StringComparison.Ordinal) && wrong.Count < 20)
							wrong.Add($"{(direct ? "reader" : "engine")} {name} {form} {at}+{length} on {PositionalSplitWebTests.Show(text)}: follow {answer}, split {given}");
					}
				}
		}

		Assert.True(compared > 1000, $"Only {compared} answers were compared.");
		Assert.True(wrong.Count == 0, $"Of {compared} answers, these differ:\n" + string.Join("\n", wrong));
	}

	/// <summary>The Finance tests' messages, as the wire and as the log writes them, and each with a character changed.</summary>
	static List<string> Texts()
	{
		var found = new HashSet<string>(StringComparer.Ordinal);

		foreach (var file in Directory.GetFiles(FinanceTests(), "*.cs").Concat(Directory.GetFiles(FinanceTests(), "*.json")).OrderBy(one => one, StringComparer.Ordinal))
		{
			var content = File.ReadAllText(file);

			foreach (Match literal in Regex.Matches(content, "\"((?:[^\"\\\\\\n]|\\\\.)*)\""))
			{
				string text;

				try
				{
					text = file.EndsWith(".json", StringComparison.Ordinal)
						? JsonSerializer.Deserialize<string>("\"" + literal.Groups[1].Value + "\"")!
						: Regex.Unescape(literal.Groups[1].Value);
				}
				catch (Exception exception) when (exception is ArgumentException or JsonException)
				{
					continue;
				}

				// A message, or a piece of one: a tag and an equals sign.
				if (text.Length is > 0 and <= 400 && Regex.IsMatch(text, "[0-9]+="))
				{
					found.Add(text);
					found.Add(text.Replace('|', '\u0001'));
					found.Add(text.Replace('\u0001', '|'));
				}
			}
		}

		var random = new Random(11);
		var texts  = found.OrderBy(one => one, StringComparer.Ordinal).OrderBy(_ => random.Next()).Take(1500).ToList();

		foreach (var text in texts.ToList())
			texts.Add(PositionalSplitWebTests.Changed(text, random));

		return texts;
	}

	static Assembly Variant(bool direct, bool split)
	{
		return _variants.GetOrAdd((direct, split), static key => Compile(key.Direct, key.Split));
	}

	static readonly System.Collections.Concurrent.ConcurrentDictionary<(bool Direct, bool Split), Assembly> _variants = new();

	static Assembly Compile(bool direct, bool split)
	{
		var parse  = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
		var file   = Path.Combine(Finance(), "Fix", "FixGrammar.cs");
		var source = File.ReadAllText(file);
		var open   = source.IndexOf("[Gram(\"\"\"", StringComparison.Ordinal) + "[Gram(\"\"\"".Length;
		var close  = source.IndexOf("\"\"\"", open, StringComparison.Ordinal);
		var lines  = source[open..close].Replace("\r\n", "\n").Split('\n');
		var indent = lines[^1].Length - lines[^1].TrimStart().Length;

		var grammar = string.Join("\n", lines.Skip(1).Take(lines.Length - 2).Select(line => line.Length >= indent ? line.Substring(indent) : line.TrimStart()));

		var trees = new List<SyntaxTree>
		{
			CSharpSyntaxTree.ParseText(GramCompiler.EmitMarkerAttributes().Text, parse),
			CSharpSyntaxTree.ParseText(SupportEmitter.EmbeddedAttribute, parse),
			CSharpSyntaxTree.ParseText("global using System;\nglobal using System.Collections.Generic;\nglobal using System.Linq;\n", parse),
			CSharpSyntaxTree.ParseText(source, parse),
		};

		// The package itself, which nothing may have loaded yet when the references were first listed.
		var package    = typeof(Fix44Context).Assembly.Location;
		var references = EmittedCode.References
			.Where(reference => !string.Equals(reference.Display, package, StringComparison.Ordinal))
			.Append(MetadataReference.CreateFromFile(package))
			.ToList();

		// The name the package lets see inside it, so that the code beside the grammar compiles here.
		const string Name = "DotGram.Finance.Tests";

		var host = CSharpCompilation.Create(Name, trees, references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		// As the attribute says, but for the carrier: pinned there, and the split is asked what it would choose.
		var generated = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName             = "FixGrammar",
			Namespace             = "DotGram.Finance.Fix",
			CSharpScanner         = RoslynCSharpScanner.Instance,
			SymbolResolver        = new RoslynSymbolResolver(host, "DotGram.Finance.Fix.FixGrammar"),
			Direct                = direct,
			PositionalFollow      = true,
			PositionalFollowSplit = split,
			LocationType          = "DotGram.Finance.Fix.IFixLocation",
			SpanCaptures          = true,
			BufferedInput         = true,
			MaxRetained           = 16 * 1024 * 1024,
			Carrier               = CarrierKind.Immediate,
		});

		Assert.DoesNotContain(generated.Diagnostics, one => one.Severity == GramSeverity.Error);

		foreach (var one in generated.Sources)
			trees.Add(CSharpSyntaxTree.ParseText(one.Text, parse));

		var compilation = CSharpCompilation.Create(Name, trees, references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		using var stream = new MemoryStream();

		var result = compilation.Emit(stream);

		Assert.True(
			result.Success,
			"FIX's grammar did not compile:\n" +
			string.Join("\n", result.Diagnostics.Where(one => one.Severity == DiagnosticSeverity.Error).Take(6)));

		return Assembly.Load(stream.ToArray());
	}

	static string Finance([CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "src", "DotGram.Finance"));
	}

	static string FinanceTests([CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "DotGram.Finance.Tests"));
	}
}
