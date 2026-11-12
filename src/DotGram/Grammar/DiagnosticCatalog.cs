using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>
/// Every identifier .Gram reports, with its title and the severity it is reported with: the one
/// place either is decided.
/// </summary>
/// <remarks>
/// <para>
/// The titles are the "What it says" column of <c>docs/diagnostics.md</c>, and the severities its
/// Severity column; a test holds the three to one another, and the severity every call site passes
/// to this table. A <see cref="GramDiagnostic"/> takes its severity from here wherever its
/// identifier is listed, so the build and the editor, which both make them, cannot disagree on it.
/// </para>
/// <para>
/// A retired number is not here: it is reported by nothing, and the reference lists it so that a
/// suppression written against it is recognizable as dead.
/// </para>
/// </remarks>
static class DiagnosticCatalog
{
	/// <summary>One identifier's title and severity.</summary>
	public readonly record struct Entry(string Title, GramSeverity Severity);

	/// <summary>Every identifier in use, by identifier.</summary>
	public static IReadOnlyDictionary<string, Entry> Entries { get; } = new Dictionary<string, Entry>(StringComparer.Ordinal)
	{
		["GRAM0001"] = new("The generator itself failed", GramSeverity.Error),
		["GRAM0002"] = new("A class hosting a grammar must be partial", GramSeverity.Error),
		["GRAM0003"] = new("No grammar file for a [Gram] class", GramSeverity.Error),
		["GRAM0004"] = new("More than one grammar file matches", GramSeverity.Error),
		["GRAM0005"] = new("The name a grammar is included under is not an identifier", GramSeverity.Error),
		["GRAM0006"] = new("Two readings of one grammar want the same scope", GramSeverity.Error),
		["GRAM0007"] = new("The name a grammar is compiled under is not an identifier", GramSeverity.Error),
		["GRAM0008"] = new("Two grammars are included under one name", GramSeverity.Error),
		["GRAM0009"] = new("A buffer option is not positive", GramSeverity.Error),
		["GRAM0010"] = new("The generated parser needs C# 8 or later", GramSeverity.Error),
		["GRAM0011"] = new("The location type declares no Locate", GramSeverity.Error),
		["GRAM0012"] = new("A name was renamed in 0.3.0", GramSeverity.Error),
		["GRAM0017"] = new("A local grammar file is not the grammar a referenced assembly carries", GramSeverity.Info),
		["GRAM1001"] = new("Unterminated character literal", GramSeverity.Error),
		["GRAM1002"] = new("Unterminated string literal", GramSeverity.Error),
		["GRAM1003"] = new("Unterminated comment", GramSeverity.Error),
		["GRAM1004"] = new("Unrecognized escape sequence", GramSeverity.Error),
		["GRAM1005"] = new("Unexpected character", GramSeverity.Error),
		["GRAM1006"] = new("Unterminated inline @(…) expression", GramSeverity.Error),
		["GRAM1007"] = new("An inline @(…) expression needs a C# scanner, and none was supplied", GramSeverity.Error),
		["GRAM1008"] = new("Expected a Unicode category in the form \\p{Lu}", GramSeverity.Error),
		["GRAM1009"] = new("Not a Unicode category", GramSeverity.Error),
		["GRAM1010"] = new("A character literal holds one character", GramSeverity.Error),
		["GRAM1011"] = new("$ or # needs a name right after it", GramSeverity.Error),
		["GRAM1012"] = new("A name contains _DotGram", GramSeverity.Error),
		["GRAM2001"] = new("Expected a particular token", GramSeverity.Error),
		["GRAM2002"] = new("Expected a rule, a namespace or a publication directive", GramSeverity.Error),
		["GRAM2003"] = new("Expected a literal, a reference, an element set, a group or an atomic group", GramSeverity.Error),
		["GRAM2004"] = new("Expected a name", GramSeverity.Error),
		["GRAM2005"] = new("A binding power is a whole number from 0 to 9999", GramSeverity.Error),
		["GRAM2006"] = new("A namespace header's rebindings need with", GramSeverity.Error),
		["GRAM2007"] = new("A publication of anything but a rule's name needs as", GramSeverity.Error),
		["GRAM2008"] = new("A named yield publication places a result type before yield", GramSeverity.Error),
		["GRAM2009"] = new("A condition after when whose operand is not followed by is", GramSeverity.Error),
		["GRAM2010"] = new("A length #q stands only inside count braces", GramSeverity.Error),
		["GRAM2011"] = new("Something stands more than 128 levels deep inside others: groups, braces, lookaheads, captures, bindings, a call's arguments, a condition's parentheses, a C# type's arguments, namespaces", GramSeverity.Error),
		["GRAM2012"] = new("positional on a find or on a parse … yield", GramSeverity.Error),
		["GRAM2013"] = new("The words before a directive's verb are out of order, one is written twice, or one stands after a directive on its line", GramSeverity.Error),
		["GRAM2014"] = new("A character range is malformed: nothing after its .. (['a'..]), a string as its bound (['a'..\"z\"]), or written outside brackets ('a'..'z')", GramSeverity.Error),
		["GRAM2015"] = new("A case-insensitive character is an element of a set or a bound of a range: ['a'i], ['a'..'z'i]", GramSeverity.Error),
		["GRAM2016"] = new("Type arguments after a name of the grammar: B<int>", GramSeverity.Error),
		["GRAM2017"] = new("A second quantifier on one operand: 'a'*?, 'a'+*, 'a'{2}?", GramSeverity.Error),
		["GRAM2019"] = new("A using or @using after the first declaration of its file or namespace block", GramSeverity.Error),
		["GRAM2020"] = new("A switch's braces hold something that is not a case label, a default or a body; or a label is not a whole number or a string, or is a number too large for a long (a ulong if it is not negative)", GramSeverity.Error),
		["GRAM2021"] = new("A switch label, or its default, is written twice", GramSeverity.Error),
		["GRAM2022"] = new("A switch's labels are numbers and strings both", GramSeverity.Error),
		["GRAM2023"] = new("A switch has no case, or a label has no body after its :", GramSeverity.Error),
		["GRAM2024"] = new("A side of is has a quantifier: when Version is ['0'..'9']+", GramSeverity.Error),
		["GRAM3001"] = new("A rule of this name is already defined in this namespace", GramSeverity.Error),
		["GRAM3002"] = new("No rule of that name", GramSeverity.Error),
		["GRAM3003"] = new("No namespace of that name is in view", GramSeverity.Error),
		["GRAM3004"] = new("No C# type of that name is in view", GramSeverity.Error),
		["GRAM3005"] = new("Two publications would generate the same method", GramSeverity.Error),
		["GRAM3006"] = new("A rebinding's left side names no visible rule", GramSeverity.Error),
		["GRAM3007"] = new("A rebinding's right side names no visible rule", GramSeverity.Error),
		["GRAM3008"] = new("A rule is bound more than once in one with", GramSeverity.Error),
		["GRAM3009"] = new("A rebinding's two sides take different numbers of parameters", GramSeverity.Error),
		["GRAM3010"] = new("A rule bound by the active namespace's own header cannot be redeclared", GramSeverity.Error),
		["GRAM3011"] = new("A rebinding is circular", GramSeverity.Error),
		["GRAM3012"] = new("A rule shadows one from an enclosing namespace or an import", GramSeverity.Error),
		["GRAM3014"] = new("A grammar declares two context types", GramSeverity.Error),
		["GRAM3016"] = new("A grammar declares two state types", GramSeverity.Error),
		["GRAM3019"] = new("The context handed to a parse does not satisfy a contract it inherited", GramSeverity.Error),
		["GRAM3020"] = new("Two grammars in one composition declare a different state", GramSeverity.Error),
		["GRAM3021"] = new("$q or #q where the rule binds nothing called q", GramSeverity.Error),
		["GRAM3022"] = new("Two imports at one scope each declare a trivia, and the scope declares none", GramSeverity.Error),
		["GRAM4001"] = new("The body of a repetition can match without consuming input", GramSeverity.Error),
		["GRAM4002"] = new("Left recursion the rewrite cannot take", GramSeverity.Error),
		["GRAM4003"] = new("trivia must accept empty input", GramSeverity.Error),
		["GRAM4005"] = new("A name in an element set is not a rule", GramSeverity.Error),
		["GRAM4006"] = new("A capture inside a lookahead, or one read there", GramSeverity.Error),
		["GRAM4007"] = new("One name is captured twice with different types", GramSeverity.Error),
		["GRAM4008"] = new("A => is not on an alternative, or a rule that builds does not say what type", GramSeverity.Error),
		["GRAM4009"] = new("An alternative is recursive and states no strength while its siblings do", GramSeverity.Error),
		["GRAM4010"] = new("A recovery's => has no sequence to put the rejected element in", GramSeverity.Error),
		["GRAM4011"] = new("A rule's declared type is neither a C# type nor a rule in view", GramSeverity.Error),
		["GRAM4012"] = new("A capture or a rule's parameter takes one of the supplied names, or one of the names the generated parser gives parameters of its own: parserSource, parserStarts, parserLengths, parserReading, parserLocating, parserWhole", GramSeverity.Error),
		["GRAM4013"] = new("A call of a parameterized rule that cannot be built, or a value where a piece of grammar goes", GramSeverity.Error),
		["GRAM4014"] = new("A rebinding's replacement produces an incompatible result", GramSeverity.Error),
		["GRAM4015"] = new("An external recognizer has more than one value-returning overload with different T", GramSeverity.Error),
		["GRAM4016"] = new("Two alternatives begin with the same operand, and that operand leads back to the rule", GramSeverity.Warning),
		["GRAM4017"] = new("Two substitutions each need the other applied first: two rules each contain a with that reaches the other, a namespace's header reaches a with that calls back into the namespace from outside it, or two namespaces' headers each reach, from outside, a rule the other rebinds", GramSeverity.Error),
		["GRAM4018"] = new("A rule nothing reaches", GramSeverity.Warning),
		["GRAM4019"] = new("word in a grammar that never said what continues a word", GramSeverity.Error),
		["GRAM4020"] = new("A when … is … somewhere other than beside the operands of an alternative", GramSeverity.Error),
		["GRAM4021"] = new("A when … is … whose sides are not two listable sets of strings", GramSeverity.Error),
		["GRAM4022"] = new("Every alternative of a rule is ruled out by a condition in this parser", GramSeverity.Info),
		["GRAM4023"] = new("A grammar published in more than 64 readings", GramSeverity.Error),
		["GRAM4024"] = new("A when that leaves different C# behind in different readings", GramSeverity.Error),
		["GRAM4025"] = new("A C# method a grammar names cannot be called the way its position calls it", GramSeverity.Error),
		["GRAM4026"] = new("An explicitly requested buffered input form is unsupported", GramSeverity.Error),
		["GRAM4027"] = new("A yield publication cannot safely produce the requested element sequence", GramSeverity.Error),
		["GRAM4028"] = new("A publication result does not fit its explicit C# contract", GramSeverity.Error),
		["GRAM4029"] = new("A mark is placed, or parserState/parserMarks asked for, in a grammar that declares no state", GramSeverity.Error),
		["GRAM4030"] = new("A mark's value names a capture of its own alternative", GramSeverity.Error),
		["GRAM4031"] = new("A guard in a fold step names another step's name for the value so far, which this step has none of", GramSeverity.Error),
		["GRAM4032"] = new("A reference or a length may be read where nothing bound its text", GramSeverity.Error),
		["GRAM4033"] = new("A binding's text read where a value goes", GramSeverity.Error),
		["GRAM4034"] = new("A binding, reference or length where it is not supported yet", GramSeverity.Error),
		["GRAM4035"] = new("A binding, reference or length in a rule read over token kinds", GramSeverity.Error),
		["GRAM4036"] = new("A with expression inside an instance of a parameterized rule made only while another rebinding was applied", GramSeverity.Error),
		["GRAM4037"] = new("Std.Nested(open, close) called with arguments a counter cannot tell apart", GramSeverity.Error),
		["GRAM4038"] = new("A recover inside a lookahead", GramSeverity.Error),
		["GRAM4039"] = new("A step of a left-recursive rule with no => captures something: its leading call, or an operand after it", GramSeverity.Error),
		["GRAM4040"] = new("An argument of a call into a namespace with a header, written outside that namespace, names a rule the header rebinds", GramSeverity.Error),
		["GRAM4041"] = new("A with expression inside an argument of a parameterized rule", GramSeverity.Error),
		["GRAM4042"] = new("A strength on an alternative of a climbing rule reaches no operand", GramSeverity.Warning),
		["GRAM4043"] = new("A rule is spelled from single characters where trivia is not empty", GramSeverity.Warning),
		["GRAM4044"] = new("A recover in a rule the lexer reads", GramSeverity.Error),
		["GRAM4045"] = new("A recover inside another one's repetition, or a synchronization with a guard, shares what a rule of its own could not", GramSeverity.Error),
		["GRAM4046"] = new("A rule that is told where it was written builds a Nullable of a struct", GramSeverity.Error),
		["GRAM4047"] = new("A literal, or a class item, is U+0000 in a grammar that names eos", GramSeverity.Error),
		["GRAM4048"] = new("Text is captured, bound or published across an eos, or parserText or parserInput would hold one", GramSeverity.Error),
		["GRAM4049"] = new("Lexical = true over a grammar that names eos", GramSeverity.Error),
		["GRAM4050"] = new("A buffered or byte form is asked of a grammar that names eos", GramSeverity.Error),
		["GRAM4051"] = new("trivia is not atomic, and a class beside one of its seams reads a character it reads", GramSeverity.Warning),
		["GRAM4052"] = new("A repetition can read one run of text in two ways", GramSeverity.Warning),
		["GRAM4053"] = new("A supplied name is written where it is not handed over: parserInput, parserState or parserMarks in a when; one of the parser's own parameters (parserSource, parserReading, …, GRAM4012) anywhere; a rejected element's name (parserMessage, parserLine, …) in a =>; the marks in a recover factory; or a supplied name used as a method, a type or a variable, where it is reserved", GramSeverity.Error),
		["GRAM4054"] = new("Two alternatives of a rule read a nested instance of the rule's own recursion before either can be refused", GramSeverity.Info),
		["GRAM4055"] = new("positional parse in a grammar that names eos", GramSeverity.Error),
		["GRAM4056"] = new("A construction's value cannot be the type it is declared to give: a capture handed back whole (=> x, => @(x)) that holds text where an array or a value type is declared, or a generated type where another type is; a grammar's type built with the wrong number of values, or from a capture that cannot be its member (=> Pair(right)); a rule's name built or taken as a type where the grammar generates none for it, because it declares its own type or captures nothing", GramSeverity.Error),
		["GRAM4057"] = new("What a rule can begin with does not settle", GramSeverity.Error),
		["GRAM4058"] = new("C# in a rule — a guard, a switch's selector, a construction, a mark — names one of the rule's parameters that stands for a piece of grammar: Value(tag) = switch @(Kind(tag)) { … }", GramSeverity.Error),
		["GRAM4059"] = new("A grammar publishes nothing, and nothing includes it", GramSeverity.Warning),
		["GRAM4060"] = new("A binding inside a lookahead", GramSeverity.Error),
		["GRAM4061"] = new("One name bound twice on one route", GramSeverity.Error),
		["GRAM4062"] = new("A reference or a length inside its own binding", GramSeverity.Error),
		["GRAM4063"] = new("A binding and its reference on two sides of a constructing group", GramSeverity.Error),
		["GRAM4064"] = new("A binding in a step of a left-recursive rule, or around its leading call to itself", GramSeverity.Error),
		["GRAM5001"] = new("A publication gets no overload taking a reader, or, in a grammar that names eos, reads its segments whole", GramSeverity.Info),
		["GRAM5002"] = new("A repetition can begin with the same input as what follows it", GramSeverity.Info),
		["GRAM5003"] = new("A generated method is left past the size at which the JIT stops optimizing it", GramSeverity.Warning),
		["GRAM5004"] = new("Lexical = true was asked for and the grammar cannot be cut in two", GramSeverity.Warning),
		["GRAM5005"] = new("A split grammar's syntactic half cannot be read by methods, so it runs on the shared engine", GramSeverity.Warning),
		["GRAM5007"] = new("The immediate carrier was asked for by name and is not what carries some of the grammar", GramSeverity.Warning),
		["GRAM5009"] = new("An optional or a repetition can take what follows it, where nothing can give it back", GramSeverity.Info),
		["GRAM5011"] = new("A terminal of a split grammar is nothing but @M", GramSeverity.Warning),
		["GRAM5012"] = new("A grammar left to the generator is carried as Immediate, and Deferred would hold some of its constructions back", GramSeverity.Info),
		["GRAM5014"] = new("A buffered form is read by the engine where its string form is read by methods", GramSeverity.Info),
		["GRAM5013"] = new("Whether a refused input is read again over a context put back as it was", GramSeverity.Info),
		["GRAM5016"] = new("A grammar left to the generator is carried as Immediate, and Deferred would be safer", GramSeverity.Warning),
		["GRAM5017"] = new("A construction of a publication read from a reader or lines changes the context", GramSeverity.Info),
		["GRAM5018"] = new("A name would be written twice into the generated class", GramSeverity.Error),
		["GRAM5019"] = new("A guard that names context is read again where a terminal's value is built", GramSeverity.Warning),
		["GRAM6001"] = new("A classification target is neither Rule nor Rule.capture", GramSeverity.Warning),
		["GRAM6002"] = new("The grammar rule a classification target names was not found", GramSeverity.Warning),
		["GRAM6003"] = new("The grammar rule a classification target names is ambiguous", GramSeverity.Warning),
		["GRAM6004"] = new("The capture a classification target names was not found in its rule", GramSeverity.Warning),
		["GRAM6005"] = new("A classification target is specified more than once", GramSeverity.Warning),
		["GRAM6101"] = new("The text of an embedded DSL site does not match its language", GramSeverity.Error),
	};

	/// <summary>The identifier's title, or the identifier itself where it is not listed.</summary>
	public static string TitleOf(string id)
	{
		return Entries.TryGetValue(id, out var entry) ? entry.Title : id;
	}

	/// <summary>The severity the identifier is reported with, or <paramref name="otherwise"/> where it is not listed.</summary>
	public static GramSeverity SeverityOf(string id, GramSeverity otherwise)
	{
		return Entries.TryGetValue(id, out var entry) ? entry.Severity : otherwise;
	}
}
