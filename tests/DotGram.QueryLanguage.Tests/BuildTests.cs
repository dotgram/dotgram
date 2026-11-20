using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace DotGram.QueryLanguage.Tests;

/// <summary>What the builder accepts, and what it refuses at the call or at <c>Build</c>.</summary>
public sealed class BuildTests
{
	public sealed class Item
	{
		public int       Id     { get; set; }
		public string?   Name   { get; set; }
		public decimal?  Price  { get; set; }
		public DateOnly  Day    { get; set; }
		public byte[]?   Blob   { get; set; }
		public string    Field = "";
		public string    this[int i] => "";
		public static int Shared { get; set; }
		public string    WriteOnly { set { } }
		public Item?     Parent { get; set; }
		public System.Collections.Generic.List<string> Tags { get; set; } = [];
		public int[]     Codes  { get; set; } = [];
		public object?   Meta   { get; set; }
	}

	public sealed class Db
	{
		public IQueryable<Item> Items      { get; } = Array.Empty<Item>().AsQueryable();
		public IQueryable<Item> OtherItems { get; } = Array.Empty<Item>().AsQueryable();
		public IQueryable<Item> items      => Items;

		public IQueryable<Item> Make()
		{
			return Items;
		}
	}

	[Fact]
	public void AnEntityWithoutANameTakesTheMembersName()
	{
		var builder = new QlModelBuilder<Db>();

		builder.Entity(db => db.Items).Column(i => i.Id);
		builder.Entity(db => db.OtherItems).Column(i => i.Id);

		var model = builder.Build();

		Assert.Equal(["Items", "OtherItems"], model.Entities.Values.Select(e => e.Name).Order());
	}

	[Fact]
	public void AnEntityWithoutANameIsAMemberOfTheContextAndNothingElse()
	{
		var builder = new QlModelBuilder<Db>();

		Assert.Throws<ArgumentException>(() => builder.Entity(db => db.Items.Where(i => i.Id > 0)));
		Assert.Throws<ArgumentException>(() => builder.Entity(db => db.Make()));
		Assert.Throws<ArgumentException>(() => builder.Entity<Item>(db => Array.Empty<Item>().AsQueryable()));
	}

	[Fact]
	public void AnEntityWithANameTakesAnySource()
	{
		var builder = new QlModelBuilder<Db>();

		builder.Entity("items", db => db.Items.Where(i => i.Id > 0)).Column(i => i.Id);

		Assert.Equal("items", builder.Build().Entities["ITEMS"].Name);
	}

	[Fact]
	public void AColumnIsAMemberReadDirectly()
	{
		var builder = new QlModelBuilder<Db>();
		var entity  = builder.Entity(db => db.Items);

		entity.Column(i => i.Id).Column("f", i => i.Field);

		Assert.Throws<ArgumentException>(() => entity.Column(i => i.Id + 1));
		Assert.Throws<ArgumentException>(() => entity.Column(i => i.Parent!.Id));

		// A navigation or a collection is not a field: it would be returned whole.
		Assert.Contains("not another entity or a collection", Assert.Throws<ArgumentException>(() => entity.Column(i => i.Parent)).Message, StringComparison.Ordinal);
		Assert.Throws<ArgumentException>(() => entity.Column(i => i.Tags));
		Assert.Throws<ArgumentException>(() => entity.Column(i => i.Codes));
		Assert.Throws<ArgumentException>(() => entity.Column(i => i.Meta));
		Assert.Throws<ArgumentException>(() => entity.Column(i => i[0]));
		Assert.Throws<ArgumentException>(() => entity.Column(i => Item.Shared));
		Assert.Throws<ArgumentException>(() => entity.Column(i => (object)i.Id));
		Assert.Throws<ArgumentException>(() => entity.Column(i => 1));
	}

	[Fact]
	public void AColumnWithoutUsesHasTheThree()
	{
		var builder = new QlModelBuilder<Db>();

		builder.Entity(db => db.Items).Column(i => i.Id).Column(i => i.Name, QlUse.Select);

		var fields = builder.Build().Entities["Items"].Fields;

		Assert.Equal(QlUse.Select | QlUse.Filter | QlUse.Sort, fields[0].Use);
		Assert.Equal(QlUse.Select,                              fields[1].Use);
	}

	[Fact]
	public void NoUseOrAnUnknownOneIsRefused()
	{
		var entity = new QlModelBuilder<Db>().Entity(db => db.Items);

		Assert.Throws<ArgumentException>(() => entity.Column(i => i.Id, 0));
		Assert.Throws<ArgumentException>(() => entity.Column(i => i.Id, (QlUse)8));
		Assert.Throws<ArgumentException>(() => entity.Column(i => i.Id, QlUse.Select | (QlUse)16));
	}

	[Fact]
	public void MemberTypesOutsideTheNineAreFieldsOfTypeOther()
	{
		var builder = new QlModelBuilder<Db>();

		builder.Entity(db => db.Items).Column(i => i.Id).Column(i => i.Name).Column(i => i.Price).Column(i => i.Day).Column(i => i.Blob);

		var fields = builder.Build().Entities["Items"].Fields;

		Assert.Equal((QlType.Int,     false, typeof(int)),      Describe(fields[0]));
		Assert.Equal((QlType.String,  true,  typeof(string)),   Describe(fields[1]));
		Assert.Equal((QlType.Decimal, true,  typeof(decimal)),  Describe(fields[2]));
		Assert.Equal((QlType.Other,   false, typeof(DateOnly)), Describe(fields[3]));
		Assert.Equal((QlType.Other,   true,  typeof(byte[])),   Describe(fields[4]));

		static (QlType, bool, Type) Describe(ModelField field)
		{
			return (field.Value.Type, field.Value.Nullable, field.Value.Core);
		}
	}

	[Theory]
	[InlineData("1st")]
	[InlineData("total price")]
	[InlineData("цена")]
	[InlineData("")]
	[InlineData("a-b")]
	[InlineData("null")]
	[InlineData("TRUE")]
	[InlineData("False")]
	public void ANameTheLanguageCannotWriteIsRefusedAtBuild(string name)
	{
		var column = new QlModelBuilder<Db>();

		column.Entity("items", db => db.Items).Column(name, i => i.Id);

		Assert.Throws<ArgumentException>(() => column.Build());

		var entity = new QlModelBuilder<Db>();

		entity.Entity(name, db => db.Items).Column(i => i.Id);

		Assert.Throws<ArgumentException>(() => entity.Build());
	}

	[Fact]
	public void ADerivedEntityNameIsCheckedLikeAWrittenOne()
	{
		var builder = new QlModelBuilder<Db>();

		builder.Entity(db => db.Items);
		builder.Entity(db => db.items);

		var error = Assert.Throws<ArgumentException>(() => builder.Build());

		Assert.Contains("`Items`", error.Message);
		Assert.Contains("`items`", error.Message);
	}

	[Fact]
	public void NamesThatDifferOnlyInCaseAreRefused()
	{
		var builder = new QlModelBuilder<Db>();

		builder.Entity(db => db.Items).Column("name", i => i.Name).Column("Name", i => i.Field);

		var error = Assert.Throws<ArgumentException>(() => builder.Build());

		Assert.Contains("`name`", error.Message);
		Assert.Contains("`Name`", error.Message);
	}

	[Fact]
	public void OneMemberMayHaveTwoNames()
	{
		var builder = new QlModelBuilder<Db>();

		builder.Entity(db => db.Items).Column("amount", i => i.Price).Column("price", i => i.Price, QlUse.Select);

		Assert.Equal(2, builder.Build().Entities["Items"].Fields.Length);
	}

	[Fact]
	public void AFormatIsForExactlyOneTypeOnce()
	{
		var builder = new QlModelBuilder<Db>();

		builder.Format<DateOnly>(d => d.ToString("O")).Format<byte[]>(Convert.ToBase64String).Format<DateTime>(d => d.ToString("O"));

		Assert.Throws<ArgumentException>(() => builder.Format<DateOnly>(d => ""));
		Assert.Throws<ArgumentException>(() => builder.Format<int?>(d => ""));
		Assert.Throws<ArgumentException>(() => builder.Format<string>(s => s));
		Assert.Throws<ArgumentException>(() => builder.Format<object>(o => ""));
		Assert.Throws<ArgumentException>(() => builder.Format<IFormattable>(o => ""));
		Assert.Throws<ArgumentException>(() => builder.Format<Enum>(o => ""));
		Assert.Throws<ArgumentException>(() => builder.Format<System.IO.Stream>(o => ""));
		Assert.Throws<ArgumentNullException>(() => builder.Format<Guid>(null!));

		Assert.Equal(3, builder.Build().Formats.Count);
	}

	[Theory]
	[InlineData(nameof(QlOptions.MaxTextLength))]
	[InlineData(nameof(QlOptions.MaxSyntaxDepth))]
	[InlineData(nameof(QlOptions.MaxNodes))]
	[InlineData(nameof(QlOptions.MaxTreeDepth))]
	[InlineData(nameof(QlOptions.MaxArithmeticOperands))]
	[InlineData(nameof(QlOptions.MaxInList))]
	[InlineData(nameof(QlOptions.MaxColumns))]
	[InlineData(nameof(QlOptions.MaxRows))]
	[InlineData(nameof(QlOptions.MaxStringLength))]
	[InlineData(nameof(QlOptions.MaxParameterBytes))]
	public void ABoundThatIsNotPositiveIsRefused(string bound)
	{
		var options = typeof(QlOptions).GetProperty(bound)!;
		var zero    = QlOptions.Default with { };

		options.SetValue(zero, 0);

		Assert.Throws<ArgumentException>(() => new QlModelBuilder<Db>().Build(zero));
	}

	[Fact]
	public void TheDefaultsAreTheDocumentedOnes()
	{
		var options = QlOptions.Default;

		Assert.Equal(
			(8192, 64, 100, 32, 8, 100, 32, (int?)1000, 10_000, 4096, 16384),
			(options.MaxTextLength, options.MaxSyntaxDepth, options.MaxNodes, options.MaxTreeDepth, options.MaxArithmeticOperands,
				options.MaxInList, options.MaxColumns, options.MaxRows, options.MaxSkip, options.MaxStringLength, options.MaxParameterBytes));

		Assert.Null(new QlModelBuilder<Db>().Build(options with { MaxRows = null }).Options.MaxRows);
		Assert.Throws<ArgumentException>(() => new QlModelBuilder<Db>().Build(options with { MaxSkip = -1 }));
		Assert.Equal(0, new QlModelBuilder<Db>().Build(options with { MaxSkip = 0 }).Options.MaxSkip);
	}
}
