using System.Collections.Generic;
using Tosna.Core.Problems;

namespace Tosna.Core.Imprints
{
	public sealed class ImprintInfo
	{
		public string FilePath { get; }

		public string TypeName { get; }

		public int Line { get; }

		public int Column { get; }

		public IReadOnlyCollection<IComplexSerializerProblem> Problems { get; }

		public ImprintInfo(string filePath, string typeName, int line, int column, params IComplexSerializerProblem[] problems)
		{
			FilePath = filePath;
			TypeName = typeName;
			Line = line;
			Column = column;
			Problems = problems;
		}
	}
}