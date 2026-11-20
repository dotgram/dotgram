// Reads a batch of inputs with a printed parser and a host, and says of each what the C# side
// says of it: the verdict, the furthest position, what was built or what was expected, and the
// host calls where the host records them.
//
//   node --experimental-strip-types driver.ts <parser.ts> <host.ts> <entry> <batch> [answers] [throwOn]
//
// A batch is a count, then for each input its length in UTF-16 code units and the units, all
// little-endian: the inputs are the code units the C# parser read, lone surrogates included.

import { readFileSync } from "node:fs";
import { pathToFileURL } from "node:url";

import { Runtime } from "./runtime.ts";

const [parserPath, hostPath, entry, batchPath, answersPath, throwOn] = process.argv.slice(2);

const parser = await import(pathToFileURL(parserPath).href);
const hosts = await import(pathToFileURL(hostPath).href);

const answers = new Map<string, boolean>();

if (answersPath)
{
	for (const line of readFileSync(answersPath, "utf8").split("\n"))
	{
		const at = line.lastIndexOf("=");

		if (at > 0)
			answers.set(line.substring(0, at), line.substring(at + 1).trim() === "true");
	}
}

const batch = readFileSync(batchPath);
const count = batch.readInt32LE(0);
const out: string[] = [];
const P = new Runtime();
const counting = process.env.IR_COUNTS !== undefined;

let at = 4;

for (let i = 0; i < count; i++)
{
	const length = batch.readInt32LE(at);

	at += 4;

	let text = "";

	for (let from = 0; from < length; from += 8192)
	{
		const units: number[] = [];

		for (let j = from; j < Math.min(length, from + 8192); j++)
			units.push(batch.readUInt16LE(at + j * 2));

		text += String.fromCharCode(...units);
	}

	at += length * 2;

	const { host, render, takeLog } = hosts.createHost(answers, throwOn || undefined);

	P.begin(text, host);

	try
	{
		const end = parser.parse(P, entry);

		out.push(end >= 0
			? "A " + end + " " + render(P.rv)
			: "R " + P.refusedAt + " " + P.refused.slice().sort((a, b) => a - b).join(","));
	}
	catch (thrown)
	{
		out.push("T " + (thrown instanceof RangeError ? "stack" : String((thrown as Error).message)));
	}

	const calls = takeLog();

	// Where only the counts are asked for, the calls are counted and not written.
	if (counting)
		out.push("K " + P.kSteps + " " + P.kEntered + " " + P.retries + " " + P.kJumps + " " + calls.length);
	else
		for (const call of calls)
			out.push("L " + call);

	out.push(".");
}

process.stdout.write(out.join("\n") + "\n");
