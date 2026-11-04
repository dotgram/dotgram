using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotGram.Language;

public enum DslGrammarSourceKind
{
	Embedded,
	File,
}

public sealed class DslClassificationDefinition(string target, string role, AttributeData? attribute)
{
	public string Target { get; } = target;
	public string Role { get; } = role;
	public AttributeData? Attribute { get; } = attribute;
}

public sealed class DslRecognitionContractDefinition(
	IReadOnlyDictionary<string, bool> guards,
	IReadOnlyDictionary<string, string> externals)
{
	public IReadOnlyDictionary<string, bool> Guards { get; } = guards;
	public IReadOnlyDictionary<string, string> Externals { get; } = externals;

	public static DslRecognitionContractDefinition Empty { get; } = new(
		new Dictionary<string, bool>(StringComparer.Ordinal),
		new Dictionary<string, string>(StringComparer.Ordinal));
}

public sealed class DslIncludedGrammarDefinition(
	string name,
	DslGrammarSourceKind sourceKind,
	string grammarSource,
	string? portableSource = null)
{
	public string Name { get; } = name;
	public DslGrammarSourceKind SourceKind { get; } = sourceKind;
	public string GrammarSource { get; } = grammarSource;

	/// <summary>
	/// What a class in a referenced assembly carries in <c>[GramSource]</c>: the grammar to read
	/// where its <c>.gram</c> file is not in the project, as the generator reads it.
	/// </summary>
	public string? PortableSource { get; } = portableSource;
}

public sealed class DslLanguageDefinition(
	string id,
	INamedTypeSymbol parserType,
	DslGrammarSourceKind sourceKind,
	string grammarSource,
	IReadOnlyList<string> extensions,
	IReadOnlyList<DslClassificationDefinition> classifications,
	IReadOnlyList<DslIncludedGrammarDefinition>? includedGrammars = null,
	int descriptorFormatVersion = 0,
	string? grammarHash = null,
	IReadOnlyDictionary<string, string>? entries = null,
	DslRecognitionContractDefinition? recognitionContract = null)
{
	public string Id { get; } = id;
	public INamedTypeSymbol ParserType { get; } = parserType;
	public DslGrammarSourceKind SourceKind { get; } = sourceKind;

	/// <summary>Embedded grammar text or the referenced <c>.gram</c> file name.</summary>
	public string GrammarSource { get; } = grammarSource;
	public IReadOnlyList<string> Extensions { get; } = extensions;
	public IReadOnlyList<DslClassificationDefinition> Classifications { get; } = classifications;
	public IReadOnlyList<DslIncludedGrammarDefinition> IncludedGrammars { get; } =
		includedGrammars ?? Array.Empty<DslIncludedGrammarDefinition>();
	public int DescriptorFormatVersion { get; } = descriptorFormatVersion;
	public string? GrammarHash { get; } = grammarHash;
	public IReadOnlyDictionary<string, string> Entries { get; } =
		entries ?? new Dictionary<string, string>();
	public DslRecognitionContractDefinition RecognitionContract { get; } =
		recognitionContract ?? DslRecognitionContractDefinition.Empty;
}

public sealed class DslLanguageCatalog(IReadOnlyList<DslLanguageDefinition> languages)
{
	public IReadOnlyList<DslLanguageDefinition> Languages { get; } = languages;

	/// <summary>
	/// What building this catalog cost, for the tests that hold the session caches to it.
	/// </summary>
	internal DslDiscoveryWork Work { get; set; }
}

/// <summary>
/// The work one discovery did rather than took from a cache.
/// </summary>
/// <param name="SourceWalks">Walks of the project's own types: one where its declarations changed, else none.</param>
/// <param name="ReferenceWalks">References whose types were walked: each one once a session.</param>
/// <param name="DescriptorDecodes">Descriptors decoded and hashed: each distinct one once a session.</param>
readonly record struct DslDiscoveryWork(int SourceWalks, int ReferenceWalks, int DescriptorDecodes);

/// <summary>
/// Discovers generated-language declarations from Roslyn symbols without loading the
/// consumer assembly or relying on the spelling used at an attribute site.
/// </summary>
/// <remarks>
/// <para>
/// The editor asks on every pause in typing, and every pause brings a new compilation, so a
/// cache keyed by the compilation alone answers only within one pass. What is kept across
/// passes is what a pass would otherwise walk again: for each reference, which of its types
/// are hosts, once a session; for the project, which of its types are, until a file that
/// declares grammar attributes changes; and every descriptor decoded, by its contents. The
/// definitions themselves are built again on every pass from the new compilation's symbols,
/// which is a handful of attribute reads, so nothing of an old compilation is held.
/// </para>
/// <para>
/// A host in a referenced assembly is read from the descriptor the generator wrote on it
/// and from nothing else. Its <c>[Gram]</c> names a file of the project that built it, and
/// looking for that name among this project's files finds nothing, or finds a file of the
/// same name that is some other grammar.
/// </para>
/// </remarks>
public static class DslLanguageDiscovery
{
	static readonly ConditionalWeakTable<Compilation, DslLanguageCatalog> Cache = new();
	static readonly object CacheGate = new();

	static readonly ConditionalWeakTable<object, ReferenceHosts> ReferenceCache = new();
	static readonly ConditionalWeakTable<SyntaxTree, TreeRelevance> TreeCache = new();
	static readonly Dictionary<string, SourceHosts> SourceCache = new(StringComparer.Ordinal);
	static readonly Dictionary<string, DecodedDescriptor> DescriptorCache = new(StringComparer.Ordinal);

	/// <summary>
	/// How many distinct descriptors are kept decoded. A session sees a few dozen at most; a
	/// cache this full has been fed by something else and starts again.
	/// </summary>
	const int DescriptorCacheLimit = 256;

	const string GramAttribute                 = "DotGram.GramAttribute";
	const string GramIncludeAttribute          = "DotGram.GramIncludeAttribute";
	const string LanguageAttribute             = GramMetadataNames.QualifiedLanguage;
	const string ClassificationAttribute       = GramMetadataNames.QualifiedClassify;
	const string Classification                = "DotGram.GramClassification";
	const string LanguageDescriptorAttribute   = GramMetadataNames.QualifiedLanguageDescriptor;
	const string ToolingGuardAttribute         = GramMetadataNames.QualifiedToolingGuard;
	const string ToolingExternalAttribute      = GramMetadataNames.QualifiedToolingExternal;

	static readonly string[] ClassificationMembers =
	[
		"Keyword", "Identifier", "Type", "Variable", "Function", "Method", "Property",
		"Number", "String", "Comment", "Operator", "Punctuation", "Namespace", "Parameter", "Label",
	];

	public static DslLanguageCatalog Discover(
		Compilation compilation,
		CancellationToken cancellationToken = default)
	{
		if (compilation is null)
			throw new ArgumentNullException(nameof(compilation));

		lock (CacheGate)
			if (Cache.TryGetValue(compilation, out var cached))
				return cached;

		var discovered = DiscoverCore(compilation, cancellationToken);

		lock (CacheGate)
		{
			if (Cache.TryGetValue(compilation, out var cached))
				return cached;

			Cache.Add(compilation, discovered);
			return discovered;
		}
	}

	static DslLanguageCatalog DiscoverCore(
		Compilation compilation,
		CancellationToken cancellationToken)
	{
		var work      = new Work();
		var languages = new List<DslLanguageDefinition>();

		foreach (var host in SourceHostTypes(compilation, work, cancellationToken))
			if (Define(host, compilation.Assembly, work) is { } language)
				languages.Add(language);

		var seen = new HashSet<IAssemblySymbol>(SymbolEqualityComparer.Default) { compilation.Assembly };
		foreach (var reference in compilation.References)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assembly ||
				!seen.Add(assembly))
				continue;

			foreach (var host in ReferenceHostTypes(compilation, reference, assembly, work, cancellationToken))
				if (Define(host, null, work) is { } language)
					languages.Add(language);
		}

		return new DslLanguageCatalog(languages)
		{
			Work = new DslDiscoveryWork(work.SourceWalks, work.ReferenceWalks, work.DescriptorDecodes),
		};
	}

	/// <summary>
	/// One host as a language, read from the symbol in hand.
	/// </summary>
	/// <param name="source">
	/// The compilation's own assembly where the host is in source; null where it is in a
	/// reference, which is then read from its descriptor alone.
	/// </param>
	static DslLanguageDefinition? Define(INamedTypeSymbol type, IAssemblySymbol? source, Work work)
	{
		var attributes        = type.GetAttributes();
		var languageAttribute = attributes.FirstOrDefault(IsLanguageAttribute);
		if (languageAttribute is null ||
			languageAttribute.ConstructorArguments is not [{ Value: string id }] ||
			string.IsNullOrWhiteSpace(id))
			return null;

		var grammarAttribute = source is null ? null : PrimaryGramAttribute(attributes);
		var descriptor       = Descriptor(attributes, work);
		if (descriptor is not null && descriptor.LanguageId != id)
			descriptor = null;
		if (grammarAttribute is null && descriptor is null)
			return null;

		var grammar = grammarAttribute is not null
			? Grammar(type, grammarAttribute)
			: (Kind: DslGrammarSourceKind.Embedded, Source: descriptor!.Source);
		if (grammar is null)
			return null;

		var classifications = attributes
			.Where(IsClassificationAttribute)
			.Select(ClassificationDefinition)
			.Where(static item => item is not null)
			.Cast<DslClassificationDefinition>()
			.ToArray();
		if (classifications.Length == 0 && descriptor is not null)
			classifications = descriptor.Classifications.ToArray();
		var recognitionContract = RecognitionContract(attributes);
		if (recognitionContract.Guards.Count == 0 && recognitionContract.Externals.Count == 0 &&
			descriptor is not null)
			recognitionContract = descriptor.RecognitionContract;

		return new DslLanguageDefinition(
			id,
			type,
			grammar.Value.Kind,
			grammar.Value.Source,
			Extensions(languageAttribute),
			classifications,
			grammarAttribute is null ? [] : IncludedGrammars(type, source!),
			descriptor?.FormatVersion ?? 0,
			descriptor?.GrammarHash,
			descriptor?.Entries,
			recognitionContract);
	}

	/// <summary>
	/// The project's own hosts: walked where a file that declares grammar attributes changed,
	/// and otherwise the ones the last walk found, looked up in the new compilation.
	/// </summary>
	static IReadOnlyList<INamedTypeSymbol> SourceHostTypes(
		Compilation compilation,
		Work work,
		CancellationToken cancellationToken)
	{
		var relevant  = new List<SyntaxTree>();
		var cacheable = true;
		foreach (var tree in compilation.SyntaxTrees)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var relevance = TreeCache.GetValue(tree, Relevance);
			if (relevance.Declares)
				relevant.Add(tree);
			if (relevance.GlobalAlias)
				cacheable = false;
		}

		// One entry per build of a project: a project built for several frameworks is one
		// assembly name with several sets of symbols, and the entries would take turns.
		var key = (compilation.AssemblyName ?? "") + "\u0000" + string.Join(
			";",
			compilation.SyntaxTrees.FirstOrDefault()?.Options.PreprocessorSymbolNames ?? []);

		// What a walk can see besides the trees: before the references are loaded nothing is
		// an attribute or a string, and before the generator has run there is no attribute of
		// the right name, so a walk then finds no host and must not be kept for after.
		var resolves = compilation.GetSpecialType(SpecialType.System_String).TypeKind != TypeKind.Error;
		var declared = compilation.Assembly.GetTypeByMetadataName(LanguageAttribute) is not null;
		if (cacheable)
		{
			SourceHosts? entry;
			lock (CacheGate)
				SourceCache.TryGetValue(key, out entry);

			if (entry is not null && entry.Resolves == resolves && entry.Declared == declared &&
				entry.Matches(relevant) &&
				Resolve(compilation.Assembly, entry.Names) is { } found)
				return found;
		}

		work.SourceWalks++;
		var hosts = Types(compilation.Assembly.GlobalNamespace, cancellationToken)
			.Where(static type => type.GetAttributes().Any(IsLanguageAttribute))
			.ToArray();

		if (cacheable)
		{
			var names = hosts.Select(MetadataName).ToArray();
			lock (CacheGate)
				SourceCache[key] = new SourceHosts(relevant, names, resolves, declared);
		}

		return hosts;
	}

	/// <summary>
	/// A reference's hosts: walked the first time the reference is seen and looked up by name
	/// after that, in whichever compilation asks.
	/// </summary>
	static IReadOnlyList<INamedTypeSymbol> ReferenceHostTypes(
		Compilation compilation,
		MetadataReference reference,
		IAssemblySymbol assembly,
		Work work,
		CancellationToken cancellationToken)
	{
		var key     = ReferenceKey(reference);
		var options = compilation.Options.MetadataImportOptions;
		if (key is not null)
		{
			ReferenceHosts? entry;
			lock (CacheGate)
				ReferenceCache.TryGetValue(key, out entry);

			if (entry is not null && entry.Options == options &&
				Resolve(assembly, entry.Names) is { } found)
				return found;
		}

		work.ReferenceWalks++;
		var hosts = HasDescriptorMarker(assembly)
			? Types(assembly.GlobalNamespace, cancellationToken)
				.Where(static type => type.GetAttributes().Any(IsLanguageAttribute))
				.ToArray()
			: [];

		if (key is not null)
		{
			var names = hosts.Select(MetadataName).ToArray();
			lock (CacheGate)
			{
				ReferenceCache.Remove(key);
				ReferenceCache.Add(key, new ReferenceHosts(options, names));
			}
		}

		return hosts;
	}

	/// <summary>
	/// What stays the same while a reference does: the metadata a file reference was read
	/// into, or the compilation a project reference stands for. Null where neither is in
	/// reach, and such a reference is walked every time.
	/// </summary>
	static object? ReferenceKey(MetadataReference reference)
	{
		switch (reference)
		{
			case CompilationReference compilationReference:
				return compilationReference.Compilation;

			case PortableExecutableReference portable:
				try
				{
					return portable.GetMetadataId();
				}
				catch (Exception exception) when (exception is IOException or BadImageFormatException or UnauthorizedAccessException)
				{
					return null;
				}

			default:
				return null;
		}
	}

	/// <summary>
	/// The types named, from <paramref name="assembly"/>; null where one of them is no longer
	/// there, which sends the caller back to walking.
	/// </summary>
	static INamedTypeSymbol[]? Resolve(IAssemblySymbol assembly, string[] names)
	{
		var types = new INamedTypeSymbol[names.Length];
		for (var index = 0; index < names.Length; index++)
		{
			if (assembly.GetTypeByMetadataName(names[index]) is not { } type)
				return null;

			types[index] = type;
		}

		return types;
	}

	static string MetadataName(INamedTypeSymbol type)
	{
		var name = type.MetadataName;
		for (var outer = type.ContainingType; outer is not null; outer = outer.ContainingType)
			name = outer.MetadataName + "+" + name;
		for (var space = type.ContainingNamespace; space is { IsGlobalNamespace: false }; space = space.ContainingNamespace)
			name = space.MetadataName + "." + name;

		return name;
	}

	/// <summary>
	/// Whether a file can change which of the project's types are hosts or what they say: it
	/// declares a type or applies an attribute whose name starts with <c>Gram</c>, or names one
	/// through an alias. Read from the declarations only, never from a member's body, and once
	/// per tree. An edit makes a new tree, so an edit to a file that declares any of them,
	/// before or after, is a walk wherever in the file it is, and an edit to any other is not.
	/// </summary>
	/// <remarks>
	/// A global alias can rename a grammar attribute for every other file, which no reading of
	/// one file can see; a project that has one is walked on every pass.
	/// </remarks>
	static TreeRelevance Relevance(SyntaxTree tree)
	{
		var declares    = false;
		var globalAlias = false;
		var pending     = new Stack<SyntaxNode>();
		pending.Push(tree.GetRoot());

		while (pending.Count > 0)
		{
			var node = pending.Pop();

			if (node is UsingDirectiveSyntax { Alias: not null } alias)
			{
				if (alias.NamespaceOrType.ToString().IndexOf("Gram", StringComparison.Ordinal) >= 0)
				{
					declares     = true;
					globalAlias |= alias.GlobalKeyword.IsKind(SyntaxKind.GlobalKeyword);
				}

				continue;
			}

			if (node is AttributeSyntax attribute)
			{
				declares |= SimpleName(attribute.Name).StartsWith("Gram", StringComparison.Ordinal);
				continue;
			}

			if (node is BaseTypeDeclarationSyntax type)
				declares |= type.Identifier.ValueText.StartsWith("Gram", StringComparison.Ordinal);

			if (node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax or BaseTypeDeclarationSyntax or AttributeListSyntax)
				foreach (var child in node.ChildNodes())
					pending.Push(child);
		}

		return new TreeRelevance(declares, globalAlias);
	}

	static string SimpleName(NameSyntax name)
	{
		return name switch
		{
			QualifiedNameSyntax qualified    => qualified.Right.Identifier.ValueText,
			AliasQualifiedNameSyntax aliased => aliased.Name.Identifier.ValueText,
			SimpleNameSyntax simple          => simple.Identifier.ValueText,
			_                                => "",
		};
	}

	sealed class Work
	{
		public int SourceWalks;
		public int ReferenceWalks;
		public int DescriptorDecodes;
	}

	sealed class TreeRelevance(bool declares, bool globalAlias)
	{
		public bool Declares    { get; } = declares;
		public bool GlobalAlias { get; } = globalAlias;
	}

	/// <summary>
	/// The hosts one walk of a project found, and what it saw. The trees are held weakly: an
	/// entry outlives its project when the solution closes, and must not keep its files alive.
	/// </summary>
	sealed class SourceHosts(IReadOnlyList<SyntaxTree> trees, string[] names, bool resolves, bool declared)
	{
		readonly WeakReference<SyntaxTree>[] _trees = trees.Select(static tree => new WeakReference<SyntaxTree>(tree)).ToArray();

		public string[] Names    { get; } = names;
		public bool     Resolves { get; } = resolves;
		public bool     Declared { get; } = declared;

		/// <summary>
		/// Whether <paramref name="relevant"/> are the very trees the walk read, in order.
		/// </summary>
		public bool Matches(IReadOnlyList<SyntaxTree> relevant)
		{
			if (_trees.Length != relevant.Count)
				return false;

			for (var index = 0; index < _trees.Length; index++)
				if (!_trees[index].TryGetTarget(out var tree) || !ReferenceEquals(tree, relevant[index]))
					return false;

			return true;
		}
	}

	sealed class ReferenceHosts(MetadataImportOptions options, string[] names)
	{
		public MetadataImportOptions Options { get; } = options;
		public string[]              Names   { get; } = names;
	}

	sealed class DecodedDescriptor(object?[] arguments, GeneratedDescriptor? descriptor)
	{
		public object?[]            Arguments  { get; } = arguments;
		public GeneratedDescriptor? Descriptor { get; } = descriptor;
	}

	sealed class GeneratedDescriptor(
		int formatVersion,
		string languageId,
		string grammarHash,
		string source,
		IReadOnlyDictionary<string, string> entries,
		IReadOnlyList<DslClassificationDefinition> classifications,
		DslRecognitionContractDefinition recognitionContract)
	{
		public int FormatVersion { get; } = formatVersion;
		public string LanguageId { get; } = languageId;
		public string GrammarHash { get; } = grammarHash;
		public string Source { get; } = source;
		public IReadOnlyDictionary<string, string> Entries { get; } = entries;
		public IReadOnlyList<DslClassificationDefinition> Classifications { get; } = classifications;
		public DslRecognitionContractDefinition RecognitionContract { get; } = recognitionContract;
	}

	/// <summary>
	/// The descriptor among a host's attributes, decoded once for every distinct one: a
	/// reference's hosts are read on every pass, and their payloads are what costs.
	/// </summary>
	static GeneratedDescriptor? Descriptor(ImmutableArray<AttributeData> attributes, Work work)
	{
		var attribute = attributes.FirstOrDefault(IsDescriptorAttribute);
		if (attribute?.ConstructorArguments is not { Length: >= 3 } arguments ||
			arguments[2].Value is not string key)
			return null;

		var values = arguments.Select(static argument => argument.Value).ToArray();
		lock (CacheGate)
			if (DescriptorCache.TryGetValue(key, out var cached) && cached.Arguments.SequenceEqual(values))
				return cached.Descriptor;

		work.DescriptorDecodes++;
		var decoded = Decode(arguments);

		lock (CacheGate)
		{
			if (DescriptorCache.Count >= DescriptorCacheLimit)
				DescriptorCache.Clear();

			DescriptorCache[key] = new DecodedDescriptor(values, decoded);
		}

		return decoded;
	}

	static GeneratedDescriptor? Decode(ImmutableArray<TypedConstant> arguments)
	{
		if (arguments.Length is not (5 or 6 or 7) ||
			arguments[0].Value is not int formatVersion ||
			formatVersion != arguments.Length - 4 ||
			arguments[1].Value is not string languageId ||
			arguments[2].Value is not string hash ||
			arguments[3].Value is not string sourcePayload ||
			arguments[4].Value is not string entriesPayload)
			return null;

		if (!TryBase64(sourcePayload, out var source) ||
			!TryBase64(entriesPayload, out var entryText) ||
			!string.Equals(Hash(source), hash, StringComparison.Ordinal))
			return null;

		var entries = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (var line in entryText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
		{
			var fields = line.Split('\t');
			if (fields.Length == 3)
				entries[fields[0]] = fields[2];
		}

		var classifications = new List<DslClassificationDefinition>();
		if (formatVersion >= 2)
		{
			if (arguments[5].Value is not string classificationsPayload ||
				!TryBase64(classificationsPayload, out var classificationText))
				return null;

			foreach (var line in classificationText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
			{
				var fields = line.Split('\t');
				if (fields.Length == 2 && fields[0].Length > 0 && ClassificationMembers.Contains(fields[1]))
					classifications.Add(new DslClassificationDefinition(fields[0], fields[1], null));
			}
		}

		var recognitionContract = DslRecognitionContractDefinition.Empty;
		if (formatVersion == 3)
		{
			if (arguments[6].Value is not string contractPayload ||
				!TryBase64(contractPayload, out var contractText) ||
				!TryRecognitionContract(contractText, out recognitionContract))
				return null;
		}

		return new GeneratedDescriptor(
			formatVersion,
			languageId,
			hash,
			source,
			entries,
			classifications,
			recognitionContract);
	}

	static DslRecognitionContractDefinition RecognitionContract(ImmutableArray<AttributeData> attributes)
	{
		var lines = new List<string>();
		foreach (var attribute in attributes)
		{
			if (IsAttributeType(attribute.AttributeClass, ToolingGuardAttribute) &&
				attribute.ConstructorArguments is [{ Value: string expression }, { Value: bool accepted }])
				lines.Add("G\t" + expression + "\t" + (accepted ? "1" : "0"));
			else if (IsAttributeType(attribute.AttributeClass, ToolingExternalAttribute) &&
				attribute.ConstructorArguments is [{ Value: string method }, { Value: string rule }])
				lines.Add("E\t" + method + "\t" + rule);
		}

		return TryRecognitionContract(string.Join("\n", lines), out var contract)
			? contract
			: DslRecognitionContractDefinition.Empty;
	}

	static bool TryRecognitionContract(string text, out DslRecognitionContractDefinition contract)
	{
		var guards = new Dictionary<string, bool>(StringComparer.Ordinal);
		var externals = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (var line in text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
		{
			var fields = line.Split('\t');
			if (fields.Length != 3 || fields[1].Length == 0 || fields[2].Length == 0)
			{
				contract = DslRecognitionContractDefinition.Empty;
				return false;
			}

			if (fields[0] == "G" && fields[2] is "0" or "1")
				guards[fields[1]] = fields[2] == "1";
			else if (fields[0] == "E")
				externals[fields[1]] = fields[2];
			else
			{
				contract = DslRecognitionContractDefinition.Empty;
				return false;
			}
		}

		contract = new DslRecognitionContractDefinition(guards, externals);
		return true;
	}

	static bool TryBase64(string payload, out string value)
	{
		try
		{
			value = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
			return true;
		}
		catch (FormatException)
		{
			value = "";
			return false;
		}
	}

	static string Hash(string value)
	{
		using var sha = SHA256.Create();
		var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
		var text = new StringBuilder(bytes.Length * 2);

		foreach (var item in bytes)
			text.Append(item.ToString("x2"));

		return text.ToString();
	}

	static bool HasDescriptorMarker(IAssemblySymbol assembly)
	{
		return assembly.GlobalNamespace.GetNamespaceMembers()
			.FirstOrDefault(static item => item.Name == GramMetadataNames.Namespace)?
			.GetTypeMembers(GramMetadataNames.LanguageDescriptor).Length > 0;
	}

	static (DslGrammarSourceKind Kind, string Source)? Grammar(
		INamedTypeSymbol parserType,
		AttributeData attribute)
	{
		if (attribute.ConstructorArguments.Length == 0)
			return (DslGrammarSourceKind.File, parserType.Name + ".gram");

		if (attribute.ConstructorArguments is not [{ Value: string source }])
			return null;

		return source.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) &&
			source.IndexOf('\r') < 0 && source.IndexOf('\n') < 0
			? (DslGrammarSourceKind.File, source)
			: (DslGrammarSourceKind.Embedded, source);
	}

	/// <param name="source">
	/// The compilation's own assembly. A class from anywhere else carries its grammar in
	/// <c>[GramSource]</c>, which is read where its file is not in the project, as the generator
	/// reads it.
	/// </param>
	static IReadOnlyList<DslIncludedGrammarDefinition> IncludedGrammars(
		INamedTypeSymbol parserType,
		IAssemblySymbol source)
	{
		var included = new List<DslIncludedGrammarDefinition>();
		var seen = new HashSet<string>(StringComparer.Ordinal) { parserType.ToDisplayString() };
		var pending = new Queue<(INamedTypeSymbol Type, string? As)>(NamedIncludes(parserType));

		while (pending.Count > 0)
		{
			var (current, called) = pending.Dequeue();
			if (!seen.Add(current.ToDisplayString()))
				continue;

			foreach (var nested in NamedIncludes(current))
				pending.Enqueue(nested);

			var attribute = PrimaryGramAttribute(current.GetAttributes());
			if (attribute is null || Grammar(current, attribute) is not { } grammar)
				continue;

			var name = called ?? attribute.NamedArguments
				.FirstOrDefault(static argument => argument.Key == "IncludedAs")
				.Value.Value as string ?? current.Name;
			if (!IsIdentifier(name))
				continue;

			var portable = SymbolEqualityComparer.Default.Equals(current.ContainingAssembly, source)
				? null
				: current.GetAttributes()
					.FirstOrDefault(static candidate =>
						candidate.AttributeClass?.ToDisplayString() == GramIncludes.GramSourceAttribute)?
					.ConstructorArguments.FirstOrDefault().Value as string;
			included.Add(new DslIncludedGrammarDefinition(name, grammar.Kind, grammar.Source, portable));
		}

		return included;
	}

	static IEnumerable<(INamedTypeSymbol Type, string? As)> NamedIncludes(INamedTypeSymbol type)
	{
		foreach (var attribute in type.GetAttributes())
			if (attribute.AttributeClass?.ToDisplayString() == GramIncludeAttribute &&
				attribute.ConstructorArguments is [{ Value: INamedTypeSymbol grammar }])
			{
				yield return (
					grammar,
					attribute.NamedArguments
						.FirstOrDefault(static argument => argument.Key == "As")
						.Value.Value as string);
			}
	}

	static bool IsIdentifier(string value)
	{
		return value.Length > 0 &&
		(value[0] == '_' || char.IsLetter(value[0])) &&
		value.Skip(1).All(static character => character == '_' || char.IsLetterOrDigit(character));
	}

	static IReadOnlyList<string> Extensions(AttributeData attribute)
	{
		var value = attribute.NamedArguments
			.FirstOrDefault(static argument => argument.Key == "Extensions").Value;
		if (value.Kind != TypedConstantKind.Array || value.IsNull)
			return Array.Empty<string>();

		return value.Values
			.Where(static item => item.Value is string)
			.Select(static item => (string)item.Value!)
			.ToArray();
	}

	static DslClassificationDefinition? ClassificationDefinition(AttributeData attribute)
	{
		if (attribute.ConstructorArguments is not
			[
				{ Value: string target },
				{ Kind: TypedConstantKind.Enum, Value: not null } role,
			])
			return null;

		var field = role.Type?.GetMembers().OfType<IFieldSymbol>().FirstOrDefault(candidate =>
			candidate.HasConstantValue && Equals(candidate.ConstantValue, role.Value));
		return field is null ? null : new DslClassificationDefinition(target, field.Name, attribute);
	}

	static bool IsGramAttribute(AttributeData attribute)
	{
		return IsAttributeType(attribute.AttributeClass, GramAttribute) &&
		attribute.AttributeConstructor?.Parameters.Length is 0 or 1 &&
		(attribute.AttributeConstructor.Parameters.Length == 0 ||
			IsString(attribute.AttributeConstructor.Parameters[0].Type)) &&
		HasProperty(attribute.AttributeClass!, "Source", SpecialType.System_String, writable: false) &&
		HasProperty(attribute.AttributeClass!, "IncludedAs", SpecialType.System_String, writable: true);
	}

	static AttributeData? PrimaryGramAttribute(IEnumerable<AttributeData> attributes)
	{
		return attributes.FirstOrDefault(IsGramAttribute);
	}

	static bool IsLanguageAttribute(AttributeData attribute)
	{
		return IsAttributeType(attribute.AttributeClass, LanguageAttribute) &&
		HasConstructor(attribute.AttributeClass!, SpecialType.System_String) &&
		HasProperty(attribute.AttributeClass!, "Id", SpecialType.System_String, writable: false) &&
		HasStringArrayProperty(attribute.AttributeClass!, "Extensions", writable: true);
	}

	static bool IsClassificationAttribute(AttributeData attribute)
	{
		return IsAttributeType(attribute.AttributeClass, ClassificationAttribute) &&
		attribute.AttributeClass is { } type &&
		type.InstanceConstructors.Any(constructor =>
			constructor.Parameters is [{ Type.SpecialType: SpecialType.System_String }, { Type: INamedTypeSymbol role }] &&
			IsClassification(role)) &&
		HasProperty(type, "Target", SpecialType.System_String, writable: false) &&
		type.GetMembers("Role").OfType<IPropertySymbol>().Any(property =>
			!property.IsStatic && IsClassification(property.Type));
	}

	static bool IsDescriptorAttribute(AttributeData attribute)
	{
		return IsAttributeType(attribute.AttributeClass, LanguageDescriptorAttribute) &&
		attribute.AttributeConstructor?.Parameters is { } parameters &&
		parameters.Length is 5 or 6 or 7 &&
		parameters[0].Type.SpecialType == SpecialType.System_Int32 &&
		parameters.Skip(1).All(static parameter =>
			parameter.Type.SpecialType == SpecialType.System_String);
	}

	static bool IsClassification(ITypeSymbol type)
	{
		return type.TypeKind == TypeKind.Enum &&
		type.ToDisplayString() == Classification &&
		ClassificationMembers.All(name => type.GetMembers(name).OfType<IFieldSymbol>().Any(static field => field.HasConstantValue));
	}

	static bool IsAttributeType(INamedTypeSymbol? type, string metadataName)
	{
		return type?.ToDisplayString() == metadataName && IsAttribute(type);
	}

	static bool IsAttribute(INamedTypeSymbol type)
	{
		for (var current = type.BaseType; current is not null; current = current.BaseType)
			if (current.ToDisplayString() == "System.Attribute")
				return true;

		return false;
	}

	static bool HasConstructor(INamedTypeSymbol type, SpecialType parameter)
	{
		return type.InstanceConstructors.Any(constructor =>
			constructor.Parameters is [{ Type.SpecialType: var specialType }] && specialType == parameter);
	}

	static bool HasProperty(
		INamedTypeSymbol type,
		string name,
		SpecialType propertyType,
		bool writable)
	{
		return type.GetMembers(name).OfType<IPropertySymbol>().Any(property =>
			!property.IsStatic && property.Type.SpecialType == propertyType &&
			property.GetMethod is not null && (!writable || property.SetMethod is not null));
	}

	static bool HasProperty(
		INamedTypeSymbol type,
		string name,
		string propertyType,
		bool writable)
	{
		return type.GetMembers(name).OfType<IPropertySymbol>().Any(property =>
			!property.IsStatic && property.Type.ToDisplayString() == propertyType &&
			property.GetMethod is not null && (!writable || property.SetMethod is not null));
	}

	static bool HasStringArrayProperty(INamedTypeSymbol type, string name, bool writable)
	{
		return type.GetMembers(name).OfType<IPropertySymbol>().Any(property =>
			!property.IsStatic && property.Type is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_String } &&
			property.GetMethod is not null && (!writable || property.SetMethod is not null));
	}

	static bool IsString(ITypeSymbol type)
	{
		return type.SpecialType == SpecialType.System_String;
	}

	static IEnumerable<INamedTypeSymbol> Types(
		INamespaceSymbol @namespace,
		CancellationToken cancellationToken)
	{
		foreach (var type in @namespace.GetTypeMembers())
		foreach (var candidate in TypeAndNested(type, cancellationToken))
			yield return candidate;

		foreach (var child in @namespace.GetNamespaceMembers())
		foreach (var candidate in Types(child, cancellationToken))
			yield return candidate;
	}

	static IEnumerable<INamedTypeSymbol> TypeAndNested(
		INamedTypeSymbol type,
		CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		yield return type;

		foreach (var nested in type.GetTypeMembers())
		foreach (var candidate in TypeAndNested(nested, cancellationToken))
			yield return candidate;
	}
}
