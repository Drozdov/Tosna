namespace Tosna.Core.Documents
{
	public class Document
	{
		public DocumentElement RootElement { get; }
		
		public DocumentInfo Info { get; }

		public bool HasInfo => Info != null;

		public Document(DocumentElement rootElement, DocumentInfo info = null)
		{
			RootElement = rootElement;
			Info = info;
		}
	}
}