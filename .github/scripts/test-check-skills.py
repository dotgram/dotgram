#!/usr/bin/env python3
"""Exercise the package gate with malformed archives as well as valid skills."""

import importlib.util
import sys
import tempfile
import unittest
import zipfile
from pathlib import Path


sys.dont_write_bytecode = True

spec = importlib.util.spec_from_file_location("check_skills", Path(__file__).with_name("check-skills.py"))
checker = importlib.util.module_from_spec(spec)
spec.loader.exec_module(checker)


class SkillPackageTests(unittest.TestCase):
	def package(self, files, owners=None):
		with tempfile.TemporaryDirectory() as directory:
			package = Path(directory) / "Example.1.0.0.nupkg"
			with zipfile.ZipFile(package, "w") as archive:
				for path, text in files.items():
					archive.writestr(path, text)
			checker.check_package(package, "example", {} if owners is None else owners)

	def files(self, body="", header="name: example\ndescription: Read examples when building a parser."):
		return {"skills/example/SKILL.md": "---\n" + header + "\n---\n" + body}

	def test_links_work_in_package_and_copied_skill(self):
		files = self.files("[guide](docs/task.md#task)\n[guide][task]\n[task]: <docs/task.md>\n")
		files["skills/example/docs/task.md"] = "[entry](../SKILL.md)\n![image](image%20file.md)\n"
		files["skills/example/docs/image file.md"] = "Example.\n"
		self.package(files)

	def test_frontmatter_and_encoding(self):
		for header in (
			"name: other\ndescription: Read examples.",
			"name: example",
			"name: example\ndescription: ",
			"name: example\ndescription: " + "x" * 1025,
			"name: example\ndescription: Read: examples.",
			"name: example\ndescription: Read # examples.",
			"name: example\ndescription: Read examples:",
			"name: example\ndescription: Read\n  examples.",
			"name: example\ndescription: Read\ndescription: Examples.",
			"name: example\ndescription: [Examples]",
			"name: example\ndescription: Read\x00examples.",
		):
			with self.subTest(header=header), self.assertRaises(ValueError):
				self.package(self.files(header=header))
		with self.assertRaises(ValueError):
			self.package({"skills/example/SKILL.md": "\ufeff---\nname: example\ndescription: Read examples.\n---\n"})
		self.package(self.files(header="name: example\ndescription: " + "x" * 1024))

	def test_root_copy_and_shared_name_are_rejected(self):
		files = self.files()
		files["SKILL.md"] = files["skills/example/SKILL.md"]
		with self.assertRaises(ValueError):
			self.package(files)
		with self.assertRaises(ValueError):
			self.package({"SKILL.md": "Example."})
		with self.assertRaises(ValueError):
			self.package(self.files(), {"example": "Other.1.0.0.nupkg"})

	def test_missing_or_escaping_links_are_rejected(self):
		for body in (
			"[missing](docs/task.md)",
			"[outside](../../README.md)",
			"[outside](%2e%2e/%2e%2e/README.md)",
			"[outside](/README.md)",
			"[outside](../other/SKILL.md)",
			"[reference][guide]\n[guide]: ../README.md",
			'<a href="../README.md">outside</a>',
		):
			with self.subTest(body=body), self.assertRaises(ValueError):
				self.package(self.files(body))

	def test_external_links_anchors_and_code_samples(self):
		self.package(self.files(
			"[web](https://example.org/guide.md) [anchor](#examples)\n"
			"`[example](missing.md)`\n```markdown\n[example](missing.md)\n```\n"
			"~~~markdown\n[example](missing.md)\n~~~\n"))


if __name__ == "__main__":
	unittest.main()
