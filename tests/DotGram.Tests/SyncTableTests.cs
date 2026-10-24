using System;
using System.IO;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A guard or a capture written directly in the synchronization of a <c>recover</c> means what it
/// means in a rule that is the synchronization: the part is a rule of its own there. Each grammar is
/// held to the same grammar with the part named, on every input, in every form and carrier.
/// </summary>
public sealed class SyncTableTests
{
	const string Elements = "I = 'x' & ';'\n";

	public static TheoryData<string, string> Spellings => new()
	{
		{ "when @(true) & ';'",                       "when @(true) & ';'" },
		{ "when @(false) | ';'",                      "when @(false) | ';'" },
		{ "c: ';'",                                   "c: ';'" },
		{ "c: ';' & when @(c == \";\")",              "c: ';' & when @(c == \";\")" },
		{ "d: ';' & when @(d.Length == 1) & 'z'?",    "d: ';' & when @(d.Length == 1) & 'z'?" },
		{ "(c: ';' | d: '!') & when @(true)",         "(c: ';' | d: '!') & when @(true)" },
		{ "(c: ';' & when @(true))+",                 "(c: ';' & when @(true))+" },
		{ "x: ';' & (y: 'q' & when @(y == \"q\"))?", "x: ';' & (y: 'q' & when @(y == \"q\"))?" },
	};

	static readonly string[] Inputs =
	[
		"", "x;", "x;x;", "y", "y;", "x;y;", "x;y;x;", "x;y;;x;", "x;y!x;", "x;yqz;x;", "x;y;z;x;y;", "x;y", "x;yz", "x;y;!",
	];

	[Theory]
	[MemberData(nameof(Spellings))]
	public void A_synchronization_written_in_place_reads_as_the_rule_that_names_it(string written, string named)
	{
		var inPlace = Elements + $"S : @int = xs: I* recover ({written}) & eof => @(xs.Length)\nparse S";
		var byName  = Elements + $"Sync = {named}\nS : @int = xs: I* recover Sync & eof => @(xs.Length)\nparse S";

		foreach (var direct in new[] { true, false })
			foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape })
				foreach (var buffered in new[] { false, true })
				{
					var here  = Built(inPlace, direct, carrier, buffered);
					var there = Built(byName, direct, carrier, buffered);

					foreach (var input in Inputs)
					{
						var a = Read(here, input, buffered);
						var b = Read(there, input, buffered);

						Assert.True(
							a.IsSuccess == b.IsSuccess && Equals(a.Value, b.Value) && a.Position == b.Position,
							$"direct {direct}, {carrier}, buffered {buffered}, \"{input}\": in place {a}, by name {b}");

						// What a refusal says is the rule's, and the part made of the synchronization has no
						// name to be told by.
						Assert.DoesNotContain("_Sync", a.Error ?? "", StringComparison.Ordinal);
						Assert.DoesNotContain("_Nested", a.Error ?? "", StringComparison.Ordinal);
					}
				}
	}

	/// <summary>A <c>recover</c> inside the synchronization is a rule of its own as well, with its own guard.</summary>
	[Theory]
	[InlineData("xs: I* recover (ys: I* recover (when @(true) & ';') & when @(true))")]
	[InlineData("xs: I* recover (c: ';' | ys: I* recover (c: ';' & when @(c == \";\")))")]
	public void A_recover_in_a_synchronization_may_hold_one_too(string repetition)
	{
		var grammar = Elements + $"S : @int = {repetition} & eof => @(xs.Length)\nparse S";

		foreach (var direct in new[] { true, false })
			foreach (var carrier in new[] { CarrierKind.Auto, CarrierKind.Tape })
			{
				var built = Built(grammar, direct, carrier, buffered: false);

				foreach (var input in Inputs)
					_ = Read(built, input, buffered: false);
			}
	}

	/// <summary>
	/// A guard in a synchronization reads what the synchronization captured before it, as a guard in
	/// a rule does, and not what the rule around it did.
	/// </summary>
	[Theory]
	[InlineData("n: 'a' & xs: I* recover (when @(n == \"a\") & ';') & eof => @(xs.Length)")]
	[InlineData("n: 'a' & xs: I* recover (';' & when @(n == \"a\")) & eof => @(xs.Length)")]
	[InlineData("xs: I* recover (when @(xs.Length > 0) & ';') & eof => @(xs.Length)")]
	public void A_guard_in_a_synchronization_cannot_read_what_the_rule_captured(string rule)
	{
		var result = GramCompiler.Compile(Elements + $"S : @int = {rule}\nparse S", new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.Contains(result.Diagnostics, static one => one.Id == GrammarNormalizer.NestedRecoveryFlow);
	}

	static Assembly Built(string grammar, bool direct, CarrierKind carrier, bool buffered)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			ClassName     = "Probe",
			Direct        = direct,
			Carrier       = carrier,
			BufferedInput = buffered,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		return EmittedCode.Compile(result.Sources[0].Text, className: "Probe", sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
	}

	static (bool IsSuccess, object? Value, string? Error, long Position) Read(Assembly assembly, string input, bool buffered)
	{
		if (!buffered)
			return EmittedCode.Match(assembly, "Probe", "TryParseS", input);

		var match = assembly.GetType("Probe")!.GetMethod("TryParseS", [typeof(TextReader), typeof(int?), typeof(int?)])!.Invoke(null, [new StringReader(input), null, null])!;

		object? Get(string name)
		{
			return match.GetType().GetProperty(name)!.GetValue(match);
		}

		return ((bool)Get("IsSuccess")!, Get("Value"), (string?)Get("Error"), Convert.ToInt64(Get("Position")));
	}
}
