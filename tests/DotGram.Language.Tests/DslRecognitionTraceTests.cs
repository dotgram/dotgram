using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;
using DotGram.Tests;

using Xunit;

namespace DotGram.Language.Tests;

public sealed class DslRecognitionTraceTests
{
	[Fact]
	public void ReportsRuleAndCaptureExtentsFromSuccessfulDerivation()
	{
		var (graph, publication) = Compile("""
			parse Start
			Start = first: Word & (' ' & second: Word)?
			Word = ['a'..'z']+
			""");

		var result = DslRecognitionTrace.Recognize(graph, publication, "one two");

		Assert.Equal(DslRecognitionStatus.Success, result.Status);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Start" && extent.Capture is null &&
			extent.Position == 0 && extent.Length == 7);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Start" && extent.Capture == "first" &&
			extent.Position == 0 && extent.Length == 3);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Start" && extent.Capture == "second" &&
			extent.Position == 4 && extent.Length == 3);
		Assert.Equal(2, result.Extents.Count(extent => extent.Rule.Name == "Word" && extent.Capture is null));
	}

	[Fact]
	public void BacktracksFromEarlierAlternativeWhenFollowingInputFails()
	{
		var (graph, publication) = Compile("""
			parse Start
			Start = ('a' | "ab") & 'c'
			""");

		var result = DslRecognitionTrace.Recognize(graph, publication, "abc");

		Assert.Equal(DslRecognitionStatus.Success, result.Status);
	}

	[Fact]
	public void WholeParseIncludesLeadingAndTrailingTrivia()
	{
		var (graph, publication) = Compile("""
			trivia = ' '*
			parse Start
			Start = word: (['a'..'z']+)
			""");

		var result = DslRecognitionTrace.Recognize(graph, publication, "  word ");

		Assert.Equal(DslRecognitionStatus.Success, result.Status);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Start" && extent.Capture == "word" &&
			extent.Position == 2 && extent.Length == 4);
	}

	[Fact]
	public void AtomicCommitsToFirstSuccessfulAlternative()
	{
		var (graph, publication) = Compile("""
			parse Start
			Start = { 'a' | "ab" } & 'c'
			""");

		var result = DslRecognitionTrace.Recognize(graph, publication, "abc");

		Assert.Equal(DslRecognitionStatus.Failure, result.Status);
	}

	[Fact]
	public void ReportsExpectedElementsAtFurthestFailure()
	{
		var (graph, publication) = Compile("""
			Keyword = "select"
			parse Start
			Start = Keyword & ' ' & ("name" | "count")
			""");

		var result = DslRecognitionTrace.Recognize(graph, publication, "select value");

		Assert.Equal(DslRecognitionStatus.Failure, result.Status);
		Assert.Equal(7, result.FailurePosition);
		Assert.Equal(new[] { "\"count\"", "\"name\"" }, result.Expected);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Keyword" && extent.Position == 0 && extent.Length == 6);
	}

	[Fact]
	public void ReportsOperatorAlternativesAfterTrailingTrivia()
	{
		var (graph, publication) = Compile("""
			trivia    = [' ' | '\t']*
			Identifier = ['a'..'z']+
			Operator   = '+' | '-'
			Operation  = left: Identifier & Operator & right: Identifier
			parse Operation
			""");

		var result = DslRecognitionTrace.Recognize(graph, publication, "customer ");

		Assert.Equal(DslRecognitionStatus.Failure, result.Status);
		Assert.Equal("customer ".Length, result.FailurePosition);
		Assert.Contains("['+' | '-']", result.Expected);
		var successful = DslRecognitionTrace.Recognize(graph, publication, "customer + value");
		Assert.Contains(successful.Extents, extent =>
			extent.Rule.Name == "Operation" && extent.Capture == "left");
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Operation" && extent.Capture == "left" &&
			extent.Position == 0 && extent.Length == "customer".Length);
	}

	[Fact]
	public void DoesNotGuessWhenRecognitionRequiresUserCode()
	{
		var (graph, publication) = ExternalGraph();

		var result = DslRecognitionTrace.Recognize(graph, publication, "anything");

		Assert.Equal(DslRecognitionStatus.Unsupported, result.Status);
	}

	[Fact]
	public void UsesToolingContractForGuardsWithoutExecutingUserCode()
	{
		var (graph, publication) = Compile("""
			parse Start
			Start = when @(Allowed) & value: ['a'..'z']+
			""");
		var accepted = DslRecognitionTrace.Recognize(
			graph,
			publication,
			"word",
			new Contract(guard: true));
		var rejected = DslRecognitionTrace.Recognize(
			graph,
			publication,
			"word",
			new Contract(guard: false));

		Assert.Equal(DslRecognitionStatus.Success, accepted.Status);
		Assert.Contains(accepted.Extents, extent => extent.Capture == "value");
		Assert.Equal(DslRecognitionStatus.Failure, rejected.Status);
	}

	[Fact]
	public void UsesToolingContractForExternalRecognizerExtent()
	{
		var (graph, publication) = ExternalGraph();

		var result = DslRecognitionTrace.Recognize(
			graph,
			publication,
			"name",
			new Contract(externalRule: graph.Rules.Single(rule => rule.Name == "External")));

		Assert.Equal(DslRecognitionStatus.Success, result.Status);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Start" && extent.Position == 0 && extent.Length == 4);
	}

	[Fact]
	public void UsesMappedGrammarRuleForExternalRecognizer()
	{
		var (graph, publication) = ExternalGraph();

		var result = DslRecognitionTrace.Recognize(
			graph,
			publication,
			"other",
			new Contract(externalRule: graph.Rules.Single(rule => rule.Name == "External")));

		Assert.Equal(DslRecognitionStatus.Failure, result.Status);
	}

	[Fact]
	public void UsesDescriptorContractForGuardAndExternalRecognizer()
	{
		var (graph, publication) = Compile("""
			Word = ['a'..'z']+
			Start = when @(Allowed) & @Read
			parse Start
			""", "Word");
		var guard = Nodes(graph.Bodies[publication.Rule]).OfType<Node.Guard>().Single();
		var definition = new DslRecognitionContractDefinition(
			new System.Collections.Generic.Dictionary<string, bool> { [guard.Text] = true },
			new System.Collections.Generic.Dictionary<string, string> { ["Read"] = "Word" });

		var result = DslRecognitionTrace.Recognize(
			graph,
			publication,
			"customer",
			new DslDescriptorRecognitionContract(graph, definition));

		Assert.Equal(DslRecognitionStatus.Success, result.Status);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Word" && extent.Position == 0 && extent.Length == 8);
	}

	[Fact]
	public void DescriptorContractMatchesGeneratedParserOnBoundedCorpus()
	{
		const string grammar = """
			Word = ['a'..'z']+
			Start = when @(true) & @Read
			parse Start
			""";
		var (graph, publication) = Compile(grammar, "Word");
		var guard = Nodes(graph.Bodies[publication.Rule]).OfType<Node.Guard>().Single();
		var contract = new DslDescriptorRecognitionContract(
			graph,
			new DslRecognitionContractDefinition(
				new System.Collections.Generic.Dictionary<string, bool> { [guard.Text] = true },
				new System.Collections.Generic.Dictionary<string, string> { ["Read"] = "Word" }));
		var generated = EmittedCode.Compile(
			DotGram.Grammar.Emit.CSharpEmitter.Emit(
				graph,
				new DotGram.Grammar.GramCompilerOptions { Carrier = DotGram.Grammar.CarrierKind.Deferred, MemoiseFailures = false }),
			declarationMembers: """
				static bool Read(global::System.ReadOnlySpan<char> input, ref int pos)
				{
					var start = pos;
					while (pos < input.Length && input[pos] >= 'a' && input[pos] <= 'z') pos++;
					return pos > start;
				}
				""");

		foreach (var input in new[] { "", "name", "customer", "123", "name1", "Name" })
		{
			var expected = EmittedCode.Match(generated, "Grammar", "TryParseStart", input).IsSuccess;
			var actual = DslRecognitionTrace.Recognize(graph, publication, input, contract).Status ==
				DslRecognitionStatus.Success;

			Assert.Equal(expected, actual);
		}
	}

	/// <summary>
	/// A rule that matched nothing is left before what follows it is read: trivia read
	/// at the start of the input, an operand that reads nothing, and trivia again at the
	/// same position is no rule calling itself.
	/// </summary>
	[Fact]
	public void RuleThatMatchedEmptyIsCalledAgainAtTheSamePosition()
	{
		const string grammar = """
			trivia = ' '*
			parse Start
			Start = Sign & word: Word
			Sign  = '-'?
			Word  = ['a'..'z']+
			""";

		var (graph, publication) = Compile(grammar);
		var result = DslRecognitionTrace.Recognize(graph, publication, "word");

		Assert.Equal(DslRecognitionStatus.Success, result.Status);
		Assert.Contains(result.Extents, extent =>
			extent.Rule.Name == "Start" && extent.Capture == "word" &&
			extent.Position == 0 && extent.Length == 4);
		AgreesWithGeneratedParser(grammar, "word", " word ", "- word", "-word", "--word", "", " ");
	}

	/// <summary>
	/// A rule that calls itself at the position it was entered at is still refused there,
	/// before the match it hands on and after the caller resumes it.
	/// </summary>
	[Fact]
	public void RuleCallingItselfWhereItWasEnteredIsRefusedThere()
	{
		var rule  = new RuleSymbol("Start", new GrammarNamespace("", null), Declaration: null);
		var graph = new RecognitionGraph(
			[rule],
			new System.Collections.Generic.Dictionary<RuleSymbol, Node>
			{
				[rule] = new Node.Choice(
				[
					new Node.Sequence([new Node.Call(rule, []), new Node.Literal("b")]),
					new Node.Sequence([new Node.Literal("a"), new Node.Repeat(new Node.Literal("c"), 0, null)]),
				]),
			},
			new System.Collections.Generic.Dictionary<RuleSymbol, bool> { [rule] = false },
			new System.Collections.Generic.Dictionary<RuleSymbol, System.Collections.Generic.IReadOnlyList<ResultMember>>(),
			new System.Collections.Generic.Dictionary<RuleSymbol, string>(),
			[],
			[],
			[]);
		var publication = new Publication(
			PublishKind.Parse,
			rule,
			"ParseStart",
			new Location(0, 0),
			rule.Namespace,
			new System.Collections.Generic.Dictionary<RuleSymbol, RuleSymbol>(),
			[]);

		Assert.Equal(DslRecognitionStatus.Success, DslRecognitionTrace.Recognize(graph, publication, "acc").Status);
		Assert.Equal(DslRecognitionStatus.Failure, DslRecognitionTrace.Recognize(graph, publication, "accb").Status);
	}

	/// <summary>
	/// Every kind of node the recognition model has is either read or answered as a site
	/// the interpreter cannot read; none is refused as if the text were wrong. A kind added
	/// to the model fails here until it is given a case.
	/// </summary>
	[Fact]
	public void EveryNodeKindIsReadOrAnsweredUnsupported()
	{
		var other   = new RuleSymbol("Other", new GrammarNamespace("", null), Declaration: null);
		var start   = new RuleSymbol("Start", new GrammarNamespace("", null), Declaration: null);
		var letter  = new Node.Element(false, [new CharRange('a', 'z')], [], []);
		var a       = new Node.Literal("a");
		var b       = new Node.Literal("b");
		var binding = new TextBinding(start, 0, "q", -1);
		var success = DslRecognitionStatus.Success;
		var failure = DslRecognitionStatus.Failure;
		var unsupported = DslRecognitionStatus.Unsupported;

		var cases = new System.Collections.Generic.Dictionary<System.Type, (Node Body, string Input, DslRecognitionStatus Expected)[]>
		{
			[typeof(Node.Empty)]     = [(Node.Empty.Instance, "", success), (Node.Empty.Instance, "a", failure)],
			[typeof(Node.Element)]   = [(letter, "q", success), (letter, "Q", failure)],
			[typeof(Node.Literal)]   = [(new Node.Literal("ab"), "ab", success), (new Node.Literal("ab"), "aa", failure)],
			[typeof(Node.Sequence)]  = [(new Node.Sequence([a, b]), "ab", success), (new Node.Sequence([a, b]), "ba", failure)],
			[typeof(Node.Choice)]    =
			[
				(new Node.Choice([a, b]), "b", success),
				(new Node.Choice([a, b]) { Selection = new Node.SwitchSelection(new Node.Guard("kind"), ["1", null]) }, "a", unsupported),
			],
			[typeof(Node.Atomic)]    = [(new Node.Atomic(a), "a", success)],
			[typeof(Node.Marked)]    = [(new Node.Marked(a, "state"), "a", success), (new Node.Marked(a, "state"), "b", failure)],
			[typeof(Node.Repeat)]    = [(new Node.Repeat(a, 1, null), "aaa", success), (new Node.Repeat(a, 1, null), "", failure)],
			[typeof(Node.Lookahead)] =
			[
				(new Node.Sequence([new Node.Lookahead(true, a), letter]), "a", success),
				(new Node.Sequence([new Node.Lookahead(false, a), letter]), "a", failure),
			],
			[typeof(Node.Behind)]    =
			[
				(new Node.Sequence([new Node.Literal("-"), new Node.Behind(letter), b]), "-b", success),
				(new Node.Sequence([a, new Node.Behind(letter), b]), "ab", failure),
				(new Node.Sequence([new Node.Behind(letter), b]), "b", success),
			],
			[typeof(Node.Reading)]   =
			[
				(new Node.Sequence([new Node.Reading(1), a]), "a", success),
				(new Node.Sequence([new Node.Reading(2), a]), "a", failure),
			],
			[typeof(Node.Glue)]      = [(new Node.Sequence([a, Node.Glue.Instance, b]), "ab", success)],
			[typeof(Node.Capture)]   = [(new Node.Capture("x", a), "a", success)],
			[typeof(Node.Bind)]      = [(new Node.Bind(binding, a), "a", unsupported)],
			[typeof(Node.Same)]      = [(new Node.Same(binding), "a", unsupported)],
			[typeof(Node.Counted)]   = [(new Node.Counted(a, new CountBound.Length(binding), null), "a", unsupported)],
			[typeof(Node.Condition)] = [(new Node.Condition(new DotGram.Grammar.Model.Test.Runs("true", -1)), "", unsupported)],
			[typeof(Node.Guard)]     = [(new Node.Guard("ready"), "", unsupported)],
			[typeof(Node.Construct)] = [(new Node.Construct(a, Construction.Sequence.Instance), "a", success)],
			[typeof(Node.External)]  = [(new Node.External("Read"), "a", unsupported)],
			[typeof(Node.Call)]      = [(new Node.Call(other, []), "a", success), (new Node.Call(other, []), "b", failure)],
		};

		var kinds = typeof(Node).Assembly.GetTypes()
			.Where(static type => type.IsSubclassOf(typeof(Node)) && !type.IsAbstract)
			.Select(static type => type.Name)
			.OrderBy(static name => name, System.StringComparer.Ordinal);

		Assert.Equal(kinds, cases.Keys.Select(static type => type.Name).OrderBy(static name => name, System.StringComparer.Ordinal));

		foreach (var (kind, readings) in cases)
		foreach (var (body, input, expected) in readings)
		{
			var graph = new RecognitionGraph(
				[start, other],
				new System.Collections.Generic.Dictionary<RuleSymbol, Node> { [start] = body, [other] = a },
				new System.Collections.Generic.Dictionary<RuleSymbol, bool> { [start] = false, [other] = false },
				new System.Collections.Generic.Dictionary<RuleSymbol, System.Collections.Generic.IReadOnlyList<ResultMember>>(),
				new System.Collections.Generic.Dictionary<RuleSymbol, string>(),
				[],
				[],
				[]);
			var publication = new Publication(
				PublishKind.Parse,
				start,
				"ParseStart",
				new Location(0, 0),
				start.Namespace,
				new System.Collections.Generic.Dictionary<RuleSymbol, RuleSymbol>(),
				[]);

			var actual = DslRecognitionTrace.Recognize(graph, publication, input).Status;

			Assert.True(expected == actual, $"{kind.Name} '{body}' over '{input}': expected {expected}, answered {actual}");
		}
	}

	/// <summary>
	/// A word literal is bounded on both sides, so it is not read at the end of a longer
	/// word an operand before it gave back; <c>~</c> leaves no room for trivia.
	/// </summary>
	[Fact]
	public void ReadsWordBoundariesAndGlueAsTheGeneratedParserDoes()
	{
		const string grammar = """
			wordboundary = ['a'..'z']
			trivia = ' '*
			parse Start
			Start = left: Name & "in" & right: Reference
			Reference = Name ~ '.' ~ Name | Name
			Name = ['a'..'z']+
			""";

		var (graph, publication) = Compile(grammar);

		Assert.Equal(DslRecognitionStatus.Success, DslRecognitionTrace.Recognize(graph, publication, "ab in c.d").Status);
		Assert.Equal(DslRecognitionStatus.Failure, DslRecognitionTrace.Recognize(graph, publication, "abin cd").Status);
		Assert.Equal(DslRecognitionStatus.Failure, DslRecognitionTrace.Recognize(graph, publication, "ab in c . d").Status);
		AgreesWithGeneratedParser(grammar, "ab in c.d", "abin cd", "ab incd", "ab in c . d", "ab in c. d", "in in in", "a in b");
	}

	/// <summary>
	/// A look does not say how far it looked: a refusal is placed where the look stands,
	/// not where what it read inside gave out.
	/// </summary>
	[Fact]
	public void RefusalInsideLookIsPlacedWhereTheLookStands()
	{
		const string grammar = """
			parse Start
			Start   = "type:" & Segment & ('.' & Segment)*
			Segment = "**" | ?!Keyword & ['a'..'z']+
			Keyword = ("static" | "public") & ?!['a'..'z']
			""";

		var (graph, publication) = Compile(grammar);
		var result = DslRecognitionTrace.Recognize(graph, publication, "type:a.static");

		Assert.Equal(DslRecognitionStatus.Failure, result.Status);
		Assert.Equal("type:a.".Length, result.FailurePosition);
		Assert.Equal(new[] { "\"**\"" }, result.Expected);
		AgreesWithGeneratedParser(grammar, "type:a.static", "type:static", "type:statics", "type:a.**", "type:a.publi", "type:", "type:a.");
	}

	/// <summary>
	/// A call is remembered with every place it ended, its first derivations among them, though
	/// its last derivation was the one that left no other route behind.
	/// </summary>
	/// <remarks>
	/// <c>Join</c> reads the comment to the end of the line, finds no <c>and</c> after it, and
	/// gives the comment back character by character; <c>End</c> then calls <c>Comment</c> where
	/// <c>Join</c> did, and must still be able to read it to the end of the line.
	/// </remarks>
	[Fact]
	public void RememberedCallKeepsEveryPlaceItEnded()
	{
		const string grammar = """
			parse Start
			Start   = Line & ('\n' & Line)*
			Line    = Word & (Join & "and " & Word)* & End
			Join    = ' '* & Comment? & '\n' & ?="and"
			End     = ' '* & Comment?
			Comment = '#' & [^ '\n']*
			Word    = ['a'..'z']+
			""";

		var (graph, publication) = Compile(grammar);

		Assert.Equal(DslRecognitionStatus.Success, DslRecognitionTrace.Recognize(graph, publication, "a # ok\nb").Status);
		AgreesWithGeneratedParser(grammar, "a # ok\nb", "a # ok\nand b", "a # ok\nand b # x\nc", "a #\n#", "a # ok\n");
	}

	/// <summary>
	/// A site nested as deep as its text makes it is read on the stack an editor gives it: the
	/// interpreter keeps what is left to do on the heap.
	/// </summary>
	[Fact]
	public void ReadsDeeplyNestedSiteOnSmallStack()
	{
		const string grammar = """
			trivia = ' '*
			parse Start
			Start = Expr
			Expr  = '(' & Expr & ')' | '<' & Expr & '>' | name: (['a'..'z']+)
			""";
		const int depth = 2000;

		var (graph, publication) = Compile(grammar);
		var input  = string.Concat(Enumerable.Repeat("( <", depth)) + " x " + string.Concat(Enumerable.Repeat("> )", depth));
		var open   = string.Concat(Enumerable.Repeat("( <", depth)) + " x";
		var result = default(DslRecognitionResult);
		var failed = default(DslRecognitionResult);
		var thread = new System.Threading.Thread(
			() =>
			{
				result = DslRecognitionTrace.Recognize(graph, publication, input);
				failed = DslRecognitionTrace.Recognize(graph, publication, open);
			},
			1024 * 1024);

		thread.Start();
		thread.Join();

		Assert.Equal(DslRecognitionStatus.Success, result!.Status);
		Assert.Equal(2 * depth + 1, result.Extents.Count(static extent => extent.Rule.Name == "Expr" && extent.Capture is null));
		Assert.Equal(DslRecognitionStatus.Failure, failed!.Status);
		Assert.Equal(open.Length, failed.FailurePosition);
		AgreesWithGeneratedParser(grammar, "(<x>)", "( < x > )", "((x)", "(<x)>", "<<<x>>>");
	}

	/// <summary>
	/// Text a grammar can divide in exponentially many ways is refused after as many steps as it
	/// has positions: a repetition reached again at a position, and a call that ends again where
	/// it ended before, read on as they did, and are not followed twice.
	/// </summary>
	[Theory]
	[InlineData("Start = Item* & 'y'\nItem = 'a' | \"aa\"")]
	[InlineData("Start = Run & 'y'\nRun = Item & Run | none\nItem = 'a' | \"aa\"")]
	public void AmbiguousTextIsRefusedWithoutFollowingEveryDivision(string rules)
	{
		var (graph, publication) = Compile("parse Start\n" + rules);
		var input  = new string('a', 200) + "z";
		var result = default(DslRecognitionResult);
		var thread = new System.Threading.Thread(() => result = DslRecognitionTrace.Recognize(graph, publication, input));

		thread.IsBackground = true;
		thread.Start();

		Assert.True(thread.Join(System.TimeSpan.FromSeconds(30)), "the interpreter followed every division of the text");
		Assert.Equal(DslRecognitionStatus.Failure, result!.Status);
		Assert.Equal(200, result.FailurePosition);
	}

	/// <summary>
	/// Every text up to five characters long over a small alphabet, read by the interpreter and
	/// by the parser generated from the same grammar: backtracking into calls and repetitions,
	/// atomic groups, looks both ways, bounded repetitions, word boundaries, ignore-case, trivia.
	/// </summary>
	[Theory]
	[InlineData("ab(). ", """
		trivia = ' '*
		parse Start
		Start = Item+ & End
		Item  = { 'a' & 'b'? } | '(' & Item* & ')' | "ba" | 'b' & ?!'a'
		End   = '.' | ?!['a'..'z' | '(' | ')']
		""")]
	[InlineData("aAb=. ", """
		wordboundary = ['a'..'z' | 'A'..'Z']
		trivia = ' '*
		parse Start
		Start = (Pair | Word){1,3} & Tail
		Pair  = left: Word & '=' & right: Word
		Word  = "ab"i | 'a' & 'a'? & 'b'?
		Tail  = '.' | none
		""")]
	public void AgreesWithGeneratedParserOnEveryShortInput(string alphabet, string grammar)
	{
		var inputs = new System.Collections.Generic.List<string> { "" };

		for (var length = 1; length <= 5; length++)
			inputs.AddRange(inputs.Where(input => input.Length == length - 1).SelectMany(input => alphabet.Select(c => input + c)).ToArray());

		AgreesWithGeneratedParser(grammar, [.. inputs]);

		var (graph, publication) = Compile(grammar);
		var accepted = inputs.Count(input => DslRecognitionTrace.Recognize(graph, publication, input).Status == DslRecognitionStatus.Success);

		// Both answers are common enough to say something.
		Assert.InRange(accepted, 100, inputs.Count - 100);
	}

	/// <summary>
	/// What the interpreter answers for each input against what the parser generated
	/// from the same grammar answers: accepted by both, or refused by both at the same
	/// position.
	/// </summary>
	static void AgreesWithGeneratedParser(string grammar, params string[] inputs)
	{
		var (graph, publication) = Compile(grammar);
		var generated = EmittedCode.Compile(
			DotGram.Grammar.Emit.CSharpEmitter.Emit(
				graph,
				new DotGram.Grammar.GramCompilerOptions { Carrier = DotGram.Grammar.CarrierKind.Deferred, MemoiseFailures = false }));

		foreach (var input in inputs)
		{
			var expected = EmittedCode.Match(generated, "Grammar", "Try" + publication.MethodName, input);
			var actual   = DslRecognitionTrace.Recognize(graph, publication, input);

			Assert.True(
				expected.IsSuccess == (actual.Status == DslRecognitionStatus.Success),
				$"'{input}': the generated parser {(expected.IsSuccess ? "accepts" : "refuses")} it, the interpreter answers {actual.Status}");

			if (!expected.IsSuccess)
				Assert.True(
					expected.Position == actual.FailurePosition,
					$"'{input}': the generated parser refuses at {expected.Position} ({expected.Error}), the interpreter at {actual.FailurePosition} ({string.Join(", ", actual.Expected)})");
		}
	}

	static (RecognitionGraph Graph, Publication Publication) ExternalGraph()
	{
		var rule = new RuleSymbol("Start", new GrammarNamespace("", null), Declaration: null);
		var external = new RuleSymbol("External", new GrammarNamespace("", null), Declaration: null);
		var graph = new RecognitionGraph(
			[rule, external],
			new System.Collections.Generic.Dictionary<RuleSymbol, Node>
			{
				[rule] = new Node.External("Read"),
				[external] = new Node.Literal("name"),
			},
			new System.Collections.Generic.Dictionary<RuleSymbol, bool> { [rule] = false, [external] = false },
			new System.Collections.Generic.Dictionary<RuleSymbol, System.Collections.Generic.IReadOnlyList<ResultMember>>(),
			new System.Collections.Generic.Dictionary<RuleSymbol, string>(),
			[],
			[],
			[]);
		var publication = new Publication(
			PublishKind.Parse,
			rule,
			"ParseStart",
			new Location(0, 0),
			rule.Namespace,
			new System.Collections.Generic.Dictionary<RuleSymbol, RuleSymbol>(),
			[]);

		return (graph, publication);
	}

	sealed class Contract(bool? guard = null, RuleSymbol? externalRule = null) : IDslRecognitionContract
	{
		public bool TryEvaluateGuard(Node.Guard guardNode, RuleSymbol owner, int position, out bool accepted)
		{
			accepted = guard.GetValueOrDefault();
			return guard.HasValue;
		}

		public bool TryResolveExternal(
			Node.External external,
			RuleSymbol owner,
			out RuleSymbol rule)
		{
			rule = externalRule!;
			return externalRule is not null;
		}
	}

	static System.Collections.Generic.IEnumerable<Node> Nodes(Node node)
	{
		yield return node;
		foreach (var child in node.Children)
		foreach (var descendant in Nodes(child))
			yield return descendant;
	}

	static (RecognitionGraph Graph, Publication Publication) Compile(
		string source,
		params string[] contractRules)
	{
		var parsed = GramParser.Parse(GramLexer.Tokenize(source, RoslynCSharpScanner.Instance));
		var model  = GrammarBinder.Bind(parsed.File);
		var graph  = DslGrammarNormalizer.Normalize(model, contractRules);

		Assert.Empty(parsed.Diagnostics);
		Assert.Empty(model.Diagnostics);
		Assert.Empty(graph.Diagnostics);

		return (graph, Assert.Single(
			graph.Publications,
			static publication => !DslGrammarNormalizer.IsToolingPublication(publication)));
	}
}
