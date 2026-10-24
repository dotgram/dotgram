using System;
using System.Linq;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A rule nothing reaches from a publication is a rule the parser never runs, and what the parser's
/// support code is sized for must come from the rules that are written: a legal grammar whose
/// output does not compile is the generator's fault, whichever rules the publications reach.
/// </summary>
public sealed class UnreachedRuleTests
{
	/// <summary>
	/// The recovering repetition sits in a rule that two publications leave unreached: the shared
	/// machine used to write its body while the failure record it reads from was sized without it.
	/// </summary>
	[Theory]
	[InlineData("I = 'x'\nA = 'a'\nB = 'b'\nR0 = I* recover ';'\nparse A\nparse B")]
	[InlineData("I = 'x'\nA = 'a'\nB = 'b'\nR0 = I* recover ';' & eof\nparse A\nparse B")]
	[InlineData("I = 'x'\nA = 'a'\nB = 'b'\nR0 = (I & ',')* recover ';'\nparse A\nparse B")]
	[InlineData("I = 'x'\nA = 'a'+\nB = 'b'+\nR0 = I* recover ';'\nparse A stream\nparse B stream")]
	[InlineData("I = 'x'\nA = 'a'+\nB = 'b'+\nR0 = I* recover ';'\nparse A\nfind B stream")]
	[InlineData("I = 'x'\nA = 'a'\nR0 = I* recover ';'\nparse A")]
	[InlineData("I = 'x'\nA = 'a'\nR0 = I* recover ';'\nR1 = R0 & 'b'\nparse A")]
	public void A_recovery_nothing_reaches_still_compiles(string grammar)
	{
		foreach (var direct in new[] { true, false })
		{
			var result = GramCompiler.Compile(grammar, new GramCompilerOptions { Direct = direct });

			Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

			EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
		}
	}
}
