using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using Tosna.Core.Common;
using Tosna.Editor.Common;
using Tosna.Editor.IDE.Interfaces;
using Tosna.Editor.IDE.Verification;
using Tosna.Editor.IDE.Vm.PropertyEditors;

namespace Tosna.Editor.IDE.Vm
{
	public class FilesViewerVm : IDisposable
	{
		private readonly VerificationService verificationService;
		private readonly IConfirmationRequester confirmationRequester;
		private readonly FilesManagerInteractionService filesManagerInteractionService;
		private readonly ILogger logger;
		private ViewerItemVm currentFileViewerItemVm;

		public FilesViewerVm(VerificationService verificationService, IConfirmationRequester confirmationRequester, FilesManagerInteractionService filesManagerInteractionService, ILogger logger)
		{
			Contract.Requires(verificationService != null);
			Contract.Requires(confirmationRequester != null);

			this.verificationService = verificationService;
			this.confirmationRequester = confirmationRequester;
			this.filesManagerInteractionService = filesManagerInteractionService;
			this.logger = logger;
		}

		public ObservableCollection<ViewerItemVm> OpenDocuments { get; } = new ObservableCollection<ViewerItemVm>();

		public XmlEditorVm OpenDocument(SingleFileManager fileManager, bool autoSelect = true)
		{
			if (currentFileViewerItemVm != null && autoSelect)
			{
				currentFileViewerItemVm.IsSelected = false;
			}

			var fileViewerItemVm = OpenDocuments.OfType<FileViewerItemVm>().FirstOrDefault(document => document.FileManager == fileManager);
			if (fileViewerItemVm == null)
			{
				OpenDocuments.Add(fileViewerItemVm = new FileViewerItemVm(fileManager, this, verificationService, filesManagerInteractionService, logger));
			}

			currentFileViewerItemVm = fileViewerItemVm;
			if (autoSelect)
			{
				currentFileViewerItemVm.IsSelected = true;
			}
			return fileViewerItemVm.XmlEditorVm;
		}

		public DescriptedImprintEditorVm OpenDocument(DescriptorFileManager fileManager, bool autoSelect = true)
		{
			if (currentFileViewerItemVm != null && autoSelect)
			{
				currentFileViewerItemVm.IsSelected = false;
			}

			var fileViewerItemVm = OpenDocuments.OfType<DescriptedViewerItemVm>().FirstOrDefault(document => document.DescriptorFileManager == fileManager);
			if (fileViewerItemVm == null)
			{
				OpenDocuments.Add(fileViewerItemVm = new DescriptedViewerItemVm(fileManager, this, filesManagerInteractionService, logger));
			}

			currentFileViewerItemVm = fileViewerItemVm;
			if (autoSelect)
			{
				currentFileViewerItemVm.IsSelected = true;
			}
			return fileViewerItemVm.EditorVm;
		}

		public void CloseDocument(DescriptorFileManager fileManager)
		{
			if (fileManager.SingleFileManager.State == SingleFileManagerState.FileWithUnsavedChanges)
			{
				var confirmationAnswer =
					confirmationRequester.ConfirmOperation($"Save changes to {fileManager.FileName}?");

				switch (confirmationAnswer)
				{
					case ConfirmationAnswer.Yes:
						fileManager.SingleFileManager.SaveToDisk();
						break;

					case ConfirmationAnswer.No:
						fileManager.SingleFileManager.ReloadFromDisk();
						break;

					case ConfirmationAnswer.Cancel:
						return;

					default:
						return;
				}
			}

			var fileViewerItemVm = OpenDocuments.OfType<DescriptedViewerItemVm>().FirstOrDefault(document => document.DescriptorFileManager == fileManager);
			if (fileViewerItemVm == null)
			{
				return;
			}

			OpenDocuments.Remove(fileViewerItemVm);
			fileViewerItemVm.Dispose();
		}

		public void CloseDocument(SingleFileManager fileManager)
		{
			if (fileManager.State == SingleFileManagerState.FileWithUnsavedChanges)
			{
				var confirmationAnswer =
					confirmationRequester.ConfirmOperation($"Save changes to {fileManager.FileName}?");

				switch (confirmationAnswer)
				{
					case ConfirmationAnswer.Yes:
						fileManager.SaveToDisk();
						break;

					case ConfirmationAnswer.No:
						fileManager.ReloadFromDisk();
						break;

					case ConfirmationAnswer.Cancel:
						return;

					default:
						return;
				}
			}

			var fileViewerItemVm = OpenDocuments.FirstOrDefault(document => document.FileManager == fileManager);
			if (fileViewerItemVm == null)
			{
				return;
			}

			OpenDocuments.Remove(fileViewerItemVm);
			fileViewerItemVm.Dispose();
		}

		public void Dispose()
		{
			foreach (var fileViewerItemVm in OpenDocuments)
			{
				fileViewerItemVm.Dispose();
			}
		}
	}

	public abstract class ViewerItemVm : NotifyPropertyChanged, IDisposable
	{
		private bool isSelected;
		public SingleFileManager FileManager { get; }

		protected ViewerItemVm(SingleFileManager fileManager, FilesViewerVm parent)
		{
			FileManager = fileManager;

			CloseCommand = new ActionCommand(() => OnClosed(parent), () => true);

			Subscribe(true);
		}

		public string Title => GetTitle() + (IsFileSaved ? "" : "*");

		public string Tooltip => FileManager.FileName;

		public bool IsFileSaved => FileManager.State == SingleFileManagerState.FileInSavedState;

		public bool IsSelected
		{
			get => isSelected;
			set
			{
				isSelected = value;
				RaisePropertyChanged(nameof(IsSelected));
			}
		}

		public ActionCommand CloseCommand { get; }

		public void Dispose()
		{
			Subscribe(false);
		}

		private void Subscribe(bool subscribe)
		{
			if (subscribe)
			{
				FileManager.StateChanged += FileManagerOnStateChanged;
			}
			else
			{
				FileManager.StateChanged -= FileManagerOnStateChanged;
			}
		}

		private void FileManagerOnStateChanged(object sender, EventArgs eventArgs)
		{
			RaisePropertyChanged(nameof(IsFileSaved));
			RaisePropertyChanged(nameof(Title));
		}

		protected abstract void OnDisposed();

		protected abstract void OnClosed(FilesViewerVm parent);

		protected abstract string GetTitle();
	}

	public class FileViewerItemVm : ViewerItemVm
	{
		public FileViewerItemVm(SingleFileManager fileManager, FilesViewerVm parent, VerificationService verificationService, FilesManagerInteractionService filesManagerInteractionService, ILogger logger) : base(fileManager, parent)
		{
			XmlEditorVm = new XmlEditorVm(fileManager, verificationService, filesManagerInteractionService, parent, logger);
		}

		public XmlEditorVm XmlEditorVm { get; }

		protected override void OnDisposed()
		{
			XmlEditorVm.Dispose();
		}

		protected override void OnClosed(FilesViewerVm parent)
		{
			parent.CloseDocument(FileManager);
		}

		protected override string GetTitle()
		{
			return Path.GetFileName(FileManager.FileName);
		}
	}

	public class DescriptedViewerItemVm : ViewerItemVm
	{
		public DescriptorFileManager DescriptorFileManager { get; }

		public DescriptedViewerItemVm(DescriptorFileManager fileManager, FilesViewerVm parent, FilesManagerInteractionService filesManagerInteractionService, ILogger logger) : base(fileManager.SingleFileManager, parent)
		{
			DescriptorFileManager = fileManager;
			EditorVm = new DescriptedImprintEditorVm(fileManager, filesManagerInteractionService, logger);
		}
		
		public DescriptedImprintEditorVm EditorVm { get; }

		protected override void OnDisposed()
		{
			EditorVm.Dispose();
		}

		protected override void OnClosed(FilesViewerVm parent)
		{
			parent.CloseDocument(DescriptorFileManager);
		}

		protected override string GetTitle()
		{
			return Path.GetFileName(DescriptorFileManager.PublicName);
		}
	}
}
