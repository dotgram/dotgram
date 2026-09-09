using System;
using System.Collections.Generic;
using System.IO;

using DotGram.Grammar.Parsing;

namespace DotGram.Grammar;

/// <summary>
/// The standard library: a grammar every grammar may include, and gets by writing
/// <c>using Std;</c> (§5.2).
/// </summary>
/// <remarks>
/// <para>
/// A <c>.gram</c> file carried inside the generator as a resource, and spliced onto a
/// grammar that asks for it the way an included grammar is — into a namespace of its
/// own, after everything the author wrote, so that nothing they can see moves. It has no
/// class behind it: its rules are compiled into whichever grammar calls them, and the
/// C# they carry names nothing but the framework, in full.
/// </para>
/// <para>
/// Asked for by name and not present otherwise. What the library holds is
/// unqualified-looking — <c>Digits</c>, <c>Identifier</c> — and a grammar that declares
/// the same names would have to know it is shadowing something it never wrote; a
/// <c>using</c> is where it says it knows. The built-in rules are the exception, and
/// there are seven of them (§3.1.1); this is the rest.
/// </para>
/// </remarks>
public static class StandardLibrary
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
	/// Whether a grammar asks for the library — <c>using Std;</c> at its top or at the top of
	/// any namespace in it, an included grammar's included.
	/// </summary>
	public static bool AskedForBy(GrammarFile file)
	{
		if (file is null)
			throw new ArgumentNullException(nameof(file));

		return Asks(file.Usings) || Asks(file.Decls);
	}

	static bool Asks(IReadOnlyList<Using> usings)
	{
		foreach (var import in usings)
			if (!import.IsCSharp && import.Name == Name)
				return true;

		return false;
	}

	static bool Asks(IReadOnlyList<Decl> declarations)
	{
		foreach (var declaration in declarations)
			if (declaration is Decl.Namespace nested && (Asks(nested.Usings) || Asks(nested.Decls)))
				return true;

		return false;
	}

	/// <summary>
	/// The grammar with the library spliced onto its end, under <see cref="Name"/>.
	/// </summary>
	public static string SplicedOnto(string grammarText) =>
		GrammarSplice.Join(
			new GrammarSplice.Part(grammarText, null, null),
			[new GrammarSplice.Part(Text, Name, null)]).Text;
}
