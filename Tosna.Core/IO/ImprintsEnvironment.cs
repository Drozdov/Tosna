using System.Collections.Generic;
using System.Linq;
using Tosna.Core.Common.Imprints;

namespace Tosna.Core.IO
{
	public class ImprintsEnvironment
	{
		private readonly ImprintsResolver resolver = new ImprintsResolver();

		public ImprintsEnvironment(IEnumerable<Imprint> imprints)
		{
			foreach (var imprint in imprints)
			{
				resolver.PutImprint(imprint);
			}
		}

		public object Get(Imprint imprint)
		{
			return resolver.Get(imprint);
		}

		public object Get(ImprintIdentifier identifier)
		{
			return resolver.Get(identifier);
		}

		public IEnumerable<object> GetAll()
		{
			return resolver.GetAll();
		}

		public IEnumerable<ImprintIdentifier> GetAllIds()
		{
			return resolver.GetAllIds().Distinct();
		}

		public bool TryGetImprintById(ImprintIdentifier id, out Imprint imprint)
		{
			return resolver.TryGetImprint(id, out imprint);
		}
	}
}
