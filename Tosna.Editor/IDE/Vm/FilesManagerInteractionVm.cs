using System;
using System.Collections.Generic;
using System.Linq;
using Tosna.Core.Imprints;
using Tosna.Editor.IDE.Verification;

namespace Tosna.Editor.IDE.Vm
{
	public class FilesManagerInteractionVm
	{
		private readonly FilesManagerInteractionService filesManagerInteractionService;
		private readonly FilesViewerVm filesViewerVm;

		private readonly VerificationService verificationService;

		public string FileName { get; }

		public FilesManagerInteractionVm(FilesManagerInteractionService filesManagerInteractionService, string fileName,
			FilesViewerVm filesViewerVm, VerificationService verificationService)
		{
			this.filesManagerInteractionService = filesManagerInteractionService;
			this.filesViewerVm = filesViewerVm;
			this.verificationService = verificationService;
			FileName = fileName;
		}

		public IEnumerable<Imprint> FindImprintsByType(Type type)
		{
			return filesManagerInteractionService.FindImprintsByType(type);
		}

		public void MoveImprint(ImprintInfoVm imprintVm, string newId, string newFilePath)
		{
			var refactorings = filesManagerInteractionService.Refactor(imprintVm.Imprint, newId, newFilePath, true);
			Apply(refactorings.ToArray());
		}

		public void RenameImprint(ImprintInfoVm imprintVm, string newId)
		{
			var refactorings = filesManagerInteractionService.Refactor(imprintVm.Imprint, newId, imprintVm.Imprint.FilePath, false);
			Apply(refactorings.ToArray());
		}

		public XmlEditorVm TryOpenReferencedImprint(ReferenceImprint referenceImprint, out Imprint imprint)
		{
			imprint = filesManagerInteractionService.FindImprint(referenceImprint, out var fileManager);
			return filesViewerVm.OpenDocument(fileManager);
		}

		private void Apply(IReadOnlyCollection<SingleFileManagerRefactoring> refactorings)
		{
			foreach (var refactoring in refactorings)
			{
				filesViewerVm.OpenDocument(refactoring.FileManager, false);
				refactoring.FileManager.Edit(refactoring.NewDocument);
			}

			verificationService.EnqueueFileChangesVerification(refactorings.Select(refactoring =>
				refactoring.FileManager));
		}
	}
}
