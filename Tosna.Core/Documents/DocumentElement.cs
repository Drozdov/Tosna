using System.Collections.Generic;
using System.Linq;

namespace Tosna.Core.Documents
{
	public class DocumentElement
	{
		public string Name { get; }
		
		public string Content { get; set; }

		public bool HasContent => Content != null;

		public DocumentElementLocation Location { get; set; } = DocumentElementLocation.Unknown;

		public List<DocumentElement> Children { get; } = new List<DocumentElement>();

		public bool HasChildren => Children.Any();
		
		public DocumentElementValidationInfo ValidationInfo { get; set; } = DocumentElementValidationInfo.CreateValid();

		public DocumentElement(string name)
		{
			Name = name;
		}
	}

	public static class DocumentElementExtensions
	{
		public static string GetContentString(this DocumentElement documentElement, string name)
		{
			return documentElement.Children.FirstOrDefault(child => child.Name == name)?.Content;
		}
	}
}