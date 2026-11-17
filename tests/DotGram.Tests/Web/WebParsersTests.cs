using System;
using System.Linq;
using System.Reflection;
using System.Threading;

using DotGram.Web;

using Xunit;

namespace DotGram.Tests.Web;

/// <summary>
/// The package's one setting of its limits, <see cref="WebParsers.Limits"/>: what it starts as, that it reaches every
/// grammar of the package, and that JSON nested past it is refused.
/// </summary>
/// <remarks>
/// Run alone: the limits are the process's, and a JSON test beside these would read them.
/// </remarks>
[Collection(typeof(Alone))]
public sealed class WebParsersTests
{
	/// <summary>
	/// The collection xunit runs after every parallel one, with nothing beside it.
	/// </summary>
	[CollectionDefinition(DisableParallelization = true)]
	public sealed class Alone;

	/// <summary>
	/// No limit until a program sets one.
	/// </summary>
	[Fact]
	public void The_limits_start_as_none()
	{
		Assert.Same(WebParserLimits.Default, WebParsers.Limits);
		Assert.Equal(0, WebParsers.Limits.MaxExtraStacks);
	}

	/// <summary>
	/// What is set reaches every grammar the package holds, found by what each declares rather than by a list a new
	/// grammar could be left out of.
	/// </summary>
	[Fact]
	public void The_limits_reach_every_grammar_of_the_package()
	{
		var grammars = typeof(JsonValue).Assembly.GetTypes()
			.Where(static type => type.GetProperty("Limits", BindingFlags.Public | BindingFlags.Static) is { } limits &&
				limits.PropertyType.Name == "ParserLimits" && limits.PropertyType.DeclaringType == type)
			.ToArray();

		Assert.True(grammars.Length >= 13, $"{grammars.Length} grammars found");

		try
		{
			WebParsers.Limits = WebParsers.Limits.WithMaxExtraStacks(5);

			Assert.Equal(5, WebParsers.Limits.MaxExtraStacks);

			foreach (var grammar in grammars)
				Assert.True(5 == Stacks(grammar), grammar.Name);
		}
		finally
		{
			WebParsers.Limits = WebParserLimits.Default;
		}

		foreach (var grammar in grammars)
			Assert.True(0 == Stacks(grammar), grammar.Name);
	}

	/// <summary>
	/// The grammars hold no limits of their own: each answers from the package's one setting, so that what a
	/// grammar of the package is told directly changes nothing, and no reading can find one grammar set and
	/// another not yet.
	/// </summary>
	[Fact]
	public void Every_grammar_answers_from_the_one_setting()
	{
		try
		{
			Rfc8259.Limits = Rfc8259.ParserLimits.Default.WithMaxExtraStacks(7);
			Assert.Equal(0, Rfc8259.Limits.MaxExtraStacks);

			WebParsers.Limits = WebParsers.Limits.WithMaxExtraStacks(3);
			Assert.Equal(3, Rfc8259.Limits.MaxExtraStacks);
			Assert.Equal(3, Rfc5322.Limits.MaxExtraStacks);
		}
		finally
		{
			Rfc8259.Limits = Rfc8259.ParserLimits.Default;
			WebParsers.Limits = WebParserLimits.Default;
		}
	}

	/// <summary>
	/// Set over and over while it is read: every reading of the setting finds one set whole, the package's value
	/// and each grammar's alike.
	/// </summary>
	[Fact]
	public void A_setting_is_read_whole_while_it_is_set()
	{
		var stop  = 0;
		var torn  = 0;
		var reads = 0;

		var setter = new Thread(() =>
		{
			var one   = WebParserLimits.Default.WithMaxExtraStacks(1);
			var other = WebParserLimits.Default.WithMaxExtraStacks(2);

			while (Volatile.Read(ref stop) == 0)
			{
				WebParsers.Limits = one;
				WebParsers.Limits = other;
			}
		});

		setter.Start();

		try
		{
			var until = DateTime.UtcNow.AddMilliseconds(300);

			while (DateTime.UtcNow < until)
			{
				var seen   = WebParsers.Current;
				var stacks = seen.Limits.MaxExtraStacks;

				if (seen.Rfc3339Limits.MaxExtraStacks != stacks || seen.Rfc8259Limits.MaxExtraStacks != stacks ||
					seen.Rfc9651Limits.MaxExtraStacks != stacks)
					torn++;

				reads++;
			}
		}
		finally
		{
			Volatile.Write(ref stop, 1);
			setter.Join();
			WebParsers.Limits = WebParserLimits.Default;
		}

		Assert.True(reads > 0);
		Assert.Equal(0, torn);
	}

	/// <summary>
	/// A negative count is refused where it is made, and null where it is set.
	/// </summary>
	[Fact]
	public void A_limit_out_of_range_is_refused()
	{
		Assert.Throws<ArgumentOutOfRangeException>(static () => WebParserLimits.Default.WithMaxExtraStacks(-1));
		Assert.Throws<ArgumentNullException>(static () => WebParsers.Limits = null!);
		Assert.Same(WebParserLimits.Default, WebParsers.Limits);
	}

	/// <summary>
	/// JSON nested past what one extra stack holds is read where nothing limits the stacks, and refused with
	/// <see cref="InsufficientExecutionStackException"/> where the limits allow one.
	/// </summary>
	[Fact]
	public void Json_nested_past_the_limit_is_refused()
	{
		const int depth = 400_000;

		var text = new string('[', depth) + "1" + new string(']', depth);

		Assert.Null(Read(text));

		try
		{
			WebParsers.Limits = WebParsers.Limits.WithMaxExtraStacks(1);

			Assert.IsType<InsufficientExecutionStackException>(Read(text));

			// A shallow text takes no extra stack, and the limit does not touch it.
			Assert.Null(Read("[[1]]"));
		}
		finally
		{
			WebParsers.Limits = WebParserLimits.Default;
		}

		Assert.Null(Read(text));
	}

	/// <summary>
	/// What parsing the text throws, on a thread whose stack runs low early; null where it parses.
	/// </summary>
	static Exception? Read(string text)
	{
		Exception? thrown = null;

		var thread = new Thread(() =>
		{
			try
			{
				JsonValue.Parse(text);
			}
			catch (Exception caught)
			{
				thrown = caught;
			}
		}, 256 * 1024);

		thread.Start();
		thread.Join();

		return thrown;
	}

	static int Stacks(Type grammar)
	{
		var limits = grammar.GetProperty("Limits", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

		return (int)limits.GetType().GetProperty("MaxExtraStacks")!.GetValue(limits)!;
	}
}
