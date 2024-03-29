using System;
using System.Collections.Generic;
using System.Linq;
using Tosna.Core.Documents;
using Tosna.Core.Imprints;
using Tosna.Core.Imprints.Fields;
using Tosna.Core.Problems;
using Tosna.Core.SerializationInterfaces;

namespace Tosna.Core
{
	/// <summary>
	/// Main serializer to work with imprints
	/// </summary>
	public class ImprintsSerializer
	{
		private readonly ISerializingElementsManager serializingElementsManager;
		private readonly ISerializingTypesResolver serializingTypesResolver;

		public ImprintsSerializer(ISerializingElementsManager serializingElementsManager, ISerializingTypesResolver serializingTypesResolver)
		{
			this.serializingElementsManager = serializingElementsManager;
			this.serializingTypesResolver = serializingTypesResolver;
		}
		
		/// <summary>
		/// Saves set of imprints to document
		/// </summary>
		/// <param name="imprints">Imprints set to save</param>
		/// <returns>Generated document</returns>
		public Document SaveRootImprints(IEnumerable<Imprint> imprints)
		{
			var rootElement = new DocumentElement("Items");
			rootElement.Children.AddRange(imprints.Select(SerializeToXElement));
			return new Document(rootElement);
		}

		/// <summary>
		/// Loads imprint from a document
		/// </summary>
		/// <param name="document">Given document</param>
		/// <returns>A set of imprints</returns>
		/// <exception cref="ParsingException">If parsing fails</exception>
		/// <exception cref="ArgumentException">If document is invalid</exception>
		public IEnumerable<Imprint> LoadRootImprints(Document document)
		{
			var filePath = document.HasInfo ? document.Info.FilePath : "<Unknown>";

			if (document.RootElement.Errors.Any())
			{
				throw new ParsingException("Parse error: " + document.RootElement.Errors.First().Error,
					document.RootElement.Errors);
			}

			if (document.RootElement.Name != "Items")
			{
				throw new ArgumentException($"Document {filePath} contains no 'Items' top element");
			}

			foreach (var element in document.RootElement.Children)
			{
				yield return DeserializeFromXElement(element, filePath);
			}
		}

		#region Serialization

		private DocumentElement SerializeToXElement(Imprint current)
		{
			return DocumentElementFactory.Create(current, serializingTypesResolver);
		}

		private class DocumentElementFactory : IImprintVisitor
		{
			private readonly DocumentElement documentElement;
			private readonly ISerializingTypesResolver serializingTypesResolver;

			private DocumentElementFactory(DocumentElement documentElement, ISerializingTypesResolver serializingTypesResolver)
			{
				this.documentElement = documentElement;
				this.serializingTypesResolver = serializingTypesResolver;
			}

			public static DocumentElement Create(Imprint imprint, ISerializingTypesResolver serializingTypesResolver)
			{
				var itemType = imprint.Type;

				if (!serializingTypesResolver.TryGetName(itemType, out var itemTypeName))
				{
					throw new ArgumentException($"Cannot serialize type {itemType}. Make sure type satisfies requirements.");
				}

				var documentElement = new DocumentElement(itemTypeName);

				if (imprint.TryGetPublicName(out var publicName))
				{
					documentElement.Children.Add( new DocumentElement("Global.PublicName")
					{
						Content = publicName
					});
				}

				if (imprint.TryGetId(out var id))
				{
					documentElement.Children.Add( new DocumentElement("Global.Id")
					{
						Content = id
					});
				}

				var visitor = new DocumentElementFactory(documentElement, serializingTypesResolver);
				imprint.Accept(visitor);

				return documentElement;
			}

			void IImprintVisitor.Visit(SimpleTypeImprint imprint)
			{
				documentElement.Children.Add( new DocumentElement("Value")
				{
					Content = serializingTypesResolver.SerializeSimple(imprint.Value)
				});
			}

			void IImprintVisitor.Visit(AggregateImprint imprint)
			{
				AggregateImprintXElementFiller.FillContent(imprint, documentElement, serializingTypesResolver,
					innerImprint => Create(innerImprint, serializingTypesResolver), documentElement.Name);
			}

			void IImprintVisitor.Visit(ReferenceImprint imprint)
			{
				documentElement.Children.Add( new DocumentElement("Reference.Id")
				{
					Content = imprint.ReferenceId
				});
				
				if (!string.IsNullOrWhiteSpace(imprint.ReferenceRelativePath))
				{
					documentElement.Children.Add( new DocumentElement("Reference.Path")
					{
						Content = imprint.ReferenceRelativePath
					});
				}
			}
		}

		private class AggregateImprintXElementFiller : IImprintFieldVisitor
		{
			private readonly DocumentElement documentElement;
			private readonly ISerializingTypesResolver serializingTypesResolver;
			private readonly Func<Imprint, DocumentElement> getDocumentElement;
			private readonly string baseTypeName;

			private AggregateImprintXElementFiller(DocumentElement documentElement, ISerializingTypesResolver serializingTypesResolver, Func<Imprint, DocumentElement> getDocumentElement, string baseTypeName)
			{
				this.documentElement = documentElement;
				this.serializingTypesResolver = serializingTypesResolver;
				this.getDocumentElement = getDocumentElement;
				this.baseTypeName = baseTypeName;
			}

			public static void FillContent(AggregateImprint imprint, DocumentElement documentElement, ISerializingTypesResolver serializingTypesResolver, Func<Imprint, DocumentElement> getDocumentElement, string baseName)
			{
				var visitor = new AggregateImprintXElementFiller(documentElement, serializingTypesResolver, getDocumentElement, baseName);
				foreach (var field in imprint.Fields)
				{
					field.Accept(visitor);
				}
			}

			void IImprintFieldVisitor.Visit(SimpleTypeImprintField field)
			{
				documentElement.Children.Add( new DocumentElement(field.Info.Name)
				{
					Content = serializingTypesResolver.SerializeSimple(field.Value)
				});
			}

			void IImprintFieldVisitor.Visit(NestedImprintField field)
			{
				var innerElement = new DocumentElement(baseTypeName + "." + field.Info.Name);
				innerElement.Children.Add(getDocumentElement(field.NestedItem));
				documentElement.Children.Add(innerElement);
			}

			void IImprintFieldVisitor.Visit(ArrayImprintField field)
			{
				var arrayElement = new DocumentElement(baseTypeName + "." + field.Info.Name);
				arrayElement.Children.AddRange(field.Items.Select(getDocumentElement));
				documentElement.Children.Add(arrayElement);
			}
		}

		#endregion

		#region Deserialization

		private Imprint DeserializeFromXElement(DocumentElement documentElement, string filePath, Type expectedType = null)
		{
			var problems = new List<IComplexSerializerProblem>();

			var reader = new DocumentElementReader(documentElement);
			
			#region Checking document errors
			
			foreach (var error in documentElement.Errors)
			{
				var problemLocation =
					error.ProblemLocation.IsUnknown
						? documentElement.Location
						: error.ProblemLocation;
				
				problems.Add(new ParsingProblem(error.Error, problemLocation,
					error.Code, expectedType ?? typeof(object),
					error.ErrorParameters, serializingElementsManager, serializingTypesResolver));
			}
			
			#endregion

			#region Getting implementation type
			
			var typeName = documentElement.Name;
			if (!serializingTypesResolver.TryGetType(typeName, out var implementationType))
			{
				var error =
					$"Cannot deserialize type {typeName}. Make sure proper type exists and satisfies requirements";
				problems.Add(new CommonProblem(error, documentElement.Location));
				var imprintInfo = new ImprintInfo(filePath, typeName, documentElement.Location, problems);
				return new AggregateImprint(typeof(object), null, null, imprintInfo, new SimpleTypeImprintField[] { });
			}
			
			#endregion
			
			#region Local functions
			
			// Validation for unused elements (duplicates or non-existing fields) only
			void CheckParsingValidationOnly(DocumentElement childDocumentElement)
			{
				foreach (var child in childDocumentElement.Children)
				{
					CheckParsingValidationOnly(child);
				}

				foreach (var error in childDocumentElement.Errors)
				{
					problems.Add(new CommonProblem(error.Error,
						error.ProblemLocation.IsUnknown
							? error.ProblemLocation
							: childDocumentElement.Location));
				}
			}
			
			DocumentElement GetDocumentElement(params string[] keys)
			{
				DocumentElement result = null;

				foreach (var element in reader.ExtractDocumentElements(keys))
				{
					if (result == null)
					{
						result = element;
					}
					else
					{
						problems.Add(new CommonProblem($"Field duplication: {result.Name}", result.Location));
						problems.Add(new CommonProblem($"Field duplication: {element.Name}", element.Location));
						CheckParsingValidationOnly(element);
					}
				}

				return result;
			}

			string GetContentString(params string[] keys)
			{
				return GetDocumentElement(keys)?.Content;
			}
			
			DocumentElement GetByFieldInfo(SerializingElement fieldInfo)
			{
				var shortName = fieldInfo.Name;
				var fullName = documentElement.Name + "." + shortName;
				return GetDocumentElement(fullName, shortName);
			}
			
			#endregion

			#region Special attributes
			
			var refIdAttribute = GetContentString("Reference.Id");
			var idAttribute = GetContentString("Global.Id");
			var publicNameAttribute = GetContentString("Global.PublicName");

			if (refIdAttribute != null)
			{
				var refFileAttribute = GetContentString("Reference.Path");
				var byRefUnresolvedStamp = new ReferenceImprint(implementationType, publicNameAttribute, idAttribute,
					new ImprintInfo(filePath, typeName, documentElement.Location, problems),
					refIdAttribute, refFileAttribute);

				return byRefUnresolvedStamp;
			}
			
			#endregion

			#region Check implementation type on being simple
			
			if (serializingTypesResolver.IsSimpleType(implementationType))
			{
				var valueAttribute = GetContentString("Value");
				if (valueAttribute == null)
				{
					problems.Add(new CommonProblem(
						$"Cannot deserialize type {implementationType}: no 'Value' attribute found",
						documentElement.Location));
					return new SimpleTypeImprint(implementationType, publicNameAttribute, idAttribute,
						new ImprintInfo(filePath, typeName, documentElement.Location, problems),
						GetDefault(implementationType));
				}

				try
				{
					var simpleTypeUnresolvedStamp = new SimpleTypeImprint(implementationType, publicNameAttribute,
						idAttribute,
						new ImprintInfo(filePath, typeName, documentElement.Location, problems),
						serializingTypesResolver.DeserializeSimple(valueAttribute, implementationType));
					return simpleTypeUnresolvedStamp;
				}
				catch (Exception e)
				{
					problems.Add(new CommonProblem(e.Message, documentElement.Location));
					return new SimpleTypeImprint(implementationType, publicNameAttribute, idAttribute,
						new ImprintInfo(filePath, typeName, documentElement.Location, problems),
						GetDefault(implementationType));
				}
			}
			
			#endregion

			#region Check on obsolete name
			
			var isObsoleteName = serializingTypesResolver.TryGetName(implementationType, out var preferredName) &&
								preferredName != typeName;
			if (isObsoleteName)
			{
				problems.Add(new ObsoleteNameProblem(typeName, preferredName, documentElement.Location));
			}
			
			#endregion
			
			#region Initializing fields
			
			var fields = new List<ImprintField>();

			foreach (var naturalFieldInfo in serializingElementsManager.GetAllElements(implementationType))
			{
				var childDocumentElement = GetByFieldInfo(naturalFieldInfo);

				if (childDocumentElement == null)
				{
					if (naturalFieldInfo.IsMandatory)
					{
						problems.Add(new MissingMembersProblem(naturalFieldInfo.Name,
							implementationType,
							documentElement.Location,
							serializingElementsManager,
							serializingTypesResolver));
					}
					else
					{
						fields.Add(new SimpleTypeImprintField(naturalFieldInfo, naturalFieldInfo.DefaultValue));
					}
					continue;
				}

				foreach (var childError in childDocumentElement.Errors)
				{
					var problemLocation =
						childError.ProblemLocation.IsUnknown
							? childDocumentElement.Location
							: childError.ProblemLocation;
				
					problems.Add(new ParsingProblem(childError.Error, problemLocation,
						childError.Code, expectedType ?? typeof(object),
						childError.ErrorParameters, serializingElementsManager, serializingTypesResolver));

				}

				var fieldType = naturalFieldInfo.Type;

				if (serializingTypesResolver.IsSimpleType(fieldType) && childDocumentElement.HasContent)
				{
					var valueStr = childDocumentElement.Content;
					try
					{
						var value = serializingTypesResolver.DeserializeSimple(valueStr, fieldType);
						fields.Add(new SimpleTypeImprintField(naturalFieldInfo, value));
					}
					catch (Exception e)
					{
						problems.Add(new CommonProblem(e.Message, childDocumentElement.Location));
					}
				}
				else if (fieldType.IsArray)
				{
					var items = childDocumentElement.Children;
					var array = new Imprint[items.Count];
					var elementType = fieldType.GetElementType();
					for (var i = 0; i < items.Count; i++)
					{
						var stamp = DeserializeFromXElement(items[i], filePath, elementType);
						if (elementType != null && stamp.Type != null && !elementType.IsAssignableFrom(stamp.Type))
						{
							problems.Add(new InvalidCastProblem(elementType, stamp.Type, items[i].Location));
						}

						array[i] = stamp;
					}
					fields.Add(new ArrayImprintField(naturalFieldInfo, array));
				}
				else
				{
					DocumentElement element;
					try
					{
						element = childDocumentElement.Children.Single();
					}
					catch (Exception e)
					{
						problems.Add(new CommonProblem(e.Message, childDocumentElement.Location));
						continue;
					}
					var stamp = DeserializeFromXElement(element, filePath, fieldType);
					if (stamp.Type != null && !fieldType.IsAssignableFrom(stamp.Type))
					{
						problems.Add(new InvalidCastProblem(fieldType, stamp.Type, element.Location));
					}

					fields.Add(new NestedImprintField(naturalFieldInfo, stamp));
				}
			}
			
			#endregion
			
			#region Unused elements

			foreach (var unused in reader.ReadRemaining())
			{
				CheckParsingValidationOnly(unused);
				var warning = $"Unknown field {unused.Name} for {documentElement.Name}, will be ignored";
				problems.Add(new CommonProblem(warning, unused.Location, false));
			}
			
			#endregion

			return new AggregateImprint(implementationType, publicNameAttribute, idAttribute,
				new ImprintInfo(filePath, typeName, documentElement.Location, problems), fields);
		}

		private static object GetDefault(Type type)
		{
			return type.IsValueType ? Activator.CreateInstance(type) : null;
		}
		
		private class DocumentElementReader
		{
			private readonly IDictionary<string, IList<DocumentElement>> elementsByNames =
				new Dictionary<string, IList<DocumentElement>>();

			public DocumentElementReader(DocumentElement element)
			{
				foreach (var child in element.Children)
				{
					if (!elementsByNames.ContainsKey(child.Name))
					{
						// In 99.9% of all cases this will be a single element list
						elementsByNames[child.Name] = new List<DocumentElement>(1);
					}
					elementsByNames[child.Name].Add(child);
				}
			}

			public IEnumerable<DocumentElement> ExtractDocumentElements(IEnumerable<string> keys)
			{
				foreach (var key in keys)
				{
					if (!elementsByNames.TryGetValue(key, out var elements)) continue;

					elementsByNames.Remove(key);
					foreach (var element in elements)
					{
						yield return element;
					}
				}
			}

			public IEnumerable<DocumentElement> ReadRemaining()
			{
				return elementsByNames.Values.SelectMany(list => list);
			}
		}

		#endregion
	}

	public class ParsingException : Exception
	{
		public override string Message { get; }
		
		public IReadOnlyCollection<DocumentError> Errors { get; }

		public ParsingException(string message, IReadOnlyCollection<DocumentError> errors)
		{
			Message = message;
			Errors = errors;
		}
	}
}
