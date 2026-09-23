using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using DotGram.Finance.Fix44;

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
	/// The value a tag's field holds is the CLR type of the type the repository declares for it —
	/// except where the repository contradicts itself, and then it is a type that holds what the
	/// repository publishes.
	/// </summary>
	/// <remarks>
	/// <strong>Two fields of FIX 4.4 declare a type their own code set does not fit.</strong>
	/// MiscFeeType (139) is declared <c>char</c> and publishes the values 10, 11 and 12;
	/// MassCancelRejectReason (532) is declared <c>char</c> and publishes 99. Held to the declared
	/// type, a message carrying a value the same specification publishes would be refused. So the
	/// rule is not "agree with the declared type" but "hold what the specification publishes", and
	/// the exception is allowed only where the declared type demonstrably cannot.
	/// </remarks>
	[Fact]
	public void Every_tag_holds_the_type_the_repository_declares()
	{
		var wrong = new List<string>();

		foreach (var field in Load("Fields.xml").Elements())
		{
			var tag      = int.Parse(Text(field, "Tag"), CultureInfo.InvariantCulture);
			var declared = Text(field, "Type");
			var ours     = Held(tag);
			var theirs   = Clr(declared);

			if (ours == theirs)
				continue;

			var codes = Codes(tag);

			// A wider type is right only where the declared one cannot hold what the same
			// specification publishes for the field, and only if the field holds all of it.
			if (codes.Count != 0 &&
				codes.Any(code => !Fits(theirs, code)) &&
				codes.All(code => Accepted(tag, code)))
				continue;

			wrong.Add($"tag {tag} ({Text(field, "Name")}): the repository says {declared}, which is {theirs?.Name}; this package holds {ours?.Name}.");
		}

		Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
	}

	/// <summary>
	/// Every value the repository publishes for a tag is one this package reads and holds to the
	/// schema without a finding.
	/// </summary>
	/// <remarks>
	/// The property the test above is an approximation of, and the one that actually matters: a
	/// value the specification prints in its own table must not be refused. It is asked through the
	/// field factory and the compiled-in check of the field, which is the consumer's path and not a
	/// second opinion about it.
	/// </remarks>
	[Fact]
	public void Every_published_value_is_one_this_package_accepts()
	{
		var refused = new List<string>();

		foreach (var field in Load("Fields.xml").Elements())
		{
			var tag = int.Parse(Text(field, "Tag"), CultureInfo.InvariantCulture);

			foreach (var code in Codes(tag))
				if (!Accepted(tag, code))
					refused.Add($"tag {tag} ({Text(field, "Name")}) publishes '{code}', which this package refuses.");
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

	/// <summary>Whether a value read as this tag's field is valid and passes the field's compiled-in check.</summary>
	static bool Accepted(int tag, string code)
	{
		var field = FixFieldFactory.Value(tag, code.AsSpan(), FixContext.Default.CustomFields);

		if (!field.IsValid)
			return false;

		var check = typeof(FixValidators).GetProperty(FixNames.Name(tag)!)!.GetValue(FixValidators.Default)!;
		var host  = new FixMessage.Custom("ZZ", []);

		((Delegate)check).DynamicInvoke(FixContext.Default, host, field);

		return host.IsValid;
	}

	/// <summary>The CLR type a tag's field holds, or null where the package has no field class for it.</summary>
	static Type? Held(int tag)
	{
		var type = FixNames.Name(tag) is { } name ? typeof(FixField).GetNestedType(name) : null;

		return type?.BaseType is { IsGenericType: true } typed ? typed.GetGenericArguments()[0] : null;
	}

	/// <summary>The CLR type this package holds a type of the repository's spelling in.</summary>
	static Type? Clr(string declared)
	{
		return declared.ToUpperInvariant() switch
		{
			"INT" or "LENGTH" or "NUMINGROUP" or "SEQNUM" or "TAGNUM" or "DAYOFMONTH"   => typeof(long),
			"FLOAT" or "QTY" or "PRICE" or "PRICEOFFSET" or "AMT" or "PERCENTAGE"         => typeof(decimal),
			"CHAR"                                                                        => typeof(char),
			"BOOLEAN"                                                                     => typeof(bool),
			"STRING" or "CURRENCY" or "COUNTRY" or "EXCHANGE" or "MONTHYEAR" or "LANGUAGE" => typeof(string),
			"MULTIPLEVALUESTRING" or "MULTIPLESTRINGVALUE" or "MULTIPLECHARVALUE"         => typeof(string[]),
			"LOCALMKTDATE" or "UTCDATEONLY" or "UTCDATE"                                  => typeof(DateOnly),
			"UTCTIMEONLY"                                                                 => typeof(TimeOnly),
			"UTCTIMESTAMP"                                                                => typeof(DateTimeOffset),
			"DATA" or "XMLDATA"                                                           => typeof(ReadOnlyMemory<byte>),
			_                                                                             => null,
		};
	}

	/// <summary>Whether a published value could be held in a CLR type at all.</summary>
	static bool Fits(Type? type, string code)
	{
		return type == typeof(char)    ? code.Length == 1
		     : type == typeof(long)    ? long.TryParse(code, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _)
		     : type == typeof(decimal) ? decimal.TryParse(code, NumberStyles.Number, CultureInfo.InvariantCulture, out _)
		     : type == typeof(bool)    ? code is "Y" or "N"
		     : type is not null;
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
			Assert.Equal(pair.Value, FixContext.Default.LengthTag(pair.Key));
			Assert.Equal(pair.Key,   FixContext.Default.DataTag(pair.Value));
		}

		// And nothing the package calls a pair is outside that set.
		for (var tag = 1; tag < 1000; tag++)
			if (FixContext.Default.DataTag(tag) != 0)
				Assert.Contains(tag, pairs.Values);
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

			// A required group of the message's own is a required counter tag; a required component —
			// a block, or a group the repository names as a component (Parties) — is reported as a
			// component, which is the other test.
			if (component is not null && component.Value.Kind == "ImplicitBlockRepeating")
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

			// A block, or a group the repository names as a component, with fields to be missing.
			if (component is not null && component.Value.Kind is "Block" or "BlockRepeating" && Rows(component.Value.Id).Any())
				count++;
		}

		return count;
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
