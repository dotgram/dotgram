using System;

namespace DotGram.Web;

/// <summary>
/// The parsers of this package, as a whole: the one place their limits are set.
/// </summary>
/// <remarks>
/// Every <c>Parse</c> and <c>TryParse</c> of the package is read by a grammar of its own, and each
/// grammar holds its own limits. This sets them all at once, so that a program facing text from
/// someone else says once how far any of them may go. Change them before parsing: while they
/// change, the grammars may briefly see different values.
/// </remarks>
public static class WebParsers
{
	static readonly object Gate = new();

	static WebParserLimits? limits;

	/// <summary>
	/// The limits every parser of this package holds its input to; <see cref="WebParserLimits.Default"/>
	/// until set.
	/// </summary>
	/// <remarks>
	/// Each parse reads them once, as it begins: a parse already running when they are set keeps the
	/// limits it began with, and one begun after the setter returns reads the new ones.
	/// </remarks>
	/// <exception cref="ArgumentNullException">Set to <c>null</c>.</exception>
	public static WebParserLimits Limits
	{
		get
		{
			return System.Threading.Volatile.Read(ref limits) ?? WebParserLimits.Default;
		}
		set
		{
			if (value is null)
				throw new ArgumentNullException(nameof(value));

			// One setter at a time, so that every grammar ends on the value of the same one.
			lock (Gate)
			{
				var stacks = value.MaxExtraStacks;

				Rfc3339.Limits = Rfc3339.ParserLimits.Default.WithMaxExtraStacks(stacks);
				Rfc3986.Limits = Rfc3986.ParserLimits.Default.WithMaxExtraStacks(stacks);
				Rfc5322.Limits = Rfc5322.ParserLimits.Default.WithMaxExtraStacks(stacks);
				Rfc5646.Limits = Rfc5646.ParserLimits.Default.WithMaxExtraStacks(stacks);
				Rfc6265.Limits = Rfc6265.ParserLimits.Default.WithMaxExtraStacks(stacks);
				Rfc6266.Limits = Rfc6266.ParserLimits.Default.WithMaxExtraStacks(stacks);
				Rfc6570.Limits = Rfc6570.ParserLimits.Default.WithMaxExtraStacks(stacks);
				Rfc6901.Limits = Rfc6901.ParserLimits.Default.WithMaxExtraStacks(stacks);
				Rfc7239.Limits = Rfc7239.ParserLimits.Default.WithMaxExtraStacks(stacks);
				Rfc8259.Limits = Rfc8259.ParserLimits.Default.WithMaxExtraStacks(stacks);
				Rfc8288.Limits = Rfc8288.ParserLimits.Default.WithMaxExtraStacks(stacks);
				Rfc9110.Limits = Rfc9110.ParserLimits.Default.WithMaxExtraStacks(stacks);
				Rfc9651.Limits = Rfc9651.ParserLimits.Default.WithMaxExtraStacks(stacks);

				System.Threading.Volatile.Write(ref limits, value);
			}
		}
	}
}

/// <summary>
/// What a parser of this package may take of untrusted input: an immutable value, changed by making
/// another with a <c>With</c> method and setting <see cref="WebParsers.Limits"/> to it.
/// </summary>
/// <remarks>
/// None of the package's parsers reads a stream, so the one limit is how deep a reading may go:
/// a JSON text nests as deep as the input does, and so do the comments of a mail address.
/// </remarks>
public sealed class WebParserLimits
{
	WebParserLimits(int maxExtraStacks)
	{
		MaxExtraStacks = maxExtraStacks;
	}

	/// <summary>
	/// No limit: a reading takes as many stacks as its nesting needs and there is memory for.
	/// </summary>
	public static WebParserLimits Default { get; } = new(0);

	/// <summary>
	/// How many stacks one parse may take beyond the one it began on; past that many it throws
	/// <see cref="InsufficientExecutionStackException"/>. Zero is no limit.
	/// </summary>
	/// <remarks>
	/// A stack holds some thousands of levels of nesting, a number that depends on the grammar and
	/// the runtime, so this bounds depth coarsely. To bound it exactly, bound the length of the text:
	/// a level takes a character at least.
	/// </remarks>
	public int MaxExtraStacks { get; }

	/// <summary>
	/// These limits with <see cref="MaxExtraStacks"/> changed.
	/// </summary>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
	public WebParserLimits WithMaxExtraStacks(int value)
	{
		if (value < 0)
			throw new ArgumentOutOfRangeException(nameof(value));

		return new WebParserLimits(value);
	}
}
