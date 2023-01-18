using Tosna.Core.Documents;

namespace Tosna.Core.Problems
{
	public class CommonProblem : IComplexSerializerProblem
	{
		public string Description { get; }
		
		public DocumentElementLocation Location { get; }
		
		public bool IsCritical { get; }

		public CommonProblem(string description, DocumentElementLocation location, bool isCritical = true)
		{
			Description = description;
			Location = location;
			IsCritical = isCritical;
		}
	}
}
