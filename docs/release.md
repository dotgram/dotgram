# Releasing DotGram

Run the PowerShell commands below from the repository root. The release consists of
five NuGet packages and one Visual Studio extension, all at the version in
`Directory.Build.props`:

- `DotGram`
- `DotGram.Sql`
- `DotGram.Web`
- `DotGram.Finance`
- `DotGram.ExpressionLanguage`
- `DotGram.VisualStudio.vsix`

The `build` workflow builds and checks artifacts. Publication is a separate manual step.

## Prepare the version

For a maintenance release, start from the previous release tag and keep the release branch
to fixes, tests, documentation and packaging. New features stay on `main`. Any candidate
change after verification needs another successful CI run on the resulting commit.

1. Set `Version` in `Directory.Build.props`. The VSIX takes this version too.
2. Update the explicit versions in all package smoke projects, installation examples
   and the version-pinned links in `src/DotGram/SKILL.md`. Preserve previous release notes
   and shipped diagnostic history. Search for the previous version to find every occurrence.
3. Review each package's `Description`, `PackageReleaseNotes` and README, the VSIX
   release notes and Marketplace description, and the release notes under `docs/release-notes/`.
   Package README links must work from NuGet, including images.
4. Review `AnalyzerReleases.Shipped.md` and `AnalyzerReleases.Unshipped.md`. At 0.1.0
   the initial rules are already in the shipped file; subsequent releases must preserve
   their identifiers and record new or changed diagnostics.
5. Commit the preparation. The release candidate must have a clean working tree.

## Check the candidate

Follow [development.md](development.md) for local builds, tests and package smoke checks.
Before publication, require a successful `build` workflow on the candidate's exact commit:
both the Linux `build` job and the `windows` job must finish successfully, including pack,
package shape checks and the generator and all four library package smoke checks.

A push to `main` runs CI. A candidate on another branch can be checked with
`gh workflow run build.yml --ref release`; this uses the remote branch, so push the
candidate first. Find its run with:

```powershell
gh run list --workflow build.yml --limit 10 --json databaseId,headSha,status,conclusion,url
```

Record the chosen run ID and verify that its commit is the local candidate:

```powershell
$releaseVersion = '0.1.1'
$releaseTag = "v$releaseVersion"
$releaseCommit = git rev-parse HEAD
if ($LASTEXITCODE -ne 0) { throw 'Cannot resolve the candidate commit.' }
if (git status --porcelain) { throw 'The candidate working tree must be clean.' }
$releaseRun = Read-Host 'Successful build workflow run ID'
$releaseRunJson = gh run view $releaseRun --json headSha,status,conclusion
if ($LASTEXITCODE -ne 0) { throw 'Cannot read the workflow run.' }
$releaseResult = $releaseRunJson | ConvertFrom-Json
if ($releaseResult.headSha -ne $releaseCommit -or $releaseResult.conclusion -ne 'success') {
    throw 'The selected run must succeed on the candidate commit.'
}
$releaseDirectory = "artifacts/release-$releaseVersion-$releaseRun"
if (Test-Path $releaseDirectory) { throw 'Choose an empty artifact destination.' }
gh run download $releaseRun --name nupkg --dir "$releaseDirectory/nuget"
if ($LASTEXITCODE -ne 0) { throw 'Cannot download the NuGet artifacts.' }
gh run download $releaseRun --name vsix --dir "$releaseDirectory/vsix"
if ($LASTEXITCODE -ne 0) { throw 'Cannot download the VSIX artifact.' }
```

Inspect the downloaded packages' IDs, versions, READMEs, dependencies, licenses and
repository commit metadata, and the VSIX manifest's version. Expect exactly the five
packages above and one VSIX. Publish these downloaded files, retaining their SHA-256
hashes with the release record; rebuilding locally would produce different artifacts.

Install the downloaded VSIX and follow the [Playground checks](visual-studio.md#verify)
on Visual Studio 17.14 and 18. Record the actual VS versions and outcomes for standalone
grammars, embedded grammars, completion, diagnostics, navigation, references, Rename and
`StringSyntax` DSLs. Compilation and unit tests do not establish editor command routing.

CI runs separate consumers of SQL, Web, ExpressionLanguage and Finance on .NET 8 and
.NET 10, using an isolated NuGet cache. To repeat these checks against the downloaded
packages, follow the library package checks in [development.md](development.md) and
use the downloaded NuGet artifact directory as the local feed.

## Publish

Confirm that the NuGet account can publish each package ID and that the Marketplace
publisher is ready. Prepare the Marketplace screenshot and description before uploading.
The following commands create remote release state and publish packages.

Tag the checked commit and create a draft GitHub Release with the downloaded artifacts:

```powershell
git tag -a $releaseTag $releaseCommit -m "DotGram $releaseVersion"
if ($LASTEXITCODE -ne 0) { throw 'Cannot create the release tag.' }
git push origin "refs/tags/$releaseTag"
if ($LASTEXITCODE -ne 0) { throw 'Cannot push the release tag.' }
$releaseAssets = @(Get-ChildItem "$releaseDirectory/nuget/*.nupkg", "$releaseDirectory/vsix/*.vsix" -File)
gh release create $releaseTag @($releaseAssets.FullName) --verify-tag --draft --title "DotGram $releaseVersion" --notes-file "docs/release-notes/$releaseVersion.md"
if ($LASTEXITCODE -ne 0) { throw 'Cannot create the draft release.' }
```

Review the draft and its six attachments. Provide `NUGET_API_KEY` through the publishing
environment; do not put its value in a script or release record. Then push each package:

```powershell
if (-not $env:NUGET_API_KEY) { throw 'NUGET_API_KEY is required.' }
foreach ($releasePackage in Get-ChildItem "$releaseDirectory/nuget/*.nupkg" -File) {
    dotnet nuget push $releasePackage.FullName --source https://api.nuget.org/v3/index.json --api-key $env:NUGET_API_KEY
    if ($LASTEXITCODE -ne 0) { throw "Publication failed for $($releasePackage.Name)." }
}
```

Upload the same VSIX through the Visual Studio Marketplace publisher portal with
`src/DotGram.VisualStudio/README-Marketplace.md` and the actual editor screenshot.
If a publication fails partway through, record which items succeeded and resume only
the missing items after verifying their remote state. Do not replace a published version
with different bytes; a correction needs a new version.

## Verify and announce

- Confirm all five package pages show the intended version, README and dependencies.
- Restore from NuGet in a fresh consumer with an isolated cache and no local package feed.
  Exercise generation and a public parsing API from each library.
- Install the Marketplace VSIX and verify its version and a Playground interaction.
- Compare downloaded release files with the recorded hashes.
- Add the final package and Marketplace links to the GitHub Release, then publish the draft:
  `gh release edit $releaseTag --draft=false`.

Record the commit, CI run URL, artifact hashes, manual checks and publication links together.

Command references: [artifact download](https://cli.github.com/manual/gh_run_download),
[release creation](https://cli.github.com/manual/gh_release_create),
[NuGet push](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-nuget-push).
