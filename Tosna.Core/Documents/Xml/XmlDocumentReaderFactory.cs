namespace Tosna.Core.Documents.Xml
{
	public class XmlDocumentReaderFactory : IDocumentReaderFactory
	{
		public IDocumentReader CreateReader()
		{
			return new XmlDocumentReader { IgnoreContent = true };
		}
	}
}