using System;
using System.IO;
using System.Linq;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Language.Tests;

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

			// Written once: the file asked for again is the one that was written, not another moved
			// into its place. A marker the writer cannot know of survives only in the same file.
			var marker = DateTime.UtcNow.AddMinutes(-5);

			File.SetAttributes(paths[0]!, FileAttributes.Normal);
			File.SetCreationTimeUtc(paths[0]!, marker);
			File.SetAttributes(paths[0]!, FileAttributes.ReadOnly);

			// And a copy marked as used within the hour is not marked again.
			var used = File.GetLastWriteTimeUtc(paths[0]!);

			Assert.Equal(paths[0], IncludedGrammarView.PathOf(grammar));
			Assert.Equal(marker, File.GetCreationTimeUtc(paths[0]!));
			Assert.Equal(used, File.GetLastWriteTimeUtc(paths[0]!));
			Assert.Equal(StandardLibrary.Text, File.ReadAllText(paths[0]!));

			// Nothing half written is left beside it.
			Assert.Empty(Directory.EnumerateFiles(Path.GetDirectoryName(paths[0]!)!, grammar.Name + ".*.tmp"));
		}
		finally
		{
			File.SetAttributes(paths[0]!, FileAttributes.Normal);
			File.Delete(paths[0]!);
		}
	}

	/// <summary>A copy found again is marked as used, so that no cleanup takes it for a stale one.</summary>
	[Fact]
	public void A_copy_found_again_is_marked_as_used()
	{
		var grammar = new StandaloneIncludedGrammar("ViewTest" + Guid.NewGuid().ToString("N"), "A = 'a'\nparse A", null);
		var path    = IncludedGrammarView.PathOf(grammar)!;

		try
		{
			File.SetAttributes(path, FileAttributes.Normal);
			File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddDays(-3));
			File.SetAttributes(path, FileAttributes.ReadOnly);

			Assert.Equal(path, IncludedGrammarView.PathToOpen(grammar));
			Assert.True(File.GetLastWriteTimeUtc(path) > DateTime.UtcNow.AddHours(-1));
			Assert.True(File.GetAttributes(path).HasFlag(FileAttributes.ReadOnly));
		}
		finally
		{
			File.SetAttributes(path, FileAttributes.Normal);
			File.Delete(path);
		}
	}
}
