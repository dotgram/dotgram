using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The outcome, the position and the value of a reading are the same in every form and in both
/// readers (docs/syntax.md §7.5: "The outcome, the position and the value do not vary this way:
/// those are the same in every form, and what a host branches on"). Each row below is one shape
/// on which the direct reader and the engine answered differently; the expected answer is the
/// one the contract names, so a failing row says which reader is wrong.
/// </summary>
public sealed class FormOutcomeTests
{
	const string Host = """
		static bool Odd(System.ReadOnlySpan<char> input, ref int p) { if (p < input.Length && input[p] == 'o') { p++; return true; } return false; }
		static bool Odd(System.ReadOnlySpan<byte> input, ref int p) { if (p < input.Length && input[p] == (byte)'o') { p++; return true; } return false; }
		""";

	const string Lexical = """
		trivia = { ' '* }
		namespace Lexical
		{
			trivia = none
			Digits : @int = { ['0'..'9']+ } => @(parserText.Length)
		}

		""";

	sealed record Row(string Grammar, string Input, string Form, int At, string Expected, bool Lexical = false, bool Buffered = true, string Rule = "R0");

	/// <summary>
	/// One row a class of disagreement: the shape, the input and the form it shows on, and what
	/// §7.5 (and §6.3 for a reading from a position) says the answer is.
	/// </summary>
	static readonly Dictionary<string, Row> Rows = new()
	{
		// A lookahead that refuses is a refusal at its own place: the furthest position the parse
		// reached (§7.5). The direct reader records nothing for it and answers from before it.
		["LookRefusedAtItsPlace"] = new("R0 = 'z' & ?!(('a')?) & eof\nparse R0\n", "z", "string", 0, "Starved@1"),

		// The same after an optional: `any+` reads to the end, the look is refused there, and more
		// input could still fit — Starved at 2, not NoMatch at 0.
		["LookRefusedAfterRun"] = new("R0 = 'b' & any+ & ?='b' & eof\nparse R0\n", "ba", "string", 0, "Starved@2"),

		// A positive lookahead the engine keeps on its arena records nothing when it fails, and the
		// alternative before it was dispatched past without recording: the answer is then the
		// initial position, before the place the reading began.
		["LookOnArenaRefused"] = new("R0 : @int = 'd' => @(0) | ?=(['a'..'c']+) => @(1)\nparse R0\n", "yz", "at", 1, "NoMatch@1"),

		// A reading that begins where it is told cannot be refused before that place (§6.3).
		["RefusalNotBeforeStart"] = new("trivia = ' '*\nR0 : @int = ?!('a') & 'b' => @(0)\nparse R0\n", "xa", "at", 1, "NoMatch@1"),

		// A literal the input ends inside of, read inside a lookahead, is the look's business:
		// the parse itself was refused at 0 by input that was there (NoMatch), not starved.
		["RunOutInsideLook"] = new("R0 = ?!(\"kk\") & 'z'\nparse R0\n", "k", "string", 0, "NoMatch@0"),

		// `parserText` is the rule's own extent, from where the rule began to where the parse
		// stands (§4): the trivia after the root rule belongs to the whole reading, not to it.
		["RootTextWithoutTrailingTrivia"] = new("trivia = ' '*\nR0 : @int = 'd' => @(parserText.Length)\nparse R0\n", "d ", "string", 0, "Success@0+2 1"),

		// The trivia a positional reading starts on is skipped, not read: `Position` says where the
		// value began, past it (§6.3). The direct reader gives the trivia back to the rule.
		["LeadingTriviaIsSkipped"] = new("trivia = ' '*\nR0 : @int = any => @(0)\nparse R0\n", " ", "at", 0, "Starved@1"),

		// A recovering repetition tries the complete continuation first (§8.2); from a position the
		// continuation is nothing and succeeds, so a refused element ends the repetition.
		// Read over a span only: a host recognizer over a span is not offered the buffered forms.
		["RecoveryEndsFromAPosition"] = new("R1 : @int = 'z' & ';' => @(0)\nR0 : @int[] = R1* recover @Odd => @(0)\nparse R0\n", "z", "at", 0, "Success@0+0 []", Buffered: false),

		// A literal the input ends inside has run out where every character agrees with it (§7.5):
		// the engine reads the rule as a scan and loses that reach.
		["ScannedLiteralRanOut"] = new("R0 = ((R1 & 'r')*)? & eof\nR1 = (\"ab\")?\nparse R0\n", "a", "string", 0, "Starved@1"),

		// A negative lookahead over a braced choice (`eol` is one) after a repetition that can give
		// back: the body refusing is the look succeeding, and the reading goes on to accept. The
		// engine fails the parse there instead.
		["LookOverBracedChoiceRefused"] = new("R0 = (['a'..'c']+)+ & ?!({ eol })\nparse R0\n", "a", "string", 0, "Success@0+1 a"),

		// A rule that is nothing but a negative look, published after another rule in a grammar with
		// trivia: the direct reader names an expected set that is not in the table and throws
		// IndexOutOfRangeException out of a Try method. Both readers refuse it at 0.
		["LookOnlyRulePublishedSecond"] = new("trivia = ' '*\nR1 = 'a'\nR0 = ?!(\"ab\")\nparse R1 as P1\nparse R0\n", "ab", "string", 0, "NoMatch@0"),

		// A run-out inside a positive look nested in a negative one is the negative look's business
		// (it holds where its body refuses): the parse is refused at 0 by input that was there. The
		// engine reads R1 as a scan, which marks the run-out and answers Starved at 1, inside the look.
		["RunOutInsidePositiveLookInsideNegativeLook"] = new("R1 = ?!(any & ?=(\"kK\"i))\nR0 : @int = R1 => @(parserText.Length)\nparse R0\n", "b", "string", 0, "NoMatch@0"),

		// The same rule called where no construction reads its text: both readers scan it and answer
		// Starved at 1, where the inline spelling `R0 = ?!(any & ?=("kK"i)) & 'q'` is not starved.
		["RunOutInsidePositiveLookInsideNegativeLookScanned"] = new("R1 = ?!(any & ?=(\"kK\"i))\nR0 : @int = R1 => @(0)\nparse R0\n", "b", "string", 0, "NoMatch@0"),

		// A positive look that ran out inside a repetition, beside a comment trivia: the string form of
		// both readers is starved at 0; the direct reader over a reader says NoMatch (the engine agrees
		// with the string form).
		["PositiveLookRanOutOverReader"] = new("trivia = (' ' | \"--\" & [^ '\\n']* & ?=eol)*\nR0 = (?=(\"xy\" & \"σς\"i & when @(true)) & \"aσ\"i){1,2}\nparse R0\n", "x", "reader", 0, "Starved@0"),

		// Both readers were starved at 0 before: `"ab"` inside `?=(H1)` runs out on `a` in a positive
		// look, which keeps the mark. The engine now says NoMatch: the mark it lost is the one the
		// negative look of the first alternative used to leave behind, and its positive look over a
		// guarded rule never wrote one of its own.
		["RunOutInPositiveLookAfterNegativeAlternative"] = new("R0 = ?!(\"ab\") | ?=(H1)\nH1 : @int = g: ('a' | \"ab\") & when @(g.Length > 1) => @(g.Length)\nparse R0\n", "a", "string", 0, "Starved@0"),

		// A look refused inside the trivia records nothing, and leaves nothing behind either: the
		// negative look after the trivia is refused at 1 in every form of the engine, as in the
		// direct reader. The engine fed through a window carried the trivia look's unrecorded
		// refusal into the outer look's, which then went unrecorded too, and answered 0.
		["NegativeLookAfterATriviaLook"] = new("trivia = { ('#' & ?!'#')* }\nR0 = 'a' & ?!('b'*)\nparse R0\n", "a##", "string", 0, "NoMatch@1"),
		["NegativeLookAfterATriviaLookStreamed"] = new("trivia = { ('#' & ?!'#')* }\nR0 = 'a' & ?!('b'*)\nparse R0\n", "a##", "stream", 0, "NoMatch@1"),
		["NegativeLookAfterATriviaLookRead"] = new("trivia = { ('#' & ?!'#')* }\nR0 = 'a' & ?!('b'*)\nparse R0\n", "a##", "reader", 0, "NoMatch@1"),

		// A repetition in the trivia short of its fewest turns, refused by the look that begins its
		// turn, records nothing either: the direct reader recorded the turn's start, and answered
		// from inside the trivia where the engine answered from the rule.
		["TriviaRepetitionBelowItsFewestAtALook"] = new("trivia = { ('#' & (?!\"ab\" & any)+)* }\nR0 : @int = 'z' => @(parserText.Length)\nparse R0\n", "#ab", "string", 0, "NoMatch@0"),
		["TriviaRepetitionBelowItsFewestAtALookRead"] = new("trivia = { ('#' & (?!\"ab\" & any)+)* }\nR0 : @int = 'z' => @(parserText.Length)\nparse R0\n", "#ab", "reader", 0, "NoMatch@0"),
		["TriviaRepetitionBelowItsFewestAtALookStreamed"] = new("trivia = { ('#' & (?!\"ab\" & any)+)* }\nR0 : @int = 'z' => @(parserText.Length)\nparse R0\n", "#ab", "stream", 0, "NoMatch@0"),

		// The trivia of a namespace a rule outside it reaches: the look refused inside it records
		// nothing, and leaves nothing behind for the negative look after the call either, whichever
		// is compiled first. The engine fed through a window answered 0 where the namespace came
		// after the rule.
		["NegativeLookAfterANamespacesTriviaLook"] = new("R0 : @int = S.R1 & ?!('b'*) => @(0)\nnamespace S\n{\n\ttrivia = { ('#' & ?!'#')* }\n\tR1 = 'a' & 'b'*\n}\nparse R0\n", "a##", "string", 0, "NoMatch@1"),
		["NegativeLookAfterANamespacesTriviaLookRead"] = new("R0 : @int = S.R1 & ?!('b'*) => @(0)\nnamespace S\n{\n\ttrivia = { ('#' & ?!'#')* }\n\tR1 = 'a' & 'b'*\n}\nparse R0\n", "a##", "reader", 0, "NoMatch@1"),
		["NegativeLookAfterANamespacesTriviaLookStreamed"] = new("R0 : @int = S.R1 & ?!('b'*) => @(0)\nnamespace S\n{\n\ttrivia = { ('#' & ?!'#')* }\n\tR1 = 'a' & 'b'*\n}\nparse R0\n", "a##", "stream", 0, "NoMatch@1"),

		// A look nested in the look that begins a trivia repetition's turn, short of its fewest
		// turns: the scan of the seam raised how far it got where the turn began, and the
		// engine's string form answered from inside the trivia where its other forms, and the
		// direct reader, answered from the rule.
		["TriviaRepetitionBelowItsFewestAtANestedLook"] = new("trivia = { ('#' & (?!(?=\"ab\") & any)+)* }\nR0 : @int = 'z' => @(parserText.Length)\nparse R0\n", "#ab", "string", 0, "NoMatch@0"),
		["TriviaRepetitionBelowItsFewestAtANestedLookRead"] = new("trivia = { ('#' & (?!(?=\"ab\") & any)+)* }\nR0 : @int = 'z' => @(parserText.Length)\nparse R0\n", "#ab", "reader", 0, "NoMatch@0"),
		["TriviaRepetitionBelowItsFewestAtANestedLookStreamed"] = new("trivia = { ('#' & (?!(?=\"ab\") & any)+)* }\nR0 : @int = 'z' => @(parserText.Length)\nparse R0\n", "#ab", "stream", 0, "NoMatch@0"),

		// A literal spelled like a name the generator writes for itself is read as any other: the
		// mark the engine once settled by rewriting the method's text rewrote the literal too.
		["LiteralSpelledLikeTheResetMark"] = new("trivia = { ('#' & ?!'#')* }\nR0 : @int = \"__DotGram_UnrecordedReset__;\" & when @(true) => @(0)\nparse R0\n", "__DotGram_UnrecordedReset__;", "string", 0, "Success@0+28 0"),
		["LiteralSpelledLikeTheResetMarkRead"] = new("trivia = { ('#' & ?!'#')* }\nR0 : @int = \"__DotGram_UnrecordedReset__;\" & when @(true) => @(0)\nparse R0\n", "__DotGram_UnrecordedReset__;", "reader", 0, "Success@0+28 0"),
		["LiteralSpelledLikeTheSentinel"] = new("trivia = { ('#' & ?!'#')* }\nR0 : @int = \"RecognizeUnrecorded_DotGram\" => @(0)\nparse R0\n", "RecognizeUnrecorded_DotGram", "string", 0, "Success@0+27 0"),

		// A look refused inside a sequence inside a trivia repetition short of its fewest turns: the
		// scan of the seam went quietly out of the sequence and loudly out of the repetition, and
		// the engine's string form answered from inside the trivia.
		["TriviaRepetitionBelowItsFewestAtALookInASequence"] = new("trivia = { ('#' & ('a' & ?!'b')+)* }\nR0 : @int = 'z' => @(parserText.Length)\nparse R0\n", "#ab", "string", 0, "NoMatch@0"),
		["TriviaRepetitionBelowItsFewestAtALookInASequenceRead"] = new("trivia = { ('#' & ('a' & ?!'b')+)* }\nR0 : @int = 'z' => @(parserText.Length)\nparse R0\n", "#ab", "reader", 0, "NoMatch@0"),
		["TriviaRepetitionBelowItsFewestAtALookInASequenceStreamed"] = new("trivia = { ('#' & ('a' & ?!'b')+)* }\nR0 : @int = 'z' => @(parserText.Length)\nparse R0\n", "#ab", "stream", 0, "NoMatch@0"),
		["TriviaRepetitionBelowItsFewestAtALookAfterAChoice"] = new("trivia = { ('#' & (('a' | 'c') & ?!'b')+)* }\nR0 : @int = 'z' => @(parserText.Length)\nparse R0\n", "#ab", "string", 0, "NoMatch@0"),
		["TriviaRepetitionBelowItsFewestAtALookInNestedRepetitions"] = new("trivia = { ('#' & (('a' & ?!'b'){1,2} & 'x')+)* }\nR0 : @int = 'z' => @(parserText.Length)\nparse R0\n", "#ab", "string", 0, "NoMatch@0"),

		// Inside the trivia a refusal is recorded only where the input ran out (§7.5): a look refused
		// there, an alternative refused beside it, a rule it calls refused, a turn given back — all
		// record nothing, and the parse answers from the rule after the trivia. Found by a breaker
		// and the harness: the readers split on which of these each recorded.
		["QuietLookAlternativeInTrivia"] = new("trivia = { ('#' & (?!\"ab\" & any | 'z')+)* }\nR0 = 'z'\nparse R0\n", "#ab", "string", 0, "NoMatch@0"),
		["QuietLookAlternativeInTriviaRead"] = new("trivia = { ('#' & (?!\"ab\" & any | 'z')+)* }\nR0 = 'z'\nparse R0\n", "#ab", "reader", 0, "NoMatch@0"),
		["QuietLookAlternativeInTriviaStreamed"] = new("trivia = { ('#' & (?!\"ab\" & any | 'z')+)* }\nR0 = 'z'\nparse R0\n", "#ab", "stream", 0, "NoMatch@0"),
		["QuietLookSecondAlternativeInTrivia"] = new("trivia = (' ' | '#' & ('y' | ?!\"ab\" & any)+)*\nR0 = 'z'\nparse R0\n", "#ab", "string", 0, "NoMatch@0"),
		["QuietLookInACalledRuleInTrivia"] = new("trivia = ('#' & (NotA)+)*\nNotA = ?!'a' & any\nR0 = 'z'\nparse R0\n", "#a", "string", 0, "NoMatch@0"),
		["QuietLookInACalledRuleInTriviaRead"] = new("trivia = ('#' & (NotA)+)*\nNotA = ?!'a' & any\nR0 = 'z'\nparse R0\n", "#a", "reader", 0, "NoMatch@0"),
		["QuietLookInACalledRuleInTriviaStreamed"] = new("trivia = ('#' & (NotA)+)*\nNotA = ?!'a' & any\nR0 = 'z'\nparse R0\n", "#a", "stream", 0, "NoMatch@0"),
		["QuietLookInACountedCalledRuleInTrivia"] = new("trivia = ('#' & (H0){2,})*\nH0 = ?!(\"ab\" | 'y') & any\nR0 = 'z'\nparse R0\n", "#y", "string", 0, "NoMatch@0"),
		["BracedTriviaTurnGivenBack"] = new("trivia = { ('#' & 'y'+)* }\nR0 = 'z'\nparse R0\n", "#a", "string", 0, "NoMatch@0"),
		["BracedTriviaTurnGivenBackRead"] = new("trivia = { ('#' & 'y'+)* }\nR0 = 'z'\nparse R0\n", "#a", "reader", 0, "NoMatch@0"),

		// And where the input ran out inside the trivia the parse is starved there: a literal the
		// input ends inside, a terminal refused at the end, a positive look whose body ran out — a
		// comment left open at the end of the input wanted more.
		["BracedTriviaTurnAtTheEnd"] = new("trivia = { ('#' & 'y'+)* }\nR0 = 'z'\nparse R0\n", "#", "string", 0, "Starved@1"),
		["BracedTriviaTurnAtTheEndRead"] = new("trivia = { ('#' & 'y'+)* }\nR0 = 'z'\nparse R0\n", "#", "reader", 0, "Starved@1"),
		["TriviaRunAtTheEnd"] = new("trivia = (' ' | '#' & ('y'){2,})*\nR0 = 'z'\nparse R0\n", "#", "string", 0, "Starved@1"),
		["PositiveLookRanOutInTrivia"] = new("trivia = (' ' | '#' & ?=\"ab\" & [^ ' ']*)*\nR0 = 'c'\nparse R0\n", "#a", "string", 0, "Starved@1"),
		["PositiveLookRanOutInTriviaRead"] = new("trivia = (' ' | '#' & ?=\"ab\" & [^ ' ']*)*\nR0 = 'c'\nparse R0\n", "#a", "reader", 0, "Starved@1"),
		["PositiveLookAtTheEndInTrivia"] = new("trivia = (' ' | '#' & ?=\"ab\" & [^ ' ']*)*\nR0 = 'c'\nparse R0\n", "#", "string", 0, "Starved@1"),
		["PositiveLookOverAGuardedRuleRanOutInTrivia"] = new("trivia = { (' ' | '#' & ?=H & [^ ' ']*)* }\nH : @int = g: ('a' | \"ab\") & when @(g.Length > 1) => @(g.Length)\nR0 = 'c'\nparse R0\n", "#a", "string", 0, "Starved@1"),
		["PositiveLookOverAGuardedRuleRanOutInTriviaRead"] = new("trivia = { (' ' | '#' & ?=H & [^ ' ']*)* }\nH : @int = g: ('a' | \"ab\") & when @(g.Length > 1) => @(g.Length)\nR0 = 'c'\nparse R0\n", "#a", "reader", 0, "Starved@1"),
		["UnclosedCommentAtTheEnd"] = new("trivia = (' ' | \"--\" & [^ '\\n']* & ?=eol)*\nR0 = ['a'..'c']+\nparse R0\n", "--", "string", 0, "Starved@2"),

		// A rule both the trivia and the grammar call is read as the call stands: refused inside the
		// trivia it says nothing, refused where the grammar called it, it is recorded as any rule.
		["SharedRuleCalledFromTriviaAndTheGrammar"] = new("trivia = { (' ' | '#' & W)* }\nW = 'x' & 'y'\nR0 = 'a' & W\nparse R0\n", "axz", "string", 0, "NoMatch@2"),
		["SharedRuleCalledFromTriviaAndTheGrammarRead"] = new("trivia = { (' ' | '#' & W)* }\nW = 'x' & 'y'\nR0 = 'a' & W\nparse R0\n", "axz", "reader", 0, "NoMatch@2"),
		["SharedRuleCalledFromTriviaAndTheGrammarStreamed"] = new("trivia = { (' ' | '#' & W)* }\nW = 'x' & 'y'\nR0 = 'a' & W\nparse R0\n", "axz", "stream", 0, "NoMatch@2"),
		["SharedRuleWithALookCalledFromTriviaAndTheGrammar"] = new("trivia = { (' ' | '#' & W)* }\nW = 'x' & ?!'z' & any\nR0 = 'a' & W\nparse R0\n", "axz", "string", 0, "NoMatch@2"),
		["SharedRuleWithALookCalledFromTriviaAndTheGrammarRead"] = new("trivia = { (' ' | '#' & W)* }\nW = 'x' & ?!'z' & any\nR0 = 'a' & W\nparse R0\n", "axz", "reader", 0, "NoMatch@2"),
		["SharedRuleRefusedInsideTheTrivia"] = new("trivia = { (' ' | '#' & W)* }\nW = 'x' & 'y'\nR0 = 'a' & W\nparse R0\n", "#xz", "string", 0, "NoMatch@0"),
		["SharedRuleRefusedInsideTheTriviaRead"] = new("trivia = { (' ' | '#' & W)* }\nW = 'x' & 'y'\nR0 = 'a' & W\nparse R0\n", "#xz", "reader", 0, "NoMatch@0"),
		["SharedRuleSayingItsRefusal"] = new("trivia = { (' ' | '#' & W)* }\nW : @int on fail \"no W\" = 'x' & 'y' => @(0)\nR0 : @int = 'a' & W => @(0)\nparse R0\n", "axz", "string", 0, "NoMatch@2"),
		["SharedRuleSayingItsRefusalInsideTheTrivia"] = new("trivia = { (' ' | '#' & W)* }\nW : @int on fail \"no W\" = 'x' & 'y' => @(0)\nR0 : @int = 'a' & W => @(0)\nparse R0\n", "#xz", "string", 0, "NoMatch@0"),

		// §7.5's example: a block comment left open at the end of the input starves the parse where
		// the comment stands.
		["BlockCommentLeftOpenAtTheEnd"] = new("trivia = { (' ' | Std.BlockComment(\"/*\", \"*/\"))* }\nR0 = 'z' & 'q'\nparse R0\n", "z /* x", "string", 0, "Starved@6"),
		["BlockCommentLeftOpenAtTheEndRead"] = new("trivia = { (' ' | Std.BlockComment(\"/*\", \"*/\"))* }\nR0 = 'z' & 'q'\nparse R0\n", "z /* x", "reader", 0, "Starved@6"),

		// Found by review of round nine: four more sites that recorded inside the trivia where the
		// input had not run out, or lost a run-out that had — a scanned positive look's body, a
		// choice resumed inside the trivia after a rule returned, the direct reader's dispatch
		// recorders, and the engine's guard and recognizer failures.
		["ScannedPositiveLookBodyRanOutInTrivia"] = new("trivia = { ('#' & ?=\"ab\" & any*)* }\nR0 = 'z'\nparse R0\n", "#a", "string", 0, "Starved@1"),
		["ScannedPositiveLookBodyRanOutInTriviaRead"] = new("trivia = { ('#' & ?=\"ab\" & any*)* }\nR0 = 'z'\nparse R0\n", "#a", "reader", 0, "Starved@1"),
		["ChoiceResumedInsideTriviaAfterAReturn"] = new("trivia = ('#' & W)*\nW = 'x' & ('y' | 'y' & 'q' & 'r')\nR0 = 'a' & W\nparse R0\n", "#xyqz", "string", 0, "NoMatch@3"),
		["ChoiceResumedInsideTriviaAfterAReturnRead"] = new("trivia = ('#' & W)*\nW = 'x' & ('y' | 'y' & 'q' & 'r')\nR0 = 'a' & W\nparse R0\n", "#xyqz", "reader", 0, "NoMatch@3"),
		["ChoiceResumedInsideTriviaAfterAReturnStreamed"] = new("trivia = ('#' & W)*\nW = 'x' & ('y' | 'y' & 'q' & 'r')\nR0 = 'a' & W\nparse R0\n", "#xyqz", "stream", 0, "NoMatch@3"),
		["GuardedAlternativeInTrivia"] = new("trivia = { ('#' & (when @(true) & 'x' | 'y'))* }\nR0 = 'z'\nparse R0\n", "#a", "string", 0, "NoMatch@0"),
		["GuardedAlternativeInTriviaRead"] = new("trivia = { ('#' & (when @(true) & 'x' | 'y'))* }\nR0 = 'z'\nparse R0\n", "#a", "reader", 0, "NoMatch@0"),
		["RecursiveRuleInTrivia"] = new("trivia = { ('#' & W)* }\nW = 'x' & (W | 'y')\nR0 = 'a' & W\nparse R0\n", "#xxz", "string", 0, "NoMatch@0"),
		["RecursiveRuleInTriviaRead"] = new("trivia = { ('#' & W)* }\nW = 'x' & (W | 'y')\nR0 = 'a' & W\nparse R0\n", "#xxz", "reader", 0, "NoMatch@0"),
		["GuardRefusedInTrivia"] = new("trivia = { ('#' & 'a' & when @(false))* }\nR0 = 'z'\nparse R0\n", "#az", "string", 0, "NoMatch@0"),
		["GuardRefusedInTriviaRead"] = new("trivia = { ('#' & 'a' & when @(false))* }\nR0 = 'z'\nparse R0\n", "#az", "reader", 0, "NoMatch@0"),
		["RecognizerRefusedInTrivia"] = new("trivia = { ('#' & 'a' & @Odd)* }\nR0 = 'z'\nparse R0\n", "#az", "string", 0, "NoMatch@0"),

		// Over kinds, a repetition short of its minimum that stopped at the end of the tokens says
		// what it wanted: the direct reader says nothing and answers from the start.
		["TokenRepetitionBelowMinimum"] = new(Lexical + "R0 = Lexical.Digits & ['a'..'c']+\nparse R0\n", "0", "string", 0, "Starved@1", Lexical: true),

		// A positive look over a rule the engine reads as a scan: the literal the input ends inside
		// marks the input as run out (§7.5), as it does read by methods, and the look keeps the
		// mark. The scanner lost it, and the engine's string form said NoMatch where its buffered
		// forms and the direct reader said Starved.
		["LookOverScannedLiteralRanOut"] = new("R0 : @int = ?=R1 & when @(true) & 'z' => @(0)\nR1 = { \"ab\" }\nparse R0\n", "a", "string", 0, "Starved@0"),

		// A recovering repetition at the end of a reading from a position reads its fewest turns
		// (§8.2, §6.3): the reading has nothing after the rule, so the complete continuation is
		// tried first and succeeds at once, and a valid first element is not read. Both readers
		// answer so, as v0.2.0 did.
		["RecoveryFromAPositionReadsNothing"] = new("R1 : @int = 'z' & ';' => @(1)\nR0 : @int[] = R1* recover @Odd => @(0)\nparse R0\n", "z;", "at", 0, "Success@0+0 []", Buffered: false),

		// The recipe §8.2 gives for reading a recovering list from a position, spelled as the doc
		// spells it, typed and untyped: a rule that ends with what ends the list. From a position it
		// then recovers to the end, the bad element included.
		["RecoveryToTheEndFromAPosition"] = new("Row : @int = 'z' & ';' => @(1)\nRows : @int[] = Row* recover @Odd => @(0)\nRowsToEnd : @int[] = r: Rows & eof => @(r)\nparse RowsToEnd\n", "z;q", "at", 0, "Success@0+3 [1,0]", Buffered: false, Rule: "RowsToEnd"),
		["RecoveryToTheEndFromAPositionUntyped"] = new("Row = 'z' & ';'\nRows = Row* recover @Odd\nRowsToEnd = Rows & eof\nparse RowsToEnd\n", "z;q", "at", 0, "Success@0+3 z;q", Buffered: false, Rule: "RowsToEnd"),
	};

	public static TheoryData<string> Names()
	{
		var names = new TheoryData<string>();

		foreach (var name in Rows.Keys)
			names.Add(name);

		return names;
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void Both_readers_answer_what_the_contract_names(string name)
	{
		var row = Rows[name];

		foreach (var direct in new[] { true, false })
		{
			var parser = Build(row.Grammar, direct, row.Lexical, positionalFollow: false, buffered: row.Buffered);
			var answer = Answer(parser, row.Rule, row.Form, row.Input, row.At);

			Assert.True(answer == row.Expected, $"{name} ({(direct ? "direct" : "engine")}, {row.Form}): expected {row.Expected}, got {answer}\n{row.Grammar}");
		}
	}

	/// <summary>
	/// A reading from a position gives the rule's first reading (§6.3). These shapes answer so
	/// in both readers when the grammar is compiled knowing it is read from a position
	/// (<see cref="GramCompilerOptions.PositionalFollow"/>). By default the rule is compiled
	/// against the end of the input for both kinds of reading, as §6.3 documents, and each
	/// reader then cuts one of them short on the strength of that end: that is the documented
	/// default, not a disagreement this class holds the readers to.
	/// </summary>
	[Theory]
	[InlineData("R0 : @int = ((\"yz\"){1,2})? => @(0)\nparse R0\n", "ya", "Success@0+0 0")]
	[InlineData("R0 = \"ab\" & ('d' & 'r')* & 'd'\nparse R0\n", "abdrc", "Success@0+3 abd")]
	[InlineData("R0 = Std.Quoted('\"')\nparse R0\n", "\"qa\"\"", "Success@0+4 \"qa\"")]
	public void A_reading_from_a_position_is_the_rules_first_reading_when_compiled_for_one(string grammar, string input, string expected)
	{
		foreach (var direct in new[] { true, false })
		{
			var parser = Build(grammar, direct, lexical: false, positionalFollow: true, buffered: true);
			var answer = Answer(parser, "R0", "at", input, 0);

			Assert.True(answer == expected, $"{(direct ? "direct" : "engine")}: expected {expected}, got {answer}\n{grammar}");
		}
	}

	/// <summary>
	/// The outcome, the position and the value are the same in every form of one reader too (§7.5):
	/// the string, a reader, a stream and bytes. A rule read as a scan kept no mark of input that
	/// ran out, and the engine's string form said NoMatch where every other form said Starved.
	/// </summary>
	[Theory]
	[InlineData("R0 : @int = ?=R1 & when @(true) & 'z' => @(0)\nR1 = { \"ab\" }\nparse R0\n", "a")]
	[InlineData("R0 : @int = ?=R1 & when @(true) & 'z' => @(0)\nR1 = { \"ab\" }\nparse R0\n", "ab")]
	[InlineData("R0 = ((R1 & 'r')*)? & eof\nR1 = (\"ab\")?\nparse R0\n", "a")]
	[InlineData("R0 = ?!(\"abc\") & any*\nparse R0\n", "a")]
	[InlineData("R0 = (\"xy\"i)? & 'z'\nparse R0\n", "xc")]
	public void Every_form_of_a_reader_answers_alike(string grammar, string input)
	{
		foreach (var direct in new[] { true, false })
		{
			var parser  = Build(grammar, direct, lexical: false, positionalFollow: false, buffered: true);
			var answers = new[] { "string", "reader", "stream", "bytes" }
				.Select(form => (form, answer: Answer(parser, "R0", form, input, 0)))
				.Where(static one => one.answer is not null)
				.ToList();

			Assert.True(answers.All(one => one.answer == answers[0].answer),
				$"{(direct ? "direct" : "engine")}: " + string.Join(", ", answers.Select(static one => $"{one.form} {one.answer}")) + "\n" + grammar);
		}
	}

	/// <summary>
	/// The direct reader puts the mark of input that ran out back after a negative look only where
	/// a literal of more than one character inside could have written it: a look over a class or a
	/// single character has no mark to put back, and keeping it was two writes on every turn of
	/// <c>(?!'x' &amp; any)+</c> for nothing.
	/// </summary>
	[Theory]
	[InlineData("R0 : @int = (?!'x' & any)+ & when @(true) => @(0)\nparse R0\n", false)]
	[InlineData("R0 : @int = (?!['x' | 'y'] & any)+ & when @(true) => @(0)\nparse R0\n", false)]
	[InlineData("R0 : @int = (?!(\"xy\") & any)+ & when @(true) => @(0)\nparse R0\n", true)]
	[InlineData("R0 : @int = (?!R1 & any)+ & when @(true) => @(0)\nR1 = 'q' & \"xy\"\nparse R0\n", true)]
	public void A_negative_look_keeps_the_mark_only_where_a_literal_could_run_out(string grammar, bool counted)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			Direct         = true,
			BufferedInput  = true,
			SymbolResolver = PermissiveSymbolResolver.Instance,
			CSharpScanner  = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

		var source = string.Concat(result.Sources.Select(static one => one.Text));

		Assert.Equal(counted, source.Contains("= failure.OutOfInput;", StringComparison.Ordinal));
	}

	[Theory]
	[InlineData("trivia = ' '*\nR0 = ?!(\"ab\")\nR1 = 'a'\nparse R0\nparse R1 as P1\n", "ab")]
	[InlineData("trivia = ' '*\nR1 = 'a'\nR0 = ?=(\"ab\")\nparse R0\nparse R1 as P1\n", "xy")]
	public void A_refused_look_rule_names_nothing_another_rule_expects(string grammar, string input)
	{
		// What Error names would have been accepted where it says (§7.5): `'a'` and `' '` are
		// another rule's, and R0 cannot begin with either.
		foreach (var direct in new[] { true, false })
		{
			var parser = Build(grammar, direct, lexical: false, positionalFollow: false, buffered: true);
			var match  = parser.GetType("Grammar")!.GetMethod("TryParseR0", [typeof(string)])!.Invoke(null, [input])!;
			var error  = (string?)match.GetType().GetProperty("Error")!.GetValue(match);

			Assert.True(error is not null && !error.Contains("'a'", StringComparison.Ordinal), $"{(direct ? "direct" : "engine")}: {error}\n{grammar}");
		}
	}

	[Theory]
	[InlineData("trivia = { (' ' | '#' & ?!'#' & [^ '\\n']*)* }\nR0 : @int = 'a' => @(parserText.Length)\nparse R0\n", "##x")]
	[InlineData("trivia = { (' ' | '#' & ?!'#' & [^ '\\n']*)* }\nR0 : @int = 'a' => @(parserText.Length)\nparse R0\n", " ##x")]
	public void A_negative_look_refused_inside_trivia_reads_alike_in_both_readers(string grammar, string input)
	{
		// Base: both readers refuse `'a'` at 0. Now the direct reader answers at 1, where the look
		// stood inside the trivia, and says nothing it expected; the engine still answers 0.
		var direct = Build(grammar, direct: true, lexical: false, positionalFollow: false, buffered: true);
		var engine = Build(grammar, direct: false, lexical: false, positionalFollow: false, buffered: true);

		foreach (var form in new[] { "string", "at", "reader", "stream" })
		{
			var d = Answer(direct, "R0", form, input, 0);
			var e = Answer(engine, "R0", form, input, 0);

			Assert.True(d == e, $"{form} \"{input}\": direct {d}, engine {e}\n{grammar}");
		}
	}

	// --- Differential ---------------------------------------------------------------------

	/// <summary>
	/// Random grammars over the whole syntax, every input up to five characters over a small
	/// alphabet, read whole as a string, from a reader, from a stream and from bytes: the direct
	/// reader and the engine give one outcome, one position and one value, and so does each reader
	/// in every one of those forms.
	/// </summary>
	[Fact]
	public void Random_grammars_read_alike_whole_in_both_readers()
	{
		var failures = Differential(seed: 20261026, count: 40, forms: ["string", "reader", "stream", "bytes"], positionalFollow: false);

		Assert.True(failures.Count == 0, string.Join("\n", failures));
	}

	/// <summary>
	/// The same from a position, with the grammar compiled knowing it is read from one: what
	/// remains is not the follow of the whole forms but the readers' own disagreements.
	/// </summary>
	[Fact]
	public void Random_grammars_read_alike_from_a_position_in_both_readers()
	{
		var failures = Differential(seed: 20261026, count: 40, forms: ["at"], positionalFollow: true);

		Assert.True(failures.Count == 0, string.Join("\n", failures));
	}

	static List<string> Differential(int seed, int count, string[] forms, bool positionalFollow)
	{
		var random   = new Random(seed);
		var failures = new List<string>();
		var compiled = 0;

		for (var round = 0; round < count * 3 && compiled < count; round++)
		{
			var grammar = new Generator(random).Build();

			Assembly direct, engine;

			// A host recognizer over a span is not offered the buffered forms.
			var buffered = !grammar.Contains("@Odd", StringComparison.Ordinal);

			try
			{
				direct = Build(grammar, direct: true, lexical: false, positionalFollow, buffered);
				engine = Build(grammar, direct: false, lexical: false, positionalFollow, buffered);
			}
			catch (Xunit.Sdk.XunitException)
			{
				// Refused by the compiler (a shape the generator does not keep clear of), or the
				// emitted source did not compile: not this test's question.
				continue;
			}

			compiled++;

			var found = 0;

			foreach (var input in Inputs("abc; ", 5))
			{
				string? firstDirect = null, firstEngine = null, firstForm = null;

				foreach (var form in forms)
				{
					var at = form == "at" ? Math.Min(1, input.Length) : 0;
					var d  = Answer(direct, "R0", form, input, at);
					var e  = Answer(engine, "R0", form, input, at);

					if (d != e && found++ < 3)
						failures.Add($"round {round}, {form} \"{input}\": direct {d}, engine {e}\n{grammar}");

					// And each reader agrees with itself across the forms (§7.5).
					if (firstForm is null)
					{
						(firstDirect, firstEngine, firstForm) = (d, e, form);
					}
					else if (d is not null && e is not null && (d != firstDirect || e != firstEngine) && found++ < 3)
					{
						failures.Add($"round {round}, \"{input}\": {firstForm} direct {firstDirect} engine {firstEngine}, {form} direct {d} engine {e}\n{grammar}");
					}
				}

				if (found >= 3)
					break;
			}
		}

		Assert.True(compiled == count, $"only {compiled} of {count} grammars compiled");

		return failures;
	}

	/// <summary>Random grammars over the syntax the readers disagree on: sequences, choices, repetitions, looks, captures, recovery, guards, trivia.</summary>
	sealed class Generator(Random random)
	{
		readonly List<string?> _types = [];

		int _looking;

		bool Chance(int percent)
		{
			return random.Next(100) < percent;
		}

		T Pick<T>(params T[] items)
		{
			return items[random.Next(items.Length)];
		}

		public string Build()
		{
			var count = 1 + random.Next(3);

			for (var i = 0; i < count; i++)
				_types.Add(Pick<string?>(null, null, "@int", "@int", "@int[]"));

			for (var i = 0; i < count; i++)
				if (_types[i] == "@int[]" && !_types.Skip(i + 1).Contains("@int"))
					_types[i] = "@int";

			var text = new StringBuilder();

			if (Chance(40))
				text.Append(Pick("trivia = ' '*\n", "trivia = { ' '* }\n"));

			for (var i = 0; i < count; i++)
				text.Append(Rule(i)).Append('\n');

			text.Append("parse R0\n");

			return text.ToString();
		}

		string Rule(int index)
		{
			var type = _types[index];
			var head = type is null ? $"R{index}" : $"R{index} : {type}";

			if (type == "@int[]")
			{
				var element = Element(index)!;
				var sync    = Pick("';'", "@Odd", "(';' & when @(true))");

				return $"{head} = {element}{Pick("*", "+")}" + (Chance(70) ? $" recover {sync}" + (Chance(40) ? " => @(0)" : "") : "");
			}

			var alternatives = new List<string>();

			for (var i = 0; i < 1 + (Chance(35) ? 1 : 0); i++)
				alternatives.Add(Alternative(index, type));

			var body = string.Join(" | ", alternatives);

			if (index == 0 && type is null && Chance(40) && alternatives.Count == 1)
				body = $"{body} & eof";

			return $"{head} = {body}";
		}

		string? Element(int index)
		{
			for (var i = index + 1; i < _types.Count; i++)
				if (_types[i] == "@int")
					return $"R{i}";

			return null;
		}

		string Alternative(int index, string? type)
		{
			var operands = new List<string>();
			var captures = 0;
			var length   = 1 + random.Next(3);

			for (var i = 0; i < length; i++)
			{
				var operand = Operand(index, 2);

				if (type == "@int" && Chance(40) && !operand.Contains("@Odd", StringComparison.Ordinal) && !operand.StartsWith('?'))
				{
					operands.Add($"c{captures}: {operand}");

					if (Chance(25))
						operands.Add($"when @(c{captures}.Length >= 0)");

					captures++;
				}
				else
				{
					operands.Add(operand);
				}
			}

			var sequence = string.Join(" & ", operands);

			if (type == "@int")
			{
				var sum = captures == 0 ? Pick("0", "parserText.Length") : string.Join(" + ", Enumerable.Range(0, captures).Select(static one => $"c{one}.Length"));

				return $"{sequence} => @({sum})";
			}

			return sequence;
		}

		string Operand(int index, int depth)
		{
			var roll = random.Next(depth > 0 ? 24 : 12);

			switch (roll)
			{
				case 0: return "'a'";
				case 1: return "'b'";
				case 2: return "\"ab\"";
				case 3: return "['a'..'c']";
				case 4: return "['a'..'c']+";
				case 5: return "[^ ';' | ' ']+";
				case 6: return "any";
				case 7: return "';'";
				case 8: return "@Odd";
				case 9: return Call(index);
				case 10: return Call(index);
				case 11: return "'d' & when @(true)";
			}

			var looks = roll is 18 or 19;

			if (looks)
				_looking++;

			var inner = Operand(index, depth - 1);

			if (looks)
				_looking--;

			var body = Empty(inner) ? $"{inner} & 'r'" : inner;

			switch (roll)
			{
				case 12: return $"({inner})?";
				case 13: return $"({body})*";
				case 14: return $"({body})+";
				case 15: return $"({body}){{1,2}}";
				case 16: return $"{{ {inner} }}";
				case 17: return $"({inner} | {Operand(index, depth - 1)})";
				case 18: return $"?=({inner})";
				case 19: return $"?!({inner})";
				case 20: return "(?!';' & any)* & ';'";
				case 21: return $"({inner} & {Operand(index, depth - 1)})";
				case 22 when _looking == 0: return $"({body})* recover {Pick("';'", "@Odd")}";
				default: return inner;
			}
		}

		static bool Empty(string operand)
		{
			return operand.EndsWith('?') || operand.EndsWith(")*", StringComparison.Ordinal) ||
				operand.StartsWith('?') || operand.Contains("recover", StringComparison.Ordinal) ||
				operand.Contains(" | ", StringComparison.Ordinal) || operand.StartsWith('R') ||
				operand.StartsWith('{') || operand.StartsWith('(') || operand.Contains("when", StringComparison.Ordinal);
		}

		string Call(int index)
		{
			if (index + 1 >= _types.Count)
				return "'z'";

			return $"R{index + 1 + random.Next(_types.Count - index - 1)}";
		}
	}

	// --- Building and asking --------------------------------------------------------------

	static Assembly Build(string grammar, bool direct, bool lexical, bool positionalFollow, bool buffered)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName        = "Grammar",
			Direct           = direct,
			Lexical          = lexical,
			BufferedInput    = buffered,
			BufferedBytes    = buffered,
			PositionalFollow = positionalFollow,
			SymbolResolver   = PermissiveSymbolResolver.Instance,
			CSharpScanner    = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

		return EmittedCode.Compile(
			result.Sources[0].Text, declarationMembers: Host,
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	static IEnumerable<string> Inputs(string alphabet, int longest)
	{
		var level = new List<string> { "" };

		yield return "";

		for (var length = 1; length <= longest; length++)
		{
			var next = new List<string>(level.Count * alphabet.Length);

			foreach (var head in level)
				foreach (var one in alphabet)
				{
					var input = head + one;

					next.Add(input);

					yield return input;
				}

			level = next;
		}
	}

	/// <summary>
	/// What a publication answers in one form, as one string: the outcome and the position of a
	/// refusal, or where a success began, its length and its value. Null where the form is not
	/// published for this grammar.
	/// </summary>
	static string? Answer(Assembly parser, string rule, string form, string input, int at)
	{
		var type   = parser.GetType("Grammar")!;
		var name   = "TryParse" + rule;
		var method = form switch
		{
			"string" => type.GetMethod(name, [typeof(string)]),
			"at"     => type.GetMethod(name, [typeof(string), typeof(int)]),
			"reader" => type.GetMethod(name, [typeof(TextReader), typeof(int?), typeof(int?)]),
			"stream" => type.GetMethod(name, [typeof(Stream), typeof(int?), typeof(int?)]),
			"bytes"  => type.GetMethod(name, [typeof(byte[])]),
			_        => throw new ArgumentOutOfRangeException(nameof(form)),
		};

		if (method is null)
			return null;

		object match;

		try
		{
			match = form switch
			{
				"string" => method.Invoke(null, [input])!,
				"at"     => method.Invoke(null, [input, at])!,
				"reader" => method.Invoke(null, [new StringReader(input), 1, null])!,
				"bytes"  => method.Invoke(null, [Encoding.UTF8.GetBytes(input)])!,
				_        => method.Invoke(null, [new MemoryStream(Encoding.UTF8.GetBytes(input)), 1, null])!,
			};
		}
		catch (TargetInvocationException thrown)
		{
			return "Threw " + thrown.InnerException!.GetType().Name;
		}

		object? Read(string member)
		{
			return match.GetType().GetProperty(member)!.GetValue(match);
		}

		if (!(bool)Read("IsSuccess")!)
			return $"{Read("Outcome")}@{Read("Position")}";

		var value = Read("Value") switch
		{
			int[] many   => "[" + string.Join(",", many) + "]",
			byte[] bytes => Encoding.UTF8.GetString(bytes),
			null         => "null",
			var one      => one.ToString(),
		};

		return $"Success@{Read("Position")}+{Read("Length")} {value}";
	}
}
