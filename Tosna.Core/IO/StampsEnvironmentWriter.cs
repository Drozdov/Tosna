using System.Collections.Generic;
using System.IO;
using Tosna.Core.Common;
using Tosna.Core.Common.Imprints;
using Tosna.Core.Common.Stamps;

namespace Tosna.Core.IO
{
	public static class StampsEnvironmentWriter
	{
		public static void Save(StampsEnvironment stampsEnvironment, ImprintsSerializer serializer)
		{
			var factory = new StampImprintsFactory(stampsEnvironment);

			var rootImprints = new Dictionary<string, IList<Imprint>>();

			foreach (var rootImprintTuple in factory.GetRootImprints())
			{
				if (!rootImprints.TryGetValue(rootImprintTuple.Item2.FullName, out var list))
				{
					list = new List<Imprint>();
					rootImprints[rootImprintTuple.Item2.FullName] = list;
				}

				list.Add(rootImprintTuple.Item1);
			}

			foreach (var rootImprintPair in rootImprints)
			{
				var imprints = rootImprintPair.Value;
				var file = rootImprintPair.Key;
				var document = serializer.SaveRootImprints(imprints);

				var directoryName = Path.GetDirectoryName(file);
				if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}

				document.Save(file);
			}
		}


		
	}
}
