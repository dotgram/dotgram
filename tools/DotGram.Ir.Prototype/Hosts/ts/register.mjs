// For a node that was built without its own TypeScript support (a distribution's package may
// be): registers the hook beside this file, which strips the types with the amaro package that
// DOTGRAM_IR_AMARO points at. A node that strips types itself needs neither file.
import { register } from "node:module";

register("./strip-hooks.mjs", import.meta.url);
