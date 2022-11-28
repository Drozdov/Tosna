using System;

namespace Tosna.Core.Common.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, AllowMultiple = true)]
	public class SerializableAsAttribute : Attribute
	{
		public string Name { get; }
		
		public bool IsObsolete { get; set; }

		public SerializableAsAttribute(string name)
		{
			Name = name;
		}
	}
}