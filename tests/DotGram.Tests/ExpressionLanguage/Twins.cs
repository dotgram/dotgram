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
