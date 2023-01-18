using System;
using System.Collections.Generic;
using Tosna.Core.Imprints;

namespace Tosna.Editor.IDE.Vm
{
	public class FilesManagerInteractionVm
	{
		private readonly FilesManagerInteractionService filesManagerInteractionService;
		private readonly FilesViewerVm filesViewerVm;

		public string FileName { get; }

		public FilesManagerInteractionVm(FilesManagerInteractionService filesManagerInteractionService, string fileName, FilesViewerVm filesViewerVm)
		{
			this.filesManagerInteractionService = filesManagerInteractionService;
			this.filesViewerVm = filesViewerVm;
			FileName = fileName;
		}

		public IEnumerable<Imprint> FindImprintsByType(Type type)
		{
			return filesManagerInteractionService.FindImprintsByType(type);
		}

		public void MoveImprint(ImprintInfoVm imprintVm, string newId, string newFilePath)
		{
			var refactorings = filesManagerInteractionService.Refactor(imprintVm.Imprint, newId, newFilePath, true);
			Apply(refactorings);
		}

		public void RenameImprint(ImprintInfoVm imprintVm, string newId)
		{
			var refactorings = filesManagerInteractionService.Refactor(imprintVm.Imprint, newId, imprintVm.Imprint.FilePath, false);
			Apply(refactorings);
		}

		public XmlEditorVm TryOpenReferencedImprint(ReferenceImprint referenceImprint, out Imprint imprint)
		{
			imprint = filesManagerInteractionService.FindImprint(referenceImprint, out var fileManager);
			return filesViewerVm.OpenDocument(fileManager);
		}

		private void Apply(IEnumerable<SingleFileManagerRefactoring> refactorings)
		{
			foreach (var refactoring in refactorings)
			{
				filesViewerVm.OpenDocument(refactoring.FileManager, false);
				refactoring.FileManager.Edit(refactoring.NewDocument);
				refactoring.FileManager.Verify();
			}

			filesManagerInteractionService.VerifyDependencies();
		}
	}
}
