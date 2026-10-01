using System.Linq;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.GeneratedAnalyzers.Tests;

public class Dga001Tests
{
	[Fact]
	public async Task Positive_decl_decl_copy_never_read_again()
	{
		var source = """
			class C
			{
				static string Construct_X(int a) => a.ToString();

				static void M()
				{
					string last0 = Construct_X(1);
					string fold = last0;
					System.Console.WriteLine(fold);
				}
			}
			""";

		var diagnostics = await AnalyzerHarness.RunAsync(source);

		var found = Assert.Single(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga001);
		Assert.Equal(DiagnosticSeverity.Error, found.Severity);
		Assert.Contains("last0", found.GetMessage());
		Assert.Contains("fold", found.GetMessage());
	}

	[Fact]
	public async Task Positive_assign_assign_copy_never_read_again()
	{
		var source = """
			class C
			{
				static void M(int seed)
				{
					int x;
					int z;
					x = seed;
					z = x;
					System.Console.WriteLine(z);
				}
			}
			""";

		var diagnostics = await AnalyzerHarness.RunAsync(source);

		Assert.Contains(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga001);
	}

	[Fact]
	public async Task Negative_local_read_again_after_the_copy()
	{
		var source = """
			class C
			{
				static string Construct_X(int a) => a.ToString();

				static void M()
				{
					string last0 = Construct_X(1);
					string fold = last0;
					System.Console.WriteLine(last0);
				}
			}
			""";

		var diagnostics = await AnalyzerHarness.RunAsync(source);

		Assert.DoesNotContain(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga001);
	}

	[Fact]
	public async Task Negative_parameter_copy_is_not_a_local_to_local_copy()
	{
		// "var p = pos;" copies a PARAMETER, not a preceding local: a separate emitter
		// question (padding-residual direct-reader work), never DGA001's business.
		var source = """
			class C
			{
				static void M(int pos)
				{
					var p = pos;
					System.Console.WriteLine(p);
				}
			}
			""";

		var diagnostics = await AnalyzerHarness.RunAsync(source);

		Assert.DoesNotContain(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga001);
	}

	[Fact]
	public async Task Silent_inside_a_line_mapped_region()
	{
		// The same copy shape as the positive test, but under a #line directive pointing at
		// the grammar author's own file: this is the user's C# (a `=>` or a `when`), not the
		// emitter's, and must not be flagged.
		var source = """
			class C
			{
				static string Construct_X(int a) => a.ToString();

				static void M()
				{
			#line 1 "Grammar.gram"
					string last0 = Construct_X(1);
					string fold = last0;
			#line default
					System.Console.WriteLine(fold);
				}
			}
			""";

		var diagnostics = await AnalyzerHarness.RunAsync(source);

		Assert.DoesNotContain(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga001);
	}

	[Fact]
	public async Task Suppressed_with_a_reason_is_silent_and_needs_no_DGA001R()
	{
		var source = """
			class C
			{
				static string Construct_X(int a) => a.ToString();

				static void M()
				{
			#pragma warning disable DGA001 // ReaderRegisters needs 'last' current across the turn
					string last0 = Construct_X(1);
					string fold = last0;
			#pragma warning restore DGA001
					System.Console.WriteLine(fold);
				}
			}
			""";

		var diagnostics = await AnalyzerHarness.RunAsync(source);

		Assert.DoesNotContain(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga001Reason);

		// "Assert.All" over an empty filtered sequence passes trivially, so this asserts DGA001
		// actually fired (and was suppressed) rather than merely never appearing unsuppressed.
		var dga001 = Assert.Single(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga001);
		Assert.True(dga001.IsSuppressed);
	}

	[Fact]
	public async Task Negative_the_source_is_a_field_not_a_local()
	{
		// A reader's own register (Machine.Reader.cs's EmitRecord writes exactly this shape,
		// "last0 = Construct_X(a); fold = last0;", for a field, not a local): the field may
		// still be read by a caller after this method returns, which a walk scoped to one
		// method body cannot see, so the source must be confirmed as a genuine LOCAL through
		// the semantic model before DGA001 may fire on it.
		var source = """
			class C
			{
				static string Construct_X(int a) => a.ToString();

				class Reader
				{
					string last0 = "";

					string M(int a)
					{
						last0 = Construct_X(a);
						string fold = last0;
						return fold;
					}
				}
			}
			""";

		var diagnostics = await AnalyzerHarness.RunAsync(source);

		Assert.DoesNotContain(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga001);
	}

	[Fact]
	public async Task Positive_a_same_named_read_in_a_sibling_branch_does_not_hide_a_dead_copy()
	{
		// The shape a whole-member text search got wrong: "last0" is read again, but only in
		// a DIFFERENT, sibling "else if" branch this specific copy's own branch never reaches.
		// The first branch's copy is genuinely dead and must still be reported.
		var source = """
			class C
			{
				static string Construct_X(int a) => a.ToString();
				static string Construct_Y(int a) => a.ToString();

				static string M(int kind, int a)
				{
					if (kind == 0)
					{
						string last0 = Construct_X(a);
						string fold = last0;
						return fold;
					}
					else if (kind == 1)
					{
						string last0 = Construct_Y(a);
						return last0;
					}

					return "";
				}
			}
			""";

		var diagnostics = await AnalyzerHarness.RunAsync(source);

		var found = Assert.Single(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga001);
		Assert.Contains("last0", found.GetMessage());
		Assert.Contains("fold", found.GetMessage());
	}

	[Fact]
	public async Task Negative_a_compound_assignment_counts_as_a_read()
	{
		// "last0 += ...;" reads last0's prior value as much as any ordinary use does (it is
		// shorthand for "last0 = last0 + ...") -- treating a compound-assignment target as "no
		// read happened" would make this a false positive now that DGA001 is an error.
		var source = """
			class C
			{
				static string Construct_X(int a) => a.ToString();

				static string M(int a)
				{
					string last0 = Construct_X(a);
					string fold = last0;
					last0 += "!";
					return fold + last0;
				}
			}
			""";

		var diagnostics = await AnalyzerHarness.RunAsync(source);

		Assert.DoesNotContain(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga001);
	}

	[Fact]
	public async Task Suppressed_without_a_reason_reports_DGA001R()
	{
		var source = """
			class C
			{
				static string Construct_X(int a) => a.ToString();

				static void M()
				{
			#pragma warning disable DGA001
					string last0 = Construct_X(1);
					string fold = last0;
			#pragma warning restore DGA001
					System.Console.WriteLine(fold);
				}
			}
			""";

		var diagnostics = await AnalyzerHarness.RunAsync(source);

		var reason = Assert.Single(diagnostics, d => d.Id == GeneratedCodeAnalyzer.Dga001Reason);
		Assert.Equal(DiagnosticSeverity.Error, reason.Severity);
		Assert.False(reason.IsSuppressed);
	}
}
