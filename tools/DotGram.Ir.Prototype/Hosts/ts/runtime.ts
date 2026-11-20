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

	// What a parser printed to count writes: functions entered and turns begun, decisions opened
	// or read back, turns gone straight to. A parser printed to be timed never touches them.
	kSteps = 0;
	kEntered = 0;
	kJumps = 0;

	// What a resuming reading keeps: a record a turn begun or a statement ended, behind the
	// sentinel that names the statement's entry on the tape. A record is good while the tape is not
	// cut below its key (the cursor where it was made); the records are a stack in the order of
	// their keys, cut with the tape. Each record names its owner (the sentinel) and the owner's
	// record before it; the head of each owner is kept beside the tape and never cleared - a head
	// left from an earlier owner or an earlier reading is told apart by the owner the record names.
	// Flags: 1 a statement ended (else a turn began), 2 it read, 4 it was made unbuilt.
	kKey = new Int32Array(64);
	kOwner = new Int32Array(64);
	kPrev = new Int32Array(64);
	kPos = new Int32Array(64);
	kTurns = new Int32Array(64);
	kFlags = new Int32Array(64);
	kBase = new Int32Array(64);
	kCount = 0;

	// What the slots a kept statement writes held: two entries a slot (a value, or a list and its length).
	kv: any[] = [];
	kvTop = 0;

	heads = new Int32Array(64);

	// Wrong on purpose: a head is taken without asking whose it is (the test of the owner check).
	anyOwner = false;

	// What was refused before an attempt that stood was read once more to be built, a loop a level.
	keptRefusals: { at: number; refused: number[] }[] = [];

	// The frames of the functions printed as machines: numbers in one array and values in the
	// other, a frame of each a call, the topmost last. The last number of a frame says whose it
	// is and the first where its owner goes on.
	I = new Int32Array(1024);
	V: any[] = [];
	top = 0;
	valueTop = 0;

	// How many numbers, and how many values, the frames may take before the input is called too
	// deep. The limit is the caller's to set: memory stands behind it, and the longest array the
	// language makes (somewhat over a hundred million values in V8).
	frameLimit = 1 << 26;

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
		this.kSteps = 0;
		this.kEntered = 0;
		this.kJumps = 0;
		this.kCount = 0;
		this.kvTop = 0;
		this.keptRefusals.length = 0;
		this.top = 0;
		this.valueTop = 0;
		this.V.length = 0;
	}

	// A frame for the machine of that number, which begins at its first state.
	call(id: number, numbers: number, values: number): void
	{
		const top = this.top + numbers;

		if (top > this.I.length)
		{
			if (top > this.frameLimit)
				throw new RangeError("The input nests deeper than the frames may.");

			const grown = new Int32Array(Math.max(top, this.I.length * 2));

			grown.set(this.I);
			this.I = grown;
		}

		this.I[top - numbers] = 0;
		this.I[top - 1] = id;
		this.top = top;
		this.valueTop += values;

		if (this.valueTop > this.V.length)
		{
			if (this.valueTop > this.frameLimit)
				throw new RangeError("The input nests deeper than the frames may.");

			while (this.V.length < this.valueTop)
				this.V.push(undefined);
		}
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

			const heads = new Int32Array(this.ways.length);

			heads.set(this.heads);
			this.heads = heads;
		}

		this.ways[this.count * 2] = 0;
		this.ways[this.count * 2 + 1] = last;
		this.count++;
		this.cursor = this.count;

		return this.count - 1;
	}

	enterCounted(last: number): number
	{
		this.kEntered++;

		return this.enter(last);
	}

	next(way: number, value: number): void
	{
		this.ways[way * 2] = value;
		this.count = way + 1;
		this.cursor = way + 1;

		if (this.kCount > 0)
			this.cut();
	}

	// The tape was cut: what was kept of places no longer behind it goes.
	cut(): void
	{
		while (this.kCount > 0 && this.kKey[this.kCount - 1] >= this.count)
		{
			const i = --this.kCount;

			this.heads[this.kOwner[i]] = this.kPrev[i];
			this.kvTop = this.kBase[i];
		}
	}

	// The latest record of a sentinel's entry, or -1.
	head(sentinel: number): number
	{
		const h = this.heads[sentinel];

		return h >= 0 && h < this.kCount && (this.anyOwner || this.kOwner[h] === sentinel) ? h : -1;
	}

	// Whether a record can stand for what the reading would do: one made while building, always;
	// one made unbuilt, only in a reading that builds nothing.
	usable(h: number): boolean
	{
		return (this.kFlags[h] & 4) === 0 || this.unbuilt > 0;
	}

	// A record of where a turn began (end false) or a statement ended, at the cursor, where one is
	// wanted: none of the owner yet, a later turn than its last, or one made unbuilt where this
	// reading builds. The index, whose n slots the caller fills, or -1.
	keep(owner: number, end: boolean, p: number, turns: number, read: boolean, n: number): number
	{
		const key = this.cursor;
		const h = this.head(owner);
		const fits = this.kCount === 0 || this.kKey[this.kCount - 1] <= key;

		if (h >= 0 && !(end ? (this.kFlags[h] & 1) === 0 : (this.kFlags[h] & 1) === 0 && this.kTurns[h] < turns) && !(!this.usable(h) && fits))
			return -1;

		// Below the top of the stack it would break the order cuts rely on: not kept (the evaluator's Unfit).
		if (!fits)
			return -1;

		if (this.kCount === this.kKey.length)
		{
			const size = this.kCount * 2;

			for (const name of ["kKey", "kOwner", "kPrev", "kPos", "kTurns", "kFlags", "kBase"] as const)
			{
				const grown = new Int32Array(size);

				grown.set(this[name]);
				this[name] = grown;
			}
		}

		const i = this.kCount++;

		this.kKey[i] = key;
		this.kOwner[i] = owner;
		this.kPrev[i] = h;
		this.kPos[i] = p;
		this.kTurns[i] = turns;
		this.kFlags[i] = (end ? 1 : 0) | (read ? 2 : 0) | (this.unbuilt > 0 ? 4 : 0);
		this.kBase[i] = this.kvTop;
		this.kvTop += n * 2;

		while (this.kv.length < this.kvTop)
			this.kv.push(undefined);

		this.heads[owner] = i;

		return i;
	}

	// What the tape and the records hold on to, in bytes, as allocated.
	held(): number
	{
		return this.ways.length * 4 + this.heads.length * 4 + this.kKey.length * 4 * 7 + this.kv.length * 8;
	}

	// After a reading: storage grown past the bound (in entries of the tape) goes back to its first
	// size, so that one long refusal does not keep its arrays for every reading after it.
	release(bound: number): void
	{
		if (this.ways.length > bound * 2)
		{
			this.ways = new Int32Array(64);
			this.heads = new Int32Array(64);
		}

		if (this.kKey.length > bound)
		{
			for (const name of ["kKey", "kOwner", "kPrev", "kPos", "kTurns", "kFlags", "kBase"] as const)
				this[name] = new Int32Array(64);
		}

		if (this.kv.length > bound * 2)
			this.kv = [];
	}

	// A way given back: the first is read building still; the second, where the reading was building, stops building.
	retried(u: number): number
	{
		if (u === 1)
		{
			this.unbuilt = 1;

			return 2;
		}

		return u === 0 && this.unbuilt === 0 ? 1 : u;
	}

	// An attempt stood after the loop stopped building: it is read once more, built, from the
	// loop's start (3); or that reading stood, and what it said of a refusal is taken back (0).
	stood(u: number, segment: number): number
	{
		if (u === 2)
		{
			this.keptRefusals.push({ at: this.refusedAt, refused: this.refused.slice() });
			this.unbuilt = 0;
			this.cursor = segment;

			return 3;
		}

		this.replayed();

		return 0;
	}

	// A loop that ends without an answer puts back what it raised.
	undeferred(u: number): void
	{
		if (u === 2)
			this.unbuilt = 0;
		else if (u === 3)
			this.replayed();
	}

	replayed(): void
	{
		const kept = this.keptRefusals.pop()!;

		this.refusedAt = kept.at;
		this.refused = kept.refused;
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

				if (this.kCount > 0)
					this.cut();

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
