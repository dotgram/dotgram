using System;
using System.Linq;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A value that is told where it was written is told once, by the reader, on the value it has just
/// made. An always-located rule whose value is a <c>Nullable</c> of a struct wrote a call the type
/// does not have, and a per-call door boxed it and lost the location; both are refused where the
/// rule is written (GRAM4046).
/// </summary>
public sealed class LocatedStructTests
{
	const string Types = """
		public interface ILoc
		{
			void Locate(int at, int length);

			void Locate(int at, int length, int gapStart);
		}

		public struct Mark : ILoc
		{
			public int At;

			public void Locate(int at, int length)
			{
				At = at;
			}

			public void Locate(int at, int length, int gapStart)
			{
				At = at;
			}
		}

		public sealed class Item : ILoc
		{
			public int At = -1;

			public void Locate(int at, int length)
			{
				At = at;
			}

			public void Locate(int at, int length, int gapStart)
			{
				At = at;
			}
		}

		public record class Rec : ILoc
		{
			public int At { get; set; } = -1;

			public void Locate(int at, int length)
			{
				At = at;
			}

			public void Locate(int at, int length, int gapStart)
			{
				At = at;
			}
		}
		""";

	static string Source(string type, bool perCall)
	{
		var options = perCall
			? "[GramOptions(LocationType = typeof(ILoc), Suffix = \"Located\", PerCall = true)]"
			: "";
		var located = perCall ? "" : ", LocationType = typeof(ILoc)";

		return $$""""
			using DotGram;

			namespace LocatedStructProbe
			{
				{{Types}}

				[Gram("""
					using Std;

					trivia = { Spacing* }

					Start : @{{type}} = Identifier => @(new {{type.Replace("System.Nullable<", "").TrimEnd('>')}}())

					parse Start
					""", Lexical = true{{located}})]
				{{options}}
				public static partial class P
				{
				}
			}
			"""";
	}

	[Theory]
	[InlineData("System.Nullable<Mark>", false)]
	[InlineData("System.Nullable<Mark>", true)]
	public void A_nullable_struct_that_would_be_located_is_refused_at_its_rule(string type, bool perCall)
	{
		var run     = GeneratorDriverTests.RunGenerator(Source(type, perCall));
		var refused = Assert.Single(run.Diagnostics, static one => one.Id == "GRAM4046");

		Assert.Contains("Start", refused.GetMessage(), StringComparison.Ordinal);
	}

	[Theory]
	[InlineData("Mark", false)]
	[InlineData("Mark", true)]
	[InlineData("Item", false)]
	[InlineData("Item", true)]
	[InlineData("Rec", false)]
	[InlineData("Rec", true)]
	public void A_struct_a_class_or_a_record_is_accepted_as_before(string type, bool perCall)
	{
		var source = Source(type, perCall);

		Assert.DoesNotContain(GeneratorDriverTests.RunGenerator(source).Diagnostics, static one => one.Id == "GRAM4046");

		// And it compiles.
		Assert.NotNull(GeneratorDriverTests.Build(source, permittedWarning: "GRAM5004"));
	}

	/// <summary>
	/// A class of the grammar's own that happens to be called <c>Nullable</c> is a class: only the
	/// framework's is refused.
	/// </summary>
	[Fact]
	public void A_class_named_Nullable_is_located_as_any_class_is()
	{
		var source = $$""""
			using DotGram;

			namespace LocatedStructProbe
			{
				{{Types}}

				public sealed class Nullable<T> : ILoc
				{
					public int At = -1;

					public void Locate(int at, int length)
					{
						At = at;
					}

					public void Locate(int at, int length, int gapStart)
					{
						At = at;
					}
				}

				[Gram("""
					using Std;

					Start : @Nullable<Mark> = Identifier => @(new Nullable<Mark>())

					parse Start
					""", Lexical = true, LocationType = typeof(ILoc))]
				public static partial class P
				{
				}
			}
			"""";

		Assert.DoesNotContain(GeneratorDriverTests.RunGenerator(source).Diagnostics, static one => one.Id == "GRAM4046");
		Assert.NotNull(GeneratorDriverTests.Build(source, permittedWarning: "GRAM5004"));
	}
}
