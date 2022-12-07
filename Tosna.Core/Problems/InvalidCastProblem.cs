using System;

namespace Tosna.Core.Problems
{
	public class InvalidCastProblem : IComplexSerializerProblem
	{
		public Type DestinationType { get; }

		public Type ActualType { get; }

		public int LineNumber { get; }

		public InvalidCastProblem(Type destinationType, Type actualType, int lineNumber)
		{
			DestinationType = destinationType;
			ActualType = actualType;
			LineNumber = lineNumber;
		}

		public void Visit(IComplexSerializerProblemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
