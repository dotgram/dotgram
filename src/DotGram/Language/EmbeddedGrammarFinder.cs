using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Language;

/// <summary>One grammar string proven by Roslyn to belong to <c>DotGram.GramAttribute</c>.</summary>
sealed class EmbeddedGrammar(
	string text,
	SyntaxToken token,
	CSharpStringMap sourceMap,
	string? analysisText = null,
	GramAnalysisOptions? options = null,
	string? name = null,
	TextSpan? hostSpan = null)
{
	public string          Text      { get; } = text;
	public SyntaxToken     Token     { get; } = token;
	public CSharpStringMap SourceMap { get; } = sourceMap;
	public string AnalysisText { get; } = analysisText ?? text;

	/// <summary>What the host's attribute says about compiling it, or null where nothing is known.</summary>
	public GramAnalysisOptions? Options { get; } = options;

	/// <summary>
	/// The name a user knows the grammar by: its class, nested classes dotted, and the
	/// <c>Name</c> after it where the attribute gives one (<c>Parser.Immediate</c>); for a string
	/// passed to a <c>StringSyntax</c> parameter, the method and the parameter (<c>Inspect(grammar)</c>).
	/// Null where nothing names it.
	/// </summary>
	public string? Name { get; } = name;

	/// <summary>
	/// What the grammar belongs to in the C# document: the declaration of the class its attribute is
	/// written on, attributes included, or the member a <c>StringSyntax</c> argument is passed in;
	/// the string itself where there is neither.
	/// </summary>
	public TextSpan HostSpan { get; } = hostSpan ?? token.Span;
}

/// <summary>Finds embedded grammars by attribute identity rather than source spelling.</summary>
static class EmbeddedGrammarFinder
{
	const string GramAttribute = "DotGram.GramAttribute";
	const string StringSyntaxAttribute = "System.Diagnostics.CodeAnalysis.StringSyntaxAttribute";
	const string DotGramSyntax = "DotGram";
	const string DotGramExtensionSyntax = ".gram";

	/// <param name="includedFile">
	/// The text of a <c>.gram</c> file in the host's project by the name an included grammar's
	/// <c>[Gram]</c> gives it, or null where it is not there. Without it, an included grammar
	/// that is a file is read from what its class carries, as across an assembly reference.
	/// </param>
	public static IReadOnlyList<EmbeddedGrammar> Find(
		SemanticModel model,
		SyntaxNode root,
		CancellationToken cancellationToken = default,
		Func<string, string?>? includedFile = null)
	{
		var includes = new CompilationIncludedGrammars(model.Compilation, includedFile);

		return Find(model, root, cancellationToken, type => GramIncludes.Walk(type)
			.Select(include => (include, includes.Select(include).Text))
			.Where(static item => item.Text is not null)
			.Select(static item => new GrammarSplice.Part(item.Text!, item.include.Name, null))
			.ToArray());
	}

	/// <summary>
	/// The same, what each include is compiled with asked of <paramref name="includes"/> first: what a
	/// solution's project answers about the grammars it includes, in an editor.
	/// </summary>
	public static async Task<IReadOnlyList<EmbeddedGrammar>> FindAsync(
		SemanticModel model,
		SyntaxNode root,
		IncludedGrammarSources includes,
		CancellationToken cancellationToken)
	{
		var parts = new Dictionary<INamedTypeSymbol, IReadOnlyList<GrammarSplice.Part>>(SymbolEqualityComparer.Default);

		foreach (var type in Hosts(model, root, cancellationToken))
			parts[type] =
			[
				.. (await includes.IncludesAsync(type, cancellationToken).ConfigureAwait(false))
					.Where(static item => item.Choice.Text is not null)
					.Select(static item => new GrammarSplice.Part(item.Choice.Text!, item.Include.Name, null)),
			];

		return Find(model, root, cancellationToken, type => parts.TryGetValue(type, out var found) ? found : []);
	}

	/// <summary>The classes a <c>[Gram]</c> of the document is written on, the attribute known by its symbol however it is spelled.</summary>
	static IEnumerable<INamedTypeSymbol> Hosts(SemanticModel model, SyntaxNode root, CancellationToken cancellationToken)
	{
		var seen = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);

		foreach (var attribute in root.DescendantNodes().OfType<AttributeSyntax>())
			if (IsGramAttribute(model, attribute, cancellationToken) &&
				attribute.Parent?.Parent is TypeDeclarationSyntax declaration &&
				model.GetDeclaredSymbol(declaration, cancellationToken) is INamedTypeSymbol type &&
				seen.Add(type))
				yield return type;
	}

	/// <param name="included">What a host's includes are compiled with, in its walk's order.</param>
	static IReadOnlyList<EmbeddedGrammar> Find(
		SemanticModel model,
		SyntaxNode root,
		CancellationToken cancellationToken,
		Func<INamedTypeSymbol, IReadOnlyList<GrammarSplice.Part>> included)
	{
		if (model is null)
			throw new ArgumentNullException(nameof(model));

		if (root is null)
			throw new ArgumentNullException(nameof(root));

		var grammars = new List<EmbeddedGrammar>();
		var seen = new HashSet<TextSpan>();

		foreach (var attribute in root.DescendantNodes().OfType<AttributeSyntax>())
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (!IsGramAttribute(model, attribute, cancellationToken) ||
				attribute.ArgumentList?.Arguments.FirstOrDefault(
					static argument => argument.NameEquals is null) is not
						{ Expression: LiteralExpressionSyntax literal } ||
				IsFile(literal.Token.ValueText) ||
				!CSharpStringMap.TryCreate(literal.Token, out var map))
				continue;

			var own = literal.Token.ValueText;
			if (IsFile(own))
				continue;

			var (parts, host) = Host(model, attribute, included, cancellationToken);
			var (analysisText, joinedMap) = parts.Count == 0
				? (own, null)
				: GrammarSplice.Join(new GrammarSplice.Part(own, null, null), parts);
			var options = new GramAnalysisOptions
			{
				Own     = parts.Count == 0 ? null : own.Length,
				Cuts    = joinedMap is null
					? null
					: GrammarSplice.Cuts(analysisText, joinedMap.Segments.Skip(1).Select(static one => (one.Start, one.Length))),
				Lexical = host?.Options.Lexical ?? false,
				Host    = host,
			};

			var name = host is null
				? null
				: host.Options.Name is { Length: > 0 } suffix ? host.ClassName + "." + suffix : host.ClassName;

			grammars.Add(new EmbeddedGrammar(
				own,
				literal.Token,
				map!,
				analysisText,
				options,
				name,
				(attribute.Parent?.Parent as MemberDeclarationSyntax)?.Span));
			seen.Add(literal.Token.Span);
		}

		foreach (var argument in root.DescendantNodes().OfType<ArgumentSyntax>())
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (argument.Expression is not LiteralExpressionSyntax literal ||
				seen.Contains(literal.Token.Span) ||
				model.GetOperation(argument, cancellationToken) is not IArgumentOperation
				{
					Parameter: { Type.SpecialType: SpecialType.System_String } parameter,
				} ||
				!HasDotGramStringSyntax(parameter) ||
				!CSharpStringMap.TryCreate(literal.Token, out var map))
				continue;

			var method = parameter.ContainingSymbol as IMethodSymbol;
			var callee = method?.MethodKind == MethodKind.Constructor ? method.ContainingType.Name : method?.Name;

			grammars.Add(new EmbeddedGrammar(
				literal.Token.ValueText,
				literal.Token,
				map!,
				name:     callee is null ? null : callee + "(" + parameter.Name + ")",
				hostSpan: argument.FirstAncestorOrSelf<MemberDeclarationSyntax>()?.Span));
			seen.Add(literal.Token.Span);
		}

		return grammars;
	}

	/// <summary>
	/// Finds source-spelled <c>Gram</c> attributes without requesting a semantic model.
	/// The result is intentionally provisional and is used only to make initial editor
	/// classification available while Roslyn finishes the authoritative analysis.
	/// </summary>
	public static IReadOnlyList<EmbeddedGrammar> FindSyntactic(
		SyntaxNode root,
		CancellationToken cancellationToken = default)
	{
		if (root is null)
			throw new ArgumentNullException(nameof(root));

		var grammars = new List<EmbeddedGrammar>();

		foreach (var attribute in root.DescendantNodes().OfType<AttributeSyntax>())
		{
			cancellationToken.ThrowIfCancellationRequested();

			var name = attribute.Name.ToString();
			if (name is not "Gram" and not "GramAttribute" &&
				!name.EndsWith(".Gram", StringComparison.Ordinal) &&
				!name.EndsWith(".GramAttribute", StringComparison.Ordinal) ||
				attribute.ArgumentList?.Arguments.FirstOrDefault(
					static argument => argument.NameEquals is null) is not
						{ Expression: LiteralExpressionSyntax literal } ||
				IsFile(literal.Token.ValueText) ||
				!CSharpStringMap.TryCreate(literal.Token, out var map))
				continue;

			grammars.Add(new EmbeddedGrammar(literal.Token.ValueText, literal.Token, map!));
		}

		return grammars;
	}

	static bool HasDotGramStringSyntax(IParameterSymbol parameter)
	{
		return parameter.GetAttributes().Any(static attribute =>
			attribute.AttributeClass?.ToDisplayString() == StringSyntaxAttribute &&
			attribute.ConstructorArguments is [{ Value: string syntax }] &&
			(string.Equals(syntax, DotGramSyntax, StringComparison.OrdinalIgnoreCase) ||
			 string.Equals(syntax, DotGramExtensionSyntax, StringComparison.OrdinalIgnoreCase)));
	}

	static bool IsGramAttribute(
		SemanticModel model, AttributeSyntax attribute, CancellationToken cancellationToken)
	{
		var symbolInfo = model.GetSymbolInfo(attribute, cancellationToken);
		var actual     = (symbolInfo.Symbol as IMethodSymbol)?.ContainingType ??
			symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().Select(static symbol => symbol.ContainingType).FirstOrDefault() ??
			model.GetTypeInfo(attribute, cancellationToken).Type;

		return actual?.ToDisplayString() == GramAttribute && IsAttribute(actual);
	}

	/// <summary>
	/// What the class an attribute is written on includes, read the way a standalone grammar's
	/// host is read, and what the attribute asks of the compilation; null where it is on no class.
	/// </summary>
	static (IReadOnlyList<GrammarSplice.Part> Included, GramHostReading? Reading) Host(
		SemanticModel model,
		AttributeSyntax attribute,
		Func<INamedTypeSymbol, IReadOnlyList<GrammarSplice.Part>> included,
		CancellationToken cancellationToken)
	{
		if (attribute.Parent?.Parent is not TypeDeclarationSyntax declaration ||
			model.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol type)
			return (Array.Empty<GrammarSplice.Part>(), null);

		// As the generator compiles each (GramIncludes.Choose), from what the project answers.
		var parts = included(type);

		// The attribute's own options, read as the generator reads them: what a reading does not
		// say it takes from the class's first one.
		var reading = type.GetAttributes()
			.FirstOrDefault(candidate => candidate.ApplicationSyntaxReference?.Span == attribute.Span);
		var firstReading = GramIncludes.Grams(type).FirstOrDefault();
		var options = reading is null ? null : GramHostOptions.Read(
			reading,
			type,
			firstReading is null || ReferenceEquals(firstReading, reading)
				? null
				: GramHostOptions.Read(firstReading, type, first: null));

		return (parts, options is null ? null : GramHostReading.Of(options, type));
	}

	static bool IsFile(string source)
	{
		return GramIncludes.IsPath(source);
	}

	static bool IsAttribute(ITypeSymbol type)
	{
		for (var current = type.BaseType; current is not null; current = current.BaseType)
			if (current.ToDisplayString() == "System.Attribute")
				return true;

		return false;
	}
}
