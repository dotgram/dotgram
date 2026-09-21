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

	/// <summary>
	/// Every pool lets go of a kept store after eight parses that did not use the room.
	/// </summary>
	/// <remarks>
	/// <para>
	/// One policy, five pools, one test — because on 2026-09-20 the policy was fixed in the two
	/// the generator writes and stayed broken in the three that carry hand-written copies of the
	/// same logic, one of which had been written an hour earlier by copying the broken pattern.
	/// A defect spreads by duplication faster than a fix does, and it does so even when whoever
	/// is copying understands the problem completely. So the rule is held here rather than
	/// described anywhere: a sixth pool inherits the test, not the intention.
	/// </para>
	/// <para>
	/// What is asserted is the policy and not its implementation. A return whose parse used the
	/// room resets the count; a return whose parse did not advances it; at eight the store stops
	/// being held strongly and is reachable only through the weak reference. The count is read
	/// from the field on every step rather than inferred from the end state, because this counter
	/// has now been written wrong twice — once reading rentals, which every rental reset, and once
	/// proposed against capacity, which never shrinks and so would have reset just as reliably.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData(CarrierKind.Tape, "DirectValues")]
	[InlineData(CarrierKind.Tape, "Ways")]
	[InlineData(CarrierKind.Immediate, "ImmediateValues")]
	[InlineData(CarrierKind.Tape, "Tokens_DotGram")]
	[InlineData(CarrierKind.Tape, "Parser")]
	public void A_kept_store_is_let_go_after_eight_parses_that_did_not_use_the_room(CarrierKind carrier, string name)
	{
		const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
		const int Budget = 1048576;

		// Two of the five exist only under their own conditions, so the grammar is the one
		// that produces the pool being asked about rather than one grammar bent to fit all.
		// The lexer's buffer needs a grammar read as tokens; the engine's arena needs one the
		// engine is emitted for, and `find` is the cheapest thing that asks for it — but `find`
		// hunts through characters, so it is refused over tokens (GRAM5004) and the two cannot
		// be the same grammar. What is held to one policy is the pools, not the grammar.
		var engine  = name == "Parser";
		var compiled = GramCompiler.Compile($$"""
			trivia = { ' '* }
			Start : @int = items: Item+ => @(items.Length)
			Item : @int = "a" => @(1)
			parse Start
			{{(engine ? "find Item as AnyItem" : "")}}
			""", new GramCompilerOptions
		{
			ClassName = "Grammar", Lexical = !engine, Carrier = carrier,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});
		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity != GramSeverity.Info);
		var assembly = EmittedCode.Compile(Assert.Single(compiled.Sources).Text);
		var owner = assembly.GetType("Grammar")!;
		var type = owner.GetNestedType(name, Flags)!;
		var rent = type.GetMethod("Rent", Flags)
			?? owner.GetMethod("Rented_DotGram", Flags) ?? owner.GetMethod("Recycled", Flags);
		var release = type.GetMethod("Return", Flags)
			?? owner.GetMethod("Recycle_DotGram", Flags) ?? owner.GetMethod("Recycle", Flags);

		// The arena keeps its own rental on the host and names it differently again; the point
		// of this test is that none of those spellings is allowed to mean a different policy.
		if (name == "Parser")
		{
			rent = owner.GetMethod("Recycled", Flags);
			release = owner.GetMethod("Recycle", Flags);
		}

		// The slot, its count and where it goes afterwards, found by what they hold rather than
		// by name: each pool spells them differently and the policy is the same.
		var slot = new[] { type, owner }
			.SelectMany(where => where.GetFields(Flags))
			.Single(field => field.IsStatic && field.FieldType == type
				&& field.Name.StartsWith("_large", StringComparison.Ordinal));
		var held = slot.DeclaringType!;
		var idle = held.GetField(slot.Name + "Idle", Flags)!;
		var letGo = held.GetField(slot.Name + "LetGo", Flags)!;

		var store = rent!.Invoke(null, null)!;
		var arrays = type.GetFields(Flags).Where(field => !field.IsStatic && field.FieldType.IsArray).ToArray();
		var counts = type.GetFields(Flags).Where(field => !field.IsStatic && field.FieldType == typeof(int)).ToArray();

		// Past the bound in capacity, so every return below takes the branch that keeps it.
		// The arena is one level down: a Parser holds an arena, and the arena holds the room.
		var arena = type.GetFields(Flags).FirstOrDefault(field => field.Name == "Entries")?.GetValue(store);
		var room  = arena?.GetType().GetFields(Flags).Single(field => field.FieldType.IsArray);
		var count = arena?.GetType().GetFields(Flags).Single(field => field.FieldType == typeof(int));

		if (arena is not null)
			room!.SetValue(arena, Array.CreateInstance(room.FieldType.GetElementType()!, 65_537));
		else
		{
			foreach (var field in arrays)
				field.SetValue(store, Array.CreateInstance(field.FieldType.GetElementType()!, 0));
			arrays[0].SetValue(store, Array.CreateInstance(arrays[0].FieldType.GetElementType()!, Budget + 1));
		}

		void Used(int howMuch)
		{
			if (arena is not null)
				count!.SetValue(arena, howMuch);
			else
				foreach (var field in counts)
					field.SetValue(store, howMuch);
		}

		// A parse that used the room: kept, and the count starts again.
		Used(arena is not null ? 65_537 : Budget + 1);
		release!.Invoke(null, [store]);
		Assert.Same(store, slot.GetValue(null));
		Assert.Equal(0, idle.GetValue(null));

		// Seven that did not: still kept, and the count is read at every step rather than at
		// the end — a counter that never advances and one that advances and is reset look the
		// same from the far side, and this one has been written wrong in both of those ways.
		for (var parse = 1; parse <= 7; parse++)
		{
			Used(0);
			release!.Invoke(null, [store]);

			Assert.Same(store, slot.GetValue(null));
			Assert.Equal(parse, idle.GetValue(null));
		}

		// A parse that used the room again puts the count back to nothing: the store is wanted.
		Used(arena is not null ? 65_537 : Budget + 1);
		release!.Invoke(null, [store]);
		Assert.Same(store, slot.GetValue(null));
		Assert.Equal(0, idle.GetValue(null));

		// And eight in a row that did not let it go: the slot is empty and what was in it is
		// reachable only weakly, which is the whole of the policy.
		for (var parse = 1; parse <= 8; parse++)
		{
			Used(0);
			release!.Invoke(null, [store]);
		}

		Assert.Null(slot.GetValue(null));

		var weak = letGo.GetValue(null);
		Assert.NotNull(weak);
		var target = weak!.GetType().GetMethod("TryGetTarget")!;
		object?[] arguments = [null];
		Assert.True((bool)target.Invoke(weak, arguments)!);
		Assert.Same(store, arguments[0]);
	}

	/// <summary>
	/// The ordinary slot lets go too, once the parses stop wanting the room it holds.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The bound above governs what may be <em>parked</em>. Nothing governed what was
	/// <em>kept</em>: a store grown once for a large document sat in <c>_spare</c> for the life
	/// of the thread, which a retained-bytes readout showed as 8.5 MB of one store still held
	/// after the bound and the parked slot's release were both in place. This is that half.
	/// </para>
	/// <para>
	/// The quantity is the record tables against this parse's record count, and not the whole
	/// store against its whole use. A dense store carries three hundred value tables, each grown
	/// to the largest record index written in it, so their capacities sum to several times the
	/// record count even when every one of them fits — a test of total room against total use
	/// fires on a store that is exactly the right size. The margin is four and not two because a
	/// table grows to <c>Math.Max(count, Length * 2)</c>: one doubling is the slack a steady
	/// workload leaves behind, and two is a workload that has actually shrunk.
	/// </para>
	/// <para>
	/// Two of the five pools are under this rule and three are not, and the three are carried
	/// here asserting that they have no such counter — so the gap is visible in the test that
	/// owns the policy rather than absent from it, and closing it fails this case rather than
	/// passing silently. That is how <c>Tokens_DotGram</c> was carried before it came under the
	/// rule above.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData(CarrierKind.Tape, "DirectValues", true)]
	[InlineData(CarrierKind.Immediate, "ImmediateValues", true)]
	[InlineData(CarrierKind.Tape, "Ways", false)]
	[InlineData(CarrierKind.Tape, "Tokens_DotGram", false)]
	[InlineData(CarrierKind.Tape, "Parser", false)]
	public void The_ordinary_spare_is_let_go_after_eight_parses_that_left_its_room_unused(
		CarrierKind carrier, string name, bool releases)
	{
		const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

		var engine   = name == "Parser";
		var compiled = GramCompiler.Compile($$"""
			trivia = { ' '* }
			Start : @int = items: Item+ => @(items.Length)
			Item : @int = "a" => @(1)
			parse Start
			{{(engine ? "find Item as AnyItem" : "")}}
			""", new GramCompilerOptions
		{
			ClassName = "Grammar", Lexical = !engine, Carrier = carrier,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity != GramSeverity.Info);

		var assembly = EmittedCode.Compile(Assert.Single(compiled.Sources).Text);
		var owner    = assembly.GetType("Grammar")!;
		var type     = owner.GetNestedType(name, Flags)!;
		var counters = new[] { type, owner }
			.SelectMany(where => where.GetFields(Flags))
			.Where(field => field.IsStatic && !field.IsLiteral && field.FieldType == typeof(int)
				&& field.Name.IndexOf("spare", StringComparison.OrdinalIgnoreCase) >= 0
				&& field.Name.EndsWith("Idle", StringComparison.Ordinal))
			.ToArray();

		if (!releases)
		{
			// The gap, asserted rather than left out. When this pool comes under the rule the
			// case fails here, which is the notice that its row should move to true.
			Assert.Empty(counters);

			return;
		}

		// Not literals: the policy's own SpareIdle is a const, which is a static int field of
		// that name too, and a search by name alone finds the threshold beside the counter.
		// Named, because a lexical grammar carries more than one pool on the host class and
		// the counter asked about here is the one belonging to the pool named by the case.
		var idle  = Assert.Single(counters, field => field.DeclaringType == type);
		var held  = idle.DeclaringType!;
		var slot     = held.GetFields(Flags).Single(field => field.IsStatic && field.FieldType == type
			&& field.Name == idle.Name[..^"Idle".Length]);
		var letGo    = held.GetField(idle.Name[..^"Idle".Length] + "LetGo", Flags)!;
		var rent     = type.GetMethod("Rent", Flags)!;
		var giveBack = type.GetMethod("Return", Flags)!;

		// Small enough in capacity that every return below takes the ordinary branch and not
		// the parked one, and one array long enough that a parse can leave most of it unused.
		var store  = rent.Invoke(null, null)!;
		var arrays = type.GetFields(Flags).Where(field => !field.IsStatic && field.FieldType.IsArray).ToArray();
		var counts = type.GetFields(Flags).Where(field => !field.IsStatic && field.FieldType == typeof(int)).ToArray();
		var room   = name == "DirectValues"
			? arrays.Single(field => field.Name == "Live")
			: arrays.First(field => field.Name.StartsWith("Stack", StringComparison.Ordinal));

		foreach (var field in arrays)
			field.SetValue(store, Array.CreateInstance(field.FieldType.GetElementType()!, 0));

		room.SetValue(store, Array.CreateInstance(room.FieldType.GetElementType()!, 64));

		void Used(int howMuch)
		{
			foreach (var field in counts)
				field.SetValue(store, howMuch);
		}

		// Sixteen of sixty-four is inside the margin: the room is wanted, and the count is nil.
		Used(16);
		giveBack.Invoke(null, [store]);
		Assert.Same(store, slot.GetValue(null));
		Assert.Equal(0, idle.GetValue(null));

		// Eight of sixty-four is two doublings out. Seven such parses and the store is still
		// held — and the count is read from the field at every step, never inferred from the
		// end state, because a counter that never advances and one that advances and is reset
		// are indistinguishable from the far side.
		for (var parse = 1; parse <= 7; parse++)
		{
			Assert.Same(store, rent.Invoke(null, null));

			Used(8);
			giveBack.Invoke(null, [store]);

			Assert.Same(store, slot.GetValue(null));
			Assert.Equal(parse, idle.GetValue(null));
		}

		// One parse that wants the room puts it back to nothing.
		Assert.Same(store, rent.Invoke(null, null));
		Used(16);
		giveBack.Invoke(null, [store]);
		Assert.Same(store, slot.GetValue(null));
		Assert.Equal(0, idle.GetValue(null));

		// And eight in a row that do not: the slot is empty and the store is reachable only
		// weakly, which is the whole of the policy — it stops competing for the memory, and it
		// is still there for a thread that wants it straight back.
		for (var parse = 1; parse <= 8; parse++)
		{
			Assert.Same(store, rent.Invoke(null, null));

			Used(8);
			giveBack.Invoke(null, [store]);
		}

		Assert.Null(slot.GetValue(null));

		var weak = letGo.GetValue(null);

		Assert.NotNull(weak);

		object?[] arguments = [null];

		Assert.True((bool)weak!.GetType().GetMethod("TryGetTarget")!.Invoke(weak, arguments)!);
		Assert.Same(store, arguments[0]);
	}
}
