using Tosna.Core.Documents;

namespace Tosna.Core.Problems
{
	public interface IComplexSerializerProblem
	{
		string Description { get; }
		
		DocumentElementLocation Location { get; }

		bool IsCritical { get; }
	}
}
