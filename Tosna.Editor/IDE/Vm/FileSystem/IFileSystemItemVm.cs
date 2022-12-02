using System;

namespace Tosna.Editor.IDE.Vm.FileSystem
{
	public interface IFileSystemItemVm : IComparable<IFileSystemItemVm>, IDisposable
	{
		ImageType ImageType { get; }
		
		string Name { get; }
		
		string Tooltip { get; }
	}
}
