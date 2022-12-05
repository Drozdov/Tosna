using System;
using System.Collections.Generic;
using System.Linq;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Editor.IDE.ProblemsDetailing
{
	internal class PatchedToReservedResolver : ISerializingTypesResolver
	{
		private readonly ISerializingTypesResolver resolver;

		internal PatchedToReservedResolver(ISerializingTypesResolver resolver)
		{
			this.resolver = resolver;
		}

		public IEnumerable<Type> GetAllTypes()
		{
			return resolver.GetAllTypes().Union(new[] { typeof(Empty) });
		}

		public bool TryGetName(Type type, out string name)
		{
			if (type == typeof(Empty))
			{
				name = Empty.SerializationLabel;
				return true;
			}
			
			return resolver.TryGetName(type, out name);
		}

		public bool TryGetType(string name, out Type type)
		{
			if (name == Empty.SerializationLabel)
			{
				type = typeof(Empty);
				return true;
			}

			return resolver.TryGetType(name, out type);
		}

		public bool IsSimpleType(Type type)
		{
			return resolver.IsSimpleType(type);
		}

		public string SerializeSimple(object item)
		{
			return resolver.SerializeSimple(item);
		}

		public object DeserializeSimple(string value, Type type)
		{
			return resolver.DeserializeSimple(value, type);
		}
	}
}