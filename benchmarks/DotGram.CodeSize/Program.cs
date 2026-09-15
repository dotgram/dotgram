using System;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;

if (args.Length != 2)
	throw new ArgumentException("Usage: DotGram.CodeSize <before-repository> <after-repository>");

var roots = args.Select(Path.GetFullPath).ToArray();
foreach (var project in new[] { "DotGram.Sql", "DotGram.ExpressionLanguage" })
{
	var relative = $"src/{project}/obj/GeneratedFiles/DotGram/DotGram.Generation.GramGenerator";
	foreach (var file in Directory.GetFiles(Path.Combine(roots[1], relative), project + "*.g.cs"))
	{
		var name = Path.GetFileName(file);
		var measurements = roots.Select(root =>
		{
			var path = Path.Combine(root, relative, name);
			var text = File.ReadAllText(path);
			var normalized = text.Replace(root.Replace('\\', '/'), "/_").Replace(root.Replace('/', '\\'), "/_");
			return new { Bytes = new FileInfo(path).Length, NormalizedBytes = Encoding.UTF8.GetByteCount(normalized), Lines = File.ReadLines(path).Count() };
		}).ToArray();
		Console.WriteLine(JsonSerializer.Serialize(new { Kind = "Source", Name = name, Before = measurements[0], After = measurements[1] }));
	}
	foreach (var root in roots)
	{
		var path = Path.Combine(root, $"src/{project}/bin/Release/net10.0/{project}.dll");
		using var stream = File.OpenRead(path);
		using var pe = new PEReader(stream);
		var metadata = pe.GetMetadataReader();
		var groups = new Dictionary<string, (int Methods, long Bytes)>();
		foreach (var handle in metadata.TypeDefinitions)
		{
			var type = metadata.GetTypeDefinition(handle);
			var owner = type;
			string? variant = null;
			while (!owner.GetDeclaringType().IsNil)
			{
				var nestedName = metadata.GetString(owner.Name);
				if (nestedName is "Located" or "Immediate") variant = nestedName;
				owner = metadata.GetTypeDefinition(owner.GetDeclaringType());
			}
			var ownerName = metadata.GetString(owner.Namespace) + "." + metadata.GetString(owner.Name);
			if (variant is not null) ownerName += "." + variant;
			var counts = groups.GetValueOrDefault(ownerName);
			foreach (var methodHandle in type.GetMethods())
			{
				var method = metadata.GetMethodDefinition(methodHandle);
				if (method.RelativeVirtualAddress == 0) continue;
				counts.Methods++;
				counts.Bytes += pe.GetMethodBody(method.RelativeVirtualAddress).GetILContent().Length;
			}
			groups[ownerName] = counts;
		}
		Console.WriteLine(JsonSerializer.Serialize(new { Kind = "Assembly", Name = project, Variant = root == roots[0] ? "Before" : "After", FileBytes = stream.Length, ILBytes = groups.Values.Sum(x => x.Bytes), Methods = groups.Values.Sum(x => x.Methods), Parsers = groups.Where(x => x.Key.Contains("Parser", StringComparison.Ordinal)).Select(x => new { Name = x.Key, x.Value.Methods, ILBytes = x.Value.Bytes }) }));
	}
}
