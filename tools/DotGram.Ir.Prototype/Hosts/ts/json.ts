// A hand-written host for the JSON grammar of DotGram.Web (RFC 8259): the ten host functions its
// fragments were lifted into, building a tree, and the tree written back as DotGram.Web writes it.
// The numbers are the fragments' order in the grammar; the printed parser's Host interface says
// which text each one was.

export type Json =
	| { kind: "null" }
	| { kind: "boolean"; value: boolean }
	| { kind: "number"; text: string }
	| { kind: "string"; value: string }
	| { kind: "array"; items: Json[] }
	| { kind: "object"; members: [string, Json][] };

const NULL: Json = { kind: "null" };
const TRUE: Json = { kind: "boolean", value: true };
const FALSE: Json = { kind: "boolean", value: false };

// The token as it was read, quotation marks and all, to the text it stands for (§7).
function unescaped(token: string): string
{
	const inside = token.substring(1, token.length - 1);

	if (inside.indexOf("\\") < 0)
		return inside;

	let out = "";

	for (let at = 0; at < inside.length; at++)
	{
		const unit = inside[at];

		if (unit !== "\\")
		{
			out += unit;
			continue;
		}

		const escaped = inside[++at];

		switch (escaped)
		{
			case "b": out += "\b"; break;
			case "f": out += "\f"; break;
			case "n": out += "\n"; break;
			case "r": out += "\r"; break;
			case "t": out += "\t"; break;

			case "u":
				out += String.fromCharCode(parseInt(inside.substring(at + 1, at + 5), 16));
				at += 4;
				break;

			default:
				out += escaped;
				break;
		}
	}

	return out;
}

function joined<T>(first: T | undefined, rest: T[]): T[]
{
	if (first === undefined)
		return [];

	rest.unshift(first);

	return rest;
}

export const host = {
	c0: (): Json => FALSE,
	c1: (): Json => NULL,
	c2: (): Json => TRUE,
	c3: (text: string): Json => ({ kind: "number", text }),
	c4: (text: string): Json => ({ kind: "string", value: unescaped(text) }),
	c5: (members: [string, Json][]): Json => ({ kind: "object", members }),
	c6: (first: [string, Json] | undefined, rest: [string, Json][]) => joined(first, rest),
	c7: (name: string, value: Json): [string, Json] => [unescaped(name), value],
	c8: (items: Json[]): Json => ({ kind: "array", items }),
	c9: (first: Json | undefined, rest: Json[]) => joined(first, rest),
};

function quoted(text: string): string
{
	let out = "\"";

	for (let i = 0; i < text.length; i++)
	{
		const unit = text.charCodeAt(i);

		switch (unit)
		{
			case 0x22: out += "\\\""; break;
			case 0x5c: out += "\\\\"; break;
			case 0x08: out += "\\b"; break;
			case 0x0c: out += "\\f"; break;
			case 0x0a: out += "\\n"; break;
			case 0x0d: out += "\\r"; break;
			case 0x09: out += "\\t"; break;

			default:
				out += unit < 0x20 || (unit >= 0xd800 && unit <= 0xdfff)
					? "\\u" + unit.toString(16).padStart(4, "0")
					: text[i];

				break;
		}
	}

	return out + "\"";
}

// With a stack of its own: a text the parser read may nest deeper than a call stack goes.
export function write(value: Json): string
{
	const out: string[] = [];
	const pending: (Json | string)[] = [value];

	while (pending.length > 0)
	{
		const next = pending.pop()!;

		if (typeof next === "string")
		{
			out.push(next);
			continue;
		}

		switch (next.kind)
		{
			case "object":
				out.push("{");
				pending.push("}");

				for (let i = next.members.length - 1; i >= 0; i--)
				{
					pending.push(next.members[i][1]);
					pending.push(quoted(next.members[i][0]) + ":");

					if (i > 0)
						pending.push(",");
				}

				break;

			case "array":
				out.push("[");
				pending.push("]");

				for (let i = next.items.length - 1; i >= 0; i--)
				{
					pending.push(next.items[i]);

					if (i > 0)
						pending.push(",");
				}

				break;

			case "string": out.push(quoted(next.value)); break;
			case "number": out.push(next.text); break;
			case "boolean": out.push(next.value ? "true" : "false"); break;
			default: out.push("null"); break;
		}
	}

	return out.join("");
}

export function createHost()
{
	return { host, render: write, takeLog: (): string[] => [] };
}
