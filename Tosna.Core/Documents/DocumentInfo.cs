namespace Tosna.Core.Documents
{
	public class DocumentInfo
	{
		public string FilePath { get; }
		
		public DocumentFormat Format { get; }

		public DocumentInfo(string filePath, DocumentFormat format)
		{
			FilePath = filePath;
			Format = format;
		}
	}
	
	public enum DocumentFormat
	{
		Xml
	}
}