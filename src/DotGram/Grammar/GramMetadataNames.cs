using System;

namespace DotGram.Grammar;

/// <summary>
/// The names of the attributes a generated parser is annotated with for tooling: what the
/// generator writes, what it reads back from a class, and what an editor looks for in a
/// referenced assembly's metadata.
/// </summary>
/// <remarks>
/// <para>
/// One spelling for all three. The attributes are emitted into every assembly separately
/// (<see cref="Emit.SupportEmitter.Attributes"/>) and are read by name rather than by type
/// identity, so a name written differently in one of the places would be a format the
/// others do not read, with nothing to say so.
/// </para>
/// <para>
/// Simple names, as a declaration writes them, and the full names Roslyn's
/// <c>ToDisplayString</c> gives back for comparing against.
/// </para>
/// </remarks>
public static class GramMetadataNames
{
	/// <summary>The namespace every attribute of the generator is emitted into.</summary>
	public const string Namespace = "DotGram";

	/// <summary>Names the language a grammar describes: <c>[GramLanguage("id")]</c>.</summary>
	public const string Language = "GramLanguageAttribute";

	/// <summary>Gives a rule or a capture a semantic role: <c>[GramClassify("Rule", GramClassification.Keyword)]</c>.</summary>
	public const string Classify = "GramClassifyAttribute";

	/// <summary>The versioned language descriptor the generator writes on a parser it compiled.</summary>
	public const string LanguageDescriptor = "GramLanguageDescriptorAttribute";

	/// <summary>A non-executing editor decision for a grammar guard.</summary>
	public const string ToolingGuard = "GramToolingGuardAttribute";

	/// <summary>An external recognizer mapped to an equivalent grammar rule for editors.</summary>
	public const string ToolingExternal = "GramToolingExternalAttribute";

	/// <summary><see cref="Language"/> with its namespace.</summary>
	public const string QualifiedLanguage = Namespace + "." + Language;

	/// <summary><see cref="Classify"/> with its namespace.</summary>
	public const string QualifiedClassify = Namespace + "." + Classify;

	/// <summary><see cref="LanguageDescriptor"/> with its namespace.</summary>
	public const string QualifiedLanguageDescriptor = Namespace + "." + LanguageDescriptor;

	/// <summary><see cref="ToolingGuard"/> with its namespace.</summary>
	public const string QualifiedToolingGuard = Namespace + "." + ToolingGuard;

	/// <summary><see cref="ToolingExternal"/> with its namespace.</summary>
	public const string QualifiedToolingExternal = Namespace + "." + ToolingExternal;
}
