using System;
using Tosna.Editor.IDE.FieldsConfigurator;

namespace Tosna.Editor.IDE.Vm.FileSystem
{
	public class DescribedItemVm : IFileSystemItemVm
	{
		public FieldsConfiguratorManager FieldsConfiguratorManager { get; }

		public DescribedItemVm(FieldsConfiguratorManager fieldsConfiguratorManager)
		{
			FieldsConfiguratorManager = fieldsConfiguratorManager;
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
		
		public string Name => FieldsConfiguratorManager.PublicName;

		public string Tooltip => Name;

		public string FileName => FieldsConfiguratorManager.FileName;

		public void Dispose()
		{
		}
	}
}