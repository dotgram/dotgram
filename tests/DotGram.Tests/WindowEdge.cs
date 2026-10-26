using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The edge oracle: a streamed form read with the window's end at every offset of its input, held to the
/// whole input's form (<c>WindowEdgeTests</c>, and the sweep over shapes in DotGram.Tests.Slow).
/// </summary>
/// <remarks>
/// The window is the generated one, and its capacity, which the generated code fixes, is made a field the
/// oracle sets before each reading. The window grows only when an element is longer than it, so a small
/// capacity puts its end inside the first elements at every offset they read to, and its later ends fall
/// across the rest.
/// </remarks>
static class WindowEdge
{
	/// <summary>The field the window's capacity is read from in the source the oracle compiles.</summary>
	const string CapacityField = "EdgeCapacity_Test";

	const string Window = "new Window_DotGram(input, 4096)";

	/// <summary>Where <see cref="Said"/> writes how a reading was refused.</summary>
	const string Refused = "refused: ";

	/// <summary>The parser of <paramref name="grammar"/>, which must compile.</summary>
	public static Parser Compile(string grammar, string? members = null)
	{
		return Compiled(grammar, members) ?? throw new InvalidOperationException("The grammar does not compile.");
	}

	/// <summary>The parser of <paramref name="grammar"/>, or none where the compiler refuses it.</summary>
	/// <param name="split">
	/// Whether the generated code is to be split into parts, as a large grammar's is: what reads the
	/// generated code has to read every part (the engine may stand in one).
	/// </param>
	public static Parser? Compiled(string grammar, string? members = null, bool split = false)
	{
		var options = new GramCompilerOptions
		{
			ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance,
		};

		if (split)
		{
			options.SourceFileSize = 1;
			options.PartSize       = 1;
		}

		var result = GramCompiler.Compile(grammar, options);

		return result.Diagnostics.Any(static one => one.Severity == GramSeverity.Error)
			? null
			: new Parser(result.Sources[0].Text, "Grammar", null, members, [.. result.Sources.Skip(1).Select(static one => one.Text)]);
	}

	/// <summary>
	/// One streamed form, read over a window that holds the whole input and then with the window's end at
	/// every offset: the first is held to the whole input's form in values, outcome and where a refusal
	/// stands (the two word a refusal their own way), and every other reading to the first (<see cref="Same"/>).
	/// </summary>
	public static void Edges(
		Parser parser, Mismatches found, string form, string shown, int length, string whole, Func<object?> read,
		string? beforeNul = null)
	{
		parser.Capacity = length + 16;

		var held = Said(read);

		found.Runs++;

		if (!Agrees(whole, held, beforeNul))
			found.Add("the form over the whole input", form, shown, length + 16, whole, held);

		foreach (var capacity in Capacities(length))
		{
			parser.Capacity = capacity;

			var said = Said(read);

			found.Runs++;

			if (!Same(held, said, beforeNul))
				found.Add("the window's end", form, shown, capacity, held, said);
		}
	}

	/// <summary>
	/// Every capacity up to a little past the text, which puts the window's end at every offset of it, and
	/// the capacity the generated code chose.
	/// </summary>
	static IEnumerable<int> Capacities(int length)
	{
		for (var capacity = 1; capacity <= Math.Min(length + 2, 160); capacity++)
			yield return capacity;

		yield return 4096;
	}

	/// <summary>
	/// Whether a reading over a window that ends sooner said what the reading over a window holding the
	/// whole input said: exactly, with one exception. Read a segment at a time, a source that holds a NUL is
	/// refused when it is read that far, so a smaller window may hand over more elements before that same
	/// refusal. Those values may only extend what the larger window handed over, and they have to be what
	/// the list form reads of the segments before the NUL (<paramref name="beforeNul"/>): a value a stream
	/// hands over before reading further is one no continuation can change, the end there included.
	/// </summary>
	public static bool Same(string held, string said, string? beforeNul)
	{
		if (said == held)
			return true;

		if (beforeNul is null || !RefusesNul(held) || Refusal(held) != Refusal(said))
			return false;

		var before = Values(held);
		var after  = Values(said);

		return after.StartsWith(before, StringComparison.Ordinal) && Values(beforeNul).StartsWith(after, StringComparison.Ordinal);
	}

	public static bool Agrees(string whole, string streamed, string? beforeNul)
	{
		// A segment of the reader is a line of the list's, and its refusal of a NUL says so in its own words.
		streamed = streamed.Replace("A line holds", "A segment holds", StringComparison.Ordinal);

		if (Refusal(whole) is not { } refused)
			return streamed == whole;

		// Where the whole input is refused, a streamed form hands over what it read before the refusal and
		// the whole form nothing: what is held is the refusal and where it stands — and, where the refusal is
		// of a NUL in the source, that what was handed over is what the list reads of the segments before it.
		// Any other refusal of the whole input has no such list to hold the values to.
		if (Refusal(streamed) is not { } also || Where(refused) != Where(also))
			return false;

		return !RefusesNul(streamed) || beforeNul is null || Values(beforeNul).StartsWith(Values(streamed), StringComparison.Ordinal);
	}

	/// <summary>Whether a reading ended in the refusal of a NUL its source holds.</summary>
	static bool RefusesNul(string said)
	{
		return Refusal(said) is { } refusal && refusal.Contains(" holds U+0000", StringComparison.Ordinal);
	}

	/// <summary>What a reading handed over, without how it ended.</summary>
	static string Values(string said)
	{
		var at = said.IndexOf(Refused, StringComparison.Ordinal);

		return at < 0 ? said : said[..at];
	}

	static string? Refusal(string said)
	{
		var at = said.IndexOf(Refused, StringComparison.Ordinal);

		return at < 0 ? null : said[(at + Refused.Length)..];
	}

	static string Where(string refusal)
	{
		var at = Place.Matches(refusal);

		return at.Count == 0 ? refusal : at[^1].Value;
	}

	static readonly Regex Place = new(@"\bat (line \d+, column \d+|\d+)", RegexOptions.CultureInvariant);

	/// <summary>What a reading said: every value it handed over, and how it ended.</summary>
	public static string Said(Func<object?> read)
	{
		var said = new StringBuilder();

		try
		{
			var value = read();

			if (value is IEnumerable values and not string)
			{
				foreach (var one in values)
					said.Append(Describe(one, 0)).Append(" | ");
			}
			else
				said.Append(Describe(value, 0));
		}
		catch (Exception thrown) when (thrown is not MissingFormException)
		{
			var inner = thrown is TargetInvocationException { InnerException: { } cause } ? cause : thrown;

			said.Append(Refused).Append(inner.GetType().Name).Append(": ").Append(inner.Message);
		}

		return said.ToString();
	}

	/// <summary>A value as its public properties, so that a match's position and length are held as well as what it read.</summary>
	static string Describe(object? value, int depth)
	{
		switch (value)
		{
			case null:
				return "null";

			case string text:
				return "\"" + Escaped(text) + "\"";

			case IEnumerable items:
				return "[" + string.Join(", ", items.Cast<object?>().Select(one => Describe(one, depth + 1))) + "]";
		}

		var type = value.GetType();

		if (type.IsPrimitive || type.IsEnum || value is decimal || depth > 4)
			return Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)!;

		var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Where(static one => one.GetIndexParameters().Length == 0)
			.OrderBy(static one => one.Name, StringComparer.Ordinal)
			.ToArray();

		if (properties.Length == 0)
			return value.ToString()!;

		return "{" + string.Join(", ", properties.Select(one => one.Name + "=" + Describe(one.GetValue(value), depth + 1))) + "}";
	}

	public static string Show(string[] segments)
	{
		return "[" + string.Join(", ", segments.Select(static one => "\"" + Escaped(one) + "\"")) + "]";
	}

	static string Escaped(string text)
	{
		return text.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\0", "\\0");
	}

	/// <summary>
	/// The scanners of <paramref name="source"/> told of the window whose refusal does not say whether it
	/// reached the end, and how many scanners told of the window there are.
	/// </summary>
	public static (int Scanners, IReadOnlyList<string> Untold) Untold(string source)
	{
		var scanners = 0;
		var untold   = new List<string>();

		foreach (Match scanner in ScannerMethod.Matches(source))
		{
			var body = scanner.Groups["body"].Value;
			var at   = body.IndexOf("Refuse:", StringComparison.Ordinal);

			if (at < 0)
				continue;

			scanners++;

			if (!body[at..].Contains("failure.Starved = true;", StringComparison.Ordinal))
				untold.Add(scanner.Groups["name"].Value);
		}

		foreach (Match scalar in ScalarMethod.Matches(source))
		{
			scanners++;

			// The first refusal, before anything is read, is the one at the end of the text.
			var body  = scalar.Groups["body"].Value;
			var first = body.IndexOf("return -1;", StringComparison.Ordinal);

			if (first < 0 || !body[..first].Contains("failure.Starved = true;", StringComparison.Ordinal))
				untold.Add(scalar.Groups["name"].Value);
		}

		return (scanners, untold);
	}

	/// <summary>
	/// Every test of the end of the text in the code a window feeds that does not say, on its own line, that
	/// it ran out: the methods the streamed forms call on the window, and every method they call in turn.
	/// </summary>
	/// <remarks>
	/// The generator writes such a test through one of a few spellings that set <c>Starved</c> on the side
	/// where the end is met (<c>… &amp;&amp; (failure.Starved = true)</c>, <c>… || !(failure.Starved = true)</c>).
	/// What is left over is listed in <see cref="Unflagging"/>, each with why it may stand.
	/// </remarks>
	/// <returns>How many tests of the end were looked at, and those that do not say it.</returns>
	public static (int Examined, IReadOnlyList<string> Unflagged) Unflagged(string source)
	{
		source = source.Replace("\r\n", "\n", StringComparison.Ordinal);

		var methods = new Dictionary<string, List<(string Signature, string Body)>>(StringComparer.Ordinal);

		foreach (Match method in AnyMethod.Matches(source))
		{
			var name = method.Groups["name"].Value;

			if (!methods.TryGetValue(name, out var bodies))
				methods[name] = bodies = [];

			bodies.Add((name + "(" + method.Groups["parameters"].Value + ")", method.Groups["body"].Value));
		}

		var reached = new HashSet<string>(StringComparer.Ordinal);
		var pending = new Queue<string>();

		foreach (Match call in WindowCall.Matches(source))
		{
			if (reached.Add(call.Groups["name"].Value))
				pending.Enqueue(call.Groups["name"].Value);
		}

		while (pending.Count > 0)
		{
			if (!methods.TryGetValue(pending.Dequeue(), out var bodies))
				continue;

			foreach (var (_, body) in bodies)
			{
				foreach (Match call in Called.Matches(body))
				{
					var name = call.Groups["name"].Value;

					if (methods.ContainsKey(name) && reached.Add(name))
						pending.Enqueue(name);
				}
			}
		}

		Assert.True(reached.Count > 0, "no method is called on a window");

		var unflagged = new List<string>();
		var examined  = 0;

		foreach (var name in reached.OrderBy(static one => one, StringComparer.Ordinal))
		{
			if (!methods.TryGetValue(name, out var bodies))
				continue;

			foreach (var line in bodies.Where(static method => !Unflagging.Any(one => one.Method?.IsMatch(method.Signature) == true))
				.SelectMany(static method => method.Body.Split('\n')).Select(static line => line.Trim()))
			{
				if (!line.Contains("text.Length", StringComparison.Ordinal))
					continue;

				examined++;

				if (line.Contains("failure.Starved = true", StringComparison.Ordinal))
					continue;

				if (Unflagging.Any(one => one.Line?.IsMatch(line) == true))
					continue;

				unflagged.Add(name + ": " + line);
			}
		}

		return (examined, unflagged);
	}

	/// <summary>
	/// What may read <c>text.Length</c> in the code a window feeds without saying that it ran out, and why.
	/// </summary>
	public static readonly (Regex? Method, Regex? Line, string Why)[] Unflagging =
	[
		// By the name and the parameters the generator gives these helpers, which no rule's method can
		// take: a name with _DotGram in it is refused in a grammar (GRAM1012), and a rule named
		// Ahead_Agreeing is the scanner Scan_DotGram_Ahead_Agreeing, read like any other.
		(new(@"^(RecognizeAgreeing\w*_DotGram\w*\(\S+ text, int p, string value, bool fold\)|RecognizeSharpen\w*_DotGram\w*\(\S+ text, int p, ref string\[\]\? expected\)|Reach\w*_DotGram\w*\(\S+ text, int pos, global::System\.ReadOnlySpan<char> want\))$"), null,
			"a walk that measures how far a literal agrees; the caller that refuses on it says whether it reached the end"),
		(new(@"^(Prefix\w*\d+_DotGram\w*\(\S+ text, int p\)|PrefixMiss\w*\d+_DotGram\w*\(\S+ text, ref int p, int row, int lookahead, ref Failure failure, out string\[\] expected\))$"), null,
			"the dispatch of a choice by the prefixes of its literals; where the text runs out it answers that nothing is decided, and the alternatives it falls back to test the end themselves"),
		(null, new(@"^if \(whole && p != text\.Length\)"),
			"the end a whole reading demands; a window's reading is never whole"),
		(null, new(@"^if \(\(uint\)(?<at>reached|literalReach) < \(uint\)text\.Length && .*\btext\[\k<at>\]"),
			"a rung of the walk of a literal the text ends inside, written in place (Machine.ReachLadder): it reads the character it bounds; the flagged test that sent the code there said it ran out, or the walk raises how far the scan got, which the exits read"),
		(null, new(@"^if \(text\.Length > furthest\) furthest = text\.Length;|^if \(found\d+ < 0\) \{ if \(text\.Length > furthest\) furthest = text\.Length;"),
			"a scanner raising how far it got to the end, which both of its exits read and say"),
		(null, new(@"^p = found\d+ < 0 \? text\.Length : p \+ found\d+;$"),
			"a run that searched to the end stands there; whatever reads next tests the end, and inside a look the line after says it"),
	];

	/// <summary>A method's signature at the start of a line, its body, and the brace at its own indent.</summary>
	static readonly Regex AnyMethod = new(
		@"^(?<indent>\t+)(?:(?:private|internal|public) )?static [^\n=;]*?\b(?<name>\w+)\((?<parameters>[^\n]*)\)\n\k<indent>\{\n(?<body>.*?)\n\k<indent>\}$",
		RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant);

	/// <summary>A method a streamed form calls on what the window holds.</summary>
	static readonly Regex WindowCall = new(@"\b(?<name>\w+)\(window\.Span\(\)", RegexOptions.CultureInvariant);

	static readonly Regex Called = new(@"\b(?<name>\w+)\(", RegexOptions.CultureInvariant);

	/// <summary>A scanner that takes the failure, from its signature to the closing brace at its own indent.</summary>
	static readonly Regex ScannerMethod = new(
		@"^(?<indent>\t+)static int (?<name>Scan\w*_DotGram\w*)\(global::System\.ReadOnlySpan<char> text, int pos, ref Failure failure[^\n]*\n(?<body>.*?)\n\k<indent>\}",
		RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant);

	/// <summary>A scanner of a run of characters that takes the failure.</summary>
	static readonly Regex ScalarMethod = new(
		@"^(?<indent>\t+)static int (?<name>Read\w*_DotGram\w*)\([^\n]*ref Failure failure\)[^\n]*\n(?<body>.*?)\n\k<indent>\}",
		RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant);

	/// <summary>A generated parser whose window's capacity is a field the oracle sets.</summary>
	public sealed class Parser
	{
		/// <param name="parts">What the generator wrote beyond the first file, where it split the code.</param>
		public Parser(string source, string className, string? @namespace, string? members, string[] parts)
		{
			// Every part, in order: the engine of a split grammar stands in a part, not in the first file.
			Source  = string.Concat([source, .. parts]);
			Files   = 1 + parts.Length;
			Streams = Source.Contains(Window, StringComparison.Ordinal);

			var windowed = "new Window_DotGram(input, " + CapacityField + ")";
			var assembly = EmittedCode.Compile(
				source.Replace(Window, windowed, StringComparison.Ordinal),
				className, @namespace, (members ?? "") + $" internal static int {CapacityField} = 4096;",
				[.. parts.Select(one => one.Replace(Window, windowed, StringComparison.Ordinal))]);

			Type = assembly.GetType(@namespace is null ? className : @namespace + "." + className)!;
		}

		public string Source { get; }

		/// <summary>How many files the generator wrote the code in.</summary>
		public int Files { get; }

		public bool Streams { get; }

		public Type Type { get; }

		public int Capacity
		{
			set { Type.GetField(CapacityField, BindingFlags.NonPublic | BindingFlags.Static)!.SetValue(null, value); }
		}

		public bool Has(string method, Type over)
		{
			return Type.GetMethod(method, [over]) is not null;
		}

		public object? Call(string method, Type over, object input)
		{
			// A form that is not there is the harness's mistake, and must not read as a refusal both sides agree on.
			var found = Type.GetMethod(method, [over]) ?? throw new MissingFormException($"No {method}({over.Name}).");

			return found.Invoke(null, [input]);
		}
	}

	/// <summary>A form the oracle asked for that the parser does not have: the harness's mistake, never a refusal.</summary>
	sealed class MissingFormException(string message) : Exception(message);

	/// <summary>What disagreed, by kind and form, with the first few cases of each.</summary>
	public sealed class Mismatches
	{
		readonly Dictionary<(string Kind, string Form), (int Count, StringBuilder Cases)> _found = [];

		public int Runs { get; set; }

		public void Add(string kind, string form, string shown, int capacity, string expected, string actual)
		{
			var (count, cases) = _found.TryGetValue((kind, form), out var known) ? known : (0, new StringBuilder());

			if (count < 3)
			{
				cases.AppendLine($"    a window of {capacity}: {shown}");
				cases.AppendLine($"      expected: {expected}");
				cases.AppendLine($"      actual  : {actual}");
			}

			_found[(kind, form)] = (count + 1, cases);
		}

		public void Assert()
		{
			var report = new StringBuilder();

			foreach (var ((kind, form), (count, cases)) in _found.OrderBy(static one => one.Key.Kind, StringComparer.Ordinal))
			{
				report.AppendLine($"{form}: {count} of {Runs} readings disagree with {kind}");
				report.Append(cases);
			}

			Xunit.Assert.True(Runs > 0, "nothing streamed was read");
			Xunit.Assert.True(report.Length == 0, report.ToString());
		}
	}
}
