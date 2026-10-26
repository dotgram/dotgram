using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Emit;

namespace DotGram.Tests;

/// <summary>
/// The rule every name the generator writes is held to, read off a name and the scope it is declared
/// in: what <see cref="GeneratedNameTests"/> asks of the snapshots and the Slow suite of the parsers
/// that ship.
/// </summary>
/// <remarks>
/// <para>
/// In the class the generator writes into, a name is the author's (a publication's method, a rule's
/// type), one of the closed lists of plain names it keeps (<see cref="CSharpEmitter.PublicNames"/>,
/// <see cref="CSharpEmitter.SupportNames"/>, the trace's, the lexer's), or holds the mark. One that holds
/// the mark and begins with a kind of <see cref="CSharpEmitter.HostKinds"/> is made from the author's
/// names and must read back as that kind and identifiers of the grammar; one that begins with no kind is
/// the generator's own and may be anything else.
/// </para>
/// <para>
/// In a class the generator declares for its own use, a name that begins with a kind of
/// <see cref="CSharpEmitter.NestedKinds"/> must read back as it and one identifier; no other name there
/// may begin with any kind, of either table.
/// </para>
/// </remarks>
static class GeneratedNameRule
{
	/// <summary>
	/// What is wrong with a name declared where the generator writes, or null if nothing is.
	/// </summary>
	/// <param name="name">The name as declared.</param>
	/// <param name="host">Whether it is declared in the class the generator writes into (or a suffix of it).</param>
	/// <param name="identifiers">
	/// The identifiers a derived name may be made of: the grammar's rules and publications. Null takes any
	/// identifier that does not hold the mark, for output whose grammar is not at hand.
	/// </param>
	/// <param name="author">The author's own names: the publications' methods and the rules' types.</param>
	public static string? Fault(string name, bool host, ISet<string>? identifiers, ISet<string> author, bool traced = true, bool lexical = true)
	{
		var head = name.IndexOf('_') is var cut and > 0 ? name.Substring(0, cut) : null;

		if (host)
		{
			if (author.Contains(name) || CSharpEmitter.PublicNames.Contains(name) || CSharpEmitter.SupportNames.Contains(name) ||
				traced && CSharpEmitter.TraceNames.Contains(name) || lexical && CSharpEmitter.LexerNames.Contains(name))
				return null;

			if (!name.Contains(CSharpEmitter.NameMark, StringComparison.Ordinal))
				return "has no mark and is in none of the lists the generator keeps";

			if (head is not null && CSharpEmitter.IsKind(head, CSharpEmitter.HostKinds))
				return Derived(name.Substring(head.Length), head, identifiers);

			// The mark appended to a name of the author's is no kind: a publication called after the
			// kind's word would be written the same.
			if (author.FirstOrDefault(one => name.StartsWith(one + CSharpEmitter.NameMark, StringComparison.Ordinal)) is { } glued)
				return $"is the author's '{glued}' with the mark appended";

			return NestedKind(head) ? "begins with a kind it is not" : null;
		}

		if (head is not null && CSharpEmitter.IsKind(head, CSharpEmitter.NestedKinds))
		{
			var identifier = name.Substring(head.Length + 1);

			return Known(identifier, identifiers) ? null : $"is not kind '{head}' and an identifier of the grammar";
		}

		return head is not null && CSharpEmitter.IsKind(head, CSharpEmitter.HostKinds)
			? "begins with a kind of the class it is not declared in"
			: null;
	}

	/// <summary>What is wrong with the rest of a host name after its kind, or null.</summary>
	static string? Derived(string rest, string kind, ISet<string>? identifiers)
	{
		if (rest == CSharpEmitter.NameMark)
			return null;

		if (!rest.StartsWith(CSharpEmitter.NameMark + "_", StringComparison.Ordinal))
			return $"begins with kind '{kind}' and is not followed by the mark";

		// A call into the host's code is named after the member it calls, which is no rule.
		var free = CSharpEmitter.Shape(kind) == "Host#";

		foreach (var identifier in rest.Substring(CSharpEmitter.NameMark.Length + 1).Split([CSharpEmitter.NameMark + "_"], StringSplitOptions.None))
			if (!(free ? Wellformed(identifier) : Known(identifier, identifiers)))
				return $"is kind '{kind}' and '{identifier}', which is no identifier of the grammar";

		return null;
	}

	static bool NestedKind(string? head)
	{
		return head is not null && CSharpEmitter.IsKind(head, CSharpEmitter.NestedKinds);
	}

	static bool Known(string identifier, ISet<string>? identifiers)
	{
		return identifiers?.Contains(identifier) ?? Wellformed(identifier);
	}

	static bool Wellformed(string identifier)
	{
		return identifier.Length > 0 && !identifier.Contains(CSharpEmitter.NameMark, StringComparison.Ordinal) &&
			identifier.All(static c => char.IsLetterOrDigit(c) || c == '_');
	}

	/// <summary>The author's own names of a compilation, as a set.</summary>
	public static HashSet<string> Author(GramNames names)
	{
		return new HashSet<string>(names.Publications.Concat(names.Types), StringComparer.Ordinal);
	}
}
