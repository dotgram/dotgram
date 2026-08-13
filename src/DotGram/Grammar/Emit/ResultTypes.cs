using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What each rule's value is called in the emitted code.
/// </summary>
/// <remarks>
/// <para>
/// A rule that captures nothing yields the text it matched, and its type is
/// <c>string</c> — which is what every rule yielded before construction existed, so a
/// grammar without captures compiles to the same file it always did.
/// </para>
/// <para>
/// A rule with captures gets a type named after it, nested in the host class: the types
/// belong to the grammar, and nesting them keeps a grammar from claiming names in the
/// consumer's namespace. §7.3 calls for a <c>record</c>; what is emitted is a sealed
/// class with a constructor and get-only properties, because a positional record needs
/// <c>IsExternalInit</c>, and that lives in a namespace this generator must not emit
/// into (.claude/rules/emitted-code.md).
/// </para>
/// </remarks>
sealed class ResultTypes
{
	readonly Dictionary<RuleSymbol, string> _names = [];
	readonly List<RuleSymbol>               _built = [];
	readonly string                         _prefix;

	/// <param name="className">The host class, as a chain — <c>Outer.Inner</c>.</param>
	public ResultTypes(RecognitionGraph graph, string className, string? @namespace)
	{
		_prefix = "global::" + (@namespace is null ? "" : @namespace + ".") + className + ".";

		var host = SimpleNameOf(className);

		foreach (var rule in graph.Rules)
		{
			if (graph.Results[rule].Count == 0)
				continue;

			// A member may not be named after the type that contains it, and a host class
			// named after the grammar's own root rule is the ordinary case. The support
			// types emitted beside the recognizers claim their names the same way: a rule
			// called Failure gets FailureValue rather than colliding with one of ours.
			_names[rule] = rule.Name == host || rule.Name == CSharpEmitter.FailureType
				? rule.Name + "Value"
				: rule.Name;

			_built.Add(rule);
		}
	}

	/// <summary>Every rule that has a type of its own, in declaration order.</summary>
	public IReadOnlyList<RuleSymbol> Built => _built;

	/// <summary>The type's own name, or null when the rule yields text.</summary>
	public string? NameOf(RuleSymbol rule) => _names.TryGetValue(rule, out var name) ? name : null;

	/// <summary>The type as it is written where the host class is not in scope.</summary>
	public string? QualifiedOf(RuleSymbol rule) =>
		_names.TryGetValue(rule, out var name) ? _prefix + name : null;

	/// <summary>
	/// The type of a value of this rule, text included. Null is the text case too: it is
	/// what a capture of anything other than a rule holds.
	/// </summary>
	public string ValueOf(RuleSymbol? rule) => rule is null ? "string" : QualifiedOf(rule) ?? "string";

	/// <summary>The innermost class, without its type parameters.</summary>
	static string SimpleNameOf(string className)
	{
		var name = className;
		var dot  = name.LastIndexOf('.');

		if (dot >= 0)
			name = name.Substring(dot + 1);

		var angle = name.IndexOf('<');

		return angle < 0 ? name : name.Substring(0, angle);
	}

	/// <summary>The property a capture becomes: <c>scheme</c> is <c>Scheme</c> (§7.3).</summary>
	/// <param name="owner">The type it goes into — which it may not be named after.</param>
	public static string PropertyOf(ResultMember member, string owner)
	{
		var name = member.Name.Length == 0
			? member.Name
			: char.ToUpperInvariant(member.Name[0]) + member.Name.Substring(1);

		return name == owner ? name + "Value" : name;
	}

	/// <summary>The constructor parameter a capture becomes — the name as written.</summary>
	public static string ParameterOf(ResultMember member) =>
		Keywords.Contains(member.Name) ? "@" + member.Name : member.Name;

	/// <summary>
	/// C#'s reserved words. A capture may be called <c>base</c> or <c>string</c>; the
	/// parameter it becomes then has to be written <c>@base</c>.
	/// </summary>
	static readonly HashSet<string> Keywords = new(StringComparer.Ordinal)
	{
		"abstract", "as",       "base",     "bool",     "break",     "byte",      "case",
		"catch",    "char",     "checked",  "class",    "const",     "continue",  "decimal",
		"default",  "delegate", "do",       "double",   "else",      "enum",      "event",
		"explicit", "extern",   "false",    "finally",  "fixed",     "float",     "for",
		"foreach",  "goto",     "if",       "implicit", "in",        "int",       "interface",
		"internal", "is",       "lock",     "long",     "namespace", "new",       "null",
		"object",   "operator", "out",      "override", "params",    "private",   "protected",
		"public",   "readonly", "ref",      "return",   "sbyte",     "sealed",    "short",
		"sizeof",   "stackalloc", "static", "string",   "struct",    "switch",    "this",
		"throw",    "true",     "try",      "typeof",   "uint",      "ulong",     "unchecked",
		"unsafe",   "ushort",   "using",    "virtual",  "void",      "volatile",  "while",
	};
}
