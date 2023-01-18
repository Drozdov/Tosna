namespace Tosna.Core.Documents.Xml
{
	public class XmlDocumentWriterFactory : IDocumentWriterFactory
	{
		public IDocumentWriter CreateWriter()
		{
			return new XmlDocumentWriter { IgnoreContent = true };
		}
	}
}