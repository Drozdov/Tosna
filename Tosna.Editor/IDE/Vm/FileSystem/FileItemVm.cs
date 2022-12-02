using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tosna.Editor.Common;
using Tosna.Editor.IDE.Verification;

namespace Tosna.Editor.IDE.Vm.FileSystem
{
	public class FileItemVm : NotifyPropertyChanged, IFileSystemItemVm
	{
		#region Fields

		private IReadOnlyCollection<DescriptorFileManager> descriptedFileManagers = new DescriptorFileManager[] { };

		#endregion

		#region Properties

		public string FullPath => FileManager.FileName;

		public SingleFileManager FileManager { get; }

		public IReadOnlyCollection<DescribedItemVm> DescriptedChildren { get; private set; }

		public ImageType ImageType
		{
			get
			{
				if (FileManager.State == SingleFileManagerState.FileInvalid)
				{
					return ImageType.TextFileMissing;
				}

				var notificationsTypes = FileManager.AllVerificationNotifications
					.Select(notification => notification.NotificationType).Distinct().ToArray();

				if (notificationsTypes.Contains(VerificationNotificationType.Error))
				{
					return ImageType.TextFileError;
				}

				if (notificationsTypes.Contains(VerificationNotificationType.Warning))
				{
					return ImageType.TextFileWarning;
				}

				return ImageType.TextFile;
			}
		}

		public string Name => Path.GetFileName(FileManager.FileName);

		public string Tooltip => FullPath;

		#endregion

		#region Ctor

		public FileItemVm(SingleFileManager fileManager)
		{
			FileManager = fileManager;

			Subscribe(true);

			UpdateChildren();
		}

		#endregion

		#region Methods

		public int CompareTo(IFileSystemItemVm other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;
			if (other is FileItemVm otherFileItemVm)
			{
				return string.Compare(Name, otherFileItemVm.Name, StringComparison.Ordinal);
			}
			if (other is DirectoryItemVm)
			{
				return 1;
			}
			return -1;
		}
		
		public void Dispose()
		{
			Subscribe(false);
		}

		#endregion

		#region Private

		private void Subscribe(bool subscribe)
		{
			if (subscribe)
			{
				FileManager.VerificationFinished += FileManagerOnVerificationFinished;
				FileManager.StateChanged += FileManagerOnStateChanged;
			}
			else
			{
				FileManager.VerificationFinished -= FileManagerOnVerificationFinished;
				FileManager.StateChanged -= FileManagerOnStateChanged;
			}
		}

		private void FileManagerOnStateChanged(object sender, EventArgs eventArgs)
		{
			RaisePropertyChanged(nameof(ImageType));
			UpdateChildren();
		}

		private void FileManagerOnVerificationFinished(object sender, EventArgs eventArgs)
		{
			RaisePropertyChanged(nameof(ImageType));
			UpdateChildren();
		}

		private void UpdateChildren()
		{
			if (descriptedFileManagers.SequenceEqual(FileManager.DescriptorFileManagers))
			{
				return;
			}

			descriptedFileManagers = FileManager.DescriptorFileManagers;
			DescriptedChildren = descriptedFileManagers.Select(manager => new DescribedItemVm(manager)).ToArray();
			RaisePropertyChanged(nameof(DescriptedChildren));
		}

		#endregion
	}
}