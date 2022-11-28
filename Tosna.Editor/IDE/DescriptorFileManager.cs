using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tosna.Core.Common;
using Tosna.Core.Common.Attributes;
using Tosna.Core.Common.Imprints;
using Tosna.Core.Helpers;
using Tosna.Core.Helpers.Xml;

namespace Tosna.Editor.IDE
{
	public class DescriptorFileManager
	{
		private readonly ControllerDescriptorContext controllerDescriptorContext;
		private readonly ImprintsSerializer imprintsSerializer;

		private IReadOnlyCollection<Imprint> imprints;

		public DescriptorFileManager(SingleFileManager singleFileManager, ControllerDescriptorContext controllerDescriptorContext, string publicName, ImprintsSerializer imprintsSerializer)
		{
			SingleFileManager = singleFileManager;
			this.controllerDescriptorContext = controllerDescriptorContext;
			PublicName = publicName;
			this.imprintsSerializer = imprintsSerializer;
		}

		public string FileName => SingleFileManager.FileName;

		public SingleFileManager SingleFileManager { get; }

		public string PublicName { get; }

		public void Refresh(FilesManagerInteractionService filesManagerInteractionService, IInfoLogger logger)
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

			var properties = controllerDescriptorContext.GetProperties(imprint);

			Fields = properties.Select(property => DescriptedFieldsFactory.GetDescriptedField(property, this, filesManagerInteractionService)).ToArray();
		}

		public IReadOnlyCollection<DescriptedField> Fields { get; private set; }

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
	}

	public abstract class DescriptedField
	{
		protected DescriptedField(string publicName)
		{
			PublicName = publicName;
		}

		public string PublicName { get; }

		protected static IReadOnlyCollection<Imprint> GetAvailableImprints(FilesManagerInteractionService filesManagerInteractionService, Type type)
		{
			return filesManagerInteractionService.FindImprintsByType(type).Where(imprint => !(imprint is ReferenceImprint) && imprint.TryGetId(out _)).ToArray();
		}
	}

	public class DescriptedFieldsFactory : IImprintFieldVisitor
	{
		private readonly DescriptorFileManager descriptorFileManager;
		private readonly FilesManagerInteractionService filesManagerInteractionService;
		private readonly string publicName;

		private DescriptedField descriptedField;

		public DescriptedFieldsFactory(DescriptorFileManager descriptorFileManager, FilesManagerInteractionService filesManagerInteractionService, string publicName)
		{
			this.descriptorFileManager = descriptorFileManager;
			this.filesManagerInteractionService = filesManagerInteractionService;
			this.publicName = publicName;
		}

		public static DescriptedField GetDescriptedField(NamedImprintField field, DescriptorFileManager descriptorFileManager, FilesManagerInteractionService filesManagerInteractionService)
		{
			var visitor = new DescriptedFieldsFactory(descriptorFileManager, filesManagerInteractionService, field.PublicName);
			field.Field.Visit(visitor);
			return visitor.descriptedField;
		}

		void IImprintFieldVisitor.Visit(SimpleTypeImprintField field)
		{
			descriptedField = new SimpleTypeDescriptedField(publicName, descriptorFileManager, field);
		}

		void IImprintFieldVisitor.Visit(NestedImprintField field)
		{
			descriptedField = new NestedImprintDescriptedField(publicName, descriptorFileManager, filesManagerInteractionService, field);
		}

		void IImprintFieldVisitor.Visit(ArrayImprintField field)
		{
			descriptedField = new ArrayImprintDescriptedField(publicName, descriptorFileManager, filesManagerInteractionService, field);
		}
	}

	public class SimpleTypeDescriptedField : DescriptedField
	{
		private readonly DescriptorFileManager descriptorFileManager;
		private SimpleTypeImprintField imprintField;

		public SimpleTypeDescriptedField(string publicName, DescriptorFileManager descriptorFileManager, SimpleTypeImprintField imprintField) : base(publicName)
		{
			this.descriptorFileManager = descriptorFileManager;
			this.imprintField = imprintField;
		}

		public object Value
		{
			get => imprintField.Value;
			set
			{
				if (Value == value)
				{
					return;
				}
				descriptorFileManager.Edit(imprintField,
					imprintField = new SimpleTypeImprintField(imprintField.Info, value));
			}
		}
	}

	public class NestedImprintDescriptedField : DescriptedField
	{
		private readonly DescriptorFileManager descriptorFileManager;
		private NestedImprintField imprintField;

		public NestedImprintDescriptedField(string publicName, DescriptorFileManager descriptorFileManager, FilesManagerInteractionService filesManagerInteractionService,
			NestedImprintField imprintField) : base(publicName)
		{
			this.descriptorFileManager = descriptorFileManager;
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
			if (info.FilePath != descriptorFileManager.FileName)
			{
				referenceFilePath = PathUtils.GetRelativePath(new FileInfo(info.FilePath), new FileInfo(descriptorFileManager.FileName).Directory);
			}
			var referenceImprint = new ReferenceImprint(imprint.Type, null, null, null, id, referenceFilePath);
			descriptorFileManager.Edit(imprintField, imprintField = new NestedImprintField(imprintField.Info, referenceImprint));
			SelectedImprint = imprint;
		}

		public IReadOnlyCollection<Imprint> AvailableImprints { get; }
	}

	public class ArrayImprintDescriptedField : DescriptedField
	{
		private readonly DescriptorFileManager descriptorFileManager;
		private ArrayImprintField imprintField;

		public ArrayImprintDescriptedField(string publicName, DescriptorFileManager descriptorFileManager, FilesManagerInteractionService filesManagerInteractionService, ArrayImprintField imprintField) : base(publicName)
		{
			this.descriptorFileManager = descriptorFileManager;
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
				if (info.FilePath != descriptorFileManager.FileName)
				{
					referenceFilePath = PathUtils.GetRelativePath(new FileInfo(info.FilePath), new FileInfo(descriptorFileManager.FileName).Directory);
				}
				var referenceImprint = new ReferenceImprint(imprint.Type, null, null, null, id, referenceFilePath);
				selectedImprints.Add(imprint);
				referenceImprints.Add(referenceImprint);
				
			}

			descriptorFileManager.Edit(imprintField, imprintField = new ArrayImprintField(imprintField.Info, referenceImprints));
			SelectedImprints = selectedImprints;
		}

		public IReadOnlyCollection<Imprint> AvailableImprints { get; }
	}
}
