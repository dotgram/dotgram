// How fast the printed JSON parser reads, beside JSON.parse and, where one is given, beside a
// parser another generator made of its own JSON grammar.
//
//   node bench.ts <directory of the printed parsers> [other.mjs]
//
// The printed parser comes three times, by where it keeps a function's frame across a call:
// parser.ts on the language's stack, parser-recursive.ts in arrays of its own for the calls that
// can come back to their caller, parser-all.ts in those arrays for every call. Each is timed with
// a host that builds what JSON.parse builds - numbers, strings, arrays, plain objects - which is
// what the others build and so the fair comparison; the tree of json.ts is timed once, beside.
// A document of about two megabytes, read whole. The readers take turns: five rounds thrown
// away, then fifteen (IR_BENCH_ROUNDS) of which the median, the least and the most are given.

import { join } from "node:path";
import { pathToFileURL } from "node:url";

import { Runtime } from "./runtime.ts";
import { host as treeHost } from "./json.ts";

const [directory, otherPath] = process.argv.slice(2);

const plain = await import(pathToFileURL(join(directory, "parser.ts")).href);
const recursive = await import(pathToFileURL(join(directory, "parser-recursive.ts")).href);
const all = await import(pathToFileURL(join(directory, "parser-all.ts")).href);
const other = otherPath ? await import(pathToFileURL(otherPath).href) : undefined;

function unescaped(token: string): string
{
	const inside = token.substring(1, token.length - 1);

	return inside.indexOf("\\") < 0 ? inside : JSON.parse(token);
}

function joined(first: any, rest: any[]): any[]
{
	if (first === undefined)
		return [];

	rest.unshift(first);

	return rest;
}

const nativeHost = {
	c0: () => false,
	c1: () => null,
	c2: () => true,
	c3: (text: string) => parseFloat(text),
	c4: (text: string) => unescaped(text),
	c5: (members: [string, any][]) =>
	{
		const built: any = {};

		for (let i = 0; i < members.length; i++)
			built[members[i][0]] = members[i][1];

		return built;
	},
	c6: joined,
	c7: (name: string, value: any) => [unescaped(name), value],
	c8: (items: any[]) => items,
	c9: joined,
};

// The same document every time: a generator with a fixed seed.
let seed = 12345;

function random(below: number): number
{
	seed = (seed * 1103515245 + 12345) & 0x7fffffff;

	return seed % below;
}

function word(): string
{
	const letters = "abcdefghijklmnopqrstuvwxyz";
	let out = "";

	for (let i = 3 + random(9); i > 0; i--)
		out += letters[random(26)];

	return out;
}

const rows: any[] = [];

for (let i = 0; i < 12000; i++)
{
	rows.push({
		id: i,
		name: word() + " " + word(),
		price: random(100000) / 100,
		ratio: -random(1000) / 7e3,
		tags: [word(), word(), word()].slice(0, random(4)),
		active: random(2) === 0,
		parent: random(5) === 0 ? null : { id: random(i + 1), note: "line\nbreak \"quoted\" é" },
	});
}

const documents: [string, string][] = [
	["compact", JSON.stringify(rows)],
	["indented", JSON.stringify(rows, null, 2)],
];

const P = new Runtime();

function ours(parser: any, host: any): (text: string) => any
{
	return text =>
	{
		P.begin(text, host);

		if (parser.parse(P, "ParseJson") < 0)
			throw new Error("refused at " + P.refusedAt);

		return P.rv;
	};
}

const readers: [string, (text: string) => any][] = [
	["JSON.parse", JSON.parse],
	["printed parser, frames on the language's stack", ours(plain, nativeHost)],
	["printed parser, frames of its own where calls recur", ours(recursive, nativeHost)],
	["printed parser, frames of its own for every call", ours(all, nativeHost)],
	["printed parser, frames of its own where calls recur, the tree of json.ts", ours(recursive, treeHost)],
];

if (other !== undefined)
	readers.push(["the other generator's parser", other.parse]);

const rounds = Number(process.env.IR_BENCH_ROUNDS ?? 15);

for (const [name, text] of documents)
{
	// What the hosts build is what JSON.parse builds.
	const expected = JSON.stringify(JSON.parse(text));

	for (const parser of [plain, recursive, all])
		if (JSON.stringify(ours(parser, nativeHost)(text)) !== expected)
			throw new Error("The printed parser and JSON.parse disagree.");

	const times: number[][] = readers.map(() => []);

	for (let round = -5; round < rounds; round++)
	{
		for (let turn = 0; turn < readers.length; turn++)
		{
			// Whoever read first in the last round reads second in this one.
			const at = (turn + round + 5) % readers.length;
			const from = process.hrtime.bigint();

			readers[at][1](text);

			if (round >= 0)
				times[at].push(Number(process.hrtime.bigint() - from) / 1e6);
		}
	}

	for (const list of times)
		list.sort((a, b) => a - b);

	const median = (at: number) => times[at][times[at].length >> 1];
	const megabytes = text.length / 1e6;

	for (let at = 0; at < readers.length; at++)
		console.log(`${name} (${megabytes.toFixed(2)} MB): ${readers[at][0]}: ${median(at).toFixed(2)} ms [${times[at][0].toFixed(2)}-${times[at][times[at].length - 1].toFixed(2)}], ${(megabytes / median(at) * 1000).toFixed(1)} MB/s, ${(median(at) / median(0)).toFixed(2)} x JSON.parse`);

	if (other !== undefined)
		for (let at = 1; at < readers.length - 1; at++)
			console.log(`${name}: ${readers[at][0]} / the other generator's parser = ${(median(at) / median(readers.length - 1)).toFixed(2)}`);
}
