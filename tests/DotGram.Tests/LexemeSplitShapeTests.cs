using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a name read by a run that gives characters back reads, where what follows the name could not
/// take a character it gave back, beside a guard and beside a recovery: the verdict, the value, the
/// message and where a refusal stands, as they are.
/// </summary>
/// <remarks>
/// <para>
/// A rule that ends in a run of a class gives characters back to whatever stands after its call, and
/// at a call followed by something that cannot begin with a character of the class, every character
/// given back is refused at once. These are the places where that is less plain: a guard between the
/// name and what follows it, which sees the name a give-back shortens, and a repetition marked
/// <c>recover</c> after the name or around it, whose resumption depends on how far the bad element
/// was read.
/// </para>
/// <para>
/// Each row is read with the generator's defaults and held to what it gives today, so that a change to
/// how such a name is read shows up as a changed row and not as a changed grammar.
/// </para>
/// </remarks>
public sealed class LexemeSplitShapeTests
{
	/// <summary>A guard between the name and the character after it; and one before a second name that may take what the first gives back.</summary>
	const string Guarded = """
		trivia = none

		NameText = ['a'..'z'] & ['a'..'z']*

		Name  : @string = text: NameText => @(text)
		Pair  : @string = name: Name & when @(name.Length <= 2) on fail "Expected a short name." & '=' & value: Name => @(name + "=" + value)
		Spill : @string = name: Name & when @(name.Length <= 2) on fail "Expected a short name." & rest: Name & '=' => @(name + "|" + rest)
		Start : @string = pair: Pair & eof => @(pair)
		                | spill: Spill & eof => @(spill)

		parse Start
		""";

	/// <summary>The name inside the element of a repetition marked <c>recover</c>, and a name followed by one.</summary>
	const string Recovering = """
		trivia = none

		NameText = ['a'..'z'] & ['a'..'z']*

		Name  : @string = text: NameText => @(text)
		Entry : @string = name: Name & '=' & value: Name & ';' => @(name + "=" + value)
		Value : @string = '=' & text: Name => @(text)
		Line  : @string = name: Name & values: Value* recover ',' => @("!") & '.' => @(name + ":" + string.Join(",", values))
		Start : @string = '[' & items: Entry* recover ';' => @("!") & ']' => @(string.Join(" ", items))
		                | line: Line => @(line)

		parse Start
		""";

	/// <summary>Each grammar, an input, and what it gives today: accepted with the value, or refused with the message at the position.</summary>
	static readonly (string Grammar, string Input, bool Accepted, string Said, long Position)[] Today =
	[
		("Guarded", "ab=c", true, "ab=c", 0),
		("Guarded", "abc=d", false, "Expected end of input.", 4),
		("Guarded", "abc=", true, "ab|c", 0),
		("Guarded", "abcd=", true, "ab|cd", 0),
		("Guarded", "a=", false, "Expected ['a'..'z'].", 2),
		("Guarded", "ab", false, "Expected '=' or ['a'..'z'].", 2),
		("Guarded", "abc", false, "Expected '='.", 3),
		("Guarded", "ab=", true, "a|b", 0),
		("Guarded", "=", false, "Expected ['a'..'z'].", 0),
		("Guarded", "", false, "Expected ['a'..'z'].", 0),
		("Recovering", "[a=b;]", true, "a=b", 0),
		("Recovering", "[a=b;c=d;]", true, "a=b c=d", 0),
		("Recovering", "[ab=;c=d;]", true, "! c=d", 0),
		("Recovering", "[abc;d=e;]", true, "! d=e", 0),
		("Recovering", "[a=b;c=;]", true, "a=b !", 0),
		("Recovering", "[a=bc]", false, "Expected ']' or NameText.", 6),
		("Recovering", "[", false, "Expected ']' or NameText.", 1),
		("Recovering", "a.", true, "a:", 0),
		("Recovering", "a=b.", true, "a:b", 0),
		("Recovering", "a=b=c.", true, "a:b,c", 0),
		("Recovering", "a=,=b.", true, "a:!,b", 0),
		("Recovering", "ab=cd!", false, "Expected '.' or '='.", 6),
		("Recovering", "a==b.", false, "Expected '.' or '='.", 5),
		("Recovering", "abc", false, "Expected '.' or '='.", 3),
		("Recovering", "a=b", false, "Expected '.' or '='.", 3),
	];

	public static IEnumerable<TheoryDataRow<string, string>> Rows()
	{
		return Today.Select(static row => new TheoryDataRow<string, string>(row.Grammar, row.Input));
	}

	[Theory]
	[MemberData(nameof(Rows))]
	public void A_name_beside_a_guard_or_a_recovery_reads_as_it_does_today(string grammar, string input)
	{
		var row      = Today.Single(one => one.Grammar == grammar && one.Input == input);
		var assembly = Compiled(grammar == nameof(Guarded) ? Guarded : Recovering);
		var (accepted, value, error, position) = EmittedCode.Match(assembly, "Grammar", "TryParseStart", input);

		Assert.Equal(row.Accepted, accepted);
		Assert.Equal(row.Said, accepted ? (string?)value : error);
		Assert.Equal(row.Position, position);
	}

	static readonly Dictionary<string, Assembly> _compiled = [];

	static Assembly Compiled(string grammar)
	{
		lock (_compiled)
		{
			if (!_compiled.TryGetValue(grammar, out var assembly))
			{
				var result = GramCompiler.Compile(grammar, new GramCompilerOptions
				{
					ClassName     = "Grammar",
					CSharpScanner = RoslynCSharpScanner.Instance,
				});

				EmittedCode.Quiet(result.Diagnostics);

				_compiled[grammar] = assembly = EmittedCode.Compile(Assert.Single(result.Sources).Text, "Grammar");
			}

			return assembly;
		}
	}
}
