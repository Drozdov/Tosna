using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Tosna.Core.Helpers;
using Tosna.Editor.Common;
using Tosna.Editor.IDE.Verification;
using Tosna.Editor.Properties;

namespace Tosna.Editor.IDE.Vm
{
	public interface IFileSystemItemVm : IComparable<IFileSystemItemVm>, IDisposable
	{
	}
	
	public class DirectoryItemVm : IFileSystemItemVm
	{
		#region Properties

		public string DirectoryPath { get; }

		public string DirectoryShortName => Path.GetFileName(DirectoryPath.TrimEnd('/', '\\'));

		public byte[] Image => Resources.Folder;

		public ObservableCollection<IFileSystemItemVm> Items { get; } = new OrderedObservableCollection<IFileSystemItemVm>();

		#endregion

		#region Ctor

		public DirectoryItemVm(string directoryPath)
		{
			DirectoryPath = directoryPath;
		}

		#endregion

		#region Methods

		public void Add(SingleFileManager item)
		{
			var relativeDirPathItems = PathUtils
				.GetRelativePath(new DirectoryInfo(Path.GetDirectoryName(item.FileName) ?? item.FileName), new DirectoryInfo(DirectoryPath))
				.Split('/', '\\').Where(s => !string.IsNullOrWhiteSpace(s));

			var current = this;
			foreach (var relativeDirPathItem in relativeDirPathItems)
			{
				var child = current.Items.OfType<DirectoryItemVm>().FirstOrDefault(dir => string.Equals(dir.DirectoryShortName, relativeDirPathItem)) ??
							new DirectoryItemVm(Path.Combine(current.DirectoryPath, relativeDirPathItem));

				if (!current.Items.Contains(child))
				{
					current.Items.Add(child);
				}
				current = child;
			}

			if (current.Items.OfType<FileItemVm>().All(file => file.FileManager != item))
			{
				current.Items.Add(new FileItemVm(item));
			}
		}

		public void Remove(SingleFileManager item)
		{
			var relativeDirPathItems = PathUtils
				.GetRelativePath(new DirectoryInfo(Path.GetDirectoryName(item.FileName) ?? item.FileName),
					new DirectoryInfo(DirectoryPath)).Split('/', '\\');

			var current = this;
			foreach (var relativeDirPathItem in relativeDirPathItems)
			{
				current = current.Items.OfType<DirectoryItemVm>().FirstOrDefault(dir => string.Equals(dir.DirectoryShortName, relativeDirPathItem));
				if (current == null)
				{
					return;
				}
			}

			var itemToDelete = current.Items.OfType<FileItemVm>().FirstOrDefault(file => file.FileManager == item);

			if (itemToDelete == null)
			{
				return;
			}

			current.Items.Remove(itemToDelete);
			itemToDelete.Dispose();
		}

		public int CompareTo(IFileSystemItemVm other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;
			if (other is DirectoryItemVm otherDirectoryItemVm)
			{
				return string.Compare(DirectoryShortName, otherDirectoryItemVm.DirectoryShortName, StringComparison.Ordinal);
			}
			return -1;
		}

		public void Dispose()
		{
			foreach (var itemVm in Items)
			{
				itemVm.Dispose();
			}
		}

		#endregion
	}

	public class FileItemVm : NotifyPropertyChanged, IFileSystemItemVm
	{
		#region Fields

		private IReadOnlyCollection<DescriptorFileManager> descriptedFileManagers = new DescriptorFileManager[] { };

		#endregion

		#region Properties

		public string Name => Path.GetFileName(FileManager.FileName);

		public string FullPath => FileManager.FileName;

		public SingleFileManager FileManager { get; }

		public byte[] Image
		{
			get
			{
				if (FileManager.State == SingleFileManagerState.FileInvalid)
				{
					return Resources.TextFileMissing;
				}

				var notificationsTypes = FileManager.AllVerificationNotifications
					.Select(notification => notification.NotificationType).Distinct().ToArray();

				if (notificationsTypes.Contains(VerificationNotificationType.Error))
				{
					return Resources.TextFileError;
				}

				if (notificationsTypes.Contains(VerificationNotificationType.Warning))
				{
					return Resources.TextFileWarning;
				}

				return Resources.TextFile;
			}
		}

		public IReadOnlyCollection<DescriptedItemVm> DescriptedChildren { get; private set; }

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
			RaisePropertyChanged(nameof(Image));
			UpdateChildren();
		}

		private void FileManagerOnVerificationFinished(object sender, EventArgs eventArgs)
		{
			RaisePropertyChanged(nameof(Image));
			UpdateChildren();
		}

		private void UpdateChildren()
		{
			if (descriptedFileManagers.SequenceEqual(FileManager.DescriptorFileManagers))
			{
				return;
			}

			descriptedFileManagers = FileManager.DescriptorFileManagers;
			DescriptedChildren = descriptedFileManagers.Select(manager => new DescriptedItemVm(manager)).ToArray();
			RaisePropertyChanged(nameof(DescriptedChildren));
		}

		#endregion
	}

	public class DescriptedItemVm : IFileSystemItemVm
	{
		public DescriptorFileManager DescriptorFileManager { get; }

		public DescriptedItemVm(DescriptorFileManager descriptorFileManager)
		{
			DescriptorFileManager = descriptorFileManager;
		}

		public int CompareTo(IFileSystemItemVm other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;
			if (other is DescriptedItemVm otherFileItemVm)
			{
				return string.Compare(Name, otherFileItemVm.Name, StringComparison.Ordinal);
			}
			return 1;
		}

		public string Name => DescriptorFileManager.PublicName;

		public string FileName => DescriptorFileManager.FileName;

		public byte[] Image => Resources.DescriptedItem;

		public void Dispose()
		{
		}
	}

	public static class FileSystemItemVmExtensions
	{
		public static bool TryGetDirectory(this IFileSystemItemVm fileSystemItemVm, out string directory)
		{
			if (fileSystemItemVm is DirectoryItemVm directoryItemVm)
			{
				directory = directoryItemVm.DirectoryPath;
				return true;
			}

			if (fileSystemItemVm is FileItemVm fileItemVm)
			{
				directory = new FileInfo(fileItemVm.FullPath).DirectoryName;
				return true;
			}

			if (fileSystemItemVm is DescriptedItemVm descriptedItemVm)
			{
				directory = new FileInfo(descriptedItemVm.FileName).DirectoryName;
				return true;
			}

			directory = null;
			return false;
		}
	}
}
