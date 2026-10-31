// The extension in a real VS Code: downloads one, installs the .NET Install Tool the extension
// depends on, opens a fixture folder and runs the suite (docs/vscode.md).

import * as cp from 'child_process';
import * as path from 'path';
import { downloadAndUnzipVSCode, resolveCliArgsFromVSCodeExecutablePath, runTests } from '@vscode/test-electron';

async function main(): Promise<void> {
    const extensionDevelopmentPath = path.resolve(__dirname, '..', '..');
    const extensionTestsPath = path.resolve(__dirname, 'suite', 'index');
    const workspace = path.resolve(extensionDevelopmentPath, 'test-fixture');
    const executable = await downloadAndUnzipVSCode('stable');
    const [cli, ...args] = resolveCliArgsFromVSCodeExecutablePath(executable);

    const installed = cp.spawnSync(cli, [...args, '--install-extension', 'ms-dotnettools.vscode-dotnet-runtime'], {
        stdio: 'inherit',
        shell: process.platform === 'win32',
    });

    if (installed.error || installed.status !== 0) {
        throw new Error(`The .NET Install Tool could not be installed: ${installed.error ?? `exit code ${installed.status}`}`);
    }

    await runTests({
        vscodeExecutablePath: executable,
        extensionDevelopmentPath,
        extensionTestsPath,
        launchArgs: [workspace, ...args.filter((arg) => arg.startsWith('--extensions-dir') || arg.startsWith('--user-data-dir'))],
    });
}

main().catch((error) => {
    console.error(error);
    process.exit(1);
});
