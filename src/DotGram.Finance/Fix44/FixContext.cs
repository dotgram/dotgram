using System;
using System.Runtime.CompilerServices;

namespace DotGram.Finance.Fix44;

/// <summary>What a reading is done by: the framing, the length/data pairs, the fields a consumer builds, and the schema a message is held to.</summary>
/// <remarks>
/// <para>
/// One value, passed to every call that reads and to <see cref="FixMessage.Validate"/>, rather
/// than read from anywhere: a process reading two counterparties with two dictionaries is
/// expressible, each pass over an input has its own, and nothing about one reaches the other.
/// Immutable, and changed with <c>with</c>: <c>FixContext.Default with { Framing = FixFraming.Log }</c>.
/// </para>
/// <para>
/// FIX carries binary values as a pair of fields: a length, then the tag whose value is that many
/// octets and may hold the separator itself. Which tags are such a pair is the one thing a parse has
/// to know before it reads a value, and the standard settles it for sixteen pairs. A counterparty
/// may define more, in the bilateral range, and <see cref="LengthDataPairs"/> is where. The answer
/// is worked out once, when the value is made, into a table the tag indexes — so the reader's
/// question on every field is an array read, and a consumer's dictionary is never consulted in
/// the middle of a parse.
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
	/// <summary>FIX 4.4 as this package compiles it in, read from the wire by the standard's own pairs.</summary>
	public static FixContext Default { get; } = new();

	/// <summary>The same, reading a lossless pipe rendering: bare <c>|</c>, spaced <c> | </c>, or a mixture.</summary>
	/// <remarks>
	/// Framing is a value here rather than a second name for every method. Which separator an input
	/// uses is a property of the input, not of the caller's wish, and a property of the input belongs
	/// in the value that describes the input.
	/// </remarks>
	public static FixContext WithLogFraming => Default with { Framing = FixFraming.Log };

	// What the reader asks of a tag. The values are the grammar's own, so that the guard is a read
	// and not a translation: 1 begins a length/data pair, -1 is the data half standing where a
	// length should have been (which is no field at all), 0 is an ordinary value read to the
	// separator. A tag nobody has declared is ordinary, which is why zero means that.
	const sbyte Ordinary = 0, Length = 1, Data = -1;

	// Past this, a tag goes in the dictionary rather than the table. It covers every tag anyone
	// writes — the bilateral range ends at 39,999 — at 64 KB for the table, built once and shared
	// by every context that declares no pairs.
	const int Tabled = 65536;

	// The standard alone, which is what most contexts read by and none of them should pay for twice.
	// A slot filled the first time it is read rather than a field an initializer fills: `Default`
	// is itself a static of this type and would otherwise be built while this one was still null,
	// which is a bug that depends on the order two lines are written in.
	static class SettledHolder
	{
		public static readonly sbyte[] SettledData = Settled();

		static sbyte[] Settled()
		{
			var kinds = new sbyte[StandardTop];

			for (var tag = 1; tag < kinds.Length; tag++)
				kinds[tag] = StandardDataTag(tag) != 0 ? Length : StandardLengthTag(tag) != 0 ? Data : Ordinary;

			return kinds;
		}
	}

	static sbyte[] Standard => SettledHolder.SettledData;

	sbyte[]                 _kinds        = Standard;
	Dictionary<int, sbyte>? _far;
	Dictionary<int, int>?   _pairs;

	/// <summary>How the input separates one field from the next: the wire's SOH, or a log's pipe.</summary>
	/// <exception cref="ArgumentOutOfRangeException">Neither of the two.</exception>
	public FixFraming Framing { get; init; }

	/// <summary>Builds the field of a tag FIX 4.4 does not define: <c>tag =&gt; tag == 25005 ? new Status() : null</c>.</summary>
	/// <remarks>
	/// Asked only of a tag the package has no class for, so a standard tag pays nothing for it. What it
	/// builds is handed the value; where it answers null, or there is none, the field is a
	/// <see cref="FixField.Invalid"/> of that tag.
	/// </remarks>
	public Func<int, FixCustomField?>? FixFieldFactory { get; init; }

	/// <summary>Builds the message of a MsgType FIX 4.4 does not define: <c>type =&gt; type == "U1" ? new VenueQuote() : null</c>.</summary>
	/// <remarks>
	/// Asked only of a type the package has no class for. What it builds is handed the fields; where it
	/// answers null, or there is none, the message is a <see cref="FixMessage.Invalid"/>.
	/// </remarks>
	public Func<string, FixCustomMessage?>? FixMessageFactory { get; init; }

	/// <summary>A consumer's own length/data pairs, length tag to data tag; null where there are none.</summary>
	/// <remarks>
	/// Copied when set. The standard's sixteen pairs always hold and are added to, never replaced,
	/// so neither tag of a supplied pair may be one the standard defines.
	/// </remarks>
	/// <exception cref="ArgumentException">
	/// A tag is not positive, a pair names one tag twice, a data tag is declared twice, a data tag
	/// is also a length tag, or either tag is one the standard already defines.
	/// </exception>
	public IReadOnlyDictionary<int, int>? LengthDataPairs
	{
		get  => _pairs;
		init => Declare(value, out _kinds, out _far, out _pairs);
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

		return this with { Validators = Validators.Load(FixDictionary.Parse(dictionary), Emitter(emitTo)) };
	}

	/// <inheritdoc cref="Load(string, string)"/>
	/// <param name="dictionary">The dictionary's text; the reader is read and left open.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot; null writes nothing.</param>
	public FixContext Load(TextReader dictionary, string? emitTo = null)
	{
		if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

		return this with { Validators = Validators.Load(FixDictionary.Read(dictionary), Emitter(emitTo)) };
	}

	/// <inheritdoc cref="Load(string, string)"/>
	/// <param name="dictionary">The dictionary's octets; the stream is read and left open.</param>
	/// <param name="emitTo">A directory to write each check into before it is compiled, one file a slot; null writes nothing.</param>
	public FixContext Load(Stream dictionary, string? emitTo = null)
	{
		if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));

		return this with { Validators = Validators.Load(FixDictionary.Read(dictionary), Emitter(emitTo)) };
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

		return this with { Validators = Validators.Load(FixDictionary.Over(read), Emitter(emitTo)) };
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

	/// <summary>What the reader does with a tag: 1 a length/data pair, -1 no field, 0 an ordinary value.</summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int Kind(int tag)
	{
		var kinds = _kinds;

		if ((uint)tag < (uint)kinds.Length)
			return kinds[tag];

		return _far is not null && _far.TryGetValue(tag, out var kind) ? kind : Ordinary;
	}

	/// <summary>The data tag a length tag is paired with, or zero where it is not a length tag.</summary>
	/// <remarks>
	/// Asked once a field is already known to begin a pair, which is sixteen tags plus a consumer's
	/// and not every field — so this may be a lookup where <see cref="Kind"/> may not.
	/// </remarks>
	internal int DataTag(int lengthTag)
	{
		var standard = StandardDataTag(lengthTag);

		if (standard != 0)
			return standard;

		return _pairs is not null && _pairs.TryGetValue(lengthTag, out var data) ? data : 0;
	}

	/// <summary>The length tag a data tag is measured by, or zero where it is not a data tag.</summary>
	/// <remarks>
	/// The standard's sixteen answer from the table. A consumer's pair is held the other way round,
	/// length to data, because that is the direction the reader asks in; this walks it, which is a
	/// handful of entries and is asked once per data field of a message rather than once per field.
	/// </remarks>
	internal int LengthTag(int dataTag)
	{
		var standard = StandardLengthTag(dataTag);

		if (standard != 0 || _pairs is null)
			return standard;

		foreach (var pair in _pairs)
			if (pair.Value == dataTag)
				return pair.Key;

		return 0;
	}

	/// <summary>Whether a tag carries binary data — the standard's, or one this consumer declared.</summary>
	internal bool IsData(int tag)
	{
		return Kind(tag) == Data;
	}

	// The pairs worked out into the table the reader indexes, once, here.
	static void Declare(IReadOnlyDictionary<int, int>? lengthDataPairs, out sbyte[] kinds, out Dictionary<int, sbyte>? far, out Dictionary<int, int>? pairs)
	{
		kinds = Standard;
		far   = null;
		pairs = null;

		if (lengthDataPairs is null || lengthDataPairs.Count == 0)
			return;

		var copied = new Dictionary<int, int>(lengthDataPairs.Count);
		var data   = new HashSet<int>();
		var top    = Standard.Length;

		foreach (var pair in lengthDataPairs)
		{
			if (pair.Key <= 0 || pair.Value <= 0 || pair.Key == pair.Value)
				throw new ArgumentException(
					"Pairs require positive, distinct length and data tags.", nameof(LengthDataPairs));

			// The standard's meaning for a tag stands. A pair that contradicts it could only be
			// ignored, and a caller who believes something that is not true is worse served by
			// silence than by this.
			if (Defines(pair.Key) || Defines(pair.Value))
				throw new ArgumentException(
					$"Tag {(Defines(pair.Key) ? pair.Key : pair.Value)} is one the standard defines; " +
					"the standard's pairs are added to, not replaced.", nameof(LengthDataPairs));

			if (!data.Add(pair.Value) || !copied.TryAdd(pair.Key, pair.Value))
				throw new ArgumentException(
					"Pairs require unique length tags and unique data tags.", nameof(LengthDataPairs));

			top = Math.Max(top, Room(pair.Key));
			top = Math.Max(top, Room(pair.Value));
		}

		foreach (var tag in data)
			if (copied.ContainsKey(tag))
				throw new ArgumentException(
					"A data tag cannot also be a length tag.", nameof(LengthDataPairs));

		var table = new sbyte[top];

		// The standard goes in first and nothing overwrites it: the checks above have already
		// refused everything that could try.
		Array.Copy(Standard, table, Standard.Length);

		foreach (var pair in copied)
		{
			Place(table, ref far, pair.Key,   Length);
			Place(table, ref far, pair.Value, Data);
		}

		kinds = table;
		pairs = copied;

		static int Room(int tag)
		{
			return tag < Tabled ? tag + 1 : 0;
		}

		static void Place(sbyte[] kinds, ref Dictionary<int, sbyte>? far, int tag, sbyte kind)
		{
			if ((uint)tag < (uint)kinds.Length)
				kinds[tag] = kind;
			else
				(far ??= [])[tag] = kind;
		}
	}

	// Whether the standard defines a tag: whether the field it builds is the standard's own class and
	// not the one built for a tag nobody defined. Asked when a context is made, of a consumer's pairs.
	static bool Defines(int tag)
	{
		return FixFieldBuilder.Value(tag, "0".AsSpan(), null) is not FixField.Invalid;
	}

	// One past the largest tag of the sixteen pairs: every tag the reader's table answers other than
	// ordinary, which is what it holds for any tag past it.
	const int StandardTop = 623;

	// The sixteen length/data pairs of FIX 4.4, both ways.
	static int StandardLengthTag(int dataTag)
	{
		return dataTag switch
		{
			 89 =>  93,  91 =>  90,  96 =>  95, 213 => 212,
			349 => 348, 351 => 350, 353 => 352, 355 => 354,
			357 => 356, 359 => 358, 361 => 360, 363 => 362,
			365 => 364, 446 => 445, 619 => 618, 622 => 621,
			_   => 0,
		};
	}

	static int StandardDataTag(int lengthTag)
	{
		return lengthTag switch
		{
			 93 =>  89,  90 =>  91,  95 =>  96, 212 => 213,
			348 => 349, 350 => 351, 352 => 353, 354 => 355,
			356 => 357, 358 => 359, 360 => 361, 362 => 363,
			364 => 365, 445 => 446, 618 => 619, 621 => 622,
			_   => 0,
		};
	}
}
