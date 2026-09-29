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
/// Many threads read at once, with a parser of every shipped package, and each gets exactly
/// what a lone thread reading the same input gets.
/// </summary>
/// <remarks>
/// <para>
/// A generated parser keeps its working state in per-thread stores and reads its tables from
/// <c>static readonly</c> fields, which is a design that should make two threads reading at
/// once safe by construction: neither should ever see the other's state, and neither should
/// have to wait for it. That is a claim about the design, not a proof — a field one call site
/// forgot to make per-thread, or a table one grammar built as mutable, would only show up
/// under real concurrency, and nothing else in the suite reads more than one thread at a time.
/// This is that proof.
/// </para>
/// <para>
/// <b>What "the same answer" means here.</b> A generated tree's own equality is not
/// trustworthy for this — a record's compiler-written <c>Equals</c> compares an array field
/// by reference, and two separate parses never share an array — so every row below reduces
/// its result to a string before two readings are compared: <see cref="SqlWriter"/> and
/// <see cref="Ast.Sql2023Writer"/> print a tree back as text, a <c>LambdaExpression</c>
/// prints itself, and a plain record of strings (<see cref="UriReference"/>, a mailbox) is
/// joined into one line. A refusal is its position and message, or, where the public surface
/// is a bare <c>bool</c>, the bool itself. Where a tree carries positions — today, only the
/// T-SQL <c>Located</c> form; <c>SqlStandardParser</c>'s own tree has the same <c>Span</c> on
/// every node but does not populate it yet — the printed text is not enough by itself: two
/// trees that print alike can still disagree about where each piece was found, which is
/// exactly the shape a race that swaps two threads' spans would take. So both rows fold every
/// node's own <c>(At, Length)</c> into the signature too, the SQL:2023 one for whenever that
/// changes and at no cost while it has not.
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
/// is still checked out, now with many threads doing it at once instead of one.
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
	public void A_parser_of_every_shipped_package_answers_alike_under_concurrency_and_alone()
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
				// The Located form carries a Span on every node; a race that swapped two
				// threads' positions but left the tree's shape alone would print the same
				// text, so the signature folds every node's (At, Length) in too.
				Name = "T-SQL: the Located form of a statement",
				Compute = () =>
				{
					var match = TransactSqlParser.Located.TryParseStatement("SELECT * FROM t WHERE c = 1 AND d = 2;");
					return Signature(match.IsSuccess, match.Position, match.Error,
						() => SqlWriter.Write(match.Value) + "|" + SpansSignature(match.Value));
				},
			},
			new Row
			{
				// Every SQL:2023 Ast node has the same Span property the Located row above
				// has (ISqlNode extends ISqlSpan), though SqlStandardParser does not populate
				// it today — this folds spans in anyway, so the row starts pulling its weight
				// the moment that changes, and costs nothing (every span is 0/0) while it has not.
				Name = "SQL:2023: a query expression that reads",
				Compute = () =>
				{
					var match = SqlStandardParser.TryParseQueryExpression("SELECT a FROM t WHERE a = 1");
					return Signature(match.IsSuccess, match.Position, match.Error,
						() => Ast.Sql2023Writer.Write(match.Value) + "|" + SpansSignature(match.Value));
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
				// A different Web package class again (Rfc8259 owns its own emitted pool),
				// with an object, an array, a number kept as text, a boolean and a null —
				// one of each of the six cases the tree is closed over.
				Name = "Web: a JSON value that reads",
				Compute = () =>
				{
					var read = JsonValue.TryParse("""{"a":[1,2,true,null],"b":"x"}""", out var value);

					return read ? "OK:" + value!.ToString() : "REFUSED";
				},
			},
			new Row
			{
				Name = "Web: an unterminated JSON object is refused",
				Compute = () => JsonValue.TryParse("{", out _) ? "READ" : "REFUSED",
			},
			new Row
			{
				// A third Web package class (Rfc9651, its own emitted pool again): a
				// structured-field Item, an integer bare value with two parameters.
				Name = "Web: a structured-field item that reads",
				Compute = () =>
				{
					var read = StructuredField.TryParseItem("42;a=1;b=?0", out var item);

					return read
						? "OK:" + BareItemText(item!.Value) + "|" + ParametersText(item.Parameters)
						: "REFUSED";
				},
			},
			new Row
			{
				Name = "Web: an unterminated structured-field string is refused",
				Compute = () => StructuredField.TryParseItem("\"unterminated", out _) ? "READ" : "REFUSED",
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

	/// <summary>
	/// Every node under <paramref name="root"/>, in the walk's own order, as its <c>(At, Length)</c>.
	/// </summary>
	/// <remarks>
	/// Printed text alone would miss a race that swaps two nodes' recorded positions but leaves
	/// the tree's shape and every value untouched — <see cref="SqlWalker"/> walks both the
	/// shared T-SQL/SQL-92 tree and the SQL:2023 one, since <c>ISqlNode</c> extends <see cref="ISqlSpan"/>.
	/// </remarks>
	static string SpansSignature(ISqlSpan root)
	{
		var spans = new List<string>();

		SqlWalker.Walk(root, node =>
		{
			spans.Add(node.Span.At.ToString(CultureInfo.InvariantCulture) + "/" +
				node.Span.Length.ToString(CultureInfo.InvariantCulture));

			return true;
		});

		return string.Join(",", spans);
	}

	/// <summary>A structured-field bare item as one line: <c>BareItem</c> is a closed set of eight cases.</summary>
	static string BareItemText(BareItem item)
	{
		return item switch
		{
			BareItem.Integer       one => "i:" + one.Value.ToString(CultureInfo.InvariantCulture),
			BareItem.Decimal       one => "d:" + one.Value.ToString(CultureInfo.InvariantCulture),
			BareItem.String        one => "s:" + one.Value,
			BareItem.Token         one => "t:" + one.Value,
			BareItem.ByteSequence  one => "b:" + Convert.ToBase64String(one.Value),
			BareItem.Boolean       one => "?:" + one.Value,
			BareItem.Date          one => "date:" + one.Value.ToString(CultureInfo.InvariantCulture),
			BareItem.DisplayString one => "ds:" + one.Value,
			_                          => throw new NotSupportedException(item.GetType().Name),
		};
	}

	static string ParametersText(OrderedMap<BareItem> parameters)
	{
		return string.Join(",", parameters.Select(pair => pair.Key + "=" + BareItemText(pair.Value)));
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
