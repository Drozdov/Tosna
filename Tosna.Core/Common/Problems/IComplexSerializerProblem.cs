namespace Tosna.Core.Common.Problems
{
	public interface IComplexSerializerProblem
	{
		void Visit(IComplexSerializerProblemVisitor visitor);
	}

	public interface IComplexSerializerProblemVisitor
	{
		void Visit(CommonProblem problem);

		void Visit(MissingMembersProblem problem);
		
		void Visit(InvalidCastProblem problem);

		void Visit(ObsoleteNameProblem problem);
	}
}
