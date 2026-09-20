namespace DotGram.Finance.Fix;

/// <summary>One member of a scope: a field, a component, or a group.</summary>
/// <param name="id">The tag for a field, the component's id, or the group's id.</param>
/// <param name="required">Whether the scope requires it.</param>
/// <param name="kind">0 a field, 1 a component, 2 a group.</param>
/// <remarks>
/// In a file of its own because the generator package compiles this same source rather than
/// referencing the assembly it is declared in: one reader of a dictionary, two compilations, and
/// no copy of anything to fall out of step.
/// </remarks>
readonly struct SchemaRef(int id, bool required, int kind)
{
	public readonly int  Id       = id;
	public readonly bool Required = required;
	public readonly int  Kind     = kind;
}
