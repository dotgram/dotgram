using System;

namespace DotGram.Tests.Deep.N1.N2.N3;

/// <summary>
/// A type of eleven parts, for <see cref="ExpressionDottedTypeRefusalAllocationTests"/>: more than a
/// name may have before the scope is asked how deep its types go.
/// </summary>
public static class C1
{
	public static class C2
	{
		public static class C3
		{
			public static class C4
			{
				public sealed class C5
				{
					public static int Value { get; } = 1;
				}
			}
		}
	}
}
