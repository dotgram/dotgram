using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Handwritten.Fix;

namespace DotGram.Benchmarks;

// `--stand-held beforeDir afterDir [--repeat N]`: what the stream form of each side of a pair
// holds while it is walked, the same reading `Streamed` takes of the two hand-and-generated
// parsers (performance-ff, 2026-09-19: the whole-stream reader is said to hold exactly what
// the engine held, all its input until the walk; if it does not, he wants to know before he
// pushes). Each input of each side is read in a process of its own, so that what one side left
// on the heap cannot be counted for another, and taken N times.

static partial class Stand
{
	/// <summary>The inputs a side's stream form is asked to hold, and how many fields each has.</summary>
	static readonly (string Name, long Fields)[] HeldInputs =
	[
		("made-2000000", StreamedFields),
		("slope-1000000", 1_000_000),
		("slope-100000", 100_000),
		("slope-16", 16),
		("recover-1000000", 1_000_000),
		("recover-100000", 100_000),

		// The same walks over a TextReader on the made stream: the yield form of BufferedText.
		("made-2000000-reader", StreamedFields),
		("slope-100000-reader", 100_000),
		("recover-100000-reader", 100_000),
	];

	static Stream HeldStream(string input)
	{
		input = input.EndsWith("-reader", StringComparison.Ordinal) ? input[..^"-reader".Length] : input;

		if (input == "made-2000000")
			return new MadeFix(StreamedFields);

		// Every second field malformed, so that a reader that recovers is asked to on every other one.
		if (input.StartsWith("recover-", StringComparison.Ordinal))
		{
			var pairs = int.Parse(input.AsSpan("recover-".Length), CultureInfo.InvariantCulture) / 2;

			return new MemoryStream(Encoding.Latin1.GetBytes(string.Concat(Enumerable.Repeat("55=ABC\u000140X=2\u0001", pairs))), false);
		}

		return new MemoryStream(FixSlopeBytes(int.Parse(input.AsSpan("slope-".Length), CultureInfo.InvariantCulture)), false);
	}

	/// <summary>One input, one side (or "hand"): prints the kilobytes held above the floor and the fields read.</summary>
	public static void HeldOne(string side, string directory, string input)
	{
		var expected = Array.Find(HeldInputs, one => one.Name == input).Fields;
		var  reader = input.EndsWith("-reader", StringComparison.Ordinal);
		Func<Stream, IEnumerable> parse = reader
			? side is "hand" or "kept"
				? stream => HandFixParser.Parse(new StreamReader(stream, Encoding.Latin1))
				: stream => new PairedSide(side, directory).FixYieldReaderFields(new StreamReader(stream, Encoding.Latin1))
			: side is "hand" or "kept"
				? stream => HandFixParser.Parse(stream)
				: new PairedSide(side, directory).FixStreamFields;

		// The input exists before the floor is taken: a memory stream's bytes are the caller's, not the reader's.
		var source = HeldStream(input);

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		var floor    = GC.GetTotalMemory(true);
		var peak     = floor;
		var fields   = 0L;
		var interval = Math.Max(1, expected / 8);

		// The control that proves the reading sees what is held: the same walk that keeps every field it meets.
		var kept = side == "kept" ? new List<object>() : null;

		foreach (var field in parse(source))
		{
			fields++;
			kept?.Add(field);

			if (fields % interval == 0)
				peak = Math.Max(peak, GC.GetTotalMemory(true));
		}

		Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{(peak - floor) / 1024.0:R} {fields}"));
	}

	public static void HeldPaired(string beforeDir, string afterDir, int repeat)
	{
		var sides = new (string Name, string Directory)[] { ("hand", beforeDir), ("before", beforeDir), ("after", afterDir) };
		var text  = new StringBuilder();

		text.AppendLine(CultureInfo.InvariantCulture, $"Held while the stream form is walked, median of {repeat} processes each (kilobytes above the live heap before the walk):");
		text.AppendLine();
		text.AppendLine("| input | fields | hand | before | after | after / before |");
		text.AppendLine("| --- | ---: | ---: | ---: | ---: | ---: |");

		foreach (var (input, expected) in HeldInputs)
		{
			var kilobytes = new double[sides.Length];

			for (var s = 0; s < sides.Length; s++)
			{
				var taken = new List<double>();

				for (var i = 0; i < repeat; i++)
				{
					var (kb, fields) = HeldChild(sides[s].Name, sides[s].Directory, input);

					if (fields != expected)
						throw new InvalidOperationException($"{input} {sides[s].Name} read {fields} fields, not {expected}.");

					taken.Add(kb);
				}

				kilobytes[s] = Median(taken);
			}

			var ratio = kilobytes[1] > 0 ? kilobytes[2] / kilobytes[1] : double.NaN;

			text.AppendLine(CultureInfo.InvariantCulture, $"| {input} | {expected:N0} | {kilobytes[0]:N0} | {kilobytes[1]:N0} | {kilobytes[2]:N0} | {ratio:0.000} |");
		}

		Console.Write(text);
	}

	static (double Kilobytes, long Fields) HeldChild(string side, string directory, string input)
	{
		var host  = Environment.ProcessPath!;
		var start = new ProcessStartInfo(host) { RedirectStandardOutput = true, UseShellExecute = false };

		if (Path.GetFileNameWithoutExtension(host).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
			start.ArgumentList.Add(typeof(Stand).Assembly.Location);

		foreach (var argument in new[] { "--stand-held-one", side, directory, input })
			start.ArgumentList.Add(argument);

		using var process = Process.Start(start)!;
		var       output  = process.StandardOutput.ReadToEnd();

		process.WaitForExit();

		if (process.ExitCode != 0)
			throw new InvalidOperationException($"A held read of {input} ({side}) exited with {process.ExitCode}.");

		var numbers = output.Trim().Split('\n')[^1].Split(' ');

		return (double.Parse(numbers[0], CultureInfo.InvariantCulture), long.Parse(numbers[1], CultureInfo.InvariantCulture));
	}
}
