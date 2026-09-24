using System;
using System.Runtime.CompilerServices;

namespace DotGram.Finance.Fix44;

/// <summary>What a reading is done by: the framing, the type of every field and the length/data pairs, and the schema a message is held to.</summary>
/// <remarks>
/// <para>
/// One value, passed to every call that reads and to <see cref="FixMessage.Validate"/>, rather
/// than read from anywhere: a process reading two counterparties with two dictionaries is
/// expressible, each pass over an input has its own, and nothing about one reaches the other.
/// Immutable, and changed with <c>with</c>: <c>FixContext.Default with { Framing = FixFraming.Log }</c>.
/// </para>
/// <para>
/// What a reading has to know of a tag is the type its value is read as, which is the class of the
/// field, and whether it is half of a pair. FIX carries binary values as a pair of fields: a length,
/// then the tag whose value is that many octets and may hold the separator itself. The standard
/// settles both for the tags it defines; a counterparty may define more, in the bilateral range,
/// and a dictionary given to <see cref="Load(string, string)"/> types them while
/// <see cref="LengthDataPairs"/> pairs them. Both are worked out once, when the value is made, into
/// a table the tag indexes — so the reader's question on every field is an array read, and nothing
/// a consumer supplies is consulted in the middle of a parse. A tag neither the standard nor a
/// loaded dictionary defines is read as a <see cref="FixField.Invalid"/>.
/// </para>
/// <para>
/// The schema is the check of each message type, of each block and of each field, held as one
/// object of slots so that a dictionary loaded at run time replaces the slots it describes and
/// leaves the rest. That object is not part of what a consumer sees: what replaces a check is a
/// dictionary, read from a file, and not a delegate written by hand.
/// </para>
/// </remarks>
public sealed record FixContext
{
	/// <summary>
	/// FIX 4.4 as this package compiles it in, read from the wire by the standard's own pairs.
	/// </summary>
	public static FixContext Default { get; } = new();

	/// <summary>The same, reading a lossless pipe rendering: bare <c>|</c>, spaced <c> | </c>, or a mixture.</summary>
	/// <remarks>
	/// Framing is a value here rather than a second name for every method. Which separator an input
	/// uses is a property of the input, not of the caller's wish, and a property of the input belongs
	/// in the value that describes the input.
	/// </remarks>
	public static FixContext WithLogFraming => Default with { Framing = FixFraming.Log };

	// The standard's sixteen pairs, length tag to data tag, and the table the reader indexes, built
	// from them and from the type of every field once. A class of their own so that they exist before
	// Default, which is built from them.
	static class Standard
	{
		public static readonly Dictionary<FixTag,FixTag> Pairs = new()
		{
			{ FixTag.SignatureLength,                  FixTag.Signature                     },
			{ FixTag.SecureDataLen,                    FixTag.SecureData                    },
			{ FixTag.RawDataLength,                    FixTag.RawData                       },
			{ FixTag.XmlDataLen,                       FixTag.XmlData                       },
			{ FixTag.EncodedIssuerLen,                 FixTag.EncodedIssuer                 },
			{ FixTag.EncodedSecurityDescLen,           FixTag.EncodedSecurityDesc           },
			{ FixTag.EncodedListExecInstLen,           FixTag.EncodedListExecInst           },
			{ FixTag.EncodedTextLen,                   FixTag.EncodedText                   },
			{ FixTag.EncodedSubjectLen,                FixTag.EncodedSubject                },
			{ FixTag.EncodedHeadlineLen,               FixTag.EncodedHeadline               },
			{ FixTag.EncodedAllocTextLen,              FixTag.EncodedAllocText              },
			{ FixTag.EncodedUnderlyingIssuerLen,       FixTag.EncodedUnderlyingIssuer       },
			{ FixTag.EncodedUnderlyingSecurityDescLen, FixTag.EncodedUnderlyingSecurityDesc },
			{ FixTag.EncodedListStatusTextLen,         FixTag.EncodedListStatusText         },
			{ FixTag.EncodedLegIssuerLen,              FixTag.EncodedLegIssuer              },
			{ FixTag.EncodedLegSecurityDescLen,        FixTag.EncodedLegSecurityDesc        },
		};

		public static readonly Codes Codes = Codes.Of(Pairs);
	}

	/// <summary>How the input separates one field from the next: the wire's SOH, or a log's pipe.</summary>
	/// <exception cref="ArgumentOutOfRangeException">Neither of the two.</exception>
	public FixFraming Framing { get; init; }

	/// <summary>Builds the message of a MsgType FIX 4.4 does not define: <c>type =&gt; type == "U1" ? new VenueQuote() : null</c>.</summary>
	/// <remarks>
	/// Asked only of a type the package has no class for. What it builds is handed the fields; where it
	/// answers null, or there is none, the message is a <see cref="FixMessage.Invalid"/>.
	/// </remarks>
	public Func<string,FixCustomMessage?>? FixMessageFactory { get; init; }

	/// <summary>The initial size of the buffer a reader or a stream is read through, in characters or bytes: 4096 unless given.</summary>
	/// <exception cref="ArgumentOutOfRangeException">Not positive.</exception>
	public int BufferSize
	{
		get;
		init => field = value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(BufferSize));
	} = 4096;

	/// <summary>
	/// The most characters or bytes one field read from a reader or a stream may take, from its tag
	/// through the separator that ends it, or a whole length/data pair: <see cref="FixParser.DefaultMaxRetained"/>
	/// unless given.
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

	readonly Dictionary<FixTag,FixTag> _pairs = Standard.Pairs;

	// What the reader knows of every tag; set by LengthDataPairs and by a load.
	Codes Coded { get; init; } = Standard.Codes;

	/// <summary>The length/data pairs, length tag to data tag: the standard's sixteen and a consumer's own.</summary>
	/// <remarks>
	/// What is set is added to the standard's pairs; a length tag set again replaces its pair. A tag of
	/// a pair that neither the standard nor a loaded dictionary gives a type is read as its half of the
	/// pair is: a length as a <see cref="FixField.Integer"/>, the data as a <see cref="FixField.Data"/>.
	/// </remarks>
	public IReadOnlyDictionary<FixTag,FixTag> LengthDataPairs
	{
		get => _pairs;
		init
		{
			var pairs = new Dictionary<FixTag,FixTag>(Standard.Pairs);

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

	/// <summary>The type a tag's value is read as: the standard's, a loaded dictionary's, or its half of a pair's.</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal FixValueType Type(int tag)
	{
		return Coded.Type(tag);
	}

	/// <summary>The check of each message type, of each block, and of each field.</summary>
	internal FixValidators Validators { get; init; } = FixValidators.Default;

	/// <summary>This context with a dictionary loaded over its schema.</summary>
	/// <param name="dictionary">A QuickFIX dictionary, or a fragment of one: the text of the file.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot, named for the slot with the extension <c>.el</c>; null writes nothing.</param>
	/// <returns>A new context, reading as this one does; this one is unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
	/// <exception cref="FormatException">The text is not a dictionary this package can read, or a check written from it names what the model does not have.</exception>
	/// <remarks>
	/// <para>
	/// Loading composes. The standard, then a venue's file, then a rule a test adds, each a context
	/// built from the one before: <c>FixContext.Default.Load(venue).Load(fragment)</c>. What a file
	/// does not mention it has no opinion about, so a fragment need say only what it adds.
	/// </para>
	/// <para>
	/// A message type, a component, a group's entries or a field the file describes has the file's
	/// whole check from then on, and what the file does not say of it is no longer asked.
	/// </para>
	/// <para>
	/// The file's names are the names of the checks: a message is the class of its name, a component
	/// the interface <c>I</c> and its name, a group the entries <c>&lt;Counter&gt;Groups</c> nested
	/// in what carries it. A file that places a field where the model has no property for it does
	/// not load; a correction read over it, in the same load, can put the place right before
	/// anything is written: <see cref="Load(IEnumerable{string}, string)"/>.
	/// </para>
	/// </remarks>
	public FixContext Load(string dictionary, string? emitTo = null)
	{
		if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

		return Loaded(FixDictionary.Parse(dictionary), emitTo);
	}

	/// <inheritdoc cref="Load(string, string)"/>
	/// <param name="dictionary">The dictionary's text; the reader is read and left open.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot; null writes nothing.</param>
	public FixContext Load(TextReader dictionary, string? emitTo = null)
	{
		if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

		return Loaded(FixDictionary.Read(dictionary), emitTo);
	}

	/// <inheritdoc cref="Load(string, string)"/>
	/// <param name="dictionary">The dictionary's octets; the stream is read and left open.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot; null writes nothing.</param>
	public FixContext Load(Stream dictionary, string? emitTo = null)
	{
		if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

		return Loaded(FixDictionary.Read(dictionary), emitTo);
	}

	/// <summary>This context with several dictionaries loaded over its schema as one: each read over the one before.</summary>
	/// <param name="dictionaries">The texts of the files, in order: a later file's message type, component or field replaces an earlier file's.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot; null writes nothing.</param>
	/// <returns>A new context, reading as this one does; this one is unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="dictionaries"/> is null, or one of them is.</exception>
	/// <exception cref="FormatException">A text is not a dictionary this package can read, or a check written from them names what the model does not have.</exception>
	/// <remarks>
	/// What a correction to somebody else's file is for. A file and its correction loaded one after
	/// the other would have the file's checks written first, and a place the file has wrong refuses
	/// to compile before the correction is read; read as one, the correction's description of a type
	/// is the one written. <c>FixContext.Default.Load([theirs, corrections])</c>.
	/// </remarks>
	public FixContext Load(IEnumerable<string> dictionaries, string? emitTo = null)
	{
		if (dictionaries == null) throw new ArgumentNullException(nameof(dictionaries));

		var read = new List<FixDictionary>();

		foreach (var dictionary in dictionaries)
			read.Add(FixDictionary.Parse(dictionary ?? throw new ArgumentNullException(nameof(dictionaries))));

		return Loaded(FixDictionary.Over(read), emitTo);
	}

	/// <inheritdoc cref="Load(string, string)"/>
	/// <param name="fileName">The path of the dictionary's file.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot; null writes nothing.</param>
	/// <exception cref="ArgumentNullException"><paramref name="fileName"/> is null.</exception>
	public FixContext LoadFile(string fileName, string? emitTo = null)
	{
		if (fileName == null) throw new ArgumentNullException(nameof(fileName));

		using var stream = File.OpenRead(fileName);

		return Load(stream, emitTo);
	}

	// A dictionary's checks go over this context's, and the fields it describes that the standard does
	// not are read, from then on, as the types it gives them.
	FixContext Loaded(FixDictionary dictionary, string? emitTo)
	{
		var types = new List<(int Tag, FixValueType Type)>();

		foreach (var field in dictionary.Fields)
			if (FixFieldBuilder.Standard((FixTag)field.Key) == FixValueType.None && TypeOf(field.Value.Type) is { } type)
				types.Add((field.Key, type));

		return this with
		{
			Validators = Validators.Load(dictionary, Emitter(emitTo)),
			Coded      = Coded.Typed(types),
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

	/// <summary>Whether a tag carries binary data — the standard's, or one this consumer declared.</summary>
	internal bool IsData(int tag)
	{
		return Kind(tag) < 0;
	}

	/// <summary>
	/// What the reader knows of every tag, one byte a tag: the type its value is read as, and whether
	/// it is the length or the data of a pair. Immutable: a context changed is a context with another.
	/// </summary>
	sealed class Codes
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

		public static Codes Of(Dictionary<FixTag,FixTag> pairs)
		{
			var table = new byte[(int)FixTag.LegInterestAccrualDate + 1];

			for (var tag = 1; tag < table.Length; tag++)
				table[tag] = (byte)FixFieldBuilder.Standard((FixTag)tag);

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
