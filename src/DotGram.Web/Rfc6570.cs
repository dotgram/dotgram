using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using DotGram;

namespace DotGram.Web;

/// <summary>A URI Template as RFC 6570 divides it: literal text, and expressions between braces.</summary>
/// <remarks>
/// What a template <i>is</i> comes from <see cref="Rfc6570.ParseTemplate(string)"/>; what it
/// <i>becomes</i> for a set of values is <see cref="Expand"/>. The two are apart because a
/// template is written once and expanded many times.
/// </remarks>
public sealed record UriTemplate(IReadOnlyList<UriTemplate.Part> Parts)
{
	/// <summary>A piece of a template: a <see cref="Literal"/> or an <see cref="Expression"/>.</summary>
	public abstract record Part;

	/// <summary>§2.1: text copied to the URI, pct-encoded where a URI may not hold it.</summary>
	public sealed record Literal(string Text) : Part;

	/// <summary>§2.2: an operator, or none, and the variables it expands.</summary>
	/// <param name="Operator">One of <c>+ # . / ; ? &amp;</c>, or null for simple string expansion.</param>
	public sealed record Expression(char? Operator, IReadOnlyList<Variable> Variables) : Part
	{
		public bool Equals(Expression? other) =>
			other is not null && Operator == other.Operator && Structural.Same(Variables, other.Variables);

		public override int GetHashCode() => Structural.Combine(Operator.GetHashCode(), Structural.Hash(Variables));
	}

	/// <summary>§2.3, §2.4: a variable's name as written, and the one modifier it may have.</summary>
	/// <param name="Name">Pct-encoded triplets included and not decoded: they are part of the name.</param>
	/// <param name="Prefix">How many characters of the value to use, from 1 to 9999, or null.</param>
	/// <param name="Explode">Whether a composite value is expanded member by member.</param>
	public sealed record Variable(string Name, int? Prefix, bool Explode);

	/// <summary>Equal to another template made of equal parts in the same order.</summary>
	public bool Equals(UriTemplate? other) => other is not null && Structural.Same(Parts, other.Parts);

	public override int GetHashCode() => Structural.Hash(Parts);

	/// <summary>The URI reference this template stands for, given the values of its variables.</summary>
	/// <remarks>
	/// <para>
	/// A value is a string; a list, any <c>IEnumerable&lt;string&gt;</c>; or an associative
	/// array, any <c>IEnumerable&lt;KeyValuePair&lt;string, string&gt;&gt;</c> — a dictionary
	/// among them — expanded in the order it enumerates. A number or anything else formattable
	/// is its invariant text. A variable that is missing or null is undefined (§2.3), and so is
	/// a list with no members and an array with no defined value.
	/// </para>
	/// <para>
	/// The expansion follows Appendix A, operator by operator. Its one error is a prefix on a
	/// composite value (§2.4.1), which throws: nothing in a template says what the prefix of a
	/// list is.
	/// </para>
	/// </remarks>
	/// <exception cref="ArgumentException">A prefix modifier names a list or an associative array.</exception>
	public string Expand(IReadOnlyDictionary<string, object?> variables)
	{
		if (variables is null)
			throw new ArgumentNullException(nameof(variables));

		var result = new StringBuilder();

		foreach (var part in Parts)
		{
			if (part is Literal literal)
				Encode(result, literal.Text, reserved: true);
			else
				Expand(result, (Expression)part, variables);
		}

		return result.ToString();
	}

	static void Expand(StringBuilder result, Expression expression, IReadOnlyDictionary<string, object?> variables)
	{
		// Appendix A's table, a column per operator.
		var (first, separator, named, ifEmpty, reserved) = expression.Operator switch
		{
			'+' => ("",  ",", false, "",  true),
			'#' => ("#", ",", false, "",  true),
			'.' => (".", ".", false, "",  false),
			'/' => ("/", "/", false, "",  false),
			';' => (";", ";", true,  "",  false),
			'?' => ("?", "&", true,  "=", false),
			'&' => ("&", "&", true,  "=", false),
			_   => ("",  ",", false, "",  false),
		};

		var defined = false;

		foreach (var variable in expression.Variables)
		{
			variables.TryGetValue(variable.Name, out var value);

			if (!Defined(value, out var text, out var list, out var pairs))
				continue;

			result.Append(defined ? separator : first);
			defined = true;

			if (text is not null)
			{
				if (named)
				{
					Encode(result, variable.Name, reserved: true);

					if (text.Length == 0)
					{
						result.Append(ifEmpty);
						continue;
					}

					result.Append('=');
				}

				Encode(result, variable.Prefix is { } length ? Prefix(text, length, reserved) : text, reserved);
				continue;
			}

			if (variable.Prefix is not null)
				throw new ArgumentException(
					$"'{variable.Name}' is a composite value, and RFC 6570 §2.4.1 gives a prefix of one no meaning.",
					nameof(variables));

			if (!variable.Explode)
			{
				if (named)
				{
					Encode(result, variable.Name, reserved: true);
					result.Append('=');
				}

				var comma = false;

				if (list is not null)
				{
					foreach (var member in list)
					{
						if (comma)
							result.Append(',');

						Encode(result, member, reserved);
						comma = true;
					}
				}
				else
				{
					foreach (var pair in pairs!)
					{
						if (comma)
							result.Append(',');

						Encode(result, pair.Key, reserved);
						result.Append(',');
						Encode(result, pair.Value, reserved);
						comma = true;
					}
				}

				continue;
			}

			var apart = false;

			if (list is not null)
			{
				foreach (var member in list)
				{
					if (apart)
						result.Append(separator);

					apart = true;

					if (named)
					{
						Encode(result, variable.Name, reserved: true);

						if (member.Length == 0)
						{
							result.Append(ifEmpty);
							continue;
						}

						result.Append('=');
					}

					Encode(result, member, reserved);
				}
			}
			else
			{
				foreach (var pair in pairs!)
				{
					if (apart)
						result.Append(separator);

					apart = true;

					Encode(result, pair.Key, reserved: named || reserved);

					if (named && pair.Value.Length == 0)
					{
						result.Append(ifEmpty);
						continue;
					}

					result.Append('=');
					Encode(result, pair.Value, reserved);
				}
			}
		}
	}

	/// <summary>Whether a value is defined (§2.3), and which of the three it is.</summary>
	static bool Defined(
		object? value, out string? text, out List<string>? list, out List<KeyValuePair<string, string>>? pairs)
	{
		text  = null;
		list  = null;
		pairs = null;

		switch (value)
		{
			case null:
				return false;

			case string one:
				text = one;
				return true;

			case IEnumerable<KeyValuePair<string, string>> array:
				pairs = [];

				foreach (var pair in array)
					if (pair.Value is not null)
						pairs.Add(pair);

				return pairs.Count > 0;

			case IEnumerable<string> members:
				list = [];

				foreach (var member in members)
					if (member is not null)
						list.Add(member);

				return list.Count > 0;

			case IFormattable formattable:
				text = formattable.ToString(null, CultureInfo.InvariantCulture);
				return true;

			default:
				text = value.ToString() ?? "";
				return true;
		}
	}

	/// <summary>The first <paramref name="length"/> characters of a value, counted as code points.</summary>
	/// <remarks>
	/// Where a pct-encoded triplet passes through unencoded, it is one character of the URI and is
	/// counted as one, so a prefix never ends inside it (§2.4.1).
	/// </remarks>
	static string Prefix(string text, int length, bool reserved)
	{
		var at = 0;

		for (var taken = 0; taken < length && at < text.Length; taken++)
		{
			if (reserved && IsTriplet(text, at))
				at += 3;
			else if (char.IsHighSurrogate(text[at]) && at + 1 < text.Length && char.IsLowSurrogate(text[at + 1]))
				at += 2;
			else
				at++;
		}

		return at >= text.Length ? text : text.Substring(0, at);
	}

	/// <summary>A value as a URI may hold it: what the allowed set holds as it is, the rest as UTF-8 triplets.</summary>
	/// <param name="reserved">
	/// Appendix A's U+R: reserved characters and pct-encoded triplets pass through too. Otherwise
	/// only unreserved characters do, and a <c>%</c> is <c>%25</c>.
	/// </param>
	static void Encode(StringBuilder result, string value, bool reserved)
	{
		for (var at = 0; at < value.Length; at++)
		{
			var character = value[at];

			if (IsUnreserved(character) || reserved && IsReserved(character))
			{
				result.Append(character);
				continue;
			}

			if (reserved && IsTriplet(value, at))
			{
				result.Append(value, at, 3);
				at += 2;
				continue;
			}

			int scalar = character;

			if (char.IsHighSurrogate(character) && at + 1 < value.Length && char.IsLowSurrogate(value[at + 1]))
				scalar = char.ConvertToUtf32(character, value[++at]);
			else if (char.IsSurrogate(character))
				scalar = 0xFFFD;

			if (scalar < 0x80)
				Octet(result, scalar);
			else if (scalar < 0x800)
			{
				Octet(result, 0xC0 | scalar >> 6);
				Octet(result, 0x80 | scalar & 0x3F);
			}
			else if (scalar < 0x10000)
			{
				Octet(result, 0xE0 | scalar >> 12);
				Octet(result, 0x80 | scalar >> 6 & 0x3F);
				Octet(result, 0x80 | scalar & 0x3F);
			}
			else
			{
				Octet(result, 0xF0 | scalar >> 18);
				Octet(result, 0x80 | scalar >> 12 & 0x3F);
				Octet(result, 0x80 | scalar >> 6 & 0x3F);
				Octet(result, 0x80 | scalar & 0x3F);
			}
		}
	}

	static void Octet(StringBuilder result, int octet) =>
		result.Append('%').Append(Hex[octet >> 4]).Append(Hex[octet & 0xF]);

	const string Hex = "0123456789ABCDEF";

	// RFC 3986 §2.3 and §2.2.
	static bool IsUnreserved(char c) =>
		c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '.' or '_' or '~';

	static bool IsReserved(char c) =>
		c is ':' or '/' or '?' or '#' or '[' or ']' or '@' or
			'!' or '$' or '&' or '\'' or '(' or ')' or '*' or '+' or ',' or ';' or '=';

	static bool IsTriplet(string text, int at) =>
		text[at] == '%' && at + 2 < text.Length && Uri.IsHexDigit(text[at + 1]) && Uri.IsHexDigit(text[at + 2]);
}

// RFC 6570, URI Template. The grammar is §2's ABNF rule for rule, all four levels, which is what
// the RFC recommends a parser read so that a template of a level it does not support is told
// apart from a template that is not one — with its one verified erratum, 6937, which lets an
// apostrophe into a literal. What the ABNF does not say is here as well:
//
//   * A literal's `ucschar` and `iprivate` beyond the Basic Multilingual Plane are one surrogate
//     pair each. The pair is read as any high surrogate followed by any low one, which lets
//     through the two noncharacters at the end of each plane that the ABNF leaves out.
//   * An operator the RFC reserves for later — `= , ! @ |` — is not an operator here, and a
//     template using one is refused, as Appendix A says it is an error.
//
// A template is refused whole where a character does not fit. Appendix A describes a processor
// that copies what it could not read and carries on; that is a way to report an error, and this
// reports it by failing.

[Gram("""
	@using System;
	@using DotGram.Web;

	trivia = none

	Alpha      = ['a'..'z' | 'A'..'Z']
	Digit      = ['0'..'9']
	Hexdig     = ['0'..'9' | 'a'..'f' | 'A'..'F']
	PctEncoded = '%' & Hexdig & Hexdig

	// §2.
	Template : @UriTemplate = parts: Part* => @(new UriTemplate(parts))

	Part : @UriTemplate.Part = expression: Expression => @(expression) | literal: Literals => @(literal)

	// §2.1 as erratum 6937 corrects it. Any Unicode character but controls, space, `"`, `%`
	// outside a triplet, `<`, `>`, `\`, `^`, `` ` ``, `{`, `|` and `}` — the apostrophe is one,
	// which the RFC's own examples use and its ABNF had left out.
	Literals : @UriTemplate.Part = (LiteralChar | PctEncoded)+ => @(new UriTemplate.Literal(parserText))

	LiteralChar = ['!' | '#'..'$' | '&'..';' | '=' | '?'..'[' | ']' | '_' | 'a'..'z' | '~'
	              | '\u00A0'..'\uD7FF' | '\uE000'..'\uFDCF' | '\uFDF0'..'\uFFEF']
	            | ['\uD800'..'\uDBFF'] & ['\uDC00'..'\uDFFF']

	// §2.2.
	Expression : @UriTemplate.Part
		= '{' & op: Operator? & first: VarSpec & rest: NextVarSpec* & '}'
		=> @(Rfc6570.Expression(op, first, rest))

	Operator = ['+' | '#' | '.' | '/' | ';' | '?' | '&']

	NextVarSpec : @UriTemplate.Variable = ',' & variable: VarSpec => @(variable)

	// §2.3, §2.4. A prefix or an explode, never both.
	VarSpec : @UriTemplate.Variable
		= name: VarName & (':' & prefix: MaxLength | explode: '*')?
		=> @(new UriTemplate.Variable(name, prefix is null ? null : int.Parse(prefix), explode is not null))

	VarName = VarChar & ('.'? & VarChar)*
	VarChar = [Alpha | Digit | '_'] | PctEncoded

	// A positive integer below 10000, without leading zeros.
	MaxLength = ['1'..'9'] & Digit{0,3}

	parse Template as ParseTemplate
	""")]
public static partial class Rfc6570
{
	// ParseTemplate and TryParseTemplate are generated here.

	internal static UriTemplate.Part Expression(string? @operator, UriTemplate.Variable first, UriTemplate.Variable[] rest)
	{
		var variables = new UriTemplate.Variable[1 + rest.Length];

		variables[0] = first;
		rest.CopyTo(variables, 1);

		return new UriTemplate.Expression(@operator is null ? null : @operator[0], variables);
	}
}
