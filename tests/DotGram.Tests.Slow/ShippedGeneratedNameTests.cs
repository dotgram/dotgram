using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Every name the generator wrote into the parsers that ship is held to the one rule the snapshots are
/// (GeneratedNameTests in DotGram.Tests): read off the compiled assemblies, so that a family a large
/// grammar reaches and no snapshot does is still seen.
/// </summary>
/// <remarks>
/// <para>
/// The assemblies hold the author's members beside the generator's, and there is no grammar text at
/// hand to tell a publication or a hand-written helper from anything else; so what is held here is what
/// the generator alone writes. In the class it writes into, a name with the mark must be a kind and
/// identifiers or a name of the generator's own that begins with no kind; in a class it declares for its
/// own use, every name. The identifiers are held to their shape (no mark, not empty) rather than to the
/// grammar's rules, which the snapshots check exactly.
/// </para>
/// </remarks>
public sealed class ShippedGeneratedNameTests
{
	[Theory]
	[InlineData("DotGram.Sql")]
	[InlineData("DotGram.Web")]
	[InlineData("DotGram.ExpressionLanguage")]
	[InlineData("DotGram.Finance")]
	[InlineData("DotGram.Examples")]
	public void Every_name_a_shipped_parser_was_written_under_is_kept_apart(string assembly)
	{
		var faults = new SortedSet<string>(StringComparer.Ordinal);
		var none   = new HashSet<string>(StringComparer.Ordinal);
		var hosts  = 0;
		var seen   = 0;

		foreach (var host in Assembly.Load(assembly).GetTypes().Where(IsHost))
		{
			hosts++;

			foreach (var (scope, inner, name) in Members(host, inner: false))
			{
				// The author's own: a publication, a rule's type, a helper written by hand.
				if (!inner && !name.Contains(CSharpEmitter.NameMark, StringComparison.Ordinal))
					continue;

				seen++;

				if (GeneratedNameRule.Fault(name, !inner, null, none) is { } fault)
					faults.Add($"{scope}.{name} {fault}");
			}
		}

		Assert.True(hosts > 0 && seen > 0, $"no generated class was found in {assembly}");
		Assert.True(faults.Count == 0, $"{faults.Count} names not kept apart:\n" + string.Join("\n", faults.Take(200)));
	}

	/// <summary>A class the generator wrote into: one that holds a type of its own (Parser_DotGram, Reader_DotGram, …).</summary>
	static bool IsHost(Type type)
	{
		return !Generators(type) && (type.DeclaringType is null || !Generators(type.DeclaringType)) &&
			type.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic).Any(Generators);
	}

	/// <summary>Whether a nested type is one the generator declares for its own use rather than a rule's type.</summary>
	static bool Generators(Type type)
	{
		var name = type.Name;
		var tick = name.IndexOf('`');

		if (tick >= 0)
			name = name.Substring(0, tick);

		return name.Contains(CSharpEmitter.NameMark, StringComparison.Ordinal) || CSharpEmitter.PublicNames.Contains(name) ||
			CSharpEmitter.SupportNames.Contains(name) || CSharpEmitter.TraceNames.Contains(name);
	}

	/// <summary>Every member a type declares, and those of the generator's own types inside it.</summary>
	static IEnumerable<(string Scope, bool Inner, string Name)> Members(Type type, bool inner)
	{
		const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

		foreach (var member in type.GetMembers(all))
		{
			if (member is ConstructorInfo || member.Name.Contains('<', StringComparison.Ordinal) ||
				member is MethodInfo { IsSpecialName: true } || member.IsDefined(typeof(CompilerGeneratedAttribute)))
				continue;

			var name = member.Name;
			var tick = name.IndexOf('`');

			if (tick >= 0)
				name = name.Substring(0, tick);

			// An explicit interface implementation is named after the interface, not by the generator.
			if (name.Contains('.', StringComparison.Ordinal))
				continue;

			yield return (type.FullName ?? type.Name, inner, name);

			if (member is Type nested && (inner || Generators(nested)))
				foreach (var one in Members(nested, inner: true))
					yield return one;
		}
	}
}
