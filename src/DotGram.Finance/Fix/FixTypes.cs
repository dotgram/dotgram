using System;

namespace DotGram.Finance.Fix;

/// <summary>A FIX calendar date, including the protocol's year zero.</summary>
public readonly struct FixDate
{
	/// <summary>
	/// Creates a date from its parts, without checking them.
	/// </summary>
	public FixDate(int year, int month, int day)
	{
		Year  = year;
		Month = month;
		Day   = day;
	}

	/// <summary>
	/// The year; FIX allows year zero.
	/// </summary>
	public int Year  { get; }
	/// <summary>
	/// The month, 1 through 12.
	/// </summary>
	public int Month { get; }
	/// <summary>
	/// The day of the month, from 1.
	/// </summary>
	public int Day   { get; }
}

/// <summary>A FIX time retaining leap-second notation and all fractional digits.</summary>
public readonly struct FixTime
{
	/// <summary>
	/// Creates a time from its parts, without checking them.
	/// </summary>
	public FixTime(int hour, int minute, int second, string fraction)
	{
		Hour     = hour;
		Minute   = minute;
		Second   = second;
		Fraction = fraction;
	}

	/// <summary>
	/// The hour, 0 through 23.
	/// </summary>
	public int    Hour     { get; }
	/// <summary>
	/// The minute, 0 through 59.
	/// </summary>
	public int    Minute   { get; }
	/// <summary>
	/// The second, 0 through 60; 60 is a leap second and occurs only at 23:59.
	/// </summary>
	public int    Second   { get; }
	/// <summary>
	/// The fractional-second digits exactly as written, without the decimal point; empty when there are none.
	/// </summary>
	public string Fraction { get; }
}

/// <summary>
/// A FIX timestamp: a date and a time of day.
/// </summary>
public readonly struct FixTimestamp
{
	/// <summary>
	/// Creates a timestamp from a date and a time.
	/// </summary>
	public FixTimestamp(FixDate date, FixTime time) { Date = date; Time = time; }

	/// <summary>
	/// The date.
	/// </summary>
	public FixDate Date { get; }
	/// <summary>
	/// The time of day.
	/// </summary>
	public FixTime Time { get; }
}

/// <summary>A year/month with an optional day or week-of-month qualifier.</summary>
public readonly struct FixMonthYear
{
	/// <summary>
	/// Creates a month-year from its parts, without checking them.
	/// </summary>
	public FixMonthYear(int year, int month, int? day, int? week) { Year = year; Month = month; Day = day; Week = week; }

	/// <summary>
	/// The year.
	/// </summary>
	public int  Year  { get; }
	/// <summary>
	/// The month, 1 through 12.
	/// </summary>
	public int  Month { get; }
	/// <summary>
	/// The day of the month, or null; never set together with <see cref="Week"/>.
	/// </summary>
	public int? Day   { get; }
	/// <summary>
	/// The week of the month, 1 through 5, written w1 to w5; or null, and never set together with <see cref="Day"/>.
	/// </summary>
	public int? Week  { get; }
}
