using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tosna.Core.Common.Imprints;
using Tosna.Core.Helpers;

namespace Tosna.Editor.IDE.FieldsConfigurator.ConfigurableFields
{
	public class ArrayImprintConfigurableField : ConfigurableField
	{
		private readonly FieldsConfiguratorManager fieldsConfiguratorManager;
		private ArrayImprintField imprintField;

		public ArrayImprintConfigurableField(string publicName, FieldsConfiguratorManager fieldsConfiguratorManager, FilesManagerInteractionService filesManagerInteractionService, ArrayImprintField imprintField) : base(publicName)
		{
			this.fieldsConfiguratorManager = fieldsConfiguratorManager;
			this.imprintField = imprintField;

			AvailableImprints = GetAvailableImprints(filesManagerInteractionService, imprintField.Info.Type.GetElementType());

			var selectedImprints = new List<Imprint>();

			foreach (var imprint in imprintField.Items)
			{
				var notReferenceImprint = imprint;
				if (imprint is ReferenceImprint referenceImprint)
				{
					notReferenceImprint = AvailableImprints.Single(impr =>
						impr.FilePath == referenceImprint.AbsoluteReferencePath && impr.TryGetId(out var id) &&
						id == referenceImprint.ReferenceId);
				}
				selectedImprints.Add(notReferenceImprint);
			}
			
			SelectedImprints = selectedImprints;
		}

		public IReadOnlyCollection<Imprint> SelectedImprints { get; private set; }

		public void Select(IReadOnlyCollection<Imprint> imprints)
		{
			if (SelectedImprints.SequenceEqual(imprints))
			{
				return;
			}

			var selectedImprints = new List<Imprint>();
			var referenceImprints = new List<Imprint>();
			foreach (var imprint in imprints)
			{
				if (!imprint.TryGetId(out var id) || !imprint.TryGetInfo(out var info))
				{
					continue;
				}

				string referenceFilePath = null;
				if (info.FilePath != fieldsConfiguratorManager.FileName)
				{
					referenceFilePath = PathUtils.GetRelativePath(new FileInfo(info.FilePath), new FileInfo(fieldsConfiguratorManager.FileName).Directory);
				}
				var referenceImprint = new ReferenceImprint(imprint.Type, null, null, null, id, referenceFilePath);
				selectedImprints.Add(imprint);
				referenceImprints.Add(referenceImprint);
				
			}

			fieldsConfiguratorManager.Edit(imprintField, imprintField = new ArrayImprintField(imprintField.Info, referenceImprints));
			SelectedImprints = selectedImprints;
		}

		public IReadOnlyCollection<Imprint> AvailableImprints { get; }
	}
}