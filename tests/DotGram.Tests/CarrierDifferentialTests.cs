using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// One grammar, one input, every way of running it: the answer must not depend on the carrier or on the
/// entry form the input came in by.
/// </summary>
/// <remarks>
/// <para>
/// A table of grammars is compiled under each carrier (left to choose, immediate, deferred). Every public
/// entry the generator wrote is found by reflection — the string, the lines of a segment grammar, a
/// <see cref="TextReader"/> (as one block, in a small window, and handed over a few characters at a time),
/// a <see cref="Stream"/> and bytes held — and each is called, in every way the publication offers
/// it (<c>Try</c> answering a <c>Match</c>, <c>Parse</c> throwing, the <c>out</c> form, <c>find</c>, a
/// lazy result), over inputs the table lists and over those made from them: empty, the first character
/// gone, cut in the middle, the last character gone, a stray character put in the middle and one put
/// at the end.
/// </para>
/// <para>
/// Two things are held. An entry gives the same answer under every carrier: accepted value or refusal
/// with its place and message, an exception with its type and text, the elements a lazy result handed
/// over before it threw. And the entries of one carrier agree with each other where they answer the
/// same question: the same value, or the same refusal in the same place. What a reader form builds
/// how many times is not asserted: only what it answers.
/// </para>
/// <para>
/// The generator writes no entry over <c>ReadOnlySpan&lt;char&gt;</c>: the characters come in as a string,
/// a sequence of strings or a reader, and the bytes as an array, memory or a stream. A sequence of strings
/// is a sequence of lines, so it is asked only of a text that is whole lines. The shipped examples are
/// compiled as they are written, beside what the generator wrote for the grammar in them with the carrier
/// they name taken out.
/// </para>
/// </remarks>
public sealed class CarrierDifferentialTests
{
	static readonly CarrierKind[] Carriers = [CarrierKind.Auto, CarrierKind.Immediate, CarrierKind.Deferred];

	/// <summary>
	/// A grammar and what to read it over. <c>Valid</c> are inputs the grammar accepts; the refused
	/// ones are made from them and from <c>Extra</c>, which holds what no mutation would reach (a
	/// corrupted record under <c>recover</c>, a guard that fails).
	/// </summary>
	sealed record Case(string Name, string Grammar, string[] Valid, string[] Extra, string Members = "", Shipped? From = null);

	/// <summary>
	/// A grammar read out of an example the repository ships, compiled with the class the example
	/// wrote around it. A carrier the example names is dropped: what is asked here is every one.
	/// </summary>
	sealed record Shipped(string File, string Class, string Namespace, bool MayThrow = false);

	static string Examples { get; } = Path.GetFullPath(Path.Combine(Here(), "..", "..", "examples", "DotGram.Examples"));

	static string Here([System.Runtime.CompilerServices.CallerFilePath] string path = "")
	{
		return Path.GetDirectoryName(path)!;
	}

	/// <summary>The text of the first <c>[Gram("""…""")]</c> of a file, without the indent of its raw string.</summary>
	static string GrammarOf(string text)
	{
		var open  = text.IndexOf("[Gram(\"\"\"", StringComparison.Ordinal);
		var start = text.IndexOf('\n', open) + 1;
		var close = text.IndexOf("\"\"\"", start, StringComparison.Ordinal);
		var lines = text.Substring(start, close - start).Replace("\r\n", "\n").Split('\n');
		var indent = lines[^1];

		return string.Join("\n", lines.Take(lines.Length - 1).Select(line => line.StartsWith(indent, StringComparison.Ordinal) ? line.Substring(indent.Length) : line));
	}

	static readonly Case[] Cases =
	[
		new("a calculator",
			"""
			trivia = ' '*

			Expr   : @int = l: Expr & '+' & r: Term => @(l + r)
			               | l: Expr & '-' & r: Term => @(l - r)
			               | t: Term => @(t)
			Term   : @int = l: Term & '*' & r: Factor => @(l * r)
			               | f: Factor => @(f)
			Factor : @int = n: Number => @(n)
			               | '(' & e: Expr & ')' => @(e)
			               | '-' & f: Factor => @(-f)
			Number : @int = d: ['0'..'9']+ => @(int.Parse(d))

			parse Expr
			""",
			["1 + 2 * 3", "(1 + 2) * 3 - 4", "-(2) * -3", "7"],
			["", "1 +", "(1", "1 2", "* 3", "1 + * 2"]),

		new("typed csv with a guard",
			"""
			Csv    : @string[] = Row* & eof
			Row    : @string = name: Name & ',' & amount: Amount & eol => @(name + "=" + amount)
			Name   : @string = text: ['a'..'z' | 'A'..'Z']+ & when @(text.Length <= 5) => @(text)
			Amount : @int = digits: ['0'..'9']+ => @(int.Parse(digits))

			parse Csv
			""",
			["ab,1\ncd,22\n", "abcde,5\n", ""],
			["abcdef,1\n", "ab,1\nabcdef,2\n", "ab,x\n", "ab,1"]),

		new("a feed with a header and a trailer",
			"""
			Feed    : @string = 'H' & eol & rows: Row* & 'T' & n: Digits & eol? & eof => @(string.Join("|", rows) + "|T" + n)
			Row     : @string = 'R' & s: Symbol & '|' & q: Digits & eol => @(s + "x" + q)
			Symbol  : @string = t: ['A'..'Z']+ => @(t)
			Digits  : @string = t: ['0'..'9']+ => @(t)

			parse Feed
			find Row
			""",
			["H\nRAB|1\nRCD|22\nT2\n", "H\nT0\n", "H\nRAB|1\nT1"],
			["H\nRAB|1\nRcd|2\nT2\n", "H\nRAB|\nT1\n", "RAB|1\nT1\n", "H\nRAB|1\nT1\nT1\n"]),

		new("a feed that recovers",
			"""
			Feed    : @string = 'H' & eos & rows: Row* recover eos => @(Bad(parserOrdinal, parserLine, parserText)) & 'T' & Digits & eos & eof => @(string.Join("|", rows))
			Row     : @string = 'R' & s: Symbol & '|' & q: Digits & eos => @(s + "x" + q)
			Symbol  : @string = t: ['A'..'Z']+ => @(t)
			Digits  : @string = t: ['0'..'9']+ => @(t)

			parse Feed
			""",
			["H\nRAB|1\nRCD|22\nT2\n", "H\nT0\n", "H\nRAB|1\nT1"],
			["H\nRAB|1\nRcd|2\nRZZ|3\nT3\n", "H\nRAB|\nRCD|2\nT2\n", "H\nRAB|1\nRCD|2\nT", "H\nRAB|1\nxx\n"],
			Members:
			"""
			static string Bad(int ordinal, int line, string text) { return "!" + ordinal + "/" + line + "/" + text; }
			"""),

		new("a version asked of the grammar",
			"""
			trivia = ' '*

			Version = "2000" | "2008" | "2022"
			Name    : @string = t: ['a'..'z']+ => @(t)

			Test    : @string = l: Name & "*=" & r: Name & when Version is "2000" => @("old:" + l + r)
			                  | l: Name & "is" & "distinct" & "from" & r: Name & when Version is "2022" => @("new:" + l + r)
			                  | l: Name & "=" & r: Name => @("eq:" + l + r)

			parse Test with (Version = "2000") as ParseOld
			parse Test with (Version = "2022") as ParseNew
			parse Test as ParseAny
			""",
			["a*=b", "a is distinct from b", "a=b", "ab = cd"],
			["a *= b c", "a is distinct b", "*=b", "a is distinct from"]),

		new("a rule specialized with `with`",
			"""
			Item  : @int = text: ['0'..'9']+ => @(int.Parse(text))
			Comma = ','
			More  : @int = Comma & value: Item => @(value)
			List  : @int = first: Item & rest: More* => @(first + rest.Length)

			parse List
			parse List with (Comma = ';') as ParseSemicolons
			parse List with (Comma = (',' | ';')) as ParseLoose
			""",
			["1,2,3", "1;2;3", "1,2;3", "7"],
			["1,;2", ";1", "1,2,"]),

		new("lazy results, find and streams",
			"""
			Word  : @string = t: ['a'..'z']+ & ';' => @(t)
			Words : @string[] = Word* & eof
			Items : @string[] = Word*
			Loose : @string[] = Word* recover ';' => @("!" + parserText)
			Chunk = ['a'..'z']+ & ';'
			Raw   = Chunk* & eof

			parse Words
			parse Words as Streamed stream
			parse Raw as RawBytes stream bytes
			parse Items as Lazy yield
			parse Loose as LooseAll
			parse Loose as LooseLazy yield
			find Word
			""",
			["ab;cd;", "ab;", ""],
			["ab;1;cd;", "ab;cd", "ab;;cd;", "1;", "ab;cd;1"]),

		new("the calculator example",
			"",
			["1 + 2 * 3", "(1 + 2) * 3 - 4 / 2", "2 ^ 3 ^ 2", "-(1 + 2) * -3"],
			["1/0", "1/0=", "8 / (2 - 2) )", "99999999999 + 1", "99999999999 +", "1.5 * 2", "0.5 / 0"],
			From: new Shipped("Expressions/CalculatorExample.cs", "Calculator", "DotGram.Examples.Expressions", MayThrow: true)),

		new("the filter example",
			"",
			["Price > 10 AND Country IN ('UK', 'DE') OR NOT Discontinued", "cpu = 0.75", "Note = null OR Note <> 'it''s'", "Size IN (1, 2.5, 3)"],
			["cpu=99999999999999999999999999999", "cpu=99999999999999999999999999999 mem", "cpu=0 .75", "Size IN (1, 2 .5, 3)"],
			From: new Shipped("Languages/FilterExample.cs", "Filter", "DotGram.Examples.Languages", MayThrow: true)),

		new("the stock count example",
			"",
			["apples: 12\npears: 7\nEND 2\n", "END 0"],
			["apples: 12\npears\nEND 1\n", "apples: 1\nEND 1\nEND 1\n", "apples: 1\nEND"],
			From: new Shipped("Feeds/StockCountExample.cs", "StockCountReader", "DotGram.Examples.Feeds")),

		new("the recovering feed example",
			"",
			["H|2026-08-13|ACME\nR|AAPL|100|2026-08-12\nR|NVDA|75|2026-08-11\nT|2"],
			["H|2026-08-13|ACME\nR|AAPL|100|2026-08-12\nR|MSFT|two hundred|2026-08-12\nR|NVDA|75|2026-08-11\nT|3", "H|2026-08-13|ACME\nR|AAPL|1|2026-08-12\nR||\nT|2"],
			From: new Shipped("Feeds/RecoveringFeedExample.cs", "RecoveringFeedReader", "DotGram.Examples.Feeds")),
	];

	public static TheoryData<string> Names()
	{
		var data = new TheoryData<string>();

		foreach (var one in Cases)
			data.Add(one.Name);

		return data;
	}

	/// <summary>Every entry answers the same under every carrier: value, refusal, exception, lazy elements.</summary>
	[Theory]
	[MemberData(nameof(Names))]
	public void An_entry_answers_the_same_under_every_carrier(string name)
	{
		var one = Cases.Single(c => c.Name == name);
		var table = Table(one);
		var told = new List<string>();

		foreach (var input in Inputs(one))
		{
			foreach (var form in table.Forms)
			{
				var answers = Carriers.Select(carrier => (carrier, answer: table.Answers[(carrier, form.Name, input)])).ToList();

				if (answers.Select(a => a.answer).Distinct().Count() > 1)
					told.Add($"{form} over {Show(input)}: " + string.Join("; ", answers.Select(a => $"{a.carrier}: {a.answer}")));
			}
		}

		Assert.True(told.Count == 0, name + ":\n" + string.Join("\n", told));
	}

	/// <summary>The entries of one carrier answer one question alike: the same value, or the same refusal.</summary>
	[Theory]
	[MemberData(nameof(Names))]
	public void The_entries_of_a_carrier_agree_with_each_other(string name)
	{
		var one = Cases.Single(c => c.Name == name);
		var table = Table(one);
		var told = new List<string>();

		foreach (var carrier in Carriers)
		{
			foreach (var input in Inputs(one))
			{
				foreach (var publication in table.Forms.GroupBy(Publication))
				{
					var reference = publication.First(f => f.Entry == "string" || f.Entry == "lines" || f.Entry == "byte[]");

					foreach (var form in publication)
					{
						if (table.Answers[(carrier, form.Name, input)] == NotAsked)
							continue;

						var exact    = Says(reference) && Says(form);
						var expected = Verdict(table.Answers[(carrier, reference.Name, input)], exact);
						var actual   = Verdict(table.Answers[(carrier, form.Name, input)], exact);

						if (expected != actual)
							told.Add($"{carrier}, {form.Name} against {reference.Name} over {Show(input)}: {actual} against {expected}");
					}
				}
			}
		}

		Assert.True(told.Count == 0, name + ":\n" + string.Join("\n", told.Distinct()));
	}

	/// <summary>Compiling the grammar tells the same, but for what a carrier is told about itself.</summary>
	[Theory]
	[MemberData(nameof(Names))]
	public void The_diagnostics_do_not_depend_on_the_carrier(string name)
	{
		var one = Cases.Single(c => c.Name == name);
		var lists = Carriers.Select(carrier => string.Join("\n", Build(one, carrier).Diagnostics.Where(Neutral))).ToList();

		Assert.Single(lists.Distinct());

		static bool Neutral(string diagnostic)
		{
			return !diagnostic.EndsWith(GramCompiler.CarrierChosen, StringComparison.Ordinal) &&
				!diagnostic.EndsWith(GramCompiler.CarrierRefused, StringComparison.Ordinal) &&
				!diagnostic.EndsWith(GramCompiler.CarrierCaution, StringComparison.Ordinal);
		}
	}

	/// <summary>
	/// What the comparison above says would mean nothing over a table that never accepts, never
	/// refuses, or has one entry: each case answers both ways, through more than one entry.
	/// </summary>
	[Theory]
	[MemberData(nameof(Names))]
	public void The_table_is_not_vacuous(string name)
	{
		var one = Cases.Single(c => c.Name == name);
		var table = Table(one);
		var answers = table.Answers.Where(a => a.Key.Item1 == CarrierKind.Deferred).Select(a => a.Value).ToList();

		Assert.Contains(answers, a => a.StartsWith("ok ", StringComparison.Ordinal));
		Assert.Contains(answers, a => a.StartsWith("refused", StringComparison.Ordinal) || a.StartsWith("throws", StringComparison.Ordinal));
		Assert.True(table.Forms.Count >= 3, $"{name}: {table.Forms.Count} entries.");

		Console.WriteLine(
			$"{name}: {Carriers.Length} carriers x {table.Forms.Count} entries x {Inputs(one).Count} inputs = {table.Answers.Count} answers; {table.Answers.Count(a => a.Value != NotAsked)} asked");
	}

	sealed record Entries(List<Form> Forms, Dictionary<(CarrierKind, string, string), string> Answers);

	static readonly Dictionary<string, Entries> Tables = [];

	static Entries Table(Case one)
	{
		lock (Tables)
		{
			if (Tables.TryGetValue(one.Name, out var found))
				return found;

			var forms = new List<Form>();
			var answers = new Dictionary<(CarrierKind, string, string), string>();
			var names = new List<string>();

			foreach (var carrier in Carriers)
			{
				var here = Forms(Build(one, carrier).Assembly, one.From).ToList();

				names.Add(string.Join("\n", here.Select(f => f.Name)));

				foreach (var form in here)
				{
					if (carrier == Carriers[0])
						forms.Add(form);

					foreach (var input in Inputs(one))
						answers[(carrier, form.Name, input)] = Answer(form, input);
				}
			}

			// The same entries under every carrier: which of them exist is an answer too.
			Assert.Single(names.Distinct());

			return Tables[one.Name] = new Entries(forms, answers);
		}
	}

	static List<string> Inputs(Case one)
	{
		return one.Valid.Concat(one.Extra).Concat(one.Valid.SelectMany(Mutants)).Distinct().ToList();
	}

	static IEnumerable<string> Mutants(string valid)
	{
		yield return "";

		if (valid.Length == 0)
			yield break;

		yield return valid.Substring(1);
		yield return valid.Substring(0, valid.Length / 2);
		yield return valid.Substring(0, valid.Length - 1);
		yield return valid.Insert(valid.Length / 2, "#");
		yield return valid + "#";
	}

	static string Show(string input)
	{
		return "\"" + input.Replace("\n", "\\n") + "\"";
	}

	static string Publication(Form form)
	{
		var name = form.Method.Name;

		return name.StartsWith("Try", StringComparison.Ordinal) ? name.Substring(3) : name;
	}

	/// <summary>
	/// What an entry says, in the terms the entries share: for a <c>Try</c> the value or the refusal
	/// with where and why; for the others, whether and what.
	/// </summary>
	static string Verdict(string answer, bool exact)
	{
		if (exact || answer.StartsWith("ok ", StringComparison.Ordinal))
			return answer;

		return answer.StartsWith("throws", StringComparison.Ordinal) || answer.StartsWith("refused", StringComparison.Ordinal)
			? "refused"
			: answer;
	}

	static bool Says(Form form)
	{
		return form.Name.StartsWith("Try", StringComparison.Ordinal) && !form.Name.Contains("/out", StringComparison.Ordinal);
	}

	static string Answer(Form form, string input)
	{
		if (form.Applies is { } applies && !applies(input))
			return NotAsked;

		object? result;
		var arguments = form.Arguments(input);

		try
		{
			result = form.Method.Invoke(null, arguments);
		}
		catch (TargetInvocationException thrown)
		{
			return Thrown(thrown.InnerException!);
		}

		if (form.Method.ReturnType == typeof(bool))
		{
			return (bool)result! ? "ok " + Format(arguments[^1]) : "refused";
		}

		if (form.Method.ReturnType.IsGenericType && form.Method.ReturnType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
		{
			// What a lazy result hands over is what it answers, whole, or what it handed over before it threw.
			var handed = new List<object?>();

			try
			{
				foreach (var element in (IEnumerable)result!)
					handed.Add(element);
			}
			catch (Exception thrown)
			{
				return Thrown(thrown) + " after " + Format(handed);
			}

			return "ok " + Format(handed);
		}

		var said = Format(result);

		// A value the entry returns, as the `Try` forms say it.
		return form.Method.ReturnType.IsGenericType && form.Method.ReturnType.Name == "Match`1" ? said : "ok " + said;
	}

	const string NotAsked = "not asked";

	static string Thrown(Exception thrown)
	{
		return "throws " + thrown.GetType().Name + ": " + thrown.Message;
	}

	static string Format(object? value)
	{
		switch (value)
		{
			case null:
				return "null";

			case string text:
				return "\"" + text + "\"";

			case byte[] bytes:
				return "\"" + Encoding.UTF8.GetString(bytes) + "\"";
		}

		var type = value.GetType();

		if (type.IsGenericType && type.GetGenericTypeDefinition().Name == "Match`1")
		{
			object? Read(string property)
			{
				return type.GetProperty(property)!.GetValue(value);
			}

			return (bool)Read("IsSuccess")!
				? "ok " + Format(Read("Value"))
				: $"refused {Read("Outcome")} at {Read("Position")}: {Read("Error")}";
		}

		if (value is IEnumerable sequence)
			return "[" + string.Join(", ", sequence.Cast<object?>().Select(Format)) + "]";

		return value is IFormattable formattable ? formattable.ToString(null, CultureInfo.InvariantCulture) : value.ToString()!;
	}

	/// <summary>
	/// What the generator wrote, beside the example it was written for, which is ordinary modern C#
	/// where the generated code is held to the floor: compiled as a project would, at the latest version.
	/// </summary>
	static Assembly CompileShipped(string generated, string example)
	{
		var parse = CSharpParseOptions.Default;

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.Shipped",
			[
				CSharpSyntaxTree.ParseText("global using System; global using System.Collections.Generic; global using System.IO; global using System.Linq;", parse),
				CSharpSyntaxTree.ParseText(GramCompiler.EmitMarkerAttributes().Text, parse),
				CSharpSyntaxTree.ParseText(SupportEmitter.EmbeddedAttribute, parse),
				CSharpSyntaxTree.ParseText(generated, parse),
				CSharpSyntaxTree.ParseText(example, parse),
			],
			EmittedCode.References,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		using var stream = new MemoryStream();

		var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);
		var errors = result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();

		Assert.True(errors.Length == 0, string.Join("\n", errors.Select(d => d.ToString())));

		return Assembly.Load(stream.ToArray());
	}

	sealed record Compiled(string Source, Assembly Assembly, string[] Diagnostics);

	static readonly Dictionary<(string, CarrierKind), Compiled> Cache = [];

	static Compiled Build(Case one, CarrierKind carrier)
	{
		lock (Cache)
		{
			if (Cache.TryGetValue((one.Name, carrier), out var found))
				return found;

			var from = one.From;
			var file = from is null ? null : File.ReadAllText(Path.Combine(Examples, from.File));

			var result = GramCompiler.Compile(from is null ? one.Grammar : GrammarOf(file!), new GramCompilerOptions
			{
				ClassName     = from?.Class ?? "Probe",
				Namespace     = from?.Namespace ?? "Carried",
				CSharpScanner = RoslynCSharpScanner.Instance,
				Carrier       = carrier,
				ConstructionsMayThrow = from?.MayThrow ?? false,
			});

			Assert.DoesNotContain(result.Diagnostics, d => d.Severity == GramSeverity.Error);

			var source = Assert.Single(result.Sources).Text;
			var assembly = from is null
				? EmittedCode.Compile(source, "Probe", "Carried", declarationMembers: one.Members)
				: CompileShipped(source, file!);
			var ids = result.Diagnostics.Select(d => d.Severity + " " + d.Id).ToArray();

			return Cache[(one.Name, carrier)] = new Compiled(source, assembly, ids);
		}
	}

	sealed record Form(string Name, MethodInfo Method, Func<string, object?[]> Arguments, Func<string, bool>? Applies = null)
	{
		/// <summary>What the input came in as: the text inside the first parenthesis.</summary>
		public string Entry
		{
			get
			{
				var from = Name.IndexOf('(') + 1;
				var to   = Name.IndexOfAny([',', ')'], from);

				return Name.Substring(from, to - from);
			}
		}
	}

	static IEnumerable<Form> Forms(Assembly assembly, Shipped? from)
	{
		var type = assembly.GetType(from is null ? "Carried.Probe" : from.Namespace + "." + from.Class)!;

		var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static).OrderBy(m => m.ToString(), StringComparer.Ordinal).ToList();

		// A grammar that names `eos` is read over lines; any other over the pieces of a text.
		var lined = methods.Any(m => m.GetParameters() is [{ } one, ..] && one.ParameterType == typeof(IReadOnlyList<string>));

		foreach (var method in methods)
		{
			var parameters = method.GetParameters();

			if (parameters.Length == 0 || parameters.Any(p => p.ParameterType.IsByRef && !p.IsOut))
				continue;

			var first = parameters[0].ParameterType;
			var extra = parameters.Skip(1).ToArray();
			var outs  = extra.Where(p => p.IsOut).ToArray();
			var rest  = extra.Where(p => !p.IsOut).ToArray();

			if (rest.Any(p => !(p.IsOptional && p.ParameterType == typeof(int?))) || outs.Length > 1)
				continue;

			var outputs = outs.Length == 1 ? "/out" : "";

			object?[] Pack(object? input, int? window)
			{
				var arguments = new object?[parameters.Length];

				arguments[0] = input;

				for (var at = 1; at < parameters.Length; at++)
					arguments[at] = parameters[at].IsOut ? null : (at == 1 ? window : null);

				return arguments;
			}

			if (first == typeof(string))
				yield return new Form(method.Name + "(string)" + outputs, method, text => Pack(text, null));
			else if (first == typeof(byte[]))
				yield return new Form(method.Name + "(byte[])" + outputs, method, text => Pack(Encoding.UTF8.GetBytes(text), null));
			else if (first == typeof(ReadOnlyMemory<byte>))
				yield return new Form(method.Name + "(memory)" + outputs, method, text => Pack(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(text)), null));
			else if (first == typeof(IReadOnlyList<string>))
				yield return new Form(method.Name + "(lines)" + outputs, method, text => Pack(Lines(text), null));
			else if (first == typeof(IEnumerable<string>))
				yield return new Form(method.Name + "(sequence)" + outputs, method, text => Pack(Lines(text), null), lined ? null : Whole);
			else if (first == typeof(TextReader))
			{
				yield return new Form(method.Name + "(reader)" + outputs, method, text => Pack(new StringReader(text), null));
				yield return new Form(method.Name + "(reader dribbled)" + outputs, method, text => Pack(new Dribbled(text), null));

				if (rest.Length > 0)
					yield return new Form(method.Name + "(reader, window 4)" + outputs, method, text => Pack(new StringReader(text), 4));
			}
			else if (first == typeof(Stream))
			{
				yield return new Form(method.Name + "(stream)" + outputs, method, text => Pack(new MemoryStream(Encoding.UTF8.GetBytes(text)), null));

				if (rest.Length > 0)
					yield return new Form(method.Name + "(stream, window 4)" + outputs, method, text => Pack(new MemoryStream(Encoding.UTF8.GetBytes(text)), 4));
			}
		}
	}

	/// <summary>
	/// A sequence of strings is a sequence of lines, each with its ending: it says what a string says
	/// only of a text that is lines and nothing else, which is where it is asked.
	/// </summary>
	static bool Whole(string text)
	{
		return text.Length == 0 || (text[^1] == '\n' && !text.Contains('\r'));
	}

	static List<string> Lines(string text)
	{
		return text.Length == 0 ? [] : new StringReader(text).ReadAllLines();
	}

	/// <summary>A reader that never hands over more than two characters at a read.</summary>
	sealed class Dribbled(string text) : TextReader
	{
		readonly StringReader _inner = new(text);

		public override int Peek()
		{
			return _inner.Peek();
		}

		public override int Read()
		{
			return _inner.Read();
		}

		public override int Read(char[] buffer, int index, int count)
		{
			return _inner.Read(buffer, index, Math.Min(count, 2));
		}

		public override int Read(Span<char> buffer)
		{
			return _inner.Read(buffer.Length > 2 ? buffer[..2] : buffer);
		}
	}
}

static class LinesExtensions
{
	public static List<string> ReadAllLines(this StringReader reader)
	{
		var lines = new List<string>();

		while (reader.ReadLine() is { } line)
			lines.Add(line);

		return lines;
	}
}
