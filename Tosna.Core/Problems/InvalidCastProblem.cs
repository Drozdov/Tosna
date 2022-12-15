using System;
using Tosna.Core.Documents;

namespace Tosna.Core.Problems
{
	public class InvalidCastProblem : IComplexSerializerProblem
	{
		public Type DestinationType { get; }

		public Type ActualType { get; }
		
		public DocumentElementLocation Location { get; }

		public string Description => $"Cannot cast {ActualType.FullName} to {DestinationType.FullName}";
		
		public bool IsCritical => true;

		public InvalidCastProblem(Type destinationType, Type actualType, DocumentElementLocation location)
		{
			DestinationType = destinationType;
			ActualType = actualType;
			Location = location;
		}
	}
}
