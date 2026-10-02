using System;

namespace DotGram.Sql.TransactSql;

// A script's batches read by this dialect: written by hand over the one-call readings, since what
// cuts a script is the client's (SqlScript) and not the language's. The same methods twice, for
// the plain reader and the located one, at every level the grammar publishes.
public abstract partial class TransactSqlParser
{
	/// <summary>
	/// Reads one batch of a script, as the server reads what one call sends it.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <remarks>
	/// <para>
	/// The whole batch is read or none of it: a reading that stops short of its end is refused where
	/// it stopped. Positions — the refusal's, and every span of a located reading — are offsets into
	/// <see cref="ScriptBatch.Text"/>, which for a batch written as one run with nothing substituted
	/// is the script itself; <see cref="ScriptBatch.Locate"/> takes any of them back to where it was
	/// written.
	/// </para>
	/// <para>
	/// A <c>GO</c> in a batch is a word like any other, as it is to the server.
	/// </para>
	/// </remarks>
	public static Match<Statement[]> TryParseSql(ScriptBatch batch)
	{
		return SqlScriptReading.Read(batch, TryParseSql, TryParseSql);
	}

	/// <summary>
	/// Reads one batch of a script, answering only whether it is read.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <param name="value">The statements, where it is read.</param>
	/// <remarks>
	/// As <see cref="TryParseSql(ScriptBatch)"/>; nothing is said about a refusal, which saves the
	/// cost of saying it.
	/// </remarks>
	public static bool TryParseSql(ScriptBatch batch, out Statement[] value)
	{
		return SqlScriptReading.Reads(batch, TryParseSql, TryParseSql, out value);
	}

	/// <summary>
	/// Reads one batch of a script.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <exception cref="FormatException">
	/// The batch is not read. <c>TryParseSql(ScriptBatch)</c> answers instead.
	/// </exception>
	public static Statement[] ParseSql(ScriptBatch batch)
	{
		return SqlScriptReading.Parsed(TryParseSql(batch));
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 100.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
	public static Match<Statement[]> TryParseSql100(ScriptBatch batch)
	{
		return SqlScriptReading.Read(batch, TryParseSql100, TryParseSql100);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 100, answering only whether it is read.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <param name="value">The statements, where it is read.</param>
	/// <remarks>
	/// As <see cref="TryParseSql100(ScriptBatch)"/>; nothing is said about a refusal, which saves the
	/// cost of saying it.
	/// </remarks>
	public static bool TryParseSql100(ScriptBatch batch, out Statement[] value)
	{
		return SqlScriptReading.Reads(batch, TryParseSql100, TryParseSql100, out value);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 100.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <exception cref="FormatException">
	/// The batch is not read. <c>TryParseSql100(ScriptBatch)</c> answers instead.
	/// </exception>
	public static Statement[] ParseSql100(ScriptBatch batch)
	{
		return SqlScriptReading.Parsed(TryParseSql100(batch));
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 110.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
	public static Match<Statement[]> TryParseSql110(ScriptBatch batch)
	{
		return SqlScriptReading.Read(batch, TryParseSql110, TryParseSql110);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 110, answering only whether it is read.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <param name="value">The statements, where it is read.</param>
	/// <remarks>
	/// As <see cref="TryParseSql110(ScriptBatch)"/>; nothing is said about a refusal, which saves the
	/// cost of saying it.
	/// </remarks>
	public static bool TryParseSql110(ScriptBatch batch, out Statement[] value)
	{
		return SqlScriptReading.Reads(batch, TryParseSql110, TryParseSql110, out value);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 110.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <exception cref="FormatException">
	/// The batch is not read. <c>TryParseSql110(ScriptBatch)</c> answers instead.
	/// </exception>
	public static Statement[] ParseSql110(ScriptBatch batch)
	{
		return SqlScriptReading.Parsed(TryParseSql110(batch));
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 120.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
	public static Match<Statement[]> TryParseSql120(ScriptBatch batch)
	{
		return SqlScriptReading.Read(batch, TryParseSql120, TryParseSql120);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 120, answering only whether it is read.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <param name="value">The statements, where it is read.</param>
	/// <remarks>
	/// As <see cref="TryParseSql120(ScriptBatch)"/>; nothing is said about a refusal, which saves the
	/// cost of saying it.
	/// </remarks>
	public static bool TryParseSql120(ScriptBatch batch, out Statement[] value)
	{
		return SqlScriptReading.Reads(batch, TryParseSql120, TryParseSql120, out value);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 120.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <exception cref="FormatException">
	/// The batch is not read. <c>TryParseSql120(ScriptBatch)</c> answers instead.
	/// </exception>
	public static Statement[] ParseSql120(ScriptBatch batch)
	{
		return SqlScriptReading.Parsed(TryParseSql120(batch));
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 130.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
	public static Match<Statement[]> TryParseSql130(ScriptBatch batch)
	{
		return SqlScriptReading.Read(batch, TryParseSql130, TryParseSql130);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 130, answering only whether it is read.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <param name="value">The statements, where it is read.</param>
	/// <remarks>
	/// As <see cref="TryParseSql130(ScriptBatch)"/>; nothing is said about a refusal, which saves the
	/// cost of saying it.
	/// </remarks>
	public static bool TryParseSql130(ScriptBatch batch, out Statement[] value)
	{
		return SqlScriptReading.Reads(batch, TryParseSql130, TryParseSql130, out value);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 130.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <exception cref="FormatException">
	/// The batch is not read. <c>TryParseSql130(ScriptBatch)</c> answers instead.
	/// </exception>
	public static Statement[] ParseSql130(ScriptBatch batch)
	{
		return SqlScriptReading.Parsed(TryParseSql130(batch));
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 140.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
	public static Match<Statement[]> TryParseSql140(ScriptBatch batch)
	{
		return SqlScriptReading.Read(batch, TryParseSql140, TryParseSql140);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 140, answering only whether it is read.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <param name="value">The statements, where it is read.</param>
	/// <remarks>
	/// As <see cref="TryParseSql140(ScriptBatch)"/>; nothing is said about a refusal, which saves the
	/// cost of saying it.
	/// </remarks>
	public static bool TryParseSql140(ScriptBatch batch, out Statement[] value)
	{
		return SqlScriptReading.Reads(batch, TryParseSql140, TryParseSql140, out value);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 140.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <exception cref="FormatException">
	/// The batch is not read. <c>TryParseSql140(ScriptBatch)</c> answers instead.
	/// </exception>
	public static Statement[] ParseSql140(ScriptBatch batch)
	{
		return SqlScriptReading.Parsed(TryParseSql140(batch));
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 150.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
	public static Match<Statement[]> TryParseSql150(ScriptBatch batch)
	{
		return SqlScriptReading.Read(batch, TryParseSql150, TryParseSql150);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 150, answering only whether it is read.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <param name="value">The statements, where it is read.</param>
	/// <remarks>
	/// As <see cref="TryParseSql150(ScriptBatch)"/>; nothing is said about a refusal, which saves the
	/// cost of saying it.
	/// </remarks>
	public static bool TryParseSql150(ScriptBatch batch, out Statement[] value)
	{
		return SqlScriptReading.Reads(batch, TryParseSql150, TryParseSql150, out value);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 150.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <exception cref="FormatException">
	/// The batch is not read. <c>TryParseSql150(ScriptBatch)</c> answers instead.
	/// </exception>
	public static Statement[] ParseSql150(ScriptBatch batch)
	{
		return SqlScriptReading.Parsed(TryParseSql150(batch));
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 160.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
	public static Match<Statement[]> TryParseSql160(ScriptBatch batch)
	{
		return SqlScriptReading.Read(batch, TryParseSql160, TryParseSql160);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 160, answering only whether it is read.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <param name="value">The statements, where it is read.</param>
	/// <remarks>
	/// As <see cref="TryParseSql160(ScriptBatch)"/>; nothing is said about a refusal, which saves the
	/// cost of saying it.
	/// </remarks>
	public static bool TryParseSql160(ScriptBatch batch, out Statement[] value)
	{
		return SqlScriptReading.Reads(batch, TryParseSql160, TryParseSql160, out value);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 160.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <exception cref="FormatException">
	/// The batch is not read. <c>TryParseSql160(ScriptBatch)</c> answers instead.
	/// </exception>
	public static Statement[] ParseSql160(ScriptBatch batch)
	{
		return SqlScriptReading.Parsed(TryParseSql160(batch));
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 170.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
	public static Match<Statement[]> TryParseSql170(ScriptBatch batch)
	{
		return SqlScriptReading.Read(batch, TryParseSql170, TryParseSql170);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 170, answering only whether it is read.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <param name="value">The statements, where it is read.</param>
	/// <remarks>
	/// As <see cref="TryParseSql170(ScriptBatch)"/>; nothing is said about a refusal, which saves the
	/// cost of saying it.
	/// </remarks>
	public static bool TryParseSql170(ScriptBatch batch, out Statement[] value)
	{
		return SqlScriptReading.Reads(batch, TryParseSql170, TryParseSql170, out value);
	}

	/// <summary>
	/// Reads one batch of a script at compatibility level 170.
	/// </summary>
	/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
	/// <exception cref="FormatException">
	/// The batch is not read. <c>TryParseSql170(ScriptBatch)</c> answers instead.
	/// </exception>
	public static Statement[] ParseSql170(ScriptBatch batch)
	{
		return SqlScriptReading.Parsed(TryParseSql170(batch));
	}

	/// <summary>
	/// Reads a script: cut into batches as sqlcmd cuts it, and each batch read as the one call it is.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <exception cref="FormatException">
	/// The script is not read. <c>TryParseScript</c> answers instead.
	/// </exception>
	/// <remarks>
	/// <para>
	/// <c>SqlScript.Read(input)</c> and <c>TryParseSql</c> of each batch, in one call: sqlcmd's reading,
	/// with the variables the script sets itself and no file to include, so a <c>:r</c> is refused.
	/// Every other command is the tool's and is passed over. For the batches one at a time, the commands
	/// and what sqlcmd would say, call those two instead.
	/// </para>
	/// <para>
	/// Positions are as <see cref="TryParseSql(ScriptBatch)"/> gives them: offsets into the script,
	/// except in a batch where a variable was substituted, whose own
	/// <see cref="ScriptBatch.Locate"/> (<see cref="Batch.Source"/>) maps them back. A refusal's
	/// position is always one in the script.
	/// </para>
	/// </remarks>
	public static Batch[] ParseScript(string input)
	{
		return SqlScriptReading.Parsed(TryParseScript(input));
	}

	/// <summary>
	/// Reads a script, answering rather than throwing.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <remarks>As <see cref="ParseScript(string)"/>; a refusal says where in the script and why.</remarks>
	public static Match<Batch[]> TryParseScript(string input)
	{
		return SqlScriptReading.Script(input, TryParseSql);
	}

	/// <summary>
	/// Reads a script, answering only whether it is read.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <param name="value">The batches, where it is read.</param>
	public static bool TryParseScript(string input, out Batch[] value)
	{
		return SqlScriptReading.Script(input, TryParseSql, TryParseSql, out value);
	}

	/// <summary>
	/// Reads a script, each batch at compatibility level 100.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <exception cref="FormatException">
	/// The script is not read. <c>TryParseScript100</c> answers instead.
	/// </exception>
	/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
	public static Batch[] ParseScript100(string input)
	{
		return SqlScriptReading.Parsed(TryParseScript100(input));
	}

	/// <summary>
	/// Reads a script at compatibility level 100, answering rather than throwing.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <remarks>As <see cref="ParseScript100(string)"/>; a refusal says where in the script and why.</remarks>
	public static Match<Batch[]> TryParseScript100(string input)
	{
		return SqlScriptReading.Script(input, TryParseSql100);
	}

	/// <summary>
	/// Reads a script at compatibility level 100, answering only whether it is read.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <param name="value">The batches, where it is read.</param>
	public static bool TryParseScript100(string input, out Batch[] value)
	{
		return SqlScriptReading.Script(input, TryParseSql100, TryParseSql100, out value);
	}

	/// <summary>
	/// Reads a script, each batch at compatibility level 110.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <exception cref="FormatException">
	/// The script is not read. <c>TryParseScript110</c> answers instead.
	/// </exception>
	/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
	public static Batch[] ParseScript110(string input)
	{
		return SqlScriptReading.Parsed(TryParseScript110(input));
	}

	/// <summary>
	/// Reads a script at compatibility level 110, answering rather than throwing.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <remarks>As <see cref="ParseScript110(string)"/>; a refusal says where in the script and why.</remarks>
	public static Match<Batch[]> TryParseScript110(string input)
	{
		return SqlScriptReading.Script(input, TryParseSql110);
	}

	/// <summary>
	/// Reads a script at compatibility level 110, answering only whether it is read.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <param name="value">The batches, where it is read.</param>
	public static bool TryParseScript110(string input, out Batch[] value)
	{
		return SqlScriptReading.Script(input, TryParseSql110, TryParseSql110, out value);
	}

	/// <summary>
	/// Reads a script, each batch at compatibility level 120.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <exception cref="FormatException">
	/// The script is not read. <c>TryParseScript120</c> answers instead.
	/// </exception>
	/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
	public static Batch[] ParseScript120(string input)
	{
		return SqlScriptReading.Parsed(TryParseScript120(input));
	}

	/// <summary>
	/// Reads a script at compatibility level 120, answering rather than throwing.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <remarks>As <see cref="ParseScript120(string)"/>; a refusal says where in the script and why.</remarks>
	public static Match<Batch[]> TryParseScript120(string input)
	{
		return SqlScriptReading.Script(input, TryParseSql120);
	}

	/// <summary>
	/// Reads a script at compatibility level 120, answering only whether it is read.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <param name="value">The batches, where it is read.</param>
	public static bool TryParseScript120(string input, out Batch[] value)
	{
		return SqlScriptReading.Script(input, TryParseSql120, TryParseSql120, out value);
	}

	/// <summary>
	/// Reads a script, each batch at compatibility level 130.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <exception cref="FormatException">
	/// The script is not read. <c>TryParseScript130</c> answers instead.
	/// </exception>
	/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
	public static Batch[] ParseScript130(string input)
	{
		return SqlScriptReading.Parsed(TryParseScript130(input));
	}

	/// <summary>
	/// Reads a script at compatibility level 130, answering rather than throwing.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <remarks>As <see cref="ParseScript130(string)"/>; a refusal says where in the script and why.</remarks>
	public static Match<Batch[]> TryParseScript130(string input)
	{
		return SqlScriptReading.Script(input, TryParseSql130);
	}

	/// <summary>
	/// Reads a script at compatibility level 130, answering only whether it is read.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <param name="value">The batches, where it is read.</param>
	public static bool TryParseScript130(string input, out Batch[] value)
	{
		return SqlScriptReading.Script(input, TryParseSql130, TryParseSql130, out value);
	}

	/// <summary>
	/// Reads a script, each batch at compatibility level 140.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <exception cref="FormatException">
	/// The script is not read. <c>TryParseScript140</c> answers instead.
	/// </exception>
	/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
	public static Batch[] ParseScript140(string input)
	{
		return SqlScriptReading.Parsed(TryParseScript140(input));
	}

	/// <summary>
	/// Reads a script at compatibility level 140, answering rather than throwing.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <remarks>As <see cref="ParseScript140(string)"/>; a refusal says where in the script and why.</remarks>
	public static Match<Batch[]> TryParseScript140(string input)
	{
		return SqlScriptReading.Script(input, TryParseSql140);
	}

	/// <summary>
	/// Reads a script at compatibility level 140, answering only whether it is read.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <param name="value">The batches, where it is read.</param>
	public static bool TryParseScript140(string input, out Batch[] value)
	{
		return SqlScriptReading.Script(input, TryParseSql140, TryParseSql140, out value);
	}

	/// <summary>
	/// Reads a script, each batch at compatibility level 150.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <exception cref="FormatException">
	/// The script is not read. <c>TryParseScript150</c> answers instead.
	/// </exception>
	/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
	public static Batch[] ParseScript150(string input)
	{
		return SqlScriptReading.Parsed(TryParseScript150(input));
	}

	/// <summary>
	/// Reads a script at compatibility level 150, answering rather than throwing.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <remarks>As <see cref="ParseScript150(string)"/>; a refusal says where in the script and why.</remarks>
	public static Match<Batch[]> TryParseScript150(string input)
	{
		return SqlScriptReading.Script(input, TryParseSql150);
	}

	/// <summary>
	/// Reads a script at compatibility level 150, answering only whether it is read.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <param name="value">The batches, where it is read.</param>
	public static bool TryParseScript150(string input, out Batch[] value)
	{
		return SqlScriptReading.Script(input, TryParseSql150, TryParseSql150, out value);
	}

	/// <summary>
	/// Reads a script, each batch at compatibility level 160.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <exception cref="FormatException">
	/// The script is not read. <c>TryParseScript160</c> answers instead.
	/// </exception>
	/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
	public static Batch[] ParseScript160(string input)
	{
		return SqlScriptReading.Parsed(TryParseScript160(input));
	}

	/// <summary>
	/// Reads a script at compatibility level 160, answering rather than throwing.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <remarks>As <see cref="ParseScript160(string)"/>; a refusal says where in the script and why.</remarks>
	public static Match<Batch[]> TryParseScript160(string input)
	{
		return SqlScriptReading.Script(input, TryParseSql160);
	}

	/// <summary>
	/// Reads a script at compatibility level 160, answering only whether it is read.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <param name="value">The batches, where it is read.</param>
	public static bool TryParseScript160(string input, out Batch[] value)
	{
		return SqlScriptReading.Script(input, TryParseSql160, TryParseSql160, out value);
	}

	/// <summary>
	/// Reads a script, each batch at compatibility level 170.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <exception cref="FormatException">
	/// The script is not read. <c>TryParseScript170</c> answers instead.
	/// </exception>
	/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
	public static Batch[] ParseScript170(string input)
	{
		return SqlScriptReading.Parsed(TryParseScript170(input));
	}

	/// <summary>
	/// Reads a script at compatibility level 170, answering rather than throwing.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <remarks>As <see cref="ParseScript170(string)"/>; a refusal says where in the script and why.</remarks>
	public static Match<Batch[]> TryParseScript170(string input)
	{
		return SqlScriptReading.Script(input, TryParseSql170);
	}

	/// <summary>
	/// Reads a script at compatibility level 170, answering only whether it is read.
	/// </summary>
	/// <param name="input">The script.</param>
	/// <param name="value">The batches, where it is read.</param>
	public static bool TryParseScript170(string input, out Batch[] value)
	{
		return SqlScriptReading.Script(input, TryParseSql170, TryParseSql170, out value);
	}

	/// <summary>
	/// What the script readings share: a batch read whole, and a script read batch by batch.
	/// </summary>
	static class SqlScriptReading
	{
		/// <summary>
		/// Reads a batch through a window of its text, and refuses a reading that stops before the end.
		/// </summary>
		/// <remarks>
		/// A window's reading is not required to reach the window's end, and a character no token
		/// begins with ends the tokens rather than the reading — so a reading that succeeds may have
		/// left something unread. What follows it is asked whether it is only spacing and comments; where
		/// it is anything else, or anything this cannot tell, the batch is read again as a text of its own,
		/// which reads to the end or says why not, and that refusal is the batch's.
		/// </remarks>
		public static Match<Statement[]> Read(
			ScriptBatch batch,
			Func<string, int, int, Match<Statement[]>> window,
			Func<string, Match<Statement[]>> whole)
		{
			if (batch is null)
				throw new ArgumentNullException(nameof(batch));

			var end = batch.At + batch.Length;

			// Spacing and comments and nothing else are a text of no statements, as the server reads
			// them; a window that holds no token at all is not asked, since it would look for one.
			if (Trivia(batch.Text, batch.At, end) == end)
				return Match<Statement[]>.Success([], batch.At, 0, end);

			var match = window(batch.Text, batch.At, batch.Length);

			if (!match.IsSuccess)
				return match;

			var over = match.Length > 0 ? (int)match.Position + match.Length : batch.At;

			if (Trivia(batch.Text, over, end) == end)
				return match;

			var alone = whole(batch.ToString());

			if (alone.IsSuccess)
				return match;

			return Match<Statement[]>.Failed(
				alone.Outcome, alone.Error ?? "Input does not match 'Sql'.", alone.Position + batch.At, null, null);
		}

		/// <summary>
		/// Reads a script: sqlcmd's batches, each read whole, and the first refusal in the text — a batch
		/// that does not read, or a line sqlcmd refuses or a file it cannot include — is the script's.
		/// </summary>
		public static Match<Batch[]> Script(string input, Func<ScriptBatch, Match<Statement[]>> read)
		{
			if (input is null)
				throw new ArgumentNullException(nameof(input));

			var script  = SqlScript.Read(input);
			var batches = new Batch[script.Batches.Count];
			var refusal = default(Match<Batch[]>?);

			foreach (var diagnostic in script.Diagnostics)
			{
				if (diagnostic.Severity == ScriptSeverity.Warning)
					continue;

				refusal = Match<Batch[]>.Failed(Outcome.NoMatch, diagnostic.Message, diagnostic.Location.Span.At, null, null);
				break;
			}

			for (var i = 0; i < batches.Length; i++)
			{
				var batch = script.Batches[i];

				if (refusal is { } before && before.Position < batch.Locate(batch.At, 0).Span.At)
					return before;

				var match = read(batch);

				if (!match.IsSuccess)
				{
					var at = batch.Locate((int)match.Position, 0).Span.At;

					if (refusal is { } earlier && earlier.Position <= at)
						return earlier;

					return Match<Batch[]>.Failed(match.Outcome, match.Error ?? "Input does not match 'Sql'.", at, null, null);
				}

				batches[i] = new Batch(SqlList.Own(match.Value), Go(batch)) { Source = batch };
			}

			if (refusal is { } last)
				return last;

			return Match<Batch[]>.Success(batches, 0, input.Length);
		}

		public delegate bool WindowReading(string input, ref int at, int length, out Statement[] value);

		public delegate bool WholeReading(string input, out Statement[] value);

		/// <summary>
		/// Reads a script as <see cref="Script(string, Func{ScriptBatch, Match{Statement[]}})"/> does,
		/// answering only whether it is read: nothing is spent on saying why not, which on a refusal
		/// is most of what a refusal costs — the readings' quiet forms are used throughout.
		/// </summary>
		public static bool Script(string input, WindowReading window, WholeReading whole, out Batch[] value)
		{
			if (input is null)
				throw new ArgumentNullException(nameof(input));

			var script = SqlScript.Read(input);

			value = null!;

			foreach (var diagnostic in script.Diagnostics)
			{
				if (diagnostic.Severity != ScriptSeverity.Warning)
					return false;
			}

			var batches = new Batch[script.Batches.Count];

			for (var i = 0; i < batches.Length; i++)
			{
				var batch = script.Batches[i];

				if (!Reads(batch, window, whole, out var statements))
					return false;

				batches[i] = new Batch(SqlList.Own(statements), Go(batch)) { Source = batch };
			}

			value = batches;
			return true;
		}

		/// <summary>
		/// Reads a batch as <see cref="Read"/> does, answering only whether it is read: the readings'
		/// quiet forms throughout.
		/// </summary>
		public static bool Reads(ScriptBatch batch, WindowReading window, WholeReading whole, out Statement[] value)
		{
			if (batch is null)
				throw new ArgumentNullException(nameof(batch));

			var at  = batch.At;
			var end = batch.At + batch.Length;

			if (Trivia(batch.Text, at, end) == end)
			{
				value = [];
				return true;
			}

			if (!window(batch.Text, ref at, batch.Length, out value))
				return false;

			// A reading of nothing leaves no end to go on from.
			if (at < batch.At)
				at = batch.At;

			if (Trivia(batch.Text, at, end) < end && !whole(batch.ToString(), out _))
			{
				value = null!;
				return false;
			}

			return true;
		}

		/// <summary>
		/// The value of a reading, or the refusal it was, thrown.
		/// </summary>
		public static T Parsed<T>(Match<T> match)
		{
			if (match.IsSuccess)
				return match.Value;

			throw new FormatException(match.Error + " at " + match.Position.ToString());
		}

		/// <summary>
		/// The line that ended a batch as <see cref="Batch.Go"/> says it: <c>GO</c>, or <c>GO 5</c>.
		/// </summary>
		static string? Go(ScriptBatch batch)
		{
			if (batch.Separator is null)
				return null;

			return batch.Count == 1 ? "GO" : "GO " + batch.Count.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Where spacing and comments after <paramref name="at"/> end, as T-SQL reads them; at the first
		/// thing that is neither, or that this does not know to be one.
		/// </summary>
		/// <remarks>
		/// The grammar's trivia, written again: its spacing, a line comment to a carriage return or a line
		/// feed, and a block comment holding others up to six deep. A deeper one is not known here, and
		/// the reading of the batch as a whole decides it.
		/// </remarks>
		static int Trivia(string text, int at, int end)
		{
			while (at < end)
			{
				var character = text[at];

				if (Spacing(character))
				{
					at++;
					continue;
				}

				if (character == '-' && at + 1 < end && text[at + 1] == '-')
				{
					at += 2;

					while (at < end && text[at] is not ('\r' or '\n'))
						at++;

					continue;
				}

				if (character == '/' && at + 1 < end && text[at + 1] == '*')
				{
					var depth = 1;
					var inner = at + 2;

					while (inner < end && depth > 0)
					{
						if (text[inner] == '*' && inner + 1 < end && text[inner + 1] == '/')
						{
							depth--;
							inner += 2;
						}
						else if (text[inner] == '/' && inner + 1 < end && text[inner + 1] == '*')
						{
							depth++;
							inner += 2;
						}
						else
						{
							inner++;
						}

						if (depth > 6)
							return at;
					}

					if (depth > 0)
						return at;

					at = inner;
					continue;
				}

				return at;
			}

			return at;
		}

		/// <summary>
		/// What T-SQL takes for a space: a control character but the null, U+0085, U+200B, and Unicode's
		/// spaces and separators.
		/// </summary>
		static bool Spacing(char character)
		{
			if (character is >= '\u0001' and <= '\u001F' or '\u0085' or '\u200B')
				return true;

			var category = char.GetUnicodeCategory(character);

			return category is System.Globalization.UnicodeCategory.SpaceSeparator
				or System.Globalization.UnicodeCategory.LineSeparator
				or System.Globalization.UnicodeCategory.ParagraphSeparator;
		}
	}

	public static partial class Located
	{
		/// <summary>
		/// Reads one batch of a script, as the server reads what one call sends it.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <remarks>
		/// <para>
		/// The whole batch is read or none of it: a reading that stops short of its end is refused where
		/// it stopped. Positions — the refusal's, and every span of a located reading — are offsets into
		/// <see cref="ScriptBatch.Text"/>, which for a batch written as one run with nothing substituted
		/// is the script itself; <see cref="ScriptBatch.Locate"/> takes any of them back to where it was
		/// written.
		/// </para>
		/// <para>
		/// A <c>GO</c> in a batch is a word like any other, as it is to the server.
		/// </para>
		/// </remarks>
		public static Match<Statement[]> TryParseSql(ScriptBatch batch)
		{
			return SqlScriptReading.Read(batch, TryParseSql, TryParseSql);
		}

		/// <summary>
		/// Reads one batch of a script, answering only whether it is read.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <param name="value">The statements, where it is read.</param>
		/// <remarks>
		/// As <see cref="TryParseSql(ScriptBatch)"/>; nothing is said about a refusal, which saves the
		/// cost of saying it.
		/// </remarks>
		public static bool TryParseSql(ScriptBatch batch, out Statement[] value)
		{
			return SqlScriptReading.Reads(batch, TryParseSql, TryParseSql, out value);
		}

		/// <summary>
		/// Reads one batch of a script.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <exception cref="FormatException">
		/// The batch is not read. <c>TryParseSql(ScriptBatch)</c> answers instead.
		/// </exception>
		public static Statement[] ParseSql(ScriptBatch batch)
		{
			return SqlScriptReading.Parsed(TryParseSql(batch));
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 100.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
		public static Match<Statement[]> TryParseSql100(ScriptBatch batch)
		{
			return SqlScriptReading.Read(batch, TryParseSql100, TryParseSql100);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 100, answering only whether it is read.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <param name="value">The statements, where it is read.</param>
		/// <remarks>
		/// As <see cref="TryParseSql100(ScriptBatch)"/>; nothing is said about a refusal, which saves the
		/// cost of saying it.
		/// </remarks>
		public static bool TryParseSql100(ScriptBatch batch, out Statement[] value)
		{
			return SqlScriptReading.Reads(batch, TryParseSql100, TryParseSql100, out value);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 100.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <exception cref="FormatException">
		/// The batch is not read. <c>TryParseSql100(ScriptBatch)</c> answers instead.
		/// </exception>
		public static Statement[] ParseSql100(ScriptBatch batch)
		{
			return SqlScriptReading.Parsed(TryParseSql100(batch));
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 110.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
		public static Match<Statement[]> TryParseSql110(ScriptBatch batch)
		{
			return SqlScriptReading.Read(batch, TryParseSql110, TryParseSql110);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 110, answering only whether it is read.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <param name="value">The statements, where it is read.</param>
		/// <remarks>
		/// As <see cref="TryParseSql110(ScriptBatch)"/>; nothing is said about a refusal, which saves the
		/// cost of saying it.
		/// </remarks>
		public static bool TryParseSql110(ScriptBatch batch, out Statement[] value)
		{
			return SqlScriptReading.Reads(batch, TryParseSql110, TryParseSql110, out value);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 110.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <exception cref="FormatException">
		/// The batch is not read. <c>TryParseSql110(ScriptBatch)</c> answers instead.
		/// </exception>
		public static Statement[] ParseSql110(ScriptBatch batch)
		{
			return SqlScriptReading.Parsed(TryParseSql110(batch));
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 120.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
		public static Match<Statement[]> TryParseSql120(ScriptBatch batch)
		{
			return SqlScriptReading.Read(batch, TryParseSql120, TryParseSql120);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 120, answering only whether it is read.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <param name="value">The statements, where it is read.</param>
		/// <remarks>
		/// As <see cref="TryParseSql120(ScriptBatch)"/>; nothing is said about a refusal, which saves the
		/// cost of saying it.
		/// </remarks>
		public static bool TryParseSql120(ScriptBatch batch, out Statement[] value)
		{
			return SqlScriptReading.Reads(batch, TryParseSql120, TryParseSql120, out value);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 120.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <exception cref="FormatException">
		/// The batch is not read. <c>TryParseSql120(ScriptBatch)</c> answers instead.
		/// </exception>
		public static Statement[] ParseSql120(ScriptBatch batch)
		{
			return SqlScriptReading.Parsed(TryParseSql120(batch));
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 130.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
		public static Match<Statement[]> TryParseSql130(ScriptBatch batch)
		{
			return SqlScriptReading.Read(batch, TryParseSql130, TryParseSql130);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 130, answering only whether it is read.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <param name="value">The statements, where it is read.</param>
		/// <remarks>
		/// As <see cref="TryParseSql130(ScriptBatch)"/>; nothing is said about a refusal, which saves the
		/// cost of saying it.
		/// </remarks>
		public static bool TryParseSql130(ScriptBatch batch, out Statement[] value)
		{
			return SqlScriptReading.Reads(batch, TryParseSql130, TryParseSql130, out value);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 130.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <exception cref="FormatException">
		/// The batch is not read. <c>TryParseSql130(ScriptBatch)</c> answers instead.
		/// </exception>
		public static Statement[] ParseSql130(ScriptBatch batch)
		{
			return SqlScriptReading.Parsed(TryParseSql130(batch));
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 140.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
		public static Match<Statement[]> TryParseSql140(ScriptBatch batch)
		{
			return SqlScriptReading.Read(batch, TryParseSql140, TryParseSql140);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 140, answering only whether it is read.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <param name="value">The statements, where it is read.</param>
		/// <remarks>
		/// As <see cref="TryParseSql140(ScriptBatch)"/>; nothing is said about a refusal, which saves the
		/// cost of saying it.
		/// </remarks>
		public static bool TryParseSql140(ScriptBatch batch, out Statement[] value)
		{
			return SqlScriptReading.Reads(batch, TryParseSql140, TryParseSql140, out value);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 140.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <exception cref="FormatException">
		/// The batch is not read. <c>TryParseSql140(ScriptBatch)</c> answers instead.
		/// </exception>
		public static Statement[] ParseSql140(ScriptBatch batch)
		{
			return SqlScriptReading.Parsed(TryParseSql140(batch));
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 150.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
		public static Match<Statement[]> TryParseSql150(ScriptBatch batch)
		{
			return SqlScriptReading.Read(batch, TryParseSql150, TryParseSql150);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 150, answering only whether it is read.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <param name="value">The statements, where it is read.</param>
		/// <remarks>
		/// As <see cref="TryParseSql150(ScriptBatch)"/>; nothing is said about a refusal, which saves the
		/// cost of saying it.
		/// </remarks>
		public static bool TryParseSql150(ScriptBatch batch, out Statement[] value)
		{
			return SqlScriptReading.Reads(batch, TryParseSql150, TryParseSql150, out value);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 150.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <exception cref="FormatException">
		/// The batch is not read. <c>TryParseSql150(ScriptBatch)</c> answers instead.
		/// </exception>
		public static Statement[] ParseSql150(ScriptBatch batch)
		{
			return SqlScriptReading.Parsed(TryParseSql150(batch));
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 160.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
		public static Match<Statement[]> TryParseSql160(ScriptBatch batch)
		{
			return SqlScriptReading.Read(batch, TryParseSql160, TryParseSql160);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 160, answering only whether it is read.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <param name="value">The statements, where it is read.</param>
		/// <remarks>
		/// As <see cref="TryParseSql160(ScriptBatch)"/>; nothing is said about a refusal, which saves the
		/// cost of saying it.
		/// </remarks>
		public static bool TryParseSql160(ScriptBatch batch, out Statement[] value)
		{
			return SqlScriptReading.Reads(batch, TryParseSql160, TryParseSql160, out value);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 160.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <exception cref="FormatException">
		/// The batch is not read. <c>TryParseSql160(ScriptBatch)</c> answers instead.
		/// </exception>
		public static Statement[] ParseSql160(ScriptBatch batch)
		{
			return SqlScriptReading.Parsed(TryParseSql160(batch));
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 170.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <remarks>As <see cref="TryParseSql(ScriptBatch)"/>, at one level.</remarks>
		public static Match<Statement[]> TryParseSql170(ScriptBatch batch)
		{
			return SqlScriptReading.Read(batch, TryParseSql170, TryParseSql170);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 170, answering only whether it is read.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <param name="value">The statements, where it is read.</param>
		/// <remarks>
		/// As <see cref="TryParseSql170(ScriptBatch)"/>; nothing is said about a refusal, which saves the
		/// cost of saying it.
		/// </remarks>
		public static bool TryParseSql170(ScriptBatch batch, out Statement[] value)
		{
			return SqlScriptReading.Reads(batch, TryParseSql170, TryParseSql170, out value);
		}

		/// <summary>
		/// Reads one batch of a script at compatibility level 170.
		/// </summary>
		/// <param name="batch">A batch of <see cref="SqlScript.Read"/>.</param>
		/// <exception cref="FormatException">
		/// The batch is not read. <c>TryParseSql170(ScriptBatch)</c> answers instead.
		/// </exception>
		public static Statement[] ParseSql170(ScriptBatch batch)
		{
			return SqlScriptReading.Parsed(TryParseSql170(batch));
		}

		/// <summary>
		/// Reads a script: cut into batches as sqlcmd cuts it, and each batch read as the one call it is.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <exception cref="FormatException">
		/// The script is not read. <c>TryParseScript</c> answers instead.
		/// </exception>
		/// <remarks>
		/// <para>
		/// <c>SqlScript.Read(input)</c> and <c>TryParseSql</c> of each batch, in one call: sqlcmd's reading,
		/// with the variables the script sets itself and no file to include, so a <c>:r</c> is refused.
		/// Every other command is the tool's and is passed over. For the batches one at a time, the commands
		/// and what sqlcmd would say, call those two instead.
		/// </para>
		/// <para>
		/// Positions are as <see cref="TryParseSql(ScriptBatch)"/> gives them: offsets into the script,
		/// except in a batch where a variable was substituted, whose own
		/// <see cref="ScriptBatch.Locate"/> (<see cref="Batch.Source"/>) maps them back. A refusal's
		/// position is always one in the script.
		/// </para>
		/// </remarks>
		public static Batch[] ParseScript(string input)
		{
			return SqlScriptReading.Parsed(TryParseScript(input));
		}

		/// <summary>
		/// Reads a script, answering rather than throwing.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <remarks>As <see cref="ParseScript(string)"/>; a refusal says where in the script and why.</remarks>
		public static Match<Batch[]> TryParseScript(string input)
		{
			return SqlScriptReading.Script(input, TryParseSql);
		}

		/// <summary>
		/// Reads a script, answering only whether it is read.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <param name="value">The batches, where it is read.</param>
		public static bool TryParseScript(string input, out Batch[] value)
		{
			return SqlScriptReading.Script(input, TryParseSql, TryParseSql, out value);
		}

		/// <summary>
		/// Reads a script, each batch at compatibility level 100.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <exception cref="FormatException">
		/// The script is not read. <c>TryParseScript100</c> answers instead.
		/// </exception>
		/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
		public static Batch[] ParseScript100(string input)
		{
			return SqlScriptReading.Parsed(TryParseScript100(input));
		}

		/// <summary>
		/// Reads a script at compatibility level 100, answering rather than throwing.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <remarks>As <see cref="ParseScript100(string)"/>; a refusal says where in the script and why.</remarks>
		public static Match<Batch[]> TryParseScript100(string input)
		{
			return SqlScriptReading.Script(input, TryParseSql100);
		}

		/// <summary>
		/// Reads a script at compatibility level 100, answering only whether it is read.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <param name="value">The batches, where it is read.</param>
		public static bool TryParseScript100(string input, out Batch[] value)
		{
			return SqlScriptReading.Script(input, TryParseSql100, TryParseSql100, out value);
		}

		/// <summary>
		/// Reads a script, each batch at compatibility level 110.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <exception cref="FormatException">
		/// The script is not read. <c>TryParseScript110</c> answers instead.
		/// </exception>
		/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
		public static Batch[] ParseScript110(string input)
		{
			return SqlScriptReading.Parsed(TryParseScript110(input));
		}

		/// <summary>
		/// Reads a script at compatibility level 110, answering rather than throwing.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <remarks>As <see cref="ParseScript110(string)"/>; a refusal says where in the script and why.</remarks>
		public static Match<Batch[]> TryParseScript110(string input)
		{
			return SqlScriptReading.Script(input, TryParseSql110);
		}

		/// <summary>
		/// Reads a script at compatibility level 110, answering only whether it is read.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <param name="value">The batches, where it is read.</param>
		public static bool TryParseScript110(string input, out Batch[] value)
		{
			return SqlScriptReading.Script(input, TryParseSql110, TryParseSql110, out value);
		}

		/// <summary>
		/// Reads a script, each batch at compatibility level 120.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <exception cref="FormatException">
		/// The script is not read. <c>TryParseScript120</c> answers instead.
		/// </exception>
		/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
		public static Batch[] ParseScript120(string input)
		{
			return SqlScriptReading.Parsed(TryParseScript120(input));
		}

		/// <summary>
		/// Reads a script at compatibility level 120, answering rather than throwing.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <remarks>As <see cref="ParseScript120(string)"/>; a refusal says where in the script and why.</remarks>
		public static Match<Batch[]> TryParseScript120(string input)
		{
			return SqlScriptReading.Script(input, TryParseSql120);
		}

		/// <summary>
		/// Reads a script at compatibility level 120, answering only whether it is read.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <param name="value">The batches, where it is read.</param>
		public static bool TryParseScript120(string input, out Batch[] value)
		{
			return SqlScriptReading.Script(input, TryParseSql120, TryParseSql120, out value);
		}

		/// <summary>
		/// Reads a script, each batch at compatibility level 130.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <exception cref="FormatException">
		/// The script is not read. <c>TryParseScript130</c> answers instead.
		/// </exception>
		/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
		public static Batch[] ParseScript130(string input)
		{
			return SqlScriptReading.Parsed(TryParseScript130(input));
		}

		/// <summary>
		/// Reads a script at compatibility level 130, answering rather than throwing.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <remarks>As <see cref="ParseScript130(string)"/>; a refusal says where in the script and why.</remarks>
		public static Match<Batch[]> TryParseScript130(string input)
		{
			return SqlScriptReading.Script(input, TryParseSql130);
		}

		/// <summary>
		/// Reads a script at compatibility level 130, answering only whether it is read.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <param name="value">The batches, where it is read.</param>
		public static bool TryParseScript130(string input, out Batch[] value)
		{
			return SqlScriptReading.Script(input, TryParseSql130, TryParseSql130, out value);
		}

		/// <summary>
		/// Reads a script, each batch at compatibility level 140.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <exception cref="FormatException">
		/// The script is not read. <c>TryParseScript140</c> answers instead.
		/// </exception>
		/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
		public static Batch[] ParseScript140(string input)
		{
			return SqlScriptReading.Parsed(TryParseScript140(input));
		}

		/// <summary>
		/// Reads a script at compatibility level 140, answering rather than throwing.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <remarks>As <see cref="ParseScript140(string)"/>; a refusal says where in the script and why.</remarks>
		public static Match<Batch[]> TryParseScript140(string input)
		{
			return SqlScriptReading.Script(input, TryParseSql140);
		}

		/// <summary>
		/// Reads a script at compatibility level 140, answering only whether it is read.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <param name="value">The batches, where it is read.</param>
		public static bool TryParseScript140(string input, out Batch[] value)
		{
			return SqlScriptReading.Script(input, TryParseSql140, TryParseSql140, out value);
		}

		/// <summary>
		/// Reads a script, each batch at compatibility level 150.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <exception cref="FormatException">
		/// The script is not read. <c>TryParseScript150</c> answers instead.
		/// </exception>
		/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
		public static Batch[] ParseScript150(string input)
		{
			return SqlScriptReading.Parsed(TryParseScript150(input));
		}

		/// <summary>
		/// Reads a script at compatibility level 150, answering rather than throwing.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <remarks>As <see cref="ParseScript150(string)"/>; a refusal says where in the script and why.</remarks>
		public static Match<Batch[]> TryParseScript150(string input)
		{
			return SqlScriptReading.Script(input, TryParseSql150);
		}

		/// <summary>
		/// Reads a script at compatibility level 150, answering only whether it is read.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <param name="value">The batches, where it is read.</param>
		public static bool TryParseScript150(string input, out Batch[] value)
		{
			return SqlScriptReading.Script(input, TryParseSql150, TryParseSql150, out value);
		}

		/// <summary>
		/// Reads a script, each batch at compatibility level 160.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <exception cref="FormatException">
		/// The script is not read. <c>TryParseScript160</c> answers instead.
		/// </exception>
		/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
		public static Batch[] ParseScript160(string input)
		{
			return SqlScriptReading.Parsed(TryParseScript160(input));
		}

		/// <summary>
		/// Reads a script at compatibility level 160, answering rather than throwing.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <remarks>As <see cref="ParseScript160(string)"/>; a refusal says where in the script and why.</remarks>
		public static Match<Batch[]> TryParseScript160(string input)
		{
			return SqlScriptReading.Script(input, TryParseSql160);
		}

		/// <summary>
		/// Reads a script at compatibility level 160, answering only whether it is read.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <param name="value">The batches, where it is read.</param>
		public static bool TryParseScript160(string input, out Batch[] value)
		{
			return SqlScriptReading.Script(input, TryParseSql160, TryParseSql160, out value);
		}

		/// <summary>
		/// Reads a script, each batch at compatibility level 170.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <exception cref="FormatException">
		/// The script is not read. <c>TryParseScript170</c> answers instead.
		/// </exception>
		/// <remarks>As <see cref="ParseScript(string)"/>, at one level.</remarks>
		public static Batch[] ParseScript170(string input)
		{
			return SqlScriptReading.Parsed(TryParseScript170(input));
		}

		/// <summary>
		/// Reads a script at compatibility level 170, answering rather than throwing.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <remarks>As <see cref="ParseScript170(string)"/>; a refusal says where in the script and why.</remarks>
		public static Match<Batch[]> TryParseScript170(string input)
		{
			return SqlScriptReading.Script(input, TryParseSql170);
		}

		/// <summary>
		/// Reads a script at compatibility level 170, answering only whether it is read.
		/// </summary>
		/// <param name="input">The script.</param>
		/// <param name="value">The batches, where it is read.</param>
		public static bool TryParseScript170(string input, out Batch[] value)
		{
			return SqlScriptReading.Script(input, TryParseSql170, TryParseSql170, out value);
		}
	}
}
