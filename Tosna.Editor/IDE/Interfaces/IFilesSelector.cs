namespace Tosna.Editor.IDE.Interfaces
{
	public interface IFilesSelector
	{
		bool CreateFile(string initialDirectory, out string fileName);

		bool SelectFiles(string initialDirectory, out string[] files);
	}
}
