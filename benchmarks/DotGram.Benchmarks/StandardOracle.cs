using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DotGram.Benchmarks;

/// <summary>
/// What the ISO BNF of the SQL standard reads, answered by the BNF itself: the standard has no
/// engine to ask, and a recognizer that takes its productions as a context-free grammar is the
/// nearest thing to one.
/// </summary>
/// <remarks>
/// <para>
/// <b>Two levels, as the standard has them.</b> §5 says what a token is — <c>&lt;token&gt;</c> —
/// and what may stand between two — <c>&lt;separator&gt;</c> — in productions over characters;
/// the rest of the grammar is productions over tokens. So a text is cut into tokens first, each
/// the longest <c>&lt;token&gt;</c> after the longest <c>&lt;separator&gt;</c>, and the tokens are
/// then recognized against the start production. A production the lexical rules reach is a
/// terminal where a syntactic one names it: <c>&lt;left paren&gt;</c> or <c>&lt;regular
/// identifier&gt;</c> matches a token whose whole text it derives. A key word written in a
/// syntactic production matches a token spelled the same, in either case.
/// </para>
/// <para>
/// <b>What the text says, written by hand and marked so.</b> A production whose body is
/// <c>!! See the Syntax Rules.</c> is a predicate on one character where it is lexical — a space,
/// what may start or continue an identifier, a quote's complement, whitespace, a newline — and
/// matches nothing where it is not, the implementation-defined statements and the host languages
/// among them. A <c>!!</c> after other pieces narrows what they read, and is not followed. And one
/// Syntax Rule is: a <c>&lt;regular identifier&gt;</c> is not a <c>&lt;reserved word&gt;</c> (§5.4).
/// </para>
/// <para>
/// The recognizer is Earley's, with Aycock and Horspool's step for what derives nothing.
/// </para>
/// </remarks>
sealed class StandardOracle
{
	readonly Dictionary<string, BnfNode> rules;
	readonly HashSet<string> lexical;
	readonly HashSet<string> reserved;
	readonly Grammar characters;
	readonly Grammar tokens;
	readonly int regular;
	readonly Dictionary<(string, string), bool> derived = new();

	public StandardOracle(Dictionary<string, BnfNode> rules)
	{
		this.rules = rules;

		lexical  = Lexical(rules);
		reserved = rules.TryGetValue("reserved word", out var words)
			? new HashSet<string>(Words(words), StringComparer.OrdinalIgnoreCase)
			: new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		var terminal = Spelled(rules, lexical);

		characters = new Grammar(rules, name => false, Character);
		tokens     = new Grammar(rules, terminal.Contains, null);
		regular    = characters.Start("regular identifier");
	}

	/// <summary>How many productions are lexical, and how many reserved words there are.</summary>
	public (int Lexical, int Reserved) Counts => (lexical.Count, reserved.Count);

	/// <summary>The text cut into tokens, or null where a character begins none.</summary>
	public List<string>? Tokens(string text)
	{
		var chars = text.ToCharArray();
		var cut   = new List<string>();
		var at    = 0;

		while (true)
		{
			at = Math.Max(at, Longest("separator", chars, at));

			if (at >= text.Length)
				return cut;

			var end = Longest("token", chars, at);

			if (end <= at)
				return null;

			cut.Add(text.Substring(at, end - at));
			at = end;
		}
	}

	/// <summary>Whether the text is a <paramref name="start"/>, and if not, the token it could not go past.</summary>
	public (bool Read, int Stopped, int Count) Reads(string start, string text)
	{
		if (!rules.ContainsKey(start))
			throw new ArgumentException($"no production <{start}>");

		if (lexical.Contains(start))
			return (Derives(start, text), 0, 1);

		var cut = Tokens(text);

		if (cut is null)
			return (false, -1, 0);

		var (ends, furthest) = Earley.Ends(tokens, tokens.Start(start), cut, 0,
			(terminal, token) => tokens.Terminal(terminal) switch
			{
				Terminal.Word word       => string.Equals(word.Text, token, StringComparison.OrdinalIgnoreCase),
				Terminal.Production made => Derives(made.Name, token),
				_                        => false,
			});

		return (ends.Contains(cut.Count), furthest, cut.Count);
	}

	/// <summary>The productions the lexical rules reach, in no particular order.</summary>
	public IEnumerable<string> LexicalNames => lexical;

	/// <summary>The named productions a start reaches over tokens that derive nothing at all.</summary>
	public IEnumerable<string> Empty(string start)
	{
		tokens.Start(start);
		return tokens.NullableNames();
	}

	/// <summary>Whether a lexical production derives the whole of a token, its Syntax Rule included.</summary>
	public bool Derives(string name, string text)
	{
		if (derived.TryGetValue((name, text), out var known))
			return known;

		var chars     = text.ToCharArray();
		var (ends, _) = Earley.Ends(characters, characters.Start(name), chars, 0, CharacterMatches, Accept(chars));
		var answer    = ends.Contains(text.Length);

		derived[(name, text)] = answer;
		return answer;
	}

	int Longest(string name, char[] text, int from)
	{
		var (ends, _) = Earley.Ends(characters, characters.Start(name), text, from, CharacterMatches, Accept(text));

		return ends.Count == 0 ? from : ends.Max();
	}

	/// <summary>§5.4's Syntax Rule, wherever a <c>&lt;regular identifier&gt;</c> is completed: it is not a reserved word.</summary>
	Func<int, int, int, bool> Accept(char[] text) =>
		(made, from, to) => made != regular || !reserved.Contains(new string(text, from, to - from));

	bool CharacterMatches(int terminal, char one) => characters.Terminal(terminal) switch
	{
		Terminal.Character c => char.ToUpperInvariant(c.One) == char.ToUpperInvariant(one),
		Terminal.Class k     => k.Test(one),
		_                    => false,
	};

	/// <summary>The one-character predicates the lexical <c>!!</c> productions stand for.</summary>
	static Func<char, bool>? Character(string name) => name switch
	{
		"space"                       => static one => one == ' ',
		"identifier start"            => static one => char.IsLetter(one) || CharUnicodeInfo.GetUnicodeCategory(one) == UnicodeCategory.LetterNumber,
		"identifier extend"           => static one => one == '·' || CharUnicodeInfo.GetUnicodeCategory(one) is
			UnicodeCategory.NonSpacingMark or UnicodeCategory.SpacingCombiningMark or UnicodeCategory.DecimalDigitNumber or
			UnicodeCategory.ConnectorPunctuation or UnicodeCategory.Format,
		"Unicode escape character"    => static one => one == '\\',
		"non-double quote character"  => static one => one != '"',
		"non-quote character"         => static one => one != '\'',
		"whitespace"                  => static one => char.IsWhiteSpace(one),
		"truncating whitespace"       => static one => char.IsWhiteSpace(one),
		"newline"                     => static one => one == '\n',
		_                             => null,
	};

	/// <summary>The productions a text's tokens are made of, as this recognizer draws the line.</summary>
	public static HashSet<string> LexicalProductions(Dictionary<string, BnfNode> rules) => Lexical(rules);

	/// <summary>Every production the lexical rules reach from a token and a separator.</summary>
	/// <remarks>
	/// Not through <c>&lt;character set specification&gt;</c>, which a character string literal may be
	/// introduced by: it names a character set the way the syntactic productions do, in parts a
	/// period divides, and taken as lexical it would make <c>&lt;identifier&gt;</c> and
	/// <c>&lt;schema name&gt;</c> one token each. Written by hand, this, and not the BNF's.
	/// </remarks>
	static HashSet<string> Lexical(Dictionary<string, BnfNode> rules)
	{
		var seen = new HashSet<string>(StringComparer.Ordinal);
		var todo = new Stack<string>(["token", "separator"]);

		while (todo.Count > 0)
		{
			var name = todo.Pop();

			if (name == "character set specification" || !rules.TryGetValue(name, out var body) || !seen.Add(name))
				continue;

			foreach (var used in Used(body))
				todo.Push(used);
		}

		return seen;
	}

	/// <summary>
	/// What a syntactic production reads as one token: the lexical productions, those that are one
	/// character, and those spelled of such characters alone — <c>&lt;SQL language identifier&gt;</c>
	/// in <c>CHARACTER SET LATIN1</c> — but not one that names another production of several tokens.
	/// </summary>
	static HashSet<string> Spelled(Dictionary<string, BnfNode> rules, HashSet<string> lexical)
	{
		var single = new HashSet<string>(StringComparer.Ordinal);

		for (var changed = true; changed;)
		{
			changed = false;

			foreach (var (name, body) in rules)
			{
				if (!single.Contains(name) && OneCharacter(name, body, single))
				{
					single.Add(name);
					changed = true;
				}
			}
		}

		var spelled = new HashSet<string>(single, StringComparer.Ordinal);

		for (var changed = true; changed;)
		{
			changed = false;

			foreach (var (name, body) in rules)
			{
				if (!spelled.Contains(name) && Characters(body, spelled))
				{
					spelled.Add(name);
					changed = true;
				}
			}
		}

		spelled.UnionWith(lexical);
		return spelled;
	}

	static bool OneCharacter(string name, BnfNode node, HashSet<string> single) => node switch
	{
		BnfWord word     => word.Text.Length == 1,
		BnfRule rule     => single.Contains(rule.Name),
		BnfChoice choice => choice.Options.All(option => OneCharacter(name, option, single)),
		BnfText          => Character(name) is not null,
		_                => false,
	};

	static bool Characters(BnfNode node, HashSet<string> spelled) => node switch
	{
		BnfWord word         => word.Text.Length == 1,
		BnfRule rule         => spelled.Contains(rule.Name),
		BnfChoice choice     => choice.Options.All(option => Characters(option, spelled)),
		BnfSequence sequence => sequence.Items.All(item => Characters(item, spelled)),
		BnfOptional optional => Characters(optional.Body, spelled),
		BnfRepeated repeated => Characters(repeated.Body, spelled),
		_                    => false,
	};

	static IEnumerable<string> Used(BnfNode node) => node switch
	{
		BnfRule rule          => [rule.Name],
		BnfSequence sequence  => sequence.Items.SelectMany(Used),
		BnfChoice choice      => choice.Options.SelectMany(Used),
		BnfOptional optional  => Used(optional.Body),
		BnfRepeated repeated  => Used(repeated.Body),
		_                     => [],
	};

	static IEnumerable<string> Words(BnfNode node) => node switch
	{
		BnfWord word         => [word.Text],
		BnfChoice choice     => choice.Options.SelectMany(Words),
		BnfSequence sequence => sequence.Items.SelectMany(Words),
		_                    => [],
	};

	/// <summary>A terminal of a compiled grammar.</summary>
	abstract record Terminal
	{
		public sealed record Character(char One) : Terminal;
		public sealed record Class(Func<char, bool> Test) : Terminal;
		public sealed record Word(string Text) : Terminal;
		public sealed record Production(string Name) : Terminal;
		public sealed record Never : Terminal;
	}

	/// <summary>
	/// The BNF as plain productions, compiled for one level: over characters, where a word is its
	/// characters and a <c>!!</c> its predicate; or over tokens, where a word is a key word and a
	/// lexical production a terminal.
	/// </summary>
	sealed class Grammar : Earley.IGrammar
	{
		readonly Dictionary<string, BnfNode> rules;
		readonly Func<string, bool> terminal;
		readonly Func<string, Func<char, bool>?>? character;
		readonly Dictionary<string, int> named = new(StringComparer.Ordinal);
		readonly List<Terminal> terminals = [];
		readonly List<int[]> right = [];
		readonly List<int> left = [];
		readonly List<List<int>> byLeft = [];
		readonly List<bool> nullable = [];
		bool settled;

		public Grammar(Dictionary<string, BnfNode> rules, Func<string, bool> terminal, Func<string, Func<char, bool>?>? character)
		{
			this.rules     = rules;
			this.terminal  = terminal;
			this.character = character;
		}

		public int Start(string name)
		{
			var id = Nonterminal(name);
			Settle();
			return id;
		}

		public Terminal Terminal(int id) => terminals[id];

		public int Count => right.Count;

		public int[] Right(int production) => right[production];

		public int Left(int production) => left[production];

		public IReadOnlyList<int> Of(int nonterminal) => byLeft[nonterminal];

		public bool Nullable(int nonterminal) => nullable[nonterminal];

		public IEnumerable<string> NullableNames() =>
			named.Where(pair => nullable[pair.Value]).Select(static pair => pair.Key).OrderBy(static name => name, StringComparer.Ordinal);

		/// <summary>A symbol: a nonterminal is its index, a terminal the complement of its index.</summary>
		int Nonterminal(string name)
		{
			if (named.TryGetValue(name, out var id))
				return id;

			id = Fresh();
			named[name] = id;

			if (!rules.TryGetValue(name, out var body))
			{
				Produce(id, [~Add(new Terminal.Never())]);
			}
			else if (body is BnfText)
			{
				var test = character?.Invoke(name);
				Produce(id, [~Add(test is null ? new Terminal.Never() : new Terminal.Class(test))]);
			}
			else
			{
				foreach (var alternative in Alternatives(body))
					Produce(id, alternative);
			}

			settled = false;
			return id;
		}

		int Fresh()
		{
			byLeft.Add([]);
			nullable.Add(false);
			return byLeft.Count - 1;
		}

		int Add(Terminal one)
		{
			terminals.Add(one);
			return terminals.Count - 1;
		}

		void Produce(int id, int[] symbols)
		{
			right.Add(symbols);
			left.Add(id);
			byLeft[id].Add(right.Count - 1);
		}

		IEnumerable<int[]> Alternatives(BnfNode node) =>
			node is BnfChoice choice ? choice.Options.Select(Symbols) : [Symbols(node)];

		int[] Symbols(BnfNode node) => node switch
		{
			BnfSequence sequence => sequence.Items.SelectMany(One).ToArray(),
			_                    => One(node).ToArray(),
		};

		IEnumerable<int> One(BnfNode node)
		{
			switch (node)
			{
				case BnfRule rule when terminal(rule.Name):
					yield return ~Add(new Terminal.Production(rule.Name));
					break;

				case BnfRule rule:
					yield return Nonterminal(rule.Name);
					break;

				case BnfWord word when character is not null:
					foreach (var one in word.Text)
						yield return ~Add(new Terminal.Character(one));
					break;

				case BnfWord word:
					yield return ~Add(new Terminal.Word(word.Text));
					break;

				case BnfText:
					break;

				case BnfOptional optional:
				{
					var id = Fresh();
					Produce(id, []);
					foreach (var alternative in Alternatives(optional.Body))
						Produce(id, alternative);
					yield return id;
					break;
				}

				case BnfRepeated repeated:
				{
					var id = Fresh();
					foreach (var alternative in Alternatives(repeated.Body))
					{
						Produce(id, alternative);
						Produce(id, [id, .. alternative]);
					}
					yield return id;
					break;
				}

				case BnfChoice or BnfSequence:
				{
					var id = Fresh();
					foreach (var alternative in Alternatives(node))
						Produce(id, alternative);
					yield return id;
					break;
				}
			}
		}

		/// <summary>Which nonterminals derive nothing, to a fixed point, whenever productions were added.</summary>
		void Settle()
		{
			if (settled)
				return;

			for (var changed = true; changed;)
			{
				changed = false;

				for (var production = 0; production < right.Count; production++)
				{
					if (nullable[left[production]])
						continue;

					if (Array.TrueForAll(right[production], symbol => symbol >= 0 && nullable[symbol]))
					{
						nullable[left[production]] = true;
						changed = true;
					}
				}
			}

			settled = true;
		}
	}
}

/// <summary>Earley's recognizer, over any input whose terminals a predicate matches.</summary>
static class Earley
{
	/// <summary>A grammar of numbered productions: a nonterminal is a number, a terminal its complement.</summary>
	public interface IGrammar
	{
		int[] Right(int production);

		int Left(int production);

		IReadOnlyList<int> Of(int nonterminal);

		bool Nullable(int nonterminal);
	}

	/// <summary>
	/// Every position the start symbol, begun at <paramref name="from"/>, can end at, and the
	/// furthest position any item reached.
	/// </summary>
	public static (HashSet<int> Ends, int Furthest) Ends<T>(
		IGrammar grammar, int start, IReadOnlyList<T> input, int from, Func<int, T, bool> matches,
		Func<int, int, int, bool>? accept = null)
	{
		var ends     = new HashSet<int>();
		var sets     = new List<List<(int Production, int Dot, int Origin)>>();
		var seen     = new List<HashSet<(int, int, int)>>();
		var furthest = from;

		void Add(int at, (int, int, int) item)
		{
			while (sets.Count <= at - from)
			{
				sets.Add([]);
				seen.Add([]);
			}

			if (seen[at - from].Add(item))
				sets[at - from].Add(item);
		}

		foreach (var production in grammar.Of(start))
			Add(from, (production, 0, from));

		for (var at = from; at - from < sets.Count; at++)
		{
			var set = sets[at - from];

			if (set.Count > 0)
				furthest = at;

			for (var i = 0; i < set.Count; i++)
			{
				var (production, dot, origin) = set[i];
				var right = grammar.Right(production);

				if (dot < right.Length)
				{
					var symbol = right[dot];

					if (symbol >= 0)
					{
						foreach (var next in grammar.Of(symbol))
							Add(at, (next, 0, at));

						if (grammar.Nullable(symbol))
							Add(at, (production, dot + 1, origin));
					}
					else if (at < input.Count && matches(~symbol, input[at]))
					{
						Add(at + 1, (production, dot + 1, origin));
					}
				}
				else
				{
					var made = grammar.Left(production);

					// What a Syntax Rule refuses of a completed production is not completed.
					if (accept is not null && !accept(made, origin, at))
						continue;

					if (made == start && origin == from)
						ends.Add(at);

					var waiting = sets[origin - from];

					for (var j = 0; j < waiting.Count; j++)
					{
						var (other, otherDot, otherOrigin) = waiting[j];
						var otherRight = grammar.Right(other);

						if (otherDot < otherRight.Length && otherRight[otherDot] == made)
							Add(at, (other, otherDot + 1, otherOrigin));
					}
				}
			}
		}

		return (ends, furthest);
	}
}
