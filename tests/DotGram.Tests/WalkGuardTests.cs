using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every recursion in <c>src/DotGram/Grammar</c> that can meet a call to another rule checks the
/// stack and moves to a fresh one when it runs low (<c>DeepStack.Low</c>), or is listed here with
/// the reason its depth is bounded.
/// </summary>
/// <remarks>
/// <para>
/// A chain of rules is as long as the grammar makes it, and a walk that follows a call by recursion
/// takes a frame per rule of it. Such walks were found one review at a time, each by a grammar
/// someone thought to write; this finds them in the sources instead. The sources of the grammar
/// half are compiled with Roslyn, every method and local function is a node, every call between
/// them an edge, and every strongly connected component with a cycle is a recursion.
/// </para>
/// <para>
/// A recursion none of whose members mentions a call — <c>Node.Call</c>, <c>Expr.Reference</c>,
/// <c>Expr.Call</c> — nor a rule's body (<c>Bodies</c>, <c>BodyOf</c>, <c>Expanded</c>, what was
/// lifted) walks one rule's own tree, and so is as deep as that tree, which the parser bounds
/// (<c>GramParser.MaxNesting</c>). A member that checks — <c>if (DeepStack.Low)</c> with a move to a
/// fresh stack under it — breaks every cycle through it, so the members that check are taken out
/// and the recursions found again in what is left; each of those that meets a call is in
/// <see cref="Bounded"/> with the reason it is bounded all the same. A new recursion that meets a
/// call fails here until it checks or is listed; an entry that no longer names a
/// recursion fails too, so the list does not outlive what it excuses.
/// </para>
/// </remarks>
public sealed class WalkGuardTests
{
	/// <summary>The recursions that meet a call and do not check the stack, by their members, and why each is bounded.</summary>
	static readonly Dictionary<string, string> Bounded = new(StringComparer.Ordinal)
	{
		["Automaton.Called | Automaton.Of | Automaton.Repeated | Automaton.Sequence | Automaton.Turn"] =
			"follows calls at most ExponentialRepetition.Deepest levels, every step charged to its budget",
		["Automaton.OneClass"] = "follows calls at most ExponentialRepetition.Deepest levels",
		["ExponentialRepetition.Collect"] = "one rule's tree; a call is a place, not followed",
		["Unfailing.Local"] = "one rule's tree; a call is answered from what was settled for it",
		["CaptureLayout.Through"] = "the rules lifted out of one rule, as deep as that rule nests (GramParser.MaxNesting)",
		["GrammarBinder.ResolveExpression"] = "one expression; a reference is resolved, not followed (GramParser.MaxNesting)",
		["GrammarNormalizer.Ethereal"] = "one tail's tree; a call is a leaf",
		["GrammarNormalizer.Called"] = "one body's tree; a call is collected, not followed",
		["GramParser.ParseDeclaration | GramParser.ParseNamespace | GramParser.ParseNamespaceWithin"] =
			"namespaces inside namespaces, counted toward GramParser.MaxNesting",
		["Language.Shortest"] = "at most Language.Deep levels",
		["Machine.LedByNothing"] = "at most three calls deep, as its callers ask",
		["Machine.Weight | Machine.WeightOfAll"] = "within Machine.Unrollable, which every level spends",
		["Retention.EndsWithEol"] = "at most sixteen levels",
		["Retention.IsTerminator"] = "at most sixteen levels",
		["SeamSplits.Walk"] = "one rule's tree; a call ends the walk",
		["Dump.Write"] = "prints one tree, for a reader",
		["Search.Expand | Search.Read | Search.Refused | Search.Then"] =
			"SharedRecursion's expansion of one continuation to its next letters: at most SharedRecursion.Nested nodes an expansion and SharedRecursion.Stack calls deep, a look listed once (Refused) and a continuation reached again not expanded again (looping)",
		["FirstSets.Following | FirstSets.Inner | FirstSets.Of"] =
			"one node's tree; a call is read off the rules' settled first sets, not entered",
		["GrammarNormalizer.BoundaryFor | GrammarNormalizer.CallTo"] =
			"a call to the built-in word asks for its boundary, a declared rule, whose call registers nothing: two levels",
		["GrammarNormalizer.Inline | GrammarNormalizer.Rebuild"] =
			"one rule's tree; a call to a forwarding rule is replaced by its sources, not entered",
		["GrammarNormalizer.ReachableFromSeed | GrammarNormalizer.Rebound | GrammarNormalizer.UnderHeader"] =
			"one level: a header's own reach is asked with composing on headers switched off, so it does not ask again",
		["Machine.CompileUnguarded"] = "one level: a node read as if braced is compiled once more inside an atomic group, which is not",
		["Machine.FlatFits | Machine.FlatValued | Machine.SiteCallee"] =
			"one level: the callees are settled first by a walk with a stack of its own, so a callee's answer is found kept",
	};

	/// <summary>The parser's own recursion, named by a member every version of it has; bounded by <c>GramParser.MaxNesting</c>.</summary>
	const string Parser = "GramParser.ParsePrefixed";

	static readonly Regex MeetsCalls = new(
		@"\b(_?[Bb]odies|BodyOf|Expanded|_lifted|Lifted)\b|Node \. Call|Expr \. Reference|Expr \. Call", RegexOptions.Compiled);

	[Fact]
	public void Every_recursion_that_meets_a_call_checks_the_stack_or_says_why_it_need_not()
	{
		var (recursions, meets) = Recursions();

		var unexcused = new List<string>();
		var excused   = new HashSet<string>(StringComparer.Ordinal);

		foreach (var members in recursions)
		{
			if (!members.Any(meets.Contains))
				continue;

			var key = string.Join(" | ", members.Select(Name).Distinct().OrderBy(static one => one, StringComparer.Ordinal));

			if (key.Contains(Parser, StringComparison.Ordinal))
				excused.Add(Parser);
			else if (Bounded.ContainsKey(key))
				excused.Add(key);
			else
				unexcused.Add(key);
		}

		Assert.True(
			unexcused.Count == 0,
			"A recursion meets a call and neither checks the stack (DeepStack.Low) nor is listed with its bound:\n" +
			string.Join("\n", unexcused));

		var stale = Bounded.Keys.Where(key => !excused.Contains(key)).ToArray();

		Assert.True(
			stale.Length == 0,
			"Listed as bounded and no longer an unchecked recursion; take it off the list:\n" + string.Join("\n", stale));

		Assert.Contains(Parser, excused);
	}

	static string Name(ISymbol symbol)
	{
		return $"{symbol.ContainingType?.Name}.{symbol.Name}";
	}

	/// <summary>
	/// The recursions of the grammar half that are left once the members which check the stack are
	/// taken out, and which members meet a call.
	/// </summary>
	/// <remarks>
	/// A member that checks breaks every cycle through it: each time round, the stack is looked at
	/// there. What is left are the cycles that go round without passing one; a group of members of
	/// which one checks is not excused as a whole, since a cycle among the others never reaches it.
	/// </remarks>
	static (List<List<ISymbol>> Recursions, HashSet<ISymbol> Meets) Recursions()
	{
		var root  = Path.Combine(SourceRoot.Directory, "src", "DotGram", "Grammar");
		var parse = new CSharpParseOptions(LanguageVersion.Preview);

		// The generated tables' writers are programs of their own, not part of the generator.
		var trees = Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
			.Where(static path => !path.EndsWith(".generate.cs", StringComparison.Ordinal))
			.Select(path => CSharpSyntaxTree.ParseText(File.ReadAllText(path), parse, path, cancellationToken: TestContext.Current.CancellationToken))
			.Append(CSharpSyntaxTree.ParseText(
				"global using System; global using System.Collections.Generic; global using System.IO; global using System.Linq; " +
				"global using System.Threading; global using System.Threading.Tasks;", parse, cancellationToken: TestContext.Current.CancellationToken))
			.ToList();

		var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Select(static path => MetadataReference.CreateFromFile(path));

		var compilation = CSharpCompilation.Create(
			"WalkGuards", trees, references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		var calls   = new Dictionary<ISymbol, HashSet<ISymbol>>(SymbolEqualityComparer.Default);
		var guarded = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
		var meets   = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

		foreach (var tree in trees)
		{
			var model = compilation.GetSemanticModel(tree);

			foreach (var declaration in tree.GetRoot(TestContext.Current.CancellationToken).DescendantNodes()
				.Where(static node => node is BaseMethodDeclarationSyntax or LocalFunctionStatementSyntax or AccessorDeclarationSyntax))
			{
				if (model.GetDeclaredSymbol(declaration, TestContext.Current.CancellationToken) is not { } member)
					continue;

				member = member.OriginalDefinition;

				if (!calls.TryGetValue(member, out var called))
					calls[member] = called = new HashSet<ISymbol>(SymbolEqualityComparer.Default);

				// Its own body: a local function inside it is a member of its own.
				var body = declaration.DescendantNodes(node => node == declaration || node is not LocalFunctionStatementSyntax).ToList();

				foreach (var node in body)
				{
					if (node is InvocationExpressionSyntax or ObjectCreationExpressionSyntax or IdentifierNameSyntax or MemberAccessExpressionSyntax)
					{
						switch (model.GetSymbolInfo(node, TestContext.Current.CancellationToken).Symbol)
						{
							case IMethodSymbol method:
								called.Add(method.ReducedFrom?.OriginalDefinition ?? method.OriginalDefinition);

								break;

							case IPropertySymbol { GetMethod: { } getter }:
								called.Add(getter.OriginalDefinition);

								break;
						}
					}

					if (Checks(node))
						guarded.Add(member);
				}

				if (MeetsCalls.IsMatch(Own(declaration)))
					meets.Add(member);
			}
		}

		foreach (var member in guarded)
			calls.Remove(member);

		return (Components(calls), meets);
	}

	/// <summary>
	/// Whether <paramref name="node"/> is a check of the stack that acts on what it finds:
	/// <c>if (DeepStack.Low)</c> with a move to a fresh stack (<c>DeepStack.Run</c>) under it. A
	/// mention of <c>DeepStack.Low</c> that does nothing with it is not one.
	/// </summary>
	static bool Checks(SyntaxNode node)
	{
		return node is IfStatementSyntax
		{
			Condition: MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "DeepStack" }, Name.Identifier.Text: "Low" },
		} check
			&& check.Statement.DescendantNodes().OfType<InvocationExpressionSyntax>().Any(static call =>
				call.Expression is MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.Text: "DeepStack" }, Name.Identifier.Text: "Run" });
	}

	/// <summary>A member's own text, without the local functions declared in it.</summary>
	static string Own(SyntaxNode declaration)
	{
		return string.Concat(declaration
			.DescendantTokens(node => node == declaration || node is not LocalFunctionStatementSyntax)
			.Select(static token => token.Text + " "));
	}

	/// <summary>The strongly connected components that hold a cycle, by Tarjan with a stack of its own.</summary>
	static List<List<ISymbol>> Components(Dictionary<ISymbol, HashSet<ISymbol>> calls)
	{
		var index   = new Dictionary<ISymbol, int>(SymbolEqualityComparer.Default);
		var low     = new Dictionary<ISymbol, int>(SymbolEqualityComparer.Default);
		var stack   = new Stack<ISymbol>();
		var on      = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
		var found   = new List<List<ISymbol>>();
		var counter = 0;

		foreach (var start in calls.Keys)
		{
			if (index.ContainsKey(start))
				continue;

			var walk = new Stack<(ISymbol Member, IEnumerator<ISymbol> Next)>();

			Enter(start);

			while (walk.Count > 0)
			{
				var (member, next) = walk.Peek();

				if (next.MoveNext())
				{
					var callee = next.Current;

					if (!calls.ContainsKey(callee))
						continue;

					if (!index.ContainsKey(callee))
						Enter(callee);
					else if (on.Contains(callee))
						low[member] = Math.Min(low[member], index[callee]);

					continue;
				}

				walk.Pop();

				if (walk.Count > 0)
					low[walk.Peek().Member] = Math.Min(low[walk.Peek().Member], low[member]);

				if (low[member] != index[member])
					continue;

				var component = new List<ISymbol>();
				ISymbol taken;

				do
				{
					taken = stack.Pop();
					on.Remove(taken);
					component.Add(taken);
				}
				while (!SymbolEqualityComparer.Default.Equals(taken, member));

				if (component.Count > 1 || calls[member].Contains(member))
					found.Add(component);
			}

			void Enter(ISymbol member)
			{
				index[member] = low[member] = counter++;
				stack.Push(member);
				on.Add(member);
				walk.Push((member, calls[member].GetEnumerator()));
			}
		}

		return found;
	}
}
