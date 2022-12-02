using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Tosna.Core.Common.Attributes;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Common
{




	public class SignaturesSerializingTypeResolver : ISerializingTypesResolver
	{
		private readonly IDictionary<string, Type> typesByName = new Dictionary<string, Type>();

		/// <summary>
		/// Регистрирует типы у которых есть атрибут <see cref="SerializableAsAttribute"/> или которые
		/// являются простыми типами-зависимостями (Enum, String, Guid, примитивные типы, т.п.)
		/// Если простой тип помечен атрибутом <see cref="SerializableAsAttribute"/>, то его имя будет
		/// браться из атрибута (позволяет избегать коллизий на enum'ы с одинаковыми названиями).
		/// </summary>
		/// <param name="serializingElementsManager">Менеджер сериализации для поиска зависимых простых типов</param>
		/// <exception cref="Exception">Исключение возникает в случае коллизии (два и более типа сериализуются
		/// по одинаковому названию)</exception>
		public SignaturesSerializingTypeResolver(ISerializingElementsManager serializingElementsManager)
		{
			var simpleTypesDependencies = new HashSet<Type>();

			foreach (var type in AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes())
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

		private static bool IsTypeSerializable(Type type)
		{
			return type.GetCustomAttributes(typeof(SerializableAsAttribute), false).Any();
		}

		private IEnumerable<Type> GetSimpleTypeDependencies(ISerializingElementsManager serializingElementsManager, Type type)
		{
			return serializingElementsManager.GetAllElements(type).Select(element => element.Type).Where(IsSimpleType);
		}

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
	}
}
