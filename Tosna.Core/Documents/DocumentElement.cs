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
		
		public List<DocumentError> Errors { get; } = new List<DocumentError>();

		public DocumentElement(string name)
		{
			Name = name;
		}
	}
}