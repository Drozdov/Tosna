using System;
using System.Collections.Generic;
using System.Linq;
using Tosna.Core;
using Tosna.Core.Imprints;
using Tosna.Editor.Helpers;
using Tosna.Editor.Helpers.Vm;
using Tosna.Editor.IDE.FieldsConfigurator;
using Tosna.Editor.IDE.FieldsConfigurator.ConfigurableFields;

namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class DescriptedImprintEditorVm : IDisposable
	{
		private readonly FieldsConfiguratorManager fieldsConfiguratorManager;

		public DescriptedImprintEditorVm(FieldsConfiguratorManager fieldsConfiguratorManager, FilesManagerInteractionService filesManagerInteractionService, ILogger logger)
		{
			this.fieldsConfiguratorManager = fieldsConfiguratorManager;

			fieldsConfiguratorManager.Refresh(filesManagerInteractionService, logger);

			SaveCommand = new ActionCommand(fieldsConfiguratorManager.SaveChanges, () => true);

			Refresh();
		}

		private void Refresh()
		{
			Properties = fieldsConfiguratorManager.Fields.Select(GetEditorVm).ToArray();
		}

		public IReadOnlyCollection<IPropertyEditorVm> Properties { get; private set; }
		
		public ActionCommand SaveCommand { get; }

		private static IPropertyEditorVm GetEditorVm(ConfigurableField field)
		{
			switch (field)
			{
				case SimpleTypeConfigurableField simpleTypeDescriptedField:
					var type = simpleTypeDescriptedField.Value?.GetType();
					var publicName = simpleTypeDescriptedField.PublicName;

					if (type == null)
					{
						return new ReadonlyPropertyEditorVm(publicName, "NULL");
					}
					if (type == typeof(bool))
					{
						return new BoolPropertyEditorVm(simpleTypeDescriptedField, publicName);
					}
					else if (type == typeof(double))
					{
						return new DoublePropertyEditorVm(simpleTypeDescriptedField, publicName);
					}
					else if (type.IsEnum)
					{
						return new EnumPropertyEditorVm(simpleTypeDescriptedField, publicName);
					}
					else if (type == typeof(int))
					{
						return new IntPropertyEditorVm(simpleTypeDescriptedField, publicName);
					}
					else if (type == typeof(string))
					{
						return new StringPropertyEditorVm(simpleTypeDescriptedField, publicName);
					}
					else
					{
						return new ReadonlyPropertyEditorVm(publicName, simpleTypeDescriptedField.Value.ToString());
					}

				case NestedImprintConfigurableField nestedImprintDescriptedField:
					return new NestedImprintPropertyEditorVm(nestedImprintDescriptedField, nestedImprintDescriptedField.PublicName);

				case ArrayImprintConfigurableField arrayImprintDescriptedField:
					return new ArrayImprintPropertyEditorVm(arrayImprintDescriptedField, arrayImprintDescriptedField.PublicName);

				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void Dispose()
		{
		}
	}

	public class DescriptedImprintFieldItemVm : ICollectionItemVm
	{
		public string PublicName { get; }

		internal Imprint Imprint { get; }

		public DescriptedImprintFieldItemVm(Imprint imprint)
		{
			Imprint = imprint;

			PublicName = imprint.TryGetPublicName(out var publicName)
				? publicName
				: imprint.TryGetId(out var id)
					? id
					: imprint.Type.Name;
		}

	}
}
