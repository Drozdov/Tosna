namespace Tosna.Core.Problems
{
	public class ObsoleteNameProblem : IComplexSerializerProblem
	{
		public string ObsoleteName { get; }

		public string PreferredName { get; }

		public int Line { get; }

		public int Position { get; }

		public ObsoleteNameProblem(string obsoleteName, string preferredName, int line, int position)
		{
			ObsoleteName = obsoleteName;
			PreferredName = preferredName;
			Line = line;
			Position = position;
		}

		public void Visit(IComplexSerializerProblemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}
