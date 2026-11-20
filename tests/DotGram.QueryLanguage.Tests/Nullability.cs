using System;
using System.Linq;

namespace DotGram.QueryLanguage.Tests;

// Types whose nullable annotations the builder reads: each shape the compiler writes them in.

/// <summary>Most members never null: the compiler marks the type, and the member that differs.</summary>
public sealed class Annotated
{
	public string  Name     { get; set; } = "";
	public string  Code     { get; set; } = "";
	public string? Comment  { get; set; }
	public string  Field    = "";
	public string? Optional;
	public byte[]  Bytes    { get; set; } = [];

	/// <summary>A nested type with no members of its own to mark: its context is the type around it.</summary>
	public sealed class Inner
	{
		public string Name { get; set; } = "";
	}
}

/// <summary>Most members may be null: the type's context says so, and the member that may not is marked.</summary>
public sealed class MostlyNullable
{
	public string? A    { get; set; }
	public string? B    { get; set; }
	public string? C    { get; set; }
	public string  Name { get; set; } = "";
}

#nullable disable
/// <summary>No annotations at all: a member may be null.</summary>
public sealed class Oblivious
{
	public string Name { get; set; }
	public string Field;
}
#nullable restore

public sealed class Annotations
{
	public IQueryable<Annotated>       Annotated { get; } = Array.Empty<Annotated>().AsQueryable();
	public IQueryable<Annotated.Inner> Inner     { get; } = Array.Empty<Annotated.Inner>().AsQueryable();
	public IQueryable<MostlyNullable>  Mostly    { get; } = Array.Empty<MostlyNullable>().AsQueryable();
	public IQueryable<Oblivious>       Oblivious { get; } = Array.Empty<Oblivious>().AsQueryable();
}
