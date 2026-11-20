using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;

using DotGram.QueryLanguage;

// What the package depends on, read from the package itself: nothing on net10.0, and on netstandard2.0 exactly
// the two polyfills — never linq2db, which a host brings for itself.
var root = AppContext.BaseDirectory;

while (root is not null && !Directory.Exists(Path.Combine(root, "artifacts")))
	root = Path.GetDirectoryName(root.TrimEnd(Path.DirectorySeparatorChar));

if (root is null)
	throw new Exception("No artifacts folder above " + AppContext.BaseDirectory + ".");

var package = Directory.GetFiles(Path.Combine(root, "artifacts"), "DotGram.QueryLanguage.*.nupkg")
	.Where(p => !p.EndsWith(".symbols.nupkg", StringComparison.Ordinal))
	.Single();

using (var zip = ZipFile.OpenRead(package))
{
	var entry  = zip.Entries.Single(e => e.FullName.EndsWith(".nuspec", StringComparison.Ordinal));
	var nuspec = XDocument.Load(entry.Open());
	var groups = nuspec.Descendants().Where(e => e.Name.LocalName == "group").ToList();

	string Dependencies(string framework)
	{
		var group = groups.Single(g => (string?)g.Attribute("targetFramework") == framework);

		return string.Join(", ", group.Elements().Select(d => $"{d.Attribute("id")!.Value} {d.Attribute("version")!.Value}").OrderBy(d => d, StringComparer.Ordinal));
	}

	if (Dependencies("net10.0") != "")
		throw new Exception("On net10.0 the package depends on " + Dependencies("net10.0") + ".");

	if (Dependencies(".NETStandard2.0") != "Microsoft.Bcl.AsyncInterfaces 8.0.0, System.Memory 4.6.3")
		throw new Exception("On netstandard2.0 the package depends on " + Dependencies(".NETStandard2.0") + ".");
}

// A model, as a host declares one.
var builder = new QlModelBuilder<Shop>();

builder.Entity(db => db.Orders).Column(o => o.Id).Column("total", o => o.Total, QlUse.Select | QlUse.Filter);

var model = builder.Build(QlOptions.Default with { MaxRows = 10 });

if (model.Options.MaxRows != 10)
	throw new Exception("The options were not kept.");

// A text described and run, here by LINQ to Objects: the tree is the one a database provider would be handed.
var info = model.Describe("from Orders where total > @min order by Id desc select Id, total * 2 as twice");

if (string.Join(",", info.Columns.Select(c => $"{c.Name}:{c.Type}")) != "Id:Int,twice:Decimal" || info.Parameters.Single().Name != "min")
	throw new Exception("The text was described wrongly.");

var rows = model.QueryData(new Shop(), "from Orders where total > @min order by Id desc select Id, total * 2 as twice", new Dictionary<string, object?> { ["min"] = 5 }).ToList();

if (rows.Count != 2 || (int)rows[0][0]! != 2 || (decimal)rows[0][1]! != 40m)
	throw new Exception("The query returned the wrong rows.");

try
{
	model.Describe("from Orders where nosuch > 1");

	throw new Exception("An unknown name was accepted.");
}
catch (QlException e) when (e.Error.Code == QlErrorCode.UnknownName && e.Error.Position == 18)
{
}

Console.WriteLine($"DotGram.QueryLanguage package smoke: dependencies as declared, a text described and run ({RuntimeInformation.FrameworkDescription}).");

sealed class Order
{
	public int     Id    { get; set; }
	public decimal Total { get; set; }
}

sealed class Shop
{
	public IQueryable<Order> Orders { get; } = new[] { new Order { Id = 1, Total = 10m }, new Order { Id = 2, Total = 20m } }.AsQueryable();
}
