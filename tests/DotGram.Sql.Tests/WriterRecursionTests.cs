using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Sql.Tests;

/// <summary>
/// No cycle of calls in a writer goes round without passing a method that checks the stack first.
/// </summary>
/// <remarks>
/// <para>
/// A writer recurses a level a node, and the parsers read trees of any depth, so every way a writer
/// can reach a method again has to pass a check that carries the walk onto a stack of its own where
/// this one runs low (<c>SqlStack</c>). The checks were placed by reading the writers, and reading
/// missed two: a grouping set inside a grouping set, and a JSON table's <c>NESTED PATH</c> inside
/// another, each reached through a method handed to <c>Each</c> rather than called. So this reads
/// the call graph from the source, with the compiler's binding of every name — a call, or a method
/// handed on as a delegate, from the method or any lambda in it — and fails on any cycle left once
/// the checked methods are taken out, naming it.
/// </para>
/// <para>
/// A method is checked when its first statement is <c>if (!SqlStack.Enough())</c>. Calls through a
/// delegate the writer did not name here are not edges; every delegate the writers call is one they
/// were handed by a method of their own, and that method's naming of it is the edge.
/// </para>
/// </remarks>
public sealed class WriterRecursionTests
{
	[Theory]
	[InlineData("SqlWriter.cs")]
	[InlineData("Standard/Sql2023Writer.cs")]
	public void Every_cycle_of_calls_passes_a_stack_check(string file)
	{
		var token = TestContext.Current.CancellationToken;

		var (compilation, trees) = Compiled();
		var tree   = trees.Single(one => one.FilePath.EndsWith(file.Replace('/', Path.DirectorySeparatorChar), StringComparison.Ordinal));
		var model  = compilation.GetSemanticModel(tree);
		var writer = model.GetDeclaredSymbol(tree.GetRoot(token).DescendantNodes().OfType<ClassDeclarationSyntax>().First(), token)!;

		var calls   = new Dictionary<IMethodSymbol, HashSet<IMethodSymbol>>(SymbolEqualityComparer.Default);
		var guarded = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

		foreach (var part in trees)
		{
			var partModel = compilation.GetSemanticModel(part);

			foreach (var method in part.GetRoot(token).DescendantNodes().OfType<MethodDeclarationSyntax>())
			{
				var symbol = partModel.GetDeclaredSymbol(method, token)!;

				if (!Within(symbol.ContainingType, writer))
					continue;

				calls[symbol] = Called(partModel, method, writer);

				if (Checks(method))
					guarded.Add(symbol);
			}
		}

		Assert.True(calls.Count > 20, $"{calls.Count} methods found in {writer.Name}: the scan is reading the wrong thing");
		Assert.NotEmpty(guarded);

		var cycling = Cycling(calls, guarded);

		Assert.True(
			cycling.Count == 0,
			$"{writer.Name} can reach these again without a stack check: {string.Join(", ", cycling)}");
	}

	/// <summary>Every method of the writer that <paramref name="method"/> names, by a call or as a delegate.</summary>
	static HashSet<IMethodSymbol> Called(SemanticModel model, MethodDeclarationSyntax method, INamedTypeSymbol writer)
	{
		var called = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);

		foreach (var name in method.DescendantNodes().OfType<SimpleNameSyntax>())
		{
			var info   = model.GetSymbolInfo(name);
			var named  = info.Symbol is { } one ? ImmutableArray.Create(one) : info.CandidateSymbols;

			foreach (var symbol in named)
			{
				if (symbol is IMethodSymbol { MethodKind: MethodKind.Ordinary } target &&
				    Within(target.ContainingType, writer))
					called.Add(target.OriginalDefinition);
			}
		}

		return called;
	}

	/// <summary>Whether a method begins with the check the writers make.</summary>
	static bool Checks(MethodDeclarationSyntax method)
	{
		return method.Body?.Statements.FirstOrDefault() is IfStatementSyntax check &&
		       check.Condition.ToString() == "!SqlStack.Enough()";
	}

	/// <summary>Every method that does not check and can reach itself through others that do not.</summary>
	static List<string> Cycling(
		Dictionary<IMethodSymbol, HashSet<IMethodSymbol>> calls, HashSet<IMethodSymbol> guarded)
	{
		var cycling = new List<string>();

		foreach (var start in calls.Keys)
		{
			if (guarded.Contains(start))
				continue;

			var seen    = new HashSet<IMethodSymbol>(SymbolEqualityComparer.Default);
			var pending = new Stack<IMethodSymbol>(calls[start]);

			while (pending.Count > 0)
			{
				var at = pending.Pop();

				if (SymbolEqualityComparer.Default.Equals(at, start))
				{
					cycling.Add(Name(start));

					break;
				}

				if (guarded.Contains(at) || !calls.TryGetValue(at, out var next) || !seen.Add(at))
					continue;

				foreach (var one in next)
					pending.Push(one);
			}
		}

		cycling.Sort(StringComparer.Ordinal);

		return cycling;
	}

	static bool Within(INamedTypeSymbol? type, INamedTypeSymbol writer)
	{
		for (var at = type; at is not null; at = at.ContainingType)
		{
			if (SymbolEqualityComparer.Default.Equals(at, writer))
				return true;
		}

		return false;
	}

	static string Name(IMethodSymbol method)
	{
		return $"{method.Name}({string.Join(", ", method.Parameters.Select(static one => one.Type.Name))})";
	}

	/// <summary>
	/// The hand-written sources of DotGram.Sql, compiled for their binding alone: the parsers the
	/// generator writes into the assembly are not here, and the errors that leaves are in code the
	/// writers do not call.
	/// </summary>
	static (CSharpCompilation Compilation, List<SyntaxTree> Trees) Compiled()
	{
		var source = Path.Combine(Root(), "src", "DotGram.Sql");
		var parse  = new CSharpParseOptions(LanguageVersion.Preview);

		var trees = Directory.EnumerateFiles(source, "*.cs", SearchOption.AllDirectories)
			.Where(static path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal) &&
			                      !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
			.Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), parse, path))
			.ToList();

		var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path));

		var compilation = CSharpCompilation.Create(
			"Writers", trees, references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		return (compilation, trees);
	}

	static string Root()
	{
		var at = new DirectoryInfo(AppContext.BaseDirectory);

		while (at is not null && !File.Exists(Path.Combine(at.FullName, "DotGram.slnx")))
			at = at.Parent;

		return at?.FullName ?? throw new InvalidOperationException("The repository root is not above the test binaries.");
	}
}
