using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A literal read from a source that has more to give but not yet: what is held decides first. A
/// character present that does not fit refuses the literal without asking the source again, and only
/// a literal whose present characters all fit waits for the rest.
/// </summary>
/// <remarks>
/// A live source — a socket, a pipe, a terminal — hands over what it has and blocks until there is
/// more. A literal asked for room for its whole length before it compared anything, so
/// <c>"abc" &amp; eof</c> over a source that had handed over <c>X</c> waited for two more characters it
/// did not need: the answer was already NoMatch at 0. The shapes are the literals the refusal places are
/// held to across forms (RefusalPlaceTests), each with prefixes that refuse and prefixes that agree.
/// </remarks>
public sealed class BlockingSourceTests
{
	/// <summary>A grammar, prefixes a character of which refuses it, and prefixes that fit as far as they go.</summary>
	static readonly (string Name, string Grammar, string[] Refusing, string[] Agreeing)[] Shapes =
	[
		("folded",    "T = \"/*\" & ';'",                                   ["x", "/x"],          ["/"]),
		("comment",   "T = \"/*\" & (?!\"*/\" & any)* & \"*/\" & eof",      ["x", "/x"],          ["/"]),
		("alone",     "T = \"abc\"",                                        ["x", "ax", "abx"],   ["a", "ab"]),
		("whole",     "T = \"abc\" & eof",                                  ["x", "ax", "abx"],   ["a", "ab"]),
		("twice",     "T = \"ab\" & \"cd\" & eof",                          ["x", "abx", "abcx"], ["a", "abc"]),
		("after",     "T = 'x' & \"abc\" & eof",                            ["a", "xx", "xax"],   ["xa", "xab"]),
		("optional",  "T = \"abc\"? & 'a' & eof",                           ["x"],                ["ab"]),
		("repeated",  "T = \"ab\"* & eof",                                  ["abx"],              ["a", "aba"]),
		("choice",    "T = (\"abc\" | \"abd\") & eof",                      ["x", "abx"],         ["a", "ab"]),
		("shorter",   "T = (\"abc\" | 'a') & eof",                          ["x", "ax"],          ["ab"]),
		("prefixed",  "T = ('a' & \"bc\" | 'a' & \"bd\") & eof",            ["x", "ax", "abx"],   ["ab"]),
		("ignored",   "T = \"abc\"i & eof",                                 ["x", "Ax"],          ["A", "aB"]),
		("keywords",  "T = (\"ab\"i | \"cd\"i) & ';' & eof",                ["x", "ax"],          ["a", "C"]),
		("looked",    "W = \"ab\"\nT = ?=W & 'a' & any & eof",              ["x", "ax"],          ["a"]),
		("refused",   "T = ?!\"ab\" & any* & eof",                          ["ab"],               ["a"]),
		("given",     "W = 'x'* & \"abc\"\nT = W & eof",                    ["xb", "xax"],        ["xa", "xab"]),
		("givencase", "W = 'x'* & \"abc\"i\nT = W & eof",                   ["xb", "xAx"],        ["xA", "xaB"]),
		("givenwide", "W = 'x'* & \"äbc\"\nT = W & eof",                    ["xb", "xäx"],        ["xä", "xäb"]),
		("retried",   "T = ('a' | \"abcd\") & eof",                         ["ax", "abx"],        ["ab", "abc"]),
		("beside",    "T = (\"abcd\" | \"abce\" | ['x'|'y']) & eof",        ["ax", "abx"],        ["ab", "abc"]),
	];

	public static TheoryData<string> Names()
	{
		var names = new TheoryData<string>();

		foreach (var (name, _, _, _) in Shapes)
			names.Add(name);

		return names;
	}

	[Theory]
	[MemberData(nameof(Names))]
	public void A_literal_refused_by_what_is_held_does_not_wait_for_more(string name)
	{
		var (_, grammar, refusing, agreeing) = Shapes.Single(one => one.Name == name);
		var failures = new StringBuilder();

		foreach (var direct in new[] { true, false })
		{
			var parser = Build(grammar, direct);
			var form   = direct ? "direct" : "engine";

			foreach (var prefix in refusing)
			{
				var whole = Answer(parser, typeof(string), prefix, null);

				foreach (var (over, buffer) in Forms(prefix))
				{
					var said = Answer(parser, over, prefix, buffer);

					if (said is not null && said != whole)
						failures.Append($"{form} {over.Name} {buffer}: \"{prefix}\" then blocked said {said}, the string {whole}\n");
				}
			}

			foreach (var prefix in agreeing)
				foreach (var (over, buffer) in Forms(prefix))
				{
					var said = Answer(parser, over, prefix, buffer);

					if (said is not null && said != Blocked)
						failures.Append($"{form} {over.Name} {buffer}: \"{prefix}\" then blocked said {said} without reading on\n");
				}
		}

		Assert.True(failures.Length == 0, $"{name}: {grammar}\n{failures}");
	}

	/// <summary>A reader with a buffer of its own and one filled a character at a time; over bytes the same where the prefix is ASCII.</summary>
	static (Type Over, int? Buffer)[] Forms(string prefix)
	{
		return prefix.All(static one => one < 0x80)
			? [(typeof(TextReader), null), (typeof(TextReader), 1), (typeof(Stream), null), (typeof(Stream), 1)]
			: [(typeof(TextReader), null), (typeof(TextReader), 1)];
	}

	const string Blocked = "blocked";

	static Assembly Build(string grammar, bool direct)
	{
		var result = GramCompiler.Compile(grammar + "\nparse T", new GramCompilerOptions
		{
			ClassName     = "Grammar",
			Direct        = direct,
			BufferedInput = true,
			BufferedBytes = true,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics.Where(static one => one.Id != GramCompiler.BufferedUnsupported));

		return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	/// <summary>
	/// The outcome and where the reading stopped, <see cref="Blocked"/> where it asked the source for
	/// more, or null where the form is not offered (a case-insensitive literal over bytes).
	/// </summary>
	static string? Answer(Assembly parser, Type over, string input, int? buffer)
	{
		var type   = parser.GetType("Grammar")!;
		var method = over == typeof(string)
			? type.GetMethod("TryParseT", [typeof(string)])
			: type.GetMethod("TryParseT", [over, typeof(int?), typeof(int?)]);

		if (method is null)
			return null;

		object match;

		try
		{
			match = over == typeof(string)
				? method.Invoke(null, [input])!
				: method.Invoke(null, [over == typeof(Stream) ? new BlockingStream(Encoding.UTF8.GetBytes(input)) : new BlockingReader(input), buffer, null])!;
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is WouldBlockException)
		{
			return Blocked;
		}

		object? Read(string member)
		{
			return match.GetType().GetProperty(member)!.GetValue(match);
		}

		return (bool)Read("IsSuccess")!
			? $"Success@{Convert.ToInt64(Read("Position")) + Convert.ToInt64(Read("Length"))}"
			: $"{Read("Outcome")}@{Read("Position")}";
	}

	/// <summary>What a live source does when asked for more than it has: here, it says so instead of waiting.</summary>
	sealed class WouldBlockException : Exception
	{
	}

	/// <summary>A reader that hands over its text, then blocks.</summary>
	sealed class BlockingReader(string text) : TextReader
	{
		int _at;

		public override int Read(char[] buffer, int index, int count)
		{
			return Read(buffer.AsSpan(index, count));
		}

		public override int Read(Span<char> buffer)
		{
			if (_at == text.Length)
				throw new WouldBlockException();

			var count = Math.Min(buffer.Length, text.Length - _at);

			text.AsSpan(_at, count).CopyTo(buffer);
			_at += count;

			return count;
		}

		public override int Read()
		{
			if (_at == text.Length)
				throw new WouldBlockException();

			return text[_at++];
		}

		public override int Peek()
		{
			if (_at == text.Length)
				throw new WouldBlockException();

			return text[_at];
		}
	}

	/// <summary>The same over bytes.</summary>
	sealed class BlockingStream(byte[] bytes) : Stream
	{
		int _at;

		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => throw new NotSupportedException();

		public override long Position
		{
			get => throw new NotSupportedException();
			set => throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return Read(buffer.AsSpan(offset, count));
		}

		public override int Read(Span<byte> buffer)
		{
			if (_at == bytes.Length)
				throw new WouldBlockException();

			var count = Math.Min(buffer.Length, bytes.Length - _at);

			bytes.AsSpan(_at, count).CopyTo(buffer);
			_at += count;

			return count;
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}
	}
}
