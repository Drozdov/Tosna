using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Tosna.Core.Documents.Xml
{
	public class XmlDocumentWriter : IDocumentWriter
	{
		public void WriteDocument(Document document, string fileName)
		{
			var xDocument = new XDocument(CreateXElement(document.RootElement));
			
			var directoryName = Path.GetDirectoryName(fileName);
			if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}

			xDocument.Save(fileName);
		}

		private static XElement CreateXElement(DocumentElement documentElement)
		{
			var result = new XElement(documentElement.Name);
			foreach (var child in documentElement.Children.Select(CreateXObject))
			{
				result.Add(child);
			}
			return result;
		}
		
		private static XObject CreateXObject(DocumentElement documentElement)
		{
			if (!documentElement.Children.Any())
			{
				return new XAttribute(documentElement.Name, documentElement.Content);
			}
			return CreateXElement(documentElement);
		}
	}
}