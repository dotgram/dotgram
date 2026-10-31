using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Language;

/// <summary>
/// What Roslyn says about the C# around and inside a grammar — definitions, references and the generated
/// API's sources — asked of the project the document belongs to in a workspace the editor hands in.
/// </summary>
/// <remarks>
/// A C# expression inside a grammar belongs to no document Roslyn knows. It is asked about as the
/// body of a method in a document added to the project for the question, where the names the host
/// class declares are in scope. Completion and Quick Info are Roslyn's Features, which this folder does
/// not reference; an editor asks them through <see cref="Project"/> and <see cref="SyntheticDocument"/>.
/// </remarks>
sealed class RoslynGramCompletion(Workspace workspace, Func<string?> filePath)
{
	const string Before = "using System; class __DotGramCompletion { object __Value() { return ";
	const string After = "; } }";

	/// <summary>What an editor says of an item Roslyn offered.</summary>
	public const string CSharpDescription = "C# symbol provided by Roslyn";

	/// <summary>
	/// The symbols a name in a C# expression may be defined as, in the order an editor tries to go to
	/// them: what the expression binds to, and then every declaration of the name, the grammar's host's
	/// own first and those in source before those in metadata.
	/// </summary>
	public async Task<RoslynDefinitionTargets?> DefinitionTargetsAsync(
		string expression,
		int position,
		CancellationToken cancellationToken)
	{
		var project = Project();
		if (project is null)
			return null;

		var document = SyntheticDocument(project, expression);
		var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (model is null)
			return null;

		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var absolutePosition = Before.Length + position;
		var token = root?.FindToken(Math.Max(0, Math.Min(absolutePosition, root.FullSpan.End - 1)));
		var symbol = await QualifiedMemberAsync(
			project, expression, position, cancellationToken).ConfigureAwait(false) ??
			token?.Parent?.AncestorsAndSelf()
			.Select(node => model.GetSymbolInfo(node, cancellationToken))
			.Select(info => info.Symbol ?? info.CandidateSymbols
				.OrderByDescending(static candidate => candidate is IFieldSymbol)
				.ThenByDescending(static candidate => candidate is IPropertySymbol)
				.FirstOrDefault())
			.FirstOrDefault(static candidate => candidate is not null) ??
			await SymbolFinder.FindSymbolAtPositionAsync(
				model,
				absolutePosition,
				workspace,
				cancellationToken).ConfigureAwait(false);
		var targets = new List<ISymbol>();

		if (symbol is not null)
			targets.Add(symbol);

		var name = IdentifierAt(expression, position);
		if (name.Length == 0)
			return new RoslynDefinitionTargets(project, targets);

		var declarations = await SymbolFinder.FindDeclarationsAsync(
			project, name, ignoreCase: false, cancellationToken).ConfigureAwait(false);
		var hostType = await HostTypeAsync(project, cancellationToken).ConfigureAwait(false);
		targets.AddRange(declarations
			.Where(candidate => candidate.Name == name)
			.OrderByDescending(candidate => hostType is not null && SymbolEqualityComparer.Default.Equals(
				candidate.ContainingType, hostType))
			.ThenByDescending(static candidate => candidate.Locations.Any(static location => location.IsInSource)));

		return new RoslynDefinitionTargets(project, targets);
	}

	internal static async Task<ISymbol?> QualifiedMemberAsync(
		Project project,
		string expression,
		int position,
		CancellationToken cancellationToken)
	{
		var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(expression, cancellationToken: cancellationToken);
		var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
		var boundedPosition = Math.Max(0, Math.Min(position, root.FullSpan.End - 1));
		var member = root.FindToken(boundedPosition).Parent?.AncestorsAndSelf()
			.OfType<MemberAccessExpressionSyntax>()
			.FirstOrDefault(candidate => candidate.Name.Span.Contains(boundedPosition));
		if (member?.Expression is not IdentifierNameSyntax typeName)
			return null;

		var declarations = await SymbolFinder.FindDeclarationsAsync(
			project, typeName.Identifier.ValueText, ignoreCase: false, cancellationToken).ConfigureAwait(false);
		return declarations
			.OfType<INamedTypeSymbol>()
			.SelectMany(type => type.GetMembers(member.Name.Identifier.ValueText))
			.OrderByDescending(static candidate => candidate is IFieldSymbol)
			.ThenByDescending(static candidate => candidate is IPropertySymbol)
			.FirstOrDefault();
	}

	public async Task<GeneratedApiSource?> GeneratedApiSourceAsync(
		int position,
		CancellationToken cancellationToken)
	{
		if (filePath() is not { } path)
			return null;

		var solution = workspace.CurrentSolution;
		var documentId = solution.GetDocumentIdsWithFilePath(path).FirstOrDefault();
		var document = documentId is null ? null : solution.GetDocument(documentId);
		if (document is null)
		{
			var fileName = FileName(path);
			foreach (var project in solution.Projects)
			{
				var generated = await project.GetSourceGeneratedDocumentsAsync(cancellationToken).ConfigureAwait(false);
				document = generated.FirstOrDefault(candidate =>
					string.Equals(candidate.Name, fileName, StringComparison.OrdinalIgnoreCase) ||
					candidate.FilePath is not null && string.Equals(
						FileName(candidate.FilePath), fileName, StringComparison.OrdinalIgnoreCase));
				if (document is not null)
					break;
			}
		}
		if (document is null)
			return null;

		var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (model is null || root is null)
			return null;

		var boundedPosition = Math.Max(0, Math.Min(position, root.FullSpan.End - 1));
		var token = root.FindToken(boundedPosition);
		var symbol = token.Parent?.AncestorsAndSelf()
			.Select(node => model.GetDeclaredSymbol(node, cancellationToken) ??
				model.GetSymbolInfo(node, cancellationToken).Symbol ??
				model.GetSymbolInfo(node, cancellationToken).CandidateSymbols.FirstOrDefault())
			.FirstOrDefault(candidate => candidate is IMethodSymbol) ??
			await SymbolFinder.FindSymbolAtPositionAsync(
				model, boundedPosition, workspace, cancellationToken).ConfigureAwait(false);
		if (symbol is not IMethodSymbol method)
			return symbol is null
				? null
				: await GrammarReferenceSourceAsync(symbol, document.Project, cancellationToken).ConfigureAwait(false);

		var names = method.Name.StartsWith("Try", StringComparison.Ordinal) && method.Name.Length > 3
			? new[] { method.Name, method.Name.Substring(3) }
			: new[] { method.Name };

		var grammarHost = method.ContainingType;
		while (grammarHost.ContainingType is not null && !grammarHost.GetAttributes().Any(static attribute =>
			attribute.AttributeClass?.ToDisplayString() == "DotGram.GramAttribute"))
			grammarHost = grammarHost.ContainingType;

		foreach (var attribute in grammarHost.GetAttributes())
		{
			if (attribute.AttributeClass?.ToDisplayString() != "DotGram.GramAttribute" ||
				attribute.ConstructorArguments.Length == 0 ||
				attribute.ConstructorArguments[0].Value is not string source)
				continue;

			if (source.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) &&
				source.IndexOf('\n') < 0 && source.IndexOf('\r') < 0)
			{
				var grammar = solution.Projects
					.SelectMany(static project => project.AdditionalDocuments)
					.FirstOrDefault(candidate => candidate.FilePath is not null &&
						string.Equals(FileName(candidate.FilePath), FileName(source), StringComparison.OrdinalIgnoreCase));
				if (grammar?.FilePath is null)
					continue;

				var text = await grammar.GetTextAsync(cancellationToken).ConfigureAwait(false);
				var api = GramLanguageService.Analyze(text.ToString()).PublishedApis
					.FirstOrDefault(candidate => names.Contains(candidate.MethodName, StringComparer.Ordinal));
				if (api.MethodName is not null)
				{
					var location = text.Lines.GetLinePosition(api.Position);
					return new GeneratedApiSource(grammar.FilePath, location.Line, location.Character);
				}
			}
			else if (attribute.ApplicationSyntaxReference is { } syntaxReference &&
				await syntaxReference.GetSyntaxAsync(cancellationToken).ConfigureAwait(false) is AttributeSyntax syntax &&
				syntax.ArgumentList?.Arguments is [{ Expression: LiteralExpressionSyntax literal }] &&
				CSharpStringMap.TryCreate(literal.Token, out var map))
			{
				var api = GramLanguageService.Analyze(literal.Token.ValueText).PublishedApis
					.FirstOrDefault(candidate => names.Contains(candidate.MethodName, StringComparer.Ordinal));
				var host = solution.GetDocument(syntax.SyntaxTree);
				if (api.MethodName is not null && host?.FilePath is not null &&
					map!.TryMap(api.Position, api.Length, out var span))
				{
					var hostText = await host.GetTextAsync(cancellationToken).ConfigureAwait(false);
					var location = hostText.Lines.GetLinePosition(span.Start);
					return new GeneratedApiSource(host.FilePath, location.Line, location.Character);
				}
			}
		}

		return await GrammarReferenceSourceAsync(method, document.Project, cancellationToken).ConfigureAwait(false);
	}

	public async Task<GeneratedApiSource?> GrammarFileSourceAsync(
		int position,
		CancellationToken cancellationToken)
	{
		if (filePath() is not { } path)
			return null;

		var solution = workspace.CurrentSolution;
		var documentId = solution.GetDocumentIdsWithFilePath(path).FirstOrDefault();
		var document = documentId is null ? null : solution.GetDocument(documentId);
		if (document is null)
			return null;

		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (root is null || model is null)
			return null;

		var literal = root.FindToken(Math.Max(0, Math.Min(position, root.FullSpan.End - 1))).Parent?
			.AncestorsAndSelf().OfType<LiteralExpressionSyntax>().FirstOrDefault();
		var attribute = literal?.Ancestors().OfType<AttributeSyntax>().FirstOrDefault();
		if (literal is null || attribute is null ||
			!literal.Token.Span.Contains(position) ||
			literal.Token.ValueText is not { } source ||
			!source.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) ||
			source.IndexOf('\n') >= 0 || source.IndexOf('\r') >= 0)
			return null;

		var symbol = model.GetSymbolInfo(attribute, cancellationToken).Symbol as IMethodSymbol;
		if (symbol?.ContainingType.ToDisplayString() != "DotGram.GramAttribute")
			return null;

		var grammar = document.Project.AdditionalDocuments.FirstOrDefault(candidate =>
			candidate.FilePath is not null && FileName(candidate.FilePath).Equals(
				FileName(source), StringComparison.OrdinalIgnoreCase));
		return grammar?.FilePath is null
			? null
			: new GeneratedApiSource(grammar.FilePath, 0, 0);
	}

	public async Task<CSharpFindReferences?> FindReferencesAsync(
		int position,
		CancellationToken cancellationToken,
		Action<string>? diagnostics = null)
	{
		if (filePath() is not { } path)
			return null;

		var solution = workspace.CurrentSolution;
		var documentId = solution.GetDocumentIdsWithFilePath(path).FirstOrDefault();
		var document = documentId is null ? null : solution.GetDocument(documentId);
		if (document is null)
			return null;

		var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (model is null || root is null)
			return null;

		var boundedPosition = Math.Max(0, Math.Min(position, root.FullSpan.End - 1));
		var symbol = root.FindToken(boundedPosition).Parent?.AncestorsAndSelf()
			.Select(node => model.GetDeclaredSymbol(node, cancellationToken) ?? model.GetSymbolInfo(node, cancellationToken).Symbol)
			.FirstOrDefault(static candidate => candidate is not null) ??
			await SymbolFinder.FindSymbolAtPositionAsync(model, boundedPosition, workspace, cancellationToken).ConfigureAwait(false);
		if (symbol is null)
			return null;

		var found = new List<CSharpFindReference>();
		foreach (var location in symbol.Locations.Where(static location => location.IsInSource))
			await AddSourceReferenceAsync(solution, location, found, cancellationToken).ConfigureAwait(false);

		foreach (var referenced in await SymbolFinder.FindReferencesAsync(symbol, solution, cancellationToken).ConfigureAwait(false))
			foreach (var location in referenced.Locations)
				await AddSourceReferenceAsync(solution, location.Location, found, cancellationToken).ConfigureAwait(false);

		found.AddRange(await GrammarReferencesAsync(symbol, document.Project, cancellationToken).ConfigureAwait(false));
		found.AddRange(await EmbeddedGrammarReferencesAsync(
			symbol, document.Project, cancellationToken, diagnostics).ConfigureAwait(false));
		return new CSharpFindReferences(
			symbol.Name,
			found.GroupBy(static item => (item.FilePath, item.Position))
				.Select(static group => group.First())
				.OrderBy(static item => item.FilePath, StringComparer.OrdinalIgnoreCase)
				.ThenBy(static item => item.Position)
				.ToArray());
	}

	static async Task AddSourceReferenceAsync(
		Solution solution,
		Location location,
		ICollection<CSharpFindReference> found,
		CancellationToken cancellationToken)
	{
		var document = location.SourceTree is null ? null : solution.GetDocument(location.SourceTree);
		if (document?.FilePath is null)
			return;

		var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
		found.Add(new CSharpFindReference(document.FilePath, text.ToString(), location.SourceSpan.Start, location.SourceSpan.Length));
	}

	async Task<GeneratedApiSource?> GrammarReferenceSourceAsync(
		ISymbol symbol,
		Project project,
		CancellationToken cancellationToken)
	{
		var references = await GrammarReferencesAsync(symbol, project, cancellationToken).ConfigureAwait(false);
		if (references.Count == 0)
			return null;

		var reference = references[0];
		var text = SourceText.From(reference.Text);
		var location = text.Lines.GetLinePosition(reference.Position);
		return new GeneratedApiSource(reference.FilePath, location.Line, location.Character);
	}

	async Task<IReadOnlyList<CSharpFindReference>> GrammarReferencesAsync(
		ISymbol symbol,
		Project project,
		CancellationToken cancellationToken)
	{
		var found = new List<CSharpFindReference>();
		foreach (var grammar in project.AdditionalDocuments.Where(document =>
			document.FilePath?.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) == true))
		{
			var grammarHost = grammar.FilePath is null
				? null
				: await HostTypeAsync(project, FileName(grammar.FilePath), cancellationToken).ConfigureAwait(false);
			var text = await grammar.GetTextAsync(cancellationToken).ConfigureAwait(false);
			var source = text.ToString();
			for (var position = source.IndexOf(symbol.Name, StringComparison.Ordinal);
				position >= 0;
				position = source.IndexOf(symbol.Name, position + symbol.Name.Length, StringComparison.Ordinal))
			{
				if (!IsIdentifier(source, position, symbol.Name.Length) ||
					!GramCSharpCompletionContext.TryGetExpression(
						source, position, out var expression, out var expressionStart, out _, out _))
					continue;

				var referenced = await SymbolInExpressionAsync(
					project,
					expression,
					position - expressionStart,
					cancellationToken).ConfigureAwait(false);
				if (!SymbolEqualityComparer.Default.Equals(referenced?.OriginalDefinition, symbol.OriginalDefinition) &&
					!IsUnqualifiedHostMember(expression, position - expressionStart, symbol, grammarHost))
					continue;

				if (grammar.FilePath is not null)
					found.Add(new CSharpFindReference(grammar.FilePath, source, position, symbol.Name.Length));
			}
		}

		return found;
	}

	internal static async Task<IReadOnlyList<CSharpFindReference>> EmbeddedGrammarReferencesAsync(
		ISymbol symbol,
		Project project,
		CancellationToken cancellationToken,
		Action<string>? diagnostics = null)
	{
		var found = new List<CSharpFindReference>();
		foreach (var document in project.Documents)
		{
			try
			{
				var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
				var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
				if (root is null || model is null || document.FilePath is null)
					continue;

				var hostText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
				foreach (var grammar in EmbeddedGrammarFinder.Find(model, root, cancellationToken))
				{
					var grammarHost = grammar.Token.Parent?.AncestorsAndSelf()
						.OfType<AttributeSyntax>()
						.Select(static attribute => attribute.Parent?.Parent)
						.OfType<TypeDeclarationSyntax>()
						.Select(declaration => model.GetDeclaredSymbol(declaration, cancellationToken) as INamedTypeSymbol)
						.FirstOrDefault(static candidate => candidate is not null);
					var source = grammar.Text;
					for (var position = source.IndexOf(symbol.Name, StringComparison.Ordinal);
						position >= 0;
						position = source.IndexOf(symbol.Name, position + symbol.Name.Length, StringComparison.Ordinal))
					{
						if (!IsIdentifier(source, position, symbol.Name.Length) ||
							!GramCSharpCompletionContext.TryGetExpression(
								source, position, out var expression, out var expressionStart, out _, out _) ||
							!grammar.SourceMap.TryMap(position, symbol.Name.Length, out var mapped))
							continue;

						var referenced = await SymbolInExpressionAsync(
							project, expression, position - expressionStart, cancellationToken).ConfigureAwait(false);
						if (!SymbolEqualityComparer.Default.Equals(referenced?.OriginalDefinition, symbol.OriginalDefinition) &&
							!IsUnqualifiedHostMember(expression, position - expressionStart, symbol, grammarHost))
							continue;

						found.Add(new CSharpFindReference(
							document.FilePath, hostText.ToString(), mapped.Start, mapped.Length));
					}
				}
			}
			catch (Exception exception) when (exception is not OutOfMemoryException && exception is not OperationCanceledException)
			{
				diagnostics?.Invoke($"Embedded Find All References failed for {document.FilePath}: {exception}");
			}
		}

		return found;
	}

	internal static bool IsUnqualifiedHostMember(
		string expression,
		int position,
		ISymbol symbol,
		INamedTypeSymbol? grammarHost)
	{
		if (grammarHost is null || !SymbolEqualityComparer.Default.Equals(symbol.ContainingType, grammarHost))
			return false;

		var root = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(expression).GetRoot();
		var boundedPosition = Math.Max(0, Math.Min(position, root.FullSpan.End - 1));
		var name = root.FindToken(boundedPosition).Parent?.AncestorsAndSelf()
			.OfType<IdentifierNameSyntax>()
			.FirstOrDefault(candidate => candidate.Span.Contains(boundedPosition));
		return name?.Identifier.ValueText == symbol.Name && name.Parent is not MemberAccessExpressionSyntax;
	}

	static async Task<ISymbol?> SymbolInExpressionAsync(
		Project project,
		string expression,
		int position,
		CancellationToken cancellationToken)
	{
		var document = SyntheticDocument(project, expression);
		var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (model is null || root is null)
			return null;

		var absolute = Before.Length + position;
		var token = root.FindToken(Math.Max(0, Math.Min(absolute, root.FullSpan.End - 1)));
		return await QualifiedMemberAsync(project, expression, position, cancellationToken).ConfigureAwait(false) ??
			token.Parent?.AncestorsAndSelf()
			.Select(node => model.GetSymbolInfo(node, cancellationToken))
			.Select(info => info.Symbol ?? info.CandidateSymbols.FirstOrDefault())
			.FirstOrDefault(static candidate => candidate is not null);
	}

	static bool IsIdentifier(string source, int position, int length)
	{
		return (position == 0 || !IsIdentifierCharacter(source[position - 1])) &&
		(position + length == source.Length || !IsIdentifierCharacter(source[position + length]));
	}

	async Task<INamedTypeSymbol?> HostTypeAsync(Project project, CancellationToken cancellationToken)
	{
		if (filePath() is not { } path)
			return null;

		return await HostTypeAsync(project, Path.GetFileName(path), cancellationToken).ConfigureAwait(false);
	}

	static async Task<INamedTypeSymbol?> HostTypeAsync(
		Project project,
		string grammarFile,
		CancellationToken cancellationToken)
	{
		foreach (var document in project.Documents)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (root is null || model is null)
				continue;

			foreach (var declaration in root.DescendantNodes().OfType<TypeDeclarationSyntax>())
			{
				var type = model.GetDeclaredSymbol(declaration, cancellationToken) as INamedTypeSymbol;
				if (type is null)
					continue;

				foreach (var attribute in type.GetAttributes())
				{
					if (attribute.AttributeClass?.ToDisplayString() != "DotGram.GramAttribute")
						continue;

					var source = attribute.ConstructorArguments.Length > 0
						? attribute.ConstructorArguments[0].Value as string
						: null;
					if (source is not null && (!source.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) ||
						source.IndexOf('\n') >= 0 || source.IndexOf('\r') >= 0))
						continue;

					var wanted = source is null ? type.Name + ".gram" : FileName(source);
					if (string.Equals(wanted, grammarFile, StringComparison.OrdinalIgnoreCase))
						return type;
				}
			}
		}

		return null;
	}

	static string FileName(string path)
	{
		var separator = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));
		return separator < 0 ? path : path.Substring(separator + 1);
	}

	static string IdentifierAt(string expression, int position)
	{
		var start = Math.Min(position, expression.Length);
		var end = start;
		while (start > 0 && IsIdentifierCharacter(expression[start - 1])) start--;
		while (end < expression.Length && IsIdentifierCharacter(expression[end])) end++;
		return expression.Substring(start, end - start);
	}

	static bool IsIdentifierCharacter(char character)
	{
		return char.IsLetterOrDigit(character) || character == '_';
	}

	/// <summary>Where a C# expression asked about begins in the document it is asked in.</summary>
	internal static int ExpressionOffset => Before.Length;

	/// <summary>The expression as the body of a method in a document added to the project for the question.</summary>
	internal static Document SyntheticDocument(Project project, string expression)
	{
		return project.AddDocument(
			"__DotGramCompletion.cs",
			SourceText.From(Before + expression + After));
	}

	/// <summary>The project the document belongs to, or the solution's first C# project; null where there is none.</summary>
	internal Project? Project()
	{
		if (filePath() is { } path)
		{
			var solution = workspace.CurrentSolution;
			var id = solution.GetDocumentIdsWithFilePath(path).FirstOrDefault();
			if (id is not null)
				return solution.GetProject(id.ProjectId);

			var additionalProject = solution.Projects.FirstOrDefault(project =>
				project.AdditionalDocuments.Any(document =>
					string.Equals(document.FilePath, path, StringComparison.OrdinalIgnoreCase)));
			if (additionalProject is not null)
				return additionalProject;
		}

		return workspace.CurrentSolution.Projects.FirstOrDefault(
			static project => project.Language == LanguageNames.CSharp);
	}
}

readonly record struct GeneratedApiSource(string FilePath, int Line, int Column);

readonly record struct CSharpFindReference(string FilePath, string Text, int Position, int Length);

sealed record CSharpFindReferences(string Name, IReadOnlyList<CSharpFindReference> References);

/// <summary>The project a C# name was looked up in, and what it may be defined as, in the order to try.</summary>
sealed record RoslynDefinitionTargets(Project Project, IReadOnlyList<ISymbol> Symbols);

/// <summary>What Roslyn's Quick Info says: paragraphs of runs, each run tagged as Roslyn tags it (<c>Keyword</c>, <c>Class</c>, ...).</summary>
sealed class RoslynGramQuickInfo(IReadOnlyList<IReadOnlyList<RoslynTaggedRun>> sections)
{
	public IReadOnlyList<IReadOnlyList<RoslynTaggedRun>> Sections { get; } = sections;
}

/// <summary>A run of Roslyn's text and its tag.</summary>
readonly record struct RoslynTaggedRun(string Tag, string Text);
