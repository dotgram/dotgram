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
