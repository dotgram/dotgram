using System;
using System.Collections.Generic;

using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Binding;

/// <summary>
/// Resolves names: which rule, parameter or C# symbol every reference means, and
/// which `Trivia` each scope sees.
/// </summary>
/// <remarks>
/// This is where §5 and §4.5 stop being prose. Scoping is lexical throughout: a rule
/// means the same thing wherever it is called, and `Trivia` is taken from where a rule
/// is declared rather than from where it is used.
/// </remarks>
public sealed class GrammarBinder
{
	public const string DuplicateRule  = "GRAM3001";
	public const string UndefinedName  = "GRAM3002";
	public const string UnknownScope   = "GRAM3003";
	public const string UnknownCSharp  = "GRAM3004";

	/// <summary>
	/// Rules every grammar has without declaring them. They live in a scope outside the
	/// global one, so a grammar can shadow any of them by declaring its own — which is
	/// exactly how whitespace handling works (§4.5).
	/// </summary>
	public static readonly string[] StandardLibrary = ["any", "none", "eol", "eof", "Trivia"];

	const string TriviaRule = "Trivia";

	readonly ISymbolResolver                      _symbols;
	readonly Dictionary<Syntax, Symbol>           _bindings = new(NodeIdentityComparer.Instance);
	readonly Dictionary<GrammarScope, RuleSymbol> _trivia   = [];
	readonly List<GramDiagnostic>                 _diagnostics = [];

	GrammarBinder(ISymbolResolver symbols) => _symbols = symbols;

	public static GrammarModel Bind(GrammarFile file, ISymbolResolver? symbols = null)
	{
		if (file is null)
			throw new ArgumentNullException(nameof(file));

		var binder   = new GrammarBinder(symbols ?? PermissiveSymbolResolver.Instance);
		var standard = binder.CreateStandardLibrary();
		var global   = new GrammarScope("", standard);

		standard.Add(global);

		// Declaring before resolving is what makes order irrelevant: a rule may refer to
		// one declared further down, which mutual recursion requires anyway (§4.3).
		binder.Declare(file.Decls, global);
		binder.ResolveImports(file.Usings, global);
		binder.ResolveTrivia(global);
		binder.Resolve(file.Decls, global);

		return new GrammarModel(global, binder._bindings, binder._trivia, binder._diagnostics);
	}

	GrammarScope CreateStandardLibrary()
	{
		var scope = new GrammarScope("<standard>", parent: null);

		foreach (var name in StandardLibrary)
			scope.TryDeclare(new RuleSymbol(name, scope, Node: null, Declaration: null));

		return scope;
	}

	void Report(string id, string message, Location at) =>
		_diagnostics.Add(new GramDiagnostic(id, message, at.Position, at.Length, GramSeverity.Error));

	// ── Pass one: declare ────────────────────────────────────────────────────────

	void Declare(IReadOnlyList<Syntax.Declaration> declarations, GrammarScope scope)
	{
		foreach (var node in declarations)
		{
			switch (node.What)
			{
				case Decl.Rule rule:

					if (!scope.TryDeclare(new RuleSymbol(rule.Name, scope, node, rule)))
						Report(
							DuplicateRule,
							$"'{rule.Name}' is already defined in this scope; put one of them in a nested scope to shadow the other.",
							node.At);

					break;

				case Decl.Scope nested:

					var child = new GrammarScope(nested.Name, scope);

					scope.Add(child);
					Declare(nested.Decls, child);
					break;
			}
		}
	}

	// ── Pass two: imports, trivia, references ────────────────────────────────────

	void ResolveImports(IReadOnlyList<Using> usings, GrammarScope scope)
	{
		foreach (var import in usings)
		{
			if (import.IsCSharp)
			{
				scope.ImportCSharp(import.Name);
				continue;
			}

			var target = FindScope(scope, import.Name);

			if (target is null)
				Report(UnknownScope, $"No scope named '{import.Name}' is in view here.", import.At);
			else
				scope.Import(target);
		}
	}

	static GrammarScope? FindScope(GrammarScope from, string name)
	{
		for (var scope = from; scope is not null; scope = scope.Parent)
			foreach (var nested in scope.Nested)
				if (nested.Name == name)
					return nested;

		return null;
	}

	/// <summary>
	/// Which `Trivia` a scope sees. Ordinary lookup — the mechanism is shadowing and
	/// nothing else (§4.5).
	/// </summary>
	void ResolveTrivia(GrammarScope scope)
	{
		if (scope.Lookup(TriviaRule) is { } trivia)
			_trivia[scope] = trivia;

		foreach (var nested in scope.Nested)
			ResolveTrivia(nested);
	}

	void Resolve(IReadOnlyList<Syntax.Declaration> declarations, GrammarScope scope)
	{
		var nestedIndex = 0;

		foreach (var node in declarations)
		{
			switch (node.What)
			{
				case Decl.Rule rule:
					ResolveRule(rule, scope);
					break;

				case Decl.Scope nested:

					var child = scope.Nested[nestedIndex++];

					ResolveImports(nested.Usings, child);
					Resolve(nested.Decls, child);
					break;

				case Decl.Publish publish when scope.LookupQualified(publish.RuleName) is null:
					Report(UndefinedName, $"No rule named '{publish.RuleName}'.", node.At);
					break;
			}
		}
	}

	void ResolveRule(Decl.Rule rule, GrammarScope scope)
	{
		var owner      = scope.Rules[rule.Name];
		var parameters = new Dictionary<string, ParameterSymbol>();

		foreach (var parameter in rule.Params)
		{
			parameters[parameter.Name] = new ParameterSymbol(parameter.Name, owner);

			if (parameter.Type is not null)
				ResolveType(parameter.Type, scope, parameters);
		}

		if (rule.Type is not null)
			ResolveType(rule.Type, scope, parameters);

		ResolveExpression(rule.Body, scope, parameters);
	}

	/// <summary>
	/// A type names a C# type, a rule, or a parameter — the last being how `: item[]`
	/// works in place of type parameters (§4.2).
	/// </summary>
	void ResolveType(TypeRef type, GrammarScope scope, Dictionary<string, ParameterSymbol> parameters)
	{
		if (type.IsCSharp || IsBuiltInCSharpType(type.Name))
		{
			if (!_symbols.TypeExists(type.Name))
				Report(UnknownCSharp, $"No C# type named '{type.Name}' is in view here.", type.At);

			return;
		}

		if (parameters.ContainsKey(type.Name) || scope.LookupQualified(type.Name) is not null)
			return;

		Report(UndefinedName, $"No rule, parameter or C# type named '{type.Name}'.", type.At);
	}

	static bool IsBuiltInCSharpType(string name) => name is
		"bool" or "byte" or "sbyte" or "char" or "decimal" or "double" or "float" or
		"int" or "uint" or "long" or "ulong" or "short" or "ushort" or "string" or
		"object" or "void";

	void ResolveExpression(Syntax node, GrammarScope scope, Dictionary<string, ParameterSymbol> parameters)
	{
		switch (node)
		{
			case Syntax.Expression(Expr.Reference reference, var at, _):
				ResolveReference(reference, node, at, scope, parameters, argumentCount: 0);
				return;

			case Syntax.Expression(Expr.Call(var target, var arguments), var at, var children):

				ResolveReference(target, node, at, scope, parameters, arguments.Count);

				for (var i = 1; i < children.Count; i++)
					ResolveExpression(children[i], scope, parameters);

				return;

			case Syntax.Expression(Expr.ElementSet(_, var items), var at, _):

				foreach (var item in items)
					if (item is Elem.Ref(var reference))
						ResolveReference(reference, node: null, at, scope, parameters, argumentCount: 0);

				return;

			// The text inside @(...) is C#, checked by the C# compiler where the
			// generator puts it. Nothing here can say anything useful about it.
			case Syntax.Expression(Expr.CSharp, _, _):
				return;
		}

		foreach (var child in node.Children)
			ResolveExpression(child, scope, parameters);
	}

	void ResolveReference(
		Expr.Reference                      reference,
		Syntax?                             node,
		Location                            at,
		GrammarScope                        scope,
		Dictionary<string, ParameterSymbol> parameters,
		int                                 argumentCount)
	{
		foreach (var typeArgument in reference.TypeArguments)
			ResolveType(typeArgument, scope, parameters);

		void Bind(Symbol symbol)
		{
			if (node is not null)
				_bindings[node] = symbol;
		}

		if (reference.IsCSharp)
		{
			if (_symbols.TryResolveMethod(reference.Name, argumentCount, out var role))
				Bind(new CSharpSymbol(reference.Name, role));
			else if (_symbols.TypeExists(reference.Name))
				Bind(new CSharpSymbol(reference.Name, Role: null));
			else
				Report(UnknownCSharp, $"No C# method or type named '{reference.Name}' is in view here.", at);

			return;
		}

		if (parameters.TryGetValue(reference.Name, out var parameter))
		{
			Bind(parameter);
			return;
		}

		if (scope.LookupQualified(reference.Name) is { } rule)
			Bind(rule);
		else
			Report(UndefinedName, $"No rule or parameter named '{reference.Name}'.", at);
	}
}
