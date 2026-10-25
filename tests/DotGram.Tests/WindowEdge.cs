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
	public static Parser? Compiled(string grammar, string? members = null)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance,
		});

		return result.Diagnostics.Any(static one => one.Severity == GramSeverity.Error)
			? null
			: new Parser(Assert.Single(result.Sources).Text, "Grammar", null, members, []);
	}

	/// <summary>
	/// One streamed form, read over a window that holds the whole input and then with the window's end at
	/// every offset: the first is held to the whole input's form in values, outcome and where a refusal
	/// stands (the two word a refusal their own way), and every other reading to the first (<see cref="Same"/>).
	/// </summary>
	public static void Edges(
		Parser parser, Mismatches found, string form, string shown, int length, string whole, Func<object?> read,
		Func<string, string>? kept = null, string? beforeNul = null)
	{
		kept ??= static one => one;

		parser.Capacity = length + 16;

		var held = kept(Said(read));

		found.Runs++;

		if (!Agrees(whole, held, beforeNul))
			found.Add("the form over the whole input", form, shown, length + 16, whole, held);

		foreach (var capacity in Capacities(length))
		{
			parser.Capacity = capacity;

			var said = kept(Said(read));

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
		catch (Exception thrown)
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

	/// <summary>A scanner that takes the failure, from its signature to the closing brace at its own indent.</summary>
	static readonly Regex ScannerMethod = new(
		@"^(?<indent>\t+)static int (?<name>Scan_\w+)\(global::System\.ReadOnlySpan<char> text, int pos, ref Failure failure[^\n]*\n(?<body>.*?)\n\k<indent>\}",
		RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant);

	/// <summary>A scanner of a run of characters that takes the failure.</summary>
	static readonly Regex ScalarMethod = new(
		@"^(?<indent>\t+)static int (?<name>Read_\w+)\([^\n]*ref Failure failure\)[^\n]*\n(?<body>.*?)\n\k<indent>\}",
		RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant);

	/// <summary>A generated parser whose window's capacity is a field the oracle sets.</summary>
	public sealed class Parser
	{
		public Parser(string source, string className, string? @namespace, string? members, string[] parts)
		{
			Source  = source;
			Streams = source.Contains(Window, StringComparison.Ordinal);

			var assembly = EmittedCode.Compile(
				source.Replace(Window, "new Window_DotGram(input, " + CapacityField + ")", StringComparison.Ordinal),
				className, @namespace, (members ?? "") + $" internal static int {CapacityField} = 4096;", parts);

			Type = assembly.GetType(@namespace is null ? className : @namespace + "." + className)!;
		}

		public string Source { get; }

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
			var found = Type.GetMethod(method, [over]) ?? throw new InvalidOperationException($"No {method}({over.Name}).");

			return found.Invoke(null, [input]);
		}
	}

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
