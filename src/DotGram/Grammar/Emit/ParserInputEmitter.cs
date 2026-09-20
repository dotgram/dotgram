using System;

namespace DotGram.Grammar.Emit;

public static partial class CSharpEmitter
{
	const string ParserInputTypes = """
		internal interface IParserInputSource<T>
		{
			bool Ensure(int position, int length);
			global::System.ReadOnlySpan<T> Slice(int position, int length);
		}

		/// <summary>Borrowed input for an external recognizer. Positions are absolute.</summary>
		public readonly ref struct ParserInput<T>
		{
			private readonly global::System.ReadOnlySpan<T> _span;
			private readonly IParserInputSource<T>? _source;

			internal ParserInput(global::System.ReadOnlySpan<T> span)
			{
				_span   = span;
				_source = null;
			}

			internal ParserInput(IParserInputSource<T> source)
			{
				_span   = default;
				_source = source;
			}

			/// <summary>Reads as needed; false means the requested extent reaches past EOF.</summary>
			public bool Ensure(int position, int length)
			{
				if (position < 0) throw new global::System.ArgumentOutOfRangeException(nameof(position));
				if (length < 0) throw new global::System.ArgumentOutOfRangeException(nameof(length));
				if (length > int.MaxValue - position) return false;

				return _source != null ? _source.Ensure(position, length) :
					position <= _span.Length && length <= _span.Length - position;
			}

			/// <summary>Advances only when the entire extent is available.</summary>
			public bool TryAdvance(ref int position, int length)
			{
				if (!Ensure(position, length)) return false;

				position += length;
				return true;
			}

			/// <summary>The item at <paramref name="position"/>, or false where the input ended before it.</summary>
			public bool Peek(int position, out T value)
			{
				if (!Ensure(position, 1))
				{
					value = default!;
					return false;
				}

				value = Slice(position, 1)[0];
				return true;
			}

			/// <summary>The span is valid until the next input operation or callback return.</summary>
			public global::System.ReadOnlySpan<T> Slice(int position, int length)
			{
				if (!Ensure(position, length)) throw new global::System.ArgumentOutOfRangeException(nameof(length));

				return _source != null ? _source.Slice(position, length) : _span.Slice(position, length);
			}
		}
		""";
}
