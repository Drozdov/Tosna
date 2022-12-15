using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Tosna.Core.Documents.Xml
{
	public class XmlDocumentReader : IDocumentReader
	{
		public Document ReadDocument(string fileName)
		{
			var xDocument = XDocument.Load(fileName, LoadOptions.SetLineInfo | LoadOptions.SetBaseUri);
			return ReadDocument(xDocument, fileName);
		}

		public Document ReadDocument(XDocument xDocument, string fileName)
		{
			var documentInfo = new DocumentInfo(fileName, DocumentFormat.Xml);
			var rootElement = ReadDocumentElement(xDocument.Root);
			return new Document(rootElement, documentInfo);
		} 

		private static DocumentElement ReadDocumentElement(XElement xElement)
		{
			var documentElement = new DocumentElement(xElement.Name.LocalName)
			{
				Content = xElement.Value
			};
			
			documentElement.Children.AddRange(xElement.Elements().Select(ReadDocumentElement));
			documentElement.Children.AddRange(xElement.Attributes().Select(ReadDocumentElement));

			var currentLine = ((IXmlLineInfo)xElement).LineNumber;
			var currentPosition = ((IXmlLineInfo)xElement).LinePosition;
			var documentElementLocation = new DocumentElementLocation(
				lineStart: currentLine,
				columnStart: currentPosition,
				lineEnd: currentLine,
				columnEnd: currentPosition + 100);

			documentElement.Location = documentElementLocation;

			return documentElement;
		}

		private static DocumentElement ReadDocumentElement(XAttribute xAttribute)
		{
			var documentElement = new DocumentElement(xAttribute.Name.LocalName)
			{
				Content = xAttribute.Value
			};
			
			var xAttributeParent = (IXmlLineInfo)xAttribute.Parent;
			// xAttributeParent will actually never be null here
			var currentLine = xAttributeParent?.LineNumber ?? 0;
			var currentPosition = xAttributeParent?.LinePosition ?? 0;
			var documentElementLocation = new DocumentElementLocation(
					lineStart: currentLine,
					columnStart: currentPosition,
					lineEnd: currentLine,
					columnEnd: currentPosition + 100);
			documentElement.Location = documentElementLocation;

			return documentElement;
		}
	}
}