using System;
using Tosna.Core;
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
			var verificationService = new VerificationService(filesManager, logger);
			ViewerVm = new FilesViewerVm(verificationService, confirmationRequester, new FilesManagerInteractionService(filesManager), logger);
			HierarchyVm = new FilesHierarchyVm(filesManager, ViewerVm, filesSelector, verificationService, logger);
		}

		public void Dispose()
		{
			ViewerVm.Dispose();
			HierarchyVm.Dispose();
		}
	}
}
