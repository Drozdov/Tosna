using System;
using System.Collections.Generic;
using System.Linq;
using Tosna.Core.Imprints;

namespace Tosna.Editor.IDE.FieldsConfigurator
{
	public abstract class ConfigurableField
	{
		protected ConfigurableField(string publicName)
		{
			PublicName = publicName;
		}

		public string PublicName { get; }

		protected static IReadOnlyCollection<Imprint> GetAvailableImprints(
			FilesManagerInteractionService filesManagerInteractionService, Type type)
		{
			return filesManagerInteractionService.FindImprintsByType(type)
				.Where(imprint => !(imprint is ReferenceImprint) && imprint.TryGetId(out _)).ToArray();
		}
	}
}