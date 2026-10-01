using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Language;

/// <summary>Editor-neutral syntax categories produced for a <c>.gram</c> document.</summary>
public enum GramSyntaxKind
{
	Invalid,
	Comment,
	Keyword,
	Identifier,
	Number,
	Character,
	String,
	CaseInsensitiveCharacter,
	CaseInsensitiveString,
	CharacterClass,
	EmbeddedCode,
	Transition,
	SpecialSymbol,
	Operator,
	Punctuation,
}

/// <summary>One classified source span in a <c>.gram</c> document.</summary>
public readonly struct GramClassifiedSpan(
	int position,
	int length,
	GramSyntaxKind kind,
	string? quickInfo = null,
	int? definitionPosition = null,
	string? ruleSignature = null,
	int ruleParameterCount = 0,
	GramSymbolKind? symbolKind = null)
{
	public int Position { get; } = position;
	public int Length { get; } = length;
	public GramSyntaxKind Kind { get; } = kind;
	public string? QuickInfo { get; } = quickInfo;
	public int? DefinitionPosition { get; } = definitionPosition;
	public string? RuleSignature { get; } = ruleSignature;
	public int RuleParameterCount { get; } = ruleParameterCount;
	public GramSymbolKind? SymbolKind { get; } = symbolKind;
}

public enum GramSymbolKind
{
	Rule,
	Parameter,
	Capture,
}

/// <summary>One declaration or reference to a grammar rule.</summary>
public readonly struct GramSymbolOccurrence(
	string name,
	int position,
	int length,
	int definitionPosition,
	bool isDefinition,
	GramSymbolKind kind = GramSymbolKind.Rule,
	int scopeStart = 0,
	int scopeEnd = int.MaxValue)
{
	public string Name { get; } = name;
	public int Position { get; } = position;
	public int Length { get; } = length;
	public int DefinitionPosition { get; } = definitionPosition;
	public bool IsDefinition { get; } = isDefinition;
	public GramSymbolKind Kind { get; } = kind;
	public int ScopeStart { get; } = scopeStart;
	public int ScopeEnd { get; } = scopeEnd;
}

public readonly record struct GramBracePair(int OpenPosition, int OpenLength, int ClosePosition, int CloseLength);

public readonly record struct GramFoldingRange(int Position, int Length, string CollapsedText);

/// <summary>An explicitly named generated C# method declared by a publication.</summary>
public readonly record struct GramPublishedApi(string MethodName, int Position, int Length);

public enum GramDocumentSymbolKind
{
	Namespace,
	Rule,
	Publication,
}

public sealed class GramDocumentSymbol(
	string name,
	GramDocumentSymbolKind kind,
	int position,
	int length,
	int selectionPosition,
	int selectionLength,
	IReadOnlyList<GramDocumentSymbol> children)
{
	public string Name { get; } = name;
	public GramDocumentSymbolKind Kind { get; } = kind;
	public int Position { get; } = position;
	public int Length { get; } = length;
	public int SelectionPosition { get; } = selectionPosition;
	public int SelectionLength { get; } = selectionLength;
	public IReadOnlyList<GramDocumentSymbol> Children { get; } = children;
}

/// <summary>The editor-neutral analysis of one immutable <c>.gram</c> document.</summary>
public sealed class GramDocument(
	IReadOnlyList<GramClassifiedSpan> classifications,
	IReadOnlyList<GramDiagnostic> diagnostics,
	IReadOnlyList<GramSymbolOccurrence> symbols,
	IReadOnlyList<GramBracePair> braces,
	IReadOnlyList<GramFoldingRange> foldingRanges,
	IReadOnlyList<GramDocumentSymbol> documentSymbols,
	IReadOnlyList<GramPublishedApi> publishedApis)
{
	public IReadOnlyList<GramClassifiedSpan> Classifications { get; } = classifications;
	public IReadOnlyList<GramDiagnostic> Diagnostics { get; } = diagnostics;
	public IReadOnlyList<GramSymbolOccurrence> Symbols { get; } = symbols;
	public IReadOnlyList<GramBracePair> Braces { get; } = braces;
	public IReadOnlyList<GramFoldingRange> FoldingRanges { get; } = foldingRanges;
	public IReadOnlyList<GramDocumentSymbol> DocumentSymbols { get; } = documentSymbols;
	public IReadOnlyList<GramPublishedApi> PublishedApis { get; } = publishedApis;
}

/// <summary>What a grammar's host says about compiling it, which the editor reads the same way.</summary>
public sealed class GramAnalysisOptions
{
	/// <summary>
	/// How much of the text is the document's own, where the rest was spliced on from what
	/// its host includes. Null when all of it is. A <c>parse</c> past this point belongs to
	/// the included grammar, which publishes it itself, as the generator has it.
	/// </summary>
	public int? Own { get; set; }

	/// <summary>Whether the host asks for the grammar to be read as tokens: <c>[Gram(Lexical = true)]</c>.</summary>
	public bool Lexical { get; set; }
}

/// <summary>
/// Adapts the existing compiler front-end to editor operations without reproducing
/// grammar recognition in an editor integration.
/// </summary>
public static class GramLanguageService
{
	/// <summary>The words a grammar document colours as keywords, and the built-in rules with them.</summary>
	/// <remarks>
	/// The built-in rules come from the binder that resolves them, so a rule added there is coloured
	/// here without anybody remembering to. The words are still a copy of what the parser takes;
	/// they are checked against it by hand today.
	/// </remarks>
	static readonly HashSet<string> Keywords = new(GrammarBinder.BuiltIn, StringComparer.Ordinal)
	{
		"using", "namespace", "parse", "find", "as", "when", "switch", "case", "default", "recover", "with",
		"context", "state", "stream", "bytes", "yield",
	};

	/// <summary>Analyzes a complete snapshot of a standalone <c>.gram</c> document.</summary>
	public static GramDocument Analyze(string text)
	{
		return Analyze(text, null);
	}

	/// <summary>
	/// Analyzes a complete snapshot of a <c>.gram</c> document compiled the way its host
	/// compiles it.
	/// </summary>
	/// <param name="text">The document, with whatever its host includes already spliced on after it.</param>
	/// <param name="options">What the host's attribute says, or null for a grammar with no host.</param>
	public static GramDocument Analyze(string text, GramAnalysisOptions? options)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		// Compiled first, because where a name resolves is the binder's to say: a rule named
		// in an included grammar, through a `using` or a qualified name, is found by the same
		// lookup the generator makes, and not by the first rule anywhere that has its name.
		GrammarModel? model = null;
		var compilation = GramCompiler.Compile(text, new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
			Own           = options?.Own,
			Lexical       = options?.Lexical ?? false,
			Bound         = bound => model = bound,
		});

		var tokens = GramLexer.Tokenize(text, RoslynCSharpScanner.Instance);
		var parsed = GramParser.Parse(tokens);
		var classifications = new List<GramClassifiedSpan>(tokens.Count);
		var (rules, rulesByPosition) = RuleDefinitions(text, parsed.File.Decls, tokens.Tokens);
		var resolved = model is null ? new Dictionary<int, int>() : ResolvedReferences(model, text.Length);
		var symbols = SymbolOccurrences(parsed.File.Decls, tokens.Tokens, rules, resolved);
		var symbolsByPosition = symbols.ToDictionary(static symbol => symbol.Position);
		var givesBackMarkers = GivesBackMarkers(parsed.File.Decls, tokens.Tokens);
		var contextualKeywords = ConditionKeywordPositions(parsed.File.Decls, tokens.Tokens);
		contextualKeywords.UnionWith(OnFailKeywordPositions(parsed.File.Decls, tokens.Tokens));
		contextualKeywords.UnionWith(PublicationAccessKeywordPositions(parsed.File.Decls, tokens.Tokens));

		foreach (var token in tokens.Tokens)
			if (TryClassify(token, contextualKeywords, out var kind))
			{
				if (symbolsByPosition.TryGetValue(token.Position, out var symbol) &&
					symbol.Kind != GramSymbolKind.Rule)
					classifications.Add(new GramClassifiedSpan(
						token.Position,
						token.Length,
						kind,
						symbol.Kind == GramSymbolKind.Parameter
							? $"{symbol.Name}: DotGram rule parameter"
							: $"{symbol.Name}: DotGram capture",
						symbol.DefinitionPosition,
						symbol.Name,
						symbolKind: symbol.Kind));
				else if (symbolsByPosition.TryGetValue(token.Position, out symbol) &&
					rulesByPosition.TryGetValue(symbol.DefinitionPosition, out var rule) ||
					token.Value is not null && rules.TryGetValue(token.Value, out rule))
					classifications.Add(new GramClassifiedSpan(
						token.Position,
						token.Length,
						kind,
						rule.ExpandedDefinition,
						rule.Position,
						rule.Signature,
						rule.ParameterCount,
						GramSymbolKind.Rule));
				else
					classifications.Add(new GramClassifiedSpan(
						token.Position,
						token.Length,
						kind,
						givesBackMarkers.Contains(token.Position)
							? "DotGram rule backtracking marker: this rule may give back within its body"
							: null));
			}

		ClassifyComments(text, tokens.Tokens, classifications);

		foreach (var classified in GramCSharpClassifier.Classify(text, parsed.File))
		{
			classifications.RemoveAll(existing => Intersects(existing, classified));
			classifications.Add(classified);
		}

		classifications.Sort(static (left, right) => left.Position.CompareTo(right.Position));

		var (braces, foldingRanges) = Structure(text, tokens.Tokens, rules, classifications);
		var documentSymbols = DocumentSymbols(parsed.File.Decls, tokens.Tokens);
		var publishedApis = PublishedApis(parsed.File.Decls, tokens.Tokens);

		return new GramDocument(
			classifications,
			NormalizeDiagnostics(compilation.Diagnostics, tokens.Tokens),
			symbols,
			braces,
			foldingRanges,
			documentSymbols,
			publishedApis);
	}

	static HashSet<int> ConditionKeywordPositions(
		IReadOnlyList<Decl> declarations,
		IReadOnlyList<Token> tokens)
	{
		var result = new HashSet<int>();
		Collect(declarations);
		return result;

		void Collect(IReadOnlyList<Decl> items)
		{
			foreach (var declaration in items)
				switch (declaration)
				{
					case Decl.Rule rule:
						Visit(rule.Body);
						break;
					case Decl.Namespace @namespace:
						Collect(@namespace.Decls);
						break;
				}
		}

		void Visit(Expr expression)
		{
			if (expression is Expr.Condition(var test))
			{
				var operands = Test.Operands(test).Concat(Test.Guards(test)).ToArray();

				for (var index = FirstTokenAtOrAfter(tokens, expression.At.Position);
					index < tokens.Count && tokens[index].Position < expression.At.End;
					index++)
				{
					var token = tokens[index];

					if (token.Kind == TokenKind.Identifier &&
						token.Value is "is" or "not" or "and" or "or" &&
						!operands.Any(operand =>
							token.Position >= operand.At.Position && token.Position < operand.At.End))
						result.Add(token.Position);
				}

				foreach (var operand in operands)
					Visit(operand);

				return;
			}

			foreach (var child in Dump.Children(expression))
				Visit(child);
		}
	}

	static int FirstTokenAtOrAfter(IReadOnlyList<Token> tokens, int position)
	{
		var low = 0;
		var high = tokens.Count;

		while (low < high)
		{
			var middle = low + (high - low) / 2;
			if (tokens[middle].Position < position)
				low = middle + 1;
			else
				high = middle;
		}

		return low;
	}

	static HashSet<int> OnFailKeywordPositions(
		IReadOnlyList<Decl> declarations,
		IReadOnlyList<Token> tokens)
	{
		var result = new HashSet<int>();
		Collect(declarations);
		return result;

		void Collect(IReadOnlyList<Decl> items)
		{
			foreach (var declaration in items)
				switch (declaration)
				{
					case Decl.Rule { OnFail: not null } rule:
						for (var index = 0; index + 2 < tokens.Count; index++)
							if (tokens[index].Position >= rule.At.Position &&
								tokens[index + 2].Position < rule.Body.At.Position &&
								tokens[index].Kind == TokenKind.Identifier &&
								tokens[index].Value == "on" &&
								tokens[index + 1].Kind == TokenKind.Identifier &&
								tokens[index + 1].Value == "fail" &&
								tokens[index + 2].Kind == TokenKind.String)
							{
								result.Add(tokens[index].Position);
								result.Add(tokens[index + 1].Position);
								break;
							}
						break;
					case Decl.Namespace @namespace:
						Collect(@namespace.Decls);
						break;
				}
		}
	}

	static HashSet<int> PublicationAccessKeywordPositions(
		IReadOnlyList<Decl> declarations,
		IReadOnlyList<Token> tokens)
	{
		var result = new HashSet<int>();
		Collect(declarations);
		return result;

		void Collect(IReadOnlyList<Decl> items)
		{
			foreach (var declaration in items)
				switch (declaration)
				{
					case Decl.Publish publication:
						var token = tokens.FirstOrDefault(candidate =>
							candidate.Position == publication.At.Position &&
							candidate.Kind == TokenKind.Identifier &&
							candidate.Value is "public" or "internal" or "private");
						if (token.Length > 0)
							result.Add(token.Position);
						break;
					case Decl.Namespace @namespace:
						Collect(@namespace.Decls);
						break;
				}
		}
	}

	static HashSet<int> GivesBackMarkers(
		IReadOnlyList<Decl> declarations,
		IReadOnlyList<Token> tokens)
	{
		var result = new HashSet<int>();
		Collect(declarations);
		return result;

		void Collect(IReadOnlyList<Decl> items)
		{
			foreach (var declaration in items)
				switch (declaration)
				{
					case Decl.Rule { GivesBack: true } rule:
						for (var index = 0; index + 1 < tokens.Count; index++)
							if (tokens[index].Kind == TokenKind.Identifier &&
								tokens[index].Value == rule.Name &&
								tokens[index].Position >= rule.At.Position &&
								tokens[index].Position < rule.At.End &&
								tokens[index + 1].Kind == TokenKind.Question)
							{
								result.Add(tokens[index + 1].Position);
								break;
							}
						break;
					case Decl.Namespace @namespace:
						Collect(@namespace.Decls);
						break;
				}
		}
	}

	static IReadOnlyList<GramPublishedApi> PublishedApis(
		IReadOnlyList<Decl> declarations,
		IReadOnlyList<Token> tokens)
	{
		var result = new List<GramPublishedApi>();

		foreach (var declaration in declarations)
			switch (declaration)
			{
				case Decl.Namespace @namespace:
					result.AddRange(PublishedApis(@namespace.Decls, tokens));
					break;

				case Decl.Publish { Alias: not null } publish:
					var alias = tokens.LastOrDefault(token =>
						token.Kind == TokenKind.Identifier &&
						token.Value == publish.Alias &&
						token.Position >= publish.At.Position &&
						token.Position < publish.At.End);
					if (alias.Length > 0)
						result.Add(new GramPublishedApi(publish.Alias, alias.Position, alias.Length));
					break;
			}

		return result;
	}

	static IReadOnlyList<GramDocumentSymbol> DocumentSymbols(
		IReadOnlyList<Decl> declarations,
		IReadOnlyList<Token> tokens)
	{
		var result = new List<GramDocumentSymbol>(declarations.Count);

		foreach (var declaration in declarations)
		{
			var end = tokens
				.Where(token => token.Position >= declaration.At.Position && token.Position < declaration.At.End)
				.Select(static token => token.Position + token.Length)
				.DefaultIfEmpty(declaration.At.End)
				.Max();

			switch (declaration)
			{
				case Decl.Rule rule:
					result.Add(new GramDocumentSymbol(
						rule.Name,
						GramDocumentSymbolKind.Rule,
						declaration.At.Position,
						end - declaration.At.Position,
						declaration.At.Position,
						rule.Name.Length,
						[]));
					break;

				case Decl.Namespace @namespace:
					var namespaceToken = tokens.FirstOrDefault(token =>
						token.Kind == TokenKind.Identifier &&
						token.Value == @namespace.Name &&
						token.Position >= declaration.At.Position &&
						token.Position < declaration.At.End);
					result.Add(new GramDocumentSymbol(
						@namespace.Name,
						GramDocumentSymbolKind.Namespace,
						declaration.At.Position,
						end - declaration.At.Position,
						namespaceToken.Position,
						namespaceToken.Length,
						DocumentSymbols(@namespace.Decls, tokens)));
					break;

				case Decl.Publish publish:
					var target = tokens.FirstOrDefault(token =>
						token.Kind == TokenKind.Identifier &&
						token.Value == publish.RuleName &&
						token.Position >= declaration.At.Position &&
						token.Position < declaration.At.End);
					result.Add(new GramDocumentSymbol(
						$"{publish.Kind.ToString().ToLowerInvariant()} {publish.RuleName}",
						GramDocumentSymbolKind.Publication,
						declaration.At.Position,
						end - declaration.At.Position,
						target.Position,
						target.Length,
						[]));
					break;
			}
		}

		return result;
	}

	static (IReadOnlyList<GramBracePair> Braces, IReadOnlyList<GramFoldingRange> FoldingRanges) Structure(
		string text,
		IReadOnlyList<Token> tokens,
		IReadOnlyDictionary<string, RuleInfo> rules,
		IReadOnlyList<GramClassifiedSpan> classifications)
	{
		var braces = new List<GramBracePair>();
		var parentheses = new Stack<Token>();
		var brackets    = new Stack<Token>();
		var blocks      = new Stack<Token>();

		foreach (var token in tokens)
			switch (token.Kind)
			{
				case TokenKind.OpenParen:   parentheses.Push(token); break;
				case TokenKind.OpenBracket: brackets.Push(token);    break;
				case TokenKind.OpenBrace:   blocks.Push(token);       break;
				case TokenKind.CloseParen:   Close(parentheses, token); break;
				case TokenKind.CloseBracket: Close(brackets, token);    break;
				case TokenKind.CloseBrace:   Close(blocks, token);       break;
			}

		var pairedPositions = new HashSet<int>(braces.SelectMany(static pair =>
			new[] { pair.OpenPosition, pair.ClosePosition }));
		var classifiedParentheses = new Stack<GramClassifiedSpan>();
		var classifiedBrackets    = new Stack<GramClassifiedSpan>();
		var classifiedBlocks      = new Stack<GramClassifiedSpan>();

		foreach (var span in classifications)
		{
			if (span.Kind != GramSyntaxKind.Punctuation ||
				span.Length != 1 ||
				pairedPositions.Contains(span.Position))
				continue;

			switch (text[span.Position])
			{
				case '(': classifiedParentheses.Push(span); break;
				case '[': classifiedBrackets.Push(span);    break;
				case '{': classifiedBlocks.Push(span);      break;
				case ')': CloseClassified(classifiedParentheses, span); break;
				case ']': CloseClassified(classifiedBrackets, span);    break;
				case '}': CloseClassified(classifiedBlocks, span);       break;
			}
		}

		braces.Sort(static (left, right) => left.OpenPosition.CompareTo(right.OpenPosition));

		var folding = new List<GramFoldingRange>();
		var starts = new HashSet<int>();

		foreach (var rule in rules.Values)
			AddFold(rule.Position, rule.Definition.Length, rule.Signature + " …");

		foreach (var pair in braces)
			AddFold(
				pair.OpenPosition,
				pair.ClosePosition + pair.CloseLength - pair.OpenPosition,
				text.Substring(pair.OpenPosition, pair.OpenLength) + "…" +
				text.Substring(pair.ClosePosition, pair.CloseLength));

		foreach (var comment in classifications)
			if (comment.Kind == GramSyntaxKind.Comment &&
				comment.Length >= 4 &&
				text.AsSpan(comment.Position, 2).SequenceEqual("/*".AsSpan()))
				AddFold(comment.Position, comment.Length, "/*…*/");

		folding.Sort(static (left, right) => left.Position.CompareTo(right.Position));
		return (braces, folding);

		void Close(Stack<Token> stack, Token close)
		{
			if (stack.Count == 0)
				return;

			var open = stack.Pop();
			braces.Add(new GramBracePair(open.Position, open.Length, close.Position, close.Length));
		}

		void CloseClassified(Stack<GramClassifiedSpan> stack, GramClassifiedSpan close)
		{
			if (stack.Count == 0)
				return;

			var open = stack.Pop();
			braces.Add(new GramBracePair(open.Position, open.Length, close.Position, close.Length));
		}

		void AddFold(int position, int length, string collapsedText)
		{
			if (length <= 0 || !starts.Add(position))
				return;

			var end = position + length;
			if (end > text.Length || text.IndexOf('\n', position, length) < 0)
				return;

			folding.Add(new GramFoldingRange(position, length, collapsedText));
		}
	}

	static IReadOnlyList<GramDiagnostic> NormalizeDiagnostics(
		IReadOnlyList<GramDiagnostic> diagnostics,
		IReadOnlyList<Token> tokens)
	{
		return diagnostics.Select(diagnostic =>
		{
			if (diagnostic.Id != "GRAM3002")
				return diagnostic;

			var token = tokens.FirstOrDefault(candidate =>
				candidate.Position == diagnostic.Position && candidate.Kind == TokenKind.Identifier);

			return token.Length > 0 && token.Length < diagnostic.Length
				? diagnostic with { Length = token.Length }
				: diagnostic;
		}).ToArray();
	}

	/// <summary>
	/// Where the binder sent each rule reference: the position a name starts at, to the
	/// position of the rule it names.
	/// </summary>
	/// <remarks>
	/// Only rules declared inside <paramref name="length"/>: the standard library is spliced
	/// on past the end of the text, and a definition there is a place nobody can be taken to.
	/// </remarks>
	static Dictionary<int, int> ResolvedReferences(GrammarModel model, int length)
	{
		var result = new Dictionary<int, int>();

		foreach (var binding in model.Bindings)
		{
			if (binding.Value is not RuleSymbol { Declaration: { } declaration } ||
				declaration.At.Position >= length)
				continue;

			var reference = binding.Key switch
			{
				Expr.Reference direct => direct,
				Expr.Call call        => call.Target,
				_                     => null,
			};

			if (reference is not null)
				result[reference.At.Position] = declaration.At.Position;
		}

		return result;
	}

	static IReadOnlyList<GramSymbolOccurrence> SymbolOccurrences(
		IReadOnlyList<Decl> declarations,
		IReadOnlyList<Token> tokens,
		IReadOnlyDictionary<string, RuleInfo> rules,
		IReadOnlyDictionary<int, int> resolved)
	{
		var result = new List<GramSymbolOccurrence>();
		var positions = new HashSet<int>();
		Dictionary<string, int>? parameters = null;
		Dictionary<string, int>? captures   = null;
		Location? localScope = null;

		VisitDeclarations(declarations);
		result.Sort(static (left, right) => left.Position.CompareTo(right.Position));

		return result;

		void AddOccurrence(
			string name,
			Location at,
			int definitionPosition,
			bool isDefinition,
			GramSymbolKind kind)
		{
			if (!positions.Add(at.Position))
				return;

			result.Add(new GramSymbolOccurrence(
				name,
				at.Position,
				name.Length,
				definitionPosition,
				isDefinition,
				kind,
				kind == GramSymbolKind.Rule ? 0 : localScope!.Value.Position,
				kind == GramSymbolKind.Rule ? int.MaxValue : localScope!.Value.End));
		}

		void AddRule(string name, Location at, bool isDefinition)
		{
			// A declaration is its own definition: two grammars spliced together may each
			// declare a rule of one name, and neither is the other's.
			if (isDefinition)
			{
				AddOccurrence(name, at, at.Position, true, GramSymbolKind.Rule);
				return;
			}

			// Where the binder sent it, and the name is the last part of what is written:
			// `Sql92.Lexical.Digits` is a use of `Digits`, and that word is what is underlined,
			// renamed and found.
			if (resolved.TryGetValue(at.Position, out var definition))
			{
				var simple = name.Substring(name.LastIndexOf('.') + 1);
				var last   = (Token?)null;

				for (var index = FirstTokenAtOrAfter(tokens, at.Position);
					index < tokens.Count && tokens[index].Position < at.End;
					index++)
					if (tokens[index].Kind == TokenKind.Identifier && tokens[index].Value == simple)
						last = tokens[index];

				if (last is { } token)
					AddOccurrence(simple, new Location(token.Position, token.Length), definition, false, GramSymbolKind.Rule);

				return;
			}

			if (!rules.TryGetValue(name, out var rule))
				return;

			AddOccurrence(name, at, rule.Position, isDefinition, GramSymbolKind.Rule);
		}

		void VisitDeclarations(IReadOnlyList<Decl> items)
		{
			foreach (var declaration in items)
				switch (declaration)
				{
					case Decl.Rule rule:
						AddRule(rule.Name, rule.At, true);
						VisitRule(rule);
						break;
					case Decl.Namespace @namespace:
						VisitDeclarations(@namespace.Decls);
						break;
					case Decl.Publish publish:
						var token = tokens.FirstOrDefault(candidate =>
							candidate.Position >= publish.At.Position &&
							candidate.Position < publish.At.End &&
							candidate.Value == publish.RuleName);
						if (token.Length > 0)
							AddRule(publish.RuleName, new Location(token.Position, token.Length), false);
						foreach (var rebinding in publish.Rebindings) AddRebinding(rebinding);
						break;
				}
		}

		void VisitRule(Decl.Rule rule)
		{
			parameters = new Dictionary<string, int>(StringComparer.Ordinal);
			captures   = new Dictionary<string, int>(StringComparer.Ordinal);
			localScope = rule.At;

			foreach (var parameter in rule.Params)
			{
				if (!parameters.TryGetValue(parameter.Name, out var definition))
					parameters.Add(parameter.Name, definition = parameter.At.Position);
				AddOccurrence(
					parameter.Name,
					new Location(parameter.At.Position, parameter.Name.Length),
					definition,
					true,
					GramSymbolKind.Parameter);
				if (parameter.Type is not null) VisitType(parameter.Type);
			}

			if (rule.Type is not null) VisitType(rule.Type);
			CollectCaptures(rule.Body);
			Visit(rule.Body);

			parameters = null;
			captures   = null;
			localScope = null;
		}

		/// <summary>Every capture of one scope: a rule's, or a constructing group's.</summary>
		void CollectCaptures(Expr expression)
		{
			// A constructing group owns its captures (syntax.md 3.7): its names do not escape,
			// and its factory cannot reach what stands outside. They are collected where the
			// group is visited; descending into one here would hoist them into the rule, and a
			// second group capturing the same name would then read as another use of the first --
			// one symbol where there are two, so Rename would rename inside both groups.
			if (expression is Expr.Group nested && Constructs(nested.Body))
				return;

			if (expression is Expr.Capture capture)
			{
				if (!captures!.TryGetValue(capture.Name, out var definition))
					captures.Add(capture.Name, definition = capture.At.Position);
				AddOccurrence(
					capture.Name,
					new Location(capture.At.Position, capture.Name.Length),
					definition,
					true,
					GramSymbolKind.Capture);
			}

			foreach (var child in Dump.Children(expression))
				CollectCaptures(child);
		}

		/// <summary>Whether a group constructs, which is what gives it a scope of its own.</summary>
		/// <remarks>
		/// Through its alternatives, because `( a => @(x) | b => @(y) )` is one group and each
		/// alternative constructs. A plain group keeps the capture scope it always had.
		/// </remarks>
		static bool Constructs(Expr body)
		{
			return body switch
			{
				Expr.Construct => true,
				Expr.Choice(var alternatives) => alternatives.Any(Constructs),
				_ => false,
			};
		}

		void VisitType(TypeRef type)
		{
			if (!type.IsCSharp)
				AddReference(type.Name, new Location(type.At.Position, type.Name.Length));
		}

		void AddReference(string name, Location at)
		{
			if (parameters is not null && parameters.TryGetValue(name, out var parameter))
				AddOccurrence(name, at, parameter, false, GramSymbolKind.Parameter);
			else if (captures is not null && captures.TryGetValue(name, out var capture))
				AddOccurrence(name, at, capture, false, GramSymbolKind.Capture);
			else
				AddRule(name, at, false);
		}

		void AddExpressionReference(Expr.Reference reference)
		{
			if (!reference.IsCSharp)
				AddReference(reference.Name, reference.At);
		}

		void AddCSharpReferences(Expr.CSharp expression)
		{
			var syntax = SyntaxFactory.ParseExpression(expression.Text);
			var offset = expression.At.Position + 2;

			foreach (var identifier in syntax.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>())
			{
				if (identifier.Parent is MemberAccessExpressionSyntax member && member.Name == identifier)
					continue;

				var name = identifier.Identifier.ValueText;
				var at = new Location(offset + identifier.SpanStart, identifier.Span.Length);
				if (parameters is not null && parameters.TryGetValue(name, out var parameter))
					AddOccurrence(name, at, parameter, false, GramSymbolKind.Parameter);
				else if (captures is not null && captures.TryGetValue(name, out var capture))
					AddOccurrence(name, at, capture, false, GramSymbolKind.Capture);
			}
		}

		void AddRebinding(Rebinding rebinding)
		{
			var names = tokens.Where(token =>
				token.Kind == TokenKind.Identifier &&
				token.Position >= rebinding.At.Position &&
				token.Position < rebinding.At.End);

			foreach (var token in names)
				if (token.Value == rebinding.Left || token.Value == rebinding.Right)
					AddRule(token.Value, new Location(token.Position, token.Length), false);
		}

		void Visit(Expr item)
		{
			switch (item)
			{
				case Expr.CSharp csharp:
					AddCSharpReferences(csharp);
					break;
				case Expr.Switch selected:
					Visit(selected.Value);
					foreach (var branch in selected.Cases) Visit(branch.Body);
					break;
				case Expr.Choice choice:
					foreach (var alternative in choice.Alternatives) Visit(alternative);
					break;
				case Expr.Sequence sequence:
					foreach (var operand in sequence.Operands) Visit(operand);
					break;
				case Expr.Glued glued:
					foreach (var operand in glued.Operands) Visit(operand);
					break;
				case Expr.Construct construct:
					Visit(construct.Pattern);
					Visit(construct.Value);
					break;
				case Expr.Bound bound:
					Visit(bound.Body);
					break;
				case Expr.Recovering recovering:
					Visit(recovering.Body);
					Visit(recovering.Sync);
					if (recovering.Factory is not null) Visit(recovering.Factory);
					break;
				case Expr.Guard guard:
					Visit(guard.Value);
					break;
				case Expr.Condition condition:
					foreach (var operand in Test.Operands(condition.Test)) Visit(operand);
					foreach (var guard in Test.Guards(condition.Test)) Visit(guard);
					break;
				case Expr.Capture capture:
					Visit(capture.Operand);
					break;
				case Expr.Group group:
					if (Constructs(group.Body))
					{
						var outer = captures;

						captures = new Dictionary<string, int>(StringComparer.Ordinal);
						CollectCaptures(group.Body);
						Visit(group.Body);

						captures = outer;
					}
					else
						Visit(group.Body);
					break;
				case Expr.Atomic atomic:
					Visit(atomic.Body);
					break;
				case Expr.Lookahead lookahead:
					Visit(lookahead.Operand);
					break;
				case Expr.ElementSet set:
					foreach (var element in set.Items)
						if (element is Elem.Ref reference) AddExpressionReference(reference.Reference);
					break;
				case Expr.Reference reference:
					AddExpressionReference(reference);
					break;
				case Expr.Call call:
					AddExpressionReference(call.Target);
					foreach (var argument in call.Arguments) Visit(argument);
					break;
				case Expr.Quantified quantified:
					Visit(quantified.Operand);
					AddCount(quantified.MinName, quantified.At);
					AddCount(quantified.MaxName, quantified.At);
					break;
				case Expr.With with:
					Visit(with.Operand);
					foreach (var rebinding in with.Rebindings) AddRebinding(rebinding);
					break;
				case Expr.Marked marked:
					Visit(marked.Operand);
					Visit(marked.Value);
					break;
			}
		}

		void AddCount(string? name, Location within)
		{
			if (name is null)
				return;

			var token = tokens.FirstOrDefault(candidate =>
				candidate.Kind == TokenKind.Identifier &&
				candidate.Value == name &&
				candidate.Position >= within.Position &&
				candidate.Position < within.End &&
				!positions.Contains(candidate.Position));

			if (token.Length > 0)
				AddReference(name, new Location(token.Position, token.Length));
		}
	}

	/// <summary>Every rule: by name, the first of each, and by where it is declared, all of them.</summary>
	/// <remarks>
	/// By position because a grammar and what it includes may each declare one name; a use
	/// the binder resolved is told about the rule it resolved to, and only a name the binder
	/// did not see falls back on the first.
	/// </remarks>
	static (Dictionary<string, RuleInfo> ByName, Dictionary<int, RuleInfo> ByPosition) RuleDefinitions(
		string text,
		IReadOnlyList<Decl> declarations,
		IReadOnlyList<Token> tokens)
	{
		var result     = new Dictionary<string, RuleInfo>(StringComparer.Ordinal);
		var byPosition = new Dictionary<int, RuleInfo>();

		Collect(declarations);

		foreach (var rule in byPosition.Values)
			rule.ExpandedDefinition = Expand(rule, result);

		return (result, byPosition);

		void Collect(IReadOnlyList<Decl> items)
		{
			foreach (var declaration in items)
				switch (declaration)
				{
					case Decl.Rule rule:
						var end = tokens
							.Where(token => token.Position >= rule.At.Position && token.Position < rule.At.End)
							.Select(static token => token.Position + token.Length)
							.DefaultIfEmpty(rule.At.Position)
							.Max();
						var length = Math.Min(end - rule.At.Position, text.Length - rule.At.Position);

						if (length > 0 && !byPosition.ContainsKey(rule.At.Position))
						{
							var info = new RuleInfo(
								rule.Name,
								text.Substring(rule.At.Position, length).TrimEnd(),
								rule.At.Position,
								References(rule.Body),
								rule.Params.Count);

							byPosition.Add(rule.At.Position, info);

							if (!result.ContainsKey(rule.Name))
								result.Add(rule.Name, info);
						}

						break;
					case Decl.Namespace @namespace:
						Collect(@namespace.Decls);
						break;
				}
		}
	}

	static string Expand(RuleInfo start, IReadOnlyDictionary<string, RuleInfo> rules)
	{
		var text    = new StringBuilder(start.Definition);
		var emitted = new HashSet<string>(StringComparer.Ordinal) { start.Name };
		var stack   = new HashSet<string>(StringComparer.Ordinal) { start.Name };

		AppendDependencies(start);

		return text.ToString();

		void AppendDependencies(RuleInfo rule)
		{
			foreach (var reference in rule.References)
			{
				if (!rules.TryGetValue(reference, out var dependency))
					continue;

				if (stack.Contains(reference))
				{
					text.Append("\n\nRecursive reference: ").Append(reference);
					continue;
				}

				if (!emitted.Add(reference))
					continue;

				text.Append("\n\nReferenced rule:\n").Append(dependency.Definition);
				stack.Add(reference);
				AppendDependencies(dependency);
				stack.Remove(reference);
			}
		}
	}

	static IReadOnlyList<string> References(Expr expression)
	{
		var result = new List<string>();

		Visit(expression);

		return result;

		void Add(Expr.Reference reference)
		{
			if (!reference.IsCSharp && !result.Contains(reference.Name))
				result.Add(reference.Name);
		}

		void AddName(string name)
		{
			if (!result.Contains(name))
				result.Add(name);
		}

		void Visit(Expr item)
		{
			switch (item)
			{
				case Expr.Switch selected:
					Visit(selected.Value);
					foreach (var branch in selected.Cases) Visit(branch.Body);
					break;
				case Expr.Choice choice:
					foreach (var alternative in choice.Alternatives) Visit(alternative);
					break;
				case Expr.Sequence sequence:
					foreach (var operand in sequence.Operands) Visit(operand);
					break;
				case Expr.Glued glued:
					foreach (var operand in glued.Operands) Visit(operand);
					break;
				case Expr.Construct construct:
					Visit(construct.Pattern);
					Visit(construct.Value);
					break;
				case Expr.Bound bound:
					Visit(bound.Body);
					break;
				case Expr.Recovering recovering:
					Visit(recovering.Body);
					Visit(recovering.Sync);
					if (recovering.Factory is not null) Visit(recovering.Factory);
					break;
				case Expr.Guard guard:
					Visit(guard.Value);
					break;
				case Expr.Condition condition:
					foreach (var operand in Test.Operands(condition.Test)) Visit(operand);
					break;
				case Expr.Capture capture:
					Visit(capture.Operand);
					break;
				case Expr.Group group:
					Visit(group.Body);
					break;
				case Expr.Atomic atomic:
					Visit(atomic.Body);
					break;
				case Expr.Lookahead lookahead:
					Visit(lookahead.Operand);
					break;
				case Expr.ElementSet set:
					foreach (var element in set.Items)
						if (element is Elem.Ref reference) Add(reference.Reference);
					break;
				case Expr.Reference reference:
					Add(reference);
					break;
				case Expr.Call call:
					Add(call.Target);
					foreach (var argument in call.Arguments) Visit(argument);
					break;
				case Expr.Quantified quantified:
					Visit(quantified.Operand);
					break;
				case Expr.With with:
					Visit(with.Operand);
					foreach (var rebinding in with.Rebindings)
					{
						AddName(rebinding.Left);
						AddName(rebinding.Right);
					}
					break;
				case Expr.Marked marked:
					Visit(marked.Operand);
					Visit(marked.Value);
					break;
			}
		}
	}

	sealed class RuleInfo(
		string name,
		string definition,
		int position,
		IReadOnlyList<string> references,
		int parameterCount)
	{
		public string Name { get; } = name;
		public string Definition { get; } = definition;
		public int Position { get; } = position;
		public IReadOnlyList<string> References { get; } = references;
		public string ExpandedDefinition { get; set; } = definition;
		public string Signature { get; } = SignatureOf(definition);
		public int ParameterCount { get; } = parameterCount;

		static string SignatureOf(string text)
		{
			var equals = text.IndexOf('=');

			return equals < 0 ? text : text.Substring(0, equals).TrimEnd();
		}
	}

	static bool Intersects(GramClassifiedSpan left, GramClassifiedSpan right)
	{
		return left.Position < right.Position + right.Length && right.Position < left.Position + left.Length;
	}

	static void ClassifyComments(
		string text,
		IReadOnlyList<Token> tokens,
		List<GramClassifiedSpan> classifications)
	{
		var previous = 0;

		foreach (var token in tokens)
		{
			ClassifyComments(text, previous, token.Position, classifications);
			previous = token.Position + token.Length;
		}
	}

	static void ClassifyComments(
		string text,
		int start,
		int end,
		List<GramClassifiedSpan> classifications)
	{
		var position = start;

		while (position + 1 < end)
		{
			if (text[position] != '/')
			{
				position++;
				continue;
			}

			var comment = position;

			if (text[position + 1] == '/')
			{
				position += 2;

				while (position < end && text[position] is not ('\r' or '\n'))
					position++;
			}
			else if (text[position + 1] == '*')
			{
				position += 2;

				while (position + 1 < end && !(text[position] == '*' && text[position + 1] == '/'))
					position++;

				position = position + 1 < end ? position + 2 : end;
			}
			else
			{
				position++;
				continue;
			}

			classifications.Add(new GramClassifiedSpan(comment, position - comment, GramSyntaxKind.Comment));
		}
	}

	static bool TryClassify(Token token, HashSet<int> conditionKeywords, out GramSyntaxKind kind)
	{
		kind = token.Kind switch
		{
			TokenKind.Unknown => GramSyntaxKind.Invalid,
			TokenKind.Identifier when conditionKeywords.Contains(token.Position) => GramSyntaxKind.Keyword,
			TokenKind.Identifier when Keywords.Contains(token.Value!) => GramSyntaxKind.Keyword,
			TokenKind.Identifier => GramSyntaxKind.Identifier,
			TokenKind.Integer => GramSyntaxKind.Number,
			TokenKind.Character => GramSyntaxKind.Character,
			TokenKind.String => GramSyntaxKind.String,
			TokenKind.CaseInsensitiveCharacter => GramSyntaxKind.CaseInsensitiveCharacter,
			TokenKind.CaseInsensitiveString => GramSyntaxKind.CaseInsensitiveString,
			TokenKind.UnicodeCategory => GramSyntaxKind.CharacterClass,
			TokenKind.CSharpExpression => GramSyntaxKind.EmbeddedCode,
			TokenKind.OpenParen or TokenKind.CloseParen => GramSyntaxKind.Punctuation,
			TokenKind.OpenBracket or TokenKind.CloseBracket => GramSyntaxKind.Punctuation,
			TokenKind.OpenBrace or TokenKind.CloseBrace => GramSyntaxKind.Punctuation,
			TokenKind.Comma or TokenKind.Semicolon => GramSyntaxKind.Punctuation,
			TokenKind.At => GramSyntaxKind.Transition,
			TokenKind.Ampersand or TokenKind.Tilde or TokenKind.Bar => GramSyntaxKind.SpecialSymbol,
			TokenKind.Question or TokenKind.Star or TokenKind.Plus or TokenKind.Caret => GramSyntaxKind.SpecialSymbol,
			TokenKind.DotDot or TokenKind.Less or TokenKind.Greater => GramSyntaxKind.SpecialSymbol,
			TokenKind.PositiveLookahead or TokenKind.NegativeLookahead => GramSyntaxKind.SpecialSymbol,
			TokenKind.Colon or TokenKind.Dot => GramSyntaxKind.Punctuation,
			TokenKind.EndOfFile => default,
			_ => GramSyntaxKind.Operator,
		};

		return token.Kind != TokenKind.EndOfFile;
	}
}
