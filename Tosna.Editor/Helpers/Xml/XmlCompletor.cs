using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Editor.Helpers.Xml
{
	public class XmlCompletor
	{
		private readonly ISerializingElementsManager serializingElementsManager;
		private readonly ISerializingTypesResolver serializingTypesResolver;

		public XmlCompletor(ISerializingElementsManager serializingElementsManager, ISerializingTypesResolver serializingTypesResolver)
		{
			this.serializingElementsManager = serializingElementsManager;
			this.serializingTypesResolver = serializingTypesResolver;
		}

		public static XDocument GetEmptyDocument()
		{
			var document = new XDocument();
			var itemsElement = new XElement("Items");
			document.Add(itemsElement);
			itemsElement.Add(new XComment(" Put your items here "));
			return document;
		}

		public void CompleteElement(XElement xElement, Type type)
		{
			if (serializingTypesResolver.IsSimpleType(type) && xElement.Attribute("Value") == null)
			{
				xElement.Add(new XAttribute("Value", serializingTypesResolver.SerializeSimple(GetDefaultSimpleTypeInstance(type))));
				return;
			}

			foreach (var naturalFieldInfo in serializingElementsManager.GetAllElements(type))
			{
				if (XmlUtils.GetByFieldInfo(xElement, naturalFieldInfo) != null)
				{
					continue;
				}

				if (serializingTypesResolver.IsSimpleType(naturalFieldInfo.Type))
				{
					xElement.Add(new XAttribute(naturalFieldInfo.Name,
						serializingTypesResolver.SerializeSimple(GetDefaultSimpleTypeInstance(naturalFieldInfo.Type))));
				}
				else
				{
					var content = new XElement(xElement.Name + "." + naturalFieldInfo.Name);
					var comment = new XComment(naturalFieldInfo.Type.IsArray ? " Put your items here " : " Put your item here ");
					content.Add(comment);
					xElement.Add(content);
				}
			}
		}

		public string GetFilledContent(XElement xElement, Type type, string startIndention)
		{
			CompleteElement(xElement, type);
			var resultLines = xElement.ToString().Split('\n').Select((line, index) => startIndention + line.Trim());
			return string.Join(Environment.NewLine, resultLines);
		}

		public string GetDefaultContent(Type type, string startIndention)
		{
			if (!serializingTypesResolver.TryGetName(type, out var name))
			{
				throw new ArgumentException("Invalid type");
			}

			var xElement = new XElement(name);

			return GetFilledContent(xElement, type, startIndention);
		}
		
		public IEnumerable<Type> GetProperTypes(string pattern)
		{
			var patternLowerInvariant = pattern.ToLowerInvariant();
			return serializingTypesResolver.GetAllTypes().Where(type =>
				serializingTypesResolver.TryGetName(type, out var name) &&
				name.ToLowerInvariant().StartsWith(patternLowerInvariant));
		}

		public IEnumerable<Type> GetProperTypes(string pattern, Type expectedType)
		{
			var patternLowerInvariant = pattern.ToLowerInvariant();
			return serializingTypesResolver.GetAllTypes().Where(type =>
				expectedType.IsAssignableFrom(type) &&
				serializingTypesResolver.TryGetName(type, out var name) &&
				name.ToLowerInvariant().StartsWith(patternLowerInvariant));
		}

		private static object GetDefaultSimpleTypeInstance(Type type)
		{
			if (type == typeof(string))
			{
				return string.Empty;
			}

			if (type.IsValueType)
			{
				return Activator.CreateInstance(type);
			}

			return null;
		}
	}
}
