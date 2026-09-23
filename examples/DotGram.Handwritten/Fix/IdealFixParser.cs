using System;

using DotGram.Finance.Fix44;

namespace DotGram.Handwritten.Fix;

/// <summary>
/// The least a reader of plain FIX text fields can do, as a yardstick: what a generated parser
/// is to be measured against beside <see cref="HandFixParser"/>.
/// </summary>
/// <remarks>
/// <para>
/// One loop over the string: the tag's digits accumulated as they are read, one compare for
/// <c>=</c>, a vectorized search for the separator, the same factory and the same checks as
/// <see cref="HandFixParser"/>'s text path, and the fields gathered into an array that grows
/// by doubling and is cut to size once. No input object, no iterator, no list.
/// </para>
/// <para>
/// An ideal, not a reference: <see cref="HandFixParser"/> is the parser that reads the
/// grammar (D1), and this one does not. Anything but a plain text field — a length/data pair,
/// a data tag out of place, a malformed field — is handed to <see cref="HandFixParser"/> whole,
/// so every answer is the hand parser's answer and only the plain path is its own. Only the
/// string form exists, and it is only ever to be timed on input of plain text fields, where
/// the hand parser is never reached; a row that reaches it measures the hand parser.
/// </para>
/// </remarks>
public static class IdealFixParser
{
	static readonly FixField[] None = [];

	/// <summary>Reads wire fields separated by SOH.</summary>
	public static FixField[] Parse(string input, FixContext? options = null)
	{
		ArgumentNullException.ThrowIfNull(input);

		var known  = options ?? FixContext.Default;
		var text   = input.AsSpan();
		var fields = new FixField[8];
		var count  = 0;
		var p      = 0;

		while (p < text.Length)
		{
			var start = p;
			var c     = text[p];

			if (c < '1' || c > '9')
				return HandFixParser.Parse(input, options);

			var tag = 0;

			do
			{
				if (tag > (int.MaxValue - (c - '0')) / 10)
					return HandFixParser.Parse(input, options);

				tag = tag * 10 + c - '0';
				p++;
			}
			while (p < text.Length && (c = text[p]) is >= '0' and <= '9');

			if (p >= text.Length || text[p] != '=' || known.DataTag(tag) != 0 || known.IsData(tag))
				return HandFixParser.Parse(input, options);

			var value = ++p;
			var found = text.Slice(p).IndexOf('\u0001');
			var end   = found < 0 ? text.Length : p + found;

			if (end == value)
				return HandFixParser.Parse(input, options);

			p = found < 0 ? end : end + 1;

			var field = FixFieldFactory.Value(tag, text.Slice(value, end - value), known.CustomFields);

			field.WithTerminator(p - end).Locate(start, p - start);

			if (count == fields.Length)
				Array.Resize(ref fields, count * 2);

			fields[count++] = field;
		}

		if (count == 0)
			return None;

		if (count != fields.Length)
			Array.Resize(ref fields, count);

		return fields;
	}
}
