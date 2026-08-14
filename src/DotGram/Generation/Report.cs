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
	EquatableArray<string> Arguments,
	string?          Written   = null,
	int              WrittenAt = 0,
	string?          Grammar   = null)
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
	/// <param name="written">
	/// The attribute's string as the author spelled it, when the grammar came from one.
	/// </param>
	/// <param name="writtenAt">Where that spelling begins in the C# file.</param>
	public static Report Of(
		GramDiagnostic diagnostic, string? filePath, string grammarText, Location? fallback,
		string? written = null, int writtenAt = 0)
	{
		var span = new TextSpan(diagnostic.Position, diagnostic.Length);

		return new Report(
			diagnostic.Id,
			diagnostic.Id,
			"{0}",
			diagnostic.Severity switch
			{
				GramSeverity.Error => DiagnosticSeverity.Error,
				GramSeverity.Info  => DiagnosticSeverity.Info,
				_                  => DiagnosticSeverity.Warning,
			},
			FilePath:  filePath,
			Position:  diagnostic.Position,
			Length:    diagnostic.Length,
			Lines:     filePath is null ? default : Diagnostics.LinesOf(grammarText, span),
			Fallback:  fallback,
			Arguments: new EquatableArray<string>([diagnostic.Message]),
			Written:   written,
			WrittenAt: writtenAt,
			Grammar:   filePath is null ? grammarText : null);
	}

	/// <summary>
	/// Where in the attribute's own string the author wrote this, or null.
	/// </summary>
	/// <remarks>
	/// <para>
	/// By looking for the text rather than by decoding the literal. A C# string knows how
	/// to turn its spelling into a value and not the other way round, and reversing it
	/// means re-implementing escapes, verbatim doubling and raw-string indent stripping —
	/// three sets of rules, each with corners, all to place a squiggle.
	/// </para>
	/// <para>
	/// So: take the line of the grammar the diagnostic is on and find it in the spelling.
	/// Found once, the offset is known exactly. Found twice or not at all — a line
	/// repeated, or one whose escapes were written differently from what they decode to —
	/// the answer is no answer, and the message lands on the attribute as it did before.
	/// Never wrong, sometimes silent.
	/// </para>
	/// </remarks>
	Location? Inline()
	{
		if (Written is not { } spelling || Grammar is not { } grammar || Fallback?.SourceTree is not { } tree)
			return null;

		var from = grammar.LastIndexOf('\n', Math.Min(Position, grammar.Length - 1)) + 1;
		var to   = grammar.IndexOf('\n', from);
		var line = grammar.Substring(from, (to < 0 ? grammar.Length : to) - from).TrimEnd('\r');

		if (line.Length == 0)
			return null;

		var at = spelling.IndexOf(line, StringComparison.Ordinal);

		if (at < 0 || spelling.IndexOf(line, at + 1, StringComparison.Ordinal) >= 0)
			return null;

		var start = WrittenAt + at + (Position - from);

		return Location.Create(tree, new TextSpan(start, Math.Max(Length, 1)));
	}

	public Diagnostic ToRoslyn()
	{
		// A grammar in a file points into that file. One written into the attribute points
		// as far into the attribute's own string as it can be placed — and at the whole
		// attribute when it cannot, which is still the right place to look.
		var location = FilePath is null
			? Inline() ?? Fallback ?? Location.None
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
