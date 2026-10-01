using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Why <c>FixGrammar</c> (<c>DotGram.Finance.Fix.FixGrammar</c>) pins <c>Carrier =
/// GramCarrier.Immediate</c> instead of leaving the choice to <c>Auto</c>.
/// </summary>
/// <remarks>
/// <para>
/// A length-and-data pair reads the length field first and the data field second, and the
/// second field's <c>switch</c> has to know, while it is still deciding which alternative to
/// try, that the tag it just read is the data that the length named. <c>FixGrammar</c> keeps
/// that across the two fields in a small piece of state (<c>FixReading._expected</c>/
/// <c>Size</c>) that the length field's own <c>=&gt;</c> writes and the data field's
/// <c>switch</c> selector reads.
/// </para>
/// <para>
/// A <c>switch</c> selector always runs immediately — it has to, since it is what decides what
/// to read next — but a <c>=&gt;</c> does not: under the tape carrier every construction is
/// deferred until the whole parse is accepted, then replayed in order. So the data field's
/// selector, reading the shared state while parsing is still underway, sees it before the
/// length field's own construction has written it, not after. It reads a stale default instead
/// of the tag the length field named, and the data field is misclassified as ordinary text
/// instead of a fixed-length payload — exactly backwards from what the length field just said.
/// </para>
/// <para>
/// A reading that only ever carries a tag value catches none of this, because nothing about a
/// tag's own text depends on another field — <see cref="FixShapedCarrierTests"/>'s <c>Kind</c>
/// is pure for that reason, and its two carriers agree. This grammar instead keeps the same
/// kind of state <c>FixReading</c> does — written by one field's construction, read by the
/// next field's selector — so it is the shape that tells the two carriers apart rather than
/// the one that shows they agree.
/// </para>
/// <para>
/// <see cref="Forcing_the_tape_misreads_the_pair"/> is skipped: it is not a regression to keep
/// green, it is kept so the mechanism can be re-run by hand (<c>dotnet test --filter</c>) if
/// the pin on <c>FixGrammar</c> is ever questioned or the generator's own carrier choice for it
/// changes.
/// </para>
/// </remarks>
public sealed class FixBinaryPairCarrierTests
{
	const string Grammar = """
		Sep = ';'

		Tag  : @int = t: ['0'..'9']+ => @(int.Parse(t.ToString()))
		Size : @int = t: ['0'..'9']+ => @(int.Parse(t.ToString()))
		Text = (?!Sep & any)+

		Field : @string =
			wire: (tag: Tag & '=' & switch @(Kind(tag)) {
				case 0: Text
				case 1: Size
				case 2: @ReadData
			}) & end: (Sep | eof)
			=> @(Built(tag, wire))

		Fields : @string[] = Field* recover Sep => @("bad:" + parserText.ToString())

		parse Fields as ParseFields
		""";

	// Tag 9 is the length of the field that follows it (RawDataLength); the field it
	// measures (RawData) may hold the separator byte itself, which is the whole reason FIX
	// reads it by length instead of scanning for the next separator (docs/syntax.md's
	// ReadData example, mirrored by FixGrammar.ReadData).
	const string Members = """
		static int _expected;
		static int _size;

		static int Kind(int tag)
		{
			var expected = _expected;
			_expected = 0;
			if (tag == expected)
				return 2;
			return tag == 9 ? 1 : 0;
		}

		static bool ReadData(System.ReadOnlySpan<char> input, ref int position)
		{
			if (position + _size > input.Length)
				return false;

			position += _size;

			return true;
		}

		static string Built(int tag, string wire)
		{
			var value = wire.Substring(wire.IndexOf('=') + 1);

			if (tag == 9)
			{
				_expected = 10;
				_size     = int.Parse(value);
			}

			return tag + "=" + value;
		}
		""";

	/// <summary>
	/// On the tape, the data field's selector reads the length field's state before that
	/// field's own construction has run, so the data (which holds the separator byte the
	/// length was read to protect against) is cut at that embedded separator instead of at
	/// its declared length — and what is left over fails to parse as a field at all.
	/// </summary>
	[Fact(Skip =
		"Documents the defect FixGrammar.cs's Carrier = GramCarrier.Immediate pin exists to " +
		"avoid, rather than a behaviour kept green: forcing this grammar's length-and-data " +
		"pair onto the tape misreads the data field, because its switch selector runs " +
		"immediately while the length field's own construction - the one that records which " +
		"tag the data is and how long it is - waits for the tape's replay. Unskip and run by " +
		"hand if FixGrammar's pin, or the generator's own carrier choice for it, is ever " +
		"revisited.")]
	public void Forcing_the_tape_misreads_the_pair()
	{
		const string wire = "9=5;10=ab;cd;";

		var tape      = Read(CarrierKind.Tape, wire);
		var immediate = Read(CarrierKind.Immediate, wire);

		// Immediate reads the pair correctly: the length field (9=5) is followed by exactly
		// five characters of data, embedded separator included.
		Assert.Equal("9=5 | 10=ab;cd", immediate);

		// The tape cuts the data field at the embedded separator instead of at its declared
		// length, then fails to read what is left over ("cd") as a field of its own.
		Assert.Equal("9=5 | 10=ab | bad:cd", tape);
	}

	static string Read(CarrierKind carrier, string wire)
	{
		var compiled = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName     = "Grammar",
			Carrier       = carrier,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.DoesNotContain(compiled.Diagnostics, one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(compiled.Sources).Text;

		var host = EmittedCode.Compile(source, declarationMembers: Members).GetType("Grammar")!;

		var read   = host.GetMethods().Single(one =>
			one.Name == "ParseFields" && one.GetParameters()[0].ParameterType == typeof(string));
		var fields = ((IEnumerable)read.Invoke(null, [wire])!).Cast<object?>().Select(one => one?.ToString() ?? "<null>");

		return string.Join(" | ", fields);
	}
}
