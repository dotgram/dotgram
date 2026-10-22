using System;

namespace DotGram.Cli;

/// <summary>What the tool's process exits with.</summary>
static class ExitCode
{
	/// <summary>Generated. (1 is not used.)</summary>
	public const int Done = 0;

	/// <summary>The grammar has errors, or in <c>.gram</c> mode the generated parser does not compile.</summary>
	public const int GrammarErrors = 2;

	/// <summary>
	/// Usage, input and output, a project that cannot be evaluated, a type a grammar asks about that
	/// cannot be found, target frameworks that generate different parsers.
	/// </summary>
	public const int Failure = 3;
}

/// <summary>A failure the tool reports in one message and an exit code, with no stack trace.</summary>
sealed class ToolException(string message, int exitCode, bool usage = false) : Exception(message)
{
	public int ExitCode { get; } = exitCode;

	/// <summary>Whether the message is about how the tool was called, and <c>--help</c> is worth a mention.</summary>
	public bool IsUsage { get; } = usage;

	public static ToolException Usage(string message)
	{
		return new ToolException(message, global::DotGram.Cli.ExitCode.Failure, usage: true);
	}

	public static ToolException Failure(string message)
	{
		return new ToolException(message, global::DotGram.Cli.ExitCode.Failure);
	}
}
