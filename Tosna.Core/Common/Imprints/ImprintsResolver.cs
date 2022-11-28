using System;
using System.Collections.Generic;
using System.Linq;
using Tosna.Core.Common.Imprints.Generation;
using Tosna.Core.Helpers;

namespace Tosna.Core.Common.Imprints
{
	public class ImprintsResolver : IObjectResolver
	{
		private readonly IDictionary<Imprint, ImprintIdentifier> identifiersByImprints = new Dictionary<Imprint, ImprintIdentifier>();
		private readonly DependenciesResolver<object, ImprintIdentifier> resolver = new DependenciesResolver<object, ImprintIdentifier>();

		public void PutImprint(Imprint imprint)
		{
			var id = GetOrAdd(imprint);
			var dependencies = imprint.GetNestedImprints().Select(GetOrAdd).Union(imprint.GetExternalDependencies()).ToArray();
			resolver.Add(new ImprintResolverInput(imprint, id, dependencies, this));
		}

		public object Get(Imprint imprint)
		{
			return Get(GetOrAdd(imprint));
		}

		public object Get(ImprintIdentifier identifier)
		{
			return resolver.Resolve(identifier);
		}

		public IEnumerable<object> GetAll()
		{
			return resolver.ResolveAll();
		}

		public IEnumerable<ImprintIdentifier> GetAllIds()
		{
			return identifiersByImprints.Values;
		}

		public bool TryGetImprint(ImprintIdentifier id, out Imprint imprint)
		{
			foreach (var keyValue in identifiersByImprints)
			{
				if (Equals(keyValue.Value, id))
				{
					imprint = keyValue.Key;
					return true;
				}
			}

			imprint = default(Imprint);
			return false;
		}

		private ImprintIdentifier GetOrAdd(Imprint imprint)
		{
			if (identifiersByImprints.TryGetValue(imprint, out var result))
			{
				return result;
			}

			if (!imprint.TryGetId(out var id))
			{
				id = Guid.NewGuid().ToString();
			}
			result = new ImprintIdentifier(id, imprint.FilePath);
			identifiersByImprints[imprint] = result;
			return result;
		}

		private class ImprintResolverInput : IResolverInput<object, ImprintIdentifier>
		{
			public ImprintIdentifier Id { get; }
			public ImprintIdentifier[] Dependencies { get; }

			private readonly Imprint imprint;
			private readonly IObjectResolver objectsResolver;

			public ImprintResolverInput(Imprint imprint, ImprintIdentifier id, ImprintIdentifier[] dependencies, IObjectResolver objectsResolver)
			{
				Id = id;
				Dependencies = dependencies;

				this.objectsResolver = objectsResolver;
				this.imprint = imprint;
			}

			public object Resolve(IResolver<object, ImprintIdentifier> resolver)
			{
				return ObjectFactory.Create(imprint, objectsResolver);
			}
		}
	}
}
