using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Parsing;

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

/// <summary>The editor-neutral analysis of one immutable <c>.gram</c> document.</summary>
public sealed class GramDocument(
	IReadOnlyList<GramClassifiedSpan> classifications,
	IReadOnlyList<GramDiagnostic> diagnostics,
	IReadOnlyList<GramSymbolOccurrence> symbols,
	IReadOnlyList<GramBracePair> braces,
	IReadOnlyList<GramFoldingRange> foldingRanges)
{
	public IReadOnlyList<GramClassifiedSpan> Classifications { get; } = classifications;
	public IReadOnlyList<GramDiagnostic> Diagnostics { get; } = diagnostics;
	public IReadOnlyList<GramSymbolOccurrence> Symbols { get; } = symbols;
	public IReadOnlyList<GramBracePair> Braces { get; } = braces;
	public IReadOnlyList<GramFoldingRange> FoldingRanges { get; } = foldingRanges;
}

/// <summary>
/// Adapts the existing compiler front-end to editor operations without reproducing
/// grammar recognition in an editor integration.
/// </summary>
public static class GramLanguageService
{
	static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
	{
		"using", "namespace", "parse", "find", "as", "when", "recover", "with",
		"any", "none", "eol", "eof", "trivia", "KeywordBoundary",
	};

	/// <summary>Analyzes a complete snapshot of a standalone <c>.gram</c> document.</summary>
	public static GramDocument Analyze(string text)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var tokens = GramLexer.Tokenize(text, RoslynCSharpScanner.Instance);
		var parsed = GramParser.Parse(tokens);
		var classifications = new List<GramClassifiedSpan>(tokens.Count);
		var rules = RuleDefinitions(text, parsed.File.Decls, tokens.Tokens);
		var symbols = SymbolOccurrences(parsed.File.Decls, tokens.Tokens, rules);
		var symbolsByPosition = symbols.ToDictionary(static symbol => symbol.Position);

		foreach (var token in tokens.Tokens)
			if (TryClassify(token, out var kind))
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
				else if (token.Value is not null && rules.TryGetValue(token.Value, out var rule))
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
						kind));
			}

		ClassifyComments(text, tokens.Tokens, classifications);

		foreach (var classified in GramCSharpClassifier.Classify(text, parsed.File))
		{
			classifications.RemoveAll(existing => Intersects(existing, classified));
			classifications.Add(classified);
		}

		classifications.Sort(static (left, right) => left.Position.CompareTo(right.Position));

		var compilation = GramCompiler.Compile(text, new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		var (braces, foldingRanges) = Structure(text, tokens.Tokens, rules, classifications);

		return new GramDocument(
			classifications,
			NormalizeDiagnostics(compilation.Diagnostics, tokens.Tokens),
			symbols,
			braces,
			foldingRanges);
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
		IReadOnlyList<Token> tokens) =>
		diagnostics.Select(diagnostic =>
		{
			if (diagnostic.Id != "GRAM3002")
				return diagnostic;

			var token = tokens.FirstOrDefault(candidate =>
				candidate.Position == diagnostic.Position && candidate.Kind == TokenKind.Identifier);

			return token.Length > 0 && token.Length < diagnostic.Length
				? diagnostic with { Length = token.Length }
				: diagnostic;
		}).ToArray();

	static IReadOnlyList<GramSymbolOccurrence> SymbolOccurrences(
		IReadOnlyList<Decl> declarations,
		IReadOnlyList<Token> tokens,
		IReadOnlyDictionary<string, RuleInfo> rules)
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

		void CollectCaptures(Expr expression)
		{
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
				case Expr.Choice choice:
					foreach (var alternative in choice.Alternatives) Visit(alternative);
					break;
				case Expr.Sequence sequence:
					foreach (var operand in sequence.Operands) Visit(operand);
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

	static Dictionary<string, RuleInfo> RuleDefinitions(
		string text,
		IReadOnlyList<Decl> declarations,
		IReadOnlyList<Token> tokens)
	{
		var result = new Dictionary<string, RuleInfo>(StringComparer.Ordinal);

		Collect(declarations);

		foreach (var pair in result)
			pair.Value.ExpandedDefinition = Expand(pair.Key, result);

		return result;

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

						if (length > 0 && !result.ContainsKey(rule.Name))
							result.Add(rule.Name, new RuleInfo(
								text.Substring(rule.At.Position, length).TrimEnd(),
								rule.At.Position,
								References(rule.Body),
								rule.Params.Count));

						break;
					case Decl.Namespace @namespace:
						Collect(@namespace.Decls);
						break;
				}
		}
	}

	static string Expand(string name, IReadOnlyDictionary<string, RuleInfo> rules)
	{
		var text    = new StringBuilder(rules[name].Definition);
		var emitted = new HashSet<string>(StringComparer.Ordinal) { name };
		var stack   = new HashSet<string>(StringComparer.Ordinal) { name };

		AppendDependencies(rules[name]);

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
				case Expr.Choice choice:
					foreach (var alternative in choice.Alternatives) Visit(alternative);
					break;
				case Expr.Sequence sequence:
					foreach (var operand in sequence.Operands) Visit(operand);
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
			}
		}
	}

	sealed class RuleInfo(
		string definition,
		int position,
		IReadOnlyList<string> references,
		int parameterCount)
	{
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

	static bool Intersects(GramClassifiedSpan left, GramClassifiedSpan right) =>
		left.Position < right.Position + right.Length && right.Position < left.Position + left.Length;

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

	static bool TryClassify(Token token, out GramSyntaxKind kind)
	{
		kind = token.Kind switch
		{
			TokenKind.Unknown => GramSyntaxKind.Invalid,
			TokenKind.Identifier when Keywords.Contains(token.Value!) => GramSyntaxKind.Keyword,
			TokenKind.Identifier => GramSyntaxKind.Identifier,
			TokenKind.Integer => GramSyntaxKind.Number,
			TokenKind.Character => GramSyntaxKind.Character,
			TokenKind.String => GramSyntaxKind.String,
			TokenKind.UnicodeCategory => GramSyntaxKind.CharacterClass,
			TokenKind.CSharpExpression => GramSyntaxKind.EmbeddedCode,
			TokenKind.OpenParen or TokenKind.CloseParen => GramSyntaxKind.Punctuation,
			TokenKind.OpenBracket or TokenKind.CloseBracket => GramSyntaxKind.Punctuation,
			TokenKind.OpenBrace or TokenKind.CloseBrace => GramSyntaxKind.Punctuation,
			TokenKind.Comma or TokenKind.Semicolon => GramSyntaxKind.Punctuation,
			TokenKind.At => GramSyntaxKind.Transition,
			TokenKind.Ampersand or TokenKind.Bar => GramSyntaxKind.SpecialSymbol,
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
