using System;
using System.Globalization;
using System.Text;

using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

public static partial class CSharpEmitter
{
	/// <summary>
	/// The lexer's scan of a grammar that looks its keywords up: the automaton of everything else, the
	/// table, and the <c>Scan</c> that reads a word and asks it.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Longest match is kept. The automaton says what it reads from the token's start; the word is the
	/// run of characters the boundary names; a keyword is the word where it is in the table and no token
	/// the automaton read is longer. Where the automaton read a token that is made of word characters it
	/// has already measured the word, and the run is only continued from its end.
	/// </para>
	/// <para>
	/// The keys are bucketed by length and by the first character, and a word is compared whole, character
	/// by character, against the few keys of its bucket. Keys are kept sorted so that a text
	/// that ends inside a word can be asked whether some keyword begins with it, which the automaton of
	/// everything else cannot say.
	/// </para>
	/// </remarks>
	static string KeywordScan(TerminalInventory inventory, KeywordTable table, bool utf8Literals)
	{
		var file    = new Writer(0);
		var keys    = table.Keys;
		var fold    = table.IgnoreCase;
		var longest = table.Longest;
		// Keys by length and by the low five bits of the first character (upper and lower case agree there),
		// the keys of a bucket together: the scan asks the bucket of the word and compares the few in it.
		var buckets = (longest + 1) * 32;
		var starts  = new int[buckets + 1];
		var order   = keys.Select((one, at) => (one, at)).OrderBy(static pair => pair.one.Length * 32 + (pair.one[0] & 31)).ThenBy(static pair => pair.at).Select(static pair => pair.at).ToArray();

		foreach (var one in keys)
			starts[one.Length * 32 + (one[0] & 31) + 1]++;

		for (var at = 0; at < buckets; at++)
			starts[at + 1] += starts[at];

		var contexts = table.Contexts.Count;
		var kinds    = new int[inventory.Kinds.Count + 1];

		for (var at = 0; at < kinds.Length; at++)
			kinds[at] = 127;

		for (var at = 0; at < contexts; at++)
			kinds[table.Contexts[at]] = at;

		file.Write(LexerEmitter.Emit(table.Machine, "Words", utf8Literals, table.Numbers));
		file.Line();

		WordGoesOn(file, "Keys", table.Boundary);

		file.Line("/// <summary>The keywords, as they are compared, in ordinal order.</summary>");
		KeywordArray(file, "string", "Words_Keys", keys.Select(Quoted));
		file.Line();
		file.Line("/// <summary>By length and first character, where the keys of that bucket begin in <c>Words_Order</c>.</summary>");
		KeywordNumbers(file, "ushort", "Words_Starts", starts);
		file.Line("/// <summary>The keys, grouped by length and first character.</summary>");
		KeywordNumbers(file, keys.Count < ushort.MaxValue ? "ushort" : "int", "Words_Order", order);
		file.Line("/// <summary>By key and context, the kind a keyword is: with what the rest of the patterns said of it.</summary>");
		KeywordNumbers(file, "int", "Words_Kinds", table.Kinds);
		file.Line("/// <summary>By key, the kind it is without asking the automaton, or 0 where it must be asked.</summary>");
		KeywordNumbers(file, "int", "Words_Direct", table.Direct);
		file.Line("/// <summary>By kind: which context it is, 127 for none.</summary>");
		KeywordNumbers(file, "byte", "Words_Of", kinds);

		var compare = fold ? "Words_Fold(text[pos + i])" : "text[pos + i]";

		if (fold)
		{
			file.Line("static char Words_Fold(char c)");

			using (file.Block(""))
				file.Line("return c >= 'a' && c <= 'z' ? (char)(c - 32) : c;");

			file.Line();
		}

		file.Line("/// <summary>Which key the word of the given length at <paramref name=\"pos\"/> is, or -1.</summary>");
		file.Line("static int Words_Find(global::System.ReadOnlySpan<char> text, int pos, int length)");

		using (file.Block(""))
		{
			file.Line("var bucket = length * 32 + (text[pos] & 31);");
			file.Line();

			using (file.Block("for (var at = (int)Words_Starts[bucket]; at < Words_Starts[bucket + 1]; at++)"))
			{
				file.Line("var key = Words_Order[at];");
				file.Line();
				file.Line("if (Words_Same(text, pos, Words_Keys[key]))");
				file.Then("return key;");
			}

			file.Line();
			file.Line("return -1;");
		}

		file.Line();
		file.Line("static bool Words_Same(global::System.ReadOnlySpan<char> text, int pos, string key)");

		using (file.Block(""))
		{
			using (file.Block("for (var i = 0; i < key.Length; i++)"))
			{
				file.Line($"if ({compare} != key[i])");
				file.Then("return false;");
			}

			file.Line();
			file.Line("return true;");
		}

		file.Line();
		file.Line("/// <summary>Whether a keyword is longer than, and begins with, the word at <paramref name=\"pos\"/>.</summary>");
		file.Line("static bool Words_Begin(global::System.ReadOnlySpan<char> text, int pos, int length)");

		using (file.Block(""))
		{
			file.Line("var low  = 0;");
			file.Line("var high = Words_Keys.Length;");
			file.Line();

			using (file.Block("while (low < high)"))
			{
				file.Line("var middle = (low + high) >> 1;");
				file.Line();
				file.Line("if (Words_Compare(text, pos, length, Words_Keys[middle]) < 0)");
				file.Then("low = middle + 1;");
				file.Line("else");
				file.Then("high = middle;");
			}

			file.Line();
			file.Line("return low < Words_Keys.Length && Words_Keys[low].Length > length && Words_Compare(text, pos, length, Words_Keys[low]) == 0;");
		}

		file.Line();
		file.Line("/// <summary>Whether the key sorts before (-1), as (0) or after (1) the word, comparing the key's first characters.</summary>");
		file.Line("static int Words_Compare(global::System.ReadOnlySpan<char> text, int pos, int length, string key)");

		using (file.Block(""))
		{
			using (file.Block("for (var i = 0; i < length; i++)"))
			{
				file.Line("if (i == key.Length)");
				file.Then("return -1;");
				file.Line();
				file.Line($"var c = {compare};");
				file.Line();
				file.Line("if (key[i] != c)");
				file.Then("return key[i] < c ? -1 : 1;");
			}

			file.Line();
			file.Line("return 0;");
		}

		file.Line();
		file.Line("/// <summary>");
		file.Line("/// One token: its kind by way of <paramref name=\"kind\"/>, and where it ends.");
		file.Line("/// </summary>");
		file.Line("/// <remarks>");
		file.Line("/// The word is read first. A keyword whose kind does not depend on what the");
		file.Line("/// rest of the patterns say of it is the token as it stands; otherwise the automaton of the rest");
		file.Line("/// reads the token, and a keyword no shorter than that is the keyword, as longest match would have it.");
		file.Line("/// </remarks>");
		file.Line("/// <returns>Where the token ends, or <paramref name=\"pos\"/> where none begins here.</returns>");
		file.Line("public static int Scan(global::System.ReadOnlySpan<char> text, int pos, out int kind)");

		using (file.Block(""))
		{
			file.Line("var run  = pos;");
			file.Line();

			using (file.Block($"while (run < text.Length && run - pos <= {longest} && WordGoesOn_DotGramKeys(text[run]))"))
			{
				file.Line("run++;");
			}

			file.Line();
			file.Line("var key = -1;");
			file.Line();
			file.Line($"if (run - pos >= {table.Shortest} && run - pos <= {longest})");

			using (file.Block(""))
			{
				file.Line("key = Words_Find(text, pos, run - pos);");
				file.Line();
				file.Line("if (key >= 0 && Words_Direct[key] != 0)");

				using (file.Block(""))
				{
					file.Line("kind = Words_Direct[key];");
					file.Line();
					file.Line("return run;");
				}
			}

			file.Line();
			file.Line("var end = ScanWords(text, pos, out kind);");
			file.Line();
			file.Line("if (key >= 0 && run >= end)");

			using (file.Block(""))
			{
				file.Line("var context = Words_Of[run == end ? kind : 0] & 127;");
				file.Line();
				file.Line($"if (context < {contexts} && Words_Kinds[key * {contexts} + context] != 0)");

				using (file.Block(""))
				{
					file.Line($"kind = Words_Kinds[key * {contexts} + context];");
					file.Line();
					file.Line("return run;");
				}
			}

			file.Line();
			file.Line("return end;");
		}

		file.Line();
		file.Line("static bool Scan_RunsOut(global::System.ReadOnlySpan<char> text, int pos)");

		using (file.Block(""))
		{
			file.Line("if (ScanWords_RunsOut(text, pos))");
			file.Then("return true;");
			file.Line();
			file.Line("var run = pos;");
			file.Line();
			file.Line($"while (run < text.Length && run - pos < {longest} && WordGoesOn_DotGramKeys(text[run]))");
			file.Then("run++;");
			file.Line();
			file.Line("return run == text.Length && run > pos && Words_Begin(text, pos, run - pos);");
		}

		return file.ToString();
	}

	static void KeywordNumbers(Writer file, string type, string name, IReadOnlyList<int> values)
	{
		KeywordArray(file, type, name, values.Select(static one => one.ToString(CultureInfo.InvariantCulture)));
		file.Line();
	}

	/// <summary>A static array written a few dozen characters to a row.</summary>
	static void KeywordArray(Writer file, string type, string name, IEnumerable<string> cells)
	{
		file.Line($"static readonly {type}[] {name} =");
		file.Line("{");

		var row = new StringBuilder();

		foreach (var cell in cells)
		{
			row.Append(cell).Append(", ");

			if (row.Length < 92)
				continue;

			file.Then(row.ToString().TrimEnd());
			row.Clear();
		}

		if (row.Length > 0)
			file.Then(row.ToString().TrimEnd());

		file.Line("};");
	}
}
