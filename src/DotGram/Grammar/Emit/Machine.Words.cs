using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Plan;

namespace DotGram.Grammar.Emit;

/// <summary>
/// A choice of words, tried through a table of them: <c>("add"i | "alter"i | … ) &amp; ?!WordChar</c>,
/// and the same list in a namespace with a <c>wordboundary</c>, where every word carries the checks
/// of its own (§4.6).
/// </summary>
/// <remarks>
/// <para>
/// Dispatched on its first character, such a choice was a group of words for each, and every word
/// of a group a method of its own, or a block in the method that reads the choice where it was
/// alone; a list of a few hundred keywords was a method that held them all, and its first call
/// compiled them whatever word stood in the input.
/// </para>
/// <para>
/// The switch on the first character stays. A group of words is then where its words begin in a
/// string the table is written as, and one loop tries them in the order the alternatives name
/// them, each as its literal was tried: compared, refused where it stops agreeing and named as the
/// literal names itself, the input marked as run out where it ends inside the word, and the look
/// after it asked. Where the group was read with a way to come back to, the loop opens the same
/// way and moves it as the alternatives did, so that what follows the choice comes back to the
/// next word as it came back to the next alternative; and where it was read without one, it
/// opens none. What the reading answers, what it says and in what order is what the alternatives
/// answered and said: WordChoiceTests holds the one to the other.
/// </para>
/// <para>
/// A group with anything else in it — a word with a run after it, an alternative of another
/// shape, a run of texts the reader refuses together — is written as it was.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>
	/// The fewest words a choice is read through a table for: below it, the alternatives are as cheap.
	/// </summary>
	internal const int LeastWords = 4;

	/// <summary>
	/// The method that tries one word of a table (<see cref="WordReader"/>).
	/// </summary>
	string? _wordReader;

	/// <summary>
	/// The tables written so far, by what they hold, so that two choices of the same words share one.
	/// </summary>
	readonly Dictionary<string, string> _wordTables = new(StringComparer.Ordinal);

	/// <summary>
	/// A choice whose groups of words are tried through a table.
	/// </summary>
	/// <param name="Table">The constant the words are written in.</param>
	/// <param name="Groups">For each group of the dispatch, where its words begin in the table; -1 where it is written as it was.</param>
	/// <param name="Ways">Whether some group of the table is read with a way to come back to.</param>
	/// <param name="Fold">Whether the words ignore case.</param>
	/// <param name="Continues">The characters the look after a word refuses to see next.</param>
	/// <param name="Behind">The look behind every word (§4.6), and the set its refusal names; null where the words carry none.</param>
	/// <param name="Before">
	/// Whether the look stands after the choice, <c>(…) &amp; ?!WordChar</c>, rather than in every
	/// alternative: what refuses a word there refuses the choice, unless a way comes back to it.
	/// </param>
	internal sealed record WordPlan(
		string Table, int[] Groups, bool Ways, bool Fold, Node.Element Continues, (Node.Element Test, string Set)? Behind, bool Before);

	/// <summary>
	/// The word an alternative is: the literal, where it is one of the table's.
	/// </summary>
	/// <param name="look">The look after the choice, where the words are bare literals before it; null where each alternative carries its own checks.</param>
	static Node.Literal? WordOf(Node alternative, Node.Lookahead? look)
	{
		return look is not null
			? alternative as Node.Literal
			: alternative is Node.Sequence { Nodes: [Node.Behind, Node.Literal literal, Node.Lookahead { IsPositive: false }] } ? literal : null;
	}

	/// <summary>
	/// Whether a literal can be a word of a table: not empty, and ASCII, so that the case it ignores
	/// is the one the table folds.
	/// </summary>
	static bool Tabled(Node.Literal literal, bool fold)
	{
		return literal.Text.Length is > 0 and < 0xFFFF && literal.IgnoreCase == fold && literal.Text.All(static one => one <= 0x7F);
	}

	/// <summary>
	/// The element a look comes down to through calls of rules that are nothing else, where it does.
	/// </summary>
	Node.Element? ElementUnder(Node node)
	{
		var seen = new HashSet<RuleSymbol>();

		while (true)
		{
			switch (node)
			{
				case Node.Element element:
					return element;

				case Node.Call(var rule, { Count: 0 }) when seen.Add(rule) && !_graph.Types.ContainsKey(rule) && _graph.Bodies.TryGetValue(rule, out var body):
					node = body;
					break;

				default:
					return null;
			}
		}
	}

	static bool SameCharacters(Node.Element one, Node.Element other)
	{
		if (ReferenceEquals(one, other))
			return true;

		var first  = FirstSets.OfElement(one);
		var second = FirstSets.OfElement(other);

		return first.Anything == second.Anything && first.Nothing == second.Nothing && first.Ranges.SequenceEqual(second.Ranges);
	}

	/// <summary>
	/// The constant a table is written in, written once for the machine and shared by every choice
	/// whose table holds the same.
	/// </summary>
	string WordTable(string words)
	{
		if (_wordTables.TryGetValue(words, out var name))
			return name;

		name = Tag.Named("Words", _wordTables.Count);

		_extra.Add($"const string {name} = {Escaped(words)};");
		_wordTables.Add(words, name);

		return name;
	}

	/// <summary>
	/// A string literal of any characters, everything outside printable ASCII written as an escape.
	/// </summary>
	static string Escaped(string value)
	{
		var text = new StringBuilder(value.Length * 2 + 2).Append('"');

		foreach (var one in value)
		{
			if (one is '"' or '\\')
				text.Append('\\').Append(one);
			else if (one is >= ' ' and <= '~')
				text.Append(one);
			else
				text.Append("\\u").Append(((int)one).ToString("X4", CultureInfo.InvariantCulture));
		}

		return text.Append('"').ToString();
	}

	/// <summary>
	/// The method that tries the word of a table at <c>at</c> from <c>p</c>, as a literal is tried:
	/// where the input from <c>p</c> is the word, the position after it; where it is not, -1, the
	/// refusal recorded where the literal records it — at the character it stops agreeing with, where
	/// the reading moves there, or the input marked as run out where it ends inside the word.
	/// </summary>
	/// <remarks>
	/// Each word is its length, the number of the set its refusal names, and its characters, lower
	/// case where case is ignored; an upper-case ASCII letter of the input is folded before it is
	/// compared, which is the comparison an ASCII literal makes, and the length it agrees for is the
	/// one RecognizeAgreeing_DotGram walks.
	/// </remarks>
	string WordReader()
	{
		if (_wordReader is not null)
			return _wordReader;

		_wordReader = Tag.Named("RecognizeWord");

		var record = KeepsDoors
			? $"{ExpectedSets.Refused}(ref failure, at, id);"
			: Quiets
			? $"if (!failure.Quiet) {ExpectedSets.Refuse}(ref failure, at, id);"
			: $"{ExpectedSets.Refuse}(ref failure, at, id);";

		if (KeepsDoors)
			_expectedSets.CallsRefused = true;
		else
			_expectedSets.CallsRefuse = true;

		var helper = new Writer(0);

		helper.Line(
			$"static int {_wordReader}({InputType} text, int p, string words, int word, bool fold, ref {CSharpEmitter.FailureType} failure)");

		using (helper.Block(""))
		{
			helper.Line("var length = (int)words[word];");
			helper.Line("var id     = (int)words[word + 1];");
			helper.Line("var agrees = 0;");
			helper.Line();

			using (helper.Block("while (agrees < length && (uint)(p + agrees) < (uint)text.Length)"))
			{
				helper.Line("var c = text[p + agrees];");
				helper.Line();
				helper.Line("if (fold && (uint)(c - 'A') <= 'Z' - 'A')");
				helper.Then("c = (char)(c | 0x20);");
				helper.Line();
				helper.Line("if (c != words[word + 2 + agrees])");
				helper.Then("break;");
				helper.Line();
				helper.Line("agrees++;");
			}

			helper.Line();
			helper.Line("if (agrees == length)");
			helper.Then("return p + length;");
			helper.Line();
			helper.Line("// Where it stops agreeing, as far as the last character of the word.");
			helper.Line("var at = p + (agrees < length - 1 ? agrees : length - 1);");
			helper.Line();
			helper.Line("if ((uint)at >= (uint)text.Length && p + 1 > failure.OutOfInput)");
			helper.Then("failure.OutOfInput = p + 1;");
			helper.Line();
			// The quiet reading stays where the word began, as a literal's refusal does (Sharpened, Reached).
			if (Quiets)
			{
				helper.Line("if (failure.Quiet)");
				helper.Then("at = p;");
				helper.Line();
			}

			helper.Line(record);
			helper.Line();
			helper.Line("return -1;");
		}

		_extra.Add(helper.ToString());

		return _wordReader;
	}

	sealed partial class ReaderWriter
	{
		/// <summary>
		/// The look after the choice the sequence is about to write, where one follows it (EmitChoice takes it).
		/// </summary>
		Node.Lookahead? _wordLook;

		/// <summary>
		/// Which groups of a dispatched choice are tried through a table of words, and how each was
		/// read: null where none is, or the choice is not one of words.
		/// </summary>
		/// <param name="look">The look after the choice, where the sequence it stands in has one.</param>
		WordPlan? PlanWords(
			IReadOnlyList<Node> alternatives, List<(FirstSets.First Set, List<Node> Members)> groups, FollowSets.Continuation following,
			Node.Lookahead? look)
		{
			if (!machine._settings.WordTables || analyzing || !_tape || machine.BufferedInput || machine.Windowed || machine.OverKinds ||
				machine.Tracing is not null || Seamed is not null || alternatives.Count < LeastWords)
				return null;

			// The words are literals before a look that refuses what goes on after them, or each a
			// literal with the look behind and after it of its own.
			if (look is not null && alternatives.Count(static one => one is Node.Literal) < LeastWords)
				look = null;

			var first = alternatives.Select(one => WordOf(one, look)).FirstOrDefault(static one => one is not null);

			if (first is null)
				return null;

			var fold = first.IgnoreCase;

			(Node.Element Test, string Set)? behind = null;
			Node.Element? continues;

			if (look is not null)
			{
				if (!Plain(look) || machine.ElementUnder(look.Body) is not { } after)
					return null;

				continues = after;
			}
			else
			{
				continues = null;

				foreach (var alternative in alternatives)
				{
					if (alternative is not Node.Sequence { Nodes: [Node.Behind(var test) before, Node.Literal, Node.Lookahead { IsPositive: false } beyond] } ||
						!Plain(beyond) || machine.ElementUnder(beyond.Body) is not { } after)
						return null;

					var set = Expected(before, 0, () => new TerminalSite { Items = Said(machine.ReaderDisplays(before)) });

					if (continues is null)
					{
						behind    = (test, set);
						continues = after;
					}
					else if (!SameCharacters(behind!.Value.Test, test) || behind.Value.Set != set || !SameCharacters(continues, after))
						return null;
				}
			}

			if (FirstSets.OfElement(continues!) is { Anything: true } or { Nothing: true })
				return null;

			var table   = new StringBuilder();
			var offsets = new int[groups.Count];
			var ways    = false;
			var tabled  = 0;

			for (var g = 0; g < groups.Count; g++)
			{
				var members = groups[g].Members;
				var mode    = Mode(members, groups[g].Set, following, look, fold);

				offsets[g] = -1;

				// The word count is stored in one char; a larger group keeps the plain emission.
				if (mode is null || members.Count > 0xFFFF)
					continue;

				offsets[g] = table.Length;
				ways      |= mode.Value;
				tabled    += members.Count;

				table.Append((char)members.Count).Append(mode.Value ? '\u0001' : '\0');

				foreach (var member in members)
				{
					var literal = WordOf(member, look)!;
					var set     = Expected(literal, 0, () => new TerminalSite { Items = Said(machine.ReaderDisplays(literal)) });

					machine._expectedUsed.Add(set);

					// The table holds the number of the set in one character.
					if (!ExpectedSets.TryId(set, out var id) || id > 0xFFFF)
						return null;

					var compared = fold ? Folded(literal.Text) : literal.Text;

					table.Append((char)compared.Length).Append((char)id).Append(compared);
				}
			}

			if (tabled < LeastWords)
				return null;

			return new WordPlan(machine.WordTable(table.ToString()), offsets, ways, fold, continues!, behind, look is not null);
		}

		/// <summary>
		/// Whether a negative look refuses as any look does where it holds: one bit, a refusal that
		/// names nothing where it stands, and nothing said of the end of the input.
		/// </summary>
		bool Plain(Node.Lookahead look)
		{
			return !machine._quietLooks.Contains(look) && !Machine.IsAny(look.Body, machine._graph.Segmented) && !machine.IsSegmentAny(look.Body);
		}

		/// <summary>
		/// How a group was read, where the table can try its words as it was: true with a way to come
		/// back to, false without (one word, or words of which no two can both hold); null where it
		/// holds anything but words, or was read some other way.
		/// </summary>
		/// <remarks>
		/// The questions EmitAmong asks of a group the switch chose, asked the same way.
		/// </remarks>
		bool? Mode(List<Node> members, FirstSets.First set, FollowSets.Continuation following, Node.Lookahead? look, bool fold)
		{
			foreach (var member in members)
				if (WordOf(member, look) is not { } literal || !Tabled(literal, fold) || machine.IsPending(member))
					return null;

			if (members.Count == 1)
				return false;

			var dispatched = _dispatched;

			_dispatched = set;

			var door = Doorway(members) is { } gate && !Chosen(gate);

			_dispatched = dispatched;

			if (door || Closing(members, following) is not null)
				return null;

			return WritesAWay(members, following);
		}

		/// <summary>
		/// The words of the group the switch chose, tried one after another from the table, after the
		/// switch has put where they begin in <paramref name="chosen"/>; nothing where it chose a group
		/// written as it was.
		/// </summary>
		void EmitWords(Writer code, WordPlan words, string chosen)
		{
			var reader = machine.WordReader();
			var number = (_calls++).ToString(CultureInfo.InvariantCulture);
			var count  = "wn" + number;
			var way    = "wy" + number;
			var at     = "wk" + number;
			var word   = "wo" + number;
			var read   = "wr" + number;
			var found  = "wf" + number;
			var skip   = "wj" + number;
			var stops  = "wb" + number;
			var fold   = words.Fold ? "true" : "false";

			using (words.Groups.Contains(-1) ? code.Block($"if ({chosen} >= 0)") : null)
			{
				code.Line($"var {count} = (int){words.Table}[{chosen}];");
				code.Line($"var {at} = 0;");

				// Read with a way, the group opens it where it is first read and, where the reading is
				// a replay, takes the word the way stands at, as the alternatives took theirs.
				if (words.Ways)
				{
					code.Line($"var {way} = -1;");
					code.Line();

					using (code.Block($"if ({words.Table}[{chosen} + 1] != 0)"))
					{
						using (code.Block("if (ways.Cursor < ways.Count)"))
						{
							code.Line($"{way} = ways.Cursor;");
							code.Line($"{at} = ways.Items[{way} * 2];");
							code.Line("ways.Cursor++;");
						}

						code.Line("else");
						code.Then($"{way} = ways.Open(0, {count} - 1);");
					}
				}

				code.Line();
				code.Line($"var {word} = {chosen} + 2;");
				code.Line();
				code.Line($"for (var {skip} = 0; {skip} < {at}; {skip}++)");
				code.Then($"{word} += {words.Table}[{word}] + 2;");
				code.Line();
				code.Line($"var {found} = -1;");
				code.Line();

				using (code.Block($"for (; {at} < {count}; {at}++, {word} += {words.Table}[{word}] + 2)"))
				{
					// Each word as its alternative read it: the look behind, the literal, the look after.
					if (words.Behind is { } behind)
					{
						machine._expectedUsed.Add(behind.Set);

						code.Line($"var {read} = -1;");
						code.Line($"var {stops} = false;");
						code.Line();

						using (code.Block("if (p > 0)"))
						{
							code.Line($"c = {machine.ReadAt("p - 1")};");
							code.Line($"{stops} = {CSharpEmitter.Test(behind.Test, machine.Tabulate)};");
						}

						code.Line();

						using (code.Block($"if ({stops})"))
							code.Line(Refusal(behind.Set));

						code.Line("else");
					}

					using (words.Behind is null ? null : code.Block(""))
					{
						code.Line($"{(words.Behind is null ? "var " : "")}{read} = {reader}(text, p, {words.Table}, {word}, {fold}, ref failure);");
						code.Line();

						using (code.Block($"if ({read} >= 0 && {machine.RawWithin(read)})"))
						{
							code.Line($"c = {machine.ReadAt(read)};");
							code.Line();

							using (code.Block($"if ({CSharpEmitter.Test(words.Continues, machine.Tabulate)})"))
							{
								code.Line(Refusal("null", at: read));

								// A look after the choice refuses the choice, but where a way comes back
								// to the next word.
								if (!words.Before)
									code.Line($"{read} = -1;");
								else if (words.Ways)
								{
									code.Line($"if ({way} < 0)");
									code.Then(Fails());
									code.Line();
									code.Line($"{read} = -1;");
								}
								else
									code.Line(Fails());
							}
						}
					}

					code.Line();

					using (code.Block($"if ({read} >= 0)"))
					{
						code.Line($"{found} = {read};");
						code.Line("break;");
					}

					if (words.Ways)
					{
						code.Line();
						code.Line($"if ({way} >= 0 && {at} < {count} - 1)");
						code.Then($"ways.Next({way}, {at} + 1, {count} - 1);");
					}
				}

				code.Line();
				code.Line($"if ({found} < 0)");
				code.Then(Fails());
				code.Line();
				code.Line($"p = {found};");
			}
		}
	}

	/// <summary>
	/// A word as the table compares it where case is ignored: ASCII letters in lower case.
	/// </summary>
	static string Folded(string text)
	{
		var folded = new StringBuilder(text.Length);

		foreach (var one in text)
			folded.Append(CaseFold.AsciiLetter(one) ?? one);

		return folded.ToString();
	}
}
