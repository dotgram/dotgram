using System;

namespace DotGram.Finance.Fix;

/// <summary>A FIX calendar date, including the protocol's year zero.</summary>
public readonly struct FixDate
{
	public FixDate(int year, int month, int day)
	{
		Year  = year;
		Month = month;
		Day   = day;
	}

	public int Year  { get; }
	public int Month { get; }
	public int Day   { get; }
}

/// <summary>A FIX time retaining leap-second notation and all fractional digits.</summary>
public readonly struct FixTime
{
	public FixTime(int hour, int minute, int second, string fraction)
	{
		Hour     = hour;
		Minute   = minute;
		Second   = second;
		Fraction = fraction;
	}

	public int    Hour     { get; }
	public int    Minute   { get; }
	public int    Second   { get; }
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

	public int  Year  { get; }
	public int  Month { get; }
	public int? Day   { get; }
	public int? Week  { get; }
}
