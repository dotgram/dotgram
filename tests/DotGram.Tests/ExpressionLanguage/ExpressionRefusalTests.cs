using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

using DotGram.ExpressionLanguage;
using DotGram.Handwritten;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// What the expression language says about text it refuses, and the state a refusal leaves
/// behind, held against what it said before — on the tape and on the immediate carrier.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RefusalTests"/> for the language whose reading writes into a context: a refusal
/// may be read twice, quietly and then recording, and between the two the context is put back
/// with its <c>Mark()</c> and <c>Rollback()</c> (§7.7, Q7.2). So what is kept here is the
/// refusal field by field — outcome, position, the whole message — and the state after it: the
/// checkpoint <c>Mark()</c> takes, which counts the scopes, declarations, bodies still waiting
/// for types and namespaces the reading left, and holds the name it refused. A second reading
/// that began anywhere but where the first began would show in one of them.
/// </para>
/// <para>
/// The texts are the shared corpus and, from each, every fifth prefix and the text with every
/// seventh character left out: refusals in the middle of a lambda, of a body read before its
/// types are known, of a block, and not only at the end.
/// </para>
/// </remarks>
public sealed class ExpressionRefusalTests
{
	[Fact]
	public void Every_refusal_and_what_it_leaves_is_the_one_it_was()
	{
		var answers = new StringBuilder();

		foreach (var text in Texts())
		{
			var tape      = Answer(text, ExpressionParser.TryParseLambda);
			var immediate = Answer(text, ExpressionParser.Immediate.TryParseLambda);

			answers
				.Append(Escaped(text))
				.Append(" | ").Append(tape)
				.Append(" | ").Append(immediate == tape ? "same" : immediate)
				.Append('\n');
		}

		var actual = answers.ToString();

		if (!File.Exists(Expected))
		{
			File.WriteAllText(Expected, actual, Utf8);

			Assert.Fail($"No record of the refusals; wrote one to {Expected}. Read it, and commit it if it is right.");
		}

		if (File.ReadAllText(Expected).Replace("\r\n", "\n") == actual)
			return;

		File.WriteAllText(Expected + ".actual", actual, Utf8);

		Assert.Fail($"A refusal is not the one it was; what it is now is in {Expected}.actual.");
	}

	static IEnumerable<string> Texts()
	{
		var seen = new HashSet<string>(StringComparer.Ordinal);

		foreach (var shape in ExpressionCorpus.Shapes)
		{
			if (seen.Add(shape))
				yield return shape;

			for (var length = 5; length < shape.Length; length += 5)
				if (seen.Add(shape.Substring(0, length)))
					yield return shape.Substring(0, length);

			for (var at = 3; at < shape.Length; at += 7)
				if (seen.Add(shape.Remove(at, 1)))
					yield return shape.Remove(at, 1);
		}
	}

	/// <summary>Accepted, or how it was refused and what the state held after — or the refusal it threw.</summary>
	static string Answer(string text, Func<string, ExpressionParser.State, ExpressionParser.Match<LambdaExpression>> read)
	{
		var state = new ExpressionParser.State(typeof(ExpressionRefusalTests).Assembly) { Text = text };

		try
		{
			var match = read(text, state);

			return match.IsSuccess
				? "accepted"
				: $"{match.Outcome} at {match.Position}: {match.Error} {state.Mark()}";
		}
		catch (Exception thrown) when (thrown is FormatException or InvalidOperationException or OverflowException ||
			thrown is ArgumentException and not ArgumentNullException)
		{
			return "threw " + thrown.GetType().Name + " " + state.Mark();
		}
	}

	static string Escaped(string input) =>
		"\"" + input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r") + "\"";

	static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

	static string Expected => Path.Combine(Path.GetDirectoryName(ThisFile)!, "ExpressionRefusalTests.txt");

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "") => path;
}
