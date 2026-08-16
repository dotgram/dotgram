using System;
using System.Collections.Generic;
using System.Text;

using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Binding;

/// <summary>A named thing a reference can resolve to.</summary>
public abstract record Symbol(string Name);

/// <summary>
/// A rule. Declaration is null for the standard library, which has symbols but no
/// source.
/// </summary>
public sealed record RuleSymbol(
	string Name, GrammarScope Scope, Decl.Rule? Declaration) : Symbol(Name)
{
	public bool IsBuiltIn => Declaration is null;

	public override string ToString() => Name;
}

/// <summary>A rule's parameter, in scope only inside that rule's body.</summary>
public sealed record ParameterSymbol(string Name, RuleSymbol Owner) : Symbol(Name);

/// <summary>
/// A name that lives on the C# side. Resolution beyond "it exists" happens later —
/// this stage only records what the resolver was asked and what it answered.
/// </summary>
/// <param name="Missing">
/// The host has no such method, and the grammar is somewhere its signature can be worked
/// out — so one is declared for the author to implement (§7.4) rather than reported.
/// </param>
public sealed record CSharpSymbol(string Name, MethodRole? Role, bool Missing = false) : Symbol(Name);

/// <summary>
/// One lexical scope: the global one at the top of a file, or a `scope` block.
/// </summary>
public sealed class GrammarScope(string name, GrammarScope? parent)
{
	readonly Dictionary<string, RuleSymbol> _rules   = [];
	readonly List<GrammarScope>             _nested  = [];
	readonly List<GrammarScope>             _imports = [];
	readonly List<string>                   _csharpImports = [];

	/// <summary>Empty for the global scope.</summary>
	public string        Name   { get; } = name;
	public GrammarScope? Parent { get; } = parent;

	public IReadOnlyDictionary<string, RuleSymbol> Rules         => _rules;
	public IReadOnlyList<GrammarScope>             Nested        => _nested;
	public IReadOnlyList<GrammarScope>             Imports       => _imports;
	public IReadOnlyList<string>                   CSharpImports => _csharpImports;

	internal bool TryDeclare(RuleSymbol rule)
	{
		if (_rules.ContainsKey(rule.Name))
			return false;

		_rules.Add(rule.Name, rule);

		return true;
	}

	internal void Add(GrammarScope nested)  => _nested.Add(nested);
	internal void Import(GrammarScope other) => _imports.Add(other);
	internal void ImportCSharp(string name)  => _csharpImports.Add(name);

	/// <summary>
	/// Looks a name up the way §5 says: this scope, then what it imports, then
	/// outwards. The first hit wins, which is what makes an inner declaration shadow
	/// an outer one.
	/// </summary>
	public RuleSymbol? Lookup(string name)
	{
		for (var scope = this; scope is not null; scope = scope.Parent)
		{
			if (scope._rules.TryGetValue(name, out var rule))
				return rule;

			foreach (var imported in scope._imports)
				if (imported._rules.TryGetValue(name, out var fromImport))
					return fromImport;
		}

		return null;
	}

	/// <summary>A qualified name, `Scope.Rule`, resolved from here.</summary>
	public RuleSymbol? LookupQualified(string qualifiedName)
	{
		var separator = qualifiedName.LastIndexOf('.');

		if (separator < 0)
			return Lookup(qualifiedName);

		var scope = FindScope(qualifiedName.Substring(0, separator));

		return scope?._rules.TryGetValue(qualifiedName.Substring(separator + 1), out var rule) == true
			? rule
			: null;
	}

	GrammarScope? FindScope(string path)
	{
		var head = path;
		var tail = "";
		var dot  = path.IndexOf('.');

		if (dot >= 0)
		{
			head = path.Substring(0, dot);
			tail = path.Substring(dot + 1);
		}

		for (var scope = this; scope is not null; scope = scope.Parent)
			foreach (var nested in scope._nested)
				if (nested.Name == head)
					return tail.Length == 0 ? nested : nested.FindScope(tail);

		return null;
	}

	public override string ToString() => Name.Length == 0 ? "<global>" : Name;
}

/// <summary>
/// Identity comparison for syntax nodes. They are records, so their built-in equality
/// is by value — two references to the same rule from different places would collide
/// as dictionary keys. netstandard2.0 has no <c>ReferenceEqualityComparer</c> of its
/// own.
/// </summary>
sealed class NodeIdentityComparer : IEqualityComparer<Expr>
{
	public static readonly NodeIdentityComparer Instance = new();

	public bool Equals(Expr? x, Expr? y) => ReferenceEquals(x, y);

	public int GetHashCode(Expr node) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(node);
}

/// <summary>
/// A rule a grammar asked to be reachable from C# — one <c>parse</c> / <c>match</c> /
/// <c>find</c> / <c>find all</c> directive, resolved (§6).
/// </summary>
/// <param name="At">
/// Where the directive is written, which is where anything said about what it did or did
/// not produce belongs — the rule is fine, the directive is what asked for the method.
/// </param>
public sealed record Publication(PublishKind Kind, RuleSymbol Rule, string MethodName, Location At)
{
	/// <summary>The name the directive produces when it does not give one itself.</summary>
	public static string DefaultMethodName(PublishKind kind, string ruleName) =>
		(kind == PublishKind.Parse ? "Parse" : "Find") + ruleName;

	public override string ToString() => $"{Kind} {Rule.Name} -> {MethodName}";
}

/// <summary>What binding produced: a scope tree, resolved references, diagnostics.</summary>
public sealed class GrammarModel(
	GrammarScope                              root,
	IReadOnlyDictionary<Expr, Symbol>   bindings,
	IReadOnlyDictionary<GrammarScope, RuleSymbol> trivia,
	IReadOnlyList<Publication>                publications,
	IReadOnlyList<GramDiagnostic>             diagnostics)
{
	public GrammarScope                            Root        { get; } = root;
	public IReadOnlyDictionary<Expr, Symbol> Bindings    { get; } = bindings;
	public IReadOnlyList<GramDiagnostic>           Diagnostics { get; } = diagnostics;

	/// <summary>The public API this grammar asked for, in declaration order.</summary>
	public IReadOnlyList<Publication> Publications { get; } = publications;

	/// <summary>The `Trivia` each scope sees — §4.5, resolved once per scope.</summary>
	public IReadOnlyDictionary<GrammarScope, RuleSymbol> Trivia { get; } = trivia;

	public bool HasErrors => Diagnostics.Count > 0;

	/// <summary>The scope tree, then the diagnostics, in one comparable dump.</summary>
	public override string ToString()
	{
		var text = new StringBuilder();

		Write(Root, 0);

		foreach (var publication in Publications)
			text.Append("publish ").AppendEndingWith(publication.ToString());

		foreach (var diagnostic in Diagnostics)
			text.AppendEndingWith(diagnostic.ToString());

		return text.ToString().TrimEnd();

		void Write(GrammarScope scope, int depth)
		{
			text.Append('\t', depth).Append("scope ").AppendEndingWith(scope.ToString());

			foreach (var import in scope.CSharpImports)
				text.Append('\t', depth + 1).Append("using @").AppendEndingWith(import);

			foreach (var import in scope.Imports)
				text.Append('\t', depth + 1).Append("using ").AppendEndingWith(import.Name);

			if (Trivia.TryGetValue(scope, out var trivia))
				text.Append('\t', depth + 1).Append("trivia = ").AppendEndingWith(trivia.Name);

			foreach (var rule in scope.Rules.Values)
				text.Append('\t', depth + 1).Append("rule ").AppendEndingWith(rule.Name);

			foreach (var nested in scope.Nested)
				Write(nested, depth + 1);
		}
	}
}
