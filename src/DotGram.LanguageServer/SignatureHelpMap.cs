using System;
using System.Linq;

using DotGram.Language;

namespace DotGram.LanguageServer;

/// <summary>The signature help provider's answer as <c>textDocument/signatureHelp</c> carries it.</summary>
static class SignatureHelpMap
{
	public static SignatureHelp Map(GramSignatureHelp help)
	{
		return new SignatureHelp(
			help.Signatures
				.Select(static signature => new SignatureInformation(
					signature.Label,
					signature.Documentation.Length == 0 ? null : MarkupContent.PlainText(signature.Documentation),
					signature.Parameters
						.Select(static parameter => new ParameterInformation(
							[parameter.Locus.Start, parameter.Locus.End],
							parameter.Documentation.Length == 0 ? null : MarkupContent.PlainText(parameter.Documentation)))
						.ToArray()))
				.ToArray(),
			Math.Max(0, help.ActiveSignature),
			help.ActiveParameter >= 0 ? help.ActiveParameter : null);
	}
}
