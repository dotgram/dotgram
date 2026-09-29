using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;

using DotGram.ExpressionLanguage;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests.ExpressionLanguage;

/// <summary>
/// Two referenced assemblies declaring one full name as two different types: the name is
/// ambiguous where it is used, as C# says it is (CS0433).
/// </summary>
/// <remarks>
/// <para>
/// Until 2026-09-29 the first assembly that answered won, so what a text meant depended on the
/// order the scope's references were walked in. Now every assembly is asked, and two different
/// types refuse the name, with a message naming both assemblies.
/// </para>
/// <para>
/// Three things that are NOT that ambiguity are held here too: one type answered by several
/// assemblies, as a facade forwards <c>System.Object</c>; the calling assembly's own type, which
/// wins over a reference's (CS0436, a warning in C#, silent here); and two versions of one
/// assembly side by side, which are two types and ambiguous, as Roslyn says.
/// </para>
/// <para>
/// Every case is held against Roslyn compiling the same expression over the same references,
/// as <see cref="NameOrderTests"/> holds the order of names. The libraries are compiled in memory
/// and loaded into contexts of their own, so that nothing but the test can find them.
/// </para>
/// </remarks>
public sealed class AssemblyAmbiguityTests
{
	[Theory]
	[InlineData("N.T.Value", "")]
	[InlineData("T.Value", "using N;")]
	[InlineData("N.T.Inner.Value", "")]
	public void A_name_two_references_declare_as_two_types_is_ambiguous(string expression, string usings)
	{
		var first  = Library("A", 1);
		var second = Library("B", 2);
		var caller = Caller(Probe, first, second);

		Assert.Equal("CS0433", CSharp(expression, usings, "", first, second));

		var refused = Refused($"{usings} () => {expression}", ResolutionScope.Around(caller));

		Assert.Contains("'N.T' exists in both", refused, StringComparison.Ordinal);
		Assert.Contains("'A, Version=", refused, StringComparison.Ordinal);
		Assert.Contains("'B, Version=", refused, StringComparison.Ordinal);
	}

	[Fact]
	public void A_generic_type_is_named_as_CSharp_writes_it()
	{
		var first  = Library("A", 1);
		var second = Library("B", 2);
		var caller = Caller(Probe, first, second);

		Assert.Equal("CS0433", CSharp("N.G<int>.Value", "", "", first, second));
		Assert.Contains("The type 'N.G<X>' exists in both", Refused("() => N.G<int>.Value", ResolutionScope.Around(caller)), StringComparison.Ordinal);

		Assert.Equal(
			"System.Collections.Generic.Dictionary<TKey, TValue>.Enumerator",
			ExpressionParser.Spelled(typeof(Dictionary<,>.Enumerator)));
	}

	[Fact]
	public void The_answer_does_not_depend_on_the_order_of_the_references()
	{
		var first  = Load(Library("A", 1));
		var second = Load(Library("B", 2));
		var caller = Load(Emit("Alone", "public class Alone { }"));

		Assert.Contains("exists in both", Refused("() => N.T.Value", ResolutionScope.Of(caller, first, second)), StringComparison.Ordinal);
		Assert.Contains("exists in both", Refused("() => N.T.Value", ResolutionScope.Of(caller, second, first)), StringComparison.Ordinal);
	}

	[Fact]
	public void A_name_only_one_reference_declares_is_still_found()
	{
		var first  = Library("A", 1);
		var second = Library("B", 2);
		var caller = Caller(Probe, first, second);

		Assert.Equal("1", CSharp("new N.OnlyA().Value", "", "", first, second));
		Assert.Equal(1, ExpressionParser.Compile<Func<int>>("() => new N.OnlyA().Value", caller)());

		// A member of the ambiguous type's name is no type, and neither is anything under it that
		// neither declares: they are refused where `N.T` is, and not read as a type of their own.
		Assert.Equal("CS0433", CSharp("N.T.Value.ToString().Length", "", "", first, second));
		Assert.Contains("exists in both", Refused("() => N.T.Value.ToString().Length", ResolutionScope.Around(caller)), StringComparison.Ordinal);
	}

	[Fact]
	public void One_type_that_several_assemblies_answer_for_is_not_ambiguous()
	{
		// A facade forwards what it names to where it is declared, so these three each answer
		// `System.Object` with the same type. Asked of the runtime first, so that the case is known
		// to be the one it claims to be.
		var facades = new[] { typeof(object).Assembly, Assembly.Load("System.Runtime"), Assembly.Load("netstandard") };

		Assert.All(facades, static one => Assert.Same(typeof(object), one.GetType("System.Object", false, false)));

		var scope = ResolutionScope.Of(Load(Emit("Alone", "public class Alone { }")), facades);

		Assert.Equal("2", CSharp("System.Math.Max(1, 2) + new System.Object().GetHashCode() * 0", "", ""));
		Assert.Equal(2, ExpressionParser.Compile<Func<int>>("() => System.Math.Max(1, 2) + new System.Object().GetHashCode() * 0", scope)());
		Assert.Equal(typeof(object), ExpressionParser.Compile<Func<Type>>("() => typeof(Object)", scope)());
	}

	[Theory]
	[InlineData("public")]
	[InlineData("")]
	public void The_calling_assemblys_own_type_wins_over_a_references(string access)
	{
		var first  = Library("A", 1);
		var second = Library("B", 2);
		var own    = $"namespace N {{ {access} static class T {{ public static int Value => 9; }} }}";
		var caller = Caller(own + Probe, first, second);

		// C# warns (CS0436) and takes its own; this has no warnings, and takes its own.
		Assert.Equal("9", CSharp("N.T.Value", "", own, first, second));
		Assert.Equal(9, ExpressionParser.Compile<Func<int>>("() => N.T.Value", caller)());
		Assert.Equal(9, ExpressionParser.Compile<Func<int>>("using N; () => T.Value", caller)());

		// A public one wins read as another assembly would, too: the caller is still the
		// compilation the text is read for. An internal one is then not there at all, and the
		// two references are what is left.
		var outside = ResolutionScope.Around(caller).WithoutInternals();

		if (access == "public")
			Assert.Equal(9, ExpressionParser.Compile<Func<int>>("() => N.T.Value", outside)());
		else
			Assert.Contains("exists in both", Refused("() => N.T.Value", outside), StringComparison.Ordinal);
	}

	[Fact]
	public void Two_versions_of_one_assembly_side_by_side_are_ambiguous()
	{
		// Strongly named, which is where C# takes both as references and then says CS0433; weakly
		// named, it refuses the second reference itself (CS1704), which is a refusal before any
		// name is read. This has no set of references to refuse, so both come to the name.
		var older = Library("S", 1, "1.0.0.0", signed: true);
		var newer = Library("S", 2, "2.0.0.0", signed: true);

		Assert.Equal("CS0433", CSharp("N.T.Value", "", "", older, newer));
		Assert.Equal("CS1704", CSharp("N.T.Value", "", "", Library("V", 1, "1.0.0.0"), Library("V", 2, "2.0.0.0")));

		var caller  = Load(Emit("Alone", "public class Alone { }"));
		var refused = Refused("() => N.T.Value", ResolutionScope.Of(caller, Load(older), Load(newer)));

		Assert.Contains("'S, Version=1.0.0.0,", refused, StringComparison.Ordinal);
		Assert.Contains("'S, Version=2.0.0.0,", refused, StringComparison.Ordinal);

		var weak = Refused(
			"() => N.T.Value",
			ResolutionScope.Of(caller, Load(Library("V", 1, "1.0.0.0")), Load(Library("V", 2, "2.0.0.0"))));

		Assert.Contains("'V, Version=1.0.0.0,", weak, StringComparison.Ordinal);
		Assert.Contains("'V, Version=2.0.0.0,", weak, StringComparison.Ordinal);
	}

	/// <summary>What a caller writes to reference both libraries, so both are in its closure.</summary>
	const string Probe = "public class Caller { public object[] Both() => new object[] { new N.OnlyA(), new N.OnlyB() }; }";

	/// <summary>A library declaring <c>N.T</c> with that value, and a type of its own besides.</summary>
	static byte[] Library(string name, int value, string? version = null, bool signed = false)
	{
		var only = value == 1 ? "OnlyA" : "OnlyB";

		return Emit(name, $$"""
			{{(version is null ? "" : $"[assembly: System.Reflection.AssemblyVersion(\"{version}\")]")}}
			namespace N
			{
				public static class T
				{
					public static int Value => {{value}};
					public static class Inner { public static int Value => {{value}}0; }
				}
				public static class G<X> { public static int Value => {{value}}; }
				public class {{only}} { public int Value => {{value}}; }
			}
			""", signed);
	}

	/// <summary>A caller with that source over those libraries, loaded where it finds them.</summary>
	static Assembly Caller(string source, params byte[][] libraries)
	{
		var image = Emit("Caller", source, false, libraries);
		return new Context(Named(libraries)).LoadFromStream(new MemoryStream(image));
	}

	/// <summary>Libraries by the simple name each is referenced under.</summary>
	static Dictionary<string, byte[]> Named(byte[][] libraries)
	{
		return libraries.ToDictionary(static image => Metadata(image).GetAssemblyDefinition().GetAssemblyName().Name!);

		static MetadataReader Metadata(byte[] image)
		{
			return new PEReader([.. image]).GetMetadataReader();
		}
	}

	/// <summary>One image in a context of its own, so that two of one name can stand side by side.</summary>
	static Assembly Load(byte[] image)
	{
		return new Context([]).LoadFromStream(new MemoryStream(image));
	}

	/// <summary>Why the text was refused; the test fails where it was not.</summary>
	static string Refused(string text, ResolutionScope scope)
	{
		Both.Agree(text, scope);

		var match = ExpressionParser.TryParse(text, scope);

		Assert.False(match.IsSuccess, $"\"{text}\" was read.");

		return match.Error!;
	}

	/// <summary>
	/// What Roslyn makes of the expression, after those usings and beside that source, over those
	/// libraries: its value, or the first error's id.
	/// </summary>
	static string CSharp(string expression, string usings, string source, params byte[][] libraries)
	{
		var text = usings + "\n" + source + "\npublic static class Asked { public static object Value => " + expression + "; }";
		var compilation = CSharpCompilation.Create(
			"Asking", [CSharpSyntaxTree.ParseText(text)], References(libraries),
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		if (compilation.GetDiagnostics().FirstOrDefault(static one => one.Severity == DiagnosticSeverity.Error) is { } refused)
			return refused.Id;

		using var stream = new MemoryStream();

		Assert.True(compilation.Emit(stream).Success);

		return new Context(Named(libraries))
			.LoadFromStream(new MemoryStream(stream.ToArray()))
			.GetType("Asked")!
			.GetProperty("Value")!
			.GetValue(null)!
			.ToString()!;
	}

	static byte[] Emit(string name, string source, bool signed = false, params byte[][] libraries)
	{
		var options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);

		// Public signing needs only a public key, and the runtime checks no signature: any strong
		// name will do, and the one the runtime's own assembly carries is at hand.
		if (signed)
			options = options.WithCryptoPublicKey([.. typeof(object).Assembly.GetName().GetPublicKey()!]).WithPublicSign(true);

		var compilation = CSharpCompilation.Create(name, [CSharpSyntaxTree.ParseText(source)], References(libraries), options);

		using var stream = new MemoryStream();

		var result = compilation.Emit(stream, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(result.Success, string.Join("\n", result.Diagnostics));

		return stream.ToArray();
	}

	static IEnumerable<MetadataReference> References(byte[][] libraries)
	{
		return ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
			.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
			.Concat(libraries.Select(static image => MetadataReference.CreateFromImage(image)));
	}

	/// <summary>A context that finds its libraries in memory and nowhere else.</summary>
	sealed class Context(Dictionary<string, byte[]> assemblies) : AssemblyLoadContext
	{
		protected override Assembly? Load(AssemblyName name)
		{
			return assemblies.TryGetValue(name.Name!, out var image)
				? LoadFromStream(new MemoryStream(image))
				: null;
		}
	}
}
