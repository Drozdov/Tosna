using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tosna.Core.SerializationIterfaces;

namespace Tosna.Core.Common.Stamps
{
	public class StampsCollectionConstructor
	{

		private readonly ISerializingElementsManager serializingElementsManager;
		private readonly ISerializingTypesResolver serializingTypesResolver;

		private readonly IDictionary<object, Stamp> stamps = new Dictionary<object, Stamp>();

		public StampsCollectionConstructor(ISerializingElementsManager serializingElementsManager, ISerializingTypesResolver serializingTypesResolver)
		{
			this.serializingElementsManager = serializingElementsManager;
			this.serializingTypesResolver = serializingTypesResolver;
		}

		public Stamp AddObject(object obj)
		{
			if (obj == null)
			{
				return null;
			}

			if (stamps.TryGetValue(obj, out var stamp))
			{
				return stamp;
			}

			if (serializingTypesResolver.IsSimpleType(obj.GetType()))
			{
				return new Stamp(obj);
			}

			var properties = new List<IStampProperty>();

			// TODO  detect cycles to prevent StackOverflow
			var elements = serializingElementsManager.GetAllElements(obj.GetType());
			foreach (var element in elements)
			{
				var elementValue = element.GetValue(obj);
				if (serializingTypesResolver.IsSimpleType(element.Type))
				{
					properties.Add(new SimpleTypeProperty(element, elementValue));
				}
				else if (element.Type.IsArray)
				{
					var array = (IEnumerable)(elementValue ?? new object[0]);
					var arrayStamps = new List<Stamp>();
					foreach (var item in array)
					{
						arrayStamps.Add(AddObject(item));
					}
					properties.Add(new ArrayStampProperty(element, arrayStamps.ToArray()));
				}
				else
				{
					var elementStamp = AddObject(elementValue);
					properties.Add(new StampProperty(element, elementStamp));
				}

			}

			stamp = new Stamp(obj.GetType(), properties.ToArray());

			stamps[obj] = stamp;

			return stamp;
		}

		public Stamp GetStamp(object obj)
		{
			return stamps[obj];
		}

		public IReadOnlyCollection<Stamp> GetCollection()
		{
			return stamps.Values.ToArray();
		}

	}
}
