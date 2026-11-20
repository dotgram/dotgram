using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>The JSON writer on its own: every kind, every class of character, the row's state between rows.</summary>
public sealed class JsonWriterTests
{
	/// <summary>One array row of the values <paramref name="writes"/> write.</summary>
	static string Row(params Func<QlJsonState, QlJsonState>[] writes)
	{
		var state = QlJson.Start(1, null, null);

		foreach (var write in writes)
			state = write(state);

		return QlJson.End(state);
	}

	static string Text(string value)
	{
		return Row(s => QlJson.String(s, value));
	}

	[Fact]
	public void EveryKindAndItsNull()
	{
		var row = Row(
			s => QlJson.Bool(s, true),
			s => QlJson.Bool(s, (bool?)false),
			s => QlJson.Bool(s, (bool?)null),
			s => QlJson.Int(s, -42),
			s => QlJson.Int(s, (int?)int.MinValue),
			s => QlJson.Int(s, (int?)null),
			s => QlJson.Long(s, 9007199254740993L),
			s => QlJson.Long(s, (long?)long.MinValue),
			s => QlJson.Long(s, (long?)null),
			s => QlJson.Decimal(s, 0.10m),
			s => QlJson.Decimal(s, (decimal?)-1250.50m),
			s => QlJson.Decimal(s, (decimal?)null),
			s => QlJson.Double(s, 1.5),
			s => QlJson.Double(s, (double?)-0.25),
			s => QlJson.Double(s, (double?)null),
			s => QlJson.String(s, "x"),
			s => QlJson.String(s, null),
			s => QlJson.Timestamp(s, new DateTime(2026, 11, 1, 9, 30, 0)),
			s => QlJson.Timestamp(s, (DateTime?)new DateTime(2026, 11, 1, 9, 30, 0, DateTimeKind.Utc)),
			s => QlJson.Timestamp(s, (DateTime?)null),
			s => QlJson.TimestampTz(s, new DateTimeOffset(2026, 11, 1, 9, 30, 0, TimeSpan.FromHours(3))),
			s => QlJson.TimestampTz(s, (DateTimeOffset?)new DateTimeOffset(2026, 11, 1, 9, 30, 0, TimeSpan.Zero)),
			s => QlJson.TimestampTz(s, (DateTimeOffset?)null),
			s => QlJson.Guid(s, new Guid("0f8fad5b-d9cb-469f-a165-70867728950e")),
			s => QlJson.Guid(s, (Guid?)System.Guid.Empty),
			s => QlJson.Guid(s, (Guid?)null));

		Assert.Equal(
			"[true,false,null,-42,-2147483648,null,9007199254740993,-9223372036854775808,null,0.10,-1250.50,null,1.5,-0.25,null,\"x\",null," +
			"\"2026-11-01T09:30:00.0000000\",\"2026-11-01T09:30:00.0000000Z\",null," +
			"\"2026-11-01T09:30:00.0000000+03:00\",\"2026-11-01T09:30:00.0000000+00:00\",null," +
			"\"0f8fad5b-d9cb-469f-a165-70867728950e\",\"00000000-0000-0000-0000-000000000000\",null]",
			row);

		using var document = JsonDocument.Parse(row);

		Assert.Equal(26, document.RootElement.GetArrayLength());
	}

	[Fact]
	public void ADoubleReadsBackAndWhatJsonHasNoNumberForIsAString()
	{
		Assert.Equal("[\"NaN\",\"Infinity\",\"-Infinity\"]", Row(s => QlJson.Double(s, double.NaN), s => QlJson.Double(s, double.PositiveInfinity), s => QlJson.Double(s, (double?)double.NegativeInfinity)));

		foreach (var value in new[] { 0.1, 1.0 / 3, 1e300, -5e-324, double.MaxValue, 123456789.125 })
		{
			var row = Row(s => QlJson.Double(s, value));

			using var document = JsonDocument.Parse(row);

			Assert.Equal(value, document.RootElement[0].GetDouble());
		}

		// The shortest text that reads back where the runtime has it; seventeen digits from the netstandard2.0 build,
		// which a net8.0 consumer of the package takes.

#if NET10_0_OR_GREATER
		Assert.Equal("[0.1]", Row(s => QlJson.Double(s, 0.1)));
#else
		Assert.Equal("[0.10000000000000001]", Row(s => QlJson.Double(s, 0.1)));
#endif
	}

	[Fact]
	public void NumbersAreCultureFree()
	{
		var culture = CultureInfo.CurrentCulture;

		try
		{
			CultureInfo.CurrentCulture = new CultureInfo("de-DE");

			Assert.Equal("[1.5,0.25,-7,\"2026-11-01T09:30:00.0000000\"]", Row(s => QlJson.Decimal(s, 1.5m), s => QlJson.Double(s, 0.25), s => QlJson.Int(s, -7), s => QlJson.Timestamp(s, new DateTime(2026, 11, 1, 9, 30, 0))));
		}
		finally
		{
			CultureInfo.CurrentCulture = culture;
		}
	}

	[Theory]
	[InlineData("\"", "\\\"")]
	[InlineData("\\", "\\\\")]
	[InlineData("\b", "\\b")]
	[InlineData("\f", "\\f")]
	[InlineData("\n", "\\n")]
	[InlineData("\r", "\\r")]
	[InlineData("\t", "\\t")]
	[InlineData("\u0000", "\\u0000")]
	[InlineData("\u0001", "\\u0001")]
	[InlineData("\u001F", "\\u001F")]
	[InlineData("<", "\\u003C")]
	[InlineData(">", "\\u003E")]
	[InlineData("&", "\\u0026")]
	[InlineData("'", "\\u0027")]
	[InlineData("/", "/")]
	[InlineData("\u007F", "\u007F")]
	[InlineData("é", "é")]
	[InlineData("Пётр", "Пётр")]
	public void EachClassOfCharacterIsWrittenItsWay(string value, string written)
	{
		Assert.Equal("[\"a" + written + "z\"]", Text("a" + value + "z"));
	}

	[Fact]
	public void TheSeparatorsJavaScriptBreaksLinesAtAreEscaped()
	{
		Assert.Equal("[\"\\u2028\\u2029\"]", Text(new string(new[] { (char)0x2028, (char)0x2029 })));
	}

	[Fact]
	public void ASurrogateWithoutItsPairIsEscapedAndAPairIsNot()
	{
		var high = (char)0xD83D;
		var low  = (char)0xDE00;

		Assert.Equal("[\"" + high + low + "\"]",     Text(new string(new[] { high, low })));
		Assert.Equal("[\"\\uD83Dx\"]",               Text(new string(new[] { high, 'x' })));
		Assert.Equal("[\"x\\uD83D\"]",               Text(new string(new[] { 'x', high })));
		Assert.Equal("[\"\\uDE00\\uD83D\"]",         Text(new string(new[] { low, high })));
		Assert.Equal("[\"\\uDE00\"]",                Text(new string(new[] { low })));
		Assert.Equal("[\"\\uD83D" + high + low + "\"]", Text(new string(new[] { high, high, low })));
	}

	[Fact]
	public void TheTextOfEveryCharacterReadsBack()
	{
		var all = new string(Enumerable.Range(0, 0x10000).Select(i => (char)i).Where(c => !char.IsSurrogate(c)).ToArray());

		using var document = JsonDocument.Parse(Text(all));

		Assert.Equal(all, document.RootElement[0].GetString());
	}

	[Fact]
	public void AnObjectNamesEachValue()
	{
		var state = QlJson.Start(1, ["\"id\":", "\"name\":"], null);

		state = QlJson.Int(state, 1);
		state = QlJson.String(state, null);

		Assert.Equal("{\"id\":1,\"name\":null}", QlJson.End(state));
		Assert.Equal("\"a\\\"b\"", QlJson.Quote("a\"b"));
	}

	[Fact]
	public void TheHostFormatsItsTypesInColumnOrder()
	{
		Func<DateTime, string?> day   = d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
		Func<byte[], string?>   bytes = b => Convert.ToBase64String(b);
		Func<int, string?>      none  = _ => null;

		var state = QlJson.Start(1, null, [day, day, bytes, bytes, none]);

		state = QlJson.FormattedNullable(state, (DateTime?)null);
		state = QlJson.Formatted(state, new DateTime(2026, 11, 1));
		state = QlJson.Formatted(state, (byte[]?)null);
		state = QlJson.Formatted(state, new byte[] { 1, 2, 3 });
		state = QlJson.Formatted(state, 7);

		Assert.Equal("[null,\"2026-11-01\",null,\"AQID\",null]", QlJson.End(state));
	}

	readonly struct Stamp(int value) : IFormattable
	{
		public string ToString(string? format, IFormatProvider? provider)
		{
			return "<" + value.ToString(provider) + ">";
		}
	}

	[Fact]
	public void ATypeWithoutAFormatPrintsItself()
	{
		var row = Row(
			s => QlJson.Formattable(s, new DateOnly(2026, 1, 2)),
			s => QlJson.FormattableNullable(s, (DateOnly?)null),
			s => QlJson.Formattable(s, Kind.Shipped),
			s => QlJson.Formattable(s, new Stamp(5)),
			s => QlJson.Printed(s, new byte[] { 1 }),
			s => QlJson.Printed(s, (byte[]?)null),
			s => QlJson.PrintedNullable(s, (TimeSpan?)TimeSpan.FromMinutes(90)));

		Assert.Equal("[\"01/02/2026\",null,\"Shipped\",\"\\u003C5\\u003E\",\"System.Byte[]\",null,\"01:30:00\"]", row);
	}

	[Fact]
	public void ARowAfterAFailedOneStartsClean()
	{
		Func<int, string?> fail = v => v == 2 ? throw new InvalidOperationException("no") : "v" + v;

		Assert.Throws<InvalidOperationException>(() =>
		{
			var state = QlJson.Start(1, ["\"a\":", "\"b\":"], [fail]);

			state = QlJson.Int(state, 1);

			QlJson.Formatted(state, 2);
		});

		Assert.Equal("[3]", Row(s => QlJson.Int(s, 3)));
	}

	[Fact]
	public void AFormatThatRunsAQueryOfItsOwnDoesNotTouchTheRow()
	{
		Func<int, string?> nested = v => Row(s => QlJson.Int(s, v * 10), s => QlJson.String(s, "in"));

		var state = QlJson.Start(1, ["\"a\":", "\"b\":", "\"c\":"], [nested]);

		state = QlJson.Int(state, 1);
		state = QlJson.Formatted(state, 2);
		state = QlJson.Int(state, 3);

		Assert.Equal("{\"a\":1,\"b\":\"[20,\\\"in\\\"]\",\"c\":3}", QlJson.End(state));
	}

	[Fact]
	public void AWideRowGrowsTheBufferAndLetsItGo()
	{
		var wide = new string('x', QlJson.KeptBuffer);

		Assert.Equal("[\"" + wide + "\",1]", Row(s => QlJson.String(s, wide), s => QlJson.Int(s, 1)));
		Assert.Equal("[2]", Row(s => QlJson.Int(s, 2)));
	}

#if NET10_0_OR_GREATER
	/// <summary>Bytes allocated on this thread by <paramref name="action"/>, after a run that warms it up.</summary>
	static long Allocated(Action action)
	{
		action();

		var before = GC.GetAllocatedBytesForCurrentThread();

		action();

		return GC.GetAllocatedBytesForCurrentThread() - before;
	}

	static long StringOf(int length)
	{
		string? made = null;

		return Allocated(() => made = new string(' ', length));
	}

	public static TheoryData<string> Kinds()
	{
		return ["Bool", "Int", "Long", "Decimal", "Double", "NaN", "String", "Escaped", "Timestamp", "TimestampTz", "Guid", "Null", "Formatted", "Formattable", "Object"];
	}

	static readonly string[] ObjectNames = ["\"a\":", "\"b\":"];

	static readonly Func<DateTime, string?> Fixed = _ => "fixed";

	static readonly Delegate[] FixedFormats = [Fixed];

	/// <summary>A row of the kind, written the way the provider's materializer calls the writer: no closures, no boxes.</summary>
	static string Write(string kind)
	{
		var s = QlJson.Start(7, kind == "Object" ? ObjectNames : null, kind == "Formatted" ? FixedFormats : null);

		s = kind switch
		{
			"Bool"        => QlJson.Bool(QlJson.Bool(s, true), (bool?)false),
			"Int"         => QlJson.Int(QlJson.Int(s, 123456), (int?)-7),
			"Long"        => QlJson.Long(QlJson.Long(s, long.MaxValue), (long?)-1L),
			"Decimal"     => QlJson.Decimal(QlJson.Decimal(s, 1250.50m), (decimal?)-0.001m),
			"Double"      => QlJson.Double(QlJson.Double(s, 1.0 / 3), (double?)1e-300),
			"NaN"         => QlJson.Double(s, double.NaN),
			"String"      => QlJson.String(s, "plain text"),
			"Escaped"     => QlJson.String(s, "a\"b\\c\n<d>"),
			"Timestamp"   => QlJson.Timestamp(QlJson.Timestamp(s, new DateTime(2026, 11, 1, 9, 30, 0)), (DateTime?)DateTime.MaxValue),
			"TimestampTz" => QlJson.TimestampTz(s, (DateTimeOffset?)new DateTimeOffset(2026, 11, 1, 9, 30, 0, TimeSpan.FromHours(-5))),
			"Guid"        => QlJson.Guid(QlJson.Guid(s, System.Guid.Empty), (Guid?)System.Guid.Empty),
			"Null"        => QlJson.Guid(QlJson.Int(QlJson.String(s, null), (int?)null), (Guid?)null),
			"Formatted"   => QlJson.Formatted(s, new DateTime(2026, 1, 1)),
			"Formattable" => QlJson.Formattable(s, new Stamp(1)),
			"Object"      => QlJson.Long(QlJson.Int(s, 1), 2L),
			_             => throw new ArgumentOutOfRangeException(nameof(kind)),
		};

		return QlJson.End(s);
	}

	/// <summary>A row allocates its text and nothing else, whatever its kinds; a host's format, its own string besides.</summary>
	[Theory]
	[MemberData(nameof(Kinds))]
	public void ARowAllocatesItsTextAlone(string kind)
	{
		var text  = "";
		var bytes = Allocated(() => text = Write(kind));

		// Stamp prints through string concatenation and int formatting: what it allocates is its own.
		var printed = kind == "Formattable" ? Allocated(() => new Stamp(1).ToString(null, CultureInfo.InvariantCulture)) : 0;

		Assert.Equal(StringOf(text.Length) + printed, bytes);
	}
#endif
}
