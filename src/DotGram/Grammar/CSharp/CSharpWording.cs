using System;

namespace DotGram.Grammar;

/// <summary>C#, as a grammar's author is told of it.</summary>
sealed class CSharpWording : TargetWording
{
	public static readonly CSharpWording Instance = new();

	CSharpWording()
	{
	}

	public override string Name => "C#";

	public override string ValuedRecognizerParameters => "(System.ReadOnlySpan<char>, ref int, out T)";

	public override string RecognizerDeclaration(string name)
	{
		return $"static bool {name}(System.ReadOnlySpan<char> input, ref int pos)";
	}

	public override string RecognizerValueParameter => "'out T value' after the position";

	public override string BufferedInputType => "ParserInput<char>";

	public override string RecognizerOverHeldText => "a C# recognizer over a span";

	public override string OptionalWrapper => "Nullable";
}
