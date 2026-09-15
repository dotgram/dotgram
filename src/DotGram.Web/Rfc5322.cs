using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

using DotGram;

namespace DotGram.Web;

/// <summary>An addr-spec (RFC 5322 §3.4.1): a local part and a domain, as they mean, not as they were written.</summary>
/// <remarks>
/// Comments and folding are gone, a quoted local part is its content with each quoted-pair's backslash taken
/// off, and an obsolete local part or domain written in pieces with whitespace between is its pieces joined by
/// <c>.</c> (§3.2.4, §4.4). Both are compared as they are held: case in a local part is the receiving host's to
/// interpret, and this does not guess.
/// </remarks>
/// <param name="LocalPart">What stands before <c>@</c>.</param>
/// <param name="Domain">A domain name, or a domain literal with its brackets.</param>
public sealed record AddrSpec(string LocalPart, string Domain)
{
	/// <summary>An addr-spec as a receiver reads it: §3's syntax and §4's obsolete syntax both.</summary>
	/// <exception cref="FormatException">The text is no addr-spec; the message says where.</exception>
	public static AddrSpec Parse(string text) =>
		Rfc5322.ParseAddrSpec(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>An addr-spec as a receiver reads it, or false where the text is not one.</summary>
	public static bool TryParse(string text, [NotNullWhen(true)] out AddrSpec? address)
	{
		var match = Rfc5322.TryParseAddrSpec(text ?? throw new ArgumentNullException(nameof(text)));

		address = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>An addr-spec in §3's syntax alone: what a sender may write.</summary>
	/// <exception cref="FormatException">The text is no addr-spec §3 makes; the message says where.</exception>
	public static AddrSpec ParseStrict(string text) =>
		Rfc5322.ParseStrictAddrSpec(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>An addr-spec in §3's syntax alone, or false where the text is not one.</summary>
	public static bool TryParseStrict(string text, [NotNullWhen(true)] out AddrSpec? address)
	{
		var match = Rfc5322.TryParseStrictAddrSpec(text ?? throw new ArgumentNullException(nameof(text)));

		address = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>Whether a local part can be written as a dot-atom: atext, and dots between runs of it.</summary>
	public static bool IsDotAtom(string text) => Rfc5322.IsDotAtomText(text);

	/// <summary>Whether the domain is a domain literal, <c>[...]</c>.</summary>
	public bool IsDomainLiteral => Domain.Length > 0 && Domain[0] == '[';

	/// <summary>The addr-spec as §3 would generate it: a dot-atom local part where it can be one, a quoted string otherwise.</summary>
	public override string ToString()
	{
		var output = new StringBuilder();

		if (Rfc5322.IsDotAtomText(LocalPart))
			output.Append(LocalPart);
		else
			Rfc5322.Quoted(output, LocalPart);

		output.Append('@');

		if (IsDomainLiteral)
		{
			output.Append('[');

			for (var at = 1; at < Domain.Length - 1; at++)
			{
				if (Domain[at] is '[' or ']' or '\\')
					output.Append('\\');

				output.Append(Domain[at]);
			}

			output.Append(']');
		}
		else
			output.Append(Domain);

		return output.ToString();
	}
}

/// <summary>An address (RFC 5322 §3.4): a mailbox, or a named group of mailboxes.</summary>
/// <remarks>A closed set: the two are nested here and nothing outside can add a third.</remarks>
public abstract record EmailAddress
{
	EmailAddress()
	{
	}

	/// <summary>An address-list as a receiver reads it, null members left out: what <c>To</c> and <c>Cc</c> hold.</summary>
	/// <exception cref="FormatException">The text is no address-list; the message says where.</exception>
	public static EmailAddress[] ParseList(string text) =>
		Rfc5322.ParseAddressList(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>An address-list as a receiver reads it, or false where the text is not one.</summary>
	public static bool TryParseList(string text, [NotNullWhen(true)] out EmailAddress[]? addresses)
	{
		var match = Rfc5322.TryParseAddressList(text ?? throw new ArgumentNullException(nameof(text)));

		addresses = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>An address-list in §3's syntax alone: what a sender may write.</summary>
	/// <exception cref="FormatException">The text is no address-list §3 makes; the message says where.</exception>
	public static EmailAddress[] ParseStrictList(string text) =>
		Rfc5322.ParseStrictAddressList(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>An address-list in §3's syntax alone, or false where the text is not one.</summary>
	public static bool TryParseStrictList(string text, [NotNullWhen(true)] out EmailAddress[]? addresses)
	{
		var match = Rfc5322.TryParseStrictAddressList(text ?? throw new ArgumentNullException(nameof(text)));

		addresses = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>A mailbox-list as a receiver reads it, null members left out: what <c>From</c> holds.</summary>
	/// <exception cref="FormatException">The text is no mailbox-list; the message says where.</exception>
	public static Mailbox[] ParseMailboxList(string text) =>
		Rfc5322.ParseMailboxList(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A mailbox-list as a receiver reads it, or false where the text is not one.</summary>
	public static bool TryParseMailboxList(string text, [NotNullWhen(true)] out Mailbox[]? mailboxes)
	{
		var match = Rfc5322.TryParseMailboxList(text ?? throw new ArgumentNullException(nameof(text)));

		mailboxes = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>A mailbox-list in §3's syntax alone.</summary>
	/// <exception cref="FormatException">The text is no mailbox-list §3 makes; the message says where.</exception>
	public static Mailbox[] ParseStrictMailboxList(string text) =>
		Rfc5322.ParseStrictMailboxList(text ?? throw new ArgumentNullException(nameof(text)));

	/// <summary>A mailbox-list in §3's syntax alone, or false where the text is not one.</summary>
	public static bool TryParseStrictMailboxList(string text, [NotNullWhen(true)] out Mailbox[]? mailboxes)
	{
		var match = Rfc5322.TryParseStrictMailboxList(text ?? throw new ArgumentNullException(nameof(text)));

		mailboxes = match.IsSuccess ? match.Value : null;

		return match.IsSuccess;
	}

	/// <summary>A mailbox: an addr-spec and, where one was given, a display name.</summary>
	/// <param name="DisplayName">
	/// The phrase as it reads: comments dropped, each run of whitespace one space, quoted strings unquoted; null where
	/// there was none.
	/// </param>
	public sealed record Mailbox(string? DisplayName, AddrSpec Address) : EmailAddress
	{
		/// <summary>A mailbox as a receiver reads it (§3.4, §4.4).</summary>
		/// <exception cref="FormatException">The text is no mailbox; the message says where.</exception>
		public static Mailbox Parse(string text) =>
			Rfc5322.ParseMailbox(text ?? throw new ArgumentNullException(nameof(text)));

		/// <summary>A mailbox as a receiver reads it, or false where the text is not one.</summary>
		public static bool TryParse(string text, [NotNullWhen(true)] out Mailbox? mailbox)
		{
			var match = Rfc5322.TryParseMailbox(text ?? throw new ArgumentNullException(nameof(text)));

			mailbox = match.IsSuccess ? match.Value : null;

			return match.IsSuccess;
		}

		/// <summary>A mailbox in §3's syntax alone.</summary>
		/// <exception cref="FormatException">The text is no mailbox §3 makes; the message says where.</exception>
		public static Mailbox ParseStrict(string text) =>
			Rfc5322.ParseStrictMailbox(text ?? throw new ArgumentNullException(nameof(text)));

		/// <summary>A mailbox in §3's syntax alone, or false where the text is not one.</summary>
		public static bool TryParseStrict(string text, [NotNullWhen(true)] out Mailbox? mailbox)
		{
			var match = Rfc5322.TryParseStrictMailbox(text ?? throw new ArgumentNullException(nameof(text)));

			mailbox = match.IsSuccess ? match.Value : null;

			return match.IsSuccess;
		}
	}

	/// <summary>A group: a display name and its mailboxes, of which there may be none.</summary>
	public sealed record Group(string DisplayName, IReadOnlyList<Mailbox> Members) : EmailAddress
	{
		public bool Equals(Group? other) =>
			other is not null && DisplayName == other.DisplayName && Structural.Same(Members, other.Members);

		public override int GetHashCode() => Structural.Combine(DisplayName.GetHashCode(), Structural.Hash(Members));
	}

	/// <summary>The address as §3 would generate it.</summary>
	/// <remarks>Sealed, or each case would print itself the way a record does instead.</remarks>
	public sealed override string ToString()
	{
		var output = new StringBuilder();

		switch (this)
		{
			case Mailbox mailbox:
				Write(output, mailbox);
				break;

			case Group group:
				Rfc5322.Phrase(output, group.DisplayName);
				output.Append(':');

				for (var index = 0; index < group.Members.Count; index++)
				{
					output.Append(index == 0 ? " " : ", ");
					Write(output, group.Members[index]);
				}

				output.Append(';');
				break;
		}

		return output.ToString();
	}

	static void Write(StringBuilder output, Mailbox mailbox)
	{
		if (mailbox.DisplayName is null)
		{
			output.Append(mailbox.Address);
			return;
		}

		Rfc5322.Phrase(output, mailbox.DisplayName);
		output.Append(" <").Append(mailbox.Address).Append('>');
	}
}

// RFC 5322, Internet Message Format, §3.2 and §3.4: addresses. §4 says a receiver MUST accept the obsolete
// syntax and a generator MUST NOT produce it, so there are two readings of one grammar:
//
//   * ParseAddrSpec, ParseMailbox, ParseMailboxList and ParseAddressList read §3 with §4 added — what a
//     receiver reads. §4's forms are supersets of §3's (an obsolete local part is words joined by dots, of
//     which a dot-atom is one), and where one is, the rule is the superset alone: ordered choice would take
//     §3's form and not come back for the longer one.
//   * The Strict publications rebind each piece of §4 to nothing, and read only what §3 generates.
//
// Two verified errata are in what is read. Erratum 1908 writes obs-FWS as 1*([CRLF] WSP), which is what is
// here; 1766 is editorial. Held and reported errata are not applied: 3135 would refuse `""@example.com`, which
// §3.2.4 as published allows.
//
// A value is what §3.2 says it means: CFWS is not part of what it surrounds, a CRLF in FWS is invisible, and a
// quoted-pair is the character alone. The domain is RFC 5322's liberal one; whether it is also a host name
// RFC 5321 would deliver to — a hyphen at a label's edge, a length — is not asked here.

[Gram("""
	@using System;
	@using DotGram.Web;

	trivia = none

	// ── §3.2.1, §3.2.2 ───────────────────────────────────────────────────────────

	Wsp  = [' ' | '\t']
	Crlf = "\r\n"

	// FWS = ([*WSP CRLF] 1*WSP) / obs-FWS, and obs-FWS = 1*([CRLF] WSP) (erratum 1908) holds the first.
	Fws        = ObsFws
	CurrentFws = (Wsp* & Crlf)? & Wsp+
	ObsFws     = (Crlf? & Wsp)+

	QuotedPair = '\\' & ['!'..'~' | ' ' | '\t'] | ObsQp

	Ctext    = ['!'..'\'' | '*'..'[' | ']'..'~'] | ObsNoWsCtl
	Ccontent = Ctext | QuotedPair | Comment
	Comment  = '(' & (Fws? & Ccontent)* & Fws? & ')'
	Cfws     = (Fws? & Comment)+ & Fws? | Fws

	// ── §3.2.3, §3.2.4, §3.2.5 ───────────────────────────────────────────────────

	Atext = ['a'..'z' | 'A'..'Z' | '0'..'9' | '!' | '#' | '$' | '%' | '&' | '\'' | '*' | '+' | '-' | '/' | '=' | '?' | '^' | '_' | '`' | '{' | '|' | '}' | '~']

	AtomText    = Atext+
	DotAtomText = Atext+ & ('.' & Atext+)*

	Qtext      = ['!' | '#'..'[' | ']'..'~'] | ObsNoWsCtl
	Qcontent   = Qtext | QuotedPair
	QuotedBody = '"' & (Fws? & Qcontent)* & Fws? & '"'

	Word : @string
		= Cfws? & text: AtomText & Cfws?   => @(text)
		| Cfws? & body: QuotedBody & Cfws? => @(Rfc5322.Unquoted(body))

	// phrase = 1*word / obs-phrase, and obs-phrase = word *(word / "." / CFWS) holds the first.
	Phrase        = ObsPhrase
	CurrentPhrase = WordText+
	ObsPhrase     = WordText & (WordText | '.' | Cfws)*
	WordText      = Cfws? & (AtomText | QuotedBody) & Cfws?

	// ── §3.4.1 ───────────────────────────────────────────────────────────────────

	AddrSpecRule : @AddrSpec = local: LocalPart & '@' & domain: Domain => @(new AddrSpec(local, domain))

	// local-part = dot-atom / quoted-string / obs-local-part, and obs-local-part = word *("." word) holds both.
	LocalPart : @string = first: Word & rest: DotWord* => @(Rfc5322.Dotted(first, rest))

	CurrentLocalPart : @string
		= Cfws? & text: DotAtomText & Cfws? => @(text)
		| Cfws? & body: QuotedBody & Cfws?  => @(Rfc5322.Unquoted(body))

	DotWord : @string = '.' & word: Word => @(word)

	// domain = dot-atom / domain-literal / obs-domain, and obs-domain = atom *("." atom) holds the dot-atom.
	Domain : @string
		= literal: DomainLiteral => @(literal)
		| first: Atom & rest: DotAtom* => @(Rfc5322.Dotted(first, rest))

	CurrentDomain : @string
		= literal: DomainLiteral             => @(literal)
		| Cfws? & text: DotAtomText & Cfws?  => @(text)

	Atom : @string = Cfws? & text: AtomText & Cfws? => @(text)

	DotAtom : @string = '.' & atom: Atom => @(atom)

	DomainLiteral : @string = Cfws? & body: DomainLiteralBody & Cfws? => @(Rfc5322.Literal(body))

	DomainLiteralBody = '[' & (Fws? & Dtext)* & Fws? & ']'

	Dtext = ['!'..'Z' | '^'..'~'] | ObsDtext

	// ── §3.4 ─────────────────────────────────────────────────────────────────────

	Address : @EmailAddress = mailbox: Mailbox => @(mailbox) | group: Group => @(group)

	Mailbox : @EmailAddress.Mailbox
		= (name: Phrase)? & address: AngleAddr => @(new EmailAddress.Mailbox(Rfc5322.DisplayName(name), address))
		| address: AddrSpecRule                => @(new EmailAddress.Mailbox(null, address))

	// angle-addr = [CFWS] "<" addr-spec ">" [CFWS] / obs-angle-addr, which adds a route to ignore.
	AngleAddr : @AddrSpec = Cfws? & '<' & ObsRoute? & address: AddrSpecRule & '>' & Cfws? => @(address)

	Group : @EmailAddress.Group
		= name: Phrase & ':' & members: GroupList & ';' & Cfws?
		=> @(new EmailAddress.Group(Rfc5322.DisplayName(name)!, members))

	GroupList : @EmailAddress.Mailbox[]
		= list: MailboxList => @(list)
		| ObsGroupList      => @(Array.Empty<EmailAddress.Mailbox>())
		| Cfws?             => @(Array.Empty<EmailAddress.Mailbox>())

	// mailbox-list = (mailbox *("," mailbox)) / obs-mbox-list, whose null members the Strict readings refuse.
	MailboxList : @EmailAddress.Mailbox[] = NullMembers & first: Mailbox & rest: NextMailbox* => @(Rfc5322.Joined(first, rest))

	NextMailbox : @EmailAddress.Mailbox[]
		= ',' & mailbox: Mailbox => @(new[] { mailbox })
		| ',' & NullMember       => @(Array.Empty<EmailAddress.Mailbox>())

	AddressList : @EmailAddress[] = NullMembers & first: Address & rest: NextAddress* => @(Rfc5322.Joined(first, rest))

	NextAddress : @EmailAddress[]
		= ',' & address: Address => @(new[] { address })
		| ',' & NullMember       => @(Array.Empty<EmailAddress>())

	// ── §4 ───────────────────────────────────────────────────────────────────────

	ObsNoWsCtl = ['\u0001'..'\u0008' | '\u000B' | '\u000C' | '\u000E'..'\u001F' | '\u007F']

	ObsQp = '\\' & ['\u0000'..'\u0008' | '\u000B' | '\u000C' | '\u000E'..'\u001F' | '\u007F' | '\n' | '\r']

	ObsDtext = ObsNoWsCtl | QuotedPair

	// obs-route = obs-domain-list ":", and obs-domain-list = *(CFWS / ",") "@" domain *("," [CFWS] ["@" domain]).
	ObsRoute = (Cfws | ',')* & '@' & Domain & (',' & Cfws? & ('@' & Domain)?)* & ':'

	ObsGroupList = (Cfws? & ',')+ & Cfws?

	NullMembers = (Cfws? & ',')*
	NullMember  = Cfws?

	// What a Strict reading puts where §4 was: it matches nothing, and consumes a character where it would.
	Never = ?!any & any

	// ── Publications ─────────────────────────────────────────────────────────────

	parse AddrSpecRule as ParseAddrSpec
	parse Mailbox      as ParseMailbox
	parse MailboxList  as ParseMailboxList
	parse AddressList  as ParseAddressList

	parse AddrSpecRule with (Fws = CurrentFws, ObsNoWsCtl = Never, ObsQp = Never, ObsDtext = Never, LocalPart = CurrentLocalPart, Domain = CurrentDomain) as ParseStrictAddrSpec
	parse Mailbox      with (Fws = CurrentFws, ObsNoWsCtl = Never, ObsQp = Never, ObsDtext = Never, LocalPart = CurrentLocalPart, Domain = CurrentDomain, Phrase = CurrentPhrase, ObsRoute = Never) as ParseStrictMailbox
	parse MailboxList  with (Fws = CurrentFws, ObsNoWsCtl = Never, ObsQp = Never, ObsDtext = Never, LocalPart = CurrentLocalPart, Domain = CurrentDomain, Phrase = CurrentPhrase, ObsRoute = Never, NullMembers = none, NullMember = Never) as ParseStrictMailboxList
	parse AddressList  with (Fws = CurrentFws, ObsNoWsCtl = Never, ObsQp = Never, ObsDtext = Never, LocalPart = CurrentLocalPart, Domain = CurrentDomain, Phrase = CurrentPhrase, ObsRoute = Never, NullMembers = none, NullMember = Never, ObsGroupList = Never) as ParseStrictAddressList
	""")]
static partial class Rfc5322
{
	// ParseAddrSpec, ParseMailbox, ParseMailboxList, ParseAddressList, their Strict readings and all their Try
	// forms are generated here.

	/// <summary>Whether a local part can be written as a dot-atom: atext, and dots between runs of it.</summary>
	public static bool IsDotAtomText(string text)
	{
		if (text is null)
			throw new ArgumentNullException(nameof(text));

		if (text.Length == 0 || text[0] == '.' || text[text.Length - 1] == '.')
			return false;

		for (var at = 0; at < text.Length; at++)
		{
			if (text[at] == '.')
			{
				if (text[at - 1] == '.')
					return false;
			}
			else if (!IsAtext(text[at]))
				return false;
		}

		return true;
	}

	// ── What the grammar calls ───────────────────────────────────────────────────

	/// <summary>A quoted-string's content (§3.2.4): the quotes, each CRLF of folding and each quoted-pair's backslash taken off.</summary>
	internal static string Unquoted(string body)
	{
		var output = new StringBuilder(body.Length);

		for (var at = 1; at < body.Length - 1; at++)
		{
			var character = body[at];

			if (character == '\\')
				output.Append(body[++at]);
			else if (character == '\r' && at + 1 < body.Length && body[at + 1] == '\n')
				at++;
			else
				output.Append(character);
		}

		return output.ToString();
	}

	/// <summary>A domain literal as it means (§3.4.1): its brackets, its dtext, folding and quoted-pairs undone.</summary>
	internal static string Literal(string body)
	{
		var output = new StringBuilder(body.Length);

		output.Append('[');

		for (var at = 1; at < body.Length - 1; at++)
		{
			var character = body[at];

			if (character == '\\')
				output.Append(body[++at]);
			else if (character == '\r' && at + 1 < body.Length && body[at + 1] == '\n')
				at++;
			else
				output.Append(character);
		}

		return output.Append(']').ToString();
	}

	internal static string Dotted(string first, string[] rest) =>
		rest.Length == 0 ? first : first + "." + string.Join(".", rest);

	internal static T[] Joined<T>(T first, T[][] rest)
	{
		var all = new List<T> { first };

		foreach (var part in rest)
			all.AddRange(part);

		return [.. all];
	}

	/// <summary>
	/// A phrase as it reads (§3.2.2, §3.2.5): comments dropped, folding undone, each run of whitespace or comments
	/// between tokens one space, quoted strings unquoted. Null for null.
	/// </summary>
	internal static string? DisplayName(string? phrase)
	{
		if (phrase is null)
			return null;

		var output = new StringBuilder(phrase.Length);
		var space  = false;

		for (var at = 0; at < phrase.Length; at++)
		{
			var character = phrase[at];

			switch (character)
			{
				case ' ' or '\t' or '\r' or '\n':
					space = true;
					break;

				case '(':
					for (var depth = 0; at < phrase.Length; at++)
					{
						if (phrase[at] == '\\')
							at++;
						else if (phrase[at] == '(')
							depth++;
						else if (phrase[at] == ')' && --depth == 0)
							break;
					}

					space = true;
					break;

				case '"':
					Separate();

					for (at++; phrase[at] != '"'; at++)
					{
						if (phrase[at] == '\\')
							output.Append(phrase[++at]);
						else if (phrase[at] is not ('\r' or '\n'))
							output.Append(phrase[at]);
					}

					break;

				default:
					Separate();
					output.Append(character);
					break;
			}
		}

		return output.ToString();

		void Separate()
		{
			if (space && output.Length > 0)
				output.Append(' ');

			space = false;
		}
	}

	/// <summary>A display name as a phrase §3 generates: atoms between single spaces where it is that, a quoted string otherwise.</summary>
	internal static void Phrase(StringBuilder output, string name)
	{
		var plain = name.Length > 0;

		foreach (var word in name.Split(' '))
			if (word.Length == 0 || !IsAtoms(word))
				plain = false;

		if (plain)
			output.Append(name);
		else
			Quoted(output, name);

		static bool IsAtoms(string word)
		{
			foreach (var character in word)
				if (!IsAtext(character))
					return false;

			return true;
		}
	}

	internal static void Quoted(StringBuilder output, string text)
	{
		output.Append('"');

		foreach (var character in text)
		{
			if (character is '"' or '\\')
				output.Append('\\');

			output.Append(character);
		}

		output.Append('"');
	}

	static bool IsAtext(char c) =>
		c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or
			'!' or '#' or '$' or '%' or '&' or '\'' or '*' or '+' or '-' or '/' or '=' or '?' or '^' or '_' or '`' or '{' or '|' or '}' or '~';
}
