using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Tosna.Core.Helpers.Xml
{
	public static class XmlFormatter
	{
		public static string FormatText(string input)
		{
			try
			{
				var xDocument = XDocument.Parse(input);
				Encoding encoding;
				try
				{
					encoding = Encoding.GetEncoding(xDocument.Declaration.Encoding);
				}
				catch
				{
					encoding = Encoding.UTF8;
				}
				using (var writer = new FixedEncodingWriter(encoding))
				{
					xDocument.Save(writer, SaveOptions.None);
					return writer.ToString();
				}
			}
			catch
			{
				return input;
			}
		}

		private class FixedEncodingWriter : StringWriter
		{
			public FixedEncodingWriter(Encoding encoding)
			{
				Encoding = encoding;
			}

			public override Encoding Encoding { get; }
		}
	}
}
