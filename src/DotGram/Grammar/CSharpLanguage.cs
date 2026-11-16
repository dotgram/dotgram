using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>
/// The language the generator writes today: C#'s spellings of the types a grammar knows.
/// </summary>
/// <remarks>
/// <para>
/// A keyword names its type whatever is imported; so does a name from <c>global::</c>. A
/// simple or namespace-qualified name is classified too, and it is the asking stage's to decide
/// whether to trust it, since a host may declare a type of the same name.
/// </para>
/// <para>
/// C# handed over in a grammar is read by an <see cref="ICSharpScanner"/> — Roslyn's, in the
/// generator and the editors — and the names it binds to where the generated code stands are
/// asked of the resolver, where that one can say (<see cref="ICSharpNames"/>). One instance is
/// made for each compilation, of the scanner and the resolver that compilation has.
/// </para>
/// <para>
/// The host questions go to that resolver too: Roslyn's in a build that sees the host, the
/// answers the incremental generator collected for it ahead of time, a permissive one where the
/// host cannot be seen, and none at all for a grammar compiled from a string. Nothing here
/// caches an answer; the resolver behind it does, where it was built to.
/// </para>
/// </remarks>
sealed class CSharpLanguage : ITargetLanguage
{
	/// <summary>C# with nothing to read it and no host: what a grammar compiled from a string alone has.</summary>
	public static CSharpLanguage Instance { get; } = new();

	const string SequenceSuffix = "[]";

	/// <summary>Every spelling of a known type, with its form.</summary>
	static readonly Dictionary<string, (KnownType Kind, TypeSpellingForm Form)> Spellings = Build();

	/// <summary>Each known type's keyword, in the order of <see cref="KnownType"/>.</summary>
	static readonly string?[] Keywords = Keywords0();

	readonly ISymbolResolver _resolver;
	readonly ICSharpScanner? _scanner;
	readonly ICSharpNames?   _names;

	/// <param name="resolver">The host; null for none, which answers every question permissively.</param>
	/// <param name="scanner">Reads the C# a grammar hands over; null where nothing can.</param>
	public CSharpLanguage(ISymbolResolver? resolver = null, ICSharpScanner? scanner = null)
	{
		_resolver  = resolver ?? PermissiveSymbolResolver.Instance;
		_scanner   = scanner;
		_names     = resolver as ICSharpNames;
		HasHost    = resolver is not null;
		HostUnseen = resolver is UnseenHostSymbolResolver;
	}

	/// <summary>A language whose C# this scanner reads, for a caller that holds only a scanner.</summary>
	public static CSharpLanguage Reading(ICSharpScanner? scanner)
	{
		return scanner is null ? Instance : new CSharpLanguage(scanner: scanner);
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

	public KnownType ClassifyLoosely(string spelling)
	{
		if (spelling is null)
			throw new ArgumentNullException(nameof(spelling));

		// `global::` and then `System.`, each where it is there: `System.int` and `global::String`
		// are read as `int` and `String`, which C# would not, and that is the reading this answers.
		var name = spelling;

		if (name.StartsWith("global::", StringComparison.Ordinal))
			name = name.Substring("global::".Length);

		if (name.StartsWith("System.", StringComparison.Ordinal))
			name = name.Substring("System.".Length);

		return Classify(name, out var form) is var kind && form is TypeSpellingForm.Syntax or TypeSpellingForm.Simple
			? kind
			: KnownType.None;
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

	public bool ReadsFragments => _scanner is not null;

	public bool TryFindFragmentEnd(string text, int open, out int close)
	{
		if (_scanner is null)
		{
			close = -1;

			return false;
		}

		return _scanner.TryFindClosingParenthesis(text, open, out close);
	}

	public IReadOnlyCollection<string>? NamesUsed(string fragment)
	{
		return _scanner?.FreeNames(fragment);
	}

	public IReadOnlyCollection<string>? NamesNotUsedAsValues(string fragment)
	{
		return _scanner?.NamesNotAsValues(fragment);
	}

	public string? Renamed(string fragment, IReadOnlyDictionary<string, string> names)
	{
		return _scanner?.Renamed(fragment, names);
	}

	public bool Spells(string fragment, string name, NameSearch search)
	{
		if (search == NameSearch.Substring)
			return fragment.Contains(name);

		for (var at = fragment.IndexOf(name, StringComparison.Ordinal); at >= 0;
			at = fragment.IndexOf(name, at + 1, StringComparison.Ordinal))
		{
			if ((at == 0 || !(Continues(fragment[at - 1]) || search == NameSearch.WordNotMember && fragment[at - 1] == '.')) &&
				(at + name.Length == fragment.Length || !Continues(fragment[at + name.Length])))
				return true;
		}

		return false;

		// What may stand inside a C# identifier, an `@` among it: `@name` is the name.
		static bool Continues(char c)
		{
			return char.IsLetterOrDigit(c) || c is '_' or '@';
		}
	}

	public string? SoleName(string fragment, SoleNameRule rule)
	{
		var name = fragment.Trim();

		switch (rule)
		{
			case SoleNameRule.OneBracket:
				if (name.StartsWith("(", StringComparison.Ordinal) && name.EndsWith(")", StringComparison.Ordinal))
					name = name[1..^1].Trim();

				return name;

			case SoleNameRule.EveryBracket:
			case SoleNameRule.EveryBracketIdentifier:
				while (name.Length > 1 && name[0] == '(' && name[name.Length - 1] == ')')
					name = name.Substring(1, name.Length - 2).Trim();

				if (rule == SoleNameRule.EveryBracket)
					return name;

				return name.Length > 0 && (char.IsLetter(name[0]) || name[0] == '_') && IsWord(name) ? name : null;

			case SoleNameRule.WrappingBrackets:
				// The brackets come off only where they wrap the whole: `(a) + (b)` is not `a) + (b`.
				while (name.Length > 2 && name[0] == '(' && name[^1] == ')' && Wrapping(name))
					name = name[1..^1].Trim();

				return name.Length > 0 && IsWord(name) ? name : null;

			default:
				throw new ArgumentOutOfRangeException(nameof(rule), rule, null);
		}

		static bool IsWord(string text)
		{
			foreach (var c in text)
				if (!char.IsLetterOrDigit(c) && c != '_')
					return false;

			return true;
		}

		// Whether the bracket that opens the text is the one that closes it.
		static bool Wrapping(string text)
		{
			var depth = 0;

			for (var at = 0; at < text.Length; at++)
			{
				if (text[at] == '(')
					depth++;

				else if (text[at] == ')' && --depth == 0)
					return at == text.Length - 1;
			}

			return false;
		}
	}

	public bool KnowsHostNames => _names is not null;

	public bool HostAnswersTo(string name, IReadOnlyList<string> imports)
	{
		return _names is not null && _names.Answers(name, imports);
	}

	public IReadOnlyCollection<string>? NamesUnboundIn(string fragment, IReadOnlyList<string> imports)
	{
		return _names?.UnboundIn(fragment, imports);
	}

	public bool HasHost { get; }

	public bool HostUnseen { get; }

	public bool TypeExists(string name)
	{
		return _resolver.TypeExists(name);
	}

	public bool Fits(string from, string to)
	{
		return _resolver.IsAssignable(from, to);
	}

	public bool TryWaysToBuild(string type, out IReadOnlyList<IReadOnlyList<MethodParameter>> ways)
	{
		return _resolver.TryResolveConstructors(type, out ways);
	}

	public bool TrySettableMembers(string type, out IReadOnlyList<ObjectMember> members)
	{
		return _resolver.TryResolveSettableProperties(type, out members);
	}

	public ExternalValueResolution ExternalValue(string method, string? against, out string? valueType)
	{
		return _resolver.TryResolveExternalValue(method, against, out valueType);
	}

	public ExternalMethodResolution ExternalMethod(string method, ExternalMethodRole role)
	{
		return _resolver.ResolveExternalMethod(method, role);
	}

	public bool Rewinds(string type)
	{
		return _resolver.Rewinds(type);
	}

	public bool IsReferenceType(string type)
	{
		return _resolver.IsReferenceType(type);
	}

	public bool IsOptional(string type)
	{
		return _resolver.IsNullable(type);
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
