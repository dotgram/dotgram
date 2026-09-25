namespace DotGram.Tests.ExpressionLanguage.Elsewhere;

// The second of the two `Twofold`s, in a namespace of its own and in no global one, so that a
// text saying both `using`s has a name that means two things and nothing that settles it. It is
// a file apart because a block namespace cannot follow a file-scoped one.

/// <summary>The other one, which makes the name ambiguous and not shadowed.</summary>
public static class Twofold
{
	public static string Where()
	{
		return "elsewhere";
	}
}
