using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DotGram.Benchmarks;

// `--stand-held-whole beforeDir afterDir [--repeat N]` (performance-ff, C4c step 2, 2026-09-19): what the whole-stream forms
// of FIX, `FixGrammar.ParseFields(Stream)` and `ParseFields(TextReader)`, hold while they read: the live heap above what was
// live before, after a full collection taken inside the stream's own reads, so the reader is in the middle of its turn. What
// the array of the result adds is in the figure too, and grows with the input on both sides; what a reader that lets go of
// what it has read adds is the difference between the sides. Each size of each form of each side is read in a process of its own.

static partial class Stand
{
	static readonly (string Form, long Fields)[] HeldWholeInputs =
	[
		("stream-10000", 10_000),
		("stream-100000", 100_000),
		("reader-10000", 10_000),
		("reader-100000", 100_000),
	];

	/// <summary>A stream that takes a full collection every so many reads, and remembers the highest live heap seen.</summary>
	sealed class SampledStream(Stream inner, int every) : Stream
	{
		int _reads;

		public long Peak { get; private set; }

		public override bool CanRead  => true;
		public override bool CanSeek  => false;
		public override bool CanWrite => false;
		public override long Length   => inner.Length;

		public override long Position
		{
			get => inner.Position;
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			Sample();

			return inner.Read(buffer, offset, count);
		}

		public override int Read(Span<byte> buffer)
		{
			Sample();

			return inner.Read(buffer);
		}

		void Sample()
		{
			if (++_reads % every == 0)
				Peak = Math.Max(Peak, GC.GetTotalMemory(true));
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
	}

	public static void HeldWholeOne(string side, string directory, string input)
	{
		var (form, fields) = HeldWholeInputs.First(one => one.Form == input);
		var parse   = new PairedSide(side, directory).FixWhole(form.StartsWith("reader", StringComparison.Ordinal));
		var stream  = new SampledStream(new MadeFix(fields), 2);
		object source = form.StartsWith("reader", StringComparison.Ordinal) ? new StreamReader(stream, Encoding.Latin1) : stream;

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		var floor  = GC.GetTotalMemory(true);
		var result = (Array)parse(source);

		Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{Math.Max(0, stream.Peak - floor) / 1024.0:R} {result.Length}"));
	}

	public static void HeldWholePaired(string beforeDir, string afterDir, int repeat)
	{
		var text = new StringBuilder();

		text.AppendLine(CultureInfo.InvariantCulture, $"Held while the whole-stream form of FIX reads, median of {repeat} processes each (kilobytes above the live heap before the read, sampled inside the stream's reads):");
		text.AppendLine();
		text.AppendLine("| input | fields read | before KB | after KB | after / before |");
		text.AppendLine("| --- | ---: | ---: | ---: | ---: |");

		foreach (var (input, _) in HeldWholeInputs)
		{
			var kilobytes = new double[2];
			var fields    = 0;

			for (var s = 0; s < 2; s++)
			{
				var taken = new List<double>();

				for (var i = 0; i < repeat; i++)
				{
					var host  = Environment.ProcessPath!;
					var start = new ProcessStartInfo(host) { RedirectStandardOutput = true, UseShellExecute = false };

					if (Path.GetFileNameWithoutExtension(host).Equals("dotnet", StringComparison.OrdinalIgnoreCase))
						start.ArgumentList.Add(typeof(Stand).Assembly.Location);

					foreach (var argument in new[] { "--stand-held-whole-one", s == 0 ? "before" : "after", s == 0 ? beforeDir : afterDir, input })
						start.ArgumentList.Add(argument);

					using var process = Process.Start(start)!;
					var       output  = process.StandardOutput.ReadToEnd();

					process.WaitForExit();

					if (process.ExitCode != 0)
						throw new InvalidOperationException($"A whole-stream held read of {input} exited with {process.ExitCode}.");

					var numbers = output.Trim().Split('\n')[^1].Split(' ');

					taken.Add(double.Parse(numbers[0], CultureInfo.InvariantCulture));
					fields = int.Parse(numbers[1], CultureInfo.InvariantCulture);
				}

				kilobytes[s] = Median(taken);
			}

			text.AppendLine(CultureInfo.InvariantCulture, $"| {input} | {fields:N0} | {kilobytes[0]:N0} | {kilobytes[1]:N0} | {(kilobytes[0] > 0 ? kilobytes[1] / kilobytes[0] : double.NaN):0.000} |");
		}

		Console.Write(text);
	}
}
