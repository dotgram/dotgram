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
	// from them once. A class of their own so that they exist before Default, which is built from them.
	static class Standard
	{
		public static readonly Dictionary<int,int> Pairs = new()
		{
			{  93,  89 }, {  90,  91 }, {  95,  96 }, { 212, 213 },
			{ 348, 349 }, { 350, 351 }, { 352, 353 }, { 354, 355 },
			{ 356, 357 }, { 358, 359 }, { 360, 361 }, { 362, 363 },
			{ 364, 365 }, { 445, 446 }, { 618, 619 }, { 621, 622 },
		};

		public static readonly sbyte[] Kinds = KindsOf(Pairs);
	}

	// What the reader asks of a tag: 1 is a length, -1 the data it measures, 0 an ordinary value.
	const sbyte Ordinary = 0, Length = 1, Data = -1;

	// The table covers every tag anyone writes, the bilateral range ending at 39,999; a tag past it
	// is looked up in the pairs themselves.
	const int Tabled = 65536;

	static sbyte[] KindsOf(Dictionary<int,int> pairs)
	{
		var kinds = new sbyte[Tabled];

		foreach (var pair in pairs)
		{
			if (pair.Key < Tabled)
				kinds[pair.Key] = Length;

			if (pair.Value < Tabled)
				kinds[pair.Value] = Data;
		}

		return kinds;
	}

	/// <summary>How the input separates one field from the next: the wire's SOH, or a log's pipe.</summary>
	/// <exception cref="ArgumentOutOfRangeException">Neither of the two.</exception>
	public FixFraming Framing { get; init; }

	/// <summary>Builds the field of a tag FIX 4.4 does not define: <c>(tag, value) =&gt; tag == 25005 ? new Status(FixConvert.ToText(value)) : null</c>.</summary>
	/// <remarks>
	/// Asked only of a tag the package has no class for, so a standard tag pays nothing for it. It is
	/// handed the tag and the value, and builds a <see cref="FixCustomField{T}"/>, or a class derived from
	/// one, the way the standard's fields are built; where it answers null, or there is none, the field is
	/// a <see cref="FixField.Invalid"/> of that tag.
	/// </remarks>
	public FixFieldFactory? FixFieldFactory { get; init; }

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

	readonly Dictionary<int,int> _pairs = Standard.Pairs;
	readonly sbyte[]             _kinds = Standard.Kinds;

	/// <summary>The length/data pairs, length tag to data tag: the standard's sixteen and a consumer's own.</summary>
	/// <remarks>What is set is added to the standard's pairs; a length tag set again replaces its pair.</remarks>
	public IReadOnlyDictionary<int,int> LengthDataPairs
	{
		get => _pairs;
		init
		{
			var pairs = new Dictionary<int,int>(Standard.Pairs);

			foreach (var pair in value)
				pairs[pair.Key] = pair.Value;

			_pairs = pairs;
			_kinds = KindsOf(pairs);
		}
	}

	/// <summary>
	/// What the reader does with a tag: 1 a length, -1 the data a length measures, 0 an ordinary value.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int Kind(int tag)
	{
		if ((uint)tag < Tabled)
			return _kinds[tag];

		return _pairs.ContainsKey(tag) ? Length : _pairs.ContainsValue(tag) ? Data : Ordinary;
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

	// A dictionary's checks go over this context's. The fields it describes that the standard does not
	// are to be built by a factory compiled through the expression language, which cannot yet hand a
	// span to a method (the architect's task to expr, 2026-09-23).
	FixContext Loaded(FixDictionary dictionary, string? emitTo)
	{
		return this with { Validators = Validators.Load(dictionary, Emitter(emitTo)) };
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
		return _pairs.GetValueOrDefault(lengthTag, 0);
	}

	/// <summary>Whether a tag carries binary data — the standard's, or one this consumer declared.</summary>
	internal bool IsData(int tag)
	{
		return Kind(tag) == Data;
	}
}
