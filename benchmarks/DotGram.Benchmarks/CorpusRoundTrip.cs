using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Sql;
using DotGram.Sql.TransactSql;

using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace DotGram.Benchmarks;

/// <summary>
/// The round trip over a corpus of somebody else's SQL, counted per file: what both parsers read,
/// what this grammar refused, what the writer could not print, and what came back the same statement.
/// </summary>
/// <remarks>
/// <para>
/// Written here and compiled into <c>DotGram.Tests.Slow</c> as well, the way
/// <c>RefusalLadders.cs</c> is: <c>--roundtrip</c> prints what it counts as an audit, and
/// <c>CorpusRoundTripTests</c> holds the same counts to a baseline. One implementation rather than
/// two, so nothing drifts — and what that costs is said where it matters: the gate catches a CHANGE
/// in these numbers, not a fault in the cutting or the comparing, which both roads would then share.
/// </para>
/// <para>
/// The comparison is not against the original text. Each statement is printed by ScriptDom twice —
/// once from ScriptDom's own reading and once from what this grammar read and <see cref="SqlWriter"/>
/// wrote — and the two printings are held against each other. So formatting is not what is being
/// measured; what the tree holds is.
/// </para>
/// </remarks>
internal static class CorpusRoundTrip
{
	/// <summary>What one file's statements came to.</summary>
	/// <param name="Read">Statements ScriptDom read and this grammar read as well.</param>
	/// <param name="Refused">Statements ScriptDom read and this grammar did not.</param>
	/// <param name="Threw">Read, and then the writer threw or printed something ScriptDom will not read.</param>
	/// <param name="Same">Read, printed, and printed back to the same statement.</param>
	internal sealed record Counts(int Read, int Refused, int Threw, int Same);

	/// <summary>What one kind of statement came to, over the whole corpus.</summary>
	internal sealed class Tally
	{
		/// <summary>Read by both.</summary>
		public int All;

		/// <summary>Read, printed, and printed back to the same statement.</summary>
		public int Same;

		/// <summary>Read, and then the writer threw or printed something ScriptDom will not read.</summary>
		public int Broken;
	}

	/// <summary>
	/// The counts, by file for a baseline and by kind for a report. The four totals a report wants are
	/// sums of the first: read, same and broken directly, and "read, printed and different" as what is
	/// left of read after the other two.
	/// </summary>
	internal sealed record Result(Dictionary<string, Counts> ByFile, Dictionary<string, Tally> ByKind);

	/// <summary>A statement that did not come back whole, for whoever wants to show it.</summary>
	/// <param name="Kind">ScriptDom's name for the kind of statement.</param>
	/// <param name="Original">The statement as the corpus writes it.</param>
	/// <param name="Why">Why it did not come back, where there is no pair of printings to compare.</param>
	/// <param name="Theirs">ScriptDom's printing of its own reading, where both printed.</param>
	/// <param name="Ours">ScriptDom's printing of what this grammar read, where both printed.</param>
	internal sealed record Trouble(string Kind, string Original, string? Why, string? Theirs, string? Ours);

	/// <summary>
	/// The version to read and print one file with, where a <c>Baselines&lt;n&gt;</c> directory names it.
	/// </summary>
	/// <remarks>
	/// Microsoft's own partition of the corpus by parser version, and it is not optional: at 180 alone,
	/// the files written for `*=`, `DUMP` and `DISABLE_DEF_CNST_CHK` are unreadable and drop out, so a
	/// reading that ignored this would cover less of the corpus and say nothing about the difference.
	/// A file outside the partition takes the version of the file of the same name inside it.
	/// </remarks>
	internal static Dictionary<string, string> Versions(string root, IEnumerable<string> files)
	{
		var all    = files as IReadOnlyCollection<string> ?? files.ToList();
		var byFile = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		var byName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		foreach (var file in all)
			if (Partition(root, file) is { } version)
			{
				byFile[file] = version;
				byName[Path.GetFileName(file)] = version;
			}

		foreach (var file in all)
			if (!byFile.ContainsKey(file) && byName.TryGetValue(Path.GetFileName(file), out var twin))
				byFile[file] = twin;

		return byFile;
	}

	/// <summary>The version a <c>Baselines&lt;n&gt;</c> directory names, or null for anything else.</summary>
	static string? Partition(string root, string file)
	{
		var inside = file[root.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		var first  = inside.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];

		return first.StartsWith("Baselines", StringComparison.Ordinal) &&
			first["Baselines".Length..] is { Length: > 0 } named &&
			named.All(char.IsAsciiDigit)
			? named
			: null;
	}

	/// <summary>
	/// Every <c>.sql</c> file under <paramref name="root"/>, counted. The key is the path relative to
	/// the root, with forward slashes, so a baseline written on one machine reads on another.
	/// </summary>
	/// <param name="root">The corpus directory.</param>
	/// <param name="ceiling">The newest version to read with; a file's own version caps it.</param>
	/// <param name="show">Told of each statement that did not come back whole.</param>
	internal static Result Run(
		string root, string ceiling = "180", Action<Trouble>? show = null)
	{
		var files      = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);
		var versions   = Versions(root, files);
		var readers    = new Dictionary<string, TSqlParser>(StringComparer.Ordinal);
		var generators = new Dictionary<string, SqlScriptGenerator>(StringComparer.Ordinal);
		var counted    = new Dictionary<string, Counts>(StringComparer.Ordinal);
		var kinds      = new Dictionary<string, Tally>(StringComparer.Ordinal);

		foreach (var file in files)
		{
			var read    = 0;
			var refused = 0;
			var threw   = 0;
			var same    = 0;

			var text  = File.ReadAllText(file);
			var named = Named(ceiling, versions.GetValueOrDefault(file));

			if (!readers.TryGetValue(named, out var parser))
				readers[named] = parser = Reader(named);

			if (!generators.TryGetValue(named, out var generator))
				generators[named] = generator = Generator(named);

			using var whole = new StringReader(text);

			// A file ScriptDom itself will not read is counted as a file with no statements: it is not
			// this grammar's failure, and `--kinds` is where what nobody reads is reported.
			if (parser.Parse(whole, out var errors) is TSqlScript script && errors.Count == 0)
				foreach (var statement in script.Batches.SelectMany(static batch => batch.Statements))
				{
					var original = text.Substring(statement.StartOffset, statement.FragmentLength).TrimEnd();
					var kind     = statement.GetType().Name;

					if (Ours(original) is not { } made)
					{
						refused++;

						continue;
					}

					read++;

					if (!kinds.TryGetValue(kind, out var tally))
						kinds[kind] = tally = new Tally();

					tally.All++;

					string printed;

					try
					{
						printed = SqlWriter.Write(made);
					}
					catch (Exception failure)
					{
						threw++;
						tally.Broken++;
						show?.Invoke(new Trouble(kind, original, "the writer threw: " + failure.GetType().Name, null, null));

						continue;
					}

					generator.GenerateScript(statement, out var theirs);

					using var again = new StringReader(printed);

					if (parser.Parse(again, out var rejected) is not TSqlScript back || rejected.Count > 0 ||
						back.Batches.SelectMany(static batch => batch.Statements).ToArray() is not [var only])
					{
						threw++;
						tally.Broken++;
						show?.Invoke(new Trouble(kind, original, "printed as: " + printed, null, null));

						continue;
					}

					generator.GenerateScript(only, out var ours);

					if (Normalized(theirs) == Normalized(ours))
					{
						same++;
						tally.Same++;
					}
					else
						show?.Invoke(new Trouble(kind, original, null, theirs, ours));
				}

			counted[Key(root, file)] = new Counts(read, refused, threw, same);
		}

		return new Result(counted, kinds);
	}

	/// <summary>A file's key in a baseline: relative to the root, with forward slashes.</summary>
	internal static string Key(string root, string file)
	{
		return file[root.Length..]
			.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
			.Replace(Path.DirectorySeparatorChar, '/');
	}

	/// <summary>What this grammar makes of one statement, or null where it refuses or throws.</summary>
	static Statement? Ours(string original)
	{
		try
		{
			var match = TransactSqlParser.TryParseStatement(original);

			return match.IsSuccess ? match.Value as Statement : null;
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>Whitespace is not what is compared.</summary>
	static string Normalized(string text)
	{
		return string.Join(' ', text.Split(' ', '\t', '\n', '\r').Where(static one => one.Length > 0));
	}

	/// <summary>A file's own version, capped at the one asked for.</summary>
	static string Named(string ceiling, string? version)
	{
		return version is not null && int.Parse(version) < int.Parse(ceiling) ? version : ceiling;
	}

	static TSqlParser Reader(string version)
	{
		return version switch
		{
			"80"  => new TSql80Parser (true),
			"90"  => new TSql90Parser (true),
			"100" => new TSql100Parser(true),
			"110" => new TSql110Parser(true),
			"120" => new TSql120Parser(true),
			"130" => new TSql130Parser(true),
			"140" => new TSql140Parser(true),
			"150" => new TSql150Parser(true),
			"160" => new TSql160Parser(true),
			"170" => new TSql170Parser(true),
			_     => new TSql180Parser(true),
		};
	}

	// The printer's options are the comparison's basis: both sides are printed with these, so a
	// difference in them is a difference in the tree and not in how it is laid out.
	static SqlScriptGenerator Generator(string version)
	{
		var options = new SqlScriptGeneratorOptions
		{
			KeywordCasing              = KeywordCasing.Uppercase,
			IncludeSemicolons          = true,
			AlignClauseBodies          = false,
			NewLineBeforeFromClause    = false,
			NewLineBeforeWhereClause   = false,
			NewLineBeforeGroupByClause = false,
			NewLineBeforeHavingClause  = false,
			NewLineBeforeOrderByClause = false,
			NewLineBeforeJoinClause    = false,
			NewLineBeforeOnClause      = false,
			NewLineBeforeOffsetClause  = false,
			NewLineBeforeOutputClause  = false,
		};

		return version switch
		{
			"80"  => new Sql80ScriptGenerator (options),
			"90"  => new Sql90ScriptGenerator (options),
			"100" => new Sql100ScriptGenerator(options),
			"110" => new Sql110ScriptGenerator(options),
			"120" => new Sql120ScriptGenerator(options),
			"130" => new Sql130ScriptGenerator(options),
			"140" => new Sql140ScriptGenerator(options),
			"150" => new Sql150ScriptGenerator(options),
			"160" => new Sql160ScriptGenerator(options),
			"170" => new Sql170ScriptGenerator(options),
			_     => new Sql180ScriptGenerator(options),
		};
	}
}
