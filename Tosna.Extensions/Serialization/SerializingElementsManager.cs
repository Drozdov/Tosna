using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Extensions.Serialization
{
	public class SerializingElementsManager : ISerializingElementsManager
	{
		private readonly Regex backingFieldRegex = new Regex("<(.*)>k__BackingField");
		
		public IEnumerable<SerializingElement> GetAllElements(Type type)
		{
			var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			
			foreach (var field in fields)
			{
				var fieldName = field.Name;

				var match = backingFieldRegex.Match(fieldName);
				if (match.Success)
				{
					fieldName = match.Groups[1].Value;
				}

				yield return new SerializingElement(field.SetValue, field.GetValue, field.FieldType, fieldName, true,
					GetDefault(field.FieldType));
			}
		}
		
		private static object GetDefault(Type type)
		{
			return type.IsValueType ? Activator.CreateInstance(type) : null;
		}
	}
}