using System;
using System.Windows;
using Microsoft.Win32;
using Tosna.Core.Common;
using Tosna.Editor.IDE;
using Tosna.Editor.IDE.Interfaces;
using Tosna.Editor.IDE.Vm;
using Tosna.Editor.Wpf.Demo.Domain;

namespace Tosna.Editor.Wpf.Demo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		private readonly IFilesSelector filesSelector = new FilesSelector();
		
		public MainWindow()
		{
			InitializeComponent();

			var filesManager = new FilesManager();
			DataContext = new XmlIdeVm(filesManager, filesSelector, new ConfirmationRequester(), new Logger());
		}

		private void MenuItemOnClick(object sender, RoutedEventArgs e)
		{
			if (filesSelector.CreateFile(null, out var fileName))
			{
				EnvironmentFactory.SaveEnvironment(fileName);
			}
		}

		#region Nested

		private class FilesSelector : IFilesSelector
		{
			public bool CreateFile(string initialDirectory, out string fileName)
			{
				var dialog = new SaveFileDialog { InitialDirectory = initialDirectory, FileName = "NewFile.xml" };
				if (dialog.ShowDialog() == true)
				{
					fileName = dialog.FileName;
					return true;
				}

				fileName = null;
				return false;
			}

			public bool SelectFiles(string initialDirectory, out string[] files)
			{
				var dialog = new OpenFileDialog{Multiselect = true, InitialDirectory = initialDirectory};
				if (dialog.ShowDialog() == true)
				{
					files = dialog.FileNames;
					return true;
				}

				files = null;
				return false;
			}
		}

		private class ConfirmationRequester : IConfirmationRequester
		{
			public ConfirmationAnswer ConfirmOperation(string question)
			{
				var resultMessageBox = MessageBox.Show(question, "Tosna", MessageBoxButton.YesNoCancel,
					MessageBoxImage.None);

				switch (resultMessageBox)
				{
					case MessageBoxResult.Yes:
						return ConfirmationAnswer.Yes;

					case MessageBoxResult.No:
						return ConfirmationAnswer.No;

					case MessageBoxResult.Cancel:
						return ConfirmationAnswer.Cancel;

					default:
						return ConfirmationAnswer.Cancel;
				}
			}
		}

		private class Logger : IInfoLogger
		{
			public void LogMessage(LogMessageType messageType, string message)
			{
				Console.WriteLine($"[{messageType.GetMessageTypeStr()}] {message}");
			}

			public void LogMessage(LogMessageType messageType, string message, Exception e)
			{
				LogMessage(messageType, message);
				LogMessage(LogMessageType.Debug, e.Message);
				if (e.StackTrace != null)
				{
					LogMessage(LogMessageType.Debug, e.StackTrace);
				}
			}
		}

		#endregion
	}
}