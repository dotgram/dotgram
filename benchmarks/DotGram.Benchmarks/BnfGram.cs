using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DotGram.Benchmarks;

/// <summary>
/// `--bnf-gram [file]`: the ISO BNF of SQL:2023 written as a `.gram` skeleton — every production
/// under its own name, the lexical ones in a namespace of their own — for the standard's grammar
/// to be written from by hand.
/// </summary>
/// <remarks>
/// <para>
/// <b>A skeleton and not a parser.</b> The BNF's choice is unordered and a `.gram` choice takes the
/// first alternative that matches; the BNF recurses on the left through other productions, which a
/// `.gram` rule may only do on itself (GRAM4002); and its lexical productions spell classes of
/// characters one character at a time. What the skeleton keeps is what a hand-written grammar must
/// not lose: the name of every production, spelled so that a test can find it again, and its shape.
/// </para>
/// <para>
/// A name is the BNF's with each part capitalized and what divides the parts dropped:
/// <c>&lt;query expression&gt;</c> is <c>QueryExpression</c>, <c>&lt;non-delimiter token&gt;</c>
/// <c>NonDelimiterToken</c>, <c>&lt;SQL/JSON special symbol&gt;</c> <c>SQLJSONSpecialSymbol</c>.
/// </para>
/// </remarks>
static class BnfGram
{
	public static void Run(string? output)
	{
		var root  = Root();
		var rules = Bnf.Read(File.ReadAllText(Path.Combine(root, "src", "DotGram.Sql", "Standard", "Specification", "ISO_IEC_9075-2(E)_Foundation.bnf.txt")));
		var path  = output ?? Path.Combine(root, ".work", "SqlStandard.skeleton.gram");

		var clashes = rules.Keys.GroupBy(Bnf.RuleName).Where(static group => group.Count() > 1).ToList();

		foreach (var clash in clashes)
			Console.WriteLine($"one name, {clash.Key}, for {string.Join(" and ", clash.Select(static name => $"<{name}>"))}");

		var lexical  = StandardOracle.LexicalProductions(rules);
		var skeleton = Skeleton(rules, lexical);

		Directory.CreateDirectory(Path.GetDirectoryName(path)!);
		File.WriteAllText(path, skeleton.Replace("\n", "\r\n"), new UTF8Encoding(false));

		var text = rules.Values.Count(static body => body is BnfText);

		Console.WriteLine($"{rules.Count} productions — {lexical.Count} lexical, {rules.Count - lexical.Count} syntactic, {text} left to the Syntax Rules, {clashes.Count} names shared — into {path}");
	}

	/// <summary>The whole skeleton: a header, the lexical namespace, and the syntactic productions after it.</summary>
	public static string Skeleton(Dictionary<string, BnfNode> rules, HashSet<string> lexical)
	{
		var text = new StringBuilder();

		text.Append("// ISO/IEC 9075-2:2023, SQL/Foundation, written from its BNF by `--bnf-gram` (BnfGram.cs).\n");
		text.Append("//\n");
		text.Append("// A skeleton, not a parser: the names and the shapes of the productions, for the standard's\n");
		text.Append("// grammar to be written from. The BNF's choice is unordered and this one's ordered, its left\n");
		text.Append("// recursion goes through other productions, and its lexical productions spell classes one\n");
		text.Append("// character at a time; all three are the hand's to settle.\n\n");
		text.Append("using Lexical;\n\n");
		text.Append("namespace Lexical\n{\n\ttrivia = none\n");

		foreach (var (name, body) in rules)
			if (lexical.Contains(name))
				Rule(text, name, body, true, "\t");

		text.Append("}\n");

		foreach (var (name, body) in rules)
			if (!lexical.Contains(name))
				Rule(text, name, body, false, "");

		return text.ToString();
	}

	static void Rule(StringBuilder text, string name, BnfNode body, bool lexical, string indent)
	{
		text.Append('\n').Append(indent).Append("// <").Append(name).Append(">\n");

		if (body is BnfText)
		{
			text.Append(indent).Append("// !! See the Syntax Rules.\n");
			text.Append(indent).Append(Bnf.RuleName(name)).Append(" = none\n");
			return;
		}

		if (Narrowed(body))
			text.Append(indent).Append("// !! Narrowed by the Syntax Rules, which are not written here.\n");

		text.Append(indent).Append(Bnf.RuleName(name));

		if (body is BnfChoice choice)
		{
			for (var i = 0; i < choice.Options.Length; i++)
				text.Append('\n').Append(indent).Append(i == 0 ? "\t= " : "\t| ").Append(Render(choice.Options[i], lexical, 1));

			text.Append('\n');
		}
		else
		{
			text.Append(" = ").Append(Render(body, lexical, 0)).Append('\n');
		}
	}

	static bool Narrowed(BnfNode node)
	{
		return node switch
		{
			BnfSequence sequence => sequence.Items.Any(static item => item is BnfText || Narrowed(item)),
			BnfChoice choice => choice.Options.Any(static option => option is BnfText || Narrowed(option)),
			BnfOptional optional => Narrowed(optional.Body),
			BnfRepeated repeated => Narrowed(repeated.Body),
			_ => false,
		};
	}

	/// <summary>
	/// A piece as a `.gram` expression, bracketed as its place needs: a choice below the top, and a
	/// sequence where a postfix applies to it.
	/// </summary>
	/// <param name="level">0 at the top, 1 inside a sequence, 2 under a postfix.</param>
	static string Render(BnfNode node, bool lexical, int level)
	{
		switch (node)
		{
			case BnfRule rule:
				return Bnf.RuleName(rule.Name);

			case BnfWord word:
				return Literal(word.Text, lexical);

			// `[ x... ]`, none or more, is `x*` rather than `x+?`.
			case BnfOptional { Body: BnfRepeated repeated }:
				return Render(repeated.Body, lexical, 2) + "*";

			case BnfOptional optional:
				return Render(optional.Body, lexical, 2) + "?";

			case BnfRepeated repeated:
				return Render(repeated.Body, lexical, 2) + "+";

			case BnfSequence sequence:
			{
				var items = sequence.Items.Where(static item => item is not BnfText).ToArray();

				if (items.Length == 0)
					return "none";

				var joined = string.Join(" & ", items.Select(item => Render(item, lexical, 1)));
				return items.Length > 1 && level >= 2 ? "(" + joined + ")" : joined;
			}

			case BnfChoice choice:
			{
				var joined = string.Join(" | ", choice.Options.Select(option => Render(option, lexical, 1)));
				return level >= 1 ? "(" + joined + ")" : joined;
			}

			default:
				return "none";
		}
	}

	/// <summary>A character in a lexical production, a key word in a syntactic one, whose case does not matter.</summary>
	static string Literal(string text, bool lexical)
	{
		if (lexical && text.Length == 1)
			return "'" + Escape(text, '\'') + "'";

		var keyword = !lexical && text.All(static one => char.IsLetterOrDigit(one) || one is '_' or '-');

		return "\"" + Escape(text, '"') + "\"" + (keyword ? "i" : "");
	}

	static string Escape(string text, char quote)
	{
		return text.Replace("\\", "\\\\").Replace(quote.ToString(), "\\" + quote);
	}

	static string Root()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? ".";
	}
}
