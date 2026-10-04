#pragma warning disable RS1035 // SPIKE: file output from the generator, test-only switch
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Emit;

public static partial class CSharpEmitter
{
	/// <summary>SPIKE: the class being emitted, for the dump's file names.</summary>
	[ThreadStatic] static string? SpikeScoped;

	/// <summary>SPIKE: whether the contained-joining pass runs (DOTGRAM_SPIKE_CONTAINED=1).</summary>
	static bool SpikeContained()
	{
		return Environment.GetEnvironmentVariable("DOTGRAM_SPIKE_CONTAINED") == "1";
	}

	/// <summary>SPIKE: an override of the limit on rules a guest adds (DOTGRAM_SPIKE_MAXX).</summary>
	static int? SpikeLimit()
	{
		return int.TryParse(Environment.GetEnvironmentVariable("DOTGRAM_SPIKE_MAXX"), out var limit) ? limit : null;
	}

	/// <summary>SPIKE: appends a line to the joins log under DOTGRAM_SPIKE_DUMP, where it is set.</summary>
	static void SpikeLog(string line)
	{
		var dir = Environment.GetEnvironmentVariable("DOTGRAM_SPIKE_DUMP");

		if (string.IsNullOrEmpty(dir))
			return;

		Directory.CreateDirectory(dir);
		File.AppendAllText(Path.Combine(dir, "joins.txt"), line + "\n");
	}

	/// <summary>SPIKE: writes one machine's structure (back edges, memo slots, MemoWords) under DOTGRAM_SPIKE_DUMP.</summary>
	static void SpikeDump(string scoped, Compiled compiled)
	{
		var dir = Environment.GetEnvironmentVariable("DOTGRAM_SPIKE_DUMP");

		if (string.IsNullOrEmpty(dir))
			return;

		Directory.CreateDirectory(dir);
		var anchor = compiled.Machine.Anchor is { } rule ? IdentifierOf(rule) : "none";
		File.WriteAllText(
			Path.Combine(dir, scoped + "." + anchor + ".txt"),
			"publications " + string.Join(", ", compiled.Publications.Select(publication => publication.MethodName)) + "\n" +
			compiled.Machine.SpikeDump());
	}
}
