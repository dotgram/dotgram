// Writes ThirdPartyNotices.txt: every package of someone else's that the .vsix carries, with its
// licence. The JavaScript is what esbuild put into dist/extension.js (its metafile names the files);
// the .NET is what the published server runs (server/*.deps.json), its licences read from the NuGet
// packages folder the publish restored them into. Run after the server is published and the client
// bundled; build.sh and build.ps1 do.

import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const here = path.resolve(path.dirname(fileURLToPath(import.meta.url)), '..');
const out = path.join(here, 'ThirdPartyNotices.txt');
const sections = [];

// The MIT licence's text, which a package licensed by the expression alone does not carry itself.
const mit = (copyright) => `${copyright}

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
associated documentation files (the "Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the
following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO
EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.`;

function licenceFile(folder) {
    if (!fs.existsSync(folder)) {
        return undefined;
    }

    const name = fs.readdirSync(folder).find((file) => /^(licen[cs]e|copying)(\.(md|txt))?$/i.test(file));

    return name ? fs.readFileSync(path.join(folder, name), 'utf8').trim() : undefined;
}

// JavaScript: each package a file of the bundle came from, once.
const meta = JSON.parse(fs.readFileSync(path.join(here, 'dist', 'meta.json'), 'utf8'));
const packages = new Map();

for (const input of Object.keys(meta.inputs)) {
    const match = /^(.*node_modules\/(?:@[^/]+\/)?[^/]+)\//.exec(input.replace(/\\/g, '/'));

    if (match) {
        packages.set(match[1], path.join(here, match[1]));
    }
}

const scripts = [];

for (const folder of packages.values()) {
    const manifest = JSON.parse(fs.readFileSync(path.join(folder, 'package.json'), 'utf8'));
    const text = licenceFile(folder);

    if (!text) {
        throw new Error(`${manifest.name} ${manifest.version} is bundled and carries no licence file.`);
    }

    scripts.push({ title: `${manifest.name} ${manifest.version} (npm, ${manifest.license})`, text });
}

sections.push(...scripts.sort((a, b) => a.title.localeCompare(b.title, 'en')));

// .NET: the packages the published server loads, and the runtime where it is published with one.
const server = path.join(here, 'server');
const deps = fs.readdirSync(server).find((file) => file.endsWith('.deps.json'));

if (!deps) {
    throw new Error('No published server in server/: publish it first.');
}

const graph = JSON.parse(fs.readFileSync(path.join(server, deps), 'utf8'));
const nuget = process.env.NUGET_PACKAGES || path.join(os.homedir(), '.nuget', 'packages');
const runtimePacks = [];

for (const [key, library] of Object.entries(graph.libraries)) {
    const slash = key.lastIndexOf('/');
    const [id, version] = [key.slice(0, slash), key.slice(slash + 1)];

    if (library.type === 'runtimepack') {
        runtimePacks.push(`${id} ${version}`);
        continue;
    }

    if (library.type !== 'package') {
        continue;
    }

    const folder = path.join(nuget, library.path ?? `${id.toLowerCase()}/${version}`);
    const nuspec = fs.existsSync(folder) ? fs.readdirSync(folder).find((file) => file.endsWith('.nuspec')) : undefined;
    const spec = nuspec ? fs.readFileSync(path.join(folder, nuspec), 'utf8') : '';
    const expression = /<license\s+type="expression">([^<]+)<\/license>/.exec(spec)?.[1];
    const file = /<license\s+type="file">([^<]+)<\/license>/.exec(spec)?.[1];
    const url = /<licenseUrl>([^<]+)<\/licenseUrl>/.exec(spec)?.[1];
    const copyright = /<copyright>([^<]+)<\/copyright>/.exec(spec)?.[1]?.trim()
        ?? `Copyright (c) ${/<authors>([^<]+)<\/authors>/.exec(spec)?.[1]?.trim() ?? id}`;
    const text = (file && fs.existsSync(path.join(folder, file)) ? fs.readFileSync(path.join(folder, file), 'utf8').trim() : undefined)
        ?? licenceFile(folder);

    if (!expression && !text && !url) {
        throw new Error(`${id} ${version} is published with the server and its licence was not found in ${folder}.`);
    }

    sections.push({
        title: `${id} ${version} (NuGet${expression ? `, ${expression}` : ''})`,
        text: text ?? (expression === 'MIT'
            ? mit(copyright)
            : expression ? `${copyright}\n\nLicensed under ${expression}: https://spdx.org/licenses/${expression}.html` : `${copyright}\n\nLicence: ${url}`),
    });
}

if (runtimePacks.length > 0) {
    sections.push({
        title: `.NET runtime (${runtimePacks.join(', ')}, MIT)`,
        text: 'The .NET runtime is licensed under the MIT licence; its own third-party notices are\n' +
            'https://github.com/dotnet/runtime/blob/main/THIRD-PARTY-NOTICES.TXT',
    });
}

const header = [
    'Third-party notices for the .Gram extension',
    '',
    'The extension carries the code of the packages below beside its own: the JavaScript bundled into',
    'dist/extension.js and the .NET assemblies of the language server in server/. The .NET runtime the',
    'server runs on is not carried unless the extension was built for one platform; otherwise it is',
    'acquired on the machine through the .NET Install Tool (ms-dotnettools.vscode-dotnet-runtime), an',
    'extension of its own, under its own licence.',
    '',
];

const body = sections.map((section) => `${'='.repeat(78)}\n${section.title}\n${'='.repeat(78)}\n\n${section.text}\n`);

fs.writeFileSync(out, [...header, ...body].join('\n'));
console.log(`ThirdPartyNotices.txt: ${sections.length} packages.`);
