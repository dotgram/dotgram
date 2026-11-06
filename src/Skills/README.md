# Package skills

This page is for maintainers and is not packed. Each subfolder holds the skill for
one main package. Its folder name equals the `name` in its `SKILL.md` frontmatter.
The project packs only that folder's Markdown files under `skills/<name>/`.

| Package | Skill folder |
| --- | --- |
| DotGram | `dotgram/` |
| DotGram.Sql | `dotgram-sql/` |
| DotGram.Web | `dotgram-web/` |
| DotGram.Finance | `dotgram-finance/` |
| DotGram.ExpressionLanguage | `dotgram-expression-language/` |
| DotGram.Cli | `dotgram-cli/` |

The package roots retain their consumer README pages, not copies of the skills.
DotGram.Cli also embeds its skill for `dotnet dotgram --skill`; this is the same
source, not a second package skill.

Consumers read a skill from `<global-packages>/<id>/<version>/skills/<name>/` or
copy the whole skill folder into `.agents/skills/<name>/` or `.claude/skills/<name>/`.
Do not create or write `.dotnet-package-skills.json`; that ownership file belongs
to the installation tool.

## Rules for skill content

- A skill works from both locations above. Links between its files are relative
  and stay inside its own named folder. Do not link to package-root paths such as
  `lib/<tfm>/DotGram.Sql.xml` or repository paths such as `src/...`. When a skill
  uses the package's XML reference, `SKILL.md` explains once where that XML file
  is in the installed package; task guides refer to it by name.
- Only the main package for a product carries its skill. Two packages never ship
  the same skill name.
- `SKILL.md` frontmatter has a `name` equal to its folder name. Its `description`
  says what it covers and when to use it, on one line, at most 1024 characters,
  without `: ` or ` #` sequences. Use plain YAML strings for these two fields.
- Optional task guides live under `docs/`. Write for someone opening one guide for
  one task. Start each guide with a one-line relative pointer to `SKILL.md` and a
  "You are here if" block. State rules plainly, without banners, capitals or
  instructions that make agents read every guide for every task.
- Comments and names written into a consumer's code are ordinary code text,
  without notes about the agent that wrote them.
- Keep guidance versioned with the code it describes. A change to a documented
  public API, provider capability, translator or default updates the guide in the
  same PR.
- Every file under a named skill folder ships to consumers. Keep maintainer
  prompts, roadmaps, test prompts and internal notes outside those folders.
- Files are UTF-8 without BOM; line endings follow `.gitattributes`.

## Packing and checking

Each package project includes `..\Skills\<name>\**\*.md` with `Pack="true"` and
`PackagePath="skills/<name>/%(RecursiveDir)%(Filename)%(Extension)"`. This README
sits outside those globs. `Directory.Build.targets` pins repository links to the
package's version in the packed copies, including task guides.

Build with `MSBUILDDISABLENODEREUSE=1 dotnet build DotGram.slnx -c Linux -nodeReuse:false`
on Linux, then pack each of the six projects with
`dotnet pack src/<package>/<package>.csproj -c Release --no-build -nodeReuse:false -o artifacts`,
also with `MSBUILDDISABLENODEREUSE=1` exported.
Run `.github/scripts/check-packages.sh` from the repository root. Both build and
publish CI run this after packing. It checks the actual archives for skill paths,
frontmatter, unique names, BOMs and relative links, and verifies that packed pages
match their sources apart from version-pinned repository links. The checker also
runs regression fixtures for invalid frontmatter, old root copies and broken or
escaping links. To run only the skill checks, use
`python3 .github/scripts/check-skills.py artifacts`.
