using System;
using System.Collections.Generic;
using System.Linq;

using DotGram;

namespace DotGram.Examples;

// XML, cut down to elements, attributes and text — and written because it needs the one
// thing no other grammar here does: a rule that has to compare two of its own captures.
//
//     XmlParser.Read("<a x='1'><b>text</b></a>")
//
// A closing tag has to name the element it closes, and no amount of grammar says that.
// What says it is a `when` (§3.6): the guard runs during the match, sees the captures
// taken before it, and saying no is an ordinary non-match — so `<a></b>` fails to be an
// element rather than parsing into one that is wrong.
//
//     Element = '<' & name: Name … "</" & close: Name & '>' & when @(name == close)
//
// That is the whole trick, and it is why the guard is recognition rather than
// validation: a language whose well-formedness rule lives outside the parser has two
// definitions of well-formed.
//
// Everything else is ordinary. Attributes are a repetition; content is a choice of
// elements and text; the tree is records, so a caller walks it with patterns.

[Gram("""
	@using DotGram.Examples;

	using Lexical;

	context Lexical
	{
		// No Trivia in here: a name stops at the first character that is not one of these,
		// and text is whatever sits between the tags, spaces included.
		Trivia = none

		NameText = ['a'..'z' | 'A'..'Z' | '_'] & ['a'..'z' | 'A'..'Z' | '0'..'9' | '_' | '-' | '.']*
		Space    = [' ' | '\t' | '\r' | '\n']*
		Quoted   = [^ '"' | '\'']*
		Chars    = [^ '<']+
	}

	// None, and that is the point. Trivia is inserted between the operands of a sequence
	// (§4.5), so a grammar that ignores whitespace ignores it inside content too — and in
	// XML the run between two tags is a text node, spaces and all. Where whitespace really
	// is insignificant here, `Lexical.Space` says so in as many words.
	Trivia = none

	Xml : @XmlElement = element: Element & eof => @(element)

	// The `when` is what makes a closing tag close *this* element. It sits after both
	// names are captured, because a guard sees what was taken before it and nothing else.
	Element : @XmlElement = '<' & name: Name & attributes: Attribute* & Lexical.Space & '>'
	                      & children: Content*
	                      & "</" & close: Name & Lexical.Space & '>' & when @(name == close)
	                        => @(new XmlElement(name, attributes, children))

	                      | '<' & name: Name & attributes: Attribute* & Lexical.Space & "/>"
	                        => @(new XmlElement(name, attributes, []))

	Content : @XmlNode = element: Element => @(element)
	                   | text: Text       => @(text)

	Text : @XmlNode = chars: Lexical.Chars => @(new XmlText(chars))

	// The leading `Space` is not decoration. §4.5 inserts Trivia between the operands of a
	// sequence, and the iterations of a repetition are not that — so `Attribute*` would
	// read one attribute and stop at the space before the next. Each iteration eats its
	// own leading whitespace instead.
	Attribute : @XmlAttribute = Lexical.Space & name: Name & '=' & value: Value
	                              => @(new XmlAttribute(name, value))

	Value : @string = '"' & text: Lexical.Quoted & '"'   => @(text)
	                | '\'' & text: Lexical.Quoted & '\'' => @(text)

	Name : @string = text: Lexical.NameText => @(text)

	parse Xml
	""")]
public sealed partial class XmlParser
{
	/// <summary>Reads one element and everything under it, or throws.</summary>
	public static XmlElement Read(string text) => ParseXml(text);
}

/// <summary>Either an element or a run of text.</summary>
public abstract record XmlNode;

public sealed record XmlText(string Text) : XmlNode;

public sealed record XmlElement(
	string Name,
	IReadOnlyList<XmlAttribute> Attributes,
	IReadOnlyList<XmlNode>      Children) : XmlNode
{
	/// <summary>The value of the attribute of this name, or null.</summary>
	public string? this[string name] =>
		Attributes.FirstOrDefault(attribute => attribute.Name == name)?.Value;

	/// <summary>The elements directly under this one, in order.</summary>
	public IEnumerable<XmlElement> Elements => Children.OfType<XmlElement>();
}

public sealed record XmlAttribute(string Name, string Value);
