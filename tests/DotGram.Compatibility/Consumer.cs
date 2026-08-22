using System;

using DotGram;

namespace DotGram.Compatibility;

// One grammar reaching for as much of the language as fits in a few lines, so that the
// generated file exercises the shapes the emitter has to be careful about: a declared type
// built from captures, a construction expression, a guard, a repetition collecting into a
// sequence, an extent, and a publication of each kind.
//
// Nothing here is called. If it compiles, the claim this project makes is true.

[Gram("""
	@using System;
	@using DotGram.Compatibility;

	Doc    : @Entry[] = (Entry & eol?)*

	Entry  : @Entry   = key: Key & '=' & value: Rest & when @(key.Length > 0)

	Key    : @string  = text: ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']+ => @(text)
	Rest   : @string  = text: [^ '\n' | '\r']* => @(text)
	Where  : @SourceSpan = ['a'..'z']+

	parse Doc
	find Key as AllKeys
	""")]
public partial class Settings
{
}

/// <summary>What the grammar above builds, filled from captures by name (§7.3).</summary>
public sealed class Entry(string key, string value)
{
	public string Key { get; } = key;

	public string Value { get; } = value;
}
