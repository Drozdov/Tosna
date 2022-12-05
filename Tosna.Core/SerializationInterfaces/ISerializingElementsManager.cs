using System;
using System.Collections.Generic;

namespace Tosna.Core.SerializationInterfaces
{
	public interface ISerializingElementsManager
	{
		IEnumerable<SerializingElement> GetAllElements(Type type);
	}
}