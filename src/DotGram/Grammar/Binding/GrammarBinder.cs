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
	public const string DuplicatePublication = "GRAM3005";

	/// <summary>
	/// Rules every grammar has without declaring them. They live in a scope outside the
	/// global one, so a grammar can shadow any of them by declaring its own — which is
	/// exactly how whitespace handling works (§4.5).
	/// </summary>
	public static readonly string[] StandardLibrary =

		["any", "none", "eol", "eof", "Trivia", "KeywordBoundary"];

	const string TriviaRule = "Trivia";

	readonly ISymbolResolver                      _symbols;
	readonly Dictionary<Expr, Symbol>           _bindings = new(NodeIdentityComparer.Instance);
	readonly Dictionary<GrammarScope, RuleSymbol> _trivia   = [];
	readonly List<Publication>                    _publications = [];
	readonly List<GramDiagnostic>                 _diagnostics  = [];

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

		return new GrammarModel(
			global, binder._bindings, binder._trivia, binder._publications, binder._diagnostics);
	}

	GrammarScope CreateStandardLibrary()
	{
		var scope = new GrammarScope("<standard>", parent: null);

		foreach (var name in StandardLibrary)
			scope.TryDeclare(new RuleSymbol(name, scope, Declaration: null));

		return scope;
	}

	void Report(string id, string message, Location at) =>
		_diagnostics.Add(new GramDiagnostic(id, message, at.Position, at.Length, GramSeverity.Error));

	// ── Pass one: declare ────────────────────────────────────────────────────────

	void Declare(IReadOnlyList<Decl> declarations, GrammarScope scope)
	{
		foreach (var node in declarations)
		{
			switch (node)
			{
				case Decl.Rule rule:

					if (!scope.TryDeclare(new RuleSymbol(rule.Name, scope, rule)))
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

	void Resolve(IReadOnlyList<Decl> declarations, GrammarScope scope)
	{
		var nestedIndex = 0;

		foreach (var node in declarations)
		{
			switch (node)
			{
				case Decl.Rule rule:
					ResolveRule(rule, scope);
					break;

				case Decl.Scope nested:

					var child = scope.Nested[nestedIndex++];

					ResolveImports(nested.Usings, child);
					Resolve(nested.Decls, child);
					break;

				case Decl.Publish publish:

					if (scope.LookupQualified(publish.RuleName) is not { } published)
					{
						Report(UndefinedName, $"No rule named '{publish.RuleName}'.", publish.At);
						break;
					}

					var method = publish.Alias ?? Publication.DefaultMethodName(publish.Kind, published.Name);

					// Two directives producing one name would generate two methods with the
					// same signature — a C# error in the consumer's build, pointing at code
					// they never wrote. Better to say it here.
					if (_publications.Find(other => other.MethodName == method) is { } clash)
						Report(
							DuplicatePublication,
							$"'{method}' is already published by '{clash.Kind} {clash.Rule.Name}'; use 'as' to give one of them another name.",
							publish.At);
					else
						_publications.Add(new Publication(publish.Kind, published, method, publish.At));

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

		// `where` and `=>` see the rule's captures as values, so they have to be in view
		// before its body is resolved — and the whole body's, since a `=>` at the end
		// names what the front of it captured.
		_captures.Clear();
		_captures.Add("parserText");

		Captures(rule.Body);

		ResolveExpression(rule.Body, scope, parameters);
	}

	readonly HashSet<string> _captures = new(StringComparer.Ordinal);

	void Captures(Expr expression)
	{
		if (expression is Expr.Capture(var name, _))
			_captures.Add(name);

		foreach (var child in Dump.Children(expression))
			Captures(child);
	}

	/// <summary>
	/// A type names a C# type, a rule, or a parameter — the last being how `: item[]`
	/// works in place of type parameters (§4.2).
	/// </summary>
	void ResolveType(TypeRef type, GrammarScope scope, Dictionary<string, ParameterSymbol> parameters)
	{
		if (type.IsCSharp || IsBuiltInCSharpType(type.Name))
		{
			if (!TypeInView(type.Name, scope))
				Report(UnknownCSharp, $"No C# type named '{type.Name}' is in view here.", type.At);

			return;
		}

		if (parameters.ContainsKey(type.Name) || scope.LookupQualified(type.Name) is not null)
			return;

		Report(UndefinedName, $"No rule, parameter or C# type named '{type.Name}'.", type.At);
	}

	/// <summary>
	/// A C# type as C# itself would find it: by the name written, then under each
	/// <c>@using</c> in view, outwards.
	/// </summary>
	/// <remarks>
	/// The search is here rather than in the resolver because what is imported is the
	/// grammar's business — the host is asked only whether one whole name exists, which
	/// is all <see cref="ISymbolResolver"/> knows how to answer. A name that resolves
	/// through an import is emitted as it was written, and the generated file carries the
	/// same <c>using</c> directives, so it stands there too.
	/// </remarks>
	bool TypeInView(string name, GrammarScope scope)
	{
		if (_symbols.TypeExists(name))
			return true;

		for (var at = scope; at is not null; at = at.Parent)
			foreach (var import in at.CSharpImports)
				if (_symbols.TypeExists(import + "." + name))
					return true;

		return false;
	}

	static bool IsBuiltInCSharpType(string name) => name is
		"bool" or "byte" or "sbyte" or "char" or "decimal" or "double" or "float" or
		"int" or "uint" or "long" or "ulong" or "short" or "ushort" or "string" or
		"object" or "void";

	void ResolveExpression(
		Expr expression,
		GrammarScope scope,
		Dictionary<string, ParameterSymbol> parameters,
		bool csharpValue = false)
	{
		switch (expression)
		{
			case Expr.Reference reference:
				ResolveReference(reference, reference, scope, parameters, argumentCount: 0, csharpValue);
				return;

			case Expr.Construct(var pattern, var value):
				ResolveExpression(pattern, scope, parameters);
				ResolveExpression(value, scope, parameters, csharpValue: true);
				return;

			case Expr.Guard(var guarded):
				ResolveExpression(guarded, scope, parameters, csharpValue: true);
				return;

			case Expr.Call(var target, var arguments):
				ResolveReference(target, expression, scope, parameters, arguments.Count, csharpValue);

				foreach (var argument in arguments)
					ResolveExpression(argument, scope, parameters, csharpValue);

				return;

			case Expr.ElementSet(_, var items):

				// Bound to the reference itself, not dropped: normalization has to merge
				// what it names into the set, and for that it needs to know what it is.
				foreach (var item in items)
					if (item is Elem.Ref(var reference))
						ResolveReference(reference, bind: reference, scope, parameters, argumentCount: 0);

				return;

			// The text inside @(...) is C#, checked by the C# compiler where the
			// generator puts it. Nothing here can say anything useful about it.
			case Expr.CSharp:
				return;
		}

		foreach (var child in Dump.Children(expression))
			ResolveExpression(child, scope, parameters, csharpValue);
	}

	void ResolveReference(
		Expr.Reference                      reference,
		Expr?                               bind,
		GrammarScope                        scope,
		Dictionary<string, ParameterSymbol> parameters,
		int                                 argumentCount,
		bool                                csharpValue = false)
	{
		var at = reference.At;

		if (!csharpValue || !reference.IsCSharp)
			foreach (var typeArgument in reference.TypeArguments)
				ResolveType(typeArgument, scope, parameters);

		void Bind(Symbol symbol)
		{
			if (bind is not null)
				_bindings[bind] = symbol;
		}

		if (reference.IsCSharp)
		{
			// A `where` or `=>` value is C# owned by the consumer. Preserve it and let
			// their compiler resolve names, overloads, generic arguments and types.
			if (csharpValue)
				return;

			if (_symbols.TryResolveMethod(reference.Name, argumentCount, out var role))
				Bind(new CSharpSymbol(reference.Name, role));
			else if (TypeInView(reference.Name, scope))
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

		// A capture of the rule being resolved, or one of the names §7.3 supplies. What
		// they are is settled at emission, where they become the parameters of the method
		// a `=>` becomes; here it is enough that they are not undefined.
		if (_captures.Contains(reference.Name))
			return;

		if (scope.LookupQualified(reference.Name) is { } rule)
		{
			Bind(rule);
		}
		else
		{
			// A dotted name is a C# one nine times in ten — `CultureInfo.InvariantCulture`
			// written where the grammar expects one of its own. §2 makes no exception for
			// argument lists, so the fix is the one character that switches namespace, and
			// saying which character is the difference between a message and a message
			// worth reading.
			Report(
				UndefinedName,
				$"No rule, parameter or capture named '{reference.Name}'." +
				(reference.Name.Contains(".")
					? $" A C# name is reached with '@' — '@{reference.Name}' (docs/syntax.md §2)."
					: ""),
				at);
		}
	}
}
