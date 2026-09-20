using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DotGram.Finance.Fix;

/// <summary>
/// The schema a walk reads, so that one walk reads either the tables this package compiles in or a
/// dictionary somebody loaded.
/// </summary>
/// <remarks>
/// This is what D71 means by one reader: build time and run time fill the same shapes, so there is
/// one implementation of the rules rather than two. Where the two roads diverge is D72's step
/// after this one — a generator unrolling a message's rules into straight-line code, which is a
/// third filling of the same shapes and not a second walk.
/// </remarks>
abstract class FixTables
{
	public abstract SchemaRef[] Message(string type);

	/// <summary>
	/// Whether the schema describes a message of this type at all.
	/// </summary>
	/// <remarks>
	/// Not the same as having members. The published FIX 4.4 dictionary carries XMLnonFIX with a
	/// comment where its body would be — "this message has no body fields" — and reading an empty
	/// composition as "no such type" is how that message came to be reported unknown by a file
	/// that describes it.
	/// </remarks>
	public abstract bool Describes(string type);
	public abstract SchemaRef[] Component(int id);
	public abstract SchemaRef[] Group(int id);
	public abstract int         Counter(int id);
	public abstract SchemaRef[] Header  { get; }
	public abstract SchemaRef[] Trailer { get; }
	public abstract byte        Type(int tag);
	public abstract string[]?   Codes(int tag);
	public abstract int         LengthTag(int dataTag);
	public abstract int         DataTag(int lengthTag);
	public abstract bool        RequiresEncoding(int tag);

	/// <summary>Whether the schema places this tag in a scope at all, for the sentence of a finding.</summary>
	public abstract bool Defines(int tag);

	/// <summary>
	/// Whether a component id is a scope walked on its own rather than a member of the body.
	/// </summary>
	/// <remarks>
	/// The compiled tables make the header and the trailer components of every message, so a
	/// body's references to them are not followed. A loaded dictionary has its own ids and its
	/// header is a list of its own, so nothing here is outer.
	/// </remarks>
	public virtual bool IsOuterScope(int componentId) => false;

	readonly ConditionalWeakTable<SchemaRef[], Dictionary<int, int>> scopes = new();

	/// <summary>
	/// Every tag a scope lists, to the group it counts or zero — resolved against THESE tables.
	/// </summary>
	/// <remarks>
	/// Construction has a map of the same shape in FixSemantics, and it is not this one: that one
	/// resolves a component against the compiled tables, which is right for reading the wire and
	/// wrong for a dictionary whose ids are its own. Reading a venue's file with the wrong one
	/// throws "Unknown component" on the first message, which is how this was found.
	/// </remarks>
	public Dictionary<int, int> Members(SchemaRef[] schema) => scopes.GetValue(schema, Create);

	Dictionary<int, int> Create(SchemaRef[] schema)
	{
		var members = new Dictionary<int, int>();

		Add(schema, members);

		return members;
	}

	void Add(SchemaRef[] schema, Dictionary<int, int> members)
	{
		foreach (var reference in schema)
		{
			if (reference.Kind == 1)
			{
				if (!IsOuterScope(reference.Id))
					Add(Component(reference.Id), members);
			}
			else
			{
				members[reference.Kind == 2 ? Counter(reference.Id) : reference.Id] =
					reference.Kind == 2 ? reference.Id : 0;
			}
		}
	}
}

/// <summary>FIX 4.4 as this package compiles it.</summary>
sealed class CompiledTables : FixTables
{
	public static readonly CompiledTables Instance = new();

	CompiledTables() { }

	public override SchemaRef[] Message(string type) => FixSchema.Message(type);

	// The compiled tables have no way to say it but the composition, which is what they have
	// always answered with; an empty one here means the type is not described.
	public override bool Describes(string type) => FixSchema.Message(type).Length != 0;
	public override SchemaRef[] Component(int id) => FixSchema.Component(id);
	public override SchemaRef[] Group(int id) => FixSchema.Group(id);
	public override int         Counter(int id) => FixSchema.Counter(id);
	public override SchemaRef[] Header  => FixSchema.Component(1024);
	public override SchemaRef[] Trailer => FixSchema.Component(1025);
	public override byte        Type(int tag) => FixSchema.TypeCode(tag);
	public override string[]?   Codes(int tag) => FixSchema.Codes(tag);
	public override int         LengthTag(int dataTag) => FixSchema.LengthTag(dataTag);
	public override int         DataTag(int lengthTag) => FixSchema.DataTag(lengthTag);
	public override bool        RequiresEncoding(int tag) => FixSchema.RequiresEncoding(tag);
	public override bool        Defines(int tag) => FixSchema.Defines(tag);
	public override bool        IsOuterScope(int componentId) => componentId is 1024 or 1025;
}

/// <summary>A dictionary somebody loaded, in the same shapes.</summary>
/// <remarks>
/// The header and the trailer are scopes of their own here as they are in the compiled tables, and
/// the two of them are why a body's references to component 1024 and 1025 are skipped in the walk:
/// a loaded dictionary has no such ids, and its header is simply its own list.
/// </remarks>
sealed class DictionaryTables(FixDictionary dictionary) : FixTables
{
	public override SchemaRef[] Message(string type) => dictionary.Message(type);
	public override bool        Describes(string type) => dictionary.Describes(type);
	public override SchemaRef[] Component(int id) => dictionary.Component(id);
	public override SchemaRef[] Group(int id) => dictionary.Group(id);
	public override int         Counter(int id) => dictionary.Counter(id);
	public override SchemaRef[] Header  => dictionary.Header;
	public override SchemaRef[] Trailer => dictionary.Trailer;
	/// <summary>
	/// The file's type name in this package's vocabulary, or null where it has no counterpart.
	/// </summary>
	/// <remarks>
	/// A dictionary spells its types its own way — <c>UTCTIMESTAMP</c> where we write
	/// <c>UTCTimestamp</c> — and holding a value to a name we do not know would call every value
	/// of that tag wrong. Where there is no counterpart the answer is null and the value is not
	/// checked, which is the honest answer: we cannot say a value is wrong against a type we do
	/// not model. <c>TZTIMEONLY</c> and <c>TZTIMESTAMP</c> are the two FIX 4.4 names that land
	/// there.
	/// </remarks>
	// One read a tag at construction, not a name parsed a field: loading is where a dictionary is
	// allowed to be slow, and checking is not.
	readonly byte[] types = Codes(dictionary);

	public override byte Type(int tag) => (uint)tag < (uint)types.Length ? types[tag] : (byte)0;

	static byte[] Codes(FixDictionary dictionary)
	{
		var widest = 0;

		foreach (var tag in dictionary.Tags)
			if (tag > widest)
				widest = tag;

		var codes = new byte[widest + 1];

		foreach (var tag in dictionary.Tags)
			codes[tag] = FixPrimitives.Code(Vocabulary(dictionary.CodeType(tag)));

		return codes;
	}

	internal static string? Vocabulary(string? declared) => declared?.ToUpperInvariant() switch
	{
		"STRING" or "LANGUAGE"             => "String",
		"CHAR"                             => "char",
		"INT"                              => "int",
		"LENGTH"                           => "Length",
		"NUMINGROUP"                       => "NumInGroup",
		"SEQNUM"                           => "SeqNum",
		"TAGNUM"                           => "TagNum",
		"DAYOFMONTH"                       => "DayOfMonth",
		"FLOAT"                            => "float",
		"QTY"                              => "Qty",
		"PRICE"                            => "Price",
		"PRICEOFFSET"                      => "PriceOffset",
		"AMT"                              => "Amt",
		"PERCENTAGE"                       => "Percentage",
		"BOOLEAN"                          => "Boolean",
		"CURRENCY"                         => "Currency",
		"COUNTRY"                          => "Country",
		"EXCHANGE"                         => "Exchange",
		"MONTHYEAR"                        => "MonthYear",
		"LOCALMKTDATE"                     => "LocalMktDate",
		"UTCDATEONLY" or "UTCDATE"         => "UTCDateOnly",
		"UTCTIMEONLY"                      => "UTCTimeOnly",
		"UTCTIMESTAMP"                     => "UTCTimestamp",
		"MULTIPLEVALUESTRING" or "MULTIPLESTRINGVALUE" or "MULTIPLECHARVALUE" => "MultipleValueString",
		"DATA" or "XMLDATA"                => "data",
		_                                  => null,
	};
	public override string[]?   Codes(int tag) => dictionary.CodeArray(tag);
	public override bool        Defines(int tag) => dictionary.Defines(tag);

	// A QuickFIX dictionary wires a length to its data by the convention that the length tag is
	// the data tag minus one, with Signature the one exception it hard-codes. Reading that
	// convention off a loaded file would be guessing where the file is silent, so the pairs stay
	// the standard's until a format that declares them arrives — Orchestra's lengthId.
	public override int  LengthTag(int dataTag) => FixSchema.LengthTag(dataTag);
	public override int  DataTag(int lengthTag) => FixSchema.DataTag(lengthTag);
	public override bool RequiresEncoding(int tag) => FixSchema.RequiresEncoding(tag);
}

/// <summary>The rules, over whatever schema they are given.</summary>
static class FixRules
{
	public static void Check(FixTables tables, FixMessage message, List<FixFinding> found)
	{
		var schema = tables.Message(message.MessageType);

		if (!tables.Describes(message.MessageType))
			found.Add(new FixFinding(
				FixRule.UnknownMessageType, FixScope.Header, 35, 0, -1, 0,
				$"The schema defines no message of type '{message.MessageType}'."));

		if (message.Header.GetField(347) == null && (Encoded(tables, message.Header) || Encoded(tables, message)))
			found.Add(new FixFinding(
				FixRule.MessageEncodingMissing, FixScope.Header, 347, 0, -1, 0,
				"MessageEncoding is required where an Encoded field is present."));

		Scope(tables, found, message.Header,  tables.Header,  FixScope.Header,  0, -1, ordered: false);
		Scope(tables, found, message,         schema,         FixScope.Body,    0, -1, ordered: false);
		Scope(tables, found, message.Trailer, tables.Trailer, FixScope.Trailer, 0, -1, ordered: false);
	}

	static bool Encoded(FixTables tables, FixFieldSet scope)
	{
		foreach (var node in scope.Nodes)
		{
			if (tables.RequiresEncoding(node.Tag))
				return true;

			if (node.Entries != null)
				foreach (var entry in node.Entries)
					if (Encoded(tables, entry))
						return true;
		}

		return false;
	}

	static void Scope(
		FixTables        tables,
		List<FixFinding> found,
		FixFieldSet      scope,
		SchemaRef[]      schema,
		FixScope         where,
		int              groupTag,
		int              entryIndex,
		bool             ordered)
	{
		Span<ulong> seen = stackalloc ulong[15];

		var           previousRank = -1;
		HashSet<int>? extended     = null;

		// Where a message names a type the schema does not know there is nothing to be out of
		// place against, and saying so of every field would bury the one finding that matters.
		var members = schema.Length == 0 ? null : tables.Members(schema);

		seen.Clear();

		for (var i = 0; i < scope.Nodes.Length; i++)
		{
			var node  = scope.Nodes[i];
			var field = node.Field(scope.Source);

			if (ordered)
			{
				var ordinal = 0;
				var rank    = Rank(tables, schema, node.Tag, ref ordinal);

				if (rank >= 0 && rank < previousRank)
					found.Add(Wrong(FixRule.FieldOutOfOrder, where, field, groupTag, entryIndex,
						"The fields of a group entry are not in the order the schema gives them."));
				else if (rank >= 0)
					previousRank = rank;
			}

			if (node.Tag is < 957 and > 0)
			{
				var bit = 1UL << (node.Tag & 63);

				if ((seen[node.Tag >> 6] & bit) != 0)
					found.Add(Wrong(FixRule.DuplicateField, where, field, groupTag, entryIndex,
						"The tag appears more than once in this scope."));

				seen[node.Tag >> 6] |= bit;
			}
			else if (!(extended ??= []).Add(node.Tag))
			{
				found.Add(Wrong(FixRule.DuplicateField, where, field, groupTag, entryIndex,
					"The tag appears more than once in this scope."));
			}

			var lengthTag = tables.LengthTag(node.Tag);

			if (lengthTag != 0 &&
				(i == 0 ||
				 scope.Nodes[i - 1].Tag != lengthTag ||
				 !scope.Nodes[i - 1].Field(scope.Source).TryGetInt64(out var measured) ||
				 measured != node.Length))
				found.Add(Wrong(FixRule.DataFieldNotAfterLength, where, field, groupTag, entryIndex,
					$"A data field must immediately follow tag {lengthTag}, which measures it."));

			var dataTag = tables.DataTag(node.Tag);

			if (dataTag != 0 && (i + 1 == scope.Nodes.Length || scope.Nodes[i + 1].Tag != dataTag))
				found.Add(Wrong(FixRule.LengthFieldNotBeforeData, where, field, groupTag, entryIndex,
					$"A length field must immediately precede tag {dataTag}, which it measures."));

			// Construction asks where a field goes and puts it in the scope it was written in;
			// whether it belongs there is this question, and it is the one the strict mode used to
			// answer by refusing to build the message at all.
			if (members != null && !members.ContainsKey(node.Tag))
				found.Add(Wrong(FixRule.FieldNotInScope, where, field, groupTag, entryIndex,
					tables.Defines(node.Tag)
						? "The schema defines this tag, but not in this scope."
						: "The schema defines no such tag."));

			// Only where the schema gives the tag a type. A tag it does not describe is already a
			// finding of its own, and calling its value wrong on top of that says something we do
			// not know: there is no type to hold the value to.
			var type = tables.Type(node.Tag);

			if (type != 0 && !FixPrimitives.Valid(field, type, tables.Codes(node.Tag)))
				found.Add(Wrong(FixRule.InvalidValue, where, field, groupTag, entryIndex,
					"The value does not fit the field's type, or is not one of its code set."));
		}

		References(tables, found, scope, seen, schema, where, groupTag, entryIndex);
	}

	static void References(
		FixTables           tables,
		List<FixFinding>    found,
		FixFieldSet         scope,
		ReadOnlySpan<ulong> seen,
		SchemaRef[]         schema,
		FixScope            where,
		int                 groupTag,
		int                 entryIndex)
	{
		foreach (var reference in schema)
		{
			if (reference.Kind == 1)
			{
				// The header and the trailer are components of every message in the compiled
				// tables and are walked as scopes of their own, so a body's references to them are
				// not followed here. A loaded dictionary has no such ids and none of this fires.
				if (tables.IsOuterScope(reference.Id))
					continue;

				var child   = tables.Component(reference.Id);
				var present = Present(tables, scope, seen, child);

				if (reference.Required && !present)
					found.Add(new FixFinding(
						FixRule.RequiredComponentMissing, where, null, groupTag, entryIndex, 0,
						$"A component the schema requires has none of its fields; its first is tag {First(tables, child)}."));

				if (present)
					References(tables, found, scope, seen, child, where, groupTag, entryIndex);

				continue;
			}

			var tag = reference.Kind == 2 ? tables.Counter(reference.Id) : reference.Id;

			if (!Has(scope, seen, tag))
			{
				if (reference.Required)
					found.Add(new FixFinding(
						FixRule.RequiredFieldMissing, where, tag, groupTag, entryIndex, 0,
						"The schema requires this tag in this scope."));

				continue;
			}

			if (reference.Kind != 2)
				continue;

			var counter = scope.GetField(tag)!.Value;
			var entries = scope.GetGroup(tag);

			if (!counter.TryGetInt64(out var count) || count != entries.Count || count < 0 ||
				reference.Required && count == 0)
				found.Add(Wrong(FixRule.GroupCountMismatch, where, counter, groupTag, entryIndex,
					$"The count says {counter} and {entries.Count} entries follow."));

			for (var entry = 0; entry < entries.Count; entry++)
				Scope(tables, found, entries[entry], tables.Group(reference.Id), where, tag, entry, ordered: true);
		}
	}

	// A component is an id and a consumer cannot look an id up, so a finding about a missing
	// component names the first tag it would have held. That is the thing they can look up, and
	// usually the thing they forgot.
	static int First(FixTables tables, SchemaRef[] schema)
	{
		foreach (var reference in schema)
		{
			var tag = reference.Kind == 1
				? First(tables, tables.Component(reference.Id))
				: reference.Kind == 2 ? tables.Counter(reference.Id) : reference.Id;

			if (tag != 0)
				return tag;
		}

		return 0;
	}

	// A scope's standard tags are in the mask its fields marked; any other tag is looked for
	// among the fields themselves.
	internal static bool Has(FixFieldSet scope, ReadOnlySpan<ulong> seen, int tag) =>
		tag is > 0 and < 957 ? (seen[tag >> 6] & 1UL << (tag & 63)) != 0 : scope.GetField(tag) != null;

	static bool Present(FixTables tables, FixFieldSet scope, ReadOnlySpan<ulong> seen, SchemaRef[] schema)
	{
		foreach (var reference in schema)
			if (reference.Kind == 1
				? Present(tables, scope, seen, tables.Component(reference.Id))
				: Has(scope, seen, reference.Kind == 2 ? tables.Counter(reference.Id) : reference.Id))
				return true;

		return false;
	}

	static int Rank(FixTables tables, SchemaRef[] schema, int tag, ref int ordinal)
	{
		foreach (var reference in schema)
		{
			if (reference.Kind == 1)
			{
				var rank = Rank(tables, tables.Component(reference.Id), tag, ref ordinal);

				if (rank >= 0)
					return rank;
			}
			else
			{
				if ((reference.Kind == 2 ? tables.Counter(reference.Id) : reference.Id) == tag)
					return ordinal;

				ordinal++;
			}
		}

		return -1;
	}

	static FixFinding Wrong(FixRule rule, FixScope where, FixFieldView field, int groupTag, int entryIndex, string reason) =>
		new(rule, where, field.Tag, groupTag, entryIndex, field.ValuePosition, reason);
}
