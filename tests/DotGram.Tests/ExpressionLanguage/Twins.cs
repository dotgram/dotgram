using System;

// Two namespaces in one file, which is the one thing a file-scoped namespace cannot hold:
// the pair is the point. A text that imports both and names `Twin` means two types, and
// `A_name_two_usings_both_give_is_ambiguous` is what says so.

namespace DotGram.Tests.ExpressionLanguage.Left
{
	/// <summary>One of two public types of one name, each in a namespace of its own.</summary>
	public static class Twin
	{
		public static int Value => 1;
	}
}

namespace DotGram.Tests.ExpressionLanguage.Right
{
	/// <summary>The other one.</summary>
	public static class Twin
	{
		public static int Value => 2;
	}
}

namespace DotGram.Tests.ExpressionLanguage.Against
{
	/// <summary>A name one of the namespaces every text GETS also has: `System.Convert`.</summary>
	/// <remarks>
	/// The pair above is two written `using`s; this one is a written `using` against a default
	/// (ResolutionScope.DefaultImports), which C# treats as peers — a global using and a written
	/// one giving one name is CS0104 exactly as two written ones are, asked of Roslyn 2026-09-26.
	/// </remarks>
	public static class Convert
	{
		public static int Value => 3;
	}
}

namespace DotGram.Tests.ExpressionLanguage
{
	/// <summary>A type whose NESTED type shares a name with `Left.Twin`.</summary>
	/// <remarks>
	/// For the ambiguity a `using static` can make: C# counts a nested type it brings in as a peer
	/// of a type a namespace `using` brings in, so both giving `Twin` is CS0104 (asked of Roslyn
	/// 2026-09-26).
	/// </remarks>
	public static class Nesting
	{
		/// <summary>The nested one.</summary>
		public static class Twin
		{
			public static int Value => 4;
		}
	}

	/// <summary>A type whose static MEMBER shares a name with `Right.Twin`, which is a type.</summary>
	/// <remarks>
	/// For the other ambiguity, and it is another diagnostic: a bare name that is both a member a
	/// `using static` gives and a type in scope is CS0229 where a VALUE is wanted, and is the type
	/// where a type is wanted. Not CS0104 — the two questions are asked of different things.
	/// </remarks>
	public static class Holds
	{
		public static int Twin => 5;
	}
}
