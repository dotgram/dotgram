using System;
using System.Globalization;
using System.IO;

using BenchmarkDotNet.Attributes;

using DotGram.Finance.Fix;

namespace DotGram.Finance.Benchmarks;

/// <summary>
/// This package against QuickFIX/n, on the two things a consumer does with a message: read it,
/// and hold it to a schema.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Both sides do the same work in every row.</strong> Their call parses and validates in
/// one movement where ours is two, so a row is never our one call against their two: parse is
/// paired with parse, and parse-and-validate with parse-and-validate.
/// </para>
/// <para>
/// <strong>And every row READS FIELDS.</strong> Ours reads a field from the source when asked,
/// theirs builds a field map while parsing, so a row that parsed and never looked at a field would
/// measure our laziness and call it speed. Eight fields of the order are read on both sides — six
/// as text and two as decimals — and the count is part of the row's name, because it is part of
/// what was measured.
/// </para>
/// <para>
/// <strong>The asymmetry is the point, not a flaw.</strong> Our schema is compiled into the
/// package AND can be loaded from a dictionary at run time; QuickFIX/n has only the second. So
/// this runs our two modes against their one, and the loaded mode reads the same file they read.
/// </para>
/// <para>
/// Their side is their documented entry point, <c>Message.FromString</c>, with the FIX 4.4 message
/// factory so that they build the typed message we build, and with the dictionary they need for
/// groups. Their <c>validate</c> flag is what makes a row the validating one. The version is read
/// from the package, not remembered: QuickFIXn.Core and QuickFIXn.FIX44 1.14.1.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class FixAgainstQuickFix
{
	/// <summary>Where our schema comes from: compiled into the package, or read from the file.</summary>
	/// <remarks>
	/// A rule lives in a static field on the message class (D115), so this is process-wide state.
	/// It is set up and put back per case, and BenchmarkDotNet launches a process a case, so the
	/// two modes cannot reach each other.
	/// </remarks>
	public enum Schema
	{
		Compiled,
		Dictionary,
	}

	[Params(Schema.Compiled, Schema.Dictionary)]
	public Schema From { get; set; }

	string _wire = "";
	QuickFix.DataDictionary.DataDictionary _theirs = null!;
	QuickFix.FIX44.MessageFactory _factory = null!;

	static string Dictionary() => Path.Combine(AppContext.BaseDirectory, "FIX44.xml");

	[GlobalSetup]
	public void Setup()
	{
		_wire = Fix44Benchmarks.Wire(
			"D", "11=ORDER|21=1|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|59=0|");

		_theirs  = new QuickFix.DataDictionary.DataDictionary(Dictionary());
		_factory = new QuickFix.FIX44.MessageFactory();

		if (From == Schema.Dictionary)
			using (var file = File.OpenRead(Dictionary()))
				FixParser.LoadDictionary(file);

		// Both sides answer before either is timed: a side that is faster because it refused the
		// message is not faster.
		if (OursParseAndValidate() != 0 || TheirsParseAndValidate() == 0)
			throw new InvalidOperationException("The two sides do not agree that this message is valid.");
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		if (From == Schema.Dictionary)
			FixRuleFields.Compiled();
	}

	// ── reading a message ────────────────────────────────────────────────────────────────────

	/// <summary>Parse, then read eight fields.</summary>
	[Benchmark(Description = "ours: parse + 8 fields")]
	public int OursParse()
	{
		var order = (FixMessage.NewOrderSingle)FixParser.ParseMessage(_wire);

		return Read(order);
	}

	/// <summary>The same, on their side: their entry point without validation, then eight fields.</summary>
	[Benchmark(Description = "QuickFIX/n: parse + 8 fields")]
	public int TheirsParse()
	{
		var message = new QuickFix.Message();

		message.FromString(_wire, false, _theirs, _theirs, _factory);

		return Read(message);
	}

	// ── reading it and holding it to the schema ──────────────────────────────────────────────

	/// <summary>Parse, validate, then read eight fields.</summary>
	/// <returns>How many findings the schema had about it, which is zero for this message.</returns>
	[Benchmark(Description = "ours: parse + validate + 8 fields")]
	public int OursParseAndValidate()
	{
		var message = FixParser.ParseMessage(_wire);
		var found   = message.Validate().Length;

		Read((FixMessage.NewOrderSingle)message);

		return found;
	}

	/// <summary>The same movement on their side, which is one call because theirs validates inside it.</summary>
	[Benchmark(Description = "QuickFIX/n: parse + validate + 8 fields")]
	public int TheirsParseAndValidate()
	{
		var message = new QuickFix.Message();

		message.FromString(_wire, true, _theirs, _theirs, _factory);

		return Read(message);
	}

	// ── the same work under another name, so that a ratio can be read ────────────────────────

	/// <summary>One of the rows above, run again under a second name.</summary>
	/// <remarks>
	/// The pair of these is the resolution of the hour: two rows doing identical work should come
	/// out identical, and whatever they differ by is what this machine could not tell apart while
	/// this run was going. A ratio quoted without the A/A of the same run is not quotable — on this
	/// machine two identical methods have come out 21% and 28% apart when the hour was bad.
	/// </remarks>
	[Benchmark(Description = "A/A ours: parse + 8 fields")]
	public int OursParseAgain()
	{
		var order = (FixMessage.NewOrderSingle)FixParser.ParseMessage(_wire);

		return Read(order);
	}

	/// <inheritdoc cref="OursParseAgain"/>
	[Benchmark(Description = "A/A QuickFIX/n: parse + 8 fields")]
	public int TheirsParseAgain()
	{
		var message = new QuickFix.Message();

		message.FromString(_wire, false, _theirs, _theirs, _factory);

		return Read(message);
	}

	// ── the eight fields, read the same way on both sides ────────────────────────────────────

	/// <summary>Six text fields and two decimals, which is what a consumer's code does next.</summary>
	static int Read(FixMessage.NewOrderSingle order)
	{
		var sink = 0;

		sink += order.ClOrdID?.Length      ?? 0;
		sink += order.Symbol?.Length       ?? 0;
		sink += order.Side?.Length         ?? 0;
		sink += order.TransactTime?.Length ?? 0;
		sink += order.OrdType?.Length      ?? 0;
		sink += order.TimeInForce?.Length  ?? 0;

		if (order.OrderQty is { } quantity && quantity.TryGetDecimal(out var got))
			sink += (int)got;

		if (order.Price is { } price && price.TryGetDecimal(out var paid))
			sink += (int)paid;

		return sink;
	}

	/// <inheritdoc cref="Read(FixMessage.NewOrderSingle)"/>
	static int Read(QuickFix.Message message)
	{
		var sink = 0;

		sink += message.GetString(11).Length;
		sink += message.GetString(55).Length;
		sink += message.GetString(54).Length;
		sink += message.GetString(60).Length;
		sink += message.GetString(40).Length;
		sink += message.GetString(59).Length;
		sink += (int)message.GetDecimal(38);
		sink += (int)message.GetDecimal(44);

		return sink;
	}

	/// <summary>
	/// The rows answered once each, printed, with no timing: what the two sides say and how much
	/// each allocates is checkable without a window.
	/// </summary>
	/// <remarks>
	/// A comparison is worth nothing until the two sides are doing the same work, and that is a
	/// question of ANSWERS, not of nanoseconds. This is the check that they agree, and it is here
	/// rather than in a comment so that it can be run.
	/// </remarks>
	public static void Check()
	{
		foreach (var from in new[] { Schema.Compiled, Schema.Dictionary })
		{
			var run = new FixAgainstQuickFix { From = from };

			run.Setup();

			Console.WriteLine(
				$"{from,-10}  ours parse {run.OursParse()}, theirs parse {run.TheirsParse()}, " +
				$"ours parse+validate {run.OursParseAndValidate()} findings, " +
				$"theirs parse+validate {run.TheirsParseAndValidate()}");

			if (run.OursParse() != run.TheirsParse())
				throw new InvalidOperationException(
					$"The two sides read different fields: {run.OursParse()} against {run.TheirsParse()}.");

			run.Cleanup();
		}

		Console.WriteLine("both sides read the same eight fields and agree the message is valid.");
	}
}
