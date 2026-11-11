using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// The attributes and enums every project that references the generator is given, held to a list
/// written here: what a consumer can write on a class is the generator's public surface, and it is
/// emitted as text, where no API baseline of an assembly can see it.
/// </summary>
/// <remarks>
/// A type, a base, a constructor, an option, its type or its default added, renamed or removed fails
/// this until the list below says so too — the change then shows up in a diff on purpose rather than
/// by accident.
/// </remarks>
public sealed class GeneratedAttributeSurfaceTests
{
	const string Expected = """
		internal sealed class GramAttribute : DotGram.GramReadingAttribute
		    [System.AttributeUsageAttribute(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
		    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
		    [Microsoft.CodeAnalysis.EmbeddedAttribute]
		    ctor()
		    ctor(string source)
		    string Source { get; }
		    string DefaultAlias { get; set; }
		internal enum GramCarrier
		    [Microsoft.CodeAnalysis.EmbeddedAttribute]
		    Auto = 0
		    Deferred = 1
		    Immediate = 2
		internal enum GramClassification
		    [Microsoft.CodeAnalysis.EmbeddedAttribute]
		    Keyword = 0
		    Identifier = 1
		    Type = 2
		    Variable = 3
		    Function = 4
		    Method = 5
		    Property = 6
		    Number = 7
		    String = 8
		    Comment = 9
		    Operator = 10
		    Punctuation = 11
		    Namespace = 12
		    Parameter = 13
		    Label = 14
		internal sealed class GramClassifyAttribute : System.Attribute
		    [System.AttributeUsageAttribute(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
		    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
		    [Microsoft.CodeAnalysis.EmbeddedAttribute]
		    ctor(string target, DotGram.GramClassification role)
		    string Target { get; }
		    DotGram.GramClassification Role { get; }
		internal sealed class GramIncludeAttribute : System.Attribute
		    [System.AttributeUsageAttribute(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
		    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
		    [Microsoft.CodeAnalysis.EmbeddedAttribute]
		    ctor(System.Type grammar)
		    System.Type Grammar { get; }
		    string Alias { get; set; }
		internal sealed class GramLanguageAttribute : System.Attribute
		    [System.AttributeUsageAttribute(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
		    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
		    [Microsoft.CodeAnalysis.EmbeddedAttribute]
		    ctor(string id)
		    string Id { get; }
		    string[] Extensions { get; set; }
		internal sealed class GramLanguageDescriptorAttribute : System.Attribute
		    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
		    [System.AttributeUsageAttribute(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
		    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
		    [Microsoft.CodeAnalysis.EmbeddedAttribute]
		    ctor(int formatVersion, string languageId, string grammarHash, string sourcePayload, string entriesPayload)
		    ctor(int formatVersion, string languageId, string grammarHash, string sourcePayload, string entriesPayload, string classificationsPayload)
		    ctor(int formatVersion, string languageId, string grammarHash, string sourcePayload, string entriesPayload, string classificationsPayload, string recognitionContractPayload)
		    int FormatVersion { get; }
		    string LanguageId { get; }
		    string GrammarHash { get; }
		    string SourcePayload { get; }
		    string EntriesPayload { get; }
		    string ClassificationsPayload { get; }
		    string RecognitionContractPayload { get; }
		internal abstract class GramReadingAttribute : System.Attribute
		    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
		    [Microsoft.CodeAnalysis.EmbeddedAttribute]
		    string Name { get; set; }
		    int TargetMethodPartSize { get; set; }
		    bool Lexical { get; set; }
		    DotGram.GramCarrier Carrier { get; set; }
		    bool ConstructionsMayThrow { get; set; }
		    bool BufferedText { get; set; }
		    bool BufferedBytes { get; set; }
		    bool UseSpanCaptures { get; set; }
		    int MaxBufferedInputLength { get; set; } = int.MaxValue
		    int InitialBufferSize { get; set; } = 4096
		    int MaxExtraStacks { get; set; }
		    System.Type LocationType { get; set; }
		    bool ExportGrammar { get; set; }
		    bool Trace { get; set; }
		internal sealed class GramSourceAttribute : System.Attribute
		    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
		    [System.AttributeUsageAttribute(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
		    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
		    [Microsoft.CodeAnalysis.EmbeddedAttribute]
		    ctor(string text)
		    string Text { get; }
		internal sealed class GramToolingExternalAttribute : System.Attribute
		    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
		    [System.AttributeUsageAttribute(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
		    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
		    [Microsoft.CodeAnalysis.EmbeddedAttribute]
		    ctor(string method, string rule)
		    string Method { get; }
		    string Rule { get; }
		internal sealed class GramToolingGuardAttribute : System.Attribute
		    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
		    [System.AttributeUsageAttribute(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
		    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
		    [Microsoft.CodeAnalysis.EmbeddedAttribute]
		    ctor(string expression, bool accepted)
		    string Expression { get; }
		    bool Accepted { get; }
		internal sealed class GramVariantAttribute : DotGram.GramReadingAttribute
		    [System.AttributeUsageAttribute(System.AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
		    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverageAttribute]
		    [Microsoft.CodeAnalysis.EmbeddedAttribute]
		    bool UsePerCallLocations { get; set; }
		""";

	[Fact]
	public void The_emitted_attributes_are_the_listed_surface()
	{
		Assert.Equal(Normalize(Expected), Normalize(Describe()));
	}

	/// <summary>The surface as the compiler sees it: every type the generator emits into its namespace, and what of it is public.</summary>
	static string Describe()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var compilation       = CSharpCompilation.Create(
			"Surface",
			[
				CSharpSyntaxTree.ParseText(SupportEmitter.Attributes, cancellationToken: cancellationToken),
				CSharpSyntaxTree.ParseText(SupportEmitter.EmbeddedAttribute, cancellationToken: cancellationToken),
			],
			[MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		Assert.DoesNotContain(
			compilation.GetDiagnostics(cancellationToken), static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);

		var space = compilation.GlobalNamespace.GetNamespaceMembers().Single(static one => one.Name == GramMetadataNames.Namespace);
		var text  = new StringBuilder();

		foreach (var type in space.GetTypeMembers().OrderBy(static one => one.Name, StringComparer.Ordinal))
		{
			text.Append(type.DeclaredAccessibility.ToString().ToLowerInvariant());

			if (type.TypeKind == TypeKind.Class && type.IsAbstract)
				text.Append(" abstract");
			else if (type.TypeKind == TypeKind.Class && type.IsSealed)
				text.Append(" sealed");

			text.Append(' ').Append(type.TypeKind.ToString().ToLowerInvariant()).Append(' ').Append(type.Name);

			if (type.TypeKind == TypeKind.Class)
				text.Append(" : ").Append(type.BaseType!.ToDisplayString());

			text.AppendLine();

			foreach (var attribute in type.GetAttributes())
				text.Append("    [").Append(attribute).AppendLine("]");

			foreach (var member in Declared(type))
				text.Append("    ").AppendLine(member);
		}

		return text.ToString();
	}

	/// <summary>The public members a consumer writes against, in the order they are declared.</summary>
	static IEnumerable<string> Declared(INamedTypeSymbol type)
	{
		var members = type
			.GetMembers()
			.Where(static member => member.DeclaredAccessibility == Accessibility.Public && !member.IsImplicitlyDeclared)
			.Where(static member => member is not IMethodSymbol { AssociatedSymbol: not null })
			.OrderBy(static member => member.Locations[0].SourceSpan.Start);

		foreach (var member in members)
		{
			var marks = string.Concat(member.GetAttributes().Select(static attribute => "[" + attribute + "] "));

			switch (member)
			{
				case IMethodSymbol { MethodKind: MethodKind.Constructor } constructor:
					yield return marks + "ctor(" + string.Join(", ", constructor.Parameters.Select(static one => one.Type.ToDisplayString() + " " + one.Name)) + ")";
					break;

				case IPropertySymbol property:
					var initializer = property.DeclaringSyntaxReferences
						.Select(static reference => reference.GetSyntax())
						.OfType<PropertyDeclarationSyntax>()
						.Select(static syntax => syntax.Initializer?.Value.ToString())
						.FirstOrDefault();

					yield return marks + property.Type.ToDisplayString() + " " + property.Name +
						(property.SetMethod is { DeclaredAccessibility: Accessibility.Public } ? " { get; set; }" : " { get; }") +
						(initializer is null ? "" : " = " + initializer);
					break;

				case IFieldSymbol { ContainingType.TypeKind: TypeKind.Enum } constant:
					yield return marks + constant.Name + " = " + constant.ConstantValue;
					break;

				default:
					yield return marks + member.Kind + " " + member.Name;
					break;
			}
		}
	}

	static string Normalize(string text)
	{
		return text.Replace("\r\n", "\n").Trim();
	}
}
