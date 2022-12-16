using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Tosna.Core.Documents.Xml
{
	public class XmlDocumentWriter : IDocumentWriter
	{
		/// <summary>
		/// If set to true, element content will be ignored if children and content are both not empty
		/// </summary>
		public bool IgnoreContent { get; set; }

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

		public string GetContent(Document document)
		{
			var xDocument = new XDocument(CreateXElement(document.RootElement));
			return xDocument.ToString();
		}

		private XElement CreateXElement(DocumentElement documentElement)
		{
			var result = new XElement(documentElement.Name);
			foreach (var child in documentElement.Children.Select(CreateXObject))
			{
				result.Add(child);
			}

			if (!IgnoreContent)
			{
				result.Value = documentElement.Content;
			}
			
			return result;
		}
		
		private XObject CreateXObject(DocumentElement documentElement)
		{
			if (!documentElement.HasChildren)
			{
				return new XAttribute(documentElement.Name, documentElement.Content);
			}
			return CreateXElement(documentElement);
		}
	}
}