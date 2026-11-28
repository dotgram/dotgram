using System;
using System.Collections.Generic;
using System.Linq;

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

	readonly ISymbolResolver? _given;
	readonly ISymbolResolver  _resolver;
	readonly ICSharpScanner?  _scanner;
	readonly ICSharpNames?    _names;

	/// <param name="resolver">The host; null for none, which answers every question permissively.</param>
	/// <param name="scanner">Reads the C# a grammar hands over; null where nothing can.</param>
	/// <param name="host">
	/// The shape of the C# the parser is generated into, where a compilation has one: what values
	/// are told their locations through, among the rest.
	/// </param>
	public CSharpLanguage(ISymbolResolver? resolver = null, ICSharpScanner? scanner = null, CSharpHostShape? host = null)
	{
		Host       = host ?? new CSharpHostShape();
		_given     = resolver;
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

	/// <summary>A <see cref="char"/>, which is what a C# string is made of.</summary>
	public TextUnit TextUnit => TextUnit.Utf16CodeUnit;

	public TargetWording Wording => CSharpWording.Instance;

	/// <summary>
	/// What C# can do. Every answer is yes for the language and the runtimes the generator writes for;
	/// a test makes a C# that answers one of them otherwise (<see cref="With"/>), so that the fork
	/// each stands at is read by a second target before there is one.
	/// </summary>
	public TargetCapabilities Can { get; private init; } = OfCSharp.Capabilities;

	/// <summary>In a class of its own, so that it is made before the first language is, whatever the order of the fields here.</summary>
	static class OfCSharp
	{
		internal static readonly TargetCapabilities Capabilities = new()
		{
			UnstructuredJumps  = true,
			ByteStringLiterals = true,
		};
	}

	/// <summary>The same language — its resolver, its scanner and its host — able to do what <paramref name="can"/> says.</summary>
	internal CSharpLanguage With(TargetCapabilities can)
	{
		return new CSharpLanguage(_given, _scanner, Host) { Can = can };
	}

	/// <summary>What reads the library's own C#, whatever the author's is read by.</summary>
	public ITargetLanguage ForLibrary => Library.Reader;

	/// <summary>Made when first asked for, and once.</summary>
	static class Library
	{
		internal static readonly CSharpLanguage Reader = Reading(StandardLibrary.Scanner);
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

	/// <summary>
	/// Whether <paramref name="type"/> may be one that every array is — <c>object</c>, <c>Array</c> and
	/// the interfaces arrays implement — by its spelling alone: with or without <c>global::</c> and one
	/// of the namespaces they live in. A generic interface may be one, depending on its argument.
	/// </summary>
	internal static bool EveryArrayMayBe(string type)
	{
		var name = type.StartsWith("global::", StringComparison.Ordinal) ? type.Substring("global::".Length) : type;

		foreach (var space in new[] { "System.Collections.Generic.", "System.Collections.", "System." })
			if (name.StartsWith(space, StringComparison.Ordinal))
			{
				name = name.Substring(space.Length);

				break;
			}

		return name.IndexOf('<') >= 0 || name is
			"object" or "Object" or "Array" or "ICloneable" or "IEnumerable" or "ICollection" or "IList" or
			"IStructuralComparable" or "IStructuralEquatable";
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

	public string? Emit(Model.RecognitionGraph graph, GramCompilerOptions options, EmitContext context)
	{
		return DotGram.Grammar.Emit.CSharpEmitter.Emit(graph, options, context);
	}

	public string SourceName(GramCompilerOptions options, int part)
	{
		var name = options.Suffix is { Length: > 0 } suffix
			? $"{options.ClassName}.{suffix}.gram.g.cs"
			: $"{options.ClassName}.gram.g.cs";

		return part == 0 ? name : name.Replace(".gram.g.cs", $".gram.part-{part:D4}.g.cs");
	}

	public IReadOnlyList<GramDiagnostic> NameClashes(Model.RecognitionGraph graph, GramCompilerOptions options)
	{
		var said = DotGram.Grammar.Emit.CSharpEmitter.NameClashes(
			graph,
			ClassOf(options),
			options.Namespace,
			DotGram.Grammar.Emit.CSharpEmitter.ClassesOf(options),
			options.Lexical);

		// The limits are declared in the class the reading goes into, or in the host where the reading shares
		// the host's, which can be called neither of their names (CS0542) nor declare either itself (CS0102):
		// said here, against the grammar, rather than as an error in code the author did not write. A reading
		// that shares them can be called neither name either, which its class and the limits would both be in
		// the host, nor declare one, which would stand between it and the host's.
		if (graph.Publications.Count > 0)
		{
			var shared = options.SharedTypes && options.SharesLimits;
			var host   = options.ClassName.Substring(options.ClassName.LastIndexOf('.') + 1);
			var into   = options.Suffix is { Length: > 0 } suffix ? suffix : host;
			var owner  = shared ? host : into;

			foreach (var name in DotGram.Grammar.Emit.CSharpEmitter.LimitsNames)
			{
				if (into == name || owner == name)
					said.Add(new GramDiagnostic(
						DotGram.Grammar.Emit.CSharpEmitter.GeneratedNameClash,
						$"The class '{name}' is given a member '{name}', the limits a caller sets (docs/syntax.md §6.5), " +
						"which no member may share its class's name with. Rename the class.",
						0, 0, GramSeverity.Error));

				if (options.MembersDeclared.Contains(name))
					said.Add(new GramDiagnostic(
						DotGram.Grammar.Emit.CSharpEmitter.GeneratedNameClash,
						$"'{owner}' declares '{name}', which the generated part declares too: the limits a caller sets " +
						"(docs/syntax.md §6.5). Rename the member.",
						0, 0, GramSeverity.Error));

				if (shared && options.MembersDeclaredInReading.Contains(name))
					said.Add(new GramDiagnostic(
						DotGram.Grammar.Emit.CSharpEmitter.GeneratedNameClash,
						$"'{into}' declares '{name}', the class of a reading of a host that has several: they share " +
						$"one set of limits, declared in '{host}' (docs/syntax.md §6.5). Rename the member.",
						0, 0, GramSeverity.Error));
			}
		}

		// A host with several readings has one span, in the host; a part the author declared in the
		// class of a reading would be a second span of the same name, which the reading reads instead.
		if (options.SharedTypes && options.SpanDeclaredInReading)
			said.Add(new GramDiagnostic(
				DotGram.Grammar.Emit.CSharpEmitter.GeneratedNameClash,
				$"'SourceSpan' is declared in '{options.Suffix}', the class of a reading of a host that has several: " +
				"they share one span, written in the host. Declare its part in the host instead (docs/syntax.md §6.2).",
				0, 0, GramSeverity.Error));

		return said;
	}

	public GramNames NamesOf(Model.RecognitionGraph graph, IReadOnlyList<Model.RecognitionGraph> more, GramCompilerOptions options)
	{
		return DotGram.Grammar.Emit.CSharpEmitter.NamesOf(
			graph, more, ClassOf(options), options.Namespace, DotGram.Grammar.Emit.CSharpEmitter.ClassesOf(options), options.Lexical);
	}

	public bool ScansSeam(Model.LexicalSplit split)
	{
		var seam = DotGram.Grammar.Emit.CSharpEmitter.Seam(split);

		return split.Trivia.Any(rule => seam.Scanner(rule) is not null);
	}

	/// <summary>The class the publications are written into, as a chain: the host, and the reading nested in it.</summary>
	static string ClassOf(GramCompilerOptions options)
	{
		return options.Suffix is { Length: > 0 } nested ? options.ClassName + "." + nested : options.ClassName;
	}

	public string RenderFragment(string fragment)
	{
		return $"({fragment})";
	}

	public string RenderReference(string name, IReadOnlyList<string> typeArguments)
	{
		return typeArguments.Count == 0 ? name : name + "<" + string.Join(", ", typeArguments) + ">";
	}

	public string RenderConstruction(string type, IReadOnlyList<string> arguments)
	{
		return "new " + type + "(" + string.Join(", ", arguments) + ")";
	}

	public string RenderCall(string target, IReadOnlyList<string> arguments)
	{
		return target + "(" + string.Join(", ", arguments) + ")";
	}

	public string RenderCharacter(string text)
	{
		return text.Length == 0 ? "''" : Model.CharRange.Quote(text[0]);
	}

	public string RenderText(string text)
	{
		return "\"" + text.Replace("\\", @"\\").Replace("\"", "\\\"") + "\"";
	}

	public string GeneratedTypeName(Binding.RuleSymbol rule)
	{
		return DotGram.Grammar.Emit.CSharpEmitter.IdentifierOf(rule);
	}

	/// <summary>The shape of the C# the parser is generated into.</summary>
	public CSharpHostShape Host { get; }

	/// <summary>
	/// The same language — its resolver and its scanner — generating into another host's shape: a
	/// compilation's own, which its options say and which every C# language it is given takes.
	/// </summary>
	internal CSharpLanguage WithHost(CSharpHostShape host)
	{
		return ReferenceEquals(host, Host) ? this : new CSharpLanguage(_given, _scanner, host) { Can = Can };
	}

	/// <summary>The shape of the C# a graph's parser is generated into, read by the emitter: none where its language is not C#'s.</summary>
	internal static CSharpHostShape HostOf(Model.RecognitionGraph graph)
	{
		return (graph.Language as CSharpLanguage)?.Host ?? NoHost;
	}

	static readonly CSharpHostShape NoHost = new();

	public bool LocationsRequested => Host.LocationType is not null;

	public bool LocationsPerCall => Host.LocatedFacade is { Length: > 0 } && Host.LocationType is not null;

	public bool IsLocated(string type, IReadOnlyList<string> imports)
	{
		if (Host.LocationType is not { } named)
			return false;

		// Bare first and then under each import, the way C# itself resolves and the way the
		// questions were asked: a grammar writes `@Statement` beside a `using`, and it is
		// the qualified spelling that a host compilation can answer about.
		if (Fits(type, named))
			return true;

		foreach (var import in imports)
			if (Fits(import + "." + type, named))
				return true;

		return false;
	}

	public string? OptionalElementOf(string spelling)
	{
		if (spelling is null)
			throw new ArgumentNullException(nameof(spelling));

		return spelling.EndsWith("?", StringComparison.Ordinal) ? spelling.Substring(0, spelling.Length - 1) : null;
	}

	public bool CanBeNull(string spelling, IReadOnlyCollection<string> references)
	{
		var type = OptionalElementOf(spelling) ?? spelling;

		return type is "string" or "object" || IsSequence(type) || references.Contains(type);
	}

	public bool IsReferenceAsWritten(string type, string? definition, IReadOnlyList<string> imports)
	{
		return AsWritten(type, definition, imports, IsReferenceType);
	}

	public bool IsOptionalAsWritten(string type, string? definition, IReadOnlyList<string> imports)
	{
		return AsWritten(type, definition, imports, IsOptional);
	}

	/// <summary>
	/// A question about a type the way C# binds its name in the generated file: the first of the bare
	/// spelling and each import's that names a type at all decides. A struct nested in the host is
	/// what a bare <c>Node</c> means there even beside a <c>using</c> of a namespace with a class
	/// <c>Node</c>, and a class of the grammar's own may be called Nullable. A spelling the grammar did
	/// not write, whose existence no question foresaw, is answered by whichever spelling says yes.
	/// </summary>
	bool AsWritten(string type, string? definition, IReadOnlyList<string> imports, Func<string, bool> ask)
	{
		if (definition is null)
		{
			if (ask(type))
				return true;

			foreach (var import in imports)
				if (ask(import + "." + type))
					return true;

			return false;
		}

		if (TypeExists(definition))
			return ask(type);

		foreach (var import in imports)
			if (TypeExists(import + "." + definition))
				return ask(import + "." + type);

		return false;
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

	public bool TakesCharacterView(string type)
	{
		return type.Contains("ReadOnlySpan<char>") || type.Contains("ReadOnlySpan<global::System.Char>") || type.Contains("ReadOnlySpan<System.Char>");
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
