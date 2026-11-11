using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>
/// The names the generator's attributes had before 0.3.0, and what each is called now: what the build
/// tells a project still written with one (GRAM0012), and the tool tells a <c>--option</c> written
/// with one.
/// </summary>
/// <remarks>
/// The old names are gone from source; C# says only that it cannot find one, which does not say what
/// to write instead. A referenced assembly built by 0.2 is still read under them for one release
/// (<c>GramHostOptions</c>, <c>GramIncludes</c>), and this goes with that reading in 0.4.
/// </remarks>
static class GramRenames
{
	/// <summary>The identifier a use of an old name is reported under.</summary>
	public const string Id = "GRAM0012";

	/// <summary>The second-reading attribute's old name, with and without its suffix.</summary>
	public const string OldVariant = "GramOptions";

	/// <summary>What it is now: its name, given as its first argument rather than <c>Suffix</c>.</summary>
	public const string NewVariant = "GramParserVariant(\"…\")";

	/// <summary>The carrier's old member and its new one.</summary>
	public const string OldCarrier = "GramCarrier.Tape";

	public const string NewCarrier = "GramCarrier.Deferred";

	/// <summary>The options of <c>[Gram]</c> and <c>[GramParserVariant]</c> that were renamed, old to new.</summary>
	public static IReadOnlyDictionary<string, string> Options { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["IncludedAs"]    = "DefaultAlias",
		["Suffix"]        = "Name",
		["BufferedInput"] = "BufferedText",
		["SpanCaptures"]  = "UseSpanCaptures",
		["BufferSize"]    = "InitialBufferSize",
		["MaxRetained"]   = "MaxBufferedInputLength",
		["Stacks"]        = "MaxExtraStacks",
		["PartSize"]      = "TargetMethodPartSize",
		["PerCall"]       = "UsePerCallLocations",
		["Portable"]      = "ExportGrammar",
	};

	/// <summary>The option of <c>[GramInclude]</c> that was renamed, old to new.</summary>
	public static IReadOnlyDictionary<string, string> IncludeOptions { get; } = new Dictionary<string, string>(StringComparer.Ordinal)
	{
		["As"] = "Alias",
	};

	/// <summary>What is said of one old name.</summary>
	public static string Message(string old, string now)
	{
		return $"'{old}' was renamed to '{now}' in 0.3.0";
	}
}
