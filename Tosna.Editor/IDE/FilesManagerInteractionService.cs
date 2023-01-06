using System;
using System.Collections.Generic;
using System.Linq;
using Tosna.Core.Documents;
using Tosna.Core.Imprints;

namespace Tosna.Editor.IDE
{
	public class FilesManagerInteractionService
	{
		private readonly FilesManager filesManager;

		public FilesManagerInteractionService(FilesManager filesManager)
		{
			this.filesManager = filesManager;
		}

		public IEnumerable<Imprint> FindImprintsByType(Type type)
		{
			var singleFileManagers = filesManager.FileManagers;
			return singleFileManagers.SelectMany(fileManager => fileManager.Imprints.GetNestedImprintsRecursively())
				.Where(imprint => type.IsAssignableFrom(imprint.Type));
		}

		public Imprint FindImprint(ReferenceImprint referenceImprint, out SingleFileManager fileManager)
		{
			var refFilePath = referenceImprint.AbsoluteReferencePath;
			var refId = referenceImprint.ReferenceId;
			try
			{
				fileManager = filesManager.FileManagers.Single(fm => fm.FileName == refFilePath);
			}
			catch (Exception e)
			{
				throw new Exception($"Could not locate file {refFilePath}", e);
			}

			try
			{
				return fileManager.Imprints.GetNestedImprintsRecursively().Single(stamp => stamp.TryGetId(out var id) && id == refId);
			}
			catch (Exception e)
			{
				throw new Exception($"Could not find id={refId} in file {refFilePath}", e);
			}
		}

		public IEnumerable<SingleFileManagerRefactoring> Refactor(Imprint imprint, string newId, string newFilePath, bool moveToTop)
		{
			foreach (var fileManager in filesManager.FileManagers)
			{
				if (!RefactorImprintProcessor.UpdateImprints(fileManager.Imprints, fileManager.FileName, imprint,
					newId, newFilePath, moveToTop, out var refactoredImprints))
				{
					continue;
				}

				var document = filesManager.Serializer.SaveRootImprints(refactoredImprints);
				yield return new SingleFileManagerRefactoring(fileManager, document);
			}
		}

		public void VerifyDependencies()
		{
			filesManager.VerifyDependencies();
		}
	}

	public class SingleFileManagerRefactoring
	{
		public SingleFileManager FileManager { get; }

		public Document NewDocument { get; }

		public SingleFileManagerRefactoring(SingleFileManager fileManager, Document newDocument)
		{
			FileManager = fileManager;
			NewDocument = newDocument;
		}
	}
}
