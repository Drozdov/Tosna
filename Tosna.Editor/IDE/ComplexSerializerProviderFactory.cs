using Tosna.Core.Common.Problems;
using Tosna.Editor.IDE.Verification;

namespace Tosna.Editor.IDE
{
	public class ComplexSerializerProviderFactory : IComplexSerializerProblemVisitor
	{
		private ICompletionDataProvider completionDataProvider;

		private ComplexSerializerProviderFactory()
		{
		}

		public static ICompletionDataProvider GetProvider(IComplexSerializerProblem problem)
		{
			var visitor = new ComplexSerializerProviderFactory();
			problem.Visit(visitor);
			return visitor.completionDataProvider;
		}

		void IComplexSerializerProblemVisitor.Visit(CommonProblem problem)
		{
			completionDataProvider = new NoneCompletionDataProvider();
		}

		void IComplexSerializerProblemVisitor.Visit(MissingMembersProblem problem)
		{
			completionDataProvider = new MissingMembersCompletionDataProvider(problem.Line, problem.Column, problem.Type,
				problem.SerializingElementsManager, problem.TypesResolver);
		}

		void IComplexSerializerProblemVisitor.Visit(InvalidCastProblem problem)
		{
			completionDataProvider = new NoneCompletionDataProvider();
		}

		void IComplexSerializerProblemVisitor.Visit(ObsoleteNameProblem problem)
		{
			completionDataProvider = new NoneCompletionDataProvider();
		}
	}
}
