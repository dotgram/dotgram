using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>What a value passed with a text may be, cell by cell, and what the call says when it may not.</summary>
public sealed class CoercionTests
{
	static object? Convert(object value, QlType type, Type? other = null)
	{
		var target = type == QlType.Other ? ScalarType.Of(other!) : ScalarType.Of(type, false);

		return Coercion.TryConvert(value, target, out var result) ? result : Refused.Value;
	}

	/// <summary>What <see cref="Convert"/> answers for a value that does not convert.</summary>
	sealed class Refused
	{
		public static readonly Refused Value = new();
	}

	public static IEnumerable<TheoryDataRow<object, QlType, object?>> Cells()
	{
		var refused = Refused.Value;

		// bool: a bool and nothing else.
		yield return new(true,                    QlType.Bool, true);
		yield return new(1,                       QlType.Bool, refused);
		yield return new("true",                  QlType.Bool, refused);

		// int: every integer type in range, an integral decimal whatever its scale, an integral double up to 2^53.
		yield return new((sbyte)-1,               QlType.Int, -1);
		yield return new((byte)1,                 QlType.Int, 1);
		yield return new((short)-2,               QlType.Int, -2);
		yield return new((ushort)2,               QlType.Int, 2);
		yield return new(3,                       QlType.Int, 3);
		yield return new(3u,                      QlType.Int, 3);
		yield return new(4L,                      QlType.Int, 4);
		yield return new(4UL,                     QlType.Int, 4);
		yield return new(5.00m,                   QlType.Int, 5);
		yield return new(6.0,                     QlType.Int, 6);
		yield return new(5.5m,                    QlType.Int, refused);
		yield return new(6.5,                     QlType.Int, refused);
		yield return new(int.MaxValue + 1L,       QlType.Int, refused);
		yield return new("1",                     QlType.Int, refused);
		yield return new(true,                    QlType.Int, refused);
		yield return new('1',                     QlType.Int, refused);
		yield return new(Kind.Paid,               QlType.Int, refused);
		yield return new(1f,                      QlType.Int, refused);

		// long: the same, wider.
		yield return new(long.MaxValue,           QlType.Long, long.MaxValue);
		yield return new(9007199254740992.0,      QlType.Long, 9007199254740992L);
		yield return new(1e16,                    QlType.Long, refused);
		yield return new(ulong.MaxValue,          QlType.Long, refused);
		yield return new(79228162514264337593543950335m, QlType.Long, refused);

		// decimal: any integer, a decimal, a double by its shortest text.
		yield return new(7,                       QlType.Decimal, 7m);
		yield return new(ulong.MaxValue,          QlType.Decimal, 18446744073709551615m);
		yield return new(7.25m,                   QlType.Decimal, 7.25m);
		yield return new(0.1,                     QlType.Decimal, 0.1m);
		yield return new(0.3,                     QlType.Decimal, 0.3m);
		yield return new(1e-7,                    QlType.Decimal, 0.0000001m);
		yield return new(123456789.123,           QlType.Decimal, 123456789.123m);
		yield return new(double.NaN,              QlType.Decimal, refused);
		yield return new(double.PositiveInfinity, QlType.Decimal, refused);
		yield return new(1e300,                   QlType.Decimal, refused);
		yield return new("7",                     QlType.Decimal, refused);

		// double: an integer up to 2^53, a decimal, a finite double.
		yield return new(8,                       QlType.Double, 8.0);
		yield return new(9007199254740992L,       QlType.Double, 9007199254740992.0);
		yield return new(9007199254740993L,       QlType.Double, refused);
		yield return new(ulong.MaxValue,          QlType.Double, refused);
		yield return new(8.5m,                    QlType.Double, 8.5);
		yield return new(8.25,                    QlType.Double, 8.25);
		yield return new(double.NaN,              QlType.Double, refused);
		yield return new(double.NegativeInfinity, QlType.Double, refused);
		yield return new("8",                     QlType.Double, refused);

		// string: a string.
		yield return new("x",                     QlType.String, "x");
		yield return new(1,                       QlType.String, refused);
		yield return new('x',                     QlType.String, refused);

		// timestamp: a DateTime; ISO 8601 without an offset (unspecified) or with Z (UTC); an offset is refused.
		var local = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Local);

		yield return new(local,                         QlType.Timestamp, local);
		yield return new("2026-01-02T03:04:05",         QlType.Timestamp, new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Unspecified));
		yield return new("2026-01-02 03:04:05.25",      QlType.Timestamp, new DateTime(2026, 1, 2, 3, 4, 5, 250, DateTimeKind.Unspecified));
		yield return new("2026-01-02",                  QlType.Timestamp, new DateTime(2026, 1, 2));
		yield return new("2026-01-02T03:04:05Z",        QlType.Timestamp, new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc));
		yield return new("2026-01-02T03:04:05+02:00",   QlType.Timestamp, refused);
		yield return new("02.01.2026",                  QlType.Timestamp, refused);
		yield return new(new DateTimeOffset(local),     QlType.Timestamp, refused);

		// timestamptz: a DateTimeOffset, a UTC DateTime, ISO 8601 with an offset or Z.
		var zoned = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.FromHours(2));
		var utc   = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);

		yield return new(zoned,                         QlType.TimestampTz, zoned);
		yield return new(utc,                           QlType.TimestampTz, new DateTimeOffset(utc));
		yield return new(local,                         QlType.TimestampTz, refused);
		yield return new("2026-01-02T03:04:05+02:00",   QlType.TimestampTz, zoned);
		yield return new("2026-01-02T03:04:05Z",        QlType.TimestampTz, new DateTimeOffset(utc));
		yield return new("2026-01-02T03:04:05",         QlType.TimestampTz, refused);

		// guid: a Guid, or a string in the "D" format only.
		var guid = new Guid(1, 2, 3, [4, 5, 6, 7, 8, 9, 10, 11]);

		yield return new(guid,                          QlType.Guid, guid);
		yield return new(guid.ToString("D"),            QlType.Guid, guid);
		yield return new(guid.ToString("N"),            QlType.Guid, refused);
		yield return new(guid.ToString("B"),            QlType.Guid, refused);
	}

	[Theory]
	[MemberData(nameof(Cells))]
	public void EachCell(object value, QlType type, object? expected)
	{
		var result = Convert(value, type);

		Assert.Equal(expected, result);

		if (result is DateTime made && expected is DateTime wanted)
			Assert.Equal(wanted.Kind, made.Kind);

		if (result is DateTimeOffset madeZoned && expected is DateTimeOffset wantedZoned)
			Assert.Equal(wantedZoned.Offset, madeZoned.Offset);
	}

	[Fact]
	public void ADoubleBecomesTheDecimalItsShortestTextSays()
	{
		// The netstandard2.0 asset (tested on net8.0) has no shortest "R" to rely on and takes G15 where it reads
		// back, else G17; net10.0 takes "R". Either way a double written with up to fifteen digits is the decimal
		// those digits say on both: 0.1 is 0.1m, never 0.1000000000000000055511151231257827m, which would not equal
		// a column holding 0.1.
		Assert.Equal(0.1m,           Convert(0.1,           QlType.Decimal));
		Assert.Equal(0.3m,           Convert(0.3,           QlType.Decimal));
		Assert.Equal(0.0000001m,     Convert(1e-7,          QlType.Decimal));
		Assert.Equal(123456789.123m, Convert(123456789.123, QlType.Decimal));
		Assert.Equal(2.5m,           Convert(2.5,           QlType.Decimal));
	}

	[Fact]
	public void ATypeOutsideTheLanguageTakesItsOwnValuesOnly()
	{
		Assert.Equal(new DateOnly(2026, 1, 1), Convert(new DateOnly(2026, 1, 1), QlType.Other, typeof(DateOnly)));
		Assert.Same(Refused.Value, Convert("2026-01-01", QlType.Other, typeof(DateOnly)));
		Assert.Same(Refused.Value, Convert(new DateTime(2026, 1, 1), QlType.Other, typeof(DateOnly)));
		Assert.Equal(Kind.Paid, Convert(Kind.Paid, QlType.Other, typeof(Kind)));
		Assert.Same(Refused.Value, Convert(1, QlType.Other, typeof(Kind)));
	}

	[Fact]
	public void AListIsAnyEnumerableButAStringOrBytes()
	{
		Assert.Equal([1, 2], Coercion.Items(new[] { 1, 2 }, 100)!);
		Assert.Equal([1L], Coercion.Items(new List<long> { 1 }, 100)!);
		Assert.Null(Coercion.Items("ab", 100));
		Assert.Null(Coercion.Items(new byte[] { 1 }, 100));
		Assert.Null(Coercion.Items(1, 100));
	}

	// ── Through a call ──────────────────────────────────────────────────────

	static readonly QlModel<Shop> Model = Shop.Model(QlOptions.Default with { MaxInList = 3, MaxStringLength = 5, MaxParameterBytes = 20, MaxSkip = 100 });

	static QlError Call(string text, object? values)
	{
		return Assert.Throws<QlException>(() => Model.QueryData(Shop.InMemory(), text, Rows.Values(values))).Error;
	}

	[Fact]
	public void AValueIsListedOnceAndEnumeratedOnce()
	{
		var list = new Counted([1, 2]);

		_ = Model.QueryData(Shop.InMemory(), "from orders where qty in @q select id", new Dictionary<string, object?> { ["q"] = list });

		Assert.Equal(1, list.Enumerations);
	}

	/// <summary>A list longer than <c>MaxInList</c> is refused one item past it: an endless sequence is never read to its end.</summary>
	[Fact]
	public void AListIsReadNoFurtherThanItsBound()
	{
		var read = 0;

		IEnumerable<int> Endless()
		{
			while (true)
			{
				read++;

				yield return read;
			}
		}

		var error = Assert.Throws<QlException>(() => Model.QueryData(Shop.InMemory(), "from orders where qty in @q select id", new Dictionary<string, object?> { ["q"] = Endless() })).Error;

		Assert.Equal(QlErrorCode.Limit, error.Code);
		Assert.Equal("`@q` has more than 3 values; at most 3 are taken.", error.Message);
		Assert.Equal(4, read);
	}

	[Fact]
	public void ParameterNamesIgnoreCaseAsTheTextsNamesDo()
	{
		Assert.NotEmpty(Model.QueryData(Shop.InMemory(), "from orders where qty > @Min select id", new Dictionary<string, object?> { ["min"] = 1 }).ToList());
	}

	[Theory]
	[InlineData("from orders where qty > @n select id",                     "{}",                                          QlErrorCode.Parameter)]
	[InlineData("from orders where qty > @n select id",                     "{\"n\":\"1\"}",                               QlErrorCode.Parameter)]
	[InlineData("from orders where qty > @n * 2 select id",                 "{\"n\":null}",                                QlErrorCode.Parameter)]
	[InlineData("from orders where customer.StartsWith(@n) select id",      "{\"n\":null}",                                QlErrorCode.Parameter)]
	[InlineData("from orders where qty in @n select id",                    "{\"n\":1}",                                   QlErrorCode.Parameter)]
	[InlineData("from orders where qty in @n select id",                    "{\"n\":null}",                                QlErrorCode.Parameter)]
	[InlineData("from orders where qty in @n select id",                    "{\"n\":[1,2,3,4]}",                           QlErrorCode.Limit)]
	[InlineData("from orders where qty in @n select id",                    "{\"n\":[1,\"a\"]}",                           QlErrorCode.Parameter)]
	[InlineData("from orders where status == @n select id",                 "{\"n\":\"abcdef\"}",                          QlErrorCode.Limit)]
	[InlineData("from orders where status in @n select id",                 "{\"n\":[\"abcd\",\"abcd\",\"abcd\"]}",       QlErrorCode.Limit)]
	[InlineData("from orders skip @n select id",                            "{\"n\":-1}",                                  QlErrorCode.Parameter)]
	[InlineData("from orders take @n select id",                            "{\"n\":null}",                                QlErrorCode.Parameter)]
	[InlineData("from orders skip @n select id",                            "{\"n\":101}",                                 QlErrorCode.Limit)]
	[InlineData("from orders skip 60 where qty > 1 skip @n select id",      "{\"n\":41}",                                  QlErrorCode.Limit)]
	[InlineData("from orders where day == @n select id",                    "{\"n\":\"2026-01-01\"}",                      QlErrorCode.Parameter)]
	public void AValueThatDoesNotFitIsRefusedByTheCall(string text, string json, QlErrorCode code)
	{
		Assert.Equal(code, Call(text, Json(json)).Code);
	}

	[Fact]
	public void SkipsAreBoundedTogether()
	{
		Model.QueryData(Shop.InMemory(), "from orders skip 60 where qty > 1 skip @n select id", new Dictionary<string, object?> { ["n"] = 40 });

		var error = Call("from orders skip 60 where qty > 1 skip @n select id", new { n = 41 });

		Assert.Equal((QlErrorCode.Limit, (int?)34), (error.Code, error.Position));

		// Skips the text writes are counted when it is bound.
		var written = Assert.Throws<QlException>(() => Model.Describe("from orders skip 60 where qty > 1 skip 50 select id")).Error;

		Assert.Equal((QlErrorCode.Limit, (int?)34), (written.Code, written.Position));
	}

	/// <summary>A small JSON object as the values a web host would pass: numbers as long or double, lists as arrays.</summary>
	static Dictionary<string, object?> Json(string json)
	{
		using var document = System.Text.Json.JsonDocument.Parse(json);

		return document.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => Loose(p.Value));

		static object? Loose(System.Text.Json.JsonElement element)
		{
			return element.ValueKind switch
			{
				System.Text.Json.JsonValueKind.String => element.GetString(),
				System.Text.Json.JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
				System.Text.Json.JsonValueKind.Array  => element.EnumerateArray().Select(Loose).ToArray(),
				System.Text.Json.JsonValueKind.True   => true,
				System.Text.Json.JsonValueKind.False  => false,
				_                                     => null,
			};
		}
	}

	sealed class Counted(int[] items) : IEnumerable
	{
		public int Enumerations { get; private set; }

		public IEnumerator GetEnumerator()
		{
			Enumerations++;

			return items.GetEnumerator();
		}
	}
}
