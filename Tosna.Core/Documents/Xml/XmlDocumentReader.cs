using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Tosna.Core.Documents.Xml
{
	public class XmlDocumentReader : IDocumentReader
	{
		/// <summary>
		/// If set to true, XElement text content will be ignored. Example: <Some>Ignored content</Some>
		/// </summary>
		public bool IgnoreContent { get; set; }
		
		public Document ReadDocument(string fileName)
		{
			var xDocument = XDocument.Load(fileName, LoadOptions.SetLineInfo);
			return GetDocument(xDocument, fileName);
		}

		public Document ParseDocument(string content, string fileName)
		{
			try
			{

				var xDocument = XDocument.Parse(content, LoadOptions.SetLineInfo);
				return GetDocument(xDocument, fileName);
			}
			catch (XmlException e)
			{
				var documentElementLocation = new DocumentElementLocation(
					lineStart: e.LineNumber,
					columnStart: e.LinePosition - 1,
					lineEnd: e.LineNumber,
					columnEnd: e.LinePosition + 100);
				return new Document(new DocumentElement("")
				{
					Errors =
					{
						new DocumentError(e.Message, DocumentErrorCode.ParsingProblem, documentElementLocation)
					}
				}, new DocumentInfo(fileName, DocumentFormat.Xml));
			}

		}

		public Document GetDocument(XDocument xDocument, string fileName)
		{
			var documentInfo = new DocumentInfo(fileName, DocumentFormat.Xml);
			var rootElement = ReadDocumentElement(xDocument.Root);
			return new Document(rootElement, documentInfo);
		} 

		private DocumentElement ReadDocumentElement(XElement xElement)
		{
			var documentElement = new DocumentElement(xElement.Name.LocalName);

			if (!IgnoreContent)
			{
				documentElement.Content = xElement.Value;
			}
			
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