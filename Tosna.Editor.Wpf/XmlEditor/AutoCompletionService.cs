using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Tosna.Editor.IDE.Verification;
using Tosna.Editor.IDE.Vm;
using Tosna.Editor.Wpf.XmlEditor.AutoCompletion;

namespace Tosna.Editor.Wpf.XmlEditor
{
	public class AutoCompletionService
	{
		private readonly TextEditor textEditor;
		private readonly DispatcherTimer refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
		private readonly object sync = new object();

		private bool isRunning;
		private bool updateNeeded;
		private bool isDirty;

		private DateTime lastUpdateTime = DateTime.MinValue;

		public AutoCompletionService(TextEditor textEditor)
		{
			this.textEditor = textEditor;

			refreshTimer.Tick += delegate {
				lock (sync)
				{
					if (updateNeeded)
					{
						updateNeeded = false;
						Refresh();
					}
					else
					{
						refreshTimer.Stop();
						isRunning = false;
					}
				}
			};
		}

		public XmlEditorVm Vm { get; set; }

		public void MarkAsDirty()
		{
			lock (sync)
			{
				isDirty = true;
			}
		}

		public void EnqueueUpdate()
		{
			lock (sync)
			{
				if (!isDirty)
				{
					return;
				}
				isDirty = false;

				updateNeeded = true;
				if (isRunning)
				{
					return;
				}
				isRunning = true;
				refreshTimer.Start();
			}
		}

		public void Refresh()
		{
			if (Vm == null)
			{
				return;
			}

			var currentTaskTime = DateTime.Now;

			var fixes = new List<ICompletionDataProvider>();

			var caretLocation = textEditor.Document.GetLocation(textEditor.CaretOffset);

			foreach (var vmVerificationNotification in Vm.VerificationNotifications)
			{
				if (TextIntervalCoordinatesChecker.Contains(vmVerificationNotification.Coordinates, caretLocation))
				{
					fixes.Add(vmVerificationNotification.CompletionDataProvider);
				}
			}

			var completionDataList = new List<ICompletionData>();
			foreach (var fix in fixes)
			{
				var competionDatas = CompletionDataFactory.CreateCompletionDatas(fix, Vm.FilesManagerInteractionVm);
				completionDataList.AddRange(competionDatas);
			}

			if (!completionDataList.Any())
			{
				return;
			}

			textEditor.Dispatcher.Invoke(delegate
			{
				if (lastUpdateTime > currentTaskTime)
				{
					return;
				}
				lastUpdateTime = currentTaskTime;

				var completionWindow = new CompletionWindow(textEditor.TextArea) { MinWidth = 500 };
				completionWindow.CompletionList.IsFiltering = false;

				foreach (var completionData in completionDataList)
				{
					completionWindow.CompletionList.CompletionData.Add(completionData);
				}

				completionWindow.Show();
				completionWindow.Closed += delegate { completionWindow = null; };
			});
		}
	}
}
