import { readFile } from "node:fs/promises";
import { fileURLToPath, pathToFileURL } from "node:url";

const loaded = await import(pathToFileURL(process.env.DOTGRAM_IR_AMARO).href);
const amaro = loaded.default ?? loaded;

export async function load(url, context, nextLoad)
{
	if (!url.endsWith(".ts"))
		return nextLoad(url, context);

	const source = await readFile(fileURLToPath(url), "utf8");
	const { code } = amaro.transformSync(source, { mode: "strip-only" });

	return { format: "module", source: code, shortCircuit: true };
}
