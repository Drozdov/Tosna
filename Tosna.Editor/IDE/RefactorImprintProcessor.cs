using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tosna.Core.Common.Imprints;
using Tosna.Core.Helpers;

namespace Tosna.Editor.IDE
{
	public class RefactorImprintProcessor : IImprintVisitor
	{
		private readonly Imprint refactoredImprint;
		private readonly string newId;
		private readonly string newFilePath;
		private readonly bool moveToTop;

		private Imprint resultImprint;

		private RefactorImprintProcessor(Imprint refactoredImprint, string newId, string newFilePath, bool moveToTop)
		{
			this.refactoredImprint = refactoredImprint;
			this.newId = newId;
			this.newFilePath = newFilePath;
			this.moveToTop = moveToTop;
		}

		public static bool UpdateImprints(IReadOnlyCollection<Imprint> rootImprints, string imprintsFilePath, Imprint refactoredImprint, string newId,
			string newFilePath, bool moveToTop, out IReadOnlyCollection<Imprint> refactoredImprints)
		{
			refactoredImprints = rootImprints.Select(imprint => UpdateImprint(imprint, refactoredImprint, newId, newFilePath, moveToTop))
				.ToArray();

			if (imprintsFilePath == newFilePath && moveToTop)
			{
				refactoredImprints = refactoredImprints.Union(new[] { refactoredImprint }).ToArray();
			}

			return !refactoredImprints.SequenceEqual(rootImprints);
		}

		private static Imprint UpdateImprint(Imprint imprint, Imprint refactoredImprint, string newId, string newFilePath, bool moveToTop)
		{
			if (imprint == refactoredImprint && moveToTop)
			{
				string relativePath = null;
				if (imprint.FilePath != newFilePath)
				{
					relativePath = PathUtils.GetRelativePath(new FileInfo(newFilePath), new FileInfo(imprint.FilePath).Directory);
				}
				return new ReferenceImprint(imprint.Type, null, null, null, newId, relativePath);
			}

			var processor = new RefactorImprintProcessor(refactoredImprint, newId, newFilePath, moveToTop);
			imprint.Visit(processor);
			return processor.resultImprint;
		}

		void IImprintVisitor.Visit(SimpleTypeImprint imprint)
		{
			resultImprint = imprint;
		}

		void IImprintVisitor.Visit(AggregateImprint imprint)
		{
			var newFields = imprint.Fields.Select(field =>
				MoveImprintFieldProcessor.UpdateImprintField(field, refactoredImprint, newId, newFilePath, moveToTop)).ToArray();
			if (!imprint.TryGetId(out var id))
			{
				id = null;
			}
			if (imprint == refactoredImprint)
			{
				id = newId;
			}

			resultImprint = newFields.SequenceEqual(imprint.Fields) && imprint != refactoredImprint
				? imprint
				: new AggregateImprint(imprint.Type, imprint.TryGetPublicName(out var publicName) ? publicName : null,
					id, null, newFields);
		}

		void IImprintVisitor.Visit(ReferenceImprint imprint)
		{
			resultImprint = refactoredImprint.TryGetId(out var id) && id == imprint.ReferenceId &&
							imprint.AbsoluteReferencePath == refactoredImprint.FilePath && (newId != id || newFilePath != imprint.AbsoluteReferencePath)
				? new ReferenceImprint(imprint.Type, null, null, null, newId,
					PathUtils.GetRelativePath(new FileInfo(newFilePath), new FileInfo(imprint.FilePath).Directory))
				: imprint;
		}

		private class MoveImprintFieldProcessor : IImprintFieldVisitor
		{
			private readonly Imprint refactoredImprint;
			private readonly string newId;
			private readonly string newFilePath;
			private readonly bool moveToTop;

			private ImprintField resultImprintField;

			private MoveImprintFieldProcessor(Imprint refactoredImprint, string newId, string newFilePath, bool moveToTop)
			{
				this.refactoredImprint = refactoredImprint;
				this.newId = newId;
				this.newFilePath = newFilePath;
				this.moveToTop = moveToTop;
			}

			public static ImprintField UpdateImprintField(ImprintField imprintField, Imprint refactoredImprint, string newId, string newFilePath, bool moveToTop)
			{
				var processor = new MoveImprintFieldProcessor(refactoredImprint, newId, newFilePath, moveToTop);
				imprintField.Visit(processor);
				return processor.resultImprintField;
			}

			void IImprintFieldVisitor.Visit(SimpleTypeImprintField field)
			{
				resultImprintField = field;
			}

			void IImprintFieldVisitor.Visit(NestedImprintField field)
			{
				var nestedImprint = UpdateImprint(field.NestedItem, refactoredImprint, newId, newFilePath, moveToTop);
				resultImprintField = field.NestedItem == nestedImprint ? field : new NestedImprintField(field.Info, nestedImprint);
			}

			void IImprintFieldVisitor.Visit(ArrayImprintField field)
			{
				var nestedItems = field.Items.Select(item => UpdateImprint(item, refactoredImprint, newId, newFilePath, moveToTop)).ToArray();
				resultImprintField = nestedItems.SequenceEqual(field.Items) ? field : new ArrayImprintField(field.Info, nestedItems);
			}
		}
	}
}
