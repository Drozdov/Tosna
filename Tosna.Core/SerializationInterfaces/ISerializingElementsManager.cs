using System;
using System.Collections.Generic;

namespace Tosna.Core.SerializationInterfaces
{
	/// <summary>
	/// Defines the way serializing elements will be extracted from type.
	/// One may customize to serialize all fields as elements (or only public properties, etc)
	/// </summary>
	public interface ISerializingElementsManager
	{
		/// <summary>
		/// Gets all field elements for given type
		/// </summary>
		/// <param name="type">Type to be represented as set of field elements</param>
		/// <returns>Set of fields</returns>
		IEnumerable<SerializingElement> GetAllElements(Type type);
	}
}