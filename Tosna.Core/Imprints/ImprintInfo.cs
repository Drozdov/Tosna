using System.Collections.Generic;
using Tosna.Core.Documents;
using Tosna.Core.Problems;

namespace Tosna.Core.Imprints
{
	public sealed class ImprintInfo
	{
		public string FilePath { get; }

		public string TypeName { get; }

		public DocumentElementLocation Location { get; }

		public IReadOnlyCollection<IComplexSerializerProblem> Problems { get; }

		public ImprintInfo(string filePath, string typeName, DocumentElementLocation location, params IComplexSerializerProblem[] problems)
		{
			FilePath = filePath;
			TypeName = typeName;
			Location = location;
			Problems = problems;
		}
	}
}