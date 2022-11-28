using Tosna.Core.Common.Attributes;

namespace Tosna.Editor.IDE.Reserved
{
	[SerializableAs(SerializationLabel)]
	// ReSharper disable once ConvertToStaticClass
	public sealed class Empty
	{
		private Empty()
		{
		}

		public const string SerializationLabel = "___RESERVED.EMPTY___";
	}
}
