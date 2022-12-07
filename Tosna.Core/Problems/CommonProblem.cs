namespace Tosna.Core.Problems
{
	public class CommonProblem : IComplexSerializerProblem
	{
		public string Message { get; }

		public int Line { get; }

		public CommonProblem(string message, int line)
		{
			Message = message;
			Line = line;
		}

		public void Visit(IComplexSerializerProblemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
