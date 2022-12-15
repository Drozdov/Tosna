using Tosna.Core.Problems;
using Tosna.Editor.IDE.Verification;
using Tosna.Editor.IDE.Verification.CompletionDataProviders;

namespace Tosna.Editor.IDE
{
	public static class ComplexSerializerProviderFactory
	{
		public static ICompletionDataProvider GetProvider(IComplexSerializerProblem problem)
		{
			switch (problem)
			{
				case MissingMembersProblem missingMembersProblem:
					return new MissingMembersCompletionDataProvider(
						missingMembersProblem.Location.LineStart,
						missingMembersProblem.Location.ColumnStart,
						missingMembersProblem.Type,
						missingMembersProblem.SerializingElementsManager,
						missingMembersProblem.TypesResolver);
				
				default:
					return new NoneCompletionDataProvider();
			}
		}

	}
}
