using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DotGram.VisualStudio;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
[Guid(PackageGuid)]
public sealed class DotGramPackage : AsyncPackage, IVsRunningDocTableEvents3
{
	public const string PackageGuid = "10462EA6-7017-4214-BD5B-4A8EB9DA54B6";
	readonly Dictionary<IVsWindowFrame, GramCodeWindowManager> _windows = [];
	IVsRunningDocumentTable? _documents;
	uint _documentEvents;

	protected override async Task InitializeAsync(
		CancellationToken cancellationToken,
		IProgress<ServiceProgressData> progress)
	{
		await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
		_documents = await GetServiceAsync(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable
			?? throw new InvalidOperationException("Visual Studio running document table is unavailable.");
		_documents.AdviseRunningDocTableEvents(this, out _documentEvents);
	}

	protected override void Dispose(bool disposing)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		if (disposing)
		{
			foreach (var window in _windows.Values)
				window.RemoveAdornments();
			_windows.Clear();
			if (_documentEvents != 0)
				_documents?.UnadviseRunningDocTableEvents(_documentEvents);
		}

		base.Dispose(disposing);
	}

	public int OnBeforeDocumentWindowShow(uint cookie, int firstShow, IVsWindowFrame frame)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		if (_documents is null || _windows.ContainsKey(frame) || !IsGramDocument(cookie))
			return VSConstants.S_OK;

		frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var docView);
		if (docView is not IVsCodeWindow codeWindow)
			return VSConstants.S_OK;

		var manager = new GramCodeWindowManager(codeWindow);
		if (ErrorHandler.Succeeded(manager.AddAdornments()))
			_windows.Add(frame, manager);

		return VSConstants.S_OK;
	}

	public int OnAfterDocumentWindowHide(uint cookie, IVsWindowFrame frame)
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		if (_windows.TryGetValue(frame, out var manager))
		{
			manager.RemoveAdornments();
			_windows.Remove(frame);
		}

		return VSConstants.S_OK;
	}

	bool IsGramDocument(uint cookie)
	{
		_documents!.GetDocumentInfo(
			cookie,
			out _,
			out _,
			out _,
			out var moniker,
			out _,
			out _,
			out var docData);
		if (docData != IntPtr.Zero)
			Marshal.Release(docData);
		return moniker?.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) == true;
	}

	public int OnAfterFirstDocumentLock(uint cookie, uint lockType, uint readLocks, uint editLocks) => VSConstants.S_OK;
	public int OnBeforeLastDocumentUnlock(uint cookie, uint lockType, uint readLocks, uint editLocks) => VSConstants.S_OK;
	public int OnAfterSave(uint cookie) => VSConstants.S_OK;
	public int OnAfterAttributeChange(uint cookie, uint attributes) => VSConstants.S_OK;
	public int OnAfterAttributeChangeEx(
		uint cookie,
		uint attributes,
		IVsHierarchy oldHierarchy,
		uint oldItemId,
		string oldMoniker,
		IVsHierarchy newHierarchy,
		uint newItemId,
		string newMoniker) => VSConstants.S_OK;
	public int OnBeforeSave(uint cookie) => VSConstants.S_OK;
}

sealed class GramCodeWindowManager(IVsCodeWindow codeWindow) : IVsCodeWindowManager
{
	readonly IVsCodeWindow _codeWindow = codeWindow;
	GramDropdownClient? _client;

	public int AddAdornments()
	{
		ThreadHelper.ThrowIfNotOnUIThread();

		if (_codeWindow is not IVsDropdownBarManager dropdowns)
			return VSConstants.E_NOINTERFACE;

		dropdowns.GetDropdownBar(out var existing);
		if (existing is not null)
			return VSConstants.S_OK;

		_codeWindow.GetPrimaryView(out var view);
		if (view is null)
			return VSConstants.E_FAIL;

		var componentModel = ServiceProvider.GlobalProvider.GetService(typeof(SComponentModel)) as IComponentModel;
		var adapters = componentModel?.GetService<IVsEditorAdaptersFactoryService>();
		var textView = adapters?.GetWpfTextView(view);
		if (textView is null)
			return VSConstants.E_FAIL;

		_client = new GramDropdownClient(textView);
		return dropdowns.AddDropdownBar(1, _client);
	}

	public int RemoveAdornments()
	{
		ThreadHelper.ThrowIfNotOnUIThread();
		_client?.Dispose();
		_client = null;
		return _codeWindow is IVsDropdownBarManager dropdowns
			? dropdowns.RemoveDropdownBar()
			: VSConstants.S_OK;
	}

	public int OnNewView(IVsTextView view) => VSConstants.S_OK;
}

sealed class GramDropdownClient : IVsDropdownBarClient, IDisposable
{
	readonly IWpfTextView _view;
	readonly GramBufferAnalysis _analysis;
	readonly List<Item> _items = [];
	IVsDropdownBar? _dropdown;

	public GramDropdownClient(IWpfTextView view)
	{
		_view = view;
		_analysis = GramBufferAnalysis.For(view.TextBuffer);
		_analysis.Changed += AnalysisChanged;
		_view.Caret.PositionChanged += CaretChanged;
		Refresh(view.TextSnapshot);
	}

	public int SetDropdownBar(IVsDropdownBar dropdown)
	{
		_dropdown = dropdown;
		SelectAtCaret();
		return VSConstants.S_OK;
	}

	public int GetComboAttributes(
		int combo,
		out uint entries,
		out uint entryType,
		out IntPtr imageList)
	{
		entries = (uint)_items.Count;
		entryType = 0;
		imageList = IntPtr.Zero;
		return VSConstants.S_OK;
	}

	public int GetEntryText(int combo, int index, out string text)
	{
		text = Valid(index) ? _items[index].Display : "";
		return Valid(index) ? VSConstants.S_OK : VSConstants.E_INVALIDARG;
	}

	public int GetEntryAttributes(int combo, int index, out uint attributes)
	{
		attributes = 0;
		return Valid(index) ? VSConstants.S_OK : VSConstants.E_INVALIDARG;
	}

	public int GetEntryImage(int combo, int index, out int image)
	{
		image = -1;
		return Valid(index) ? VSConstants.S_OK : VSConstants.E_INVALIDARG;
	}

	public int OnItemSelected(int combo, int index) => VSConstants.S_OK;

	public int OnItemChosen(int combo, int index)
	{
		if (!Valid(index))
			return VSConstants.E_INVALIDARG;

		var snapshot = _view.TextSnapshot;
		var position = Math.Min(_items[index].SelectionPosition, snapshot.Length);
		var point = new SnapshotPoint(snapshot, position);
		_view.Caret.MoveTo(point);
		_view.ViewScroller.EnsureSpanVisible(new SnapshotSpan(point, 0));
		_view.VisualElement.Focus();
		return VSConstants.S_OK;
	}

	public int OnComboGetFocus(int combo) => VSConstants.S_OK;

	public int GetComboTipText(int combo, out string text)
	{
		text = "DotGram declarations";
		return VSConstants.S_OK;
	}

	void AnalysisChanged(ITextSnapshot snapshot)
	{
		if (snapshot != _view.TextSnapshot)
			return;

		Refresh(snapshot);
		_dropdown?.RefreshCombo(0, 0);
		SelectAtCaret();
	}

	void CaretChanged(object sender, CaretPositionChangedEventArgs args) => SelectAtCaret();

	void Refresh(ITextSnapshot snapshot)
	{
		_items.Clear();
		Append(_analysis.Document(snapshot).DocumentSymbols, 0);
	}

	void Append(IReadOnlyList<GramDocumentSymbol> symbols, int depth)
	{
		foreach (var symbol in symbols)
		{
			_items.Add(new Item(
				new string(' ', depth * 2) + symbol.Name,
				symbol.Position,
				symbol.Length,
				symbol.SelectionPosition));
			Append(symbol.Children, depth + 1);
		}
	}

	void SelectAtCaret()
	{
		if (_dropdown is null)
			return;

		var position = _view.Caret.Position.BufferPosition.Position;
		var selected = -1;
		var selectedLength = int.MaxValue;
		for (var index = 0; index < _items.Count; index++)
		{
			var item = _items[index];
			if (item.Position <= position && position < item.Position + item.Length &&
				item.Length < selectedLength)
			{
				selected = index;
				selectedLength = item.Length;
			}
		}

		_dropdown.SetCurrentSelection(0, selected);
	}

	bool Valid(int index) => index >= 0 && index < _items.Count;

	public void Dispose()
	{
		_analysis.Changed -= AnalysisChanged;
		_view.Caret.PositionChanged -= CaretChanged;
		_dropdown = null;
	}

	readonly record struct Item(string Display, int Position, int Length, int SelectionPosition);
}
