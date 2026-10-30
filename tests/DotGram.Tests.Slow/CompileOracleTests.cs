using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every grammar is answered with a parser that compiles or with a diagnostic, over grammars nobody
/// wrote on purpose.
/// </summary>
/// <remarks>
/// <para>
/// <c>FuzzTests</c> holds the generator to answering a mutated text at all; this holds what it
/// answers to the C# compiler. A grammar the generator takes and writes C# for that does not build
/// fails the consumer's build in code they did not write and cannot fix, which is worse than a
/// refusal that names the construct. The defects of that kind came from features meeting — a
/// recognizer of the host in a recovery's synchronization read over a buffer, a guard inside a
/// lookahead reading a capture made before it, a repetition that recovers captured whole — so the
/// grammars are built from the features, at random, a few rules at a time, and compiled under every
/// option that changes what is written: methods or the engine, characters or tokens, a trace build,
/// a source split in parts, the buffered and byte forms, spans for captures, and the tape.
/// </para>
/// <para>
/// The C# a grammar carries is written to compile in every form it may be read in: a raw capture
/// is a <c>string</c>, a <c>ReadOnlySpan&lt;char&gt;</c> or a <c>ReadOnlySpan&lt;byte&gt;</c>, and it
/// is only ever asked its <c>Length</c> or handed to an overload the host declares for each. So a
/// CS error in the generated files is the generator's; one in the host's own declaration is the
/// harness's, and fails as that.
/// </para>
/// <para>
/// Seeded: a failure names its seed and its round, and reads the same again. Compiled, not loaded:
/// nothing here runs a parser, so no assembly is kept.
/// </para>
/// </remarks>
public sealed class CompileOracleTests
{
	/// <summary>Grammars a seed makes. The environment variable sets another count, for a longer run by hand.</summary>
	static int Rounds
	{
		get
		{
			return int.TryParse(Environment.GetEnvironmentVariable("DOTGRAM_ORACLE_ROUNDS"), out var rounds) ? rounds : 60;
		}
	}

	/// <summary>How long one grammar may take to be compiled and its C# read, on a loaded machine.</summary>
	static readonly TimeSpan Budget = TimeSpan.FromSeconds(
		Environment.GetEnvironmentVariable("DOTGRAM_ORACLE_MODES") == "all" ? 600 : 120);

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(3)]
	[InlineData(4)]
	[InlineData(5)]
	[InlineData(6)]
	public void A_random_grammar_compiles_or_is_refused(int seed)
	{
		var outcome = Run(seed, Rounds, TestContext.Current.CancellationToken);

		TestContext.Current.SendDiagnosticMessage(outcome.Summary());

		// Every failure in full, for a longer run by hand that wants more than the first five.
		if (Environment.GetEnvironmentVariable("DOTGRAM_ORACLE_LOG") is { Length: > 0 } log)
			System.IO.File.AppendAllText(log, $"### seed {seed}: {outcome.Summary()}\n\n" + string.Join("\n\n", outcome.Failures) + "\n\n");

		Assert.True(outcome.Failures.Count == 0, outcome.Summary() + "\n\n" + string.Join("\n\n", outcome.Failures.Take(5)));
	}

	/// <summary>What one seed's grammars came to.</summary>
	internal sealed class Outcome
	{
		public int Compiled;

		public int Refused;

		public readonly Dictionary<string, int> Refusals = new(StringComparer.Ordinal);

		public readonly Dictionary<string, int> Errors = new(StringComparer.Ordinal);

		public readonly List<string> Failures = [];

		public string Summary()
		{
			return $"{Compiled} compiled, {Refused} refused ({string.Join(", ", Refusals.OrderByDescending(static one => one.Value).Take(8).Select(static one => one.Key + " " + one.Value))}), " +
				$"{Failures.Count} failed ({string.Join(", ", Errors.OrderByDescending(static one => one.Value).Select(static one => one.Key + " " + one.Value))})";
		}
	}

	internal static Outcome Run(int seed, int rounds, CancellationToken cancellation)
	{
		var outcome = new Outcome();
		var cases   = new List<Case>();
		var random  = new Random(seed);

		for (var round = 0; round < rounds; round++)
			cases.Add(new Builder(random, seed, round).Build());

		var results = new string?[cases.Count];
		var lockObj = new object();

		Parallel.For(0, cases.Count, new ParallelOptions { MaxDegreeOfParallelism = 4, CancellationToken = cancellation }, index =>
		{
			var one = cases[index];

			try
			{
				// A generator that does not come back is a failure of its own, said with the grammar:
				// the compile is left running, since nothing can stop it, and the run goes on.
				var work = System.Threading.Tasks.Task.Run(() => Compile(one, cancellation), cancellation);

				if (!work.Wait(Budget, cancellation))
				{
					lock (lockObj)
					{
						outcome.Errors["hang"] = outcome.Errors.TryGetValue("hang", out var hung) ? hung + 1 : 1;
						results[index] = one.Describe() + $"\nno answer in {Budget.TotalSeconds} s";
					}

					return;
				}

				var (refusal, complaints, failing) = work.Result;

				lock (lockObj)
				{
					if (refusal is not null)
					{
						outcome.Refused++;
						outcome.Refusals[refusal] = outcome.Refusals.TryGetValue(refusal, out var count) ? count + 1 : 1;
					}
					else if (complaints.Length == 0)
					{
						outcome.Compiled++;
					}
					else
					{
						foreach (var id in complaints.Select(static one => one.Id).Distinct())
							outcome.Errors[id] = outcome.Errors.TryGetValue(id, out var count) ? count + 1 : 1;

						results[index] = failing.Describe() + "\n" + string.Join("\n", complaints.Take(4).Select(static one => one.ToString()));
					}
				}
			}
			catch (Exception thrown)
			{
				var failure = thrown is AggregateException { InnerExceptions.Count: 1 } single ? single.InnerExceptions[0] : thrown;

				lock (lockObj)
				{
					outcome.Errors["crash"] = outcome.Errors.TryGetValue("crash", out var count) ? count + 1 : 1;
					results[index] = one.Describe() + "\n" + failure;
				}
			}
		});

		outcome.Failures.AddRange(results.OfType<string>());

		return outcome;
	}

	/// <summary>
	/// The grammar compiled under each of its modes: by methods and on the engine always, and under every
	/// combination of a trace build, the tape and a split source where the environment says <c>all</c>.
	/// Refused where every mode refused it; failed where any mode wrote C# that does not compile, which
	/// is then the case the failure names.
	/// </summary>
	/// <remarks>
	/// The engine on its own: a defect of the engine's that the methods never reach was missed by a run
	/// that drew one of the two.
	/// </remarks>
	static (string? Refusal, Diagnostic[] Complaints, Case Failing) Compile(Case one, CancellationToken cancellation)
	{
		var all   = Environment.GetEnvironmentVariable("DOTGRAM_ORACLE_MODES") == "all";
		var modes = new List<Case>();

		foreach (var direct in new[] { true, false })
			if (all)
			{
				for (var mode = 0; mode < 8; mode++)
					modes.Add(one with { Direct = direct, Trace = (mode & 1) != 0, Tape = (mode & 2) != 0, Split = (mode & 4) != 0 });
			}
			else
			{
				modes.Add(one with { Direct = direct });
			}

		string? refusal = null;
		var compiled    = false;

		foreach (var mode in modes)
		{
			var (refused, complaints) = CompileOne(mode, cancellation);

			if (complaints.Length > 0)
				return (null, complaints, mode);

			if (refused is null)
				compiled = true;
			else
				refusal ??= refused;
		}

		return (compiled ? null : refusal, [], one);
	}

	/// <summary>
	/// The grammar compiled once: the first error the generator said, or every CS error and warning in what
	/// it wrote. A warning counts, since generated code lands in builds that treat warnings as errors.
	/// </summary>
	static (string? Refusal, Diagnostic[] Complaints) CompileOne(Case one, CancellationToken cancellation)
	{
		var members = Host.Members(one.Text);
		var host    = CSharpCompilation.Create(
			"Host",
			[CSharpSyntaxTree.ParseText($"public partial class Grammar {{ {members} }}", cancellationToken: cancellation)],
			EmittedCode.References);

		var result = GramCompiler.Compile(one.Text, new GramCompilerOptions
		{
			ClassName      = "Grammar",
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(host, "Grammar"),
			Direct         = one.Direct,
			Lexical        = one.Lexical,
			Trace          = one.Trace,
			SourceFileSize = one.Split ? 4000 : 0,
			Carrier        = one.Tape ? CarrierKind.Tape : CarrierKind.Auto,
			BufferedInput  = one.Buffered,
			BufferedBytes  = one.Bytes,
			SpanCaptures   = one.Spans,
			LocationType   = one.PerCall ? "Grammar.ILoc" : null,
			LocatedFacade  = one.PerCall ? "Located" : null,
		});

		var error = result.Diagnostics.FirstOrDefault(static diagnostic => diagnostic.Severity == GramSeverity.Error);

		if (error is not null)
		{
			if (Environment.GetEnvironmentVariable("DOTGRAM_ORACLE_REFUSALS") is { Length: > 0 } log)
				lock (Refusing)
					System.IO.File.AppendAllText(log, one.Describe() + "\n" + error.Id + ": " + error.Message + "\n\n");

			return (error.Id, []);
		}

		var complaints = EmittedCode.Complaints(result.Sources[0].Text, "Grammar", members, result.Sources.Skip(1).Select(static source => source.Text), cancellation);

		return (null, complaints);
	}

	static readonly object Refusing = new();

	/// <summary>One grammar and the options it is compiled under.</summary>
	internal sealed record Case(int Seed, int Round, string Text, bool Direct, bool Lexical, bool Trace, bool Split, bool Tape, bool Buffered, bool Bytes, bool Spans, bool PerCall)
	{
		public string Describe()
		{
			return $"seed {Seed}, round {Round}: Direct={Direct} Lexical={Lexical} Trace={Trace} Split={Split} Tape={Tape} " +
				$"BufferedInput={Buffered} BufferedBytes={Bytes} SpanCaptures={Spans} PerCall={PerCall}\n--- grammar ---\n{Text}\n---";
		}
	}

	/// <summary>
	/// What the grammars may call in the host: recognizers over spans, over the input view, with a value
	/// and with the context, each for characters and for bytes, the text helper the constructions
	/// hand a raw capture to, and a value told where it was written.
	/// </summary>
	static class Host
	{
		const string Always = """
			public sealed class Ctx { public int N; }
			public interface ILoc { void Locate(int at, int length); void Locate(int at, int length, int gapStart); }
			public sealed class Loc : ILoc { public int At; public void Locate(int at, int length) { At = at; } public void Locate(int at, int length, int gapStart) { At = at; } }
			static bool Odd(System.ReadOnlySpan<char> input, ref int p) { if (p < input.Length && input[p] == 'o') { p++; return true; } return false; }
			static bool Odd(System.ReadOnlySpan<byte> input, ref int p) { if (p < input.Length && input[p] == (byte)'o') { p++; return true; } return false; }
			static bool Num(System.ReadOnlySpan<char> input, ref int p, out int value) { value = 0; while (p < input.Length && input[p] >= '0' && input[p] <= '9') { value++; p++; } return value > 0; }
			static bool Num(System.ReadOnlySpan<byte> input, ref int p, out int value) { value = 0; while (p < input.Length && input[p] >= (byte)'0' && input[p] <= (byte)'9') { value++; p++; } return value > 0; }
			public static bool Gen<T>(System.ReadOnlySpan<char> input, ref int p, out int value) { value = typeof(T).Name.Length; return true; }
			static string S(string text) { return text; }
			static string S(System.ReadOnlySpan<char> text) { return text.ToString(); }
			static string S(System.ReadOnlySpan<byte> text) { return System.Text.Encoding.ASCII.GetString(text.ToArray()); }
			""";

		const string Views = """
			static bool Peek(ParserInput<char> input, ref int p) { if (input.Peek(p, out var c) && c == 'p') { p++; return true; } return false; }
			static bool Peek(ParserInput<byte> input, ref int p) { if (input.Peek(p, out var c) && c == (byte)'p') { p++; return true; } return false; }
			static bool Sized(ParserInput<char> input, ref int p, Ctx context) { return input.TryAdvance(ref p, context.N); }
			static bool Sized(ParserInput<byte> input, ref int p, Ctx context) { return input.TryAdvance(ref p, context.N); }
			""";

		public static string Members(string grammar)
		{
			// The input view is a type the generator writes only for a grammar that calls through it.
			return grammar.Contains("@Peek", StringComparison.Ordinal) || grammar.Contains("@Sized", StringComparison.Ordinal)
				? Always + Views
				: Always;
		}
	}

	/// <summary>What a capture holds, which decides what C# may be asked of it.</summary>
	enum Held { Text, Int, Many }

	/// <summary>One random grammar.</summary>
	sealed class Builder(Random random, int seed, int round)
	{
		readonly List<string> _rules = [];

		/// <summary>The result of each rule <c>R1</c>…, null for one that is its text.</summary>
		readonly List<string?> _types = [];

		bool _segments;

		bool _context;

		bool _lexical;

		/// <summary>Whether the grammar declares a <c>wordboundary</c>, under which a keyword looks behind where it begins.</summary>
		bool _words;

		/// <summary>Whether a door that locates is decided per call, beside rules whose values are told where they were written.</summary>
		bool _perCall;

		/// <summary>Whether a form over a buffer or bytes is asked for, which a recognizer over a span refuses.</summary>
		bool _buffered;

		/// <summary>How many lookaheads the operand being drawn stands in.</summary>
		int _looking;

		/// <summary>The bindings drawn so far, each named apart.</summary>
		int _bound;

		int _captures;

		bool Chance(int percent)
		{
			return random.Next(100) < percent;
		}

		T Pick<T>(params T[] items)
		{
			return items[random.Next(items.Length)];
		}

		public Case Build()
		{
			_segments = Chance(12);
			_lexical  = !_segments && Chance(20);
			_context  = Chance(15);
			_buffered = !_segments && Chance(35);

			var count = 1 + random.Next(4);

			// Decided before the bodies, so that a rule may call one written after it: the calls go only to
			// higher numbers, which keeps the grammars clear of left recursion through rules (that is
			// GRAM4002's, and says nothing about the C#).
			// A sequence of values needs a rule after it that builds one.
			for (var i = 0; i < count; i++)
				_types.Add(Pick<string?>(null, null, "@int", "@string", "@int[]"));

			for (var i = 0; i < count; i++)
				if (_types[i] == "@int[]" && !_types.Skip(i + 1).Contains("@int"))
					_types[i] = "@int";

			var text = new StringBuilder();

			if (_context)
				text.Append("context : @Ctx\n");

			_words = !_lexical && Chance(20);

			if (_words)
				text.Append("wordboundary = ['a'..'z']\n");

			if (_lexical)
				text.Append("trivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n\tWord = { ['a'..'z']+ }\n\tDigits : @int = { ['0'..'9']+ } => @(parserText.Length)\n}\n");
			else if (Chance(20))
				text.Append(Pick("trivia = ' '*\n", "trivia = { ' '* }\n", "trivia = { (' ' | Std.Nested(\"/*\", \"*/\"))* }\n", "trivia = (' ' | Std.Nested(\"/*\", \"*/\"))*\n"));

			for (var i = count - 1; i >= 0; i--)
				_rules.Insert(0, Rule(i));

			if (_rules.Any(static rule => rule.Contains("Wrap(", StringComparison.Ordinal)))
				_rules.Add("Wrap(item) = '(' & item & ')'");

			if (_rules.Any(static rule => rule.Contains("Twice(", StringComparison.Ordinal)))
				_rules.Add("Twice(item) = item & item");

			// A rule that builds a value and is read only where a recovery synchronizes.
			if (_rules.Any(static rule => rule.Contains("recover Sync", StringComparison.Ordinal)))
				_rules.Add("Sync : @int = n: ['0'..'9']* & ';' => @(n.Length)");

			// Values told where they were written, in a repetition a reader may hand over one at a time.
			_perCall = !_segments && Chance(15);

			if (_perCall)
				_rules.Add(Pick("L0 : @Loc[] = L1*", "L0 : @Loc[] = L1* recover eol", "L0 : @Loc[] = '{' & ls: L1* & '}' => @(ls)") + "\nL1 : @Loc = 'k' & " + Pick("eol", "';' & eol", "['a'..'c']+ & eol") + " => @(new Loc())");

			foreach (var rule in _rules)
				text.Append(rule).Append('\n');

			// Every rule is reached one way or another, so that none is GRAM4018's: a rule no rule before it
			// calls is published under a name of its own.
			text.Append(Publication("R0", null));

			for (var i = 1; i < count; i++)
				if (Chance(35) || !_rules.Take(i).Any(rule => Calls(rule, i)))
					text.Append(Publication($"R{i}", $"P{i}"));

			if (_perCall)
				text.Append(Publication("L0", "PL"));

			return new Case(
				seed, round, text.ToString(),
				Direct: Chance(65), Lexical: _lexical, Trace: Chance(25), Split: Chance(20), Tape: Chance(20),
				Buffered: _buffered && Chance(50), Bytes: _buffered && Chance(40), Spans: Chance(20), PerCall: _perCall);
		}

		/// <summary>Whether a rule is a sequence of the values of one rule and nothing else, which is what may be yielded.</summary>
		bool Yields(string rule)
		{
			return _rules.Exists(one => System.Text.RegularExpressions.Regex.IsMatch(
				one, "^" + rule + @" : @int\[\] = R[0-9]+[*+]( recover [^=]*)?$"));
		}

		static bool Calls(string rule, int callee)
		{
			var name = $"R{callee}";
			var at   = rule.IndexOf('=', StringComparison.Ordinal);

			for (var found = rule.IndexOf(name, at, StringComparison.Ordinal); found >= 0; found = rule.IndexOf(name, found + 1, StringComparison.Ordinal))
				if (found + name.Length == rule.Length || !char.IsDigit(rule[found + name.Length]))
					return true;

			return false;
		}

		string Publication(string rule, string? name)
		{
			var verb = Chance(15) && !_segments ? "find" : "parse";
			var line = new StringBuilder(verb).Append(' ').Append(rule);

			if (name is not null)
				line.Append(" as ").Append(name);

			if (_buffered && Chance(50))
				line.Append(Pick(" stream", " stream bytes"));

			// A sequence handed over as it is read, where the rule is one: a repetition of a rule.
			if (verb == "parse" && name is not null && Yields(rule) && Chance(50))
				line.Append(" yield");

			return line.Append('\n').ToString();
		}

		string Rule(int index)
		{
			var type = _types[index];

			_captures = 0;

			// A rule that says its own refusal is read otherwise on the engine (entered, not inlined),
			// which every analysis of a call there has to know.
			var head = (type is null ? $"R{index}" : $"R{index} : {type}") + (Chance(20) ? $" on fail \"Expected R{index}.\"" : "");

			if (type == "@int[]")
				return $"{head} = {ListBody(index)}";

			var alternatives = new List<string>();

			for (var i = 0; i < 1 + (Chance(30) ? 1 : 0); i++)
				alternatives.Add(Alternative(index, type, top: true));

			var body = string.Join(" | ", alternatives);

			if (index == 0 && Chance(50) && alternatives.Count == 1)
				body = type is null ? $"{body} & {(_segments ? "eos" : "eof")}" : body;

			return $"{head} = {body}";
		}

		/// <summary>A sequence of elements, recovering or not.</summary>
		string ListBody(int index)
		{
			var element = Element(index);
			var end     = _segments ? Pick("eos", "';' & eos") : Chance(50) ? Pick("';'", "eol", "@Odd", "(';' | eol)") : Sync();
			var each    = element ?? Pick("('x' & ';')", "(['a'..'c']+ & ';')");
			var recover = Chance(60) ? $" recover {end}" + (element is not null && Chance(50) ? $" => @({Pick("0", "parserText.Length", "(int)parserPosition", "parserOrdinal")})" : "") : "";

			if (element is null)
				return Chance(50) ? $"x: {each}*{recover} => @(new int[x.Length])" : $"{each}*{recover} => @(new int[0])";

			if (Chance(25))
				return $"items: ({element}*{recover}) & {(_segments ? "eos" : "eof")} => @(new int[items.Length])";

			return $"{element}{Pick("*", "+")}{recover}";
		}

		/// <summary>A rule after this one that builds an <c>@int</c>, or a new one written for the list.</summary>
		string? Element(int index)
		{
			for (var i = index + 1; i < _types.Count; i++)
				if (_types[i] == "@int")
					return $"R{i}";

			return null;
		}

		string Alternative(int index, string? type, bool top)
		{
			var captures = new List<(string Name, Held Held)>();
			var operands = new List<string>();
			var length   = 1 + random.Next(3);

			for (var i = 0; i < length; i++)
			{
				var (operand, held) = Operand(index, 2);

				if (type is not null && top && held is not null && Chance(55))
				{
					var name = "c" + _captures++;

					captures.Add((name, held.Value));
					operands.Add($"{name}: {operand}");

					if (Chance(20))
						operands.Add($"when @({Int(name, held.Value)} >= 0)");
					else if (Chance(10))
						operands.Add($"?=(when @({Int(name, held.Value)} == 0))");
				}
				else
				{
					operands.Add(operand);
				}
			}

			if (_context && Chance(25))
				operands.Add("when @(context.N == 0)");

			if (_segments && index == 0 && Chance(60))
				operands.Add("eos");

			var sequence = string.Join(" & ", operands);

			return type switch
			{
				"@int"    => $"{sequence} => @({IntOf(captures)})",
				"@string" => $"{sequence} => @({StringOf(captures)})",
				_         => sequence,
			};
		}

		static string Int(string name, Held held)
		{
			return held == Held.Int ? name : name + ".Length";
		}

		string IntOf(List<(string Name, Held Held)> captures)
		{
			if (captures.Count == 0)
				return Pick("0", "parserText.Length");

			return string.Join(" + ", captures.Select(static one => Int(one.Name, one.Held)));
		}

		string StringOf(List<(string Name, Held Held)> captures)
		{
			if (captures.Count == 0)
				return Pick("\"\"", "S(parserText)");

			return string.Join(" + ", captures.Select(static one => one.Held switch
			{
				Held.Text => $"S({one.Name})",
				Held.Int  => $"{one.Name}.ToString()",
				_         => $"{one.Name}.Length.ToString()",
			}));
		}

		/// <summary>An operand and what a capture of it would hold, null where none may be taken.</summary>
		(string Operand, Held? Held) Operand(int index, int depth)
		{
			var roll = random.Next(depth > 0 ? 30 : 16);

			switch (roll)
			{
				case 0: return ("'a'", Held.Text);
				case 1: return ("'b'", Held.Text);
				case 2: return ("\"ab\"", Held.Text);
				case 3: return ("['a'..'c']", Held.Text);
				case 4: return ("['a'..'c']+", Held.Text);
				case 5: return ("[^ ';' | '\\n']+", Held.Text);
				case 6: return ("any", Held.Text);
				case 7: return (_segments ? "'e'" : "eol", Held.Text);
				case 8: return (_buffered ? "@Peek" : "@Odd", Held.Text);
				case 9: return !_buffered && Chance(50) ? ("@Num", Held.Int) : ("@Peek", Held.Text);
				case 10: return Call(index);
				case 11: return Call(index);
				case 12: return (_lexical ? Pick("Lexical.Word", "Lexical.Digits") : Pick("'x'i", "\"yz\""), _lexical ? null : Held.Text);
				case 13: return (_context ? "@Sized" : _buffered ? "@Peek" : Chance(50) ? "@Gen<int>" : "@Gen<long>", null);
				case 14: return (_lexical ? "'o'" : Pick("Std.Digits", "Std.Quoted('\"')", "Twice('k')"), _lexical ? Held.Text : null);
				case 15: return ("'d' & when @(true)", null);
			}

			// The operand of a lookahead is drawn knowing it is one: no binding and no recovery there.
			var looks = roll is 21 or 22;

			if (looks)
				_looking++;

			var (inner, held) = Operand(index, depth - 1);

			if (looks)
				_looking--;

			// A body that may read nothing is no body for a repetition (GRAM4001): it is given a
			// character to read, so that the shape under it is still the one that was drawn.
			var body = Empty(inner) ? $"{inner} & 'r'" : inner;

			switch (roll)
			{
				case 16: return ($"({inner})?", null);
				case 17: return ($"({body})*", Repeated(Empty(inner) ? null : held));
				case 18: return ($"({body})+", Repeated(Empty(inner) ? null : held));
				case 19: return ($"({body}){{1,2}}", Repeated(Empty(inner) ? null : held));
				case 20: return ($"{{ {inner} }}", held == Held.Text ? Held.Text : null);
				case 21: return ($"?=({inner})", held);
				case 22: return ($"?!({inner})", null);
				case 23: return ("(?!';' & any)* & ';'", null);
				case 24: return _looking > 0 || _lexical ? (inner, held) : ($"$q{_bound++}: ({inner}) & $q{_bound - 1}", null);
				case 25: return ($"({inner} & {Operand(index, depth - 1).Operand})", null);
				case 26: return ($"({inner} | {Operand(index, depth - 1).Operand})", null);
				case 27: return ($"Twice({body})", null);
				case 28 when _looking == 0:
					return ($"({body})* recover {(_segments ? "eos" : Chance(50) ? Pick("';'", "eol", _buffered ? "@Peek" : "@Odd", "@Peek", "(';' & when @(true))", "Sync") : Sync())}", null);
				default: return (inner, held);
			}
		}

		/// <summary>
		/// A synchronization that reads what a body would: a recognizer of the host, one over the input view
		/// or handed the context, a keyword that looks behind where it begins, a character past what a byte
		/// holds, and a rule that builds. Every question about what a rule reads asks it as it asks a body.
		/// </summary>
		string Sync()
		{
			var picks = new List<string> { "@Peek", "Sync", "'Ā'", "(\"é\" & ';')", "(?=@Peek & eol)" };

			if (!_buffered)
				picks.AddRange(["@Odd", "@Num", "@Gen<int>"]);

			// Handed the context, and drawn where none is declared too: that is refused, as in a body.
			if (_context || Chance(20))
				picks.AddRange(["@Sized", "(@Sized & ';')"]);

			if (_words)
				picks.AddRange(["\"end\"", "(\"end\" & eol)", "(';' | \"end\")"]);

			return Pick([.. picks]);
		}

		/// <summary>Whether a shape drawn may match no input, for all this can tell from its text.</summary>
		static bool Empty(string operand)
		{
			return operand.EndsWith("?", StringComparison.Ordinal) || operand.EndsWith(")*", StringComparison.Ordinal) ||
				operand.StartsWith("?", StringComparison.Ordinal) || operand.Contains("@Gen", StringComparison.Ordinal) ||
				operand.Contains("@Sized", StringComparison.Ordinal) || operand.Contains("recover", StringComparison.Ordinal) ||
				operand.Contains(" | ", StringComparison.Ordinal) || operand.StartsWith("R", StringComparison.Ordinal) ||
				operand.StartsWith("Wrap", StringComparison.Ordinal) || operand.StartsWith("Twice", StringComparison.Ordinal) ||
				operand.StartsWith("{", StringComparison.Ordinal) || operand.StartsWith("(", StringComparison.Ordinal) ||
				operand.Contains("when", StringComparison.Ordinal);
		}

		/// <summary>A repetition of text is text, and one of values their array; one of what is not known is not captured.</summary>
		static Held? Repeated(Held? held)
		{
			return held switch
			{
				Held.Text => Held.Text,
				null      => null,
				_         => Held.Many,
			};
		}

		(string, Held?) Call(int index)
		{
			if (index + 1 >= _types.Count)
				return ("'z'", Held.Text);

			var callee = index + 1 + random.Next(_types.Count - index - 1);
			var held   = _types[callee] switch
			{
				null      => Held.Text,
				"@int"    => Held.Int,
				"@string" => (Held?)null,
				_         => Held.Many,
			};

			if (Chance(15) && _types[callee] is null)
				return ($"Wrap(R{callee})", null);

			return ($"R{callee}", held);
		}
	}
}
