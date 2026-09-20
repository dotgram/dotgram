namespace DotGram.Finance.Fix;

/// <summary>Holding a field's value to what its schema says it is.</summary>
/// <remarks>
/// One implementation, called and never copied. A rule written by hand for one message type needs
/// exactly this, and so does code a dictionary is compiled into — which lives in the consumer's own
/// assembly and therefore cannot reach anything internal. Emitting a second copy of these rules
/// would be a second thing to disagree with the first, and three quarters of what it costs is
/// reading the characters of the value, which no arrangement of the code removes.
/// </remarks>
public static class FixValues
{
	/// <summary>Whether a value fits a type, and is one of the code set where one is given.</summary>
	/// <param name="field">The field, whose tag settles the one rule that needs it: IOIQty takes either a quantity or a code.</param>
	/// <param name="type">The type to hold the value to; <see cref="FixValueType.None"/> answers false.</param>
	/// <param name="codes">The values the schema allows, or null where it allows any of the type.</param>
	public static bool Valid(FixFieldView field, FixValueType type, string[]? codes) =>
		FixPrimitives.Valid(field, type, codes);

	/// <summary>The type of a tag in the schema this package compiles in, which is FIX 4.4's.</summary>
	/// <param name="tag">The numeric FIX tag.</param>
	/// <returns><see cref="FixValueType.None"/> where the standard does not define the tag.</returns>
	public static FixValueType Type(int tag) => FixSchema.Type(tag);
}
