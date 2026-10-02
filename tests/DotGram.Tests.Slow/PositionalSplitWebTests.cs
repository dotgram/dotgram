using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The positional follow told apart (<see cref="GramCompilerOptions.PositionalFollowSplit"/>)
/// answers every shipped web grammar's readings as the positional follow alone does: from
/// positions, in windows, by the form that moves the position and by the whole form, in the
/// engine and in the reader.
/// </summary>
/// <remarks>
/// <para>
/// <b>What it holds.</b> The split changes what the generator proves about a grammar, never what
/// a reading is; so it is held to the option without it, which tells a positional stop from
/// nothing and is right by construction (PositionalFollowTests holds that one to the reference
/// interpreter). Every web grammar is read from its file and compiled here four ways — engine or
/// reader, split or not — together with the hand-written code beside it, and every publication
/// is asked the same questions of all four.
/// </para>
/// <para>
/// <b>What it asks.</b> The texts the web tests are written with, the corpora beside them, each
/// once more with one character put in, taken out or changed, and every grammar's texts offered
/// to the others as well; from the start, from the end, from a handful of places between, and
/// in windows that end at random. An answer is whether it read, where and how much, and the
/// value it built, written out member by member; a refusal's message is not an answer and is
/// not compared.
/// </para>
/// </remarks>
public sealed class PositionalSplitWebTests
{
	/// <summary>The web grammar files, by the class each hosts.</summary>
	public static IEnumerable<object[]> Grammars()
	{
		foreach (var file in Directory.GetFiles(Web(), "Rfc*.cs").OrderBy(one => one, StringComparer.Ordinal))
			if (File.ReadAllText(file).Contains("[Gram(\"\"\"", StringComparison.Ordinal))
				yield return [Path.GetFileNameWithoutExtension(file)];
	}

	[Theory]
	[MemberData(nameof(Grammars))]
	public void Split_answers_as_the_positional_follow_does(string grammar)
	{
		var texts    = Texts(grammar);
		var compared = 0;
		var wrong    = new List<string>();

		foreach (var direct in new[] { false, true })
		{
			var follow = Variant(direct, split: false).GetType("DotGram.Web." + grammar)!;
			var split  = Variant(direct, split: true).GetType("DotGram.Web." + grammar)!;
			// A seed that is the same in every process, so that a failure can be run again.
			var random = new Random(grammar.Sum(static letter => (int)letter) * 2 + (direct ? 1 : 0));

			foreach (var name in Publications(follow))
			{
				foreach (var text in texts)
				{
					var places = Places(text, random, out var windows);

					foreach (var (form, at, length) in Forms(places, windows))
					{
						compared++;

						var answer = Answer(follow, name, form, text, at, length);
						var other  = Answer(split, name, form, text, at, length);

						if (!string.Equals(answer, other, StringComparison.Ordinal) && wrong.Count < 20)
							wrong.Add($"{(direct ? "reader" : "engine")} {name} {form} {at}+{length} on {Show(text)}: follow {answer}, split {other}");
					}
				}
			}
		}

		Assert.True(compared > 1000, $"{grammar}: only {compared} answers were compared.");
		Assert.True(wrong.Count == 0, $"{grammar}: of {compared} answers, these differ:\n" + string.Join("\n", wrong));
	}

	/// <summary>Every name a publication answers to: the <c>TryParse</c> methods that read from a position.</summary>
	internal static IEnumerable<string> Publications(Type type, int extra = 0)
	{
		return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
			.Where(one => one.Name.StartsWith("TryParse", StringComparison.Ordinal) &&
				one.GetParameters() is var taken && taken.Length == 2 + extra &&
				taken[0].ParameterType == typeof(string) && taken[1].ParameterType == typeof(int))
			.Select(one => one.Name)
			.Distinct()
			.OrderBy(one => one, StringComparer.Ordinal);
	}

	/// <summary>Where a text is read from: the start, the end, and up to six places between.</summary>
	internal static List<int> Places(string text, Random random, out List<int> windows)
	{
		var places = new SortedSet<int> { 0, text.Length };

		if (text.Length <= 8)
			for (var at = 1; at < text.Length; at++)
				places.Add(at);
		else
			for (var i = 0; i < 6; i++)
				places.Add(random.Next(text.Length + 1));

		windows = [];

		foreach (var at in places)
			windows.Add(random.Next(text.Length - at + 1));

		return [.. places];
	}

	/// <summary>Every form of a publication, at each place and in each window: the whole form once.</summary>
	internal static IEnumerable<(string Form, int At, int Length)> Forms(List<int> places, List<int> windows)
	{
		yield return ("whole", 0, -1);

		for (var i = 0; i < places.Count; i++)
		{
			yield return ("at", places[i], -1);
			yield return ("ref", places[i], -1);
			yield return ("window", places[i], windows[i]);
			yield return ("window ref", places[i], windows[i]);
		}
	}

	/// <summary>One form's answer, written out: whether it read, where and how much, and what it built.</summary>
	/// <param name="context">What a grammar with a <c>context</c> is handed last, made afresh for each call; null where it has none.</param>
	internal static string Answer(Type type, string name, string form, string text, int at, int length, Func<object>? context = null)
	{
		const BindingFlags Static = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

		var extra = context is null ? 0 : 1;

		object?[] With(params object?[] arguments)
		{
			return context is null ? arguments : [.. arguments, context()];
		}

		MethodInfo Taking(int count, Func<ParameterInfo[], bool> shape)
		{
			return type.GetMethods(Static).Single(one => one.Name == name && one.GetParameters() is var taken && taken.Length == count + extra && shape(taken));
		}

		try
		{
			if (form is "ref" or "window ref")
			{
				var method = type.GetMethods(Static).Single(one =>
					one.Name == name && one.GetParameters() is var taken &&
					taken.Length == (length < 0 ? 3 : 4) + extra && taken[1].ParameterType.IsByRef && taken[^1].IsOut);
				var given     = length < 0 ? new object?[] { text, at } : [text, at, length];
				var arguments = context is null ? [.. given, null] : given.Append(context()).Append(null).ToArray();
				var read      = (bool)method.Invoke(null, arguments)!;

				return read ? $"read to {arguments[1]}: {Dump(arguments[^1])}" : "refused";
			}

			var match = form switch
			{
				"whole" => Taking(1, static taken => taken[0].ParameterType == typeof(string)).Invoke(null, With(text))!,
				"at"    => Taking(2, static taken => taken[0].ParameterType == typeof(string) && taken[1].ParameterType == typeof(int)).Invoke(null, With(text, at))!,
				_       => Taking(3, static taken => taken[0].ParameterType == typeof(string) && taken[1].ParameterType == typeof(int) && taken[2].ParameterType == typeof(int)).Invoke(null, With(text, at, length))!,
			};

			var kind = match.GetType();

			if (!(bool)kind.GetProperty("IsSuccess")!.GetValue(match)!)
				return "refused";

			return $"read {kind.GetProperty("Position")!.GetValue(match)}+{kind.GetProperty("Length")!.GetValue(match)}: " +
				Dump(kind.GetProperty("Value")!.GetValue(match));
		}
		catch (TargetInvocationException thrown)
		{
			return "threw " + thrown.InnerException?.GetType().Name;
		}
	}

	/// <summary>A value written out member by member, so that two assemblies' values can be compared.</summary>
	internal static string Dump(object? value, int depth = 0)
	{
		if (value is null)
			return "null";

		if (depth > 6)
			return "…";

		var type = value.GetType();

		if (value is string text)
			return Show(text);

		if (type.IsPrimitive || type.IsEnum || value is decimal || value is DateTime || value is DateTimeOffset || value is TimeSpan || value is Guid)
			return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "";

		if (value is IEnumerable items)
		{
			var listed = new StringBuilder("[");

			foreach (var item in items)
				listed.Append(Dump(item, depth + 1)).Append(',');

			return listed.Append(']').ToString();
		}

		var members = new StringBuilder(type.Name).Append('{');

		foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(one => one.Name, StringComparer.Ordinal))
			if (property.GetIndexParameters().Length == 0 && property.Name != "EqualityContract" && !property.PropertyType.IsByRefLike)
			{
				object? held;

				try
				{
					held = property.GetValue(value);
				}
				catch (TargetInvocationException)
				{
					held = "threw";
				}

				members.Append(property.Name).Append('=').Append(Dump(held, depth + 1)).Append(';');
			}

		foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance).OrderBy(one => one.Name, StringComparer.Ordinal))
			members.Append(field.Name).Append('=').Append(Dump(field.GetValue(value), depth + 1)).Append(';');

		return members.Append('}').ToString();
	}

	internal static string Show(string text)
	{
		return JsonSerializer.Serialize(text.Length > 80 ? text.Substring(0, 80) + "…" : text);
	}

	/// <summary>
	/// What a grammar is asked to read: its tests' texts, its corpus, every other grammar's texts,
	/// and each once more with one character changed.
	/// </summary>
	static List<string> Texts(string grammar)
	{
		var own    = Harvested(grammar);
		var others = Grammars().Select(one => (string)one[0]).Where(one => one != grammar).SelectMany(Harvested).Distinct().ToList();
		var random = new Random(17);
		var texts  = new HashSet<string>(own, StringComparer.Ordinal);

		// A sample of the other grammars' texts: a fixed one, so that a failure can be run again.
		foreach (var other in others.OrderBy(_ => random.Next()).Take(300))
			texts.Add(other);

		foreach (var text in texts.ToList())
			texts.Add(Changed(text, random));

		return [.. texts.OrderBy(one => one, StringComparer.Ordinal)];
	}

	/// <summary>A text with one character put in, taken out or replaced.</summary>
	internal static string Changed(string text, Random random)
	{
		const string Letters = ";,=/#?%\"' -:@[]{}()<>.0aZ\t~*!$&+\\";

		var at     = random.Next(text.Length + 1);
		var letter = Letters[random.Next(Letters.Length)];

		return random.Next(3) switch
		{
			0                         => text.Insert(at, letter.ToString()),
			1 when at < text.Length   => text.Remove(at, 1),
			_ when at < text.Length   => text.Remove(at, 1).Insert(at, letter.ToString()),
			_                         => text + letter,
		};
	}

	/// <summary>The texts of one grammar's tests and corpus.</summary>
	static IEnumerable<string> Harvested(string grammar)
	{
		return _harvested.GetOrAdd(grammar, static name =>
		{
			var found = new HashSet<string>(StringComparer.Ordinal);
			var tests = Path.Combine(WebTests(), name + "Tests.cs");

			if (File.Exists(tests))
				foreach (Match literal in Regex.Matches(File.ReadAllText(tests), "@?\"((?:[^\"\\\\\\n]|\\\\.)*)\""))
				{
					try
					{
						found.Add(Regex.Unescape(literal.Groups[1].Value));
					}
					catch (ArgumentException)
					{
						found.Add(literal.Groups[1].Value);
					}
				}

			var corpus = name switch
			{
				"Rfc3339" => "Timestamps",
				"Rfc5322" => "IsEmail",
				"Rfc5646" => "LanguageTags",
				"Rfc6265" => "HttpState",
				"Rfc6570" => "UriTemplates",
				"Rfc6901" => "JsonPatch",
				"Rfc8259" => "JsonTestSuite",
				"Rfc9110" => "MediaTypes",
				"Rfc9651" => "StructuredFields",
				_         => null,
			};

			if (corpus is not null)
				foreach (var file in Directory.GetFiles(Path.Combine(WebTests(), corpus), "*", SearchOption.AllDirectories)
					.OrderBy(one => one, StringComparer.Ordinal))
					foreach (var text in Corpus(file, name))
						found.Add(text);

			// Bounded: a corpus of thousands of lines is sampled, at random but always the same.
			var random = new Random(name.Length);

			return [.. found.Where(one => one.Length <= 400).OrderBy(one => one, StringComparer.Ordinal).OrderBy(_ => random.Next()).Take(1200)];
		});
	}

	static readonly System.Collections.Concurrent.ConcurrentDictionary<string, List<string>> _harvested = new();

	/// <summary>The texts in one corpus file: a JSON document's strings and itself, or a file's lines.</summary>
	static IEnumerable<string> Corpus(string file, string grammar)
	{
		var content = File.ReadAllText(file);

		if (file.EndsWith(".json", StringComparison.Ordinal))
		{
			if (grammar == "Rfc8259")
			{
				yield return content;

				yield break;
			}

			JsonDocument? document = null;

			try
			{
				document = JsonDocument.Parse(content);
			}
			catch (JsonException)
			{
			}

			if (document is not null)
			{
				foreach (var text in Strings(document.RootElement))
					yield return text;

				document.Dispose();

				yield break;
			}
		}

		foreach (var line in content.Replace("\r\n", "\n").Split('\n'))
		{
			yield return line;

			// A registry's `Tag: x-y` and a CSV's cells: the value after a field name, and each cell.
			if (line.IndexOf(": ", StringComparison.Ordinal) is var colon and > 0)
				yield return line.Substring(colon + 2);

			foreach (var cell in line.Split(','))
				yield return cell.Trim();
		}
	}

	static IEnumerable<string> Strings(JsonElement element)
	{
		switch (element.ValueKind)
		{
			case JsonValueKind.String:
				yield return element.GetString()!;
				break;

			case JsonValueKind.Array:
				foreach (var item in element.EnumerateArray())
					foreach (var text in Strings(item))
						yield return text;
				break;

			case JsonValueKind.Object:
				foreach (var property in element.EnumerateObject())
				{
					yield return property.Name;

					foreach (var text in Strings(property.Value))
						yield return text;
				}
				break;
		}
	}

	/// <summary>Every web grammar and the code beside it, compiled one of four ways.</summary>
	static Assembly Variant(bool direct, bool split)
	{
		return _variants.GetOrAdd((direct, split), static key => Compile(key.Direct, key.Split));
	}

	static readonly System.Collections.Concurrent.ConcurrentDictionary<(bool Direct, bool Split), Assembly> _variants = new();

	static Assembly Compile(bool direct, bool split)
	{
		var parse = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
		// The package builds with implicit usings.
		const string Usings =
			"global using System;\nglobal using System.Collections.Generic;\nglobal using System.IO;\nglobal using System.Linq;\n" +
			"global using System.Net.Http;\nglobal using System.Threading;\nglobal using System.Threading.Tasks;\n";

		var trees = new List<SyntaxTree>
		{
			CSharpSyntaxTree.ParseText(GramCompiler.EmitMarkerAttributes().Text, parse),
			CSharpSyntaxTree.ParseText(SupportEmitter.EmbeddedAttribute, parse),
			CSharpSyntaxTree.ParseText(Usings, parse),
		};

		var files = Directory.GetFiles(Web(), "*.cs").OrderBy(one => one, StringComparer.Ordinal).ToList();

		foreach (var file in files)
			trees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(file), parse));

		// The package's own assembly is left out: each variant declares every type it does.
		var references = EmittedCode.References.Where(
			reference => !string.Equals(Path.GetFileName(reference.Display), "DotGram.Web.dll", StringComparison.OrdinalIgnoreCase)).ToList();

		// What the generator answers C# questions from: the hand-written code, before anything is generated.
		var host = CSharpCompilation.Create(
			"DotGram.Tests.WebHost", trees, references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		foreach (var file in files)
		{
			var source = File.ReadAllText(file);
			var open   = source.IndexOf("[Gram(\"\"\"", StringComparison.Ordinal);

			if (open < 0)
				continue;

			open += "[Gram(\"\"\"".Length;

			var close     = source.IndexOf("\"\"\"", open, StringComparison.Ordinal);
			var attribute = source.Substring(close, source.IndexOf(")]", close, StringComparison.Ordinal) - close);
			var grammar   = Dedented(source[open..close]);

			var generated = GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				ClassName             = Path.GetFileNameWithoutExtension(file),
				Namespace             = "DotGram.Web",
				CSharpScanner         = RoslynCSharpScanner.Instance,
				SymbolResolver        = new RoslynSymbolResolver(host, "DotGram.Web." + Path.GetFileNameWithoutExtension(file)),
				Direct                = direct,
				PositionalFollow      = true,
				PositionalFollowSplit = split,
				SpanCaptures          = attribute.Contains("SpanCaptures = true", StringComparison.Ordinal),
			});

			Assert.DoesNotContain(generated.Diagnostics, one => one.Severity == GramSeverity.Error);

			foreach (var one in generated.Sources)
				trees.Add(CSharpSyntaxTree.ParseText(one.Text, parse));
		}

		var compilation = CSharpCompilation.Create(
			$"DotGram.Tests.Web{(direct ? "Reader" : "Engine")}{(split ? "Split" : "Follow")}",
			trees,
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable, allowUnsafe: true));

		using var stream = new MemoryStream();

		var result = compilation.Emit(stream);

		Assert.True(
			result.Success,
			"The web grammars did not compile:\n" +
			string.Join("\n", result.Diagnostics.Where(one => one.Severity == DiagnosticSeverity.Error).Take(6).Select(one =>
				one + " in: " + one.Location.SourceTree?.GetText().Lines[one.Location.GetLineSpan().StartLinePosition.Line].ToString().Trim())));

		return Assembly.Load(stream.ToArray());
	}

	/// <summary>A raw string literal's text, its closing quotes' indentation taken off every line.</summary>
	static string Dedented(string raw)
	{
		var lines   = raw.Replace("\r\n", "\n").Split('\n');
		var closing = lines[^1];
		var indent  = closing.Length - closing.TrimStart().Length;

		return string.Join("\n", lines.Skip(1).Take(lines.Length - 2).Select(line => line.Length >= indent ? line.Substring(indent) : line.TrimStart()));
	}

	static string Web([CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "src", "DotGram.Web"));
	}

	static string WebTests([CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "DotGram.Tests", "Web"));
	}
}
