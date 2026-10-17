using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Bindings read through a buffer, probed where a buffer is likely to break them: every short
/// input over a small alphabet for many placements of a binding, bindings longer than any piece
/// the input arrives in, characters a buffer cuts in two, find, yield and recovering feeds over
/// long inputs through a small window, and random grammars. Each buffered reading is held to
/// the string's: the answer and the value always, and where a refusal stopped wherever the two
/// forms place refusals alike for grammars without bindings too.
/// </summary>
public sealed class BackReferenceBreakTests(ITestOutputHelper output)
{
	static readonly int[] Sizes = [1, 2, 3, 7, 4096];

	static void Info(string line)
	{
		TestContext.Current.TestOutputHelper?.WriteLine(line);
	}

	static (bool IsSuccess, string Value, string? Error, long Position) Read(object match)
	{
		object? Get(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		var value = Get("Value") switch
		{
			byte[] bytes => Encoding.UTF8.GetString(bytes),
			null => "<null>",
			var other => other.ToString()!,
		};

		return ((bool)Get("IsSuccess")!, value, (string?)Get("Error"), Convert.ToInt64(Get("Position")));
	}

	static string Call(Func<object> call)
	{
		try
		{
			var (ok, value, error, position) = Read(call());

			return ok ? $"ok {value} @{position}" : $"no @{position}";
		}
		catch (TargetInvocationException e)
		{
			return "THROW " + e.InnerException!.GetType().Name + ": " + e.InnerException.Message;
		}
	}

	static string Whole(Assembly assembly, string method, string input)
	{
		return Call(() => assembly.GetType("Grammar")!.GetMethod(method, [typeof(string)])!.Invoke(null, [input])!);
	}

	static string Reader(Assembly assembly, string method, string input, int size, int? retained = null)
	{
		return Call(() => assembly.GetType("Grammar")!.GetMethod(method, [typeof(TextReader), typeof(int?), typeof(int?)])!
			.Invoke(null, [new StringReader(input), size, retained])!);
	}

	static string Streamed(Assembly assembly, string method, byte[] input, int size, int? retained = null)
	{
		return Call(() => assembly.GetType("Grammar")!.GetMethod(method, [typeof(Stream), typeof(int?), typeof(int?)])!
			.Invoke(null, [new MemoryStream(input), size, retained])!);
	}

	static string Held(Assembly assembly, string method, byte[] input)
	{
		return Call(() => assembly.GetType("Grammar")!.GetMethod(method, [typeof(byte[])])!.Invoke(null, [input])!);
	}

	static string Memory(Assembly assembly, string method, byte[] input, int offset)
	{
		var padded = new byte[input.Length + offset * 2];

		for (var at = 0; at < padded.Length; at++)
			padded[at] = (byte)'#';

		Array.Copy(input, 0, padded, offset, input.Length);

		return Call(() => assembly.GetType("Grammar")!.GetMethod(method, [typeof(ReadOnlyMemory<byte>)])!
			.Invoke(null, [new ReadOnlyMemory<byte>(padded, offset, input.Length)])!);
	}

	internal static (Assembly Assembly, string Source, bool Bytes) Compile(
		string grammar, bool direct, CarrierKind carrier = CarrierKind.Auto, bool bytes = true, string? members = null)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier       = carrier,
			Direct        = direct,
			BufferedInput = true,
			BufferedBytes = bytes,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		if (bytes && result.Diagnostics.Any(static one => one.Id == GramCompiler.BufferedUnsupported))
		{
			Info("no bytes: " + string.Join("; ", result.Diagnostics.Where(static one => one.Id == GramCompiler.BufferedUnsupported).Select(static one => one.Message)));

			return Compile(grammar, direct, carrier, false, members);
		}

		var errors = result.Diagnostics.Where(static one => one.Severity == GramSeverity.Error).ToArray();

		if (errors.Length > 0)
			throw new InvalidOperationException("refused: " + string.Join("; ", errors.Select(static one => one.Id + " " + one.Message)));

		var source   = string.Join("\n", result.Sources.Select(static one => one.Text));
		var assembly = EmittedCode.Compile(
			result.Sources[0].Text,
			declarationMembers: members ??
				"static string Text(string text) { return text; } " +
				"static string Text(global::System.ReadOnlySpan<char> text) { return text.ToString(); } " +
				"static string Text(global::System.ReadOnlySpan<byte> text) { return global::System.Text.Encoding.UTF8.GetString(text.ToArray()); }",
			sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

		return (assembly, source, bytes);
	}

	/// <summary>Every buffered reading of each input against the string's; a list of disagreements.</summary>
	static List<string> Disagreements(string grammar, string rule, IEnumerable<string> inputs, bool positions = true)
	{
		var told = new List<string>();

		foreach (var direct in new[] { true, false })
			foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape, CarrierKind.Immediate })
			{
				(Assembly Assembly, string Source, bool Bytes) compiled;

				try
				{
					compiled = Compile(grammar, direct, carrier);
				}
				catch (InvalidOperationException e) when (e.Message.StartsWith("refused", StringComparison.Ordinal))
				{
					told.Add($"[{grammar.Replace("\n", " ⏎ ")}] {(direct ? "reader" : "engine")}/{carrier}: {e.Message}");

					return told;
				}

				var method = "TryParse" + rule;

				foreach (var input in inputs)
				{
					var whole = Whole(compiled.Assembly, method, input);
					var ascii = input.All(static one => one < 128);
					var bytes = Encoding.UTF8.GetBytes(input);

					void Check(string got, string how)
					{
						var same = positions || !ascii ? got == whole : got.Split('@')[0] == whole.Split('@')[0];

						if (!ascii)
							same = got.Split('@')[0].Split(' ')[0] == whole.Split('@')[0].Split(' ')[0];

						if (!same || got.StartsWith("THROW", StringComparison.Ordinal))
							told.Add($"[{grammar.Replace("\n", " ⏎ ")}] {(direct ? "reader" : "engine")}/{carrier} {how} \"{input.Replace("\n", "\\n")}\": string {whole} / buffered {got}");
					}

					foreach (var size in Sizes)
					{
						Check(Reader(compiled.Assembly, method, input, size), $"reader/{size}");

						if (compiled.Bytes)
							Check(Streamed(compiled.Assembly, method, bytes, size), $"stream/{size}");
					}

					if (compiled.Bytes)
					{
						Check(Held(compiled.Assembly, method, bytes), "byte[]");
						Check(Memory(compiled.Assembly, method, bytes, 3), "memory+3");
					}
				}
			}

		return told;
	}

	static IEnumerable<string> All(string alphabet, int most)
	{
		var level = new List<string> { "" };

		yield return "";

		for (var length = 1; length <= most; length++)
		{
			var next = new List<string>();

			foreach (var prefix in level)
				foreach (var one in alphabet)
				{
					next.Add(prefix + one);

					yield return prefix + one;
				}

			level = next;
		}
	}

	void Report(List<string> told)
	{
		foreach (var line in told.Take(20))
			output.WriteLine(line);

		Assert.True(told.Count == 0, $"{told.Count} disagreements; first: {told.FirstOrDefault()}");
	}

	// ── Exhaustive small-alphabet differentials over many placements of a binding ─────────

	public static TheoryData<string, string> Shapes => new()
	{
		// Alternation backtracking: a binding that gives back, references tried again.
		{ "R = $q: 'a'+ & 'a'* & ':' & $q & eof", "a:" },
		{ "R = ($q: 'a'+ & ':' & $q | $q: 'a' & 'a' & $q & 'b') & eof", "ab:" },
		{ "R = $q: ('a' | 'b')* & ':' & $q & $q? & eof", "ab:" },
		// Nested bindings, both referenced.
		{ "R = $o: ('<' & $i: 'a'* & '>') & $i & $o & eof", "<a>" },
		// Reference inside a lookahead, before and at the end of input.
		{ "R = $q: 'a'+ & (?!$q & any)* & $q & eof", "ab" },
		{ "R = $q: 'a'+ & ':' & ?=$q & 'a'* & eof", "a:" },
		{ "R = $q: 'a'+ & ':' & ?!($q & eof) & any* & eof", "a:" },
		// Inside a repetition: the binding re-made at each turn, read in its turn.
		{ "R = ($q: ('a' | 'b') & $q & ',')* & eof", "ab," },
		{ "R = ($q: ('a' | 'b')+ & ':' & $q & ';')+ & eof", "ab:;" },
		// A count by a length, over and under the binding.
		{ "R = $q: 'a'+ & ':' & 'b'{#q} & eof", "ab:" },
		{ "R = $q: 'a'* & ':' & 'b'{#q,} & ':' & $q & eof", "ab:" },
		{ "R = $q: 'a'+ & $r: 'b'+ & 'c'{#q,#r} & eof", "abc" },
		// A reference read twice and atomic.
		{ "R = { $q: 'a'+ } & ':' & $q & $q & eof", "a:" },
		{ "R = $q: any & any* & $q & eof", "ab" },
		// Across rules: a binding in a called rule, called twice.
		{ "R = P & ';' & P & eof\nP = $q: ('a' | 'b')+ & ':' & $q", "ab:;" },
		// Trivia that binds, woven between operands.
		{ "trivia = { ($s: ' '+ & '.' & $s)? }\nR = 'a' & 'b' & eof", "ab. " },
		{ "trivia = { ' '* }\nR = $q: ('a' & 'b'?)+ & ':' & $q & eof", "ab: " },
		// No eof: a prefix match, the reference at the end of what was read.
		{ "R = $q: 'a'+ & ':' & $q", "ab:" },
	};

	[Theory]
	[MemberData(nameof(Shapes))]
	public void Exhaustive_shape_agrees(string grammar, string alphabet)
	{
		Report(Disagreements(grammar + "\nparse R stream bytes\n", "R", All(alphabet, alphabet.Length <= 3 ? 7 : 6)));
	}

	// ── Long bindings: longer than every buffer piece, than the part size ─────────────

	[Theory]
	[InlineData(1)]
	[InlineData(1000)]
	[InlineData(70000)]
	public void A_long_binding_is_compared_over_a_tiny_buffer(int length)
	{
		const string grammar = "R = $q: ['a'..'z']+ & ':' & $q & eof\nparse R stream bytes\n";

		var good = new string('a', length - 1) + "b:" + new string('a', length - 1) + "b";
		var bad  = new string('a', length - 1) + "b:" + new string('a', length - 1) + "c";
		var told = new List<string>();

		foreach (var direct in new[] { true, false })
		{
			var compiled = Compile(grammar, direct);

			foreach (var input in new[] { good, bad, good.Substring(0, good.Length - 1) })
			{
				var whole = Whole(compiled.Assembly, "TryParseR", input);

				foreach (var size in new[] { 1, 2, 4096 })
				{
					var got = Reader(compiled.Assembly, "TryParseR", input, size);

					if (got != whole)
						told.Add($"{direct} {length} size {size}: string {whole[..Math.Min(40, whole.Length)]} / reader {got[..Math.Min(80, got.Length)]}");

					got = Streamed(compiled.Assembly, "TryParseR", Encoding.ASCII.GetBytes(input), size);

					if (got != whole)
						told.Add($"{direct} {length} size {size}: string {whole[..Math.Min(40, whole.Length)]} / stream {got[..Math.Min(80, got.Length)]}");
				}
			}
		}

		Report(told);
	}

	// ── Characters a buffer cuts in two ─────────────────────────────────────────────────

	[Fact]
	public void Surrogate_pairs_and_utf8_cut_by_the_buffer_compare_alike()
	{
		// A grammar that does not care what an item is: a reference is ordinal, char or byte.
		const string grammar = "R = $q: (?!':' & any)+ & ':' & $q & eof\nparse R stream bytes\n";

		var inputs = new List<string>();

		foreach (var q in new[] { "😀", "a😀", "😀😀", "é", "aé", "日本", "\U0001F600x" })
		{
			inputs.Add(q + ":" + q);
			inputs.Add(q + ":" + q + "x");
			inputs.Add(q + ":" + q.Substring(0, q.Length - 1));
			// The same first char unit, a different low surrogate / a different trailing byte.
			if (q.Length >= 2 && char.IsHighSurrogate(q[q.Length - 2]))
				inputs.Add(q + ":" + q.Substring(0, q.Length - 1) + "\uDE01");
			inputs.Add(q + ":" + q.Replace("é", "è").Replace("日", "旦"));
		}

		Report(Disagreements(grammar, "R", inputs));
	}

	[Fact]
	public void A_count_by_a_length_over_bytes_counts_bytes_as_documented()
	{
		const string grammar = "R = $q: (?!':' & any)+ & ':' & '='{#q} & eof\nparse R stream bytes\n";

		var compiled = Compile(grammar, true);

		// One UTF-16 unit over a string and a reader; two bytes over a stream.
		Assert.StartsWith("ok", Whole(compiled.Assembly, "TryParseR", "é:="), StringComparison.Ordinal);
		Assert.StartsWith("ok", Reader(compiled.Assembly, "TryParseR", "é:=", 1), StringComparison.Ordinal);
		Assert.StartsWith("no", Streamed(compiled.Assembly, "TryParseR", Encoding.UTF8.GetBytes("é:="), 1), StringComparison.Ordinal);
		Assert.StartsWith("ok", Streamed(compiled.Assembly, "TryParseR", Encoding.UTF8.GetBytes("é:=="), 1), StringComparison.Ordinal);

		// A surrogate pair is two units over characters.
		Assert.StartsWith("ok", Whole(compiled.Assembly, "TryParseR", "😀:=="), StringComparison.Ordinal);
		Assert.StartsWith("ok", Reader(compiled.Assembly, "TryParseR", "😀:==", 1), StringComparison.Ordinal);
	}

	// ── find and yield over a long stream with a small window ───────────────────────────

	[Fact]
	public void Find_with_a_binding_streams_a_long_input_through_a_small_window()
	{
		const string grammar = "R = '<' & $q: ['a'..'z']+ & '>' & (?!(\"</\" & $q & '>') & any)* & \"</\" & $q & '>'\nfind R stream bytes\nparse R\n";

		var unit = "<ab>x<a>y</a></ab>..<c></d>.<c></c>";
		var text = string.Concat(Enumerable.Repeat(unit, 5000));
		var told = new List<string>();

		foreach (var direct in new[] { true, false })
		{
			var compiled = Compile(grammar, direct);
			var type     = compiled.Assembly.GetType("Grammar")!;

			long[] Positions(object found)
			{
				var list = new List<long>();

				foreach (var one in (System.Collections.IEnumerable)found)
					list.Add(Convert.ToInt64(one!.GetType().GetProperty("Position")!.GetValue(one)));

				return [.. list];
			}

			var whole = Positions(type.GetMethod("FindR", [typeof(string)])!.Invoke(null, [text])!);

			foreach (var size in new[] { 1, 3, 64 })
			{
				try
				{
					var read = Positions(type.GetMethod("FindR", [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [new StringReader(text), size, 256])!);

					if (!read.SequenceEqual(whole))
						told.Add($"{direct} reader {size}: {read.Length} vs {whole.Length}");

					var streamed = Positions(type.GetMethod("FindR", [typeof(Stream), typeof(int?), typeof(int?)])!.Invoke(null, [new MemoryStream(Encoding.ASCII.GetBytes(text)), size, 256])!);

					if (!streamed.SequenceEqual(whole))
						told.Add($"{direct} stream {size}: {streamed.Length} vs {whole.Length}");
				}
				catch (TargetInvocationException e)
				{
					told.Add($"{direct} {size}: {e.InnerException}");
				}
			}
		}

		Report(told);
	}

	[Fact]
	public void Yield_with_a_binding_streams_a_long_input_through_a_small_window()
	{
		const string grammar = "E : @string = $q: ['a'..'z']+ & ':' & v: $q & ';' => @(Text(v))\nEs : @string[] = E*\nparse Es as All stream bytes yield : @string\nparse Es\n";

		var told = new List<string>();

		foreach (var direct in new[] { true, false })
		{
			var compiled = Compile(grammar, direct);
			var type     = compiled.Assembly.GetType("Grammar")!;
			var text     = string.Concat(Enumerable.Range(0, 20000).Select(static at => new string((char)('a' + at % 26), 1 + at % 7) + ":" + new string((char)('a' + at % 26), 1 + at % 7) + ";"));

			foreach (var size in new[] { 1, 5, 64 })
			{
				try
				{
					var count = 0;

					foreach (var one in (System.Collections.IEnumerable)type.GetMethod("All", [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [new StringReader(text), size, 64])!)
						count++;

					if (count != 20000)
						told.Add($"{direct} reader {size}: {count}");

					count = 0;

					foreach (var one in (System.Collections.IEnumerable)type.GetMethod("All", [typeof(Stream), typeof(int?), typeof(int?)])!.Invoke(null, [new MemoryStream(Encoding.ASCII.GetBytes(text)), size, 64])!)
						count++;

					if (count != 20000)
						told.Add($"{direct} stream {size}: {count}");
				}
				catch (TargetInvocationException e)
				{
					told.Add($"{direct} {size}: {e.InnerException}");
				}
			}

			// A bad element: the same refusal over a reader as over the string.
			foreach (var bad in new[] { "ab:ab;ab:ac;", "ab:ab;ab:a" })
			{
				string Collect(Func<object> make)
				{
					var list = new List<string>();

					try
					{
						foreach (var one in (System.Collections.IEnumerable)make())
							list.Add(one?.ToString() ?? "<null>");

						return string.Join(",", list);
					}
					catch (TargetInvocationException e)
					{
						return string.Join(",", list) + " THROW " + e.InnerException!.GetType().Name;
					}
					catch (FormatException e)
					{
						return string.Join(",", list) + " THROW " + e.GetType().Name;
					}
				}

				var r1 = Collect(() => type.GetMethod("All", [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [new StringReader(bad), 1, null])!);
				var r2 = Collect(() => type.GetMethod("All", [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [new StringReader(bad), 4096, null])!);

				Info($"{direct} yield {bad}: size1 {r1} / size4096 {r2}");

				if (r1 != r2)
					told.Add($"{direct} yield {bad}: {r1} vs {r2}");
			}
		}

		Report(told);
	}

	// ── Recovering feeds ──────────────────────────────────────────────────────────────────

	const string FeedHead =
		"""
		Line : @string = $q: ['a'..'z']+ & ':' & v: $q & eol => @(Text(v))
		W              = ['a'..'z']+

		""";

	static string Feed(Assembly assembly, string method, string text, Type over, int size, int? retained)
	{
		var type = assembly.GetType("Grammar")!;

		return Call(() => type.GetMethod(method, [over, typeof(int?), typeof(int?)])!.Invoke(null, [
			over == typeof(TextReader) ? new StringReader(text) : new MemoryStream(Encoding.ASCII.GetBytes(text)), size, retained])!);
	}

	[Theory]
	// A binding in the element rule: in the turn that lets go.
	[InlineData("Feed : @string = lines: Line* recover eol => @(\"!\") & eof => @(string.Join(\"|\", lines))\nparse Feed\n")]
	// A binding in the continuation, read at every turn as the way out is tried.
	[InlineData("Feed : @string = lines: Line* recover eol => @(\"!\") & $t: W & '.' & $t & eof => @(string.Join(\"|\", lines))\nparse Feed\n")]
	// A continuation that binds and then holds a second recovering repetition.
	[InlineData("Feed : @string = lines: Line* recover eol => @(\"!\") & $t: W & '.' & eol & more: Line* recover eol => @(\"?\") & $t & eof => @(string.Join(\"|\", lines) + \"#\" + string.Join(\"|\", more))\nparse Feed\n")]
	// The same with no capture of the second repetition.
	[InlineData("Feed : @string = lines: Line* recover eol => @(\"!\") & $t: W & '.' & eol & (W & eol)* recover eol & $t & eof => @(string.Join(\"|\", lines))\nparse Feed\n")]
	public void A_recovering_feed_with_bindings_agrees_and_streams(string tail)
	{
		var grammar = FeedHead + tail;
		var told    = new List<string>();

		(Assembly Assembly, string Source, bool Bytes) compiled;

		try
		{
			compiled = Compile(grammar, true, CarrierKind.Immediate);
		}
		catch (InvalidOperationException e)
		{
			output.WriteLine("compile: " + e.Message);

			throw;
		}

		var letGo = compiled.Source.Contains("text.LetGo(", StringComparison.Ordinal);

		Info($"feed [{tail.Replace("\n", " ⏎ ")}]: LetGo {letGo}");

		var two = tail.Contains("more") || tail.Contains("(W & eol)*");

		string Body(int lines, bool bad)
		{
			var sb = new StringBuilder();

			for (var at = 0; at < lines; at++)
				sb.Append(bad && at % 97 == 5 ? "ab:ac\n" : "abc:abc\n");

			return sb.ToString();
		}

		IEnumerable<string> Inputs(int lines)
		{
			foreach (var bad in new[] { false, true })
				if (!tail.Contains("$t"))
					yield return Body(lines, bad);
				else if (!two)
				{
					yield return Body(lines, bad) + "xy.xy";
					yield return Body(lines, bad) + "xy.xz";
				}
				else
				{
					yield return Body(3, bad) + "xy.\n" + Body(lines, bad) + "xy";
					yield return Body(3, bad) + "xy.\n" + Body(lines, bad) + "xz";
				}
		}

		foreach (var input in Inputs(12))
		{
			var whole = Whole(compiled.Assembly, "TryParseFeed", input);

			foreach (var size in Sizes)
				foreach (var over in compiled.Bytes ? new[] { typeof(TextReader), typeof(Stream) } : new[] { typeof(TextReader) })
				{
					var got = Feed(compiled.Assembly, "TryParseFeed", input, over, size, null);

					if (got != whole)
						told.Add($"short {over.Name}/{size} \"{input.Replace("\n", "\\n")}\": string {whole} / {got}");
				}
		}

		// Long: 20,000 lines through a window of 256, where the window is let go.
		foreach (var input in letGo ? Inputs(20000) : Enumerable.Empty<string>())
		{
			var whole = Whole(compiled.Assembly, "TryParseFeed", input);

			foreach (var over in compiled.Bytes ? new[] { typeof(TextReader), typeof(Stream) } : new[] { typeof(TextReader) })
			{
				var got = Feed(compiled.Assembly, "TryParseFeed", input, over, 16, 256);

				if (got != whole)
					told.Add($"long {over.Name}: string {whole[..Math.Min(60, whole.Length)]}… / {got[..Math.Min(200, got.Length)]}");
			}
		}

		Report(told);
	}

	// ── The generator check: shapes around a turn that lets the window go ─────────────────

	public static TheoryData<string, string, string> FeedShapes => new()
	{
		// The feed reached again from its own element: a binding opened before the inner feed, read after it.
		{ "W = ['a'..'z']+\nLine : @string = n: ['0'..'9']+ & eol => @(Text(n)) | '(' & $q: (W?) & ':' & eol & f: Feed & $q => @(f)\nFeed : @string = lines: Line* recover eol => @(\"!\") & eof => @(string.Join(\"|\", lines))\nparse Feed\n",
			"1\n(:\n2\n", "1\n(:\nx\n" },
		{ "W = ['a'..'z']+\nLine : @string = n: ['0'..'9']+ & eol => @(Text(n)) | '(' & $q: (W?) & ':' & eol & f: Feed & $q => @(f)\nFeed : @string = lines: Line* recover eol => @(\"!\") & eof => @(string.Join(\"|\", lines))\nparse Feed\n",
			"1\n(ab:\n2\n", "1\n(:\nx\n" },
		// A feed called from a rule that binds before the call and refers after it, while the feed is
		// also published on its own (so it is a root somewhere).
		{ "W = ['a'..'z']+\nLine : @string = n: ['0'..'9']+ & eol => @(Text(n))\nR : @string = lines: Line* recover eol => @(\"!\") & eof => @(string.Join(\"|\", lines))\nFeed : @string = $q: (W?) & ':' & eol & r: R & $q => @(r)\nparse Feed\nparse R\n",
			"ab:\n1\n", "ab:\nx\n" },
		{ "W = ['a'..'z']+\nLine : @string = n: ['0'..'9']+ & eol => @(Text(n))\nR : @string = lines: Line* recover eol => @(\"!\") & eof => @(string.Join(\"|\", lines))\nFeed : @string = $q: (W?) & ':' & eol & r: R & $q => @(r)\nparse Feed\n",
			":\n1\n", ":\nx\n" },
		// A recovering repetition nested in the element rule, between a binding and its reference.
		{ "W = ['a'..'z']+\nLine : @string = $q: W & ':' & (W & ',')* recover ',' & v: $q & eol => @(Text(v))\nFeed : @string = lines: Line* recover eol => @(\"!\") & eof => @(string.Join(\"|\", lines))\nparse Feed\n",
			"ab:x,y,ab\n", "ab:x,y,ac\n" },
		// The same through a call from the element.
		{ "W = ['a'..'z']+\nItems = (W & ',')* recover ','\nLine : @string = $q: W & ':' & Items & v: $q & eol => @(Text(v))\nFeed : @string = lines: Line* recover eol => @(\"!\") & eof => @(string.Join(\"|\", lines))\nparse Feed\n",
			"ab:x,y,ab\n", "ab:x,1,ac\n" },
		// A continuation that binds, then calls a rule with a recovering repetition, then refers.
		{ "W = ['a'..'z']+\nItems = (W & eol)* recover eol\nLine : @string = n: ['0'..'9']+ & eol => @(Text(n))\nFeed : @string = lines: Line* recover eol => @(\"!\") & $t: W & '.' & eol & Items & $t & eof => @(string.Join(\"|\", lines))\nparse Feed\n",
			"1\nxy.\nab\nxy", "1\nxy.\nab\nxz" },
		// Trivia that binds, woven where a turn begins.
		{ "namespace N\n{\n\ttrivia = { $s: ' '* & $s }\n\tL : @string = n: ['0'..'9']+ & ' ' & eol => @(Text(n))\n}\nLine : @string = l: N.L => @(l)\nFeed : @string = lines: Line* recover eol => @(\"!\") & eof => @(string.Join(\"|\", lines))\nparse Feed\n",
			"1  \n  2\n", "1 \n" },
		// An element that is a binding of the whole line.
		{ "Line : @string = $q: (['0'..'9']+) & '=' & v: $q & eol => @(Text(v))\nFeed : @string = lines: Line* recover eol => @(\"!\") & eof => @(string.Join(\"|\", lines))\nparse Feed\n",
			"12=12\n3=3\n", "12=13\n3=3\n" },
	};

	[Theory]
	[MemberData(nameof(FeedShapes))]
	public void A_feed_shape_that_lets_go(string grammar, string good, string bad)
	{
		(Assembly Assembly, string Source, bool Bytes) compiled;

		try
		{
			compiled = Compile(grammar, true, CarrierKind.Immediate);
		}
		catch (InvalidOperationException e) when (e.Message.StartsWith("refused", StringComparison.Ordinal))
		{
			// Refused with a diagnostic, as a binding around a recovery point is; never a defect
			// thrown by the generator, which would not be caught here.
			Info($"shape [{grammar.Replace("\n", " ⏎ ")}]: {e.Message}");

			return;
		}

		var letGo = compiled.Source.Contains("text.LetGo(", StringComparison.Ordinal);
		var told  = new List<string>();

		Info($"shape [{grammar.Replace("\n", " ⏎ ")}]: LetGo {letGo}");

		var parts = new[] { good, bad };
		var inputs = new List<string>(parts);

		foreach (var a in parts)
			foreach (var b in parts)
				inputs.Add(a + b);

		// Long: the first line of the good input, repeated, before the rest.
		var first = good.Substring(0, good.IndexOf('\n') + 1);
		var longs = new[] { string.Concat(Enumerable.Repeat(first, 30000)) + good, string.Concat(Enumerable.Repeat(first, 30000)) + bad + good };

		foreach (var input in inputs)
		{
			var whole = Whole(compiled.Assembly, "TryParseFeed", input);

			foreach (var size in Sizes)
				foreach (var over in compiled.Bytes ? new[] { typeof(TextReader), typeof(Stream) } : new[] { typeof(TextReader) })
				{
					var got = Feed(compiled.Assembly, "TryParseFeed", input, over, size, null);

					if (got != whole)
						told.Add($"short {over.Name}/{size} \"{input.Replace("\n", "\\n")}\": string {whole} / {got}");
				}
		}

		foreach (var input in longs)
		{
			var whole = Whole(compiled.Assembly, "TryParseFeed", input);

			foreach (var over in compiled.Bytes ? new[] { typeof(TextReader), typeof(Stream) } : new[] { typeof(TextReader) })
				foreach (var retained in new int?[] { 256, null })
				{
					var got = Feed(compiled.Assembly, "TryParseFeed", input, over, 16, retained);

					if (got != whole && !(retained is not null && !letGo && got.Contains("IOException", StringComparison.Ordinal)))
						told.Add($"long {over.Name}/{retained}: string {whole[..Math.Min(60, whole.Length)]}… / {got[..Math.Min(200, got.Length)]}");
					else if (got != whole)
						Info($"  long {over.Name}/{retained}: {got[..Math.Min(120, got.Length)]}");
				}
		}

		Report(told);
	}

	// ── Random binding grammars, every buffered reading against the string ────────────────

	static string RandomGrammar(Random random)
	{
		string[] atoms = ["'a'", "'b'", "('a' | 'b')", "'a'+", "['a'..'b']*", "any", "\"ab\"", "'b'?", "':'"];
		var bound = new List<string>();
		var parts = new List<string>();
		var count = random.Next(2, 6);

		string Atom()
		{
			return atoms[random.Next(atoms.Length)];
		}

		for (var at = 0; at < count; at++)
		{
			var pick = bound.Count == 0 ? 0 : random.Next(10);
			var q    = bound.Count == 0 ? "" : bound[random.Next(bound.Count)];

			switch (pick)
			{
				case 0:
				case 1:
					var name = "q" + bound.Count;
					var body = random.Next(4) switch
					{
						0 => Atom(),
						1 => "(" + Atom() + " & " + Atom() + ")",
						2 => "{ " + Atom() + " }",
						_ => "(" + Atom() + " | " + Atom() + " & " + Atom() + ")",
					};
					parts.Add($"${name}: {body}");
					bound.Add(name);
					break;
				case 2: parts.Add("$" + q); break;
				case 3: parts.Add(Atom()); break;
				case 4: parts.Add(random.Next(3) switch { 0 => $"'a'{{#{q}}}", 1 => $"any{{#{q},}}", _ => $"'b'{{0,#{q}}}" }); break;
				case 5: parts.Add(random.Next(2) == 0 ? $"?!(${q} & any)" : $"?=${q}"); break;
				case 6: parts.Add($"(?!${q} & any)*"); break;
				case 7: parts.Add($"(${q} | {Atom()})"); break;
				case 8: parts.Add($"(${q} & ':')*"); break;
				default: parts.Add($"{{ ${q} }}"); break;
			}
		}

		if (bound.Count > 0 && random.Next(2) == 0)
			parts.Add("$" + bound[random.Next(bound.Count)]);

		if (random.Next(3) != 0)
			parts.Add("eof");

		return "R = " + string.Join(" & ", parts) + "\nparse R stream bytes\n";
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	public void Random_binding_grammars_agree(int seed)
	{
		var random = new Random(seed * 7919);
		var told   = new List<string>();
		var tried  = 0;
		var refused = 0;
		var inputs = All("ab:", 6).ToArray();
		var placed = 0;
		var placedGrammars = new HashSet<string>();

		while (tried < 40)
		{
			var grammar = RandomGrammar(random);
			var result  = GramCompiler.Compile(grammar, new GramCompilerOptions { BufferedInput = true, BufferedBytes = true, CSharpScanner = RoslynCSharpScanner.Instance });

			if (result.Diagnostics.Any(static one => one.Severity == GramSeverity.Error))
			{
				refused++;

				if (refused > 400)
					break;

				continue;
			}

			tried++;

			foreach (var direct in new[] { true, false })
			{
				var compiled = Compile(grammar, direct);

				foreach (var input in inputs)
				{
					var whole = Whole(compiled.Assembly, "TryParseR", input);
					var bytes = Encoding.ASCII.GetBytes(input);

					void Check(string got, string how)
					{
						if (got == whole)
							return;

						// A refusal placed elsewhere is counted apart; any other difference is a break.
						if (got.StartsWith("no @", StringComparison.Ordinal) && whole.StartsWith("no @", StringComparison.Ordinal))
						{
							placed++;

							if (placedGrammars.Add(grammar))
								Info($"  placed apart: [{grammar.Replace("\n", " ⏎ ")}] {(direct ? "reader" : "engine")} {how} \"{input}\": string {whole} / {got}");

							return;
						}

						told.Add($"[{grammar.Replace("\n", " ⏎ ")}] {(direct ? "reader" : "engine")} {how} \"{input}\": string {whole} / {got}");
					}

					foreach (var size in new[] { 1, 2, 3 })
					{
						Check(Reader(compiled.Assembly, "TryParseR", input, size), $"reader/{size}");
						Check(Streamed(compiled.Assembly, "TryParseR", bytes, size), $"stream/{size}");
					}

					Check(Held(compiled.Assembly, "TryParseR", bytes), "byte[]");
					Check(Memory(compiled.Assembly, "TryParseR", bytes, 2), "memory+2");
				}
			}
		}

		Info($"random seed {seed}: {tried} grammars run, {refused} refused at build, {told.Count} disagreements, {placed} refusals placed apart in {placedGrammars.Count} grammars");
		Report(told);
	}
}
