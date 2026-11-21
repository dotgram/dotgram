using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>
/// A turn of a repetition that gives back inside itself, where the plan writes no way for the
/// repetition: read by the proofs lowering, by the all-open lowering and by the generated parser,
/// the three answer alike.
/// </summary>
public sealed class GiveBackTests(ITestOutputHelper output)
{
	static readonly IrLowering.Options AllOpen = new(AllResumable: true);

	/// <summary>No part read again in place: wrong on purpose.</summary>
	static readonly IrLowering.Options Closed = new(KeepsPartsWhole: true);

	/// <summary>No part read again in place, and a repetition whose turn gives back inside opened instead: right answers, other host calls.</summary>
	static readonly IrLowering.Options Reopened = new(KeepsPartsWhole: true, ReopensInstead: true);

	/// <summary>
	/// <c>00a,b;c;</c>: the second turn of <c>(B* &amp; Letter &amp; ';')+</c> has <c>B*</c> give the
	/// <c>c</c> back. Accepted as the generated parser accepts it, with its host calls, the turn read
	/// again in place; refused with no part read so, since the braces have sealed before <c>eof</c> fails.
	/// </summary>
	[Fact]
	public void ATurnPastTheFewest_GivesBackInsideItself()
	{
		var fixture = Cases.Fixtures["GivesBackInTurn"];

		var (theirs, theirLog, _) = Reading.Generated(fixture.Parser, fixture.Entry, "00a,b;c;");
		var (ours, ourLog, _)     = Reading.Evaluated(fixture.Grammar, fixture.Entry, "00a,b;c;");
		var (closed, _, _)        = Reading.Evaluated(fixture.Grammar, fixture.Entry, "00a,b;c;", options: Closed);

		Assert.True(theirs.Accepted);
		Assert.Equal(theirs.Line, ours.Line);
		Assert.Equal(theirLog, ourLog);
		Assert.False(closed.Accepted);
		Assert.Equal(1, Reading.Program(fixture.Grammar).Retrying);
	}

	/// <summary>
	/// <c>0"a,b;"</c>, the same past a one-character stop, each <c>A</c> built: read in place, the host
	/// is called as the generated parser calls it; with no part read so, the turn given up is read again
	/// by the replay of the whole rule, which answers alike and builds <c>A*</c> once more.
	/// </summary>
	[Fact]
	public void ATurnPastAStop_GivesBackInsideItself()
	{
		var fixture = Cases.Fixtures["GivesBackPastStopBuilt"];

		var (theirs, theirLog, _)  = Reading.Generated(fixture.Parser, fixture.Entry, "0\"a,b;\"");
		var (ours, ourLog, _)      = Reading.Evaluated(fixture.Grammar, fixture.Entry, "0\"a,b;\"");
		var (closed, closedLog, _) = Reading.Evaluated(fixture.Grammar, fixture.Entry, "0\"a,b;\"", options: Closed);

		Assert.True(theirs.Accepted);
		Assert.Equal(theirs.Line, ours.Line);
		Assert.Equal(theirs.Line, closed.Line);
		Assert.Equal(theirLog, ourLog);
		Assert.Equal(["c1(\"0\")", "c1(\"0\")"], closedLog.Where(static call => call.StartsWith("c1", StringComparison.Ordinal)));
		Assert.Equal(1, Reading.Program(fixture.Grammar).Retrying);
	}

	/// <summary>
	/// A turn below the fewest that fails after a decision of its own is read again in place, as the
	/// generated reader reads every turn: the first turn of <c>+</c> on <c>00a,b;c;</c> builds
	/// <c>A*</c> once. Opening the repetition instead (what the lowering did before parts were read in
	/// place) answers alike and reads the first turn again by the replay around it, building <c>A*</c> twice.
	/// </summary>
	[Fact]
	public void ATurnBelowTheFewest_IsReadAgainInPlace()
	{
		var fixture = Cases.Fixtures["GivesBackInTurnBuilt"];

		var (theirs, theirLog, _) = Reading.Generated(fixture.Parser, fixture.Entry, "00a,b;c;");

		Assert.Equal(["c1(\"0\")", "c1(\"0\")", "c0([c1(\"0\"),c1(\"0\")])"], theirLog);

		foreach (var options in new[] { null, AllOpen })
		{
			var (ours, ourLog, _) = Reading.Evaluated(fixture.Grammar, fixture.Entry, "00a,b;c;", options: options);

			Assert.Equal(theirs.Line, ours.Line);
			Assert.Equal(theirLog, ourLog);
		}

		var (reopened, reopenedLog, _) = Reading.Evaluated(fixture.Grammar, fixture.Entry, "00a,b;c;", options: Reopened);

		Assert.Equal(theirs.Line, reopened.Line);
		Assert.Equal(["c1(\"0\")", "c1(\"0\")", "c1(\"0\")", "c1(\"0\")", "c0([c1(\"0\"),c1(\"0\")])"], reopenedLog);
	}

	/// <summary>
	/// A turn and the last alternative of a choice read with a way, each giving back two items or more:
	/// read again in place, they stop building at the second give-back where the plan's readers do at
	/// that kind of loop, and read the attempt that stands once more, built — the host called as the
	/// generated parser calls it on every input (<see cref="ContractTests.TheEvaluatorAnswersAsTheGeneratedParser"/>).
	/// </summary>
	[Theory]
	[InlineData("TurnDefers", "0a,b,cd;")]
	[InlineData("LastAlternativeGivesBack", "0a,b,cd!")]
	public void APartReadInPlace_StopsBuildingAsTheReaderDoes(string name, string input)
	{
		var fixture = Cases.Fixtures[name];
		var program = Reading.Program(fixture.Grammar);
		var parts   = program.Functions.SelectMany(static function => Walk(function.Body)).ToList();

		Assert.Contains(parts, static part => part is IrStmt.Repeat { Retries: true, Defers: true } or IrStmt.Choice { Retries: true, Defers: true });

		var (theirs, theirLog, _) = Reading.Generated(fixture.Parser, fixture.Entry, input);
		var (ours, ourLog, evaluator) = Reading.Evaluated(fixture.Grammar, fixture.Entry, input);

		Assert.Equal(theirs.Line, ours.Line);
		Assert.Equal(ourLog, theirLog.Take(ourLog.Count));
		Assert.True(evaluator.StoodReReads > 0, "no attempt stood unbuilt");

		static IEnumerable<IrStmt> Walk(IrStmt stmt)
		{
			return stmt.Children.SelectMany(Walk).Prepend(stmt);
		}
	}

	/// <summary>
	/// The proofs lowering, the all-open lowering (each replaying and resuming) and the generated
	/// parser give one answer — verdict, furthest refusal, value — on every input of every fixture,
	/// on the ladders at small sizes, and on every input up to seven characters of every member of
	/// <see cref="GiveBackFamily"/>. With the repetitions left closed the family is answered otherwise.
	/// </summary>
	[Fact]
	public void ThreeVoices_AnswerAlike()
	{
		var report = new StringBuilder();
		var wrong  = new ConcurrentQueue<string>();

		var fixtures = Cases.Fixtures.Keys.SelectMany(static name => Cases.InputsOf(name).Select(input => (name, input))).ToList();
		var (read, over) = Corpus("fixtures", fixtures.Select(static row => (Cases.Fixtures[row.Item1], row.Item2)), wrong, null);

		report.Append(Line("fixtures", fixtures.Count, read, over, wrong.Count));

		var ladders = Ladders.Shapes.SelectMany(static shape => new[] { 1, 2, 3, 4, 8, 16, 100 }.Select(n => (Cases.Fixtures[shape.Fixture], shape.Input(n)))).ToList();

		(read, over) = Corpus("ladders", ladders, wrong, null);
		report.Append(Line("ladders", ladders.Count, read, over, wrong.Count));

		var inputs   = Enumerated("0a,;", 7);
		var members  = 0L;
		var beyond   = 0L;
		var closedBy = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);
		var calls    = new ConcurrentDictionary<string, int>(StringComparer.Ordinal);

		Parallel.ForEach(GiveBackFamily.Members, member =>
		{
			var fixture = new Cases.Fixture(member.Grammar, member.Parser, "ParseStart");
			var (r, o)  = Corpus(member.Name, inputs.Select(input => (fixture, input)), wrong, closedBy,
				member.Name.EndsWith("Immediate", StringComparison.Ordinal) ? calls : null);

			Interlocked.Add(ref members, r);
			Interlocked.Add(ref beyond, o);
		});

		report.Append(Line($"family ({GiveBackFamily.Members.Length} grammars x {inputs.Count} inputs)", GiveBackFamily.Members.Length * inputs.Count, members, beyond, wrong.Count));
		report.Append(string.Create(CultureInfo.InvariantCulture,
			$"closed on purpose: {closedBy.Count} grammars answered otherwise, {closedBy.Values.Sum()} inputs\n"));

		foreach (var (name, count) in closedBy.OrderBy(static one => one.Key, StringComparer.Ordinal))
			report.Append(string.Create(CultureInfo.InvariantCulture, $"  {name}: {count}\n"));

		foreach (var (named, count) in calls.OrderBy(static one => one.Key, StringComparer.Ordinal))
			report.Append(string.Create(CultureInfo.InvariantCulture, $"carried immediately, host called otherwise than by the generated parser ({named}): {count}\n"));

		foreach (var one in wrong.Take(20))
			report.Append(one).Append('\n');

		output.WriteLine(report.ToString());

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } directory)
			File.WriteAllText(Path.Combine(directory, "three-voices.txt"), report.ToString());

		Assert.True(wrong.IsEmpty, string.Join("; ", wrong.Take(20)));
		Assert.NotEmpty(closedBy);
		Assert.True(calls.ContainsKey("reopened instead"), "opening the repetitions instead calls the host as the generated parser does");
	}

	static string Line(string corpus, long inputs, long read, long over, int wrong)
	{
		return string.Create(CultureInfo.InvariantCulture, $"{corpus}: {inputs} inputs, {read} readings, {over} over budget, differing so far {wrong}\n");
	}

	/// <summary>Every text over <paramref name="alphabet"/> no longer than <paramref name="longest"/>.</summary>
	static List<string> Enumerated(string alphabet, int longest)
	{
		var all   = new List<string> { "" };
		var level = new List<string> { "" };

		for (var length = 1; length <= longest; length++)
		{
			level = [.. level.SelectMany(text => alphabet.Select(unit => text + unit))];
			all.AddRange(level);
		}

		return all;
	}

	/// <summary>
	/// The readings of a corpus, each held to the generated parser's answer; with
	/// <paramref name="closedBy"/>, the inputs the lowering with the repetitions left closed answers
	/// otherwise, counted by grammar.
	/// </summary>
	/// <param name="calls">
	/// Where given, the accepted inputs whose host calls, read replaying, are not the generated
	/// parser's, counted by lowering: for a grammar carried immediately, which calls the host as it reads.
	/// </param>
	static (long Read, long Over) Corpus(
		string corpus, IEnumerable<(Cases.Fixture Fixture, string Input)> rows, ConcurrentQueue<string> wrong,
		ConcurrentDictionary<string, int>? closedBy, ConcurrentDictionary<string, int>? calls = null)
	{
		var read = 0L;
		var over = 0L;

		foreach (var (fixture, input) in rows)
		{
			var (theirs, theirLog, _) = Reading.Generated(fixture.Parser, fixture.Entry, input);

			if (calls is not null)
			{
				foreach (var (named, options) in new[] { ("proofs", (IrLowering.Options?)null), ("all open", AllOpen), ("closed on purpose", Closed), ("reopened instead", Reopened) })
				{
					var host    = new RecordingHost(fixture.Guards);
					var outcome = new IrEvaluator(Reading.Program(fixture.Grammar, options), host).Parse(fixture.Entry, input);

					// As the contract test holds them: a refused input the generated parser reads once more, asking guards only.
					var alike = outcome.Accepted == theirs.Accepted && (theirs.Accepted
						? host.Log.SequenceEqual(theirLog)
						: theirLog.Take(host.Log.Count).SequenceEqual(host.Log) && theirLog.Skip(host.Log.Count).All(static call => call.StartsWith('g')));

					if (!alike)
					{
						calls.AddOrUpdate(named, 1, static (_, count) => count + 1);

						if (options is null && wrong.Count < 100)
							wrong.Enqueue($"{corpus} {Twin.Quoted(input)} ({named}): host [{string.Join(" ", host.Log)}], the generated parser [{string.Join(" ", theirLog)}]");
					}
				}
			}

			foreach (var options in new[] { null, AllOpen })
			{
				foreach (var resumes in new[] { false, true })
				{
					var line = Answer(fixture, input, options, resumes);

					if (line is null)
					{
						over++;

						continue;
					}

					read++;

					if (line != theirs.Line && wrong.Count < 100)
						wrong.Enqueue($"{corpus} {Twin.Quoted(input)} ({(options is null ? "proofs" : "all open")}, {(resumes ? "resuming" : "replaying")}): {line}, the generated parser {theirs.Line}");
				}
			}

			if (closedBy is not null && Answer(fixture, input, Closed, resumes: false) is { } closed && closed != theirs.Line)
				closedBy.AddOrUpdate(corpus, 1, static (_, count) => count + 1);
		}

		return (read, over);
	}

	/// <summary>The evaluator's answer, or null where it ran past its budget.</summary>
	static string? Answer(Cases.Fixture fixture, string input, IrLowering.Options? options, bool resumes)
	{
		var evaluator = new IrEvaluator(Reading.Program(fixture.Grammar, options), new RecordingHost(fixture.Guards)) { Resumes = resumes, StepBudget = 20_000_000 };

		try
		{
			return evaluator.Parse(fixture.Entry, input).Line;
		}
		catch (IrStepsExceeded)
		{
			return null;
		}
		catch (IrContractViolation broken)
		{
			return "broken: " + broken.Message;
		}
	}
}
