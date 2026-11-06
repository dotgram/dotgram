#!/usr/bin/env python3
"""Validate the skills in the packages produced by the CI pack step."""

import posixpath
import re
import sys
import zipfile
from pathlib import Path
from urllib.parse import unquote, urlsplit


SKILLS = {
	"DotGram": "dotgram",
	"DotGram.Sql": "dotgram-sql",
	"DotGram.Web": "dotgram-web",
	"DotGram.Finance": "dotgram-finance",
	"DotGram.ExpressionLanguage": "dotgram-expression-language",
	"DotGram.Cli": "dotgram-cli",
}


def frontmatter(text, name):
	lines = text.splitlines()
	if not lines or lines[0] != "---" or "---" not in lines[1:]:
		raise ValueError("SKILL.md must start with YAML frontmatter without a BOM")

	fields = {}
	for line in lines[1:lines.index("---", 1)]:
		match = re.fullmatch(r"(name|description): (\S.*)", line)
		if not match or match[1] in fields:
			raise ValueError("frontmatter needs unique one-line name and description fields")
		value = match[2]
		# These skills use plain YAML strings, avoiding quoting and multiline scalar rules.
		if (": " in value or " #" in value or value.endswith(":") or "\t" in value
			or re.match(r"[\[\]{},&*#|>!%@`\"']|[-?:](?:\s|$)", value)
			or value.lower() in {"null", "~", "true", "false", "yes", "no", "on", "off", ".nan", ".inf", "-.inf", "+.inf"}
			or any(ord(char) < 32 or 127 <= ord(char) < 160 for char in value)
			or not re.search(r"[a-zA-Z]", value)):
			raise ValueError("frontmatter values must be plain YAML strings without ': ' or ' #'")
		fields[match[1]] = value

	if fields.get("name") != name or not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", name):
		raise ValueError("frontmatter name must match the skill folder")
	if not fields.get("description", "").strip() or len(fields["description"]) > 1024:
		raise ValueError("description must be present and at most 1024 characters")


def links(text):
	# Code samples are not Markdown links. Check inline links and images, reference
	# definitions (including collapsed references), autolinks and HTML href/src values.
	text = re.sub(r"^\s*(`{3,}|~{3,})[^\n]*\n.*?^\s*\1\s*$", "", text, flags=re.M | re.S)
	text = re.sub(r"(`+).*?\1", "", text, flags=re.S)
	destination = r'<([^>\n]+)>|((?:\\.|[^\s()<>]|\([^()\n]*\))+)'
	for match in re.finditer(r'!?\[[^\]\n]*\]\(\s*(?:' + destination + r')', text):
		yield match[1] or match[2]
	for match in re.finditer(r'^ {0,3}\[[^\]\n]+\]:\s*(?:' + destination + r')', text, re.M):
		yield match[1] or match[2]
	for match in re.finditer(r'<([^<>\s]+\.md(?:[?#][^<>\s]*)?)>', text):
		yield match[1]
	for match in re.finditer(r'\b(?:href|src)=[\"\']([^\"\']+)[\"\']', text):
		yield match[1]


def check_package(package, name, owners):
	with zipfile.ZipFile(package) as archive:
		entries = archive.namelist()
		files = {entry for entry in entries if not entry.endswith("/")}
		root = "skills/" + name + "/"
		entrypoint = root + "SKILL.md"
		if entries.count(entrypoint) != 1 or "SKILL.md" in files:
			raise ValueError("expected exactly one " + entrypoint + " and no package-root SKILL.md")
		if name in owners:
			raise ValueError("skill name is already owned by " + owners[name])
		owners[name] = str(package)

		for entry in files:
			if not entry.startswith("skills/"):
				continue
			if not entry.startswith(root) or posixpath.normpath(entry) != entry or "\\" in entry:
				raise ValueError("unexpected skill path " + entry)
			if not entry.endswith(".md"):
				continue
			data = archive.read(entry)
			if data.startswith(b"\xef\xbb\xbf"):
				raise ValueError(entry + " has a UTF-8 BOM")
			text = data.decode("utf-8")
			if entry == entrypoint:
				frontmatter(text, name)
			for link in links(text):
				url = urlsplit(link)
				if url.scheme or url.netloc:
					continue
				path = unquote(url.path)
				if not path:
					continue
				target = posixpath.normpath(posixpath.join(posixpath.dirname(entry), path))
				if "\\" in path or not target.startswith(root) or target not in files:
					raise ValueError(entry + ": relative link leaves the skill or does not exist: " + link)


def main():
	artifacts = Path(sys.argv[1] if len(sys.argv) > 1 else "artifacts")
	owners = {}
	for package_id, name in SKILLS.items():
		packages = list(artifacts.glob(package_id + ".[0-9]*.nupkg"))
		if len(packages) != 1:
			raise ValueError(package_id + ": expected exactly one packed version")
		check_package(packages[0], name, owners)
		print(package_id + ": skills/" + name + "/SKILL.md is valid")


if __name__ == "__main__":
	try:
		main()
	except (ValueError, UnicodeError, zipfile.BadZipFile) as error:
		sys.exit(str(error))
