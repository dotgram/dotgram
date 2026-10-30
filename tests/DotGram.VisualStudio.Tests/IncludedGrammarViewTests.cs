using System;
using System.IO;
using System.Linq;

using DotGram.Grammar;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class IncludedGrammarViewTests
{
	/// <summary>
	/// A copy is named for its content, written once and never changed: asking again, or asking
	/// from many places at once as several instances of Visual Studio may, hands back the same
	/// file, read-only and whole.
	/// </summary>
	[Fact]
	public void A_copy_is_content_addressed_and_written_once_however_many_ask()
	{
		var grammar = new StandaloneIncludedGrammar("ViewTest" + Guid.NewGuid().ToString("N"), StandardLibrary.Text, null);

		var paths = Enumerable.Range(0, 16)
			.AsParallel()
			.Select(_ => IncludedGrammarView.PathOf(grammar))
			.ToArray();

		try
		{
			Assert.All(paths, path => Assert.Equal(paths[0], path));
			Assert.Equal(StandardLibrary.Text, File.ReadAllText(paths[0]!));
			Assert.True(File.GetAttributes(paths[0]!).HasFlag(FileAttributes.ReadOnly));

			var written = File.GetLastWriteTimeUtc(paths[0]!);

			Assert.Equal(paths[0], IncludedGrammarView.PathOf(grammar));
			Assert.Equal(written, File.GetLastWriteTimeUtc(paths[0]!));

			// Nothing half written is left beside it.
			Assert.Empty(Directory.EnumerateFiles(Path.GetDirectoryName(paths[0]!)!, grammar.Name + ".*.tmp"));
		}
		finally
		{
			File.SetAttributes(paths[0]!, FileAttributes.Normal);
			File.Delete(paths[0]!);
		}
	}
}
