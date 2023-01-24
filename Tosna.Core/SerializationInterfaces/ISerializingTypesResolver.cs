using System;
using System.Collections.Generic;

namespace Tosna.Core.SerializationInterfaces
{
	/// <summary>
	/// Defines types that are allowed to be serialized/deserialized. It also defines
	/// a set of simple types that can be serialized to a string (and parsed from a string).
	/// </summary>
	public interface ISerializingTypesResolver
	{
		/// <summary>
		/// Retrieves all types for serialization.
		/// </summary>
		/// <returns>All serializable types, including simple ones</returns>
		IEnumerable<Type> GetAllTypes();

		/// <summary>
		/// Gets the name of a given type.
		/// </summary>
		/// <param name="type">Type to get a name from</param>
		/// <param name="name">Corresponding user-friendly type name (preferred one, if many)</param>
		/// <returns>True if this type is serializable</returns>
		bool TryGetName(Type type, out string name);

		/// <summary>
		/// Gets  the type by given type name.
		/// </summary>
		/// <param name="name">User-friendly name of a type</param>
		/// <param name="type">Corresponding type, if found</param>
		/// <returns>True if proper type can be found</returns>
		bool TryGetType(string name, out Type type);

		/// <summary>
		/// Checks whether type can be saved to string or parsed from string.
		/// </summary>
		/// <param name="type">Type to be checked</param>
		/// <returns>True if type is a simple type</returns>
		bool IsSimpleType(Type type);

		/// <summary>
		/// Converts simple type instance to string
		/// </summary>
		/// <param name="item">Instance of simple type</param>
		/// <returns>String representation of instance</returns>
		string SerializeSimple(object item);

		/// <summary>
		/// Creates object of simple type from a string.
		/// </summary>
		/// <param name="value">String value to be parsed</param>
		/// <param name="type">Simple type</param>
		/// <returns>New instance of simple type passed</returns>
		object DeserializeSimple(string value, Type type);

	}
}