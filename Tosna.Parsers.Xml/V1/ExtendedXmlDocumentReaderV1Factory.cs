using Tosna.Core.Documents;

namespace Tosna.Parsers.Xml.V1
{
	public class ExtendedXmlDocumentReaderV1Factory : IDocumentReaderFactory
	{
		public IDocumentReader CreateReader()
		{
			return new ExtendedXmlDocumentReaderV1();
		}
	}
}