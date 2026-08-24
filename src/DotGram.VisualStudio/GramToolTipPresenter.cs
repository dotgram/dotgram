using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace DotGram.VisualStudio;

interface IDotGramQuickInfoContent
{
	bool ShouldDisplay { get; }
	ITrackingSpan TrackingSpan { get; }
}

sealed class DotGramQuickInfoSuppression(ITrackingSpan trackingSpan) : IDotGramQuickInfoContent
{
	public bool ShouldDisplay => false;
	public ITrackingSpan TrackingSpan { get; } = trackingSpan;
}

[Export(typeof(IToolTipPresenterFactory))]
[Name(Name)]
[Order(Before = "default")]
sealed class GramToolTipPresenterFactory : IToolTipPresenterFactory
{
	const string Name = "DotGram tooltip presenter";

	[ImportMany]
	IEnumerable<Lazy<IToolTipPresenterFactory, IOrderable>> Factories { get; set; } = null!;

	public IToolTipPresenter Create(ITextView textView, ToolTipParameters parameters)
	{
		var inner = Orderer.Order(Factories)
			.First(factory => factory.Metadata.Name != Name)
			.Value
			.Create(textView, parameters);

		return new GramToolTipPresenter(inner);
	}
}

sealed class GramToolTipPresenter(IToolTipPresenter inner) : IToolTipPresenter
{
	public event EventHandler? Dismissed
	{
		add => inner.Dismissed += value;
		remove => inner.Dismissed -= value;
	}

	public void StartOrUpdate(ITrackingSpan applicableToSpan, IEnumerable<object> content)
	{
		var items = content.ToArray();
		var markers = items.OfType<IDotGramQuickInfoContent>().ToArray();

		if (markers.Length == 0)
		{
			inner.StartOrUpdate(applicableToSpan, items);
			return;
		}

		var visible = markers.Where(static item => item.ShouldDisplay).Cast<object>().ToArray();
		if (visible.Length == 0)
			inner.Dismiss();
		else
			inner.StartOrUpdate(markers[0].TrackingSpan, visible);
	}

	public void Dismiss() => inner.Dismiss();
}
