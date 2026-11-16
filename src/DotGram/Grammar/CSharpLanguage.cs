using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>
/// The language the generator writes today: C#'s spellings of the types a grammar knows.
/// </summary>
/// <remarks>
/// A keyword names its type whatever is imported; so does a name from <c>global::</c>. A
/// simple or namespace-qualified name is classified too, and it is the asking stage's to decide
/// whether to trust it, since a host may declare a type of the same name.
/// </remarks>
sealed class CSharpLanguage : ITargetLanguage
{
	/// <summary>The one instance: it holds nothing of a compilation's.</summary>
	public static CSharpLanguage Instance { get; } = new();

	const string SequenceSuffix = "[]";

	/// <summary>Every spelling of a known type, with its form.</summary>
	static readonly Dictionary<string, (KnownType Kind, TypeSpellingForm Form)> Spellings = Build();

	/// <summary>Each known type's keyword, in the order of <see cref="KnownType"/>.</summary>
	static readonly string?[] Keywords = Keywords0();

	CSharpLanguage()
	{
	}

	public KnownType Classify(string spelling, out TypeSpellingForm form)
	{
		if (spelling is null)
			throw new ArgumentNullException(nameof(spelling));

		if (Spellings.TryGetValue(spelling, out var known))
		{
			form = known.Form;

			return known.Kind;
		}

		if (IsSequence(spelling))
		{
			form = TypeSpellingForm.Syntax;

			return KnownType.Sequence;
		}

		form = TypeSpellingForm.None;

		return KnownType.None;
	}

	public bool IsGrammarKeyword(string spelling)
	{
		// The keywords of the value types and of text and object; not nint and nuint, which
		// are contextual in C#, and not void, which no rule's value can be.
		return Classify(spelling, out var form) is >= KnownType.Text and <= KnownType.Decimal && form == TypeSpellingForm.Syntax;
	}

	public bool IsSequence(string spelling)
	{
		if (spelling is null)
			throw new ArgumentNullException(nameof(spelling));

		return spelling.EndsWith(SequenceSuffix, StringComparison.Ordinal);
	}

	public string? ElementOf(string spelling)
	{
		return IsSequence(spelling) ? spelling.Substring(0, spelling.Length - SequenceSuffix.Length) : null;
	}

	public string SequenceOf(string element)
	{
		if (element is null)
			throw new ArgumentNullException(nameof(element));

		return element + SequenceSuffix;
	}

	public string Spelling(KnownType kind)
	{
		if ((int)kind < 0 || (int)kind >= Keywords.Length || Keywords[(int)kind] is not { } keyword)
			throw new ArgumentOutOfRangeException(nameof(kind), kind, "A sequence, or no type, has no spelling of its own.");

		return keyword;
	}

	static string?[] Keywords0()
	{
		var keywords = new string?[(int)KnownType.Sequence + 1];

		foreach (var (kind, keyword, _, _) in Table())
			keywords[(int)kind] = keyword;

		return keywords;
	}

	static Dictionary<string, (KnownType, TypeSpellingForm)> Build()
	{
		var spellings = new Dictionary<string, (KnownType, TypeSpellingForm)>(StringComparer.Ordinal);

		foreach (var (kind, keyword, space, name) in Table())
		{
			if (keyword != name)
				spellings.Add(keyword, (kind, TypeSpellingForm.Syntax));

			spellings.Add(name,                        (kind, keyword == name ? TypeSpellingForm.Syntax : TypeSpellingForm.Simple));
			spellings.Add(space + "." + name,          (kind, TypeSpellingForm.Qualified));
			spellings.Add("global::" + space + "." + name, (kind, TypeSpellingForm.Global));
		}

		return spellings;
	}

	/// <summary>
	/// Each known type: its keyword, its namespace and its name there. <c>SourceSpan</c> is
	/// emitted into the host class and has no keyword: its name is what the generated code writes.
	/// </summary>
	static (KnownType Kind, string Keyword, string Space, string Name)[] Table()
	{
		return
		[
			(KnownType.Text,       "string",     "System",  "String"),
			(KnownType.Object,     "object",     "System",  "Object"),
			(KnownType.Boolean,    "bool",       "System",  "Boolean"),
			(KnownType.Char,       "char",       "System",  "Char"),
			(KnownType.SByte,      "sbyte",      "System",  "SByte"),
			(KnownType.Byte,       "byte",       "System",  "Byte"),
			(KnownType.Int16,      "short",      "System",  "Int16"),
			(KnownType.UInt16,     "ushort",     "System",  "UInt16"),
			(KnownType.Int32,      "int",        "System",  "Int32"),
			(KnownType.UInt32,     "uint",       "System",  "UInt32"),
			(KnownType.Int64,      "long",       "System",  "Int64"),
			(KnownType.UInt64,     "ulong",      "System",  "UInt64"),
			(KnownType.Single,     "float",      "System",  "Single"),
			(KnownType.Double,     "double",     "System",  "Double"),
			(KnownType.Decimal,    "decimal",    "System",  "Decimal"),
			(KnownType.NInt,       "nint",       "System",  "IntPtr"),
			(KnownType.NUInt,      "nuint",      "System",  "UIntPtr"),
			(KnownType.Void,       "void",       "System",  "Void"),
			(KnownType.SourceSpan, "SourceSpan", "DotGram", "SourceSpan"),
		];
	}
}
