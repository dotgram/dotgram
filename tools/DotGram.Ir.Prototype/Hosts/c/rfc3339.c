/* A hand-written host for the timestamp grammar of DotGram.Web (RFC 3339): two guards that decide
   the reading - a day that the month has, a time that the clock has - and three constructions. */

#include <stdio.h>

#include "runtime.h"
#include "parser.h"

typedef struct FullDate
{
	int year, month, day;
} FullDate;

typedef struct FullTime
{
	int  hour, minute, second;
	Val  fraction;   /* the digits after the point, or nothing */
	int  offset;     /* minutes */
	bool unknown;
} FullTime;

typedef struct Timestamp
{
	FullDate *date;
	FullTime *time;
} Timestamp;

/* What a value is, which a pointer does not say: the host keeps it in the value's spare field. */
enum { H_DATE = 1, H_TIME, H_TIMESTAMP };

static Val made(void *built, int what)
{
	Val value = val_host(built);

	value.from = what;

	return value;
}

/* The grammar has read these as digits. */
static int number(Parser *P, int32_t from, int32_t to)
{
	int value = 0;

	for (int32_t i = from; i < to; i++)
		value = value * 10 + (P->text[i] - '0');

	return value;
}

static int digits(Parser *P, Val text)
{
	return number(P, text.from, text.to);
}

/* Section 5.7: a month of the year, and a day within it. */
bool host_g1(Parser *P, Val year, Val month, Val day)
{
	int y = digits(P, year);
	int m = digits(P, month);
	int d = digits(P, day);

	if (m < 1 || m > 12 || d < 1)
		return false;

	bool leap = y % 4 == 0 && (y % 100 != 0 || y % 400 == 0);

	return d <= (m == 2 ? (leap ? 29 : 28) : m == 4 || m == 6 || m == 9 || m == 11 ? 30 : 31);
}

/* Section 5.7: an hour, a minute and an offset in range, and a leap second only at 23:59 UTC. */
bool host_g3(Parser *P, Val hour, Val minute, Val second, Val offset)
{
	int h = digits(P, hour);
	int m = digits(P, minute);
	int s = digits(P, second);

	if (h > 23 || m > 59 || s > 60)
		return false;

	int shift = 0;

	if (offset.to - offset.from > 1)
	{
		int offsetHour   = number(P, offset.from + 1, offset.from + 3);
		int offsetMinute = number(P, offset.from + 4, offset.from + 6);

		if (offsetHour > 23 || offsetMinute > 59)
			return false;

		shift = (P->text[offset.from] == '-' ? -1 : 1) * (offsetHour * 60 + offsetMinute);
	}

	if (s < 60)
		return true;

	return ((h * 60 + m - shift) % 1440 + 1440) % 1440 == 23 * 60 + 59;
}

Val host_c0(Parser *P, Val date, Val time)
{
	Timestamp *timestamp = rt_alloc(P, sizeof(Timestamp));

	timestamp->date = date.ptr;
	timestamp->time = time.ptr;

	return made(timestamp, H_TIMESTAMP);
}

Val host_c2(Parser *P, Val year, Val month, Val day)
{
	FullDate *date = rt_alloc(P, sizeof(FullDate));

	date->year  = digits(P, year);
	date->month = digits(P, month);
	date->day   = digits(P, day);

	return made(date, H_DATE);
}

Val host_c4(Parser *P, Val hour, Val minute, Val second, Val fraction, Val offset)
{
	FullTime *time = rt_alloc(P, sizeof(FullTime));

	time->hour     = digits(P, hour);
	time->minute   = digits(P, minute);
	time->second   = digits(P, second);
	time->fraction = fraction.to == fraction.from ? VAL_NONE : val_text(fraction.from + 1, fraction.to);
	time->offset   = 0;
	time->unknown  = false;

	if (offset.to - offset.from > 1)
	{
		int  size  = number(P, offset.from + 1, offset.from + 3) * 60 + number(P, offset.from + 4, offset.from + 6);
		bool minus = P->text[offset.from] == '-';

		time->offset  = minus ? -size : size;
		time->unknown = minus && size == 0;
	}

	return made(time, H_TIME);
}

static void date_out(const FullDate *date)
{
	printf("D(%d,%d,%d)", date->year, date->month, date->day);
}

static void time_out(Parser *P, const FullTime *time)
{
	printf("T(%d,%d,%d,", time->hour, time->minute, time->second);

	if (time->fraction.kind == V_NONE)
		fputs("null", stdout);
	else
		for (int32_t i = time->fraction.from; i < time->fraction.to; i++)
			putchar(P->text[i]);

	printf(",%d,%s)", time->offset, time->unknown ? "true" : "false");
}

void host_begin(Parser *P, const char *answers, const char *throwOn)
{
	(void)P;
	(void)answers;
	(void)throwOn;
}

void host_render(Parser *P, Val value)
{
	switch (value.from)
	{
		case H_DATE:
			date_out(value.ptr);
			break;

		case H_TIME:
			time_out(P, value.ptr);
			break;

		default:
		{
			const Timestamp *timestamp = value.ptr;

			fputs("TS(", stdout);
			date_out(timestamp->date);
			putchar(',');
			time_out(P, timestamp->time);
			putchar(')');
			break;
		}
	}
}

void host_end(Parser *P)
{
	(void)P;
}
