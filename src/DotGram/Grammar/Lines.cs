using System;
using System.Text;

namespace DotGram.Grammar;

/// <summary>
/// Ends a line the same way on every machine.
/// </summary>
/// <remarks>
/// <para>
/// <c>StringBuilder.AppendLine</c> uses <see cref="Environment.NewLine"/>, so the same
/// grammar compiled on Windows and on Linux produced different bytes. For a compiler
/// component that is a defect on its own — generated output is an artifact, and an
/// artifact that depends on the machine cannot be diffed, snapshotted or reproduced.
/// </para>
/// <para>
/// CRLF rather than LF because that is what this repository writes everywhere else
/// (see CLAUDE.md) and what Visual Studio writes into the generated files it puts on
/// disk. Which one it is matters far less than that it is always the same one.
/// </para>
/// </remarks>
static class Lines
{
	public const string Ending = "\r\n";

	public static StringBuilder EndLine(this StringBuilder text) =>
		text.Append(Ending);

	public static StringBuilder AppendEndingWith(this StringBuilder text, string value) =>
		text.Append(value).Append(Ending);

	/// <summary>
	/// The same text with every line ending <see cref="Ending"/>.
	/// </summary>
	/// <remarks>
	/// For text this component did not write: a raw string literal carries whatever the
	/// file it was typed in was saved with, and everything downstream of it reads lines by
	/// splitting on <see cref="Ending"/>.
	/// </remarks>
	public static string Normalize(string text) =>
		text.IndexOf('\n') < 0 ? text : text.Replace("\r\n", "\n").Replace("\n", Ending);
}
