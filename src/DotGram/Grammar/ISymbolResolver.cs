using System;
using System.Collections.Generic;

namespace DotGram.Grammar;

/// <summary>
/// Answers the C# type questions the grammar cannot answer by itself.
/// </summary>
/// <remarks>
/// One of the two seams between the grammar half and its host. Resolving C# type
/// relationships needs the host compilation; everything else the grammar half does is
/// a pure function of the grammar text. Keeping the dependency behind one small interface
/// is what lets the rest be tested, and run, without Roslyn.
/// </remarks>
public interface ISymbolResolver
{
	/// <summary>Whether a C# type of this name is in scope.</summary>
	bool TypeExists(string qualifiedName);

	/// <summary>
	/// Whether a value of one C# type may be put where the other is expected.
	/// </summary>
	/// <remarks>
	/// What §4.1 case 2 rests on: <c>Feed : FeedItem[] = Header &amp; Row* &amp; Trailer</c>
	/// takes the operands assignable to <c>FeedItem</c> and leaves the rest, and only the
	/// host knows which those are — assignability is inheritance, interfaces and
	/// conversions, none of which a grammar can see.
	/// </remarks>
	/// <param name="from">The type a rule's value has, as the grammar declared it.</param>
	/// <param name="to">The element type of the sequence being built.</param>
	bool IsAssignable(string from, string to);

	/// <summary>
	/// The constructors a declared type offers, each as its parameters in order.
	/// </summary>
	/// <remarks>
	/// §7.3's first way of filling a result in: captures are matched to a constructor by
	/// name, so which constructors there are and what they take is the one thing that has
	/// to be asked. Accessibility is the host's business — what comes back is what the
	/// generated code may actually call from where it will sit.
	/// </remarks>
	/// <returns><c>false</c> when the type is not in scope, or offers nothing to call.</returns>
	bool TryResolveConstructors(
		string qualifiedName, out IReadOnlyList<IReadOnlyList<MethodParameter>> constructors);

	/// <summary>
	/// The properties of a type that can be set when it is made — §7.3's second way of
	/// filling a result in.
	/// </summary>
	/// <remarks>
	/// Only what an object initializer may write: <c>init</c> and settable properties, and
	/// among them the <c>required</c> ones have to be covered or the type will not compile.
	/// Whether one is settable from where the generated code sits is the host's business,
	/// the same as for a constructor.
	/// </remarks>
	/// <returns><c>false</c> when the type is not in scope, or nothing about it can be set.</returns>
	bool TryResolveSettableProperties(string qualifiedName, out IReadOnlyList<ObjectMember> properties);
}

/// <summary>A property an object initializer may write.</summary>
/// <param name="Type">Fully qualified, so the grammar half can hand it back unchanged.</param>
/// <param name="IsRequired">
/// <c>required</c>: not covering it is not an option, so a type with one the captures
/// cannot fill is not a type these captures can build.
/// </param>
public readonly record struct ObjectMember(string Name, string Type, bool IsRequired);

/// <summary>One parameter of a C# method or constructor, as the host sees it.</summary>
/// <param name="Type">Fully qualified, so the grammar half can hand it back unchanged.</param>
public readonly record struct MethodParameter(string Name, string Type, bool IsOptional);

/// <summary>
/// Accepts every name it is asked about. For tests and tooling that exercise the
/// grammar side without a host compilation; never correct for real generation.
/// </summary>
public sealed class PermissiveSymbolResolver : ISymbolResolver
{
	public static readonly PermissiveSymbolResolver Instance = new();

	public bool TypeExists(string qualifiedName) => true;

	public bool IsAssignable(string from, string to) => true;

	/// <remarks>
	/// None, rather than everything: a made-up constructor would have the grammar half
	/// emit a call to something that is not there, and the failure would arrive in the
	/// consumer's build. Saying no leaves the grammar to build its value with a
	/// <c>=&gt;</c>, which needs no host at all.
	/// </remarks>
	public bool TryResolveConstructors(
		string qualifiedName, out IReadOnlyList<IReadOnlyList<MethodParameter>> constructors)
	{
		constructors = [];

		return false;
	}

	/// <remarks>The same reasoning as the constructors above: none rather than everything.</remarks>
	public bool TryResolveSettableProperties(string qualifiedName, out IReadOnlyList<ObjectMember> properties)
	{
		properties = [];

		return false;
	}
}
