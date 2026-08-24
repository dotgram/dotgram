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

	/// <summary>
	/// Whether an external recognizer named this hands back a value of its own — §7.1's
	/// third row, <c>bool M(ReadOnlySpan&lt;char&gt; input, ref int pos, out T value)</c> —
	/// and what <c>T</c> is when it does.
	/// </summary>
	/// <remarks>
	/// Unlike every other member here, this is asked about a method rather than a type: the
	/// bare <c>@Name</c> notation is unchanged and does not say which of the two shapes it
	/// means, so the generator — not the grammar — has to look. More than one such overload
	/// with a different <c>T</c> is left a tie rather than guessed at, the same as §7.3's
	/// constructors: a wrong <c>T</c> silently chosen is the failure worth avoiding, not a
	/// slower one.
	/// </remarks>
	/// <param name="against">
	/// The type <c>T</c> would have to fit for a whole rule's body to be exactly this call
	/// (§4.1 case 3 applied to one), or null where nothing needs to fit — a captured or
	/// otherwise nested use, which asks only what <c>T</c> is. Folded into this one question
	/// rather than asked separately, because <c>T</c> is discovered here and not knowable
	/// from grammar syntax alone — nothing upstream could have asked "is T assignable to
	/// this" as an ordinary <see cref="IsAssignable"/> question when T is what this call
	/// exists to find out.
	/// </param>
	ExternalValueResolution TryResolveExternalValue(string methodName, string? against, out string? valueType);
}

/// <summary>What asking about an external recognizer's value overload found.</summary>
public enum ExternalValueResolution
{
	/// <summary>
	/// No <c>(ReadOnlySpan&lt;char&gt;, ref int, out T)</c> overload of this name is in
	/// scope. Bare <c>@Name</c> is §7.1's second row, unchanged.
	/// </summary>
	NotFound,

	/// <summary>Exactly one such overload. The out parameter it names is <c>T</c>.</summary>
	Found,

	/// <summary>More than one, with different <c>T</c>. Left as a tie.</summary>
	Ambiguous,
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

	/// <remarks>The same reasoning again: a grammar tested without a host sees only §7.1's
	/// plain, text-covering form.</remarks>
	public ExternalValueResolution TryResolveExternalValue(string methodName, string? against, out string? valueType)
	{
		valueType = null;

		return ExternalValueResolution.NotFound;
	}
}
