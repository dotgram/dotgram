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
	int ruleParameterCount = 0)
{
	public int Position { get; } = position;
	public int Length { get; } = length;
	public GramSyntaxKind Kind { get; } = kind;
	public string? QuickInfo { get; } = quickInfo;
	public int? DefinitionPosition { get; } = definitionPosition;
	public string? RuleSignature { get; } = ruleSignature;
	public int RuleParameterCount { get; } = ruleParameterCount;
}

/// <summary>One declaration or reference to a grammar rule.</summary>
public readonly struct GramSymbolOccurrence(
	string name,
	int position,
	int length,
	int definitionPosition,
	bool isDefinition)
{
	public string Name { get; } = name;
	public int Position { get; } = position;
	public int Length { get; } = length;
	public int DefinitionPosition { get; } = definitionPosition;
	public bool IsDefinition { get; } = isDefinition;
}

/// <summary>The editor-neutral analysis of one immutable <c>.gram</c> document.</summary>
public sealed class GramDocument(
	IReadOnlyList<GramClassifiedSpan> classifications,
	IReadOnlyList<GramDiagnostic> diagnostics,
	IReadOnlyList<GramSymbolOccurrence> symbols)
{
	public IReadOnlyList<GramClassifiedSpan> Classifications { get; } = classifications;
	public IReadOnlyList<GramDiagnostic> Diagnostics { get; } = diagnostics;
	public IReadOnlyList<GramSymbolOccurrence> Symbols { get; } = symbols;
}

/// <summary>
/// Adapts the existing compiler front-end to editor operations without reproducing
/// grammar recognition in an editor integration.
/// </summary>
public static class GramLanguageService
{
	static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
	{
		"using", "context", "parse", "find", "as", "when", "recover",
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

		foreach (var token in tokens.Tokens)
			if (TryClassify(token, out var kind))
			{
				if (token.Value is not null && rules.TryGetValue(token.Value, out var rule))
					classifications.Add(new GramClassifiedSpan(
						token.Position,
						token.Length,
						kind,
						rule.ExpandedDefinition,
						rule.Position,
						rule.Signature,
						rule.ParameterCount));
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

		return new GramDocument(
			classifications,
			NormalizeDiagnostics(compilation.Diagnostics, tokens.Tokens),
			SymbolOccurrences(parsed.File.Decls, tokens.Tokens, rules));
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

		VisitDeclarations(declarations);
		result.Sort(static (left, right) => left.Position.CompareTo(right.Position));

		return result;

		void Add(string name, Location at, bool isDefinition)
		{
			if (!rules.TryGetValue(name, out var rule))
				return;

			result.Add(new GramSymbolOccurrence(
				name,
				at.Position,
				name.Length,
				rule.Position,
				isDefinition));
		}

		void VisitDeclarations(IReadOnlyList<Decl> items)
		{
			foreach (var declaration in items)
				switch (declaration)
				{
					case Decl.Rule rule:
						Add(rule.Name, rule.At, true);
						Visit(rule.Body);
						break;
					case Decl.Context context:
						VisitDeclarations(context.Decls);
						break;
					case Decl.Publish publish:
						var token = tokens.FirstOrDefault(candidate =>
							candidate.Position >= publish.At.Position &&
							candidate.Position < publish.At.End &&
							candidate.Value == publish.RuleName);
						if (token.Length > 0)
							Add(publish.RuleName, new Location(token.Position, token.Length), false);
						break;
				}
		}

		void AddReference(Expr.Reference reference)
		{
			if (!reference.IsCSharp)
				Add(reference.Name, reference.At, false);
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
						if (element is Elem.Ref reference) AddReference(reference.Reference);
					break;
				case Expr.Reference reference:
					AddReference(reference);
					break;
				case Expr.Call call:
					AddReference(call.Target);
					foreach (var argument in call.Arguments) Visit(argument);
					break;
				case Expr.Quantified quantified:
					Visit(quantified.Operand);
					break;
			}
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
					case Decl.Context context:
						Collect(context.Decls);
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
