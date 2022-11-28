using System;

namespace Tosna.Core.Common.Imprints
{
	[Serializable]
	public class ImprintIdentifier
	{
		public string Id { get; }

		public string FilePath { get; }

		public ImprintIdentifier(string id, string filePath)
		{
			Id = id;
			FilePath = filePath;
		}

		public override bool Equals(object obj)
		{
			var stampIdentifier = obj as ImprintIdentifier;
			return obj != null && Equals(stampIdentifier);
		}

		protected bool Equals(ImprintIdentifier other)
		{
			return string.Equals(Id, other.Id) && string.Equals(FilePath, other.FilePath);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (FilePath != null ? FilePath.GetHashCode() : 0);
			}
		}

		public override string ToString()
		{
			return $"{Id} / {FilePath}";
		}
	}
}
