using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;

using DotGram.Finance.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// Every tag, through both doors of the factory, against the class its tag names.
/// </summary>
/// <remarks>
/// <para>
/// The factory dispatches on a tag by dividing it into parts, because one switch over nine hundred
/// labels is the largest method a first call waits for. Which divisor it uses is a readability
/// decision and is expected to change; what must not change is the answer for any tag. Regrouping
/// nine hundred and twenty cases by hand or by script is exactly the edit where one case goes
/// missing and nothing fails: a tag whose class is lost becomes custom text, which parses, reads
/// and validates, and is simply the wrong field.
/// </para>
/// <para>
/// So this asks for every tag from one to eleven hundred — past the last the standard defines, and
/// past the last part's edge under any grouping — and holds the answer to what
/// the published repository names, where QuickFIX does not name it otherwise. It is the whole table, so nothing can hide in the part that
/// was not sampled.
/// </para>
/// <para>
/// <strong>What it does not cover: the third door.</strong> Beside the two <c>Value</c> overloads
/// there is <c>Binary(int, (bool, ReadOnlyMemory&lt;byte&gt;), Func&lt;int, FixCustomField?&gt;?)</c>, which builds
/// the sixteen data fields from a payload already read by length. It is one flat switch of sixteen
/// labels, in no parts and needing none, and nothing here asks it anything. Sixteen labels can be
/// counted by eye; nine hundred and twelve cannot, which is why this exists for the other two and
/// not for that one.
/// </para>
/// </remarks>
public sealed class FixFieldFactoryTests
{
	[Fact]
	public void Every_tag_builds_the_field_its_tag_names()
	{
		IReadOnlyDictionary<int, FixCustom>? custom = null;
		var wrong  = new List<string>();
		var named  = Named();

		for (var tag = 1; tag <= 1100; tag++)
		{
			var expected = named.TryGetValue(tag, out var name) ? name : nameof(FixField.Invalid);

			foreach (var (door, field) in Both(tag, custom))
			{
				if (field.GetType().Name != expected)
					wrong.Add($"tag {tag} through {door}: built {field.GetType().Name}, and the tag names {expected}.");

				if (field.Tag != tag && expected != nameof(FixField.Invalid))
					wrong.Add($"tag {tag} through {door}: the field it built carries tag {field.Tag}.");
			}
		}

		Assert.True(wrong.Count == 0,
			$"{wrong.Count} of 2200:" + Environment.NewLine + string.Join(Environment.NewLine, wrong.GetRange(0, Math.Min(20, wrong.Count))));
	}

	/// <summary>
	/// The tags past the last one the standard defines come back as Invalid through both doors.
	/// </summary>
	/// <remarks>
	/// The edge of the last part is the one place a regrouping can change behaviour quietly: a tag
	/// above the standard's last falls into whichever part its division lands it in, and out of
	/// that part's default. Under one divisor it is one part, under another it is a different one,
	/// and both roads end at the same answer — which is why a difference here would not show.
	/// </remarks>
	[Theory]
	[InlineData(956)]   // the last tag FIX 4.4 defines
	[InlineData(957)]   // the first above it
	[InlineData(999)]   // the last of a hundred-wide part
	[InlineData(1000)]  // the first of the next
	[InlineData(1023)]  // the last of a sixty-four-wide part
	[InlineData(1024)]  // the first of the next
	[InlineData(5000)]  // where a counterparty writes its own
	public void A_tag_past_the_standard_is_Invalid_through_both_doors(int tag)
	{
		IReadOnlyDictionary<int, FixCustom>? custom = null;

		foreach (var (door, field) in Both(tag, custom))
		{
			if (tag == 956)
			{
				Assert.False(field is FixField.Invalid, $"tag 956 through {door} should be a field of the standard.");

				continue;
			}

			Assert.True(field is FixField.Invalid, $"tag {tag} through {door} built {field.GetType().Name}.");
		}
	}

	/// <summary>The two doors, so that neither is checked alone.</summary>
	static IEnumerable<(string Door, FixField Field)> Both(int tag, IReadOnlyDictionary<int, FixCustom>? custom)
	{
		yield return ("characters", FixFieldBuilder.Value(tag, "1".AsSpan(), custom));
		yield return ("octets",     FixFieldBuilder.Value(tag, Encoding.Latin1.GetBytes("1").AsSpan(), custom));
	}

	/// <summary>
	/// What each tag is called, which is what the classes are named after: QuickFIX's name where its
	/// dictionary declares the tag, since that is the name a dictionary a consumer loads spells it
	/// with, and the repository's where it does not.
	/// </summary>
	static Dictionary<int, string> Named()
	{
		var corpus = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Corpus");
		var named  = new Dictionary<int, string>();

		foreach (var field in XDocument.Load(Path.Combine(corpus, "FixRepository", "FIX.4.4", "Base", "Fields.xml")).Root!.Elements())
			named[int.Parse(field.Element(field.Name.Namespace + "Tag")!.Value, CultureInfo.InvariantCulture)] =
				field.Element(field.Name.Namespace + "Name")!.Value;

		foreach (var field in XDocument.Load(Path.Combine(corpus, "Fix", "FIX44.xml")).Root!.Element("fields")!.Elements("field"))
		{
			var tag = int.Parse(field.Attribute("number")!.Value, CultureInfo.InvariantCulture);

			if (named.ContainsKey(tag))
				named[tag] = field.Attribute("name")!.Value;
		}

		return named;
	}

}
