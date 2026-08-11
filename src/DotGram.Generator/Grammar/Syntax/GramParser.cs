using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DotGram.Grammar.Syntax;

/// <summary>
/// Turns tokens into a syntax tree, following the productions in docs/syntax.md §9.
/// </summary>
/// <remarks>
/// <para>
/// Diagnostics come before everything else here. The parser never stops at the first
/// problem: on an error it reports it with a span pointing at the offending token,
/// then skips to the next declaration boundary and carries on, so one broken rule
/// costs one message rather than the rest of the file.
/// </para>
/// <para>
/// A declaration boundary is recognizable without any layout rule: an identifier
/// followed by <c>=</c>, <c>:</c> or <c>(</c> can only start a rule, because every
/// operand needs an explicit connector and no expression can end there.
/// </para>
/// </remarks>
public sealed class GramParser
{
	public const string ExpectedToken       = "GRAM2001";
	public const string ExpectedDeclaration = "GRAM2002";
	public const string ExpectedExpression  = "GRAM2003";
	public const string ExpectedName        = "GRAM2004";
	public const string InvalidCount        = "GRAM2005";

	readonly TokenList            _tokens;
	readonly List<GramDiagnostic> _diagnostics = [];

	int _index;

	/// <summary>
	/// Set when a required operand was missing. Composition loops stop looking at it,
	/// so a broken expression abandons its own rule instead of swallowing the name of
	/// the next one — a bare identifier is, after all, a perfectly good reference.
	/// </summary>
	bool _panic;

	GramParser(TokenList tokens) => _tokens = tokens;

	public static ParseResult Parse(TokenList tokens)
	{
		if (tokens is null)
			throw new ArgumentNullException(nameof(tokens));

		var parser = new GramParser(tokens);
		var file   = parser.ParseFile();

		return new ParseResult(file, [.. tokens.Diagnostics, .. parser._diagnostics]);
	}

	// ── Token access ─────────────────────────────────────────────────────────────

	Token Current => _tokens[_index];
	Token Next    => _index + 1 < _tokens.Count ? _tokens[_index + 1] : _tokens[_tokens.Count - 1];

	bool AtEnd => Current.Kind == TokenKind.EndOfFile;

	Token Take() => AtEnd ? Current : _tokens[_index++];

	bool At(TokenKind kind) => Current.Kind == kind;

	bool AtKeyword(string keyword) =>
		Current.Kind == TokenKind.Identifier && Current.Value == keyword;

	bool TakeIf(TokenKind kind)
	{
		if (!At(kind))
			return false;

		_index++;

		return true;
	}

	bool TakeIfKeyword(string keyword)
	{
		if (!AtKeyword(keyword))
			return false;

		_index++;

		return true;
	}

	void Report(string id, string message) =>
		Report(id, message, Current.Position, Math.Max(Current.Length, 1));

	void Report(string id, string message, int position, int length) =>
		_diagnostics.Add(new GramDiagnostic(id, message, position, length, GramSeverity.Error));

	Token Expect(TokenKind kind)
	{
		if (At(kind))
			return Take();

		Report(ExpectedToken, $"Expected '{kind.Spelling() ?? kind.ToString()}'.");

		return new Token(kind, Current.Position, 0, null);
	}

	string ExpectName()
	{
		if (At(TokenKind.Identifier))
			return Take().Value!;

		Report(ExpectedName, "Expected a name.");

		return "";
	}

	/// <summary>A qualified name, dots and all: <c>System.Text</c>, <c>Lexical.Token</c>.</summary>
	string ExpectQualifiedName()
	{
		var name = ExpectName();

		while (At(TokenKind.Dot))
		{
			Take();
			name += "." + ExpectName();
		}

		return name;
	}

	// ── File and declarations ────────────────────────────────────────────────────

	GrammarFile ParseFile()
	{
		var usings       = new List<UsingDirective>();
		var declarations = new List<Declaration>();

		while (AtUsing())
			usings.Add(ParseUsing());

		while (!AtEnd)
		{
			var before      = _index;
			var declaration = ParseDeclaration();

			if (declaration is not null)
				declarations.Add(declaration);

			// Nothing consumed means the current token starts nothing at all; skipping
			// it is what keeps the loop from spinning on the same diagnostic.
			if (_index == before)
				SkipToDeclaration();
		}

		return new GrammarFile(usings, declarations, 0, _tokens[_tokens.Count - 1].Position);
	}

	bool AtUsing() =>
		AtKeyword("using") && Next.Kind == TokenKind.Identifier ||
		At(TokenKind.At) && Next.Kind == TokenKind.Identifier && Next.Value == "using";

	UsingDirective ParseUsing()
	{
		var start    = Current.Position;
		var isCSharp = TakeIf(TokenKind.At);

		Take();                                     // `using`

		var name = ExpectQualifiedName();

		Expect(TokenKind.Semicolon);

		return new UsingDirective(isCSharp, name, start, Current.Position - start);
	}

	Declaration? ParseDeclaration()
	{
		if (AtKeyword("scope") && Next.Kind == TokenKind.Identifier && !StartsRule())
			return ParseScope();

		if (AtPublication())
			return ParsePublication();

		if (At(TokenKind.Identifier))
			return ParseRule();

		Report(ExpectedDeclaration, "Expected a rule, a scope or a publication directive.");

		return null;
	}

	/// <summary>
	/// Whether the current identifier starts a rule rather than being a contextual
	/// keyword — the check that lets `scope`, `parse` and the rest stay ordinary names.
	/// </summary>
	bool StartsRule() =>
		At(TokenKind.Identifier) &&
		Next.Kind is TokenKind.Equals or TokenKind.Colon or TokenKind.OpenParen;

	bool AtPublication() =>
		!StartsRule() &&
		(AtKeyword("parse") || AtKeyword("match") || AtKeyword("find")) &&
		(Next.Kind == TokenKind.Identifier);

	ScopeDeclaration ParseScope()
	{
		var start = Current.Position;

		Take();                                     // `scope`

		var name         = ExpectName();
		var usings       = new List<UsingDirective>();
		var declarations = new List<Declaration>();

		Expect(TokenKind.OpenBrace);

		while (AtUsing())
			usings.Add(ParseUsing());

		while (!AtEnd && !At(TokenKind.CloseBrace))
		{
			var before      = _index;
			var declaration = ParseDeclaration();

			if (declaration is not null)
				declarations.Add(declaration);

			if (_index == before)
				SkipToDeclaration();
		}

		Expect(TokenKind.CloseBrace);

		return new ScopeDeclaration(name, usings, declarations, start, Current.Position - start);
	}

	PublicationDirective ParsePublication()
	{
		var start = Current.Position;
		var word  = Take().Value!;

		var kind = word switch
		{
			"parse" => PublicationKind.Parse,
			"match" => PublicationKind.Match,
			_       => TakeIfKeyword("all") ? PublicationKind.FindAll : PublicationKind.Find,
		};

		var name  = ExpectQualifiedName();
		var alias = TakeIfKeyword("as") ? ExpectName() : null;

		return new PublicationDirective(kind, name, alias, start, Current.Position - start);
	}

	RuleDeclaration ParseRule()
	{
		_panic = false;

		var start      = Current.Position;
		var name       = ExpectName();
		var parameters = At(TokenKind.OpenParen) ? ParseParameters() : [];
		var type       = TakeIf(TokenKind.Colon) ? ParseType() : null;

		Expect(TokenKind.Equals);

		var body = ParseBody();

		return new RuleDeclaration(name, parameters, type, body, start, Current.Position - start);
	}

	List<ParameterDeclaration> ParseParameters()
	{
		var parameters = new List<ParameterDeclaration>();

		Expect(TokenKind.OpenParen);

		while (!AtEnd && !At(TokenKind.CloseParen))
		{
			var start = Current.Position;
			var name  = ExpectName();
			var type  = TakeIf(TokenKind.Colon) ? ParseType() : null;

			parameters.Add(new ParameterDeclaration(name, type, start, Current.Position - start));

			if (!TakeIf(TokenKind.Comma))
				break;
		}

		Expect(TokenKind.CloseParen);

		return parameters;
	}

	TypeReference ParseType()
	{
		var start    = Current.Position;
		var isCSharp = TakeIf(TokenKind.At);
		var name     = ExpectQualifiedName();
		var sequence = false;

		if (At(TokenKind.OpenBracket) && Next.Kind == TokenKind.CloseBracket)
		{
			Take();
			Take();

			sequence = true;
		}

		return new TypeReference(isCSharp, name, sequence, start, Current.Position - start);
	}

	// ── Expressions ──────────────────────────────────────────────────────────────

	Expression ParseBody()
	{
		var start        = Current.Position;
		var alternatives = new List<Expression> { ParseAlternative() };

		while (!_panic && TakeIf(TokenKind.Bar))
			alternatives.Add(ParseAlternative());

		return alternatives.Count == 1
			? alternatives[0]
			: new ChoiceExpression(alternatives, start, Current.Position - start);
	}

	Expression ParseAlternative()
	{
		var start   = Current.Position;
		var pattern = ParseSequence();

		if (!TakeIf(TokenKind.Arrow))
			return pattern;

		return new AlternativeExpression(pattern, ParseValue(), start, Current.Position - start);
	}

	Expression ParseSequence()
	{
		var start    = Current.Position;
		var operands = new List<Expression> { ParseOperand() };

		while (!_panic && TakeIf(TokenKind.Ampersand))
			operands.Add(ParseOperand());

		return operands.Count == 1
			? operands[0]
			: new SequenceExpression(operands, start, Current.Position - start);
	}

	Expression ParseOperand()
	{
		if (!AtKeyword("where"))
			return ParseQuantified();

		var start = Current.Position;

		Take();

		return new GuardExpression(ParseValue(), start, Current.Position - start);
	}

	Expression ParseQuantified()
	{
		var start   = Current.Position;
		var operand = ParsePrefixed();

		if (TakeIf(TokenKind.Question))
			return new QuantifiedExpression(operand, QuantifierKind.Optional, null, null, null, null, start, Current.Position - start);

		if (TakeIf(TokenKind.Star))
			return new QuantifiedExpression(operand, QuantifierKind.ZeroOrMore, null, null, null, null, start, Current.Position - start);

		if (TakeIf(TokenKind.Plus))
			return new QuantifiedExpression(operand, QuantifierKind.OneOrMore, null, null, null, null, start, Current.Position - start);

		if (!At(TokenKind.OpenBrace))
			return operand;

		Take();

		var (min, minName) = ParseCount();

		int?    max     = min;
		string? maxName = minName;

		if (TakeIf(TokenKind.Comma))
			(max, maxName) = At(TokenKind.CloseBrace) ? (null, null) : ParseCount();

		Expect(TokenKind.CloseBrace);

		return new QuantifiedExpression(
			operand, QuantifierKind.Count, min, minName, max, maxName, start, Current.Position - start);
	}

	(int? Value, string? Name) ParseCount()
	{
		if (At(TokenKind.Identifier))
			return (null, Take().Value);

		if (!At(TokenKind.Integer))
		{
			Report(InvalidCount, "Expected a repetition count or a parameter name.");

			return (null, null);
		}

		return (int.Parse(Take().Value!, CultureInfo.InvariantCulture), null);
	}

	Expression ParsePrefixed()
	{
		if (!At(TokenKind.PositiveLookahead) && !At(TokenKind.NegativeLookahead))
			return ParseCaptured();

		var start      = Current.Position;
		var isPositive = Take().Kind == TokenKind.PositiveLookahead;

		return new LookaheadExpression(isPositive, ParseCaptured(), start, Current.Position - start);
	}

	Expression ParseCaptured()
	{
		if (!At(TokenKind.Identifier) || Next.Kind != TokenKind.Colon)
			return ParsePrimary();

		var start = Current.Position;
		var name  = Take().Value!;

		Take();                                     // `:`

		return new CaptureExpression(name, ParsePrimary(), start, Current.Position - start);
	}

	Expression ParsePrimary()
	{
		var start = Current.Position;

		switch (Current.Kind)
		{
			case TokenKind.Character:
			case TokenKind.String:
			{
				var token = Take();

				return new LiteralExpression(token.Kind == TokenKind.Character, token.Value!, start, token.Length);
			}

			case TokenKind.CSharpExpression:
			{
				var token = Take();

				return new CSharpExpression(token.Value!, start, token.Length);
			}

			case TokenKind.OpenBracket:
				return ParseElementSet();

			case TokenKind.OpenParen:
			{
				Take();

				var body = ParseBody();

				Expect(TokenKind.CloseParen);

				return new GroupExpression(body, start, Current.Position - start);
			}

			case TokenKind.At:
			case TokenKind.Identifier:
				return ParseReferenceOrCall();

			default:
				Report(ExpectedExpression, "Expected a literal, a reference, an element set or a group.");

				_panic = true;

				return new ReferenceExpression(false, "", [], start, 0);
		}
	}

	/// <summary>A value position — <c>=&gt;</c> and <c>where</c> take these.</summary>
	Expression ParseValue() =>
		At(TokenKind.CSharpExpression)
			? ParsePrimary()
			: At(TokenKind.At) || At(TokenKind.Identifier)
				? ParseReferenceOrCall()
				: ParsePrimary();

	Expression ParseReferenceOrCall()
	{
		var reference = ParseReference();

		if (!At(TokenKind.OpenParen))
			return reference;

		Take();

		var arguments = new List<Expression>();

		while (!AtEnd && !At(TokenKind.CloseParen))
		{
			arguments.Add(ParseAlternative());

			if (!TakeIf(TokenKind.Comma))
				break;
		}

		Expect(TokenKind.CloseParen);

		return new CallExpression(reference, arguments, reference.Position, Current.Position - reference.Position);
	}

	ReferenceExpression ParseReference()
	{
		var start         = Current.Position;
		var isCSharp      = TakeIf(TokenKind.At);
		var name          = ExpectQualifiedName();
		var typeArguments = new List<TypeReference>();

		if (TakeIf(TokenKind.Less))
		{
			do
				typeArguments.Add(ParseType());
			while (TakeIf(TokenKind.Comma));

			Expect(TokenKind.Greater);
		}

		return new ReferenceExpression(isCSharp, name, typeArguments, start, Current.Position - start);
	}

	ElementSetExpression ParseElementSet()
	{
		var start = Current.Position;

		Expect(TokenKind.OpenBracket);

		var negated = TakeIf(TokenKind.Caret);
		var items   = new List<ElementSetItem>();

		do
		{
			var itemStart = Current.Position;

			switch (Current.Kind)
			{
				case TokenKind.Character:
				{
					var from = Take().Value!;
					string? to = null;

					if (TakeIf(TokenKind.DotDot))
						to = At(TokenKind.Character) ? Take().Value : null;

					items.Add(new CharacterRangeItem(from, to, itemStart, Current.Position - itemStart));
					break;
				}

				case TokenKind.UnicodeCategory:
					items.Add(new UnicodeCategoryItem(Take().Value!, itemStart, Current.Position - itemStart));
					break;

				case TokenKind.At:
				case TokenKind.Identifier:
					items.Add(new ReferenceItem(ParseReference(), itemStart, Current.Position - itemStart));
					break;

				default:
					Report(ExpectedExpression, "Expected a character, a range, a Unicode category or a reference.");
					goto done;
			}
		}
		while (TakeIf(TokenKind.Bar));

	done:
		Expect(TokenKind.CloseBracket);

		return new ElementSetExpression(negated, items, start, Current.Position - start);
	}

	// ── Recovery ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// Skips to something that can begin a declaration, so a broken rule costs one
	/// diagnostic instead of every diagnostic after it.
	/// </summary>
	void SkipToDeclaration()
	{
		Take();

		while (!AtEnd && !At(TokenKind.CloseBrace))
		{
			if (StartsRule() || AtUsing() || AtKeyword("scope") || AtPublication())
				return;

			Take();
		}
	}
}

/// <summary>What parsing produced: a tree, and everything wrong with the source.</summary>
public sealed class ParseResult(GrammarFile file, IReadOnlyList<GramDiagnostic> diagnostics)
{
	public GrammarFile                   File        { get; } = file;
	public IReadOnlyList<GramDiagnostic> Diagnostics { get; } = diagnostics;

	public bool HasErrors => Diagnostics.Count > 0;

	/// <summary>The tree, then the diagnostics — both in one comparable dump.</summary>
	public override string ToString()
	{
		var text = new StringBuilder(File.ToString());

		foreach (var diagnostic in Diagnostics)
			text.AppendLine().Append(diagnostic);

		return text.ToString();
	}
}
