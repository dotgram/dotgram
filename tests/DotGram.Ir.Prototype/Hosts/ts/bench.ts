// How fast the printed JSON parser reads, beside JSON.parse and, where one is given, beside a
// parser another generator made of its own JSON grammar.
//
//   node bench.ts <parser.ts> [other.mjs]
//
// The printed parser is timed with two hosts: the tree of json.ts, and one that builds what
// JSON.parse builds - numbers, strings, arrays, plain objects - which is what the others build
// and so the fair comparison. A document of about two megabytes, read whole; the median of
// fifteen readings after five that are thrown away.

import { pathToFileURL } from "node:url";

import { Runtime } from "./runtime.ts";
import { host as treeHost } from "./json.ts";

const [parserPath, otherPath] = process.argv.slice(2);

const parser = await import(pathToFileURL(parserPath).href);
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

function ours(host: any): (text: string) => any
{
	return text =>
	{
		P.begin(text, host);

		if (parser.parse(P, "ParseJson") < 0)
			throw new Error("refused at " + P.refusedAt);

		return P.rv;
	};
}

function median(read: (text: string) => any, text: string): number
{
	for (let i = 0; i < 5; i++)
		read(text);

	const times: number[] = [];

	for (let i = 0; i < 15; i++)
	{
		const from = process.hrtime.bigint();

		read(text);
		times.push(Number(process.hrtime.bigint() - from) / 1e6);
	}

	times.sort((a, b) => a - b);

	return times[7];
}

for (const [name, text] of documents)
{
	// What the hosts build is what JSON.parse builds.
	if (JSON.stringify(ours(nativeHost)(text)) !== JSON.stringify(JSON.parse(text)))
		throw new Error("The printed parser and JSON.parse disagree.");

	const builtIn = median(JSON.parse, text);
	const native = median(ours(nativeHost), text);
	const tree = median(ours(treeHost), text);
	const megabytes = text.length / 1e6;

	const line = (what: string, ms: number) =>
		console.log(`${name} (${megabytes.toFixed(2)} MB): ${what}: ${ms.toFixed(2)} ms, ${(megabytes / ms * 1000).toFixed(1)} MB/s, ${(ms / builtIn).toFixed(2)} x JSON.parse`);

	line("JSON.parse", builtIn);
	line("printed parser, values as JSON.parse builds them", native);
	line("printed parser, the tree of json.ts", tree);

	if (other !== undefined)
	{
		const theirs = median(other.parse, text);

		line("the other generator's parser", theirs);
		console.log(`${name}: printed parser / the other generator's parser = ${(native / theirs).toFixed(2)}`);
	}
}
