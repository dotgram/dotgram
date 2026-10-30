using System;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Tests;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// The supplied names completion offers in a construction, a guard and a recovery's factory are
/// the ones the generated parser hands that C#: each is compiled in each place, and the editor's
/// lists (<see cref="GramVocabulary.SuppliedToConstructions"/>, <see cref="GramVocabulary.SuppliedWithState"/>,
/// <see cref="GramVocabulary.SuppliedToRecoveries"/>) are held to what the compiler accepts.
/// </summary>
public sealed class EditorSuppliedNamesTests
{
	public static TheoryData<string, string, bool> Places()
	{
		var data = new TheoryData<string, string, bool>();

		foreach (var name in SuppliedNames.All)
			foreach (var place in new[] { "construction", "guard", "recovery" })
				foreach (var state in new[] { false, true })
				{
					// A recovery's factory is handed `default` for a state, which compiles and says
					// nothing; whether it is offered there is the editor's decision, not the compiler's.
					if (place == "recovery" && GramVocabulary.SuppliedWithState.Contains(name))
						continue;

					data.Add(name, place, state);
				}

		return data;
	}

	[Theory]
	[MemberData(nameof(Places))]
	public void A_supplied_name_is_offered_exactly_where_the_generated_parser_hands_it_over(string name, string place, bool state)
	{
		var declared = state ? "state : @int\n" : "";
		var grammar  = declared + place switch
		{
			"construction" => $"A : @string = 'a' => @({name}.ToString())\nparse A",
			"guard"        => $"A = 'a' & when @({name}.ToString() != null)\nparse A",
			_              => $"Rows : @string[] = (v: Row => @(v))* recover eol => @({name}.ToString())\nRow : @string = 'r' & eol => @(\"r\")\nparse Rows",
		};

		var options  = new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance };
		var compiled = GramCompiler.Compile(grammar, options);
		var refusal  = compiled.Diagnostics.FirstOrDefault(static one => one.Severity == GramSeverity.Error) is { } refused
			? $"{refused.Id} {refused.Message}"
			: EmittedCode.Errors(compiled.Sources[0].Text, "Grammar", null, null, compiled.Sources.Skip(1).Select(static one => one.Text))
				.FirstOrDefault()?.ToString();
		var accepted = refusal is null;

		var at      = grammar.LastIndexOf(name, StringComparison.Ordinal);
		var offered = GramVocabulary.SuppliedAt(grammar, at).Contains(name);

		Assert.True(accepted == offered, $"'{name}' is {(accepted ? "" : "not ")}accepted ({refusal}) and {(offered ? "" : "not ")}offered in:\n{grammar}");
	}

	[Theory]
	[InlineData("A : @string = 'a' => @(|)\nparse A",                                                        "construction")]
	[InlineData("A = 'a' & when @(|)\nparse A",                                                              "guard")]
	[InlineData("A : @string = 'a' => @Hold(|)\nparse A",                                                   "construction")]
	[InlineData("Rows : @string[] = (v: Row => @(v))* recover eol => @(|)\nRow : @string = 'r'\nparse Rows", "recovery")]
	[InlineData("Rows : @string[] = (v: Row => @(|))* recover eol => @(v)\nRow : @string = 'r'\nparse Rows", "construction")]
	[InlineData("A = 'a' & |\nparse A",                                                                      "none")]
	public void The_action_a_position_stands_in_is_read_from_the_grammar(string marked, string expected)
	{
		var position = marked.IndexOf('|', StringComparison.Ordinal);
		var action   = GramVocabulary.ActionAt(marked.Remove(position, 1), position, out _);

		Assert.Equal(expected, action.ToString().ToLowerInvariant());
	}
}
