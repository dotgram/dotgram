using System;
using System.Collections.Generic;
using System.Text;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>An inclusive range of input items.</summary>
public readonly record struct CharRange(char From, char To)
{
	public bool IsSingle => From == To;

	public override string ToString() =>
		IsSingle ? Quote(From) : $"{Quote(From)}..{Quote(To)}";

	internal static string Quote(char value) => value switch
	{
		'\n' => @"'\n'",
		'\r' => @"'\r'",
		'\t' => @"'\t'",
		'\'' => @"'\''",
		'\\' => @"'\\'",
		_    => $"'{value}'",
	};
}

/// <summary>
/// What a grammar means, stripped of how it was written.
/// </summary>
/// <remarks>
/// Deliberately smaller than the syntax tree: groups are flattened, literals merged,
/// character alternatives folded into sets. What it keeps is rule boundaries — those
/// are what diagnostics are phrased in terms of, so inlining rules the way Roc's macro
/// did would buy speed at the cost of every message.
/// </remarks>
public abstract record Node
{
	public abstract override string ToString();
}

/// <summary>Matches nothing at all, and succeeds. What `none` lowers to.</summary>
public sealed record EmptyNode : Node
{
	public static readonly EmptyNode Instance = new();

	public override string ToString() => "none";
}

/// <summary>One input item drawn from a set.</summary>
public sealed record ElementNode(
	bool                      IsNegated,
	IReadOnlyList<CharRange>  Ranges,
	IReadOnlyList<string>     Categories,
	IReadOnlyList<Symbol>     References) : Node
{
	public override string ToString()
	{
		var parts = new List<string>();

		foreach (var range in Ranges)      parts.Add(range.ToString());
		foreach (var category in Categories) parts.Add($@"\p{{{category}}}");
		foreach (var reference in References) parts.Add(reference.Name);

		return $"[{(IsNegated ? "^ " : "")}{string.Join(" | ", parts)}]";
	}
}

/// <summary>A fixed run of input items.</summary>
public sealed record LiteralNode(string Text) : Node
{
	public override string ToString() =>
		Text.Length == 1 ? CharRange.Quote(Text[0]) : $"\"{Text}\"";
}

public sealed record SequenceNode(IReadOnlyList<Node> Nodes) : Node
{
	public override string ToString() => string.Join(" & ", Nodes);
}

public sealed record ChoiceNode(IReadOnlyList<Node> Nodes) : Node
{
	public override string ToString() => $"({string.Join(" | ", Nodes)})";
}

public sealed record RepeatNode(Node Body, int Min, int? Max) : Node
{
	public override string ToString() => (Min, Max) switch
	{
		(0, 1)                              => $"{Body}?",
		(0, null)                           => $"{Body}*",
		(1, null)                           => $"{Body}+",
		(var min, var max) when min == max  => $"{Body}{{{min}}}",
		(var min, null)                     => $"{Body}{{{min},}}",
		(var min, var max)                  => $"{Body}{{{min},{max}}}",
	};
}

public sealed record LookaheadNode(bool IsPositive, Node Body) : Node
{
	public override string ToString() => $"{(IsPositive ? "?=" : "?!")}{Body}";
}

public sealed record CaptureNode(string Name, Node Body) : Node
{
	public override string ToString() => $"{Name}: {Body}";
}

/// <summary>A `where` guard. Consumes nothing.</summary>
public sealed record GuardNode(string Text) : Node
{
	public override string ToString() => $"where {Text}";
}

/// <summary>A `=>` construction. Consumes nothing, runs after the alternative matched.</summary>
public sealed record ConstructNode(Node Body, string Text) : Node
{
	public override string ToString() => $"{Body} => {Text}";
}

/// <summary>A call to another rule; rule boundaries survive normalization.</summary>
public sealed record RuleCallNode(RuleSymbol Rule, IReadOnlyList<Node> Arguments) : Node
{
	public override string ToString() =>
		Arguments.Count == 0 ? Rule.Name : $"{Rule.Name}({string.Join(", ", Arguments)})";
}

/// <summary>The whole grammar after normalization: one node per rule, plus what was
/// worked out about them.</summary>
public sealed class RecognitionGraph(
	IReadOnlyList<RuleSymbol>              rules,
	IReadOnlyDictionary<RuleSymbol, Node>  bodies,
	IReadOnlyDictionary<RuleSymbol, bool>  nullable,
	IReadOnlyList<GramDiagnostic>          diagnostics)
{
	public IReadOnlyList<RuleSymbol>             Rules       { get; } = rules;
	public IReadOnlyDictionary<RuleSymbol, Node> Bodies      { get; } = bodies;
	public IReadOnlyDictionary<RuleSymbol, bool> Nullable    { get; } = nullable;
	public IReadOnlyList<GramDiagnostic>         Diagnostics { get; } = diagnostics;

	public bool HasErrors => Diagnostics.Count > 0;

	/// <summary>
	/// One rule per line, rendered back into notation.
	/// </summary>
	/// <remarks>
	/// Rendering the normalized form as text is what makes a normalization regression
	/// legible: a test states the grammar as written and the grammar as normalized, and
	/// a diff between them says exactly which fold changed. Roc's macro tested itself
	/// this way and it is the one thing from that project that transfers unchanged.
	/// </remarks>
	public override string ToString()
	{
		var text = new StringBuilder();

		foreach (var rule in Rules)
			text.Append(rule.Name).Append(" = ").AppendLine(Bodies[rule].ToString());

		foreach (var diagnostic in Diagnostics)
			text.AppendLine(diagnostic.ToString());

		return text.ToString().TrimEnd();
	}
}
