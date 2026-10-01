using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// <see cref="SqlScript"/>: a script cut into batches as sqlcmd cuts it, held to what sqlcmd sent.
/// </summary>
/// <remarks>
/// <para>
/// <c>Scripts/sqlcmd-oracle.json</c> is the statement of what the reader does. Each row is a script,
/// and what ODBC sqlcmd 18.6 for Linux sent the server for it, captured on the server by an Extended
/// Events session on <c>sql_batch_starting</c> — the exact text of every batch, after substitution,
/// once for each time it was sent — with whether sqlcmd stopped on a syntax error and how many
/// times it said a variable was not defined. A row is compared byte for byte.
/// </para>
/// <para>
/// The rows marked <c>runs</c> are where sqlcmd ran a command the reader only reports — <c>exit</c>,
/// <c>quit</c>, <c>:reset</c>, <c>:connect</c>, and the two sqlcmd for Linux stops at
/// (<c>:serverlist</c>, <c>:xml</c>) — and they carry what the reader sends instead: the same text
/// with the command's line cut out and nothing else changed.
/// </para>
/// </remarks>
public sealed class SqlScriptTests
{
	static string Here([System.Runtime.CompilerServices.CallerFilePath] string here = "")
	{
		return Path.GetDirectoryName(here)!;
	}

	static readonly Lazy<JsonElement[]> Oracle = new(static () =>
	{
		var text = File.ReadAllText(Path.Combine(Here(), "Scripts", "sqlcmd-oracle.json"));

		return [.. JsonDocument.Parse(text).RootElement.EnumerateArray()];
	});

	public static TheoryData<string> Rows()
	{
		var rows = new TheoryData<string>();

		foreach (var row in Oracle.Value)
			rows.Add(row.GetProperty("name").GetString()!);

		return rows;
	}

	[Theory]
	[MemberData(nameof(Rows))]
	public void A_script_is_cut_as_sqlcmd_cuts_it(string name)
	{
		var row    = Oracle.Value.Single(one => one.GetProperty("name").GetString() == name);
		var text   = row.GetProperty("text").GetString()!;
		var files  = Strings(row, "files");
		var script = SqlScript.Read(text, new ScriptOptions
		{
			Variables           = Strings(row, "variables"),
			SubstituteVariables = !row.TryGetProperty("noSubstitution", out _),
			SourceName          = "probe.sql",
			ResolveInclude      = include => files.TryGetValue(include.Path, out var file) ? new ScriptSource(include.Path, file) : null,
		});

		var loops    = row.TryGetProperty("loops", out _);
		var expected = row.GetProperty(row.TryGetProperty("expected", out _) ? "expected" : "sqlcmd")
			.EnumerateArray().Select(static one => one.GetString()!).ToArray();
		var sent     = script.Batches
			.SelectMany(static batch => Enumerable.Repeat(batch.ToString(), (int)Math.Min(batch.Count, 16)))
			.ToArray();

		Assert.Equal(expected, loops ? sent.Take(expected.Length) : sent);
		Assert.Equal(row.TryGetProperty("fatal", out _), script.Diagnostics.Any(static one => one.Severity == ScriptSeverity.Fatal));

		var undefined = row.TryGetProperty("undefined", out var count) ? count.GetInt32() : 0;

		Assert.Equal(undefined, script.Diagnostics.Count(static one => one.Message.EndsWith("scripting variable not defined.", StringComparison.Ordinal)));

		if (loops)
			Assert.Equal(long.MaxValue, script.Batches[0].Count);
	}

	static Dictionary<string, string> Strings(JsonElement row, string property)
	{
		var strings = new Dictionary<string, string>(StringComparer.Ordinal);

		if (row.TryGetProperty(property, out var map))
		{
			foreach (var one in map.EnumerateObject())
				strings[one.Name] = one.Value.GetString()!;
		}

		return strings;
	}

	// ── Where a batch was written ────────────────────────────────────────────

	/// <summary>
	/// A batch written as one run, with nothing substituted, is a window of the script itself: the
	/// same string, so a position in it is a position in the script.
	/// </summary>
	[Fact]
	public void A_verbatim_batch_is_a_window_of_the_script_itself()
	{
		var text   = "SELECT 1\r\nGO\r\nSELECT 2\r\nGO 3\r\n  SELECT 3";
		var script = SqlScript.Read(text, new ScriptOptions { SourceName = "a.sql" });

		Assert.Equal(3, script.Batches.Count);

		foreach (var batch in script.Batches)
		{
			Assert.Same(text, batch.Text);
			Assert.True(batch.IsVerbatim);
			Assert.Equal(new ScriptLocation("a.sql", new SqlSpan(batch.At + 2, 3)), batch.Locate(batch.At + 2, 3));
		}

		Assert.Equal(["SELECT 1\r\n", "SELECT 2\r\n", "  SELECT 3"], script.Batches.Select(static one => one.ToString()));
		Assert.Equal([1L, 3L, 1L], script.Batches.Select(static one => one.Count));
		Assert.Equal(new SqlSpan(10, 2), script.Batches[0].Separator!.Value.Span);
		Assert.Null(script.Batches[2].Separator);
	}

	/// <summary>
	/// A substituted value maps back to the whole reference: a range never claims part of one.
	/// </summary>
	[Fact]
	public void A_position_in_a_value_maps_to_its_reference()
	{
		var text   = ":setvar table Orders\nSELECT * FROM $(table) WHERE 1 = 1\n";
		var script = SqlScript.Read(text);
		var batch  = Assert.Single(script.Batches);
		var sent   = batch.ToString();

		Assert.Equal("SELECT * FROM Orders WHERE 1 = 1\n", sent);
		Assert.False(batch.IsVerbatim);
		Assert.Equal(0, batch.At);

		var reference = text.IndexOf("$(table)", StringComparison.Ordinal);
		var value     = sent.IndexOf("Orders", StringComparison.Ordinal);

		Assert.Equal(new SqlSpan(reference, 8), batch.Locate(value, 6).Span);
		Assert.Equal(new SqlSpan(reference, 8), batch.Locate(value + 2, 2).Span);
		Assert.Equal(new SqlSpan(text.IndexOf("SELECT", StringComparison.Ordinal), 6), batch.Locate(0, 6).Span);

		var where = sent.IndexOf("WHERE", StringComparison.Ordinal);

		Assert.Equal(new SqlSpan(text.IndexOf("WHERE", StringComparison.Ordinal), 5), batch.Locate(where, 5).Span);

		// From before the reference to after it: the whole of it, in the script's own offsets.
		Assert.Equal(
			new SqlSpan(text.IndexOf("FROM", StringComparison.Ordinal), text.IndexOf(" WHERE", StringComparison.Ordinal) - text.IndexOf("FROM", StringComparison.Ordinal)),
			batch.Locate(sent.IndexOf("FROM", StringComparison.Ordinal), 11).Span);
	}

	/// <summary>
	/// A command line inside a batch is cut out of what is sent, so the batch is two runs; one
	/// before the batch's first line leaves it one.
	/// </summary>
	[Fact]
	public void A_command_inside_a_batch_makes_two_runs()
	{
		var text   = ":setvar x 1\nSELECT 1\n:on error exit\nSELECT 2\nGO\n";
		var script = SqlScript.Read(text);
		var batch  = Assert.Single(script.Batches);

		Assert.Equal("SELECT 1\nSELECT 2\n", batch.ToString());
		Assert.False(batch.IsVerbatim);
		Assert.Equal(new SqlSpan(text.IndexOf("SELECT 2", StringComparison.Ordinal), 8), batch.Locate(9, 8).Span);

		var leading = SqlScript.Read(":setvar x 1\nSELECT 1\nGO\n").Batches.Single();

		Assert.True(leading.IsVerbatim);
	}

	/// <summary>
	/// An included file's text is read in place, and a position in it maps to that file.
	/// </summary>
	[Fact]
	public void An_included_file_maps_to_itself()
	{
		var files = new Dictionary<string, string>
		{
			["inner.sql"] = "SELECT 2\nGO\nSELECT 3\n",
		};
		var script = SqlScript.Read("SELECT 1\n:r inner.sql\nSELECT 4\n", new ScriptOptions
		{
			SourceName     = "outer.sql",
			ResolveInclude = include => new ScriptSource(include.Path, files[include.Path]),
		});

		Assert.Equal(["SELECT 1\nSELECT 2\n", "SELECT 3\nSELECT 4\n"], script.Batches.Select(static one => one.ToString()));

		var first = script.Batches[0];

		Assert.Equal(new ScriptLocation("outer.sql", new SqlSpan(0, 6)), first.Locate(0, 6));
		Assert.Equal(new ScriptLocation("inner.sql", new SqlSpan(0, 6)), first.Locate(9, 6));

		// A range from one file into the next ends where its first file's part does.
		Assert.Equal(new ScriptLocation("outer.sql", new SqlSpan(0, 9)), first.Locate(0, 15));

		var directive = Assert.Single(script.Directives);

		Assert.Equal(("r", "inner.sql", 0), (directive.Name, directive.Arguments, directive.Batch));
		Assert.Equal(new ScriptLocation("outer.sql", new SqlSpan(9, 12)), directive.Location);
	}

	[Fact]
	public void An_include_with_nothing_to_resolve_it_is_an_error_and_the_reading_goes_on()
	{
		var script = SqlScript.Read("SELECT 1\n:r missing.sql\nSELECT 2\n");

		Assert.Equal("SELECT 1\nSELECT 2\n", Assert.Single(script.Batches).ToString());

		var diagnostic = Assert.Single(script.Diagnostics);

		Assert.Equal(ScriptSeverity.Error, diagnostic.Severity);
		Assert.Equal("'missing.sql': Invalid filename.", diagnostic.Message);
	}

	[Fact]
	public void A_file_that_includes_itself_is_an_error_and_is_not_read_again()
	{
		var files = new Dictionary<string, string>
		{
			["a.sql"] = ":r b.sql\nSELECT 1\n",
			["b.sql"] = ":r a.sql\nSELECT 2\n",
		};
		var script = SqlScript.Read(":r a.sql\nSELECT 9\n", new ScriptOptions
		{
			ResolveInclude = include => new ScriptSource(include.Path, files[include.Path]),
		});

		Assert.Equal("SELECT 2\nSELECT 1\nSELECT 9\n", Assert.Single(script.Batches).ToString());
		Assert.Equal("File 'a.sql' recursively included.", Assert.Single(script.Diagnostics).Message);
	}

	[Fact]
	public void A_chain_of_includes_stops_at_its_depth()
	{
		var script = SqlScript.Read(":r 0\n", new ScriptOptions
		{
			ResolveInclude = include => new ScriptSource(include.Path, $":r {int.Parse(include.Path) + 1}\nSELECT {include.Path}\n"),
		});

		Assert.Equal(ScriptSeverity.Error, Assert.Single(script.Diagnostics).Severity);
		Assert.Equal(32, Assert.Single(script.Batches).ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries).Length);
	}

	// ── Variables ────────────────────────────────────────────────────────────

	[Fact]
	public void Variables_given_are_overridden_by_the_script()
	{
		var script = SqlScript.Read("SELECT '$(a)', '$(B)'\n:setvar b 3\n", new ScriptOptions
		{
			Variables = new Dictionary<string, string> { ["A"] = "1", ["b"] = "2" },
		});

		Assert.Equal("SELECT '1', '3'\n", Assert.Single(script.Batches).ToString());
	}

	[Fact]
	public void An_undefined_variable_stays_as_written_and_the_batch_stays_a_window()
	{
		var text   = "SELECT '$(nope)'\n";
		var script = SqlScript.Read(text);
		var batch  = Assert.Single(script.Batches);

		Assert.Same(text, batch.Text);
		Assert.True(batch.IsVerbatim);
		Assert.Equal(ScriptSeverity.Warning, Assert.Single(script.Diagnostics).Severity);
	}

	[Fact]
	public void A_malformed_reference_ends_the_script()
	{
		var script = SqlScript.Read("SELECT 1\nGO\nSELECT '$(x'\nGO\nSELECT 3\n");

		Assert.Equal("SELECT 1\n", Assert.Single(script.Batches).ToString());

		var fatal = Assert.Single(script.Diagnostics);

		Assert.Equal(ScriptSeverity.Fatal, fatal.Severity);
		Assert.Equal(20, fatal.Location.Span.At);
	}

	[Fact]
	public void Without_substitution_a_reference_is_text()
	{
		var script = SqlScript.Read(":setvar x 1\nSELECT '$(x'\n", new ScriptOptions { SubstituteVariables = false });

		Assert.Equal("SELECT '$(x'\n", Assert.Single(script.Batches).ToString());
		Assert.Empty(script.Diagnostics);
	}

	// ── Profiles ─────────────────────────────────────────────────────────────

	[Fact]
	public void Without_SQLCMD_mode_commands_and_references_are_text()
	{
		var text   = ":setvar x 1\nSELECT '$(x)'\nGO 2\n:r a.sql\n";
		var script = SqlScript.Read(text, new ScriptOptions { Profile = ScriptProfile.Ssms() });

		Assert.Equal([":setvar x 1\nSELECT '$(x)'\n", ":r a.sql\n"], script.Batches.Select(static one => one.ToString()));
		Assert.Equal(2, script.Batches[0].Count);
		Assert.Empty(script.Directives);
		Assert.Empty(script.Diagnostics);
	}

	[Fact]
	public void The_separator_is_the_profiles_word()
	{
		var script = SqlScript.Read("SELECT 1\nGO\nSELECT 2\nrun 2\nSELECT 3\n", new ScriptOptions { Profile = ScriptProfile.SqlCmd("RUN") });

		Assert.Equal(["SELECT 1\nGO\nSELECT 2\n", "SELECT 3\n"], script.Batches.Select(static one => one.ToString()));
		Assert.Equal(2, script.Batches[0].Count);
		Assert.Equal("RUN", ScriptProfile.SqlCmd("RUN").Separator);
		Assert.Equal("ssms", ScriptProfile.Ssms().Name);
	}

	[Theory]
	[InlineData("")]
	[InlineData("G O")]
	[InlineData("GO;")]
	[InlineData("ГО")]
	public void A_separator_is_a_word(string separator)
	{
		Assert.Throws<ArgumentException>(() => ScriptProfile.SqlCmd(separator));
		Assert.Throws<ArgumentException>(() => ScriptProfile.Ssms(separator));
	}

	// ── What was the tool's ──────────────────────────────────────────────────

	/// <summary>
	/// Whether anything was the tool's rather than the server's: one batch ended by <c>GO</c>
	/// counts, which is why the question is not how many batches there are.
	/// </summary>
	[Theory]
	[InlineData("SELECT 1", false)]
	[InlineData("SELECT 1\nSELECT 2\n", false)]
	[InlineData("SELECT go FROM t\n-- GO\n/*\nGO\n*/", false)]
	[InlineData("SELECT '$x', $1", false)]
	[InlineData("SELECT 1\nGO\n", true)]
	[InlineData("SELECT 1\nGO 2", true)]
	[InlineData(":setvar x 1\nSELECT 1", true)]
	[InlineData("SELECT '$(x)'", true)]
	[InlineData("SELECT 1\n:on error exit\n", true)]
	public void Client_syntax_is_whatever_the_tool_reads(string text, bool client)
	{
		Assert.Equal(client, SqlScript.Read(text).HasClientSyntax);
	}

	[Fact]
	public void Commands_are_reported_in_order_and_not_run()
	{
		var script = SqlScript.Read("SELECT 1\n:on error exit\n:reset\nGO\nexit(SELECT 2)\nSELECT 3\n!! dir\n");

		Assert.Equal(["SELECT 1\n", "SELECT 3\n"], script.Batches.Select(static one => one.ToString()));
		Assert.Equal(
			[("on", "error exit", 0), ("reset", "", 0), ("exit", "(SELECT 2)", 1), ("!!", "dir", 1)],
			script.Directives.Select(static one => (one.Name, one.Arguments, one.Batch)));
	}

	[Fact]
	public void A_count_past_a_long_is_the_largest_one()
	{
		var script = SqlScript.Read("SELECT 1\nGO 99999999999999999999999999\n");

		Assert.Equal(long.MaxValue, Assert.Single(script.Batches).Count);
	}

	[Fact]
	public void A_batch_ended_by_GO_0_is_kept_with_no_sends()
	{
		var script = SqlScript.Read("SELECT 1\nGO 0\nSELECT 2\n");

		Assert.Equal([0L, 1L], script.Batches.Select(static one => one.Count));
	}

	[Fact]
	public void Reading_nothing_gives_nothing()
	{
		var script = SqlScript.Read("");

		Assert.Empty(script.Batches);
		Assert.False(script.HasClientSyntax);
		Assert.Throws<ArgumentNullException>(() => SqlScript.Read(null!));
	}
}
