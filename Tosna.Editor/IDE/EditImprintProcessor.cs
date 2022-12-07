using System.Collections.Generic;
using System.Linq;
using Tosna.Core.Imprints;
using Tosna.Core.Imprints.Fields;

namespace Tosna.Editor.IDE
{
	public class EditImprintProcessor : IImprintVisitor
	{
		private readonly ImprintField oldImprintField;
		private readonly ImprintField newImprintField;

		private Imprint editedImprint;

		private EditImprintProcessor(ImprintField oldImprintField, ImprintField newImprintField)
		{
			this.oldImprintField = oldImprintField;
			this.newImprintField = newImprintField;
		}

		public static bool EditImprints(IReadOnlyCollection<Imprint> rootImprints, ImprintField oldImprintField, ImprintField newImprintField, out IReadOnlyCollection<Imprint> editedImprints)
		{
			editedImprints = rootImprints.Select(imprint => EditImprint(imprint, oldImprintField, newImprintField)).ToArray();
			return !editedImprints.SequenceEqual(rootImprints);
		}

		private static Imprint EditImprint(Imprint originalImprint, ImprintField oldImprint, ImprintField newImprint)
		{
			var visitor = new EditImprintProcessor(oldImprint, newImprint);
			originalImprint.Visit(visitor);
			return visitor.editedImprint;
		}

		void IImprintVisitor.Visit(SimpleTypeImprint imprint)
		{
			editedImprint = imprint;
		}

		void IImprintVisitor.Visit(AggregateImprint imprint)
		{
			var nestedItems = imprint.Fields
				.Select(field => EditImprintFieldsProcessor.EditImprintField(field, oldImprintField, newImprintField)).ToArray();
			editedImprint = imprint.Fields.SequenceEqual(nestedItems)
				? imprint
				: new AggregateImprint(imprint.Type, imprint.TryGetPublicName(out var publicName) ? publicName : null,
					imprint.TryGetId(out var id) ? id : null, null, nestedItems);
		}

		void IImprintVisitor.Visit(ReferenceImprint imprint)
		{
			editedImprint = imprint;
		}

		private class EditImprintFieldsProcessor : IImprintFieldVisitor
		{
			private readonly ImprintField oldImprintField;
			private readonly ImprintField newImprintField;

			private ImprintField editedImprintField;

			private EditImprintFieldsProcessor(ImprintField oldImprintField, ImprintField newImprintField)
			{
				this.oldImprintField = oldImprintField;
				this.newImprintField = newImprintField;
			}

			public static ImprintField EditImprintField(ImprintField originalImprintField, ImprintField oldImprintField, ImprintField newImprintField)
			{
				if (originalImprintField == oldImprintField)
				{
					return newImprintField;
				}

				var visitor = new EditImprintFieldsProcessor(oldImprintField, newImprintField);
				originalImprintField.Visit(visitor);
				return visitor.editedImprintField;
			}

			public void Visit(SimpleTypeImprintField field)
			{
				editedImprintField = field;
			}

			public void Visit(NestedImprintField field)
			{
				var editedImprint = EditImprint(field.NestedItem, oldImprintField, newImprintField);
				editedImprintField = editedImprint == field.NestedItem ? field : new NestedImprintField(field.Info, editedImprint);
			}

			public void Visit(ArrayImprintField field)
			{
				var editedImprints = field.Items.Select(item => EditImprint(item, oldImprintField, newImprintField)).ToArray();
				editedImprintField = editedImprints.SequenceEqual(field.Items) ? field : new ArrayImprintField(field.Info, editedImprints);
			}
		}
	}
}
