// What every printed parser stands on: the input, the tape of decisions, the furthest refusal,
// and the register a function leaves its value in. Hand-written, once for every grammar.

export class Runtime
{
	text = "";
	len = 0;

	host: any = undefined;

	// What the function that last returned built.
	rv: any = undefined;

	// Inside a look: nothing is recorded, and nothing is built.
	quiet = 0;
	unbuilt = 0;

	refusedAt = -1;
	refused: number[] = [];

	// The tape: a decision and the last value it can take, an entry. Under the cursor is what an
	// earlier run wrote, read back; past the count a new entry is opened.
	ways = new Int32Array(64);
	count = 0;
	cursor = 0;
	retries = 0;

	begin(text: string, host: any): void
	{
		this.text = text;
		this.len = text.length;
		this.host = host;
		this.rv = undefined;
		this.quiet = 0;
		this.unbuilt = 0;
		this.refusedAt = -1;
		this.refused.length = 0;
		this.count = 0;
		this.cursor = 0;
		this.retries = 0;
	}

	refuse(at: number, expected: number): void
	{
		if (at > this.refusedAt)
		{
			this.refusedAt = at;
			this.refused.length = 0;
		}

		if (!this.refused.includes(expected))
			this.refused.push(expected);
	}

	enter(last: number): number
	{
		if (this.cursor < this.count)
			return this.cursor++;

		if (this.count * 2 + 2 > this.ways.length)
		{
			const grown = new Int32Array(this.ways.length * 2);

			grown.set(this.ways);
			this.ways = grown;
		}

		this.ways[this.count * 2] = 0;
		this.ways[this.count * 2 + 1] = last;
		this.count++;
		this.cursor = this.count;

		return this.count - 1;
	}

	next(way: number, value: number): void
	{
		this.ways[way * 2] = value;
		this.count = way + 1;
		this.cursor = way + 1;
	}

	retry(segment: number): boolean
	{
		for (let way = this.cursor - 1; way >= segment; way--)
		{
			if (this.ways[way * 2] < this.ways[way * 2 + 1])
			{
				this.ways[way * 2]++;
				this.count = way + 1;
				this.cursor = segment;
				this.retries++;

				return true;
			}
		}

		return false;
	}

	starts(at: number, literal: string): boolean
	{
		return this.text.startsWith(literal, at);
	}

	// How far the input agrees with a text it does not go on to match.
	agree(at: number, literal: string): number
	{
		let agreed = 0;

		while (agreed < literal.length && at + agreed < this.len && this.text.charCodeAt(at + agreed) === literal.charCodeAt(agreed))
			agreed++;

		return agreed;
	}

	same(from: number, at: number, length: number): boolean
	{
		for (let i = 0; i < length; i++)
			if (this.text.charCodeAt(from + i) !== this.text.charCodeAt(at + i))
				return false;

		return true;
	}
}
