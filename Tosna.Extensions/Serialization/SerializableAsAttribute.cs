using System;

namespace Tosna.Extensions.Serialization
{
	/// <summary>
	/// Marks class, structure or enum as a valid type for <see cref="SerializingTypesResolver"/>.
	/// Should be applied to classes with no behavior only.
	/// </summary>
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