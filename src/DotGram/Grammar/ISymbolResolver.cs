using System;

namespace DotGram.Grammar;

/// <summary>
/// Answers what a grammar's <c>@Name</c> refers to on the C# side.
/// </summary>
/// <remarks>
/// One of the two seams between the grammar half and its host. Resolving a C# name
/// needs the host compilation; everything else the grammar half does is a pure
/// function of the grammar text. Keeping the dependency behind one small interface is
/// what lets the rest be tested, and run, without Roslyn.
/// </remarks>
public interface ISymbolResolver
{
	/// <summary>Whether a C# type of this name is in scope.</summary>
	bool TypeExists(string qualifiedName);

	/// <summary>
	/// Classifies a C# method by its signature (docs/syntax.md §7.1): element predicate,
	/// external recognizer, value transformation, or guard.
	/// </summary>
	/// <returns><c>false</c> when no such method is in scope.</returns>
	bool TryResolveMethod(string qualifiedName, int argumentCount, out MethodRole role);

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
}

/// <summary>Role a C# method plays, decided by its signature and call position.</summary>
public enum MethodRole
{
	/// <summary><c>bool M(char c)</c> — tests and consumes one input item.</summary>
	ElementPredicate,

	/// <summary><c>bool M(ReadOnlySpan&lt;char&gt; input, ref int pos, out T value)</c>.</summary>
	ExternalRecognizer,

	/// <summary><c>T M(args…)</c> — never touches input.</summary>
	ValueTransformation,

	/// <summary>
	/// <c>bool M(args…, out T value)</c> — a transformation that may refuse (§8.1).
	/// </summary>
	/// <remarks>
	/// The shape <c>int.TryParse</c> already has, and the reason §8.1 needs no notation of
	/// its own: a conversion says it can fail by being written the way the BCL writes one.
	/// </remarks>
	FallibleTransformation,

	/// <summary><c>bool M(args…)</c> in a <c>where</c> position.</summary>
	Guard,
}

/// <summary>
/// Accepts every name it is asked about. For tests and tooling that exercise the
/// grammar side without a host compilation; never correct for real generation.
/// </summary>
public sealed class PermissiveSymbolResolver : ISymbolResolver
{
	public static readonly PermissiveSymbolResolver Instance = new();

	public bool TypeExists(string qualifiedName) => true;

	public bool TryResolveMethod(string qualifiedName, int argumentCount, out MethodRole role)
	{
		role = argumentCount == 0 ? MethodRole.ElementPredicate : MethodRole.ValueTransformation;
		return true;
	}

	public bool IsAssignable(string from, string to) => true;
}
