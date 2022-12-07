namespace Tosna.Core.Problems
{
	public static class ComplexSerializerProblemsExtensions
	{
		public static string GetProblemMessage(this IComplexSerializerProblem problem)
		{
			return ComplexSerializerProblemMessageGetter.GetMessage(problem);
		}

		public static string GetProblemMessageWithLocation(this IComplexSerializerProblem problem)
		{
			return ComplexSerializerProblemMessageGetter.GetMessageWithLocation(problem);
		}

		public static int GetProblemLineNumber(this IComplexSerializerProblem problem)
		{
			return ComplexSerializerProblemMessageGetter.GetLineNumber(problem);
		}

		public static bool IsProblemCritical(this IComplexSerializerProblem problem)
		{
			return CriticalLevelChecker.IsProblemCriticalImpl(problem);
		}

		#region Nested

		private class ComplexSerializerProblemMessageGetter : IComplexSerializerProblemVisitor
		{
			private string errorMessage;
			private string location;
			private int lineNumber;

			private ComplexSerializerProblemMessageGetter()
			{
			}

			public static string GetMessage(IComplexSerializerProblem problem)
			{
				var visitor = new ComplexSerializerProblemMessageGetter();
				problem.Visit(visitor);
				return visitor.errorMessage;
			}

			public static string GetMessageWithLocation(IComplexSerializerProblem problem)
			{
				var visitor = new ComplexSerializerProblemMessageGetter();
				problem.Visit(visitor);
				return $"{visitor.errorMessage}, {visitor.location}";
			}

			public static int GetLineNumber(IComplexSerializerProblem problem)
			{
				var visitor = new ComplexSerializerProblemMessageGetter();
				problem.Visit(visitor);
				return visitor.lineNumber;
			}

			void IComplexSerializerProblemVisitor.Visit(CommonProblem problem)
			{
				errorMessage = problem.Message;
				location = $"line {problem.Line}";
				lineNumber = problem.Line;
			}

			void IComplexSerializerProblemVisitor.Visit(MissingMembersProblem problem)
			{
				errorMessage = $"Missing field {problem.FieldName} for {problem.Type.FullName}";
				location = $"line {problem.Line}, column {problem.Column}";
				lineNumber = problem.Line;
			}

			void IComplexSerializerProblemVisitor.Visit(InvalidCastProblem problem)
			{
				errorMessage = $"Cannot cast {problem.ActualType.FullName} to {problem.DestinationType.FullName}";
				location = $"line {problem.LineNumber}";
				lineNumber = problem.LineNumber;
			}

			void IComplexSerializerProblemVisitor.Visit(ObsoleteNameProblem problem)
			{
				errorMessage = $"Obsolete name {problem.ObsoleteName}. Consider renamaming to {problem.PreferredName}";
				location = $"line {problem.Line}";
				lineNumber = problem.Line;
			}
		}

		private class CriticalLevelChecker : IComplexSerializerProblemVisitor
		{
			private bool isError;

			private CriticalLevelChecker()
			{
			}

			public static bool IsProblemCriticalImpl(IComplexSerializerProblem problem)
			{
				var warningLevelGetter = new CriticalLevelChecker();
				problem.Visit(warningLevelGetter);
				return warningLevelGetter.isError;
			}

			void IComplexSerializerProblemVisitor.Visit(CommonProblem problem)
			{
				isError = true;
			}

			void IComplexSerializerProblemVisitor.Visit(MissingMembersProblem problem)
			{
				isError = true;
			}

			void IComplexSerializerProblemVisitor.Visit(InvalidCastProblem problem)
			{
				isError = true;
			}

			void IComplexSerializerProblemVisitor.Visit(ObsoleteNameProblem problem)
			{
				isError = false;
			}
		}

		#endregion
	}

}
