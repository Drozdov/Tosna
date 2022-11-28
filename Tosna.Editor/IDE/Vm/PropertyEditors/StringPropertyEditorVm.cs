namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class StringPropertyEditorVm : IPropertyEditorVm
	{
		private readonly SimpleTypeDescriptedField property;

		public StringPropertyEditorVm(SimpleTypeDescriptedField property, string publicName)
		{
			this.property = property;
			PublicName = publicName;
		}

		public string StringValue
		{
			get => (string)property.Value;
			set => property.Value = value;
		}

		public string PublicName { get; }
	}
}
