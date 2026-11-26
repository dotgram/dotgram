using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Grammar;
using DotGram.Grammar.Model;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A rule ending in a run that gives characters back, read by a copy that gives none back where it is
/// called before something that could take none of them: every input answered as the parser without
/// the copies answers it, the copies made where they should be and nowhere else, and a long name
/// refused in as many entries as it has characters.
/// </summary>
/// <remarks>
/// <para>
/// The XML here is the example's grammar with strings for its values: what is read, where a refusal
/// stands and what it says are the example's. An attribute may follow an element's name with no space
/// between, so the name has to give characters back there; before an attribute's <c>=</c> nothing
/// can take one, and on a long name refused at its end the two together tried every way to cut the
/// name in two.
/// </para>
/// <para>
/// Each grammar is compiled with the copies and without, for the reader and for the engine, and in a
/// trace build; each input is read by all of them and every answer — accepted or not, the value, the
/// message, the position — has to be the same.
/// </para>
/// </remarks>
public sealed class SettledRunTailTests
{
	/// <summary>The example's XML, with strings for its values.</summary>
	const string Xml = """
		using Lexical;

		namespace Lexical
		{
			trivia = none

			NameText = ['a'..'z' | 'A'..'Z' | '_'] & ['a'..'z' | 'A'..'Z' | '0'..'9' | '_' | '-' | '.']*
			Space    = { [' ' | '\t' | '\r' | '\n']* }
			Quoted   = { [^ '"' | '\'']* }
			Chars    = { [^ '<']+ }
		}

		trivia = none

		Start : @string = element: Element & eof => @(element)

		Element : @string = '<' & name: Name & { attributes: Attribute* } & Lexical.Space & '>'
		                  & children: Content*
		                  & "</" & close: Name & Lexical.Space & '>' & when @(name == close) on fail "Expected the closing tag of the open element."
		                    => @("<" + name + string.Concat(attributes) + ">" + string.Concat(children) + "</" + close + ">")

		                  | '<' & name: Name & { attributes: Attribute* } & Lexical.Space & "/>"
		                    => @("<" + name + string.Concat(attributes) + "/>")

		Content : @string = element: Element => @(element)
		                  | text: Text       => @(text)

		Text : @string = chars: Lexical.Chars => @("[" + chars + "]")

		Attribute : @string = Lexical.Space & name: Name & '=' & value: Value
		                        => @(" " + name + "=(" + value + ")")

		Value : @string = '"' & text: Lexical.Quoted & '"'   => @(text)
		                | '\'' & text: Lexical.Quoted & '\'' => @(text)

		Name : @string = text: Lexical.NameText => @(text)

		parse Start
		""";

	/// <summary>
	/// A name whose rule comes back to itself past a recognizer that may not move, and ends in a run in
	/// its other alternative, called before <c>'='</c>: its entry is cut where one already stands, and
	/// a copy would be another rule entered there again, so no copy is made.
	/// </summary>
	const string Standing = """
		trivia = none

		Name  : @string = @Pfx & n: Name => @(n) | text: ['a'..'z']+ => @(text)
		Start : @string = name: Name & '=' & eof => @(name)

		parse Start
		""";

	const string StandingHost = """
		[System.ThreadStatic] static int calls;
		public static void Reset() { calls = 0; }
		static bool Pfx(System.ReadOnlySpan<char> input, ref int p) { if (++calls > 20000) throw new System.InvalidOperationException("went round"); return true; }
		""";

	/// <summary>Each name read before something that could take one of its characters: nothing is settled.</summary>
	const string Overlapping = """
		trivia = none

		Word  = ['a'..'z']+
		Start = first: Word & second: Word & 'x' & eof

		parse Start
		""";

	/// <summary>
	/// A name built from a word and a path that ends in a name or in itself, each read once before
	/// <c>'='</c> and once where a second name may take what it gives back.
	/// </summary>
	const string Chained = """
		trivia = none

		Word  = ['a'..'z'] & ['a'..'z' | '0'..'9']*
		Name  : @string = text: Word => @(text)
		Path  : @string = n: Name => @(n)
		                | '/' & p: Path => @("/" + p)
		Pair  : @string = k: Path & '=' & a: Path & b: Name? & '!' => @(k + "=" + a + "|" + b)
		Start : @string = p: Pair & (';' & Pair)* & eof => @(p)

		parse Start
		""";

	static readonly string[] XmlInputs =
	[
		"""<a x="1" y='2'><b>text</b><c/></a>""", "<a></b>", "<a><a></a></a>", """<a x="1" ></a >""", "<a>  <b/>  </a>",
		"<a> a b </a>", """<ab="1">""", """<ab="1"></a>""", """<ab="1"></ab>""", """<ab ="1">""", """<a b="1">""",
		"""<a b="1"></a>""", """<ab="1"/>""", """<a b="1"/>""", "<a/>", "<a>", "<", "", "<a></a>x", "<a.b-c_1></a.b-c_1>",
		"<a" + new string('a', 40) + "!", "<" + new string('a', 40) + " !", "<a " + new string('b', 40) + "!",
		"""<a x="1"y="2"/>""", """<a x="1" y="2" z="3"></a>""", "<_a></_a>", "<1a/>", "<a x=1/>", """<a x="1></a>""",
	];

	static readonly string[] ChainedInputs =
	[
		"a=b!", "a=bc!", "/b=//de!", "ab1=cd2!", "a=!", "=b!", "a=b!;c=d!", "//c=/f!", "abc", "a=b=c!", "/=b!", "a=b/!", "/a=/bc!",
	];

	static readonly string[] StandingInputs = ["a=", "ab=", "=", "a", "", "abc=d", "abc="];

	static readonly string[] OverlappingInputs = ["abx", "aax", "ax", "x", "", "abcx", "abc"];

	public static TheoryData<string> Grammars()
	{
		return new TheoryData<string>("Xml", "Chained", "Standing", "Overlapping", "Guarded", "Recovering", "Url", "Gram");
	}

	/// <summary>How a parser is read: by methods or by the engine, with a trace build of each, or from a reader of text.</summary>
	public enum Door
	{
		Reader,
		Engine,
		ReaderTrace,
		EngineTrace,
		Streamed,
	}

	/// <summary>A grammar under test: its text, what the host adds, its inputs, what it publishes and how it is read.</summary>
	sealed record Subject(string Grammar, string? Host, string[] Inputs, string Alphabet, int Drawn, int Longest, string Start, Door[] Doors)
	{
		public string Class { get; init; } = "Grammar";

		public string? Namespace { get; init; }

		/// <summary>Whether what the host adds is written in a C# above the floor the parsers are compiled at.</summary>
		public bool Modern { get; init; }

		public string Type => Namespace is null ? Class : Namespace + "." + Class;
	}

	static readonly Door[] Four = [Door.Reader, Door.Engine, Door.ReaderTrace, Door.EngineTrace];

	[Theory]
	[MemberData(nameof(Grammars))]
	public void Every_input_is_answered_as_without_the_copies(string name)
	{
		var subject  = Of(name);
		var random   = new Random(20261126);
		var all      = subject.Inputs
			.Concat(Enumerable.Range(0, subject.Drawn).Select(_ => Random(random, subject.Alphabet, subject.Longest)))
			.Distinct()
			.ToList();
		var compared = 0;
		var differ   = new List<string>();

		foreach (var door in subject.Doors)
		{
			var settled = Build(subject, settle: true,  door);
			var plain   = Build(subject, settle: false, door);
			var count   = 0;

			foreach (var input in all)
			{
				var expected = Answer(plain, subject, door, input);
				var got      = Answer(settled, subject, door, input);

				if (expected != got && count++ < 5)
					differ.Add($"{name} {door} {Shown(input)}: without the copies {Shown(expected)} | with them {Shown(got)}");

				compared++;
			}

			if (count > 0)
				differ.Add($"{name} {door}: {count} of {all.Count} inputs answered otherwise");
		}

		TestContext.Current.SendDiagnosticMessage($"{name}: {compared} answers compared");

		Assert.True(differ.Count == 0, string.Join("\n", differ));
	}

	[Theory]
	[InlineData("Xml",         true)]
	[InlineData("Chained",     true)]
	[InlineData("Standing",    false)]
	[InlineData("Overlapping", false)]
	[InlineData("Guarded",     true)]
	[InlineData("Recovering",  false)]
	public void A_copy_is_made_where_the_call_settles_the_run_and_nowhere_else(string name, bool copied)
	{
		var grammar = Of(name).Grammar;

		var with    = Source(grammar, settle: true,  Door.Reader);
		var without = Source(grammar, settle: false, Door.Reader);

		Assert.Equal(copied, with.Contains("_Settled(", StringComparison.Ordinal));
		Assert.Equal(copied, with != without);
	}

	/// <summary>
	/// A guard between the name and the <c>'='</c> reads the name a give-back shortens, and still
	/// takes the copy: whatever the guard says of a shorter name, the <c>'='</c> after it stands on a
	/// character of the name. Before a second name, which may take what the first gives back, the
	/// call keeps the rule.
	/// </summary>
	[Fact]
	public void A_call_before_a_guard_takes_the_copy_and_one_before_a_second_name_keeps_the_rule()
	{
		var graph = Graph(LexemeSplitShapeTests.Guarded);
		var calls = new Dictionary<string, List<string>>(StringComparer.Ordinal);

		foreach (var rule in graph.Rules.Where(static one => one.Declaration?.Name is "Pair" or "Spill"))
			calls[rule.Declaration!.Name] = [.. NodeWalk.Descendants(graph.Bodies[rule]).OfType<Node.Call>()
				.Where(static call => call.Rule.Declaration?.Name == "Name").Select(static call => call.Rule.Name)];

		Assert.Equal(["Name_Settled", "Name_Settled"], calls["Pair"]);
		Assert.Equal(["Name", "Name_Settled"], calls["Spill"]);
	}

	[Fact]
	public void A_rule_cut_where_it_stands_is_not_copied()
	{
		var graph = Graph(Standing);

		Assert.Contains(graph.Rules, static rule => rule.Name == "Name" && rule.Declaration is not null);
		Assert.Empty(graph.Settled);
	}

	[Fact]
	public void The_copies_are_named_and_counted_as_the_rules_they_were_made_from()
	{
		var graph = Graph(Xml);

		Assert.Equal(
			["NameText_Settled <- NameText", "Name_Settled <- Name"],
			graph.Settled.Select(static pair => $"{pair.Key.Name} <- {pair.Value.Name}").OrderBy(static one => one, StringComparer.Ordinal));

		foreach (var pair in graph.Settled)
		{
			Assert.Same(pair.Value.Declaration, pair.Key.Declaration);
			Assert.Same(pair.Value, graph.Counted(pair.Key));
		}

		var source = Source(Xml, settle: true, Door.Reader, counts: true);

		Assert.DoesNotContain("CountEntered_Name_Settled", source, StringComparison.Ordinal);
		Assert.Contains("CountEntered_Name;", source, StringComparison.Ordinal);
	}

	/// <summary>
	/// A long name refused at its end, read in as many entries of rules as it has characters: the
	/// attribute's name is read once at each place the element's name gives back to, and gives none
	/// back itself.
	/// </summary>
	[Theory]
	[InlineData("<a", "!")]
	[InlineData("<a", " !")]
	public void A_long_name_refused_at_its_end_is_entered_a_number_of_times_linear_in_its_length(string before, string after)
	{
		var settled = Counted(Xml, settle: true);
		var plain   = Counted(Xml, settle: false);
		var rows    = new List<string>();

		long? last = null;

		foreach (var n in new[] { 64, 128, 256, 512 })
		{
			var input = before + new string('a', n) + after;

			var (entries, scanned) = Counts(settled, input);
			var (was, wasScanned)  = Counts(plain, input);

			rows.Add($"n={n}: entries {was} -> {entries}, characters {wasScanned} -> {scanned}");

			if (last is { } previous)
				Assert.True(entries <= previous * 2.3, $"entries grew from {previous} to {entries} on doubling: " + string.Join("; ", rows));

			last = entries;
		}

		TestContext.Current.SendDiagnosticMessage(string.Join("\n", rows));
	}

	/// <summary>
	/// Every grammar of the repository read over characters, normalized with the copies: each copy it
	/// makes settles its run when the follow sets are worked out again over the calls as they stand.
	/// </summary>
	[Fact]
	public void Every_copy_the_repository_makes_settles_its_run()
	{
		var before = GrammarNormalizer.Unsettles;
		var made   = new System.Collections.Concurrent.ConcurrentBag<string>();

		System.Threading.Tasks.Parallel.ForEach(
			ExponentialRepetitionTests.Grammars(SourceRoot.Directory).Where(static grammar => grammar.Modes.Contains(false)),
			grammar =>
			{
				RecognitionGraph graph;

				try
				{
					graph = ExponentialRepetitionTests.Built(grammar, lexical: false, out _, settle: true);
				}
				catch (Exception)
				{
					return;
				}

				if (graph.Settled.Count > 0)
					made.Add($"{grammar.Name}: {string.Join(", ", graph.Settled.Values.Select(static rule => rule.Name).Distinct().Order(StringComparer.Ordinal))}");
			});

		TestContext.Current.SendDiagnosticMessage(string.Join("\n", made.Order(StringComparer.Ordinal)));

		Assert.NotEmpty(made);
		Assert.Equal(before, GrammarNormalizer.Unsettles);
	}

	/// <summary>
	/// What a parser says of itself in text — a refusal, a step, a trace, a check that fails — names
	/// the rules the author wrote: no string the generated code holds names a settled copy, in a
	/// plain build, a trace build, a build that steps or one that counts, read by methods or the engine.
	/// </summary>
	[Theory]
	[InlineData("Xml")]
	[InlineData("Chained")]
	[InlineData("Url")]
	[InlineData("Gram")]
	[InlineData("Notation")]
	public void No_text_the_parser_holds_names_a_copy(string name)
	{
		var grammar = name == "Notation"
			? File.ReadAllText(Path.Combine(SourceRoot.Directory, "tests", "Snapshots", "Notation.gram"))
			: Of(name).Grammar;
		var copies  = Graph(grammar).Settled.Keys.Select(static copy => copy.Name).ToList();
		var found   = new List<string>();

		Assert.NotEmpty(copies);

		foreach (var direct in new[] { true, false })
			foreach (var (trace, steps, counts) in new[] { (false, false, false), (true, false, false), (false, true, false), (false, false, true) })
			{
				var result = GramCompiler.Compile(grammar, new GramCompilerOptions
				{
					ClassName      = "Grammar",
					CSharpScanner  = RoslynCSharpScanner.Instance,
					SymbolResolver = PermissiveSymbolResolver.Instance,
					Direct         = direct,
					Trace          = trace,
					Steps          = steps,
					CountRules     = counts,
				});

				foreach (var source in result.Sources)
					foreach (var token in Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(source.Text, cancellationToken: TestContext.Current.CancellationToken).GetRoot(TestContext.Current.CancellationToken).DescendantTokens())
						if (token.RawKind == (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralToken ||
							token.RawKind == (int)Microsoft.CodeAnalysis.CSharp.SyntaxKind.InterpolatedStringTextToken)
							foreach (var copy in copies)
								if (System.Text.RegularExpressions.Regex.IsMatch(token.ValueText, $@"\b{copy}\b"))
									found.Add($"{(direct ? "reader" : "engine")}{(trace ? " trace" : "")}{(steps ? " steps" : "")}{(counts ? " counts" : "")}: {token.Text}");
			}

		Assert.True(found.Count == 0, string.Join("\n", found.Distinct().Take(20)));
	}

	/// <summary>
	/// Every grammar of the repository read over characters, compiled for the engine with the copies
	/// and without: the rules it reads by a scanner are the same rules. A copy is never one, and an
	/// original whose follow narrowed when some of its calls moved to a copy does not become one —
	/// a scanner names its rule in a refusal where the states it replaces named what failed.
	/// </summary>
	[Fact]
	public void The_engine_scans_the_same_rules_with_the_copies_and_without()
	{
		var differ   = new System.Collections.Concurrent.ConcurrentBag<string>();
		var compared = 0;

		System.Threading.Tasks.Parallel.ForEach(
			ExponentialRepetitionTests.Grammars(SourceRoot.Directory)
				.Where(static grammar => grammar.Modes.Contains(false) && grammar.Included.Count == 0),
			grammar =>
			{
				string[] with, without;

				try
				{
					with    = Scanners(grammar.Text, settle: true);
					without = Scanners(grammar.Text, settle: false);
				}
				catch (Exception)
				{
					return;
				}

				System.Threading.Interlocked.Increment(ref compared);

				if (!with.SequenceEqual(without))
					differ.Add($"{grammar.Name}: with the copies [{string.Join(", ", with.Except(without))}], without them [{string.Join(", ", without.Except(with))}]");
			});

		TestContext.Current.SendDiagnosticMessage($"{compared} grammars compared");

		Assert.True(compared > 50, $"only {compared} grammars compiled");
		Assert.True(differ.IsEmpty, string.Join("\n", differ.Order(StringComparer.Ordinal)));

		static string[] Scanners(string text, bool settle)
		{
			var result = GramCompiler.Compile(text, new GramCompilerOptions
			{
				ClassName      = "Grammar",
				CSharpScanner  = RoslynCSharpScanner.Instance,
				SymbolResolver = PermissiveSymbolResolver.Instance,
				Direct         = false,
				SettleRunTails = settle,
			});

			if (result.Diagnostics.Any(static one => one.Severity == GramSeverity.Error))
				throw new InvalidOperationException("not compiled");

			return
			[
				.. result.Sources
					.SelectMany(static source => System.Text.RegularExpressions.Regex.Matches(source.Text, @"\bScan(?:\w*?)_DotGram_\w+").Select(static match => match.Value))
					.Distinct()
					.Order(StringComparer.Ordinal),
			];
		}
	}

	static Subject Of(string name)
	{
		return name switch
		{
			"Xml"         => new(Xml, null, XmlInputs, "<>=/\"'ab !", 10000, 12, "TryParseStart", [.. Four, Door.Streamed]),
			"Chained"     => new(Chained, null, ChainedInputs, "ab1=/;!", 2000, 12, "TryParseStart", Four),
			"Standing"    => new(Standing, StandingHost, StandingInputs, "abx=", 2000, 12, "TryParseStart", Four),
			"Overlapping" => new(Overlapping, null, OverlappingInputs, "abx=", 2000, 12, "TryParseStart", Four),
			"Guarded"     => new(LexemeSplitShapeTests.Guarded, null, ["ab=c", "abc=d", "abc=", "abcd=", "a=", "ab", "abc", "ab=", "=", ""], "abx=", 2000, 12, "TryParseStart", Four),
			"Recovering"  => new(LexemeSplitShapeTests.Recovering, null, ["[a=b;]", "[ab=;c=d;]", "[abc;d=e;]", "[a=bc]", "a=,=b.", "ab=cd!", "abc"], "abx=", 2000, 12, "TryParseStart", Four),
			"Url"         => new(UrlGrammar, null, UrlInputs, "htp:/1.2[]:a@?#%f", 3000, 20, "TryParseUrl", Four),
			"Gram"        => GramSubject(),
			_             => throw new ArgumentOutOfRangeException(nameof(name)),
		};
	}

	static readonly string[] UrlInputs =
	[
		"http://a.b/c", "http://1.2.3.4/x", "http://1.2.3.400/", "http://1.2.3/", "http://1234.1.1.1/", "ftp://u:p@h:21/p?q#f",
		"http://[::1]/", "http://[1:2:3:4:5:6:7:8]/", "http://[1:2:3:4:5:6:1.2.3.4]/", "http://[::ffff:1.2.3.4]", "http://[1::]/",
		"http://[12345::]/", "http://123", "https://a%2", "http://a@b@c/", "HTTP://A.B:8080",
	];

	static string UrlGrammar => File.ReadAllText(Path.Combine(SourceRoot.Directory, "tests", "Snapshots", "Url.gram"));

	/// <summary>
	/// The example's grammar of the notation itself, with the records it builds: its host class beside
	/// it under a namespace of the test's own, compiled at the C# the records are written in.
	/// </summary>
	static Subject GramSubject()
	{
		var file    = File.ReadAllText(Path.Combine(SourceRoot.Directory, "examples", "DotGram.Examples", "Languages", "GramExample.cs")).Replace("\r\n", "\n");
		var begin   = file.IndexOf("[Gram(\"\"\"", StringComparison.Ordinal) + "[Gram(\"\"\"".Length;
		var end     = file.IndexOf("\"\"\")]", begin, StringComparison.Ordinal);
		var grammar = string.Join("\n", file[begin..end].Split('\n').Select(static line => line.StartsWith('\t') ? line[1..] : line))
			.Replace("@using DotGram.Examples.Languages;", "", StringComparison.Ordinal);
		var body    = file.IndexOf('{', file.IndexOf("public partial class GramGrammar", end, StringComparison.Ordinal)) + 1;

		// On one line, as the harness declares the class: a line's comment would end the class.
		var host    = string.Join("\n", file[body..file.LastIndexOf('}')].Split('\n')
				.Where(static line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)))
			.Replace("first is null ? [] :", "first is null ? System.Array.Empty<GramExpr>() :", StringComparison.Ordinal)
			.Replace("(ReadOnlySpan<char>", "(System.ReadOnlySpan<char>", StringComparison.Ordinal);
		var texts   = new[] { grammar, Xml, Chained, UrlGrammar, LexemeSplitShapeTests.Guarded, LexemeSplitShapeTests.Recovering };
		var inputs  = texts.Concat(texts.SelectMany(static text => Enumerable.Range(1, text.Length / 23).Select(cut => text[..(cut * 23)]))).ToArray();

		return new(grammar, "\n#nullable enable\n" + host, inputs, "Ab=|&'\"/ *;{}@:()+?.\n", 1000, 16, "TryParseFile", Four)
		{
			Class     = "GramGrammar",
			Namespace = "Probe",
			Modern    = true,
		};
	}

	/// <summary>Text on one line, its middle left out where it is long.</summary>
	static string Shown(string text)
	{
		var line = text.Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);

		return line.Length <= 160 ? $"\"{line}\"" : $"\"{line[..60]}…{line[^90..]}\" ({text.Length} characters)";
	}

	static string Random(Random random, string alphabet, int most)
	{
		var length = random.Next(most + 1);
		var chars  = new char[length];

		for (var i = 0; i < length; i++)
			chars[i] = alphabet[random.Next(alphabet.Length)];

		return new string(chars);
	}

	/// <summary>What a parser answers for an input, in one string: the value, or the message and where it stands.</summary>
	static string Answer(Assembly parser, Subject subject, Door door, string input)
	{
		var type = parser.GetType(subject.Type)!;

		type.GetMethod("Reset")?.Invoke(null, null);

		try
		{
			if (door != Door.Streamed)
			{
				var (accepted, value, error, position) = EmittedCode.Match(parser, subject.Type, subject.Start, input);

				return accepted ? $"ok {value}" : $"no {error} @{position}";
			}

			var method = type.GetMethods().Single(one => one.Name == subject.Start &&
				one.GetParameters() is [{ ParameterType: var first }, ..] && first == typeof(TextReader));
			var given  = method.GetParameters().Select(static one => one.HasDefaultValue ? one.DefaultValue : null).ToArray();

			using var reader = new StringReader(input);

			given[0] = reader;

			var match = method.Invoke(null, given)!;

			object? Read(string property)
			{
				return match.GetType().GetProperty(property)!.GetValue(match);
			}

			return (bool)Read("IsSuccess")! ? $"ok {Read("Value")}" : $"no {Read("Error")} @{Read("Position")}";
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException?.Message == "went round")
		{
			return "LOOP";
		}
	}

	static readonly Dictionary<(string, bool, Door), Assembly> _built = [];

	static Assembly Build(Subject subject, bool settle, Door door)
	{
		lock (_built)
		{
			if (!_built.TryGetValue((subject.Grammar, settle, door), out var assembly))
			{
				var result = Compiled(subject.Grammar, settle, door, counts: false, subject.Class, subject.Namespace);

				_built[(subject.Grammar, settle, door)] = assembly = EmittedCode.Compile(
					result.Sources[0].Text, subject.Class, subject.Namespace, subject.Host, result.Sources.Skip(1).Select(static one => one.Text),
					utf8Literals: subject.Modern);
			}

			return assembly;
		}
	}

	static Assembly Counted(string grammar, bool settle)
	{
		return EmittedCode.Compile(Source(grammar, settle, Door.Reader, counts: true), "Grammar", symbols: ["DOTGRAM_COUNTS"]);
	}

	/// <summary>The entries to every rule and the characters every run stepped over, reading one input.</summary>
	static (long Entries, long Scanned) Counts(Assembly assembly, string input)
	{
		var type     = assembly.GetType("Grammar")!;
		var counters = new[] { type }
			.Concat(type.GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Public))
			.SelectMany(static one => one.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
			.Where(static field => field.FieldType == typeof(long) &&
				(field.Name.StartsWith("CountEntered_", StringComparison.Ordinal) || field.Name.StartsWith("CountScanned_", StringComparison.Ordinal)))
			.ToList();

		foreach (var counter in counters)
			counter.SetValue(null, 0L);

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseStart", input).IsSuccess);

		return (
			counters.Where(static one => one.Name.StartsWith("CountEntered_", StringComparison.Ordinal)).Sum(static one => (long)one.GetValue(null)!),
			counters.Where(static one => one.Name.StartsWith("CountScanned_", StringComparison.Ordinal)).Sum(static one => (long)one.GetValue(null)!));
	}

	static string Source(string grammar, bool settle, Door door, bool counts = false)
	{
		return Assert.Single(Compiled(grammar, settle, door, counts).Sources).Text;
	}

	static GramCompilation Compiled(string grammar, bool settle, Door door, bool counts, string className = "Grammar", string? @namespace = null)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = className,
			Namespace      = @namespace,
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			Direct         = door is not (Door.Engine or Door.EngineTrace),
			Trace          = door is Door.ReaderTrace or Door.EngineTrace,
			BufferedInput  = door == Door.Streamed,
			CountRules     = counts,
			SettleRunTails = settle,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return result;
	}

	static RecognitionGraph Graph(string grammar)
	{
		var parsed = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance);
		var model  = DotGram.Grammar.Binding.GrammarBinder.Bind(parsed.File, PermissiveSymbolResolver.Instance, grammar.Length);

		return GrammarNormalizer.Normalize(model, PermissiveSymbolResolver.Instance, RoslynCSharpScanner.Instance, settle: true);
	}
}
