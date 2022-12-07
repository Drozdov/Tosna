using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tosna.Core.Helpers;
using Tosna.Core.Imprints;
using Tosna.Core.Imprints.Fields;

namespace Tosna.Editor.IDE.FieldsConfigurator.ConfigurableFields
{
	public class NestedImprintConfigurableField : ConfigurableField
	{
		private readonly FieldsConfiguratorManager fieldsConfiguratorManager;
		private NestedImprintField imprintField;

		public NestedImprintConfigurableField(string publicName, FieldsConfiguratorManager fieldsConfiguratorManager, FilesManagerInteractionService filesManagerInteractionService,
			NestedImprintField imprintField) : base(publicName)
		{
			this.fieldsConfiguratorManager = fieldsConfiguratorManager;
			this.imprintField = imprintField;

			AvailableImprints = GetAvailableImprints(filesManagerInteractionService, imprintField.Info.Type);

			var imprint = imprintField.NestedItem;
			if (imprint is ReferenceImprint referenceImprint)
			{
				imprint = AvailableImprints.Single(impr =>
					impr.FilePath == referenceImprint.AbsoluteReferencePath && impr.TryGetId(out var id) &&
					id == referenceImprint.ReferenceId);
			}

			SelectedImprint = imprint;
		}

		public Imprint SelectedImprint { get; private set; }
		
		public void Select(Imprint imprint)
		{
			if (SelectedImprint == imprint)
			{
				return;
			}

			if (!imprint.TryGetId(out var id) || !imprint.TryGetInfo(out var info))
			{
				return;
			}

			string referenceFilePath = null;
			if (info.FilePath != fieldsConfiguratorManager.FileName)
			{
				referenceFilePath = PathUtils.GetRelativePath(new FileInfo(info.FilePath), new FileInfo(fieldsConfiguratorManager.FileName).Directory);
			}
			var referenceImprint = new ReferenceImprint(imprint.Type, null, null, null, id, referenceFilePath);
			fieldsConfiguratorManager.Edit(imprintField, imprintField = new NestedImprintField(imprintField.Info, referenceImprint));
			SelectedImprint = imprint;
		}

		public IReadOnlyCollection<Imprint> AvailableImprints { get; }
	}
}