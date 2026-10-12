using System;

namespace DotGram.Sql.Ast;

public static partial class Sql2023Writer
{
	sealed partial class Text
	{
		/// <summary>
		/// SQL-92's nodes (Sql2023Ast.Sql92.cs): a bit string type and a bit string literal. Answers false for every
		/// other node. A union join is written by <see cref="PutUnionJoin"/>, from the join's own arm.
		/// </summary>
		bool TryPutSql92(ISqlNode node)
		{
			switch (node)
			{
				case DataType.BitString bits:
					PutTypeName(bits, bits.Kind switch
					{
						BitStringTypeKind.Bit        => "BIT",
						BitStringTypeKind.BitVarying => "BIT VARYING",
						_                            => throw NoText(bits.Kind),
					});

					if (bits.Length is { } length)
					{
						Call();
						Number(length);
						Close();
					}

					return true;

				case LiteralValue.BitString bits:
					if (!bits.Text.StartsWith("'", StringComparison.Ordinal))
						throw Misplaced(bits, "Text", "without quotes");

					Word("B" + bits.Text);
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// <c>UNION JOIN</c> between a join's two sources. It joins on nothing and is neither natural nor outer, and
		/// takes no partitioning and no hint, so each of those is refused rather than dropped.
		/// </summary>
		void PutUnionJoin(TableSource.Join join)
		{
			if (join.Specification is not null)
				throw Misplaced(join, "Specification", "on a union join");

			if (join.Natural)
				throw Misplaced(join, "Natural", "on a union join");

			if (join.OuterKeyword)
				throw Misplaced(join, "OuterKeyword", "on a union join");

			if (join.LeftPartition is not null || join.RightPartition is not null)
				throw Misplaced(join, "a partitioning", "on a union join");

			if (join.Hint is not null)
				throw Misplaced(join, "Hint", "on a union join");

			Word("UNION JOIN");
		}
	}
}
