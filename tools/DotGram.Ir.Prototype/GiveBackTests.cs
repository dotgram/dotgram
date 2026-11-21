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

	/// <summary>The repetitions left closed where their turns decide: wrong on purpose.</summary>
	static readonly IrLowering.Options Closed = new(ClosesDecidingTurns: true);

	/// <summary>
	/// <c>00a,b;c;</c>: the second turn of <c>(B* &amp; Letter &amp; ';')+</c> has <c>B*</c> give the
	/// <c>c</c> back. Accepted as the generated parser accepts it, with its host calls; refused with
	/// the repetition left closed, since the braces have sealed before <c>eof</c> fails.
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
		Assert.Equal(1, Reading.Program(fixture.Grammar).Reopened);
	}

	/// <summary>
	/// <c>0"a,b;"</c>, the same past a one-character stop, each <c>A</c> built: left closed, the turn
	/// given up is read again by the replay of the whole rule, which answers alike and builds <c>A*</c>
	/// once more; opened, the turn is read again alone, and the host is called as the generated parser
	/// calls it.
	/// </summary>
	[Fact]
	public void ATurnPastAStop_GivesBackInsideItself()
	{
		const string input = "0\"a,b;\"";

		var grammar = GiveBackGrammars.GivesBackPastStopBuilt;

		var (theirs, theirLog, _)  = Reading.Generated(typeof(GivesBackPastStopBuiltParser), "ParseStart", input);
		var (ours, ourLog, _)      = Reading.Evaluated(grammar, "ParseStart", input);
		var (closed, closedLog, _) = Reading.Evaluated(grammar, "ParseStart", input, options: Closed);

		Assert.True(theirs.Accepted);
		Assert.Equal(theirs.Line, ours.Line);
		Assert.Equal(theirs.Line, closed.Line);
		Assert.Equal(theirLog, ourLog);
		Assert.Equal(["c1(\"0\")", "c1(\"0\")"], closedLog.Where(static call => call.StartsWith("c1", StringComparison.Ordinal)));
		Assert.Equal(1, Reading.Program(grammar).Reopened);
	}

	/// <summary>
	/// What the tree does not yet read as the generated parser does, said as it is: a turn below the
	/// fewest that fails after a decision of its own is read again by the replay around the
	/// repetition, not alone. The answer is the same; the host is called for what that replay reads
	/// before the repetition once more. The generated reader reads every turn in a loop of its own,
	/// below the fewest too, and the tree has such a loop only past the fewest of a repetition that
	/// writes a way: the first turn of <c>+</c> on <c>00a,b;c;</c> builds <c>A*</c> twice.
	/// </summary>
	[Fact]
	public void ATurnBelowTheFewest_IsReadAgainByTheReplayAroundIt()
	{
		const string input = "00a,b;c;";

		var grammar = GiveBackGrammars.GivesBackInTurnBuilt;

		var (theirs, theirLog, _) = Reading.Generated(typeof(GivesBackInTurnBuiltParser), "ParseStart", input);

		foreach (var options in new[] { null, AllOpen })
		{
			var (ours, ourLog, _) = Reading.Evaluated(grammar, "ParseStart", input, options: options);

			Assert.Equal(theirs.Line, ours.Line);
			Assert.Equal(["c1(\"0\")", "c1(\"0\")", "c0([c1(\"0\"),c1(\"0\")])"], theirLog);
			Assert.Equal(["c1(\"0\")", "c1(\"0\")", "c1(\"0\")", "c1(\"0\")", "c0([c1(\"0\"),c1(\"0\")])"], ourLog);
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
			report.Append(string.Create(CultureInfo.InvariantCulture, $"accepted, carried immediately, host called otherwise than by the generated parser ({named}): {count}\n"));

		foreach (var one in wrong.Take(20))
			report.Append(one).Append('\n');

		output.WriteLine(report.ToString());

		if (Environment.GetEnvironmentVariable("IR_REPORT") is { } directory)
			File.WriteAllText(Path.Combine(directory, "three-voices.txt"), report.ToString());

		Assert.True(wrong.IsEmpty, string.Join("; ", wrong.Take(20)));
		Assert.NotEmpty(closedBy);
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

			if (calls is not null && theirs.Accepted)
			{
				foreach (var (named, options) in new[] { ("proofs", (IrLowering.Options?)null), ("all open", AllOpen), ("closed on purpose", Closed) })
				{
					var host = new RecordingHost(fixture.Guards);

					new IrEvaluator(Reading.Program(fixture.Grammar, options), host).Parse(fixture.Entry, input);

					if (!host.Log.SequenceEqual(theirLog))
						calls.AddOrUpdate(named, 1, static (_, count) => count + 1);
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
