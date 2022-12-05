using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Extensions.Serialization
{
	/// <summary>
	/// This implementation of <see cref="ISerializingTypesResolver"/> works with types that are either
	/// marked with <see cref="SerializableAsAttribute"/> or represent simple dependency types
	/// (like Enums, Strings, Guids, primitive types, etc)
	/// </summary>
	public class SerializingTypesResolver : ISerializingTypesResolver
	{
		private readonly IDictionary<string, Type> typesByName = new Dictionary<string, Type>();

		/// <summary>
		/// Looks through the assemblies for serializable classes and registers them.
		/// </summary>
		/// <param name="serializingElementsManager">Serializing elements manager</param>
		/// <param name="assemblies">Assemblies to load serializable types from</param>
		/// <exception cref="Exception">Exception will be thrown if collision occurs
		/// (multiple types serialized with the same alias)</exception>
		public SerializingTypesResolver(ISerializingElementsManager serializingElementsManager, IEnumerable<Assembly> assemblies)
		{
			var simpleTypesDependencies = new HashSet<Type>();

			foreach (var type in assemblies.SelectMany(assembly => assembly.GetTypes())
				.Where(IsTypeSerializable))
			{
				RegisterType(type);

				foreach (var simpleTypeDependency in GetSimpleTypeDependencies(serializingElementsManager, type))
				{
					simpleTypesDependencies.Add(simpleTypeDependency);
				}
			}

			foreach (var simpleTypesDependency in simpleTypesDependencies)
			{
				RegisterType(simpleTypesDependency);
			}
		}

		/// <summary>
		/// Looks through the assemblies for serializable classes and registers them.
		/// </summary>
		/// <param name="serializingElementsManager">Serializing elements manager</param>
		/// <exception cref="Exception">Exception will be thrown if collision occurs
		/// (multiple types serialized with the same alias)</exception>
		public SerializingTypesResolver(ISerializingElementsManager serializingElementsManager) : this(
			serializingElementsManager, AppDomain.CurrentDomain.GetAssemblies())
		{
		}

		#region Registering types
		
		private void RegisterType(Type type)
		{
			var isSerializable = false;
			var serializableAsAttributes =
				type.GetCustomAttributes(typeof(SerializableAsAttribute), false).Cast<SerializableAsAttribute>();
			foreach (var serializableAsAttribute in serializableAsAttributes)
			{
				isSerializable = true;
				RegisterTypeAs(type, serializableAsAttribute.Name);
			}

			if (!isSerializable && IsSimpleType(type))
			{
				RegisterTypeAs(type, type.Name);
			}
		}

		private void RegisterTypeAs(Type type, string name)
		{
			if (typesByName.ContainsKey(name))
			{
				if (typesByName[name] == type)
				{
					return;
				}

				throw new Exception(
					$"Multiple types marked as serializable as '{name}': {typesByName[name].FullName} and {type.FullName}");
			}
			typesByName[name] = type;
		}
		
		private static bool IsTypeSerializable(Type type)
		{
			return type.GetCustomAttributes(typeof(SerializableAsAttribute), false).Any();
		}

		private IEnumerable<Type> GetSimpleTypeDependencies(ISerializingElementsManager serializingElementsManager, Type type)
		{
			return serializingElementsManager.GetAllElements(type).Select(element => element.Type).Where(IsSimpleType);
		}
		
		#endregion
		
		#region ISerializingTypesResolver

		public IEnumerable<Type> GetAllTypes()
		{
			return typesByName.Values.Distinct();
		}

		public bool TryGetName(Type type, out string name)
		{
			if (IsSimpleType(type))
			{
				name = type.Name;
				return true;
			}

			var serializableAsAttribute = type.GetCustomAttributes(typeof(SerializableAsAttribute), false)
				.Cast<SerializableAsAttribute>().FirstOrDefault(attribute => !attribute.IsObsolete);
			if (serializableAsAttribute != null)
			{
				name = serializableAsAttribute.Name;
				return true;
			}

			if (type.FullName != null)
			{
				name = type.FullName;
				return true;
			}

			name = null;
			return false;
		}

		public bool TryGetType(string name, out Type type)
		{
			return typesByName.TryGetValue(name, out type);
		}

		public bool IsSimpleType(Type type)
		{
			return type.IsPrimitive || type == typeof(string) || type.IsEnum || type == typeof(Guid);
		}

		public string SerializeSimple(object item)
		{
			return Convert.ToString(item, CultureInfo.InvariantCulture);
		}

		public object DeserializeSimple(string value, Type type)
		{
			if (type == typeof(Guid))
			{
				return new Guid(value);
			}

			if (type.IsEnum)
			{
				return Enum.Parse(type, value);
			}

			return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
		}
		
		#endregion
	}
}
