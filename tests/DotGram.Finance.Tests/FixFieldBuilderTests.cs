using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Linq;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// Every tag, through both doors of the reading, against the name and the type the repository gives it.
/// </summary>
/// <remarks>
/// <para>
/// A field is a class of the type of its value, and which type a tag has is one table the generator
/// writes. A tag given the wrong type still parses, reads and validates, and is simply the wrong
/// field, so this asks for every tag from one to eleven hundred — past the last the standard
/// defines — and holds the answer to the published repository, read here and not from the table.
/// </para>
/// <para>
/// The names are held the same way: <see cref="FixTag"/> is every tag of every version the package
/// reads, named as the newest version that defines it names it.
/// </para>
/// </remarks>
public sealed class FixFieldBuilderTests
{
	[Fact]
	public void Every_tag_is_named_as_the_newest_version_names_it()
	{
		var wrong = new List<string>();
		var named = new Dictionary<int, string>();
		var ours  = typeof(FixTag).GetFields().ToDictionary(one => (int)one.GetRawConstantValue()!, one => one.Name);

		foreach (var version in Versions)
			foreach (var (tag, (name, _)) in Fields(version))
				named[tag] = name;

		foreach (var (tag, name) in named)
			if (ours.GetValueOrDefault(tag) != name)
				wrong.Add($"tag {tag}: FixTag names it {ours.GetValueOrDefault(tag) ?? "nothing"}, and the repository {name}.");

		foreach (var (tag, name) in ours)
			if (!named.ContainsKey(tag))
				wrong.Add($"FixTag.{name} is {tag}, which no version defines.");

		Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
	}

	[Fact]
	public void Every_tag_builds_the_class_of_its_type()
	{
		var wrong = new List<string>();
		var named = Fields("FIX.4.4");

		for (var tag = 1; tag <= 1100; tag++)
		{
			var expected = named.TryGetValue(tag, out var field) ? Class(tag, field.Type) : nameof(FixField.Invalid);

			foreach (var (door, built) in Both(tag))
			{
				if (built.GetType().Name != expected)
					wrong.Add($"tag {tag} through {door}: built {built.GetType().Name}, and its type is {expected}.");

				if (built.Tag != tag)
					wrong.Add($"tag {tag} through {door}: the field it built carries tag {built.Tag}.");
			}
		}

		Assert.True(wrong.Count == 0,
			$"{wrong.Count} of 2200:" + Environment.NewLine + string.Join(Environment.NewLine, wrong.GetRange(0, Math.Min(20, wrong.Count))));
	}

	[Theory]
	[InlineData(957)]   // the first above the last FIX 4.4 defines
	[InlineData(5000)]  // where a counterparty writes its own
	[InlineData(65536)] // past the table, where a tag is looked up
	public void A_tag_past_the_standard_is_Invalid_through_both_doors(int tag)
	{
		foreach (var (door, field) in Both(tag))
			Assert.True(field is FixField.Invalid { IsValid: false }, $"tag {tag} through {door} built {field.GetType().Name}.");
	}

	/// <summary>The two doors, so that neither is checked alone.</summary>
	static IEnumerable<(string Door, FixField Field)> Both(int tag)
	{
		var type = Fix44Context.Default.Type(tag);

		yield return ("characters", FixFieldBuilder.Value(tag, type, "1".AsSpan()));
		yield return ("octets",     FixFieldBuilder.Value(tag, type, Encoding.Latin1.GetBytes("1").AsSpan()));
	}

	// The class a type of the repository is read into. MiscFeeType and MassCancelRejectReason are
	// declared char and publish values a character cannot hold, so they are read as text.
	static string Class(int tag, string type)
	{
		if (tag is 139 or 532)
			return nameof(FixField.Text);

		return type switch
		{
			"String" or "Currency" or "Exchange" or "Country"                       => nameof(FixField.Text),
			"char"                                                                  => nameof(FixField.Character),
			"Boolean"                                                               => nameof(FixField.Boolean),
			"int" or "Length" or "SeqNum" or "NumInGroup"                           => nameof(FixField.Integer),
			"float" or "Qty" or "Price" or "PriceOffset" or "Amt" or "Percentage"   => nameof(FixField.Decimal),
			"UTCTimestamp"                                                          => nameof(FixField.Timestamp),
			"UTCTimeOnly"                                                           => nameof(FixField.Time),
			"UTCDateOnly" or "LocalMktDate"                                         => nameof(FixField.Date),
			"MonthYear"                                                             => nameof(FixField.MonthYear),
			"MultipleValueString"                                                   => nameof(FixField.Multiple),
			"data"                                                                  => nameof(FixField.Data),
			_                                                                       => "a type the repository did not have: " + type,
		};
	}

	/// <summary>The versions the package reads, oldest first, as the repository's directories name them.</summary>
	static readonly string[] Versions = ["FIX.4.2", "FIX.4.4"];

	/// <summary>What each tag of a version is called and its type, as the repository gives them.</summary>
	static Dictionary<int, (string Name, string Type)> Fields(string version)
	{
		var corpus = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Corpus", "FixRepository");
		var named  = new Dictionary<int, (string Name, string Type)>();

		foreach (var field in XDocument.Load(Path.Combine(corpus, version, "Base", "Fields.xml")).Root!.Elements())
			named[int.Parse(field.Element(field.Name.Namespace + "Tag")!.Value, CultureInfo.InvariantCulture)] =
				(field.Element(field.Name.Namespace + "Name")!.Value, field.Element(field.Name.Namespace + "Type")!.Value);

		return named;
	}
}
