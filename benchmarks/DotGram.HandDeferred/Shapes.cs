using System;

namespace DotGram.HandDeferred;

/// <summary>What a shape can do once the reading has been accepted.</summary>
/// <remarks>
/// Only the two leaves implement it, and only so that <see cref="Boxed"/> can hold them
/// where it holds everything — behind a reference. The readings that keep their types
/// call <c>Build</c> directly on the field and never go through this.
/// </remarks>
interface IBuilds
{
	string Build(string text);
}

/// <summary><c>Name : @string = t: ['a'..'z']+ =&gt; @(t)</c>, as eight bytes.</summary>
/// <remarks>
/// Shared by every reading that has a shape at all, so that what differs between them is
/// how a parent holds its children and nothing else.
/// </remarks>
readonly struct Name : IBuilds
{
	readonly int _at;
	readonly int _end;

	public Name(int at, int end)
	{
		_at  = at;
		_end = end;
	}

	public string Build(string text) => Author.Name(text.Substring(_at, _end - _at));
}

/// <summary><c>Digits : @string = t: ['0'..'9']+ =&gt; @(t)</c>, likewise.</summary>
readonly struct Digits : IBuilds
{
	readonly int _at;
	readonly int _end;

	public Digits(int at, int end)
	{
		_at  = at;
		_end = end;
	}

	public string Build(string text) => Author.Digits(text.Substring(_at, _end - _at));
}
