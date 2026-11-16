// Writes ThirdPartyNotices.txt: every package of someone else's that the .vsix carries, with its
// licence and the notices it ships. The JavaScript is what esbuild put into dist/extension.js (its
// metafile names the files); the .NET is what the published server runs (server/*.deps.json), read
// from the NuGet packages folder the publish restored it into, its runtime among them where the server
// is published with one. Run after the server is published and the client bundled; build.sh and
// build.ps1 do. A package whose licence text cannot be found fails the build: the notices are complete
// or there is no package.

import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath, pathToFileURL } from 'node:url';

const scripts = path.dirname(fileURLToPath(import.meta.url));
const rule = '='.repeat(78);
const windows1252 = new TextDecoder('windows-1252');

// The notices already written, by their text: several packages of one vendor ship the same, written once.
const written = new Map();

/** The texts of the licences a package may name by expression alone (scripts/licenses, from SPDX). */
export function spdx(expression) {
    const file = path.join(scripts, 'licenses', `${expression}.txt`);

    return /^[A-Za-z0-9.+-]+$/.test(expression) && fs.existsSync(file) ? fs.readFileSync(file, 'utf8').trim() : undefined;
}

/** RTF read as the text it shows: control words and groups that are not text dropped, paragraphs kept. */
export function rtfText(rtf) {
    let text = '';
    let depth = 0;
    const skipped = [];

    for (let at = 0; at < rtf.length; at++) {
        const character = rtf[at];

        if (character === '{') {
            depth++;

            // A destination group ({\* ...}, the font and colour tables and the like) is not text.
            if (/^\{\\(\*|fonttbl|colortbl|stylesheet|info|generator)/.test(rtf.slice(at, at + 12))) {
                skipped.push(depth);
            }

            continue;
        }

        if (character === '}') {
            if (skipped.at(-1) === depth) {
                skipped.pop();
            }

            depth--;
            continue;
        }

        if (character === '\\') {
            const word = /^\\([a-z]+)(-?\d+)? ?|^\\'([0-9a-f]{2})|^\\([\\{}])/i.exec(rtf.slice(at));

            if (!word) {
                continue;
            }

            at += word[0].length - 1;

            if (skipped.length > 0) {
                continue;
            }

            if (word[3]) {
                // A byte of the document's code page, which RTF writers make Windows-1252.
                text += windows1252.decode(Uint8Array.of(parseInt(word[3], 16)));
            } else if (word[4]) {
                text += word[4];
            } else if (word[1] === 'par' || word[1] === 'line') {
                text += '\n';
            } else if (word[1] === 'tab') {
                text += '\t';
            }

            continue;
        }

        if (skipped.length === 0 && character !== '\r' && character !== '\n') {
            text += character;
        }
    }

    // Without the control characters a writer leaves in, a NUL at the end among them.
    return text.replace(/[\u0000-\u0008\u000b\u000c\u000e-\u001f]/g, '').replace(/[ \t]+\n/g, '\n').trim();
}

/** The licence and notice files a package folder carries, read as text. */
function files(folder, pattern) {
    if (!fs.existsSync(folder)) {
        return [];
    }

    return fs.readdirSync(folder)
        .filter((file) => pattern.test(file))
        .sort()
        .map((file) => {
            const text = fs.readFileSync(path.join(folder, file), 'utf8');

            return { file, text: (file.toLowerCase().endsWith('.rtf') ? rtfText(text) : text).trim() };
        });
}

const licences = /^(licen[cs]e|copying)(\.(md|txt))?$/i;
const noticeFiles = /^(third[-_]?party[-_]?notices?)(\.(md|txt|rtf))?$/i;

/** A package's section: its licence, then the notices it ships, each under the file's name. */
function section(title, licence, shipped) {
    const parts = [licence, ...shipped.map((notice) => {
        const first = written.get(notice.text);

        if (first) {
            return `--- ${notice.file} ---\n\nThe same as the notices under ${first} above.`;
        }

        written.set(notice.text, title);

        return `--- ${notice.file} ---\n\n${notice.text}`;
    })];

    return `${rule}\n${title}\n${rule}\n\n${parts.join('\n\n')}\n`;
}

/** The npm packages the bundle's files came from, each once. */
function npm(root) {
    const meta = JSON.parse(fs.readFileSync(path.join(root, 'dist', 'meta.json'), 'utf8'));
    const folders = new Map();

    for (const input of Object.keys(meta.inputs)) {
        const match = /^(.*node_modules\/(?:@[^/]+\/)?[^/]+)\//.exec(input.replace(/\\/g, '/'));

        if (match) {
            folders.set(match[1], path.join(root, match[1]));
        }
    }

    const result = [];
    const bundled = [...folders.values()]
        .map((folder) => ({ folder, manifest: JSON.parse(fs.readFileSync(path.join(folder, 'package.json'), 'utf8')) }))
        .sort((a, b) => ordinal(`${a.manifest.name} ${a.manifest.version}`, `${b.manifest.name} ${b.manifest.version}`));

    for (const { folder, manifest } of bundled) {
        const own = files(folder, licences);
        const licence = own.map((item) => item.text).join('\n\n') || (manifest.license && spdx(manifest.license));

        if (!licence) {
            throw new Error(`${manifest.name} ${manifest.version} is bundled, and neither carries a licence file nor names a licence whose text scripts/licenses has.`);
        }

        result.push(section(`${manifest.name} ${manifest.version} (npm, ${manifest.license})`, licence, files(folder, noticeFiles)));
    }

    return result;
}

/** Strings in the order of their UTF-16 code units, as on every machine alike. */
function ordinal(a, b) {
    return a < b ? -1 : a > b ? 1 : 0;
}

/** One NuGet package's section, from its folder in the packages folder. */
export function nugetSection(id, version, folder) {
    const nuspec = fs.existsSync(folder) ? fs.readdirSync(folder).find((file) => file.endsWith('.nuspec')) : undefined;

    if (!nuspec) {
        throw new Error(`${id} ${version} is published with the server and is not in ${folder}: restore it first.`);
    }

    const spec = fs.readFileSync(path.join(folder, nuspec), 'utf8');
    const expression = /<license\s+type="expression"\s*>([^<]+)<\/license>/.exec(spec)?.[1]?.trim();
    const declared = /<license\s+type="file"\s*>([^<]+)<\/license>/.exec(spec)?.[1]?.trim();
    const copyright = /<copyright>([^<]+)<\/copyright>/.exec(spec)?.[1]?.trim();
    let licence;

    if (declared) {
        const file = path.join(folder, declared.replace(/\\/g, '/'));

        if (!fs.existsSync(file)) {
            throw new Error(`${id} ${version} declares its licence in ${declared}, which the package does not have.`);
        }

        licence = fs.readFileSync(file, 'utf8').trim();
    } else if (expression) {
        // The expression's own text, where the package does not carry it: one expression, not a combination.
        licence = files(folder, licences).map((item) => item.text).join('\n\n') || spdx(expression);

        if (!licence) {
            throw new Error(`${id} ${version} is licensed under ${expression}, whose text the package does not carry and scripts/licenses does not have.`);
        }
    } else {
        licence = files(folder, licences).map((item) => item.text).join('\n\n');

        if (!licence) {
            throw new Error(`${id} ${version} names its licence only by a URL and carries no licence file.`);
        }
    }

    const heading = copyright && !licence.includes(copyright) ? `${copyright}\n\n${licence}` : licence;

    return section(`${id} ${version} (NuGet${expression ? `, ${expression}` : ''})`, heading, files(folder, noticeFiles));
}

/** The NuGet packages the published server loads, and its runtime where it carries one. */
function nuget(server, packages) {
    const deps = fs.readdirSync(server).find((file) => file.endsWith('.deps.json'));

    if (!deps) {
        throw new Error('No published server in server/: publish it first.');
    }

    const graph = JSON.parse(fs.readFileSync(path.join(server, deps), 'utf8'));
    const result = [];

    for (const [key, library] of Object.entries(graph.libraries).sort(([a], [b]) => ordinal(a, b))) {
        if (library.type !== 'package' && library.type !== 'runtimepack') {
            continue;
        }

        const slash = key.lastIndexOf('/');
        const [id, version] = [key.slice(0, slash), key.slice(slash + 1)];

        result.push(nugetSection(id, version, path.join(packages, library.path ?? `${id.toLowerCase()}/${version}`)));
    }

    return result;
}

/** The whole file's text. */
export function notices(root, packages) {
    const header = [
        'Third-party notices for the .Gram extension',
        '',
        'The extension carries the code of the packages below beside its own: the JavaScript bundled into',
        'dist/extension.js and the .NET assemblies of the language server in server/, each with its licence',
        'and the notices it ships. The .NET runtime the server runs on is listed where the extension carries',
        'it, built for one platform; otherwise the .NET Install Tool (ms-dotnettools.vscode-dotnet-runtime),',
        'an extension of its own, acquires it on the machine under its own licence.',
        '',
    ];

    return [...header, ...npm(root), ...nuget(path.join(root, 'server'), packages)].join('\n');
}

if (process.argv[1] && import.meta.url === pathToFileURL(path.resolve(process.argv[1])).href) {
    const root = path.resolve(scripts, '..');
    const packages = process.env.NUGET_PACKAGES || path.join(os.homedir(), '.nuget', 'packages');
    const text = notices(root, packages);

    fs.writeFileSync(path.join(root, 'ThirdPartyNotices.txt'), text);
    console.log(`ThirdPartyNotices.txt: ${(text.match(new RegExp(`^${rule}\\n[^\\n]+\\n${rule}$`, 'gm')) ?? []).length} packages.`);
}
