using System;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

public sealed class PoolRetentionTests
{
	[Theory]
	[InlineData(CarrierKind.Tape, "DirectValues")]
	[InlineData(CarrierKind.Tape, "Ways")]
	[InlineData(CarrierKind.Immediate, "ImmediateValues")]
	[InlineData(CarrierKind.Tape, "Tokens_DotGram")]
	public void Oversized_stores_are_not_retained_or_allowed_to_replace_a_nested_spare(CarrierKind carrier, string name)
	{
		const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
		const int Budget = 1048576;
		var compiled = GramCompiler.Compile("""
			trivia = { ' '* }
			Start : @int = items: Item+ => @(items.Length)
			Item : @int = "a" => @(1)
			parse Start
			""", new GramCompilerOptions
		{
			ClassName = "Grammar", Lexical = true, Carrier = carrier,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity != GramSeverity.Info);
		var assembly = EmittedCode.Compile(Assert.Single(compiled.Sources).Text);
		var owner = assembly.GetType("Grammar")!;
		var type = owner.GetNestedType(name, Flags)!;
		var rent = (name == "Tokens_DotGram" ? owner.GetMethod("Rented_DotGram", Flags) : type.GetMethod("Rent", Flags))!;
		var release = (name == "Tokens_DotGram" ? owner.GetMethod("Recycle_DotGram", Flags) : type.GetMethod("Return", Flags))!;
		var arrays = type.GetFields(Flags).Where(field => !field.IsStatic && field.FieldType.IsArray).ToArray();

		object Rent() => rent.Invoke(null, null)!;
		void Return(object value) => release.Invoke(null, [value]);
		void Capacity(object value, int count)
		{
			foreach (var field in arrays)
				field.SetValue(value, Array.CreateInstance(field.FieldType.GetElementType()!, 0));
			var first = arrays[0];
			var firstCount = arrays.Length > 1 ? count / 2 : count;
			first.SetValue(value, Array.CreateInstance(first.FieldType.GetElementType()!, firstCount));
			if (arrays.Length > 1)
				arrays[1].SetValue(value, Array.CreateInstance(arrays[1].FieldType.GetElementType()!, count - firstCount));
		}

		var ordinary = Rent();
		Return(ordinary);
		Assert.Same(ordinary, Rent());

		Capacity(ordinary, Budget);
		Return(ordinary);
		Assert.Same(ordinary, Rent());

		Capacity(ordinary, Budget + 1);
		Return(ordinary);
		var fresh = Rent();
		Assert.NotSame(ordinary, fresh);

		// A reentrant parse can return a smaller store while the outer one is active.
		Return(fresh);
		Return(ordinary);
		Assert.Same(fresh, Rent());

		// Replacing synthetic test buffers with a normal rental leaves parsing usable.
		var match = EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a a a");
		Assert.True(match.IsSuccess);
		Assert.Equal(3, match.Value);
	}
}
