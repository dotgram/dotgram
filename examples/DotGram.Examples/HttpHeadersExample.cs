using System;
using System.Collections.Generic;
using System.Linq;

using DotGram;

namespace DotGram.Examples;

// HTTP header fields, read into a lookup:
//
//     var headers = HttpHeaders.Read(text);
//
//     headers["Content-Type"]      // "text/plain; charset=utf-8"
//     headers["content-type"]      // the same — field names are case-insensitive
//     headers.Names                // in the order they arrived
//
// The reason this format is here rather than another: a value may continue on the next
// line. RFC 822 calls it folding — a line beginning with a space or a tab is not a new
// field but the rest of the one above it:
//
//     Subject: a long subject
//      continued here
//
// That makes it the one format in this folder where a single value spans lines, and the
// grammar has to say where a field ends without an end marker: it ends when the next
// line does not begin with whitespace. `Name` excludes the space, so a continuation
// simply cannot be read as a field name, and the repetition stops on its own.
//
// Everything else follows the same principle as the INI reader: the parser hands back
// the dictionary, folded and joined, rather than a list of lines for the caller to
// assemble.

[Gram("""
	@using DotGram.Examples;

	Trivia = none

	Headers : @HttpHeaders = fields: Field* & eol? & eof => @(new HttpHeaders(fields))

	// A field is its first line plus every folded line under it. The `Name` rule cannot
	// match a leading space, which is the whole of how a fold is told from a new field.
	Field : @HttpField = name: Name & ':' & Space & first: Line & (eol | ?=eof)
	                   & folded: Fold*
	                     => @(new HttpField(name, first, folded))

	Fold : @string = [' ' | '\t'] & Space & text: Line & (eol | ?=eof) => @(text)

	Name : @string = text: [^ ':' | ' ' | '\t' | '\n' | '\r']+ => @(text)

	// To the end of the line and no further; trailing spaces are the value's own, which
	// RFC 7230 says to strip, and the C# side does it in one place.
	Line : @string = text: [^ '\n' | '\r']* => @(text)

	Space = [' ' | '\t']*

	parse Headers
	""")]
public sealed partial class HttpParser
{
	public static HttpHeaders Read(string text) => ParseHeaders(text);
}

/// <summary>One field, with the lines it was folded across.</summary>
public sealed record HttpField(string Name, string First, IReadOnlyList<string> Folded)
{
	/// <summary>
	/// The value as one string: folded lines joined with a single space, which is what
	/// RFC 7230 says a recipient may replace the fold with.
	/// </summary>
	public string Value =>
		Folded.Count == 0
			? First.Trim()
			: string.Join(" ", Folded.Select(line => line.Trim()).Prepend(First.Trim()));
}

/// <summary>The fields as the lookup a caller wants, names compared without case.</summary>
public sealed class HttpHeaders
{
	readonly Dictionary<string, string> _fields = new(StringComparer.OrdinalIgnoreCase);

	public HttpHeaders(IReadOnlyList<HttpField> fields)
	{
		Names = [.. fields.Select(field => field.Name)];

		// A repeated field is one value with the copies joined by commas, which is what
		// RFC 7230 says they mean — not the last one winning.
		foreach (var field in fields)
			_fields[field.Name] = _fields.TryGetValue(field.Name, out var already)
				? already + ", " + field.Value
				: field.Value;
	}

	public string? this[string name] => _fields.TryGetValue(name, out var value) ? value : null;

	/// <summary>Field names, in the order they arrived, repeats included.</summary>
	public IReadOnlyList<string> Names { get; }

	public bool Has(string name) => _fields.ContainsKey(name);
}
