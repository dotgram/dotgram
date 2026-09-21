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
			// The room goes in the array the pool' own release reads, which for the direct store
			// is Live and not whichever array happens to be declared first: since 2026-09-21 both
			// slots ask whether the room is far larger than the use, so a test that puts the room
			// somewhere the rule does not look asks nothing at all.
			var sized = arrays.FirstOrDefault(field => field.Name == (name == "DirectValues" ? "Live" : "Kinds")) ?? arrays[0];

			foreach (var field in arrays)
				field.SetValue(store, Array.CreateInstance(field.FieldType.GetElementType()!, 0));

			sized.SetValue(store, Array.CreateInstance(sized.FieldType.GetElementType()!, Budget + 1));
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
	/// The bound governs what may be <em>parked</em>. Nothing governed what was <em>kept</em>: a
	/// store grown once for a large document sat in the ordinary slot for the life of the thread,
	/// which a retained-bytes readout showed as 8.5 MB of one store still held after the bound
	/// and the parked slot's release were both in place. This is that half, and all five pools
	/// are under it — the rule is held here rather than described anywhere, because on 2026-09-20
	/// the neighbouring policy was fixed in the two pools the generator writes and stayed broken
	/// in the three that carry hand-written copies of it.
	/// </para>
	/// <para>
	/// <b>Room against use, and for four of the five they are the same quantity twice.</b> The
	/// arena has a list's own capacity and count; the lexer's three arrays and the tape's are all
	/// indexed by one thing, so each pool's bound already sums exactly what its release counts.
	/// The dense value store is the exception: its three hundred value tables are each grown to
	/// the largest record index written <em>in that table</em>, so their capacities sum to several
	/// times the record count even when every one of them fits, and a test of total room against
	/// total use would fire on a store that is exactly the right size. It reads
	/// <c>Live.Length</c> against the record count instead.
	/// </para>
	/// <para>
	/// The margin is four and not two because every one of them grows by doubling: one doubling
	/// is the slack a steady workload leaves behind, and a threshold there would demote the spare
	/// of every steady parse every eight parses. Two doublings cannot be reached by slack.
	/// </para>
	/// <para>
	/// The count is read from the field at every step and never inferred from the end state. A
	/// counter that never advances and one that advances and is reset are indistinguishable from
	/// the far side, and the neighbouring counter has been written wrong in both of those ways.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData(CarrierKind.Tape, "DirectValues")]
	[InlineData(CarrierKind.Immediate, "ImmediateValues")]
	[InlineData(CarrierKind.Tape, "Ways")]
	[InlineData(CarrierKind.Tape, "Tokens_DotGram")]
	[InlineData(CarrierKind.Tape, "Parser")]
	public void The_ordinary_spare_is_let_go_after_eight_parses_that_left_its_room_unused(CarrierKind carrier, string name)
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

		// The slot, and from its name the count and where the store goes afterwards: each pool
		// spells all three differently and two of them keep them on the host class, which is
		// exactly why they are found by what they hold rather than by a name written here.
		var slot  = new[] { type, owner }
			.SelectMany(where => where.GetFields(Flags))
			.Single(field => field.IsStatic && field.FieldType == type
				&& field.Name.StartsWith("_spare", StringComparison.Ordinal));
		var held  = slot.DeclaringType!;
		var idle  = held.GetField(slot.Name + "Idle", Flags)!;
		var letGo = held.GetField(slot.Name + "LetGo", Flags)!;
		var rent  = type.GetMethod("Rent", Flags)
			?? owner.GetMethod("Rented_DotGram", Flags) ?? owner.GetMethod("Recycled", Flags);
		var back  = type.GetMethod("Return", Flags)
			?? owner.GetMethod("Recycle_DotGram", Flags) ?? owner.GetMethod("Recycle", Flags);

		if (engine)
		{
			rent = owner.GetMethod("Recycled", Flags);
			back = owner.GetMethod("Recycle", Flags);
		}

		var store  = rent!.Invoke(null, null)!;
		var arrays = type.GetFields(Flags).Where(field => !field.IsStatic && field.FieldType.IsArray).ToArray();
		var counts = type.GetFields(Flags).Where(field => !field.IsStatic && field.FieldType == typeof(int)).ToArray();

		// Room enough that a parse can leave most of it unused, and small enough in capacity
		// that every return below takes the ordinary branch and never the parked one. The arena
		// is one level down: a Parser holds an arena, and the arena holds the room.
		var arena = type.GetFields(Flags).FirstOrDefault(field => field.Name == "Entries")?.GetValue(store);
		var room  = arena?.GetType().GetFields(Flags).Single(field => field.FieldType.IsArray);
		var count = arena?.GetType().GetFields(Flags).Single(field => field.FieldType == typeof(int));

		if (arena is not null)
			room!.SetValue(arena, Array.CreateInstance(room.FieldType.GetElementType()!, 64));
		else
		{
			// Every array the pool's own test reads is given the same room, and the rest none,
			// so that "room" in this test is the number that test sums and nothing else.
			var counted = name switch
			{
				"DirectValues"     => new[] { "Live" },
				"ImmediateValues"  => arrays.Where(field => field.Name.StartsWith("Stack", StringComparison.Ordinal))
					.Take(1).Select(field => field.Name).ToArray(),
				"Ways"             => ["Items", "Log", "Refs"],
				_                  => ["Kinds", "Starts", "Lengths"],
			};

			foreach (var field in arrays)
				field.SetValue(store, Array.CreateInstance(field.FieldType.GetElementType()!,
					counted.Contains(field.Name) ? 64 : 0));
		}

		void Used(int howMuch)
		{
			if (arena is not null)
				count!.SetValue(arena, howMuch);
			else
				foreach (var field in counts)
					field.SetValue(store, howMuch);
		}

		// Sixteen of sixty-four is inside the margin: the room is wanted, and the count is nil.
		Used(16);
		back!.Invoke(null, [store]);
		Assert.Same(store, slot.GetValue(null));
		Assert.Equal(0, idle.GetValue(null));

		// Eight of sixty-four is two doublings out. Seven such parses and the store is held
		// still, with the count read from the field after each one.
		for (var parse = 1; parse <= 7; parse++)
		{
			Assert.Same(store, rent.Invoke(null, null));

			Used(8);
			back.Invoke(null, [store]);

			Assert.Same(store, slot.GetValue(null));
			Assert.Equal(parse, idle.GetValue(null));
		}

		// One parse that wants the room puts the count back to nothing.
		Assert.Same(store, rent.Invoke(null, null));
		Used(16);
		back.Invoke(null, [store]);
		Assert.Same(store, slot.GetValue(null));
		Assert.Equal(0, idle.GetValue(null));

		// And eight in a row that do not: the slot is empty and the store is reachable only
		// weakly, which is the whole of the policy — it stops competing for the memory, and it
		// is still there for a thread that wants it straight back.
		for (var parse = 1; parse <= 8; parse++)
		{
			Assert.Same(store, rent.Invoke(null, null));

			Used(8);
			back.Invoke(null, [store]);
		}

		Assert.Null(slot.GetValue(null));

		var weak = letGo.GetValue(null);

		Assert.NotNull(weak);

		object?[] arguments = [null];

		Assert.True((bool)weak!.GetType().GetMethod("TryGetTarget")!.Invoke(weak, arguments)!);
		Assert.Same(store, arguments[0]);
	}
}
