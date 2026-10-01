using System;
using System.Collections.Generic;

namespace DotGram.Sql;

// A client's script, as the client cuts it before anything reaches a server. Dialect-neutral: the
// units come out as text, and a dialect's parser reads each one as the one call it is
// (TransactSqlParser.TryParseSql(ScriptBatch)). The scanner that does the cutting is
// SqlScriptReader.cs.

/// <summary>
/// A script read the way a client tool reads it: the batches it would send, the commands it
/// would act on, and what it would have said about them.
/// </summary>
/// <remarks>
/// <para>
/// <b>Text from a file or an editor is a script; text a program sends in one command is not.</b>
/// A <c>GO</c> line, a <c>:setvar</c> or a <c>$(name)</c> belongs to the tool — sqlcmd, SQL Server
/// Management Studio — and the server never sees it. Handed to the server in one call, the same
/// text means something else: <c>SELECT 1</c> and a line <c>GO</c> is a column called <c>GO</c>.
/// So a script goes through here, and each <see cref="ScriptBatch"/> is then read on its own.
/// </para>
/// <para>
/// Nothing here is executed and nothing is read from disk. A command is reported as a
/// <see cref="ScriptDirective"/>; only the two that change the text a tool sends act: <c>:setvar</c>
/// sets a variable and <c>:r</c> asks <see cref="ScriptOptions.ResolveInclude"/> for the file.
/// </para>
/// <para>
/// The reading is held to ODBC sqlcmd 18.6 for Linux, byte for byte, on what it sends; the other
/// tools that read the same scripts (go-sqlcmd, Invoke-Sqlcmd, Azure Data Studio) are known to
/// differ from it in corners, such as <c>GO 0</c>.
/// </para>
/// </remarks>
public sealed class SqlScript
{
	SqlScript(
		IReadOnlyList<ScriptBatch> batches, IReadOnlyList<ScriptDirective> directives,
		IReadOnlyList<ScriptDiagnostic> diagnostics, bool hasClientSyntax)
	{
		Batches         = batches;
		Directives      = directives;
		Diagnostics     = diagnostics;
		HasClientSyntax = hasClientSyntax;
	}

	/// <summary>
	/// The batches, in the order the tool would send them.
	/// </summary>
	/// <remarks>
	/// A batch with nothing in it is not sent and is not here; one holding only spacing or a
	/// comment is sent, and is. A batch ended by <c>GO 0</c> is here with a
	/// <see cref="ScriptBatch.Count"/> of 0, though no tool sends it.
	/// </remarks>
	public IReadOnlyList<ScriptBatch> Batches { get; }

	/// <summary>
	/// The commands written in the script, in reading order — <c>:setvar</c>, <c>:r</c>,
	/// <c>:on error</c>, <c>exit</c> and the rest — each as written and none of them run.
	/// </summary>
	public IReadOnlyList<ScriptDirective> Directives { get; }

	/// <summary>
	/// What the tool would have said while reading: an undefined variable, a file it could not
	/// include, a line it cannot read.
	/// </summary>
	public IReadOnlyList<ScriptDiagnostic> Diagnostics { get; }

	/// <summary>
	/// Whether anything in the text was the tool's rather than the server's: a separator line, a
	/// command, or a variable reference.
	/// </summary>
	/// <remarks>
	/// The question to ask of text whose origin is not known. Where this is false, the one batch
	/// is the text and reading it in one call answers the same. Where it is true, a reading of the
	/// whole text in one call — <c>ParseSql</c> — reads something the tool would never have sent:
	/// a script of one batch ended by <c>GO</c> counts, which is why this is not
	/// <c>Batches.Count &gt; 1</c>.
	/// </remarks>
	public bool HasClientSyntax { get; }

	/// <summary>
	/// Reads a script.
	/// </summary>
	/// <param name="text">The script.</param>
	/// <param name="options">The tool and what it is given; sqlcmd with nothing else when null.</param>
	/// <remarks>
	/// Never throws for what the text says: a line the tool would refuse ends the reading with a
	/// <see cref="ScriptSeverity.Fatal"/> diagnostic, and the batches before it are kept, as the tool
	/// had sent them by then.
	/// </remarks>
	public static SqlScript Read(string text, ScriptOptions? options = null)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		var reader = new SqlScriptReader(options ?? ScriptOptions.Default);

		reader.Read(text);

		return new SqlScript(reader.Batches, reader.Directives, reader.Diagnostics, reader.SawClientSyntax);
	}
}

/// <summary>
/// The client tool whose reading of a script is wanted.
/// </summary>
/// <remarks>
/// A closed set, made by the factories here: what a tool does is written into the reader, and a
/// new tool is a new factory.
/// </remarks>
public sealed class ScriptProfile
{
	ScriptProfile(string name, string separator, bool commands)
	{
		Name      = name;
		Separator = separator;
		Commands  = commands;
	}

	/// <summary>
	/// The tool's name: <c>sqlcmd</c> or <c>ssms</c>.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// The word that, alone on a line, ends a batch: <c>GO</c> unless the tool was told another.
	/// </summary>
	public string Separator { get; }

	/// <summary>
	/// Whether the tool reads commands (<c>:setvar</c>, <c>:r</c> …) and substitutes variables.
	/// </summary>
	internal bool Commands { get; }

	/// <summary>
	/// sqlcmd, and SQL Server Management Studio or Azure Data Studio in SQLCMD mode: separator
	/// lines, <c>GO n</c>, the commands and <c>$(name)</c>.
	/// </summary>
	/// <param name="separator">
	/// The separator word, as sqlcmd's <c>-c</c> sets it. A command's own word — <c>reset</c>,
	/// <c>quit</c> — is taken, and a line of it is still read as the command, as sqlcmd reads it.
	/// </param>
	/// <exception cref="ArgumentException">The separator is not a word, or is <c>exit</c>.</exception>
	public static ScriptProfile SqlCmd(string separator = "GO")
	{
		// sqlcmd refuses `-c exit`: "Invalid batch terminator 'exit' - reserved keyword".
		if (string.Equals(separator, "exit", StringComparison.OrdinalIgnoreCase))
			throw new ArgumentException("sqlcmd reserves 'exit', and refuses it as a batch separator.", nameof(separator));

		return new ScriptProfile("sqlcmd", Checked(separator), commands: true);
	}

	/// <summary>
	/// SQL Server Management Studio or Azure Data Studio as they start: separator lines and
	/// <c>GO n</c> and nothing else, so a <c>:setvar</c> line and a <c>$(name)</c> are text.
	/// </summary>
	/// <param name="separator">The separator word, as the batch separator option sets it.</param>
	/// <exception cref="ArgumentException">The separator is not a word.</exception>
	/// <remarks>
	/// Read with sqlcmd's lexis — its strings, comments and line starts — which is an assumption:
	/// the editors have not been asked line by line the way sqlcmd has.
	/// </remarks>
	public static ScriptProfile Ssms(string separator = "GO")
	{
		return new ScriptProfile("ssms", Checked(separator), commands: false);
	}

	static string Checked(string separator)
	{
		if (separator is null)
			throw new ArgumentNullException(nameof(separator));

		if (separator.Length == 0)
			throw new ArgumentException("A batch separator is a word, and this one is empty.", nameof(separator));

		foreach (var character in separator)
		{
			if (!(character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '_'))
				throw new ArgumentException(
					$"A batch separator is a word of letters, digits and '_', and '{separator}' is not one.",
					nameof(separator));
		}

		return separator;
	}
}

/// <summary>
/// How a script is read: the tool, and what that tool would be given besides the text.
/// </summary>
public sealed class ScriptOptions
{
	internal static readonly ScriptOptions Default = new();

	/// <summary>
	/// The tool; <see cref="ScriptProfile.SqlCmd"/> unless said.
	/// </summary>
	public ScriptProfile Profile { get; init; } = ScriptProfile.SqlCmd();

	/// <summary>
	/// The variables the tool starts with, as sqlcmd's <c>-v</c> gives them. Names compare without
	/// case, and a <c>:setvar</c> in the script overrides one.
	/// </summary>
	public IReadOnlyDictionary<string, string>? Variables { get; init; }

	/// <summary>
	/// Whether <c>$(name)</c> is replaced; false is sqlcmd's <c>-x</c>, and leaves every reference
	/// as written.
	/// </summary>
	public bool SubstituteVariables { get; init; } = true;

	/// <summary>
	/// The text of a file a <c>:r</c> names, or null where there is none. Without this, every
	/// <c>:r</c> is a file that cannot be read.
	/// </summary>
	/// <remarks>
	/// The reader does no I/O. Where a relative path is looked for — the working directory, as
	/// sqlcmd does, or the including file's — is the callback's to decide, and the
	/// <see cref="ScriptInclude.Location"/> it is given says which file asked.
	/// </remarks>
	public Func<ScriptInclude, ScriptSource?>? ResolveInclude { get; init; }

	/// <summary>
	/// What the script is called, carried by every <see cref="ScriptLocation"/> in it.
	/// </summary>
	/// <remarks>
	/// Not a name a <c>:r</c> is checked against for a cycle: sqlcmd reads the script once more when
	/// it includes itself, and refuses only the include after that.
	/// </remarks>
	public string? SourceName { get; init; }

	/// <summary>
	/// How many files <c>:r</c> may read in all before the reading stops, with a
	/// <see cref="ScriptSeverity.Fatal"/> diagnostic; 10,000 unless said.
	/// </summary>
	/// <remarks>
	/// sqlcmd sets no such limit. This is a guard for a script that is not trusted: a cycle is caught
	/// by name and a chain by its depth (32), but a file that includes two others that each include
	/// two more is read 2^n times, and twenty levels of that is a million files. The default is far
	/// beyond any deployment script; <see cref="int.MaxValue"/> lifts it.
	/// </remarks>
	public int MaximumIncludes { get; init; } = 10_000;
}

/// <summary>
/// One batch: the text the tool sends the server in one call.
/// </summary>
/// <remarks>
/// <para>
/// The batch is the window <c>[At, At + Length)</c> of <see cref="Text"/>. Where it was written as
/// one run of one text with nothing substituted (<see cref="IsVerbatim"/>), <see cref="Text"/> is
/// that text itself — for a batch of the script, the very string given to
/// <see cref="SqlScript.Read"/> — so a position a parser reports in it is already a position in
/// the script. Otherwise it is the text the tool would assemble, <see cref="At"/> is 0, and
/// <see cref="Locate"/> takes a position back to where it was written.
/// </para>
/// <para>
/// A command line inside a batch is cut out of what is sent, so it too makes a batch of two runs.
/// </para>
/// </remarks>
public sealed class ScriptBatch
{
	readonly ScriptPiece[] _pieces;

	internal ScriptBatch(
		string text, int at, int length, long count, bool verbatim, ScriptLocation? separator,
		ScriptPiece[] pieces)
	{
		Text       = text;
		At         = at;
		Length     = length;
		Count      = count;
		IsVerbatim = verbatim;
		Separator  = separator;
		_pieces    = pieces;
	}

	/// <summary>
	/// The text the batch is a window of.
	/// </summary>
	public string Text { get; }

	/// <summary>
	/// Where the batch begins in <see cref="Text"/>.
	/// </summary>
	public int At { get; }

	/// <summary>
	/// How long the batch is.
	/// </summary>
	public int Length { get; }

	/// <summary>
	/// How many times the tool sends it: <c>GO 5</c>'s 5, 1 where no count was written, and 0 for
	/// <c>GO 0</c>, which is never sent. A count too large for a <see cref="long"/> is
	/// <see cref="long.MaxValue"/>.
	/// </summary>
	public long Count { get; }

	/// <summary>
	/// Whether the batch is one run of the text it was written in, with nothing substituted, so that
	/// <see cref="Locate"/> changes nothing.
	/// </summary>
	public bool IsVerbatim { get; }

	/// <summary>
	/// The separator line that ended the batch, or null where the end of the script did.
	/// </summary>
	public ScriptLocation? Separator { get; }

	/// <summary>
	/// Where a range of <see cref="Text"/> was written.
	/// </summary>
	/// <param name="at">A position in <see cref="Text"/>, as a parser reports it.</param>
	/// <param name="length">How long the range is.</param>
	/// <remarks>
	/// <para>
	/// A position inside a substituted value stands for the whole reference: its start for the
	/// start of a range and its end for the end, so a range never claims part of a
	/// <c>$(name)</c>.
	/// </para>
	/// <para>
	/// A range is one range in one text, so one that runs from one file into another — across the
	/// edge of an included file — ends where its first file's part of the batch ends.
	/// </para>
	/// <para>
	/// The line break sqlcmd adds where a file ends inside a string, a quoted name or a comment
	/// without one was written nowhere: it maps to the empty range at the end of that file.
	/// </para>
	/// </remarks>
	public ScriptLocation Locate(int at, int length)
	{
		if (length < 0)
			throw new ArgumentOutOfRangeException(nameof(length));

		var first = PieceAt(at);
		var piece = _pieces[first];
		var start = piece.Start(at);
		var last  = first;
		var end   = at + length;

		while (last + 1 < _pieces.Length && _pieces[last + 1].TextAt < end && _pieces[last + 1].Source == piece.Source)
			last++;

		var stop = _pieces[last].End(Math.Min(end, _pieces[last].TextEnd));

		if (stop < start)
			stop = start;

		return new ScriptLocation(piece.Source.Name, new SqlSpan(start, stop - start));
	}

	/// <summary>
	/// The batch's text.
	/// </summary>
	public override string ToString()
	{
		return Text.Substring(At, Length);
	}

	int PieceAt(int at)
	{
		var low  = 0;
		var high = _pieces.Length - 1;

		while (low < high)
		{
			var middle = (low + high + 1) / 2;

			if (_pieces[middle].TextAt <= at)
				low = middle;
			else
				high = middle - 1;
		}

		return low;
	}
}

/// <summary>
/// Where something was written: the name of the text it is in, and the range in that text.
/// </summary>
/// <param name="Source">
/// The text's name: <see cref="ScriptOptions.SourceName"/> for the script, <see cref="ScriptSource.Name"/>
/// for an included file.
/// </param>
/// <param name="Span">The range, in characters of that text.</param>
public readonly record struct ScriptLocation(string? Source, SqlSpan Span);

/// <summary>
/// The text of an included file, as <see cref="ScriptOptions.ResolveInclude"/> hands it back.
/// </summary>
/// <param name="Name">
/// What to call it: every location in it carries this, and a file that includes a file of the same
/// name that is still being read is a cycle.
/// </param>
/// <param name="Text">The file's text.</param>
public sealed record ScriptSource(string Name, string Text);

/// <summary>
/// A file a <c>:r</c> asks for.
/// </summary>
/// <param name="Path">The path as written, with its variables substituted and its quotes taken off.</param>
/// <param name="Location">Where the <c>:r</c> line was written.</param>
public sealed record ScriptInclude(string Path, ScriptLocation Location);

/// <summary>
/// A command of the tool, as written in the script.
/// </summary>
/// <param name="Name">
/// The command's word in lower case and without its colon: <c>setvar</c>, <c>r</c>, <c>on</c>,
/// <c>exit</c>, <c>quit</c>, <c>reset</c>, <c>connect</c> …, or <c>!!</c>.
/// </param>
/// <param name="Arguments">The rest of its line, as written, without the spacing around it.</param>
/// <param name="Location">The line, without its line break.</param>
/// <param name="Batch">The index in <see cref="SqlScript.Batches"/> of the first batch after it.</param>
/// <remarks>
/// None of them is run. Two of them would change what the tool sends, and do not here: <c>:reset</c>
/// would discard the batch so far, and <c>exit</c> and <c>quit</c> would end the reading, the
/// pending batch unsent; here the batch goes on and so does the reading.
/// </remarks>
public sealed record ScriptDirective(string Name, string Arguments, ScriptLocation Location, int Batch);

/// <summary>
/// Something the tool would have said while reading.
/// </summary>
/// <param name="Severity">What the tool does next.</param>
/// <param name="Message">What it says, in its own words where it has any.</param>
/// <param name="Location">What it is about.</param>
public sealed record ScriptDiagnostic(ScriptSeverity Severity, string Message, ScriptLocation Location);

/// <summary>
/// What a tool does after it says something.
/// </summary>
public enum ScriptSeverity
{
	/// <summary>
	/// Goes on as if nothing were wrong, as with a variable that has no value.
	/// </summary>
	Warning,

	/// <summary>
	/// Goes on without what it could not do, as with a file it cannot include.
	/// </summary>
	Error,

	/// <summary>
	/// Stops: nothing after this is read, and the batch it was in is not sent.
	/// </summary>
	Fatal,
}
