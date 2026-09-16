using System;
using System.Globalization;
using System.Numerics;

namespace DotGram.Finance.Fix;

/// <summary>An exact base-ten number: Coefficient multiplied by ten to the power -Scale.</summary>
public readonly struct FixDecimal
{
	public FixDecimal(BigInteger coefficient, int scale) { Coefficient = coefficient; Scale = scale; }
	public BigInteger Coefficient { get; }
	public int Scale { get; }
	public bool TryGetDecimal(out decimal value)
	{
		value = 0;
		var coefficient = Coefficient;
		var scale = Scale;
		while (scale > 0 && coefficient % 10 == 0) { coefficient /= 10; scale--; }
		if (scale > 28 || coefficient < new BigInteger(decimal.MinValue) || coefficient > new BigInteger(decimal.MaxValue)) return false;
		value = (decimal)coefficient;
		while (scale-- > 0) value /= 10;
		return true;
	}
	public override string ToString()
	{
		var digits = BigInteger.Abs(Coefficient).ToString(CultureInfo.InvariantCulture);
		if (Scale > 0) { digits = digits.PadLeft(Scale + 1, '0'); digits = digits.Insert(digits.Length - Scale, "."); }
		return Coefficient.Sign < 0 ? "-" + digits : digits;
	}
}

/// <summary>A FIX calendar date, including the protocol's year zero.</summary>
public readonly struct FixDate
{
	public FixDate(int year, int month, int day) { Year = year; Month = month; Day = day; }
	public int Year { get; }
	public int Month { get; }
	public int Day { get; }
}

/// <summary>A FIX time retaining leap-second notation and all fractional digits.</summary>
public readonly struct FixTime
{
	public FixTime(int hour, int minute, int second, string fraction) { Hour = hour; Minute = minute; Second = second; Fraction = fraction; }
	public int Hour { get; }
	public int Minute { get; }
	public int Second { get; }
	public string Fraction { get; }
}

public readonly struct FixTimestamp
{
	public FixTimestamp(FixDate date, FixTime time) { Date = date; Time = time; }
	public FixDate Date { get; }
	public FixTime Time { get; }
}

/// <summary>A year/month with an optional day or week-of-month qualifier.</summary>
public readonly struct FixMonthYear
{
	public FixMonthYear(int year, int month, int? day, int? week) { Year = year; Month = month; Day = day; Week = week; }
	public int Year { get; }
	public int Month { get; }
	public int? Day { get; }
	public int? Week { get; }
}
