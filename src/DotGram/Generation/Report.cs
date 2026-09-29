using System;
using System.Collections.Immutable;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Generation;

/// <summary>Where something is, in a form that compares the way arithmetic does.</summary>
/// <remarks>
/// A <see cref="Location"/> compares by tree and span, and a tree is a new object after every
/// edit — so a pipeline value holding one is unequal to itself across two compilations of the
/// same text, and nothing downstream of it can be reused. This says the same thing in a path, a
/// span and a line span, all of which compare by value. The <c>Location</c> is built from it at
/// delivery, where the tree is to hand and nothing is cached.
/// <para>
/// The lines are carried as well as the span because a <c>.gram</c> file has no
/// <c>SyntaxTree</c> in the compilation to look up, and an external location needs them.
/// </para>
/// </remarks>
readonly record struct Place(string? Path, TextSpan Span, LinePositionSpan Lines)
{
	public static Place? Of(Location? location)
	{
		if (location is null)
			return null;

		var span = location.GetLineSpan();

		return new Place(location.SourceTree?.FilePath ?? span.Path, location.SourceSpan, span.Span);
	}

	/// <summary>The location this stands for, given the trees of the compilation delivering it.</summary>
	public Location ToLocation(Func<string, SyntaxTree?> treeOf)
	{
		return Path is null
			? Location.None
			: treeOf(Path) is { } tree
				? Location.Create(tree, Span)
				: Location.Create(Path, Span, Lines);
	}
}

/// <summary>
/// Where one host's grammar is written: the attribute, the literal in it, and the attribute of
/// every grammar it includes.
/// </summary>
/// <remarks>
/// <para>
/// Everything about a host that changes when an edit moves it and nothing else, kept apart from
/// the <c>Host</c> the compile reads so that such an edit leaves the compile to be reused (D145).
/// Only the output step and the reporting read it: the one to write the literal's line and
/// column into each <c>#line</c>, the other to put a report on an attribute.
/// </para>
/// <para>
/// A report names a place here by an anchor rather than by carrying one: <see cref="None"/>,
/// <see cref="Attribute"/>, or <see cref="Include"/> of an include's index.
/// </para>
/// </remarks>
/// <param name="Key">Which host this is, the same as its <c>Host.Key</c>.</param>
/// <param name="At">The attribute.</param>
/// <param name="Path">The C# file the literal is in, or null for none.</param>
/// <param name="LiteralAt">Where the literal's spelling begins in that file, or 0 for none.</param>
/// <param name="Line">The literal's first line, 1-based.</param>
/// <param name="Column">The literal's first column, 1-based.</param>
/// <param name="Bases">The attribute of each grammar the host includes, in the host's order.</param>
readonly record struct Site(
	string                   Key,
	Place?                   At,
	string?                  Path,
	int                      LiteralAt,
	int                      Line,
	int                      Column,
	EquatableArray<Site.Base> Bases)
{
	public const int None      = -1;
	public const int Attribute = 0;

	/// <summary>An included grammar's attribute, and where its literal begins.</summary>
	/// <param name="At">Null where the base is in a referenced assembly.</param>
	public readonly record struct Base(Place? At, int LiteralAt);

	/// <summary>The anchor of the include at <paramref name="index"/>.</summary>
	public static int Include(int index)
	{
		return index + 1;
	}

	public Place? PlaceOf(int anchor)
	{
		return anchor == Attribute
			? At
			: anchor > Attribute && anchor <= Bases.Items.Length ? Bases.Items[anchor - 1].At : null;
	}

	public int LiteralAtOf(int anchor)
	{
		return anchor == Attribute
			? LiteralAt
			: anchor > Attribute && anchor <= Bases.Items.Length ? Bases.Items[anchor - 1].LiteralAt : 0;
	}
}

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
/// So the pieces travel and the <c>Diagnostic</c> is built at delivery. Where a report has no
/// place in a grammar file it names an attribute instead — the host's, or that of a grammar
/// the host includes — by its <see cref="Anchor"/> in the host's <see cref="Site"/>, and not by
/// where that attribute is: a report is made by the compile, and the compile is reused across
/// edits that move the attribute (D145).
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
	int              Anchor,
	EquatableArray<string> Arguments,
	string?          Written   = null,
	string?          Grammar   = null)
{
	/// <summary>A diagnostic the shell raises about the host, from a fixed descriptor.</summary>
	/// <param name="at">Which attribute it is about, as a <see cref="Site"/> anchor.</param>
	public static Report Of(DiagnosticDescriptor descriptor, int at, params string[] arguments)
	{
		return new(
			descriptor.Id,
			descriptor.Title.ToString(),
			descriptor.MessageFormat.ToString(),
			descriptor.DefaultSeverity,
			FilePath: null,
			Position: 0,
			Length: 0,
			Lines: default,
			Anchor: at,
			Arguments: new EquatableArray<string>([.. arguments]));
	}

	/// <summary>A diagnostic the grammar half raised, placed in the grammar it came from.</summary>
	/// <param name="anchor">
	/// The attribute it falls back to, as a <see cref="Site"/> anchor, and whose literal
	/// <paramref name="written"/> is.
	/// </param>
	/// <param name="written">
	/// The attribute's string as the author spelled it, when the grammar came from one.
	/// </param>
	public static Report Of(
		GramDiagnostic diagnostic, string? filePath, string grammarText, int anchor,
		string? written = null)
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
			Anchor:    anchor,
			Arguments: new EquatableArray<string>([diagnostic.Message]),
			Written:   written,
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
	Location? Inline(Func<string, SyntaxTree?> treeOf, Site site)
	{
		if (Written is not { } spelling || Grammar is not { } grammar ||
			site.PlaceOf(Anchor)?.Path is not { } path || treeOf(path) is not { } tree)
			return null;

		var from = grammar.LastIndexOf('\n', Math.Min(Position, grammar.Length - 1)) + 1;
		var to   = grammar.IndexOf('\n', from);
		var line = grammar.Substring(from, (to < 0 ? grammar.Length : to) - from).TrimEnd('\r');

		if (line.Length == 0)
			return null;

		var at = spelling.IndexOf(line, StringComparison.Ordinal);

		if (at < 0 || spelling.IndexOf(line, at + 1, StringComparison.Ordinal) >= 0)
			return null;

		var start = site.LiteralAtOf(Anchor) + at + (Position - from);

		return Location.Create(tree, new TextSpan(start, Math.Max(Length, 1)));
	}

	/// <param name="site">Where the host that made this report is written.</param>
	public Diagnostic ToRoslyn(Func<string, SyntaxTree?> treeOf, Site site)
	{
		// A grammar in a file points into that file. One written into the attribute points
		// as far into the attribute's own string as it can be placed — and at the whole
		// attribute when it cannot, which is still the right place to look.
		var location = FilePath is null
			? Inline(treeOf, site) ?? site.PlaceOf(Anchor)?.ToLocation(treeOf) ?? Location.None
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

	public override bool Equals(object? obj)
	{
		return obj is EquatableArray<T> other && Equals(other);
	}

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
