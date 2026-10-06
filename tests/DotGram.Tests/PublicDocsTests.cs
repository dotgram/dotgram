using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A generated parser documents what another assembly sees of it, and nothing else.
/// </summary>
/// <remarks>
/// The emitter writes its comments everywhere, and the file loses those of every member nothing
/// outside the consumer's assembly can reach. Asked here of the checked-in snapshots through
/// Roslyn's syntax tree, which reads the file as the compiler does rather than a line at a time as
/// the pass that strips them does; <c>SnapshotTests</c> holds the snapshots to the generator.
/// </remarks>
public sealed class PublicDocsTests
{
	// First: the snapshots below are listed from it as the type initializes, in the order written.
	static string ThisFile { get; } = FilePath();

	public static TheoryData<string> Snapshots { get; } =
		new(Directory.GetFiles(SnapshotDirectory, "*.g.cs").Select(static path => Path.GetFileName(path)).Order(StringComparer.Ordinal));

	[Theory]
	[MemberData(nameof(Snapshots))]
	public void Only_what_another_assembly_sees_is_documented(string name)
	{
		var tree = CSharpSyntaxTree.ParseText(
			File.ReadAllText(Path.Combine(SnapshotDirectory, name)),
			CSharpParseOptions.Default.WithDocumentationMode(DocumentationMode.Parse),
			cancellationToken: TestContext.Current.CancellationToken);

		var documented = tree.GetRoot(TestContext.Current.CancellationToken)
			.DescendantNodes()
			.OfType<MemberDeclarationSyntax>()
			.Where(static member => member.GetLeadingTrivia().Any(static trivia => trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)))
			// The author's own C#, mapped back to the grammar it was written in, is documented as they wrote it.
			.Where(member => !tree.GetMappedLineSpan(member.Span).HasMappedPath)
			.ToList();

		var hidden = documented.Where(static member => !Seen(member)).Select(Named).ToList();

		Assert.True(hidden.Count == 0, "Documented, but seen by no other assembly: " + string.Join(", ", hidden.Take(20)));
	}

	/// <summary>
	/// Whether a declaration is visible outside its assembly, taking a <c>partial</c> type that
	/// says nothing about its accessibility to be the author's — the host — and so visible.
	/// </summary>
	static bool Seen(MemberDeclarationSyntax member)
	{
		for (SyntaxNode? node = member; node is not null and not BaseNamespaceDeclarationSyntax and not CompilationUnitSyntax; node = node.Parent)
		{
			if (node is not MemberDeclarationSyntax declared)
				continue;

			// The members of an enum or an interface are as visible as it is.
			if (node is EnumMemberDeclarationSyntax || node.Parent is InterfaceDeclarationSyntax)
				continue;

			var modifiers = declared.Modifiers;

			if (modifiers.Any(SyntaxKind.PublicKeyword))
				continue;

			if (modifiers.Any(SyntaxKind.ProtectedKeyword) && !modifiers.Any(SyntaxKind.PrivateKeyword))
				continue;

			if (node is BaseTypeDeclarationSyntax && modifiers.Any(SyntaxKind.PartialKeyword) &&
				!modifiers.Any(SyntaxKind.PrivateKeyword) && !modifiers.Any(SyntaxKind.InternalKeyword))
				continue;

			return false;
		}

		return true;
	}

	static string Named(MemberDeclarationSyntax member)
	{
		var line = member.GetLocation().GetLineSpan().StartLinePosition.Line + 1;

		return member switch
		{
			BaseTypeDeclarationSyntax type => type.Identifier.Text,
			MethodDeclarationSyntax method => method.Identifier.Text,
			PropertyDeclarationSyntax property => property.Identifier.Text,
			FieldDeclarationSyntax field => field.Declaration.Variables[0].Identifier.Text,
			_ => member.Kind().ToString(),
		} + " (line " + line + ")";
	}

	static string SnapshotDirectory => Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!, "Snapshots");

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}
