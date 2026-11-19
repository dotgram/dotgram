// The recording host: every construction builds the text that says which fragment it was and
// over what, every call is written down in the order it ran, and a guard answers what the C#
// parser's guard answered over the same arguments. One host for every grammar: it is asked by
// name and answers whatever the name.

class Built
{
	spelled: string;

	constructor(spelled: string)
	{
		this.spelled = spelled;
	}
}

// One spelling for every reading: a text in quotes, nothing as null, a sequence in brackets.
export function canon(value: any): string
{
	if (value === undefined || value === null)
		return "null";

	if (value instanceof Built)
		return value.spelled;

	if (typeof value === "string")
		return quoted(value);

	if (Array.isArray(value))
		return "[" + value.map(canon).join(",") + "]";

	return String(value);
}

export function quoted(text: string): string
{
	let out = "\"";

	for (let i = 0; i < text.length; i++)
	{
		const unit = text.charCodeAt(i);

		if (unit === 0x22 || unit === 0x5c || unit < 0x20 || unit > 0x7e)
			out += "\\u" + unit.toString(16).padStart(4, "0");
		else
			out += text[i];
	}

	return out + "\"";
}

export function createHost(answers: Map<string, boolean>, throwOn: string | undefined)
{
	const log: string[] = [];

	const host = new Proxy({}, {
		get(_target, name)
		{
			const fragment = String(name);

			return (...args: any[]) =>
			{
				const call = fragment + "(" + args.map(canon).join(",") + ")";

				if (fragment.startsWith("g"))
				{
					const answer = answers.get(call);

					if (answer === undefined)
						throw new Error("No answer recorded for " + call);

					log.push(call + "=" + answer);

					return answer;
				}

				log.push(call);

				if (call === throwOn)
					throw new Error("thrown by " + call);

				return new Built(call);
			};
		},
	});

	return { host, render: canon, takeLog: () => log.splice(0) };
}
