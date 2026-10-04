using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DotGram.Grammar.Emit;

/// <summary>
/// Every set of items a refusal of one parser class can name, by number: the items written once
/// for the whole class, and each set as the numbers of its items.
/// </summary>
/// <remarks>
/// <para>
/// A set was a property of its own, a <c>new string[] { … }</c> of string literals built the first
/// time a refusal asked for it. A grammar the size of T-SQL's has thousands of them, hundreds
/// naming every keyword, and they were a large part of what the C# compiler was given to read.
/// Written as numbers in one literal they are one token, and the items they share are written once.
/// </para>
/// <para>
/// A set is still one array for the life of the process, built where a refusal first asks for it
/// and never in the type's initializer (D17): a refusal tells two sets apart by reference
/// (<c>Refuse_DotGram</c> drops a tie it holds already, <c>Refuse_DotGram_Over</c> removes the set a
/// longer one covers), and a trace build's explanation finds a refusal by the same reference.
/// </para>
/// <para>
/// Numbers are given by whoever owns the list a set is looked up in, not here: machines that share
/// a list share a number for the same items, and a machine with a list of its own gets numbers of
/// its own for items another list holds too. So two sets that were separate references stay
/// separate, whichever machines they come from; what every machine of the class shares is the
/// items, which nothing tells apart by reference.
/// </para>
/// </remarks>
sealed class ExpectedSets
{
	/// <summary>The accessor a site calls with a set's number, which a refusal passes on as it is.</summary>
	public const string Accessor = "ExpectedSet_DotGram";

	/// <summary>Each set's items, escaped for a string literal, in the order the set names them.</summary>
	readonly List<IReadOnlyList<string>> _sets = [];

	/// <summary>
	/// The calls something wrote into a state. A set asked for and not written is not built at
	/// run time, and its items are left out of the list unless a written set names them too.
	/// </summary>
	public HashSet<string> Used { get; } = new(StringComparer.Ordinal);

	/// <summary>A new number for <paramref name="items"/>, however many numbers the same items have already.</summary>
	public int Add(IReadOnlyList<string> items)
	{
		_sets.Add(items);

		return _sets.Count - 1;
	}

	/// <summary>The call a site writes for the set numbered <paramref name="id"/>.</summary>
	public static string Call(int id)
	{
		return Accessor + "(" + id.ToString(CultureInfo.InvariantCulture) + ")";
	}

	/// <summary>The number <paramref name="call"/> asks for, where it is a call <see cref="Call"/> wrote.</summary>
	public static bool TryId(string call, out int id)
	{
		id = -1;

		return
			call.StartsWith(Accessor + "(", StringComparison.Ordinal) &&
			call.EndsWith(")", StringComparison.Ordinal) &&
			int.TryParse(
				call.Substring(Accessor.Length + 1, call.Length - Accessor.Length - 2),
				NumberStyles.None,
				CultureInfo.InvariantCulture,
				out id);
	}

	/// <summary>
	/// The accessor and what it reads, for the sets something wrote a call to; empty where nothing
	/// did.
	/// </summary>
	/// <param name="utf8">
	/// Whether the consumer's C# is 11 or later, so that the numbers may be a UTF-8 literal, which is
	/// data in the image. Below it they are an ordinary string, which goes to the heap of user
	/// strings: a T-SQL-sized grammar puts a few hundred kilobytes there, against the sixteen
	/// megabytes the whole assembly has (<see cref="SpelledNumbers"/>).
	/// </param>
	public string Render(bool utf8)
	{
		var count = 0;

		for (var id = 0; id < _sets.Count; id++)
			if (Used.Contains(Call(id)))
				count = id + 1;

		if (count == 0)
			return "";

		// The items in the order the written sets first name them, each once; a set is the
		// numbers of its own items in its own order, never sorted, because that order is the
		// order its message names them in.
		var items   = new List<string>();
		var indices = new Dictionary<string, int>(StringComparer.Ordinal);
		var bodies  = new List<int>();
		var starts  = new int[count];
		var width   = 0;

		for (var id = 0; id < count; id++)
		{
			if (!Used.Contains(Call(id)))
			{
				starts[id] = -1;

				continue;
			}

			starts[id] = width;
			bodies.Add(_sets[id].Count);
			width += SpelledNumbers.Width(_sets[id].Count);

			foreach (var item in _sets[id])
			{
				if (!indices.TryGetValue(item, out var index))
				{
					index = items.Count;
					indices.Add(item, index);
					items.Add(item);
				}

				bodies.Add(index);
				width += SpelledNumbers.Width(index);
			}
		}

		var numbers = new List<int>(starts.Length + bodies.Count);

		numbers.AddRange(starts);
		numbers.AddRange(bodies);

		var size = count.ToString(CultureInfo.InvariantCulture);
		var file = new Writer(0);

		file.Line("// The sets a refusal names, by number: built the first time a refusal asks for one, never in the");
		file.Line("// type's initializer, and then the same array for as long as the process lives, because refusals");
		file.Line("// tell sets apart by reference.");
		file.Line("static string[]?[]? ExpectedBuilt_DotGram;");
		file.Line("static string[]? ExpectedItems_DotGram;");
		file.Line("static int[]? ExpectedStarts_DotGram;");
		file.Line();

		using (file.Block($"static string[] {Accessor}(int id)"))
		{
			file.Line("var built = ExpectedBuilt_DotGram;");
			file.Line();

			using (file.Block("if (built != null)"))
			{
				file.Line("var set = built[id];");
				file.Line();
				file.Line("if (set != null)");
				file.Then("return set;");
			}

			file.Line();
			file.Line("return ExpectedDecoded_DotGram(id);");
		}

		file.Line();

		// What the numbers say: where each set begins, after the starts, then each set as how many
		// items it has and the number of each in the list. Every number is SpelledNumbers': groups
		// of five bits, the last of a number below 0x60.
		using (file.Block("static string[] ExpectedDecoded_DotGram(int id)"))
		{
			var line = new StringBuilder("var data = ");

			SpelledNumbers.Append(line, numbers, utf8);
			line.Append(';');
			file.Line(line.ToString());
			file.Line(
				"var built = ExpectedBuilt_DotGram ?? global::System.Threading.Interlocked.CompareExchange(" +
				$"ref ExpectedBuilt_DotGram, new string[]?[{size}], null) ?? ExpectedBuilt_DotGram!;");
			file.Line("var items = ExpectedItems_DotGram ?? global::System.Threading.Interlocked.CompareExchange(ref ExpectedItems_DotGram, new string[]");

			using (file.Indent())
			{
				file.Line("{");

				using (file.Indent())
					foreach (var item in items)
						file.Line($"\"{item}\",");

				file.Line("}, null) ?? ExpectedItems_DotGram!;");
			}

			file.Line("var starts = ExpectedStarts_DotGram;");
			file.Line("var at = 0;");
			file.Line("int value;");
			file.Line();

			using (file.Block("if (starts == null)"))
			{
				file.Line($"starts = new int[{size}];");
				file.Line();

				using (file.Block("for (var i = 0; i < starts.Length; i++)"))
				{
					Read(file);
					file.Line("starts[i] = value;");
				}

				file.Line();
				file.Line("for (var i = 0; i < starts.Length; i++)");
				file.Then("starts[i] += at;");
				file.Line();
				file.Line("starts = global::System.Threading.Interlocked.CompareExchange(ref ExpectedStarts_DotGram, starts, null) ?? starts;");
			}

			file.Line();
			file.Line("at = starts[id];");
			Read(file);
			file.Line();
			file.Line("var set = new string[value];");
			file.Line();

			using (file.Block("for (var i = 0; i < set.Length; i++)"))
			{
				Read(file);
				file.Line("set[i] = items[value];");
			}

			file.Line();
			file.Line("return global::System.Threading.Interlocked.CompareExchange(ref built[id], set, null) ?? set;");
		}

		return file.ToString();

		// One number into `value`, from `at` on.
		static void Read(Writer file)
		{
			var mask = $"0x{(1 << SpelledNumbers.Bits) - 1:X2}";

			file.Line("value = 0;");
			file.Line($"while (data[at] >= 0x{SpelledNumbers.More:X2})");
			file.Then($"value = value << {SpelledNumbers.Bits} | data[at++] & {mask};");
			file.Line($"value = (value << {SpelledNumbers.Bits} | data[at++] & {mask}) - 1;");
		}
	}
}
