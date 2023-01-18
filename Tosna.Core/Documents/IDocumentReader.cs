namespace Tosna.Core.Documents
{
	public interface IDocumentReader
	{
		Document ReadDocument(string fileName);
		
		Document ParseDocument(string content, string fileName);
	}
}