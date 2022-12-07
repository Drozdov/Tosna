using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Tosna.Core.Helpers;
using Tosna.Editor.Helpers;

namespace Tosna.Editor.IDE.Vm.FileSystem
{
	public class DirectoryItemVm : IFileSystemItemVm
	{
		#region Properties

		public string DirectoryPath { get; }

		public string DirectoryShortName => Path.GetFileName(DirectoryPath.TrimEnd('/', '\\'));

		public ObservableCollection<IFileSystemItemVm> Items { get; } = new OrderedObservableCollection<IFileSystemItemVm>();

		public ImageType ImageType => ImageType.Folder;

		public string Name => DirectoryShortName;

		public string Tooltip => DirectoryPath;
		
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
}