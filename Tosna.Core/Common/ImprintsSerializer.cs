using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Tosna.Core.Common.Imprints;
using Tosna.Core.Common.Problems;
using Tosna.Core.Helpers.Xml;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core.Common
{
	public class ImprintsSerializer
	{
		private readonly ISerializingElementsManager serializingElementsManager;
		private readonly ISerializingTypesResolver serializingTypesResolver;

		public ImprintsSerializer(ISerializingElementsManager serializingElementsManager, ISerializingTypesResolver serializingTypesResolver)
		{
			this.serializingElementsManager = serializingElementsManager;
			this.serializingTypesResolver = serializingTypesResolver;
		}

		public XDocument SaveRootImprints(IEnumerable<Imprint> imprints)
		{
			var xElement = new XElement("Items");
			foreach (var imprint in imprints)
			{
				xElement.Add(SerializeToXElement(imprint));
			}
			var xDocument = new XDocument(xElement);
			return xDocument;
		}

		public IEnumerable<Imprint> LoadRootImprints(XDocument xDocument, string filePath)
		{
			if (filePath == null)
			{
				filePath = new Uri(xDocument.BaseUri).LocalPath;
			}

			var xElement = xDocument.Element("Items");

			if (xElement == null)
			{
				throw new ArgumentException($"XML file {filePath} contains no 'Items' property on top level");
			}

			var itemsElements = xElement.Elements();

			foreach (var element in itemsElements)
			{
				yield return DeserializeFromXElement(element, filePath);
			}
		}

		#region Serialization

		private XElement SerializeToXElement(Imprint current)
		{
			return XElementFactory.Create(current, serializingTypesResolver);
		}

		private class XElementFactory : IImprintVisitor
		{
			private readonly XElement xElement;
			private readonly ISerializingTypesResolver serializingTypesResolver;

			private XElementFactory(XElement xElement, ISerializingTypesResolver serializingTypesResolver)
			{
				this.xElement = xElement;
				this.serializingTypesResolver = serializingTypesResolver;
			}

			public static XElement Create(Imprint imprint, ISerializingTypesResolver serializingTypesResolver)
			{
				var itemType = imprint.Type;

				if (!serializingTypesResolver.TryGetName(itemType, out var itemTypeName))
				{
					throw new ArgumentException($"Cannot serialize type {itemType}. Make sure type satisfies requirements.");
				}

				var xElement = new XElement(itemTypeName);

				if (imprint.TryGetPublicName(out var publicName))
				{
					xElement.Add(new XAttribute("Global.PublicName", publicName));
				}

				if (imprint.TryGetId(out var id))
				{
					xElement.Add(new XAttribute("Global.Id", id));
				}

				var visitor = new XElementFactory(xElement, serializingTypesResolver);
				imprint.Visit(visitor);

				return xElement;
			}

			void IImprintVisitor.Visit(SimpleTypeImprint imprint)
			{
				xElement.Add(new XAttribute("Value", serializingTypesResolver.SerializeSimple(imprint.Value)));
			}

			void IImprintVisitor.Visit(AggregateImprint imprint)
			{
				AggregateImprintXElementFiller.FillContent(imprint, xElement, serializingTypesResolver,
					impr => Create(impr, serializingTypesResolver), xElement.Name.LocalName);
			}

			void IImprintVisitor.Visit(ReferenceImprint imprint)
			{
				xElement.Add(new XAttribute("Reference.Id", imprint.ReferenceId));
				if (!string.IsNullOrWhiteSpace(imprint.ReferenceRelativePath))
				{
					xElement.Add(new XAttribute("Reference.Path", imprint.ReferenceRelativePath));
				}
			}
		}

		private class AggregateImprintXElementFiller : IImprintFieldVisitor
		{
			private readonly XElement xElement;
			private readonly ISerializingTypesResolver serializingTypesResolver;
			private readonly Func<Imprint, XElement> getXElement;
			private readonly string baseTypeName;

			private AggregateImprintXElementFiller(XElement xElement, ISerializingTypesResolver serializingTypesResolver, Func<Imprint, XElement> getXElement, string baseTypeName)
			{
				this.xElement = xElement;
				this.serializingTypesResolver = serializingTypesResolver;
				this.getXElement = getXElement;
				this.baseTypeName = baseTypeName;
			}

			public static void FillContent(AggregateImprint imprint, XElement xElement, ISerializingTypesResolver serializingTypesResolver, Func<Imprint, XElement> getXElement, string baseName)
			{
				var visitor = new AggregateImprintXElementFiller(xElement, serializingTypesResolver, getXElement, baseName);
				foreach (var field in imprint.Fields)
				{
					field.Visit(visitor);
				}
			}

			void IImprintFieldVisitor.Visit(SimpleTypeImprintField field)
			{
				xElement.Add(new XAttribute(field.Info.Name, serializingTypesResolver.SerializeSimple(field.Value)));
			}

			void IImprintFieldVisitor.Visit(NestedImprintField field)
			{
				xElement.Add(new XElement(baseTypeName + "." + field.Info.Name, getXElement(field.NestedItem)));
			}

			void IImprintFieldVisitor.Visit(ArrayImprintField field)
			{
				var arrayElement = new XElement(baseTypeName + "." + field.Info.Name);
				arrayElement.Add(field.Items.Select(getXElement).Cast<object>().ToArray());
				xElement.Add(arrayElement);
			}
		}

		#endregion

		#region Deserialization

		private Imprint DeserializeFromXElement(XElement xElement, string filePath)
		{
			var currentLine = ((IXmlLineInfo)xElement).LineNumber;
			var currentPosition = ((IXmlLineInfo)xElement).LinePosition;

			var typeName = xElement.Name.LocalName;
			if (!serializingTypesResolver.TryGetType(typeName, out var implementationType))
			{
				var error =
					$"Cannot deserialize type {typeName}. Make sure proper type exists and satisfies requirements";
				var imprintInfo = new ImprintInfo(filePath, typeName, currentLine, currentPosition, new CommonProblem(error, currentLine));
				return new AggregateImprint(typeof(object), null, null, imprintInfo, new SimpleTypeImprintField[] { });
			}

			var refIdAttribute = xElement.Attribute("Reference.Id");
			var idAttribute = xElement.Attribute("Global.Id");
			var publicNameAttribute = xElement.Attribute("Global.PublicName");

			if (refIdAttribute != null)
			{
				var refFileAttribute = xElement.Attribute("Reference.Path");
				var byRefUnresolvedStamp = new ReferenceImprint(implementationType, publicNameAttribute?.Value, idAttribute?.Value,
					new ImprintInfo(filePath, typeName, currentLine, currentPosition),
					refIdAttribute.Value, refFileAttribute?.Value);

				return byRefUnresolvedStamp;
			}

			if (serializingTypesResolver.IsSimpleType(implementationType))
			{
				var valueAttribute = xElement.Attribute("Value");
				if (valueAttribute == null)
				{
					return new SimpleTypeImprint(implementationType, publicNameAttribute?.Value, idAttribute?.Value,
						new ImprintInfo(filePath, typeName, currentLine, currentPosition,
							new CommonProblem($"Cannot deserialize type {implementationType}: no 'Value' attribute found", currentLine)),
						GetDefault(implementationType));
				}
				try
				{
					var simpleTypeUnresolvedStamp = new SimpleTypeImprint(implementationType, publicNameAttribute?.Value,
						idAttribute?.Value,
						new ImprintInfo(filePath, typeName, currentLine, currentPosition),
						serializingTypesResolver.DeserializeSimple(valueAttribute.Value, implementationType));
					return simpleTypeUnresolvedStamp;
				}
				catch (Exception e)
				{
					return new SimpleTypeImprint(implementationType, publicNameAttribute?.Value, idAttribute?.Value,
						new ImprintInfo(filePath, typeName, currentLine, currentPosition,
							new CommonProblem(e.Message, currentLine)),
						GetDefault(implementationType));
				}
			}

			var fields = new List<ImprintField>();
			var problems = new List<IComplexSerializerProblem>();

			var isObsoleteName = serializingTypesResolver.TryGetName(implementationType, out var preferredName) &&
								preferredName != typeName;
			if (isObsoleteName)
			{
				problems.Add(new ObsoleteNameProblem(typeName, preferredName, currentLine, currentPosition));
			}

			foreach (var naturalFieldInfo in serializingElementsManager.GetAllElements(implementationType))
			{
				var childObject = XmlUtils.GetByFieldInfo(xElement, naturalFieldInfo);
				var childAttribute = childObject as XAttribute;
				var childElement = childObject as XElement;

				if (childAttribute == null && childElement == null)
				{
					if (naturalFieldInfo.IsMandatory)
					{
						problems.Add(new MissingMembersProblem(currentLine, currentPosition, naturalFieldInfo.Name, implementationType,
							serializingElementsManager,
							serializingTypesResolver));
					}
					else
					{
						fields.Add(new SimpleTypeImprintField(naturalFieldInfo, naturalFieldInfo.DefaultValue));
					}
					continue;
				}

				var fieldType = naturalFieldInfo.Type;

				if (childAttribute != null)
				{
					var childAttributeLine = ((IXmlLineInfo)childAttribute).LineNumber;
					if (!serializingTypesResolver.IsSimpleType(fieldType))
					{
						problems.Add(new CommonProblem($"Unknown parse rule for type {fieldType} from string", childAttributeLine));
						continue;
					}
					var valueStr = childAttribute.Value;
					try
					{
						var value = serializingTypesResolver.DeserializeSimple(valueStr, fieldType);
						fields.Add(new SimpleTypeImprintField(naturalFieldInfo, value));
					}
					catch (Exception e)
					{
						problems.Add(new CommonProblem(e.Message, childAttributeLine));
					}
				}
				else if (fieldType.IsArray)
				{
					var items = childElement.Elements().ToArray();
					var array = new Imprint[items.Length];
					var elementType = fieldType.GetElementType();
					for (var i = 0; i < items.Length; i++)
					{
						var stamp = DeserializeFromXElement(items[i], filePath);
						if (elementType != null && stamp.Type != null && !elementType.IsAssignableFrom(stamp.Type))
						{
							problems.Add(new InvalidCastProblem(elementType, stamp.Type, ((IXmlLineInfo)items[i]).LineNumber));
						}

						array[i] = stamp;
					}
					fields.Add(new ArrayImprintField(naturalFieldInfo, array));
				}
				else
				{
					XElement element;
					try
					{
						element = childElement.Elements().Single();
					}
					catch (Exception e)
					{
						problems.Add(new CommonProblem(e.Message, ((IXmlLineInfo)childElement).LineNumber));
						continue;
					}
					var stamp = DeserializeFromXElement(element, filePath);
					if (stamp.Type != null && !fieldType.IsAssignableFrom(stamp.Type))
					{
						problems.Add(new InvalidCastProblem(fieldType, stamp.Type, ((IXmlLineInfo)element).LineNumber));
					}

					fields.Add(new NestedImprintField(naturalFieldInfo, stamp));
				}
			}

			return new AggregateImprint(implementationType, publicNameAttribute?.Value, idAttribute?.Value,
				new ImprintInfo(filePath, typeName, currentLine, currentPosition, problems.ToArray()), fields);
		}

		private static object GetDefault(Type type)
		{
			return type.IsValueType ? Activator.CreateInstance(type) : null;
		}
		
		#endregion
	}
}
