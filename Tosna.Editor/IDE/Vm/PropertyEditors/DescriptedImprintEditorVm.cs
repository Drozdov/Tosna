using System;
using System.Collections.Generic;
using System.Linq;
using Tosna.Core.Common;
using Tosna.Core.Common.Imprints;
using Tosna.Editor.Common.Vm;

namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class DescriptedImprintEditorVm : IDisposable
	{
		private readonly DescriptorFileManager descriptorFileManager;

		public DescriptedImprintEditorVm(DescriptorFileManager descriptorFileManager, FilesManagerInteractionService filesManagerInteractionService, ILogger logger)
		{
			this.descriptorFileManager = descriptorFileManager;

			descriptorFileManager.Refresh(filesManagerInteractionService, logger);

			Refresh();
		}

		private void Refresh()
		{
			Properties = descriptorFileManager.Fields.Select(GetEditorVm).ToArray();
		}

		public IReadOnlyCollection<IPropertyEditorVm> Properties { get; private set; }

		private static IPropertyEditorVm GetEditorVm(DescriptedField field)
		{
			switch (field)
			{
				case SimpleTypeDescriptedField simpleTypeDescriptedField:
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

				case NestedImprintDescriptedField nestedImprintDescriptedField:
					return new NestedImprintPropertyEditorVm(nestedImprintDescriptedField, nestedImprintDescriptedField.PublicName);

				case ArrayImprintDescriptedField arrayImprintDescriptedField:
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
