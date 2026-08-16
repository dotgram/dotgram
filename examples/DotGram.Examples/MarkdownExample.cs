using System;
using System.Collections.Generic;
using System.Linq;

using DotGram;

namespace DotGram.Examples;

// Markdown, cut down to its block structure — headings, bullet lists, fenced code and
// paragraphs. Written because it is the first format here where the line is the unit:
// everything else in this folder is made of tokens with whitespace between them, and
// this one is made of lines with meaning in their first character.
//
//     MarkdownParser.Read("# Title\n\nsome text\n- one\n- two\n")
//
// Three things it shows that the other examples do not:
//
//   * `Trivia = none` and every newline written down. A block format cannot ignore
//     whitespace — the difference between one paragraph and two is a blank line.
//
//   * Ordered choice doing real work. `Block` tries a heading, then a list, then code,
//     and a paragraph last, because a paragraph is "a line that is none of those" and
//     the only thing that says so is the order (§3.2).
//
//   * A capture read for its length rather than its text: `hashes: '#'+` and then
//     `hashes.Length` is the heading level, which is what a `=>` is for.

[Gram("""
	@using DotGram.Examples;

	Trivia = none

	Doc : @MarkdownBlock[] = Block* & eof

	// Order is the definition here: a paragraph is a line that is none of the others.
	Block : @MarkdownBlock = block: Heading   => @(block)
	                       | block: Bullets   => @(block)
	                       | block: Code      => @(block)
	                       | Blank            => @(new MarkdownBlank())
	                       | block: Paragraph => @(block)

	Heading : @MarkdownBlock = hashes: '#'+ & ' ' & text: Line & eol
	                             => @(new MarkdownHeading(hashes.Length, text))

	// One `Bullet+` is one list. The next blank line or ordinary line ends it, because
	// neither starts with "- ".
	Bullets : @MarkdownBlock = items: Bullet+ => @(new MarkdownList(items))

	Bullet : @string = "- " & text: Line & eol => @(text)

	Code : @MarkdownBlock = "```" & Line & eol & lines: CodeLine* & "```" & eol
	                          => @(new MarkdownCode(lines))

	CodeLine : @string = ?!"```" & text: Line & eol => @(text)

	Blank = eol

	Paragraph : @MarkdownBlock = text: Line & eol => @(new MarkdownParagraph(text))

	Line : @string = chars: [^ '\n' | '\r']* => @(chars)

	parse Doc
	""")]
public sealed partial class MarkdownParser
{
	/// <summary>Reads a document into its blocks, blank lines and all.</summary>
	public static IReadOnlyList<MarkdownBlock> Read(string text) => ParseDoc(text);

	/// <summary>The same, with the blank lines dropped — usually what a caller wants.</summary>
	public static IEnumerable<MarkdownBlock> Blocks(string text) =>
		Read(text).Where(block => block is not MarkdownBlank);
}

/// <summary>One block of a document.</summary>
public abstract record MarkdownBlock;

public sealed record MarkdownBlank                                  : MarkdownBlock;
public sealed record MarkdownHeading  (int Level, string Text)      : MarkdownBlock;
public sealed record MarkdownParagraph(string Text)                 : MarkdownBlock;
public sealed record MarkdownCode     (IReadOnlyList<string> Lines) : MarkdownBlock;
public sealed record MarkdownList     (IReadOnlyList<string> Items) : MarkdownBlock;
