using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.ExpressionLanguage;

using DotGram.Finance.Fix;

namespace DotGram.Finance.Benchmarks;

/// <summary>
/// Writes out the validation text a dictionary composes into, and asks the compiler whether it
/// is a program.
/// </summary>
/// <remarks>
/// <para>
/// A file per message type, because the text of ninety-three messages in one file is not
/// something anybody reads. Each is the whole rule for that type: the header, the body and the
/// trailer, the components expanded into the switch, the groups written where they are walked.
/// </para>
/// <para>
/// It is not a benchmark and not a test. It is the way to LOOK at what the composer writes, which
/// is what has to be judged before any of it is offered to a consumer. It compiles what it wrote
/// in the same run, so a dump that reads well and would not compile cannot be mistaken for one
/// that works — and it compiles the BLANKED copy, the one a consumer's process would compile,
/// because that is the text whose line numbers a failure would name.
/// </para>
/// </remarks>
static class FixRuleDump
{
	public static void Run(string[] args)
	{
		var file = args.Length > 0 ? args[0] : Path.Combine(Repository(), "tests", "Corpus", "Fix", "FIX44.xml");
		var into = args.Length > 1 ? args[1] : Path.Combine(Repository(), ".work", "rules");
		var only = args.Length > 2 ? args[2] : null;

		FixDictionary dictionary;

		using (var input = File.OpenRead(file))
			dictionary = FixDictionary.Load(input);

		Directory.CreateDirectory(into);

		var composer = new FixRuleComposer(dictionary);
		var types    = dictionary.MessageTypes.OrderBy(type => type, StringComparer.Ordinal)
			.Where(type => only is null || type == only)
			.ToArray();

		var refused   = new List<string>();
		var octets    = 0L;
		var longest   = ("", 0);
		var assembly  = typeof(FixMessage).Assembly;
		var composing = new System.Diagnostics.Stopwatch();
		var blanks    = new List<string>();
		var reading   = new System.Diagnostics.Stopwatch();

		foreach (var type in types)
		{
			var name = dictionary.MessageName(type) ?? type;

			composing.Start();

			var text    = composer.Rule(type);
			var blanked = FixRuleText.Blank(text);

			composing.Stop();

			// The name a reader would look for, and the MsgType beside it because that is what the
			// validator is keyed by: "D-NewOrderSingle.txt".
			File.WriteAllText(Path.Combine(into, Safe(type) + "-" + Safe(name) + ".txt"), text);

			octets += text.Length;

			if (text.Length > longest.Item2)
				longest = (type + " " + name, text.Length);

			blanks.Add(blanked);

			reading.Start();

			var said = ExpressionParser.TryParse(blanked, assembly);

			reading.Stop();

			if (!said.IsSuccess)
				refused.Add($"{type} {name}: {Where(text, (int)said.Position)}: {said.Error}");
		}

		Console.WriteLine($"{types.Length} rules, {octets:N0} characters, longest {longest.Item1} at {longest.Item2:N0}.");
		Console.WriteLine($"written to {into}");
		Console.WriteLine($"composed in {composing.ElapsedMilliseconds} ms, read by the language in {reading.ElapsedMilliseconds} ms.");

		// The first pass carries the JIT of the parser itself, and a parser measured on its first
		// pass is a parser measured at tier 0. Two more passes over the same texts, and the last one
		// is what a process that reads dictionaries for a living would see.
		for (var pass = 2; pass <= 3; pass++)
		{
			var again = System.Diagnostics.Stopwatch.StartNew();

			foreach (var one in blanks)
				ExpressionParser.TryParse(one, assembly);

			again.Stop();

			Console.WriteLine(
				$"pass {pass}: {again.ElapsedMilliseconds} ms, " +
				$"{octets / (again.Elapsed.TotalSeconds * 1024 * 1024):F1} MB/s");
		}

		if (refused.Count == 0)
		{
			Console.WriteLine("every rule compiles.");

			return;
		}

		Console.WriteLine($"{refused.Count} of {types.Length} did not compile:");

		foreach (var one in refused.Take(10))
			Console.WriteLine("  " + one);
	}

	/// <summary>
	/// The line and column of a position, and the line itself — read out of the text WITH its
	/// comments, which is the whole point of blanking: the compiler read a copy of the same shape,
	/// so its position lands here.
	/// </summary>
	static string Where(string text, int at)
	{
		var line   = 1;
		var start  = 0;

		for (var i = 0; i < at && i < text.Length; i++)
			if (text[i] == '\n')
			{
				line++;
				start = i + 1;
			}

		var end = text.IndexOf('\n', start);

		if (end < 0)
			end = text.Length;

		return $"line {line}, column {at - start + 1} |{text.Substring(start, end - start).Trim()}|";
	}

	// A MsgType may be "n" or "AB" and a name is the file's; neither is trusted to be a filename.
	static string Safe(string name)
	{
		return string.Concat(name.Select(c => char.IsLetterOrDigit(c) ? c : '_'));
	}

	static string Repository()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? throw new InvalidOperationException("The repository root was not found.");
	}
}
