using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotGram.Benchmarks;

/// <summary>A piece of a production, as ISO writes its BNF.</summary>
abstract record BnfNode;

/// <summary><c>&lt;name&gt;</c>: another production.</summary>
sealed record BnfRule(string Name) : BnfNode;

/// <summary>A terminal written out: a key word in a syntactic production, characters in a lexical one.</summary>
sealed record BnfWord(string Text) : BnfNode;

/// <summary>Pieces one after another.</summary>
sealed record BnfSequence(BnfNode[] Items) : BnfNode;

/// <summary><c>a | b</c>.</summary>
sealed record BnfChoice(BnfNode[] Options) : BnfNode;

/// <summary><c>[ … ]</c>.</summary>
sealed record BnfOptional(BnfNode Body) : BnfNode;

/// <summary><c>…...</c>: once or more.</summary>
sealed record BnfRepeated(BnfNode Body) : BnfNode;

/// <summary><c>!! See the Syntax Rules.</c>: what only the standard's text says.</summary>
sealed record BnfText : BnfNode;

/// <summary>
/// The ISO BNF of a part of ISO/IEC 9075, read as it is published: a production begins at the
/// start of a line with <c>&lt;name&gt; ::=</c>, and its body runs to the next one.
/// </summary>
/// <remarks>
/// <para>
/// The body is read as one stream and not line by line, because the file breaks lines inside a
/// bracket and puts the break's <c>|</c> at the start of the next: <c>UNION [ ALL</c> and then
/// <c>| DISTINCT ]</c>.
/// </para>
/// <para>
/// A body that is one word and nothing else is that word, whatever it is spelled with:
/// <c>&lt;left bracket&gt; ::= [</c>, <c>&lt;concatenation operator&gt; ::= ||</c>,
/// <c>&lt;not equals operator&gt; ::= &lt;&gt;</c>. Everywhere else the brackets, the bar and
/// the ellipsis are the notation's.
/// </para>
/// </remarks>
static class Bnf
{
	static readonly Regex Header = new(@"^<([^>]+)>\s*::=(.*)$", RegexOptions.Compiled);

	static readonly Regex Named = new(@"<[A-Za-z][^>\n]*>", RegexOptions.Compiled);

	/// <summary>
	/// A production's name as a rule of a `.gram` grammar spells it: each part capitalized and what
	/// divides the parts dropped, so that <c>&lt;non-delimiter token&gt;</c> is <c>NonDelimiterToken</c>.
	/// </summary>
	public static string RuleName(string name)
	{
		var spelled = new StringBuilder();

		foreach (var part in Regex.Split(name, "[^A-Za-z0-9]+"))
			if (part.Length > 0)
				spelled.Append(char.ToUpperInvariant(part[0])).Append(part, 1, part.Length - 1);

		return spelled.ToString();
	}

	/// <summary>Every production of the text, by name.</summary>
	public static Dictionary<string, BnfNode> Read(string text)
	{
		var rules = new Dictionary<string, BnfNode>(StringComparer.Ordinal);
		var body  = new StringBuilder();
		string? name = null;

		foreach (var line in text.Replace("\r\n", "\n").Split('\n'))
		{
			var header = Header.Match(line);

			if (header.Success)
			{
				if (name is not null)
					rules[name] = Parse(name, body.ToString());

				name = header.Groups[1].Value;
				body.Clear().Append(header.Groups[2].Value).Append('\n');
			}
			else if (name is not null)
			{
				body.Append(line).Append('\n');
			}
		}

		if (name is not null)
			rules[name] = Parse(name, body.ToString());

		return rules;
	}

	static BnfNode Parse(string name, string body)
	{
		var trimmed = body.Trim();
		var bang    = trimmed.IndexOf("!!", StringComparison.Ordinal);
		var bare    = bang >= 0 ? trimmed.Substring(0, bang).Trim() : trimmed;

		if (bare.Length > 0 && !bare.Any(char.IsWhiteSpace) && !Named.IsMatch(bare) &&
			bare.IndexOf("...", StringComparison.Ordinal) < 0)
			return new BnfWord(bare);

		var tokens = Lex(body);
		var at     = 0;
		var node   = Choice(tokens, ref at);

		if (at != tokens.Count)
			throw new FormatException($"<{name}>: '{tokens[at]}' is not read");

		return node;
	}

	static BnfNode Choice(List<string> tokens, ref int at)
	{
		var options = new List<BnfNode> { Sequence(tokens, ref at) };

		while (at < tokens.Count && tokens[at] == "|")
		{
			at++;
			options.Add(Sequence(tokens, ref at));
		}

		// The file sets some lists with a bar before the first alternative as well, after an empty
		// line — `<set function specification> ::=`, then `| [ <running or final> ] …` — and what
		// stands before that bar is no alternative that derives nothing. Five productions are set
		// so, and no production of the BNF has an empty alternative anywhere else.
		if (options.Count > 1 && options[0] is BnfSequence { Items.Length: 0 })
			options.RemoveAt(0);

		return options.Count == 1 ? options[0] : new BnfChoice([.. options]);
	}

	static BnfNode Sequence(List<string> tokens, ref int at)
	{
		var items = new List<BnfNode>();

		while (at < tokens.Count && tokens[at] is not ("|" or "]" or "}"))
		{
			var item = Primary(tokens, ref at);

			while (at < tokens.Count && tokens[at] == "...")
			{
				at++;
				item = new BnfRepeated(item);
			}

			items.Add(item);
		}

		return items.Count == 1 ? items[0] : new BnfSequence([.. items]);
	}

	static BnfNode Primary(List<string> tokens, ref int at)
	{
		var token = tokens[at++];

		switch (token)
		{
			case "[":
			{
				var body = Choice(tokens, ref at);
				Expect(tokens, ref at, "]");
				return new BnfOptional(body);
			}

			case "{":
			{
				var body = Choice(tokens, ref at);
				Expect(tokens, ref at, "}");
				return body;
			}

			case "!!":
				return new BnfText();

			case "...":
				throw new FormatException("an ellipsis with nothing before it");
		}

		return token.Length > 2 && token[0] == '<' && token[^1] == '>' && Named.IsMatch(token)
			? new BnfRule(token.Substring(1, token.Length - 2))
			: new BnfWord(token);
	}

	static void Expect(List<string> tokens, ref int at, string what)
	{
		if (at >= tokens.Count || tokens[at] != what)
			throw new FormatException($"'{what}' expected, '{(at < tokens.Count ? tokens[at] : "the end")}' found");

		at++;
	}

	static List<string> Lex(string body)
	{
		var tokens = new List<string>();
		var at     = 0;

		while (at < body.Length)
		{
			var one = body[at];

			if (char.IsWhiteSpace(one))
			{
				at++;
			}
			else if (Bang(body, at))
			{
				tokens.Add("!!");

				while (at < body.Length && body[at] != '\n')
					at++;
			}
			else if (Ellipsis(body, at))
			{
				tokens.Add("...");
				at += 3;
			}
			else if (Name(body, at) is { } length)
			{
				tokens.Add(body.Substring(at, length));
				at += length;
			}
			else if (one is '[' or ']' or '{' or '}' or '|')
			{
				tokens.Add(one.ToString());
				at++;
			}
			else
			{
				var start = at;

				while (at < body.Length && !char.IsWhiteSpace(body[at]) && body[at] is not ('[' or ']' or '{' or '}' or '|') &&
					!Bang(body, at) && !Ellipsis(body, at) && Name(body, at) is null)
					at++;

				tokens.Add(body.Substring(start, at - start));
			}
		}

		return tokens;
	}

	static bool Bang(string body, int at) =>
		body[at] == '!' && at + 1 < body.Length && body[at + 1] == '!';

	static bool Ellipsis(string body, int at) =>
		at + 2 < body.Length && body[at] == '.' && body[at + 1] == '.' && body[at + 2] == '.';

	/// <summary>The length of a <c>&lt;name&gt;</c> starting here, or null: a letter after the bracket and its close on the same line.</summary>
	static int? Name(string body, int at)
	{
		if (body[at] != '<' || at + 1 >= body.Length || !char.IsLetter(body[at + 1]))
			return null;

		var close = body.IndexOf('>', at + 1);
		var line  = body.IndexOf('\n', at + 1);

		return close < 0 || (line >= 0 && line < close) ? null : close - at + 1;
	}
}
