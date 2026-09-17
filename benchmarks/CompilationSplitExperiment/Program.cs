using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

if (args.Length == 0 || args[0] == "--help")
{
	Console.WriteLine("Run from the repository root: prepare | original RUN | split RUN. Outputs: .work/file-split");
	return;
}

try
{
	var repo = Directory.GetCurrentDirectory();
	var work = Path.Combine(repo, ".work/file-split");
	var project = Path.Combine(repo, "examples/DotGram.Examples");
	var parse = new CSharpParseOptions(LanguageVersion.Preview, preprocessorSymbols: ["DEBUG", "TRACE", "NET10_0", "NET10_0_OR_GREATER", "NET", "NETCOREAPP"]);
	var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
		.Where(p => Path.GetFileName(p).StartsWith("System") || Path.GetFileName(p) is "netstandard.dll" or "mscorlib.dll" or "Microsoft.CSharp.dll")
		.Append(Path.Combine(repo, "src/DotGram.Finance/bin/Release/net10.0/DotGram.Finance.dll"))
		.Select(p => MetadataReference.CreateFromFile(p)).ToArray();
	var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Debug,
		concurrentBuild: true, deterministic: true, nullableContextOptions: NullableContextOptions.Enable);
	if (args[0] == "prepare")
	{
		var original = Path.Combine(work,"original");
		if (Directory.Exists(original) && Directory.EnumerateFileSystemEntries(original).Any()) throw new InvalidOperationException("Preparation requires a fresh .work/file-split directory; preserve or move previous results first.");
		Directory.CreateDirectory(original);
		var split = Path.Combine(work,"split"); Directory.CreateDirectory(split);
		var files = Directory.EnumerateFiles(project,"*.cs",SearchOption.AllDirectories)
			.Where(p => !p.Contains("\\obj\\") && !p.Contains("\\bin\\")).ToList();
		files.Add(Path.Combine(project,"obj/Release/net10.0/DotGram.Examples.GlobalUsings.g.cs"));
		files.Add(Path.Combine(project,"obj/Release/net10.0/DotGram.Examples.AssemblyInfo.cs"));
		var trees = files.Select(p => CSharpSyntaxTree.ParseText(File.ReadAllText(p),parse,p,Encoding.UTF8)).ToArray();
		var compilation = CSharpCompilation.Create("DotGram.Examples",trees,references,options);
		var additional = Directory.EnumerateFiles(project,"*.gram",SearchOption.AllDirectories)
			.Where(p=>!p.Contains("\\obj\\")&&!p.Contains("\\bin\\")).Select(p=>(AdditionalText)new Input(p)).ToImmutableArray();
		GeneratorDriver driver = CSharpGeneratorDriver.Create([new GramGenerator().AsSourceGenerator()],additional,parse);
		driver = driver.RunGenerators(compilation);
		var result = driver.GetRunResult();
		var errors = result.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error).ToArray();
		if(errors.Length>0) throw new Exception(string.Join("\n",errors.Select(d=>d.ToString())));
		var all = trees.Select((t,i)=>(Name:$"source-{i:D3}.cs", Text:t.ToString()))
			.Concat(result.GeneratedTrees.Select((t,i)=>(Name:Path.GetFileName(t.FilePath),Text:t.ToString()))).ToArray();
		foreach(var file in all) { File.WriteAllText(Path.Combine(original,file.Name),file.Text,new UTF8Encoding(false)); File.WriteAllText(Path.Combine(split,file.Name),file.Text,new UTF8Encoding(false)); }
		var target = all.Single(f=>f.Name=="DotGram.Examples.Finance.Fix44Grammar.g.cs");
		var root = CSharpSyntaxTree.ParseText(target.Text,parse).GetCompilationUnitRoot();
		var ns = (NamespaceDeclarationSyntax)root.Members.Single();
		var type = (ClassDeclarationSyntax)ns.Members.Single();
		if(type.AttributeLists.Count!=0 || type.BaseList!=null) throw new Exception("Unexpected class header");
		var methods = type.Members.OfType<MethodDeclarationSyntax>().ToArray();
		var keep = new StringBuilder(target.Text[..type.OpenBraceToken.FullSpan.End]);
		foreach(var member in type.Members.Where(m=>m is not MethodDeclarationSyntax)) keep.Append(member.ToFullString());
		keep.Append(target.Text[type.CloseBraceToken.FullSpan.Start..]);
		File.WriteAllText(Path.Combine(split,target.Name),keep.ToString(),new UTF8Encoding(false));
		var header=target.Text[..type.OpenBraceToken.FullSpan.End];
		var footer=target.Text[type.CloseBraceToken.FullSpan.Start..];
		var batch=new StringBuilder(); var part=0; var sizes=new List<int>();
		void Flush() { if(batch.Length==0)return; var text=header+batch+footer; File.WriteAllText(Path.Combine(split,$"Fix44.part-{part++:D3}.g.cs"),text,new UTF8Encoding(false)); sizes.Add(text.Length); batch.Clear(); }
		foreach(var method in methods) { if(batch.Length>0 && batch.Length+method.FullSpan.Length>2_000_000)Flush(); batch.Append(method.ToFullString()); }
		Flush();
		string Hash(string text)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
		var originalHashes=methods.Select(m=>Hash(m.ToFullString())).Order().ToArray();
		var splitHashes=Directory.EnumerateFiles(split,"Fix44.part-*.g.cs").SelectMany(p=>CSharpSyntaxTree.ParseText(File.ReadAllText(p),parse).GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First(c=>c.Identifier.ValueText=="Fix44Grammar").Members.OfType<MethodDeclarationSyntax>()).Select(m=>Hash(m.ToFullString())).Order().ToArray();
		if(!originalHashes.SequenceEqual(splitHashes))throw new Exception("Method text changed");
		File.WriteAllText(Path.Combine(work,"inventory.json"),JsonSerializer.Serialize(new {originalFiles=all.Length,originalBytes=Encoding.UTF8.GetByteCount(target.Text),methodCount=methods.Length,parts=part,partCharacters=sizes,methodTextIdentical=true,largestMethods=methods.OrderByDescending(m=>m.FullSpan.Length).Take(12).Select(m=>new{name=m.Identifier.ValueText,characters=m.FullSpan.Length})},new JsonSerializerOptions{WriteIndented=true}));
		Console.WriteLine(File.ReadAllText(Path.Combine(work,"inventory.json")));
		return;
	}
	var variant=args[0]; var run=args[1];
	var sourceFiles=Directory.EnumerateFiles(Path.Combine(work,variant),"*.cs").Order().ToArray();
	var texts=sourceFiles.Select(p=>(Path:p,Text:SourceText.From(File.ReadAllText(p),Encoding.UTF8))).ToArray();
	GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
	var allocated=GC.GetTotalAllocatedBytes(true); var watch=Stopwatch.StartNew();
	var syntax=new SyntaxTree[texts.Length];
	Parallel.For(0,texts.Length,i=>syntax[i]=CSharpSyntaxTree.ParseText(texts[i].Text,parse,texts[i].Path));
	var parseMs=watch.Elapsed.TotalMilliseconds;
	var unit=CSharpCompilation.Create("DotGram.Examples",syntax,references,options);
	var output=Path.Combine(work,variant+"-"+run+".dll");
	using(var stream=File.Create(output))
	{
		var emitted=unit.Emit(stream,options:new EmitOptions(debugInformationFormat:DebugInformationFormat.Embedded));
		if(!emitted.Success){ foreach(var error in emitted.Diagnostics.Where(d=>d.Severity==DiagnosticSeverity.Error).Take(20))Console.Error.WriteLine(error); Environment.Exit(1); }
	}
	watch.Stop();
	Console.WriteLine(JsonSerializer.Serialize(new {variant,run,files=texts.Length,parseMs,totalMs=watch.Elapsed.TotalMilliseconds,allocatedBytes=GC.GetTotalAllocatedBytes(true)-allocated,peakWorkingSet=Process.GetCurrentProcess().PeakWorkingSet64,assemblyBytes=new FileInfo(output).Length}));
}
catch (Exception error)
{
	Console.Error.WriteLine(error.ToString());
	Environment.ExitCode = 1;
}
sealed class Input(string path):AdditionalText
{
	public override string Path=>path;
	public override SourceText GetText(CancellationToken cancellationToken=default)=>SourceText.From(File.ReadAllText(path),Encoding.UTF8);
}
