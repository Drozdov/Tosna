using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Search;
using Tosna.Core.Helpers.Xml;
using Tosna.Editor.Common;
using Tosna.Editor.IDE.Verification;
using Tosna.Editor.IDE.Vm;
using Tosna.Editor.Wpf.XmlEditor.TextMarkers;
using Tosna.Editor.Wpf.XmlEditor.Tooltips;

namespace Tosna.Editor.Wpf.XmlEditor
{
	public partial class XmlEditorControl
	{
		private readonly TextMarkerService textMarkerService;
		private readonly AutoCompletionService autoCompletionService;
		private readonly TooltipsService tooltipsService;

		private XmlEditorVm vm;
		
		public XmlEditorControl()
		{
			InitializeComponent();
			
			DataContextChanged += OnDataContextChanged;

			SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);
			
			textMarkerService = new TextMarkerService(Document);
			TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
			TextArea.TextView.LineTransformers.Add(textMarkerService);
			var services = (IServiceContainer)Document.ServiceProvider.GetService(typeof(IServiceContainer));
			services?.AddService(typeof(ITextMarkerService), textMarkerService);

			autoCompletionService = new AutoCompletionService(this);

			tooltipsService = new TooltipsService(this);
			var textEditingService = new TextEditingService(this);

			MouseMove += (sender, args) => tooltipsService.EnqueueMouseMoveSignal();
			TextChanged += (sender, args) =>
			{
				vm?.SetText(Text);
				textEditingService.EnqueueUpdate();
				autoCompletionService.EnqueueUpdate();
			};
			TextArea.TextEntered += TextAreaOnTextEntered;
			TextArea.Caret.PositionChanged += (sender, args) => { vm?.SetTextPosition(new TextPosition(TextArea.Caret.Line, TextArea.Caret.Column)); };

			SearchPanel.Install(TextArea);

			Document.UndoStack.PropertyChanged += UndoStackOnPropertyChanged;
		}

		private void TextAreaOnTextEntered(object o, TextCompositionEventArgs args)
		{
			autoCompletionService.MarkAsDirty();
		}

		#region Overrides

		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Right)
			{
				var point = Mouse.GetPosition(this);
				var nullablePosition = GetPositionFromPoint(point);

				if (nullablePosition == null)
				{
					return;
				}
				var position = nullablePosition.Value;
				TextArea.Caret.Location = new TextLocation(position.Line, position.Column);
			}
			base.OnMouseDown(e);
		}

		protected override void OnPreviewKeyDown(KeyEventArgs e)
		{
			if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) &&
				(e.KeyboardDevice.IsKeyDown(Key.LeftShift) || e.KeyboardDevice.IsKeyDown(Key.RightShift)) &&
				e.Key == Key.F)
			{
				var formattedText = XmlFormatter.FormatText(Text);

				if (Text != formattedText)
				{
					Document.Replace(new TextSegment{StartOffset = 0, Length = Text.Length}, formattedText);
				}
			}

			if ((e.KeyboardDevice.IsKeyDown(Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(Key.RightCtrl)) &&
				e.KeyboardDevice.IsKeyDown(Key.Space))
			{
				autoCompletionService.Refresh();
				e.Handled = true;
				return;
			}

			base.OnPreviewKeyDown(e);
		}
		
		#endregion

		#region Private

		private void UndoStackOnPropertyChanged(object o, PropertyChangedEventArgs args)
		{
			if (args.PropertyName == nameof(UndoStack.IsOriginalFile) && Document.UndoStack.IsOriginalFile)
			{
				vm?.MarkStateAsSaved();
			}
		}

		private void OnDataContextChanged(object o, DependencyPropertyChangedEventArgs args)
		{
			if (vm != null)
			{
				Subscribe(false);
			}

			vm = null;
			if (DataContext is XmlEditorVm xmlEditorVm)
			{
				Text = xmlEditorVm.Content;
				Document.UndoStack.MarkAsOriginalFile();
			}

			vm = DataContext as XmlEditorVm;
			autoCompletionService.Vm = vm;

			if (vm != null)
			{
				Subscribe(true);
			}

			UpdateVerificationNotifications();
		}

		private void Subscribe(bool subscribe)
		{
			if (subscribe)
			{
				vm.VerificationDone += VmOnVerificationDone;
				vm.ReloadRequest += VmOnReloadRequest;
				vm.DocumentSaved += VmOnDocumentSaved;
				vm.RenameImprintRequest += VmOnRenameImprintRequest;
				vm.GoToPositionRequest += VmOnGoToPositionRequest;
				vm.Disposed += VmOnDisposed;
			}
			else
			{
				vm.VerificationDone -= VmOnVerificationDone;
				vm.ReloadRequest -= VmOnReloadRequest;
				vm.DocumentSaved -= VmOnDocumentSaved;
				vm.RenameImprintRequest -= VmOnRenameImprintRequest;
				vm.GoToPositionRequest -= VmOnGoToPositionRequest;
				vm.Disposed -= VmOnDisposed;
			}
		}

		private void VmOnGoToPositionRequest(object o, ItemEventArgs<TextPosition> eventArgs)
		{
			TextArea.Caret.Location = new TextLocation(eventArgs.Item.Line, eventArgs.Item.Column);
			TextArea.Caret.BringCaretToView();
		}

		private void VmOnRenameImprintRequest(object o, RenameImprintVmEventArgs eventArgs)
		{
			var newNameWindow = new NewNameWindow { DataContext = eventArgs };
			var result = newNameWindow.ShowDialog();
			if (result.HasValue && result.Value)
			{
				eventArgs.RenameConfirmed = true;
			}
		}

		private void VmOnDisposed(object o, EventArgs eventArgs)
		{
			DataContext = null;
		}

		private void VmOnReloadRequest(object o, EventArgs eventArgs)
		{
			if (vm != null)
			{
				Document.Replace(new TextSegment { StartOffset = 0, Length = Text.Length }, vm.Content);
			}
		}

		private void VmOnDocumentSaved(object o, EventArgs eventArgs)
		{
			Document.UndoStack.MarkAsOriginalFile();
		}

		private void VmOnVerificationDone(object o, EventArgs eventArgs)
		{
			Dispatcher.Invoke(UpdateVerificationNotifications);
			autoCompletionService.EnqueueUpdate();
		}

		private void UpdateVerificationNotifications()
		{
			if (vm == null)
			{
				return;
			}

			var notifications = vm.VerificationNotifications;

			textMarkerService.RemoveAll(m => true);

			try
			{
				var notificationMetasByCoords = new Dictionary<ITextIntervalCoordinates, NotificationMeta>();

				foreach (var verificationNotification in notifications)
				{
					if (!notificationMetasByCoords.TryGetValue(verificationNotification.Coordinates, out var meta))
					{
						meta = new NotificationMeta(NotificationType.None, verificationNotification.Coordinates, new string[] { }, new ActionCommand[] { });
					}

					NotificationType newNotificationType;
					switch (verificationNotification.NotificationType)
					{
						case VerificationNotificationType.Warning:
							newNotificationType = NotificationType.Warning;
							break;

						case VerificationNotificationType.Error:
							newNotificationType = NotificationType.Error;
							break;

						default:
							newNotificationType = NotificationType.None;
							break;
					}

					newNotificationType =
						(NotificationType) Math.Max((int) newNotificationType, (int) meta.NotificationNotificationType);

					var newMessages = meta.Messages.Union(new[] {verificationNotification.Message }).ToArray();
					
					var newActions = meta.Actions;
					if (ActionsFactory.TryCreateActionCommand(verificationNotification.CompletionDataProvider, TextArea,
						out var newAction) && !newActions.Any(action =>
							string.Equals(action.PublicName, newAction.PublicName)))
					{
						newActions = newActions.Union(new[] {newAction}).ToArray();
					}

					var newMeta = new NotificationMeta(newNotificationType, verificationNotification.Coordinates,
						newMessages, newActions);

					notificationMetasByCoords[verificationNotification.Coordinates] = newMeta;
				}

				foreach (var notification in notificationMetasByCoords.Values)
				{
					var marker = TextMarkerFactory.CreateMarker(notification.Coordinates, textMarkerService, Document);
					marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
					marker.MarkerColor = notification.NotificationNotificationType == NotificationType.Error
						? Colors.Red
						: Colors.Orange;
				}

				tooltipsService.UpdateTooltips(notificationMetasByCoords.Values.ToArray());
			}
			catch (Exception e)
			{
				Text = string.Join(Environment.NewLine, "Inner error:", e.Message, e.StackTrace);
			}
		}

		#endregion
	}
}
