// What notices.mjs writes of a package, on packages made up for it: the licence it carries or the text
// of the one it names, the notices it ships (an RTF one read as text), and a failure wherever a
// licence's text is missing. Run by `npm run test:notices`, which build.sh and build.ps1 run before
// they write the notices.

import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import test from 'node:test';

import { nugetSection, rtfText, spdx } from './notices.mjs';

function nugetPackage(nuspec, files = {}) {
    const folder = fs.mkdtempSync(path.join(os.tmpdir(), 'dotgram-notices-'));

    fs.writeFileSync(path.join(folder, 'package.nuspec'), `<package><metadata>${nuspec}</metadata></package>`);

    for (const [name, text] of Object.entries(files)) {
        fs.writeFileSync(path.join(folder, name), text);
    }

    return folder;
}

test('a package licensed by an expression it carries no text of gets the expression\'s text', () => {
    const folder = nugetPackage('<license type="expression">Apache-2.0</license><copyright>Copyright Somebody</copyright>');
    const text = nugetSection('Some.Package', '1.0.0', folder);

    assert.match(text, /Some\.Package 1\.0\.0 \(NuGet, Apache-2\.0\)/);
    assert.match(text, /Copyright Somebody/);
    assert.match(text, /Apache License\s+Version 2\.0, January 2004/);
});

test('the notices a package ships are written after its licence, an RTF one as text', () => {
    const rtf = '{\\rtf1\\ansi{\\fonttbl{\\f0 Tahoma;}}{\\*\\generator Riched20}\\pard THIRD-PARTY SOFTWARE NOTICES\\par\\par Copyright (c) .NET Foundation and Contributors\\par}';
    const folder = nugetPackage('<license type="expression">MIT</license>', { 'ThirdPartyNotices.rtf': rtf, 'THIRD-PARTY-NOTICES.TXT': 'Plain notice' });
    const text = nugetSection('Roslyn.Like', '4.14.0', folder);

    assert.match(text, /--- THIRD-PARTY-NOTICES\.TXT ---\n\nPlain notice/);
    assert.match(text, /--- ThirdPartyNotices\.rtf ---\n\nTHIRD-PARTY SOFTWARE NOTICES\n\nCopyright \(c\) \.NET Foundation and Contributors/);
    assert.doesNotMatch(text, /Tahoma|Riched20|\\par/);
});

test('a licence file the package declares and does not have fails the build', () => {
    const folder = nugetPackage('<license type="file">LICENSE.txt</license><licenseUrl>https://example.com/licence</licenseUrl>');

    assert.throws(() => nugetSection('Missing.File', '1.0.0', folder), /declares its licence in LICENSE\.txt/);
});

test('an expression with no text the package carries or scripts/licenses has fails the build', () => {
    const folder = nugetPackage('<license type="expression">LGPL-2.1-only</license>');

    assert.throws(() => nugetSection('Odd.Licence', '1.0.0', folder), /LGPL-2\.1-only, whose text/);
});

test('a licence named only by a URL fails the build where the package carries no licence file', () => {
    const folder = nugetPackage('<licenseUrl>https://example.com/licence</licenseUrl>');

    assert.throws(() => nugetSection('Url.Only', '1.0.0', folder), /only by a URL/);
});

test('the licence texts kept are SPDX\'s, and only those', () => {
    for (const id of ['MIT', 'Apache-2.0', 'BSD-2-Clause', 'BSD-3-Clause', 'ISC', '0BSD']) {
        assert.ok(spdx(id), id);
    }

    assert.equal(spdx('../package'), undefined);
    assert.equal(rtfText('{\\rtf1 a\\\'e9b}'), 'aéb');
});
