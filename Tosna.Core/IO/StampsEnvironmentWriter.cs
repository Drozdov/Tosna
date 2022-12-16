using System.Collections.Generic;
using System.IO;
using Tosna.Core.Documents;
using Tosna.Core.Documents.Xml;
using Tosna.Core.Imprints;
using Tosna.Core.Stamps;

namespace Tosna.Core.IO
{
	public static class StampsEnvironmentWriter
	{
		public static void Save(StampsEnvironment stampsEnvironment, IDocumentWriter writer, ImprintsSerializer serializer)
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
				
				writer.WriteDocument(document, file);
			}
		}


		
	}
}
