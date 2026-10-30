using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Grammar.Parsing;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// Every grammar in the repository, read the way the Visual Studio extension reads it, comes out
/// classified token for token: no token the generator's lexer makes is left without a colour or
/// shown as unrecognized.
/// </summary>
/// <remarks>
/// The grammars are every <c>.gram</c> file and every grammar written into a <c>[Gram("…")]</c>
/// attribute under <c>src</c>, <c>examples</c>, <c>tests</c> and <c>benchmarks</c>, which a new
/// grammar joins without anyone adding it here. Here rather than in DotGram.Tests because the
/// editor's analysis compiles each grammar whole, and the T-SQL grammar alone takes half a minute.
/// </remarks>
public sealed class EditorCorpusTests
{
	public static TheoryData<string> Grammars()
	{
		var data = new TheoryData<string>();

		foreach (var (name, _) in Corpus())
			data.Add(name);

		return data;
	}

	[Theory]
	[MemberData(nameof(Grammars))]
	public void Every_token_of_the_grammar_is_classified(string name)
	{
		var text     = Corpus().Single(grammar => grammar.Name == name).Text;
		var document = GramLanguageService.Analyze(text);
		var spans    = document.Classifications.OrderBy(static span => span.Position).ToArray();
		var starts   = spans.Select(static span => span.Position).ToArray();
		var missing  = new List<string>();

		foreach (var token in GramLexer.Tokenize(text, RoslynCSharpScanner.Instance).Tokens)
		{
			if (token.Kind == TokenKind.EndOfFile)
				continue;

			// The last span that starts at or before the token is the only one that can cover it.
			var at      = Array.BinarySearch(starts, token.Position);
			var index   = at >= 0 ? at : ~at - 1;
			var covered = index >= 0 && token.Position < spans[index].Position + spans[index].Length;

			if (!covered)
				missing.Add($"{Place(text, token.Position)} '{text.Substring(token.Position, token.Length)}' ({token.Kind}) has no classification");
			else if (spans[index].Kind == GramSyntaxKind.Invalid && token.Kind != TokenKind.Unknown)
				missing.Add($"{Place(text, token.Position)} '{text.Substring(token.Position, token.Length)}' ({token.Kind}) is shown as unrecognized");
		}

		Assert.True(missing.Count == 0, string.Join("\n", missing.Take(20)));
	}

	[Fact]
	public void The_corpus_holds_files_and_attributes()
	{
		var corpus = Corpus();

		Assert.True(Unread.Count == 0, "A [Gram] whose grammar is no constant this test can read: " + string.Join("; ", Unread));
		Assert.Contains(corpus, static grammar => grammar.Name.StartsWith("tests/DotGram.Compatibility/Consumer.cs#", StringComparison.Ordinal));

		Assert.Contains(corpus, static grammar => grammar.Name.EndsWith("TransactSql.gram", StringComparison.Ordinal));
		Assert.Contains(corpus, static grammar => grammar.Name.Contains(".cs#", StringComparison.Ordinal));
	}

	static string Place(string text, int position)
	{
		var line   = 1;
		var column = 1;

		for (var at = 0; at < position; at++)
		{
			if (text[at] == '\n')
			{
				line++;
				column = 1;
			}
			else
				column++;
		}

		return $"{line}:{column}";
	}

	static IReadOnlyList<(string Name, string Text)> Corpus()
	{
		return _corpus ??= Read();
	}

	static IReadOnlyList<(string Name, string Text)>? _corpus;

	static IReadOnlyList<(string Name, string Text)> Read()
	{
		var result = new List<(string Name, string Text)>();

		foreach (var file in Files("*.gram"))
			result.Add((Relative(file), File.ReadAllText(file)));

		foreach (var file in Files("*.cs"))
		{
			var source = File.ReadAllText(file);

			// Only a cheap way past the files that cannot hold one: every spelling of the
			// attribute's name, `Gram` and `GramAttribute`, alone or qualified, has this in it.
			if (!source.Contains("Gram", StringComparison.Ordinal))
				continue;

			var index = 0;

			foreach (var attribute in CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().OfType<AttributeSyntax>())
			{
				var name = attribute.Name switch
				{
					QualifiedNameSyntax qualified   => qualified.Right.Identifier.ValueText,
					AliasQualifiedNameSyntax global => global.Name.Identifier.ValueText,
					SimpleNameSyntax simple         => simple.Identifier.ValueText,
					_                               => "",
				};

				if (name is not ("Gram" or "GramAttribute") ||
					attribute.ArgumentList?.Arguments.FirstOrDefault() is not { NameEquals: null } argument)
					continue;

				var place = $"{Relative(file)}#{++index}";

				if (Constant(argument.Expression) is not { } grammar)
				{
					Unread.Add($"{place}: {argument.Expression}");
					continue;
				}

				// A file's name, which the grammar of that file stands for.
				if (grammar.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) && grammar.IndexOf('\n') < 0)
					continue;

				result.Add((place, grammar));
			}
		}

		return result;
	}

	/// <summary>The first arguments of a <c>[Gram]</c> that are not a constant this test can read.</summary>
	static readonly List<string> Unread = [];

	/// <summary>The text of a constant string: a literal, or literals joined with <c>+</c>.</summary>
	internal static string? Constant(ExpressionSyntax expression)
	{
		return expression switch
		{
			LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression) => literal.Token.ValueText,
			ParenthesizedExpressionSyntax parenthesized => Constant(parenthesized.Expression),
			BinaryExpressionSyntax binary when binary.IsKind(SyntaxKind.AddExpression) &&
				Constant(binary.Left) is { } left && Constant(binary.Right) is { } right => left + right,
			_ => null,
		};
	}

	internal static IEnumerable<string> Files(string pattern)
	{
		foreach (var top in new[] { "src", "examples", "tests", "benchmarks" })
			foreach (var file in Directory.EnumerateFiles(Path.Combine(Root, top), pattern, SearchOption.AllDirectories))
			{
				var relative = Relative(file);

				if (relative.Split('/').Any(static part => part is "bin" or "obj" or ".build"))
					continue;

				yield return file;
			}
	}

	internal static string Relative(string file)
	{
		return Path.GetRelativePath(Root, file).Replace('\\', '/');
	}

	static string Root =>
		Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(ThisFile)!)!)!;

	static string ThisFile { get; } = FilePath();

	static string FilePath([CallerFilePath] string path = "")
	{
		return path;
	}
}
