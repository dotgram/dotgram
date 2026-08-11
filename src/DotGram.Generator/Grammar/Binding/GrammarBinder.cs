using System;
using System.Collections.Generic;

using DotGram.Grammar.Syntax;

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
	public const string DuplicateRule    = "GRAM3001";
	public const string UndefinedName    = "GRAM3002";
	public const string UnknownScope     = "GRAM3003";
	public const string UnknownCSharp    = "GRAM3004";
	public const string TriviaNotARule   = "GRAM3005";

	/// <summary>
	/// Rules every grammar has without declaring them. They live in a scope outside the
	/// global one, so a grammar can shadow any of them by declaring its own — which is
	/// exactly how whitespace handling works (§4.5).
	/// </summary>
	public static readonly string[] StandardLibrary = ["any", "none", "eol", "eof", "Trivia"];

	const string TriviaRule = "Trivia";

	readonly ISymbolResolver                      _symbols;
	readonly Dictionary<SyntaxNode, Symbol>       _bindings = new(NodeIdentityComparer.Instance);
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
		binder.Declare(file.Usings, file.Declarations, global);
		binder.ResolveImports(file.Usings, global);
		binder.ResolveTrivia(global);
		binder.Resolve(file.Declarations, global);

		return new GrammarModel(global, binder._bindings, binder._trivia, binder._diagnostics);
	}

	GrammarScope CreateStandardLibrary()
	{
		var scope = new GrammarScope("<standard>", parent: null);

		foreach (var name in StandardLibrary)
			scope.TryDeclare(new RuleSymbol(name, scope, Declaration: null));

		return scope;
	}

	void Report(string id, string message, SyntaxNode node) =>
		_diagnostics.Add(new GramDiagnostic(id, message, node.Position, node.Length, GramSeverity.Error));

	// ── Pass one: declare ────────────────────────────────────────────────────────

	void Declare(IReadOnlyList<UsingDirective> usings, IReadOnlyList<Declaration> declarations, GrammarScope scope)
	{
		_ = usings;

		foreach (var declaration in declarations)
		{
			switch (declaration)
			{
				case RuleDeclaration rule when !scope.TryDeclare(new RuleSymbol(rule.Name, scope, rule)):
					Report(
						DuplicateRule,
						$"'{rule.Name}' is already defined in this scope; put one of them in a nested scope to shadow the other.",
						rule);
					break;

				case RuleDeclaration:
					break;

				case ScopeDeclaration nested:
					var child = new GrammarScope(nested.Name, scope);

					scope.Add(child);
					Declare(nested.Usings, nested.Declarations, child);
					break;
			}
		}
	}

	// ── Pass two: imports, trivia, references ────────────────────────────────────

	void ResolveImports(IReadOnlyList<UsingDirective> usings, GrammarScope scope)
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
				Report(UnknownScope, $"No scope named '{import.Name}' is in view here.", import);
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
		var trivia = scope.Lookup(TriviaRule);

		if (trivia is not null)
			_trivia[scope] = trivia;

		foreach (var nested in scope.Nested)
			ResolveTrivia(nested);
	}

	void Resolve(IReadOnlyList<Declaration> declarations, GrammarScope scope)
	{
		var nestedIndex = 0;

		foreach (var declaration in declarations)
		{
			switch (declaration)
			{
				case RuleDeclaration rule:
					ResolveRule(rule, scope);
					break;

				case ScopeDeclaration nested:
					var child = scope.Nested[nestedIndex++];

					ResolveImports(nested.Usings, child);
					Resolve(nested.Declarations, child);
					break;

				case PublicationDirective publication:
					if (scope.LookupQualified(publication.RuleName) is null)
						Report(UndefinedName, $"No rule named '{publication.RuleName}'.", publication);
					break;
			}
		}
	}

	void ResolveRule(RuleDeclaration rule, GrammarScope scope)
	{
		var owner      = scope.Rules[rule.Name];
		var parameters = new Dictionary<string, ParameterSymbol>();

		foreach (var parameter in rule.Parameters)
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
	void ResolveType(TypeReference type, GrammarScope scope, Dictionary<string, ParameterSymbol> parameters)
	{
		if (type.IsCSharp || IsBuiltInCSharpType(type.Name))
		{
			if (!_symbols.TypeExists(type.Name))
				Report(UnknownCSharp, $"No C# type named '{type.Name}' is in view here.", type);
			else
				_bindings[type] = new CSharpSymbol(type.Name, Role: null);

			return;
		}

		if (parameters.TryGetValue(type.Name, out var parameter))
		{
			_bindings[type] = parameter;
			return;
		}

		var rule = scope.LookupQualified(type.Name);

		if (rule is null)
			Report(UndefinedName, $"No rule, parameter or C# type named '{type.Name}'.", type);
		else
			_bindings[type] = rule;
	}

	static bool IsBuiltInCSharpType(string name) => name is
		"bool" or "byte" or "sbyte" or "char" or "decimal" or "double" or "float" or
		"int" or "uint" or "long" or "ulong" or "short" or "ushort" or "string" or
		"object" or "void";

	void ResolveExpression(SyntaxNode node, GrammarScope scope, Dictionary<string, ParameterSymbol> parameters)
	{
		switch (node)
		{
			case ReferenceExpression reference:
				ResolveReference(reference, scope, parameters, argumentCount: 0);
				break;

			case CallExpression call:
				ResolveReference(call.Target, scope, parameters, call.Arguments.Count);

				foreach (var argument in call.Arguments)
					ResolveExpression(argument, scope, parameters);

				return;

			case TypeReference type:
				ResolveType(type, scope, parameters);
				return;

			// The text inside @(...) is C#, checked by the C# compiler where the
			// generator puts it. Nothing here can say anything useful about it.
			case CSharpExpression:
				return;
		}

		foreach (var child in node.Children)
			ResolveExpression(child, scope, parameters);
	}

	void ResolveReference(
		ReferenceExpression                 reference,
		GrammarScope                        scope,
		Dictionary<string, ParameterSymbol> parameters,
		int                                 argumentCount)
	{
		foreach (var typeArgument in reference.TypeArguments)
			ResolveType(typeArgument, scope, parameters);

		if (reference.IsCSharp)
		{
			if (_symbols.TryResolveMethod(reference.Name, argumentCount, out var role))
				_bindings[reference] = new CSharpSymbol(reference.Name, role);
			else if (_symbols.TypeExists(reference.Name))
				_bindings[reference] = new CSharpSymbol(reference.Name, Role: null);
			else
				Report(UnknownCSharp, $"No C# method or type named '{reference.Name}' is in view here.", reference);

			return;
		}

		if (parameters.TryGetValue(reference.Name, out var parameter))
		{
			_bindings[reference] = parameter;
			return;
		}

		var rule = scope.LookupQualified(reference.Name);

		if (rule is null)
			Report(UndefinedName, $"No rule or parameter named '{reference.Name}'.", reference);
		else
			_bindings[reference] = rule;
	}
}
