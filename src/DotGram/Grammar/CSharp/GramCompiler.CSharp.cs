using System;
using System.Collections.Generic;

using DotGram.Grammar.Emit;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar;

/// <summary>The compiler as every caller of today asks for it: the parser written in C#.</summary>
static partial class GramCompiler
{
	/// <summary>
	/// Reads one grammar whose host fragments are C#: the first two stages, with the standard library
	/// spliced on.
	/// </summary>
	/// <param name="grammarText">Contents of a <c>.gram</c> file.</param>
	/// <param name="scanner">Finds where an inline <c>@(...)</c> ends; null refuses one (GRAM1007).</param>
	/// <param name="own">Where the author's own text ends in <paramref name="grammarText"/>, where grammars were joined after it.</param>
	/// <param name="cuts">Where each further text of its own begins.</param>
	public static ParseResult Read(
		string grammarText, ICSharpScanner? scanner = null, int? own = null, IReadOnlyList<int>? cuts = null)
	{
		if (grammarText is null)
			throw new ArgumentNullException(nameof(grammarText));

		return Read(CSharpLanguage.Reading(scanner), grammarText, own, cuts);
	}

	/// <summary>Compiles one grammar.</summary>
	/// <param name="grammarText">Contents of a <c>.gram</c> file.</param>
	/// <param name="options">Compilation options; defaults are used when null.</param>
	public static GramCompilation Compile(string grammarText, GramCompilerOptions? options = null)
	{
		if (grammarText is null)
			throw new ArgumentNullException(nameof(grammarText));

		options ??= new GramCompilerOptions();

		// The shape of the host is the compilation's, from its options, whichever C# language reads the
		// grammar: a language handed in brings its resolver and its scanner, never the class, the
		// readings or what values are told their locations through.
		var language = options.Language is CSharpLanguage given ? given.WithHost(options.Host)
			: options.Language ?? new CSharpLanguage(options.SymbolResolver, options.CSharpScanner, options.Host);

		return Compile(language, grammarText, options);
	}

	/// <summary>
	/// Emits <c>[Gram]</c>, which is the whole of what a compilation gets before any grammar
	/// in it has been looked at. Always internal, always present, one copy per compilation.
	/// <c>SourceSpan</c> used to be here beside it and is emitted into each host class
	/// instead, where it can be public without two assemblies colliding over it.
	/// </summary>
	public static GeneratedSource EmitMarkerAttributes()
	{
		return new("DotGram.Attributes.g.cs", SupportEmitter.Attributes);
	}
}
