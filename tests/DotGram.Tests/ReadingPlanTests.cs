using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Plan;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the planning stage decides about a file before any of it is printed: how each machine is
/// read and why no cheaper way was taken, and that a plan the target cannot read is never printed.
/// </summary>
/// <remarks>
/// Held over the snapshot grammars, in every rendering they are held in, for C# and for a C# made to
/// have no jumps. Without jumps the reader is asked first: what it reads it reads, and what only the
/// jump machine reads is refused where it is published rather than written with jumps.
/// </remarks>
public class ReadingPlanTests
{
	[Fact]
	public void The_plan_says_how_every_snapshot_machine_is_read_and_why()
	{
		Assert.Equal(Expected, Planned(Language).Concat(Planned(WithoutJumps).Select(static line => "no jumps: " + line)));
	}

	/// <summary>
	/// What a target without jumps reads by the reader, out of what C# reads by the jump machine and by
	/// the flat machine: every flat machine, and the jump machines whose publications the reader does
	/// not refuse. Counted by machine of C#, and asked of each of its publications, since without jumps
	/// the readers are joined where the flat machines were not.
	/// </summary>
	[Fact]
	public void Without_jumps_the_reader_takes_every_flat_machine_and_refuses_what_only_jumps_read()
	{
		var withoutJumps = new Dictionary<string, MachinePlan>(StringComparer.Ordinal);

		foreach (var (label, plan) in Plans(WithoutJumps))
		{
			foreach (var machine in plan.Machines)
			{
				Assert.True(
					machine.Strategy == ReadingStrategy.Reader || machine.Refusal is not null || machine.Publications.Count == 0,
					$"{label}: {machine.Name} is read by {machine.Strategy} without jumps, and not refused");

				foreach (var publication in machine.Publications)
					withoutJumps[Key(label, publication)] = machine;
			}
		}

		var flat = 0;
		var jump = 0;
		var read = 0;

		foreach (var (label, plan) in Plans(Language))
		{
			foreach (var machine in plan.Machines.Where(static one => one.Publications.Count > 0 && !one.PositionalOnly))
			{
				var readers = machine.Publications.All(one =>
					withoutJumps.TryGetValue(Key(label, one), out var other) && other.Strategy == ReadingStrategy.Reader);

				if (machine.Strategy == ReadingStrategy.Flat)
				{
					flat++;

					Assert.True(readers, $"{label}: the flat {machine.Name} is not read by the reader without jumps");
				}
				else if (machine.Strategy == ReadingStrategy.Jump)
				{
					jump++;

					if (readers)
						read++;
				}
			}
		}

		Assert.Equal((FlatMachines, JumpMachines, JumpMachinesRead), (flat, jump, read));

		static string Key(string label, Publication publication)
		{
			return $"{label}: {publication.Kind} {publication.MethodName}";
		}
	}

	/// <summary>
	/// A <c>find</c> is read only by the jump machine: without jumps the plan refuses it, the refusal is
	/// said as it always was, and the printer is never entered — no parser is written for it.
	/// </summary>
	[Fact]
	public void Without_jumps_a_refused_plan_is_never_printed()
	{
		const string grammar = """
			Name = ['a'..'z']+
			find Name
			""";

		var refused = GramCompiler.Compile(grammar, new GramCompilerOptions { Language = WithoutJumps });
		var told    = Assert.Single(refused.Diagnostics, static one => one.Severity == GramSeverity.Error);

		Assert.Equal("GRAM5020", told.Id);
		Assert.NotNull(refused.Plan);
		Assert.True(refused.Plan.Refused);
		Assert.Equal([told], refused.Plan.Refusals);
		Assert.False(refused.Printed);
		Assert.Empty(refused.Sources);

		// And C#, which has jumps, plans the same grammar and prints it.
		var printed = GramCompiler.Compile(grammar, new GramCompilerOptions { Language = Language });

		Assert.False(printed.Plan!.Refused);
		Assert.True(printed.Printed);
		Assert.Single(printed.Sources);
	}

	/// <summary>
	/// The readers of every snapshot ask the plan whether a choice or a repetition writes a way, and
	/// the plan has an answer for each place they are printed in: a place it did not decide throws,
	/// which no snapshot does. Held here by the counts of what was decided and by which proof.
	/// </summary>
	[Fact]
	public void The_plan_decides_every_way_the_snapshot_readers_write()
	{
		var proofs = new SortedDictionary<WriteProof, int>();

		foreach (var (_, plan) in Plans(Language))
			foreach (var machine in plan.Machines)
				if (machine.Writes is { } writes)
					foreach (var proof in writes.Proofs())
						proofs[proof] = proofs.TryGetValue(proof, out var count) ? count + 1 : 1;

		Assert.Equal(ExpectedProofs, string.Join(", ", proofs.Select(static one => $"{one.Key} {one.Value}")));
	}

	/// <summary>A place the plan did not decide is never decided while printing, and one place is never decided two ways.</summary>
	[Fact]
	public void A_write_decision_the_plan_does_not_hold_throws()
	{
		var plan    = GramCompiler.Compile("A = '(' & A & ')' | 'x'\nparse A", new GramCompilerOptions { Language = Language }).Plan!;
		var decided = plan.Machines.First(static one => one.Writes is not null).Writes!;
		var rule    = new RuleSymbol("A", new GrammarNamespace("", null, isRoot: true), null);
		var node    = new DotGram.Grammar.Model.Node.Empty();
		var other   = new WriteDecisions();

		Assert.Throws<InvalidOperationException>(() => decided.Of(rule, node, 0, 0));

		other.Record(rule, node, 0, 1, WriteDecision.Writes);
		other.Record(rule, node, 0, 1, WriteDecision.Writes);

		Assert.Throws<InvalidOperationException>(() => other.Record(rule, node, 0, 1, WriteDecision.By(WriteProof.Fixed)));
		Assert.Equal(WriteDecision.Writes, other.Agreed(rule, node, 0));

		other.Record(rule, node, 0, 2, WriteDecision.By(WriteProof.Fixed));

		Assert.Null(other.Agreed(rule, node, 0));
	}

	/// <summary>Every machine of every snapshot rendering, with its plan, under one key a machine.</summary>
	static List<(string Key, MachinePlan Plan)> Machines(CSharpLanguage language)
	{
		var machines = new List<(string Key, MachinePlan Plan)>();

		foreach (var (label, plan) in Plans(language))
			foreach (var machine in plan.Machines)
				machines.Add(($"{label}: {machine.Name}", machine));

		return machines;
	}

	/// <summary>Every machine of every snapshot rendering, said in one line: how it is read, and why.</summary>
	static IEnumerable<string> Planned(CSharpLanguage language)
	{
		foreach (var (key, machine) in Machines(language))
			yield return $"{key}: {Said(machine.Strategy)}" +
				(machine.Refusal is not null ? ", refused" : "") +
				(machine.Reason is { } reason ? $" ({reason})" : "");
	}

	static IEnumerable<(string Label, ReadingPlan Plan)> Plans(CSharpLanguage language)
	{
		foreach (var name in SnapshotTests.Snapshots.Select(static row => row.Data).Order(StringComparer.Ordinal))
		{
			var text = System.IO.File.ReadAllText(System.IO.Path.Combine(SnapshotTests.Directory, name + ".gram"));

			foreach (var (options, _) in SnapshotTests.Renderings(name, text))
			{
				options.Language = new CSharpLanguage(options.SymbolResolver, options.CSharpScanner).With(language.Can);

				var result = GramCompiler.Compile(text, options);
				var label  = options.Suffix is { Length: > 0 } suffix ? $"{name}.{suffix}" : name;

				Assert.NotNull(result.Plan);

				yield return (label, result.Plan);
			}
		}
	}

	static string Said(ReadingStrategy strategy)
	{
		return strategy switch
		{
			ReadingStrategy.Flat   => "flat",
			ReadingStrategy.Reader => "reader",
			_                      => "jump",
		};
	}

	/// <summary>The flat machines of the snapshots, every one read by the reader without jumps.</summary>
	const int FlatMachines     = 23;

	/// <summary>The jump machines of the snapshots that read a publication, and how many of them the reader reads without jumps.</summary>
	const int JumpMachines     = 14;
	const int JumpMachinesRead = 2;

	/// <summary>How every machine of the snapshots is read, and why: C#, then a C# without jumps.</summary>
	static readonly string[] Expected =
	[
		"BackReference: Recognize_DotGram_RawString: jump (a rule it reaches recurses, backtracks or holds a construction back; and 'RawString' cannot be read by methods because it is published with 'find')",
		"BackReference: Recognize_DotGram_LongString: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"BackReference: Recognize_DotGram_DollarQuoted: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"BackReference: Recognize_DotGram_Quoted: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"BackReference: Recognize_DotGram_Heading: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"BackReference: Recognize_DotGram_Between: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"BackReference: Recognize_DotGram_Element: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"BackReference: Recognize_DotGram_Pair: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Begun: Recognize_DotGram: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Begun: RecognizeValue_DotGram: jump (it reads a token again for the value of a terminal, which is done by jumps)",
		"Begun: RecognizeSeam_DotGram: jump (it scans the seam between tokens, which the lexer does by jumps)",
		"Buffered: Recognize_DotGram_Feed: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Buffered: Recognize_DotGram_Record: jump (it is read a window at a time)",
		"Buffered: RecognizeBuf_DotGram_ParseFeed: reader (it reads a buffer, which the flat machine does not)",
		"Buffered: RecognizeBufBytes_DotGram_ParseFeed: reader (it reads a buffer, which the flat machine does not)",
		"Buffered: RecognizeBuf_DotGram_FindRecord: jump (it reads a buffer, which the flat machine does not, and the publication is not read by methods over a string)",
		"Buffered: RecognizeBufBytes_DotGram_FindRecord: jump (it reads a buffer, which the flat machine does not, and the publication is not read by methods over a string)",
		"Csv: Recognize_DotGram: jump (there is nothing published)",
		"Feed: Recognize_DotGram_Feed: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Feed: Recognize_DotGram_Name: jump (it is read a window at a time)",
		"Feed: Recognize_DotGram_Row: jump (it is read a window at a time)",
		"Kept: Recognize_DotGram: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Kept: RecognizeSeam_DotGram: jump (it scans the seam between tokens, which the lexer does by jumps)",
		"Lexical: Recognize_DotGram: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Lexical: RecognizeSeam_DotGram: jump (it scans the seam between tokens, which the lexer does by jumps)",
		"Lexical.Chained: Recognize_DotGram: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Lexical.Chained: RecognizeSeam_DotGram: jump (it scans the seam between tokens, which the lexer does by jumps)",
		"Lexical.Utf8: Recognize_DotGram: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Lexical.Utf8: RecognizeSeam_DotGram: jump (it scans the seam between tokens, which the lexer does by jumps)",
		"Located: Recognize_DotGram: jump (it is read a window at a time)",
		"Minimal: Recognize_DotGram_A: flat",
		"Minimal: Recognize_DotGram_B: flat",
		"Minimal: Recognize_DotGram_C: flat",
		"Minimal: Recognize_DotGram_D: flat",
		"Minimal: Recognize_DotGram_E: flat",
		"Minimal: Recognize_DotGram_F: flat",
		"Minimal: Recognize_DotGram_Text: flat",
		"Minimal: Recognize_DotGram_Number: flat",
		"Minimal: Recognize_DotGram_Alias: flat",
		"Minimal: Recognize_DotGram_Predicted: flat",
		"Minimal: Recognize_DotGram_List: flat",
		"Minimal: Recognize_DotGram_Counted: flat",
		"Minimal: Recognize_DotGram_Maybe: flat",
		"Minimal: Recognize_DotGram_Ahead: flat",
		"Minimal: Recognize_DotGram_Not: flat",
		"Minimal: Recognize_DotGram_Committed: flat",
		"Minimal: Recognize_DotGram_Sum: reader (a rule it reaches climbs)",
		"Minimal: Recognize_DotGram_Either: flat",
		"Minimal: Recognize_DotGram_Wrapped: flat",
		"Minimal: Recognize_DotGram_Sheet: jump (a rule it reaches recovers)",
		"Minimal: Recognize_DotGram_Ci: flat",
		"Minimal: Recognize_DotGram_Upper: flat",
		"Minimal: Recognize_DotGram_Spaced_Pair: flat",
		"Minimal: Recognize_DotGram_Item: jump (it is read a window at a time)",
		"Minimal: RecognizeAt_DotGram_Text: reader (it reads the forms of a flat machine's publications that begin where they are told)",
		"Notation: Recognize_DotGram_Hashed: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Notation: Recognize_DotGram_Marked: flat",
		"Notation: Recognize_DotGram_List_With1: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Notation: Recognize_DotGram_List: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Notation: Recognize_DotGram_Small: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Notation: Recognize_DotGram_Ab: flat",
		"Notation: Recognize_DotGram_Primary: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Notation: Recognize_DotGram_Word: jump (it is read a window at a time)",
		"Segmented: Recognize_DotGram: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Specialized: Recognize_DotGram_List: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Specialized: Recognize_DotGram_List_With1: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Specialized: Recognize_DotGram_List_With2: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Split: Recognize_DotGram: jump (it is read a window at a time)",
		"Tower: Recognize_DotGram: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Tower.Checked: Recognize_DotGram: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Twice: Recognize_DotGram: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Twice.Immediate: Recognize_DotGram: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Twice.Traced: Recognize_DotGram: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Url: Recognize_DotGram: jump (it is read a window at a time)",
		"Url.Traced: Recognize_DotGram: jump (it is read a window at a time)",
		"Url.Stepped: Recognize_DotGram: jump (it is read a window at a time)",
		"Words: Recognize_DotGram_Statement: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"Words: Recognize_DotGram_Spaced_List: reader (a rule it reaches recurses, backtracks or holds a construction back)",
		"no jumps: BackReference: Recognize_DotGram_RawString: jump, refused (C# as it is targeted here has no jumps; and 'RawString' cannot be read by methods because it is published with 'find')",
		"no jumps: BackReference: Recognize_DotGram_LongString: reader (C# as it is targeted here has no jumps)",
		"no jumps: BackReference: Recognize_DotGram_DollarQuoted: reader (C# as it is targeted here has no jumps)",
		"no jumps: BackReference: Recognize_DotGram_Quoted: reader (C# as it is targeted here has no jumps)",
		"no jumps: BackReference: Recognize_DotGram_Heading: reader (C# as it is targeted here has no jumps)",
		"no jumps: BackReference: Recognize_DotGram_Between: reader (C# as it is targeted here has no jumps)",
		"no jumps: BackReference: Recognize_DotGram_Element: reader (C# as it is targeted here has no jumps)",
		"no jumps: BackReference: Recognize_DotGram_Pair: reader (C# as it is targeted here has no jumps)",
		"no jumps: Begun: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Begun: RecognizeValue_DotGram: jump, refused (it reads a token again for the value of a terminal, which is done by jumps)",
		"no jumps: Begun: RecognizeSeam_DotGram: jump (it scans the seam between tokens, which the lexer does by jumps)",
		"no jumps: Buffered: Recognize_DotGram_Feed: reader (C# as it is targeted here has no jumps)",
		"no jumps: Buffered: Recognize_DotGram_Record: jump, refused (C# as it is targeted here has no jumps; and 'Record' cannot be read by methods because it is published with 'find')",
		"no jumps: Buffered: RecognizeBuf_DotGram_ParseFeed: reader (it reads a buffer, which the flat machine does not)",
		"no jumps: Buffered: RecognizeBufBytes_DotGram_ParseFeed: reader (it reads a buffer, which the flat machine does not)",
		"no jumps: Buffered: RecognizeBuf_DotGram_FindRecord: jump, refused (it reads a buffer, which the flat machine does not, and the publication is not read by methods over a string)",
		"no jumps: Buffered: RecognizeBufBytes_DotGram_FindRecord: jump, refused (it reads a buffer, which the flat machine does not, and the publication is not read by methods over a string)",
		"no jumps: Csv: Recognize_DotGram: jump (C# as it is targeted here has no jumps; and there is nothing published)",
		"no jumps: Feed: Recognize_DotGram_Feed: reader (C# as it is targeted here has no jumps)",
		"no jumps: Feed: Recognize_DotGram_Row: jump, refused (C# as it is targeted here has no jumps; and 'Row' cannot be read by methods because it is published with 'find')",
		"no jumps: Kept: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Kept: RecognizeSeam_DotGram: jump (it scans the seam between tokens, which the lexer does by jumps)",
		"no jumps: Lexical: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Lexical: RecognizeSeam_DotGram: jump (it scans the seam between tokens, which the lexer does by jumps)",
		"no jumps: Lexical.Chained: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Lexical.Chained: RecognizeSeam_DotGram: jump (it scans the seam between tokens, which the lexer does by jumps)",
		"no jumps: Lexical.Utf8: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Lexical.Utf8: RecognizeSeam_DotGram: jump (it scans the seam between tokens, which the lexer does by jumps)",
		"no jumps: Located: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_A: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_B: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_C: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_D: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_E: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_F: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Text: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Alias: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Predicted: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_List: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Counted: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Maybe: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Ahead: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Not: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Committed: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Sum: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Either: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Wrapped: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Sheet: jump, refused (C# as it is targeted here has no jumps; and 'Sheet' cannot be read by methods because it recovers from a bad element)",
		"no jumps: Minimal: Recognize_DotGram_Ci: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Upper: reader (C# as it is targeted here has no jumps)",
		"no jumps: Minimal: Recognize_DotGram_Spaced_Pair: reader (C# as it is targeted here has no jumps)",
		"no jumps: Notation: Recognize_DotGram_Hashed: reader (C# as it is targeted here has no jumps)",
		"no jumps: Notation: Recognize_DotGram_Marked: reader (C# as it is targeted here has no jumps)",
		"no jumps: Notation: Recognize_DotGram_List_With1: reader (C# as it is targeted here has no jumps)",
		"no jumps: Notation: Recognize_DotGram_List: reader (C# as it is targeted here has no jumps)",
		"no jumps: Notation: Recognize_DotGram_Small: reader (C# as it is targeted here has no jumps)",
		"no jumps: Notation: Recognize_DotGram_Ab: reader (C# as it is targeted here has no jumps)",
		"no jumps: Notation: Recognize_DotGram_Primary: reader (C# as it is targeted here has no jumps)",
		"no jumps: Notation: Recognize_DotGram_Word: jump, refused (C# as it is targeted here has no jumps; and 'Word' cannot be read by methods because it is published with 'find')",
		"no jumps: Segmented: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Specialized: Recognize_DotGram_List: reader (C# as it is targeted here has no jumps)",
		"no jumps: Specialized: Recognize_DotGram_List_With1: reader (C# as it is targeted here has no jumps)",
		"no jumps: Specialized: Recognize_DotGram_List_With2: reader (C# as it is targeted here has no jumps)",
		"no jumps: Split: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Tower: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Tower.Checked: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Twice: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Twice.Immediate: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Twice.Traced: Recognize_DotGram: reader (C# as it is targeted here has no jumps)",
		"no jumps: Url: Recognize_DotGram: jump, refused (C# as it is targeted here has no jumps; and 'Url' cannot be read by methods because it is published with 'find')",
		"no jumps: Url.Traced: Recognize_DotGram: jump, refused (C# as it is targeted here has no jumps; and 'Url' cannot be read by methods because it is published with 'find')",
		"no jumps: Url.Stepped: Recognize_DotGram: jump, refused (C# as it is targeted here has no jumps; and 'Url' cannot be read by methods because it is published with 'find')",
		"no jumps: Words: Recognize_DotGram_Statement: reader (C# as it is targeted here has no jumps)",
		"no jumps: Words: Recognize_DotGram_Spaced_List: reader (C# as it is targeted here has no jumps)",
	];

	/// <summary>How many places of the snapshot readers each proof settled, and how many write a way (None).</summary>
	const string ExpectedProofs = "None 33, LiteralRun 3, Exclusive 46, SettledText 1, Fixed 4, NeverGivesBack 60, NeverGivesBackPast 1";

	static readonly CSharpLanguage Language = new(scanner: RoslynCSharpScanner.Instance);

	static readonly CSharpLanguage WithoutJumps = Language.With(new TargetCapabilities
	{
		UnstructuredJumps  = false,
		ByteStringLiterals = true,
	});
}
