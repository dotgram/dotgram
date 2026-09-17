namespace DotGram.Grammar.Emit;

public static partial class CSharpEmitter
{
	internal const string PagedValuesSupport = """
		/// <summary>Lazy pages indexed by stable record numbers, without a flat prefix.</summary>
		struct PagedValueTable<T>
		{
			Held<T>[][]? _pages;
			int _capacity;

			internal int Length => _capacity;

			// Materialization reads only records whose values have already been written.
			[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
			internal ref Held<T> Read(int record) => ref _pages![record >> 6][record & 63];

			internal ref Held<T> this[int record]
			{
				[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
				get
				{
					var pages = _pages;
					var index = record >> 6;
					if (pages != null && (uint)index < (uint)pages.Length)
					{
						var page = pages[index];
						if (page != null) return ref page[record & 63];
					}
					return ref Grow(record);
				}
			}

			ref Held<T> Grow(int record)
			{
				var index = record >> 6;
				if (_pages == null)
				{
					_pages = new Held<T>[global::System.Math.Max(8, index + 1)][];
					_capacity += _pages.Length;
				}
				else if (index >= _pages.Length)
				{
					var size = global::System.Math.Max(index + 1, _pages.Length * 2);
					_capacity += size - _pages.Length;
					global::System.Array.Resize(ref _pages, size);
				}
				var page = _pages[index];
				if (page == null)
				{
					page = _pages[index] = new Held<T>[64];
					_capacity += page.Length;
				}
				return ref page[record & 63];
			}

			internal void Clear(int count)
			{
				if (_pages == null) return;
				var end = global::System.Math.Min(_pages.Length, ((count - 1) >> 6) + 1);
				for (var index = 0; index < end; index++)
				{
					var page = _pages[index];
					if (page != null) global::System.Array.Clear(page, 0, page.Length);
				}
			}
		}
		""";
}
