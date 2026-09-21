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
	/// <summary>
	/// A store past the bound is kept in a slot of its own, and comes back as new.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This asserted the opposite until 2026-09-20, and it was right to: an oversized store used
	/// to be dropped. That was a cliff rather than a bound — one entry over and the next parse of
	/// a document that size grew everything again from nothing — so the decision was reversed and
	/// the store is now held while the work keeps wanting it. The test is rewritten to the policy
	/// that was chosen rather than deleted, because a test contradicting a deliberate change of
	/// design looks exactly like a test that caught a regression, and only knowing which way the
	/// decision went tells them apart.
	/// </para>
	/// <para>
	/// The second assertion is the one that was missing. Keeping the store was implemented by
	/// returning early, which skipped the lines that empty the tables <em>and</em> the line that
	/// resets the cursor — so a kept store was handed to the next parse still believing it held
	/// the last one. That is not a leak but corruption, and it failed 190 tests across suites
	/// with no connection to pooling. A store comes back from its slot indistinguishable from a
	/// new one in everything but capacity, and that is what is checked here.
	/// </para>
	/// <para>
	/// All four pools are under this policy. <c>Tokens_DotGram</c> was not when this was written
	/// and was carried here anyway, asserting that it dropped — so that the gap stayed visible in
	/// the test that owns the rule rather than being absent from it. Bringing it under the rule is
	/// what made that case fail, which is the whole of why it was written that way.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData(CarrierKind.Tape, "DirectValues")]
	[InlineData(CarrierKind.Tape, "Ways")]
	[InlineData(CarrierKind.Immediate, "ImmediateValues")]
	[InlineData(CarrierKind.Tape, "Tokens_DotGram")]
	public void An_oversized_store_is_kept_in_a_slot_of_its_own_and_comes_back_as_new(CarrierKind carrier, string name)
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

		// What a store looks like when it is ready to read: the state every rental must be in,
		// whichever slot it came from. Read from a new one, so it is the runtime's own answer
		// rather than a list of field names this test would have to keep in step.
		int[] Counters(object value) => type.GetFields(Flags)
			.Where(field => !field.IsStatic && field.FieldType == typeof(int))
			.OrderBy(field => field.Name, StringComparer.Ordinal)
			.Select(field => (int)field.GetValue(value)!)
			.ToArray();

		var ordinary = Rent();
		var asNew    = Counters(ordinary);

		Return(ordinary);
		Assert.Same(ordinary, Rent());

		Capacity(ordinary, Budget);
		Return(ordinary);
		Assert.Same(ordinary, Rent());

		// Past the bound. A second rental while the first is out has nothing to hand back, so
		// it builds one: that is how this gets two stores to talk about at once.
		Capacity(ordinary, Budget + 1);
		var nested = Rent();
		Assert.NotSame(ordinary, nested);

		Return(ordinary);

		// A reentrant parse returns its smaller store while the large one is already parked.
		// The ordinary spare is handed back first, and the large one is not lost behind it.
		Return(nested);
		Assert.Same(nested, Rent());
		var fresh = Rent();
		Assert.Same(ordinary, fresh);

		// And it came back as a new one would: nothing carried over from the parse that grew it.
		Assert.Equal(asNew, Counters(fresh));

		// Replacing synthetic test buffers with a normal rental leaves parsing usable.
		var match = EmittedCode.Match(assembly, "Grammar", "TryParseStart", "a a a");
		Assert.True(match.IsSuccess);
		Assert.Equal(3, match.Value);
	}
}
