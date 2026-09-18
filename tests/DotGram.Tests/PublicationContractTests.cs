using System;
using System.Collections;
using System.IO;
using System.Linq;
using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace DotGram.Tests;

public sealed class PublicationContractTests
{
	const string Declarations = "public interface INode {} public class BaseNode : INode {} public sealed class Leaf : BaseNode {} public class Other {}";

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void Contracts_preserve_values_across_parse_find_and_input_forms(bool direct)
	{
		var result = Compile("""
			Item : @Leaf = 'a' => @(new Leaf())
			Feed : @Leaf[] = Item+
			parse Item as Native
			parse Item as One : @BaseNode stream
			parse Item as Bytes : @INode stream bytes
			parse Feed as Enumerable : @System.Collections.Generic.IEnumerable<BaseNode>
			parse Feed as All : @BaseNode[] stream
			find Item as Found : @INode stream
			find Item as FoundBytes : @BaseNode stream bytes
			find Item as Legacy : @BaseNode
			parse Feed as Lazy yield : @BaseNode
			""", direct);
		EmittedCode.Quiet(result.Diagnostics);
		var assembly = EmittedCode.Compile(result.Sources.Single().Text, declarationMembers: Declarations + """
			public static object[] Exercise()
			{
				using var reader = new System.IO.StringReader("a");
				using var bytes = new System.IO.MemoryStream(new byte[] { 97 });
				using var all = new System.IO.StringReader("aa");
				using var find = new System.IO.StringReader("!a!a");
				using var findBytes = new System.IO.MemoryStream(new byte[] { 33, 97, 33, 97 });
				return new object[] { One("a"), One(reader), Bytes(bytes), All(all),
					TryOne("!a!", 1, 1).Value, TryOne("!").IsSuccess,
					Found("!a!a"), System.Linq.Enumerable.ToArray(Found(find)), System.Linq.Enumerable.ToArray(FoundBytes(findBytes)) };
			}
			""");
		var host = assembly.GetType("Grammar")!;
		Assert.Equal("Leaf", host.GetMethod("Native", [typeof(string)])!.ReturnType.Name);
		Assert.Equal("BaseNode", host.GetMethod("One", [typeof(string)])!.ReturnType.Name);
		Assert.Equal("BaseNode[]", host.GetMethod("All", [typeof(string)])!.ReturnType.Name);
		Assert.Equal("IEnumerable`1", host.GetMethod("Enumerable", [typeof(string)])!.ReturnType.Name);
		Assert.Equal(2, ((IEnumerable)host.GetMethod("Enumerable", [typeof(string)])!.Invoke(null, ["aa"])!).Cast<object>().Count());
		Assert.Equal("BaseNode", host.GetMethod("TryOne", [typeof(string)])!.ReturnType.GetGenericArguments()[0].Name);
		var values = (object[])host.GetMethod("Exercise")!.Invoke(null, null)!;
		foreach (var index in new[] { 0, 1, 2, 4 }) Assert.Equal("Leaf", values[index].GetType().Name);
		Assert.Equal(2, ((Array)values[3]).Length);
		Assert.Equal(false, values[5]);
		foreach (var index in new[] { 6, 7, 8 })
		{
			var matches = ((IEnumerable)values[index]).Cast<object>().ToArray();
			Assert.Equal(2, matches.Length);
			Assert.All(matches, match => Assert.Equal("Leaf", match.GetType().GetProperty("Value")!.GetValue(match)!.GetType().Name));
		}
	}

	[Theory]
	[InlineData("parse Item : @Other")]
	[InlineData("find Item : @Other")]
	[InlineData("parse Item : @BaseNode[]")]
	public void Incompatible_contracts_are_grammar_errors(string publication)
	{
		var result = Compile("Item : @Leaf = 'a' => @(new Leaf())\n" + publication);
		Assert.Contains(result.Diagnostics, d => d.Id == GrammarNormalizer.PublicationTypeMismatch);
		Assert.Empty(result.Sources);
	}

	[Fact]
	public void Specialization_is_checked_before_the_contract()
	{
		var result = Compile("""
			Item : @Leaf = 'a' => @(new Leaf())
			Replacement : @Leaf = 'b' => @(new Leaf())
			parse Item with (Item = Replacement) as Rebound : @Other
			""");
		Assert.Contains(result.Diagnostics, d => d.Id == GrammarNormalizer.PublicationTypeMismatch);
	}

	[Fact]
	public void Extent_contract_accepts_both_character_and_byte_values()
	{
		var result = Compile("Item = 'a'\nparse Item : @object stream bytes");
		EmittedCode.Quiet(result.Diagnostics);
		var assembly = EmittedCode.Compile(result.Sources.Single().Text);
		var host = assembly.GetType("Grammar")!;
		Assert.Equal("a", host.GetMethod("ParseItem", [typeof(string)])!.Invoke(null, ["a"]));
		using var bytes = new MemoryStream(new byte[] { 97 });
		Assert.Equal(new byte[] { 97 }, (byte[])host.GetMethod("ParseItem", [typeof(Stream), typeof(int?), typeof(int?)])!.Invoke(null, [bytes, 1, 16])!);
		Assert.Contains(Compile("Item = 'a'\nparse Item : @string stream bytes").Diagnostics,
			d => d.Id == GrammarNormalizer.PublicationTypeMismatch);
	}

	[Fact]
	public void Source_generator_collects_publication_type_questions()
	{
		var assembly = GeneratorDriverTests.Build(""""
			using DotGram;
			[Gram("""
				Item : @Leaf = 'a' => @(new Leaf())
				Feed : @Leaf[] = Item*
				Text = 'a'
				parse Item : @BaseNode
				parse Feed as All : @BaseNode[] stream
				parse Feed as Lazy yield : @BaseNode
				find Item : @BaseNode stream bytes
				parse Text : @object stream bytes
				""")]
			public partial class Grammar
			{
				public class BaseNode {}
				public sealed class Leaf : BaseNode {}
			}
			"""");
		Assert.Equal("BaseNode", assembly.GetType("Grammar")!.GetMethod("ParseItem", [typeof(string)])!.ReturnType.Name);
	}

	[Fact]
	public void Imported_contract_types_are_resolved_by_the_generator()
	{
		var assembly = GeneratorDriverTests.Build(""""
			using DotGram;
			namespace Nodes { public interface INode {} public sealed class Leaf : INode {} }
			[Gram("""
				@using Nodes;
				Item : @Leaf = 'a' => @(new Leaf())
				parse Item : @INode
				""")]
			public partial class Grammar {}
			"""");
		Assert.Equal("Nodes.INode", assembly.GetType("Grammar")!.GetMethod("ParseItem", [typeof(string)])!.ReturnType.FullName);
	}

	static GramCompilation Compile(string grammar, bool direct = false)
	{
		var host = CSharpCompilation.Create("PublicationTypes",
			[CSharpSyntaxTree.ParseText("public partial class Grammar { " + Declarations + " }", cancellationToken: TestContext.Current.CancellationToken)],
			[MetadataReference.CreateFromFile(typeof(object).Assembly.Location)], new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
		return GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
			SymbolResolver = new RoslynSymbolResolver(host, "Grammar"), Direct = direct,
		});
	}
}
