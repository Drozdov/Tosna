using Tosna.Core.Documents;

namespace Tosna.Core.Problems
{
	public class ObsoleteNameProblem : IComplexSerializerProblem
	{
		public string ObsoleteName { get; }

		public string PreferredName { get; }
		
		public DocumentElementLocation Location { get; }
		
		public string Description => $"Obsolete name {ObsoleteName}. Consider renaming to {PreferredName}";
		
		public bool IsCritical => false;

		public ObsoleteNameProblem(string obsoleteName, string preferredName, DocumentElementLocation location)
		{
			ObsoleteName = obsoleteName;
			PreferredName = preferredName;
			Location = location;
		}
	}
}
