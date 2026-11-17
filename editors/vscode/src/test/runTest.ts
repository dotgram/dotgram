// The packaged extension in a real VS Code: the .vsix the build wrote is installed into an extensions
// folder of the run's own, beside the .NET Install Tool it depends on, and the suite runs against that
// installation, so that what packaging leaves out fails here (docs/vscode.md). VS Code and the tool are
// pinned: an unchanged commit tests the same thing, and the VS Code it tests is the oldest the
// manifest accepts (engines.vscode), with the newest .NET Install Tool that VS Code can run. A run may
// install the C# extension beside them (DOTGRAM_SMOKE_CSHARP, ms-dotnettools.csharp@<version>), to see
// the grammars in a C# document painted over its colours.

import * as cp from 'child_process';
import * as fs from 'fs';
import * as path from 'path';
import { downloadAndUnzipVSCode, resolveCliArgsFromVSCodeExecutablePath, runTests } from '@vscode/test-electron';

const vscodeVersion = process.env.DOTGRAM_SMOKE_VSCODE || '1.90.0';
const runtimeTool = process.env.DOTGRAM_SMOKE_RUNTIME_TOOL || 'ms-dotnettools.vscode-dotnet-runtime@2.3.2';
const csharp = process.env.DOTGRAM_SMOKE_CSHARP || '';

async function main(): Promise<void> {
    const root = path.resolve(__dirname, '..', '..');
    const manifest = JSON.parse(fs.readFileSync(path.join(root, 'package.json'), 'utf8')) as { name: string; version: string };
    const vsix = process.env.DOTGRAM_VSIX || path.resolve(root, '..', '..', 'artifacts', `${manifest.name}-${manifest.version}.vsix`);

    if (!fs.existsSync(vsix)) {
        throw new Error(`No package at ${vsix}: build it first (build.sh or build.ps1).`);
    }

    const executable = await downloadAndUnzipVSCode(vscodeVersion);
    const [cli, ...args] = resolveCliArgsFromVSCodeExecutablePath(executable);
    const run = path.join(root, '.vscode-test', 'smoke');
    const extensions = path.join(run, 'extensions');
    const folders = [`--extensions-dir=${extensions}`, `--user-data-dir=${path.join(run, 'user-data')}`];

    fs.rmSync(run, { recursive: true, force: true });

    // The tool first, at its pinned version: installing the package would otherwise fetch its newest.
    for (const extension of [runtimeTool, ...(csharp ? [csharp] : []), vsix]) {
        const installed = cp.spawnSync(cli, [...args.filter((arg) => !arg.startsWith('--extensions-dir') && !arg.startsWith('--user-data-dir')), ...folders, '--install-extension', extension], {
            stdio: 'inherit',
            shell: process.platform === 'win32',
        });

        if (installed.error || installed.status !== 0) {
            throw new Error(`${extension} could not be installed: ${installed.error ?? `exit code ${installed.status}`}`);
        }
    }

    await runTests({
        vscodeExecutablePath: executable,
        // A host of the suite's own: the extension under test is the installed package, not this folder.
        extensionDevelopmentPath: path.join(root, 'test-host'),
        extensionTestsPath: path.resolve(__dirname, 'suite', 'index'),
        extensionTestsEnv: { DOTGRAM_SMOKE_EXTENSIONS: extensions, DOTGRAM_SMOKE_CSHARP: csharp },
        launchArgs: [path.join(root, 'test-fixture'), ...folders],
    });
}

main().catch((error) => {
    console.error(error);
    process.exit(1);
});
