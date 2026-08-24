using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;

namespace DotGram.VisualStudio;

static class GramRenameAdornment
{
	static readonly Dictionary<IWpfTextView, Func<Guid, uint, bool>> CommandHandlers = new();

	public static bool TryHandleCommand(IWpfTextView view, Guid group, uint commandId) =>
		CommandHandlers.TryGetValue(view, out var handler) && handler(group, commandId);

	public static void Show(IWpfTextView view, string name, Action<string> apply)
	{
		var textBox = new TextBox
		{
			Text = name,
			MinWidth = 180,
			Padding = new Thickness(4, 2, 4, 2),
			BorderThickness = new Thickness(1),
			BorderBrush = SystemColors.HighlightBrush,
			Background = SystemColors.WindowBrush,
			Foreground = SystemColors.WindowTextBrush,
			FontFamily = view.FormattedLineSource.DefaultTextProperties.Typeface.FontFamily,
			FontSize = view.FormattedLineSource.DefaultTextProperties.FontRenderingEmSize,
		};
		var panel = new StackPanel
		{
			Background = SystemColors.ControlBrush,
		};
		panel.Children.Add(textBox);
		panel.Children.Add(new TextBlock
		{
			Text = "Enter to rename  •  Esc to cancel",
			Margin = new Thickness(4, 2, 4, 3),
			Foreground = SystemColors.GrayTextBrush,
			FontSize = Math.Max(9, textBox.FontSize - 2),
		});
		void Validate()
		{
			var valid = IsIdentifier(textBox.Text);
			textBox.BorderBrush = valid ? SystemColors.HighlightBrush : Brushes.IndianRed;
			textBox.ToolTip = valid ? null : "A rule name must be an identifier.";
		}
		textBox.TextChanged += (_, _) => Validate();

		var position = view.Caret.Position.BufferPosition;
		var line = view.TextViewLines.GetTextViewLineContainingBufferPosition(position);
		var bounds = line.GetCharacterBounds(position);
		var popup = new Popup
		{
			Child = panel,
			Placement = PlacementMode.Relative,
			PlacementTarget = view.VisualElement,
			HorizontalOffset = Math.Max(0, bounds.Left - view.ViewportLeft),
			VerticalOffset = Math.Max(0, bounds.Bottom - view.ViewportTop + 2),
			AllowsTransparency = true,
			PopupAnimation = PopupAnimation.Fade,
			StaysOpen = false,
		};

		var finished = false;
		void Finish(bool commit)
		{
			if (finished)
				return;

			finished = true;
			CommandHandlers.Remove(view);
			var replacement = textBox.Text.Trim();
			popup.IsOpen = false;
			view.VisualElement.Focus();

			if (commit && IsIdentifier(replacement))
				apply(replacement);
		}

		textBox.PreviewKeyDown += (_, args) =>
		{
			if (args.Key == Key.Enter && IsIdentifier(textBox.Text.Trim()))
			{
				args.Handled = true;
				Finish(true);
			}
			else if (args.Key == Key.Escape)
			{
				args.Handled = true;
				Finish(false);
			}
		};
		popup.Closed += (_, _) => Finish(false);
		view.Closed += (_, _) => Finish(false);
		bool HandleCommand(Guid group, uint commandId)
		{
			if (group == VSConstants.GUID_VSStandardCommandSet97 &&
				commandId == (uint)VSConstants.VSStd97CmdID.Delete)
			{
				EditingCommands.Delete.Execute(null, textBox);
				return true;
			}

			if (group != VSConstants.VSStd2K)
				return false;

			var command = (VSConstants.VSStd2KCmdID)commandId;
			RoutedUICommand? editingCommand = command switch
			{
				VSConstants.VSStd2KCmdID.LEFT          => EditingCommands.MoveLeftByCharacter,
				VSConstants.VSStd2KCmdID.LEFT_EXT      => EditingCommands.SelectLeftByCharacter,
				VSConstants.VSStd2KCmdID.RIGHT         => EditingCommands.MoveRightByCharacter,
				VSConstants.VSStd2KCmdID.RIGHT_EXT     => EditingCommands.SelectRightByCharacter,
				VSConstants.VSStd2KCmdID.BOL           => EditingCommands.MoveToLineStart,
				VSConstants.VSStd2KCmdID.BOL_EXT       => EditingCommands.SelectToLineStart,
				VSConstants.VSStd2KCmdID.EOL           => EditingCommands.MoveToLineEnd,
				VSConstants.VSStd2KCmdID.EOL_EXT       => EditingCommands.SelectToLineEnd,
				VSConstants.VSStd2KCmdID.WORDPREV      => EditingCommands.MoveLeftByWord,
				VSConstants.VSStd2KCmdID.WORDPREV_EXT  => EditingCommands.SelectLeftByWord,
				VSConstants.VSStd2KCmdID.WORDNEXT      => EditingCommands.MoveRightByWord,
				VSConstants.VSStd2KCmdID.WORDNEXT_EXT  => EditingCommands.SelectRightByWord,
				VSConstants.VSStd2KCmdID.BACKSPACE     => EditingCommands.Backspace,
				VSConstants.VSStd2KCmdID.DELETE        => EditingCommands.Delete,
				VSConstants.VSStd2KCmdID.DELETEKEY     => EditingCommands.Delete,
				_ => null,
			};

			if (editingCommand is not null)
			{
				editingCommand.Execute(null, textBox);
				return true;
			}

			if (command == VSConstants.VSStd2KCmdID.RETURN)
			{
				if (IsIdentifier(textBox.Text.Trim()))
					Finish(true);
				return true;
			}

			if (command is VSConstants.VSStd2KCmdID.CANCEL or VSConstants.VSStd2KCmdID.REVERSECANCEL)
			{
				Finish(false);
				return true;
			}

			return false;
		}

		CommandHandlers[view] = HandleCommand;
		popup.IsOpen = true;
		textBox.Focus();
		Keyboard.Focus(textBox);
		textBox.SelectAll();
	}

	static bool IsIdentifier(string text)
	{
		if (text.Length == 0 || text[0] != '_' && !char.IsLetter(text[0]))
			return false;

		for (var i = 1; i < text.Length; i++)
			if (text[i] != '_' && !char.IsLetterOrDigit(text[i]))
				return false;

		return true;
	}
}
