using System;

using BenchmarkDotNet.Running;

namespace DotGram.HandDeferred;

/// <summary>
/// What the harness needs of a reading, which is the two phases and nothing else.
/// </summary>
/// <remarks>
/// <see cref="Reader"/> is a <c>ref struct</c> and cannot implement it, so it is driven on
/// its own everywhere it appears. Everything else goes through here.
/// </remarks>
interface IReading
{
	bool Recognize();

	string Construct();
}

/// <summary>
/// Every safe reading of <c>Deferred.gram</c> in this project, side by side.
/// </summary>
/// <remarks>
/// With no arguments it shows what they answer, and checks that they agree. With any
/// argument it hands over to BenchmarkDotNet, which is what says what they cost:
/// <c>--filter *</c> for all of it, <c>--filter *Recognizing*</c> for the half where the
/// representations differ rather than the author's own quadratic.
/// </remarks>
static class Program
{
	static void Main(string[] args)
	{
		if (args.Length > 0)
		{
			BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

			return;
		}

		foreach (var input in new[]
			{ "a = 1 + bb = 22", "a=1", "a = 1 + (b = 2 + c = 3)", "((a = 1))", "a = 1 + bb", "a = ", "(a = 1", "" })
		{
			Show(input);
		}
	}

	static void Show(string input)
	{
		Console.WriteLine($"\"{input}\"");

		Author.Forget();

		var reader = new Reader(input);
		var read   = reader.Recognize();

		Console.WriteLine($"  tape:     read {read}, and the author's code has run {Author.Constructions} times");

		foreach (var line in reader.Describe().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
			Console.WriteLine("            " + line);

		var expected = read ? reader.Construct() : null;

		if (read)
			Console.WriteLine($"            value \"{expected}\", after {Author.Constructions} of them");

		Agrees("closures", new Closures(input), read, expected);
		Agrees("mixed",    new Mixed   (input), read, expected);
		Agrees("arenas",   new Arenas  (input), read, expected);
		Agrees("boxed",    new Boxed   (input), read, expected);
		Agrees("classes",  new Classes (input), read, expected);

		Console.WriteLine();
	}

	/// <summary>One reading against the tape, which is the reference for all of them.</summary>
	static void Agrees(string what, IReading reading, bool read, string? expected)
	{
		Author.Forget();

		var answered = reading.Recognize();

		Console.WriteLine(
			$"  {what + ":",-9} read {answered}, and the author's code has run {Author.Constructions} times");

		if (answered != read)
			throw new InvalidOperationException($"{what} says {answered} where the tape says {read}.");

		if (!answered)
			return;

		var value = reading.Construct();

		Console.WriteLine($"            value \"{value}\", after {Author.Constructions} of them");

		if (value != expected)
			throw new InvalidOperationException($"{what} built \"{value}\" where the tape built \"{expected}\".");
	}
}
