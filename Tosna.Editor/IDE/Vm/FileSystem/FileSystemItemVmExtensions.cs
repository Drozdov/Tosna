using System.IO;

namespace Tosna.Editor.IDE.Vm.FileSystem
{
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

			if (fileSystemItemVm is DescribedItemVm descriptedItemVm)
			{
				directory = new FileInfo(descriptedItemVm.FileName).DirectoryName;
				return true;
			}

			directory = null;
			return false;
		}
	}
}