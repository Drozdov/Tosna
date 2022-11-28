using System.IO;
using System.Xml;
using System.Xml.Linq;
using Tosna.Core.SerializationIterfaces;

namespace Tosna.Core.Helpers.Xml
{
	public static class XmlUtils
	{
		public static bool TryReadXElement(string xml, int startLine, int startPosition, out XElement xElement, out XElementInfo info)
		{
			try
			{
				using (var stringReader = new StringReader(xml))
				{
					for (var i = 0; i < startLine - 1; i++)
					{
						stringReader.ReadLine();
					}

					for (var i = 0; i < startPosition - 1; i++)
					{
						stringReader.Read();
					}

					using (var reader = XmlReader.Create(stringReader))
					{
						var xmlLineInfo = (IXmlLineInfo) reader;

						reader.MoveToContent();

						var lineStart = xmlLineInfo.LineNumber + startLine - 1;
						var columnStart = xmlLineInfo.LinePosition + startPosition - 1;

						xElement = XNode.ReadFrom(reader) as XElement;

						var lineEnd = xmlLineInfo.LineNumber + startLine - 1;
						var columnEnd = xmlLineInfo.LinePosition + (lineStart != lineEnd ? 0 : startPosition - 1);

						info = new XElementInfo(lineStart, columnStart, lineEnd, columnEnd);

						return xElement != null;
					}
				}

			}
			catch
			{
				xElement = null;
				info = null;
				return false;
			}
		}

		public static XObject GetByFieldInfo(XElement xElement, SerializingElement fieldInfo)
		{
			var shortPreferredNameInvariant = fieldInfo.Name.ToLowerInvariant();
			var preferredNameInvariant = xElement.Name.ToString().ToLowerInvariant() + "." + shortPreferredNameInvariant;

			foreach (var attribute in xElement.Attributes())
			{
				var attributeNameInvariant = attribute.Name.ToString().ToLowerInvariant();
				if (attributeNameInvariant == shortPreferredNameInvariant || attributeNameInvariant == preferredNameInvariant)
				{
					return attribute;
				}
			}

			foreach (var element in xElement.Elements())
			{
				var elementNameInvariant = element.Name.ToString().ToLowerInvariant();
				if (elementNameInvariant == shortPreferredNameInvariant || elementNameInvariant == preferredNameInvariant)
				{
					return element;
				}
			}

			return null;
		}
	}

	public class XElementInfo
	{
		public int LineStart { get; }

		public int ColumnStart { get; }

		public int LineEnd { get; }

		public int ColumnEnd { get; }

		public XElementInfo(int lineStart, int columnStart, int lineEnd, int columnEnd)
		{
			LineStart = lineStart;
			ColumnStart = columnStart;
			LineEnd = lineEnd;
			ColumnEnd = columnEnd;
		}
	}
}
