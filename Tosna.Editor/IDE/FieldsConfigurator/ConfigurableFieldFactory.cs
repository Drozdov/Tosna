using Tosna.Core.Imprints;
using Tosna.Core.Imprints.Fields;
using Tosna.Editor.IDE.FieldsConfigurator.ConfigurableFields;

namespace Tosna.Editor.IDE.FieldsConfigurator
{
	public class ConfigurableFieldFactory : IImprintFieldVisitor
	{
		private readonly FieldsConfiguratorManager fieldsConfiguratorManager;
		private readonly FilesManagerInteractionService filesManagerInteractionService;
		private readonly string publicName;

		private ConfigurableField configurableField;

		private ConfigurableFieldFactory(FieldsConfiguratorManager fieldsConfiguratorManager, FilesManagerInteractionService filesManagerInteractionService, string publicName)
		{
			this.fieldsConfiguratorManager = fieldsConfiguratorManager;
			this.filesManagerInteractionService = filesManagerInteractionService;
			this.publicName = publicName;
		}

		public static ConfigurableField GetConfigurableField(NamedImprintField field, FieldsConfiguratorManager fieldsConfiguratorManager, FilesManagerInteractionService filesManagerInteractionService)
		{
			var visitor = new ConfigurableFieldFactory(fieldsConfiguratorManager, filesManagerInteractionService, field.PublicName);
			field.Field.Visit(visitor);
			return visitor.configurableField;
		}

		void IImprintFieldVisitor.Visit(SimpleTypeImprintField field)
		{
			configurableField = new SimpleTypeConfigurableField(publicName, fieldsConfiguratorManager, field);
		}

		void IImprintFieldVisitor.Visit(NestedImprintField field)
		{
			configurableField = new NestedImprintConfigurableField(publicName, fieldsConfiguratorManager, filesManagerInteractionService, field);
		}

		void IImprintFieldVisitor.Visit(ArrayImprintField field)
		{
			configurableField = new ArrayImprintConfigurableField(publicName, fieldsConfiguratorManager, filesManagerInteractionService, field);
		}
	}
}