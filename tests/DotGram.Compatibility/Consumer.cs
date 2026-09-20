using System;

using DotGram;

namespace DotGram.Compatibility
{
	/// <summary>A publication yielded from a reader, and the same over bytes.</summary>
	[Gram("Item : @int = 'a' & ';' => @(1)\nFeed : @int[] = { Item* }\n" +
		"parse Feed as Array stream\nparse Feed as Rows stream yield : @int\nparse Feed as Bytes stream bytes yield : @int")]
	public partial class YieldInput
	{
	}

	/// <summary>Captures taken as spans, over a buffer and over bytes.</summary>
	[Gram("Start : @int = text: ['0'..'9']+ => @(ToInt(text))\nparse Start\nparse Start as Boxed : @object", BufferedInput = true, BufferedBytes = true, SpanCaptures = true)]
	public partial class NativeCapture
	{
		static int ToInt(System.ReadOnlySpan<char> text)
		{
			var result = 0;
			for (var i = 0; i < text.Length; i++) result = checked(result * 10 + text[i] - '0');
			return result;
		}

		static int ToInt(System.ReadOnlySpan<byte> text)
		{
			var result = 0;
			for (var i = 0; i < text.Length; i++) result = checked(result * 10 + text[i] - (byte)'0');
			return result;
		}
	}

	/// <summary>A parse of buffered input, over characters and over bytes.</summary>
	[Gram("Start : @int = \"ab\"* & '!' => @(42)\nparse Start", BufferedInput = true, BufferedBytes = true)]
	public partial class BufferedInput
	{
	}

	/// <summary>A choice the host computes, read over a byte stream.</summary>
	[Gram("Start : @int = switch @(1) { case 1: 'a' => @(1) default: 'b' => @(2) }\nparse Start stream bytes")]
	public partial class ComputedDispatch
	{
	}

	/// <summary>A yielded element that recovers, so a broken one is handed over in its place.</summary>
	[Gram("Item : @int = { ?=any } & 'a' & ';' => @(1)\nItems : @int[] = Item* recover ';' => @(0)\nparse Items stream bytes yield : @int")]
	public partial class RecoveringYield
	{
	}

	/// <summary>A capture of everything, read through a buffer.</summary>
	[Gram("Start : @string = text: any* => @(text)\nparse Start stream")]
	public partial class BufferedCapture
	{
	}

	// A choice whose alternatives begin alike over more characters than a switch names, so
	// that the reader finds its groups by a table: a span over a byte literal, which the floor
	// has to lower to the assembly's data with the span System.Memory gives it.
	/// <summary>A choice too wide for a switch, found by a table of its groups.</summary>
	[Gram(
		"Start : @int = Wide & '!' & eof => @(1) | Latin & '?' & eof => @(2) | '#' & Wide & eof => @(3)\n" +
		"Wide = ['a'..'z' | '\\u00C0'..'\\u024F']+\n" +
		"Latin = ['\\u0100'..'\\u017F']+\n" +
		"parse Start")]
	public partial class WideChoice
	{
	}

	// One grammar reaching for as much of the language as fits in a few lines, so that the
	// generated file exercises the shapes the emitter has to be careful about: a declared
	// type built from captures, a construction expression, a guard, a repetition collecting
	// into a sequence, an extent, and a publication of each kind.
	//
	// Nothing here is called from C#. If it compiles, the claim this project makes is
	// true — and every rule is published, because a rule nothing reaches is a rule the
	// generator never emits a recognizer for, which exercises nothing (GRAM4018 said so).
	//
	// Written in C# 8 itself — a block namespace, concatenated strings, an ordinary
	// constructor — because this project's netstandard2.0 build compiles at the floor the
	// emitted code declares, and a consumer file needing more than the floor fails before
	// the generated one can be judged. That is exactly what happened the first time this
	// was measured: the grammar below used to be a raw string literal, the file failed to
	// parse, the attribute went unrecognized, and the generator produced nothing to check.
	/// <summary>A document of several publications: parses, a find, and a rule that recurses.</summary>
	[Gram(
		"@using System;\n" +
		"@using DotGram.Compatibility;\n" +
		"\n" +
		"Doc    : @Entry[] = (Entry & eol?)*\n" +
		"\n" +
		"Entry  : @Entry   = key: Key & '=' & value: Rest & when @(key.Length > 0)\n" +
		"\n" +
		"Key    : @string  = text: ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']+ => @(text)\n" +
		"Rest   : @string  = text: [^ '\\n' | '\\r']* => @(text)\n" +
		"Where  : @SourceSpan = ['a'..'z']+\n" +
		// Recursion, so that the generated file carries the stack probe. It asks the
		// runtime whether there is stack left for another level, and the method it used
		// to ask with is .NET Core's: a grammar that never recurses never finds that out.
		"Nest   : @string  = '(' & inner: Nest & ')' => @(\"(\" + inner + \")\")\n" +
		"                  | t: Key => @(t)\n" +
		"\n" +
		"parse Doc as Objects : @object[] stream\n" +
		"find Entry as Entries : @object stream\n" +
		"parse Doc\n" +
		"parse Nest as Nested\n" +
		"parse Where as Span\n" +
		"find Key as AllKeys")]
	public partial class Settings
	{
	}

	// A guard that names a value built while the text is read, under a mark: the walk that
	// builds for such a guard is written with a method of its own asking whether what it may
	// build is handed the marks standing over it, and that method is a local function. What a
	// generator writes above a local function may not be a documentation comment — a consumer
	// who generates the documentation of their own assembly compiles our file with that switch
	// on, and the compiler refuses it (CS1587). This project builds with it on, so it refuses
	// here instead.
	/// <summary>A guard naming a value built under a mark, carried on the tape.</summary>
	[Gram(
		"state : @int\n" +
		"Digits : @int  = d: ['0'..'9']+ => @(ToInt(d))\n" +
		"Start  : @int  = v: Digits with state @(1) & when @(v > 0) & '!' => @(Under(v, parserState))\n" +
		"parse Start", Carrier = GramCarrier.Tape)]
	public partial class MarkedGuard
	{
		static int ToInt(string text)
		{
			var result = 0;
			for (var i = 0; i < text.Length; i++) result = checked(result * 10 + text[i] - '0');
			return result;
		}

		// Naming the marks is what makes the walk ask whether a record it may build is handed
		// them, and that question is a method of its own — the local function whose comment
		// this project is here to compile.
		static int Under(int value, System.ReadOnlySpan<int> marks)
		{
			return marks.Length > 0 ? value + marks[marks.Length - 1] : value;
		}
	}

	/// <summary>What the grammar above builds, filled from captures by name (§7.3).</summary>
	public sealed class Entry
	{
		/// <summary>An entry of the key and the value the grammar read.</summary>
		public Entry(string key, string value)
		{
			Key   = key;
			Value = value;
		}

		/// <summary>What the entry is called.</summary>
		public string Key { get; }

		/// <summary>What it is set to.</summary>
		public string Value { get; }
	}
}
