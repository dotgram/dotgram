using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

using DotGram.Handwritten.Fix;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// A field of FIX serialized the way the stand's own FIX rows do: an invalid one by its position, length and raw text, any other by
	/// its type and every property. Works on a field of a side's own assembly, whose type is not this process's.
	/// </summary>
	static string DescribeFixField(object field)
	{
		var type = field.GetType();

		return type.Name == "Invalid"
			? $"Invalid:{type.GetProperty("Position")!.GetValue(field)}:{type.GetProperty("Length")!.GetValue(field)}:{type.GetProperty("RawText")!.GetValue(field)}"
			: type.Name + JsonSerializer.Serialize(field, type);
	}

	/// <summary>
	/// What both sides of a pair build from the FIX inputs, held to what this process's hand-written parser builds, before anything is
	/// timed: the paired rows sum the tags of the fields (the sides' types are not this process's), so a side that built less per field
	/// would be faster and pass them; this is the field-by-field comparison the unpaired rows make, over the inputs of the paired rows
	/// and the three forms that read them.
	/// </summary>
	static void PairedFixContent(string beforeDir, string afterDir)
	{
		var order = "8=FIX.4.4\u00019=65\u000135=D\u000111=ORDER\u000155=ABC\u000154=1\u000160=20260915-12:00:00\u000138=100\u000140=2\u000144=12.50\u000110=000\u0001";

		var inputs = new (string Name, string Text)[]
		{
			("One", "55=ABC\u0001"),
			("Order", order),
			("OrderMalformed", order.Replace("\u000140=2\u0001", "\u000140X=2\u0001")),
			("BinaryMany", string.Concat(Enumerable.Repeat("95=3\u000196=a\u0001b\u0001", 64))),
			("slope-16", FixSlopeText(16)),
		};

		foreach (var (name, directory) in new[] { ("before", beforeDir), ("after", afterDir) })
		{
			var side = new PairedSide(name, directory);

			foreach (var (input, text) in inputs)
			{
				var bytes = Encoding.Latin1.GetBytes(text);

				foreach (var form in new[] { "text", "bytes", "stream" })
				{
					// Each form against the hand-written parser's same form: an invalid field of the bytes forms carries no raw text on either side.
					var hand = (form switch
					{
						"bytes"  => HandFixParser.Parse(bytes),
						"stream" => HandFixParser.Parse(new System.IO.MemoryStream(bytes, false)),
						_        => HandFixParser.Parse(text),
					}).Select(DescribeFixField).ToArray();

					if (Differ(hand, side.FixFields(form, text)) is { } difference)
						throw new InvalidOperationException($"fix/{input}.{form}: the {name} side builds other fields than the hand-written parser, so no ratio would mean anything.\n{difference.Replace("generated", name.PadRight(9))}");
				}
			}
		}
	}
}
