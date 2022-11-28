using System;
using System.Collections.Generic;
using Tosna.Core.Common;
using Tosna.Core.Common.Imprints;
using Tosna.Editor.Common;
using Tosna.Editor.IDE.Verification;

namespace Tosna.Editor.IDE.Vm
{
	public class XmlEditorVm : NotifyPropertyChanged, IDisposable
	{
		#region Fields

		private readonly SingleFileManager singleFileManager;
		private readonly VerificationService verificationService;

		private string content;
		private SingleFileManagerState state;
		private ImprintInfoVm selectedImprintInfoVm;

		#endregion

		#region Ctor & Dispose

		public XmlEditorVm(SingleFileManager singleFileManager, VerificationService verificationService, FilesManagerInteractionService filesManagerInteractionService, FilesViewerVm filesViewerVm, IInfoLogger logger)
		{
			this.singleFileManager = singleFileManager;
			this.verificationService = verificationService;

			SaveCommand = new ActionCommand(singleFileManager.SaveToDisk, () => true, logger);
			ReloadCommand = new ActionCommand(singleFileManager.ReloadFromDisk, () => true, logger);
			RefactorMoveToTopCommand = new ActionCommand(RefactorMoveToTop, CanRefactorMoveToTop, logger);
			RefactorRenameCommand = new ActionCommand(RefactorRename, CanRefactorRename, logger);
			RefactorMoveToFileCommand = new ActionCommand(RefactorMoveToFile, CanRefactorMoveToFile, logger);
			RefactorInlineCommand = new ActionCommand(RefactorInline, CanRefactorInline, logger);
			GoToDefinitionCommand = new ActionCommand(GoToDefinition, CanGoToDefinition, logger);

			State = singleFileManager.State;
			Content = singleFileManager.Content;

			FilesManagerInteractionVm = new FilesManagerInteractionVm(filesManagerInteractionService, singleFileManager.FileName, filesViewerVm);

			Subscribe(true);
		}

		public void Dispose()
		{
			Subscribe(false);
			Disposed(this, EventArgs.Empty);
		}

		#endregion

		#region Properties

		public string Content
		{
			get => content;
			private set
			{
				if (content == value)
				{
					return;
				}

				content = value;
				ReloadRequest(this, EventArgs.Empty);
			}
		}

		public SingleFileManagerState State
		{
			get => state;
			private set
			{
				if (state == value)
				{
					return;
				}
				state = value;

				switch (state)
				{
					case SingleFileManagerState.FileInSavedState:
						DocumentSaved(this, EventArgs.Empty);
						break;
				}
			}
		}

		public IReadOnlyCollection<VerificationNotification> VerificationNotifications => singleFileManager.AllErrors;

		public FilesManagerInteractionVm FilesManagerInteractionVm { get; }

		public ImprintInfoVm SelectedImprintInfoVm
		{
			get => selectedImprintInfoVm;
			private set
			{
				if (Equals(selectedImprintInfoVm, value))
				{
					return;
				}

				selectedImprintInfoVm = value;
				RaisePropertyChanged(nameof(SelectedImprintInfoVm));
				RaisePropertyChanged(nameof(RefactorCommandsVisible));
			}
		}

		public bool RefactorCommandsVisible => SelectedImprintInfoVm != null;
		
		public ActionCommand SaveCommand { get; }

		public ActionCommand ReloadCommand { get; }

		public ActionCommand RefactorMoveToTopCommand { get; }

		public ActionCommand RefactorRenameCommand { get; }

		public ActionCommand RefactorMoveToFileCommand { get; }

		public ActionCommand RefactorInlineCommand { get; }

		public ActionCommand GoToDefinitionCommand { get; }

		#endregion

		#region Methods

		public void MarkStateAsSaved()
		{
			singleFileManager.MarkStateAsSaved();
		}

		public void SetText(string newText)
		{
			content = newText;
			singleFileManager.Edit(newText);
			verificationService.EnqueueUpdate(singleFileManager);
		}

		public void SetTextPosition(TextPosition textPosition)
		{
			if (TryGetImprintInfoVm(textPosition.Line, textPosition.Column, out var imprintInfoVm))
			{
				SelectedImprintInfoVm = imprintInfoVm;
			}
		}

		public bool TryGetImprintInfoVm(int line, int column, out ImprintInfoVm imprintInfoVm)
		{
			if (singleFileManager.TryGetImprint(line, column, out var imprint, out var isRootImprint))
			{
				imprintInfoVm = new ImprintInfoVm(imprint, isRootImprint);
				return true;
			}

			imprintInfoVm = default(ImprintInfoVm);
			return false;
		}

		#endregion

		#region Events

		public event EventHandler VerificationDone = delegate { };

		public event EventHandler DocumentSaved = delegate { };

		public event EventHandler ReloadRequest = delegate { };

		public event EventHandler<ItemEventArgs<TextPosition>> GoToPositionRequest = delegate { };

		public event EventHandler<RenameImprintVmEventArgs> RenameImprintRequest = delegate { };

		public event EventHandler Disposed = delegate { };

		#endregion

		#region Private & Protected

		private void Subscribe(bool subscribe)
		{
			if (subscribe)
			{
				singleFileManager.ContentUpdated += SingleFileManagerOnContentUpdated;
				singleFileManager.VerificationFinished += SingleFileManagerOnVerificationFinished;
				singleFileManager.StateChanged += SingleFileManagerOnStateChanged;
			}
			else
			{
				singleFileManager.ContentUpdated -= SingleFileManagerOnContentUpdated;
				singleFileManager.VerificationFinished -= SingleFileManagerOnVerificationFinished;
				singleFileManager.StateChanged -= SingleFileManagerOnStateChanged;
			}
		}

		private void SingleFileManagerOnContentUpdated(object sender, EventArgs eventArgs)
		{
			Content = singleFileManager.Content;
		}

		private void SingleFileManagerOnStateChanged(object sender, EventArgs eventArgs)
		{
			State = singleFileManager.State;
		}

		private void SingleFileManagerOnVerificationFinished(object sender, EventArgs eventArgs)
		{
			RaisePropertyChanged(nameof(VerificationNotifications));
			VerificationDone(this, EventArgs.Empty);
		}

		private void GoToPosition(TextPosition textPosition)
		{
			GoToPositionRequest(this, new ItemEventArgs<TextPosition>(textPosition));
		}

		#endregion

		#region Refactor commands implementations

		private void RefactorMoveToTop()
		{
			FilesManagerInteractionVm.MoveImprint(SelectedImprintInfoVm, SelectedImprintInfoVm.Id, FilesManagerInteractionVm.FileName);
		}

		private bool CanRefactorMoveToTop()
		{
			return SelectedImprintInfoVm != null && !SelectedImprintInfoVm.IsRootImprint;
		}

		private bool CanRefactorRename()
		{
			return SelectedImprintInfoVm != null;
		}

		private void RefactorRename()
		{
			var args = new RenameImprintVmEventArgs { ImprintId = SelectedImprintInfoVm.Id ?? SelectedImprintInfoVm.TypeName };
			RenameImprintRequest(this, args);
			if (!args.RenameConfirmed)
			{
				return;
			}

			FilesManagerInteractionVm.RenameImprint(SelectedImprintInfoVm, args.ImprintId);
		}

		private void RefactorInline()
		{
			throw new NotImplementedException();
		}

		private bool CanRefactorInline()
		{
			return SelectedImprintInfoVm != null && SelectedImprintInfoVm.IsRootImprint;
		}

		private void RefactorMoveToFile()
		{
			throw new NotImplementedException();
		}

		private bool CanRefactorMoveToFile()
		{
			return SelectedImprintInfoVm != null;
		}

		private void GoToDefinition()
		{
			var referenceImprint = (ReferenceImprint) SelectedImprintInfoVm.Imprint;
			var editorVm = FilesManagerInteractionVm.TryOpenReferencedImprint(referenceImprint, out var imprint);
			if (imprint.TryGetInfo(out var info))
			{
				editorVm.GoToPosition(new TextPosition(info.Line, info.Column));
			}
		}

		private bool CanGoToDefinition()
		{
			return SelectedImprintInfoVm?.Imprint is ReferenceImprint;
		}

		#endregion
	}

	public class RenameImprintVmEventArgs : EventArgs
	{
		public string ImprintId { get; set; }

		public bool RenameConfirmed { get; set; } = false;
	}
}
