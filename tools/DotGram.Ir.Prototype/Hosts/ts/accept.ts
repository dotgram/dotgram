// What resuming costs a reading that is accepted: the printed parser with its sentinels and the
// same parser printed without them, read in turns over the same inputs, each way of keeping
// frames on its own. A direction, on a shared machine: the median of the rounds.
//
//   node accept.ts <directory> <entry> <batch>
//
// The directory holds parser-{on,off}-{none,recursive,all}.ts. The host builds nothing and
// every guard says yes: what is timed is the reading. A batch is the drivers' format.

import { readFileSync } from "node:fs";
import { join } from "node:path";
import { pathToFileURL } from "node:url";

import { Runtime } from "./runtime.ts";

const [directory, entry, batchPath] = process.argv.slice(2);

const batch = readFileSync(batchPath);
const count = batch.readInt32LE(0);
const inputs: string[] = [];

let at = 4;

for (let i = 0; i < count; i++)
{
	const length = batch.readInt32LE(at);

	at += 4;

	let text = "";

	for (let j = 0; j < length; j++)
		text += String.fromCharCode(batch.readUInt16LE(at + j * 2));

	at += length * 2;
	inputs.push(text);
}

const host = new Proxy({}, {
	get(_target, name)
	{
		return String(name).startsWith("g") ? () => true : () => null;
	},
});

const P = new Runtime();

function reader(parser: any): () => number
{
	return () =>
	{
		let accepted = 0;

		for (const text of inputs)
		{
			P.begin(text, host);

			if (parser.parse(P, entry) >= 0)
				accepted++;
		}

		return accepted;
	};
}

const rounds = Number(process.env.IR_BENCH_ROUNDS ?? 21);

for (const frames of ["none", "recursive", "all"])
{
	const on = reader(await import(pathToFileURL(join(directory, `parser-on-${frames}.ts`)).href));
	const off = reader(await import(pathToFileURL(join(directory, `parser-off-${frames}.ts`)).href));

	if (on() !== off())
		throw new Error("The two parsers accept different inputs.");

	// Enough repetitions of the batch for a reading of some milliseconds.
	let reps = 1;

	for (let from = performance.now(); performance.now() - from < 20; reps++)
		off();

	const ratios: number[] = [];
	const times: number[][] = [[], []];

	for (let round = -3; round < rounds; round++)
	{
		const pair = round % 2 === 0 ? [on, off] : [off, on];
		const took: number[] = [];

		for (const read of pair)
		{
			const from = performance.now();

			for (let r = 0; r < reps; r++)
				read();

			took.push(performance.now() - from);
		}

		if (round < 0)
			continue;

		const [timeOn, timeOff] = round % 2 === 0 ? took : [took[1], took[0]];

		times[0].push(timeOn);
		times[1].push(timeOff);
		ratios.push(timeOn / timeOff);
	}

	ratios.sort((a, b) => a - b);

	for (const list of times)
		list.sort((a, b) => a - b);

	console.log(`${frames}\t${on()}/${inputs.length}\t${(times[0][times[0].length >> 1] / reps).toFixed(4)}\t${(times[1][times[1].length >> 1] / reps).toFixed(4)}\t${ratios[ratios.length >> 1].toFixed(3)}\t${ratios[0].toFixed(3)}-${ratios[ratios.length - 1].toFixed(3)}`);
}
