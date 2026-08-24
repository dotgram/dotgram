using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Binding;

/// <summary>
/// Resolves names: which rule, parameter or C# symbol every reference means, and
/// which `trivia` each context sees.
/// </summary>
/// <remarks>
/// This is where §5 and §4.5 stop being prose. Scoping is lexical throughout: a rule
/// means the same thing wherever it is called, and `trivia` is taken from where a rule
/// is declared rather than from where it is used.
/// </remarks>
public sealed class GrammarBinder
{
	public const string DuplicateRule  = "GRAM3001";
	public const string UndefinedName  = "GRAM3002";
	public const string UnknownContext = "GRAM3003";
	public const string UnknownCSharp  = "GRAM3004";
	public const string DuplicatePublication = "GRAM3005";
	public const string UnknownContextTarget      = "GRAM3006";
	public const string UnknownContextReplacement = "GRAM3007";
	public const string DuplicateContextBinding   = "GRAM3008";
	public const string ParameterizedContextBinding = "GRAM3009";
	public const string ContextBoundNameRedeclared = "GRAM3010";
	public const string CircularContextBinding    = "GRAM3011";

	/// <summary>
	/// Rules every grammar has without declaring them. They live in a context outside
	/// the global one, so a grammar can shadow any of them by declaring its own — which
	/// is exactly how whitespace handling works (§4.5).
	/// </summary>
	public static readonly string[] StandardLibrary =

		["any", "none", "eol", "eof", "trivia", "wordboundary"];

	const string TriviaRule = "trivia";

	readonly ISymbolResolver                        _symbols;
	readonly Dictionary<Expr, Symbol>             _bindings = new(NodeIdentityComparer.Instance);
	readonly Dictionary<GrammarContext, RuleSymbol> _trivia   = [];
	readonly List<Publication>                      _publications = [];
	readonly List<GramDiagnostic>                   _diagnostics  = [];

	GrammarBinder(ISymbolResolver symbols) => _symbols = symbols;

	public static GrammarModel Bind(GrammarFile file, ISymbolResolver? symbols = null)
	{
		if (file is null)
			throw new ArgumentNullException(nameof(file));

		var binder   = new GrammarBinder(symbols ?? PermissiveSymbolResolver.Instance);
		var standard = binder.CreateStandardLibrary();
		var global   = new GrammarContext("", standard);

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

	GrammarContext CreateStandardLibrary()
	{
		var context = new GrammarContext("<standard>", parent: null);

		foreach (var name in StandardLibrary)
			context.TryDeclare(new RuleSymbol(name, context, Declaration: null));

		return context;
	}

	void Report(string id, string message, Location at) =>
		_diagnostics.Add(new GramDiagnostic(id, message, at.Position, at.Length, GramSeverity.Error));

	// ── Pass one: declare ────────────────────────────────────────────────────────

	void Declare(IReadOnlyList<Decl> declarations, GrammarContext context)
	{
		foreach (var node in declarations)
		{
			switch (node)
			{
				case Decl.Rule rule:

					if (!context.TryDeclare(new RuleSymbol(rule.Name, context, rule)))
						Report(
							DuplicateRule,
							$"'{rule.Name}' is already defined in this context; put one of them in a nested context to shadow the other.",
							node.At);

					break;

				case Decl.Context nested:

					var child = new GrammarContext(nested.Name, context);

					context.Add(child);
					Declare(nested.Decls, child);
					break;
			}
		}
	}

	// ── Pass two: imports, trivia, references ────────────────────────────────────

	void ResolveImports(IReadOnlyList<Using> usings, GrammarContext context)
	{
		foreach (var import in usings)
		{
			if (import.IsCSharp)
			{
				context.ImportCSharp(import.Name);
				continue;
			}

			var target = FindContext(context, import.Name);

			if (target is null)
				Report(UnknownContext, $"No context named '{import.Name}' is in view here.", import.At);
			else
				context.Import(target);
		}
	}

	static GrammarContext? FindContext(GrammarContext from, string name)
	{
		for (var context = from; context is not null; context = context.Parent)
			foreach (var nested in context.Nested)
				if (nested.Name == name)
					return nested;

		return null;
	}

	/// <summary>
	/// Which `trivia` a context sees. Ordinary lookup — the mechanism is shadowing and
	/// nothing else (§4.5).
	/// </summary>
	void ResolveTrivia(GrammarContext context)
	{
		if (context.Lookup(TriviaRule) is { } trivia)
			_trivia[context] = trivia;

		foreach (var nested in context.Nested)
			ResolveTrivia(nested);
	}

	void Resolve(IReadOnlyList<Decl> declarations, GrammarContext context)
	{
		var nestedIndex = 0;

		foreach (var node in declarations)
		{
			switch (node)
			{
				case Decl.Rule rule:
					ResolveRule(rule, context);
					break;

				case Decl.Context nested:

					var child = context.Nested[nestedIndex++];

					ResolveContextBindings(nested, child, context);
					ResolveImports(nested.Usings, child);
					Resolve(nested.Decls, child);
					break;

				case Decl.Publish publish:

					if (context.LookupQualified(publish.RuleName) is not { } published)
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
						_publications.Add(new Publication(publish.Kind, published, method, publish.At, context));

					break;
			}
		}
	}

	// ── Contextual bindings — §5, §7, §12 ───────────────────────────────────────

	/// <summary>
	/// Resolves a `context (...)` header's rebindings against <paramref name="context"/>,
	/// the enclosing lexical environment — never against <paramref name="child"/> itself,
	/// which is what keeps a header from naming something declared in the very body it
	/// introduces. Runs for every nested context, header or not, since the §12
	/// redeclaration check has to see every level's layered environment regardless of
	/// whether this level added a binding of its own.
	/// </summary>
	void ResolveContextBindings(Decl.Context nested, GrammarContext child, GrammarContext context)
	{
		foreach (var rebinding in nested.Rebindings)
		{
			var left = context.LookupQualified(rebinding.Left);

			if (left is null)
			{
				Report(
					UnknownContextTarget,
					$"'{rebinding.Left}' cannot be contextually bound because no visible rule with that name exists.",
					rebinding.At);

				continue;
			}

			var right = context.LookupQualified(rebinding.Right);

			if (right is null)
			{
				Report(
					UnknownContextReplacement,
					$"'{rebinding.Right}' cannot replace '{rebinding.Left}' because no visible rule with that name exists.",
					rebinding.At);

				continue;
			}

			if (left.Declaration is { Params.Count: > 0 })
			{
				Report(
					ParameterizedContextBinding,
					$"'{rebinding.Left}' cannot be contextually bound: parameterized rules are not supported in a context header yet.",
					rebinding.At);

				continue;
			}

			if (right.Declaration is { Params.Count: > 0 })
			{
				Report(
					ParameterizedContextBinding,
					$"'{rebinding.Right}' cannot replace '{rebinding.Left}': parameterized rules are not supported in a context header yet.",
					rebinding.At);

				continue;
			}

			if (!child.TryBind(new ContextRebinding(left, right, rebinding.At)))
				Report(
					DuplicateContextBinding,
					$"'{rebinding.Left}' is bound more than once in this context.",
					rebinding.At);
		}

		child.ContextBindings = ChainResolve(context.ContextBindings, child.OwnBindings);

		foreach (var rule in child.Rules.Values)
			if (child.ContextBindings.Keys.Any(bound => bound.Name == rule.Name))
				Report(
					ContextBoundNameRedeclared,
					$"Rule '{rule.Name}' is contextually bound in the active context and cannot be redeclared. Use a nested context binding to replace it.",
					rule.Declaration!.At);
	}

	/// <summary>
	/// Layers <paramref name="ownBindings"/> over <paramref name="inherited"/>, chain-
	/// following each of this level's own bindings so that a header like
	/// <c>context (A = B, B = D)</c> resolves <c>A</c> straight to <c>D</c> — §8's "a
	/// chain of rebindings composes" done once here, rather than by repeated lookup
	/// wherever a binding is used. An inherited entry is already fully resolved by the
	/// level that produced it, so it needs no re-following of its own.
	/// </summary>
	IReadOnlyDictionary<RuleSymbol, RuleSymbol> ChainResolve(
		IReadOnlyDictionary<RuleSymbol, RuleSymbol> inherited, IReadOnlyList<ContextRebinding> ownBindings)
	{
		if (ownBindings.Count == 0)
			return inherited;

		var raw      = new Dictionary<RuleSymbol, RuleSymbol>();
		var resolved = new Dictionary<RuleSymbol, RuleSymbol>();

		foreach (var pair in inherited)
		{
			raw[pair.Key]      = pair.Value;
			resolved[pair.Key] = pair.Value;
		}

		foreach (var binding in ownBindings)
			raw[binding.Left] = binding.Right;

		foreach (var binding in ownBindings)
			resolved[binding.Left] = Follow(binding, raw);

		return resolved;
	}

	RuleSymbol Follow(ContextRebinding binding, Dictionary<RuleSymbol, RuleSymbol> raw)
	{
		var visited = new HashSet<RuleSymbol> { binding.Left };
		var current = binding.Right;

		while (raw.TryGetValue(current, out var next))
		{
			if (!visited.Add(current))
			{
				Report(
					CircularContextBinding,
					$"'{binding.Left}' is contextually bound in a cycle through '{current}'.",
					binding.At);

				return current;
			}

			current = next;
		}

		return current;
	}

	void ResolveRule(Decl.Rule rule, GrammarContext context)
	{
		var owner      = context.Rules[rule.Name];
		var parameters = new Dictionary<string, ParameterSymbol>();

		foreach (var parameter in rule.Params)
		{
			parameters[parameter.Name] = new ParameterSymbol(parameter.Name, owner);

			if (parameter.Type is not null)
				ResolveType(parameter.Type, context, parameters);
		}

		if (rule.Type is not null)
			ResolveType(rule.Type, context, parameters);

		// `when` and `=>` see the rule's captures as values, so they have to be in view
		// before its body is resolved — and the whole body's, since a `=>` at the end
		// names what the front of it captured.
		_captures.Clear();
		_captures.Add("parserText");

		Captures(rule.Body);

		ResolveExpression(rule.Body, context, parameters);
	}

	readonly HashSet<string> _captures = new(StringComparer.Ordinal);

	void Captures(Expr expression)
	{
		if (expression is Expr.Capture(var name, _))
			_captures.Add(name);

		foreach (var child in Dump.Children(expression))
			Captures(child);
	}

	/// <summary>However §4.1 case 4's own type is written.</summary>
	static bool IsSourceSpan(string name) =>
		name is "SourceSpan" or "DotGram.SourceSpan" or "global::DotGram.SourceSpan";

	/// <summary>
	/// A type names a C# type, a rule, or a parameter — the last being how `: item[]`
	/// works in place of type parameters (§4.2).
	/// </summary>
	void ResolveType(TypeRef type, GrammarContext context, Dictionary<string, ParameterSymbol> parameters)
	{
		if (type.IsCSharp || IsBuiltInCSharpType(type.Name))
		{
			// The one type the notation names itself (§4.1 case 4). It is not in the
			// consumer's compilation to be found, because it is emitted into the host class
			// this grammar is about to become — looking for it would be looking for the
			// answer inside the question.
			if (IsSourceSpan(type.Name))
				return;

			if (!TypeInView(type.Name, context))
				Report(UnknownCSharp, $"No C# type named '{type.Name}' is in view here.", type.At);

			return;
		}

		if (parameters.ContainsKey(type.Name) || context.LookupQualified(type.Name) is not null)
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
	bool TypeInView(string name, GrammarContext context)
	{
		if (_symbols.TypeExists(name))
			return true;

		for (var at = context; at is not null; at = at.Parent)
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
		GrammarContext context,
		Dictionary<string, ParameterSymbol> parameters,
		bool csharpValue = false)
	{
		switch (expression)
		{
			case Expr.Reference reference:
				ResolveReference(reference, reference, context, parameters, csharpValue);
				return;

			case Expr.Construct(var pattern, var value):
				ResolveExpression(pattern, context, parameters);
				ResolveExpression(value, context, parameters, csharpValue: true);
				return;

			case Expr.Guard(var guarded):
				ResolveExpression(guarded, context, parameters, csharpValue: true);
				return;

			case Expr.Call(var target, var arguments):
				ResolveReference(target, expression, context, parameters, csharpValue);

				foreach (var argument in arguments)
					ResolveExpression(argument, context, parameters, csharpValue);

				return;

			case Expr.ElementSet(_, var items):

				// Bound to the reference itself, not dropped: normalization has to merge
				// what it names into the set, and for that it needs to know what it is.
				foreach (var item in items)
					if (item is Elem.Ref(var reference))
						ResolveReference(reference, bind: reference, context, parameters);

				return;

			// The text inside @(...) is C#, checked by the C# compiler where the
			// generator puts it. Nothing here can say anything useful about it.
			case Expr.CSharp:
				return;
		}

		foreach (var child in Dump.Children(expression))
			ResolveExpression(child, context, parameters, csharpValue);
	}

	void ResolveReference(
		Expr.Reference                      reference,
		Expr?                               bind,
		GrammarContext                      context,
		Dictionary<string, ParameterSymbol> parameters,
		bool                                csharpValue = false)
	{
		var at = reference.At;

		if (!csharpValue || !reference.IsCSharp)
			foreach (var typeArgument in reference.TypeArguments)
				ResolveType(typeArgument, context, parameters);

		void Bind(Symbol symbol)
		{
			if (bind is not null)
				_bindings[bind] = symbol;
		}

		if (reference.IsCSharp)
		{
			// A `when` or `=>` value is C# owned by the consumer. Preserve it and let
			// their compiler resolve names, overloads, generic arguments and types.
			if (csharpValue)
				return;

			Bind(new CSharpSymbol(reference.Name));

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

		if (context.LookupQualified(reference.Name) is { } rule)
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
