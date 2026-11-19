// A hand-written host for the timestamp grammar of DotGram.Web (RFC 3339): two guards that decide
// the reading - a day that the month has, a time that the clock has - and three constructions.

type FullDate = { year: number; month: number; day: number };
type FullTime = { hour: number; minute: number; second: number; fraction: string | undefined; offset: number; unknown: boolean };

// The grammar has read these as digits.
function number(digits: string): number
{
	let value = 0;

	for (let i = 0; i < digits.length; i++)
		value = value * 10 + (digits.charCodeAt(i) - 0x30);

	return value;
}

// §5.7: a month of the year, and a day within it.
function isDate(year: string, month: string, day: string): boolean
{
	const y = number(year);
	const m = number(month);
	const d = number(day);

	if (m < 1 || m > 12 || d < 1)
		return false;

	const leap = y % 4 === 0 && (y % 100 !== 0 || y % 400 === 0);

	return d <= (m === 2 ? (leap ? 29 : 28) : m === 4 || m === 6 || m === 9 || m === 11 ? 30 : 31);
}

// §5.7: an hour, a minute and an offset in range, and a leap second only at 23:59 UTC.
function isTime(hour: string, minute: string, second: string, offset: string): boolean
{
	const h = number(hour);
	const m = number(minute);
	const s = number(second);

	if (h > 23 || m > 59 || s > 60)
		return false;

	let shift = 0;

	if (offset.length > 1)
	{
		const offsetHour = number(offset.substring(1, 3));
		const offsetMinute = number(offset.substring(4, 6));

		if (offsetHour > 23 || offsetMinute > 59)
			return false;

		shift = (offset[0] === "-" ? -1 : 1) * (offsetHour * 60 + offsetMinute);
	}

	if (s < 60)
		return true;

	return ((h * 60 + m - shift) % 1440 + 1440) % 1440 === 23 * 60 + 59;
}

function time(hour: string, minute: string, second: string, fraction: string, offset: string): FullTime
{
	const digits = fraction.length === 0 ? undefined : fraction.substring(1);

	if (offset.length === 1)
		return { hour: number(hour), minute: number(minute), second: number(second), fraction: digits, offset: 0, unknown: false };

	const size = number(offset.substring(1, 3)) * 60 + number(offset.substring(4, 6));

	return {
		hour: number(hour), minute: number(minute), second: number(second), fraction: digits,
		offset: offset[0] === "-" ? -size : size, unknown: offset === "-00:00",
	};
}

export const host = {
	c0: (date: FullDate, time: FullTime) => ({ date, time }),
	g1: isDate,
	c2: (year: string, month: string, day: string): FullDate => ({ year: number(year), month: number(month), day: number(day) }),
	g3: isTime,
	c4: time,
};

function render(value: any): string
{
	if (value.date !== undefined)
		return "TS(" + render(value.date) + "," + render(value.time) + ")";

	if (value.year !== undefined)
		return "D(" + value.year + "," + value.month + "," + value.day + ")";

	return "T(" + value.hour + "," + value.minute + "," + value.second + "," + (value.fraction ?? "null") + "," + value.offset + "," + value.unknown + ")";
}

export function createHost()
{
	return { host, render, takeLog: (): string[] => [] };
}
