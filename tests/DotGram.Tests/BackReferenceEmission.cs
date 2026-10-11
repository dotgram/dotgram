using System;
using System.Linq;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Which back ends generate code for bindings yet, and the skip that stands in for a test of one
/// that does not.
/// </summary>
/// <remarks>
/// <para>
/// The grammar's own stages take bindings — they are read, bound, checked and refused with their
/// diagnostics — before any back end writes code for them, and the tests of what a generated
/// parser does with one are written ahead of that code. Each such test asks here first and is
/// skipped, with the reason, until the back end it runs on is marked as generating them: so the
/// suite stays green and the tests are there, compiled and listed as skipped, to turn on.
/// </para>
/// <para>
/// Turn a back end on by setting its field to true, in the change that makes it generate code
/// for bindings. To run the tests before that, against work in progress, set
/// <c>DOTGRAM_BACKREF_EMISSION</c> to a comma-separated list of back ends
/// (<c>reader,engine</c>) or to <c>all</c>.
/// </para>
/// <para>
/// Once every field is true this class has done its work and goes, with every call to it.
/// </para>
/// </remarks>
static class BackReferenceEmission
{
	/// <summary>The reader over contiguous input: <c>Direct</c> on, a string.</summary>
	static readonly bool ReaderGenerates = false;

	/// <summary>The engine: <c>Direct</c> off, <c>find</c>, and what the reader hands it.</summary>
	static readonly bool EngineGenerates = false;

	public static bool Reader => ReaderGenerates || Asked("reader");

	public static bool Engine => EngineGenerates || Asked("engine");

	/// <summary>Skips the calling test unless the reader generates code for bindings.</summary>
	public static void RequireReader()
	{
		Assert.SkipUnless(Reader, "Pending: the reader does not generate code for bindings yet (BackReferenceEmission).");
	}

	/// <summary>Skips the calling test unless the engine generates code for bindings.</summary>
	public static void RequireEngine()
	{
		Assert.SkipUnless(Engine, "Pending: the engine does not generate code for bindings yet (BackReferenceEmission).");
	}

	/// <summary>Skips the calling test unless both back ends generate code for bindings.</summary>
	public static void RequireBoth()
	{
		RequireReader();
		RequireEngine();
	}

	static bool Asked(string backEnd)
	{
		var asked = Environment.GetEnvironmentVariable("DOTGRAM_BACKREF_EMISSION");

		if (string.IsNullOrWhiteSpace(asked))
			return false;

		return asked
			.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Any(one => one is "all" || string.Equals(one, backEnd, StringComparison.OrdinalIgnoreCase));
	}
}
