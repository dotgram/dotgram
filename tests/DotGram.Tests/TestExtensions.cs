using System;
using System.Diagnostics.CodeAnalysis;

namespace DotGram.Tests;

public static class TestExtensions
{
	public static string AsDotGram([StringSyntax("DotGram")] this string value)
	{
		return value;
	}
}
