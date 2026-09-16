using System;

namespace DotGram.Finance.Fix;

/// <summary>Receives source coordinates from the generated parser.</summary>
public interface IFixLocation
{
	void Locate(int position, int length);
}

/// <summary>One case in the FIX field algebra, with its original source extent.</summary>
public abstract partial class FixField : IFixLocation
{
	int prefixLength;
	internal bool IsBinary => prefixLength != TagPrefixLength;
	internal int DataPosition => ValuePosition - TagPrefixLength;
	int TagPrefixLength
	{
		get
		{
			var prefix = 2;
			for (var digits = Tag; digits >= 10; digits /= 10) prefix++;
			return prefix;
		}
	}

	protected FixField(int tag, bool valid)
	{
		Tag = tag;
		prefixLength = 2;
		for (var digits = tag; digits >= 10; digits /= 10) prefixLength++;
		IsValid = valid;
	}

	public int Tag { get; }
	public int Position { get; private set; }
	public int ValuePosition => Position + prefixLength;
	public int Length { get; private set; }

	/// <summary>Called by the parser, innermost rule first; Field supplies the complete tag=value extent.</summary>
	public void Locate(int position, int length)
	{
		Position = position;
		Length = length - prefixLength - 1;
	}

	internal FixField WithBinary(FixBinaryValue value, int start)
	{
		// The source extent begins at the length tag; the value begins after both headers.
		prefixLength = value.Position - start;
		return this;
	}

	/// <summary>Whether conversion to the declared primitive succeeded. Raw input is retained in Lenient mode.</summary>
	public bool IsValid { get; }

	public sealed class Unknown : FixField<ReadOnlyMemory<byte>>
	{
		internal Unknown(int tag, (bool Valid, ReadOnlyMemory<byte> Value) parsed)
			: base(tag, parsed) { }
	}
}

public abstract class FixField<T> : FixField
{
	readonly T value;

	protected FixField(int tag, T value) : base(tag, true) => this.value = value;

	protected FixField(int tag, (bool Valid, T Value) parsed) : base(tag, parsed.Valid)
		=> value = parsed.Value;

	/// <summary>The converted value; throws when a Lenient field has invalid primitive syntax.</summary>
	public T Value => IsValid ? value : throw new InvalidOperationException("The field has no valid typed value; inspect its original wire value.");

	public bool TryGetValue(out T result)
	{
		result = value;
		return IsValid;
	}
}
