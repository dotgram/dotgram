using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using Microsoft.CodeAnalysis;

namespace DotGram.VisualStudio;

public enum DslGrammarSourceKind
{
	Embedded,
	File,
}

public sealed class DslClassificationDefinition(string target, string role, AttributeData attribute)
{
	public string Target { get; } = target;
	public string Role { get; } = role;
	public AttributeData Attribute { get; } = attribute;
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
	IReadOnlyList<DslIncludedGrammarDefinition>? includedGrammars = null)
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
}

public sealed class DslAttributeCarrier(
	INamedTypeSymbol attributeType,
	DslLanguageDefinition language)
{
	public INamedTypeSymbol AttributeType { get; } = attributeType;
	public DslLanguageDefinition Language { get; } = language;
}

public sealed class DslLanguageCatalog(
	IReadOnlyList<DslLanguageDefinition> languages,
	IReadOnlyList<DslAttributeCarrier> attributeCarriers)
{
	public IReadOnlyList<DslLanguageDefinition> Languages { get; } = languages;
	public IReadOnlyList<DslAttributeCarrier> AttributeCarriers { get; } = attributeCarriers;
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
	const string LanguageMarkerAttribute       = "DotGram.GramLanguageMarkerAttribute";

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

		var types = Types(compilation.Assembly.GlobalNamespace, cancellationToken).ToArray();
		var languages = new List<DslLanguageDefinition>();

		foreach (var type in types)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var languageAttribute = type.GetAttributes().FirstOrDefault(IsLanguageAttribute);
			var grammarAttribute  = type.GetAttributes().FirstOrDefault(IsGramAttribute);
			if (languageAttribute is null || grammarAttribute is null ||
				languageAttribute.ConstructorArguments is not [{ Value: string id }] ||
				string.IsNullOrWhiteSpace(id))
				continue;

			var grammar = Grammar(type, grammarAttribute);
			if (grammar is null)
				continue;

			var classifications = type.GetAttributes()
				.Where(IsClassificationAttribute)
				.Select(ClassificationDefinition)
				.Where(static item => item is not null)
				.Cast<DslClassificationDefinition>()
				.ToArray();

			languages.Add(new DslLanguageDefinition(
				id,
				type,
				grammar.Value.Kind,
				grammar.Value.Source,
				Extensions(languageAttribute),
				classifications,
				IncludedGrammars(type)));
		}

		var carriers = new List<DslAttributeCarrier>();
		foreach (var language in languages)
		foreach (var marker in language.ParserType.GetAttributes().Where(IsLanguageMarkerAttribute))
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (marker.ConstructorArguments is not [{ Value: INamedTypeSymbol markerType }] ||
				!IsAttribute(markerType))
				continue;

			carriers.Add(new DslAttributeCarrier(markerType, language));
		}

		return new DslLanguageCatalog(languages, carriers);
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

	static bool IsLanguageMarkerAttribute(AttributeData attribute) =>
		IsAttributeType(attribute.AttributeClass, LanguageMarkerAttribute) &&
		attribute.AttributeClass is { } type &&
		HasConstructor(type, "System.Type") &&
		HasProperty(type, "Marker", "System.Type", writable: false);

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

	static bool HasConstructor(INamedTypeSymbol type, string parameter) =>
		type.InstanceConstructors.Any(constructor =>
			constructor.Parameters is [{ Type: var parameterType }] && parameterType.ToDisplayString() == parameter);

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
