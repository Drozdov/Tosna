namespace Tosna.Core.Documents
{
	public interface IDocumentWriter
	{
		void WriteDocument(Document document, string fileName);
	}
}