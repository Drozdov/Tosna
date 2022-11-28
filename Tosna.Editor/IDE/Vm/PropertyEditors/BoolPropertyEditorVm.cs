namespace Tosna.Editor.IDE.Vm.PropertyEditors
{
	public class BoolPropertyEditorVm : IPropertyEditorVm
	{
		private readonly SimpleTypeDescriptedField property;
		
		public BoolPropertyEditorVm(SimpleTypeDescriptedField property, string publicName)
		{
			this.property = property;
			PublicName = publicName;
		}

		public bool BoolValue
		{
			get => (bool) property.Value;
			set => property.Value = value;
		}

		public string PublicName { get; }
	}
}
