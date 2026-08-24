using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DotGram.Grammar.Parsing;

/// <summary>
/// Turns tokens into a syntax tree, following the productions in docs/syntax.md §10.
/// </summary>
/// <remarks>
/// <para>
/// Diagnostics come before everything else. The parser never stops at the first
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

	Location From(int start) => new(start, Current.Position - start);

	void Report(string id, string message) =>
		Report(id, message, new Location(Current.Position, Math.Max(Current.Length, 1)));

	void Report(string id, string message, Location at) =>
		_diagnostics.Add(new GramDiagnostic(id, message, at.Position, at.Length, GramSeverity.Error));

	void Expect(TokenKind kind)
	{
		if (At(kind))
			Take();
		else
			Report(ExpectedToken, $"Expected '{kind.Spelling() ?? kind.ToString()}'.");
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
		var usings       = new List<Using>();
		var declarations = new List<Decl>();

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

		return new GrammarFile(usings, declarations, new Location(0, _tokens[_tokens.Count - 1].Position));
	}

	bool AtUsing() =>
		AtKeyword("using") && Next.Kind == TokenKind.Identifier ||
		At(TokenKind.At) && Next.Kind == TokenKind.Identifier && Next.Value == "using";

	Using ParseUsing()
	{
		var start    = Current.Position;
		var isCSharp = TakeIf(TokenKind.At);

		Take();                                     // `using`

		var name = ExpectQualifiedName();

		Expect(TokenKind.Semicolon);

		return new Using(isCSharp, name, From(start));
	}

	Decl? ParseDeclaration()
	{
		if (AtKeyword("context") && Next.Kind == TokenKind.Identifier && !StartsRule())
			return ParseContext();

		if (AtPublication())
			return ParsePublication();

		if (At(TokenKind.Identifier))
			return ParseRule();

		Report(ExpectedDeclaration, "Expected a rule, a context or a publication directive.");

		return null;
	}

	/// <summary>
	/// Whether the current identifier starts a rule rather than being a contextual
	/// keyword — the check that lets `context`, `parse` and the rest stay ordinary names.
	/// </summary>
	bool StartsRule() =>
		At(TokenKind.Identifier) &&
		(Next.Kind is TokenKind.Equals or TokenKind.OpenParen || StartsTypedRule());

	/// <summary>
	/// Whether the current identifier begins a typed rule rather than a capture left in
	/// the wreckage of a broken expression. Recovery needs the whole <c>: Type =</c>
	/// prefix: <c>date: Digit{4}</c> inside a rule is otherwise indistinguishable from a
	/// declaration at the colon and produces a second cascade.
	/// </summary>
	bool StartsTypedRule()
	{
		if (Next.Kind != TokenKind.Colon)
			return false;

		var at = _index + 2;

		if (_tokens[at].Kind == TokenKind.At)
			at++;

		if (_tokens[at].Kind != TokenKind.Identifier)
			return false;

		at++;

		while (_tokens[at].Kind == TokenKind.Dot && _tokens[at + 1].Kind == TokenKind.Identifier)
			at += 2;

		if (_tokens[at].Kind == TokenKind.OpenBracket && _tokens[at + 1].Kind == TokenKind.CloseBracket)
			at += 2;

		return _tokens[at].Kind == TokenKind.Equals;
	}

	bool AtPublication() =>
		!StartsRule() &&
		(AtKeyword("parse") || AtKeyword("find")) &&
		Next.Kind == TokenKind.Identifier;

	Decl ParseContext()
	{
		var start = Current.Position;

		Take();                                     // `context`

		var name         = ExpectName();
		var rebindings   = At(TokenKind.OpenParen) ? ParseRebindings() : [];
		var usings       = new List<Using>();
		var declarations = new List<Decl>();

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

		return new Decl.Context(name, rebindings, usings, declarations) { At = From(start) };
	}

	List<Rebinding> ParseRebindings()
	{
		var rebindings = new List<Rebinding>();

		Expect(TokenKind.OpenParen);

		while (!AtEnd && !At(TokenKind.CloseParen))
		{
			var start = Current.Position;
			var left  = ExpectName();

			Expect(TokenKind.Equals);

			var right = ExpectName();

			rebindings.Add(new Rebinding(left, right, From(start)));

			if (!TakeIf(TokenKind.Comma))
				break;
		}

		Expect(TokenKind.CloseParen);

		return rebindings;
	}

	Decl ParsePublication()
	{
		var start = Current.Position;
		var word  = Take().Value!;
		var kind  = word == "parse" ? PublishKind.Parse : PublishKind.Find;
		var name  = ExpectQualifiedName();
		var alias = TakeIfKeyword("as") ? ExpectName() : null;

		return new Decl.Publish(kind, name, alias) { At = From(start) };
	}

	Decl ParseRule()
	{
		_panic = false;

		var start      = Current.Position;
		var name       = ExpectName();
		var parameters = At(TokenKind.OpenParen) ? ParseParameters() : [];
		var type       = TakeIf(TokenKind.Colon) ? ParseType() : null;

		Expect(TokenKind.Equals);

		var body = ParseBody();

		return new Decl.Rule(name, parameters, type, body) { At = From(start) };
	}

	List<Param> ParseParameters()
	{
		var parameters = new List<Param>();

		Expect(TokenKind.OpenParen);

		while (!AtEnd && !At(TokenKind.CloseParen))
		{
			var start = Current.Position;
			var name  = ExpectName();
			var type  = TakeIf(TokenKind.Colon) ? ParseType() : null;

			parameters.Add(new Param(name, type, From(start)));

			if (!TakeIf(TokenKind.Comma))
				break;
		}

		Expect(TokenKind.CloseParen);

		return parameters;
	}

	TypeRef ParseType()
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

		return new TypeRef(isCSharp, name, sequence, From(start));
	}

	// ── Expressions ──────────────────────────────────────────────────────────────

	Expr ParseBody()
	{
		var start        = Current.Position;
		var alternatives = new List<Expr> { ParseAlternative() };

		while (!_panic && TakeIf(TokenKind.Bar))
			alternatives.Add(ParseAlternative());

		return alternatives.Count == 1
			? alternatives[0]
			: new Expr.Choice(alternatives) { At = From(start) };
	}

	Expr ParseAlternative()
	{
		var start   = Current.Position;
		var pattern = ParseBound(ParseSequence(), start);

		if (!TakeIf(TokenKind.Arrow))
			return pattern;

		return new Expr.Construct(pattern, ParseValue()) { At = From(start) };
	}

	/// <summary>
	/// A binding power on an alternative — <c>&lt;&lt; 2</c> or <c>&gt;&gt; 3</c> (§4.3.1).
	/// </summary>
	/// <remarks>
	/// Two tokens rather than one, which costs nothing and keeps <c>&gt;&gt;</c> from
	/// having to be told apart from the end of a nested type argument list. Neither can
	/// appear where an alternative has just ended, so a doubled one here is unambiguous.
	/// </remarks>
	Expr ParseBound(Expr pattern, int start)
	{
		var isLeft = At(TokenKind.Less)    && Next.Kind == TokenKind.Less;
		var isDown = At(TokenKind.Greater) && Next.Kind == TokenKind.Greater;

		if (!isLeft && !isDown)
			return pattern;

		Take();
		Take();

		var level = Current.Kind == TokenKind.Integer && Current.Value is { } text &&
			int.TryParse(text, out var parsed)
				? parsed
				: -1;

		if (level < 0)
			Report(InvalidCount, "A binding power is a whole number: '<< 2', '>> 3'.");
		else
			Take();

		return new Expr.Bound(pattern, isLeft, level) { At = From(start) };
	}

	Expr ParseSequence()
	{
		var start    = Current.Position;
		var operands = new List<Expr> { ParseOperand() };

		while (!_panic && TakeIf(TokenKind.Ampersand))
			operands.Add(ParseOperand());

		return operands.Count == 1
			? operands[0]
			: new Expr.Sequence(operands) { At = From(start) };
	}

	Expr ParseOperand()
	{
		if (!AtKeyword("when"))
			return ParseQuantified();

		var start = Current.Position;

		Take();

		return new Expr.Guard(ParseValue()) { At = From(start) };
	}

	Expr ParseQuantified()
	{
		var start = Current.Position;

		return ParseWith(ParseQuantifiedCore(start), start);
	}

	Expr ParseQuantifiedCore(int start)
	{
		var operand = ParsePrefixed();

		Expr Quantify(QuantifierKind kind, int? min = null, string? minName = null, int? max = null, string? maxName = null) =>
			Recovering(new Expr.Quantified(operand, kind, min, minName, max, maxName) { At = From(start) }, start);

		if (TakeIf(TokenKind.Question)) return Quantify(QuantifierKind.Optional);
		if (TakeIf(TokenKind.Star))     return Quantify(QuantifierKind.ZeroOrMore);
		if (TakeIf(TokenKind.Plus))     return Quantify(QuantifierKind.OneOrMore);

		if (!At(TokenKind.OpenBrace))
			return Recovering(operand, start);

		Take();

		var (min, minName) = ParseCount();

		int?    max     = min;
		string? maxName = minName;

		if (TakeIf(TokenKind.Comma))
			(max, maxName) = At(TokenKind.CloseBrace) ? (null, null) : ParseCount();

		var closeAt = Current.Position;

		Expect(TokenKind.CloseBrace);

		// Not caught later: a recognizer for {5,2} builds and runs, and simply never
		// matches, which is the hardest kind of grammar bug to see.
		if (min is { } lower && max is { } upper && lower > upper)
			Report(
				InvalidCount,
				$"'{{{lower},{upper}}}' asks for at least {lower} and at most {upper}, so it can never match.",
				new Location(start, closeAt + 1 - start));

		return Quantify(QuantifierKind.Count, min, minName, max, maxName);
	}

	/// <summary>
	/// <c>Number with (Point = Comma)</c> — §5.1's substitution, applied to one
	/// expression instead of a whole <c>context</c> block. Checked last, outermost of
	/// quantifier/<c>recover</c>/<c>with</c> at this one precedence level: <c>Number+
	/// with (X=Y)</c> is <c>(Number+) with (X=Y)</c>, and the reverse needs parens.
	/// </summary>
	Expr ParseWith(Expr operand, int start)
	{
		if (!TakeIfKeyword("with"))
			return operand;

		return new Expr.With(operand, ParseRebindings()) { At = From(start) };
	}

	/// <summary>
	/// <c>recover eol</c>, or <c>recover eol =&gt; @Bad(…)</c> — a repetition that
	/// survives a bad element (§8.2).
	/// </summary>
	/// <remarks>
	/// The synchronization expression is a prefixed operand rather than a whole sequence:
	/// anything larger would swallow the <c>&amp;</c> that ends the repetition's place in
	/// the sequence it sits in.
	/// </remarks>
	Expr Recovering(Expr repetition, int start)
	{
		if (!TakeIfKeyword("recover"))
			return repetition;

		var sync    = ParsePrefixed();
		var factory = TakeIf(TokenKind.Arrow) ? ParseValue() : null;

		return new Expr.Recovering(repetition, sync, factory) { At = From(start) };
	}

	/// <summary>A number where a value goes, with the same overflow care as a count.</summary>
	Expr ParseNumber()
	{
		var token = Take();

		if (int.TryParse(token.Value!, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
			return new Expr.Number(value) { At = new Location(token.Position, token.Length) };

		Report(
			InvalidCount,
			$"'{token.Value}' is too large for an argument.",
			new Location(token.Position, token.Length));

		return new Expr.Number(0) { At = new Location(token.Position, token.Length) };
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

		var token = Take();

		// int.Parse would throw, and an exception out of here is not a grammar error any
		// more — it is a generator crash, reported against the consumer's build as
		// CS8785 with our stack trace in it.
		if (!int.TryParse(token.Value!, NumberStyles.None, CultureInfo.InvariantCulture, out var count))
		{
			Report(
				InvalidCount,
				$"'{token.Value}' is too large for a repetition count.",
				new Location(token.Position, token.Length));

			return (null, null);
		}

		return (count, null);
	}

	Expr ParsePrefixed()
	{
		if (!At(TokenKind.PositiveLookahead) && !At(TokenKind.NegativeLookahead))
			return ParseCaptured();

		var start      = Current.Position;
		var isPositive = Take().Kind == TokenKind.PositiveLookahead;

		return new Expr.Lookahead(isPositive, ParseCaptured()) { At = From(start) };
	}

	Expr ParseCaptured()
	{
		if (!At(TokenKind.Identifier) || Next.Kind != TokenKind.Colon)
			return ParsePrimary();

		var start = Current.Position;
		var name  = Take().Value!;

		Take();                                     // `:`

		// Prefixed and not primary: §3.6 writes `n: ?=Number`, a capture of what a
		// lookahead saw. The two nest either way round — `?=n: Number` is the same
		// question asked with the naming inside — and only one of them used to parse.
		return new Expr.Capture(name, ParsePrefixed()) { At = From(start) };
	}

	Expr ParsePrimary()
	{
		var start = Current.Position;

		switch (Current.Kind)
		{
			case TokenKind.Character:
			case TokenKind.String:
			case TokenKind.CaseInsensitiveCharacter:
			case TokenKind.CaseInsensitiveString:
			{
				var token      = Take();
				var isChar     = token.Kind is TokenKind.Character or TokenKind.CaseInsensitiveCharacter;
				var ignoreCase = token.Kind is TokenKind.CaseInsensitiveCharacter or TokenKind.CaseInsensitiveString;

				return new Expr.Literal(isChar, token.Value!) { At = From(start), IgnoreCase = ignoreCase };
			}

			case TokenKind.CSharpExpression:
				return new Expr.CSharp(Take().Value!) { At = From(start) };

			case TokenKind.OpenBracket:
				return ParseElementSet();

			case TokenKind.OpenParen:
			{
				Take();

				var body = ParseBody();

				Expect(TokenKind.CloseParen);

				return new Expr.Group(body) { At = From(start) };
			}

			case TokenKind.OpenBrace:
			{
				Take();

				var body = ParseBody();

				Expect(TokenKind.CloseBrace);

				return new Expr.Atomic(body) { At = From(start) };
			}

			case TokenKind.At:
			case TokenKind.Identifier:
				return ParseReferenceOrCall();

			default:
				Report(ExpectedExpression, "Expected a literal, a reference, an element set, a group or an atomic group.");

				_panic = true;

				return new Expr.Reference(false, "", []) { At = From(start) };
		}
	}

	/// <summary>A value position — <c>=&gt;</c> and <c>when</c> take these.</summary>
	Expr ParseValue() =>
		At(TokenKind.At) || At(TokenKind.Identifier) ? ParseReferenceOrCall() : ParsePrimary();

	Expr ParseReferenceOrCall()
	{
		var start     = Current.Position;
		var reference = ParseReference();

		if (!At(TokenKind.OpenParen))
			return reference;

		Take();

		var arguments = new List<Expr>();

		while (!AtEnd && !At(TokenKind.CloseParen))
		{
			// §4.2: an argument is a piece of grammar or a value. A number is only ever the
			// second — there is no position in a recognition expression where a bare one
			// means anything — so it is read here and nowhere else.
			arguments.Add(At(TokenKind.Integer) ? ParseNumber() : ParseAlternative());

			if (!TakeIf(TokenKind.Comma))
				break;
		}

		Expect(TokenKind.CloseParen);

		return new Expr.Call(reference, arguments) { At = From(start) };
	}

	Expr.Reference ParseReference()
	{
		var start         = Current.Position;
		var isCSharp      = TakeIf(TokenKind.At);
		var name          = ExpectQualifiedName();
		var typeArguments = new List<TypeRef>();

		// `<<` is a binding power (§4.3.1) and never the start of a type argument list:
		// nothing may follow the first `<` but a type, and a type never begins with `<`.
		if (Next.Kind != TokenKind.Less && TakeIf(TokenKind.Less))
		{
			do
				typeArguments.Add(ParseType());
			while (TakeIf(TokenKind.Comma));

			Expect(TokenKind.Greater);
		}

		return new Expr.Reference(isCSharp, name, typeArguments) { At = From(start) };
	}

	Expr ParseElementSet()
	{
		var start = Current.Position;

		Expect(TokenKind.OpenBracket);

		var negated = TakeIf(TokenKind.Caret);
		var items   = new List<Elem>();

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

					items.Add(new Elem.Chars(from, to) { At = From(itemStart) });
					break;
				}

				case TokenKind.UnicodeCategory:
					items.Add(new Elem.Category(Take().Value!) { At = From(itemStart) });
					break;

				case TokenKind.At:
				case TokenKind.Identifier:
					items.Add(new Elem.Ref(ParseReference()) { At = From(itemStart) });
					break;

				default:
					Report(ExpectedExpression, "Expected a character, a range, a Unicode category or a reference.");
					goto done;
			}
		}
		while (TakeIf(TokenKind.Bar));

	done:
		Expect(TokenKind.CloseBracket);

		return new Expr.ElementSet(negated, items) { At = From(start) };
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
			if (StartsRule() || AtUsing() || AtKeyword("context") || AtPublication())
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
			text.EndLine().Append(diagnostic);

		return text.ToString();
	}
}
