using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Language;

/// <summary>
/// What the editor says about the words and symbols of the notation: the words completion
/// offers, and the text Quick Info shows for a keyword, a built-in rule, a name the parser
/// supplies, a symbol, a Unicode category and a rule of the standard library.
/// </summary>
/// <remarks>
/// <para>
/// No Visual Studio type is used here, so that the generator's own tests compile this file
/// too and hold every table against what the generator knows: the words its parser takes,
/// <see cref="GrammarBinder.BuiltIn"/>, <see cref="SuppliedNames.All"/>, the fixed spellings of
/// <see cref="TokenKind"/>, the categories of <see cref="UnicodeCategories"/> and the rules of
/// <see cref="StandardLibrary"/>. A word the parser starts to take and this table does not
/// describe fails a test there, rather than going unoffered in the editor.
/// </para>
/// <para>
/// The built-in rules, the supplied names and the standard library are read from the
/// generator's tables; only the descriptions are written here.
/// </para>
/// </remarks>
public static class GramVocabulary
{
	/// <summary>
	/// Every word the parser reads as part of the notation, with what it does. Most are words only
	/// where they stand in that place: a rule may still be called <c>state</c> or <c>on</c>.
	/// </summary>
	public static IReadOnlyDictionary<string, string> Keywords { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["using"]     = "imports a grammar namespace, as in 'using Lexical;'; '@using' imports a C# namespace",
		["namespace"] = "declares a grammar namespace; 'namespace N with (A = B) { ... }' rebinds names inside it",
		["parse"]     = "publishes a rule as a parse method: 'parse Rule with (A = B) as Name : @T stream bytes yield', every part after the rule optional",
		["find"]      = "publishes a rule as a search method, which finds where the rule matches in the input",
		["as"]        = "names the method a 'parse' or 'find' publishes",
		["with"]      = "rebinds names for an expression, a namespace or a publication: 'with (A = B)'; 'with state @(...)' sets the parser state while its operand is read",
		["when"]      = "a guard: the alternative goes on only where the condition holds, as in 'when @IsValid(x)', 'when @(x > 0)' or 'when Version is \"2022\"'",
		["is"]        = "in a 'when' condition: 'Rule is \"text\"' holds where the rule is bound to that text",
		["not"]       = "in a 'when' condition: 'Rule is not \"text\"'",
		["and"]       = "in a 'when' condition: both conditions hold",
		["or"]        = "in a 'when' condition: either condition holds",
		["switch"]    = "computed choice: 'switch @(selector) { case value: Expr; default: Expr }' reads the case the C# selector names",
		["case"]      = "a case of a computed choice: 'case value: Expr;'",
		["default"]   = "the case of a computed choice taken where no other case matches",
		["recover"]   = "a repetition that survives a bad element: 'X* recover Sync' skips to Sync and goes on; '=> @Factory(...)' builds the rejected element",
		["on"]        = "'on fail \"...\"' after a rule's type: what a refusal of the rule says",
		["fail"]      = "'on fail \"...\"' after a rule's type: what a refusal of the rule says",
		["public"]    = "a publication's method is public (the default)",
		["internal"]  = "a publication's method is internal: the assembly and its friends",
		["private"]   = "a publication's method is private to the class the grammar is attached to",
		["context"]   = "'context : @T' declares the object every publication is handed and the C# code reads as 'context'",
		["state"]     = "'state : @T' declares what holds while an operand is read; 'with state @(...)' sets it and 'parserState' reads it",
		["stream"]    = "a publication option: the method also reads a TextReader, or with 'bytes' a byte Stream",
		["bytes"]     = "a publication option after 'stream': the method reads bytes, from a Stream, a byte[] or a ReadOnlyMemory<byte>",
		["yield"]     = "a publication option: the method returns the elements of a sequence lazily, 'yield : @T' naming their type",
	};

	public static IReadOnlyDictionary<string, string> BuiltInDescriptions { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["any"]          = "one input item, whatever it is; fails only where there is none left",
		["none"]         = "zero input items: always succeeds and consumes nothing",
		["eol"]          = "one line ending: \"\\r\\n\", \"\\n\" or \"\\r\"",
		["eof"]          = "the end of the input: succeeds exactly where 'any' would fail, consuming nothing",
		["eos"]          = "the end of a segment; a grammar that names it reads a sequence of segments",
		["trivia"]       = "what may stand between the operands of a sequence; none until the grammar declares 'trivia'",
		["word"]         = "one whole word, whatever it says, as 'wordboundary' defines a word",
		["wordboundary"] = "what a word is made of; none until the grammar declares 'wordboundary'",
	};

	public static IReadOnlyDictionary<string, string> SuppliedDescriptions { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["parserText"]            = "the input the element covers, as a string",
		["parserPosition"]        = "where the element starts, as an absolute offset (long)",
		["parserOrdinal"]         = "which element of the repetition this is, counting rejected ones, from 0",
		["parserLine"]            = "the line the element starts on, from 1",
		["parserColumn"]          = "the column the element starts at, from 1",
		["parserSpan"]            = "the element's extent in the input",
		["parserMessage"]         = "why the element was rejected; in a recovery's factory only",
		["parserExpected"]        = "what would have fit where reading the element stopped; in a recovery's factory only",
		["parserFailurePosition"] = "where reading the rejected element stopped, as an absolute offset (long); in a recovery's factory only",
		["parserFailureLine"]     = "the line where reading the rejected element stopped, from 1; in a recovery's factory only",
		["parserFailureColumn"]   = "the column where reading the rejected element stopped, from 1; in a recovery's factory only",
		["parserInput"]           = "the whole of what was read, as a string",
		["parserState"]           = "the marks 'with state' placed over the construction, outermost first (ReadOnlySpan of the declared state type)",
		["parserMarks"]           = "for each mark in parserState, where in the input the reading under it began, outermost first (ReadOnlySpan<int>)",
	};

	public static IReadOnlyDictionary<TokenKind, string> SymbolDescriptions { get; } = new Dictionary<TokenKind, string>()
	{
		[TokenKind.Ampersand]         = "sequence: 'a & b'",
		[TokenKind.Bar]               = "ordered choice: 'a | b'; inside '[ ]' a union of elements",
		[TokenKind.OpenParen]         = "grouping, a rule's parameters or a call's arguments",
		[TokenKind.CloseParen]        = "grouping, a rule's parameters or a call's arguments",
		[TokenKind.OpenBracket]       = "element set: one input item, as in ['a'..'z' | '_']",
		[TokenKind.CloseBracket]      = "element set: one input item, as in ['a'..'z' | '_']",
		[TokenKind.OpenBrace]         = "an atomic group '{ ... }', a count 'X{2,4}', or a namespace's or computed choice's body",
		[TokenKind.CloseBrace]        = "an atomic group '{ ... }', a count 'X{2,4}', or a namespace's or computed choice's body",
		[TokenKind.Comma]             = "separates parameters, arguments, rebindings and the bounds of a count",
		[TokenKind.Semicolon]         = "ends a 'using', a case of a computed choice, or separates rules",
		[TokenKind.Colon]             = "a capture 'name: X', a binding '$name: X', or a type ': @T'",
		[TokenKind.Equals]            = "a rule's definition, or a rebinding 'A = B'",
		[TokenKind.Arrow]             = "construction: '=> value' builds the alternative's result",
		[TokenKind.Question]          = "optional: zero or one; after a rule's name, that the rule may give back within its body",
		[TokenKind.Star]              = "repetition: zero or more",
		[TokenKind.Plus]              = "repetition: one or more",
		[TokenKind.Minus]             = "a negative number, in a case of a computed choice",
		[TokenKind.Caret]             = "element set complement: [^ a | b] is any item except those",
		[TokenKind.Dot]               = "a qualified name: 'Namespace.Rule' or a C# member",
		[TokenKind.DotDot]            = "range: 'a'..'z'",
		[TokenKind.Less]              = "'<<' binds an alternative to the left at a strength; also a C# type's argument list",
		[TokenKind.Greater]           = "'>>' binds an alternative to the right at a strength; also a C# type's argument list",
		[TokenKind.At]                = "switches from the grammar to C#: '@Name', '@Name(args)', '@(expression)'",
		[TokenKind.Tilde]             = "sequence with no trivia between: 'a ~ b'",
		[TokenKind.PositiveLookahead] = "positive lookahead: '?=X' matches where X does, consuming nothing",
		[TokenKind.NegativeLookahead] = "negative lookahead: '?!X' matches where X does not, consuming nothing",
	};

	static readonly Lazy<IReadOnlyDictionary<string, StandardRule>> Standard = new(ReadStandardLibrary);

	/// <summary>
	/// The keywords that are keywords only where the parser reads them as one, and are coloured
	/// only there: a condition's words, <c>on fail</c>, and a publication's access.
	/// </summary>
	public static IReadOnlyCollection<string> ContextualKeywords { get; } = new HashSet<string>(StringComparer.Ordinal)
	{
		"is", "not", "and", "or", "on", "fail", "public", "internal", "private",
	};

	/// <summary>The words completion offers where a name goes: the built-in rules, and the keywords.</summary>
	public static IReadOnlyList<string> CompletionWords { get; } = [.. GrammarBinder.BuiltIn, .. Keywords.Keys];

	/// <summary>The names a <c>=&gt;</c>, a <c>when</c> and a recovery's factory are supplied with.</summary>
	public static IReadOnlyList<string> SuppliedNames { get; } = Grammar.Binding.SuppliedNames.All;

	/// <summary>The rules of the standard library, which a grammar reaches as <c>Std.Name</c>.</summary>
	public static IReadOnlyDictionary<string, StandardRule> StandardRules => Standard.Value;

	/// <summary>What Quick Info and completion say about a built-in rule, or null for another name.</summary>
	public static string? BuiltIn(string name)
	{
		return BuiltInDescriptions.TryGetValue(name, out var description)
			? $"DotGram built-in rule {name}: {description}"
			: null;
	}

	/// <summary>What Quick Info and completion say about a keyword, or null for another word.</summary>
	public static string? Keyword(string word)
	{
		return Keywords.TryGetValue(word, out var description)
			? $"DotGram keyword {word}: {description}"
			: null;
	}

	/// <summary>What Quick Info and completion say about a name the parser supplies, or null for another name.</summary>
	public static string? Supplied(string name)
	{
		return SuppliedDescriptions.TryGetValue(name, out var description)
			? $"DotGram supplied name {name}: {description}"
			: null;
	}

	/// <summary>What Quick Info says about a symbol of the notation, or null for anything else.</summary>
	public static string? Symbol(string text)
	{
		foreach (var pair in SymbolDescriptions)
			if (pair.Key.Spelling() == text)
				return $"DotGram '{text}': {pair.Value}";

		return null;
	}

	/// <summary>What Quick Info says about <c>\p{…}</c>, or null where it names no category.</summary>
	public static string? Category(string text)
	{
		if (!text.StartsWith("\\p{", StringComparison.Ordinal) || !text.EndsWith("}", StringComparison.Ordinal))
			return null;

		var abbreviation = text.Substring(3, text.Length - 4);

		if (!UnicodeCategories.Exists(abbreviation))
			return null;

		var names = UnicodeCategories.Expand(abbreviation).Select(static name => Spelled(name) + " " + name);

		return $"DotGram Unicode category {abbreviation} (Unicode {UnicodeCategories.UnicodeVersion}): {string.Join(", ", names)}";
	}

	static string Spelled(string name)
	{
		var category = (System.Globalization.UnicodeCategory)Enum.Parse(typeof(System.Globalization.UnicodeCategory), name);

		return UnicodeCategories.Abbreviation(category);
	}

	/// <summary>The text Quick Info shows for a span the analysis said nothing more about.</summary>
	public static string Describe(GramSyntaxKind kind, string text)
	{
		return kind switch
		{
			GramSyntaxKind.Keyword                  => BuiltIn(text) ?? Keyword(text) ?? $"DotGram keyword: {text}",
			GramSyntaxKind.Identifier               => Supplied(text) ?? StandardNamespace(text) ?? $"DotGram rule or binding: {text}",
			GramSyntaxKind.Number                   => "DotGram numeric literal",
			GramSyntaxKind.Character                => "DotGram character literal",
			GramSyntaxKind.String                   => "DotGram string literal",
			GramSyntaxKind.CaseInsensitiveCharacter => "DotGram case-insensitive character literal",
			GramSyntaxKind.CaseInsensitiveString    => "DotGram case-insensitive string literal",
			GramSyntaxKind.CharacterClass           => Category(text) ?? "DotGram character class",
			GramSyntaxKind.EmbeddedCode             => "Embedded C# expression",
			GramSyntaxKind.Transition               => "Switch from DotGram grammar to C#",
			GramSyntaxKind.SpecialSymbol            => Symbol(text) ?? $"DotGram special symbol: {text}",
			GramSyntaxKind.Comment                  => "DotGram comment",
			GramSyntaxKind.Invalid                  => "Unrecognized DotGram syntax",
			GramSyntaxKind.Operator                 => Symbol(text) ?? $"DotGram operator: {text}",
			GramSyntaxKind.Punctuation              => Symbol(text) ?? $"DotGram syntax: {text}",
			GramSyntaxKind.Binding                  => $"DotGram binding: {text}",
			_                                       => $"DotGram syntax: {text}",
		};
	}

	/// <summary>What completion says about a word it offers that no rule of the grammar declares.</summary>
	public static string DescribeWord(string word)
	{
		return BuiltIn(word) ?? Keyword(word) ?? Supplied(word) ?? $"DotGram keyword: {word}";
	}

	/// <summary>
	/// The standard library's rule a name written <c>Std.Name</c> ends in, where the text before
	/// <paramref name="nameStart"/> is <c>Std.</c>.
	/// </summary>
	public static StandardRule? QualifiedStandardRule(string text, int nameStart, int nameLength)
	{
		if (!IsStandardQualifier(text, nameStart) || nameLength <= 0 || nameStart + nameLength > text.Length)
			return null;

		return StandardRules.TryGetValue(text.Substring(nameStart, nameLength), out var rule) ? rule : null;
	}

	/// <summary>Whether <c>Std.</c> stands right before <paramref name="nameStart"/>, and not as the end of a longer name.</summary>
	public static bool IsStandardQualifier(string text, int nameStart)
	{
		var qualifier = StandardLibrary.Name + ".";
		var start     = nameStart - qualifier.Length;

		return start >= 0 &&
			string.CompareOrdinal(text, start, qualifier, 0, qualifier.Length) == 0 &&
			(start == 0 || !(char.IsLetterOrDigit(text[start - 1]) || text[start - 1] is '_' or '.'));
	}

	/// <summary>The supplied names a construction's <c>=&gt;</c> is handed, besides the two a <c>state</c> brings.</summary>
	/// <remarks>
	/// The rest are a rejected element's, and a construction that named one would be handed nothing:
	/// EditorVocabularyTests compiles every name in every place and holds these lists to what the
	/// generated C# accepts.
	/// </remarks>
	public static IReadOnlyCollection<string> SuppliedToConstructions { get; } = new HashSet<string>(StringComparer.Ordinal)
	{
		"parserText", "parserSpan", "parserInput",
	};

	/// <summary>The supplied names a <c>when</c> is handed.</summary>
	public static IReadOnlyCollection<string> SuppliedToGuards { get; } = new HashSet<string>(StringComparer.Ordinal)
	{
		"parserText", "parserSpan",
	};

	/// <summary>The supplied names only a grammar that declares <c>state : @T</c> hands over.</summary>
	public static IReadOnlyCollection<string> SuppliedWithState { get; } = new HashSet<string>(StringComparer.Ordinal)
	{
		"parserState", "parserMarks",
	};

	/// <summary>The supplied names a recovery's factory, <c>recover Sync =&gt; …</c>, is handed.</summary>
	public static IReadOnlyCollection<string> SuppliedToRecoveries { get; } =
		new HashSet<string>(Grammar.Binding.SuppliedNames.All.Except(["parserState", "parserMarks", "parserInput"]), StringComparer.Ordinal);

	/// <summary>
	/// The supplied names the C# at <paramref name="position"/> of a grammar may name: a
	/// construction's, a guard's or a recovery's, and the state's where the grammar declares one;
	/// none outside them.
	/// </summary>
	public static IReadOnlyList<string> SuppliedAt(string grammar, int position)
	{
		var action = ActionAt(grammar, position, out var state);

		if (action == GramAction.None)
			return [];

		var names = action switch
		{
			GramAction.Recovery => SuppliedToRecoveries,
			GramAction.Guard    => SuppliedToGuards,
			_                   => SuppliedToConstructions,
		};

		return [.. SuppliedNames.Where(name => names.Contains(name) || state && action == GramAction.Construction && SuppliedWithState.Contains(name))];
	}

	/// <summary>Which C# of the grammar <paramref name="position"/> stands in, and whether the grammar declares a <c>state</c>.</summary>
	public static GramAction ActionAt(string grammar, int position, out bool declaresState)
	{
		var file   = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance).File;
		var state  = false;
		var action = GramAction.None;

		void Walk(Expr expression)
		{
			switch (expression)
			{
				case Expr.Recovering { Factory: { } factory } when Inside(factory.At, position):
					action = GramAction.Recovery;
					return;

				case Expr.Construct construct when Inside(construct.Value.At, position):
					if (action == GramAction.None)
						action = GramAction.Construction;

					break;

				case Expr.Guard guard when Inside(guard.At, position):
					if (action == GramAction.None)
						action = GramAction.Guard;

					break;
			}

			foreach (var child in Dump.Children(expression))
				Walk(child);
		}

		void Declarations(IReadOnlyList<Decl> declarations)
		{
			foreach (var declaration in declarations)
				switch (declaration)
				{
					case Decl.State:
						state = true;
						break;

					case Decl.Namespace nested:
						Declarations(nested.Decls);
						break;

					case Decl.Rule rule:
						Walk(rule.Body);
						break;
				}
		}

		Declarations(file.Decls);
		declaresState = state;

		return action;
	}

	static bool Inside(Location at, int position)
	{
		return at.Position <= position && position <= at.Position + at.Length;
	}

	/// <summary>
	/// Whether the supplied names belong among what C# completion offers at <paramref name="position"/>:
	/// inside an <c>@(…)</c>, where a name stands on its own, and not after a dot or in an <c>@Name</c>.
	/// </summary>
	/// <param name="text">The text the caret is in.</param>
	/// <param name="position">The caret.</param>
	/// <param name="prefix">What C# completion was asked about, which ends at the caret.</param>
	public static bool SuppliedNamesApply(string text, int position, string prefix)
	{
		var start = position - prefix.Length;

		if (start > 0 && text[start - 1] == '@')
			return false;

		var word = prefix.Length;

		while (word > 0 && (char.IsLetterOrDigit(prefix[word - 1]) || prefix[word - 1] == '_'))
			word--;

		return word == 0 || prefix[word - 1] != '.';
	}

	/// <summary>
	/// Whether <paramref name="position"/> is among the arguments of an <c>@Name(…)</c>, which are
	/// grammar values and may name what the parser supplies, as in <c>@Hold(parserInput, parserSpan)</c>.
	/// </summary>
	public static bool InCallArguments(string text, int position)
	{
		var depth = 0;

		for (var at = Math.Min(position, text.Length) - 1; at >= 0; at--)
		{
			switch (text[at])
			{
				case ')':
					depth++;
					continue;

				case '(' when depth > 0:
					depth--;
					continue;

				case '(':
					var name = at;

					while (name > 0 && (char.IsLetterOrDigit(text[name - 1]) || text[name - 1] is '_' or '.'))
						name--;

					return name < at && name > 0 && text[name - 1] == '@';
			}
		}

		return false;
	}

	static string? StandardNamespace(string name)
	{
		return name == StandardLibrary.Name
			? "DotGram standard library: the rules every grammar reaches as Std.Name, or opens with 'using Std;'"
			: null;
	}

	static IReadOnlyDictionary<string, StandardRule> ReadStandardLibrary()
	{
		var text     = StandardLibrary.Text;
		var document = GramLanguageService.Analyze(text);
		var rules    = new Dictionary<string, StandardRule>(StringComparer.Ordinal);

		foreach (var item in document.Classifications)
		{
			if (item.SymbolKind != GramSymbolKind.Rule || item.DefinitionPosition != item.Position ||
				item.QuickInfo is null || item.RuleSignature is null)
				continue;

			var name = text.Substring(item.Position, item.Length);

			if (rules.ContainsKey(name))
				continue;

			var line      = 0;
			var lineStart = 0;

			for (var at = 0; at < item.Position; at++)
				if (text[at] == '\n')
				{
					line++;
					lineStart = at + 1;
				}

			rules.Add(name, new StandardRule(
				name,
				StandardLibrary.Name + "." + item.QuickInfo,
				StandardLibrary.Name + "." + item.RuleSignature,
				item.RuleParameterCount,
				line,
				item.Position - lineStart));
		}

		return rules;
	}
}

/// <summary>A rule of the standard library: what Quick Info, completion and signature help say about it, and where it is.</summary>
/// <param name="Name">The rule's name, without <c>Std.</c>.</param>
/// <param name="Description">Its definition, qualified, as Quick Info shows a rule.</param>
/// <param name="Signature">Its name and parameters, qualified.</param>
/// <param name="ParameterCount">How many parameters it takes.</param>
/// <param name="Line">The line it is declared on in the standard library, from 0.</param>
/// <param name="Column">The column its name starts at there, from 0.</param>
public readonly record struct StandardRule(string Name, string Description, string Signature, int ParameterCount, int Line, int Column);

/// <summary>The C# a position of a grammar stands in, which decides what the parser supplies to it.</summary>
public enum GramAction
{
	/// <summary>No construction, guard or recovery: nothing is supplied.</summary>
	None,

	/// <summary>A construction's <c>=&gt; …</c>: the rule's captures, and what the parser supplies to them.</summary>
	Construction,

	/// <summary>A <c>when</c>: the captures read so far, and the extent read so far.</summary>
	Guard,

	/// <summary>A recovery's factory, <c>recover Sync =&gt; …</c>: what the parser supplies about the rejected element, and no capture.</summary>
	Recovery,
}
