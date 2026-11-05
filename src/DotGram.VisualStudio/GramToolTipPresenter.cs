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

	// Only the choice of the next factory is guarded: what that factory does is Visual Studio's.
	// Without one this factory presents nothing either, and the next in order is asked instead.
	public IToolTipPresenter Create(ITextView textView, ToolTipParameters parameters)
	{
		var next = EditorGuard.Run<IToolTipPresenterFactory?>(
			"Tooltip presenter",
			null,
			() => Orderer.Order(Factories).First(factory => factory.Metadata.Name != Name).Value);

		if (next is null)
			return null!;

		return new GramToolTipPresenter(next.Create(textView, parameters));
	}
}

sealed class GramToolTipPresenter(IToolTipPresenter inner) : IToolTipPresenter
{
	public event EventHandler? Dismissed
	{
		add => inner.Dismissed += value;
		remove => inner.Dismissed -= value;
	}

	// Only the filtering of the content is guarded: should it fail, Visual Studio's presenter is
	// handed what it was given, as if this one were not there. The presenter's own failures are
	// its own.
	public void StartOrUpdate(ITrackingSpan applicableToSpan, IEnumerable<object> content)
	{
		var shown = EditorGuard.Run<Shown?>("Tooltip content", null, () => Filter(applicableToSpan, content));

		if (shown is null)
			inner.StartOrUpdate(applicableToSpan, content);
		else if (shown.Dismiss)
			inner.Dismiss();
		else
			inner.StartOrUpdate(shown.Span, shown.Items);
	}

	static Shown Filter(ITrackingSpan applicableToSpan, IEnumerable<object> content)
	{
		var items = content.ToArray();
		var markers = items.OfType<IDotGramQuickInfoContent>().ToArray();

		if (markers.Length == 0)
			return new Shown(applicableToSpan, items, false);

		var visible = markers.Where(static item => item.ShouldDisplay).Cast<object>().ToArray();
		return new Shown(markers[0].TrackingSpan, visible, visible.Length == 0);
	}

	sealed record Shown(ITrackingSpan Span, object[] Items, bool Dismiss);

	public void Dismiss()
	{
		inner.Dismiss();
	}
}
