using Tosna.Core.Documents;

namespace Tosna.Core.Problems
{
	public class ParsingProblem : IComplexSerializerProblem
	{
		public string Description { get; }
		
		public DocumentElementLocation Location { get; }

		public DocumentValidationCode Code { get; }
		
		public bool IsCritical => true;

		public ParsingProblem(string description, DocumentElementLocation location, DocumentValidationCode code)
		{
			Description = description;
			Location = location;
			Code = code;
		}
	}
}