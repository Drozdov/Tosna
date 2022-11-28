using System;
using System.Collections.Generic;

namespace Tosna.Core.SerializationIterfaces
{
	
	public interface ISerializingElementsManager
	{
		IEnumerable<SerializingElement> GetAllElements(Type type);
	}
}