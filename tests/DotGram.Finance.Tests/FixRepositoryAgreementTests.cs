using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// What each of the ninety-three message types asks for, held against what the specification's own
/// machine-readable form says it must have.
/// </summary>
/// <remarks>
/// <para>
/// This is the test that stands in for generating the schema. Validation is written out type by
/// type — no table to compare against, no lookup that could be read twice — so the guarantee that
/// the transfer from the specification is complete cannot come from the code agreeing with itself.
/// It comes from here: the repository is read afresh, and what it requires of a message is held
/// against what an empty message of that type is told it is missing.
/// </para>
/// <para>
/// It reads <c>tests/Corpus/FixRepository</c>, which is the FIX Repository as its authors publish
/// it, and never the dictionary in <c>tests/Corpus/Fix</c>, which is an implementor's file. Two
/// readings of one specification check each other through the public call rather than over their
/// tables.
/// </para>
/// </remarks>
public sealed class FixRepositoryAgreementTests
{
	static readonly string Base = Path.Combine(
		AppContext.BaseDirectory, "..", "..", "..", "..", "Corpus", "FixRepository", "FIX.4.4", "Base");

	/// <summary>Every message type, by the name and the MsgType the repository gives it.</summary>
	public static TheoryData<string, string> Messages()
	{
		var data = new TheoryData<string, string>();

		foreach (var message in Load("Messages.xml").Elements())
			data.Add(Text(message, "Name"), Text(message, "MsgType"));

		return data;
	}

	/// <summary>
	/// A message of this type with nothing in it asks for exactly the fields the repository marks
	/// required, and for no others.
	/// </summary>
	/// <remarks>
	/// The two readings meet at the public call rather than over their tables: this one reads the
	/// repository here, and the package read it when its checks were written. What this can prove
	/// is that nothing was lost between the table and the code — which is what three hundred and
	/// twenty-five checks across ninety-three types is most likely to lose. It cannot prove the
	/// table is right; nothing here can.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Messages))]
	public void A_message_asks_for_the_fields_the_repository_requires(string name, string type)
	{
		var message = Empty(type);

		message.Validate(FixContext.Default);

		var asked = (message.InvalidFindings ?? [])
			.Where(one => one.Rule == FixRule.RequiredFieldMissing)
			.Select(one => one.Tag)
			.OrderBy(tag => tag)
			.ToArray();

		Assert.Equal(Required(type), asked);
		Assert.NotNull(name);
	}

	/// <summary>
	/// And it is told about exactly the blocks the repository marks required, named by the first tag
	/// each would have held.
	/// </summary>
	/// <remarks>
	/// A block is written into its carrier field by field, so "the block is missing" is "the carrier
	/// has no field of it" — asked once of the interface every carrier implements, and asked here of
	/// all ninety-three types through the public call.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Messages))]
	public void A_message_asks_for_the_components_the_repository_requires(string name, string type)
	{
		var message = Empty(type);

		message.Validate(FixContext.Default);

		var asked = (message.InvalidFindings ?? []).Count(one => one.Rule == FixRule.RequiredComponentMissing);

		Assert.Equal(RequiredComponents(type), asked);
		Assert.NotNull(name);
	}

	/// <summary>
	/// The type this package gives a tag is the type the repository declares for it — except where
	/// the repository contradicts itself, and then it is a type that holds what the repository
	/// publishes.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The generator reads the repository's spelling and writes the member it lands on; the
	/// run-time reader of a dictionary does the same through <c>FixVocabulary</c>. Two copies of
	/// one map disagree silently — by leaving a tag unchecked rather than by failing — so they are
	/// held against each other here, where a disagreement is loud.
	/// </para>
	/// <para>
	/// <strong>Two fields of FIX 4.4 declare a type their own code set does not fit.</strong>
	/// MiscFeeType (139) is declared <c>char</c> and publishes the values 10, 11 and 12;
	/// MassCancelRejectReason (532) is declared <c>char</c> and publishes 99. Held to the declared
	/// type, a message carrying a value the same specification publishes would be refused. So the
	/// rule is not "agree with the declared type" but "hold what the specification publishes", and
	/// the exception is allowed only where the declared type demonstrably cannot.
	/// </para>
	/// </remarks>
	[Fact]
	public void Every_tag_has_the_type_the_repository_declares()
	{
		var wrong = new List<string>();

		foreach (var field in Load("Fields.xml").Elements())
		{
			var tag      = int.Parse(Text(field, "Tag"), CultureInfo.InvariantCulture);
			var declared = Text(field, "Type");
			var ours     = FixSchema.Type(tag);
			var theirs   = Vocabulary(declared);

			if (ours == theirs)
				continue;

			var codes = Codes(tag);

			// A wider type is right only where the declared one cannot hold what the same
			// specification publishes for the field, and only if the wider one holds all of it.
			if (codes.Count != 0 &&
				codes.Any(code => !Fits(theirs, tag, code)) &&
				codes.All(code => Fits(ours, tag, code)))
				continue;

			wrong.Add($"tag {tag} ({Text(field, "Name")}): the repository says {declared}, which is {theirs}; this package says {ours}.");
		}

		Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
	}

	/// <summary>
	/// Every value the repository publishes for a tag is one this package's type for that tag
	/// accepts.
	/// </summary>
	/// <remarks>
	/// The property the test above is an approximation of, and the one that actually matters: a
	/// value the specification prints in its own table must not be refused. It is asked through
	/// <c>FixPrimitives</c>, which is what a validator asks, so this is the consumer's path and not
	/// a second opinion about it.
	/// </remarks>
	[Fact]
	public void Every_published_value_is_one_this_package_accepts()
	{
		var refused = new List<string>();

		foreach (var field in Load("Fields.xml").Elements())
		{
			var tag  = int.Parse(Text(field, "Tag"), CultureInfo.InvariantCulture);
			var type = FixSchema.Type(tag);

			if (type == FixValueType.None)
				continue;

			foreach (var code in Codes(tag))
				if (!Fits(type, tag, code))
					refused.Add($"tag {tag} ({Text(field, "Name")}) publishes '{code}', which this package's {type} refuses.");
		}

		Assert.True(refused.Count == 0, string.Join(Environment.NewLine, refused));
	}

	/// <summary>The values the repository publishes for a tag, or none where it publishes none.</summary>
	static IReadOnlyList<string> Codes(int tag)
	{
		return Load("Enums.xml").Elements()
			.Where(one => Text(one, "Tag") == tag.ToString(CultureInfo.InvariantCulture))
			.Select(one => Text(one, "Value"))
			.ToArray();
	}

	/// <summary>Whether a value fits a type, asked of the code a validator asks.</summary>
	static bool Fits(FixValueType type, int tag, string code)
	{
		return FixPrimitives.Valid(tag, code, type, null);
	}

	/// <summary>
	/// Where a component's declared type disagrees with its shape, and the shape is what is read.
	/// </summary>
	/// <remarks>
	/// The repository declares <c>Hop</c> an <c>ImplicitBlock</c>, and its rows are a counter with
	/// three members one indent deeper — a repeating group, which is how this package reads it and
	/// how a message on the wire is written. The disagreement is pinned here rather than absorbed:
	/// everything that decides between a block and a group asks the shape, and if a later edition
	/// of the repository disagrees somewhere else, this names the place instead of changing what is
	/// read behind our backs.
	/// </remarks>
	[Fact]
	public void A_components_declared_type_agrees_with_its_shape()
	{
		string[] known = ["Hop"];

		var disagreeing = new List<string>();

		foreach (var component in Load("Components.xml").Elements())
		{
			var name     = Text(component, "Name");
			var id       = int.Parse(Text(component, "ComponentID"), CultureInfo.InvariantCulture);
			var declared = Text(component, "ComponentType").EndsWith("Repeating", StringComparison.Ordinal);

			if (declared != IsGroup(id) && !known.Contains(name))
				disagreeing.Add($"{name} ({id}) is declared {Text(component, "ComponentType")} and is shaped like {(IsGroup(id) ? "a group" : "a block")}.");
		}

		Assert.True(disagreeing.Count == 0, string.Join(Environment.NewLine, disagreeing));

		// And the one that is known still disagrees, so that the exception is not kept after the
		// reason for it has gone.
		Assert.All(known, name =>
		{
			var component = Load("Components.xml").Elements().Single(one => Text(one, "Name") == name);
			var id        = int.Parse(Text(component, "ComponentID"), CultureInfo.InvariantCulture);

			Assert.NotEqual(Text(component, "ComponentType").EndsWith("Repeating", StringComparison.Ordinal), IsGroup(id));
		});
	}

	/// <summary>
	/// The sixteen length/data pairs are the sixteen the repository describes, and no others.
	/// </summary>
	/// <remarks>
	/// Taken from the fields of type <c>data</c> and the length field named after each, because
	/// <c>BodyLength</c> and <c>MaxMessageSize</c> also carry the type <c>Length</c> and measure no
	/// data field: a rule that read the length type alone would invent two pairs that do not exist.
	/// </remarks>
	[Fact]
	public void The_length_data_pairs_are_the_repositorys()
	{
		var byName = Load("Fields.xml").Elements().ToDictionary(one => Text(one, "Name"));
		var pairs  = new SortedDictionary<int, int>();

		foreach (var field in Load("Fields.xml").Elements())
		{
			if (Text(field, "Type") is not ("data" or "XMLData"))
				continue;

			var name = Text(field, "Name");
			var data = int.Parse(Text(field, "Tag"), CultureInfo.InvariantCulture);

			if (!byName.TryGetValue(name + "Len", out var length) && !byName.TryGetValue(name + "Length", out length))
				throw new InvalidOperationException($"No length field for data field {name}, tag {data}.");

			pairs[data] = int.Parse(Text(length, "Tag"), CultureInfo.InvariantCulture);
		}

		Assert.Equal(16, pairs.Count);

		foreach (var pair in pairs)
		{
			Assert.Equal(pair.Value, FixSchema.LengthTag(pair.Key));
			Assert.Equal(pair.Key,   FixSchema.DataTag(pair.Value));
		}

		// And nothing the package calls a pair is outside that set.
		for (var tag = 1; tag < 1000; tag++)
			if (FixSchema.DataTag(tag) != 0)
				Assert.Contains(tag, pairs.Values);
	}

	/// <summary>
	/// Every tag the specification declares is inside the mask a scope marks what it has seen with.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Trivially true today: FIX 4.4's largest tag is 956 and the mask covers 1 through
	/// <see cref="FixSchema.TagLimit"/>. It is asserted because a deletion rests on it. Asking
	/// whether a scope holds a tag is done by reading that mask, and the branch that looked for a
	/// tag outside it among the fields themselves existed for a counterparty's own tag, arriving
	/// from a loaded dictionary. Nothing generated asks it: a validator asks only about tags the
	/// schema lists.
	/// </para>
	/// <para>
	/// If a later edition of the repository -- or another version of FIX read through the same
	/// files -- ever declares a tag above the mask, presence would be asked of a bit that is not
	/// there. This says so at once instead of letting it become a silent wrong answer.
	/// </para>
	/// </remarks>
	[Fact]
	public void Every_tag_the_repository_declares_is_inside_the_mask()
	{
		var largest = Load("Fields.xml").Elements()
			.Select(one => int.Parse(Text(one, "Tag"), CultureInfo.InvariantCulture))
			.Max();

		Assert.True(largest < FixSchema.TagLimit,
			$"the repository declares tag {largest} and the mask covers 1 through {FixSchema.TagLimit - 1}.");
	}

	// ── the repository, read afresh ──────────────────────────────────────────────────────────

	// Read once for the whole class: ninety-three types times two tests is a hundred and
	// eighty-six readings of a seven-hundred-kilobyte file otherwise.
	static readonly Dictionary<string, XElement> Files = new();

	static XElement Load(string name)
	{
		lock (Files)
		{
			if (!Files.TryGetValue(name, out var root))
				Files[name] = root = XDocument.Load(Path.Combine(Base, name)).Root!;

			return root;
		}
	}

	static string Text(XElement element, string name)
	{
		return element.Element(name)?.Value.Trim() ?? "";
	}

	/// <summary>The rows of a message or component, in the order the repository gives them.</summary>
	static IEnumerable<XElement> Rows(int componentId)
	{
		return Load("MsgContents.xml").Elements()
			.Where(one => Text(one, "ComponentID") == componentId.ToString(CultureInfo.InvariantCulture))
			.OrderBy(one => double.Parse(Text(one, "Position"), CultureInfo.InvariantCulture));
	}

	static int ComponentId(string type)
	{
		return int.Parse(
			Text(Load("Messages.xml").Elements().Single(one => Text(one, "MsgType") == type), "ComponentID"),
			CultureInfo.InvariantCulture);
	}

	static (int Id, string Kind)? Component(string name)
	{
		var found = Load("Components.xml").Elements().FirstOrDefault(one => Text(one, "Name") == name);

		return found is null
			? null
			: (int.Parse(Text(found, "ComponentID"), CultureInfo.InvariantCulture), Text(found, "ComponentType"));
	}

	/// <summary>
	/// Whether a component IS a repeating group, asked of its rows rather than of its declared type.
	/// </summary>
	/// <remarks>
	/// <c>ComponentType</c> cannot be trusted for this: <c>Hop</c> is declared <c>ImplicitBlock</c>
	/// and its rows are a counter with three members one indent deeper, which is a group and nothing
	/// else. The structure is what the reader of a message meets, so the structure decides, and the
	/// declared type is kept as a second opinion — see
	/// <see cref="A_components_declared_type_agrees_with_its_shape"/>.
	/// </remarks>
	static bool IsGroup(int componentId)
	{
		var rows = Rows(componentId).ToArray();

		if (rows.Length < 2)
			return false;

		var outermost = rows.Min(one => int.Parse(Text(one, "Indent"), CultureInfo.InvariantCulture));
		var opening   = rows.Where(one => int.Parse(Text(one, "Indent"), CultureInfo.InvariantCulture) == outermost).ToArray();

		return opening.Length == 1 && int.TryParse(Text(opening[0], "TagText"), NumberStyles.None, CultureInfo.InvariantCulture, out _);
	}

	/// <summary>The tags a message of this type must carry in its body, the repository's answer.</summary>
	static int[] Required(string type)
	{
		var tags = new List<int>();

		foreach (var row in Rows(ComponentId(type)))
		{
			// What the message itself requires. A row deeper than that is a requirement of the
			// component it sits in, and is asked of that component rather than of every type that
			// carries it.
			if (Text(row, "Reqd") != "1" || Text(row, "Indent") != "0")
				continue;

			var text = Text(row, "TagText");

			if (int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var tag))
			{
				tags.Add(tag);

				continue;
			}

			var component = Component(text);

			// A required repeating group is a required counter tag; a required block is reported as
			// a component, which is the other test.
			if (component is not null && IsGroup(component.Value.Id))
				tags.Add(int.Parse(Text(Rows(component.Value.Id).First(), "TagText"), CultureInfo.InvariantCulture));
		}

		tags.Sort();

		return tags.ToArray();
	}

	/// <summary>How many components of the body the repository requires.</summary>
	static int RequiredComponents(string type)
	{
		var count = 0;

		foreach (var row in Rows(ComponentId(type)))
		{
			// What the message itself requires, as in Required() above.
			if (Text(row, "Reqd") != "1" || Text(row, "Indent") != "0")
				continue;

			var text = Text(row, "TagText");

			if (int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out _))
				continue;

			// The standard header and the standard trailer are scopes of their own, walked beside
			// the body rather than inside it, so a body's reference to them is not a component the
			// body can be missing.
			if (text is "StandardHeader" or "StandardTrailer")
				continue;

			var component = Component(text);

			// A component with no fields of its own has nothing to be missing.
			if (component is not null && !IsGroup(component.Value.Id) && Rows(component.Value.Id).Any())
				count++;
		}

		return count;
	}

	/// <summary>This package's reading of the repository's spelling of a type.</summary>
	static FixValueType Vocabulary(string declared)
	{
		return declared.ToUpperInvariant() switch
		{
			"STRING" or "LANGUAGE"                                           => FixValueType.String,
			"CHAR"                                                           => FixValueType.Char,
			"INT"                                                            => FixValueType.Int,
			"LENGTH"                                                         => FixValueType.Length,
			"NUMINGROUP"                                                     => FixValueType.NumInGroup,
			"SEQNUM"                                                         => FixValueType.SeqNum,
			"TAGNUM"                                                         => FixValueType.TagNum,
			"DAYOFMONTH"                                                     => FixValueType.DayOfMonth,
			"FLOAT"                                                          => FixValueType.Float,
			"QTY"                                                            => FixValueType.Qty,
			"PRICE"                                                          => FixValueType.Price,
			"PRICEOFFSET"                                                    => FixValueType.PriceOffset,
			"AMT"                                                            => FixValueType.Amt,
			"PERCENTAGE"                                                     => FixValueType.Percentage,
			"BOOLEAN"                                                        => FixValueType.Boolean,
			"CURRENCY"                                                       => FixValueType.Currency,
			"COUNTRY"                                                        => FixValueType.Country,
			"EXCHANGE"                                                       => FixValueType.Exchange,
			"MONTHYEAR"                                                      => FixValueType.MonthYear,
			"LOCALMKTDATE"                                                   => FixValueType.LocalMktDate,
			"UTCDATEONLY" or "UTCDATE"                                       => FixValueType.UTCDateOnly,
			"UTCTIMEONLY"                                                    => FixValueType.UTCTimeOnly,
			"UTCTIMESTAMP"                                                   => FixValueType.UTCTimestamp,
			"DATA" or "XMLDATA"                                              => FixValueType.Data,
			"MULTIPLEVALUESTRING" or "MULTIPLESTRINGVALUE" or "MULTIPLECHARVALUE" => FixValueType.MultipleValueString,
			_                                                                => FixValueType.None,
		};
	}

	// ── a message of that type with nothing in its body ──────────────────────────────────────

	static FixMessage Empty(string type)
	{
		var body = Encoding.Latin1.GetBytes(
			("35=" + type + "|49=SENDER|56=TARGET|34=1|52=20260920-12:00:00|").Replace('|', '\u0001'));
		var head = Encoding.Latin1.GetBytes("8=FIX.4.4\u00019=" + body.Length + "\u0001");
		var sum  = head.Sum(octet => (int)octet) + body.Sum(octet => (int)octet);
		var tail = Encoding.Latin1.GetBytes("10=" + (sum % 256).ToString("000") + "\u0001");

		var wire = new byte[head.Length + body.Length + tail.Length];

		head.CopyTo(wire, 0);
		body.CopyTo(wire, head.Length);
		tail.CopyTo(wire, head.Length + body.Length);

		return FixParser.ParseMessage(wire);
	}
}
