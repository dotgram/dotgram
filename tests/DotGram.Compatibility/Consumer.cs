using System;

using DotGram;

namespace DotGram.Compatibility
{
	// One grammar reaching for as much of the language as fits in a few lines, so that the
	// generated file exercises the shapes the emitter has to be careful about: a declared
	// type built from captures, a construction expression, a guard, a repetition collecting
	// into a sequence, an extent, and a publication of each kind.
	//
	// Nothing here is called. If it compiles, the claim this project makes is true.
	//
	// Written in C# 8 itself — a block namespace, concatenated strings, an ordinary
	// constructor — because this project's netstandard2.0 build compiles at the floor the
	// emitted code declares, and a consumer file needing more than the floor fails before
	// the generated one can be judged. That is exactly what happened the first time this
	// was measured: the grammar below used to be a raw string literal, the file failed to
	// parse, the attribute went unrecognized, and the generator produced nothing to check.
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
		"\n" +
		"parse Doc\n" +
		"find Key as AllKeys")]
	public partial class Settings
	{
	}

	/// <summary>What the grammar above builds, filled from captures by name (§7.3).</summary>
	public sealed class Entry
	{
		public Entry(string key, string value)
		{
			Key   = key;
			Value = value;
		}

		public string Key { get; }

		public string Value { get; }
	}
}
