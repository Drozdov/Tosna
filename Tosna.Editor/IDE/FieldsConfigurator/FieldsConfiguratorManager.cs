using System;
using System.Collections.Generic;
using System.Linq;
using Tosna.Core;
using Tosna.Core.Imprints;
using Tosna.Editor.Helpers.Xml;

namespace Tosna.Editor.IDE.FieldsConfigurator
{
	public class FieldsConfiguratorManager
	{
		private readonly FieldsConfiguratorContext fieldsConfiguratorContext;
		private readonly ImprintsSerializer imprintsSerializer;

		private IReadOnlyCollection<Imprint> imprints;

		public FieldsConfiguratorManager(SingleFileManager singleFileManager, FieldsConfiguratorContext fieldsConfiguratorContext, string publicName, ImprintsSerializer imprintsSerializer)
		{
			SingleFileManager = singleFileManager;
			this.fieldsConfiguratorContext = fieldsConfiguratorContext;
			PublicName = publicName;
			this.imprintsSerializer = imprintsSerializer;
		}

		public string FileName => SingleFileManager.FileName;

		public SingleFileManager SingleFileManager { get; }

		public string PublicName { get; }

		public IReadOnlyCollection<ConfigurableField> Fields { get; private set; }

		public void Refresh(FilesManagerInteractionService filesManagerInteractionService, ILogger logger)
		{
			imprints = SingleFileManager.Imprints;

			AggregateImprint imprint;
			try
			{
				imprint = SingleFileManager.Imprints.OfType<AggregateImprint>().Single(impr => Equals(impr.GetCompactDescription(), PublicName));
			}
			catch (Exception e)
			{
				logger.LogError($"Cannot locate imprint by public name = {PublicName}. {e.Message}", e);
				return;
			}

			var properties = fieldsConfiguratorContext.GetProperties(imprint);

			Fields = properties.Select(property => ConfigurableFieldFactory.GetConfigurableField(property, this, filesManagerInteractionService)).ToArray();
		}

		public void Edit(ImprintField oldImprintField, ImprintField newImprintField)
		{
			if (!EditImprintProcessor.EditImprints(imprints, oldImprintField, newImprintField, out var editedImprints))
			{
				return;
			}

			imprints = editedImprints;
			var editedDocument = imprintsSerializer.SaveRootImprints(imprints);
			var editedText = XmlFormatter.FormatText(editedDocument.ToString());
			SingleFileManager.Edit(editedText);
		}

		public void SaveChanges()
		{
			SingleFileManager.SaveToDisk();
		}
	}
}
