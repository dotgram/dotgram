using System;

namespace DotGram.HandDeferred;

/// <summary>
/// Runs <see cref="Reader"/> over a few inputs and shows both halves of it: the tape a
/// reading leaves behind, and the constructions that do or do not follow.
/// </summary>
static class Program
{
	static void Main()
	{
		foreach (var input in new[] { "a = 1 + bb = 22", "a=1", "a = 1 + bb", "a = ", "" })
			Show(input);
	}

	static void Show(string input)
	{
		Reader.Forget();

		var reader = new Reader(input);
		var read   = reader.Recognize();

		Console.WriteLine($"\"{input}\"");
		Console.WriteLine($"  read {read}, and the author's code has run {Reader.Constructions} times");

		foreach (var line in reader.Describe().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
			Console.WriteLine("  " + line);

		if (read)
		{
			var value = reader.Construct();

			Console.WriteLine($"  value \"{value}\", after {Reader.Constructions} of them");
		}

		Console.WriteLine();
	}
}
