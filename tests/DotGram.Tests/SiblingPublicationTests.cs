using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Tests;

public sealed class SiblingPublicationTests
{
	[Theory]
	[InlineData(8, false, 2)]
	[InlineData(130, false, 1)]
	[InlineData(130, true, 2)]
	public void Shared_rules_preserve_root_results_and_end_of_input(int count, bool guard, int materializers)
	{
		var grammar = string.Join("\n", Enumerable.Range(0, count - 1)
			.Select(i => $"R{i} : @int = n: R{i + 1} => @(n + 1)")) +
			$"\nR{count - 1} : @int = '(' & n: R0 & ')' => @(n + 1) | 'v' => @(1)\n" +
			"Number : @int = n: R0" + (guard ? " & when @(n > 0)" : "") + " => @(n)\n" +
			"Text : @string = n: R0 => @(n.ToString())\nparse Number\nparse Text";
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier = CarrierKind.Tape, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		var methods = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)
			.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>();
		Assert.Equal(materializers, methods.Count(m => m.Identifier.Text.StartsWith("Materialize_DotGram_") && m.Identifier.Text.EndsWith("_Direct")));
		var assembly = EmittedCode.Compile(source);
		Assert.Equal(count * 3, EmittedCode.Match(assembly, "Grammar", "TryParseNumber", "((v))").Value);
		Assert.Equal((count * 3).ToString(), EmittedCode.Match(assembly, "Grammar", "TryParseText", "((v))").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseNumber", "v!").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseText", "v!").IsSuccess);
	}

	[Fact]
	public void Shared_machines_preserve_each_publications_reading()
	{
		var grammar = "Version = \"Old\" | \"New\"\n" +
			string.Join("\n", Enumerable.Range(0, 129)
				.Select(i => $"R{i} : @int = n: R{i + 1} => @(n + 1)")) +
			"\nR129 : @int = '(' & n: R0 & ')' => @(n + 1)" +
			" | when Version is \"Old\" & 'o' => @(1)" +
			" | when Version is \"New\" & 'n' => @(2)\n" +
			"Number : @int = n: R0 => @(n)\n" +
			"Text : @string = n: R0 => @(n.ToString())\n" +
			"parse Number with (Version = \"Old\") as OldNumber\n" +
			"parse Text with (Version = \"New\") as NewText";
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier = CarrierKind.Tape, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		var methods = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)
			.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>();
		Assert.Single(methods, m => m.Identifier.Text.StartsWith("Materialize_DotGram_") && m.Identifier.Text.EndsWith("_Direct"));
		var assembly = EmittedCode.Compile(source);

		for (var i = 0; i < 2; i++)
		{
			Assert.Equal(390, EmittedCode.Match(assembly, "Grammar", "TryOldNumber", "((o))").Value);
			Assert.Equal("391", EmittedCode.Match(assembly, "Grammar", "TryNewText", "((n))").Value);
			Assert.False(EmittedCode.Match(assembly, "Grammar", "TryOldNumber", "((n))").IsSuccess);
			Assert.False(EmittedCode.Match(assembly, "Grammar", "TryNewText", "((o))").IsSuccess);
		}
	}
	[Fact]
	public void Joined_publications_preserve_each_roots_failure_expectations()
	{
		var rules = string.Join("\n", Enumerable.Range(0, 129)
			.Select(i => $"R{i} : @int = n: R{i + 1} => @(n + 1)")) +
			"\nR129 : @int = '(' & n: R0 & ')' => @(n + 1) | 'v' => @(1)\n" +
			"Number : @int = 'n' & n: R0 => @(n)\n" +
			"Text : @string = 't' & n: R0 => @(n.ToString())\n";
		var options = new GramCompilerOptions
		{
			Carrier = CarrierKind.Tape, CSharpScanner = RoslynCSharpScanner.Instance,
		};
		var together = GramCompiler.Compile(rules + "parse Number\nparse Text", options);
		EmittedCode.Quiet(together.Diagnostics);
		var combined = EmittedCode.Compile(Assert.Single(together.Sources).Text);

		foreach (var root in new[] { "Number", "Text" })
		{
			var separate = GramCompiler.Compile(rules + "parse " + root, options);
			// The other publication root is deliberately unused in this control grammar.
			EmittedCode.Quiet(separate.Diagnostics.Where(diagnostic => diagnostic.Id != "GRAM4018"));
			var single = EmittedCode.Compile(Assert.Single(separate.Sources).Text);

			foreach (var input in new[] { "", "!", "n!", "t!", "n(v", "t(v" })
			{
				var expected = EmittedCode.Match(single, "Grammar", "TryParse" + root, input);
				var actual = EmittedCode.Match(combined, "Grammar", "TryParse" + root, input);
				Assert.False(actual.IsSuccess);
				Assert.Equal(expected.Position, actual.Position);
				Assert.Equal(expected.Error, actual.Error);
			}
		}
	}

	/// <summary>
	/// A rule streamed by more than one publication reads the same parts in each: the
	/// recognizer of a part that is not a call is the rule's, written once and shared, not
	/// written again for every publication under the same name.
	/// </summary>
	[Theory]
	[InlineData("parse Rows|parse Rows as All stream bytes")]
	[InlineData("parse Rows|parse Rows as Again|parse Rows as All stream bytes")]
	[InlineData("parse Rows as Chars stream|parse Rows|parse Rows as All stream bytes")]
	public void A_rule_streamed_by_several_publications_shares_its_parts(string publications)
	{
		var grammar = "A : @string = 'a' => @(\"a\")\n" +
			"C : @string = 'c' => @(\"c\")\n" +
			"Rows : @string[] = A* & ';' & C*\n" +
			publications.Replace('|', '\n');
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source  = Assert.Single(result.Sources).Text;
		var methods = CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)
			.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>();
		Assert.Single(methods, m => m.Identifier.Text == "Recognize_Rows_Whole_Part1");
		var host = EmittedCode.Compile(source).GetType("Grammar")!;

		foreach (var publication in publications.Split('|'))
		{
			var named  = publication.IndexOf(" as ", StringComparison.Ordinal);
			var method = named < 0 ? "ParseRows" : publication.Substring(named + 4).Split(' ')[0];
			var forms  = new List<Func<string, string[]>>
			{
				input => (string[])host.GetMethod(method, [typeof(string)])!.Invoke(null, [input])!,
			};

			// Without `stream` the reader overload is the streamed one, handing the parts back
			// as it reads them; `stream` replaces it with a buffered one that returns the whole
			// array, and `stream bytes` adds the same over bytes beside the streamed reader.
			if (publication.EndsWith(" stream", StringComparison.Ordinal))
				forms.Add(input => (string[])host.GetMethod(method, [typeof(TextReader), typeof(int?), typeof(int?)])!
					.Invoke(null, [new StringReader(input), 1, 16])!);
			else
				forms.Add(input => ((IEnumerable<string>)host.GetMethod(method, [typeof(TextReader)])!
					.Invoke(null, [new StringReader(input)])!).ToArray());

			if (publication.EndsWith(" stream bytes", StringComparison.Ordinal))
			{
				forms.Add(input => (string[])host.GetMethod(method, [typeof(Stream), typeof(int?), typeof(int?)])!
					.Invoke(null, [new MemoryStream(Encoding.ASCII.GetBytes(input)), 1, 16])!);
				forms.Add(input => (string[])host.GetMethod(method, [typeof(byte[])])!
					.Invoke(null, [Encoding.ASCII.GetBytes(input)])!);
			}

			foreach (var form in forms)
			{
				Assert.Equal(new[] { "a", "a", "c", "c", "c" }, form("aa;ccc"));
				Assert.Equal(new[] { "c" }, form(";c"));
				Assert.Empty(form(";"));
			}

			var attempt = method == "ParseRows" ? "TryParseRows" : "Try" + method;

			Assert.False(EmittedCode.Match(host.Assembly, "Grammar", attempt, "a;b").IsSuccess);
			Assert.False(EmittedCode.Match(host.Assembly, "Grammar", attempt, "aa").IsSuccess);
		}
	}

	/// <summary>
	/// Two large publications carried immediately share one machine as they would on the tape,
	/// and a small one keeps its own: the carrier a grammar is compiled on does not decide how
	/// many machines it comes out as.
	/// </summary>
	[Theory]
	[InlineData(8, 2)]
	[InlineData(130, 1)]
	public void Immediate_publications_share_a_machine_as_the_tapes_do(int count, int readers)
	{
		var grammar = Chain(count) +
			"Number : @int = n: R0 => @(n)\n" +
			"Text : @string = n: R0 => @(n.ToString())\n" +
			"parse Number\nparse Text";
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier = CarrierKind.Immediate, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		Assert.Equal(readers, Readers(source).Count);
		Assert.Empty(Materializers(source));
		Assert.Contains("ImmediateValues", source, StringComparison.Ordinal);
		var assembly = EmittedCode.Compile(source);
		Assert.Equal(count * 3, EmittedCode.Match(assembly, "Grammar", "TryParseNumber", "((v))").Value);
		Assert.Equal((count * 3).ToString(), EmittedCode.Match(assembly, "Grammar", "TryParseText", "((v))").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseNumber", "v!").IsSuccess);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseText", "v!").IsSuccess);
	}

	/// <summary>
	/// A machine the immediate carrier refuses falls back to the tape, and shares with no machine
	/// it carries: the two carriers' machines stay apart however much they overlap, and the one it
	/// carries is still carried.
	/// </summary>
	[Fact]
	public void Machines_on_different_carriers_stay_apart()
	{
		// `Mixed` gathers two members onto the int stack in one construction, which the immediate
		// carrier refuses (GRAM5007); `Number` reaches the same rules and is carried.
		var grammar = Chain(130) +
			"Number : @int = n: R0 => @(n)\n" +
			"Mixed : @int = a: R0* & ';' & b: R0* => @(a.Length * 100 + b.Length)\n" +
			"parse Number\nparse Mixed";
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier = CarrierKind.Immediate, CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		Assert.Contains(result.Diagnostics, one => one.Id == GramCompiler.CarrierRefused && one.Message.Contains("'Mixed'", StringComparison.Ordinal));
		var source = Assert.Single(result.Sources).Text;
		Assert.Equal(2, Readers(source).Count);
		Assert.Single(Materializers(source));
		Assert.Contains("ImmediateValues", source, StringComparison.Ordinal);
		var assembly = EmittedCode.Compile(source);
		Assert.Equal(390, EmittedCode.Match(assembly, "Grammar", "TryParseNumber", "((v))").Value);
		Assert.Equal(201, EmittedCode.Match(assembly, "Grammar", "TryParseMixed", "vv;v").Value);
		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseMixed", "v").IsSuccess);
	}

	/// <summary>
	/// Left to choose, a grammar whose gates let it through is carried immediately, and its
	/// large publications share one machine on that carrier as they would on the tape.
	/// </summary>
	[Fact]
	public void Immediate_machines_chosen_by_auto_share_a_machine()
	{
		var grammar = Chain(130) +
			"Number : @int = n: R0 => @(n)\n" +
			"Text : @string = n: R0 => @(n.ToString())\n" +
			"parse Number\nparse Text";
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		Assert.Contains("ImmediateValues", source, StringComparison.Ordinal);
		Assert.Empty(Materializers(source));
		Assert.Single(Readers(source));
		var assembly = EmittedCode.Compile(source);
		Assert.Equal(390, EmittedCode.Match(assembly, "Grammar", "TryParseNumber", "((v))").Value);
		Assert.Equal("390", EmittedCode.Match(assembly, "Grammar", "TryParseText", "((v))").Value);
	}

	/// <summary>
	/// Left to choose, a publication the tape would hold back — a value it builds is read where the
	/// reading may not stand — is carried immediately all the same, and shares one machine with a
	/// large publication beside it as it would under the immediate carrier named.
	/// </summary>
	/// <remarks>
	/// <c>Taped</c> builds Q in an alternative that can still fail after it — at 'x', where the next
	/// alternative reads the same 'q' as Q2 — so Q is read where the reading may not stand. That once
	/// kept <c>Taped</c> on the tape and apart from <c>Number</c>, two readers and a walk; the gates
	/// choose no carrier now, and the two publications are one machine, carried immediately.
	/// </remarks>
	[Fact]
	public void Left_to_choose_a_publication_the_tape_would_hold_back_shares_the_carried_machine()
	{
		var grammar = TapedBeside;
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
		EmittedCode.Quiet(result.Diagnostics);
		var source = Assert.Single(result.Sources).Text;
		Assert.Single(Readers(source));
		Assert.Empty(Materializers(source));
		Assert.Contains("ImmediateValues", source, StringComparison.Ordinal);
		var assembly = EmittedCode.Compile(source);
		Assert.Equal(390, EmittedCode.Match(assembly, "Grammar", "TryParseNumber", "((v))").Value);
		Assert.Equal(391, EmittedCode.Match(assembly, "Grammar", "TryParseTaped", "qx((v))").Value);
		Assert.Equal(492, EmittedCode.Match(assembly, "Grammar", "TryParseTaped", "qy((v))").Value);
	}

	/// <summary>
	/// Two large publications sharing nearly every rule, one of them building a value read where the
	/// reading may not stand (<see cref="Left_to_choose_a_publication_the_tape_would_hold_back_shares_the_carried_machine"/>).
	/// </summary>
	internal static readonly string TapedBeside = Chain(130) +
		"Q : @int = 'q' => @(1)\n" +
		"Q2 : @int = 'q' => @(2)\n" +
		"Number : @int = n: R0 => @(n)\n" +
		"Taped : @int = n: Q & 'x' & m: R0 => @(n + m) | n: Q2 & 'y' & m: R0 => @(n + m + 100)\n" +
		"parse Number\nparse Taped";

	/// <summary>A chain of <paramref name="count"/> valued rules, the last of them recursive through parentheses.</summary>
	static string Chain(int count)
	{
		return string.Join("\n", Enumerable.Range(0, count - 1)
			.Select(i => $"R{i} : @int = n: R{i + 1} => @(n + 1)")) +
			$"\nR{count - 1} : @int = '(' & n: R0 & ')' => @(n + 1) | 'v' => @(1)\n";
	}

	/// <summary>The readers of the file, one per machine written as methods.</summary>
	internal static List<string> Readers(string source)
	{
		return CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)
			.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<StructDeclarationSyntax>()
			.Select(static one => one.Identifier.Text)
			.Where(static name => name.StartsWith("Reader_DotGram", StringComparison.Ordinal))
			.ToList();
	}

	/// <summary>The walks that build a tape machine's values, one per machine on the tape.</summary>
	internal static List<string> Materializers(string source)
	{
		return CSharpSyntaxTree.ParseText(source, cancellationToken: TestContext.Current.CancellationToken)
			.GetRoot(TestContext.Current.CancellationToken).DescendantNodes().OfType<MethodDeclarationSyntax>()
			.Select(static one => one.Identifier.Text)
			.Where(static name => name.StartsWith("Materialize_DotGram_", StringComparison.Ordinal) && name.EndsWith("_Direct", StringComparison.Ordinal))
			.ToList();
	}
}
