using System;
using System.Text;

using DotGram.Finance.Fix;

using Xunit;

namespace DotGram.Finance.Tests;

/// <summary>
/// FIX 5.0's zoned values, TZTimestamp and TZTimeOnly, read as the repository describes them: the
/// clock to the minute with seconds optional, and an offset that is <c>Z</c> or a sign and hours from
/// 01 to 12 with minutes optional. Every case through characters and through octets.
/// </summary>
public sealed class FixZonedConvertTests
{
	[Theory]
	[InlineData("20060901-07:39Z",        2006, 9, 1,  7, 39,  0,   0,   0)]
	[InlineData("20060901-02:39-05",      2006, 9, 1,  2, 39,  0,  -5,   0)]
	[InlineData("20060901-15:39+08",      2006, 9, 1, 15, 39,  0,   8,   0)]
	[InlineData("20060901-13:09+05:30",   2006, 9, 1, 13,  9,  0,   5,  30)]
	[InlineData("20060901-13:09:47-03:30", 2006, 9, 1, 13, 9, 47,  -3, -30)]
	public void A_zoned_timestamp_keeps_the_offset_it_was_written_with(string text, int year, int month, int day, int hour, int minute, int second, int hours, int minutes)
	{
		var expected = new DateTimeOffset(year, month, day, hour, minute, second, new TimeSpan(hours, minutes, 0));

		foreach (var (valid, value) in new[] { text.AsSpan().ToZonedTimestamp(), Encoding.Latin1.GetBytes(text).AsSpan().ToZonedTimestamp() })
		{
			Assert.True(valid, text);
			Assert.Equal(expected, value);
			Assert.Equal(expected.Offset, value.Offset);
		}
	}

	[Theory]
	[InlineData("07:39Z",       7, 39,  0,  0,   0)]
	[InlineData("02:39-05",     2, 39,  0, -5,   0)]
	[InlineData("13:09+05:30", 13,  9,  0,  5,  30)]
	[InlineData("13:09:47+12", 13,  9, 47, 12,   0)]
	public void A_zoned_time_is_the_clock_as_written_and_its_offset(string text, int hour, int minute, int second, int hours, int minutes)
	{
		var expected = (new TimeOnly(hour, minute, second), new TimeSpan(hours, minutes, 0));

		foreach (var (valid, value) in new[] { text.AsSpan().ToZonedTime(), Encoding.Latin1.GetBytes(text).AsSpan().ToZonedTime() })
		{
			Assert.True(valid, text);
			Assert.Equal(expected, value);
		}
	}

	[Theory]
	[InlineData("13:09")]          // no offset: which instant it is, it does not say
	[InlineData("13:09+00")]       // the hours of an offset run from 01
	[InlineData("13:09+13")]       // to 12
	[InlineData("13:09+5")]
	[InlineData("13:09+05:60")]
	[InlineData("13:09+0530")]
	[InlineData("13:09ZZ")]
	[InlineData("24:00Z")]
	[InlineData("13:60Z")]
	[InlineData("23:59:60Z")]      // a zoned value admits no leap second
	[InlineData("13:9Z")]
	[InlineData("13:09:4Z")]
	[InlineData("Z")]
	[InlineData("")]
	public void A_zoned_time_the_format_does_not_describe_is_not_valid(string text)
	{
		Assert.False(text.AsSpan().ToZonedTime().Valid, text);
		Assert.False(Encoding.Latin1.GetBytes(text).AsSpan().ToZonedTime().Valid, text);
		Assert.False(("20060901-" + text).AsSpan().ToZonedTimestamp().Valid, text);
		Assert.False(Encoding.Latin1.GetBytes("20060901-" + text).AsSpan().ToZonedTimestamp().Valid, text);
	}

	[Theory]
	[InlineData("20060931-07:39Z")]   // no such day
	[InlineData("2006091-07:39Z")]
	[InlineData("20060901 07:39Z")]
	[InlineData("00000101-07:39Z")]   // the year 0000, which no calendar type holds
	public void A_zoned_timestamp_on_no_day_is_not_valid(string text)
	{
		Assert.False(text.AsSpan().ToZonedTimestamp().Valid, text);
		Assert.False(Encoding.Latin1.GetBytes(text).AsSpan().ToZonedTimestamp().Valid, text);
	}

	[Fact]
	public void A_zoned_timestamp_whose_instant_is_past_the_calendar_is_not_valid()
	{
		// The last day at an offset behind UTC is an instant after the last one DateTimeOffset holds.
		Assert.False("99991231-23:59-01".AsSpan().ToZonedTimestamp().Valid);
		Assert.True("99991231-23:59+01".AsSpan().ToZonedTimestamp().Valid);
	}
}
