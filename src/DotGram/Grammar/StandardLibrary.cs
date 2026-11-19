using System;
using System.Collections.Generic;
using System.IO;

namespace DotGram.Grammar;

/// <summary>
/// The standard library: a grammar every grammar has, under <c>Std</c> (§5.2).
/// </summary>
/// <remarks>
/// <para>
/// A <c>.gram</c> file carried inside the generator as a resource, and spliced onto every
/// grammar the way an included grammar is — into a namespace of its own, after everything
/// the author wrote, so that nothing they can see moves. It has no class behind it: its
/// rules are compiled into whichever grammar calls them, and the C# they carry names
/// nothing but the framework, in full.
/// </para>
/// <para>
/// Always there, the way the framework is to C#: <c>Std.Integer</c> names it in full with
/// nothing declared, and <c>using Std;</c> opens the namespace as it opens any other. What
/// is not called is pruned, so a grammar that never names it pays a few milliseconds of
/// compilation and nothing in what comes out.
/// </para>
/// </remarks>
static partial class StandardLibrary
{
	/// <summary>The namespace a grammar reaches the library through.</summary>
	public const string Name = "Std";

	static string? _text;

	/// <summary>The library's own text, as it was written.</summary>
	public static string Text => _text ??= Read();

	static string Read()
	{
		using var stream = typeof(StandardLibrary).Assembly.GetManifestResourceStream("DotGram.Std.gram")
			?? throw new InvalidOperationException("The standard library is not in this assembly.");
		using var reader = new StreamReader(stream);

		return reader.ReadToEnd();
	}

	/// <summary>
	/// The grammar with the library spliced onto its end, under <see cref="Name"/>.
	/// </summary>
	public static string SplicedOnto(string grammarText)
	{
		return GrammarSplice.Join(
			new GrammarSplice.Part(grammarText, null, null),
			[new GrammarSplice.Part(Text, Name, null)]).Text;
	}
}
