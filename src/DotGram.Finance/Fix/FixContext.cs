using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DotGram.Finance.Fix;

/// <summary>What a reading is done by: the framing, the type of every field and the length/data pairs, and the checks a message is held to.</summary>
/// <remarks>
/// <para>
/// The part of a context every version of FIX shares; each version's context derives from it and
/// adds what is its own, <see cref="Fix44.Fix44Context"/> for FIX 4.4. One value, passed to every
/// call that reads and to a message's <c>Validate</c>, rather than read from anywhere: a process
/// reading two counterparties with two dictionaries is expressible, each pass over an input has its
/// own, and nothing about one reaches the other. Immutable, and changed with <c>with</c>.
/// </para>
/// <para>
/// What a reading has to know of a tag is the type its value is read as, which is the class of the
/// field, and whether it is half of a pair. FIX carries binary values as a pair of fields: a length,
/// then the tag whose value is that many octets and may hold the separator itself. The version
/// settles both for the tags it defines; a counterparty may define more, in the bilateral range,
/// and a dictionary loaded into the context types them while <see cref="LengthDataPairs"/> pairs
/// them. Both are worked out once, when the value is made, into a table the tag indexes — so the
/// reader's question on every field is an array read, and nothing a consumer supplies is consulted
/// in the middle of a parse. A tag neither the version nor a loaded dictionary defines is read as a
/// <see cref="FixField.Invalid"/>.
/// </para>
/// <para>
/// The checks are those of each message type, of each block and of each field, held as one object
/// of slots so that a dictionary loaded at run time replaces the slots it describes and leaves the
/// rest. That object is not part of what a consumer sees: what replaces a check is a dictionary,
/// read from a file, and not a delegate written by hand.
/// </para>
/// </remarks>
public abstract record FixContext
{
	// The version's pairs and types, and the table the reader indexes built from them.
	readonly FixVersion _version;

	private protected FixContext(FixVersion version, FixChecks checks)
	{
		_version = version;
		_pairs   = version.Pairs;
		Coded    = version.Codes;
		Checks   = checks;
	}

	/// <summary>How the input separates one field from the next: the wire's SOH, or a log's pipe.</summary>
	/// <exception cref="ArgumentOutOfRangeException">Neither of the two.</exception>
	public FixFraming Framing { get; init; }

	/// <summary>The initial size of the buffer a reader or a stream is read through, in characters or bytes: 4096 unless given.</summary>
	/// <exception cref="ArgumentOutOfRangeException">Not positive.</exception>
	public int BufferSize
	{
		get;
		init => field = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(BufferSize));
	} = 4096;

	/// <summary>
	/// The most characters or bytes one field read from a reader or a stream may take, from its tag
	/// through the separator that ends it, or a whole length/data pair: 16 Mi unless given.
	/// </summary>
	/// <remarks>
	/// It bounds one field, not the input, which is read and let go field by field. A field that needs
	/// more throws <see cref="System.IO.IOException"/>. Input read whole, a string or an array, is not
	/// bounded by it.
	/// </remarks>
	/// <exception cref="ArgumentOutOfRangeException">Not positive.</exception>
	public int MaxRetained
	{
		get;
		init => field = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(MaxRetained));
	} = FixGrammar.DefaultMaxRetained;

	readonly Dictionary<FixTag,FixTag> _pairs;

	// What the reader knows of every tag; set by LengthDataPairs and by a load.
	Codes Coded { get; init; }

	/// <summary>The length/data pairs, length tag to data tag: the version's and a consumer's own.</summary>
	/// <remarks>
	/// What is set is added to the version's pairs; a length tag set again replaces its pair. A tag of
	/// a pair that neither the version nor a loaded dictionary gives a type is read as its half of the
	/// pair is: a length as a <see cref="FixField.Integer"/>, the data as a <see cref="FixField.Data"/>.
	/// </remarks>
	public IReadOnlyDictionary<FixTag,FixTag> LengthDataPairs
	{
		get => _pairs;
		init
		{
			var pairs = new Dictionary<FixTag,FixTag>(_version.Pairs);

			foreach (var pair in value)
				pairs[pair.Key] = pair.Value;

			_pairs = pairs;
			Coded  = Coded.Paired(pairs);
		}
	}

	/// <summary>
	/// What the reader does with a tag: 1 a length, -1 the data a length measures, 0 an ordinary value.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int Kind(int tag)
	{
		return Coded.Kind(tag);
	}

	/// <summary>The type a tag's value is read as: the version's, a loaded dictionary's, or its half of a pair's.</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal FixValueType Type(int tag)
	{
		return Coded.Type(tag);
	}

	/// <summary>The check of each message type, of each block, and of each field: the version's.</summary>
	internal FixChecks Checks { get; init; }

	// A dictionary's checks go over this context's, and the fields it describes that the version does
	// not are read, from then on, as the types it gives them. A clone of this context, whatever
	// version's it is.
	private protected FixContext Loaded(FixDictionary dictionary, string? emitTo)
	{
		var types = new List<(int Tag, FixValueType Type)>();

		foreach (var field in dictionary.Fields)
			if (_version.Type((FixTag)field.Key) == FixValueType.None && TypeOf(field.Value.Type) is { } type)
				types.Add((field.Key, type));

		return this with
		{
			Checks = Checks.Load(dictionary, Emitter(emitTo)),
			Coded  = Coded.Typed(types),
		};
	}

	// The type of a field as a QuickFIX dictionary names it; a field the file gives no type is not
	// typed by it, and a name this does not know is text.
	static FixValueType? TypeOf(string? type)
	{
		return type?.ToUpperInvariant() switch
		{
			null                                                                                => null,
			"CHAR"                                                                              => FixValueType.Character,
			"BOOLEAN"                                                                           => FixValueType.Boolean,
			"INT" or "LENGTH" or "SEQNUM" or "NUMINGROUP" or "DAYOFMONTH" or "TAGNUM"           => FixValueType.Integer,
			"FLOAT" or "QTY" or "QUANTITY" or "PRICE" or "PRICEOFFSET" or "AMT" or "PERCENTAGE" => FixValueType.Decimal,
			"UTCTIMESTAMP" or "TZTIMESTAMP" or "TIME"                                           => FixValueType.Timestamp,
			"UTCTIMEONLY" or "TZTIMEONLY"                                                       => FixValueType.Time,
			"UTCDATEONLY" or "UTCDATE" or "LOCALMKTDATE" or "DATE"                              => FixValueType.Date,
			"MONTHYEAR"                                                                         => FixValueType.MonthYear,
			"MULTIPLEVALUESTRING" or "MULTIPLECHARVALUE" or "MULTIPLESTRINGVALUE"               => FixValueType.Multiple,
			"DATA" or "XMLDATA"                                                                 => FixValueType.Data,
			_                                                                                   => FixValueType.Text,
		};
	}

	// The texts a load writes are the expression language's, one file a slot, so that what a
	// dictionary was turned into can be read and kept; a check that would not compile is written
	// before the refusal names it.
	static Action<string, string>? Emitter(string? directory)
	{
		if (directory is null)
			return null;

		Directory.CreateDirectory(directory);

		return (slot, text) => File.WriteAllText(Path.Combine(directory, slot + ".el"), text);
	}

	/// <summary>The data tag a length tag is paired with, or zero where it is not a length tag.</summary>
	internal int DataTag(int lengthTag)
	{
		return (int)_pairs.GetValueOrDefault((FixTag)lengthTag);
	}

	/// <summary>Whether a tag carries binary data — the version's, or one this consumer declared.</summary>
	internal bool IsData(int tag)
	{
		return Kind(tag) < 0;
	}

	/// <summary>
	/// What the reader knows of every tag, one byte a tag: the type its value is read as, and whether
	/// it is the length or the data of a pair. Immutable: a context changed is a context with another.
	/// </summary>
	internal sealed class Codes
	{
		// The low bits are the type a standard or a dictionary gives the tag, and a pair's half is a
		// bit of its own, so that pairing a tag again leaves its type as it was.
		const byte TypeMask = 0x0F, Length = 0x10, Data = 0x20;

		// The table covers every tag anyone writes, the bilateral range ending at 39,999; a tag past it
		// is looked up in Wide.
		const int Tabled = 65536;

		readonly byte[]               _table;
		readonly Dictionary<int,byte> _wide;

		Codes(byte[] table, Dictionary<int,byte> wide)
		{
			_table = table;
			_wide  = wide;
		}

		public static Codes Of(Dictionary<FixTag,FixTag> pairs, Func<FixTag, FixValueType> types, FixTag last)
		{
			var table = new byte[(int)last + 1];

			for (var tag = 1; tag < table.Length; tag++)
				table[tag] = (byte)types((FixTag)tag);

			return new Codes(table, []).Paired(pairs);
		}

		byte Code(int tag)
		{
			if ((uint)tag < (uint)_table.Length)
				return _table[tag];

			return _wide.Count != 0 && _wide.TryGetValue(tag, out var code) ? code : (byte)0;
		}

		/// <summary>1 a length, -1 the data a length measures, 0 an ordinary value.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Kind(int tag)
		{
			var code = Code(tag);

			return (code & Length) != 0 ? 1 : (code & Data) != 0 ? -1 : 0;
		}

		/// <summary>The type the tag's value is read as; a pair's half no type is given is read as that half.</summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public FixValueType Type(int tag)
		{
			var code = Code(tag);
			var type = (FixValueType)(code & TypeMask);

			if (type != FixValueType.None)
				return type;

			return (code & Length) != 0 ? FixValueType.Integer : (code & Data) != 0 ? FixValueType.Data : FixValueType.None;
		}

		/// <summary>These codes with every pair's halves those of <paramref name="pairs"/>, and the types as they were.</summary>
		public Codes Paired(Dictionary<FixTag,FixTag> pairs)
		{
			var codes = With(pairs.SelectMany(static pair => new[] { (int)pair.Key, (int)pair.Value }));

			for (var tag = 0; tag < codes._table.Length; tag++)
				codes._table[tag] &= TypeMask;

			foreach (var tag in codes._wide.Keys.ToArray())
				codes._wide[tag] &= TypeMask;

			foreach (var pair in pairs)
			{
				codes.Set((int)pair.Key,   (byte)(codes.Code((int)pair.Key)   | Length));
				codes.Set((int)pair.Value, (byte)(codes.Code((int)pair.Value) | Data));
			}

			return codes;
		}

		/// <summary>These codes with the tags given their types, and the pairs as they were.</summary>
		public Codes Typed(List<(int Tag, FixValueType Type)> types)
		{
			if (types.Count == 0)
				return this;

			var codes = With(types.Select(static type => type.Tag));

			foreach (var (tag, type) in types)
				codes.Set(tag, (byte)((codes.Code(tag) & ~TypeMask) | (byte)type));

			return codes;
		}

		// A copy, its table long enough for the tags about to be set.
		Codes With(IEnumerable<int> tags)
		{
			var length = _table.Length;

			foreach (var tag in tags)
				if (tag >= length && tag < Tabled)
					length = tag + 1;

			var table = new byte[length];

			_table.CopyTo(table, 0);

			return new Codes(table, new Dictionary<int,byte>(_wide));
		}

		void Set(int tag, byte code)
		{
			if ((uint)tag < (uint)_table.Length)
				_table[tag] = code;
			else if (tag > 0)
				_wide[tag] = code;
		}
	}
}
