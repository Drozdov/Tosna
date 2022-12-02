using System;

namespace Tosna.Editor.IDE.Vm.FileSystem
{
	public class DescribedItemVm : IFileSystemItemVm
	{
		public DescriptorFileManager DescriptorFileManager { get; }

		public DescribedItemVm(DescriptorFileManager descriptorFileManager)
		{
			DescriptorFileManager = descriptorFileManager;
		}

		public int CompareTo(IFileSystemItemVm other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;
			if (other is DescribedItemVm otherFileItemVm)
			{
				return string.Compare(Name, otherFileItemVm.Name, StringComparison.Ordinal);
			}
			return 1;
		}

		public ImageType ImageType => ImageType.Described;
		
		public string Name => DescriptorFileManager.PublicName;

		public string Tooltip => Name;

		public string FileName => DescriptorFileManager.FileName;

		public void Dispose()
		{
		}
	}
}