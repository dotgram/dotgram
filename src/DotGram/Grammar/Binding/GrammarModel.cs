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
	string Name, GrammarNamespace Namespace, Decl.Rule? Declaration) : Symbol(Name)
{
	public bool IsBuiltIn => Declaration is null;

	public override string ToString() => Name;
}

/// <summary>A rule's parameter, in scope only inside that rule's body.</summary>
public sealed record ParameterSymbol(string Name, RuleSymbol Owner) : Symbol(Name);

/// <summary>One `A = B` entry in a `with (...)` header, resolved to symbols (§5, §7).</summary>
public sealed record ResolvedRebinding(RuleSymbol Left, RuleSymbol Right, Location At);

/// <summary>A name that lives on the C# side.</summary>
public sealed record CSharpSymbol(string Name) : Symbol(Name);

/// <summary>
/// One lexical namespace: the global one at the top of a file, or a `namespace` block.
/// </summary>
public sealed class GrammarNamespace(string name, GrammarNamespace? parent)
{
	readonly Dictionary<string, RuleSymbol> _rules   = [];
	readonly List<GrammarNamespace>         _nested  = [];
	readonly List<GrammarNamespace>         _imports = [];
	readonly List<string>                   _csharpImports = [];
	readonly List<ResolvedRebinding>        _ownRebindings = [];

	/// <summary>Empty for the global namespace.</summary>
	public string            Name   { get; } = name;
	public GrammarNamespace? Parent { get; } = parent;

	public IReadOnlyDictionary<string, RuleSymbol> Rules         => _rules;
	public IReadOnlyList<GrammarNamespace>         Nested        => _nested;
	public IReadOnlyList<GrammarNamespace>         Imports       => _imports;
	public IReadOnlyList<string>                   CSharpImports => _csharpImports;

	/// <summary>This namespace's own `with (...)` header, resolved, in source order (§5, §7).</summary>
	public IReadOnlyList<ResolvedRebinding> OwnRebindings => _ownRebindings;

	/// <summary>
	/// The <c>context</c> the rules declared here were written against, or null.
	/// </summary>
	/// <remarks>
	/// The contract, not the object. One grammar including another puts it in a namespace of
	/// its own, and the rules in there were written against whatever contract *that* grammar
	/// declared — which the caller's object satisfies by being assignable to it, and which
	/// the including grammar may strengthen for its own rules without changing what the
	/// included ones were compiled against (docs/next.md, "Decided: `context` is a
	/// contract"). Null anywhere no grammar declared one, including in the global namespace
	/// of a grammar that declares none at all.
	/// </remarks>
	public TypeRef? Context { get; internal set; }

	/// <summary>
	/// The layered environment this namespace and everything lexically inside it sees: the
	/// parent's <see cref="Rebindings"/>, overridden key-by-key by
	/// <see cref="OwnRebindings"/> (§11).
	/// </summary>
	public IReadOnlyDictionary<RuleSymbol, RuleSymbol> Rebindings { get; internal set; } =
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

	internal bool TryBind(ResolvedRebinding binding)
	{
		if (_ownRebindings.Exists(existing => existing.Left == binding.Left))
			return false;

		_ownRebindings.Add(binding);

		return true;
	}

	internal void Add(GrammarNamespace nested)   => _nested.Add(nested);
	internal void Import(GrammarNamespace other) => _imports.Add(other);
	internal void ImportCSharp(string name)      => _csharpImports.Add(name);

	/// <summary>
	/// Looks a name up the way §5 says: this namespace, then what it imports, then
	/// outwards. The first hit wins, which is what makes an inner declaration shadow
	/// an outer one.
	/// </summary>
	public RuleSymbol? Lookup(string name)
	{
		for (var ns = this; ns is not null; ns = ns.Parent)
		{
			if (ns._rules.TryGetValue(name, out var rule))
				return rule;

			foreach (var imported in ns._imports)
				if (imported._rules.TryGetValue(name, out var fromImport))
					return fromImport;
		}

		return null;
	}

	/// <summary>A qualified name, `Namespace.Rule`, resolved from here.</summary>
	public RuleSymbol? LookupQualified(string qualifiedName)
	{
		var separator = qualifiedName.LastIndexOf('.');

		if (separator < 0)
			return Lookup(qualifiedName);

		var ns = FindNamespace(qualifiedName.Substring(0, separator));

		return ns?._rules.TryGetValue(qualifiedName.Substring(separator + 1), out var rule) == true
			? rule
			: null;
	}

	GrammarNamespace? FindNamespace(string path)
	{
		var head = path;
		var tail = "";
		var dot  = path.IndexOf('.');

		if (dot >= 0)
		{
			head = path.Substring(0, dot);
			tail = path.Substring(dot + 1);
		}

		for (var ns = this; ns is not null; ns = ns.Parent)
			foreach (var nested in ns._nested)
				if (nested.Name == head)
					return tail.Length == 0 ? nested : nested.FindNamespace(tail);

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
/// The namespace the directive itself sits in — not <paramref name="Rule"/>'s own namespace —
/// which is what lets a `parse`/`find` declared inside a bound `namespace (...)` be remapped
/// to the specialized clone it meant (§18) once one exists.
/// </param>
/// <param name="Rebindings">
/// This directive's own `with (A = B, ...)` (§5.1), resolved and chain-followed — empty
/// when it has none. The same substitution a `namespace (...)` header applies to a whole
/// block, written directly on the one publication that needs it instead.
/// </param>
/// <param name="OwnRebindings">
/// The same header, one entry per `A = B` exactly as written — not chain-followed, so
/// each keeps its own position and its own two names rather than the target a chain of
/// several might resolve to. What a type-compatibility check reports against
/// (<see cref="GrammarNamespace.OwnRebindings"/> is the same split, for a header); what
/// specialization clones against is <see cref="Rebindings"/>.
/// </param>
public sealed record Publication(
	PublishKind Kind, RuleSymbol Rule, string MethodName, Location At, GrammarNamespace DeclaredIn,
	IReadOnlyDictionary<RuleSymbol, RuleSymbol> Rebindings,
	IReadOnlyList<ResolvedRebinding> OwnRebindings)
{
	/// <summary>The name the directive produces when it does not give one itself.</summary>
	public static string DefaultMethodName(PublishKind kind, string ruleName) =>
		(kind == PublishKind.Parse ? "Parse" : "Find") + ruleName;

	public override string ToString() => $"{Kind} {Rule.Name} -> {MethodName}";
}

/// <summary>What binding produced: a namespace tree, resolved references, diagnostics.</summary>
public sealed class GrammarModel(
	GrammarNamespace                                                       root,
	IReadOnlyDictionary<Expr, Symbol>                                      bindings,
	IReadOnlyDictionary<Expr, IReadOnlyDictionary<RuleSymbol, RuleSymbol>> withBindings,
	IReadOnlyDictionary<Expr, IReadOnlyList<ResolvedRebinding>>            withOwnRebindings,
	IReadOnlyDictionary<GrammarNamespace, RuleSymbol>                      trivia,
	IReadOnlyList<Publication>                                             publications,
	IReadOnlyList<GramDiagnostic>                                          diagnostics)
{
	public GrammarNamespace                  Root        { get; } = root;
	public IReadOnlyDictionary<Expr, Symbol> Bindings    { get; } = bindings;

	/// <summary>Each `with (...)`'s own rebindings, resolved and chain-followed (§5.1) —
	/// keyed by the <see cref="Expr.With"/> node itself, the same way <see cref="Bindings"/>
	/// is keyed by node identity rather than by value. What specialization clones against;
	/// see <see cref="WithOwnRebindings"/> for the entry-by-entry form a type-compatibility
	/// check needs instead.</summary>
	public IReadOnlyDictionary<Expr, IReadOnlyDictionary<RuleSymbol, RuleSymbol>> WithBindings { get; } = withBindings;

	/// <summary>The same headers as <see cref="WithBindings"/>, one entry per `A = B`
	/// exactly as written rather than chain-followed — the same split
	/// <see cref="Publication.OwnRebindings"/> and <see cref="GrammarNamespace.OwnRebindings"/>
	/// each make for the same reason.</summary>
	public IReadOnlyDictionary<Expr, IReadOnlyList<ResolvedRebinding>> WithOwnRebindings { get; } = withOwnRebindings;

	public IReadOnlyList<GramDiagnostic> Diagnostics { get; } = diagnostics;

	/// <summary>The public API this grammar asked for, in declaration order.</summary>
	public IReadOnlyList<Publication> Publications { get; } = publications;

	/// <summary>
	/// The declared type of the grammar's own state, or null where it declares none.
	/// </summary>
	/// <remarks>
	/// What a `context : @T` says. The type is a C# name this half never resolves — it is
	/// written into the generated signature and checked where it is written, the same as a
	/// rule's own `: @T`.
	/// </remarks>
	public TypeRef? Context { get; init; }

	/// <summary>
	/// The type every mark a <c>with state</c> site places is written in, or null where the
	/// grammar declares none.
	/// </summary>
	/// <remarks>
	/// What a `state : @T` says, resolved no further here than <see cref="Context"/> is.
	/// One type for all of them: what tells two marks apart is their value, read by the hook
	/// that cares (§7.8).
	/// </remarks>
	public TypeRef? State { get; init; }

	/// <summary>The `trivia` each namespace sees — §4.5, resolved once per namespace.</summary>
	public IReadOnlyDictionary<GrammarNamespace, RuleSymbol> Trivia { get; } = trivia;

	public bool HasErrors => Diagnostics.Count > 0;

	/// <summary>The namespace tree, then the diagnostics, in one comparable dump.</summary>
	public override string ToString()
	{
		var text = new StringBuilder();

		Write(Root, 0);

		foreach (var publication in Publications)
			text.Append("publish ").AppendEndingWith(publication.ToString());

		foreach (var diagnostic in Diagnostics)
			text.AppendEndingWith(diagnostic.ToString());

		return text.ToString().TrimEnd();

		void Write(GrammarNamespace ns, int depth)
		{
			text.Append('\t', depth).Append("namespace ").AppendEndingWith(ns.ToString());

			foreach (var import in ns.CSharpImports)
				text.Append('\t', depth + 1).Append("using @").AppendEndingWith(import);

			foreach (var import in ns.Imports)
				text.Append('\t', depth + 1).Append("using ").AppendEndingWith(import.Name);

			if (Trivia.TryGetValue(ns, out var trivia))
				text.Append('\t', depth + 1).Append("trivia = ").AppendEndingWith(trivia.Name);

			foreach (var rule in ns.Rules.Values)
				text.Append('\t', depth + 1).Append("rule ").AppendEndingWith(rule.Name);

			foreach (var nested in ns.Nested)
				Write(nested, depth + 1);
		}
	}
}
