using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

using Microsoft.CodeAnalysis;

namespace DotGram.VisualStudio;

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
	string grammarSource)
{
	public string Name { get; } = name;
	public DslGrammarSourceKind SourceKind { get; } = sourceKind;
	public string GrammarSource { get; } = grammarSource;
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
}

/// <summary>
/// Discovers generated-language declarations from Roslyn symbols without loading the
/// consumer assembly or relying on the spelling used at an attribute site.
/// </summary>
public static class DslLanguageDiscovery
{
	static readonly ConditionalWeakTable<Compilation, DslLanguageCatalog> Cache = new();
	static readonly object CacheGate = new();

	const string GramAttribute                 = "DotGram.GramAttribute";
	const string LanguageAttribute             = "DotGram.GramLanguageAttribute";
	const string ClassificationAttribute       = "DotGram.GramClassifyAttribute";
	const string Classification                = "DotGram.GramClassification";
	const string LanguageDescriptorAttribute   = "DotGram.GramLanguageDescriptorAttribute";
	const string ToolingGuardAttribute         = "DotGram.GramToolingGuardAttribute";
	const string ToolingExternalAttribute      = "DotGram.GramToolingExternalAttribute";

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

		var assemblies = new List<IAssemblySymbol> { compilation.Assembly };
		assemblies.AddRange(compilation.References
			.Select(compilation.GetAssemblyOrModuleSymbol)
			.OfType<IAssemblySymbol>()
			.Where(HasDescriptorMarker));
		var types = assemblies
			.SelectMany(assembly => Types(assembly.GlobalNamespace, cancellationToken))
			.ToArray();
		var languages = new List<DslLanguageDefinition>();

		foreach (var type in types)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var languageAttribute = type.GetAttributes().FirstOrDefault(IsLanguageAttribute);
			var grammarAttribute  = type.GetAttributes().FirstOrDefault(IsGramAttribute);
			var descriptor = Descriptor(type);
			if (languageAttribute is null || grammarAttribute is null && descriptor is null ||
				languageAttribute.ConstructorArguments is not [{ Value: string id }] ||
				string.IsNullOrWhiteSpace(id))
				continue;
			if (descriptor is not null && descriptor.LanguageId != id)
				descriptor = null;
			if (grammarAttribute is null && descriptor is null)
				continue;

			var grammar = grammarAttribute is not null
				? Grammar(type, grammarAttribute)
				: descriptor is { } generated
					? (DslGrammarSourceKind.Embedded, generated.Source)
					: null;
			if (grammar is null)
				continue;

			var classifications = type.GetAttributes()
				.Where(IsClassificationAttribute)
				.Select(ClassificationDefinition)
				.Where(static item => item is not null)
				.Cast<DslClassificationDefinition>()
				.ToArray();
			if (classifications.Length == 0 && descriptor is not null)
				classifications = descriptor.Classifications.ToArray();
			var recognitionContract = RecognitionContract(type);
			if (recognitionContract.Guards.Count == 0 && recognitionContract.Externals.Count == 0 &&
				descriptor is not null)
				recognitionContract = descriptor.RecognitionContract;

			languages.Add(new DslLanguageDefinition(
				id,
				type,
				grammar.Value.Kind,
				grammar.Value.Source,
				Extensions(languageAttribute),
				classifications,
				grammarAttribute is null ? [] : IncludedGrammars(type),
				descriptor?.FormatVersion ?? 0,
				descriptor?.GrammarHash,
				descriptor?.Entries,
				recognitionContract));
		}

		return new DslLanguageCatalog(languages);
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

	static GeneratedDescriptor? Descriptor(INamedTypeSymbol type)
	{
		var attribute = type.GetAttributes().FirstOrDefault(IsDescriptorAttribute);
		if (attribute?.ConstructorArguments is not { } arguments ||
			arguments.Length is not (5 or 6 or 7) ||
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

	static DslRecognitionContractDefinition RecognitionContract(INamedTypeSymbol type)
	{
		var lines = new List<string>();
		foreach (var attribute in type.GetAttributes())
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

	static bool HasDescriptorMarker(IAssemblySymbol assembly) =>
		assembly.GlobalNamespace.GetNamespaceMembers()
			.FirstOrDefault(static item => item.Name == "DotGram")?
			.GetTypeMembers("GramLanguageDescriptorAttribute").Length > 0;

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

	static IReadOnlyList<DslIncludedGrammarDefinition> IncludedGrammars(INamedTypeSymbol parserType)
	{
		var included = new List<DslIncludedGrammarDefinition>();

		for (var current = parserType.BaseType; current is not null; current = current.BaseType)
		{
			var attribute = current.GetAttributes().FirstOrDefault(IsGramAttribute);
			if (attribute is null || Grammar(current, attribute) is not { } grammar)
				continue;

			var name = attribute.NamedArguments
				.FirstOrDefault(static argument => argument.Key == "IncludedAs")
				.Value.Value as string ?? current.Name;
			if (!IsIdentifier(name))
				continue;

			included.Add(new DslIncludedGrammarDefinition(name, grammar.Kind, grammar.Source));
		}

		return included;
	}

	static bool IsIdentifier(string value) =>
		value.Length > 0 &&
		(value[0] == '_' || char.IsLetter(value[0])) &&
		value.Skip(1).All(static character => character == '_' || char.IsLetterOrDigit(character));

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

	static bool IsGramAttribute(AttributeData attribute) =>
		IsAttributeType(attribute.AttributeClass, GramAttribute) &&
		attribute.AttributeConstructor?.Parameters.Length is 0 or 1 &&
		(attribute.AttributeConstructor.Parameters.Length == 0 ||
			IsString(attribute.AttributeConstructor.Parameters[0].Type)) &&
		HasProperty(attribute.AttributeClass!, "Source", SpecialType.System_String, writable: false) &&
		HasProperty(attribute.AttributeClass!, "IncludedAs", SpecialType.System_String, writable: true);

	static bool IsLanguageAttribute(AttributeData attribute) =>
		IsAttributeType(attribute.AttributeClass, LanguageAttribute) &&
		HasConstructor(attribute.AttributeClass!, SpecialType.System_String) &&
		HasProperty(attribute.AttributeClass!, "Id", SpecialType.System_String, writable: false) &&
		HasStringArrayProperty(attribute.AttributeClass!, "Extensions", writable: true);

	static bool IsClassificationAttribute(AttributeData attribute) =>
		IsAttributeType(attribute.AttributeClass, ClassificationAttribute) &&
		attribute.AttributeClass is { } type &&
		type.InstanceConstructors.Any(constructor =>
			constructor.Parameters is [{ Type.SpecialType: SpecialType.System_String }, { Type: INamedTypeSymbol role }] &&
			IsClassification(role)) &&
		HasProperty(type, "Target", SpecialType.System_String, writable: false) &&
		type.GetMembers("Role").OfType<IPropertySymbol>().Any(property =>
			!property.IsStatic && IsClassification(property.Type));

	static bool IsDescriptorAttribute(AttributeData attribute) =>
		IsAttributeType(attribute.AttributeClass, LanguageDescriptorAttribute) &&
		attribute.AttributeConstructor?.Parameters is { } parameters &&
		parameters.Length is 5 or 6 or 7 &&
		parameters[0].Type.SpecialType == SpecialType.System_Int32 &&
		parameters.Skip(1).All(static parameter =>
			parameter.Type.SpecialType == SpecialType.System_String);

	static bool IsClassification(ITypeSymbol type) =>
		type.TypeKind == TypeKind.Enum &&
		type.ToDisplayString() == Classification &&
		ClassificationMembers.All(name => type.GetMembers(name).OfType<IFieldSymbol>().Any(static field => field.HasConstantValue));

	static bool IsAttributeType(INamedTypeSymbol? type, string metadataName) =>
		type?.ToDisplayString() == metadataName && IsAttribute(type);

	static bool IsAttribute(INamedTypeSymbol type)
	{
		for (var current = type.BaseType; current is not null; current = current.BaseType)
			if (current.ToDisplayString() == "System.Attribute")
				return true;

		return false;
	}

	static bool HasConstructor(INamedTypeSymbol type, SpecialType parameter) =>
		type.InstanceConstructors.Any(constructor =>
			constructor.Parameters is [{ Type.SpecialType: var specialType }] && specialType == parameter);

	static bool HasProperty(
		INamedTypeSymbol type,
		string name,
		SpecialType propertyType,
		bool writable) =>
		type.GetMembers(name).OfType<IPropertySymbol>().Any(property =>
			!property.IsStatic && property.Type.SpecialType == propertyType &&
			property.GetMethod is not null && (!writable || property.SetMethod is not null));

	static bool HasProperty(
		INamedTypeSymbol type,
		string name,
		string propertyType,
		bool writable) =>
		type.GetMembers(name).OfType<IPropertySymbol>().Any(property =>
			!property.IsStatic && property.Type.ToDisplayString() == propertyType &&
			property.GetMethod is not null && (!writable || property.SetMethod is not null));

	static bool HasStringArrayProperty(INamedTypeSymbol type, string name, bool writable) =>
		type.GetMembers(name).OfType<IPropertySymbol>().Any(property =>
			!property.IsStatic && property.Type is IArrayTypeSymbol { ElementType.SpecialType: SpecialType.System_String } &&
			property.GetMethod is not null && (!writable || property.SetMethod is not null));

	static bool IsString(ITypeSymbol type) => type.SpecialType == SpecialType.System_String;

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
