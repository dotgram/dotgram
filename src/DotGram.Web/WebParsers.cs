using System;

namespace DotGram.Web;

/// <summary>
/// The parsers of this package, as a whole: the one place their limits are set.
/// </summary>
/// <remarks>
/// Every <c>Parse</c> and <c>TryParse</c> of the package is read by a grammar of its own. None of them
/// holds limits of its own: each reads them from here, from one reference that a set replaces whole, so
/// that no parse, of any grammar, can see one grammar's limits set and another's not yet.
/// </remarks>
public static class WebParsers
{
	static Snapshot? current;

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
			return Current.Limits;
		}
		set
		{
			if (value is null)
				throw new ArgumentNullException(nameof(value));

			System.Threading.Volatile.Write(ref current, new Snapshot(value));
		}
	}

	/// <summary>
	/// What every grammar of the package reads its limits from (OnReadingLimits, below).
	/// </summary>
	internal static Snapshot Current
	{
		get
		{
			return System.Threading.Volatile.Read(ref current) ?? Snapshot.Default;
		}
	}

	/// <summary>
	/// The package's limits and what each grammar makes of them, made together and published as one.
	/// </summary>
	internal sealed class Snapshot
	{
		public static readonly Snapshot Default = new(WebParserLimits.Default);

		internal readonly WebParserLimits Limits;

		internal readonly Rfc3339.ParserLimits Rfc3339Limits;
		internal readonly Rfc3986.ParserLimits Rfc3986Limits;
		internal readonly Rfc5322.ParserLimits Rfc5322Limits;
		internal readonly Rfc5646.ParserLimits Rfc5646Limits;
		internal readonly Rfc6265.ParserLimits Rfc6265Limits;
		internal readonly Rfc6266.ParserLimits Rfc6266Limits;
		internal readonly Rfc6570.ParserLimits Rfc6570Limits;
		internal readonly Rfc6901.ParserLimits Rfc6901Limits;
		internal readonly Rfc7239.ParserLimits Rfc7239Limits;
		internal readonly Rfc8259.ParserLimits Rfc8259Limits;
		internal readonly Rfc8288.ParserLimits Rfc8288Limits;
		internal readonly Rfc9110.ParserLimits Rfc9110Limits;
		internal readonly Rfc9651.ParserLimits Rfc9651Limits;

		internal Snapshot(WebParserLimits limits)
		{
			var stacks = limits.MaxExtraStacks;

			Limits        = limits;
			Rfc3339Limits = Rfc3339.ParserLimits.Default.WithMaxExtraStacks(stacks);
			Rfc3986Limits = Rfc3986.ParserLimits.Default.WithMaxExtraStacks(stacks);
			Rfc5322Limits = Rfc5322.ParserLimits.Default.WithMaxExtraStacks(stacks);
			Rfc5646Limits = Rfc5646.ParserLimits.Default.WithMaxExtraStacks(stacks);
			Rfc6265Limits = Rfc6265.ParserLimits.Default.WithMaxExtraStacks(stacks);
			Rfc6266Limits = Rfc6266.ParserLimits.Default.WithMaxExtraStacks(stacks);
			Rfc6570Limits = Rfc6570.ParserLimits.Default.WithMaxExtraStacks(stacks);
			Rfc6901Limits = Rfc6901.ParserLimits.Default.WithMaxExtraStacks(stacks);
			Rfc7239Limits = Rfc7239.ParserLimits.Default.WithMaxExtraStacks(stacks);
			Rfc8259Limits = Rfc8259.ParserLimits.Default.WithMaxExtraStacks(stacks);
			Rfc8288Limits = Rfc8288.ParserLimits.Default.WithMaxExtraStacks(stacks);
			Rfc9110Limits = Rfc9110.ParserLimits.Default.WithMaxExtraStacks(stacks);
			Rfc9651Limits = Rfc9651.ParserLimits.Default.WithMaxExtraStacks(stacks);
		}
	}
}

// Each grammar answers its limits from the package's one setting.

static partial class Rfc3339
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc3339Limits;
	}
}

static partial class Rfc3986
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc3986Limits;
	}
}

static partial class Rfc5322
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc5322Limits;
	}
}

static partial class Rfc5646
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc5646Limits;
	}
}

static partial class Rfc6265
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc6265Limits;
	}
}

static partial class Rfc6266
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc6266Limits;
	}
}

static partial class Rfc6570
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc6570Limits;
	}
}

static partial class Rfc6901
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc6901Limits;
	}
}

static partial class Rfc7239
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc7239Limits;
	}
}

static partial class Rfc8259
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc8259Limits;
	}
}

static partial class Rfc8288
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc8288Limits;
	}
}

static partial class Rfc9110
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc9110Limits;
	}
}

static partial class Rfc9651
{
	static partial void OnReadingLimits(ref ParserLimits limits)
	{
		limits = WebParsers.Current.Rfc9651Limits;
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
