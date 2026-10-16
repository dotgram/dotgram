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
	/// The table is open addressing over a pick of the word (its length and three of its characters),
	/// compared against the key as written: a pick that agrees by chance costs a compare, never a wrong
	/// answer. Keys are kept sorted so that a text
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
		// Open addressing over a pick of the word: its length and three of its characters, which a word is
		// told apart by far more often than not. Nothing is hashed along the word; the key in the slot is compared whole.
		var bits = 1;

		while ((1 << bits) < keys.Count * 3)
			bits++;

		var slots = new int[1 << bits];

		foreach (var (key, at) in keys.Select((one, at) => (one, at)))
		{
			var slot = KeywordSlot(key, bits);

			while (slots[slot] != 0)
				slot = (slot + 1) & (slots.Length - 1);

			slots[slot] = at + 1;
		}

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
		file.Line("/// <summary>Where a key is by the pick of its word, one-based; 0 where there is none.</summary>");
		KeywordNumbers(file, keys.Count < ushort.MaxValue ? "ushort" : "int", "Words_Slots", slots);
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
			file.Line("var pick = (uint)length;");
			file.Line();
			file.Line("pick = pick * 31 + (uint)(text[pos] & 0xDF);");
			file.Line("pick = pick * 31 + (uint)(text[pos + (length >> 1)] & 0xDF);");
			file.Line("pick = pick * 31 + (uint)(text[pos + length - 1] & 0xDF);");
			file.Line();
			file.Line($"var slot = (int)((pick * 2654435761u) >> {32 - bits});");
			file.Line();

			using (file.Block("while (Words_Slots[slot] != 0)"))
			{
				file.Line("var key = Words_Slots[slot] - 1;");
				file.Line();
				file.Line("if (Words_Keys[key].Length == length && Words_Same(text, pos, Words_Keys[key]))");
				file.Then("return key;");
				file.Line();
				file.Line($"slot = (slot + 1) & {(1 << bits) - 1};");
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
		file.Line("/// The automaton of the rest reads the token first, and the word is read on from where that token");
		file.Line("/// ended: a word that is a keyword is the whole of the token it makes, or longer. A keyword whose kind");
		file.Line("/// does not depend on what the rest of the patterns say of it is the token as it stands; otherwise the");
		file.Line("/// kind is taken by the keyword and by that, as longest match would have it.");
		file.Line("/// </remarks>");
		file.Line("/// <returns>Where the token ends, or <paramref name=\"pos\"/> where none begins here.</returns>");
		file.Line("public static int Scan(global::System.ReadOnlySpan<char> text, int pos, out int kind)");

		using (file.Block(""))
		{
			file.Line("var end = ScanWords(text, pos, out kind);");
			file.Line();
			file.Line("if (pos >= text.Length || !WordGoesOn_DotGramKeys(text[pos]))");
			file.Then("return end;");
			file.Line();
			file.Line("var run = end;");
			file.Line();
			file.Line($"while (run < text.Length && run - pos <= {longest} && WordGoesOn_DotGramKeys(text[run]))");
			file.Then("run++;");
			file.Line();
			file.Line($"if (run - pos < {table.Shortest} || run - pos > {longest})");
			file.Then("return end;");
			file.Line();
			file.Line("var key = Words_Find(text, pos, run - pos);");
			file.Line();
			file.Line("if (key < 0)");
			file.Then("return end;");
			file.Line();
			file.Line("if (Words_Direct[key] != 0)");

			using (file.Block(""))
			{
				file.Line("kind = Words_Direct[key];");
				file.Line();
				file.Line("return run;");
			}

			file.Line();
			file.Line("var context = Words_Of[run == end ? kind : 0] & 127;");
			file.Line();
			file.Line($"if (context < {contexts} && Words_Kinds[key * {contexts} + context] != 0)");

			using (file.Block(""))
			{
				file.Line($"kind = Words_Kinds[key * {contexts} + context];");
				file.Line();
				file.Line("return run;");
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

	/// <summary>The slot a key is placed in; the scan picks the same of the word.</summary>
	static int KeywordSlot(string key, int bits)
	{
		var pick = (uint)key.Length;

		pick = pick * 31 + (uint)(key[0] & 0xDF);
		pick = pick * 31 + (uint)(key[key.Length >> 1] & 0xDF);
		pick = pick * 31 + (uint)(key[^1] & 0xDF);

		return (int)((pick * 2654435761u) >> (32 - bits));
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
