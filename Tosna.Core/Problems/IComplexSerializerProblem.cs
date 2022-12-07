namespace Tosna.Core.Problems
{
	public interface IComplexSerializerProblem
	{
		void Accept(IComplexSerializerProblemVisitor visitor);
	}

	public interface IComplexSerializerProblemVisitor
	{
		void Visit(CommonProblem problem);

		void Visit(MissingMembersProblem problem);
		
		void Visit(InvalidCastProblem problem);

		void Visit(ObsoleteNameProblem problem);
	}
}
