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
	string Name, GrammarContext Context, Decl.Rule? Declaration) : Symbol(Name)
{
	public bool IsBuiltIn => Declaration is null;

	public override string ToString() => Name;
}

/// <summary>A rule's parameter, in scope only inside that rule's body.</summary>
public sealed record ParameterSymbol(string Name, RuleSymbol Owner) : Symbol(Name);

/// <summary>One `A = B` entry in a `context (...)` header, resolved to symbols (§5, §7).</summary>
public sealed record ContextRebinding(RuleSymbol Left, RuleSymbol Right, Location At);

/// <summary>A name that lives on the C# side.</summary>
public sealed record CSharpSymbol(string Name) : Symbol(Name);

/// <summary>
/// One lexical context: the global one at the top of a file, or a `context` block.
/// </summary>
public sealed class GrammarContext(string name, GrammarContext? parent)
{
	readonly Dictionary<string, RuleSymbol> _rules   = [];
	readonly List<GrammarContext>           _nested  = [];
	readonly List<GrammarContext>           _imports = [];
	readonly List<string>                   _csharpImports = [];
	readonly List<ContextRebinding>         _ownBindings   = [];

	/// <summary>Empty for the global context.</summary>
	public string          Name   { get; } = name;
	public GrammarContext? Parent { get; } = parent;

	public IReadOnlyDictionary<string, RuleSymbol> Rules         => _rules;
	public IReadOnlyList<GrammarContext>           Nested        => _nested;
	public IReadOnlyList<GrammarContext>           Imports       => _imports;
	public IReadOnlyList<string>                   CSharpImports => _csharpImports;

	/// <summary>This context's own `context (...)` header, resolved, in source order (§5, §7).</summary>
	public IReadOnlyList<ContextRebinding> OwnBindings => _ownBindings;

	/// <summary>
	/// The layered environment this context and everything lexically inside it sees: the
	/// parent's <see cref="ContextBindings"/>, overridden key-by-key by
	/// <see cref="OwnBindings"/> (§11).
	/// </summary>
	public IReadOnlyDictionary<RuleSymbol, RuleSymbol> ContextBindings { get; internal set; } =
		EmptyBindings;

	static readonly IReadOnlyDictionary<RuleSymbol, RuleSymbol> EmptyBindings =
		new Dictionary<RuleSymbol, RuleSymbol>();

	internal bool TryDeclare(RuleSymbol rule)
	{
		if (_rules.ContainsKey(rule.Name))
			return false;

		_rules.Add(rule.Name, rule);

		return true;
	}

	internal bool TryBind(ContextRebinding binding)
	{
		if (_ownBindings.Exists(existing => existing.Left == binding.Left))
			return false;

		_ownBindings.Add(binding);

		return true;
	}

	internal void Add(GrammarContext nested)  => _nested.Add(nested);
	internal void Import(GrammarContext other) => _imports.Add(other);
	internal void ImportCSharp(string name)   => _csharpImports.Add(name);

	/// <summary>
	/// Looks a name up the way §5 says: this context, then what it imports, then
	/// outwards. The first hit wins, which is what makes an inner declaration shadow
	/// an outer one.
	/// </summary>
	public RuleSymbol? Lookup(string name)
	{
		for (var context = this; context is not null; context = context.Parent)
		{
			if (context._rules.TryGetValue(name, out var rule))
				return rule;

			foreach (var imported in context._imports)
				if (imported._rules.TryGetValue(name, out var fromImport))
					return fromImport;
		}

		return null;
	}

	/// <summary>A qualified name, `Context.Rule`, resolved from here.</summary>
	public RuleSymbol? LookupQualified(string qualifiedName)
	{
		var separator = qualifiedName.LastIndexOf('.');

		if (separator < 0)
			return Lookup(qualifiedName);

		var context = FindContext(qualifiedName.Substring(0, separator));

		return context?._rules.TryGetValue(qualifiedName.Substring(separator + 1), out var rule) == true
			? rule
			: null;
	}

	GrammarContext? FindContext(string path)
	{
		var head = path;
		var tail = "";
		var dot  = path.IndexOf('.');

		if (dot >= 0)
		{
			head = path.Substring(0, dot);
			tail = path.Substring(dot + 1);
		}

		for (var context = this; context is not null; context = context.Parent)
			foreach (var nested in context._nested)
				if (nested.Name == head)
					return tail.Length == 0 ? nested : nested.FindContext(tail);

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
/// <param name="DeclaredIn">
/// The context the directive itself sits in — not <paramref name="Rule"/>'s own context —
/// which is what lets a `parse`/`find` declared inside a bound `context (...)` be remapped
/// to the specialized clone it meant (§18) once one exists.
/// </param>
/// <param name="Rebindings">
/// This directive's own `with (A = B, ...)` (§5.1), resolved — empty when it has none.
/// The same substitution a `context (...)` header applies to a whole block, written
/// directly on the one publication that needs it instead.
/// </param>
public sealed record Publication(
	PublishKind Kind, RuleSymbol Rule, string MethodName, Location At, GrammarContext DeclaredIn,
	IReadOnlyDictionary<RuleSymbol, RuleSymbol> Rebindings)
{
	/// <summary>The name the directive produces when it does not give one itself.</summary>
	public static string DefaultMethodName(PublishKind kind, string ruleName) =>
		(kind == PublishKind.Parse ? "Parse" : "Find") + ruleName;

	public override string ToString() => $"{Kind} {Rule.Name} -> {MethodName}";
}

/// <summary>What binding produced: a context tree, resolved references, diagnostics.</summary>
public sealed class GrammarModel(
	GrammarContext                                  root,
	IReadOnlyDictionary<Expr, Symbol>         bindings,
	IReadOnlyDictionary<Expr, IReadOnlyDictionary<RuleSymbol, RuleSymbol>> withBindings,
	IReadOnlyDictionary<GrammarContext, RuleSymbol> trivia,
	IReadOnlyList<Publication>                      publications,
	IReadOnlyList<GramDiagnostic>                   diagnostics)
{
	public GrammarContext                          Root        { get; } = root;
	public IReadOnlyDictionary<Expr, Symbol> Bindings    { get; } = bindings;

	/// <summary>Each `with (...)`'s own rebindings, resolved (§5.1) — keyed by the
	/// <see cref="Expr.With"/> node itself, the same way <see cref="Bindings"/> is keyed
	/// by node identity rather than by value.</summary>
	public IReadOnlyDictionary<Expr, IReadOnlyDictionary<RuleSymbol, RuleSymbol>> WithBindings { get; } = withBindings;

	public IReadOnlyList<GramDiagnostic>           Diagnostics { get; } = diagnostics;

	/// <summary>The public API this grammar asked for, in declaration order.</summary>
	public IReadOnlyList<Publication> Publications { get; } = publications;

	/// <summary>The `trivia` each context sees — §4.5, resolved once per context.</summary>
	public IReadOnlyDictionary<GrammarContext, RuleSymbol> Trivia { get; } = trivia;

	public bool HasErrors => Diagnostics.Count > 0;

	/// <summary>The context tree, then the diagnostics, in one comparable dump.</summary>
	public override string ToString()
	{
		var text = new StringBuilder();

		Write(Root, 0);

		foreach (var publication in Publications)
			text.Append("publish ").AppendEndingWith(publication.ToString());

		foreach (var diagnostic in Diagnostics)
			text.AppendEndingWith(diagnostic.ToString());

		return text.ToString().TrimEnd();

		void Write(GrammarContext context, int depth)
		{
			text.Append('\t', depth).Append("context ").AppendEndingWith(context.ToString());

			foreach (var import in context.CSharpImports)
				text.Append('\t', depth + 1).Append("using @").AppendEndingWith(import);

			foreach (var import in context.Imports)
				text.Append('\t', depth + 1).Append("using ").AppendEndingWith(import.Name);

			if (Trivia.TryGetValue(context, out var trivia))
				text.Append('\t', depth + 1).Append("trivia = ").AppendEndingWith(trivia.Name);

			foreach (var rule in context.Rules.Values)
				text.Append('\t', depth + 1).Append("rule ").AppendEndingWith(rule.Name);

			foreach (var nested in context.Nested)
				Write(nested, depth + 1);
		}
	}
}
