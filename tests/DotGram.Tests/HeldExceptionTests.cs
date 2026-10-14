using System;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a construction throws on the immediate carrier is held, and thrown only where the tape
/// would have thrown it: the two carriers answer every shape alike, exceptions included.
/// </summary>
/// <remarks>
/// <para>
/// On the tape a construction runs once the parse has accepted, or where a guard asks for its
/// value, so one that throws on a reading the parse gives up is never run. Carried immediately
/// it runs where its alternative is read; the reader holds what it throws, puts the held
/// exception back with its marks wherever a reading is given up, and throws it at acceptance
/// or before a guard that asks for a value built since its rule began (§7.5,
/// Machine.Immediate.cs). These are the shapes that has to get right, each read on both carriers
/// and held to the same answer: the verdict and the value, or the exception's type and message.
/// </para>
/// <para>
/// <see cref="ExpressionLanguage.CarrierAgreementTests"/> asks the same of the expression
/// language over ten thousand texts; this asks it of one shape at a time, so that a shape that
/// breaks says which.
/// </para>
/// </remarks>
public sealed class HeldExceptionTests
{
	const string Members =
		"public static int Boom(string text) { throw new System.InvalidOperationException(\"boom \" + text); }\n" +
		"public static int Bang(string text) { throw new System.InvalidOperationException(\"bang \" + text); }\n" +
		"public static int Count(string text) { return int.Parse(text); }\n";

	const string Rules =
		"Bad  : @int = d: Std.Digits => @(Boom(d))\n" +
		"Good : @int = d: Std.Digits => @(Count(d))\n";

	/// <summary>A thrower in an alternative the parse gives up is a refusal, or the other alternative's answer; one in the alternative that stands throws.</summary>
	[Theory]
	[InlineData("12y", "accepted 12")]
	[InlineData("12z", "NoMatch at 2: Expected 'x' or 'y'.")]
	[InlineData("12x", "threw InvalidOperationException: boom 12")]
	public void An_alternative_given_up(string input, string expected)
	{
		Alike(Rules + "Start : @int = b: Bad & 'x' => @(b) | g: Good & 'y' => @(g)\nparse Start\n", input, expected);
	}

	/// <summary>A thrower in a turn the loop gives back goes with the turn; one in a turn that stands throws.</summary>
	[Theory]
	[InlineData("5y", "accepted 5")]
	[InlineData("55y", "threw InvalidOperationException: boom 5")]
	public void A_turn_given_back(string input, string expected)
	{
		Alike(
			"Item  : @int = d: ['0'..'9'] => @(Boom(d))\n" +
			"Start : @int = items: Item* & last: ['0'..'9'] & 'y' => @(Count(last))\n" +
			"parse Start\n",
			input, expected);
	}

	/// <summary>A guard that asks for the value throws before it is asked, where the tape builds the value for it — whether the alternative would have stood or not.</summary>
	[Theory]
	[InlineData("12x", "threw InvalidOperationException: boom 12")]
	[InlineData("12y", "threw InvalidOperationException: boom 12")]
	public void A_guard_asking_for_the_value(string input, string expected)
	{
		Alike(Rules + "Start : @int = v: Bad & when @(v > 0) & 'x' => @(v) | g: Good & 'y' => @(g)\nparse Start\n", input, expected);
	}

	/// <summary>A guard that reads text only builds nothing on the tape, and throws nothing here: the alternative is given up with what it held.</summary>
	[Theory]
	[InlineData("12ay", "accepted 12")]
	[InlineData("12qz", "NoMatch at 3: Expected 'x' or 'y'.")]
	[InlineData("12qx", "threw InvalidOperationException: boom 12")]
	public void A_guard_asking_for_text(string input, string expected)
	{
		Alike(
			Rules +
			"Start : @int = v: Bad & t: ['a'..'z'] & when @(t == \"q\") & 'x' => @(v)\n" +
			"             | d: Std.Digits & ['a'..'z'] & 'y' => @(Count(d))\n" +
			"parse Start\n",
			input, expected);
	}

	/// <summary>The same guard, in a rule read in parts: the mark is handed down to the part with the exception it held.</summary>
	[Theory]
	[InlineData("12ay", "accepted 12")]
	[InlineData("12qx", "threw InvalidOperationException: boom 12")]
	public void A_guard_in_a_part(string input, string expected)
	{
		Alike(
			Rules +
			"Start : @int = v: Bad & t: ['a'..'z'] & when @(t == \"q\") & 'x' => @(v)\n" +
			"             | d: Std.Digits & ['a'..'z'] & 'y' => @(Count(d))\n" +
			"parse Start\n",
			input, expected, partSize: 1, mustContain: "lmarkf");
	}

	/// <summary>Of two throwers in the reading that stands, the first in reading order is the one thrown: the tape builds in that order and never reaches the second.</summary>
	[Fact]
	public void The_first_of_two_throwers()
	{
		Alike(
			"Bad   : @int = d: Std.Digits => @(Boom(d))\n" +
			"Worse : @int = d: Std.Digits => @(Bang(d))\n" +
			"Start : @int = a: Bad & ',' & b: Worse => @(a + b)\n" +
			"parse Start\n",
			"1,2", "threw InvalidOperationException: boom 1");
	}

	/// <summary>A thrower inside a lookahead is read and given back, on both carriers.</summary>
	[Fact]
	public void A_lookahead()
	{
		Alike(Rules + "Start : @int = ?=Bad & g: Good & 'y' => @(g)\nparse Start\n", "12y", "accepted 12");
	}

	/// <summary>A fold step that throws: held through the turns, thrown where the fold stands, dropped where the alternative around it is given up.</summary>
	[Theory]
	[InlineData("1+2z", "accepted 1")]
	[InlineData("1+2y", "threw InvalidOperationException: boom 1")]
	public void A_fold_step(string input, string expected)
	{
		Alike(
			"Num   : @int = d: ['0'..'9'] => @(Count(d))\n" +
			"Expr  : @int = l: Expr & '+' & r: Num => @(Boom(l.ToString())) | n: Num => @(n)\n" +
			"Start : @int = e: Expr & 'y' => @(e) | d: ['0'..'9'] & '+' & ['0'..'9'] & 'z' => @(Count(d))\n" +
			"parse Start\n",
			input, expected);
	}

	/// <summary>A grammar with §7.8 marks holds the exception beside the mark depth, and puts both back together.</summary>
	[Theory]
	[InlineData("12y", "accepted 12")]
	[InlineData("12x", "threw InvalidOperationException: boom 12")]
	public void Under_a_mark(string input, string expected)
	{
		Alike(
			"state : @int\n" +
			Rules +
			"Start : @int = b: Bad with state @(1) & 'x' => @(b) | g: Good & 'y' => @(g)\n" +
			"parse Start\n",
			input, expected, mustContain: "marked");
	}

	/// <summary>The publication without the <c>Try</c> prefix throws the construction's own exception, not the refusal's <c>FormatException</c>.</summary>
	[Fact]
	public void Parse_throws_the_constructions_exception()
	{
		var grammar = Rules + "Start : @int = b: Bad & 'x' => @(b) | g: Good & 'y' => @(g)\nparse Start\n";

		foreach (var carrier in new[] { CarrierKind.Tape, CarrierKind.Immediate })
		{
			var assembly = Compiled(grammar, carrier);

			Assert.Equal("threw InvalidOperationException: boom 12", Parsed(assembly, "12x"));
			Assert.Equal("threw FormatException",                    Parsed(assembly, "12z").Split(':')[0]);
			Assert.Equal("value 12",                                 Parsed(assembly, "12y"));
		}
	}

	/// <summary>A machine that builds nothing holds nothing: no field, no catch, the output it had.</summary>
	[Fact]
	public void A_machine_without_constructions_holds_nothing()
	{
		var bare = Source("Start = Std.Digits & 'x'\nparse Start\n", CarrierKind.Immediate);

		Assert.DoesNotContain("Faulted_DotGram",   bare, StringComparison.Ordinal);
		Assert.DoesNotContain("Exception? fault;", bare, StringComparison.Ordinal);

		var built = Source(Rules + "Start : @int = b: Bad & 'x' => @(b)\nparse Start\n", CarrierKind.Immediate);

		Assert.Contains("Faulted_DotGram", built, StringComparison.Ordinal);
		Assert.Contains("Thrown_DotGram",  built, StringComparison.Ordinal);

		var tape = Source(Rules + "Start : @int = b: Bad & 'x' => @(b)\nparse Start\n", CarrierKind.Tape);

		Assert.DoesNotContain("Faulted_DotGram", tape, StringComparison.Ordinal);
	}

	/// <summary>Both carriers read the input and answer the same: the verdict and the value, or the exception.</summary>
	static void Alike(string grammar, string input, string expected, int? partSize = null, string? mustContain = null)
	{
		foreach (var carrier in new[] { CarrierKind.Tape, CarrierKind.Immediate })
		{
			var source = Source(grammar, carrier, partSize);

			if (carrier == CarrierKind.Immediate)
			{
				Assert.Contains("ImmediateValues", source, StringComparison.Ordinal);

				if (mustContain is not null)
					Assert.Contains(mustContain, source, StringComparison.Ordinal);
			}

			var answer = Answer(EmittedCode.Compile(source, declarationMembers: Members), input);

			Assert.True(expected == answer, $"{carrier} on \"{input}\": expected {expected}, got {answer}.");
		}
	}

	static Assembly Compiled(string grammar, CarrierKind carrier)
	{
		return EmittedCode.Compile(Source(grammar, carrier), declarationMembers: Members);
	}

	static string Source(string grammar, CarrierKind carrier, int? partSize = null)
	{
		var compiled = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName = "Grammar", Carrier = carrier, PartSize = partSize, CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity == GramSeverity.Error);

		return Assert.Single(compiled.Sources).Text;
	}

	/// <summary>What <c>TryParseStart</c> answered: the value, the refusal, or what it threw.</summary>
	static string Answer(Assembly assembly, string input)
	{
		var type = assembly.GetType("Grammar")!;

		try
		{
			var match = type.GetMethod("TryParseStart", [typeof(string)])!.Invoke(null, [input])!;

			object? Read(string name)
			{
				return match.GetType().GetProperty(name)!.GetValue(match);
			}

			return (bool)Read("IsSuccess")!
				? "accepted " + Read("Value")
				: $"{Read("Outcome")} at {Read("Position")}: {Read("Error")}";
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is { } inner)
		{
			return $"threw {inner.GetType().Name}: {inner.Message}";
		}
	}

	/// <summary>What <c>ParseStart</c> answered: the value, or what it threw.</summary>
	static string Parsed(Assembly assembly, string input)
	{
		var type = assembly.GetType("Grammar")!;

		try
		{
			return "value " + type.GetMethod("ParseStart", [typeof(string)])!.Invoke(null, [input]);
		}
		catch (TargetInvocationException thrown) when (thrown.InnerException is { } inner)
		{
			return $"threw {inner.GetType().Name}: {inner.Message}";
		}
	}
}
