using System;
using System.Collections.Generic;
using System.Linq;

using DotGram;

namespace DotGram.Examples;

// Nesting by indentation, to any depth:
//
//     server:
//       host: example.com
//       ports:
//         http: 80
//         https: 443
//
//     var root = YamlLite.Read(text);
//     root["server"]["ports"]["https"].Value      // "443"
//
// A grammar cannot express this, and does not try. Depth here means comparing the indent
// of this line with the indent of the last one, which is a value carried from one place
// in the input into the shape of another — the wall netstrings hit, and §7.1 does not get
// round it either, since an external recognizer has nowhere to keep a stack between
// calls.
//
// What a grammar can do is read every line exactly: its indent as text, its key, its
// value. That is a flat list, and a flat list of indents is all a tree needs — the
// building is a stack, twenty lines of C#, and it happens once here rather than in every
// caller.
//
// The split is the point. Recognition is what the grammar is for and is where the fiddly
// parts live: what counts as an indent character, where a comment starts, that a value
// runs to the end of the line. Structure is arithmetic over what was recognised, and
// arithmetic belongs in C#.
//
// Deliberately a subset — no flow style, no anchors, no multi-line scalars. Enough for
// the configuration files people actually write, and honest about the rest.

[Gram("""
	@using DotGram.Examples;

	Trivia = none

	// §4.1 case 3: the document is what `Lines` produced.
	Doc : Lines = Lines & eof

	Lines : @YamlLine[] = (Line | Blank)*

	// The indent is captured rather than counted, because counting is the tree's business.
	// Tabs are refused outright, as YAML refuses them — a file that mixes them is a file
	// whose shape depends on somebody's editor.
	Line : @YamlLine = indent: Indent & key: Key & ':' & Space & value: Rest & (eol | ?=eof)
	                     => @(new YamlLine(indent.Length, key, value.TrimEnd()))

	Blank = Space & Comment? & eol

	Comment = '#' & [^ '\n' | '\r']*

	Indent : @string = text: ' '* => @(text)

	Key    : @string = text: ['a'..'z' | 'A'..'Z' | '0'..'9' | '_' | '-' | '.']+ => @(text)

	Rest   : @string = text: [^ '\n' | '\r']* => @(text)

	Space  = ' '*

	parse Doc
	""")]
public sealed partial class YamlLite
{
	/// <summary>Reads a document into a tree, nested as deep as its indentation goes.</summary>
	public static YamlNode Read(string text) => YamlNode.Build(ParseDoc(text));
}

/// <summary>One line as the grammar saw it: how far in, what it names, what it holds.</summary>
public sealed record YamlLine(int Indent, string Key, string Value);

/// <summary>
/// A node of the tree the lines describe.
/// </summary>
public sealed class YamlNode
{
	readonly Dictionary<string, YamlNode> _children = new(StringComparer.Ordinal);

	YamlNode(string value) => Value = value;

	/// <summary>What was written after the colon, empty where the line only opens a block.</summary>
	public string Value { get; private set; }

	public IReadOnlyDictionary<string, YamlNode> Children => _children;

	/// <summary>The child of this name; an absent one is an empty node rather than null.</summary>
	public YamlNode this[string key] =>
		_children.TryGetValue(key, out var found) ? found : Empty;

	static readonly YamlNode Empty = new("");

	/// <summary>
	/// The tree the indents describe, built with a stack of open levels.
	/// </summary>
	/// <remarks>
	/// One line, one decision: deeper than the level on top of the stack opens a level,
	/// shallower closes as many as it takes, and equal replaces the last. Nothing here
	/// needs to know how many spaces a level is — only that one indent is larger than
	/// another, which is why the grammar hands the indent over as a width rather than as
	/// a level number.
	/// </remarks>
	public static YamlNode Build(IReadOnlyList<YamlLine> lines)
	{
		var root  = new YamlNode("");
		var stack = new List<(int Indent, YamlNode Node)> { (-1, root) };

		foreach (var line in lines)
		{
			while (stack.Count > 1 && line.Indent <= stack[^1].Indent)
				stack.RemoveAt(stack.Count - 1);

			var node = new YamlNode(line.Value);

			stack[^1].Node._children[line.Key] = node;
			stack.Add((line.Indent, node));
		}

		return root;
	}
}
