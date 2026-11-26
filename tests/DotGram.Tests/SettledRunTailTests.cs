using System;
using System.Collections.Generic;
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
		return new TheoryData<string>("Xml", "Chained", "Standing", "Overlapping", "Guarded", "Recovering");
	}

	[Theory]
	[MemberData(nameof(Grammars))]
	public void Every_input_is_answered_as_without_the_copies(string name)
	{
		var (grammar, host, inputs) = Of(name);
		var alphabet = name == "Xml" ? "<>=/\"'ab !" : name == "Chained" ? "ab1=/;!" : "abx=";
		var random   = new Random(20261126);
		var all      = inputs.Concat(Enumerable.Range(0, name == "Xml" ? 10000 : 2000).Select(_ => Random(random, alphabet, 12))).ToList();
		var compared = 0;
		var differ   = new List<string>();

		foreach (var (direct, trace) in new[] { (true, false), (false, false), (true, true), (false, true) })
		{
			var settled = Build(grammar, host, settle: true,  direct, trace);
			var plain   = Build(grammar, host, settle: false, direct, trace);
			var where   = $"{(direct ? "reader" : "engine")}{(trace ? " trace" : "")}";
			var count   = 0;

			foreach (var input in all.Distinct())
			{
				var expected = Answer(plain, input);
				var got      = Answer(settled, input);

				if (expected != got && count++ < 5)
					differ.Add($"{name} {where} \"{input}\": without the copies {expected} | with them {got}");

				compared++;
			}

			if (count > 0)
				differ.Add($"{name} {where}: {count} of {all.Distinct().Count()} inputs answered otherwise");
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
		var (grammar, _, _) = Of(name);

		var with    = Source(grammar, settle: true,  direct: true, trace: false, counts: false);
		var without = Source(grammar, settle: false, direct: true, trace: false, counts: false);

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

		var source = Source(Xml, settle: true, direct: true, trace: false, counts: true);

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

	static (string Grammar, string? Host, string[] Inputs) Of(string name)
	{
		return name switch
		{
			"Xml"         => (Xml, null, XmlInputs),
			"Chained"     => (Chained, null, ChainedInputs),
			"Standing"    => (Standing, StandingHost, StandingInputs),
			"Overlapping" => (Overlapping, null, OverlappingInputs),
			"Guarded"     => (LexemeSplitShapeTests.Guarded, null, ["ab=c", "abc=d", "abc=", "abcd=", "a=", "ab", "abc", "ab=", "=", ""]),
			"Recovering"  => (LexemeSplitShapeTests.Recovering, null, ["[a=b;]", "[ab=;c=d;]", "[abc;d=e;]", "[a=bc]", "a=,=b.", "ab=cd!", "abc"]),
			_             => throw new ArgumentOutOfRangeException(nameof(name)),
		};
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
	static string Answer(Assembly parser, string input)
	{
		parser.GetType("Grammar")!.GetMethod("Reset")?.Invoke(null, null);

		try
		{
			var (accepted, value, error, position) = EmittedCode.Match(parser, "Grammar", "TryParseStart", input);

			return accepted ? $"ok {value}" : $"no {error} @{position}";
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException?.Message == "went round")
		{
			return "LOOP";
		}
	}

	static readonly Dictionary<(string, bool, bool, bool), Assembly> _built = [];

	static Assembly Build(string grammar, string? host, bool settle, bool direct, bool trace)
	{
		lock (_built)
		{
			if (!_built.TryGetValue((grammar, settle, direct, trace), out var assembly))
				_built[(grammar, settle, direct, trace)] = assembly = EmittedCode.Compile(
					Source(grammar, settle, direct, trace, counts: false), "Grammar", declarationMembers: host);

			return assembly;
		}
	}

	static Assembly Counted(string grammar, bool settle)
	{
		return EmittedCode.Compile(Source(grammar, settle, direct: true, trace: false, counts: true), "Grammar", symbols: ["DOTGRAM_COUNTS"]);
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

	static string Source(string grammar, bool settle, bool direct, bool trace, bool counts)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			Direct         = direct,
			Trace          = trace,
			CountRules     = counts,
			SettleRunTails = settle,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return Assert.Single(result.Sources).Text;
	}

	static RecognitionGraph Graph(string grammar)
	{
		var parsed = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance);
		var model  = DotGram.Grammar.Binding.GrammarBinder.Bind(parsed.File, PermissiveSymbolResolver.Instance, grammar.Length);

		return GrammarNormalizer.Normalize(model, PermissiveSymbolResolver.Instance, RoslynCSharpScanner.Instance, settle: true);
	}
}
