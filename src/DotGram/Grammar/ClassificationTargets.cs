using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Parsing;

namespace DotGram.Grammar;

/// <summary>What a classification target is short of, where it is short of anything.</summary>
enum ClassificationTargetProblem
{
	None,
	Malformed,
	UnknownRule,
	AmbiguousRule,
	UnknownCapture,
	Duplicate,
}

/// <summary>
/// One target of a <c>[GramClassify]</c>, bound to the rule — and the capture — of the grammar it names,
/// or the reason it is not.
/// </summary>
/// <param name="RulePosition">Where the rule's name is declared; meaningful where <see cref="Problem"/> is none.</param>
/// <param name="CapturePosition">Where the capture is first written in the rule, for a <c>Rule.capture</c> target.</param>
readonly record struct ClassificationTarget(
	string                      Target,
	ClassificationTargetProblem Problem,
	string?                     Message,
	int                         RulePosition,
	int?                        CapturePosition)
{
	/// <summary>The identifier a problem is reported under, or null where there is none.</summary>
	public string? Id => ClassificationTargets.IdOf(Problem);
}

/// <summary>
/// Binds the targets <c>[GramClassify]</c> names to the grammar's own rules and captures, as the grammar is
/// written and before anything is lowered: the one binder the build and the editor both report from, so that
/// GRAM6001–GRAM6005 say the same thing, at the same place, in both.
/// </summary>
/// <remarks>
/// A target is <c>Rule</c> or <c>Rule.capture</c>. A rule is looked for by its name wherever it is declared,
/// a namespace's included, and is ambiguous where two answer to it; a capture is one written in the rule
/// itself, not inside a constructing group of it, which keeps its captures to itself.
/// </remarks>
static class ClassificationTargets
{
	public const string Malformed      = "GRAM6001";
	public const string UnknownRule    = "GRAM6002";
	public const string AmbiguousRule  = "GRAM6003";
	public const string UnknownCapture = "GRAM6004";
	public const string Duplicate      = "GRAM6005";

	/// <summary>Each target bound, in the order given.</summary>
	/// <param name="grammar">The grammar's own text, without what it includes.</param>
	/// <param name="scanner">What reads the grammar's C# blocks, as it is read anywhere else.</param>
	public static IReadOnlyList<ClassificationTarget> Bind(string grammar, IReadOnlyList<string> targets, ICSharpScanner? scanner)
	{
		if (grammar is null)
			throw new ArgumentNullException(nameof(grammar));

		if (targets is null)
			throw new ArgumentNullException(nameof(targets));

		var parsed = GramParser.Parse(GramLexer.Tokenize(grammar, scanner, GramCompiler.CutsOf(null, null), grammar.Length, null));
		var rules  = new List<Decl.Rule>();

		Collect(parsed.File.Decls, rules);

		var bound  = new List<ClassificationTarget>(targets.Count);
		var taken  = new HashSet<(bool Capture, int Position)>();

		foreach (var target in targets)
			bound.Add(BindOne(target));

		return bound;

		ClassificationTarget BindOne(string target)
		{
			var parts = target.Split('.');

			if (parts.Length is < 1 or > 2 || parts.Any(string.IsNullOrWhiteSpace) ||
				parts.Any(static part => !string.Equals(part, part.Trim(), StringComparison.Ordinal)))
			{
				return Refused(ClassificationTargetProblem.Malformed, $"Classification target '{target}' must be 'Rule' or 'Rule.capture'.");
			}

			var matching = rules.Where(rule => rule.Name == parts[0]).ToArray();

			if (matching.Length == 0)
				return Refused(ClassificationTargetProblem.UnknownRule, $"Grammar rule '{parts[0]}' was not found.");

			if (matching.Length > 1)
				return Refused(ClassificationTargetProblem.AmbiguousRule, $"Grammar rule '{parts[0]}' is ambiguous.");

			var rule = matching[0];

			if (parts.Length == 1)
				return Taken(rule.At.Position, null);

			var capture = FirstCapture(rule.Body, parts[1]);

			if (capture is null)
				return Refused(ClassificationTargetProblem.UnknownCapture, $"Capture '{parts[1]}' was not found in rule '{parts[0]}'.");

			return Taken(rule.At.Position, capture);

			ClassificationTarget Taken(int rulePosition, int? capturePosition)
			{
				return taken.Add((capturePosition is not null, capturePosition ?? rulePosition))
					? new ClassificationTarget(target, ClassificationTargetProblem.None, null, rulePosition, capturePosition)
					: Refused(ClassificationTargetProblem.Duplicate, $"Classification target '{target}' is specified more than once.");
			}

			ClassificationTarget Refused(ClassificationTargetProblem problem, string message)
			{
				return new ClassificationTarget(target, problem, message, 0, null);
			}
		}
	}

	/// <summary>The identifier a problem is reported under.</summary>
	public static string? IdOf(ClassificationTargetProblem problem)
	{
		return problem switch
		{
			ClassificationTargetProblem.Malformed      => Malformed,
			ClassificationTargetProblem.UnknownRule    => UnknownRule,
			ClassificationTargetProblem.AmbiguousRule  => AmbiguousRule,
			ClassificationTargetProblem.UnknownCapture => UnknownCapture,
			ClassificationTargetProblem.Duplicate      => Duplicate,
			_                                          => null,
		};
	}

	/// <summary>Every rule declared, a namespace's included, in the order written.</summary>
	static void Collect(IReadOnlyList<Decl> declarations, List<Decl.Rule> rules)
	{
		foreach (var declaration in declarations)
		{
			if (declaration is Decl.Rule rule)
				rules.Add(rule);
			else if (declaration is Decl.Namespace @namespace)
				Collect(@namespace.Decls, rules);
		}
	}

	/// <summary>Where a capture of that name is first written in the rule's own scope, or null.</summary>
	static int? FirstCapture(Expr expression, string name)
	{
		// A constructing group owns its captures (syntax.md §3.7): they are not the rule's.
		if (expression is Expr.Group nested && Constructs(nested.Body))
			return null;

		if (expression is Expr.Capture capture && capture.Name == name)
			return capture.At.Position;

		foreach (var child in Dump.Children(expression))
			if (FirstCapture(child, name) is { } found)
				return found;

		return null;
	}

	static bool Constructs(Expr body)
	{
		return body switch
		{
			Expr.Construct                => true,
			Expr.Choice(var alternatives) => alternatives.Any(Constructs),
			_                             => false,
		};
	}
}
