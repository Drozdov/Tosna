using System;
using System.Collections.Generic;

namespace Tosna.Core.SerializationInterfaces
{
	public interface ISerializingTypesResolver
	{
		IEnumerable<Type> GetAllTypes();

		bool TryGetName(Type type, out string name);

		bool TryGetType(string name, out Type type);

		bool IsSimpleType(Type type);

		string SerializeSimple(object item);

		object DeserializeSimple(string value, Type type);

	}
}