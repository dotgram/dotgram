using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The enumerator a generated parser takes from a sequence of lines is disposed by the parser:
/// when a lazily returned sequence has been walked to its end, when the caller stops early or an
/// exception passes through, and not before the caller has started walking it.
/// </summary>
public sealed class LinesDisposeTests
{
	const string Grammar =
		"Num : @string = d: ['0'..'9']+ => @(Check(d.ToString()))\n" +
		"find Num\n";

	const string Members =
		"static string Check(string text) { if (text == \"13\") throw new global::System.InvalidOperationException(\"unlucky\"); return text; }\n";

	static readonly Type Host = Compiled(Grammar);

	static Type Compiled(string grammar)
	{
		var result = GramCompiler.Compile(
			grammar,
			new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance, Direct = true });

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		return EmittedCode.Compile(result.Sources[0].Text, declarationMembers: Members).GetType("Grammar")!;
	}

	sealed class Spy : IEnumerable<string>
	{
		readonly string[] _lines;

		public Spy(params string[] lines)
		{
			_lines = lines;
		}

		public int Enumerators;
		public int Disposed;

		public IEnumerator<string> GetEnumerator()
		{
			Enumerators++;

			return new Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		sealed class Enumerator : IEnumerator<string>
		{
			readonly Spy _owner;
			int _at = -1;

			public Enumerator(Spy owner)
			{
				_owner = owner;
			}

			public string Current => _owner._lines[_at];

			object IEnumerator.Current => Current;

			public bool MoveNext()
			{
				return ++_at < _owner._lines.Length;
			}

			public void Reset()
			{
				_at = -1;
			}

			public void Dispose()
			{
				_owner.Disposed++;
			}
		}
	}

	static IEnumerable<string> Find(IEnumerable<string> lines, Type? host = null, string name = "FindNum")
	{
		var method = (host ?? Host).GetMethod(name, [typeof(IEnumerable<string>)])!;

		try
		{
			return (IEnumerable<string>)Enumerate((IEnumerable)method.Invoke(null, [lines])!);
		}
		catch (TargetInvocationException e)
		{
			throw e.InnerException!;
		}
	}

	static IEnumerable<string> Enumerate(IEnumerable matches)
	{
		foreach (var match in matches)
		{
			var value = match.GetType().GetProperty("Value")!.GetValue(match);

			yield return (string)value!;
		}
	}

	[Fact]
	public void NothingIsTakenBeforeTheWalkStarts()
	{
		var spy    = new Spy("1", "2");
		var result = Find(spy);

		Assert.Equal(0, spy.Enumerators);

		result.GetEnumerator().Dispose();

		Assert.Equal(0, spy.Enumerators);
	}

	[Fact]
	public void DisposedOnceAfterTheWalkEnds()
	{
		var spy = new Spy("1", "x", "2");

		Assert.Equal(["1", "2"], Find(spy).ToArray());
		Assert.Equal(1, spy.Enumerators);
		Assert.Equal(1, spy.Disposed);
	}

	[Fact]
	public void NotDisposedWhileTheCallerIsStillWalking()
	{
		var spy = new Spy("1", "2", "3");

		using var walk = Find(spy).GetEnumerator();

		Assert.True(walk.MoveNext());
		Assert.Equal("1", walk.Current);
		Assert.Equal(0, spy.Disposed);

		Assert.True(walk.MoveNext());
		Assert.Equal(0, spy.Disposed);
	}

	[Fact]
	public void DisposedWhenTheCallerStopsEarly()
	{
		var spy = new Spy("1", "2", "3");

		Assert.Equal("1", Find(spy).First());
		Assert.Equal(1, spy.Enumerators);
		Assert.Equal(1, spy.Disposed);
	}

	[Fact]
	public void DisposedWhenTheCallerThrows()
	{
		var spy = new Spy("1", "2");

		Assert.Throws<ApplicationException>(() =>
		{
			foreach (var _ in Find(spy))
				throw new ApplicationException();
		});

		Assert.Equal(1, spy.Disposed);
	}

	[Fact]
	public void DisposedWhenAHookThrows()
	{
		var spy = new Spy("1", "13", "2");

		Assert.Throws<InvalidOperationException>(() => Find(spy).ToArray());
		Assert.Equal(1, spy.Disposed);
	}

	[Fact]
	public void NoSequenceIsAnArgumentError()
	{
		Assert.Throws<ArgumentNullException>(() => Find(null!));
	}

	[Fact]
	public void AFileIsClosedWhenTheCallerStopsEarly()
	{
		if (!OperatingSystem.IsLinux())
			return;

		var path = Path.Combine(Path.GetTempPath(), "lines-dispose-" + Guid.NewGuid().ToString("N") + ".txt");

		File.WriteAllLines(path, Enumerable.Repeat("1", 20_000));

		try
		{
			Assert.Equal("1", Find(File.ReadLines(path)).First());
			Assert.DoesNotContain(
				Directory.GetFiles("/proc/self/fd").Select(static fd => new FileInfo(fd).LinkTarget),
				target => target == path);
		}
		finally
		{
			File.Delete(path);
		}
	}

	[Theory]
	[InlineData("OverLines")]
	[InlineData("lines")]
	[InlineData("item")]
	public void APublicationMayBeNamedLikeTheLocalsOfTheOverload(string name)
	{
		var host = Compiled(Grammar.Replace("find Num", "find Num as " + name, StringComparison.Ordinal));
		var spy  = new Spy("1", "2");

		Assert.Equal(["1", "2"], Find(spy, host, name).ToArray());
		Assert.Equal(1, spy.Disposed);
	}
}
