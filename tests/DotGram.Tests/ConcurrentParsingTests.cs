using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;
using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Sql;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;
using DotGram.Web;

using Xunit;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Tests;

/// <summary>
/// Many threads read at once, with every shipped parser, and each gets exactly what a lone
/// thread reading the same input gets.
/// </summary>
/// <remarks>
/// <para>
/// The architect's plan, item 16, written after A-030 (<c>notes/parser-instances.md</c>):
/// every mutable static an emitted parser keeps is <c>[ThreadStatic]</c> — the working stores
/// come from a per-thread pool, tables are <c>static readonly</c>, and the expression
/// language's own resolution caches are a <c>ConcurrentDictionary</c> — so concurrent calls
/// from different threads are safe by construction. That is a claim about the generator, not
/// a proof; this is the proof, and it is what the threading contract in
/// <c>docs/status.md</c> and the package READMEs is written against.
/// </para>
/// <para>
/// <b>What "the same answer" means here.</b> A generated tree's own equality is not
/// trustworthy for this — a record's compiler-written <c>Equals</c> compares an array field
/// by reference, and two separate parses never share an array — so every row below reduces
/// its result to a string before two readings are compared: <see cref="SqlWriter"/> and
/// <see cref="Ast.Sql2023Writer"/> print a tree back as text, a <c>LambdaExpression</c>
/// prints itself, and a plain record of strings (<see cref="UriReference"/>, a mailbox) is
/// joined into one line. A refusal is its position and message, or, where the public surface
/// is a bare <c>bool</c>, the bool itself.
/// </para>
/// <para>
/// <b>Cold and warm, at once.</b> <see cref="ColdThreadTests"/> already reads what a fresh
/// thread answers, one at a time. Here a batch of brand-new threads that have never read
/// anything runs beside a smaller batch of long-lived threads already several rounds into
/// reading everything, released together — so a store's first use on one thread genuinely
/// overlaps the same store's sixth use on another, rather than the two happening in series.
/// </para>
/// <para>
/// <b>Re-entrancy.</b> No shipped grammar happens to call one parse from inside another's
/// action, so that row is a small grammar built for it: the same shape
/// <c>ValueStoreRetentionTests</c> already compiles to prove a nested parse gives back what
/// it rented. <c>Outer</c>'s action calls <c>TryParseInner</c> while <c>Outer</c>'s own store
/// is still checked out — which is what the pool's "deeper spares" stack exists for
/// (<c>notes/parser-instances.md</c>) — now with many threads doing it at once instead of one.
/// </para>
/// <para>
/// <b>Bounded on purpose.</b> Every input is short, the thread counts are a small multiple of
/// <see cref="Environment.ProcessorCount"/> capped well below what a large box would otherwise
/// spin up, and every row's own expected answer is computed once, single-threaded, before any
/// thread starts. So a mismatch is unambiguous: not "the parser disagreed with itself" but "a
/// concurrent reading disagreed with the one reading that is not in question."
/// </para>
/// </remarks>
public sealed class ConcurrentParsingTests
{
	sealed class Row
	{
		public string       Name    = "";
		public Func<string> Compute = null!;
		public string       Expected = "";
	}

	[Fact]
	public void Every_shipped_parser_answers_alike_under_concurrency_and_alone()
	{
		var rows = BuildRows();

		// The single reading that is not in question: computed once, in order, before any
		// thread below exists, so nothing here can race against it.
		foreach (var row in rows)
			row.Expected = row.Compute();

		RunConcurrently(rows);
	}

	static Row[] BuildRows()
	{
		var reentrant = CompileReentrantGrammar();

		return
		[
			new Row
			{
				// The cold-thread defect's own warm input (ColdThreadTests): kept here too,
				// so the two suites are reading the same known-good statement.
				Name = "T-SQL: a SELECT that reads",
				Compute = () =>
				{
					var match = TransactSqlParser.TryParseSelect("SELECT * FROM t WHERE c = 1 AND d = 2");
					return Signature(match.IsSuccess, match.Position, match.Error, () => SqlWriter.Write(match.Value));
				},
			},
			new Row
			{
				// The cold-thread defect's own refused input: a condition whose second
				// operand is a value rather than a condition.
				Name = "T-SQL: a SELECT that is refused",
				Compute = () =>
				{
					var match = TransactSqlParser.TryParseSelect("SELECT * FROM t WHERE c = 1 AND dbo.f(1)");
					return Signature(match.IsSuccess, match.Position, match.Error, () => SqlWriter.Write(match.Value));
				},
			},
			new Row
			{
				Name = "T-SQL: the Located form of a statement",
				Compute = () =>
				{
					var match = TransactSqlParser.Located.TryParseStatement("SELECT * FROM t WHERE c = 1 AND d = 2;");
					return Signature(match.IsSuccess, match.Position, match.Error, () => SqlWriter.Write(match.Value));
				},
			},
			new Row
			{
				Name = "SQL:2023: a query expression that reads",
				Compute = () =>
				{
					var match = SqlStandardParser.TryParseQueryExpression("SELECT a FROM t WHERE a = 1");
					return Signature(match.IsSuccess, match.Position, match.Error, () => Ast.Sql2023Writer.Write(match.Value));
				},
			},
			new Row
			{
				// The SKILL's own example of what the standard refuses: TOP is T-SQL, not SQL:2023.
				Name = "SQL:2023: TOP is not standard, and is refused",
				Compute = () =>
				{
					var match = SqlStandardParser.TryParseQueryExpression("SELECT TOP 1 a FROM t");
					return Signature(match.IsSuccess, match.Position, match.Error, () => Ast.Sql2023Writer.Write(match.Value));
				},
			},
			new Row
			{
				Name = "SQL-92: a SELECT that reads",
				Compute = () =>
				{
					var match = Sql92Parser.TryParseSelect("SELECT DISTINCT a, t.* FROM u AS t WHERE a > 1");
					return Signature(match.IsSuccess, match.Position, match.Error, () => SqlWriter.Write(match.Value));
				},
			},
			new Row
			{
				Name = "SQL-92: a condition that is refused",
				Compute = () =>
				{
					var match = Sql92Parser.TryParseSearchCondition("a AND");
					return Signature(match.IsSuccess, match.Position, match.Error, () => SqlWriter.Write(match.Value));
				},
			},
			new Row
			{
				Name = "EL: a lambda that reads",
				Compute = () =>
				{
					var match = DotGram.ExpressionLanguage.ExpressionParser.TryParse(
						"(int x) => { var i = x; System.Math.Abs(i); return i; }",
						typeof(ConcurrentParsingTests).Assembly);
					return Signature(match.IsSuccess, match.Position, match.Error, () => match.Value.ToString());
				},
			},
			new Row
			{
				// Reads as syntax and is refused in resolution: nothing brings System.Math.Abs
				// into scope, which exercises the member-resolution caches under load and not
				// only the grammar.
				Name = "EL: an unresolved call is refused",
				Compute = () =>
				{
					var match = DotGram.ExpressionLanguage.ExpressionParser.TryParse("() => Abs(-2)");
					return Signature(match.IsSuccess, match.Position, match.Error, () => match.Value.ToString());
				},
			},
			new Row
			{
				Name = "Web: a URI reference that reads",
				Compute = () =>
				{
					var read = UriReference.TryParse("https://example.org/a/b?c=1#d", out var reference);

					return read
						? "OK:" + string.Join("|", reference!.Scheme, reference.UserInfo, reference.Host,
							reference.Port, reference.Path, reference.Query, reference.Fragment)
						: "REFUSED";
				},
			},
			new Row
			{
				Name = "Web: a URI reference with a space is refused",
				Compute = () => UriReference.TryParse("http://exa mple.com/", out _) ? "READ" : "REFUSED",
			},
			new Row
			{
				Name = "Web: a mailbox list that reads",
				Compute = () =>
				{
					var read = EmailAddress.TryParseMailboxList(
						"Alice <alice@example.com>, Bob <bob@example.org>", out var mailboxes);

					return read
						? "OK:" + string.Join(";", mailboxes!.Select(one =>
							one.DisplayName + "<" + one.Address.LocalPart + "@" + one.Address.Domain + ">"))
						: "REFUSED";
				},
			},
			new Row
			{
				Name = "Web: a mailbox list with no local part is refused",
				Compute = () => EmailAddress.TryParseMailboxList("@@@, not-an-email", out _) ? "READ" : "REFUSED",
			},
			new Row
			{
				Name = "FIX: fields of a message that reads",
				Compute = () => FieldsSignature(FixParser.ParseFields(FixMessage("0", "112=TEST|"))),
			},
			new Row
			{
				// The streamed publication: a fresh reader per call, a small buffer so it
				// refills several times over one short message.
				Name = "FIX: a message streamed through a small buffer",
				Compute = () =>
				{
					using var reader = new StringReader(FixMessage("0", "112=TEST|"));

					return FieldsSignature(FixParser.ReadFields(reader, new Fix44Context { BufferSize = 3 }).ToArray());
				},
			},
			new Row
			{
				Name = "FIX: a message truncated before its checksum is refused",
				Compute = () =>
				{
					var wire = FixMessage("0", "112=TEST|");
					var read = FixParser.TryParseMessage(wire[..(wire.Length - 5)], out _, out var error);

					return read ? "READ" : "REFUSED:" + error;
				},
			},
			new Row
			{
				Name = "Re-entrant: a parse from inside another's action",
				Compute = () => ReentrantSignature(reentrant, "[ab][cde]"),
			},
			new Row
			{
				Name = "Re-entrant: the outer parse refused",
				Compute = () => ReentrantSignature(reentrant, "[ab][XY]"),
			},
		];
	}

	/// <summary>
	/// Many threads, released together, each reading every row and holding it to the answer
	/// computed before any of them started.
	/// </summary>
	/// <remarks>
	/// A smaller batch of long-lived threads loops several rounds; a same-sized batch of
	/// brand-new threads, each used exactly once, is started while the long-lived ones are
	/// still mid-loop — so a store's first use and its later uses genuinely overlap.
	/// </remarks>
	static void RunConcurrently(Row[] rows)
	{
		const int RoundsPerWarmThread = 6;

		var threads = Math.Clamp(Environment.ProcessorCount, 4, 24);

		var failures = new ConcurrentQueue<string>();
		var ready    = new CountdownEvent(threads);
		var go       = new ManualResetEventSlim(false);

		void CheckAll(string origin)
		{
			foreach (var row in rows)
			{
				string actual;

				try
				{
					actual = row.Compute();
				}
				catch (Exception exception)
				{
					failures.Enqueue($"{origin}, {row.Name}: threw {exception}");
					continue;
				}

				if (actual != row.Expected)
					failures.Enqueue($"{origin}, {row.Name}: expected <{row.Expected}> but got <{actual}>");
			}
		}

		var warmThreads = new Thread[threads];

		for (var w = 0; w < threads; w++)
		{
			var origin = "warm" + w.ToString(CultureInfo.InvariantCulture);

			warmThreads[w] = new Thread(() =>
			{
				ready.Signal();
				go.Wait();

				for (var round = 0; round < RoundsPerWarmThread; round++)
					CheckAll(origin + "/round" + round.ToString(CultureInfo.InvariantCulture));
			})
			{ IsBackground = true };
		}

		foreach (var thread in warmThreads)
			thread.Start();

		ready.Wait();
		go.Set();

		var coldThreads = new Thread[threads];

		for (var c = 0; c < threads; c++)
		{
			var origin = "cold" + c.ToString(CultureInfo.InvariantCulture);

			coldThreads[c] = new Thread(() => CheckAll(origin)) { IsBackground = true };
			coldThreads[c].Start();
		}

		foreach (var thread in coldThreads)
			thread.Join();

		foreach (var thread in warmThreads)
			thread.Join();

		Assert.True(failures.IsEmpty, Environment.NewLine + string.Join(Environment.NewLine, failures));
	}

	static string Signature(bool isSuccess, long position, string? error, Func<string> whenSuccess)
	{
		return isSuccess
			? "OK:" + whenSuccess()
			: "REFUSED:" + position.ToString(CultureInfo.InvariantCulture) + ":" + error;
	}

	static string FieldsSignature(IReadOnlyList<FixField> fields)
	{
		return string.Join(";", fields.Select(field =>
			field.Tag.ToString(CultureInfo.InvariantCulture) + ":" +
			field.Position.ToString(CultureInfo.InvariantCulture) + ":" +
			field.Length.ToString(CultureInfo.InvariantCulture) + ":" +
			field.IsValid));
	}

	/// <summary>A standard FIX 4.4 message of the given type around a body written with '|' for SOH.</summary>
	/// <remarks>The same shape <c>DotGram.Finance.Tests.FixFixtures.Wire</c> builds, kept here so this
	/// project does not take a dependency on that one for one small helper.</remarks>
	static string FixMessage(string type, string body)
	{
		body = "35=" + type + "\u000149=SENDER\u000156=TARGET\u000134=1\u000152=20260915-12:00:00\u0001"
			+ body.Replace('|', '\u0001');

		var prefix   = "8=FIX.4.4\u00019=" + body.Length.ToString(CultureInfo.InvariantCulture) + "\u0001" + body;
		var checksum = prefix.Aggregate(0, (sum, c) => (sum + c) & 255);

		return prefix + "10=" + checksum.ToString("000", CultureInfo.InvariantCulture) + "\u0001";
	}

	// ── The re-entrant row's own grammar ─────────────────────────────────────────

	/// <summary>
	/// <c>Outer</c>'s action calls <c>TryParseInner</c> — the generated class's own public
	/// entry point — while <c>Outer</c>'s parse is still in progress, which is exactly what
	/// no shipped grammar happens to do and what the pool's "deeper spares" stack exists for.
	/// </summary>
	const string ReentrantGrammar = """
		Outer : @int = rows: Row+ => @(rows.Length)
		Row : @string = '[' & text: Letters & ']' => @(Nested(text.ToString()))
		Letters = ['a'..'z']+
		Inner : @string = parts: Part+ => @(string.Concat(parts))
		Part : @string = c: ['a'..'z'] => @(c.ToString())
		parse Outer
		parse Inner
		""";

	const string ReentrantMembers = """
		static string Nested(string text) => TryParseInner(text).Value!;
		""";

	static (object Assembly, MethodInfo TryParseOuter) CompileReentrantGrammar()
	{
		var compiled = GramCompiler.Compile(ReentrantGrammar, new GramCompilerOptions
		{
			ClassName = "ReentrantGrammar", Carrier = CarrierKind.Tape, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity != GramSeverity.Info);

		var source   = Assert.Single(compiled.Sources).Text;
		var assembly = EmittedCode.Compile(source, className: "ReentrantGrammar", declarationMembers: ReentrantMembers);
		var type     = assembly.GetType("ReentrantGrammar")!;
		var method   = type.GetMethod("TryParseOuter", [typeof(string)])!;

		return (assembly, method);
	}

	static string ReentrantSignature((object Assembly, MethodInfo TryParseOuter) reentrant, string input)
	{
		var match = reentrant.TryParseOuter.Invoke(null, [input])!;
		var type  = match.GetType();

		var isSuccess = (bool)type.GetProperty("IsSuccess")!.GetValue(match)!;
		var position  = (long)type.GetProperty("Position")!.GetValue(match)!;
		var error     = (string?)type.GetProperty("Error")!.GetValue(match);

		return isSuccess
			? "OK:" + ((int)type.GetProperty("Value")!.GetValue(match)!).ToString(CultureInfo.InvariantCulture)
			: "REFUSED:" + position.ToString(CultureInfo.InvariantCulture) + ":" + error;
	}
}
