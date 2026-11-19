using System;

namespace DotGram.Grammar;

/// <summary>
/// How the language a parser is generated in is named in what a grammar's author is told: its name,
/// and the few things of it a message has to spell.
/// </summary>
/// <remarks>
/// A stage that reads a grammar knows no target language, and yet what it says is about one: a
/// guard "is C#", a type that is not in view is "no C# type". The words that differ by language
/// are asked for here (<see cref="ITargetLanguage.Wording"/>), so that the sentence stays the
/// stage's and the name the language's. Only what some message spells has a member.
/// </remarks>
abstract class TargetWording
{
	/// <summary>The language's name, as a sentence writes it: <c>C#</c>.</summary>
	public abstract string Name { get; }

	/// <summary>
	/// The parameters of an external recognizer that hands back a value, as the host declares them,
	/// in brackets.
	/// </summary>
	public abstract string ValuedRecognizerParameters { get; }

	/// <summary>The declaration of an external recognizer of a given name that hands back no value.</summary>
	public abstract string RecognizerDeclaration(string name);

	/// <summary>What an external recognizer adds to that declaration to hand back a value, and where.</summary>
	public abstract string RecognizerValueParameter { get; }

	/// <summary>The type an external recognizer over buffered input reads from.</summary>
	public abstract string BufferedInputType { get; }

	/// <summary>What an external recognizer is, said of one that cannot be told more input is coming.</summary>
	public abstract string RecognizerOverHeldText { get; }

	/// <summary>The language's wrapper that makes a value type optional, as a noun.</summary>
	public abstract string OptionalWrapper { get; }
}
