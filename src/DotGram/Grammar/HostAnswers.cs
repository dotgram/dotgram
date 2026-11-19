using System;

namespace DotGram.Grammar;

// What the host answers with, when a grammar's stage asks the target language about it
// (ITargetLanguage's host questions).

/// <summary>What asking about an external recognizer's value overload found.</summary>
enum ExternalValueResolution
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

/// <summary>How a grammar calls a C# method it names, which its position says (§7.1).</summary>
enum ExternalMethodRole
{
	/// <summary>A bare <c>@Name</c> operand: <c>bool M(ReadOnlySpan&lt;char&gt; input, ref int pos)</c>, with or without <c>out T value</c>.</summary>
	Recognizer,

	/// <summary>An <c>[@Name]</c> in an element set: <c>bool M(char c)</c>.</summary>
	Predicate,
}

/// <summary>What asking whether a C# method can be called in its role found.</summary>
enum ExternalMethodResolution
{
	/// <summary>It can — or nothing says for certain that it cannot.</summary>
	Found,

	/// <summary>No method of that name anywhere the call could reach.</summary>
	NoMethod,

	/// <summary>Methods of that name, and none of them one the parser can call in that role.</summary>
	NoOverload,

	/// <summary>A recognizer taking the generated ParserInput&lt;char&gt; view.</summary>
	FoundInput,

	/// <summary>An input-view recognizer also taking the grammar context as its last argument.</summary>
	FoundInputWithContext,

	/// <summary>
	/// A method the generated class writes for a publication has that name, and where the call stands
	/// it hides every method of the name outside the class, none of which it is.
	/// </summary>
	Hidden,
}

/// <summary>A property an object initializer may write.</summary>
/// <param name="Type">Fully qualified, so the grammar half can hand it back unchanged.</param>
/// <param name="IsRequired">
/// <c>required</c>: not covering it is not an option, so a type with one the captures
/// cannot fill is not a type these captures can build.
/// </param>
readonly record struct ObjectMember(string Name, string Type, bool IsRequired);

/// <summary>One parameter of a C# method or constructor, as the host sees it.</summary>
/// <param name="Type">Fully qualified, so the grammar half can hand it back unchanged.</param>
readonly record struct MethodParameter(string Name, string Type, bool IsOptional);
