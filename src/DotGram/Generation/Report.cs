using System;
using System.Collections.Immutable;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Generation;

/// <summary>
/// One diagnostic, as values.
/// </summary>
/// <remarks>
/// <para>
/// An incremental generator decides whether the next step must run by comparing what the
/// last one produced, so everything a step produces has to compare by value. A
/// <c>Diagnostic</c> does not, reliably: it holds a descriptor and a <c>Location</c>, and
/// a location holds the syntax tree it came from. Carrying one would make the output
/// unequal whenever any tree was reparsed, which is what the whole arrangement is trying
/// to avoid.
/// </para>
/// <para>
/// So the pieces travel and the <c>Diagnostic</c> is built at delivery. The one location
/// kept as a location is the fallback — the host class's own — because it points into a
/// tree that only changes when the file holding the grammar changes, which is a
/// regeneration anyway.
/// </para>
/// </remarks>
readonly record struct Report(
	string           Id,
	string           Title,
	string           MessageFormat,
	DiagnosticSeverity Severity,
	string?          FilePath,
	int              Position,
	int              Length,
	LinePositionSpan Lines,
	Location?        Fallback,
	EquatableArray<string> Arguments)
{
	/// <summary>A diagnostic the shell raises about the host, from a fixed descriptor.</summary>
	public static Report Of(DiagnosticDescriptor descriptor, Location? at, params string[] arguments) =>
		new(
			descriptor.Id,
			descriptor.Title.ToString(),
			descriptor.MessageFormat.ToString(),
			descriptor.DefaultSeverity,
			FilePath:  null,
			Position:  0,
			Length:    0,
			Lines:     default,
			Fallback:  at,
			Arguments: new EquatableArray<string>([.. arguments]));

	/// <summary>A diagnostic the grammar half raised, placed in the grammar it came from.</summary>
	public static Report Of(
		GramDiagnostic diagnostic, string? filePath, string grammarText, Location? fallback)
	{
		var span = new TextSpan(diagnostic.Position, diagnostic.Length);

		return new Report(
			diagnostic.Id,
			diagnostic.Id,
			"{0}",
			diagnostic.Severity == GramSeverity.Error
				? DiagnosticSeverity.Error
				: DiagnosticSeverity.Warning,
			FilePath:  filePath,
			Position:  diagnostic.Position,
			Length:    diagnostic.Length,
			Lines:     filePath is null ? default : Diagnostics.LinesOf(grammarText, span),
			Fallback:  fallback,
			Arguments: new EquatableArray<string>([diagnostic.Message]));
	}

	public Diagnostic ToRoslyn()
	{
		// An inline grammar has no file to point into, so the message lands on the
		// attribute that carries it — still the right place to look, if not the right
		// character.
		var location = FilePath is null
			? Fallback ?? Location.None
			: Location.Create(FilePath, new TextSpan(Position, Length), Lines);

		return Diagnostic.Create(
			Diagnostics.DescriptorFor(Id, Title, MessageFormat, Severity),
			location,
			[.. Arguments.Items]);
	}
}

/// <summary>
/// An <see cref="ImmutableArray{T}"/> that compares by its contents.
/// </summary>
/// <remarks>
/// <c>ImmutableArray&lt;T&gt;.Equals</c> compares the underlying array by reference, so a
/// step that hands one out is unequal to itself every run. It is the classic way to write
/// an incremental generator, watch it recompute everything, and find nothing wrong by
/// reading the code.
/// </remarks>
readonly struct EquatableArray<T>(ImmutableArray<T> items) : IEquatable<EquatableArray<T>>
	where T : IEquatable<T>
{
	readonly ImmutableArray<T> _items = items;

	/// <summary>
	/// The contents, empty when there are none.
	/// </summary>
	/// <remarks>
	/// Through a property because a <c>default</c> of this struct never ran the
	/// constructor, so the field is a default <c>ImmutableArray</c> — the one whose every
	/// member throws. A record with one of these among its fields is defaulted the moment
	/// any of its cases has nothing to say.
	/// </remarks>
	public ImmutableArray<T> Items => _items.IsDefault ? [] : _items;

	public bool Equals(EquatableArray<T> other)
	{
		if (Items.Length != other.Items.Length)
			return false;

		for (var i = 0; i < Items.Length; i++)
			if (!Items[i].Equals(other.Items[i]))
				return false;

		return true;
	}

	public override bool Equals(object? obj) => obj is EquatableArray<T> other && Equals(other);

	public override int GetHashCode()
	{
		var hash = Items.Length;

		foreach (var item in Items)
			hash = unchecked((hash * 397) ^ (item?.GetHashCode() ?? 0));

		return hash;
	}

	public static bool operator ==(EquatableArray<T> left, EquatableArray<T> right) =>  left.Equals(right);
	public static bool operator !=(EquatableArray<T> left, EquatableArray<T> right) => !left.Equals(right);
}
