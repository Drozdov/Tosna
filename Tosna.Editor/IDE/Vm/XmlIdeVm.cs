using System;
using Tosna.Core.Common;
using Tosna.Editor.IDE.Interfaces;
using Tosna.Editor.IDE.Verification;

namespace Tosna.Editor.IDE.Vm
{
	public class XmlIdeVm : IDisposable
	{
		public FilesHierarchyVm HierarchyVm { get; }

		public FilesViewerVm ViewerVm { get; }

		public XmlIdeVm(FilesManager filesManager, IFilesSelector filesSelector, IConfirmationRequester confirmationRequester, ILogger logger)
		{
			ViewerVm = new FilesViewerVm(new VerificationService(filesManager, logger), confirmationRequester, new FilesManagerInteractionService(filesManager), logger);
			HierarchyVm = new FilesHierarchyVm(filesManager, ViewerVm, filesSelector, logger);
		}

		public void Dispose()
		{
			ViewerVm.Dispose();
			HierarchyVm.Dispose();
		}
	}
}
