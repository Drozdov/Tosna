using Tosna.Core.Documents;

namespace Tosna.Parsers.Xml.V2
{
	public class ExtendedXmlDocumentReaderV2Factory : IDocumentReaderFactory
	{
		public IDocumentReader CreateReader()
		{
			return new ExtendedXmlDocumentReaderV2();
		}
	}
}