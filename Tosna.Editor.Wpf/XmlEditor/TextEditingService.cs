using System;
using System.Windows.Threading;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Folding;

namespace Tosna.Editor.Wpf.XmlEditor
{
	public class TextEditingService
	{
		private readonly TextEditor textEditor;

		//private readonly TooltipsService tooltipsService;
		
		private readonly DispatcherTimer refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };

		private readonly FoldingManager foldingManager;
		private readonly XmlFoldingStrategy foldingStrategy = new XmlFoldingStrategy();

		private bool isRunning;
		private bool updateNeeded;

		public TextEditingService(TextEditor textEditor/*, TooltipsService tooltipsService*/)
		{
			this.textEditor = textEditor;
			//this.tooltipsService = tooltipsService;

			foldingManager = FoldingManager.Install(textEditor.TextArea);

			refreshTimer.Tick += delegate {
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

			};
		}

		public void EnqueueUpdate()
		{
			updateNeeded = true;
			if (isRunning)
			{
				return;
			}
			isRunning = true;
			refreshTimer.Start();
		}

		public void Refresh()
		{
			if (textEditor.Document.Text.Length == 0)
			{
				return;
			}

			foldingStrategy.UpdateFoldings(foldingManager, textEditor.Document);

			//tooltipsService.Reset();
		}
	}
}
