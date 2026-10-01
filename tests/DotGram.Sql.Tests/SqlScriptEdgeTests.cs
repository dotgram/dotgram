using System;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// <see cref="SqlScript"/> where sqlcmd 18.6 was asked and the oracle rows cannot say it: the
/// separator sqlcmd is started with, and what the reader reports about a script rather than what it
/// sends.
/// </summary>
public sealed class SqlScriptEdgeTests
{
	/// <summary>
	/// A reference sqlcmd cannot read ends the script, so the text is the tool's: reading it in one
	/// call is not what the tool would have done.
	/// </summary>
	[Theory]
	[InlineData("SELECT '$('")]
	[InlineData("SELECT '$(x")]
	[InlineData("SELECT $( x )")]
	public void A_reference_sqlcmd_refuses_is_client_syntax(string text)
	{
		var script = SqlScript.Read(text);

		Assert.Contains(script.Diagnostics, static one => one.Severity == ScriptSeverity.Fatal);
		Assert.True(script.HasClientSyntax);
	}

	/// <summary>
	/// sqlcmd refuses <c>-c exit</c> before it reads anything: "Invalid batch terminator 'exit' -
	/// reserved keyword (Option '-c')".
	/// </summary>
	[Fact]
	public void A_separator_sqlcmd_reserves_is_refused()
	{
		Assert.Throws<ArgumentException>(() => ScriptProfile.SqlCmd("exit"));
	}

	/// <summary>
	/// With <c>-c reset</c> or <c>-c quit</c>, sqlcmd still reads a line of that word as the command:
	/// <c>reset 2</c> is refused as <c>:reset</c> with an argument, and <c>quit</c> ends the reading
	/// before anything is sent.
	/// </summary>
	[Theory]
	[InlineData("reset")]
	[InlineData("quit")]
	public void A_command_word_given_as_the_separator_is_still_the_command(string word)
	{
		var script = SqlScript.Read($"PRINT 1\n{word}\nPRINT 2\n{word} 2\nPRINT 3\n", new ScriptOptions { Profile = ScriptProfile.SqlCmd(word) });

		Assert.Contains(script.Directives, one => one.Name == word);
		Assert.Contains(script.Diagnostics, static one => one.Severity == ScriptSeverity.Fatal);
		Assert.Empty(script.Batches);
	}
}
